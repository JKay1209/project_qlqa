using Shared.Constants;
using Shared.DTOs.Auth;

namespace Client.Services;

public class AuthService(ApiService api)
{
    public AuthService() : this(new ApiService()) { }

    // ===================== Login =========================
    // POST /api/auth/login
    public Task<LoginResponse> LoginAsync(string username, string password, CancellationToken ct = default)
        => api.PostAsync<LoginRequest, LoginResponse>(ApiRoutes.Login, new LoginRequest(username, password), ct);

    // GET /api/auth/me
    public Task<UserDto> GetMeAsync(CancellationToken ct = default)
        => api.GetAsync<UserDto>(ApiRoutes.Me, ct);


    // ======================= Register =========================
    // POST /api/auth/register  (cần token Admin, ApiService tự đính kèm)
    public Task<UserDto> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
        => api.PostAsync<RegisterRequest, UserDto>(ApiRoutes.Register, request, ct);

    
    // ==================== Forgot password ===========================
    // POST /api/auth/forgot-password: yêu cầu gửi OTP
    public Task<MessageResponse> ForgotPasswordAsync(string email, CancellationToken ct = default)
        => api.PostAsync<ForgotPasswordRequest, MessageResponse>(ApiRoutes.ForgotPassword, new ForgotPasswordRequest(email), ct);

    // POST /api/auth/verify-otp: xác thực OTP, nhận ResetToken
    public Task<VerifyOtpResponse> VerifyOtpAsync(string email, string otp, CancellationToken ct = default)
        => api.PostAsync<VerifyOtpRequest, VerifyOtpResponse>(ApiRoutes.VerifyOtp, new VerifyOtpRequest(email, otp), ct);

    // POST /api/auth/reset-password: đặt mật khẩu mới
    public Task<MessageResponse> ResetPasswordAsync(string resetToken, string newPassword, CancellationToken ct = default)
        => api.PostAsync<ResetPasswordRequest, MessageResponse>(ApiRoutes.ResetPassword, new ResetPasswordRequest(resetToken, newPassword), ct);
}
