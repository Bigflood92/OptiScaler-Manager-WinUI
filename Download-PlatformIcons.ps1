# ?? DESCARGAR ICONOS DE PLATAFORMAS - SCRIPT RÁPIDO

Write-Host "`n?? DESCARGA DE ICONOS DE PLATAFORMAS`n" -ForegroundColor Cyan
Write-Host "====================================`n" -ForegroundColor Cyan

# Crear carpeta de iconos
$iconsDir = Join-Path $PSScriptRoot "OptiScaler.UI\Assets\PlatformIcons"
if (-not (Test-Path $iconsDir)) {
    New-Item -ItemType Directory -Path $iconsDir -Force | Out-Null
    Write-Host "? Carpeta creada: $iconsDir`n" -ForegroundColor Green
} else {
    Write-Host "?? Carpeta ya existe: $iconsDir`n" -ForegroundColor Yellow
}

Write-Host "?? INSTRUCCIONES DE DESCARGA:`n" -ForegroundColor Cyan

Write-Host "Descarga estos 7 iconos PNG (256x256 o más) desde:" -ForegroundColor White
Write-Host ""

# URLs de descarga
$downloads = @(
    @{
        Name = "Steam"
        Color = "Cyan"
        URLs = @(
            "https://www.freepnglogos.com/pics/steam-logo",
            "https://www.flaticon.com/search?word=steam",
            "https://iconscout.com/search?query=steam"
        )
        File = "steam.png"
        Tips = "Busca el logo azul/negro de Steam. Descarga PNG transparente."
    },
    @{
        Name = "Epic Games"
        Color = "White"
        URLs = @(
            "https://www.freepnglogos.com/pics/epic-games",
            "https://www.flaticon.com/search?word=epic%20games",
            "https://iconscout.com/search?query=epic%20games"
        )
        File = "epic.png"
        Tips = "Logo blanco/negro de Epic Games. Descarga PNG transparente."
    },
    @{
        Name = "Xbox"
        Color = "Green"
        URLs = @(
            "https://www.freepnglogos.com/pics/xbox-logo",
            "https://www.flaticon.com/search?word=xbox",
            "https://iconscout.com/search?query=xbox"
        )
        File = "xbox.png"
        Tips = "Logo verde circular de Xbox. Descarga PNG transparente."
    },
    @{
        Name = "GOG"
        Color = "Magenta"
        URLs = @(
            "https://www.freepnglogos.com/pics/gog",
            "https://www.flaticon.com/search?word=gog",
            "https://iconscout.com/search?query=gog%20galaxy"
        )
        File = "gog.png"
        Tips = "Logo morado/violeta de GOG. Descarga PNG transparente."
    },
    @{
        Name = "EA App"
        Color = "Red"
        URLs = @(
            "https://www.freepnglogos.com/pics/ea-logo",
            "https://www.flaticon.com/search?word=electronic%20arts",
            "https://iconscout.com/search?query=ea%20sports"
        )
        File = "ea.png"
        Tips = "Logo rojo de EA. Descarga PNG transparente."
    },
    @{
        Name = "Ubisoft"
        Color = "Blue"
        URLs = @(
            "https://www.freepnglogos.com/pics/ubisoft",
            "https://www.flaticon.com/search?word=ubisoft",
            "https://iconscout.com/search?query=ubisoft"
        )
        File = "ubisoft.png"
        Tips = "Logo azul en espiral de Ubisoft. Descarga PNG transparente."
    },
    @{
        Name = "Manual (Genérico)"
        Color = "Gray"
        URLs = @(
            "https://www.flaticon.com/search?word=game%20controller",
            "https://www.flaticon.com/search?word=folder",
            "https://iconscout.com/search?query=game%20folder"
        )
        File = "manual.png"
        Tips = "Icono de carpeta o gamepad genérico. Descarga PNG transparente."
    }
)

