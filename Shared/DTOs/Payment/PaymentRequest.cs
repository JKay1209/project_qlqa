using Shared.Enums;

namespace Shared.DTOs.Payment;

public sealed record PaymentRequest(int OrderId, PaymentMethod Method, decimal AmountPaid);
