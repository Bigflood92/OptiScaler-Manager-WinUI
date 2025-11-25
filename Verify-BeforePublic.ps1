################################################################################
# VERIFICAR REPO ANTES DE HACERLO PÚBLICO
################################################################################

Write-Host "`n?? VERIFICANDO REPO PARA HACERLO PÚBLICO`n" -ForegroundColor Cyan
Write-Host "=========================================`n" -ForegroundColor Cyan

# 1. Buscar archivos sensibles
Write-Host "?? Buscando archivos sensibles..." -ForegroundColor Yellow
$sensitive = @()
$patterns = @("*.pfx", "*.p12", "*.key", "secrets.json", ".env")

foreach ($pattern in $patterns) {
    $files = Get-ChildItem -Recurse -Filter $pattern -ErrorAction SilentlyContinue |
        Where-Object { -not $_.FullName.Contains(".git") }
    $sensitive += $files
}

if ($sensitive.Count -gt 0) {
    Write-Host "   ??  ARCHIVOS SENSIBLES ENCONTRADOS:" -ForegroundColor Red
    foreach ($file in $sensitive) {
        Write-Host "      - $($file.Name) ($($file.DirectoryName))" -ForegroundColor Yellow
    }
    Write-Host "`n   ?? Estos archivos están en .gitignore y NO se subirán`n" -ForegroundColor Cyan
} else {
    Write-Host "   ? No hay archivos sensibles`n" -ForegroundColor Green
}

# 2. Limpiar carpetas build
Write-Host "?? Limpiando carpetas de build..." -ForegroundColor Yellow
$cleaned = 0
$folders = @("bin", "obj", "AppPackages")

foreach ($folder in $folders) {
    Get-ChildItem -Recurse -Directory -Filter $folder -ErrorAction SilentlyContinue |
        Where-Object { -not $_.FullName.Contains(".git") } |
        ForEach-Object {
            Remove-Item $_.FullName -Recurse -Force -ErrorAction SilentlyContinue
            $cleaned++
        }
}

Write-Host "   ? $cleaned carpetas eliminadas`n" -ForegroundColor Green

# 3. Verificar tamaño
Write-Host "?? Calculando tamaño del repo..." -ForegroundColor Yellow
$size = (Get-ChildItem -Recurse -File -ErrorAction SilentlyContinue |
    Where-Object { -not $_.FullName.Contains(".git") } |
    Measure-Object -Property Length -Sum).Sum / 1MB

Write-Host "   ?? Tamaño: $([math]::Round($size, 2)) MB" -ForegroundColor White
if ($size -gt 100) {
    Write-Host "   ??  Repo grande (>100MB)`n" -ForegroundColor Yellow
} else {
    Write-Host "   ? Tamaño OK`n" -ForegroundColor Green
}

# 4. Git status
Write-Host "?? Estado de Git..." -ForegroundColor Yellow
git add .gitignore | Out-Null
$status = git status --short
if ($status) {
    Write-Host "$status`n" -ForegroundColor Gray
}

# Summary
Write-Host "???????????????????????????????????????????" -ForegroundColor Cyan
Write-Host "? REPO LISTO PARA HACERLO PÚBLICO" -ForegroundColor Green
Write-Host "???????????????????????????????????????????`n" -ForegroundColor Cyan

Write-Host "?? PRÓXIMOS PASOS:`n" -ForegroundColor Cyan
Write-Host "1. Commit .gitignore actualizado:" -ForegroundColor White
Write-Host "   git commit -m ""Update .gitignore for public repo""" -ForegroundColor Gray
Write-Host "   git push`n" -ForegroundColor Gray

Write-Host "2. Hacer repo público:" -ForegroundColor White
Write-Host "   ? https://github.com/Bigflood92/OptiScaler-Manager-WinUI/settings" -ForegroundColor Cyan
Write-Host "   ? Danger Zone > Change visibility > Make public`n" -ForegroundColor Gray

Write-Host "3. Activar GitHub Pages:" -ForegroundColor White
Write-Host "   ? https://github.com/Bigflood92/OptiScaler-Manager-WinUI/settings/pages" -ForegroundColor Cyan
Write-Host "   ? Source: master, Folder: /docs, Save`n" -ForegroundColor Gray

Write-Host "?? PROTEGIDO: Certificados, secrets, builds" -ForegroundColor Green
Write-Host ""
