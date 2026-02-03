using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyRuiRoTinDung.Models.EF;
using QuanLyRuiRoTinDung.Models.Entities;
using System.Security.Claims;

namespace QuanLyRuiRoTinDung.Controllers
{
    /// <summary>
    /// Enhanced version of LanhDaoController with improved data handling and business logic
    /// </summary>
    public partial class LanhDaoController : Controller
    {
        // Enhanced GetBadDebtStatistics with proper includes
        private async Task<dynamic> GetBadDebtStatisticsEnhanced()
        {
            var totalLoans = await _context.KhoanVays.CountAsync();
            var noXauLoans = await _context.TheoDoiNoXaus.CountAsync();
            var totalAmount = await _context.KhoanVays.SumAsync(k => k.SoTienVay);
            var badDebtAmount = await _context.TheoDoiNoXaus
                .SumAsync(n => n.SoDuNo ?? 0);

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
                BadDebtRatio = totalLoans > 0 ? Math.Round((decimal)noXauLoans / totalLoans * 100, 2) : 0,
                TotalLoanAmount = totalAmount,
                BadDebtAmount = badDebtAmount,
                BadDebtAmountRatio = totalAmount > 0 ? Math.Round(badDebtAmount / totalAmount * 100, 2) : 0,
                BadDebtByType = noXauByType,
                RecentBadDebts = recentBadDebts
            };
        }

        // Enhanced GetCreditOperationStatistics with better monthly aggregation
        private async Task<dynamic> GetCreditOperationStatisticsEnhanced()
        {
            var now = DateTime.Now;
            var lastYear = now.AddYears(-1);

            // Get monthly statistics for the last 12 months
            var monthlySummary = new List<dynamic>();
            
            for (int i = 11; i >= 0; i--)
            {
                var monthDate = now.AddMonths(-i);
                var submitted = await _context.KhoanVays
                    .CountAsync(k => k.NgayNopHoSo.HasValue && 
                                    k.NgayNopHoSo.Value.Year == monthDate.Year &&
                                    k.NgayNopHoSo.Value.Month == monthDate.Month);
                
                var approved = await _context.KhoanVays
                    .CountAsync(k => k.TrangThaiKhoanVay == "Đã phê duyệt" && 
                                    k.NgayPheDuyet.HasValue &&
                                    k.NgayPheDuyet.Value.Year == monthDate.Year && 
                                    k.NgayPheDuyet.Value.Month == monthDate.Month);

                var rejected = submitted - approved;

                monthlySummary.Add(new
                {
                    Month = monthDate.ToString("MM/yyyy"),
                    Submitted = submitted,
                    Approved = approved,
                    Rejected = rejected > 0 ? rejected : 0,
                    ApprovalRate = submitted > 0 ? Math.Round((decimal)approved / submitted * 100, 2) : 0
                });
            }

            // Current month statistics
            var thisMonthSubmitted = await _context.KhoanVays
                .CountAsync(k => k.NgayNopHoSo.HasValue && 
                                k.NgayNopHoSo.Value.Year == now.Year &&
                                k.NgayNopHoSo.Value.Month == now.Month);
            
            var thisMonthApproved = await _context.KhoanVays
                .CountAsync(k => k.TrangThaiKhoanVay == "Đã phê duyệt" && 
                                k.NgayPheDuyet.HasValue &&
                                k.NgayPheDuyet.Value.Year == now.Year && 
                                k.NgayPheDuyet.Value.Month == now.Month);

            var averageLoanAmount = await _context.KhoanVays
                .Where(k => k.NgayNopHoSo.HasValue && k.NgayNopHoSo.Value >= lastYear)
                .AverageAsync(k => (decimal?)k.SoTienVay) ?? 0;

            return new
            {
                LoansThisMonth = thisMonthSubmitted,
                ApprovedThisMonth = thisMonthApproved,
                ApprovalRate = thisMonthSubmitted > 0 ? Math.Round((decimal)thisMonthApproved / thisMonthSubmitted * 100, 2) : 0,
                AverageLoanAmount = (long)averageLoanAmount,
                MonthlySummary = monthlySummary
            };
        }

        // Enhanced loan approval with proper status tracking
        public async Task<IActionResult> PheDuyetKhoanVayEnhanced(int id)
        {
            var khoanVay = await _context.KhoanVays
                .Include(k => k.MaNhanVienTinDungNavigation)
                .Include(k => k.DanhGiaRuiRos)
                    .ThenInclude(d => d.ChiTietDanhGiaRuiRos)
                .Include(k => k.KhoanVayTaiSans)
                    .ThenInclude(kt => kt.MaTaiSanNavigation)
                .FirstOrDefaultAsync(k => k.MaKhoanVay == id);

            if (khoanVay == null)
            {
                return NotFound();
            }

            return View("ChiTietKhoanVay", khoanVay);
        }

        // Process loan approval with audit trail
        [HttpPost]
        public async Task<IActionResult> PheDuyetKhoanVayProcess(int id, string decision, string notes = "")
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
                    
                    // Create history record
                    var history = new LichSuTrangThaiKhoanVay
                    {
                        MaKhoanVay = id,
                        TrangThaiCu = "Chờ phê duyệt",
                        TrangThaiMoi = "Đã phê duyệt",
                        NgayThayDoi = now,
                        NguoiThayDoi = userId,
                        NhanXet = notes
                    };
                    _context.LichSuTrangThaiKhoanVays.Add(history);

