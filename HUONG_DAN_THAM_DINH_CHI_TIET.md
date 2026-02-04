# 📋 HƯỚNG DẪN CHI TIẾT CHỨC NĂNG THẨM ĐỊNH TÀI SẢN

## 🎯 Mục đích
Hệ thống thẩm định tài sản giúp nhân viên quản lý rủi ro đánh giá giá trị tài sản đảm bảo của khách hàng dựa trên:
- **Giá tham chiếu thị trường** (đất đai, xe cộ, vàng)
- **Tỷ lệ thẩm định** (% giá trị được chấp nhận)
- **Hồ sơ file đính kèm** (ảnh chụp, giấy tờ pháp lý)

---

## 🔄 LUỒNG HOẠT ĐỘNG CHÍNH

### 📌 **Bước 1: Xem danh sách khoản vay cần thẩm định**

**Vị trí:** `/QuanLyRuiRo/ThamDinhRuiRoTinDung`

**Màn hình hiển thị:**
```
┌─────────────────────────────────────────────────────┐
│  Khoản vay 1: KV-001 - Nguyễn Văn A                │
│  Số tiền: 500,000,000 VNĐ                          │
│  Trạng thái: Đang xử lý                            │
│  [Xem chi tiết]                                    │
└─────────────────────────────────────────────────────┘
```

**Dữ liệu load từ:**
- API: `GET /QuanLyRuiRo/GetDanhSachKhoanVay`
- Service: `GetKhoanVayCanThamDinhAsync()`
- Database: Bảng `KhoanVay` JOIN `KhachHangCaNhan` hoặc `KhachHangDoanhNghiep`

---

### 📌 **Bước 2: Xem chi tiết khoản vay**

**Action:** Click nút "Xem chi tiết" → Modal hiện lên

**JavaScript function:** `displayAssessmentDetail(maKhoanVay)`

**Modal gồm 3 phần chính:**

#### **2.1. Thông tin khoản vay**
```javascript
GET /QuanLyRuiRo/GetKhoanVayDetail?maKhoanVay=123
→ Service: GetKhoanVayFullDetailAsync(123)
→ Trả về: KhoanVayDetailViewModel
```

Hiển thị:
- Thông tin khách hàng (Cá nhân/Doanh nghiệp)
- Số tiền vay, lãi suất, kỳ hạn
- Mục đích vay, hình thức trả nợ

#### **2.2. Hồ sơ File đính kèm** ✨
```javascript
GET /QuanLyRuiRo/GetFileDinhKem?maKhoanVay=123
→ Service: GetFileDinhKemByKhoanVayAsync(123)
→ Database: HoSoVay_FileDinhKem
→ Hiển thị: Grid ảnh thumbnail + file PDF/DOC
```

**Tính năng:**
- **Ảnh (JPG, PNG, GIF):** Hiển thị thumbnail 200px
- **Click vào ảnh:** Xem fullscreen với nền đen
- **File khác (PDF, DOC, XLS):** Hiển thị icon + tên + kích thước
- **Nút "Tải xuống"** cho mỗi file

**Ví dụ HTML sinh ra:**
```html
<div class="row">
  <div class="col-md-3">
    <img src="/uploads/giay-to-nha.jpg" class="img-thumbnail" 
         onclick="viewImageFullscreen('/uploads/giay-to-nha.jpg', 'Giấy tờ nhà')">
    <div>giay-to-nha.jpg</div>
    <div>46.5 KB • 3/2/2026</div>
    <a href="/uploads/giay-to-nha.jpg" download>Tải xuống</a>
  </div>
</div>
```

#### **2.3. Danh sách tài sản đảm bảo**
```javascript
→ Hiển thị từ KhoanVayDetailViewModel.TaiSanDamBaos[]
→ Mỗi tài sản có nút [Thẩm định]
```

---

### 📌 **Bước 3: Thẩm định tài sản**

**Action:** Click nút "Thẩm định" trên tài sản → Modal thẩm định xuất hiện

**JavaScript function:** `thamDinhTaiSan(maTaiSan)`

**Form thẩm định gồm:**

#### **3.1. Thông tin tài sản**
```
Mã tài sản: [Tự động điền]
Tên tài sản: [Tự động điền từ TaiSanDamBao]
Loại tài sản: [Dropdown: Đất | Xe cộ | Vàng]
```

#### **3.2. Nhập thông tin theo loại tài sản**

