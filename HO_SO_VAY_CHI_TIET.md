# PHÂN TÍCH CHI TIẾT HỒ SƠ VAY CÁ NHÂN VÀ DOANH NGHIỆP

## 📋 TỔNG QUAN

Dựa trên phân tích database, hệ thống sử dụng **1 bảng chung `KhoanVay`** để lưu thông tin khoản vay cho cả cá nhân và doanh nghiệp, phân biệt bằng trường `LoaiKhachHang` (CaNhan hoặc DoanhNghiep).

---

## 🏦 HỒ SƠ VAY CÁ NHÂN

### 1. THÔNG TIN TỪ BẢNG `KhoanVay` (Bảng chung)

| Trường | Kiểu dữ liệu | Mô tả | Bắt buộc |
|--------|--------------|-------|-----------|
| `MaKhoanVay_Code` | NVARCHAR(20) | Mã khoản vay: LOAN0001, LOAN0002 | ✅ |
| `LoaiKhachHang` | NVARCHAR(20) | = "CaNhan" | ✅ |
| `MaKhachHang` | INT | FK đến `KhachHang_CaNhan.MaKhachHang` | ✅ |
| `MaLoaiVay` | INT | FK đến `LoaiKhoanVay.MaLoaiVay` | ✅ |
| `SoTienVay` | DECIMAL(18,2) | Số tiền vay yêu cầu (VNĐ) | ✅ |
| `LaiSuat` | DECIMAL(5,2) | Lãi suất áp dụng (%/năm) | ✅ |
| `KyHanVay` | INT | Kỳ hạn vay (tháng) | ✅ |
| `HinhThucTraNo` | NVARCHAR(50) | Trả góp đều, Trả gốc cuối kỳ, Trả gốc lãi cuối kỳ | ❌ |
| `SoTienTraHangThang` | DECIMAL(18,2) | Số tiền phải trả mỗi tháng (tính tự động nếu trả góp đều) | ❌ |
| `MucDichVay` | NVARCHAR(200) | Mục đích sử dụng tiền vay | ❌ |
| `CoTaiSanDamBao` | BIT | 1: Có tài sản, 0: Không có (vay tín chấp) | ❌ (Default: 0) |
| `NgayNopHoSo` | DATETIME | Ngày nộp hồ sơ vay | ✅ (Default: GETDATE()) |
| `TrangThaiKhoanVay` | NVARCHAR(50) | Đang xử lý, Chờ bổ sung, Đã duyệt, Từ chối, Đã giải ngân, Đang trả nợ, Đã thanh toán, Quá hạn | ✅ (Default: "Đang xử lý") |
| `MaNhanVienTinDung` | INT | FK đến `NguoiDung.MaNguoiDung` (nhân viên phụ trách) | ✅ |
| `MucDoRuiRo` | NVARCHAR(20) | Thấp, Trung bình, Cao, Rất cao | ❌ |
| `DiemRuiRo` | DECIMAL(5,2) | Điểm đánh giá rủi ro (0-100) | ❌ |
| `XepHangRuiRo` | NVARCHAR(10) | AAA, AA, A, BBB, BB, B, CCC, CC, C, D | ❌ |
| `GhiChu` | NVARCHAR(1000) | Ghi chú bổ sung | ❌ |
| `DuongDanHoSo` | NVARCHAR(500) | Đường dẫn thư mục lưu hồ sơ vay (file đính kèm) | ❌ |
| `NguoiTao` | INT | FK đến `NguoiDung.MaNguoiDung` | ✅ |
| `NgayTao` | DATETIME | Ngày tạo hồ sơ | ✅ (Default: GETDATE()) |

**Các trường khác (tự động tính hoặc cập nhật sau):**
- `NgayPheDuyet`, `NgayGiaiNgan`, `NgayBatDauTra`, `NgayDaoHan`
- `SoDuGocConLai`, `SoDuLaiConLai`, `TongDaThanhToan`, `SoKyDaTra`, `SoKyConLai`
- `TongDuNo`, `TyLeHoanThanh`, `SoNgayQuaHan`, `MaPhanLoaiNo`, `NgayPhanLoaiNo`
- `NguoiPheDuyet`, `CapPheDuyet`, `LyDoTuChoi`

