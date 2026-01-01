using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuanLyRuiRoTinDung.Models.Entities;

[Table("KhoanVay_TaiSan")]
public partial class KhoanVayTaiSan
{
    [Key]
    public int MaLienKet { get; set; }

    public int MaKhoanVay { get; set; }

    public int MaTaiSan { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? GiaTriDinhGiaTaiThoiDiemVay { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? TyLeTheChap { get; set; }

    public DateOnly? NgayTheChap { get; set; }

    public DateOnly? NgayGiaiChap { get; set; }

    [StringLength(50)]
    public string? TrangThai { get; set; }

    [StringLength(500)]
    public string? GhiChu { get; set; }

    [ForeignKey("MaKhoanVay")]
    [InverseProperty("KhoanVayTaiSans")]
    public virtual KhoanVay MaKhoanVayNavigation { get; set; } = null!;

    [ForeignKey("MaTaiSan")]
    [InverseProperty("KhoanVayTaiSans")]
    public virtual TaiSanDamBao MaTaiSanNavigation { get; set; } = null!;
}
