# Tworzy GitHub release v2.0.1 w garyblu71mods/SimpleNV z ZIPem
# Uzycie: .\create-github-release.ps1 -Token "ghp_..."

param([Parameter(Mandatory=$true)][string]$Token)

$RepoRoot = Split-Path $PSScriptRoot -Parent
$ZipPath  = Join-Path $RepoRoot "Releases\KerbVisionIR_v2_0_1.zip"
$H = @{
    Authorization = "Bearer $Token"
    Accept = "application/vnd.github+json"
    "X-GitHub-Api-Version" = "2022-11-28"
}

Write-Host "=== Create GitHub Release v2.0.1 ===" -ForegroundColor Cyan

if (!(Test-Path $ZipPath)) {
    Write-Host "ERROR: ZIP not found at $ZipPath" -ForegroundColor Red
    Write-Host "Run package.ps1 first." -ForegroundColor Yellow
    exit 1
}

# Delete existing tag/release if present
Write-Host "[1/3] Checking for existing release..." -ForegroundColor Yellow
try {
    $existing = Invoke-RestMethod -Uri "https://api.github.com/repos/garyblu71mods/SimpleNV/releases/tags/v2.0.1" -Headers $H
    Invoke-RestMethod -Uri "https://api.github.com/repos/garyblu71mods/SimpleNV/releases/$($existing.id)" -Method DELETE -Headers $H | Out-Null
    Write-Host "      Removed existing release." -ForegroundColor Gray
} catch {}

# Create release
Write-Host "[2/3] Creating release v2.0.1..." -ForegroundColor Yellow
$release = Invoke-RestMethod -Uri "https://api.github.com/repos/garyblu71mods/SimpleNV/releases" `
    -Method POST -Headers $H -ContentType "application/json" `
    -Body (@{
        tag_name = "v2.0.1"
        name     = "KerbVisionIR 2.0.1"
        body     = "Night vision mod for KSP 1.12.x.`n`nSee CHANGELOG.md for details."
        draft    = $false
        prerelease = $false
    } | ConvertTo-Json)
Write-Host "      Release created: $($release.html_url)" -ForegroundColor Green

# Upload ZIP asset
Write-Host "[3/3] Uploading ZIP..." -ForegroundColor Yellow
$uploadUrl = $release.upload_url -replace '\{.*\}', ''
$uploadUrl += "?name=KerbVisionIR_v2_0_1.zip"
$zipBytes = [IO.File]::ReadAllBytes($ZipPath)
Invoke-RestMethod -Uri $uploadUrl -Method POST -Headers $H `
    -ContentType "application/zip" -Body $zipBytes | Out-Null
Write-Host "      ZIP uploaded OK." -ForegroundColor Green

Write-Host ""
Write-Host "Done! Release: $($release.html_url)" -ForegroundColor Cyan
