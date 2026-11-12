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

    // Kiem tra email trong database
    public Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
    => db.Users.AnyAsync(u => u.Email == email, ct);

    // Them username moi vao database
    public async Task AddAsync(User user, CancellationToken ct = default)
    {
        db.Users.Add(user);
        await db.SaveChangesAsync(ct);
    }

    // ============ Forgot password =======
    // Tìm tài khoản bằng email
    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        => db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email, ct);

    // Lấy yêu cầu đặt lại mật khẩu mới nhất
    public Task<PasswordResetRequest?> GetLatestResetAsync(int userId, CancellationToken ct = default)
        => db.PasswordResetRequests
            .Where(r => r.UserId == userId && !r.IsUsed)
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync(ct);

    // Tìm yêu cầu bằng hash Reset Token
    public Task<PasswordResetRequest?> GetResetByTokenHashAsync(string tokenHash, CancellationToken ct = default)
        => db.PasswordResetRequests
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.ResetTokenHash == tokenHash, ct);

    // Vô hiệu hóa các yêu cầu OTP cũ
    public Task InvalidateResetsAsync(int userId, CancellationToken ct = default)
        => db.PasswordResetRequests
            .Where(r => r.UserId == userId && !r.IsUsed)
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.IsUsed, true), ct);

    // Thêm yêu cầu OTP mới
    public async Task AddResetAsync(PasswordResetRequest request, CancellationToken ct = default)
    {
        db.PasswordResetRequests.Add(request);
        await db.SaveChangesAsync(ct);
    }

    // Lưu thay đổi
    public Task SaveChangesAsync(CancellationToken ct = default)
        => db.SaveChangesAsync(ct);
}