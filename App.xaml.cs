using MaterialDesignThemes.Wpf;
using OfficeOpenXml;
using System.Configuration;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Windows;

namespace QL_CFE_WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var loginWindow = new Views.LoginWindow();
            bool? dialogResult = loginWindow.ShowDialog();// Hiển thị cửa sổ đăng nhập và chờ kết quả
            if (dialogResult == true)
            {
                var mainWindow = new MainWindow();
                mainWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                mainWindow.Show();
            }
            else
            {
                // Nếu người dùng đóng cửa sổ đăng nhập hoặc đăng nhập không thành công, thoát ứng dụng
                Shutdown();
            }
        }
    }
}
   
