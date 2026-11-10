using Microsoft.EntityFrameworkCore;
using Server.Models.Database;
using Server.Models.Entities;

namespace Server.Repositories;

public class EmployeeRepository(RestaurantDbContext db)
{
    // ============ Login =======
    // Chỉ truy vấn dữ liệu 
    public Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
        => db.Users.Include(u => u.Role)
                   .AsNoTracking()
                   .FirstOrDefaultAsync(u => u.Username == username, ct);

    public Task<User?> GetByIdAsync(int id, CancellationToken ct = default)
        => db.Users.Include(u => u.Role)
                   .AsNoTracking()
                   .FirstOrDefaultAsync(u => u.UserId == id, ct);

    // ============ Register =======
    // Kiem tra du lieu username trong database
    public Task<bool> UsernameExistsAsync(string username, CancellationToken ct = default)
        => db.Users.AnyAsync(u => u.Username == username, ct);

    // Them username moi vao database
    public async Task AddAsync(User user, CancellationToken ct = default)
    {
        db.Users.Add(user);
        await db.SaveChangesAsync(ct);
    }

    // ============ Forgot password =======
    // Tìm user theo email.
    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        => db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email, ct);

    // Lấy yêu cầu đặt lại mới nhất còn hiệu lực của user
    public Task<PasswordResetRequest?> GetLatestResetAsync(int userId, CancellationToken ct = default)
        => db.PasswordResetRequests
             .Where(r => r.UserId == userId && !r.IsUsed)
             .OrderByDescending(r => r.CreatedAt)
             .FirstOrDefaultAsync(ct);

    // Tìm yêu cầu theo hash của ResetToken
    public Task<PasswordResetRequest?> GetResetByTokenHashAsync(string tokenHash, CancellationToken ct = default)
        => db.PasswordResetRequests
             .Include(r => r.User)
             .FirstOrDefaultAsync(r => r.ResetTokenHash == tokenHash, ct);

    // Vô hiệu hóa mọi OTP cũ của user
    public Task InvalidateResetsAsync(int userId, CancellationToken ct = default)
        => db.PasswordResetRequests
             .Where(r => r.UserId == userId && !r.IsUsed)
             .ExecuteUpdateAsync(s => s.SetProperty(r => r.IsUsed, true), ct);

    // Thêm yêu cầu đặt lại mới
    public async Task AddResetAsync(PasswordResetRequest request, CancellationToken ct = default)
    {
        db.PasswordResetRequests.Add(request);
        await db.SaveChangesAsync(ct);
    }

    // Lưu các thay đổi trên entity đang được theo dõi
    public Task SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
}
