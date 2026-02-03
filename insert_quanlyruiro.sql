-- =============================================
-- INSERT VAI TRÒ VÀ NHÂN VIÊN QUẢN LÝ RỦI RO
-- =============================================

USE QuanLyRuiRoTinDung
GO

-- 1. INSERT VAI TRÒ QUẢN LÝ RỦI RO
-- =============================================
IF NOT EXISTS (SELECT 1 FROM VaiTro WHERE TenVaiTro = N'QuanLyRuiRo')
BEGIN
    INSERT INTO VaiTro (TenVaiTro, MoTa, TrangThaiHoatDong)
    VALUES (N'QuanLyRuiRo', N'Bộ phận Quản lý Rủi ro - Đánh giá, kiểm soát và giám sát rủi ro tín dụng', 1);
END
GO

-- 2. INSERT PHÒNG BAN QUẢN LÝ RỦI RO (nếu chưa có)
-- =============================================
IF NOT EXISTS (SELECT 1 FROM PhongBan WHERE TenPhongBan = N'Phòng Quản lý Rủi ro')
BEGIN
    INSERT INTO PhongBan (TenPhongBan, MaPhongBan_Code, MoTa, TrangThaiHoatDong)
    VALUES (N'Phòng Quản lý Rủi ro', N'QLRR', N'Phòng phụ trách đánh giá, kiểm soát và giám sát rủi ro tín dụng', 1);
END
GO

-- 3. INSERT NHÂN VIÊN QUẢN LÝ RỦI RO
-- =============================================
-- Mật khẩu: quanlyruiro123 (lưu dạng plain text - chưa hash)

IF NOT EXISTS (SELECT 1 FROM NguoiDung WHERE TenDangNhap = 'qlrr1')
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
        'qlrr1',
        'qlrr1', -- Mật khẩu plain text (chưa hash)
        N'Nguyễn Văn Quản Lý Rủi Ro',
        'quanlyruiro@bank.com',
        '0901234567',
        (SELECT MaVaiTro FROM VaiTro WHERE TenVaiTro = N'QuanLyRuiRo'),
        (SELECT MaPhongBan FROM PhongBan WHERE TenPhongBan = N'Phòng Quản lý Rủi ro'),
        1,
        1
    );
END
GO

-- 4. INSERT THÊM NHÂN VIÊN QUẢN LÝ RỦI RO THỨ 2 (Tùy chọn)
-- =============================================
IF NOT EXISTS (SELECT 1 FROM NguoiDung WHERE TenDangNhap = 'qlrr001')
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
        'qlrr001',
        'qlrr123', -- Mật khẩu plain text (chưa hash)
        N'Trần Thị Phân Tích Rủi Ro',
        'qlrr001@bank.com',
        '0902345678',
        (SELECT MaVaiTro FROM VaiTro WHERE TenVaiTro = N'QuanLyRuiRo'),
        (SELECT MaPhongBan FROM PhongBan WHERE TenPhongBan = N'Phòng Quản lý Rủi ro'),
        1,
        1
    );
END
GO

PRINT N'Đã thêm vai trò và nhân viên Quản lý Rủi ro thành công!';
PRINT N'Tài khoản đăng nhập 1: quanlyruiro / quanlyruiro123';
PRINT N'Tài khoản đăng nhập 2: qlrr001 / qlrr123';

