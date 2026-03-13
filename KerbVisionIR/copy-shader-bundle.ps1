# KerbVisionIR - Shader Bundle Copy Script
# Automatically copies shader bundle from Unity project to all required locations

param(
    [string]$UnityProjectPath = ""
)

Write-Host "`n╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║  KerbVisionIR - Shader Bundle Copy Utility              ║" -ForegroundColor Cyan
Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# Detect Unity project path
if ($UnityProjectPath -eq "") {
    $possiblePaths = @(
        "$env:USERPROFILE\Desktop\KerbVisionIR-Shaders",
        "$env:USERPROFILE\Documents\Unity Projects\KerbVisionIR-Shaders",
        "$env:USERPROFILE\Unity Projects\KerbVisionIR-Shaders"
    )
    
    foreach ($path in $possiblePaths) {
        if (Test-Path "$path\AssetBundles\kerbvision-pp.ssf") {
            $UnityProjectPath = $path
            break
        }
    }
}

if ($UnityProjectPath -eq "") {
    Write-Host "❌ Nie znaleziono projektu Unity!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Podaj ścieżkę do projektu Unity:" -ForegroundColor Yellow
    Write-Host "Np: C:\Users\Nazwa\Desktop\KerbVisionIR-Shaders" -ForegroundColor Gray
    Write-Host ""
    $UnityProjectPath = Read-Host "Ścieżka"
}

$sourceBundle = Join-Path $UnityProjectPath "AssetBundles\kerbvision-pp.ssf"

# Verify source exists
if (-not (Test-Path $sourceBundle)) {
    Write-Host "❌ BŁĄD: Nie znaleziono bundle w:" -ForegroundColor Red
    Write-Host "   $sourceBundle" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Czy zbudowałeś bundle w Unity?" -ForegroundColor Yellow
    Write-Host "   Menu: Assets → Build PostProcessing Bundle" -ForegroundColor Gray
    exit 1
}

$bundleInfo = Get-Item $sourceBundle
Write-Host "✓ Bundle znaleziony:" -ForegroundColor Green
Write-Host "  Lokalizacja: $($bundleInfo.FullName)" -ForegroundColor Gray
Write-Host "  Rozmiar: $([math]::Round($bundleInfo.Length/1KB, 2)) KB" -ForegroundColor Gray
Write-Host "  Data: $($bundleInfo.LastWriteTime)" -ForegroundColor Gray
Write-Host ""

# Define destinations
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$destinations = @{
    "Projekt GameData" = Join-Path $scriptDir "GameData\KerbVisionIR\Shaders\kerbvision-pp.ssf"
    "Release Package" = Join-Path $scriptDir "Release\GameData\KerbVisionIR\Shaders\kerbvision-pp.ssf"
    "KSP Installation" = "C:\Program Files\Epic Games\KerbalSpaceProgram\English\GameData\KerbVisionIR\Shaders\kerbvision-pp.ssf"
}

Write-Host "Kopiowanie do lokalizacji..." -ForegroundColor Cyan
Write-Host ""

$successCount = 0
foreach ($dest in $destinations.GetEnumerator()) {
    $destPath = $dest.Value
    $destDir = Split-Path -Parent $destPath
    
    try {
        # Create directory if doesn't exist
        if (-not (Test-Path $destDir)) {
            New-Item -ItemType Directory -Force -Path $destDir | Out-Null
        }
        
        # Copy file
        Copy-Item $sourceBundle $destPath -Force
        
        # Verify
        if (Test-Path $destPath) {
            $destFile = Get-Item $destPath
            Write-Host "  ✓ $($dest.Key)" -ForegroundColor Green
            Write-Host "    → $destPath" -ForegroundColor Gray
            Write-Host "    → $([math]::Round($destFile.Length/1KB, 2)) KB" -ForegroundColor Gray
            $successCount++
        } else {
            Write-Host "  ✗ $($dest.Key) - weryfikacja failed" -ForegroundColor Red
        }
    }
    catch {
        Write-Host "  ✗ $($dest.Key) - błąd: $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Gray
Write-Host ""

if ($successCount -eq $destinations.Count) {
    Write-Host "🎉 SUKCES! Shader skopiowany do wszystkich lokalizacji!" -ForegroundColor Green
    Write-Host ""
    Write-Host "NASTĘPNE KROKI:" -ForegroundColor Cyan
    Write-Host "  1. ZRESTARTUJ KSP (shader ładuje się przy starcie)" -ForegroundColor White
    Write-Host "  2. Wejdź w Flight" -ForegroundColor White
    Write-Host "  3. Alt + F8 → sprawdź 'FULL MODE'" -ForegroundColor White
    Write-Host "  4. Testuj efekty (vignette, saturation, etc.)" -ForegroundColor White
    Write-Host ""
    Write-Host "📋 Sprawdź logi KSP:" -ForegroundColor Yellow
    Write-Host '  Get-Content "$env:ProgramFiles\Epic Games\KerbalSpaceProgram\English\KSP.log" | Select-String "KerbVisionIR"' -ForegroundColor Gray
    Write-Host ""
    Write-Host "✨ Powodzenia!" -ForegroundColor Green
}
else {
    Write-Host "⚠️ Skopiowano do $successCount/$($destinations.Count) lokalizacji" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Sprawdź błędy powyżej i spróbuj ponownie." -ForegroundColor Gray
}

Write-Host ""