**Nếu chọn "Đất":**
```html
<select id="thamDinhQuan">
  <option value="Quận 1">Quận 1</option>
  <option value="Bình Thạnh">Bình Thạnh</option>
  ...
</select>
<input type="number" id="thamDinhDienTich" placeholder="Diện tích (m²)">
<input type="number" id="thamDinhGiaTriKhaiBao" placeholder="Giá trị khai báo">
```

**Nếu chọn "Xe cộ":**
```html
<select id="thamDinhHangXe" onchange="loadDongXeOptions()">
  <option value="Honda">Honda</option>
  <option value="Toyota">Toyota</option>
  ...
</select>
<select id="thamDinhDongXe">
  <option value="City">City</option>
  <option value="Civic">Civic</option>
</select>
<input type="number" id="thamDinhNamSanXuat" min="1990" max="2025">
<input type="number" id="thamDinhGiaTriKhaiBao">
```

**Load dữ liệu dropdown:**
```javascript
// Load quận
GET /QuanLyRuiRo/GetGiaTriThamChieu?loaiTaiSan=Đất
→ Service: GetGiaTriThamChieuAsync("Đất", null)
→ Database: SELECT DISTINCT Quan FROM GiaTriTaiSan_ThamChieu

// Load hãng xe
GET /QuanLyRuiRo/GetGiaTriThamChieu?loaiTaiSan=Xe cộ
→ Database: SELECT DISTINCT HangXe FROM GiaTriTaiSan_ThamChieu

// Load dòng xe theo hãng
GET /QuanLyRuiRo/GetGiaTriThamChieu?loaiTaiSan=Xe cộ&keyword=Honda
→ Database: SELECT DISTINCT DongXe WHERE HangXe = 'Honda'
```

---

### 📌 **Bước 4: Tra cứu giá tham chiếu**

**Action:** Click nút "Tra cứu giá tham chiếu"

**JavaScript function:** `traCuuGiaThamChieu()`

**Request gửi đi:**
```javascript
POST /QuanLyRuiRo/TimGiaTriThamChieu
Headers: { 'Content-Type': 'application/json' }
Body: {
  "loaiTaiSan": "Đất",
  "quan": "Bình Thạnh",
  // HOẶC cho xe cộ:
  "loaiTaiSan": "Xe cộ",
  "hangXe": "Honda",
  "dongXe": "City",
  "namSanXuat": 2024
}
```

**Backend xử lý:**
```csharp
[HttpPost]
public async Task<IActionResult> TimGiaTriThamChieu([FromBody] TimGiaTriRequest request)
{
    var data = await _ruiRoService.TimGiaTriThamChieuAsync(
        request.LoaiTaiSan, 
        request.Quan, 
        request.HangXe, 
        request.DongXe, 
        request.NamSanXuat
    );
    return Json(new { success = true, data = data });
}
```

**Service logic:**
```csharp
public async Task<GiaTriTaiSanThamChieu?> TimGiaTriThamChieuAsync(...)
{
    var query = _context.GiaTriTaiSanThamChieus
        .Where(g => g.LoaiTaiSan == loaiTaiSan && g.TrangThaiHoatDong == true);

    if (loaiTaiSan == "Đất" && !string.IsNullOrEmpty(quan))
        query = query.Where(g => g.Quan == quan);
    
    if (loaiTaiSan == "Xe cộ") {
        if (!string.IsNullOrEmpty(hangXe))
            query = query.Where(g => g.HangXe == hangXe);
        if (!string.IsNullOrEmpty(dongXe))
            query = query.Where(g => g.DongXe == dongXe);
        if (namSanXuat.HasValue) {
            // Tìm năm chính xác hoặc năm gần nhất
            var exactMatch = await query.FirstOrDefaultAsync(g => g.NamSanXuat == namSanXuat);
            if (exactMatch != null) return exactMatch;
            
            var allMatches = await query.ToListAsync();
            return allMatches
                .OrderBy(g => Math.Abs((g.NamSanXuat ?? 0) - namSanXuat.Value))
                .FirstOrDefault();
        }
    }
    
    return await query.FirstOrDefaultAsync();
}
```

**Ví dụ dữ liệu trả về (Đất):**
```json
{
  "success": true,
  "data": {
    "maGiaTri": 12,
    "loaiTaiSan": "Đất",
    "thanhPho": "Hồ Chí Minh",
    "quan": "Bình Thạnh",
    "giaTriThamChieu": 80000000,
    "tyLeThamDinh": 70,
    "trangThaiHoatDong": true
  }
}
```

