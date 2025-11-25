using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Shapes; // Rectangle
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using OptiScaler.Core.Models;
using OptiScaler.Core.Services;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Text;
using Microsoft.UI;
using IOPath = System.IO.Path;

namespace OptiScaler.UI.Views;

public sealed partial class ModConfigPage : Page
{
    private readonly GlobalSettingsService _globalSettingsService;
    private readonly StorageService _storageService;
    private readonly SystemInfoService _systemInfoService = new();
    private GlobalSettings _currentSettings;
    private bool _hasOptiScalerInstalled;
    private bool _hasOptiPatcherInstalled;
    private GlobalSettings _customPresetSnapshot = new();
    private GlobalSettings _lastSavedSnapshot = new();
    private bool _advancedLoaded;
    private string _activePreset = "default"; // Track active preset

    public ModConfigPage()
    {
        InitializeComponent();
        _globalSettingsService = new GlobalSettingsService();
        _storageService = new StorageService();
        _currentSettings = new GlobalSettings();
        Loaded += ModConfigPage_LoadedAsync; // use async handler
    }

    private async void ModConfigPage_LoadedAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            await LoadGlobalSettings();
            await CheckInstalledMods();
            UpdateUIVisibility();
            WireEvents();
            _lastSavedSnapshot = CloneSettings(_currentSettings);
            
