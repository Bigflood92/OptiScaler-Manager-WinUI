################################################################################
# ADD MIT LICENSE
# Añade licencia MIT al proyecto para protección legal
################################################################################

Write-Host "`n?? AÑADIENDO LICENCIA MIT`n" -ForegroundColor Cyan
Write-Host "========================`n" -ForegroundColor Cyan

# MIT License content
$year = (Get-Date).Year
$licenseContent = @"
MIT License

Copyright (c) $year Bigflood92

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
"@

# Step 1: Crear LICENSE file
Write-Host "?? Step 1: Creando LICENSE file..." -ForegroundColor Yellow
$licenseContent | Out-File -FilePath "LICENSE" -Encoding UTF8 -Force
Write-Host "   ? LICENSE creado`n" -ForegroundColor Green

# Step 2: Verificar contenido
Write-Host "?? Step 2: Verificando contenido..." -ForegroundColor Yellow
if (Test-Path "LICENSE") {
    Write-Host "   ? LICENSE file existe" -ForegroundColor Green
    $content = Get-Content "LICENSE" -Raw
    if ($content -match "MIT License") {
        Write-Host "   ? Contenido correcto`n" -ForegroundColor Green
    }
}

# Summary
Write-Host "`n" -NoNewline
Write-Host "???????????????????????????????????????????" -ForegroundColor Cyan
Write-Host "? LICENCIA MIT AÑADIDA!" -ForegroundColor Green
Write-Host "???????????????????????????????????????????`n" -ForegroundColor Cyan

Write-Host "?? DETALLES DE LA LICENCIA:" -ForegroundColor Yellow
Write-Host "`n? PERMITE:" -ForegroundColor Green
Write-Host "   • Ver el código fuente" -ForegroundColor White
Write-Host "   • Usar como referencia" -ForegroundColor White
Write-Host "   • Modificar para uso personal" -ForegroundColor White
Write-Host "   • Contribuir mejoras (pull requests)" -ForegroundColor White

Write-Host "`n? NO PERMITE (sin tu permiso):" -ForegroundColor Red
Write-Host "   • Publicar en Microsoft Store como propia" -ForegroundColor White
Write-Host "   • Vender comercialmente" -ForegroundColor White
Write-Host "   • Quitar tu copyright" -ForegroundColor White
Write-Host "   • Usar tu nombre/marca" -ForegroundColor White

Write-Host "`n??? PROTECCIÓN LEGAL:" -ForegroundColor Cyan
Write-Host "   ? Copyright: Bigflood92 ($year)" -ForegroundColor Green
Write-Host "   ? Licencia: MIT (reconocida globalmente)" -ForegroundColor Green
Write-Host "   ? Válido legalmente en todos los países" -ForegroundColor Green

Write-Host "`n?? MÁS INFORMACIÓN:" -ForegroundColor Gray
Write-Host "   Ver: PUBLIC_REPO_SECURITY_ANALYSIS.md" -ForegroundColor White

Write-Host "`n?? PRÓXIMOS PASOS:" -ForegroundColor Yellow
Write-Host "   1. Commit LICENSE:" -ForegroundColor White
Write-Host "      git add LICENSE" -ForegroundColor Gray
Write-Host "      git commit -m ""Add MIT License""" -ForegroundColor Gray
Write-Host "      git push" -ForegroundColor Gray

Write-Host "`n   2. Hacer repo público:" -ForegroundColor White
Write-Host "      ? Ir a: https://github.com/Bigflood92/OptiScaler-Manager-WinUI/settings" -ForegroundColor Gray
Write-Host "      ? Danger Zone > Change visibility > Make public" -ForegroundColor Gray

Write-Host "`n   3. Activar GitHub Pages:" -ForegroundColor White
Write-Host "      ? Ir a: https://github.com/Bigflood92/OptiScaler-Manager-WinUI/settings/pages" -ForegroundColor Gray
Write-Host "      ? Source: master, Folder: /docs, Save" -ForegroundColor Gray

Write-Host ""
