using QL_CFE_WPF.Data;
using QL_CFE_WPF.Models;
using QL_CFE_WPF.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace QL_CFE_WPF
{
    /// <summary>
    /// Interaction logic for SanPhamView.xaml
    /// </summary>
    public partial class SanPhamView : UserControl
    {
        public SanPhamView()
        {
            InitializeComponent();
            DataContext = new SanPhamViewModel();
        }
        private void TreeView_SelectedItemChanged(
    object sender,
    RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is SanPhamViewModel vm
                && e.NewValue is NhomHangTreeVM nhom)
            {
                vm.SelectedNhomHang=new NhomHang
                {
                    Id = nhom.Id,
                    TenNhom = nhom.TenNhom,
                    ParentId = nhom.ParentId
                };
                vm.SelectedNhomHangId = nhom.Id;
              
                vm.LoadSanPhamTheoNhom();
            }
        }
       
    }
}
