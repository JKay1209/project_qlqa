using QLQA.Enums;

namespace QLQA.DTOs.Order;

public sealed record OrderItemDto(int MenuItemId, string Name, int Quantity, decimal UnitPrice, ItemStatus Status, string? Note);
