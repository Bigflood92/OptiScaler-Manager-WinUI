using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using OptiScaler.Core.Services;
using OptiScaler.Core;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace OptiScaler.UI.Views;

public sealed partial class AppSettingsPage : Page
{
    private readonly GlobalSettingsService _settingsService;
    private readonly ObservableCollection<string> _customPaths;
    private bool _initialized;

    public AppSettingsPage()
    {
        this.InitializeComponent();
        _settingsService = new GlobalSettingsService();
        _customPaths = new ObservableCollection<string>();
        this.Loaded += AppSettingsPage_Loaded;
        InitializeUI();
    }

    private void InitializeUI()
    {
        AppNameText.Text = AppInfo.AppName;
        AppVersionText.Text = AppInfo.Version;
        var dataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OptiScaler Manager");
        DataFolderText.Text = dataFolder;
        CustomPathsList.ItemsSource = _customPaths;
        InitializeAutoSaveEvents();
    }

    private void InitializeAutoSaveEvents()
    {
        SteamToggle.Toggled += async (s, e) => await SaveSettingsAutomatically();
        EpicToggle.Toggled += async (s, e) => await SaveSettingsAutomatically();
        XboxToggle.Toggled += async (s, e) => await SaveSettingsAutomatically();
        GOGToggle.Toggled += async (s, e) => await SaveSettingsAutomatically();
        EAToggle.Toggled += async (s, e) => await SaveSettingsAutomatically();
        UbisoftToggle.Toggled += async (s, e) => await SaveSettingsAutomatically();
        DeepScanToggle.Toggled += async (s, e) => await SaveSettingsAutomatically();
        AutoScanToggle.Toggled += async (s, e) => await SaveSettingsAutomatically();
        StartWithWindowsToggle.Toggled += async (s, e) => await SaveSettingsAutomatically();
        StartMinimizedToggle.Toggled += async (s, e) => await SaveSettingsAutomatically();
        NotificationsToggle.Toggled += async (s, e) => await SaveSettingsAutomatically();
    }

