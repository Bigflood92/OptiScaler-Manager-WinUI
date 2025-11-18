using System.Diagnostics;
using System.IO.Compression;
using OptiScaler.Core.Contracts;
using OptiScaler.Core.Models;

namespace OptiScaler.Core.Services;

/// <summary>
/// Service for installing, updating, and managing mods in games
/// </summary>
public class ModInstallerService : IModInstallerService
{
    private readonly IGitHubService _gitHubService;
    
    public event EventHandler<InstallProgressEventArgs>? InstallProgress;

    // Known mod file patterns
    private static readonly Dictionary<ModType, string[]> ModFilePatterns = new()
    {
        { 
            ModType.OptiScaler, 
            new[] { "nvngx.dll", "libxess.dll", "amd_fidelityfx_vk.dll", "nvngx.ini", "EnableSignatureOverride.reg" }
        },
        { 
            ModType.DlssgToFsr3, 
            new[] { "dlssg_to_fsr3.dll", "nvngx_dlssg.dll", "nvngx_dlss.dll", "dlssg_to_fsr3.ini" }
        }
    };

    public ModInstallerService(IGitHubService gitHubService)
    {
        _gitHubService = gitHubService ?? throw new ArgumentNullException(nameof(gitHubService));
    }

    /// <summary>
    /// Install a mod to a game from a downloaded archive
    /// </summary>
    public async Task<ModOperationResult> InstallModAsync(GameInfo game, ModType modType, string archivePath, CancellationToken cancellationToken = default)
    {
        var result = new ModOperationResult
        {
            Operation = ModOperation.Install,
            ModType = modType
        };

        try
        {
            ReportProgress(modType, game.Name, ModOperation.Install, 1, 5, "Validating archive...");

            if (!File.Exists(archivePath))
            {
                result.Success = false;
                result.ErrorMessage = "Archive file not found";
                return result;
            }

            // Create temporary extraction directory
            var tempDir = Path.Combine(Path.GetTempPath(), $"OptiScaler_{Guid.NewGuid()}");
            Directory.CreateDirectory(tempDir);

            try
            {
                ReportProgress(modType, game.Name, ModOperation.Install, 2, 5, "Extracting archive...");

                // Extract archive
                ZipFile.ExtractToDirectory(archivePath, tempDir);

                ReportProgress(modType, game.Name, ModOperation.Install, 3, 5, "Finding mod files...");

                // Find mod files in extracted directory
                var modFiles = FindModFilesInDirectory(tempDir, modType);
                if (modFiles.Count == 0)
                {
                    result.Success = false;
                    result.ErrorMessage = "No mod files found in archive";
                    return result;
                }

                ReportProgress(modType, game.Name, ModOperation.Install, 4, 5, "Installing files...");

                // Copy mod files to game directory
                foreach (var sourceFile in modFiles)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        result.Success = false;
                        result.ErrorMessage = "Installation cancelled";
                        return result;
                    }

                    var fileName = Path.GetFileName(sourceFile);
                    var destFile = Path.Combine(game.Path, fileName);

                    // Backup existing file if it exists
                    if (File.Exists(destFile))
                    {
                        var backupFile = $"{destFile}.backup";
                        File.Copy(destFile, backupFile, true);
                    }

                    File.Copy(sourceFile, destFile, true);
                    result.AffectedFiles.Add(destFile);
                }

                ReportProgress(modType, game.Name, ModOperation.Install, 5, 5, "Installation complete");

                result.Success = true;
            }
            finally
            {
                // Clean up temporary directory
                try
                {
                    if (Directory.Exists(tempDir))
                        Directory.Delete(tempDir, true);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error cleaning up temp directory: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error installing mod: {ex.Message}");
            result.Success = false;
            result.ErrorMessage = ex.Message;
        }

        return result;
    }

