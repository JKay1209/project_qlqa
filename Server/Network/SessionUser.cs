using Shared.Enums;

namespace Server.Network;

// Người dùng đang đăng nhập trên một kết nối TCP. TCP giữ kết nối nên phiên nằm ngay trên ClientSession.
public sealed record SessionUser(int UserId, string Username, UserRole Role);
