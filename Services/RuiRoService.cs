using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuanLyRuiRoTinDung.Models.EF;
using QuanLyRuiRoTinDung.Models.Entities;

namespace QuanLyRuiRoTinDung.Services
{
    public interface IRuiRoService
    {
        Task<List<KhoanVayThemDinhViewModel>> GetKhoanVayCanThamDinhAsync(string? maKhoanVay = null, string? trangThai = null, string? mucDoRuiRo = null, int? maNguoiDung = null);
        Task<KhoanVay?> GetKhoanVayDetailAsync(int maKhoanVay);
        Task<KhoanVayDetailViewModel?> GetKhoanVayFullDetailAsync(int maKhoanVay);
        Task<DanhGiaRuiRo?> GetDanhGiaRuiRoByKhoanVayAsync(int maKhoanVay);
        Task<DanhGiaRuiRo> CreateOrUpdateDanhGiaRuiRoAsync(DanhGiaRuiRo danhGia, int nguoiDanhGia);
        Task<List<TieuChiDanhGiaRuiRo>> GetTieuChiDanhGiaAsync();
        Task<List<ChiTietDanhGiaRuiRo>> GetChiTietDanhGiaAsync(int maDanhGia);
        Task<List<GiaTriTaiSanThamChieu>> GetGiaTriThamChieuAsync(string loaiTaiSan, string? keyword = null);
        Task<GiaTriTaiSanThamChieu?> TimGiaTriThamChieuAsync(string loaiTaiSan, string? quan = null, string? hangXe = null, string? dongXe = null, int? namSanXuat = null);
        Task<List<HoSoVayFileDinhKem>> GetFileDinhKemByKhoanVayAsync(int maKhoanVay);
        Task<object?> GetTaiSanDetailAsync(int maTaiSan);
        Task<bool> LuuKetQuaThamDinhAsync(int maTaiSan, decimal giaTriThamChieu, decimal giaTriThamDinh, decimal tyLeThamDinh, string? ghiChu, int nguoiThamDinh, 
            string? soGiayTo, DateOnly? ngayCap, string? noiCap, string? chuSoHuu, string? diaChi, string? thanhPho, string? quan, string? tinhTrang, decimal? dienTich);
        Task<bool> UpdateTaiSanThamDinhAsync(int maLienKet, decimal? giaTriDinhGia, decimal? tyLeTheChap, DateOnly? ngayTheChap, string? ghiChu);
        Task<ThongTinCicViewModel?> GetThongTinCicByKhoanVayAsync(int maKhoanVay);
        Task<List<PhanLoaiRuiRoViewModel>> GetPhanLoaiMucDoRuiRoAsync(string? maKhoanVay = null, string? tenKhachHang = null, decimal? diemTu = null, decimal? diemDen = null, string? mucDoRuiRo = null);
        Task<ThongKePhanLoaiRuiRoViewModel> GetThongKePhanLoaiRuiRoAsync();
        
        // Danh mục tín dụng
        Task<DanhMucTinDungTongQuatViewModel> GetDanhMucTinDungTongQuatAsync();
        Task<List<DanhMucTinDung>> GetDanhMucTinDungListAsync(string? kyThoiGian = null);
        Task<DanhMucTinDung?> CapNhatDanhMucTinDungAsync(int nguoiTao);
    }

    // ViewModel cho tổng quan danh mục tín dụng
    public class DanhMucTinDungTongQuatViewModel
    {
        public int TongSoKhoanVay { get; set; }
        public decimal TongDuNo { get; set; }
        public decimal TyLeNoXau { get; set; }
        public decimal DiemRuiRoTrungBinh { get; set; }
        public int SoKhoanVayRuiRoThap { get; set; }
        public int SoKhoanVayRuiRoTrungBinh { get; set; }
        public int SoKhoanVayRuiRoCao { get; set; }
        public decimal TongSoTienVay { get; set; }
        
        // So sánh với kỳ trước
        public int SoKhoanVayMoiThang { get; set; }
        public decimal TyLeTangDuNo { get; set; }
        public decimal ThayDoiTyLeNoXau { get; set; }
        public decimal ThayDoiDiemRuiRo { get; set; }
    }

    public class ThongTinCicViewModel
    {
        public int MaCic { get; set; }
        public string MaCicCode { get; set; } = null!;
        public string LoaiKhachHang { get; set; } = null!;
        public string? HoTen { get; set; }
        public string? SoCmndCccd { get; set; }
        public string? MaSoThue { get; set; }
        
        // Thông tin tín dụng
        public int TongSoKhoanVayCic { get; set; }
        public int SoKhoanVayDangVayCic { get; set; }
        public int SoKhoanVayDaTraXongCic { get; set; }
        public int SoKhoanVayQuaHanCic { get; set; }
        public int SoKhoanVayNoXauCic { get; set; }
        
        // Thông tin dư nợ
        public decimal TongDuNoCic { get; set; }
        public decimal DuNoQuaHanCic { get; set; }
        public decimal DuNoNoXauCic { get; set; }
        public decimal TongGiaTriVayCic { get; set; }
        
        // Điểm tín dụng
        public int? DiemTinDungCic { get; set; }
        public string? XepHangTinDungCic { get; set; }
        public string? MucDoRuiRo { get; set; }
        public string? KhaNangTraNo { get; set; }
        
        // Lịch sử
        public int SoLanQuaHanCic { get; set; }
        public int SoLanNoXauCic { get; set; }
        public int SoNgayQuaHanToiDaCic { get; set; }
        public decimal TyLeTraNoDungHanCic { get; set; }
        
        // Đánh giá
        public string? DanhGiaTongQuat { get; set; }
        public string? KhuyenNghiChoVay { get; set; }
        public string? LyDoKhuyenNghi { get; set; }
        
        public DateTime? NgayTraCuuCuoi { get; set; }
    }

    public class KhoanVayThemDinhViewModel
    {
        public int MaKhoanVay { get; set; }
        public string MaKhoanVayCode { get; set; } = null!;
        public string TenKhachHang { get; set; } = null!;
        public string LoaiKhachHang { get; set; } = null!;
        public decimal SoTienVay { get; set; }
        public DateTime? NgayNopHoSo { get; set; }
        public decimal? DiemRuiRo { get; set; }
        public string? MucDoRuiRo { get; set; }
        public string? XepHangRuiRo { get; set; }
        public string TrangThaiKhoanVay { get; set; } = null!;
        public string? TrangThaiDanhGia { get; set; }
        public DateTime? NgayDanhGia { get; set; }
        public int? NguoiDanhGia { get; set; }
    }

    public class KhoanVayDetailViewModel
    {
        public KhoanVayViewModel KhoanVay { get; set; } = null!;
        public KhachHangCaNhanViewModel? KhachHangCaNhan { get; set; }
        public KhachHangDoanhNghiepViewModel? KhachHangDoanhNghiep { get; set; }
        public LoaiVayViewModel? LoaiVay { get; set; }
        public List<TaiSanDamBaoViewModel> TaiSanDamBaos { get; set; } = new List<TaiSanDamBaoViewModel>();
        public DanhGiaRuiRoViewModel? DanhGiaRuiRo { get; set; }
        public List<TieuChiDanhGiaRuiRo> TieuChis { get; set; } = new List<TieuChiDanhGiaRuiRo>();
        public List<ChiTietDanhGiaRuiRo> ChiTietDanhGias { get; set; } = new List<ChiTietDanhGiaRuiRo>();
    }

