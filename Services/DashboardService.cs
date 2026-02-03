using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuanLyRuiRoTinDung.Models.EF;
using QuanLyRuiRoTinDung.Models.Entities;
using System.Globalization;

namespace QuanLyRuiRoTinDung.Services
{
    public interface IDashboardService
    {
        Task<DashboardViewModel> GetDashboardDataAsync(int maNhanVien);
    }

    public class DashboardService : IDashboardService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DashboardService> _logger;

        public DashboardService(ApplicationDbContext context, ILogger<DashboardService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<DashboardViewModel> GetDashboardDataAsync(int maNhanVien)
        {
            var viewModel = new DashboardViewModel();

            try
            {
                // 1. Tổng số khách hàng (cả cá nhân và doanh nghiệp)
                var tongKhachHangCaNhan = await _context.KhachHangCaNhans
                    .Where(k => k.TrangThaiHoatDong == true)
                    .CountAsync();

                var tongKhachHangDoanhNghiep = await _context.KhachHangDoanhNghieps
                    .Where(k => k.TrangThaiHoatDong == true)
                    .CountAsync();

                viewModel.TongKhachHang = tongKhachHangCaNhan + tongKhachHangDoanhNghiep;

                // 2. Tổng hồ sơ vay đang quản lý (của nhân viên này)
                viewModel.TongHoSoVay = await _context.KhoanVays
                    .Where(k => k.MaNhanVienTinDung == maNhanVien 
                        && k.TrangThaiKhoanVay != "Đã thanh toán" 
                        && k.TrangThaiKhoanVay != "Từ chối")
                    .CountAsync();

                // 3. Hồ sơ chờ phê duyệt (Nhân viên đã gửi đi, đang chờ duyệt)
                viewModel.ChoPheDuyet = await _context.KhoanVays
                    .Where(k => k.MaNhanVienTinDung == maNhanVien 
                        && k.TrangThaiKhoanVay == "Chờ duyệt")
                    .CountAsync();

                // 4. Tổng hồ sơ nháp (của cả khách hàng cá nhân và doanh nghiệp)
                viewModel.YeuCauBoSung = await _context.KhoanVays
                    .Where(k => k.MaNhanVienTinDung == maNhanVien 
                        && k.TrangThaiKhoanVay == "Nháp")
                    .CountAsync();

                // 5. Hồ sơ cần xử lý hôm nay (các hồ sơ đang xử lý, chờ bổ sung, đã duyệt)
                var ngayHomNay = DateTime.Now.Date;
                viewModel.HoSoCanXuLy = await _context.KhoanVays
                    .Where(k => k.MaNhanVienTinDung == maNhanVien
                        && (k.TrangThaiKhoanVay == "Đã duyệt" 
                            || k.TrangThaiKhoanVay == "Chờ bổ sung"
                            || k.TrangThaiKhoanVay == "Đang xử lý")
                        && k.NgayNopHoSo.HasValue
                        && k.NgayNopHoSo.Value.Date <= ngayHomNay)
                    .CountAsync();

                // 6. Trạng thái khoản vay
                var tongKhoanVayDangQuanLy = await _context.KhoanVays
                    .Where(k => k.MaNhanVienTinDung == maNhanVien
                        && (k.TrangThaiKhoanVay == "Đã giải ngân" || k.TrangThaiKhoanVay == "Đang trả nợ"))
                    .CountAsync();

                if (tongKhoanVayDangQuanLy > 0)
                {
                    viewModel.DangHoatDong = await _context.KhoanVays
                        .Where(k => k.MaNhanVienTinDung == maNhanVien
                            && (k.TrangThaiKhoanVay == "Đã giải ngân" || k.TrangThaiKhoanVay == "Đang trả nợ")
                            && (k.SoNgayQuaHan == null || k.SoNgayQuaHan == 0))
                        .CountAsync();

                    var ngayHomNayDateOnly = DateOnly.FromDateTime(ngayHomNay);
                    var ngay7NgaySau = DateOnly.FromDateTime(ngayHomNay.AddDays(7));
                    
                    viewModel.DenHan7Ngay = await _context.KhoanVays
                        .Where(k => k.MaNhanVienTinDung == maNhanVien
                            && (k.TrangThaiKhoanVay == "Đã giải ngân" || k.TrangThaiKhoanVay == "Đang trả nợ")
                            && k.NgayDaoHan.HasValue
                            && k.NgayDaoHan.Value <= ngay7NgaySau
                            && k.NgayDaoHan.Value > ngayHomNayDateOnly
                            && (k.SoNgayQuaHan == null || k.SoNgayQuaHan == 0))
                        .CountAsync();

                    viewModel.QuaHan = await _context.KhoanVays
                        .Where(k => k.MaNhanVienTinDung == maNhanVien
                            && (k.TrangThaiKhoanVay == "Đã giải ngân" || k.TrangThaiKhoanVay == "Đang trả nợ")
                            && k.SoNgayQuaHan > 0)
                        .CountAsync();

                    // Tính phần trăm
                    viewModel.TyLeDangHoatDong = (double)viewModel.DangHoatDong / tongKhoanVayDangQuanLy * 100;
                    viewModel.TyLeDenHan7Ngay = (double)viewModel.DenHan7Ngay / tongKhoanVayDangQuanLy * 100;
                    viewModel.TyLeQuaHan = (double)viewModel.QuaHan / tongKhoanVayDangQuanLy * 100;
                }

                // 7. Cảnh báo cần chú ý
                var khoanVayIds = await _context.KhoanVays
                    .Where(k => k.MaNhanVienTinDung == maNhanVien)
                    .Select(k => k.MaKhoanVay)
                    .ToListAsync();

                viewModel.CanhBaos = await _context.CanhBaos
                    .Where(c => c.MaKhoanVay != null
                        && khoanVayIds.Contains(c.MaKhoanVay.Value)
                        && (c.TrangThai == "Chưa xử lý" || c.TrangThai == null))
                    .OrderByDescending(c => c.NgayCanhBao)
                    .Take(5)
                    .Include(c => c.MaLoaiCanhBaoNavigation)
                    .Include(c => c.MaKhoanVayNavigation)
                    .ToListAsync();

                // 8. Hồ sơ vay cần xử lý
                var hoSoVayList = await _context.KhoanVays
                    .Where(k => k.MaNhanVienTinDung == maNhanVien
                        && (k.TrangThaiKhoanVay == "Đã duyệt"
                            || k.TrangThaiKhoanVay == "Chờ bổ sung"
                            || k.TrangThaiKhoanVay == "Đang xử lý"))
                    .OrderBy(k => k.NgayNopHoSo)
                    .Take(10)
                    .ToListAsync();

                viewModel.HoSoVayCanXuLy = hoSoVayList.Select(k => new HoSoVayItem
                {
                    MaKhoanVay = k.MaKhoanVay,
                    MaKhoanVayCode = k.MaKhoanVayCode,
                    LoaiKhachHang = k.LoaiKhachHang,
                    MaKhachHang = k.MaKhachHang,
                    SoTienVay = k.SoTienVay,
                    TrangThaiKhoanVay = k.TrangThaiKhoanVay,
                    MucDoRuiRo = k.MucDoRuiRo,
                    NgayNopHoSo = k.NgayNopHoSo,
                    NgayDaoHan = k.NgayDaoHan.HasValue ? k.NgayDaoHan.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null
                }).ToList();

                // Lấy thông tin khách hàng cho từng hồ sơ
                foreach (var hoSo in viewModel.HoSoVayCanXuLy)
                {
                    if (hoSo.LoaiKhachHang == "CaNhan")
                    {
                        var khachHang = await _context.KhachHangCaNhans
                            .FirstOrDefaultAsync(k => k.MaKhachHang == hoSo.MaKhachHang);
                        if (khachHang != null)
                        {
                            hoSo.TenKhachHang = khachHang.HoTen;
                        }
                    }
                    else if (hoSo.LoaiKhachHang == "DoanhNghiep")
                    {
                        var khachHang = await _context.KhachHangDoanhNghieps
                            .FirstOrDefaultAsync(k => k.MaKhachHang == hoSo.MaKhachHang);
                        if (khachHang != null)
                        {
                            hoSo.TenKhachHang = khachHang.TenCongTy;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard data for employee: {MaNhanVien}", maNhanVien);
            }

            return viewModel;
        }
    }

    // ViewModel cho Dashboard
    public class DashboardViewModel
    {
        public int TongKhachHang { get; set; }
        public int TongHoSoVay { get; set; }
        public int ChoPheDuyet { get; set; }
        public int YeuCauBoSung { get; set; }
        public int HoSoCanXuLy { get; set; }

        // Trạng thái khoản vay
        public int DangHoatDong { get; set; }
        public int DenHan7Ngay { get; set; }
        public int QuaHan { get; set; }
        public double TyLeDangHoatDong { get; set; }
        public double TyLeDenHan7Ngay { get; set; }
        public double TyLeQuaHan { get; set; }

        // Cảnh báo
        public List<CanhBao> CanhBaos { get; set; } = new List<CanhBao>();

        // Hồ sơ vay cần xử lý
        public List<HoSoVayItem> HoSoVayCanXuLy { get; set; } = new List<HoSoVayItem>();
    }

    public class HoSoVayItem
    {
        public int MaKhoanVay { get; set; }
        public string? MaKhoanVayCode { get; set; }
        public string? LoaiKhachHang { get; set; }
        public int MaKhachHang { get; set; }
        public string? TenKhachHang { get; set; }
        public decimal SoTienVay { get; set; }
        public string? TrangThaiKhoanVay { get; set; }
        public string? MucDoRuiRo { get; set; }
        public DateTime? NgayNopHoSo { get; set; }
        public DateTime? NgayDaoHan { get; set; }
    }
}

