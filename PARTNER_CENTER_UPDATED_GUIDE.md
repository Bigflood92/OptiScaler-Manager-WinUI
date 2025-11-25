# ?? PARTNER CENTER - GUÍA ACTUALIZADA 2025

**La interfaz de Partner Center cambió. Aquí está la guía actualizada.**

---

## ?? **NUEVA ESTRUCTURA (Enero 2025):**

Después de crear una app, verás estas secciones en el menú lateral izquierdo:

```
OptiScaler Manager
?? Product identity
?? Availability  
?? Properties
?? Pricing
?? Store listings
?? Packages
?? Submit
```

---

## ?? **PASO 1: RESERVAR NOMBRE**

1. Dashboard: https://partner.microsoft.com/dashboard
2. Click: **"Apps and games"**
3. Click: **"+ New"** o **"+ New product"**
4. Tipo de producto: **"MSIX or PWA app"**
5. Nombre: **"OptiScaler Manager"**
6. Click: **"Reserve product name"**

**Nombre reservado ?**

---

## ?? **PASO 2: AVAILABILITY (Disponibilidad)**

En el menú lateral izquierdo, click: **"Availability"**

**Markets (Mercados):**
- Selecciona: **"All markets"**
- O selecciona países específicos si prefieres

**Release schedule:**
- Opción 1: **"Release as soon as it passes certification"** ? (recomendado)
- Opción 2: Selecciona fecha específica

Click: **"Save"**

**Availability completo ?**

---

## ?? **PASO 3: PROPERTIES (Propiedades)**

En el menú lateral izquierdo, click: **"Properties"**

### **3.1 Category and subcategory:**

- **Category:** Developer tools
- **Subcategory:** Other developer tools

(Alternativa: Utilities & tools > System optimization)

### **3.2 System requirements:**

**Minimum:**
- OS: Windows 10 version 1903 (build 18362)
- Architecture: x64
- Memory: 2 GB
- DirectX: Version 11
- Disk space: 500 MB

**Recommended:**
- OS: Windows 11 (latest version)
- Architecture: x64
- Memory: 4 GB
- Disk space: 1 GB

### **3.3 Support information:**

- **Privacy policy URL:** 
  ```
  https://bigflood92.github.io/OptiScaler-Manager-WinUI/PrivacyPolicy.html
  ```

- **Website:**
  ```
  https://github.com/Bigflood92/OptiScaler-Manager-WinUI
  ```

- **Support contact email:**
  ```
  (tu email personal o de soporte)
  ```

### **3.4 Age rating:**

Click: **"Get age rating"** (o "IARC rating")

**Cuestionario:**
- Violence: No
- Sexual content: No
- Nudity: No
- Bad language: No
- Controlled substances: No
- Gambling: No
- User-generated content: No
- Location sharing: No
- Digital purchases: No

**Resultado esperado:** PEGI 3 / ESRB Everyone

Click: **"Save rating"**

Click: **"Save"** (guardar todo Properties)

**Properties completo ?**

---

## ?? **PASO 4: PRICING (Precio)**

En el menú lateral izquierdo, click: **"Pricing"**

### **4.1 Base price:**

- Selecciona: **"Free"** ?
- (Recomendado para v0.1.0, puedes cambiar después)

### **4.2 Free trial:**

- No aplica (la app ya es gratis)

### **4.3 Add-ons:**

- Skip por ahora (puedes agregar IAP después)

Click: **"Save"**

**Pricing completo ?**

---

## ?? **PASO 5: STORE LISTINGS (Lo más importante)**

En el menú lateral izquierdo, click: **"Store listings"**

### **5.1 Crear listing:**

1. Click: **"Manage store listing languages"**
2. Click: **"+ Add a language"**
3. Selecciona: **"English (United States)"**
4. Click: **"Update"**
5. Click en **"English (United States)"** para editarlo

### **5.2 Product description:**

**Copia esto:**

