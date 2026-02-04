using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuanLyRuiRoTinDung.Models.EF;
using QuanLyRuiRoTinDung.Models.Entities;

namespace QuanLyRuiRoTinDung.Services
{
    public interface IReportService
    {
        Task<ReportViewModel> GetReportDataAsync(int maNhanVien, ReportFilterModel filter);
        Task<List<LoanByTypeData>> GetLoansByTypeAsync(int maNhanVien, ReportFilterModel filter);
        Task<List<LoanByStatusData>> GetLoansByStatusAsync(int maNhanVien, ReportFilterModel filter);
        Task<List<MonthlyLoanData>> GetMonthlyLoanDataAsync(int maNhanVien, ReportFilterModel filter);
        Task<List<QuarterlyLoanData>> GetQuarterlyLoanDataAsync(int maNhanVien, ReportFilterModel filter);
        Task<List<RiskLevelData>> GetRiskLevelDataAsync(int maNhanVien, ReportFilterModel filter);
        Task<List<CustomerTypeData>> GetCustomerTypeDataAsync(int maNhanVien, ReportFilterModel filter);
        Task<TrendData> GetTrendDataAsync(int maNhanVien, int year);
        Task<List<int>> GetAvailableYearsAsync(int maNhanVien);
        Task<string?> GetEmployeeNameAsync(int maNhanVien);
        // New methods for additional charts
        Task<List<PaymentStatusData>> GetPaymentStatusDataAsync(int maNhanVien, ReportFilterModel filter);
        Task<List<DisbursementTrendData>> GetDisbursementTrendDataAsync(int maNhanVien, ReportFilterModel filter);
        Task<CollectionRateData> GetCollectionRateDataAsync(int maNhanVien, ReportFilterModel filter);
        Task<List<LoanAmountRangeData>> GetLoanAmountRangeDataAsync(int maNhanVien, ReportFilterModel filter);
    }

    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ReportService> _logger;

        // Các trạng thái được coi là "Đã duyệt"
        private readonly string[] _approvedStatuses = new[] 
        { 
            "Đã phê duyệt", "Đã giải ngân", "Đang vay", "Đã thanh toán" 
        };

        // Các trạng thái "Chờ duyệt"
        private readonly string[] _pendingStatuses = new[] 
        { 
            "Chờ duyệt", "Đang xử lý", "Chờ xét duyệt" 
        };

