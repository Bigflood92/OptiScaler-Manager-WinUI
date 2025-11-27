namespace OptiScaler.Core.Models;

/// <summary>
/// Application configuration and user preferences
/// </summary>
public class AppConfiguration
{
    /// <summary>
    /// Custom game paths added by user
    /// </summary>
    public List<string> CustomGamePaths { get; set; } = new();

    /// <summary>
    /// Platforms to exclude from automatic scanning
    /// </summary>
    public List<GamePlatform> ExcludedPlatforms { get; set; } = new();

    /// <summary>
    /// Whether to check for mod updates on startup
    /// </summary>
    public bool CheckForUpdatesOnStartup { get; set; } = true;

    /// <summary>
    /// Whether to create backups before installing mods
    /// </summary>
    public bool CreateBackupsBeforeInstall { get; set; } = true;

    /// <summary>
    /// Maximum number of backups to keep per game
    /// </summary>
    public int MaxBackupsPerGame { get; set; } = 5;

    /// <summary>
    /// Theme preference
    /// </summary>
    public AppTheme Theme { get; set; } = AppTheme.System;

    /// <summary>
    /// Whether to minimize to system tray
    /// </summary>
    public bool MinimizeToTray { get; set; } = true;

    /// <summary>
    /// Whether to start with Windows
    /// </summary>
    public bool StartWithWindows { get; set; } = false;

    /// <summary>
    /// Whether to show notifications
    /// </summary>
    public bool ShowNotifications { get; set; } = true;

    /// <summary>
    /// Download location for mod files
    /// </summary>
    public string DownloadPath { get; set; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "OptiScaler Manager", "Downloads");

    /// <summary>
    /// Backup location
    /// </summary>
    public string BackupPath { get; set; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "OptiScaler Manager", "Backups");

    /// <summary>
    /// Last scan date
    /// </summary>
    public DateTime? LastScanDate { get; set; }

    /// <summary>
    /// Preferred mod type when both are applicable
    /// </summary>
    public ModType PreferredModType { get; set; } = ModType.OptiScaler;

    /// <summary>
    /// Language/culture code
    /// </summary>
    public string Language { get; set; } = "en-US";
}

/// <summary>
/// Application theme options
/// </summary>
public enum AppTheme
{
    System,
    Light,
    Dark,
    Purple // Custom OptiScaler Manager theme
}
