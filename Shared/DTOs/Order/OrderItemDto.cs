using Shared.Enums;

namespace Shared.DTOs.Order;

public sealed record OrderItemDto(
    int MenuItemId, string Name, int Quantity, decimal UnitPrice,
    ItemStatus Status, string? Note,
    int DetailId,          // OrderDetailId: định danh từng dòng món
    DateTime SentAt);      // UTC: lúc dòng món này được gửi bếp