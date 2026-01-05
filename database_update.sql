-- =============================================
-- HỆ THỐNG QUẢN LÝ RỦI RO TÍN DỤNG
-- SQL Server Database Schema - Tiếng Việt có chú thích
-- =============================================

CREATE DATABASE QuanLyRuiRoTinDung
GO

USE QuanLyRuiRoTinDung
GO

-- 1. BẢNG QUẢN TRỊ HỆ THỐNG
-- =============================================

-- Bảng vai trò người dùng
CREATE TABLE VaiTro (
    MaVaiTro INT PRIMARY KEY IDENTITY(1,1),                    -- Mã định danh duy nhất của vai trò
    TenVaiTro NVARCHAR(50) NOT NULL UNIQUE,                    -- Tên vai trò: Admin, NhanVienTinDung, QuanLyRuiRo, LanhDao
    MoTa NVARCHAR(200),                                        -- Mô tả chi tiết về vai trò và trách nhiệm
    NgayTao DATETIME DEFAULT GETDATE(),                        -- Ngày tạo vai trò trong hệ thống
    TrangThaiHoatDong BIT DEFAULT 1                            -- 1: Đang hoạt động, 0: Tạm khóa
);

-- Bảng người dùng
CREATE TABLE NguoiDung (
    MaNguoiDung INT PRIMARY KEY IDENTITY(1,1),                 -- Mã định danh duy nhất của người dùng
    TenDangNhap NVARCHAR(50) NOT NULL UNIQUE,                  -- Tên đăng nhập vào hệ thống (không trùng)
    MatKhauHash NVARCHAR(255) NOT NULL,                        -- Mật khẩu đã mã hóa (dùng BCrypt hoặc SHA-256)
    HoTen NVARCHAR(100) NOT NULL,                              -- Họ và tên đầy đủ của người dùng
    Email NVARCHAR(100),                                       -- Địa chỉ email liên hệ
    SoDienThoai NVARCHAR(20),                                  -- Số điện thoại liên lạc
    MaVaiTro INT NOT NULL,                                     -- Vai trò của người dùng (FK đến bảng VaiTro)
    MaPhongBan INT,                                            -- Phòng ban mà người dùng thuộc về
    TrangThaiHoatDong BIT DEFAULT 1,                           -- 1: Đang hoạt động, 0: Đã khóa tài khoản
    LanDangNhapCuoi DATETIME,                                  -- Thời điểm đăng nhập gần nhất
    NgayTao DATETIME DEFAULT GETDATE(),                        -- Ngày tạo tài khoản
    NguoiTao INT,                                              -- Người tạo tài khoản này (thường là Admin)
    NgayCapNhat DATETIME,                                      -- Ngày cập nhật thông tin gần nhất
    NguoiCapNhat INT,                                          -- Người thực hiện cập nhật cuối cùng
    FOREIGN KEY (MaVaiTro) REFERENCES VaiTro(MaVaiTro)
);

-- Bảng phòng ban
CREATE TABLE PhongBan (
    MaPhongBan INT PRIMARY KEY IDENTITY(1,1),                  -- Mã định danh duy nhất của phòng ban
    TenPhongBan NVARCHAR(100) NOT NULL,                        -- Tên phòng ban: Phòng Tín dụng, Phòng Rủi ro, Ban Giám đốc
    MaPhongBan_Code NVARCHAR(20) UNIQUE,                       -- Mã code ngắn gọn: TD, RR, BGD
    MaTruongPhong INT,                                         -- Mã người dùng là trưởng phòng
    MoTa NVARCHAR(200),                                        -- Mô tả chức năng nhiệm vụ của phòng ban
    NgayTao DATETIME DEFAULT GETDATE(),                        -- Ngày thành lập phòng ban
    TrangThaiHoatDong BIT DEFAULT 1                            -- 1: Đang hoạt động, 0: Đã giải thể
);

-- Bảng quyền hạn
CREATE TABLE Quyen (
    MaQuyen INT PRIMARY KEY IDENTITY(1,1),                     -- Mã định danh duy nhất của quyền
    TenQuyen NVARCHAR(100) NOT NULL,                           -- Tên quyền: Quản lý khoản vay, Đánh giá rủi ro
    MaQuyen_Code NVARCHAR(50) UNIQUE,                          -- Mã code: LOAN_MANAGE, RISK_ASSESS
    PhanHe NVARCHAR(50),                                       -- Phân hệ thuộc về: TinDung, RuiRo, BaoCao, HeThong
    MoTa NVARCHAR(200),                                        -- Mô tả chi tiết quyền hạn
    NgayTao DATETIME DEFAULT GETDATE()                         -- Ngày tạo quyền trong hệ thống
);

-- Bảng phân quyền theo vai trò
CREATE TABLE VaiTro_Quyen (
    MaVaiTroQuyen INT PRIMARY KEY IDENTITY(1,1),               -- Mã định danh duy nhất của phân quyền
    MaVaiTro INT NOT NULL,                                     -- Vai trò được phân quyền
    MaQuyen INT NOT NULL,                                      -- Quyền được gán cho vai trò
    DuocXem BIT DEFAULT 0,                                     -- 1: Được xem dữ liệu, 0: Không được xem
    DuocThem BIT DEFAULT 0,                                    -- 1: Được thêm mới, 0: Không được thêm
    DuocSua BIT DEFAULT 0,                                     -- 1: Được chỉnh sửa, 0: Không được sửa
    DuocXoa BIT DEFAULT 0,                                     -- 1: Được xóa, 0: Không được xóa
    DuocPheDuyet BIT DEFAULT 0,                                -- 1: Được phê duyệt, 0: Không được phê duyệt
    NgayTao DATETIME DEFAULT GETDATE(),                        -- Ngày phân quyền
    FOREIGN KEY (MaVaiTro) REFERENCES VaiTro(MaVaiTro),
    FOREIGN KEY (MaQuyen) REFERENCES Quyen(MaQuyen)
);

-- 2. BẢNG KHÁCH HÀNG
-- =============================================

-- Bảng loại khách hàng
CREATE TABLE LoaiKhachHang (
    MaLoaiKH INT PRIMARY KEY IDENTITY(1,1),                    -- Mã định danh loại khách hàng
    TenLoai NVARCHAR(50) NOT NULL,                             -- Tên loại: Cá nhân, Doanh nghiệp
    MaLoai NVARCHAR(20) UNIQUE,                                -- Mã code: CN, DN
    MoTa NVARCHAR(200)                                         -- Mô tả đặc điểm của loại khách hàng
);

