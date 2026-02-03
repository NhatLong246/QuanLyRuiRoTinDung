using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuanLyRuiRoTinDung.Models.Entities;

[Table("ThongTin_CIC")]
[Index("MaCicCode", Name = "UQ__ThongTin__06C01F5C", IsUnique = true)]
[Index("LoaiKhachHang", "MaKhachHang", Name = "IX_CIC_LoaiKhachHang_MaKhachHang")]
[Index("SoCmndCccd", Name = "IX_CIC_SoCMND")]
[Index("MaSoThue", Name = "IX_CIC_MaSoThue")]
[Index("XepHangTinDungCic", Name = "IX_CIC_XepHangTinDung")]
[Index("MucDoRuiRo", Name = "IX_CIC_MucDoRuiRo")]
[Index("KhuyenNghiChoVay", Name = "IX_CIC_KhuyenNghiChoVay")]
[Index("NgayTraCuuCuoi", Name = "IX_CIC_NgayTraCuuCuoi")]
public partial class ThongTinCic
{
    [Key]
    public int MaCic { get; set; }

    [Column("MaCIC_Code")]
    [StringLength(20)]
    public string MaCicCode { get; set; } = null!;

    [StringLength(20)]
    public string LoaiKhachHang { get; set; } = null!;

    public int? MaKhachHang { get; set; }

    [Column("SoCMND_CCCD")]
    [StringLength(20)]
    public string SoCmndCccd { get; set; } = null!;

    [StringLength(20)]
    public string? MaSoThue { get; set; }

    [StringLength(100)]
    public string? HoTen { get; set; }

    [Column("TongSoKhoanVayCIC")]
    public int TongSoKhoanVayCic { get; set; }

    [Column("SoKhoanVayDangVayCIC")]
    public int SoKhoanVayDangVayCic { get; set; }

    [Column("SoKhoanVayDaTraXongCIC")]
    public int SoKhoanVayDaTraXongCic { get; set; }

    [Column("SoKhoanVayQuaHanCIC")]
    public int SoKhoanVayQuaHanCic { get; set; }

    [Column("SoKhoanVayNoXauCIC")]
    public int SoKhoanVayNoXauCic { get; set; }

    [Column("TongDuNoCIC", TypeName = "decimal(18, 2)")]
    public decimal TongDuNoCic { get; set; }

    [Column("DuNoQuaHanCIC", TypeName = "decimal(18, 2)")]
    public decimal DuNoQuaHanCic { get; set; }

    [Column("DuNoNoXauCIC", TypeName = "decimal(18, 2)")]
    public decimal DuNoNoXauCic { get; set; }

    [Column("DuNoToiDaCIC", TypeName = "decimal(18, 2)")]
    public decimal DuNoToiDaCic { get; set; }

    [Column("TongGiaTriVayCIC", TypeName = "decimal(18, 2)")]
    public decimal TongGiaTriVayCic { get; set; }

    public int? DiemTinDungCic { get; set; }

    [Column("XepHangTinDungCIC")]
    [StringLength(10)]
    public string? XepHangTinDungCic { get; set; }

    [StringLength(20)]
    public string? MucDoRuiRo { get; set; }

    [StringLength(20)]
    public string? KhaNangTraNo { get; set; }

    [Column("SoLanQuaHanCIC")]
    public int SoLanQuaHanCic { get; set; }

    [Column("SoLanNoXauCIC")]
    public int SoLanNoXauCic { get; set; }

    [Column("SoNgayQuaHanToiDaCIC")]
    public int SoNgayQuaHanToiDaCic { get; set; }

    [Column("NgayQuaHanLanCuoiCIC")]
    public DateOnly? NgayQuaHanLanCuoiCic { get; set; }

    [Column("NgayNoXauLanCuoiCIC")]
    public DateOnly? NgayNoXauLanCuoiCic { get; set; }

    [Column("ThoiGianTraNoTotCIC")]
    public int ThoiGianTraNoTotCic { get; set; }

    [Column("TyLeTraNoDungHanCIC", TypeName = "decimal(5, 2)")]
    public decimal TyLeTraNoDungHanCic { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    public string? DanhSachToChucTinDung { get; set; }

    public int SoToChucTinDungDaVay { get; set; }

    [StringLength(1000)]
    public string? DanhGiaTongQuat { get; set; }

    [StringLength(50)]
    public string? KhuyenNghiChoVay { get; set; }

    [StringLength(1000)]
    public string? LyDoKhuyenNghi { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NgayTraCuuCuoi { get; set; }

    public int? NguoiTraCuu { get; set; }

    [StringLength(50)]
    public string? KetQuaTraCuu { get; set; }

    [Column("ThongTinTraVeCIC", TypeName = "nvarchar(max)")]
    public string? ThongTinTraVeCic { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NgayTao { get; set; }

    public int? NguoiTao { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NgayCapNhat { get; set; }

    public int? NguoiCapNhat { get; set; }

    public bool? TrangThaiHoatDong { get; set; }

    [ForeignKey("NguoiCapNhat")]
    [InverseProperty("ThongTinCicNguoiCapNhatNavigations")]
    public virtual NguoiDung? NguoiCapNhatNavigation { get; set; }

    [ForeignKey("NguoiTao")]
    [InverseProperty("ThongTinCicNguoiTaoNavigations")]
    public virtual NguoiDung? NguoiTaoNavigation { get; set; }

    [ForeignKey("NguoiTraCuu")]
    [InverseProperty("ThongTinCicNguoiTraCuuNavigations")]
    public virtual NguoiDung? NguoiTraCuuNavigation { get; set; }

    [InverseProperty("MaCicNavigation")]
    public virtual ICollection<LichSuTraCuuCic> LichSuTraCuuCics { get; set; } = new List<LichSuTraCuuCic>();
}
