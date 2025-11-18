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
        var platforms = Enum.GetValues<GamePlatform>().Where(p => p != GamePlatform.Unknown && p != GamePlatform.Manual).ToArray();
        int completed = 0;

        ReportProgress(platforms.Length, completed, "Starting scan...", 0);

        foreach (var platform in platforms)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            ReportProgress(platforms.Length, completed, $"Scanning {platform}...", allGames.Count);

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
            ReportProgress(platforms.Length, completed, $"Completed {platform}", allGames.Count);
        }

        ReportProgress(platforms.Length, completed, $"Scan complete - {allGames.Count} games found", allGames.Count);
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
            // Check for OptiScaler DLL files
            game.HasOptiScaler = CheckForOptiScalerMod(game.Path);
            
            // Check for DLSSG-to-FSR3 mod files
            game.HasDlssgToFsr3 = CheckForDlssgToFsr3Mod(game.Path);
            
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
            // Xbox Game Pass / Microsoft Store apps location
            var windowsAppsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                "WindowsApps");

            if (!Directory.Exists(windowsAppsPath))
                return games;

            // Note: WindowsApps requires special permissions
            // May need to implement alternative detection method via package manager
            await Task.Run(() =>
            {
                try
                {
                    foreach (var gameFolder in Directory.GetDirectories(windowsAppsPath))
                    {
                        if (cancellationToken.IsCancellationRequested)
                            break;

                        // Skip system apps
                        var folderName = Path.GetFileName(gameFolder);
                        if (folderName.StartsWith("Microsoft.") || folderName.StartsWith("Windows."))
                            continue;

                        var executablePath = FindGameExecutableInPath(gameFolder);
                        if (!string.IsNullOrEmpty(executablePath))
                        {
                            var game = new GameInfo
                            {
                                Name = folderName.Split('_')[0],
                                Path = gameFolder,
                                Executable = executablePath,
                                Platform = GamePlatform.Xbox
                            };

                            RefreshModStatusAsync(game).Wait();
                            games.Add(game);
                            GameDiscovered?.Invoke(this, game);
                        }
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    Debug.WriteLine("Xbox: Insufficient permissions for WindowsApps folder");
                }
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Xbox scan error: {ex.Message}");
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
        // Check for OptiScaler DLL files in game directory
        var optiScalerFiles = new[] { "nvngx.dll", "libxess.dll", "amd_fidelityfx_vk.dll" };
        
        foreach (var file in optiScalerFiles)
        {
            var filePath = Path.Combine(gamePath, file);
            if (File.Exists(filePath))
            {
                // Additional verification: check file metadata or signature
                // For now, simple file existence check
                return true;
            }
        }

        return false;
    }

    private bool CheckForDlssgToFsr3Mod(string gamePath)
    {
        // Check for DLSSG-to-FSR3 specific files
        var dlssgFiles = new[] { "dlssg_to_fsr3.dll", "nvngx_dlssg.dll" };
        
        foreach (var file in dlssgFiles)
        {
            var filePath = Path.Combine(gamePath, file);
            if (File.Exists(filePath))
                return true;
        }

        return false;
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