$counter = 1
foreach ($platform in $downloads) {
    Write-Host "$counter. " -NoNewline -ForegroundColor Yellow
    Write-Host "$($platform.Name)" -ForegroundColor $platform.Color
    Write-Host "   Archivo: " -NoNewline -ForegroundColor Gray
    Write-Host "$($platform.File)" -ForegroundColor White
    Write-Host "   Tips: $($platform.Tips)" -ForegroundColor DarkGray
    Write-Host "   URLs:" -ForegroundColor Gray
    foreach ($url in $platform.URLs) {
        Write-Host "      • $url" -ForegroundColor DarkCyan
    }
    Write-Host ""
    $counter++
}

Write-Host "`n?? SITIOS WEB RECOMENDADOS:`n" -ForegroundColor Cyan

$sites = @(
    @{Name="FreePNGLogos"; URL="https://www.freepnglogos.com"; Description="Logos de alta calidad, gratis"},
    @{Name="Flaticon"; URL="https://www.flaticon.com"; Description="Iconos vectoriales y PNG"},
    @{Name="IconScout"; URL="https://iconscout.com"; Description="Iconos profesionales"},
    @{Name="Brands of the World"; URL="https://www.brandsoftheworld.com"; Description="Logos vectoriales oficiales"},
    @{Name="SeekLogo"; URL="https://seeklogo.com"; Description="Base de datos de logos"}
)

foreach ($site in $sites) {
    Write-Host "• " -NoNewline -ForegroundColor Green
    Write-Host "$($site.Name)" -NoNewline -ForegroundColor White
    Write-Host " - $($site.Description)" -ForegroundColor Gray
    Write-Host "  $($site.URL)" -ForegroundColor DarkCyan
}

Write-Host "`n?? ESPECIFICACIONES:`n" -ForegroundColor Cyan
Write-Host "   • Formato: PNG con transparencia" -ForegroundColor White
Write-Host "   • Tamaño mínimo: 128x128 px" -ForegroundColor White
Write-Host "   • Tamaño recomendado: 256x256 px o más" -ForegroundColor White
Write-Host "   • Fondo: Transparente" -ForegroundColor White
Write-Host "   • Calidad: Alta resolución" -ForegroundColor White

Write-Host "`n?? DÓNDE GUARDAR:`n" -ForegroundColor Cyan
Write-Host "   Guarda todos los archivos en:" -ForegroundColor White
Write-Host "   $iconsDir" -ForegroundColor Yellow

Write-Host "`n? NOMBRES DE ARCHIVO EXACTOS:`n" -ForegroundColor Cyan
foreach ($platform in $downloads) {
    Write-Host "   ? $($platform.File)" -ForegroundColor Green
}

Write-Host "`n? MÉTODO MÁS RÁPIDO:`n" -ForegroundColor Yellow
Write-Host "   1. Ir a: https://www.flaticon.com" -ForegroundColor White
Write-Host "   2. Buscar cada plataforma (steam, epic, xbox, etc.)" -ForegroundColor White
Write-Host "   3. Descargar PNG 256x256" -ForegroundColor White
Write-Host "   4. Renombrar según lista de arriba" -ForegroundColor White
Write-Host "   5. Guardar en: OptiScaler.UI\Assets\PlatformIcons\" -ForegroundColor White

Write-Host "`n?? DESPUÉS DE DESCARGAR:`n" -ForegroundColor Cyan
Write-Host "   Ejecuta este comando para verificar:" -ForegroundColor White
Write-Host "   Get-ChildItem '$iconsDir' | Select-Object Name" -ForegroundColor Yellow

Write-Host "`n?? CUANDO TERMINES:" -ForegroundColor Cyan
Write-Host "   Avísame y te doy el código para integrarlos" -ForegroundColor White

Write-Host "`n??  Tiempo estimado: 10-15 minutos" -ForegroundColor Gray
Write-Host ""

# Abrir carpeta en Explorer
$response = Read-Host "`n¿Abrir carpeta de Assets ahora? (s/n)"
if ($response -eq 's' -or $response -eq 'S') {
    New-Item -ItemType Directory -Path $iconsDir -Force | Out-Null
    explorer $iconsDir
    Write-Host "`n? Carpeta abierta en Explorer" -ForegroundColor Green
}

Write-Host "`n?? ¡Buena suerte descargando los iconos!" -ForegroundColor Green
Write-Host ""
