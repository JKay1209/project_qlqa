namespace Shared.DTOs.Auth;

// Server trả khi có lỗi (400, 401, 500...). Client đọc Message để hiển thị.
public sealed record ErrorResponse(string Message);