using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyRuiRoTinDung.Models.EF;
using QuanLyRuiRoTinDung.Models.Entities;
using System.IO;

namespace QuanLyRuiRoTinDung.Controllers
{
    public partial class LanhDaoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LanhDaoController> _logger;

        public LanhDaoController(ApplicationDbContext context, ILogger<LanhDaoController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Dashboard chính - Trang chủ cho Lãnh đạo
        public async Task<IActionResult> Index()
        {
            var dashboard = await GetDashboardData();
            return View(dashboard);
        }

        // Báo cáo tổng hợp rủi ro tín dụng
        public async Task<IActionResult> BaoCaoRuiRo(string search = "", string status = "")
        {
            var baoCaoQuery = _context.BaoCaoRuiRos
                .Include(b => b.NguoiLapNavigation)
                .AsQueryable();

            // Tìm kiếm theo mã hoặc loại báo cáo
            if (!string.IsNullOrEmpty(search))
            {
                baoCaoQuery = baoCaoQuery.Where(b => 
                    b.MaBaoCao.ToString().Contains(search) ||
                    (b.LoaiBaoCao ?? "").Contains(search) ||
                    (b.KienNghiXuLy ?? "").Contains(search));
            }

            // Lọc theo trạng thái
            if (!string.IsNullOrEmpty(status))
            {
                baoCaoQuery = baoCaoQuery.Where(b => b.TrangThai == status);
            }

            // Lấy danh sách trạng thái duy nhất từ database
            var statusList = await _context.BaoCaoRuiRos
                .Where(b => !string.IsNullOrEmpty(b.TrangThai))
                .Select(b => b.TrangThai)
                .Distinct()
                .OrderBy(s => s)
                .ToListAsync();

            ViewBag.StatusList = statusList;
            ViewBag.SelectedStatus = status;

            var baoCaoList = await baoCaoQuery
                .OrderByDescending(b => b.NgayLap)
                .ToListAsync();
            
            return View(baoCaoList);
        }

        // Chi tiết báo cáo rủi ro
        public async Task<IActionResult> ChiTietBaoCao(int id, bool partial = false)
        {
            var baoCao = await _context.BaoCaoRuiRos
                .Include(b => b.NguoiLapNavigation)
                .FirstOrDefaultAsync(b => b.MaBaoCao == id);

            if (baoCao == null)
            {
                return NotFound();
            }

            if (partial)
            {
                return PartialView("_ChiTietBaoCaoPartial", baoCao);
            }

            return View(baoCao);
        }

        // Tải file báo cáo
        public async Task<IActionResult> DownloadFile(int id)
        {
            var baoCao = await _context.BaoCaoRuiRos
                .FirstOrDefaultAsync(b => b.MaBaoCao == id);

            if (baoCao == null || string.IsNullOrEmpty(baoCao.DuongDanFile))
            {
                return NotFound();
            }

            // Xử lý đường dẫn file (có thể là đường dẫn tuyệt đối hoặc tương đối)
            string filePath;
            if (baoCao.DuongDanFile.StartsWith("http"))
            {
                // Nếu là URL, redirect đến URL đó
                return Redirect(baoCao.DuongDanFile);
            }
            else if (Path.IsPathRooted(baoCao.DuongDanFile))
            {
                // Nếu là đường dẫn tuyệt đối
                filePath = baoCao.DuongDanFile;
            }
            else
            {
                // Nếu là đường dẫn tương đối, tìm trong wwwroot
                var relativePath = baoCao.DuongDanFile.TrimStart('~', '/', '\\');
                filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath);
            }

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("File không tồn tại.");
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            var fileName = Path.GetFileName(baoCao.DuongDanFile);
            var contentType = GetContentType(filePath);

            return File(fileBytes, contentType, fileName);
        }

