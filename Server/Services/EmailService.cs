using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using Server.Config;

namespace Server.Services;

public class EmailService(IOptions<EmailOptions> options, ILogger<EmailService> logger, IHostEnvironment env)
{
    private readonly EmailOptions _opt = options.Value;

    // Gửi OTP và báo lỗi cho AuthService nếu SMTP chưa cấu hình hoặc gửi thất bại.
    public async Task SendOtpAsync(string toEmail, string displayName, string otp, int validMinutes)
    {
        if (string.IsNullOrWhiteSpace(_opt.Host)
            || string.IsNullOrWhiteSpace(_opt.User)
            || string.IsNullOrWhiteSpace(_opt.Password)
            || string.IsNullOrWhiteSpace(_opt.FromAddress))
        {
            throw new InvalidOperationException(
                "Chưa cấu hình SMTP. Hãy thiết lập Email:Host, Email:User, Email:Password và Email:FromAddress.");
        }

        try
        {
            using var client = new SmtpClient(_opt.Host, _opt.Port)
            {
                EnableSsl = _opt.EnableSsl,
                Credentials = new NetworkCredential(_opt.User, _opt.Password),
                Timeout = 15000
            };

            using var message = new MailMessage
            {
                From = new MailAddress(_opt.FromAddress, _opt.FromName),
                Subject = "Mã xác thực đặt lại mật khẩu",
                IsBodyHtml = true,
                Body = $"""
                    <p>Xin chào <b>{WebUtility.HtmlEncode(displayName)}</b>,</p>
                    <p>Mã OTP đặt lại mật khẩu của bạn là:</p>
                    <h2 style="letter-spacing:4px">{otp}</h2>
                    <p>Mã có hiệu lực trong {validMinutes} phút.</p>
                    <p>Nếu bạn không yêu cầu đặt lại mật khẩu, hãy bỏ qua email này.</p>
                    """
            };
            message.To.Add(toEmail);

            await client.SendMailAsync(message);
            logger.LogInformation("Đã gửi email OTP tới {Email}.", toEmail);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Gửi email OTP thất bại tới {Email}.", toEmail);
            throw new InvalidOperationException(
                "Không thể gửi email OTP. Hãy kiểm tra cấu hình SMTP và thử lại.", ex);
        }
    }
}
