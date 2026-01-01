using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuanLyRuiRoTinDung.Models.Entities;

[Table("CauHinh_HeThong")]
[Index("KhoaCauHinh", Name = "UQ__CauHinh___1D212521B6CA7DFC", IsUnique = true)]
public partial class CauHinhHeThong
{
    [Key]
    public int MaCauHinh { get; set; }

    [StringLength(100)]
    public string KhoaCauHinh { get; set; } = null!;

    [StringLength(500)]
    public string? GiaTriCauHinh { get; set; }

    [StringLength(20)]
    public string? KieuDuLieu { get; set; }

    [StringLength(50)]
    public string? DanhMuc { get; set; }

    [StringLength(200)]
    public string? MoTa { get; set; }

    public bool? CoTheSua { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NgayCapNhat { get; set; }

    public int? NguoiCapNhat { get; set; }

    [ForeignKey("NguoiCapNhat")]
    [InverseProperty("CauHinhHeThongs")]
    public virtual NguoiDung? NguoiCapNhatNavigation { get; set; }
}
