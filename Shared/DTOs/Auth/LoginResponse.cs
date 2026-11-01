using Shared.Enums;

namespace Shared.DTOs.Auth;

public sealed record LoginResponse(
    int UserId,
    string Username,
    string DisplayName,
    UserRole Role,
    string AccessToken,
    DateTime ExpiresAt)  
{
    // không in AccessToken
    public override string ToString() => $"LoginResponse {{ UserId = {UserId}, Username = {Username}, Role = {Role} }}";
}