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

        public Task<UserDto> RegisterAsync(
            RegisterRequest request,
            CancellationToken ct = default)
            => PostAsync<UserDto>("api/auth/register", request, ct);


        public async Task ForgotPasswordAsync(string email,CancellationToken ct = default)
        {
            await PostAsync(
                "api/auth/forgot-password",
                new ForgotPasswordRequest(email));
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
