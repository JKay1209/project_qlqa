namespace Server.Config;

// Ánh xạ mục "SocketServer" trong appsettings.json
public class SocketServerOptions
{
    public const string SectionName = "SocketServer";

    public int Port { get; set; } = 9000;
    public int Backlog { get; set; } = 100;                 // hàng đợi kết nối chờ accept: đủ cho mọi máy nối lại cùng lúc

    // ---- Giới hạn kết nối ----
    public int MaxClients { get; set; } = 100;              // tối đa client toàn Server
    public int MaxClientsPerIp { get; set; } = 10;          // tối đa kết nối trên mỗi IP

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

    // ---- Heartbeat (Task 19) ----
    // Client gửi PING mỗi 5s. Quá IdleTimeoutSeconds không nhận được byte nào từ client => zombie, ngắt.
    // Nên đặt >= 3 lần chu kỳ PING của Client để 1-2 gói bị trễ không làm ngắt oan.
    public int IdleTimeoutSeconds { get; set; } = 15;       // 0 = tắt dọn zombie
    public int IdleSweepSeconds { get; set; } = 1;          // chu kỳ quét zombie

    public void Validate()
    {
        if (Port is < 1 or > 65535)
            throw new InvalidOperationException("SocketServer:Port phải trong khoảng 1–65535.");
        if (Backlog < 1)
            throw new InvalidOperationException("SocketServer:Backlog phải >= 1.");
        if (MaxClients < 1 || MaxClientsPerIp < 1 || MaxClientsPerIp > MaxClients)
            throw new InvalidOperationException("SocketServer: cần 1 <= MaxClientsPerIp <= MaxClients.");
        if (ReceiveBufferBytes is < 256 or > 1_048_576)
            throw new InvalidOperationException("SocketServer:ReceiveBufferBytes phải trong khoảng 256–1048576.");
        if (MaxLineBytes < 1024 || MaxPendingSendBytes < MaxLineBytes)
            throw new InvalidOperationException("SocketServer: cần 1024 <= MaxLineBytes <= MaxPendingSendBytes.");
        if (KeepAlive && (KeepAliveTimeSeconds < 1 || KeepAliveIntervalSeconds < 1 || KeepAliveRetryCount < 1))
            throw new InvalidOperationException("Các tham số KeepAlive phải >= 1.");
        if (ShutdownTimeoutSeconds < 0)
            throw new InvalidOperationException("SocketServer:ShutdownTimeoutSeconds phải >= 0.");
        if (IdleTimeoutSeconds < 0)
            throw new InvalidOperationException("SocketServer:IdleTimeoutSeconds phải >= 0 (0 = tắt).");
        if (IdleTimeoutSeconds > 0 && IdleSweepSeconds < 1)
            throw new InvalidOperationException("SocketServer:IdleSweepSeconds phải >= 1.");
    }
}