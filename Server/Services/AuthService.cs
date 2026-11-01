using Microsoft.AspNetCore.Identity;
using Server.Repositories;
using Server.Security;
using Shared.DTOs.Auth;
using Shared.Enums;
using Shared.DTOs.Auth;
using Shared.Enums;

namespace Server.Services;

public class AuthService(
    EmployeeRepository repo,
    PasswordHasher hasher,
    JwtService jwt,
    ILogger<AuthService> logger)
{
    /// <summary>Trả null nếu đăng nhập thất bại (không phân biệt sai username / sai mật khẩu / bị khóa).</summary>
    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        string username = request.Username.Trim();

        var user = await repo.GetByUsernameAsync(username, ct);
        if (user is null)
        {
            hasher.VerifyDummy(request.Password);   // chống dò username qua thời gian phản hồi
            logger.LogWarning("Đăng nhập thất bại: username '{Username}' không tồn tại.", username);
            return null;
        }

        bool passwordOk = hasher.Verify(request.Password, user.PasswordHash);
        if (!passwordOk || !user.IsActive)
        {
            logger.LogWarning("Đăng nhập thất bại: '{Username}' (sai mật khẩu hoặc bị khóa).", username);
            return null;
        }

        var (token, expiresAt) = jwt.GenerateAccessToken(user);
        logger.LogInformation("Đăng nhập thành công: '{Username}'.", username);

        return new LoginResponse(
            user.UserId, user.Username, user.DisplayName, (UserRole)user.RoleId, token, expiresAt);
    }

    public async Task<UserDto?> GetCurrentUserAsync(int userId, CancellationToken ct = default)
    {
        var user = await repo.GetByIdAsync(userId, ct);
        if (user is null || !user.IsActive) return null;
        return new UserDto(user.UserId, user.Username, user.DisplayName, (UserRole)user.RoleId);
    }
}
