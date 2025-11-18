# 💡 OptiScaler Manager - Service Usage Examples

> **Practical examples for implementing features using the core services**

## 📋 Table of Contents

- [GameScannerService Examples](#gamescannerservice-examples)
- [GitHubService Examples](#githubservice-examples)
- [ModInstallerService Examples](#modinstallerservice-examples)
- [ConfigurationService Examples](#configurationservice-examples)
- [Complete Workflows](#complete-workflows)

---

## 🔍 GameScannerService Examples

### **Example 1: Scan All Platforms**

```csharp
using OptiScaler.Core.Services;
using OptiScaler.Core.Models;

public class GameScanningViewModel
{
    private readonly GameScannerService _scanner;
    private ObservableCollection<GameInfo> _games;

    public GameScanningViewModel()
    {
        _scanner = new GameScannerService();
        _games = new ObservableCollection<GameInfo>();
        
        // Subscribe to events
        _scanner.GameDiscovered += OnGameDiscovered;
        _scanner.ScanProgress += OnScanProgress;
    }

    public async Task ScanForGamesAsync()
    {
        _games.Clear();
        
        var cancellationTokenSource = new CancellationTokenSource();
        var games = await _scanner.ScanAllPlatformsAsync(cancellationTokenSource.Token);
        
        foreach (var game in games)
        {
            _games.Add(game);
        }
    }

    private void OnGameDiscovered(object? sender, GameInfo game)
    {
        // Update UI in real-time as games are found
        Application.Current.Dispatcher.Dispatch(() =>
        {
            _games.Add(game);
        });
    }

    private void OnScanProgress(object? sender, ScanProgressEventArgs e)
    {
        // Update progress bar
        StatusMessage = e.StatusMessage;
        ProgressPercentage = e.ProgressPercentage;
    }
}
```

### **Example 2: Scan Specific Platform**

```csharp
public async Task ScanSteamOnlyAsync()
{
    var scanner = new GameScannerService();
    var steamGames = await scanner.ScanPlatformAsync(GamePlatform.Steam);
    
    Console.WriteLine($"Found {steamGames.Count()} Steam games");
    
    foreach (var game in steamGames)
    {
        Console.WriteLine($"- {game.Name}");
        Console.WriteLine($"  Path: {game.Path}");
        Console.WriteLine($"  Has OptiScaler: {game.HasOptiScaler}");
    }
}
```

### **Example 3: Manual Game Path Verification**

```csharp
public async Task<bool> AddCustomGameAsync(string gamePath)
{
    var scanner = new GameScannerService();
    var gameInfo = await scanner.VerifyGamePathAsync(gamePath);
    
    if (gameInfo != null)
    {
        // Valid game path
        _games.Add(gameInfo);
        await SaveCustomGamePathAsync(gamePath);
        return true;
    }
    
    return false;
}
```

### **Example 4: Refresh Mod Status**

```csharp
public async Task RefreshGameModStatusAsync(GameInfo game)
{
    var scanner = new GameScannerService();
    var updatedGame = await scanner.RefreshModStatusAsync(game);
    
    // Update UI
    game.HasOptiScaler = updatedGame.HasOptiScaler;
    game.HasDlssgToFsr3 = updatedGame.HasDlssgToFsr3;
    game.LastScanned = updatedGame.LastScanned;
}
```

---

## 📥 GitHubService Examples

### **Example 1: Get Latest Release**

```csharp
using OptiScaler.Core.Services;

public class UpdateCheckViewModel
{
    private readonly GitHubService _github;

    public UpdateCheckViewModel()
    {
        _github = new GitHubService();
    }

    public async Task CheckForOptiScalerUpdateAsync()
    {
        var release = await _github.GetLatestReleaseAsync("cdozdil", "OptiScaler");
        
        if (release != null)
        {
            var version = release.Version;
            var releaseNotes = release.Body;
            var publishDate = release.PublishedAt;
            
            Console.WriteLine($"Latest Version: {version}");
            Console.WriteLine($"Published: {publishDate:yyyy-MM-dd}");
            Console.WriteLine($"Assets: {release.Assets.Count}");
        }
    }
}
```

### **Example 2: Download with Progress**

```csharp
public async Task DownloadModWithProgressAsync()
{
    var github = new GitHubService();
    
    // Subscribe to download progress
    github.DownloadProgress += (sender, e) =>
    {
        var percentComplete = e.ProgressPercentage;
        var speed = e.FormattedSpeed;
        var fileName = e.FileName;
        
        Console.WriteLine($"Downloading {fileName}: {percentComplete:F1}% @ {speed}");
    };
    
    // Download latest OptiScaler
    var tempPath = Path.Combine(Path.GetTempPath(), "OptiScaler.zip");
    var result = await github.DownloadLatestModAsync(ModType.OptiScaler, tempPath);
    
    if (result.HasValue)
    {
        Console.WriteLine($"Downloaded to: {result.Value.FilePath}");
        Console.WriteLine($"Release: {result.Value.Release.Name}");
    }
}
```

### **Example 3: List All Releases**

```csharp
public async Task ShowReleaseHistoryAsync()
{
    var github = new GitHubService();
    var releases = await github.GetReleasesAsync("cdozdil", "OptiScaler", includePrerelease: false);
    
    Console.WriteLine("OptiScaler Release History:");
    foreach (var release in releases.Take(5))
    {
        Console.WriteLine($"\n{release.Name} ({release.TagName})");
        Console.WriteLine($"Published: {release.PublishedAt:yyyy-MM-dd}");
        Console.WriteLine($"Downloads: {release.Assets.Sum(a => a.DownloadCount)}");
    }
}
```

---

## 📦 ModInstallerService Examples

### **Example 1: Install Mod**

```csharp
using OptiScaler.Core.Services;
using OptiScaler.Core.Contracts;

public class ModInstallationViewModel
{
    private readonly IModInstallerService _installer;
    private readonly IGitHubService _github;

    public ModInstallationViewModel()
    {
        _github = new GitHubService();
        _installer = new ModInstallerService(_github);
        
        // Subscribe to installation progress
        _installer.InstallProgress += OnInstallProgress;
    }

    public async Task<bool> InstallOptiScalerAsync(GameInfo game)
    {
        try
        {
            // Download latest version
            var tempFile = Path.Combine(Path.GetTempPath(), "OptiScaler.zip");
            var download = await _github.DownloadLatestModAsync(ModType.OptiScaler, tempFile);
            
            if (!download.HasValue)
            {
                ShowError("Failed to download OptiScaler");
                return false;
            }
            
            // Install to game
            var result = await _installer.InstallModAsync(
                game, 
                ModType.OptiScaler, 
                download.Value.FilePath);
            
            if (result.Success)
            {
                ShowSuccess($"OptiScaler installed to {game.Name}");
                return true;
            }
            else
            {
                ShowError($"Installation failed: {result.ErrorMessage}");
                return false;
            }
        }
        finally
        {
            // Cleanup temp file
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    private void OnInstallProgress(object? sender, InstallProgressEventArgs e)
    {
        StatusText = e.CurrentAction;
        ProgressValue = e.ProgressPercentage;
    }
}
```

### **Example 2: Uninstall Mod**

```csharp
public async Task<bool> UninstallModAsync(GameInfo game, ModType modType)
{
    var confirmDialog = new ContentDialog
    {
        Title = "Confirm Uninstallation",
        Content = $"Remove {modType} from {game.Name}?",
        PrimaryButtonText = "Uninstall",
        CloseButtonText = "Cancel"
    };
    
    if (await confirmDialog.ShowAsync() != ContentDialogResult.Primary)
        return false;
    
    var result = await _installer.UninstallModAsync(game, modType);
    
    if (result.Success)
    {
        ShowSuccess($"{modType} removed successfully");
        await RefreshGameModStatusAsync(game);
        return true;
    }
    
    ShowError($"Uninstallation failed: {result.ErrorMessage}");
    return false;
}
```

### **Example 3: Update Mod**

```csharp
public async Task UpdateModIfAvailableAsync(GameInfo game, ModType modType)
{
    // Check if update is available
    var installedMods = await _installer.GetInstalledModsAsync(game);
    var mod = installedMods.FirstOrDefault(m => m.Type == modType);
    
    if (mod == null)
    {
        ShowError($"{modType} is not installed");
        return;
    }
    
    if (!mod.UpdateAvailable)
    {
        ShowInfo($"{modType} is already up to date (v{mod.InstalledVersion})");
        return;
    }
    
    // Update
    var result = await _installer.UpdateModAsync(game, modType);
    
    if (result.Success)
    {
        ShowSuccess($"Updated {modType} to v{mod.LatestVersion}");
    }
    else
    {
        ShowError($"Update failed: {result.ErrorMessage}");
    }
}
```

### **Example 4: Create and Restore Backup**

```csharp
public async Task SafeModInstallationAsync(GameInfo game, ModType modType)
{
    // Create backup first
    var backupPath = await _installer.CreateBackupAsync(game);
    Console.WriteLine($"Backup created at: {backupPath}");
    
    try
    {
        // Download and install
        var tempFile = Path.Combine(Path.GetTempPath(), $"{modType}.zip");
        var download = await _github.DownloadLatestModAsync(modType, tempFile);
        
        if (download.HasValue)
        {
            var result = await _installer.InstallModAsync(game, modType, download.Value.FilePath);
            
            if (!result.Success)
            {
                // Installation failed, restore backup
                Console.WriteLine("Installation failed, restoring backup...");
                await _installer.RestoreBackupAsync(game, backupPath);
            }
        }
    }
    catch (Exception ex)
    {
        // Error occurred, restore backup
        Console.WriteLine($"Error: {ex.Message}");
        await _installer.RestoreBackupAsync(game, backupPath);
    }
}
```

---

## ⚙️ ConfigurationService Examples

### **Example 1: Load and Save Settings**

```csharp
public class SettingsViewModel
{
    private readonly IConfigurationService _config;

    public SettingsViewModel()
    {
        _config = new ConfigurationService();
        LoadSettings();
    }

    private void LoadSettings()
    {
        var config = _config.GetConfiguration();
        
        CheckUpdatesOnStartup = config.CheckForUpdatesOnStartup;
        CreateBackups = config.CreateBackupsBeforeInstall;
        SelectedTheme = config.Theme;
        MaxBackups = config.MaxBackupsPerGame;
    }

    public async Task SaveSettingsAsync()
    {
        var config = _config.GetConfiguration();
        
        config.CheckForUpdatesOnStartup = CheckUpdatesOnStartup;
        config.CreateBackupsBeforeInstall = CreateBackups;
        config.Theme = SelectedTheme;
        config.MaxBackupsPerGame = MaxBackups;
        
        var success = await _config.SaveConfigurationAsync(config);
        
        if (success)
            ShowSuccess("Settings saved");
        else
            ShowError("Failed to save settings");
    }
}
```

### **Example 2: React to Configuration Changes**

```csharp
public class MainViewModel
{
    public MainViewModel()
    {
        var config = new ConfigurationService();
        
        // Subscribe to changes
        config.ConfigurationChanged += OnConfigurationChanged;
    }

    private void OnConfigurationChanged(object? sender, ConfigurationChangedEventArgs e)
    {
        Console.WriteLine($"Setting changed: {e.PropertyName}");
        Console.WriteLine($"Old value: {e.OldValue}");
        Console.WriteLine($"New value: {e.NewValue}");
        
        // Apply changes
        if (e.PropertyName == "Theme")
        {
            ApplyTheme((AppTheme)e.NewValue);
        }
    }
}
```

### **Example 3: Export/Import Settings**

```csharp
public async Task ExportSettingsAsync()
{
    var savePicker = new FileSavePicker
    {
        SuggestedFileName = "OptiScaler_Settings",
        FileTypeChoices = { { "JSON", new[] { ".json" } } }
    };
    
    var file = await savePicker.PickSaveFileAsync();
    if (file != null)
    {
        var success = await _config.ExportConfigurationAsync(file.Path);
        if (success)
            ShowSuccess("Settings exported");
    }
}

public async Task ImportSettingsAsync()
{
    var openPicker = new FileOpenPicker
    {
        FileTypeFilter = { ".json" }
    };
    
    var file = await openPicker.PickSingleFileAsync();
    if (file != null)
    {
        var config = await _config.ImportConfigurationAsync(file.Path);
        if (config != null)
        {
            ShowSuccess("Settings imported");
            LoadSettings(); // Refresh UI
        }
    }
}
```

---

## 🔄 Complete Workflows

### **Workflow 1: Scan → Select → Install**

```csharp
public class MainWorkflowViewModel
{
    private readonly GameScannerService _scanner;
    private readonly GitHubService _github;
    private readonly ModInstallerService _installer;
    
    public async Task CompleteInstallWorkflowAsync()
    {
        // Step 1: Scan for games
        var games = await _scanner.ScanAllPlatformsAsync();
        var gameList = games.ToList();
        
        if (!gameList.Any())
        {
            ShowError("No games found");
            return;
        }
        
        // Step 2: Let user select a game (simplified)
        var selectedGame = gameList.First();
        
        // Step 3: Check installed mods
        var installedMods = await _installer.GetInstalledModsAsync(selectedGame);
        
        if (installedMods.Any(m => m.Type == ModType.OptiScaler))
        {
            ShowInfo("OptiScaler already installed");
            return;
        }
        
        // Step 4: Download mod
        var tempFile = Path.Combine(Path.GetTempPath(), "OptiScaler.zip");
        var download = await _github.DownloadLatestModAsync(ModType.OptiScaler, tempFile);
        
        if (!download.HasValue)
        {
            ShowError("Download failed");
            return;
        }
        
        // Step 5: Install
        var result = await _installer.InstallModAsync(
            selectedGame, 
            ModType.OptiScaler, 
            download.Value.FilePath);
        
        if (result.Success)
        {
            ShowSuccess("Installation complete!");
        }
    }
}
```

### **Workflow 2: Check for Updates → Update All**

```csharp
public async Task UpdateAllGamesAsync()
{
    var games = await _scanner.ScanAllPlatformsAsync();
    var updateTasks = new List<Task>();
    
    foreach (var game in games)
    {
        var mods = await _installer.GetInstalledModsAsync(game);
        
        foreach (var mod in mods.Where(m => m.UpdateAvailable))
        {
            Console.WriteLine($"Updating {mod.Name} for {game.Name}...");
            
            var updateTask = _installer.UpdateModAsync(game, mod.Type);
            updateTasks.Add(updateTask);
        }
    }
    
    await Task.WhenAll(updateTasks);
    ShowSuccess($"Updated {updateTasks.Count} mods");
}
```

### **Workflow 3: Scheduled Background Scan**

```csharp
public class BackgroundScanService
{
    private readonly PeriodicTimer _timer;
    private readonly GameScannerService _scanner;
    private readonly ConfigurationService _config;
    
    public BackgroundScanService()
    {
        _scanner = new GameScannerService();
        _config = new ConfigurationService();
        _timer = new PeriodicTimer(TimeSpan.FromHours(6));
    }
    
    public async Task StartAsync(CancellationToken stoppingToken)
    {
        while (await _timer.WaitForNextTickAsync(stoppingToken))
        {
            if (_config.GetConfiguration().CheckForUpdatesOnStartup)
            {
                await PerformBackgroundScanAsync();
            }
        }
    }
    
    private async Task PerformBackgroundScanAsync()
    {
        var games = await _scanner.ScanAllPlatformsAsync();
        var updatesAvailable = 0;
        
        foreach (var game in games)
        {
            var mods = await _installer.GetInstalledModsAsync(game);
            updatesAvailable += mods.Count(m => m.UpdateAvailable);
        }
        
        if (updatesAvailable > 0)
        {
            ShowNotification($"{updatesAvailable} mod update(s) available");
        }
    }
}
```

---

## 🎯 Best Practices

### **1. Always Use CancellationTokens**
```csharp
// ✅ Good
public async Task ScanGamesAsync(CancellationToken cancellationToken)
{
    var games = await _scanner.ScanAllPlatformsAsync(cancellationToken);
}

// ❌ Bad
public async Task ScanGamesAsync()
{
    var games = await _scanner.ScanAllPlatformsAsync(); // Can't cancel
}
```

### **2. Handle Errors Gracefully**
```csharp
try
{
    var result = await _installer.InstallModAsync(game, modType, archivePath);
    if (!result.Success)
    {
        LogError($"Installation failed: {result.ErrorMessage}");
        ShowUserFriendlyError("Could not install mod. Please try again.");
    }
}
catch (Exception ex)
{
    LogException(ex);
    ShowUserFriendlyError("An unexpected error occurred.");
}
```

### **3. Dispose Services Properly**
```csharp
public class MyViewModel : IDisposable
{
    private readonly GitHubService _github;
    
    public MyViewModel()
    {
        _github = new GitHubService();
    }
    
    public void Dispose()
    {
        _github?.Dispose();
    }
}
```

### **4. Update UI from Progress Events**
```csharp
_scanner.ScanProgress += (sender, e) =>
{
    // Ensure UI updates on UI thread
    DispatcherQueue.TryEnqueue(() =>
    {
        ProgressText = e.StatusMessage;
        ProgressValue = e.ProgressPercentage;
    });
};
```

---

**📅 Last Updated:** November 18, 2024  
**👨‍💻 Author:** Jorge Coronas (Bigflood92)  
**🔖 Version:** 0.0.1
