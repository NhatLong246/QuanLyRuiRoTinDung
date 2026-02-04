-- =============================================
-- Tạo tài khoản Lãnh đạo
-- =============================================

USE QuanLyRuiRoTinDung;
GO

-- Lấy MaVaiTro của LanhDao
DECLARE @MaVaiTroLanhDao INT = (SELECT MaVaiTro FROM VaiTro WHERE TenVaiTro = 'LanhDao');

-- Kiểm tra và thêm tài khoản Lãnh đạo nếu chưa tồn tại
IF NOT EXISTS (SELECT 1 FROM NguoiDung WHERE TenDangNhap = 'lanhdao')
BEGIN
    INSERT INTO NguoiDung (TenDangNhap, MatKhauHash, HoTen, Email, SoDienThoai, MaVaiTro, TrangThaiHoatDong, NgayTao)
    VALUES (
        N'lanhdao',                         -- TenDangNhap
        N'lanhdao123',                      -- MatKhauHash
        N'Nguyễn Văn Lãnh Đạo',             -- HoTen
        N'lanhdao@bank.com',                -- Email
        N'0909000002',                      -- SoDienThoai
        @MaVaiTroLanhDao,                   -- MaVaiTro
        1,                                  -- TrangThaiHoatDong
        GETDATE()                           -- NgayTao
    );
    PRINT N'Đã tạo tài khoản Lãnh đạo: lanhdao / lanhdao123';
END
ELSE
BEGIN
    -- Cập nhật vai trò cho tài khoản lanhdao đã tồn tại
    UPDATE NguoiDung 
    SET MaVaiTro = @MaVaiTroLanhDao,
        TrangThaiHoatDong = 1
    WHERE TenDangNhap = 'lanhdao';
    PRINT N'Đã cập nhật vai trò LanhDao cho tài khoản lanhdao';
END
GO

-- Hiển thị kết quả
SELECT nd.MaNguoiDung, nd.TenDangNhap, nd.HoTen, nd.Email, vt.TenVaiTro, nd.TrangThaiHoatDong
FROM NguoiDung nd
INNER JOIN VaiTro vt ON nd.MaVaiTro = vt.MaVaiTro
WHERE vt.TenVaiTro = 'LanhDao';

GO
