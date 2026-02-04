using Microsoft.EntityFrameworkCore;
using QuanLyRuiRoTinDung.Models.EF;
using QuanLyRuiRoTinDung.Models.Entities;

namespace QuanLyRuiRoTinDung.Services
{
    /// <summary>
    /// Background service kiểm tra và xử lý các khoản vay quá hạn
    /// - Nếu quá hạn > 3 ngày: Trừ điểm CIC, ghi nhận nợ xấu, tăng lãi suất
    /// </summary>
    public class OverduePaymentBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OverduePaymentBackgroundService> _logger;
        
        // Chạy kiểm tra mỗi 6 giờ
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(6);
        
        // Số ngày quá hạn để bắt đầu xử phạt
        private const int OVERDUE_DAYS_THRESHOLD = 3;
        
        // Điểm CIC bị trừ mỗi lần quá hạn
        private const int CIC_PENALTY_POINTS = 50;
        
        // Phần trăm lãi suất tăng thêm khi quá hạn (0.5% mỗi tháng)
        private const decimal INTEREST_PENALTY_RATE = 0.5m;

        public OverduePaymentBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<OverduePaymentBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Overdue Payment Background Service đang khởi động...");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessOverduePaymentsAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Service đang được dừng, đây là hành vi bình thường
                    _logger.LogInformation("Overdue Payment Background Service đang dừng...");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi xử lý các khoản vay quá hạn");
                }

                try
                {
                    await Task.Delay(_checkInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Service đang được dừng
                    break;
                }
            }
            
            _logger.LogInformation("Overdue Payment Background Service đã dừng.");
        }

        private async Task ProcessOverduePaymentsAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var today = DateOnly.FromDateTime(DateTime.Now);
            var thresholdDate = today.AddDays(-OVERDUE_DAYS_THRESHOLD);

            // Tìm các khoản trả nợ chưa thanh toán và đã quá hạn > 3 ngày
            var overduePayments = await context.LichSuTraNos
                .Include(l => l.MaKhoanVayNavigation)
                .Where(l => l.TrangThai == "Chưa thanh toán" 
                         && l.NgayTraDuKien < thresholdDate
                         && (l.GhiChu == null || !l.GhiChu.Contains("[OVERDUE_PROCESSED]")))
                .ToListAsync(stoppingToken);

            _logger.LogInformation($"Tìm thấy {overduePayments.Count} khoản trả nợ quá hạn cần xử lý");

            foreach (var payment in overduePayments)
            {
                try
                {
                    await ProcessSingleOverduePaymentAsync(context, payment, today, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Lỗi xử lý khoản trả nợ {payment.MaGiaoDichCode}");
                }
            }
        }

        private async Task ProcessSingleOverduePaymentAsync(
            ApplicationDbContext context, 
            LichSuTraNo payment,
            DateOnly today,
            CancellationToken stoppingToken)
        {
            var khoanVay = payment.MaKhoanVayNavigation;
            if (khoanVay == null)
            {
                khoanVay = await context.KhoanVays.FindAsync(payment.MaKhoanVay);
                if (khoanVay == null) return;
            }

            // Tính số ngày quá hạn
            int soNgayQuaHan = today.DayNumber - payment.NgayTraDuKien.DayNumber;
            payment.SoNgayTraCham = soNgayQuaHan;
            payment.TrangThai = soNgayQuaHan > 30 ? "Nợ xấu" : "Quá hạn";

            _logger.LogInformation($"Xử lý khoản vay {khoanVay.MaKhoanVayCode}, kỳ {payment.KyTraNo}, quá hạn {soNgayQuaHan} ngày");

            // 1. Cập nhật điểm CIC của khách hàng
            await UpdateCicScoreAsync(context, khoanVay, soNgayQuaHan, stoppingToken);

            // 2. Tăng lãi suất cho các kỳ tiếp theo
            await IncreaseInterestForFuturePaymentsAsync(context, khoanVay, payment.KyTraNo, soNgayQuaHan, stoppingToken);

            // 3. Tính phí trả chậm
            decimal phiTraCham = CalculateLatePaymentFee(payment.TongPhaiTra, soNgayQuaHan);
            payment.SoTienPhiTraCham = phiTraCham;

            // 4. Ghi chú đã xử lý
            payment.GhiChu = $"{payment.GhiChu ?? ""} [OVERDUE_PROCESSED:{DateTime.Now:yyyy-MM-dd}] Quá hạn {soNgayQuaHan} ngày, phí trả chậm: {phiTraCham:N0}đ";

            // 5. Cập nhật trạng thái khoản vay
            if (soNgayQuaHan > 90)
            {
                khoanVay.TrangThaiKhoanVay = "Nợ xấu";
                khoanVay.MucDoRuiRo = "Rất cao";
            }
            else if (soNgayQuaHan > 30)
            {
                khoanVay.TrangThaiKhoanVay = "Quá hạn nghiêm trọng";
                khoanVay.MucDoRuiRo = "Cao";
            }
            else
            {
                khoanVay.TrangThaiKhoanVay = "Quá hạn";
                khoanVay.MucDoRuiRo = "Trung bình";
            }

            await context.SaveChangesAsync(stoppingToken);
            _logger.LogInformation($"Đã xử lý xong khoản vay {khoanVay.MaKhoanVayCode}, kỳ {payment.KyTraNo}");
        }

        /// <summary>
        /// Cập nhật điểm CIC của khách hàng
        /// </summary>
        private async Task UpdateCicScoreAsync(
            ApplicationDbContext context, 
            KhoanVay khoanVay, 
            int soNgayQuaHan,
            CancellationToken stoppingToken)
        {
            ThongTinCic? cicInfo = null;

            if (khoanVay.LoaiKhachHang == "CaNhan")
            {
                var khachHang = await context.KhachHangCaNhans
                    .FirstOrDefaultAsync(k => k.MaKhachHang == khoanVay.MaKhachHang, stoppingToken);
                
                if (khachHang != null)
                {
                    cicInfo = await context.ThongTinCics
                        .FirstOrDefaultAsync(c => c.SoCmndCccd == khachHang.SoCmnd && c.LoaiKhachHang == "Cá nhân", stoppingToken);
                }
            }
            else
            {
                var doanhNghiep = await context.KhachHangDoanhNghieps
                    .FirstOrDefaultAsync(k => k.MaKhachHang == khoanVay.MaKhachHang, stoppingToken);
                
                if (doanhNghiep != null)
                {
                    cicInfo = await context.ThongTinCics
                        .FirstOrDefaultAsync(c => c.MaSoThue == doanhNghiep.MaSoThue && c.LoaiKhachHang == "Doanh nghiệp", stoppingToken);
                }
            }

            if (cicInfo != null)
            {
                // Tính điểm trừ dựa trên số ngày quá hạn
                int diemTru = CIC_PENALTY_POINTS;
                if (soNgayQuaHan > 30) diemTru = 100;
                if (soNgayQuaHan > 60) diemTru = 150;
                if (soNgayQuaHan > 90) diemTru = 200;

                // Trừ điểm CIC
                cicInfo.DiemTinDungCic = Math.Max(0, (cicInfo.DiemTinDungCic ?? 500) - diemTru);

                // Cập nhật xếp hạng tín dụng
                cicInfo.XepHangTinDungCic = GetCicRating(cicInfo.DiemTinDungCic ?? 0);

                // Cập nhật số lần quá hạn
                cicInfo.SoLanQuaHanCic++;
                cicInfo.SoNgayQuaHanToiDaCic = Math.Max(cicInfo.SoNgayQuaHanToiDaCic, soNgayQuaHan);
                cicInfo.NgayQuaHanLanCuoiCic = DateOnly.FromDateTime(DateTime.Now);

                // Cập nhật số khoản vay quá hạn
                cicInfo.SoKhoanVayQuaHanCic++;

                // Nếu quá 90 ngày, đánh dấu nợ xấu
                if (soNgayQuaHan > 90)
                {
                    cicInfo.SoKhoanVayNoXauCic++;
                    cicInfo.SoLanNoXauCic++;
                    cicInfo.NgayNoXauLanCuoiCic = DateOnly.FromDateTime(DateTime.Now);
                    cicInfo.MucDoRuiRo = "Rất cao";
                    cicInfo.KhuyenNghiChoVay = "Từ chối";
                    cicInfo.LyDoKhuyenNghi = $"Khách hàng có nợ xấu, quá hạn {soNgayQuaHan} ngày";
                }
                else if (soNgayQuaHan > 30)
                {
                    cicInfo.MucDoRuiRo = "Cao";
                    cicInfo.KhuyenNghiChoVay = "Cần xem xét";
                    cicInfo.LyDoKhuyenNghi = $"Khách hàng có lịch sử trả nợ chậm {soNgayQuaHan} ngày";
                }
                else
                {
                    cicInfo.MucDoRuiRo = "Trung bình";
                    cicInfo.KhuyenNghiChoVay = "Cần xem xét";
                }

                // Cập nhật tỷ lệ trả nợ đúng hạn
                var totalPayments = cicInfo.TongSoKhoanVayCic > 0 ? cicInfo.TongSoKhoanVayCic : 1;
                cicInfo.TyLeTraNoDungHanCic = Math.Max(0, 100 - (cicInfo.SoLanQuaHanCic * 100m / totalPayments));

                // Cập nhật đánh giá tổng quát
                cicInfo.DanhGiaTongQuat = $"Khách hàng có lịch sử quá hạn {cicInfo.SoLanQuaHanCic} lần. " +
                    $"Điểm tín dụng hiện tại: {cicInfo.DiemTinDungCic}. " +
                    $"Xếp hạng: {cicInfo.XepHangTinDungCic}. " +
                    $"Cập nhật: {DateTime.Now:dd/MM/yyyy HH:mm}";

                cicInfo.NgayCapNhat = DateTime.Now;

                _logger.LogInformation($"Đã trừ {diemTru} điểm CIC. Điểm hiện tại: {cicInfo.DiemTinDungCic}, Xếp hạng: {cicInfo.XepHangTinDungCic}");

                // Ghi lịch sử tra cứu CIC
                var lichSuCic = new LichSuTraCuuCic
                {
                    MaCic = cicInfo.MaCic,
                    LoaiKhachHang = cicInfo.LoaiKhachHang,
                    MaKhachHang = cicInfo.MaKhachHang,
                    SoCmndCccd = cicInfo.SoCmndCccd,
                    MaSoThue = cicInfo.MaSoThue,
                    NgayTraCuu = DateTime.Now,
                    NguoiTraCuu = 1, // System
                    KetQua = "Cập nhật tự động",
                    ThongTinTraVe = $"Trừ điểm do quá hạn thanh toán {soNgayQuaHan} ngày",
                    DiemTinDung = cicInfo.DiemTinDungCic,
                    XepHangTinDung = cicInfo.XepHangTinDungCic,
                    TongDuNo = cicInfo.TongDuNoCic,
                    SoKhoanVayDangVay = cicInfo.SoKhoanVayDangVayCic,
                    SoKhoanVayNoXau = cicInfo.SoKhoanVayNoXauCic,
                    GhiChu = $"[AUTO] Quá hạn {soNgayQuaHan} ngày, trừ {diemTru} điểm CIC"
                };
                context.LichSuTraCuuCics.Add(lichSuCic);
            }
        }

        /// <summary>
        /// Tăng lãi suất cho các kỳ trả nợ tiếp theo
        /// </summary>
        private async Task IncreaseInterestForFuturePaymentsAsync(
            ApplicationDbContext context,
            KhoanVay khoanVay,
            int currentKy,
            int soNgayQuaHan,
            CancellationToken stoppingToken)
        {
            // Tính tỷ lệ tăng lãi suất dựa trên số ngày quá hạn
            decimal interestIncrease = INTEREST_PENALTY_RATE;
            if (soNgayQuaHan > 30) interestIncrease = 1.0m;
            if (soNgayQuaHan > 60) interestIncrease = 1.5m;
            if (soNgayQuaHan > 90) interestIncrease = 2.0m;

            // Lấy các kỳ trả nợ chưa thanh toán sau kỳ hiện tại
            var futurePayments = await context.LichSuTraNos
                .Where(l => l.MaKhoanVay == khoanVay.MaKhoanVay 
                         && l.KyTraNo > currentKy 
                         && l.TrangThai == "Chưa thanh toán")
                .ToListAsync(stoppingToken);

            foreach (var payment in futurePayments)
            {
                // Tính lãi suất mới (lãi suất cũ + phần trăm phạt)
                decimal laiSuatMoi = (khoanVay.LaiSuat / 100 / 12) + (interestIncrease / 100);
                
                // Tính lại tiền lãi phải trả
                decimal laiPhaiTraMoi = payment.SoDuGocConLai.HasValue 
                    ? payment.SoDuGocConLai.Value * laiSuatMoi
                    : (khoanVay.SoDuGocConLai ?? khoanVay.SoTienVay) * laiSuatMoi;

                // Chỉ cập nhật nếu lãi mới cao hơn
                if (laiPhaiTraMoi > payment.SoTienLaiPhaiTra)
                {
                    decimal chenhLech = laiPhaiTraMoi - payment.SoTienLaiPhaiTra;
                    payment.SoTienLaiPhaiTra = laiPhaiTraMoi;
                    payment.TongPhaiTra = payment.SoTienGocPhaiTra + laiPhaiTraMoi;
                    
                    payment.GhiChu = $"{payment.GhiChu ?? ""} [INTEREST_INCREASED:{DateTime.Now:yyyy-MM-dd}] Tăng lãi +{interestIncrease}% do quá hạn kỳ {currentKy}, lãi tăng thêm: {chenhLech:N0}đ";
                    
                    _logger.LogInformation($"Kỳ {payment.KyTraNo}: Tăng lãi từ {payment.SoTienLaiPhaiTra - chenhLech:N0}đ lên {laiPhaiTraMoi:N0}đ (+{chenhLech:N0}đ)");
                }
            }

            // Cập nhật lãi suất của khoản vay (ghi nhận lãi suất phạt)
            _logger.LogInformation($"Đã tăng lãi suất {interestIncrease}% cho {futurePayments.Count} kỳ tiếp theo của khoản vay {khoanVay.MaKhoanVayCode}");
        }

        /// <summary>
        /// Tính phí trả chậm
        /// </summary>
        private decimal CalculateLatePaymentFee(decimal tongPhaiTra, int soNgayQuaHan)
        {
            // Phí trả chậm = 0.05% số tiền phải trả mỗi ngày quá hạn
            decimal tyLePhi = 0.0005m; // 0.05%
            return Math.Round(tongPhaiTra * tyLePhi * soNgayQuaHan, 0);
        }

        /// <summary>
        /// Xác định xếp hạng CIC từ điểm số
        /// </summary>
        private string GetCicRating(int score)
        {
            return score switch
            {
                >= 800 => "AAA",
                >= 700 => "AA",
                >= 600 => "A",
                >= 500 => "BBB",
                >= 400 => "BB",
                >= 300 => "B",
                >= 200 => "CCC",
                >= 100 => "CC",
                _ => "C"
            };
        }
    }
}