**JavaScript tính toán:**
```javascript
if (loaiTaiSan === 'Đất') {
    const dienTich = 30; // m²
    const giaTriThamChieuDonVi = 80,000,000; // VNĐ/m²
    const giaTriThamChieuTotal = 80,000,000 * 30 = 2,400,000,000;
    const tyLeThamDinh = 70%; // Chỉ chấp nhận 70%
    const giaTriThamDinh = 2,400,000,000 * 0.7 = 1,680,000,000;
}
```

**Hiển thị kết quả:**
```html
<table class="table table-bordered">
  <tr><th>Quận:</th><td>Bình Thạnh</td></tr>
  <tr><th>Giá tham chiếu (VNĐ/m²):</th><td><strong>80,000,000</strong></td></tr>
  <tr><th>Diện tích (m²):</th><td>30</td></tr>
  <tr class="bg-light">
    <th>Tổng giá trị tham chiếu:</th>
    <td class="text-primary"><strong>2,400,000,000</strong></td>
  </tr>
  <tr><th>Tỷ lệ thẩm định:</th><td>70%</td></tr>
  <tr class="table-success">
    <th>Giá trị thẩm định:</th>
    <td class="text-success"><strong>1,680,000,000</strong></td>
  </tr>
</table>
```

---

### 📌 **Bước 5: Lưu kết quả thẩm định**

**Action:** Click nút "Lưu kết quả"

**JavaScript function:** `luuKetQuaThamDinh()`

**Tính toán chênh lệch:**
```javascript
const giaTriKhaiBao = 2,000,000,000; // Khách hàng khai
const giaTriThamDinh = 1,680,000,000; // Hệ thống tính

const chenhLech = 2,000,000,000 - 1,680,000,000 = 320,000,000;
const tyLeChenhLech = (320,000,000 / 1,680,000,000) * 100 = 19.05%;

// Cảnh báo nếu chênh lệch > 20%
if (Math.abs(tyLeChenhLech) > 20) {
    ghiChu += "⚠️ Cảnh báo: Chênh lệch lớn hơn 20%, cần xem xét kỹ!";
}
```

**Request POST:**
```javascript
POST /QuanLyRuiRo/LuuKetQuaThamDinh
Headers: { 'Content-Type': 'application/json' }
Body: {
  "maTaiSan": 5,
  "giaTriThamChieu": 2400000000,
  "giaTriThamDinh": 1680000000,
  "tyLeThamDinh": 70,
  "ghiChu": "Loại tài sản: Đất. Quận: Bình Thạnh, Diện tích: 30m². Giá khai báo: 2,000,000,000. Chênh lệch: 19.05%."
}
```

**Backend Controller:**
```csharp
[HttpPost]
public async Task<IActionResult> LuuKetQuaThamDinh([FromBody] ThamDinhRequest request)
{
    // 1. Kiểm tra session
    var maNguoiDung = HttpContext.Session.GetString("MaNguoiDung");
    if (string.IsNullOrEmpty(maNguoiDung)) {
        return Json(new { success = false, message = "Phiên đăng nhập hết hạn" });
    }

    // 2. Gọi service lưu
    var result = await _ruiRoService.LuuKetQuaThamDinhAsync(
        request.MaTaiSan,
        request.GiaTriThamChieu,
        request.GiaTriThamDinh,
        request.TyLeThamDinh,
        request.GhiChu,
        int.Parse(maNguoiDung)
    );

    return Json(new { 
        success = result, 
        message = result ? "Đã lưu kết quả thẩm định" : "Không thể lưu kết quả" 
    });
}
```

