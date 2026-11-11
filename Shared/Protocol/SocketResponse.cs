using System.Text.Json;
using Shared.Enums;
using Shared.Serialization;

namespace Shared.Protocol;

/// <summary>Body của gói Response: thay cho mã HTTP + ErrorResponse của REST.</summary>
public sealed record SocketResponse(bool Ok, ErrorCode Code, string? Message, JsonElement? Data)
{
    public static SocketResponse Success(string? message = null)
        => new(true, ErrorCode.None, message, null);

    public static SocketResponse Success<T>(T data)
        => new(true, ErrorCode.None, null, JsonSerializer.SerializeToElement(data, JsonDefaults.Options));

    public static SocketResponse Fail(ErrorCode code, string message)
        => new(false, code, message, null);

    public T? ReadData<T>()
        => Data is { ValueKind: not (JsonValueKind.Null or JsonValueKind.Undefined) } d
            ? d.Deserialize<T>(JsonDefaults.Options)
            : default;
}
