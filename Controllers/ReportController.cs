using Microsoft.AspNetCore.Mvc;
using QuanLyRuiRoTinDung.Services;
using System.Globalization;

namespace QuanLyRuiRoTinDung.Controllers
{
    public class ReportController : Controller
    {
        private readonly IReportService _reportService;
        private readonly ILogger<ReportController> _logger;

        public ReportController(IReportService reportService, ILogger<ReportController> logger)
        {
            _reportService = reportService;
            _logger = logger;
        }

        // GET: Report
        public async Task<IActionResult> Index(int? year, int? quarter, int? month, string? fromDate, string? toDate)
        {
            // Kiểm tra đăng nhập
            var maNguoiDungStr = HttpContext.Session.GetString("MaNguoiDung");
            if (string.IsNullOrEmpty(maNguoiDungStr) || !int.TryParse(maNguoiDungStr, out int maNhanVien))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                // Parse date range
                DateTime? parsedFromDate = null;
                DateTime? parsedToDate = null;

                if (!string.IsNullOrEmpty(fromDate))
                {
                    DateTime.TryParse(fromDate, out DateTime fd);
                    parsedFromDate = fd;
                }

                if (!string.IsNullOrEmpty(toDate))
                {
                    DateTime.TryParse(toDate, out DateTime td);
                    parsedToDate = td;
                }

                // Tạo filter model
                var filter = new ReportFilterModel
                {
                    Year = year ?? DateTime.Now.Year,
                    Quarter = quarter,
                    Month = month,
                    FromDate = parsedFromDate,
                    ToDate = parsedToDate
                };

                // Lấy tên nhân viên
                var employeeName = await _reportService.GetEmployeeNameAsync(maNhanVien);
                ViewBag.EmployeeName = employeeName;
                ViewBag.MaNhanVien = maNhanVien;

                // Lấy dữ liệu báo cáo CỦA NHÂN VIÊN ĐÓ
                var viewModel = await _reportService.GetReportDataAsync(maNhanVien, filter);
                
                // Lấy dữ liệu cho các biểu đồ
                ViewBag.LoansByType = await _reportService.GetLoansByTypeAsync(maNhanVien, filter);
                ViewBag.LoansByStatus = await _reportService.GetLoansByStatusAsync(maNhanVien, filter);
                ViewBag.MonthlyData = await _reportService.GetMonthlyLoanDataAsync(maNhanVien, filter);
                ViewBag.QuarterlyData = await _reportService.GetQuarterlyLoanDataAsync(maNhanVien, filter);
                ViewBag.RiskLevelData = await _reportService.GetRiskLevelDataAsync(maNhanVien, filter);
                ViewBag.CustomerTypeData = await _reportService.GetCustomerTypeDataAsync(maNhanVien, filter);
                ViewBag.TrendData = await _reportService.GetTrendDataAsync(maNhanVien, filter.Year ?? DateTime.Now.Year);
                ViewBag.SelectedYear = filter.Year;

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading report data");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải dữ liệu báo cáo.";
                return View(new ReportViewModel());
            }
        }

