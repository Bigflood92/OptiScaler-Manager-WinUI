using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using OptiScaler.UI.ViewModels;
using OptiScaler.Core.Models;
using OptiScaler.UI.Dialogs;
using OptiScaler.UI.Services;

namespace OptiScaler.UI.Views;

public sealed partial class GamesPage : Page
{
    public GamesViewModel ViewModel { get; }

    public GamesPage()
    {
        this.InitializeComponent();
        ViewModel = new GamesViewModel();
        this.DataContext = ViewModel;
        System.Diagnostics.Debug.WriteLine("[GamesPage] Page initialized successfully");
        
        // Setup navigation (gamepad, keyboard, click-to-focus)
        this.Loaded += OnPageLoaded;
    }

    private void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        // Setup complete navigation support
        InputNavigationService.SetupPageNavigation(
            this,
            onRefresh: async () => await ViewModel.ScanGamesCommand.ExecuteAsync(null),
            onSettings: null
        );
        
        System.Diagnostics.Debug.WriteLine("[GamesPage] Navigation setup complete - gamepad, keyboard, and click-to-focus enabled");
    }

    private async void ScanGames_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.ScanGamesCommand.ExecuteAsync(null);
        if (ViewModel.Games.Any() && sender is Button button)
        {
            var stackPanel = button.Content as StackPanel;
            if (stackPanel != null)
            {
                foreach (var child in stackPanel.Children)
                {
                    if (child is TextBlock tb) tb.Text = "Refresh";
                    else if (child is FontIcon icon) icon.Glyph = "\uE72C";
                }
            }
        }
    }

    private async void InstallMod_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is Button button && button.Tag is GameInfo game)
            {
                await ViewModel.InstallModCommand.ExecuteAsync(game);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[GamesPage] InstallMod_Click error: {ex}");
        }
    }

    private async void UninstallMod_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is Button button && button.Tag is GameInfo game)
            {
                await ViewModel.UninstallModCommand.ExecuteAsync(game);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[GamesPage] UninstallMod_Click error: {ex}");
        }
    }

    private async void Launch_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is Button button && button.Tag is GameInfo game)
            {
                await ViewModel.LaunchGameCommand.ExecuteAsync(game);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[GamesPage] Launch_Click error: {ex}");
        }
    }

    private void ShowDetails_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is Button button && button.Tag is GameInfo game)
            {
                ViewModel.ShowGameDetailsCommand.Execute(game);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[GamesPage] ShowDetails_Click error: {ex}");
        }
    }

    private async void ShowSettings_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is Button button && button.Tag is GameInfo game)
            {
                // Open game configuration dialog
                var dialog = new GameConfigDialog
                {
                    Game = game,
                    XamlRoot = this.XamlRoot
                };

                var result = await dialog.ShowAsync();

                // If changes were made, refresh the game list
                if (dialog.HasChanges)
                {
                    await ViewModel.RefreshGameAsync(game);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[GamesPage] ShowSettings_Click error: {ex}");
        }
    }
}
