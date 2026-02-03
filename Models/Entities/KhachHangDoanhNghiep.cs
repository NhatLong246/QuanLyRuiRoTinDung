using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuanLyRuiRoTinDung.Models.Entities;

[Table("KhachHang_DoanhNghiep")]
[Index("MaKhachHangCode", Name = "UQ__KhachHan__06C01F5CB0CDDDAB", IsUnique = true)]
[Index("MaSoThue", Name = "UQ__KhachHan__1E811CB1CC1FB9B5", IsUnique = true)]
public partial class KhachHangDoanhNghiep
{
    [Key]
    public int MaKhachHang { get; set; }

    [Column("MaKhachHang_Code")]
    [StringLength(20)]
    public string MaKhachHangCode { get; set; } = null!;

    [StringLength(200)]
    public string TenCongTy { get; set; } = null!;

    [StringLength(20)]
    public string MaSoThue { get; set; } = null!;

    [StringLength(50)]
    public string? SoGiayPhepKinhDoanh { get; set; }

    public DateOnly? NgayCapGiayPhep { get; set; }

    public DateOnly? NgayDangKy { get; set; }

    [StringLength(100)]
    public string? NguoiDaiDienPhapLuat { get; set; }

    [Column("SoCCCD_NguoiDaiDienPhapLuat")]
    [StringLength(20)]
    public string? SoCccdNguoiDaiDienPhapLuat { get; set; }

    [StringLength(20)]
    public string? SoDienThoai { get; set; }

    [StringLength(100)]
    public string? Email { get; set; }

    [StringLength(200)]
    public string? DiaChi { get; set; }

    [StringLength(50)]
    public string? ThanhPho { get; set; }

    [StringLength(50)]
    public string? Quan { get; set; }

    [StringLength(50)]
    public string? Phuong { get; set; }

    [StringLength(100)]
    public string? LinhVucKinhDoanh { get; set; }

    [StringLength(500)]
    public string? AnhGiayPhepKinhDoanh { get; set; }

    [StringLength(500)]
    public string? AnhBaoCaoTaichinh { get; set; }

    [StringLength(500)]
    public string? AnhGiayToLienQuanKhac { get; set; }

    [Column("CCCDTruoc")]
    [StringLength(500)]
    public string? CccdTruoc { get; set; }

    [Column("CCCDSau")]
    [StringLength(500)]
    public string? CccdSau { get; set; }

    [StringLength(500)]
    public string? AnhNguoiDaiDien { get; set; }

    public DateOnly? NgaySinh { get; set; }

    [StringLength(10)]
    public string? GioiTinh { get; set; }

    public int? SoLuongNhanVien { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? DoanhThuHangNam { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? TongTaiSan { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? VonDieuLe { get; set; }

    public int? DiemTinDung { get; set; }

    [StringLength(10)]
    public string? XepHangTinDung { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NgayTao { get; set; }

    public int? NguoiTao { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NgayCapNhat { get; set; }

    public int? NguoiCapNhat { get; set; }

    public bool? TrangThaiHoatDong { get; set; }

    [ForeignKey("NguoiTao")]
    [InverseProperty("KhachHangDoanhNghieps")]
    public virtual NguoiDung? NguoiTaoNavigation { get; set; }
}
