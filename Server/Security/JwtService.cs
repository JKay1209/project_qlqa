using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Server.Config;
using Server.Models.Entities;

namespace Server.Security;

public class JwtService
{
    private readonly JwtOptions _opt;
    private readonly SigningCredentials _credentials;
    private readonly JsonWebTokenHandler _handler = new();

    public JwtService(IOptions<JwtOptions> options)
    {
        _opt = options.Value;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opt.Key));
        _credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    }

    /// <summary>Yêu cầu user.Role đã được Include.</summary>
    public (string Token, DateTime ExpiresAt) GenerateAccessToken(User user)
    {
        DateTime expires = DateTime.UtcNow.AddMinutes(_opt.ExpiresMinutes);

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role.RoleName)
            }),
            Issuer = _opt.Issuer,
            Audience = _opt.Audience,
            Expires = expires,
            SigningCredentials = _credentials
        };

        return (_handler.CreateToken(descriptor), expires);
    }
}
