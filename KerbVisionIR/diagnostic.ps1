# KerbVisionIR - Diagnostic Check
# Uruchom PRZED startem KSP

Write-Host "=== KerbVisionIR Diagnostyka ===" -ForegroundColor Cyan
Write-Host ""

$kspPath = "C:\Program Files\Epic Games\KerbalSpaceProgram\English"
$modPath = "$kspPath\GameData\KerbVisionIR"

# Check DLL
if (Test-Path "$modPath\Plugins\KerbVisionIR.dll") {
    $dll = Get-Item "$modPath\Plugins\KerbVisionIR.dll"
    Write-Host "✓ DLL znaleziona" -ForegroundColor Green
    Write-Host "  Rozmiar: $([math]::Round($dll.Length/1KB,2)) KB" -ForegroundColor Gray
    Write-Host "  Data: $($dll.LastWriteTime)" -ForegroundColor Gray
} else {
    Write-Host "✗ DLL BRAK!" -ForegroundColor Red
}

# Check shader
if (Test-Path "$modPath\Shaders\kerbvision-pp.ssf") {
    Write-Host "✓ Shader znaleziony" -ForegroundColor Green
} elseif (Test-Path "$modPath\Shaders\tufx-universal.ssf") {
    Write-Host "⚠ Shader z inną nazwą (tufx-universal.ssf)" -ForegroundColor Yellow
    Write-Host "  Kod oczekuje: kerbvision-pp.ssf" -ForegroundColor Gray
} else {
    Write-Host "✗ SHADER BRAK - KRYTYCZNE!" -ForegroundColor Red
    Write-Host "  Skopiuj z: GameData\TUFX\Shaders\tufx-universal.ssf" -ForegroundColor Yellow
}

# Check sound
if (Test-Path "$modPath\Sounds\NVon.wav") {
    Write-Host "✓ Dźwięk znaleziony" -ForegroundColor Green
} else {
    Write-Host "⚠ Dźwięk brak (opcjonalny)" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "=== CO ZROBIĆ ===" -ForegroundColor Cyan
Write-Host "1. Zrestartuj KSP (jeśli było uruchomione)" -ForegroundColor White
Write-Host "2. Załaduj save w trybie Flight" -ForegroundColor White
Write-Host "3. Naciśnij Alt + ` (backtick)" -ForegroundColor White
Write-Host "4. Sprawdź KSP.log na linie z '[KerbVisionIR]'" -ForegroundColor White
Write-Host ""
Write-Host "Jeśli nie działa, przeszukaj KSP.log:" -ForegroundColor Yellow
Write-Host '  Get-Content "$kspPath\KSP.log" | Select-String "KerbVisionIR"' -ForegroundColor Gray
