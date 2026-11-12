using Microsoft.EntityFrameworkCore;
using Server.Models.Entities;
using Server.Repositories;
using Server.Security;
using Shared.DTOs.Auth;
using Shared.Enums;
using System.Security.Cryptography;
using System.Text;

namespace Server.Services;

public class AuthService(
    EmployeeRepository repo,
    PasswordHasher hasher,
    JwtService jwt,
    EmailService email,                 
    ILogger<AuthService> logger)
{
    private const int OtpMinutes = 5;           
    private const int ResetTokenMinutes = 10;   
    private const int MaxOtpAttempts = 5;       
    private static readonly TimeSpan ResendCooldown = TimeSpan.FromSeconds(60);

    // ======================= Login ================================
    // Trả null nếu đăng nhập thất bại (không phân biệt sai username / sai mật khẩu / bị khóa)
    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        string username = request.Username.Trim();

        var user = await repo.GetByUsernameAsync(username, ct);
        if (user is null)
        {
            hasher.VerifyDummy(request.Password);   // chống dò username qua thời gian phản hồi
            logger.LogWarning("Đăng nhập thất bại: username '{Username}' không tồn tại.", username);
            return null;
        }

        bool passwordOk = hasher.Verify(request.Password, user.PasswordHash);
        if (!passwordOk || !user.IsActive)
        {
            logger.LogWarning("Đăng nhập thất bại: '{Username}' (sai mật khẩu hoặc bị khóa).", username);
            return null;
        }

        var (token, expiresAt) = jwt.GenerateAccessToken(user);
        logger.LogInformation("Đăng nhập thành công: '{Username}'.", username);

        return new LoginResponse(
            user.UserId, user.Username, user.DisplayName, (UserRole)user.RoleId, token, expiresAt);
    }

    public async Task<UserDto?> GetCurrentUserAsync(int userId, CancellationToken ct = default)
    {
        var user = await repo.GetByIdAsync(userId, ct);
        if (user is null || !user.IsActive) return null;
        return new UserDto(user.UserId, user.Username, user.DisplayName, (UserRole)user.RoleId);
    }


    // =============================== Register ==========================
    // Tạo tài khoản mới. Trả (user, None) nếu thành công;
    // (null, Conflict) nếu username đã tồn tại.
    public async Task<(UserDto? User, ErrorCode Error)> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        string username = request.Username.Trim();
        string? emailNorm = string.IsNullOrWhiteSpace(request.Email) ? null : NormalizeEmail(request.Email);

        // Kiem tra username va email trong database
        if (await repo.UsernameExistsAsync(username, ct)
            || (emailNorm is not null && await repo.EmailExistsAsync(emailNorm, ct)))
        {
            logger.LogWarning("Tạo tài khoản thất bại: '{Username}' hoặc email đã tồn tại.", username);
            return (null, ErrorCode.Conflict);
        }

        var user = new User
        {
            Username = username,
            PasswordHash = hasher.Hash(request.Password),
            DisplayName = request.DisplayName.Trim(),
            Email = emailNorm,
            RoleId = (int)request.Role,
            IsActive = true
        };

        // 3. Lưu. Nếu 2 request cùng lúc tạo trùng tên, unique index trong DB sẽ chặn
        try
        {
            await repo.AddAsync(user, ct);
        }
        catch (DbUpdateException)
        {
            if (await repo.UsernameExistsAsync(username, ct)
            || (emailNorm is not null && await repo.EmailExistsAsync(emailNorm, ct)))
                return (null, ErrorCode.Conflict);
            throw;   // lỗi DB khác -> để ExceptionMiddleware xử lý (500)
        }

        logger.LogInformation("Đã tạo tài khoản '{Username}' (Role: {Role}).", username, request.Role);
        return (new UserDto(user.UserId, user.Username, user.DisplayName, request.Role), ErrorCode.None);
    }


    // =============================== Forgot password ================================
    // Bước 1: Tạo OTP và gửi mail
    public async Task RequestPasswordResetAsync(string emailAddress, CancellationToken ct = default)
    {
        string normalized = NormalizeEmail(emailAddress);

        var user = await repo.GetByEmailAsync(normalized, ct);
        if (user is null || !user.IsActive)
        {
            logger.LogWarning("Quên mật khẩu: email '{Email}' không hợp lệ hoặc bị khóa.", normalized);
            return;   // im lặng để không lộ email nào tồn tại
        }

        // Chống spam: đang trong thời gian chờ thì không gửi thêm
        var latest = await repo.GetLatestResetAsync(user.UserId, ct);
        if (latest is not null && DateTime.UtcNow - latest.CreatedAt < ResendCooldown)
            return;

        await repo.InvalidateResetsAsync(user.UserId, ct);   // OTP cũ mất hiệu lực

        string otp = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6"); 
        await repo.AddResetAsync(new PasswordResetRequest
        {
            UserId = user.UserId,
            OtpHash = hasher.Hash(otp),
            ExpiresAt = DateTime.UtcNow.AddMinutes(OtpMinutes),
            CreatedAt = DateTime.UtcNow
        }, ct);

        // Không await: thời gian phản hồi như nhau dù email có tồn tại hay không
        _ = email.SendOtpAsync(user.Email!, user.DisplayName, otp, OtpMinutes);
        logger.LogInformation("Đã tạo OTP đặt lại mật khẩu cho '{Username}'.", user.Username);
    }

    // Bước 2: kiểm tra OTP. Đúng thì trả ResetToken
    public async Task<string?> VerifyOtpAsync(string emailAddress, string otp, CancellationToken ct = default)
    {
        var user = await repo.GetByEmailAsync(NormalizeEmail(emailAddress), ct);
        if (user is null || !user.IsActive)
        {
            hasher.VerifyDummy(otp);   // chống dò qua thời gian phản hồi
            return null;
        }

        var req = await repo.GetLatestResetAsync(user.UserId, ct);
        if (req is null
            || req.ResetTokenHash is not null           // OTP này đã đổi token rồi, không dùng lại
            || req.ExpiresAt < DateTime.UtcNow
            || req.FailedAttempts >= MaxOtpAttempts)
        {
            hasher.VerifyDummy(otp);
            return null;
        }

        if (!hasher.Verify(otp, req.OtpHash))
        {
            req.FailedAttempts++;
            await repo.SaveChangesAsync(ct);
            logger.LogWarning("Sai OTP của '{Username}' (lần {N}).", user.Username, req.FailedAttempts);
            return null;
        }

        // OTP đúng -> cấp token ngẫu nhiên 256 bit; DB chỉ lưu hash
        string token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
                              .Replace('+', '-').Replace('/', '_').TrimEnd('=');
        req.ResetTokenHash = HashToken(token);
        req.ResetTokenExpiresAt = DateTime.UtcNow.AddMinutes(ResetTokenMinutes);
        await repo.SaveChangesAsync(ct);

        return token;
    }

    // Bước 3: dùng ResetToken để đổi mật khẩu. 
    public async Task<bool> ResetPasswordAsync(string resetToken, string newPassword, CancellationToken ct = default)
    {
        var req = await repo.GetResetByTokenHashAsync(HashToken(resetToken), ct);
        if (req is null
            || req.IsUsed
            || req.ResetTokenExpiresAt is null
            || req.ResetTokenExpiresAt < DateTime.UtcNow
            || !req.User.IsActive)
            return false;

        req.User.PasswordHash = hasher.Hash(newPassword);
        req.IsUsed = true;                       // token chỉ dùng 1 lần
        await repo.SaveChangesAsync(ct);         // 1 lần Save = đổi mật khẩu và đánh dấu đã dùng cùng lúc

        logger.LogInformation("Đã đặt lại mật khẩu cho '{Username}'.", req.User.Username);
        return true;
    }

    // Email lưu và so sánh dạng chữ thường, bỏ khoảng trắng
    private static string NormalizeEmail(string e) => e.Trim().ToLowerInvariant();

    // Hash SHA-256 token ngẫu nhiên 
    private static string HashToken(string token)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}