using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore.Diagnostics;
using QL_CFE_WPF.Models;
using QL_CFE_WPF.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;

namespace QL_CFE_WPF.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        //[ObservableProperty]
        
        //private ObservableCollection<SanPham> sanPhams;
        //public MainViewModel()
        //{
        //    LoadData();
        //}
        //void LoadData()
        //{

        //    using var db = new Data.AppDbContext();
        //    SanPhams = new ObservableCollection<SanPham>(db.SanPhams.OrderBy(x=>x.TenSP).ToList());

        //}
        //[RelayCommand]
        //void AddSanPham()
        //{
        //    var newSanPham = new SanPham
        //    {
        //        TenSP = TenSP,
        //        Gia = Gia,
        //        NhomHangId = SelectedNhomHang?.Id ?? 0,
        //    };
        //    using var db = new Data.AppDbContext();
        //    db.SanPhams.Add(newSanPham);
        //    db.SaveChanges();
        //    SanPhams.Add(newSanPham);
        //}
        //[RelayCommand]
        //void RemoveSanPham(SanPham sp)
        //{
        //    if (sp == null) return;
        //    using var db = new Data.AppDbContext();
        //    db.SanPhams.Remove(sp);
        //    db.SaveChanges();
        //    SanPhams.Remove(sp);
        //}
        //[ObservableProperty]
        //private string tenSP;
        //[ObservableProperty]
        //private decimal gia;
        //[ObservableProperty]
        //private SanPham selectedSanPham;
        //[RelayCommand]
        //void UpdateSanPham()
        //{
        //    if (SelectedSanPham == null) return;
        //    using var db = new Data.AppDbContext();
        //    var sp = db.SanPhams.Find(SelectedSanPham.MaSP);
        //    if (sp != null)
        //    {
        //        sp.TenSP = TenSP;
        //        sp.Gia = Gia;
        //        db.SaveChanges();
        //        LoadData();
        //    }


        //}
        //partial void OnSelectedSanPhamChanged(SanPham value)
        //{
        //    if (value != null)
        //    {
        //        TenSP = value.TenSP;
        //        Gia = value.Gia;
        //    }
        //}
        [ObservableProperty]
        private object currentView;
        [RelayCommand]
        void ShowSanPham()
        {
            CurrentView = new SanPhamView();
        }
        [RelayCommand]
        void ShowPos()
        {
            CurrentView = new Views.PosView();
        }
        [RelayCommand]
        void MoBaoCaoDoanhThu()
        {
            CurrentView = new Views.BaoCaoDoanhThu();
        }

        [RelayCommand]
        void MoBaoCaoHangHoa()
        {
            CurrentView = new BaoCaoHangHoa();
        }
        [RelayCommand]
        void ShowDashboard()
        {
            CurrentView = new DashboardView
            {
                DataContext = new DashboardViewModel()
            };
        }
        [RelayCommand]
        void ShowNhanVien()
        {
            if (!PermissionService.Has("MANAGE_USER"))
            {
                MessageBox.Show("Không có quyền");
                return;
            }
            CurrentView = new NhanVienView();
        }
        [RelayCommand]
        void Quit()
        {
            Application.Current.Shutdown();
        }
    }
}
