# ?? MICROSOFT STORE SUBMISSION - GUÍA PASO A PASO

**¡Felicitaciones por activar tu cuenta Partner Center!**

Ahora vamos a publicar OptiScaler Manager en Microsoft Store en 5 pasos.

---

## ?? **CHECKLIST PRE-SUBMISSION:**

Verifica que tienes todo listo:

- [x] ? Cuenta Partner Center activa ($19 pagados)
- [x] ? MSIX package creado (120.27 MB)
- [x] ? 6 Screenshots profesionales (Screenshots/*.png)
- [x] ? Privacy Policy URL live (GitHub Pages)
- [x] ? Store Description (STORE_DESCRIPTION.md)
- [x] ? Assets optimizados (< 50KB cada uno)
- [x] ? Package.appxmanifest válido

**Todo listo ? Procede con Paso 1**

---

## ?? **PASO 1: CREAR APP LISTING (15-20 min)**

### **1.1 Ir al Dashboard:**

1. Abre: https://partner.microsoft.com/dashboard
2. Click: **"Apps and games"** en el menú izquierdo
3. Click: **"+ New product"** (botón azul arriba a la derecha)
4. Selecciona: **"MSIX or PWA app"**

### **1.2 Reservar Nombre:**

1. **App name:** `OptiScaler Manager`
2. Click: **"Check availability"**
   - ? Si está disponible ? "Reserve product name"
   - ? Si no está disponible ? Prueba:
     - "OptiScaler Manager - Game Optimizer"
     - "OptiScaler Game Manager"
     - "OptiScaler Mod Manager"
3. Click: **"Reserve product name"**
4. Espera confirmación (10-30 segundos)

**Nombre reservado ? ? Continúa a 1.3**

### **1.3 Product Setup:**

1. En el menú izquierdo, click: **"Product setup"**

2. **Availability:**
   - Markets: **"All markets"** (o selecciona específicos)
   - Schedule: **"Make this product available as soon as it passes certification"**
   - Click: **"Save"**

3. **Properties:**
   - Category: **"Developer tools"**
   - Subcategory: **"Other developer tools"**
   - (Alternativa: "Utilities & tools" > "File managers")
   
   - System requirements:
     - Minimum OS: **Windows 10 version 1903 (build 18362)**
     - Recommended OS: **Windows 11 (latest)**
     - Architecture: **x64**
     - Memory: **2 GB**
     - Disk space: **500 MB**
   
   - Support info:
     - Privacy policy URL: `https://bigflood92.github.io/OptiScaler-Manager-WinUI/PrivacyPolicy.html`
     - Website: `https://github.com/Bigflood92/OptiScaler-Manager-WinUI`
     - Support email: (tu email)
   
   - Click: **"Save"**

4. **Age ratings:**
   - Click: **"Get age rating"**
   - Completa cuestionario:
     - Violence: No
     - Nudity: No
     - Profanity: No
     - Drugs: No
     - Gambling: No
     - User-generated content: No
     - Location sharing: No
     - Purchase of goods: No
   - Resultado esperado: **PEGI 3 / ESRB Everyone**
   - Click: **"Save rating"**

**Properties configuradas ? ? Continúa a 1.4**

### **1.4 Pricing:**

1. En el menú izquierdo, click: **"Pricing and availability"**

2. **Pricing:**
   - Base price: **"Free"** ? (recomendado para v0.1.0)
   - (Puedes cambiar a pago después)

3. **Free trial:**
   - No trial (ya es gratis)

4. **Sale pricing:**
   - Skip (no aplicable para apps gratis)

5. Click: **"Save"**

**Pricing configurado ? ? Continúa a 1.5**

### **1.5 Store Listing:**

**Esta es la parte más importante - lo que ven los usuarios**

1. En el menú izquierdo, click: **"Store listings"**
2. Click: **"+ Add/Edit listing"**
3. Selecciona idioma: **"English (United States)"**

**Descripción:**

Copia de `STORE_DESCRIPTION.md` y pega en el campo de descripción.

O usa este texto condensado:

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

**Screenshots:**

1. Click: **"+ Add screenshot"**
2. Upload tus 6 screenshots desde `Screenshots/`:
   - `1_games_library.png`
   - `2_game_with_mods.png`
   - `3_mod_configuration.png`
   - `4_game_settings.png`
   - `5_app_settings.png`
   - `6_mods_page.png`

3. Para cada screenshot:
   - Title: Descriptivo (ej: "Games Library", "Mod Configuration")
   - Caption: Breve descripción (opcional)

**App Icon:**

1. Upload: `OptiScaler.UI/Assets/Square150x150Logo.png`

**Search terms (keywords):**

Máximo 7 términos separados por comas:

```
upscaling, DLSS, FSR, XeSS, gaming, frame generation, mod manager
```

**Copyright and trademark:**

```
© 2025 Bigflood92. All rights reserved.
```

**Additional license terms:**

```
Licensed under MIT License. See GitHub repository for details.
```

**Developed by:**

```
Bigflood92
```

**Release notes:**

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

4. Click: **"Save"**

**Store listing completo ? ? Continúa al Paso 2**

---

## ?? **PASO 2: UPLOAD MSIX PACKAGE (5 min)**

### **2.1 Ir a Packages:**

1. En el menú izquierdo, click: **"Packages"**
2. Click: **"+ New package"**

### **2.2 Upload Package:**

1. Click: **"Browse"** o arrastra y suelta
2. Selecciona: `PackageOutput\OptiScalerManager_10.0.26100.0_x64.msix`
3. Espera que suba (120 MB, puede tardar 2-5 minutos)
4. **Progreso:**
   - Uploading... (barra de progreso)
   - Processing... (Microsoft extrae metadata)
   - Validating... (verifica estructura)
   - ? Package uploaded successfully

### **2.3 Verificar Metadata:**

Microsoft extrae automáticamente del package:

- **Version:** 0.1.0.0 ?
- **Architecture:** x64 ?
- **Target OS:** Windows 10 (10.0.17763.0) ?
- **Package name:** Bigflood92.OptiScalerManager ?
- **Publisher:** CN=Bigflood92 ?

Si todo se ve correcto ? Click **"Save"**

### **2.4 Package Capabilities:**

Microsoft detecta las capabilities del manifest:

- ? `internetClient` - For mod downloads
- ? `broadFileSystemAccess` - For game scanning
- ? `runFullTrust` - Desktop app

Verifica que sean correctas ? Click **"Save"**

**Package uploaded ? ? Continúa al Paso 3**

---

## ? **PASO 3: SUBMIT PARA CERTIFICACIÓN (2 min)**

### **3.1 Revisar Submission:**

1. En el menú izquierdo, click: **"Submission overview"**
2. Verifica que todas las secciones tienen ?:
   - [x] Product setup
   - [x] Pricing and availability
   - [x] Properties
   - [x] Age ratings
   - [x] Store listings
   - [x] Packages

Si todo tiene ? ? Continúa a 3.2

Si falta algo ? Ve a esa sección y completa

### **3.2 Notes for Certification (opcional pero recomendado):**

En el campo "Notes for certification", escribe:

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

### **3.3 Submit:**

1. Click: **"Submit to Store"** (botón azul grande)
2. Confirmación: **"Yes, submit"**
3. **Proceso iniciado:**
   - Generating package...
   - Queuing for certification...
   - ? Submission received

**Submission enviada ? ? Continúa al Paso 4**

---

## ? **PASO 4: CERTIFICACIÓN (3-7 DÍAS)**

### **4.1 Qué Pasa Ahora:**

Microsoft revisa tu app en varios pasos:

**Día 0-1: Validación Automática**
- Escaneo de malware
- Validación de package
- Verificación de compliance básico
- Análisis de capabilities

**Día 1-3: Revisión Manual**
- Tester humano instala la app
- Verifica que funciona básicamente
- Revisa contenido (no hay contenido inapropiado)
- Verifica que la descripción es precisa

**Día 3-5: Compliance Review**
- Verificación de políticas de Microsoft Store
- Privacy compliance
- Seguridad
- Accesibilidad básica

**Día 5-7: Aprobación Final**
- Revisión final
- Firma oficial de Microsoft
- Publicación

### **4.2 Seguimiento:**

**Ver estado:**
1. Dashboard > Apps and games > OptiScaler Manager
2. Click: **"Submission overview"**
3. Estado actual:
   - "In certification" (en progreso)
   - "Passed certification" (aprobado)
   - "Failed certification" (necesita correcciones)

**Notificaciones por email:**
- Recibirás email cuando cambie el estado
- Si hay issues, te dirán qué corregir

### **4.3 Posibles Resultados:**

**? Aprobación (80-90% de casos):**
- Email: "Your app passed certification"
- Estado: "Publishing"
- App aparece en Store en 1-2 horas

**?? Necesita Correcciones (10-15% de casos):**
- Email con detalles de qué corregir
- Ejemplos comunes:
  - Descripción inexacta
  - Screenshot de baja calidad (poco probable, los tuyos son profesionales)
  - Crash al lanzar (poco probable si testeas antes)
  - Privacy policy incompleta (la tuya es completa)
- Corriges y reenvías
- Nueva certificación (2-3 días más)

**? Rechazo (5% de casos):**
- Violación de políticas (malware, contenido inapropiado, etc.)
- Poco probable para tu app (es limpia)

### **4.4 Tiempo Esperado:**

**Escenario típico:**
- **Día 0:** Submit
- **Día 1:** Validación automática completa
- **Día 2-3:** Revisión manual
- **Día 3-4:** Aprobación
- **Día 4:** Publicación

**Total: 3-5 días en promedio**

---

## ?? **PASO 5: LAUNCH! (DÍA 4-7)**

### **5.1 App Publicada:**

Cuando recibas email "Your app passed certification":

**Tu app estará disponible en:**
- Microsoft Store app (Windows)
- Microsoft Store web (microsoft.com/store)
- Búsqueda de Store

**URL de tu app:**
```
ms-windows-store://pdp/?ProductId=XXXXXXXXX
```
(Recibirás el Product ID en el email)

### **5.2 Primeros Pasos Post-Launch:**

**Día 1:**
1. Verifica que la app aparece en búsqueda
2. Instala desde Store (verifica que funciona)
3. Toma screenshots de la página en Store
4. Comparte en redes sociales (opcional)

**Semana 1:**
1. Monitorea reviews y ratings
2. Responde a feedback de usuarios
3. Anota bugs reportados
4. Planea v0.2.0

**Mes 1:**
1. Analiza métricas (downloads, ratings)
2. Prioriza features basado en feedback
3. Prepara próxima actualización

### **5.3 Actualizar la App:**

**Cuando tengas v0.2.0:**
1. Build nuevo MSIX package
2. Dashboard > Packages > + New package
3. Upload nuevo MSIX
4. Submit (certificación más rápida, 1-2 días)
5. Los usuarios reciben update automático

---

## ?? **MÉTRICAS A MONITOREAR:**

### **En Partner Center:**

**Acquisitions:**
- Total downloads
- Conversions (page views ? installs)
- Source (search, external, etc.)

**Usage:**
- Daily active users
- Session duration
- Retention (users que vuelven)

**Reviews:**
- Ratings (1-5 stars)
- Written reviews
- Sentiment analysis

**Health:**
- Crashes
- Hangs
- Memory usage

---

## ?? **TIPS PARA ÉXITO:**

### **Antes de Submit:**
- ? Testea MSIX package localmente
- ? Verifica que todos los screenshots se ven bien
- ? Revisa descripción sin typos
- ? Asegúrate que Privacy Policy está live

### **Durante Certificación:**
- ? Paciencia (3-7 días es normal)
- ?? Monitorea email
- ?? Revisa Dashboard diariamente

### **Post-Launch:**
- ?? Responde a reviews
- ?? Fix bugs rápidamente
- ?? Monitorea métricas
- ?? Planea updates

---

## ?? **PROBLEMAS COMUNES Y SOLUCIONES:**

### **Error: "Package validation failed"**

**Causa:** Package corrupto o mal formado

**Solución:**
1. Regenera package con `Create-MSIXPackage.ps1`
2. Verifica que termina sin errores
3. Upload nuevo package

### **Error: "Privacy policy URL not accessible"**

**Causa:** GitHub Pages no está activo

**Solución:**
1. Verifica URL en navegador
2. Espera 5 minutos (propagación DNS)
3. Reintenta

### **Error: "Screenshot resolution too low"**

**Causa:** Screenshots < 1366x768

**Solución:**
- Tus screenshots son 1386x893 ?
- No deberías tener este error

### **Warning: "App crashes on launch"**

**Causa:** Dependencias faltantes o error en código

**Solución:**
1. Testea MSIX package localmente primero
2. Ejecuta: `.\Install-Package.ps1`
3. Verifica que lanza sin crashes
4. Si crashea, revisa logs en `%LocalAppData%\OptiScaler\CrashLogs`

---

## ?? **CHECKLIST FINAL PRE-SUBMIT:**

Antes de hacer Submit, verifica:

- [ ] Cuenta Partner Center activa ?
- [ ] Nombre de app reservado ?
- [ ] Product setup completo ?
- [ ] Pricing configurado (Free) ?
- [ ] Properties y age rating ?
- [ ] Store listing completo ?
- [ ] 6 Screenshots uploaded ?
- [ ] MSIX package uploaded ?
- [ ] Privacy Policy URL válida ?
- [ ] Notes for certification escritas ?
- [ ] Todo tiene checkmark verde ?

**Si todos los items tienen ? ? Submit to Store!**

---

## ?? **¡FELICITACIONES!**

Una vez que completes estos 5 pasos:

```
? PASO 1: App Listing creado
? PASO 2: Package uploaded
? PASO 3: Submitted para certificación
? PASO 4: Esperando aprobación (3-7 días)
?? PASO 5: LAUNCH!
```

**Tu app estará en Microsoft Store!** ??

---

## ?? **RECURSOS ÚTILES:**

### **Microsoft Partner Center:**
- Dashboard: https://partner.microsoft.com/dashboard
- Documentación: https://docs.microsoft.com/windows/apps/publish/
- Support: https://developer.microsoft.com/windows/support
- Políticas: https://docs.microsoft.com/windows/uwp/publish/store-policies

### **Tus Documentos:**
- `STORE_DESCRIPTION.md` - Descripción para Store
- `STORE_SUBMISSION_CHECKLIST.md` - Checklist completo
- `MSIX_PACKAGING_GUIDE.md` - Guía de packaging
- `FINAL_SESSION_SUMMARY.md` - Resumen de desarrollo

### **Support:**
- Si tienes problemas durante certificación:
  - Email a Microsoft Store Support
  - O usa el chat en Partner Center

---

## ?? **DESPUÉS DEL LAUNCH:**

### **Próximas Features (v0.2.0):**
Ver `PHASE_PLAN_UPDATED.md` para roadmap completo.

Ideas:
- Cloud sync de settings
- Advanced presets
- Auto-update mods
- Game performance overlay
- More platform support

### **Community Building:**
- GitHub Discussions
- Discord server (opcional)
- Twitter/X announcements
- Reddit posts en r/PCGaming

---

**¡Estás a solo unos clicks de tener tu app en Microsoft Store!** ??

**Siguiente acción:** Ir a https://partner.microsoft.com/dashboard y seguir PASO 1.

---

**Última actualización:** 25 Enero 2025  
**Para:** OptiScaler Manager v0.1.0  
**Cuenta Partner Center:** ? Activa
