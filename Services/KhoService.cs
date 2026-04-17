using QL_CFE_WPF.Data;
using QL_CFE_WPF.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace QL_CFE_WPF.Services
{
    
    public class KhoService
    {
        private readonly AppDbContext _db;

       public KhoService(AppDbContext db)
        {
            _db = db;
        }
        public void NhapKho(List<NhapKhoItem> items, int nhanVienId, int khoId)
        {
            using var tran = _db.Database.BeginTransaction();

            try
            {
                var phieuNhap = new PhieuNhap
                {
                    NgayNhap = DateTime.Now,
                    NhanVienId = nhanVienId,
                    TongTien = items.Sum(x => x.SoLuong * x.DonGia)
                };

                _db.PhieuNhaps.Add(phieuNhap);
                _db.SaveChanges(); // để lấy Id

                foreach (var item in items)
                {
                    // thêm chi tiết
                    _db.PhieuNhapCTs.Add(new PhieuNhapCT
                    {
                        PhieuNhapId = phieuNhap.Id,
                        SanPhamId = item.SanPhamId,
                        SoLuong = item.SoLuong,
                        DonGia = item.DonGia
                    });

                    // cập nhật tồn
                    var ton = _db.TonKhos
                        .FirstOrDefault(x => x.SanPhamId == item.SanPhamId && x.KhoId == khoId);

                    if (ton == null)
                    {
                        ton = new TonKho
                        {
                            SanPhamId = item.SanPhamId,
                            KhoId = khoId,
                            SoLuong = 0
                        };
                        _db.TonKhos.Add(ton);
                    }

                    ton.SoLuong += item.SoLuong;
                }

                _db.SaveChanges();
                tran.Commit();
            }
            catch (Exception)
            {
                tran.Rollback();
                throw;
            }
        }
        public void XuatKho(List<XuatKhoItem> items, int nhanVienId, int khoId, string loaiXuat)
        {
            using var tran = _db.Database.BeginTransaction();

            try
            {
                var phieuXuat = new PhieuXuat
                {
                    NgayXuat = DateTime.Now,
                    NhanVienId = nhanVienId,
                    LoaiXuat = loaiXuat
                };

                _db.PhieuXuats.Add(phieuXuat);
                _db.SaveChanges();

                foreach (var item in items)
                {
                    var ton = _db.TonKhos
                        .FirstOrDefault(x => x.SanPhamId == item.SanPhamId && x.KhoId == khoId);

                    if (ton == null || ton.SoLuong < item.SoLuong)
                        throw new Exception($"Không đủ tồn cho hàng {item.SanPhamId}");

                    // trừ tồn
                    ton.SoLuong -= item.SoLuong;

                    // chi tiết xuất
                    _db.PhieuXuatCTs.Add(new PhieuXuatCT
                    {
                        PhieuXuatId = phieuXuat.Id,
                        SanPhamId = item.SanPhamId,
                        SoLuong = item.SoLuong
                    });
                }

                _db.SaveChanges();
                tran.Commit();
            }
            catch (Exception)
            {
                tran.Rollback();
                throw;
            }
        }

    }
}
