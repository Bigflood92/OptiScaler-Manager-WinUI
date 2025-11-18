// OptiScaler Manager - Core Library
// Version and application information

namespace OptiScaler.Core;

/// <summary>
/// Application version and metadata information
/// </summary>
public static class AppInfo
{
    /// <summary>
    /// Current application version
    /// </summary>
    public const string Version = "0.0.1";
    
    /// <summary>
    /// Application display name
    /// </summary>
    public const string AppName = "OptiScaler Manager";
    
    /// <summary>
    /// Full application title with version
    /// </summary>
    public static string FullTitle => $"{AppName} v{Version}";
    
    /// <summary>
    /// Application description
    /// </summary>
    public const string Description = "Modern Windows app for game optimization with FSR3 and DLSS mods";
    
    /// <summary>
    /// Copyright information
    /// </summary>
    public const string Copyright = "© 2024 OptiScaler Manager";
    
    /// <summary>
    /// Build target framework
    /// </summary>
    public const string Framework = ".NET 8";
    
    /// <summary>
    /// UI Framework used
    /// </summary>
    public const string UIFramework = "WinUI 3";
    
    /// <summary>
    /// Company/Developer name
    /// </summary>
    public const string Company = "Bigflood92";
    
    /// <summary>
    /// Product identifier
    /// </summary>
    public const string ProductId = "OptiScaler.Manager";
    
    /// <summary>
    /// GitHub repository URL
    /// </summary>
    public const string GitHubUrl = "https://github.com/Bigflood92/OptiScaler-Manager";
    
    /// <summary>
    /// Release notes for current version
    /// </summary>
    public const string ReleaseNotes = @"
OptiScaler Manager v0.0.1 - Initial Release

🚀 NEW PROJECT:
• Complete separation from Python version
• Independent repository and development
• Modern .NET 8 and WinUI 3 foundation
• Microsoft Store targeting

🏗️ ARCHITECTURE:
• Clean MVVM pattern implementation
• Modular service-based design
• Async/await for responsive UI
• Modern C# language features

📋 ROADMAP:
• v0.1.0: Core services and game scanning
• v0.2.0: Mod installation and GitHub integration
• v0.3.0: Xbox Game Bar overlay
• v1.0.0: Microsoft Store release
";
}