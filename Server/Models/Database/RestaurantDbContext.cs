using Microsoft.EntityFrameworkCore;
using Server.Models.Entities;
using Shared.Enums;

namespace Server.Models.Database;

public class RestaurantDbContext(DbContextOptions<RestaurantDbContext> options) : DbContext(options)
{
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();

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

            e.HasOne(x => x.Role)
             .WithMany(r => r.Users)
             .HasForeignKey(x => x.RoleId)
             .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
