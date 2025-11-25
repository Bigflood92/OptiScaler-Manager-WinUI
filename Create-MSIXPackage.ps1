################################################################################
# MSIX PACKAGING AUTOMATION SCRIPT
# Creates MSIX package for OptiScaler Manager
################################################################################

Write-Host "`n?? MSIX PACKAGING AUTOMATION`n" -ForegroundColor Cyan
Write-Host "============================`n" -ForegroundColor Cyan

# Configuration
$ProjectPath = "OptiScaler.UI\OptiScaler.UI.csproj"
$OutputDir = "PackageOutput"
$PackageName = "OptiScalerManager"
$Version = "0.1.0.0"

# Step 1: Clean previous builds
Write-Host "?? Step 1: Cleaning previous builds..." -ForegroundColor Yellow
dotnet clean $ProjectPath -c Release
if (Test-Path $OutputDir) {
    Remove-Item $OutputDir -Recurse -Force
}
Write-Host "   ? Clean complete`n" -ForegroundColor Green

# Step 2: Restore NuGet packages
Write-Host "?? Step 2: Restoring NuGet packages..." -ForegroundColor Yellow
dotnet restore $ProjectPath
Write-Host "   ? Restore complete`n" -ForegroundColor Green

# Step 3: Build Release
Write-Host "?? Step 3: Building Release configuration..." -ForegroundColor Yellow
dotnet build $ProjectPath -c Release
if ($LASTEXITCODE -ne 0) {
    Write-Host "   ? Build failed!" -ForegroundColor Red
    exit 1
}
Write-Host "   ? Build successful`n" -ForegroundColor Green

# Step 4: Publish
Write-Host "?? Step 4: Publishing application..." -ForegroundColor Yellow
$PublishPath = "OptiScaler.UI\bin\Release\net8.0-windows10.0.19041.0\win-x64\publish"
dotnet publish $ProjectPath `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=false `
    -p:PublishReadyToRun=true

if ($LASTEXITCODE -ne 0) {
    Write-Host "   ? Publish failed!" -ForegroundColor Red
    exit 1
}
Write-Host "   ? Publish complete`n" -ForegroundColor Green

# Step 5: Check for MSIX in build output
Write-Host "?? Step 5: Looking for MSIX package..." -ForegroundColor Yellow
$AppPackagesPath = "OptiScaler.UI\bin\x64\Release\net8.0-windows10.0.19041.0\win-x64\AppPackages"
$MsixPath = "OptiScaler.UI\AppPackages"

if (Test-Path $AppPackagesPath) {
    Write-Host "   ?? Found AppPackages at: $AppPackagesPath" -ForegroundColor Green
    
    # Find MSIX file
    $msixFiles = Get-ChildItem -Path $AppPackagesPath -Filter "*.msix" -Recurse
    if ($msixFiles.Count -gt 0) {
        Write-Host "   ? MSIX package found!" -ForegroundColor Green
        foreach ($msix in $msixFiles) {
            Write-Host "      ? $($msix.FullName)" -ForegroundColor White
            Write-Host "      ? Size: $([math]::Round($msix.Length/1MB, 2)) MB" -ForegroundColor Gray
        }
    } else {
        Write-Host "   ??  No MSIX files found in AppPackages" -ForegroundColor Yellow
    }
} else {
    Write-Host "   ??  AppPackages folder not found" -ForegroundColor Yellow
    Write-Host "   ?? Trying alternative packaging method..." -ForegroundColor Cyan
}

# Step 6: Alternative - Create package structure manually
Write-Host "`n?? Step 6: Creating package structure..." -ForegroundColor Yellow

# Create output directory
New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null

# Copy published files
$PackageFilesPath = Join-Path $OutputDir "PackageFiles"
New-Item -ItemType Directory -Path $PackageFilesPath -Force | Out-Null

if (Test-Path $PublishPath) {
    Write-Host "   ?? Copying published files..." -ForegroundColor Gray
    Copy-Item -Path "$PublishPath\*" -Destination $PackageFilesPath -Recurse -Force
    
    # Copy manifest
    Write-Host "   ?? Copying manifest..." -ForegroundColor Gray
    Copy-Item -Path "OptiScaler.UI\Package.appxmanifest" -Destination $PackageFilesPath -Force
    
    # Copy assets
    Write-Host "   ?? Copying assets..." -ForegroundColor Gray
    $AssetsSource = "OptiScaler.UI\Assets"
    $AssetsDest = Join-Path $PackageFilesPath "Assets"
    if (Test-Path $AssetsSource) {
        Copy-Item -Path $AssetsSource -Destination $AssetsDest -Recurse -Force
    }
    
    Write-Host "   ? Package structure created`n" -ForegroundColor Green
} else {
    Write-Host "   ? Publish path not found: $PublishPath" -ForegroundColor Red
    exit 1
}

