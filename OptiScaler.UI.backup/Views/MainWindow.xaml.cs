using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using OptiScaler.Core;

#nullable enable

namespace OptiScaler.UI.Views;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        this.InitializeComponent();
        Title = AppInfo.FullTitle;
        
        // Navigate to default page
        NavigateToPage(typeof(GamesPage));
    }

    private void NavigationView_SelectionChanged(
        NavigationView sender,
        NavigationViewSelectionChangedEventArgs args)
    {
        if (args.IsSettingsSelected)
        {
            NavigateToPage(typeof(SettingsPage));
        }
        else if (args.SelectedItemContainer != null)
        {
            var tag = args.SelectedItemContainer.Tag?.ToString();
            switch (tag)
            {
                case "games":
                    NavigateToPage(typeof(GamesPage));
                    break;
                case "mods":
                    NavigateToPage(typeof(ModsPage));
                    break;
                case "scan":
                    NavigateToPage(typeof(ScanPage));
                    break;
            }
        }
    }

    private void NavigateToPage(Type pageType)
    {
        ContentFrame.Navigate(pageType);
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
