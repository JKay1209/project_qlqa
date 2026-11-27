using System.Collections.Concurrent;
using Shared.DTOs.Order;
using Shared.Serialization;
using System.Text.Json;

namespace Client.Network;

public enum SubmitResult
{
    Sent,     // Server đã xác nhận
    Queued    // Chưa gửi được, đã lưu, sẽ tự gửi bù khi có mạng
}

// Gửi order qua TCP Socket.
// Luồng: lưu ổ nhớ -> gửi -> chờ ACK -> xóa khỏi ổ nhớ.
// Mất mạng thì order nằm lại trong OrderStore, khi SocketClient nối lại sẽ tự gửi lại.
//
// Định dạng dòng:
//   Client -> Server:  ORDER|{orderId:N}|{json CreateOrderRequest}
//   Server -> Client:  ACK|{orderId:N}
//
// Server phải bỏ qua order có orderId đã xử lý (lost ACK).
internal sealed class OrderSender : IDisposable
{
    private static readonly TimeSpan AckTimeout = TimeSpan.FromSeconds(5);

    private readonly SocketClient _socket;
    private readonly OrderStore _store;
    private readonly ConcurrentDictionary<Guid, TaskCompletionSource> _waitingAcks = new();
    private readonly SemaphoreSlim _flushLock = new(1, 1);   // mỗi lúc chỉ 1 luồng gửi bù -> giữ đúng thứ tự

    public OrderSender(SocketClient socket, OrderStore store)
    {
        _socket = socket;
        _store = store;
        _socket.StateChanged += OnStateChanged;
        _socket.MessageReceived += OnMessageReceived;
    }

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
                if (_socket.State != ConnectionState.Connected) break;
                if (!await TrySendAsync(order, ct)) break;
            }
        }
        finally
        {
            _flushLock.Release();
        }
    }

    private async Task<bool> TrySendAsync(PendingOrder order, CancellationToken ct)
    {
        var ack = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _waitingAcks[order.Id] = ack;

        try
        {
            if (!await _socket.SendLineAsync($"ORDER|{order.Id:N}|{order.PayloadJson}", ct))
                return false;

            try
            {
                await ack.Task.WaitAsync(AckTimeout, ct);
            }
            catch (TimeoutException)
            {
                // Server im lặng: nhiều khả năng kết nối đã chết ngầm -> ép nối lại
                _socket.DropConnection();
                return false;
            }

            _store.Remove(order.Id);
            PendingCountChanged?.Invoke(_store.Count);
            return true;
        }
        finally
        {
            _waitingAcks.TryRemove(order.Id, out _);
        }
    }

    // ===================== Sự kiện từ SocketClient =====================
    private void OnStateChanged(ConnectionState state)
    {
        // Không await ở đây: handler chạy trên luồng nhận, chờ ACK ở đây sẽ tự khóa chính mình.
        if (state == ConnectionState.Connected)
            _ = Task.Run(() => FlushAsync());
    }

    private void OnMessageReceived(string line)
    {
        if (TryParseAck(line, out Guid id) && _waitingAcks.TryGetValue(id, out var tcs))
            tcs.TrySetResult();
    }

    private static bool TryParseAck(string line, out Guid id)
    {
        id = default;
        string[] parts = line.Split('|', 3);
        if (parts.Length < 2) return false;

        // TẠM THỜI: Server hiện chỉ "ECHO: <nội dung>" (xem TcpSocketServer.ReceiveLoopAsync),
        // nên coi dòng echo của lệnh ORDER là ACK để test được ngay.
        // Khi Server có packet thật (Task 11/13/18) thì bỏ "ECHO: ORDER".
        bool isAck = parts[0] is "ACK" or "ECHO: ORDER";

        return isAck && Guid.TryParseExact(parts[1], "N", out id);
    }

    public void Dispose()
    {
        _socket.StateChanged -= OnStateChanged;
        _socket.MessageReceived -= OnMessageReceived;
        _flushLock.Dispose();
    }
}