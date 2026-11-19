using Shared.Enums;
namespace Shared.DTOs.Auth;

// ====================== Login =======================
public sealed record LoginRequest(string Username, string Password)
{
    // record mặc định in mọi thuộc tính -> che mật khẩu để không lọt vào log
    public override string ToString() => $"LoginRequest {{ Username = {Username}, Password = *** }}";
}

// ====================== Register =======================

// Dữ liệu Client gửi khi tạo tài khoản mới (chỉ Admin được gọi).
public sealed record RegisterRequest(
    string Username,
    string Password,
    string DisplayName,
    UserRole Role,
    string? Email = null)
{
    // che mật khẩu
    public override string ToString() =>
        $"RegisterRequest {{ Username = {Username}, DisplayName = {DisplayName}, Role = {Role}, Password = *** }}";
}

// ====================== Forgot password =======================

// Bước 1: Client gửi email cần lấy lại mật khẩu
public sealed record ForgotPasswordRequest(string Email);

// Bước 2: Client gửi email + mã OTP nhận được
public sealed record VerifyOtpRequest(string Email, string Otp)
{
    public override string ToString() => $"VerifyOtpRequest {{ Email = {Email}, Otp = *** }}";
}

// Bước 2: Server trả token đặt lại mật khẩu (ngắn hạn, dùng 1 lần)
public sealed record VerifyOtpResponse(string ResetToken)
{
    public override string ToString() => "VerifyOtpResponse { ResetToken = *** }";
}

// Bước 3: Client gửi token + mật khẩu mới
public sealed record ResetPasswordRequest(string ResetToken, string NewPassword)
{
    public override string ToString() => "ResetPasswordRequest { ResetToken = ***, NewPassword = *** }";
}

// Phản hồi chung
public sealed record MessageResponse(string Message);
