using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server.Network;

// TCP Socket Server nền tảng: Socket -> Bind -> Listen -> Accept.
// Chạy nền cùng ứng dụng Server, lắng nghe ở cổng 9000.
public class TcpSocketServer(ILogger<TcpSocketServer> logger) : BackgroundService
{
    private const int Port = 9000;
    private const int Backlog = 10;     // số kết nối chờ tối đa trong hàng đợi

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // 1. socket(): tạo socket TCP (IPv4, kiểu stream)
        using var listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        // 2. bind(): gắn socket vào mọi địa chỉ IP của máy, cổng 9000
        listener.Bind(new IPEndPoint(IPAddress.Any, Port));

        // 3. listen(): bắt đầu chờ kết nối
        listener.Listen(Backlog);
        logger.LogInformation("Socket server đang lắng nghe ở cổng {Port}", Port);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // 4. accept(): chờ client kết nối, mỗi client xử lý ở một luồng riêng
                Socket client = await listener.AcceptAsync(stoppingToken);
                _ = Task.Run(() => HandleClientAsync(client, stoppingToken), stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Server đang tắt, thoát vòng lặp bình thường
        }
    }

    // Nhận chữ từ client rồi trả lời: "PING" -> "PONG", còn lại -> "ECHO: <nội dung>"
    private async Task HandleClientAsync(Socket client, CancellationToken ct)
    {
        using (client)
        {
            logger.LogInformation("Client kết nối: {Endpoint}", client.RemoteEndPoint);
            var buffer = new byte[1024];

            try
            {
                while (!ct.IsCancellationRequested)
                {
                    int received = await client.ReceiveAsync(buffer, SocketFlags.None, ct);
                    if (received == 0) break;   // client đã đóng kết nối

                    string message = Encoding.UTF8.GetString(buffer, 0, received).Trim();
                    string reply = message.Equals("PING", StringComparison.OrdinalIgnoreCase)
                        ? "PONG"
                        : $"ECHO: {message}";

                    await client.SendAsync(Encoding.UTF8.GetBytes(reply + "\n"), SocketFlags.None, ct);
                }
            }
            catch (OperationCanceledException) { }
            catch (SocketException ex)
            {
                logger.LogWarning("Lỗi socket với client: {Message}", ex.Message);
            }

            logger.LogInformation("Client ngắt kết nối.");
        }
    }
}