using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuanLyRuiRoTinDung.Models.Entities;

[Table("LoaiKhachHang")]
[Index("MaLoai", Name = "UQ__LoaiKhac__730A5758F305EE5D", IsUnique = true)]
public partial class LoaiKhachHang
{
    [Key]
    [Column("MaLoaiKH")]
    public int MaLoaiKh { get; set; }

    [StringLength(50)]
    public string TenLoai { get; set; } = null!;

    [StringLength(20)]
    public string? MaLoai { get; set; }

    [StringLength(200)]
    public string? MoTa { get; set; }
}
