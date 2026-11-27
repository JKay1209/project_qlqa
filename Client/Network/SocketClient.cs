using System.Diagnostics;
using System.Net.Sockets;
using System.Text;

namespace Client.Network;

public enum ConnectionState
{
    Disconnected,
    Connecting,
    Reconnecting,
    Connected
}

// Cấu hình kết nối của Client. Giá trị KeepAlive khớp với appsettings.json phía Server.
internal sealed class SocketClientOptions
{
    public string Host { get; set; } = "127.0.0.1";
    public int Port { get; set; } = 9000;

    public TimeSpan ConnectTimeout { get; set; } = TimeSpan.FromSeconds(5);
    public TimeSpan SendTimeout { get; set; } = TimeSpan.FromSeconds(5);

    // Reconnect: chờ 1s, 2s, 4s, 8s... tối đa 15s (cộng thêm jitter ngẫu nhiên)
    public TimeSpan InitialRetryDelay { get; set; } = TimeSpan.FromSeconds(1);
    public TimeSpan MaxRetryDelay { get; set; } = TimeSpan.FromSeconds(15);

    public int KeepAliveTimeSeconds { get; set; } = 5;
    public int KeepAliveIntervalSeconds { get; set; } = 2;
    public int KeepAliveRetryCount { get; set; } = 3;
}

// TCP client: Socket -> Connect -> nhận/gửi theo dòng ("...\n").
// Mất kết nối (Server tắt, rút mạng, timeout) thì tự kết nối lại với backoff.
//
// LƯU Ý: StateChanged và MessageReceived được gọi từ luồng nền.
// Khi cập nhật giao diện phải dùng Control.BeginInvoke.
internal sealed class SocketClient : IAsyncDisposable
{
    private const int ReceiveBufferBytes = 4096;
    private const int MaxLineChars = 1_000_000;   // 1 dòng quá dài -> coi là lỗi, ngắt kết nối

    private readonly SocketClientOptions _opt;
    private readonly SemaphoreSlim _sendLock = new(1, 1);   // nhiều Task gửi cùng lúc phải xếp hàng

    private CancellationTokenSource? _cts;
    private Task? _runTask;
    private Socket? _socket;
    private int _state = (int)ConnectionState.Disconnected;

    public SocketClient(SocketClientOptions options) => _opt = options;

    public event Action<ConnectionState>? StateChanged;
    public event Action<string>? MessageReceived;

    public ConnectionState State => (ConnectionState)Volatile.Read(ref _state);
    public string? LastError { get; private set; }

    // ===================== Vòng đời =====================
    public void Start()
    {
        if (_runTask is not null) return;

        _cts = new CancellationTokenSource();
        var ct = _cts.Token;
        _runTask = Task.Run(() => RunAsync(ct));
    }

    public async Task StopAsync()
    {
        var cts = _cts;
        var run = _runTask;
        if (cts is null || run is null) return;

        cts.Cancel();
        DropConnection();

        try { await run; }
        catch (OperationCanceledException) { }

        cts.Dispose();
        _cts = null;
        _runTask = null;
        SetState(ConnectionState.Disconnected);
    }

    public async ValueTask DisposeAsync() => await StopAsync();

    // Ép đóng socket hiện tại; vòng lặp nền sẽ tự kết nối lại.
    public void DropConnection()
    {
        try { Volatile.Read(ref _socket)?.Close(); }
        catch { /* socket đã đóng */ }
    }

    // ===================== Gửi =====================
    // Gửi 1 dòng. Trả false nếu đang không kết nối hoặc gửi lỗi (không ném exception mạng).
    public async Task<bool> SendLineAsync(string line, CancellationToken ct = default)
    {
        if (line.Contains('\n') || line.Contains('\r'))
            throw new ArgumentException("Dòng gửi không được chứa ký tự xuống dòng.", nameof(line));

        var socket = Volatile.Read(ref _socket);
        if (socket is null || State != ConnectionState.Connected) return false;

        byte[] data = Encoding.UTF8.GetBytes(line + "\n");

        try
        {
            await _sendLock.WaitAsync(ct);
        }
        catch (OperationCanceledException) { throw; }

        try
        {
            using var sendCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            sendCts.CancelAfter(_opt.SendTimeout);

            int sent = 0;
            while (sent < data.Length)
                sent += await socket.SendAsync(data.AsMemory(sent), SocketFlags.None, sendCts.Token);

            return true;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex) when (ex is SocketException or ObjectDisposedException or OperationCanceledException)
        {
            // Gửi dở dang thì dòng đã hỏng -> bỏ cả kết nối để kết nối lại sạch sẽ
            LastError = ex.Message;
            DropConnection();
            return false;
        }
        finally
        {
            _sendLock.Release();
        }
    }

