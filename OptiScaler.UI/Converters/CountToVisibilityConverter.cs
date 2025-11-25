using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace OptiScaler.UI.Converters;

public class CountToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        try
        {
            if (value is int count)
            {
                return count > 5 ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        catch { }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return 0;
    }
}