# Step 7: Find makeappx.exe
Write-Host "?? Step 7: Locating Windows SDK tools..." -ForegroundColor Yellow
$WindowsKitsPath = "C:\Program Files (x86)\Windows Kits\10\bin"
$makeappxPath = $null

if (Test-Path $WindowsKitsPath) {
    # Find latest SDK version
    $sdkVersions = Get-ChildItem $WindowsKitsPath -Directory | 
        Where-Object { $_.Name -match '^\d+\.\d+\.\d+\.\d+$' } |
        Sort-Object Name -Descending
    
    foreach ($version in $sdkVersions) {
        $testPath = Join-Path $version.FullName "x64\makeappx.exe"
        if (Test-Path $testPath) {
            $makeappxPath = $testPath
            Write-Host "   ? Found makeappx.exe: $makeappxPath" -ForegroundColor Green
            break
        }
    }
}

if (-not $makeappxPath) {
    Write-Host "   ??  makeappx.exe not found!" -ForegroundColor Yellow
    Write-Host "   ?? Install Windows SDK from:" -ForegroundColor Cyan
    Write-Host "      https://developer.microsoft.com/windows/downloads/windows-sdk/" -ForegroundColor Gray
    Write-Host "`n   ?? Package files ready at: $PackageFilesPath" -ForegroundColor Green
    Write-Host "   ?? See MSIX_PACKAGING_GUIDE.md for manual packaging steps" -ForegroundColor Gray
    exit 0
}

# Step 8: Create MSIX with makeappx
Write-Host "`n?? Step 8: Creating MSIX package..." -ForegroundColor Yellow
$MsixOutputPath = Join-Path $OutputDir "${PackageName}_${Version}_x64.msix"

& $makeappxPath pack /d $PackageFilesPath /p $MsixOutputPath /o

if ($LASTEXITCODE -eq 0 -and (Test-Path $MsixOutputPath)) {
    Write-Host "   ? MSIX package created successfully!" -ForegroundColor Green
    $msixFile = Get-Item $MsixOutputPath
    Write-Host "   ?? Package: $MsixOutputPath" -ForegroundColor White
    Write-Host "   ?? Size: $([math]::Round($msixFile.Length/1MB, 2)) MB`n" -ForegroundColor Gray
} else {
    Write-Host "   ? Failed to create MSIX package!" -ForegroundColor Red
    exit 1
}

# Step 9: Check for signtool
Write-Host "?? Step 9: Checking for signing tool..." -ForegroundColor Yellow
$signtoolPath = $null
$sdkBinPath = Split-Path $makeappxPath -Parent

$testSignToolPath = Join-Path $sdkBinPath "signtool.exe"
if (Test-Path $testSignToolPath) {
    $signtoolPath = $testSignToolPath
    Write-Host "   ? Found signtool.exe" -ForegroundColor Green
} else {
    Write-Host "   ??  signtool.exe not found" -ForegroundColor Yellow
    Write-Host "   ?? Package created but not signed" -ForegroundColor Cyan
    Write-Host "   ?? For Store submission, Microsoft will sign it" -ForegroundColor Cyan
}

# Summary
Write-Host "`n" -NoNewline
Write-Host "???????????????????????????????????????" -ForegroundColor Cyan
Write-Host "? PACKAGING COMPLETE!" -ForegroundColor Green
Write-Host "???????????????????????????????????????`n" -ForegroundColor Cyan

Write-Host "?? PACKAGE LOCATION:" -ForegroundColor Cyan
Write-Host "   $MsixOutputPath`n" -ForegroundColor White

Write-Host "?? NEXT STEPS:" -ForegroundColor Yellow
Write-Host "   1. Install certificate (first time only):" -ForegroundColor White
Write-Host "      See MSIX_PACKAGING_GUIDE.md`n" -ForegroundColor Gray

Write-Host "   2. Install package:" -ForegroundColor White
Write-Host "      Add-AppxPackage -Path `"$MsixOutputPath`"`n" -ForegroundColor Gray

Write-Host "   3. Test application:" -ForegroundColor White
Write-Host "      Search 'OptiScaler Manager' in Start Menu`n" -ForegroundColor Gray

Write-Host "   4. For Microsoft Store:" -ForegroundColor White
Write-Host "      Upload to Partner Center`n" -ForegroundColor Gray

Write-Host "?? Documentation: MSIX_PACKAGING_GUIDE.md" -ForegroundColor Gray
Write-Host ""