    // ===================== Vòng lặp kết nối =====================
    private async Task RunAsync(CancellationToken ct)
    {
        int attempt = 0;
        bool first = true;

        while (!ct.IsCancellationRequested)
        {
            SetState(first ? ConnectionState.Connecting : ConnectionState.Reconnecting);
            first = false;

            Socket? socket = null;
            long connectedAt = 0;
            try
            {
                // Server bind IPAddress.Any (IPv4) nên Client cũng dùng IPv4
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                Configure(socket);

                using (var connectCts = CancellationTokenSource.CreateLinkedTokenSource(ct))
                {
                    connectCts.CancelAfter(_opt.ConnectTimeout);
                    await socket.ConnectAsync(_opt.Host, _opt.Port, connectCts.Token);
                }

                Volatile.Write(ref _socket, socket);
                connectedAt = Stopwatch.GetTimestamp();
                LastError = null;
                SetState(ConnectionState.Connected);

                await ReceiveLoopAsync(socket, ct);
                LastError = "Server đã đóng kết nối.";
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex) when (ex is SocketException or ObjectDisposedException
                                          or IOException or InvalidDataException
                                          or OperationCanceledException)
            {
                LastError = ex is OperationCanceledException ? "Hết thời gian kết nối." : ex.Message;
                Debug.WriteLine($"[SocketClient] {LastError}");
            }
            finally
            {
                Volatile.Write(ref _socket, null);
                socket?.Dispose();

                // Chỉ đặt lại backoff khi kết nối đã sống đủ lâu. Bị Server từ chối (đủ chỗ) thì
                // đóng ngay, nên thời gian chờ tăng dần thay vì nối lại liên tục mỗi giây.
                if (connectedAt != 0 && Stopwatch.GetElapsedTime(connectedAt) >= TimeSpan.FromSeconds(10))
                    attempt = 0;
            }

            if (ct.IsCancellationRequested) break;

            SetState(ConnectionState.Reconnecting);
            try { await Task.Delay(NextDelay(attempt++), ct); }
            catch (OperationCanceledException) { break; }
        }

        SetState(ConnectionState.Disconnected);
    }

    // Nhận byte -> ghép thành dòng (TCP là luồng byte, 1 lần đọc có thể nửa dòng hoặc nhiều dòng).
    private async Task ReceiveLoopAsync(Socket socket, CancellationToken ct)
    {
        var buffer = new byte[ReceiveBufferBytes];
        var chars = new char[Encoding.UTF8.GetMaxCharCount(ReceiveBufferBytes)];
        var decoder = Encoding.UTF8.GetDecoder();   // giữ trạng thái khi 1 ký tự UTF-8 bị cắt đôi giữa 2 lần đọc
        var line = new StringBuilder();

        while (true)
        {
            int received = await socket.ReceiveAsync(buffer.AsMemory(), SocketFlags.None, ct);
            if (received == 0) return;   // Server đóng kết nối

            int count = decoder.GetChars(buffer, 0, received, chars, 0);
            for (int i = 0; i < count; i++)
            {
                if (chars[i] == '\n')
                {
                    RaiseMessage(line.ToString().TrimEnd('\r'));
                    line.Clear();
                }
                else
                {
                    line.Append(chars[i]);
                }
            }

            if (line.Length > MaxLineChars)
                throw new InvalidDataException("Server gửi dòng dữ liệu quá dài.");
        }
    }

    // ===================== Helper =====================
    private void Configure(Socket socket)
    {
        socket.NoDelay = true;   // gói order nhỏ gửi đi ngay
        socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

        try
        {
            socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, _opt.KeepAliveTimeSeconds);
            socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, _opt.KeepAliveIntervalSeconds);
            socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount, _opt.KeepAliveRetryCount);
        }
        catch (Exception ex) when (ex is SocketException or PlatformNotSupportedException or ArgumentException)
        {
            // Hệ điều hành cũ: dùng thông số KeepAlive mặc định
        }
    }

    private TimeSpan NextDelay(int attempt)
    {
        double ms = _opt.InitialRetryDelay.TotalMilliseconds * Math.Pow(2, Math.Min(attempt, 10));
        ms = Math.Min(ms, _opt.MaxRetryDelay.TotalMilliseconds);
        return TimeSpan.FromMilliseconds(ms + Random.Shared.Next(0, 500));   // jitter: tránh mọi client nối lại cùng lúc
    }

    private void SetState(ConnectionState state)
    {
        if (Interlocked.Exchange(ref _state, (int)state) == (int)state) return;

        try { StateChanged?.Invoke(state); }
        catch (Exception ex) { Debug.WriteLine($"[SocketClient] StateChanged lỗi: {ex}"); }
    }

    private void RaiseMessage(string line)
    {
        try { MessageReceived?.Invoke(line); }
        catch (Exception ex) { Debug.WriteLine($"[SocketClient] MessageReceived lỗi: {ex}"); }
    }
}