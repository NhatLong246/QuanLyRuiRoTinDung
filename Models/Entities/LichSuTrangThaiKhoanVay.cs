using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuanLyRuiRoTinDung.Models.Entities;

[Table("LichSu_TrangThaiKhoanVay")]
public partial class LichSuTrangThaiKhoanVay
{
    [Key]
    public int MaLichSu { get; set; }

    public int MaKhoanVay { get; set; }

    [StringLength(50)]
    public string? TrangThaiCu { get; set; }

    [StringLength(50)]
    public string TrangThaiMoi { get; set; } = null!;

    public int NguoiThayDoi { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NgayThayDoi { get; set; }

    [StringLength(500)]
    public string? NhanXet { get; set; }

    [ForeignKey("MaKhoanVay")]
    [InverseProperty("LichSuTrangThaiKhoanVays")]
    public virtual KhoanVay MaKhoanVayNavigation { get; set; } = null!;

    [ForeignKey("NguoiThayDoi")]
    [InverseProperty("LichSuTrangThaiKhoanVays")]
    public virtual NguoiDung NguoiThayDoiNavigation { get; set; } = null!;
}
