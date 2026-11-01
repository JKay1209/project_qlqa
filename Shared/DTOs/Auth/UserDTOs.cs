using Shared.Enums;

namespace Shared.DTOs.Auth
{
    public sealed record UserDto(int UserId, string Username, string DisplayName, UserRole Role);

}
