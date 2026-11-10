using Shared.Enums;

namespace Server.Models.Entities;

public class Payment
{
    public int PaymentId { get; set; }
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
    public DateTime PaidAt { get; set; } = DateTime.UtcNow;
    public string? Reference { get; set; }          // mã giao dịch (chuyển khoản/thẻ)

    public Order Order { get; set; } = null!;
}
