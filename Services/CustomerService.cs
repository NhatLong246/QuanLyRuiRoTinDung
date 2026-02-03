using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuanLyRuiRoTinDung.Models.EF;
using QuanLyRuiRoTinDung.Models.Entities;

namespace QuanLyRuiRoTinDung.Services
{
    public interface ICustomerService
    {
        Task<bool> CheckCustomerExistsAsync(string soCmnd, string? maSoThue = null);
        Task<KhachHangCaNhan?> GetCustomerByCmndAsync(string soCmnd);
        Task<KhachHangDoanhNghiep?> GetCustomerByMstAsync(string maSoThue);
        Task<List<KhachHangDoanhNghiep>> GetDoanhNghiepByCccdAsync(string soCccd);
        Task<bool> CheckTenCongTyExistsAsync(string tenCongTy);
        Task<bool> CheckSoGiayPhepKinhDoanhExistsAsync(string soGiayPhep);
        Task<KhachHangCaNhan> CreateCaNhanAsync(KhachHangCaNhan khachHang, int nguoiTao);
        Task<KhachHangDoanhNghiep> CreateDoanhNghiepAsync(KhachHangDoanhNghiep khachHang, int nguoiTao);
        Task<string> GenerateCustomerCodeAsync(string loai);
        Task<List<KhachHangCaNhan>> GetAllCaNhanAsync();
        Task<List<KhachHangDoanhNghiep>> GetAllDoanhNghiepAsync();
        Task<bool> CheckEmailExistsAsync(string email);
        Task<bool> CheckPhoneExistsAsync(string soDienThoai);
    }

    public class CustomerService : ICustomerService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CustomerService> _logger;

