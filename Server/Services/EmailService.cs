using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using Server.Config;

namespace Server.Services;

public class EmailService(IOptions<EmailOptions> options, ILogger<EmailService> logger)
{
    private readonly EmailOptions _opt = options.Value;

<<<<<<< HEAD
=======
    // Gửi OTP và báo lỗi cho AuthService nếu SMTP chưa cấu hình hoặc gửi thất bại.
>>>>>>> e24f0db0665e7a926e3e0a18d96f1545dd0d9c5b
    public async Task SendOtpAsync(string toEmail, string displayName, string otp, int validMinutes)
    {
        if (string.IsNullOrWhiteSpace(_opt.Host)
            || string.IsNullOrWhiteSpace(_opt.User)
            || string.IsNullOrWhiteSpace(_opt.Password)
            || string.IsNullOrWhiteSpace(_opt.FromAddress))
        {
            throw new InvalidOperationException(
<<<<<<< HEAD
                "Chưa cấu hình SMTP trong appsettings hoặc User Secrets.");
=======
                "Chưa cấu hình SMTP. Hãy thiết lập Email:Host, Email:User, Email:Password và Email:FromAddress.");
>>>>>>> e24f0db0665e7a926e3e0a18d96f1545dd0d9c5b
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
<<<<<<< HEAD
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
=======
                    <p>Xin chào <b>{WebUtility.HtmlEncode(displayName)}</b>,</p>
                    <p>Mã OTP đặt lại mật khẩu của bạn là:</p>
                    <h2 style="letter-spacing:4px">{otp}</h2>
                    <p>Mã có hiệu lực trong {validMinutes} phút.</p>
                    <p>Nếu bạn không yêu cầu đặt lại mật khẩu, hãy bỏ qua email này.</p>
>>>>>>> e24f0db0665e7a926e3e0a18d96f1545dd0d9c5b
                    """
            };

            // Địa chỉ nhận OTP của người dùng
            message.To.Add(new MailAddress(toEmail));

            await client.SendMailAsync(message);
<<<<<<< HEAD

            logger.LogInformation(
                "Đã gửi email OTP tới {Email}.",
                toEmail);
=======
            logger.LogInformation("Đã gửi email OTP tới {Email}.", toEmail);
>>>>>>> e24f0db0665e7a926e3e0a18d96f1545dd0d9c5b
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Gửi email OTP thất bại tới {Email}.", toEmail);
<<<<<<< HEAD

            // Không nuốt lỗi: để AuthService biết gửi thất bại
            throw new InvalidOperationException( "Không thể gửi email OTP. Vui lòng kiểm tra cấu hình SMTP.", ex);
=======
            throw new InvalidOperationException(
                "Không thể gửi email OTP. Hãy kiểm tra cấu hình SMTP và thử lại.", ex);
>>>>>>> e24f0db0665e7a926e3e0a18d96f1545dd0d9c5b
        }
    }
}