        public ReportService(ApplicationDbContext context, ILogger<ReportService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<string?> GetEmployeeNameAsync(int maNhanVien)
        {
            var nhanVien = await _context.NguoiDungs
                .FirstOrDefaultAsync(n => n.MaNguoiDung == maNhanVien);
            return nhanVien?.HoTen;
        }

        public async Task<List<int>> GetAvailableYearsAsync(int maNhanVien)
        {
            var years = await _context.KhoanVays
                .Where(k => k.NgayTao.HasValue && k.MaNhanVienTinDung == maNhanVien)
                .Select(k => k.NgayTao!.Value.Year)
                .Distinct()
                .OrderByDescending(y => y)
                .ToListAsync();

            if (!years.Any())
            {
                years.Add(DateTime.Now.Year);
            }

            return years;
        }

        private IQueryable<KhoanVay> ApplyFilters(IQueryable<KhoanVay> query, int maNhanVien, ReportFilterModel filter)
        {
            // Lọc theo nhân viên
            query = query.Where(k => k.MaNhanVienTinDung == maNhanVien);

            // Lọc theo khoảng thời gian
            if (filter.FromDate.HasValue)
            {
                query = query.Where(k => k.NgayTao.HasValue && k.NgayTao.Value.Date >= filter.FromDate.Value.Date);
            }

            if (filter.ToDate.HasValue)
            {
                query = query.Where(k => k.NgayTao.HasValue && k.NgayTao.Value.Date <= filter.ToDate.Value.Date);
            }

            // Lọc theo năm (chỉ áp dụng nếu không có khoảng thời gian)
            if (!filter.FromDate.HasValue && !filter.ToDate.HasValue && filter.Year.HasValue)
            {
                query = query.Where(k => k.NgayTao.HasValue && k.NgayTao.Value.Year == filter.Year.Value);

                // Lọc theo quý
                if (filter.Quarter.HasValue)
                {
                    var quarterStartMonth = (filter.Quarter.Value - 1) * 3 + 1;
                    var quarterEndMonth = quarterStartMonth + 2;
                    query = query.Where(k => k.NgayTao.HasValue && 
                        k.NgayTao.Value.Month >= quarterStartMonth && 
                        k.NgayTao.Value.Month <= quarterEndMonth);
                }

                // Lọc theo tháng
                if (filter.Month.HasValue)
                {
                    query = query.Where(k => k.NgayTao.HasValue && k.NgayTao.Value.Month == filter.Month.Value);
                }
            }

            return query;
        }

        public async Task<ReportViewModel> GetReportDataAsync(int maNhanVien, ReportFilterModel filter)
        {
            var viewModel = new ReportViewModel();

            // Query cơ bản - LỌC THEO NHÂN VIÊN
            var query = ApplyFilters(_context.KhoanVays.AsQueryable(), maNhanVien, filter);

            // ===== TỔNG SỐ HỒ SƠ =====
            viewModel.TotalLoans = await query.CountAsync();

            // ===== HỒ SƠ CHỜ DUYỆT =====
            viewModel.PendingLoans = await query
                .Where(k => _pendingStatuses.Contains(k.TrangThaiKhoanVay))
                .CountAsync();

            viewModel.PendingAmount = await query
                .Where(k => _pendingStatuses.Contains(k.TrangThaiKhoanVay))
                .SumAsync(k => k.SoTienVay);

            // ===== HỒ SƠ ĐÃ DUYỆT =====
            viewModel.ApprovedLoans = await query
                .Where(k => _approvedStatuses.Contains(k.TrangThaiKhoanVay))
                .CountAsync();

            viewModel.ApprovedAmount = await query
                .Where(k => _approvedStatuses.Contains(k.TrangThaiKhoanVay))
                .SumAsync(k => k.SoTienVay);

            // ===== TỔNG DƯ NỢ (CHỈ HỒ SƠ ĐÃ DUYỆT VÀ ĐANG VAY) =====
            viewModel.TotalOutstandingDebt = await query
                .Where(k => k.TrangThaiKhoanVay == "Đang vay" || k.TrangThaiKhoanVay == "Đã giải ngân")
                .SumAsync(k => k.SoTienVay);

            // ===== TỔNG SỐ TIỀN VAY (TẤT CẢ HỒ SƠ) =====
            viewModel.TotalLoanAmount = await query.SumAsync(k => k.SoTienVay);

            // ===== TỔNG SỐ TIỀN ĐÃ GIẢI NGÂN (CHỈ HỒ SƠ ĐÃ DUYỆT) =====
            viewModel.TotalDisbursedAmount = await query
                .Where(k => k.TrangThaiKhoanVay == "Đã giải ngân" || k.TrangThaiKhoanVay == "Đang vay" || k.TrangThaiKhoanVay == "Đã thanh toán")
                .SumAsync(k => k.SoTienVay);

            // ===== SỐ KHOẢN VAY ĐANG HOẠT ĐỘNG =====
            viewModel.ActiveLoans = await query
                .Where(k => k.TrangThaiKhoanVay == "Đang vay")
                .CountAsync();

            // ===== SỐ KHOẢN VAY QUÁ HẠN =====
            viewModel.OverdueLoans = await query
                .Where(k => k.SoNgayQuaHan.HasValue && k.SoNgayQuaHan > 0)
                .CountAsync();

            viewModel.OverdueAmount = await query
                .Where(k => k.SoNgayQuaHan.HasValue && k.SoNgayQuaHan > 0)
                .SumAsync(k => k.SoTienVay);

            // ===== SỐ KHOẢN VAY ĐÃ THANH TOÁN =====
            viewModel.CompletedLoans = await query
                .Where(k => k.TrangThaiKhoanVay == "Đã thanh toán")
                .CountAsync();

            // ===== SỐ KHOẢN VAY BỊ TỪ CHỐI =====
            viewModel.RejectedLoans = await query
                .Where(k => k.TrangThaiKhoanVay == "Từ chối")
                .CountAsync();

            // ===== TỶ LỆ PHÊ DUYỆT =====
            var totalProcessed = await query
                .Where(k => _approvedStatuses.Contains(k.TrangThaiKhoanVay) || k.TrangThaiKhoanVay == "Từ chối")
                .CountAsync();
            
            var totalApproved = await query
                .Where(k => _approvedStatuses.Contains(k.TrangThaiKhoanVay))
                .CountAsync();

            viewModel.ApprovalRate = totalProcessed > 0 ? Math.Round((decimal)totalApproved / totalProcessed * 100, 1) : 0;

            // ===== LÃI SUẤT TRUNG BÌNH (CHỈ HỒ SƠ ĐÃ DUYỆT) =====
            var approvedQuery = query.Where(k => _approvedStatuses.Contains(k.TrangThaiKhoanVay));
            viewModel.AverageInterestRate = await approvedQuery.AnyAsync() 
                ? await approvedQuery.AverageAsync(k => k.LaiSuat) 
                : 0;

            // ===== KỲ HẠN TRUNG BÌNH (CHỈ HỒ SƠ ĐÃ DUYỆT) =====
            viewModel.AverageTerm = await approvedQuery.AnyAsync() 
                ? (int)await approvedQuery.AverageAsync(k => k.KyHanVay) 
                : 0;

            // ===== SỐ KHÁCH HÀNG =====
            viewModel.IndividualCustomers = await query
                .Where(k => k.LoaiKhachHang == "CaNhan")
                .Select(k => k.MaKhachHang)
                .Distinct()
                .CountAsync();

            viewModel.EnterpriseCustomers = await query
                .Where(k => k.LoaiKhachHang == "DoanhNghiep")
                .Select(k => k.MaKhachHang)
                .Distinct()
                .CountAsync();

            // Thông tin filter
            viewModel.SelectedYear = filter.Year;
            viewModel.SelectedQuarter = filter.Quarter;
            viewModel.SelectedMonth = filter.Month;
            viewModel.FromDate = filter.FromDate;
            viewModel.ToDate = filter.ToDate;
            viewModel.AvailableYears = await GetAvailableYearsAsync(maNhanVien);

            return viewModel;
        }

        public async Task<List<LoanByTypeData>> GetLoansByTypeAsync(int maNhanVien, ReportFilterModel filter)
        {
            var query = ApplyFilters(
                _context.KhoanVays.Include(k => k.MaLoaiVayNavigation).AsQueryable(),
                maNhanVien, filter);

            var result = await query
                .GroupBy(k => new { k.MaLoaiVay, TenLoaiVay = k.MaLoaiVayNavigation != null ? k.MaLoaiVayNavigation.TenLoaiVay : "Chưa phân loại" })
                .Select(g => new LoanByTypeData
                {
                    LoanTypeName = g.Key.TenLoaiVay ?? "Chưa phân loại",
                    Count = g.Count(),
                    TotalAmount = g.Sum(k => k.SoTienVay),
                    ApprovedCount = g.Count(k => _approvedStatuses.Contains(k.TrangThaiKhoanVay)),
                    ApprovedAmount = g.Where(k => _approvedStatuses.Contains(k.TrangThaiKhoanVay)).Sum(k => k.SoTienVay)
                })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            return result;
        }

        public async Task<List<LoanByStatusData>> GetLoansByStatusAsync(int maNhanVien, ReportFilterModel filter)
        {
            var query = ApplyFilters(_context.KhoanVays.AsQueryable(), maNhanVien, filter);

            var result = await query
                .GroupBy(k => k.TrangThaiKhoanVay)
                .Select(g => new LoanByStatusData
                {
                    Status = g.Key ?? "Không xác định",
                    Count = g.Count(),
                    TotalAmount = g.Sum(k => k.SoTienVay)
                })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            return result;
        }

        public async Task<List<MonthlyLoanData>> GetMonthlyLoanDataAsync(int maNhanVien, ReportFilterModel filter)
        {
            var result = new List<MonthlyLoanData>();
            var monthNames = new[] { "T1", "T2", "T3", "T4", "T5", "T6", "T7", "T8", "T9", "T10", "T11", "T12" };

            int year = filter.Year ?? DateTime.Now.Year;

            var data = await _context.KhoanVays
                .Where(k => k.NgayTao.HasValue && k.NgayTao.Value.Year == year && k.MaNhanVienTinDung == maNhanVien)
                .GroupBy(k => k.NgayTao!.Value.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    Count = g.Count(),
                    TotalAmount = g.Sum(k => k.SoTienVay),
                    // Chỉ tính giải ngân cho hồ sơ đã duyệt
                    DisbursedAmount = g.Where(k => k.TrangThaiKhoanVay == "Đã giải ngân" || 
                                                   k.TrangThaiKhoanVay == "Đang vay" || 
                                                   k.TrangThaiKhoanVay == "Đã thanh toán")
                                       .Sum(k => k.SoTienVay),
                    ApprovedCount = g.Count(k => k.TrangThaiKhoanVay == "Đã phê duyệt" || 
                                                 k.TrangThaiKhoanVay == "Đã giải ngân" ||
                                                 k.TrangThaiKhoanVay == "Đang vay" ||
                                                 k.TrangThaiKhoanVay == "Đã thanh toán"),
                    PendingCount = g.Count(k => k.TrangThaiKhoanVay == "Chờ duyệt" || 
                                               k.TrangThaiKhoanVay == "Đang xử lý" ||
                                               k.TrangThaiKhoanVay == "Chờ xét duyệt")
                })
                .ToListAsync();

            for (int i = 1; i <= 12; i++)
            {
                var monthData = data.FirstOrDefault(d => d.Month == i);
                result.Add(new MonthlyLoanData
                {
                    Month = i,
                    MonthName = monthNames[i - 1],
                    Count = monthData?.Count ?? 0,
                    TotalAmount = monthData?.TotalAmount ?? 0,
                    DisbursedAmount = monthData?.DisbursedAmount ?? 0,
                    ApprovedCount = monthData?.ApprovedCount ?? 0,
                    PendingCount = monthData?.PendingCount ?? 0
                });
            }

            return result;
        }

