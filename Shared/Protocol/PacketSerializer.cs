using System.Text.Json;
using System.Text.Json.Serialization;

namespace QLQA.Protocol;

/// <summary>DTO <-> byte[] (JSON UTF-8). Tách riêng để sau này đổi sang MessagePack/Protobuf chỉ sửa 1 file.</summary>
public static class PacketSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        // enum gửi dạng chữ ("Cashier") thay vì số -> dễ debug, và không vỡ khi thêm enum giữa chừng
        Converters = { new JsonStringEnumConverter() }
    };

    public static byte[] Serialize<T>(T value) => JsonSerializer.SerializeToUtf8Bytes(value, Options);

    public static T Deserialize<T>(byte[] payload)
        => JsonSerializer.Deserialize<T>(payload, Options)
           ?? throw new InvalidDataException($"Payload rỗng/null, không giải được thành {typeof(T).Name}.");
}
