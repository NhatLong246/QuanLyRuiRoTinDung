-- =============================================
-- BẢNG LƯU FILE ĐÍNH KÈM HỒ SƠ VAY
-- =============================================
-- Bảng này lưu trữ các file đính kèm của hồ sơ vay
-- Mỗi hồ sơ vay có thể có nhiều file đính kèm
-- File được phân loại theo: Pháp lý, Tài chính, Tài sản đảm bảo

USE QuanLyRuiRoTinDung
GO

-- Bảng file đính kèm hồ sơ vay
CREATE TABLE HoSoVay_FileDinhKem (
    MaFile INT PRIMARY KEY IDENTITY(1,1),                        -- Mã định danh file
    MaKhoanVay INT NOT NULL,                                      -- Mã khoản vay (FK đến KhoanVay)
    LoaiFile NVARCHAR(50) NOT NULL,                              -- Loại file: PhapLy, TaiChinh, TaiSanDamBao
    TenFile NVARCHAR(255) NOT NULL,                               -- Tên file gốc khi upload
    TenFileLuu NVARCHAR(255) NOT NULL,                            -- Tên file đã lưu trên server (có thể hash)
    DuongDan NVARCHAR(500) NOT NULL,                             -- Đường dẫn đầy đủ đến file trên server
    KichThuoc BIGINT,                                             -- Kích thước file (bytes)
    DinhDang NVARCHAR(10),                                        -- Định dạng file: PDF, JPG, PNG, DOCX
    MoTa NVARCHAR(500),                                           -- Mô tả file (tùy chọn)
    NgayTao DATETIME DEFAULT GETDATE(),                          -- Ngày upload file
    NguoiTao INT,                                                -- Người upload file (FK đến NguoiDung)
    TrangThai BIT DEFAULT 1,                                     -- 1: Đang sử dụng, 0: Đã xóa
    
    FOREIGN KEY (MaKhoanVay) REFERENCES KhoanVay(MaKhoanVay) ON DELETE CASCADE,
    FOREIGN KEY (NguoiTao) REFERENCES NguoiDung(MaNguoiDung),
    
    -- Constraint: Loại file phải là một trong các giá trị hợp lệ
    CONSTRAINT CK_HoSoVay_FileDinhKem_LoaiFile CHECK (LoaiFile IN ('PhapLy', 'TaiChinh', 'TaiSanDamBao'))
);

-- Tạo index để tìm kiếm nhanh theo khoản vay
CREATE INDEX IX_HoSoVay_FileDinhKem_MaKhoanVay ON HoSoVay_FileDinhKem(MaKhoanVay);
CREATE INDEX IX_HoSoVay_FileDinhKem_LoaiFile ON HoSoVay_FileDinhKem(LoaiFile);

GO