        public async Task<List<QuarterlyLoanData>> GetQuarterlyLoanDataAsync(int maNhanVien, ReportFilterModel filter)
        {
            var result = new List<QuarterlyLoanData>();
            int year = filter.Year ?? DateTime.Now.Year;

            // Xác định quý cần lấy dữ liệu
            int startQuarter = 1;
            int endQuarter = 4;

            if (filter.Quarter.HasValue)
            {
                startQuarter = filter.Quarter.Value;
                endQuarter = filter.Quarter.Value;
            }

            for (int q = startQuarter; q <= endQuarter; q++)
            {
                var startMonth = (q - 1) * 3 + 1;
                var endMonth = startMonth + 2;

                var data = await _context.KhoanVays
                    .Where(k => k.NgayTao.HasValue && 
                                k.NgayTao.Value.Year == year &&
                                k.NgayTao.Value.Month >= startMonth &&
                                k.NgayTao.Value.Month <= endMonth &&
                                k.MaNhanVienTinDung == maNhanVien)
                    .ToListAsync();

                result.Add(new QuarterlyLoanData
                {
                    Quarter = q,
                    QuarterName = $"Quý {q}",
                    Count = data.Count,
                    TotalAmount = data.Sum(k => k.SoTienVay),
                    // Chỉ tính giải ngân cho hồ sơ đã duyệt
                    DisbursedAmount = data.Where(k => k.TrangThaiKhoanVay == "Đã giải ngân" || 
                                                      k.TrangThaiKhoanVay == "Đang vay" || 
                                                      k.TrangThaiKhoanVay == "Đã thanh toán")
                                         .Sum(k => k.SoTienVay),
                    ApprovedCount = data.Count(k => k.TrangThaiKhoanVay == "Đã phê duyệt" || 
                                                    k.TrangThaiKhoanVay == "Đã giải ngân" ||
                                                    k.TrangThaiKhoanVay == "Đang vay" ||
                                                    k.TrangThaiKhoanVay == "Đã thanh toán"),
                    RejectedCount = data.Count(k => k.TrangThaiKhoanVay == "Từ chối"),
                    PendingCount = data.Count(k => k.TrangThaiKhoanVay == "Chờ duyệt" || 
                                                   k.TrangThaiKhoanVay == "Đang xử lý" ||
                                                   k.TrangThaiKhoanVay == "Chờ xét duyệt")
                });
            }

            return result;
        }

