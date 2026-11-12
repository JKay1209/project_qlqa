using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using Server.Config;

namespace Server.Services;

public class EmailService(IOptions<EmailOptions> options, ILogger<EmailService> logger)
{
    private readonly EmailOptions _opt = options.Value;

    public async Task SendOtpAsync(string toEmail, string displayName, string otp, int validMinutes)
    {
        if (string.IsNullOrWhiteSpace(_opt.Host)
            || string.IsNullOrWhiteSpace(_opt.User)
            || string.IsNullOrWhiteSpace(_opt.Password)
            || string.IsNullOrWhiteSpace(_opt.FromAddress))
        {
            throw new InvalidOperationException(
                "Chưa cấu hình SMTP trong appsettings hoặc User Secrets.");
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
                        <h3>Xin chào {WebUtility.HtmlEncode(displayName)},</h3>

                        <p>Bạn vừa yêu cầu đặt lại mật khẩu.</p>

                        <p>Mã OTP của bạn là:</p>

                        <h1 style="letter-spacing: 5px">
                            {otp}
                        </h1>

                        <p>Mã OTP có hiệu lực trong
                        <b>{validMinutes} phút</b>.</p>

                        <p>Nếu bạn không thực hiện yêu cầu này,
                        hãy bỏ qua email.</p>
                    </body>
                    </html>
                    """
            };

            // Địa chỉ nhận OTP của người dùng
            message.To.Add(new MailAddress(toEmail));

            await client.SendMailAsync(message);

            logger.LogInformation(
                "Đã gửi email OTP tới {Email}.",
                toEmail);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Gửi email OTP thất bại tới {Email}.", toEmail);

            // Không nuốt lỗi: để AuthService biết gửi thất bại
            throw new InvalidOperationException( "Không thể gửi email OTP. Vui lòng kiểm tra cấu hình SMTP.", ex);
        }
    }
}