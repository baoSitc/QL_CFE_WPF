using QL_CFE_WPF.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace QL_CFE_WPF.Views
{
    /// <summary>
    /// Interaction logic for NhanVienView.xaml
    /// </summary>
    public partial class NhanVienView : UserControl
    {
        public event Action ClearPasswordRequested;
        public NhanVienView()
        {
            InitializeComponent();
            var vm=new NhanVienViewModel();
            DataContext = vm;
            vm.ClearPasswordRequested += () => {               
                txtPassword.Password = string.Empty;
            };

        }
        private void txtPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is NhanVienViewModel vm)
            {
                vm.MatKhau = txtPassword.Password;
            }
        }
    }
}
