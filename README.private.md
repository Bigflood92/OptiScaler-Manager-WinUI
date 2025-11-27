# OptiScaler Manager - Private Repository

> **This is the PRIVATE development repository** containing the full source code of OptiScaler Manager.

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/download)
[![WinUI](https://img.shields.io/badge/WinUI-3-0078D4)](https://microsoft.github.io/microsoft-ui-xaml/)

---

## Repository Structure

This repository contains:
- **Full source code** of OptiScaler Manager (OptiScaler.UI and OptiScaler.Core projects)
- **Development documentation** (MSIX packaging guides, Partner Center guides, etc.)
- **Internal guides** for store submission and deployment
- **Project files** (.sln, .csproj)
- **Assets and screenshots**

## Related Repository

**Public Documentation Repository:**  
https://github.com/Bigflood92/OptiScaler-Manager-WinUI

The public repository contains only:
- GitHub Pages documentation
- README and LICENSE
- Public-facing documentation files

---

## Project Overview

OptiScaler Manager is a modern Windows application built with .NET 8 and WinUI 3 that simplifies the installation and configuration of upscaling mods (DLSS, FSR, XeSS) for PC games.

### Technology Stack

- **Framework:** .NET 8 (LTS)
- **UI:** WinUI 3 (Windows App SDK 1.8.3)
- **Architecture:** MVVM with CommunityToolkit.Mvvm
- **Language:** C# 12
- **Packaging:** MSIX for Microsoft Store

### Project Structure

```
OptiScaler Manager (Private)
|-- OptiScaler.Core/              # Business logic layer
|   |-- Services/                 # Core services
|   |-- Models/                   # Data models
|   |-- Contracts/                # Service interfaces
|-- OptiScaler.UI/                # Presentation layer (WinUI 3)
|   |-- Views/                    # XAML pages
|   |-- ViewModels/               # View logic (MVVM)
|   |-- Converters/               # Data binding converters
|   |-- Assets/                   # Images, icons, logos
|-- docs/                         # Documentation
|-- Screenshots/                  # App screenshots
|-- CHANGELOG.md                  # Version history
|-- MSIX_PACKAGING_GUIDE.md       # MSIX creation guide
|-- PARTNER_CENTER_*.md           # Store submission guides
|-- STORE_*.md                    # Store listing content
```

---

## Development Setup

### Prerequisites

- **Visual Studio 2022** (17.8 or later)
- **.NET 8 SDK** (8.0.100 or later)
- **Windows App SDK 1.8.3**
- **Windows 10 SDK** (10.0.19041.0 or later)

### Getting Started

1. **Clone the repository:**
   ```powershell
   git clone https://github.com/Bigflood92/OptiScaler-Manager-WinUI-Private.git
   cd OptiScaler-Manager-WinUI-Private
   ```

2. **Open the solution:**
   ```
   OptiScaler.sln (recommended - main solution)
   or
   OptiScaler.Manager.sln (alternative)
   ```

3. **Restore NuGet packages:**
   ```powershell
   dotnet restore
   ```

4. **Build the solution:**
   ```powershell
   dotnet build
   ```

5. **Run the application:**
   - Set `OptiScaler.UI` as startup project
   - Press F5 to debug

---

## Building & Packaging

### Debug Build

```powershell
dotnet build --configuration Debug
```

### Release Build

```powershell
dotnet build --configuration Release
```

### Create MSIX Package

See [MSIX_PACKAGING_GUIDE.md](MSIX_PACKAGING_GUIDE.md) for detailed instructions.

Quick steps:
1. Build in Release mode
2. Right-click `OptiScaler.UI` project > Publish > Create App Packages
3. Follow the wizard to create MSIX package

---

## Documentation

### Development Guides
- **[MSIX_PACKAGING_GUIDE.md](MSIX_PACKAGING_GUIDE.md)** - How to create MSIX packages
- **[PARTNER_CENTER_UPDATED_GUIDE.md](PARTNER_CENTER_UPDATED_GUIDE.md)** - Partner Center setup
- **[PARTNER_CENTER_CHECKLIST.md](PARTNER_CENTER_CHECKLIST.md)** - Pre-submission checklist
- **[STORE_SUBMISSION_GUIDE.md](STORE_SUBMISSION_GUIDE.md)** - Store submission process
- **[CERTIFICATE_OPTIONS_GUIDE.md](CERTIFICATE_OPTIONS_GUIDE.md)** - Code signing options

### Technical Documentation
- **[docs/ARCHITECTURE.md](docs/ARCHITECTURE.md)** - Architecture overview
- **[docs/USAGE_EXAMPLES.md](docs/USAGE_EXAMPLES.md)** - Code examples
- **[CHANGELOG.md](CHANGELOG.md)** - Version history

### Store Content
- **[STORE_DESCRIPTION.md](STORE_DESCRIPTION.md)** - Store listing description
- **[STORE_SUBMISSION_CHECKLIST.md](STORE_SUBMISSION_CHECKLIST.md)** - Submission checklist

---

## Key Features

### Multi-Platform Game Detection
- Steam, Epic Games Store, Xbox (PC Game Pass), GOG, EA, Ubisoft Connect
- Automatic game library scanning
- Registry and filesystem-based detection

### Upscaling Mod Management
- OptiScaler framework integration
- Support for DLSS, FSR 2.2/3.1, XeSS
- Frame generation (DLSS 3, FSR 3)
- Automatic backup and restore

### User Interface
- Xbox-style design with Fluent Design System
- Gamepad and keyboard navigation
- Dark theme support
- Responsive layout

### Privacy & Security
- No telemetry or analytics
- Fully offline operation
- Local-only data storage
- Open configuration files (INI format)

---

## Contributing

This is a private repository. For external contributions:

1. Report issues or suggest features in the [public repository](https://github.com/Bigflood92/OptiScaler-Manager-WinUI/issues)
2. Contact the maintainer for collaboration opportunities

---

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file.

### Third-Party Components

- [OptiScaler](https://github.com/cdozdil/OptiScaler) - MIT License
- [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet) - MIT License
- [SharpCompress](https://github.com/adamhathcock/sharpcompress) - MIT License

See [THIRD_PARTY_NOTICES.txt](THIRD_PARTY_NOTICES.txt) for complete attribution.

---

## Security

### Sensitive Files (Never Commit)

The `.gitignore` is configured to exclude:
- **Certificates** (*.pfx, *.p12, *.key)
- **Secrets** (secrets.json, .env files)
- **Build outputs** (bin/, obj/, AppPackages/)
- **Local scripts** (Test*.ps1, local development scripts)

**Important:** Always verify that no sensitive data is committed before pushing.

---

## Support & Contact

- **Developer:** Bigflood92
- **Public Repository:** https://github.com/Bigflood92/OptiScaler-Manager-WinUI
- **Documentation:** https://bigflood92.github.io/OptiScaler-Manager-WinUI/

---

## Version

**Current Version:** 0.1.0 (Development)  
**Last Updated:** January 2025

---

**This is a private development repository. Do not share access without authorization.**
