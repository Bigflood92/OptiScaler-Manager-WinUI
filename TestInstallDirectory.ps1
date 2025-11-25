# Test InstallDirectory Detection
Write-Host "=== Testing InstallDirectory Detection ===" -ForegroundColor Cyan
Write-Host ""

$xboxPath = "C:\XboxGames"

if (Test-Path $xboxPath) {
    $gameFolders = Get-ChildItem $xboxPath -Directory | Where-Object { $_.Name -ne "GameSave" }
    
    foreach ($folder in $gameFolders) {
        Write-Host "Game: $($folder.Name)" -ForegroundColor White
        Write-Host "  Root Path: $($folder.FullName)" -ForegroundColor Gray
        
        # Find executable
        $contentPath = Join-Path $folder.FullName "Content"
        $searchPath = if (Test-Path $contentPath) { $contentPath } else { $folder.FullName }
        
        $exes = Get-ChildItem $searchPath -Recurse -Filter "*.exe" -ErrorAction SilentlyContinue | 
            Where-Object { 
                $_.Name -notlike "*helper*" -and 
                $_.Name -notlike "*repair*" -and
                $_.Name -notlike "*UEPrereq*" -and
                $_.Name -notlike "*unins*" -and
                $_.Name -notlike "*setup*"
            } | 
            Sort-Object FullName
        
        if ($exes.Count -gt 0) {
            $mainExe = $exes | Where-Object { 
                $_.FullName -like "*WinGDK*" -or 
                $_.Name -like "*Shipping*" 
            } | Select-Object -First 1
            
            if (-not $mainExe) {
                $mainExe = $exes | Select-Object -First 1
            }
            
            Write-Host "  Executable: $($mainExe.FullName)" -ForegroundColor Green
            Write-Host "  InstallDirectory (where mod goes): $($mainExe.Directory.FullName)" -ForegroundColor Yellow
            
            # Verify this is correct
            if ($folder.Name -eq "Keeper") {
                $expectedDir = "C:\XboxGames\Keeper\Content\PaganIdol\Binaries\WinGDK"
                if ($mainExe.Directory.FullName -eq $expectedDir) {
                    Write-Host "  ? CORRECT! Matches expected path for Keeper" -ForegroundColor Green
                } else {
                    Write-Host "  ? MISMATCH! Expected: $expectedDir" -ForegroundColor Red
                    Write-Host "  ?           Got:      $($mainExe.Directory.FullName)" -ForegroundColor Red
                }
            }
        }
        
        Write-Host ""
    }
    
    Write-Host "=== Summary ===" -ForegroundColor Cyan
    Write-Host "? Root Path (GameInfo.Path): Game root folder (e.g., C:\XboxGames\Keeper)" -ForegroundColor Gray
    Write-Host "? Executable (GameInfo.Executable): Full path to .exe" -ForegroundColor Gray
    Write-Host "? InstallDirectory (GameInfo.InstallDirectory): Directory containing .exe (WHERE MODS GO)" -ForegroundColor Yellow
} else {
    Write-Host "? Xbox Games folder not found: $xboxPath" -ForegroundColor Red
}
