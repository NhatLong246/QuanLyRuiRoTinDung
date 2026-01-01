using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuanLyRuiRoTinDung.Models.Entities;

[Table("NhatKy_HoatDong")]
public partial class NhatKyHoatDong
{
    [Key]
    public int MaNhatKy { get; set; }

    public int? MaNguoiDung { get; set; }

    [StringLength(100)]
    public string HanhDong { get; set; } = null!;

    [StringLength(100)]
    public string? TenBang { get; set; }

    public int? MaBanGhi { get; set; }

    public string? GiaTriCu { get; set; }

    public string? GiaTriMoi { get; set; }

    [Column("DiaChiIP")]
    [StringLength(50)]
    public string? DiaChiIp { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ThoiGian { get; set; }

    [ForeignKey("MaNguoiDung")]
    [InverseProperty("NhatKyHoatDongs")]
    public virtual NguoiDung? MaNguoiDungNavigation { get; set; }
}
