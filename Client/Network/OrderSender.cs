using System.Collections.Concurrent;
using Shared.DTOs.Auth;
using Shared.DTOs.Order;
using Shared.Enums;
using Shared.Protocol;
using Shared.Serialization;
using System.Text.Json;

namespace Client.Network;

public enum SubmitResult
{
    Sent,     // Server đã xác nhận
    Queued,   // Chưa gửi được, đã lưu, sẽ tự gửi bù khi có mạng
    Rejected  // Server từ chối (món hết, sai quyền...), xem OrderRejected
}

// Gửi order qua TCP Socket.
// Luồng: lưu ổ nhớ -> gửi -> chờ ACK -> xóa khỏi ổ nhớ.
// Mất mạng thì order nằm lại trong OrderStore, khi SocketClient nối lại sẽ tự gửi lại.
//
// Định dạng dòng:
//   Client -> Server:  ORDER|{orderId:N}|{json CreateOrderRequest}
//   Server -> Client:  ACK|{orderId:N}                    đã ghi, xóa khỏi hàng đợi
//                      NAK|{orderId:N}|{json lỗi}         từ chối, chuyển sang rejected-orders.json
//                      RETRY|{orderId:N}                  lỗi tạm thời, giữ lại gửi sau
// Chỉ gửi khi kết nối đã xác thực bằng tài khoản được phép gọi món.
//
// Server bỏ qua order có orderId đã xử lý (lost ACK).
internal sealed class OrderSender : IDisposable
{
    private static readonly TimeSpan AckTimeout = TimeSpan.FromSeconds(5);

    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);

    private readonly SocketClient _socket;
    private readonly RealtimeConnection _realtime;
    private readonly OrderStore _store;
    private readonly OrderStore _rejected;
    private readonly ConcurrentDictionary<Guid, TaskCompletionSource<Reply>> _waiting = new();
    private readonly ConcurrentDictionary<Guid, bool> _rejectedNow = new();
    private readonly SemaphoreSlim _flushLock = new(1, 1);   // mỗi lúc chỉ 1 luồng gửi bù -> giữ đúng thứ tự
    private volatile bool _disposed;

    private sealed record Reply(string Command, string? Error);

    public OrderSender(SocketClient socket, RealtimeConnection realtime, OrderStore store, OrderStore rejected)
    {
        _socket = socket;
        _realtime = realtime;
        _store = store;
        _rejected = rejected;
        _realtime.Authenticated += OnAuthenticated;
        _socket.MessageReceived += OnMessageReceived;
    }

    // Order bị Server từ chối (kèm lý do) để giao diện báo cho phục vụ.
    public event Action<CreateOrderRequest, string>? OrderRejected;

    // Số order đang chờ gửi (hiển thị trên giao diện). Gọi từ luồng nền -> dùng BeginInvoke.
    public event Action<int>? PendingCountChanged;

    public int PendingCount => _store.Count;

    // Gọi từ OrderLogic khi bấm "Gửi bếp". Không bao giờ làm mất order.
    public async Task<SubmitResult> SubmitAsync(CreateOrderRequest request, CancellationToken ct = default)
    {
        string json = JsonSerializer.Serialize(request, JsonDefaults.Options);
        var pending = _store.Add(json);   // lưu trước
        PendingCountChanged?.Invoke(_store.Count);

        await FlushAsync(ct);

        if (_rejectedNow.ContainsKey(pending.Id)) return SubmitResult.Rejected;
        return _store.Contains(pending.Id) ? SubmitResult.Queued : SubmitResult.Sent;
    }

    // Gửi lần lượt các order đang chờ. Dừng ở order đầu tiên thất bại để không đảo thứ tự.
    public async Task FlushAsync(CancellationToken ct = default)
    {
        await _flushLock.WaitAsync(ct);
        try
        {
            foreach (var order in _store.Snapshot())
            {
                if (!CanSend) break;
                if (!await TrySendAsync(order, ct)) break;
            }
        }
        finally
        {
            _flushLock.Release();
        }
    }

    private bool CanSend =>
        !_disposed
        && _socket.State == ConnectionState.Connected
        && _realtime.IsAuthenticated
        && _realtime.Role is UserRole.Waiter or UserRole.Admin or UserRole.Manager;

    // true = order đã có kết quả cuối (nhận hoặc từ chối), gửi tiếp order sau.
    private async Task<bool> TrySendAsync(PendingOrder order, CancellationToken ct)
    {
        var waiting = new TaskCompletionSource<Reply>(TaskCreationOptions.RunContinuationsAsynchronously);
        _waiting[order.Id] = waiting;

        try
        {
            if (!await _socket.SendLineAsync($"{Protocol.Order}|{order.Id:N}|{order.PayloadJson}", ct))
                return false;

            Reply reply;
            try
            {
                reply = await waiting.Task.WaitAsync(AckTimeout, ct);
            }
            catch (TimeoutException)
            {
                // Server im lặng: nhiều khả năng kết nối đã chết ngầm -> ép nối lại
                _socket.DropConnection();
                return false;
            }

            if (reply.Command == Protocol.Retry)
            {
                _ = Task.Delay(RetryDelay, CancellationToken.None).ContinueWith(_ => FlushAsync(), TaskScheduler.Default);
                return false;
            }

            if (reply.Command == Protocol.Nak)
                Reject(order, reply.Error ?? "Server từ chối order.");

            _store.Remove(order.Id);
            PendingCountChanged?.Invoke(_store.Count);
            return true;
        }
        finally
        {
            _waiting.TryRemove(order.Id, out _);
        }
    }

    private void Reject(PendingOrder order, string reason)
    {
        _rejectedNow[order.Id] = true;
        _rejected.Add(order.PayloadJson);

        var request = JsonSerializer.Deserialize<CreateOrderRequest>(order.PayloadJson, JsonDefaults.Options);
        if (request is not null)
            OrderRejected?.Invoke(request, reason);
    }

    // ===================== Sự kiện =====================
    // Không await ở đây: handler chạy trên luồng nhận, chờ ACK ở đây sẽ tự khóa chính mình.
    private void OnAuthenticated(UserRole role) => _ = Task.Run(() => FlushAsync());

    private void OnMessageReceived(string line)
    {
        string[] parts = line.Split(Protocol.Separator, 3);
        if (parts.Length < 2 || parts[0] is not (Protocol.Ack or Protocol.Nak or Protocol.Retry))
            return;
        if (!Guid.TryParseExact(parts[1], "N", out var id) || !_waiting.TryGetValue(id, out var waiting))
            return;

        string? error = null;
        if (parts.Length == 3)
        {
            try { error = JsonSerializer.Deserialize<ErrorResponse>(parts[2], JsonDefaults.Options)?.Message; }
            catch (JsonException) { }
        }
        waiting.TrySetResult(new Reply(parts[0], error));
    }

    public void Dispose()
    {
        _disposed = true;   // không Dispose _flushLock: luồng gửi bù có thể còn đang chạy
        _realtime.Authenticated -= OnAuthenticated;
        _socket.MessageReceived -= OnMessageReceived;
    }
}