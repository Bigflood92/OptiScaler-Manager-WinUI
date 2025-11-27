using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI;
using OptiScaler.Core.Models;
using OptiScaler.Core.Services;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace OptiScaler.UI.Dialogs;

public sealed partial class GameConfigDialog : ContentDialog
{
    private GameInfo _game = null!;
    private readonly GameScannerService _scannerService;
    private readonly ModInstallerService _modInstallerService;
    private readonly GitHubService _githubService;
    private readonly GlobalSettingsService _globalSettingsService;
    private readonly StorageService _storageService;
    private bool _hasChanges = false;

    public GameInfo Game
    {
        get => _game;
        set
        {
            _game = value;
            LoadGameData();
        }
    }

    public GameConfigDialog()
    {
        this.InitializeComponent();
        _scannerService = new GameScannerService();
        _githubService = new GitHubService();
        _modInstallerService = new ModInstallerService(_githubService);
        _globalSettingsService = new GlobalSettingsService();
        _storageService = new StorageService();
    }

    private void LoadGameData()
    {
        if (_game == null) return;

        Title = _game.Name;

        // Platform
        PlatformIcon.Glyph = GetPlatformIcon(_game.Platform);
        PlatformText.Text = _game.Platform.ToString();

        // Executable
        ExecutableText.Text = Path.GetFileName(_game.Executable);

        // Install Directory
        InstallPathTextBox.Text = _game.InstallDirectory ?? _game.Path;

        // Update primary button text based on mod installation status
        UpdatePrimaryButtonText();

        // Show/hide configuration panels
        UpdatePanelVisibility();

        // Load configuration if mods are installed
        if (_game.HasOptiScaler)
        {
            _ = LoadConfigurationAsync();
        }
    }

    private void UpdatePrimaryButtonText()
    {
        // Change primary button text based on mod installation status
        if (_game.HasOptiScaler || _game.HasOptiPatcher)
        {
            PrimaryButtonText = "Apply Changes";
        }
        else
        {
            PrimaryButtonText = "Install Mod";
        }
    }

    private void UpdatePanelVisibility()
    {
        // Show Quick Configuration only if mods are installed
        if (QuickConfigPanel != null)
            QuickConfigPanel.Visibility = _game.HasOptiScaler ? Visibility.Visible : Visibility.Collapsed;
        
        // Show Advanced Tab only if mods are installed
        if (AdvancedTab != null)
            AdvancedTab.Visibility = _game.HasOptiScaler ? Visibility.Visible : Visibility.Collapsed;
        
        // Show Advanced Settings Panel only if mods are installed
        if (AdvancedSettingsPanel != null)
            AdvancedSettingsPanel.Visibility = _game.HasOptiScaler ? Visibility.Visible : Visibility.Collapsed;
    }