**Backend Service:**
```csharp
public async Task<bool> LuuKetQuaThamDinhAsync(
    int maTaiSan, 
    decimal giaTriThamChieu, 
    decimal giaTriThamDinh, 
    decimal tyLeThamDinh, 
    string? ghiChu, 
    int nguoiThamDinh)
{
    // 1. Tìm tài sản
    var taiSan = await _context.TaiSanDamBaos.FindAsync(maTaiSan);
    if (taiSan == null) return false;

    // 2. Cập nhật thông tin thẩm định
    taiSan.GiaTriThiTruong = giaTriThamChieu;
    taiSan.GiaTriDinhGia = giaTriThamDinh;
    taiSan.NgayDinhGia = DateOnly.FromDateTime(DateTime.Now);
    taiSan.DonViDinhGia = "Phòng Quản lý Rủi ro";
    taiSan.NgayCapNhat = DateTime.Now;
    taiSan.NguoiCapNhat = nguoiThamDinh;

    // 3. Lưu lịch sử định giá (Audit Trail)
    var lichSu = new LichSuDinhGiaTaiSan
    {
        MaTaiSan = maTaiSan,
        GiaTriCu = taiSan.GiaTriDinhGia, // Giá trị cũ (nếu đã có)
        GiaTriMoi = giaTriThamDinh,       // Giá trị mới
        ChenhLech = giaTriThamDinh - (taiSan.GiaTriDinhGia ?? 0),
        TyLeThayDoi = taiSan.GiaTriDinhGia.HasValue && taiSan.GiaTriDinhGia > 0 
            ? (giaTriThamDinh - taiSan.GiaTriDinhGia.Value) / taiSan.GiaTriDinhGia.Value * 100 
            : null,
        LyDoDinhGia = $"Thẩm định theo giá tham chiếu. Tỷ lệ: {tyLeThamDinh:F2}%",
        NgayDinhGia = DateOnly.FromDateTime(DateTime.Now),
        NguoiDinhGia = nguoiThamDinh.ToString(),
        DonViDinhGia = "Phòng Quản lý Rủi ro",
        PhuongPhapDinhGia = "Phương pháp so sánh giá tham chiếu",
        FileDinhGia = ghiChu,
        NgayTao = DateTime.Now,
        NguoiTao = nguoiThamDinh
    };

    _context.LichSuDinhGiaTaiSans.Add(lichSu);
    await _context.SaveChangesAsync();

    return true;
}
```

**Database Updates:**

**Bảng `TaiSanDamBao`:**
```sql
UPDATE TaiSanDamBao
SET GiaTriThiTruong = 2400000000,
    GiaTriDinhGia = 1680000000,
    NgayDinhGia = '2026-02-03',
    DonViDinhGia = 'Phòng Quản lý Rủi ro',
    NgayCapNhat = '2026-02-03 14:30:00',
    NguoiCapNhat = 5
WHERE MaTaiSan = 5;
```

**Bảng `LichSu_DinhGiaTaiSan`:**
```sql
INSERT INTO LichSu_DinhGiaTaiSan
(MaTaiSan, GiaTriCu, GiaTriMoi, ChenhLech, TyLeThayDoi, LyDoDinhGia, 
 NgayDinhGia, NguoiDinhGia, DonViDinhGia, PhuongPhapDinhGia, FileDinhGia)
VALUES
(5, 1500000000, 1680000000, 180000000, 12.00, 
 'Thẩm định theo giá tham chiếu. Tỷ lệ: 70.00%',
 '2026-02-03', '5', 'Phòng Quản lý Rủi ro', 
 'Phương pháp so sánh giá tham chiếu',
 'Loại tài sản: Đất. Quận: Bình Thạnh...');
```

---

### 📌 **Bước 6: Hiển thị kết quả và cập nhật UI**

**JavaScript xử lý response:**
```javascript
if (result.success) {
    // 1. Hiển thị thông báo thành công trong modal
    const resultHtml = `
        <div class="alert alert-success mt-3">
            <h5>✅ Đã lưu kết quả thẩm định!</h5>
            <ul class="mb-0">
                <li>Giá trị tham chiếu: <strong>2,400,000,000 VNĐ</strong></li>
                <li>Giá trị thẩm định: <strong>1,680,000,000 VNĐ</strong> (70%)</li>
                <li>Giá trị khai báo: <strong>2,000,000,000 VNĐ</strong></li>
                <li>Chênh lệch: <strong class="text-danger">320,000,000 VNĐ (19.05%)</strong></li>
            </ul>
        </div>
    `;
    $('#thamDinhResult_' + maTaiSan).html(resultHtml);
    
    // 2. Cập nhật item tài sản với viền xanh
    $('#taiSanItem_' + maTaiSan + ' .card').addClass('border-success');
    $('#taiSanItem_' + maTaiSan + ' .card-body').append(`
        <div class="mt-2 p-2 bg-success bg-opacity-10 rounded">
            <small class="text-success">
                <strong>✓ Đã thẩm định:</strong> 1,680,000,000 VNĐ (Tỷ lệ: 70%)
            </small>
        </div>
    `);
    
    // 3. Tự động đóng modal sau 2 giây
    setTimeout(() => {
        closeThamDinhModal();
    }, 2000);
}
```

