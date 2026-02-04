-- =============================================
-- Thêm vai trò Admin và LanhDao
-- Chạy script này để thêm các vai trò mới
-- =============================================

USE QuanLyRuiRoTinDung;
GO

-- Kiểm tra và thêm vai trò Admin nếu chưa tồn tại
IF NOT EXISTS (SELECT 1 FROM VaiTro WHERE TenVaiTro = 'Admin')
BEGIN
    INSERT INTO VaiTro (TenVaiTro, MoTa, NgayTao, TrangThaiHoatDong)
    VALUES (N'Admin', N'Quản trị hệ thống - Toàn quyền quản lý hệ thống', GETDATE(), 1);
    PRINT N'Đã thêm vai trò Admin';
END
ELSE
BEGIN
    PRINT N'Vai trò Admin đã tồn tại';
END
GO

-- Kiểm tra và thêm vai trò LanhDao nếu chưa tồn tại
IF NOT EXISTS (SELECT 1 FROM VaiTro WHERE TenVaiTro = 'LanhDao')
BEGIN
    INSERT INTO VaiTro (TenVaiTro, MoTa, NgayTao, TrangThaiHoatDong)
    VALUES (N'LanhDao', N'Lãnh đạo phê duyệt - Phê duyệt các khoản vay và quyết định cuối cùng', GETDATE(), 1);
    PRINT N'Đã thêm vai trò LanhDao';
END
ELSE
BEGIN
    PRINT N'Vai trò LanhDao đã tồn tại';
END
GO

-- Lấy MaVaiTro của Admin
DECLARE @MaVaiTroAdmin INT;
SELECT @MaVaiTroAdmin = MaVaiTro FROM VaiTro WHERE TenVaiTro = 'Admin';

-- Kiểm tra và thêm tài khoản Admin nếu chưa tồn tại
IF NOT EXISTS (SELECT 1 FROM NguoiDung WHERE TenDangNhap = 'admin')
BEGIN
    INSERT INTO NguoiDung (TenDangNhap, MatKhauHash, HoTen, Email, SoDienThoai, MaVaiTro, TrangThaiHoatDong, NgayTao)
    VALUES (
        N'admin',                           -- TenDangNhap
        N'admin123',                        -- MatKhauHash (nên hash trong thực tế)
        N'Quản trị viên hệ thống',          -- HoTen
        N'admin@bank.com',                  -- Email
        N'0909000001',                      -- SoDienThoai
        @MaVaiTroAdmin,                     -- MaVaiTro
        1,                                  -- TrangThaiHoatDong
        GETDATE()                           -- NgayTao
    );
    PRINT N'Đã tạo tài khoản Admin: admin / admin123';
END
ELSE
BEGIN
    -- Cập nhật vai trò cho tài khoản admin đã tồn tại
    UPDATE NguoiDung 
    SET MaVaiTro = @MaVaiTroAdmin,
        TrangThaiHoatDong = 1
    WHERE TenDangNhap = 'admin';
    PRINT N'Đã cập nhật vai trò Admin cho tài khoản admin';
END
GO

-- Hiển thị kết quả
PRINT N'';
PRINT N'=== DANH SÁCH VAI TRÒ ===';
SELECT MaVaiTro, TenVaiTro, MoTa, TrangThaiHoatDong FROM VaiTro ORDER BY MaVaiTro;

PRINT N'';
PRINT N'=== TÀI KHOẢN ADMIN ===';
SELECT nd.MaNguoiDung, nd.TenDangNhap, nd.HoTen, nd.Email, vt.TenVaiTro, nd.TrangThaiHoatDong
FROM NguoiDung nd
INNER JOIN VaiTro vt ON nd.MaVaiTro = vt.MaVaiTro
WHERE vt.TenVaiTro = 'Admin';

GO
