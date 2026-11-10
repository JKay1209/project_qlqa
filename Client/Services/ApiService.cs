using Client.Network;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Client.Services
{
    internal class ApiService
    {
        protected readonly HttpClient client =
            HttpClientManager.Client;

        protected async Task<T?> PostAsync<T>(
            string url,
            object data)
        {
            string json =
                JsonSerializer.Serialize(data);

            StringContent content =
                new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json");

            HttpResponseMessage response =
                await client.PostAsync(
                    url,
                    content);

            if (!response.IsSuccessStatusCode)
                return default;

            string result =
                await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<T>(
            result,
            new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
                Converters =
                {
                    new JsonStringEnumConverter()
                }
            });
        }
    }
}