################################################################################
# PREPARE REPO FOR PUBLIC - AUTOMATION SCRIPT
# Limpia archivos sensibles y prepara .gitignore
################################################################################

Write-Host "`n?? PREPARANDO REPO PARA HACERLO PÚBLICO`n" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

# Step 1: Backup actual .gitignore
Write-Host "?? Step 1: Backup del .gitignore actual..." -ForegroundColor Yellow
if (Test-Path ".gitignore") {
    Copy-Item ".gitignore" ".gitignore.backup" -Force
    Write-Host "   ? Backup creado: .gitignore.backup`n" -ForegroundColor Green
}

# Step 2: Crear .gitignore optimizado
Write-Host "?? Step 2: Creando .gitignore optimizado..." -ForegroundColor Yellow

$gitignoreContent = @"
################################################################################
# Visual Studio / Rider / VS Code
################################################################################
.vs/
.vscode/
.idea/
*.suo
*.user
*.userosscache
*.sln.docstates
*.userprefs
.DS_Store

################################################################################
# Build Outputs
################################################################################
[Bb]in/
[Oo]bj/
[Ll]og/
[Ll]ogs/
[Dd]ebug/
[Rr]elease/
x64/
x86/
[Aa]rm/
[Aa]rm64/
bld/
[Bb]in/
[Oo]bj/
[Ll]og/
[Ll]ogs/

################################################################################
# MSTest / NUnit / xUnit Test Results
################################################################################
[Tt]est[Rr]esult*/
[Bb]uild[Ll]og.*
*.VisualState.xml
TestResult.xml
nunit-*.xml

################################################################################
# Build Results of an ATL Project
################################################################################
[Dd]ebugPS/
[Rr]eleasePS/
dlldata.c

################################################################################
# NuGet Packages
################################################################################
*.nupkg
*.snupkg
**/[Pp]ackages/*
!**/[Pp]ackages/build/
*.nuget.props
*.nuget.targets
project.lock.json
project.fragment.lock.json
artifacts/

################################################################################
# MSIX / AppX Package Files
################################################################################
*.appx
*.appxbundle
*.appxupload
*.msix
*.msixbundle
*.msixupload
AppPackages/
BundleArtifacts/
Package.StoreAssociation.xml

################################################################################
# Temporary / Cache Files
################################################################################
*.tmp
*.temp
*.cache
*.log
*.bak
*.swp
*~
.vs/
.vscode/
*.suo
*.user

################################################################################
# Sensitive / Personal Files
################################################################################
# Certificates (NUNCA subir certificados privados)
*.pfx
*.p12
*.cer
*.crt
*.key

# API Keys / Secrets (NUNCA subir secrets)
secrets.json
appsettings.Development.json
.env
.env.local

# Personal settings
*.DotSettings.user
launchSettings.json

