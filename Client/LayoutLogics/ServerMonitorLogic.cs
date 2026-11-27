using Client.Network;
using Client.Services;
using Shared.DTOs.Monitor;
using System.Net;

namespace Client.LayoutLogics;

// Logic cho màn hình giám sát Server: Layout chỉ gọi hàm và hiển thị kết quả.
public class ServerMonitorLogic
{
    private readonly ServerMonitorService _service = new();

    public Task<(bool Ok, SocketServerStatusDto? Data, string? Error)> GetStatusAsync(CancellationToken ct = default)
        => RunAsync(() => _service.GetStatusAsync(ct));

    public Task<(bool Ok, List<ConnectedClientDto>? Data, string? Error)> GetClientsAsync(CancellationToken ct = default)
        => RunAsync(() => _service.GetClientsAsync(ct));

    public async Task<(bool Ok, string? Error)> DisconnectClientAsync(int clientId, CancellationToken ct = default)
    {
        var (ok, _, error) = await RunAsync(() => _service.DisconnectClientAsync(clientId, ct));
        return (ok, error);
    }

    // Xử lý lỗi chung, trả (Ok, Data, Error) để Layout không phải try/catch
    private static async Task<(bool Ok, T? Data, string? Error)> RunAsync<T>(Func<Task<T>> action)
    {
        try
        {
            return (true, await action(), null);
        }
        catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
        {
            return (false, default, "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.");
        }
        catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.Forbidden)
        {
            return (false, default, "Bạn không có quyền xem thông tin Server.");
        }
        catch (ApiException ex) { return (false, default, ex.Message); }
        catch (ApiTimeoutException) { return (false, default, "Hết thời gian chờ server."); }
        catch (ApiConnectionException) { return (false, default, "Không kết nối được tới server. Server đã bật chưa?"); }
        catch (OperationCanceledException) { return (false, default, "Đã hủy."); }
    }
}