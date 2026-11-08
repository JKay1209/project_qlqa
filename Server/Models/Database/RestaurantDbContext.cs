using Microsoft.EntityFrameworkCore;
using Server.Models.Entities;
using Shared.Enums;

namespace Server.Models.Database;

public class RestaurantDbContext(DbContextOptions<RestaurantDbContext> options) : DbContext(options)
{
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<PasswordResetRequest> PasswordResetRequests => Set<PasswordResetRequest>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<Table> Tables => Set<Table>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderDetail> OrderDetails => Set<OrderDetail>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Role>(e =>
        {
            e.ToTable("Roles");
            e.HasKey(x => x.RoleId);
            e.Property(x => x.RoleId).ValueGeneratedNever();   // Id lấy từ enum UserRole
            e.Property(x => x.RoleName).IsRequired().HasMaxLength(50);
            e.HasIndex(x => x.RoleName).IsUnique();

            // Seed vai trò theo enum UserRole
            e.HasData(Enum.GetValues<UserRole>()
                .Select(r => new Role { RoleId = (int)r, RoleName = r.ToString() }));
        });

        b.Entity<User>(e =>
        {
            e.ToTable("Users");
            e.HasKey(x => x.UserId);
            e.Property(x => x.Username).IsRequired().HasMaxLength(50);
            e.HasIndex(x => x.Username).IsUnique();
            e.Property(x => x.PasswordHash).IsRequired().HasMaxLength(256);
            e.Property(x => x.DisplayName).IsRequired().HasMaxLength(100);

            // cot Email 
            e.Property(x => x.Email).HasMaxLength(100);
            e.HasIndex(x => x.Email)
             .IsUnique()
             .HasFilter("[Email] IS NOT NULL"); 

            e.HasOne(x => x.Role)
             .WithMany(r => r.Users)
             .HasForeignKey(x => x.RoleId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // Bảng PasswordResetRequests
        b.Entity<PasswordResetRequest>(e =>
        {
            e.ToTable("PasswordResetRequests");
            e.HasKey(x => x.Id);
            e.Property(x => x.OtpHash).IsRequired().HasMaxLength(256);
            e.Property(x => x.ResetTokenHash).HasMaxLength(64);
            e.HasIndex(x => x.ResetTokenHash);
            e.HasIndex(x => new { x.UserId, x.CreatedAt });

            e.HasOne(x => x.User)
             .WithMany()
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<Category>(e =>
        {
            e.ToTable("Categories");
            e.HasKey(x => x.CategoryId);
            e.Property(x => x.Name).IsRequired().HasMaxLength(100);
            e.HasIndex(x => x.Name).IsUnique();
        });

        b.Entity<MenuItem>(e =>
        {
            e.ToTable("MenuItems");
            e.HasKey(x => x.MenuItemId);
            e.Property(x => x.Name).IsRequired().HasMaxLength(100);
            e.Property(x => x.Price).HasColumnType("decimal(18,2)");
            e.Property(x => x.Description).HasMaxLength(500);
            e.Property(x => x.ImageUrl).HasMaxLength(300);
            e.HasIndex(x => x.Name);
            e.HasIndex(x => new { x.CategoryId, x.IsAvailable });

            e.HasOne(x => x.Category)
             .WithMany(c => c.MenuItems)
             .HasForeignKey(x => x.CategoryId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<Table>(e =>
        {
            e.ToTable("Tables");
            e.HasKey(x => x.TableId);
            e.Property(x => x.TableName).IsRequired().HasMaxLength(50);
            e.HasIndex(x => x.TableName).IsUnique();
            e.Property(x => x.Status).HasConversion<int>();
            e.HasIndex(x => x.Status);
        });

        b.Entity<Order>(e =>
        {
            e.ToTable("Orders");
            e.HasKey(x => x.OrderId);
            e.Property(x => x.Status).HasConversion<int>();
            e.Property(x => x.Note).HasMaxLength(500);
            e.HasIndex(x => x.Status);
            e.HasIndex(x => x.CreatedAt);
            e.HasIndex(x => new { x.TableId, x.Status });   // tìm đơn đang mở của một bàn

            e.HasOne(x => x.Table)
             .WithMany()
             .HasForeignKey(x => x.TableId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.User)
             .WithMany()
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<OrderDetail>(e =>
        {
            e.ToTable("OrderDetails");
            e.HasKey(x => x.OrderDetailId);
            e.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)");
            e.Property(x => x.Note).HasMaxLength(300);
            e.Property(x => x.Status).HasConversion<int>();
            e.HasIndex(x => x.OrderId);
            e.HasIndex(x => x.Status);                      // bếp lọc món Pending/Cooking

            e.HasOne(x => x.Order)
             .WithMany(o => o.OrderDetails)
             .HasForeignKey(x => x.OrderId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.MenuItem)
             .WithMany()
             .HasForeignKey(x => x.MenuItemId)
             .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
