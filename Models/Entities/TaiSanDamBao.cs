using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuanLyRuiRoTinDung.Models.Entities;

[Table("TaiSanDamBao")]
[Index("MaTaiSanCode", Name = "UQ__TaiSanDa__2BFDC3C1A0A1585D", IsUnique = true)]
public partial class TaiSanDamBao
{
    [Key]
    public int MaTaiSan { get; set; }

    [Column("MaTaiSan_Code")]
    [StringLength(20)]
    public string MaTaiSanCode { get; set; } = null!;

    public int MaLoaiTaiSan { get; set; }

    [StringLength(200)]
    public string TenGoi { get; set; } = null!;

    [StringLength(1000)]
    public string? MoTaChiTiet { get; set; }

    [StringLength(100)]
    public string? SoGiayTo { get; set; }

    public DateOnly? NgayCap { get; set; }

    [StringLength(100)]
    public string? NoiCap { get; set; }

    [StringLength(200)]
    public string? ChuSoHuu { get; set; }

    [StringLength(200)]
    public string? DiaChi { get; set; }

    [StringLength(50)]
    public string? ThanhPho { get; set; }

    [StringLength(50)]
    public string? Quan { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? DienTich { get; set; }

    public int? NamSanXuat { get; set; }

    [StringLength(50)]
    public string? TinhTrang { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? GiaTriThiTruong { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? GiaTriDinhGia { get; set; }

    public DateOnly? NgayDinhGia { get; set; }

    [StringLength(100)]
    public string? DonViDinhGia { get; set; }

    [StringLength(50)]
    public string? TrangThaiSuDung { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NgayTao { get; set; }

    public int? NguoiTao { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NgayCapNhat { get; set; }

    public int? NguoiCapNhat { get; set; }

    [InverseProperty("MaTaiSanNavigation")]
    public virtual ICollection<KhoanVayTaiSan> KhoanVayTaiSans { get; set; } = new List<KhoanVayTaiSan>();

    [InverseProperty("MaTaiSanNavigation")]
    public virtual ICollection<LichSuDinhGiaTaiSan> LichSuDinhGiaTaiSans { get; set; } = new List<LichSuDinhGiaTaiSan>();

    [ForeignKey("MaLoaiTaiSan")]
    [InverseProperty("TaiSanDamBaos")]
    public virtual LoaiTaiSanDamBao MaLoaiTaiSanNavigation { get; set; } = null!;

    [ForeignKey("NguoiTao")]
    [InverseProperty("TaiSanDamBaos")]
    public virtual NguoiDung? NguoiTaoNavigation { get; set; }
}
