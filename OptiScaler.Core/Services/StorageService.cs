using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using OptiScaler.Core.Models;

namespace OptiScaler.Core.Services;

/// <summary>
/// Service for persisting application data to local storage
/// </summary>
public class StorageService
{
    private readonly string _storageRoot;

    public StorageService()
    {
        _storageRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "OptiScaler Manager");
        Directory.CreateDirectory(_storageRoot);
    }

    /// <summary>
    /// Save scanned games to disk
    /// </summary>
    public async Task SaveGamesAsync(IEnumerable<GameInfo> games)
    {
        try
        {
            var filePath = Path.Combine(_storageRoot, "scanned_games.json");
            var json = JsonSerializer.Serialize(games, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(filePath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving games: {ex.Message}");
        }
    }

    /// <summary>
    /// Load scanned games from disk
    /// </summary>
    public async Task<List<GameInfo>> LoadGamesAsync()
    {
        try
        {
            var filePath = Path.Combine(_storageRoot, "scanned_games.json");
            if (!File.Exists(filePath))
                return new List<GameInfo>();

            var json = await File.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<List<GameInfo>>(json) ?? new List<GameInfo>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading games: {ex.Message}");
            return new List<GameInfo>();
        }
    }

    /// <summary>
    /// Save downloaded releases metadata
    /// </summary>
    public async Task SaveDownloadedReleasesAsync(Dictionary<string, List<string>> downloadedFiles)
    {
        try
        {
            var filePath = Path.Combine(_storageRoot, "downloaded_releases.json");
            var json = JsonSerializer.Serialize(downloadedFiles, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(filePath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving releases: {ex.Message}");
        }
    }

    /// <summary>
    /// Load downloaded releases metadata
    /// </summary>
    public async Task<Dictionary<string, List<string>>> LoadDownloadedReleasesAsync()
    {
        try
        {
            var filePath = Path.Combine(_storageRoot, "downloaded_releases.json");
            if (!File.Exists(filePath))
                return new Dictionary<string, List<string>>();

            var json = await File.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json) ?? new Dictionary<string, List<string>>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading releases: {ex.Message}");
            return new Dictionary<string, List<string>>();
        }
    }

    /// <summary>
    /// Save downloaded releases metadata
    /// </summary>
    public async Task SaveReleasesAsync(List<GitHubRelease> optiScalerReleases, List<GitHubRelease> optiPatcherReleases)
    {
        try
        {
            var data = new
            {
                OptiScaler = optiScalerReleases,
                OptiPatcher = optiPatcherReleases,
                LastUpdated = DateTime.UtcNow
            };
            
            var filePath = Path.Combine(_storageRoot, "releases_cache.json");
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(filePath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving releases: {ex.Message}");
        }
    }

    /// <summary>
    /// Load cached releases
    /// </summary>
    public async Task<(List<GitHubRelease> OptiScaler, List<GitHubRelease> OptiPatcher)?> LoadReleasesAsync()
    {
        try
        {
            var filePath = Path.Combine(_storageRoot, "releases_cache.json");
            if (!File.Exists(filePath))
                return null;

            var json = await File.ReadAllTextAsync(filePath);
            var data = JsonSerializer.Deserialize<ReleasesCache>(json);
            
            if (data == null)
                return null;

            // Only use cache if it's less than 24 hours old
            if ((DateTime.UtcNow - data.LastUpdated).TotalHours < 24)
            {
                return (data.OptiScaler ?? new List<GitHubRelease>(), 
                        data.OptiPatcher ?? new List<GitHubRelease>());
            }
            
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading releases: {ex.Message}");
            return null;
        }
    }

    private class ReleasesCache
    {
        public List<GitHubRelease>? OptiScaler { get; set; }
        public List<GitHubRelease>? OptiPatcher { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
