using System;
using Microsoft.UI.Xaml.Data;

namespace OptiScaler.UI.Converters;

public class NullToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var textIfNull = parameter as string ?? string.Empty;
        if (value is string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return textIfNull;
            try
            {
                // If it's a path, show just the file name
                return System.IO.Path.GetFileName(s);
            }
            catch
            {
                return s;
            }
        }
        return textIfNull;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return value;
    }
}
