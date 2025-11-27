# 📝 Changelog - OptiScaler Manager

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [Unreleased]

### Planned for v0.1.0
- WinUI 3 user interface implementation
- XAML views and ViewModels
- Data binding and commands
- Visual design following Windows 11 guidelines

### Planned for v0.2.0
- Unit testing suite
- Integration tests
- Performance optimization
- Error handling improvements

### Planned for v0.3.0
- Xbox Game Bar integration
- Overlay functionality (Win+G)
- Quick toggle interface

### Planned for v1.0.0
- MSIX packaging
- Microsoft Store submission
- Public release

---

## [0.0.1] - 2024-11-18

### 🎉 Initial Release - Core Services Implementation

This is the foundational release of OptiScaler Manager, marking the complete rewrite from Python to .NET 8. The focus of this version is establishing the core architecture and business logic layer.

### ✨ Added

#### **Project Structure**
- Created .NET 8 solution with two projects:
  - `OptiScaler.Core` - Business logic and services
  - `OptiScaler.UI` - User interface (WinForms placeholder)
- Configured VS Code workspace with purple theme
- Set up build tasks for Visual Studio Code
- Added comprehensive documentation structure

#### **Core Services**

**GameScannerService** - Multi-platform game detection
- ✅ Steam library scanning via registry and libraryfolders.vdf parsing
- ✅ Epic Games Store support via manifest file parsing
- ✅ Xbox Game Pass / Microsoft Store game detection
- ✅ GOG Galaxy integration via Windows Registry
- ✅ EA App / Origin games folder scanning
- ✅ Ubisoft Connect games support
- ✅ Manual game path verification
- ✅ Real-time progress reporting via events
- ✅ Mod detection (OptiScaler and DLSSG-to-FSR3)
- ✅ Platform-specific Windows API annotations

**GitHubService** - GitHub API integration
- ✅ Fetch latest releases from repositories
- ✅ List all releases with prerelease filtering
- ✅ Download release assets with progress tracking
- ✅ Speed calculation and formatted display
- ✅ Automatic retry logic
- ✅ Default repository mappings for known mods
- ✅ JSON deserialization of GitHub API responses

**ModInstallerService** - Mod management
- ✅ Install mods from ZIP archives
- ✅ Automatic backup creation before installation
- ✅ Uninstall with backup restoration
- ✅ Update existing installations
- ✅ Mod integrity verification
- ✅ Progress reporting for all operations
- ✅ Multi-step installation workflow
- ✅ Temporary file cleanup

**ConfigurationService** - Settings management
- ✅ JSON-based configuration storage
- ✅ Type-safe value access
- ✅ Configuration change events
- ✅ Import/Export functionality
- ✅ Reset to defaults
- ✅ Persistent storage in LocalAppData

#### **Data Models**

- `GameInfo` - Game installation information with INotifyPropertyChanged
- `ModInfo` - Installed mod metadata and version tracking
- `GitHubRelease` - GitHub release information with JSON mapping
- `GitHubAsset` - Downloadable asset details
- `ModOperationResult` - Operation result with success/error tracking
- `AppConfiguration` - User preferences and settings

#### **Enumerations**
- `GamePlatform` - Steam, Epic, Xbox, GOG, EA, Ubisoft, Manual
- `ModType` - OptiScaler, DlssgToFsr3, Custom
- `ModOperation` - Install, Update, Uninstall, Verify
- `AppTheme` - System, Light, Dark, Purple

#### **Event Arguments**
- `ScanProgressEventArgs` - Game scanning progress
- `DownloadProgressEventArgs` - Download progress with speed
- `InstallProgressEventArgs` - Installation step tracking
- `ConfigurationChangedEventArgs` - Configuration change notifications

#### **Documentation**

Created comprehensive documentation:
- 📄 `README.md` - Project overview and quick start
- 📄 `docs/ARCHITECTURE.md` - Detailed architecture documentation
- 📄 `docs/USAGE_EXAMPLES.md` - Code examples and workflows
- 📄 `docs/MSIX_SETUP.md` - MSIX packaging guide
- 📄 `docs/STORE_PUBLISHING.md` - Microsoft Store submission guide
- 📄 `LICENSE` - MIT License
- 📄 `CHANGELOG.md` - This file

### 🏗️ Technical Details

#### **Technologies Used**
- .NET 8 LTS
- C# 12
- System.Text.Json for JSON handling
- Windows Registry API (with platform annotations)
- Async/await patterns throughout
- INotifyPropertyChanged for data binding support

#### **Design Patterns**
- Service-oriented architecture
- Dependency injection ready (interfaces defined)
- MVVM pattern support
- Event-driven progress reporting
- Repository pattern for GitHub access

#### **Platform Support**
- Windows 10 version 1903 or later
- Windows 11 (all versions)
- x64 architecture
- Windows-only APIs properly annotated with `[SupportedOSPlatform("windows")]`

#### **Performance Optimizations**
- Asynchronous I/O operations
- Streaming downloads (no full memory load)
- Cancellation token support
- Temporary file cleanup
- Efficient file enumeration

### 🔧 Code Quality

#### **Error Handling**
- Try-catch blocks in all service methods
- Debug.WriteLine for diagnostic logging
- Graceful fallbacks on errors
- User-friendly error messages in results

#### **Code Organization**
- Clear separation of concerns
- Interface-based contracts
- XML documentation comments
- Consistent naming conventions
- SOLID principles adherence

### 📊 Project Statistics

```
Total Files Created: 17
Lines of Code: ~2,500
Services Implemented: 4
Models Created: 6
Interfaces Defined: 4
Documentation Pages: 5
```

### 🎯 Completeness

**v0.0.1 Goals: 100% Complete** ✅

- [x] Project structure setup
- [x] Core service implementations
- [x] Data models and contracts
- [x] Event-driven architecture
- [x] Comprehensive documentation
- [x] Successful compilation without errors
- [x] Platform-specific API handling

### 🚀 Next Steps (v0.1.0)

The next version will focus on the user interface:

1. **WinUI 3 Migration**
   - Convert from WinForms to WinUI 3
   - Implement XAML views
   - Create ViewModels with CommunityToolkit.Mvvm
   - Design modern UI following Fluent Design

2. **Data Binding**
   - Connect services to UI
   - Implement command pattern
   - Real-time progress updates
   - Responsive UI design

3. **User Experience**
   - Game list with filtering/sorting
   - Mod installation wizard
   - Settings page
   - Notifications

### 🐛 Known Issues

None at this time. This is the initial core implementation.

### 📋 Breaking Changes

N/A - This is the first release.

---

## Version Comparison

### OptiScaler (Python) vs OptiScaler Manager (.NET)

| Feature | Python v2.4.x | .NET v0.0.1 |
|---------|--------------|-------------|
| Runtime | Python + Nuitka | Native .NET 8 |
| UI | CustomTkinter | WinUI 3 (planned) |
| Distribution | GitHub Releases | Microsoft Store |
| Platform Detection | ✅ | ✅ |
| Mod Installation | ✅ | ✅ |
| Auto-Updates | ✅ | ⏳ Planned |
| Xbox Game Bar | ❌ | ⏳ Planned |
| MSIX Package | ❌ | ⏳ Planned |

---

## Contributors

- **Jorge Coronas (Bigflood92)** - Project creator and lead developer

---

## Links

- [Project Repository](https://github.com/bigflood92/OptiScaler-Manager) (Private - Development)
- [Microsoft Store Listing](https://www.microsoft.com/store) (Coming soon)
- [Documentation](./docs/README.md)

---

**[0.0.1]:** 2024-11-18 - Initial core implementation
