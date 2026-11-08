using Shared.Enums;

namespace Server.Models.Entities;

public class Order
{
    public int OrderId { get; set; }
    public int TableId { get; set; }
    public int UserId { get; set; }                 // nhân viên tạo đơn
    public OrderStatus Status { get; set; } = OrderStatus.Open;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PaidAt { get; set; }
    public string? Note { get; set; }

    public Table Table { get; set; } = null!;
    public User User { get; set; } = null!;
    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
