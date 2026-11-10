namespace Client.Security;

// Giữ Access Token trong bộ nhớ RAM.
public static class TokenManager
{
    private static string? _token;

    public static void SetToken(string token) => _token = token;
    public static string? GetToken() => _token;
    public static void Clear() => _token = null;
}
