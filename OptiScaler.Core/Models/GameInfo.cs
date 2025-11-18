using System.ComponentModel;

namespace OptiScaler.Core.Models;

/// <summary>
/// Represents a detected game installation
/// </summary>
public class GameInfo : INotifyPropertyChanged
{
    private string _name = string.Empty;
    private string _path = string.Empty;
    private string _executable = string.Empty;
    private GamePlatform _platform = GamePlatform.Unknown;
    private bool _hasOptiscaler;
    private bool _hasDlssgToFsr3;
    private DateTime _lastScanned = DateTime.Now;

    /// <summary>
    /// Game display name
    /// </summary>
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    /// <summary>
    /// Game installation directory path
    /// </summary>
    public string Path
    {
        get => _path;
        set => SetProperty(ref _path, value);
    }

    /// <summary>
    /// Main game executable file path
    /// </summary>
    public string Executable
    {
        get => _executable;
        set => SetProperty(ref _executable, value);
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
    /// Whether DLSSG-to-FSR3 mod is installed
    /// </summary>
    public bool HasDlssgToFsr3
    {
        get => _hasDlssgToFsr3;
        set => SetProperty(ref _hasDlssgToFsr3, value);
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
    /// Unique identifier for this game installation
    /// </summary>
    public string Id => $"{Platform}_{Name}_{Path.GetHashCode()}";

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
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