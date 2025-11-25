# OptiScaler Manager

A modern Windows application for managing upscaling mods (DLSS, FSR, XeSS) for PC games.

## ?? Features

- **Multi-Platform Support**: Automatically detects games from Steam, Epic Games, Xbox, GOG, EA, and Ubisoft
- **Upscaling Technology**: Install and configure DLSS, FSR 3.1, and XeSS mods
- **Frame Generation**: Enable frame generation for supported games
- **Easy Configuration**: User-friendly interface for managing mod settings
- **Automatic Updates**: Download latest mod versions from GitHub

## ?? Installation

### From Microsoft Store (Recommended)
*Coming soon*

### From Releases
1. Download the latest `.msix` package from [Releases](https://github.com/Bigflood92/OptiScaler-Manager-WinUI/releases)
2. Double-click to install
3. Launch from Start Menu

## ?? Usage

1. **Scan for Games**: Click "Scan for Games" to detect installed games
2. **Select a Game**: Choose a game from your library
3. **Install Mod**: Click "Install Mods" to add upscaling support
4. **Configure**: Customize upscaler, quality preset, and advanced settings
5. **Launch**: Start your game with enhanced graphics

## ??? Development

### Prerequisites
- Visual Studio 2022 or later
- .NET 8 SDK
- Windows App SDK 1.8+

### Building
```bash
git clone https://github.com/Bigflood92/OptiScaler-Manager-WinUI.git
cd OptiScaler-Manager-WinUI
dotnet build
```

### Running
```bash
dotnet run --project OptiScaler.UI
```

## ?? Documentation

- [MSIX Packaging Guide](MSIX_PACKAGING_GUIDE.md) - How to create packages for distribution
- [Phase Plan](PHASE_PLAN_UPDATED.md) - Development roadmap and progress
- [Store Submission Checklist](STORE_SUBMISSION_CHECKLIST.md) - Microsoft Store requirements
- [Changelog](CHANGELOG.md) - Version history

## ?? Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## ?? License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## ?? Privacy

OptiScaler Manager respects your privacy:
- ? No data collection or telemetry
- ? No analytics or tracking
- ? All settings stored locally
- ? Crash logs remain on your device

See our [Privacy Policy](https://bigflood92.github.io/OptiScaler-Manager-WinUI/PrivacyPolicy.html) for details.

## ?? Credits

- [OptiScaler](https://github.com/cdozdil/OptiScaler) - The upscaling mod framework
- Platform logos are trademarks of their respective owners

## ?? Support

- [GitHub Issues](https://github.com/Bigflood92/OptiScaler-Manager-WinUI/issues) - Bug reports and feature requests
- [Discussions](https://github.com/Bigflood92/OptiScaler-Manager-WinUI/discussions) - Community support

---

**Made with ?? for the PC gaming community**