**Kết quả hiển thị:**
```
┌───────────────────────────────────────────────┐
│ ✅ Tài sản 1: Nhà đất Bình Thạnh              │
│ [BORDER XANH THÀNH CÔNG]                      │
│                                               │
│ Loại tài sản: Đất                            │
│ Giá trị ghi nhận: 1,500,000,000 VNĐ          │
│ Mô tả: Đất thổ cư 30m²                       │
│                                               │
│ ✓ Đã thẩm định: 1,680,000,000 VNĐ (Tỷ lệ: 70%)│
│                                               │
│ [Thẩm định] (disabled)                       │
└───────────────────────────────────────────────┘
```

---

## 📊 SƠ ĐỒ LUỒNG DỮ LIỆU

```
┌─────────────────┐
│   USER (QLRR)   │
└────────┬────────┘
         │
         │ 1. Click "Xem chi tiết"
         ▼
┌──────────────────────────────────┐
│  JavaScript: displayAssessment   │
│  Detail(maKhoanVay)              │
└────────┬─────────────────────────┘
         │
         │ 2. Gọi 3 API song song
         ├────────────────┬─────────────────┬──────────────────┐
         ▼                ▼                 ▼                  ▼
    GetKhoanVay      GetFileDinhKem    GetTaiSanList    GetDanhGiaRuiRo
    Detail           ByKhoanVay        (trong Detail)   (trong Detail)
         │                │                 │                  │
         ▼                ▼                 ▼                  ▼
    ┌─────────────────────────────────────────────────────────┐
    │          Controller: QuanLyRuiRoController              │
    └────────┬────────────────────────────────────────────────┘
             │
             │ 3. Delegate to Service Layer
             ▼
    ┌─────────────────────────────────────────────────────────┐
    │          Service: RuiRoService                          │
    │  - GetKhoanVayFullDetailAsync()                         │
    │  - GetFileDinhKemByKhoanVayAsync()                      │
    └────────┬────────────────────────────────────────────────┘
             │
             │ 4. Query Database
             ▼
    ┌─────────────────────────────────────────────────────────┐
    │          Database: SQL Server                           │
    │  Tables:                                                │
    │  - KhoanVay                                             │
    │  - KhachHangCaNhan / KhachHangDoanhNghiep              │
    │  - TaiSanDamBao                                         │
    │  - HoSoVay_FileDinhKem                                  │
    │  - KhoanVay_TaiSan (junction table)                     │
    └─────────────────────────────────────────────────────────┘
             │
             │ 5. Return ViewModel
             ▼
    ┌─────────────────────────────────────────────────────────┐
    │          JavaScript renders UI:                         │
    │  ┌──────────────────────────────────────────┐           │
    │  │ Hồ sơ File đính kèm                      │           │
    │  │ [📷 ảnh 1] [📷 ảnh 2] [📄 PDF]           │           │
    │  └──────────────────────────────────────────┘           │
    │  ┌──────────────────────────────────────────┐           │
    │  │ Tài sản 1: Nhà đất          [Thẩm định] │           │
    │  │ Tài sản 2: Xe ô tô          [Thẩm định] │           │
    │  └──────────────────────────────────────────┘           │
    └─────────────────────────────────────────────────────────┘
             │
             │ 6. User clicks "Thẩm định"
             ▼
    ┌─────────────────────────────────────────────────────────┐
    │  Modal: Thẩm định Tài sản                               │
    │  - Chọn loại: [Đất ▼]                                   │
    │  - Quận: [Bình Thạnh ▼]                                 │
    │  - Diện tích: [30] m²                                   │
    │  - Giá khai báo: [2,000,000,000]                        │
    │                                                         │
    │  [Tra cứu giá tham chiếu] [Lưu kết quả]                │
    └─────────────────────────────────────────────────────────┘
             │
             │ 7. Click "Tra cứu giá tham chiếu"
             ▼
    POST /QuanLyRuiRo/TimGiaTriThamChieu
    Body: { loaiTaiSan: "Đất", quan: "Bình Thạnh" }
             │
             ▼
    Service: TimGiaTriThamChieuAsync("Đất", "Bình Thạnh", ...)
             │
             ▼
    Database: GiaTriTaiSan_ThamChieu
    WHERE LoaiTaiSan = 'Đất' AND Quan = 'Bình Thạnh'
             │
             ▼
    Return: { giaTriThamChieu: 80000000, tyLeThamDinh: 70 }
             │
             │ 8. JavaScript tính toán
             ▼
    giaTriThamChieuTotal = 80M × 30m² = 2,400M
    giaTriThamDinh = 2,400M × 70% = 1,680M
             │
             │ 9. Hiển thị kết quả
             ▼
    ┌─────────────────────────────────────────────────────────┐
    │  Kết quả tra cứu:                                       │
    │  ┌──────────────────────────────────────────┐           │
    │  │ Quận: Bình Thạnh                         │           │
    │  │ Giá tham chiếu: 80,000,000 VNĐ/m²       │           │
    │  │ Diện tích: 30 m²                         │           │
    │  │ Tổng giá trị: 2,400,000,000 VNĐ         │           │
    │  │ Tỷ lệ thẩm định: 70%                     │           │
    │  │ Giá trị thẩm định: 1,680,000,000 VNĐ    │           │
    │  └──────────────────────────────────────────┘           │
    └─────────────────────────────────────────────────────────┘
             │
             │ 10. Click "Lưu kết quả"
             ▼
    POST /QuanLyRuiRo/LuuKetQuaThamDinh
    Body: {
      maTaiSan: 5,
      giaTriThamChieu: 2400000000,
      giaTriThamDinh: 1680000000,
      tyLeThamDinh: 70,
      ghiChu: "Loại: Đất, Quận: Bình Thạnh..."
    }
             │
             ▼
    Service: LuuKetQuaThamDinhAsync(...)
             │
             ├─► UPDATE TaiSanDamBao
             │   SET GiaTriThiTruong = 2400000000
             │       GiaTriDinhGia = 1680000000
             │       NgayDinhGia = TODAY
             │
             └─► INSERT LichSu_DinhGiaTaiSan
                 (MaTaiSan, GiaTriCu, GiaTriMoi, ChenhLech, ...)
             │
             │ 11. Response success
             ▼
    JavaScript: Hiển thị thông báo + Cập nhật UI
    - Viền xanh cho tài sản đã thẩm định
    - Badge "✓ Đã thẩm định"
    - Tự động đóng modal sau 2s
```