        public async Task<List<RiskLevelData>> GetRiskLevelDataAsync(int maNhanVien, ReportFilterModel filter)
        {
            var query = ApplyFilters(_context.KhoanVays.AsQueryable(), maNhanVien, filter);

            var data = await query.ToListAsync();
            
            var result = new List<RiskLevelData>
            {
                new RiskLevelData
                {
                    RiskLevel = "Thấp",
                    Count = data.Count(k => k.SoNgayQuaHan == null || k.SoNgayQuaHan <= 0),
                    TotalAmount = data.Where(k => k.SoNgayQuaHan == null || k.SoNgayQuaHan <= 0).Sum(k => k.SoTienVay)
                },
                new RiskLevelData
                {
                    RiskLevel = "Trung bình",
                    Count = data.Count(k => k.SoNgayQuaHan > 0 && k.SoNgayQuaHan <= 30),
                    TotalAmount = data.Where(k => k.SoNgayQuaHan > 0 && k.SoNgayQuaHan <= 30).Sum(k => k.SoTienVay)
                },
                new RiskLevelData
                {
                    RiskLevel = "Cao",
                    Count = data.Count(k => k.SoNgayQuaHan > 30 && k.SoNgayQuaHan <= 90),
                    TotalAmount = data.Where(k => k.SoNgayQuaHan > 30 && k.SoNgayQuaHan <= 90).Sum(k => k.SoTienVay)
                },
                new RiskLevelData
                {
                    RiskLevel = "Rất cao",
                    Count = data.Count(k => k.SoNgayQuaHan > 90),
                    TotalAmount = data.Where(k => k.SoNgayQuaHan > 90).Sum(k => k.SoTienVay)
                }
            };

            return result.Where(r => r.Count > 0).ToList();
        }

