using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Mirage.UI.Converters;

public class ActionToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // Normalize the action string to handle case sensitivity
        string action = value?.ToString()?.ToUpper() ?? "";

        return action switch
        {
            "CREATE" => Brushes.Green,      // New data added
            "UPDATE" => Brushes.Orange,     // Existing data changed
            "DEACTIVATE" => Brushes.Red,    // Item was soft-deleted
            _ => Brushes.Gray               // Default for any other actions
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}