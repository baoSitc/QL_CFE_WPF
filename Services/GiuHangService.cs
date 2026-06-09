using QL_CFE_WPF.Models;
using System;
using System.Collections.Generic;
using System.Text;


namespace QL_CFE_WPF.Services
{
    public class GiuHangService
    {
        private readonly Data.AppDbContext _db;
        public GiuHangService(Data.AppDbContext db)
        {
            _db = db;
        }
        public bool TryReserve(int sanPhamId, int soLuong, int BanId, int hoaDonId)
        {
            using var tran = _db.Database.BeginTransaction();

            try
            {
                var ton = _db.TonKhos.First(x => x.SanPhamId == sanPhamId);

                var daGiu = _db.GiuHangs
                    .Where(x => x.SanPhamId == sanPhamId && x.TrangThai == 0)
                    .Sum(x => (int?)x.SoLuong) ?? 0;

                var available = ton.SoLuong - daGiu;

                if (available < soLuong)
                    return false;

                _db.GiuHangs.Add(new GiuHang
                {
                    SanPhamId = sanPhamId,
                    SoLuong = soLuong,
                    MaBan = BanId,
                    HoaDonId = hoaDonId,
                    CreatedAt = DateTime.Now,
                    ExpiredAt = DateTime.Now.AddMinutes(30),
                    TrangThai = 0
                });

                _db.SaveChanges();
                tran.Commit();

                return true;
            }
            catch
            {
                tran.Rollback();
                return false;
            }
        }

        public void Release(int sanPhamId, int hoaDonId, decimal soLuong)
        {
            var list = _db.GiuHangs
                .Where(x => x.SanPhamId == sanPhamId
                         && x.HoaDonId == hoaDonId
                         && x.TrangThai == 0)
                .OrderBy(x => x.CreatedAt)
                .ToList();

            decimal remain = soLuong;

            foreach (var g in list)
            {
                if (remain <= 0) break;

                if (g.SoLuong <= remain)
                {
                    remain -= g.SoLuong;
                    g.TrangThai = 2; // release
                }
                else
                {
                    g.SoLuong -= remain;
                    remain = 0;
                }
            }

            _db.SaveChanges();
        }

        public decimal GetReserved(int sanPhamId)
        {
            return _db.GiuHangs
                .Where(x => x.SanPhamId == sanPhamId && x.TrangThai == 0)
                .Sum(x => (decimal?)x.SoLuong) ?? 0;
        }
    }
}
