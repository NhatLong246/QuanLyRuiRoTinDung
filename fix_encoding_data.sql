-- =============================================
-- FIX LỖI ENCODING - XÓA DỮ LIỆU CŨ VÀ CHẠY LẠI
-- =============================================

USE QuanLyRuiRoTinDung
GO

-- Xóa dữ liệu cũ bị lỗi encoding
DELETE FROM GiaTriTaiSan_ThamChieu;

-- Reset identity về 1
DBCC CHECKIDENT ('GiaTriTaiSan_ThamChieu', RESEED, 0);

GO

-- Sau đó chạy lại file create_gia_tri_tai_san_tham_chieu.sql 
-- từ dòng INSERT để thêm dữ liệu mới với encoding đúng
