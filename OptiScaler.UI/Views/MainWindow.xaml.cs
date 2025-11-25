using System;
using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using OptiScaler.Core;
using WinRT.Interop;

#nullable enable

namespace OptiScaler.UI.Views;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        this.InitializeComponent();
        Title = AppInfo.FullTitle;
        
        // Set window icon
        SetWindowIcon();
        
        // Customize title bar
        CustomizeTitleBar();
        
        // Set window size
        this.AppWindow.Resize(new Windows.Graphics.SizeInt32(1400, 900));
        
        // Navigate to default page
        NavigateToPage(typeof(GamesPage), "games");
        
        // Check for review prompt after a delay
        _ = CheckForReviewPromptAsync();
    }

    private void SetWindowIcon()
    {
        try
        {
            var hWnd = WindowNative.GetWindowHandle(this);
            var windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
            var appWindow = AppWindow.GetFromWindowId(windowId);

            if (appWindow != null)
            {
                // Try to set icon - works for packaged apps
                try
                {
                    appWindow.SetIcon("Assets/Square44x44Logo.png");
                }
                catch
                {
                    // For unpackaged apps (debug), use Win32 API
                    try
                    {
                        var iconPath = System.IO.Path.Combine(AppContext.BaseDirectory, "Assets", "Square44x44Logo.png");
                        if (System.IO.File.Exists(iconPath))
                        {
                            appWindow.SetIcon(iconPath);
                        }
                    }
                    catch
                    {
                        // Icon setting failed - not critical, continue
                    }
                }
            }
        }
        catch
        {
            // Silently fail if icon can't be set
        }
    }

    private void CustomizeTitleBar()
    {
        // Get the AppWindow for this window
        var hWnd = WindowNative.GetWindowHandle(this);
        var windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
        var appWindow = AppWindow.GetFromWindowId(windowId);

        if (appWindow != null)
        {
            // Customize title bar colors to match our dark theme
            var titleBar = appWindow.TitleBar;
            
            // Set title bar to be custom
            titleBar.ExtendsContentIntoTitleBar = false;
            
            // Dark theme colors that match our background (#0A0A0A)
            var darkBackgroundColor = Windows.UI.Color.FromArgb(255, 10, 10, 10);      // #0A0A0A
            var lightTextColor = Windows.UI.Color.FromArgb(255, 255, 255, 255);        // White
            var inactiveTextColor = Windows.UI.Color.FromArgb(255, 153, 153, 153);     // Gray
            
            // Title bar background
            titleBar.BackgroundColor = darkBackgroundColor;
            titleBar.InactiveBackgroundColor = darkBackgroundColor;
            
            // Title bar text
            titleBar.ForegroundColor = lightTextColor;
            titleBar.InactiveForegroundColor = inactiveTextColor;
            
            // Title bar buttons (minimize, maximize, close)
            titleBar.ButtonBackgroundColor = darkBackgroundColor;
            titleBar.ButtonInactiveBackgroundColor = darkBackgroundColor;
            
            titleBar.ButtonForegroundColor = lightTextColor;
            titleBar.ButtonInactiveForegroundColor = inactiveTextColor;
            
            // Title bar buttons on hover
            titleBar.ButtonHoverBackgroundColor = Windows.UI.Color.FromArgb(255, 32, 32, 32);  // Slightly lighter
            titleBar.ButtonHoverForegroundColor = lightTextColor;
            
            // Title bar buttons when pressed
            titleBar.ButtonPressedBackgroundColor = Windows.UI.Color.FromArgb(255, 16, 16, 16); // Darker
            titleBar.ButtonPressedForegroundColor = lightTextColor;
        }
    }

    private async Task CheckForReviewPromptAsync()
    {
        await Task.Delay(3000);
        
        var app = Application.Current as App;
        if (app?.ReviewPromptService == null) return;

        if (app.ReviewPromptService.ShouldShowReviewPrompt())
        {
            DispatcherQueue.TryEnqueue(async () =>
            {
                await app.ReviewPromptService.ShowReviewPromptAsync(Content.XamlRoot);
            });
        }
    }

    private void NavigationView_SelectionChanged(
        NavigationView sender,
        NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItemContainer != null)
        {
            var tag = args.SelectedItemContainer.Tag?.ToString();
            switch (tag)
            {
                case "games":
                    NavigateToPage(typeof(GamesPage), tag);
                    break;
                case "mod-manager":
                    NavigateToPage(typeof(ModsPage), tag);
                    break;
                case "mod-config":
                    NavigateToPage(typeof(ModConfigPage), tag);
                    break;
                case "app-settings":
                    NavigateToPage(typeof(AppSettingsPage), tag);
                    break;
            }
        }
    }

    private void NavigateToPage(Type pageType, string tag = "")
    {
        ContentFrame.Navigate(pageType, null, new EntranceNavigationTransitionInfo());
        
        // Select the corresponding navigation item
        if (!string.IsNullOrEmpty(tag))
        {
            foreach (var item in NavView.MenuItems)
            {
                if (item is NavigationViewItem navItem && navItem.Tag?.ToString() == tag)
                {
                    NavView.SelectedItem = navItem;
                    break;
                }
            }
        }
    }

    public void ShowNotification(string title, string message, InfoBarSeverity severity = InfoBarSeverity.Informational)
    {
        NotificationBar.Title = title;
        NotificationBar.Message = message;
        NotificationBar.Severity = severity;
        NotificationBar.IsOpen = true;
    }

    public void SetBusy(bool isBusy, string? statusMessage = null)
    {
        // Can be used to show loading state in notifications
        if (isBusy && !string.IsNullOrEmpty(statusMessage))
        {
            ShowNotification("Working", statusMessage, InfoBarSeverity.Informational);
        }
    }
}