        // Helper method để xác định content type
        private string GetContentType(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            return extension switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                _ => "application/octet-stream"
            };
        }

        // Phê duyệt hoặc từ chối khoản vay
        public async Task<IActionResult> PheDuyetKhoanVay(string search = "", string status = "", string riskLevel = "")
        {
            // Luôn lọc theo trạng thái "Chờ phê duyệt"
            var fixedStatus = "Chờ phê duyệt";
            
            var khoanVayQuery = _context.KhoanVays
                .Include(k => k.MaNhanVienTinDungNavigation)
                .Include(k => k.MaLoaiVayNavigation)
                .Where(k => k.TrangThaiKhoanVay == fixedStatus)
                .AsQueryable();

            // Tìm kiếm theo mã khoản vay
            if (!string.IsNullOrEmpty(search))
            {
                khoanVayQuery = khoanVayQuery.Where(k => 
                    k.MaKhoanVayCode.Contains(search) ||
                    k.MaKhachHang.ToString().Contains(search));
            }

            // Lọc theo mức độ rủi ro - lấy từ DanhGia_RuiRo hoặc KhoanVay
            if (!string.IsNullOrEmpty(riskLevel))
            {
                // Lấy danh sách MaKhoanVay có mức độ rủi ro tương ứng từ DanhGia_RuiRo
                var loanIdsFromAssessment = await _context.DanhGiaRuiRos
                    .Where(d => d.MucDoRuiRo == riskLevel)
                    .Select(d => d.MaKhoanVay)
                    .Distinct()
                    .ToListAsync();

                // Lọc: nếu có trong DanhGia_RuiRo thì lấy từ đó, nếu không thì lấy từ KhoanVay.MucDoRuiRo
                khoanVayQuery = khoanVayQuery.Where(k => 
                    loanIdsFromAssessment.Contains(k.MaKhoanVay) || 
                    k.MucDoRuiRo == riskLevel);
            }

            // Lấy danh sách mức độ rủi ro từ DanhGia_RuiRo (ưu tiên) hoặc KhoanVay
            var riskLevelFromAssessment = await _context.DanhGiaRuiRos
                .Where(d => !string.IsNullOrEmpty(d.MucDoRuiRo))
                .Select(d => d.MucDoRuiRo)
                .Distinct()
                .ToListAsync();

            var riskLevelFromLoan = await _context.KhoanVays
                .Where(k => !string.IsNullOrEmpty(k.MucDoRuiRo))
                .Select(k => k.MucDoRuiRo)
                .Distinct()
                .ToListAsync();

            // Kết hợp và loại bỏ trùng lặp
            var riskLevelList = riskLevelFromAssessment
                .Union(riskLevelFromLoan)
                .Distinct()
                .OrderBy(r => r switch
                {
                    "Thấp" => 1,
                    "Trung bình" => 2,
                    "Cao" => 3,
                    "Rất cao" => 4,
                    _ => 99
                })
                .ToList();

            ViewBag.RiskLevelList = riskLevelList;
            ViewBag.SelectedRiskLevel = riskLevel;
            ViewBag.Search = search;
            ViewBag.Status = fixedStatus;

            var khoanVayList = await khoanVayQuery
                .OrderByDescending(k => k.NgayNopHoSo)
                .ToListAsync();

            // Load thông tin khách hàng cho tất cả khoản vay (tối ưu hóa)
            var caNhanIds = khoanVayList.Where(k => k.LoaiKhachHang == "CaNhan")
                .Select(k => k.MaKhachHang).Distinct().ToList();
            var doanhNghiepIds = khoanVayList.Where(k => k.LoaiKhachHang == "DoanhNghiep")
                .Select(k => k.MaKhachHang).Distinct().ToList();

            var khachHangCaNhans = await _context.KhachHangCaNhans
                .Where(k => caNhanIds.Contains(k.MaKhachHang))
                .ToDictionaryAsync(k => k.MaKhachHang, k => k.HoTen);

            var khachHangDoanhNghieps = await _context.KhachHangDoanhNghieps
                .Where(k => doanhNghiepIds.Contains(k.MaKhachHang))
                .ToDictionaryAsync(k => k.MaKhachHang, k => k.TenCongTy);

            // Load mức độ rủi ro từ DanhGia_RuiRo cho các khoản vay
            var loanIds = khoanVayList.Select(k => k.MaKhoanVay).ToList();
            var latestRiskAssessments = await _context.DanhGiaRuiRos
                .Where(d => loanIds.Contains(d.MaKhoanVay) && !string.IsNullOrEmpty(d.MucDoRuiRo))
                .GroupBy(d => d.MaKhoanVay)
                .Select(g => new 
                { 
                    MaKhoanVay = g.Key, 
                    MucDoRuiRo = g.OrderByDescending(x => x.NgayDanhGia ?? DateTime.MinValue).First().MucDoRuiRo 
                })
                .ToDictionaryAsync(a => a.MaKhoanVay, a => a.MucDoRuiRo);

            // Map tên khách hàng và mức độ rủi ro vào ViewData
            foreach (var loan in khoanVayList)
            {
                if (loan.LoaiKhachHang == "CaNhan" && khachHangCaNhans.TryGetValue(loan.MaKhachHang, out var hoTen))
                {
                    ViewData[$"CustomerName_{loan.MaKhoanVay}"] = hoTen;
                }
                else if (loan.LoaiKhachHang == "DoanhNghiep" && khachHangDoanhNghieps.TryGetValue(loan.MaKhachHang, out var tenCongTy))
                {
                    ViewData[$"CustomerName_{loan.MaKhoanVay}"] = tenCongTy;
                }

                // Map mức độ rủi ro (ưu tiên từ DanhGia_RuiRo)
                if (latestRiskAssessments.TryGetValue(loan.MaKhoanVay, out var assessedRiskLevel))
                {
                    ViewData[$"RiskLevel_{loan.MaKhoanVay}"] = assessedRiskLevel;
                }
                else if (!string.IsNullOrEmpty(loan.MucDoRuiRo))
                {
                    ViewData[$"RiskLevel_{loan.MaKhoanVay}"] = loan.MucDoRuiRo;
                }
            }

            return View(khoanVayList);
        }

        // Chi tiết khoản vay để quyết định phê duyệt
        public async Task<IActionResult> ChiTietKhoanVay(int id, bool partial = false)
        {
            var khoanVay = await _context.KhoanVays
                .Include(k => k.MaNhanVienTinDungNavigation)
                .Include(k => k.DanhGiaRuiRos)
                    .ThenInclude(d => d.NguoiDanhGiaNavigation)
                .Include(k => k.DanhGiaRuiRos)
                    .ThenInclude(d => d.ChiTietDanhGiaRuiRos)
                .Include(k => k.KhoanVayTaiSans)
                    .ThenInclude(kt => kt.MaTaiSanNavigation)
                        .ThenInclude(ts => ts.MaLoaiTaiSanNavigation)
                .FirstOrDefaultAsync(k => k.MaKhoanVay == id);

            if (khoanVay == null)
            {
                return NotFound();
            }

            if (partial)
            {
                return PartialView("_ChiTietKhoanVayPartial", khoanVay);
            }

            return View(khoanVay);
        }

        // Xử lý phê duyệt khoản vay
        [HttpPost]
        public async Task<IActionResult> PheDuyetKhoanVay(int id, string decision, string notes = "")
        {
            try
            {
                var khoanVay = await _context.KhoanVays.FindAsync(id);
                if (khoanVay == null)
                {
                    return NotFound();
                }

                var userId = GetCurrentUserId();
                var now = DateTime.Now;

                if (decision == "approve")
                {
                    khoanVay.TrangThaiKhoanVay = "Đã phê duyệt";
                    khoanVay.NgayPheDuyet = now;
                    khoanVay.NguoiPheDuyet = userId;
                    khoanVay.LyDoTuChoi = notes;

                    TempData["Success"] = "Phê duyệt khoản vay thành công";
                }
                else if (decision == "reject")
                {
                    khoanVay.TrangThaiKhoanVay = "Từ chối";
                    khoanVay.NgayPheDuyet = now;
                    khoanVay.NguoiPheDuyet = userId;
                    khoanVay.LyDoTuChoi = notes;

                    TempData["Success"] = "Từ chối khoản vay thành công";
                }

                _context.KhoanVays.Update(khoanVay);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(PheDuyetKhoanVay));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing loan decision");
                TempData["Error"] = "Có lỗi xảy ra khi xử lý phê duyệt khoản vay";
                return RedirectToAction(nameof(PheDuyetKhoanVay));
            }
        }

        // Theo dõi nợ xấu
        public async Task<IActionResult> TheoDoiNoXau(string search = "", string status = "", string classification = "")
        {
            var noXauQuery = _context.TheoDoiNoXaus
                .Include(n => n.MaKhoanVayNavigation)
                .Include(n => n.MaPhanLoaiNavigation)
                .Include(n => n.NguoiXuLyNavigation)
                .AsQueryable();

            // Tìm kiếm theo mã khoản vay
            if (!string.IsNullOrEmpty(search))
            {
                noXauQuery = noXauQuery.Where(n => 
                    n.MaKhoanVayNavigation.MaKhoanVayCode.Contains(search) ||
                    n.MaKhoanVayNavigation.MaKhachHang.ToString().Contains(search));
            }

            // Lọc theo trạng thái
            if (!string.IsNullOrEmpty(status))
            {
                noXauQuery = noXauQuery.Where(n => n.TrangThai == status);
            }

            // Lọc theo phân loại nợ xấu
            if (!string.IsNullOrEmpty(classification))
            {
                noXauQuery = noXauQuery.Where(n => n.MaPhanLoaiNavigation.TenPhanLoai == classification);
            }

            // Lấy danh sách trạng thái duy nhất từ database
            var statusList = await _context.TheoDoiNoXaus
                .Where(n => !string.IsNullOrEmpty(n.TrangThai))
                .Select(n => n.TrangThai)
                .Distinct()
                .OrderBy(s => s)
                .ToListAsync();

            // Lấy danh sách phân loại nợ từ database
            var classificationList = await _context.PhanLoaiNos
                .Select(p => p.TenPhanLoai)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            ViewBag.StatusList = statusList;
            ViewBag.ClassificationList = classificationList;
            ViewBag.SelectedStatus = status;
            ViewBag.SelectedClassification = classification;

            var noXauList = await noXauQuery
                .OrderByDescending(n => n.NgayPhanLoai)
                .ToListAsync();

            return View(noXauList);
        }

        // Danh sách khoản vay đã quyết định (đã phê duyệt và từ chối)
        public async Task<IActionResult> DanhSachKhoanVayDaQuyetDinh(string search = "", string tab = "approved")
        {
            var approvedQuery = _context.KhoanVays
                .Include(k => k.MaNhanVienTinDungNavigation)
                .Include(k => k.NguoiPheDuyetNavigation)
                .Where(k => k.TrangThaiKhoanVay == "Đã phê duyệt")
                .AsQueryable();

            var rejectedQuery = _context.KhoanVays
                .Include(k => k.MaNhanVienTinDungNavigation)
                .Include(k => k.NguoiPheDuyetNavigation)
                .Where(k => k.TrangThaiKhoanVay == "Từ chối")
                .AsQueryable();

            // Tìm kiếm
            if (!string.IsNullOrEmpty(search))
            {
                approvedQuery = approvedQuery.Where(k => 
                    k.MaKhoanVayCode.Contains(search) ||
                    k.MaKhachHang.ToString().Contains(search));
                
                rejectedQuery = rejectedQuery.Where(k => 
                    k.MaKhoanVayCode.Contains(search) ||
                    k.MaKhachHang.ToString().Contains(search));
            }

            var approvedLoans = await approvedQuery
                .OrderByDescending(k => k.NgayPheDuyet)
                .ToListAsync();

            var rejectedLoans = await rejectedQuery
                .OrderByDescending(k => k.NgayPheDuyet)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.Tab = tab;
            ViewBag.ApprovedLoans = approvedLoans;
            ViewBag.RejectedLoans = rejectedLoans;

            return View();
        }

        // Thống kê tỷ lệ nợ xấu
        public async Task<IActionResult> TyLeNoXau()
        {
            var statisticData = await GetBadDebtStatistics();
            return View(statisticData);
        }

        // Thống kê hiệu quả hoạt động tín dụng
        public async Task<IActionResult> ThongKeHieuQua()
        {
            var statisticData = await GetCreditOperationStatistics();
            return View(statisticData);
        }

        // API endpoint để lấy dữ liệu biểu đồ
        [HttpGet]
        public async Task<IActionResult> GetChartData(string type)
        {
            try
            {
                var data = type switch
                {
                    "loanByStatus" => await GetLoanStatusChartData(),
                    "riskLevel" => await GetRiskLevelChartData(),
                    "badDebtTrend" => await GetBadDebtTrendData(),
                    "approvalRate" => await GetApprovalRateData(),
                    "badDebtBreakdown" => await GetBadDebtBreakdownChartData(),
                    "riskDistribution" => await GetRiskDistributionData(),
                    "badDebtRatio" => await GetBadDebtRatioChartData(),
                    _ => null
                };

                if (data == null)
                    return BadRequest("Loại biểu đồ không được hỗ trợ");

                return Json(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting chart data");
                return BadRequest("Lỗi khi lấy dữ liệu biểu đồ");
            }
        }

        // (UpdateBadDebtStatus moved to enhanced partial controller)

        // ============ HELPER METHODS ============

        private async Task<dynamic> GetDashboardData()
        {
            var totalLoans = await _context.KhoanVays.CountAsync();
            var approvedLoans = await _context.KhoanVays.CountAsync(k => k.TrangThaiKhoanVay == "Đã phê duyệt");
            var rejectedLoans = await _context.KhoanVays.CountAsync(k => k.TrangThaiKhoanVay == "Từ chối");
            var pendingLoans = await _context.KhoanVays.CountAsync(k => 
                k.TrangThaiKhoanVay == "Chờ phê duyệt" || 
                k.TrangThaiKhoanVay == "Đang xem xét");
            
            var totalLoanAmount = await _context.KhoanVays.SumAsync(k => k.SoTienVay);
            var badDebtCount = await _context.TheoDoiNoXaus.CountAsync();
            var badDebtAmount = await _context.TheoDoiNoXaus.SumAsync(n => n.SoDuNo ?? 0);

            var badDebtRatio = totalLoanAmount > 0 ? badDebtAmount / totalLoanAmount * 100 : 0;

            return new
            {
                TotalLoans = totalLoans,
                ApprovedLoans = approvedLoans,
                RejectedLoans = rejectedLoans,
                PendingLoans = pendingLoans,
                TotalLoanAmount = totalLoanAmount,
                BadDebtCount = badDebtCount,
                BadDebtAmount = badDebtAmount,
                BadDebtRatio = Math.Round(badDebtRatio, 2),
                RecentReports = await _context.BaoCaoRuiRos
                    .Include(b => b.NguoiLapNavigation)
                    .OrderByDescending(b => b.NgayLap)
                    .Take(5)
                    .ToListAsync(),
                HighRiskLoans = await _context.KhoanVays
                    .Where(k => k.MucDoRuiRo == "Cao")
                    .CountAsync(),
                MediumRiskLoans = await _context.KhoanVays
                    .Where(k => k.MucDoRuiRo == "Trung bình")
                    .CountAsync(),
                LowRiskLoans = await _context.KhoanVays
                    .Where(k => k.MucDoRuiRo == "Thấp")
                    .CountAsync()
            };
        }

        private async Task<dynamic> GetBadDebtStatistics()
        {
            var totalLoans = await _context.KhoanVays.CountAsync();
            var noXauLoans = await _context.TheoDoiNoXaus.CountAsync();
            var totalAmount = await _context.KhoanVays.SumAsync(k => k.SoTienVay);
            var badDebtAmount = await _context.TheoDoiNoXaus.SumAsync(n => n.SoDuNo ?? 0);

            var noXauByType = await _context.TheoDoiNoXaus
                .Include(n => n.MaPhanLoaiNavigation)
                .GroupBy(n => n.MaPhanLoaiNavigation != null ? n.MaPhanLoaiNavigation.TenPhanLoai : "Chưa phân loại")
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            var recentBadDebts = await _context.TheoDoiNoXaus
                .Include(n => n.MaKhoanVayNavigation)
                .Include(n => n.MaPhanLoaiNavigation)
                .OrderByDescending(n => n.NgayPhanLoai)
                .Take(10)
                .ToListAsync();

            return new
            {
                TotalLoans = totalLoans,
                BadDebtLoans = noXauLoans,
                BadDebtRatio = Math.Round(totalLoans > 0 ? (decimal)noXauLoans / totalLoans * 100 : 0, 2),
                TotalLoanAmount = totalAmount,
                BadDebtAmount = badDebtAmount,
                BadDebtAmountRatio = Math.Round(totalAmount > 0 ? badDebtAmount / totalAmount * 100 : 0, 2),
                BadDebtByType = noXauByType,
                RecentBadDebts = recentBadDebts
            };
        }

        private async Task<dynamic> GetCreditOperationStatistics()
        {
            var now = DateTime.Now;
            var lastMonth = now.AddMonths(-1);
            var lastYear = now.AddYears(-1);

            var loansThisMonth = await _context.KhoanVays
                .CountAsync(k => k.NgayNopHoSo.HasValue && 
                                k.NgayNopHoSo.Value.Year == now.Year &&
                                k.NgayNopHoSo.Value.Month == now.Month);
            
            var loansLastMonth = await _context.KhoanVays
                .CountAsync(k => k.NgayNopHoSo.HasValue && 
                                k.NgayNopHoSo.Value.Year == lastMonth.Year &&
                                k.NgayNopHoSo.Value.Month == lastMonth.Month);
            
            var approvedThisMonth = await _context.KhoanVays
                .CountAsync(k => k.TrangThaiKhoanVay == "Đã phê duyệt" && 
                                k.NgayPheDuyet.HasValue &&
                                k.NgayPheDuyet.Value.Year == now.Year && 
                                k.NgayPheDuyet.Value.Month == now.Month);

            var rejectedThisMonth = await _context.KhoanVays
                .CountAsync(k => k.TrangThaiKhoanVay == "Từ chối" && 
                                k.NgayPheDuyet.HasValue &&
                                k.NgayPheDuyet.Value.Year == now.Year && 
                                k.NgayPheDuyet.Value.Month == now.Month);

            var approvalRate = loansThisMonth > 0 
                ? Math.Round((decimal)approvedThisMonth / loansThisMonth * 100, 2)
                : 0;

            // Tính tỷ lệ nợ xấu
            var totalLoans = await _context.KhoanVays.CountAsync();
            var badDebtLoans = await _context.TheoDoiNoXaus
                .Select(n => n.MaKhoanVay)
                .Distinct()
                .CountAsync();
            
            // Đếm thêm các khoản vay có MaPhanLoaiNo >= 3 nhưng chưa có trong TheoDoiNoXau
            var badDebtFromClassification = await _context.KhoanVays
                .Where(k => k.MaPhanLoaiNo.HasValue && 
                            k.MaPhanLoaiNo >= 3 && 
                            !_context.TheoDoiNoXaus.Any(t => t.MaKhoanVay == k.MaKhoanVay))
                .CountAsync();
            
            var totalBadDebt = badDebtLoans + badDebtFromClassification;
            var badDebtRatio = totalLoans > 0 
                ? Math.Round((decimal)totalBadDebt / totalLoans * 100, 2)
                : 0;

            var averageLoanAmount = await _context.KhoanVays
                .Where(k => k.NgayNopHoSo.HasValue && k.NgayNopHoSo.Value >= lastYear)
                .AverageAsync(k => (decimal?)k.SoTienVay) ?? 0;

            // Tính thời gian xử lý trung bình (ngày từ nộp hồ sơ đến phê duyệt)
            var processingTimes = await _context.KhoanVays
                .Where(k => k.NgayNopHoSo.HasValue && k.NgayPheDuyet.HasValue && 
                           k.NgayPheDuyet.Value >= lastYear)
                .Select(k => (k.NgayPheDuyet.Value - k.NgayNopHoSo.Value).Days)
                .ToListAsync();
            
            var avgProcessingDays = processingTimes.Any() 
                ? Math.Round(processingTimes.Average(), 1) 
                : 0;

            // Tính tỷ lệ hoàn thành đúng hạn (khoản vay đã thanh toán đúng hạn)
            var totalCompleted = await _context.KhoanVays
                .CountAsync(k => k.TrangThaiKhoanVay == "Đã thanh toán");
            
            // Sử dụng lại biến totalLoans đã khai báo ở trên
            var onTimeCompletionRate = totalLoans > 0 
                ? Math.Round((decimal)totalCompleted / totalLoans * 100, 2) 
                : 0;

            // Tính chất lượng danh mục (dựa trên tỷ lệ nợ xấu)
            var badDebtCount = await _context.KhoanVays
                .CountAsync(k => k.SoNgayQuaHan > 0 || k.MaPhanLoaiNo.HasValue);
            var portfolioQuality = totalLoans > 0 
                ? Math.Round((decimal)(totalLoans - badDebtCount) / totalLoans * 100, 2)
                : 100;
            
            var qualityGrade = portfolioQuality >= 95 ? "A" : 
                              portfolioQuality >= 85 ? "B" : 
                              portfolioQuality >= 75 ? "C" : "D";

            return new
            {
                LoansThisMonth = loansThisMonth,
                LoansLastMonth = loansLastMonth,
                ApprovedThisMonth = approvedThisMonth,
                RejectedThisMonth = rejectedThisMonth,
                ApprovalRate = approvalRate,
                BadDebtRatio = badDebtRatio,
                AverageLoanAmount = (long)averageLoanAmount,
                AvgProcessingDays = avgProcessingDays,
                OnTimeCompletionRate = onTimeCompletionRate,
                PortfolioQuality = portfolioQuality,
                QualityGrade = qualityGrade,
                MonthlySummary = await GetMonthlySummary()
            };
        }

        private async Task<dynamic> GetLoanStatusChartData()
        {
            var statusData = await _context.KhoanVays
                .GroupBy(k => k.TrangThaiKhoanVay)
                .Select(g => new { Status = g.Key ?? "Chưa xác định", Count = g.Count() })
                .ToListAsync();

            // Định nghĩa màu sắc cho từng trạng thái
            var statusColors = new Dictionary<string, string>
            {
                { "Đã phê duyệt", "#198754" },      // Xanh lá
                { "Chờ phê duyệt", "#ffc107" },     // Vàng
                { "Đã giải ngân", "#0d6efd" },     // Xanh dương
                { "Từ chối", "#dc3545" },           // Đỏ
                { "Đang xử lý", "#6c757d" },        // Xám
                { "Đang xem xét", "#17a2b8" },     // Xanh nhạt
                { "Chờ bổ sung", "#fd7e14" },       // Cam
                { "Đã thanh toán", "#20c997" },     // Xanh ngọc
                { "Quá hạn", "#e83e8c" },           // Hồng
                { "Chưa xác định", "#6c757d" }      // Xám
            };

            return new
            {
                Labels = statusData.Select(x => x.Status).ToList(),
                Data = statusData.Select(x => x.Count).ToList(),
                BackgroundColor = statusData.Select(x => 
                    statusColors.ContainsKey(x.Status) ? statusColors[x.Status] : "#6c757d").ToList()
            };
        }

        private async Task<dynamic> GetRiskLevelChartData()
        {
            // Lấy tất cả khoản vay
            var allLoans = await _context.KhoanVays
                .Select(k => new { k.MaKhoanVay, k.MucDoRuiRo })
                .ToListAsync();
            
            // Lấy tất cả đánh giá rủi ro, nhóm theo MaKhoanVay và lấy đánh giá mới nhất
            var latestAssessments = await _context.DanhGiaRuiRos
                .Where(d => !string.IsNullOrEmpty(d.MucDoRuiRo))
                .GroupBy(d => d.MaKhoanVay)
                .Select(g => new 
                { 
                    MaKhoanVay = g.Key, 
                    MucDoRuiRo = g.OrderByDescending(x => x.NgayDanhGia ?? DateTime.MinValue).First().MucDoRuiRo 
                })
                .ToListAsync();

            var assessmentDict = latestAssessments.ToDictionary(a => a.MaKhoanVay, a => a.MucDoRuiRo);

            // Xác định mức độ rủi ro cho từng khoản vay
            // Ưu tiên: DanhGia_RuiRo > KhoanVay.MucDoRuiRo > "Chưa đánh giá"
            var riskGroups = allLoans
                .Select(k => 
                {
                    string riskLevel;
                    if (assessmentDict.ContainsKey(k.MaKhoanVay))
                    {
                        riskLevel = assessmentDict[k.MaKhoanVay];
                    }
                    else if (!string.IsNullOrEmpty(k.MucDoRuiRo))
                    {
                        riskLevel = k.MucDoRuiRo;
                    }
                    else
                    {
                        riskLevel = "Chưa đánh giá";
                    }
                    return new { Risk = riskLevel };
                })
                .GroupBy(x => x.Risk)
                .Select(g => new { Risk = g.Key, Count = g.Count() })
                .ToList();

            // Định nghĩa thứ tự và màu sắc theo: Thấp, Trung bình, Cao, Rất cao, Chưa đánh giá
            var riskOrder = new Dictionary<string, int>
            {
                { "Thấp", 1 },
                { "Trung bình", 2 },
                { "Cao", 3 },
                { "Rất cao", 4 },
                { "Chưa đánh giá", 5 },
                { "Chưa xác định", 99 }
            };

            var riskColors = new Dictionary<string, string>
            {
                { "Thấp", "#28a745" },          // Xanh lá - Rủi ro thấp
                { "Trung bình", "#ffc107" },    // Vàng - Rủi ro trung bình
                { "Cao", "#dc3545" },           // Đỏ - Rủi ro cao
                { "Rất cao", "#8b0000" },       // Đỏ đậm - Rủi ro rất cao
                { "Chưa đánh giá", "#6c757d" }, // Xám - Chưa đánh giá
                { "Chưa xác định", "#6c757d" }  // Xám - Chưa xác định
            };

            // Sắp xếp theo thứ tự: Thấp -> Trung bình -> Cao -> Rất cao -> Chưa đánh giá
            var sortedRiskData = riskGroups
                .OrderBy(x => riskOrder.ContainsKey(x.Risk) ? riskOrder[x.Risk] : 99)
                .ToList();

            return new
            {
                Labels = sortedRiskData.Select(x => x.Risk).ToList(),
                Data = sortedRiskData.Select(x => x.Count).ToList(),
                BackgroundColor = sortedRiskData.Select(x => riskColors.ContainsKey(x.Risk) ? riskColors[x.Risk] : "#6c757d").ToList()
            };
        }

        private async Task<dynamic> GetBadDebtTrendData()
        {
            var last12Months = Enumerable.Range(0, 12)
                .Select(i => DateTime.Now.AddMonths(-i))
                .OrderBy(d => d)
                .ToList();

            var trendData = new List<int>();
            foreach (var month in last12Months)
            {
                var count = await _context.TheoDoiNoXaus
                    .CountAsync(n => n.NgayPhanLoai.Month == month.Month && 
                                    n.NgayPhanLoai.Year == month.Year);
                trendData.Add(count);
            }

            return new
            {
                Labels = last12Months.Select(d => d.ToString("MM/yyyy")).ToList(),
                Data = trendData
            };
        }

        private async Task<dynamic> GetApprovalRateData()
        {
            var last12Months = Enumerable.Range(0, 12)
                .Select(i => DateTime.Now.AddMonths(-i))
                .OrderBy(d => d)
                .ToList();

            var approvalRates = new List<decimal>();
            foreach (var month in last12Months)
            {
                var submitted = await _context.KhoanVays
                    .CountAsync(k => k.NgayNopHoSo.HasValue && 
                                    k.NgayNopHoSo.Value.Month == month.Month && 
                                    k.NgayNopHoSo.Value.Year == month.Year);
                
                var approved = await _context.KhoanVays
                    .CountAsync(k => k.TrangThaiKhoanVay == "Đã phê duyệt" && 
                                    k.NgayNopHoSo.HasValue &&
                                    k.NgayNopHoSo.Value.Month == month.Month && 
                                    k.NgayNopHoSo.Value.Year == month.Year);

                var rate = submitted > 0 ? (decimal)approved / submitted * 100 : 0;
                approvalRates.Add(Math.Round(rate, 2));
            }

            return new
            {
                Labels = last12Months.Select(d => d.ToString("MM/yyyy")).ToList(),
                Data = approvalRates
            };
        }

        private async Task<dynamic> GetBadDebtBreakdownChartData()
        {
            var breakdown = await _context.TheoDoiNoXaus
                .GroupBy(n => n.SoNgayQuaHan.HasValue ? 
                    (n.SoNgayQuaHan <= 30 ? "0-30 ngày" : 
                     n.SoNgayQuaHan <= 90 ? "30-90 ngày" :
                     n.SoNgayQuaHan <= 180 ? "90-180 ngày" : "> 180 ngày") 
                    : "Chưa xác định")
                .Select(g => new { Duration = g.Key, Count = g.Count() })
                .OrderBy(x => x.Duration)
                .ToListAsync();

            return new
            {
                Labels = breakdown.Select(x => x.Duration).ToList(),
                Data = breakdown.Select(x => x.Count).ToList(),
                BackgroundColor = new[] { "#28a745", "#ffc107", "#fd7e14", "#dc3545" }
            };
        }

        private async Task<dynamic> GetRiskDistributionData()
        {
            var highRisk = await _context.KhoanVays.CountAsync(k => k.MucDoRuiRo == "Cao");
            var mediumRisk = await _context.KhoanVays.CountAsync(k => k.MucDoRuiRo == "Trung bình");
            var lowRisk = await _context.KhoanVays.CountAsync(k => k.MucDoRuiRo == "Thấp");

            return new
            {
                Labels = new[] { "Rủi ro cao", "Rủi ro trung bình", "Rủi ro thấp" },
                Data = new[] { highRisk, mediumRisk, lowRisk },
                BackgroundColor = new[] { "#dc3545", "#ffc107", "#28a745" }
            };
        }

        private async Task<dynamic> GetBadDebtRatioChartData()
        {
            var totalLoans = await _context.KhoanVays.CountAsync();
            var badDebtLoans = await _context.TheoDoiNoXaus
                .Select(n => n.MaKhoanVay)
                .Distinct()
                .CountAsync();
            
            // Đếm thêm các khoản vay có MaPhanLoaiNo >= 3 nhưng chưa có trong TheoDoiNoXau
            var badDebtFromClassification = await _context.KhoanVays
                .Where(k => k.MaPhanLoaiNo.HasValue && 
                            k.MaPhanLoaiNo >= 3 && 
                            !_context.TheoDoiNoXaus.Any(t => t.MaKhoanVay == k.MaKhoanVay))
                .CountAsync();
            
            var totalBadDebt = badDebtLoans + badDebtFromClassification;
            var goodDebt = totalLoans - totalBadDebt;

            return new
            {
                Labels = new[] { "Nợ tốt", "Nợ xấu" },
                Data = new[] { goodDebt, totalBadDebt },
                BackgroundColor = new[] { "#28a745", "#dc3545" }
            };
        }

        private async Task<dynamic> GetMonthlySummary()
        {
            var last6Months = Enumerable.Range(0, 6)
                .Select(i => DateTime.Now.AddMonths(-i))
                .OrderBy(d => d)
                .ToList();

            var summary = new List<dynamic>();
            foreach (var month in last6Months)
            {
                var submitted = await _context.KhoanVays
                    .CountAsync(k => k.NgayNopHoSo.HasValue && 
                                    k.NgayNopHoSo.Value.Month == month.Month && 
                                    k.NgayNopHoSo.Value.Year == month.Year);
                
                var approved = await _context.KhoanVays
                    .CountAsync(k => k.TrangThaiKhoanVay == "Đã phê duyệt" && 
                                    k.NgayNopHoSo.HasValue &&
                                    k.NgayNopHoSo.Value.Month == month.Month && 
                                    k.NgayNopHoSo.Value.Year == month.Year);

                // Tính số khoản từ chối thực tế trong tháng (chỉ tính những khoản đã bị từ chối)
                var rejected = await _context.KhoanVays
                    .CountAsync(k => k.TrangThaiKhoanVay == "Từ chối" && 
                                    k.NgayPheDuyet.HasValue &&
                                    k.NgayPheDuyet.Value.Month == month.Month && 
                                    k.NgayPheDuyet.Value.Year == month.Year);

                // Tính tổng vốn vay đã phê duyệt trong tháng
                var totalLoanAmount = await _context.KhoanVays
                    .Where(k => k.TrangThaiKhoanVay == "Đã phê duyệt" && 
                               k.NgayPheDuyet.HasValue &&
                               k.NgayPheDuyet.Value.Month == month.Month && 
                               k.NgayPheDuyet.Value.Year == month.Year)
                    .SumAsync(k => (decimal?)k.SoTienVay) ?? 0;

                summary.Add(new
                {
                    Month = month.ToString("MM/yyyy"),
                    Submitted = submitted,
                    Approved = approved,
                    Rejected = rejected,
                    TotalLoanAmount = totalLoanAmount
                });
            }

            return summary;
        }

        private static int GetCurrentUserId()
        {
            // TODO: Implement based on your authentication method
            // For now, return a default value
            return 1;
        }

        // ============================================
        // QUẢN LÝ NHÂN VIÊN - PHÂN VAI TRÒ VÀ PHÒNG BAN
        // ============================================

        // Danh sách nhân viên với filter đã phân/chưa phân vai trò
        public async Task<IActionResult> QuanLyNhanVien(string search = "", string filter = "all")
        {
            var query = _context.NguoiDungs
                .Include(u => u.MaVaiTroNavigation)
                .AsQueryable();

            // Loại bỏ Admin và Lãnh đạo khỏi danh sách
            var adminRole = await _context.VaiTros
                .FirstOrDefaultAsync(v => v.TenVaiTro == "Admin" && v.TrangThaiHoatDong == true);
            var lanhDaoRole = await _context.VaiTros
                .FirstOrDefaultAsync(v => v.TenVaiTro == "LanhDao" && v.TrangThaiHoatDong == true);

            if (adminRole != null)
            {
                query = query.Where(u => u.MaVaiTro != adminRole.MaVaiTro);
            }
            if (lanhDaoRole != null)
            {
                query = query.Where(u => u.MaVaiTro != lanhDaoRole.MaVaiTro);
            }

            // Tìm kiếm
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(u => 
                    u.TenDangNhap.Contains(search) ||
                    u.HoTen.Contains(search) ||
                    (u.Email != null && u.Email.Contains(search)));
            }

            // Lọc theo trạng thái phân vai trò
            if (filter == "assigned")
            {
                // Đã phân vai trò và phòng ban
                query = query.Where(u => u.MaVaiTro > 0 && u.MaPhongBan.HasValue);
            }
            else if (filter == "unassigned")
            {
                // Chưa phân vai trò hoặc phòng ban
                query = query.Where(u => u.MaVaiTro == 0 || !u.MaPhongBan.HasValue);
            }
            // "all" - hiển thị tất cả

            var nhanViens = await query
                .OrderByDescending(u => u.NgayTao)
                .ToListAsync();

            // Load thông tin phòng ban cho các nhân viên
            var phongBanIds = nhanViens
                .Where(u => u.MaPhongBan.HasValue)
                .Select(u => u.MaPhongBan.Value)
                .Distinct()
                .ToList();

            var phongBans = await _context.PhongBans
                .Where(p => phongBanIds.Contains(p.MaPhongBan))
                .ToDictionaryAsync(p => p.MaPhongBan, p => p.TenPhongBan);

            ViewBag.PhongBans = phongBans;
            ViewBag.VaiTros = await _context.VaiTros
                .Where(v => v.TrangThaiHoatDong == true 
                    && v.TenVaiTro != "Admin" 
                    && v.TenVaiTro != "LanhDao")
                .OrderBy(v => v.TenVaiTro)
                .ToListAsync();

            ViewBag.AllPhongBans = await _context.PhongBans
                .Where(p => p.TrangThaiHoatDong == true)
                .OrderBy(p => p.TenPhongBan)
                .ToListAsync();

            // Tạo mapping giữa vai trò và phòng ban để tự động chọn
            var roleDepartmentMapping = new Dictionary<string, string>();
            var allPhongBans = await _context.PhongBans
                .Where(p => p.TrangThaiHoatDong == true)
                .ToListAsync();
            
            foreach (var pb in allPhongBans)
            {
                if (pb.TenPhongBan.Contains("Tín dụng", StringComparison.OrdinalIgnoreCase) || 
                    pb.TenPhongBan.Contains("TinDung", StringComparison.OrdinalIgnoreCase))
                {
                    roleDepartmentMapping["NhanVienTinDung"] = pb.MaPhongBan.ToString();
                }
                else if (pb.TenPhongBan.Contains("Rủi ro", StringComparison.OrdinalIgnoreCase) || 
                         pb.TenPhongBan.Contains("RuiRo", StringComparison.OrdinalIgnoreCase) ||
                         pb.TenPhongBan.Contains("Quản lý Rủi ro", StringComparison.OrdinalIgnoreCase))
                {
                    roleDepartmentMapping["QuanLyRuiRo"] = pb.MaPhongBan.ToString();
                }
            }
            ViewBag.RoleDepartmentMapping = roleDepartmentMapping;

            ViewBag.Search = search;
            ViewBag.Filter = filter;

            // Set user info for layout
            ViewBag.HoTen = HttpContext.Session.GetString("HoTen") ?? "Lãnh đạo";
            ViewBag.TenVaiTro = HttpContext.Session.GetString("TenVaiTro") ?? "LanhDao";

            return View(nhanViens);
        }

        // Cập nhật vai trò và phòng ban cho nhân viên
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CapNhatVaiTroPhongBan(int maNguoiDung, int maVaiTro, int? maPhongBan)
        {
            var maNguoiDungStr = HttpContext.Session.GetString("MaNguoiDung");
            if (string.IsNullOrEmpty(maNguoiDungStr))
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập để tiếp tục." });
            }

            var nguoiDung = await _context.NguoiDungs.FindAsync(maNguoiDung);
            if (nguoiDung == null)
            {
                return Json(new { success = false, message = "Không tìm thấy nhân viên." });
            }

            // Kiểm tra vai trò hợp lệ
            var vaiTro = await _context.VaiTros
                .FirstOrDefaultAsync(v => v.MaVaiTro == maVaiTro && v.TrangThaiHoatDong == true);
            
            if (vaiTro == null)
            {
                return Json(new { success = false, message = "Vai trò không hợp lệ." });
            }

            // Kiểm tra phòng ban nếu có
            if (maPhongBan.HasValue)
            {
                var phongBan = await _context.PhongBans
                    .FirstOrDefaultAsync(p => p.MaPhongBan == maPhongBan.Value && p.TrangThaiHoatDong == true);
                
                if (phongBan == null)
                {
                    return Json(new { success = false, message = "Phòng ban không hợp lệ." });
                }
            }

            // Cập nhật thông tin
            nguoiDung.MaVaiTro = maVaiTro;
            nguoiDung.MaPhongBan = maPhongBan;
            nguoiDung.NgayCapNhat = DateTime.Now;
            
            if (int.TryParse(maNguoiDungStr, out int nguoiCapNhat))
            {
                nguoiDung.NguoiCapNhat = nguoiCapNhat;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Lãnh đạo {MaNguoiDung} đã cập nhật vai trò/phòng ban cho nhân viên {MaNhanVien}", 
                maNguoiDungStr, maNguoiDung);

            // Lấy thông tin vai trò và phòng ban để trả về
            var tenVaiTro = vaiTro.TenVaiTro;
            string tenPhongBan = "";
            if (maPhongBan.HasValue)
            {
                var phongBan = await _context.PhongBans.FindAsync(maPhongBan.Value);
                tenPhongBan = phongBan?.TenPhongBan ?? "";
            }

            return Json(new 
            { 
                success = true, 
                message = "Cập nhật vai trò và phòng ban thành công!",
                tenVaiTro = tenVaiTro,
                tenPhongBan = tenPhongBan
            });
        }
    }
}
