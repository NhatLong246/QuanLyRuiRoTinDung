-- =============================================
-- BẢNG GIÁ TRỊ TÀI SẢN THAM CHIẾU
-- Dùng để lưu giá trị tham chiếu của các loại tài sản
-- để nhân viên đánh giá rủi ro có thể so sánh và thẩm định
-- =============================================

USE QuanLyRuiRoTinDung
GO

-- Bảng giá trị tài sản tham chiếu
CREATE TABLE GiaTriTaiSan_ThamChieu (
    MaThamChieu INT PRIMARY KEY IDENTITY(1,1),
    LoaiTaiSan NVARCHAR(50) NOT NULL,                   -- 'Đất', 'Xe', 'Nhà', 'Vàng'
    TenMucThamChieu NVARCHAR(200) NOT NULL,             -- VD: 'Đất Quận 1', 'Honda City 2020'
    ThongTinChiTiet NVARCHAR(1000),                     -- Mô tả chi tiết
    
    -- Thông tin địa lý (cho đất)
    ThanhPho NVARCHAR(100),
    Quan NVARCHAR(100),
    Phuong NVARCHAR(100),
    KhuVuc NVARCHAR(100),                               -- Trung tâm, Ngoại ô, v.v.
    
    -- Thông tin xe
    HangXe NVARCHAR(100),                               -- Honda, Toyota, v.v.
    DongXe NVARCHAR(100),                               -- City, Vios, v.v.
    NamSanXuat INT,
    
    -- Giá trị
    GiaTriThamChieu DECIMAL(18, 2) NOT NULL,            -- Giá trị tham chiếu
    DonVi NVARCHAR(50),                                 -- 'VNĐ/m2', 'VNĐ/xe', v.v.
    GiaTriToiThieu DECIMAL(18, 2),                      -- Giá trị thấp nhất
    GiaTriToiDa DECIMAL(18, 2),                         -- Giá trị cao nhất
    
    TyLeThamDinh DECIMAL(5, 2) DEFAULT 70.00,           -- % giá trị có thể thẩm định (70%)
    
    NgayCapNhat DATETIME DEFAULT GETDATE(),
    NguoiCapNhat INT,
    TrangThaiHoatDong BIT DEFAULT 1,
    GhiChu NVARCHAR(500),
    
    FOREIGN KEY (NguoiCapNhat) REFERENCES NguoiDung(MaNguoiDung)
);

-- Index để tối ưu tìm kiếm
CREATE INDEX IX_GiaTriTaiSan_LoaiTaiSan ON GiaTriTaiSan_ThamChieu(LoaiTaiSan);
CREATE INDEX IX_GiaTriTaiSan_ThanhPho_Quan ON GiaTriTaiSan_ThamChieu(ThanhPho, Quan);
CREATE INDEX IX_GiaTriTaiSan_HangXe_DongXe ON GiaTriTaiSan_ThamChieu(HangXe, DongXe, NamSanXuat);

-- =============================================
-- INSERT DỮ LIỆU MẪU - GIÁ ĐẤT TP.HCM
-- =============================================

-- Giá đất các quận trung tâm TP.HCM (2026)
INSERT INTO GiaTriTaiSan_ThamChieu (LoaiTaiSan, TenMucThamChieu, ThongTinChiTiet, ThanhPho, Quan, KhuVuc, GiaTriThamChieu, DonVi, GiaTriToiThieu, GiaTriToiDa, TyLeThamDinh, TrangThaiHoatDong)
VALUES
-- Quận 1 - Trung tâm
(N'Đất', N'Đất Quận 1 - Trung tâm', N'Giá đất trung bình khu vực trung tâm Quận 1', N'TP. Hồ Chí Minh', N'Quận 1', N'Trung tâm', 500000000, N'VNĐ/m2', 400000000, 600000000, 70.00, 1),
(N'Đất', N'Đất Quận 1 - Ngoại vi', N'Giá đất khu vực ngoại vi Quận 1', N'TP. Hồ Chí Minh', N'Quận 1', N'Ngoại vi', 350000000, N'VNĐ/m2', 300000000, 450000000, 70.00, 1),

-- Quận 2 (Thủ Đức)
(N'Đất', N'Đất Quận 2 - Khu đô thị mới', N'Giá đất khu đô thị mới Quận 2', N'TP. Hồ Chí Minh', N'Quận 2', N'Đô thị mới', 120000000, N'VNĐ/m2', 100000000, 150000000, 75.00, 1),
(N'Đất', N'Đất Quận 2 - Gần sông', N'Giá đất ven sông Quận 2', N'TP. Hồ Chí Minh', N'Quận 2', N'Ven sông', 150000000, N'VNĐ/m2', 120000000, 180000000, 75.00, 1),

