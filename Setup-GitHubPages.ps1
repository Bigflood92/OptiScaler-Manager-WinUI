################################################################################
# SETUP GITHUB PAGES - PRIVACY POLICY MIGRATION
# Migra Privacy Policy de Netlify a GitHub Pages (gratis e ilimitado)
################################################################################

Write-Host "`n?? GITHUB PAGES SETUP`n" -ForegroundColor Cyan
Write-Host "=====================`n" -ForegroundColor Cyan

# Step 1: Verificar que existe PrivacyPolicy.html
Write-Host "?? Step 1: Verificando Privacy Policy..." -ForegroundColor Yellow
$privacyPolicyPath = "OptiScaler.UI\PrivacyPolicy.html"
if (-not (Test-Path $privacyPolicyPath)) {
    Write-Host "   ? PrivacyPolicy.html no encontrado!" -ForegroundColor Red
    Write-Host "   ?? Buscando en raíz..." -ForegroundColor Yellow
    if (Test-Path "PrivacyPolicy.html") {
        $privacyPolicyPath = "PrivacyPolicy.html"
    } else {
        Write-Host "   ? No se encontró PrivacyPolicy.html" -ForegroundColor Red
        exit 1
    }
}
Write-Host "   ? Privacy Policy encontrado: $privacyPolicyPath`n" -ForegroundColor Green

# Step 2: Crear carpeta docs/
Write-Host "?? Step 2: Creando carpeta docs/..." -ForegroundColor Yellow
New-Item -ItemType Directory -Path "docs" -Force | Out-Null
Write-Host "   ? Carpeta docs/ creada`n" -ForegroundColor Green

# Step 3: Copiar Privacy Policy
Write-Host "?? Step 3: Copiando Privacy Policy a docs/..." -ForegroundColor Yellow
Copy-Item -Path $privacyPolicyPath -Destination "docs\PrivacyPolicy.html" -Force
Write-Host "   ? Privacy Policy copiado`n" -ForegroundColor Green

# Step 4: Crear index.html redirect (opcional)
Write-Host "?? Step 4: Creando index.html redirect..." -ForegroundColor Yellow
$indexContent = @"
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta http-equiv="refresh" content="0;url=PrivacyPolicy.html">
    <title>Redirecting...</title>
</head>
<body>
    <p>Redirecting to <a href="PrivacyPolicy.html">Privacy Policy</a>...</p>
</body>
</html>
"@
$indexContent | Out-File -FilePath "docs\index.html" -Encoding UTF8 -Force
Write-Host "   ? index.html creado`n" -ForegroundColor Green

# Step 5: Verificar Git status
Write-Host "?? Step 5: Verificando cambios..." -ForegroundColor Yellow
git status --short docs/
Write-Host "   ? Cambios listos para commit`n" -ForegroundColor Green

# Step 6: Commit cambios
Write-Host "?? Step 6: Commiteando a Git..." -ForegroundColor Yellow
git add docs/
git commit -m "Add Privacy Policy for GitHub Pages hosting

- Migrated from Netlify (bandwidth limit reached)
- GitHub Pages provides unlimited free hosting
- New URL: https://bigflood92.github.io/OptiScaler-Manager-WinUI/PrivacyPolicy.html"

if ($LASTEXITCODE -ne 0) {
    Write-Host "   ??  No hay cambios para commit o ya está commiteado" -ForegroundColor Yellow
} else {
    Write-Host "   ? Commit exitoso`n" -ForegroundColor Green
}

# Step 7: Push a GitHub
Write-Host "?? Step 7: Pusheando a GitHub..." -ForegroundColor Yellow
git push
if ($LASTEXITCODE -eq 0) {
    Write-Host "   ? Push exitoso`n" -ForegroundColor Green
} else {
    Write-Host "   ? Error al pushear!" -ForegroundColor Red
    exit 1
}

# Summary
Write-Host "`n" -NoNewline
Write-Host "???????????????????????????????????????????" -ForegroundColor Cyan
Write-Host "? GITHUB PAGES CONFIGURADO!" -ForegroundColor Green
Write-Host "???????????????????????????????????????????`n" -ForegroundColor Cyan

Write-Host "?? PRÓXIMOS PASOS (MANUAL):" -ForegroundColor Yellow
Write-Host "`n1??  Activar GitHub Pages:" -ForegroundColor White
Write-Host "   • Ir a: https://github.com/Bigflood92/OptiScaler-Manager-WinUI/settings/pages" -ForegroundColor Gray
Write-Host "   • Source: Deploy from a branch" -ForegroundColor Gray
Write-Host "   • Branch: master" -ForegroundColor Gray
Write-Host "   • Folder: /docs" -ForegroundColor Gray
Write-Host "   • Click 'Save'`n" -ForegroundColor Gray

Write-Host "2??  Esperar deployment (1-2 minutos):" -ForegroundColor White
Write-Host "   • GitHub procesará los archivos" -ForegroundColor Gray
Write-Host "   • Recibirás email de confirmación`n" -ForegroundColor Gray

Write-Host "3??  Verificar Privacy Policy:" -ForegroundColor White
Write-Host "   • URL: https://bigflood92.github.io/OptiScaler-Manager-WinUI/PrivacyPolicy.html" -ForegroundColor Cyan
Write-Host "   • Abre en navegador para verificar`n" -ForegroundColor Gray

Write-Host "4??  Actualizar Package.appxmanifest:" -ForegroundColor White
Write-Host "   • Abrir: OptiScaler.UI\Package.appxmanifest" -ForegroundColor Gray
Write-Host "   • Cambiar PrivacyPolicy URL a:" -ForegroundColor Gray
Write-Host "     https://bigflood92.github.io/OptiScaler-Manager-WinUI/PrivacyPolicy.html" -ForegroundColor Cyan
Write-Host ""

Write-Host "?? VENTAJAS DE GITHUB PAGES:" -ForegroundColor Yellow
Write-Host "   ? Bandwidth ILIMITADO (vs 100GB en Netlify)" -ForegroundColor Green
Write-Host "   ? 100% GRATUITO para siempre" -ForegroundColor Green
Write-Host "   ? CDN global de GitHub" -ForegroundColor Green
Write-Host "   ? HTTPS automático" -ForegroundColor Green
Write-Host "   ? Sin pausas ni límites" -ForegroundColor Green
Write-Host ""

Write-Host "?? Ver: NETLIFY_PAUSED_SOLUTION.md para más detalles" -ForegroundColor Gray
Write-Host ""
