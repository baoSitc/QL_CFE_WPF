using BCrypt.Net;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using QL_CFE_WPF.Data;
using QL_CFE_WPF.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Windows;


namespace QL_CFE_WPF.ViewModels
{
    public partial class NhanVienViewModel : ObservableObject, IDataErrorInfo
    {
        public string Error => null;
        public ObservableCollection<NhanVien> DanhSach { get; set; } = new();
        private List<NhanVien> _cacheNhanVien = new();
        public List<Role> DanhSachRole { get; set; }

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
        public int roleId;

        [ObservableProperty]
        private bool trangThai = true;
        internal Action ClearPasswordRequested;
        [ObservableProperty]
        private bool isEditing=false;
        private bool isAddting=true; // true = đang sửa, false = đang thêm
        public bool IsValid =>
    !string.IsNullOrWhiteSpace(TenDangNhap) &&
    !string.IsNullOrWhiteSpace(TenHienThi) &&
    RoleId > 0 &&
    (!string.IsNullOrWhiteSpace(MatKhau));
        void UpdateIsValid()
        {
            OnPropertyChanged(nameof(IsValid));
        }
        partial void OnTenDangNhapChanged(string value) => UpdateIsValid();
        partial void OnMatKhauChanged(string value) => UpdateIsValid();
        partial void OnTenHienThiChanged(string value) => UpdateIsValid();
                partial void OnRoleIdChanged(int value) => UpdateIsValid();


        public string this[string columnName]
        {
            get
            {
                if (columnName == nameof(TenDangNhap))
                {
                    if (string.IsNullOrWhiteSpace(TenDangNhap))
                        return "Tên đăng nhập không được để trống.";
                    if (_cacheNhanVien.Any(x =>
                       x.TenDangNhap.ToLower() == TenDangNhap.ToLower()
                       && (!IsEditing || x.Id != SelectedItem?.Id)))
                        return "Tên đăng nhập đã tồn tại";
              
                }
                else if (columnName == nameof(MatKhau))
                {
                    if (!IsEditing && string.IsNullOrWhiteSpace(MatKhau))
                        return "Mật khẩu không được để trống.";
                }
                else if (columnName == nameof(TenHienThi))
                {
                    if (string.IsNullOrWhiteSpace(TenHienThi))
                        return "Tên hiển thị không được để trống.";
                }
                else if (columnName == nameof(VaiTro))
                {
                    if (string.IsNullOrWhiteSpace(VaiTro))
                        return "Vai trò không được để trống.";
                }
                return null;
            }
        }   
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

            foreach (var nv in db.NhanViens.Include(n => n.Role))
                DanhSach.Add(nv);
            _cacheNhanVien = DanhSach.ToList();
            DanhSachRole = db.Roles.ToList();
         
        }
        [RelayCommand]
        void Them()
        {
            using var db = new AppDbContext();
            //kiểm tra xem tên đăng nhập đã tồn tại trong cơ sở dữ liệu chưa
            if (db.NhanViens.Any(x => x.TenDangNhap.ToLower() == TenDangNhap.ToLower()))
            {
                MessageBox.Show("Tên đăng nhập đã tồn tại","Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var nv = new NhanVien
            {
                TenDangNhap = TenDangNhap.ToLower(),
                MatKhau = BCrypt.Net.BCrypt.HashPassword(MatKhau),
                TenHienThi = TenHienThi,
                RoleId=RoleId,
                TrangThai = TrangThai
            };

            db.NhanViens.Add(nv);
            db.SaveChanges();

            DanhSach.Add(nv);
            ClearForm();
            Load();
        }

        [RelayCommand]
        void Sua()
        {
            if (SelectedItem == null)
            {
                MessageBox.Show("Chưa chọn nhân viên", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using var db = new AppDbContext();

            var nv = db.NhanViens.Find(SelectedItem.Id);
            if (nv == null) return;

            nv.TenDangNhap = TenDangNhap;
            // Nếu người dùng đã nhập mật khẩu mới thì cập nhật, ngược lại giữ nguyên mật khẩu cũ
            if (!string.IsNullOrWhiteSpace(MatKhau))
            {
                nv.MatKhau = BCrypt.Net.BCrypt.HashPassword(MatKhau);
            }
            nv.TenHienThi = TenHienThi;
            nv.RoleId = RoleId;
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
            RoleId = value.RoleId;
            TrangThai = value.TrangThai;
            MatKhau = null; // không hiển thị mật khẩu cũ khi chọn nhân viên để sửa
            IsEditing = true;
            //cập nhậ lại IsValid khi đổi SelectedItem để cập nhật trạng thái của nút Lưu/Sửa
           UpdateIsValid();

        }

        void ClearForm()
        {
            TenDangNhap = "";
            MatKhau = null;
            TenHienThi = "";
            RoleId = 0;
            TrangThai = true;
            ClearPasswordRequested?.Invoke();
            IsEditing = false;
        }
    }
}
