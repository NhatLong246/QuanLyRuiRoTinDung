-- Kiểm tra dữ liệu khoản vay để debug
USE QuanLyRuiRoTinDung;
GO

-- 1. Kiểm tra tổng số khoản vay
SELECT 
    COUNT(*) as TongSoKhoanVay,
    TrangThaiKhoanVay,
    COUNT(CASE WHEN NgayNopHoSo IS NOT NULL THEN 1 END) as SoKhoanVayDaNopHoSo
FROM KhoanVay
GROUP BY TrangThaiKhoanVay;

-- 2. Kiểm tra các khoản vay đã nộp hồ sơ (điều kiện để hiển thị trong Thẩm định)
SELECT 
    kv.MaKhoanVay,
    kv.MaKhoanVay_Code,
    kv.TrangThaiKhoanVay,
    kv.NgayNopHoSo,
    kv.SoTienVay,
    kv.LoaiKhachHang,
    kv.MaKhachHang,
    kv.DiemRuiRo,
    kv.MucDoRuiRo,
    kv.XepHangRuiRo
FROM KhoanVay kv
WHERE kv.TrangThaiKhoanVay != N'Nháp' 
  AND kv.NgayNopHoSo IS NOT NULL
ORDER BY kv.NgayNopHoSo DESC;

-- 3. Kiểm tra thông tin đánh giá rủi ro
SELECT 
    kv.MaKhoanVay_Code,
    dg.MaDanhGia,
    dg.TrangThai as TrangThaiDanhGia,
    dg.NgayDanhGia,
    dg.TongDiem,
    dg.MucDoRuiRo,
    dg.XepHangRuiRo
FROM KhoanVay kv
LEFT JOIN DanhGia_RuiRo dg ON kv.MaKhoanVay = dg.MaKhoanVay
WHERE kv.TrangThaiKhoanVay != N'Nháp' 
  AND kv.NgayNopHoSo IS NOT NULL;

-- 4. Kiểm tra khách hàng cá nhân
SELECT 
    MaKhachHang,
    MaKhachHang_Code,
    HoTen
FROM KhachHang_CaNhan;

-- 5. Kiểm tra khách hàng doanh nghiệp
SELECT 
    MaKhachHang,
    MaKhachHang_Code,
    TenCongTy
FROM KhachHang_DoanhNghiep;

-- 6. Nếu không có dữ liệu, kiểm tra xem có khoản vay nào không
SELECT COUNT(*) as TongSoKhoanVay FROM KhoanVay;

-- 7. Kiểm tra chi tiết một khoản vay cụ thể (nếu có)
SELECT TOP 1
    kv.*,
    CASE 
        WHEN kv.LoaiKhachHang = 'CaNhan' THEN cn.HoTen
        WHEN kv.LoaiKhachHang = 'DoanhNghiep' THEN dn.TenCongTy
        ELSE NULL
    END as TenKhachHang
FROM KhoanVay kv
LEFT JOIN KhachHang_CaNhan cn ON kv.LoaiKhachHang = 'CaNhan' AND kv.MaKhachHang = cn.MaKhachHang
LEFT JOIN KhachHang_DoanhNghiep dn ON kv.LoaiKhachHang = 'DoanhNghiep' AND kv.MaKhachHang = dn.MaKhachHang;
