using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuanLyRuiRoTinDung.Models.EF;
using QuanLyRuiRoTinDung.Models.Entities;

namespace QuanLyRuiRoTinDung.Services
{
    public interface IAuthenticationService
    {
        Task<NguoiDung?> AuthenticateAsync(string tenDangNhap, string matKhau);
        Task UpdateLastLoginAsync(int maNguoiDung);
    }

    public class AuthenticationService : IAuthenticationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuthenticationService> _logger;

        public AuthenticationService(ApplicationDbContext context, ILogger<AuthenticationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<NguoiDung?> AuthenticateAsync(string tenDangNhap, string matKhau)
        {
            if (string.IsNullOrWhiteSpace(tenDangNhap) || string.IsNullOrWhiteSpace(matKhau))
            {
                return null;
            }

            // Trim whitespace trước khi query
            tenDangNhap = tenDangNhap.Trim();
            matKhau = matKhau.Trim();

            try
            {
                // Tối ưu: 
                // 1. Sử dụng AsNoTracking() để tắt change tracking (không cần vì chỉ đọc)
                // 2. So sánh trực tiếp với TenDangNhap (đã có index unique) thay vì ToLower()
                // 3. Filter TrangThaiHoatDong trong query thay vì sau khi load
                // 4. Chỉ Include VaiTro khi cần
                var nguoiDung = await _context.NguoiDungs
                    .AsNoTracking() // Tắt tracking để tăng performance đáng kể
                    .Include(u => u.MaVaiTroNavigation)
                    .Where(u => u.TenDangNhap == tenDangNhap && u.TrangThaiHoatDong == true)
                    .FirstOrDefaultAsync();

                if (nguoiDung == null)
                {
                    return null;
                }

                // Kiểm tra mật khẩu (plain text - chưa hash)
                if (nguoiDung.MatKhauHash?.Trim() != matKhau)
                {
                    return null;
                }

                return nguoiDung;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during authentication for user: {TenDangNhap}", tenDangNhap);
                return null;
            }
        }

        public async Task UpdateLastLoginAsync(int maNguoiDung)
        {
            try
            {
                // Tối ưu: Update trực tiếp không cần load entity
                await _context.Database.ExecuteSqlRawAsync(
                    "UPDATE NguoiDung SET LanDangNhapCuoi = GETDATE() WHERE MaNguoiDung = {0}",
                    maNguoiDung);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating last login time for user ID: {MaNguoiDung}", maNguoiDung);
            }
        }
    }
}

