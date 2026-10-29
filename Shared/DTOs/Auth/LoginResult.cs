using QLQA.Enums;

namespace QLQA.DTOs.Auth;

public sealed record LoginResult(bool Success, int UserId, string FullName, UserRole Role, string? Message);
