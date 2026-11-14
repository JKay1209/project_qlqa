using Microsoft.Extensions.Caching.Memory;

namespace Server.Security;

public class TokenBlacklist(IMemoryCache cache)
{
    public void Revoke(string jti, DateTime expiresAtUtc)
    {
        var ttl = expiresAtUtc - DateTime.UtcNow;
        if (ttl > TimeSpan.Zero)
            cache.Set("revoked:" + jti, true, ttl);   // tự xóa khi token hết hạn
    }

    public bool IsRevoked(string? jti)
        => !string.IsNullOrEmpty(jti) && cache.TryGetValue("revoked:" + jti, out _);
}