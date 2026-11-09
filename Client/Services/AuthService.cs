using Shared.DTOs.Auth;
using System.Threading;
using System.Threading.Tasks;

namespace Client.Services
{
    internal class AuthService : ApiService
    {
        public async Task<LoginResponse?> LoginAsync(
            string username,
            string password,
            CancellationToken ct = default)
        {
            LoginRequest request =
                new LoginRequest(username, password);

            return await PostAsync<LoginResponse>(
                "api/auth/login",
                request);
        }

        public async Task RegisterAsync(
            RegisterRequest request,
            CancellationToken ct = default)
        {
            // Tạm thời để build được
            await Task.CompletedTask;
        }

        public async Task ForgotPasswordAsync(
            string email,
            CancellationToken ct = default)
        {
            await Task.CompletedTask;
        }

        public async Task<VerifyOtpResponse> VerifyOtpAsync(
            string email,
            string otp,
            CancellationToken ct = default)
        {
            await Task.CompletedTask;

            return new VerifyOtpResponse("");
        }

        public async Task ResetPasswordAsync(
            string resetToken,
            string newPassword,
            CancellationToken ct = default)
        {
            await Task.CompletedTask;
        }
    }
}
