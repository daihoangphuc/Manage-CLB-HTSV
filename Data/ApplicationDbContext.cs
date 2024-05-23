using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using website_CLB_HTSV.Models;

namespace website_CLB_HTSV.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Khoa> Khoa { get; set; }
        public DbSet<LopHoc> LopHoc { get; set; }
        public DbSet<ChucVu> ChucVu { get; set; }
        public DbSet<SinhVien> SinhVien { get; set; }
        public DbSet<HoatDong> HoatDong { get; set; }
        public DbSet<DangKyHoatDong> DangKyHoatDong { get; set; }
        public DbSet<TinTuc> TinTuc { get; set; }
        public DbSet<TaiKhoan> TaiKhoan { get; set; }
        public DbSet<ThamGiaHoatDong> ThamGiaHoatDong { get; set; }
    }
}