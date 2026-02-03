-- =============================================
-- THÊM TRƯỜNG CCCD CỦA NGƯỜI ĐẠI DIỆN PHÁP LUẬT
-- =============================================
-- Script này thêm trường SoCCCD_NguoiDaiDienPhapLuat vào bảng KhachHang_DoanhNghiep
-- Để lưu số CCCD của người đại diện pháp luật

USE QuanLyRuiRoTinDung
GO

-- Kiểm tra xem cột đã tồn tại chưa, nếu chưa thì thêm
IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID('KhachHang_DoanhNghiep') 
    AND name = 'SoCCCD_NguoiDaiDienPhapLuat'
)
BEGIN
    ALTER TABLE KhachHang_DoanhNghiep
    ADD SoCCCD_NguoiDaiDienPhapLuat NVARCHAR(20) NULL;  -- Số CCCD của người đại diện pháp luật
    
    PRINT 'Đã thêm cột SoCCCD_NguoiDaiDienPhapLuat vào bảng KhachHang_DoanhNghiep';
END
ELSE
BEGIN
    PRINT 'Cột SoCCCD_NguoiDaiDienPhapLuat đã tồn tại trong bảng KhachHang_DoanhNghiep';
END
GO

-- Tạo index để tìm kiếm nhanh (nếu cần)
IF NOT EXISTS (
    SELECT * FROM sys.indexes 
    WHERE name = 'IX_KhachHang_DoanhNghiep_SoCCCD_NguoiDaiDien'
    AND object_id = OBJECT_ID('KhachHang_DoanhNghiep')
)
BEGIN
    CREATE INDEX IX_KhachHang_DoanhNghiep_SoCCCD_NguoiDaiDien 
    ON KhachHang_DoanhNghiep(SoCCCD_NguoiDaiDienPhapLuat);
    
    PRINT 'Đã tạo index IX_KhachHang_DoanhNghiep_SoCCCD_NguoiDaiDien';
END
GO
