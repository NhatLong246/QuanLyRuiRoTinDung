using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuanLyRuiRoTinDung.Models.EF;
using QuanLyRuiRoTinDung.Models.Entities;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace QuanLyRuiRoTinDung.Services
{
    public interface ILoanService
    {
        Task<string> GenerateLoanCodeAsync();
        Task<KhoanVay> CreateLoanAsync(KhoanVay khoanVay, int nguoiTao, List<KhoanVayTaiSan>? taiSanDamBaos = null);
        Task<LoaiKhoanVay?> GetLoaiKhoanVayAsync(int maLoaiVay);
        Task<List<LoaiKhoanVay>> GetAllLoaiKhoanVayAsync();
        Task<List<LoaiKhoanVay>> GetLoaiKhoanVayByCustomerTypeAsync(string loaiKhachHang);
        Task<KhachHangCaNhan?> GetKhachHangCaNhanAsync(int maKhachHang);
        Task<KhachHangDoanhNghiep?> GetKhachHangDoanhNghiepAsync(int maKhachHang);
        Task<bool> CheckCustomerHasActiveLoanAsync(int maKhachHang, string loaiKhachHang);
        Task<List<TaiSanDamBao>> GetAllTaiSanDamBaoAsync();
        Task<TaiSanDamBao?> GetTaiSanDamBaoAsync(int maTaiSan);
        Task<List<LoaiTaiSanDamBao>> GetAllLoaiTaiSanAsync();
        Task<TaiSanDamBao> CreateTaiSanAsync(int maLoaiTaiSan, string tenGoi, string? moTaChiTiet, string? chuSoHuu, string? diaChi, decimal? giaTriDinhGia, int nguoiTao);
        Task<int> GetOrCreateLoaiTaiSanAsync(string tenLoai);
        Task<List<HoSoVayFileDinhKem>> SaveLoanFilesAsync(int maKhoanVay, List<(IFormFile File, string LoaiFile)> files, int nguoiTao, string webRootPath);
        Task<List<HoSoVayFileDinhKem>> GetLoanFilesAsync(int maKhoanVay);
        Task<List<KhoanVay>> GetLoansByCustomerAsync(int maKhachHang, string loaiKhachHang, int maNhanVien);
        Task<KhoanVay?> GetLoanByIdAsync(int maKhoanVay);
        Task<KhoanVay> UpdateLoanAsync(KhoanVay khoanVay, int nguoiCapNhat, List<KhoanVayTaiSan>? taiSanDamBaos = null);
        Task<KhoanVay> SubmitLoanForApprovalAsync(int maKhoanVay, string phanLoai, string? ghiChu, int nguoiCapNhat);
        Task<bool> DeleteLoanAsync(int maKhoanVay, int nguoiXoa);
        Task<bool> CancelSubmissionAsync(int maKhoanVay, int nguoiCapNhat);
        
        // Loan Management - Index page
        Task<List<KhoanVay>> GetMyLoansAsync(int maNhanVien, string? trangThai = null, string? searchTerm = null);
        Task<List<KhoanVay>> GetAllLoansAsync(string? trangThai = null, string? searchTerm = null);
        Task<(int Total, int Nhap, int ChoDuyet, int DangXuLy, int DaPheDuyet, int DangVay, int TuChoi, int DaThanhToan)> GetLoanStatsAsync(int? maNhanVien = null);
        
        // Active loans management (approved loans)
        Task<List<KhoanVay>> GetActiveLoansAsync(int maNhanVien, string? trangThai = null, string? searchTerm = null, string? loaiKhachHang = null, bool onlyMyLoans = true);
        
        // Giải ngân khoản vay
        Task<bool> GiaiNganKhoanVayAsync(int maKhoanVay, int nguoiGiaiNgan);
        
        // Lấy lịch sử trả nợ
        Task<List<LichSuTraNo>> GetLichSuTraNoAsync(int maKhoanVay);
        
        // Lấy danh sách khách hàng có khoản vay đang hoạt động (gộp theo khách hàng)
        Task<List<CustomerLoanSummary>> GetCustomersWithActiveLoansAsync(int maNhanVien, string? loaiKhachHang = null, string? searchTerm = null, bool onlyMyLoans = true);
        
        // Lấy khoản vay của một khách hàng
        Task<List<KhoanVay>> GetLoansByCustomerForManageAsync(int maKhachHang, string loaiKhachHang, string? trangThai = null);
        
        // Ghi nhận thanh toán kỳ trả nợ
        Task<(bool Success, string Message)> GhiNhanThanhToanAsync(int maGiaoDich, decimal soTienTra, int nguoiGhiNhan);
        
        // Quản lý khoản quá hạn
        Task<List<OverduePaymentDto>> GetOverduePaymentsAsync(DateOnly thresholdDate);
        Task<(bool Success, string Message)> ProcessOverduePaymentAsync(int maGiaoDich);
        Task<OverduePaymentDto?> GetOverduePaymentInfoAsync(int maGiaoDich);
    }
    
    // DTO cho thông tin khoản quá hạn
    public class OverduePaymentDto
    {
        public int MaGiaoDich { get; set; }
        public string MaGiaoDichCode { get; set; } = "";
        public int MaKhoanVay { get; set; }
        public string MaKhoanVayCode { get; set; } = "";
        public string TenKhachHang { get; set; } = "";
        public string LoaiKhachHang { get; set; } = "";
        public int KyTraNo { get; set; }
        public int TongSoKy { get; set; }
        public DateOnly NgayTraDuKien { get; set; }
        public int SoNgayQuaHan { get; set; }
        public decimal SoTienGocPhaiTra { get; set; }
        public decimal SoTienLaiPhaiTra { get; set; }
        public decimal TongPhaiTra { get; set; }
        public decimal PhiPhat { get; set; }
        public int DiemCicBiTru { get; set; }
        public decimal LaiSuatTangThem { get; set; }
        public string TrangThai { get; set; } = "";
        public string? GhiChu { get; set; }
        public bool DaXuLy { get; set; }
    }
    
    // DTO cho thông tin tóm tắt khách hàng có khoản vay
    public class CustomerLoanSummary
    {
        public int MaKhachHang { get; set; }
        public string? MaKhachHangCode { get; set; }
        public string TenKhachHang { get; set; } = "";
        public string LoaiKhachHang { get; set; } = "";
        public string? AnhDaiDien { get; set; }
        public string? SoDienThoai { get; set; }
        public string? Email { get; set; }
        public int TongKhoanVay { get; set; }
        public int ChoGiaiNgan { get; set; }
        public int DangVay { get; set; }
        public int QuaHan { get; set; }
        public decimal TongSoTienVay { get; set; }
        public decimal TongDuNoConLai { get; set; }
    }

    public class LoanService : ILoanService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LoanService> _logger;

        public LoanService(ApplicationDbContext context, ILogger<LoanService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<string> GenerateLoanCodeAsync()
        {
            // Lấy số thứ tự lớn nhất hiện tại
            var maxCode = await _context.KhoanVays
                .Where(k => k.MaKhoanVayCode.StartsWith("LOAN"))
                .Select(k => k.MaKhoanVayCode)
                .ToListAsync();

            int maxNumber = 0;
            if (maxCode.Any())
            {
                var numbers = maxCode
                    .Where(c => c.Length >= 5 && int.TryParse(c.Substring(4), out _))
                    .Select(c => int.Parse(c.Substring(4)))
                    .ToList();
                maxNumber = numbers.Any() ? numbers.Max() : 0;
            }

            // Tạo mã mới
            maxNumber++;
            return $"LOAN{maxNumber:D4}";
        }

        public async Task<KhoanVay> CreateLoanAsync(KhoanVay khoanVay, int nguoiTao, List<KhoanVayTaiSan>? taiSanDamBaos = null)
        {
            // Tạo mã khoản vay nếu chưa có
            if (string.IsNullOrEmpty(khoanVay.MaKhoanVayCode))
            {
                khoanVay.MaKhoanVayCode = await GenerateLoanCodeAsync();
            }

            khoanVay.NguoiTao = nguoiTao;
            khoanVay.NgayTao = DateTime.Now;
            if (string.IsNullOrEmpty(khoanVay.TrangThaiKhoanVay))
            {
                khoanVay.TrangThaiKhoanVay = "Đang xử lý";
            }
            if (khoanVay.TrangThaiKhoanVay != "Nháp")
            {
                khoanVay.NgayNopHoSo = DateTime.Now;
            }
            khoanVay.SoDuGocConLai = khoanVay.SoTienVay;
            khoanVay.SoKyConLai = khoanVay.KyHanVay;
            khoanVay.SoKyDaTra = 0;
            khoanVay.TongDaThanhToan = 0;
            khoanVay.SoNgayQuaHan = 0;

            // Tính số tiền trả hàng tháng nếu là trả góp đều
            if (khoanVay.HinhThucTraNo == "Trả góp đều" && khoanVay.KyHanVay > 0)
            {
                // Phương pháp "Gốc đều" - phổ biến tại Việt Nam
                // Gốc mỗi kỳ cố định, lãi tính trên dư nợ giảm dần
                // Số tiền trả kỳ đầu (cao nhất) = Gốc/kỳ + Lãi trên toàn bộ dư nợ
                decimal laiSuatThang = khoanVay.LaiSuat / 100 / 12;
                int soKy = khoanVay.KyHanVay;
                decimal soTienVay = khoanVay.SoTienVay;

                decimal gocMoiKy = soTienVay / soKy;
                decimal laiKyDau = soTienVay * laiSuatThang;
                khoanVay.SoTienTraHangThang = gocMoiKy + laiKyDau;
            }

            _context.KhoanVays.Add(khoanVay);
            await _context.SaveChangesAsync();

            // Lưu tài sản đảm bảo nếu có
            if (taiSanDamBaos != null && taiSanDamBaos.Any())
            {
                foreach (var taiSan in taiSanDamBaos)
                {
                    taiSan.MaKhoanVay = khoanVay.MaKhoanVay;
                    taiSan.TrangThai = "Đang thế chấp";
                    if (taiSan.NgayTheChap == null)
                    {
                        taiSan.NgayTheChap = DateOnly.FromDateTime(DateTime.Now);
                    }
                    _context.KhoanVayTaiSans.Add(taiSan);
                }
                await _context.SaveChangesAsync();
            }

            // Tạo lịch sử trạng thái
            var lichSu = new LichSuTrangThaiKhoanVay
            {
                MaKhoanVay = khoanVay.MaKhoanVay,
                TrangThaiCu = null,
                TrangThaiMoi = "Đang xử lý",
                NguoiThayDoi = nguoiTao,
                NgayThayDoi = DateTime.Now,
                NhanXet = "Tạo hồ sơ vay mới"
            };
            _context.LichSuTrangThaiKhoanVays.Add(lichSu);
            await _context.SaveChangesAsync();

            return khoanVay;
        }

        public async Task<LoaiKhoanVay?> GetLoaiKhoanVayAsync(int maLoaiVay)
        {
            return await _context.LoaiKhoanVays
                .FirstOrDefaultAsync(l => l.MaLoaiVay == maLoaiVay && l.TrangThaiHoatDong == true);
        }

        public async Task<List<LoaiKhoanVay>> GetAllLoaiKhoanVayAsync()
        {
            return await _context.LoaiKhoanVays
                .Where(l => l.TrangThaiHoatDong == true)
                .OrderBy(l => l.TenLoaiVay)
                .ToListAsync();
        }

        public async Task<List<LoaiKhoanVay>> GetLoaiKhoanVayByCustomerTypeAsync(string loaiKhachHang)
        {
            if (loaiKhachHang == "CaNhan")
            {
                // Chỉ lấy 4 loại vay cho cá nhân
                var codes = new[] { "CONSUME_CN", "SOCIAL_CN", "STUDENT_CN", "STARTUP_CN" };
                return await _context.LoaiKhoanVays
                    .Where(l => l.TrangThaiHoatDong == true && codes.Contains(l.MaLoaiVayCode))
                    .OrderBy(l => l.TenLoaiVay)
                    .ToListAsync();
            }
            else if (loaiKhachHang == "DoanhNghiep")
            {
                // Lấy 2 loại vay cho doanh nghiệp
                var codes = new[] { "COLLATERAL_DN", "UNSECURED_DN" };
                return await _context.LoaiKhoanVays
                    .Where(l => l.TrangThaiHoatDong == true && codes.Contains(l.MaLoaiVayCode))
                    .OrderBy(l => l.TenLoaiVay)
                    .ToListAsync();
            }
            else
            {
                // Trường hợp khác: trả về danh sách rỗng
                return new List<LoaiKhoanVay>();
            }
        }

        public async Task<KhachHangCaNhan?> GetKhachHangCaNhanAsync(int maKhachHang)
        {
            return await _context.KhachHangCaNhans
                .FirstOrDefaultAsync(k => k.MaKhachHang == maKhachHang && k.TrangThaiHoatDong == true);
        }

        public async Task<KhachHangDoanhNghiep?> GetKhachHangDoanhNghiepAsync(int maKhachHang)
        {
            return await _context.KhachHangDoanhNghieps
                .FirstOrDefaultAsync(k => k.MaKhachHang == maKhachHang && k.TrangThaiHoatDong == true);
        }

        public async Task<bool> CheckCustomerHasActiveLoanAsync(int maKhachHang, string loaiKhachHang)
        {
            return await _context.KhoanVays
                .AnyAsync(k => k.MaKhachHang == maKhachHang
                    && k.LoaiKhachHang == loaiKhachHang
                    && k.TrangThaiKhoanVay != "Đã thanh toán"
                    && k.TrangThaiKhoanVay != "Từ chối");
        }

        public async Task<List<TaiSanDamBao>> GetAllTaiSanDamBaoAsync()
        {
            return await _context.TaiSanDamBaos
                .Where(t => t.TrangThaiSuDung == "Chưa thế chấp" || t.TrangThaiSuDung == null)
                .Include(t => t.MaLoaiTaiSanNavigation)
                .OrderBy(t => t.TenGoi)
                .ToListAsync();
        }

        public async Task<TaiSanDamBao?> GetTaiSanDamBaoAsync(int maTaiSan)
        {
            return await _context.TaiSanDamBaos
                .Include(t => t.MaLoaiTaiSanNavigation)
                .FirstOrDefaultAsync(t => t.MaTaiSan == maTaiSan);
        }

        public async Task<List<LoaiTaiSanDamBao>> GetAllLoaiTaiSanAsync()
        {
            return await _context.LoaiTaiSanDamBaos
                .OrderBy(l => l.TenLoaiTaiSan)
                .ToListAsync();
        }

        public async Task<int> GetOrCreateLoaiTaiSanAsync(string tenLoai)
        {
            var loaiTaiSan = await _context.LoaiTaiSanDamBaos
                .FirstOrDefaultAsync(l => l.TenLoaiTaiSan == tenLoai);
            
            if (loaiTaiSan != null)
            {
                return loaiTaiSan.MaLoaiTaiSan;
            }

            // Tạo loại tài sản mới
            var maCode = tenLoai switch
            {
                "Đất" => "DAT",
                "Xe" => "XE",
                "Khác" => "KHAC",
                _ => "OTHER"
            };

            var newLoaiTaiSan = new LoaiTaiSanDamBao
            {
                TenLoaiTaiSan = tenLoai,
                MaLoaiTaiSanCode = maCode,
                TyLeChoVayToiDa = 70, // Mặc định 70%
                ThoiGianDinhGiaLai = 12, // 12 tháng
                MoTa = $"Loại tài sản: {tenLoai}"
            };

            _context.LoaiTaiSanDamBaos.Add(newLoaiTaiSan);
            await _context.SaveChangesAsync();
            return newLoaiTaiSan.MaLoaiTaiSan;
        }

        /// <summary>
        /// Tạo mới một tài sản đảm bảo cho khoản vay. Mỗi khoản vay sẽ có tài sản riêng biệt.
        /// </summary>
        public async Task<TaiSanDamBao> CreateTaiSanAsync(int maLoaiTaiSan, string tenGoi, string? moTaChiTiet, string? chuSoHuu, string? diaChi, decimal? giaTriDinhGia, int nguoiTao)
        {
            // Tạo mã tài sản mới
            var maxCode = await _context.TaiSanDamBaos
                .Where(t => t.MaTaiSanCode.StartsWith("TS"))
                .Select(t => t.MaTaiSanCode)
                .ToListAsync();

            int maxNumber = 0;
            if (maxCode.Any())
            {
                var numbers = maxCode
                    .Where(c => c.Length >= 3 && int.TryParse(c.Substring(2), out _))
                    .Select(c => int.Parse(c.Substring(2)))
                    .ToList();
                maxNumber = numbers.Any() ? numbers.Max() : 0;
            }

            maxNumber++;
            var maTaiSanCode = $"TS{maxNumber:D4}";

            var newTaiSan = new TaiSanDamBao
            {
                MaTaiSanCode = maTaiSanCode,
                MaLoaiTaiSan = maLoaiTaiSan,
                TenGoi = tenGoi,
                MoTaChiTiet = moTaChiTiet,
                ChuSoHuu = chuSoHuu,
                DiaChi = diaChi,
                GiaTriDinhGia = giaTriDinhGia,
                NgayDinhGia = DateOnly.FromDateTime(DateTime.Now),
                TrangThaiSuDung = "Đang thế chấp",
                NgayTao = DateTime.Now,
                NguoiTao = nguoiTao
            };

            _context.TaiSanDamBaos.Add(newTaiSan);
            await _context.SaveChangesAsync();
            return newTaiSan;
        }

        public async Task<List<HoSoVayFileDinhKem>> SaveLoanFilesAsync(int maKhoanVay, List<(IFormFile File, string LoaiFile)> files, int nguoiTao, string webRootPath)
        {
            var savedFiles = new List<HoSoVayFileDinhKem>();
            
            if (files == null || !files.Any())
                return savedFiles;

            // Lấy mã khoản vay code để tạo thư mục
            var khoanVay = await _context.KhoanVays
                .FirstOrDefaultAsync(k => k.MaKhoanVay == maKhoanVay);
            
            if (khoanVay == null)
                return savedFiles;

            var baseUploadPath = Path.Combine(webRootPath, "uploads", "ho-so-vay", khoanVay.MaKhoanVayCode);
            
            foreach (var (file, loaiFile) in files)
            {
                if (file == null || file.Length == 0)
                    continue;

                // Validate file type
                var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".docx" };
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    _logger.LogWarning($"File type not allowed: {fileExtension} for file {file.FileName}");
                    continue;
                }

                // Validate file size (max 10MB)
                const long maxFileSize = 10 * 1024 * 1024; // 10MB
                if (file.Length > maxFileSize)
                {
                    _logger.LogWarning($"File too large: {file.FileName} ({file.Length} bytes)");
                    continue;
                }

                // Tạo thư mục theo loại file
                var loaiFolder = loaiFile switch
                {
                    "PhapLy" => "phap-ly",
                    "TaiChinh" => "tai-chinh",
                    "TaiSanDamBao" => "tai-san-dam-bao",
                    _ => "other"
                };
                
                var fileFolder = Path.Combine(baseUploadPath, loaiFolder);
                if (!Directory.Exists(fileFolder))
                {
                    Directory.CreateDirectory(fileFolder);
                }

                // Tạo tên file unique (hash)
                var fileName = file.FileName;
                var fileHash = GenerateFileHash(fileName, DateTime.Now);
                var savedFileName = $"{fileHash}{fileExtension}";
                var filePath = Path.Combine(fileFolder, savedFileName);

                // Lưu file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Lưu thông tin vào database
                var hoSoFile = new HoSoVayFileDinhKem
                {
                    MaKhoanVay = maKhoanVay,
                    LoaiFile = loaiFile,
                    TenFile = fileName,
                    TenFileLuu = savedFileName,
                    DuongDan = $"/uploads/ho-so-vay/{khoanVay.MaKhoanVayCode}/{loaiFolder}/{savedFileName}",
                    KichThuoc = file.Length,
                    DinhDang = fileExtension.TrimStart('.'),
                    NgayTao = DateTime.Now,
                    NguoiTao = nguoiTao,
                    TrangThai = true
                };

                _context.HoSoVayFileDinhKems.Add(hoSoFile);
                savedFiles.Add(hoSoFile);
            }

            if (savedFiles.Any())
            {
                await _context.SaveChangesAsync();
            }

            return savedFiles;
        }

        public async Task<List<HoSoVayFileDinhKem>> GetLoanFilesAsync(int maKhoanVay)
        {
            return await _context.HoSoVayFileDinhKems
                .Where(f => f.MaKhoanVay == maKhoanVay && f.TrangThai == true)
                .OrderBy(f => f.LoaiFile)
                .ThenBy(f => f.NgayTao)
                .ToListAsync();
        }

        public async Task<List<KhoanVay>> GetLoansByCustomerAsync(int maKhachHang, string loaiKhachHang, int maNhanVien)
        {
            return await _context.KhoanVays
                .Where(k => k.MaKhachHang == maKhachHang 
                    && k.LoaiKhachHang == loaiKhachHang
                    && k.MaNhanVienTinDung == maNhanVien
                    && (k.TrangThaiKhoanVay == "Nháp" || k.TrangThaiKhoanVay == "Chờ duyệt" || k.TrangThaiKhoanVay == "Đang xử lý"))
                .Include(k => k.MaLoaiVayNavigation)
                .OrderByDescending(k => k.NgayTao)
                .ToListAsync();
        }

        public async Task<KhoanVay?> GetLoanByIdAsync(int maKhoanVay)
        {
            return await _context.KhoanVays
                .Include(k => k.MaLoaiVayNavigation)
                .Include(k => k.KhoanVayTaiSans)
                    .ThenInclude(ts => ts.MaTaiSanNavigation)
                        .ThenInclude(t => t.MaLoaiTaiSanNavigation)
                .FirstOrDefaultAsync(k => k.MaKhoanVay == maKhoanVay);
        }

        public async Task<KhoanVay> UpdateLoanAsync(KhoanVay khoanVay, int nguoiCapNhat, List<KhoanVayTaiSan>? taiSanDamBaos = null)
        {
            var existingLoan = await _context.KhoanVays
                .Include(k => k.KhoanVayTaiSans)
                .FirstOrDefaultAsync(k => k.MaKhoanVay == khoanVay.MaKhoanVay);

            if (existingLoan == null)
            {
                throw new Exception("Khoản vay không tồn tại.");
            }

            // Cập nhật thông tin cơ bản
            existingLoan.SoTienVay = khoanVay.SoTienVay;
            existingLoan.LaiSuat = khoanVay.LaiSuat;
            existingLoan.KyHanVay = khoanVay.KyHanVay;
            existingLoan.HinhThucTraNo = khoanVay.HinhThucTraNo;
            existingLoan.MucDichVay = khoanVay.MucDichVay;
            existingLoan.CoTaiSanDamBao = khoanVay.CoTaiSanDamBao;
            existingLoan.MaLoaiVay = khoanVay.MaLoaiVay;
            existingLoan.GhiChu = khoanVay.GhiChu;
            existingLoan.NguoiCapNhat = nguoiCapNhat;
            existingLoan.NgayCapNhat = DateTime.Now;

            // Tính lại số tiền trả hàng tháng
            if (existingLoan.HinhThucTraNo == "Trả góp đều" && existingLoan.KyHanVay > 0)
            {
                // Phương pháp "Gốc đều" - phổ biến tại Việt Nam
                // Gốc mỗi kỳ cố định, lãi tính trên dư nợ giảm dần
                // Số tiền trả kỳ đầu (cao nhất) = Gốc/kỳ + Lãi trên toàn bộ dư nợ
                decimal laiSuatThang = existingLoan.LaiSuat / 100 / 12;
                int soKy = existingLoan.KyHanVay;
                decimal soTienVay = existingLoan.SoTienVay;

                decimal gocMoiKy = soTienVay / soKy;
                decimal laiKyDau = soTienVay * laiSuatThang;
                existingLoan.SoTienTraHangThang = gocMoiKy + laiKyDau;
            }

            // Xóa tài sản đảm bảo cũ
            var existingTaiSans = _context.KhoanVayTaiSans
                .Where(t => t.MaKhoanVay == existingLoan.MaKhoanVay)
                .ToList();
            
            if (existingTaiSans.Any())
            {
                _context.KhoanVayTaiSans.RemoveRange(existingTaiSans);
            }

            // Thêm tài sản đảm bảo mới (nếu có)
            if (taiSanDamBaos != null && taiSanDamBaos.Any())
            {
                foreach (var taiSan in taiSanDamBaos)
                {
                    var newKhoanVayTaiSan = new KhoanVayTaiSan
                    {
                        MaKhoanVay = existingLoan.MaKhoanVay,
                        MaTaiSan = taiSan.MaTaiSan,
                        GiaTriDinhGiaTaiThoiDiemVay = taiSan.GiaTriDinhGiaTaiThoiDiemVay,
                        TyLeTheChap = taiSan.TyLeTheChap,
                        NgayTheChap = taiSan.NgayTheChap,
                        NgayGiaiChap = taiSan.NgayGiaiChap,
                        TrangThai = taiSan.TrangThai ?? "Đang thế chấp",
                        GhiChu = taiSan.GhiChu,
                        TenTaiSanKhac = taiSan.TenTaiSanKhac,
                        DonVi = taiSan.DonVi,
                        SoLuong = taiSan.SoLuong
                    };
                    _context.KhoanVayTaiSans.Add(newKhoanVayTaiSan);
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving loan update");
                throw;
            }
            return existingLoan;
        }

        public async Task<KhoanVay> SubmitLoanForApprovalAsync(int maKhoanVay, string phanLoai, string? ghiChu, int nguoiCapNhat)
        {
            var loan = await _context.KhoanVays.FirstOrDefaultAsync(k => k.MaKhoanVay == maKhoanVay);
            if (loan == null)
            {
                throw new Exception("Khoản vay không tồn tại.");
            }

            loan.MucDoRuiRo = phanLoai;
            
            // Lưu ghi chú của nhân viên vào trường riêng (GhiChuNhanVien)
            // Không thay đổi GhiChu (ghi chú về hồ sơ thiếu)
            loan.GhiChuNhanVien = !string.IsNullOrEmpty(ghiChu) ? ghiChu : null;
            
            loan.TrangThaiKhoanVay = "Chờ duyệt";
            loan.NguoiCapNhat = nguoiCapNhat;
            loan.NgayCapNhat = DateTime.Now;
            loan.NgayNopHoSo = DateTime.Now;

            await _context.SaveChangesAsync();
            return loan;
        }

        private string GenerateFileHash(string fileName, DateTime timestamp)
        {
            var input = $"{fileName}_{timestamp:yyyyMMddHHmmss}_{Guid.NewGuid()}";
            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                var hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                return hashString.Substring(0, 16); // Lấy 16 ký tự đầu
            }
        }

        public async Task<bool> DeleteLoanAsync(int maKhoanVay, int nguoiXoa)
        {
            try
            {
                _logger.LogInformation("DeleteLoanAsync - Starting deletion for loan {LoanId} by user {UserId}", maKhoanVay, nguoiXoa);
                
                var loan = await _context.KhoanVays
                    .Include(k => k.KhoanVayTaiSans)
                    .Include(k => k.HoSoVayFileDinhKems)
                    .Include(k => k.LichSuTrangThaiKhoanVays)
                    .Include(k => k.LichSuTraNos)
                    .FirstOrDefaultAsync(k => k.MaKhoanVay == maKhoanVay && k.TrangThaiKhoanVay == "Nháp");

                if (loan == null)
                {
                    _logger.LogWarning("DeleteLoanAsync - Loan {LoanId} not found or not in draft status", maKhoanVay);
                    return false;
                }

                _logger.LogInformation("DeleteLoanAsync - Found loan {LoanId} with {TaiSanCount} collateral, {FileCount} files, {LichSuTrangThaiCount} status history, {LichSuTraNoCount} payment history", 
                    maKhoanVay, 
                    loan.KhoanVayTaiSans?.Count ?? 0, 
                    loan.HoSoVayFileDinhKems?.Count ?? 0,
                    loan.LichSuTrangThaiKhoanVays?.Count ?? 0,
                    loan.LichSuTraNos?.Count ?? 0);

                // QUAN TRỌNG: Xóa các bản ghi liên quan theo thứ tự để tránh foreign key constraint violation
                // Thứ tự: Xóa từ bảng con (có FK) trước, rồi mới xóa bảng cha
                
                // 1. Xóa chi tiết đánh giá rủi ro (nếu có) - có FK đến DanhGiaRuiRo
                var danhGiaRuiRos = await _context.DanhGiaRuiRos
                    .Where(d => d.MaKhoanVay == maKhoanVay)
                    .Include(d => d.ChiTietDanhGiaRuiRos)
                    .ToListAsync();
                if (danhGiaRuiRos.Any())
                {
                    foreach (var danhGia in danhGiaRuiRos)
                    {
                        if (danhGia.ChiTietDanhGiaRuiRos != null && danhGia.ChiTietDanhGiaRuiRos.Any())
                        {
                            _context.ChiTietDanhGiaRuiRos.RemoveRange(danhGia.ChiTietDanhGiaRuiRos);
                        }
                    }
                    _context.DanhGiaRuiRos.RemoveRange(danhGiaRuiRos);
                    _logger.LogInformation("DeleteLoanAsync - Removed {Count} risk assessment records", danhGiaRuiRos.Count);
                }
                
                // 2. Xóa theo dõi nợ xấu (nếu có) - có FK đến KhoanVay
                var theoDoiNoXaus = await _context.TheoDoiNoXaus
                    .Where(t => t.MaKhoanVay == maKhoanVay)
                    .ToListAsync();
                if (theoDoiNoXaus.Any())
                {
                    _context.TheoDoiNoXaus.RemoveRange(theoDoiNoXaus);
                    _logger.LogInformation("DeleteLoanAsync - Removed {Count} bad debt tracking records", theoDoiNoXaus.Count);
                }
                
                // 3. Xóa lịch sử trả nợ (nếu có) - có FK đến KhoanVay
                if (loan.LichSuTraNos != null && loan.LichSuTraNos.Any())
                {
                    _context.LichSuTraNos.RemoveRange(loan.LichSuTraNos);
                    _logger.LogInformation("DeleteLoanAsync - Removed {Count} payment history records", loan.LichSuTraNos.Count);
                }

                // 4. Xóa lịch sử trạng thái khoản vay (QUAN TRỌNG: Có FK NOT NULL đến KhoanVay - phải xóa trước khi xóa KhoanVay)
                if (loan.LichSuTrangThaiKhoanVays != null && loan.LichSuTrangThaiKhoanVays.Any())
                {
                    _context.LichSuTrangThaiKhoanVays.RemoveRange(loan.LichSuTrangThaiKhoanVays);
                    _logger.LogInformation("DeleteLoanAsync - Removed {Count} status history records", loan.LichSuTrangThaiKhoanVays.Count);
                }
                
                // 5. Xóa cảnh báo liên quan (nếu có) - FK nullable nên có thể có hoặc không
                var canhBaos = await _context.CanhBaos
                    .Where(c => c.MaKhoanVay == maKhoanVay)
                    .ToListAsync();
                if (canhBaos.Any())
                {
                    _context.CanhBaos.RemoveRange(canhBaos);
                    _logger.LogInformation("DeleteLoanAsync - Removed {Count} warning records", canhBaos.Count);
                }

                // Xóa các file đính kèm
                if (loan.HoSoVayFileDinhKems != null && loan.HoSoVayFileDinhKems.Any())
                {
                    _context.HoSoVayFileDinhKems.RemoveRange(loan.HoSoVayFileDinhKems);
                    _logger.LogInformation("DeleteLoanAsync - Removed {Count} files", loan.HoSoVayFileDinhKems.Count);
                }

                // Xóa các tài sản đảm bảo
                if (loan.KhoanVayTaiSans != null && loan.KhoanVayTaiSans.Any())
                {
                    _context.KhoanVayTaiSans.RemoveRange(loan.KhoanVayTaiSans);
                    _logger.LogInformation("DeleteLoanAsync - Removed {Count} collateral items", loan.KhoanVayTaiSans.Count);
                }

                // Xóa hồ sơ vay (sau khi đã xóa tất cả các bản ghi liên quan)
                _context.KhoanVays.Remove(loan);
                _logger.LogInformation("DeleteLoanAsync - Removed loan {LoanId}", maKhoanVay);

                await _context.SaveChangesAsync();
                _logger.LogInformation("DeleteLoanAsync - Successfully deleted loan {LoanId}", maKhoanVay);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteLoanAsync - Error deleting loan {LoanId}", maKhoanVay);
                throw;
            }
        }

        public async Task<bool> CancelSubmissionAsync(int maKhoanVay, int nguoiCapNhat)
        {
            var loan = await _context.KhoanVays
                .FirstOrDefaultAsync(k => k.MaKhoanVay == maKhoanVay && 
                    (k.TrangThaiKhoanVay == "Chờ duyệt" || k.TrangThaiKhoanVay == "Đang xử lý"));

            if (loan == null)
            {
                return false;
            }

            loan.TrangThaiKhoanVay = "Nháp";
            loan.NgayCapNhat = DateTime.Now;
            loan.NguoiCapNhat = nguoiCapNhat;

            await _context.SaveChangesAsync();
            return true;
        }

        // Loan Management - Get my loans (loans assigned to current employee)
        public async Task<List<KhoanVay>> GetMyLoansAsync(int maNhanVien, string? trangThai = null, string? searchTerm = null)
        {
            var query = _context.KhoanVays
                .Where(k => k.MaNhanVienTinDung == maNhanVien)
                .Include(k => k.MaLoaiVayNavigation)
                .Include(k => k.MaNhanVienTinDungNavigation)
                .AsQueryable();

            // Filter by status
            if (!string.IsNullOrEmpty(trangThai) && trangThai != "all")
            {
                query = query.Where(k => k.TrangThaiKhoanVay == trangThai);
            }

            // Search by loan code, customer info
            if (!string.IsNullOrEmpty(searchTerm))
            {
                var term = searchTerm.ToLower();
                query = query.Where(k => 
                    k.MaKhoanVayCode.ToLower().Contains(term) ||
                    (k.MucDichVay != null && k.MucDichVay.ToLower().Contains(term)));
            }

            return await query
                .OrderByDescending(k => k.NgayTao)
                .ToListAsync();
        }

        // Loan Management - Get all loans
        public async Task<List<KhoanVay>> GetAllLoansAsync(string? trangThai = null, string? searchTerm = null)
        {
            var query = _context.KhoanVays
                .Include(k => k.MaLoaiVayNavigation)
                .Include(k => k.MaNhanVienTinDungNavigation)
                .AsQueryable();

            // Filter by status
            if (!string.IsNullOrEmpty(trangThai) && trangThai != "all")
            {
                query = query.Where(k => k.TrangThaiKhoanVay == trangThai);
            }

            // Search by loan code
            if (!string.IsNullOrEmpty(searchTerm))
            {
                var term = searchTerm.ToLower();
                query = query.Where(k => 
                    k.MaKhoanVayCode.ToLower().Contains(term) ||
                    (k.MucDichVay != null && k.MucDichVay.ToLower().Contains(term)));
            }

            return await query
                .OrderByDescending(k => k.NgayTao)
                .ToListAsync();
        }

        // Get loan statistics
        public async Task<(int Total, int Nhap, int ChoDuyet, int DangXuLy, int DaPheDuyet, int DangVay, int TuChoi, int DaThanhToan)> GetLoanStatsAsync(int? maNhanVien = null)
        {
            var query = _context.KhoanVays.AsQueryable();
            
            if (maNhanVien.HasValue)
            {
                query = query.Where(k => k.MaNhanVienTinDung == maNhanVien.Value);
            }

            var stats = await query
                .GroupBy(k => k.TrangThaiKhoanVay)
                .Select(g => new { TrangThai = g.Key, Count = g.Count() })
                .ToListAsync();

            return (
                Total: stats.Sum(s => s.Count),
                Nhap: stats.FirstOrDefault(s => s.TrangThai == "Nháp")?.Count ?? 0,
                ChoDuyet: stats.FirstOrDefault(s => s.TrangThai == "Chờ duyệt")?.Count ?? 0,
                DangXuLy: stats.FirstOrDefault(s => s.TrangThai == "Đang xử lý")?.Count ?? 0,
                DaPheDuyet: stats.FirstOrDefault(s => s.TrangThai == "Đã phê duyệt")?.Count ?? 0,
                DangVay: stats.FirstOrDefault(s => s.TrangThai == "Đang vay")?.Count ?? 0,
                TuChoi: stats.FirstOrDefault(s => s.TrangThai == "Từ chối")?.Count ?? 0,
                DaThanhToan: stats.FirstOrDefault(s => s.TrangThai == "Đã thanh toán")?.Count ?? 0
            );
        }

        // Get active loans (approved and being repaid)
        public async Task<List<KhoanVay>> GetActiveLoansAsync(int maNhanVien, string? trangThai = null, string? searchTerm = null, string? loaiKhachHang = null, bool onlyMyLoans = true)
        {
            // Các trạng thái của khoản vay đang hoạt động - bao gồm cả "Đã phê duyệt" và "Đã duyệt"
            var activeStatuses = new[] { "Đã phê duyệt", "Đã duyệt", "Đang vay", "Quá hạn", "Đã thanh toán" };

            var query = _context.KhoanVays
                .Include(k => k.MaLoaiVayNavigation)
                .Include(k => k.MaNhanVienTinDungNavigation)
                .Where(k => activeStatuses.Contains(k.TrangThaiKhoanVay));

            // Filter by employee (my loans or all loans)
            if (onlyMyLoans)
            {
                query = query.Where(k => k.MaNhanVienTinDung == maNhanVien);
            }

            // Filter by customer type (CaNhan or DoanhNghiep)
            if (!string.IsNullOrEmpty(loaiKhachHang) && loaiKhachHang != "all")
            {
                query = query.Where(k => k.LoaiKhachHang == loaiKhachHang);
            }

            // Filter by specific status
            if (!string.IsNullOrEmpty(trangThai) && trangThai != "all")
            {
                // Map "Đã duyệt" to also include "Đã phê duyệt" for filtering
                if (trangThai == "Đã duyệt" || trangThai == "Đã phê duyệt")
                {
                    query = query.Where(k => k.TrangThaiKhoanVay == "Đã duyệt" || k.TrangThaiKhoanVay == "Đã phê duyệt");
                }
                else
                {
                    query = query.Where(k => k.TrangThaiKhoanVay == trangThai);
                }
            }

            // Search by loan code or purpose
            if (!string.IsNullOrEmpty(searchTerm))
            {
                var term = searchTerm.ToLower();
                query = query.Where(k => 
                    k.MaKhoanVayCode.ToLower().Contains(term) ||
                    (k.MucDichVay != null && k.MucDichVay.ToLower().Contains(term)));
            }

            return await query
                .OrderByDescending(k => k.NgayCapNhat)
                .ToListAsync();
        }

        // Giải ngân khoản vay
        public async Task<bool> GiaiNganKhoanVayAsync(int maKhoanVay, int nguoiGiaiNgan)
        {
            var khoanVay = await _context.KhoanVays.FindAsync(maKhoanVay);
            if (khoanVay == null || (khoanVay.TrangThaiKhoanVay != "Đã duyệt" && khoanVay.TrangThaiKhoanVay != "Đã phê duyệt"))
            {
                return false;
            }

            var trangThaiCu = khoanVay.TrangThaiKhoanVay;
            var ngayGiaiNgan = DateTime.Now;
            
            // Tính ngày bắt đầu trả nợ: ngày 5 của tháng sau
            var ngayBatDauTra = new DateTime(ngayGiaiNgan.Year, ngayGiaiNgan.Month, 5);
            if (ngayGiaiNgan.Day >= 5)
            {
                ngayBatDauTra = ngayBatDauTra.AddMonths(1);
            }
            
            // Tính ngày đáo hạn: ngày 5 của tháng cuối cùng
            var ngayDaoHan = ngayBatDauTra.AddMonths(khoanVay.KyHanVay - 1);
            
            // Cập nhật trạng thái khoản vay
            khoanVay.TrangThaiKhoanVay = "Đang vay";
            khoanVay.NgayGiaiNgan = ngayGiaiNgan;
            khoanVay.NgayBatDauTra = DateOnly.FromDateTime(ngayBatDauTra);
            khoanVay.NgayDaoHan = DateOnly.FromDateTime(ngayDaoHan);
            khoanVay.SoDuGocConLai = khoanVay.SoTienVay;
            khoanVay.SoKyConLai = khoanVay.KyHanVay;
            khoanVay.SoKyDaTra = 0;
            khoanVay.TongDaThanhToan = 0;
            khoanVay.TyLeHoanThanh = 0;
            khoanVay.NguoiCapNhat = nguoiGiaiNgan;
            khoanVay.NgayCapNhat = DateTime.Now;

            // Tính tổng dư nợ (gốc + lãi)
            decimal tongLai = khoanVay.SoTienVay * (khoanVay.LaiSuat / 100) * (khoanVay.KyHanVay / 12m);
            khoanVay.TongDuNo = khoanVay.SoTienVay + tongLai;
            khoanVay.SoDuLaiConLai = tongLai;

            // Tạo lịch sử trả nợ - ngày 5 hàng tháng
            await TaoLichTraNoAsync(khoanVay, ngayBatDauTra);

            // Tạo lịch sử trạng thái
            var lichSu = new LichSuTrangThaiKhoanVay
            {
                MaKhoanVay = maKhoanVay,
                TrangThaiCu = trangThaiCu,
                TrangThaiMoi = "Đang vay",
                NguoiThayDoi = nguoiGiaiNgan,
                NgayThayDoi = DateTime.Now,
                NhanXet = $"Giải ngân khoản vay thành công. Ngày giải ngân: {ngayGiaiNgan:dd/MM/yyyy}. Ngày bắt đầu trả: {ngayBatDauTra:dd/MM/yyyy} (ngày 5 hàng tháng)."
            };
            _context.LichSuTrangThaiKhoanVays.Add(lichSu);

            await _context.SaveChangesAsync();
            return true;
        }

        // Tạo lịch trả nợ theo ngày 5 hàng tháng - xử lý theo hình thức trả nợ
        private async Task TaoLichTraNoAsync(KhoanVay khoanVay, DateTime ngayBatDauTra)
        {
            decimal soTienVay = khoanVay.SoTienVay;
            decimal laiSuatNam = khoanVay.LaiSuat / 100;
            decimal laiSuatThang = laiSuatNam / 12;
            int soKy = khoanVay.KyHanVay;
            string hinhThucTraNo = khoanVay.HinhThucTraNo ?? "Trả góp đều";
            
            decimal soDuGocConLai = soTienVay;
            
            // Lấy mã giao dịch cuối cùng
            var maxCode = await _context.LichSuTraNos
                .Select(l => l.MaGiaoDichCode)
                .ToListAsync();
            
            int maxNumber = 0;
            if (maxCode.Any())
            {
                var numbers = maxCode
                    .Where(c => c.StartsWith("TRN") && c.Length >= 4 && int.TryParse(c.Substring(3), out _))
                    .Select(c => int.Parse(c.Substring(3)))
                    .ToList();
                maxNumber = numbers.Any() ? numbers.Max() : 0;
            }
            
            // Tạo lịch trả nợ theo hình thức
            if (hinhThucTraNo == "Trả góp đều")
            {
                // Phương pháp "Gốc đều" - phổ biến tại Việt Nam
                // Gốc mỗi kỳ cố định, lãi tính trên dư nợ giảm dần
                decimal gocMoiKy = soTienVay / soKy;
                
                for (int ky = 1; ky <= soKy; ky++)
                {
                    var ngayTraDuKien = ngayBatDauTra.AddMonths(ky - 1);
                    
                    // Lãi = Dư nợ gốc còn lại * lãi suất tháng
                    decimal soTienLaiKy = soDuGocConLai * laiSuatThang;
                    
                    // Gốc cố định mỗi kỳ (kỳ cuối trả hết dư nợ còn lại)
                    decimal soTienGocKy = (ky == soKy) ? soDuGocConLai : gocMoiKy;
                    
                    decimal tongPhaiTra = soTienGocKy + soTienLaiKy;
                    
                    maxNumber++;
                    var lichSuTraNo = new LichSuTraNo
                    {
                        MaGiaoDichCode = $"TRN{maxNumber:D6}",
                        MaKhoanVay = khoanVay.MaKhoanVay,
                        KyTraNo = ky,
                        NgayTraDuKien = DateOnly.FromDateTime(ngayTraDuKien),
                        SoTienGocPhaiTra = Math.Round(soTienGocKy, 0),
                        SoTienLaiPhaiTra = Math.Round(soTienLaiKy, 0),
                        TongPhaiTra = Math.Round(tongPhaiTra, 0),
                        SoDuGocConLai = Math.Round(soDuGocConLai - soTienGocKy, 0),
                        TrangThai = "Chưa thanh toán"
                    };
                    
                    _context.LichSuTraNos.Add(lichSuTraNo);
                    soDuGocConLai -= soTienGocKy;
                }
            }
            else if (hinhThucTraNo == "Trả gốc cuối kỳ")
            {
                // Trả lãi hàng tháng, trả gốc cuối kỳ
                for (int ky = 1; ky <= soKy; ky++)
                {
                    var ngayTraDuKien = ngayBatDauTra.AddMonths(ky - 1);
                    
                    // Lãi cố định mỗi tháng = Gốc * lãi suất tháng
                    decimal soTienLaiKy = soTienVay * laiSuatThang;
                    // Gốc = 0 các kỳ đầu, kỳ cuối = toàn bộ gốc
                    decimal soTienGocKy = (ky == soKy) ? soTienVay : 0;
                    decimal tongPhaiTra = soTienGocKy + soTienLaiKy;
                    
                    maxNumber++;
                    var lichSuTraNo = new LichSuTraNo
                    {
                        MaGiaoDichCode = $"TRN{maxNumber:D6}",
                        MaKhoanVay = khoanVay.MaKhoanVay,
                        KyTraNo = ky,
                        NgayTraDuKien = DateOnly.FromDateTime(ngayTraDuKien),
                        SoTienGocPhaiTra = Math.Round(soTienGocKy, 0),
                        SoTienLaiPhaiTra = Math.Round(soTienLaiKy, 0),
                        TongPhaiTra = Math.Round(tongPhaiTra, 0),
                        SoDuGocConLai = (ky == soKy) ? 0 : soTienVay,
                        TrangThai = "Chưa thanh toán"
                    };
                    
                    _context.LichSuTraNos.Add(lichSuTraNo);
                }
            }
            else if (hinhThucTraNo == "Trả gốc lãi cuối kỳ")
            {
                // Trả toàn bộ gốc + lãi vào kỳ cuối cùng
                decimal tongLai = soTienVay * laiSuatNam * (soKy / 12m);
                
                // Chỉ tạo 1 kỳ thanh toán vào cuối kỳ hạn
                var ngayTraDuKien = ngayBatDauTra.AddMonths(soKy - 1);
                
                maxNumber++;
                var lichSuTraNo = new LichSuTraNo
                {
                    MaGiaoDichCode = $"TRN{maxNumber:D6}",
                    MaKhoanVay = khoanVay.MaKhoanVay,
                    KyTraNo = 1,
                    NgayTraDuKien = DateOnly.FromDateTime(ngayTraDuKien),
                    SoTienGocPhaiTra = soTienVay,
                    SoTienLaiPhaiTra = Math.Round(tongLai, 0),
                    TongPhaiTra = Math.Round(soTienVay + tongLai, 0),
                    SoDuGocConLai = 0,
                    TrangThai = "Chưa thanh toán"
                };
                
                _context.LichSuTraNos.Add(lichSuTraNo);
            }
        }

        // Ghi nhận thanh toán kỳ trả nợ
        public async Task<(bool Success, string Message)> GhiNhanThanhToanAsync(int maGiaoDich, decimal soTienTra, int nguoiGhiNhan)
        {
            var lichSu = await _context.LichSuTraNos
                .Include(l => l.MaKhoanVayNavigation)
                .FirstOrDefaultAsync(l => l.MaGiaoDich == maGiaoDich);
                
            if (lichSu == null)
            {
                return (false, "Không tìm thấy kỳ trả nợ");
            }
            
            if (lichSu.TrangThai == "Đã thanh toán")
            {
                return (false, "Kỳ này đã được thanh toán");
            }
            
            var khoanVay = lichSu.MaKhoanVayNavigation;
            if (khoanVay == null)
            {
                return (false, "Không tìm thấy khoản vay");
            }
            
            // Kiểm tra số tiền trả
            if (soTienTra < lichSu.TongPhaiTra)
            {
                return (false, $"Số tiền trả ({soTienTra:N0}) phải lớn hơn hoặc bằng tổng phải trả ({lichSu.TongPhaiTra:N0})");
            }
            
            var ngayTraThucTe = DateTime.Now;
            var ngayTraDuKien = lichSu.NgayTraDuKien.ToDateTime(TimeOnly.MinValue);
            
            // Tính số ngày trả chậm
            int soNgayTraCham = 0;
            decimal phiTraCham = 0;
            if (ngayTraThucTe.Date > ngayTraDuKien.Date)
            {
                soNgayTraCham = (ngayTraThucTe.Date - ngayTraDuKien.Date).Days;
                // Phí trả chậm: 0.05% mỗi ngày trên số tiền chậm
                phiTraCham = lichSu.TongPhaiTra * 0.0005m * soNgayTraCham;
            }
            
            // Cập nhật lịch sử trả nợ
            lichSu.NgayTraThucTe = ngayTraThucTe;
            lichSu.SoTienGocDaTra = lichSu.SoTienGocPhaiTra;
            lichSu.SoTienLaiDaTra = lichSu.SoTienLaiPhaiTra;
            lichSu.SoTienPhiTraCham = Math.Round(phiTraCham, 0);
            lichSu.TongDaTra = lichSu.SoTienGocPhaiTra + lichSu.SoTienLaiPhaiTra + phiTraCham;
            lichSu.SoNgayTraCham = soNgayTraCham;
            lichSu.TrangThai = soNgayTraCham > 0 ? "Trả chậm" : "Đã thanh toán";
            
            // Cập nhật khoản vay
            khoanVay.SoDuGocConLai = (khoanVay.SoDuGocConLai ?? khoanVay.SoTienVay) - lichSu.SoTienGocPhaiTra;
            khoanVay.TongDaThanhToan = (khoanVay.TongDaThanhToan ?? 0) + lichSu.TongDaTra;
            khoanVay.SoKyDaTra = (khoanVay.SoKyDaTra ?? 0) + 1;
            khoanVay.SoKyConLai = (khoanVay.SoKyConLai ?? khoanVay.KyHanVay) - 1;
            khoanVay.NguoiCapNhat = nguoiGhiNhan;
            khoanVay.NgayCapNhat = DateTime.Now;
            
            // Tính tỷ lệ hoàn thành
            var tongKy = await _context.LichSuTraNos.CountAsync(l => l.MaKhoanVay == khoanVay.MaKhoanVay);
            var kyDaTra = await _context.LichSuTraNos.CountAsync(l => l.MaKhoanVay == khoanVay.MaKhoanVay && 
                (l.TrangThai == "Đã thanh toán" || l.TrangThai == "Trả chậm"));
            khoanVay.TyLeHoanThanh = tongKy > 0 ? Math.Round((decimal)kyDaTra / tongKy * 100, 2) : 0;
            
            // Nếu đã trả hết, cập nhật trạng thái khoản vay
            if (khoanVay.SoDuGocConLai <= 0)
            {
                khoanVay.TrangThaiKhoanVay = "Đã thanh toán";
                khoanVay.SoDuGocConLai = 0;
                khoanVay.SoDuLaiConLai = 0;
                khoanVay.TyLeHoanThanh = 100;
                
                // Ghi lịch sử trạng thái
                var lichSuTrangThai = new LichSuTrangThaiKhoanVay
                {
                    MaKhoanVay = khoanVay.MaKhoanVay,
                    TrangThaiCu = "Đang vay",
                    TrangThaiMoi = "Đã thanh toán",
                    NguoiThayDoi = nguoiGhiNhan,
                    NgayThayDoi = DateTime.Now,
                    NhanXet = "Khoản vay đã được thanh toán đầy đủ"
                };
                _context.LichSuTrangThaiKhoanVays.Add(lichSuTrangThai);
            }
            
            await _context.SaveChangesAsync();
            
            return (true, soNgayTraCham > 0 
                ? $"Ghi nhận thanh toán thành công. Trả chậm {soNgayTraCham} ngày, phí phạt: {phiTraCham:N0} VNĐ" 
                : "Ghi nhận thanh toán thành công");
        }

        // Lấy lịch sử trả nợ
        public async Task<List<LichSuTraNo>> GetLichSuTraNoAsync(int maKhoanVay)
        {
            return await _context.LichSuTraNos
                .Where(l => l.MaKhoanVay == maKhoanVay)
                .OrderBy(l => l.KyTraNo)
                .ToListAsync();
        }
        
        // Lấy danh sách khách hàng có khoản vay đang hoạt động (gộp theo khách hàng)
        public async Task<List<CustomerLoanSummary>> GetCustomersWithActiveLoansAsync(int maNhanVien, string? loaiKhachHang = null, string? searchTerm = null, bool onlyMyLoans = true)
        {
            // Các trạng thái của khoản vay đang hoạt động
            var activeStatuses = new[] { "Đã phê duyệt", "Đã duyệt", "Đang vay", "Quá hạn", "Đã thanh toán" };
            
            var query = _context.KhoanVays
                .Where(k => activeStatuses.Contains(k.TrangThaiKhoanVay));
            
            // Filter theo nhân viên
            if (onlyMyLoans)
            {
                query = query.Where(k => k.MaNhanVienTinDung == maNhanVien);
            }
            
            // Filter theo loại khách hàng
            if (!string.IsNullOrEmpty(loaiKhachHang) && loaiKhachHang != "all")
            {
                query = query.Where(k => k.LoaiKhachHang == loaiKhachHang);
            }
            
            // Gộp theo khách hàng
            var loansByCustomer = await query
                .GroupBy(k => new { k.MaKhachHang, k.LoaiKhachHang })
                .Select(g => new 
                {
                    MaKhachHang = g.Key.MaKhachHang,
                    LoaiKhachHang = g.Key.LoaiKhachHang,
                    TongKhoanVay = g.Count(),
                    ChoGiaiNgan = g.Count(k => k.TrangThaiKhoanVay == "Đã phê duyệt" || k.TrangThaiKhoanVay == "Đã duyệt"),
                    DangVay = g.Count(k => k.TrangThaiKhoanVay == "Đang vay"),
                    QuaHan = g.Count(k => k.TrangThaiKhoanVay == "Quá hạn"),
                    TongSoTienVay = g.Sum(k => k.SoTienVay),
                    TongDuNoConLai = g.Sum(k => k.SoDuGocConLai ?? 0)
                })
                .ToListAsync();
            
            var result = new List<CustomerLoanSummary>();
            
            foreach (var item in loansByCustomer)
            {
                var summary = new CustomerLoanSummary
                {
                    MaKhachHang = item.MaKhachHang,
                    LoaiKhachHang = item.LoaiKhachHang,
                    TongKhoanVay = item.TongKhoanVay,
                    ChoGiaiNgan = item.ChoGiaiNgan,
                    DangVay = item.DangVay,
                    QuaHan = item.QuaHan,
                    TongSoTienVay = item.TongSoTienVay,
                    TongDuNoConLai = item.TongDuNoConLai
                };
                
                // Lấy thông tin khách hàng
                if (item.LoaiKhachHang == "CaNhan")
                {
                    var kh = await _context.KhachHangCaNhans.FindAsync(item.MaKhachHang);
                    if (kh != null)
                    {
                        summary.TenKhachHang = kh.HoTen;
                        summary.MaKhachHangCode = kh.MaKhachHangCode;
                        summary.AnhDaiDien = kh.AnhDaiDien;
                        summary.SoDienThoai = kh.SoDienThoai;
                        summary.Email = kh.Email;
                    }
                }
                else
                {
                    var kh = await _context.KhachHangDoanhNghieps.FindAsync(item.MaKhachHang);
                    if (kh != null)
                    {
                        summary.TenKhachHang = kh.TenCongTy;
                        summary.MaKhachHangCode = kh.MaKhachHangCode;
                        summary.AnhDaiDien = kh.AnhNguoiDaiDien;
                        summary.SoDienThoai = kh.SoDienThoai;
                        summary.Email = kh.Email;
                    }
                }
                
                result.Add(summary);
            }
            
            // Tìm kiếm theo tên/mã khách hàng
            if (!string.IsNullOrEmpty(searchTerm))
            {
                var term = searchTerm.ToLower();
                result = result.Where(c => 
                    c.TenKhachHang.ToLower().Contains(term) ||
                    (c.MaKhachHangCode != null && c.MaKhachHangCode.ToLower().Contains(term)) ||
                    (c.SoDienThoai != null && c.SoDienThoai.Contains(term))
                ).ToList();
            }
            
            // Sắp xếp: ưu tiên khách hàng có khoản vay quá hạn, chờ giải ngân
            return result
                .OrderByDescending(c => c.QuaHan)
                .ThenByDescending(c => c.ChoGiaiNgan)
                .ThenByDescending(c => c.TongKhoanVay)
                .ToList();
        }
        
        // Lấy khoản vay của một khách hàng cụ thể
        public async Task<List<KhoanVay>> GetLoansByCustomerForManageAsync(int maKhachHang, string loaiKhachHang, string? trangThai = null)
        {
            var activeStatuses = new[] { "Đã phê duyệt", "Đã duyệt", "Đang vay", "Quá hạn", "Đã thanh toán" };
            
            var query = _context.KhoanVays
                .Include(k => k.MaLoaiVayNavigation)
                .Include(k => k.MaNhanVienTinDungNavigation)
                .Where(k => k.MaKhachHang == maKhachHang && 
                            k.LoaiKhachHang == loaiKhachHang &&
                            activeStatuses.Contains(k.TrangThaiKhoanVay));
            
            // Filter theo trạng thái cụ thể
            if (!string.IsNullOrEmpty(trangThai) && trangThai != "all")
            {
                if (trangThai == "Đã duyệt" || trangThai == "Đã phê duyệt" || trangThai == "ChoGiaiNgan")
                {
                    query = query.Where(k => k.TrangThaiKhoanVay == "Đã duyệt" || k.TrangThaiKhoanVay == "Đã phê duyệt");
                }
                else
                {
                    query = query.Where(k => k.TrangThaiKhoanVay == trangThai);
                }
            }
            
            return await query
                .OrderByDescending(k => k.NgayTao)
                .ToListAsync();
        }

        // Lấy danh sách các khoản quá hạn
        public async Task<List<OverduePaymentDto>> GetOverduePaymentsAsync(DateOnly thresholdDate)
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            
            var overduePayments = await _context.LichSuTraNos
                .Include(l => l.MaKhoanVayNavigation)
                .Where(l => l.TrangThai == "Chưa thanh toán" && l.NgayTraDuKien < thresholdDate)
                .ToListAsync();

            var result = new List<OverduePaymentDto>();

            foreach (var payment in overduePayments)
            {
                var khoanVay = payment.MaKhoanVayNavigation;
                if (khoanVay == null) continue;

                string tenKhachHang = "";
                if (khoanVay.LoaiKhachHang == "CaNhan")
                {
                    var kh = await _context.KhachHangCaNhans.FindAsync(khoanVay.MaKhachHang);
                    tenKhachHang = kh?.HoTen ?? "";
                }
                else
                {
                    var kh = await _context.KhachHangDoanhNghieps.FindAsync(khoanVay.MaKhachHang);
                    tenKhachHang = kh?.TenCongTy ?? "";
                }

                int soNgayQuaHan = today.DayNumber - payment.NgayTraDuKien.DayNumber;
                int tongSoKy = khoanVay.KyHanVay;
                
                // Tính phí phạt và điểm CIC trừ
                decimal phiPhat = CalculateLatePaymentFee(payment.TongPhaiTra, soNgayQuaHan);
                int diemCicTru = CalculateCicPenalty(soNgayQuaHan);
                decimal laiSuatTangThem = CalculateInterestPenalty(soNgayQuaHan);
                
                bool daXuLy = payment.GhiChu?.Contains("[OVERDUE_PROCESSED]") ?? false;

                result.Add(new OverduePaymentDto
                {
                    MaGiaoDich = payment.MaGiaoDich,
                    MaGiaoDichCode = payment.MaGiaoDichCode,
                    MaKhoanVay = khoanVay.MaKhoanVay,
                    MaKhoanVayCode = khoanVay.MaKhoanVayCode,
                    TenKhachHang = tenKhachHang,
                    LoaiKhachHang = khoanVay.LoaiKhachHang,
                    KyTraNo = payment.KyTraNo,
                    TongSoKy = tongSoKy,
                    NgayTraDuKien = payment.NgayTraDuKien,
                    SoNgayQuaHan = soNgayQuaHan,
                    SoTienGocPhaiTra = payment.SoTienGocPhaiTra,
                    SoTienLaiPhaiTra = payment.SoTienLaiPhaiTra,
                    TongPhaiTra = payment.TongPhaiTra,
                    PhiPhat = phiPhat,
                    DiemCicBiTru = diemCicTru,
                    LaiSuatTangThem = laiSuatTangThem,
                    TrangThai = soNgayQuaHan > 90 ? "Nợ xấu" : "Quá hạn",
                    GhiChu = payment.GhiChu,
                    DaXuLy = daXuLy
                });
            }

            return result.OrderByDescending(p => p.SoNgayQuaHan).ToList();
        }

        // Xử lý khoản quá hạn thủ công
        public async Task<(bool Success, string Message)> ProcessOverduePaymentAsync(int maGiaoDich)
        {
            try
            {
                var payment = await _context.LichSuTraNos
                    .Include(l => l.MaKhoanVayNavigation)
                    .FirstOrDefaultAsync(l => l.MaGiaoDich == maGiaoDich);

                if (payment == null)
                    return (false, "Không tìm thấy khoản thanh toán");

                var today = DateOnly.FromDateTime(DateTime.Now);
                int soNgayQuaHan = today.DayNumber - payment.NgayTraDuKien.DayNumber;

                if (soNgayQuaHan <= 3)
                    return (false, "Khoản thanh toán chưa quá hạn đủ 3 ngày");

                if (payment.GhiChu?.Contains("[OVERDUE_PROCESSED]") ?? false)
                    return (false, "Khoản thanh toán đã được xử lý trước đó");

                var khoanVay = payment.MaKhoanVayNavigation;
                if (khoanVay == null)
                    return (false, "Không tìm thấy khoản vay");

                // 1. Tính phí phạt
                decimal phiPhat = CalculateLatePaymentFee(payment.TongPhaiTra, soNgayQuaHan);
                payment.SoTienPhiTraCham = phiPhat;
                payment.SoNgayTraCham = soNgayQuaHan;
                payment.TrangThai = soNgayQuaHan > 90 ? "Nợ xấu" : "Quá hạn";

                // 2. Cập nhật CIC
                int diemTru = CalculateCicPenalty(soNgayQuaHan);
                await UpdateCicForOverdueAsync(khoanVay, soNgayQuaHan, diemTru);

                // 3. Tăng lãi suất các kỳ sau
                decimal laiSuatTang = CalculateInterestPenalty(soNgayQuaHan);
                int kyDuocTang = await IncreaseFutureInterestAsync(khoanVay.MaKhoanVay, payment.KyTraNo, laiSuatTang);

                // 4. Ghi chú
                payment.GhiChu = $"{payment.GhiChu ?? ""} [OVERDUE_PROCESSED:{DateTime.Now:yyyy-MM-dd}] Quá hạn {soNgayQuaHan} ngày. Phí: {phiPhat:N0}đ. CIC trừ: {diemTru} điểm. Lãi tăng: +{laiSuatTang}%";

                // 5. Cập nhật trạng thái khoản vay
                khoanVay.TrangThaiKhoanVay = soNgayQuaHan > 90 ? "Nợ xấu" : "Quá hạn";
                khoanVay.MucDoRuiRo = soNgayQuaHan > 90 ? "Rất cao" : (soNgayQuaHan > 30 ? "Cao" : "Trung bình");

                await _context.SaveChangesAsync();

                return (true, $"Đã xử lý thành công. Phí phạt: {phiPhat:N0}đ, CIC trừ: {diemTru} điểm, Lãi tăng: +{laiSuatTang}% cho {kyDuocTang} kỳ sau.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xử lý khoản quá hạn {MaGiaoDich}", maGiaoDich);
                return (false, $"Lỗi: {ex.Message}");
            }
        }

        // Lấy thông tin chi tiết khoản quá hạn
        public async Task<OverduePaymentDto?> GetOverduePaymentInfoAsync(int maGiaoDich)
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            
            var payment = await _context.LichSuTraNos
                .Include(l => l.MaKhoanVayNavigation)
                .FirstOrDefaultAsync(l => l.MaGiaoDich == maGiaoDich);

            if (payment == null) return null;

            var khoanVay = payment.MaKhoanVayNavigation;
            if (khoanVay == null) return null;

            string tenKhachHang = "";
            if (khoanVay.LoaiKhachHang == "CaNhan")
            {
                var kh = await _context.KhachHangCaNhans.FindAsync(khoanVay.MaKhachHang);
                tenKhachHang = kh?.HoTen ?? "";
            }
            else
            {
                var kh = await _context.KhachHangDoanhNghieps.FindAsync(khoanVay.MaKhachHang);
                tenKhachHang = kh?.TenCongTy ?? "";
            }

            int soNgayQuaHan = today.DayNumber - payment.NgayTraDuKien.DayNumber;
            
            return new OverduePaymentDto
            {
                MaGiaoDich = payment.MaGiaoDich,
                MaGiaoDichCode = payment.MaGiaoDichCode,
                MaKhoanVay = khoanVay.MaKhoanVay,
                MaKhoanVayCode = khoanVay.MaKhoanVayCode,
                TenKhachHang = tenKhachHang,
                LoaiKhachHang = khoanVay.LoaiKhachHang,
                KyTraNo = payment.KyTraNo,
                TongSoKy = khoanVay.KyHanVay,
                NgayTraDuKien = payment.NgayTraDuKien,
                SoNgayQuaHan = soNgayQuaHan,
                SoTienGocPhaiTra = payment.SoTienGocPhaiTra,
                SoTienLaiPhaiTra = payment.SoTienLaiPhaiTra,
                TongPhaiTra = payment.TongPhaiTra,
                PhiPhat = CalculateLatePaymentFee(payment.TongPhaiTra, soNgayQuaHan),
                DiemCicBiTru = CalculateCicPenalty(soNgayQuaHan),
                LaiSuatTangThem = CalculateInterestPenalty(soNgayQuaHan),
                TrangThai = soNgayQuaHan > 90 ? "Nợ xấu" : "Quá hạn",
                GhiChu = payment.GhiChu,
                DaXuLy = payment.GhiChu?.Contains("[OVERDUE_PROCESSED]") ?? false
            };
        }

        // Helper: Tính phí trả chậm
        private decimal CalculateLatePaymentFee(decimal tongPhaiTra, int soNgayQuaHan)
        {
            // 0.05% mỗi ngày
            return Math.Round(tongPhaiTra * 0.0005m * soNgayQuaHan, 0);
        }

        // Helper: Tính điểm CIC bị trừ
        private int CalculateCicPenalty(int soNgayQuaHan)
        {
            if (soNgayQuaHan > 90) return 200;
            if (soNgayQuaHan > 60) return 150;
            if (soNgayQuaHan > 30) return 100;
            return 50;
        }

        // Helper: Tính lãi suất tăng thêm
        private decimal CalculateInterestPenalty(int soNgayQuaHan)
        {
            if (soNgayQuaHan > 90) return 2.0m;
            if (soNgayQuaHan > 60) return 1.5m;
            if (soNgayQuaHan > 30) return 1.0m;
            return 0.5m;
        }

        // Helper: Cập nhật điểm CIC
        private async Task UpdateCicForOverdueAsync(KhoanVay khoanVay, int soNgayQuaHan, int diemTru)
        {
            ThongTinCic? cicInfo = null;

            if (khoanVay.LoaiKhachHang == "CaNhan")
            {
                var kh = await _context.KhachHangCaNhans.FindAsync(khoanVay.MaKhachHang);
                if (kh != null)
                {
                    cicInfo = await _context.ThongTinCics
                        .FirstOrDefaultAsync(c => c.SoCmndCccd == kh.SoCmnd && c.LoaiKhachHang == "Cá nhân");
                }
            }
            else
            {
                var kh = await _context.KhachHangDoanhNghieps.FindAsync(khoanVay.MaKhachHang);
                if (kh != null)
                {
                    cicInfo = await _context.ThongTinCics
                        .FirstOrDefaultAsync(c => c.MaSoThue == kh.MaSoThue && c.LoaiKhachHang == "Doanh nghiệp");
                }
            }

            if (cicInfo != null)
            {
                cicInfo.DiemTinDungCic = Math.Max(0, (cicInfo.DiemTinDungCic ?? 500) - diemTru);
                cicInfo.XepHangTinDungCic = GetCicRating(cicInfo.DiemTinDungCic ?? 0);
                cicInfo.SoLanQuaHanCic++;
                cicInfo.SoNgayQuaHanToiDaCic = Math.Max(cicInfo.SoNgayQuaHanToiDaCic, soNgayQuaHan);
                cicInfo.NgayQuaHanLanCuoiCic = DateOnly.FromDateTime(DateTime.Now);
                cicInfo.SoKhoanVayQuaHanCic++;
                
                if (soNgayQuaHan > 90)
                {
                    cicInfo.SoKhoanVayNoXauCic++;
                    cicInfo.SoLanNoXauCic++;
                    cicInfo.NgayNoXauLanCuoiCic = DateOnly.FromDateTime(DateTime.Now);
                    cicInfo.MucDoRuiRo = "Rất cao";
                    cicInfo.KhuyenNghiChoVay = "Từ chối";
                }
                else
                {
                    cicInfo.MucDoRuiRo = soNgayQuaHan > 30 ? "Cao" : "Trung bình";
                    cicInfo.KhuyenNghiChoVay = "Cần xem xét";
                }

                cicInfo.DanhGiaTongQuat = $"Quá hạn {cicInfo.SoLanQuaHanCic} lần. Điểm: {cicInfo.DiemTinDungCic}. Cập nhật: {DateTime.Now:dd/MM/yyyy}";
                cicInfo.NgayCapNhat = DateTime.Now;

                // Ghi lịch sử CIC
                _context.LichSuTraCuuCics.Add(new LichSuTraCuuCic
                {
                    MaCic = cicInfo.MaCic,
                    LoaiKhachHang = cicInfo.LoaiKhachHang,
                    MaKhachHang = cicInfo.MaKhachHang,
                    SoCmndCccd = cicInfo.SoCmndCccd,
                    MaSoThue = cicInfo.MaSoThue,
                    NgayTraCuu = DateTime.Now,
                    NguoiTraCuu = 1,
                    KetQua = "Cập nhật tự động",
                    ThongTinTraVe = $"Trừ {diemTru} điểm do quá hạn {soNgayQuaHan} ngày",
                    DiemTinDung = cicInfo.DiemTinDungCic,
                    XepHangTinDung = cicInfo.XepHangTinDungCic,
                    GhiChu = $"[PENALTY] Quá hạn {soNgayQuaHan} ngày"
                });
            }
        }

        // Helper: Tăng lãi suất các kỳ sau
        private async Task<int> IncreaseFutureInterestAsync(int maKhoanVay, int currentKy, decimal laiSuatTang)
        {
            var khoanVay = await _context.KhoanVays.FindAsync(maKhoanVay);
            if (khoanVay == null) return 0;

            var futurePayments = await _context.LichSuTraNos
                .Where(l => l.MaKhoanVay == maKhoanVay && l.KyTraNo > currentKy && l.TrangThai == "Chưa thanh toán")
                .ToListAsync();

            foreach (var payment in futurePayments)
            {
                decimal laiSuatMoi = (khoanVay.LaiSuat / 100 / 12) + (laiSuatTang / 100);
                decimal duNoGoc = payment.SoDuGocConLai ?? (khoanVay.SoDuGocConLai ?? khoanVay.SoTienVay);
                decimal laiMoi = duNoGoc * laiSuatMoi;

                if (laiMoi > payment.SoTienLaiPhaiTra)
                {
                    decimal chenhLech = laiMoi - payment.SoTienLaiPhaiTra;
                    payment.SoTienLaiPhaiTra = laiMoi;
                    payment.TongPhaiTra = payment.SoTienGocPhaiTra + laiMoi;
                    payment.GhiChu = $"{payment.GhiChu ?? ""} [INTEREST+{laiSuatTang}%:{DateTime.Now:yyyy-MM-dd}] +{chenhLech:N0}đ";
                }
            }

            return futurePayments.Count;
        }

        // Helper: Xếp hạng CIC
        private string GetCicRating(int score)
        {
            return score switch
            {
                >= 800 => "AAA",
                >= 700 => "AA",
                >= 600 => "A",
                >= 500 => "BBB",
                >= 400 => "BB",
                >= 300 => "B",
                >= 200 => "CCC",
                >= 100 => "CC",
                _ => "C"
            };
        }
    }
}
