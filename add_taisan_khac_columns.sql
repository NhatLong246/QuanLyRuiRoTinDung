-- Script thêm các cột cho Tài sản đảm bảo loại "Khác"
-- Chạy script này trong SQL Server Management Studio

USE QuanLyRuiRoTinDung;
GO

-- Thêm cột TenTaiSanKhac vào bảng KhoanVay_TaiSan
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'KhoanVay_TaiSan') AND name = 'TenTaiSanKhac')
BEGIN
    ALTER TABLE KhoanVay_TaiSan
    ADD TenTaiSanKhac NVARCHAR(200) NULL;
    PRINT N'Đã thêm cột TenTaiSanKhac';
END
ELSE
BEGIN
    PRINT N'Cột TenTaiSanKhac đã tồn tại';
END
GO

-- Thêm cột DonVi vào bảng KhoanVay_TaiSan
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'KhoanVay_TaiSan') AND name = 'DonVi')
BEGIN
    ALTER TABLE KhoanVay_TaiSan
    ADD DonVi NVARCHAR(50) NULL;
    PRINT N'Đã thêm cột DonVi';
END
ELSE
BEGIN
    PRINT N'Cột DonVi đã tồn tại';
END
GO

-- Thêm cột SoLuong vào bảng KhoanVay_TaiSan
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'KhoanVay_TaiSan') AND name = 'SoLuong')
BEGIN
    ALTER TABLE KhoanVay_TaiSan
    ADD SoLuong DECIMAL(18, 2) NULL;
    PRINT N'Đã thêm cột SoLuong';
END
ELSE
BEGIN
    PRINT N'Cột SoLuong đã tồn tại';
END
GO

-- Xác nhận các cột đã được thêm
SELECT 
    c.name AS ColumnName,
    t.name AS DataType,
    c.max_length AS MaxLength,
    c.is_nullable AS IsNullable
FROM sys.columns c
INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
WHERE c.object_id = OBJECT_ID(N'KhoanVay_TaiSan')
AND c.name IN ('TenTaiSanKhac', 'DonVi', 'SoLuong', 'GhiChu');
GO

PRINT N'Script hoàn tất!';
