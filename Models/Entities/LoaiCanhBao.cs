using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuanLyRuiRoTinDung.Models.Entities;

[Table("LoaiCanhBao")]
[Index("MaCanhBao", Name = "UQ__LoaiCanh__73C23D92BBBA663F", IsUnique = true)]
public partial class LoaiCanhBao
{
    [Key]
    public int MaLoaiCanhBao { get; set; }

    [StringLength(100)]
    public string TenLoaiCanhBao { get; set; } = null!;

    [StringLength(50)]
    public string? MaCanhBao { get; set; }

    [StringLength(20)]
    public string? MucDoNghiemTrong { get; set; }

    [StringLength(200)]
    public string? MoTa { get; set; }

    public bool? TrangThaiHoatDong { get; set; }

    [InverseProperty("MaLoaiCanhBaoNavigation")]
    public virtual ICollection<CanhBao> CanhBaos { get; set; } = new List<CanhBao>();
}
