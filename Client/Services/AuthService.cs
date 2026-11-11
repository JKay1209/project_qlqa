using Client.Network;
using Shared.DTOs.Auth;
using Shared.Enums;
using Shared.Protocol;
using System.Net;

namespace Client.Services;

// Gọi các lệnh xác thực qua TCP. Lỗi do Server báo được đổi thành ApiException
// (mã HTTP tương ứng) để AuthLogic xử lý như khi còn dùng REST.
internal class AuthService
{
    private readonly TcpConnection _conn = TcpConnection.Instance;

    public async Task<LoginResponse> LoginAsync(string username, string password, CancellationToken ct = default)
    {
        var r = await CallAsync(CommandType.Login, new LoginRequest(username, password), ct);
        return r.ReadData<LoginResponse>() ?? throw InvalidReply();
    }

    public async Task LogoutAsync(CancellationToken ct = default)
        => await CallAsync(CommandType.Logout, new { }, ct);

    public async Task RegisterAsync(RegisterRequest request, CancellationToken ct = default)
        => await CallAsync(CommandType.Register, request, ct);

    public async Task ForgotPasswordAsync(string email, CancellationToken ct = default)
        => await CallAsync(CommandType.ForgotPassword, new ForgotPasswordRequest(email), ct);

    public async Task<VerifyOtpResponse> VerifyOtpAsync(string email, string otp, CancellationToken ct = default)
    {
        var r = await CallAsync(CommandType.VerifyOtp, new VerifyOtpRequest(email, otp), ct);
        return r.ReadData<VerifyOtpResponse>() ?? throw InvalidReply();
    }

    public async Task ResetPasswordAsync(string resetToken, string newPassword, CancellationToken ct = default)
        => await CallAsync(CommandType.ResetPassword, new ResetPasswordRequest(resetToken, newPassword), ct);

    // ---------------------------------------------------------------

    private async Task<SocketResponse> CallAsync<T>(CommandType command, T body, CancellationToken ct)
    {
        var r = await _conn.RequestAsync(command, body, ct);
        if (!r.Ok)
            throw new ApiException(ToStatusCode(r.Code), r.Message ?? "Lỗi không xác định.");
        return r;
    }

    private static HttpStatusCode ToStatusCode(ErrorCode code) => code switch
    {
        ErrorCode.InvalidCredentials or ErrorCode.Unauthorized => HttpStatusCode.Unauthorized,
        ErrorCode.AccountLocked or ErrorCode.Forbidden => HttpStatusCode.Forbidden,
        ErrorCode.NotFound => HttpStatusCode.NotFound,
        ErrorCode.InvalidRequest => HttpStatusCode.BadRequest,
        ErrorCode.Conflict => HttpStatusCode.Conflict,
        _ => HttpStatusCode.InternalServerError
    };

    private static ApiException InvalidReply()
        => new(HttpStatusCode.InternalServerError, "Phản hồi từ server không hợp lệ.");
}
