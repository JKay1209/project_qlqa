using QLQA.Enums;

namespace QLQA.DTOs.Auth;

public sealed record ErrorMessage(ErrorCode Code, string Message);
