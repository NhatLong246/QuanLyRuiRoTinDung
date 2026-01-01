using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuanLyRuiRoTinDung.Models.Entities;

[Table("LichSu_TraNo")]
[Index("MaGiaoDichCode", Name = "UQ__LichSu_T__77B683925A3A86D4", IsUnique = true)]
public partial class LichSuTraNo
{
    [Key]
    public int MaGiaoDich { get; set; }

    [Column("MaGiaoDich_Code")]
    [StringLength(20)]
    public string MaGiaoDichCode { get; set; } = null!;

    public int MaKhoanVay { get; set; }

    public int KyTraNo { get; set; }

    public DateOnly NgayTraDuKien { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NgayTraThucTe { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal SoTienGocPhaiTra { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal SoTienLaiPhaiTra { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal TongPhaiTra { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? SoTienGocDaTra { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? SoTienLaiDaTra { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? SoTienPhiTraCham { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? TongDaTra { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? SoDuGocConLai { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? SoDuLaiConLai { get; set; }

    [StringLength(50)]
    public string TrangThai { get; set; } = null!;

    public int? SoNgayTraCham { get; set; }

    [StringLength(50)]
    public string? HinhThucThanhToan { get; set; }

    [StringLength(100)]
    public string? MaGiaoDichNganHang { get; set; }

    [StringLength(100)]
    public string? NganHangThanhToan { get; set; }

    public int? NguoiThuTien { get; set; }

    public int? NguoiXacNhan { get; set; }

    [StringLength(500)]
    public string? GhiChu { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NgayTao { get; set; }

    public int? NguoiTao { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NgayCapNhat { get; set; }

    public int? NguoiCapNhat { get; set; }

    [ForeignKey("MaKhoanVay")]
    [InverseProperty("LichSuTraNos")]
    public virtual KhoanVay MaKhoanVayNavigation { get; set; } = null!;

    [ForeignKey("NguoiTao")]
    [InverseProperty("LichSuTraNoNguoiTaoNavigations")]
    public virtual NguoiDung? NguoiTaoNavigation { get; set; }

    [ForeignKey("NguoiThuTien")]
    [InverseProperty("LichSuTraNoNguoiThuTienNavigations")]
    public virtual NguoiDung? NguoiThuTienNavigation { get; set; }

    [ForeignKey("NguoiXacNhan")]
    [InverseProperty("LichSuTraNoNguoiXacNhanNavigations")]
    public virtual NguoiDung? NguoiXacNhanNavigation { get; set; }
}
