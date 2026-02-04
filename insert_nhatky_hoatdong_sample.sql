-- =============================================
-- Tạo dữ liệu mẫu cho bảng NhatKy_HoatDong
-- Bao gồm: Đăng nhập, Đăng xuất, Upload file
-- =============================================

USE QuanLyRuiRoTinDung;
GO

-- Xóa dữ liệu cũ nếu cần (tùy chọn)
-- DELETE FROM NhatKy_HoatDong;

-- Lấy MaNguoiDung của admin
DECLARE @MaAdmin INT = (SELECT TOP 1 MaNguoiDung FROM NguoiDung WHERE TenDangNhap = 'admin');
DECLARE @MaQLRR INT = (SELECT TOP 1 MaNguoiDung FROM NguoiDung WHERE TenDangNhap = 'qlrr2');
DECLARE @MaNVTD INT = (SELECT TOP 1 MaNguoiDung FROM NguoiDung WHERE TenDangNhap LIKE 'nvtd%');

-- Nếu không có user, dùng ID mặc định
IF @MaAdmin IS NULL SET @MaAdmin = 1;
IF @MaQLRR IS NULL SET @MaQLRR = 2;
IF @MaNVTD IS NULL SET @MaNVTD = 3;

-- Nhật ký đăng nhập mẫu
INSERT INTO NhatKy_HoatDong (MaNguoiDung, HanhDong, TenBang, MaBanGhi, GiaTriCu, GiaTriMoi, DiaChiIP, ThoiGian)
VALUES 
    (@MaAdmin, N'Đăng nhập hệ thống', N'NguoiDung', @MaAdmin, NULL, N'Tài khoản: admin đăng nhập thành công', N'127.0.0.1', DATEADD(HOUR, -2, GETDATE())),
    (@MaQLRR, N'Đăng nhập hệ thống', N'NguoiDung', @MaQLRR, NULL, N'Tài khoản: qlrr2 đăng nhập thành công', N'192.168.1.100', DATEADD(HOUR, -3, GETDATE())),
    (@MaNVTD, N'Đăng nhập hệ thống', N'NguoiDung', @MaNVTD, NULL, N'Tài khoản: nvtd đăng nhập thành công', N'192.168.1.101', DATEADD(HOUR, -4, GETDATE())),
    (@MaAdmin, N'Đăng nhập hệ thống', N'NguoiDung', @MaAdmin, NULL, N'Tài khoản: admin đăng nhập thành công', N'127.0.0.1', DATEADD(DAY, -1, GETDATE())),
    (@MaQLRR, N'Đăng nhập hệ thống', N'NguoiDung', @MaQLRR, NULL, N'Tài khoản: qlrr2 đăng nhập thành công', N'192.168.1.100', DATEADD(DAY, -1, DATEADD(HOUR, -5, GETDATE())));

-- Nhật ký đăng xuất mẫu
INSERT INTO NhatKy_HoatDong (MaNguoiDung, HanhDong, TenBang, MaBanGhi, GiaTriCu, GiaTriMoi, DiaChiIP, ThoiGian)
VALUES 
    (@MaAdmin, N'Đăng xuất hệ thống', N'NguoiDung', @MaAdmin, NULL, N'Tài khoản: admin đã đăng xuất', N'127.0.0.1', DATEADD(HOUR, -1, GETDATE())),
    (@MaQLRR, N'Đăng xuất hệ thống', N'NguoiDung', @MaQLRR, NULL, N'Tài khoản: qlrr2 đã đăng xuất', N'192.168.1.100', DATEADD(HOUR, -2, GETDATE())),
    (@MaNVTD, N'Đăng xuất hệ thống', N'NguoiDung', @MaNVTD, NULL, N'Tài khoản: nvtd đã đăng xuất', N'192.168.1.101', DATEADD(HOUR, -3, GETDATE()));

