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
        Task<bool> CheckEmailExistsAsync(string email, int? excludeId = null, string? customerType = null);
        Task<bool> CheckPhoneExistsAsync(string soDienThoai, int? excludeId = null, string? customerType = null);
        Task<ThongTinCic?> GetCicByCccdAsync(string soCccd);
        Task<ThongTinCic?> GetCicByMstAsync(string maSoThue);
        Task<ThongTinCic?> GetCicByCccdAndLoaiAsync(string soCccd, string loaiKhachHang);
        Task<ThongTinCic> CreateCicAsync(string loaiKhachHang, int maKhachHang, string soCmndCccd, string? maSoThue, string hoTen, int nguoiTao);
        Task<(bool isValid, string? errorMessage, string? fieldName)> ValidateCicForCaNhanAsync(string soCccd, string hoTen);
        Task<(bool isValid, string? errorMessage, string? fieldName)> ValidateCicForDoanhNghiepAsync(string maSoThue, string? soCccdNguoiDaiDien, string tenCongTy);

        // Cross-validation giữa Cá nhân và Doanh nghiệp theo CCCD
        Task<CrossCccdValidationResult> ValidateCrossCheckCccdForDoanhNghiepAsync(string soCccd, string? nguoiDaiDien, DateOnly? ngaySinh, string? gioiTinh);
        Task<CrossCccdValidationResult> ValidateCrossCheckCccdForCaNhanAsync(string soCccd, string? hoTen, DateOnly? ngaySinh, string? gioiTinh);

        // Quản lý khách hàng - lấy danh sách
        Task<List<KhachHangCaNhan>> GetCaNhanByNguoiTaoAsync(int nguoiTao, string? searchTerm = null);
        Task<List<KhachHangDoanhNghiep>> GetDoanhNghiepByNguoiTaoAsync(int nguoiTao, string? searchTerm = null);
        Task<List<KhachHangCaNhan>> GetAllCaNhanWithFilterAsync(string? searchTerm = null);
        Task<List<KhachHangDoanhNghiep>> GetAllDoanhNghiepWithFilterAsync(string? searchTerm = null);
        Task<int> GetTotalCaNhanCountAsync();
        Task<int> GetTotalDoanhNghiepCountAsync();
        Task<int> GetCaNhanCountByNguoiTaoAsync(int nguoiTao);
        Task<int> GetDoanhNghiepCountByNguoiTaoAsync(int nguoiTao);

        // Chi tiết khách hàng
        Task<KhachHangCaNhan?> GetCaNhanByIdAsync(int maKhachHang);
        Task<KhachHangDoanhNghiep?> GetDoanhNghiepByIdAsync(int maKhachHang);

        // Cập nhật khách hàng
        Task<KhachHangCaNhan> UpdateCaNhanAsync(KhachHangCaNhan khachHang);
        Task<KhachHangDoanhNghiep> UpdateDoanhNghiepAsync(KhachHangDoanhNghiep khachHang);

        // New methods for realtime validation
        Task<KhachHangDoanhNghiep?> GetCustomerByGiayPhepAsync(string soGiayPhep);
        Task<CrossValidateCICResult> CrossValidateCICForDoanhNghiepAsync(string maSoThue, string tenCongTy, string? cccd);
        Task<CrossCheckNguoiDaiDienResult> CrossCheckNguoiDaiDienWithCICAsync(string soCccd, string? nguoiDaiDien, DateOnly? ngaySinh, string? gioiTinh, int? excludeDoanhNghiepId = null);
    }

    // DTO for CIC cross-validation result
    public class CrossValidateCICResult
    {
        public bool HasCicRecord { get; set; }
        public bool MaSoThueMismatch { get; set; }
        public bool TenCongTyMismatch { get; set; }
        public bool CccdMismatch { get; set; }
        public string? CicMaSoThue { get; set; }
        public string? CicTenCongTy { get; set; }
        public string? CicCccd { get; set; }
    }

    // DTO for nguoi dai dien cross-check result
    public class CrossCheckNguoiDaiDienResult
    {
        public List<ValidationWarning> Warnings { get; set; } = new();
        public ReferenceData? ReferenceData { get; set; }
    }

    // DTO for reference data from system
    public class ReferenceData
    {
        public string? HoTen { get; set; }
        public string? NgaySinh { get; set; }
        public string? GioiTinh { get; set; }
        public string? Source { get; set; }
    }

    public class ValidationWarning
    {
        public string FieldName { get; set; } = "";
        public string Message { get; set; } = "";
    }

    // DTO cho kết quả cross-validation CCCD
    public class CrossCccdValidationResult
    {
        public bool HasExistingData { get; set; }
        public bool IsValid { get; set; }
        public List<CrossCccdValidationError> Errors { get; set; } = new();
        public string? SourceType { get; set; } // "CaNhan", "DoanhNghiep", "CicCaNhan"

        // Thông tin tham chiếu từ nguồn
        public string? RefHoTen { get; set; }
        public DateOnly? RefNgaySinh { get; set; }
        public string? RefGioiTinh { get; set; }
    }

    public class CrossCccdValidationError
    {
        public string FieldName { get; set; } = "";
        public string ErrorMessage { get; set; } = "";
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

            // Kiểm tra và tạo bản ghi CIC nếu chưa có (chỉ tìm CIC loại Cá nhân)
            if (!string.IsNullOrEmpty(khachHang.SoCmnd))
            {
                var existingCic = await GetCicByCccdAndLoaiAsync(khachHang.SoCmnd, "CaNhan");
                if (existingCic == null)
                {
                    // Tạo bản ghi CIC mới với thông tin tốt
                    await CreateCicAsync(
                        loaiKhachHang: "CaNhan",
                        maKhachHang: khachHang.MaKhachHang,
                        soCmndCccd: khachHang.SoCmnd,
                        maSoThue: null,
                        hoTen: khachHang.HoTen ?? "",
                        nguoiTao: nguoiTao
                    );
                    _logger.LogInformation("Đã tạo bản ghi CIC mới cho khách hàng cá nhân: {CCCD}", khachHang.SoCmnd);
                }
                else
                {
                    // Cập nhật liên kết MaKhachHang nếu chưa có
                    if (existingCic.MaKhachHang == null || existingCic.MaKhachHang == 0)
                    {
                        existingCic.MaKhachHang = khachHang.MaKhachHang;
                        existingCic.NgayCapNhat = DateTime.Now;
                        existingCic.NguoiCapNhat = nguoiTao;
                        await _context.SaveChangesAsync();
                        _logger.LogInformation("Đã liên kết CIC có sẵn với khách hàng cá nhân: {CCCD}", khachHang.SoCmnd);
                    }
                }
            }

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

            // Kiểm tra và tạo bản ghi CIC nếu chưa có (dựa vào MST cho doanh nghiệp)
            if (!string.IsNullOrEmpty(khachHang.MaSoThue))
            {
                var existingCic = await GetCicByMstAsync(khachHang.MaSoThue);
                if (existingCic == null)
                {
                    // Tạo bản ghi CIC mới với thông tin tốt cho doanh nghiệp
                    await CreateCicAsync(
                        loaiKhachHang: "DoanhNghiep",
                        maKhachHang: khachHang.MaKhachHang,
                        soCmndCccd: khachHang.SoCccdNguoiDaiDienPhapLuat ?? "",
                        maSoThue: khachHang.MaSoThue,
                        hoTen: khachHang.TenCongTy ?? "",
                        nguoiTao: nguoiTao
                    );
                    _logger.LogInformation("Đã tạo bản ghi CIC mới cho doanh nghiệp: {MST}", khachHang.MaSoThue);
                }
                else
                {
                    // Cập nhật liên kết MaKhachHang nếu chưa có
                    if (existingCic.MaKhachHang == null || existingCic.MaKhachHang == 0)
                    {
                        existingCic.MaKhachHang = khachHang.MaKhachHang;
                        existingCic.NgayCapNhat = DateTime.Now;
                        existingCic.NguoiCapNhat = nguoiTao;
                        await _context.SaveChangesAsync();
                        _logger.LogInformation("Đã liên kết CIC có sẵn với doanh nghiệp: {MST}", khachHang.MaSoThue);
                    }
                }
            }

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

        public async Task<bool> CheckEmailExistsAsync(string email, int? excludeId = null, string? customerType = null)
        {
            if (string.IsNullOrEmpty(email)) return false;

            bool existsInCaNhan = false;
            bool existsInDoanhNghiep = false;

            if (customerType == null || customerType == "canhan")
            {
                var query = _context.KhachHangCaNhans
                    .Where(k => k.Email == email && k.TrangThaiHoatDong == true);
                
                if (excludeId.HasValue && customerType == "canhan")
                {
                    query = query.Where(k => k.MaKhachHang != excludeId.Value);
                }
                
                existsInCaNhan = await query.AnyAsync();
            }

            if (customerType == null || customerType == "doanhnghiep")
            {
                var query = _context.KhachHangDoanhNghieps
                    .Where(k => k.Email == email && k.TrangThaiHoatDong == true);
                
                if (excludeId.HasValue && customerType == "doanhnghiep")
                {
                    query = query.Where(k => k.MaKhachHang != excludeId.Value);
                }
                
                existsInDoanhNghiep = await query.AnyAsync();
            }

            return existsInCaNhan || existsInDoanhNghiep;
        }

        public async Task<bool> CheckPhoneExistsAsync(string soDienThoai, int? excludeId = null, string? customerType = null)
        {
            if (string.IsNullOrEmpty(soDienThoai)) return false;

            bool existsInCaNhan = false;
            bool existsInDoanhNghiep = false;

            if (customerType == null || customerType == "canhan")
            {
                var query = _context.KhachHangCaNhans
                    .Where(k => k.SoDienThoai == soDienThoai && k.TrangThaiHoatDong == true);
                
                if (excludeId.HasValue && customerType == "canhan")
                {
                    query = query.Where(k => k.MaKhachHang != excludeId.Value);
                }
                
                existsInCaNhan = await query.AnyAsync();
            }

            if (customerType == null || customerType == "doanhnghiep")
            {
                var query = _context.KhachHangDoanhNghieps
                    .Where(k => k.SoDienThoai == soDienThoai && k.TrangThaiHoatDong == true);
                
                if (excludeId.HasValue && customerType == "doanhnghiep")
                {
                    query = query.Where(k => k.MaKhachHang != excludeId.Value);
                }
                
                existsInDoanhNghiep = await query.AnyAsync();
            }

            return existsInCaNhan || existsInDoanhNghiep;
        }

        // ============ CIC Methods ============

        public async Task<ThongTinCic?> GetCicByCccdAsync(string soCccd)
        {
            if (string.IsNullOrEmpty(soCccd)) return null;
            return await _context.ThongTinCics
                .FirstOrDefaultAsync(c => c.SoCmndCccd == soCccd && c.TrangThaiHoatDong == true);
        }

        public async Task<ThongTinCic?> GetCicByMstAsync(string maSoThue)
        {
            if (string.IsNullOrEmpty(maSoThue)) return null;
            return await _context.ThongTinCics
                .FirstOrDefaultAsync(c => c.MaSoThue == maSoThue && c.LoaiKhachHang == "DoanhNghiep" && c.TrangThaiHoatDong == true);
        }

        public async Task<ThongTinCic?> GetCicByCccdAndLoaiAsync(string soCccd, string loaiKhachHang)
        {
            if (string.IsNullOrEmpty(soCccd)) return null;
            return await _context.ThongTinCics
                .FirstOrDefaultAsync(c => c.SoCmndCccd == soCccd && c.LoaiKhachHang == loaiKhachHang && c.TrangThaiHoatDong == true);
        }

        /// <summary>
        /// Validate thông tin khách hàng cá nhân với CIC
        /// Kiểm tra: Nếu CCCD đã tồn tại trong CIC (loại Cá nhân), tên phải khớp
        /// </summary>
        public async Task<(bool isValid, string? errorMessage, string? fieldName)> ValidateCicForCaNhanAsync(string soCccd, string hoTen)
        {
            if (string.IsNullOrEmpty(soCccd)) return (true, null, null);

            // Tìm CIC cá nhân theo CCCD
            var existingCic = await _context.ThongTinCics
                .FirstOrDefaultAsync(c => c.SoCmndCccd == soCccd && c.LoaiKhachHang == "CaNhan" && c.TrangThaiHoatDong == true);

            if (existingCic != null)
            {
                // Kiểm tra tên phải khớp
                if (!string.IsNullOrEmpty(existingCic.HoTen) && !string.IsNullOrEmpty(hoTen))
                {
                    if (!existingCic.HoTen.Equals(hoTen, StringComparison.OrdinalIgnoreCase))
                    {
                        return (false, $"Họ tên phải khớp với thông tin CIC đã đăng ký với CCCD này. Tên trong CIC: {existingCic.HoTen}", "HoTen");
                    }
                }
            }

            return (true, null, null);
        }

        /// <summary>
        /// Validate thông tin doanh nghiệp với CIC
        /// Kiểm tra: Nếu MST đã tồn tại trong CIC (loại Doanh nghiệp), CCCD người đại diện và tên công ty phải khớp
        /// </summary>
        public async Task<(bool isValid, string? errorMessage, string? fieldName)> ValidateCicForDoanhNghiepAsync(string maSoThue, string? soCccdNguoiDaiDien, string tenCongTy)
        {
            if (string.IsNullOrEmpty(maSoThue)) return (true, null, null);

            // Tìm CIC doanh nghiệp theo MST
            var existingCic = await _context.ThongTinCics
                .FirstOrDefaultAsync(c => c.MaSoThue == maSoThue && c.LoaiKhachHang == "DoanhNghiep" && c.TrangThaiHoatDong == true);

            if (existingCic != null)
            {
                // Kiểm tra CCCD người đại diện phải khớp
                if (!string.IsNullOrEmpty(existingCic.SoCmndCccd) && !string.IsNullOrEmpty(soCccdNguoiDaiDien))
                {
                    if (existingCic.SoCmndCccd != soCccdNguoiDaiDien)
                    {
                        return (false, $"Số CCCD người đại diện pháp luật phải khớp với thông tin CIC đã đăng ký với MST này. CCCD trong CIC: {existingCic.SoCmndCccd}", "SoCccdNguoiDaiDienPhapLuat");
                    }
                }

                // Kiểm tra tên công ty phải khớp
                if (!string.IsNullOrEmpty(existingCic.HoTen) && !string.IsNullOrEmpty(tenCongTy))
                {
                    if (!existingCic.HoTen.Equals(tenCongTy, StringComparison.OrdinalIgnoreCase))
                    {
                        return (false, $"Tên công ty phải khớp với thông tin CIC đã đăng ký với MST này. Tên trong CIC: {existingCic.HoTen}", "TenCongTy");
                    }
                }
            }

            return (true, null, null);
        }

        public async Task<ThongTinCic> CreateCicAsync(string loaiKhachHang, int maKhachHang, string soCmndCccd, string? maSoThue, string hoTen, int nguoiTao)
        {
            // Tạo mã CIC mới
            var maCicCode = await GenerateCicCodeAsync();

            var cicRecord = new ThongTinCic
            {
                MaCicCode = maCicCode,
                LoaiKhachHang = loaiKhachHang,
                MaKhachHang = maKhachHang,
                SoCmndCccd = soCmndCccd,
                MaSoThue = maSoThue,
                HoTen = hoTen,

                // Thông tin tín dụng - tất cả đều tốt (0 khoản vay, không nợ xấu)
                TongSoKhoanVayCic = 0,
                SoKhoanVayDangVayCic = 0,
                SoKhoanVayDaTraXongCic = 0,
                SoKhoanVayQuaHanCic = 0,
                SoKhoanVayNoXauCic = 0,

                // Dư nợ - tất cả = 0
                TongDuNoCic = 0,
                DuNoQuaHanCic = 0,
                DuNoNoXauCic = 0,
                DuNoToiDaCic = 0,
                TongGiaTriVayCic = 0,

                // Điểm tín dụng tốt
                DiemTinDungCic = 750, // Điểm tín dụng tốt
                XepHangTinDungCic = "A", // Xếp hạng A
                MucDoRuiRo = "Thấp",
                KhaNangTraNo = "Tốt",

                // Lịch sử - không có quá hạn, nợ xấu
                SoLanQuaHanCic = 0,
                SoLanNoXauCic = 0,
                SoNgayQuaHanToiDaCic = 0,
                NgayQuaHanLanCuoiCic = null,
                NgayNoXauLanCuoiCic = null,
                ThoiGianTraNoTotCic = 0,
                TyLeTraNoDungHanCic = 100, // 100% trả nợ đúng hạn

                // Tổ chức tín dụng
                DanhSachToChucTinDung = null,
                SoToChucTinDungDaVay = 0,

                // Đánh giá
                DanhGiaTongQuat = "Khách hàng mới, chưa có lịch sử tín dụng. Đủ điều kiện vay vốn.",
                KhuyenNghiChoVay = "Đồng ý",
                LyDoKhuyenNghi = "Khách hàng mới chưa có nợ xấu hoặc lịch sử tín dụng tiêu cực.",

                // Thông tin tra cứu
                NgayTraCuuCuoi = DateTime.Now,
                NguoiTraCuu = nguoiTao,
                KetQuaTraCuu = "Thành công",
                ThongTinTraVeCic = "Khách hàng mới - Tự động tạo khi tạo tài khoản",

                // Metadata
                NgayTao = DateTime.Now,
                NguoiTao = nguoiTao,
                TrangThaiHoatDong = true
            };

            _context.ThongTinCics.Add(cicRecord);
            await _context.SaveChangesAsync();

            return cicRecord;
        }

        private async Task<string> GenerateCicCodeAsync()
        {
            // Tạo mã CIC: CIC + năm + số thứ tự
            var year = DateTime.Now.Year;
            var prefix = $"CIC{year}";

            var maxCode = await _context.ThongTinCics
                .Where(c => c.MaCicCode.StartsWith(prefix))
                .Select(c => c.MaCicCode)
                .ToListAsync();

            int maxNumber = 0;
            if (maxCode.Any())
            {
                var numbers = maxCode
                    .Where(c => c.Length >= prefix.Length + 1 && int.TryParse(c.Substring(prefix.Length), out _))
                    .Select(c => int.Parse(c.Substring(prefix.Length)))
                    .ToList();
                maxNumber = numbers.Any() ? numbers.Max() : 0;
            }

            maxNumber++;
            return $"{prefix}{maxNumber:D4}";
        }

        /// <summary>
        /// Cross-validation: Kiểm tra CCCD người đại diện của Doanh nghiệp với thông tin từ Khách hàng cá nhân/CIC cá nhân
        /// </summary>
        public async Task<CrossCccdValidationResult> ValidateCrossCheckCccdForDoanhNghiepAsync(
            string soCccd, string? nguoiDaiDien, DateOnly? ngaySinh, string? gioiTinh)
        {
            var result = new CrossCccdValidationResult { HasExistingData = false, IsValid = true };

            if (string.IsNullOrEmpty(soCccd)) return result;

            // 1. Kiểm tra trong Khách hàng cá nhân trước
            var existingCaNhan = await _context.KhachHangCaNhans
                .FirstOrDefaultAsync(k => k.SoCmnd == soCccd && k.TrangThaiHoatDong == true);

            if (existingCaNhan != null)
            {
                result.HasExistingData = true;
                result.SourceType = "CaNhan";
                result.RefHoTen = existingCaNhan.HoTen;
                result.RefNgaySinh = existingCaNhan.NgaySinh;
                result.RefGioiTinh = existingCaNhan.GioiTinh;

                // Kiểm tra tên người đại diện vs họ tên cá nhân
                if (!string.IsNullOrEmpty(existingCaNhan.HoTen) && !string.IsNullOrEmpty(nguoiDaiDien))
                {
                    if (!existingCaNhan.HoTen.Equals(nguoiDaiDien, StringComparison.OrdinalIgnoreCase))
                    {
                        result.IsValid = false;
                        result.Errors.Add(new CrossCccdValidationError
                        {
                            FieldName = "NguoiDaiDienPhapLuat",
                            ErrorMessage = $"Tên người đại diện pháp luật phải khớp với họ tên khách hàng cá nhân đã đăng ký với CCCD này. Họ tên: {existingCaNhan.HoTen}"
                        });
                    }
                }

                // Kiểm tra ngày sinh
                if (existingCaNhan.NgaySinh.HasValue && ngaySinh.HasValue)
                {
                    if (existingCaNhan.NgaySinh.Value != ngaySinh.Value)
                    {
                        result.IsValid = false;
                        result.Errors.Add(new CrossCccdValidationError
                        {
                            FieldName = "NgaySinh",
                            ErrorMessage = $"Ngày sinh người đại diện phải khớp với ngày sinh khách hàng cá nhân đã đăng ký với CCCD này. Ngày sinh: {existingCaNhan.NgaySinh.Value:dd/MM/yyyy}"
                        });
                    }
                }

                // Kiểm tra giới tính
                if (!string.IsNullOrEmpty(existingCaNhan.GioiTinh) && !string.IsNullOrEmpty(gioiTinh))
                {
                    if (existingCaNhan.GioiTinh != gioiTinh)
                    {
                        result.IsValid = false;
                        result.Errors.Add(new CrossCccdValidationError
                        {
                            FieldName = "GioiTinh",
                            ErrorMessage = $"Giới tính người đại diện phải khớp với giới tính khách hàng cá nhân đã đăng ký với CCCD này. Giới tính: {existingCaNhan.GioiTinh}"
                        });
                    }
                }

                return result;
            }

            // 2. Kiểm tra trong CIC cá nhân
            var existingCicCaNhan = await _context.ThongTinCics
                .FirstOrDefaultAsync(c => c.SoCmndCccd == soCccd && c.LoaiKhachHang == "CaNhan" && c.TrangThaiHoatDong == true);

            if (existingCicCaNhan != null)
            {
                result.HasExistingData = true;
                result.SourceType = "CicCaNhan";
                result.RefHoTen = existingCicCaNhan.HoTen;

                // Kiểm tra tên người đại diện vs họ tên trong CIC
                if (!string.IsNullOrEmpty(existingCicCaNhan.HoTen) && !string.IsNullOrEmpty(nguoiDaiDien))
                {
                    if (!existingCicCaNhan.HoTen.Equals(nguoiDaiDien, StringComparison.OrdinalIgnoreCase))
                    {
                        result.IsValid = false;
                        result.Errors.Add(new CrossCccdValidationError
                        {
                            FieldName = "NguoiDaiDienPhapLuat",
                            ErrorMessage = $"Tên người đại diện pháp luật phải khớp với họ tên trong CIC cá nhân đã đăng ký với CCCD này. Họ tên: {existingCicCaNhan.HoTen}"
                        });
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Cross-validation: Kiểm tra CCCD Khách hàng cá nhân với thông tin từ Doanh nghiệp (người đại diện)
        /// </summary>
        public async Task<CrossCccdValidationResult> ValidateCrossCheckCccdForCaNhanAsync(
            string soCccd, string? hoTen, DateOnly? ngaySinh, string? gioiTinh)
        {
            var result = new CrossCccdValidationResult { HasExistingData = false, IsValid = true };

            if (string.IsNullOrEmpty(soCccd)) return result;

            // Kiểm tra trong Doanh nghiệp - tìm các doanh nghiệp có CCCD người đại diện trùng
            var existingDoanhNghieps = await _context.KhachHangDoanhNghieps
                .Where(k => k.SoCccdNguoiDaiDienPhapLuat == soCccd && k.TrangThaiHoatDong == true)
                .ToListAsync();

            if (existingDoanhNghieps.Any())
            {
                var firstDoanhNghiep = existingDoanhNghieps.First();
                result.HasExistingData = true;
                result.SourceType = "DoanhNghiep";
                result.RefHoTen = firstDoanhNghiep.NguoiDaiDienPhapLuat;
                result.RefNgaySinh = firstDoanhNghiep.NgaySinh;
                result.RefGioiTinh = firstDoanhNghiep.GioiTinh;

                // Kiểm tra họ tên vs tên người đại diện
                if (!string.IsNullOrEmpty(firstDoanhNghiep.NguoiDaiDienPhapLuat) && !string.IsNullOrEmpty(hoTen))
                {
                    if (!firstDoanhNghiep.NguoiDaiDienPhapLuat.Equals(hoTen, StringComparison.OrdinalIgnoreCase))
                    {
                        result.IsValid = false;
                        result.Errors.Add(new CrossCccdValidationError
                        {
                            FieldName = "HoTen",
                            ErrorMessage = $"Họ tên phải khớp với tên người đại diện pháp luật đã đăng ký với CCCD này trong doanh nghiệp {firstDoanhNghiep.TenCongTy}. Tên người đại diện: {firstDoanhNghiep.NguoiDaiDienPhapLuat}"
                        });
                    }
                }

                // Kiểm tra ngày sinh
                if (firstDoanhNghiep.NgaySinh.HasValue && ngaySinh.HasValue)
                {
                    if (firstDoanhNghiep.NgaySinh.Value != ngaySinh.Value)
                    {
                        result.IsValid = false;
                        result.Errors.Add(new CrossCccdValidationError
                        {
                            FieldName = "NgaySinh",
                            ErrorMessage = $"Ngày sinh phải khớp với ngày sinh người đại diện đã đăng ký với CCCD này trong doanh nghiệp. Ngày sinh: {firstDoanhNghiep.NgaySinh.Value:dd/MM/yyyy}"
                        });
                    }
                }

                // Kiểm tra giới tính
                if (!string.IsNullOrEmpty(firstDoanhNghiep.GioiTinh) && !string.IsNullOrEmpty(gioiTinh))
                {
                    if (firstDoanhNghiep.GioiTinh != gioiTinh)
                    {
                        result.IsValid = false;
                        result.Errors.Add(new CrossCccdValidationError
                        {
                            FieldName = "GioiTinh",
                            ErrorMessage = $"Giới tính phải khớp với giới tính người đại diện đã đăng ký với CCCD này trong doanh nghiệp. Giới tính: {firstDoanhNghiep.GioiTinh}"
                        });
                    }
                }
            }

            return result;
        }

        // ========== QUẢN LÝ KHÁCH HÀNG - CÁC PHƯƠNG THỨC MỚI ==========

        public async Task<List<KhachHangCaNhan>> GetCaNhanByNguoiTaoAsync(int nguoiTao, string? searchTerm = null)
        {
            var query = _context.KhachHangCaNhans
                .Include(k => k.NguoiTaoNavigation)
                .Where(k => k.NguoiTao == nguoiTao && k.TrangThaiHoatDong == true);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(k =>
                    (k.HoTen != null && k.HoTen.ToLower().Contains(searchTerm)) ||
                    (k.SoCmnd != null && k.SoCmnd.Contains(searchTerm)) ||
                    (k.SoDienThoai != null && k.SoDienThoai.Contains(searchTerm)) ||
                    (k.Email != null && k.Email.ToLower().Contains(searchTerm)) ||
                    (k.MaKhachHangCode != null && k.MaKhachHangCode.ToLower().Contains(searchTerm))
                );
            }

            return await query
                .OrderByDescending(k => k.NgayTao)
                .ToListAsync();
        }

        public async Task<List<KhachHangDoanhNghiep>> GetDoanhNghiepByNguoiTaoAsync(int nguoiTao, string? searchTerm = null)
        {
            var query = _context.KhachHangDoanhNghieps
                .Include(k => k.NguoiTaoNavigation)
                .Where(k => k.NguoiTao == nguoiTao && k.TrangThaiHoatDong == true);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(k =>
                    (k.TenCongTy != null && k.TenCongTy.ToLower().Contains(searchTerm)) ||
                    (k.MaSoThue != null && k.MaSoThue.Contains(searchTerm)) ||
                    (k.SoDienThoai != null && k.SoDienThoai.Contains(searchTerm)) ||
                    (k.Email != null && k.Email.ToLower().Contains(searchTerm)) ||
                    (k.MaKhachHangCode != null && k.MaKhachHangCode.ToLower().Contains(searchTerm)) ||
                    (k.NguoiDaiDienPhapLuat != null && k.NguoiDaiDienPhapLuat.ToLower().Contains(searchTerm))
                );
            }

            return await query
                .OrderByDescending(k => k.NgayTao)
                .ToListAsync();
        }

        public async Task<List<KhachHangCaNhan>> GetAllCaNhanWithFilterAsync(string? searchTerm = null)
        {
            var query = _context.KhachHangCaNhans
                .Include(k => k.NguoiTaoNavigation)
                .Where(k => k.TrangThaiHoatDong == true);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(k =>
                    (k.HoTen != null && k.HoTen.ToLower().Contains(searchTerm)) ||
                    (k.SoCmnd != null && k.SoCmnd.Contains(searchTerm)) ||
                    (k.SoDienThoai != null && k.SoDienThoai.Contains(searchTerm)) ||
                    (k.Email != null && k.Email.ToLower().Contains(searchTerm)) ||
                    (k.MaKhachHangCode != null && k.MaKhachHangCode.ToLower().Contains(searchTerm))
                );
            }

            return await query
                .OrderByDescending(k => k.NgayTao)
                .ToListAsync();
        }

        public async Task<List<KhachHangDoanhNghiep>> GetAllDoanhNghiepWithFilterAsync(string? searchTerm = null)
        {
            var query = _context.KhachHangDoanhNghieps
                .Include(k => k.NguoiTaoNavigation)
                .Where(k => k.TrangThaiHoatDong == true);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(k =>
                    (k.TenCongTy != null && k.TenCongTy.ToLower().Contains(searchTerm)) ||
                    (k.MaSoThue != null && k.MaSoThue.Contains(searchTerm)) ||
                    (k.SoDienThoai != null && k.SoDienThoai.Contains(searchTerm)) ||
                    (k.Email != null && k.Email.ToLower().Contains(searchTerm)) ||
                    (k.MaKhachHangCode != null && k.MaKhachHangCode.ToLower().Contains(searchTerm)) ||
                    (k.NguoiDaiDienPhapLuat != null && k.NguoiDaiDienPhapLuat.ToLower().Contains(searchTerm))
                );
            }

            return await query
                .OrderByDescending(k => k.NgayTao)
                .ToListAsync();
        }

        public async Task<int> GetTotalCaNhanCountAsync()
        {
            return await _context.KhachHangCaNhans
                .CountAsync(k => k.TrangThaiHoatDong == true);
        }

        public async Task<int> GetTotalDoanhNghiepCountAsync()
        {
            return await _context.KhachHangDoanhNghieps
                .CountAsync(k => k.TrangThaiHoatDong == true);
        }

        public async Task<int> GetCaNhanCountByNguoiTaoAsync(int nguoiTao)
        {
            return await _context.KhachHangCaNhans
                .CountAsync(k => k.NguoiTao == nguoiTao && k.TrangThaiHoatDong == true);
        }

        public async Task<int> GetDoanhNghiepCountByNguoiTaoAsync(int nguoiTao)
        {
            return await _context.KhachHangDoanhNghieps
                .CountAsync(k => k.NguoiTao == nguoiTao && k.TrangThaiHoatDong == true);
        }

        public async Task<KhachHangCaNhan?> GetCaNhanByIdAsync(int maKhachHang)
        {
            var khachHang = await _context.KhachHangCaNhans
                .Include(k => k.NguoiTaoNavigation)
                .FirstOrDefaultAsync(k => k.MaKhachHang == maKhachHang);

            if (khachHang != null && !string.IsNullOrEmpty(khachHang.SoCmnd))
            {
                // Load CIC information for this customer
                var cic = await GetCicByCccdAndLoaiAsync(khachHang.SoCmnd, "CaNhan");
                if (cic != null)
                {
                    // Update customer's credit score and rating from CIC
                    khachHang.DiemTinDung = cic.DiemTinDungCic;
                    khachHang.XepHangTinDung = cic.XepHangTinDungCic;
                }
            }

            return khachHang;
        }

        public async Task<KhachHangDoanhNghiep?> GetDoanhNghiepByIdAsync(int maKhachHang)
        {
            var khachHang = await _context.KhachHangDoanhNghieps
                .Include(k => k.NguoiTaoNavigation)
                .FirstOrDefaultAsync(k => k.MaKhachHang == maKhachHang);

            if (khachHang != null && !string.IsNullOrEmpty(khachHang.MaSoThue))
            {
                // Load CIC information for this business
                var cic = await GetCicByMstAsync(khachHang.MaSoThue);
                if (cic != null)
                {
                    // Update business's credit score and rating from CIC
                    khachHang.DiemTinDung = cic.DiemTinDungCic;
                    khachHang.XepHangTinDung = cic.XepHangTinDungCic;
                }
            }

            return khachHang;
        }

        // Cập nhật khách hàng cá nhân
        public async Task<KhachHangCaNhan> UpdateCaNhanAsync(KhachHangCaNhan khachHang)
        {
            khachHang.NgayCapNhat = DateTime.Now;
            _context.KhachHangCaNhans.Update(khachHang);
            await _context.SaveChangesAsync();
            return khachHang;
        }

        // Cập nhật khách hàng doanh nghiệp
        public async Task<KhachHangDoanhNghiep> UpdateDoanhNghiepAsync(KhachHangDoanhNghiep khachHang)
        {
            khachHang.NgayCapNhat = DateTime.Now;
            _context.KhachHangDoanhNghieps.Update(khachHang);
            await _context.SaveChangesAsync();
            return khachHang;
        }

        // Lấy doanh nghiệp theo giấy phép kinh doanh
        public async Task<KhachHangDoanhNghiep?> GetCustomerByGiayPhepAsync(string soGiayPhep)
        {
            if (string.IsNullOrEmpty(soGiayPhep))
                return null;

            return await _context.KhachHangDoanhNghieps
                .FirstOrDefaultAsync(k => k.SoGiayPhepKinhDoanh == soGiayPhep && k.TrangThaiHoatDong == true);
        }

        // Cross-validate doanh nghiệp với CIC (Mã số thuế, Tên công ty, CCCD người đại diện)
        public async Task<CrossValidateCICResult> CrossValidateCICForDoanhNghiepAsync(string maSoThue, string tenCongTy, string? cccd)
        {
            var result = new CrossValidateCICResult();

            // Tìm CIC record theo Mã số thuế
            var cicRecord = await GetCicByMstAsync(maSoThue);

            if (cicRecord == null)
            {
                result.HasCicRecord = false;
                return result;
            }

            result.HasCicRecord = true;
            result.CicMaSoThue = cicRecord.MaSoThue;
            result.CicTenCongTy = cicRecord.HoTen;
            result.CicCccd = cicRecord.SoCmndCccd;

            // Kiểm tra Mã số thuế
            if (!string.IsNullOrEmpty(cicRecord.MaSoThue) && cicRecord.MaSoThue != maSoThue)
            {
                result.MaSoThueMismatch = true;
            }

            // Kiểm tra Tên công ty (so sánh không phân biệt hoa thường)
            if (!string.IsNullOrEmpty(cicRecord.HoTen) &&
                !cicRecord.HoTen.Equals(tenCongTy, StringComparison.OrdinalIgnoreCase))
            {
                result.TenCongTyMismatch = true;
            }

            // Kiểm tra CCCD người đại diện nếu có
            if (!string.IsNullOrEmpty(cccd) && !string.IsNullOrEmpty(cicRecord.SoCmndCccd) &&
                cicRecord.SoCmndCccd != cccd)
            {
                result.CccdMismatch = true;
            }

            return result;
        }

        // Cross-check người đại diện với CIC, Khách hàng cá nhân VÀ các Doanh nghiệp khác
        public async Task<CrossCheckNguoiDaiDienResult> CrossCheckNguoiDaiDienWithCICAsync(
            string soCccd, string? nguoiDaiDien, DateOnly? ngaySinh, string? gioiTinh, int? excludeDoanhNghiepId = null)
        {
            var result = new CrossCheckNguoiDaiDienResult();
            string? referenceHoTen = null;
            string? referenceNgaySinh = null;
            string? referenceGioiTinh = null;
            string? referenceSource = null;

            // 1. Tìm Khách hàng cá nhân theo CCCD (ưu tiên kiểm tra trước)
            var caNhan = await GetCustomerByCmndAsync(soCccd);
            if (caNhan != null)
            {
                referenceSource = "Khách hàng cá nhân";
                referenceHoTen = caNhan.HoTen;
                referenceNgaySinh = caNhan.NgaySinh?.ToString("dd/MM/yyyy");
                referenceGioiTinh = caNhan.GioiTinh;

                // Kiểm tra tên người đại diện với Khách hàng cá nhân
                if (!string.IsNullOrEmpty(nguoiDaiDien) && !string.IsNullOrEmpty(caNhan.HoTen))
                {
                    if (!caNhan.HoTen.Equals(nguoiDaiDien, StringComparison.OrdinalIgnoreCase))
                    {
                        result.Warnings.Add(new ValidationWarning
                        {
                            FieldName = "NguoiDaiDienPhapLuat",
                            Message = $"Tên người đại diện không khớp với Khách hàng cá nhân đã có. Hệ thống: {caNhan.HoTen}"
                        });
                    }
                }

                // Kiểm tra ngày sinh với Khách hàng cá nhân
                if (ngaySinh.HasValue && caNhan.NgaySinh.HasValue &&
                    caNhan.NgaySinh.Value != ngaySinh.Value)
                {
                    result.Warnings.Add(new ValidationWarning
                    {
                        FieldName = "NgaySinh",
                        Message = $"Ngày sinh không khớp với Khách hàng cá nhân đã có. Hệ thống: {caNhan.NgaySinh.Value:dd/MM/yyyy}"
                    });
                }

                // Kiểm tra giới tính với Khách hàng cá nhân
                if (!string.IsNullOrEmpty(gioiTinh) && !string.IsNullOrEmpty(caNhan.GioiTinh) &&
                    !caNhan.GioiTinh.Equals(gioiTinh, StringComparison.OrdinalIgnoreCase))
                {
                    result.Warnings.Add(new ValidationWarning
                    {
                        FieldName = "GioiTinh",
                        Message = $"Giới tính không khớp với Khách hàng cá nhân đã có. Hệ thống: {caNhan.GioiTinh}"
                    });
                }
            }

            // 2. Tìm CIC record cá nhân theo CCCD (nếu chưa có warning từ KhachHangCaNhan)
            var cicRecord = await GetCicByCccdAndLoaiAsync(soCccd, "CaNhan");
            if (cicRecord != null)
            {
                // Nếu chưa có reference source từ KhachHangCaNhan
                if (referenceSource == null)
                {
                    referenceSource = "CIC";
                    referenceHoTen = cicRecord.HoTen;
                    // CIC không có trường GioiTinh
                }

                // Kiểm tra tên người đại diện với CIC (nếu chưa có warning về tên từ KhachHangCaNhan)
                if (!result.Warnings.Any(w => w.FieldName == "NguoiDaiDienPhapLuat"))
                {
                    if (!string.IsNullOrEmpty(nguoiDaiDien) && !string.IsNullOrEmpty(cicRecord.HoTen))
                    {
                        if (!cicRecord.HoTen.Equals(nguoiDaiDien, StringComparison.OrdinalIgnoreCase))
                        {
                            result.Warnings.Add(new ValidationWarning
                            {
                                FieldName = "NguoiDaiDienPhapLuat",
                                Message = $"Tên người đại diện không khớp với thông tin trong CIC. CIC: {cicRecord.HoTen}"
                            });
                        }
                    }
                }
                // CIC không có trường GioiTinh nên không kiểm tra giới tính với CIC
            }

            // 3. Kiểm tra với các Doanh nghiệp KHÁC có cùng CCCD người đại diện
            var otherDoanhNghiepsQuery = _context.KhachHangDoanhNghieps
                .Where(k => k.SoCccdNguoiDaiDienPhapLuat == soCccd && k.TrangThaiHoatDong == true);
            
            // Exclude doanh nghiệp hiện tại nếu đang edit
            if (excludeDoanhNghiepId.HasValue)
            {
                otherDoanhNghiepsQuery = otherDoanhNghiepsQuery.Where(k => k.MaKhachHang != excludeDoanhNghiepId.Value);
            }

            var otherDoanhNghieps = await otherDoanhNghiepsQuery.ToListAsync();
            
            if (otherDoanhNghieps.Any())
            {
                var firstOtherDN = otherDoanhNghieps.First();
                
                // Nếu chưa có reference source, lấy từ doanh nghiệp khác
                if (referenceSource == null)
                {
                    referenceSource = $"Doanh nghiệp: {firstOtherDN.TenCongTy}";
                    referenceHoTen = firstOtherDN.NguoiDaiDienPhapLuat;
                    referenceNgaySinh = firstOtherDN.NgaySinh?.ToString("dd/MM/yyyy");
                    referenceGioiTinh = firstOtherDN.GioiTinh;
                }

                // Kiểm tra tên người đại diện với doanh nghiệp khác (nếu chưa có warning)
                if (!result.Warnings.Any(w => w.FieldName == "NguoiDaiDienPhapLuat"))
                {
                    if (!string.IsNullOrEmpty(nguoiDaiDien) && !string.IsNullOrEmpty(firstOtherDN.NguoiDaiDienPhapLuat))
                    {
                        if (!firstOtherDN.NguoiDaiDienPhapLuat.Equals(nguoiDaiDien, StringComparison.OrdinalIgnoreCase))
                        {
                            result.Warnings.Add(new ValidationWarning
                            {
                                FieldName = "NguoiDaiDienPhapLuat",
                                Message = $"Tên người đại diện không khớp với Doanh nghiệp khác đã sử dụng CCCD này ({firstOtherDN.TenCongTy}). Hệ thống: {firstOtherDN.NguoiDaiDienPhapLuat}"
                            });
                        }
                    }
                }

                // Kiểm tra ngày sinh với doanh nghiệp khác (nếu chưa có warning)
                if (!result.Warnings.Any(w => w.FieldName == "NgaySinh"))
                {
                    if (ngaySinh.HasValue && firstOtherDN.NgaySinh.HasValue &&
                        firstOtherDN.NgaySinh.Value != ngaySinh.Value)
                    {
                        result.Warnings.Add(new ValidationWarning
                        {
                            FieldName = "NgaySinh",
                            Message = $"Ngày sinh không khớp với Doanh nghiệp khác đã sử dụng CCCD này ({firstOtherDN.TenCongTy}). Hệ thống: {firstOtherDN.NgaySinh.Value:dd/MM/yyyy}"
                        });
                    }
                }

                // Kiểm tra giới tính với doanh nghiệp khác (nếu chưa có warning)
                if (!result.Warnings.Any(w => w.FieldName == "GioiTinh"))
                {
                    if (!string.IsNullOrEmpty(gioiTinh) && !string.IsNullOrEmpty(firstOtherDN.GioiTinh) &&
                        !firstOtherDN.GioiTinh.Equals(gioiTinh, StringComparison.OrdinalIgnoreCase))
                    {
                        result.Warnings.Add(new ValidationWarning
                        {
                            FieldName = "GioiTinh",
                            Message = $"Giới tính không khớp với Doanh nghiệp khác đã sử dụng CCCD này ({firstOtherDN.TenCongTy}). Hệ thống: {firstOtherDN.GioiTinh}"
                        });
                    }
                }
            }

            // 4. Gán thông tin tham chiếu vào result
            result.ReferenceData = new ReferenceData
            {
                HoTen = referenceHoTen,
                NgaySinh = referenceNgaySinh,
                GioiTinh = referenceGioiTinh,
                Source = referenceSource
            };

            return result;
        }
    }
}
