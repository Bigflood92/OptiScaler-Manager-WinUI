# ?? CAPTURA DE SCREENSHOTS - GUÍA PASO A PASO

## ? **LA APP YA ESTÁ CORRIENDO** (PID: 31964)

---

## ?? **CAPTURA SCREENSHOT 1: GAMES LIBRARY** ? CRÍTICO

### **QUÉ CAPTURAR:**
La página principal con la lista de juegos.

### **PASOS:**
1. **Asegúrate de estar en la pestaña "Games"** (sidebar izquierdo)
2. **Verifica que haya juegos visibles:**
   - Si NO hay juegos ? Click "Scan Games" y espera
   - Si SÍ hay juegos ? Continúa
3. **Posiciona la ventana:**
   - Centra la ventana en la pantalla
   - Asegúrate de que se vean al menos 4-6 game cards
4. **Captura:**
   - Presiona: `Win + Shift + S`
   - Selecciona "Rectangle Snip"
   - Dibuja un rectángulo alrededor del CONTENIDO (sin barra de Windows)
   - Click en la notificación que aparece
   - Ctrl + V en Paint
   - Guardar como: `Screenshots\Screenshot_01_GamesLibrary.png`

### **VERIFICA:**
- ? Se ven múltiples juegos
- ? Logos de plataformas visibles (Steam, Xbox, Epic, etc.)
- ? Search bar visible arriba
- ? Hero header con "Games Library"
- ? Algunos juegos con badges verdes (si tienes mods instalados)

---

## ?? **CAPTURA SCREENSHOT 2: GAME WITH MODS** ? CRÍTICO

### **PREPARACIÓN:**
Si NO tienes juegos con mods instalados:
1. Scroll en la lista de juegos
2. Click "Install Mods" en 1-2 juegos
3. Espera a que termine la instalación (30-60 seg)
4. Verás badges verdes aparecer

### **QUÉ CAPTURAR:**
Un game card que tenga mods instalados.

### **PASOS:**
1. **Scroll** hasta encontrar un juego con badges verdes
2. **Asegúrate de que se vea:**
   - ? OptiScaler badge verde
   - ? Configuration details (Upscaler, Quality, Frame Gen)
   - ? Botones: Launch Game, Details, Uninstall
3. **Captura:**
   - `Win + Shift + S`
   - Selecciona solo ESE game card (o 2-3 cards con mods)
   - Guardar como: `Screenshots\Screenshot_02_GameWithMods.png`

### **VERIFICA:**
- ? Se ven badges verdes de mods instalados
- ? Configuration details visibles
- ? Logos de plataforma visibles
- ? Botones de acción visibles

---

## ?? **CAPTURA SCREENSHOT 3: MOD CONFIGURATION** ? IMPORTANTE

### **QUÉ CAPTURAR:**
La página de configuración global de mods.

### **PASOS:**
1. **Click en "Mod Config"** en el sidebar izquierdo
2. **Verifica que se vea:**
   - Preset buttons (Auto, Performance, Balanced, Quality)
   - Upscaler dropdown
   - Quality dropdown
   - Frame Generation toggle
   - GPU information (si tienes)
3. **Captura:**
   - `Win + Shift + S`
   - Captura toda la página de configuración
   - Guardar como: `Screenshots\Screenshot_03_ModConfiguration.png`

### **VERIFICA:**
- ? Preset buttons visibles
- ? Configuración de upscaler visible
- ? Toggles y dropdowns visibles
- ? Dark theme se ve bien

---

## ?? **CAPTURA SCREENSHOT 4: GAME SETTINGS DIALOG** ? IMPORTANTE

### **QUÉ CAPTURAR:**
El dialog de configuración per-game.

### **PASOS:**
1. **Vuelve a "Games"** en el sidebar
2. **Click en el icono ??** (Settings) en cualquier game card con mods
3. **El dialog se abre**
4. **Asegúrate de estar en la pestaña "General"**
5. **Captura:**
   - `Win + Shift + S`
   - Captura todo el dialog
   - Guardar como: `Screenshots\Screenshot_04_GameSettings.png`

### **VERIFICA:**
- ? Dialog centrado y completo
- ? Tabs visibles (General/Presets/Advanced)
- ? Opciones de configuración visibles
- ? Botones Apply/Cancel visibles

---

## ? **DESPUÉS DE CAPTURAR LOS 4 SCREENSHOTS:**

### **Verifica los archivos:**
```powershell
.\Screenshot-Helper.ps1
```

**Debe mostrar:**
```
Críticos:    2/2 ?
Importantes: 2/2 ?
Total: 4/4 (100%) ?
```

### **Commit a Git:**
```powershell
git add Screenshots/
git commit -m "Add Microsoft Store screenshots (1920x1080)"
git push
```

---

## ?? **TIPS DE CAPTURA:**

### **DO:**
- ? Captura el CONTENIDO interno (no bordes de Windows)
- ? Asegúrate de que NO hay errores visibles
- ? Verifica que TODO el texto sea legible
- ? Muestra datos REALES (juegos reales)
- ? Usa resolución Full HD o cercana

### **DON'T:**
- ? No captures la barra de título de Windows
- ? No captures con errores o InfoBars rojos
- ? No captures con lista vacía
- ? No captures en resoluciones bajas

---

## ?? **SI TIENES PROBLEMAS:**

### **"No veo juegos en la lista"**
? Click "Scan Games" y espera 30-60 segundos

### **"No tengo mods instalados"**
? Click "Install Mods" en 1-2 juegos y espera

### **"La captura se ve mal"**
? Asegúrate de capturar en modo "Rectangle Snip", no "Window Snip"

### **"Los logos no se ven"**
? Reinicia la app (Shift+F5 ? F5) para recargar assets

---

## ?? **TIEMPO ESTIMADO:**

- Screenshot 1: 2-3 min
- Screenshot 2: 2-3 min
- Screenshot 3: 2 min
- Screenshot 4: 3 min
- Verificación: 2 min

**Total: 15-20 minutos**

---

## ?? **¡EMPIEZA AHORA!**

**Paso 1:** La app YA está corriendo ? Ve a la ventana  
**Paso 2:** Sigue la guía desde "Screenshot 1"  
**Paso 3:** Captura con `Win + Shift + S`  
**Paso 4:** Guarda en `Screenshots/`  

**¡Adelante! ??**
