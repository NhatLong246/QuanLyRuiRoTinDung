using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using QuanLyRuiRoTinDung.Models;

namespace QuanLyRuiRoTinDung.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            // Kiểm tra đăng nhập
            var maNguoiDung = HttpContext.Session.GetString("MaNguoiDung");
            if (!string.IsNullOrEmpty(maNguoiDung))
            {
                // Đã đăng nhập, redirect đến Dashboard dựa trên vai trò
                var tenVaiTro = HttpContext.Session.GetString("TenVaiTro");
                if (!string.IsNullOrWhiteSpace(tenVaiTro))
                {
                    var tenVaiTroNormalized = tenVaiTro.Trim();
                    
                    if (tenVaiTroNormalized.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                    {
                        return RedirectToAction("Index", "Admin");
                    }
                    if (tenVaiTroNormalized.Equals("LanhDao", StringComparison.OrdinalIgnoreCase))
                    {
                        return RedirectToAction("Index", "LanhDao");
                    }
                    if (tenVaiTroNormalized.Equals("QuanLyRuiRo", StringComparison.OrdinalIgnoreCase))
                    {
                        return RedirectToAction("Index", "QuanLyRuiRo");
                    }
                }
                // Mặc định redirect đến Dashboard nhân viên
                return RedirectToAction("Index", "Dashboard");
            }

            // Chưa đăng nhập, hiển thị landing page
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
