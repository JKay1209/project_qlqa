using Shared.Enums;

namespace Shared.DTOs.Order;

/// <summary>Bếp đổi trạng thái món -> server đẩy cho phục vụ.</summary>
public sealed record ItemStatusChangedEvent(int OrderId, int MenuItemId, ItemStatus NewStatus);
