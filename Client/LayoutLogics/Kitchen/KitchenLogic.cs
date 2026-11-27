using Client.Network;
using Client.Security;
using Shared.DTOs.Order;
using Shared.DTOs.Table;
using Shared.Enums;

namespace Client.LayoutLogics.Kitchen;

// Logic màn hình bếp: gộp snapshot + sự kiện realtime thành 1 trạng thái, gửi lệnh đổi món.
// LƯU Ý: Changed / NewOrderArrived được gọi từ luồng nền -> UI phải dùng Control.BeginInvoke.
public sealed class KitchenLogic : IDisposable
{
    private sealed class OrderState
    {
        public int TableId;
        public DateTime CreatedAtUtc;
        public List<KitchenLine> Lines = [];
    }

    private readonly object _lock = new();
    private readonly Dictionary<int, OrderState> _orders = new();
    private Dictionary<int, string> _tableNames = new();
    private RealtimeConnection? _rt;

    // Ngưỡng cảnh báo thời gian chờ (UI đổi màu theo LevelOf)
    public TimeSpan WarnAfter { get; set; } = TimeSpan.FromMinutes(10);
    public TimeSpan LateAfter { get; set; } = TimeSpan.FromMinutes(20);

    public event Action? Changed;               // dữ liệu đổi -> UI vẽ lại
    public event Action<int>? NewOrderArrived;  // orderId có món mới -> UI phát tiếng "ding"

    // Chỉ Bếp (và Chủ quán/Quản lý) được thao tác món; khớp RoutingRules.CanChangeItem.
    public static bool CanOperate
        => SessionManager.Role is UserRole.Kitchen or UserRole.Admin or UserRole.Manager;

    // ===================== Vòng đời =====================
    public void Start()
    {
        if (_rt is not null) return;
        _rt = SocketManager.Realtime;
        _rt.TablesLoaded += OnTablesLoaded;
        _rt.OrdersLoaded += OnOrdersLoaded;
        _rt.OrderReceived += OnOrderReceived;
        _rt.ItemStatusChanged += OnItemStatusChanged;
        _ = _rt.RequestSyncAsync();   // mở màn hình giữa chừng vẫn có đủ dữ liệu
    }

    public void Dispose()
    {
        if (_rt is null) return;
        _rt.TablesLoaded -= OnTablesLoaded;
        _rt.OrdersLoaded -= OnOrdersLoaded;
        _rt.OrderReceived -= OnOrderReceived;
        _rt.ItemStatusChanged -= OnItemStatusChanged;
        _rt = null;
    }

    // ===================== Đọc dữ liệu (cho UI/Timer) =====================
    // Bản chụp bất biến, an toàn gọi từ luồng UI. Phiếu có món chờ lâu nhất đứng đầu.
    public IReadOnlyList<KitchenTicket> GetTickets()
    {
        lock (_lock)
        {
            return _orders
                .Select(kv => new KitchenTicket(
                    kv.Key, kv.Value.TableId,
                    _tableNames.GetValueOrDefault(kv.Value.TableId, $"Bàn {kv.Value.TableId}"),
                    kv.Value.CreatedAtUtc,
                    kv.Value.Lines
                        .Where(l => l.Status is ItemStatus.Pending or ItemStatus.Cooking or ItemStatus.Ready)
                        .OrderBy(l => l.SentAtUtc).ToList()))
                .Where(t => t.Lines.Count > 0)
                .OrderBy(t => t.OldestActiveAtUtc ?? DateTime.MaxValue)
                .ThenBy(t => t.CreatedAtUtc)
                .ToList();
        }
    }

    // Mức cảnh báo cho 1 dòng; UI Timer 1s gọi với DateTime.UtcNow.
    public WaitLevel LevelOf(KitchenLine line, DateTime nowUtc)
    {
        if (!line.IsActive) return WaitLevel.Normal;
        var w = line.WaitingAt(nowUtc);
        return w >= LateAfter ? WaitLevel.Late : w >= WarnAfter ? WaitLevel.Warning : WaitLevel.Normal;
    }

    // ===================== Gửi lệnh (Protocol: ITEM|reqId|json) =====================
    public Task<(bool Ok, string? Error)> StartCookingAsync(int orderId, int menuItemId, CancellationToken ct = default)
        => ChangeAsync(orderId, menuItemId, ItemStatus.Cooking, ct);

