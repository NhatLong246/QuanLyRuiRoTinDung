using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyRuiRoTinDung.Models.EF;
using QuanLyRuiRoTinDung.Services;

namespace QuanLyRuiRoTinDung.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ILogger<DashboardController> _logger;
        private readonly IDashboardService _dashboardService;
        private readonly ApplicationDbContext _context;

        public DashboardController(ILogger<DashboardController> logger, IDashboardService dashboardService, ApplicationDbContext context)
        {
            _logger = logger;
            _dashboardService = dashboardService;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Kiểm tra đăng nhập
            var maNguoiDungStr = HttpContext.Session.GetString("MaNguoiDung");
            if (string.IsNullOrEmpty(maNguoiDungStr))
            {
                return RedirectToAction("Login", "Account");
            }

            if (!int.TryParse(maNguoiDungStr, out int maNguoiDung))
            {
                return RedirectToAction("Login", "Account");
            }

            // Lấy dữ liệu dashboard
            var viewModel = await _dashboardService.GetDashboardDataAsync(maNguoiDung);

            // Lấy thông tin người dùng từ session
            ViewBag.HoTen = HttpContext.Session.GetString("HoTen") ?? "Nhân viên";
            ViewBag.TenVaiTro = HttpContext.Session.GetString("TenVaiTro") ?? "Nhân viên tín dụng";

            return View(viewModel);
        }

        // API Tìm kiếm khách hàng và hồ sơ vay (cho AJAX dropdown)
        [HttpGet]
        public async Task<IActionResult> SearchApi(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return Json(new { customers = new List<object>(), loans = new List<object>() });
            }

            keyword = keyword.Trim().ToLower();

            // Tìm khách hàng cá nhân
            var khachHangCaNhan = await _context.KhachHangCaNhans
                .Where(k => k.TrangThaiHoatDong == true &&
                    (k.HoTen.ToLower().Contains(keyword) ||
                     k.SoCmnd.Contains(keyword) ||
                     k.SoDienThoai.Contains(keyword) ||
                     k.Email.ToLower().Contains(keyword)))
                .Take(5)
                .Select(k => new
                {
                    maKhachHang = k.MaKhachHang,
                    ten = k.HoTen,
                    loai = "Cá nhân",
                    soDinhDanh = k.SoCmnd,
                    soDienThoai = k.SoDienThoai
                })
                .ToListAsync();

            // Tìm khách hàng doanh nghiệp
            var khachHangDoanhNghiep = await _context.KhachHangDoanhNghieps
                .Where(k => k.TrangThaiHoatDong == true &&
                    (k.TenCongTy.ToLower().Contains(keyword) ||
                     k.MaSoThue.Contains(keyword) ||
                     k.SoDienThoai.Contains(keyword) ||
                     k.Email.ToLower().Contains(keyword)))
                .Take(5)
                .Select(k => new
                {
                    maKhachHang = k.MaKhachHang,
                    ten = k.TenCongTy,
                    loai = "Doanh nghiệp",
                    soDinhDanh = k.MaSoThue,
                    soDienThoai = k.SoDienThoai
                })
                .ToListAsync();

            var customers = khachHangCaNhan.Cast<object>().Concat(khachHangDoanhNghiep.Cast<object>()).ToList();

            // Tìm hồ sơ vay
            var loans = await _context.KhoanVays
                .Where(kv => kv.MaKhoanVayCode.ToLower().Contains(keyword) ||
                    (kv.MucDichVay != null && kv.MucDichVay.ToLower().Contains(keyword)))
                .Take(5)
                .Select(kv => new
                {
                    maKhoanVay = kv.MaKhoanVay,
                    maKhoanVayCode = kv.MaKhoanVayCode,
                    soTienVay = kv.SoTienVay,
                    trangThai = kv.TrangThaiKhoanVay,
                    loaiKhachHang = kv.LoaiKhachHang,
                    maKhachHang = kv.MaKhachHang
                })
                .ToListAsync();

            return Json(new { customers, loans });
        }

        // Trang kết quả tìm kiếm
        [HttpGet]
        public async Task<IActionResult> Search(string keyword)
        {
            ViewBag.Keyword = keyword ?? "";

            if (string.IsNullOrWhiteSpace(keyword))
            {
                ViewBag.KhachHangCaNhans = new List<QuanLyRuiRoTinDung.Models.Entities.KhachHangCaNhan>();
                ViewBag.KhachHangDoanhNghieps = new List<QuanLyRuiRoTinDung.Models.Entities.KhachHangDoanhNghiep>();
                ViewBag.KhoanVays = new List<QuanLyRuiRoTinDung.Models.Entities.KhoanVay>();
                return View();
            }

            keyword = keyword.Trim().ToLower();

            // Tìm khách hàng cá nhân
            var khachHangCaNhans = await _context.KhachHangCaNhans
                .Where(k => k.TrangThaiHoatDong == true &&
                    (k.HoTen.ToLower().Contains(keyword) ||
                     k.SoCmnd.Contains(keyword) ||
                     k.SoDienThoai.Contains(keyword) ||
                     k.Email.ToLower().Contains(keyword)))
                .Take(20)
                .ToListAsync();

            // Tìm khách hàng doanh nghiệp
            var khachHangDoanhNghieps = await _context.KhachHangDoanhNghieps
                .Where(k => k.TrangThaiHoatDong == true &&
                    (k.TenCongTy.ToLower().Contains(keyword) ||
                     k.MaSoThue.Contains(keyword) ||
                     k.SoDienThoai.Contains(keyword) ||
                     k.Email.ToLower().Contains(keyword)))
                .Take(20)
                .ToListAsync();

            // Tìm hồ sơ vay
            var khoanVays = await _context.KhoanVays
                .Include(k => k.MaLoaiVayNavigation)
                .Where(kv => kv.MaKhoanVayCode.ToLower().Contains(keyword) ||
                    (kv.MucDichVay != null && kv.MucDichVay.ToLower().Contains(keyword)))
                .Take(20)
                .ToListAsync();

            ViewBag.KhachHangCaNhans = khachHangCaNhans;
            ViewBag.KhachHangDoanhNghieps = khachHangDoanhNghieps;
            ViewBag.KhoanVays = khoanVays;

            return View();
        }

        // Lấy danh sách thông báo
        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var maNguoiDungStr = HttpContext.Session.GetString("MaNguoiDung");
            if (string.IsNullOrEmpty(maNguoiDungStr) || !int.TryParse(maNguoiDungStr, out int maNguoiDung))
            {
                return Json(new { notifications = new List<object>(), count = 0 });
            }

            var notifications = new List<object>();

            // 1. Hồ sơ đã được duyệt (trong 7 ngày gần đây)
            var hoSoDaDuyet = await _context.KhoanVays
                .Where(kv => kv.MaNhanVienTinDung == maNguoiDung &&
                    kv.TrangThaiKhoanVay == "Đã duyệt" &&
                    kv.NgayCapNhat >= DateTime.Now.AddDays(-7))
                .OrderByDescending(kv => kv.NgayCapNhat)
                .Take(5)
                .Select(kv => new
                {
                    id = kv.MaKhoanVay,
                    type = "approved",
                    title = "Hồ sơ đã được duyệt",
                    message = $"Hồ sơ #{kv.MaKhoanVayCode} đã được phê duyệt",
                    date = kv.NgayCapNhat,
                    icon = "check-circle"
                })
                .ToListAsync();

            notifications.AddRange(hoSoDaDuyet);

            // 2. Hồ sơ nháp lâu chưa bổ sung (hơn 3 ngày)
            var threeDaysAgo = DateTime.Now.AddDays(-3);
            var hoSoNhapCuData = await _context.KhoanVays
                .Where(kv => kv.MaNhanVienTinDung == maNguoiDung &&
                    kv.TrangThaiKhoanVay == "Nháp" &&
                    kv.NgayTao < threeDaysAgo)
                .OrderBy(kv => kv.NgayTao)
                .Take(5)
                .Select(kv => new
                {
                    MaKhoanVay = kv.MaKhoanVay,
                    MaKhoanVayCode = kv.MaKhoanVayCode,
                    NgayTao = kv.NgayTao
                })
                .ToListAsync();

            var hoSoNhapCu = hoSoNhapCuData.Select(kv => new
            {
                id = kv.MaKhoanVay,
                type = "draft",
                title = "Hồ sơ nháp chờ hoàn thiện",
                message = $"Hồ sơ #{kv.MaKhoanVayCode} đã nháp {(kv.NgayTao.HasValue ? (int)(DateTime.Now - kv.NgayTao.Value).TotalDays : 0)} ngày",
                date = kv.NgayTao,
                icon = "file-text"
            }).ToList();

            notifications.AddRange(hoSoNhapCu);

            // 3. Hồ sơ yêu cầu bổ sung
            var hoSoBoSung = await _context.KhoanVays
                .Where(kv => kv.MaNhanVienTinDung == maNguoiDung &&
                    kv.TrangThaiKhoanVay == "Chờ bổ sung")
                .OrderByDescending(kv => kv.NgayCapNhat)
                .Take(5)
                .Select(kv => new
                {
                    id = kv.MaKhoanVay,
                    type = "supplement",
                    title = "Yêu cầu bổ sung hồ sơ",
                    message = $"Hồ sơ #{kv.MaKhoanVayCode} cần bổ sung thông tin",
                    date = kv.NgayCapNhat,
                    icon = "alert-circle"
                })
                .ToListAsync();

            notifications.AddRange(hoSoBoSung);

            // Sắp xếp theo ngày mới nhất
            var sortedNotifications = notifications
                .OrderByDescending(n => ((dynamic)n).date)
                .Take(10)
                .ToList();

            return Json(new { notifications = sortedNotifications, count = sortedNotifications.Count });
        }
    }
}

