-- Script cập nhật LoaiKhachHang trong bảng ThongTin_CIC
-- Chuyển từ "Cá nhân"/"Doanh nghiệp" sang "CaNhan"/"DoanhNghiep"

-- Kiểm tra dữ liệu hiện tại
SELECT MaCIC_Code, LoaiKhachHang, SoCMND_CCCD, MaSoThue, HoTen 
FROM ThongTin_CIC
WHERE TrangThaiHoatDong = 1;

-- Cập nhật "Cá nhân" -> "CaNhan"
UPDATE ThongTin_CIC 
SET LoaiKhachHang = 'CaNhan'
WHERE LoaiKhachHang = N'Cá nhân';

-- Cập nhật "Doanh nghiệp" -> "DoanhNghiep"
UPDATE ThongTin_CIC 
SET LoaiKhachHang = 'DoanhNghiep'
WHERE LoaiKhachHang = N'Doanh nghiệp';

-- Kiểm tra lại sau khi cập nhật
SELECT MaCIC_Code, LoaiKhachHang, SoCMND_CCCD, MaSoThue, HoTen 
FROM ThongTin_CIC
WHERE TrangThaiHoatDong = 1;