    private async Task LoadConfigurationAsync()
    {
        try
        {
            var iniPath = Path.Combine(_game.InstallDirectory ?? _game.Path, "OptiScaler.ini");
            if (!File.Exists(iniPath)) return;

            var configService = new OptiScalerConfigService();
            var config = configService.ReadConfig(iniPath);

            // Upscaler
            var upscaler = config.Dx12Upscaler?.ToLowerInvariant() ?? "auto";
            UpscalerComboBox.SelectedIndex = upscaler switch
            {
                "dlss" => 1,
                "xess" => 2,
                "fsr22" => 3,
                "fsr31" => 4,
                _ => 0
            };

            // Quality
            var quality = config.GetQualityPresetName();
            QualityComboBox.SelectedIndex = quality switch
            {
                "Ultra Quality" => 0,
                "Quality" => 1,
                "Balanced" => 2,
                "Performance" => 3,
                "Ultra Performance" => 4,
                _ => 1
            };

            // Frame Generation
            FrameGenToggle.IsOn = config.IsFrameGenerationEnabled();

            // Overlay Menu
            OverlayMenuToggle.IsOn = config.OverlayMenu;

            // DLL Filename - Load from global settings if available
            var globalSettings = await _globalSettingsService.LoadSettingsAsync();
            var dllOptions = new[] { "dxgi.dll", "d3d12.dll", "winmm.dll", "version.dll", "dbghelp.dll", "wininet.dll", "winhttp.dll", "OptiScaler.asi" };
            var dllIndex = Array.IndexOf(dllOptions, globalSettings.PreferredDllName ?? "dxgi.dll");
            DllFilenameComboBox.SelectedIndex = dllIndex >= 0 ? dllIndex : 0;

            // Sharpness
            SharpnessSlider.Value = config.Sharpness;
            SharpnessValueText.Text = config.Sharpness.ToString("F2");

            // Advanced Settings
            if (ShowFpsToggle != null) ShowFpsToggle.IsOn = config.ShowFps;
            if (MenuScaleSlider != null) MenuScaleSlider.Value = config.MenuScale;
            if (DlssEnabledToggle != null) DlssEnabledToggle.IsOn = config.DLSSEnabled;
            if (DlssPresetOverrideToggle != null) DlssPresetOverrideToggle.IsOn = config.DLSSRenderPresetOverride;
            
            if (FsrNearTextBox != null && config.FSRCameraNear > 0) FsrNearTextBox.Text = config.FSRCameraNear.ToString();
            if (FsrFarTextBox != null && config.FSRCameraFar > 0) FsrFarTextBox.Text = config.FSRCameraFar.ToString();
            if (FsrDebugToggle != null) FsrDebugToggle.IsOn = config.FSRDebugView;
            
            if (LogLevelComboBox != null) LogLevelComboBox.SelectedIndex = Math.Clamp(config.LogLevel, 0, 5);
            if (LogToFileToggle != null) LogToFileToggle.IsOn = config.LogToFile;
            if (LogToConsoleToggle != null) LogToConsoleToggle.IsOn = config.LogToConsole;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[GameConfigDialog] Error loading configuration: {ex.Message}");
        }
    }

