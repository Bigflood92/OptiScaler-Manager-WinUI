################################################################################
# SIGN MSIX PACKAGE - Create certificate and sign package
################################################################################

Write-Host "`n?? FIRMANDO MSIX PACKAGE`n" -ForegroundColor Cyan
Write-Host "=======================`n" -ForegroundColor Cyan

$packagePath = "PackageOutput\OptiScalerManager_10.0.26100.0_x64.msix"
$certPath = "PackageOutput\OptiScaler_TestCert.pfx"
$certPassword = "TestPassword123"

# Step 1: Verificar package
Write-Host "?? Step 1: Verificando package..." -ForegroundColor Yellow
if (-not (Test-Path $packagePath)) {
    Write-Host "   ? Package no encontrado: $packagePath" -ForegroundColor Red
    exit 1
}
Write-Host "   ? Package encontrado`n" -ForegroundColor Green

# Step 2: Crear certificado de prueba
Write-Host "?? Step 2: Creando certificado de prueba..." -ForegroundColor Yellow
try {
    $cert = New-SelfSignedCertificate `
        -Type CodeSigningCert `
        -Subject "CN=Bigflood92" `
        -KeyUsage DigitalSignature `
        -FriendlyName "OptiScaler Manager Test Certificate" `
        -CertStoreLocation "Cert:\CurrentUser\My" `
        -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.3", "2.5.29.19={text}")
    
    Write-Host "   ? Certificado creado: $($cert.Thumbprint)" -ForegroundColor Green
} catch {
    Write-Host "   ? Error creando certificado: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Step 3: Exportar certificado a PFX
Write-Host "`n?? Step 3: Exportando certificado..." -ForegroundColor Yellow
try {
    $certPasswordSecure = ConvertTo-SecureString -String $certPassword -Force -AsPlainText
    Export-PfxCertificate -Cert $cert -FilePath $certPath -Password $certPasswordSecure | Out-Null
    Write-Host "   ? Certificado exportado: $certPath`n" -ForegroundColor Green
} catch {
    Write-Host "   ? Error exportando certificado: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Step 4: Buscar signtool
Write-Host "?? Step 4: Buscando signtool..." -ForegroundColor Yellow
$signtoolPath = $null
$windowsKitsPath = "C:\Program Files (x86)\Windows Kits\10\bin"

if (Test-Path $windowsKitsPath) {
    $sdkVersions = Get-ChildItem $windowsKitsPath -Directory | 
        Where-Object { $_.Name -match '^\d+\.\d+\.\d+\.\d+$' } |
        Sort-Object Name -Descending
    
    foreach ($version in $sdkVersions) {
        $testPath = Join-Path $version.FullName "x64\signtool.exe"
        if (Test-Path $testPath) {
            $signtoolPath = $testPath
            Write-Host "   ? Encontrado: $signtoolPath`n" -ForegroundColor Green
            break
        }
    }
}

if (-not $signtoolPath) {
    Write-Host "   ? signtool.exe no encontrado!" -ForegroundColor Red
    Write-Host "   ?? Instala Windows SDK desde:" -ForegroundColor Yellow
    Write-Host "      https://developer.microsoft.com/windows/downloads/windows-sdk/" -ForegroundColor Gray
    exit 1
}

# Step 5: Firmar package
Write-Host "??  Step 5: Firmando package..." -ForegroundColor Yellow
try {
    & $signtoolPath sign /fd SHA256 /a /f $certPath /p $certPassword $packagePath
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "   ? Package firmado exitosamente!`n" -ForegroundColor Green
    } else {
        Write-Host "   ? Error firmando package (exit code: $LASTEXITCODE)" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "   ? Error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Step 6: Verificar firma
Write-Host "?? Step 6: Verificando firma..." -ForegroundColor Yellow
try {
    & $signtoolPath verify /pa $packagePath
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "   ? Firma verificada`n" -ForegroundColor Green
    } else {
        Write-Host "   ??  Advertencia: Verificación con error (normal para cert de prueba)`n" -ForegroundColor Yellow
    }
} catch {
    Write-Host "   ??  No se pudo verificar: $($_.Exception.Message)`n" -ForegroundColor Yellow
}

# Step 7: Instalar certificado en Trusted Root
Write-Host "?? Step 7: Instalando certificado en Trusted Root..." -ForegroundColor Yellow
Write-Host "   ??  Se requieren permisos de administrador" -ForegroundColor Yellow
Write-Host "   ?? Copia y ejecuta este comando en PowerShell como Admin:" -ForegroundColor Cyan
Write-Host "`n   Import-Certificate -FilePath '$certPath' -CertStoreLocation 'Cert:\LocalMachine\TrustedPeople'`n" -ForegroundColor White

# Summary
Write-Host "`n" -NoNewline
Write-Host "???????????????????????????????????????????" -ForegroundColor Cyan
Write-Host "? PACKAGE FIRMADO!" -ForegroundColor Green
Write-Host "???????????????????????????????????????????`n" -ForegroundColor Cyan

Write-Host "?? PACKAGE INFO:" -ForegroundColor Cyan
Write-Host "   Package: $packagePath" -ForegroundColor White
Write-Host "   Certificado: $certPath" -ForegroundColor White
Write-Host "   Thumbprint: $($cert.Thumbprint)`n" -ForegroundColor White

Write-Host "?? PRÓXIMOS PASOS:" -ForegroundColor Yellow
Write-Host "`n   1. Abrir PowerShell como ADMINISTRADOR" -ForegroundColor White
Write-Host "   2. Ejecutar:" -ForegroundColor White
Write-Host "      cd 'C:\Users\Jorge\OneDrive\OptiScaler Manager'" -ForegroundColor Gray
Write-Host "      Import-Certificate -FilePath '$certPath' -CertStoreLocation 'Cert:\LocalMachine\TrustedPeople'`n" -ForegroundColor Gray

Write-Host "   3. Luego instalar package:" -ForegroundColor White
Write-Host "      Add-AppxPackage -Path '$packagePath'`n" -ForegroundColor Gray

Write-Host "?? NOTA: El certificado solo es necesario para testing local." -ForegroundColor Cyan
Write-Host "   Microsoft Store firmará el package oficialmente." -ForegroundColor Cyan
Write-Host ""
