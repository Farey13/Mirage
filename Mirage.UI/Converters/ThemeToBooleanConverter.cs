using System;
using System.Globalization;
using System.Windows.Data;

namespace Mirage.UI.Converters;

public class ThemeToBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // If the theme name is "Dark", the switch is "On" (true)
        return value is string themeName && themeName == "Dark";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // If the switch is "On" (true), the theme name is "Dark", otherwise it's "Light"
        return value is bool isChecked && isChecked ? "Dark" : "Light";
    }
}