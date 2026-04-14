# Tworzy GitHub release w garyblu71mods/SimpleNV z ZIPem
# Uzycie: .\create-github-release.ps1 -Token "ghp_..." [-Version "2.0.3"]

param(
    [Parameter(Mandatory=$true)][string]$Token,
    [string]$Version = "2.0.3"
)

$RepoRoot   = Split-Path $PSScriptRoot -Parent
$ZipName    = "KerbVisionIR_v$($Version.Replace('.','_')).zip"
$ZipPath    = Join-Path $RepoRoot "Releases\$ZipName"
$TagName    = "v$Version"
$ReleaseName = "KerbVisionIR $Version"

$H = @{
    Authorization = "Bearer $Token"
    Accept = "application/vnd.github+json"
    "X-GitHub-Api-Version" = "2022-11-28"
}

Write-Host "=== Create GitHub Release $TagName ===" -ForegroundColor Cyan

if (!(Test-Path $ZipPath)) {
    Write-Host "ERROR: ZIP not found at $ZipPath" -ForegroundColor Red
    Write-Host "Run package.ps1 -Version $Version first." -ForegroundColor Yellow
    exit 1
}

# Read changelog entry for this version
$changelogPath = Join-Path $RepoRoot "CHANGELOG.md"
$changelogBody = "Night vision mod for KSP 1.12.x.`n`nSee CHANGELOG.md for details."
if (Test-Path $changelogPath) {
    $lines = Get-Content $changelogPath
    $capture = $false
    $entry = @()
    foreach ($line in $lines) {
        if ($line -match "^## $([regex]::Escape($Version))") { $capture = $true; continue }
        if ($capture -and $line -match "^## " ) { break }
        if ($capture) { $entry += $line }
    }
    if ($entry.Count -gt 0) { $changelogBody = ($entry | Where-Object { $_ -ne "" }) -join "`n" }
}

# Delete existing tag/release if present
Write-Host "[1/3] Checking for existing release $TagName..." -ForegroundColor Yellow
try {
    $existing = Invoke-RestMethod -Uri "https://api.github.com/repos/garyblu71mods/SimpleNV/releases/tags/$TagName" -Headers $H
    Invoke-RestMethod -Uri "https://api.github.com/repos/garyblu71mods/SimpleNV/releases/$($existing.id)" -Method DELETE -Headers $H | Out-Null
    Write-Host "      Removed existing release." -ForegroundColor Gray
} catch {}
try {
    Invoke-RestMethod -Uri "https://api.github.com/repos/garyblu71mods/SimpleNV/git/refs/tags/$TagName" -Method DELETE -Headers $H | Out-Null
    Write-Host "      Removed existing tag." -ForegroundColor Gray
} catch {}

# Create release
Write-Host "[2/3] Creating release $ReleaseName..." -ForegroundColor Yellow
$release = Invoke-RestMethod -Uri "https://api.github.com/repos/garyblu71mods/SimpleNV/releases" `
    -Method POST -Headers $H -ContentType "application/json" `
    -Body (@{
        tag_name = $TagName
        name     = $ReleaseName
        body     = $changelogBody
        draft    = $false
        prerelease = $false
    } | ConvertTo-Json)
Write-Host "      Release created: $($release.html_url)" -ForegroundColor Green

# Upload ZIP asset
Write-Host "[3/3] Uploading $ZipName..." -ForegroundColor Yellow
$uploadUrl = $release.upload_url -replace '\{.*\}', ''
$uploadUrl += "?name=$ZipName"
$zipBytes = [IO.File]::ReadAllBytes($ZipPath)
Invoke-RestMethod -Uri $uploadUrl -Method POST -Headers $H `
    -ContentType "application/zip" -Body $zipBytes | Out-Null
Write-Host "      ZIP uploaded OK." -ForegroundColor Green

Write-Host ""
Write-Host "Done! Release: $($release.html_url)" -ForegroundColor Cyan
