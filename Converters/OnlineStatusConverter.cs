using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Color = System.Windows.Media.Color;

namespace RobloxBuddy.Converters
{
    public class OnlineStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isOnline = (bool)value;
            return isOnline ? new SolidColorBrush(Color.FromRgb(2, 183, 87)) :
                             new SolidColorBrush(Color.FromRgb(128, 128, 128));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}