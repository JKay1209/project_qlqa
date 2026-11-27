namespace Client.Network;

// Giữ 1 kết nối Socket dùng chung cho cả app
// Địa chỉ Server: biến môi trường QLQA_SERVER_HOST / QLQA_SERVER_PORT,
// mặc định 127.0.0.1:9000 (cổng SocketServer:Port trong appsettings.json).
// UDP Broadcast truyền địa chỉ tìm được vào Start().
internal static class SocketManager
{
    private static SocketClient? _connection;
    private static OrderSender? _orders;

    public static SocketClient Connection =>
        _connection ?? throw new InvalidOperationException("Chưa gọi SocketManager.Start().");

    public static OrderSender Orders =>
        _orders ?? throw new InvalidOperationException("Chưa gọi SocketManager.Start().");

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
        _orders = new OrderSender(_connection, new OrderStore());
        _connection.Start();
    }

    public static async Task StopAsync()
    {
        _orders?.Dispose();
        if (_connection is not null)
            await _connection.StopAsync();

        _orders = null;
        _connection = null;
    }
}