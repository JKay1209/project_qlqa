using Shared.Enums;

namespace Shared.DTOs.Order;

public sealed record OrderDto(int Id, int TableId, OrderStatus Status, DateTime CreatedAt, List<OrderItemDto> Items)
{
    public decimal Total => Items.Sum(i => i.UnitPrice * i.Quantity);
}
