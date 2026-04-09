using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace QL_CFE_WPF.Views
{
    public class PathToImageConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string path = value as string;
            if (!string.IsNullOrEmpty(path))
            {
                try
                {
                    return new System.Windows.Media.Imaging.BitmapImage(new Uri($"pack://siteoforigin:,,,/{path}"));
                }
                catch
                {
                    // Handle exceptions (e.g., file not found) if necessary
                    return new BitmapImage(new Uri("pack://siteoforigin:,,,/Images/cf001.jpg"));
                }
            }
            return new BitmapImage(new Uri("pack://siteoforigin:,,,/Images/cf001.jpg"));
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    
    }
}
