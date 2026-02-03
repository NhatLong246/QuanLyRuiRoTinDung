using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuanLyRuiRoTinDung.Models.Entities;

[Table("KhoanVay")]
[Index("MaKhoanVayCode", Name = "UQ__KhoanVay__F0FEF756E2016B51", IsUnique = true)]
public partial class KhoanVay
{
    [Key]
    public int MaKhoanVay { get; set; }

    [Column("MaKhoanVay_Code")]
    [StringLength(20)]
    public string MaKhoanVayCode { get; set; } = null!;

    [StringLength(20)]
    public string LoaiKhachHang { get; set; } = null!;

    public int MaKhachHang { get; set; }

    public int MaLoaiVay { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal SoTienVay { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    [Range(0, 100, ErrorMessage = "Lãi suất phải từ 0 đến 100%.")]
    public decimal LaiSuat { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Thời hạn vay phải lớn hơn 0 tháng.")]
    public int KyHanVay { get; set; }

    [StringLength(50)]
    public string? HinhThucTraNo { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? SoTienTraHangThang { get; set; }

    [StringLength(200)]
    public string? MucDichVay { get; set; }

    public bool? CoTaiSanDamBao { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NgayNopHoSo { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NgayPheDuyet { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NgayGiaiNgan { get; set; }

    public DateOnly? NgayBatDauTra { get; set; }

    public DateOnly? NgayDaoHan { get; set; }

    [StringLength(50)]
    public string TrangThaiKhoanVay { get; set; } = null!;

    public int MaNhanVienTinDung { get; set; }

    [StringLength(20)]
    public string? MucDoRuiRo { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? DiemRuiRo { get; set; }

    [StringLength(10)]
    public string? XepHangRuiRo { get; set; }

    public int? NguoiPheDuyet { get; set; }

    [StringLength(50)]
    public string? CapPheDuyet { get; set; }

    [StringLength(500)]
    public string? LyDoTuChoi { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? SoDuGocConLai { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? SoDuLaiConLai { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? TongDaThanhToan { get; set; }

    public int? SoKyDaTra { get; set; }

    public int? SoKyConLai { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? TongDuNo { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? TyLeHoanThanh { get; set; }

    public int? SoNgayQuaHan { get; set; }

    public int? MaPhanLoaiNo { get; set; }

    public DateOnly? NgayPhanLoaiNo { get; set; }

    [StringLength(1000)]
    public string? GhiChu { get; set; }

    [StringLength(1000)]
    public string? GhiChuNhanVien { get; set; }

    [StringLength(500)]
    public string? DuongDanHoSo { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NgayTao { get; set; }

    public int? NguoiTao { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NgayCapNhat { get; set; }

    public int? NguoiCapNhat { get; set; }

    [InverseProperty("MaKhoanVayNavigation")]
    public virtual ICollection<CanhBao> CanhBaos { get; set; } = new List<CanhBao>();

    [InverseProperty("MaKhoanVayNavigation")]
    public virtual ICollection<DanhGiaRuiRo> DanhGiaRuiRos { get; set; } = new List<DanhGiaRuiRo>();

    [InverseProperty("MaKhoanVayNavigation")]
    public virtual ICollection<KhoanVayTaiSan> KhoanVayTaiSans { get; set; } = new List<KhoanVayTaiSan>();

    [InverseProperty("MaKhoanVayNavigation")]
    public virtual ICollection<HoSoVayFileDinhKem> HoSoVayFileDinhKems { get; set; } = new List<HoSoVayFileDinhKem>();

    [InverseProperty("MaKhoanVayNavigation")]
    public virtual ICollection<LichSuTraNo> LichSuTraNos { get; set; } = new List<LichSuTraNo>();

    [InverseProperty("MaKhoanVayNavigation")]
    public virtual ICollection<LichSuTrangThaiKhoanVay> LichSuTrangThaiKhoanVays { get; set; } = new List<LichSuTrangThaiKhoanVay>();

    [ForeignKey("MaLoaiVay")]
    [InverseProperty("KhoanVays")]
    public virtual LoaiKhoanVay MaLoaiVayNavigation { get; set; } = null!;

    [ForeignKey("MaNhanVienTinDung")]
    [InverseProperty("KhoanVayMaNhanVienTinDungNavigations")]
    public virtual NguoiDung MaNhanVienTinDungNavigation { get; set; } = null!;

    [ForeignKey("NguoiCapNhat")]
    [InverseProperty("KhoanVayNguoiCapNhatNavigations")]
    public virtual NguoiDung? NguoiCapNhatNavigation { get; set; }

    [ForeignKey("NguoiPheDuyet")]
    [InverseProperty("KhoanVayNguoiPheDuyetNavigations")]
    public virtual NguoiDung? NguoiPheDuyetNavigation { get; set; }

    [ForeignKey("NguoiTao")]
    [InverseProperty("KhoanVayNguoiTaoNavigations")]
    public virtual NguoiDung? NguoiTaoNavigation { get; set; }

    [InverseProperty("MaKhoanVayNavigation")]
    public virtual ICollection<TheoDoiNoXau> TheoDoiNoXaus { get; set; } = new List<TheoDoiNoXau>();
}