    private void SharpnessSlider_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        if (SharpnessValueText != null)
        {
            SharpnessValueText.Text = e.NewValue.ToString("F2");
        }
    }

    private void TabChanged(object sender, RoutedEventArgs e)
    {
        if (sender is not RadioButton radioButton) return;

        // Hide all tab contents
        if (GeneralContent != null) GeneralContent.Visibility = Visibility.Collapsed;
        if (PresetsContent != null) PresetsContent.Visibility = Visibility.Collapsed;
        if (AdvancedContent != null) AdvancedContent.Visibility = Visibility.Collapsed;

        // Show selected tab content
        if (radioButton == GeneralTab && GeneralContent != null)
        {
            GeneralContent.Visibility = Visibility.Visible;
        }
        else if (radioButton == PresetsTab && PresetsContent != null)
        {
            PresetsContent.Visibility = Visibility.Visible;
        }
        else if (radioButton == AdvancedTab && AdvancedContent != null)
        {
            AdvancedContent.Visibility = Visibility.Visible;
        }
    }

    private async void OpenFolder_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var path = _game.InstallDirectory ?? _game.Path;
            if (Directory.Exists(path))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = path,
                    UseShellExecute = true
                });
            }
            else
            {
                await ShowMessageAsync("Folder not found", "The game folder does not exist.", "OK");
            }
        }
        catch (Exception ex)
        {
            await ShowMessageAsync("Error", $"Failed to open folder: {ex.Message}", "OK");
        }
    }

    private async void ChangePath_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var folderPicker = new FolderPicker();
            folderPicker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            folderPicker.FileTypeFilter.Add("*");

            var hwnd = WindowNative.GetWindowHandle((Application.Current as App)?.m_window);
            InitializeWithWindow.Initialize(folderPicker, hwnd);

            var folder = await folderPicker.PickSingleFolderAsync();
            if (folder == null) return;

            var newPath = folder.Path;

            // Validate new path
            if (!Directory.Exists(newPath))
            {
                await ShowMessageAsync("Invalid Path", "The selected folder does not exist.", "OK");
                return;
            }

            // Check if mods are installed
            if (_game.HasOptiScaler || _game.HasOptiPatcher)
            {
                var result = await ShowConfirmationAsync(
                    "Move Mods?",
                    "OptiScaler mods are currently installed. Do you want to move them to the new location?",
                    "Move Files",
                    "Change Path Only");

                if (result == ContentDialogResult.Primary)
                {
                    await MoveModsToNewPathAsync(newPath);
                }
            }

            // Update path
            var oldPath = _game.InstallDirectory ?? _game.Path;
            _game.InstallDirectory = newPath;
            
            // Try to find executable in new path
            var exePath = FindExecutableInPath(newPath);
            if (!string.IsNullOrEmpty(exePath))
            {
                _game.Executable = exePath;
            }

            InstallPathTextBox.Text = newPath;
            ExecutableText.Text = Path.GetFileName(_game.Executable);

            // Save changes
            await _storageService.SaveGamesAsync(new[] { _game });
            
            _hasChanges = true;
            ShowStatus($"Install path changed to: {newPath}", true);
        }
        catch (Exception ex)
        {
            await ShowMessageAsync("Error", $"Failed to change path: {ex.Message}", "OK");
        }
    }

    private async Task MoveModsToNewPathAsync(string newPath)
    {
        try
        {
            var oldPath = _game.InstallDirectory ?? _game.Path;
            
            ShowStatus("Moving mod files...", false);

            var filesToMove = new[]
            {
                "OptiScaler.dll", "OptiScaler.ini",
                "nvngx.dll", "libxess.dll", "libxess_dx11.dll",
                "amd_fidelityfx_vk.dll", "amd_fidelityfx_dx12.dll",
                "amd_fidelityfx_upscaler_dx12.dll", "amd_fidelityfx_framegeneration_dx12.dll",
                "OptiPatcher.asi", "nvngx_patch.dll"
            };

            foreach (var fileName in filesToMove)
            {
                var sourcePath = Path.Combine(oldPath, fileName);
                if (File.Exists(sourcePath))
                {
                    var destPath = Path.Combine(newPath, fileName);
                    File.Move(sourcePath, destPath, overwrite: true);
                    Debug.WriteLine($"[GameConfigDialog] Moved: {fileName}");
                }
            }

            // Move folders
            var foldersToMove = new[] { "nvngx", "mods", "plugins" };
            foreach (var folderName in foldersToMove)
            {
                var sourcePath = Path.Combine(oldPath, folderName);
                if (Directory.Exists(sourcePath))
                {
                    var destPath = Path.Combine(newPath, folderName);
                    if (Directory.Exists(destPath))
                    {
                        Directory.Delete(destPath, recursive: true);
                    }
                    Directory.Move(sourcePath, destPath);
                    Debug.WriteLine($"[GameConfigDialog] Moved folder: {folderName}");
                }
            }

            ShowStatus("Mods moved successfully!", true);
            
            // Refresh mod status
            await _scannerService.RefreshModStatusAsync(_game);
            UpdatePanelVisibility();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[GameConfigDialog] Error moving mods: {ex.Message}");
            await ShowMessageAsync("Error Moving Mods", $"Some files could not be moved: {ex.Message}", "OK");
        }
    }

    private string? FindExecutableInPath(string path)
    {
        try
        {
            var exeFiles = Directory.GetFiles(path, "*.exe", SearchOption.TopDirectoryOnly);
            if (exeFiles.Length > 0)
            {
                return exeFiles[0];
            }

            // Check common subdirectories
            var commonFolders = new[] { "bin", "Binaries", "Game" };
            foreach (var folder in commonFolders)
            {
                var subPath = Path.Combine(path, folder);
                if (Directory.Exists(subPath))
                {
                    exeFiles = Directory.GetFiles(subPath, "*.exe", SearchOption.TopDirectoryOnly);
                    if (exeFiles.Length > 0)
                    {
                        return exeFiles[0];
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[GameConfigDialog] Error finding executable: {ex.Message}");
        }

        return null;
    }

    private async void LaunchGame_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (File.Exists(_game.Executable))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = _game.Executable,
                    UseShellExecute = true,
                    WorkingDirectory = Path.GetDirectoryName(_game.Executable)
                });
                
                ShowStatus($"Launching {_game.Name}...", true);
            }
            else
            {
                await ShowMessageAsync("Executable Not Found", 
                    $"The game executable was not found:\n{_game.Executable}", "OK");
            }
        }
        catch (Exception ex)
        {
            await ShowMessageAsync("Launch Failed", $"Failed to launch game: {ex.Message}", "OK");
        }
    }

    private async void EditIni_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var iniPath = Path.Combine(_game.InstallDirectory ?? _game.Path, "OptiScaler.ini");
            
            if (!File.Exists(iniPath))
            {
                var result = await ShowConfirmationAsync(
                    "Config Not Found",
                    "OptiScaler.ini not found. Create a default configuration file?",
                    "Create",
                    "Cancel");

                if (result == ContentDialogResult.Primary)
                {
                    await CreateDefaultConfigAsync(iniPath);
                }
                else
                {
                    return;
                }
            }

            // Open with default text editor
            Process.Start(new ProcessStartInfo
            {
                FileName = iniPath,
                UseShellExecute = true
            });
            
            ShowStatus("Opening OptiScaler.ini in default editor...", true);
        }
        catch (Exception ex)
        {
            await ShowMessageAsync("Error", $"Failed to open INI file: {ex.Message}", "OK");
        }
    }

    private async Task CreateDefaultConfigAsync(string iniPath)
    {
        try
        {
            var globalSettings = await _globalSettingsService.LoadSettingsAsync();
            var config = new OptiScalerConfig();
            
            _globalSettingsService.ApplyGlobalSettingsToConfig(config, globalSettings);
            
            var configService = new OptiScalerConfigService();
            configService.WriteConfig(iniPath, config);
            
            ShowStatus("Default configuration created successfully!", true);
        }
        catch (Exception ex)
        {
            await ShowMessageAsync("Error", $"Failed to create default config: {ex.Message}", "OK");
        }
    }

    private async void PresetAuto_Click(object sender, RoutedEventArgs e)
    {
        await ApplyPresetAsync("auto", "Auto preset applied");
    }

    private async void PresetPerformance_Click(object sender, RoutedEventArgs e)
    {
        await ApplyPresetAsync("performance", "Performance preset applied");
    }

    private async void PresetBalanced_Click(object sender, RoutedEventArgs e)
    {
        await ApplyPresetAsync("balanced", "Balanced preset applied");
    }

    private async void PresetQuality_Click(object sender, RoutedEventArgs e)
    {
        await ApplyPresetAsync("quality", "Quality preset applied");
    }

    private async Task ApplyPresetAsync(string presetName, string message)
    {
        try
        {
            switch (presetName.ToLowerInvariant())
            {
                case "auto":
                    UpscalerComboBox.SelectedIndex = 0; // Auto
                    QualityComboBox.SelectedIndex = 1; // Quality
                    FrameGenToggle.IsOn = false;
                    SharpnessSlider.Value = 0.30f;
                    break;

                case "performance":
                    UpscalerComboBox.SelectedIndex = 4; // FSR 3.1
                    QualityComboBox.SelectedIndex = 3; // Performance
                    FrameGenToggle.IsOn = true;
                    SharpnessSlider.Value = 0.25f;
                    break;

                case "balanced":
                    UpscalerComboBox.SelectedIndex = 0; // Auto
                    QualityComboBox.SelectedIndex = 2; // Balanced
                    FrameGenToggle.IsOn = false;
                    SharpnessSlider.Value = 0.30f;
                    break;

                case "quality":
                    UpscalerComboBox.SelectedIndex = 1; // DLSS
                    QualityComboBox.SelectedIndex = 1; // Quality
                    FrameGenToggle.IsOn = false;
                    SharpnessSlider.Value = 0.35f;
                    break;
            }

            ShowStatus(message, true);
            
            // Auto-hide status message
            await Task.Delay(2000);
            StatusMessageBorder.Visibility = Visibility.Collapsed;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[GameConfigDialog] Error applying preset: {ex.Message}");
        }
    }

    private async void PrimaryButton_Click(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        args.Cancel = true; // Prevent dialog from closing immediately

        try
        {
            // If mods are NOT installed, install them
            if (!_game.HasOptiScaler && !_game.HasOptiPatcher)
            {
                await InstallModsAsync();
            }
            else
            {
                // If mods ARE installed, save configuration
                await SaveConfigurationAndCloseAsync();
            }
        }
        catch (Exception ex)
        {
            await ShowMessageAsync("Error", $"Operation failed: {ex.Message}", "OK");
        }
    }

    private void SecondaryButton_Click(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        // Just close the dialog
    }

    private async Task InstallModsAsync()
    {
        try
        {
            ShowStatus("Installing mods...", false);
            IsPrimaryButtonEnabled = false;
            IsSecondaryButtonEnabled = false;

            // Download mods if needed
            var modsBaseDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "OptiScaler Manager", "Mods");

            var optiScalerDir = Path.Combine(modsBaseDir, "OptiScaler");
            var globalSettings = await _globalSettingsService.LoadSettingsAsync();
            var dllName = globalSettings.PreferredDllName;

            // Check if OptiScaler is downloaded
            string? optiScalerPath = null;
            if (Directory.Exists(optiScalerDir))
            {
                var files = Directory.GetFiles(optiScalerDir, "*.7z");
                if (files.Length > 0)
                {
                    optiScalerPath = files[0];
                }
            }

            if (string.IsNullOrEmpty(optiScalerPath))
            {
                ShowStatus("Downloading OptiScaler...", false);
                var downloadResult = await _githubService.DownloadLatestModAsync(
                    ModType.OptiScaler,
                    Path.Combine(optiScalerDir, "OptiScaler_latest.7z"));

                if (downloadResult == null)
                {
                    ShowStatus("Failed to download OptiScaler!", false);
                    return;
                }

                optiScalerPath = downloadResult.Value.FilePath;
            }

            // Install OptiScaler
            ShowStatus("Installing OptiScaler...", false);
            var result = await _modInstallerService.InstallModAsync(_game, ModType.OptiScaler, optiScalerPath, dllName);

            if (result.Success)
            {
                // Refresh status
                await _scannerService.RefreshModStatusAsync(_game);
                UpdatePrimaryButtonText();
                UpdatePanelVisibility();
                await LoadConfigurationAsync();
                
                _hasChanges = true;
                ShowStatus("Mods installed successfully!", true);
                
                // Wait a moment to show success message, then close
                await Task.Delay(1500);
                this.Hide();
            }
            else
            {
                ShowStatus($"Installation failed: {result.ErrorMessage}", false);
            }
        }
        catch (Exception ex)
        {
            ShowStatus($"Error: {ex.Message}", false);
        }
        finally
        {
            IsPrimaryButtonEnabled = true;
            IsSecondaryButtonEnabled = true;
        }
    }

    private async Task SaveConfigurationAndCloseAsync()
    {
        try
        {
            var iniPath = Path.Combine(_game.InstallDirectory ?? _game.Path, "OptiScaler.ini");
            var configService = new OptiScalerConfigService();
            
            OptiScalerConfig config;
            if (File.Exists(iniPath))
            {
                config = configService.ReadConfig(iniPath);
            }
            else
            {
                config = new OptiScalerConfig();
            }

            // Apply UI values - Basic Configuration
            var upscaler = UpscalerComboBox.SelectedIndex switch
            {
                1 => "dlss",
                2 => "xess",
                3 => "fsr22",
                4 => "fsr31",
                _ => "auto"
            };
            
            config.Dx12Upscaler = upscaler;
            config.Dx11Upscaler = upscaler;
            config.VulkanUpscaler = upscaler;

            // Frame Generation
            config.OptiFGEnabled = FrameGenToggle.IsOn;
            config.FGType = FrameGenToggle.IsOn ? "optifg" : "nofg";

            // Overlay
            config.OverlayMenu = OverlayMenuToggle.IsOn;
            if (ShowFpsToggle != null) config.ShowFps = ShowFpsToggle.IsOn;
            if (MenuScaleSlider != null) config.MenuScale = (float)MenuScaleSlider.Value;

            // DLL Filename - Save to global settings
            if (DllFilenameComboBox?.SelectedItem is ComboBoxItem dllItem && dllItem.Content is string dllName)
            {
                var globalSettings = await _globalSettingsService.LoadSettingsAsync();
                globalSettings.PreferredDllName = dllName;
                await _globalSettingsService.SaveSettingsAsync(globalSettings);
            }

            // Sharpness
            config.Sharpness = (float)SharpnessSlider.Value;

            // Advanced Settings - DLSS
            if (DlssEnabledToggle != null) config.DLSSEnabled = DlssEnabledToggle.IsOn;
            if (DlssPresetOverrideToggle != null) config.DLSSRenderPresetOverride = DlssPresetOverrideToggle.IsOn;

            // Advanced Settings - FSR
            if (FsrNearTextBox != null && float.TryParse(FsrNearTextBox.Text, out var nearVal))
                config.FSRCameraNear = nearVal;
            if (FsrFarTextBox != null && float.TryParse(FsrFarTextBox.Text, out var farVal))
                config.FSRCameraFar = farVal;
            if (FsrDebugToggle != null) config.FSRDebugView = FsrDebugToggle.IsOn;

            // Advanced Settings - Logging
            if (LogLevelComboBox != null) config.LogLevel = LogLevelComboBox.SelectedIndex;
            if (LogToFileToggle != null) config.LogToFile = LogToFileToggle.IsOn;
            if (LogToConsoleToggle != null) config.LogToConsole = LogToConsoleToggle.IsOn;

            // Save
            configService.WriteConfig(iniPath, config);

            // Refresh game info
            await _scannerService.RefreshModStatusAsync(_game);
            
            _hasChanges = true;
            ShowStatus("Configuration saved successfully!", true);
            
            // Wait a moment to show success message
            await Task.Delay(800);
            
            // Close dialog
            this.Hide();
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to save configuration: {ex.Message}", ex);
        }
    }

    private string GetPlatformIcon(GamePlatform platform)
    {
        return platform switch
        {
            GamePlatform.Steam => "\uE8AD",
            GamePlatform.Epic => "\uE7FC",
            GamePlatform.Xbox => "\uE990",
            GamePlatform.GOG => "\uE838",
            GamePlatform.EA => "\uE7FC",
            GamePlatform.Ubisoft => "\uE7FC",
            GamePlatform.Manual => "\uE8B7",
            _ => "\uE7FC"
        };
    }

    private string GetTimeAgo(DateTime dateTime)
    {
        var timeSpan = DateTime.Now - dateTime;
        
        if (timeSpan.TotalMinutes < 1) return "Just now";
        if (timeSpan.TotalMinutes < 60) return $"{(int)timeSpan.TotalMinutes} minutes ago";
        if (timeSpan.TotalHours < 24) return $"{(int)timeSpan.TotalHours} hours ago";
        if (timeSpan.TotalDays < 7) return $"{(int)timeSpan.TotalDays} days ago";
        
        return dateTime.ToShortDateString();
    }

    private void ShowStatus(string message, bool isSuccess)
    {
        StatusMessageBorder.Visibility = Visibility.Visible;
        StatusMessageText.Text = message;
        StatusMessageBorder.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
            isSuccess 
                ? Windows.UI.Color.FromArgb(255, 22, 163, 74) // Green
                : Windows.UI.Color.FromArgb(255, 30, 58, 138)); // Blue

        // Auto-hide after 3 seconds
        _ = Task.Run(async () =>
        {
            await Task.Delay(3000);
            DispatcherQueue.TryEnqueue(() =>
            {
                StatusMessageBorder.Visibility = Visibility.Collapsed;
            });
        });
    }

    private async Task<ContentDialogResult> ShowConfirmationAsync(string title, string message, string primaryText, string secondaryText)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            PrimaryButtonText = primaryText,
            SecondaryButtonText = secondaryText,
            DefaultButton = ContentDialogButton.Secondary,
            XamlRoot = this.XamlRoot
        };

        return await dialog.ShowAsync();
    }

    private async Task ShowMessageAsync(string title, string message, string closeButtonText)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            CloseButtonText = closeButtonText,
            XamlRoot = this.XamlRoot
        };

        await dialog.ShowAsync();
    }

    public bool HasChanges => _hasChanges;
}
