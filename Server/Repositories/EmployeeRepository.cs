using Microsoft.EntityFrameworkCore;
using Server.Models.Database;
using Server.Models.Entities;

namespace Server.Repositories;

// Chỉ truy vấn dữ liệu 
public class EmployeeRepository(RestaurantDbContext db)
{
    public Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
        => db.Users.Include(u => u.Role)
                   .AsNoTracking()
                   .FirstOrDefaultAsync(u => u.Username == username, ct);

    public Task<User?> GetByIdAsync(int id, CancellationToken ct = default)
        => db.Users.Include(u => u.Role)
                   .AsNoTracking()
                   .FirstOrDefaultAsync(u => u.UserId == id, ct);
}
