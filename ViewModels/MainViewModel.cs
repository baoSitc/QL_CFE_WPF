using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore.Diagnostics;
using QL_CFE_WPF.Data;
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
        private void CapNhatHoaDonTreo()
        {
            using var db = new AppDbContext();

            var dsTreo = db.HoaDons
      .Where(x => x.TrangThai == 0
               && x.Ngay < DateTime.Today)
      .ToList();

            foreach (var hd in dsTreo)
                hd.TrangThai = 7;

            if (dsTreo.Any())
                db.SaveChanges();
        }
        //Contructor
        public  MainViewModel()
        {
           CapNhatHoaDonTreo();
            CurrentView = new DashboardView();

        }

        [ObservableProperty]
        private object currentView;
        [RelayCommand]
        void SanPham()
        {
            CurrentView = new SanPhamView();
        }
        [RelayCommand]
        void Pos()
        {
            CurrentView = new Views.PosView();
        }
        [RelayCommand]
        void BaoCaoDoanhThu()
        {
            CurrentView = new Views.BaoCaoDoanhThu();
        }

        [RelayCommand]
        void BaoCaoHangHoa()
        {
            CurrentView = new BaoCaoHangHoa();
        }
        [RelayCommand]
        void Dashboard()
        {
            CurrentView = new DashboardView
            {
                DataContext = new DashboardViewModel()
            };
        }
        [RelayCommand]
        void NhanVien()
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
        [RelayCommand]
        void HoaDon()
        {
            CurrentView = new HoaDonView();
        }
        [RelayCommand]
        void Ban()
        {
            
           
            var win = new Views.BanDialog();           
            win.Show();
        }
    }
}
