using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QL_CFE_WPF.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace QL_CFE_WPF.ViewModels
{
    public partial class LoginViewModel:ObservableObject
    {
        private string _username;
        private string _password;
        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }
        public bool IsLoginSuccessful { get; private set; }
         public event Action LoginSucceeded;
        [RelayCommand]
        private void Login()
        {
            if(string.IsNullOrEmpty(_username) || string.IsNullOrEmpty(_password)) {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin đăng nhập.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            using var db = new Data.AppDbContext();
            var user = db.NhanViens.FirstOrDefault(nv => nv.TenDangNhap == _username);
            if (user != null && BCrypt.Net.BCrypt.Verify(_password, user.MatKhau))
            {
                IsLoginSuccessful = true;
                                Session.CurrentUser = user;
                Session.Permissions = db.RolePermissions
                    .Where(x => x.RoleId == user.RoleId)
                    .Select(x => x.Permission.MaQuyen)
                    .ToList();  
                LoginSucceeded?.Invoke();
            }
            else
            {
                IsLoginSuccessful = false;
                MessageBox.Show("Tên đăng nhập hoặc mật khẩu không đúng.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
