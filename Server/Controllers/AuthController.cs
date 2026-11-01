using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Services;
using Shared.DTOs.Auth;

namespace Server.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(AuthService auth) : ControllerBase
{
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

    // TODO: POST /api/auth/logout, POST /api/auth/refresh (làm sau)
}
