using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuanLyRuiRoTinDung.Models.Entities;

[Table("LoaiKhoanVay")]
[Index("MaLoaiVayCode", Name = "UQ__LoaiKhoa__95F88599379CCE3A", IsUnique = true)]
public partial class LoaiKhoanVay
{
    [Key]
    public int MaLoaiVay { get; set; }

    [StringLength(100)]
    public string TenLoaiVay { get; set; } = null!;

    [Column("MaLoaiVay_Code")]
    [StringLength(20)]
    public string? MaLoaiVayCode { get; set; }

    [StringLength(200)]
    public string? MoTa { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? SoTienVayToiDa { get; set; }

    public int? KyHanVayToiDa { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? LaiSuatToiThieu { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? LaiSuatToiDa { get; set; }

    public bool? TrangThaiHoatDong { get; set; }

    [InverseProperty("MaLoaiVayNavigation")]
    public virtual ICollection<KhoanVay> KhoanVays { get; set; } = new List<KhoanVay>();
}
