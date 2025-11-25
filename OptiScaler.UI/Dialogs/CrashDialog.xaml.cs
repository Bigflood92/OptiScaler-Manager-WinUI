using System;
using System.Diagnostics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Text;
using OptiScaler.Core.Services;

namespace OptiScaler.UI.Dialogs;

/// <summary>
/// Displays crash information to the user with options to restart or view log
/// </summary>
public static class CrashDialog
{
    public static async void ShowCrashDialog(Window parentWindow, Exception exception, string crashLogPath)
    {
        try
        {
            var message = BuildCrashMessage(exception, crashLogPath);

            var dialog = new ContentDialog
            {
                Title = "Application Error",
                Content = new ScrollViewer
                {
                    Content = new StackPanel
                    {
                        Spacing = 16,
                        Children =
                        {
                            new StackPanel
                            {
                                Spacing = 12,
                                HorizontalAlignment = HorizontalAlignment.Center,
                                Children =
                                {
                                    new FontIcon
                                    {
                                        Glyph = "\uE783",
                                        FontSize = 48,
                                        Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                                            Windows.UI.Color.FromArgb(255, 239, 68, 68))
                                    },
                                    new TextBlock
                                    {
                                        Text = "OptiScaler Manager has encountered an error",
                                        FontSize = 18,
                                        FontWeight = FontWeights.SemiBold,
                                        TextAlignment = TextAlignment.Center,
                                        TextWrapping = TextWrapping.Wrap
                                    }
                                }
                            },
                            new TextBlock
                            {
                                Text = message,
                                TextWrapping = TextWrapping.Wrap,
                                FontFamily = new Microsoft.UI.Xaml.Media.FontFamily("Consolas"),
                                FontSize = 12,
                                MaxHeight = 300
                            },
                            new TextBlock
                            {
                                Text = "A crash report has been saved. You can:",
                                FontWeight = FontWeights.SemiBold,
                                Margin = new Thickness(0, 8, 0, 0)
                            },
                            new TextBlock
                            {
                                Text = "• Restart the app to try again\n• View the crash log for technical details\n• Close and check for updates in Microsoft Store",
                                TextWrapping = TextWrapping.Wrap,
                                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                                    Windows.UI.Color.FromArgb(255, 204, 204, 204))
                            }
                        }
                    },
                    MaxHeight = 500
                },
                PrimaryButtonText = "Restart App",
                SecondaryButtonText = "View Log",
                CloseButtonText = "Close",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = parentWindow.Content.XamlRoot
            };

            var result = await dialog.ShowAsync();

            switch (result)
            {
                case ContentDialogResult.Primary:
                    RestartApplication();
                    break;

                case ContentDialogResult.Secondary:
                    OpenCrashLog(crashLogPath);
                    break;

                default:
                    Application.Current.Exit();
                    break;
            }
        }
        catch
        {
            Application.Current.Exit();
        }
    }

    private static string BuildCrashMessage(Exception exception, string crashLogPath)
    {
        var message = $"Error: {exception.Message}";
        
        if (exception.InnerException != null)
        {
            message += $"\n\nCause: {exception.InnerException.Message}";
        }

        if (!string.IsNullOrEmpty(crashLogPath))
        {
            message += $"\n\nLog saved to:\n{crashLogPath}";
        }

        return message;
    }

    private static void RestartApplication()
    {
        try
        {
            var exePath = Environment.ProcessPath;
            if (!string.IsNullOrEmpty(exePath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = exePath,
                    UseShellExecute = true
                });
            }
        }
        catch
        {
            // Ignore restart errors
        }
        finally
        {
            Application.Current.Exit();
        }
    }

    private static void OpenCrashLog(string crashLogPath)
    {
        try
        {
            if (!string.IsNullOrEmpty(crashLogPath) && System.IO.File.Exists(crashLogPath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "notepad.exe",
                    Arguments = $"\"{crashLogPath}\"",
                    UseShellExecute = true
                });
            }
        }
        catch
        {
            // Ignore if can't open
        }
        finally
        {
            Application.Current.Exit();
        }
    }
}
