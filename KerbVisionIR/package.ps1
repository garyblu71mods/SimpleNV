# KerbVisionIR Release Packaging Script
# Creates KerbVisionIR_v2_0_1.zip ready for SpaceDock/CKAN
# Usage: .\package.ps1 -KSPPath "C:\Path\To\KSP"

param(
    [string]$KSPPath = $env:KSPRoot,
    [string]$Version = "2.0.3"
)

$RepoRoot   = Split-Path $PSScriptRoot -Parent
$CsprojPath = Join-Path $RepoRoot "Source\TUFX.csproj"
$StagingDir = Join-Path $RepoRoot "Releases\staging\GameData\KerbVisionIR"
$ZipName    = "KerbVisionIR_v$($Version.Replace('.','_')).zip"
$ZipOut     = Join-Path $RepoRoot "Releases\$ZipName"

Write-Host ""
Write-Host "=== KerbVisionIR Package Builder v$Version ===" -ForegroundColor Cyan
Write-Host ""

# --- 1. Check KSP path (needed for build references) ---
if ([string]::IsNullOrEmpty($KSPPath) -or !(Test-Path $KSPPath)) {
    Write-Host "ERROR: Valid KSP path required." -ForegroundColor Red
    Write-Host "Usage: .\package.ps1 -KSPPath 'C:\Path\To\KSP'" -ForegroundColor Yellow
    exit 1
}
$env:KSPRoot = $KSPPath

# --- 2. Build ---
Write-Host "[1/5] Building..." -ForegroundColor Yellow
dotnet build $CsprojPath -c Release
if ($LASTEXITCODE -ne 0) { Write-Host "Build FAILED." -ForegroundColor Red; exit 1 }
Write-Host "      Build OK." -ForegroundColor Green

# --- 3. Prepare staging folder ---
Write-Host "[2/5] Preparing staging folder..." -ForegroundColor Yellow
if (Test-Path $StagingDir) { Remove-Item $StagingDir -Recurse -Force }
New-Item -ItemType Directory -Path "$StagingDir\Plugins" -Force | Out-Null
New-Item -ItemType Directory -Path "$StagingDir\Shaders"  -Force | Out-Null
New-Item -ItemType Directory -Path "$StagingDir\Sounds"   -Force | Out-Null
New-Item -ItemType Directory -Path "$StagingDir\Assets"   -Force | Out-Null

# --- 4. Copy files ---
Write-Host "[3/5] Copying files..." -ForegroundColor Yellow

# DLL (output by KSPBuildTools directly to GameData/KerbVisionIR/Plugins)
$dll = Join-Path $RepoRoot "GameData\KerbVisionIR\Plugins\KerbVisionIR.dll"
if (Test-Path $dll) {
    Copy-Item $dll "$StagingDir\Plugins\" -Force
    Write-Host "      KerbVisionIR.dll OK" -ForegroundColor Green
} else {
    Write-Host "      ERROR: KerbVisionIR.dll not found at $dll" -ForegroundColor Red
    exit 1
}

# Version file
Copy-Item (Join-Path $RepoRoot "GameData\KerbVisionIR\KerbVisionIR.version") $StagingDir -Force
Write-Host "      KerbVisionIR.version OK" -ForegroundColor Green

# Shader bundle — try kerbvision-pp.ssf first, fall back to tufx-universal.ssf
$shaderDest = "$StagingDir\Shaders\kerbvision-pp.ssf"
$shaderSrc  = Join-Path $RepoRoot "GameData\KerbVisionIR\Shaders\kerbvision-pp.ssf"
$shaderFallback = Join-Path $RepoRoot "GameData\TUFX\Shaders\tufx-universal.ssf"

if (Test-Path $shaderSrc) {
    Copy-Item $shaderSrc $shaderDest -Force
    Write-Host "      kerbvision-pp.ssf OK" -ForegroundColor Green
} elseif (Test-Path $shaderFallback) {
    Copy-Item $shaderFallback $shaderDest -Force
    Write-Host "      kerbvision-pp.ssf (copied from tufx-universal.ssf) OK" -ForegroundColor Green
} else {
    Write-Host "      WARNING: Shader bundle not found. NV effect will not work!" -ForegroundColor Magenta
    Write-Host "      Expected: GameData\KerbVisionIR\Shaders\kerbvision-pp.ssf" -ForegroundColor Gray
}

# Sound
$sound = Join-Path $RepoRoot "GameData\KerbVisionIR\Sounds\NVon.wav"
if (Test-Path $sound) {
    Copy-Item $sound "$StagingDir\Sounds\" -Force
    Write-Host "      NVon.wav OK" -ForegroundColor Green
} else {
    Write-Host "      WARNING: NVon.wav not found (sound optional)" -ForegroundColor Yellow
}

# Toolbar icon
$icon = Join-Path $RepoRoot "GameData\KerbVisionIR\Assets\KerbVisionIR-Icon.png"
if (Test-Path $icon) {
    Copy-Item $icon "$StagingDir\Assets\" -Force
    Write-Host "      KerbVisionIR-Icon.png OK" -ForegroundColor Green
} else {
    Write-Host "      WARNING: Toolbar icon not found (toolbar button will be blank)" -ForegroundColor Yellow
}

# --- 5. Create ZIP ---
Write-Host "[4/5] Creating ZIP..." -ForegroundColor Yellow
if (Test-Path $ZipOut) { Remove-Item $ZipOut -Force }
$zipBase = Split-Path $StagingDir -Parent  # .../staging/GameData
$zipRoot = Split-Path $zipBase -Parent      # .../staging
Compress-Archive -Path "$zipRoot\GameData" -DestinationPath $ZipOut
Write-Host "      $ZipOut" -ForegroundColor Green

# --- 6. Summary ---
Write-Host ""
Write-Host "[5/5] Done!" -ForegroundColor Cyan
Write-Host ""
Write-Host "ZIP contents:" -ForegroundColor Gray
Get-ChildItem "$zipRoot\GameData" -Recurse | Where-Object { !$_.PSIsContainer } |
    ForEach-Object { Write-Host "  $($_.FullName.Replace($zipRoot,''))" -ForegroundColor Gray }
Write-Host ""
Write-Host "Upload $ZipName to SpaceDock mod 4105 as version $Version" -ForegroundColor Cyan
