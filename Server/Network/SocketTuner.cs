using System.Net.Sockets;
using Server.Config;

namespace Server.Network;

// Gom việc cấu hình Socket Option vào một chỗ.
public static class SocketTuner
{
    // Phải gọi TRƯỚC Bind().
    public static void ConfigureListener(Socket listener, SocketServerOptions o, ILogger logger)
    {
        if (o.ReuseAddress)
        {
            // SO_REUSEADDR: bind lại cổng ngay khi restart Server (không bị kẹt TIME_WAIT)
            listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            logger.LogInformation("Đã bật SO_REUSEADDR cho listener.");
        }
    }

    // Gọi ngay sau Accept() cho từng client.
    public static void ConfigureClient(Socket client, SocketServerOptions o, ILogger logger)
    {
        if (o.NoDelay)
            client.NoDelay = true;

        if (o.KeepAlive)
            EnableKeepAlive(client, o, logger);
    }

    private static void EnableKeepAlive(Socket client, SocketServerOptions o, ILogger logger)
    {
        // SO_KEEPALIVE: HĐH tự thăm dò kết nối im lặng, phát hiện client rút mạng/tắt máy đột ngột
        client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

        try
        {
            // Tinh chỉnh thời gian (Windows 10 1703+ / Linux). Hệ cũ không hỗ trợ thì dùng mặc định HĐH.
            client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, o.KeepAliveTimeSeconds);
            client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, o.KeepAliveIntervalSeconds);
            client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount, o.KeepAliveRetryCount);
        }
        catch (Exception ex) when (ex is SocketException or PlatformNotSupportedException or ArgumentException)
        {
            logger.LogDebug("Không chỉnh được thông số KeepAlive, dùng mặc định HĐH: {Message}", ex.Message);
        }
    }
}