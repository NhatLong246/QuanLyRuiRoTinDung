using Microsoft.AspNetCore.Mvc;
using QuanLyRuiRoTinDung.Services;
using ClosedXML.Excel;

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

        [HttpGet]
        public async Task<IActionResult> GetPhanLoaiMucDoRuiRo(string? maKhoanVay = null, string? tenKhachHang = null, 
            decimal? diemTu = null, decimal? diemDen = null, string? mucDoRuiRo = null)
        {
            try
            {
                var data = await _ruiRoService.GetPhanLoaiMucDoRuiRoAsync(maKhoanVay, tenKhachHang, diemTu, diemDen, mucDoRuiRo);
                return Json(new { success = true, data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetThongKePhanLoaiRuiRo()
        {
            try
            {
                var data = await _ruiRoService.GetThongKePhanLoaiRuiRoAsync();
                return Json(new { success = true, data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
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

        // API: Lấy tổng quan danh mục tín dụng
        [HttpGet]
        public async Task<IActionResult> GetDanhMucTinDungTongQuat()
        {
            try
            {
                var data = await _ruiRoService.GetDanhMucTinDungTongQuatAsync();
                return Json(new { success = true, data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // API: Lấy danh sách lịch sử danh mục tín dụng
        [HttpGet]
        public async Task<IActionResult> GetDanhMucTinDungList(string? kyThoiGian = null)
        {
            try
            {
                var data = await _ruiRoService.GetDanhMucTinDungListAsync(kyThoiGian);
                return Json(new { success = true, data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // API: Cập nhật danh mục tín dụng
        [HttpPost]
        public async Task<IActionResult> CapNhatDanhMucTinDung()
        {
            try
            {
                var maNguoiDung = HttpContext.Session.GetString("MaNguoiDung");
                if (string.IsNullOrEmpty(maNguoiDung))
                {
                    return Json(new { success = false, message = "Phiên đăng nhập hết hạn" });
                }

                var data = await _ruiRoService.CapNhatDanhMucTinDungAsync(int.Parse(maNguoiDung));
                return Json(new { success = true, data, message = "Cập nhật danh mục tín dụng thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // API: Xuất báo cáo danh mục tín dụng ra Excel
        [HttpGet]
        public async Task<IActionResult> XuatBaoCaoDanhMucTinDung(string? kyThoiGian = null)
        {
            try
            {
                var danhMucList = await _ruiRoService.GetDanhMucTinDungListAsync(kyThoiGian);
                var tongQuat = await _ruiRoService.GetDanhMucTinDungTongQuatAsync();

                using (var workbook = new XLWorkbook())
                {
                    // Sheet 1: Tổng quan
                    var wsTongQuat = workbook.Worksheets.Add("Tổng quan");
                    
                    // Tiêu đề
                    wsTongQuat.Cell(1, 1).Value = "BÁO CÁO DANH MỤC TÍN DỤNG";
                    wsTongQuat.Range(1, 1, 1, 4).Merge().Style.Font.Bold = true;
                    wsTongQuat.Range(1, 1, 1, 4).Style.Font.FontSize = 16;
                    wsTongQuat.Range(1, 1, 1, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    
                    wsTongQuat.Cell(2, 1).Value = $"Ngày xuất báo cáo: {DateTime.Now:dd/MM/yyyy HH:mm}";
                    wsTongQuat.Range(2, 1, 2, 4).Merge();
                    wsTongQuat.Range(2, 1, 2, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    // Thông tin tổng quan
                    wsTongQuat.Cell(4, 1).Value = "CHỈ TIÊU";
                    wsTongQuat.Cell(4, 2).Value = "GIÁ TRỊ";
                    wsTongQuat.Range(4, 1, 4, 2).Style.Font.Bold = true;
                    wsTongQuat.Range(4, 1, 4, 2).Style.Fill.BackgroundColor = XLColor.LightBlue;

                    wsTongQuat.Cell(5, 1).Value = "Tổng số khoản vay";
                    wsTongQuat.Cell(5, 2).Value = tongQuat.TongSoKhoanVay;

                    wsTongQuat.Cell(6, 1).Value = "Tổng số tiền vay (VNĐ)";
                    wsTongQuat.Cell(6, 2).Value = tongQuat.TongSoTienVay;
                    wsTongQuat.Cell(6, 2).Style.NumberFormat.Format = "#,##0";

                    wsTongQuat.Cell(7, 1).Value = "Tổng dư nợ (VNĐ)";
                    wsTongQuat.Cell(7, 2).Value = tongQuat.TongDuNo;
                    wsTongQuat.Cell(7, 2).Style.NumberFormat.Format = "#,##0";

                    wsTongQuat.Cell(8, 1).Value = "Tỷ lệ nợ xấu (%)";
                    wsTongQuat.Cell(8, 2).Value = tongQuat.TyLeNoXau;
                    wsTongQuat.Cell(8, 2).Style.NumberFormat.Format = "0.00";

                    wsTongQuat.Cell(9, 1).Value = "Điểm rủi ro trung bình";
                    wsTongQuat.Cell(9, 2).Value = tongQuat.DiemRuiRoTrungBinh;
                    wsTongQuat.Cell(9, 2).Style.NumberFormat.Format = "0.0";

                    wsTongQuat.Cell(11, 1).Value = "PHÂN LOẠI RỦI RO";
                    wsTongQuat.Range(11, 1, 11, 2).Merge().Style.Font.Bold = true;

                    wsTongQuat.Cell(12, 1).Value = "Rủi ro Thấp";
                    wsTongQuat.Cell(12, 2).Value = tongQuat.SoKhoanVayRuiRoThap;

                    wsTongQuat.Cell(13, 1).Value = "Rủi ro Trung bình";
                    wsTongQuat.Cell(13, 2).Value = tongQuat.SoKhoanVayRuiRoTrungBinh;

                    wsTongQuat.Cell(14, 1).Value = "Rủi ro Cao";
                    wsTongQuat.Cell(14, 2).Value = tongQuat.SoKhoanVayRuiRoCao;

                    wsTongQuat.Columns().AdjustToContents();

                    // Sheet 2: Chi tiết theo thời gian
                    var wsChiTiet = workbook.Worksheets.Add("Chi tiết theo thời gian");
                    
                    // Header
                    wsChiTiet.Cell(1, 1).Value = "Ngày danh mục";
                    wsChiTiet.Cell(1, 2).Value = "Tổng số khoản vay";
                    wsChiTiet.Cell(1, 3).Value = "Tổng số tiền vay (VNĐ)";
                    wsChiTiet.Cell(1, 4).Value = "Tổng dư nợ (VNĐ)";
                    wsChiTiet.Cell(1, 5).Value = "Tỷ lệ nợ xấu (%)";
                    wsChiTiet.Cell(1, 6).Value = "Điểm rủi ro TB";
                    wsChiTiet.Cell(1, 7).Value = "Rủi ro Thấp";
                    wsChiTiet.Cell(1, 8).Value = "Rủi ro TB";
                    wsChiTiet.Cell(1, 9).Value = "Rủi ro Cao";

                    wsChiTiet.Range(1, 1, 1, 9).Style.Font.Bold = true;
                    wsChiTiet.Range(1, 1, 1, 9).Style.Fill.BackgroundColor = XLColor.LightBlue;
                    wsChiTiet.Range(1, 1, 1, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    // Data rows
                    int row = 2;
                    foreach (var item in danhMucList)
                    {
                        wsChiTiet.Cell(row, 1).Value = item.NgayDanhMuc.ToString("dd/MM/yyyy");
                        wsChiTiet.Cell(row, 2).Value = item.TongSoKhoanVay ?? 0;
                        wsChiTiet.Cell(row, 3).Value = item.TongSoTienVay ?? 0;
                        wsChiTiet.Cell(row, 3).Style.NumberFormat.Format = "#,##0";
                        wsChiTiet.Cell(row, 4).Value = item.TongDuNo ?? 0;
                        wsChiTiet.Cell(row, 4).Style.NumberFormat.Format = "#,##0";
                        wsChiTiet.Cell(row, 5).Value = item.TyLeNoXau ?? 0;
                        wsChiTiet.Cell(row, 5).Style.NumberFormat.Format = "0.00";
                        wsChiTiet.Cell(row, 6).Value = item.DiemRuiRoTrungBinh ?? 0;
                        wsChiTiet.Cell(row, 6).Style.NumberFormat.Format = "0.0";
                        wsChiTiet.Cell(row, 7).Value = item.SoKhoanVayRuiRoThap ?? 0;
                        wsChiTiet.Cell(row, 8).Value = item.SoKhoanVayRuiRoTrungBinh ?? 0;
                        wsChiTiet.Cell(row, 9).Value = item.SoKhoanVayRuiRoCao ?? 0;
                        row++;
                    }

                    // Nếu không có dữ liệu lịch sử, thêm dòng dữ liệu hiện tại
                    if (!danhMucList.Any())
                    {
                        wsChiTiet.Cell(row, 1).Value = DateTime.Now.ToString("dd/MM/yyyy");
                        wsChiTiet.Cell(row, 2).Value = tongQuat.TongSoKhoanVay;
                        wsChiTiet.Cell(row, 3).Value = tongQuat.TongSoTienVay;
                        wsChiTiet.Cell(row, 3).Style.NumberFormat.Format = "#,##0";
                        wsChiTiet.Cell(row, 4).Value = tongQuat.TongDuNo;
                        wsChiTiet.Cell(row, 4).Style.NumberFormat.Format = "#,##0";
                        wsChiTiet.Cell(row, 5).Value = tongQuat.TyLeNoXau;
                        wsChiTiet.Cell(row, 5).Style.NumberFormat.Format = "0.00";
                        wsChiTiet.Cell(row, 6).Value = tongQuat.DiemRuiRoTrungBinh;
                        wsChiTiet.Cell(row, 6).Style.NumberFormat.Format = "0.0";
                        wsChiTiet.Cell(row, 7).Value = tongQuat.SoKhoanVayRuiRoThap;
                        wsChiTiet.Cell(row, 8).Value = tongQuat.SoKhoanVayRuiRoTrungBinh;
                        wsChiTiet.Cell(row, 9).Value = tongQuat.SoKhoanVayRuiRoCao;
                    }

                    wsChiTiet.Columns().AdjustToContents();

                    // Tạo border cho bảng
                    wsChiTiet.Range(1, 1, row - 1, 9).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    wsChiTiet.Range(1, 1, row - 1, 9).Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    // Xuất file
                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        var content = stream.ToArray();
                        var fileName = $"BaoCao_DanhMucTinDung_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
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
                // Lấy mã người dùng hiện tại từ session
                var maNguoiDungStr = HttpContext.Session.GetString("MaNguoiDung");
                int? maNguoiDung = null;
                if (!string.IsNullOrEmpty(maNguoiDungStr) && int.TryParse(maNguoiDungStr, out var parsedId))
                {
                    maNguoiDung = parsedId;
                }

                var data = await _ruiRoService.GetKhoanVayCanThamDinhAsync(maKhoanVay, trangThai, mucDoRuiRo, maNguoiDung);
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

                // Cắt ngắn các trường nếu vượt quá giới hạn database
                var nhanXet = request.NhanXet;
                if (!string.IsNullOrEmpty(nhanXet) && nhanXet.Length > 1000)
                {
                    nhanXet = nhanXet.Substring(0, 997) + "...";
                }

                var danhGia = new Models.Entities.DanhGiaRuiRo
                {
                    MaKhoanVay = request.MaKhoanVay,
                    TongDiem = request.TongDiem,
                    MucDoRuiRo = request.MucDoRuiRo,
                    XepHangRuiRo = request.XepHangRuiRo,
                    KienNghi = request.KienNghi,
                    NhanXet = nhanXet,
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
                    request.TinhTrang,
                    request.DienTich
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

        [HttpGet]
        public async Task<IActionResult> GetTaiSanDetail(int maTaiSan)
        {
            try
            {
                var taiSan = await _ruiRoService.GetTaiSanDetailAsync(maTaiSan);
                if (taiSan == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy tài sản" });
                }
                
                return Json(new { success = true, data = taiSan });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetThongTinCic(int maKhoanVay)
        {
            try
            {
                var maNguoiDung = HttpContext.Session.GetString("MaNguoiDung");
                if (string.IsNullOrEmpty(maNguoiDung))
                {
                    return Json(new { success = false, message = "Phiên đăng nhập hết hạn. Vui lòng đăng nhập lại." });
                }

                var cicInfo = await _ruiRoService.GetThongTinCicByKhoanVayAsync(maKhoanVay);
                if (cicInfo == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin CIC cho khách hàng này." });
                }

                return Json(new { success = true, data = cicInfo });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
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
        public decimal? DienTich { get; set; }
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