        public async Task<List<CustomerTypeData>> GetCustomerTypeDataAsync(int maNhanVien, ReportFilterModel filter)
        {
            var query = ApplyFilters(_context.KhoanVays.AsQueryable(), maNhanVien, filter);

            var result = await query
                .GroupBy(k => k.LoaiKhachHang)
                .Select(g => new CustomerTypeData
                {
                    CustomerType = g.Key == "CaNhan" ? "Cá nhân" : "Doanh nghiệp",
                    Count = g.Count(),
                    TotalAmount = g.Sum(k => k.SoTienVay)
                })
                .ToListAsync();

            return result;
        }

        public async Task<TrendData> GetTrendDataAsync(int maNhanVien, int year)
        {
            var currentYearData = await _context.KhoanVays
                .Where(k => k.NgayTao.HasValue && k.NgayTao.Value.Year == year && k.MaNhanVienTinDung == maNhanVien)
                .ToListAsync();

            var previousYearData = await _context.KhoanVays
                .Where(k => k.NgayTao.HasValue && k.NgayTao.Value.Year == year - 1 && k.MaNhanVienTinDung == maNhanVien)
                .ToListAsync();

            var result = new TrendData
            {
                CurrentYearLoans = currentYearData.Count,
                PreviousYearLoans = previousYearData.Count,
                CurrentYearAmount = currentYearData.Sum(k => k.SoTienVay),
                PreviousYearAmount = previousYearData.Sum(k => k.SoTienVay)
            };

            if (result.PreviousYearLoans > 0)
            {
                result.LoanGrowthRate = Math.Round(
                    ((decimal)(result.CurrentYearLoans - result.PreviousYearLoans) / result.PreviousYearLoans) * 100, 1);
            }

            if (result.PreviousYearAmount > 0)
            {
                result.AmountGrowthRate = Math.Round(
                    ((result.CurrentYearAmount - result.PreviousYearAmount) / result.PreviousYearAmount) * 100, 1);
            }

            return result;
        }
        
