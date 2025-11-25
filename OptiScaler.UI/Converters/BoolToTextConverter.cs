using System;
using Microsoft.UI.Xaml.Data;

namespace OptiScaler.UI.Converters;

public class BoolToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var isTrue = value is bool b && b;
        var texts = (parameter as string ?? "True|False").Split('|');
        
        if (texts.Length != 2)
            return value?.ToString() ?? string.Empty;

        return isTrue ? texts[0] : texts[1];
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
