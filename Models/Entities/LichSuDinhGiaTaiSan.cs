using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuanLyRuiRoTinDung.Models.Entities;

[Table("LichSu_DinhGiaTaiSan")]
public partial class LichSuDinhGiaTaiSan
{
    [Key]
    public int MaLichSu { get; set; }

    public int MaTaiSan { get; set; }

    public DateOnly NgayDinhGia { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? GiaTriCu { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? GiaTriMoi { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? ChenhLech { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? TyLeThayDoi { get; set; }

    [StringLength(100)]
    public string? DonViDinhGia { get; set; }

    [StringLength(100)]
    public string? NguoiDinhGia { get; set; }

    [StringLength(200)]
    public string? PhuongPhapDinhGia { get; set; }

    [StringLength(500)]
    public string? LyDoDinhGia { get; set; }

    [StringLength(500)]
    public string? FileDinhGia { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NgayTao { get; set; }

    public int? NguoiTao { get; set; }

    [ForeignKey("MaTaiSan")]
    [InverseProperty("LichSuDinhGiaTaiSans")]
    public virtual TaiSanDamBao MaTaiSanNavigation { get; set; } = null!;

    [ForeignKey("NguoiTao")]
    [InverseProperty("LichSuDinhGiaTaiSans")]
    public virtual NguoiDung? NguoiTaoNavigation { get; set; }
}
