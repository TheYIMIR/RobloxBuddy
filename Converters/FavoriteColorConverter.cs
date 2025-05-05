using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Color = System.Windows.Media.Color;

namespace RobloxBuddy.Converters
{
    public class FavoriteColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isFavorite = (bool)value;
            return isFavorite ? new SolidColorBrush(Color.FromRgb(255, 188, 0)) :
                               new SolidColorBrush(Color.FromRgb(128, 128, 128));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}