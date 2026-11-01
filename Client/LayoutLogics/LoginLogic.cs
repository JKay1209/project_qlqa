using Client.Security;
using Client.Services;
using Client.Network;
using System.Net;

namespace Client.LayoutLogics;

// Điều phối đăng nhập: gọi service, lưu phiên, đổi lỗi thành thông báo cho người dùng.
public class LoginLogic
{
    private readonly AuthService _auth = new();

    public async Task<(bool Ok, string? Error)> LoginAsync(string username, string password, CancellationToken ct = default)
    {
        try
        {
            var result = await _auth.LoginAsync(username, password, ct);
            TokenManager.SetToken(result.AccessToken);
            SessionManager.Start(result);
            return (true, null);
        }
        catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
        {
            return (false, "Sai tài khoản hoặc mật khẩu.");
        }
        catch (ApiException ex)
        {
            return (false, ex.Message);
        }
        catch (ApiTimeoutException)
        {
            return (false, "Hết thời gian chờ server.");
        }
        catch (ApiConnectionException)
        {
            return (false, "Không kết nối được tới server. Server đã bật chưa?");
        }
        catch (OperationCanceledException)
        {
            return (false, "Đã hủy.");
        }
    }

    public void Logout()
    {
        TokenManager.Clear();
        SessionManager.End();
    }
}
