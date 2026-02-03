-- Script thêm 2 loại vay cho khách hàng doanh nghiệp
USE QuanLyRuiRoTinDung
GO

-- 1. Vay có tài sản bảo đảm
INSERT INTO LoaiKhoanVay (TenLoaiVay, MaLoaiVay_Code, MoTa, SoTienVayToiDa, KyHanVayToiDa, LaiSuatToiThieu, LaiSuatToiDa, TrangThaiHoatDong)
VALUES (N'Vay có tài sản bảo đảm', 'COLLATERAL_DN', N'Vay có tài sản bảo đảm cho doanh nghiệp', 10000000000, 120, 5, 15, 1);

-- 2. Vay tín chấp doanh nghiệp
INSERT INTO LoaiKhoanVay (TenLoaiVay, MaLoaiVay_Code, MoTa, SoTienVayToiDa, KyHanVayToiDa, LaiSuatToiThieu, LaiSuatToiDa, TrangThaiHoatDong)
VALUES (N'Vay tín chấp doanh nghiệp', 'UNSECURED_DN', N'Vay tín chấp doanh nghiệp, yêu cầu CIC tốt', 5000000000, 60, 8, 18, 1);

GO
