namespace OptiScaler.Core.Models;

/// <summary>
/// Information about an installed or available mod
/// </summary>
public class ModInfo
{
    /// <summary>
    /// Unique identifier for the mod type
    /// </summary>
    public ModType Type { get; set; }

    /// <summary>
    /// Display name of the mod
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Installed version (null if not installed)
    /// </summary>
    public Version? InstalledVersion { get; set; }

    /// <summary>
    /// Latest available version from GitHub
    /// </summary>
    public Version? LatestVersion { get; set; }

    /// <summary>
    /// Whether the mod is currently installed
    /// </summary>
    public bool IsInstalled => InstalledVersion != null;

    /// <summary>
    /// Whether an update is available
    /// </summary>
    public bool UpdateAvailable => IsInstalled && LatestVersion != null && LatestVersion > InstalledVersion;

    /// <summary>
    /// GitHub repository owner
    /// </summary>
    public string RepositoryOwner { get; set; } = string.Empty;

    /// <summary>
    /// GitHub repository name
    /// </summary>
    public string RepositoryName { get; set; } = string.Empty;

    /// <summary>
    /// Full repository identifier (owner/name)
    /// </summary>
    public string Repository => $"{RepositoryOwner}/{RepositoryName}";

    /// <summary>
    /// Description of the mod
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// List of DLL files that belong to this mod
    /// </summary>
    public List<string> DllFiles { get; set; } = new();

    /// <summary>
    /// Installation date and time
    /// </summary>
    public DateTime? InstallDate { get; set; }
}

/// <summary>
/// Types of mods supported by OptiScaler Manager
/// </summary>
public enum ModType
{
    /// <summary>
    /// OptiScaler - Universal upscaling mod
    /// </summary>
    OptiScaler,

    /// <summary>
    /// DLSS3 to FSR3 Frame Generation
    /// </summary>
    DlssgToFsr3,

    /// <summary>
    /// Custom or unknown mod type
    /// </summary>
    Custom
}

/// <summary>
/// Result of a mod installation/uninstallation operation
/// </summary>
public class ModOperationResult
{
    /// <summary>
    /// Whether the operation was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Error message if operation failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// List of files that were installed/uninstalled
    /// </summary>
    public List<string> AffectedFiles { get; set; } = new();

    /// <summary>
    /// Operation type performed
    /// </summary>
    public ModOperation Operation { get; set; }

    /// <summary>
    /// Mod that was operated on
    /// </summary>
    public ModType ModType { get; set; }
}

/// <summary>
/// Type of mod operation
/// </summary>
public enum ModOperation
{
    Install,
    Update,
    Uninstall,
    Verify
}
