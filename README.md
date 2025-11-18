# OptiScaler Manager

> **⚡ Modern Windows App for Game Optimization** - Built with .NET 8 & WinUI 3

![Version](https://img.shields.io/badge/version-0.0.1-blue)
![Framework](https://img.shields.io/badge/.NET-8.0-purple)
![UI](https://img.shields.io/badge/UI-WinUI_3-green)
![Platform](https://img.shields.io/badge/platform-Windows_10/11-lightgrey)

## 🎯 **About**

**OptiScaler Manager** is the next-generation evolution of the popular OptiScaler tool, completely rewritten for modern Windows with Microsoft Store distribution and Xbox Game Bar integration.

## 🚀 **Key Features**

### **✨ Current (Python Version) Features**
- 🎮 **Auto Game Detection** - Steam, Epic, Xbox Game Pass, GOG
- 📥 **One-Click Mod Installation** - OptiScaler & DLSSG-to-FSR3
- ⚙️ **Smart Configuration** - Performance/Balanced/Quality presets
- 🔄 **Auto-Updates** - Latest mods and app versions
- 🖥️ **GPU Detection** - NVIDIA/AMD/Intel optimization

### **🆕 New (.NET Version) Features**
- 🏪 **Microsoft Store** - Official distribution channel
- 🎮 **Xbox Game Bar** - Overlay integration (Win+G)
- 🎨 **Modern UI** - Native Windows 11 design
- 📦 **MSIX Packaging** - Secure, sandboxed installation
- ⚡ **Better Performance** - Native .NET runtime

## 🏗️ **Project Structure**

```
OptiScaler Manager/
├── 📄 OptiScaler.Manager.sln      # Visual Studio Solution
├── 🔧 OptiScaler.Core/            # Business Logic & Services
│   ├── Services/                  # Game scanning, GitHub API, etc.
│   ├── Models/                    # Data models
│   └── Contracts/                 # Interfaces
├── 🎨 OptiScaler.UI/              # WinUI 3 User Interface
│   ├── Views/                     # XAML Pages
│   ├── ViewModels/                # MVVM Pattern
│   └── Controls/                  # Custom Controls
├── 📦 OptiScaler.Package/         # MSIX Packaging (Future)
└── 📚 docs/                       # Documentation
```

## 🎨 **Technology Stack**

| 🏷️ **Component** | 🔧 **Technology** | 📋 **Purpose** |
|------------------|------------------|---------------|
| **Runtime** | .NET 8 LTS | Long-term support & performance |
| **UI Framework** | WinUI 3 | Native Windows experience |
| **Architecture** | MVVM | Clean separation of concerns |
| **Packaging** | MSIX | Modern app deployment |
| **Distribution** | Microsoft Store | Official channel |

## 🚀 **Development Roadmap**

| 🏁 **Version** | 🎯 **Milestone** | 📅 **Target** | ✅ **Status** |
|---------------|-----------------|---------------|--------------|
| **v0.0.1** | Project setup + Basic UI | Week 1 | 🚧 In Progress |
| **v0.1.0** | Core services migration | Week 2 | ⏳ Planned |
| **v0.2.0** | Game scanning & mod install | Week 3 | ⏳ Planned |
| **v0.3.0** | Xbox Game Bar integration | Week 4 | ⏳ Planned |
| **v0.4.0** | MSIX packaging | Week 5 | ⏳ Planned |
| **v1.0.0** | Microsoft Store release | Week 6 | ⏳ Planned |

## 🛠️ **Development Setup**

### **Prerequisites**
- Visual Studio 2022 Community (recommended) or VS Code
- .NET 8 SDK
- Windows 10 version 1903+ or Windows 11
- Windows App SDK

### **Quick Start**

```bash
# Clone repository
git clone https://github.com/Bigflood92/OptiScaler-Manager.git
cd "OptiScaler Manager"

# Restore packages
dotnet restore

# Build solution
dotnet build

# Run application
dotnet run --project OptiScaler.UI
```

## 📋 **Relationship with Original**

### **🐍 OptiScaler (Python) - v2.4.x**
- **Status**: ✅ Stable maintenance
- **Purpose**: Bug fixes and minor improvements
- **Distribution**: GitHub Releases (.exe)
- **Repository**: [OptiScaler-Manager](https://github.com/Bigflood92/OptiScaler-Manager) `main` branch

### **⚡ OptiScaler Manager (.NET) - v0.0.x**
- **Status**: 🚧 Active development
- **Purpose**: Complete modern rewrite
- **Distribution**: Microsoft Store (MSIX)
- **Repository**: This repository `main` branch

## 🤝 **Contributing**

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit changes (`git commit -m 'Add amazing feature'`)
4. Push to branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 **License**

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 **Acknowledgments**

- Original OptiScaler community for feedback and testing
- Microsoft for WinUI 3 and Windows App SDK
- All contributors who help make this project better

---

**🎮 Transform your gaming experience with modern Windows optimization tools!**