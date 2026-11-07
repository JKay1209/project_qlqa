namespace Server.Models.Entities;

// Mỗi lần bấm "Quên mật khẩu" tạo 1 dòng. Giữ OTP và token đặt lại duoi dang hash.
public class PasswordResetRequest
{
    public int Id { get; set; }
    public int UserId { get; set; }

    public string OtpHash { get; set; } = string.Empty;   // Hash của OTP
    public DateTime ExpiresAt { get; set; }               // Hạn của OTP
    public int FailedAttempts { get; set; }               // Số lần nhập OTP sai

    public string? ResetTokenHash { get; set; }           // Có giá trị = OTP đã xác thực
    public DateTime? ResetTokenExpiresAt { get; set; }

    public bool IsUsed { get; set; }                      // Đã dùng hoặc bị vô hiệu
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}