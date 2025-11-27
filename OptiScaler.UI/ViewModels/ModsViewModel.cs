using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using OptiScaler.Core.Models;
using OptiScaler.Core.Services;
using OptiScaler.Core.Contracts;
using SharpCompress.Archives;
using SharpCompress.Common;

#nullable enable

namespace OptiScaler.UI.ViewModels;

/// <summary>
/// ViewModel for the Mods page (download and manage mod versions)
/// </summary>
public partial class ModsViewModel : ObservableObject
{
    private readonly GitHubService _githubService;
    private readonly StorageService _storage;
    private readonly DispatcherQueue? _dispatcherQueue;

    // All releases
    [ObservableProperty]
    private ObservableCollection<ReleaseItem> _optiScalerReleases = new();

    [ObservableProperty]
    private ObservableCollection<ReleaseItem> _optiPatcherReleases = new();

    // Displayed (paged) releases
    [ObservableProperty]
    private ObservableCollection<ReleaseItem> _displayedOptiScalerReleases = new();

    [ObservableProperty]
    private ObservableCollection<ReleaseItem> _displayedOptiPatcherReleases = new();

    [ObservableProperty]
    private bool _showAllOptiScaler;

    [ObservableProperty]
    private bool _showAllOptiPatcher;

    [ObservableProperty]
    private bool _isOptiScalerExpanded = false;

    [ObservableProperty]
    private bool _isOptiPatcherExpanded = false;

    [ObservableProperty]
    private ReleaseItem? _selectedOptiScalerRelease;

    [ObservableProperty]
    private ReleaseItem? _selectedOptiPatcherRelease;

    [ObservableProperty]
    private bool _isLoadingReleases;

    [ObservableProperty]
    private bool _isDownloading;

    [ObservableProperty]
    private string _statusMessage = "Click 'Check for Updates' to load available mod versions";

    [ObservableProperty]
    private double _downloadProgress;

    [ObservableProperty]
    private string _downloadSpeed = string.Empty;

    [ObservableProperty]
    private bool _hasNoReleases = true;

    public bool HasOptiScalerReleases => OptiScalerReleases.Count > 0;
    public bool HasOptiPatcherReleases => OptiPatcherReleases.Count > 0;

    public string OptiScalerDownloadedInfo
    {
        get
        {
            var downloaded = OptiScalerReleases
                .Where(r => r.IsDownloaded)
                .OrderByDescending(r => r.Release.PublishedAt)
                .FirstOrDefault();
            
            var latest = OptiScalerReleases.FirstOrDefault(); // Already ordered by date from API
            
            // If we have downloaded files
            if (downloaded != null)
            {
                // Check if there's a newer version available
                if (latest != null && latest.Release.PublishedAt > downloaded.Release.PublishedAt)
                {
                    var latestDate = latest.Release.PublishedAt.ToString("MMM dd, yyyy");
                    return $"New update available: {latest.Release.TagName} - {latestDate}";
                }
                else
                {
                    // Already have the latest
                    var date = downloaded.Release.PublishedAt.ToString("MMM dd, yyyy");
                    return $"{downloaded.Release.TagName} - {date}";
                }
            }
            
            // No downloads, check if there are releases available
            if (latest != null)
            {
                var latestDate = latest.Release.PublishedAt.ToString("MMM dd, yyyy");
                return $"New update available: {latest.Release.TagName} - {latestDate}";
            }
            
            return string.Empty;
        }
    }

    public string OptiPatcherDownloadedInfo
    {
        get
        {
            var downloaded = OptiPatcherReleases
                .Where(r => r.IsDownloaded)
                .OrderByDescending(r => r.Release.PublishedAt)
                .FirstOrDefault();
            
            var latest = OptiPatcherReleases.FirstOrDefault(); // Already ordered by date from API
            
            // If we have downloaded files
            if (downloaded != null)
            {
                // Check if there's a newer version available
                if (latest != null && latest.Release.PublishedAt > downloaded.Release.PublishedAt)
                {
                    var latestDate = latest.Release.PublishedAt.ToString("MMM dd, yyyy");
                    return $"New update available: {latest.Release.TagName} - {latestDate}";
                }
                else
                {
                    // Already have the latest
                    var date = downloaded.Release.PublishedAt.ToString("MMM dd, yyyy");
                    return $"{downloaded.Release.TagName} - {date}";
                }
            }
            
            // No downloads, check if there are releases available
            if (latest != null)
            {
                var latestDate = latest.Release.PublishedAt.ToString("MMM dd, yyyy");
                return $"New update available: {latest.Release.TagName} - {latestDate}";
            }
            
            return string.Empty;
        }
    }

