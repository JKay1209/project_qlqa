namespace QLQA.Protocol;

// Một thông điệp hoàn chỉnh: loại + RequestId + payload (JSON UTF-8).
public sealed class Packet
{
    public MessageType Type { get; }
    public int RequestId { get; }
    public byte[] Payload { get; }

    public Packet(MessageType type, int requestId, byte[] payload)
    {
        Type = type;
        RequestId = requestId;
        Payload = payload;
    }

    // Tạo gói từ DTO: tự serialize sang JSON.
    public static Packet Create<T>(MessageType type, T dto, int requestId = 0)
        => new(type, requestId, PacketSerializer.Serialize(dto));

    // Gói không có dữ liệu (Connect, ConnectACK)
    public static Packet Empty(MessageType type, int requestId = 0)
        => new(type, requestId, Array.Empty<byte>());

    // Giải payload thành DTO
    public T GetPayload<T>() => PacketSerializer.Deserialize<T>(Payload);

    public override string ToString() => $"[{Type} id={RequestId} {Payload.Length}B]";
}
