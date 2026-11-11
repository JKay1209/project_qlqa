namespace Server.Network;

// Ánh xạ mục "Socket" trong appsettings.json
public class SocketServerOptions
{
    public const string SectionName = "Socket";

    public int Port { get; set; } = 9000;
    public int Backlog { get; set; } = 10;                  // số kết nối chờ tối đa trong hàng đợi listen()
    public int MaxPayloadBytes { get; set; } = 1024 * 1024; // gói lớn hơn sẽ bị từ chối và ngắt kết nối
}
