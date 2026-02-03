-- =============================================
-- INSERT DỮ LIỆU MẪU CHO BẢNG THÔNG TIN CIC
-- Mẫu danh sách khách hàng có sẵn trong CIC quốc gia
-- =============================================

USE QuanLyRuiRoTinDung
GO

-- Xóa dữ liệu cũ nếu có (tùy chọn)
-- DELETE FROM ThongTin_CIC;
-- GO

-- 1. KHÁCH HÀNG TỐT - Điểm tín dụng cao, không nợ xấu
-- =============================================
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
    'CIC0001', 'CaNhan', NULL,
    '001234567890', NULL, N'Nguyễn Văn An',
    3, 1, 2,
    0, 0,
    50000000, 0, 0, 200000000, 500000000,
    850, 'AA', N'Thấp', N'Tốt',
    0, 0, 0,
    24, 100.00,
    N'[{"TenNganHang":"Vietcombank","SoKhoanVay":2},{"TenNganHang":"BIDV","SoKhoanVay":1}]', 2,
    N'Khách hàng có lịch sử tín dụng tốt, luôn trả nợ đúng hạn. Điểm tín dụng cao, không có nợ xấu.', 
    N'ChoVay', 
    N'Khách hàng có khả năng trả nợ tốt, lịch sử tín dụng sạch, điểm CIC cao. Nên cho vay với điều kiện bình thường.',
    GETDATE(), 1, N'ThanhCong',
    1, 1
);
GO

-- 2. KHÁCH HÀNG TRUNG BÌNH - Có một số khoản vay, đôi khi quá hạn nhẹ
-- =============================================
INSERT INTO ThongTin_CIC (
    MaCIC_Code, LoaiKhachHang, MaKhachHang,
    SoCMND_CCCD, MaSoThue, HoTen,
    TongSoKhoanVayCIC, SoKhoanVayDangVayCIC, SoKhoanVayDaTraXongCIC,
    SoKhoanVayQuaHanCIC, SoKhoanVayNoXauCIC,
    TongDuNoCIC, DuNoQuaHanCIC, DuNoNoXauCIC, DuNoToiDaCIC, TongGiaTriVayCIC,
    DiemTinDungCIC, XepHangTinDungCIC, MucDoRuiRo, KhaNangTraNo,
    SoLanQuaHanCIC, SoLanNoXauCIC, SoNgayQuaHanToiDaCIC,
    NgayQuaHanLanCuoiCIC, ThoiGianTraNoTotCIC, TyLeTraNoDungHanCIC,
    DanhSachToChucTinDung, SoToChucTinDungDaVay,
    DanhGiaTongQuat, KhuyenNghiChoVay, LyDoKhuyenNghi,
    NgayTraCuuCuoi, NguoiTraCuu, KetQuaTraCuu,
    NguoiTao, TrangThaiHoatDong
)
VALUES (
    'CIC0002', 'CaNhan', NULL,
    '002345678901', NULL, N'Trần Thị Bình',
    5, 2, 3,
    1, 0,
    150000000, 20000000, 0, 300000000, 800000000,
    650, 'BBB', N'Trung bình', N'Khá',
    2, 0, 15,
    DATEADD(DAY, -30, GETDATE()), 18, 85.50,
    N'[{"TenNganHang":"Techcombank","SoKhoanVay":2},{"TenNganHang":"ACB","SoKhoanVay":2},{"TenNganHang":"VPBank","SoKhoanVay":1}]', 3,
    N'Khách hàng có lịch sử tín dụng trung bình, đã từng quá hạn nhẹ nhưng đã trả đủ. Cần theo dõi chặt chẽ.', 
    N'CanThanTrong', 
    N'Khách hàng có một số khoản vay đang vay, đã từng quá hạn nhưng đã xử lý. Nên cho vay với điều kiện chặt chẽ hơn, yêu cầu tài sản đảm bảo.',
    GETDATE(), 1, N'ThanhCong',
    1, 1
);
GO

