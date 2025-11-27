using OptiScaler.Core.Models;

namespace OptiScaler.Core.Contracts;

/// <summary>
/// Service for scanning and detecting game installations
/// </summary>
public interface IGameScannerService
{
    /// <summary>
    /// Event raised when a game is discovered during scanning
    /// </summary>
    event EventHandler<GameInfo>? GameDiscovered;
    
    /// <summary>
    /// Event raised when scanning progress updates
    /// </summary>
    event EventHandler<ScanProgressEventArgs>? ScanProgress;
    
    /// <summary>
    /// Scan all supported gaming platforms for installed games
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for stopping the scan</param>
    /// <returns>Collection of discovered games</returns>
    Task<IEnumerable<GameInfo>> ScanAllPlatformsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Scan a specific gaming platform for installed games
    /// </summary>
    /// <param name="platform">Platform to scan</param>
    /// <param name="cancellationToken">Cancellation token for stopping the scan</param>
    /// <returns>Collection of discovered games from the platform</returns>
    Task<IEnumerable<GameInfo>> ScanPlatformAsync(GamePlatform platform, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verify if a game path is valid and extract game information
    /// </summary>
    /// <param name="gamePath">Path to game directory</param>
    /// <returns>Game information if valid, null otherwise</returns>
    Task<GameInfo?> VerifyGamePathAsync(string gamePath);
    
    /// <summary>
    /// Refresh mod status for a specific game
    /// </summary>
    /// <param name="game">Game to refresh mod status for</param>
    /// <returns>Updated game information</returns>
    Task<GameInfo> RefreshModStatusAsync(GameInfo game);
}

/// <summary>
/// Event arguments for scan progress updates
/// </summary>
public class ScanProgressEventArgs : EventArgs
{
    public int TotalPlatforms { get; set; }
    public int CompletedPlatforms { get; set; }
    public string CurrentPlatform { get; set; } = string.Empty;
    public int GamesFound { get; set; }
    public string StatusMessage { get; set; } = string.Empty;
    
    public double ProgressPercentage => TotalPlatforms > 0 ? (double)CompletedPlatforms / TotalPlatforms * 100 : 0;
}