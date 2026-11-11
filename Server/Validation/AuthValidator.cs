using Shared.DTOs.Auth;

namespace Server.Validation;

// Quy tắc kiểm tra dữ liệu xác thực, dùng chung cho tầng TCP (và có thể cho REST Controller).
public static class AuthValidator
{
    // Trả về thông báo lỗi, null nếu hợp lệ.
    public static string? ValidateRegister(RegisterRequest r)
    {
        if (string.IsNullOrWhiteSpace(r.Username) || string.IsNullOrWhiteSpace(r.DisplayName)
            || string.IsNullOrEmpty(r.Password))
            return "Vui lòng nhập đầy đủ thông tin.";

        string username = r.Username.Trim();
        if (username.Length is < 3 or > 50 ||
            !username.All(c => char.IsAsciiLetterOrDigit(c) || c is '.' or '_' or '-'))
            return "Tài khoản dài 3–50 ký tự, chỉ gồm chữ, số, '.', '_', '-'.";

        if (r.DisplayName.Trim().Length > 100)
            return "Tên hiển thị tối đa 100 ký tự.";

        if (r.Password.Contains(username, StringComparison.OrdinalIgnoreCase))
            return "Mật khẩu không được chứa tên tài khoản.";

        if (!IsStrongPassword(r.Password, out string? passwordError))
            return passwordError;

        if (!Enum.IsDefined(r.Role))
            return "Vai trò không hợp lệ.";

        return null;
    }

    public static bool IsStrongPassword(string password, out string? error)
    {
        error = null;

        if (password.Length is < 8 or > 128)
            error = "Mật khẩu dài 8–128 ký tự.";
        else if (!password.Any(char.IsAsciiLetterUpper))
            error = "Mật khẩu phải có ít nhất 1 chữ in hoa (A–Z).";
        else if (!password.Any(char.IsAsciiLetterLower))
            error = "Mật khẩu phải có ít nhất 1 chữ thường (a–z).";
        else if (!password.Any(char.IsAsciiDigit))
            error = "Mật khẩu phải có ít nhất 1 chữ số (0–9).";
        else if (!password.Any(c => char.IsAscii(c) && !char.IsAsciiLetterOrDigit(c) && !char.IsWhiteSpace(c)))
            error = "Mật khẩu phải có ít nhất 1 ký tự đặc biệt (ví dụ: ! @ # $ % ^ & *).";
        else if (password.Any(char.IsWhiteSpace))
            error = "Mật khẩu không được chứa khoảng trắng.";

        return error is null;
    }

    public static bool IsValidEmail(string? email)
        => !string.IsNullOrWhiteSpace(email) && email.Length <= 100
           && System.Net.Mail.MailAddress.TryCreate(email, out _);
}
