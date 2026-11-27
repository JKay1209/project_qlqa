using Shared.Enums;

namespace Server.Network;

// Người dùng đã xác thực trên một kết nối TCP.
public sealed record SessionUser(int UserId, UserRole Role, string TokenId, DateTime ExpiresAtUtc);