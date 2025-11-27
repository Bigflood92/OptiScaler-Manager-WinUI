# OptiScaler Manager - Microsoft Store Submission Checklist

## ?? **TECHNICAL REQUIREMENTS** (Pre-Submission)

### ? Package Requirements
- [x] Package.appxmanifest configured
- [x] Publisher certificate (CN=Bigflood92)
- [x] Version number (0.1.0.0)
- [x] App identity and display name
- [x] Capabilities declared
- [x] Support URL provided
- [ ] Privacy Policy URL (host PrivacyPolicy.html somewhere)

### ? Code Requirements
- [x] Global crash handler implemented
- [x] Privacy policy page created
- [ ] Age rating compliance check
- [ ] Input validation on all forms
- [ ] Keyboard/Gamepad navigation complete
- [ ] AutomationProperties on controls

### ? Assets Required (ALL PNG)
- [ ] Square44x44Logo.png (44x44)
- [ ] Square71x71Logo.png (71x71) - SmallTile
- [ ] Square150x150Logo.png (150x150)
- [ ] Square310x310Logo.png (310x310) - LargeTile
- [ ] Wide310x150Logo.png (310x150)
- [ ] StoreLogo.png (50x50)
- [ ] BadgeLogo.png (24x24)
- [ ] SplashScreen.png (620x300)

---

## ?? **STORE LISTING REQUIREMENTS**

### App Title & Description
**Title:** OptiScaler Manager  
**Subtitle:** (max 100 chars)
```
Modern game optimization with DLSS, FSR3, and XeSS upscaling
```

**Short Description:** (max 200 chars)
```
Manage DLSS, FSR, and XeSS upscaling mods for PC games. Improve performance and visual quality with advanced frame generation and upscaling technology.
```

**Full Description:** (max 10,000 chars)
```markdown
# OptiScaler Manager

Transform your PC gaming experience with OptiScaler Manager - the modern Windows app for managing advanced upscaling and frame generation mods.

## ?? Key Features

### Intelligent Game Detection
- Automatic scanning of Steam, Epic Games, and EA App libraries
- Manual game addition support
- Detailed game information and mod status

### Multiple Upscaling Technologies
- **NVIDIA DLSS** - AI-powered upscaling for RTX GPUs
- **AMD FSR 2.2/3.1** - Universal upscaling for all GPUs
- **Intel XeSS** - Optimized for Arc GPUs
- Auto-detection based on your hardware

### Advanced Frame Generation
- DLSS Frame Generation support
- FSR 3 Frame Generation
- Significant FPS improvements in supported games

### Easy Mod Management
- One-click mod installation
- Automatic configuration based on GPU
- Built-in preset configurations
- Per-game custom settings

### Modern Xbox-Style Interface
- Beautiful dark theme design
- Full gamepad support
- Keyboard navigation
- Optimized for Windows 11

## ?? How It Works

1. **Scan Your Games** - OptiScaler Manager automatically finds installed games
2. **Choose a Preset** - Select Auto, Performance, Balanced, or Quality
3. **Install Mod** - One click to install and configure
4. **Launch & Play** - Enjoy better performance and visual quality

## ?? Configuration Options

- Upscaler selection (DLSS/FSR/XeSS)
- Quality presets (Ultra Quality to Ultra Performance)
- Frame generation toggle
- In-game overlay menu
- Sharpness adjustment
- DLL injection method
- Advanced per-technology settings

## ?? System Requirements

- Windows 10 version 1809 (build 17763) or higher
- Recommended: Windows 11
- .NET 8 Runtime (included)
- DirectX 12 compatible GPU
- 200 MB free disk space

## ?? Supported Platforms

- Steam
- Epic Games Store
- EA App
- Microsoft Store games
- Manual installation path

## ?? Privacy & Security

OptiScaler Manager respects your privacy:
- No user tracking or analytics
- No data collection
- All operations are local
- Open mod configurations
- Crash logs stored locally only

## ?? Perfect For

- Gamers with mid-range GPUs wanting better performance
- RTX users maximizing DLSS capabilities
- AMD GPU owners using FSR 3.1
- Intel Arc GPU optimization
- Multi-GPU system configuration

## ? Why OptiScaler Manager?

- **Modern Design** - Beautiful WinUI 3 interface
- **Easy to Use** - No technical knowledge required
- **Safe** - Non-invasive mod installation
- **Flexible** - Extensive customization options
- **Free** - No subscriptions or ads

Transform your gaming performance today with OptiScaler Manager!
```

