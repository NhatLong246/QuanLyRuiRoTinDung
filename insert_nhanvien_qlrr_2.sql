-- =============================================
-- INSERT NHÂN VIÊN QUẢN LÝ RỦI RO THỨ 2 ĐỂ TEST
-- =============================================

USE QuanLyRuiRoTinDung
GO

-- INSERT NHÂN VIÊN QUẢN LÝ RỦI RO MỚI
-- =============================================
-- Tài khoản đăng nhập: qlrr2 / qlrr2

IF NOT EXISTS (SELECT 1 FROM NguoiDung WHERE TenDangNhap = 'qlrr2')
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
        'qlrr2',
        'qlrr2', -- Mật khẩu plain text (chưa hash)
        N'Lê Văn Phân Tích Rủi Ro 2',
        'qlrr2@bank.com',
        '0903456789',
        (SELECT MaVaiTro FROM VaiTro WHERE TenVaiTro = N'QuanLyRuiRo'),
        (SELECT MaPhongBan FROM PhongBan WHERE TenPhongBan = N'Phòng Quản lý Rủi ro'),
        1,
        1
    );
    PRINT N'Đã thêm nhân viên Quản lý Rủi ro mới thành công!';
END
ELSE
BEGIN
    PRINT N'Tài khoản qlrr2 đã tồn tại!';
END
GO

-- Hiển thị danh sách nhân viên Quản lý Rủi ro
SELECT 
    nd.MaNguoiDung,
    nd.TenDangNhap,
    nd.MatKhauHash AS MatKhau,
    nd.HoTen,
    nd.Email,
    vt.TenVaiTro,
    pb.TenPhongBan
FROM NguoiDung nd
JOIN VaiTro vt ON nd.MaVaiTro = vt.MaVaiTro
LEFT JOIN PhongBan pb ON nd.MaPhongBan = pb.MaPhongBan
WHERE vt.TenVaiTro = N'QuanLyRuiRo'
ORDER BY nd.MaNguoiDung;

PRINT N'';
PRINT N'==============================================';
PRINT N'THÔNG TIN TÀI KHOẢN ĐĂNG NHẬP';
PRINT N'==============================================';
PRINT N'Tài khoản mới: qlrr2 / qlrr2';
PRINT N'';
PRINT N'Các tài khoản QLRR hiện có:';
PRINT N'  - qlrr1 / qlrr1';
PRINT N'  - qlrr001 / qlrr123';
PRINT N'  - qlrr2 / qlrr2 (mới tạo)';
PRINT N'==============================================';
