using System.Diagnostics;
using System.IO.Compression;
using OptiScaler.Core.Contracts;
using OptiScaler.Core.Models;
using SharpCompress.Archives;
using SharpCompress.Common;

namespace OptiScaler.Core.Services;

/// <summary>
/// Service for installing, updating, and managing mods in games
/// </summary>
public class ModInstallerService : IModInstallerService
{
    private readonly IGitHubService _gitHubService;
    private readonly GlobalSettingsService _globalSettingsService;
    
    public event EventHandler<InstallProgressEventArgs>? InstallProgress;

    // Known mod file patterns - Updated for OptiScaler 0.7.9+ (Complete with folders)
    private static readonly Dictionary<ModType, string[]> ModFilePatterns = new()
    {
        { 
            ModType.OptiScaler, 
            new[] { 
                // Core DLL (0.7.9+)
                "OptiScaler.dll",
                // XeSS libraries
                "libxess.dll",
                "libxess_dx11.dll",
                // AMD FidelityFX libraries (all variants)
                "amd_fidelityfx_vk.dll",
                "amd_fidelityfx_dx12.dll",
                "amd_fidelityfx_upscaler_dx12.dll",
                "amd_fidelityfx_framegeneration_dx12.dll",
                // Legacy DLSS (older versions)
                "nvngx.dll",
                // Configuration files
                "OptiScaler.ini",
                "EnableSignatureOverride.reg",
                "DisableSignatureOverride.reg",
                // D3D12 Optiscaler folder
                "D3D12Core.dll"
            }
        },
        { 
            ModType.OptiPatcher, 
            new[] { "OptiPatcher.asi", "nvngx_patch.dll" }
        }
    };

    public ModInstallerService(IGitHubService gitHubService)
    {
        _gitHubService = gitHubService ?? throw new ArgumentNullException(nameof(gitHubService));
        _globalSettingsService = new GlobalSettingsService();
    }

