# KerbVisionIR Build Script
# Usage: .\build.ps1 [-KSPPath "C:\Path\To\KSP"]

param(
    [string]$KSPPath = $env:KSPRoot
)

if ([string]::IsNullOrEmpty($KSPPath)) {
    Write-Host "ERROR: KSP path not set!" -ForegroundColor Red
    Write-Host "Usage: .\build.ps1 -KSPPath 'C:\Path\To\KSP'" -ForegroundColor Yellow
    Write-Host "Or set the KSPRoot environment variable" -ForegroundColor Yellow
    exit 1
}

if (!(Test-Path $KSPPath)) {
    Write-Host "ERROR: KSP path does not exist: $KSPPath" -ForegroundColor Red
    exit 1
}

Write-Host "Building KerbVisionIR..." -ForegroundColor Cyan
Write-Host "KSP Path: $KSPPath" -ForegroundColor Gray

# Set the environment variable for the build
$env:KSPRoot = $KSPPath

# Build the project
dotnet build KerbVisionIR.csproj -c Release

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}

Write-Host "Build successful!" -ForegroundColor Green

# Check if GameData folder exists
$gameDataDest = Join-Path $KSPPath "GameData\KerbVisionIR"
if (Test-Path $gameDataDest) {
    Write-Host "Deploying to KSP..." -ForegroundColor Cyan
    
    # Copy DLL
    $dllSource = "bin\Release\net4.8\KerbVisionIR.dll"
    $dllDest = Join-Path $gameDataDest "Plugins\KerbVisionIR.dll"
    
    if (Test-Path $dllSource) {
        Copy-Item $dllSource $dllDest -Force
        Write-Host "DLL copied to: $dllDest" -ForegroundColor Green
    } else {
        Write-Host "WARNING: DLL not found at $dllSource" -ForegroundColor Yellow
    }
    
    # Copy version file
    $versionSource = "GameData\KerbVisionIR\KerbVisionIR.version"
    $versionDest = Join-Path $gameDataDest "KerbVisionIR.version"
    
    if (Test-Path $versionSource) {
        Copy-Item $versionSource $versionDest -Force
        Write-Host "Version file copied" -ForegroundColor Green
    }
    
    Write-Host "`nDeployment complete!" -ForegroundColor Green
    Write-Host "Launch KSP and check KSP.log for '[KerbVisionIR]' messages" -ForegroundColor Cyan
} else {
    Write-Host "`nGameData folder not found in KSP. Manual deployment required." -ForegroundColor Yellow
    Write-Host "Copy the following to your KSP installation:" -ForegroundColor Yellow
    Write-Host "  - bin\Release\net4.8\KerbVisionIR.dll -> GameData\KerbVisionIR\Plugins\" -ForegroundColor Gray
    Write-Host "  - GameData\KerbVisionIR\* -> GameData\KerbVisionIR\" -ForegroundColor Gray
}

Write-Host "`nRemember to copy the TUFX shader bundle!" -ForegroundColor Magenta
Write-Host "See REQUIRED_FILES.md for details" -ForegroundColor Magenta
