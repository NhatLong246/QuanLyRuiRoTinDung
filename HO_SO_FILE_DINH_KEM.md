# TÀI LIỆU: HỒ SƠ FILE ĐÍNH KÈM

## 📋 TÓM TẮT TÌNH TRẠNG HIỆN TẠI

### ✅ **1. TÀI SẢN ĐẢM BẢO - ĐÃ HỖ TRỢ NHIỀU TÀI SẢN**

**Database:**
- Bảng `KhoanVay_TaiSan` (dòng 332-345 trong `database_update.sql`)
- Đây là bảng liên kết **nhiều-nhiều** giữa `KhoanVay` và `TaiSanDamBao`
- **1 hồ sơ vay có thể có NHIỀU tài sản đảm bảo** ✅

**Code:**
- `LoanController.cs` (dòng 224-278): Xử lý nhiều tài sản bằng cách:
  - Split các giá trị từ form: `LoaiTaiSan`, `TenTaiSanKhac`, `GiaTriDinhGia`, etc.
  - Tạo `List<KhoanVayTaiSan>` và lưu vào database
- `Views/Loan/Create.cshtml`: Có function `addTaiSan()` để thêm nhiều tài sản động

**Kết luận:** ✅ **Đã đảm bảo 1 hồ sơ có thể lưu được nhiều tài sản đảm bảo**

---

### ❌ **2. FILE ĐÍNH KÈM - CHƯA CÓ CẤU TRÚC DATABASE**

**Tình trạng hiện tại:**
- Bảng `KhoanVay` chỉ có cột `DuongDanHoSo NVARCHAR(500)` (dòng 303)
- Cột này chỉ lưu được **1 đường dẫn duy nhất** ❌
- View có phần upload file nhưng **chưa có xử lý backend** ❌
- **Không có bảng nào để lưu nhiều file đính kèm** ❌

**Cần làm:**
1. ✅ Tạo bảng mới `HoSoVay_FileDinhKem` (đã tạo trong `create_ho_so_file_table.sql`)
2. ⏳ Tạo Entity Model `HoSoVayFileDinhKem.cs`
3. ⏳ Cập nhật `ApplicationDbContext.cs`
4. ⏳ Implement xử lý upload file trong `LoanController.cs`
5. ⏳ Cập nhật View để submit file

---

## 📊 CẤU TRÚC DATABASE ĐỀ XUẤT

### Bảng: `HoSoVay_FileDinhKem`

```sql
CREATE TABLE HoSoVay_FileDinhKem (
    MaFile INT PRIMARY KEY IDENTITY(1,1),              -- Mã file
    MaKhoanVay INT NOT NULL,                           -- FK đến KhoanVay
    LoaiFile NVARCHAR(50) NOT NULL,                    -- PhapLy, TaiChinh, TaiSanDamBao
    TenFile NVARCHAR(255) NOT NULL,                    -- Tên file gốc
    TenFileLuu NVARCHAR(255) NOT NULL,                  -- Tên file đã lưu (hash)
    DuongDan NVARCHAR(500) NOT NULL,                    -- Đường dẫn đầy đủ
    KichThuoc BIGINT,                                  -- Kích thước (bytes)
    DinhDang NVARCHAR(10),                             -- PDF, JPG, PNG, DOCX
    MoTa NVARCHAR(500),                                -- Mô tả (tùy chọn)
    NgayTao DATETIME DEFAULT GETDATE(),
    NguoiTao INT,                                      -- FK đến NguoiDung
    TrangThai BIT DEFAULT 1,                           -- 1: Đang dùng, 0: Đã xóa
    
    FOREIGN KEY (MaKhoanVay) REFERENCES KhoanVay(MaKhoanVay) ON DELETE CASCADE,
    FOREIGN KEY (NguoiTao) REFERENCES NguoiDung(MaNguoiDung),
    CONSTRAINT CK_LoaiFile CHECK (LoaiFile IN ('PhapLy', 'TaiChinh', 'TaiSanDamBao'))
);
```

**Phân loại file:**
- **PhapLy**: "Pháp lý & Định danh (CCCD/GPKD)" - File CCCD, GPKD, giấy tờ pháp lý
- **TaiChinh**: "Báo cáo tài chính & Thu nhập" - Báo cáo tài chính, sao kê ngân hàng
- **TaiSanDamBao**: "Hợp đồng / Tài sản đảm bảo" - Hợp đồng thế chấp, ảnh tài sản

**Lưu ý:** Với "Vay sinh viên", các tiêu đề sẽ thay đổi nhưng `LoaiFile` vẫn giữ nguyên:
- PhapLy → "Giấy trúng tuyển đại học"
- TaiChinh → "Hồ sơ nhập học"
- TaiSanDamBao → "Giấy chứng nhận sinh viên"

---

## 📁 CẤU TRÚC THƯ MỤC LƯU FILE

**Đề xuất cấu trúc:**
```
wwwroot/
  uploads/
    ho-so-vay/
      {MaKhoanVayCode}/
        phap-ly/
          {TenFileLuu}.{extension}
        tai-chinh/
          {TenFileLuu}.{extension}
        tai-san-dam-bao/
          {TenFileLuu}.{extension}
```

**Ví dụ:**
```
wwwroot/uploads/ho-so-vay/LOAN0001/phap-ly/cccd_20241201_abc123.pdf
wwwroot/uploads/ho-so-vay/LOAN0001/tai-chinh/sao-ke_20241201_def456.pdf
```

---

## 🔄 CÁC BƯỚC TIẾP THEO

1. **Chạy SQL script** `create_ho_so_file_table.sql` để tạo bảng
2. **Tạo Entity Model** `Models/Entities/HoSoVayFileDinhKem.cs`
3. **Cập nhật DbContext** thêm `DbSet<HoSoVayFileDinhKem>`
4. **Implement Service** để xử lý upload/download file
5. **Cập nhật Controller** xử lý file upload trong POST action
6. **Cập nhật View** để submit file với FormData

---

## ✅ KẾT LUẬN

- ✅ **Tài sản đảm bảo:** Đã hỗ trợ nhiều tài sản cho 1 hồ sơ vay
- ❌ **File đính kèm:** Cần tạo bảng mới và implement xử lý upload

Bạn có muốn tôi tiếp tục implement phần xử lý file upload không?
