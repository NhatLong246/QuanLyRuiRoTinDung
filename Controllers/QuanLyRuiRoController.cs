using Microsoft.AspNetCore.Mvc;

namespace QuanLyRuiRoTinDung.Controllers
{
    public class QuanLyRuiRoController : Controller
    {
        public IActionResult Index()
        {
            // Kiểm tra đăng nhập
            var maNguoiDung = HttpContext.Session.GetString("MaNguoiDung");
            if (string.IsNullOrEmpty(maNguoiDung))
            {
                return RedirectToAction("Login", "Account");
            }

            // Kiểm tra vai trò (tùy chọn - có thể bỏ nếu muốn cho phép tất cả)
            var tenVaiTro = HttpContext.Session.GetString("TenVaiTro");
            if (tenVaiTro != "QuanLyRuiRo")
            {
                // Có thể redirect hoặc hiển thị thông báo không có quyền
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập chức năng này.";
                return RedirectToAction("Index", "Dashboard");
            }

            ViewData["Title"] = "Quản lý Rủi ro Tín dụng";
            return View();
        }

        private bool CheckAuthorization()
        {
            var maNguoiDung = HttpContext.Session.GetString("MaNguoiDung");
            if (string.IsNullOrEmpty(maNguoiDung))
            {
                return false;
            }

            var tenVaiTro = HttpContext.Session.GetString("TenVaiTro");
            return tenVaiTro == "QuanLyRuiRo";
        }

        public IActionResult ThamDinhRuiRoTinDung()
        {
            if (!CheckAuthorization())
            {
                return RedirectToAction("Login", "Account");
            }
            ViewData["Title"] = "Thẩm định Rủi ro Tín dụng";
            return View();
        }

        public IActionResult PhanLoaiMucDoRuiRo()
        {
            if (!CheckAuthorization())
            {
                return RedirectToAction("Login", "Account");
            }
            ViewData["Title"] = "Phân loại Mức độ Rủi ro";
            return View();
        }

        public IActionResult XayDungTieuChi()
        {
            if (!CheckAuthorization())
            {
                return RedirectToAction("Login", "Account");
            }
            ViewData["Title"] = "Xây dựng Tiêu chí Đánh giá Rủi ro";
            return View();
        }

        public IActionResult TheoDoiDanhMucTinDung()
        {
            if (!CheckAuthorization())
            {
                return RedirectToAction("Login", "Account");
            }
            ViewData["Title"] = "Theo dõi Danh mục Tín dụng";
            return View();
        }

        public IActionResult PhatHienSomNoXau()
        {
            if (!CheckAuthorization())
            {
                return RedirectToAction("Login", "Account");
            }
            ViewData["Title"] = "Phát hiện sớm Nợ xấu";
            return View();
        }

        public IActionResult DeXuatBienPhapXuLy()
        {
            if (!CheckAuthorization())
            {
                return RedirectToAction("Login", "Account");
            }
            ViewData["Title"] = "Đề xuất Biện pháp Xử lý Rủi ro";
            return View();
        }
    }
}
