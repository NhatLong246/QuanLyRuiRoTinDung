using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuanLyRuiRoTinDung.Models.Entities;

[Table("VaiTro_Quyen")]
public partial class VaiTroQuyen
{
    [Key]
    public int MaVaiTroQuyen { get; set; }

    public int MaVaiTro { get; set; }

    public int MaQuyen { get; set; }

    public bool? DuocXem { get; set; }

    public bool? DuocThem { get; set; }

    public bool? DuocSua { get; set; }

    public bool? DuocXoa { get; set; }

    public bool? DuocPheDuyet { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NgayTao { get; set; }

    [ForeignKey("MaQuyen")]
    [InverseProperty("VaiTroQuyens")]
    public virtual Quyen MaQuyenNavigation { get; set; } = null!;

    [ForeignKey("MaVaiTro")]
    [InverseProperty("VaiTroQuyens")]
    public virtual VaiTro MaVaiTroNavigation { get; set; } = null!;
}
