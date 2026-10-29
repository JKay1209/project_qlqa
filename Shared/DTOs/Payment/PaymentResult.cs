namespace QLQA.DTOs.Payment;

public sealed record PaymentResult(bool Success, decimal Change, string? Message);
