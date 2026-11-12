using Shared.DTOs.Auth;
using System.Threading;
using System.Threading.Tasks;

namespace Client.Services
{
    internal class AuthService : ApiService
    {
        public async Task<LoginResponse> LoginAsync(
            string username,
            string password,
            CancellationToken ct = default)
        {
            LoginRequest request = new LoginRequest(username, password);

            return await PostAsync<LoginResponse>("api/auth/login", request, ct);
        }

        public Task<UserDto> RegisterAsync(RegisterRequest request,
            CancellationToken ct = default)
            => PostAsync<UserDto>("api/auth/register", request, ct);


        // Bước 1: Gửi email yêu cầu OTP
        public Task<MessageResponse> ForgotPasswordAsync(
            string email,
            CancellationToken ct = default)
            => PostAsync<MessageResponse>(
                "api/auth/forgot-password",
                new ForgotPasswordRequest(email), ct);

        // Bước 2: Gửi email + OTP để xác thực
        public Task<VerifyOtpResponse> VerifyOtpAsync(
            string email,
            string otp,
            CancellationToken ct = default)
            => PostAsync<VerifyOtpResponse>(
                "api/auth/verify-otp",
                new VerifyOtpRequest(email, otp),
                ct);

        // Bước 3: Gửi Reset Token + mật khẩu mới
        public Task<MessageResponse> ResetPasswordAsync(
            string resetToken,
            string newPassword,
            CancellationToken ct = default)
            => PostAsync<MessageResponse>(
                "api/auth/reset-password",
                new ResetPasswordRequest(resetToken, newPassword), ct);
    }
}
