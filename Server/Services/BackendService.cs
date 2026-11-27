using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Server.Config;
using Server.Models.Database;
using Server.Models.Entities;
using Server.Network;
using Server.Network.Routing;
using Server.Security;
using Shared.DTOs.Order;
using Shared.DTOs.Table;
using Shared.Enums;

namespace Server.Services;

// Nghiệp vụ cho Socket Server: xác thực JWT và đọc/ghi order, bàn.
// Singleton; mỗi yêu cầu tạo một scope riêng cho DbContext (giống mỗi HTTP request).
public sealed class RestaurantBackend(
    IServiceScopeFactory scopes,
    IOptions<JwtOptions> jwtOptions,
    TokenBlacklist blacklist) : BackendRouting
{
    private readonly JsonWebTokenHandler _jwt = new();

    // ============================ Xác thực ============================
    public async Task<SessionUser?> AuthenticateAsync(string token, CancellationToken ct)
    {
        var o = jwtOptions.Value;
        var result = await _jwt.ValidateTokenAsync(token, new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = o.Issuer,
            ValidateAudience = true,
            ValidAudience = o.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(o.Key)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        });

        if (!result.IsValid || result.SecurityToken is not JsonWebToken jwt || blacklist.IsRevoked(jwt.Id))
            return null;

        if (!int.TryParse(result.ClaimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId))
            return null;

        // Lấy vai trò mới nhất từ CSDL (tài khoản có thể đã bị khóa / đổi vai trò)
        using var scope = scopes.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RestaurantDbContext>();
        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == userId && u.IsActive, ct);

        return user is null ? null : new SessionUser(userId, (UserRole)user.RoleId, jwt.Id, jwt.ValidTo);
    }

    public bool IsStillValid(SessionUser user)
        => user.ExpiresAtUtc > DateTime.UtcNow && !blacklist.IsRevoked(user.TokenId);

    // ============================ Order ============================
    public async Task<OrderCreated> CreateOrderAsync(SessionUser user, CreateOrderRequest request, CancellationToken ct)
    {
        using var scope = scopes.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RestaurantDbContext>();

        var table = await db.Tables.FirstOrDefaultAsync(t => t.TableId == request.TableId, ct)
            ?? throw new RouteRejectedException("Bàn không tồn tại.");

        // Tên món và giá lấy từ CSDL, KHÔNG tin giá Client gửi lên
        var ids = request.Items.Select(i => i.MenuItemId).Distinct().ToList();
        var menu = await db.MenuItems.AsNoTracking()
            .Where(m => ids.Contains(m.MenuItemId) && m.IsAvailable)
            .ToDictionaryAsync(m => m.MenuItemId, ct);
        if (menu.Count != ids.Count)
            throw new RouteRejectedException("Có món không tồn tại hoặc đã hết.");

        // Bàn đang có order mở thì gọi thêm vào order đó
        var order = await db.Orders.FirstOrDefaultAsync(o => o.TableId == table.TableId
            && o.Status != OrderStatus.Paid && o.Status != OrderStatus.Cancelled, ct);
        if (order is null)
        {
            order = new Order { TableId = table.TableId, UserId = user.UserId };
            db.Orders.Add(order);
        }
        order.Status = OrderStatus.SentToKitchen;

        DateTime sentAt = DateTime.UtcNow;
        var added = request.Items.Select(i => new OrderDetail
        {
            MenuItemId = i.MenuItemId,
            Quantity = i.Quantity,
            UnitPrice = menu[i.MenuItemId].Price,
            Note = string.IsNullOrWhiteSpace(i.Note) ? null : i.Note.Trim(),
            Status = ItemStatus.Pending,
            SentAt = sentAt
        }).ToList();

        foreach (var detail in added) order.OrderDetails.Add(detail);

        TableChangedEvent? tableChange = null;
        if (table.Status != TableStatus.Occupied)
        {
            table.Status = TableStatus.Occupied;
            tableChange = new TableChangedEvent(table.TableId, table.Status);
        }

        await db.SaveChangesAsync(ct);   // 1 lần lưu = 1 transaction: order + bàn cùng thành công hoặc cùng hủy

        var items = added.Select(d => ToDto(d, menu[d.MenuItemId].Name)).ToList();
        return new OrderCreated(
            new OrderDto(order.OrderId, order.TableId, order.Status, order.CreatedAt, items), tableChange);
    }

    public async Task<ItemChanged> ChangeItemStatusAsync(SessionUser user, ItemStatusChangedEvent change, CancellationToken ct)
    {
        using var scope = scopes.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RestaurantDbContext>();

        var order = await db.Orders.Include(o => o.OrderDetails)
            .FirstOrDefaultAsync(o => o.OrderId == change.OrderId
                && o.Status != OrderStatus.Paid && o.Status != OrderStatus.Cancelled, ct)
            ?? throw new RouteRejectedException("Order không tồn tại hoặc đã đóng.");

        // Một món có thể được gọi nhiều lần trong order: đổi mọi dòng đang chuyển được
        var targets = order.OrderDetails
            .Where(d => d.MenuItemId == change.MenuItemId
                && RoutingRules.CanChangeItem(user.Role, d.Status, change.NewStatus))
            .ToList();
        if (targets.Count == 0)
            throw new RouteRejectedException("Không thể đổi món sang trạng thái này.");

        foreach (var d in targets) d.Status = change.NewStatus;

        if (order.OrderDetails.All(d => d.Status is ItemStatus.Served or ItemStatus.Cancelled))
            order.Status = order.OrderDetails.All(d => d.Status == ItemStatus.Cancelled)
                ? OrderStatus.Cancelled
                : OrderStatus.Served;

        await db.SaveChangesAsync(ct);
        return new ItemChanged(change with { 
            DetailIds = targets.Select(d => d.OrderDetailId).ToList() 
        });
    }

    // ============================ Bàn ============================
    public async Task<TableChangedEvent> ChangeTableStatusAsync(SessionUser user, TableChangedEvent change, CancellationToken ct)
    {
        using var scope = scopes.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RestaurantDbContext>();

        var table = await db.Tables.FirstOrDefaultAsync(t => t.TableId == change.TableId, ct)
            ?? throw new RouteRejectedException("Bàn không tồn tại.");

        bool unpaid = await db.Orders.AnyAsync(o => o.TableId == table.TableId
            && o.Status != OrderStatus.Paid && o.Status != OrderStatus.Cancelled
            && o.OrderDetails.Any(d => d.Status != ItemStatus.Cancelled), ct);

        if (!RoutingRules.CanChangeTable(user.Role, table.Status, change.NewStatus, unpaid))
            throw new RouteRejectedException("Không thể đổi bàn sang trạng thái này.");

        table.Status = change.NewStatus;
        await db.SaveChangesAsync(ct);
        return new TableChangedEvent(table.TableId, table.Status);
    }

    // ============================ Snapshot ============================
    public async Task<Snapshot> GetSnapshotAsync(CancellationToken ct)
    {
        using var scope = scopes.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RestaurantDbContext>();

        var tables = await db.Tables.AsNoTracking()
            .OrderBy(t => t.TableId)
            .Select(t => new TableDto(t.TableId, t.TableName, t.Capacity, t.Status))
            .ToListAsync(ct);

        var orders = await db.Orders.AsNoTracking()
            .Include(o => o.OrderDetails).ThenInclude(d => d.MenuItem)
            .Where(o => o.Status != OrderStatus.Paid && o.Status != OrderStatus.Cancelled)
            .OrderBy(o => o.CreatedAt)
            .ToListAsync(ct);

        return new Snapshot(tables, orders.Select(o => new OrderDto(
            o.OrderId, o.TableId, o.Status, o.CreatedAt,
            o.OrderDetails.Select(d => ToDto(d, d.MenuItem.Name)).ToList())).ToList());
    }

    private static OrderItemDto ToDto(OrderDetail d, string name)
        => new(d.MenuItemId, name, d.Quantity, d.UnitPrice, d.Status, d.Note,
               d.OrderDetailId, DateTime.SpecifyKind(d.SentAt, DateTimeKind.Utc));
}