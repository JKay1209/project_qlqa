using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Server.Security;
using System.Text;

namespace Server.Config;

public static class JwtConfig
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration config)
    {
        var section = config.GetSection(JwtOptions.SectionName);
        var o = section.Get<JwtOptions>() ?? throw new InvalidOperationException("Thiếu mục Jwt trong appsettings.json.");
        if (Encoding.UTF8.GetByteCount(o.Key) < 32)
            throw new InvalidOperationException("Jwt:Key phải dài tối thiểu 32 byte.");

        services.Configure<JwtOptions>(section);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opt =>
            {
                opt.Events = new JwtBearerEvents
                {
                    OnTokenValidated = ctx =>
                    {
                        var blacklist = ctx.HttpContext.RequestServices.GetRequiredService<TokenBlacklist>();
                        if (ctx.SecurityToken is JsonWebToken jwt && blacklist.IsRevoked(jwt.Id))
                            ctx.Fail("Token đã bị thu hồi.");
                        return Task.CompletedTask;
                    }
                };

                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = o.Issuer,
                    ValidateAudience = true,
                    ValidAudience = o.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(o.Key)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });

        services.AddAuthorization();
        return services;
    }
}
