using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyRuiRoTinDung.Models;
using QuanLyRuiRoTinDung.Models.EF;
using QuanLyRuiRoTinDung.Models.Entities;
using QuanLyRuiRoTinDung.Services;

namespace QuanLyRuiRoTinDung.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly IAuthenticationService _authenticationService;
        private readonly ApplicationDbContext _context;

        public AccountController(ILogger<AccountController> logger, IAuthenticationService authenticationService, ApplicationDbContext context)
        {
            _logger = logger;
            _authenticationService = authenticationService;
            _context = context;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            // Nếu đã đăng nhập, redirect dựa trên vai trò
            var maNguoiDung = HttpContext.Session.GetString("MaNguoiDung");
            if (!string.IsNullOrEmpty(maNguoiDung))
            {
                var tenVaiTro = HttpContext.Session.GetString("TenVaiTro");
                if (!string.IsNullOrWhiteSpace(tenVaiTro))
                {
                    var tenVaiTroNormalized = tenVaiTro.Trim();
                    
                    if (tenVaiTroNormalized.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogInformation("Already logged in as Admin, redirecting to Admin dashboard");
                        return RedirectToAction("Index", "Admin");
                    }
                    if (tenVaiTroNormalized.Equals("LanhDao", StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogInformation("Already logged in as LanhDao, redirecting to LanhDao dashboard");
                        return RedirectToAction("Index", "LanhDao");
                    }
                    if (tenVaiTroNormalized.Equals("QuanLyRuiRo", StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogInformation("Already logged in as QuanLyRuiRo, redirecting to QuanLyRuiRo dashboard");
                        return RedirectToAction("Index", "QuanLyRuiRo");
                    }
                }
                _logger.LogInformation("Already logged in, redirecting to default Dashboard");
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
                
                string tenVaiTro = "";
                if (nguoiDung.MaVaiTroNavigation != null)
                {
                    tenVaiTro = (nguoiDung.MaVaiTroNavigation.TenVaiTro ?? "").Trim();
                    HttpContext.Session.SetString("TenVaiTro", tenVaiTro);
                }

                // Cập nhật lần đăng nhập cuối
                await _authenticationService.UpdateLastLoginAsync(nguoiDung.MaNguoiDung);

                _logger.LogInformation("User {TenDangNhap} logged in successfully with role: {TenVaiTro}", model.TenDangNhap, tenVaiTro);

                // Redirect to returnUrl or based on role
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    _logger.LogInformation("Redirecting to returnUrl: {ReturnUrl}", returnUrl);
                    return Redirect(returnUrl);
                }

                // Redirect based on role (case-insensitive comparison)
                if (!string.IsNullOrWhiteSpace(tenVaiTro))
                {
                    var tenVaiTroNormalized = tenVaiTro.Trim();
                    
                    if (tenVaiTroNormalized.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogInformation("Redirecting Admin user to Admin dashboard");
                        return RedirectToAction("Index", "Admin");
                    }
                    if (tenVaiTroNormalized.Equals("LanhDao", StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogInformation("Redirecting LanhDao user to LanhDao dashboard");
                        return RedirectToAction("Index", "LanhDao");
                    }
                    if (tenVaiTroNormalized.Equals("QuanLyRuiRo", StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogInformation("Redirecting QuanLyRuiRo user to QuanLyRuiRo dashboard");
                        return RedirectToAction("Index", "QuanLyRuiRo");
                    }
                }

                _logger.LogInformation("Redirecting user to default Dashboard");
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

        [HttpGet]
        public async Task<IActionResult> TaoTaiKhoanNhanVien()
        {
            // Kiểm tra đăng nhập và quyền (chỉ Admin mới được tạo tài khoản)
            var maNguoiDungStr = HttpContext.Session.GetString("MaNguoiDung");
            var tenVaiTro = HttpContext.Session.GetString("TenVaiTro");
            
            // Log để debug
            _logger.LogInformation("TaoTaiKhoanNhanVien - MaNguoiDung: {MaNguoiDung}, TenVaiTro: '{TenVaiTro}'", 
                maNguoiDungStr, tenVaiTro);
            
            if (string.IsNullOrEmpty(maNguoiDungStr))
            {
                TempData["Error"] = "Vui lòng đăng nhập để tiếp tục.";
                return RedirectToAction("Login", "Account");
            }
            
            // Kiểm tra quyền với case-insensitive và trim
            if (string.IsNullOrWhiteSpace(tenVaiTro) || 
                !tenVaiTro.Trim().Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Access denied - User role: '{TenVaiTro}'", tenVaiTro);
                TempData["Error"] = "Bạn không có quyền truy cập trang này. Chỉ Admin mới được tạo tài khoản nhân viên.";
                return RedirectToAction("Index", "Admin");
            }

            // Load danh sách vai trò và phòng ban
            // Không hiển thị vai trò Admin và LanhDao (chỉ cho phép tạo tài khoản nhân viên)
            var vaiTros = await _context.VaiTros
                .Where(v => v.TrangThaiHoatDong == true 
                    && v.TenVaiTro != "Admin" 
                    && v.TenVaiTro != "LanhDao")
                .OrderBy(v => v.TenVaiTro)
                .ToListAsync();

            var phongBans = await _context.PhongBans
                .Where(p => p.TrangThaiHoatDong == true)
                .OrderBy(p => p.TenPhongBan)
                .ToListAsync();

            ViewBag.VaiTros = vaiTros;
            ViewBag.PhongBans = phongBans;

            // Tạo mapping giữa vai trò và phòng ban để tự động chọn
            var roleDepartmentMapping = new Dictionary<string, string>();
            foreach (var pb in phongBans)
            {
                if (pb.TenPhongBan.Contains("Tín dụng", StringComparison.OrdinalIgnoreCase) || 
                    pb.TenPhongBan.Contains("TinDung", StringComparison.OrdinalIgnoreCase))
                {
                    roleDepartmentMapping["NhanVienTinDung"] = pb.MaPhongBan.ToString();
                }
                else if (pb.TenPhongBan.Contains("Rủi ro", StringComparison.OrdinalIgnoreCase) || 
                         pb.TenPhongBan.Contains("RuiRo", StringComparison.OrdinalIgnoreCase) ||
                         pb.TenPhongBan.Contains("Quản lý Rủi ro", StringComparison.OrdinalIgnoreCase))
                {
                    roleDepartmentMapping["QuanLyRuiRo"] = pb.MaPhongBan.ToString();
                }
            }
            ViewBag.RoleDepartmentMapping = roleDepartmentMapping;

            return View(new CreateEmployeeAccountViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TaoTaiKhoanNhanVien(CreateEmployeeAccountViewModel model)
        {
            // Kiểm tra đăng nhập và quyền
            var maNguoiDungStr = HttpContext.Session.GetString("MaNguoiDung");
            var tenVaiTro = HttpContext.Session.GetString("TenVaiTro");
            
            if (string.IsNullOrEmpty(maNguoiDungStr))
            {
                TempData["Error"] = "Vui lòng đăng nhập để tiếp tục.";
                return RedirectToAction("Login", "Account");
            }
            
            // Kiểm tra quyền với case-insensitive và trim
            if (string.IsNullOrWhiteSpace(tenVaiTro) || 
                !tenVaiTro.Trim().Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Access denied for POST - User role: '{TenVaiTro}'", tenVaiTro);
                TempData["Error"] = "Bạn không có quyền thực hiện thao tác này. Chỉ Admin mới được tạo tài khoản nhân viên.";
                return RedirectToAction("Index", "Admin");
            }

            if (!int.TryParse(maNguoiDungStr, out int nguoiTao))
            {
                TempData["Error"] = "Thông tin đăng nhập không hợp lệ.";
                return RedirectToAction("Index", "Admin");
            }

            // Kiểm tra tên đăng nhập đã tồn tại chưa
            var existingUser = await _context.NguoiDungs
                .FirstOrDefaultAsync(u => u.TenDangNhap == model.TenDangNhap);
            
            if (existingUser != null)
            {
                ModelState.AddModelError("TenDangNhap", "Tên đăng nhập đã tồn tại. Vui lòng chọn tên khác.");
            }

            // Không yêu cầu vai trò và phòng ban trong form, sẽ được cấu hình sau

            if (!ModelState.IsValid)
            {
                // Load lại danh sách vai trò và phòng ban
                // Không hiển thị vai trò Admin và LanhDao
                var vaiTros = await _context.VaiTros
                    .Where(v => v.TrangThaiHoatDong == true 
                        && v.TenVaiTro != "Admin" 
                        && v.TenVaiTro != "LanhDao")
                    .OrderBy(v => v.TenVaiTro)
                    .ToListAsync();

                var phongBans = await _context.PhongBans
                    .Where(p => p.TrangThaiHoatDong == true)
                    .OrderBy(p => p.TenPhongBan)
                    .ToListAsync();

                ViewBag.VaiTros = vaiTros;
                ViewBag.PhongBans = phongBans;

                // Tạo mapping giữa vai trò và phòng ban để tự động chọn
                var roleDepartmentMapping = new Dictionary<string, string>();
                foreach (var pb in phongBans)
                {
                    if (pb.TenPhongBan.Contains("Tín dụng", StringComparison.OrdinalIgnoreCase) || 
                        pb.TenPhongBan.Contains("TinDung", StringComparison.OrdinalIgnoreCase))
                    {
                        roleDepartmentMapping["NhanVienTinDung"] = pb.MaPhongBan.ToString();
                    }
                    else if (pb.TenPhongBan.Contains("Rủi ro", StringComparison.OrdinalIgnoreCase) || 
                             pb.TenPhongBan.Contains("RuiRo", StringComparison.OrdinalIgnoreCase) ||
                             pb.TenPhongBan.Contains("Quản lý Rủi ro", StringComparison.OrdinalIgnoreCase))
                    {
                        roleDepartmentMapping["QuanLyRuiRo"] = pb.MaPhongBan.ToString();
                    }
                }
                ViewBag.RoleDepartmentMapping = roleDepartmentMapping;

                return View(model);
            }

            try
            {
                // Tạo tài khoản mới
                // Lấy vai trò mặc định (NhanVienTinDung) nếu không có vai trò nào được chọn
                var defaultVaiTro = await _context.VaiTros
                    .FirstOrDefaultAsync(v => v.TenVaiTro == "NhanVienTinDung" && v.TrangThaiHoatDong == true);
                
                if (defaultVaiTro == null)
                {
                    // Nếu không tìm thấy vai trò mặc định, lấy vai trò đầu tiên có sẵn (không phải Admin hoặc LanhDao)
                    defaultVaiTro = await _context.VaiTros
                        .Where(v => v.TrangThaiHoatDong == true 
                            && v.TenVaiTro != "Admin" 
                            && v.TenVaiTro != "LanhDao")
                        .FirstOrDefaultAsync();
                }
                
                var nguoiDung = new NguoiDung
                {
                    TenDangNhap = model.TenDangNhap.Trim(),
                    MatKhauHash = model.MatKhau.Trim(), // Lưu plain text (theo hệ thống hiện tại)
                    HoTen = model.HoTen.Trim(),
                    Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim(),
                    SoDienThoai = string.IsNullOrWhiteSpace(model.SoDienThoai) ? null : model.SoDienThoai.Trim(),
                    MaVaiTro = model.MaVaiTro > 0 ? model.MaVaiTro : (defaultVaiTro?.MaVaiTro ?? 0),
                    MaPhongBan = model.MaPhongBan,
                    TrangThaiHoatDong = true,
                    NgayTao = DateTime.Now,
                    NguoiTao = nguoiTao
                };

                _context.NguoiDungs.Add(nguoiDung);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Employee account created: {TenDangNhap} by {NguoiTao}", model.TenDangNhap, nguoiTao);
                
                // Lưu thông tin vào TempData để hiển thị trong modal
                TempData["Success"] = $"Tạo tài khoản nhân viên thành công! Tên đăng nhập: {model.TenDangNhap}";
                TempData["CreatedTenDangNhap"] = model.TenDangNhap;
                TempData["CreatedHoTen"] = model.HoTen;
                TempData["CreatedEmail"] = model.Email ?? "";
                TempData["CreatedSoDienThoai"] = model.SoDienThoai ?? "";

                return RedirectToAction("TaoTaiKhoanNhanVien");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating employee account");
                ModelState.AddModelError("", "Đã xảy ra lỗi khi tạo tài khoản. Vui lòng thử lại sau.");

                // Load lại danh sách vai trò và phòng ban
                // Không hiển thị vai trò Admin và LanhDao
                var vaiTros = await _context.VaiTros
                    .Where(v => v.TrangThaiHoatDong == true 
                        && v.TenVaiTro != "Admin" 
                        && v.TenVaiTro != "LanhDao")
                    .OrderBy(v => v.TenVaiTro)
                    .ToListAsync();

                var phongBans = await _context.PhongBans
                    .Where(p => p.TrangThaiHoatDong == true)
                    .OrderBy(p => p.TenPhongBan)
                    .ToListAsync();

                ViewBag.VaiTros = vaiTros;
                ViewBag.PhongBans = phongBans;

                // Tạo mapping giữa vai trò và phòng ban để tự động chọn
                var roleDepartmentMapping = new Dictionary<string, string>();
                foreach (var pb in phongBans)
                {
                    if (pb.TenPhongBan.Contains("Tín dụng", StringComparison.OrdinalIgnoreCase) || 
                        pb.TenPhongBan.Contains("TinDung", StringComparison.OrdinalIgnoreCase))
                    {
                        roleDepartmentMapping["NhanVienTinDung"] = pb.MaPhongBan.ToString();
                    }
                    else if (pb.TenPhongBan.Contains("Rủi ro", StringComparison.OrdinalIgnoreCase) || 
                             pb.TenPhongBan.Contains("RuiRo", StringComparison.OrdinalIgnoreCase) ||
                             pb.TenPhongBan.Contains("Quản lý Rủi ro", StringComparison.OrdinalIgnoreCase))
                    {
                        roleDepartmentMapping["QuanLyRuiRo"] = pb.MaPhongBan.ToString();
                    }
                }
                ViewBag.RoleDepartmentMapping = roleDepartmentMapping;

                return View(model);
            }
        }
    }
}

