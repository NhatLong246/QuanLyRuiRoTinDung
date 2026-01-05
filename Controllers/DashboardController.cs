using Microsoft.AspNetCore.Mvc;
using QuanLyRuiRoTinDung.Services;

namespace QuanLyRuiRoTinDung.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ILogger<DashboardController> _logger;
        private readonly IDashboardService _dashboardService;

        public DashboardController(ILogger<DashboardController> logger, IDashboardService dashboardService)
        {
            _logger = logger;
            _dashboardService = dashboardService;
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
    }
}

