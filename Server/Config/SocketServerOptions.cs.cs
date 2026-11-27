namespace Server.Config;

// Ánh xạ mục "SocketServer" trong appsettings.json
public class SocketServerOptions
{
    public const string SectionName = "SocketServer";

    public int Port { get; set; } = 9000;
    public int Backlog { get; set; } = 100;                 // hàng đợi kết nối chờ accept (Task 17 sẽ tinh chỉnh)
    
    public int ReceiveBufferBytes { get; set; } = 4096;     // buffer đọc mỗi lần của vòng select
    public int MaxLineBytes { get; set; } = 262_144;        // 1 dòng lệnh dài hơn -> ngắt client
    public int MaxPendingSendBytes { get; set; } = 4_194_304;   // client nhận quá chậm -> ngắt
    public string ProcessedOrderLogPath { get; set; } = "App_Data/processed-orders.log";   // chống trùng order


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
        if (MaxLineBytes < 1024 || MaxPendingSendBytes < MaxLineBytes)
            throw new InvalidOperationException("SocketServer: cần 1024 <= MaxLineBytes <= MaxPendingSendBytes.");
        if (KeepAlive && (KeepAliveTimeSeconds < 1 || KeepAliveIntervalSeconds < 1 || KeepAliveRetryCount < 1))
            throw new InvalidOperationException("Các tham số KeepAlive phải >= 1.");
        if (ShutdownTimeoutSeconds < 0)
            throw new InvalidOperationException("SocketServer:ShutdownTimeoutSeconds phải >= 0.");
    }
}