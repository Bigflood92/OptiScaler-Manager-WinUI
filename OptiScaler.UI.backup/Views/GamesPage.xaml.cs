using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using OptiScaler.UI.ViewModels;
using OptiScaler.Core.Models;

namespace OptiScaler.UI.Views;

public sealed partial class GamesPage : Page
{
    public GamesViewModel ViewModel { get; }

    public GamesPage()
    {
        this.InitializeComponent();
        ViewModel = new GamesViewModel();
    }

    private async void ScanGames_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.ScanGamesCommand.ExecuteAsync(null);
    }

    private void Refresh_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.RefreshCommand.Execute(null);
    }

    private async void InstallMod_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is GameInfo game)
        {
            await ViewModel.InstallModCommand.ExecuteAsync(game);
        }
    }

    private async void Launch_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is GameInfo game)
        {
            await ViewModel.LaunchGameCommand.ExecuteAsync(game);
        }
    }
}
