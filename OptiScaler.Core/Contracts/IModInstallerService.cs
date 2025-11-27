using OptiScaler.Core.Models;

namespace OptiScaler.Core.Contracts;

/// <summary>
/// Service for installing, updating, and managing mods in games
/// </summary>
public interface IModInstallerService
{
    /// <summary>
    /// Event raised when installation progress updates
    /// </summary>
    event EventHandler<InstallProgressEventArgs>? InstallProgress;

    /// <summary>
    /// Install a mod to a game from a downloaded archive
    /// </summary>
    /// <param name="game">Game to install mod to</param>
    /// <param name="modType">Type of mod to install</param>
    /// <param name="archivePath">Path to mod archive file</param>
    /// <param name="targetDllName">Optional: Target DLL name for renaming (OptiScaler only)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of installation operation</returns>
    Task<ModOperationResult> InstallModAsync(GameInfo game, ModType modType, string archivePath, string? targetDllName = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Uninstall a mod from a game
    /// </summary>
    /// <param name="game">Game to uninstall mod from</param>
    /// <param name="modType">Type of mod to uninstall</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of uninstallation operation</returns>
    Task<ModOperationResult> UninstallModAsync(GameInfo game, ModType modType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an installed mod to the latest version
    /// </summary>
    /// <param name="game">Game to update mod in</param>
    /// <param name="modType">Type of mod to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of update operation</returns>
    Task<ModOperationResult> UpdateModAsync(GameInfo game, ModType modType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get information about installed mods in a game
    /// </summary>
    /// <param name="game">Game to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of installed mods with version information</returns>
    Task<IEnumerable<ModInfo>> GetInstalledModsAsync(GameInfo game, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verify integrity of installed mod files
    /// </summary>
    /// <param name="game">Game to verify</param>
    /// <param name="modType">Type of mod to verify</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of verification</returns>
    Task<ModOperationResult> VerifyModIntegrityAsync(GameInfo game, ModType modType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a backup of game files before mod installation
    /// </summary>
    /// <param name="game">Game to backup</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Path to backup location</returns>
    Task<string> CreateBackupAsync(GameInfo game, CancellationToken cancellationToken = default);

    /// <summary>
    /// Restore game files from a backup
    /// </summary>
    /// <param name="game">Game to restore</param>
    /// <param name="backupPath">Path to backup</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of restore operation</returns>
    Task<ModOperationResult> RestoreBackupAsync(GameInfo game, string backupPath, CancellationToken cancellationToken = default);
}

/// <summary>
/// Event arguments for installation progress
/// </summary>
public class InstallProgressEventArgs : EventArgs
{
    public ModOperation Operation { get; set; }
    public ModType ModType { get; set; }
    public string GameName { get; set; } = string.Empty;
    public int CurrentStep { get; set; }
    public int TotalSteps { get; set; }
    public string CurrentAction { get; set; } = string.Empty;
    public double ProgressPercentage => TotalSteps > 0 ? (double)CurrentStep / TotalSteps * 100 : 0;
}