        // ===== NEW METHODS FOR ADDITIONAL CHARTS =====
        
        public async Task<List<PaymentStatusData>> GetPaymentStatusDataAsync(int maNhanVien, ReportFilterModel filter)
        {
            var query = ApplyFilters(_context.KhoanVays.AsQueryable(), maNhanVien, filter);
            
            // Chỉ lấy các khoản vay đã giải ngân
            var disbursedLoans = await query
                .Where(k => k.TrangThaiKhoanVay == "Đã giải ngân" || 
                           k.TrangThaiKhoanVay == "Đang vay" || 
                           k.TrangThaiKhoanVay == "Đã thanh toán")
                .ToListAsync();
            
            var result = new List<PaymentStatusData>
            {
                new PaymentStatusData
                {
                    Status = "Đã thanh toán đầy đủ",
                    Count = disbursedLoans.Count(k => k.TrangThaiKhoanVay == "Đã thanh toán"),
                    TotalAmount = disbursedLoans.Where(k => k.TrangThaiKhoanVay == "Đã thanh toán").Sum(k => k.SoTienVay)
                },
                new PaymentStatusData
                {
                    Status = "Đang trả nợ đúng hạn",
                    Count = disbursedLoans.Count(k => (k.TrangThaiKhoanVay == "Đã giải ngân" || k.TrangThaiKhoanVay == "Đang vay") && (k.SoNgayQuaHan == null || k.SoNgayQuaHan <= 0)),
                    TotalAmount = disbursedLoans.Where(k => (k.TrangThaiKhoanVay == "Đã giải ngân" || k.TrangThaiKhoanVay == "Đang vay") && (k.SoNgayQuaHan == null || k.SoNgayQuaHan <= 0)).Sum(k => k.SoTienVay)
                },
                new PaymentStatusData
                {
                    Status = "Quá hạn 1-30 ngày",
                    Count = disbursedLoans.Count(k => k.SoNgayQuaHan > 0 && k.SoNgayQuaHan <= 30),
                    TotalAmount = disbursedLoans.Where(k => k.SoNgayQuaHan > 0 && k.SoNgayQuaHan <= 30).Sum(k => k.SoTienVay)
                },
                new PaymentStatusData
                {
                    Status = "Quá hạn 31-90 ngày",
                    Count = disbursedLoans.Count(k => k.SoNgayQuaHan > 30 && k.SoNgayQuaHan <= 90),
                    TotalAmount = disbursedLoans.Where(k => k.SoNgayQuaHan > 30 && k.SoNgayQuaHan <= 90).Sum(k => k.SoTienVay)
                },
                new PaymentStatusData
                {
                    Status = "Quá hạn trên 90 ngày",
                    Count = disbursedLoans.Count(k => k.SoNgayQuaHan > 90),
                    TotalAmount = disbursedLoans.Where(k => k.SoNgayQuaHan > 90).Sum(k => k.SoTienVay)
                }
            };
            
            return result.Where(r => r.Count > 0).ToList();
        }
        
        public async Task<List<DisbursementTrendData>> GetDisbursementTrendDataAsync(int maNhanVien, ReportFilterModel filter)
        {
            var result = new List<DisbursementTrendData>();
            int year = filter.Year ?? DateTime.Now.Year;
            
            for (int q = 1; q <= 4; q++)
            {
                var startMonth = (q - 1) * 3 + 1;
                var endMonth = startMonth + 2;
                
                var data = await _context.KhoanVays
                    .Where(k => k.NgayTao.HasValue && 
                                k.NgayTao.Value.Year == year &&
                                k.NgayTao.Value.Month >= startMonth &&
                                k.NgayTao.Value.Month <= endMonth &&
                                k.MaNhanVienTinDung == maNhanVien)
                    .ToListAsync();
                
                var proposedAmount = data.Sum(k => k.SoTienVay);
                var disbursedAmount = data.Where(k => k.TrangThaiKhoanVay == "Đã giải ngân" || 
                                                      k.TrangThaiKhoanVay == "Đang vay" || 
                                                      k.TrangThaiKhoanVay == "Đã thanh toán")
                                         .Sum(k => k.SoTienVay);
                var rejectedAmount = data.Where(k => k.TrangThaiKhoanVay == "Từ chối")
                                        .Sum(k => k.SoTienVay);
                
                result.Add(new DisbursementTrendData
                {
                    Quarter = q,
                    QuarterName = $"Quý {q}",
                    ProposedAmount = proposedAmount,
                    DisbursedAmount = disbursedAmount,
                    RejectedAmount = rejectedAmount,
                    DisbursementRate = proposedAmount > 0 ? Math.Round((disbursedAmount / proposedAmount) * 100, 1) : 0
                });
            }
            
            return result;
        }
        