-- 3. KHÁCH HÀNG RỦI RO - Có nợ xấu, quá hạn nhiều
-- =============================================
INSERT INTO ThongTin_CIC (
    MaCIC_Code, LoaiKhachHang, MaKhachHang,
    SoCMND_CCCD, MaSoThue, HoTen,
    TongSoKhoanVayCIC, SoKhoanVayDangVayCIC, SoKhoanVayDaTraXongCIC,
    SoKhoanVayQuaHanCIC, SoKhoanVayNoXauCIC,
    TongDuNoCIC, DuNoQuaHanCIC, DuNoNoXauCIC, DuNoToiDaCIC, TongGiaTriVayCIC,
    DiemTinDungCIC, XepHangTinDungCIC, MucDoRuiRo, KhaNangTraNo,
    SoLanQuaHanCIC, SoLanNoXauCIC, SoNgayQuaHanToiDaCIC,
    NgayQuaHanLanCuoiCIC, NgayNoXauLanCuoiCIC, ThoiGianTraNoTotCIC, TyLeTraNoDungHanCIC,
    DanhSachToChucTinDung, SoToChucTinDungDaVay,
    DanhGiaTongQuat, KhuyenNghiChoVay, LyDoKhuyenNghi,
    NgayTraCuuCuoi, NguoiTraCuu, KetQuaTraCuu,
    NguoiTao, TrangThaiHoatDong
)
VALUES (
    'CIC0003', 'CaNhan', NULL,
    '003456789012', NULL, N'Lê Văn Cường',
    4, 2, 1,
    2, 1,
    300000000, 150000000, 80000000, 500000000, 1200000000,
    420, 'D', N'Rất cao', N'Rất yếu',
    5, 1, 90,
    DATEADD(DAY, -15, GETDATE()), DATEADD(DAY, -60, GETDATE()), 6, 45.00,
    N'[{"TenNganHang":"Vietinbank","SoKhoanVay":2},{"TenNganHang":"Sacombank","SoKhoanVay":1},{"TenNganHang":"MBBank","SoKhoanVay":1}]', 3,
    N'Khách hàng có lịch sử tín dụng xấu, đang có nợ xấu và quá hạn. Rủi ro cao.', 
    N'KhongChoVay', 
    N'Khách hàng đang có nợ xấu, quá hạn nhiều lần, điểm CIC thấp. Không nên cho vay trong thời điểm hiện tại.',
    GETDATE(), 1, N'ThanhCong',
    1, 1
);
GO

-- 4. DOANH NGHIỆP TỐT - Công ty có tín dụng tốt
-- =============================================
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
    'CIC0004', 'DoanhNghiep', NULL,
    '012345678901', '0101234567', N'Công ty TNHH Thương mại ABC',
    8, 3, 5,
    0, 0,
    2000000000, 0, 0, 5000000000, 15000000000,
    920, 'AAA', N'Thấp', N'Tốt',
    0, 0, 0,
    36, 100.00,
    N'[{"TenNganHang":"Vietcombank","SoKhoanVay":3},{"TenNganHang":"BIDV","SoKhoanVay":2},{"TenNganHang":"Vietinbank","SoKhoanVay":2},{"TenNganHang":"Techcombank","SoKhoanVay":1}]', 4,
    N'Doanh nghiệp có lịch sử tín dụng xuất sắc, luôn trả nợ đúng hạn. Điểm tín dụng rất cao, không có nợ xấu.', 
    N'ChoVay', 
    N'Doanh nghiệp có khả năng tài chính tốt, lịch sử tín dụng sạch, điểm CIC rất cao. Nên cho vay với điều kiện ưu đãi.',
    GETDATE(), 1, N'ThanhCong',
    1, 1
);
GO

-- 5. KHÁCH HÀNG MỚI - Chưa có lịch sử tín dụng
-- =============================================
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
    'CIC0005', 'CaNhan', NULL,
    '004567890123', NULL, N'Phạm Thị Dung',
    0, 0, 0,
    0, 0,
    0, 0, 0, 0, 0,
    NULL, NULL, NULL, NULL,
    0, 0, 0,
    0, 0.00,
    NULL, 0,
    N'Khách hàng chưa có lịch sử tín dụng trong hệ thống CIC. Cần đánh giá dựa trên thông tin khác.', 
    N'CanThanTrong', 
    N'Khách hàng mới, chưa có lịch sử tín dụng. Nên yêu cầu tài sản đảm bảo và đánh giá kỹ khả năng tài chính.',
    GETDATE(), 1, N'KhongCoThongTin',
    1, 1
);
GO

