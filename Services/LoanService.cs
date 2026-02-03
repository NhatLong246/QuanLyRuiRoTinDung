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
        Task<TaiSanDamBao> CreateOrGetTaiSanAsync(string loaiTaiSan, string? tenTaiSanKhac = null, string? donVi = null, decimal? giaTriDinhGia = null);
        Task<int> GetOrCreateLoaiTaiSanAsync(string tenLoai);
        Task<List<HoSoVayFileDinhKem>> SaveLoanFilesAsync(int maKhoanVay, List<(IFormFile File, string LoaiFile)> files, int nguoiTao, string webRootPath);
        Task<List<HoSoVayFileDinhKem>> GetLoanFilesAsync(int maKhoanVay);
        Task<List<KhoanVay>> GetLoansByCustomerAsync(int maKhachHang, string loaiKhachHang, int maNhanVien);
        Task<KhoanVay?> GetLoanByIdAsync(int maKhoanVay);
        Task<KhoanVay> UpdateLoanAsync(KhoanVay khoanVay, int nguoiCapNhat, List<KhoanVayTaiSan>? taiSanDamBaos = null);
        Task<KhoanVay> SubmitLoanForApprovalAsync(int maKhoanVay, string phanLoai, string? ghiChu, int nguoiCapNhat);
        Task<bool> DeleteLoanAsync(int maKhoanVay, int nguoiXoa);
        Task<bool> CancelSubmissionAsync(int maKhoanVay, int nguoiCapNhat);
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
                // Công thức tính trả góp đều: PMT = PV * (r(1+r)^n) / ((1+r)^n - 1)
                // PV = Số tiền vay, r = Lãi suất tháng, n = Số kỳ
                decimal laiSuatThang = khoanVay.LaiSuat / 100 / 12;
                int soKy = khoanVay.KyHanVay;
                decimal soTienVay = khoanVay.SoTienVay;

                if (laiSuatThang > 0)
                {
                    decimal tuSo = laiSuatThang * (decimal)Math.Pow((double)(1 + laiSuatThang), soKy);
                    decimal mauSo = (decimal)Math.Pow((double)(1 + laiSuatThang), soKy) - 1;
                    khoanVay.SoTienTraHangThang = soTienVay * (tuSo / mauSo);
                }
                else
                {
                    khoanVay.SoTienTraHangThang = soTienVay / soKy;
                }
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

        public async Task<TaiSanDamBao> CreateOrGetTaiSanAsync(string loaiTaiSan, string? tenTaiSanKhac = null, string? donVi = null, decimal? giaTriDinhGia = null)
        {
            // Tìm loại tài sản
            string tenLoai = loaiTaiSan switch
            {
                "Dat" => "Đất",
                "Xe" => "Xe",
                "Khac" => "Khác",
                _ => "Khác"
            };

            var maLoaiTaiSan = await GetOrCreateLoaiTaiSanAsync(tenLoai);

            // Tìm tài sản hiện có
            TaiSanDamBao? existingTaiSan = null;

            if (loaiTaiSan == "Khac" && !string.IsNullOrEmpty(tenTaiSanKhac))
            {
                // Tìm tài sản "Khác" có tên tương tự
                existingTaiSan = await _context.TaiSanDamBaos
                    .FirstOrDefaultAsync(t => t.MaLoaiTaiSan == maLoaiTaiSan && 
                                             t.TenGoi.Contains(tenTaiSanKhac));
            }
            else
            {
                // Tìm tài sản "Đất" hoặc "Xe" chung
                existingTaiSan = await _context.TaiSanDamBaos
                    .FirstOrDefaultAsync(t => t.MaLoaiTaiSan == maLoaiTaiSan && 
                                             (t.TenGoi.Contains(tenLoai) || t.TenGoi == tenLoai));
            }

            if (existingTaiSan != null)
            {
                return existingTaiSan;
            }

            // Tạo tài sản mới
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

            var tenGoi = loaiTaiSan == "Khac" && !string.IsNullOrEmpty(tenTaiSanKhac)
                ? tenTaiSanKhac
                : tenLoai;

            var newTaiSan = new TaiSanDamBao
            {
                MaTaiSanCode = maTaiSanCode,
                MaLoaiTaiSan = maLoaiTaiSan,
                TenGoi = tenGoi,
                MoTaChiTiet = loaiTaiSan == "Khac" && !string.IsNullOrEmpty(tenTaiSanKhac)
                    ? $"Tài sản khác: {tenTaiSanKhac}, Đơn vị: {donVi}"
                    : $"Tài sản {tenLoai}",
                GiaTriDinhGia = giaTriDinhGia,
                NgayDinhGia = DateOnly.FromDateTime(DateTime.Now),
                DonViDinhGia = donVi,
                TrangThaiSuDung = "Chưa thế chấp",
                NgayTao = DateTime.Now
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
                decimal laiSuatThang = existingLoan.LaiSuat / 100 / 12;
                int soKy = existingLoan.KyHanVay;
                decimal soTienVay = existingLoan.SoTienVay;

                if (laiSuatThang > 0)
                {
                    decimal tuSo = laiSuatThang * (decimal)Math.Pow((double)(1 + laiSuatThang), soKy);
                    decimal mauSo = (decimal)Math.Pow((double)(1 + laiSuatThang), soKy) - 1;
                    existingLoan.SoTienTraHangThang = soTienVay * (tuSo / mauSo);
                }
                else
                {
                    existingLoan.SoTienTraHangThang = soTienVay / soKy;
                }
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
    }
}
