using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace OptiScaler.UI.Converters;

/// <summary>
/// Converts string values to Visibility (Visible if not null/empty, Collapsed otherwise)
/// </summary>
public class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return !string.IsNullOrEmpty(value as string) 
            ? Visibility.Visible 
            : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}