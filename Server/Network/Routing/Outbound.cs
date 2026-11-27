using Shared.Enums;

namespace Server.Network.Routing;

// Một dòng cần gửi: cho đúng 1 client (TargetId) hoặc cho mọi client có vai trò thỏa Audience.
public sealed record Outbound(string Line, int? TargetId = null, Func<UserRole, bool>? Audience = null);