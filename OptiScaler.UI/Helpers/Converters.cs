using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using OptiScaler.Core.Models;

namespace OptiScaler.UI.Helpers;

/// <summary>
/// Converts boolean to color brush (green for true, red for false)
/// </summary>
public class BoolToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool boolValue)
        {
            return boolValue
                ? new SolidColorBrush(Colors.LightGreen)
                : new SolidColorBrush(Colors.Gray);
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts null to boolean
/// </summary>
public class NullToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value != null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts DateTime to formatted string
/// </summary>
public class DateTimeToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is DateTime dateTime)
        {
            var timeAgo = DateTime.Now - dateTime;
            if (timeAgo.TotalMinutes < 1)
                return "Just now";
            if (timeAgo.TotalHours < 1)
                return $"{(int)timeAgo.TotalMinutes}m ago";
            if (timeAgo.TotalDays < 1)
                return $"{(int)timeAgo.TotalHours}h ago";
            if (timeAgo.TotalDays < 7)
                return $"{(int)timeAgo.TotalDays}d ago";
            
            return dateTime.ToString("MMM dd, yyyy");
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts collection count to visibility (collapsed if count is 0)
/// </summary>
public class CountToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is int count)
        {
            return count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts boolean to visibility
/// </summary>
public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool boolValue)
        {
            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is Visibility visibility)
        {
            return visibility == Visibility.Visible;
        }
        return false;
    }
}

/// <summary>
/// Inverts boolean to visibility (true = Collapsed, false = Visible)
/// </summary>
public class InverseBoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool boolValue)
        {
            return boolValue ? Visibility.Collapsed : Visibility.Visible;
        }
        return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts GamePlatform to display string
/// </summary>
public class GamePlatformToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is GamePlatform platform)
        {
            return platform switch
            {
                GamePlatform.Steam => "Steam",
                GamePlatform.Epic => "Epic Games",
                GamePlatform.Xbox => "Xbox",
                GamePlatform.GOG => "GOG",
                GamePlatform.EA => "EA",
                GamePlatform.Ubisoft => "Ubisoft",
                GamePlatform.Manual => "Manual",
                _ => "Unknown"
            };
        }
        return "Unknown";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts GamePlatform to icon glyph
/// </summary>
public class GamePlatformToIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is GamePlatform platform)
        {
            return platform switch
            {
                GamePlatform.Steam => "\uE90B",      // Library icon
                GamePlatform.Epic => "\uE7FC",       // Shop icon
                GamePlatform.Xbox => "\uE990",       // Xbox controller
                GamePlatform.GOG => "\uE895",        // Shop bag
                GamePlatform.EA => "\uE7F4",         // Globe
                GamePlatform.Ubisoft => "\uE7EE",    // Globe variant
                GamePlatform.Manual => "\uE8B7",     // Folder
                _ => "\uE7C3"                        // Question mark
            };
        }
        return "\uE7C3";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts mod installation status to text
/// </summary>
public class ModStatusToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool hasOptiScaler)
        {
            return hasOptiScaler ? "Installed" : "Not Installed";
        }
        return "Unknown";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
