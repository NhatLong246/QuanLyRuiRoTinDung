using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuanLyRuiRoTinDung.Models.Entities;

[Table("DanhMuc_TinDung")]
public partial class DanhMucTinDung
{
    [Key]
    public int MaDanhMuc { get; set; }

    public DateOnly NgayDanhMuc { get; set; }

    public int? TongSoKhoanVay { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? TongSoTienVay { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? TongDuNo { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? TyLeNoXau { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? DiemRuiRoTrungBinh { get; set; }

    [Column("SoKhoanVay_RuiRoThap")]
    public int? SoKhoanVayRuiRoThap { get; set; }

    [Column("SoKhoanVay_RuiRoTrungBinh")]
    public int? SoKhoanVayRuiRoTrungBinh { get; set; }

    [Column("SoKhoanVay_RuiRoCao")]
    public int? SoKhoanVayRuiRoCao { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NgayTao { get; set; }

    public int? NguoiTao { get; set; }

    [ForeignKey("NguoiTao")]
    [InverseProperty("DanhMucTinDungs")]
    public virtual NguoiDung? NguoiTaoNavigation { get; set; }
}
