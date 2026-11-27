namespace Shared.DTOs.Monitor;

// Trạng thái Socket Server và các Socket Option đang áp dụng.
public sealed record SocketServerStatusDto(
    bool IsListening,
    int Port,
    DateTime? StartedAt,
    int ConnectedClients,
    long TotalAccepted,
    bool ReuseAddress,
    bool KeepAlive,
    int KeepAliveTimeSeconds,
    int KeepAliveIntervalSeconds,
    int KeepAliveRetryCount,
    bool NoDelay
);