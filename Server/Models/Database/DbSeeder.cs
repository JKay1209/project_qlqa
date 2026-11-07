using Microsoft.EntityFrameworkCore;
using Server.Models.Entities;
using Shared.Enums;
using Server.Security;

namespace Server.Models.Database;

// Tạo tài khoản admin đầu tiên nếu bảng Users còn trống. Chạy khi server khởi động.
public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RestaurantDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<PasswordHasher>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbSeeder");

        if (await db.Users.AnyAsync()) return;

        string password = config["Seed:AdminPassword"] ?? "Admin@123";
        db.Users.Add(new User
        {
            Username = "admin",
            PasswordHash = hasher.Hash(password),
            DisplayName = "Quản trị viên",
            Email = config["Seed:AdminEmail"], 
            RoleId = (int)UserRole.Admin,
            IsActive = true
        });
        await db.SaveChangesAsync();
        logger.LogWarning("Đã tạo tài khoản mặc định 'admin'. Hãy đổi mật khẩu ngay.");
    }
}
