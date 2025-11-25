# ?? CÓDIGO PARA INTEGRAR ICONOS - PREPARADO

## ? **ESTE CÓDIGO SE APLICARÁ CUANDO TERMINES DE DESCARGAR**

---

## ?? **CAMBIOS QUE HARÉ:**

### **1. Actualizar OptiScaler.UI.csproj**
Añadir los iconos como Content files para que se copien al output.

### **2. Crear PlatformIconConverter**
Converter personalizado que mapea cada plataforma a su imagen PNG.

### **3. Actualizar GamesPage.xaml**
Cambiar FontIcon por Image con los logos reales.

---

## ?? **ARCHIVOS QUE MODIFICARÉ:**

```
? OptiScaler.UI/OptiScaler.UI.csproj
   ? Añadir <Content Include> para iconos

? OptiScaler.UI/Helpers/PlatformIconConverter.cs (nuevo)
   ? Converter que devuelve path de imagen según plataforma

? OptiScaler.UI/Views/GamesPage.xaml
   ? Cambiar FontIcon ? Image con binding a converter
```

---

## ?? **CÓDIGO PREPARADO:**

### **1. OptiScaler.UI.csproj** (ItemGroup para iconos)

```xml
<ItemGroup>
    <Content Include="Assets\PlatformIcons\*.png">
        <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
</ItemGroup>
```

---

### **2. PlatformIconConverter.cs** (nuevo archivo)

```csharp
using System;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;
using OptiScaler.Core.Models;

namespace OptiScaler.UI.Helpers;

public class PlatformIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is GamePlatform platform)
        {
            var iconName = platform switch
            {
                GamePlatform.Steam => "steam.png",
                GamePlatform.Epic => "epic.png",
                GamePlatform.Xbox => "xbox.png",
                GamePlatform.GOG => "gog.png",
                GamePlatform.EA => "ea.png",
                GamePlatform.Ubisoft => "ubisoft.png",
                GamePlatform.Manual => "manual.png",
                _ => "manual.png"
            };

            var uri = new Uri($"ms-appx:///Assets/PlatformIcons/{iconName}");
            return new BitmapImage(uri);
        }

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
```

---

### **3. GamesPage.xaml** (cambio en Platform Badge)

**ANTES:**
```xml
<FontIcon 
    Glyph="{x:Bind Platform, Converter={StaticResource GamePlatformToIconConverter}}"
    FontSize="14"
    Foreground="White"/>
```

**DESPUÉS:**
```xml
<Image 
    Source="{x:Bind Platform, Converter={StaticResource PlatformIconConverter}}"
    Width="20"
    Height="20"
    Stretch="Uniform"/>
```

---

### **4. GamesPage.xaml** (añadir Converter a Resources)

```xml
<Page.Resources>
    <!-- Converters -->
    <helpers:BoolToVisibilityConverter x:Key="BoolToVisibilityConverter"/>
    <helpers:PlatformIconConverter x:Key="PlatformIconConverter"/> <!-- NUEVO -->
    <!-- ...otros converters... -->
</Page.Resources>
```

---

## ? **APLICACIÓN AUTOMÁTICA:**

Cuando me digas **"Iconos descargados"**, ejecutaré:

```powershell
# 1. Verificar que existen los 7 archivos
# 2. Aplicar cambios en .csproj
# 3. Crear PlatformIconConverter.cs
# 4. Actualizar GamesPage.xaml
# 5. Rebuild del proyecto
# 6. Commit a Git
```

---

## ? **RESULTADO ESPERADO:**

### **Antes:**
```
???????????????????
? Game Name       ?
? ?? Steam        ? ? FontIcon genérico
???????????????????
```

### **Después:**
```
???????????????????
? Game Name       ?
? [LOGO] Steam    ? ? Logo real de Steam
???????????????????
```

---

## ?? **IMPACTO EN SCREENSHOTS:**

Los screenshots se verán **MUCHO más profesionales** con:
- ? Logo real de Steam (azul/negro)
- ? Logo real de Epic Games (blanco)
- ? Logo real de Xbox (verde circular)
- ? Logo real de GOG (morado)
- ? Logo real de EA (rojo)
- ? Logo real de Ubisoft (azul en espiral)
- ? Icono genérico para juegos manuales

---

## ?? **TIEMPO DE APLICACIÓN:**

- Verificar iconos: 1 min
- Aplicar código: 2 min
- Build y test: 2 min
- **Total: ~5 minutos**

---

## ?? **CUANDO TERMINES:**

Simplemente escribe:
```
"Iconos descargados y listos"
```

Y yo aplicaré todo automáticamente! ??

---

**Mientras tanto, descarga tranquilo los 7 iconos siguiendo la guía.**
