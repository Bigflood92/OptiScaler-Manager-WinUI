namespace OptiScaler.Core.Models;

/// <summary>
/// Comprehensive OptiScaler configuration model
/// </summary>
public class OptiScalerConfig
{
    // ===============================
    // [Upscalers] Section
    // ===============================
    public string Dx11Upscaler { get; set; } = "auto";
    public string Dx12Upscaler { get; set; } = "auto"; 
    public string VulkanUpscaler { get; set; } = "auto";

    // ===============================
    // [FrameGen] Section
    // ===============================
    public string FGType { get; set; } = "auto";

    // ===============================
    // [OptiFG] Section
    // ===============================
    public bool OptiFGEnabled { get; set; } = false;
    public bool OptiFGDebugView { get; set; } = false;
    public bool OptiFGAllowAsync { get; set; } = false;
    public bool OptiFGHUDFix { get; set; } = false;

    // ===============================
    // [Menu] Section
    // ===============================
    public bool OverlayMenu { get; set; } = true;
    public float MenuScale { get; set; } = 1.0f;
    public int ShortcutKey { get; set; } = 0x2D; // VK_INSERT
    public bool ShowFps { get; set; } = false;
    public int FpsOverlayPos { get; set; } = 0; // Top Left

    // ===============================
    // [DLSS] Section
    // ===============================
    public bool DLSSEnabled { get; set; } = true;
    public string? DLSSLibraryPath { get; set; }
    public bool DLSSRenderPresetOverride { get; set; } = false;

    // ===============================
    // [FSR] Section
    // ===============================
    public float FSRVerticalFov { get; set; } = 60.0f;
    public float FSRCameraNear { get; set; } = 0.1f;
    public float FSRCameraFar { get; set; } = 10000.0f;
    public bool FSRDebugView { get; set; } = false;
    public int FSRUpscalerIndex { get; set; } = 0; // FSR 3.1.2

    // ===============================
    // [XeSS] Section
    // ===============================
    public bool XeSSBuildPipelines { get; set; } = true;
    public int XeSSNetworkModel { get; set; } = 0; // KPSS
    public string? XeSSLibraryPath { get; set; }

    // ===============================
    // [Spoofing] Section
    // ===============================
    public bool SpoofingEnabled { get; set; } = true;
    public int SpoofedVendorId { get; set; } = 0x10de; // Nvidia
    public int SpoofedDeviceId { get; set; } = 0x2684; // RTX 4090
    public string SpoofedGPUName { get; set; } = "NVIDIA GeForce RTX 4090";
    public bool DxgiSpoofing { get; set; } = true;

    // ===============================
    // [Quality Overrides] Section
    // ===============================
    public bool QualityRatioOverrideEnabled { get; set; } = false;
    public float QualityRatioDLAA { get; set; } = 1.0f;
    public float QualityRatioUltraQuality { get; set; } = 1.3f;
    public float QualityRatioQuality { get; set; } = 1.5f;
    public float QualityRatioBalanced { get; set; } = 1.7f;
    public float QualityRatioPerformance { get; set; } = 2.0f;
    public float QualityRatioUltraPerformance { get; set; } = 3.0f;

    // ===============================
    // [Sharpness] Section
    // ===============================
    public bool SharpnessOverride { get; set; } = false;
    public float Sharpness { get; set; } = 0.3f;

    // ===============================
    // [CAS] Section
    // ===============================
    public bool CASEnabled { get; set; } = false;
    public bool CASMotionSharpnessEnabled { get; set; } = false;
    public float CASMotionSharpness { get; set; } = 0.4f;

    // ===============================
    // [Log] Section
    // ===============================
    public string LogFile { get; set; } = "OptiScaler.log";
    public int LogLevel { get; set; } = 2; // Info
    public bool LogToFile { get; set; } = false;
    public bool LogToConsole { get; set; } = false;

    // ===============================
    // Helper Methods
    // ===============================

    /// <summary>
    /// Get the primary upscaler based on API priority
    /// </summary>
    public string GetPrimaryUpscaler()
    {
        // Prefer DX12 > DX11 > Vulkan based on usage
        if (Dx12Upscaler != "auto") return Dx12Upscaler;
        if (Dx11Upscaler != "auto") return Dx11Upscaler;
        if (VulkanUpscaler != "auto") return VulkanUpscaler;
        return "auto";
    }

    /// <summary>
    /// Get user-friendly upscaler display name
    /// </summary>
    public string GetUpscalerDisplayName()
    {
        var primary = GetPrimaryUpscaler();
        return primary switch
        {
            "dlss" => "DLSS",
            "xess" => "XeSS", 
            "fsr21" => "FSR 2.1",
            "fsr22" => "FSR 2.2",
            "fsr31" => "FSR 3.1",
            "auto" => "Auto",
            _ => primary.ToUpperInvariant()
        };
    }

    /// <summary>
    /// Check if frame generation is enabled
    /// </summary>
    public bool IsFrameGenerationEnabled()
    {
        return FGType != "nofg" && (OptiFGEnabled || FGType == "optifg" || FGType == "nukems");
    }

    /// <summary>
    /// Get quality preset display name
    /// </summary>
    public string GetQualityPresetName()
    {
        if (!QualityRatioOverrideEnabled) return "Default";

        // Find closest match to current ratios
        if (Math.Abs(QualityRatioQuality - 1.5f) < 0.1f) return "Quality";
        if (Math.Abs(QualityRatioBalanced - 1.7f) < 0.1f) return "Balanced";
        if (Math.Abs(QualityRatioPerformance - 2.0f) < 0.1f) return "Performance";
        if (Math.Abs(QualityRatioUltraQuality - 1.3f) < 0.1f) return "Ultra Quality";
        if (Math.Abs(QualityRatioUltraPerformance - 3.0f) < 0.1f) return "Ultra Performance";
        if (Math.Abs(QualityRatioDLAA - 1.0f) < 0.1f) return "DLAA";

        return "Custom";
    }
}