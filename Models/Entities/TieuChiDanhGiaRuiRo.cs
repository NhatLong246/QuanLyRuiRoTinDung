using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuanLyRuiRoTinDung.Models.Entities;

[Table("TieuChi_DanhGiaRuiRo")]
[Index("MaTieuChiCode", Name = "UQ__TieuChi___9948028BBADA0A63", IsUnique = true)]
public partial class TieuChiDanhGiaRuiRo
{
    [Key]
    public int MaTieuChi { get; set; }

    [StringLength(100)]
    public string TenTieuChi { get; set; } = null!;

    [Column("MaTieuChi_Code")]
    [StringLength(50)]
    public string? MaTieuChiCode { get; set; }

    [StringLength(20)]
    public string? LoaiKhachHang { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? TrongSo { get; set; }

    public int? DiemToiThieu { get; set; }

    public int? DiemToiDa { get; set; }

    [StringLength(500)]
    public string? MoTa { get; set; }

    public bool? TrangThaiHoatDong { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NgayTao { get; set; }

    public int? NguoiTao { get; set; }

    [InverseProperty("MaTieuChiNavigation")]
    public virtual ICollection<ChiTietDanhGiaRuiRo> ChiTietDanhGiaRuiRos { get; set; } = new List<ChiTietDanhGiaRuiRo>();

    [ForeignKey("NguoiTao")]
    [InverseProperty("TieuChiDanhGiaRuiRos")]
    public virtual NguoiDung? NguoiTaoNavigation { get; set; }
}