-- Bảng khách hàng cá nhân
CREATE TABLE KhachHang_CaNhan (
    MaKhachHang INT PRIMARY KEY IDENTITY(1,1),                 -- Mã định danh duy nhất của khách hàng
    MaKhachHang_Code NVARCHAR(20) UNIQUE NOT NULL,             -- Mã khách hàng hiển thị: CN0001, CN0002
    HoTen NVARCHAR(100) NOT NULL,                              -- Họ và tên đầy đủ
    NgaySinh DATE,                                             -- Ngày tháng năm sinh
    GioiTinh NVARCHAR(10),                                     -- Nam, Nữ, Khác
    SoCMND NVARCHAR(20) UNIQUE,                                -- Số CMND/CCCD (không trùng)
    NgayCapCMND DATE,                                          -- Ngày cấp CMND/CCCD
    NoiCapCMND NVARCHAR(100),                                  -- Nơi cấp CMND/CCCD
    SoDienThoai NVARCHAR(20),                                  -- Số điện thoại liên lạc
    Email NVARCHAR(100),                                       -- Địa chỉ email
    DiaChi NVARCHAR(200),                                      -- Địa chỉ thường trú đầy đủ
    ThanhPho NVARCHAR(50),                                     -- Thành phố/Tỉnh
    Quan NVARCHAR(50),                                         -- Quận/Huyện
    Phuong NVARCHAR(50),                                       -- Phường/Xã
    TinhTrangHonNhan NVARCHAR(20),                             -- Độc thân, Đã kết hôn, Ly hôn, Góa
    NgheNghiep NVARCHAR(100),                                  -- Nghề nghiệp hiện tại
    ThuNhapHangThang DECIMAL(18,2),                            -- Thu nhập trung bình hàng tháng (VNĐ)
    TenCongTy NVARCHAR(100),                                   -- Tên công ty đang làm việc
    SoNamLamViec INT,                                          -- Số năm làm việc tại công ty hiện tại
    DiemTinDung INT,                                           -- Điểm tín dụng (0-1000)
    XepHangTinDung NVARCHAR(10),                               -- Xếp hạng: AAA, AA, A, BBB, BB, B, CCC, CC, C, D
    NgayTao DATETIME DEFAULT GETDATE(),                        -- Ngày tạo hồ sơ khách hàng
    NguoiTao INT,                                              -- Nhân viên tạo hồ sơ
    NgayCapNhat DATETIME,                                      -- Ngày cập nhật thông tin gần nhất
    NguoiCapNhat INT,                                          -- Người cập nhật thông tin
    TrangThaiHoatDong BIT DEFAULT 1,                           -- 1: Đang hoạt động, 0: Ngừng giao dịch
    FOREIGN KEY (NguoiTao) REFERENCES NguoiDung(MaNguoiDung)
);

-- Bảng khách hàng doanh nghiệp
CREATE TABLE KhachHang_DoanhNghiep (
    MaKhachHang INT PRIMARY KEY IDENTITY(1,1),                 -- Mã định danh duy nhất của doanh nghiệp
    MaKhachHang_Code NVARCHAR(20) UNIQUE NOT NULL,             -- Mã doanh nghiệp: DN0001, DN0002
    TenCongTy NVARCHAR(200) NOT NULL,                          -- Tên đầy đủ công ty
    MaSoThue NVARCHAR(20) UNIQUE NOT NULL,                     -- Mã số thuế (không trùng)
    SoGiayPhepKinhDoanh NVARCHAR(50),                          -- Số giấy phép đăng ký kinh doanh
    NgayCapGiayPhep DATE,                                      -- Ngày cấp giấy phép
    NgayDangKy DATE,                                           -- Ngày đăng ký thành lập công ty
    NguoiDaiDienPhapLuat NVARCHAR(100),                        -- Họ tên người đại diện pháp luật
    SoDienThoai NVARCHAR(20),                                  -- Số điện thoại công ty
    Email NVARCHAR(100),                                       -- Email công ty
    DiaChi NVARCHAR(200),                                      -- Địa chỉ trụ sở chính
    ThanhPho NVARCHAR(50),                                     -- Thành phố/Tỉnh
    Quan NVARCHAR(50),                                         -- Quận/Huyện
    LinhVucKinhDoanh NVARCHAR(100),                            -- Lĩnh vực kinh doanh chính
    SoLuongNhanVien INT,                                       -- Tổng số lao động
    DoanhThuHangNam DECIMAL(18,2),                             -- Doanh thu năm gần nhất (VNĐ)
    TongTaiSan DECIMAL(18,2),                                  -- Tổng tài sản theo báo cáo tài chính (VNĐ)
    VonDieuLe DECIMAL(18,2),                                   -- Vốn điều lệ đã đăng ký (VNĐ)
    DiemTinDung INT,                                           -- Điểm tín dụng doanh nghiệp (0-1000)
    XepHangTinDung NVARCHAR(10),                               -- Xếp hạng tín nhiệm: AAA -> D
    NgayTao DATETIME DEFAULT GETDATE(),                        -- Ngày tạo hồ sơ doanh nghiệp
    NguoiTao INT,                                              -- Nhân viên tạo hồ sơ
    NgayCapNhat DATETIME,                                      -- Ngày cập nhật thông tin gần nhất
    NguoiCapNhat INT,                                          -- Người cập nhật thông tin
    TrangThaiHoatDong BIT DEFAULT 1,                           -- 1: Đang hoạt động, 0: Ngừng giao dịch
    FOREIGN KEY (NguoiTao) REFERENCES NguoiDung(MaNguoiDung)
);

-- 3. BẢNG TÀI SẢN ĐẢM BẢO
-- =============================================

-- Bảng loại tài sản đảm bảo
CREATE TABLE LoaiTaiSanDamBao (
    MaLoaiTaiSan INT PRIMARY KEY IDENTITY(1,1),
    TenLoaiTaiSan NVARCHAR(100) NOT NULL,                      -- Bất động sản, Xe cộ, Máy móc, Giấy tờ có giá
    MaLoaiTaiSan_Code NVARCHAR(20) UNIQUE,                     -- BDS, XE, MM, GTCG
    TyLeChoVayToiDa DECIMAL(5,2),                              -- % giá trị tài sản được cho vay (VD: BĐS 70%, xe 50%)
    ThoiGianDinhGiaLai INT,                                    -- Số tháng phải định giá lại
    MoTa NVARCHAR(200)
);