    /// <summary>
    /// Uninstall a mod from a game
    /// </summary>
    public async Task<ModOperationResult> UninstallModAsync(GameInfo game, ModType modType, CancellationToken cancellationToken = default)
    {
        var result = new ModOperationResult
        {
            Operation = ModOperation.Uninstall,
            ModType = modType
        };

        try
        {
            ReportProgress(modType, game.Name, ModOperation.Uninstall, 1, 3, "Finding mod files...");

            // Find mod files in game directory
            if (!ModFilePatterns.TryGetValue(modType, out var patterns))
            {
                result.Success = false;
                result.ErrorMessage = "Unknown mod type";
                return result;
            }

            var filesToDelete = new List<string>();
            foreach (var pattern in patterns)
            {
                var filePath = Path.Combine(game.Path, pattern);
                if (File.Exists(filePath))
                {
                    filesToDelete.Add(filePath);
                }
            }

            if (filesToDelete.Count == 0)
            {
                result.Success = false;
                result.ErrorMessage = "Mod files not found";
                return result;
            }

            ReportProgress(modType, game.Name, ModOperation.Uninstall, 2, 3, "Removing files...");

            await Task.Run(() =>
            {
                foreach (var file in filesToDelete)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        result.Success = false;
                        result.ErrorMessage = "Uninstallation cancelled";
                        return;
                    }

                    try
                    {
                        // Check if backup exists and restore it
                        var backupFile = $"{file}.backup";
                        if (File.Exists(backupFile))
                        {
                            File.Copy(backupFile, file, true);
                            File.Delete(backupFile);
                        }
                        else
                        {
                            File.Delete(file);
                        }

                        result.AffectedFiles.Add(file);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error deleting file {file}: {ex.Message}");
                    }
                }
            }, cancellationToken);

            ReportProgress(modType, game.Name, ModOperation.Uninstall, 3, 3, "Uninstallation complete");

