using System.Text.Json;
using Shared.Serialization;

namespace Shared.Protocol;

/// <summary>
/// Một gói tin: Command + RequestId + Payload (JSON UTF-8).
/// RequestId do Client đặt, Server chép lại vào gói trả lời để Client ghép cặp.
/// RequestId = 0 dành cho gói Server chủ động đẩy xuống (realtime).
/// </summary>
public sealed record Packet(CommandType Command, int RequestId, byte[] Payload)
{
    public static Packet Create<T>(CommandType command, int requestId, T body)
        => new(command, requestId, JsonSerializer.SerializeToUtf8Bytes(body, JsonDefaults.Options));

    public static Packet Empty(CommandType command, int requestId = 0)
        => new(command, requestId, []);

    public T? ReadBody<T>()
        => Payload.Length == 0 ? default : JsonSerializer.Deserialize<T>(Payload, JsonDefaults.Options);
}
