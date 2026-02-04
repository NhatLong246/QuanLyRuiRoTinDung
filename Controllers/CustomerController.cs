using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QuanLyRuiRoTinDung.Models.Entities;
using QuanLyRuiRoTinDung.Services;

namespace QuanLyRuiRoTinDung.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ILogger<CustomerController> _logger;
        private readonly ICustomerService _customerService;

        public CustomerController(ILogger<CustomerController> logger, ICustomerService customerService)
        {
            _logger = logger;
            _customerService = customerService;
        }

        // GET: Customer - Chọn loại khách hàng (Redirect to List)
        public IActionResult Index()
        {
            // Redirect tới trang quản lý khách hàng
            return RedirectToAction("List");
        }

        // GET: Customer/List - Quản lý danh sách khách hàng
        public async Task<IActionResult> List(string? searchTerm = null, string? tab = null)
        {
            // Kiểm tra đăng nhập
            var maNguoiDungStr = HttpContext.Session.GetString("MaNguoiDung");
            if (string.IsNullOrEmpty(maNguoiDungStr) || !int.TryParse(maNguoiDungStr, out int nguoiDung))
            {
                return RedirectToAction("Login", "Account");
            }

            // Tab hiện tại (default là "my")
            ViewBag.CurrentTab = tab ?? "my";
            ViewBag.SearchTerm = searchTerm;
            ViewBag.CurrentUserName = HttpContext.Session.GetString("HoTen");

            // Lấy dữ liệu khách hàng của nhân viên
            var myCaNhans = await _customerService.GetCaNhanByNguoiTaoAsync(nguoiDung, searchTerm);
            var myDoanhNghieps = await _customerService.GetDoanhNghiepByNguoiTaoAsync(nguoiDung, searchTerm);

            // Lấy tất cả khách hàng
            var allCaNhans = await _customerService.GetAllCaNhanWithFilterAsync(searchTerm);
            var allDoanhNghieps = await _customerService.GetAllDoanhNghiepWithFilterAsync(searchTerm);

            // Đếm số lượng
            ViewBag.MyCaNhanCount = myCaNhans.Count;
            ViewBag.MyDoanhNghiepCount = myDoanhNghieps.Count;
            ViewBag.AllCaNhanCount = allCaNhans.Count;
            ViewBag.AllDoanhNghiepCount = allDoanhNghieps.Count;

            // Pass data to view
            ViewBag.MyCaNhans = myCaNhans;
            ViewBag.MyDoanhNghieps = myDoanhNghieps;
            ViewBag.AllCaNhans = allCaNhans;
            ViewBag.AllDoanhNghieps = allDoanhNghieps;

            return View();
        }

        // GET: Customer/Create - Chọn loại khách hàng để tạo
        public IActionResult Create()
        {
            // Kiểm tra đăng nhập
            var maNguoiDungStr = HttpContext.Session.GetString("MaNguoiDung");
            if (string.IsNullOrEmpty(maNguoiDungStr))
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        // GET: Customer/CreateCaNhan
        public IActionResult CreateCaNhan()
        {
            // Kiểm tra đăng nhập
            var maNguoiDungStr = HttpContext.Session.GetString("MaNguoiDung");
            if (string.IsNullOrEmpty(maNguoiDungStr))
            {
                return RedirectToAction("Login", "Account");
            }

            return View(new KhachHangCaNhan());
        }

        // POST: Customer/CreateCaNhan
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCaNhan(KhachHangCaNhan khachHang, IFormFile? cccdTruocFile, IFormFile? cccdSauFile, IFormFile? anhDaiDienFile)
        {
            // Kiểm tra đăng nhập
            var maNguoiDungStr = HttpContext.Session.GetString("MaNguoiDung");
            if (string.IsNullOrEmpty(maNguoiDungStr) || !int.TryParse(maNguoiDungStr, out int nguoiTao))
            {
                return RedirectToAction("Login", "Account");
            }

            // Tạo mã khách hàng tự động TRƯỚC KHI validate để tránh lỗi "required"
            if (string.IsNullOrEmpty(khachHang.MaKhachHangCode))
            {
                khachHang.MaKhachHangCode = await _customerService.GenerateCustomerCodeAsync("CN");
            }

            // Loại bỏ lỗi validation cho MaKhachHangCode vì nó được tạo tự động
            ModelState.Remove("MaKhachHangCode");

            // Validation: Ngày sinh phải đủ 18 tuổi
            if (khachHang.NgaySinh.HasValue)
            {
                var ngaySinh = khachHang.NgaySinh.Value;
                var ngayHienTai = DateOnly.FromDateTime(DateTime.Now);
                var tuoi = ngayHienTai.Year - ngaySinh.Year;
                if (ngayHienTai < ngaySinh.AddYears(tuoi)) tuoi--;
                if (tuoi < 18)
                {
                    ModelState.AddModelError("NgaySinh", "Khách hàng phải đủ 18 tuổi.");
                }
            }

            // Validation: Số năm làm việc không được âm và không lớn hơn tuổi
            if (khachHang.SoNamLamViec.HasValue)
            {
                if (khachHang.SoNamLamViec.Value < 0)
                {
                    ModelState.AddModelError("SoNamLamViec", "Số năm làm việc không được âm.");
                }
                else if (khachHang.NgaySinh.HasValue)
                {
                    var ngaySinh = khachHang.NgaySinh.Value;
                    var ngayHienTai = DateOnly.FromDateTime(DateTime.Now);
                    var tuoi = ngayHienTai.Year - ngaySinh.Year;
                    if (ngayHienTai < ngaySinh.AddYears(tuoi)) tuoi--;

                    if (khachHang.SoNamLamViec.Value > tuoi)
                    {
                        ModelState.AddModelError("SoNamLamViec", "Số năm làm việc không thể lớn hơn tuổi của khách hàng.");
                    }
                }
            }

            // Validation: Giới tính chỉ có Nam hoặc Nữ
            if (!string.IsNullOrEmpty(khachHang.GioiTinh) && khachHang.GioiTinh != "Nam" && khachHang.GioiTinh != "Nữ")
            {
                ModelState.AddModelError("GioiTinh", "Giới tính chỉ có thể là Nam hoặc Nữ.");
            }

            // Validation: Số CCCD phải đủ 12 số
            if (!string.IsNullOrEmpty(khachHang.SoCmnd))
            {
                // Loại bỏ khoảng trắng và ký tự đặc biệt
                var soCmndClean = khachHang.SoCmnd.Replace(" ", "").Replace("-", "");
                if (soCmndClean.Length != 12 || !soCmndClean.All(char.IsDigit))
                {
                    ModelState.AddModelError("SoCmnd", "Số CCCD phải đủ 12 chữ số.");
                }
                else
                {
                    khachHang.SoCmnd = soCmndClean;
                }

                // Kiểm tra CMND đã tồn tại trong bảng cá nhân chưa
                var existingCaNhan = await _customerService.GetCustomerByCmndAsync(khachHang.SoCmnd);
                if (existingCaNhan != null)
                {
                    ModelState.AddModelError("SoCmnd", "Số CMND/CCCD này đã được sử dụng bởi khách hàng cá nhân khác.");
                }

                // Kiểm tra với CIC: Nếu CCCD đã tồn tại trong CIC (loại Cá nhân), tên phải khớp
                var cicValidation = await _customerService.ValidateCicForCaNhanAsync(khachHang.SoCmnd, khachHang.HoTen ?? "");
                if (!cicValidation.isValid && !string.IsNullOrEmpty(cicValidation.fieldName))
                {
                    ModelState.AddModelError(cicValidation.fieldName, cicValidation.errorMessage ?? "Thông tin không khớp với CIC.");
                }

                // Kiểm tra CMND đã tồn tại trong bảng doanh nghiệp chưa (người đại diện pháp luật)
                var existingDoanhNghieps = await _customerService.GetDoanhNghiepByCccdAsync(khachHang.SoCmnd);
                if (existingDoanhNghieps.Any())
                {
                    var firstDoanhNghiep = existingDoanhNghieps.First();
                    
                    // Kiểm tra Họ tên (cá nhân) vs Người đại diện pháp luật (doanh nghiệp)
                    if (!string.IsNullOrEmpty(firstDoanhNghiep.NguoiDaiDienPhapLuat) && 
                        !string.IsNullOrEmpty(khachHang.HoTen) &&
                        firstDoanhNghiep.NguoiDaiDienPhapLuat != khachHang.HoTen)
                    {
                        ModelState.AddModelError("HoTen", $"Họ tên phải giống với thông tin người đại diện pháp luật đã đăng ký với CCCD này trong doanh nghiệp. Giá trị hiện tại: {firstDoanhNghiep.NguoiDaiDienPhapLuat}");
                    }
                    
                    // Kiểm tra Ngày sinh
                    if (firstDoanhNghiep.NgaySinh.HasValue && khachHang.NgaySinh.HasValue &&
                        firstDoanhNghiep.NgaySinh.Value != khachHang.NgaySinh.Value)
                    {
                        ModelState.AddModelError("NgaySinh", $"Ngày sinh phải giống với thông tin người đại diện pháp luật đã đăng ký với CCCD này trong doanh nghiệp. Giá trị hiện tại: {firstDoanhNghiep.NgaySinh.Value:dd/MM/yyyy}");
                    }
                    
                    // Kiểm tra Giới tính
                    if (!string.IsNullOrEmpty(firstDoanhNghiep.GioiTinh) && 
                        !string.IsNullOrEmpty(khachHang.GioiTinh) &&
                        firstDoanhNghiep.GioiTinh != khachHang.GioiTinh)
                    {
                        ModelState.AddModelError("GioiTinh", $"Giới tính phải giống với thông tin người đại diện pháp luật đã đăng ký với CCCD này trong doanh nghiệp. Giá trị hiện tại: {firstDoanhNghiep.GioiTinh}");
                    }
                }
            }

            // Validation: Ngày cấp CCCD phải lớn hơn ngày sinh 16 năm
            if (khachHang.NgaySinh.HasValue && khachHang.NgayCapCmnd.HasValue)
            {
                var minNgayCap = khachHang.NgaySinh.Value.AddYears(16);
                if (khachHang.NgayCapCmnd.Value < minNgayCap)
                {
                    ModelState.AddModelError("NgayCapCmnd", "Ngày cấp CCCD phải lớn hơn ngày sinh ít nhất 16 năm.");
                }
            }

            // Validation: Số điện thoại phải đủ 10 số và không được trùng
            if (!string.IsNullOrEmpty(khachHang.SoDienThoai))
            {
                var soDienThoaiClean = khachHang.SoDienThoai.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");
                if (soDienThoaiClean.Length != 10 || !soDienThoaiClean.All(char.IsDigit))
                {
                    ModelState.AddModelError("SoDienThoai", "Số điện thoại phải đủ 10 chữ số.");
                }
                else
                {
                    khachHang.SoDienThoai = soDienThoaiClean;
                    // Kiểm tra số điện thoại trùng (cả cá nhân và doanh nghiệp)
                    var existingPhone = await _customerService.CheckPhoneExistsAsync(khachHang.SoDienThoai);
                    if (existingPhone)
                    {
                        ModelState.AddModelError("SoDienThoai", "Số điện thoại này đã được sử dụng bởi khách hàng khác.");
                    }
                }
            }

            // Validation: Email không được trùng và đúng định dạng
            if (!string.IsNullOrEmpty(khachHang.Email))
            {
                // Kiểm tra định dạng email
                var emailRegex = new System.Text.RegularExpressions.Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                if (!emailRegex.IsMatch(khachHang.Email))
                {
                    ModelState.AddModelError("Email", "Email không đúng định dạng.");
                }
                else
                {
                    // Kiểm tra email trùng (cả cá nhân và doanh nghiệp)
                    var existingEmail = await _customerService.CheckEmailExistsAsync(khachHang.Email);
                    if (existingEmail)
                    {
                        ModelState.AddModelError("Email", "Email này đã được sử dụng bởi khách hàng khác.");
                    }
                }
            }

            // Validation: Bắt buộc điền đầy đủ các trường quan trọng (server-side backup)
            if (string.IsNullOrWhiteSpace(khachHang.HoTen))
            {
                ModelState.AddModelError("HoTen", "Họ và tên là bắt buộc.");
            }
            if (!khachHang.NgaySinh.HasValue)
            {
                ModelState.AddModelError("NgaySinh", "Ngày sinh là bắt buộc.");
            }
            if (string.IsNullOrWhiteSpace(khachHang.GioiTinh))
            {
                ModelState.AddModelError("GioiTinh", "Giới tính là bắt buộc.");
            }
            if (string.IsNullOrWhiteSpace(khachHang.SoCmnd))
            {
                ModelState.AddModelError("SoCmnd", "Số CCCD là bắt buộc.");
            }
            if (string.IsNullOrWhiteSpace(khachHang.SoDienThoai))
            {
                ModelState.AddModelError("SoDienThoai", "Số điện thoại là bắt buộc.");
            }
            if (string.IsNullOrWhiteSpace(khachHang.Email))
            {
                ModelState.AddModelError("Email", "Email là bắt buộc.");
            }
            if (string.IsNullOrWhiteSpace(khachHang.DiaChi))
            {
                ModelState.AddModelError("DiaChi", "Địa chỉ thường trú là bắt buộc.");
            }
            if (string.IsNullOrWhiteSpace(khachHang.ThanhPho))
            {
                ModelState.AddModelError("ThanhPho", "Thành phố/Tỉnh là bắt buộc.");
            }
            if (string.IsNullOrWhiteSpace(khachHang.Quan))
            {
                ModelState.AddModelError("Quan", "Quận/Huyện là bắt buộc.");
            }
            if (string.IsNullOrWhiteSpace(khachHang.Phuong))
            {
                ModelState.AddModelError("Phuong", "Phường/Xã là bắt buộc.");
            }
            if (string.IsNullOrWhiteSpace(khachHang.TinhTrangHonNhan))
            {
                ModelState.AddModelError("TinhTrangHonNhan", "Tình trạng hôn nhân là bắt buộc.");
            }
            if (string.IsNullOrWhiteSpace(khachHang.NgheNghiep))
            {
                ModelState.AddModelError("NgheNghiep", "Nghề nghiệp là bắt buộc.");
            }

            // Xử lý upload file ảnh
            if (cccdTruocFile != null && cccdTruocFile.Length > 0)
            {
                khachHang.CccdTruoc = await SaveFileAsync(cccdTruocFile, "cccd");
            }

            if (cccdSauFile != null && cccdSauFile.Length > 0)
            {
                khachHang.CccdSau = await SaveFileAsync(cccdSauFile, "cccd");
            }

            if (anhDaiDienFile != null && anhDaiDienFile.Length > 0)
            {
                khachHang.AnhDaiDien = await SaveFileAsync(anhDaiDienFile, "avatar");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _customerService.CreateCaNhanAsync(khachHang, nguoiTao);
                    var successMessage = $"Tạo khách hàng cá nhân thành công! Mã khách hàng: {khachHang.MaKhachHangCode}";
                    TempData["SuccessMessage"] = successMessage;
                    // Clear form sau khi thành công
                    return View(new KhachHangCaNhan());
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating customer");
                    ModelState.AddModelError("", "Có lỗi xảy ra khi tạo khách hàng. Vui lòng thử lại.");
                }
            }

            return View(khachHang);
        }

        // GET: Customer/CreateDoanhNghiep
        public IActionResult CreateDoanhNghiep()
        {
            // Kiểm tra đăng nhập
            var maNguoiDungStr = HttpContext.Session.GetString("MaNguoiDung");
            if (string.IsNullOrEmpty(maNguoiDungStr))
            {
                return RedirectToAction("Login", "Account");
            }

            return View(new KhachHangDoanhNghiep());
        }

        // POST: Customer/CreateDoanhNghiep
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDoanhNghiep(
            KhachHangDoanhNghiep khachHang,
            List<IFormFile>? anhGiayPhepKinhDoanhFiles,
            List<IFormFile>? anhBaoCaoTaichinhFiles,
            List<IFormFile>? anhGiayToLienQuanKhacFiles,
            IFormFile? cccdTruocFile,
            IFormFile? cccdSauFile,
            IFormFile? anhNguoiDaiDienFile)
        {
            // Kiểm tra đăng nhập
            var maNguoiDungStr = HttpContext.Session.GetString("MaNguoiDung");
            if (string.IsNullOrEmpty(maNguoiDungStr) || !int.TryParse(maNguoiDungStr, out int nguoiTao))
            {
                return RedirectToAction("Login", "Account");
            }

            // Tạo mã khách hàng tự động TRƯỚC KHI validate để tránh lỗi "required"
            if (string.IsNullOrEmpty(khachHang.MaKhachHangCode))
            {
                khachHang.MaKhachHangCode = await _customerService.GenerateCustomerCodeAsync("DN");
            }

            // Loại bỏ lỗi validation cho MaKhachHangCode vì nó được tạo tự động
            ModelState.Remove("MaKhachHangCode");

            // Validation: Mã số thuế - format và không được trùng
            // Lấy giá trị từ hidden field (đã bỏ dấu -) hoặc từ model
            var maSoThueValue = Request.Form["MaSoThue"].ToString();
            if (string.IsNullOrEmpty(maSoThueValue) && !string.IsNullOrEmpty(khachHang.MaSoThue))
            {
                maSoThueValue = khachHang.MaSoThue.Replace("-", "");
            }
            else if (!string.IsNullOrEmpty(maSoThueValue))
            {
                maSoThueValue = maSoThueValue.Replace("-", "");
            }
            
            if (!string.IsNullOrEmpty(maSoThueValue))
            {
                // Validate format: phải đủ 13 chữ số
                if (maSoThueValue.Length != 13 || !maSoThueValue.All(char.IsDigit))
                {
                    ModelState.AddModelError("MaSoThue", "Mã số thuế phải đủ 13 chữ số (10 chữ số + 3 chữ số phụ).");
                }
                else
                {
                    // Lưu giá trị số thuần (13 chữ số) vào model
                    khachHang.MaSoThue = maSoThueValue;
                    
                    // Kiểm tra trùng
                    var existing = await _customerService.GetCustomerByMstAsync(khachHang.MaSoThue);
                    if (existing != null)
                    {
                        ModelState.AddModelError("MaSoThue", "Mã số thuế này đã được sử dụng bởi doanh nghiệp khác.");
                    }

                    // Kiểm tra với CIC: Nếu MST đã tồn tại trong CIC (loại Doanh nghiệp), 
                    // CCCD người đại diện và tên công ty phải khớp
                    var cicValidation = await _customerService.ValidateCicForDoanhNghiepAsync(
                        khachHang.MaSoThue,
                        khachHang.SoCccdNguoiDaiDienPhapLuat,
                        khachHang.TenCongTy ?? "");
                    if (!cicValidation.isValid && !string.IsNullOrEmpty(cicValidation.fieldName))
                    {
                        ModelState.AddModelError(cicValidation.fieldName, cicValidation.errorMessage ?? "Thông tin không khớp với CIC.");
                    }
                }
            }

            // Validation: Số điện thoại phải đủ 10 số và không được trùng
            if (!string.IsNullOrEmpty(khachHang.SoDienThoai))
            {
                var soDienThoaiClean = khachHang.SoDienThoai.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");
                if (soDienThoaiClean.Length != 10 || !soDienThoaiClean.All(char.IsDigit))
                {
                    ModelState.AddModelError("SoDienThoai", "Số điện thoại phải đủ 10 chữ số.");
                }
                else
                {
                    khachHang.SoDienThoai = soDienThoaiClean;
                    // Kiểm tra số điện thoại trùng (cả cá nhân và doanh nghiệp)
                    var existingPhone = await _customerService.CheckPhoneExistsAsync(khachHang.SoDienThoai);
                    if (existingPhone)
                    {
                        ModelState.AddModelError("SoDienThoai", "Số điện thoại này đã được sử dụng bởi khách hàng khác.");
                    }
                }
            }

            // Validation: Email không được trùng và đúng định dạng
            if (!string.IsNullOrEmpty(khachHang.Email))
            {
                // Kiểm tra định dạng email
                var emailRegex = new System.Text.RegularExpressions.Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                if (!emailRegex.IsMatch(khachHang.Email))
                {
                    ModelState.AddModelError("Email", "Email không đúng định dạng.");
                }
                else
                {
                    // Kiểm tra email trùng (cả cá nhân và doanh nghiệp)
                    var existingEmail = await _customerService.CheckEmailExistsAsync(khachHang.Email);
                    if (existingEmail)
                    {
                        ModelState.AddModelError("Email", "Email này đã được sử dụng bởi khách hàng khác.");
                    }
                }
            }

            // Xử lý upload file ảnh (cho phép nhiều file cho một loại, lưu đường dẫn cách nhau bằng ;)
            if (anhGiayPhepKinhDoanhFiles != null && anhGiayPhepKinhDoanhFiles.Any(f => f != null && f.Length > 0))
            {
                var paths = new List<string>();
                foreach (var file in anhGiayPhepKinhDoanhFiles.Where(f => f != null && f.Length > 0))
                {
                    paths.Add(await SaveFileAsync(file, "giayphep"));
                }
                khachHang.AnhGiayPhepKinhDoanh = string.Join(";", paths);
            }

            if (anhBaoCaoTaichinhFiles != null && anhBaoCaoTaichinhFiles.Any(f => f != null && f.Length > 0))
            {
                var paths = new List<string>();
                foreach (var file in anhBaoCaoTaichinhFiles.Where(f => f != null && f.Length > 0))
                {
                    paths.Add(await SaveFileAsync(file, "baocao"));
                }
                khachHang.AnhBaoCaoTaichinh = string.Join(";", paths);
            }

            if (anhGiayToLienQuanKhacFiles != null && anhGiayToLienQuanKhacFiles.Any(f => f != null && f.Length > 0))
            {
                var paths = new List<string>();
                foreach (var file in anhGiayToLienQuanKhacFiles.Where(f => f != null && f.Length > 0))
                {
                    paths.Add(await SaveFileAsync(file, "giayto"));
                }
                khachHang.AnhGiayToLienQuanKhac = string.Join(";", paths);
            }

            if (cccdTruocFile != null && cccdTruocFile.Length > 0)
            {
                khachHang.CccdTruoc = await SaveFileAsync(cccdTruocFile, "cccd");
            }

            if (cccdSauFile != null && cccdSauFile.Length > 0)
            {
                khachHang.CccdSau = await SaveFileAsync(cccdSauFile, "cccd");
            }

            if (anhNguoiDaiDienFile != null && anhNguoiDaiDienFile.Length > 0)
            {
                khachHang.AnhNguoiDaiDien = await SaveFileAsync(anhNguoiDaiDienFile, "avatar");
            }

            // Validation: Tên công ty không được trùng
            if (!string.IsNullOrEmpty(khachHang.TenCongTy))
            {
                var tenCongTyExists = await _customerService.CheckTenCongTyExistsAsync(khachHang.TenCongTy);
                if (tenCongTyExists)
                {
                    ModelState.AddModelError("TenCongTy", "Tên công ty này đã được sử dụng bởi doanh nghiệp khác.");
                }
            }

            // Validation: Số giấy phép kinh doanh không được trùng
            if (!string.IsNullOrEmpty(khachHang.SoGiayPhepKinhDoanh))
            {
                var soGiayPhepExists = await _customerService.CheckSoGiayPhepKinhDoanhExistsAsync(khachHang.SoGiayPhepKinhDoanh);
                if (soGiayPhepExists)
                {
                    ModelState.AddModelError("SoGiayPhepKinhDoanh", "Số giấy phép kinh doanh này đã được sử dụng bởi doanh nghiệp khác.");
                }
            }

            // Validation: Nếu CCCD đã tồn tại (trong doanh nghiệp hoặc cá nhân), các thông tin phải giống nhau
            if (!string.IsNullOrEmpty(khachHang.SoCccdNguoiDaiDienPhapLuat))
            {
                // Kiểm tra trong bảng doanh nghiệp
                var existingDoanhNghieps = await _customerService.GetDoanhNghiepByCccdAsync(khachHang.SoCccdNguoiDaiDienPhapLuat);
                if (existingDoanhNghieps.Any())
                {
                    var firstDoanhNghiep = existingDoanhNghieps.First();
                    
                    // Kiểm tra Người đại diện pháp luật
                    if (!string.IsNullOrEmpty(firstDoanhNghiep.NguoiDaiDienPhapLuat) && 
                        !string.IsNullOrEmpty(khachHang.NguoiDaiDienPhapLuat) &&
                        firstDoanhNghiep.NguoiDaiDienPhapLuat != khachHang.NguoiDaiDienPhapLuat)
                    {
                        ModelState.AddModelError("NguoiDaiDienPhapLuat", $"Người đại diện pháp luật phải giống với các doanh nghiệp khác đã đăng ký với CCCD này. Giá trị hiện tại: {firstDoanhNghiep.NguoiDaiDienPhapLuat}");
                    }
                    
                    // Kiểm tra Ngày sinh
                    if (firstDoanhNghiep.NgaySinh.HasValue && khachHang.NgaySinh.HasValue &&
                        firstDoanhNghiep.NgaySinh.Value != khachHang.NgaySinh.Value)
                    {
                        ModelState.AddModelError("NgaySinh", $"Ngày sinh người đại diện phải giống với các doanh nghiệp khác đã đăng ký với CCCD này. Giá trị hiện tại: {firstDoanhNghiep.NgaySinh.Value:dd/MM/yyyy}");
                    }
                    
                    // Kiểm tra Giới tính
                    if (!string.IsNullOrEmpty(firstDoanhNghiep.GioiTinh) && 
                        !string.IsNullOrEmpty(khachHang.GioiTinh) &&
                        firstDoanhNghiep.GioiTinh != khachHang.GioiTinh)
                    {
                        ModelState.AddModelError("GioiTinh", $"Giới tính người đại diện phải giống với các doanh nghiệp khác đã đăng ký với CCCD này. Giá trị hiện tại: {firstDoanhNghiep.GioiTinh}");
                    }
                }

                // Kiểm tra trong bảng cá nhân
                var existingCaNhan = await _customerService.GetCustomerByCmndAsync(khachHang.SoCccdNguoiDaiDienPhapLuat);
                if (existingCaNhan != null)
                {
                    // Kiểm tra Họ tên (cá nhân) vs Người đại diện pháp luật (doanh nghiệp)
                    if (!string.IsNullOrEmpty(existingCaNhan.HoTen) && 
                        !string.IsNullOrEmpty(khachHang.NguoiDaiDienPhapLuat) &&
                        existingCaNhan.HoTen != khachHang.NguoiDaiDienPhapLuat)
                    {
                        ModelState.AddModelError("NguoiDaiDienPhapLuat", $"Người đại diện pháp luật phải giống với thông tin khách hàng cá nhân đã đăng ký với CCCD này. Giá trị hiện tại: {existingCaNhan.HoTen}");
                    }
                    
                    // Kiểm tra Ngày sinh
                    if (existingCaNhan.NgaySinh.HasValue && khachHang.NgaySinh.HasValue &&
                        existingCaNhan.NgaySinh.Value != khachHang.NgaySinh.Value)
                    {
                        ModelState.AddModelError("NgaySinh", $"Ngày sinh người đại diện phải giống với thông tin khách hàng cá nhân đã đăng ký với CCCD này. Giá trị hiện tại: {existingCaNhan.NgaySinh.Value:dd/MM/yyyy}");
                    }
                    
                    // Kiểm tra Giới tính
                    if (!string.IsNullOrEmpty(existingCaNhan.GioiTinh) && 
                        !string.IsNullOrEmpty(khachHang.GioiTinh) &&
                        existingCaNhan.GioiTinh != khachHang.GioiTinh)
                    {
                        ModelState.AddModelError("GioiTinh", $"Giới tính người đại diện phải giống với thông tin khách hàng cá nhân đã đăng ký với CCCD này. Giá trị hiện tại: {existingCaNhan.GioiTinh}");
                    }
                }
            }

            // Validation: Số lượng nhân viên không được âm
            if (khachHang.SoLuongNhanVien.HasValue && khachHang.SoLuongNhanVien.Value < 0)
            {
                ModelState.AddModelError("SoLuongNhanVien", "Số lượng nhân viên không được âm.");
            }

            // Validation: Ngày sinh người đại diện phải đủ 18 tuổi
            if (khachHang.NgaySinh.HasValue)
            {
                var ngaySinh = khachHang.NgaySinh.Value;
                var ngayHienTai = DateOnly.FromDateTime(DateTime.Now);
                var tuoi = ngayHienTai.Year - ngaySinh.Year;
                if (ngayHienTai < ngaySinh.AddYears(tuoi)) tuoi--;
                if (tuoi < 18)
                {
                    ModelState.AddModelError("NgaySinh", "Người đại diện pháp luật phải đủ 18 tuổi.");
                }
            }

            // Validation: Ngày cấp giấy phép phải trước hoặc bằng ngày đăng ký thành lập
            if (khachHang.NgayCapGiayPhep.HasValue && khachHang.NgayDangKy.HasValue)
            {
                if (khachHang.NgayCapGiayPhep.Value > khachHang.NgayDangKy.Value)
                {
                    ModelState.AddModelError("NgayCapGiayPhep", "Ngày cấp giấy phép phải trước hoặc bằng ngày đăng ký thành lập.");
                    ModelState.AddModelError("NgayDangKy", "Ngày đăng ký thành lập phải sau hoặc bằng ngày cấp giấy phép.");
                }
            }
            
            // Validation: Ngày không được ở tương lai
            if (khachHang.NgayCapGiayPhep.HasValue && khachHang.NgayCapGiayPhep.Value > DateOnly.FromDateTime(DateTime.Now))
            {
                ModelState.AddModelError("NgayCapGiayPhep", "Ngày cấp giấy phép không được ở tương lai.");
            }
            
            if (khachHang.NgayDangKy.HasValue && khachHang.NgayDangKy.Value > DateOnly.FromDateTime(DateTime.Now))
            {
                ModelState.AddModelError("NgayDangKy", "Ngày đăng ký thành lập không được ở tương lai.");
            }

            // Validation: Doanh thu hàng năm, Tổng tài sản, Vốn điều lệ không được âm
            if (khachHang.DoanhThuHangNam.HasValue && khachHang.DoanhThuHangNam.Value < 0)
            {
                ModelState.AddModelError("DoanhThuHangNam", "Doanh thu hàng năm không được âm.");
            }

            if (khachHang.TongTaiSan.HasValue && khachHang.TongTaiSan.Value < 0)
            {
                ModelState.AddModelError("TongTaiSan", "Tổng tài sản không được âm.");
            }

            if (khachHang.VonDieuLe.HasValue && khachHang.VonDieuLe.Value < 0)
            {
                ModelState.AddModelError("VonDieuLe", "Vốn điều lệ không được âm.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Convert string values từ hidden fields sang decimal nếu cần
                    // (Các giá trị đã được gửi từ hidden fields dưới dạng string, cần parse)
                    await _customerService.CreateDoanhNghiepAsync(khachHang, nguoiTao);
                    var successMessage = $"Tạo khách hàng doanh nghiệp thành công! Mã khách hàng: {khachHang.MaKhachHangCode}";
                    TempData["SuccessMessage"] = successMessage;
                    // Clear form sau khi thành công
                    return View(new KhachHangDoanhNghiep());
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating customer");
                    ModelState.AddModelError("", "Có lỗi xảy ra khi tạo khách hàng. Vui lòng thử lại.");
                }
            }

            return View(khachHang);
        }

        // Helper method để lưu file ảnh
        private async Task<string> SaveFileAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
                return string.Empty;

            // Kiểm tra định dạng file
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".pdf" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                throw new ArgumentException("Định dạng file không được hỗ trợ. Chỉ chấp nhận: JPG, JPEG, PNG, GIF, PDF");
            }

            // Tạo thư mục nếu chưa tồn tại
            var uploadsFolder = Path.Combine("wwwroot", "uploads", folder);
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Tạo tên file unique
            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // Lưu file
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // Trả về đường dẫn tương đối từ wwwroot
            return $"/uploads/{folder}/{uniqueFileName}";
        }

        // AJAX: Kiểm tra mã số thuế trùng
        [HttpGet]
        public async Task<IActionResult> CheckMaSoThueExists(string maSoThue)
        {
            if (string.IsNullOrEmpty(maSoThue))
            {
                return Json(new { exists = false });
            }

            // Bỏ dấu - nếu có
            var maSoThueClean = maSoThue.Replace("-", "");
            
            var existing = await _customerService.GetCustomerByMstAsync(maSoThueClean);
            return Json(new { exists = existing != null });
        }

        // AJAX: Kiểm tra số điện thoại trùng
        [HttpGet]
        public async Task<IActionResult> CheckPhoneExists(string phone, int? excludeId = null, string? customerType = null)
        {
            if (string.IsNullOrEmpty(phone))
            {
                return Json(new { exists = false });
            }

            var exists = await _customerService.CheckPhoneExistsAsync(phone, excludeId, customerType);
            return Json(new { exists = exists });
        }

        // AJAX: Kiểm tra email trùng
        [HttpGet]
        public async Task<IActionResult> CheckEmailExists(string email, int? excludeId = null, string? customerType = null)
        {
            if (string.IsNullOrEmpty(email))
            {
                return Json(new { exists = false });
            }

            var exists = await _customerService.CheckEmailExistsAsync(email, excludeId, customerType);
            return Json(new { exists = exists });
        }

        // AJAX: Kiểm tra CCCD/CMND trùng (tất cả - dùng cho doanh nghiệp)
        [HttpGet]
        public async Task<IActionResult> CheckCmndExists(string soCmnd)
        {
            if (string.IsNullOrEmpty(soCmnd))
            {
                return Json(new { exists = false });
            }

            var soCmndClean = soCmnd.Replace(" ", "").Replace("-", "");

            var existingCaNhan = await _customerService.GetCustomerByCmndAsync(soCmndClean);
            var existingDoanhNghieps = await _customerService.GetDoanhNghiepByCccdAsync(soCmndClean);

            var exists = existingCaNhan != null || (existingDoanhNghieps != null && existingDoanhNghieps.Any());
            return Json(new { exists });
        }

        // AJAX: Kiểm tra CCCD/CMND trùng - CHỈ trong bảng KhachHangCaNhan (dùng cho tạo khách hàng cá nhân)
        [HttpGet]
        public async Task<IActionResult> CheckCmndExistsForCaNhan(string soCmnd, int? excludeId = null)
        {
            if (string.IsNullOrEmpty(soCmnd))
            {
                return Json(new { exists = false });
            }

            var soCmndClean = soCmnd.Replace(" ", "").Replace("-", "");

            var existingCaNhan = await _customerService.GetCustomerByCmndAsync(soCmndClean);
            
            // Nếu có excludeId, bỏ qua khách hàng hiện tại (cho trường hợp edit)
            var exists = existingCaNhan != null && (!excludeId.HasValue || existingCaNhan.MaKhachHang != excludeId.Value);
            return Json(new { exists });
        }

        // AJAX: Lấy thông tin từ CCCD (khách hàng cá nhân) để auto-fill vào form doanh nghiệp
        [HttpGet]
        public async Task<IActionResult> GetInfoFromCccd(string soCccd)
        {
            if (string.IsNullOrEmpty(soCccd))
            {
                return Json(new { exists = false });
            }

            var soCccdClean = soCccd.Replace(" ", "").Replace("-", "");
            var caNhan = await _customerService.GetCustomerByCmndAsync(soCccdClean);

            if (caNhan != null)
            {
                return Json(new
                {
                    exists = true,
                    hoTen = caNhan.HoTen,
                    ngaySinh = caNhan.NgaySinh?.ToString("yyyy-MM-dd"),
                    gioiTinh = caNhan.GioiTinh
                });
            }

            return Json(new { exists = false });
        }

        // AJAX: Kiểm tra giấy phép kinh doanh trùng
        [HttpGet]
        public async Task<IActionResult> CheckGiayPhepExists(string soGiayPhep)
        {
            if (string.IsNullOrEmpty(soGiayPhep))
            {
                return Json(new { exists = false });
            }

            var existing = await _customerService.GetCustomerByGiayPhepAsync(soGiayPhep.Trim());
            return Json(new { exists = existing != null });
        }

        // AJAX: Kiểm tra tên công ty trùng - REALTIME
        [HttpGet]
        public async Task<IActionResult> CheckTenCongTyExists(string tenCongTy)
        {
            if (string.IsNullOrEmpty(tenCongTy))
            {
                return Json(new { exists = false });
            }

            var exists = await _customerService.CheckTenCongTyExistsAsync(tenCongTy.Trim());
            return Json(new { exists = exists });
        }

        // AJAX: Cross-validate doanh nghiệp với CIC (Mã số thuế, Tên công ty, CCCD người đại diện)
        [HttpGet]
        public async Task<IActionResult> CrossValidateCIC(string maSoThue, string tenCongTy, string? cccd)
        {
            if (string.IsNullOrEmpty(maSoThue) || string.IsNullOrEmpty(tenCongTy))
            {
                return Json(new { hasCicRecord = false });
            }

            var maSoThueClean = maSoThue.Replace("-", "");
            var result = await _customerService.CrossValidateCICForDoanhNghiepAsync(maSoThueClean, tenCongTy, cccd);

            return Json(new
            {
                hasCicRecord = result.HasCicRecord,
                maSoThueMismatch = result.MaSoThueMismatch,
                tenCongTyMismatch = result.TenCongTyMismatch,
                cccdMismatch = result.CccdMismatch,
                cicMaSoThue = result.CicMaSoThue,
                cicTenCongTy = result.CicTenCongTy,
                cicCccd = result.CicCccd
            });
        }

        // AJAX: Cross-check người đại diện với CIC, Khách hàng cá nhân VÀ các Doanh nghiệp khác
        [HttpGet]
        public async Task<IActionResult> CrossCheckWithCIC(string soCccd, string? nguoiDaiDien, string? ngaySinh, string? gioiTinh, int? excludeId = null)
        {
            if (string.IsNullOrEmpty(soCccd))
            {
                return Json(new { hasWarnings = false, warnings = new List<object>() });
            }

            var soCccdClean = soCccd.Replace(" ", "").Replace("-", "");
            DateOnly? ngaySinhParsed = null;
            if (!string.IsNullOrEmpty(ngaySinh) && DateOnly.TryParse(ngaySinh, out var parsed))
            {
                ngaySinhParsed = parsed;
            }

            var result = await _customerService.CrossCheckNguoiDaiDienWithCICAsync(
                soCccdClean, nguoiDaiDien, ngaySinhParsed, gioiTinh, excludeId);

            return Json(new
            {
                hasWarnings = result.Warnings.Count > 0,
                warnings = result.Warnings.Select(w => new { fieldName = w.FieldName, message = w.Message }),
                referenceData = result.ReferenceData != null ? new
                {
                    hoTen = result.ReferenceData.HoTen,
                    ngaySinh = result.ReferenceData.NgaySinh,
                    gioiTinh = result.ReferenceData.GioiTinh,
                    source = result.ReferenceData.Source
                } : null
            });
        }

        // AJAX: Kiểm tra CIC cho khách hàng cá nhân - trả về thông tin để so sánh realtime
        [HttpGet]
        public async Task<IActionResult> CheckCicForCaNhan(string soCccd, string hoTen)
        {
            if (string.IsNullOrEmpty(soCccd))
            {
                return Json(new { hasCic = false });
            }

            var soCccdClean = soCccd.Replace(" ", "").Replace("-", "");
            var validation = await _customerService.ValidateCicForCaNhanAsync(soCccdClean, hoTen ?? "");

            if (!validation.isValid)
            {
                return Json(new { 
                    hasCic = true, 
                    isValid = false, 
                    fieldName = validation.fieldName,
                    errorMessage = validation.errorMessage 
                });
            }

            return Json(new { hasCic = true, isValid = true });
        }

        // AJAX: Kiểm tra CIC cho doanh nghiệp - trả về thông tin để so sánh realtime
        [HttpGet]
        public async Task<IActionResult> CheckCicForDoanhNghiep(string maSoThue, string? soCccdNguoiDaiDien, string tenCongTy)
        {
            if (string.IsNullOrEmpty(maSoThue))
            {
                return Json(new { hasCic = false });
            }

            var maSoThueClean = maSoThue.Replace("-", "");
            var validation = await _customerService.ValidateCicForDoanhNghiepAsync(maSoThueClean, soCccdNguoiDaiDien, tenCongTy ?? "");

            if (!validation.isValid)
            {
                return Json(new { 
                    hasCic = true, 
                    isValid = false, 
                    fieldName = validation.fieldName,
                    errorMessage = validation.errorMessage 
                });
            }

            return Json(new { hasCic = true, isValid = true });
        }

        // AJAX: Cross-validation CCCD cho Doanh nghiệp - kiểm tra với Khách hàng cá nhân/CIC cá nhân
        [HttpGet]
        public async Task<IActionResult> CrossCheckCccdForDoanhNghiep(string soCccd, string? nguoiDaiDien, string? ngaySinh, string? gioiTinh)
        {
            if (string.IsNullOrEmpty(soCccd))
            {
                return Json(new { hasExistingData = false });
            }

            var soCccdClean = soCccd.Replace(" ", "").Replace("-", "");
            DateOnly? ngaySinhParsed = null;
            if (!string.IsNullOrEmpty(ngaySinh) && DateOnly.TryParse(ngaySinh, out var parsed))
            {
                ngaySinhParsed = parsed;
            }

            var result = await _customerService.ValidateCrossCheckCccdForDoanhNghiepAsync(
                soCccdClean, nguoiDaiDien, ngaySinhParsed, gioiTinh);

            return Json(new
            {
                hasExistingData = result.HasExistingData,
                isValid = result.IsValid,
                sourceType = result.SourceType,
                refHoTen = result.RefHoTen,
                refNgaySinh = result.RefNgaySinh?.ToString("yyyy-MM-dd"),
                refGioiTinh = result.RefGioiTinh,
                errors = result.Errors.Select(e => new { fieldName = e.FieldName, errorMessage = e.ErrorMessage })
            });
        }

        // AJAX: Cross-validation CCCD cho Cá nhân - kiểm tra với Doanh nghiệp (người đại diện)
        [HttpGet]
        public async Task<IActionResult> CrossCheckCccdForCaNhan(string soCccd, string? hoTen, string? ngaySinh, string? gioiTinh)
        {
            if (string.IsNullOrEmpty(soCccd))
            {
                return Json(new { hasExistingData = false });
            }

            var soCccdClean = soCccd.Replace(" ", "").Replace("-", "");
            DateOnly? ngaySinhParsed = null;
            if (!string.IsNullOrEmpty(ngaySinh) && DateOnly.TryParse(ngaySinh, out var parsed))
            {
                ngaySinhParsed = parsed;
            }

            var result = await _customerService.ValidateCrossCheckCccdForCaNhanAsync(
                soCccdClean, hoTen, ngaySinhParsed, gioiTinh);

            return Json(new
            {
                hasExistingData = result.HasExistingData,
                isValid = result.IsValid,
                sourceType = result.SourceType,
                refHoTen = result.RefHoTen,
                refNgaySinh = result.RefNgaySinh?.ToString("yyyy-MM-dd"),
                refGioiTinh = result.RefGioiTinh,
                errors = result.Errors.Select(e => new { fieldName = e.FieldName, errorMessage = e.ErrorMessage })
            });
        }

        // GET: Customer/DetailsCaNhan/5 - Xem chi tiết khách hàng cá nhân
        public async Task<IActionResult> DetailsCaNhan(int id)
        {
            // Kiểm tra đăng nhập
            var maNguoiDungStr = HttpContext.Session.GetString("MaNguoiDung");
            if (string.IsNullOrEmpty(maNguoiDungStr))
            {
                return RedirectToAction("Login", "Account");
            }

            var khachHang = await _customerService.GetCaNhanByIdAsync(id);
            if (khachHang == null)
            {
                return NotFound();
            }

            return View(khachHang);
        }

        // GET: Customer/DetailsDoanhNghiep/5 - Xem chi tiết khách hàng doanh nghiệp
        public async Task<IActionResult> DetailsDoanhNghiep(int id)
        {
            // Kiểm tra đăng nhập
            var maNguoiDungStr = HttpContext.Session.GetString("MaNguoiDung");
            if (string.IsNullOrEmpty(maNguoiDungStr))
            {
                return RedirectToAction("Login", "Account");
            }

            var khachHang = await _customerService.GetDoanhNghiepByIdAsync(id);
            if (khachHang == null)
            {
                return NotFound();
            }

            return View(khachHang);
        }

        // GET: Customer/EditCaNhan/5 - Form cập nhật khách hàng cá nhân
        public async Task<IActionResult> EditCaNhan(int id)
        {
            // Kiểm tra đăng nhập
            var maNguoiDungStr = HttpContext.Session.GetString("MaNguoiDung");
            if (string.IsNullOrEmpty(maNguoiDungStr))
            {
                return RedirectToAction("Login", "Account");
            }

            var khachHang = await _customerService.GetCaNhanByIdAsync(id);
            if (khachHang == null)
            {
                return NotFound();
            }

            return View(khachHang);
        }

        // POST: Customer/EditCaNhan/5 - Xử lý cập nhật khách hàng cá nhân
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCaNhan(int id, KhachHangCaNhan model,
            IFormFile? cccdTruocFile, IFormFile? cccdSauFile, IFormFile? anhDaiDienFile,
            string? existingCccdTruoc, string? existingCccdSau, string? existingAnhDaiDien)
        {
            // Kiểm tra đăng nhập
            var maNguoiDungStr = HttpContext.Session.GetString("MaNguoiDung");
            if (string.IsNullOrEmpty(maNguoiDungStr))
            {
                return RedirectToAction("Login", "Account");
            }

            if (id != model.MaKhachHang)
            {
                return BadRequest();
            }

            var existing = await _customerService.GetCaNhanByIdAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            // Cập nhật các thông tin (không cho phép thay đổi CCCD, mã khách hàng, họ tên)
            existing.SoDienThoai = model.SoDienThoai;
            existing.Email = model.Email;
            existing.DiaChi = model.DiaChi;
            existing.ThanhPho = model.ThanhPho;
            existing.Quan = model.Quan;
            existing.Phuong = model.Phuong;
            existing.NgheNghiep = model.NgheNghiep;
            existing.ThuNhapHangThang = model.ThuNhapHangThang;
            existing.TinhTrangHonNhan = model.TinhTrangHonNhan;
            existing.TenCongTy = model.TenCongTy;
            existing.SoNamLamViec = model.SoNamLamViec;

            // Xử lý upload file ảnh
            if (cccdTruocFile != null && cccdTruocFile.Length > 0)
            {
                existing.CccdTruoc = await SaveFileAsync(cccdTruocFile, "cccd");
            }
            else if (!string.IsNullOrEmpty(existingCccdTruoc))
            {
                existing.CccdTruoc = existingCccdTruoc;
            }

            if (cccdSauFile != null && cccdSauFile.Length > 0)
            {
                existing.CccdSau = await SaveFileAsync(cccdSauFile, "cccd");
            }
            else if (!string.IsNullOrEmpty(existingCccdSau))
            {
                existing.CccdSau = existingCccdSau;
            }

            if (anhDaiDienFile != null && anhDaiDienFile.Length > 0)
            {
                existing.AnhDaiDien = await SaveFileAsync(anhDaiDienFile, "avatar");
            }
            else if (!string.IsNullOrEmpty(existingAnhDaiDien))
            {
                existing.AnhDaiDien = existingAnhDaiDien;
            }

            await _customerService.UpdateCaNhanAsync(existing);

            TempData["SuccessMessage"] = "Cập nhật thông tin khách hàng thành công!";
            return RedirectToAction("DetailsCaNhan", new { id = id });
        }

        // GET: Customer/EditDoanhNghiep/5 - Form cập nhật khách hàng doanh nghiệp
        public async Task<IActionResult> EditDoanhNghiep(int id)
        {
            // Kiểm tra đăng nhập
            var maNguoiDungStr = HttpContext.Session.GetString("MaNguoiDung");
            if (string.IsNullOrEmpty(maNguoiDungStr))
            {
                return RedirectToAction("Login", "Account");
            }

            var khachHang = await _customerService.GetDoanhNghiepByIdAsync(id);
            if (khachHang == null)
            {
                return NotFound();
            }

            return View(khachHang);
        }

        // POST: Customer/EditDoanhNghiep/5 - Xử lý cập nhật khách hàng doanh nghiệp
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDoanhNghiep(int id, KhachHangDoanhNghiep model,
            IFormFile? cccdTruocFile, IFormFile? cccdSauFile, IFormFile? anhNguoiDaiDienFile,
            List<IFormFile>? anhGiayPhepKinhDoanhFiles, List<IFormFile>? anhBaoCaoTaichinhFiles,
            List<IFormFile>? anhGiayToLienQuanKhacFiles,
            string? existingCccdTruoc, string? existingCccdSau, string? existingAnhNguoiDaiDien,
            string? existingAnhGiayPhepKinhDoanh, string? existingAnhBaoCaoTaichinh, string? existingAnhGiayToLienQuanKhac)
        {
            // Kiểm tra đăng nhập
            var maNguoiDungStr = HttpContext.Session.GetString("MaNguoiDung");
            if (string.IsNullOrEmpty(maNguoiDungStr))
            {
                return RedirectToAction("Login", "Account");
            }

            if (id != model.MaKhachHang)
            {
                return BadRequest();
            }

            var existing = await _customerService.GetDoanhNghiepByIdAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            // VALIDATION: Kiểm tra CCCD người đại diện phải khớp với khách hàng cá nhân/CIC/các doanh nghiệp khác
            if (!string.IsNullOrEmpty(model.SoCccdNguoiDaiDienPhapLuat))
            {
                var soCccdClean = model.SoCccdNguoiDaiDienPhapLuat.Replace(" ", "").Replace("-", "");
                var crossCheckResult = await _customerService.CrossCheckNguoiDaiDienWithCICAsync(
                    soCccdClean, model.NguoiDaiDienPhapLuat, model.NgaySinh, model.GioiTinh, id); // Exclude current DN when editing
                
                if (crossCheckResult.Warnings.Count > 0)
                {
                    // Nếu có cảnh báo từ CIC hoặc khách hàng cá nhân => CHẶN không cho lưu
                    foreach (var warning in crossCheckResult.Warnings)
                    {
                        ModelState.AddModelError(warning.FieldName, warning.Message);
                    }
                    TempData["ErrorMessage"] = "Thông tin người đại diện không khớp với dữ liệu đã có trong hệ thống (Khách hàng cá nhân hoặc CIC). Vui lòng kiểm tra lại!";
                    return View(model);
                }
            }

            // Cập nhật các thông tin (không cho phép thay đổi MST, mã khách hàng)
            existing.TenCongTy = model.TenCongTy;
            existing.SoGiayPhepKinhDoanh = model.SoGiayPhepKinhDoanh;
            existing.NgayCapGiayPhep = model.NgayCapGiayPhep;
            existing.NguoiDaiDienPhapLuat = model.NguoiDaiDienPhapLuat;
            existing.SoCccdNguoiDaiDienPhapLuat = model.SoCccdNguoiDaiDienPhapLuat;
            existing.NgaySinh = model.NgaySinh;
            existing.GioiTinh = model.GioiTinh;
            existing.SoDienThoai = model.SoDienThoai;
            existing.Email = model.Email;
            existing.DiaChi = model.DiaChi;
            existing.ThanhPho = model.ThanhPho;
            existing.Quan = model.Quan;
            existing.Phuong = model.Phuong;
            existing.LinhVucKinhDoanh = model.LinhVucKinhDoanh;
            existing.SoLuongNhanVien = model.SoLuongNhanVien;
            existing.DoanhThuHangNam = model.DoanhThuHangNam;
            existing.TongTaiSan = model.TongTaiSan;
            existing.VonDieuLe = model.VonDieuLe;

            // Xử lý upload file ảnh CCCD
            if (cccdTruocFile != null && cccdTruocFile.Length > 0)
            {
                existing.CccdTruoc = await SaveFileAsync(cccdTruocFile, "cccd");
            }
            else if (!string.IsNullOrEmpty(existingCccdTruoc))
            {
                existing.CccdTruoc = existingCccdTruoc;
            }

            if (cccdSauFile != null && cccdSauFile.Length > 0)
            {
                existing.CccdSau = await SaveFileAsync(cccdSauFile, "cccd");
            }
            else if (!string.IsNullOrEmpty(existingCccdSau))
            {
                existing.CccdSau = existingCccdSau;
            }

            if (anhNguoiDaiDienFile != null && anhNguoiDaiDienFile.Length > 0)
            {
                existing.AnhNguoiDaiDien = await SaveFileAsync(anhNguoiDaiDienFile, "avatar");
            }
            else if (!string.IsNullOrEmpty(existingAnhNguoiDaiDien))
            {
                existing.AnhNguoiDaiDien = existingAnhNguoiDaiDien;
            }

            // Xử lý upload nhiều file giấy phép kinh doanh
            if (anhGiayPhepKinhDoanhFiles != null && anhGiayPhepKinhDoanhFiles.Any(f => f.Length > 0))
            {
                var filePaths = new List<string>();
                foreach (var file in anhGiayPhepKinhDoanhFiles.Where(f => f.Length > 0))
                {
                    var path = await SaveFileAsync(file, "business");
                    filePaths.Add(path);
                }
                existing.AnhGiayPhepKinhDoanh = string.Join(";", filePaths);
            }
            else if (!string.IsNullOrEmpty(existingAnhGiayPhepKinhDoanh))
            {
                existing.AnhGiayPhepKinhDoanh = existingAnhGiayPhepKinhDoanh;
            }

            // Xử lý upload nhiều file báo cáo tài chính
            if (anhBaoCaoTaichinhFiles != null && anhBaoCaoTaichinhFiles.Any(f => f.Length > 0))
            {
                var filePaths = new List<string>();
                foreach (var file in anhBaoCaoTaichinhFiles.Where(f => f.Length > 0))
                {
                    var path = await SaveFileAsync(file, "finance");
                    filePaths.Add(path);
                }
                existing.AnhBaoCaoTaichinh = string.Join(";", filePaths);
            }
            else if (!string.IsNullOrEmpty(existingAnhBaoCaoTaichinh))
            {
                existing.AnhBaoCaoTaichinh = existingAnhBaoCaoTaichinh;
            }

            // Xử lý upload nhiều file giấy tờ liên quan khác
            if (anhGiayToLienQuanKhacFiles != null && anhGiayToLienQuanKhacFiles.Any(f => f.Length > 0))
            {
                var filePaths = new List<string>();
                foreach (var file in anhGiayToLienQuanKhacFiles.Where(f => f.Length > 0))
                {
                    var path = await SaveFileAsync(file, "documents");
                    filePaths.Add(path);
                }
                existing.AnhGiayToLienQuanKhac = string.Join(";", filePaths);
            }
            else if (!string.IsNullOrEmpty(existingAnhGiayToLienQuanKhac))
            {
                existing.AnhGiayToLienQuanKhac = existingAnhGiayToLienQuanKhac;
            }

            await _customerService.UpdateDoanhNghiepAsync(existing);

            TempData["SuccessMessage"] = "Cập nhật thông tin doanh nghiệp thành công!";
            return RedirectToAction("DetailsDoanhNghiep", new { id = id });
        }
    }
}
