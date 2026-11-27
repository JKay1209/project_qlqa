namespace Server.Config;

// Ánh xạ mục "SocketServer" trong appsettings.json
public class SocketServerOptions
{
    public const string SectionName = "SocketServer";

    public int Port { get; set; } = 9000;
    public int Backlog { get; set; } = 100;                 // hàng đợi kết nối chờ accept (Task 17 sẽ tinh chỉnh)
    public int ReceiveBufferBytes { get; set; } = 4096;     // buffer nhận cho mỗi client

    // ---- Socket Options ----
    public bool ReuseAddress { get; set; } = true;          // SO_REUSEADDR
    public bool KeepAlive { get; set; } = true;             // SO_KEEPALIVE
    public int KeepAliveTimeSeconds { get; set; } = 5;      // im lặng bao lâu thì bắt đầu gửi probe
    public int KeepAliveIntervalSeconds { get; set; } = 2;  // khoảng cách giữa các probe
    public int KeepAliveRetryCount { get; set; } = 3;       // số probe thất bại thì coi là chết (~11s)
    public bool NoDelay { get; set; } = true;               // TCP_NODELAY: gói order nhỏ gửi đi ngay

    public int ShutdownTimeoutSeconds { get; set; } = 5;    // thời gian chờ các client đóng khi tắt Server

    public void Validate()
    {
        if (Port is < 1 or > 65535)
            throw new InvalidOperationException("SocketServer:Port phải trong khoảng 1–65535.");
        if (Backlog < 1)
            throw new InvalidOperationException("SocketServer:Backlog phải >= 1.");
        if (ReceiveBufferBytes is < 256 or > 1_048_576)
            throw new InvalidOperationException("SocketServer:ReceiveBufferBytes phải trong khoảng 256–1048576.");
        if (KeepAlive && (KeepAliveTimeSeconds < 1 || KeepAliveIntervalSeconds < 1 || KeepAliveRetryCount < 1))
            throw new InvalidOperationException("Các tham số KeepAlive phải >= 1.");
        if (ShutdownTimeoutSeconds < 0)
            throw new InvalidOperationException("SocketServer:ShutdownTimeoutSeconds phải >= 0.");
    }
}