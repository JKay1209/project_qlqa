using System.Collections.Concurrent;
using Shared.Enums;
using Shared.Protocol;

namespace Server.Network;

// Danh sách client đang kết nối. Nền tảng cho realtime: đẩy order xuống Bếp, broadcast trạng thái bàn (Task 11, 12).
public sealed class ClientRegistry
{
    private readonly ConcurrentDictionary<Guid, ClientSession> _sessions = new();

    public int Count => _sessions.Count;

    public void Add(ClientSession session) => _sessions[session.Id] = session;

    public void Remove(ClientSession session) => _sessions.TryRemove(session.Id, out _);

    // Gửi một gói cho tất cả client thỏa filter (null = tất cả).
    public Task BroadcastAsync(Packet packet, Func<ClientSession, bool>? filter = null, CancellationToken ct = default)
    {
        var targets = _sessions.Values.Where(s => filter?.Invoke(s) ?? true).ToList();
        return Task.WhenAll(targets.Select(s => TrySendAsync(s, packet, ct)));
    }

    // Gửi cho các client đã đăng nhập với một trong các vai trò chỉ định.
    public Task BroadcastToRolesAsync(Packet packet, params UserRole[] roles)
        => BroadcastAsync(packet, s => s.User is { } u && roles.Contains(u.Role));

    // Một client lỗi không được làm hỏng đợt broadcast. Client đó sẽ tự bị dọn bởi vòng đọc của chính nó.
    private static async Task TrySendAsync(ClientSession session, Packet packet, CancellationToken ct)
    {
        try { await session.SendAsync(packet, ct); }
        catch (Exception) { }
    }
}
