using System.Text.Json;
using Server.Services;
using Server.Validation;
using Shared.DTOs.Auth;
using Shared.Enums;
using Shared.Protocol;

namespace Server.Network;

// Định tuyến: Command trong gói tin -> hàm xử lý -> gọi Service nghiệp vụ -> gói Response.
// Đây là "Controller" của tầng TCP. Service/Repository dùng lại nguyên bản, không phụ thuộc HTTP.
public sealed class PacketDispatcher(IServiceScopeFactory scopeFactory, ILogger<PacketDispatcher> logger)
{
    public async Task<Packet> DispatchAsync(ClientSession session, Packet request, CancellationToken ct)
    {
        try
        {
            if (request.Command == CommandType.Ping)
                return Packet.Empty(CommandType.Pong, request.RequestId);

            // Service dạng Scoped (DbContext...) nên mỗi yêu cầu một scope riêng, giống mỗi HTTP request.
            using var scope = scopeFactory.CreateScope();
            var auth = scope.ServiceProvider.GetRequiredService<AuthService>();

            SocketResponse response = request.Command switch
            {
                CommandType.Login => await LoginAsync(session, request, auth, ct),
                CommandType.Logout => Logout(session),
                CommandType.Me => await MeAsync(session, auth, ct),
                CommandType.Register => await RegisterAsync(session, request, auth, ct),
                CommandType.ForgotPassword => await ForgotPasswordAsync(request, auth, ct),
                CommandType.VerifyOtp => await VerifyOtpAsync(request, auth, ct),
                CommandType.ResetPassword => await ResetPasswordAsync(request, auth, ct),
                _ => SocketResponse.Fail(ErrorCode.InvalidRequest, "Lệnh không được hỗ trợ.")
            };

            return Packet.Create(CommandType.Response, request.RequestId, response);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (JsonException)
        {
            return Reply(request, SocketResponse.Fail(ErrorCode.InvalidRequest, "Dữ liệu không hợp lệ."));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Lỗi xử lý lệnh {Command} từ {Endpoint}", request.Command, session.Endpoint);
            return Reply(request, SocketResponse.Fail(ErrorCode.InternalError, "Lỗi hệ thống. Vui lòng thử lại sau."));
        }
    }

    private static Packet Reply(Packet request, SocketResponse response)
        => Packet.Create(CommandType.Response, request.RequestId, response);

    // ============================ Auth ============================

    private static async Task<SocketResponse> LoginAsync(ClientSession session, Packet request, AuthService auth, CancellationToken ct)
    {
        session.User = null;   // đăng nhập lại (kể cả thất bại) thì phiên cũ không còn hiệu lực

        var req = request.ReadBody<LoginRequest>();
        if (req is null || string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrEmpty(req.Password))
            return SocketResponse.Fail(ErrorCode.InvalidRequest, "Vui lòng nhập tài khoản và mật khẩu.");

        if (req.Username.Length > 50 || req.Password.Length > 128)
            return SocketResponse.Fail(ErrorCode.InvalidRequest, "Dữ liệu không hợp lệ.");

        var result = await auth.LoginAsync(req, ct);
        if (result is null)
            return SocketResponse.Fail(ErrorCode.InvalidCredentials, "Sai tài khoản hoặc mật khẩu.");

        session.User = new SessionUser(result.UserId, result.Username, result.Role);
        return SocketResponse.Success(result);
    }

    private static SocketResponse Logout(ClientSession session)
    {
        session.User = null;
        return SocketResponse.Success();
    }

    private static async Task<SocketResponse> MeAsync(ClientSession session, AuthService auth, CancellationToken ct)
    {
        if (session.User is null)
            return SocketResponse.Fail(ErrorCode.Unauthorized, "Chưa đăng nhập.");

        var dto = await auth.GetCurrentUserAsync(session.User.UserId, ct);
        return dto is null
            ? SocketResponse.Fail(ErrorCode.Unauthorized, "Chưa đăng nhập.")
            : SocketResponse.Success(dto);
    }

    // Chỉ Admin. Quyền lấy từ phiên đăng nhập trên kết nối, Client không tự khai được.
    private static async Task<SocketResponse> RegisterAsync(ClientSession session, Packet request, AuthService auth, CancellationToken ct)
    {
        if (session.User is null)
            return SocketResponse.Fail(ErrorCode.Unauthorized, "Chưa đăng nhập.");
        if (session.User.Role != UserRole.Admin)
            return SocketResponse.Fail(ErrorCode.Forbidden, "Bạn không có quyền tạo tài khoản.");

        var req = request.ReadBody<RegisterRequest>();
        if (req is null)
            return SocketResponse.Fail(ErrorCode.InvalidRequest, "Dữ liệu không hợp lệ.");

        string? error = AuthValidator.ValidateRegister(req);
        if (error is not null)
            return SocketResponse.Fail(ErrorCode.InvalidRequest, error);

        var (user, code) = await auth.RegisterAsync(req, ct);
        return code switch
        {
            ErrorCode.None => SocketResponse.Success(user!),
            ErrorCode.Conflict => SocketResponse.Fail(ErrorCode.Conflict, "Tên tài khoản đã tồn tại."),
            _ => SocketResponse.Fail(ErrorCode.InternalError, "Không thể tạo tài khoản.")
        };
    }

    // ======================= Quên mật khẩu ========================

    private static async Task<SocketResponse> ForgotPasswordAsync(Packet request, AuthService auth, CancellationToken ct)
    {
        var req = request.ReadBody<ForgotPasswordRequest>();
        if (req is null || !AuthValidator.IsValidEmail(req.Email))
            return SocketResponse.Fail(ErrorCode.InvalidRequest, "Email không hợp lệ.");

        await auth.RequestPasswordResetAsync(req.Email, ct);

        // Thông báo chung, không lộ email nào tồn tại
        return SocketResponse.Success("Nếu email tồn tại trong hệ thống, mã OTP đã được gửi. Mã có hiệu lực 5 phút.");
    }

    private static async Task<SocketResponse> VerifyOtpAsync(Packet request, AuthService auth, CancellationToken ct)
    {
        var req = request.ReadBody<VerifyOtpRequest>();
        if (req is null || !AuthValidator.IsValidEmail(req.Email)
            || req.Otp is not { Length: 6 } || !req.Otp.All(char.IsAsciiDigit))
            return SocketResponse.Fail(ErrorCode.InvalidRequest, "Dữ liệu không hợp lệ.");

        var token = await auth.VerifyOtpAsync(req.Email, req.Otp, ct);
        return token is null
            ? SocketResponse.Fail(ErrorCode.InvalidRequest, "Mã OTP không đúng hoặc đã hết hạn.")
            : SocketResponse.Success(new VerifyOtpResponse(token));
    }

    private static async Task<SocketResponse> ResetPasswordAsync(Packet request, AuthService auth, CancellationToken ct)
    {
        var req = request.ReadBody<ResetPasswordRequest>();
        if (req is null || string.IsNullOrWhiteSpace(req.ResetToken) || req.ResetToken.Length > 100)
            return SocketResponse.Fail(ErrorCode.InvalidRequest, "Dữ liệu không hợp lệ.");

        if (string.IsNullOrEmpty(req.NewPassword) || req.NewPassword.Length < 8 || req.NewPassword.Length > 128)
            return SocketResponse.Fail(ErrorCode.InvalidRequest, "Mật khẩu phải từ 8 đến 128 ký tự.");

        bool ok = await auth.ResetPasswordAsync(req.ResetToken, req.NewPassword, ct);
        return ok
            ? SocketResponse.Success("Đặt lại mật khẩu thành công. Hãy đăng nhập lại.")
            : SocketResponse.Fail(ErrorCode.InvalidRequest, "Phiên đặt lại mật khẩu không hợp lệ hoặc đã hết hạn.");
    }
}