    public bool HasOptiScalerDownloaded => !string.IsNullOrEmpty(OptiScalerDownloadedInfo);
    public bool HasOptiPatcherDownloaded => !string.IsNullOrEmpty(OptiPatcherDownloadedInfo);

    public bool IsOptiScalerUpdateAvailable
    {
        get
        {
            var downloaded = OptiScalerReleases
                .Where(r => r.IsDownloaded)
                .OrderByDescending(r => r.Release.PublishedAt)
                .FirstOrDefault();
            
            var latest = OptiScalerReleases.FirstOrDefault();
            
            if (latest == null) return false;
            if (downloaded == null) return true; // New release available, nothing downloaded
            
            return latest.Release.PublishedAt > downloaded.Release.PublishedAt;
        }
    }

    public bool IsOptiPatcherUpdateAvailable
    {
        get
        {
            var downloaded = OptiPatcherReleases
                .Where(r => r.IsDownloaded)
                .OrderByDescending(r => r.Release.PublishedAt)
                .FirstOrDefault();
            
            var latest = OptiPatcherReleases.FirstOrDefault();
            
            if (latest == null) return false;
            if (downloaded == null) return true; // New release available, nothing downloaded
            
            return latest.Release.PublishedAt > downloaded.Release.PublishedAt;
        }
    }

    public ModsViewModel()
    {
        _githubService = new GitHubService();
        _storage = new StorageService();
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        // Subscribe to download progress
        _githubService.DownloadProgress += OnDownloadProgress;

        // Subscribe to collection changes
        OptiScalerReleases.CollectionChanged += (s, e) => UpdateHasNoReleases();
        OptiPatcherReleases.CollectionChanged += (s, e) => UpdateHasNoReleases();

        // Load cached releases on startup
        _ = LoadCachedReleasesAsync();
    }

