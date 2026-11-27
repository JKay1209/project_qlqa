namespace Client.Network;

// Giữ 1 kết nối Socket dùng chung cho cả app
// Địa chỉ Server: biến môi trường QLQA_SERVER_HOST / QLQA_SERVER_PORT,
// mặc định 127.0.0.1:9000 (cổng SocketServer:Port trong appsettings.json).
// UDP Broadcast truyền địa chỉ tìm được vào Start().
internal static class SocketManager
{
    private static SocketClient? _connection;
    private static OrderSender? _orders;
    private static RealtimeConnection? _realtime;

    public static SocketClient Connection =>
        _connection ?? throw new InvalidOperationException("Chưa gọi SocketManager.Start().");

    public static OrderSender Orders =>
        _orders ?? throw new InvalidOperationException("Chưa gọi SocketManager.Start().");

    public static RealtimeConnection Realtime =>
        _realtime ?? throw new InvalidOperationException("Chưa gọi SocketManager.Start().");

    public static void Start(string? host = null, int? port = null)
    {
        if (_connection is not null) return;

        var options = new SocketClientOptions
        {
            Host = host
                ?? Environment.GetEnvironmentVariable("QLQA_SERVER_HOST")
                ?? "127.0.0.1",
            Port = port
                ?? (int.TryParse(Environment.GetEnvironmentVariable("QLQA_SERVER_PORT"), out int p) ? p : 9000)
        };

        _connection = new SocketClient(options);
        _realtime = new RealtimeConnection(_connection);
        _orders = new OrderSender(_connection, _realtime, new OrderStore(), new OrderStore(RejectedOrdersPath));
        _connection.Start();
    }

    // Order bị Server từ chối được giữ lại ở đây, không xóa im lặng
    private static string RejectedOrdersPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "QLQA", "rejected-orders.json");

    public static async Task StopAsync()
    {
        _orders?.Dispose();
        _realtime?.Dispose();
        if (_connection is not null)
            await _connection.StopAsync();

        _orders = null;
        _realtime = null;
        _connection = null;
    }
}