using Microsoft.EntityFrameworkCore;
using QuanLyRuiRoTinDung.Models.EF;
using QuanLyRuiRoTinDung.Models.Entities;

namespace QuanLyRuiRoTinDung.Services
{
    public interface INhatKyHoatDongService
    {
        Task GhiNhatKyAsync(int? maNguoiDung, string hanhDong, string? tenBang = null, 
            int? maBanGhi = null, string? giaTriCu = null, string? giaTriMoi = null, string? diaChiIp = null);
        
        Task GhiNhatKyDangNhapAsync(int maNguoiDung, string tenDangNhap, string? diaChiIp = null);
        Task GhiNhatKyDangXuatAsync(int maNguoiDung, string tenDangNhap, string? diaChiIp = null);
        Task GhiNhatKyUploadFileAsync(int maNguoiDung, string tenFile, int? maHoSo = null, string? diaChiIp = null);
        Task GhiNhatKyXoaFileAsync(int maNguoiDung, string tenFile, int? maFile = null, string? diaChiIp = null);
    }

    public class NhatKyHoatDongService : INhatKyHoatDongService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<NhatKyHoatDongService> _logger;

        public NhatKyHoatDongService(ApplicationDbContext context, ILogger<NhatKyHoatDongService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task GhiNhatKyAsync(int? maNguoiDung, string hanhDong, string? tenBang = null, 
            int? maBanGhi = null, string? giaTriCu = null, string? giaTriMoi = null, string? diaChiIp = null)
        {
            try
            {
                var nhatKy = new NhatKyHoatDong
                {
                    MaNguoiDung = maNguoiDung,
                    HanhDong = hanhDong,
                    TenBang = tenBang,
                    MaBanGhi = maBanGhi,
                    GiaTriCu = giaTriCu,
                    GiaTriMoi = giaTriMoi,
                    DiaChiIp = diaChiIp,
                    ThoiGian = DateTime.Now
                };

                _context.NhatKyHoatDongs.Add(nhatKy);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Đã ghi nhật ký: {HanhDong} - Người dùng: {MaNguoiDung}", hanhDong, maNguoiDung);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi ghi nhật ký hoạt động: {HanhDong}", hanhDong);
            }
        }

        public async Task GhiNhatKyDangNhapAsync(int maNguoiDung, string tenDangNhap, string? diaChiIp = null)
        {
            await GhiNhatKyAsync(
                maNguoiDung,
                "Đăng nhập hệ thống",
                "NguoiDung",
                maNguoiDung,
                null,
                $"Tài khoản: {tenDangNhap} đăng nhập thành công",
                diaChiIp
            );
        }

        public async Task GhiNhatKyDangXuatAsync(int maNguoiDung, string tenDangNhap, string? diaChiIp = null)
        {
            await GhiNhatKyAsync(
                maNguoiDung,
                "Đăng xuất hệ thống",
                "NguoiDung",
                maNguoiDung,
                null,
                $"Tài khoản: {tenDangNhap} đã đăng xuất",
                diaChiIp
            );
        }

        public async Task GhiNhatKyUploadFileAsync(int maNguoiDung, string tenFile, int? maHoSo = null, string? diaChiIp = null)
        {
            await GhiNhatKyAsync(
                maNguoiDung,
                "Upload file đính kèm",
                "HoSoVayFileDinhKem",
                maHoSo,
                null,
                $"Tên file: {tenFile}",
                diaChiIp
            );
        }

        public async Task GhiNhatKyXoaFileAsync(int maNguoiDung, string tenFile, int? maFile = null, string? diaChiIp = null)
        {
            await GhiNhatKyAsync(
                maNguoiDung,
                "Xóa file đính kèm",
                "HoSoVayFileDinhKem",
                maFile,
                $"Tên file: {tenFile}",
                null,
                diaChiIp
            );
        }
    }
}
