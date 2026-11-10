using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using Server.Config;

namespace Server.Services;

public class EmailService(IOptions<EmailOptions> options, ILogger<EmailService> logger, IHostEnvironment env)
{
    private readonly EmailOptions _opt = options.Value;

    // Gửi mail chứa OTP.
    public async Task SendOtpAsync(string toEmail, string displayName, string otp, int validMinutes)
    {
        try
        {
            // Chưa cấu hình SMTP: chỉ khi chạy Development mới in OTP ra log để test
            if (string.IsNullOrWhiteSpace(_opt.Host))
            {
                if (env.IsDevelopment())
                    logger.LogWarning("[DEV] OTP của {Email}: {Otp}", toEmail, otp);
                else
                    logger.LogError("Chưa cấu hình Email:Host, không thể gửi OTP.");
                return;
            }

            using var client = new SmtpClient(_opt.Host, _opt.Port)
            {
                EnableSsl = _opt.EnableSsl,
                Credentials = new NetworkCredential(_opt.User, _opt.Password)
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
                    """
            };
            message.To.Add(toEmail);

            await client.SendMailAsync(message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Gửi email OTP thất bại tới {Email}", toEmail);
        }
    }
}