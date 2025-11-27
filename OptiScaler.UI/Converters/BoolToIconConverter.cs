using System;
using Microsoft.UI.Xaml.Data;

namespace OptiScaler.UI.Converters;

/// <summary>
/// Converts boolean values to appropriate icons (checkmark or X)
/// </summary>
public class BoolToIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool boolValue)
        {
            return boolValue
                ? "\uE73E"  // Checkmark icon
                : "\uE711"; // X icon
        }
        
        return "\uE711"; // Default to X
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}