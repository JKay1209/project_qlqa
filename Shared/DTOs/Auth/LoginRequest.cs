namespace Shared.DTOs.Auth;

public sealed record LoginRequest(string Username, string Password)
{
    // record mặc định in mọi thuộc tính -> che mật khẩu để không lọt vào log
    public override string ToString() => $"LoginRequest {{ Username = {Username}, Password = *** }}";
}
