using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuanLyRuiRoTinDung.Models.Entities;

[Table("CanhBao")]
public partial class CanhBao
{
    [Key]
    public int MaCanhBao { get; set; }

    public int MaLoaiCanhBao { get; set; }

    public int? MaKhoanVay { get; set; }

    public int? MaKhachHang { get; set; }

    [StringLength(20)]
    public string? LoaiKhachHang { get; set; }

    [StringLength(20)]
    public string? MucDoNghiemTrong { get; set; }

    [StringLength(200)]
    public string TieuDe { get; set; } = null!;

    [StringLength(1000)]
    public string? NoiDung { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NgayCanhBao { get; set; }

    [StringLength(50)]
    public string? TrangThai { get; set; }

    public int? NguoiXuLy { get; set; }

    public int? NguoiGiaiQuyet { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NgayGiaiQuyet { get; set; }

    [StringLength(1000)]
    public string? KetQuaXuLy { get; set; }

    [ForeignKey("MaKhoanVay")]
    [InverseProperty("CanhBaos")]
    public virtual KhoanVay? MaKhoanVayNavigation { get; set; }

    [ForeignKey("MaLoaiCanhBao")]
    [InverseProperty("CanhBaos")]
    public virtual LoaiCanhBao MaLoaiCanhBaoNavigation { get; set; } = null!;

    [ForeignKey("NguoiGiaiQuyet")]
    [InverseProperty("CanhBaoNguoiGiaiQuyetNavigations")]
    public virtual NguoiDung? NguoiGiaiQuyetNavigation { get; set; }

    [ForeignKey("NguoiXuLy")]
    [InverseProperty("CanhBaoNguoiXuLyNavigations")]
    public virtual NguoiDung? NguoiXuLyNavigation { get; set; }
}
