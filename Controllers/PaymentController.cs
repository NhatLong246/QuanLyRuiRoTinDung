using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyRuiRoTinDung.Models.EF;
using QuanLyRuiRoTinDung.Services;
using System.Text.Json;

namespace QuanLyRuiRoTinDung.Controllers
{
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IZaloPayService _zaloPayService;
        private readonly IEmailService _emailService;
        private readonly ILoanService _loanService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(
            ApplicationDbContext context,
            IZaloPayService zaloPayService,
            IEmailService emailService,
            ILoanService loanService,
            ILogger<PaymentController> logger)
        {
            _context = context;
            _zaloPayService = zaloPayService;
            _emailService = emailService;
            _loanService = loanService;
            _logger = logger;
        }

        // Tạo đơn thanh toán ZaloPay
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateZaloPayOrder([FromBody] CreatePaymentRequest request)
        {
            try
            {
                var lichSu = await _context.LichSuTraNos
                    .Include(l => l.MaKhoanVayNavigation)
                    .FirstOrDefaultAsync(l => l.MaGiaoDich == request.MaGiaoDich);

                if (lichSu == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy kỳ trả nợ" });
                }

                if (lichSu.TrangThai != "Chưa thanh toán")
                {
                    return Json(new { success = false, message = "Kỳ này đã được thanh toán" });
                }

                var khoanVay = lichSu.MaKhoanVayNavigation;
                if (khoanVay == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin khoản vay" });
                }

                // Lấy thông tin khách hàng
                string? customerEmail = null;
                string? customerPhone = null;
                if (khoanVay.LoaiKhachHang == "CaNhan")
                {
                    var khachHang = await _context.KhachHangCaNhans
                        .FirstOrDefaultAsync(k => k.MaKhachHang == khoanVay.MaKhachHang);
                    customerEmail = khachHang?.Email;
                    customerPhone = khachHang?.SoDienThoai;
                }
                else
                {
                    var khachHang = await _context.KhachHangDoanhNghieps
                        .FirstOrDefaultAsync(k => k.MaKhachHang == khoanVay.MaKhachHang);
                    customerEmail = khachHang?.Email;
                    customerPhone = khachHang?.SoDienThoai;
                }

                // Tính số tiền cần thanh toán (bao gồm phí phạt nếu quá hạn)
                decimal soTienTra = lichSu.TongPhaiTra;
                var today = DateOnly.FromDateTime(DateTime.Now);
                if (lichSu.NgayTraDuKien < today)
                {
                    int soNgayTre = today.DayNumber - lichSu.NgayTraDuKien.DayNumber;
                    decimal phiTraCham = lichSu.TongPhaiTra * 0.0005m * soNgayTre;
                    soTienTra += phiTraCham;
                }

                var orderRequest = new ZaloPayOrderRequest
                {
                    MaGiaoDich = lichSu.MaGiaoDich,
                    MaKhoanVay = khoanVay.MaKhoanVay,
                    MaKhoanVayCode = khoanVay.MaKhoanVayCode ?? "",
                    KyTraNo = lichSu.KyTraNo,
                    SoTien = soTienTra,
                    CustomerEmail = customerEmail,
                    CustomerPhone = customerPhone
                };

                var result = await _zaloPayService.CreateOrderAsync(orderRequest);

                if (result.Success)
                {
                    // Lưu AppTransId vào database để tracking
                    lichSu.GhiChu = $"ZaloPay: {result.AppTransId}";
                    await _context.SaveChangesAsync();

                    return Json(new
                    {
                        success = true,
                        orderUrl = result.OrderUrl,
                        appTransId = result.AppTransId,
                        qrCode = result.QrCode,
                        message = "Tạo đơn thanh toán thành công"
                    });
                }

                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating ZaloPay order");
                return Json(new { success = false, message = "Có lỗi xảy ra khi tạo đơn thanh toán" });
            }
        }

