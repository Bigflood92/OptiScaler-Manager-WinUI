# ?? SCREENSHOT HELPER SCRIPT
# Prepara la app y verifica screenshots para Microsoft Store

Write-Host "`n?? SCREENSHOT PREPARATION TOOL" -ForegroundColor Cyan
Write-Host "================================`n" -ForegroundColor Cyan

# 1. Crear carpeta de screenshots
$screenshotsDir = Join-Path $PSScriptRoot "Screenshots"
if (-not (Test-Path $screenshotsDir)) {
    New-Item -ItemType Directory -Path $screenshotsDir -Force | Out-Null
    Write-Host "? Carpeta Screenshots creada: $screenshotsDir" -ForegroundColor Green
} else {
    Write-Host "?? Carpeta Screenshots ya existe" -ForegroundColor Yellow
}

Write-Host "`n?? PREPARACIÓN:" -ForegroundColor Cyan
Write-Host "===============" -ForegroundColor Cyan

# 2. Verificar resolución de pantalla
Add-Type -AssemblyName System.Windows.Forms
$screen = [System.Windows.Forms.Screen]::PrimaryScreen
$width = $screen.Bounds.Width
$height = $screen.Bounds.Height

Write-Host "`n???  Resolución de pantalla: " -NoNewline
Write-Host "$width x $height" -ForegroundColor White

if ($width -ge 1920 -and $height -ge 1080) {
    Write-Host "   ? Resolución soporta Full HD (1920x1080)" -ForegroundColor Green
} else {
    Write-Host "   ??  Resolución menor a 1920x1080 - puede haber problemas" -ForegroundColor Yellow
}

# 3. Verificar si la app está corriendo
$appProcess = Get-Process -Name "OptiScaler.UI" -ErrorAction SilentlyContinue
if ($appProcess) {
    Write-Host "`n?? App en ejecución: " -NoNewline
    Write-Host "SÍ (PID: $($appProcess.Id))" -ForegroundColor Green
} else {
    Write-Host "`n?? App en ejecución: " -NoNewline
    Write-Host "NO" -ForegroundColor Yellow
    Write-Host "   ?? Ejecuta la app con F5 en Visual Studio" -ForegroundColor Gray
}

# 4. Listar screenshots existentes
Write-Host "`n?? SCREENSHOTS CAPTURADOS:" -ForegroundColor Cyan
Write-Host "===========================" -ForegroundColor Cyan

$screenshots = Get-ChildItem -Path $screenshotsDir -Filter "*.png" -ErrorAction SilentlyContinue

if ($screenshots.Count -eq 0) {
    Write-Host "`n   ??  No hay screenshots aún" -ForegroundColor Yellow
    Write-Host "   ?? Captura al menos 4 screenshots (ver SCREENSHOT_GUIDE.md)" -ForegroundColor Gray
} else {
    Write-Host ""
    foreach ($screenshot in $screenshots | Sort-Object Name) {
        $sizeKB = [math]::Round($screenshot.Length / 1KB, 2)
        $sizeMB = [math]::Round($screenshot.Length / 1MB, 2)
        
        # Obtener resolución
        try {
            Add-Type -AssemblyName System.Drawing
            $img = [System.Drawing.Image]::FromFile($screenshot.FullName)
            $resolution = "$($img.Width)x$($img.Height)"
            $img.Dispose()
        } catch {
            $resolution = "Unknown"
        }
        
        # Verificar calidad
        $isValidSize = $screenshot.Length -lt 2MB
        $isValidResolution = $resolution -eq "1920x1080"
        
        if ($isValidSize -and $isValidResolution) {
            $status = "?"
            $color = "Green"
        } elseif ($isValidSize -or $isValidResolution) {
            $status = "??"
            $color = "Yellow"
        } else {
            $status = "?"
            $color = "Red"
        }
        
        Write-Host "   $status " -NoNewline -ForegroundColor $color
        Write-Host "$($screenshot.Name)" -NoNewline -ForegroundColor White
        Write-Host " - $resolution - $sizeKB KB" -ForegroundColor Gray
    }
    
    Write-Host "`n   ?? Total: $($screenshots.Count) screenshots" -ForegroundColor Cyan
}

# 5. Checklist de screenshots requeridos
Write-Host "`n?? CHECKLIST DE SCREENSHOTS:" -ForegroundColor Cyan
Write-Host "=============================" -ForegroundColor Cyan

$requiredScreenshots = @(
    @{ Name = "Screenshot_01_GamesLibrary.png"; Priority = "CRÍTICO"; Description = "Games library con juegos detectados" },
    @{ Name = "Screenshot_02_GameWithMods.png"; Priority = "CRÍTICO"; Description = "Game card con mods instalados" },
    @{ Name = "Screenshot_03_ModConfiguration.png"; Priority = "IMPORTANTE"; Description = "Mod configuration panel" },
    @{ Name = "Screenshot_04_GameSettings.png"; Priority = "IMPORTANTE"; Description = "Game settings dialog" },
    @{ Name = "Screenshot_05_ModsManagement.png"; Priority = "Opcional"; Description = "Mods page" },
    @{ Name = "Screenshot_06_AppSettings.png"; Priority = "Opcional"; Description = "App settings" },
    @{ Name = "Screenshot_07_AdvancedSettings.png"; Priority = "Opcional"; Description = "Advanced configuration" },
    @{ Name = "Screenshot_08_Dashboard.png"; Priority = "Opcional"; Description = "Dashboard overview" }
)

