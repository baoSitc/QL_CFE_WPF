using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using QL_CFE_WPF.Data;
using QL_CFE_WPF.Models;
using QL_CFE_WPF.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace QL_CFE_WPF.ViewModels
{
    public partial class HoaDonViewModel : ObservableObject
    {
        public ObservableCollection<HoaDon> HoaDons { get; set; }
            = new();

        public ObservableCollection<ChiTietHoaDon> ChiTiets { get; set; }
            = new();
        public class TrangThaiItem
        {
            public int? Value { get; set; }
            public string Name { get; set; }
        }
        public ObservableCollection<TrangThaiItem> TrangThaiFilters { get; set; }

        private TrangThaiItem _selectedTrangThai;
        public TrangThaiItem SelectedTrangThai
        {
            get => _selectedTrangThai;
            set
            {
                SetProperty(ref _selectedTrangThai, value);

                LoadHoaDon();
            }
        }


        [ObservableProperty]
        private HoaDon selectedHoaDon;

        [ObservableProperty]
        private DateTime tuNgay = DateTime.Today;

        [ObservableProperty]
        private DateTime denNgay = DateTime.Today;

        [ObservableProperty]
        private string soHoaDon;

        public HoaDonViewModel()
        {
            TrangThaiFilters = new ObservableCollection<TrangThaiItem>
            {
                new() { Value = null, Name = "Tất cả" },
                new() { Value = 0, Name = "Đang phục vụ" },
                new() { Value = 1, Name = "Đã thanh toán" },
                 new() { Value = 7, Name = "Treo qua ngày" },
                 new() { Value = 8, Name = "Còn nợ" },
                new() { Value = 9, Name = "Đã hủy" }
            };
            SelectedTrangThai = TrangThaiFilters[0]; // mặc định Đã thanh toán hoặc [0] nếu muốn tất cả
            LoadHoaDon();
        }

        public void LoadHoaDon()
        {
            using var db = new AppDbContext();

            var query = db.HoaDons
                .Include(x => x.NhanVien)
                .Include(x => x.Ban)
                .AsQueryable();


            query = query.Where(x =>
                x.Ngay >= TuNgay.Date &&
                x.Ngay < DenNgay.Date.AddDays(1));

            if (!string.IsNullOrWhiteSpace(SoHoaDon))
            {
                query = query.Where(x =>
                    x.SoHoaDon.Contains(SoHoaDon));
            }
            if (SelectedTrangThai?.Value != null)
            {
                query = query.Where(x =>
                    x.TrangThai == SelectedTrangThai.Value);
            }


            HoaDons = new ObservableCollection<HoaDon>(
                query.OrderByDescending(x => x.MaHD)
                     .ToList());

            OnPropertyChanged(nameof(HoaDons));
        }
        partial void OnSelectedHoaDonChanged(HoaDon value)
        {
            if (value == null)
                return;

            using var db = new AppDbContext();

            ChiTiets = new ObservableCollection<ChiTietHoaDon>(
                db.ChiTietHoaDons
                .Include(x => x.SanPham)
                .Where(x => x.MaHD == value.MaHD)
                .ToList());

            OnPropertyChanged(nameof(ChiTiets));
        }
        [RelayCommand]
        void InLai()
        {
            if (SelectedHoaDon == null)
                return;

            using var db = new AppDbContext();

            var hd = db.HoaDons
                .Include(x => x.ChiTietHoaDons)
                    .ThenInclude(x => x.SanPham)
                .First(x => x.MaHD == SelectedHoaDon.MaHD);

            hd.LanIn++;

            db.SaveChanges();

            InBill(hd);
        }
        void InBill(HoaDon hd)
        {
            pdfBillService pdf = new pdfBillService();
            pdf.ExportPdf(hd);
            return;

        }
        [RelayCommand]
        void HuyHoaDon()
        {
            if (SelectedHoaDon == null)
                return;

            if (MessageBox.Show(
                "Hủy hóa đơn?",
                "Xác nhận",
                MessageBoxButton.YesNo)
                != MessageBoxResult.Yes)
                return;

            using var db = new AppDbContext();

            var hd = db.HoaDons
                .First(x => x.MaHD == SelectedHoaDon.MaHD);

            hd.TrangThai = 9;

            db.SaveChanges();

            LoadHoaDon();
        }
        [RelayCommand]
        void TimKiem()
        {
            LoadHoaDon();
        }
    }
    }
