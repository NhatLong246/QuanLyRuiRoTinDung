using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyRuiRoTinDung.Models.Entities;

[Table("GiaTriTaiSan_ThamChieu")]
public partial class GiaTriTaiSanThamChieu
{
    [Key]
    public int MaThamChieu { get; set; }

    [StringLength(50)]
    public string LoaiTaiSan { get; set; } = null!;

    [StringLength(200)]
    public string TenMucThamChieu { get; set; } = null!;

    [StringLength(1000)]
    public string? ThongTinChiTiet { get; set; }

    // Thông tin địa lý (cho đất)
    [StringLength(100)]
    public string? ThanhPho { get; set; }

    [StringLength(100)]
    public string? Quan { get; set; }

    [StringLength(100)]
    public string? Phuong { get; set; }

    [StringLength(100)]
    public string? KhuVuc { get; set; }

    // Thông tin xe
    [StringLength(100)]
    public string? HangXe { get; set; }

    [StringLength(100)]
    public string? DongXe { get; set; }

    public int? NamSanXuat { get; set; }

    // Giá trị
    [Column(TypeName = "decimal(18, 2)")]
    public decimal GiaTriThamChieu { get; set; }

    [StringLength(50)]
    public string? DonVi { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? GiaTriToiThieu { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? GiaTriToiDa { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? TyLeThamDinh { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NgayCapNhat { get; set; }

    public int? NguoiCapNhat { get; set; }

    public bool? TrangThaiHoatDong { get; set; }

    [StringLength(500)]
    public string? GhiChu { get; set; }

    [ForeignKey("NguoiCapNhat")]
    [InverseProperty("GiaTriTaiSanThamChieus")]
    public virtual NguoiDung? NguoiCapNhatNavigation { get; set; }
}