        public async Task<CollectionRateData> GetCollectionRateDataAsync(int maNhanVien, ReportFilterModel filter)
        {
            var query = ApplyFilters(_context.KhoanVays.AsQueryable(), maNhanVien, filter);
            
            // Tổng các khoản vay đã giải ngân
            var disbursedLoans = await query
                .Where(k => k.TrangThaiKhoanVay == "Đã giải ngân" || 
                           k.TrangThaiKhoanVay == "Đang vay" || 
                           k.TrangThaiKhoanVay == "Đã thanh toán")
                .ToListAsync();
            
            var totalDisbursed = disbursedLoans.Sum(k => k.SoTienVay);
            var fullyPaid = disbursedLoans.Where(k => k.TrangThaiKhoanVay == "Đã thanh toán").Sum(k => k.SoTienVay);
            var onTimePayment = disbursedLoans.Where(k => k.SoNgayQuaHan == null || k.SoNgayQuaHan <= 0).Sum(k => k.SoTienVay);
            var overduePayment = disbursedLoans.Where(k => k.SoNgayQuaHan > 0).Sum(k => k.SoTienVay);
            
            // Lấy tổng tiền đã trả từ lịch sử trả nợ
            var loanIds = disbursedLoans.Select(k => k.MaKhoanVay).ToList();
            var totalPaid = await _context.LichSuTraNos
                .Where(l => loanIds.Contains(l.MaKhoanVay) && l.TrangThai == "Đã thanh toán")
                .SumAsync(l => l.TongDaTra ?? 0);
            
            return new CollectionRateData
            {
                TotalDisbursedAmount = totalDisbursed,
                TotalCollectedAmount = totalPaid,
                FullyPaidAmount = fullyPaid,
                OnTimeAmount = onTimePayment,
                OverdueAmount = overduePayment,
                CollectionRate = totalDisbursed > 0 ? Math.Round((totalPaid / totalDisbursed) * 100, 1) : 0,
                OnTimeRate = totalDisbursed > 0 ? Math.Round((onTimePayment / totalDisbursed) * 100, 1) : 0
            };
        }
        
