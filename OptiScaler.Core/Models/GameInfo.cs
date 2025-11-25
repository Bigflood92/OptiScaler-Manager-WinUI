using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace OptiScaler.Core.Models;

/// <summary>
/// Represents a detected game installation
/// </summary>
public class GameInfo : INotifyPropertyChanged
{
    private string _name = string.Empty;
    private string _path = string.Empty;
    private string _executable = string.Empty;
    private string _installDirectory = string.Empty;
    private GamePlatform _platform = GamePlatform.Unknown;
    private bool _hasOptiscaler;
    private bool _hasOptiPatcher;
    private DateTime _lastScanned = DateTime.Now;
    private string? _upscalingMethod;
    private bool _hasFrameGeneration;
    private string? _qualityPreset;

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Game display name
    /// </summary>
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    /// <summary>
    /// Game installation directory path (root folder)
    /// </summary>
    public string Path
    {
        get => _path;
        set => SetProperty(ref _path, value);
    }

    /// <summary>
    /// Main game executable file path (full path to .exe)
    /// </summary>
    public string Executable
    {
        get => _executable;
        set => SetProperty(ref _executable, value);
    }

    /// <summary>
    /// Directory where the game executable is located (where mods should be installed)
    /// This is the directory containing the .exe file, NOT the game root folder
    /// Example: For C:\XboxGames\Keeper\Content\PaganIdol\Binaries\WinGDK\game.exe
    /// This would be: C:\XboxGames\Keeper\Content\PaganIdol\Binaries\WinGDK
    /// </summary>
    public string InstallDirectory
    {
        get => _installDirectory;
        set => SetProperty(ref _installDirectory, value);
    }

    /// <summary>
    /// Platform where the game was found (Steam, Epic, etc.)
    /// </summary>
    public GamePlatform Platform
    {
        get => _platform;
        set => SetProperty(ref _platform, value);
    }

    /// <summary>
    /// Whether OptiScaler mod is installed
    /// </summary>
    public bool HasOptiScaler
    {
        get => _hasOptiscaler;
        set => SetProperty(ref _hasOptiscaler, value);
    }

    /// <summary>
    /// Whether OptiPatcher (ASI plugin) is installed
    /// </summary>
    public bool HasOptiPatcher
    {
        get => _hasOptiPatcher;
        set => SetProperty(ref _hasOptiPatcher, value);
    }

    /// <summary>
    /// Last time this game was scanned for mods
    /// </summary>
    public DateTime LastScanned
    {
        get => _lastScanned;
        set => SetProperty(ref _lastScanned, value);
    }

    /// <summary>
    /// OptiScaler upscaling method currently configured (e.g., "DLSS 3.7", "XeSS", "FSR")
    /// </summary>
    public string? UpscalingMethod
    {
        get => _upscalingMethod;
        set => SetProperty(ref _upscalingMethod, value);
    }

    /// <summary>
    /// Whether frame generation is enabled
    /// </summary>
    public bool HasFrameGeneration
    {
        get => _hasFrameGeneration;
        set => SetProperty(ref _hasFrameGeneration, value);
    }

    /// <summary>
    /// Quality preset being used (e.g., "Quality", "Balanced", "Performance")
    /// </summary>
    public string? QualityPreset
    {
        get => _qualityPreset;
        set => SetProperty(ref _qualityPreset, value);
    }

    /// <summary>
    /// Display path for UI (shows InstallDirectory if available, otherwise Path)
    /// </summary>
    public string DisplayPath => !string.IsNullOrEmpty(InstallDirectory) ? InstallDirectory : Path;

    protected void SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (!Equals(field, value))
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

/// <summary>
/// Gaming platform enumeration
/// </summary>
public enum GamePlatform
{
    Unknown,
    Steam,
    Epic,
    Xbox,
    GOG,
    EA,
    Ubisoft,
    Manual
}