using Shared.Protocol;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace Client.Network;

// Một kết nối TCP duy nhất tới Server, dùng chung cho cả ứng dụng.
//  - Gửi yêu cầu kèm RequestId, chờ gói Response có cùng RequestId (nhiều yêu cầu có thể bay cùng lúc).
//  - Gói Server chủ động đẩy xuống (RequestId = 0) được báo qua sự kiện PacketReceived (realtime).
//  - Tự kết nối ở yêu cầu đầu tiên; mất kết nối thì yêu cầu kế tiếp sẽ kết nối lại.
//  - Lỗi được ném ra dưới dạng ApiConnectionException / ApiTimeoutException / ApiException để AuthLogic xử lý như cũ.
internal sealed class TcpConnection : IDisposable
{
    public static TcpConnection Instance { get; } = new();

    public string Host { get; set; } = "127.0.0.1";
    public int Port { get; set; } = 9000;
    public TimeSpan ConnectTimeout { get; set; } = TimeSpan.FromSeconds(5);
    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(10);

    // Chạy ở luồng nền: muốn cập nhật UI phải dùng Control.Invoke (Task 12).
    public event Action<Packet>? PacketReceived;
    public event Action? Disconnected;

    private readonly object _stateLock = new();
    private readonly SemaphoreSlim _connectLock = new(1, 1);
    private readonly SemaphoreSlim _writeLock = new(1, 1);
    private readonly ConcurrentDictionary<int, TaskCompletionSource<Packet>> _pending = new();
    private int _lastRequestId;
    private TcpClient? _tcp;
    private NetworkStream? _stream;
    private CancellationTokenSource? _loopCts;
    private volatile bool _connected;

    public bool IsConnected => _connected;

    public async Task<SocketResponse> RequestAsync<T>(CommandType command, T body, CancellationToken ct = default)
    {
        await EnsureConnectedAsync(ct);

        int id = NextRequestId();
        var tcs = new TaskCompletionSource<Packet>(TaskCreationOptions.RunContinuationsAsynchronously);
        _pending[id] = tcs;

        try
        {
            NetworkStream stream = _stream ?? throw new ApiConnectionException(new IOException("Chưa kết nối."));

            await _writeLock.WaitAsync(ct);
            try { await PacketCodec.WriteAsync(stream, Packet.Create(command, id, body), ct); }
            finally { _writeLock.Release(); }

            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeout.CancelAfter(RequestTimeout);
            Packet reply = await tcs.Task.WaitAsync(timeout.Token);

            return reply.ReadBody<SocketResponse>()
                   ?? throw new ApiException(HttpStatusCode.InternalServerError, "Phản hồi từ server không hợp lệ.");
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            throw new ApiTimeoutException();
        }
        catch (Exception ex) when (ex is IOException or SocketException)
        {
            throw new ApiConnectionException(ex);
        }
        finally
        {
            _pending.TryRemove(id, out _);
        }
    }

    public void Close()
    {
        NetworkStream? stream;
        lock (_stateLock) stream = _stream;
        if (stream is not null) Drop(stream);
    }

    public void Dispose() => Close();

    // ---------------------------------------------------------------

    private async Task EnsureConnectedAsync(CancellationToken ct)
    {
        if (_connected) return;

        await _connectLock.WaitAsync(ct);
        try
        {
            if (_connected) return;

            var tcp = new TcpClient { NoDelay = true };
            try
            {
                using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
                timeout.CancelAfter(ConnectTimeout);
                await tcp.ConnectAsync(Host, Port, timeout.Token);
            }
            catch (OperationCanceledException) when (!ct.IsCancellationRequested)
            {
                tcp.Dispose();
                throw new ApiTimeoutException();
            }
            catch (SocketException ex)
            {
                tcp.Dispose();
                throw new ApiConnectionException(ex);
            }
            catch
            {
                tcp.Dispose();
                throw;
            }

            tcp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            NetworkStream stream = tcp.GetStream();
            var loopCts = new CancellationTokenSource();

            lock (_stateLock)
            {
                _tcp = tcp;
                _stream = stream;
                _loopCts = loopCts;
                _connected = true;
            }

            _ = Task.Run(() => ReadLoopAsync(stream, loopCts.Token));
        }
        finally
        {
            _connectLock.Release();
        }
    }

    private async Task ReadLoopAsync(NetworkStream stream, CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                Packet? packet = await PacketCodec.ReadAsync(stream, PacketCodec.DefaultMaxPayload, ct);
                if (packet is null) break;   // server đóng kết nối

                if (packet.RequestId != 0 && _pending.TryRemove(packet.RequestId, out var tcs))
                    tcs.TrySetResult(packet);
                else
                    PacketReceived?.Invoke(packet);
            }
        }
        catch (Exception)
        {
            // mất kết nối hoặc gói sai: dọn dẹp ở finally
        }
        finally
        {
            Drop(stream);
        }
    }

    // Đóng kết nối hiện tại và báo lỗi cho mọi yêu cầu đang chờ.
    private void Drop(NetworkStream stream)
    {
        lock (_stateLock)
        {
            if (!ReferenceEquals(_stream, stream)) return;   // kết nối này đã được dọn rồi

            _connected = false;
            _stream = null;
            _loopCts?.Cancel();
            _loopCts = null;
            _tcp?.Dispose();
            _tcp = null;
        }

        foreach (int id in _pending.Keys)
        {
            if (_pending.TryRemove(id, out var tcs))
                tcs.TrySetException(new ApiConnectionException(new IOException("Mất kết nối tới server.")));
        }

        Disconnected?.Invoke();
    }

    private int NextRequestId()
    {
        int id;
        do { id = Interlocked.Increment(ref _lastRequestId); } while (id == 0);   // 0 dành cho gói server đẩy
        return id;
    }
}
