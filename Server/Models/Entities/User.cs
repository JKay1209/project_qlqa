namespace Server.Models.Entities;

public class User
{
    public int UserId { get; set; }
    public string? Email { get; set; }   // Dùng để gửi OTP, lowercase letters
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public bool IsActive { get; set; } = true;

    public Role Role { get; set; } = null!;
}
