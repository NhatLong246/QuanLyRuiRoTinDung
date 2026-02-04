using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using QuanLyRuiRoTinDung.Models.Entities;

namespace QuanLyRuiRoTinDung.Models.EF;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BaoCaoRuiRo> BaoCaoRuiRos { get; set; }

    public virtual DbSet<CanhBao> CanhBaos { get; set; }

    public virtual DbSet<CauHinhHeThong> CauHinhHeThongs { get; set; }

    public virtual DbSet<ChiTietDanhGiaRuiRo> ChiTietDanhGiaRuiRos { get; set; }

    public virtual DbSet<DanhGiaRuiRo> DanhGiaRuiRos { get; set; }

    public virtual DbSet<DanhMucTinDung> DanhMucTinDungs { get; set; }

    public virtual DbSet<GiaTriTaiSanThamChieu> GiaTriTaiSanThamChieus { get; set; }

    public virtual DbSet<KhachHangCaNhan> KhachHangCaNhans { get; set; }

    public virtual DbSet<KhachHangDoanhNghiep> KhachHangDoanhNghieps { get; set; }

    public virtual DbSet<KhoanVay> KhoanVays { get; set; }

    public virtual DbSet<KhoanVayTaiSan> KhoanVayTaiSans { get; set; }

    public virtual DbSet<HoSoVayFileDinhKem> HoSoVayFileDinhKems { get; set; }

    public virtual DbSet<LichSuDinhGiaTaiSan> LichSuDinhGiaTaiSans { get; set; }

    public virtual DbSet<LichSuTraNo> LichSuTraNos { get; set; }

    public virtual DbSet<LichSuTrangThaiKhoanVay> LichSuTrangThaiKhoanVays { get; set; }

    public virtual DbSet<LoaiCanhBao> LoaiCanhBaos { get; set; }

    public virtual DbSet<LoaiKhachHang> LoaiKhachHangs { get; set; }

    public virtual DbSet<LoaiKhoanVay> LoaiKhoanVays { get; set; }

    public virtual DbSet<LoaiTaiSanDamBao> LoaiTaiSanDamBaos { get; set; }

    public virtual DbSet<NguoiDung> NguoiDungs { get; set; }

    public virtual DbSet<NhatKyHoatDong> NhatKyHoatDongs { get; set; }

    public virtual DbSet<PhanLoaiNo> PhanLoaiNos { get; set; }

    public virtual DbSet<PhongBan> PhongBans { get; set; }

    public virtual DbSet<Quyen> Quyens { get; set; }

    public virtual DbSet<TaiSanDamBao> TaiSanDamBaos { get; set; }

    public virtual DbSet<TheoDoiNoXau> TheoDoiNoXaus { get; set; }

    public virtual DbSet<TieuChiDanhGiaRuiRo> TieuChiDanhGiaRuiRos { get; set; }

    public virtual DbSet<ThongTinCic> ThongTinCics { get; set; }

    public virtual DbSet<LichSuTraCuuCic> LichSuTraCuuCics { get; set; }

    public virtual DbSet<VaiTro> VaiTros { get; set; }

    public virtual DbSet<VaiTroQuyen> VaiTroQuyens { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        //Không cấu hình 
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BaoCaoRuiRo>(entity =>
        {
            entity.HasKey(e => e.MaBaoCao).HasName("PK__BaoCao_R__25A9188C3B5C254E");

            entity.Property(e => e.NgayLap).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.NguoiLapNavigation).WithMany(p => p.BaoCaoRuiRoNguoiLapNavigations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BaoCao_Ru__Nguoi__5F7E2DAC");

            entity.HasOne(d => d.NguoiPheDuyetNavigation).WithMany(p => p.BaoCaoRuiRoNguoiPheDuyetNavigations).HasConstraintName("FK__BaoCao_Ru__Nguoi__607251E5");
        });

        modelBuilder.Entity<CanhBao>(entity =>
        {
            entity.HasKey(e => e.MaCanhBao).HasName("PK__CanhBao__73C23D936146BBA1");

            entity.Property(e => e.NgayCanhBao).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.TrangThai).HasDefaultValue("Chưa xử lý");

            entity.HasOne(d => d.MaKhoanVayNavigation).WithMany(p => p.CanhBaos).HasConstraintName("FK__CanhBao__MaKhoan__55009F39");

            entity.HasOne(d => d.MaLoaiCanhBaoNavigation).WithMany(p => p.CanhBaos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CanhBao__MaLoaiC__540C7B00");

            entity.HasOne(d => d.NguoiGiaiQuyetNavigation).WithMany(p => p.CanhBaoNguoiGiaiQuyetNavigations).HasConstraintName("FK__CanhBao__NguoiGi__56E8E7AB");

            entity.HasOne(d => d.NguoiXuLyNavigation).WithMany(p => p.CanhBaoNguoiXuLyNavigations).HasConstraintName("FK__CanhBao__NguoiXu__55F4C372");
        });

        modelBuilder.Entity<CauHinhHeThong>(entity =>
        {
            entity.HasKey(e => e.MaCauHinh).HasName("PK__CauHinh___F0685B7DF316CC72");

            entity.Property(e => e.CoTheSua).HasDefaultValue(true);

            entity.HasOne(d => d.NguoiCapNhatNavigation).WithMany(p => p.CauHinhHeThongs).HasConstraintName("FK__CauHinh_H__Nguoi__72910220");
        });

        modelBuilder.Entity<ChiTietDanhGiaRuiRo>(entity =>
        {
            entity.HasKey(e => e.MaChiTiet).HasName("PK__ChiTiet___CDF0A1144A7C4656");

            entity.HasOne(d => d.MaDanhGiaNavigation).WithMany(p => p.ChiTietDanhGiaRuiRos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTiet_D__MaDan__4A8310C6");

            entity.HasOne(d => d.MaTieuChiNavigation).WithMany(p => p.ChiTietDanhGiaRuiRos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTiet_D__MaTie__4B7734FF");
        });

        modelBuilder.Entity<DanhGiaRuiRo>(entity =>
        {
            entity.HasKey(e => e.MaDanhGia).HasName("PK__DanhGia___AA9515BFD9A85005");

            entity.Property(e => e.NgayDanhGia).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.NgayTao).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.MaKhoanVayNavigation).WithMany(p => p.DanhGiaRuiRos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DanhGia_R__MaKho__45BE5BA9");

            entity.HasOne(d => d.NguoiDanhGiaNavigation).WithMany(p => p.DanhGiaRuiRoNguoiDanhGiaNavigations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DanhGia_R__Nguoi__46B27FE2");

            entity.HasOne(d => d.NguoiPheDuyetNavigation).WithMany(p => p.DanhGiaRuiRoNguoiPheDuyetNavigations).HasConstraintName("FK__DanhGia_R__Nguoi__47A6A41B");
        });

        modelBuilder.Entity<DanhMucTinDung>(entity =>
        {
            entity.HasKey(e => e.MaDanhMuc).HasName("PK__DanhMuc___B3750887FA9DCA14");

            entity.Property(e => e.NgayTao).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.NguoiTaoNavigation).WithMany(p => p.DanhMucTinDungs).HasConstraintName("FK__DanhMuc_T__Nguoi__5AB9788F");
        });

        modelBuilder.Entity<KhachHangCaNhan>(entity =>
        {
            entity.HasKey(e => e.MaKhachHang).HasName("PK__KhachHan__88D2F0E5BBF14DF5");

            entity.Property(e => e.NgayTao).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.TrangThaiHoatDong).HasDefaultValue(true);

            entity.HasOne(d => d.NguoiTaoNavigation).WithMany(p => p.KhachHangCaNhans).HasConstraintName("FK__KhachHang__Nguoi__6D0D32F4");
        });

        modelBuilder.Entity<KhachHangDoanhNghiep>(entity =>
        {
            entity.HasKey(e => e.MaKhachHang).HasName("PK__KhachHan__88D2F0E5012D7663");

            entity.Property(e => e.NgayTao).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.TrangThaiHoatDong).HasDefaultValue(true);

            entity.HasOne(d => d.NguoiTaoNavigation).WithMany(p => p.KhachHangDoanhNghieps).HasConstraintName("FK__KhachHang__Nguoi__73BA3083");
        });

        modelBuilder.Entity<KhoanVay>(entity =>
        {
            entity.HasKey(e => e.MaKhoanVay).HasName("PK__KhoanVay__FC48C389FFC9A59B");

            entity.Property(e => e.CoTaiSanDamBao).HasDefaultValue(false);
            entity.Property(e => e.NgayNopHoSo).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.NgayTao).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.SoDuGocConLai).HasDefaultValue(0m);
            entity.Property(e => e.SoDuLaiConLai).HasDefaultValue(0m);
            entity.Property(e => e.SoKyDaTra).HasDefaultValue(0);
            entity.Property(e => e.SoNgayQuaHan).HasDefaultValue(0);
            entity.Property(e => e.TongDaThanhToan).HasDefaultValue(0m);
            entity.Property(e => e.TongDuNo).HasDefaultValue(0m);
            entity.Property(e => e.TrangThaiKhoanVay).HasDefaultValue("Đang xử lý");
            entity.Property(e => e.TyLeHoanThanh).HasDefaultValue(0m);

            entity.HasOne(d => d.MaLoaiVayNavigation).WithMany(p => p.KhoanVays)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__KhoanVay__MaLoai__14270015");

            entity.HasOne(d => d.MaNhanVienTinDungNavigation).WithMany(p => p.KhoanVayMaNhanVienTinDungNavigations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__KhoanVay__MaNhan__151B244E");

            entity.HasOne(d => d.NguoiCapNhatNavigation).WithMany(p => p.KhoanVayNguoiCapNhatNavigations).HasConstraintName("FK__KhoanVay__NguoiC__17F790F9");

            entity.HasOne(d => d.NguoiPheDuyetNavigation).WithMany(p => p.KhoanVayNguoiPheDuyetNavigations).HasConstraintName("FK__KhoanVay__NguoiP__160F4887");

            entity.HasOne(d => d.NguoiTaoNavigation).WithMany(p => p.KhoanVayNguoiTaoNavigations).HasConstraintName("FK__KhoanVay__NguoiT__17036CC0");
        });

        modelBuilder.Entity<KhoanVayTaiSan>(entity =>
        {
            entity.HasKey(e => e.MaLienKet).HasName("PK__KhoanVay__64752CF689DEE6E0");

            entity.HasOne(d => d.MaKhoanVayNavigation).WithMany(p => p.KhoanVayTaiSans)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__KhoanVay___MaKho__245D67DE");

            entity.HasOne(d => d.MaTaiSanNavigation).WithMany(p => p.KhoanVayTaiSans)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__KhoanVay___MaTai__25518C17");
        });

        modelBuilder.Entity<LichSuDinhGiaTaiSan>(entity =>
        {
            entity.HasKey(e => e.MaLichSu).HasName("PK__LichSu_D__C443222A4086B839");

            entity.Property(e => e.NgayTao).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.MaTaiSanNavigation).WithMany(p => p.LichSuDinhGiaTaiSans)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LichSu_Di__MaTai__01142BA1");

            entity.HasOne(d => d.NguoiTaoNavigation).WithMany(p => p.LichSuDinhGiaTaiSans).HasConstraintName("FK__LichSu_Di__Nguoi__02084FDA");
        });

        modelBuilder.Entity<LichSuTraNo>(entity =>
        {
            entity.HasKey(e => e.MaGiaoDich).HasName("PK__LichSu_T__0A2A24EB57D6E47D");

            entity.Property(e => e.NgayTao).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.SoNgayTraCham).HasDefaultValue(0);
            entity.Property(e => e.SoTienGocDaTra).HasDefaultValue(0m);
            entity.Property(e => e.SoTienLaiDaTra).HasDefaultValue(0m);
            entity.Property(e => e.SoTienPhiTraCham).HasDefaultValue(0m);
            entity.Property(e => e.TongDaTra).HasDefaultValue(0m);
            entity.Property(e => e.TrangThai).HasDefaultValue("Chưa trả");

            entity.HasOne(d => d.MaKhoanVayNavigation).WithMany(p => p.LichSuTraNos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LichSu_Tr__MaKho__2FCF1A8A");

            entity.HasOne(d => d.NguoiTaoNavigation).WithMany(p => p.LichSuTraNoNguoiTaoNavigations).HasConstraintName("FK__LichSu_Tr__Nguoi__32AB8735");

            entity.HasOne(d => d.NguoiThuTienNavigation).WithMany(p => p.LichSuTraNoNguoiThuTienNavigations).HasConstraintName("FK__LichSu_Tr__Nguoi__30C33EC3");

            entity.HasOne(d => d.NguoiXacNhanNavigation).WithMany(p => p.LichSuTraNoNguoiXacNhanNavigations).HasConstraintName("FK__LichSu_Tr__Nguoi__31B762FC");
        });

        modelBuilder.Entity<LichSuTrangThaiKhoanVay>(entity =>
        {
            entity.HasKey(e => e.MaLichSu).HasName("PK__LichSu_T__C443222A1562B4A5");

            entity.Property(e => e.NgayThayDoi).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.MaKhoanVayNavigation).WithMany(p => p.LichSuTrangThaiKhoanVays)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LichSu_Tr__MaKho__3864608B");

            entity.HasOne(d => d.NguoiThayDoiNavigation).WithMany(p => p.LichSuTrangThaiKhoanVays)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LichSu_Tr__Nguoi__395884C4");
        });

        modelBuilder.Entity<LoaiCanhBao>(entity =>
        {
            entity.HasKey(e => e.MaLoaiCanhBao).HasName("PK__LoaiCanh__40A49C5A799EAF6A");

            entity.Property(e => e.TrangThaiHoatDong).HasDefaultValue(true);
        });

        modelBuilder.Entity<LoaiKhachHang>(entity =>
        {
            entity.HasKey(e => e.MaLoaiKh).HasName("PK__LoaiKhac__12250B7E0AB25627");
        });

        modelBuilder.Entity<LoaiKhoanVay>(entity =>
        {
            entity.HasKey(e => e.MaLoaiVay).HasName("PK__LoaiKhoa__621128304E754564");

            entity.Property(e => e.TrangThaiHoatDong).HasDefaultValue(true);
        });

        modelBuilder.Entity<LoaiTaiSanDamBao>(entity =>
        {
            entity.HasKey(e => e.MaLoaiTaiSan).HasName("PK__LoaiTaiS__DAFA3F3C54EEFE86");
        });

        modelBuilder.Entity<NguoiDung>(entity =>
        {
            entity.HasKey(e => e.MaNguoiDung).HasName("PK__NguoiDun__C539D76232CDD8E7");

            entity.Property(e => e.NgayTao).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.TrangThaiHoatDong).HasDefaultValue(true);

            entity.HasOne(d => d.MaVaiTroNavigation).WithMany(p => p.NguoiDungs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__NguoiDung__MaVai__5165187F");
        });

        modelBuilder.Entity<NhatKyHoatDong>(entity =>
        {
            entity.HasKey(e => e.MaNhatKy).HasName("PK__NhatKy_H__E42EF42E8BA074D8");

            entity.Property(e => e.ThoiGian).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.MaNguoiDungNavigation).WithMany(p => p.NhatKyHoatDongs).HasConstraintName("FK__NhatKy_Ho__MaNgu__6DCC4D03");
        });

        modelBuilder.Entity<PhanLoaiNo>(entity =>
        {
            entity.HasKey(e => e.MaPhanLoai).HasName("PK__PhanLoai__E8C0182E4F6873CB");
        });

        modelBuilder.Entity<PhongBan>(entity =>
        {
            entity.HasKey(e => e.MaPhongBan).HasName("PK__PhongBan__D0910CC88CA8020C");

            entity.Property(e => e.NgayTao).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.TrangThaiHoatDong).HasDefaultValue(true);
        });

        modelBuilder.Entity<Quyen>(entity =>
        {
            entity.HasKey(e => e.MaQuyen).HasName("PK__Quyen__1D4B7ED4D4F55FEE");

            entity.Property(e => e.NgayTao).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<TaiSanDamBao>(entity =>
        {
            entity.HasKey(e => e.MaTaiSan).HasName("PK__TaiSanDa__8DB7C7BE899B007E");

            entity.Property(e => e.NgayTao).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.TrangThaiSuDung).HasDefaultValue("Chưa thế chấp");

            entity.HasOne(d => d.MaLoaiTaiSanNavigation).WithMany(p => p.TaiSanDamBaos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TaiSanDam__MaLoa__7C4F7684");

            entity.HasOne(d => d.NguoiTaoNavigation).WithMany(p => p.TaiSanDamBaos).HasConstraintName("FK__TaiSanDam__Nguoi__7D439ABD");
        });

        modelBuilder.Entity<HoSoVayFileDinhKem>(entity =>
        {
            entity.HasKey(e => e.MaFile).HasName("PK__HoSoVay___F0FEF756E2016B52");

            entity.Property(e => e.NgayTao).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.TrangThai).HasDefaultValue(true);

            entity.HasOne(d => d.MaKhoanVayNavigation).WithMany(p => p.HoSoVayFileDinhKems)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__HoSoVay_F__MaKho__CASCADE");

            entity.HasOne(d => d.NguoiTaoNavigation).WithMany(p => p.HoSoVayFileDinhKems)
                .HasConstraintName("FK__HoSoVay_F__Nguoi__CASCADE");
        });

        modelBuilder.Entity<TheoDoiNoXau>(entity =>
        {
            entity.HasKey(e => e.MaNoXau).HasName("PK__TheoDoi___EAE49BD82072B26B");

            entity.Property(e => e.NgayTao).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.MaKhoanVayNavigation).WithMany(p => p.TheoDoiNoXaus)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TheoDoi_N__MaKho__671F4F74");

            entity.HasOne(d => d.MaPhanLoaiNavigation).WithMany(p => p.TheoDoiNoXaus)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TheoDoi_N__MaPha__681373AD");

            entity.HasOne(d => d.NguoiTaoNavigation).WithMany(p => p.TheoDoiNoXauNguoiTaoNavigations).HasConstraintName("FK__TheoDoi_N__Nguoi__69FBBC1F");

            entity.HasOne(d => d.NguoiXuLyNavigation).WithMany(p => p.TheoDoiNoXauNguoiXuLyNavigations).HasConstraintName("FK__TheoDoi_N__Nguoi__690797E6");
        });

        modelBuilder.Entity<TieuChiDanhGiaRuiRo>(entity =>
        {
            entity.HasKey(e => e.MaTieuChi).HasName("PK__TieuChi___41F85A35F26E6682");

            entity.Property(e => e.DiemToiDa).HasDefaultValue(100);
            entity.Property(e => e.DiemToiThieu).HasDefaultValue(0);
            entity.Property(e => e.NgayTao).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.TrangThaiHoatDong).HasDefaultValue(true);

            entity.HasOne(d => d.NguoiTaoNavigation).WithMany(p => p.TieuChiDanhGiaRuiRos).HasConstraintName("FK__TieuChi_D__Nguoi__40F9A68C");
        });

        modelBuilder.Entity<VaiTro>(entity =>
        {
            entity.HasKey(e => e.MaVaiTro).HasName("PK__VaiTro__C24C41CF71BF5F2D");

            entity.Property(e => e.NgayTao).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.TrangThaiHoatDong).HasDefaultValue(true);
        });

        modelBuilder.Entity<VaiTroQuyen>(entity =>
        {
            entity.HasKey(e => e.MaVaiTroQuyen).HasName("PK__VaiTro_Q__947B8C7E82A61186");

            entity.Property(e => e.DuocPheDuyet).HasDefaultValue(false);
            entity.Property(e => e.DuocSua).HasDefaultValue(false);
            entity.Property(e => e.DuocThem).HasDefaultValue(false);
            entity.Property(e => e.DuocXem).HasDefaultValue(false);
            entity.Property(e => e.DuocXoa).HasDefaultValue(false);
            entity.Property(e => e.NgayTao).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.MaQuyenNavigation).WithMany(p => p.VaiTroQuyens)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__VaiTro_Qu__MaQuy__6383C8BA");

            entity.HasOne(d => d.MaVaiTroNavigation).WithMany(p => p.VaiTroQuyens)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__VaiTro_Qu__MaVai__628FA481");
        });

        modelBuilder.Entity<ThongTinCic>(entity =>
        {
            entity.HasKey(e => e.MaCic).HasName("PK__ThongTin__25A9188C");

            entity.Property(e => e.NgayTao).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.NgayTraCuuCuoi).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.TrangThaiHoatDong).HasDefaultValue(true);
            entity.Property(e => e.TongSoKhoanVayCic).HasDefaultValue(0);
            entity.Property(e => e.SoKhoanVayDangVayCic).HasDefaultValue(0);
            entity.Property(e => e.SoKhoanVayDaTraXongCic).HasDefaultValue(0);
            entity.Property(e => e.SoKhoanVayQuaHanCic).HasDefaultValue(0);
            entity.Property(e => e.SoKhoanVayNoXauCic).HasDefaultValue(0);
            entity.Property(e => e.TongDuNoCic).HasDefaultValue(0m);
            entity.Property(e => e.DuNoQuaHanCic).HasDefaultValue(0m);
            entity.Property(e => e.DuNoNoXauCic).HasDefaultValue(0m);
            entity.Property(e => e.DuNoToiDaCic).HasDefaultValue(0m);
            entity.Property(e => e.TongGiaTriVayCic).HasDefaultValue(0m);
            entity.Property(e => e.SoLanQuaHanCic).HasDefaultValue(0);
            entity.Property(e => e.SoLanNoXauCic).HasDefaultValue(0);
            entity.Property(e => e.SoNgayQuaHanToiDaCic).HasDefaultValue(0);
            entity.Property(e => e.ThoiGianTraNoTotCic).HasDefaultValue(0);
            entity.Property(e => e.TyLeTraNoDungHanCic).HasDefaultValue(0m);
            entity.Property(e => e.SoToChucTinDungDaVay).HasDefaultValue(0);

            entity.HasOne(d => d.NguoiCapNhatNavigation)
                .WithMany(p => p.ThongTinCicNguoiCapNhatNavigations)
                .HasForeignKey(d => d.NguoiCapNhat)
                .HasConstraintName("FK_ThongTin_CIC_NguoiCapNhat");

            entity.HasOne(d => d.NguoiTaoNavigation)
                .WithMany(p => p.ThongTinCicNguoiTaoNavigations)
                .HasForeignKey(d => d.NguoiTao)
                .HasConstraintName("FK_ThongTin_CIC_NguoiTao");

            entity.HasOne(d => d.NguoiTraCuuNavigation)
                .WithMany(p => p.ThongTinCicNguoiTraCuuNavigations)
                .HasForeignKey(d => d.NguoiTraCuu)
                .HasConstraintName("FK_ThongTin_CIC_NguoiTraCuu");
        });

        modelBuilder.Entity<LichSuTraCuuCic>(entity =>
        {
            entity.HasKey(e => e.MaLichSu).HasName("PK__LichSu_T__C443222A");

            entity.Property(e => e.NgayTraCuu).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.MaCicNavigation).WithMany(p => p.LichSuTraCuuCics).HasConstraintName("FK__LichSu_Tr__MaCIC__");

            entity.HasOne(d => d.NguoiTraCuuNavigation).WithMany(p => p.LichSuTraCuuCicNguoiTraCuuNavigations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LichSu_Tr__Nguoi__");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
