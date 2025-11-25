################################################################################
# INSTALL CERTIFICATE AND PACKAGE
# Instala el certificado y luego el package
# DEBE EJECUTARSE COMO ADMINISTRADOR
################################################################################

# Verificar si se ejecuta como admin
$isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $isAdmin) {
    Write-Host "`n? ERROR: Este script debe ejecutarse como ADMINISTRADOR`n" -ForegroundColor Red
    Write-Host "?? Clic derecho en PowerShell > Ejecutar como administrador`n" -ForegroundColor Yellow
    Write-Host "Luego ejecuta:" -ForegroundColor White
    Write-Host "   cd 'C:\Users\Jorge\OneDrive\OptiScaler Manager'" -ForegroundColor Gray
    Write-Host "   .\Install-Package.ps1`n" -ForegroundColor Gray
    exit 1
}

Write-Host "`n?? INSTALANDO OPTISCALER MANAGER`n" -ForegroundColor Cyan
Write-Host "=================================`n" -ForegroundColor Cyan

$certPath = "PackageOutput\OptiScaler_TestCert.pfx"
$packagePath = "PackageOutput\OptiScalerManager_10.0.26100.0_x64.msix"

# Step 1: Verificar archivos
Write-Host "?? Step 1: Verificando archivos..." -ForegroundColor Yellow
if (-not (Test-Path $certPath)) {
    Write-Host "   ? Certificado no encontrado: $certPath" -ForegroundColor Red
    Write-Host "   ?? Ejecuta primero: .\Sign-MSIXPackage.ps1" -ForegroundColor Yellow
    exit 1
}
if (-not (Test-Path $packagePath)) {
    Write-Host "   ? Package no encontrado: $packagePath" -ForegroundColor Red
    exit 1
}
Write-Host "   ? Archivos encontrados`n" -ForegroundColor Green

# Step 2: Extraer certificado del PFX
Write-Host "?? Step 2: Extrayendo certificado..." -ForegroundColor Yellow
$tempCerPath = "PackageOutput\OptiScaler_TestCert.cer"

try {
    # Extraer el certificado público del PFX
    $certPasswordSecure = ConvertTo-SecureString -String "TestPassword123" -Force -AsPlainText
    $pfxCert = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2($certPath, $certPasswordSecure)
    
    # Exportar a CER (solo clave pública)
    $cerBytes = $pfxCert.Export([System.Security.Cryptography.X509Certificates.X509ContentType]::Cert)
    [System.IO.File]::WriteAllBytes($tempCerPath, $cerBytes)
    
    Write-Host "   ? Certificado extraído`n" -ForegroundColor Green
} catch {
    Write-Host "   ? Error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Step 3: Instalar certificado en TrustedPeople
Write-Host "?? Step 3: Instalando certificado en TrustedPeople..." -ForegroundColor Yellow
try {
    Import-Certificate -FilePath $tempCerPath -CertStoreLocation 'Cert:\LocalMachine\TrustedPeople' -ErrorAction Stop | Out-Null
    Write-Host "   ? Certificado instalado`n" -ForegroundColor Green
} catch {
    if ($_.Exception.Message -match "already exists") {
        Write-Host "   ??  Certificado ya estaba instalado`n" -ForegroundColor Cyan
    } else {
        Write-Host "   ? Error: $($_.Exception.Message)" -ForegroundColor Red
        exit 1
    }
}

# Step 4: Instalar package
Write-Host "?? Step 4: Instalando OptiScaler Manager..." -ForegroundColor Yellow
try {
    Add-AppxPackage -Path $packagePath -ErrorAction Stop
    Write-Host "   ? Package instalado exitosamente!`n" -ForegroundColor Green
} catch {
    Write-Host "   ? Error instalando package: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Step 5: Verificar instalación
Write-Host "??  Step 5: Verificando instalación..." -ForegroundColor Yellow
try {
    $installedApp = Get-AppxPackage | Where-Object { $_.Name -like "*OptiScaler*" }
    if ($installedApp) {
        Write-Host "   ? App instalada correctamente" -ForegroundColor Green
        Write-Host "   ?? Nombre: $($installedApp.Name)" -ForegroundColor White
        Write-Host "   ?? Versión: $($installedApp.Version)" -ForegroundColor White
        Write-Host "   ?? Publisher: $($installedApp.Publisher)" -ForegroundColor White
    } else {
        Write-Host "   ??  App no encontrada en lista de instaladas" -ForegroundColor Yellow
    }
} catch {
    Write-Host "   ??  No se pudo verificar: $($_.Exception.Message)" -ForegroundColor Yellow
}

# Cleanup
Remove-Item $tempCerPath -ErrorAction SilentlyContinue

# Summary
Write-Host "`n" -NoNewline
Write-Host "???????????????????????????????????????????" -ForegroundColor Cyan
Write-Host "? INSTALACIÓN COMPLETADA!" -ForegroundColor Green
Write-Host "???????????????????????????????????????????`n" -ForegroundColor Cyan

Write-Host "?? SIGUIENTE PASO:" -ForegroundColor Yellow
Write-Host "   1. Abre el menú Start" -ForegroundColor White
Write-Host "   2. Busca 'OptiScaler Manager'" -ForegroundColor White
Write-Host "   3. Click para lanzar la app`n" -ForegroundColor White

Write-Host "?? TESTING CHECKLIST:" -ForegroundColor Cyan
Write-Host "   [ ] App lanza correctamente" -ForegroundColor White
Write-Host "   [ ] Scan de juegos funciona" -ForegroundColor White
Write-Host "   [ ] Instalación de mods funciona" -ForegroundColor White
Write-Host "   [ ] Settings se guardan" -ForegroundColor White
Write-Host "   [ ] Gamepad navigation funciona" -ForegroundColor White
Write-Host "   [ ] Screenshots funcionan (Win+Alt+PrtScr)`n" -ForegroundColor White

Write-Host "?? Para desinstalar:" -ForegroundColor Gray
Write-Host "   Get-AppxPackage *OptiScaler* | Remove-AppxPackage" -ForegroundColor White
Write-Host ""