    public Task<(bool Ok, string? Error)> MarkReadyAsync(int orderId, int menuItemId, CancellationToken ct = default)
        => ChangeAsync(orderId, menuItemId, ItemStatus.Ready, ct);

    public Task<(bool Ok, string? Error)> CancelItemAsync(int orderId, int menuItemId, CancellationToken ct = default)
        => ChangeAsync(orderId, menuItemId, ItemStatus.Cancelled, ct);

    // Không sửa state cục bộ ở đây: Server broadcast ITEM_STATUS cho cả người gửi,
    // OnItemStatusChanged sẽ cập nhật -> mọi máy cùng một nguồn sự thật.
    private async Task<(bool Ok, string? Error)> ChangeAsync(int orderId, int menuItemId, ItemStatus status, CancellationToken ct)
    {
        if (_rt is null) return (false, "Màn hình bếp chưa khởi động.");
        if (!CanOperate) return (false, "Bạn không có quyền thao tác món.");
        try { return await _rt.ChangeItemStatusAsync(orderId, menuItemId, status, ct); }
        catch (OperationCanceledException) { return (false, "Đã hủy."); }
    }

    // ===================== Xử lý sự kiện realtime =====================
    private void OnTablesLoaded(List<TableDto> tables)
    {
        lock (_lock) _tableNames = tables.ToDictionary(t => t.Id, t => t.Name);
        Changed?.Invoke();
    }

    // Snapshot: thay toàn bộ (cũng chạy lại sau reconnect -> bù sự kiện bị lỡ).
    private void OnOrdersLoaded(List<OrderDto> orders)
    {
        lock (_lock)
        {
            _orders.Clear();
            foreach (var o in orders)
                _orders[o.Id] = new OrderState
                {
                    TableId = o.TableId,
                    CreatedAtUtc = AsUtc(o.CreatedAt),
                    Lines = o.Items.Select(ToLine).ToList()
                };
        }
        Changed?.Invoke();
    }

    // ORDER_NEW: Items chỉ gồm các món VỪA gửi -> nối vào order (tạo mới nếu chưa có).
    private void OnOrderReceived(OrderDto o)
    {
        lock (_lock)
        {
            if (!_orders.TryGetValue(o.Id, out var state))
                _orders[o.Id] = state = new OrderState { TableId = o.TableId, CreatedAtUtc = AsUtc(o.CreatedAt) };

            foreach (var item in o.Items)
                if (state.Lines.All(l => l.DetailId != item.DetailId))   // chống trùng
                    state.Lines.Add(ToLine(item));
        }
        NewOrderArrived?.Invoke(o.Id);
        Changed?.Invoke();
    }

    private void OnItemStatusChanged(ItemStatusChangedEvent e)
    {
        bool changed = false, unknownOrder = false;
        lock (_lock)
        {
            if (!_orders.TryGetValue(e.OrderId, out var state)) unknownOrder = true;
            else
                for (int i = 0; i < state.Lines.Count; i++)
                {
                    var l = state.Lines[i];
                    bool hit = e.DetailIds is { } ids ? ids.Contains(l.DetailId) : l.MenuItemId == e.MenuItemId;
                    if (!hit || l.Status == e.NewStatus) continue;
                    state.Lines[i] = l with { Status = e.NewStatus };
                    changed = true;
                }
        }
        if (unknownOrder) _ = _rt?.RequestSyncAsync();   // lỡ order -> xin snapshot lại
        if (changed) Changed?.Invoke();
    }

    // ===================== Helper =====================
    private static KitchenLine ToLine(OrderItemDto i)
        => new(i.DetailId, i.MenuItemId, i.Name, i.Quantity, i.Note, i.Status, AsUtc(i.SentAt));

    // JSON từ DB có thể mất Kind -> luôn coi là UTC để tính giờ chờ đúng.
    private static DateTime AsUtc(DateTime d) => d.Kind switch
    {
        DateTimeKind.Utc => d,
        DateTimeKind.Local => d.ToUniversalTime(),
        _ => DateTime.SpecifyKind(d, DateTimeKind.Utc)
    };
}