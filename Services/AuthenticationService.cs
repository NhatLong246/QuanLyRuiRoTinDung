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
                _logger.LogWarning("LOGIN FAILED: Empty username or password");
                return null;
            }

            // Trim whitespace trước khi query
            tenDangNhap = tenDangNhap.Trim();
            matKhau = matKhau.Trim();

            _logger.LogInformation("LOGIN ATTEMPT: Username='{User}', Password Length={PassLen}", tenDangNhap, matKhau.Length);

            try
            {
                var nguoiDung = await _context.NguoiDungs
                    .Include(u => u.MaVaiTroNavigation)
                    .Where(u => u.TenDangNhap == tenDangNhap && (u.TrangThaiHoatDong == true || u.TrangThaiHoatDong == null))
                    .FirstOrDefaultAsync();

                if (nguoiDung == null)
                {
                    _logger.LogWarning("LOGIN FAILED: User not found '{TenDangNhap}'", tenDangNhap);
                    return null;
                }

                _logger.LogInformation("USER FOUND: Username='{User}', PasswordInDB='{DBPass}', DBPassLen={DBLen}", 
                    nguoiDung.TenDangNhap, nguoiDung.MatKhauHash, nguoiDung.MatKhauHash?.Length ?? 0);

                var matKhauFromDb = nguoiDung.MatKhauHash?.Trim() ?? string.Empty;
                var matKhauInput = matKhau.Trim();

                if (string.IsNullOrEmpty(matKhauFromDb))
                {
                    _logger.LogWarning("LOGIN FAILED: Password is null or empty for user '{TenDangNhap}'", tenDangNhap);
                    return null;
                }

                _logger.LogInformation("PASSWORD COMPARE: Input='{Input}' vs DB='{DB}' | Match={Match}", 
                    matKhauInput, matKhauFromDb, matKhauFromDb.Equals(matKhauInput, StringComparison.Ordinal));

                if (!matKhauFromDb.Equals(matKhauInput, StringComparison.Ordinal))
                {
                    _logger.LogWarning("LOGIN FAILED: Password mismatch for '{TenDangNhap}'", tenDangNhap);
                    return null;
                }

                _logger.LogInformation("LOGIN SUCCESS: User '{TenDangNhap}' authenticated", tenDangNhap);
                return nguoiDung;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LOGIN ERROR: Exception during authentication for '{TenDangNhap}'", tenDangNhap);
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