-- 6. DOANH NGHIỆP RỦI RO - Có nợ xấu
-- =============================================
INSERT INTO ThongTin_CIC (
    MaCIC_Code, LoaiKhachHang, MaKhachHang,
    SoCMND_CCCD, MaSoThue, HoTen,
    TongSoKhoanVayCIC, SoKhoanVayDangVayCIC, SoKhoanVayDaTraXongCIC,
    SoKhoanVayQuaHanCIC, SoKhoanVayNoXauCIC,
    TongDuNoCIC, DuNoQuaHanCIC, DuNoNoXauCIC, DuNoToiDaCIC, TongGiaTriVayCIC,
    DiemTinDungCIC, XepHangTinDungCIC, MucDoRuiRo, KhaNangTraNo,
    SoLanQuaHanCIC, SoLanNoXauCIC, SoNgayQuaHanToiDaCIC,
    NgayQuaHanLanCuoiCIC, NgayNoXauLanCuoiCIC, ThoiGianTraNoTotCIC, TyLeTraNoDungHanCIC,
    DanhSachToChucTinDung, SoToChucTinDungDaVay,
    DanhGiaTongQuat, KhuyenNghiChoVay, LyDoKhuyenNghi,
    NgayTraCuuCuoi, NguoiTraCuu, KetQuaTraCuu,
    NguoiTao, TrangThaiHoatDong
)
VALUES (
    'CIC0006', 'DoanhNghiep', NULL,
    '023456789012', '0109876543', N'Công ty CP Sản xuất XYZ',
    6, 2, 2,
    2, 1,
    5000000000, 2000000000, 1500000000, 8000000000, 20000000000,
    380, 'D', N'Rất cao', N'Rất yếu',
    8, 2, 120,
    DATEADD(DAY, -20, GETDATE()), DATEADD(DAY, -90, GETDATE()), 12, 35.00,
    N'[{"TenNganHang":"Vietinbank","SoKhoanVay":2},{"TenNganHang":"BIDV","SoKhoanVay":2},{"TenNganHang":"ACB","SoKhoanVay":1},{"TenNganHang":"VPBank","SoKhoanVay":1}]', 4,
    N'Doanh nghiệp có lịch sử tín dụng xấu, đang có nợ xấu lớn và quá hạn nhiều. Rủi ro rất cao.', 
    N'KhongChoVay', 
    N'Doanh nghiệp đang có nợ xấu lớn, quá hạn nhiều lần, điểm CIC rất thấp. Không nên cho vay.',
    GETDATE(), 1, N'ThanhCong',
    1, 1
);
GO

-- 7. KHÁCH HÀNG TỐT - Đã trả xong tất cả khoản vay
-- =============================================
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
    'CIC0007', 'CaNhan', NULL,
    '005678901234', NULL, N'Hoàng Văn Em',
    4, 0, 4,
    0, 0,
    0, 0, 0, 150000000, 600000000,
    880, 'AAA', N'Thấp', N'Tốt',
    0, 0, 0,
    48, 100.00,
    N'[{"TenNganHang":"Vietcombank","SoKhoanVay":2},{"TenNganHang":"Techcombank","SoKhoanVay":1},{"TenNganHang":"ACB","SoKhoanVay":1}]', 3,
    N'Khách hàng có lịch sử tín dụng xuất sắc, đã trả xong tất cả các khoản vay đúng hạn. Điểm tín dụng rất cao.', 
    N'ChoVay', 
    N'Khách hàng có lịch sử tín dụng hoàn hảo, đã trả xong tất cả khoản vay. Nên cho vay với điều kiện ưu đãi.',
    GETDATE(), 1, N'ThanhCong',
    1, 1
);
GO

PRINT N'Đã thêm 7 bản ghi mẫu vào bảng ThongTin_CIC thành công!';
PRINT N'Bao gồm:';
PRINT N'  - 2 khách hàng tốt (CIC0001, CIC0007)';
PRINT N'  - 1 khách hàng trung bình (CIC0002)';
PRINT N'  - 1 khách hàng rủi ro (CIC0003)';
PRINT N'  - 1 khách hàng mới (CIC0005)';
PRINT N'  - 1 doanh nghiệp tốt (CIC0004)';
PRINT N'  - 1 doanh nghiệp rủi ro (CIC0006)';
