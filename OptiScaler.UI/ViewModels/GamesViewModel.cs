using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
    private readonly GitHubService _githubService;
    private readonly ModInstallerService _modInstaller;
    private readonly StorageService _storage;
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
    private string _statusMessage = "Loading games...";

    [ObservableProperty]
    private double _scanProgress;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _hasNoGames = true;

    public GamesViewModel()
    {
        _scanner = new GameScannerService();
        _githubService = new GitHubService();
        _modInstaller = new ModInstallerService(_githubService);
        _storage = new StorageService();
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        
        // Subscribe to Games collection changes
        Games.CollectionChanged += (s, e) => 
        {
            HasNoGames = Games.Count == 0;
        };
        
        // Subscribe to scanner events
        _scanner.GameDiscovered += OnGameDiscovered;
        _scanner.ScanProgress += OnScanProgress;

        // Subscribe to GitHub download progress
        _githubService.DownloadProgress += OnDownloadProgress;

        // Subscribe to mod installer progress
        _modInstaller.InstallProgress += OnInstallProgress;

        // Load saved games on startup
        _ = LoadSavedGamesAsync();
    }

    private async Task LoadSavedGamesAsync()
    {
        try
        {
            var savedGames = await _storage.LoadGamesAsync();
            if (savedGames.Any())
            {
                // Use HashSet to avoid loading duplicates
                var uniqueGames = new HashSet<string>();
                
                _allGames.Clear();
                Games.Clear();
                
                foreach (var game in savedGames)
                {
                    // Use executable as unique key
                    if (!uniqueGames.Add(game.Executable))
                        continue; // Skip duplicate
                    
                    // FORCE REFRESH MOD STATUS ON LOAD to ensure accurate state
                    try
                    {
                        await _scanner.RefreshModStatusAsync(game);
                        Debug.WriteLine($"[GamesVM] Refreshed mod status for {game.Name}: OptiScaler={game.HasOptiScaler}, OptiPatcher={game.HasOptiPatcher}");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[GamesVM] Error refreshing mod status for {game.Name}: {ex.Message}");
                    }
                    
                    _allGames.Add(game);
                    Games.Add(game);
                }
                
                // Save the refreshed states back to disk
                await _storage.SaveGamesAsync(_allGames);
                
                StatusMessage = $"Loaded {Games.Count} saved games (mod status refreshed from disk)";
            }
            else
            {
                StatusMessage = "No saved games. Click 'Scan Games' to detect installed games.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading saved games: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task ScanGamesAsync()
    {
        if (IsScanning) return;

        try
        {
            IsScanning = true;
            _allGames.Clear();
            Games.Clear();
            StatusMessage = "Scanning for games...";
            ScanProgress = 0;

            var games = await _scanner.ScanAllPlatformsAsync();
            
            // Use a HashSet to avoid duplicates based on executable path
            var uniqueGames = new HashSet<string>();
            
            foreach (var game in games)
            {
                // User executable path as unique key
                if (!uniqueGames.Add(game.Executable))
                    continue; // Skip duplicate
                
                _allGames.Add(game);
                if (!Games.Contains(game))
                {
                    Games.Add(game);
                }
            }

            // Save scanned games to disk
            await _storage.SaveGamesAsync(_allGames);

            StatusMessage = Games.Any() 
                ? $"Found {Games.Count} games (saved)" 
                : "No games found. Try adding custom game paths in Settings.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error scanning: {ex.Message}";
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
        Debug.WriteLine($"[GamesVM] === InstallModAsync CALLED ===");
        Debug.WriteLine($"[GamesVM] Game: {game?.Name ?? "NULL"}");
        Debug.WriteLine($"[GamesVM] IsInstallingMod: {IsInstallingMod}");
        
        if (game == null || IsInstallingMod) 
        {
            Debug.WriteLine($"[GamesVM] Early return - conditions not met");
            return;
        }

        try
        {
            Debug.WriteLine($"[GamesVM] Starting installation for: {game.Name}");
            IsInstallingMod = true;
            
            // Check for downloaded mods
            var modsBaseDir = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "OptiScaler Manager", "Mods");

            var optiScalerDir = System.IO.Path.Combine(modsBaseDir, "OptiScaler");
            var optiPatcherDir = System.IO.Path.Combine(modsBaseDir, "OptiPatcher");

            // Check what mods are available
            string? optiScalerPath = null;
            string? optiPatcherPath = null;
            bool hasOptiPatcher = false;

            // === CHECK OPTISCALER ===
            StatusMessage = $"Checking available mods for {game.Name}...";
            
            // Look for OptiScaler (priority order: extracted dirs > archives)
            if (System.IO.Directory.Exists(optiScalerDir))
            {
                // Priority 1: Pre-extracted directories
                var extractedDirs = System.IO.Directory.GetDirectories(optiScalerDir, "*_extracted")
                    .OrderByDescending(d => System.IO.Directory.GetCreationTime(d))
                    .ToList();

                if (extractedDirs.Any())
                {
                    optiScalerPath = extractedDirs.First();
                    Debug.WriteLine($"[GamesVM] Found OptiScaler extracted: {optiScalerPath}");
                }
                else
                {
                    // Priority 2: Archive files
                    var downloadedFiles = System.IO.Directory.GetFiles(optiScalerDir, "*.*")
                        .Where(f => f.EndsWith(".7z", StringComparison.OrdinalIgnoreCase) || 
                                    f.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                        .OrderByDescending(f => System.IO.File.GetLastWriteTime(f))
                        .ToList();

                    if (downloadedFiles.Any())
                    {
                        var archivePath = downloadedFiles.First();
                        var extractedDirName = Path.GetFileNameWithoutExtension(archivePath) + "_extracted";
                        var extractedPath = Path.Combine(Path.GetDirectoryName(archivePath) ?? "", extractedDirName);
                        
                        optiScalerPath = Directory.Exists(extractedPath) ? extractedPath : archivePath;
                        Debug.WriteLine($"[GamesVM] Found OptiScaler archive: {optiScalerPath}");
                    }
                }
            }

            // === CHECK OPTIPATCHER ===
            if (System.IO.Directory.Exists(optiPatcherDir))
            {
                var patcherFiles = System.IO.Directory.GetFiles(optiPatcherDir, "*.asi")
                    .OrderByDescending(f => System.IO.File.GetLastWriteTime(f))
                    .ToList();

                if (patcherFiles.Any())
                {
                    optiPatcherPath = patcherFiles.First();
                    hasOptiPatcher = true;
                    Debug.WriteLine($"[GamesVM] Found OptiPatcher: {optiPatcherPath}");
                }
            }

            // === DOWNLOAD OPTISCALER IF NOT FOUND ===
            if (string.IsNullOrEmpty(optiScalerPath))
            {
                StatusMessage = $"Downloading OptiScaler for {game.Name}...";
                Debug.WriteLine($"[GamesVM] Downloading OptiScaler...");

                var downloadTask = Task.Run(async () =>
                {
                    return await _githubService.DownloadLatestModAsync(
                        ModType.OptiScaler, 
                        System.IO.Path.Combine(optiScalerDir, "OptiScaler_latest.7z"));
                });

                var downloadResult = await downloadTask;

                if (downloadResult == null)
                {
                    StatusMessage = $"Failed to download OptiScaler";
                    Debug.WriteLine($"[GamesVM] OptiScaler download failed");
                    return;
                }

                optiScalerPath = downloadResult.Value.FilePath;
                Debug.WriteLine($"[GamesVM] OptiScaler downloaded: {optiScalerPath}");
            }

            // === INSTALL MODS ===
            var installationTasks = new List<(string name, Task<ModOperationResult> task)>();

            // Always install OptiScaler
            StatusMessage = $"Installing OptiScaler to {game.Name}...";

            // Load global settings for DLL name
            var globalSettings = await new GlobalSettingsService().LoadSettingsAsync();
            var dllName = globalSettings.PreferredDllName;

            var optiScalerTask = Task.Run(async () =>
            {
                return await _modInstaller.InstallModAsync(game, ModType.OptiScaler, optiScalerPath, dllName);
            });
            installationTasks.Add(("OptiScaler", optiScalerTask));

            // Install OptiPatcher if available
            if (hasOptiPatcher && !string.IsNullOrEmpty(optiPatcherPath))
            {
                StatusMessage = $"Installing OptiScaler + OptiPatcher (AMD FidelityFX Frame Generation) to {game.Name}...";
                var optiPatcherTask = Task.Run(async () =>
                {
                    return await _modInstaller.InstallModAsync(game, ModType.OptiPatcher, optiPatcherPath);
                });
                installationTasks.Add(("OptiPatcher", optiPatcherTask));
                Debug.WriteLine($"[GamesVM] Installing both OptiScaler and OptiPatcher (AMD FidelityFX Frame Generation enabled)");
            }
            else
            {
                Debug.WriteLine($"[GamesVM] Installing OptiScaler only (OptiPatcher not available - no AMD FidelityFX Frame Generation)");
            }

            // === WAIT FOR INSTALLATIONS TO COMPLETE ===
            var results = new List<(string name, ModOperationResult result)>();
            foreach (var (name, task) in installationTasks)
            {
                var result = await task;
                results.Add((name, result));
                Debug.WriteLine($"[GamesVM] {name} installation result: {result.Success}");
            }

            // === CHECK RESULTS ===
            var successfulInstalls = results.Where(r => r.result.Success).ToList();
            var failedInstalls = results.Where(r => !r.result.Success).ToList();

            if (successfulInstalls.Any())
            {
                // Update game status
                game.HasOptiScaler = successfulInstalls.Any(r => r.name == "OptiScaler");
                game.HasOptiPatcher = successfulInstalls.Any(r => r.name == "OptiPatcher");
                game.LastScanned = DateTime.Now;

                // Create enhanced success message with Frame Generation info
                var installedMods = string.Join(" + ", successfulInstalls.Select(r => r.name));
                var frameGenInfo = game.HasOptiPatcher ? " (AMD FidelityFX Frame Generation Ready)" : " (Upscaling Only)";
                StatusMessage = $"{installedMods} successfully installed to {game.Name}{frameGenInfo}!";
                Debug.WriteLine($"[GamesVM] Installation successful: {installedMods}");

                // Save updated games list
                await _storage.SaveGamesAsync(_allGames);

                // Force UI update
                var index = Games.IndexOf(game);
                if (index >= 0)
                {
                    Games.RemoveAt(index);
                    Games.Insert(index, game);
                }

                // Clear success message after 3 seconds
                await Task.Delay(3000);
                if (StatusMessage.Contains("successfully"))
                {
                    StatusMessage = $"Showing {Games.Count} games";
                }
            }
            else
            {
                // All installations failed
                var errorMessages = string.Join("; ", failedInstalls.Select(r => $"{r.name}: {r.result.ErrorMessage}"));
                StatusMessage = $"Installation failed: {errorMessages}";
                Debug.WriteLine($"[GamesVM] All installations failed: {errorMessages}");
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to install mods: {ex.Message}";
            Debug.WriteLine($"[GamesVM] Install error: {ex}");
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
            StatusMessage = $"Uninstalling mods from {game.Name}...";

            var uninstallTasks = new List<(string name, Task<ModOperationResult> task)>();

            // Uninstall OptiScaler if present
            if (game.HasOptiScaler)
            {
                var optiScalerTask = Task.Run(async () =>
                {
                    return await _modInstaller.UninstallModAsync(game, ModType.OptiScaler);
                });
                uninstallTasks.Add(("OptiScaler", optiScalerTask));
            }

            // Uninstall OptiPatcher if present
            if (game.HasOptiPatcher)
            {
                var optiPatcherTask = Task.Run(async () =>
                {
                    return await _modInstaller.UninstallModAsync(game, ModType.OptiPatcher);
                });
                uninstallTasks.Add(("OptiPatcher", optiPatcherTask));
            }

            if (uninstallTasks.Any())
            {
                var results = new List<(string name, ModOperationResult result)>();
                foreach (var (name, task) in uninstallTasks)
                {
                    var result = await task;
                    results.Add((name, result));
                }

                var successfulUninstalls = results.Where(r => r.result.Success).ToList();

                if (successfulUninstalls.Any())
                {
                    // Update game status to FALSE immediately
                    game.HasOptiScaler = false;
                    game.HasOptiPatcher = false;
                    game.HasFrameGeneration = false; // Reset frame generation flag
                    game.LastScanned = DateTime.Now;

                    // FORCE REAL MOD STATUS CHECK from disk
                    await _scanner.RefreshModStatusAsync(game);

                    var uninstalledMods = string.Join(" + ", successfulUninstalls.Select(r => r.name));
                    StatusMessage = $"{uninstalledMods} successfully uninstalled from {game.Name}!";

                    // Save updated games list to disk immediately  
                    await _storage.SaveGamesAsync(_allGames);

                    // Force UI update
                    var index = Games.IndexOf(game);
                    if (index >= 0)
                    {
                        Games.RemoveAt(index);
                        Games.Insert(index, game);
                    }

                    // Clear success message after 3 seconds
                    await Task.Delay(3000);
                    if (StatusMessage.Contains("successfully"))
                    {
                        StatusMessage = $"Showing {Games.Count} games";
                    }
                }
                else
                {
                    var errorMessages = string.Join("; ", results.Where(r => !r.result.Success).Select(r => $"{r.name}: {r.result.ErrorMessage}"));
                    StatusMessage = $"Failed to uninstall: {errorMessages}";
                }
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to uninstall mods: {ex.Message}";
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
        
        // Create comprehensive details message
        var details = new List<string>();
        details.Add($"Game Details for {game.Name}");
        details.Add($"Path: {game.Path}");
        details.Add($"Executable: {Path.GetFileName(game.Executable)}");
        details.Add($"Platform: {game.Platform}");
        details.Add($"Last Scanned: {game.LastScanned:g}");
        
        if (game.HasOptiScaler || game.HasOptiPatcher)
        {
            details.Add($"Mods Installed:");
            if (game.HasOptiScaler) details.Add($"  - OptiScaler ({game.UpscalingMethod ?? "Auto"} @ {game.QualityPreset ?? "Quality"})");
            if (game.HasOptiPatcher) details.Add($"  - OptiPatcher (AMD FidelityFX Frame Generation)");
            if (game.HasFrameGeneration) details.Add($"  - Frame Generation: ACTIVE");
        }
        else
        {
            details.Add($"No mods installed");
        }
        
        var detailsMessage = string.Join(" | ", details);
        StatusMessage = detailsMessage;
        
        Debug.WriteLine($"[GamesVM] Game Details:\n{string.Join("\n", details)}");
        
        // Clear detailed message after 10 seconds
        _ = Task.Run(async () =>
        {
            await Task.Delay(10000);
            if (_dispatcherQueue != null)
            {
                _dispatcherQueue.TryEnqueue(() =>
                {
                    if (StatusMessage == detailsMessage)
                    {
                        StatusMessage = $"Showing {Games.Count} games";
                    }
                });
            }
        });
    }

    [RelayCommand]
    private async Task ShowGameSettingsAsync(GameInfo? game)
    {
        if (game == null) return;
        
        try
        {
            Debug.WriteLine($"[GamesVM] ===== ShowGameSettings CALLED =====");
            Debug.WriteLine($"[GamesVM] Game: {game.Name}");
            Debug.WriteLine($"[GamesVM] HasOptiScaler: {game.HasOptiScaler}");
            Debug.WriteLine($"[GamesVM] HasOptiPatcher: {game.HasOptiPatcher}");
            
            SelectedGame = game;
            
            // Create detailed configuration info message with proper emojis
            var configInfo = new List<string>();
            configInfo.Add($"{game.Name}");
            configInfo.Add($"Platform: {game.Platform}");
            configInfo.Add($"Install Dir: {game.InstallDirectory ?? game.Path}");
            configInfo.Add($"Executable: {game.Executable}");
            
            if (game.HasOptiScaler)
            {
                configInfo.Add($"OptiScaler: Installed");
                configInfo.Add($"Upscaler: {game.UpscalingMethod ?? "Auto"}");
                configInfo.Add($"Quality: {game.QualityPreset ?? "Quality"}");
                
                // Enhanced Frame Generation detection
                var installDir = game.InstallDirectory ?? game.Path;
                var configPath = Path.Combine(installDir, "OptiScaler.ini");
                
                bool frameGenEnabled = game.HasFrameGeneration;
                if (File.Exists(configPath))
                {
                    try
                    {
                        var config = new OptiScalerConfigService().ReadConfig(configPath);
                        frameGenEnabled = config.IsFrameGenerationEnabled();
                        
                        // Update game object with real frame generation status
                        game.HasFrameGeneration = frameGenEnabled;
                        
                        configInfo.Add($"Config File: Found");
                        configInfo.Add($"Primary Upscaler: {config.GetPrimaryUpscaler()}");
                        configInfo.Add($"Overlay Menu: {(config.OverlayMenu ? "Enabled" : "Disabled")}");
                        configInfo.Add($"FPS Counter: {(config.ShowFps ? "Enabled" : "Disabled")}");
                        configInfo.Add($"GPU Spoofing: {(config.DxgiSpoofing ? "Enabled" : "Disabled")}");
                        
                        // Detailed Frame Generation information
                        if (frameGenEnabled)
                        {
                            configInfo.Add($"AMD FidelityFX Frame Gen: ACTIVE (enabled in OptiScaler.ini)");
                        }
                        else
                        {
                            configInfo.Add($"AMD FidelityFX Frame Gen: DISABLED (set to 'nofg' in OptiScaler.ini)");
                        }
                    }
                    catch (Exception ex)
                    {
                        configInfo.Add($"Config Error: {ex.Message}");
                    }
                }
                else
                {
                    configInfo.Add($"Config File: Not Found");
                    configInfo.Add($"Frame Gen: Unknown (no config file)");
                }
            }
            else
            {
                configInfo.Add($"OptiScaler: Not Installed");
            }
            
            if (game.HasOptiPatcher)
            {
                configInfo.Add($"OptiPatcher: Installed (AMD FidelityFX Frame Generation Support)");
                if (game.HasOptiScaler && game.HasFrameGeneration)
                {
                    configInfo.Add($"COMBO: OptiScaler + OptiPatcher = Maximum Performance with Frame Gen!");
                }
            }
            else
            {
                configInfo.Add($"OptiPatcher: Not Installed");
            }

            var message = string.Join(" | ", configInfo.Take(8));
            StatusMessage = $"{message}";
            
            // Log detailed configuration in debug
            var fullConfig = string.Join("\n", configInfo);
            Debug.WriteLine($"[GamesVM] Game Configuration:\n{fullConfig}");
            
            // Clear detailed message after 8 seconds (longer for more info)
            await Task.Delay(8000);
            if (StatusMessage.Contains(game.Name))
            {
                StatusMessage = $"Showing {Games.Count} games";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"? Error accessing settings for {game.Name}: {ex.Message}";
            Debug.WriteLine($"[GamesVM] ShowGameSettings error: {ex}");
        }
    }

    /// <summary>
    /// Refresh a specific game's status and update the UI
    /// </summary>
    public async Task RefreshGameAsync(GameInfo game)
    {
        try
        {
            Debug.WriteLine($"[GamesVM] Refreshing game: {game.Name}");
            
            // Refresh mod status
            await _scanner.RefreshModStatusAsync(game);
            
            // Save updated games list
            await _storage.SaveGamesAsync(_allGames);
            
            // Force UI update
            var index = Games.IndexOf(game);
            if (index >= 0)
            {
                Games.RemoveAt(index);
                Games.Insert(index, game);
            }
            
            StatusMessage = $"{game.Name} refreshed";
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[GamesVM] Error refreshing game: {ex.Message}");
        }
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
            StatusMessage = Games.Count > 0 
                ? $"Showing {Games.Count} games" 
                : "No games loaded. Click 'Scan Games' to detect installed games.";
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

            StatusMessage = Games.Count > 0
                ? $"Found {Games.Count} game(s) matching '{SearchText}'"
                : $"No games found matching '{SearchText}'";
        }
    }

    private void OnGameDiscovered(object? sender, GameInfo game)
    {
        // Add game to collection (must be on UI thread)
        _dispatcherQueue?.TryEnqueue(() =>
        {
            _allGames.Add(game);
            
            // Only add to filtered collection if it matches current search
            if (string.IsNullOrWhiteSpace(SearchText) ||
                game.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                game.Platform.ToString().Contains(SearchText, StringComparison.OrdinalIgnoreCase))
            {
                Games.Add(game);
            }
            
            StatusMessage = $"Found {_allGames.Count} games...";
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

    private void OnDownloadProgress(object? sender, DownloadProgressEventArgs e)
    {
        _dispatcherQueue?.TryEnqueue(() =>
        {
            var progress = (e.BytesReceived / (double)e.TotalBytes) * 100;
            StatusMessage = $"Downloading {e.FileName}: {progress:F1}% ({e.FormattedSpeed})";
        });
    }

    private void OnInstallProgress(object? sender, InstallProgressEventArgs e)
    {
        _dispatcherQueue?.TryEnqueue(() =>
        {
            var progress = (e.CurrentStep / (double)e.TotalSteps) * 100;
            StatusMessage = $"{e.CurrentAction} ({progress:F0}%)";
        });
    }
}