```
OptiScaler Manager - Enhance Your Gaming Experience

Transform your gaming visuals with OptiScaler Manager, a powerful Windows utility for managing upscaling mods (DLSS, FSR 3.1, XeSS) in your favorite PC games.

KEY FEATURES:
? Automatic Game Detection - Scans Steam, Epic, Xbox, GOG, EA, and Ubisoft libraries
? One-Click Mod Installation - Install OptiScaler mods with a single click
? Multiple Upscaling Technologies - DLSS, FSR 3.1, XeSS support
? Frame Generation - Enable AMD FSR 3 frame generation
? Per-Game Configuration - Customize settings for each game
? Gamepad Support - Navigate with Xbox or PlayStation controllers
? Modern UI - Beautiful WinUI 3 interface

SUPPORTED PLATFORMS:
• Steam
• Epic Games Store
• Xbox Game Pass
• GOG Galaxy
• EA App
• Ubisoft Connect
• Manual game paths

PRIVACY FIRST:
? Zero data collection
? No telemetry or analytics
? All settings stored locally
? Open source

Perfect for gamers who want maximum performance and visual quality from their games.
```

### **5.3 Screenshots:**

**Requisitos:**
- Mínimo: 1366 x 768
- Máximo: 3840 x 2160
- Formato: PNG o JPG
- Tamaño: < 2 MB cada uno

**Upload tus 6 screenshots:**

1. Click: **"+ Add screenshot"**
2. Arrastra y suelta o selecciona desde:
   ```
   C:\Users\Jorge\OneDrive\OptiScaler Manager\Screenshots\
   ```

3. Upload en este orden:
   - `1_games_library.png` ? Caption: "Games Library"
   - `2_game_with_mods.png` ? Caption: "Game with Mods Installed"
   - `3_mod_configuration.png` ? Caption: "Mod Configuration"
   - `4_game_settings.png` ? Caption: "Game Settings"
   - `5_app_settings.png` ? Caption: "App Settings"
   - `6_mods_page.png` ? Caption: "Mods Download Page"

### **5.4 App tile icon (Store logo):**

Upload:
```
C:\Users\Jorge\OneDrive\OptiScaler Manager\OptiScaler.UI\Assets\Square150x150Logo.png
```

### **5.5 Search terms (Keywords):**

Máximo 7 términos separados por comas:

```
upscaling, DLSS, FSR, XeSS, gaming, frame generation, mod manager
```

### **5.6 Copyright and trademark info:**

```
© 2025 Bigflood92. All rights reserved.
```

### **5.7 Additional license terms:**

```
Licensed under MIT License. See GitHub repository for details.
```

### **5.8 Developed by:**

```
Bigflood92
```

### **5.9 Release notes:**

```
Initial release of OptiScaler Manager v0.1.0

Features:
- Multi-platform game detection
- OptiScaler mod installation
- Per-game configuration
- Gamepad and keyboard navigation
- Modern WinUI 3 interface

This is the first public release. Feedback welcome!
```

Click: **"Save"**

**Store listings completo ?**

---

## ?? **PASO 6: PACKAGES (Upload MSIX)**

En el menú lateral izquierdo, click: **"Packages"**

### **6.1 Upload package:**

1. Click: **"Browse"** o arrastra y suelta
2. Selecciona:
   ```
   C:\Users\Jorge\OneDrive\OptiScaler Manager\PackageOutput\OptiScalerManager_10.0.26100.0_x64.msix
   ```
3. Espera que suba (120 MB, ~2-5 minutos)

### **6.2 Verificación automática:**

Microsoft verifica:
- ? Estructura del package
- ? Manifesto válido
- ? Firma (Microsoft re-firma automáticamente)
- ? Capabilities

**Metadata extraído:**
- Version: 0.1.0.0
- Architecture: x64
- Min OS: Windows 10 (10.0.17763.0)
- Package name: Bigflood92.OptiScalerManager
- Publisher: CN=Bigflood92

### **6.3 Capabilities detectadas:**

