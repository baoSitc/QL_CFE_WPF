using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace QL_CFE_WPF.Views
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            var vm = new ViewModels.LoginViewModel();
            DataContext = vm;
            vm.LoginSucceeded += async () =>
            {
                var window = Window.GetWindow(this);

                // 🔥 animation fade out
                DoubleAnimation fadeOut = new DoubleAnimation
                {
                    From = 1,
                    To = 0,
                    Duration = TimeSpan.FromMilliseconds(300)
                };

                window.BeginAnimation(Window.OpacityProperty, fadeOut);

                await Task.Delay(600);

                window.DialogResult = true;
            };
        }
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.LoginViewModel vm)
            {
                vm.Password = txtPassword.Password;
            }
        }
    }
}
