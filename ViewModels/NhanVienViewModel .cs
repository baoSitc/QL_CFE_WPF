using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QL_CFE_WPF.Data;
using QL_CFE_WPF.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using BCrypt.Net;
using System.Windows;

namespace QL_CFE_WPF.ViewModels
{
    public partial class NhanVienViewModel : ObservableObject
    {
        public ObservableCollection<NhanVien> DanhSach { get; set; } = new();

        [ObservableProperty]
        private NhanVien selectedItem;

        [ObservableProperty]
        private string tenDangNhap;

        [ObservableProperty]
        private string matKhau;

        [ObservableProperty]
        private string tenHienThi;

        [ObservableProperty]
        private string vaiTro;

        [ObservableProperty]
        private bool trangThai = true;
        internal Action ClearPasswordRequested;

        public List<string> DanhSachVaiTro { get; set; } = new()
    {
        "Admin", "ThuNgan", "NhanVien"
    };

        public NhanVienViewModel()
        {
            Load();
        }

        void Load()
        {
            using var db = new AppDbContext();
            DanhSach.Clear();

            foreach (var nv in db.NhanViens)
                DanhSach.Add(nv);
        }

        [RelayCommand]
        void Them()
        {
            using var db = new AppDbContext();

            var nv = new NhanVien
            {
                TenDangNhap = TenDangNhap,
                MatKhau = BCrypt.Net.BCrypt.HashPassword(MatKhau),
                TenHienThi = TenHienThi,
                VaiTro = VaiTro,
                TrangThai = TrangThai
            };

            db.NhanViens.Add(nv);
            db.SaveChanges();

            DanhSach.Add(nv);
            ClearForm();
        }

        [RelayCommand]
        void Sua()
        {
            if (SelectedItem == null) return;

            using var db = new AppDbContext();

            var nv = db.NhanViens.Find(SelectedItem.Id);
            if (nv == null) return;

            nv.TenDangNhap = TenDangNhap;
            nv.MatKhau = BCrypt.Net.BCrypt.HashPassword(MatKhau);
            nv.TenHienThi = TenHienThi;
            nv.VaiTro = VaiTro;
            nv.TrangThai = TrangThai;
            db.SaveChanges();
            MessageBox.Show("Đã cập nhật dữ liệu", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            ClearForm();
            Load();

        }

        [RelayCommand]
        void Xoa()
        {
            if (SelectedItem == null) return;

            using var db = new AppDbContext();

            var nv = db.NhanViens.Find(SelectedItem.Id);
            if (nv == null) return;

            db.NhanViens.Remove(nv);
            db.SaveChanges();

            DanhSach.Remove(SelectedItem);
            ClearForm();
        }

        partial void OnSelectedItemChanged(NhanVien value)
        {
            if (value == null) return;

            TenDangNhap = value.TenDangNhap;
            //MatKhau = value.MatKhau;
            TenHienThi = value.TenHienThi;
            VaiTro = value.VaiTro;
            TrangThai = value.TrangThai;
            MatKhau = "";
        }

        void ClearForm()
        {
            TenDangNhap = "";
            MatKhau = "";
            TenHienThi = "";
            VaiTro = null;
            TrangThai = true;
            ClearPasswordRequested?.Invoke();
        }
    }
}
