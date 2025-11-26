# OptiScaler Manager - Documentation

> Modern Windows application for managing DLSS, FSR, and XeSS upscaling mods for PC games

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/download)
[![WinUI](https://img.shields.io/badge/WinUI-3-0078D4)](https://microsoft.github.io/microsoft-ui-xaml/)

---

## ?? Documentation Site

**?? Visit our documentation:** [https://bigflood92.github.io/OptiScaler-Manager-WinUI/](https://bigflood92.github.io/OptiScaler-Manager-WinUI/)

---

## ?? About This Repository

This is the **public documentation repository** for OptiScaler Manager. The source code is maintained in a private repository.

### Available Documentation

- **[Architecture Overview](docs/ARCHITECTURE.md)** - Technical design and patterns
- **[Usage Examples](docs/USAGE_EXAMPLES.md)** - Implementation guides
- **[MSIX Setup](docs/MSIX_SETUP.md)** - Packaging guide
- **[Store Publishing](docs/STORE_PUBLISHING.md)** - Microsoft Store submission
- **[Privacy Policy](docs/PrivacyPolicy.html)** - Data privacy details

---

## ? What is OptiScaler Manager?

OptiScaler Manager is a Windows application that simplifies the installation and configuration of advanced upscaling mods for PC games:

### Key Features

- ?? **Multi-Platform Detection** - Automatically finds games from Steam, Epic, Xbox, GOG, EA, Ubisoft
- ?? **Upscaling Technologies** - DLSS, FSR 2.2/3.1, XeSS support
- ? **Frame Generation** - Enable DLSS 3 and FSR 3 frame generation
- ?? **Easy Configuration** - Quick presets and advanced settings
- ?? **Modern UI** - Xbox-style interface with gamepad support
- ?? **Privacy Focused** - No telemetry, all data stays local

---

## ?? Quick Start

### Installation

**From Microsoft Store** (Coming Soon)
- Search "OptiScaler Manager" in Microsoft Store
- One-click install and automatic updates

**From GitHub Releases**
1. Download the latest `.msix` package from [Releases](https://github.com/Bigflood92/OptiScaler-Manager-WinUI/releases)
2. Install the certificate (first time only)
3. Double-click the `.msix` file to install
4. Launch from Start Menu

### Basic Usage

1. **Scan Games** ? Detect installed games automatically
2. **Select Game** ? Choose from your library
3. **Install Mod** ? One-click installation
4. **Configure** ? Adjust upscaler and quality preset
5. **Play** ? Launch game with enhanced performance

---

## ??? Architecture

OptiScaler Manager is built with:

- **Framework:** .NET 8 (LTS)
- **UI:** WinUI 3 (Windows App SDK 1.8.3)
- **Pattern:** MVVM with CommunityToolkit.Mvvm
- **Language:** C# 12

### Project Structure

```
OptiScaler Manager
??? OptiScaler.Core          # Business logic layer
?   ??? Services             # Core services (scanning, installation)
?   ??? Models               # Data models
?   ??? Contracts            # Service interfaces
??? OptiScaler.UI            # Presentation layer
    ??? Views                # XAML pages
    ??? ViewModels           # View logic
    ??? Converters           # Data binding converters
```

See [Architecture Documentation](docs/ARCHITECTURE.md) for details.

---

## ?? System Requirements

### Minimum
- **OS:** Windows 10 version 1809 (build 17763) or later
- **RAM:** 4 GB
- **Storage:** 200 MB available space
- **Graphics:** DirectX 12 compatible GPU

### Recommended
- **OS:** Windows 11 22H2 or later
- **GPU:** NVIDIA RTX 20/30/40 series, AMD RX 5000/6000/7000, or Intel Arc
- **Storage:** SSD for faster mod installation

---

## ?? Privacy & Security

OptiScaler Manager is designed with privacy in mind:

- ? **No Data Collection** - Zero telemetry or analytics
- ? **No User Accounts** - Use completely offline
- ? **Local Storage Only** - Settings stay on your device
- ? **No Tracking** - No cookies, no third-party services
- ? **Open Configuration** - Human-readable INI files

Read our full [Privacy Policy](https://bigflood92.github.io/OptiScaler-Manager-WinUI/PrivacyPolicy.html).

---

## ?? Contributing

We welcome contributions! If you'd like to contribute:

1. **Report Issues** - Use [GitHub Issues](https://github.com/Bigflood92/OptiScaler-Manager-WinUI/issues) for bugs or feature requests
2. **Improve Documentation** - Submit PRs to improve this documentation
3. **Share Feedback** - Join [Discussions](https://github.com/Bigflood92/OptiScaler-Manager-WinUI/discussions)

For code contributions, please contact the maintainer for access to the private repository.

---

## ?? License

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.

### Third-Party Components

OptiScaler Manager uses the following open-source libraries:

- [OptiScaler](https://github.com/cdozdil/OptiScaler) - MIT License
- [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet) - MIT License
- [SharpCompress](https://github.com/adamhathcock/sharpcompress) - MIT License

See [THIRD_PARTY_NOTICES.txt](THIRD_PARTY_NOTICES.txt) for complete attribution.

---

## ?? Credits

- **OptiScaler Framework** - [cdozdil](https://github.com/cdozdil/OptiScaler)
- **Microsoft** - For .NET and WinUI 3
- **Community** - For feedback and support

Platform logos are trademarks of their respective owners.

---

## ?? Support

- ?? **Bug Reports:** [GitHub Issues](https://github.com/Bigflood92/OptiScaler-Manager-WinUI/issues)
- ?? **Discussions:** [GitHub Discussions](https://github.com/Bigflood92/OptiScaler-Manager-WinUI/discussions)
- ?? **Contact:** Open an issue for general inquiries

---

## ??? Roadmap

### Current Version: v0.1.0 (Beta)
- ? Multi-platform game detection
- ? OptiScaler mod installation
- ? Configuration presets
- ? Xbox-style UI

### Planned Features
- ?? **Automatic Updates** - Keep mods up to date
- ?? **Xbox Game Bar Widget** - Quick access overlay
- ?? **Benchmark Tools** - Before/after performance comparison
- ?? **Cloud Sync** - Sync settings across devices (optional)

---

## ? Star This Repo

If you find OptiScaler Manager useful, please consider giving it a star! It helps others discover the project.

---

**Made with ?? for the PC gaming community**

**Developer:** Bigflood92  
**Version:** 0.1.0 (Development)  
**Last Updated:** January 2025
