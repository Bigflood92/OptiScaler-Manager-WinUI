# ?? DESCARGA RÁPIDA DE ICONOS - GUÍA VISUAL

## ? MÉTODO MÁS RÁPIDO (10-15 min):

### **1. IR A FLATICON** (Recomendado)
?? https://www.flaticon.com

---

## ?? LISTA DE DESCARGAS:

### ? **1. STEAM** (Logo azul)
- ?? Buscar en Flaticon: **"steam logo"**
- ?? Descargar PNG 256x256
- ?? Guardar como: `steam.png`
- ?? Color: Azul/Negro/Blanco

### ? **2. EPIC GAMES** (Logo blanco/negro)
- ?? Buscar: **"epic games logo"**
- ?? Descargar PNG 256x256
- ?? Guardar como: `epic.png`
- ?? Color: Blanco/Negro

### ? **3. XBOX** (Logo verde circular)
- ?? Buscar: **"xbox logo"**
- ?? Descargar PNG 256x256
- ?? Guardar como: `xbox.png`
- ?? Color: Verde

### ? **4. GOG** (Logo morado)
- ?? Buscar: **"gog galaxy logo"**
- ?? Descargar PNG 256x256
- ?? Guardar como: `gog.png`
- ?? Color: Morado/Violeta

### ? **5. EA** (Logo rojo)
- ?? Buscar: **"ea sports logo"** o **"electronic arts"**
- ?? Descargar PNG 256x256
- ?? Guardar como: `ea.png`
- ?? Color: Rojo

### ? **6. UBISOFT** (Logo azul en espiral)
- ?? Buscar: **"ubisoft logo"**
- ?? Descargar PNG 256x256
- ?? Guardar como: `ubisoft.png`
- ?? Color: Azul

### ? **7. MANUAL** (Icono genérico)
- ?? Buscar: **"game controller"** o **"folder icon"**
- ?? Descargar PNG 256x256
- ?? Guardar como: `manual.png`
- ?? Color: Gris/Blanco

---

## ?? **GUARDAR EN:**

```
C:\Users\Jorge\OneDrive\OptiScaler Manager\OptiScaler.UI\Assets\PlatformIcons\
```

**Estructura final:**
```
OptiScaler.UI\Assets\PlatformIcons\
??? steam.png
??? epic.png
??? xbox.png
??? gog.png
??? ea.png
??? ubisoft.png
??? manual.png
```

---

## ?? **SITIOS ALTERNATIVOS:**

### **Si no encuentras en Flaticon:**

1. **FreePNGLogos** - https://www.freepnglogos.com
   - Buscar cada plataforma
   - Descargar PNG transparente

2. **IconScout** - https://iconscout.com
   - Buscar por nombre
   - Descargar PNG 256x256

3. **Brands of the World** - https://www.brandsoftheworld.com
   - Logos vectoriales oficiales
   - Convertir SVG ? PNG si es necesario

4. **Google Images** - Búsqueda avanzada
   - Buscar: "[platform] logo PNG transparent"
   - Filtrar: Tamaño > Mediano
   - Filtrar: Color > Transparente

---

## ? **CHECKLIST DE CALIDAD:**

Antes de guardar cada icono, verifica:

- [ ] **Formato:** PNG (no JPG)
- [ ] **Fondo:** Transparente (no blanco)
- [ ] **Tamaño:** Mínimo 128x128, ideal 256x256
- [ ] **Calidad:** Alta resolución, no pixelado
- [ ] **Nombre:** Exacto según lista (steam.png, epic.png, etc.)

---

## ?? **PASOS DETALLADOS:**

### **EJEMPLO: Descargar Steam Icon**

1. **Ir a Flaticon:**
   ```
   https://www.flaticon.com
   ```

2. **Buscar:**
   ```
   steam logo
   ```

3. **Filtrar:**
   - Click en icono que te guste
   - Seleccionar tamaño: 256 o 512
   - Formato: PNG

4. **Descargar:**
   - Click "Download PNG"
   - Guardar en carpeta PlatformIcons/

5. **Renombrar:**
   - De: "steam-icon-12345.png"
   - A: "steam.png"

6. **Repetir para las otras 6 plataformas**

---

## ?? **TIEMPO ESTIMADO:**

| Tarea | Tiempo |
|-------|--------|
| Crear carpeta PlatformIcons | 1 min |
| Descargar 7 iconos | 8-10 min |
| Renombrar archivos | 1-2 min |
| **TOTAL** | **10-15 min** |

---

## ?? **DESPUÉS DE DESCARGAR:**

### **Verificar archivos:**
```powershell
# Ejecutar en PowerShell:
Get-ChildItem "OptiScaler.UI\Assets\PlatformIcons\" | Select-Object Name, Length
```

### **Debe mostrar:**
```
Name         Length
----         ------
steam.png    [tamaño]
epic.png     [tamaño]
xbox.png     [tamaño]
gog.png      [tamaño]
ea.png       [tamaño]
ubisoft.png  [tamaño]
manual.png   [tamaño]
```

---

## ?? **CUANDO TERMINES:**

**Avísame** diciendo: "Iconos descargados y listos"

Entonces te daré:
1. ? Código para integrarlos en la app
2. ? Converter para usarlos dinámicamente
3. ? Cambios en GameCard XAML

---

## ?? **TIPS:**

- ? **Prioriza iconos con fondo transparente**
- ? **Busca versiones "flat" o "modern"**
- ? **Evita logos con mucho detalle (se ven mal pequeños)**
- ? **Si encuentras SVG, mejor (convierte después)**
- ? **Los iconos monocromáticos (blanco) son mejores para dark theme**

---

## ? **COMENZAR AHORA:**

```powershell
# Ejecuta el helper script:
.\Download-PlatformIcons.ps1

# O simplemente:
# 1. Abre https://www.flaticon.com
# 2. Descarga los 7 iconos
# 3. Guarda en OptiScaler.UI\Assets\PlatformIcons\
```

**¡Empieza la descarga! Te espero cuando termines** ??