    private async Task LoadCachedReleasesAsync()
    {
        try
        {
            // Try to load cached releases from previous session
            var cachedReleases = await _storage.LoadReleasesAsync();
            
            if (cachedReleases.HasValue)
            {
                var (optiReleases, patcherReleases) = cachedReleases.Value;
                
                OptiScalerReleases.Clear();
                foreach (var r in optiReleases)
                    OptiScalerReleases.Add(new ReleaseItem(r, ModType.OptiScaler));

                OptiPatcherReleases.Clear();
                foreach (var r in patcherReleases)
                    OptiPatcherReleases.Add(new ReleaseItem(r, ModType.OptiPatcher));

                UpdateDisplayedOptiScaler();
                UpdateDisplayedOptiPatcher();

                StatusMessage = $"Loaded {OptiScalerReleases.Count} OptiScaler and {OptiPatcherReleases.Count} OptiPatcher releases from cache";
                return;
            }

            // Scan downloads folder for downloaded files
            var optiDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OptiScaler Manager", "Mods", "OptiScaler");
            var patcherDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OptiScaler Manager", "Mods", "OptiPatcher");

            bool hasDownloaded = false;

            if (Directory.Exists(optiDir))
            {
                var files = Directory.GetFiles(optiDir, "*.*")
                    .Where(f => f.EndsWith(".7z", StringComparison.OrdinalIgnoreCase) || 
                                f.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                    .ToList();
                hasDownloaded = files.Any();
            }

            if (!hasDownloaded && Directory.Exists(patcherDir))
            {
                var files = Directory.GetFiles(patcherDir, "*.asi", SearchOption.TopDirectoryOnly);
                hasDownloaded = files.Any();
            }

            StatusMessage = hasDownloaded 
                ? "Found downloaded mods. Click 'Check for Updates' to see all available versions."
                : "Click 'Check for Updates' to load available mod versions";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading cache: {ex.Message}";
        }
    }

    public class ReleaseItem : ObservableObject
    {
        public GitHubRelease Release { get; }
        public ModType ModType { get; }
        public string DownloadsDir { get; }

        private string _localPath = string.Empty;
        public string LocalPath
        {
            get => _localPath;
            set
            {
                SetProperty(ref _localPath, value);
                OnPropertyChanged(nameof(IsDownloaded));
            }
        }

        public bool IsDownloaded => !string.IsNullOrEmpty(LocalPath) && File.Exists(LocalPath);

        public string DisplayTitle => !string.IsNullOrEmpty(Release.TagName) ? Release.TagName : Release.Name;

        public ReleaseItem(GitHubRelease release, ModType modType)
        {
            Release = release;
            ModType = modType;
            DownloadsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OptiScaler Manager", "Mods", modType.ToString());
            Directory.CreateDirectory(DownloadsDir);
            // Determine local path if already downloaded
            var asset = SelectPreferredAsset(release, modType);
            if (asset != null)
            {
                var candidate = Path.Combine(DownloadsDir, asset.Name);
                if (File.Exists(candidate))
                    LocalPath = candidate;
            }
        }

        public static GitHubAsset? SelectPreferredAsset(GitHubRelease release, ModType modType)
        {
            if (modType == ModType.OptiScaler)
            {
                return release.Assets.FirstOrDefault(a => a.Name.EndsWith(".7z", StringComparison.OrdinalIgnoreCase))
                    ?? release.Assets.FirstOrDefault(a => a.Name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                    ?? release.Assets.FirstOrDefault();
            }
            else if (modType == ModType.OptiPatcher)
            {
                return release.Assets.FirstOrDefault(a => a.Name.EndsWith(".asi", StringComparison.OrdinalIgnoreCase))
                    ?? release.Assets.FirstOrDefault();
            }

            return release.Assets.FirstOrDefault();
        }
    }

    [RelayCommand]
    private async Task CheckForUpdatesAsync()
    {
        if (IsLoadingReleases) return;

        try
        {
            IsLoadingReleases = true;
            StatusMessage = "Loading available mod versions from GitHub...";

            // Load OptiScaler releases
            var (optiOwner, optiRepo) = _githubService.GetModRepository(ModType.OptiScaler);
            var optiReleases = (await _githubService.GetReleasesAsync(optiOwner, optiRepo, includePrerelease: false)).ToList();

            OptiScalerReleases.Clear();
            foreach (var r in optiReleases)
                OptiScalerReleases.Add(new ReleaseItem(r, ModType.OptiScaler));

            // Load OptiPatcher releases
            var (patcherOwner, patcherRepo) = _githubService.GetModRepository(ModType.OptiPatcher);
            var patcherReleases = (await _githubService.GetReleasesAsync(patcherOwner, patcherRepo, includePrerelease: false)).ToList();

            OptiPatcherReleases.Clear();
            foreach (var r in patcherReleases)
                OptiPatcherReleases.Add(new ReleaseItem(r, ModType.OptiPatcher));

            // Save releases to cache
            await _storage.SaveReleasesAsync(
                optiReleases.Select(r => r).ToList(),
                patcherReleases.Select(r => r).ToList()
            );

            // Update displayed collections
            UpdateDisplayedOptiScaler();
            UpdateDisplayedOptiPatcher();

            StatusMessage = $"Loaded {OptiScalerReleases.Count} OptiScaler and {OptiPatcherReleases.Count} OptiPatcher releases";

            // Auto-select latest versions
            SelectedOptiScalerRelease = OptiScalerReleases.FirstOrDefault();
            SelectedOptiPatcherRelease = OptiPatcherReleases.FirstOrDefault();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading releases: {ex.Message}";
        }
        finally
        {
            IsLoadingReleases = false;
        }
    }

    private void UpdateDisplayedOptiScaler()
    {
        DisplayedOptiScalerReleases.Clear();
        var items = ShowAllOptiScaler ? OptiScalerReleases : OptiScalerReleases.Take(5);
        foreach (var it in items)
            DisplayedOptiScalerReleases.Add(it);
    }

    private void UpdateDisplayedOptiPatcher()
    {
        DisplayedOptiPatcherReleases.Clear();
        var items = ShowAllOptiPatcher ? OptiPatcherReleases : OptiPatcherReleases.Take(3);
        foreach (var it in items)
            DisplayedOptiPatcherReleases.Add(it);
    }

    partial void OnShowAllOptiScalerChanged(bool value)
    {
        UpdateDisplayedOptiScaler();
    }

    partial void OnShowAllOptiPatcherChanged(bool value)
    {
        UpdateDisplayedOptiPatcher();
    }

    [RelayCommand]
    private async Task DownloadReleaseAsync(ReleaseItem? item)
    {
        if (item == null || IsDownloading) return;

        CancellationTokenSource? cts = null;
        try
        {
            cts = new CancellationTokenSource(TimeSpan.FromMinutes(10));
            IsDownloading = true;
            DownloadProgress = 0;

            var asset = ReleaseItem.SelectPreferredAsset(item.Release, item.ModType);
            if (asset == null)
            {
                await SafeUpdateUIAsync(() =>
                {
                    StatusMessage = "No downloadable asset found for this release.";
                });
                return;
            }

            // STEP 1: Clean up old versions before downloading
            await SafeUpdateUIAsync(() =>
            {
                StatusMessage = $"Preparing to download {item.Release.TagName}... Cleaning old versions...";
            });

            await Task.Run(() =>
            {
                try
                {
                    if (Directory.Exists(item.DownloadsDir))
                    {
                        // Delete all files and extracted folders
                        var files = Directory.GetFiles(item.DownloadsDir, "*.*", SearchOption.TopDirectoryOnly);
                        foreach (var file in files)
                        {
                            try
                            {
                                File.Delete(file);
                                System.Diagnostics.Debug.WriteLine($"[ModsVM] Deleted old file: {file}");
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"[ModsVM] Could not delete {file}: {ex.Message}");
                            }
                        }

                        // Delete all extracted folders (ending with _extracted)
                        var directories = Directory.GetDirectories(item.DownloadsDir, "*_extracted", SearchOption.TopDirectoryOnly);
                        foreach (var dir in directories)
                        {
                            try
                            {
                                Directory.Delete(dir, true);
                                System.Diagnostics.Debug.WriteLine($"[ModsVM] Deleted old extracted folder: {dir}");
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"[ModsVM] Could not delete {dir}: {ex.Message}");
                            }
                        }
                    }
                    else
                    {
                        Directory.CreateDirectory(item.DownloadsDir);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[ModsVM] Error cleaning old versions: {ex.Message}");
                }
            }, cts.Token);

            // Clear LocalPath for all items of this mod type
            await SafeUpdateUIAsync(() =>
            {
                var itemsToUpdate = item.ModType == ModType.OptiScaler 
                    ? OptiScalerReleases 
                    : OptiPatcherReleases;
                
                foreach (var releaseItem in itemsToUpdate)
                {
                    releaseItem.LocalPath = string.Empty;
                }
            });

            // STEP 2: Download the new version
            StatusMessage = $"Downloading {item.Release.TagName}...";
            var destinationPath = Path.Combine(item.DownloadsDir, asset.Name);

            // Run download and extraction on background thread
            string finalPath = await Task.Run(async () =>
            {
                System.Diagnostics.Debug.WriteLine($"[ModsVM] Starting download: {asset.Name}");
                
                // Download the file
                await _githubService.DownloadAssetAsync(asset, destinationPath, cts.Token);
                System.Diagnostics.Debug.WriteLine($"[ModsVM] Download complete: {asset.Name}");
                
                // STEP 3: Extract immediately if it's an archive
                var fileExt = Path.GetExtension(destinationPath).ToLowerInvariant();
                if (fileExt == ".7z" || fileExt == ".zip")
                {
                    // Update UI to show extraction phase
                    await SafeUpdateUIAsync(() =>
                    {
                        StatusMessage = $"Extracting {item.Release.TagName}...";
                        DownloadProgress = 0; // Reset progress for extraction
                    });
                    
                    System.Diagnostics.Debug.WriteLine($"[ModsVM] Extracting archive: {destinationPath}");
                    
                    var archiveDir = Path.GetDirectoryName(destinationPath) ?? Path.GetTempPath();
                    var archiveName = Path.GetFileNameWithoutExtension(destinationPath);
                    var extractDir = Path.Combine(archiveDir, $"{archiveName}_extracted");
                    
                    // Create fresh extraction directory
                    if (Directory.Exists(extractDir))
                    {
                        Directory.Delete(extractDir, true);
                    }
                    Directory.CreateDirectory(extractDir);
                    
                    // Extract using SharpCompress
                    using var stream = File.OpenRead(destinationPath);
                    using var archive = SharpCompress.Archives.ArchiveFactory.Open(stream);
                    
                    var totalEntries = archive.Entries.Count(e => !e.IsDirectory);
                    var processedEntries = 0;
                    
                    foreach (var entry in archive.Entries.Where(e => !e.IsDirectory))
                    {
                        cts.Token.ThrowIfCancellationRequested();
                        
                        var destPath = Path.Combine(extractDir, entry.Key.Replace('/', Path.DirectorySeparatorChar));
                        var destDirPath = Path.GetDirectoryName(destPath);
                        
                        if (!string.IsNullOrEmpty(destDirPath) && !Directory.Exists(destDirPath))
                        {
                            Directory.CreateDirectory(destDirPath);
                        }
                        
                        entry.WriteToFile(destPath, new SharpCompress.Common.ExtractionOptions { ExtractFullPath = true, Overwrite = true });
                        
                        processedEntries++;
                        
                        // Update extraction progress
                        if (totalEntries > 0)
                        {
                            var extractionProgress = (processedEntries * 100.0) / totalEntries;
                            await SafeUpdateUIAsync(() =>
                            {
                                DownloadProgress = extractionProgress;
                                StatusMessage = $"Extracting {item.Release.TagName}... ({processedEntries}/{totalEntries} files)";
                            });
                        }
                    }
                    
                    System.Diagnostics.Debug.WriteLine($"[ModsVM] Extraction complete: {extractDir}");
                }
                
                return destinationPath;
            }, cts.Token);

            // Update UI on success
            await SafeUpdateUIAsync(() =>
            {
                item.LocalPath = finalPath;
                StatusMessage = $"Successfully downloaded {item.Release.TagName}. Old versions have been removed.";
                DownloadProgress = 100;
                
                // Update downloaded info in header
                OnPropertyChanged(nameof(OptiScalerDownloadedInfo));
                OnPropertyChanged(nameof(OptiPatcherDownloadedInfo));
                OnPropertyChanged(nameof(HasOptiScalerDownloaded));
                OnPropertyChanged(nameof(HasOptiPatcherDownloaded));
                OnPropertyChanged(nameof(IsOptiScalerUpdateAvailable));
                OnPropertyChanged(nameof(IsOptiPatcherUpdateAvailable));
            });
        }
        catch (OperationCanceledException)
        {
            await SafeUpdateUIAsync(() => StatusMessage = "Download cancelled or timed out.");
        }
        catch (UnauthorizedAccessException ex)
        {
            await SafeUpdateUIAsync(() => StatusMessage = $"Access denied: {ex.Message}. Check folder permissions.");
            System.Diagnostics.Debug.WriteLine($"[ModsVM] Access error: {ex}");
        }
        catch (IOException ex)
        {
            await SafeUpdateUIAsync(() => StatusMessage = $"File error: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[ModsVM] IO error: {ex}");
        }
        catch (Exception ex)
        {
            await SafeUpdateUIAsync(() => StatusMessage = $"Download failed: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[ModsVM] Error type: {ex.GetType().FullName}");
            System.Diagnostics.Debug.WriteLine($"[ModsVM] Error: {ex}");
        }
        finally
        {
            cts?.Dispose();
            await SafeUpdateUIAsync(() =>
            {
                IsDownloading = false;
                DownloadProgress = 0;
                DownloadSpeed = string.Empty;
            });
        }
    }

    private async Task SafeUpdateUIAsync(Action action)
    {
        if (_dispatcherQueue != null)
        {
            var tcs = new TaskCompletionSource<bool>();
            _dispatcherQueue.TryEnqueue(() =>
            {
                try
                {
                    action();
                    tcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[ModsVM] UI update error: {ex}");
                    tcs.SetException(ex);
                }
            });
            await tcs.Task;
        }
        else
        {
            action();
        }
    }

    [RelayCommand]
    private void DeleteDownloaded(ReleaseItem? item)
    {
        if (item == null) return;
        try
        {
            bool deletedSomething = false;

            // Delete the downloaded archive file
            if (!string.IsNullOrEmpty(item.LocalPath) && File.Exists(item.LocalPath))
            {
                File.Delete(item.LocalPath);
                System.Diagnostics.Debug.WriteLine($"[ModsVM] Deleted file: {item.LocalPath}");
                deletedSomething = true;

                // Also delete the extracted folder if it exists
                var archiveName = Path.GetFileNameWithoutExtension(item.LocalPath);
                var extractDir = Path.Combine(item.DownloadsDir, $"{archiveName}_extracted");
                
                if (Directory.Exists(extractDir))
                {
                    Directory.Delete(extractDir, true);
                    System.Diagnostics.Debug.WriteLine($"[ModsVM] Deleted extracted folder: {extractDir}");
                }

                item.LocalPath = string.Empty;
            }

            // Also check for any orphaned extracted folders
            if (Directory.Exists(item.DownloadsDir))
            {
                var extractedDirs = Directory.GetDirectories(item.DownloadsDir, "*_extracted", SearchOption.TopDirectoryOnly);
                foreach (var dir in extractedDirs)
                {
                    try
                    {
                        Directory.Delete(dir, true);
                        System.Diagnostics.Debug.WriteLine($"[ModsVM] Deleted orphaned folder: {dir}");
                        deletedSomething = true;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[ModsVM] Could not delete {dir}: {ex.Message}");
                    }
                }
            }

            if (deletedSomething)
            {
                StatusMessage = $"Deleted {item.Release.TagName} and all associated files";
            }
            else
            {
                StatusMessage = $"No files to delete for {item.Release.TagName}";
            }
            
            // Update downloaded info in header
            OnPropertyChanged(nameof(OptiScalerDownloadedInfo));
            OnPropertyChanged(nameof(OptiPatcherDownloadedInfo));
            OnPropertyChanged(nameof(HasOptiScalerDownloaded));
            OnPropertyChanged(nameof(HasOptiPatcherDownloaded));
            OnPropertyChanged(nameof(IsOptiScalerUpdateAvailable));
            OnPropertyChanged(nameof(IsOptiPatcherUpdateAvailable));
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to delete files: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"[ModsVM] Delete error: {ex}");
        }
    }

    private void OnDownloadProgress(object? sender, DownloadProgressEventArgs e)
    {
        _dispatcherQueue?.TryEnqueue(() =>
        {
            DownloadProgress = e.ProgressPercentage;
            DownloadSpeed = e.FormattedSpeed;
            StatusMessage = $"Downloading {e.FileName}: {e.ProgressPercentage:F1}% at {e.FormattedSpeed}";
        });
    }

    private void UpdateHasNoReleases()
    {
        HasNoReleases = OptiScalerReleases.Count == 0 && OptiPatcherReleases.Count == 0;
        OnPropertyChanged(nameof(HasOptiScalerReleases));
        OnPropertyChanged(nameof(HasOptiPatcherReleases));
        OnPropertyChanged(nameof(OptiScalerDownloadedInfo));
        OnPropertyChanged(nameof(OptiPatcherDownloadedInfo));
        OnPropertyChanged(nameof(HasOptiScalerDownloaded));
        OnPropertyChanged(nameof(HasOptiPatcherDownloaded));
        OnPropertyChanged(nameof(IsOptiScalerUpdateAvailable));
        OnPropertyChanged(nameof(IsOptiPatcherUpdateAvailable));
    }
}
