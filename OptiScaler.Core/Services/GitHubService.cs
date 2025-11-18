using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using OptiScaler.Core.Contracts;
using OptiScaler.Core.Models;

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
        { ModType.OptiScaler, ("cdozdil", "OptiScaler") },
        { ModType.DlssgToFsr3, ("Nukem9", "dlssg-to-fsr3") }
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
            // Ensure directory exists
            var directory = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            var startTime = DateTime.Now;
            long lastBytesReceived = 0;
            var lastUpdateTime = DateTime.Now;

            using var response = await _httpClient.GetAsync(asset.BrowserDownloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? asset.Size;

            using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
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

            return destinationPath;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error downloading asset: {ex.Message}");
            
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
        if (!ModRepositories.TryGetValue(modType, out var repo))
        {
            Debug.WriteLine($"Unknown mod type: {modType}");
            return null;
        }

        var release = await GetLatestReleaseAsync(repo.Owner, repo.Repo, cancellationToken);
        if (release == null)
        {
            Debug.WriteLine($"No release found for {modType}");
            return null;
        }

        // Find the appropriate asset to download
        // Prefer .zip files, fallback to first asset
        var asset = release.Assets.FirstOrDefault(a => a.Name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                    ?? release.Assets.FirstOrDefault();

        if (asset == null)
        {
            Debug.WriteLine($"No downloadable assets found in release");
            return null;
        }

        // Ensure destination path has proper extension
        if (string.IsNullOrEmpty(Path.GetExtension(destinationPath)))
        {
            var extension = Path.GetExtension(asset.Name);
            destinationPath = Path.ChangeExtension(destinationPath, extension);
        }

        var downloadedPath = await DownloadAssetAsync(asset, destinationPath, cancellationToken);
        return (downloadedPath, release);
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