### 2. THÔNG TIN TỪ BẢNG `KhachHang_CaNhan` (Tham chiếu)

| Trường | Mô tả | Hiển thị trong form |
|--------|-------|---------------------|
| `HoTen` | Họ và tên đầy đủ | ✅ Hiển thị (readonly) |
| `MaKhachHang_Code` | Mã khách hàng: CN0001 | ✅ Hiển thị (readonly) |
| `NgaySinh` | Ngày tháng năm sinh | ✅ Hiển thị (readonly) |
| `GioiTinh` | Nam, Nữ | ✅ Hiển thị (readonly) |
| `SoCMND` | Số CMND/CCCD | ✅ Hiển thị (readonly) |
| `SoDienThoai` | Số điện thoại | ✅ Hiển thị (readonly) |
| `Email` | Email | ✅ Hiển thị (readonly) |
| `DiaChi`, `ThanhPho`, `Quan`, `Phuong` | Địa chỉ | ✅ Hiển thị (readonly) |
| `TinhTrangHonNhan` | Độc thân, Đã kết hôn, Ly hôn | ✅ Hiển thị (readonly) |
| `NgheNghiep` | Nghề nghiệp hiện tại | ✅ Hiển thị (readonly) |
| `ThuNhapHangThang` | Thu nhập trung bình hàng tháng (VNĐ) | ✅ Hiển thị (readonly) |
| `TenCongTy` | Tên công ty đang làm việc | ✅ Hiển thị (readonly) |
| `SoNamLamViec` | Số năm làm việc tại công ty | ✅ Hiển thị (readonly) |
| `DiemTinDung` | Điểm tín dụng (0-1000) | ✅ Hiển thị (readonly) |
| `XepHangTinDung` | Xếp hạng: AAA, AA, A, BBB, BB, B, CCC, CC, C, D | ✅ Hiển thị (readonly) |
| `AnhDaiDien` | Ảnh đại diện | ✅ Hiển thị (readonly) |

### 3. THÔNG TIN TỪ BẢNG `ThongTin_CIC` (Tra cứu tự động)

| Trường | Mô tả | Hiển thị trong form |
|--------|-------|---------------------|
| `HoTen` | Họ tên trên CIC | ✅ Hiển thị (readonly) |
| `TongSoKhoanVayCIC` | Tổng số khoản vay trên CIC | ✅ Hiển thị (readonly) |
| `TongDuNoCIC` | Tổng dư nợ trên CIC | ✅ Hiển thị (readonly) |
| `SoKhoanVayDangVayCIC` | Số khoản vay đang vay | ✅ Hiển thị (readonly) |
| `SoKhoanVayNoXauCIC` | Số khoản vay nợ xấu | ✅ Hiển thị (readonly) |
| `DiemTinDungCic` | Điểm tín dụng CIC | ✅ Hiển thị (readonly) |
| `XepHangTinDungCIC` | Xếp hạng tín dụng CIC | ✅ Hiển thị (readonly) |
| `MucDoRuiRo` | Mức độ rủi ro | ✅ Hiển thị (readonly) |
| `KhuyenNghiChoVay` | Khuyến nghị cho vay | ✅ Hiển thị (readonly) |
| `DanhSachToChucTinDung` | Danh sách tổ chức tín dụng đã vay | ✅ Hiển thị (readonly) |

### 4. THÔNG TIN TỪ BẢNG `KhoanVay_TaiSan` (Nếu có tài sản đảm bảo)

| Trường | Mô tả | Nhập trong form |
|--------|-------|-----------------|
| `MaTaiSan` | FK đến `TaiSanDamBao.MaTaiSan` | ✅ Chọn từ danh sách tài sản |
| `GiaTriDinhGiaTaiThoiDiemVay` | Giá trị định giá tại thời điểm vay | ✅ Nhập |
| `TyLeTheChap` | % giá trị tài sản trong tổng đảm bảo | ✅ Nhập |
| `NgayTheChap` | Ngày thế chấp | ✅ Nhập |
| `GhiChu` | Ghi chú về tài sản | ❌ |

