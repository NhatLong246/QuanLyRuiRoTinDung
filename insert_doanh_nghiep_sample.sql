-- =============================================
-- INSERT DỮ LIỆU MẪU KHÁCH HÀNG DOANH NGHIỆP
-- =============================================
-- Script này thêm dữ liệu mẫu cho bảng KhachHang_DoanhNghiep
-- Bao gồm đầy đủ thông tin người đại diện pháp luật (tên, CCCD, ngày sinh, giới tính)

USE QuanLyRuiRoTinDung
GO

-- Kiểm tra xem đã có dữ liệu mẫu chưa
IF NOT EXISTS (SELECT 1 FROM KhachHang_DoanhNghiep WHERE MaKhachHang_Code = 'DN0001')
BEGIN
    -- Lấy MaNguoiDung đầu tiên làm NguoiTao (thường là admin hoặc nhân viên)
    DECLARE @NguoiTao INT;
    SELECT TOP 1 @NguoiTao = MaNguoiDung FROM NguoiDung ORDER BY MaNguoiDung;
    
    -- Nếu chưa có người dùng nào, tạo một người dùng mặc định
    IF @NguoiTao IS NULL
    BEGIN
        -- Tạo vai trò nếu chưa có
        IF NOT EXISTS (SELECT 1 FROM VaiTro WHERE TenVaiTro = N'Admin')
        BEGIN
            INSERT INTO VaiTro (TenVaiTro, MoTa, TrangThaiHoatDong)
            VALUES (N'Admin', N'Quản trị viên hệ thống', 1);
        END
        
        -- Tạo người dùng admin mặc định
        INSERT INTO NguoiDung (TenDangNhap, MatKhauHash, HoTen, MaVaiTro, TrangThaiHoatDong)
        VALUES ('admin', 'admin123', N'Quản trị viên', (SELECT MaVaiTro FROM VaiTro WHERE TenVaiTro = N'Admin'), 1);
        
        SET @NguoiTao = SCOPE_IDENTITY();
    END

    -- 1. Doanh nghiệp mẫu 1: Công ty TNHH ABC
    INSERT INTO KhachHang_DoanhNghiep (
        MaKhachHang_Code,
        TenCongTy,
        MaSoThue,
        SoGiayPhepKinhDoanh,
        NgayCapGiayPhep,
        NgayDangKy,
        NguoiDaiDienPhapLuat,
        SoCCCD_NguoiDaiDienPhapLuat,
        SoDienThoai,
        Email,
        DiaChi,
        ThanhPho,
        Quan,
        Phuong,
        LinhVucKinhDoanh,
        SoLuongNhanVien,
        DoanhThuHangNam,
        TongTaiSan,
        VonDieuLe,
        DiemTinDung,
        XepHangTinDung,
        NgaySinh,
        GioiTinh,
        NguoiTao,
        TrangThaiHoatDong
    )
    VALUES (
        'DN0001',
        N'Công ty TNHH Thương mại và Dịch vụ ABC',
        '0123456789',
        '41-123456789',
        '2020-01-15',
        '2020-01-10',
        N'Nguyễn Văn A',
        '001234567890',  -- Số CCCD của người đại diện pháp luật
        '02438567890',
        'contact@abc-company.com',
        N'Số 123, Đường ABC',
        N'Hà Nội',
        N'Quận Cầu Giấy',
        N'Phường Dịch Vọng',
        N'Thương mại và Dịch vụ',
        50,
        5000000000.00,  -- 5 tỷ VNĐ
        10000000000.00, -- 10 tỷ VNĐ
        2000000000.00,  -- 2 tỷ VNĐ
        750,
        'AA',
        '1980-05-15',  -- Ngày sinh người đại diện pháp luật
        N'Nam',         -- Giới tính người đại diện pháp luật
        @NguoiTao,
        1
    );

    -- 2. Doanh nghiệp mẫu 2: Công ty Cổ phần XYZ
    INSERT INTO KhachHang_DoanhNghiep (
        MaKhachHang_Code,
        TenCongTy,
        MaSoThue,
        SoGiayPhepKinhDoanh,
        NgayCapGiayPhep,
        NgayDangKy,
        NguoiDaiDienPhapLuat,
        SoCCCD_NguoiDaiDienPhapLuat,
        SoDienThoai,
        Email,
        DiaChi,
        ThanhPho,
        Quan,
        Phuong,
        LinhVucKinhDoanh,
        SoLuongNhanVien,
        DoanhThuHangNam,
        TongTaiSan,
        VonDieuLe,
        DiemTinDung,
        XepHangTinDung,
        NgaySinh,
        GioiTinh,
        NguoiTao,
        TrangThaiHoatDong
    )
    VALUES (
        'DN0002',
        N'Công ty Cổ phần Sản xuất và Kinh doanh XYZ',
        '0987654321',
        '41-987654321',
        '2018-06-20',
        '2018-06-15',
        N'Trần Thị B',
        '098765432109',  -- Số CCCD của người đại diện pháp luật
        '02812345678',
        'info@xyz-corp.vn',
        N'Số 456, Đường XYZ',
        N'Thành phố Hồ Chí Minh',
        N'Quận 1',
        N'Phường Bến Nghé',
        N'Sản xuất và Kinh doanh',
        150,
        15000000000.00, -- 15 tỷ VNĐ
        30000000000.00, -- 30 tỷ VNĐ
        5000000000.00,  -- 5 tỷ VNĐ
        850,
        'AAA',
        '1975-08-22',  -- Ngày sinh người đại diện pháp luật
        N'Nữ',         -- Giới tính người đại diện pháp luật
        @NguoiTao,
        1
    );

    -- 3. Doanh nghiệp mẫu 3: Công ty TNHH Công nghệ DEF
    INSERT INTO KhachHang_DoanhNghiep (
        MaKhachHang_Code,
        TenCongTy,
        MaSoThue,
        SoGiayPhepKinhDoanh,
        NgayCapGiayPhep,
        NgayDangKy,
        NguoiDaiDienPhapLuat,
        SoCCCD_NguoiDaiDienPhapLuat,
        SoDienThoai,
        Email,
        DiaChi,
        ThanhPho,
        Quan,
        Phuong,
        LinhVucKinhDoanh,
        SoLuongNhanVien,
        DoanhThuHangNam,
        TongTaiSan,
        VonDieuLe,
        DiemTinDung,
        XepHangTinDung,
        NgaySinh,
        GioiTinh,
        NguoiTao,
        TrangThaiHoatDong
    )
    VALUES (
        'DN0003',
        N'Công ty TNHH Công nghệ Thông tin DEF',
        '0555666777',
        '41-555666777',
        '2021-03-10',
        '2021-03-05',
        N'Lê Văn C',
        '055566677788',  -- Số CCCD của người đại diện pháp luật
        '02345678901',
        'contact@def-tech.com',
        N'Số 789, Đường DEF',
        N'Đà Nẵng',
        N'Quận Hải Châu',
        N'Phường Thanh Bình',
        N'Công nghệ thông tin',
        30,
        2000000000.00,  -- 2 tỷ VNĐ
        5000000000.00,  -- 5 tỷ VNĐ
        1000000000.00,  -- 1 tỷ VNĐ
        700,
        'A',
        '1985-11-30',  -- Ngày sinh người đại diện pháp luật
        N'Nam',         -- Giới tính người đại diện pháp luật
        @NguoiTao,
        1
    );

    PRINT N'Đã thêm 3 doanh nghiệp mẫu thành công!';
    PRINT N'DN0001: Công ty TNHH ABC';
    PRINT N'DN0002: Công ty Cổ phần XYZ';
    PRINT N'DN0003: Công ty TNHH Công nghệ DEF';
END
ELSE
BEGIN
    PRINT N'Dữ liệu mẫu đã tồn tại. Bỏ qua việc insert.';
END
GO
