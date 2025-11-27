using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using OptiScaler.Core.Contracts;
using OptiScaler.Core.Models;
using SharpCompress.Archives;
using SharpCompress.Common;

namespace OptiScaler.Core.Services;

/// <summary>
/// Service for interacting with GitHub API to fetch mod releases
/// </summary>
public class GitHubService : IGitHubService, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public event EventHandler<DownloadProgressEventArgs>? DownloadProgress;

    // Default repositories for known mod types
    private static readonly Dictionary<ModType, (string Owner, string Repo)> ModRepositories = new()
    {
        { ModType.OptiScaler, ("optiscaler", "OptiScaler") },
        { ModType.OptiPatcher, ("optiscaler", "OptiPatcher") }
    };

    public GitHubService()
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", $"OptiScaler-Manager/{AppInfo.Version}");
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    /// <summary>
    /// Get the latest release for a repository
    /// </summary>
    public async Task<GitHubRelease?> GetLatestReleaseAsync(string owner, string repo, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"https://api.github.com/repos/{owner}/{repo}/releases/latest";
            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"GitHub API error: {response.StatusCode}");
                return null;
            }

            var release = await response.Content.ReadFromJsonAsync<GitHubRelease>(_jsonOptions, cancellationToken);
            return release;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error fetching latest release: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Get all releases for a repository
    /// </summary>
    public async Task<IEnumerable<GitHubRelease>> GetReleasesAsync(string owner, string repo, bool includePrerelease = false, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"https://api.github.com/repos/{owner}/{repo}/releases";
            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"GitHub API error: {response.StatusCode}");
                return Enumerable.Empty<GitHubRelease>();
            }

            var releases = await response.Content.ReadFromJsonAsync<List<GitHubRelease>>(_jsonOptions, cancellationToken);
            if (releases == null)
                return Enumerable.Empty<GitHubRelease>();

            // Filter out prereleases if requested
            if (!includePrerelease)
                releases = releases.Where(r => !r.Prerelease && !r.Draft).ToList();

            return releases.OrderByDescending(r => r.PublishedAt);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error fetching releases: {ex.Message}");
            return Enumerable.Empty<GitHubRelease>();
        }
    }

    /// <summary>
    /// Download a release asset to a temporary location
    /// </summary>
    public async Task<string> DownloadAssetAsync(GitHubAsset asset, string destinationPath, CancellationToken cancellationToken = default)
    {
        try
        {
            Debug.WriteLine($"[GitHub] Start download: {asset.Name} from {asset.BrowserDownloadUrl}");
            
            // Ensure directory exists
            var directory = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            var startTime = DateTime.Now;
            long lastBytesReceived = 0;
            var lastUpdateTime = DateTime.Now;

            Debug.WriteLine($"[GitHub] Sending GET request...");
            using var response = await _httpClient.GetAsync(asset.BrowserDownloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            
            Debug.WriteLine($"[GitHub] Response status: {response.StatusCode}");
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? asset.Size;
            Debug.WriteLine($"[GitHub] Content-Length: {totalBytes} bytes");

            using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            Debug.WriteLine($"[GitHub] Begin reading stream...");
            
            using var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

            var buffer = new byte[8192];
            long totalBytesRead = 0;
            int bytesRead;

            while ((bytesRead = await contentStream.ReadAsync(buffer, cancellationToken)) > 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                totalBytesRead += bytesRead;

                // Report progress every 100ms
                var now = DateTime.Now;
                if ((now - lastUpdateTime).TotalMilliseconds >= 100)
                {
                    var timeDiff = (now - lastUpdateTime).TotalSeconds;
                    var bytesDiff = totalBytesRead - lastBytesReceived;
                    var speed = timeDiff > 0 ? bytesDiff / timeDiff : 0;

                    DownloadProgress?.Invoke(this, new DownloadProgressEventArgs
                    {
                        BytesReceived = totalBytesRead,
                        TotalBytes = totalBytes,
                        FileName = asset.Name,
                        SpeedBytesPerSecond = speed
                    });

                    lastBytesReceived = totalBytesRead;
                    lastUpdateTime = now;
                }
            }

            // Final progress update
            DownloadProgress?.Invoke(this, new DownloadProgressEventArgs
            {
                BytesReceived = totalBytesRead,
                TotalBytes = totalBytes,
                FileName = asset.Name,
                SpeedBytesPerSecond = 0
            });

            Debug.WriteLine($"[GitHub] Download complete: {totalBytesRead} bytes written to {destinationPath}");
            return destinationPath;
        }
        catch (HttpRequestException httpEx)
        {
            Debug.WriteLine($"[GitHub] HTTP error downloading {asset.Name}: {httpEx.Message}");
            
            // Clean up partial download
            if (File.Exists(destinationPath))
            {
                try { File.Delete(destinationPath); } catch { }
            }

            throw;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[GitHub] Error downloading {asset.Name}: {ex.Message}");
            Debug.WriteLine($"[GitHub] Stack trace: {ex.StackTrace}");
            
            // Clean up partial download
            if (File.Exists(destinationPath))
            {
                try { File.Delete(destinationPath); } catch { }
            }

            throw;
        }
    }

    /// <summary>
    /// Download the latest release for a mod type
    /// </summary>
    public async Task<(string FilePath, GitHubRelease Release)?> DownloadLatestModAsync(ModType modType, string destinationPath, CancellationToken cancellationToken = default)
    {
        try
        {
            var (owner, repo) = GetModRepository(modType);
            var release = await GetLatestReleaseAsync(owner, repo, cancellationToken);
            
            if (release == null)
                return null;

            // Select preferred asset (.7z for OptiScaler, .asi for OptiPatcher)
            GitHubAsset? asset = null;
            if (modType == ModType.OptiScaler)
            {
                asset = release.Assets.FirstOrDefault(a => a.Name.EndsWith(".7z", StringComparison.OrdinalIgnoreCase))
                     ?? release.Assets.FirstOrDefault(a => a.Name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                     ?? release.Assets.FirstOrDefault();
            }
            else if (modType == ModType.OptiPatcher)
            {
                asset = release.Assets.FirstOrDefault(a => a.Name.EndsWith(".asi", StringComparison.OrdinalIgnoreCase))
                     ?? release.Assets.FirstOrDefault();
            }

            if (asset == null)
                return null;

            // Download the archive
            await DownloadAssetAsync(asset, destinationPath, cancellationToken);
            
            // Extract immediately if it's an archive
            var extractedPath = await ExtractArchiveIfNeeded(destinationPath, modType, cancellationToken);
            
            return (extractedPath, release);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Download latest mod error: {ex}");
            return null;
        }
    }

    /// <summary>
    /// Extract archive and return path to extracted content
    /// </summary>
    private async Task<string> ExtractArchiveIfNeeded(string archivePath, ModType modType, CancellationToken cancellationToken)
    {
        var fileExt = Path.GetExtension(archivePath).ToLowerInvariant();
        
        // Only extract archives, return as-is for single files
        if (fileExt != ".7z" && fileExt != ".zip")
        {
            Debug.WriteLine($"[GitHub] Not an archive, returning original path: {archivePath}");
            return archivePath;
        }

        // Create extraction directory next to archive
        var archiveDir = Path.GetDirectoryName(archivePath) ?? Path.GetTempPath();
        var archiveName = Path.GetFileNameWithoutExtension(archivePath);
        var extractDir = Path.Combine(archiveDir, $"{archiveName}_extracted");
        
        // Check if already extracted
        if (Directory.Exists(extractDir))
        {
            Debug.WriteLine($"[GitHub] Archive already extracted: {extractDir}");
            return extractDir;
        }

        Debug.WriteLine($"[GitHub] Extracting {archivePath} to {extractDir}");
        
        try
        {
            Directory.CreateDirectory(extractDir);
            
            // Use SharpCompress for extraction
            using var stream = File.OpenRead(archivePath);
            using var archive = SharpCompress.Archives.ArchiveFactory.Open(stream);
            
            await Task.Run(() =>
            {
                foreach (var entry in archive.Entries.Where(e => !e.IsDirectory))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    
                    var destPath = Path.Combine(extractDir, entry.Key.Replace('/', Path.DirectorySeparatorChar));
                    var destDirPath = Path.GetDirectoryName(destPath);
                    
                    if (!string.IsNullOrEmpty(destDirPath) && !Directory.Exists(destDirPath))
                    {
                        Directory.CreateDirectory(destDirPath);
                    }
                    
                    entry.WriteToFile(destPath, new SharpCompress.Common.ExtractionOptions { ExtractFullPath = true, Overwrite = true });
                    Debug.WriteLine($"[GitHub] Extracted: {entry.Key} -> {destPath}");
                }
            }, cancellationToken);
            
            Debug.WriteLine($"[GitHub] Extraction complete: {extractDir}");
            return extractDir;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[GitHub] Extraction failed: {ex.Message}");
            
            // Clean up failed extraction
            if (Directory.Exists(extractDir))
            {
                try { Directory.Delete(extractDir, true); } catch { }
            }
            
            // Return original archive path as fallback
            return archivePath;
        }
    }

    /// <summary>
    /// Get the default repository for a mod type
    /// </summary>
    public (string Owner, string Repo) GetModRepository(ModType modType)
    {
        if (ModRepositories.TryGetValue(modType, out var repo))
            return repo;

        return (string.Empty, string.Empty);
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}
