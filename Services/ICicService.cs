using QuanLyRuiRoTinDung.Models.Entities;

namespace QuanLyRuiRoTinDung.Services
{
    public interface ICicService
    {
        Task<List<ThongTinCic>> GetAllCicAsync(string? searchTerm = null, string? loaiKhachHang = null, string? khuyenNghi = null);
        Task<ThongTinCic?> GetCicByIdAsync(int maCic);
        Task<ThongTinCic?> GetCicByCmndAsync(string soCmndCccd);
        Task<ThongTinCic?> GetCicByMstAsync(string maSoThue);
        Task<int> GetTotalCicCountAsync();
    }
}
