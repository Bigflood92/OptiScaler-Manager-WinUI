using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace OptiScaler.UI.Converters;

public class BoolToVisibilityConverter : IValueConverter
{
    // parameter: "invert" to invert logic
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var invert = (parameter as string)?.Equals("invert", StringComparison.OrdinalIgnoreCase) ?? false;
        var flag = value is bool b && b;
        if (invert) flag = !flag;
        return flag ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is Visibility v)
            return v == Visibility.Visible;
        return false;
    }
}