    public class KhoanVayViewModel
    {
        public int MaKhoanVay { get; set; }
        public string MaKhoanVayCode { get; set; } = null!;
        public string LoaiKhachHang { get; set; } = null!;
        public int MaKhachHang { get; set; }
        public decimal SoTienVay { get; set; }
        public decimal LaiSuat { get; set; }
        public int KyHanVay { get; set; }
        public string? HinhThucTraNo { get; set; }
        public string? MucDichVay { get; set; }
        public DateTime? NgayNopHoSo { get; set; }
        public DateTime? NgayGiaiNgan { get; set; }
        public string TrangThaiKhoanVay { get; set; } = null!;
        public string? MucDoRuiRo { get; set; }
        public decimal? DiemRuiRo { get; set; }
        public string? XepHangRuiRo { get; set; }
    }

    public class LoaiVayViewModel
    {
        public int MaLoaiVay { get; set; }
        public string TenLoaiVay { get; set; } = null!;
        public string? MoTa { get; set; }
    }

    public class TaiSanDamBaoViewModel
    {
        public int MaTaiSan { get; set; }
        public int MaLienKet { get; set; }
        public string MaTaiSanCode { get; set; } = null!;
        public string? TenGoi { get; set; }
        public string? MoTaChiTiet { get; set; }
        
        // Thông tin loại tài sản
        public int MaLoaiTaiSan { get; set; }
        public string? TenLoaiTaiSan { get; set; }
        
        // Thông tin giá trị từ TaiSanDamBao
        public decimal? GiaTriDinhGia { get; set; }
        public string? TinhTrang { get; set; }
        
        // Thông tin từ KhoanVay_TaiSan (có thể chỉnh sửa)
        public decimal? GiaTriDinhGiaTaiThoiDiemVay { get; set; }
        public decimal? TyLeTheChap { get; set; }
        public DateOnly? NgayTheChap { get; set; }
        public DateOnly? NgayGiaiChap { get; set; }
        public string? TrangThaiTheChap { get; set; }
        public string? GhiChu { get; set; }
        
        // Thông tin tài sản loại "Khác"
        public string? TenTaiSanKhac { get; set; }
        public string? DonVi { get; set; }
        public decimal? SoLuong { get; set; }
        
        // Thông tin chi tiết cho các loại tài sản
        public string? SoGiayTo { get; set; }
        public string? DiaChi { get; set; }
        public string? Quan { get; set; }
        public decimal? DienTich { get; set; }
        public string? HangXe { get; set; }
        public string? DongXe { get; set; }
        public int? NamSanXuat { get; set; }
        public decimal? TrongLuong { get; set; }
    }

    public class DanhGiaRuiRoViewModel
    {
        public int MaDanhGia { get; set; }
        public decimal? TongDiem { get; set; }
        public string? MucDoRuiRo { get; set; }
        public string? XepHangRuiRo { get; set; }
        public string? KienNghi { get; set; }
        public string? NhanXet { get; set; }
        public string? TrangThai { get; set; }
        public DateTime? NgayDanhGia { get; set; }
    }

    public class KhachHangCaNhanViewModel
    {
        public string HoTen { get; set; } = null!;
        public DateOnly? NgaySinh { get; set; }
        public string? GioiTinh { get; set; }
        public string? SoCmnd { get; set; }
        public string? SoDienThoai { get; set; }
        public string? Email { get; set; }
        public string? DiaChi { get; set; }
        public string? NgheNghiep { get; set; }
        public decimal? ThuNhapHangThang { get; set; }
    }

    public class KhachHangDoanhNghiepViewModel
    {
        public string TenCongTy { get; set; } = null!;
        public string MaSoThue { get; set; } = null!;
        public string? NguoiDaiDienPhapLuat { get; set; }
        public string? SoDienThoai { get; set; }
        public string? Email { get; set; }
        public string? DiaChi { get; set; }
        public string? LinhVucKinhDoanh { get; set; }
        public decimal? VonDieuLe { get; set; }
        public decimal? DoanhThuHangNam { get; set; }
    }

    public class RuiRoService : IRuiRoService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RuiRoService> _logger;

