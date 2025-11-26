using System.Diagnostics;
using System.Runtime.Versioning;
using Microsoft.Win32;
using OptiScaler.Core.Contracts;
using OptiScaler.Core.Models;

namespace OptiScaler.Core.Services;

/// <summary>
/// Service for scanning and detecting game installations across multiple platforms
/// </summary>
public class GameScannerService : IGameScannerService
{
    public event EventHandler<GameInfo>? GameDiscovered;
    public event EventHandler<ScanProgressEventArgs>? ScanProgress;

    private static readonly string[] GameExecutableExtensions = { ".exe" };
    private static readonly string[] CommonGameFolders = { "bin", "Binaries", "Game", "" };

    /// <summary>
    /// Scan all supported gaming platforms for installed games
    /// </summary>
    [SupportedOSPlatform("windows")]
    public async Task<IEnumerable<GameInfo>> ScanAllPlatformsAsync(CancellationToken cancellationToken = default)
    {
        var allGames = new List<GameInfo>();
        
        // Load settings to check which platforms are enabled (force reload to respect recent changes)
        var globalSettings = await new GlobalSettingsService().LoadSettingsAsync(forceReload: true);
        
        // Build list of enabled platforms
        var enabledPlatforms = new List<GamePlatform>();
        
        if (globalSettings.ScanSteam) enabledPlatforms.Add(GamePlatform.Steam);
        if (globalSettings.ScanEpic) enabledPlatforms.Add(GamePlatform.Epic);
        if (globalSettings.ScanXbox) enabledPlatforms.Add(GamePlatform.Xbox);
        if (globalSettings.ScanGOG) enabledPlatforms.Add(GamePlatform.GOG);
        if (globalSettings.ScanEA) enabledPlatforms.Add(GamePlatform.EA);
        if (globalSettings.ScanUbisoft) enabledPlatforms.Add(GamePlatform.Ubisoft);
        
        // Add custom game paths as Manual platform
        if (globalSettings.CustomGamePaths.Any())
        {
            foreach (var customPath in globalSettings.CustomGamePaths)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;
                    
                var game = await VerifyGamePathAsync(customPath);
                if (game != null)
                {
                    allGames.Add(game);
                    GameDiscovered?.Invoke(this, game);
                }
            }
        }
        
        if (!enabledPlatforms.Any())
        {
            ReportProgress(0, 0, "No platforms enabled for scanning", 0);
            return allGames;
        }
        
        int completed = 0;
        int total = enabledPlatforms.Count;

        ReportProgress(total, completed, "Starting scan...", 0);

        foreach (var platform in enabledPlatforms)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            ReportProgress(total, completed, $"Scanning {platform}...", allGames.Count);

            try
            {
                var platformGames = await ScanPlatformAsync(platform, cancellationToken);
                allGames.AddRange(platformGames);
            }
            catch (Exception ex)
            {
                // Log error but continue with other platforms
                Debug.WriteLine($"Error scanning {platform}: {ex.Message}");
            }

