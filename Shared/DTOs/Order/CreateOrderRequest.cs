namespace Shared.DTOs.Order;

public sealed record CreateOrderRequest(int TableId, List<OrderItemDto> Items);
