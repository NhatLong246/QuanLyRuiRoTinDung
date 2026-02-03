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

        // GET: Customer - Chọn loại khách hàng
        public IActionResult Index()
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
        public async Task<IActionResult> CheckPhoneExists(string phone)
        {
            if (string.IsNullOrEmpty(phone))
            {
                return Json(new { exists = false });
            }

            var exists = await _customerService.CheckPhoneExistsAsync(phone);
            return Json(new { exists = exists });
        }

        // AJAX: Kiểm tra email trùng
        [HttpGet]
        public async Task<IActionResult> CheckEmailExists(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return Json(new { exists = false });
            }

            var exists = await _customerService.CheckEmailExistsAsync(email);
            return Json(new { exists = exists });
        }

        // AJAX: Kiểm tra CCCD/CMND trùng
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
    }
}
