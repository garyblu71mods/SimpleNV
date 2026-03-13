# KerbVisionIR - File Structure Checker
# Verifies all required files exist before building

Write-Host "=== KerbVisionIR File Structure Check ===" -ForegroundColor Cyan
Write-Host ""

$allGood = $true

# Check source files
$sourceFiles = @(
    "src\KerbVisionIR.cs",
    "src\VisionSettings.cs",
    "src\VisionConfig.cs",
    "src\VisionSettingsWindow.cs",
    "src\VisionPostProcessBridge.cs",
    "src\VisionLightingController.cs",
    "src\VisionAudio.cs",
    "src\PostProcessing\ParameterOverride.cs",
    "src\PostProcessing\PostProcessEffectSettings.cs",
    "src\PostProcessing\PostProcessEffectRenderer.cs",
    "src\PostProcessing\PostProcessRenderContext.cs",
    "src\PostProcessing\PostProcessProfile.cs",
    "src\PostProcessing\PostProcessResources.cs",
    "src\PostProcessing\PostProcessLayer.cs",
    "src\PostProcessing\Effects\Vignette.cs",
    "src\PostProcessing\Effects\ColorGrading.cs",
    "src\PostProcessing\Effects\Grain.cs",
    "src\PostProcessing\Utils\PropertySheet.cs",
    "src\PostProcessing\Utils\PropertySheetFactory.cs",
    "src\PostProcessing\Utils\RuntimeUtilities.cs"
)

Write-Host "Checking source files..." -ForegroundColor Yellow
foreach ($file in $sourceFiles) {
    if (Test-Path $file) {
        Write-Host "  ✓ $file" -ForegroundColor Green
    } else {
        Write-Host "  ✗ $file MISSING!" -ForegroundColor Red
        $allGood = $false
    }
}

Write-Host ""
Write-Host "Checking project files..." -ForegroundColor Yellow

$projectFiles = @(
    "KerbVisionIR.csproj",
    "Properties\AssemblyInfo.cs",
    "build.ps1"
)

foreach ($file in $projectFiles) {
    if (Test-Path $file) {
        Write-Host "  ✓ $file" -ForegroundColor Green
    } else {
        Write-Host "  ✗ $file MISSING!" -ForegroundColor Red
        $allGood = $false
    }
}

Write-Host ""
Write-Host "Checking external files..." -ForegroundColor Yellow

# Shader bundle
if (Test-Path "GameData\KerbVisionIR\Shaders\kerbvision-pp.ssf") {
    Write-Host "  ✓ Shader bundle (kerbvision-pp.ssf)" -ForegroundColor Green
} elseif (Test-Path "GameData\KerbVisionIR\Shaders\tufx-universal.ssf") {
    Write-Host "  ✓ Shader bundle (tufx-universal.ssf) - rename to kerbvision-pp.ssf or update code" -ForegroundColor Yellow
} else {
    Write-Host "  ✗ Shader bundle MISSING - Copy from TUFX!" -ForegroundColor Red
    Write-Host "     Copy: GameData/TUFX/Shaders/tufx-universal.ssf" -ForegroundColor Gray
    Write-Host "     To:   GameData/KerbVisionIR/Shaders/kerbvision-pp.ssf" -ForegroundColor Gray
    $allGood = $false
}

# Sound file (optional)
if (Test-Path "GameData\KerbVisionIR\Sounds\NVon.wav") {
    Write-Host "  ✓ Sound file (NVon.wav)" -ForegroundColor Green
} else {
    Write-Host "  ⚠ Sound file missing (optional)" -ForegroundColor Yellow
    Write-Host "     Mod will work but won't play activation sound" -ForegroundColor Gray
}

Write-Host ""
Write-Host "Checking documentation..." -ForegroundColor Yellow

$docFiles = @(
    "README.md",
    "QUICK_START.md",
    "REQUIRED_FILES.md",
    "PROJECT_SUMMARY.md",
    "MIGRATION_COMPLETE.md"
)

foreach ($file in $docFiles) {
    if (Test-Path $file) {
        Write-Host "  ✓ $file" -ForegroundColor Green
    } else {
        Write-Host "  ⚠ $file missing" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "=== Summary ===" -ForegroundColor Cyan

if ($allGood) {
    Write-Host "✓ All required files present!" -ForegroundColor Green
    Write-Host "Ready to build with: .\build.ps1" -ForegroundColor Green
} else {
    Write-Host "✗ Some files are missing!" -ForegroundColor Red
    Write-Host "Fix the issues above before building" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "File count:" -ForegroundColor Gray
Write-Host "  Source files: $($sourceFiles.Count)" -ForegroundColor Gray
Write-Host "  Project files: $($projectFiles.Count)" -ForegroundColor Gray
Write-Host "  Documentation: $($docFiles.Count)" -ForegroundColor Gray
Write-Host "  Total: $($sourceFiles.Count + $projectFiles.Count + $docFiles.Count)" -ForegroundColor Gray
