using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QL_CFE_WPF.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace QL_CFE_WPF.ViewModels
{
    public partial class BaoCaoHangHoaViewModel : ObservableObject
    {
        public ObservableCollection<BaoCaoHangHoaModel> BaoCaoHangHoa { get; set; } = new();

        public List<KieuBaoCao> DanhSachKieu { get; } =
            Enum.GetValues(typeof(KieuBaoCao)).Cast<KieuBaoCao>().ToList();

        [ObservableProperty]
        private KieuBaoCao kieuDangChon = KieuBaoCao.ChiTiet;

        [ObservableProperty]
        private DateTime tuNgay = DateTime.Today;

        [ObservableProperty]
        private DateTime denNgay = DateTime.Today;

        public decimal TongTien => BaoCaoHangHoa.Sum(x => x.ThanhTien);
        

        [RelayCommand]
        void LoadBaoCaoHangHoa()
        {
            using var db = new Data.AppDbContext();

            IQueryable<ChiTietHoaDon> query = db.ChiTietHoaDons
                
                .Where(ct => ct.HoaDon.TrangThai == 1 &&
                             ct.HoaDon.Ngay >= TuNgay &&
                             ct.HoaDon.Ngay <= DenNgay.AddDays(1).AddTicks(-1));

            List<BaoCaoHangHoaModel> data;

            switch (KieuDangChon)
            {
                case KieuBaoCao.ChiTiet:
                    data = query
                        .Select(ct => new BaoCaoHangHoaModel
                    {
                        TenSP = ct.SanPham.TenSP,
                        SoLuong = ct.SoLuong,
                        DonGia = ct.Gia,
                        ThanhTien = ct.SoLuong * ct.Gia,
                        Ngay = ct.HoaDon.Ngay,
                        SoBan = ct.HoaDon.MaBan.ToString(),
                        TenNhom = ct.SanPham.NhomHang.TenNhom,
                    })
                        .OrderBy(ct => ct.TenNhom)                     
                    .ThenByDescending(x => x.Ngay)
                    .ToList();
                    break;

                case KieuBaoCao.TongHop:
                    data = query
                        .GroupBy(ct => ct.SanPham.TenSP)
                        .Select(g => new BaoCaoHangHoaModel
                        {
                            TenSP = g.Key,
                            SoLuong = g.Sum(x => x.SoLuong),
                            DonGia = g.First().Gia,
                            ThanhTien = g.Sum(x => x.SoLuong * x.Gia),
                            TenNhom = g.First().SanPham.NhomHang.TenNhom

                        })                        
                        .OrderByDescending(x => x.TenNhom)
                        .ToList();
                    break;

                case KieuBaoCao.TopBanChay:
                    data = query
                        .GroupBy(ct => ct.SanPham.TenSP)
                        .Select(g => new BaoCaoHangHoaModel
                        {
                            TenSP = g.Key,
                            SoLuong = g.Sum(x => x.SoLuong),
                            ThanhTien = g.Sum(x => x.SoLuong * x.Gia)
                        })
                        .OrderByDescending(x => x.SoLuong)
                        .Take(5)
                        .ToList();
                    break;

                default:
                    data = new List<BaoCaoHangHoaModel>();
                    break;
            }

            BaoCaoHangHoa.Clear();
            foreach (var item in data)
                BaoCaoHangHoa.Add(item);

            OnPropertyChanged(nameof(TongTien));
        }
    }
}
