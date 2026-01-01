using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuanLyRuiRoTinDung.Models.Entities;

[Table("PhanLoaiNo")]
[Index("MaPhanLoaiCode", Name = "UQ__PhanLoai__A4BD0F120569C815", IsUnique = true)]
public partial class PhanLoaiNo
{
    [Key]
    public int MaPhanLoai { get; set; }

    [StringLength(50)]
    public string TenPhanLoai { get; set; } = null!;

    [Column("MaPhanLoai_Code")]
    [StringLength(20)]
    public string? MaPhanLoaiCode { get; set; }

    public int? SoNgayQuaHanToiThieu { get; set; }

    public int? SoNgayQuaHanToiDa { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? TyLeTriLapDuPhong { get; set; }

    [StringLength(200)]
    public string? MoTa { get; set; }

    [InverseProperty("MaPhanLoaiNavigation")]
    public virtual ICollection<TheoDoiNoXau> TheoDoiNoXaus { get; set; } = new List<TheoDoiNoXau>();
}
