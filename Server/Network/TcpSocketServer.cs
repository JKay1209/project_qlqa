using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Options;
using Server.Config;

namespace Server.Network;

// TCP Socket Server: Socket -> (Socket Options) -> Bind -> Listen -> Accept.
// Mỗi client được xử lý bằng một Task riêng (async), lỗi của client này không ảnh hưởng client khác.
public class TcpSocketServer(
    ILogger<TcpSocketServer> logger,
    IOptions<SocketServerOptions> options,
    ClientRegistry registry) : BackgroundService
{
    private readonly SocketServerOptions _opt = options.Value;
    private readonly ConcurrentDictionary<int, Task> _handlers = new();   // để chờ dọn dẹp khi tắt Server

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _opt.Validate();

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

        registry.MarkListening(_opt.Port);
        logger.LogInformation("Socket server đang lắng nghe ở cổng {Port} (backlog {Backlog})", _opt.Port, _opt.Backlog);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // 4. accept(): chờ client, không chặn luồng nào
                Socket client;
                try
                {
                    client = await listener.AcceptAsync(stoppingToken);
                }
                catch (SocketException ex)
                {
                    // Lỗi thoáng qua (client reset giữa chừng...): ghi log rồi tiếp tục nhận client khác
                    logger.LogWarning("Lỗi khi accept: {Message}", ex.Message);
                    await Task.Delay(100, stoppingToken);
                    continue;
                }

                // Mỗi client một Task. HandleClientAsync không bao giờ ném lỗi ra ngoài.
                int id = registry.NextId();
                Task handler = Task.Run(() => HandleClientAsync(id, client, stoppingToken), CancellationToken.None);
                _handlers[id] = handler;
                _ = handler.ContinueWith(_ => _handlers.TryRemove(id, out _), TaskScheduler.Default);
            }
        }
        catch (OperationCanceledException)
        {
            // Server đang tắt, thoát vòng lặp bình thường
        }
        finally
        {
            registry.MarkStopped();
            await DrainAsync();
        }
    }

    // Vòng đời một client: cấu hình socket -> đăng ký -> nhận/gửi -> dọn dẹp.
    private async Task HandleClientAsync(int id, Socket socket, CancellationToken serverToken)
    {
        var session = new ClientSession(id, socket, serverToken);

        try
        {
            SocketTuner.ConfigureClient(socket, _opt, logger);
            registry.Add(session);
            logger.LogInformation("Client #{Id} kết nối: {Endpoint} (đang có {Count})",
                session.Id, session.RemoteEndPoint, registry.Count);

            await ReceiveLoopAsync(session);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Client #{Id} bị đóng ({Reason}).",
                session.Id, serverToken.IsCancellationRequested ? "Server đang tắt" : "bị ngắt chủ động");
        }
        catch (Exception ex) when (ex is SocketException or ObjectDisposedException)
        {
            logger.LogWarning("Client #{Id} lỗi socket: {Message}", session.Id, ex.Message);
        }
        catch (Exception ex)
        {
            // Cô lập lỗi: 1 client lỗi không được làm sập Server (Task 27 sẽ bổ sung thêm)
            logger.LogError(ex, "Lỗi không xử lý với client #{Id}", session.Id);
        }
        finally
        {
            registry.Remove(session.Id);
            session.Dispose();
            logger.LogInformation("Client #{Id} đã ngắt kết nối (còn {Count}).", session.Id, registry.Count);
        }
    }

    // Nhận chữ từ client rồi trả lời: "PING" -> "PONG", còn lại -> "ECHO: <nội dung>".
    // TẠM THỜI: sẽ thay bằng packet Header/Body + kiểm tra gói tin (Task 11, 13, 18).
    private async Task ReceiveLoopAsync(ClientSession session)
    {
        var buffer = new byte[_opt.ReceiveBufferBytes];
        var ct = session.Token;

        while (!ct.IsCancellationRequested)
        {
            int received = await session.Socket.ReceiveAsync(buffer.AsMemory(), SocketFlags.None, ct);
            if (received == 0) break;   // client đã đóng kết nối

            session.OnReceived(received);

            string message = Encoding.UTF8.GetString(buffer, 0, received).Trim();
            string reply = message.Equals("PING", StringComparison.OrdinalIgnoreCase)
                ? "PONG"
                : $"ECHO: {message}";

            await session.SendAsync(Encoding.UTF8.GetBytes(reply + "\n"), ct);
        }
    }

    // Khi tắt Server: ngắt mọi client rồi chờ các Task kết thúc (có giới hạn thời gian).
    private async Task DrainAsync()
    {
        registry.DisconnectAll();

        Task all = Task.WhenAll(_handlers.Values);
        Task finished = await Task.WhenAny(all, Task.Delay(TimeSpan.FromSeconds(_opt.ShutdownTimeoutSeconds)));

        if (finished != all)
            logger.LogWarning("Hết {Seconds}s nhưng còn {Count} client chưa đóng xong.",
                _opt.ShutdownTimeoutSeconds, _handlers.Count);
    }
}