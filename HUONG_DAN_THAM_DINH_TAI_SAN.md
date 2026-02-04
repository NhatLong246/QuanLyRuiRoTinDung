# HƯỚNG DẪN SỬ DỤNG CHỨC NĂNG THẨM ĐỊNH TÀI SẢN

## 1. Giới thiệu

Chức năng thẩm định tài sản được tích hợp vào module **Quản lý Rủi ro Tín dụng**, cho phép nhân viên đánh giá rủi ro:
- Tra cứu giá trị tham chiếu của tài sản đảm bảo
- So sánh giá trị khai báo với giá trị thị trường
- Tính toán giá trị thẩm định dựa trên tỷ lệ quy định
- Cảnh báo khi có chênh lệch lớn giữa giá khai báo và giá tham chiếu

## 2. Chuẩn bị Database

### Bước 1: Chạy SQL Script
Mở SQL Server Management Studio và chạy file:
```
create_gia_tri_tai_san_tham_chieu.sql
```

Script này sẽ:
- Tạo bảng `GiaTriTaiSan_ThamChieu` 
- Insert dữ liệu giá đất của 11 quận/huyện TP.HCM
- Insert dữ liệu giá xe của 5 hãng (Honda, Toyota, Mazda, Hyundai, Kia)
- Insert dữ liệu giá vàng SJC, PNJ

### Bước 2: Verify Data
Kiểm tra dữ liệu đã được insert:
```sql
-- Xem giá đất
SELECT * FROM GiaTriTaiSan_ThamChieu WHERE LoaiTaiSan = N'Đất' ORDER BY GiaTriThamChieu DESC;

-- Xem giá xe
SELECT * FROM GiaTriTaiSan_ThamChieu WHERE LoaiTaiSan = N'Xe' ORDER BY HangXe, NamSanXuat DESC;
```

## 3. Cách sử dụng

### Bước 1: Truy cập giao diện Thẩm định
1. Đăng nhập với tài khoản có vai trò **Quản lý Rủi ro**
2. Vào menu: **Quản lý Rủi ro** > **Thẩm định Rủi ro Tín dụng**
3. Tìm khoản vay cần đánh giá
4. Click nút **Đánh giá** để mở modal chi tiết

### Bước 2: Xem thông tin tài sản
Trong modal đánh giá, cuộn xuống phần **Tài sản Đảm bảo & Thẩm định**:
- Hiển thị danh sách tất cả tài sản đảm bảo của khoản vay
- Mỗi tài sản có nút **Thẩm định**

### Bước 3: Thẩm định tài sản

#### A. Thẩm định ĐẤT
1. Click nút **Thẩm định** trên tài sản cần đánh giá
2. Chọn **Loại tài sản**: `Đất`
3. Chọn **Quận/Huyện**: (Ví dụ: Quận 1, Quận 7, Bình Thạnh,...)
4. Nhập **Diện tích** (m²)
5. Nhập **Giá trị khai báo** (của khách hàng)
6. Click **Tra cứu giá tham chiếu**

Hệ thống sẽ hiển thị:
- Giá tham chiếu (VNĐ/m²)
- Tổng giá trị tham chiếu = Giá/m² × Diện tích
- Tỷ lệ thẩm định (%)
- **Giá trị thẩm định** = Tổng giá trị × Tỷ lệ
- Chênh lệch so với giá khai báo

#### B. Thẩm định XE CỘ
1. Click nút **Thẩm định** trên tài sản xe
2. Chọn **Loại tài sản**: `Xe cộ`
3. Chọn **Hãng xe**: (Honda, Toyota, Mazda, Hyundai, Kia)
4. Chọn **Dòng xe**: (City, Vios, CX-5, Accent, Morning,...)
5. Nhập **Năm sản xuất**
6. Nhập **Giá trị khai báo**
7. Click **Tra cứu giá tham chiếu**

Hệ thống sẽ hiển thị:
- Giá tham chiếu của dòng xe
- Tỷ lệ thẩm định (%)
- **Giá trị thẩm định**
- Chênh lệch so với giá khai báo
- ⚠️ Cảnh báo nếu chênh lệch > 20%

### Bước 4: Lưu kết quả
1. Sau khi xem kết quả tra cứu, click **Lưu kết quả**
2. Kết quả thẩm định sẽ hiển thị ngay dưới thông tin tài sản
3. Tiếp tục thẩm định các tài sản khác (nếu có)

### Bước 5: Hoàn thành đánh giá
Sau khi thẩm định xong tất cả tài sản:
1. Cuộn xuống phần **Đánh giá Rủi ro**
2. Điền các thông tin:
   - Tổng điểm rủi ro
   - Mức độ rủi ro
   - Xếp hạng
   - Kiến nghị (Chấp thuận/Cân nhắc/Từ chối)
   - Nhận xét chi tiết (bao gồm kết quả thẩm định tài sản)