- internetClient ?
- broadFileSystemAccess ?
- runFullTrust ?

Click: **"Save"**

**Packages completo ?**

---

## ?? **PASO 7: SUBMIT (Enviar a certificación)**

### **7.1 Verificar todo:**

En el menú lateral izquierdo, verifica que todas las secciones tienen ?:

- [x] Product identity
- [x] Availability
- [x] Properties
- [x] Pricing
- [x] Store listings
- [x] Packages

### **7.2 Notes for certification (opcional):**

Antes de submit, puedes agregar notas para el revisor:

Click: **"Submission options"** o **"Notes for certification"**

**Escribe:**

```
This is the initial release of OptiScaler Manager v0.1.0, a game optimization utility.

Testing Notes:
- The app requires game installation directories to function properly
- broadFileSystemAccess is needed to scan game folders across drives
- For testing, you can use the "Manual" game path option if no games are detected
- Screenshots show the full functionality

The app is open source: https://github.com/Bigflood92/OptiScaler-Manager-WinUI
Privacy Policy: https://bigflood92.github.io/OptiScaler-Manager-WinUI/PrivacyPolicy.html

No sensitive data is collected. All settings are stored locally.

Thank you for your review!
```

### **7.3 Submit:**

1. Click: **"Submit for review"** o **"Submit to Store"**
2. Confirmación: **"Yes, submit"**

**Status:**
- Queued for certification
- En 1-2 horas verás: "In certification"

**Submission enviada ?**

---

## ? **QUÉ PASA AHORA (3-7 DÍAS):**

**Día 0-1: Automated checks**
- Malware scan
- Package validation
- Basic compliance

**Día 1-3: Manual review**
- Tester instala la app
- Verifica funcionalidad básica
- Revisa contenido

**Día 3-5: Final approval**
- Compliance review
- Firma oficial de Microsoft
- Publicación

**Email notifications:**
- Recibirás email en cada cambio de estado
- "In certification" ? "Passed certification" ? "Publishing" ? "In Store"

---

## ?? **VERIFICAR PROGRESO:**

1. Dashboard > Apps and games
2. Click: **"OptiScaler Manager"**
3. Status actual:
   - "In certification" (en progreso)
   - "Passed certification" (aprobado)
   - "Published" (live en Store)

---

## ?? **SI HAY PROBLEMAS:**

### **Error: "Privacy policy URL not reachable"**

**Solución:**
1. Verifica que la URL funciona en tu navegador:
   ```
   https://bigflood92.github.io/OptiScaler-Manager-WinUI/PrivacyPolicy.html
   ```
2. Si no carga, espera 5 minutos (propagación)
3. Actualiza en Properties

### **Error: "Package validation failed"**

**Solución:**
1. Regenera package:
   ```powershell
   .\Create-MSIXPackage.ps1
   ```
2. Upload nuevo package

### **Warning: "Description too long"**

**Solución:**
- Descripción máxima: 10,000 caracteres
- Tu descripción actual: ~900 caracteres ?
- No deberías tener este error

---

## ? **CHECKLIST FINAL:**

Antes de submit, verifica:

- [ ] Nombre reservado: "OptiScaler Manager" ?
- [ ] Availability: All markets ?
- [ ] Properties: Category, age rating ?
- [ ] Pricing: Free ?
- [ ] Store listing: Descripción + 6 screenshots ?
- [ ] Package: MSIX uploaded (120 MB) ?
- [ ] Notes for certification: Escritas ?
- [ ] Todo tiene checkmark verde ?

**Si todo tiene ? ? Submit!**

---

## ?? **¡LISTO!**

**Timeline esperado:**
- Hoy: Submit ?
- Día 1-2: Automated checks
- Día 3-4: Manual review
- Día 4-5: Approval
- Día 5: **LIVE EN STORE** ??

---

**Última actualización:** 25 Enero 2025  
**Interfaz:** Partner Center 2025 (actualizada)
