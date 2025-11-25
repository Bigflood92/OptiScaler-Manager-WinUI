using OptiScaler.Core.Models;
using System.Text.Json;

namespace OptiScaler.Core.Services;

/// <summary>
/// Service for managing global OptiScaler preferences
/// </summary>
public class GlobalSettingsService
{
    private readonly string _settingsPath;
    private GlobalSettings? _cachedSettings;

    public GlobalSettingsService()
    {
        _settingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "OptiScaler Manager",
            "GlobalSettings.json");
    }

    /// <summary>
    /// Load global settings
    /// </summary>
    public async Task<GlobalSettings> LoadSettingsAsync()
    {
        if (_cachedSettings != null)
            return _cachedSettings;

        try
        {
            if (!File.Exists(_settingsPath))
            {
                _cachedSettings = CreateDefaultSettings();
                await SaveSettingsAsync(_cachedSettings);
                return _cachedSettings;
            }

            var json = await File.ReadAllTextAsync(_settingsPath);
            _cachedSettings = JsonSerializer.Deserialize<GlobalSettings>(json) ?? CreateDefaultSettings();

            // Migration for new properties (Option B/C additions)
            if (string.IsNullOrWhiteSpace(_cachedSettings.PreferredDllName)) _cachedSettings.PreferredDllName = "dxgi.dll";
            if (string.IsNullOrWhiteSpace(_cachedSettings.FrameGenerationType)) _cachedSettings.FrameGenerationType = _cachedSettings.EnableFrameGeneration ? "optifg" : "nofg";
            if (_cachedSettings.SharpnessValue <= 0f) _cachedSettings.SharpnessValue = 0.3f;
            if (_cachedSettings.LogLevel < 0 || _cachedSettings.LogLevel > 5) _cachedSettings.LogLevel = 2;
            if (string.IsNullOrWhiteSpace(_cachedSettings.PreferredGpuVendor)) _cachedSettings.PreferredGpuVendor = "auto";
            // Version bump placeholder
            if (_cachedSettings.Version < 2)
            {
                _cachedSettings.Version = 2; // increment version for new fields
                await SaveSettingsAsync(_cachedSettings); // persist migrated
            }
            return _cachedSettings;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading global settings: {ex.Message}");
            _cachedSettings = CreateDefaultSettings();
            return _cachedSettings;
        }
    }

    /// <summary>
    /// Save global settings
    /// </summary>
    public async Task SaveSettingsAsync(GlobalSettings settings)
    {
        try
        {
            var directory = Path.GetDirectoryName(_settingsPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            
            await File.WriteAllTextAsync(_settingsPath, json);
            _cachedSettings = settings;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving global settings: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Create default global settings
    /// </summary>
    private GlobalSettings CreateDefaultSettings()
    {
        return new GlobalSettings
        {
            DefaultUpscaler = "auto",
            DefaultQualityPreset = "Quality",
            EnableFrameGeneration = false,
            FrameGenerationType = "nofg",
            PreferredDllName = "dxgi.dll",
            ApplyConfigToNewInstalls = true,
            EnableGPUSpoofing = false,
            SpoofedGPU = "RTX 4090",
            EnableOverlayMenu = true,
            ShowFPSCounter = false,
            AutoDetectOptimalSettings = true,
            VerticalFovOverride = 0f,
            SharpnessOverrideEnabled = false,
            SharpnessValue = 0.3f,
            LogLevel = 2,
            
            // Platform scanning defaults
            ScanSteam = true,
            ScanEpic = true,
            ScanXbox = true,
            ScanGOG = true,
            ScanEA = true,
            ScanUbisoft = true,
            EnableDeepScan = false,
            AutoScanOnStartup = false,  // Changed from true to false
            
            // App preferences defaults
            StartWithWindows = false,
            StartMinimized = false,
            EnableNotifications = true,
            Theme = "Dark",
            Language = "en-US",
            CheckForUpdatesOnStartup = true,
            LastUpdated = DateTime.Now,
            Version = 2,

            // GPU vendor preference
            PreferredGpuVendor = "auto"
        };
    }

    /// <summary>
    /// Apply global settings to a new OptiScaler configuration
    /// </summary>
    public OptiScalerConfig ApplyGlobalSettingsToConfig(OptiScalerConfig config, GlobalSettings globalSettings)
    {
        var upscaler = globalSettings.DefaultUpscaler.ToLowerInvariant();
        config.Dx12Upscaler = upscaler;
        config.Dx11Upscaler = upscaler;
        config.VulkanUpscaler = upscaler;

        if (globalSettings.DefaultQualityPreset != "Quality")
        {
            config.QualityRatioOverrideEnabled = true;
            SetQualityRatios(config, globalSettings.DefaultQualityPreset);
        }

        // FrameGen
        config.OptiFGEnabled = globalSettings.EnableFrameGeneration;
        config.FGType = globalSettings.EnableFrameGeneration ? "optifg" : "nofg";
        config.OptiFGDebugView = globalSettings.OptiFGDebugView;
        config.OptiFGAllowAsync = globalSettings.OptiFGAllowAsync;
        config.OptiFGHUDFix = globalSettings.OptiFGHUDFix;

        // Spoofing
        config.DxgiSpoofing = globalSettings.EnableGPUSpoofing;
        if (globalSettings.EnableGPUSpoofing)
            SetSpoofedGPU(config, globalSettings.SpoofedGPU);

        // Menu / HUD
        config.OverlayMenu = globalSettings.EnableOverlayMenu;
        config.ShowFps = globalSettings.ShowFPSCounter;
        config.MenuScale = globalSettings.MenuScale;
        config.FpsOverlayPos = globalSettings.FpsOverlayPos;

        // DLSS
        config.DLSSEnabled = globalSettings.DLSSEnabled;
        config.DLSSRenderPresetOverride = globalSettings.DLSSRenderPresetOverride;

        // FSR
        if (globalSettings.VerticalFovOverride > 0) config.FSRVerticalFov = globalSettings.VerticalFovOverride;
        config.FSRCameraNear = globalSettings.FSRCameraNear;
        config.FSRCameraFar = globalSettings.FSRCameraFar;
        config.FSRDebugView = globalSettings.FSRDebugView;
        config.FSRUpscalerIndex = globalSettings.FSRUpscalerIndex;

        // XeSS
        config.XeSSBuildPipelines = globalSettings.XeSSBuildPipelines;
        config.XeSSNetworkModel = globalSettings.XeSSNetworkModel;

        // Quality ratios (if global override explicitly enabled)
        if (globalSettings.QualityRatioOverrideEnabled)
        {
            config.QualityRatioOverrideEnabled = true;
            config.QualityRatioDLAA = globalSettings.QualityRatioDLAA;
            config.QualityRatioUltraQuality = globalSettings.QualityRatioUltraQuality;
            config.QualityRatioQuality = globalSettings.QualityRatioQuality;
            config.QualityRatioBalanced = globalSettings.QualityRatioBalanced;
            config.QualityRatioPerformance = globalSettings.QualityRatioPerformance;
            config.QualityRatioUltraPerformance = globalSettings.QualityRatioUltraPerformance;
        }

        // Sharpness / CAS
        config.SharpnessOverride = globalSettings.SharpnessOverrideEnabled;
        config.Sharpness = globalSettings.SharpnessOverrideEnabled ? globalSettings.SharpnessValue : config.Sharpness;
        config.CASEnabled = globalSettings.CASEnabled;
        config.CASMotionSharpnessEnabled = globalSettings.CASMotionSharpnessEnabled;
        config.CASMotionSharpness = globalSettings.CASMotionSharpness;

        // Logging
        config.LogLevel = globalSettings.LogLevel;
        config.LogToFile = globalSettings.LogToFile;
        config.LogToConsole = globalSettings.LogToConsole;

        return config;
    }

    /// <summary>
    /// Set quality ratios based on preset name
    /// </summary>
    private void SetQualityRatios(OptiScalerConfig config, string presetName)
    {
        switch (presetName.ToLowerInvariant())
        {
            case "ultra quality":
                config.QualityRatioQuality = 1.3f;
                break;
            case "quality":
                config.QualityRatioQuality = 1.5f;
                break;
            case "balanced":
                config.QualityRatioQuality = 1.7f;
                break;
            case "performance":
                config.QualityRatioQuality = 2.0f;
                break;
            case "ultra performance":
                config.QualityRatioQuality = 3.0f;
                break;
        }
    }

    /// <summary>
    /// Set spoofed GPU configuration
    /// </summary>
    private void SetSpoofedGPU(OptiScalerConfig config, string gpuName)
    {
        switch (gpuName.ToLowerInvariant())
        {
            case "rtx 4090":
                config.SpoofedVendorId = 0x10de;
                config.SpoofedDeviceId = 0x2684;
                config.SpoofedGPUName = "NVIDIA GeForce RTX 4090";
                break;
            case "rtx 4080":
                config.SpoofedVendorId = 0x10de;
                config.SpoofedDeviceId = 0x2704;
                config.SpoofedGPUName = "NVIDIA GeForce RTX 4080";
                break;
            case "rtx 4070":
                config.SpoofedVendorId = 0x10de;
                config.SpoofedDeviceId = 0x2782;
                config.SpoofedGPUName = "NVIDIA GeForce RTX 4070";
                break;
            default:
                config.SpoofedVendorId = 0x10de;
                config.SpoofedDeviceId = 0x2684;
                config.SpoofedGPUName = "NVIDIA GeForce RTX 4090";
                break;
        }
    }
}