**Lưu ý:** Có thể có nhiều tài sản đảm bảo cho 1 khoản vay (quan hệ nhiều-nhiều).

---

## 🏢 HỒ SƠ VAY DOANH NGHIỆP

### 1. THÔNG TIN TỪ BẢNG `KhoanVay` (Bảng chung)

**Giống hệt với hồ sơ vay cá nhân**, chỉ khác:
- `LoaiKhachHang` = "DoanhNghiep"
- `MaKhachHang` = FK đến `KhachHang_DoanhNghiep.MaKhachHang`

### 2. THÔNG TIN TỪ BẢNG `KhachHang_DoanhNghiep` (Tham chiếu)

| Trường | Mô tả | Hiển thị trong form |
|--------|-------|---------------------|
| `TenCongTy` | Tên đầy đủ công ty | ✅ Hiển thị (readonly) |
| `MaKhachHang_Code` | Mã doanh nghiệp: DN0001 | ✅ Hiển thị (readonly) |
| `MaSoThue` | Mã số thuế | ✅ Hiển thị (readonly) |
| `SoGiayPhepKinhDoanh` | Số giấy phép đăng ký kinh doanh | ✅ Hiển thị (readonly) |
| `NgayCapGiayPhep` | Ngày cấp giấy phép | ✅ Hiển thị (readonly) |
| `NgayDangKy` | Ngày đăng ký thành lập công ty | ✅ Hiển thị (readonly) |
| `NguoiDaiDienPhapLuat` | Họ tên người đại diện pháp luật | ✅ Hiển thị (readonly) |
| `SoDienThoai` | Số điện thoại công ty | ✅ Hiển thị (readonly) |
| `Email` | Email công ty | ✅ Hiển thị (readonly) |
| `DiaChi`, `ThanhPho`, `Quan`, `Phuong` | Địa chỉ trụ sở chính | ✅ Hiển thị (readonly) |
| `LinhVucKinhDoanh` | Lĩnh vực kinh doanh chính | ✅ Hiển thị (readonly) |
| `SoLuongNhanVien` | Tổng số lao động | ✅ Hiển thị (readonly) |
| `DoanhThuHangNam` | Doanh thu năm gần nhất (VNĐ) | ✅ Hiển thị (readonly) |
| `TongTaiSan` | Tổng tài sản theo báo cáo tài chính (VNĐ) | ✅ Hiển thị (readonly) |
| `VonDieuLe` | Vốn điều lệ đã đăng ký (VNĐ) | ✅ Hiển thị (readonly) |
| `DiemTinDung` | Điểm tín dụng doanh nghiệp (0-1000) | ✅ Hiển thị (readonly) |
| `XepHangTinDung` | Xếp hạng tín nhiệm: AAA -> D | ✅ Hiển thị (readonly) |
| `AnhNguoiDaiDien` | Ảnh người đại diện pháp luật | ✅ Hiển thị (readonly) |
| `NgaySinh` | Ngày sinh người đại diện | ✅ Hiển thị (readonly) |
| `GioiTinh` | Giới tính người đại diện | ✅ Hiển thị (readonly) |

### 3. THÔNG TIN TỪ BẢNG `ThongTin_CIC` (Tra cứu tự động)

**Giống với hồ sơ vay cá nhân**, nhưng tra cứu theo `MaSoThue` thay vì `SoCMND_CCCD`.

### 4. THÔNG TIN TỪ BẢNG `KhoanVay_TaiSan` (Nếu có tài sản đảm bảo)

**Giống hệt với hồ sơ vay cá nhân.**

---

## 📝 TÓM TẮT SỰ KHÁC BIỆT

### Điểm giống nhau:
- ✅ Cùng sử dụng bảng `KhoanVay` để lưu thông tin khoản vay
- ✅ Các trường trong `KhoanVay` giống nhau cho cả 2 loại
- ✅ Cùng tra cứu CIC (chỉ khác cách tra cứu: CMND vs MST)
- ✅ Cùng quản lý tài sản đảm bảo qua bảng `KhoanVay_TaiSan`

