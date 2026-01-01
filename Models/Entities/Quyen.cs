using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuanLyRuiRoTinDung.Models.Entities;

[Table("Quyen")]
[Index("MaQuyenCode", Name = "UQ__Quyen__C8E6E22195FF7270", IsUnique = true)]
public partial class Quyen
{
    [Key]
    public int MaQuyen { get; set; }

    [StringLength(100)]
    public string TenQuyen { get; set; } = null!;

    [Column("MaQuyen_Code")]
    [StringLength(50)]
    public string? MaQuyenCode { get; set; }

    [StringLength(50)]
    public string? PhanHe { get; set; }

    [StringLength(200)]
    public string? MoTa { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NgayTao { get; set; }

    [InverseProperty("MaQuyenNavigation")]
    public virtual ICollection<VaiTroQuyen> VaiTroQuyens { get; set; } = new List<VaiTroQuyen>();
}
