using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuanLyRuiRoTinDung.Models.Entities;

[Table("ChiTiet_DanhGiaRuiRo")]
public partial class ChiTietDanhGiaRuiRo
{
    [Key]
    public int MaChiTiet { get; set; }

    public int MaDanhGia { get; set; }

    public int MaTieuChi { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal Diem { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? DiemCoTrongSo { get; set; }

    [StringLength(500)]
    public string? NhanXet { get; set; }

    [ForeignKey("MaDanhGia")]
    [InverseProperty("ChiTietDanhGiaRuiRos")]
    public virtual DanhGiaRuiRo MaDanhGiaNavigation { get; set; } = null!;

    [ForeignKey("MaTieuChi")]
    [InverseProperty("ChiTietDanhGiaRuiRos")]
    public virtual TieuChiDanhGiaRuiRo MaTieuChiNavigation { get; set; } = null!;
}
