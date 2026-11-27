using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using Shared.DTOs.Auth;
using Shared.DTOs.Order;
using Shared.DTOs.Table;
using Shared.Enums;
using Shared.Protocol;
using Shared.Serialization;

namespace Client.Network;

// Lớp giao thức trên SocketClient: xác thực kết nối, nhận sự kiện Server đẩy xuống,
// gửi yêu cầu đổi trạng thái món / bàn.

internal sealed class RealtimeConnection : IDisposable
{
    private static readonly TimeSpan ReplyTimeout = TimeSpan.FromSeconds(5);

    private readonly SocketClient _socket;
    private readonly ConcurrentDictionary<Guid, TaskCompletionSource<(bool Ok, string? Error)>> _pending = new();
    private volatile string? _token;
    private volatile bool _authenticated;

    public RealtimeConnection(SocketClient socket)
    {
        _socket = socket;
        _socket.StateChanged += OnStateChanged;
        _socket.MessageReceived += OnMessageReceived;
    }

    public bool IsAuthenticated => _authenticated;
    public UserRole? Role { get; private set; }

    public event Action<UserRole>? Authenticated;
    public event Action<string>? AuthenticationFailed;
    public event Action<OrderDto>? OrderReceived;                    // Bếp, Thu ngân: Items = món vừa gửi
    public event Action<ItemStatusChangedEvent>? ItemStatusChanged;  // Phục vụ: món đã xong
    public event Action<TableChangedEvent>? TableStatusChanged;
    public event Action<List<TableDto>>? TablesLoaded;
    public event Action<List<OrderDto>>? OrdersLoaded;
    public event Action<string>? ServerError;

    // ===================== Phiên đăng nhập =====================
    public void SignIn(string token)
    {
        _token = token;
        _authenticated = false;
        _ = SendAuthAsync();   // nếu đang mất mạng thì sẽ gửi khi nối lại
    }

    public void SignOut()
    {
        _token = null;
        _authenticated = false;
        Role = null;
        _socket.DropConnection();   // Server quên phiên cùng với kết nối
    }

    // Màn hình vừa mở gọi hàm này để nhận lại TablesLoaded + OrdersLoaded.
    public Task<bool> RequestSyncAsync(CancellationToken ct = default)
        => _socket.SendLineAsync(Protocol.Sync, ct);

    // ===================== Yêu cầu đổi trạng thái =====================
    // Không xếp hàng khi mất mạng: đổi trạng thái trễ có thể ghi đè dữ liệu mới hơn.
    public Task<(bool Ok, string? Error)> ChangeItemStatusAsync(int orderId, int menuItemId, ItemStatus status, CancellationToken ct = default)
        => RequestAsync(Protocol.Item, new ItemStatusChangedEvent(orderId, menuItemId, status), ct);

    public Task<(bool Ok, string? Error)> ChangeTableStatusAsync(int tableId, TableStatus status, CancellationToken ct = default)
        => RequestAsync(Protocol.Table, new TableChangedEvent(tableId, status), ct);

    private async Task<(bool Ok, string? Error)> RequestAsync<T>(string command, T body, CancellationToken ct)
    {
        if (!_authenticated) return (false, "Chưa kết nối tới Server.");

        var id = Guid.NewGuid();
        var reply = new TaskCompletionSource<(bool Ok, string? Error)>(TaskCreationOptions.RunContinuationsAsynchronously);
        _pending[id] = reply;
        try
        {
            string json = JsonSerializer.Serialize(body, JsonDefaults.Options);
            if (!await _socket.SendLineAsync($"{command}|{id:N}|{json}", ct))
                return (false, "Mất kết nối tới Server.");

            return await reply.Task.WaitAsync(ReplyTimeout, ct);
        }
        catch (TimeoutException)
        {
            return (false, "Server không phản hồi.");
        }
        finally
        {
            _pending.TryRemove(id, out _);
        }
    }

    // ===================== Sự kiện từ SocketClient =====================
    private void OnStateChanged(ConnectionState state)
    {
        if (state == ConnectionState.Connected)
        {
            _ = SendAuthAsync();
            return;
        }

        _authenticated = false;
        foreach (var reply in _pending.Values)
            reply.TrySetResult((false, "Mất kết nối tới Server."));
    }

    private async Task SendAuthAsync()
    {
        string? token = _token;
        if (token is not null)
            await _socket.SendLineAsync($"{Protocol.Auth}|{token}");
    }

    private void OnMessageReceived(string line)
    {
        string[] head = line.Split(Protocol.Separator, 2);
        string body = head.Length > 1 ? head[1] : "";

        try
        {
            switch (head[0])
            {
                case Protocol.AuthOk when Enum.TryParse(body, out UserRole role) && _token is not null:
                    Role = role;
                    _authenticated = true;
                    Authenticated?.Invoke(role);
                    break;
                case Protocol.AuthFail:
                    _token = null;   // token hỏng/hết hạn: phải đăng nhập lại
                    _authenticated = false;
                    AuthenticationFailed?.Invoke(Read<ErrorResponse>(body).Message);
                    break;
                case Protocol.OrderNew:
                    OrderReceived?.Invoke(Read<OrderDto>(body));
                    break;
                case Protocol.ItemStatus:
                    ItemStatusChanged?.Invoke(Read<ItemStatusChangedEvent>(body));
                    break;
                case Protocol.TableStatus:
                    TableStatusChanged?.Invoke(Read<TableChangedEvent>(body));
                    break;
                case Protocol.Tables:
                    TablesLoaded?.Invoke(Read<List<TableDto>>(body));
                    break;
                case Protocol.Orders:
                    OrdersLoaded?.Invoke(Read<List<OrderDto>>(body));
                    break;
                case Protocol.Error:
                    ServerError?.Invoke(Read<ErrorResponse>(body).Message);
                    break;
                case Protocol.Ack:
                    Complete(body, ok: true);
                    break;
                case Protocol.Nak:
                    Complete(body, ok: false);
                    break;
                case Protocol.Retry:
                    if (Guid.TryParseExact(body, "N", out var retryId) && _pending.TryGetValue(retryId, out var r))
                        r.TrySetResult((false, "Server đang lỗi, hãy thử lại."));
                    break;
            }
        }
        catch (JsonException ex)
        {
            Debug.WriteLine($"[RealtimeConnection] Dữ liệu sai từ Server: {ex.Message}");
        }
    }

    private void Complete(string body, bool ok)
    {
        string[] parts = body.Split(Protocol.Separator, 2);
        if (!Guid.TryParseExact(parts[0], "N", out var id) || !_pending.TryGetValue(id, out var reply)) return;

        reply.TrySetResult((ok, ok || parts.Length < 2 ? null : Read<ErrorResponse>(parts[1]).Message));
    }

    private static T Read<T>(string json)
        => JsonSerializer.Deserialize<T>(json, JsonDefaults.Options)
           ?? throw new JsonException("Dữ liệu rỗng.");

    public void Dispose()
    {
        _socket.StateChanged -= OnStateChanged;
        _socket.MessageReceived -= OnMessageReceived;
    }
}