-- Nhật ký upload file mẫu
INSERT INTO NhatKy_HoatDong (MaNguoiDung, HanhDong, TenBang, MaBanGhi, GiaTriCu, GiaTriMoi, DiaChiIP, ThoiGian)
VALUES 
    (@MaNVTD, N'Upload file đính kèm', N'HoSoVayFileDinhKem', 1, NULL, N'Tên file: CMND_MatTruoc.jpg', N'192.168.1.101', DATEADD(HOUR, -5, GETDATE())),
    (@MaNVTD, N'Upload file đính kèm', N'HoSoVayFileDinhKem', 1, NULL, N'Tên file: CMND_MatSau.jpg', N'192.168.1.101', DATEADD(HOUR, -5, GETDATE())),
    (@MaNVTD, N'Upload file đính kèm', N'HoSoVayFileDinhKem', 2, NULL, N'Tên file: HopDongLaoDong.pdf', N'192.168.1.101', DATEADD(HOUR, -4, GETDATE())),
    (@MaNVTD, N'Upload file đính kèm', N'HoSoVayFileDinhKem', 2, NULL, N'Tên file: GiayXacNhanLuong.pdf', N'192.168.1.101', DATEADD(HOUR, -4, GETDATE())),
    (@MaQLRR, N'Upload file đính kèm', N'HoSoVayFileDinhKem', 3, NULL, N'Tên file: BaoCaoThamDinh.pdf', N'192.168.1.100', DATEADD(DAY, -1, GETDATE()));

-- Nhật ký tạo hồ sơ vay mẫu
INSERT INTO NhatKy_HoatDong (MaNguoiDung, HanhDong, TenBang, MaBanGhi, GiaTriCu, GiaTriMoi, DiaChiIP, ThoiGian)
VALUES 
    (@MaNVTD, N'Tạo hồ sơ vay mới', N'KhoanVay', 1, NULL, N'Mã hồ sơ: LOAN0001 - Vay tiêu dùng - 100,000,000 VNĐ', N'192.168.1.101', DATEADD(DAY, -2, GETDATE())),
    (@MaNVTD, N'Tạo hồ sơ vay mới', N'KhoanVay', 2, NULL, N'Mã hồ sơ: LOAN0002 - Vay mua nhà - 500,000,000 VNĐ', N'192.168.1.101', DATEADD(DAY, -1, GETDATE()));

-- Nhật ký duyệt hồ sơ mẫu
INSERT INTO NhatKy_HoatDong (MaNguoiDung, HanhDong, TenBang, MaBanGhi, GiaTriCu, GiaTriMoi, DiaChiIP, ThoiGian)
VALUES 
    (@MaQLRR, N'Đánh giá rủi ro', N'DanhGiaRuiRo', 1, NULL, N'Khoản vay LOAN0001 - Mức rủi ro: Thấp - Xếp hạng: AAA', N'192.168.1.100', DATEADD(HOUR, -6, GETDATE())),
    (@MaAdmin, N'Phê duyệt hồ sơ', N'KhoanVay', 1, N'Trạng thái: Chờ duyệt', N'Trạng thái: Đã phê duyệt', N'127.0.0.1', DATEADD(HOUR, -5, GETDATE()));

-- Nhật ký cập nhật thông tin
INSERT INTO NhatKy_HoatDong (MaNguoiDung, HanhDong, TenBang, MaBanGhi, GiaTriCu, GiaTriMoi, DiaChiIP, ThoiGian)
VALUES 
    (@MaAdmin, N'Cập nhật cấu hình hệ thống', N'CauHinhHeThong', 1, N'LaiSuatCoSo: 8.5%', N'LaiSuatCoSo: 9.0%', N'127.0.0.1', DATEADD(DAY, -3, GETDATE())),
    (@MaAdmin, N'Tạo tài khoản mới', N'NguoiDung', @MaNVTD, NULL, N'Tạo tài khoản nhân viên tín dụng mới', N'127.0.0.1', DATEADD(DAY, -5, GETDATE()));

PRINT N'Đã tạo dữ liệu mẫu cho NhatKy_HoatDong';

-- Kiểm tra kết quả
SELECT 
    n.MaNhatKy,
    nd.TenDangNhap,
    nd.HoTen,
    n.HanhDong,
    n.TenBang,
    n.MaBanGhi,
    n.DiaChiIP,
    n.ThoiGian
FROM NhatKy_HoatDong n
LEFT JOIN NguoiDung nd ON n.MaNguoiDung = nd.MaNguoiDung
ORDER BY n.ThoiGian DESC;

GO
