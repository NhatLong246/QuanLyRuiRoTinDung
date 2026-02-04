using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace QuanLyRuiRoTinDung.Services
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true);
        Task<bool> SendPasswordResetEmailAsync(string toEmail, string userName, string resetLink);
        Task<bool> SendOtpEmailAsync(string toEmail, string userName, string otpCode);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly string _fromEmail;
        private readonly string _fromName;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            
            _smtpServer = _configuration["EmailSettings:SmtpServer"] ?? "smtp.gmail.com";
            _smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
            _smtpUsername = _configuration["EmailSettings:SmtpUsername"] ?? "";
            _smtpPassword = _configuration["EmailSettings:SmtpPassword"] ?? "";
            _fromEmail = _configuration["EmailSettings:FromEmail"] ?? "";
            _fromName = _configuration["EmailSettings:FromName"] ?? "Hệ thống Quản lý Rủi ro Tín dụng";
            
            // Log configuration status (without sensitive data)
            _logger.LogInformation("Email Service initialized - Server: {Server}, Port: {Port}, FromEmail: {FromEmail}", 
                _smtpServer, _smtpPort, string.IsNullOrEmpty(_fromEmail) ? "NOT SET" : _fromEmail);
            
            if (string.IsNullOrWhiteSpace(_smtpUsername) || string.IsNullOrWhiteSpace(_smtpPassword))
            {
                _logger.LogWarning("Email credentials are missing. SmtpUsername: {HasUsername}, SmtpPassword: {HasPassword}", 
                    !string.IsNullOrWhiteSpace(_smtpUsername), !string.IsNullOrWhiteSpace(_smtpPassword));
            }
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true)
        {
            if (string.IsNullOrWhiteSpace(toEmail))
            {
                _logger.LogWarning("Cannot send email: recipient email is empty");
                return false;
            }

            if (string.IsNullOrWhiteSpace(_smtpUsername) || string.IsNullOrWhiteSpace(_smtpPassword))
            {
                _logger.LogWarning("Email configuration is missing. Please configure EmailSettings in appsettings.json");
                return false;
            }

            try
            {
                using (var client = new SmtpClient(_smtpServer, _smtpPort))
                {
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);
                    client.Timeout = 30000; // 30 seconds

                    using (var message = new MailMessage())
                    {
                        message.From = new MailAddress(_fromEmail, _fromName);
                        message.To.Add(new MailAddress(toEmail));
                        message.Subject = subject;
                        message.Body = body;
                        message.IsBodyHtml = isHtml;

                        await client.SendMailAsync(message);
                        _logger.LogInformation("Email sent successfully to {ToEmail}", toEmail);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {ToEmail}: {ErrorMessage}", toEmail, ex.Message);
                return false;
            }
        }

        public async Task<bool> SendPasswordResetEmailAsync(string toEmail, string userName, string resetLink)
        {
            var subject = "Yêu cầu đặt lại mật khẩu";
            var body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #007bff; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
        .content {{ background-color: #f9f9f9; padding: 30px; border-radius: 0 0 5px 5px; }}
        .button {{ display: inline-block; padding: 12px 30px; background-color: #007bff; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .button:hover {{ background-color: #0056b3; }}
        .footer {{ margin-top: 20px; padding-top: 20px; border-top: 1px solid #ddd; font-size: 12px; color: #666; }}
        .warning {{ background-color: #fff3cd; border-left: 4px solid #ffc107; padding: 10px; margin: 15px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>Hệ thống Quản lý Rủi ro Tín dụng</h2>
        </div>
        <div class='content'>
            <p>Xin chào <strong>{userName}</strong>,</p>
            <p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn.</p>
            <p>Vui lòng click vào nút bên dưới để đặt lại mật khẩu:</p>
            <p style='text-align: center;'>
                <a href='{resetLink}' class='button'>Đặt lại mật khẩu</a>
            </p>
            <p>Hoặc copy và dán link sau vào trình duyệt:</p>
            <p style='word-break: break-all; color: #007bff;'>{resetLink}</p>
            <div class='warning'>
                <strong>Lưu ý:</strong>
                <ul>
                    <li>Link này chỉ có hiệu lực trong 24 giờ.</li>
                    <li>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.</li>
                    <li>Để bảo mật tài khoản, vui lòng không chia sẻ link này với bất kỳ ai.</li>
                </ul>
            </div>
            <p>Trân trọng,<br>Đội ngũ Hệ thống Quản lý Rủi ro Tín dụng</p>
        </div>
        <div class='footer'>
            <p>Email này được gửi tự động, vui lòng không trả lời email này.</p>
            <p>Nếu bạn gặp vấn đề, vui lòng liên hệ bộ phận hỗ trợ kỹ thuật.</p>
        </div>
    </div>
</body>
</html>";

            return await SendEmailAsync(toEmail, subject, body, true);
        }

        public async Task<bool> SendOtpEmailAsync(string toEmail, string userName, string otpCode)
        {
            var subject = "Mã xác nhận đặt lại mật khẩu";
            var body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #007bff; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
        .content {{ background-color: #f9f9f9; padding: 30px; border-radius: 0 0 5px 5px; }}
        .otp-code {{ background-color: #fff; border: 2px solid #007bff; border-radius: 8px; padding: 20px; text-align: center; margin: 20px 0; font-size: 32px; font-weight: bold; letter-spacing: 8px; color: #007bff; }}
        .footer {{ margin-top: 20px; padding-top: 20px; border-top: 1px solid #ddd; font-size: 12px; color: #666; }}
        .warning {{ background-color: #fff3cd; border-left: 4px solid #ffc107; padding: 10px; margin: 15px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>Hệ thống Quản lý Rủi ro Tín dụng</h2>
        </div>
        <div class='content'>
            <p>Xin chào <strong>{userName}</strong>,</p>
            <p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn.</p>
            <p>Mã xác nhận của bạn là:</p>
            <div class='otp-code'>{otpCode}</div>
            <p>Vui lòng nhập mã này vào trang xác nhận để tiếp tục đặt lại mật khẩu.</p>
            <div class='warning'>
                <strong>Lưu ý:</strong>
                <ul>
                    <li>Mã xác nhận chỉ có hiệu lực trong 10 phút.</li>
                    <li>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.</li>
                    <li>Để bảo mật tài khoản, vui lòng không chia sẻ mã này với bất kỳ ai.</li>
                </ul>
            </div>
            <p>Trân trọng,<br>Đội ngũ Hệ thống Quản lý Rủi ro Tín dụng</p>
        </div>
        <div class='footer'>
            <p>Email này được gửi tự động, vui lòng không trả lời email này.</p>
            <p>Nếu bạn gặp vấn đề, vui lòng liên hệ bộ phận hỗ trợ kỹ thuật.</p>
        </div>
    </div>
</body>
</html>";

            return await SendEmailAsync(toEmail, subject, body, true);
        }
    }
}
