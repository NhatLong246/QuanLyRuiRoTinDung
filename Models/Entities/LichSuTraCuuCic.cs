using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuanLyRuiRoTinDung.Models.Entities;

[Table("LichSu_TraCuuCIC")]
[Index("MaCic", Name = "IX_LichSuTraCuuCIC_MaCIC")]
[Index("SoCmndCccd", Name = "IX_LichSuTraCuuCIC_SoCMND")]
[Index("NgayTraCuu", Name = "IX_LichSuTraCuuCIC_NgayTraCuu")]
[Index("NguoiTraCuu", Name = "IX_LichSuTraCuuCIC_NguoiTraCuu")]
public partial class LichSuTraCuuCic
{
    [Key]
    public int MaLichSu { get; set; }

    [Column("MaCIC")]
    public int? MaCic { get; set; }

    [StringLength(20)]
    public string LoaiKhachHang { get; set; } = null!;

    public int? MaKhachHang { get; set; }

    [Column("SoCMND_CCCD")]
    [StringLength(20)]
    public string SoCmndCccd { get; set; } = null!;

    [StringLength(20)]
    public string? MaSoThue { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NgayTraCuu { get; set; }

    public int NguoiTraCuu { get; set; }

    [StringLength(50)]
    public string? KetQua { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    public string? ThongTinTraVe { get; set; }

    public int? DiemTinDung { get; set; }

    [StringLength(10)]
    public string? XepHangTinDung { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? TongDuNo { get; set; }

    public int? SoKhoanVayDangVay { get; set; }

    public int? SoKhoanVayNoXau { get; set; }

    [StringLength(1000)]
    public string? GhiChu { get; set; }

    [ForeignKey("MaCic")]
    [InverseProperty("LichSuTraCuuCics")]
    public virtual ThongTinCic? MaCicNavigation { get; set; }

    [ForeignKey("NguoiTraCuu")]
    [InverseProperty("LichSuTraCuuCicNguoiTraCuuNavigations")]
    public virtual NguoiDung NguoiTraCuuNavigation { get; set; } = null!;
}
