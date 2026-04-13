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
    /// Interaction logic for BaoCaoDoanhThu.xaml
    /// </summary>
    public partial class BaoCaoDoanhThu : UserControl
    {
        public BaoCaoDoanhThu()
        {
            InitializeComponent();
            DataContext = new ViewModels.BaoCaoViewModel();
        }
    }
}
