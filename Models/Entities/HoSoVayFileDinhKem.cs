using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuanLyRuiRoTinDung.Models.Entities;

[Table("HoSoVay_FileDinhKem")]
[Index("MaKhoanVay", Name = "IX_HoSoVay_FileDinhKem_MaKhoanVay")]
[Index("LoaiFile", Name = "IX_HoSoVay_FileDinhKem_LoaiFile")]
public partial class HoSoVayFileDinhKem
{
    [Key]
    public int MaFile { get; set; }

    public int MaKhoanVay { get; set; }

    [StringLength(50)]
    public string LoaiFile { get; set; } = null!; // PhapLy, TaiChinh, TaiSanDamBao

    [StringLength(255)]
    public string TenFile { get; set; } = null!; // Tên file gốc

    [StringLength(255)]
    public string TenFileLuu { get; set; } = null!; // Tên file đã lưu (hash)

    [StringLength(500)]
    public string DuongDan { get; set; } = null!; // Đường dẫn đầy đủ

    public long? KichThuoc { get; set; } // Kích thước (bytes)

    [StringLength(10)]
    public string? DinhDang { get; set; } // PDF, JPG, PNG, DOCX

    [StringLength(500)]
    public string? MoTa { get; set; } // Mô tả (tùy chọn)

    [Column(TypeName = "datetime")]
    public DateTime? NgayTao { get; set; }

    public int? NguoiTao { get; set; }

    public bool? TrangThai { get; set; } // 1: Đang sử dụng, 0: Đã xóa

    [ForeignKey("MaKhoanVay")]
    [InverseProperty("HoSoVayFileDinhKems")]
    public virtual KhoanVay MaKhoanVayNavigation { get; set; } = null!;

    [ForeignKey("NguoiTao")]
    [InverseProperty("HoSoVayFileDinhKems")]
    public virtual NguoiDung? NguoiTaoNavigation { get; set; }
}
