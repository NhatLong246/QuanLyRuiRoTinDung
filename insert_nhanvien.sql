-- =============================================
-- INSERT VAI TRÒ VÀ NHÂN VIÊN ĐỂ ĐĂNG NHẬP
-- =============================================

USE QuanLyRuiRoTinDung
GO

-- 1. INSERT VAI TRÒ NHÂN VIÊN
-- =============================================
IF NOT EXISTS (SELECT 1 FROM VaiTro WHERE TenVaiTro = N'NhanVienTinDung')
BEGIN
    INSERT INTO VaiTro (TenVaiTro, MoTa, TrangThaiHoatDong)
    VALUES (N'NhanVienTinDung', N'Nhân viên tín dụng - Xử lý hồ sơ vay', 1);
END
GO

-- 2. INSERT PHÒNG BAN (nếu chưa có)
-- =============================================
IF NOT EXISTS (SELECT 1 FROM PhongBan WHERE TenPhongBan = N'Phòng Tín dụng')
BEGIN
    INSERT INTO PhongBan (TenPhongBan, MaPhongBan_Code, MoTa, TrangThaiHoatDong)
    VALUES (N'Phòng Tín dụng', N'TD', N'Phòng phụ trách xử lý hồ sơ vay', 1);
END
GO

-- 3. INSERT NHÂN VIÊN
-- =============================================
-- Mật khẩu: nhanvien123 (lưu dạng plain text - chưa hash)

IF NOT EXISTS (SELECT 1 FROM NguoiDung WHERE TenDangNhap = 'nhanvien')
BEGIN
    INSERT INTO NguoiDung (
        TenDangNhap, 
        MatKhauHash, 
        HoTen, 
        Email, 
        SoDienThoai, 
        MaVaiTro, 
        MaPhongBan, 
        TrangThaiHoatDong,
        NguoiTao
    )
    VALUES (
        'nhanvien',
        'nhanvien123', -- Mật khẩu plain text (chưa hash)
        N'Trần Thị Nhân Viên',
        'nhanvien@bank.com',
        '0907654321',
        (SELECT MaVaiTro FROM VaiTro WHERE TenVaiTro = N'NhanVienTinDung'),
        (SELECT MaPhongBan FROM PhongBan WHERE TenPhongBan = N'Phòng Tín dụng'),
        1,
        1
    );
END
GO

PRINT N'Đã thêm vai trò và nhân viên thành công!';
PRINT N'Tài khoản đăng nhập: nhanvien / nhanvien123';

