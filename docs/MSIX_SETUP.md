# 🏗️ MSIX Packaging Configuration

## 📦 **Windows Application Packaging Project Setup**

### **1. Create Packaging Project**

Add Windows Application Packaging Project to solution:

```xml
<!-- OptiScaler.Package.wapproj -->
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="15.0" DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <PropertyGroup>
    <ProjectGuid>{PACKAGE-GUID-HERE}</ProjectGuid>
    <TargetPlatformVersion>10.0.19041.0</TargetPlatformVersion>
    <TargetPlatformMinVersion>10.0.17763.0</TargetPlatformMinVersion>
    <DefaultLanguage>en-US</DefaultLanguage>
    <PackageCertificateKeyFile>OptiScaler.Manager_TemporaryKey.pfx</PackageCertificateKeyFile>
    <PackageCertificateThumbprint>CERT-THUMBPRINT-HERE</PackageCertificateThumbprint>
    <GenerateAppInstallerFile>False</GenerateAppInstallerFile>
    <AppxAutoIncrementPackageRevision>True</AppxAutoIncrementPackageRevision>
    <GenerateTestCertificate>True</GenerateTestCertificate>
    <HoursBetweenUpdateChecks>0</HoursBetweenUpdateChecks>
  </PropertyGroup>
  
  <ItemGroup>
    <AppxManifest Include="Package.appxmanifest">
      <SubType>Designer</SubType>
    </AppxManifest>
  </ItemGroup>
  
  <ItemGroup>
    <ProjectReference Include="..\OptiScaler.UI\OptiScaler.UI.csproj" />
  </ItemGroup>
  
  <ItemGroup>
    <Content Include="Assets\**\*.*" />
  </ItemGroup>
  
  <Import Project="$(WapProjPath)\Microsoft.DesktopBridge.props" />
  <Import Project="$(WapProjPath)\Microsoft.DesktopBridge.targets" />
</Project>
```

### **2. Package.appxmanifest Configuration**

```xml
<?xml version="1.0" encoding="utf-8"?>
<Package xmlns="http://schemas.microsoft.com/appx/manifest/foundation/windows10"
         xmlns:mp="http://schemas.microsoft.com/appx/2014/phone/manifest"
         xmlns:uap="http://schemas.microsoft.com/appx/manifest/uap/windows10"
         xmlns:desktop="http://schemas.microsoft.com/appx/manifest/desktop/windows10"
         xmlns:rescap="http://schemas.microsoft.com/appx/manifest/foundation/windows10/restrictedcapabilities"
         IgnorableNamespaces="mp uap desktop rescap">

  <Identity Name="BigfloodSoftware.OptiScalerManager"
            Publisher="CN=Bigflood Software"
            Version="0.0.1.0" />

  <Properties>
    <DisplayName>OptiScaler Manager</DisplayName>
    <PublisherDisplayName>Bigflood Software</PublisherDisplayName>
    <Logo>Assets\StoreLogo.png</Logo>
    <Description>Modern Windows app for game optimization with FSR3 and DLSS mods</Description>
  </Properties>

  <Dependencies>
    <TargetDeviceFamily Name="Windows.Universal" MinVersion="10.0.17763.0" MaxVersionTested="10.0.22631.0" />
    <TargetDeviceFamily Name="Windows.Desktop" MinVersion="10.0.17763.0" MaxVersionTested="10.0.22631.0" />
  </Dependencies>

  <Resources>
    <Resource Language="x-generate"/>
  </Resources>

  <Applications>
    <Application Id="OptiScalerManager" Executable="OptiScaler.UI.exe" EntryPoint="Windows.FullTrustApplication">
      <uap:VisualElements DisplayName="OptiScaler Manager"
                         Description="Modern Windows app for game optimization"
                         BackgroundColor="transparent"
                         Square150x150Logo="Assets\Square150x150Logo.png"
                         Square44x44Logo="Assets\Square44x44Logo.png">
        <uap:DefaultTile Wide310x150Logo="Assets\Wide310x150Logo.png" 
                        Square310x310Logo="Assets\Square310x310Logo.png"
                        Square71x71Logo="Assets\Square71x71Logo.png">
          <uap:ShowNameOnTiles>
            <uap:ShowOn Tile="square150x150Logo"/>
            <uap:ShowOn Tile="wide310x150Logo"/>
            <uap:ShowOn Tile="square310x310Logo"/>
          </uap:ShowNameOnTiles>
        </uap:DefaultTile>
        <uap:SplashScreen Image="Assets\SplashScreen.png"/>
      </uap:VisualElements>
      
      <Extensions>
        <desktop:Extension Category="windows.fullTrustProcess" Executable="OptiScaler.UI.exe" />
        
        <!-- Game Bar Extension (Future) -->
        <uap:Extension Category="windows.protocol">
          <uap:Protocol Name="optiscaler">
            <uap:DisplayName>OptiScaler Manager Protocol</uap:DisplayName>
          </uap:Protocol>
        </uap:Extension>
      </Extensions>
    </Application>
  </Applications>

  <Capabilities>
    <Capability Name="internetClient" />
    <rescap:Capability Name="runFullTrust" />
    <rescap:Capability Name="unrestrictedData" />
  </Capabilities>
</Package>
```

### **3. Required Assets Checklist**

```
Assets/
├── StoreLogo.png (50x50)
├── Square44x44Logo.png
├── Square44x44Logo.targetsize-24_altform-unplated.png
├── Square71x71Logo.png
├── Square150x150Logo.png
├── Square310x310Logo.png
├── Wide310x150Logo.png
├── SplashScreen.png (620x300)
└── LockScreenLogo.png (24x24)
```

### **4. Build Commands**

```powershell
# Debug build
dotnet build -c Debug -f net8.0-windows10.0.19041.0

# Release build for store
dotnet build -c Release -f net8.0-windows10.0.19041.0

# Create MSIX package
msbuild OptiScaler.Package.wapproj /p:Configuration=Release /p:Platform=x64 /p:AppxBundlePlatforms=x64 /p:AppxPackageDir="C:\Packages\OptiScaler\"

# Run Windows App Certification Kit
"C:\Program Files (x86)\Windows Kits\10\App Certification Kit\appcert.exe" test -apptype AppxPackage -packagefullname [PackageFullName]
```

### **5. Certificate Management**

For Store submission:
1. Use certificate provided by Microsoft Store
2. For local testing: Use generated test certificate
3. For enterprise: Use your organization's certificate

### **6. Testing Checklist**

- [ ] App launches correctly from Start Menu
- [ ] All features work in packaged environment
- [ ] No crashes during normal operation
- [ ] Proper uninstall behavior
- [ ] Windows App Certification Kit passes
- [ ] Test on clean Windows installation

---

**🚀 Ready for Store**: Once all checklist items complete, package is ready for Microsoft Store submission.