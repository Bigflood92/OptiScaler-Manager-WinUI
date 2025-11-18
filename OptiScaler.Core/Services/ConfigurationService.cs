using System.Diagnostics;
using System.Text.Json;
using OptiScaler.Core.Contracts;
using OptiScaler.Core.Models;

namespace OptiScaler.Core.Services;

/// <summary>
/// Service for managing application configuration and settings
/// </summary>
public class ConfigurationService : IConfigurationService
{
    private readonly string _configFilePath;
    private readonly JsonSerializerOptions _jsonOptions;
    private AppConfiguration _currentConfiguration;

    public event EventHandler<ConfigurationChangedEventArgs>? ConfigurationChanged;

    public ConfigurationService()
    {
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "OptiScaler Manager");

        Directory.CreateDirectory(appDataPath);
        _configFilePath = Path.Combine(appDataPath, "config.json");

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        _currentConfiguration = LoadConfiguration();
    }

    /// <summary>
    /// Get current application configuration
    /// </summary>
    public AppConfiguration GetConfiguration()
    {
        return _currentConfiguration;
    }

    /// <summary>
    /// Save application configuration
    /// </summary>
    public async Task<bool> SaveConfigurationAsync(AppConfiguration configuration)
    {
        try
        {
            var json = JsonSerializer.Serialize(configuration, _jsonOptions);
            await File.WriteAllTextAsync(_configFilePath, json);
            
            _currentConfiguration = configuration;
            
            // Raise event for configuration change
            ConfigurationChanged?.Invoke(this, new ConfigurationChangedEventArgs
            {
                PropertyName = "Configuration",
                OldValue = _currentConfiguration,
                NewValue = configuration
            });

            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error saving configuration: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Reset configuration to defaults
    /// </summary>
    public async Task<AppConfiguration> ResetToDefaultsAsync()
    {
        var defaultConfig = new AppConfiguration();
        await SaveConfigurationAsync(defaultConfig);
        return defaultConfig;
    }

    /// <summary>
    /// Get a specific configuration value
    /// </summary>
    public T GetValue<T>(string key, T defaultValue)
    {
        try
        {
            var property = typeof(AppConfiguration).GetProperty(key);
            if (property != null)
            {
                var value = property.GetValue(_currentConfiguration);
                if (value is T typedValue)
                    return typedValue;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error getting configuration value: {ex.Message}");
        }

        return defaultValue;
    }

    /// <summary>
    /// Set a specific configuration value
    /// </summary>
    public async Task<bool> SetValueAsync<T>(string key, T value)
    {
        try
        {
            var property = typeof(AppConfiguration).GetProperty(key);
            if (property != null && property.CanWrite)
            {
                var oldValue = property.GetValue(_currentConfiguration);
                property.SetValue(_currentConfiguration, value);
                
                await SaveConfigurationAsync(_currentConfiguration);
                
                ConfigurationChanged?.Invoke(this, new ConfigurationChangedEventArgs
                {
                    PropertyName = key,
                    OldValue = oldValue,
                    NewValue = value
                });

                return true;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error setting configuration value: {ex.Message}");
        }

        return false;
    }

    /// <summary>
    /// Import configuration from file
    /// </summary>
    public async Task<AppConfiguration?> ImportConfigurationAsync(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                return null;

            var json = await File.ReadAllTextAsync(filePath);
            var configuration = JsonSerializer.Deserialize<AppConfiguration>(json, _jsonOptions);
            
            if (configuration != null)
            {
                await SaveConfigurationAsync(configuration);
            }

            return configuration;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error importing configuration: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Export configuration to file
    /// </summary>
    public async Task<bool> ExportConfigurationAsync(string filePath)
    {
        try
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            var json = JsonSerializer.Serialize(_currentConfiguration, _jsonOptions);
            await File.WriteAllTextAsync(filePath, json);
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error exporting configuration: {ex.Message}");
            return false;
        }
    }

    private AppConfiguration LoadConfiguration()
    {
        try
        {
            if (File.Exists(_configFilePath))
            {
                var json = File.ReadAllText(_configFilePath);
                var config = JsonSerializer.Deserialize<AppConfiguration>(json, _jsonOptions);
                if (config != null)
                    return config;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading configuration: {ex.Message}");
        }

        // Return default configuration if loading fails
        return new AppConfiguration();
    }
}
