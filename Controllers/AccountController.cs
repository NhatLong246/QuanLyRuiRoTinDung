using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using QuanLyRuiRoTinDung.Models;
using QuanLyRuiRoTinDung.Services;

namespace QuanLyRuiRoTinDung.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly IAuthenticationService _authenticationService;

        public AccountController(ILogger<AccountController> logger, IAuthenticationService authenticationService)
        {
            _logger = logger;
            _authenticationService = authenticationService;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            // Nếu đã đăng nhập, redirect về Dashboard
            if (HttpContext.Session.GetString("MaNguoiDung") != null)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (string.IsNullOrWhiteSpace(model.TenDangNhap) || string.IsNullOrWhiteSpace(model.MatKhau))
            {
                ModelState.AddModelError("", "Tên đăng nhập và mật khẩu không được để trống.");
                return View(model);
            }

            try
            {
                // Xác thực người dùng
                var nguoiDung = await _authenticationService.AuthenticateAsync(model.TenDangNhap, model.MatKhau);

                if (nguoiDung == null)
                {
                    ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng. Vui lòng kiểm tra lại.");
                    _logger.LogWarning("Login failed for user: {TenDangNhap}", model.TenDangNhap);
                    return View(model);
                }

                // Lưu thông tin đăng nhập vào Session
                HttpContext.Session.SetString("MaNguoiDung", nguoiDung.MaNguoiDung.ToString());
                HttpContext.Session.SetString("TenDangNhap", nguoiDung.TenDangNhap ?? "");
                HttpContext.Session.SetString("HoTen", nguoiDung.HoTen ?? "");
                HttpContext.Session.SetString("MaVaiTro", nguoiDung.MaVaiTro.ToString());
                
                if (nguoiDung.MaVaiTroNavigation != null)
                {
                    HttpContext.Session.SetString("TenVaiTro", nguoiDung.MaVaiTroNavigation.TenVaiTro ?? "");
                }

                // Cập nhật lần đăng nhập cuối
                await _authenticationService.UpdateLastLoginAsync(nguoiDung.MaNguoiDung);

                _logger.LogInformation("User {TenDangNhap} logged in successfully", model.TenDangNhap);

                // Redirect to returnUrl or Dashboard
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "Dashboard");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user: {TenDangNhap}", model.TenDangNhap);
                ModelState.AddModelError("", "Đã xảy ra lỗi trong quá trình đăng nhập. Vui lòng thử lại sau.");
                return View(model);
            }
        }

        [HttpPost]
        public IActionResult Logout()
        {
            var tenDangNhap = HttpContext.Session.GetString("TenDangNhap");
            
            // Xóa session
            HttpContext.Session.Clear();

            _logger.LogInformation("User {TenDangNhap} logged out", tenDangNhap);

            return RedirectToAction("Login", "Account");
        }
    }
}