    /// <summary>
    /// Install a mod to a game from a downloaded archive
    /// </summary>
    public async Task<ModOperationResult> InstallModAsync(GameInfo game, ModType modType, string sourcePath, string? targetDllName = null, CancellationToken cancellationToken = default)
    {
        var result = new ModOperationResult
        {
            Operation = ModOperation.Install,
            ModType = modType
        };

        try
        {
            ReportProgress(modType, game.Name, ModOperation.Install, 1, 5, "Validating source...");
            Debug.WriteLine($"[ModInstaller] Source path: {sourcePath}");
            Debug.WriteLine($"[ModInstaller] Source exists as directory: {Directory.Exists(sourcePath)}");
            Debug.WriteLine($"[ModInstaller] Source exists as file: {File.Exists(sourcePath)}");
            Debug.WriteLine($"[ModInstaller] Mod type: {modType}");

            // Load global settings for DLL renaming (if targetDllName not provided)
            var globalSettings = await _globalSettingsService.LoadSettingsAsync();
            var preferredDllName = targetDllName ?? globalSettings.PreferredDllName ?? "dxgi.dll";
            Debug.WriteLine($"[ModInstaller] Preferred DLL name: {preferredDllName}");

            string sourceRoot;
            
            // Check if sourcePath is already an extracted directory
            if (Directory.Exists(sourcePath))
            {
                Debug.WriteLine($"[ModInstaller] Using pre-extracted directory: {sourcePath}");
                sourceRoot = sourcePath;
                ReportProgress(modType, game.Name, ModOperation.Install, 2, 5, "Using pre-extracted files...");
            }
            else if (File.Exists(sourcePath))
            {
                Debug.WriteLine($"[ModInstaller] Source is a file: {sourcePath}");
                
                // For OptiPatcher single .asi file, use the file directly
                if (modType == ModType.OptiPatcher && sourcePath.EndsWith(".asi", StringComparison.OrdinalIgnoreCase))
                {
                    Debug.WriteLine($"[ModInstaller] Using OptiPatcher ASI file directly: {sourcePath}");
                    sourceRoot = sourcePath; // Use the file path directly
                    ReportProgress(modType, game.Name, ModOperation.Install, 2, 5, "Using OptiPatcher ASI file...");
                }
                else
                {
                    // Check if we have a pre-extracted directory next to the archive
                    var extractedDirName = Path.GetFileNameWithoutExtension(sourcePath) + "_extracted";
                    var extractedPath = Path.Combine(Path.GetDirectoryName(sourcePath) ?? "", extractedDirName);
                    
                    if (Directory.Exists(extractedPath))
                    {
                        Debug.WriteLine($"[ModInstaller] Found corresponding extracted directory: {extractedPath}");
                        sourceRoot = extractedPath;
                        ReportProgress(modType, game.Name, ModOperation.Install, 2, 5, "Using pre-extracted files...");
                    }
                    else
                    {
                        // Fallback to extraction (for compatibility)
                        Debug.WriteLine($"[ModInstaller] No pre-extracted directory found, extracting {sourcePath}");
                        
                        ReportProgress(modType, game.Name, ModOperation.Install, 2, 5, "Extracting archive...");
                        
                        var tempDir = Path.Combine(Path.GetTempPath(), $"OptiScaler_{Guid.NewGuid()}");
                        Directory.CreateDirectory(tempDir);
                        sourceRoot = tempDir;
                        
                        var ext = Path.GetExtension(sourcePath).ToLowerInvariant();
                        if (ext == ".zip" || ext == ".7z")
                        {
                            using var stream = File.OpenRead(sourcePath);
                            using var archive = SharpCompress.Archives.ArchiveFactory.Open(stream);
                            
                            foreach (var entry in archive.Entries.Where(e => !e.IsDirectory))
                            {
                                cancellationToken.ThrowIfCancellationRequested();
                                var destPath = Path.Combine(tempDir, entry.Key.Replace('/', Path.DirectorySeparatorChar));
                                Directory.CreateDirectory(Path.GetDirectoryName(destPath)!);
                                entry.WriteToFile(destPath, new SharpCompress.Common.ExtractionOptions { ExtractFullPath = true, Overwrite = true });
                            }
                        }
                        Debug.WriteLine($"[ModInstaller] Extraction complete to: {tempDir}");
                    }
                }
            }
            else
            {
                result.Success = false;
                result.ErrorMessage = $"Source path not found: {sourcePath}";
                Debug.WriteLine($"[ModInstaller] ERROR: Source path does not exist");
                return result;
            }

            // For OptiPatcher we may need to rename versioned .asi (OptiPatcher_vX.XX.asi -> OptiPatcher.asi)
            if (modType == ModType.OptiPatcher)
            {
                ReportProgress(modType, game.Name, ModOperation.Install, 3, 5, "Normalizing OptiPatcher filename...");
                Debug.WriteLine($"[ModInstaller] === OPTIPATCHER NORMALIZATION START ===");
                Debug.WriteLine($"[ModInstaller] Normalizing OptiPatcher filename in: {sourceRoot}");
                Debug.WriteLine($"[ModInstaller] Source root exists as file: {File.Exists(sourceRoot)}");
                Debug.WriteLine($"[ModInstaller] Source root exists as directory: {Directory.Exists(sourceRoot)}");
                
                // Handle single file case (direct .asi file)
                if (File.Exists(sourceRoot) && sourceRoot.EndsWith(".asi", StringComparison.OrdinalIgnoreCase))
                {
                    var fileName = Path.GetFileName(sourceRoot);
                    Debug.WriteLine($"[ModInstaller] Processing single ASI file: {fileName}");
                    Debug.WriteLine($"[ModInstaller] File size: {new FileInfo(sourceRoot).Length} bytes");
                    
                    if (fileName.StartsWith("OptiPatcher", StringComparison.OrdinalIgnoreCase) && 
                        !fileName.Equals("OptiPatcher.asi", StringComparison.OrdinalIgnoreCase))
                    {
                        var targetName = Path.Combine(Path.GetDirectoryName(sourceRoot) ?? Path.GetTempPath(), "OptiPatcher.asi");
                        Debug.WriteLine($"[ModInstaller] Target normalized name: {targetName}");
                        
                        try
                        {
                            if (File.Exists(targetName)) 
                            {
                                File.Delete(targetName);
                                Debug.WriteLine($"[ModInstaller] Deleted existing target file");
                            }
                            
                            File.Copy(sourceRoot, targetName, true);
                            sourceRoot = targetName; // Update sourceRoot to the normalized file
                            Debug.WriteLine($"[ModInstaller] SUCCESS: Renamed single ASI file to: {targetName}");
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"[ModInstaller] ERROR: Failed to rename ASI file: {ex.Message}");
                            Debug.WriteLine($"[ModInstaller] Stack trace: {ex.StackTrace}");
                        }
                    }
                    else if (fileName.Equals("OptiPatcher.asi", StringComparison.OrdinalIgnoreCase))
                    {
                        Debug.WriteLine($"[ModInstaller] File already has correct name: OptiPatcher.asi");
                    }
                    else
                    {
                        Debug.WriteLine($"[ModInstaller] File doesn't start with OptiPatcher: {fileName}");
                    }
                }
                else
                {
                    Debug.WriteLine($"[ModInstaller] Handling directory case for normalization");
                    // Handle directory case (extracted files)
                    foreach (var file in Directory.GetFiles(sourceRoot, "*.asi", SearchOption.AllDirectories))
                    {
                        var name = Path.GetFileName(file);
                        Debug.WriteLine($"[ModInstaller] Found ASI file in directory: {name}");
                        
                        if (name.StartsWith("OptiPatcher", StringComparison.OrdinalIgnoreCase) && 
                            !name.Equals("OptiPatcher.asi", StringComparison.OrdinalIgnoreCase))
                        {
                            var targetName = Path.Combine(Path.GetDirectoryName(file)!, "OptiPatcher.asi");
                            try
                            {
                                if (File.Exists(targetName)) File.Delete(targetName);
                                File.Move(file, targetName);
                                Debug.WriteLine($"[ModInstaller] SUCCESS: Renamed ASI file: {name} -> OptiPatcher.asi");
                                break;
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"[ModInstaller] ERROR: Failed to rename ASI file {name}: {ex.Message}");
                            }
                        }
                    }
                }
                
                Debug.WriteLine($"[ModInstaller] === OPTIPATCHER NORMALIZATION END ===");
                Debug.WriteLine($"[ModInstaller] Final sourceRoot after normalization: {sourceRoot}");
            }

