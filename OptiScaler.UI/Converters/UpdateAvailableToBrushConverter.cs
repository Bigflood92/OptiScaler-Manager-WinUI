using System;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace OptiScaler.UI.Converters;

public class UpdateAvailableToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var isUpdateAvailable = value is bool b && b;
        
        if (isUpdateAvailable)
        {
            // White/light color for "New update available"
            return new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 255, 255));
        }
        else
        {
            // Green for "already have latest"
            return new SolidColorBrush(Windows.UI.Color.FromArgb(255, 76, 175, 80)); // #4CAF50
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