            result.Success = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error uninstalling mod: {ex.Message}");
            result.Success = false;
            result.ErrorMessage = ex.Message;
        }

        return result;
    }

    /// <summary>
    /// Update an installed mod to the latest version
    /// </summary>
    public async Task<ModOperationResult> UpdateModAsync(GameInfo game, ModType modType, CancellationToken cancellationToken = default)
    {
        var result = new ModOperationResult
        {
            Operation = ModOperation.Update,
            ModType = modType
        };

        try
        {
            ReportProgress(modType, game.Name, ModOperation.Update, 1, 4, "Downloading latest version...");

            // Download latest version
            var tempFile = Path.Combine(Path.GetTempPath(), $"{modType}_{Guid.NewGuid()}.zip");
            var downloadResult = await _gitHubService.DownloadLatestModAsync(modType, tempFile, cancellationToken);

            if (downloadResult == null)
            {
                result.Success = false;
                result.ErrorMessage = "Failed to download latest version";
                return result;
            }

            try
            {
                ReportProgress(modType, game.Name, ModOperation.Update, 2, 4, "Backing up current installation...");

                // Backup current installation
                var backupPath = await CreateBackupAsync(game, cancellationToken);

                ReportProgress(modType, game.Name, ModOperation.Update, 3, 4, "Installing update...");

                // Install new version
                var installResult = await InstallModAsync(game, modType, downloadResult.Value.FilePath, cancellationToken);
                
                if (!installResult.Success)
                {
                    // Restore backup on failure
                    await RestoreBackupAsync(game, backupPath, cancellationToken);
                    result.Success = false;
                    result.ErrorMessage = $"Installation failed: {installResult.ErrorMessage}";
                    return result;
                }

                ReportProgress(modType, game.Name, ModOperation.Update, 4, 4, "Update complete");

                result.Success = true;
                result.AffectedFiles = installResult.AffectedFiles;
            }
            finally
            {
                // Clean up downloaded file
                try
                {
                    if (File.Exists(tempFile))
                        File.Delete(tempFile);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error cleaning up download: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error updating mod: {ex.Message}");
            result.Success = false;
            result.ErrorMessage = ex.Message;
        }

        return result;
    }

    /// <summary>
    /// Get information about installed mods in a game
    /// </summary>
    public async Task<IEnumerable<ModInfo>> GetInstalledModsAsync(GameInfo game, CancellationToken cancellationToken = default)
    {
        var mods = new List<ModInfo>();

        await Task.Run(() =>
        {
            foreach (var modType in Enum.GetValues<ModType>())
            {
                if (modType == ModType.Custom)
                    continue;

                if (!ModFilePatterns.TryGetValue(modType, out var patterns))
                    continue;

                var installedFiles = new List<string>();
                foreach (var pattern in patterns)
                {
                    var filePath = Path.Combine(game.Path, pattern);
                    if (File.Exists(filePath))
                    {
                        installedFiles.Add(filePath);
                    }
                }

                if (installedFiles.Count > 0)
                {
                    var repo = _gitHubService.GetModRepository(modType);
                    var modInfo = new ModInfo
                    {
                        Type = modType,
                        Name = modType.ToString(),
                        RepositoryOwner = repo.Owner,
                        RepositoryName = repo.Repo,
                        DllFiles = installedFiles,
                        InstalledVersion = GetInstalledVersion(installedFiles.First()),
                        InstallDate = File.GetCreationTime(installedFiles.First())
                    };

                    mods.Add(modInfo);
                }
            }
        }, cancellationToken);

        // Fetch latest versions
        foreach (var mod in mods)
        {
            try
            {
                var latestRelease = await _gitHubService.GetLatestReleaseAsync(mod.RepositoryOwner, mod.RepositoryName, cancellationToken);
                if (latestRelease != null)
                {
                    mod.LatestVersion = latestRelease.Version;
                    mod.Description = latestRelease.Name;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching latest version for {mod.Name}: {ex.Message}");
            }
        }

        return mods;
    }

    /// <summary>
    /// Verify integrity of installed mod files
    /// </summary>
    public async Task<ModOperationResult> VerifyModIntegrityAsync(GameInfo game, ModType modType, CancellationToken cancellationToken = default)
    {
        var result = new ModOperationResult
        {
            Operation = ModOperation.Verify,
            ModType = modType
        };

        await Task.Run(() =>
        {
            if (!ModFilePatterns.TryGetValue(modType, out var patterns))
            {
                result.Success = false;
                result.ErrorMessage = "Unknown mod type";
                return;
            }

            var missingFiles = new List<string>();
            foreach (var pattern in patterns)
            {
                var filePath = Path.Combine(game.Path, pattern);
                if (!File.Exists(filePath))
                {
                    missingFiles.Add(pattern);
                }
                else
                {
                    result.AffectedFiles.Add(filePath);
                }
            }

            if (missingFiles.Count > 0)
            {
                result.Success = false;
                result.ErrorMessage = $"Missing files: {string.Join(", ", missingFiles)}";
            }
            else
            {
                result.Success = true;
            }
        }, cancellationToken);

        return result;
    }

    /// <summary>
    /// Create a backup of game files before mod installation
    /// </summary>
    public async Task<string> CreateBackupAsync(GameInfo game, CancellationToken cancellationToken = default)
    {
        var backupDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "OptiScaler Manager", "Backups", game.Name, DateTime.Now.ToString("yyyyMMdd_HHmmss"));

        await Task.Run(() =>
        {
            Directory.CreateDirectory(backupDir);

            // Backup all known mod files that exist
            foreach (var patterns in ModFilePatterns.Values)
            {
                foreach (var pattern in patterns)
                {
                    var sourceFile = Path.Combine(game.Path, pattern);
                    if (File.Exists(sourceFile))
                    {
                        var destFile = Path.Combine(backupDir, pattern);
                        File.Copy(sourceFile, destFile, true);
                    }
                }
            }
        }, cancellationToken);

        return backupDir;
    }

    /// <summary>
    /// Restore game files from a backup
    /// </summary>
    public async Task<ModOperationResult> RestoreBackupAsync(GameInfo game, string backupPath, CancellationToken cancellationToken = default)
    {
        var result = new ModOperationResult
        {
            Operation = ModOperation.Install,
            ModType = ModType.Custom
        };

        try
        {
            if (!Directory.Exists(backupPath))
            {
                result.Success = false;
                result.ErrorMessage = "Backup directory not found";
                return result;
            }

            await Task.Run(() =>
            {
                foreach (var backupFile in Directory.GetFiles(backupPath))
                {
                    var fileName = Path.GetFileName(backupFile);
                    var destFile = Path.Combine(game.Path, fileName);
                    File.Copy(backupFile, destFile, true);
                    result.AffectedFiles.Add(destFile);
                }
            }, cancellationToken);

            result.Success = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error restoring backup: {ex.Message}");
            result.Success = false;
            result.ErrorMessage = ex.Message;
        }

        return result;
    }

    #region Helper Methods

    private List<string> FindModFilesInDirectory(string directory, ModType modType)
    {
        var modFiles = new List<string>();

        if (!ModFilePatterns.TryGetValue(modType, out var patterns))
            return modFiles;

        // Search recursively for mod files
        foreach (var pattern in patterns)
        {
            var files = Directory.GetFiles(directory, pattern, SearchOption.AllDirectories);
            modFiles.AddRange(files);
        }

        return modFiles;
    }

    private Version? GetInstalledVersion(string filePath)
    {
        try
        {
            var versionInfo = FileVersionInfo.GetVersionInfo(filePath);
            if (!string.IsNullOrEmpty(versionInfo.FileVersion))
            {
                // Try to parse version
                if (Version.TryParse(versionInfo.FileVersion, out var version))
                    return version;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error reading file version: {ex.Message}");
        }

        return null;
    }

    private void ReportProgress(ModType modType, string gameName, ModOperation operation, int current, int total, string action)
    {
        InstallProgress?.Invoke(this, new InstallProgressEventArgs
        {
            Operation = operation,
            ModType = modType,
            GameName = gameName,
            CurrentStep = current,
            TotalSteps = total,
            CurrentAction = action
        });
    }

    #endregion
}