-- Bảng tài sản đảm bảo
CREATE TABLE TaiSanDamBao (
    MaTaiSan INT PRIMARY KEY IDENTITY(1,1),
    MaTaiSan_Code NVARCHAR(20) UNIQUE NOT NULL,                -- TS0001, TS0002
    MaLoaiTaiSan INT NOT NULL,
    TenGoi NVARCHAR(200) NOT NULL,                             -- Căn hộ Vinhomes, Xe Honda City
    MoTaChiTiet NVARCHAR(1000),                                -- Mô tả đầy đủ
    
    -- Thông tin pháp lý
    SoGiayTo NVARCHAR(100),                                    -- Số sổ đỏ, biển số xe
    NgayCap DATE,
    NoiCap NVARCHAR(100),
    ChuSoHuu NVARCHAR(200),                                    -- Tên chủ sở hữu theo giấy tờ
    
    -- Vị trí (dành cho BĐS)
    DiaChi NVARCHAR(200),
    ThanhPho NVARCHAR(50),
    Quan NVARCHAR(50),
    
    -- Thông tin kỹ thuật
    DienTich DECIMAL(10,2),                                    -- m2 hoặc thông số khác
    NamSanXuat INT,                                            -- Năm xây/sản xuất
    TinhTrang NVARCHAR(50),                                    -- Mới, Cũ, Đã qua sử dụng
    
    -- Giá trị hiện tại
    GiaTriThiTruong DECIMAL(18,2),                             -- Giá trị thị trường hiện tại
    GiaTriDinhGia DECIMAL(18,2),                               -- Giá trị định giá chính thức
    NgayDinhGia DATE,                                          -- Ngày định giá gần nhất
    DonViDinhGia NVARCHAR(100),                                -- Tên công ty thẩm định
    
    TrangThaiSuDung NVARCHAR(50) DEFAULT N'Chưa thế chấp',     -- Chưa thế chấp, Đang thế chấp, Đã giải chấp
    
    NgayTao DATETIME DEFAULT GETDATE(),
    NguoiTao INT,
    NgayCapNhat DATETIME,
    NguoiCapNhat INT,
    
    FOREIGN KEY (MaLoaiTaiSan) REFERENCES LoaiTaiSanDamBao(MaLoaiTaiSan),
    FOREIGN KEY (NguoiTao) REFERENCES NguoiDung(MaNguoiDung)
);

-- Bảng lịch sử định giá lại
CREATE TABLE LichSu_DinhGiaTaiSan (
    MaLichSu INT PRIMARY KEY IDENTITY(1,1),
    MaTaiSan INT NOT NULL,
    NgayDinhGia DATE NOT NULL,
    GiaTriCu DECIMAL(18,2),
    GiaTriMoi DECIMAL(18,2),
    ChenhLech DECIMAL(18,2),                                   -- GiaTriMoi - GiaTriCu
    TyLeThayDoi DECIMAL(5,2),                                  -- % thay đổi
    DonViDinhGia NVARCHAR(100),
    NguoiDinhGia NVARCHAR(100),
    PhuongPhapDinhGia NVARCHAR(200),                           -- So sánh, Thu nhập, Chi phí
    LyDoDinhGia NVARCHAR(500),                                 -- Định kỳ, Yêu cầu khách hàng, Cảnh báo giảm giá
    FileDinhGia NVARCHAR(500),                                 -- Link file báo cáo thẩm định
    
    NgayTao DATETIME DEFAULT GETDATE(),
    NguoiTao INT,
    
    FOREIGN KEY (MaTaiSan) REFERENCES TaiSanDamBao(MaTaiSan),
    FOREIGN KEY (NguoiTao) REFERENCES NguoiDung(MaNguoiDung)
);

-- 4. BẢNG KHOẢN VAY
-- =============================================

-- Bảng loại khoản vay
CREATE TABLE LoaiKhoanVay (
    MaLoaiVay INT PRIMARY KEY IDENTITY(1,1),                   -- Mã định danh loại khoản vay
    TenLoaiVay NVARCHAR(100) NOT NULL,                         -- Tên loại: Vay tiêu dùng, Vay mua nhà, Vay kinh doanh
    MaLoaiVay_Code NVARCHAR(20) UNIQUE,                        -- Mã code: CONSUME, HOME, BUSINESS
    MoTa NVARCHAR(200),                                        -- Mô tả điều kiện và đặc điểm
    SoTienVayToiDa DECIMAL(18,2),                              -- Hạn mức tối đa cho phép (VNĐ)
    KyHanVayToiDa INT,                                         -- Kỳ hạn tối đa (tháng)
    LaiSuatToiThieu DECIMAL(5,2),                              -- Lãi suất tối thiểu (%/năm)
    LaiSuatToiDa DECIMAL(5,2),                                 -- Lãi suất tối đa (%/năm)
    TrangThaiHoatDong BIT DEFAULT 1                            -- 1: Đang áp dụng, 0: Ngừng cho vay
);