            completed++;
            ReportProgress(total, completed, $"Completed {platform}", allGames.Count);
        }

        ReportProgress(total, completed, $"Scan complete - {allGames.Count} games found", allGames.Count);
        return allGames;
    }

    /// <summary>
    /// Scan a specific gaming platform for installed games
    /// </summary>
    [SupportedOSPlatform("windows")]
    public async Task<IEnumerable<GameInfo>> ScanPlatformAsync(GamePlatform platform, CancellationToken cancellationToken = default)
    {
        return platform switch
        {
            GamePlatform.Steam => await ScanSteamAsync(cancellationToken),
            GamePlatform.Epic => await ScanEpicAsync(cancellationToken),
            GamePlatform.Xbox => await ScanXboxAsync(cancellationToken),
            GamePlatform.GOG => await ScanGOGAsync(cancellationToken),
            GamePlatform.EA => await ScanEAAsync(cancellationToken),
            GamePlatform.Ubisoft => await ScanUbisoftAsync(cancellationToken),
            _ => Enumerable.Empty<GameInfo>()
        };
    }

    /// <summary>
    /// Verify if a game path is valid and extract game information
    /// </summary>
    public async Task<GameInfo?> VerifyGamePathAsync(string gamePath)
    {
        if (string.IsNullOrWhiteSpace(gamePath) || !Directory.Exists(gamePath))
            return null;

        var executablePath = await Task.Run(() => FindGameExecutableInPath(gamePath));
        if (string.IsNullOrEmpty(executablePath))
            return null;

        var game = new GameInfo
        {
            Name = Path.GetFileNameWithoutExtension(executablePath),
            Path = gamePath,
            Executable = executablePath,
            InstallDirectory = Path.GetDirectoryName(executablePath) ?? gamePath,
            Platform = GamePlatform.Manual
        };

        await RefreshModStatusAsync(game);
        return game;
    }

    /// <summary>
    /// Refresh mod status for a specific game
    /// </summary>
    public async Task<GameInfo> RefreshModStatusAsync(GameInfo game)
    {
        await Task.Run(() =>
        {
            // Check for OptiScaler DLL files in the install directory (where the .exe is located)
            var installDir = !string.IsNullOrEmpty(game.InstallDirectory) ? game.InstallDirectory : game.Path;
            game.HasOptiScaler = CheckForOptiScalerMod(installDir);
            
            // Check for OptiPatcher mod files
            game.HasOptiPatcher = CheckForOptiPatcherMod(installDir);
            
            // If OptiScaler is installed, try to read configuration
            if (game.HasOptiScaler)
            {
                ReadOptiScalerConfiguration(game, installDir);
            }
            else
            {
                // Clear configuration if OptiScaler is not installed
                game.UpscalingMethod = null;
                game.QualityPreset = null;
                game.HasFrameGeneration = false;
            }
            
            game.LastScanned = DateTime.Now;
        });

        return game;
    }

    #region Platform-Specific Scanning

    [SupportedOSPlatform("windows")]
    private async Task<IEnumerable<GameInfo>> ScanSteamAsync(CancellationToken cancellationToken)
    {
        var games = new List<GameInfo>();

        try
        {
            // Try to find Steam installation path from registry
            if (!OperatingSystem.IsWindows())
                return games;

            var steamPath = GetSteamInstallPath();
            if (string.IsNullOrEmpty(steamPath))
                return games;

            var libraryFolders = await GetSteamLibraryFoldersAsync(steamPath, cancellationToken);
            
            foreach (var libraryFolder in libraryFolders)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                var steamAppsPath = Path.Combine(libraryFolder, "steamapps", "common");
                if (!Directory.Exists(steamAppsPath))
                    continue;

                await Task.Run(() =>
                {
                    foreach (var gameFolder in Directory.GetDirectories(steamAppsPath))
                    {
                        if (cancellationToken.IsCancellationRequested)
                            break;

                        var executablePath = FindGameExecutableInPath(gameFolder);
                        if (!string.IsNullOrEmpty(executablePath))
                        {
                            var game = new GameInfo
                            {
                                Name = Path.GetFileName(gameFolder),
                                Path = gameFolder,
                                Executable = executablePath,
                                InstallDirectory = Path.GetDirectoryName(executablePath) ?? gameFolder,
                                Platform = GamePlatform.Steam
                            };

                            RefreshModStatusAsync(game).Wait();
                            games.Add(game);
                            GameDiscovered?.Invoke(this, game);
                        }
                    }
                }, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Steam scan error: {ex.Message}");
        }

        return games;
    }

    private async Task<IEnumerable<GameInfo>> ScanEpicAsync(CancellationToken cancellationToken)
    {
        var games = new List<GameInfo>();

        try
        {
            // Epic Games manifests location
            var epicManifestsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "Epic", "EpicGamesLauncher", "Data", "Manifests");

            if (!Directory.Exists(epicManifestsPath))
                return games;

            await Task.Run(() =>
            {
                foreach (var manifestFile in Directory.GetFiles(epicManifestsPath, "*.item"))
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    try
                    {
                        var game = ParseEpicManifest(manifestFile);
                        if (game != null && Directory.Exists(game.Path))
                        {
                            game.InstallDirectory = Path.GetDirectoryName(game.Executable) ?? game.Path;
                            
                            RefreshModStatusAsync(game).Wait();
                            games.Add(game);
                            GameDiscovered?.Invoke(this, game);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Epic manifest parse error: {ex.Message}");
                    }
                }
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Epic scan error: {ex.Message}");
        }

        return games;
    }

    private async Task<IEnumerable<GameInfo>> ScanXboxAsync(CancellationToken cancellationToken)
    {
        var games = new List<GameInfo>();

        try
        {
            // Xbox Game Pass games location - C:\XboxGames (primary location for Game Pass games)
            var xboxGamesPath = @"C:\XboxGames";

            if (Directory.Exists(xboxGamesPath))
            {
                Debug.WriteLine($"[GameScanner] Scanning Xbox Game Pass directory: {xboxGamesPath}");
                
                await Task.Run(() =>
                {
                    foreach (var gameFolder in Directory.GetDirectories(xboxGamesPath))
                    {
                        if (cancellationToken.IsCancellationRequested)
                            break;

                        // Skip non-game folders
                        var folderName = Path.GetFileName(gameFolder);
                        if (folderName.Equals("GameSave", StringComparison.OrdinalIgnoreCase))
                        {
                            Debug.WriteLine($"[GameScanner] Skipping GameSave folder");
                            continue;
                        }

                        // Xbox games typically have a Content subfolder
                        var contentFolder = Path.Combine(gameFolder, "Content");
                        var searchPath = Directory.Exists(contentFolder) ? contentFolder : gameFolder;

                        var executablePath = FindXboxGameExecutable(searchPath);
                        if (!string.IsNullOrEmpty(executablePath))
                        {
                            var exeDir = Path.GetDirectoryName(executablePath) ?? gameFolder;
                            
                            Debug.WriteLine($"[GameScanner] Found Xbox game: {folderName}");
                            Debug.WriteLine($"[GameScanner] Executable: {executablePath}");
                            Debug.WriteLine($"[GameScanner] InstallDirectory: {exeDir}");
                            
                            var game = new GameInfo
                            {
                                Name = folderName,
                                Path = gameFolder,
                                Executable = executablePath,
                                InstallDirectory = exeDir,
                                Platform = GamePlatform.Xbox
                            };

                            RefreshModStatusAsync(game).Wait();
                            games.Add(game);
                            GameDiscovered?.Invoke(this, game);
                        }
                        else
                        {
                            Debug.WriteLine($"[GameScanner] No valid executable found in {folderName}");
                        }
                    }
                }, cancellationToken);
                
                Debug.WriteLine($"[GameScanner] Xbox scan complete: {games.Count} games found in {xboxGamesPath}");
            }
            else
            {
                Debug.WriteLine($"[GameScanner] Xbox Game Pass directory not found: {xboxGamesPath}");
            }

            // NOTE: WindowsApps scanning removed - it's too broad, slow, and has permission issues
            // Xbox Game Pass games are already in C:\XboxGames
            // If you need to scan WindowsApps, enable Deep Scan and add custom path: C:\Program Files\WindowsApps
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[GameScanner] Xbox scan error: {ex.Message}");
        }

        return games;
    }

    [SupportedOSPlatform("windows")]
    private async Task<IEnumerable<GameInfo>> ScanGOGAsync(CancellationToken cancellationToken)
    {
        var games = new List<GameInfo>();

        try
        {
            // GOG Galaxy games are registered in the registry
            if (!OperatingSystem.IsWindows())
                return games;

            using var gogKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\GOG.com\Games");
            if (gogKey == null)
                return games;

            await Task.Run(() =>
            {
                foreach (var gameKeyName in gogKey.GetSubKeyNames())
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    using var gameKey = gogKey.OpenSubKey(gameKeyName);
                    var gamePath = gameKey?.GetValue("path") as string;
                    var gameName = gameKey?.GetValue("gameName") as string;
                    var exe = gameKey?.GetValue("exe") as string;

                    if (!string.IsNullOrEmpty(gamePath) && Directory.Exists(gamePath))
                    {
                        var executablePath = !string.IsNullOrEmpty(exe) 
                            ? Path.Combine(gamePath, exe) 
                            : FindGameExecutableInPath(gamePath);

                        if (!string.IsNullOrEmpty(executablePath))
                        {
                            var game = new GameInfo
                            {
                                Name = gameName ?? Path.GetFileName(gamePath),
                                Path = gamePath,
                                Executable = executablePath,
                                InstallDirectory = Path.GetDirectoryName(executablePath) ?? gamePath,
                                Platform = GamePlatform.GOG
                            };

                            RefreshModStatusAsync(game).Wait();
                            games.Add(game);
                            GameDiscovered?.Invoke(this, game);
                        }
                    }
                }
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GOG scan error: {ex.Message}");
        }

        return games;
    }

    private async Task<IEnumerable<GameInfo>> ScanEAAsync(CancellationToken cancellationToken)
    {
        var games = new List<GameInfo>();

        try
        {
            // EA App / Origin games location
            var eaPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                "EA Games");

            if (Directory.Exists(eaPath))
            {
                await ScanGenericGameFolder(eaPath, GamePlatform.EA, games, cancellationToken);
            }

            // Check Origin folder as well
            var originPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                "Origin Games");

            if (Directory.Exists(originPath))
            {
                await ScanGenericGameFolder(originPath, GamePlatform.EA, games, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"EA scan error: {ex.Message}");
        }

        return games;
    }

    private async Task<IEnumerable<GameInfo>> ScanUbisoftAsync(CancellationToken cancellationToken)
    {
        var games = new List<GameInfo>();

        try
        {
            // Ubisoft Connect games location
            var ubisoftPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                "Ubisoft", "Ubisoft Game Launcher", "games");

            if (Directory.Exists(ubisoftPath))
            {
                await ScanGenericGameFolder(ubisoftPath, GamePlatform.Ubisoft, games, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Ubisoft scan error: {ex.Message}");
        }

        return games;
    }

    #endregion

    #region Helper Methods

    private async Task ScanGenericGameFolder(string basePath, GamePlatform platform, List<GameInfo> games, CancellationToken cancellationToken)
    {
        await Task.Run(() =>
        {
            foreach (var gameFolder in Directory.GetDirectories(basePath))
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                var executablePath = FindGameExecutableInPath(gameFolder);
                if (!string.IsNullOrEmpty(executablePath))
                {
                    var game = new GameInfo
                    {
                        Name = Path.GetFileName(gameFolder),
                        Path = gameFolder,
                        Executable = executablePath,
                        InstallDirectory = Path.GetDirectoryName(executablePath) ?? gameFolder,
                        Platform = platform
                    };

                    RefreshModStatusAsync(game).Wait();
                    games.Add(game);
                    GameDiscovered?.Invoke(this, game);
                }
            }
        }, cancellationToken);
    }

    private string? FindGameExecutableInPath(string gamePath)
    {
        // Check common game folders for executables
        foreach (var folder in CommonGameFolders)
        {
            var searchPath = string.IsNullOrEmpty(folder) ? gamePath : Path.Combine(gamePath, folder);
            
            if (!Directory.Exists(searchPath))
                continue;

            try
            {
                var executables = Directory.GetFiles(searchPath, "*.exe", SearchOption.TopDirectoryOnly)
                    .Where(f => !IsSystemExecutable(f))
                    .ToArray();

                if (executables.Length > 0)
                {
                    // Prefer executables with specific patterns
                    var preferredExe = executables.FirstOrDefault(e => 
                        !Path.GetFileName(e).Contains("unins", StringComparison.OrdinalIgnoreCase) &&
                        !Path.GetFileName(e).Contains("crash", StringComparison.OrdinalIgnoreCase) &&
                        !Path.GetFileName(e).Contains("launch", StringComparison.OrdinalIgnoreCase));

                    return preferredExe ?? executables[0];
                }
            }
            catch (UnauthorizedAccessException)
            {
                continue;
            }
        }

        return null;
    }

    private string? FindXboxGameExecutable(string gamePath)
    {
        try
        {
            Debug.WriteLine($"[GameScanner] Searching Xbox executable in: {gamePath}");
            
            var allExecutables = Directory.GetFiles(gamePath, "*.exe", SearchOption.AllDirectories)
                .Where(f => !IsSystemExecutable(f) && !IsXboxHelperExecutable(f))
                .ToArray();

            Debug.WriteLine($"[GameScanner] Found {allExecutables.Length} potential executables");
            foreach (var exe in allExecutables)
            {
                Debug.WriteLine($"[GameScanner]   - {exe}");
            }

            if (allExecutables.Length == 0)
                return null;

            // Priority 1: WinGDK executables (Xbox Game Pass games)
            var wingdkExecutables = allExecutables.Where(e =>
            {
                var dirName = Path.GetDirectoryName(e) ?? "";
                var fileName = Path.GetFileName(e);
                return dirName.Contains("WinGDK", StringComparison.OrdinalIgnoreCase) ||
                       fileName.Contains("WinGDK", StringComparison.OrdinalIgnoreCase);
            }).ToArray();

            if (wingdkExecutables.Length > 0)
            {
                var shippingExe = wingdkExecutables.FirstOrDefault(e =>
                    Path.GetFileName(e).Contains("Shipping", StringComparison.OrdinalIgnoreCase));
                if (shippingExe != null)
                {
                    Debug.WriteLine($"[GameScanner] Selected WinGDK Shipping executable: {shippingExe}");
                    return shippingExe;
                }
                
                Debug.WriteLine($"[GameScanner] Selected WinGDK executable: {wingdkExecutables[0]}");
                return wingdkExecutables[0];
            }

            // Priority 2: Executables in Binaries folder
            var binariesExecutables = allExecutables.Where(e =>
            {
                var dirName = Path.GetDirectoryName(e) ?? "";
                return dirName.Contains("Binaries", StringComparison.OrdinalIgnoreCase);
            }).ToArray();

            if (binariesExecutables.Length > 0)
            {
                var selected = binariesExecutables.OrderBy(f => f.Length).First();
                Debug.WriteLine($"[GameScanner] Selected Binaries executable: {selected}");
                return selected;
            }

            // Priority 3: Avoid helper/launcher executables
            var gameExecutables = allExecutables.Where(e =>
            {
                var fileName = Path.GetFileName(e).ToLowerInvariant();
                return !fileName.Contains("helper") && 
                       !fileName.Contains("launcher") && 
                       !fileName.Contains("crashhandler") &&
                       !fileName.Contains("unins");
            }).ToArray();

            if (gameExecutables.Length > 0)
            {
                var gameFolderName = Path.GetFileName(gamePath).ToLowerInvariant();
                var matchingExe = gameExecutables.FirstOrDefault(e =>
                {
                    var fileName = Path.GetFileNameWithoutExtension(e).ToLowerInvariant();
                    return fileName.Contains(gameFolderName) || gameFolderName.Contains(fileName);
                });

                if (matchingExe != null)
                {
                    Debug.WriteLine($"[GameScanner] Selected matching executable: {matchingExe}");
                    return matchingExe;
                }

                var selected = gameExecutables[0];
                Debug.WriteLine($"[GameScanner] Selected fallback executable: {selected}");
                return selected;
            }

            var lastResort = allExecutables.OrderBy(f => f.Length).First();
            Debug.WriteLine($"[GameScanner] Selected last resort executable: {lastResort}");
            return lastResort;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[GameScanner] Error finding Xbox executable in {gamePath}: {ex.Message}");
            return null;
        }
    }

    private bool IsXboxHelperExecutable(string exePath)
    {
        var fileName = Path.GetFileName(exePath).ToLowerInvariant();
        return fileName.Contains("gamelaunchhelper") ||
               fileName.Contains("gamingrepairtool") ||
               fileName.Contains("gamingrepair") ||
               fileName.Contains("ueprereqsetup");
    }

    private bool IsSystemExecutable(string exePath)
    {
        var fileName = Path.GetFileName(exePath).ToLowerInvariant();
        return fileName.Contains("unins") || 
               fileName.Contains("setup") || 
               fileName.Contains("installer") ||
               fileName.Contains("redist") ||
               fileName.Contains("vcredist") ||
               fileName.Contains("directx");
    }

    private bool CheckForOptiScalerMod(string gamePath)
    {
        // Check for OptiScaler DLL files in game directory (updated for 0.7.9+ complete)
        var optiScalerFiles = new[] { 
            "OptiScaler.dll",                          // 0.7.9+ main DLL
            "nvngx.dll",                               // Legacy (older versions)
            "libxess.dll",                             // XeSS library
            "libxess_dx11.dll",                        // XeSS DX11 library
            "amd_fidelityfx_vk.dll",                   // AMD FSR (Vulkan)
            "amd_fidelityfx_dx12.dll",                 // AMD FSR (DirectX 12)
            "amd_fidelityfx_upscaler_dx12.dll",        // AMD FSR Upscaler (DX12)
            "amd_fidelityfx_framegeneration_dx12.dll"  // AMD FSR Frame Generation (DX12)
        };
        
        foreach (var file in optiScalerFiles)
        {
            var filePath = Path.Combine(gamePath, file);
            if (File.Exists(filePath))
            {
                return true;
            }
        }

        return false;
    }

    private bool CheckForOptiPatcherMod(string gamePath)
    {
        // Check for OptiPatcher ASI plugin files
        var optiPatcherFiles = new[] { "OptiPatcher.asi", "nvngx_patch.dll" };
        
        // Check in plugins folder first
        var pluginsPath = Path.Combine(gamePath, "plugins");
        if (Directory.Exists(pluginsPath))
        {
            foreach (var file in optiPatcherFiles)
            {
                var filePath = Path.Combine(pluginsPath, file);
                if (File.Exists(filePath))
                {
                    Debug.WriteLine($"[GameScanner] Found OptiPatcher in plugins: {filePath}");
                    return true;
                }
            }
        }
        
        // Check in game root directory
        foreach (var file in optiPatcherFiles)
        {
            var filePath = Path.Combine(gamePath, file);
            if (File.Exists(filePath))
            {
                Debug.WriteLine($"[GameScanner] Found OptiPatcher in root: {filePath}");
                return true;
            }
        }
        
        // Also check recursively for any .asi files that start with OptiPatcher
        try
        {
            var allAsiFiles = Directory.GetFiles(gamePath, "*.asi", SearchOption.AllDirectories);
            foreach (var asiFile in allAsiFiles)
            {
                var fileName = Path.GetFileName(asiFile);
                if (fileName.StartsWith("OptiPatcher", StringComparison.OrdinalIgnoreCase))
                {
                    Debug.WriteLine($"[GameScanner] Found OptiPatcher variant: {asiFile}");
                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[GameScanner] Error checking for OptiPatcher recursively: {ex.Message}");
        }

        return false;
    }

    private void ReadOptiScalerConfiguration(GameInfo game, string installDir)
    {
        try
        {
            var iniPath = Path.Combine(installDir, "OptiScaler.ini");
            if (!File.Exists(iniPath))
            {
                SetDefaultConfiguration(game, installDir);
                return;
            }

            var configService = new OptiScalerConfigService();
            var config = configService.ReadConfig(iniPath);

            game.UpscalingMethod = config.GetUpscalerDisplayName();
            game.HasFrameGeneration = config.IsFrameGenerationEnabled();
            game.QualityPreset = config.GetQualityPresetName();

            Debug.WriteLine($"[GameScanner] Read OptiScaler config - Upscaler: {game.UpscalingMethod}, FG: {game.HasFrameGeneration}, Quality: {game.QualityPreset}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[GameScanner] Error reading OptiScaler config: {ex.Message}");
            SetDefaultConfiguration(game, installDir);
        }
    }

    private void SetDefaultConfiguration(GameInfo game, string installDir)
    {
        if (File.Exists(Path.Combine(installDir, "libxess.dll")))
        {
            game.UpscalingMethod = "XeSS";
        }
        else if (File.Exists(Path.Combine(installDir, "amd_fidelityfx_dx12.dll")))
        {
            if (File.Exists(Path.Combine(installDir, "amd_fidelityfx_framegeneration_dx12.dll")))
            {
                game.UpscalingMethod = "FSR 3";
                game.HasFrameGeneration = true;
            }
            else
            {
                game.UpscalingMethod = "FSR 2";
            }
        }
        else if (File.Exists(Path.Combine(installDir, "nvngx.dll")))
        {
            game.UpscalingMethod = "DLSS";
            if (game.HasOptiPatcher)
            {
                game.HasFrameGeneration = true;
            }
        }
        else
        {
            game.UpscalingMethod = "Auto";
        }

        if (string.IsNullOrEmpty(game.QualityPreset))
        {
            game.QualityPreset = "Quality";
        }
    }

    [SupportedOSPlatform("windows")]
    private string? GetSteamInstallPath()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Valve\Steam");
            return key?.GetValue("InstallPath") as string;
        }
        catch
        {
            return null;
        }
    }

    private async Task<List<string>> GetSteamLibraryFoldersAsync(string steamPath, CancellationToken cancellationToken)
    {
        var folders = new List<string> { steamPath };

        try
        {
            var libraryFoldersFile = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");
            if (!File.Exists(libraryFoldersFile))
                return folders;

            var content = await File.ReadAllTextAsync(libraryFoldersFile, cancellationToken);
            
            // Simple VDF parsing - look for "path" entries
            var lines = content.Split('\n');
            foreach (var line in lines)
            {
                if (line.Contains("\"path\""))
                {
                    var pathStart = line.IndexOf("\"", line.IndexOf("\"path\"") + 6) + 1;
                    var pathEnd = line.IndexOf("\"", pathStart);
                    if (pathStart > 0 && pathEnd > pathStart)
                    {
                        var path = line.Substring(pathStart, pathEnd - pathStart);
                        path = path.Replace("\\\\", "\\");
                        if (Directory.Exists(path) && !folders.Contains(path))
                            folders.Add(path);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Steam library folders parse error: {ex.Message}");
        }

        return folders;
    }

    private GameInfo? ParseEpicManifest(string manifestPath)
    {
        try
        {
            var content = File.ReadAllText(manifestPath);
            
            // Simple JSON-like parsing for Epic manifests
            // In production, use System.Text.Json or Newtonsoft.Json
            var installLocation = ExtractJsonValue(content, "InstallLocation");
            var displayName = ExtractJsonValue(content, "DisplayName");
            var launchExecutable = ExtractJsonValue(content, "LaunchExecutable");

            if (!string.IsNullOrEmpty(installLocation))
            {
                var executablePath = !string.IsNullOrEmpty(launchExecutable)
                    ? Path.Combine(installLocation, launchExecutable)
                    : FindGameExecutableInPath(installLocation);

                if (!string.IsNullOrEmpty(executablePath))
                {
                    return new GameInfo
                    {
                        Name = displayName ?? Path.GetFileName(installLocation),
                        Path = installLocation,
                        Executable = executablePath,
                        Platform = GamePlatform.Epic
                    };
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Epic manifest parse error: {ex.Message}");
        }

        return null;
    }

    private string? ExtractJsonValue(string json, string key)
    {
        var keyPattern = $"\"{key}\"";
        var keyIndex = json.IndexOf(keyPattern);
        if (keyIndex < 0)
            return null;

        var valueStart = json.IndexOf("\"", keyIndex + keyPattern.Length);
        if (valueStart < 0)
            return null;

        valueStart++;
        var valueEnd = json.IndexOf("\"", valueStart);
        if (valueEnd < 0)
            return null;

        return json.Substring(valueStart, valueEnd - valueStart);
    }

    private void ReportProgress(int total, int completed, string message, int gamesFound)
    {
        ScanProgress?.Invoke(this, new ScanProgressEventArgs
        {
            TotalPlatforms = total,
            CompletedPlatforms = completed,
            CurrentPlatform = message,
            GamesFound = gamesFound,
            StatusMessage = message
        });
    }

    #endregion
}
