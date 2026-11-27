using Shared.DTOs.Order;
using Shared.DTOs.Table;

namespace Server.Network.Routing;

// Phần nghiệp vụ (JWT + CSDL) mà bộ định tuyến cần. Tách interface để tầng mạng
// không phụ thuộc EF Core.
//
// Quy ước: yêu cầu sai nghiệp vụ -> ném RouteRejectedException (Client nhận NAK, không gửi lại).
//          Exception khác (mất CSDL...) -> Client nhận RETRY và gửi lại sau.
public interface BackendRouting
{
    // Trả null nếu token sai / hết hạn / đã đăng xuất.
    Task<SessionUser?> AuthenticateAsync(string token, CancellationToken ct);

    // Token còn hiệu lực không (hết hạn, đã logout).
    bool IsStillValid(SessionUser user);

    Task<OrderCreated> CreateOrderAsync(SessionUser user, CreateOrderRequest request, CancellationToken ct);

    Task<ItemChanged> ChangeItemStatusAsync(SessionUser user, ItemStatusChangedEvent change, CancellationToken ct);

    Task<TableChangedEvent> ChangeTableStatusAsync(SessionUser user, TableChangedEvent change, CancellationToken ct);

    Task<Snapshot> GetSnapshotAsync(CancellationToken ct);
}

// NewItems: chỉ những món vừa gửi. TableChange: null nếu trạng thái bàn không đổi.
public sealed record OrderCreated(OrderDto NewItems, TableChangedEvent? TableChange);

public sealed record ItemChanged(ItemStatusChangedEvent Event);

public sealed record Snapshot(List<TableDto> Tables, List<OrderDto> Orders);

public sealed class RouteRejectedException(string message) : Exception(message);