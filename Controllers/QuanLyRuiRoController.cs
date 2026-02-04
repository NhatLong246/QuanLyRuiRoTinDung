using Microsoft.AspNetCore.Mvc;
using QuanLyRuiRoTinDung.Services;

namespace QuanLyRuiRoTinDung.Controllers
{
    public class QuanLyRuiRoController : Controller
    {
        private readonly IRuiRoService _ruiRoService;

        public QuanLyRuiRoController(IRuiRoService ruiRoService)
        {
            _ruiRoService = ruiRoService;
        }
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

        // API Endpoints
        [HttpGet]
        public async Task<IActionResult> GetKhoanVayCanThamDinh(string? maKhoanVay = null, string? trangThai = null, string? mucDoRuiRo = null)
        {
            try
            {
                var data = await _ruiRoService.GetKhoanVayCanThamDinhAsync(maKhoanVay, trangThai, mucDoRuiRo);
                return Json(new { success = true, data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetKhoanVayDetail(int maKhoanVay)
        {
            try
            {
                var khoanVay = await _ruiRoService.GetKhoanVayDetailAsync(maKhoanVay);
                if (khoanVay == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy khoản vay" });
                }

                return Json(new { success = true, data = khoanVay });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetDanhGiaRuiRo(int maKhoanVay)
        {
            try
            {
                var danhGia = await _ruiRoService.GetDanhGiaRuiRoByKhoanVayAsync(maKhoanVay);
                return Json(new { success = true, data = danhGia });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetKhoanVayFullDetail(int maKhoanVay)
        {
            try
            {
                // Kiểm tra session
                var maNguoiDung = HttpContext.Session.GetString("MaNguoiDung");
                if (string.IsNullOrEmpty(maNguoiDung))
                {
                    return Json(new { success = false, message = "Phiên đăng nhập hết hạn. Vui lòng đăng nhập lại." });
                }

                var data = await _ruiRoService.GetKhoanVayFullDetailAsync(maKhoanVay);
                if (data == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy khoản vay" });
                }

                return Json(new { success = true, data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveDanhGiaRuiRo([FromBody] DanhGiaRuiRoRequest request)
        {
            try
            {
                var maNguoiDung = HttpContext.Session.GetString("MaNguoiDung");
                if (string.IsNullOrEmpty(maNguoiDung))
                {
                    return Json(new { success = false, message = "Phiên làm việc hết hạn" });
                }

                var danhGia = new Models.Entities.DanhGiaRuiRo
                {
                    MaKhoanVay = request.MaKhoanVay,
                    TongDiem = request.TongDiem,
                    MucDoRuiRo = request.MucDoRuiRo,
                    XepHangRuiRo = request.XepHangRuiRo,
                    KienNghi = request.KienNghi,
                    NhanXet = request.NhanXet,
                    TrangThai = request.TrangThai
                };

                var result = await _ruiRoService.CreateOrUpdateDanhGiaRuiRoAsync(danhGia, int.Parse(maNguoiDung));
                
                return Json(new { success = true, data = result, message = "Lưu đánh giá thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetGiaTriThamChieu(string loaiTaiSan, string? keyword = null)
        {
            try
            {
                var data = await _ruiRoService.GetGiaTriThamChieuAsync(loaiTaiSan, keyword);
                return Json(new { success = true, data = data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> TimGiaTriThamChieu([FromBody] TimGiaTriRequest request)
        {
            try
            {
                var data = await _ruiRoService.TimGiaTriThamChieuAsync(
                    request.LoaiTaiSan, 
                    request.Quan, 
                    request.HangXe, 
                    request.DongXe, 
                    request.NamSanXuat);
                return Json(new { success = true, data = data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetFileDinhKem(int maKhoanVay)
        {
            try
            {
                var files = await _ruiRoService.GetFileDinhKemByKhoanVayAsync(maKhoanVay);
                return Json(new { success = true, data = files });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> LuuKetQuaThamDinh([FromBody] ThamDinhRequest request)
        {
            try
            {
                var maNguoiDung = HttpContext.Session.GetString("MaNguoiDung");
                if (string.IsNullOrEmpty(maNguoiDung))
                {
                    return Json(new { success = false, message = "Phiên đăng nhập hết hạn" });
                }

                var result = await _ruiRoService.LuuKetQuaThamDinhAsync(
                    request.MaTaiSan,
                    request.GiaTriThamChieu,
                    request.GiaTriThamDinh,
                    request.TyLeThamDinh,
                    request.GhiChu,
                    int.Parse(maNguoiDung),
                    request.SoGiayTo,
                    request.NgayCap,
                    request.NoiCap,
                    request.ChuSoHuu,
                    request.DiaChi,
                    request.ThanhPho,
                    request.Quan,
                    request.TinhTrang
                );

                return Json(new { success = result, message = result ? "Đã lưu kết quả thẩm định" : "Không thể lưu kết quả" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateTaiSanThamDinh([FromBody] UpdateTaiSanThamDinhRequest request)
        {
            try
            {
                var result = await _ruiRoService.UpdateTaiSanThamDinhAsync(
                    request.MaLienKet,
                    request.GiaTriDinhGia,
                    request.TyLeTheChap,
                    request.NgayTheChap,
                    request.GhiChu
                );

                return Json(new { success = result, message = result ? "Đã cập nhật thông tin tài sản" : "Không thể cập nhật" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }

    public class DanhGiaRuiRoRequest
    {
        public int MaKhoanVay { get; set; }
        public decimal? TongDiem { get; set; }
        public string? MucDoRuiRo { get; set; }
        public string? XepHangRuiRo { get; set; }
        public string? KienNghi { get; set; }
        public string? NhanXet { get; set; }
        public string? TrangThai { get; set; }
    }

    public class TimGiaTriRequest
    {
        public string LoaiTaiSan { get; set; } = "";
        public string? Quan { get; set; }
        public string? HangXe { get; set; }
        public string? DongXe { get; set; }
        public int? NamSanXuat { get; set; }
    }

    public class ThamDinhRequest
    {
        public int MaTaiSan { get; set; }
        public decimal GiaTriThamChieu { get; set; }
        public decimal GiaTriThamDinh { get; set; }
        public decimal TyLeThamDinh { get; set; }
        public string? GhiChu { get; set; }
        public string? SoGiayTo { get; set; }
        public DateOnly? NgayCap { get; set; }
        public string? NoiCap { get; set; }
        public string? ChuSoHuu { get; set; }
        public string? DiaChi { get; set; }
        public string? ThanhPho { get; set; }
        public string? Quan { get; set; }
        public string? TinhTrang { get; set; }
    }

    public class UpdateTaiSanThamDinhRequest
    {
        public int MaLienKet { get; set; }
        public decimal? GiaTriDinhGia { get; set; }
        public decimal? TyLeTheChap { get; set; }
        public DateOnly? NgayTheChap { get; set; }
        public string? GhiChu { get; set; }
    }
}