$criticalCount = 0
$importantCount = 0
$optionalCount = 0

Write-Host ""
foreach ($required in $requiredScreenshots) {
    $exists = Test-Path (Join-Path $screenshotsDir $required.Name)
    
    if ($exists) {
        Write-Host "   ? " -NoNewline -ForegroundColor Green
        if ($required.Priority -eq "CRÍTICO") { $criticalCount++ }
        if ($required.Priority -eq "IMPORTANTE") { $importantCount++ }
        if ($required.Priority -eq "Opcional") { $optionalCount++ }
    } else {
        Write-Host "   ? " -NoNewline -ForegroundColor Gray
    }
    
    $priorityColor = switch ($required.Priority) {
        "CRÍTICO" { "Red" }
        "IMPORTANTE" { "Yellow" }
        "Opcional" { "Gray" }
    }
    
    Write-Host "[$($required.Priority)]" -NoNewline -ForegroundColor $priorityColor
    Write-Host " $($required.Name)" -ForegroundColor White
    Write-Host "      $($required.Description)" -ForegroundColor Gray
}

# 6. Resumen de progreso
Write-Host "`n?? PROGRESO:" -ForegroundColor Cyan
Write-Host "============" -ForegroundColor Cyan
Write-Host "   Críticos:   $criticalCount/2" -ForegroundColor $(if ($criticalCount -eq 2) { "Green" } else { "Red" })
Write-Host "   Importantes: $importantCount/2" -ForegroundColor $(if ($importantCount -eq 2) { "Green" } else { "Yellow" })
Write-Host "   Opcionales:  $optionalCount/4" -ForegroundColor Gray

$totalRequired = $criticalCount + $importantCount
$percentage = [math]::Round(($totalRequired / 4) * 100)

Write-Host "`n   Total requeridos: $totalRequired/4 ($percentage%)" -ForegroundColor Cyan

# 7. Instrucciones siguientes
Write-Host "`n?? PRÓXIMOS PASOS:" -ForegroundColor Cyan
Write-Host "==================" -ForegroundColor Cyan

if ($criticalCount -lt 2) {
    Write-Host "`n   1??  Captura screenshots CRÍTICOS primero:" -ForegroundColor Yellow
    Write-Host "       - Screenshot_01_GamesLibrary.png" -ForegroundColor White
    Write-Host "       - Screenshot_02_GameWithMods.png" -ForegroundColor White
    Write-Host "`n   ?? Usa Win + Shift + S para capturar" -ForegroundColor Gray
} elseif ($importantCount -lt 2) {
    Write-Host "`n   2??  Captura screenshots IMPORTANTES:" -ForegroundColor Yellow
    Write-Host "       - Screenshot_03_ModConfiguration.png" -ForegroundColor White
    Write-Host "       - Screenshot_04_GameSettings.png" -ForegroundColor White
} else {
    Write-Host "`n   ? Screenshots críticos e importantes completados!" -ForegroundColor Green
    Write-Host "`n   3??  (Opcional) Captura screenshots adicionales:" -ForegroundColor Gray
    Write-Host "       - Screenshot_05-08 para mostrar más features" -ForegroundColor White
    Write-Host "`n   ? O continúa con testing de navegación!" -ForegroundColor Green
}

# 8. Tips rápidos
Write-Host "`n?? TIPS RÁPIDOS:" -ForegroundColor Cyan
Write-Host "=================" -ForegroundColor Cyan
Write-Host "   • Win + Shift + S - Snipping tool (recomendado)" -ForegroundColor White
Write-Host "   • Captura en resolución 1920x1080" -ForegroundColor White
Write-Host "   • Guarda como PNG en carpeta Screenshots/" -ForegroundColor White
Write-Host "   • Asegúrate de que la UI esté limpia (sin errores)" -ForegroundColor White
Write-Host "   • Muestra datos reales (juegos reales)" -ForegroundColor White

# 9. Comandos útiles
Write-Host "`n?? COMANDOS ÚTILES:" -ForegroundColor Cyan
Write-Host "===================" -ForegroundColor Cyan
Write-Host "   • Abrir carpeta:" -ForegroundColor White
Write-Host "     explorer '$screenshotsDir'" -ForegroundColor Gray
Write-Host "`n   • Re-ejecutar este script:" -ForegroundColor White
Write-Host "     .\Screenshot-Helper.ps1" -ForegroundColor Gray
Write-Host "`n   • Commit screenshots:" -ForegroundColor White
Write-Host "     git add Screenshots/" -ForegroundColor Gray
Write-Host "     git commit -m 'Add Microsoft Store screenshots'" -ForegroundColor Gray

Write-Host "`n? ¡Buena suerte con los screenshots!" -ForegroundColor Green
Write-Host ""

# Opción para abrir carpeta
$response = Read-Host "`n¿Abrir carpeta Screenshots? (s/n)"
if ($response -eq 's' -or $response -eq 'S') {
    explorer $screenshotsDir
}
