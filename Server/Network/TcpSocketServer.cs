using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Sockets;

namespace Server.Network;

// TCP Socket Server: Socket -> Bind -> Listen -> Accept (Task 05).
// Chạy nền cùng ứng dụng Server. Mỗi client được giao cho một ClientSession xử lý riêng.
public class TcpSocketServer(
    IOptions<SocketServerOptions> options,
    ClientRegistry clients,
    PacketDispatcher dispatcher,
    ILoggerFactory loggerFactory,
    ILogger<TcpSocketServer> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var opt = options.Value;

        // 1. socket(): tạo socket TCP (IPv4, kiểu stream)
        using var listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        // 2. bind(): gắn socket vào mọi địa chỉ IP của máy, cổng cấu hình trong appsettings (mặc định 9000)
        listener.Bind(new IPEndPoint(IPAddress.Any, opt.Port));

        // 3. listen(): bắt đầu chờ kết nối. Backlog = số kết nối chờ tối đa trong hàng đợi
        listener.Listen(opt.Backlog);
        logger.LogInformation("Socket server đang lắng nghe ở cổng {Port}", opt.Port);

        while (!stoppingToken.IsCancellationRequested)
        {
            Socket client;
            try
            {
                // 4. accept(): chờ client kết nối
                client = await listener.AcceptAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;   // Server đang tắt
            }
            catch (SocketException ex)
            {
                logger.LogWarning("Lỗi khi accept: {Message}", ex.Message);
                continue;
            }

            _ = Task.Run(() => RunClientAsync(client, opt, stoppingToken), stoppingToken);
        }
    }

    private async Task RunClientAsync(Socket socket, SocketServerOptions opt, CancellationToken ct)
    {
        var session = new ClientSession(socket, dispatcher, opt.MaxPayloadBytes, loggerFactory.CreateLogger<ClientSession>());
        clients.Add(session);
        logger.LogInformation("Client kết nối: {Endpoint} (đang có {Count})", session.Endpoint, clients.Count);

        try
        {
            await session.RunAsync(ct);
        }
        finally
        {
            clients.Remove(session);
            await session.DisposeAsync();
            logger.LogInformation("Client ngắt kết nối: {Endpoint} (còn {Count})", session.Endpoint, clients.Count);
        }
    }
}
