using Microsoft.EntityFrameworkCore;
using Server.Models.Database;

namespace Server.Config;

public static class DatabaseConfig
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration config)
    {
        string cs = config.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Thiếu ConnectionStrings:Default trong appsettings.json.");

        services.AddDbContext<RestaurantDbContext>(opt =>
            opt.UseSqlServer(cs, sql =>
            {
                sql.CommandTimeout(30);
                sql.EnableRetryOnFailure(3);
            }));
        return services;
    }
}
