using QL_CFE_WPF.Models;
using QL_CFE_WPF.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for BaoCaoHangHoa.xaml
    /// </summary>
    public partial class BaoCaoHangHoa : UserControl
    {
        public BaoCaoHangHoa()
        {
            InitializeComponent();
            Loaded += BaoCaoHangHoa_Loaded;
            DataContextChanged += BaoCaoHangHoa_DataContextChanged;
        }

        private void BaoCaoHangHoa_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            GanSuKIenVM();
        }
       

        private void GanSuKIenVM()
        {
            if (DataContext is BaoCaoHangHoaViewModel vm) {
                vm.PropertyChanged -= Vm_PropertyChanged;
                vm.PropertyChanged += Vm_PropertyChanged;
                UpdateColumnVisibility();
            }
        }


        private void Vm_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(BaoCaoHangHoaViewModel.KieuDangChon))
            {
                UpdateColumnVisibility();
            }
        }

        private void BaoCaoHangHoa_Loaded(object sender, RoutedEventArgs e)
        {
            GanSuKIenVM();
        }

        private void UpdateColumnVisibility()
        {
            if (DataContext is not BaoCaoHangHoaViewModel vm) return;

            dgBaoCao.Columns[4].Visibility =
                vm.KieuDangChon == KieuBaoCao.ChiTiet
                ? Visibility.Visible
                : Visibility.Collapsed;

            dgBaoCao.Columns[5].Visibility =
                vm.KieuDangChon == KieuBaoCao.ChiTiet
                ? Visibility.Visible
                : Visibility.Collapsed;
        }
        
    }
}