        public RuiRoService(ApplicationDbContext context, ILogger<RuiRoService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<KhoanVayThemDinhViewModel>> GetKhoanVayCanThamDinhAsync(string? maKhoanVay = null, string? trangThai = null, string? mucDoRuiRo = null, int? maNguoiDung = null)
        {
            try
            {
                _logger.LogInformation("GetKhoanVayCanThamDinhAsync called with maKhoanVay={MaKhoanVay}, trangThai={TrangThai}, mucDoRuiRo={MucDoRuiRo}, maNguoiDung={MaNguoiDung}", 
                    maKhoanVay, trangThai, mucDoRuiRo, maNguoiDung);

                // Query các khoản vay đã được nộp hồ sơ (trạng thái "Đang xử lý", "Đang đánh giá", "Đã phê duyệt", "Từ chối")
                var query = _context.KhoanVays
                    .Where(k => k.TrangThaiKhoanVay != "Nháp" && k.NgayNopHoSo != null);

                var totalCount = await query.CountAsync();
                _logger.LogInformation("Total loans matching initial criteria: {Count}", totalCount);

                // Lọc theo mã khoản vay
                if (!string.IsNullOrEmpty(maKhoanVay))
                {
                    query = query.Where(k => k.MaKhoanVayCode.Contains(maKhoanVay));
                }

                // Lọc theo mức độ rủi ro
                if (!string.IsNullOrEmpty(mucDoRuiRo))
                {
                    query = query.Where(k => k.MucDoRuiRo == mucDoRuiRo);
                }

                var khoanVays = await query
                    .OrderByDescending(k => k.NgayNopHoSo)
                    .ToListAsync();

                _logger.LogInformation("Loans after filtering: {Count}", khoanVays.Count);

                // Load thông tin đánh giá rủi ro
                var maKhoanVayIds = khoanVays.Select(k => k.MaKhoanVay).ToList();
                var danhGias = await _context.DanhGiaRuiRos
                    .Where(d => maKhoanVayIds.Contains(d.MaKhoanVay))
                    .ToListAsync();

                _logger.LogInformation("Risk assessments found: {Count}", danhGias.Count);

                var result = new List<KhoanVayThemDinhViewModel>();

                foreach (var khoanVay in khoanVays)
                {
                    string tenKhachHang = "";

                    // Lấy tên khách hàng dựa trên loại khách hàng
                    if (khoanVay.LoaiKhachHang == "CaNhan")
                    {
                        var khachHang = await _context.KhachHangCaNhans
                            .FirstOrDefaultAsync(k => k.MaKhachHang == khoanVay.MaKhachHang);
                        tenKhachHang = khachHang?.HoTen ?? "Không xác định";
                    }
                    else if (khoanVay.LoaiKhachHang == "DoanhNghiep")
                    {
                        var khachHang = await _context.KhachHangDoanhNghieps
                            .FirstOrDefaultAsync(k => k.MaKhachHang == khoanVay.MaKhachHang);
                        tenKhachHang = khachHang?.TenCongTy ?? "Không xác định";
                    }

                    // Lấy thông tin đánh giá
                    var danhGia = danhGias.FirstOrDefault(d => d.MaKhoanVay == khoanVay.MaKhoanVay);
                    string? trangThaiDanhGia = danhGia?.TrangThai;

                    // Nếu chưa có đánh giá, set trạng thái là "Chưa đánh giá"
                    if (danhGia == null)
                    {
                        trangThaiDanhGia = "Chưa đánh giá";
                    }

                    // Lọc theo trạng thái đánh giá
                    if (!string.IsNullOrEmpty(trangThai))
                    {
                        if (trangThai != trangThaiDanhGia)
                        {
                            continue;
                        }
                    }

                    // Logic phân quyền hiển thị:
                    // - Khoản vay chưa đánh giá: hiển thị cho tất cả nhân viên QLRR
                    // - Khoản vay đã đánh giá: chỉ hiển thị cho người đã đánh giá
                    if (maNguoiDung.HasValue && danhGia != null)
                    {
                        // Khoản vay đã có đánh giá, chỉ hiển thị cho người đánh giá
                        if (danhGia.NguoiDanhGia != maNguoiDung.Value)
                        {
                            continue;
                        }
                    }

                    result.Add(new KhoanVayThemDinhViewModel
                    {
                        MaKhoanVay = khoanVay.MaKhoanVay,
                        MaKhoanVayCode = khoanVay.MaKhoanVayCode,
                        TenKhachHang = tenKhachHang,
                        LoaiKhachHang = khoanVay.LoaiKhachHang,
                        SoTienVay = khoanVay.SoTienVay,
                        NgayNopHoSo = khoanVay.NgayNopHoSo,
                        DiemRuiRo = khoanVay.DiemRuiRo ?? danhGia?.TongDiem,
                        MucDoRuiRo = khoanVay.MucDoRuiRo ?? danhGia?.MucDoRuiRo,
                        XepHangRuiRo = khoanVay.XepHangRuiRo ?? danhGia?.XepHangRuiRo,
                        TrangThaiKhoanVay = khoanVay.TrangThaiKhoanVay,
                        TrangThaiDanhGia = trangThaiDanhGia,
                        NgayDanhGia = danhGia?.NgayDanhGia,
                        NguoiDanhGia = danhGia?.NguoiDanhGia
                    });
                }

                _logger.LogInformation("Final result count: {Count}", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách khoản vay cần thẩm định");
                throw;
            }
        }

        public async Task<KhoanVay?> GetKhoanVayDetailAsync(int maKhoanVay)
        {
            try
            {
                var khoanVay = await _context.KhoanVays
                    .Include(k => k.MaLoaiVayNavigation)
                    .Include(k => k.DanhGiaRuiRos)
                    .Include(k => k.KhoanVayTaiSans)
                        .ThenInclude(kvts => kvts.MaTaiSanNavigation)
                    .FirstOrDefaultAsync(k => k.MaKhoanVay == maKhoanVay);

                return khoanVay;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy chi tiết khoản vay {MaKhoanVay}", maKhoanVay);
                throw;
            }
        }

        public async Task<KhoanVayDetailViewModel?> GetKhoanVayFullDetailAsync(int maKhoanVay)
        {
            try
            {
                var khoanVay = await _context.KhoanVays
                    .Include(k => k.MaLoaiVayNavigation)
                    .Include(k => k.KhoanVayTaiSans)
                        .ThenInclude(kvts => kvts.MaTaiSanNavigation)
                            .ThenInclude(ts => ts.MaLoaiTaiSanNavigation)
                    .FirstOrDefaultAsync(k => k.MaKhoanVay == maKhoanVay);

                if (khoanVay == null)
                    return null;

                var result = new KhoanVayDetailViewModel
                {
                    KhoanVay = new KhoanVayViewModel
                    {
                        MaKhoanVay = khoanVay.MaKhoanVay,
                        MaKhoanVayCode = khoanVay.MaKhoanVayCode,
                        LoaiKhachHang = khoanVay.LoaiKhachHang,
                        MaKhachHang = khoanVay.MaKhachHang,
                        SoTienVay = khoanVay.SoTienVay,
                        LaiSuat = khoanVay.LaiSuat,
                        KyHanVay = khoanVay.KyHanVay,
                        HinhThucTraNo = khoanVay.HinhThucTraNo,
                        MucDichVay = khoanVay.MucDichVay,
                        NgayNopHoSo = khoanVay.NgayNopHoSo,
                        NgayGiaiNgan = khoanVay.NgayGiaiNgan,
                        TrangThaiKhoanVay = khoanVay.TrangThaiKhoanVay,
                        MucDoRuiRo = khoanVay.MucDoRuiRo,
                        DiemRuiRo = khoanVay.DiemRuiRo,
                        XepHangRuiRo = khoanVay.XepHangRuiRo
                    },
                    LoaiVay = khoanVay.MaLoaiVayNavigation != null ? new LoaiVayViewModel
                    {
                        MaLoaiVay = khoanVay.MaLoaiVayNavigation.MaLoaiVay,
                        TenLoaiVay = khoanVay.MaLoaiVayNavigation.TenLoaiVay,
                        MoTa = khoanVay.MaLoaiVayNavigation.MoTa
                    } : null,
                    TaiSanDamBaos = khoanVay.KhoanVayTaiSans
                        .Where(kvts => kvts.MaTaiSanNavigation != null)
                        .Select(kvts => new TaiSanDamBaoViewModel
                        {
                            MaTaiSan = kvts.MaTaiSanNavigation!.MaTaiSan,
                            MaLienKet = kvts.MaLienKet,
                            MaTaiSanCode = kvts.MaTaiSanNavigation.MaTaiSanCode,
                            TenGoi = kvts.MaTaiSanNavigation.TenGoi,
                            MoTaChiTiet = kvts.MaTaiSanNavigation.MoTaChiTiet,
                            
                            // Thông tin loại tài sản
                            MaLoaiTaiSan = kvts.MaTaiSanNavigation.MaLoaiTaiSan,
                            TenLoaiTaiSan = kvts.MaTaiSanNavigation.MaLoaiTaiSanNavigation?.TenLoaiTaiSan,
                            
                            // Giá trị từ TaiSanDamBao
                            GiaTriDinhGia = kvts.MaTaiSanNavigation.GiaTriDinhGia,
                            TinhTrang = kvts.MaTaiSanNavigation.TinhTrang,
                            
                            // Thông tin từ KhoanVay_TaiSan
                            GiaTriDinhGiaTaiThoiDiemVay = kvts.GiaTriDinhGiaTaiThoiDiemVay,
                            TyLeTheChap = kvts.TyLeTheChap,
                            NgayTheChap = kvts.NgayTheChap,
                            NgayGiaiChap = kvts.NgayGiaiChap,
                            TrangThaiTheChap = kvts.TrangThai,
                            GhiChu = kvts.GhiChu,
                            
                            // Thông tin tài sản loại "Khác"
                            TenTaiSanKhac = kvts.TenTaiSanKhac,
                            DonVi = kvts.DonVi,
                            SoLuong = kvts.SoLuong,
                            
                            // Thông tin chi tiết từ TaiSanDamBao
                            SoGiayTo = kvts.MaTaiSanNavigation.SoGiayTo,
                            DiaChi = kvts.MaTaiSanNavigation.DiaChi,
                            Quan = kvts.MaTaiSanNavigation.Quan,
                            DienTich = kvts.MaTaiSanNavigation.DienTich,
                            NamSanXuat = kvts.MaTaiSanNavigation.NamSanXuat
                            // Lưu ý: HangXe, DongXe, TrongLuong không có trong TaiSanDamBao
                            // Các thông tin này sẽ cần lấy từ bảng khác nếu cần
                        })
                        .ToList()
                };

                // Lấy thông tin khách hàng
                if (khoanVay.LoaiKhachHang == "CaNhan")
                {
                    var khachHang = await _context.KhachHangCaNhans
                        .FirstOrDefaultAsync(k => k.MaKhachHang == khoanVay.MaKhachHang);
                    
                    if (khachHang != null)
                    {
                        result.KhachHangCaNhan = new KhachHangCaNhanViewModel
                        {
                            HoTen = khachHang.HoTen,
                            NgaySinh = khachHang.NgaySinh,
                            GioiTinh = khachHang.GioiTinh,
                            SoCmnd = khachHang.SoCmnd,
                            SoDienThoai = khachHang.SoDienThoai,
                            Email = khachHang.Email,
                            DiaChi = khachHang.DiaChi,
                            NgheNghiep = khachHang.NgheNghiep,
                            ThuNhapHangThang = khachHang.ThuNhapHangThang
                        };
                    }
                }
                else if (khoanVay.LoaiKhachHang == "DoanhNghiep")
                {
                    var khachHang = await _context.KhachHangDoanhNghieps
                        .FirstOrDefaultAsync(k => k.MaKhachHang == khoanVay.MaKhachHang);
                    
                    if (khachHang != null)
                    {
                        result.KhachHangDoanhNghiep = new KhachHangDoanhNghiepViewModel
                        {
                            TenCongTy = khachHang.TenCongTy,
                            MaSoThue = khachHang.MaSoThue,
                            NguoiDaiDienPhapLuat = khachHang.NguoiDaiDienPhapLuat,
                            SoDienThoai = khachHang.SoDienThoai,
                            Email = khachHang.Email,
                            DiaChi = khachHang.DiaChi,
                            LinhVucKinhDoanh = khachHang.LinhVucKinhDoanh,
                            VonDieuLe = khachHang.VonDieuLe,
                            DoanhThuHangNam = khachHang.DoanhThuHangNam
                        };
                    }
                }

                // Lấy đánh giá rủi ro hiện tại (nếu có)
                var danhGia = await _context.DanhGiaRuiRos
                    .Include(d => d.ChiTietDanhGiaRuiRos)
                        .ThenInclude(ct => ct.MaTieuChiNavigation)
                    .FirstOrDefaultAsync(d => d.MaKhoanVay == maKhoanVay);

                if (danhGia != null)
                {
                    result.DanhGiaRuiRo = new DanhGiaRuiRoViewModel
                    {
                        MaDanhGia = danhGia.MaDanhGia,
                        TongDiem = danhGia.TongDiem,
                        MucDoRuiRo = danhGia.MucDoRuiRo,
                        XepHangRuiRo = danhGia.XepHangRuiRo,
                        KienNghi = danhGia.KienNghi,
                        NhanXet = danhGia.NhanXet,
                        TrangThai = danhGia.TrangThai,
                        NgayDanhGia = danhGia.NgayDanhGia
                    };
                    result.ChiTietDanhGias = danhGia.ChiTietDanhGiaRuiRos.ToList();
                }

                // Lấy danh sách tiêu chí đánh giá
                result.TieuChis = await _context.TieuChiDanhGiaRuiRos
                    .Where(tc => tc.TrangThaiHoatDong == true)
                    .Where(tc => tc.LoaiKhachHang == null || tc.LoaiKhachHang == khoanVay.LoaiKhachHang)
                    .OrderBy(tc => tc.MaTieuChi)
                    .ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy chi tiết đầy đủ khoản vay {MaKhoanVay}", maKhoanVay);
                throw;
            }
        }

        public async Task<DanhGiaRuiRo?> GetDanhGiaRuiRoByKhoanVayAsync(int maKhoanVay)
        {
            try
            {
                var danhGia = await _context.DanhGiaRuiRos
                    .Include(d => d.ChiTietDanhGiaRuiRos)
                        .ThenInclude(ct => ct.MaTieuChiNavigation)
                    .FirstOrDefaultAsync(d => d.MaKhoanVay == maKhoanVay);

                return danhGia;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy đánh giá rủi ro cho khoản vay {MaKhoanVay}", maKhoanVay);
                throw;
            }
        }

        public async Task<DanhGiaRuiRo> CreateOrUpdateDanhGiaRuiRoAsync(DanhGiaRuiRo danhGia, int nguoiDanhGia)
        {
            try
            {
                var existingDanhGia = await _context.DanhGiaRuiRos
                    .FirstOrDefaultAsync(d => d.MaKhoanVay == danhGia.MaKhoanVay);

                if (existingDanhGia != null)
                {
                    // Cập nhật đánh giá hiện có
                    existingDanhGia.NgayDanhGia = DateTime.Now;
                    existingDanhGia.NguoiDanhGia = nguoiDanhGia;
                    existingDanhGia.TongDiem = danhGia.TongDiem;
                    existingDanhGia.MucDoRuiRo = danhGia.MucDoRuiRo;
                    existingDanhGia.XepHangRuiRo = danhGia.XepHangRuiRo;
                    existingDanhGia.KienNghi = danhGia.KienNghi;
                    existingDanhGia.NhanXet = danhGia.NhanXet;
                    existingDanhGia.TrangThai = danhGia.TrangThai;

                    _context.DanhGiaRuiRos.Update(existingDanhGia);
                }
                else
                {
                    // Tạo mới đánh giá
                    danhGia.NgayDanhGia = DateTime.Now;
                    danhGia.NguoiDanhGia = nguoiDanhGia;
                    danhGia.NgayTao = DateTime.Now;
                    if (string.IsNullOrEmpty(danhGia.TrangThai))
                    {
                        danhGia.TrangThai = "Đang đánh giá";
                    }

                    _context.DanhGiaRuiRos.Add(danhGia);
                }

                await _context.SaveChangesAsync();

                // Cập nhật thông tin rủi ro vào bảng KhoanVay
                var khoanVay = await _context.KhoanVays.FindAsync(danhGia.MaKhoanVay);
                if (khoanVay != null)
                {
                    khoanVay.DiemRuiRo = danhGia.TongDiem;
                    khoanVay.MucDoRuiRo = danhGia.MucDoRuiRo;
                    khoanVay.XepHangRuiRo = danhGia.XepHangRuiRo;
                    _context.KhoanVays.Update(khoanVay);
                    await _context.SaveChangesAsync();
                }

                return existingDanhGia ?? danhGia;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo hoặc cập nhật đánh giá rủi ro");
                throw;
            }
        }

        public async Task<List<TieuChiDanhGiaRuiRo>> GetTieuChiDanhGiaAsync()
        {
            try
            {
                var tieuChis = await _context.TieuChiDanhGiaRuiRos
                    .Where(tc => tc.TrangThaiHoatDong == true)
                    .OrderBy(tc => tc.MaTieuChi)
                    .ToListAsync();

                return tieuChis;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách tiêu chí đánh giá");
                throw;
            }
        }

        public async Task<List<ChiTietDanhGiaRuiRo>> GetChiTietDanhGiaAsync(int maDanhGia)
        {
            try
            {
                var chiTiets = await _context.ChiTietDanhGiaRuiRos
                    .Include(ct => ct.MaTieuChiNavigation)
                    .Where(ct => ct.MaDanhGia == maDanhGia)
                    .ToListAsync();

                return chiTiets;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy chi tiết đánh giá rủi ro");
                throw;
            }
        }

        public async Task<List<GiaTriTaiSanThamChieu>> GetGiaTriThamChieuAsync(string loaiTaiSan, string? keyword = null)
        {
            try
            {
                var query = _context.GiaTriTaiSanThamChieus
                    .Where(g => g.LoaiTaiSan == loaiTaiSan && g.TrangThaiHoatDong == true);

                if (!string.IsNullOrEmpty(keyword))
                {
                    keyword = keyword.ToLower();
                    if (loaiTaiSan == "Đất")
                    {
                        query = query.Where(g => g.Quan != null && g.Quan.ToLower().Contains(keyword));
                    }
                    else if (loaiTaiSan == "Xe")
                    {
                        query = query.Where(g => 
                            (g.HangXe != null && g.HangXe.ToLower().Contains(keyword)) ||
                            (g.DongXe != null && g.DongXe.ToLower().Contains(keyword)));
                    }
                }

                return await query.OrderBy(g => g.LoaiTaiSan).ThenBy(g => g.Quan).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy giá trị tham chiếu tài sản");
                throw;
            }
        }

        public async Task<GiaTriTaiSanThamChieu?> TimGiaTriThamChieuAsync(string loaiTaiSan, string? quan = null, string? hangXe = null, string? dongXe = null, int? namSanXuat = null)
        {
            try
            {
                var query = _context.GiaTriTaiSanThamChieus
                    .Where(g => g.LoaiTaiSan == loaiTaiSan && g.TrangThaiHoatDong == true);

                if (loaiTaiSan == "Đất" && !string.IsNullOrEmpty(quan))
                {
                    query = query.Where(g => g.Quan == quan);
                }
                else if (loaiTaiSan == "Xe")
                {
                    if (!string.IsNullOrEmpty(hangXe))
                        query = query.Where(g => g.HangXe == hangXe);
                    if (!string.IsNullOrEmpty(dongXe))
                        query = query.Where(g => g.DongXe == dongXe);
                    if (namSanXuat.HasValue)
                    {
                        // Tìm xe có năm sản xuất gần nhất với năm đang tìm
                        var exactMatch = await query.FirstOrDefaultAsync(g => g.NamSanXuat == namSanXuat);
                        if (exactMatch != null)
                            return exactMatch;

                        // Nếu không có năm chính xác, lấy năm gần nhất
                        var allMatches = await query.ToListAsync();
                        return allMatches
                            .OrderBy(g => Math.Abs((g.NamSanXuat ?? 0) - namSanXuat.Value))
                            .FirstOrDefault();
                    }
                }

                return await query.FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tìm giá trị tham chiếu tài sản");
                throw;
            }
        }

        public async Task<List<HoSoVayFileDinhKem>> GetFileDinhKemByKhoanVayAsync(int maKhoanVay)
        {
            try
            {
                return await _context.HoSoVayFileDinhKems
                    .Where(f => f.MaKhoanVay == maKhoanVay)
                    .OrderBy(f => f.NgayTao)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy file đính kèm của khoản vay {MaKhoanVay}", maKhoanVay);
                throw;
            }
        }

        public async Task<bool> LuuKetQuaThamDinhAsync(int maTaiSan, decimal giaTriThamChieu, decimal giaTriThamDinh, decimal tyLeThamDinh, string? ghiChu, int nguoiThamDinh,
            string? soGiayTo, DateOnly? ngayCap, string? noiCap, string? chuSoHuu, string? diaChi, string? thanhPho, string? quan, string? tinhTrang, decimal? dienTich)
        {
            try
            {
                var taiSan = await _context.TaiSanDamBaos.FindAsync(maTaiSan);
                if (taiSan == null)
                {
                    _logger.LogWarning("Không tìm thấy tài sản {MaTaiSan}", maTaiSan);
                    return false;
                }

                // Cập nhật thông tin thẩm định
                taiSan.GiaTriThiTruong = giaTriThamChieu;
                taiSan.GiaTriDinhGia = giaTriThamDinh;
                taiSan.NgayDinhGia = DateOnly.FromDateTime(DateTime.Now);
                taiSan.DonViDinhGia = "Phòng Quản lý Rủi ro";
                taiSan.NgayCapNhat = DateTime.Now;
                taiSan.NguoiCapNhat = nguoiThamDinh;
                
                // Cập nhật thông tin chi tiết tài sản
                if (!string.IsNullOrEmpty(soGiayTo)) taiSan.SoGiayTo = soGiayTo;
                if (ngayCap.HasValue) taiSan.NgayCap = ngayCap;
                if (!string.IsNullOrEmpty(noiCap)) taiSan.NoiCap = noiCap;
                if (!string.IsNullOrEmpty(chuSoHuu)) taiSan.ChuSoHuu = chuSoHuu;
                if (!string.IsNullOrEmpty(diaChi)) taiSan.DiaChi = diaChi;
                if (!string.IsNullOrEmpty(thanhPho)) taiSan.ThanhPho = thanhPho;
                if (!string.IsNullOrEmpty(quan)) taiSan.Quan = quan;
                if (!string.IsNullOrEmpty(tinhTrang)) taiSan.TinhTrang = tinhTrang;
                if (dienTich.HasValue && dienTich.Value > 0) taiSan.DienTich = dienTich;

                // Lưu lịch sử định giá
                var lichSu = new LichSuDinhGiaTaiSan
                {
                    MaTaiSan = maTaiSan,
                    GiaTriCu = taiSan.GiaTriDinhGia,
                    GiaTriMoi = giaTriThamDinh,
                    ChenhLech = giaTriThamDinh - (taiSan.GiaTriDinhGia ?? 0),
                    TyLeThayDoi = taiSan.GiaTriDinhGia.HasValue && taiSan.GiaTriDinhGia > 0 
                        ? (giaTriThamDinh - taiSan.GiaTriDinhGia.Value) / taiSan.GiaTriDinhGia.Value * 100 
                        : null,
                    LyDoDinhGia = $"Thẩm định theo giá tham chiếu. Tỷ lệ: {tyLeThamDinh:F2}%",
                    NgayDinhGia = DateOnly.FromDateTime(DateTime.Now),
                    NguoiDinhGia = nguoiThamDinh.ToString(),
                    DonViDinhGia = "Phòng Quản lý Rủi ro",
                    PhuongPhapDinhGia = "Phương pháp so sánh giá tham chiếu",
                    FileDinhGia = ghiChu,
                    NgayTao = DateTime.Now,
                    NguoiTao = nguoiThamDinh
                };

                _context.LichSuDinhGiaTaiSans.Add(lichSu);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Đã lưu kết quả thẩm định tài sản {MaTaiSan}", maTaiSan);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lưu kết quả thẩm định tài sản {MaTaiSan}", maTaiSan);
                return false;
            }
        }

        public async Task<bool> UpdateTaiSanThamDinhAsync(int maLienKet, decimal? giaTriDinhGia, decimal? tyLeTheChap, DateOnly? ngayTheChap, string? ghiChu)
        {
            try
            {
                var khoanVayTaiSan = await _context.KhoanVayTaiSans.FindAsync(maLienKet);
                if (khoanVayTaiSan == null)
                {
                    _logger.LogWarning("Không tìm thấy liên kết khoản vay - tài sản {MaLienKet}", maLienKet);
                    return false;
                }

                // Cập nhật thông tin thẩm định trong bảng KhoanVay_TaiSan
                khoanVayTaiSan.GiaTriDinhGiaTaiThoiDiemVay = giaTriDinhGia;
                khoanVayTaiSan.TyLeTheChap = tyLeTheChap;
                khoanVayTaiSan.NgayTheChap = ngayTheChap;
                khoanVayTaiSan.GhiChu = ghiChu;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Đã cập nhật thông tin thẩm định cho liên kết {MaLienKet}", maLienKet);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật thông tin thẩm định liên kết {MaLienKet}", maLienKet);
                return false;
            }
        }

