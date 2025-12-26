using System;
using System.Globalization;
using System.Windows.Data;

namespace Mirage.UI.Converters;

public class MinutesToDurationConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int minutes && minutes > 0)
        {
            TimeSpan ts = TimeSpan.FromMinutes(minutes);
            // Format: "2d 5h" or "5h 30m"
            if (ts.TotalDays >= 1)
                return $"{ts.Days}d {ts.Hours}h";

            return $"{ts.Hours}h {ts.Minutes}m";
        }
        return "-";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}