            ReportProgress(modType, game.Name, ModOperation.Install, 4, 5, "Finding mod files...");
            Debug.WriteLine($"[ModInstaller] Searching for mod files in: {sourceRoot}");
            Debug.WriteLine($"[ModInstaller] Source root is file: {File.Exists(sourceRoot)}");
            Debug.WriteLine($"[ModInstaller] Source root is directory: {Directory.Exists(sourceRoot)}");
            
            var modFiles = FindModFilesInDirectory(sourceRoot, modType);
            Debug.WriteLine($"[ModInstaller] Found {modFiles.Count} mod files for {modType}");
            
            if (modFiles.Count == 0)
            {
                Debug.WriteLine($"[ModInstaller] ERROR: No mod files found!");
                Debug.WriteLine($"[ModInstaller] Searched in: {sourceRoot}");
                Debug.WriteLine($"[ModInstaller] Mod type: {modType}");
                
                // Additional debugging for OptiPatcher
                if (modType == ModType.OptiPatcher)
                {
                    Debug.WriteLine($"[ModInstaller] OptiPatcher Debug - Source is ASI file: {sourceRoot.EndsWith(".asi", StringComparison.OrdinalIgnoreCase)}");
                    if (File.Exists(sourceRoot))
                    {
                        Debug.WriteLine($"[ModInstaller] OptiPatcher Debug - File exists, size: {new FileInfo(sourceRoot).Length} bytes");
                        Debug.WriteLine($"[ModInstaller] OptiPatcher Debug - File name: {Path.GetFileName(sourceRoot)}");
                    }
                }
                
                result.Success = false;
                result.ErrorMessage = $"No {modType} files found after extraction. Source: {sourceRoot}";
                return result;
            }
            
            foreach (var file in modFiles)
            {
                Debug.WriteLine($"[ModInstaller] - {file}");
            }
            
            // Determine the correct installation directory
            string installDir;
            
            // First, use InstallDirectory if set (directory containing the .exe)
            if (!string.IsNullOrEmpty(game.InstallDirectory) && Directory.Exists(game.InstallDirectory))
            {
                installDir = game.InstallDirectory;
                Debug.WriteLine($"[ModInstaller] Using InstallDirectory: {installDir}");
            }
            // Fallback to exe directory
            else if (!string.IsNullOrEmpty(game.Executable) && File.Exists(game.Executable))
            {
                installDir = Path.GetDirectoryName(game.Executable) ?? game.Path;
                Debug.WriteLine($"[ModInstaller] Using exe directory: {installDir}");
            }
            // Last resort: use Path
            else
            {
                installDir = game.Path;
                Debug.WriteLine($"[ModInstaller] Using game Path: {installDir}");
            }
            
            if (!Directory.Exists(installDir))
            {
                result.Success = false;
                result.ErrorMessage = $"Game install directory not found: {installDir}";
                return result;
            }

            Debug.WriteLine($"[ModInstaller] Final install directory: {installDir}");
            Debug.WriteLine($"[ModInstaller] Mod type: {modType}");

            // OptiScaler: Install DLLs in the same directory as the game executable
            // OptiPatcher: Install into plugins subfolder (ASI loader convention)
            string targetBase;
            