-- Quận 3
(N'Đất', N'Đất Quận 3 - Trung tâm', N'Giá đất trung tâm Quận 3', N'TP. Hồ Chí Minh', N'Quận 3', N'Trung tâm', 300000000, N'VNĐ/m2', 250000000, 400000000, 70.00, 1),

-- Quận 7
(N'Đất', N'Đất Quận 7 - Phú Mỹ Hưng', N'Giá đất khu Phú Mỹ Hưng', N'TP. Hồ Chí Minh', N'Quận 7', N'Khu đô thị', 180000000, N'VNĐ/m2', 150000000, 220000000, 75.00, 1),

-- Quận 9 (Thủ Đức)
(N'Đất', N'Đất Quận 9 - Trung tâm', N'Giá đất trung tâm Quận 9', N'TP. Hồ Chí Minh', N'Quận 9', N'Trung tâm', 80000000, N'VNĐ/m2', 70000000, 100000000, 75.00, 1),

-- Bình Thạnh
(N'Đất', N'Đất Bình Thạnh - Trung tâm', N'Giá đất trung tâm Bình Thạnh', N'TP. Hồ Chí Minh', N'Bình Thạnh', N'Trung tâm', 200000000, N'VNĐ/m2', 180000000, 250000000, 72.00, 1),

-- Tân Bình
(N'Đất', N'Đất Tân Bình - Gần sân bay', N'Giá đất gần sân bay Tân Sơn Nhất', N'TP. Hồ Chí Minh', N'Tân Bình', N'Gần sân bay', 180000000, N'VNĐ/m2', 150000000, 200000000, 72.00, 1),

-- Gò Vấp
(N'Đất', N'Đất Gò Vấp - Trung tâm', N'Giá đất trung tâm Gò Vấp', N'TP. Hồ Chí Minh', N'Gò Vấp', N'Trung tâm', 90000000, N'VNĐ/m2', 80000000, 110000000, 75.00, 1),

-- Hóc Môn
(N'Đất', N'Đất Hóc Môn', N'Giá đất Hóc Môn', N'TP. Hồ Chí Minh', N'Hóc Môn', N'Ngoại thành', 40000000, N'VNĐ/m2', 30000000, 50000000, 80.00, 1),

-- Củ Chi
(N'Đất', N'Đất Củ Chi', N'Giá đất Củ Chi', N'TP. Hồ Chí Minh', N'Củ Chi', N'Ngoại thành', 25000000, N'VNĐ/m2', 20000000, 35000000, 80.00, 1);

-- =============================================
-- INSERT DỮ LIỆU MẪU - GIÁ XE
-- =============================================

-- Xe Honda
INSERT INTO GiaTriTaiSan_ThamChieu (LoaiTaiSan, TenMucThamChieu, ThongTinChiTiet, HangXe, DongXe, NamSanXuat, GiaTriThamChieu, DonVi, GiaTriToiThieu, GiaTriToiDa, TyLeThamDinh, TrangThaiHoatDong)
VALUES
(N'Xe', N'Honda City 2024', N'Xe sedan 4 chỗ, mới 100%', N'Honda', N'City', 2024, 559000000, N'VNĐ/xe', 540000000, 580000000, 60.00, 1),
(N'Xe', N'Honda City 2023', N'Xe sedan 4 chỗ, đã qua sử dụng 1 năm', N'Honda', N'City', 2023, 480000000, N'VNĐ/xe', 460000000, 500000000, 55.00, 1),
(N'Xe', N'Honda Civic 2024', N'Xe sedan cao cấp, mới 100%', N'Honda', N'Civic', 2024, 789000000, N'VNĐ/xe', 770000000, 810000000, 60.00, 1),
(N'Xe', N'Honda CR-V 2024', N'Xe SUV 7 chỗ, mới 100%', N'Honda', N'CR-V', 2024, 1109000000, N'VNĐ/xe', 1080000000, 1150000000, 60.00, 1),
(N'Xe', N'Honda Accord 2024', N'Xe sedan hạng sang, mới 100%', N'Honda', N'Accord', 2024, 1319000000, N'VNĐ/xe', 1280000000, 1350000000, 60.00, 1),

-- Xe Toyota
(N'Xe', N'Toyota Vios 2024', N'Xe sedan 4 chỗ, mới 100%', N'Toyota', N'Vios', 2024, 478000000, N'VNĐ/xe', 460000000, 500000000, 60.00, 1),
(N'Xe', N'Toyota Vios 2023', N'Xe sedan 4 chỗ, đã qua sử dụng 1 năm', N'Toyota', N'Vios', 2023, 410000000, N'VNĐ/xe', 390000000, 430000000, 55.00, 1),
(N'Xe', N'Toyota Camry 2024', N'Xe sedan cao cấp, mới 100%', N'Toyota', N'Camry', 2024, 1220000000, N'VNĐ/xe', 1180000000, 1260000000, 60.00, 1),
(N'Xe', N'Toyota Fortuner 2024', N'Xe SUV 7 chỗ, mới 100%', N'Toyota', N'Fortuner', 2024, 1250000000, N'VNĐ/xe', 1200000000, 1300000000, 60.00, 1),
(N'Xe', N'Toyota Innova 2024', N'Xe MPV 7 chỗ, mới 100%', N'Toyota', N'Innova', 2024, 810000000, N'VNĐ/xe', 780000000, 850000000, 60.00, 1),

