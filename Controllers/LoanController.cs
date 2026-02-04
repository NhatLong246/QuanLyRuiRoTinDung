using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QuanLyRuiRoTinDung.Models.Entities;
using QuanLyRuiRoTinDung.Services;

namespace QuanLyRuiRoTinDung.Controllers
{
    public class LoanController : Controller
    {
        private readonly ILogger<LoanController> _logger;
        private readonly ILoanService _loanService;
        private readonly ICustomerService _customerService;
        private readonly ICicService _cicService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public LoanController(ILogger<LoanController> logger, ILoanService loanService, ICustomerService customerService, ICicService cicService, IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _loanService = loanService;
            _customerService = customerService;
            _cicService = cicService;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Loan/Index - Quản lý hồ sơ vay
        public async Task<IActionResult> Index(string tab = "my", string? trangThai = null, string? searchTerm = null)
        {
            var maNguoiDungStr = HttpContext.Session.GetString("MaNguoiDung");
            if (string.IsNullOrEmpty(maNguoiDungStr) || !int.TryParse(maNguoiDungStr, out int maNhanVien))
            {
                return RedirectToAction("Login", "Account");
            }

            // Lấy danh sách hồ sơ vay
            var myLoans = await _loanService.GetMyLoansAsync(maNhanVien, trangThai, searchTerm);
            var allLoans = await _loanService.GetAllLoansAsync(trangThai, searchTerm);

            // Lấy thống kê
            var myStats = await _loanService.GetLoanStatsAsync(maNhanVien);
            var allStats = await _loanService.GetLoanStatsAsync();

            // Lấy thông tin khách hàng cho từng hồ sơ vay
            var customerInfoDict = new Dictionary<int, (string TenKhachHang, string LoaiKhachHang, string? MaKhachHangCode, string? AnhDaiDien)>();
            
            var allLoansList = myLoans.Concat(allLoans).DistinctBy(l => l.MaKhoanVay).ToList();
            foreach (var loan in allLoansList)
            {
                if (!customerInfoDict.ContainsKey(loan.MaKhoanVay))
                {
                    string tenKH = "";
                    string? maKHCode = null;
                    string? anhDaiDien = null;
                    
                    if (loan.LoaiKhachHang == "CaNhan")
                    {
                        var kh = await _loanService.GetKhachHangCaNhanAsync(loan.MaKhachHang);
                        if (kh != null)
                        {
                            tenKH = kh.HoTen;
                            maKHCode = kh.MaKhachHangCode;
                            anhDaiDien = kh.AnhDaiDien;
                        }
                    }
                    else
                    {
                        var kh = await _loanService.GetKhachHangDoanhNghiepAsync(loan.MaKhachHang);
                        if (kh != null)
                        {
                            tenKH = kh.TenCongTy;
                            maKHCode = kh.MaKhachHangCode;
                            anhDaiDien = kh.AnhNguoiDaiDien;
                        }
                    }
                    
                    customerInfoDict[loan.MaKhoanVay] = (tenKH, loan.LoaiKhachHang, maKHCode, anhDaiDien);
                }
            }

            ViewBag.MyLoans = myLoans;
            ViewBag.AllLoans = allLoans;
            ViewBag.MyStats = myStats;
            ViewBag.AllStats = allStats;
            ViewBag.CustomerInfoDict = customerInfoDict;
            ViewBag.CurrentTab = tab;
            ViewBag.CurrentTrangThai = trangThai ?? "all";
            ViewBag.SearchTerm = searchTerm;
            ViewBag.MaNhanVien = maNhanVien;

            return View();
        }

        // GET: Loan/Manage - Quản lý khoản vay (các khoản vay đã duyệt, đang hoạt động)
        public async Task<IActionResult> Manage(string? trangThai = null, string? searchTerm = null)
        {
            var maNguoiDungStr = HttpContext.Session.GetString("MaNguoiDung");
            if (string.IsNullOrEmpty(maNguoiDungStr) || !int.TryParse(maNguoiDungStr, out int maNhanVien))
            {
                return RedirectToAction("Login", "Account");
            }

            // Lấy danh sách khoản vay đã duyệt (đang hoạt động, quá hạn, đã thanh toán...)
            var activeLoans = await _loanService.GetActiveLoansAsync(maNhanVien, trangThai, searchTerm);

            // Lấy thông tin khách hàng cho từng khoản vay
            var customerInfoDict = new Dictionary<int, (string TenKhachHang, string LoaiKhachHang, string? MaKhachHangCode, string? AnhDaiDien)>();
            
            foreach (var loan in activeLoans)
            {
                if (!customerInfoDict.ContainsKey(loan.MaKhoanVay))
                {
                    string tenKH = "";
                    string? maKHCode = null;
                    string? anhDaiDien = null;
                    
                    if (loan.LoaiKhachHang == "CaNhan")
                    {
                        var kh = await _loanService.GetKhachHangCaNhanAsync(loan.MaKhachHang);
                        if (kh != null)
                        {
                            tenKH = kh.HoTen;
                            maKHCode = kh.MaKhachHangCode;
                            anhDaiDien = kh.AnhDaiDien;
                        }
                    }
                    else
                    {
                        var kh = await _loanService.GetKhachHangDoanhNghiepAsync(loan.MaKhachHang);
                        if (kh != null)
                        {
                            tenKH = kh.TenCongTy;
                            maKHCode = kh.MaKhachHangCode;
                            anhDaiDien = kh.AnhNguoiDaiDien;
                        }
                    }
                    
                    customerInfoDict[loan.MaKhoanVay] = (tenKH, loan.LoaiKhachHang, maKHCode, anhDaiDien);
                }
            }

            ViewBag.ActiveLoans = activeLoans;
            ViewBag.CustomerInfoDict = customerInfoDict;
            ViewBag.CurrentTrangThai = trangThai ?? "all";
            ViewBag.SearchTerm = searchTerm;
            ViewBag.MaNhanVien = maNhanVien;

            return View();
        }

        // GET: Loan/SelectCustomer - Hiển thị danh sách khách hàng để chọn
        public async Task<IActionResult> SelectCustomer()
        {
            // Kiểm tra đăng nhập
            var maNguoiDungStr = HttpContext.Session.GetString("MaNguoiDung");
            if (string.IsNullOrEmpty(maNguoiDungStr))
            {
                return RedirectToAction("Login", "Account");
            }

            var viewModel = new SelectCustomerViewModel
            {
                KhachHangCaNhans = await _customerService.GetAllCaNhanAsync(),
                KhachHangDoanhNghieps = await _customerService.GetAllDoanhNghiepAsync()
            };

            return View(viewModel);
        }

        // GET: Loan/Create?customerId=1&customerType=CaNhan
        public async Task<IActionResult> Create(int customerId, string customerType)
        {
            // Kiểm tra đăng nhập
            var maNguoiDungStr = HttpContext.Session.GetString("MaNguoiDung");
            if (string.IsNullOrEmpty(maNguoiDungStr) || !int.TryParse(maNguoiDungStr, out int maNhanVien))
            {
                return RedirectToAction("Login", "Account");
            }

            if (customerId <= 0 || string.IsNullOrEmpty(customerType))
            {
                return RedirectToAction("SelectCustomer");
            }

            // Lấy thông tin khách hàng
            object? khachHang = null;
            string tenKhachHang = "";
            string? anhDaiDien = null;
            string? soCmndCccd = null;
            string? maSoThue = null;
            string? maKhachHangCode = null;
            string? soDienThoai = null;
            string? email = null;

            if (customerType == "CaNhan")
            {
                var kh = await _loanService.GetKhachHangCaNhanAsync(customerId);
                if (kh == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy khách hàng cá nhân.";
                    return RedirectToAction("SelectCustomer");
                }
                khachHang = kh;
                tenKhachHang = kh.HoTen;
                anhDaiDien = kh.AnhDaiDien;
                soCmndCccd = kh.SoCmnd;
                maKhachHangCode = kh.MaKhachHangCode;
                soDienThoai = kh.SoDienThoai;
                email = kh.Email;
            }
            else if (customerType == "DoanhNghiep")
            {
                var kh = await _loanService.GetKhachHangDoanhNghiepAsync(customerId);
                if (kh == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy khách hàng doanh nghiệp.";
                    return RedirectToAction("SelectCustomer");
                }
                khachHang = kh;
                tenKhachHang = kh.TenCongTy;
                anhDaiDien = kh.AnhNguoiDaiDien;
                // Doanh nghiệp không có SoCMND riêng, chỉ có MST
                maSoThue = kh.MaSoThue;
                maKhachHangCode = kh.MaKhachHangCode;
                soDienThoai = kh.SoDienThoai;
                email = kh.Email;
            }
            else
            {
                return RedirectToAction("SelectCustomer");
            }

            // Tra cứu CIC cho khách hàng
            ThongTinCic? cicInfo = null;
            if (!string.IsNullOrEmpty(soCmndCccd))
            {
                cicInfo = await _cicService.GetCicByCmndAsync(soCmndCccd);
            }
            else if (!string.IsNullOrEmpty(maSoThue))
            {
                cicInfo = await _cicService.GetCicByMstAsync(maSoThue);
            }

            // Lấy danh sách loại khoản vay theo loại khách hàng
            var loaiKhoanVays = await _loanService.GetLoaiKhoanVayByCustomerTypeAsync(customerType);
            ViewBag.LoaiKhoanVays = new SelectList(loaiKhoanVays, "MaLoaiVay", "TenLoaiVay");
            
            // Lưu thông tin loại vay để JavaScript sử dụng
            ViewBag.LoaiKhoanVaysData = loaiKhoanVays.Select(l => new { 
                MaLoaiVay = l.MaLoaiVay, 
                TenLoaiVay = l.TenLoaiVay,
                MaLoaiVayCode = l.MaLoaiVayCode,
                LaiSuatToiThieu = l.LaiSuatToiThieu,
                LaiSuatToiDa = l.LaiSuatToiDa
            }).ToList();

            // Lấy danh sách loại tài sản đảm bảo
            var loaiTaiSans = await _loanService.GetAllLoaiTaiSanAsync();
            ViewBag.LoaiTaiSans = loaiTaiSans;

            var khoanVay = new KhoanVay
            {
                LoaiKhachHang = customerType,
                MaKhachHang = customerId,
                MaNhanVienTinDung = maNhanVien
            };

            ViewBag.TenKhachHang = tenKhachHang;
            ViewBag.CustomerType = customerType;
            ViewBag.AnhDaiDien = anhDaiDien;
            ViewBag.KhachHang = khachHang;
            ViewBag.MaKhachHangCode = maKhachHangCode;
            ViewBag.SoDienThoai = soDienThoai;
            ViewBag.Email = email;
            ViewBag.CicInfo = cicInfo;

            return View(khoanVay);
        }

        // POST: Loan/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(KhoanVay khoanVay)
        {
            // Kiểm tra đăng nhập
            var maNguoiDungStr = HttpContext.Session.GetString("MaNguoiDung");
            if (string.IsNullOrEmpty(maNguoiDungStr) || !int.TryParse(maNguoiDungStr, out int nguoiTao))
            {
                return RedirectToAction("Login", "Account");
            }

            // Xử lý CoTaiSanDamBao từ form (checkbox trả về "true" hoặc không có giá trị)
            var coTaiSanValue = Request.Form["CoTaiSanDamBao"].ToString();
            khoanVay.CoTaiSanDamBao = coTaiSanValue == "true";

            // Xử lý SoTienTraHangThang từ hidden field (nếu có)
            var soTienTraHangThangStr = Request.Form["SoTienTraHangThang"].ToString();
            if (!string.IsNullOrEmpty(soTienTraHangThangStr) && decimal.TryParse(soTienTraHangThangStr.Replace(".", ""), out decimal soTienTraHangThang))
            {
                khoanVay.SoTienTraHangThang = soTienTraHangThang;
            }

            // Validate
            if (khoanVay.SoTienVay <= 0)
            {
                ModelState.AddModelError("SoTienVay", "Số tiền vay phải lớn hơn 0.");
            }

            if (khoanVay.LaiSuat < 0 || khoanVay.LaiSuat > 100)
            {
                ModelState.AddModelError("LaiSuat", "Lãi suất phải từ 0 đến 100%.");
            }

            if (khoanVay.KyHanVay <= 0)
            {
                ModelState.AddModelError("KyHanVay", "Kỳ hạn vay phải lớn hơn 0.");
            }

            // Kiểm tra loại khoản vay
            var loaiVay = await _loanService.GetLoaiKhoanVayAsync(khoanVay.MaLoaiVay);
            if (loaiVay == null)
            {
                ModelState.AddModelError("MaLoaiVay", "Loại khoản vay không hợp lệ.");
            }
            else
            {
                // Kiểm tra số tiền vay có vượt hạn mức không
                if (loaiVay.SoTienVayToiDa.HasValue && khoanVay.SoTienVay > loaiVay.SoTienVayToiDa.Value)
                {
                    ModelState.AddModelError("SoTienVay", $"Số tiền vay không được vượt quá hạn mức tối đa: {loaiVay.SoTienVayToiDa.Value:N0} VNĐ.");
                }

                // Kiểm tra kỳ hạn vay
                if (loaiVay.KyHanVayToiDa.HasValue && khoanVay.KyHanVay > loaiVay.KyHanVayToiDa.Value)
                {
                    ModelState.AddModelError("KyHanVay", $"Kỳ hạn vay không được vượt quá: {loaiVay.KyHanVayToiDa.Value} tháng.");
                }

                // Kiểm tra lãi suất
                if (loaiVay.LaiSuatToiThieu.HasValue && khoanVay.LaiSuat < loaiVay.LaiSuatToiThieu.Value)
                {
                    ModelState.AddModelError("LaiSuat", $"Lãi suất không được thấp hơn: {loaiVay.LaiSuatToiThieu.Value}%.");
                }

                if (loaiVay.LaiSuatToiDa.HasValue && khoanVay.LaiSuat > loaiVay.LaiSuatToiDa.Value)
                {
                    ModelState.AddModelError("LaiSuat", $"Lãi suất không được cao hơn: {loaiVay.LaiSuatToiDa.Value}%.");
                }
            }

            // Xử lý tài sản đảm bảo
            List<KhoanVayTaiSan>? khoanVayTaiSans = null;
            if (khoanVay.CoTaiSanDamBao == true)
            {
                // Lấy tất cả giá trị từ form (bao gồm cả giá trị rỗng)
                var formLoaiTaiSans = Request.Form["LoaiTaiSan"].ToArray();
                var tenTaiSanKhacs = Request.Form["TenTaiSanKhac"].ToArray();
                var donViTaiSans = Request.Form["DonViTaiSan"].ToArray();
                var soLuongTaiSans = Request.Form["SoLuongTaiSan"].ToArray();
                var giaTriDinhGias = Request.Form["GiaTriDinhGia"].ToArray();
                var tyLeTheChaps = Request.Form["TyLeTheChap"].ToArray();
                var ngayTheChaps = Request.Form["NgayTheChap"].ToArray();
                
                _logger.LogInformation("Create - LoaiTaiSans count: {Count}, values: {Values}", formLoaiTaiSans.Length, string.Join("|", formLoaiTaiSans));
                _logger.LogInformation("Create - TenTaiSanKhacs count: {Count}, values: {Values}", tenTaiSanKhacs.Length, string.Join("|", tenTaiSanKhacs));
                _logger.LogInformation("Create - DonViTaiSans count: {Count}, values: {Values}", donViTaiSans.Length, string.Join("|", donViTaiSans));
                _logger.LogInformation("Create - SoLuongTaiSans count: {Count}, values: {Values}", soLuongTaiSans.Length, string.Join("|", soLuongTaiSans));

                if (formLoaiTaiSans.Length > 0)
                {
                    khoanVayTaiSans = new List<KhoanVayTaiSan>();
                    for (int i = 0; i < formLoaiTaiSans.Length; i++)
                    {
                        var loaiTaiSan = formLoaiTaiSans.ElementAtOrDefault(i) ?? "";
                        if (string.IsNullOrWhiteSpace(loaiTaiSan)) continue;
                        
                        var tenTaiSanKhac = tenTaiSanKhacs.ElementAtOrDefault(i) ?? "";
                        var donViTaiSan = donViTaiSans.ElementAtOrDefault(i) ?? "";
                        var soLuongTaiSanStr = soLuongTaiSans.ElementAtOrDefault(i) ?? "";
                        var giaTriDinhGiaStr = giaTriDinhGias.ElementAtOrDefault(i)?.Replace(".", "") ?? "";
                        
                        _logger.LogInformation("Create - Asset {Index}: loaiTaiSan={LoaiTaiSan}, tenTaiSanKhac={TenTaiSanKhac}, donVi={DonVi}, soLuong={SoLuong}", 
                            i, loaiTaiSan, tenTaiSanKhac, donViTaiSan, soLuongTaiSanStr);
                        
                        decimal? giaTriDinhGia = null;
                        if (decimal.TryParse(giaTriDinhGiaStr, out decimal giaTri))
                        {
                            giaTriDinhGia = giaTri;
                        }
                        
                        decimal? soLuong = null;
                        if (!string.IsNullOrEmpty(soLuongTaiSanStr) && decimal.TryParse(soLuongTaiSanStr, out decimal sl))
                        {
                            soLuong = sl;
                        }
                        
                        // Lấy tên khách hàng để làm chủ sở hữu mặc định
                        string? chuSoHuu = null;
                        if (khoanVay.LoaiKhachHang == "CaNhan")
                        {
                            var khCN = await _loanService.GetKhachHangCaNhanAsync(khoanVay.MaKhachHang);
                            chuSoHuu = khCN?.HoTen;
                        }
                        else
                        {
                            var khDN = await _loanService.GetKhachHangDoanhNghiepAsync(khoanVay.MaKhachHang);
                            chuSoHuu = khDN?.TenCongTy;
                        }
                        
                        // Tạo tài sản đảm bảo MỚI cho khoản vay
                        TaiSanDamBao taiSan;
                        string? tenTaiSanKhacToSave = null;
                        string? donViToSave = null;
                        decimal? soLuongToSave = null;
                        
                        // Lấy MaLoaiTaiSan từ loại tài sản đã chọn
                        string tenLoaiTaiSan = loaiTaiSan switch
                        {
                            "Dat" => "Đất",
                            "Xe" => "Xe",
                            "Khac" => "Khác",
                            _ => "Khác"
                        };
                        var maLoaiTaiSan = await _loanService.GetOrCreateLoaiTaiSanAsync(tenLoaiTaiSan);
                        
                        if (loaiTaiSan == "Khac")
                        {
                            // Tài sản "Khác": lưu tất cả thông tin
                            taiSan = await _loanService.CreateTaiSanAsync(maLoaiTaiSan, tenTaiSanKhac, donViTaiSan, chuSoHuu, null, giaTriDinhGia, nguoiTao);
                            tenTaiSanKhacToSave = !string.IsNullOrEmpty(tenTaiSanKhac) ? tenTaiSanKhac : null;
                            donViToSave = !string.IsNullOrEmpty(donViTaiSan) ? donViTaiSan : null;
                            soLuongToSave = soLuong;
                        }
                        else
                        {
                            // Tài sản "Đất" hoặc "Xe"
                            taiSan = await _loanService.CreateTaiSanAsync(maLoaiTaiSan, tenLoaiTaiSan, null, chuSoHuu, null, giaTriDinhGia, nguoiTao);
                        }
                        
                        var khoanVayTaiSan = new KhoanVayTaiSan
                        {
                            MaTaiSan = taiSan.MaTaiSan,
                            GiaTriDinhGiaTaiThoiDiemVay = giaTriDinhGia,
                            TyLeTheChap = decimal.TryParse(tyLeTheChaps.ElementAtOrDefault(i)?.Replace(".", ""), out decimal tyLe) ? tyLe : null,
                            NgayTheChap = DateOnly.TryParse(ngayTheChaps.ElementAtOrDefault(i), out DateOnly ngay) ? ngay : DateOnly.FromDateTime(DateTime.Now),
                            TrangThai = "Đang thế chấp",
                            TenTaiSanKhac = tenTaiSanKhacToSave,
                            DonVi = donViToSave,
                            SoLuong = soLuongToSave
                        };
                        khoanVayTaiSans.Add(khoanVayTaiSan);
                    }
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var createdLoan = await _loanService.CreateLoanAsync(khoanVay, nguoiTao, khoanVayTaiSans);
                    
                    // Xử lý file upload nếu có
                    var filesToSave = new List<(IFormFile File, string LoaiFile)>();
                    
                    // Lấy files từ form theo loại
                    var phapLyFiles = Request.Form.Files.Where(f => f.Name == "PhapLyFiles").ToList();
                    var taiChinhFiles = Request.Form.Files.Where(f => f.Name == "TaiChinhFiles").ToList();
                    var taiSanFiles = Request.Form.Files.Where(f => f.Name == "TaiSanDamBaoFiles").ToList();
                    
                    foreach (var file in phapLyFiles)
                    {
                        if (file != null && file.Length > 0)
                            filesToSave.Add((file, "PhapLy"));
                    }
                    
                    foreach (var file in taiChinhFiles)
                    {
                        if (file != null && file.Length > 0)
                            filesToSave.Add((file, "TaiChinh"));
                    }
                    
                    foreach (var file in taiSanFiles)
                    {
                        if (file != null && file.Length > 0)
                            filesToSave.Add((file, "TaiSanDamBao"));
                    }
                    
                    if (filesToSave.Any())
                    {
                        var webRootPath = _webHostEnvironment.WebRootPath;
                        await _loanService.SaveLoanFilesAsync(createdLoan.MaKhoanVay, filesToSave, nguoiTao, webRootPath);
                    }
                    
                    TempData["SuccessMessage"] = $"Tạo hồ sơ vay thành công! Mã hồ sơ: {createdLoan.MaKhoanVayCode}";
                    return RedirectToAction("Index", "Dashboard");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating loan");
                    ModelState.AddModelError("", "Có lỗi xảy ra khi tạo hồ sơ vay. Vui lòng thử lại.");
                }
            }

            // Load lại dữ liệu cho view
            var loaiKhoanVays = await _loanService.GetAllLoaiKhoanVayAsync();
            ViewBag.LoaiKhoanVays = new SelectList(loaiKhoanVays, "MaLoaiVay", "TenLoaiVay");

            // Lấy danh sách loại tài sản đảm bảo
            var loaiTaiSans = await _loanService.GetAllLoaiTaiSanAsync();
            ViewBag.LoaiTaiSans = loaiTaiSans;

            // Lấy lại thông tin khách hàng và CIC
            object? khachHang = null;
            string tenKhachHang = "";
            string? anhDaiDien = null;
            string? soCmndCccd = null;
            string? maSoThue = null;
            string? maKhachHangCode = null;
            string? soDienThoai = null;
            string? email = null;

            if (khoanVay.LoaiKhachHang == "CaNhan")
            {
                var kh = await _loanService.GetKhachHangCaNhanAsync(khoanVay.MaKhachHang);
                if (kh != null)
                {
                    khachHang = kh;
                    tenKhachHang = kh.HoTen;
                    anhDaiDien = kh.AnhDaiDien;
                    soCmndCccd = kh.SoCmnd;
                    maKhachHangCode = kh.MaKhachHangCode;
                    soDienThoai = kh.SoDienThoai;
                    email = kh.Email;
                }
            }
            else
            {
                var kh = await _loanService.GetKhachHangDoanhNghiepAsync(khoanVay.MaKhachHang);
                if (kh != null)
                {
                    khachHang = kh;
                    tenKhachHang = kh.TenCongTy;
                    anhDaiDien = kh.AnhNguoiDaiDien;
                    maSoThue = kh.MaSoThue;
                    maKhachHangCode = kh.MaKhachHangCode;
                    soDienThoai = kh.SoDienThoai;
                    email = kh.Email;
                }
            }

            // Tra cứu CIC
            ThongTinCic? cicInfo = null;
            if (!string.IsNullOrEmpty(soCmndCccd))
            {
                cicInfo = await _cicService.GetCicByCmndAsync(soCmndCccd);
            }
            else if (!string.IsNullOrEmpty(maSoThue))
            {
                cicInfo = await _cicService.GetCicByMstAsync(maSoThue);
            }

            ViewBag.TenKhachHang = tenKhachHang;
            ViewBag.CustomerType = khoanVay.LoaiKhachHang;
            ViewBag.AnhDaiDien = anhDaiDien;
            ViewBag.KhachHang = khachHang;
            ViewBag.MaKhachHangCode = maKhachHangCode;
            ViewBag.SoDienThoai = soDienThoai;
            ViewBag.Email = email;
            ViewBag.CicInfo = cicInfo;

            return View(khoanVay);
        }

        // GET: Loan/LoanList?customerId=1&customerType=CaNhan
        public async Task<IActionResult> LoanList(int customerId, string customerType)
        {
            var maNguoiDungStr = HttpContext.Session.GetString("MaNguoiDung");
            if (string.IsNullOrEmpty(maNguoiDungStr) || !int.TryParse(maNguoiDungStr, out int maNhanVien))
            {
                return RedirectToAction("Login", "Account");
            }

            var loans = await _loanService.GetLoansByCustomerAsync(customerId, customerType, maNhanVien);
            
            // Lấy thông tin khách hàng
            object? khachHang = null;
            string tenKhachHang = "";
            if (customerType == "CaNhan")
            {
                khachHang = await _loanService.GetKhachHangCaNhanAsync(customerId);
                if (khachHang is KhachHangCaNhan khCN)
                {
                    tenKhachHang = khCN.HoTen ?? "";
                }
            }
            else
            {
                khachHang = await _loanService.GetKhachHangDoanhNghiepAsync(customerId);
                if (khachHang is KhachHangDoanhNghiep khDN)
                {
                    tenKhachHang = khDN.TenCongTy ?? "";
                }
            }

            ViewBag.CustomerId = customerId;
            ViewBag.CustomerType = customerType;
            ViewBag.TenKhachHang = tenKhachHang;
            ViewBag.KhachHang = khachHang;

            return View(loans);
        }

        // POST: Loan/SaveDraft
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveDraft(KhoanVay khoanVay)
        {
            var maNguoiDungStr = HttpContext.Session.GetString("MaNguoiDung");
            if (string.IsNullOrEmpty(maNguoiDungStr) || !int.TryParse(maNguoiDungStr, out int nguoiTao))
            {
                return Json(new { success = false, message = "Phiên đăng nhập đã hết hạn." });
            }

            try
            {
                // Lấy MaKhoanVay từ form để đảm bảo update đúng khi edit
                var maKhoanVayStr = Request.Form["MaKhoanVay"].ToString();
                if (!string.IsNullOrEmpty(maKhoanVayStr) && int.TryParse(maKhoanVayStr, out int maKhoanVayFromForm))
                {
                    khoanVay.MaKhoanVay = maKhoanVayFromForm;
                }
                
                // Xử lý CoTaiSanDamBao
                var coTaiSanValue = Request.Form["CoTaiSanDamBao"].ToString();
                khoanVay.CoTaiSanDamBao = coTaiSanValue == "true";

                // Xử lý SoTienVay - parse từ format có dấu chấm
                var soTienVayStr = Request.Form["SoTienVay"].ToString();
                if (!string.IsNullOrEmpty(soTienVayStr))
                {
                    var soTienVayClean = soTienVayStr.Replace(".", "").Replace(",", "");
                    if (decimal.TryParse(soTienVayClean, out decimal soTienVay))
                    {
                        khoanVay.SoTienVay = soTienVay;
                    }
                }

                // Xử lý SoTienTraHangThang
                var soTienTraHangThangStr = Request.Form["SoTienTraHangThang"].ToString();
                if (!string.IsNullOrEmpty(soTienTraHangThangStr))
                {
                    var soTienTraHangThangClean = soTienTraHangThangStr.Replace(".", "").Replace(",", "");
                    if (decimal.TryParse(soTienTraHangThangClean, out decimal soTienTraHangThang))
                    {
                        khoanVay.SoTienTraHangThang = soTienTraHangThang;
                    }
                }

                // Xử lý GhiChu (Ghi chú bổ sung về hồ sơ vay) - luôn cập nhật khi lưu nháp
                var ghiChuBoSung = Request.Form["GhiChu"].ToString();
                khoanVay.GhiChu = ghiChuBoSung;

                // Xử lý tài sản đảm bảo - luôn kiểm tra nếu có dữ liệu
                List<KhoanVayTaiSan>? khoanVayTaiSans = null;
                
                // Sử dụng ToArray() để lấy tất cả giá trị bao gồm cả giá trị rỗng
                var loaiTaiSans = Request.Form["LoaiTaiSan"].ToArray();
                var tenTaiSanKhacs = Request.Form["TenTaiSanKhac"].ToArray();
                var donViTaiSans = Request.Form["DonViTaiSan"].ToArray();
                var soLuongTaiSans = Request.Form["SoLuongTaiSan"].ToArray();
                var giaTriDinhGias = Request.Form["GiaTriDinhGia"].ToArray();
                var tyLeTheChaps = Request.Form["TyLeTheChap"].ToArray();
                var ngayTheChaps = Request.Form["NgayTheChap"].ToArray();
                
                _logger.LogInformation("SaveDraft - LoaiTaiSan count: {Count}, values: [{Values}]", loaiTaiSans.Length, string.Join(", ", loaiTaiSans));
                _logger.LogInformation("SaveDraft - TenTaiSanKhac count: {Count}, values: [{Values}]", tenTaiSanKhacs.Length, string.Join(", ", tenTaiSanKhacs));
                _logger.LogInformation("SaveDraft - DonViTaiSan count: {Count}, values: [{Values}]", donViTaiSans.Length, string.Join(", ", donViTaiSans));
                _logger.LogInformation("SaveDraft - SoLuongTaiSan count: {Count}, values: [{Values}]", soLuongTaiSans.Length, string.Join(", ", soLuongTaiSans));
                
                if (loaiTaiSans.Length > 0)
                {
                    khoanVayTaiSans = new List<KhoanVayTaiSan>();
                    for (int i = 0; i < loaiTaiSans.Length; i++)
                    {
                        var loaiTaiSan = loaiTaiSans.ElementAtOrDefault(i) ?? "";
                        if (string.IsNullOrWhiteSpace(loaiTaiSan))
                            continue;
                            
                        var tenTaiSanKhac = tenTaiSanKhacs.ElementAtOrDefault(i) ?? "";
                        var donViTaiSan = donViTaiSans.ElementAtOrDefault(i) ?? "";
                        var soLuongTaiSanStr = soLuongTaiSans.ElementAtOrDefault(i) ?? "";
                        var giaTriDinhGiaStr = giaTriDinhGias.ElementAtOrDefault(i)?.Replace(".", "").Replace(",", "") ?? "";
                        
                        _logger.LogInformation("SaveDraft - Processing asset {Index}: LoaiTaiSan='{LoaiTaiSan}', TenTaiSanKhac='{TenTaiSanKhac}', DonVi='{DonVi}', SoLuong='{SoLuong}'", 
                            i, loaiTaiSan, tenTaiSanKhac, donViTaiSan, soLuongTaiSanStr);
                        
                        decimal? giaTriDinhGia = null;
                        if (!string.IsNullOrEmpty(giaTriDinhGiaStr) && decimal.TryParse(giaTriDinhGiaStr, out decimal giaTri))
                        {
                            giaTriDinhGia = giaTri;
                        }
                        
                        decimal? soLuong = null;
                        if (!string.IsNullOrEmpty(soLuongTaiSanStr) && decimal.TryParse(soLuongTaiSanStr, out decimal sl))
                        {
                            soLuong = sl;
                        }
                        
                        // Lấy tên khách hàng để làm chủ sở hữu mặc định
                        string? chuSoHuu = null;
                        if (khoanVay.LoaiKhachHang == "CaNhan")
                        {
                            var khCN = await _loanService.GetKhachHangCaNhanAsync(khoanVay.MaKhachHang);
                            chuSoHuu = khCN?.HoTen;
                        }
                        else
                        {
                            var khDN = await _loanService.GetKhachHangDoanhNghiepAsync(khoanVay.MaKhachHang);
                            chuSoHuu = khDN?.TenCongTy;
                        }

                        TaiSanDamBao taiSan;
                        string? tenTaiSanKhacToSave = null;
                        string? donViToSave = null;
                        
                        // Lấy MaLoaiTaiSan từ loại tài sản đã chọn
                        string tenLoaiTaiSan = loaiTaiSan switch
                        {
                            "Dat" => "Đất",
                            "Xe" => "Xe",
                            "Khac" => "Khác",
                            _ => "Khác"
                        };
                        var maLoaiTaiSan = await _loanService.GetOrCreateLoaiTaiSanAsync(tenLoaiTaiSan);
                        
                        // Nếu loại tài sản là "Khac", luôn lưu TenTaiSanKhac và DonVi (kể cả nếu rỗng)
                        if (loaiTaiSan == "Khac")
                        {
                            taiSan = await _loanService.CreateTaiSanAsync(maLoaiTaiSan, tenTaiSanKhac, donViTaiSan, chuSoHuu, null, giaTriDinhGia, nguoiTao);
                            tenTaiSanKhacToSave = tenTaiSanKhac;
                            donViToSave = donViTaiSan;
                            _logger.LogInformation("SaveDraft - Loại Khác: TenTaiSanKhacToSave='{Ten}', DonViToSave='{DonVi}'", tenTaiSanKhacToSave, donViToSave);
                        }
                        else
                        {
                            taiSan = await _loanService.CreateTaiSanAsync(maLoaiTaiSan, tenLoaiTaiSan, null, chuSoHuu, null, giaTriDinhGia, nguoiTao);
                        }
                        
                        // Parse tỷ lệ thế chấp - xử lý đúng cả số thập phân
                        decimal? tyLeTheChap = null;
                        var tyLeStr = tyLeTheChaps.ElementAtOrDefault(i)?.Trim();
                        if (!string.IsNullOrEmpty(tyLeStr))
                        {
                            tyLeStr = tyLeStr.Replace(",", ".");
                            if (decimal.TryParse(tyLeStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal tyLe))
                            {
                                if (tyLe < 0)
                                    tyLe = 0;
                                else if (tyLe > 999.99m)
                                    tyLe = 999.99m;
                                tyLeTheChap = tyLe;
                            }
                        }
                        
                        var khoanVayTaiSan = new KhoanVayTaiSan
                        {
                            MaTaiSan = taiSan.MaTaiSan,
                            GiaTriDinhGiaTaiThoiDiemVay = giaTriDinhGia,
                            TyLeTheChap = tyLeTheChap,
                            NgayTheChap = DateOnly.TryParse(ngayTheChaps.ElementAtOrDefault(i), out DateOnly ngay) ? ngay : DateOnly.FromDateTime(DateTime.Now),
                            TrangThai = "Đang thế chấp",
                            TenTaiSanKhac = tenTaiSanKhacToSave,
                            DonVi = donViToSave,
                            SoLuong = soLuong
                        };
                        khoanVayTaiSans.Add(khoanVayTaiSan);
                    }
                }
                
                // Đánh dấu CoTaiSanDamBao dựa trên có tài sản hay không
                if (khoanVayTaiSans != null && khoanVayTaiSans.Any())
                {
                    khoanVay.CoTaiSanDamBao = true;
                    _logger.LogInformation("SaveDraft - Đã tìm thấy {Count} tài sản đảm bảo", khoanVayTaiSans.Count);
                }
                else
                {
                    khoanVay.CoTaiSanDamBao = false;
                    _logger.LogInformation("SaveDraft - Không có tài sản đảm bảo được gửi từ form");
                }

                // Validate các field cơ bản
                if (khoanVay.SoTienVay <= 0)
                {
                    var soTienVayStrRetry = Request.Form["SoTienVay"].ToString();
                    if (!string.IsNullOrEmpty(soTienVayStrRetry))
                    {
                        var soTienVayCleanRetry = soTienVayStrRetry.Replace(".", "").Replace(",", "");
                        if (decimal.TryParse(soTienVayCleanRetry, out decimal soTienVayRetry))
                        {
                            khoanVay.SoTienVay = soTienVayRetry;
                        }
                    }
                    
                    if (khoanVay.SoTienVay <= 0)
                    {
                        return Json(new { success = false, message = "Số tiền vay phải lớn hơn 0." });
                    }
                }

                if (khoanVay.LaiSuat < 0 || khoanVay.LaiSuat > 100)
                {
                    return Json(new { success = false, message = "Lãi suất phải từ 0 đến 100%." });
                }

                if (khoanVay.KyHanVay <= 0)
                {
                    return Json(new { success = false, message = "Kỳ hạn vay phải lớn hơn 0." });
                }

                KhoanVay savedLoan;
                _logger.LogInformation("SaveDraft - MaKhoanVay: {MaKhoanVay}, sẽ {Action}", 
                    khoanVay.MaKhoanVay, khoanVay.MaKhoanVay > 0 ? "UPDATE" : "CREATE");
                
                if (khoanVay.MaKhoanVay > 0)
                {
                    // Update hồ sơ nháp hiện có
                    khoanVay.TrangThaiKhoanVay = "Nháp";
                    savedLoan = await _loanService.UpdateLoanAsync(khoanVay, nguoiTao, khoanVayTaiSans);
                    _logger.LogInformation("SaveDraft - Đã cập nhật hồ sơ nháp: MaKhoanVay = {MaKhoanVay}", savedLoan.MaKhoanVay);
                }
                else
                {
                    // Tạo hồ sơ nháp mới
                    khoanVay.TrangThaiKhoanVay = "Nháp";
                    savedLoan = await _loanService.CreateLoanAsync(khoanVay, nguoiTao, khoanVayTaiSans);
                    _logger.LogInformation("SaveDraft - Đã tạo hồ sơ nháp mới: MaKhoanVay = {MaKhoanVay}", savedLoan.MaKhoanVay);
                }

                // Xử lý file upload nếu có
                var filesToSave = new List<(IFormFile File, string LoaiFile)>();
                var phapLyFiles = Request.Form.Files.Where(f => f.Name == "PhapLyFiles").ToList();
                var taiChinhFiles = Request.Form.Files.Where(f => f.Name == "TaiChinhFiles").ToList();
                var taiSanFiles = Request.Form.Files.Where(f => f.Name == "TaiSanDamBaoFiles").ToList();

                _logger.LogInformation("SaveDraft - Found {PhapLyCount} PhapLy files, {TaiChinhCount} TaiChinh files, {TaiSanCount} TaiSan files", 
                    phapLyFiles.Count, taiChinhFiles.Count, taiSanFiles.Count);

                foreach (var file in phapLyFiles)
                {
                    if (file != null && file.Length > 0)
                    {
                        filesToSave.Add((file, "PhapLy"));
                        _logger.LogInformation("SaveDraft - Adding PhapLy file: {FileName} ({Size} bytes)", file.FileName, file.Length);
                    }
                }

                foreach (var file in taiChinhFiles)
                {
                    if (file != null && file.Length > 0)
                    {
                        filesToSave.Add((file, "TaiChinh"));
                        _logger.LogInformation("SaveDraft - Adding TaiChinh file: {FileName} ({Size} bytes)", file.FileName, file.Length);
                    }
                }

                foreach (var file in taiSanFiles)
                {
                    if (file != null && file.Length > 0)
                    {
                        filesToSave.Add((file, "TaiSanDamBao"));
                        _logger.LogInformation("SaveDraft - Adding TaiSanDamBao file: {FileName} ({Size} bytes)", file.FileName, file.Length);
                    }
                }

                if (filesToSave.Any())
                {
                    var webRootPath = _webHostEnvironment.WebRootPath;
                    await _loanService.SaveLoanFilesAsync(savedLoan.MaKhoanVay, filesToSave, nguoiTao, webRootPath);
                }

                return Json(new { success = true, message = "Đã lưu nháp thành công!", loanId = savedLoan.MaKhoanVay });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving draft: {Message}", ex.Message);
                _logger.LogError(ex, "Stack trace: {StackTrace}", ex.StackTrace);
                
                var errorMessage = "Có lỗi xảy ra khi lưu nháp. Vui lòng thử lại.";
                var exceptionMessage = ex.InnerException?.Message ?? ex.Message;
                
                if (exceptionMessage.Contains("Parameter value") && exceptionMessage.Contains("out of range"))
                {
                    var match = System.Text.RegularExpressions.Regex.Match(exceptionMessage, @"Parameter value '([^']+)'");
                    if (match.Success)
                    {
                        var paramValue = match.Groups[1].Value;
                        errorMessage = $"Giá trị '{paramValue}' vượt quá giới hạn cho phép. Vui lòng kiểm tra lại các trường nhập liệu (tỷ lệ thế chấp không được vượt quá 999.99%).";
                    }
                    else
                    {
                        errorMessage = "Một trong các giá trị nhập vào vượt quá giới hạn cho phép của hệ thống. Vui lòng kiểm tra lại các trường nhập liệu.";
                    }
                }
                else if (exceptionMessage.Contains("Cannot insert duplicate key"))
                {
                    errorMessage = "Dữ liệu đã tồn tại trong hệ thống. Vui lòng kiểm tra lại.";
                }
                else if (exceptionMessage.Contains("foreign key") || exceptionMessage.Contains("FOREIGN KEY"))
                {
                    errorMessage = "Dữ liệu tham chiếu không hợp lệ. Vui lòng kiểm tra lại thông tin đã nhập.";
                }
                else if (exceptionMessage.Contains("violation of") || exceptionMessage.Contains("constraint"))
                {
                    errorMessage = "Dữ liệu nhập vào không đúng quy định. Vui lòng kiểm tra lại các trường bắt buộc và giá trị hợp lệ.";
                }
                else if (!string.IsNullOrEmpty(exceptionMessage))
                {
                    errorMessage = "Có lỗi xảy ra khi lưu nháp. Vui lòng kiểm tra lại thông tin đã nhập và thử lại.";
                }
                
                return Json(new { success = false, message = errorMessage });
            }
        }

        // POST: Loan/SubmitForApproval
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitForApproval(int maKhoanVay, string phanLoai, string? ghiChu)
        {
            var maNguoiDungStr = HttpContext.Session.GetString("MaNguoiDung");
            if (string.IsNullOrEmpty(maNguoiDungStr) || !int.TryParse(maNguoiDungStr, out int nguoiCapNhat))
            {
                return Json(new { success = false, message = "Phiên đăng nhập đã hết hạn." });
            }

            try
            {
                var loan = await _loanService.SubmitLoanForApprovalAsync(maKhoanVay, phanLoai, ghiChu, nguoiCapNhat);
                return Json(new { success = true, message = "Đã gửi hồ sơ thành công!", loanId = loan.MaKhoanVay });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting loan for approval");
                return Json(new { success = false, message = "Có lỗi xảy ra khi gửi hồ sơ. Vui lòng thử lại." });
            }
        }

        // GET: Loan/Edit?loanId=1
        public async Task<IActionResult> Edit(int loanId)
        {
            var maNguoiDungStr = HttpContext.Session.GetString("MaNguoiDung");
            if (string.IsNullOrEmpty(maNguoiDungStr) || !int.TryParse(maNguoiDungStr, out int maNhanVien))
            {
                return RedirectToAction("Login", "Account");
            }

            var loan = await _loanService.GetLoanByIdAsync(loanId);
            if (loan == null || loan.MaNhanVienTinDung != maNhanVien || loan.TrangThaiKhoanVay != "Nháp")
            {
                TempData["ErrorMessage"] = "Hồ sơ không tồn tại hoặc không thể chỉnh sửa.";
                return RedirectToAction("SelectCustomer");
            }
            
            // Đảm bảo KhoanVayTaiSans được load
            if (loan.KhoanVayTaiSans == null)
            {
                loan.KhoanVayTaiSans = new List<KhoanVayTaiSan>();
            }

            // Load dữ liệu tương tự như Create
            var loaiKhoanVays = await _loanService.GetLoaiKhoanVayByCustomerTypeAsync(loan.LoaiKhachHang);
            ViewBag.LoaiKhoanVays = new SelectList(loaiKhoanVays, "MaLoaiVay", "TenLoaiVay");
            
            ViewBag.LoaiKhoanVaysData = loaiKhoanVays.Select(l => new { 
                MaLoaiVay = l.MaLoaiVay, 
                TenLoaiVay = l.TenLoaiVay,
                MaLoaiVayCode = l.MaLoaiVayCode,
                LaiSuatToiThieu = l.LaiSuatToiThieu,
                LaiSuatToiDa = l.LaiSuatToiDa
            }).ToList();

            var loaiTaiSans = await _loanService.GetAllLoaiTaiSanAsync();
            ViewBag.LoaiTaiSans = loaiTaiSans;

            // Lấy thông tin khách hàng
            object? khachHang = null;
            string tenKhachHang = "";
            string? anhDaiDien = null;
            string? soCmndCccd = null;
            string? maSoThue = null;
            string? maKhachHangCode = null;
            string? soDienThoai = null;
            string? email = null;

            if (loan.LoaiKhachHang == "CaNhan")
            {
                var kh = await _loanService.GetKhachHangCaNhanAsync(loan.MaKhachHang);
                if (kh != null)
                {
                    khachHang = kh;
                    tenKhachHang = kh.HoTen ?? "";
                    anhDaiDien = kh.AnhDaiDien;
                    soCmndCccd = kh.SoCmnd;
                    maKhachHangCode = kh.MaKhachHangCode;
                    soDienThoai = kh.SoDienThoai;
                    email = kh.Email;
                }
            }
            else
            {
                var kh = await _loanService.GetKhachHangDoanhNghiepAsync(loan.MaKhachHang);
                if (kh != null)
                {
                    khachHang = kh;
                    tenKhachHang = kh.TenCongTy ?? "";
                    anhDaiDien = kh.AnhNguoiDaiDien;
                    maSoThue = kh.MaSoThue;
                    maKhachHangCode = kh.MaKhachHangCode;
                    soDienThoai = kh.SoDienThoai;
                    email = kh.Email;
                }
            }

            // Tra cứu CIC
            ThongTinCic? cicInfo = null;
            if (!string.IsNullOrEmpty(soCmndCccd))
            {
                cicInfo = await _cicService.GetCicByCmndAsync(soCmndCccd);
            }
            else if (!string.IsNullOrEmpty(maSoThue))
            {
                cicInfo = await _cicService.GetCicByMstAsync(maSoThue);
            }

            ViewBag.TenKhachHang = tenKhachHang;
            ViewBag.CustomerType = loan.LoaiKhachHang;
            ViewBag.AnhDaiDien = anhDaiDien;
            ViewBag.KhachHang = khachHang;
            ViewBag.MaKhachHangCode = maKhachHangCode;
            ViewBag.SoDienThoai = soDienThoai;
            ViewBag.Email = email;
            ViewBag.CicInfo = cicInfo;
            
            // Load các file đính kèm đã có
            var existingFiles = await _loanService.GetLoanFilesAsync(loanId);
            ViewBag.ExistingFiles = existingFiles;

            // Lọc bỏ phần "--- Ghi chú của chuyên viên ---" khỏi GhiChu (dữ liệu cũ)
            // Phần này chỉ giữ lại ghi chú hồ sơ, ghi chú nhân viên đã được lưu riêng trong GhiChuNhanVien
            if (!string.IsNullOrEmpty(loan.GhiChu))
            {
                var separatorIndex = loan.GhiChu.IndexOf("--- Ghi chú của chuyên viên ---");
                if (separatorIndex >= 0)
                {
                    loan.GhiChu = loan.GhiChu.Substring(0, separatorIndex).Trim();
                }
            }

            return View("Create", loan);
        }

        // POST: Loan/DeleteLoan
        [HttpPost]
        public async Task<IActionResult> DeleteLoan(int loanId)
        {
            try
            {
                var maNguoiDungStr = HttpContext.Session.GetString("MaNguoiDung");
                if (string.IsNullOrEmpty(maNguoiDungStr) || !int.TryParse(maNguoiDungStr, out int nguoiXoa))
                {
                    _logger.LogWarning("DeleteLoan - Session expired or invalid user");
                    return Json(new { success = false, message = "Phiên đăng nhập đã hết hạn." });
                }

                if (loanId <= 0)
                {
                    _logger.LogWarning("DeleteLoan - Invalid loanId: {LoanId}", loanId);
                    return Json(new { success = false, message = "Thông tin hồ sơ không hợp lệ." });
                }

                _logger.LogInformation("DeleteLoan - Attempting to delete loan {LoanId} by user {UserId}", loanId, nguoiXoa);
                
                var success = await _loanService.DeleteLoanAsync(loanId, nguoiXoa);
                if (success)
                {
                    _logger.LogInformation("Loan {LoanId} deleted successfully by user {UserId}", loanId, nguoiXoa);
                    return Json(new { success = true, message = "Đã xóa hồ sơ nháp thành công." });
                }
                else
                {
                    _logger.LogWarning("Loan {LoanId} not found or cannot be deleted", loanId);
                    return Json(new { success = false, message = "Không tìm thấy hồ sơ nháp hoặc không thể xóa. Vui lòng kiểm tra lại." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting loan {LoanId}. Exception: {ExceptionMessage}", loanId, ex.Message);
                return Json(new { success = false, message = $"Có lỗi xảy ra khi xóa hồ sơ: {ex.Message}. Vui lòng thử lại sau." });
            }
        }

        // POST: Loan/CancelSubmission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelSubmission([FromBody] CancelSubmissionRequest request)
        {
            var maNguoiDungStr = HttpContext.Session.GetString("MaNguoiDung");
            if (string.IsNullOrEmpty(maNguoiDungStr) || !int.TryParse(maNguoiDungStr, out int nguoiCapNhat))
            {
                return Json(new { success = false, message = "Phiên đăng nhập đã hết hạn." });
            }

            try
            {
                // Support both LoanId and Id (for different call sources)
                var loanId = request.LoanId > 0 ? request.LoanId : request.Id;
                
                var success = await _loanService.CancelSubmissionAsync(loanId, nguoiCapNhat);
                if (success)
                {
                    return Json(new { success = true, message = "Đã hủy gửi hồ sơ thành công." });
                }
                else
                {
                    return Json(new { success = false, message = "Không tìm thấy hồ sơ đã gửi hoặc không thể hủy gửi." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error canceling submission for loan {LoanId}", request.LoanId);
                return Json(new { success = false, message = "Có lỗi xảy ra khi hủy gửi hồ sơ." });
            }
        }

        // POST: Loan/Delete - API for deleting loans from Index page
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete([FromBody] DeleteLoanRequest request)
        {
            try
            {
                var maNguoiDungStr = HttpContext.Session.GetString("MaNguoiDung");
                if (string.IsNullOrEmpty(maNguoiDungStr) || !int.TryParse(maNguoiDungStr, out int nguoiXoa))
                {
                    return Json(new { success = false, message = "Phiên đăng nhập đã hết hạn." });
                }

                // Support both LoanId and Id
                var loanId = request.LoanId > 0 ? request.LoanId : request.Id;

                if (loanId <= 0)
                {
                    return Json(new { success = false, message = "Thông tin hồ sơ không hợp lệ." });
                }

                var success = await _loanService.DeleteLoanAsync(loanId, nguoiXoa);
                if (success)
                {
                    return Json(new { success = true, message = "Đã xóa hồ sơ vay thành công." });
                }
                else
                {
                    return Json(new { success = false, message = "Không tìm thấy hồ sơ hoặc không thể xóa." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting loan");
                return Json(new { success = false, message = "Có lỗi xảy ra khi xóa hồ sơ." });
            }
        }

        // GET: Loan/Details/{id} - View loan details
        public async Task<IActionResult> Details(int id)
        {
            var maNguoiDungStr = HttpContext.Session.GetString("MaNguoiDung");
            if (string.IsNullOrEmpty(maNguoiDungStr) || !int.TryParse(maNguoiDungStr, out int maNhanVien))
            {
                return RedirectToAction("Login", "Account");
            }

            var loan = await _loanService.GetLoanByIdAsync(id);
            if (loan == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy hồ sơ vay.";
                return RedirectToAction("Index");
            }

            // Lấy thông tin khách hàng
            object? khachHang = null;
            string tenKhachHang = "";
            string? anhDaiDien = null;
            string? maKhachHangCode = null;
            string? soDienThoai = null;
            string? email = null;
            string? diaChi = null;
            string? soCmndCccd = null;
            string? maSoThue = null;

            if (loan.LoaiKhachHang == "CaNhan")
            {
                var kh = await _loanService.GetKhachHangCaNhanAsync(loan.MaKhachHang);
                if (kh != null)
                {
                    khachHang = kh;
                    tenKhachHang = kh.HoTen;
                    anhDaiDien = kh.AnhDaiDien;
                    maKhachHangCode = kh.MaKhachHangCode;
                    soDienThoai = kh.SoDienThoai;
                    email = kh.Email;
                    diaChi = kh.DiaChi;
                    soCmndCccd = kh.SoCmnd;
                }
            }
            else
            {
                var kh = await _loanService.GetKhachHangDoanhNghiepAsync(loan.MaKhachHang);
                if (kh != null)
                {
                    khachHang = kh;
                    tenKhachHang = kh.TenCongTy;
                    anhDaiDien = kh.AnhNguoiDaiDien;
                    maKhachHangCode = kh.MaKhachHangCode;
                    soDienThoai = kh.SoDienThoai;
                    email = kh.Email;
                    diaChi = kh.DiaChi;
                    maSoThue = kh.MaSoThue;
                }
            }

            // Tra cứu CIC
            ThongTinCic? cicInfo = null;
            if (!string.IsNullOrEmpty(soCmndCccd))
            {
                cicInfo = await _cicService.GetCicByCmndAsync(soCmndCccd);
            }
            else if (!string.IsNullOrEmpty(maSoThue))
            {
                cicInfo = await _cicService.GetCicByMstAsync(maSoThue);
            }

            // Lấy các file đính kèm
            var files = await _loanService.GetLoanFilesAsync(id);

            ViewBag.Loan = loan;
            ViewBag.TenKhachHang = tenKhachHang;
            ViewBag.CustomerType = loan.LoaiKhachHang;
            ViewBag.AnhDaiDien = anhDaiDien;
            ViewBag.KhachHang = khachHang;
            ViewBag.MaKhachHangCode = maKhachHangCode;
            ViewBag.SoDienThoai = soDienThoai;
            ViewBag.Email = email;
            ViewBag.DiaChi = diaChi;
            ViewBag.CicInfo = cicInfo;
            ViewBag.Files = files;
            ViewBag.MaNhanVien = maNhanVien;
            ViewBag.IsMyLoan = loan.MaNhanVienTinDung == maNhanVien;

            return View(loan);
        }
    }

    // ViewModel cho SelectCustomer
    public class SelectCustomerViewModel
    {
        public List<KhachHangCaNhan> KhachHangCaNhans { get; set; } = new();
        public List<KhachHangDoanhNghiep> KhachHangDoanhNghieps { get; set; } = new();
    }

    public class DeleteLoanRequest
    {
        public int LoanId { get; set; }
        public int Id { get; set; }
    }

    public class CancelSubmissionRequest
    {
        public int LoanId { get; set; }
        public int Id { get; set; }
    }
}
