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
                    title = "Khoản vay đã được duyệt",
                    message = $"Hồ sơ #{kv.MaKhoanVayCode} đã được phê duyệt, cần giải ngân",
                    date = kv.NgayCapNhat,
                    icon = "check-circle",
                    color = "#10B981"
                })
                .ToListAsync();

            notifications.AddRange(hoSoDaDuyet);

            // 2. Hồ sơ bị từ chối (trong 7 ngày gần đây)
            var hoSoBiTuChoi = await _context.KhoanVays
                .Where(kv => kv.MaNhanVienTinDung == maNguoiDung &&
                    kv.TrangThaiKhoanVay == "Từ chối" &&
                    kv.NgayCapNhat >= DateTime.Now.AddDays(-7))
                .OrderByDescending(kv => kv.NgayCapNhat)
                .Take(5)
                .Select(kv => new
                {
                    id = kv.MaKhoanVay,
                    type = "rejected",
                    title = "Khoản vay bị từ chối",
                    message = $"Hồ sơ #{kv.MaKhoanVayCode} đã bị từ chối phê duyệt",
                    date = kv.NgayCapNhat,
                    icon = "x-circle",
                    color = "#EF4444"
                })
                .ToListAsync();

            notifications.AddRange(hoSoBiTuChoi);

            // 3. Nhắc nhở giải ngân (khoản vay đã duyệt quá 3 ngày chưa giải ngân)
            var threeDaysAgo = DateTime.Now.AddDays(-3);
            var canGiaiNgan = await _context.KhoanVays
                .Where(kv => kv.MaNhanVienTinDung == maNguoiDung &&
                    kv.TrangThaiKhoanVay == "Đã duyệt" &&
                    kv.NgayCapNhat < threeDaysAgo)
                .OrderBy(kv => kv.NgayCapNhat)
                .Take(5)
                .Select(kv => new
                {
                    id = kv.MaKhoanVay,
                    type = "disbursement_reminder",
                    title = "Nhắc nhở giải ngân",
                    message = $"Hồ sơ #{kv.MaKhoanVayCode} đã duyệt, cần giải ngân sớm",
                    date = kv.NgayCapNhat,
                    icon = "clock",
                    color = "#F59E0B"
                })
                .ToListAsync();

            notifications.AddRange(canGiaiNgan);

            // 4. Khoản vay quá hạn thanh toán
            var khoanVayQuaHan = await _context.KhoanVays
                .Where(kv => kv.MaNhanVienTinDung == maNguoiDung &&
                    (kv.TrangThaiKhoanVay == "Đã giải ngân" || kv.TrangThaiKhoanVay == "Đang trả nợ") &&
                    kv.SoNgayQuaHan > 0)
                .OrderByDescending(kv => kv.SoNgayQuaHan)
                .Take(5)
                .Select(kv => new
                {
                    id = kv.MaKhoanVay,
                    type = "overdue",
                    title = "Khoản vay quá hạn",
                    message = $"Hồ sơ #{kv.MaKhoanVayCode} quá hạn {kv.SoNgayQuaHan} ngày",
                    date = kv.NgayCapNhat,
                    icon = "alert-triangle",
                    color = "#EF4444"
                })
                .ToListAsync();

            notifications.AddRange(khoanVayQuaHan);

            // 5. Khoản vay sắp đến hạn thanh toán (trong 7 ngày)
            var ngayHomNay = DateOnly.FromDateTime(DateTime.Now);
            var ngay7NgaySau = DateOnly.FromDateTime(DateTime.Now.AddDays(7));
            var sapDenHan = await _context.KhoanVays
                .Where(kv => kv.MaNhanVienTinDung == maNguoiDung &&
                    (kv.TrangThaiKhoanVay == "Đã giải ngân" || kv.TrangThaiKhoanVay == "Đang trả nợ") &&
                    kv.NgayDaoHan.HasValue &&
                    kv.NgayDaoHan.Value > ngayHomNay &&
                    kv.NgayDaoHan.Value <= ngay7NgaySau)
                .OrderBy(kv => kv.NgayDaoHan)
                .Take(5)
                .Select(kv => new
                {
                    id = kv.MaKhoanVay,
                    type = "due_soon",
                    title = "Sắp đến hạn thanh toán",
                    message = $"Hồ sơ #{kv.MaKhoanVayCode} đến hạn vào {(kv.NgayDaoHan.HasValue ? kv.NgayDaoHan.Value.ToString("dd/MM/yyyy") : "N/A")}",
                    date = kv.NgayCapNhat,
                    icon = "calendar",
                    color = "#3B82F6"
                })
                .ToListAsync();

            notifications.AddRange(sapDenHan);

            // 6. Hồ sơ yêu cầu bổ sung
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
                    icon = "alert-circle",
                    color = "#8B5CF6"
                })
                .ToListAsync();

            notifications.AddRange(hoSoBoSung);

            // 7. Hồ sơ nháp lâu ngày (quá 3 ngày chưa gửi duyệt)
            var threeDaysAgoForDraft = DateTime.Now.AddDays(-3);
            var hoSoNhapLauNgay = await _context.KhoanVays
                .Where(kv => kv.MaNhanVienTinDung == maNguoiDung &&
                    kv.TrangThaiKhoanVay == "Nháp" &&
                    kv.NgayTao <= threeDaysAgoForDraft)
                .OrderBy(kv => kv.NgayTao)
                .Take(5)
                .ToListAsync();

            var hoSoNhapLauNgayNotifications = hoSoNhapLauNgay.Select(kv => new
            {
                id = kv.MaKhoanVay,
                type = "draft_reminder",
                title = "Hồ sơ nháp cần xử lý",
                message = $"Hồ sơ #{kv.MaKhoanVayCode} đã lưu nháp {(kv.NgayTao.HasValue ? (int)(DateTime.Now - kv.NgayTao.Value).TotalDays : 0)} ngày, cần hoàn thiện và gửi duyệt",
                date = kv.NgayTao,
                icon = "file-text",
                color = "#6366F1"
            }).ToList();

            notifications.AddRange(hoSoNhapLauNgayNotifications);

            // Sắp xếp theo ngày mới nhất
            var sortedNotifications = notifications
                .OrderByDescending(n => ((dynamic)n).date)
                .Take(15)
                .ToList();

            return Json(new { notifications = sortedNotifications, count = sortedNotifications.Count });
        }
    }
}

