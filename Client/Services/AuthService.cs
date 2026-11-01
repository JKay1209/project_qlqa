using Client.Services;
using Shared.Constants;
using Shared.DTOs.Auth;

namespace Client.Services;

public class AuthService(ApiService api)
{
    public AuthService() : this(new ApiService()) { }

    // POST /api/auth/login
    public Task<LoginResponse> LoginAsync(string username, string password, CancellationToken ct = default)
        => api.PostAsync<LoginRequest, LoginResponse>(ApiRoutes.Login, new LoginRequest(username, password), ct);

    // GET /api/auth/me
    public Task<UserDto> GetMeAsync(CancellationToken ct = default)
        => api.GetAsync<UserDto>(ApiRoutes.Me, ct);
}
