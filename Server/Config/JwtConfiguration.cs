using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

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
