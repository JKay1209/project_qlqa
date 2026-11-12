using Client.Network;
using Client.Security;
using Client.Services;
using Shared.DTOs.Auth;
using Shared.Enums;
using System.Net;

namespace Client.LayoutLogics;

public class AuthLogic
{
    private readonly AuthService _auth = new();

    // ================ Login ==============================
    // Điều phối: gọi service, lưu phiên, thông báo
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


    // ============================ Register ==================
    // Điều phối: kiểm tra quyền phía Client, gọi service, thông báo.

    // ============================ Register ==================
    // Người chưa đăng nhập được đăng ký công khai.
    // Người đăng ký công khai chỉ được cấp vai trò Waiter.
    // Admin đã đăng nhập có thể tạo tài khoản với vai trò được chọn.
    public async Task<(bool Ok, string? Error)> RegisterAsync(
        string username, string password, string displayName, UserRole role,
        string? email = null, CancellationToken ct = default)
    {
        // Không cho người dùng chưa đăng nhập tự chọn quyền cao.
        UserRole effectiveRole =
            SessionManager.IsLoggedIn && SessionManager.Role == UserRole.Admin
                ? role
                : UserRole.Waiter;

        try
        {
            await _auth.RegisterAsync(
                new RegisterRequest(username, password, displayName, effectiveRole, email), ct);

            return (true, null);
        }
        catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
        {
            return (false, "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.");
        }
        catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.Forbidden)
        {
            return (false, "Bạn không có quyền thực hiện thao tác này.");
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



    // ======================== Forgot password ===========================
    private string? _resetToken;   // chỉ nằm trong RAM, không ghi ra đĩa

    // Bước 1: gửi OTP tới email
    public Task<(bool Ok, string? Error)> SendOtpAsync(string email, CancellationToken ct = default)
        => RunAsync(() => _auth.ForgotPasswordAsync(email, ct));

    // Bước 2: xác thực OTP, lưu ResetToken
    public Task<(bool Ok, string? Error)> VerifyOtpAsync(string email, string otp, CancellationToken ct = default)
        => RunAsync(async () =>
        {
            var r = await _auth.VerifyOtpAsync(email, otp, ct);
            _resetToken = r.ResetToken;
        });

    // Bước 3: đổi mật khẩu bằng ResetToken
    public Task<(bool Ok, string? Error)> ResetPasswordAsync(string newPassword, CancellationToken ct = default)
    {
        if (_resetToken is null)
            return Task.FromResult<(bool Ok, string? Error)>((false, "Chưa xác thực OTP."));

        return RunAsync(async () =>
        {
            await _auth.ResetPasswordAsync(_resetToken, newPassword, ct);
            _resetToken = null;   // dùng xong là xóa
        });
    }

    // Xử lý lỗi chung
    private static async Task<(bool Ok, string? Error)> RunAsync(Func<Task> action)
    {
        try
        {
            await action();
            return (true, null);
        }
        catch (ApiException ex) { return (false, ex.Message); }
        catch (ApiTimeoutException) { return (false, "Hết thời gian chờ server."); }
        catch (ApiConnectionException) { return (false, "Không kết nối được tới server. Server đã bật chưa?"); }
        catch (OperationCanceledException) { return (false, "Đã hủy."); }
    }


    public void Logout()
    {
        TokenManager.Clear();
        SessionManager.End();
    }
}
