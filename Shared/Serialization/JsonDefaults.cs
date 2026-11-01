using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shared.Serialization;

// Cấu hình JSON dùng chung cho client-server: camelCase, enum gửi dạng chữ
public static class JsonDefaults
{
    public static readonly JsonSerializerOptions Options = Create();

    private static JsonSerializerOptions Create()
    {
        var o = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        o.Converters.Add(new JsonStringEnumConverter());
        return o;
    }
}
