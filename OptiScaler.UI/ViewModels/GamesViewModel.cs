using System;
using System.Linq;
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
    
    // Full collection of all games (unfiltered)
    private readonly ObservableCollection<GameInfo> _allGames = new();
    
    [ObservableProperty]
    private ObservableCollection<GameInfo> _games = new();

    [ObservableProperty]
    private GameInfo? _selectedGame;

    [ObservableProperty]
    private bool _isScanning;

    [ObservableProperty]
    private bool _isInstallingMod;

    [ObservableProperty]
    private string _statusMessage = "No games loaded. Click 'Scan Games' to detect installed games.";

    [ObservableProperty]
    private double _scanProgress;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _hasNoGames = true;

    public GamesViewModel()
    {
        _scanner = new GameScannerService();
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        
        // Subscribe to Games collection changes
        Games.CollectionChanged += (s, e) => 
        {
            HasNoGames = Games.Count == 0;
        };
        
        // Subscribe to scanner events
        _scanner.GameDiscovered += OnGameDiscovered;
        _scanner.ScanProgress += OnScanProgress;

        // Load sample data for testing
        LoadSampleData();
    }

    /// <summary>
    /// Loads sample game data for UI testing
    /// </summary>
    private void LoadSampleData()
    {
        // Sample games with OptiScaler installed
        _allGames.Add(new GameInfo
        {
            Name = "Cyberpunk 2077",
            Path = @"C:\Program Files (x86)\Steam\steamapps\common\Cyberpunk 2077",
            Executable = @"C:\Program Files (x86)\Steam\steamapps\common\Cyberpunk 2077\bin\x64\Cyberpunk2077.exe",
            Platform = GamePlatform.Steam,
            HasOptiScaler = true,
            HasDlssgToFsr3 = false,
            LastScanned = DateTime.Now.AddMinutes(-5)
        });

        _allGames.Add(new GameInfo
        {
            Name = "Red Dead Redemption 2",
            Path = @"C:\Program Files\Epic Games\RedDeadRedemption2",
            Executable = @"C:\Program Files\Epic Games\RedDeadRedemption2\RDR2.exe",
            Platform = GamePlatform.Epic,
            HasOptiScaler = true,
            HasDlssgToFsr3 = true,
            LastScanned = DateTime.Now.AddHours(-2)
        });

        _allGames.Add(new GameInfo
        {
            Name = "Starfield",
            Path = @"C:\Xbox Games\Starfield\Content",
            Executable = @"C:\Xbox Games\Starfield\Content\Starfield.exe",
            Platform = GamePlatform.Xbox,
            HasOptiScaler = false,
            HasDlssgToFsr3 = false,
            LastScanned = DateTime.Now.AddDays(-1)
        });

        // Sample games without OptiScaler
        _allGames.Add(new GameInfo
        {
            Name = "Baldur's Gate 3",
            Path = @"C:\Program Files (x86)\Steam\steamapps\common\Baldurs Gate 3",
            Executable = @"C:\Program Files (x86)\Steam\steamapps\common\Baldurs Gate 3\bin\bg3.exe",
            Platform = GamePlatform.Steam,
            HasOptiScaler = false,
            HasDlssgToFsr3 = false,
            LastScanned = DateTime.Now.AddMinutes(-30)
        });

        _allGames.Add(new GameInfo
        {
            Name = "Hogwarts Legacy",
            Path = @"C:\Program Files (x86)\Steam\steamapps\common\Hogwarts Legacy",
            Executable = @"C:\Program Files (x86)\Steam\steamapps\common\Hogwarts Legacy\HogwartsLegacy.exe",
            Platform = GamePlatform.Steam,
            HasOptiScaler = false,
            HasDlssgToFsr3 = false,
            LastScanned = DateTime.Now.AddHours(-5)
        });

        _allGames.Add(new GameInfo
        {
            Name = "Forza Horizon 5",
            Path = @"C:\Program Files (x86)\Steam\steamapps\common\FH5",
            Executable = @"C:\Program Files (x86)\Steam\steamapps\common\FH5\ForzaHorizon5.exe",
            Platform = GamePlatform.Steam,
            HasOptiScaler = true,
            HasDlssgToFsr3 = false,
            LastScanned = DateTime.Now.AddMinutes(-15)
        });

        _allGames.Add(new GameInfo
        {
            Name = "The Witcher 3",
            Path = @"C:\GOG Games\The Witcher 3",
            Executable = @"C:\GOG Games\The Witcher 3\bin\x64\witcher3.exe",
            Platform = GamePlatform.GOG,
            HasOptiScaler = true,
            HasDlssgToFsr3 = false,
            LastScanned = DateTime.Now.AddDays(-2)
        });

        _allGames.Add(new GameInfo
        {
            Name = "Assassin's Creed Mirage",
            Path = @"C:\Program Files\Ubisoft\Assassin's Creed Mirage",
            Executable = @"C:\Program Files\Ubisoft\Assassin's Creed Mirage\ACMirage.exe",
            Platform = GamePlatform.Ubisoft,
            HasOptiScaler = false,
            HasDlssgToFsr3 = false,
            LastScanned = DateTime.Now.AddHours(-8)
        });

        _allGames.Add(new GameInfo
        {
            Name = "Star Wars Jedi: Survivor",
            Path = @"C:\Program Files\EA Games\Jedi Survivor",
            Executable = @"C:\Program Files\EA Games\Jedi Survivor\JediSurvivor.exe",
            Platform = GamePlatform.EA,
            HasOptiScaler = false,
            HasDlssgToFsr3 = false,
            LastScanned = DateTime.Now.AddHours(-12)
        });

        _allGames.Add(new GameInfo
        {
            Name = "Alan Wake 2",
            Path = @"C:\Program Files\Epic Games\AlanWake2",
            Executable = @"C:\Program Files\Epic Games\AlanWake2\AlanWake2.exe",
            Platform = GamePlatform.Epic,
            HasOptiScaler = true,
            HasDlssgToFsr3 = true,
            LastScanned = DateTime.Now.AddMinutes(-45)
        });

        // Copy all games to visible collection
        foreach (var game in _allGames)
        {
            Games.Add(game);
        }

        StatusMessage = $"Loaded {Games.Count} sample games for testing";
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
        if (game == null || IsInstallingMod) return;

        try
        {
            IsInstallingMod = true;
            StatusMessage = $"?? Installing OptiScaler to {game.Name}...";
            
            // Simulate downloading
            await Task.Delay(800);
            StatusMessage = $"?? Downloading OptiScaler files for {game.Name}...";
            
            // Simulate extracting
            await Task.Delay(700);
            StatusMessage = $"?? Extracting files to {game.Name}...";
            
            // Simulate installing
            await Task.Delay(500);
            StatusMessage = $"?? Configuring OptiScaler for {game.Name}...";
            
            await Task.Delay(500);
            
            // Update game status
            game.HasOptiScaler = true;
            game.LastScanned = DateTime.Now;
            
            StatusMessage = $"? OptiScaler successfully installed to {game.Name}!";
            
            // Force UI update by refreshing the item
            var index = Games.IndexOf(game);
            if (index >= 0)
            {
                Games.RemoveAt(index);
                Games.Insert(index, game);
            }
            
            // Clear success message after 3 seconds
            await Task.Delay(3000);
            if (StatusMessage.StartsWith("?"))
            {
                StatusMessage = $"Showing {Games.Count} games";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"? Failed to install mod: {ex.Message}";
        }
        finally
        {
            IsInstallingMod = false;
        }
    }

    [RelayCommand]
    private async Task UninstallModAsync(GameInfo? game)
    {
        if (game == null || IsInstallingMod) return;

        try
        {
            IsInstallingMod = true;
            StatusMessage = $"?? Uninstalling OptiScaler from {game.Name}...";
            
            // Simulate removing files
            await Task.Delay(800);
            StatusMessage = $"??? Removing OptiScaler files from {game.Name}...";
            
            // Simulate restoring backup
            await Task.Delay(700);
            StatusMessage = $"?? Restoring original game files for {game.Name}...";
            
            await Task.Delay(500);
            
            // Update game status
            game.HasOptiScaler = false;
            game.HasDlssgToFsr3 = false;
            game.LastScanned = DateTime.Now;
            
            StatusMessage = $"? OptiScaler successfully uninstalled from {game.Name}!";
            
            // Force UI update by refreshing the item
            var index = Games.IndexOf(game);
            if (index >= 0)
            {
                Games.RemoveAt(index);
                Games.Insert(index, game);
            }
            
            // Clear success message after 3 seconds
            await Task.Delay(3000);
            if (StatusMessage.StartsWith("?"))
            {
                StatusMessage = $"Showing {Games.Count} games";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"? Failed to uninstall mod: {ex.Message}";
        }
        finally
        {
            IsInstallingMod = false;
        }
    }

    [RelayCommand]
    private void ShowGameDetails(GameInfo? game)
    {
        if (game == null) return;
        
        SelectedGame = game;
        StatusMessage = $"Showing details for {game.Name}";
        // TODO: Open details dialog/page
    }

    [RelayCommand]
    private void ShowGameSettings(GameInfo? game)
    {
        if (game == null) return;
        
        SelectedGame = game;
        StatusMessage = $"Opening settings for {game.Name}";
        // TODO: Open settings dialog/page for this specific game
    }

    partial void OnSearchTextChanged(string value)
    {
        FilterGames();
    }

    /// <summary>
    /// Filters games based on search text
    /// </summary>
    private void FilterGames()
    {
        Games.Clear();

        if (string.IsNullOrWhiteSpace(SearchText))
        {
            // No filter, show all games
            foreach (var game in _allGames)
            {
                Games.Add(game);
            }
            StatusMessage = $"Showing {Games.Count} games";
        }
        else
        {
            // Filter by game name or platform
            var searchLower = SearchText.ToLower();
            var filtered = _allGames.Where(g =>
                g.Name.ToLower().Contains(searchLower) ||
                g.Platform.ToString().ToLower().Contains(searchLower) ||
                g.Path.ToLower().Contains(searchLower)
            );

            foreach (var game in filtered)
            {
                Games.Add(game);
            }

            StatusMessage = $"Found {Games.Count} game(s) matching '{SearchText}'";
        }
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
