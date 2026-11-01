using Shared.Enums;

namespace Shared.DTOs.Order;

public sealed record OrderItemDto(int MenuItemId, string Name, int Quantity, decimal UnitPrice, ItemStatus Status, string? Note);