            if (modType == ModType.OptiPatcher)
            {
                // OptiPatcher goes into plugins subfolder
                targetBase = Path.Combine(installDir, "plugins");
                
                // Ensure plugins directory exists
                if (!Directory.Exists(targetBase))
                {
                    Directory.CreateDirectory(targetBase);
                    Debug.WriteLine($"[ModInstaller] Created plugins directory: {targetBase}");
                }
                else
                {
                    Debug.WriteLine($"[ModInstaller] Plugins directory already exists: {targetBase}");
                }
                
                Debug.WriteLine($"[ModInstaller] OptiPatcher target directory: {targetBase}");
            }
            else
            {
                // OptiScaler goes directly into the executable directory
                targetBase = installDir;
                Debug.WriteLine($"[ModInstaller] OptiScaler target directory: {targetBase}");
            }

            ReportProgress(modType, game.Name, ModOperation.Install, 5, 5, "Installing files...");
            
            foreach (var sourceFile in modFiles)
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                string relativePath;
                string destFile;
                
                // Handle different cases for relative path calculation
                if (File.Exists(sourceRoot))
                {
                    // sourceRoot is a file, so use just the filename
                    relativePath = Path.GetFileName(sourceFile);
                    destFile = Path.Combine(targetBase, relativePath);
                    Debug.WriteLine($"[ModInstaller] File-based source: Using filename as relative path: {relativePath}");
                }
                else
                {
                    // sourceRoot is a directory, calculate relative path normally
                    relativePath = Path.GetRelativePath(sourceRoot, sourceFile);
                    destFile = Path.Combine(targetBase, relativePath);
                    Debug.WriteLine($"[ModInstaller] Directory-based source: Calculated relative path: {relativePath}");
                }
                
                // RENAME OptiScaler.dll to preferred DLL name if this is OptiScaler mod
                if (modType == ModType.OptiScaler && Path.GetFileName(sourceFile).Equals("OptiScaler.dll", StringComparison.OrdinalIgnoreCase))
                {
                    var originalDestFile = destFile;
                    destFile = Path.Combine(Path.GetDirectoryName(destFile) ?? targetBase, preferredDllName);
                    Debug.WriteLine($"[ModInstaller] Renaming OptiScaler.dll to {preferredDllName}");
                    Debug.WriteLine($"[ModInstaller]   Original: {originalDestFile}");
                    Debug.WriteLine($"[ModInstaller]   Renamed:  {destFile}");
                }
                
