using Shared.DTOs.Auth;
using Shared.Enums;

namespace Client.Security;

public static class SessionManager
{
    public static int UserId { get; private set; }
    public static string Username { get; private set; } = "";
    public static string DisplayName { get; private set; } = "";
    public static UserRole Role { get; private set; }
    public static DateTime ExpiresAt { get; private set; }
    public static bool IsLoggedIn { get; private set; }

    public static void Start(LoginResponse r)
    {
        UserId = r.UserId;
        Username = r.Username;
        DisplayName = r.DisplayName;
        Role = r.Role;
        ExpiresAt = r.ExpiresAt;
        IsLoggedIn = true;
    }

    public static void End()
    {
        UserId = 0;
        Username = DisplayName = "";
        Role = default;
        ExpiresAt = default;
        IsLoggedIn = false;
    }
}
