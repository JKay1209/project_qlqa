using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using Server.Config;

namespace Server.Services;

public class EmailService(IOptions<EmailOptions> options, ILogger<EmailService> logger)
{
    private readonly EmailOptions _opt = options.Value;

    public async Task SendOtpAsync(
        string toEmail,
        string displayName,
        string otp,
        int validMinutes)
    {
        // Kiểm tra cấu hình SMTP
        if (string.IsNullOrWhiteSpace(_opt.Host)
            || string.IsNullOrWhiteSpace(_opt.User)
            || string.IsNullOrWhiteSpace(_opt.Password)
            || string.IsNullOrWhiteSpace(_opt.FromAddress))
        {
            throw new InvalidOperationException("Chưa cấu hình SMTP. Hãy kiểm tra User Secrets.");
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

                Subject = "Mã OTP đặt lại mật khẩu",

                IsBodyHtml = true,

                Body = $"""
                    <html>
                    <body>
                        <h3>Xin chào
                        {WebUtility.HtmlEncode(displayName)}</h3>

                        <p>Mã OTP đặt lại mật khẩu của bạn:</p>

                        <h1 style="letter-spacing:5px">
                            {otp}
                        </h1>

                        <p>Mã có hiệu lực trong
                        <b>{validMinutes} phút</b>.</p>

                        <p>Nếu bạn không yêu cầu,
                        hãy bỏ qua email này.</p>
                    </body>
                    </html>
                    """
            };

            message.To.Add(new MailAddress(toEmail));

            await client.SendMailAsync(message);

            logger.LogInformation("Đã gửi OTP tới {Email}", toEmail);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Gửi OTP thất bại tới {Email}", toEmail);

            throw new InvalidOperationException("Không thể gửi OTP. Hãy kiểm tra cấu hình SMTP.", ex);
        }
    }
}
