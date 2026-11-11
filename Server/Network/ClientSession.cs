using System.Net.Sockets;
using Shared.Protocol;

namespace Server.Network;

// Một kết nối TCP tới một client: đọc gói -> chuyển cho PacketDispatcher -> ghi gói trả lời.
// Mọi lỗi của client này được cô lập tại đây, không lan ra các client khác hay làm sập Server.
public sealed class ClientSession : IAsyncDisposable
{
    private readonly NetworkStream _stream;
    private readonly PacketDispatcher _dispatcher;
    private readonly int _maxPayload;
    private readonly ILogger _logger;
    private readonly SemaphoreSlim _writeLock = new(1, 1);   // nhiều luồng cùng ghi vào 1 stream sẽ làm lẫn byte

    public Guid Id { get; } = Guid.NewGuid();
    public string Endpoint { get; }
    public SessionUser? User { get; set; }

    public ClientSession(Socket socket, PacketDispatcher dispatcher, int maxPayload, ILogger logger)
    {
        _stream = new NetworkStream(socket, ownsSocket: true);
        _dispatcher = dispatcher;
        _maxPayload = maxPayload;
        _logger = logger;
        Endpoint = socket.RemoteEndPoint?.ToString() ?? "unknown";
    }

    public async Task RunAsync(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                Packet? request = await PacketCodec.ReadAsync(_stream, _maxPayload, ct);
                if (request is null) break;   // client đóng kết nối

                Packet reply = await _dispatcher.DispatchAsync(this, request, ct);
                await SendAsync(reply, ct);
            }
        }
        catch (PacketException ex)
        {
            _logger.LogWarning("Gói tin sai từ {Endpoint}: {Message}. Ngắt kết nối.", Endpoint, ex.Message);
        }
        catch (Exception ex) when (ex is IOException or SocketException or EndOfStreamException)
        {
            _logger.LogInformation("Client {Endpoint} mất kết nối: {Message}", Endpoint, ex.Message);
        }
        catch (OperationCanceledException)
        {
            // Server đang tắt
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi không xử lý với client {Endpoint}", Endpoint);
        }
    }

    public async Task SendAsync(Packet packet, CancellationToken ct = default)
    {
        await _writeLock.WaitAsync(ct);
        try { await PacketCodec.WriteAsync(_stream, packet, ct); }
        finally { _writeLock.Release(); }
    }

    public ValueTask DisposeAsync()
    {
        _stream.Dispose();   // đóng luôn socket
        return ValueTask.CompletedTask;
    }
}
