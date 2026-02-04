-- =============================================
-- INSERT DỮ LIỆU CIC CHO DOANH NGHIỆP MỚI
-- Đảm bảo có thể tạo được tài khoản doanh nghiệp
-- =============================================

USE QuanLyRuiRoTinDung
GO

-- =============================================
-- DOANH NGHIỆP MỚI - CIC TỐT (Có thể tạo tài khoản)
-- Thông tin cần khớp khi tạo khách hàng doanh nghiệp:
--   - MaSoThue: 0312345678001 (13 chữ số)
--   - SoCMND_CCCD (người đại diện): 079123456789 (12 chữ số)
--   - HoTen (tên công ty): Công ty TNHH Công Nghệ Việt Nam
-- =============================================

-- Kiểm tra xem đã tồn tại chưa
IF NOT EXISTS (SELECT 1 FROM ThongTin_CIC WHERE MaCIC_Code = 'CIC_DN001' OR MaSoThue = '0312345678001')
BEGIN
    INSERT INTO ThongTin_CIC (
        MaCIC_Code, LoaiKhachHang, MaKhachHang,
        SoCMND_CCCD, MaSoThue, HoTen,
        TongSoKhoanVayCIC, SoKhoanVayDangVayCIC, SoKhoanVayDaTraXongCIC,
        SoKhoanVayQuaHanCIC, SoKhoanVayNoXauCIC,
        TongDuNoCIC, DuNoQuaHanCIC, DuNoNoXauCIC, DuNoToiDaCIC, TongGiaTriVayCIC,
        DiemTinDungCIC, XepHangTinDungCIC, MucDoRuiRo, KhaNangTraNo,
        SoLanQuaHanCIC, SoLanNoXauCIC, SoNgayQuaHanToiDaCIC,
        ThoiGianTraNoTotCIC, TyLeTraNoDungHanCIC,
        DanhSachToChucTinDung, SoToChucTinDungDaVay,
        DanhGiaTongQuat, KhuyenNghiChoVay, LyDoKhuyenNghi,
        NgayTraCuuCuoi, NguoiTraCuu, KetQuaTraCuu,
        NguoiTao, TrangThaiHoatDong
    )
    VALUES (
        'CIC_DN001',                    -- Mã CIC
        'DoanhNghiep',                  -- Loại khách hàng: DoanhNghiep
        NULL,                           -- MaKhachHang (sẽ link sau khi tạo)
        '079123456789',                 -- CCCD người đại diện pháp luật (12 chữ số)
        '0312345678001',                -- Mã số thuế (13 chữ số: 10 + 3 phụ)
        N'Công ty TNHH Công Nghệ Việt Nam',  -- Tên công ty (HoTen)
        5, 2, 3,                        -- Số khoản vay: tổng=5, đang vay=2, trả xong=3
        0, 0,                           -- Quá hạn=0, Nợ xấu=0
        500000000, 0, 0, 2000000000, 3000000000,  -- Dư nợ (đơn vị: VNĐ)
        850, 'AA', N'Thấp', N'Tốt',     -- Điểm tín dụng tốt, rủi ro thấp
        0, 0, 0,                        -- Không có lần quá hạn/nợ xấu
        36, 100.00,                     -- Trả nợ tốt 36 tháng, 100% đúng hạn
        N'[{"TenNganHang":"Vietcombank","SoKhoanVay":2},{"TenNganHang":"BIDV","SoKhoanVay":2},{"TenNganHang":"Techcombank","SoKhoanVay":1}]', 
        3,                              -- Đã vay tại 3 tổ chức tín dụng
        N'Doanh nghiệp có lịch sử tín dụng tốt, luôn trả nợ đúng hạn. Điểm tín dụng cao, không có nợ xấu.', 
        N'ChoVay', 
        N'Doanh nghiệp có khả năng tài chính tốt, lịch sử tín dụng sạch, điểm CIC cao. Nên cho vay với điều kiện bình thường.',
        GETDATE(), 1, N'ThanhCong',
        1, 1                            -- TrangThaiHoatDong = 1 (Active)
    );
    
    PRINT N'✓ Đã thêm CIC doanh nghiệp CIC_DN001 thành công!';