### Điểm khác nhau:
- ❌ **Nguồn thông tin khách hàng:**
  - Cá nhân: Lấy từ `KhachHang_CaNhan` (thu nhập cá nhân, nghề nghiệp, tình trạng hôn nhân)
  - Doanh nghiệp: Lấy từ `KhachHang_DoanhNghiep` (doanh thu, vốn điều lệ, số lượng nhân viên, lĩnh vực kinh doanh)
- ❌ **Cách tra cứu CIC:**
  - Cá nhân: Tra theo `SoCMND_CCCD`
  - Doanh nghiệp: Tra theo `MaSoThue`
- ❌ **Thông tin hiển thị:**
  - Cá nhân: Tập trung vào thu nhập cá nhân, nghề nghiệp, tình trạng hôn nhân
  - Doanh nghiệp: Tập trung vào doanh thu, vốn điều lệ, quy mô doanh nghiệp, người đại diện pháp luật

---

## 🎯 KẾT LUẬN VÀ ĐỀ XUẤT

### Form tạo hồ sơ vay cần:

#### **Phần chung (cho cả 2 loại):**
1. **Thông tin khoản vay:**
   - Số tiền vay (VNĐ) - `SoTienVay` ✅
   - Lãi suất (%/năm) - `LaiSuat` ✅
   - Kỳ hạn vay (tháng) - `KyHanVay` ✅
   - Loại hình vay - `MaLoaiVay` (dropdown từ `LoaiKhoanVay`) ✅
   - Hình thức trả nợ - `HinhThucTraNo` (dropdown) ✅
   - Mục đích vay - `MucDichVay` (textarea) ✅

2. **Tài sản đảm bảo:**
   - Checkbox "Có tài sản đảm bảo" - `CoTaiSanDamBao` ✅
   - Nếu có: Cho phép chọn nhiều tài sản từ danh sách `TaiSanDamBao`
   - Nhập giá trị định giá, tỷ lệ thế chấp cho từng tài sản

3. **Hồ sơ đính kèm:**
   - Upload file (lưu đường dẫn vào `DuongDanHoSo` hoặc bảng riêng nếu cần)

4. **Ghi chú:**
   - `GhiChu` (textarea)

#### **Phần riêng - Hiển thị thông tin khách hàng (readonly):**

**Cá nhân:**
- Thông tin cá nhân: Họ tên, ngày sinh, giới tính, CMND, SĐT, Email, địa chỉ
- Thông tin công việc: Nghề nghiệp, thu nhập hàng tháng, tên công ty, số năm làm việc
- Tình trạng hôn nhân
- Điểm tín dụng và xếp hạng

**Doanh nghiệp:**
- Thông tin doanh nghiệp: Tên công ty, MST, giấy phép kinh doanh, ngày thành lập
- Thông tin người đại diện: Họ tên, ngày sinh, giới tính, ảnh
- Thông tin tài chính: Doanh thu hàng năm, tổng tài sản, vốn điều lệ
- Quy mô: Số lượng nhân viên, lĩnh vực kinh doanh
- Điểm tín dụng và xếp hạng

#### **Phần chung - Thông tin CIC (readonly):**
- Hiển thị kết quả tra cứu CIC tự động
- Điểm tín dụng CIC, xếp hạng, mức độ rủi ro
- Khuyến nghị cho vay
- Danh sách tổ chức tín dụng đã vay

---

## ✅ XÁC NHẬN

Sau khi xem xét tài liệu này, vui lòng xác nhận:
- [ ] Đồng ý với cấu trúc hồ sơ vay như trên
- [ ] Cần bổ sung/thay đổi thông tin nào không?
- [ ] Có cần tạo bảng riêng cho hồ sơ vay cá nhân và doanh nghiệp không, hay giữ nguyên 1 bảng `KhoanVay`?

Sau khi xác nhận, tôi sẽ tiến hành thiết kế lại giao diện form tạo hồ sơ vay với đầy đủ các trường trên.
