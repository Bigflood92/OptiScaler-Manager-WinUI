using System;
using System.Collections.Generic;

namespace OptiScaler.Core.Models;

/// <summary>
/// Global application settings and mod preferences
/// </summary>
public class GlobalSettings
{
    // ===============================
    // Mod Configuration Defaults
    // ===============================
    
    /// <summary>
    /// Default upscaler for new installations
    /// </summary>
    public string DefaultUpscaler { get; set; } = "auto";
    
    /// <summary>
    /// Default quality preset for new installations
    /// </summary>
    public string DefaultQualityPreset { get; set; } = "Quality";
    
    /// <summary>
    /// Enable frame generation by default
    /// </summary>
    public bool EnableFrameGeneration { get; set; } = false;

    /// <summary>
    /// Frame generation type (nofg | optifg)
    /// Currently only optifg is supported (requires OptiPatcher)
    /// </summary>
    public string FrameGenerationType { get; set; } = "nofg";
    
    /// <summary>
    /// Preferred DLL filename for OptiScaler installations
    /// </summary>
    public string PreferredDllName { get; set; } = "dxgi.dll";

    /// <summary>
    /// Apply global configuration to new mod installations
    /// </summary>
    public bool ApplyConfigToNewInstalls { get; set; } = true;
    
    /// <summary>
    /// Enable GPU spoofing by default
    /// </summary>
    public bool EnableGPUSpoofing { get; set; } = false;
    
    /// <summary>
    /// Spoofed GPU model
    /// </summary>
    public string SpoofedGPU { get; set; } = "RTX 4090";
    
    /// <summary>
    /// Enable overlay menu by default
    /// </summary>
    public bool EnableOverlayMenu { get; set; } = true;
    
    /// <summary>
    /// Show FPS counter by default
    /// </summary>
    public bool ShowFPSCounter { get; set; } = false;
    
    /// <summary>
    /// Auto-detect optimal settings for hardware
    /// </summary>
    public bool AutoDetectOptimalSettings { get; set; } = true;

    // ===============================
    // Advanced OptiScaler Overrides (Option B additions)
    // ===============================
    
    /// <summary>
    /// Override camera vertical FOV for FSR (0 = disabled)
    /// </summary>
    public float VerticalFovOverride { get; set; } = 0f;
    
    /// <summary>
    /// Enable sharpness override in OptiScaler.ini
    /// </summary>
    public bool SharpnessOverrideEnabled { get; set; } = false;
    
    /// <summary>
    /// Sharpness value when override enabled (0.0 - 1.0)
    /// </summary>
    public float SharpnessValue { get; set; } = 0.3f;
    
    /// <summary>
    /// OptiScaler log level (0-4)
    /// </summary>
    public int LogLevel { get; set; } = 2;

    // ===============================
    // Platform Scanning Settings
    // ===============================
    
    /// <summary>
    /// Scan Steam games
    /// </summary>
    public bool ScanSteam { get; set; } = true;
    
    /// <summary>
    /// Scan Epic Games Store
    /// </summary>
    public bool ScanEpic { get; set; } = true;
    
    /// <summary>
    /// Scan Xbox/Microsoft Store games
    /// </summary>
    public bool ScanXbox { get; set; } = true;
    
    /// <summary>
    /// Scan GOG Galaxy games
    /// </summary>
    public bool ScanGOG { get; set; } = true;
    
    /// <summary>
    /// Scan EA App games
    /// </summary>
    public bool ScanEA { get; set; } = true;
    
    /// <summary>
    /// Scan Ubisoft Connect games
    /// </summary>
    public bool ScanUbisoft { get; set; } = true;
    
    /// <summary>
    /// Custom game directories to scan
    /// </summary>
    public List<string> CustomGamePaths { get; set; } = new();
    
    /// <summary>
    /// Enable deep scanning mode (slower but more thorough)
    /// </summary>
    public bool EnableDeepScan { get; set; } = false;
    
    /// <summary>
    /// Auto-scan for games on application startup
    /// </summary>
    public bool AutoScanOnStartup { get; set; } = false;

    // ===============================
    // Platform Paths
    // ===============================
    
    /// <summary>
    /// Custom Steam library path (empty = use default detection)
    /// </summary>
    public string SteamLibraryPath { get; set; } = string.Empty;
    
    /// <summary>
    /// Custom Epic Games library path (empty = use default detection)
    /// </summary>
    public string EpicLibraryPath { get; set; } = string.Empty;
    
    /// <summary>
    /// Custom Xbox library path (empty = use default detection)
    /// </summary>
    public string XboxLibraryPath { get; set; } = string.Empty;
    
    /// <summary>
    /// Custom GOG library path (empty = use default detection)
    /// </summary>
    public string GOGLibraryPath { get; set; } = string.Empty;
    
    /// <summary>
    /// Custom EA library path (empty = use default detection)
    /// </summary>
    public string EALibraryPath { get; set; } = string.Empty;
    
    /// <summary>
    /// Custom Ubisoft library path (empty = use default detection)
    /// </summary>
    public string UbisoftLibraryPath { get; set; } = string.Empty;