### Screenshots Needed (4-8 images, 1920x1080)
1. **Main Games Library** - Showing detected games
2. **Game Configuration Dialog** - Preset selection
3. **Mod Installation Success** - Post-install view
4. **Settings/About Page** - System info
5. **Advanced Configuration** (optional)
6. **Multiple Platforms** (optional)

### Categories (Select 2-3)
- **Primary:** Games > Game Utilities
- **Secondary:** Productivity > Utilities & Tools
- **Tertiary:** Developer Tools

### Keywords (max 7)
```
DLSS, FSR, upscaling, frame generation, game mod, performance, optimization
```

---

## ?? **PUBLISHER INFORMATION**

### Contact Info
- **Email:** bigflood92@example.com (CHANGE THIS)
- **Support URL:** https://github.com/Bigflood92/OptiScaler-Manager
- **Privacy URL:** https://yourdomain.com/privacy (HOST THE HTML FILE)

### Age Rating
**Recommended:** PEGI 3 / ESRB Everyone  
**Reason:** Utility app, no mature content

### Content Declarations
- [ ] No advertising
- [ ] No in-app purchases
- [ ] No data collection
- [x] Accesses file system (for game mod installation)

---

## ?? **STORE SUBMISSION NOTES**

### Justification for `broadFileSystemAccess`
```
OptiScaler Manager requires broadFileSystemAccess capability to:

1. Scan game installation directories across multiple drives
2. Install mod files (.dll, .ini) to game directories
3. Read and modify game configuration files
4. Support games installed in protected directories (Program Files)

Without this capability, the app cannot perform its core function of 
mod installation and management for games located outside the app's 
sandboxed environment.

Alternative implementations would require users to manually copy files, 
defeating the purpose of automated mod management.
```

### First Review Preparation
- Test on clean Windows 10 & 11 VMs
- Verify all gamepad/keyboard navigation works
- Check accessibility features
- Test crash handler thoroughly
- Have 3-4 test games ready for screenshots

---

## ?? **POST-SUBMISSION**

### After Approval
1. Update `AppInfo.StoreUrl` with actual Store link
2. Add "Rate & Review" prompt after 3 uses
3. Monitor crash logs from users
4. Prepare v0.2.0 with user feedback

### Update Process
1. Increment version in Package.appxmanifest
2. Update AppInfo.Version
3. Write release notes
4. Resubmit through Partner Center

---

## ?? **TIMELINE ESTIMATE**

- **Assets Creation:** 4-6 hours (design + export)
- **Screenshots:** 2 hours (capture + edit)
- **Description Writing:** 1 hour
- **Testing:** 4 hours (clean VM tests)
- **Submission Form:** 1-2 hours
- **Microsoft Review:** 1-3 business days

**Total to submission:** ~2-3 days of work  
**Total to approval:** ~1 week

---

## ?? **PRIORITY ORDER**

### This Week (Store Ready)
1. ? Crash handler (DONE)
2. ?? Create all PNG assets
3. ?? Capture screenshots
4. ?? Write store description
5. ?? Host privacy policy online
6. ?? Complete keyboard navigation
7. ?? Test on clean Windows 10/11

### Next Week (Submit)
8. ?? Create Partner Center account
9. ?? Fill submission form
10. ? Wait for review
11. ?? Publish!
