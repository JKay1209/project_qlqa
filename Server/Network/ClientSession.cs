using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server.Network;

// Một client đang kết nối. Socket ở chế độ non-blocking.
// Các hàm Receive/Enqueue/Flush CHỈ được gọi từ luồng select của TcpSocketServer, nên không cần khóa.
public sealed class ClientSession : IDisposable
{
    private readonly Action _wake;                          // đánh thức vòng select
    private readonly MemoryStream _line = new();            // phần dòng đang nhận dở
    private readonly LinkedList<ArraySegment<byte>> _outbox = new();
    private int _pendingBytes;
    private long _lastActivityTicks = DateTime.UtcNow.Ticks;
    private long _bytesReceived;
    private long _bytesSent;
    private volatile bool _disconnectRequested;
    private volatile SessionUser? _user;
    private bool _disposed;

    public ClientSession(int id, Socket socket, Action wake)
    {
        Id = id;
        Socket = socket;
        _wake = wake;
        ConnectedAtUtc = DateTime.UtcNow;
        (RemoteAddress, RemoteEndPoint) = ReadRemote(socket);
    }

    public int Id { get; }
    public Socket Socket { get; }
    public IPAddress? RemoteAddress { get; }
    public string RemoteEndPoint { get; }
    public DateTime ConnectedAtUtc { get; }

    // Ghi bởi worker định tuyến, đọc bởi vòng select.
    public SessionUser? User { get => _user; set => _user = value; }

    public bool IsDisconnectRequested => _disconnectRequested;
    public bool HasPendingSend => _outbox.Count > 0;

    public DateTime LastActivityUtc => new(Interlocked.Read(ref _lastActivityTicks), DateTimeKind.Utc);
    public long BytesReceived => Interlocked.Read(ref _bytesReceived);
    public long BytesSent => Interlocked.Read(ref _bytesSent);

    // Yêu cầu ngắt client (gọi được từ mọi luồng). Vòng select sẽ đóng socket.
    public void Disconnect()
    {
        _disconnectRequested = true;
        _wake();
    }

    // Ghép byte nhận được thành các dòng. '\n' không bao giờ nằm giữa 1 ký tự UTF-8 nên cắt theo byte là an toàn.
    // Trả false nếu có dòng dài quá giới hạn.
    public bool Append(ReadOnlySpan<byte> data, int maxLineBytes, List<string> lines)
    {
        Interlocked.Add(ref _bytesReceived, data.Length);
        Interlocked.Exchange(ref _lastActivityTicks, DateTime.UtcNow.Ticks);

        while (!data.IsEmpty)
        {
            int newline = data.IndexOf((byte)'\n');
            var chunk = newline < 0 ? data : data[..newline];

            if (_line.Length + chunk.Length > maxLineBytes) return false;
            _line.Write(chunk);

            if (newline < 0) break;

            string text = Encoding.UTF8.GetString(_line.GetBuffer(), 0, (int)_line.Length).TrimEnd('\r');
            _line.SetLength(0);
            if (text.Length > 0) lines.Add(text);

            data = data[(newline + 1)..];
        }
        return true;
    }

    // Xếp 1 dòng vào hàng chờ gửi. Trả false nếu client nhận quá chậm (hàng chờ đầy).
    public bool Enqueue(byte[] data, int maxPendingBytes)
    {
        if (_pendingBytes > maxPendingBytes) return false;
        _outbox.AddLast(new ArraySegment<byte>(data));
        _pendingBytes += data.Length;
        return true;
    }

    // Gửi được bao nhiêu thì gửi, không bao giờ chặn. Phần còn lại chờ select báo "ghi được".
    public void Flush()
    {
        while (_outbox.First is { } first)
        {
            var segment = first.Value;
            int sent = Socket.Send(segment.AsSpan(), SocketFlags.None, out SocketError error);
            if (error == SocketError.WouldBlock) return;
            if (error != SocketError.Success) throw new SocketException((int)error);

            Interlocked.Add(ref _bytesSent, sent);
            _pendingBytes -= sent;

            if (sent < segment.Count)
            {
                first.Value = segment.Slice(sent);   // phần còn lại vẫn đứng đầu hàng
                return;
            }
            _outbox.RemoveFirst();
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        try { Socket.Shutdown(SocketShutdown.Both); } catch { /* client đã đóng trước */ }
        Socket.Dispose();
        _line.Dispose();
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