namespace Shared.DTOs.Monitor;

// hông tin một client đang kết nối vào Socket Server (thời gian theo UTC)
public sealed record ConnectedClientDto(
    int Id,
    string RemoteEndPoint,
    DateTime ConnectedAt,
    DateTime LastActivity,
    long BytesReceived,
    long BytesSent
);
