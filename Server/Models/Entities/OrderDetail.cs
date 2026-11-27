using Shared.Enums;

namespace Server.Models.Entities;

public class OrderDetail
{
    public int OrderDetailId { get; set; }
    public int OrderId { get; set; }
    public int MenuItemId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }          // giá tại thời điểm gọi món
    public ItemStatus Status { get; set; } = ItemStatus.Pending;
    public string? Note { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;   // lúc dòng món được gửi bếp
    public Order Order { get; set; } = null!;
    public MenuItem MenuItem { get; set; } = null!;
}