---

## 🎨 CẤU TRÚC DATABASE

### **GiaTriTaiSan_ThamChieu** (Bảng tham chiếu giá)
```sql
MaGiaTri        INT PRIMARY KEY
LoaiTaiSan      NVARCHAR(50)   -- 'Đất', 'Xe cộ', 'Vàng'
ThanhPho        NVARCHAR(100)  -- 'Hồ Chí Minh'
Quan            NVARCHAR(100)  -- 'Quận 1', 'Bình Thạnh'
HangXe          NVARCHAR(50)   -- 'Honda', 'Toyota'
DongXe          NVARCHAR(100)  -- 'City', 'Vios'
NamSanXuat      INT            -- 2024, 2023
GiaTriThamChieu DECIMAL(18,2)  -- 80000000
TyLeThamDinh    DECIMAL(5,2)   -- 70.00 (%)
TrangThaiHoatDong BIT
```

### **TaiSanDamBao** (Tài sản của khách hàng)
```sql
MaTaiSan         INT PRIMARY KEY
TenGoi           NVARCHAR(200)
LoaiTaiSan       NVARCHAR(100)
GiaTriThiTruong  DECIMAL(18,2)  -- ← CẬP NHẬT khi thẩm định
GiaTriDinhGia    DECIMAL(18,2)  -- ← CẬP NHẬT khi thẩm định
NgayDinhGia      DATE            -- ← CẬP NHẬT khi thẩm định
DonViDinhGia     NVARCHAR(100)  -- 'Phòng Quản lý Rủi ro'
NguoiCapNhat     INT             -- FK → NguoiDung
NgayCapNhat      DATETIME
```

### **LichSu_DinhGiaTaiSan** (Audit Trail)
```sql
MaLichSu         INT PRIMARY KEY
MaTaiSan         INT FK → TaiSanDamBao
NgayDinhGia      DATE
GiaTriCu         DECIMAL(18,2)  -- Giá trị trước khi thẩm định
GiaTriMoi        DECIMAL(18,2)  -- Giá trị sau khi thẩm định
ChenhLech        DECIMAL(18,2)  -- GiaTriMoi - GiaTriCu
TyLeThayDoi      DECIMAL(5,2)   -- % thay đổi
DonViDinhGia     NVARCHAR(100)
NguoiDinhGia     NVARCHAR(100)
PhuongPhapDinhGia NVARCHAR(200)
LyDoDinhGia      NVARCHAR(500)
FileDinhGia      NVARCHAR(500)  -- Ghi chú chi tiết
NgayTao          DATETIME
NguoiTao         INT
```

