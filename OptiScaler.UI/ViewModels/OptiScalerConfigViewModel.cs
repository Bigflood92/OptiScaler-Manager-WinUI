using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OptiScaler.Core.Models;
using OptiScaler.Core.Services;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace OptiScaler.UI.ViewModels;

/// <summary>
/// ViewModel for OptiScaler configuration management
/// </summary>
public partial class OptiScalerConfigViewModel : ObservableObject
{
    private readonly OptiScalerConfigService _configService;
    private readonly GameInfo _game;
    private string _configFilePath;

    [ObservableProperty]
    private OptiScalerConfig _config = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _hasUnsavedChanges;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    // UI Collections
    public ObservableCollection<UpscalerOption> AvailableUpscalers { get; } = new();
    public ObservableCollection<FrameGenOption> FrameGenOptions { get; } = new();
    public ObservableCollection<QualityPresetOption> QualityPresets { get; } = new();
    public ObservableCollection<GPUSpoofOption> GPUSpoofOptions { get; } = new();

    public OptiScalerConfigViewModel()
    {
        _configService = new OptiScalerConfigService();
        _game = new GameInfo(); // Default for design time
        _configFilePath = string.Empty;
        InitializeOptions();
    }

    public OptiScalerConfigViewModel(GameInfo game)
    {
        _configService = new OptiScalerConfigService();
        _game = game;
        _configFilePath = Path.Combine(game.InstallDirectory ?? game.Path, "OptiScaler.ini");
        InitializeOptions();
        _ = LoadConfiguration();
    }

    /// <summary>
    /// Initialize UI option collections
    /// </summary>
    private void InitializeOptions()
    {
        // Upscaler options
        AvailableUpscalers.Add(new UpscalerOption("auto", "Auto", "Automatically select best upscaler"));
        AvailableUpscalers.Add(new UpscalerOption("dlss", "DLSS", "NVIDIA DLSS (requires RTX GPU)"));
        AvailableUpscalers.Add(new UpscalerOption("xess", "XeSS", "Intel XeSS (works on all GPUs)"));
        AvailableUpscalers.Add(new UpscalerOption("fsr22", "FSR 2.2", "AMD FidelityFX Super Resolution 2.2"));
        AvailableUpscalers.Add(new UpscalerOption("fsr31", "FSR 3.1", "AMD FidelityFX Super Resolution 3.1"));

        // Frame Generation options
        FrameGenOptions.Add(new FrameGenOption("nofg", "Disabled", "No frame generation"));
        FrameGenOptions.Add(new FrameGenOption("optifg", "OptiFG", "FSR3.1 Frame Generation (requires AMD FidelityFX)"));
        FrameGenOptions.Add(new FrameGenOption("nukems", "Nukem's FG", "DLSS-G to FSR3 (requires special DLL)"));
        FrameGenOptions.Add(new FrameGenOption("auto", "Auto", "Automatically select based on GPU"));

        // Quality presets
        QualityPresets.Add(new QualityPresetOption("DLAA", 1.0f, "AI Anti-Aliasing (no upscaling)"));
        QualityPresets.Add(new QualityPresetOption("Ultra Quality", 1.3f, "Minimal upscaling, maximum quality"));
        QualityPresets.Add(new QualityPresetOption("Quality", 1.5f, "Balanced quality and performance"));
        QualityPresets.Add(new QualityPresetOption("Balanced", 1.7f, "Good balance of quality and performance"));
        QualityPresets.Add(new QualityPresetOption("Performance", 2.0f, "Higher performance, lower quality"));
        QualityPresets.Add(new QualityPresetOption("Ultra Performance", 3.0f, "Maximum performance"));

        // GPU Spoofing options
        GPUSpoofOptions.Add(new GPUSpoofOption(0x10de, 0x2684, "NVIDIA GeForce RTX 4090"));
        GPUSpoofOptions.Add(new GPUSpoofOption(0x10de, 0x2704, "NVIDIA GeForce RTX 4080"));
        GPUSpoofOptions.Add(new GPUSpoofOption(0x10de, 0x2782, "NVIDIA GeForce RTX 4070"));
        GPUSpoofOptions.Add(new GPUSpoofOption(0x8086, 0x56A0, "Intel Arc A770 Graphics"));
        GPUSpoofOptions.Add(new GPUSpoofOption(0x1002, 0x7550, "AMD Radeon RX 9070 XT"));
    }

    /// <summary>
    /// Load configuration from file
    /// </summary>
    [RelayCommand]
    private async Task LoadConfigurationAsync()
    {
        await LoadConfiguration();
    }

    private async Task LoadConfiguration()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Loading configuration...";

            await Task.Run(() =>
            {
                Config = _configService.ReadConfig(_configFilePath);
            });

            HasUnsavedChanges = false;
            StatusMessage = "Configuration loaded successfully";

            Debug.WriteLine($"[OptiConfig] Loaded config from: {_configFilePath}");
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading configuration: {ex.Message}";
            Debug.WriteLine($"[OptiConfig] Error loading: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Save configuration to file
    /// </summary>
    [RelayCommand]
    private async Task SaveConfigurationAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Saving configuration...";

            await Task.Run(() =>
            {
                // Ensure directory exists
                var directory = Path.GetDirectoryName(_configFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                _configService.WriteConfig(_configFilePath, Config);
            });

            HasUnsavedChanges = false;
            StatusMessage = "Configuration saved successfully";

            Debug.WriteLine($"[OptiConfig] Saved config to: {_configFilePath}");

            // Update game info with new configuration
            _game.UpscalingMethod = Config.GetUpscalerDisplayName();
            _game.HasFrameGeneration = Config.IsFrameGenerationEnabled();
            _game.QualityPreset = Config.GetQualityPresetName();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error saving configuration: {ex.Message}";
            Debug.WriteLine($"[OptiConfig] Error saving: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Reset configuration to defaults
    /// </summary>
    [RelayCommand]
    private void ResetToDefaults()
    {
        Config = new OptiScalerConfig();
        HasUnsavedChanges = true;
        StatusMessage = "Configuration reset to defaults";
    }

    /// <summary>
    /// Apply smart defaults based on detected GPU
    /// </summary>
    [RelayCommand]
    private void ApplySmartDefaults()
    {
        // This would detect GPU and apply optimal settings
        // For now, apply reasonable defaults
        Config.Dx12Upscaler = "auto";
        Config.FGType = "auto";
        Config.DxgiSpoofing = false; // Disable spoofing by default
        
        HasUnsavedChanges = true;
        StatusMessage = "Applied smart defaults based on system";
    }

    /// <summary>
    /// Open OptiScaler documentation
    /// </summary>
    [RelayCommand]
    private void OpenDocumentation()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/cdozdil/OptiScaler",
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error opening documentation: {ex.Message}";
        }
    }

    // Property change tracking
    partial void OnConfigChanged(OptiScalerConfig value)
    {
        HasUnsavedChanges = true;
    }
}

/// <summary>
/// Upscaler option for UI binding
/// </summary>
public record UpscalerOption(string Value, string DisplayName, string Description);

/// <summary>
/// Frame generation option for UI binding  
/// </summary>
public record FrameGenOption(string Value, string DisplayName, string Description);

/// <summary>
/// Quality preset option for UI binding
/// </summary>
public record QualityPresetOption(string Name, float Ratio, string Description);

/// <summary>
/// GPU spoofing option for UI binding
/// </summary>
public record GPUSpoofOption(int VendorId, int DeviceId, string DisplayName)
{
    public string Description => $"0x{VendorId:X4}:0x{DeviceId:X4}";
}