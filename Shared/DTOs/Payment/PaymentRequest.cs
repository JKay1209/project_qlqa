using QLQA.Enums;

namespace QLQA.DTOs.Payment;

public sealed record PaymentRequest(int OrderId, PaymentMethod Method, decimal AmountPaid);
