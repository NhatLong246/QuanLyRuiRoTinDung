using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QuanLyRuiRoTinDung.Models.EF;

namespace QuanLyRuiRoTinDung.Services
{
    public class PaymentReminderBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PaymentReminderBackgroundService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(6); // Kiểm tra mỗi 6 giờ

        public PaymentReminderBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<PaymentReminderBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Payment Reminder Service is starting.");

            // Đợi 1 phút sau khi app khởi động để đảm bảo các service khác đã sẵn sàng
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await SendPaymentRemindersAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while sending payment reminders.");
                }

                // Đợi đến lần kiểm tra tiếp theo
                await Task.Delay(_checkInterval, stoppingToken);
            }
        }

        private async Task SendPaymentRemindersAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Checking for payment reminders at {Time}", DateTime.Now);

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
            var zaloPayService = scope.ServiceProvider.GetRequiredService<IZaloPayService>();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            var today = DateOnly.FromDateTime(DateTime.Now);
            var tomorrow = today.AddDays(1);

            // Lấy các kỳ trả nợ chưa thanh toán và đến hạn trong ngày mai hoặc hôm nay
            var paymentsDue = await context.LichSuTraNos
                .Include(l => l.MaKhoanVayNavigation)
                .Where(l => l.TrangThai == "Chưa thanh toán" 
                    && (l.NgayTraDuKien == today || l.NgayTraDuKien == tomorrow))
                .ToListAsync(stoppingToken);

            _logger.LogInformation("Found {Count} payments due for reminder", paymentsDue.Count);

            foreach (var payment in paymentsDue)
            {
                if (stoppingToken.IsCancellationRequested) break;

                try
                {
                    var khoanVay = payment.MaKhoanVayNavigation;
                    if (khoanVay == null) continue;

                    // Lấy thông tin khách hàng
                    string customerEmail = "";
                    string customerName = "";

                    if (khoanVay.LoaiKhachHang == "CaNhan")
                    {
                        var khachHang = await context.KhachHangCaNhans
                            .FirstOrDefaultAsync(k => k.MaKhachHang == khoanVay.MaKhachHang, stoppingToken);
                        customerEmail = khachHang?.Email ?? "";
                        customerName = khachHang?.HoTen ?? "";
                    }
                    else
                    {
                        var khachHang = await context.KhachHangDoanhNghieps
                            .FirstOrDefaultAsync(k => k.MaKhachHang == khoanVay.MaKhachHang, stoppingToken);
                        customerEmail = khachHang?.Email ?? "";
                        customerName = khachHang?.TenCongTy ?? "";
                    }

                    if (string.IsNullOrEmpty(customerEmail))
                    {
                        _logger.LogWarning("No email found for customer of loan {LoanCode}", khoanVay.MaKhoanVayCode);
                        continue;
                    }

                    // Kiểm tra xem đã gửi nhắc nợ trong ngày chưa (tránh gửi trùng)
                    if (!string.IsNullOrEmpty(payment.GhiChu) && payment.GhiChu.Contains($"Reminder:{today}"))
                    {
                        _logger.LogInformation("Reminder already sent for payment {PaymentId} today", payment.MaGiaoDich);
                        continue;
                    }

                    // Tạo đơn ZaloPay để lấy payment link
                    var orderRequest = new ZaloPayOrderRequest
                    {
                        MaGiaoDich = payment.MaGiaoDich,
                        MaKhoanVay = khoanVay.MaKhoanVay,
                        MaKhoanVayCode = khoanVay.MaKhoanVayCode ?? "",
                        KyTraNo = payment.KyTraNo,
                        SoTien = payment.TongPhaiTra
                    };

                    var orderResult = await zaloPayService.CreateOrderAsync(orderRequest);

                    // Đếm tổng số kỳ
                    var tongSoKy = await context.LichSuTraNos
                        .Where(l => l.MaKhoanVay == khoanVay.MaKhoanVay)
                        .CountAsync(stoppingToken);

                    // Tính số ngày còn lại
                    var dueDate = payment.NgayTraDuKien.ToDateTime(TimeOnly.MinValue);
                    var daysUntilDue = (int)(dueDate - DateTime.Today).TotalDays;

                    // Gửi email nhắc nợ
                    var reminderEmail = new PaymentReminderEmail
                    {
                        ToEmail = customerEmail,
                        CustomerName = customerName,
                        MaKhoanVayCode = khoanVay.MaKhoanVayCode ?? "",
                        KyTraNo = payment.KyTraNo,
                        TongSoKy = tongSoKy,
                        NgayTraDuKien = dueDate,
                        SoTienGoc = payment.SoTienGocPhaiTra,
                        SoTienLai = payment.SoTienLaiPhaiTra,
                        TongPhaiTra = payment.TongPhaiTra,
                        DaysUntilDue = daysUntilDue,
                        PaymentUrl = orderResult.Success ? orderResult.OrderUrl ?? "" : ""
                    };

                    var emailSent = await emailService.SendPaymentReminderAsync(reminderEmail);

                    if (emailSent)
                    {
                        // Cập nhật ghi chú để đánh dấu đã gửi
                        var existingNote = payment.GhiChu ?? "";
                        payment.GhiChu = $"{existingNote}|Reminder:{today}";
                        
                        if (orderResult.Success)
                        {
                            payment.GhiChu += $"|ZaloPay:{orderResult.AppTransId}";
                        }

                        await context.SaveChangesAsync(stoppingToken);

                        _logger.LogInformation(
                            "Payment reminder sent successfully to {Email} for loan {LoanCode}, period {Period}",
                            customerEmail, khoanVay.MaKhoanVayCode, payment.KyTraNo);
                    }
                    else
                    {
                        _logger.LogWarning(
                            "Failed to send payment reminder to {Email} for loan {LoanCode}",
                            customerEmail, khoanVay.MaKhoanVayCode);
                    }

                    // Đợi 2 giây giữa mỗi email để tránh bị rate limit
                    await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing payment reminder for {PaymentId}", payment.MaGiaoDich);
                }
            }

            _logger.LogInformation("Finished processing payment reminders");
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Payment Reminder Service is stopping.");
            await base.StopAsync(stoppingToken);
        }
    }
}
