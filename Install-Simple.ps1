# INSTALACION SIMPLE - METODO ALTERNATIVO

Write-Host "`nMETODO SIMPLE DE INSTALACION`n" -ForegroundColor Cyan

# Verificar que somos admin
$isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Write-Host "ERROR: Ejecuta como Administrador`n" -ForegroundColor Red
    exit 1
}

$packagePath = "PackageOutput\OptiScalerManager_10.0.26100.0_x64.msix"

Write-Host "Instalando package directamente...`n" -ForegroundColor Yellow

try {
    Add-AppxPackage -Path $packagePath -ErrorAction Stop
    Write-Host "EXITO! Package instalado`n" -ForegroundColor Green
    Write-Host "Lanza la app desde Start Menu > OptiScaler Manager`n" -ForegroundColor Cyan
} catch {
    Write-Host "ERROR: $($_.Exception.Message)`n" -ForegroundColor Red
    Write-Host "SOLUCION:" -ForegroundColor Yellow
    Write-Host "1. Settings > Update & Security > For developers" -ForegroundColor White
    Write-Host "2. Activar 'Developer mode'" -ForegroundColor White  
    Write-Host "3. Reintentar instalacion`n" -ForegroundColor White
}