-- Xe Mazda
(N'Xe', N'Mazda 3 2024', N'Xe sedan 4 chỗ, mới 100%', N'Mazda', N'Mazda 3', 2024, 669000000, N'VNĐ/xe', 650000000, 690000000, 60.00, 1),
(N'Xe', N'Mazda CX-5 2024', N'Xe SUV 5 chỗ, mới 100%', N'Mazda', N'CX-5', 2024, 849000000, N'VNĐ/xe', 820000000, 880000000, 60.00, 1),
(N'Xe', N'Mazda CX-8 2024', N'Xe SUV 7 chỗ, mới 100%', N'Mazda', N'CX-8', 2024, 1149000000, N'VNĐ/xe', 1120000000, 1180000000, 60.00, 1),

-- Xe Hyundai
(N'Xe', N'Hyundai Accent 2024', N'Xe sedan 4 chỗ, mới 100%', N'Hyundai', N'Accent', 2024, 439000000, N'VNĐ/xe', 420000000, 460000000, 60.00, 1),
(N'Xe', N'Hyundai Elantra 2024', N'Xe sedan 4 chỗ cao cấp, mới 100%', N'Hyundai', N'Elantra', 2024, 659000000, N'VNĐ/xe', 640000000, 680000000, 60.00, 1),
(N'Xe', N'Hyundai Tucson 2024', N'Xe SUV 5 chỗ, mới 100%', N'Hyundai', N'Tucson', 2024, 799000000, N'VNĐ/xe', 770000000, 830000000, 60.00, 1),
(N'Xe', N'Hyundai Santa Fe 2024', N'Xe SUV 7 chỗ, mới 100%', N'Hyundai', N'Santa Fe', 2024, 1069000000, N'VNĐ/xe', 1040000000, 1100000000, 60.00, 1),

-- Xe Kia
(N'Xe', N'Kia Morning 2024', N'Xe hatchback 5 chỗ, mới 100%', N'Kia', N'Morning', 2024, 349000000, N'VNĐ/xe', 330000000, 370000000, 60.00, 1),
(N'Xe', N'Kia Seltos 2024', N'Xe SUV 5 chỗ, mới 100%', N'Kia', N'Seltos', 2024, 649000000, N'VNĐ/xe', 630000000, 670000000, 60.00, 1),
(N'Xe', N'Kia Sorento 2024', N'Xe SUV 7 chỗ, mới 100%', N'Kia', N'Sorento', 2024, 1099000000, N'VNĐ/xe', 1070000000, 1130000000, 60.00, 1);

-- =============================================
-- INSERT DỮ LIỆU MẪU - GIÁ TÀI SẢN KHÁC
-- =============================================

-- Vàng
INSERT INTO GiaTriTaiSan_ThamChieu (LoaiTaiSan, TenMucThamChieu, ThongTinChiTiet, GiaTriThamChieu, DonVi, GiaTriToiThieu, GiaTriToiDa, TyLeThamDinh, TrangThaiHoatDong)
VALUES
(N'Vàng', N'Vàng SJC 9999', N'Vàng miếng SJC 9999', 76500000, N'VNĐ/lượng', 75000000, 78000000, 85.00, 1),
(N'Vàng', N'Vàng PNJ 9999', N'Vàng miếng PNJ 9999', 76000000, N'VNĐ/lượng', 74500000, 77500000, 85.00, 1);

-- =============================================
-- SELECT DỮ LIỆU ĐỂ KIỂM TRA
-- =============================================

-- Xem giá đất các quận TP.HCM
SELECT 
    MaThamChieu,
    TenMucThamChieu,
    Quan,
    KhuVuc,
    FORMAT(GiaTriThamChieu, 'N0') as GiaTriThamChieu,
    DonVi,
    TyLeThamDinh
FROM GiaTriTaiSan_ThamChieu
WHERE LoaiTaiSan = N'Đất'
ORDER BY GiaTriThamChieu DESC;

-- Xem giá xe
SELECT 
    MaThamChieu,
    TenMucThamChieu,
    HangXe,
    DongXe,
    NamSanXuat,
    FORMAT(GiaTriThamChieu, 'N0') as GiaTriThamChieu,
    TyLeThamDinh
FROM GiaTriTaiSan_ThamChieu
WHERE LoaiTaiSan = N'Xe'
ORDER BY HangXe, GiaTriThamChieu DESC;

GO