END
ELSE
BEGIN
    PRINT N'⚠ CIC doanh nghiệp CIC_DN001 đã tồn tại.';
END
GO

-- =============================================
-- HƯỚNG DẪN TẠO KHÁCH HÀNG DOANH NGHIỆP
-- =============================================
PRINT N'';
PRINT N'========================================';
PRINT N'HƯỚNG DẪN TẠO KHÁCH HÀNG DOANH NGHIỆP';
PRINT N'========================================';
PRINT N'Khi tạo khách hàng doanh nghiệp mới, nhập:';
PRINT N'';
PRINT N'1. Mã số thuế: 0312345678-001 hoặc 0312345678001';
PRINT N'2. Tên công ty: Công ty TNHH Công Nghệ Việt Nam';
PRINT N'3. Số CCCD người đại diện: 079123456789';
PRINT N'4. Tên người đại diện: Có thể nhập tên bất kỳ';
PRINT N'';
PRINT N'Lưu ý: Mã số thuế, Tên công ty và CCCD người đại diện';
PRINT N'       PHẢI khớp với thông tin trong bảng CIC!';
PRINT N'========================================';
GO

-- =============================================
-- THÊM CIC DOANH NGHIỆP THỨ 2 (Tùy chọn)
-- =============================================
IF NOT EXISTS (SELECT 1 FROM ThongTin_CIC WHERE MaCIC_Code = 'CIC_DN002' OR MaSoThue = '0305678901001')
BEGIN
    INSERT INTO ThongTin_CIC (
        MaCIC_Code, LoaiKhachHang, MaKhachHang,
        SoCMND_CCCD, MaSoThue, HoTen,
        TongSoKhoanVayCIC, SoKhoanVayDangVayCIC, SoKhoanVayDaTraXongCIC,
        SoKhoanVayQuaHanCIC, SoKhoanVayNoXauCIC,
        TongDuNoCIC, DuNoQuaHanCIC, DuNoNoXauCIC, DuNoToiDaCIC, TongGiaTriVayCIC,
        DiemTinDungCIC, XepHangTinDungCIC, MucDoRuiRo, KhaNangTraNo,
        SoLanQuaHanCIC, SoLanNoXauCIC, SoNgayQuaHanToiDaCIC,
        ThoiGianTraNoTotCIC, TyLeTraNoDungHanCIC,
        DanhSachToChucTinDung, SoToChucTinDungDaVay,
        DanhGiaTongQuat, KhuyenNghiChoVay, LyDoKhuyenNghi,
        NgayTraCuuCuoi, NguoiTraCuu, KetQuaTraCuu,
        NguoiTao, TrangThaiHoatDong
    )
    VALUES (
        'CIC_DN002',                    -- Mã CIC
        'DoanhNghiep',                  -- Loại khách hàng
        NULL,                           
        '034567890123',                 -- CCCD người đại diện
        '0305678901001',                -- Mã số thuế
        N'Công ty Cổ phần Thương mại Sài Gòn',  -- Tên công ty
        3, 1, 2,                        
        0, 0,                           
        1000000000, 0, 0, 5000000000, 8000000000,
        900, 'AAA', N'Thấp', N'Rất tốt',
        0, 0, 0,                        
        48, 100.00,                     
        N'[{"TenNganHang":"Vietcombank","SoKhoanVay":2},{"TenNganHang":"ACB","SoKhoanVay":1}]', 
        2,                              
        N'Doanh nghiệp có lịch sử tín dụng xuất sắc, luôn trả nợ đúng hạn. Điểm tín dụng rất cao.', 
        N'ChoVay', 
        N'Doanh nghiệp uy tín, tài chính lành mạnh. Đủ điều kiện cho vay với lãi suất ưu đãi.',
        GETDATE(), 1, N'ThanhCong',
        1, 1                            
    );
    
    PRINT N'✓ Đã thêm CIC doanh nghiệp CIC_DN002 thành công!';
END
GO

