using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using OptiScaler.UI.ViewModels;
using OptiScaler.Core.Models;
using OptiScaler.UI.Services;

namespace OptiScaler.UI.Views;

public sealed partial class ModsPage : Page
{
    public ModsViewModel ViewModel { get; }

    public ModsPage()
    {
        this.InitializeComponent();
        ViewModel = new ModsViewModel();
        
        // Setup navigation
        this.Loaded += OnPageLoaded;
    }

    private void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        // Setup complete navigation support
        InputNavigationService.SetupPageNavigation(
            this,
            onRefresh: async () => await ViewModel.CheckForUpdatesCommand.ExecuteAsync(null),
            onSettings: null
        );
    }

    private async void CheckForUpdates_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.CheckForUpdatesCommand.ExecuteAsync(null);
    }

    private async void DownloadOptiScaler_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is OptiScaler.UI.ViewModels.ModsViewModel.ReleaseItem item)
        {
            await ViewModel.DownloadReleaseCommand.ExecuteAsync(item);
        }
    }

    private async void DownloadOptiPatcher_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is OptiScaler.UI.ViewModels.ModsViewModel.ReleaseItem item)
        {
            await ViewModel.DownloadReleaseCommand.ExecuteAsync(item);
        }
    }

    private void DeleteDownloaded_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is OptiScaler.UI.ViewModels.ModsViewModel.ReleaseItem item)
        {
            ViewModel.DeleteDownloadedCommand.Execute(item);
        }
    }

    private void OpenDownloadsFolder_Click(object sender, RoutedEventArgs e)
    {
        var downloadsDir = System.IO.Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData),
            "OptiScaler Manager", "Mods");

        if (System.IO.Directory.Exists(downloadsDir))
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = downloadsDir,
                UseShellExecute = true,
                Verb = "open"
            });
        }
    }

    private void ToggleShowMore_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.ShowAllOptiScaler = !ViewModel.ShowAllOptiScaler;
    }

    private void ToggleShowMoreOptiPatcher_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.ShowAllOptiPatcher = !ViewModel.ShowAllOptiPatcher;
    }
}
