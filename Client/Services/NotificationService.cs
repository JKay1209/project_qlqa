using QLQA.DTOs.Auth;
using Shared.DTOs.Auth;
using System.Threading.Tasks;

namespace Client.Services
{
<<<<<<< HEAD:Client/Services/NotificationService.cs
    internal class NotificationService
=======
    internal class AuthService : ApiService
>>>>>>> c086b91 (Hoan thanh giao dien login):Client/Service/AuthService.cs
    {
        public async Task<LoginResponse?> LoginAsync(
            string username,
            string password)
        {
            LoginRequest request =
                new LoginRequest(
                    username,
                    password);

            return await PostAsync<LoginResponse>(
                "api/auth/login",
                request);
        }
    }
}