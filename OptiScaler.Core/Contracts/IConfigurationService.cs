using OptiScaler.Core.Models;

namespace OptiScaler.Core.Contracts;

/// <summary>
/// Service for managing application configuration and settings
/// </summary>
public interface IConfigurationService
{
    /// <summary>
    /// Event raised when configuration changes
    /// </summary>
    event EventHandler<ConfigurationChangedEventArgs>? ConfigurationChanged;

    /// <summary>
    /// Get current application configuration
    /// </summary>
    /// <returns>Current configuration</returns>
    AppConfiguration GetConfiguration();

    /// <summary>
    /// Save application configuration
    /// </summary>
    /// <param name="configuration">Configuration to save</param>
    /// <returns>True if save was successful</returns>
    Task<bool> SaveConfigurationAsync(AppConfiguration configuration);

    /// <summary>
    /// Reset configuration to defaults
    /// </summary>
    /// <returns>Default configuration</returns>
    Task<AppConfiguration> ResetToDefaultsAsync();

    /// <summary>
    /// Get a specific configuration value
    /// </summary>
    /// <typeparam name="T">Type of value</typeparam>
    /// <param name="key">Configuration key</param>
    /// <param name="defaultValue">Default value if key not found</param>
    /// <returns>Configuration value</returns>
    T GetValue<T>(string key, T defaultValue);

    /// <summary>
    /// Set a specific configuration value
    /// </summary>
    /// <typeparam name="T">Type of value</typeparam>
    /// <param name="key">Configuration key</param>
    /// <param name="value">Value to set</param>
    /// <returns>True if save was successful</returns>
    Task<bool> SetValueAsync<T>(string key, T value);

    /// <summary>
    /// Import configuration from file
    /// </summary>
    /// <param name="filePath">Path to configuration file</param>
    /// <returns>Imported configuration</returns>
    Task<AppConfiguration?> ImportConfigurationAsync(string filePath);

    /// <summary>
    /// Export configuration to file
    /// </summary>
    /// <param name="filePath">Path to save configuration</param>
    /// <returns>True if export was successful</returns>
    Task<bool> ExportConfigurationAsync(string filePath);
}

/// <summary>
/// Event arguments for configuration changes
/// </summary>
public class ConfigurationChangedEventArgs : EventArgs
{
    public string PropertyName { get; set; } = string.Empty;
    public object? OldValue { get; set; }
    public object? NewValue { get; set; }
}