3. Click **Hoàn thành đánh giá**

## 4. Cơ chế tính toán

### A. Giá trị Thẩm định
```
Giá trị thẩm định = Giá trị tham chiếu × Tỷ lệ thẩm định
```

**Tỷ lệ thẩm định mặc định:**
- Đất: 70% - 80%
- Xe cộ: 60% - 70%
- Vàng: 85%

### B. Cảnh báo Chênh lệch
Hệ thống tự động cảnh báo khi:
```
|Giá khai báo - Giá thẩm định| / Giá thẩm định > 20%
```

**Ví dụ:**
- Giá thẩm định: 500 triệu
- Giá khai báo: 700 triệu
- Chênh lệch: +200 triệu (40%)
- ⚠️ **Cảnh báo: Chênh lệch lớn, cần xem xét kỹ!**

## 5. Dữ liệu Tham chiếu

### A. Giá Đất TP.HCM (2024)

| Quận/Huyện | Khu vực | Giá (triệu/m²) | Tỷ lệ thẩm định |
|------------|---------|----------------|-----------------|
| Quận 1 | Trung tâm | 500 | 70% |
| Quận 1 | Ngoại vi | 350 | 70% |
| Quận 2 | Đô thị mới | 120 | 70% |
| Quận 3 | Trung tâm | 300 | 70% |
| Quận 7 | Phú Mỹ Hưng | 180 | 75% |
| Quận 9 | Trung tâm | 80 | 70% |
| Bình Thạnh | Trung tâm | 200 | 75% |
| Tân Bình | Gần sân bay | 180 | 75% |
| Gò Vấp | Trung tâm | 90 | 70% |
| Hóc Môn | Ngoại thành | 40 | 70% |
| Củ Chi | Ngoại thành | 25 | 70% |

### B. Giá Xe (mẫu)

| Hãng | Dòng xe | Năm | Giá (triệu) | Tỷ lệ thẩm định |
|------|---------|-----|-------------|-----------------|
| Honda | City | 2024 | 559 | 70% |
| Honda | CR-V | 2024 | 1,029 | 70% |
| Toyota | Vios | 2024 | 478 | 70% |
| Toyota | Camry | 2024 | 1,220 | 70% |
| Mazda | CX-5 | 2024 | 839 | 70% |
| Hyundai | Accent | 2024 | 439 | 65% |
| Kia | Morning | 2024 | 349 | 65% |

**Lưu ý:** Giá xe cũ sẽ được hệ thống tự động tìm kiếm dựa trên năm sản xuất gần nhất.

## 6. Lưu ý quan trọng

### Quy trình thẩm định
1. **Luôn tra cứu giá tham chiếu** trước khi đánh giá
2. **Chú ý cảnh báo** khi chênh lệch > 20%
3. **Ghi rõ kết quả thẩm định** vào phần Nhận xét
4. **Xem xét tổng hợp** tất cả tài sản đảm bảo

### Trường hợp đặc biệt
- **Đất không có trong danh sách quận**: Sử dụng giá quận lân cận tương tự
- **Xe không có trong database**: Tìm xe cùng phân khúc, cùng năm
- **Tài sản khác** (nhà, vàng): Tra cứu thủ công và nhập giá trị thẩm định

### Cập nhật giá tham chiếu
Giá tham chiếu nên được cập nhật **3-6 tháng/lần** để đảm bảo tính chính xác:
```sql
UPDATE GiaTriTaiSan_ThamChieu
SET GiaTriThamChieu = [giá mới],
    NgayCapNhat = GETDATE()
WHERE MaThamChieu = [mã cần cập nhật];
```

## 7. Troubleshooting

### Lỗi: "Không tìm thấy giá trị tham chiếu"
**Nguyên nhân:** Tài sản chưa có trong database  
**Giải pháp:** Thêm dữ liệu mới vào bảng `GiaTriTaiSan_ThamChieu`

### Lỗi: Database không có bảng
**Nguyên nhân:** Chưa chạy script SQL  
**Giải pháp:** Chạy file `create_gia_tri_tai_san_tham_chieu.sql`

### Modal không hiển thị
**Nguyên nhân:** Lỗi JavaScript  
**Giải pháp:** Kiểm tra Console (F12) để xem lỗi chi tiết

## 8. Liên hệ hỗ trợ

Nếu gặp vấn đề khi sử dụng, vui lòng liên hệ:
- Email: support@example.com
- Hotline: 1900-xxxx
