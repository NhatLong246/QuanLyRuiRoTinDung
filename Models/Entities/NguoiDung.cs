using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuanLyRuiRoTinDung.Models.Entities;

[Table("NguoiDung")]
[Index("TenDangNhap", Name = "UQ__NguoiDun__55F68FC03DDB8B84", IsUnique = true)]
public partial class NguoiDung
{
    [Key]
    public int MaNguoiDung { get; set; }

    [StringLength(50)]
    public string TenDangNhap { get; set; } = null!;

    [StringLength(255)]
    public string MatKhauHash { get; set; } = null!;

    [StringLength(100)]
    public string HoTen { get; set; } = null!;

    [StringLength(100)]
    public string? Email { get; set; }

    [StringLength(20)]
    public string? SoDienThoai { get; set; }

    public int MaVaiTro { get; set; }

    public int? MaPhongBan { get; set; }

    public bool? TrangThaiHoatDong { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? LanDangNhapCuoi { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NgayTao { get; set; }

    public int? NguoiTao { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NgayCapNhat { get; set; }

    public int? NguoiCapNhat { get; set; }

    [InverseProperty("NguoiLapNavigation")]
    public virtual ICollection<BaoCaoRuiRo> BaoCaoRuiRoNguoiLapNavigations { get; set; } = new List<BaoCaoRuiRo>();

    [InverseProperty("NguoiPheDuyetNavigation")]
    public virtual ICollection<BaoCaoRuiRo> BaoCaoRuiRoNguoiPheDuyetNavigations { get; set; } = new List<BaoCaoRuiRo>();

    [InverseProperty("NguoiGiaiQuyetNavigation")]
    public virtual ICollection<CanhBao> CanhBaoNguoiGiaiQuyetNavigations { get; set; } = new List<CanhBao>();

    [InverseProperty("NguoiXuLyNavigation")]
    public virtual ICollection<CanhBao> CanhBaoNguoiXuLyNavigations { get; set; } = new List<CanhBao>();

    [InverseProperty("NguoiCapNhatNavigation")]
    public virtual ICollection<CauHinhHeThong> CauHinhHeThongs { get; set; } = new List<CauHinhHeThong>();

    [InverseProperty("NguoiDanhGiaNavigation")]
    public virtual ICollection<DanhGiaRuiRo> DanhGiaRuiRoNguoiDanhGiaNavigations { get; set; } = new List<DanhGiaRuiRo>();

    [InverseProperty("NguoiPheDuyetNavigation")]
    public virtual ICollection<DanhGiaRuiRo> DanhGiaRuiRoNguoiPheDuyetNavigations { get; set; } = new List<DanhGiaRuiRo>();

    [InverseProperty("NguoiTaoNavigation")]
    public virtual ICollection<DanhMucTinDung> DanhMucTinDungs { get; set; } = new List<DanhMucTinDung>();

    [InverseProperty("NguoiTaoNavigation")]
    public virtual ICollection<KhachHangCaNhan> KhachHangCaNhans { get; set; } = new List<KhachHangCaNhan>();

    [InverseProperty("NguoiTaoNavigation")]
    public virtual ICollection<KhachHangDoanhNghiep> KhachHangDoanhNghieps { get; set; } = new List<KhachHangDoanhNghiep>();

    [InverseProperty("MaNhanVienTinDungNavigation")]
    public virtual ICollection<KhoanVay> KhoanVayMaNhanVienTinDungNavigations { get; set; } = new List<KhoanVay>();

    [InverseProperty("NguoiCapNhatNavigation")]
    public virtual ICollection<KhoanVay> KhoanVayNguoiCapNhatNavigations { get; set; } = new List<KhoanVay>();

    [InverseProperty("NguoiPheDuyetNavigation")]
    public virtual ICollection<KhoanVay> KhoanVayNguoiPheDuyetNavigations { get; set; } = new List<KhoanVay>();

    [InverseProperty("NguoiTaoNavigation")]
    public virtual ICollection<KhoanVay> KhoanVayNguoiTaoNavigations { get; set; } = new List<KhoanVay>();

    [InverseProperty("NguoiTaoNavigation")]
    public virtual ICollection<LichSuDinhGiaTaiSan> LichSuDinhGiaTaiSans { get; set; } = new List<LichSuDinhGiaTaiSan>();

    [InverseProperty("NguoiTaoNavigation")]
    public virtual ICollection<LichSuTraNo> LichSuTraNoNguoiTaoNavigations { get; set; } = new List<LichSuTraNo>();

    [InverseProperty("NguoiThuTienNavigation")]
    public virtual ICollection<LichSuTraNo> LichSuTraNoNguoiThuTienNavigations { get; set; } = new List<LichSuTraNo>();

    [InverseProperty("NguoiXacNhanNavigation")]
    public virtual ICollection<LichSuTraNo> LichSuTraNoNguoiXacNhanNavigations { get; set; } = new List<LichSuTraNo>();

    [InverseProperty("NguoiThayDoiNavigation")]
    public virtual ICollection<LichSuTrangThaiKhoanVay> LichSuTrangThaiKhoanVays { get; set; } = new List<LichSuTrangThaiKhoanVay>();

    [ForeignKey("MaVaiTro")]
    [InverseProperty("NguoiDungs")]
    public virtual VaiTro MaVaiTroNavigation { get; set; } = null!;

    [InverseProperty("MaNguoiDungNavigation")]
    public virtual ICollection<NhatKyHoatDong> NhatKyHoatDongs { get; set; } = new List<NhatKyHoatDong>();

    [InverseProperty("NguoiTaoNavigation")]
    public virtual ICollection<TaiSanDamBao> TaiSanDamBaos { get; set; } = new List<TaiSanDamBao>();

    [InverseProperty("NguoiTaoNavigation")]
    public virtual ICollection<TheoDoiNoXau> TheoDoiNoXauNguoiTaoNavigations { get; set; } = new List<TheoDoiNoXau>();

    [InverseProperty("NguoiXuLyNavigation")]
    public virtual ICollection<TheoDoiNoXau> TheoDoiNoXauNguoiXuLyNavigations { get; set; } = new List<TheoDoiNoXau>();

    [InverseProperty("NguoiTaoNavigation")]
    public virtual ICollection<TieuChiDanhGiaRuiRo> TieuChiDanhGiaRuiRos { get; set; } = new List<TieuChiDanhGiaRuiRo>();

    [InverseProperty("NguoiTaoNavigation")]
    public virtual ICollection<ThongTinCic> ThongTinCicNguoiTaoNavigations { get; set; } = new List<ThongTinCic>();

    [InverseProperty("NguoiCapNhatNavigation")]
    public virtual ICollection<ThongTinCic> ThongTinCicNguoiCapNhatNavigations { get; set; } = new List<ThongTinCic>();

    [InverseProperty("NguoiTraCuuNavigation")]
    public virtual ICollection<ThongTinCic> ThongTinCicNguoiTraCuuNavigations { get; set; } = new List<ThongTinCic>();

    [InverseProperty("NguoiTraCuuNavigation")]
    public virtual ICollection<LichSuTraCuuCic> LichSuTraCuuCicNguoiTraCuuNavigations { get; set; } = new List<LichSuTraCuuCic>();

    [InverseProperty("NguoiTaoNavigation")]
    public virtual ICollection<HoSoVayFileDinhKem> HoSoVayFileDinhKems { get; set; } = new List<HoSoVayFileDinhKem>();

    [InverseProperty("NguoiCapNhatNavigation")]
    public virtual ICollection<GiaTriTaiSanThamChieu> GiaTriTaiSanThamChieus { get; set; } = new List<GiaTriTaiSanThamChieu>();
}