        // API endpoint để lấy dữ liệu biểu đồ theo AJAX
        [HttpGet]
        public async Task<IActionResult> GetChartData(int? year, int? quarter, int? month, string? fromDate, string? toDate, string chartType)
        {
            // Kiểm tra đăng nhập
            var maNguoiDungStr = HttpContext.Session.GetString("MaNguoiDung");
            if (string.IsNullOrEmpty(maNguoiDungStr) || !int.TryParse(maNguoiDungStr, out int maNhanVien))
            {
                return Unauthorized();
            }

            try
            {
                // Parse date range
                DateTime? parsedFromDate = null;
                DateTime? parsedToDate = null;

                if (!string.IsNullOrEmpty(fromDate))
                {
                    DateTime.TryParse(fromDate, out DateTime fd);
                    parsedFromDate = fd;
                }

                if (!string.IsNullOrEmpty(toDate))
                {
                    DateTime.TryParse(toDate, out DateTime td);
                    parsedToDate = td;
                }

                var filter = new ReportFilterModel
                {
                    Year = year ?? DateTime.Now.Year,
                    Quarter = quarter,
                    Month = month,
                    FromDate = parsedFromDate,
                    ToDate = parsedToDate
                };

                object? data = chartType switch
                {
                    "loansByType" => await _reportService.GetLoansByTypeAsync(maNhanVien, filter),
                    "loansByStatus" => await _reportService.GetLoansByStatusAsync(maNhanVien, filter),
                    "monthlyData" => await _reportService.GetMonthlyLoanDataAsync(maNhanVien, filter),
                    "quarterlyData" => await _reportService.GetQuarterlyLoanDataAsync(maNhanVien, filter),
                    "riskLevel" => await _reportService.GetRiskLevelDataAsync(maNhanVien, filter),
                    "customerType" => await _reportService.GetCustomerTypeDataAsync(maNhanVien, filter),
                    "trend" => await _reportService.GetTrendDataAsync(maNhanVien, filter.Year ?? DateTime.Now.Year),
                    "summary" => await _reportService.GetReportDataAsync(maNhanVien, filter),
                    _ => null
                };

                return Json(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting chart data");
                return BadRequest(new { error = "Có lỗi xảy ra khi tải dữ liệu biểu đồ." });
            }
        }

        // Xuất báo cáo
        [HttpGet]
        public async Task<IActionResult> Export(int? year, int? quarter, int? month, string? fromDate, string? toDate, string format = "pdf")
        {
            // Kiểm tra đăng nhập
            var maNguoiDungStr = HttpContext.Session.GetString("MaNguoiDung");
            if (string.IsNullOrEmpty(maNguoiDungStr) || !int.TryParse(maNguoiDungStr, out int maNhanVien))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                // Parse date range
                DateTime? parsedFromDate = null;
                DateTime? parsedToDate = null;

                if (!string.IsNullOrEmpty(fromDate))
                {
                    DateTime.TryParse(fromDate, out DateTime fd);
                    parsedFromDate = fd;
                }

                if (!string.IsNullOrEmpty(toDate))
                {
                    DateTime.TryParse(toDate, out DateTime td);
                    parsedToDate = td;
                }

                var filter = new ReportFilterModel
                {
                    Year = year ?? DateTime.Now.Year,
                    Quarter = quarter,
                    Month = month,
                    FromDate = parsedFromDate,
                    ToDate = parsedToDate
                };

                var data = await _reportService.GetReportDataAsync(maNhanVien, filter);
                var employeeName = await _reportService.GetEmployeeNameAsync(maNhanVien);

                // Tạo nội dung báo cáo
                var reportContent = GenerateReportContent(data, employeeName ?? "Nhân viên", format);

                if (format.ToLower() == "excel")
                {
                    return File(
                        System.Text.Encoding.UTF8.GetBytes(reportContent),
                        "application/vnd.ms-excel",
                        $"BaoCao_{DateTime.Now:yyyyMMdd_HHmmss}.xls"
                    );
                }
                else
                {
                    return Content(reportContent, "text/html");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting report");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xuất báo cáo.";
                return RedirectToAction(nameof(Index));
            }
        }

        private string GenerateReportContent(ReportViewModel data, string employeeName, string format)
        {
            var html = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Báo cáo cá nhân - {employeeName}</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 20px; }}
        h1 {{ color: #4F46E5; }}
        table {{ width: 100%; border-collapse: collapse; margin: 20px 0; }}
        th, td {{ border: 1px solid #ddd; padding: 12px; text-align: left; }}
        th {{ background-color: #4F46E5; color: white; }}
        tr:nth-child(even) {{ background-color: #f9f9f9; }}
        .summary {{ display: flex; gap: 20px; margin-bottom: 20px; }}
        .stat-box {{ background: #f5f5f5; padding: 15px; border-radius: 8px; flex: 1; }}
        .stat-value {{ font-size: 24px; font-weight: bold; color: #4F46E5; }}
        .stat-label {{ color: #666; }}
    </style>
</head>
<body>
    <h1>Báo cáo hoạt động tín dụng cá nhân</h1>
    <p><strong>Nhân viên:</strong> {employeeName}</p>
    <p><strong>Ngày xuất báo cáo:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</p>
    
    <h2>Tổng quan</h2>
    <table>
        <tr><th>Chỉ tiêu</th><th>Giá trị</th></tr>
        <tr><td>Tổng hồ sơ</td><td>{data.TotalLoans:N0}</td></tr>
        <tr><td>Hồ sơ chờ duyệt</td><td>{data.PendingLoans:N0}</td></tr>
        <tr><td>Hồ sơ đã duyệt</td><td>{data.ApprovedLoans:N0}</td></tr>
        <tr><td>Tổng dư nợ (đã duyệt)</td><td>{(data.TotalOutstandingDebt / 1000000):N0} triệu VND</td></tr>
        <tr><td>Tỷ lệ phê duyệt</td><td>{data.ApprovalRate}%</td></tr>
        <tr><td>Lãi suất TB (đã duyệt)</td><td>{data.AverageInterestRate:N2}%</td></tr>
        <tr><td>Kỳ hạn TB</td><td>{data.AverageTerm} tháng</td></tr>
        <tr><td>Khoản vay quá hạn</td><td>{data.OverdueLoans:N0}</td></tr>
    </table>
</body>
</html>";

            return html;
        }
    }
}
