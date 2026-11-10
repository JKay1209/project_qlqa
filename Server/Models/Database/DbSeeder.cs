using Microsoft.EntityFrameworkCore;
using Server.Models.Entities;
using Shared.Enums;
using Server.Security;

namespace Server.Models.Database;

// Tạo dữ liệu mẫu nếu bảng còn trống. Chạy khi server khởi động.
public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RestaurantDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<PasswordHasher>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbSeeder");

        await SeedAdminAsync(db, hasher, config, logger);
        await SeedTablesAsync(db);
        await SeedMenuAsync(db);
    }

    // Tạo tài khoản admin đầu tiên nếu bảng Users còn trống.
    private static async Task SeedAdminAsync(
        RestaurantDbContext db, PasswordHasher hasher, IConfiguration config, ILogger logger)
    {
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

    // Tạo 10 bàn mẫu nếu bảng Tables còn trống.
    private static async Task SeedTablesAsync(RestaurantDbContext db)
    {
        if (await db.Tables.AnyAsync()) return;

        for (int i = 1; i <= 10; i++)
        {
            db.Tables.Add(new Table
            {
                TableName = $"Bàn {i:00}",
                Capacity = i <= 6 ? 4 : 6,
                Status = TableStatus.Free
            });
        }
        await db.SaveChangesAsync();
    }

    // Tạo danh mục và món mẫu nếu bảng Categories còn trống.
    private static async Task SeedMenuAsync(RestaurantDbContext db)
    {
        if (await db.Categories.AnyAsync()) return;

        db.Categories.AddRange(
            new Category
            {
                Name = "Khai vị",
                MenuItems =
                {
                    new MenuItem { Name = "Gỏi cuốn", Price = 45000m },
                    new MenuItem { Name = "Chả giò", Price = 55000m }
                }
            },
            new Category
            {
                Name = "Món chính",
                MenuItems =
                {
                    new MenuItem { Name = "Cơm gà xối mỡ", Price = 65000m },
                    new MenuItem { Name = "Bò lúc lắc", Price = 120000m },
                    new MenuItem { Name = "Mì xào hải sản", Price = 85000m }
                }
            },
            new Category
            {
                Name = "Đồ uống",
                MenuItems =
                {
                    new MenuItem { Name = "Trà đá", Price = 10000m },
                    new MenuItem { Name = "Nước cam", Price = 35000m },
                    new MenuItem { Name = "Cà phê sữa", Price = 30000m }
                }
            },
            new Category
            {
                Name = "Tráng miệng",
                MenuItems =
                {
                    new MenuItem { Name = "Chè khúc bạch", Price = 35000m },
                    new MenuItem { Name = "Flan", Price = 25000m }
                }
            });
        await db.SaveChangesAsync();
    }
}