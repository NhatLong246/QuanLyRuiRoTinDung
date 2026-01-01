using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuanLyRuiRoTinDung.Models.Entities;

[Table("BaoCao_RuiRo")]
[Index("MaBaoCaoCode", Name = "UQ__BaoCao_R__1EE922FCF94A0C13", IsUnique = true)]
public partial class BaoCaoRuiRo
{
    [Key]
    public int MaBaoCao { get; set; }

    [Column("MaBaoCao_Code")]
    [StringLength(50)]
    public string? MaBaoCaoCode { get; set; }

    [StringLength(50)]
    public string? LoaiBaoCao { get; set; }

    [StringLength(50)]
    public string? KyBaoCao { get; set; }

    public DateOnly NgayBaoCao { get; set; }

    public int? TongSoKhoanVayRaSoat { get; set; }

    public int? SoKhoanVayRuiRoCao { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? GiaTriNoXau { get; set; }

    [StringLength(2000)]
    public string? KienNghiXuLy { get; set; }

    public int NguoiLap { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NgayLap { get; set; }

    public int? NguoiPheDuyet { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NgayPheDuyet { get; set; }

    [StringLength(50)]
    public string? TrangThai { get; set; }

    [StringLength(500)]
    public string? DuongDanFile { get; set; }

    [ForeignKey("NguoiLap")]
    [InverseProperty("BaoCaoRuiRoNguoiLapNavigations")]
    public virtual NguoiDung NguoiLapNavigation { get; set; } = null!;

    [ForeignKey("NguoiPheDuyet")]
    [InverseProperty("BaoCaoRuiRoNguoiPheDuyetNavigations")]
    public virtual NguoiDung? NguoiPheDuyetNavigation { get; set; }
}