### **HoSoVay_FileDinhKem** (File đính kèm)
```sql
MaFile      INT PRIMARY KEY
MaKhoanVay  INT FK → KhoanVay
TenFile     NVARCHAR(255)     -- 'giay-to-nha.jpg'
DuongDan    NVARCHAR(500)     -- '/uploads/giay-to-nha.jpg'
LoaiFile    NVARCHAR(50)      -- 'Giấy tờ tài sản', 'Ảnh chụp'
KichThuoc   BIGINT            -- 47616 (bytes)
NgayTao     DATETIME
NguoiTao    INT
```

---

## 🔐 QUYỀN HẠN & BẢO MẬT

### **Session Required:**
```csharp
var maNguoiDung = HttpContext.Session.GetString("MaNguoiDung");
var tenVaiTro = HttpContext.Session.GetString("TenVaiTro");

if (tenVaiTro != "QuanLyRuiRo") {
    return RedirectToAction("Index", "Dashboard");
}
```

### **Database Audit:**
- **Tạo mới:** `NgayTao`, `NguoiTao`
- **Cập nhật:** `NgayCapNhat`, `NguoiCapNhat`
- **Lịch sử:** Bảng `LichSu_DinhGiaTaiSan` lưu toàn bộ thay đổi

---

## ⚠️ XỬ LÝ LỖI THƯỜNG GẶP

### **1. "Có lỗi xảy ra khi tra cứu giá tham chiếu"**
**Nguyên nhân:**
- API trả về `{ success: false }` hoặc exception
- Không tìm thấy dữ liệu trong `GiaTriTaiSan_ThamChieu`

**Giải pháp:**
- Mở F12 → Console → Xem log: `console.log('Response từ API:', result)`
- Kiểm tra database có dữ liệu: 
  ```sql
  SELECT * FROM GiaTriTaiSan_ThamChieu WHERE LoaiTaiSan = N'Đất'
  ```
- Chạy lại script: `create_gia_tri_tai_san_tham_chieu.sql`

### **2. "Phiên đăng nhập hết hạn"**
**Nguyên nhân:** Session expired

**Giải pháp:**
- Đăng nhập lại
- Kiểm tra `appsettings.json`:
  ```json
  "SessionOptions": {
    "IdleTimeout": "00:30:00"
  }
  ```

### **3. File ảnh không hiển thị**
**Nguyên nhân:**
- Đường dẫn sai: `/uploads/file.jpg` không tồn tại
- MIME type không đúng

**Giải pháp:**
- Kiểm tra `wwwroot/uploads/` có file
- Kiểm tra `DuongDan` trong database:
  ```sql
  SELECT TenFile, DuongDan FROM HoSoVay_FileDinhKem
  ```
- Thêm fallback: `onerror="this.src='/asset/no-image.png'"`

---

## 📈 KẾT QUẢ CUỐI CÙNG

Sau khi hoàn thành thẩm định:

1. **Database:**
   - `TaiSanDamBao.GiaTriDinhGia` = 1,680,000,000
   - `LichSu_DinhGiaTaiSan` có 1 record mới

2. **UI:**
   - Tài sản có viền xanh + badge "✓ Đã thẩm định"
   - Nút "Thẩm định" có thể disabled hoặc hiển thị "Xem lại"

3. **Báo cáo:**
   - Có thể xuất báo cáo thẩm định theo ngày/người thẩm định
   - Truy vết lịch sử thay đổi qua `LichSu_DinhGiaTaiSan`

---

## 🎓 KẾT LUẬN

Hệ thống thẩm định tài sản đã hoàn chỉnh với:
- ✅ Tra cứu giá tham chiếu tự động
- ✅ Hiển thị hình ảnh và file đính kèm
- ✅ Tính toán giá trị thẩm định chính xác
- ✅ Lưu trữ và audit trail đầy đủ
- ✅ UI/UX thân thiện với cảnh báo rõ ràng

**Liên hệ hỗ trợ:** Kiểm tra log trong `F12 Console` và `Server logs` khi gặp vấn đề.
