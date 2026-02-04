using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
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
        private readonly IEmailService _emailService;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AccountController(ILogger<AccountController> logger, IAuthenticationService authenticationService, IEmailService emailService, ApplicationDbContext context, IConfiguration configuration)
        {
            _logger = logger;
            _authenticationService = authenticationService;
            _emailService = emailService;
            _context = context;
            _configuration = configuration;
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

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View(model);
        //    }

        //    try
        //    {
        //        // Tìm người dùng theo email
        //        var nguoiDung = await _context.NguoiDungs
        //            .FirstOrDefaultAsync(u => u.Email != null && u.Email.Trim().ToLower() == model.Email.Trim().ToLower());

        //        // Luôn hiển thị thông báo thành công để bảo mật (không tiết lộ email có tồn tại hay không)
        //        if (nguoiDung == null)
        //        {
        //            _logger.LogWarning("Password reset requested for non-existent email: {Email}", model.Email);
        //            TempData["SuccessMessage"] = "Nếu email tồn tại trong hệ thống, bạn sẽ nhận được mã xác nhận qua email.";
        //            return RedirectToAction("ForgotPassword");
        //        }

        //        // Kiểm tra tài khoản có đang hoạt động không
        //        if (nguoiDung.TrangThaiHoatDong == false)
        //        {
        //            _logger.LogWarning("Password reset requested for inactive account: {Email}", model.Email);
        //            TempData["SuccessMessage"] = "Nếu email tồn tại trong hệ thống, bạn sẽ nhận được mã xác nhận qua email.";
        //            return RedirectToAction("ForgotPassword");
        //        }

        //        // Tạo mã OTP 6 chữ số
        //        var random = new Random();
        //        var otpCode = random.Next(100000, 999999).ToString();

        //        // Lưu mã OTP và thông tin vào session (hết hạn sau 10 phút)
        //        HttpContext.Session.SetString($"OtpCode_{model.Email.ToLower()}", otpCode);
        //        HttpContext.Session.SetString($"OtpEmail_{model.Email.ToLower()}", model.Email);
        //        HttpContext.Session.SetString($"OtpUserId_{model.Email.ToLower()}", nguoiDung.MaNguoiDung.ToString());
        //        HttpContext.Session.SetString($"OtpExpiry_{model.Email.ToLower()}", DateTime.UtcNow.AddMinutes(10).ToString("O"));

        //        // Gửi email chứa mã OTP
        //        var emailSent = await _emailService.SendOtpEmailAsync(nguoiDung.Email!, nguoiDung.HoTen, otpCode);

        //        if (emailSent)
        //        {
        //            _logger.LogInformation("OTP email sent successfully to {Email}", model.Email);
        //            TempData["SuccessMessage"] = "Chúng tôi đã gửi mã xác nhận đến email của bạn. Vui lòng kiểm tra hộp thư (cả thư mục Spam).";
        //            TempData["EmailForOtp"] = model.Email;
        //            return RedirectToAction("VerifyOtp", new { email = model.Email });
        //        }
        //        else
        //        {
        //            _logger.LogError("Failed to send OTP email to {Email}", model.Email);
        //            TempData["ErrorMessage"] = "Không thể gửi email. Vui lòng kiểm tra cấu hình email hoặc thử lại sau.";
        //        }

        //        return RedirectToAction("ForgotPassword");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error processing forgot password request for email: {Email}", model.Email);
        //        TempData["ErrorMessage"] = "Đã xảy ra lỗi khi xử lý yêu cầu. Vui lòng thử lại sau.";
        //        return View(model);
        //    }
        //}

        [HttpGet]
        public IActionResult VerifyOtp(string? email)
        {
            if (string.IsNullOrEmpty(email))
            {
                TempData["ErrorMessage"] = "Email không hợp lệ.";
                return RedirectToAction("ForgotPassword");
            }

            var model = new VerifyOtpViewModel
            {
                Email = email
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult VerifyOtp(VerifyOtpViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var emailKey = model.Email.ToLower();
                var storedOtp = HttpContext.Session.GetString($"OtpCode_{emailKey}");
                var storedEmail = HttpContext.Session.GetString($"OtpEmail_{emailKey}");
                var storedUserId = HttpContext.Session.GetString($"OtpUserId_{emailKey}");
                var expiryString = HttpContext.Session.GetString($"OtpExpiry_{emailKey}");

                // Kiểm tra mã OTP có tồn tại không
                if (string.IsNullOrEmpty(storedOtp) || string.IsNullOrEmpty(storedEmail) || string.IsNullOrEmpty(storedUserId))
                {
                    TempData["ErrorMessage"] = "Mã xác nhận không hợp lệ hoặc đã hết hạn. Vui lòng yêu cầu lại.";
                    return RedirectToAction("ForgotPassword");
                }

                // Kiểm tra email khớp
                if (storedEmail.ToLower() != model.Email.ToLower())
                {
                    TempData["ErrorMessage"] = "Email không khớp.";
                    return RedirectToAction("ForgotPassword");
                }

                // Kiểm tra thời gian hết hạn
                if (DateTime.TryParse(expiryString, out DateTime expiry) && DateTime.UtcNow > expiry)
                {
                    TempData["ErrorMessage"] = "Mã xác nhận đã hết hạn. Vui lòng yêu cầu lại.";
                    // Xóa session
                    HttpContext.Session.Remove($"OtpCode_{emailKey}");
                    HttpContext.Session.Remove($"OtpEmail_{emailKey}");
                    HttpContext.Session.Remove($"OtpUserId_{emailKey}");
                    HttpContext.Session.Remove($"OtpExpiry_{emailKey}");
                    return RedirectToAction("ForgotPassword");
                }

                // Kiểm tra mã OTP
                if (storedOtp != model.OtpCode.Trim())
                {
                    ModelState.AddModelError("OtpCode", "Mã xác nhận không đúng. Vui lòng thử lại.");
                    return View(model);
                }

                // Mã OTP đúng, đánh dấu đã xác thực và chuyển đến trang đổi mật khẩu
                HttpContext.Session.SetString($"OtpVerified_{emailKey}", "true");
                TempData["SuccessMessage"] = "Xác thực thành công! Vui lòng nhập mật khẩu mới.";

                return RedirectToAction("ChangePassword", new { email = model.Email });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying OTP for email: {Email}", model.Email);
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi xác thực mã. Vui lòng thử lại sau.";
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult ChangePassword(string? email)
        {
            if (string.IsNullOrEmpty(email))
            {
                TempData["ErrorMessage"] = "Email không hợp lệ.";
                return RedirectToAction("ForgotPassword");
            }

            // Kiểm tra đã verify OTP chưa
            var emailKey = email.ToLower();
            var otpVerified = HttpContext.Session.GetString($"OtpVerified_{emailKey}");
            
            if (otpVerified != "true")
            {
                TempData["ErrorMessage"] = "Vui lòng xác thực mã OTP trước.";
                return RedirectToAction("VerifyOtp", new { email = email });
            }

            var model = new ChangePasswordViewModel
            {
                Email = email
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var emailKey = model.Email.ToLower();
                
                // Kiểm tra đã verify OTP chưa
                var otpVerified = HttpContext.Session.GetString($"OtpVerified_{emailKey}");
                if (otpVerified != "true")
                {
                    TempData["ErrorMessage"] = "Phiên làm việc đã hết hạn. Vui lòng yêu cầu lại mã xác nhận.";
                    return RedirectToAction("ForgotPassword");
                }

                var storedUserId = HttpContext.Session.GetString($"OtpUserId_{emailKey}");
                if (string.IsNullOrEmpty(storedUserId) || !int.TryParse(storedUserId, out int userId))
                {
                    TempData["ErrorMessage"] = "Thông tin không hợp lệ. Vui lòng yêu cầu lại mã xác nhận.";
                    return RedirectToAction("ForgotPassword");
                }

                // Tìm người dùng
                var nguoiDung = await _context.NguoiDungs.FindAsync(userId);
                if (nguoiDung == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy tài khoản.";
                    return RedirectToAction("ForgotPassword");
                }

                // Kiểm tra email khớp
                if (nguoiDung.Email?.Trim().ToLower() != model.Email.Trim().ToLower())
                {
                    TempData["ErrorMessage"] = "Email không khớp với tài khoản.";
                    return RedirectToAction("ForgotPassword");
                }

                // Cập nhật mật khẩu
                nguoiDung.MatKhauHash = model.NewPassword.Trim();
                nguoiDung.NgayCapNhat = DateTime.Now;

                await _context.SaveChangesAsync();

                // Xóa tất cả session liên quan đến OTP
                HttpContext.Session.Remove($"OtpCode_{emailKey}");
                HttpContext.Session.Remove($"OtpEmail_{emailKey}");
                HttpContext.Session.Remove($"OtpUserId_{emailKey}");
                HttpContext.Session.Remove($"OtpExpiry_{emailKey}");
                HttpContext.Session.Remove($"OtpVerified_{emailKey}");

                _logger.LogInformation("Password changed successfully for user: {Email}", model.Email);
                TempData["SuccessMessage"] = "Đổi mật khẩu thành công! Vui lòng đăng nhập với mật khẩu mới.";

                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for email: {Email}", model.Email);
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi đổi mật khẩu. Vui lòng thử lại sau.";
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult ResetPassword(string? token, string? email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                TempData["ErrorMessage"] = "Link đặt lại mật khẩu không hợp lệ.";
                return RedirectToAction("Login");
            }

            var model = new ResetPasswordViewModel
            {
                Token = token,
                Email = email
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Xác thực token
                if (!ValidatePasswordResetToken(model.Token, model.Email, out int? userId))
                {
                    TempData["ErrorMessage"] = "Link đặt lại mật khẩu không hợp lệ hoặc đã hết hạn. Vui lòng yêu cầu lại.";
                    return RedirectToAction("ForgotPassword");
                }

                if (!userId.HasValue)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin người dùng.";
                    return RedirectToAction("ForgotPassword");
                }

                // Tìm người dùng
                var nguoiDung = await _context.NguoiDungs.FindAsync(userId.Value);
                if (nguoiDung == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy tài khoản.";
                    return RedirectToAction("ForgotPassword");
                }

                // Kiểm tra email khớp
                if (nguoiDung.Email?.Trim().ToLower() != model.Email.Trim().ToLower())
                {
                    TempData["ErrorMessage"] = "Email không khớp với tài khoản.";
                    return RedirectToAction("ForgotPassword");
                }

                // Cập nhật mật khẩu (lưu plain text như hệ thống hiện tại)
                nguoiDung.MatKhauHash = model.NewPassword.Trim();
                nguoiDung.NgayCapNhat = DateTime.Now;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Password reset successfully for user: {Email}", model.Email);
                TempData["SuccessMessage"] = "Đặt lại mật khẩu thành công! Vui lòng đăng nhập với mật khẩu mới.";

                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for email: {Email}", model.Email);
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi đặt lại mật khẩu. Vui lòng thử lại sau.";
                return View(model);
            }
        }

        private string GeneratePasswordResetToken(string email, int userId)
        {
            // Tạo token từ email, userId và timestamp
            var timestamp = DateTime.UtcNow.Ticks;
            var data = $"{email}|{userId}|{timestamp}";
            
            // Mã hóa token
            var key = _configuration["PasswordReset:SecretKey"] ?? "DefaultSecretKeyForPasswordReset123!@#";
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
                var token = Convert.ToBase64String(hash) + "|" + Convert.ToBase64String(Encoding.UTF8.GetBytes(data));
                return Convert.ToBase64String(Encoding.UTF8.GetBytes(token));
            }
        }

        private bool ValidatePasswordResetToken(string token, string email, out int? userId)
        {
            userId = null;

            try
            {
                // Giải mã token
                var decodedBytes = Convert.FromBase64String(token);
                var decodedToken = Encoding.UTF8.GetString(decodedBytes);
                
                var parts = decodedToken.Split('|');
                if (parts.Length != 2)
                {
                    return false;
                }

                var hashPart = parts[0];
                var dataPart = parts[1];

                // Giải mã data
                var dataBytes = Convert.FromBase64String(dataPart);
                var data = Encoding.UTF8.GetString(dataBytes);
                
                var dataParts = data.Split('|');
                if (dataParts.Length != 3)
                {
                    return false;
                }

                var tokenEmail = dataParts[0];
                var tokenUserId = dataParts[1];
                var timestamp = long.Parse(dataParts[2]);

                // Kiểm tra email khớp
                if (tokenEmail.Trim().ToLower() != email.Trim().ToLower())
                {
                    return false;
                }

                // Kiểm tra thời gian hết hạn (24 giờ)
                var tokenTime = new DateTime(timestamp);
                if (DateTime.UtcNow - tokenTime > TimeSpan.FromHours(24))
                {
                    return false;
                }

                // Xác thực hash
                var key = _configuration["PasswordReset:SecretKey"] ?? "DefaultSecretKeyForPasswordReset123!@#";
                using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key)))
                {
                    var expectedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
                    var expectedHashString = Convert.ToBase64String(expectedHash);
                    
                    if (hashPart != expectedHashString)
                    {
                        return false;
                    }
                }

                userId = int.Parse(tokenUserId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating password reset token");
                return false;
            }
        }
    }
}