                    TempData["Success"] = "Phê duyệt khoản vay thành công";
                }
                else if (decision == "reject")
                {
                    khoanVay.TrangThaiKhoanVay = "Từ chối";
                    khoanVay.NgayPheDuyet = now;
                    khoanVay.NguoiPheDuyet = userId;
                    khoanVay.LyDoTuChoi = notes;

                    var history = new LichSuTrangThaiKhoanVay
                    {
                        MaKhoanVay = id,
                        TrangThaiCu = "Chờ phê duyệt",
                        TrangThaiMoi = "Từ chối",
                        NgayThayDoi = now,
                        NguoiThayDoi = userId,
                        NhanXet = notes
                    };
                    _context.LichSuTrangThaiKhoanVays.Add(history);

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

        // Enhanced bad debt tracking with detailed status
        public async Task<IActionResult> TheoDoiNoXauEnhanced(int? pageNumber = 1, int pageSize = 20)
        {
            var query = _context.TheoDoiNoXaus
                .Include(n => n.MaKhoanVayNavigation)
                .Include(n => n.MaPhanLoaiNavigation)
                .AsQueryable();

            var totalRecords = await query.CountAsync();
            var pageNumber_safe = pageNumber ?? 1;

            var noXauList = await query
                .OrderByDescending(n => n.NgayPhanLoai)
                .Skip((pageNumber_safe - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.PageNumber = pageNumber_safe;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalRecords = totalRecords;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            return View("TheoDoiNoXau", noXauList);
        }

        // Update bad debt status with proper tracking
        [HttpPost]
        public async Task<IActionResult> UpdateBadDebtStatus(int id, string newStatus, string notes = "")
        {
            try
            {
                var noXau = await _context.TheoDoiNoXaus.FindAsync(id);
                if (noXau == null)
                {
                    return NotFound();
                }

                var oldStatus = noXau.TrangThai;
                noXau.TrangThai = newStatus;
                noXau.NguoiCapNhat = GetCurrentUserId();
                noXau.NgayCapNhat = DateTime.Now;

                _context.TheoDoiNoXaus.Update(noXau);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Cập nhật trạng thái nợ xấu từ '{oldStatus}' thành '{newStatus}' thành công";
                return RedirectToAction(nameof(TheoDoiNoXau));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating bad debt status");
                TempData["Error"] = "Có lỗi xảy ra khi cập nhật trạng thái";
                return RedirectToAction(nameof(TheoDoiNoXau));
            }
        }

        // Get bad debt breakdown by duration
        [HttpGet]
        public async Task<IActionResult> GetBadDebtBreakdown()
        {
            try
            {
                var breakdown = await _context.TheoDoiNoXaus
                    .GroupBy(n => n.SoNgayQuaHan.HasValue ? 
                        (n.SoNgayQuaHan <= 30 ? "0-30 ngày" : 
                         n.SoNgayQuaHan <= 90 ? "30-90 ngày" :
                         n.SoNgayQuaHan <= 180 ? "90-180 ngày" : "> 180 ngày") 
                        : "Chưa xác định")
                    .Select(g => new { Duration = g.Key, Count = g.Count() })
                    .ToListAsync();

                return Json(new
                {
                    labels = breakdown.Select(x => x.Duration),
                    datasets = new[] { new { 
                        label = "Số khoản nợ xấu",
                        data = breakdown.Select(x => x.Count),
                        backgroundColor = new[] { "#28a745", "#ffc107", "#fd7e14", "#dc3545" }
                    }}
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bad debt breakdown");
                return BadRequest("Lỗi khi lấy dữ liệu");
            }
        }

        // Get approval rate trends
        [HttpGet]
        public async Task<IActionResult> GetApprovalTrend(int months = 12)
        {
            try
            {
                var trends = new List<dynamic>();
                var now = DateTime.Now;

                for (int i = months - 1; i >= 0; i--)
                {
                    var monthDate = now.AddMonths(-i);
                    var submitted = await _context.KhoanVays
                        .CountAsync(k => k.NgayNopHoSo.HasValue && 
                                        k.NgayNopHoSo.Value.Year == monthDate.Year &&
                                        k.NgayNopHoSo.Value.Month == monthDate.Month);
                    
                    var approved = await _context.KhoanVays
                        .CountAsync(k => k.TrangThaiKhoanVay == "Đã phê duyệt" && 
                                        k.NgayPheDuyet.HasValue &&
                                        k.NgayPheDuyet.Value.Year == monthDate.Year && 
                                        k.NgayPheDuyet.Value.Month == monthDate.Month);

                    var rate = submitted > 0 ? Math.Round((decimal)approved / submitted * 100, 2) : 0;
                    
                    trends.Add(new
                    {
                        Month = monthDate.ToString("MM/yyyy"),
                        Rate = rate
                    });
                }

                return Json(new
                {
                    labels = trends.Select(x => x.Month),
                    datasets = new[] { new {
                        label = "Tỷ lệ phê duyệt (%)",
                        data = trends.Select(x => x.Rate),
                        borderColor = "#007bff",
                        backgroundColor = "rgba(0, 123, 255, 0.1)",
                        fill = true
                    }}
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting approval trend");
                return BadRequest("Lỗi khi lấy dữ liệu");
            }
        }

        // Helper to get current user ID from claims
        private int GetCurrentUserIdFromClaims()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdClaim?.Value, out var userId))
            {
                return userId;
            }
            return GetCurrentUserId(); // Fallback
        }
    }
}
