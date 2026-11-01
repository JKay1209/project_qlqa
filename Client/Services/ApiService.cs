using Client.Network;
using Client.Security;
using Shared.Serialization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Client.Services;

// Dịch vụ REST tổng quát: gửi JSON, đính Bearer token, thông báo exception rõ nghĩa.
public class ApiService
{
    private static readonly JsonSerializerOptions Json = JsonDefaults.Options;

    public async Task<TRes> PostAsync<TReq, TRes>(string route, TReq body, CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, route)
        {
            Content = JsonContent.Create(body, options: Json)
        };
        return await SendAsync<TRes>(request, ct);
    }

    public async Task<TRes> GetAsync<TRes>(string route, CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, route);
        return await SendAsync<TRes>(request, ct);
    }

    private static async Task<TRes> SendAsync<TRes>(HttpRequestMessage request, CancellationToken ct)
    {
        string? token = TokenManager.GetToken();
        if (token is not null)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage response;
        try
        {
            response = await HttpClientManager.Instance.SendAsync(request, ct);
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            throw new ApiTimeoutException();
        }
        catch (HttpRequestException ex)
        {
            throw new ApiConnectionException(ex);
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
            {
                string message = await ReadErrorMessageAsync(response, ct)
                                 ?? $"Lỗi từ server ({(int)response.StatusCode}).";
                throw new ApiException(response.StatusCode, message);
            }

            var result = await response.Content.ReadFromJsonAsync<TRes>(Json, ct);
            return result ?? throw new ApiException(response.StatusCode, "Server trả dữ liệu rỗng.");
        }
    }

    private static async Task<string?> ReadErrorMessageAsync(HttpResponseMessage response, CancellationToken ct)
    {
        try
        {
            var err = await response.Content.ReadFromJsonAsync<ErrorResponse>(Json, ct);
            return err?.Message;
        }
        catch (JsonException) { return null; }
        catch (NotSupportedException) { return null; }
    }
}
