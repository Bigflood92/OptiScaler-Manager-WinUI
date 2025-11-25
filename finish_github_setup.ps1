# Post-Push Setup - Activar GitHub Pages y actualizar Manifest
# Ejecutar DESPUÉS de hacer push exitoso

Write-Host ""
Write-Host "?? Git push completado!" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Gray
Write-Host ""

Write-Host "?? PASO FINAL: Activar GitHub Pages" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Gray
Write-Host ""

Write-Host "1. Ve a tu repositorio en GitHub:" -ForegroundColor White
Write-Host "   https://github.com/Bigflood92/OptiScaler-Manager-WinUI" -ForegroundColor Cyan
Write-Host ""

Write-Host "2. Click en 'Settings' (pestaña superior)" -ForegroundColor White
Write-Host ""

Write-Host "3. En el menú izquierdo, busca y click en 'Pages'" -ForegroundColor White
Write-Host ""

Write-Host "4. En 'Build and deployment':" -ForegroundColor White
Write-Host "   Source: Deploy from a branch" -ForegroundColor Yellow
Write-Host "   Branch: master" -ForegroundColor Yellow
Write-Host "   Folder: /docs" -ForegroundColor Yellow
Write-Host ""

Write-Host "5. Click 'Save'" -ForegroundColor White
Write-Host ""

Write-Host "6. Espera 1-2 minutos..." -ForegroundColor Gray
Write-Host ""

Write-Host "7. Verifica que funcione:" -ForegroundColor White
Write-Host "   https://bigflood92.github.io/OptiScaler-Manager-WinUI/" -ForegroundColor Cyan
Write-Host "   https://bigflood92.github.io/OptiScaler-Manager-WinUI/PrivacyPolicy.html" -ForegroundColor Cyan
Write-Host ""

Write-Host "================================================" -ForegroundColor Gray
Write-Host ""

$pagesReady = Read-Host "¿GitHub Pages está funcionando? (y/n)"

if ($pagesReady -eq "y") {
    Write-Host ""
    Write-Host "? Perfecto! Ahora actualizando Package.appxmanifest..." -ForegroundColor Green
    
    $manifestPath = "OptiScaler.UI\Package.appxmanifest"
    $content = Get-Content $manifestPath -Raw
    
    # Actualizar URL si aún tiene placeholder
    if ($content -match "bigflood92.github.io/OptiScaler-Manager/") {
        $content = $content -replace "bigflood92.github.io/OptiScaler-Manager/", "bigflood92.github.io/OptiScaler-Manager-WinUI/"
        Set-Content -Path $manifestPath -Value $content -NoNewline
        
        Write-Host "   ? Manifest actualizado con URL correcta" -ForegroundColor Green
        Write-Host ""
        
        # Commit cambio
        git add $manifestPath
        git commit -m "Update privacy policy URL to GitHub Pages"
        git push
        
        Write-Host "   ? Cambio commiteado y pusheado" -ForegroundColor Green
    } else {
        Write-Host "   ??  URL ya está correcta en manifest" -ForegroundColor Cyan
    }
    
    Write-Host ""
    Write-Host "================================================" -ForegroundColor Gray
    Write-Host "?? TODO LISTO!" -ForegroundColor Green
    Write-Host "================================================" -ForegroundColor Gray
    Write-Host ""
    Write-Host "? Repo privado creado" -ForegroundColor Green
    Write-Host "? Código subido a GitHub (privado)" -ForegroundColor Green
    Write-Host "? GitHub Pages activado" -ForegroundColor Green
    Write-Host "? Privacy Policy URL funcionando" -ForegroundColor Green
    Write-Host "? Manifest actualizado" -ForegroundColor Green
    Write-Host ""
    Write-Host "?? SIGUIENTE:" -ForegroundColor Cyan
    Write-Host "   1. Optimizar PNG assets (TinyPNG.com)" -ForegroundColor White
    Write-Host "   2. Capturar screenshots" -ForegroundColor White
    Write-Host "   3. Testing navegación" -ForegroundColor White
    Write-Host "   4. Submit a Microsoft Store!" -ForegroundColor White
    Write-Host ""
    
} else {
    Write-Host ""
    Write-Host "??  Esperando a que GitHub Pages esté listo..." -ForegroundColor Yellow
    Write-Host "   Ejecuta este script nuevamente cuando esté funcionando." -ForegroundColor Gray
    Write-Host ""
}