        public async Task<object?> GetTaiSanDetailAsync(int maTaiSan)
        {
            try
            {
                var taiSan = await _context.TaiSanDamBaos
                    .Include(t => t.MaLoaiTaiSanNavigation)
                    .FirstOrDefaultAsync(t => t.MaTaiSan == maTaiSan);

                if (taiSan == null)
                {
                    _logger.LogWarning("Không tìm thấy tài sản {MaTaiSan}", maTaiSan);
                    return null;
                }

                return new
                {
                    maTaiSan = taiSan.MaTaiSan,
                    tenGoi = taiSan.TenGoi,
                    maLoaiTaiSan = taiSan.MaLoaiTaiSan,
                    tenLoaiTaiSan = taiSan.MaLoaiTaiSanNavigation?.TenLoaiTaiSan,
                    soGiayTo = taiSan.SoGiayTo,
                    chuSoHuu = taiSan.ChuSoHuu,
                    diaChi = taiSan.DiaChi,
                    thanhPho = taiSan.ThanhPho,
                    quan = taiSan.Quan,
                    dienTich = taiSan.DienTich,
                    tinhTrang = taiSan.TinhTrang,
                    giaTriDinhGia = taiSan.GiaTriDinhGia
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin tài sản {MaTaiSan}", maTaiSan);
                return null;
            }
        }

        public async Task<ThongTinCicViewModel?> GetThongTinCicByKhoanVayAsync(int maKhoanVay)
        {
            try
            {
                // Lấy thông tin khoản vay để biết khách hàng
                var khoanVay = await _context.KhoanVays.FindAsync(maKhoanVay);
                if (khoanVay == null)
                {
                    _logger.LogWarning("Không tìm thấy khoản vay {MaKhoanVay}", maKhoanVay);
                    return null;
                }

                ThongTinCic? cicInfo = null;

                // Tìm thông tin CIC dựa trên loại khách hàng
                if (khoanVay.LoaiKhachHang == "CaNhan")
                {
                    var khachHang = await _context.KhachHangCaNhans
                        .FirstOrDefaultAsync(k => k.MaKhachHang == khoanVay.MaKhachHang);
                    
                    if (khachHang != null && !string.IsNullOrEmpty(khachHang.SoCmnd))
                    {
                        cicInfo = await _context.ThongTinCics
                            .FirstOrDefaultAsync(c => c.SoCmndCccd == khachHang.SoCmnd && c.LoaiKhachHang == "CaNhan");
                    }
                }
                else if (khoanVay.LoaiKhachHang == "DoanhNghiep")
                {
                    var khachHang = await _context.KhachHangDoanhNghieps
                        .FirstOrDefaultAsync(k => k.MaKhachHang == khoanVay.MaKhachHang);
                    
                    if (khachHang != null && !string.IsNullOrEmpty(khachHang.MaSoThue))
                    {
                        cicInfo = await _context.ThongTinCics
                            .FirstOrDefaultAsync(c => c.MaSoThue == khachHang.MaSoThue && c.LoaiKhachHang == "DoanhNghiep");
                    }
                }

                if (cicInfo == null)
                {
                    _logger.LogInformation("Không tìm thấy thông tin CIC cho khoản vay {MaKhoanVay}", maKhoanVay);
                    return null;
                }

                return new ThongTinCicViewModel
                {
                    MaCic = cicInfo.MaCic,
                    MaCicCode = cicInfo.MaCicCode,
                    LoaiKhachHang = cicInfo.LoaiKhachHang,
                    HoTen = cicInfo.HoTen,
                    SoCmndCccd = cicInfo.SoCmndCccd,
                    MaSoThue = cicInfo.MaSoThue,
                    TongSoKhoanVayCic = cicInfo.TongSoKhoanVayCic,
                    SoKhoanVayDangVayCic = cicInfo.SoKhoanVayDangVayCic,
                    SoKhoanVayDaTraXongCic = cicInfo.SoKhoanVayDaTraXongCic,
                    SoKhoanVayQuaHanCic = cicInfo.SoKhoanVayQuaHanCic,
                    SoKhoanVayNoXauCic = cicInfo.SoKhoanVayNoXauCic,
                    TongDuNoCic = cicInfo.TongDuNoCic,
                    DuNoQuaHanCic = cicInfo.DuNoQuaHanCic,
                    DuNoNoXauCic = cicInfo.DuNoNoXauCic,
                    TongGiaTriVayCic = cicInfo.TongGiaTriVayCic,
                    DiemTinDungCic = cicInfo.DiemTinDungCic,
                    XepHangTinDungCic = cicInfo.XepHangTinDungCic,
                    MucDoRuiRo = cicInfo.MucDoRuiRo,
                    KhaNangTraNo = cicInfo.KhaNangTraNo,
                    SoLanQuaHanCic = cicInfo.SoLanQuaHanCic,
                    SoLanNoXauCic = cicInfo.SoLanNoXauCic,
                    SoNgayQuaHanToiDaCic = cicInfo.SoNgayQuaHanToiDaCic,
                    TyLeTraNoDungHanCic = cicInfo.TyLeTraNoDungHanCic,
                    DanhGiaTongQuat = cicInfo.DanhGiaTongQuat,
                    KhuyenNghiChoVay = cicInfo.KhuyenNghiChoVay,
                    LyDoKhuyenNghi = cicInfo.LyDoKhuyenNghi,
                    NgayTraCuuCuoi = cicInfo.NgayTraCuuCuoi
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin CIC cho khoản vay {MaKhoanVay}", maKhoanVay);
                return null;
            }
        }

        public async Task<List<PhanLoaiRuiRoViewModel>> GetPhanLoaiMucDoRuiRoAsync(string? maKhoanVay = null, string? tenKhachHang = null, 
            decimal? diemTu = null, decimal? diemDen = null, string? mucDoRuiRo = null)
        {
            try
            {
                // Query các khoản vay đã có đánh giá rủi ro
                var query = _context.KhoanVays
                    .Where(k => k.TrangThaiKhoanVay != "Nháp" && k.NgayNopHoSo != null);

                // Lọc theo mã khoản vay
                if (!string.IsNullOrEmpty(maKhoanVay))
                {
                    query = query.Where(k => k.MaKhoanVayCode.Contains(maKhoanVay));
                }

                // Lọc theo mức độ rủi ro
                if (!string.IsNullOrEmpty(mucDoRuiRo) && mucDoRuiRo != "all")
                {
                    query = query.Where(k => k.MucDoRuiRo == mucDoRuiRo);
                }

                // Lọc theo điểm số
                if (diemTu.HasValue)
                {
                    query = query.Where(k => k.DiemRuiRo >= diemTu.Value);
                }
                if (diemDen.HasValue)
                {
                    query = query.Where(k => k.DiemRuiRo <= diemDen.Value);
                }

                var khoanVays = await query
                    .OrderByDescending(k => k.DiemRuiRo)
                    .ThenByDescending(k => k.NgayNopHoSo)
                    .ToListAsync();

                var result = new List<PhanLoaiRuiRoViewModel>();

                foreach (var khoanVay in khoanVays)
                {
                    string tenKH = "";

                    // Lấy tên khách hàng dựa trên loại khách hàng
                    if (khoanVay.LoaiKhachHang == "CaNhan")
                    {
                        var khachHang = await _context.KhachHangCaNhans
                            .FirstOrDefaultAsync(k => k.MaKhachHang == khoanVay.MaKhachHang);
                        tenKH = khachHang?.HoTen ?? "Không xác định";
                    }
                    else if (khoanVay.LoaiKhachHang == "DoanhNghiep")
                    {
                        var khachHang = await _context.KhachHangDoanhNghieps
                            .FirstOrDefaultAsync(k => k.MaKhachHang == khoanVay.MaKhachHang);
                        tenKH = khachHang?.TenCongTy ?? "Không xác định";
                    }

                    // Lọc theo tên khách hàng nếu có
                    if (!string.IsNullOrEmpty(tenKhachHang))
                    {
                        if (!tenKH.ToLower().Contains(tenKhachHang.ToLower()))
                        {
                            continue;
                        }
                    }

                    result.Add(new PhanLoaiRuiRoViewModel
                    {
                        MaKhoanVay = khoanVay.MaKhoanVay,
                        MaKhoanVayCode = khoanVay.MaKhoanVayCode,
                        TenKhachHang = tenKH,
                        LoaiKhachHang = khoanVay.LoaiKhachHang,
                        SoTienVay = khoanVay.SoTienVay,
                        DiemRuiRo = khoanVay.DiemRuiRo,
                        MucDoRuiRo = khoanVay.MucDoRuiRo ?? "Chưa đánh giá",
                        XepHangRuiRo = khoanVay.XepHangRuiRo,
                        TrangThaiKhoanVay = khoanVay.TrangThaiKhoanVay
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách phân loại mức độ rủi ro");
                throw;
            }
        }

        public async Task<ThongKePhanLoaiRuiRoViewModel> GetThongKePhanLoaiRuiRoAsync()
        {
            try
            {
                var khoanVays = await _context.KhoanVays
                    .Where(k => k.TrangThaiKhoanVay != "Nháp" && k.NgayNopHoSo != null && k.MucDoRuiRo != null)
                    .ToListAsync();

                var tongSo = khoanVays.Count;
                var soRuiRoThap = khoanVays.Count(k => k.MucDoRuiRo == "Thấp");
                var soRuiRoTrungBinh = khoanVays.Count(k => k.MucDoRuiRo == "Trung bình");
                var soRuiRoCao = khoanVays.Count(k => k.MucDoRuiRo == "Cao");

                return new ThongKePhanLoaiRuiRoViewModel
                {
                    TongSoKhoanVay = tongSo,
                    SoRuiRoThap = soRuiRoThap,
                    SoRuiRoTrungBinh = soRuiRoTrungBinh,
                    SoRuiRoCao = soRuiRoCao,
                    TyLeRuiRoThap = tongSo > 0 ? Math.Round((decimal)soRuiRoThap / tongSo * 100, 1) : 0,
                    TyLeRuiRoTrungBinh = tongSo > 0 ? Math.Round((decimal)soRuiRoTrungBinh / tongSo * 100, 1) : 0,
                    TyLeRuiRoCao = tongSo > 0 ? Math.Round((decimal)soRuiRoCao / tongSo * 100, 1) : 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thống kê phân loại rủi ro");
                throw;
            }
        }

        // Lấy tổng quan danh mục tín dụng (tính toán realtime từ bảng KhoanVay)
        public async Task<DanhMucTinDungTongQuatViewModel> GetDanhMucTinDungTongQuatAsync()
        {
            try
            {
                var now = DateTime.Now;
                var dauThangNay = new DateTime(now.Year, now.Month, 1);
                var dauThangTruoc = dauThangNay.AddMonths(-1);

                // Lấy tất cả khoản vay đang hoạt động (không phải nháp)
                var khoanVays = await _context.KhoanVays
                    .Where(k => k.TrangThaiKhoanVay != "Nháp" && k.NgayNopHoSo != null)
                    .ToListAsync();

                var tongSoKhoanVay = khoanVays.Count;
                var tongDuNo = khoanVays.Sum(k => k.TongDuNo ?? 0);
                var tongSoTienVay = khoanVays.Sum(k => k.SoTienVay);

                // Tính tỷ lệ nợ xấu (khoản vay có trạng thái "Quá hạn" hoặc phân loại nợ >= 3)
                var khoanVayNoXau = khoanVays.Count(k => 
                    k.TrangThaiKhoanVay == "Quá hạn" || 
                    k.MaPhanLoaiNo >= 3);
                var tyLeNoXau = tongSoKhoanVay > 0 ? Math.Round((decimal)khoanVayNoXau / tongSoKhoanVay * 100, 2) : 0;

                // Tính điểm rủi ro trung bình
                var khoanVayCoDiem = khoanVays.Where(k => k.DiemRuiRo.HasValue).ToList();
                var diemRuiRoTrungBinh = khoanVayCoDiem.Any() 
                    ? Math.Round(khoanVayCoDiem.Average(k => k.DiemRuiRo!.Value), 1) 
                    : 0;

                // Phân loại theo mức độ rủi ro
                var soRuiRoThap = khoanVays.Count(k => k.MucDoRuiRo == "Thấp");
                var soRuiRoTrungBinh = khoanVays.Count(k => k.MucDoRuiRo == "Trung bình");
                var soRuiRoCao = khoanVays.Count(k => k.MucDoRuiRo == "Cao");

                // Tính số khoản vay mới trong tháng
                var soKhoanVayMoiThang = khoanVays.Count(k => 
                    k.NgayNopHoSo.HasValue && k.NgayNopHoSo.Value >= dauThangNay);

                // Lấy dữ liệu tháng trước để so sánh
                var danhMucThangTruoc = await _context.DanhMucTinDungs
                    .Where(d => d.NgayDanhMuc >= DateOnly.FromDateTime(dauThangTruoc) && 
                               d.NgayDanhMuc < DateOnly.FromDateTime(dauThangNay))
                    .OrderByDescending(d => d.NgayDanhMuc)
                    .FirstOrDefaultAsync();

                decimal tyLeTangDuNo = 0;
                decimal thayDoiTyLeNoXau = 0;
                decimal thayDoiDiemRuiRo = 0;

                if (danhMucThangTruoc != null)
                {
                    if (danhMucThangTruoc.TongDuNo.HasValue && danhMucThangTruoc.TongDuNo > 0)
                    {
                        tyLeTangDuNo = Math.Round(((tongDuNo - danhMucThangTruoc.TongDuNo.Value) / danhMucThangTruoc.TongDuNo.Value) * 100, 1);
                    }
                    if (danhMucThangTruoc.TyLeNoXau.HasValue)
                    {
                        thayDoiTyLeNoXau = Math.Round(tyLeNoXau - danhMucThangTruoc.TyLeNoXau.Value, 1);
                    }
                    if (danhMucThangTruoc.DiemRuiRoTrungBinh.HasValue)
                    {
                        thayDoiDiemRuiRo = Math.Round(diemRuiRoTrungBinh - danhMucThangTruoc.DiemRuiRoTrungBinh.Value, 1);
                    }
                }

                return new DanhMucTinDungTongQuatViewModel
                {
                    TongSoKhoanVay = tongSoKhoanVay,
                    TongDuNo = tongDuNo,
                    TyLeNoXau = tyLeNoXau,
                    DiemRuiRoTrungBinh = diemRuiRoTrungBinh,
                    SoKhoanVayRuiRoThap = soRuiRoThap,
                    SoKhoanVayRuiRoTrungBinh = soRuiRoTrungBinh,
                    SoKhoanVayRuiRoCao = soRuiRoCao,
                    TongSoTienVay = tongSoTienVay,
                    SoKhoanVayMoiThang = soKhoanVayMoiThang,
                    TyLeTangDuNo = tyLeTangDuNo,
                    ThayDoiTyLeNoXau = thayDoiTyLeNoXau,
                    ThayDoiDiemRuiRo = thayDoiDiemRuiRo
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy tổng quan danh mục tín dụng");
                throw;
            }
        }

        // Lấy danh sách lịch sử danh mục tín dụng
        public async Task<List<DanhMucTinDung>> GetDanhMucTinDungListAsync(string? kyThoiGian = null)
        {
            try
            {
                var query = _context.DanhMucTinDungs.AsQueryable();

                // Lọc theo kỳ thời gian
                if (!string.IsNullOrEmpty(kyThoiGian))
                {
                    var now = DateTime.Now;
                    switch (kyThoiGian)
                    {
                        case "thang":
                            var dauThang = new DateTime(now.Year, now.Month, 1);
                            query = query.Where(d => d.NgayDanhMuc >= DateOnly.FromDateTime(dauThang));
                            break;
                        case "quy":
                            var quy = (now.Month - 1) / 3;
                            var dauQuy = new DateTime(now.Year, quy * 3 + 1, 1);
                            query = query.Where(d => d.NgayDanhMuc >= DateOnly.FromDateTime(dauQuy));
                            break;
                        case "nam":
                            var dauNam = new DateTime(now.Year, 1, 1);
                            query = query.Where(d => d.NgayDanhMuc >= DateOnly.FromDateTime(dauNam));
                            break;
                    }
                }

                return await query
                    .OrderByDescending(d => d.NgayDanhMuc)
                    .Take(12) // Lấy tối đa 12 bản ghi
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách danh mục tín dụng");
                throw;
            }
        }

        // Cập nhật/tạo mới bản ghi danh mục tín dụng cho ngày hiện tại
        public async Task<DanhMucTinDung?> CapNhatDanhMucTinDungAsync(int nguoiTao)
        {
            try
            {
                var today = DateOnly.FromDateTime(DateTime.Now);
                
                // Kiểm tra xem đã có bản ghi cho ngày hôm nay chưa
                var existingRecord = await _context.DanhMucTinDungs
                    .FirstOrDefaultAsync(d => d.NgayDanhMuc == today);

                // Lấy dữ liệu tổng quan
                var tongQuat = await GetDanhMucTinDungTongQuatAsync();

                if (existingRecord != null)
                {
                    // Cập nhật bản ghi hiện tại
                    existingRecord.TongSoKhoanVay = tongQuat.TongSoKhoanVay;
                    existingRecord.TongSoTienVay = tongQuat.TongSoTienVay;
                    existingRecord.TongDuNo = tongQuat.TongDuNo;
                    existingRecord.TyLeNoXau = tongQuat.TyLeNoXau;
                    existingRecord.DiemRuiRoTrungBinh = tongQuat.DiemRuiRoTrungBinh;
                    existingRecord.SoKhoanVayRuiRoThap = tongQuat.SoKhoanVayRuiRoThap;
                    existingRecord.SoKhoanVayRuiRoTrungBinh = tongQuat.SoKhoanVayRuiRoTrungBinh;
                    existingRecord.SoKhoanVayRuiRoCao = tongQuat.SoKhoanVayRuiRoCao;
                    
                    await _context.SaveChangesAsync();
                    return existingRecord;
                }
                else
                {
                    // Tạo bản ghi mới
                    var newRecord = new DanhMucTinDung
                    {
                        NgayDanhMuc = today,
                        TongSoKhoanVay = tongQuat.TongSoKhoanVay,
                        TongSoTienVay = tongQuat.TongSoTienVay,
                        TongDuNo = tongQuat.TongDuNo,
                        TyLeNoXau = tongQuat.TyLeNoXau,
                        DiemRuiRoTrungBinh = tongQuat.DiemRuiRoTrungBinh,
                        SoKhoanVayRuiRoThap = tongQuat.SoKhoanVayRuiRoThap,
                        SoKhoanVayRuiRoTrungBinh = tongQuat.SoKhoanVayRuiRoTrungBinh,
                        SoKhoanVayRuiRoCao = tongQuat.SoKhoanVayRuiRoCao,
                        NgayTao = DateTime.Now,
                        NguoiTao = nguoiTao
                    };

                    _context.DanhMucTinDungs.Add(newRecord);
                    await _context.SaveChangesAsync();
                    return newRecord;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật danh mục tín dụng");
                throw;
            }
        }
    }

    public class PhanLoaiRuiRoViewModel
    {
        public int MaKhoanVay { get; set; }
        public string MaKhoanVayCode { get; set; } = null!;
        public string TenKhachHang { get; set; } = null!;
        public string LoaiKhachHang { get; set; } = null!;
        public decimal SoTienVay { get; set; }
        public decimal? DiemRuiRo { get; set; }
        public string MucDoRuiRo { get; set; } = null!;
        public string? XepHangRuiRo { get; set; }
        public string TrangThaiKhoanVay { get; set; } = null!;
    }

    public class ThongKePhanLoaiRuiRoViewModel
    {
        public int TongSoKhoanVay { get; set; }
        public int SoRuiRoThap { get; set; }
        public int SoRuiRoTrungBinh { get; set; }
        public int SoRuiRoCao { get; set; }
        public decimal TyLeRuiRoThap { get; set; }
        public decimal TyLeRuiRoTrungBinh { get; set; }
        public decimal TyLeRuiRoCao { get; set; }
    }
}
