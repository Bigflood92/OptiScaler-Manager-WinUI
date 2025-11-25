# ?? MSIX PACKAGING GUIDE - PASO A PASO

## ?? OBJETIVO:
Crear un MSIX package para Microsoft Store submission

---

## ?? ESTADO ACTUAL:

### ? LO QUE YA TIENES:
- ? Package.appxmanifest configurado
- ? Assets optimizados (PNG < 50KB)
- ? Privacy Policy URL en manifest
- ? App funcional sin crashes
- ? Screenshots listos

### ?? LO QUE FALTA:
- ? Configurar WindowsPackageType
- ? Crear certificado de desarrollo
- ? Build MSIX package
- ? Testing del package

---

## ?? OPCIÓN 1: PACKAGING AUTOMÁTICO (RECOMENDADO)

### **Paso 1: Modificar .csproj para packaging**

Ya tienes `<EnableMsixTooling>true</EnableMsixTooling>` pero necesitas cambiar:

**ANTES:**
```xml
<WindowsPackageType>None</WindowsPackageType>
```

**DESPUÉS:**
```xml
<WindowsPackageType>MSIX</WindowsPackageType>
```

### **Paso 2: Build en Release mode**

```powershell
# Limpiar builds anteriores
dotnet clean

# Build Release
dotnet build -c Release

# O desde Visual Studio: Build > Configuration Manager > Release
```

### **Paso 3: Crear MSIX package**

```powershell
# Método 1: Visual Studio
# 1. Click derecho en OptiScaler.UI project
# 2. Publish > Create App Packages
# 3. Seguir wizard

# Método 2: Command line
dotnet publish -c Release -r win-x64 --self-contained true
```

---

## ?? OPCIÓN 2: PACKAGING MANUAL (MÁS CONTROL)

### **Paso 1: Instalar Windows SDK**

Verifica que tienes Windows SDK instalado:
```powershell
# Verificar makeappx.exe
Get-Command makeappx.exe
```

Si no existe:
```
Descargar de: https://developer.microsoft.com/windows/downloads/windows-sdk/
```

### **Paso 2: Crear certificado auto-firmado (para testing)**

```powershell
# Crear certificado
$cert = New-SelfSignedCertificate `
    -Type Custom `
    -Subject "CN=Bigflood92" `
    -KeyUsage DigitalSignature `
    -FriendlyName "OptiScaler Manager Dev Certificate" `
    -CertStoreLocation "Cert:\CurrentUser\My" `
    -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.3", "2.5.29.19={text}")

# Exportar certificado (opcional, para otros PCs)
$password = ConvertTo-SecureString -String "YourPassword123!" -Force -AsPlainText
Export-PfxCertificate -Cert $cert -FilePath "OptiScalerManager_TemporaryKey.pfx" -Password $password
```

### **Paso 3: Build en Release**

```powershell
dotnet publish OptiScaler.UI\OptiScaler.UI.csproj `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=false `
    -p:PublishReadyToRun=true
```

### **Paso 4: Crear estructura de package**

```powershell
# Crear carpeta temporal
New-Item -ItemType Directory -Path "PackageFiles" -Force

# Copiar archivos publicados
Copy-Item -Path "OptiScaler.UI\bin\Release\net8.0-windows10.0.19041.0\win-x64\publish\*" `
    -Destination "PackageFiles\" -Recurse -Force

# Copiar manifest
Copy-Item -Path "OptiScaler.UI\Package.appxmanifest" `
    -Destination "PackageFiles\" -Force

# Copiar assets
Copy-Item -Path "OptiScaler.UI\Assets\*" `
    -Destination "PackageFiles\Assets\" -Recurse -Force
```

### **Paso 5: Crear MSIX con makeappx**

```powershell
# Navegar a Windows SDK bin folder
cd "C:\Program Files (x86)\Windows Kits\10\bin\10.0.22621.0\x64"

# Crear package
.\makeappx.exe pack /d "C:\Users\Jorge\OneDrive\OptiScaler Manager\PackageFiles" `
    /p "C:\Users\Jorge\OneDrive\OptiScaler Manager\OptiScalerManager_0.1.0.0_x64.msix"
```

### **Paso 6: Firmar package**

