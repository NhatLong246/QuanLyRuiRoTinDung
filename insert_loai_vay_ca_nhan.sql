-- Script thêm 4 loại vay cho khách hàng cá nhân
USE QuanLyRuiRoTinDung
GO

-- Xóa các loại vay cá nhân cũ nếu có (tùy chọn)
-- DELETE FROM LoaiKhoanVay WHERE MaLoaiVay_Code IN ('CONSUME', 'SOCIAL', 'STUDENT', 'STARTUP');

-- 1. Vay tiêu dùng
INSERT INTO LoaiKhoanVay (TenLoaiVay, MaLoaiVay_Code, MoTa, SoTienVayToiDa, KyHanVayToiDa, LaiSuatToiThieu, LaiSuatToiDa, TrangThaiHoatDong)
VALUES (N'Vay tiêu dùng', 'CONSUME_CN', N'Vay tiêu dùng cho khách hàng cá nhân', 500000000, 60, 0, 20, 1);

-- 2. Vay An sinh xã hội
INSERT INTO LoaiKhoanVay (TenLoaiVay, MaLoaiVay_Code, MoTa, SoTienVayToiDa, KyHanVayToiDa, LaiSuatToiThieu, LaiSuatToiDa, TrangThaiHoatDong)
VALUES (N'Vay An sinh xã hội', 'SOCIAL_CN', N'Vay hỗ trợ an sinh xã hội cho khách hàng cá nhân', 200000000, 36, 2, 2, 1);

-- 3. Vay Sinh viên
INSERT INTO LoaiKhoanVay (TenLoaiVay, MaLoaiVay_Code, MoTa, SoTienVayToiDa, KyHanVayToiDa, LaiSuatToiThieu, LaiSuatToiDa, TrangThaiHoatDong)
VALUES (N'Vay Sinh viên', 'STUDENT_CN', N'Vay hỗ trợ sinh viên, lãi suất 0%', 50000000, 48, 0, 0, 1);

-- 4. Vay kinh doanh khởi nghiệp
INSERT INTO LoaiKhoanVay (TenLoaiVay, MaLoaiVay_Code, MoTa, SoTienVayToiDa, KyHanVayToiDa, LaiSuatToiThieu, LaiSuatToiDa, TrangThaiHoatDong)
VALUES (N'Vay kinh doanh khởi nghiệp', 'STARTUP_CN', N'Vay hỗ trợ khởi nghiệp, lãi suất ưu đãi 3%/năm', 500000000, 60, 3, 3, 1);

GO