    private void SettingsNavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is NavigationViewItem item)
        {
            var tag = item.Tag as string;
            var panels = new[]{"GeneralPanel","ScanningPanel","PlatformsPanel","PreferencesPanel"};
            foreach (var pName in panels)
            {
                var p = this.FindName(pName) as StackPanel;
                if (p != null) p.Visibility = Visibility.Collapsed;
            }
            var panelName = tag + "Panel";
            var target = this.FindName(panelName) as StackPanel;
            if (target != null) target.Visibility = Visibility.Visible;
        }
    }

    private async void AppSettingsPage_Loaded(object sender, RoutedEventArgs e)
    {
        if (!_initialized)
        {
            await LoadSettings();
            await DetectPlatformPaths();
            _initialized = true;
        }
    }

    private async Task DetectPlatformPaths()
    {
        try
        {
            var settings = await _settingsService.LoadSettingsAsync();
            
            // Steam
            var steamPath = !string.IsNullOrEmpty(settings.SteamLibraryPath) 
                ? settings.SteamLibraryPath 
                : DetectSteamPath();
            if (this.FindName("SteamPathText") is TextBlock steamTb) steamTb.Text = $"Path: {steamPath}";
            
            // Epic
            var epicPath = !string.IsNullOrEmpty(settings.EpicLibraryPath) 
                ? settings.EpicLibraryPath 
                : DetectEpicPath();
            if (this.FindName("EpicPathText") is TextBlock epicTb) epicTb.Text = $"Path: {epicPath}";
            
            // Xbox
            var xboxPath = !string.IsNullOrEmpty(settings.XboxLibraryPath) 
                ? settings.XboxLibraryPath 
                : DetectXboxPath();
            if (this.FindName("XboxPathText") is TextBlock xboxTb) xboxTb.Text = $"Path: {xboxPath}";
            
            // GOG
            var gogPath = !string.IsNullOrEmpty(settings.GOGLibraryPath) 
                ? settings.GOGLibraryPath 
                : DetectGOGPath();
            if (this.FindName("GOGPathText") is TextBlock gogTb) gogTb.Text = $"Path: {gogPath}";
            
            // EA
            var eaPath = !string.IsNullOrEmpty(settings.EALibraryPath) 
                ? settings.EALibraryPath 
                : DetectEAPath();
            if (this.FindName("EAPathText") is TextBlock eaTb) eaTb.Text = $"Path: {eaPath}";
            
            // Ubisoft
            var ubisoftPath = !string.IsNullOrEmpty(settings.UbisoftLibraryPath) 
                ? settings.UbisoftLibraryPath 
                : DetectUbisoftPath();
            if (this.FindName("UbisoftPathText") is TextBlock ubisoftTb) ubisoftTb.Text = $"Path: {ubisoftPath}";
        }
        catch (Exception ex)
        {
            StatusMessage.Text = $"Error detecting paths: {ex.Message}";
        }
    }

    private string DetectSteamPath()
    {
        var steamPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Steam");
        return Directory.Exists(steamPath) ? steamPath : "Not found";
    }

    private string DetectEpicPath()
    {
        var epicPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Epic Games");
        return Directory.Exists(epicPath) ? epicPath : "Not found";
    }

    private string DetectXboxPath()
    {
        var xboxPath = Path.Combine("C:\\", "XboxGames");
        return Directory.Exists(xboxPath) ? xboxPath : "Not found";
    }

    private string DetectGOGPath()
    {
        var gogPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "GOG Galaxy", "Games");
        return Directory.Exists(gogPath) ? gogPath : "Not found";
    }

    private string DetectEAPath()
    {
        var eaPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "EA Games");
        return Directory.Exists(eaPath) ? eaPath : "Not found";
    }

    private string DetectUbisoftPath()
    {
        var ubisoftPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Ubisoft", "Ubisoft Game Launcher", "games");
        return Directory.Exists(ubisoftPath) ? ubisoftPath : "Not found";
    }

    private async Task LoadSettings()
    {
        try
        {
            StatusMessage.Text = "Loading application settings...";
            var settings = await _settingsService.LoadSettingsAsync();

            // Primera carga: solo forzar Deep Scan y Auto Scan a false si es primera vez
            bool firstTime = settings.LastUpdated == default || settings.Version < 2;
            if (firstTime)
            {
                // Solo Deep Scan y Auto Scan deben estar desactivados por defecto
                settings.EnableDeepScan = false;
                settings.AutoScanOnStartup = false;
                
                // Las plataformas deben estar activadas por defecto (mantener los defaults de GlobalSettings)
                // Solo guardar si realmente es primera vez
                await _settingsService.SaveSettingsAsync(settings);
            }

            // Cargar valores actuales (respetando los defaults de GlobalSettings.cs)
            SteamToggle.IsOn = settings.ScanSteam;
            EpicToggle.IsOn = settings.ScanEpic;
            XboxToggle.IsOn = settings.ScanXbox;
            GOGToggle.IsOn = settings.ScanGOG;
            EAToggle.IsOn = settings.ScanEA;
            UbisoftToggle.IsOn = settings.ScanUbisoft;
            DeepScanToggle.IsOn = settings.EnableDeepScan;
            AutoScanToggle.IsOn = settings.AutoScanOnStartup;
            StartWithWindowsToggle.IsOn = settings.StartWithWindows;
            StartMinimizedToggle.IsOn = settings.StartMinimized;
            NotificationsToggle.IsOn = settings.EnableNotifications;

            _customPaths.Clear();
            foreach (var path in settings.CustomGamePaths) _customPaths.Add(path);

            StatusMessage.Text = "Application settings loaded";
        }
        catch (Exception ex)
        {
            StatusMessage.Text = $"Error loading settings: {ex.Message}";
        }
    }

    private async Task SaveSettingsAutomatically()
    {
        try
        {
            var settings = await _settingsService.LoadSettingsAsync();
            settings.ScanSteam = SteamToggle.IsOn;
            settings.ScanEpic = EpicToggle.IsOn;
            settings.ScanXbox = XboxToggle.IsOn;
            settings.ScanGOG = GOGToggle.IsOn;
            settings.ScanEA = EAToggle.IsOn;
            settings.ScanUbisoft = UbisoftToggle.IsOn;
            settings.EnableDeepScan = DeepScanToggle.IsOn;
            settings.AutoScanOnStartup = AutoScanToggle.IsOn;
            settings.StartWithWindows = StartWithWindowsToggle.IsOn;
            settings.StartMinimized = StartMinimizedToggle.IsOn;
            settings.EnableNotifications = NotificationsToggle.IsOn;
            settings.CustomGamePaths.Clear();
            foreach (var p in _customPaths) settings.CustomGamePaths.Add(p);
            settings.LastUpdated = DateTime.Now;

            await _settingsService.SaveSettingsAsync(settings);
            var original = StatusMessage.Text;
            StatusMessage.Text = "Settings auto-saved";
            StatusMessage.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.LightGreen);
            await Task.Delay(1500);
            StatusMessage.Text = original;
            StatusMessage.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.LightGray);
            
            // Refresh platform paths display
            await DetectPlatformPaths();
        }
        catch (Exception ex)
        {
            StatusMessage.Text = $"Auto-save failed: {ex.Message}";
            StatusMessage.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Orange);
        }
    }

    private void OpenDataFolder_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var dataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OptiScaler Manager");
            Directory.CreateDirectory(dataFolder);
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = dataFolder, UseShellExecute = true });
        }
        catch (Exception ex) { StatusMessage.Text = $"Error opening data folder: {ex.Message}"; }
    }

    private async void BrowseCustomPath_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var picker = new Windows.Storage.Pickers.FolderPicker { SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.ComputerFolder };
            picker.FileTypeFilter.Add("*");
            var app = Application.Current as App; var win = app?.m_window;
            if (win != null)
            {
                var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(win);
                WinRT.Interop.InitializeWithWindow.Initialize(picker, hWnd);
            }
            var folder = await picker.PickSingleFolderAsync();
            if (folder != null) CustomPathTextBox.Text = folder.Path;
        }
        catch (Exception ex) { StatusMessage.Text = $"Error browsing: {ex.Message}"; }
    }

    private async void AddCustomPath_Click(object sender, RoutedEventArgs e)
    {
        var path = CustomPathTextBox.Text?.Trim();
        if (string.IsNullOrEmpty(path)) 
        { 
            StatusMessage.Text = "Please enter a folder path first";
            StatusMessage.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Orange);
            return; 
        }
        
        if (!Directory.Exists(path)) 
        { 
            StatusMessage.Text = "Folder does not exist";
            StatusMessage.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Orange);
            return; 
        }
        
        if (_customPaths.Contains(path)) 
        { 
            StatusMessage.Text = "This folder is already in the list";
            StatusMessage.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Orange);
            return; 
        }
        
        _customPaths.Add(path);
        CustomPathTextBox.Text = string.Empty;
        await SaveSettingsAutomatically();
        StatusMessage.Text = $"Folder added: {path}";
        StatusMessage.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.LightGreen);
        await Task.Delay(1500);
        StatusMessage.Text = "Settings are saved automatically";
        StatusMessage.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.LightGray);
    }

    private async void RemoveCustomPath_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button b && b.Tag is string p)
        {
            _customPaths.Remove(p);
            await SaveSettingsAutomatically();
            StatusMessage.Text = $"Custom path removed: {p}";
        }
    }

    private async void ResetSettings_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var dialog = new ContentDialog
            {
                Title = "Reset Settings",
                Content = "Reset all settings to defaults?",
                PrimaryButtonText = "Reset",
                SecondaryButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Secondary,
                XamlRoot = this.XamlRoot
            };
            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var defaults = new OptiScaler.Core.Models.GlobalSettings();
                // override defaults to all disabled
                defaults.ScanSteam = false; defaults.ScanEpic = false; defaults.ScanXbox = false; defaults.ScanGOG = false; defaults.ScanEA = false; defaults.ScanUbisoft = false;
                defaults.EnableDeepScan = false; defaults.AutoScanOnStartup = false; defaults.StartWithWindows = false; defaults.StartMinimized = false; defaults.EnableNotifications = false;
                await _settingsService.SaveSettingsAsync(defaults);
                await LoadSettings();
                StatusMessage.Text = "Settings reset";
            }
        }
        catch (Exception ex) { StatusMessage.Text = $"Reset error: {ex.Message}"; }
    }

    private async void ClearCache_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var dialog = new ContentDialog
            {
                Title = "Clear Cache",
                Content = "Delete all cached data (mods, backups, logs, settings cache)? This will NOT delete game mods installed in game folders.",
                PrimaryButtonText = "Clear",
                SecondaryButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Secondary,
                XamlRoot = this.XamlRoot
            };
            var result = await dialog.ShowAsync();
            if (result != ContentDialogResult.Primary) return;

            var dataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OptiScaler Manager");
            if (Directory.Exists(dataFolder))
            {
                foreach (var file in Directory.GetFiles(dataFolder, "*", SearchOption.AllDirectories))
                {
                    try { File.Delete(file); } catch { }
                }
                foreach (var dir in Directory.GetDirectories(dataFolder, "*", SearchOption.AllDirectories))
                {
                    try { Directory.Delete(dir, true); } catch { }
                }
            }
            Directory.CreateDirectory(dataFolder); // recreate root
            StatusMessage.Text = "Cache cleared";
        }
        catch (Exception ex) { StatusMessage.Text = $"Clear cache error: {ex.Message}"; }
    }

    private async void ExportSettings_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var picker = new Windows.Storage.Pickers.FileSavePicker
            {
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary,
                SuggestedFileName = $"OptiScaler_Settings_{DateTime.Now:yyyy-MM-dd}"
            };
            picker.FileTypeChoices.Add("JSON Settings", new[] { ".json" });
            var app = Application.Current as App; var win = app?.m_window; if (win != null) { var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(win); WinRT.Interop.InitializeWithWindow.Initialize(picker, hWnd); }
            var file = await picker.PickSaveFileAsync();
            if (file != null)
            {
                var settings = await _settingsService.LoadSettingsAsync();
                var json = System.Text.Json.JsonSerializer.Serialize(settings, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                await Windows.Storage.FileIO.WriteTextAsync(file, json);
                StatusMessage.Text = $"Settings exported: {file.Path}";
            }
        }
        catch (Exception ex) { StatusMessage.Text = $"Export error: {ex.Message}"; }
    }

    private async void ChangeSteamPath_Click(object sender, RoutedEventArgs e) => await ChangePlatformPath("Steam", "SteamLibraryPath", this.FindName("SteamPathText") as TextBlock);
    private async void ChangeEpicPath_Click(object sender, RoutedEventArgs e) => await ChangePlatformPath("Epic Games", "EpicLibraryPath", this.FindName("EpicPathText") as TextBlock);
    private async void ChangeXboxPath_Click(object sender, RoutedEventArgs e) => await ChangePlatformPath("Xbox", "XboxLibraryPath", this.FindName("XboxPathText") as TextBlock);
    private async void ChangeGOGPath_Click(object sender, RoutedEventArgs e) => await ChangePlatformPath("GOG", "GOGLibraryPath", this.FindName("GOGPathText") as TextBlock);
    private async void ChangeEAPath_Click(object sender, RoutedEventArgs e) => await ChangePlatformPath("EA", "EALibraryPath", this.FindName("EAPathText") as TextBlock);
    private async void ChangeUbisoftPath_Click(object sender, RoutedEventArgs e) => await ChangePlatformPath("Ubisoft", "UbisoftLibraryPath", this.FindName("UbisoftPathText") as TextBlock);

    private async Task ChangePlatformPath(string platformName, string settingPropertyName, TextBlock? pathTextBlock)
    {
        if (pathTextBlock == null) return;
        
        try
        {
            var picker = new Windows.Storage.Pickers.FolderPicker 
            { 
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.ComputerFolder,
                ViewMode = Windows.Storage.Pickers.PickerViewMode.List
            };
            picker.FileTypeFilter.Add("*");
            
            var app = Application.Current as App;
            var win = app?.m_window;
            if (win != null)
            {
                var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(win);
                WinRT.Interop.InitializeWithWindow.Initialize(picker, hWnd);
            }
            
            var folder = await picker.PickSingleFolderAsync();
            if (folder != null)
            {
                var settings = await _settingsService.LoadSettingsAsync();
                
                // Update the appropriate property using reflection
                var property = typeof(OptiScaler.Core.Models.GlobalSettings).GetProperty(settingPropertyName);
                if (property != null)
                {
                    property.SetValue(settings, folder.Path);
                    await _settingsService.SaveSettingsAsync(settings);
                    pathTextBlock.Text = $"Path: {folder.Path}";
                    StatusMessage.Text = $"{platformName} path updated";
                    StatusMessage.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.LightGreen);
                    await Task.Delay(1500);
                    StatusMessage.Text = "Settings are saved automatically";
                    StatusMessage.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.LightGray);
                }
            }
        }
        catch (Exception ex)
        {
            StatusMessage.Text = $"Error changing {platformName} path: {ex.Message}";
            StatusMessage.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Orange);
        }
    }
}