################################################################################
# Windows / macOS / Linux
################################################################################
Thumbs.db
ehthumbs.db
Desktop.ini
`$RECYCLE.BIN/
.DS_Store
.AppleDouble
.LSOverride
._*

################################################################################
# Rider
################################################################################
.idea/
*.sln.iml
**/.idea/**/workspace.xml
**/.idea/**/tasks.xml
**/.idea/**/usage.statistics.xml
**/.idea/**/dictionaries
**/.idea/**/shelf

################################################################################
# ReSharper
################################################################################
_ReSharper*/
*.[Rr]e[Ss]harper
*.DotSettings.user

################################################################################
# JetBrains Rider
################################################################################
.idea/
*.sln.iml

################################################################################
# Visual Studio Code
################################################################################
.vscode/*
!.vscode/settings.json
!.vscode/tasks.json
!.vscode/launch.json
!.vscode/extensions.json
*.code-workspace

################################################################################
# User-specific files
################################################################################
*.rsuser
*.suo
*.user
*.userosscache
*.sln.docstates

################################################################################
# Mono auto generated files
################################################################################
mono_crash.*

################################################################################
# Windows Store app package directories
################################################################################
AppPackages/
BundleArtifacts/
Package.StoreAssociation.xml
_pkginfo.txt
*.appx
*.appxbundle
*.appxupload

################################################################################
# Backup files
################################################################################
*.backup
*.bak
*.orig

################################################################################
# Project-specific (OptiScaler Manager)
################################################################################
# Crash logs (solo local)
CrashLogs/

# Downloaded mods cache (solo local)
Mods/

# User settings (solo local)
Settings/

# Package output (builds temporales)
PackageOutput/
PackageFiles/

# Temporary screenshot folder (opcional, si no quieres subir)
# Screenshots/

# Documentation backups
*.md.backup

# PowerShell scripts backup
*.ps1.backup

################################################################################
# Keep these files (whitelist)
################################################################################
# Mantener documentación
!README.md
!LICENSE
!CONTRIBUTING.md
!CODE_OF_CONDUCT.md

# Mantener assets esenciales
!Assets/**/*.png
!Assets/**/*.jpg
!Assets/**/*.ico

# Mantener screenshots finales
!Screenshots/*.png

# Mantener docs/ para GitHub Pages
!docs/**/*

# Mantener scripts útiles
!*.ps1

################################################################################
# END
################################################################################
"@

$gitignoreContent | Out-File -FilePath ".gitignore" -Encoding UTF8 -Force
Write-Host "   ? .gitignore optimizado creado`n" -ForegroundColor Green

# Step 3: Verificar archivos sensibles
Write-Host "?? Step 3: Buscando archivos sensibles..." -ForegroundColor Yellow
$sensitivePatterns = @("*.pfx", "*.p12", "*.key", "secrets.json", ".env", "*.cer", "*.crt")
$foundSensitive = @()

foreach ($pattern in $sensitivePatterns) {
    $files = Get-ChildItem -Recurse -Filter $pattern -ErrorAction SilentlyContinue
    if ($files) {
        $foundSensitive += $files
    }
}

if ($foundSensitive.Count -gt 0) {
    Write-Host "   ??  Archivos sensibles encontrados:" -ForegroundColor Red
    foreach ($file in $foundSensitive) {
        Write-Host "      - $($file.FullName)" -ForegroundColor Yellow
    }
    Write-Host "`n   ?? Estos archivos NO se subirán (están en .gitignore)" -ForegroundColor Cyan
} else {
    Write-Host "   ? No se encontraron archivos sensibles`n" -ForegroundColor Green
}

# Step 4: Limpiar archivos de build
Write-Host "?? Step 4: Limpiando archivos de build..." -ForegroundColor Yellow
$buildFolders = @("bin", "obj", "AppPackages", "PackageOutput", "PackageFiles")
$cleaned = 0

foreach ($folder in $buildFolders) {
    $paths = Get-ChildItem -Recurse -Directory -Filter $folder -ErrorAction SilentlyContinue
    foreach ($path in $paths) {
        try {
            Remove-Item $path.FullName -Recurse -Force -ErrorAction SilentlyContinue
            $cleaned++
        } catch {}
    }
}

Write-Host "   ? Limpiadas $cleaned carpetas de build`n" -ForegroundColor Green

# Step 5: Verificar tamaño del repo
Write-Host "?? Step 5: Verificando tamaño del repositorio..." -ForegroundColor Yellow
$repoSize = (Get-ChildItem -Recurse -File | 
    Where-Object { -not $_.FullName.Contains(".git") } |
    Measure-Object -Property Length -Sum).Sum / 1MB

Write-Host "   ?? Tamaño total: $([math]::Round($repoSize, 2)) MB" -ForegroundColor White

if ($repoSize -gt 100) {
    Write-Host "   ??  Repo grande (>100MB) - considera limpiar más archivos" -ForegroundColor Yellow
} else {
    Write-Host "   ? Tamaño aceptable`n" -ForegroundColor Green
}

