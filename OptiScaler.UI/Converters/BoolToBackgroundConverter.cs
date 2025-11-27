using System;
using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace OptiScaler.UI.Converters;

/// <summary>
/// Converts boolean values to appropriate background colors
/// </summary>
public class BoolToBackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool boolValue)
        {
            return boolValue
                ? new SolidColorBrush(Color.FromArgb(255, 34, 197, 94))  // Green for installed
                : new SolidColorBrush(Color.FromArgb(255, 75, 85, 99));  // Gray for not installed
        }
        
        return new SolidColorBrush(Color.FromArgb(255, 75, 85, 99));
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}