                // Create destination directory if it doesn't exist
                var destDir = Path.GetDirectoryName(destFile);
                if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir))
                {
                    Directory.CreateDirectory(destDir);
                    Debug.WriteLine($"[ModInstaller] Created directory: {destDir}");
                }

                Debug.WriteLine($"[ModInstaller] Copying {Path.GetFileName(sourceFile)} to {destFile}");

                // Backup existing file if it exists
                if (File.Exists(destFile))
                {
                    var backupFile = $"{destFile}.backup";
                    try 
                    { 
                        File.Copy(destFile, backupFile, true);
                        Debug.WriteLine($"[ModInstaller] Backed up existing file to {backupFile}");
                    } 
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[ModInstaller] Failed to backup {destFile}: {ex.Message}");
                    }
                }

                File.Copy(sourceFile, destFile, true);
                result.AffectedFiles.Add(destFile);
                Debug.WriteLine($"[ModInstaller] Successfully copied {Path.GetFileName(sourceFile)}");
            }

            Debug.WriteLine($"[ModInstaller] Installed {result.AffectedFiles.Count} files");
            result.Success = true;
            
            // Apply global settings to newly installed OptiScaler
            if (modType == ModType.OptiScaler)
            {
                await ApplyGlobalSettingsToGame(game);
            }
            
            // Clean up temporary extraction directory if we created one
            if (sourceRoot.StartsWith(Path.GetTempPath()))
            {
                try
                {
                    Directory.Delete(sourceRoot, true);
                    Debug.WriteLine($"[ModInstaller] Cleaned up temporary directory: {sourceRoot}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[ModInstaller] Failed to clean up temp directory: {ex.Message}");
                }
            }
        }
        catch (OperationCanceledException)
        {
            result.Success = false;
            result.ErrorMessage = "Installation cancelled";
        }
        catch (Exception ex)
        {
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
            Debug.WriteLine($"[ModInstaller] Uninstalling {modType} from {game.Name}");

            // Load global settings to know which DLL name was used
            var globalSettings = await _globalSettingsService.LoadSettingsAsync();

            // Get installation directory (where the .exe is located)
            var installDir = !string.IsNullOrEmpty(game.InstallDirectory) ? game.InstallDirectory : game.Path;
            Debug.WriteLine($"[ModInstaller] Uninstall directory: {installDir}");

            var filesToDelete = new List<string>();
            var directoriesToRemove = new List<string>();

            if (modType == ModType.OptiScaler)
            {
                // OptiScaler: Remove all files and directories that were installed
                await Task.Run(() =>
                {
                    // Add common DLL names (in case settings changed or manual install)
                    var commonDllNames = new[] 
                    { 
                        "dxgi.dll", "d3d12.dll", "winmm.dll", "version.dll", 
                        "dbghelp.dll", "wininet.dll", "winhttp.dll", "OptiScaler.asi",
                        "OptiScaler.dll" // Also check original name
                    };

                    foreach (var dllName in commonDllNames)
                    {
                        var dllPath = Path.Combine(installDir, dllName);
                        if (File.Exists(dllPath))
                        {
                            // Check if this DLL is actually OptiScaler (has OptiScaler signature or size)
                            try
                            {
                                var fileInfo = new FileInfo(dllPath);
                                // OptiScaler DLL is typically >1MB
                                if (fileInfo.Length > 500000) // 500KB threshold
                                {
                                    filesToDelete.Add(dllPath);
                                    Debug.WriteLine($"[ModInstaller] Will remove OptiScaler DLL: {dllPath}");
                                }
                            }
                            catch
                            {
                                // If we can't check, skip it
                            }
                        }
                    }

                    // Find all OptiScaler files recursively
                    var patterns = new[] { "*.dll", "*.ini", "*.reg", "*.txt", "*.md", "*.bat", "*.sh" };
                    
                    foreach (var pattern in patterns)
                    {
                        var files = Directory.GetFiles(installDir, pattern, SearchOption.AllDirectories);
                        foreach (var file in files)
                        {
                            var fileName = Path.GetFileName(file);
                            var relativePath = Path.GetRelativePath(installDir, file);
                            
                            // Check if this is an OptiScaler file
                            if (IsOptiScalerFile(fileName, relativePath))
                            {
                                if (!filesToDelete.Contains(file))
                                {
                                    filesToDelete.Add(file);
                                    Debug.WriteLine($"[ModInstaller] Will remove OptiScaler file: {file}");
                                }
                            }
                        }
                    }
                    
                    // Find OptiScaler directories to remove
                    var optiScalerDirs = new[] { "Licenses", "DlssOverrides", "D3D12_Optiscaler" };
                    foreach (var dirName in optiScalerDirs)
                    {
                        var dirPath = Path.Combine(installDir, dirName);
                        if (Directory.Exists(dirPath))
                        {
                            directoriesToRemove.Add(dirPath);
                            Debug.WriteLine($"[ModInstaller] Will remove OptiScaler directory: {dirPath}");
                        }
                    }
                }, cancellationToken);
            }
            else if (modType == ModType.OptiPatcher)
            {
                // OptiPatcher: Look in both root directory and plugins subfolder
                await Task.Run(() =>
                {
                    var optiPatcherFiles = new[] { "OptiPatcher.asi", "nvngx_patch.dll" };
                    
                    // Check root directory first
                    foreach (var fileName in optiPatcherFiles)
                    {
                        var filePath = Path.Combine(installDir, fileName);
                        if (File.Exists(filePath))
                        {
                            filesToDelete.Add(filePath);
                            Debug.WriteLine($"[ModInstaller] Will remove OptiPatcher file from root: {filePath}");
                        }
                    }
                    
                    // Check plugins directory
                    var pluginsDir = Path.Combine(installDir, "plugins");
                    if (Directory.Exists(pluginsDir))
                    {
                        foreach (var fileName in optiPatcherFiles)
                        {
                            var filePath = Path.Combine(pluginsDir, fileName);
                            if (File.Exists(filePath))
                            {
                                filesToDelete.Add(filePath);
                                Debug.WriteLine($"[ModInstaller] Will remove OptiPatcher file from plugins: {filePath}");
                            }
                        }
                        
                        // Check if plugins directory will be empty after removal
                        var remainingFiles = Directory.GetFiles(pluginsDir).Where(f => !filesToDelete.Contains(f)).ToList();
                        var remainingDirs = Directory.GetDirectories(pluginsDir);
                        
                        if (!remainingFiles.Any() && !remainingDirs.Any())
                        {
                            directoriesToRemove.Add(pluginsDir);
                            Debug.WriteLine($"[ModInstaller] Will remove empty plugins directory: {pluginsDir}");
                        }
                        else
                        {
                            Debug.WriteLine($"[ModInstaller] Plugins directory will not be removed - has {remainingFiles.Count} remaining files and {remainingDirs.Length} subdirectories");
                        }
                    }
                    
                    // Also check for any OptiPatcher*.asi files recursively
                    var allAsiFiles = Directory.GetFiles(installDir, "*.asi", SearchOption.AllDirectories);
                    foreach (var asiFile in allAsiFiles)
                    {
                        var fileName = Path.GetFileName(asiFile);
                        if (fileName.StartsWith("OptiPatcher", StringComparison.OrdinalIgnoreCase))
                        {
                            if (!filesToDelete.Contains(asiFile))
                            {
                                filesToDelete.Add(asiFile);
                                Debug.WriteLine($"[ModInstaller] Will remove OptiPatcher variant: {asiFile}");
                            }
                        }
                    }
                }, cancellationToken);
            }

            if (filesToDelete.Count == 0 && directoriesToRemove.Count == 0)
            {
                result.Success = false;
                result.ErrorMessage = $"No {modType} files found to remove";
                Debug.WriteLine($"[ModInstaller] No {modType} files found for removal");
                return result;
            }

            ReportProgress(modType, game.Name, ModOperation.Uninstall, 2, 3, "Removing files and folders...");
            Debug.WriteLine($"[ModInstaller] Removing {filesToDelete.Count} files and {directoriesToRemove.Count} directories");

            await Task.Run(() =>
            {
                // Remove files first
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
                        // Try to remove read-only attribute if present
                        if (File.Exists(file))
                        {
                            var fileInfo = new FileInfo(file);
                            if (fileInfo.Attributes.HasFlag(FileAttributes.ReadOnly))
                            {
                                fileInfo.Attributes &= ~FileAttributes.ReadOnly;
                                Debug.WriteLine($"[ModInstaller] Removed read-only attribute from file: {file}");
                            }
                        }
                        
                        // Check if backup exists and restore it
                        var backupFile = $"{file}.backup";
                        if (File.Exists(backupFile))
                        {
                            File.Copy(backupFile, file, true);
                            File.Delete(backupFile);
                            Debug.WriteLine($"[ModInstaller] Restored backup for: {file}");
                        }
                        else
                        {
                            File.Delete(file);
                            Debug.WriteLine($"[ModInstaller] Deleted file: {file}");
                        }

                        result.AffectedFiles.Add(file);
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        Debug.WriteLine($"[ModInstaller] Permission denied deleting file {file}: {ex.Message}");
                        if (string.IsNullOrEmpty(result.ErrorMessage))
                        {
                            result.ErrorMessage = $"Permission denied. Try running as Administrator to delete mod files.";
                        }
                        // Continue with other files even if one fails
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[ModInstaller] Error deleting file {file}: {ex.Message}");
                    }
                }
                
                // Remove empty directories
                foreach (var directory in directoriesToRemove)
                {
                    try
                    {
                        if (Directory.Exists(directory))
                        {
                            // Double-check that directory is actually empty before deleting
                            var files = Directory.GetFiles(directory, "*", SearchOption.AllDirectories);
                            var dirs = Directory.GetDirectories(directory);
                            
                            if (files.Length == 0 && dirs.Length == 0)
                            {
                                // Try to remove read-only attribute if present
                                var dirInfo = new DirectoryInfo(directory);
                                if (dirInfo.Attributes.HasFlag(FileAttributes.ReadOnly))
                                {
                                    dirInfo.Attributes &= ~FileAttributes.ReadOnly;
                                    Debug.WriteLine($"[ModInstaller] Removed read-only attribute from directory: {directory}");
                                }
                                
                                Directory.Delete(directory, true);
                                Debug.WriteLine($"[ModInstaller] Deleted empty directory: {directory}");
                                result.AffectedFiles.Add(directory);
                            }
                            else
                            {
                                Debug.WriteLine($"[ModInstaller] Directory not empty, skipping deletion: {directory} ({files.Length} files, {dirs.Length} subdirs)");
                            }
                        }
                        else
                        {
                            Debug.WriteLine($"[ModInstaller] Directory already doesn't exist: {directory}");
                        }
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        Debug.WriteLine($"[ModInstaller] Permission denied deleting directory {directory}: {ex.Message}");
                        result.ErrorMessage = $"Permission denied. Try running as Administrator to delete: {Path.GetFileName(directory)}";
                        // Don't mark as complete failure for permission issues
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[ModInstaller] Error deleting directory {directory}: {ex.Message}");
                    }
                }
            }, cancellationToken);

            ReportProgress(modType, game.Name, ModOperation.Uninstall, 3, 3, "Uninstallation complete");

            result.Success = true;
            Debug.WriteLine($"[ModInstaller] Successfully uninstalled {modType}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ModInstaller] Error uninstalling mod: {ex.Message}");
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
                var installResult = await InstallModAsync(game, modType, downloadResult.Value.FilePath, null, cancellationToken);
                
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
        var installDir = !string.IsNullOrEmpty(game.InstallDirectory) ? game.InstallDirectory : game.Path;

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
                    var filePath = Path.Combine(installDir, pattern);
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

        var installDir = !string.IsNullOrEmpty(game.InstallDirectory) ? game.InstallDirectory : game.Path;

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
                var filePath = Path.Combine(installDir, pattern);
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
            "OptiScaler Manager", "Backups", game.Name, DateTime.Now.ToString("yyyyMMdd_HHmms"));

        var installDir = !string.IsNullOrEmpty(game.InstallDirectory) ? game.InstallDirectory : game.Path;

        await Task.Run(() =>
        {
            Directory.CreateDirectory(backupDir);

            // Backup all known mod files that exist
            foreach (var patterns in ModFilePatterns.Values)
            {
                foreach (var pattern in patterns)
                {
                    var sourceFile = Path.Combine(installDir, pattern);
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

        var installDir = !string.IsNullOrEmpty(game.InstallDirectory) ? game.InstallDirectory : game.Path;

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
                    var destFile = Path.Combine(installDir, fileName);
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

        // For OptiScaler, copy ALL relevant files, not just the patterns
        if (modType == ModType.OptiScaler)
        {
            // Copy all DLL files
            var allDlls = Directory.GetFiles(directory, "*.dll", SearchOption.AllDirectories);
            modFiles.AddRange(allDlls);
            
            // Copy all .ini files
            var allInis = Directory.GetFiles(directory, "*.ini", SearchOption.AllDirectories);
            modFiles.AddRange(allInis);
            
            // Copy all .reg files
            var allRegs = Directory.GetFiles(directory, "*.reg", SearchOption.AllDirectories);
            modFiles.AddRange(allRegs);
            
            // Copy license files (txt, md)
            var allLicenses = Directory.GetFiles(directory, "*.txt", SearchOption.AllDirectories);
            modFiles.AddRange(allLicenses);
            
            var allMarkdown = Directory.GetFiles(directory, "*.md", SearchOption.AllDirectories);
            modFiles.AddRange(allMarkdown);
            
            // Copy setup scripts (bat, sh)
            var allBatch = Directory.GetFiles(directory, "*.bat", SearchOption.AllDirectories);
            modFiles.AddRange(allBatch);
            
            var allShell = Directory.GetFiles(directory, "*.sh", SearchOption.AllDirectories);
            modFiles.AddRange(allShell);
        }
        else if (modType == ModType.OptiPatcher)
        {
            Debug.WriteLine($"[ModInstaller] Looking for OptiPatcher files in: {directory}");
            Debug.WriteLine($"[ModInstaller] Directory parameter is file: {File.Exists(directory)}");
            Debug.WriteLine($"[ModInstaller] Directory parameter is directory: {Directory.Exists(directory)}");
            
            // Handle single file case (sourceRoot is the .asi file itself)
            if (File.Exists(directory) && directory.EndsWith(".asi", StringComparison.OrdinalIgnoreCase))
            {
                var fileName = Path.GetFileName(directory);
                Debug.WriteLine($"[ModInstaller] Single ASI file detected: {fileName}");
                Debug.WriteLine($"[ModInstaller] File size: {new FileInfo(directory).Length} bytes");
                
                // Check if it's an OptiPatcher file
                if (fileName.StartsWith("OptiPatcher", StringComparison.OrdinalIgnoreCase))
                {
                    modFiles.Add(directory);
                    Debug.WriteLine($"[ModInstaller] Added OptiPatcher ASI file: {fileName}");
                }
                else
                {
                    Debug.WriteLine($"[ModInstaller] ASI file doesn't start with OptiPatcher: {fileName}");
                }
            }
            else if (Directory.Exists(directory))
            {
                // Handle directory case - look for all .asi files that start with OptiPatcher
                var allAsiFiles = Directory.GetFiles(directory, "*.asi", SearchOption.AllDirectories);
                Debug.WriteLine($"[ModInstaller] Found {allAsiFiles.Length} ASI files in directory");
                
                foreach (var asiFile in allAsiFiles)
                {
                    var fileName = Path.GetFileName(asiFile);
                    Debug.WriteLine($"[ModInstaller] Checking ASI file: {fileName}");
                    
                    // Accept any OptiPatcher ASI file (will be normalized later)
                    if (fileName.StartsWith("OptiPatcher", StringComparison.OrdinalIgnoreCase))
                    {
                        modFiles.Add(asiFile);
                        Debug.WriteLine($"[ModInstaller] Added OptiPatcher ASI file: {fileName}");
                    }
                    else
                    {
                        Debug.WriteLine($"[ModInstaller] Skipped non-OptiPatcher ASI: {fileName}");
                    }
                }
                
                // Also look for additional OptiPatcher files
                var nvngxPatchFiles = Directory.GetFiles(directory, "nvngx_patch.dll", SearchOption.AllDirectories);
                modFiles.AddRange(nvngxPatchFiles);
                
                if (nvngxPatchFiles.Any())
                {
                    Debug.WriteLine($"[ModInstaller] Found {nvngxPatchFiles.Length} nvngx_patch.dll files");
                }
            }
            else
            {
                Debug.WriteLine($"[ModInstaller] ERROR: OptiPatcher source path doesn't exist as file or directory: {directory}");
            }
            
            Debug.WriteLine($"[ModInstaller] Total OptiPatcher files found: {modFiles.Count}");
        }
        else
        {
            // For other mod types, use patterns as before
            foreach (var pattern in patterns)
            {
                var files = Directory.GetFiles(directory, pattern, SearchOption.AllDirectories);
                modFiles.AddRange(files);
            }
        }

        return modFiles.Distinct().ToList(); // Remove duplicates
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

    /// <summary>
    /// Check if a file is an OptiScaler file based on name and location
    /// </summary>
    private bool IsOptiScalerFile(string fileName, string relativePath)
    {
        var lowerFileName = fileName.ToLowerInvariant();
        var lowerRelativePath = relativePath.ToLowerInvariant();
        
        // Known OptiScaler files
        var optiScalerFiles = new[]
        {
            "optiscaler.dll", "libxess.dll", "libxess_dx11.dll", "optiscaler.ini",
            "amd_fidelityfx_vk.dll", "amd_fidelityfx_dx12.dll", "amd_fidelityfx_upscaler_dx12.dll",
            "amd_fidelityfx_framegeneration_dx12.dll", "nvngx.dll", "enablesignatureoverride.reg",
            "disablesignatureoverride.reg", "d3d12core.dll", "setup_windows.bat", "setup_linux.sh"
        };
        
        // Check direct file match
        if (optiScalerFiles.Contains(lowerFileName))
            return true;
        
        // Check if it's in OptiScaler directories
        if (lowerRelativePath.StartsWith("licenses\\") || lowerRelativePath.StartsWith("licenses/") ||
            lowerRelativePath.StartsWith("dlssoverrides\\") || lowerRelativePath.StartsWith("dlssoverrides/") ||
            lowerRelativePath.StartsWith("d3d12_optiscaler\\") || lowerRelativePath.StartsWith("d3d12_optiscaler/"))
        {
            return true;
        }
        
        return false;
    }

    /// <summary>
    /// Apply global settings to a newly installed OptiScaler game
    /// </summary>
    private async Task ApplyGlobalSettingsToGame(GameInfo game)
    {
        try
        {
            Debug.WriteLine($"[ModInstaller] Applying global settings to {game.Name}");
            
            // Load global settings
            var globalSettings = await _globalSettingsService.LoadSettingsAsync();
            
            // Create OptiScaler config with global settings applied
            var configService = new OptiScalerConfigService();
            var config = new OptiScalerConfig();
            config = _globalSettingsService.ApplyGlobalSettingsToConfig(config, globalSettings);
            
            // Write config to game directory
            var installDir = !string.IsNullOrEmpty(game.InstallDirectory) ? game.InstallDirectory : game.Path;
            var configPath = Path.Combine(installDir, "OptiScaler.ini");
            configService.WriteConfig(configPath, config);
            
            Debug.WriteLine($"[ModInstaller] Applied global settings and created {configPath}");
            
            // Update game info with new configuration
            game.UpscalingMethod = config.GetUpscalerDisplayName();
            game.HasFrameGeneration = config.IsFrameGenerationEnabled();
            game.QualityPreset = config.GetQualityPresetName();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ModInstaller] Error applying global settings: {ex.Message}");
            // Don't fail the installation if global settings application fails
        }
    }

    #endregion
}
