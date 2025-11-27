using OptiScaler.Core.Models;

namespace OptiScaler.Core.Contracts;

/// <summary>
/// Service for interacting with GitHub API to fetch mod releases
/// </summary>
public interface IGitHubService
{
    /// <summary>
    /// Event raised when download progress updates
    /// </summary>
    event EventHandler<DownloadProgressEventArgs>? DownloadProgress;

    /// <summary>
    /// Get the latest release for a repository
    /// </summary>
    /// <param name="owner">Repository owner</param>
    /// <param name="repo">Repository name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Latest release information</returns>
    Task<GitHubRelease?> GetLatestReleaseAsync(string owner, string repo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all releases for a repository
    /// </summary>
    /// <param name="owner">Repository owner</param>
    /// <param name="repo">Repository name</param>
    /// <param name="includePrerelease">Whether to include pre-releases</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of releases</returns>
    Task<IEnumerable<GitHubRelease>> GetReleasesAsync(string owner, string repo, bool includePrerelease = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Download a release asset to a temporary location
    /// </summary>
    /// <param name="asset">Asset to download</param>
    /// <param name="destinationPath">Destination file path</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Path to downloaded file</returns>
    Task<string> DownloadAssetAsync(GitHubAsset asset, string destinationPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Download the latest release for a mod type
    /// </summary>
    /// <param name="modType">Type of mod to download</param>
    /// <param name="destinationPath">Destination file path</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Path to downloaded file and release information</returns>
    Task<(string FilePath, GitHubRelease Release)?> DownloadLatestModAsync(ModType modType, string destinationPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the default repository for a mod type
    /// </summary>
    /// <param name="modType">Type of mod</param>
    /// <returns>Repository owner and name</returns>
    (string Owner, string Repo) GetModRepository(ModType modType);
}

/// <summary>
/// Event arguments for download progress
/// </summary>
public class DownloadProgressEventArgs : EventArgs
{
    public long BytesReceived { get; set; }
    public long TotalBytes { get; set; }
    public double ProgressPercentage => TotalBytes > 0 ? (double)BytesReceived / TotalBytes * 100 : 0;
    public string FileName { get; set; } = string.Empty;
    public double SpeedBytesPerSecond { get; set; }

    public string FormattedSpeed
    {
        get
        {
            string[] sizes = { "B/s", "KB/s", "MB/s", "GB/s" };
            double speed = SpeedBytesPerSecond;
            int order = 0;
            while (speed >= 1024 && order < sizes.Length - 1)
            {
                order++;
                speed = speed / 1024;
            }
            return $"{speed:0.##} {sizes[order]}";
        }
    }
}