-- =============================================
-- THÊM CIC DOANH NGHIỆP MỚI (Chưa có lịch sử)
-- =============================================
IF NOT EXISTS (SELECT 1 FROM ThongTin_CIC WHERE MaCIC_Code = 'CIC_DN003' OR MaSoThue = '0309876543001')
BEGIN
    INSERT INTO ThongTin_CIC (
        MaCIC_Code, LoaiKhachHang, MaKhachHang,
        SoCMND_CCCD, MaSoThue, HoTen,
        TongSoKhoanVayCIC, SoKhoanVayDangVayCIC, SoKhoanVayDaTraXongCIC,
        SoKhoanVayQuaHanCIC, SoKhoanVayNoXauCIC,
        TongDuNoCIC, DuNoQuaHanCIC, DuNoNoXauCIC, DuNoToiDaCIC, TongGiaTriVayCIC,
        DiemTinDungCIC, XepHangTinDungCIC, MucDoRuiRo, KhaNangTraNo,
        SoLanQuaHanCIC, SoLanNoXauCIC, SoNgayQuaHanToiDaCIC,
        ThoiGianTraNoTotCIC, TyLeTraNoDungHanCIC,
        DanhSachToChucTinDung, SoToChucTinDungDaVay,
        DanhGiaTongQuat, KhuyenNghiChoVay, LyDoKhuyenNghi,
        NgayTraCuuCuoi, NguoiTraCuu, KetQuaTraCuu,
        NguoiTao, TrangThaiHoatDong
    )
    VALUES (
        'CIC_DN003',                    
        'DoanhNghiep',                  
        NULL,                           
        '056789012345',                 -- CCCD người đại diện
        '0309876543001',                -- Mã số thuế
        N'Công ty TNHH Dịch vụ Miền Nam',  -- Tên công ty
        0, 0, 0,                        -- Chưa có khoản vay nào
        0, 0,                           
        0, 0, 0, 0, 0,                  -- Không có dư nợ
        NULL, NULL, NULL, NULL,         -- Chưa xếp hạng
        0, 0, 0,                        
        0, 0,                           
        NULL, 0,                        
        N'Doanh nghiệp mới thành lập, chưa có lịch sử tín dụng trong hệ thống CIC.', 
        N'CanThanTrong', 
        N'Doanh nghiệp mới, chưa có lịch sử. Cần đánh giá kỹ tài chính và yêu cầu tài sản đảm bảo.',
        GETDATE(), 1, N'KhongCoThongTin',
        1, 1                            
    );
    
    PRINT N'✓ Đã thêm CIC doanh nghiệp CIC_DN003 (doanh nghiệp mới) thành công!';
END
GO

-- =============================================
-- TỔNG KẾT CÁC CIC DOANH NGHIỆP ĐÃ THÊM
-- =============================================
PRINT N'';
PRINT N'========================================';
PRINT N'DANH SÁCH CIC DOANH NGHIỆP CÓ THỂ DÙNG';
PRINT N'========================================';
PRINT N'';
PRINT N'1. CIC_DN001 - Công ty TNHH Công Nghệ Việt Nam';
PRINT N'   - MST: 0312345678001 (hoặc 0312345678-001)';
PRINT N'   - CCCD người đại diện: 079123456789';
PRINT N'   - Xếp hạng: AA (Tốt)';
PRINT N'';
PRINT N'2. CIC_DN002 - Công ty Cổ phần Thương mại Sài Gòn';
PRINT N'   - MST: 0305678901001 (hoặc 0305678901-001)';
PRINT N'   - CCCD người đại diện: 034567890123';
PRINT N'   - Xếp hạng: AAA (Xuất sắc)';
PRINT N'';
PRINT N'3. CIC_DN003 - Công ty TNHH Dịch vụ Miền Nam';
PRINT N'   - MST: 0309876543001 (hoặc 0309876543-001)';
PRINT N'   - CCCD người đại diện: 056789012345';
PRINT N'   - Xếp hạng: Chưa có (Doanh nghiệp mới)';
PRINT N'========================================';
GO