        public async Task<List<LoanAmountRangeData>> GetLoanAmountRangeDataAsync(int maNhanVien, ReportFilterModel filter)
        {
            var query = ApplyFilters(_context.KhoanVays.AsQueryable(), maNhanVien, filter);
            var loans = await query.ToListAsync();
            
            var ranges = new[]
            {
                ("Dưới 50 triệu", 0m, 50000000m),
                ("50-100 triệu", 50000000m, 100000000m),
                ("100-500 triệu", 100000000m, 500000000m),
                ("500 triệu - 1 tỷ", 500000000m, 1000000000m),
                ("1-5 tỷ", 1000000000m, 5000000000m),
                ("Trên 5 tỷ", 5000000000m, decimal.MaxValue)
            };
            
            var result = ranges.Select(r => new LoanAmountRangeData
            {
                RangeName = r.Item1,
                MinAmount = r.Item2,
                MaxAmount = r.Item3,
                Count = loans.Count(k => k.SoTienVay >= r.Item2 && k.SoTienVay < r.Item3),
                TotalAmount = loans.Where(k => k.SoTienVay >= r.Item2 && k.SoTienVay < r.Item3).Sum(k => k.SoTienVay),
                ApprovedCount = loans.Count(k => k.SoTienVay >= r.Item2 && k.SoTienVay < r.Item3 && 
                                              _approvedStatuses.Contains(k.TrangThaiKhoanVay))
            }).Where(r => r.Count > 0).ToList();
            
            return result;
        }
    }

    #region View Models / DTOs

    public class ReportFilterModel
    {
        public int? Year { get; set; }
        public int? Quarter { get; set; }
        public int? Month { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    public class ReportViewModel
    {
        // Tổng quan
        public int TotalLoans { get; set; }
        public decimal TotalLoanAmount { get; set; }
        public decimal TotalDisbursedAmount { get; set; }
        public decimal TotalOutstandingDebt { get; set; }
        
        // Hồ sơ chờ duyệt
        public int PendingLoans { get; set; }
        public decimal PendingAmount { get; set; }
        
        // Hồ sơ đã duyệt
        public int ApprovedLoans { get; set; }
        public decimal ApprovedAmount { get; set; }
        
        public int ActiveLoans { get; set; }
        public int OverdueLoans { get; set; }
        public decimal OverdueAmount { get; set; }
        public int CompletedLoans { get; set; }
        public int RejectedLoans { get; set; }
        public decimal ApprovalRate { get; set; }
        public decimal AverageInterestRate { get; set; }
        public int AverageTerm { get; set; }
        public int IndividualCustomers { get; set; }
        public int EnterpriseCustomers { get; set; }

        // Filters
        public int? SelectedYear { get; set; }
        public int? SelectedQuarter { get; set; }
        public int? SelectedMonth { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public List<int> AvailableYears { get; set; } = new List<int>();
    }

    public class LoanByTypeData
    {
        public string LoanTypeName { get; set; } = "";
        public int Count { get; set; }
        public decimal TotalAmount { get; set; }
        public int ApprovedCount { get; set; }
        public decimal ApprovedAmount { get; set; }
    }

    public class LoanByStatusData
    {
        public string Status { get; set; } = "";
        public int Count { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class MonthlyLoanData
    {
        public int Month { get; set; }
        public string MonthName { get; set; } = "";
        public int Count { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DisbursedAmount { get; set; }
        public int ApprovedCount { get; set; }
        public int PendingCount { get; set; }
    }

    public class QuarterlyLoanData
    {
        public int Quarter { get; set; }
        public string QuarterName { get; set; } = "";
        public int Count { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DisbursedAmount { get; set; }
        public int ApprovedCount { get; set; }
        public int RejectedCount { get; set; }
        public int PendingCount { get; set; }
    }

    public class RiskLevelData
    {
        public string RiskLevel { get; set; } = "";
        public int Count { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class CustomerTypeData
    {
        public string CustomerType { get; set; } = "";
        public int Count { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class TrendData
    {
        public int CurrentYearLoans { get; set; }
        public int PreviousYearLoans { get; set; }
        public decimal CurrentYearAmount { get; set; }
        public decimal PreviousYearAmount { get; set; }
        public decimal LoanGrowthRate { get; set; }
        public decimal AmountGrowthRate { get; set; }
    }
    
    // ===== NEW DTOs FOR ADDITIONAL CHARTS =====
    
    public class PaymentStatusData
    {
        public string Status { get; set; } = "";
        public int Count { get; set; }
        public decimal TotalAmount { get; set; }
    }
    
    public class DisbursementTrendData
    {
        public int Quarter { get; set; }
        public string QuarterName { get; set; } = "";
        public decimal ProposedAmount { get; set; }
        public decimal DisbursedAmount { get; set; }
        public decimal RejectedAmount { get; set; }
        public decimal DisbursementRate { get; set; }
    }
    
    public class CollectionRateData
    {
        public decimal TotalDisbursedAmount { get; set; }
        public decimal TotalCollectedAmount { get; set; }
        public decimal FullyPaidAmount { get; set; }
        public decimal OnTimeAmount { get; set; }
        public decimal OverdueAmount { get; set; }
        public decimal CollectionRate { get; set; }
        public decimal OnTimeRate { get; set; }
    }
    
    public class LoanAmountRangeData
    {
        public string RangeName { get; set; } = "";
        public decimal MinAmount { get; set; }
        public decimal MaxAmount { get; set; }
        public int Count { get; set; }
        public decimal TotalAmount { get; set; }
        public int ApprovedCount { get; set; }
    }

    #endregion
}
