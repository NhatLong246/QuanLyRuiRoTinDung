using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuanLyRuiRoTinDung.Models.Entities;

[Table("PhongBan")]
[Index("MaPhongBanCode", Name = "UQ__PhongBan__64B49E40AB4A19D0", IsUnique = true)]
public partial class PhongBan
{
    [Key]
    public int MaPhongBan { get; set; }

    [StringLength(100)]
    public string TenPhongBan { get; set; } = null!;

    [Column("MaPhongBan_Code")]
    [StringLength(20)]
    public string? MaPhongBanCode { get; set; }

    public int? MaTruongPhong { get; set; }

    [StringLength(200)]
    public string? MoTa { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NgayTao { get; set; }

    public bool? TrangThaiHoatDong { get; set; }
}
