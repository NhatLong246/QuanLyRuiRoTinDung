-- =============================================
-- INSERT NHÂN VIÊN QUẢN LÝ RỦI RO THỨ 2 ĐỂ TEST
-- =============================================

USE QuanLyRuiRoTinDung
GO

-- Kiểm tra các vai trò hiện có trong hệ thống
PRINT N'=== DANH SÁCH VAI TRÒ HIỆN CÓ ===';
SELECT MaVaiTro, TenVaiTro FROM VaiTro;
PRINT N'';

-- INSERT NHÂN VIÊN QUẢN LÝ RỦI RO MỚI
-- =============================================
-- Tài khoản đăng nhập: qlrr2 / qlrr2

DECLARE @MaVaiTroQLRR INT;
DECLARE @MaPhongBanQLRR INT;

-- Tìm vai trò Quản lý Rủi ro (có thể là 'QuanLyRuiRo' hoặc 'Quản lý rủi ro' hoặc tương tự)
SELECT @MaVaiTroQLRR = MaVaiTro FROM VaiTro 
WHERE TenVaiTro = N'QuanLyRuiRo' 
   OR TenVaiTro = N'Quản lý rủi ro'
   OR TenVaiTro LIKE N'%Rủi ro%'
   OR TenVaiTro LIKE N'%RuiRo%';

-- Tìm phòng ban Quản lý Rủi ro
SELECT @MaPhongBanQLRR = MaPhongBan FROM PhongBan 
WHERE TenPhongBan = N'Phòng Quản lý Rủi ro'
   OR TenPhongBan LIKE N'%Rủi ro%';

IF @MaVaiTroQLRR IS NULL
BEGIN
    PRINT N'LỖI: Không tìm thấy vai trò Quản lý Rủi ro trong bảng VaiTro!';
    PRINT N'Vui lòng kiểm tra lại tên vai trò trong bảng VaiTro.';
END
ELSE IF NOT EXISTS (SELECT 1 FROM NguoiDung WHERE TenDangNhap = 'qlrr2')
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
        @MaVaiTroQLRR,
        @MaPhongBanQLRR,
        1,
        1
    );
    PRINT N'Đã thêm nhân viên Quản lý Rủi ro mới thành công!';
    PRINT N'MaVaiTro sử dụng: ' + CAST(@MaVaiTroQLRR AS NVARCHAR(10));
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
