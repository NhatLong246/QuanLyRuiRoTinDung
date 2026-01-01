using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuanLyRuiRoTinDung.Models.Entities;

[Table("DanhGia_RuiRo")]
public partial class DanhGiaRuiRo
{
    [Key]
    public int MaDanhGia { get; set; }

    public int MaKhoanVay { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NgayDanhGia { get; set; }

    public int NguoiDanhGia { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? TongDiem { get; set; }

    [StringLength(20)]
    public string? MucDoRuiRo { get; set; }

    [StringLength(10)]
    public string? XepHangRuiRo { get; set; }

    [StringLength(50)]
    public string? KienNghi { get; set; }

    [StringLength(1000)]
    public string? NhanXet { get; set; }

    public int? NguoiPheDuyet { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NgayPheDuyet { get; set; }

    [StringLength(50)]
    public string? TrangThai { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NgayTao { get; set; }

    [InverseProperty("MaDanhGiaNavigation")]
    public virtual ICollection<ChiTietDanhGiaRuiRo> ChiTietDanhGiaRuiRos { get; set; } = new List<ChiTietDanhGiaRuiRo>();

    [ForeignKey("MaKhoanVay")]
    [InverseProperty("DanhGiaRuiRos")]
    public virtual KhoanVay MaKhoanVayNavigation { get; set; } = null!;

    [ForeignKey("NguoiDanhGia")]
    [InverseProperty("DanhGiaRuiRoNguoiDanhGiaNavigations")]
    public virtual NguoiDung NguoiDanhGiaNavigation { get; set; } = null!;

    [ForeignKey("NguoiPheDuyet")]
    [InverseProperty("DanhGiaRuiRoNguoiPheDuyetNavigations")]
    public virtual NguoiDung? NguoiPheDuyetNavigation { get; set; }
}