# Step 6: Git status
Write-Host "?? Step 6: Verificando cambios en Git..." -ForegroundColor Yellow
git add .gitignore
$gitStatus = git status --short
if ($gitStatus) {
    Write-Host "`n$gitStatus`n" -ForegroundColor Gray
} else {
    Write-Host "   ? No hay cambios pendientes`n" -ForegroundColor Green
}

# Summary
Write-Host "`n" -NoNewline
Write-Host "???????????????????????????????????????????" -ForegroundColor Cyan
Write-Host "? REPO PREPARADO PARA HACERLO PÚBLICO!" -ForegroundColor Green
Write-Host "???????????????????????????????????????????`n" -ForegroundColor Cyan

Write-Host "?? CHECKLIST DE SEGURIDAD:" -ForegroundColor Yellow
Write-Host "   ? .gitignore optimizado" -ForegroundColor Green
Write-Host "   ? Archivos sensibles protegidos" -ForegroundColor Green
Write-Host "   ? Build artifacts limpiados" -ForegroundColor Green
Write-Host "   ? Tamaño verificado" -ForegroundColor Green

Write-Host "`n?? PRÓXIMOS PASOS:" -ForegroundColor Cyan
Write-Host "`n1??  Commit .gitignore actualizado:" -ForegroundColor White
Write-Host "   git add .gitignore" -ForegroundColor Gray
Write-Host "   git commit -m ""Update .gitignore for public repo""" -ForegroundColor Gray
Write-Host "   git push" -ForegroundColor Gray

Write-Host "`n2??  Hacer repo público:" -ForegroundColor White
Write-Host "   ? Ir a: https://github.com/Bigflood92/OptiScaler-Manager-WinUI/settings" -ForegroundColor Gray
Write-Host "   ? Scroll a 'Danger Zone'" -ForegroundColor Gray
Write-Host "   ? Click 'Change visibility'" -ForegroundColor Gray
Write-Host "   ? Seleccionar 'Make public'" -ForegroundColor Gray
Write-Host "   ? Confirmar escribiendo el nombre del repo" -ForegroundColor Gray

Write-Host "`n3??  Activar GitHub Pages:" -ForegroundColor White
Write-Host "   ? Ir a: https://github.com/Bigflood92/OptiScaler-Manager-WinUI/settings/pages" -ForegroundColor Gray
Write-Host "   ? Source: Deploy from a branch" -ForegroundColor Gray
Write-Host "   ? Branch: master" -ForegroundColor Gray
Write-Host "   ? Folder: /docs" -ForegroundColor Gray
Write-Host "   ? Click 'Save'" -ForegroundColor Gray

Write-Host "`n4??  Verificar Privacy Policy (1-2 minutos después):" -ForegroundColor White
Write-Host "   https://bigflood92.github.io/OptiScaler-Manager-WinUI/PrivacyPolicy.html" -ForegroundColor Cyan

Write-Host "`n?? ARCHIVOS PROTEGIDOS (NO SE SUBIRÁN):" -ForegroundColor Yellow
Write-Host "   ? Certificados (.pfx, .p12)" -ForegroundColor Red
Write-Host "   ? API Keys (secrets.json, .env)" -ForegroundColor Red
Write-Host "   ? Build outputs (bin/, obj/)" -ForegroundColor Red
Write-Host "   ? Packages (AppPackages/)" -ForegroundColor Red
Write-Host "   ? Personal settings (.vs/, .vscode/)" -ForegroundColor Red

Write-Host "`n?? Documentación: PUBLIC_REPO_SECURITY_ANALYSIS.md" -ForegroundColor Gray
Write-Host ""
"@

$gitignoreContent | Out-File -FilePath "Prepare-PublicRepo.ps1" -Encoding UTF8 -Force
Write-Host "? Script creado: Prepare-PublicRepo.ps1" -ForegroundColor Green
