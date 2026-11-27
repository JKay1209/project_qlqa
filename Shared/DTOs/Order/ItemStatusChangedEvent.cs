using Shared.Enums;

namespace Shared.DTOs.Order;

/// <summary>
/// Đổi trạng thái món. Client gửi lên với DetailIds = null.
/// Server broadcast lại với DetailIds = đúng các dòng đã bị đổi.
/// </summary>
public sealed record ItemStatusChangedEvent(
    int OrderId, int MenuItemId, ItemStatus NewStatus,
    List<int>? DetailIds = null);