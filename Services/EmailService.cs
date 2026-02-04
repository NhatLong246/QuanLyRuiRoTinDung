using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace QuanLyRuiRoTinDung.Services
{
    public interface IEmailService
    {
        Task<bool> SendPaymentReminderAsync(PaymentReminderEmail reminder);
        Task<bool> SendPaymentLinkAsync(PaymentLinkEmail paymentLink);
        Task<bool> SendPaymentSuccessAsync(PaymentSuccessEmail success);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        private string SmtpServer => _configuration["EmailSettings:SmtpServer"] ?? "smtp.gmail.com";
        private int SmtpPort => int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
        private string SenderEmail => _configuration["EmailSettings:FromEmail"] ?? _configuration["EmailSettings:SenderEmail"] ?? "";
        private string SenderName => _configuration["EmailSettings:FromName"] ?? _configuration["EmailSettings:SenderName"] ?? "Bank CRM";
        private string SenderPassword => _configuration["EmailSettings:SmtpPassword"] ?? _configuration["EmailSettings:SenderPassword"] ?? "";
        private bool EnableSsl => bool.Parse(_configuration["EmailSettings:EnableSsl"] ?? "true");
        private bool TestMode => bool.Parse(_configuration["EmailSettings:TestMode"] ?? "false");

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendPaymentReminderAsync(PaymentReminderEmail reminder)
        {
            try
            {
                var subject = $"[Bank CRM] Nhắc nhở thanh toán kỳ {reminder.KyTraNo} - Khoản vay {reminder.MaKhoanVayCode}";
                
                var body = GeneratePaymentReminderBody(reminder);

                return await SendEmailAsync(reminder.ToEmail, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending payment reminder email to {Email}", reminder.ToEmail);
                return false;
            }
        }

        public async Task<bool> SendPaymentLinkAsync(PaymentLinkEmail paymentLink)
        {
            try
            {
                var subject = $"[Bank CRM] Link thanh toán kỳ {paymentLink.KyTraNo} - Khoản vay {paymentLink.MaKhoanVayCode}";
                
                var body = GeneratePaymentLinkBody(paymentLink);

                return await SendEmailAsync(paymentLink.ToEmail, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending payment link email to {Email}", paymentLink.ToEmail);
                return false;
            }
        }

        public async Task<bool> SendPaymentSuccessAsync(PaymentSuccessEmail success)
        {
            try
            {
                var subject = $"[Bank CRM] Xác nhận thanh toán thành công - Khoản vay {success.MaKhoanVayCode}";
                
                var body = GeneratePaymentSuccessBody(success);

                return await SendEmailAsync(success.ToEmail, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending payment success email to {Email}", success.ToEmail);
                return false;
            }
        }

        private async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                // Chế độ test - chỉ log, không gửi email thật
                if (TestMode)
                {
                    _logger.LogInformation("=== EMAIL TEST MODE ===");
                    _logger.LogInformation("To: {Email}", toEmail);
                    _logger.LogInformation("Subject: {Subject}", subject);
                    _logger.LogInformation("Body length: {Length} characters", body.Length);
                    _logger.LogInformation("Email would be sent in production mode.");
                    _logger.LogInformation("======================");
                    
                    // Trong test mode, coi như gửi thành công
                    return true;
                }

                if (string.IsNullOrEmpty(SenderEmail) || string.IsNullOrEmpty(SenderPassword))
                {
                    _logger.LogWarning("Email settings not configured properly. Please update appsettings.json with valid Gmail credentials.");
                    return false;
                }

                using var client = new SmtpClient(SmtpServer, SmtpPort)
                {
                    Credentials = new NetworkCredential(SenderEmail, SenderPassword),
                    EnableSsl = EnableSsl
                };

                var message = new MailMessage
                {
                    From = new MailAddress(SenderEmail, SenderName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                message.To.Add(toEmail);

                await client.SendMailAsync(message);
                
                _logger.LogInformation("Email sent successfully to {Email}", toEmail);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
                return false;
            }
        }

        private string GeneratePaymentReminderBody(PaymentReminderEmail reminder)
        {
            var daysText = reminder.DaysUntilDue switch
            {
                0 => "<span style='color: #f59e0b; font-weight: bold;'>hôm nay</span>",
                1 => "<span style='color: #f59e0b; font-weight: bold;'>ngày mai</span>",
                < 0 => $"<span style='color: #ef4444; font-weight: bold;'>đã quá hạn {Math.Abs(reminder.DaysUntilDue)} ngày</span>",
                _ => $"còn <span style='color: #10b981; font-weight: bold;'>{reminder.DaysUntilDue} ngày</span>"
            };

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: 'Segoe UI', Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #3b82f6 0%, #1d4ed8 100%); color: white; padding: 30px; text-align: center; border-radius: 12px 12px 0 0; }}
        .header h1 {{ margin: 0; font-size: 24px; }}
        .content {{ background: #f8fafc; padding: 30px; border: 1px solid #e2e8f0; }}
        .info-box {{ background: white; border-radius: 10px; padding: 20px; margin: 20px 0; box-shadow: 0 2px 8px rgba(0,0,0,0.08); }}
        .info-row {{ display: flex; justify-content: space-between; padding: 10px 0; border-bottom: 1px solid #f1f5f9; }}
        .info-row:last-child {{ border-bottom: none; }}
        .info-label {{ color: #64748b; }}
        .info-value {{ font-weight: 600; color: #1e293b; }}
        .amount {{ font-size: 24px; color: #3b82f6; font-weight: 700; }}
        .btn {{ display: inline-block; padding: 14px 28px; background: linear-gradient(135deg, #10b981 0%, #059669 100%); color: white; text-decoration: none; border-radius: 10px; font-weight: 600; margin: 20px 0; }}
        .footer {{ background: #1e293b; color: #94a3b8; padding: 20px; text-align: center; border-radius: 0 0 12px 12px; font-size: 14px; }}
        .warning {{ background: #fef3c7; border-left: 4px solid #f59e0b; padding: 15px; border-radius: 8px; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🔔 Nhắc nhở thanh toán</h1>
            <p style='margin: 10px 0 0 0; opacity: 0.9;'>Bank CRM - Hệ thống quản lý tín dụng</p>
        </div>
        <div class='content'>
            <p>Kính gửi <strong>{reminder.CustomerName}</strong>,</p>
            <p>Chúng tôi xin thông báo kỳ thanh toán của bạn đến hạn {daysText}.</p>
            
            <div class='info-box'>
                <div class='info-row'>
                    <span class='info-label'>Mã khoản vay</span>
                    <span class='info-value'>#{reminder.MaKhoanVayCode}</span>
                </div>
                <div class='info-row'>
                    <span class='info-label'>Kỳ thanh toán</span>
                    <span class='info-value'>Kỳ {reminder.KyTraNo}/{reminder.TongSoKy}</span>
                </div>
                <div class='info-row'>
                    <span class='info-label'>Ngày đến hạn</span>
                    <span class='info-value'>{reminder.NgayTraDuKien:dd/MM/yyyy}</span>
                </div>
                <div class='info-row'>
                    <span class='info-label'>Gốc phải trả</span>
                    <span class='info-value'>{reminder.SoTienGoc:N0} VNĐ</span>
                </div>
                <div class='info-row'>
                    <span class='info-label'>Lãi phải trả</span>
                    <span class='info-value'>{reminder.SoTienLai:N0} VNĐ</span>
                </div>
                <div class='info-row'>
                    <span class='info-label'>Tổng phải trả</span>
                    <span class='amount'>{reminder.TongPhaiTra:N0} VNĐ</span>
                </div>
            </div>

            {(reminder.DaysUntilDue < 0 ? @"
            <div class='warning'>
                ⚠️ <strong>Lưu ý:</strong> Kỳ thanh toán này đã quá hạn. Phí phạt trả chậm sẽ được tính 0.05%/ngày trên số tiền phải trả.
            </div>" : "")}

            <p style='text-align: center;'>
                <a href='{reminder.PaymentUrl}' class='btn'>💳 Thanh toán ngay qua ZaloPay</a>
            </p>

            <p style='color: #64748b; font-size: 14px;'>
                Nếu bạn đã thanh toán, vui lòng bỏ qua email này.<br>
                Mọi thắc mắc xin vui lòng liên hệ hotline: <strong>1900 xxxx</strong>
            </p>
        </div>
        <div class='footer'>
            <p>© 2026 Bank CRM. All rights reserved.</p>
            <p>Email này được gửi tự động, vui lòng không trả lời.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GeneratePaymentLinkBody(PaymentLinkEmail paymentLink)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: 'Segoe UI', Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #8b5cf6 0%, #6d28d9 100%); color: white; padding: 30px; text-align: center; border-radius: 12px 12px 0 0; }}
        .header h1 {{ margin: 0; font-size: 24px; }}
        .content {{ background: #f8fafc; padding: 30px; border: 1px solid #e2e8f0; }}
        .qr-box {{ background: white; border-radius: 16px; padding: 30px; margin: 20px 0; text-align: center; box-shadow: 0 4px 15px rgba(0,0,0,0.1); }}
        .qr-code {{ width: 200px; height: 200px; margin: 20px auto; background: #f1f5f9; border-radius: 12px; display: flex; align-items: center; justify-content: center; }}
        .amount {{ font-size: 28px; color: #8b5cf6; font-weight: 700; margin: 15px 0; }}
        .btn {{ display: inline-block; padding: 14px 28px; background: linear-gradient(135deg, #8b5cf6 0%, #6d28d9 100%); color: white; text-decoration: none; border-radius: 10px; font-weight: 600; margin: 20px 0; }}
        .info-row {{ display: flex; justify-content: space-between; padding: 10px 0; border-bottom: 1px solid #f1f5f9; }}
        .footer {{ background: #1e293b; color: #94a3b8; padding: 20px; text-align: center; border-radius: 0 0 12px 12px; font-size: 14px; }}
        .steps {{ background: white; border-radius: 10px; padding: 20px; margin: 20px 0; }}
        .step {{ display: flex; align-items: center; gap: 15px; padding: 10px 0; }}
        .step-number {{ width: 30px; height: 30px; background: #8b5cf6; color: white; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-weight: bold; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>💳 Link thanh toán ZaloPay</h1>
            <p style='margin: 10px 0 0 0; opacity: 0.9;'>Bank CRM - Hệ thống quản lý tín dụng</p>
        </div>
        <div class='content'>
            <p>Kính gửi <strong>{paymentLink.CustomerName}</strong>,</p>
            <p>Dưới đây là link thanh toán qua ZaloPay cho kỳ {paymentLink.KyTraNo} của khoản vay #{paymentLink.MaKhoanVayCode}.</p>
            
            <div class='qr-box'>
                <h3 style='color: #1e293b; margin: 0 0 10px 0;'>Số tiền thanh toán</h3>
                <div class='amount'>{paymentLink.SoTien:N0} VNĐ</div>
                <p style='color: #64748b; margin: 0;'>Kỳ {paymentLink.KyTraNo} - Hạn: {paymentLink.NgayTraDuKien:dd/MM/yyyy}</p>
                
                <a href='{paymentLink.PaymentUrl}' class='btn'>Thanh toán qua ZaloPay</a>
                
                <p style='color: #94a3b8; font-size: 13px; margin-top: 15px;'>
                    Hoặc quét mã QR trong app ZaloPay
                </p>
            </div>

            <div class='steps'>
                <h4 style='margin: 0 0 15px 0; color: #1e293b;'>📋 Hướng dẫn thanh toán:</h4>
                <div class='step'>
                    <span class='step-number'>1</span>
                    <span>Mở app <strong>ZaloPay</strong> trên điện thoại</span>
                </div>
                <div class='step'>
                    <span class='step-number'>2</span>
                    <span>Nhấn vào nút <strong>Thanh toán qua ZaloPay</strong> ở trên</span>
                </div>
                <div class='step'>
                    <span class='step-number'>3</span>
                    <span>Xác nhận thông tin và hoàn tất thanh toán</span>
                </div>
            </div>

            <p style='color: #64748b; font-size: 14px;'>
                ⏰ Link thanh toán có hiệu lực trong <strong>15 phút</strong>.<br>
                Nếu hết hạn, vui lòng yêu cầu link mới từ hệ thống.
            </p>
        </div>
        <div class='footer'>
            <p>© 2026 Bank CRM. All rights reserved.</p>
            <p>Email này được gửi tự động, vui lòng không trả lời.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GeneratePaymentSuccessBody(PaymentSuccessEmail success)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: 'Segoe UI', Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #10b981 0%, #059669 100%); color: white; padding: 30px; text-align: center; border-radius: 12px 12px 0 0; }}
        .header h1 {{ margin: 0; font-size: 24px; }}
        .content {{ background: #f8fafc; padding: 30px; border: 1px solid #e2e8f0; }}
        .success-icon {{ font-size: 64px; text-align: center; margin: 20px 0; }}
        .info-box {{ background: white; border-radius: 10px; padding: 20px; margin: 20px 0; box-shadow: 0 2px 8px rgba(0,0,0,0.08); }}
        .info-row {{ display: flex; justify-content: space-between; padding: 10px 0; border-bottom: 1px solid #f1f5f9; }}
        .info-row:last-child {{ border-bottom: none; }}
        .info-label {{ color: #64748b; }}
        .info-value {{ font-weight: 600; color: #1e293b; }}
        .amount {{ color: #10b981; font-weight: 700; }}
        .footer {{ background: #1e293b; color: #94a3b8; padding: 20px; text-align: center; border-radius: 0 0 12px 12px; font-size: 14px; }}
        .remaining {{ background: #ecfdf5; border-left: 4px solid #10b981; padding: 15px; border-radius: 8px; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>✅ Thanh toán thành công!</h1>
            <p style='margin: 10px 0 0 0; opacity: 0.9;'>Bank CRM - Hệ thống quản lý tín dụng</p>
        </div>
        <div class='content'>
            <div class='success-icon'>🎉</div>
            <p style='text-align: center; font-size: 18px;'>Kính gửi <strong>{success.CustomerName}</strong>,</p>
            <p style='text-align: center;'>Cảm ơn bạn đã thanh toán đúng hạn. Giao dịch của bạn đã được xử lý thành công.</p>
            
            <div class='info-box'>
                <h4 style='margin: 0 0 15px 0; color: #1e293b;'>📋 Chi tiết giao dịch</h4>
                <div class='info-row'>
                    <span class='info-label'>Mã giao dịch</span>
                    <span class='info-value'>#{success.MaGiaoDich}</span>
                </div>
                <div class='info-row'>
                    <span class='info-label'>Mã khoản vay</span>
                    <span class='info-value'>#{success.MaKhoanVayCode}</span>
                </div>
                <div class='info-row'>
                    <span class='info-label'>Kỳ thanh toán</span>
                    <span class='info-value'>Kỳ {success.KyTraNo}/{success.TongSoKy}</span>
                </div>
                <div class='info-row'>
                    <span class='info-label'>Số tiền đã trả</span>
                    <span class='info-value amount'>{success.SoTienDaTra:N0} VNĐ</span>
                </div>
                <div class='info-row'>
                    <span class='info-label'>Ngày thanh toán</span>
                    <span class='info-value'>{success.NgayThanhToan:dd/MM/yyyy HH:mm}</span>
                </div>
                <div class='info-row'>
                    <span class='info-label'>Phương thức</span>
                    <span class='info-value'>ZaloPay</span>
                </div>
            </div>

            <div class='remaining'>
                <strong>📊 Thông tin khoản vay:</strong><br>
                Dư nợ gốc còn lại: <strong>{success.DuNoConLai:N0} VNĐ</strong><br>
                Số kỳ còn lại: <strong>{success.SoKyConLai} kỳ</strong>
            </div>

            <p style='color: #64748b; font-size: 14px; text-align: center;'>
                Nếu bạn có bất kỳ thắc mắc nào, vui lòng liên hệ hotline: <strong>1900 xxxx</strong>
            </p>
        </div>
        <div class='footer'>
            <p>© 2026 Bank CRM. All rights reserved.</p>
            <p>Email này được gửi tự động, vui lòng không trả lời.</p>
        </div>
    </div>
</body>
</html>";
        }
    }

    // Email model classes
    public class PaymentReminderEmail
    {
        public string ToEmail { get; set; } = "";
        public string CustomerName { get; set; } = "";
        public string MaKhoanVayCode { get; set; } = "";
        public int KyTraNo { get; set; }
        public int TongSoKy { get; set; }
        public DateTime NgayTraDuKien { get; set; }
        public decimal SoTienGoc { get; set; }
        public decimal SoTienLai { get; set; }
        public decimal TongPhaiTra { get; set; }
        public int DaysUntilDue { get; set; }
        public string PaymentUrl { get; set; } = "";
    }

    public class PaymentLinkEmail
    {
        public string ToEmail { get; set; } = "";
        public string CustomerName { get; set; } = "";
        public string MaKhoanVayCode { get; set; } = "";
        public int KyTraNo { get; set; }
        public DateTime NgayTraDuKien { get; set; }
        public decimal SoTien { get; set; }
        public string PaymentUrl { get; set; } = "";
    }

    public class PaymentSuccessEmail
    {
        public string ToEmail { get; set; } = "";
        public string CustomerName { get; set; } = "";
        public string MaGiaoDich { get; set; } = "";
        public string MaKhoanVayCode { get; set; } = "";
        public int KyTraNo { get; set; }
        public int TongSoKy { get; set; }
        public decimal SoTienDaTra { get; set; }
        public DateTime NgayThanhToan { get; set; }
        public decimal DuNoConLai { get; set; }
        public int SoKyConLai { get; set; }
    }
}
