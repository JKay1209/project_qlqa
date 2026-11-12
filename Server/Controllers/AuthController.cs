using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Services;
using Shared.DTOs.Auth;
using Shared.Enums;
using System.Security.Claims;

namespace Server.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(AuthService auth) : ControllerBase
{
    // =========================== Login ===============================
    // POST /api/auth/login
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrEmpty(request.Password))
            return BadRequest(new ErrorResponse("Vui lòng nhập tài khoản và mật khẩu."));

        if (request.Username.Length > 50 || request.Password.Length > 128)
            return BadRequest(new ErrorResponse("Dữ liệu không hợp lệ."));

        var result = await auth.LoginAsync(request, ct);
        return result is null
            ? Unauthorized(new ErrorResponse("Sai tài khoản hoặc mật khẩu."))
            : Ok(result);
    }

    // GET /api/auth/me
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> Me(CancellationToken ct)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int id))
            return Unauthorized();

        var dto = await auth.GetCurrentUserAsync(id, ct);
        return dto is null ? Unauthorized() : Ok(dto);
    }


    // ================== Register =========================
    // Cho phep nguoi dung dang nhap tai khoan moi
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        string? error = ValidateRegister(request);
        if (error is not null)
            return BadRequest(new ErrorResponse(error));

        var (user, code) = await auth.RegisterAsync(request, ct);

        return code switch
        {
            ErrorCode.None => StatusCode(StatusCodes.Status201Created, user),
            ErrorCode.Conflict => Conflict(new ErrorResponse("Tên tài khoản hoặc email đã tồn tại.")),
            _ => StatusCode(StatusCodes.Status500InternalServerError,
                            new ErrorResponse("Không thể tạo tài khoản."))
        };
    }

    // Kiểm tra dữ liệu đầu vào. Trả về thông báo lỗi.
    private static string? ValidateRegister(RegisterRequest r)
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

        if (!string.IsNullOrWhiteSpace(r.Email) && !IsValidEmail(r.Email))
            return "Email không hợp lệ.";

        return null;
    }

    // Kiểm tra độ mạnh mật khẩu. Trả true nếu hợp lệ.
    private static bool IsStrongPassword(string password, out string? error)
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


    // ============================== Forgot password =================================
    // POST /api/auth/forgot-password  — Bước 1: gửi OTP qua email
    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public async Task<ActionResult<MessageResponse>> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken ct)
    {
        if (!IsValidEmail(request.Email))
            return BadRequest(new ErrorResponse("Email không hợp lệ."));

        await auth.RequestPasswordResetAsync(request.Email, ct);

        // Thong bao chung
        return Ok(new MessageResponse("Nếu email tồn tại trong hệ thống, mã OTP đã được gửi. Mã có hiệu lực 5 phút."));
    }

    // POST /api/auth/verify-otp  — Bước 2: xác thực OTP, nhận ResetToken
    [AllowAnonymous]
    [HttpPost("verify-otp")]
    public async Task<ActionResult<VerifyOtpResponse>> VerifyOtp([FromBody] VerifyOtpRequest request, CancellationToken ct)
    {
        if (!IsValidEmail(request.Email) || request.Otp is not { Length: 6 } || !request.Otp.All(char.IsAsciiDigit))
            return BadRequest(new ErrorResponse("Dữ liệu không hợp lệ."));

        var token = await auth.VerifyOtpAsync(request.Email, request.Otp, ct);
        return token is null
            ? BadRequest(new ErrorResponse("Mã OTP không đúng hoặc đã hết hạn."))
            : Ok(new VerifyOtpResponse(token));
    }

    // POST /api/auth/reset-password  — Bước 3: đặt mật khẩu mới bằng ResetToken
    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<ActionResult<MessageResponse>> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ResetToken) || request.ResetToken.Length > 100)
            return BadRequest(new ErrorResponse("Dữ liệu không hợp lệ."));

        if (string.IsNullOrEmpty(request.NewPassword) || request.NewPassword.Length < 8 || request.NewPassword.Length > 128)
            return BadRequest(new ErrorResponse("Mật khẩu phải từ 8 đến 128 ký tự."));

        bool ok = await auth.ResetPasswordAsync(request.ResetToken, request.NewPassword, ct);
        return ok
            ? Ok(new MessageResponse("Đặt lại mật khẩu thành công. Hãy đăng nhập lại."))
            : BadRequest(new ErrorResponse("Phiên đặt lại mật khẩu không hợp lệ hoặc đã hết hạn."));
    }

    // Kiểm tra định dạng email cơ bản
    private static bool IsValidEmail(string? email)
        => !string.IsNullOrWhiteSpace(email) && email.Length <= 100 && System.Net.Mail.MailAddress.TryCreate(email, out _);


    // TODO: POST /api/auth/logout, POST /api/auth/refresh (làm sau)
}