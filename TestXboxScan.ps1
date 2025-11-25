# Test Xbox Game Scanner
Write-Host "=== Testing Xbox Game Detection ===" -ForegroundColor Cyan
Write-Host ""

$xboxPath = "C:\XboxGames"

if (Test-Path $xboxPath) {
    Write-Host "? Xbox Games folder found: $xboxPath" -ForegroundColor Green
    Write-Host ""
    
    $gameFolders = Get-ChildItem $xboxPath -Directory | Where-Object { $_.Name -ne "GameSave" }
    
    Write-Host "Found $($gameFolders.Count) potential game folders:" -ForegroundColor Yellow
    Write-Host ""
    
    foreach ($folder in $gameFolders) {
        Write-Host "Game: $($folder.Name)" -ForegroundColor White
        
        # Check for Content folder
        $contentPath = Join-Path $folder.FullName "Content"
        $searchPath = if (Test-Path $contentPath) { $contentPath } else { $folder.FullName }
        
        Write-Host "  Search path: $searchPath" -ForegroundColor Gray
        
        # Find executables
        $exes = Get-ChildItem $searchPath -Recurse -Filter "*.exe" -ErrorAction SilentlyContinue | 
            Where-Object { 
                $_.Name -notlike "*unins*" -and 
                $_.Name -notlike "*setup*" -and
                $_.Name -notlike "*helper*" -and
                $_.Name -notlike "*repair*" -and
                $_.Name -notlike "*UEPrereq*" -and
                $_.Name -notlike "*redist*" -and
                $_.Name -notlike "*vcredist*" -and
                $_.Name -notlike "*directx*"
            } | 
            Sort-Object FullName
        
        if ($exes.Count -gt 0) {
            # Prefer WinGDK or Shipping builds
            $mainExe = $exes | Where-Object { 
                $_.FullName -like "*WinGDK*" -or 
                $_.Name -like "*Shipping*" 
            } | Select-Object -First 1
            
            if (-not $mainExe) {
                $mainExe = $exes | Select-Object -First 1
            }
            
            Write-Host "  ? Executable: $($mainExe.Name)" -ForegroundColor Green
            Write-Host "  ? Full path: $($mainExe.FullName)" -ForegroundColor Green
            
            # Check for OptiScaler mods
            $gameRoot = $folder.FullName
            $hasOptiScaler = (Test-Path (Join-Path $gameRoot "nvngx.dll")) -or
                            (Test-Path (Join-Path $gameRoot "libxess.dll")) -or
                            (Test-Path (Join-Path $gameRoot "amd_fidelityfx_vk.dll"))
            
            if ($hasOptiScaler) {
                Write-Host "  ? OptiScaler detected!" -ForegroundColor Magenta
            } else {
                Write-Host "  ? OptiScaler not installed" -ForegroundColor DarkGray
            }
        } else {
            Write-Host "  ? No valid executable found" -ForegroundColor Red
        }
        
        Write-Host ""
    }
    
    Write-Host "=== Scan Complete ===" -ForegroundColor Cyan
} else {
    Write-Host "? Xbox Games folder not found: $xboxPath" -ForegroundColor Red
}