            // Detectar preset activo basado en configuración cargada
            DetectActivePreset();
            UpdatePresetButtonStyles();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ModConfigPage] Load error: {ex.Message}");
            SetStatus($"Error loading: {ex.Message}", Colors.OrangeRed);
        }
    }

    private void DetectActivePreset()
    {
        // Detectar qué preset está activo basándose en la configuración actual
        if (_currentSettings.DefaultUpscaler == "auto" && 
            _currentSettings.DefaultQualityPreset == "Quality" && 
            !_currentSettings.EnableFrameGeneration &&
            !_currentSettings.SharpnessOverrideEnabled)
        {
            _activePreset = "default";
        }
        else if (_currentSettings.DefaultUpscaler == "fsr31" && 
                 _currentSettings.DefaultQualityPreset == "Performance" && 
                 _currentSettings.EnableFrameGeneration)
        {
            _activePreset = "performance";
        }
        else if (_currentSettings.DefaultUpscaler == "auto" && 
                 _currentSettings.DefaultQualityPreset == "Balanced" &&
                 !_currentSettings.EnableFrameGeneration &&
                 _currentSettings.SharpnessOverrideEnabled &&
                 Math.Abs(_currentSettings.SharpnessValue - 0.30f) < 0.01f)
        {
            _activePreset = "balanced";
        }
        else if (_currentSettings.DefaultUpscaler == "dlss" && 
                 _currentSettings.DefaultQualityPreset == "Quality" &&
                 !_currentSettings.EnableFrameGeneration &&
                 _currentSettings.SharpnessOverrideEnabled &&
                 Math.Abs(_currentSettings.SharpnessValue - 0.35f) < 0.01f)
        {
            _activePreset = "quality";
        }
        else
        {
            // Si no coincide con ningún preset, es custom
            _activePreset = "custom";
        }
    }

    private T? GetCtrl<T>(string name) where T : class => FindName(name) as T;

    private void BasicNavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is NavigationViewItem item)
        {
            var tag = item.Tag as string;
            var panels = new[]{"UpscalingPanel","FrameGenPanel","InterfacePanel","InstallationPanel","GpuPanel","AdvancedPanel"};
            foreach (var pName in panels)
            {
                var p = GetCtrl<StackPanel>(pName);
                if (p != null) p.Visibility = Visibility.Collapsed;
            }
            if (tag == "Advanced") EnsureAdvancedLoaded();
            string panelName = tag == "GPU" ? "GpuPanel" : tag == "Advanced" ? "AdvancedPanel" : tag + "Panel";
            var target = GetCtrl<StackPanel>(panelName);
            if (target != null) target.Visibility = Visibility.Visible;
        }
    }

    private void EnsureAdvancedLoaded()
    {
        if (_advancedLoaded) return;
        var host = GetCtrl<StackPanel>("AdvancedPanel");
        if (host == null) return;
        host.Children.Clear();
        var outerBorder = new Border
        {
            Background = (Brush)Application.Current.Resources["ButtonSecondaryBrush"],
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(18)
        };
        var root = new StackPanel { Spacing = 20 };
        // Título
        root.Children.Add(new TextBlock{ Text="Advanced Settings", FontSize=16, FontWeight= FontWeights.SemiBold, Foreground = new SolidColorBrush(Colors.White)});

        // FrameGen Advanced
        root.Children.Add(BuildGroup("FrameGen Advanced", new UIElement[]{ BuildLabeledToggle("OptiFGDebugToggle","Debug View"), BuildLabeledToggle("OptiFGAsyncToggle","Allow Async"), BuildLabeledToggle("OptiFGHudFixToggle","HUD Fix") }));
        root.Children.Add(BuildSeparator());

        // DLSS
        root.Children.Add(BuildGroup("DLSS", new UIElement[]{ BuildLabeledToggle("DlssEnabledToggle","DLSS Enabled", true), BuildLabeledToggle("DlssPresetOverrideToggle","Render Preset Override") }));
        root.Children.Add(BuildSeparator());

        // FSR
        root.Children.Add(BuildGroup("FSR", new UIElement[]{ BuildLabelBold("Camera Near"), BuildTextBox("FsrNearTextBox","0.1",160), BuildLabelBold("Camera Far"), BuildTextBox("FsrFarTextBox","10000",160), BuildLabeledToggle("FsrDebugToggle","Debug View") }));
        root.Children.Add(BuildSeparator());

        // XeSS
        root.Children.Add(BuildGroup("XeSS", new UIElement[]{ BuildLabeledToggle("XessBuildPipelinesToggle","Build Pipelines", true), BuildLabelBold("Network Model"), BuildCombo("XessModelComboBox",160,new[]{"KPSS","KPVS"}) }));
        root.Children.Add(BuildSeparator());

        // Quality Overrides
        root.Children.Add(BuildGroup("Quality Overrides", new UIElement[]{ BuildLabeledToggle("QualityOverrideToggle","Enable Ratios Override"), BuildHorizontal(new UIElement[]{ BuildLabelWidth("DLAA",70), BuildTextBox("RatioDLAABox","1.0",80), BuildLabelWidth("UltraQ",70), BuildTextBox("RatioUltraQBox","1.3",80), BuildLabelWidth("Quality",60), BuildTextBox("RatioQualityBox","1.5",80)}), BuildHorizontal(new UIElement[]{ BuildLabelWidth("Balanced",70), BuildTextBox("RatioBalancedBox","1.7",80), BuildLabelWidth("Performance",70), BuildTextBox("RatioPerformanceBox","2.0",80), BuildLabelWidth("UltraPerf",70), BuildTextBox("RatioUltraPerfBox","3.0",80)}) }));
        root.Children.Add(BuildSeparator());

        // Sharpness & CAS
        root.Children.Add(BuildGroup("Sharpness & CAS", new UIElement[]{ BuildLabeledToggle("AdvSharpnessOverrideToggle","Override Sharpness"), BuildLabelBold("Sharpness Value"), BuildTextBox("AdvSharpnessValueTextBox","0.3",120), BuildLabeledToggle("CasEnabledToggle","CAS Enabled"), BuildLabeledToggle("CasMotionToggle","Motion Sharpness"), BuildLabelBold("Motion Sharpness Value"), BuildTextBox("CasMotionValueTextBox","0.4",120) }));
        root.Children.Add(BuildSeparator());

        // Logging
        root.Children.Add(BuildGroup("Logging", new UIElement[]{ BuildLabelBold("Log Level"), BuildCombo("AdvLogLevelComboBox",120,new[]{"0","1","2","3","4","5"},2), BuildLabeledToggle("LogToFileToggle","Log To File"), BuildLabeledToggle("LogToConsoleToggle","Log To Console") }));

        outerBorder.Child = root;
        host.Children.Add(outerBorder);
        _advancedLoaded = true;
    }

    private UIElement BuildSeparator() => new Rectangle { Height = 1, Fill = new SolidColorBrush(ColorHelper.FromArgb(0x22,0xFF,0xFF,0xFF)), Margin = new Thickness(0,4,0,4) };

    private Border BuildGroup(string title, IEnumerable<UIElement> children)
    {
        var b = new Border
        {
            Background = (Brush)Application.Current.Resources["CardDarkBrush"],
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(14),
            Margin = new Thickness(0,0,0,0)
        };
        var stack = new StackPanel { Spacing = 10 };
        stack.Children.Add(new TextBlock { Text = title, FontSize = 14, FontWeight = FontWeights.SemiBold, Foreground = new SolidColorBrush(Colors.White) });
        foreach (var c in children) stack.Children.Add(c);
        b.Child = stack;
        return b;
    }

    private StackPanel BuildLabeledToggle(string name, string label, bool isOn=false)
    {
        var sp = new StackPanel { Spacing = 4 };
        sp.Children.Add(new TextBlock { Text = label, FontSize = 14, FontWeight = FontWeights.SemiBold, Foreground = new SolidColorBrush(Colors.White) });
        sp.Children.Add(new ToggleSwitch { Name = name, IsOn = isOn, OnContent = "Enabled", OffContent = "Disabled" });
        return sp;
    }

    private TextBlock BuildLabel(string text) => new TextBlock { Text = text, Foreground = new SolidColorBrush(ColorHelper.FromArgb(0xFF,0xCC,0xCC,0xCC)), FontSize = 12 };
    private TextBlock BuildLabelBold(string text) => new TextBlock { Text = text, Foreground = new SolidColorBrush(Colors.White), FontSize = 14, FontWeight = FontWeights.SemiBold };
    private TextBlock BuildLabelWidth(string text, double width) => new TextBlock { Text = text, Foreground = new SolidColorBrush(ColorHelper.FromArgb(0xFF,0xCC,0xCC,0xCC)), FontSize = 12, Width = width };
    private TextBox BuildTextBox(string name, string placeholder, double width) => new TextBox { Name = name, PlaceholderText = placeholder, Width = width };
    private ComboBox BuildCombo(string name, double width, IEnumerable<string> items, int selectedIndex = 0)
    { var combo = new ComboBox { Name = name, Width = width, SelectedIndex = selectedIndex }; foreach (var i in items) combo.Items.Add(new ComboBoxItem { Content = i }); return combo; }
    private StackPanel BuildHorizontal(IEnumerable<UIElement> children) { var sp = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6 }; foreach (var c in children) sp.Children.Add(c); return sp; }

    private void SetStatus(string text, Windows.UI.Color color)
    {
        var status = GetCtrl<TextBlock>("StatusMessage");
        if (status != null)
        {
            status.Text = text;
            status.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(color);
        }
    }

    private async Task LoadGlobalSettings()
    {
        SetStatus("Loading global settings...", Microsoft.UI.Colors.LightGray);
        _currentSettings = await _globalSettingsService.LoadSettingsAsync();
        
        // Asegurar que PreferredGpuVendor tenga un valor por defecto
        if (string.IsNullOrEmpty(_currentSettings.PreferredGpuVendor))
        {
            _currentSettings.PreferredGpuVendor = "auto";
        }
        
        UpdateUIFromSettings();
        SetStatus("Global settings loaded", Microsoft.UI.Colors.LightGreen);

        var sysText = GetCtrl<TextBlock>("SystemInfoText");
        if (sysText != null)
        {
            var summary = _systemInfoService.BuildSystemSummary();
            sysText.Text = summary;
            var (vendor, _) = _systemInfoService.DetectGpuVendor();
            // Solo auto-detectar si está en auto y es la primera vez
            if (_currentSettings.PreferredGpuVendor == "auto" && vendor != "unknown")
            {
                // No cambiar PreferredGpuVendor, mantenerlo en "auto"
                // Esto permite que el usuario pueda forzar manualmente después
            }
        }
    }

    private void WireEvents()
    {
        var c = GetCtrl<ComboBox>("UpscalerComboBox"); if (c != null) c.SelectionChanged += Upscaler_Changed;
        c = GetCtrl<ComboBox>("QualityComboBox"); if (c != null) c.SelectionChanged += Quality_Changed;
        var ts = GetCtrl<ToggleSwitch>("FrameGenToggle"); if (ts != null) ts.Toggled += FrameGen_Toggled;
        ts = GetCtrl<ToggleSwitch>("OverlayMenuToggle"); if (ts != null) ts.Toggled += Overlay_Toggled;
        ts = GetCtrl<ToggleSwitch>("FPSCounterToggle"); if (ts != null) ts.Toggled += FPS_Toggled;
        ts = GetCtrl<ToggleSwitch>("ApplyConfigToggle"); if (ts != null) ts.Toggled += ApplyCfg_Toggled;
        c = GetCtrl<ComboBox>("SpoofGPUComboBox"); if (c != null) c.SelectionChanged += SpoofGpu_Changed;
        c = GetCtrl<ComboBox>("DllFilenameComboBox"); if (c != null) c.SelectionChanged += DllCombo_Changed;
        ts = GetCtrl<ToggleSwitch>("AdvSharpnessOverrideToggle"); if (ts != null) ts.Toggled += SharpOverride_Toggled;
        var tb = GetCtrl<TextBox>("AdvSharpnessValueTextBox"); if (tb != null) tb.TextChanged += SharpValue_Changed;
        tb = GetCtrl<TextBox>("VerticalFovTextBox"); if (tb != null) tb.TextChanged += Vfov_Changed;
        ts = GetCtrl<ToggleSwitch>("SpoofingToggle"); if (ts != null) ts.Toggled += SpoofingToggle_Toggled;
        c = GetCtrl<ComboBox>("AdvLogLevelComboBox"); if (c != null) c.SelectionChanged += LogLevel_Changed;
        c = GetCtrl<ComboBox>("GpuVendorComboBox"); if (c != null) c.SelectionChanged += GpuVendor_Changed;
        
        // New Interface controls
        var slider = GetCtrl<Slider>("MenuScaleSlider"); if (slider != null) slider.ValueChanged += MenuScaleSlider_ValueChanged;
        tb = GetCtrl<TextBox>("MenuScaleTextBox"); if (tb != null) tb.TextChanged += MenuScaleText_Changed;
        c = GetCtrl<ComboBox>("FpsPositionComboBox"); if (c != null) c.SelectionChanged += FpsPosition_Changed;
    }

    // Event handlers minimal
    private void Upscaler_Changed(object sender, SelectionChangedEventArgs e) { _ = SaveSettingsAutomatically(); }
    private void Quality_Changed(object sender, SelectionChangedEventArgs e) { _ = SaveSettingsAutomatically(); }
    private void FrameGen_Toggled(object sender, RoutedEventArgs e) { _ = SaveSettingsAutomatically(); }
    private void Overlay_Toggled(object sender, RoutedEventArgs e) { _ = SaveSettingsAutomatically(); }
    private void FPS_Toggled(object sender, RoutedEventArgs e) { _ = SaveSettingsAutomatically(); }
    private void ApplyCfg_Toggled(object sender, RoutedEventArgs e) { _ = SaveSettingsAutomatically(); }
    private void SpoofGpu_Changed(object sender, SelectionChangedEventArgs e) { _ = SaveSettingsAutomatically(); }
    private void DllCombo_Changed(object sender, SelectionChangedEventArgs e) { _ = SaveSettingsAutomatically(); }
    private void SharpOverride_Toggled(object sender, RoutedEventArgs e) { _ = SaveSettingsAutomatically(); }
    private void SharpValue_Changed(object sender, TextChangedEventArgs e) { _ = SaveSettingsAutomatically(); }
    private void Vfov_Changed(object sender, TextChangedEventArgs e) { _ = SaveSettingsAutomatically(); }
    private void LogLevel_Changed(object sender, SelectionChangedEventArgs e) { _ = SaveSettingsAutomatically(); }
    private void GpuVendor_Changed(object sender, SelectionChangedEventArgs e) { _ = SaveSettingsAutomatically(); }
    private void FpsPosition_Changed(object sender, SelectionChangedEventArgs e) { _ = SaveSettingsAutomatically(); }

    private void MenuScaleSlider_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        var textBox = GetCtrl<TextBox>("MenuScaleTextBox");
        if (textBox != null)
        {
            textBox.Text = e.NewValue.ToString("F1");
        }
        _ = SaveSettingsAutomatically();
    }

    private void MenuScaleText_Changed(object sender, TextChangedEventArgs e)
    {
        var textBox = sender as TextBox;
        var slider = GetCtrl<Slider>("MenuScaleSlider");
        if (textBox != null && slider != null && float.TryParse(textBox.Text, out var value))
        {
            slider.Value = Math.Clamp(value, 0.5f, 2.0f);
        }
    }

    private void PresetAuto_Click(object sender, RoutedEventArgs e)
    {
        // Leer preferencia del selector GPU
        var gpuCombo = GetCtrl<ComboBox>("GpuVendorComboBox");
        string vendorPreference = "auto";
        
        if (gpuCombo?.SelectedItem is ComboBoxItem selectedItem)
        {
            vendorPreference = selectedItem.Content?.ToString()?.ToLowerInvariant() ?? "auto";
        }

        string actualVendor;
        
        if (vendorPreference == "auto")
        {
            // Solo detectar si está en Auto
            (actualVendor, _) = _systemInfoService.DetectGpuVendor();
        }
        else
        {
            // Usar la preferencia manual (nvidia/amd/intel)
            actualVendor = vendorPreference;
        }

        // Aplicar configuración según el vendor (real o forzado)
        if (actualVendor == "nvidia")
        {
            ApplyPreset(s => { 
                s.DefaultUpscaler = "dlss"; 
                s.DefaultQualityPreset = "Quality"; 
                s.EnableFrameGeneration = false; 
                s.EnableGPUSpoofing = false; 
                s.SharpnessOverrideEnabled = true; 
                s.SharpnessValue = 0.35f; 
                s.VerticalFovOverride = 0f; 
                s.LogLevel = 2; 
                s.PreferredGpuVendor = vendorPreference; // Guardar preferencia, no detección
            }, $"Auto preset applied (NVIDIA)", "auto");
        }
        else if (actualVendor == "amd")
        {
            ApplyPreset(s => { 
                s.DefaultUpscaler = "fsr31"; 
                s.DefaultQualityPreset = "Balanced"; 
                s.EnableFrameGeneration = true; 
                s.EnableGPUSpoofing = false; 
                s.SharpnessOverrideEnabled = true; 
                s.SharpnessValue = 0.30f; 
                s.VerticalFovOverride = 0f; 
                s.LogLevel = 2; 
                s.PreferredGpuVendor = vendorPreference;
            }, $"Auto preset applied (AMD)", "auto");
        }
        else if (actualVendor == "intel")
        {
            ApplyPreset(s => { 
                s.DefaultUpscaler = "xess"; 
                s.DefaultQualityPreset = "Quality"; 
                s.EnableFrameGeneration = false; 
                s.EnableGPUSpoofing = false; 
                s.SharpnessOverrideEnabled = true; 
                s.SharpnessValue = 0.30f; 
                s.VerticalFovOverride = 0f; 
                s.LogLevel = 2; 
                s.PreferredGpuVendor = vendorPreference;
            }, $"Auto preset applied (Intel)", "auto");
        }
        else
        {
            ApplyPreset(s => { 
                s.DefaultUpscaler = "auto"; 
                s.DefaultQualityPreset = "Balanced"; 
                s.EnableFrameGeneration = false; 
                s.EnableGPUSpoofing = false; 
                s.SharpnessOverrideEnabled = false; 
                s.VerticalFovOverride = 0f; 
                s.LogLevel = 2; 
                s.PreferredGpuVendor = vendorPreference;
            }, $"Auto preset applied (Generic)", "auto");
        }
    }

    private void PresetDefault_Click(object sender, RoutedEventArgs e) => ApplyPreset(s => { s.DefaultUpscaler = "auto"; s.DefaultQualityPreset = "Quality"; s.EnableFrameGeneration = false; s.EnableGPUSpoofing=false; s.SharpnessOverrideEnabled=false; s.VerticalFovOverride=0f; s.LogLevel=2; }, "Default preset applied", "default");
    private void PresetPerformance_Click(object sender, RoutedEventArgs e) => ApplyPreset(s => { s.DefaultUpscaler = "fsr31"; s.DefaultQualityPreset="Performance"; s.EnableFrameGeneration=true; s.SharpnessOverrideEnabled=true; s.SharpnessValue=0.25f; s.EnableGPUSpoofing=false; s.VerticalFovOverride=0f; s.LogLevel=1; }, "Performance preset applied", "performance");
    private void PresetBalanced_Click(object sender, RoutedEventArgs e) => ApplyPreset(s => { s.DefaultUpscaler = "auto"; s.DefaultQualityPreset="Balanced"; s.EnableFrameGeneration=false; s.SharpnessOverrideEnabled=true; s.SharpnessValue=0.30f; s.EnableGPUSpoofing=false; s.VerticalFovOverride=0f; s.LogLevel=2; }, "Balanced preset applied", "balanced");
    private void PresetQuality_Click(object sender, RoutedEventArgs e) => ApplyPreset(s => { s.DefaultUpscaler = "dlss"; s.DefaultQualityPreset="Quality"; s.EnableFrameGeneration=false; s.SharpnessOverrideEnabled=true; s.SharpnessValue=0.35f; s.EnableGPUSpoofing=false; s.VerticalFovOverride=0f; s.LogLevel=2; }, "Quality preset applied", "quality");
    private void PresetCustom_Click(object sender, RoutedEventArgs e) { _currentSettings = CloneSettings(_customPresetSnapshot); UpdateUIFromSettings(); SetStatus("Custom preset restored", Microsoft.UI.Colors.LightGray); _activePreset = "custom"; UpdatePresetButtonStyles(); }

    private void ApplyPreset(Action<GlobalSettings> mutator, string message, string presetName)
    {
        _customPresetSnapshot = CloneSettings(_currentSettings);
        mutator(_currentSettings);
        _currentSettings.LastUpdated = DateTime.Now;
        UpdateUIFromSettings();
        _globalSettingsService.SaveSettingsAsync(_currentSettings);
        SetStatus(message, Microsoft.UI.Colors.LightGreen);
        _activePreset = presetName;
        UpdatePresetButtonStyles();
    }

    private void UpdatePresetButtonStyles()
    {
        // Update button styles to show active preset
        var presets = new Dictionary<string, string>
        {
            {"auto", "Auto"},
            {"default", "Default"},
            {"performance", "Performance"},
            {"balanced", "Balanced"},
            {"quality", "Quality"},
            {"custom", "Custom"}
        };

        foreach (var preset in presets)
        {
            var btn = FindName(preset.Value + "PresetBtn") as Button;
            if (btn != null)
            {
                if (preset.Key == _activePreset)
                {
                    // Active style
                    btn.Style = (Style)Application.Current.Resources["XboxButtonStyle"];
                }
                else
                {
                    // Inactive style
                    btn.Style = (Style)Application.Current.Resources["XboxSecondaryButtonStyle"];
                }
            }
        }
    }

    private GlobalSettings CloneSettings(GlobalSettings s) => new()
    {
        DefaultUpscaler = s.DefaultUpscaler,
        DefaultQualityPreset = s.DefaultQualityPreset,
        EnableFrameGeneration = s.EnableFrameGeneration,
        FrameGenerationType = s.FrameGenerationType,
        PreferredDllName = s.PreferredDllName,
        ApplyConfigToNewInstalls = s.ApplyConfigToNewInstalls,
        EnableGPUSpoofing = s.EnableGPUSpoofing,
        SpoofedGPU = s.SpoofedGPU,
        EnableOverlayMenu = s.EnableOverlayMenu,
        ShowFPSCounter = s.ShowFPSCounter,
        VerticalFovOverride = s.VerticalFovOverride,
        SharpnessOverrideEnabled = s.SharpnessOverrideEnabled,
        SharpnessValue = s.SharpnessValue,
        LogLevel = s.LogLevel,
        OptiFGDebugView = s.OptiFGDebugView,
        OptiFGAllowAsync = s.OptiFGAllowAsync,
        OptiFGHUDFix = s.OptiFGHUDFix,
        DLSSEnabled = s.DLSSEnabled,
        DLSSRenderPresetOverride = s.DLSSRenderPresetOverride,
        FSRCameraNear = s.FSRCameraNear,
        FSRCameraFar = s.FSRCameraFar,
        FSRDebugView = s.FSRDebugView,
        XeSSBuildPipelines = s.XeSSBuildPipelines,
        XeSSNetworkModel = s.XeSSNetworkModel,
        QualityRatioOverrideEnabled = s.QualityRatioOverrideEnabled,
        QualityRatioDLAA = s.QualityRatioDLAA,
        QualityRatioUltraQuality = s.QualityRatioUltraQuality,
        QualityRatioQuality = s.QualityRatioQuality,
        QualityRatioBalanced = s.QualityRatioBalanced,
        QualityRatioPerformance = s.QualityRatioPerformance,
        QualityRatioUltraPerformance = s.QualityRatioUltraPerformance,
        CASEnabled = s.CASEnabled,
        CASMotionSharpnessEnabled = s.CASMotionSharpnessEnabled,
        CASMotionSharpness = s.CASMotionSharpness,
        LogToFile = s.LogToFile,
        LogToConsole = s.LogToConsole,
        LastUpdated = s.LastUpdated,
        PreferredGpuVendor = s.PreferredGpuVendor
    };

    private void OpenDocumentation_Click(object sender, RoutedEventArgs e)
    {
        try { Process.Start(new ProcessStartInfo { FileName = "https://github.com/optiscaler/OptiScaler", UseShellExecute = true }); }
        catch (Exception ex) { SetStatus($"Doc error: {ex.Message}", Microsoft.UI.Colors.OrangeRed); }
    }

    private void UpdateUIFromSettings()
    {
        var upscaler = GetCtrl<ComboBox>("UpscalerComboBox");
        if (upscaler != null)
            upscaler.SelectedIndex = _currentSettings.DefaultUpscaler.ToLowerInvariant() switch { "dlss" => 1, "xess" => 2, "fsr22" => 3, "fsr31" => 4, _ => 0 };
        var quality = GetCtrl<ComboBox>("QualityComboBox");
        if (quality != null)
            quality.SelectedIndex = _currentSettings.DefaultQualityPreset switch { "Ultra Quality" => 0, "Quality" => 1, "Balanced" => 2, "Performance" => 3, "Ultra Performance" => 4, _ => 1 };
        
        // Restaurar selector GPU
        var gpuCombo = GetCtrl<ComboBox>("GpuVendorComboBox");
        if (gpuCombo != null)
        {
            var vendor = _currentSettings.PreferredGpuVendor?.ToLowerInvariant() ?? "auto";
            gpuCombo.SelectedIndex = vendor switch { "nvidia" => 1, "amd" => 2, "intel" => 3, _ => 0 };
        }
        
        SetToggle("FrameGenToggle", _currentSettings.EnableFrameGeneration);
        SetToggle("SpoofingToggle", _currentSettings.EnableGPUSpoofing);
        SetToggle("OverlayMenuToggle", _currentSettings.EnableOverlayMenu);
        SetToggle("FPSCounterToggle", _currentSettings.ShowFPSCounter);
        SetToggle("ApplyConfigToggle", _currentSettings.ApplyConfigToNewInstalls);
        
        // Interface controls
        var menuScaleSlider = GetCtrl<Slider>("MenuScaleSlider");
        if (menuScaleSlider != null) menuScaleSlider.Value = _currentSettings.MenuScale;
        var menuScaleText = GetCtrl<TextBox>("MenuScaleTextBox");
        if (menuScaleText != null) menuScaleText.Text = _currentSettings.MenuScale.ToString("F1");
        var fpsPos = GetCtrl<ComboBox>("FpsPositionComboBox");
        if (fpsPos != null) fpsPos.SelectedIndex = Math.Clamp(_currentSettings.FpsOverlayPos, 0, 3);
        
        var dllCombo = GetCtrl<ComboBox>("DllFilenameComboBox");
        if (dllCombo != null)
        {
            var dlls = new List<string>{"dxgi.dll","d3d12.dll","winmm.dll","version.dll","dbghelp.dll","wininet.dll","winhttp.dll","OptiScaler.asi"};
            var idx = dlls.IndexOf(_currentSettings.PreferredDllName);
            dllCombo.SelectedIndex = idx >= 0 ? idx : 0;
        }
        var vfov = GetCtrl<TextBox>("VerticalFovTextBox"); if (vfov != null) vfov.Text = _currentSettings.VerticalFovOverride > 0 ? _currentSettings.VerticalFovOverride.ToString("F1") : "0";
        SetToggle("AdvSharpnessOverrideToggle", _currentSettings.SharpnessOverrideEnabled);
        SetText("AdvSharpnessValueTextBox", _currentSettings.SharpnessOverrideEnabled ? _currentSettings.SharpnessValue : 0.3f);
        var advLog = GetCtrl<ComboBox>("AdvLogLevelComboBox"); if (advLog != null) advLog.SelectedIndex = Math.Clamp(_currentSettings.LogLevel,0,5);
        SetToggle("OptiFGDebugToggle", _currentSettings.OptiFGDebugView);
        SetToggle("OptiFGAsyncToggle", _currentSettings.OptiFGAllowAsync);
        SetToggle("OptiFGHudFixToggle", _currentSettings.OptiFGHUDFix);
        SetToggle("DlssEnabledToggle", _currentSettings.DLSSEnabled);
        SetToggle("DlssPresetOverrideToggle", _currentSettings.DLSSRenderPresetOverride);
        SetText("FsrNearTextBox", _currentSettings.FSRCameraNear);
        SetText("FsrFarTextBox", _currentSettings.FSRCameraFar);
        SetToggle("FsrDebugToggle", _currentSettings.FSRDebugView);
        SetToggle("XessBuildPipelinesToggle", _currentSettings.XeSSBuildPipelines);
        var xessModel = GetCtrl<ComboBox>("XessModelComboBox"); if (xessModel != null) xessModel.SelectedIndex = _currentSettings.XeSSNetworkModel;
        SetToggle("QualityOverrideToggle", _currentSettings.QualityRatioOverrideEnabled);
        SetText("RatioDLAABox", _currentSettings.QualityRatioDLAA);
        SetText("RatioUltraQBox", _currentSettings.QualityRatioUltraQuality);
        SetText("RatioQualityBox", _currentSettings.QualityRatioQuality);
        SetText("RatioBalancedBox", _currentSettings.QualityRatioBalanced);
        SetText("RatioPerformanceBox", _currentSettings.QualityRatioPerformance);
        SetText("RatioUltraPerfBox", _currentSettings.QualityRatioUltraPerformance);
        SetToggle("CasEnabledToggle", _currentSettings.CASEnabled);
        SetToggle("CasMotionToggle", _currentSettings.CASMotionSharpnessEnabled);
        SetText("CasMotionValueTextBox", _currentSettings.CASMotionSharpness);
        SetToggle("LogToFileToggle", _currentSettings.LogToFile);
        SetToggle("LogToConsoleToggle", _currentSettings.LogToConsole);
    }
    
    private void UpdateSettingsFromUI()
    {
        var upscaler = GetCtrl<ComboBox>("UpscalerComboBox");
        if (upscaler != null)
            _currentSettings.DefaultUpscaler = upscaler.SelectedIndex switch {1=>"dlss",2=>"xess",3=>"fsr22",4=>"fsr31",_=>"auto"};
        var quality = GetCtrl<ComboBox>("QualityComboBox");
        if (quality != null)
            _currentSettings.DefaultQualityPreset = quality.SelectedIndex switch {0=>"Ultra Quality",1=>"Quality",2=>"Balanced",3=>"Performance",4=>"Ultra Performance",_=>"Quality"};
        
        // Guardar selector GPU
        var gpuCombo = GetCtrl<ComboBox>("GpuVendorComboBox");
        if (gpuCombo?.SelectedItem is ComboBoxItem gpuItem)
        {
            _currentSettings.PreferredGpuVendor = gpuItem.Content?.ToString()?.ToLowerInvariant() ?? "auto";
        }
        
        _currentSettings.EnableFrameGeneration = GetCtrl<ToggleSwitch>("FrameGenToggle")?.IsOn ?? _currentSettings.EnableFrameGeneration;
        _currentSettings.FrameGenerationType = _currentSettings.EnableFrameGeneration ? "optifg" : "nofg";
        _currentSettings.EnableGPUSpoofing = GetCtrl<ToggleSwitch>("SpoofingToggle")?.IsOn ?? _currentSettings.EnableGPUSpoofing;
        _currentSettings.EnableOverlayMenu = GetCtrl<ToggleSwitch>("OverlayMenuToggle")?.IsOn ?? _currentSettings.EnableOverlayMenu;
        _currentSettings.ShowFPSCounter = GetCtrl<ToggleSwitch>("FPSCounterToggle")?.IsOn ?? _currentSettings.ShowFPSCounter;
        _currentSettings.ApplyConfigToNewInstalls = GetCtrl<ToggleSwitch>("ApplyConfigToggle")?.IsOn ?? _currentSettings.ApplyConfigToNewInstalls;
        
        // Interface controls
        var menuScaleText = GetCtrl<TextBox>("MenuScaleTextBox");
        if (menuScaleText != null && float.TryParse(menuScaleText.Text, out var menuScale))
        {
            _currentSettings.MenuScale = Math.Clamp(menuScale, 0.5f, 2.0f);
        }
        var fpsPos = GetCtrl<ComboBox>("FpsPositionComboBox");
        if (fpsPos != null) _currentSettings.FpsOverlayPos = fpsPos.SelectedIndex;
        
        var dllCombo = GetCtrl<ComboBox>("DllFilenameComboBox"); if (dllCombo?.SelectedItem is ComboBoxItem ci && ci.Content is string dllName) _currentSettings.PreferredDllName = dllName;
        var vfov = GetCtrl<TextBox>("VerticalFovTextBox"); if (vfov != null && float.TryParse(vfov.Text, out var vf)) _currentSettings.VerticalFovOverride = vf;
        var advSharpOverride = GetCtrl<ToggleSwitch>("AdvSharpnessOverrideToggle"); _currentSettings.SharpnessOverrideEnabled = advSharpOverride?.IsOn ?? _currentSettings.SharpnessOverrideEnabled;
        var advSharpValue = GetCtrl<TextBox>("AdvSharpnessValueTextBox"); if (advSharpValue != null && float.TryParse(advSharpValue.Text,out var sh)) _currentSettings.SharpnessValue = sh;
        if (_currentSettings.EnableGPUSpoofing)
        { var spoofGpu = GetCtrl<ComboBox>("SpoofGPUComboBox"); if (spoofGpu != null) _currentSettings.SpoofedGPU = spoofGpu.SelectedIndex switch {0=>"RTX 4090",1=>"RTX 4080",2=>"RTX 4070",_=>"RTX 4090"}; }
        var logCombo = GetCtrl<ComboBox>("AdvLogLevelComboBox"); if (logCombo?.SelectedItem is ComboBoxItem logItem && int.TryParse(logItem.Content?.ToString(), out var lvl)) _currentSettings.LogLevel = lvl;
        _currentSettings.OptiFGDebugView = GetCtrl<ToggleSwitch>("OptiFGDebugToggle")?.IsOn ?? _currentSettings.OptiFGDebugView;
        _currentSettings.OptiFGAllowAsync = GetCtrl<ToggleSwitch>("OptiFGAsyncToggle")?.IsOn ?? _currentSettings.OptiFGAllowAsync;
        _currentSettings.OptiFGHUDFix = GetCtrl<ToggleSwitch>("OptiFGHudFixToggle")?.IsOn ?? _currentSettings.OptiFGHUDFix;
        _currentSettings.DLSSEnabled = GetCtrl<ToggleSwitch>("DlssEnabledToggle")?.IsOn ?? _currentSettings.DLSSEnabled;
        _currentSettings.DLSSRenderPresetOverride = GetCtrl<ToggleSwitch>("DlssPresetOverrideToggle")?.IsOn ?? _currentSettings.DLSSRenderPresetOverride;
        if (float.TryParse(GetCtrl<TextBox>("FsrNearTextBox")?.Text, out var nearVal)) _currentSettings.FSRCameraNear = nearVal;
        if (float.TryParse(GetCtrl<TextBox>("FsrFarTextBox")?.Text, out var farVal)) _currentSettings.FSRCameraFar = farVal;
        _currentSettings.FSRDebugView = GetCtrl<ToggleSwitch>("FsrDebugToggle")?.IsOn ?? _currentSettings.FSRDebugView;
        _currentSettings.XeSSBuildPipelines = GetCtrl<ToggleSwitch>("XessBuildPipelinesToggle")?.IsOn ?? _currentSettings.XeSSBuildPipelines;
        var xessModel = GetCtrl<ComboBox>("XessModelComboBox"); if (xessModel != null) _currentSettings.XeSSNetworkModel = xessModel.SelectedIndex;
        _currentSettings.QualityRatioOverrideEnabled = GetCtrl<ToggleSwitch>("QualityOverrideToggle")?.IsOn ?? _currentSettings.QualityRatioOverrideEnabled;
        ParseRatio("RatioDLAABox", v => _currentSettings.QualityRatioDLAA = v);
        ParseRatio("RatioUltraQBox", v => _currentSettings.QualityRatioUltraQuality = v);
        ParseRatio("RatioQualityBox", v => _currentSettings.QualityRatioQuality = v);
        ParseRatio("RatioBalancedBox", v => _currentSettings.QualityRatioBalanced = v);
        ParseRatio("RatioPerformanceBox", v => _currentSettings.QualityRatioPerformance = v);
        ParseRatio("RatioUltraPerfBox", v => _currentSettings.QualityRatioUltraPerformance = v);
        _currentSettings.CASEnabled = GetCtrl<ToggleSwitch>("CasEnabledToggle")?.IsOn ?? _currentSettings.CASEnabled;
        _currentSettings.CASMotionSharpnessEnabled = GetCtrl<ToggleSwitch>("CasMotionToggle")?.IsOn ?? _currentSettings.CASMotionSharpnessEnabled;
        if (float.TryParse(GetCtrl<TextBox>("CasMotionValueTextBox")?.Text, out var casVal)) _currentSettings.CASMotionSharpness = casVal;
        _currentSettings.LogToFile = GetCtrl<ToggleSwitch>("LogToFileToggle")?.IsOn ?? _currentSettings.LogToFile;
        _currentSettings.LogToConsole = GetCtrl<ToggleSwitch>("LogToConsoleToggle")?.IsOn ?? _currentSettings.LogToConsole;
        _currentSettings.LastUpdated = DateTime.Now;
    }

    private async void SpoofingToggle_Toggled(object sender, RoutedEventArgs e)
    {
        var panel = GetCtrl<StackPanel>("SpoofingOptionsPanel");
        var spoofTs = GetCtrl<ToggleSwitch>("SpoofingToggle");
        if (panel != null && spoofTs != null)
            panel.Visibility = spoofTs.IsOn ? Visibility.Visible : Visibility.Collapsed;
        await SaveSettingsAutomatically();
    }

    private async Task SaveSettingsAutomatically()
    {
        try
        {
            UpdateSettingsFromUI();
            await _globalSettingsService.SaveSettingsAsync(_currentSettings);
            _lastSavedSnapshot = CloneSettings(_currentSettings);
            SetStatus("Settings auto-saved", Microsoft.UI.Colors.LightGreen);
            await Task.Delay(1200);
            SetStatus("Settings are saved automatically", Microsoft.UI.Colors.LightGray);
        }
        catch (Exception ex)
        {
            SetStatus($"Auto-save failed: {ex.Message}", Microsoft.UI.Colors.Orange);
        }
    }

    private async Task CheckInstalledMods()
    {
        try
        {
            var modsBaseDir = IOPath.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OptiScaler Manager", "Mods");
            var optiScalerDir = IOPath.Combine(modsBaseDir, "OptiScaler");
            var optiPatcherDir = IOPath.Combine(modsBaseDir, "OptiPatcher");
            _hasOptiScalerInstalled = Directory.Exists(optiScalerDir);
            _hasOptiPatcherInstalled = Directory.Exists(optiPatcherDir);
            var cachedGames = await _storageService.LoadGamesAsync();
            if (cachedGames.Any())
            {
                _hasOptiScalerInstalled |= cachedGames.Any(g => g.HasOptiScaler);
                _hasOptiPatcherInstalled |= cachedGames.Any(g => g.HasOptiPatcher);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ModConfigPage] CheckInstalledMods error: {ex.Message}");
        }
    }

    private void UpdateUIVisibility()
    {
        var info = GetCtrl<InfoBar>("ModsInfoBar");
        bool hasMods = _hasOptiScalerInstalled || _hasOptiPatcherInstalled;
        if (info != null)
        {
            info.IsOpen = !hasMods;
        }
    }

    private void SetToggle(string name, bool value)
    {
        var t = GetCtrl<ToggleSwitch>(name);
        if (t != null) t.IsOn = value;
    }

    private void SetText(string name, float value)
    {
        var tb = GetCtrl<TextBox>(name);
        if (tb != null) tb.Text = value.ToString("F2");
    }

    private void ParseRatio(string name, Action<float> setter)
    {
        var tb = GetCtrl<TextBox>(name);
        if (tb != null && float.TryParse(tb.Text, out var v))
            setter(v);
    }
}


