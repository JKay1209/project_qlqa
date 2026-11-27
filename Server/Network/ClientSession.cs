using System.Net;
using System.Net.Sockets;

namespace Server.Network;

// Một client đang kết nối. Mọi tác vụ liên quan tới client đều đi qua lớp này.
public sealed class ClientSession : IDisposable
{
    private readonly CancellationTokenSource _cts;
    private readonly SemaphoreSlim _sendLock = new(1, 1);   // nhiều Task cùng gửi vào 1 socket thì phải xếp hàng
    private long _lastActivityTicks = DateTime.UtcNow.Ticks;
    private long _bytesReceived;
    private long _bytesSent;
    private int _disposed;

    public ClientSession(int id, Socket socket, CancellationToken serverToken)
    {
        Id = id;
        Socket = socket;
        ConnectedAtUtc = DateTime.UtcNow;
        (RemoteAddress, RemoteEndPoint) = ReadRemote(socket);
        _cts = CancellationTokenSource.CreateLinkedTokenSource(serverToken);   // Server tắt -> client tự hủy theo
    }

    public int Id { get; }
    public Socket Socket { get; }
    public IPAddress? RemoteAddress { get; }     
    public string RemoteEndPoint { get; }
    public DateTime ConnectedAtUtc { get; }

    // Token hủy riêng của client: bị hủy khi Server tắt hoặc khi bị ngắt chủ động.
    public CancellationToken Token => _cts.Token;
    public bool IsDisconnectRequested => _cts.IsCancellationRequested;

    public DateTime LastActivityUtc => new(Interlocked.Read(ref _lastActivityTicks), DateTimeKind.Utc);
    public long BytesReceived => Interlocked.Read(ref _bytesReceived);
    public long BytesSent => Interlocked.Read(ref _bytesSent);

    public void Touch() => Interlocked.Exchange(ref _lastActivityTicks, DateTime.UtcNow.Ticks);

    public void OnReceived(int bytes)
    {
        Interlocked.Add(ref _bytesReceived, bytes);
        Touch();
    }

    // Yêu cầu ngắt client
    public void Disconnect()
    {
        try { _cts.Cancel(); }
        catch (ObjectDisposedException) { }
    }

    // Gửi toàn bộ dữ liệu, an toàn khi nhiều Task gọi cùng lúc.
    // Ném ObjectDisposedException nếu session đã đóng: nơi broadcast cần bắt lỗi này.
    public async Task SendAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default)
    {
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct, Token);
        await _sendLock.WaitAsync(linked.Token);
        try
        {
            while (!data.IsEmpty)
            {
                int sent = await Socket.SendAsync(data, SocketFlags.None, linked.Token);
                data = data[sent..];
                Interlocked.Add(ref _bytesSent, sent);
            }
        }
        finally
        {
            _sendLock.Release();
        }
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 1) return;

        Disconnect();
        try { Socket.Shutdown(SocketShutdown.Both); } catch { /* client đã đóng trước */ }
        Socket.Dispose();
        _cts.Dispose();
    }

    private static (IPAddress?, string) ReadRemote(Socket socket)
    {
        try
        {
            var ep = socket.RemoteEndPoint;
            return ((ep as IPEndPoint)?.Address, ep?.ToString() ?? "unknown");
        }
        catch (Exception ex) when (ex is SocketException or ObjectDisposedException)
        {
            return (null, "unknown");  
        }
    }
}