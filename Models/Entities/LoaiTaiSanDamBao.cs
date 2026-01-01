using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuanLyRuiRoTinDung.Models.Entities;

[Table("LoaiTaiSanDamBao")]
[Index("MaLoaiTaiSanCode", Name = "UQ__LoaiTaiS__F9C1501F1BFC81F6", IsUnique = true)]
public partial class LoaiTaiSanDamBao
{
    [Key]
    public int MaLoaiTaiSan { get; set; }

    [StringLength(100)]
    public string TenLoaiTaiSan { get; set; } = null!;

    [Column("MaLoaiTaiSan_Code")]
    [StringLength(20)]
    public string? MaLoaiTaiSanCode { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? TyLeChoVayToiDa { get; set; }

    public int? ThoiGianDinhGiaLai { get; set; }

    [StringLength(200)]
    public string? MoTa { get; set; }

    [InverseProperty("MaLoaiTaiSanNavigation")]
    public virtual ICollection<TaiSanDamBao> TaiSanDamBaos { get; set; } = new List<TaiSanDamBao>();
}
