namespace Shared.Protocol;

/// <summary>Kiểu lệnh trong Header của gói tin (2 byte).</summary>
public enum CommandType : ushort
{
    // ---- Điều khiển kết nối ----
    Ping = 1,
    Pong = 2,
    Response = 3,       // Server trả lời một yêu cầu, Body là SocketResponse

    // ---- Xác thực ----
    Login = 10,
    Register = 11,      // chỉ Admin
    Me = 12,
    Logout = 13,

    // ---- Quên mật khẩu ----
    ForgotPassword = 20,
    VerifyOtp = 21,
    ResetPassword = 22,
}
