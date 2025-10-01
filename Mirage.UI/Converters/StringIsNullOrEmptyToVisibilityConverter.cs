using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Mirage.UI.Converters;

public class StringIsNullOrEmptyToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var s = value as string;
        bool isInverted = parameter as string == "Inverse";

        if (isInverted)
        {
            return string.IsNullOrEmpty(s) ? Visibility.Collapsed : Visibility.Visible;
        }
        return string.IsNullOrEmpty(s) ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}