        public CustomerService(ApplicationDbContext context, ILogger<CustomerService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> CheckCustomerExistsAsync(string soCmnd, string? maSoThue = null)
        {
            // Kiểm tra khách hàng cá nhân qua CMND
            if (!string.IsNullOrEmpty(soCmnd))
            {
                var exists = await _context.KhachHangCaNhans
                    .AnyAsync(k => k.SoCmnd == soCmnd && k.TrangThaiHoatDong == true);
                if (exists) return true;
            }

            // Kiểm tra khách hàng doanh nghiệp qua MST
            if (!string.IsNullOrEmpty(maSoThue))
            {
                var exists = await _context.KhachHangDoanhNghieps
                    .AnyAsync(k => k.MaSoThue == maSoThue && k.TrangThaiHoatDong == true);
                if (exists) return true;
            }

            return false;
        }

        public async Task<KhachHangCaNhan?> GetCustomerByCmndAsync(string soCmnd)
        {
            return await _context.KhachHangCaNhans
                .FirstOrDefaultAsync(k => k.SoCmnd == soCmnd && k.TrangThaiHoatDong == true);
        }

        public async Task<KhachHangDoanhNghiep?> GetCustomerByMstAsync(string maSoThue)
        {
            return await _context.KhachHangDoanhNghieps
                .FirstOrDefaultAsync(k => k.MaSoThue == maSoThue && k.TrangThaiHoatDong == true);
        }

        public async Task<List<KhachHangDoanhNghiep>> GetDoanhNghiepByCccdAsync(string soCccd)
        {
            return await _context.KhachHangDoanhNghieps
                .Where(k => k.SoCccdNguoiDaiDienPhapLuat == soCccd && k.TrangThaiHoatDong == true)
                .ToListAsync();
        }

        public async Task<bool> CheckTenCongTyExistsAsync(string tenCongTy)
        {
            return await _context.KhachHangDoanhNghieps
                .AnyAsync(k => k.TenCongTy == tenCongTy && k.TrangThaiHoatDong == true);
        }

        public async Task<bool> CheckSoGiayPhepKinhDoanhExistsAsync(string soGiayPhep)
        {
            if (string.IsNullOrEmpty(soGiayPhep)) return false;
            return await _context.KhachHangDoanhNghieps
                .AnyAsync(k => k.SoGiayPhepKinhDoanh == soGiayPhep && k.TrangThaiHoatDong == true);
        }

        public async Task<KhachHangCaNhan> CreateCaNhanAsync(KhachHangCaNhan khachHang, int nguoiTao)
        {
            // Tạo mã khách hàng nếu chưa có
            if (string.IsNullOrEmpty(khachHang.MaKhachHangCode))
            {
                khachHang.MaKhachHangCode = await GenerateCustomerCodeAsync("CN");
            }

            khachHang.NguoiTao = nguoiTao;
            khachHang.NgayTao = DateTime.Now;
            khachHang.TrangThaiHoatDong = true;

            _context.KhachHangCaNhans.Add(khachHang);
            await _context.SaveChangesAsync();

            return khachHang;
        }

        public async Task<KhachHangDoanhNghiep> CreateDoanhNghiepAsync(KhachHangDoanhNghiep khachHang, int nguoiTao)
        {
            // Tạo mã khách hàng nếu chưa có
            if (string.IsNullOrEmpty(khachHang.MaKhachHangCode))
            {
                khachHang.MaKhachHangCode = await GenerateCustomerCodeAsync("DN");
            }

            khachHang.NguoiTao = nguoiTao;
            khachHang.NgayTao = DateTime.Now;
            khachHang.TrangThaiHoatDong = true;

            _context.KhachHangDoanhNghieps.Add(khachHang);
            await _context.SaveChangesAsync();

            return khachHang;
        }

        public async Task<string> GenerateCustomerCodeAsync(string loai)
        {
            // Lấy số thứ tự lớn nhất hiện tại
            int maxNumber = 0;

            if (loai == "CN")
            {
                var maxCode = await _context.KhachHangCaNhans
                    .Where(k => k.MaKhachHangCode.StartsWith("CN"))
                    .Select(k => k.MaKhachHangCode)
                    .ToListAsync();

                if (maxCode.Any())
                {
                    var numbers = maxCode
                        .Where(c => c.Length >= 3 && int.TryParse(c.Substring(2), out _))
                        .Select(c => int.Parse(c.Substring(2)))
                        .ToList();
                    maxNumber = numbers.Any() ? numbers.Max() : 0;
                }
            }
            else if (loai == "DN")
            {
                var maxCode = await _context.KhachHangDoanhNghieps
                    .Where(k => k.MaKhachHangCode.StartsWith("DN"))
                    .Select(k => k.MaKhachHangCode)
                    .ToListAsync();

                if (maxCode.Any())
                {
                    var numbers = maxCode
                        .Where(c => c.Length >= 3 && int.TryParse(c.Substring(2), out _))
                        .Select(c => int.Parse(c.Substring(2)))
                        .ToList();
                    maxNumber = numbers.Any() ? numbers.Max() : 0;
                }
            }

            // Tạo mã mới
            maxNumber++;
            return $"{loai}{maxNumber:D4}";
        }

        public async Task<List<KhachHangCaNhan>> GetAllCaNhanAsync()
        {
            return await _context.KhachHangCaNhans
                .Where(k => k.TrangThaiHoatDong == true)
                .OrderByDescending(k => k.NgayTao)
                .ToListAsync();
        }

        public async Task<List<KhachHangDoanhNghiep>> GetAllDoanhNghiepAsync()
        {
            return await _context.KhachHangDoanhNghieps
                .Where(k => k.TrangThaiHoatDong == true)
                .OrderByDescending(k => k.NgayTao)
                .ToListAsync();
        }

        public async Task<bool> CheckEmailExistsAsync(string email)
        {
            var existsInCaNhan = await _context.KhachHangCaNhans
                .AnyAsync(k => k.Email == email && k.TrangThaiHoatDong == true);
            
            if (existsInCaNhan) return true;

            var existsInDoanhNghiep = await _context.KhachHangDoanhNghieps
                .AnyAsync(k => k.Email == email && k.TrangThaiHoatDong == true);

            return existsInDoanhNghiep;
        }

        public async Task<bool> CheckPhoneExistsAsync(string soDienThoai)
        {
            if (string.IsNullOrEmpty(soDienThoai)) return false;

            var existsInCaNhan = await _context.KhachHangCaNhans
                .AnyAsync(k => k.SoDienThoai == soDienThoai && k.TrangThaiHoatDong == true);
            
            if (existsInCaNhan) return true;

            var existsInDoanhNghiep = await _context.KhachHangDoanhNghieps
                .AnyAsync(k => k.SoDienThoai == soDienThoai && k.TrangThaiHoatDong == true);

            return existsInDoanhNghiep;
        }
    }
}
