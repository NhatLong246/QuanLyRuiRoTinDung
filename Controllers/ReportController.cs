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
                
                // New chart data
                ViewBag.PaymentStatusData = await _reportService.GetPaymentStatusDataAsync(maNhanVien, filter);
                ViewBag.DisbursementTrendData = await _reportService.GetDisbursementTrendDataAsync(maNhanVien, filter);
                ViewBag.CollectionRateData = await _reportService.GetCollectionRateDataAsync(maNhanVien, filter);
                ViewBag.LoanAmountRangeData = await _reportService.GetLoanAmountRangeDataAsync(maNhanVien, filter);

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

        // Xuất báo cáo Excel
        [HttpGet]
        public async Task<IActionResult> Export(int? year, int? quarter, int? month, string? fromDate, string? toDate, string format = "excel")
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

                // Lấy tất cả dữ liệu báo cáo
                var data = await _reportService.GetReportDataAsync(maNhanVien, filter);
                var employeeName = await _reportService.GetEmployeeNameAsync(maNhanVien);
                var loansByType = await _reportService.GetLoansByTypeAsync(maNhanVien, filter);
                var loansByStatus = await _reportService.GetLoansByStatusAsync(maNhanVien, filter);
                var monthlyData = await _reportService.GetMonthlyLoanDataAsync(maNhanVien, filter);
                var quarterlyData = await _reportService.GetQuarterlyLoanDataAsync(maNhanVien, filter);
                var riskLevelData = await _reportService.GetRiskLevelDataAsync(maNhanVien, filter);
                var customerTypeData = await _reportService.GetCustomerTypeDataAsync(maNhanVien, filter);

                // Tạo nội dung Excel với nhiều sheet
                var reportContent = GenerateExcelContent(data, employeeName ?? "Nhân viên", filter, 
                    loansByType, loansByStatus, monthlyData, quarterlyData, riskLevelData, customerTypeData);

                // Đường dẫn thư mục xuất file
                var exportFolder = @"D:\HK1-nam3\fold";
                
                // Tạo thư mục nếu chưa tồn tại
                if (!Directory.Exists(exportFolder))
                {
                    Directory.CreateDirectory(exportFolder);
                }

                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var fileName = $"BaoCao_{employeeName?.Replace(" ", "_")}_{timestamp}.xls";
                var filePath = Path.Combine(exportFolder, fileName);
                
                // Ghi file Excel vào thư mục
                await System.IO.File.WriteAllTextAsync(filePath, reportContent, System.Text.Encoding.UTF8);

                _logger.LogInformation("Report exported successfully to: {FilePath}", filePath);

                // Trả về JSON với thông tin file đã xuất
                return Json(new { 
                    success = true, 
                    message = $"Đã xuất báo cáo thành công!",
                    filePath = filePath,
                    fileName = fileName
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting report");
                return Json(new { success = false, message = "Có lỗi xảy ra khi xuất báo cáo: " + ex.Message });
            }
        }

        private string GenerateExcelContent(ReportViewModel data, string employeeName, ReportFilterModel filter,
            List<LoanByTypeData> loansByType, List<LoanByStatusData> loansByStatus, 
            List<MonthlyLoanData> monthlyData, List<QuarterlyLoanData> quarterlyData,
            List<RiskLevelData> riskLevelData, List<CustomerTypeData> customerTypeData)
        {
            // Tạo thông tin filter cho tiêu đề
            var filterInfo = "";
            if (filter.FromDate.HasValue && filter.ToDate.HasValue)
            {
                filterInfo = $"Từ {filter.FromDate.Value:dd/MM/yyyy} đến {filter.ToDate.Value:dd/MM/yyyy}";
            }
            else if (filter.Month.HasValue)
            {
                filterInfo = $"Tháng {filter.Month}/{filter.Year}";
            }
            else if (filter.Quarter.HasValue)
            {
                filterInfo = $"Quý {filter.Quarter}/{filter.Year}";
            }
            else
            {
                filterInfo = $"Năm {filter.Year}";
            }

            var xml = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<?mso-application progid=""Excel.Sheet""?>
<Workbook xmlns=""urn:schemas-microsoft-com:office:spreadsheet""
 xmlns:o=""urn:schemas-microsoft-com:office:office""
 xmlns:x=""urn:schemas-microsoft-com:office:excel""
 xmlns:ss=""urn:schemas-microsoft-com:office:spreadsheet""
 xmlns:html=""http://www.w3.org/TR/REC-html40"">
 <DocumentProperties xmlns=""urn:schemas-microsoft-com:office:office"">
  <Author>{employeeName}</Author>
  <LastAuthor>{employeeName}</LastAuthor>
  <Created>{DateTime.Now:yyyy-MM-ddTHH:mm:ssZ}</Created>
  <Company>Bank CRM - Hệ thống tín dụng</Company>
 </DocumentProperties>
 <Styles>
  <Style ss:ID=""Default"" ss:Name=""Normal"">
   <Alignment ss:Vertical=""Center""/>
   <Font ss:FontName=""Arial"" ss:Size=""11""/>
  </Style>
  <Style ss:ID=""HeaderTitle"">
   <Alignment ss:Horizontal=""Center"" ss:Vertical=""Center""/>
   <Font ss:FontName=""Arial"" ss:Size=""18"" ss:Bold=""1"" ss:Color=""#4F46E5""/>
  </Style>
  <Style ss:ID=""SubTitle"">
   <Alignment ss:Horizontal=""Left"" ss:Vertical=""Center""/>
   <Font ss:FontName=""Arial"" ss:Size=""12"" ss:Color=""#374151""/>
  </Style>
  <Style ss:ID=""TableHeader"">
   <Alignment ss:Horizontal=""Center"" ss:Vertical=""Center""/>
   <Borders>
    <Border ss:Position=""Bottom"" ss:LineStyle=""Continuous"" ss:Weight=""2"" ss:Color=""#4F46E5""/>
    <Border ss:Position=""Left"" ss:LineStyle=""Continuous"" ss:Weight=""1"" ss:Color=""#E5E7EB""/>
    <Border ss:Position=""Right"" ss:LineStyle=""Continuous"" ss:Weight=""1"" ss:Color=""#E5E7EB""/>
    <Border ss:Position=""Top"" ss:LineStyle=""Continuous"" ss:Weight=""1"" ss:Color=""#E5E7EB""/>
   </Borders>
   <Font ss:FontName=""Arial"" ss:Size=""11"" ss:Bold=""1"" ss:Color=""#FFFFFF""/>
   <Interior ss:Color=""#4F46E5"" ss:Pattern=""Solid""/>
  </Style>
  <Style ss:ID=""TableCell"">
   <Alignment ss:Vertical=""Center""/>
   <Borders>
    <Border ss:Position=""Bottom"" ss:LineStyle=""Continuous"" ss:Weight=""1"" ss:Color=""#E5E7EB""/>
    <Border ss:Position=""Left"" ss:LineStyle=""Continuous"" ss:Weight=""1"" ss:Color=""#E5E7EB""/>
    <Border ss:Position=""Right"" ss:LineStyle=""Continuous"" ss:Weight=""1"" ss:Color=""#E5E7EB""/>
   </Borders>
   <Font ss:FontName=""Arial"" ss:Size=""10""/>
  </Style>
  <Style ss:ID=""TableCellAlt"">
   <Alignment ss:Vertical=""Center""/>
   <Borders>
    <Border ss:Position=""Bottom"" ss:LineStyle=""Continuous"" ss:Weight=""1"" ss:Color=""#E5E7EB""/>
    <Border ss:Position=""Left"" ss:LineStyle=""Continuous"" ss:Weight=""1"" ss:Color=""#E5E7EB""/>
    <Border ss:Position=""Right"" ss:LineStyle=""Continuous"" ss:Weight=""1"" ss:Color=""#E5E7EB""/>
   </Borders>
   <Font ss:FontName=""Arial"" ss:Size=""10""/>
   <Interior ss:Color=""#F9FAFB"" ss:Pattern=""Solid""/>
  </Style>
  <Style ss:ID=""NumberCell"">
   <Alignment ss:Horizontal=""Right"" ss:Vertical=""Center""/>
   <Borders>
    <Border ss:Position=""Bottom"" ss:LineStyle=""Continuous"" ss:Weight=""1"" ss:Color=""#E5E7EB""/>
    <Border ss:Position=""Left"" ss:LineStyle=""Continuous"" ss:Weight=""1"" ss:Color=""#E5E7EB""/>
    <Border ss:Position=""Right"" ss:LineStyle=""Continuous"" ss:Weight=""1"" ss:Color=""#E5E7EB""/>
   </Borders>
   <Font ss:FontName=""Arial"" ss:Size=""10""/>
   <NumberFormat ss:Format=""#,##0""/>
  </Style>
  <Style ss:ID=""CurrencyCell"">
   <Alignment ss:Horizontal=""Right"" ss:Vertical=""Center""/>
   <Borders>
    <Border ss:Position=""Bottom"" ss:LineStyle=""Continuous"" ss:Weight=""1"" ss:Color=""#E5E7EB""/>
    <Border ss:Position=""Left"" ss:LineStyle=""Continuous"" ss:Weight=""1"" ss:Color=""#E5E7EB""/>
    <Border ss:Position=""Right"" ss:LineStyle=""Continuous"" ss:Weight=""1"" ss:Color=""#E5E7EB""/>
   </Borders>
   <Font ss:FontName=""Arial"" ss:Size=""10""/>
   <NumberFormat ss:Format=""#,##0 &quot;VNĐ&quot;""/>
  </Style>
  <Style ss:ID=""PercentCell"">
   <Alignment ss:Horizontal=""Right"" ss:Vertical=""Center""/>
   <Borders>
    <Border ss:Position=""Bottom"" ss:LineStyle=""Continuous"" ss:Weight=""1"" ss:Color=""#E5E7EB""/>
    <Border ss:Position=""Left"" ss:LineStyle=""Continuous"" ss:Weight=""1"" ss:Color=""#E5E7EB""/>
    <Border ss:Position=""Right"" ss:LineStyle=""Continuous"" ss:Weight=""1"" ss:Color=""#E5E7EB""/>
   </Borders>
   <Font ss:FontName=""Arial"" ss:Size=""10""/>
   <NumberFormat ss:Format=""0.00%""/>
  </Style>
  <Style ss:ID=""SectionTitle"">
   <Alignment ss:Horizontal=""Left"" ss:Vertical=""Center""/>
   <Font ss:FontName=""Arial"" ss:Size=""14"" ss:Bold=""1"" ss:Color=""#4F46E5""/>
  </Style>
  <Style ss:ID=""HighlightGreen"">
   <Alignment ss:Horizontal=""Right"" ss:Vertical=""Center""/>
   <Borders>
    <Border ss:Position=""Bottom"" ss:LineStyle=""Continuous"" ss:Weight=""1"" ss:Color=""#E5E7EB""/>
    <Border ss:Position=""Left"" ss:LineStyle=""Continuous"" ss:Weight=""1"" ss:Color=""#E5E7EB""/>
    <Border ss:Position=""Right"" ss:LineStyle=""Continuous"" ss:Weight=""1"" ss:Color=""#E5E7EB""/>
   </Borders>
   <Font ss:FontName=""Arial"" ss:Size=""10"" ss:Bold=""1"" ss:Color=""#10B981""/>
  </Style>
  <Style ss:ID=""HighlightRed"">
   <Alignment ss:Horizontal=""Right"" ss:Vertical=""Center""/>
   <Borders>
    <Border ss:Position=""Bottom"" ss:LineStyle=""Continuous"" ss:Weight=""1"" ss:Color=""#E5E7EB""/>
    <Border ss:Position=""Left"" ss:LineStyle=""Continuous"" ss:Weight=""1"" ss:Color=""#E5E7EB""/>
    <Border ss:Position=""Right"" ss:LineStyle=""Continuous"" ss:Weight=""1"" ss:Color=""#E5E7EB""/>
   </Borders>
   <Font ss:FontName=""Arial"" ss:Size=""10"" ss:Bold=""1"" ss:Color=""#EF4444""/>
  </Style>
 </Styles>
 
 <!-- Sheet 1: Tổng quan -->
 <Worksheet ss:Name=""Tổng quan"">
  <Table ss:ExpandedColumnCount=""4"" ss:DefaultRowHeight=""20"">
   <Column ss:Width=""250""/>
   <Column ss:Width=""180""/>
   <Column ss:Width=""180""/>
   <Column ss:Width=""180""/>
   <Row ss:Height=""40"">
    <Cell ss:StyleID=""HeaderTitle"" ss:MergeAcross=""3""><Data ss:Type=""String"">📊 BÁO CÁO HOẠT ĐỘNG TÍN DỤNG CÁ NHÂN</Data></Cell>
   </Row>
   <Row ss:Height=""25"">
    <Cell ss:StyleID=""SubTitle""><Data ss:Type=""String"">Nhân viên: {employeeName}</Data></Cell>
   </Row>
   <Row ss:Height=""25"">
    <Cell ss:StyleID=""SubTitle""><Data ss:Type=""String"">Kỳ báo cáo: {filterInfo}</Data></Cell>
   </Row>
   <Row ss:Height=""25"">
    <Cell ss:StyleID=""SubTitle""><Data ss:Type=""String"">Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm:ss}</Data></Cell>
   </Row>
   <Row></Row>
   <Row ss:Height=""30"">
    <Cell ss:StyleID=""SectionTitle""><Data ss:Type=""String"">📋 THỐNG KÊ TỔNG QUAN</Data></Cell>
   </Row>
   <Row ss:Height=""25"">
    <Cell ss:StyleID=""TableHeader""><Data ss:Type=""String"">Chỉ tiêu</Data></Cell>
    <Cell ss:StyleID=""TableHeader""><Data ss:Type=""String"">Giá trị</Data></Cell>
    <Cell ss:StyleID=""TableHeader""><Data ss:Type=""String"">Đơn vị</Data></Cell>
    <Cell ss:StyleID=""TableHeader""><Data ss:Type=""String"">Ghi chú</Data></Cell>
   </Row>
   <Row ss:Height=""22"">
    <Cell ss:StyleID=""TableCell""><Data ss:Type=""String"">Tổng số hồ sơ</Data></Cell>
    <Cell ss:StyleID=""NumberCell""><Data ss:Type=""Number"">{data.TotalLoans}</Data></Cell>
    <Cell ss:StyleID=""TableCell""><Data ss:Type=""String"">Hồ sơ</Data></Cell>
    <Cell ss:StyleID=""TableCell""><Data ss:Type=""String"">Tất cả hồ sơ trong kỳ</Data></Cell>
   </Row>
   <Row ss:Height=""22"">
    <Cell ss:StyleID=""TableCellAlt""><Data ss:Type=""String"">Hồ sơ chờ duyệt</Data></Cell>
    <Cell ss:StyleID=""NumberCell""><Data ss:Type=""Number"">{data.PendingLoans}</Data></Cell>
    <Cell ss:StyleID=""TableCellAlt""><Data ss:Type=""String"">Hồ sơ</Data></Cell>
    <Cell ss:StyleID=""TableCellAlt""><Data ss:Type=""String"">Đang chờ xử lý</Data></Cell>
   </Row>
   <Row ss:Height=""22"">
    <Cell ss:StyleID=""TableCell""><Data ss:Type=""String"">Hồ sơ đã duyệt</Data></Cell>
    <Cell ss:StyleID=""HighlightGreen""><Data ss:Type=""Number"">{data.ApprovedLoans}</Data></Cell>
    <Cell ss:StyleID=""TableCell""><Data ss:Type=""String"">Hồ sơ</Data></Cell>
    <Cell ss:StyleID=""TableCell""><Data ss:Type=""String"">Đã phê duyệt thành công</Data></Cell>
   </Row>
   <Row ss:Height=""22"">
    <Cell ss:StyleID=""TableCellAlt""><Data ss:Type=""String"">Hồ sơ bị từ chối</Data></Cell>
    <Cell ss:StyleID=""HighlightRed""><Data ss:Type=""Number"">{data.TotalLoans - data.ApprovedLoans - data.PendingLoans}</Data></Cell>
    <Cell ss:StyleID=""TableCellAlt""><Data ss:Type=""String"">Hồ sơ</Data></Cell>
    <Cell ss:StyleID=""TableCellAlt""><Data ss:Type=""String"">Không đạt điều kiện</Data></Cell>
   </Row>
   <Row ss:Height=""22"">
    <Cell ss:StyleID=""TableCell""><Data ss:Type=""String"">Tỷ lệ phê duyệt</Data></Cell>
    <Cell ss:StyleID=""HighlightGreen""><Data ss:Type=""String"">{data.ApprovalRate}%</Data></Cell>
    <Cell ss:StyleID=""TableCell""><Data ss:Type=""String"">%</Data></Cell>
    <Cell ss:StyleID=""TableCell""><Data ss:Type=""String"">Đã duyệt / Tổng hồ sơ</Data></Cell>
   </Row>
   <Row ss:Height=""22"">
    <Cell ss:StyleID=""TableCellAlt""><Data ss:Type=""String"">Khoản vay quá hạn</Data></Cell>
    <Cell ss:StyleID=""HighlightRed""><Data ss:Type=""Number"">{data.OverdueLoans}</Data></Cell>
    <Cell ss:StyleID=""TableCellAlt""><Data ss:Type=""String"">Khoản</Data></Cell>
    <Cell ss:StyleID=""TableCellAlt""><Data ss:Type=""String"">Cần theo dõi</Data></Cell>
   </Row>
   <Row></Row>
   <Row ss:Height=""30"">
    <Cell ss:StyleID=""SectionTitle""><Data ss:Type=""String"">💰 THÔNG TIN TÀI CHÍNH</Data></Cell>
   </Row>
   <Row ss:Height=""25"">
    <Cell ss:StyleID=""TableHeader""><Data ss:Type=""String"">Chỉ tiêu</Data></Cell>
    <Cell ss:StyleID=""TableHeader""><Data ss:Type=""String"">Số tiền (VNĐ)</Data></Cell>
    <Cell ss:StyleID=""TableHeader""><Data ss:Type=""String"">Quy đổi</Data></Cell>
    <Cell ss:StyleID=""TableHeader""><Data ss:Type=""String"">Mô tả</Data></Cell>
   </Row>
   <Row ss:Height=""22"">
    <Cell ss:StyleID=""TableCell""><Data ss:Type=""String"">Tổng dư nợ hiện tại</Data></Cell>
    <Cell ss:StyleID=""CurrencyCell""><Data ss:Type=""Number"">{data.TotalOutstandingDebt}</Data></Cell>
    <Cell ss:StyleID=""TableCell""><Data ss:Type=""String"">{FormatCurrency(data.TotalOutstandingDebt)}</Data></Cell>
    <Cell ss:StyleID=""TableCell""><Data ss:Type=""String"">Hồ sơ đang vay</Data></Cell>
   </Row>
   <Row ss:Height=""22"">
    <Cell ss:StyleID=""TableCellAlt""><Data ss:Type=""String"">Tổng giá trị chờ duyệt</Data></Cell>
    <Cell ss:StyleID=""CurrencyCell""><Data ss:Type=""Number"">{data.PendingAmount}</Data></Cell>
    <Cell ss:StyleID=""TableCellAlt""><Data ss:Type=""String"">{FormatCurrency(data.PendingAmount)}</Data></Cell>
    <Cell ss:StyleID=""TableCellAlt""><Data ss:Type=""String"">Đang chờ phê duyệt</Data></Cell>
   </Row>
   <Row ss:Height=""22"">
    <Cell ss:StyleID=""TableCell""><Data ss:Type=""String"">Tổng giá trị đã duyệt</Data></Cell>
    <Cell ss:StyleID=""CurrencyCell""><Data ss:Type=""Number"">{data.ApprovedAmount}</Data></Cell>
    <Cell ss:StyleID=""TableCell""><Data ss:Type=""String"">{FormatCurrency(data.ApprovedAmount)}</Data></Cell>
    <Cell ss:StyleID=""TableCell""><Data ss:Type=""String"">Đã phê duyệt</Data></Cell>
   </Row>
   <Row ss:Height=""22"">
    <Cell ss:StyleID=""TableCellAlt""><Data ss:Type=""String"">Tổng giá trị hồ sơ</Data></Cell>
    <Cell ss:StyleID=""CurrencyCell""><Data ss:Type=""Number"">{data.TotalLoanAmount}</Data></Cell>
    <Cell ss:StyleID=""TableCellAlt""><Data ss:Type=""String"">{FormatCurrency(data.TotalLoanAmount)}</Data></Cell>
    <Cell ss:StyleID=""TableCellAlt""><Data ss:Type=""String"">Tất cả hồ sơ</Data></Cell>
   </Row>
   <Row></Row>
   <Row ss:Height=""30"">
    <Cell ss:StyleID=""SectionTitle""><Data ss:Type=""String"">📈 CHỈ SỐ HIỆU SUẤT</Data></Cell>
   </Row>
   <Row ss:Height=""25"">
    <Cell ss:StyleID=""TableHeader""><Data ss:Type=""String"">Chỉ số</Data></Cell>
    <Cell ss:StyleID=""TableHeader""><Data ss:Type=""String"">Giá trị</Data></Cell>
    <Cell ss:StyleID=""TableHeader""><Data ss:Type=""String"">Đánh giá</Data></Cell>
    <Cell ss:StyleID=""TableHeader""><Data ss:Type=""String"">Chi tiết</Data></Cell>
   </Row>
   <Row ss:Height=""22"">
    <Cell ss:StyleID=""TableCell""><Data ss:Type=""String"">Lãi suất trung bình</Data></Cell>
    <Cell ss:StyleID=""TableCell""><Data ss:Type=""String"">{data.AverageInterestRate:N2}%/năm</Data></Cell>
    <Cell ss:StyleID=""TableCell""><Data ss:Type=""String"">{(data.AverageInterestRate > 12 ? "Cao" : data.AverageInterestRate > 8 ? "Trung bình" : "Thấp")}</Data></Cell>
    <Cell ss:StyleID=""TableCell""><Data ss:Type=""String"">Hồ sơ đã duyệt</Data></Cell>
   </Row>
   <Row ss:Height=""22"">
    <Cell ss:StyleID=""TableCellAlt""><Data ss:Type=""String"">Kỳ hạn trung bình</Data></Cell>
    <Cell ss:StyleID=""TableCellAlt""><Data ss:Type=""String"">{data.AverageTerm} tháng</Data></Cell>
    <Cell ss:StyleID=""TableCellAlt""><Data ss:Type=""String"">{(data.AverageTerm > 24 ? "Dài hạn" : data.AverageTerm > 12 ? "Trung hạn" : "Ngắn hạn")}</Data></Cell>
    <Cell ss:StyleID=""TableCellAlt""><Data ss:Type=""String"">Hồ sơ đã duyệt</Data></Cell>
   </Row>
   <Row ss:Height=""22"">
    <Cell ss:StyleID=""TableCell""><Data ss:Type=""String"">Số khách hàng cá nhân</Data></Cell>
    <Cell ss:StyleID=""NumberCell""><Data ss:Type=""Number"">{data.IndividualCustomers}</Data></Cell>
    <Cell ss:StyleID=""TableCell""><Data ss:Type=""String"">-</Data></Cell>
    <Cell ss:StyleID=""TableCell""><Data ss:Type=""String"">Khách hàng cá nhân</Data></Cell>
   </Row>
   <Row ss:Height=""22"">
    <Cell ss:StyleID=""TableCellAlt""><Data ss:Type=""String"">Số khách hàng doanh nghiệp</Data></Cell>
    <Cell ss:StyleID=""NumberCell""><Data ss:Type=""Number"">{data.EnterpriseCustomers}</Data></Cell>
    <Cell ss:StyleID=""TableCellAlt""><Data ss:Type=""String"">-</Data></Cell>
    <Cell ss:StyleID=""TableCellAlt""><Data ss:Type=""String"">Khách hàng doanh nghiệp</Data></Cell>
   </Row>
  </Table>
 </Worksheet>
 
 <!-- Sheet 2: Theo loại vay -->
 <Worksheet ss:Name=""Theo loại vay"">
  <Table ss:ExpandedColumnCount=""5"" ss:DefaultRowHeight=""20"">
   <Column ss:Width=""200""/>
   <Column ss:Width=""100""/>
   <Column ss:Width=""150""/>
   <Column ss:Width=""100""/>
   <Column ss:Width=""150""/>
   <Row ss:Height=""35"">
    <Cell ss:StyleID=""SectionTitle"" ss:MergeAcross=""4""><Data ss:Type=""String"">📊 PHÂN TÍCH THEO LOẠI VAY</Data></Cell>
   </Row>
   <Row></Row>
   <Row ss:Height=""25"">
    <Cell ss:StyleID=""TableHeader""><Data ss:Type=""String"">Loại vay</Data></Cell>
    <Cell ss:StyleID=""TableHeader""><Data ss:Type=""String"">Số hồ sơ</Data></Cell>
    <Cell ss:StyleID=""TableHeader""><Data ss:Type=""String"">Tổng giá trị</Data></Cell>
    <Cell ss:StyleID=""TableHeader""><Data ss:Type=""String"">Đã duyệt</Data></Cell>
    <Cell ss:StyleID=""TableHeader""><Data ss:Type=""String"">Giá trị duyệt</Data></Cell>
   </Row>
   {GenerateLoansByTypeRows(loansByType)}
  </Table>
 </Worksheet>
 
 <!-- Sheet 3: Theo trạng thái -->
 <Worksheet ss:Name=""Theo trạng thái"">
  <Table ss:ExpandedColumnCount=""3"" ss:DefaultRowHeight=""20"">
   <Column ss:Width=""200""/>
   <Column ss:Width=""100""/>
   <Column ss:Width=""180""/>
   <Row ss:Height=""35"">
    <Cell ss:StyleID=""SectionTitle"" ss:MergeAcross=""2""><Data ss:Type=""String"">📋 PHÂN TÍCH THEO TRẠNG THÁI</Data></Cell>
   </Row>
   <Row></Row>
   <Row ss:Height=""25"">
    <Cell ss:StyleID=""TableHeader""><Data ss:Type=""String"">Trạng thái</Data></Cell>
    <Cell ss:StyleID=""TableHeader""><Data ss:Type=""String"">Số lượng</Data></Cell>
    <Cell ss:StyleID=""TableHeader""><Data ss:Type=""String"">Tổng giá trị</Data></Cell>
   </Row>
   {GenerateLoansByStatusRows(loansByStatus)}
  </Table>
 </Worksheet>
 
 <!-- Sheet 4: Theo tháng -->
 <Worksheet ss:Name=""Theo tháng"">
  <Table ss:ExpandedColumnCount=""6"" ss:DefaultRowHeight=""20"">
   <Column ss:Width=""80""/>
   <Column ss:Width=""100""/>
   <Column ss:Width=""150""/>
   <Column ss:Width=""150""/>
   <Column ss:Width=""100""/>
   <Column ss:Width=""100""/>
   <Row ss:Height=""35"">
    <Cell ss:StyleID=""SectionTitle"" ss:MergeAcross=""5""><Data ss:Type=""String"">📅 THỐNG KÊ THEO THÁNG - NĂM {filter.Year}</Data></Cell>
   </Row>
   <Row></Row>
   <Row ss:Height=""25"">
    <Cell ss:StyleID=""TableHeader""><Data ss:Type=""String"">Tháng</Data></Cell>
    <Cell ss:StyleID=""TableHeader""><Data ss:Type=""String"">Tổng hồ sơ</Data></Cell>
    <Cell ss:StyleID=""TableHeader""><Data ss:Type=""String"">Tổng giá trị</Data></Cell>
    <Cell ss:StyleID=""TableHeader""><Data ss:Type=""String"">Đã giải ngân</Data></Cell>
    <Cell ss:StyleID=""TableHeader""><Data ss:Type=""String"">Đã duyệt</Data></Cell>
    <Cell ss:StyleID=""TableHeader""><Data ss:Type=""String"">Chờ duyệt</Data></Cell>
   </Row>
   {GenerateMonthlyRows(monthlyData)}
  </Table>
 </Worksheet>
 
 <!-- Sheet 5: Theo quý -->
 <Worksheet ss:Name=""Theo quý"">
  <Table ss:ExpandedColumnCount=""5"" ss:DefaultRowHeight=""20"">
   <Column ss:Width=""100""/>
   <Column ss:Width=""100""/>
   <Column ss:Width=""150""/>
   <Column ss:Width=""150""/>
   <Column ss:Width=""100""/>
   <Row ss:Height=""35"">
    <Cell ss:StyleID=""SectionTitle"" ss:MergeAcross=""4""><Data ss:Type=""String"">📊 THỐNG KÊ THEO QUÝ - NĂM {filter.Year}</Data></Cell>
   </Row>
   <Row></Row>
   <Row ss:Height=""25"">
    <Cell ss:StyleID=""TableHeader""><Data ss:Type=""String"">Quý</Data></Cell>
    <Cell ss:StyleID=""TableHeader""><Data ss:Type=""String"">Số hồ sơ</Data></Cell>
    <Cell ss:StyleID=""TableHeader""><Data ss:Type=""String"">Tổng giá trị</Data></Cell>
    <Cell ss:StyleID=""TableHeader""><Data ss:Type=""String"">Đã giải ngân</Data></Cell>
    <Cell ss:StyleID=""TableHeader""><Data ss:Type=""String"">Đã duyệt</Data></Cell>
   </Row>
   {GenerateQuarterlyRows(quarterlyData)}
  </Table>
 </Worksheet>
 
 <!-- Sheet 6: Mức độ rủi ro -->
 <Worksheet ss:Name=""Mức độ rủi ro"">
  <Table ss:ExpandedColumnCount=""4"" ss:DefaultRowHeight=""20"">
   <Column ss:Width=""150""/>
   <Column ss:Width=""100""/>
   <Column ss:Width=""150""/>
   <Column ss:Width=""120""/>
   <Row ss:Height=""35"">
    <Cell ss:StyleID=""SectionTitle"" ss:MergeAcross=""3""><Data ss:Type=""String"">⚠️ PHÂN TÍCH MỨC ĐỘ RỦI RO</Data></Cell>
   </Row>
   <Row></Row>
   <Row ss:Height=""25"">
    <Cell ss:StyleID=""TableHeader""><Data ss:Type=""String"">Mức độ rủi ro</Data></Cell>
    <Cell ss:StyleID=""TableHeader""><Data ss:Type=""String"">Số hồ sơ</Data></Cell>
    <Cell ss:StyleID=""TableHeader""><Data ss:Type=""String"">Tổng giá trị</Data></Cell>
    <Cell ss:StyleID=""TableHeader""><Data ss:Type=""String"">Tỷ lệ</Data></Cell>
   </Row>
   {GenerateRiskLevelRows(riskLevelData, data.TotalLoans)}
  </Table>
 </Worksheet>
 
 <!-- Sheet 7: Loại khách hàng -->
 <Worksheet ss:Name=""Loại khách hàng"">
  <Table ss:ExpandedColumnCount=""3"" ss:DefaultRowHeight=""20"">
   <Column ss:Width=""180""/>
   <Column ss:Width=""100""/>
   <Column ss:Width=""150""/>
   <Row ss:Height=""35"">
    <Cell ss:StyleID=""SectionTitle"" ss:MergeAcross=""2""><Data ss:Type=""String"">👥 PHÂN TÍCH THEO LOẠI KHÁCH HÀNG</Data></Cell>
   </Row>
   <Row></Row>
   <Row ss:Height=""25"">
    <Cell ss:StyleID=""TableHeader""><Data ss:Type=""String"">Loại khách hàng</Data></Cell>
    <Cell ss:StyleID=""TableHeader""><Data ss:Type=""String"">Số hồ sơ</Data></Cell>
    <Cell ss:StyleID=""TableHeader""><Data ss:Type=""String"">Tổng giá trị</Data></Cell>
   </Row>
   {GenerateCustomerTypeRows(customerTypeData)}
  </Table>
 </Worksheet>
 
