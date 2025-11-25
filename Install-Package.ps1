################################################################################
# INSTALL CERTIFICATE AND PACKAGE - SIMPLIFIED
# Instala el certificado y luego el package
# DEBE EJECUTARSE COMO ADMINISTRADOR
################################################################################

# Verificar si se ejecuta como admin
$isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $isAdmin) {
    Write-Host "`nERROR: Este script debe ejecutarse como ADMINISTRADOR`n" -ForegroundColor Red
    Write-Host "Clic derecho en PowerShell > Ejecutar como administrador`n" -ForegroundColor Yellow
    exit 1
}

Write-Host "`nINSTALANDO OPTISCALER MANAGER`n" -ForegroundColor Cyan
Write-Host "=================================`n" -ForegroundColor Cyan

$scriptDir = $PSScriptRoot
$certPath = Join-Path $scriptDir "PackageOutput\OptiScaler_TestCert.pfx"
$packagePath = Join-Path $scriptDir "PackageOutput\OptiScalerManager_10.0.26100.0_x64.msix"

# Step 1: Verificar archivos
Write-Host "Step 1: Verificando archivos..." -ForegroundColor Yellow
if (-not (Test-Path $certPath)) {
    Write-Host "  ERROR: Certificado no encontrado: $certPath" -ForegroundColor Red
    Write-Host "  Ejecuta primero: .\Sign-MSIXPackage.ps1" -ForegroundColor Yellow
    exit 1
}
if (-not (Test-Path $packagePath)) {
    Write-Host "  ERROR: Package no encontrado: $packagePath" -ForegroundColor Red
    exit 1
}
Write-Host "  Archivos encontrados`n" -ForegroundColor Green

# Step 2: Extraer e instalar certificado
Write-Host "Step 2: Instalando certificado..." -ForegroundColor Yellow
try {
    $tempCerPath = Join-Path $scriptDir "PackageOutput\OptiScaler_TestCert.cer"
    
    # Cargar PFX
    $certPasswordSecure = ConvertTo-SecureString -String "TestPassword123" -Force -AsPlainText
    $pfxCert = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2
    $pfxCert.Import($certPath, $certPasswordSecure, [System.Security.Cryptography.X509Certificates.X509KeyStorageFlags]::DefaultKeySet)
    
    # Exportar a CER
    $cerBytes = $pfxCert.Export([System.Security.Cryptography.X509Certificates.X509ContentType]::Cert)
    [System.IO.File]::WriteAllBytes($tempCerPath, $cerBytes)
    
    # Importar certificado
    Import-Certificate -FilePath $tempCerPath -CertStoreLocation 'Cert:\LocalMachine\TrustedPeople' -ErrorAction Stop | Out-Null
    Write-Host "  Certificado instalado`n" -ForegroundColor Green
    
    # Cleanup
    Remove-Item $tempCerPath -Force -ErrorAction SilentlyContinue
} catch {
    if ($_.Exception.Message -match "already exists") {
        Write-Host "  Certificado ya estaba instalado`n" -ForegroundColor Cyan
    } else {
        Write-Host "  ERROR: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host "`nIntentando instalacion alternativa...`n" -ForegroundColor Yellow
        
        # Metodo alternativo: instalar directamente desde PFX
        try {
            Import-PfxCertificate -FilePath $certPath -CertStoreLocation 'Cert:\LocalMachine\TrustedPeople' -Password $certPasswordSecure -ErrorAction Stop | Out-Null
            Write-Host "  Certificado instalado (metodo alternativo)`n" -ForegroundColor Green
        } catch {
            Write-Host "  ERROR: $($_.Exception.Message)" -ForegroundColor Red
            Write-Host "`nNo se pudo instalar el certificado. Continuando de todas formas...`n" -ForegroundColor Yellow
        }
    }
}

# Step 3: Instalar package
Write-Host "Step 3: Instalando OptiScaler Manager..." -ForegroundColor Yellow
try {
    Add-AppxPackage -Path $packagePath -ErrorAction Stop
    Write-Host "  Package instalado exitosamente!`n" -ForegroundColor Green
} catch {
    Write-Host "  ERROR: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "`nSi el error es sobre certificado, activa Developer Mode:" -ForegroundColor Yellow
    Write-Host "  Settings > For developers > Developer mode`n" -ForegroundColor Gray
    exit 1
}

# Step 4: Verificar instalacion
Write-Host "Step 4: Verificando instalacion..." -ForegroundColor Yellow
try {
    $installedApp = Get-AppxPackage | Where-Object { $_.Name -like "*OptiScaler*" }
    if ($installedApp) {
        Write-Host "  App instalada correctamente" -ForegroundColor Green
        Write-Host "  Nombre: $($installedApp.Name)" -ForegroundColor White
        Write-Host "  Version: $($installedApp.Version)" -ForegroundColor White
        Write-Host "  Publisher: $($installedApp.Publisher)" -ForegroundColor White
    }
} catch {
    Write-Host "  No se pudo verificar" -ForegroundColor Yellow
}

Write-Host "`n==========================================" -ForegroundColor Cyan
Write-Host "INSTALACION COMPLETADA!" -ForegroundColor Green
Write-Host "==========================================`n" -ForegroundColor Cyan

Write-Host "SIGUIENTE PASO:" -ForegroundColor Yellow
Write-Host "  1. Abre el menu Start" -ForegroundColor White
Write-Host "  2. Busca 'OptiScaler Manager'" -ForegroundColor White
Write-Host "  3. Click para lanzar la app`n" -ForegroundColor White

Write-Host "Para desinstalar:" -ForegroundColor Gray
Write-Host "  Get-AppxPackage *OptiScaler* | Remove-AppxPackage`n" -ForegroundColor White
