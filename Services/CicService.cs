using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuanLyRuiRoTinDung.Models.EF;
using QuanLyRuiRoTinDung.Models.Entities;

namespace QuanLyRuiRoTinDung.Services
{
    public class CicService : ICicService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CicService> _logger;

        public CicService(ApplicationDbContext context, ILogger<CicService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<ThongTinCic>> GetAllCicAsync(string? searchTerm = null, string? loaiKhachHang = null, string? khuyenNghi = null)
        {
            try
            {
                var query = _context.ThongTinCics
                    .AsNoTracking()
                    .Where(c => c.TrangThaiHoatDong == true)
                    .OrderByDescending(c => c.NgayTraCuuCuoi)
                    .AsQueryable();

                // Lọc theo loại khách hàng
                if (!string.IsNullOrWhiteSpace(loaiKhachHang))
                {
                    query = query.Where(c => c.LoaiKhachHang == loaiKhachHang);
                }

                // Lọc theo khuyến nghị
                if (!string.IsNullOrWhiteSpace(khuyenNghi))
                {
                    query = query.Where(c => c.KhuyenNghiChoVay == khuyenNghi);
                }

                // Tìm kiếm
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    searchTerm = searchTerm.Trim();
                    query = query.Where(c =>
                        c.SoCmndCccd.Contains(searchTerm) ||
                        (c.MaSoThue != null && c.MaSoThue.Contains(searchTerm)) ||
                        (c.HoTen != null && c.HoTen.Contains(searchTerm)) ||
                        c.MaCicCode.Contains(searchTerm));
                }

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting CIC list");
                return new List<ThongTinCic>();
            }
        }

        public async Task<ThongTinCic?> GetCicByIdAsync(int maCic)
        {
            try
            {
                return await _context.ThongTinCics
                    .AsNoTracking()
                    .Include(c => c.NguoiTraCuuNavigation)
                    .Include(c => c.NguoiTaoNavigation)
                    .Include(c => c.NguoiCapNhatNavigation)
                    .Include(c => c.LichSuTraCuuCics.OrderByDescending(l => l.NgayTraCuu).Take(10))
                        .ThenInclude(l => l.NguoiTraCuuNavigation)
                    .FirstOrDefaultAsync(c => c.MaCic == maCic && c.TrangThaiHoatDong == true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting CIC by ID: {MaCic}", maCic);
                return null;
            }
        }

        public async Task<ThongTinCic?> GetCicByCmndAsync(string soCmndCccd)
        {
            try
            {
                return await _context.ThongTinCics
                    .AsNoTracking()
                    .Where(c => c.SoCmndCccd == soCmndCccd && c.TrangThaiHoatDong == true)
                    .OrderByDescending(c => c.NgayTraCuuCuoi)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting CIC by CMND: {SoCmndCccd}", soCmndCccd);
                return null;
            }
        }

        public async Task<ThongTinCic?> GetCicByMstAsync(string maSoThue)
        {
            try
            {
                return await _context.ThongTinCics
                    .AsNoTracking()
                    .Where(c => c.MaSoThue == maSoThue && c.TrangThaiHoatDong == true)
                    .OrderByDescending(c => c.NgayTraCuuCuoi)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting CIC by MST: {MaSoThue}", maSoThue);
                return null;
            }
        }

        public async Task<int> GetTotalCicCountAsync()
        {
            try
            {
                return await _context.ThongTinCics
                    .Where(c => c.TrangThaiHoatDong == true)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total CIC count");
                return 0;
            }
        }
    }
}
