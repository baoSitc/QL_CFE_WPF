using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace QL_CFE_WPF.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Models.SanPham> SanPhams { get; set; }
        public DbSet<Models.Ban> Bans { get; set; }
        public DbSet<Models.HoaDon> HoaDons { get; set; }
        public DbSet<Models.ChiTietHoaDon> ChiTietHoaDons { get; set; }
        public DbSet<Models.NhanVien> NhanViens { get; set; }
        public DbSet<Models.Role> Roles { get; set; }
        public DbSet<Models.Permission> Permissions { get; set; }
        public DbSet<Models.RolePermission> RolePermissions { get; set; }
        public DbSet<Models.TonKho> TonKhos { get; set; }
        public DbSet<Models.PhieuNhap> PhieuNhaps { get; set; }
        public DbSet<Models.PhieuNhapCT> PhieuNhapCTs { get; set; }
        public DbSet<Models.PhieuXuat> PhieuXuats { get; set; }
        public DbSet<Models.PhieuXuatCT> PhieuXuatCTs { get; set; }
        public DbSet<Models.Kho> Khos { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(
@"Data Source=192.0.0.251\sqlexpress;
Initial Catalog=QL_CAFE_WPF;
User Id=sa;
Password=Mclcnnbc@123Encovy;
TrustServerCertificate=True;");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Models.ChiTietHoaDon>()
                .HasKey(ct => new { ct.MaHD, ct.MaSP });
            
            // In AppDbContext.OnModelCreating
            modelBuilder.Entity<Models.ChiTietHoaDon>()
                .HasOne(ct => ct.HoaDon)
                .WithMany(hd => hd.ChiTietHoaDons)    // specify inverse navigation
                .HasForeignKey(ct => ct.MaHD);


            

            modelBuilder.Entity<Models.ChiTietHoaDon>()
                .HasOne(ct => ct.SanPham)
                .WithMany()
                .HasForeignKey(ct => ct.MaSP);

            //modelBuilder.Entity<Models.HoaDon>()
            //    .HasOne(hd => hd.Ban)
            //    .WithMany()
            //    .HasForeignKey(hd => hd.MaBan);
        }

    }
}