    // ===============================
    // Application Preferences
    // ===============================
    
    /// <summary>
    /// Start application with Windows
    /// </summary>
    public bool StartWithWindows { get; set; } = false;
    
    /// <summary>
    /// Start application minimized
    /// </summary>
    public bool StartMinimized { get; set; } = false;
    
    /// <summary>
    /// Enable desktop notifications
    /// </summary>
    public bool EnableNotifications { get; set; } = true;
    
    /// <summary>
    /// Application theme preference
    /// </summary>
    public string Theme { get; set; } = "Dark";
    
    /// <summary>
    /// Language/culture preference
    /// </summary>
    public string Language { get; set; } = "en-US";
    
    /// <summary>
    /// Check for app updates on startup
    /// </summary>
    public bool CheckForUpdatesOnStartup { get; set; } = true;

    // ===============================
    // Metadata
    // ===============================
    
    /// <summary>
    /// Last time settings were updated
    /// </summary>
    public DateTime LastUpdated { get; set; } = DateTime.Now;
    
    /// <summary>
    /// Settings version for migration purposes
    /// </summary>
    public int Version { get; set; } = 2;

    // ===============================
    // Advanced Settings
    // ===============================
    
    /// <summary>
    /// Menu scale for the application
    /// </summary>
    public float MenuScale { get; set; } = 1.0f;
    
    /// <summary>
    /// Position of the FPS overlay (0 - top left, 1 - top right, 2 - bottom left, 3 - bottom right)
    /// </summary>
    public int FpsOverlayPos { get; set; } = 0;
    
    /// <summary>
    /// Enable debug view for OptiFG
    /// </summary>
    public bool OptiFGDebugView { get; set; } = false;
    
    /// <summary>
    /// Allow async compute for OptiFG
    /// </summary>
    public bool OptiFGAllowAsync { get; set; } = false;
    
    /// <summary>
    /// Fix HUD for OptiFG
    /// </summary>
    public bool OptiFGHUDFix { get; set; } = false;
    
    /// <summary>
    /// Enable DLSS
    /// </summary>
    public bool DLSSEnabled { get; set; } = true;
    
    /// <summary>
    /// Override DLSS render preset
    /// </summary>
    public bool DLSSRenderPresetOverride { get; set; } = false;
    
    /// <summary>
    /// Near clipping plane for FSR camera
    /// </summary>
    public float FSRCameraNear { get; set; } = 0.1f;
    
    /// <summary>
    /// Far clipping plane for FSR camera
    /// </summary>
    public float FSRCameraFar { get; set; } = 10000f;
    
    /// <summary>
    /// Enable debug view for FSR
    /// </summary>
    public bool FSRDebugView { get; set; } = false;
    
    /// <summary>
    /// Upscaler index for FSR
    /// </summary>
    public int FSRUpscalerIndex { get; set; } = 0;
    
    /// <summary>
    /// Build XeSS pipelines
    /// </summary>
    public bool XeSSBuildPipelines { get; set; } = true;
    
    /// <summary>
    /// Network model for XeSS (0 = Default, 1 = Model A, 2 = Model B)
    /// </summary>
    public int XeSSNetworkModel { get; set; } = 0;
    
    /// <summary>
    /// Enable quality ratio override
    /// </summary>
    public bool QualityRatioOverrideEnabled { get; set; } = false;
    
    /// <summary>
    /// Quality ratio for DLAA
    /// </summary>
    public float QualityRatioDLAA { get; set; } = 1.0f;
    
    /// <summary>
    /// Quality ratio for Ultra Quality
    /// </summary>
    public float QualityRatioUltraQuality { get; set; } = 1.3f;
    
    /// <summary>
    /// Quality ratio for Quality
    /// </summary>
    public float QualityRatioQuality { get; set; } = 1.5f;
    
    /// <summary>
    /// Quality ratio for Balanced
    /// </summary>
    public float QualityRatioBalanced { get; set; } = 1.7f;
    
    /// <summary>
    /// Quality ratio for Performance
    /// </summary>
    public float QualityRatioPerformance { get; set; } = 2.0f;
    
    /// <summary>
    /// Quality ratio for Ultra Performance
    /// </summary>
    public float QualityRatioUltraPerformance { get; set; } = 3.0f;
    
    /// <summary>
    /// Enable CAS (Contrast Adaptive Sharpening)
    /// </summary>
    public bool CASEnabled { get; set; } = false;
    
    /// <summary>
    /// Enable motion sharpness for CAS
    /// </summary>
    public bool CASMotionSharpnessEnabled { get; set; } = false;
    
    /// <summary>
    /// Motion sharpness value for CAS
    /// </summary>
    public float CASMotionSharpness { get; set; } = 0.4f;
    
    /// <summary>
    /// Log settings
    /// </summary>
    public bool LogToFile { get; set; } = false;
    public bool LogToConsole { get; set; } = false;

    /// <summary>
    /// Preferred GPU vendor (auto, nvidia, amd, intel)
    /// </summary>
    public string PreferredGpuVendor { get; set; } = "auto";
}