</Workbook>";

            return xml;
        }

        private string FormatCurrency(decimal amount)
        {
            if (amount >= 1000000000)
                return $"{amount / 1000000000:N2} tỷ";
            else if (amount >= 1000000)
                return $"{amount / 1000000:N1} triệu";
            else if (amount >= 1000)
                return $"{amount / 1000:N0} nghìn";
            else
                return $"{amount:N0} đ";
        }

        private string GenerateLoansByTypeRows(List<LoanByTypeData> data)
        {
            var rows = new System.Text.StringBuilder();
            bool alt = false;
            foreach (var item in data)
            {
                var style = alt ? "TableCellAlt" : "TableCell";
                rows.AppendLine($@"   <Row ss:Height=""22"">
    <Cell ss:StyleID=""{style}""><Data ss:Type=""String"">{item.LoanTypeName}</Data></Cell>
    <Cell ss:StyleID=""NumberCell""><Data ss:Type=""Number"">{item.Count}</Data></Cell>
    <Cell ss:StyleID=""{style}""><Data ss:Type=""String"">{FormatCurrency(item.TotalAmount)}</Data></Cell>
    <Cell ss:StyleID=""NumberCell""><Data ss:Type=""Number"">{item.ApprovedCount}</Data></Cell>
    <Cell ss:StyleID=""{style}""><Data ss:Type=""String"">{FormatCurrency(item.ApprovedAmount)}</Data></Cell>
   </Row>");
                alt = !alt;
            }
            return rows.ToString();
        }

        private string GenerateLoansByStatusRows(List<LoanByStatusData> data)
        {
            var rows = new System.Text.StringBuilder();
            bool alt = false;
            foreach (var item in data)
            {
                var style = alt ? "TableCellAlt" : "TableCell";
                rows.AppendLine($@"   <Row ss:Height=""22"">
    <Cell ss:StyleID=""{style}""><Data ss:Type=""String"">{item.Status}</Data></Cell>
    <Cell ss:StyleID=""NumberCell""><Data ss:Type=""Number"">{item.Count}</Data></Cell>
    <Cell ss:StyleID=""{style}""><Data ss:Type=""String"">{FormatCurrency(item.TotalAmount)}</Data></Cell>
   </Row>");
                alt = !alt;
            }
            return rows.ToString();
        }

        private string GenerateMonthlyRows(List<MonthlyLoanData> data)
        {
            var rows = new System.Text.StringBuilder();
            bool alt = false;
            foreach (var item in data)
            {
                var style = alt ? "TableCellAlt" : "TableCell";
                rows.AppendLine($@"   <Row ss:Height=""22"">
    <Cell ss:StyleID=""{style}""><Data ss:Type=""String"">Tháng {item.Month}</Data></Cell>
    <Cell ss:StyleID=""NumberCell""><Data ss:Type=""Number"">{item.Count}</Data></Cell>
    <Cell ss:StyleID=""{style}""><Data ss:Type=""String"">{FormatCurrency(item.TotalAmount)}</Data></Cell>
    <Cell ss:StyleID=""{style}""><Data ss:Type=""String"">{FormatCurrency(item.DisbursedAmount)}</Data></Cell>
    <Cell ss:StyleID=""NumberCell""><Data ss:Type=""Number"">{item.ApprovedCount}</Data></Cell>
    <Cell ss:StyleID=""NumberCell""><Data ss:Type=""Number"">{item.PendingCount}</Data></Cell>
   </Row>");
                alt = !alt;
            }
            return rows.ToString();
        }

        private string GenerateQuarterlyRows(List<QuarterlyLoanData> data)
        {
            var rows = new System.Text.StringBuilder();
            bool alt = false;
            foreach (var item in data)
            {
                var style = alt ? "TableCellAlt" : "TableCell";
                rows.AppendLine($@"   <Row ss:Height=""22"">
    <Cell ss:StyleID=""{style}""><Data ss:Type=""String"">{item.QuarterName}</Data></Cell>
    <Cell ss:StyleID=""NumberCell""><Data ss:Type=""Number"">{item.Count}</Data></Cell>
    <Cell ss:StyleID=""{style}""><Data ss:Type=""String"">{FormatCurrency(item.TotalAmount)}</Data></Cell>
    <Cell ss:StyleID=""{style}""><Data ss:Type=""String"">{FormatCurrency(item.DisbursedAmount)}</Data></Cell>
    <Cell ss:StyleID=""NumberCell""><Data ss:Type=""Number"">{item.ApprovedCount}</Data></Cell>
   </Row>");
                alt = !alt;
            }
            return rows.ToString();
        }

        private string GenerateRiskLevelRows(List<RiskLevelData> data, int totalLoans)
        {
            var rows = new System.Text.StringBuilder();
            bool alt = false;
            foreach (var item in data)
            {
                var style = alt ? "TableCellAlt" : "TableCell";
                var percentage = totalLoans > 0 ? (item.Count * 100.0 / totalLoans) : 0;
                rows.AppendLine($@"   <Row ss:Height=""22"">
    <Cell ss:StyleID=""{style}""><Data ss:Type=""String"">{item.RiskLevel}</Data></Cell>
    <Cell ss:StyleID=""NumberCell""><Data ss:Type=""Number"">{item.Count}</Data></Cell>
    <Cell ss:StyleID=""{style}""><Data ss:Type=""String"">{FormatCurrency(item.TotalAmount)}</Data></Cell>
    <Cell ss:StyleID=""{style}""><Data ss:Type=""String"">{percentage:N1}%</Data></Cell>
   </Row>");
                alt = !alt;
            }
            return rows.ToString();
        }

        private string GenerateCustomerTypeRows(List<CustomerTypeData> data)
        {
            var rows = new System.Text.StringBuilder();
            bool alt = false;
            foreach (var item in data)
            {
                var style = alt ? "TableCellAlt" : "TableCell";
                rows.AppendLine($@"   <Row ss:Height=""22"">
    <Cell ss:StyleID=""{style}""><Data ss:Type=""String"">{item.CustomerType}</Data></Cell>
    <Cell ss:StyleID=""NumberCell""><Data ss:Type=""Number"">{item.Count}</Data></Cell>
    <Cell ss:StyleID=""{style}""><Data ss:Type=""String"">{FormatCurrency(item.TotalAmount)}</Data></Cell>
   </Row>");
                alt = !alt;
            }
            return rows.ToString();
        }
    }
}
