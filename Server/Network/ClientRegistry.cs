using System.Collections.Concurrent;

namespace Server.Network;

// Danh sách client đang kết nối + số liệu của Socket Server. Đăng ký Singleton.
// Thread-safe: nhiều Task đọc/ghi cùng lúc.
public sealed class ClientRegistry
{
    private readonly ConcurrentDictionary<int, ClientSession> _clients = new();
    private int _lastId;
    private long _totalAccepted;
    private long _startedAtTicks;      // 0 = chưa chạy
    private int _port;
    private volatile bool _isListening;

    public int Count => _clients.Count;
    public long TotalAccepted => Interlocked.Read(ref _totalAccepted);
    public bool IsListening => _isListening;
    public int Port => Volatile.Read(ref _port);

    public DateTime? StartedAtUtc
    {
        get
        {
            long t = Interlocked.Read(ref _startedAtTicks);
            return t == 0 ? null : new DateTime(t, DateTimeKind.Utc);
        }
    }

    public int NextId() => Interlocked.Increment(ref _lastId);

    public void MarkListening(int port)
    {
        Volatile.Write(ref _port, port);
        Interlocked.Exchange(ref _startedAtTicks, DateTime.UtcNow.Ticks);
        _isListening = true;
    }

    public void MarkStopped() => _isListening = false;

    public void Add(ClientSession session)
    {
        _clients[session.Id] = session;
        Interlocked.Increment(ref _totalAccepted);
    }

    public bool Remove(int id) => _clients.TryRemove(id, out _);

    // Bản chụp tại thời điểm gọi, duyệt an toàn dù client vào/ra.
    public IReadOnlyList<ClientSession> Snapshot() => _clients.Values.OrderBy(c => c.Id).ToList();

    public bool Disconnect(int id)
    {
        if (!_clients.TryGetValue(id, out var session)) return false;
        session.Disconnect();
        return true;
    }

    public void DisconnectAll()
    {
        foreach (var s in _clients.Values) s.Disconnect();
    }
}