-- Bảng khoản vay
CREATE TABLE KhoanVay (
    MaKhoanVay INT PRIMARY KEY IDENTITY(1,1),                  -- Mã định danh duy nhất khoản vay
    MaKhoanVay_Code NVARCHAR(20) UNIQUE NOT NULL,              -- Mã khoản vay hiển thị: LOAN0001, LOAN0002
    
    -- Thông tin khách hàng
    LoaiKhachHang NVARCHAR(20) NOT NULL,                       -- Loại khách hàng: CaNhan hoặc DoanhNghiep
    MaKhachHang INT NOT NULL,                                  -- Mã khách hàng vay (từ bảng KhachHang_CaNhan hoặc KhachHang_DoanhNghiep)
    
    -- Thông tin khoản vay
    MaLoaiVay INT NOT NULL,                                    -- Loại khoản vay (FK đến LoaiKhoanVay)
    SoTienVay DECIMAL(18,2) NOT NULL,                          -- Số tiền vay yêu cầu (VNĐ)
    LaiSuat DECIMAL(5,2) NOT NULL,                             -- Lãi suất áp dụng (%/năm)
    KyHanVay INT NOT NULL,                                     -- Kỳ hạn vay (tháng)
    HinhThucTraNo NVARCHAR(50),                                -- Trả góp đều, Trả gốc cuối kỳ, Trả gốc lãi cuối kỳ
    SoTienTraHangThang DECIMAL(18,2),                          -- Số tiền phải trả mỗi tháng (nếu trả góp đều)
    MucDichVay NVARCHAR(200),                                  -- Mục đích sử dụng tiền vay
    
    -- Tài sản đảm bảo (đã chuyển sang bảng riêng, chỉ giữ lại trường tổng hợp)
    CoTaiSanDamBao BIT DEFAULT 0,                              -- 1: Có tài sản, 0: Không có (vay tín chấp)
    
    -- Ngày tháng quan trọng
    NgayNopHoSo DATETIME DEFAULT GETDATE(),                    -- Ngày nộp hồ sơ vay
    NgayPheDuyet DATETIME,                                     -- Ngày phê duyệt khoản vay
    NgayGiaiNgan DATETIME,                                     -- Ngày giải ngân tiền cho khách hàng
    NgayBatDauTra DATE,                                        -- Ngày bắt đầu trả nợ đầu tiên
    NgayDaoHan DATE,                                           -- Ngày đáo hạn khoản vay
    
    -- Trạng thái và xử lý
    TrangThaiKhoanVay NVARCHAR(50) NOT NULL DEFAULT N'Đang xử lý',  -- Trạng thái: Đang xử lý, Chờ bổ sung, Đã duyệt, Từ chối, Đã giải ngân, Đang trả nợ, Đã thanh toán, Quá hạn
    MaNhanVienTinDung INT NOT NULL,                            -- Nhân viên tín dụng phụ trách (FK đến NguoiDung)
    
    -- Đánh giá rủi ro
    MucDoRuiRo NVARCHAR(20),                                   -- Mức độ rủi ro: Thấp, Trung bình, Cao, Rất cao
    DiemRuiRo DECIMAL(5,2),                                    -- Điểm đánh giá rủi ro (0-100)
    XepHangRuiRo NVARCHAR(10),                                 -- Xếp hạng rủi ro: AAA, AA, A, BBB, BB, B, CCC, CC, C, D
    
    -- Phê duyệt và từ chối
    NguoiPheDuyet INT,                                         -- Lãnh đạo phê duyệt khoản vay (FK đến NguoiDung)
    CapPheDuyet NVARCHAR(50),                                  -- Cấp phê duyệt: Trưởng phòng, Giám đốc, Hội đồng tín dụng
    LyDoTuChoi NVARCHAR(500),                                  -- Lý do từ chối (nếu không được duyệt)
    
    -- Thông tin tài chính hiện tại
    SoDuGocConLai DECIMAL(18,2) DEFAULT 0,                     -- Số dư nợ gốc còn lại (VNĐ)
    SoDuLaiConLai DECIMAL(18,2) DEFAULT 0,                     -- Số dư lãi còn phải trả (VNĐ)
    TongDaThanhToan DECIMAL(18,2) DEFAULT 0,                   -- Tổng số tiền đã thanh toán (VNĐ)
    SoKyDaTra INT DEFAULT 0,                                   -- Số kỳ đã trả (tháng)
    SoKyConLai INT,                                            -- Số kỳ còn lại (tháng)
    
    -- Thông tin tính toán (cập nhật từ code C#)
    TongDuNo DECIMAL(18,2) DEFAULT 0,                          -- Tổng dư nợ (gốc + lãi) - tính trong C#
    TyLeHoanThanh DECIMAL(5,2) DEFAULT 0,                      -- Tỷ lệ hoàn thành (%) - tính trong C#
    
    -- Tình trạng nợ
    SoNgayQuaHan INT DEFAULT 0,                                -- Số ngày quá hạn thanh toán hiện tại
    MaPhanLoaiNo INT,                                          -- Nhóm nợ hiện tại (FK đến PhanLoaiNo): Nợ nhóm 1-5 theo NHNN
    NgayPhanLoaiNo DATE,                                       -- Ngày phân loại vào nhóm nợ gần nhất
    
    -- Ghi chú và tài liệu
    GhiChu NVARCHAR(1000),                                     -- Ghi chú bổ sung
    DuongDanHoSo NVARCHAR(500),                                -- Đường dẫn thư mục lưu hồ sơ vay
    
    -- Audit trail
    NgayTao DATETIME DEFAULT GETDATE(),                        -- Ngày tạo hồ sơ trong hệ thống
    NguoiTao INT,                                              -- Người tạo hồ sơ (FK đến NguoiDung)
    NgayCapNhat DATETIME,                                      -- Ngày cập nhật gần nhất
    NguoiCapNhat INT,                                          -- Người cập nhật (FK đến NguoiDung)
    
    -- Foreign Keys
    FOREIGN KEY (MaLoaiVay) REFERENCES LoaiKhoanVay(MaLoaiVay),
    FOREIGN KEY (MaNhanVienTinDung) REFERENCES NguoiDung(MaNguoiDung),
    FOREIGN KEY (NguoiPheDuyet) REFERENCES NguoiDung(MaNguoiDung),
    FOREIGN KEY (NguoiTao) REFERENCES NguoiDung(MaNguoiDung),
    FOREIGN KEY (NguoiCapNhat) REFERENCES NguoiDung(MaNguoiDung),
    
    -- Constraints
    CONSTRAINT CK_KhoanVay_SoTienVay CHECK (SoTienVay > 0),
    CONSTRAINT CK_KhoanVay_LaiSuat CHECK (LaiSuat >= 0 AND LaiSuat <= 100),
    CONSTRAINT CK_KhoanVay_KyHanVay CHECK (KyHanVay > 0),
    CONSTRAINT CK_KhoanVay_LoaiKhachHang CHECK (LoaiKhachHang IN ('CaNhan', 'DoanhNghiep')),
    CONSTRAINT CK_KhoanVay_DiemRuiRo CHECK (DiemRuiRo >= 0 AND DiemRuiRo <= 100),
    CONSTRAINT CK_KhoanVay_SoNgayQuaHan CHECK (SoNgayQuaHan >= 0),
    CONSTRAINT CK_KhoanVay_NgayGiaiNgan CHECK (NgayGiaiNgan IS NULL OR NgayPheDuyet IS NULL OR NgayGiaiNgan >= NgayPheDuyet),
    CONSTRAINT CK_KhoanVay_NgayDaoHan CHECK (NgayDaoHan IS NULL OR NgayGiaiNgan IS NULL OR NgayDaoHan > NgayGiaiNgan),
    CONSTRAINT CK_KhoanVay_SoDuGoc CHECK (SoDuGocConLai >= 0),
    CONSTRAINT CK_KhoanVay_SoDuLai CHECK (SoDuLaiConLai >= 0)
);

-- Bảng liên kết khoản vay - tài sản (quan hệ nhiều-nhiều)
CREATE TABLE KhoanVay_TaiSan (
    MaLienKet INT PRIMARY KEY IDENTITY(1,1),
    MaKhoanVay INT NOT NULL,
    MaTaiSan INT NOT NULL,
    GiaTriDinhGiaTaiThoiDiemVay DECIMAL(18,2),                 -- Giá trị tại thời điểm vay
    TyLeTheChap DECIMAL(5,2),                                  -- % giá trị tài sản này trong tổng đảm bảo
    NgayTheChap DATE,
    NgayGiaiChap DATE,
    TrangThai NVARCHAR(50),                                    -- Đang thế chấp, Đã giải chấp
    GhiChu NVARCHAR(500),
    
    FOREIGN KEY (MaKhoanVay) REFERENCES KhoanVay(MaKhoanVay),
    FOREIGN KEY (MaTaiSan) REFERENCES TaiSanDamBao(MaTaiSan)
);

-- Bảng lịch sử trả nợ
CREATE TABLE LichSu_TraNo (
    MaGiaoDich INT PRIMARY KEY IDENTITY(1,1),                  -- Mã định danh giao dịch trả nợ
    MaGiaoDich_Code NVARCHAR(20) UNIQUE NOT NULL,              -- Mã giao dịch: TN0001, TN0002
    MaKhoanVay INT NOT NULL,                                   -- Khoản vay được trả
    KyTraNo INT NOT NULL,                                      -- Kỳ trả nợ thứ mấy (1, 2, 3...)
    NgayTraDuKien DATE NOT NULL,                               -- Ngày phải trả theo lịch
    NgayTraThucTe DATETIME,                                    -- Ngày thực tế khách hàng trả
    
    -- Số tiền theo kế hoạch
    SoTienGocPhaiTra DECIMAL(18,2) NOT NULL,                   -- Số tiền gốc phải trả trong kỳ
    SoTienLaiPhaiTra DECIMAL(18,2) NOT NULL,                   -- Số tiền lãi phải trả trong kỳ
    TongPhaiTra DECIMAL(18,2) NOT NULL,                        -- Tổng tiền phải trả = Gốc + Lãi
    
    -- Số tiền thực tế
    SoTienGocDaTra DECIMAL(18,2) DEFAULT 0,                    -- Số tiền gốc đã trả
    SoTienLaiDaTra DECIMAL(18,2) DEFAULT 0,                    -- Số tiền lãi đã trả
    SoTienPhiTraCham DECIMAL(18,2) DEFAULT 0,                  -- Phí trả chậm (nếu có)
    TongDaTra DECIMAL(18,2) DEFAULT 0,                         -- Tổng tiền thực tế đã trả
    
    -- Số dư sau khi trả
    SoDuGocConLai DECIMAL(18,2),                               -- Số dư gốc còn lại sau khi trả kỳ này
    SoDuLaiConLai DECIMAL(18,2),                               -- Số dư lãi còn lại sau khi trả kỳ này
    
    -- Trạng thái
    TrangThai NVARCHAR(50) NOT NULL DEFAULT N'Chưa trả',       -- Chưa trả, Đã trả đủ, Trả thiếu, Trả trễ, Trả trước hạn
    SoNgayTraCham INT DEFAULT 0,                               -- Số ngày trả chậm so với ngày đến hạn
    
    -- Thông tin thanh toán
    HinhThucThanhToan NVARCHAR(50),                            -- Tiền mặt, Chuyển khoản, Séc, Thẻ
    MaGiaoDichNganHang NVARCHAR(100),                          -- Mã giao dịch ngân hàng (nếu chuyển khoản)
    NganHangThanhToan NVARCHAR(100),                           -- Tên ngân hàng
    
    -- Người xử lý
    NguoiThuTien INT,                                          -- Nhân viên thu tiền (FK đến NguoiDung)
    NguoiXacNhan INT,                                          -- Người xác nhận giao dịch
    
    GhiChu NVARCHAR(500),                                      -- Ghi chú về giao dịch
    NgayTao DATETIME DEFAULT GETDATE(),                        -- Ngày tạo bản ghi
    NguoiTao INT,                                              -- Người tạo bản ghi
    NgayCapNhat DATETIME,                                      -- Ngày cập nhật
    NguoiCapNhat INT,                                          -- Người cập nhật
    
    FOREIGN KEY (MaKhoanVay) REFERENCES KhoanVay(MaKhoanVay),
    FOREIGN KEY (NguoiThuTien) REFERENCES NguoiDung(MaNguoiDung),
    FOREIGN KEY (NguoiXacNhan) REFERENCES NguoiDung(MaNguoiDung),
    FOREIGN KEY (NguoiTao) REFERENCES NguoiDung(MaNguoiDung),
    
    -- Constraints
    CONSTRAINT CK_LichSuTraNo_SoTien CHECK (SoTienGocPhaiTra >= 0 AND SoTienLaiPhaiTra >= 0),
    CONSTRAINT CK_LichSuTraNo_NgayTra CHECK (NgayTraThucTe IS NULL OR NgayTraThucTe >= DATEADD(DAY, -30, NgayTraDuKien)) -- Cho phép trả trước tối đa 30 ngày
);

-- Bảng lịch sử trạng thái khoản vay
CREATE TABLE LichSu_TrangThaiKhoanVay (
    MaLichSu INT PRIMARY KEY IDENTITY(1,1),                    -- Mã định danh bản ghi lịch sử
    MaKhoanVay INT NOT NULL,                                   -- Khoản vay được thay đổi trạng thái
    TrangThaiCu NVARCHAR(50),                                  -- Trạng thái trước khi thay đổi
    TrangThaiMoi NVARCHAR(50) NOT NULL,                        -- Trạng thái sau khi thay đổi
    NguoiThayDoi INT NOT NULL,                                 -- Người thực hiện thay đổi trạng thái
    NgayThayDoi DATETIME DEFAULT GETDATE(),                    -- Thời điểm thay đổi
    NhanXet NVARCHAR(500),                                     -- Nhận xét hoặc lý do thay đổi
    FOREIGN KEY (MaKhoanVay) REFERENCES KhoanVay(MaKhoanVay),
    FOREIGN KEY (NguoiThayDoi) REFERENCES NguoiDung(MaNguoiDung)
);

-- 5. BẢNG ĐÁNH GIÁ RỦI RO
-- =============================================

-- Bảng tiêu chí đánh giá rủi ro
CREATE TABLE TieuChi_DanhGiaRuiRo (
    MaTieuChi INT PRIMARY KEY IDENTITY(1,1),                   -- Mã định danh tiêu chí
    TenTieuChi NVARCHAR(100) NOT NULL,                         -- Tên tiêu chí: Thu nhập, Tài sản đảm bảo, Lịch sử tín dụng
    MaTieuChi_Code NVARCHAR(50) UNIQUE,                        -- Mã code: INCOME, COLLATERAL, CREDIT_HISTORY
    LoaiKhachHang NVARCHAR(20),                                -- Áp dụng cho: CaNhan, DoanhNghiep, TatCa
    TrongSo DECIMAL(5,2),                                      -- Trọng số tiêu chí (tổng = 100%)
    DiemToiThieu INT DEFAULT 0,                                -- Điểm tối thiểu của thang đo
    DiemToiDa INT DEFAULT 100,                                 -- Điểm tối đa của thang đo
    MoTa NVARCHAR(500),                                        -- Mô tả cách đánh giá tiêu chí
    TrangThaiHoatDong BIT DEFAULT 1,                           -- 1: Đang áp dụng, 0: Ngừng sử dụng
    NgayTao DATETIME DEFAULT GETDATE(),                        -- Ngày tạo tiêu chí
    NguoiTao INT,                                              -- Người tạo tiêu chí
    FOREIGN KEY (NguoiTao) REFERENCES NguoiDung(MaNguoiDung)
);

-- Bảng đánh giá rủi ro
CREATE TABLE DanhGia_RuiRo (
    MaDanhGia INT PRIMARY KEY IDENTITY(1,1),                   -- Mã định danh đánh giá
    MaKhoanVay INT NOT NULL,                                   -- Khoản vay được đánh giá
    NgayDanhGia DATETIME DEFAULT GETDATE(),                    -- Ngày thực hiện đánh giá
    NguoiDanhGia INT NOT NULL,                                 -- Người đánh giá rủi ro
    TongDiem DECIMAL(5,2),                                     -- Tổng điểm sau khi tính trọng số
    MucDoRuiRo NVARCHAR(20),                                   -- Mức độ rủi ro: Thấp, Trung bình, Cao
    XepHangRuiRo NVARCHAR(10),                                 -- Xếp hạng rủi ro: AAA -> D
    KienNghi NVARCHAR(50),                                     -- Kiến nghị: Chấp thuận, Từ chối, Cần xem xét thêm
    NhanXet NVARCHAR(1000),                                    -- Nhận xét chi tiết của người đánh giá
    NguoiPheDuyet INT,                                         -- Lãnh đạo phê duyệt kết quả đánh giá
    NgayPheDuyet DATETIME,                                     -- Ngày phê duyệt
    TrangThai NVARCHAR(50),                                    -- Trạng thái: Đang xử lý, Đã hoàn thành
    NgayTao DATETIME DEFAULT GETDATE(),                        -- Ngày tạo đánh giá
    FOREIGN KEY (MaKhoanVay) REFERENCES KhoanVay(MaKhoanVay),
    FOREIGN KEY (NguoiDanhGia) REFERENCES NguoiDung(MaNguoiDung),
    FOREIGN KEY (NguoiPheDuyet) REFERENCES NguoiDung(MaNguoiDung)
);

-- Bảng chi tiết đánh giá theo tiêu chí
CREATE TABLE ChiTiet_DanhGiaRuiRo (
    MaChiTiet INT PRIMARY KEY IDENTITY(1,1),                   -- Mã định danh chi tiết
    MaDanhGia INT NOT NULL,                                    -- Đánh giá tổng thể
    MaTieuChi INT NOT NULL,                                    -- Tiêu chí được đánh giá
    Diem DECIMAL(5,2) NOT NULL,                                -- Điểm thô của tiêu chí (trước khi nhân trọng số)
    DiemCoTrongSo DECIMAL(5,2),                                -- Điểm sau khi nhân trọng số
    NhanXet NVARCHAR(500),                                     -- Nhận xét về tiêu chí này
    FOREIGN KEY (MaDanhGia) REFERENCES DanhGia_RuiRo(MaDanhGia),
    FOREIGN KEY (MaTieuChi) REFERENCES TieuChi_DanhGiaRuiRo(MaTieuChi)
);

-- 6. BẢNG CẢNH BÁO VÀ THEO DÕI
-- =============================================

-- Bảng loại cảnh báo
CREATE TABLE LoaiCanhBao (
    MaLoaiCanhBao INT PRIMARY KEY IDENTITY(1,1),               -- Mã định danh loại cảnh báo
    TenLoaiCanhBao NVARCHAR(100) NOT NULL,                     -- Tên loại: Quá hạn thanh toán, Tài sản giảm giá, Thay đổi tín dụng
    MaCanhBao NVARCHAR(50) UNIQUE,                             -- Mã code: OVERDUE, COLLATERAL_DROP, CREDIT_CHANGE
    MucDoNghiemTrong NVARCHAR(20),                             -- Mức độ: Thấp, Trung bình, Cao, Khẩn cấp
    MoTa NVARCHAR(200),                                        -- Mô tả điều kiện kích hoạt cảnh báo
    TrangThaiHoatDong BIT DEFAULT 1                            -- 1: Đang kích hoạt, 0: Tạm tắt
);

-- Bảng cảnh báo
CREATE TABLE CanhBao (
    MaCanhBao INT PRIMARY KEY IDENTITY(1,1),                   -- Mã định danh cảnh báo
    MaLoaiCanhBao INT NOT NULL,                                -- Loại cảnh báo
    MaKhoanVay INT,                                            -- Khoản vay liên quan (nếu có)
    MaKhachHang INT,                                           -- Khách hàng liên quan (nếu có)
    LoaiKhachHang NVARCHAR(20),                                -- Loại khách hàng: CaNhan hoặc DoanhNghiep
    MucDoNghiemTrong NVARCHAR(20),                             -- Mức độ nghiêm trọng của cảnh báo
    TieuDe NVARCHAR(200) NOT NULL,                             -- Tiêu đề cảnh báo
    NoiDung NVARCHAR(1000),                                    -- Nội dung chi tiết cảnh báo
    NgayCanhBao DATETIME DEFAULT GETDATE(),                    -- Thời điểm phát sinh cảnh báo
    TrangThai NVARCHAR(50) DEFAULT N'Chưa xử lý',              -- Trạng thái: Chưa xử lý, Đang xử lý, Đã xử lý
    NguoiXuLy INT,                                             -- Người được giao xử lý cảnh báo
    NguoiGiaiQuyet INT,                                        -- Người thực sự giải quyết
    NgayGiaiQuyet DATETIME,                                    -- Ngày hoàn tất xử lý
    KetQuaXuLy NVARCHAR(1000),                                 -- Kết quả và biện pháp đã áp dụng
    FOREIGN KEY (MaLoaiCanhBao) REFERENCES LoaiCanhBao(MaLoaiCanhBao),
    FOREIGN KEY (MaKhoanVay) REFERENCES KhoanVay(MaKhoanVay),
    FOREIGN KEY (NguoiXuLy) REFERENCES NguoiDung(MaNguoiDung),
    FOREIGN KEY (NguoiGiaiQuyet) REFERENCES NguoiDung(MaNguoiDung)
);

-- 7. BẢNG BÁO CÁO VÀ THỐNG KÊ
-- =============================================

-- Bảng danh mục tín dụng
CREATE TABLE DanhMuc_TinDung (
    MaDanhMuc INT PRIMARY KEY IDENTITY(1,1),                   -- Mã định danh snapshot
    NgayDanhMuc DATE NOT NULL,                                 -- Ngày chụp dữ liệu danh mục
    TongSoKhoanVay INT,                                        -- Tổng số khoản vay đang hoạt động
    TongSoTienVay DECIMAL(18,2),                               -- Tổng giá trị giải ngân (VNĐ)
    TongDuNo DECIMAL(18,2),                                    -- Tổng dư nợ hiện tại (VNĐ)
    TyLeNoXau DECIMAL(5,2),                                    -- Tỷ lệ nợ xấu (%)
    DiemRuiRoTrungBinh DECIMAL(5,2),                           -- Điểm rủi ro trung bình toàn danh mục
    SoKhoanVay_RuiRoThap INT,                                  -- Số lượng khoản vay rủi ro thấp
    SoKhoanVay_RuiRoTrungBinh INT,                             -- Số lượng khoản vay rủi ro trung bình
    SoKhoanVay_RuiRoCao INT,                                   -- Số lượng khoản vay rủi ro cao
    NgayTao DATETIME DEFAULT GETDATE(),                        -- Ngày tạo báo cáo
    NguoiTao INT,                                              -- Người tạo báo cáo
    FOREIGN KEY (NguoiTao) REFERENCES NguoiDung(MaNguoiDung)
);

-- Bảng báo cáo rủi ro
CREATE TABLE BaoCao_RuiRo (
    MaBaoCao INT PRIMARY KEY IDENTITY(1,1),                    -- Mã định danh báo cáo
    MaBaoCao_Code NVARCHAR(50) UNIQUE,                         -- Mã báo cáo: BC202412-001
    LoaiBaoCao NVARCHAR(50),                                   -- Loại báo cáo: Hàng ngày, Hàng tuần, Hàng tháng, Đặc biệt
    KyBaoCao NVARCHAR(50),                                     -- Kỳ báo cáo: Tháng 12/2024, Quý 4/2024
    NgayBaoCao DATE NOT NULL,                                  -- Ngày lập báo cáo
    TongSoKhoanVayRaSoat INT,                                  -- Tổng số khoản vay được rà soát
    SoKhoanVayRuiRoCao INT,                                    -- Số khoản vay rủi ro cao trong kỳ
    GiaTriNoXau DECIMAL(18,2),                                 -- Tổng giá trị nợ xấu (VNĐ)
    KienNghiXuLy NVARCHAR(2000),                               -- Kiến nghị biện pháp xử lý
    NguoiLap INT NOT NULL,                                     -- Người lập báo cáo
    NgayLap DATETIME DEFAULT GETDATE(),                        -- Ngày lập
    NguoiPheDuyet INT,                                         -- Lãnh đạo phê duyệt báo cáo
    NgayPheDuyet DATETIME,                                     -- Ngày phê duyệt
    TrangThai NVARCHAR(50),                                    -- Trạng thái: Đang soạn, Chờ duyệt, Đã duyệt
    DuongDanFile NVARCHAR(500),                                -- Đường dẫn file báo cáo đính kèm
    FOREIGN KEY (NguoiLap) REFERENCES NguoiDung(MaNguoiDung),
    FOREIGN KEY (NguoiPheDuyet) REFERENCES NguoiDung(MaNguoiDung)
);

-- 8. BẢNG NỢ XẤU
-- =============================================

-- Bảng phân loại nợ
CREATE TABLE PhanLoaiNo (
    MaPhanLoai INT PRIMARY KEY IDENTITY(1,1),                  -- Mã định danh phân loại
    TenPhanLoai NVARCHAR(50) NOT NULL,                         -- Tên nhóm nợ theo NHNN: Nợ đủ tiêu chuẩn, Nợ cần chú ý, Nợ dưới tiêu chuẩn, Nợ nghi ngờ, Nợ có khả năng mất vốn
    MaPhanLoai_Code NVARCHAR(20) UNIQUE,                       -- Mã code: NHOM1, NHOM2, NHOM3, NHOM4, NHOM5
    SoNgayQuaHanToiThieu INT,                                  -- Số ngày quá hạn tối thiểu để vào nhóm
    SoNgayQuaHanToiDa INT,                                     -- Số ngày quá hạn tối đa của nhóm
    TyLeTriLapDuPhong DECIMAL(5,2),                            -- Tỷ lệ trích lập dự phòng rủi ro (%)
    MoTa NVARCHAR(200)                                         -- Mô tả đặc điểm nhóm nợ
);

-- Bảng theo dõi nợ xấu
CREATE TABLE TheoDoi_NoXau (
    MaNoXau INT PRIMARY KEY IDENTITY(1,1),                     -- Mã định danh theo dõi nợ xấu
    MaKhoanVay INT NOT NULL,                                   -- Khoản vay bị nợ xấu
    MaPhanLoai INT NOT NULL,                                   -- Nhóm nợ hiện tại
    NgayPhanLoai DATE NOT NULL,                                -- Ngày phân loại vào nhóm
    SoNgayQuaHan INT,                                          -- Số ngày quá hạn hiện tại
    SoDuNo DECIMAL(18,2),                                      -- Số dư nợ gốc + lãi (VNĐ)
    SoTienDuPhong DECIMAL(18,2),                               -- Số tiền dự phòng phải trích lập (VNĐ)
    BienPhapThuHoi NVARCHAR(1000),                             -- Biện pháp đã/đang áp dụng để thu hồi
    TrangThai NVARCHAR(50),                                    -- Trạng thái: Đang xử lý, Đã thu hồi một phần, Đã thu hồi toàn bộ, Đã xóa nợ
    NguoiXuLy INT,                                             -- Người phụ trách xử lý nợ xấu
    NgayTao DATETIME DEFAULT GETDATE(),                        -- Ngày tạo bản ghi theo dõi
    NguoiTao INT,                                              -- Người tạo bản ghi
    NgayCapNhat DATETIME,                                      -- Ngày cập nhật gần nhất
    NguoiCapNhat INT,                                          -- Người cập nhật
    FOREIGN KEY (MaKhoanVay) REFERENCES KhoanVay(MaKhoanVay),
    FOREIGN KEY (MaPhanLoai) REFERENCES PhanLoaiNo(MaPhanLoai),
    FOREIGN KEY (NguoiXuLy) REFERENCES NguoiDung(MaNguoiDung),
    FOREIGN KEY (NguoiTao) REFERENCES NguoiDung(MaNguoiDung)
);

-- 9. BẢNG AUDIT LOG
-- =============================================

-- Bảng nhật ký hoạt động
CREATE TABLE NhatKy_HoatDong (
    MaNhatKy INT PRIMARY KEY IDENTITY(1,1),                    -- Mã định danh nhật ký
    MaNguoiDung INT,                                           -- Người thực hiện hành động
    HanhDong NVARCHAR(100) NOT NULL,                           -- Loại hành động: Thêm, Sửa, Xóa, Xem, Đăng nhập, Phê duyệt
    TenBang NVARCHAR(100),                                     -- Tên bảng bị tác động
    MaBanGhi INT,                                              -- ID của bản ghi bị tác động
    GiaTriCu NVARCHAR(MAX),                                    -- Giá trị trước khi thay đổi (JSON format)
    GiaTriMoi NVARCHAR(MAX),                                   -- Giá trị sau khi thay đổi (JSON format)
    DiaChiIP NVARCHAR(50),                                     -- Địa chỉ IP thực hiện hành động
    ThoiGian DATETIME DEFAULT GETDATE(),                       -- Thời gian thực hiện
    FOREIGN KEY (MaNguoiDung) REFERENCES NguoiDung(MaNguoiDung)
);

-- 10. BẢNG CẤU HÌNH HỆ THỐNG
-- =============================================

-- Bảng cấu hình
CREATE TABLE CauHinh_HeThong (
    MaCauHinh INT PRIMARY KEY IDENTITY(1,1),                   -- Mã định danh cấu hình
    KhoaCauHinh NVARCHAR(100) UNIQUE NOT NULL,                 -- Tên khóa cấu hình: MAX_LOAN_AMOUNT, INTEREST_RATE_DEFAULT
    GiaTriCauHinh NVARCHAR(500),                               -- Giá trị của cấu hình
    KieuDuLieu NVARCHAR(20),                                   -- Kiểu dữ liệu: String, Number, Boolean, JSON
    DanhMuc NVARCHAR(50),                                      -- Danh mục: TinDung, RuiRo, HeThong, BaoMat
    MoTa NVARCHAR(200),                                        -- Mô tả công dụng của cấu hình
    CoTheSua BIT DEFAULT 1,                                    -- 1: Được phép sửa, 0: Không được sửa (hệ thống)
    NgayCapNhat DATETIME,                                      -- Ngày cập nhật giá trị gần nhất
    NguoiCapNhat INT,                                          -- Người cập nhật
    FOREIGN KEY (NguoiCapNhat) REFERENCES NguoiDung(MaNguoiDung)
);

-- 11. INDEXES ĐỂ TỐI ƯU HIỆU SUẤT
-- =============================================

-- Indexes cho bảng NguoiDung
CREATE INDEX IX_NguoiDung_TenDangNhap ON NguoiDung(TenDangNhap);
CREATE INDEX IX_NguoiDung_MaVaiTro ON NguoiDung(MaVaiTro);
CREATE INDEX IX_NguoiDung_TrangThaiHoatDong ON NguoiDung(TrangThaiHoatDong);

-- Indexes cho bảng TaiSanDamBao
CREATE INDEX IX_TaiSan_TrangThaiSuDung ON TaiSanDamBao(TrangThaiSuDung);
CREATE INDEX IX_TaiSan_LoaiTaiSan ON TaiSanDamBao(MaLoaiTaiSan);
CREATE INDEX IX_LichSuDinhGia_MaTaiSan ON LichSu_DinhGiaTaiSan(MaTaiSan);
CREATE INDEX IX_LichSuDinhGia_NgayDinhGia ON LichSu_DinhGiaTaiSan(NgayDinhGia);

-- Indexes cho bảng KhoanVay
CREATE INDEX IX_KhoanVay_MaKhoanVay_Code ON KhoanVay(MaKhoanVay_Code);
CREATE INDEX IX_KhoanVay_LoaiKhachHang_MaKhachHang ON KhoanVay(LoaiKhachHang, MaKhachHang);
CREATE INDEX IX_KhoanVay_TrangThaiKhoanVay ON KhoanVay(TrangThaiKhoanVay);
CREATE INDEX IX_KhoanVay_MaNhanVienTinDung ON KhoanVay(MaNhanVienTinDung);
CREATE INDEX IX_KhoanVay_NgayNopHoSo ON KhoanVay(NgayNopHoSo);
CREATE INDEX IX_KhoanVay_MucDoRuiRo ON KhoanVay(MucDoRuiRo);
CREATE INDEX IX_KhoanVay_SoNgayQuaHan ON KhoanVay(SoNgayQuaHan) WHERE SoNgayQuaHan > 0;
CREATE INDEX IX_KhoanVay_MaPhanLoaiNo ON KhoanVay(MaPhanLoaiNo);
CREATE INDEX IX_KhoanVay_TrangThaiHoatDong ON KhoanVay(TrangThaiKhoanVay, NgayGiaiNgan) WHERE TrangThaiKhoanVay IN (N'Đã giải ngân', N'Đang trả nợ');
CREATE INDEX IX_KhoanVay_NhanVien_TrangThai ON KhoanVay(MaNhanVienTinDung, TrangThaiKhoanVay);
CREATE INDEX IX_KhoanVay_NgayDaoHan ON KhoanVay(NgayDaoHan) WHERE NgayDaoHan IS NOT NULL;

-- Indexes cho bảng KhoanVay_TaiSan
CREATE INDEX IX_KhoanVayTaiSan_MaKhoanVay ON KhoanVay_TaiSan(MaKhoanVay);
CREATE INDEX IX_KhoanVayTaiSan_TrangThai ON KhoanVay_TaiSan(TrangThai);

-- Indexes cho bảng DanhGia_RuiRo
CREATE INDEX IX_DanhGiaRuiRo_MaKhoanVay ON DanhGia_RuiRo(MaKhoanVay);
CREATE INDEX IX_DanhGiaRuiRo_NguoiDanhGia ON DanhGia_RuiRo(NguoiDanhGia);
CREATE INDEX IX_DanhGiaRuiRo_TrangThai ON DanhGia_RuiRo(TrangThai);

-- Indexes cho bảng CanhBao
CREATE INDEX IX_CanhBao_TrangThai ON CanhBao(TrangThai);
CREATE INDEX IX_CanhBao_NguoiXuLy ON CanhBao(NguoiXuLy);
CREATE INDEX IX_CanhBao_NgayCanhBao ON CanhBao(NgayCanhBao);

-- Indexes cho bảng TheoDoi_NoXau
CREATE INDEX IX_NoXau_MaKhoanVay ON TheoDoi_NoXau(MaKhoanVay);
CREATE INDEX IX_NoXau_TrangThai ON TheoDoi_NoXau(TrangThai);
CREATE INDEX IX_NoXau_NgayPhanLoai ON TheoDoi_NoXau(NgayPhanLoai);