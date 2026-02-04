-- =============================================
-- INSERT NHÂN VIÊN MỚI: nhanvien1
-- Mật khẩu: 123123
-- =============================================

USE QuanLyRuiRoTinDung
GO

-- Đảm bảo có vai trò NhanVienTinDung
IF NOT EXISTS (SELECT 1 FROM VaiTro WHERE TenVaiTro = N'NhanVienTinDung')
BEGIN
    INSERT INTO VaiTro (TenVaiTro, MoTa, TrangThaiHoatDong)
    VALUES (N'NhanVienTinDung', N'Nhân viên tín dụng - Xử lý hồ sơ vay', 1);
END
GO

-- Đảm bảo có Phòng Tín dụng
IF NOT EXISTS (SELECT 1 FROM PhongBan WHERE TenPhongBan = N'Phòng Tín dụng')
BEGIN
    INSERT INTO PhongBan (TenPhongBan, MaPhongBan_Code, MoTa, TrangThaiHoatDong)
    VALUES (N'Phòng Tín dụng', N'TD', N'Phòng phụ trách xử lý hồ sơ vay', 1);
END
GO

-- INSERT NHÂN VIÊN nhanvien1
IF NOT EXISTS (SELECT 1 FROM NguoiDung WHERE TenDangNhap = 'nhanvien1')
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
        NgayTao
    )
    VALUES (
        'nhanvien1',
        '123123',
        N'Nhân Viên 1',
        'nhanvien1@bank.com',
        '0901234567',
        (SELECT MaVaiTro FROM VaiTro WHERE TenVaiTro = N'NhanVienTinDung'),
        (SELECT MaPhongBan FROM PhongBan WHERE TenPhongBan = N'Phòng Tín dụng'),
        1,
        GETDATE()
    );
    PRINT N'Đã tạo tài khoản nhanvien1 thành công!';
END
ELSE
BEGIN
    PRINT N'Tài khoản nhanvien1 đã tồn tại!';
END
GO
