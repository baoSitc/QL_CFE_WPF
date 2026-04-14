using CommunityToolkit.Mvvm.ComponentModel;
using LiveCharts;
using LiveCharts.Wpf;
using QL_CFE_WPF.Data;
using QL_CFE_WPF.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace QL_CFE_WPF.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        public decimal DoanhThuHomNay { get; set; }
        public int SoHoaDon { get; set; }
        public int SoBanDangDung { get; set; }
        public int SoBanDaPhucVu { get; set; }
        public ObservableCollection<KieuThongKe> DanhSachKieu { get; set; }
        [ObservableProperty]
        private KieuThongKe kieuDangChon;

        public ObservableCollection<TopSanPhamModel> TopSanPham { get; set; } = new();

        public SeriesCollection SeriesDoanhThu { get; set; }
        public SeriesCollection Series { get; set; }
        public string[] Labels { get; set; }
        public Func<double, string> Formatter { get; set; }
        public Func<ChartPoint, string> PointFormatter { get; set; }
        public Func<double, string> SoBanFormatter { get; set; } =
    value => ((int)value).ToString();
        partial void OnKieuDangChonChanged(KieuThongKe value)
        {
            LoadChart();
        }

        public DashboardViewModel()
        {
            Formatter = value => value.ToString("N0") + " đ";
            PointFormatter = chartPoint => chartPoint.Y.ToString("N0") + " đ";
            DanhSachKieu = new ObservableCollection<KieuThongKe>
            {
                KieuThongKe.HomNay,
                KieuThongKe.HomQua,
                KieuThongKe.BayNgayQua,
                KieuThongKe.ThangNay,
                KieuThongKe.ThangTruoc,
            };
            KieuDangChon = KieuThongKe.HomNay;
            Load();
            LoadChart();
            LoadTop();
        }


        void LoadChart()
        {
            using var db = new AppDbContext();

            var query = db.HoaDons
                .Where(x => x.TrangThai == 1 && x.NgayThanhToan.HasValue);

           List<ChartItem> result;

            if (KieuDangChon == KieuThongKe.HomNay)
            {
                var rawData = db.HoaDons
    .Where(x => x.TrangThai == 1 && x.NgayThanhToan.HasValue && x.NgayThanhToan.Value.Date == DateTime.Today)
    .GroupBy(x => new
    {
        x.NgayThanhToan.Value.Date

    })
    .Select(g => new
    {
        g.Key.Date,
        Tong = g.Sum(x => x.TongTien),
        SoBan = g.Count()
    })
    .OrderBy(x => x.Date)
    .ToList(); // 🔥 QUAN TRỌNG

                // 👉 format ở đây (C#)
                result = rawData.Select(x => new ChartItem
                {
                    Label = x.Date.ToString("dd/MM/yyyy"),
                    Tong = x.Tong,
                    SoBan = x.SoBan
                }).ToList();

            }
            else if (KieuDangChon == KieuThongKe.HomQua)
            {
                var rawData = db.HoaDons
     .Where(x => x.TrangThai == 1 && x.NgayThanhToan.HasValue && x.NgayThanhToan.Value.Date == DateTime.Today.AddDays(-1))
     .GroupBy(x => new
     {
         x.NgayThanhToan.Value.Date,

     })
     .Select(g => new
     {
         g.Key.Date,

         Tong = g.Sum(x => x.TongTien), SoBan = g.Count()
     })
     .OrderBy(x => x.Date)

     .ToList(); // 🔥 QUAN TRỌNG

                // 👉 format ở đây (C#)
                result = rawData.Select(x => new ChartItem
                {
                    Label = $"{x.Date}",
                    Tong = x.Tong,
                    SoBan=x.SoBan
                }).ToList();
            }
            else if (KieuDangChon == KieuThongKe.BayNgayQua)

            {
                var rawData = db.HoaDons
      .Where(x => x.TrangThai == 1 && x.NgayThanhToan.HasValue && x.NgayThanhToan.Value.Date >= DateTime.Today.AddDays(-7))
      .GroupBy(x => new
      {
          x.NgayThanhToan.Value.Date

      })
      .Select(g => new
      {
          g.Key.Date,

          Tong = g.Sum(x => x.TongTien), SoBan = g.Count()
      })
      .OrderBy(x => x.Date)

      .ToList(); // 🔥 QUAN TRỌNG

                // 👉 format ở đây (C#)
                result = rawData.Select(x => new ChartItem
                {
                    Label = x.Date.ToString("dd/MM/yyyy"),
                    Tong = x.Tong, SoBan = x.SoBan
                }).ToList();    
                
            }
            //Tháng này
            else if (KieuDangChon == KieuThongKe.ThangNay)
            {
                var rawData = db.HoaDons
     .Where(x => x.TrangThai == 1 && x.NgayThanhToan.HasValue && x.NgayThanhToan.Value.Month == DateTime.Today.Month && x.NgayThanhToan.Value.Year == DateTime.Today.Year)
     .GroupBy(x => new
     {
         x.NgayThanhToan.Value.Month,
         x.NgayThanhToan.Value.Year

     })
     .Select(g => new
     {
         g.Key.Month,
         g.Key.Year,
         Tong = g.Sum(x => x.TongTien), SoBan = g.Count()
     })
     .OrderBy(x => x.Month)
     .ThenBy(x => x.Year)
     .ToList(); // 🔥 QUAN TRỌNG

                // 👉 format ở đây (C#)
                result = rawData.Select(x => new ChartItem
                {
                     Label = $"{x.Month:00}/{x.Year}",
                     Tong = x.Tong,
                     SoBan = x.SoBan
                }).ToList();    
              
            }
            else // Tháng trước
            {
                var rawData = db.HoaDons
    .Where(x => x.TrangThai == 1 && x.NgayThanhToan.HasValue && x.NgayThanhToan.Value.Month == DateTime.Today.AddMonths(-1).Month && x.NgayThanhToan.Value.Year == DateTime.Today.AddMonths(-1).Year)
    .GroupBy(x => new
    {
        x.NgayThanhToan.Value.Month,
        x.NgayThanhToan.Value.Year

    })
    .Select(g => new
    {
        g.Key.Month,
        g.Key.Year,

        Tong = g.Sum(x => x.TongTien), SoBan = g.Count()
    })
    .OrderBy(x => x.Month)
    .ThenBy(x => x.Year)
    .ToList(); // 🔥 QUAN TRỌNG

                // 👉 format ở đây (C#)
                result = rawData.Select(x => new ChartItem
                { 
                    Label = $"{x.Month:00}/{x.Year}",
                    Tong = x.Tong,
                    SoBan = x.SoBan
                }).ToList();
            }

            // 👉 bind chart
            Labels = result.Select(x => x.Label).ToArray();

            Series = new SeriesCollection
{
    // 💰 DOANH THU
    new ColumnSeries
    {
        Title = "Doanh thu",
        Values = new ChartValues<decimal>(result.Select(x => x.Tong)),

        ScalesYAt = 0,

        DataLabels = true,
        LabelPoint = p => p.Y.ToString("N0") + " đ",

        MaxColumnWidth = 40,
        ColumnPadding = 10,

        Fill = System.Windows.Media.Brushes.SeaGreen
    },

    // 🪑 SỐ BÀN
    new LineSeries
    {
        Title = "Số bàn",
        Values = new ChartValues<int>(result.Select(x => x.SoBan)),

        ScalesYAt = 1,

        DataLabels = true,
        LabelPoint = p =>((int)p.Y).ToString()+" bàn",

        StrokeThickness = 3,
        PointGeometrySize = 10,

        Stroke = System.Windows.Media.Brushes.Orange,
        Fill = System.Windows.Media.Brushes.Transparent
    }
};

            OnPropertyChanged(nameof(Labels));
            OnPropertyChanged(nameof(Series));
        }
        public void Load()
        {
            using var db = new AppDbContext();

            var today = DateTime.Today;

            DoanhThuHomNay = db.HoaDons
                .Where(x => x.TrangThai == 1 && x.NgayThanhToan >= today)
                .Sum(x => (decimal?)x.TongTien) ?? 0;

            SoHoaDon = db.HoaDons
                .Count(x => x.TrangThai == 1 && x.NgayThanhToan >= today);

            SoBanDangDung = db.HoaDons
                .Count(x => x.TrangThai == 0 && x.Ngay.Date == today);

            OnPropertyChanged(nameof(DoanhThuHomNay));
            OnPropertyChanged(nameof(SoHoaDon));
            OnPropertyChanged(nameof(SoBanDangDung));
        }
        void LoadTop()
        {
            using var db = new AppDbContext();

            var data = db.ChiTietHoaDons
                .Where(x => x.HoaDon.TrangThai == 1)
                .GroupBy(x => x.SanPham)
                .Select(g => new TopSanPhamModel
                {
                    TenSP = g.Key.TenSP,
                    SoLuong = g.Sum(x => x.SoLuong),
                    DoanhThu = g.Sum(x => x.SoLuong * x.Gia),
                    HinhAnh = g.Key.HinhAnh
                })
                .OrderByDescending(x => x.SoLuong)
                .Take(5)
                .ToList();

            TopSanPham.Clear();
            foreach (var item in data)
                TopSanPham.Add(item);
        }
    }
}
