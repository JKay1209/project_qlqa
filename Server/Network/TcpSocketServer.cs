using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using Microsoft.Extensions.Options;
using Server.Config;
using Server.Network.Routing;
using Shared.DTOs.Auth;
using Shared.DTOs.Table;
using Shared.Protocol;
using Shared.Serialization;

namespace Server.Network;

// TCP Socket Server theo mô hình I/O Multiplexing: Socket -> (Socket Options) -> Bind -> Listen -> select().
//
//  - MỘT luồng gọi Socket.Select() để chờ cùng lúc: listener, mọi client, và socket "đánh thức".
//    Chỉ luồng này đụng vào socket và danh sách client.
//  - Việc chậm (kiểm tra JWT, CSDL) được đẩy sang một worker qua hàng đợi, vòng select không bị chặn.
//  - Worker trả kết quả về hàng đợi _outgoing rồi đánh thức vòng select để gửi đi.
public class TcpSocketServer(
    ILogger<TcpSocketServer> logger,
    IOptions<SocketServerOptions> options,
    ClientRegistry registry,
    MessageRouter router) : BackgroundService
{
    private const int SelectTimeoutMicroseconds = 500_000;   // dự phòng, bình thường được đánh thức ngay
    private const int MaxDeliveriesPerLoop = 256;
    private const int MaxAcceptsPerLoop = 64;               // bị dồn kết nối vẫn còn lượt đọc/gửi cho client cũ

    private readonly SocketServerOptions _opt = options.Value;
    private readonly Dictionary<Socket, ClientSession> _sessions = new();   // chỉ luồng select dùng
    private readonly ConcurrentQueue<Outbound> _outgoing = new();
    private readonly Channel<(ClientSession Session, string Line)> _work =
        Channel.CreateBounded<(ClientSession, string)>(new BoundedChannelOptions(1000) { SingleReader = true });

    private ConnectionLimiter _limiter = null!;             // tạo sau khi cấu hình đã được kiểm tra
    private long _rejected;
    private long _lastRejectLogTicks;
    private Socket? _wakeSocket;
    private EndPoint? _wakeEndPoint;

    // Cho các phần khác của Server (VD thanh toán - Task 22) báo đổi trạng thái bàn. Gọi từ mọi luồng.
    public void PublishTableStatus(TableChangedEvent e) => Post(MessageRouter.TableStatusMessage(e));

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _opt.Validate();
        _limiter = new ConnectionLimiter(_opt.MaxClients, _opt.MaxClientsPerIp);

        // 1. socket()
        using var listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        // 2. Socket Options của listener (SO_REUSEADDR) - bắt buộc trước bind()
        SocketTuner.ConfigureListener(listener, _opt, logger);

        try
        {
            // 3. bind() + listen()
            listener.Bind(new IPEndPoint(IPAddress.Any, _opt.Port));
            listener.Listen(_opt.Backlog);
        }
        catch (SocketException ex)
        {
            logger.LogCritical(ex, "Không mở được cổng {Port}. Cổng đang bị chương trình khác dùng?", _opt.Port);
            throw;
        }
        listener.Blocking = false;

        // Socket UDP loopback tự gửi cho chính nó để đánh thức select() (self-pipe trick)
        using var wake = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        wake.Bind(new IPEndPoint(IPAddress.Loopback, 0));
        wake.Blocking = false;
        _wakeEndPoint = wake.LocalEndPoint;
        _wakeSocket = wake;

        registry.MarkListening(_opt.Port);
        logger.LogInformation("Socket server (select) đang lắng nghe ở cổng {Port} (backlog {Backlog})",
            _opt.Port, _opt.Backlog);

        Task worker = Task.Run(() => WorkerAsync(stoppingToken), CancellationToken.None);
        using var stopRegistration = stoppingToken.Register(Wake);

        try
        {
            // select() là lời gọi chặn nên chạy trên một luồng riêng
            await Task.Factory.StartNew(() => SelectLoop(listener, wake, stoppingToken),
                CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }
        finally
        {
            registry.MarkStopped();
            foreach (var session in _sessions.Values.ToList()) Close(session, "Server đang tắt");

            _work.Writer.TryComplete();
            await Task.WhenAny(worker, Task.Delay(TimeSpan.FromSeconds(_opt.ShutdownTimeoutSeconds)));
            _wakeSocket = null;
        }
    }

    // ============================ Vòng select ============================
    private void SelectLoop(Socket listener, Socket wake, CancellationToken ct)
    {
        var buffer = new byte[_opt.ReceiveBufferBytes];
        var lines = new List<string>();

        while (!ct.IsCancellationRequested)
        {
            DeliverOutgoing();

            foreach (var s in _sessions.Values.Where(s => s.IsDisconnectRequested).ToList())
                Close(s, "bị ngắt chủ động");

            var read = new List<Socket>(_sessions.Count + 2) { listener, wake };
            read.AddRange(_sessions.Keys);
            var write = _sessions.Values.Where(s => s.HasPendingSend).Select(s => s.Socket).ToList();

            try
            {
                // 4. select(): chặn tới khi có ít nhất 1 socket sẵn sàng
                Socket.Select(read, write.Count > 0 ? write : null, null,
                    _outgoing.IsEmpty ? SelectTimeoutMicroseconds : 0);   // còn việc gửi thì không ngủ
            }
            catch (SocketException ex)
            {
                logger.LogWarning("Lỗi select: {Message}", ex.Message);
                Thread.Sleep(100);
                continue;
            }

            foreach (var socket in read)
            {
                if (socket == listener) AcceptPending(listener);
                else if (socket == wake) DrainWake(wake, buffer);
                else if (_sessions.TryGetValue(socket, out var session)) Receive(session, buffer, lines);
            }

            foreach (var socket in write)
                if (_sessions.TryGetValue(socket, out var session))
                    Guard(session, session.Flush);
        }
    }

    // accept() các kết nối đang chờ, chặn kết nối vượt ngưỡng
    private void AcceptPending(Socket listener)
    {
        for (int i = 0; i < MaxAcceptsPerLoop; i++)
        {
            Socket client;
            try
            {
                client = listener.Accept();
            }
            catch (SocketException ex)
            {
                if (ex.SocketErrorCode != SocketError.WouldBlock)
                    logger.LogWarning("Lỗi khi accept: {Message}", ex.Message);
                return;
            }

            try
            {
                client.Blocking = false;
                SocketTuner.ConfigureClient(client, _opt, logger);
            }
            catch (SocketException ex)
            {
                logger.LogWarning("Không cấu hình được socket client: {Message}", ex.Message);
                client.Dispose();
                continue;
            }

            var session = new ClientSession(registry.NextId(), client, Wake);
            if (!_limiter.TryAcquire(IpOf(session), out string? reason))
            {
                Reject(session, reason!);
                continue;
            }
            _sessions[client] = session;
            registry.Add(session);
            logger.LogInformation("Client #{Id} kết nối: {Endpoint} (đang có {Count})",
                session.Id, session.RemoteEndPoint, registry.Count);
        }
    }

    private static IPAddress IpOf(ClientSession session) => session.RemoteAddress ?? IPAddress.None;

    // Báo lý do (không chờ) rồi đóng. Kết nối bị từ chối không vào danh sách client.
    private void Reject(ClientSession session, string reason)
    {
        long total = ++_rejected;
        long now = Environment.TickCount64;
        if (total == 1 || now - _lastRejectLogTicks >= 1000)   // tối đa 1 dòng log/giây khi bị dồn kết nối
        {
            _lastRejectLogTicks = now;
            logger.LogWarning("Từ chối {Endpoint}: {Reason} (tổng đã từ chối {Total})",
                session.RemoteEndPoint, reason, total);
        }

        string line = $"{Protocol.Error}|{JsonSerializer.Serialize(new ErrorResponse(reason), JsonDefaults.Options)}\n";
        session.Socket.Send(Encoding.UTF8.GetBytes(line), SocketFlags.None, out _);
        session.Dispose();
    }

    // Socket đọc được: đọc 1 lần (không để 1 client chiếm vòng lặp), tách dòng, xử lý
    private void Receive(ClientSession session, byte[] buffer, List<string> lines)
        => Guard(session, () =>
        {
            int n = session.Socket.Receive(buffer.AsSpan(), SocketFlags.None, out SocketError error);
            if (error == SocketError.WouldBlock) return;
            if (error != SocketError.Success) throw new SocketException((int)error);
            if (n == 0) { Close(session, "client đóng kết nối"); return; }

            lines.Clear();
            if (!session.Append(buffer.AsSpan(0, n), _opt.MaxLineBytes, lines))
            {
                Close(session, "gói tin quá lớn");
                return;
            }

            foreach (var line in lines)
            {
                if (line == Protocol.Ping)
                    Send(session, Protocol.Pong);        // trả ngay, không cần qua worker
                else if (!_work.Writer.TryWrite((session, line)))
                {
                    Close(session, "Server quá tải");        // Client nối lại và gửi bù
                    return;
                }
            }
        });

    // ============================ Gửi ============================
    private void DeliverOutgoing()
    {
        // Giới hạn mỗi vòng để một đợt broadcast lớn không làm chậm việc đọc / accept
        for (int i = 0; i < MaxDeliveriesPerLoop && _outgoing.TryDequeue(out var message); i++)
        {
            foreach (var session in _sessions.Values.ToList())
            {
                bool match = message.TargetId is int target
                    ? session.Id == target
                    : session.User is { } user && message.Audience!(user.Role) && user.ExpiresAtUtc > DateTime.UtcNow;

                if (match) Send(session, message.Line);
            }
        }
    }

    private void Send(ClientSession session, string line)
        => Guard(session, () =>
        {
            if (!session.Enqueue(Encoding.UTF8.GetBytes(line + "\n"), _opt.MaxPendingSendBytes))
            {
                Close(session, "client nhận quá chậm");
                return;
            }
            session.Flush();   // thường gửi hết ngay; còn dư thì select báo sau
        });

    // Cô lập lỗi: 1 client lỗi không được làm sập vòng select
    private void Guard(ClientSession session, Action action)
    {
        if (!_sessions.ContainsKey(session.Socket)) return;
        try
        {
            action();
        }
        catch (Exception ex) when (ex is SocketException or ObjectDisposedException)
        {
            Close(session, "lỗi socket: " + ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Lỗi không xử lý với client #{Id}", session.Id);
            Close(session, "lỗi xử lý");
        }
    }

    private void Close(ClientSession session, string reason)
    {
        if (!_sessions.Remove(session.Socket)) return;

        registry.Remove(session.Id);
        _limiter.Release(IpOf(session));   // chỉ chạy 1 lần nhờ _sessions.Remove ở trên
        session.Dispose();
        logger.LogInformation("Client #{Id} đã ngắt: {Reason} (còn {Count}).", session.Id, reason, registry.Count);
    }

    // ============================ Worker ============================
    private async Task WorkerAsync(CancellationToken ct)
    {
        try
        {
            await foreach (var (session, line) in _work.Reader.ReadAllAsync(ct))
            {
                try
                {
                    // Vẫn xử lý kể cả khi client vừa rớt: order đã tới Server thì phải được ghi
                    await router.RouteAsync(session, line, Post, ct);
                }
                catch (Exception ex) when (!ct.IsCancellationRequested)
                {
                    logger.LogError(ex, "Lỗi định tuyến từ client #{Id}", session.Id);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Server đang tắt
        }
    }

    private void Post(Outbound message)
    {
        _outgoing.Enqueue(message);
        Wake();
    }

    private void Wake()
    {
        try
        {
            if (_wakeSocket is { } s && _wakeEndPoint is { } ep) s.SendTo([1], ep);
        }
        catch (Exception ex) when (ex is SocketException or ObjectDisposedException)
        {
            // đang tắt, hoặc bộ đệm đầy (khi đó select đã được đánh thức rồi)
        }
    }

    private static void DrainWake(Socket wake, byte[] buffer)
    {
        while (wake.Available > 0)
            wake.Receive(buffer.AsSpan(), SocketFlags.None, out _);
    }
}