using System;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using OptiScaler.Core.Models;
using OptiScaler.Core.Services;
using OptiScaler.Core.Contracts;

#nullable enable

namespace OptiScaler.UI.ViewModels;

/// <summary>
/// ViewModel for the Games page
/// </summary>
public partial class GamesViewModel : ObservableObject
{
    private readonly GameScannerService _scanner;
    private readonly DispatcherQueue? _dispatcherQueue;
    
    [ObservableProperty]
    private ObservableCollection<GameInfo> _games = new();

    [ObservableProperty]
    private GameInfo? _selectedGame;

    [ObservableProperty]
    private bool _isScanning;

    [ObservableProperty]
    private string _statusMessage = "No games loaded. Click 'Scan Games' to detect installed games.";

    [ObservableProperty]
    private double _scanProgress;

    [ObservableProperty]
    private string _searchText = string.Empty;

    public bool HasNoGames => Games.Count == 0;

    public GamesViewModel()
    {
        _scanner = new GameScannerService();
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        
        // Subscribe to scanner events
        _scanner.GameDiscovered += OnGameDiscovered;
        _scanner.ScanProgress += OnScanProgress;
    }

    [RelayCommand]
    private async Task ScanGamesAsync()
    {
        if (IsScanning) return;

        try
        {
            IsScanning = true;
            Games.Clear();
            StatusMessage = "Scanning for games...";
            ScanProgress = 0;

            var games = await _scanner.ScanAllPlatformsAsync();
            
            foreach (var game in games)
            {
                if (!Games.Contains(game))
                {
                    Games.Add(game);
                }
            }

            StatusMessage = $"Found {Games.Count} games";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsScanning = false;
            ScanProgress = 100;
        }
    }

    [RelayCommand]
    private async Task RefreshModStatusAsync(GameInfo? game)
    {
        if (game == null) return;

        try
        {
            await _scanner.RefreshModStatusAsync(game);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error refreshing {game.Name}: {ex.Message}";
        }
    }

    [RelayCommand]
    private void SelectGame(GameInfo game)
    {
        SelectedGame = game;
    }

    [RelayCommand]
    private void Refresh()
    {
        // Refresh current games list mod status
        foreach (var game in Games)
        {
            _ = RefreshModStatusAsync(game);
        }
    }

    [RelayCommand]
    private async Task LaunchGameAsync(GameInfo? game)
    {
        if (game == null) return;

        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = game.Executable,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to launch {game.Name}: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task InstallModAsync(GameInfo? game)
    {
        if (game == null) return;

        try
        {
            StatusMessage = $"Installing OptiScaler to {game.Name}...";
            // TODO: Implement actual mod installation via ModInstallerService
            await Task.Delay(2000); // Placeholder
            StatusMessage = $"OptiScaler installed to {game.Name}";
            await RefreshModStatusAsync(game);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to install mod: {ex.Message}";
        }
    }

    partial void OnSearchTextChanged(string value)
    {
        // Implement filtering logic
        // This will be expanded later
    }

    private void OnGameDiscovered(object? sender, GameInfo game)
    {
        // Add game to collection (must be on UI thread)
        _dispatcherQueue?.TryEnqueue(() =>
        {
            Games.Add(game);
            StatusMessage = $"Found {Games.Count} games...";
        });
    }

    private void OnScanProgress(object? sender, ScanProgressEventArgs e)
    {
        _dispatcherQueue?.TryEnqueue(() =>
        {
            ScanProgress = e.ProgressPercentage;
            StatusMessage = e.StatusMessage;
        });
    }
}
