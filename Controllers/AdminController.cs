using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyRuiRoTinDung.Models.EF;
using QuanLyRuiRoTinDung.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace QuanLyRuiRoTinDung.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(ApplicationDbContext context, ILogger<AdminController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Kiểm tra quyền Admin
        private bool CheckAdminPermission()
        {
            var maNguoiDungStr = HttpContext.Session.GetString("MaNguoiDung");
            var tenVaiTro = HttpContext.Session.GetString("TenVaiTro");
            
            if (string.IsNullOrEmpty(maNguoiDungStr))
                return false;
            
            return !string.IsNullOrWhiteSpace(tenVaiTro) && 
                   tenVaiTro.Trim().Equals("Admin", StringComparison.OrdinalIgnoreCase);
        }

        public async Task<IActionResult> Index()
        {
            // Kiểm tra đăng nhập và quyền Admin
            var maNguoiDungStr = HttpContext.Session.GetString("MaNguoiDung");
            var tenVaiTro = HttpContext.Session.GetString("TenVaiTro");
            
            if (string.IsNullOrEmpty(maNguoiDungStr))
            {
                return RedirectToAction("Login", "Account");
            }
            
            // Kiểm tra quyền với case-insensitive
            if (string.IsNullOrWhiteSpace(tenVaiTro) || 
                !tenVaiTro.Trim().Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "Bạn không có quyền truy cập trang này.";
                return RedirectToAction("Index", "Dashboard");
            }

            // Lấy thông tin người dùng từ session
            ViewBag.HoTen = HttpContext.Session.GetString("HoTen") ?? "Admin";
            ViewBag.TenVaiTro = tenVaiTro;

            // Lấy dữ liệu thống kê cho admin
            var dashboardData = await GetAdminDashboardData();
            
            return View(dashboardData);
        }

        private async Task<AdminDashboardViewModel> GetAdminDashboardData()
        {
            var viewModel = new AdminDashboardViewModel();

            try
            {
                // Tổng số người dùng
                viewModel.TongNguoiDung = await _context.NguoiDungs.CountAsync();
                
                // Số người dùng đang hoạt động
                viewModel.NguoiDungHoatDong = await _context.NguoiDungs
                    .Where(u => u.TrangThaiHoatDong == true)
                    .CountAsync();
                
                // Số người dùng đã khóa
                viewModel.NguoiDungKhoa = await _context.NguoiDungs
                    .Where(u => u.TrangThaiHoatDong == false)
                    .CountAsync();
                
                // Số người dùng mới trong tháng này
                var ngayDauThang = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                viewModel.NguoiDungMoiThangNay = await _context.NguoiDungs
                    .Where(u => u.NgayTao >= ngayDauThang)
                    .CountAsync();

                // Tổng số vai trò
                viewModel.TongVaiTro = await _context.VaiTros
                    .Where(v => v.TrangThaiHoatDong == true)
                    .CountAsync();

                // Tổng số phòng ban
                viewModel.TongPhongBan = await _context.PhongBans
                    .Where(p => p.TrangThaiHoatDong == true)
                    .CountAsync();

                // Tổng số quyền
                viewModel.TongQuyen = await _context.Quyens.CountAsync();

                // Tổng số cấu hình hệ thống
                viewModel.TongCauHinh = await _context.CauHinhHeThongs.CountAsync();

                // Phân bổ người dùng theo vai trò
                viewModel.PhanBoNguoiDungTheoVaiTro = await _context.NguoiDungs
                    .Include(u => u.MaVaiTroNavigation)
                    .Where(u => u.MaVaiTroNavigation != null)
                    .GroupBy(u => u.MaVaiTroNavigation!.TenVaiTro)
                    .Select(g => new PhanBoVaiTroViewModel
                    {
                        TenVaiTro = g.Key ?? "Chưa xác định",
                        SoLuong = g.Count()
                    })
                    .ToListAsync();

                // Người dùng mới nhất - sử dụng join để lấy thông tin phòng ban
                viewModel.NguoiDungMoiNhat = await (from u in _context.NguoiDungs
                    join v in _context.VaiTros on u.MaVaiTro equals v.MaVaiTro
                    join p in _context.PhongBans on u.MaPhongBan equals p.MaPhongBan into phongBanGroup
                    from pb in phongBanGroup.DefaultIfEmpty()
                    orderby u.NgayTao descending
                    select new NguoiDungMoiNhatViewModel
                    {
                        MaNguoiDung = u.MaNguoiDung,
                        TenDangNhap = u.TenDangNhap ?? "",
                        HoTen = u.HoTen ?? "",
                        TenVaiTro = v.TenVaiTro ?? "",
                        TenPhongBan = pb != null ? pb.TenPhongBan : "",
                        TrangThaiHoatDong = u.TrangThaiHoatDong ?? false,
                        NgayTao = u.NgayTao
                    })
                    .Take(10)
                    .ToListAsync();

                // Nhật ký hoạt động gần đây
                viewModel.NhatKyHoatDongGanDay = await _context.NhatKyHoatDongs
                    .Include(n => n.MaNguoiDungNavigation)
                    .OrderByDescending(n => n.ThoiGian)
                    .Take(10)
                    .Select(n => new NhatKyHoatDongViewModel
                    {
                        MaNhatKy = n.MaNhatKy,
                        TenDangNhap = n.MaNguoiDungNavigation != null ? n.MaNguoiDungNavigation.TenDangNhap : "",
                        HoTen = n.MaNguoiDungNavigation != null ? n.MaNguoiDungNavigation.HoTen : "",
                        HanhDong = n.HanhDong ?? "",
                        TenBang = n.TenBang ?? "",
                        ThoiGian = n.ThoiGian
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading admin dashboard data");
            }

            return viewModel;
        }

        // ============================================
        // 1. QUẢN LÝ NGƯỜI DÙNG
        // ============================================

        // Danh sách người dùng
        public async Task<IActionResult> QuanLyNguoiDung(string search = "", string role = "", bool? status = null)
        {
            if (!CheckAdminPermission())
            {
                TempData["Error"] = "Bạn không có quyền truy cập trang này.";
                return RedirectToAction("Index", "Dashboard");
            }

            var query = _context.NguoiDungs
                .Include(u => u.MaVaiTroNavigation)
                .AsQueryable();

            // Tìm kiếm
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(u => 
                    u.TenDangNhap.Contains(search) ||
                    u.HoTen.Contains(search) ||
                    (u.Email != null && u.Email.Contains(search)));
            }

            // Lọc theo vai trò
            if (!string.IsNullOrWhiteSpace(role))
            {
                query = query.Where(u => u.MaVaiTroNavigation != null && 
                    u.MaVaiTroNavigation.TenVaiTro == role);
            }

            // Lọc theo trạng thái
            if (status.HasValue)
            {
                query = query.Where(u => u.TrangThaiHoatDong == status.Value);
            }

            var nguoiDungs = await query
                .OrderByDescending(u => u.NgayTao)
                .ToListAsync();

            ViewBag.VaiTros = await _context.VaiTros
                .Where(v => v.TrangThaiHoatDong == true)
                .OrderBy(v => v.TenVaiTro)
                .ToListAsync();

            ViewBag.PhongBans = await _context.PhongBans
                .Where(p => p.TrangThaiHoatDong == true)
                .OrderBy(p => p.TenPhongBan)
                .ToListAsync();

            // Pass filter values to view
            ViewBag.Search = search;
            ViewBag.SelectedRole = role;
            ViewBag.SelectedStatus = status?.ToString();

            // Set user info for layout
            ViewBag.HoTen = HttpContext.Session.GetString("HoTen") ?? "Admin";
            ViewBag.TenVaiTro = HttpContext.Session.GetString("TenVaiTro") ?? "Admin";

            return View(nguoiDungs);
        }

        // Chi tiết người dùng
        public async Task<IActionResult> ChiTietNguoiDung(int id)
        {
            if (!CheckAdminPermission())
            {
                return Json(new { success = false, message = "Không có quyền truy cập" });
            }

            var nguoiDung = await _context.NguoiDungs
                .Include(u => u.MaVaiTroNavigation)
                .FirstOrDefaultAsync(u => u.MaNguoiDung == id);

            if (nguoiDung == null)
            {
                return Json(new { success = false, message = "Không tìm thấy người dùng" });
            }

            // Lấy phòng ban nếu có
            string tenPhongBan = "";
            if (nguoiDung.MaPhongBan.HasValue)
            {
                var phongBan = await _context.PhongBans.FindAsync(nguoiDung.MaPhongBan.Value);
                tenPhongBan = phongBan?.TenPhongBan ?? "";
            }

            // Lấy lịch sử đăng nhập
            var lichSuDangNhap = await _context.NhatKyHoatDongs
                .Where(n => n.MaNguoiDung == id && n.HanhDong == "Đăng nhập")
                .OrderByDescending(n => n.ThoiGian)
                .Take(10)
                .ToListAsync();

            return Json(new
            {
                success = true,
                data = new
                {
                    MaNguoiDung = nguoiDung.MaNguoiDung,
                    TenDangNhap = nguoiDung.TenDangNhap,
                    HoTen = nguoiDung.HoTen,
                    Email = nguoiDung.Email ?? "",
                    SoDienThoai = nguoiDung.SoDienThoai ?? "",
                    TenVaiTro = nguoiDung.MaVaiTroNavigation?.TenVaiTro ?? "",
                    TenPhongBan = tenPhongBan,
                    TrangThaiHoatDong = nguoiDung.TrangThaiHoatDong ?? false,
                    LanDangNhapCuoi = nguoiDung.LanDangNhapCuoi?.ToString("dd/MM/yyyy HH:mm") ?? "Chưa đăng nhập",
                    NgayTao = nguoiDung.NgayTao?.ToString("dd/MM/yyyy HH:mm") ?? "",
                    LichSuDangNhap = lichSuDangNhap.Select(n => new
                    {
                        ThoiGian = n.ThoiGian?.ToString("dd/MM/yyyy HH:mm") ?? "",
                        DiaChiIP = n.DiaChiIp ?? ""
                    })
                }
            });
        }

        // Khóa/Mở khóa tài khoản
        [HttpPost]
        public async Task<IActionResult> KhoaMoKhoaTaiKhoan(int id)
        {
            if (!CheckAdminPermission())
            {
                return Json(new { success = false, message = "Không có quyền thực hiện" });
            }

            var nguoiDung = await _context.NguoiDungs.FindAsync(id);
            if (nguoiDung == null)
            {
                return Json(new { success = false, message = "Không tìm thấy người dùng" });
            }

            var maNguoiDungStr = HttpContext.Session.GetString("MaNguoiDung");
            if (!int.TryParse(maNguoiDungStr, out int maNguoiDung))
            {
                return Json(new { success = false, message = "Lỗi xác thực" });
            }

            nguoiDung.TrangThaiHoatDong = !nguoiDung.TrangThaiHoatDong;
            nguoiDung.NgayCapNhat = DateTime.Now;
            nguoiDung.NguoiCapNhat = maNguoiDung;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Admin {MaAdmin} đã {Action} tài khoản {MaNguoiDung}", 
                maNguoiDung, nguoiDung.TrangThaiHoatDong == true ? "mở khóa" : "khóa", id);

            return Json(new { 
                success = true, 
                message = nguoiDung.TrangThaiHoatDong == true ? "Đã mở khóa tài khoản" : "Đã khóa tài khoản",
                trangThai = nguoiDung.TrangThaiHoatDong
            });
        }

        // Reset mật khẩu
        [HttpPost]
        public async Task<IActionResult> ResetMatKhau(int id, string matKhauMoi)
        {
            if (!CheckAdminPermission())
            {
                return Json(new { success = false, message = "Không có quyền thực hiện" });
            }

            if (string.IsNullOrWhiteSpace(matKhauMoi) || matKhauMoi.Length < 6)
            {
                return Json(new { success = false, message = "Mật khẩu phải có ít nhất 6 ký tự" });
            }

            var nguoiDung = await _context.NguoiDungs.FindAsync(id);
            if (nguoiDung == null)
            {
                return Json(new { success = false, message = "Không tìm thấy người dùng" });
            }

            var maNguoiDungStr = HttpContext.Session.GetString("MaNguoiDung");
            if (!int.TryParse(maNguoiDungStr, out int maNguoiDung))
            {
                return Json(new { success = false, message = "Lỗi xác thực" });
            }

            nguoiDung.MatKhauHash = matKhauMoi.Trim();
            nguoiDung.NgayCapNhat = DateTime.Now;
            nguoiDung.NguoiCapNhat = maNguoiDung;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Admin {MaAdmin} đã reset mật khẩu cho người dùng {MaNguoiDung}", 
                maNguoiDung, id);

            return Json(new { success = true, message = "Đã reset mật khẩu thành công" });
        }

        // Cập nhật thông tin người dùng
        [HttpPost]
        public async Task<IActionResult> CapNhatNguoiDung(int id, string hoTen, string email, string soDienThoai, int maVaiTro, int? maPhongBan)
        {
            if (!CheckAdminPermission())
            {
                return Json(new { success = false, message = "Không có quyền thực hiện" });
            }

            var nguoiDung = await _context.NguoiDungs.FindAsync(id);
            if (nguoiDung == null)
            {
                return Json(new { success = false, message = "Không tìm thấy người dùng" });
            }

            var maNguoiDungStr = HttpContext.Session.GetString("MaNguoiDung");
            if (!int.TryParse(maNguoiDungStr, out int maNguoiDung))
            {
                return Json(new { success = false, message = "Lỗi xác thực" });
            }

            nguoiDung.HoTen = hoTen.Trim();
            nguoiDung.Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
            nguoiDung.SoDienThoai = string.IsNullOrWhiteSpace(soDienThoai) ? null : soDienThoai.Trim();
            nguoiDung.MaVaiTro = maVaiTro;
            nguoiDung.MaPhongBan = maPhongBan;
            nguoiDung.NgayCapNhat = DateTime.Now;
            nguoiDung.NguoiCapNhat = maNguoiDung;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Admin {MaAdmin} đã cập nhật thông tin người dùng {MaNguoiDung}", 
                maNguoiDung, id);

            return Json(new { success = true, message = "Đã cập nhật thông tin thành công" });
        }

        // ============================================
        // 2. QUẢN LÝ VAI TRÒ & QUYỀN HẠN
        // ============================================

        // Danh sách vai trò
        public async Task<IActionResult> QuanLyVaiTro()
        {
            if (!CheckAdminPermission())
            {
                TempData["Error"] = "Bạn không có quyền truy cập trang này.";
                return RedirectToAction("Index", "Dashboard");
            }

            var vaiTros = await _context.VaiTros
                .OrderBy(v => v.TenVaiTro)
                .ToListAsync();

            return View(vaiTros);
        }

        // Chi tiết vai trò và quyền hạn
        public async Task<IActionResult> ChiTietVaiTro(int id)
        {
            if (!CheckAdminPermission())
            {
                return Json(new { success = false, message = "Không có quyền truy cập" });
            }

            var vaiTro = await _context.VaiTros.FindAsync(id);
            if (vaiTro == null)
            {
                return Json(new { success = false, message = "Không tìm thấy vai trò" });
            }

            // Lấy danh sách quyền của vai trò
            var quyenCuaVaiTro = await _context.VaiTroQuyens
                .Include(vq => vq.MaQuyenNavigation)
                .Where(vq => vq.MaVaiTro == id)
                .Select(vq => new
                {
                    MaQuyen = vq.MaQuyen,
                    TenQuyen = vq.MaQuyenNavigation.TenQuyen,
                    PhanHe = vq.MaQuyenNavigation.PhanHe,
                    DuocXem = vq.DuocXem ?? false,
                    DuocThem = vq.DuocThem ?? false,
                    DuocSua = vq.DuocSua ?? false,
                    DuocXoa = vq.DuocXoa ?? false,
                    DuocPheDuyet = vq.DuocPheDuyet ?? false
                })
                .ToListAsync();

            // Lấy tất cả quyền trong hệ thống
            var tatCaQuyen = await _context.Quyens
                .OrderBy(q => q.PhanHe)
                .ThenBy(q => q.TenQuyen)
                .ToListAsync();

            return Json(new
            {
                success = true,
                data = new
                {
                    MaVaiTro = vaiTro.MaVaiTro,
                    TenVaiTro = vaiTro.TenVaiTro,
                    MoTa = vaiTro.MoTa ?? "",
                    TrangThaiHoatDong = vaiTro.TrangThaiHoatDong ?? false,
                    QuyenCuaVaiTro = quyenCuaVaiTro,
                    TatCaQuyen = tatCaQuyen.Select(q => new
                    {
                        MaQuyen = q.MaQuyen,
                        TenQuyen = q.TenQuyen,
                        PhanHe = q.PhanHe ?? "",
                        MoTa = q.MoTa ?? ""
                    })
                }
            });
        }

        // Cập nhật quyền cho vai trò
        [HttpPost]
        public async Task<IActionResult> CapNhatQuyenVaiTro(int maVaiTro, int maQuyen, bool duocXem, bool duocThem, bool duocSua, bool duocXoa, bool duocPheDuyet)
        {
            if (!CheckAdminPermission())
            {
                return Json(new { success = false, message = "Không có quyền thực hiện" });
            }

            var vaiTroQuyen = await _context.VaiTroQuyens
                .FirstOrDefaultAsync(vq => vq.MaVaiTro == maVaiTro && vq.MaQuyen == maQuyen);

            if (vaiTroQuyen == null)
            {
                // Tạo mới
                vaiTroQuyen = new VaiTroQuyen
                {
                    MaVaiTro = maVaiTro,
                    MaQuyen = maQuyen,
                    DuocXem = duocXem,
                    DuocThem = duocThem,
                    DuocSua = duocSua,
                    DuocXoa = duocXoa,
                    DuocPheDuyet = duocPheDuyet,
                    NgayTao = DateTime.Now
                };
                _context.VaiTroQuyens.Add(vaiTroQuyen);
            }
            else
            {
                // Cập nhật
                vaiTroQuyen.DuocXem = duocXem;
                vaiTroQuyen.DuocThem = duocThem;
                vaiTroQuyen.DuocSua = duocSua;
                vaiTroQuyen.DuocXoa = duocXoa;
                vaiTroQuyen.DuocPheDuyet = duocPheDuyet;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Admin đã cập nhật quyền cho vai trò {MaVaiTro}, quyền {MaQuyen}", 
                maVaiTro, maQuyen);

            return Json(new { success = true, message = "Đã cập nhật quyền thành công" });
        }

        // ============================================
        // 3. QUẢN LÝ CẤU HÌNH HỆ THỐNG
        // ============================================

        // Danh sách cấu hình
        public async Task<IActionResult> QuanLyCauHinh(string danhMuc = "")
        {
            if (!CheckAdminPermission())
            {
                TempData["Error"] = "Bạn không có quyền truy cập trang này.";
                return RedirectToAction("Index", "Dashboard");
            }

            var query = _context.CauHinhHeThongs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(danhMuc))
            {
                query = query.Where(c => c.DanhMuc == danhMuc);
            }

            var cauHinhs = await query
                .OrderBy(c => c.DanhMuc)
                .ThenBy(c => c.KhoaCauHinh)
                .ToListAsync();

            ViewBag.DanhMucs = await _context.CauHinhHeThongs
                .Select(c => c.DanhMuc)
                .Distinct()
                .Where(d => !string.IsNullOrEmpty(d))
                .OrderBy(d => d)
                .ToListAsync();

            return View(cauHinhs);
        }

        // Cập nhật cấu hình
        [HttpPost]
        public async Task<IActionResult> CapNhatCauHinh(int id, string giaTri)
        {
            if (!CheckAdminPermission())
            {
                return Json(new { success = false, message = "Không có quyền thực hiện" });
            }

            var cauHinh = await _context.CauHinhHeThongs.FindAsync(id);
            if (cauHinh == null)
            {
                return Json(new { success = false, message = "Không tìm thấy cấu hình" });
            }

            if (cauHinh.CoTheSua == false)
            {
                return Json(new { success = false, message = "Cấu hình này không được phép sửa" });
            }

            var maNguoiDungStr = HttpContext.Session.GetString("MaNguoiDung");
            if (!int.TryParse(maNguoiDungStr, out int maNguoiDung))
            {
                return Json(new { success = false, message = "Lỗi xác thực" });
            }

            cauHinh.GiaTriCauHinh = giaTri;
            cauHinh.NgayCapNhat = DateTime.Now;
            cauHinh.NguoiCapNhat = maNguoiDung;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Admin {MaAdmin} đã cập nhật cấu hình {KhoaCauHinh} = {GiaTri}", 
                maNguoiDung, cauHinh.KhoaCauHinh, giaTri);

            return Json(new { success = true, message = "Đã cập nhật cấu hình thành công" });
        }

        // ============================================
        // 4. QUẢN LÝ DANH MỤC
        // ============================================

        // Quản lý loại khách hàng
        public async Task<IActionResult> QuanLyLoaiKhachHang()
        {
            if (!CheckAdminPermission())
            {
                TempData["Error"] = "Bạn không có quyền truy cập trang này.";
                return RedirectToAction("Index", "Dashboard");
            }

            var loaiKhachHangs = await _context.LoaiKhachHangs
                .OrderBy(l => l.TenLoai)
                .ToListAsync();

            return View(loaiKhachHangs);
        }

        // Quản lý loại khoản vay
        public async Task<IActionResult> QuanLyLoaiKhoanVay()
        {
            if (!CheckAdminPermission())
            {
                TempData["Error"] = "Bạn không có quyền truy cập trang này.";
                return RedirectToAction("Index", "Dashboard");
            }

            var loaiKhoanVays = await _context.LoaiKhoanVays
                .OrderBy(l => l.TenLoaiVay)
                .ToListAsync();

            return View(loaiKhoanVays);
        }

        // Quản lý phân loại nợ
        public async Task<IActionResult> QuanLyPhanLoaiNo()
        {
            if (!CheckAdminPermission())
            {
                TempData["Error"] = "Bạn không có quyền truy cập trang này.";
                return RedirectToAction("Index", "Dashboard");
            }

            var phanLoaiNos = await _context.PhanLoaiNos
                .OrderBy(p => p.MaPhanLoai)
                .ToListAsync();

            return View(phanLoaiNos);
        }

        // ============================================
        // 5. GIÁM SÁT HỆ THỐNG
        // ============================================

        // Nhật ký hoạt động (Audit Log)
        public async Task<IActionResult> NhatKyHoatDong(string search = "", string hanhDong = "", string tenBang = "", DateTime? tuNgay = null, DateTime? denNgay = null)
        {
            if (!CheckAdminPermission())
            {
                TempData["Error"] = "Bạn không có quyền truy cập trang này.";
                return RedirectToAction("Index", "Dashboard");
            }

            var query = _context.NhatKyHoatDongs
                .Include(n => n.MaNguoiDungNavigation)
                .AsQueryable();

            // Tìm kiếm
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(n => 
                    (n.MaNguoiDungNavigation != null && n.MaNguoiDungNavigation.TenDangNhap.Contains(search)) ||
                    (n.MaNguoiDungNavigation != null && n.MaNguoiDungNavigation.HoTen.Contains(search)));
            }

            // Lọc theo hành động
            if (!string.IsNullOrWhiteSpace(hanhDong))
            {
                query = query.Where(n => n.HanhDong == hanhDong);
            }

            // Lọc theo bảng
            if (!string.IsNullOrWhiteSpace(tenBang))
            {
                query = query.Where(n => n.TenBang == tenBang);
            }

            // Lọc theo ngày
            if (tuNgay.HasValue)
            {
                query = query.Where(n => n.ThoiGian >= tuNgay.Value);
            }

            if (denNgay.HasValue)
            {
                query = query.Where(n => n.ThoiGian <= denNgay.Value.AddDays(1));
            }

            var nhatKys = await query
                .OrderByDescending(n => n.ThoiGian)
                .Take(500) // Giới hạn 500 bản ghi
                .ToListAsync();

            ViewBag.HanhDongs = await _context.NhatKyHoatDongs
                .Select(n => n.HanhDong)
                .Distinct()
                .Where(h => !string.IsNullOrEmpty(h))
                .OrderBy(h => h)
                .ToListAsync();

            ViewBag.TenBangs = await _context.NhatKyHoatDongs
                .Select(n => n.TenBang)
                .Distinct()
                .Where(t => !string.IsNullOrEmpty(t))
                .OrderBy(t => t)
                .ToListAsync();

            return View(nhatKys);
        }

        // Giám sát cảnh báo
        public async Task<IActionResult> GiamSatCanhBao(string search = "", string trangThai = "", string mucDo = "")
        {
            if (!CheckAdminPermission())
            {
                TempData["Error"] = "Bạn không có quyền truy cập trang này.";
                return RedirectToAction("Index", "Dashboard");
            }

            var query = _context.CanhBaos
                .Include(c => c.MaLoaiCanhBaoNavigation)
                .Include(c => c.MaKhoanVayNavigation)
                .Include(c => c.NguoiXuLyNavigation)
                .AsQueryable();

            // Tìm kiếm
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c => 
                    c.TieuDe.Contains(search) ||
                    (c.MaKhoanVayNavigation != null && c.MaKhoanVayNavigation.MaKhoanVayCode.Contains(search)));
            }

            // Lọc theo trạng thái
            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                query = query.Where(c => c.TrangThai == trangThai);
            }

            // Lọc theo mức độ
            if (!string.IsNullOrWhiteSpace(mucDo))
            {
                query = query.Where(c => c.MucDoNghiemTrong == mucDo);
            }

            var canhBaos = await query
                .OrderByDescending(c => c.NgayCanhBao)
                .ToListAsync();

            ViewBag.TrangThais = await _context.CanhBaos
                .Select(c => c.TrangThai)
                .Distinct()
                .Where(t => !string.IsNullOrEmpty(t))
                .OrderBy(t => t)
                .ToListAsync();

            ViewBag.MucDos = await _context.CanhBaos
                .Select(c => c.MucDoNghiemTrong)
                .Distinct()
                .Where(m => !string.IsNullOrEmpty(m))
                .OrderBy(m => m)
                .ToListAsync();

            return View(canhBaos);
        }
    }

    // ViewModel cho Admin Dashboard
    public class AdminDashboardViewModel
    {
        public int TongNguoiDung { get; set; }
        public int NguoiDungHoatDong { get; set; }
        public int NguoiDungKhoa { get; set; }
        public int NguoiDungMoiThangNay { get; set; }
        public int TongVaiTro { get; set; }
        public int TongPhongBan { get; set; }
        public int TongQuyen { get; set; }
        public int TongCauHinh { get; set; }
        public List<PhanBoVaiTroViewModel> PhanBoNguoiDungTheoVaiTro { get; set; } = new();
        public List<NguoiDungMoiNhatViewModel> NguoiDungMoiNhat { get; set; } = new();
        public List<NhatKyHoatDongViewModel> NhatKyHoatDongGanDay { get; set; } = new();
    }

    public class PhanBoVaiTroViewModel
    {
        public string TenVaiTro { get; set; } = string.Empty;
        public int SoLuong { get; set; }
    }

    public class NguoiDungMoiNhatViewModel
    {
        public int MaNguoiDung { get; set; }
        public string TenDangNhap { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public string TenVaiTro { get; set; } = string.Empty;
        public string TenPhongBan { get; set; } = string.Empty;
        public bool TrangThaiHoatDong { get; set; }
        public DateTime? NgayTao { get; set; }
    }

    public class NhatKyHoatDongViewModel
    {
        public int MaNhatKy { get; set; }
        public string TenDangNhap { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public string HanhDong { get; set; } = string.Empty;
        public string TenBang { get; set; } = string.Empty;
        public DateTime? ThoiGian { get; set; }
    }
}
