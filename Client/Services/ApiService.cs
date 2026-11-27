using Client.Network;
using Client.Security;
using Shared.DTOs.Auth;
using Shared.Serialization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Client.Services
{
    internal class ApiService
    {
        protected readonly HttpClient client =
            HttpClientManager.Client;

        protected Task<T> PostAsync<T>(
            string url,
            object data,
            CancellationToken ct = default)
            => SendAsync<T>(HttpMethod.Post, url, data, ct);

        protected Task<T> GetAsync<T>(
            string url,
            CancellationToken ct = default)
            => SendAsync<T>(HttpMethod.Get, url, null, ct);

        // Server trả MessageResponse khi xóa/ngắt thành công
        protected Task<MessageResponse> DeleteAsync(
            string url,
            CancellationToken ct = default)
            => SendAsync<MessageResponse>(HttpMethod.Delete, url, null, ct);

        // Gửi request chung: gắn token, đọc lỗi, đổi lỗi mạng thành exception của Client
        private async Task<T> SendAsync<T>(
            HttpMethod method,
            string url,
            object? data,
            CancellationToken ct)
        {
            try
            {
                using var request = new HttpRequestMessage(method, url);

                if (data is not null)
                {
                    string json =
                        JsonSerializer.Serialize(data, JsonDefaults.Options);
                    request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                }

                // Gắn token nếu đã đăng nhập
                string? token = TokenManager.GetToken();
                if (!string.IsNullOrEmpty(token))
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                using HttpResponseMessage response =
                    await client.SendAsync(request, ct);

                string result =
                    await response.Content.ReadAsStringAsync(ct);

                if (!response.IsSuccessStatusCode)
                    throw new ApiException(response.StatusCode, ReadError(result, (int)response.StatusCode));

                return JsonSerializer.Deserialize<T>(result, JsonDefaults.Options)
                    ?? throw new ApiException(response.StatusCode, "Server trả dữ liệu rỗng.");
            }
            catch (HttpRequestException ex)
            {
                throw new ApiConnectionException(ex);
            }
            catch (TaskCanceledException) when (!ct.IsCancellationRequested)
            {
                throw new ApiTimeoutException();
            }
        }

        // Server trả {"message":"..."} khi lỗi
        private static string ReadError(string body, int status)
        {
            try
            {
                var e = JsonSerializer.Deserialize<ErrorResponse>(body, JsonDefaults.Options);
                if (!string.IsNullOrWhiteSpace(e?.Message)) return e.Message;
            }
            catch (JsonException) { }
            return $"Lỗi HTTP {status}";
        }
 
        protected async Task PostAsync(string url, object data)
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
            {
                string error =
                    await response.Content.ReadAsStringAsync();

                throw new Exception(error);
            }
        }
    }
}