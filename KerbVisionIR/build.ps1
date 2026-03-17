# KerbVisionIR Build Script
# Usage: .\build.ps1 -KSPPath "C:\Path\To\KSP"
# Or set env var KSPRoot before running

param(
    [string]$KSPPath = $env:KSPRoot
)

$RepoRoot = Split-Path $PSScriptRoot -Parent
$CsprojPath = Join-Path $RepoRoot "Source\TUFX.csproj"

if ([string]::IsNullOrEmpty($KSPPath)) {
    Write-Host "ERROR: KSP path not set." -ForegroundColor Red
    Write-Host "Usage: .\build.ps1 -KSPPath 'C:\Path\To\KSP'" -ForegroundColor Yellow
    Write-Host "Or set the KSPRoot environment variable." -ForegroundColor Yellow
    exit 1
}

if (!(Test-Path $KSPPath)) {
    Write-Host "ERROR: KSP path does not exist: $KSPPath" -ForegroundColor Red
    exit 1
}

Write-Host "Building KerbVisionIR..." -ForegroundColor Cyan
Write-Host "KSP Path: $KSPPath" -ForegroundColor Gray
$env:KSPRoot = $KSPPath

dotnet build $CsprojPath -c Release
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build FAILED." -ForegroundColor Red
    exit 1
}

Write-Host "Build successful." -ForegroundColor Green

# DLL is output directly to GameData\KerbVisionIR\Plugins\ by KSPBuildTools
$dllDest = Join-Path $RepoRoot "GameData\KerbVisionIR\Plugins\KerbVisionIR.dll"
if (Test-Path $dllDest) {
    Write-Host "DLL ready: $dllDest" -ForegroundColor Green
} else {
    Write-Host "WARNING: DLL not found at expected path: $dllDest" -ForegroundColor Yellow
}

# Deploy to KSP GameData if available
$kspGameData = Join-Path $KSPPath "GameData\KerbVisionIR"
if (Test-Path $kspGameData) {
    Write-Host "Deploying to KSP..." -ForegroundColor Cyan
    Copy-Item $dllDest (Join-Path $kspGameData "Plugins\KerbVisionIR.dll") -Force
    Copy-Item (Join-Path $RepoRoot "GameData\KerbVisionIR\KerbVisionIR.version") $kspGameData -Force
    Write-Host "Deployment complete. Launch KSP and check KSP.log for [KerbVisionIR] messages." -ForegroundColor Green
} else {
    Write-Host "KSP GameData not found - manual copy required." -ForegroundColor Yellow
}
