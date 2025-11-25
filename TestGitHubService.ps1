# Test GitHub Service
Write-Host "=== Testing GitHub Service ===" -ForegroundColor Cyan
Write-Host ""

# Add the assemblies
Add-Type -Path "OptiScaler.Core\bin\Debug\net8.0\OptiScaler.Core.dll"

Write-Host "Creating GitHub Service..." -ForegroundColor Yellow
$githubService = New-Object OptiScaler.Core.Services.GitHubService

Write-Host "Fetching latest OptiScaler release..." -ForegroundColor Yellow
try {
    $task = $githubService.GetLatestReleaseAsync("cdozdil", "OptiScaler")
    $task.Wait()
    $release = $task.Result
    
    if ($release) {
        Write-Host "? Latest Release Found!" -ForegroundColor Green
        Write-Host "  Tag: $($release.TagName)" -ForegroundColor White
        Write-Host "  Name: $($release.Name)" -ForegroundColor White
        Write-Host "  Published: $($release.PublishedAt)" -ForegroundColor White
        Write-Host "  Assets: $($release.Assets.Count)" -ForegroundColor White
        Write-Host ""
        
        foreach ($asset in $release.Assets) {
            Write-Host "  ?? $($asset.Name) - $($asset.FormattedSize)" -ForegroundColor Cyan
            Write-Host "     Downloads: $($asset.DownloadCount)" -ForegroundColor Gray
        }
    } else {
        Write-Host "? No release found" -ForegroundColor Red
    }
} catch {
    Write-Host "? Error: $_" -ForegroundColor Red
}

Write-Host ""
Write-Host "Fetching DLSSG-to-FSR3 releases..." -ForegroundColor Yellow
try {
    $task = $githubService.GetLatestReleaseAsync("Nukem9", "dlssg-to-fsr3")
    $task.Wait()
    $release = $task.Result
    
    if ($release) {
        Write-Host "? Latest Release Found!" -ForegroundColor Green
        Write-Host "  Tag: $($release.TagName)" -ForegroundColor White
        Write-Host "  Name: $($release.Name)" -ForegroundColor White
        Write-Host "  Published: $($release.PublishedAt)" -ForegroundColor White
        Write-Host "  Assets: $($release.Assets.Count)" -ForegroundColor White
    } else {
        Write-Host "? No release found" -ForegroundColor Red
    }
} catch {
    Write-Host "? Error: $_" -ForegroundColor Red
}

Write-Host ""
Write-Host "=== Test Complete ===" -ForegroundColor Cyan

$githubService.Dispose()
