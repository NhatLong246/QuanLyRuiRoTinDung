using Microsoft.AspNetCore.Mvc;
using QuanLyRuiRoTinDung.Services;

namespace QuanLyRuiRoTinDung.Controllers
{
    public class CicController : Controller
    {
        private readonly ILogger<CicController> _logger;
        private readonly ICicService _cicService;

        public CicController(ILogger<CicController> logger, ICicService cicService)
        {
            _logger = logger;
            _cicService = cicService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? searchTerm, string? loaiKhachHang, string? khuyenNghi)
        {
            // Kiểm tra đăng nhập
            var maNguoiDungStr = HttpContext.Session.GetString("MaNguoiDung");
            if (string.IsNullOrEmpty(maNguoiDungStr))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var cicList = await _cicService.GetAllCicAsync(searchTerm, loaiKhachHang, khuyenNghi);
                var totalCount = await _cicService.GetTotalCicCountAsync();

                ViewBag.SearchTerm = searchTerm;
                ViewBag.LoaiKhachHang = loaiKhachHang;
                ViewBag.KhuyenNghi = khuyenNghi;
                ViewBag.TotalCount = totalCount;

                return View(cicList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading CIC index page");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi tải danh sách CIC. Vui lòng thử lại sau.";
                return View(new List<QuanLyRuiRoTinDung.Models.Entities.ThongTinCic>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            // Kiểm tra đăng nhập
            var maNguoiDungStr = HttpContext.Session.GetString("MaNguoiDung");
            if (string.IsNullOrEmpty(maNguoiDungStr))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var cic = await _cicService.GetCicByIdAsync(id);
                if (cic == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin CIC.";
                    return RedirectToAction("Index");
                }

                return View(cic);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading CIC details for ID: {Id}", id);
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi tải chi tiết CIC.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public IActionResult Search(string searchTerm, string? loaiKhachHang, string? khuyenNghi)
        {
            return RedirectToAction("Index", new { searchTerm, loaiKhachHang, khuyenNghi });
        }
    }
}
