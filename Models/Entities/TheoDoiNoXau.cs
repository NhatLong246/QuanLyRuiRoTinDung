using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuanLyRuiRoTinDung.Models.Entities;

[Table("TheoDoi_NoXau")]
public partial class TheoDoiNoXau
{
    [Key]
    public int MaNoXau { get; set; }

    public int MaKhoanVay { get; set; }

    public int MaPhanLoai { get; set; }

    public DateOnly NgayPhanLoai { get; set; }

    public int? SoNgayQuaHan { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? SoDuNo { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? SoTienDuPhong { get; set; }

    [StringLength(1000)]
    public string? BienPhapThuHoi { get; set; }

    [StringLength(50)]
    public string? TrangThai { get; set; }

    public int? NguoiXuLy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NgayTao { get; set; }

    public int? NguoiTao { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NgayCapNhat { get; set; }

    public int? NguoiCapNhat { get; set; }

    [ForeignKey("MaKhoanVay")]
    [InverseProperty("TheoDoiNoXaus")]
    public virtual KhoanVay MaKhoanVayNavigation { get; set; } = null!;

    [ForeignKey("MaPhanLoai")]
    [InverseProperty("TheoDoiNoXaus")]
    public virtual PhanLoaiNo MaPhanLoaiNavigation { get; set; } = null!;

    [ForeignKey("NguoiTao")]
    [InverseProperty("TheoDoiNoXauNguoiTaoNavigations")]
    public virtual NguoiDung? NguoiTaoNavigation { get; set; }

    [ForeignKey("NguoiXuLy")]
    [InverseProperty("TheoDoiNoXauNguoiXuLyNavigations")]
    public virtual NguoiDung? NguoiXuLyNavigation { get; set; }
}