        // Callback từ ZaloPay khi thanh toán hoàn tất
        [HttpPost]
        public async Task<IActionResult> ZaloPayCallback()
        {
            try
            {
                using var reader = new StreamReader(Request.Body);
                var body = await reader.ReadToEndAsync();
                
                _logger.LogInformation("ZaloPay Callback received: {Body}", body);

                var callbackData = JsonSerializer.Deserialize<ZaloPayCallbackRequest>(body);
                if (callbackData == null)
                {
                    return Json(new { return_code = -1, return_message = "Invalid callback data" });
                }

                // Validate MAC
                if (!_zaloPayService.ValidateCallback(callbackData.Data ?? "", callbackData.Mac ?? ""))
                {
                    _logger.LogWarning("Invalid MAC in ZaloPay callback");
                    return Json(new { return_code = -1, return_message = "Invalid MAC" });
                }

                // Parse embed_data để lấy thông tin thanh toán
                var data = JsonSerializer.Deserialize<ZaloPayCallbackData>(callbackData.Data ?? "{}");
                if (data == null)
                {
                    return Json(new { return_code = -1, return_message = "Invalid data" });
                }

                var embedData = JsonSerializer.Deserialize<ZaloPayEmbedData>(data.Embed_data ?? "{}");
                if (embedData == null || embedData.MaGiaoDich == 0)
                {
                    return Json(new { return_code = -1, return_message = "Invalid embed data" });
                }

                // Cập nhật thanh toán
                var nguoiGhiNhan = HttpContext.Session.GetInt32("MaNhanVien") ?? 0;
                var result = await _loanService.GhiNhanThanhToanAsync(
                    embedData.MaGiaoDich,
                    data.Amount,
                    nguoiGhiNhan
                );

                if (result.Success)
                {
                    // Gửi email xác nhận
                    await SendPaymentSuccessEmailAsync(embedData.MaGiaoDich);
                    
                    return Json(new { return_code = 1, return_message = "Success" });
                }

                return Json(new { return_code = 0, return_message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing ZaloPay callback");
                return Json(new { return_code = -1, return_message = ex.Message });
            }
        }

        // Redirect sau khi thanh toán thành công
        [HttpGet]
        public async Task<IActionResult> ZaloPaySuccess([FromQuery] string? apptransid)
        {
            if (string.IsNullOrEmpty(apptransid))
            {
                return RedirectToAction("Manage", "Loan");
            }

            // Query trạng thái đơn hàng
            var queryResult = await _zaloPayService.QueryOrderAsync(apptransid);
            
            ViewBag.AppTransId = apptransid;
            ViewBag.QueryResult = queryResult;

            // Nếu thanh toán thành công (return_code = 1), cập nhật database
            if (queryResult.ReturnCode == 1)
            {
                // Tìm giao dịch theo AppTransId
                var lichSu = await _context.LichSuTraNos
                    .Include(l => l.MaKhoanVayNavigation)
                    .FirstOrDefaultAsync(l => l.GhiChu != null && l.GhiChu.Contains(apptransid));

                if (lichSu != null && lichSu.TrangThai == "Chưa thanh toán")
                {
                    var nguoiGhiNhan = HttpContext.Session.GetInt32("MaNhanVien") ?? 0;
                    var result = await _loanService.GhiNhanThanhToanAsync(
                        lichSu.MaGiaoDich,
                        queryResult.Amount,
                        nguoiGhiNhan
                    );

                    if (result.Success)
                    {
                        // Gửi email xác nhận
                        await SendPaymentSuccessEmailAsync(lichSu.MaGiaoDich);
                    }

                    ViewBag.PaymentResult = result;
                    ViewBag.LichSu = lichSu;
                }
            }

            return View();
        }

        // Gửi email thông báo thanh toán (thủ công)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendPaymentNotification([FromBody] SendNotificationRequest request)
        {
            try
            {
                var lichSu = await _context.LichSuTraNos
                    .Include(l => l.MaKhoanVayNavigation)
                    .FirstOrDefaultAsync(l => l.MaGiaoDich == request.MaGiaoDich);

                if (lichSu == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy kỳ trả nợ" });
                }

                if (lichSu.TrangThai != "Chưa thanh toán")
                {
                    return Json(new { success = false, message = "Kỳ này đã được thanh toán" });
                }

                var khoanVay = lichSu.MaKhoanVayNavigation;
                if (khoanVay == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin khoản vay" });
                }

                // Lấy thông tin khách hàng
                string customerEmail = "";
                string customerName = "";
                
                if (khoanVay.LoaiKhachHang == "CaNhan")
                {
                    var khachHang = await _context.KhachHangCaNhans
                        .FirstOrDefaultAsync(k => k.MaKhachHang == khoanVay.MaKhachHang);
                    customerEmail = khachHang?.Email ?? "";
                    customerName = khachHang?.HoTen ?? "";
                }
                else
                {
                    var khachHang = await _context.KhachHangDoanhNghieps
                        .FirstOrDefaultAsync(k => k.MaKhachHang == khoanVay.MaKhachHang);
                    customerEmail = khachHang?.Email ?? "";
                    customerName = khachHang?.TenCongTy ?? "";
                }

                if (string.IsNullOrEmpty(customerEmail))
                {
                    return Json(new { success = false, message = "Khách hàng chưa có email" });
                }

                // Tạo đơn ZaloPay để lấy payment link
                var orderRequest = new ZaloPayOrderRequest
                {
                    MaGiaoDich = lichSu.MaGiaoDich,
                    MaKhoanVay = khoanVay.MaKhoanVay,
                    MaKhoanVayCode = khoanVay.MaKhoanVayCode ?? "",
                    KyTraNo = lichSu.KyTraNo,
                    SoTien = lichSu.TongPhaiTra
                };

                var orderResult = await _zaloPayService.CreateOrderAsync(orderRequest);

                if (!orderResult.Success)
                {
                    return Json(new { success = false, message = $"Không thể tạo link thanh toán: {orderResult.Message}" });
                }

                // Lưu AppTransId
                lichSu.GhiChu = $"ZaloPay: {orderResult.AppTransId}";
                await _context.SaveChangesAsync();

                // Đếm tổng số kỳ
                var tongSoKy = await _context.LichSuTraNos
                    .Where(l => l.MaKhoanVay == khoanVay.MaKhoanVay)
                    .CountAsync();

                // Tính số ngày còn lại
                var today = DateTime.Today;
                var dueDate = lichSu.NgayTraDuKien.ToDateTime(TimeOnly.MinValue);
                var daysUntilDue = (int)(dueDate - today).TotalDays;

                // Gửi email với payment link
                var emailData = new PaymentLinkEmail
                {
                    ToEmail = customerEmail,
                    CustomerName = customerName,
                    MaKhoanVayCode = khoanVay.MaKhoanVayCode ?? "",
                    KyTraNo = lichSu.KyTraNo,
                    NgayTraDuKien = dueDate,
                    SoTien = lichSu.TongPhaiTra,
                    PaymentUrl = orderResult.OrderUrl ?? ""
                };

                var emailSent = await _emailService.SendPaymentLinkAsync(emailData);

                if (emailSent)
                {
                    return Json(new
                    {
                        success = true,
                        message = $"Đã gửi email thông báo thanh toán đến {customerEmail}",
                        paymentUrl = orderResult.OrderUrl
                    });
                }

                // Nếu gửi email thất bại, vẫn trả về payment link
                return Json(new
                {
                    success = true,
                    message = "Không thể gửi email nhưng đã tạo link thanh toán",
                    paymentUrl = orderResult.OrderUrl,
                    emailError = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending payment notification");
                return Json(new { success = false, message = "Có lỗi xảy ra khi gửi thông báo" });
            }
        }

        // Kiểm tra trạng thái thanh toán
        [HttpGet]
        public async Task<IActionResult> CheckPaymentStatus(string appTransId)
        {
            try
            {
                var result = await _zaloPayService.QueryOrderAsync(appTransId);
                
                return Json(new
                {
                    success = true,
                    returnCode = result.ReturnCode,
                    returnMessage = result.ReturnMessage,
                    isProcessing = result.IsProcessing,
                    amount = result.Amount,
                    isPaid = result.ReturnCode == 1
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking payment status");
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Trang thanh toán QR
        [HttpGet]
        public async Task<IActionResult> PaymentQR(int maGiaoDich)
        {
            var lichSu = await _context.LichSuTraNos
                .Include(l => l.MaKhoanVayNavigation)
                .FirstOrDefaultAsync(l => l.MaGiaoDich == maGiaoDich);

            if (lichSu == null)
            {
                return NotFound();
            }

            if (lichSu.TrangThai != "Chưa thanh toán")
            {
                TempData["Error"] = "Kỳ này đã được thanh toán";
                return RedirectToAction("LichSuTraNo", "Loan", new { id = lichSu.MaKhoanVay });
            }

            var khoanVay = lichSu.MaKhoanVayNavigation;
            
            // Lấy thông tin khách hàng
            string customerName = "";
            if (khoanVay?.LoaiKhachHang == "CaNhan")
            {
                var khachHang = await _context.KhachHangCaNhans
                    .FirstOrDefaultAsync(k => k.MaKhachHang == khoanVay.MaKhachHang);
                customerName = khachHang?.HoTen ?? "";
            }
            else if (khoanVay != null)
            {
                var khachHang = await _context.KhachHangDoanhNghieps
                    .FirstOrDefaultAsync(k => k.MaKhachHang == khoanVay.MaKhachHang);
                customerName = khachHang?.TenCongTy ?? "";
            }

            // Tính phí phạt nếu quá hạn
            decimal phiPhat = 0;
            int soNgayTre = 0;
            var today = DateOnly.FromDateTime(DateTime.Now);
            if (lichSu.NgayTraDuKien < today)
            {
                soNgayTre = today.DayNumber - lichSu.NgayTraDuKien.DayNumber;
                phiPhat = lichSu.TongPhaiTra * 0.0005m * soNgayTre;
            }

            ViewBag.LichSu = lichSu;
            ViewBag.KhoanVay = khoanVay;
            ViewBag.CustomerName = customerName;
            ViewBag.PhiPhat = phiPhat;
            ViewBag.SoNgayTre = soNgayTre;
            ViewBag.TongThanhToan = lichSu.TongPhaiTra + phiPhat;

            return View();
        }

        // Helper: Gửi email xác nhận thanh toán thành công
        private async Task SendPaymentSuccessEmailAsync(int maGiaoDich)
        {
            try
            {
                var lichSu = await _context.LichSuTraNos
                    .Include(l => l.MaKhoanVayNavigation)
                    .FirstOrDefaultAsync(l => l.MaGiaoDich == maGiaoDich);

                if (lichSu?.MaKhoanVayNavigation == null) return;

                var khoanVay = lichSu.MaKhoanVayNavigation;
                string customerEmail = "";
                string customerName = "";

                if (khoanVay.LoaiKhachHang == "CaNhan")
                {
                    var khachHang = await _context.KhachHangCaNhans
                        .FirstOrDefaultAsync(k => k.MaKhachHang == khoanVay.MaKhachHang);
                    customerEmail = khachHang?.Email ?? "";
                    customerName = khachHang?.HoTen ?? "";
                }
                else
                {
                    var khachHang = await _context.KhachHangDoanhNghieps
                        .FirstOrDefaultAsync(k => k.MaKhachHang == khoanVay.MaKhachHang);
                    customerEmail = khachHang?.Email ?? "";
                    customerName = khachHang?.TenCongTy ?? "";
                }

                if (string.IsNullOrEmpty(customerEmail)) return;

                var tongSoKy = await _context.LichSuTraNos
                    .Where(l => l.MaKhoanVay == khoanVay.MaKhoanVay)
                    .CountAsync();

                var soKyConLai = await _context.LichSuTraNos
                    .Where(l => l.MaKhoanVay == khoanVay.MaKhoanVay && l.TrangThai == "Chưa thanh toán")
                    .CountAsync();

                var emailData = new PaymentSuccessEmail
                {
                    ToEmail = customerEmail,
                    CustomerName = customerName,
                    MaGiaoDich = lichSu.MaGiaoDichCode ?? "",
                    MaKhoanVayCode = khoanVay.MaKhoanVayCode ?? "",
                    KyTraNo = lichSu.KyTraNo,
                    TongSoKy = tongSoKy,
                    SoTienDaTra = lichSu.TongDaTra ?? lichSu.TongPhaiTra,
                    NgayThanhToan = lichSu.NgayTraThucTe ?? DateTime.Now,
                    DuNoConLai = khoanVay.SoDuGocConLai ?? 0,
                    SoKyConLai = soKyConLai
                };

                await _emailService.SendPaymentSuccessAsync(emailData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending payment success email");
            }
        }
    }

    // Request models
    public class CreatePaymentRequest
    {
        public int MaGiaoDich { get; set; }
    }

    public class SendNotificationRequest
    {
        public int MaGiaoDich { get; set; }
    }

    public class ZaloPayCallbackRequest
    {
        public string? Data { get; set; }
        public string? Mac { get; set; }
        public int Type { get; set; }
    }

    public class ZaloPayEmbedData
    {
        public string? Redirecturl { get; set; }
        public int MaGiaoDich { get; set; }
        public int MaKhoanVay { get; set; }
        public int KyTraNo { get; set; }
    }
}
