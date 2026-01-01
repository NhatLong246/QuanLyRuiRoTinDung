using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuanLyRuiRoTinDung.Models.Entities;

[Table("KhachHang_CaNhan")]
[Index("MaKhachHangCode", Name = "UQ__KhachHan__06C01F5C4ADB131A", IsUnique = true)]
[Index("SoCmnd", Name = "UQ__KhachHan__F5EEA1C697BBA7C8", IsUnique = true)]
public partial class KhachHangCaNhan
{
    [Key]
    public int MaKhachHang { get; set; }

    [Column("MaKhachHang_Code")]
    [StringLength(20)]
    public string MaKhachHangCode { get; set; } = null!;

    [StringLength(100)]
    public string HoTen { get; set; } = null!;

    public DateOnly? NgaySinh { get; set; }

    [StringLength(10)]
    public string? GioiTinh { get; set; }

    [Column("SoCMND")]
    [StringLength(20)]
    public string? SoCmnd { get; set; }

    [Column("NgayCapCMND")]
    public DateOnly? NgayCapCmnd { get; set; }

    [Column("NoiCapCMND")]
    [StringLength(100)]
    public string? NoiCapCmnd { get; set; }

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

    [StringLength(20)]
    public string? TinhTrangHonNhan { get; set; }

    [StringLength(100)]
    public string? NgheNghiep { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? ThuNhapHangThang { get; set; }

    [StringLength(100)]
    public string? TenCongTy { get; set; }

    public int? SoNamLamViec { get; set; }

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
    [InverseProperty("KhachHangCaNhans")]
    public virtual NguoiDung? NguoiTaoNavigation { get; set; }
}