```powershell
# Firmar con certificado
.\signtool.exe sign /fd SHA256 /a /f "C:\Users\Jorge\OneDrive\OptiScaler Manager\OptiScalerManager_TemporaryKey.pfx" `
    /p "YourPassword123!" `
    "C:\Users\Jorge\OneDrive\OptiScaler Manager\OptiScalerManager_0.1.0.0_x64.msix"
```

---

## ?? TESTING DEL PACKAGE:

### **Método 1: Instalar certificado (solo primera vez)**

```powershell
# Importar certificado a Trusted Root
Import-Certificate -FilePath "OptiScalerManager_TemporaryKey.pfx" `
    -CertStoreLocation "Cert:\LocalMachine\Root" `
    -Password $password
```

### **Método 2: Instalar package**

```powershell
# Opción A: Double-click en .msix file
# Opción B: PowerShell
Add-AppxPackage -Path "OptiScalerManager_0.1.0.0_x64.msix"
```

### **Método 3: Verificar instalación**

```powershell
# Ver app instalada
Get-AppxPackage | Where-Object {$_.Name -like "*OptiScaler*"}

# Lanzar app
# Buscar "OptiScaler Manager" en Start Menu
```

---

## ?? PARA MICROSOFT STORE:

### **Diferencias importantes:**

1. **Publisher Certificate:**
   - Para Store, Microsoft firma tu package
   - Tu certificado auto-firmado es solo para testing

2. **Identity Name:**
   - Debe coincidir con el reservado en Partner Center
   - Formato: `Publisher.AppName` (ej: `Bigflood92.OptiScalerManager`)

3. **Package Upload:**
   - Subir `.msix` o `.msixupload`
   - Partner Center valida automáticamente

---

## ?? CHECKLIST PRE-SUBMISSION:

- [ ] Package.appxmanifest completo
  - [ ] Identity Name correcto
  - [ ] Publisher correcto (debe coincidir con Partner Center)
  - [ ] Version incrementada (0.1.0.0)
  - [ ] Privacy Policy URL válida
  
- [ ] Assets incluidos
  - [ ] Square44x44Logo.png
  - [ ] Square150x150Logo.png
  - [ ] Wide310x150Logo.png
  - [ ] LargeTile.png (Square310x310Logo)
  - [ ] StoreLogo.png
  - [ ] SplashScreen.png
  
- [ ] Capabilities necesarias
  - [ ] internetClient (para descargar mods)
  - [ ] runFullTrust (app desktop completa)
  - [ ] broadFileSystemAccess (acceso a carpetas de juegos)
  
- [ ] Testing
  - [ ] Install package funciona
  - [ ] App launches sin crashes
  - [ ] Todas las features funcionan
  - [ ] Privacy Policy link funciona

---

## ?? TROUBLESHOOTING:

### **Error: "Package could not be registered"**
```
Solución: Desinstalar versión anterior primero
Get-AppxPackage Bigflood92.OptiScalerManager | Remove-AppxPackage
```

### **Error: "Certificate chain not trusted"**
```
Solución: Instalar certificado en Trusted Root
Import-Certificate -FilePath cert.pfx -CertStoreLocation Cert:\LocalMachine\Root
```

### **Error: "Publisher and certificate subject mismatch"**
```
Solución: Verificar que Publisher en manifest = Subject en certificado
Subject en cert debe ser: CN=Bigflood92
```

### **Error: "Assets not found"**
```
Solución: Verificar rutas relativas en manifest
Assets deben estar en carpeta "Assets\" dentro del package
```

---

## ?? SIGUIENTE PASO RECOMENDADO:

### **OPCIÓN RÁPIDA (Recomendada):**

1. ? Modificar .csproj:
   ```xml
   <WindowsPackageType>MSIX</WindowsPackageType>
   ```

2. ? Build desde Visual Studio:
   - Click derecho en proyecto ? Publish
   - Create App Packages
   - Sideloading
   - Seguir wizard

3. ? Testing local

### **OPCIÓN COMPLETA (Más control):**

Usar método manual con makeappx (arriba)

---

## ?? RECURSOS:

- [MSIX Packaging Guide](https://docs.microsoft.com/windows/msix/)
- [Windows App SDK Packaging](https://docs.microsoft.com/windows/apps/windows-app-sdk/deploy-packaged-apps)
- [Microsoft Partner Center](https://partner.microsoft.com/dashboard)

---

**¿Quieres que proceda con la configuración automática?** ??
