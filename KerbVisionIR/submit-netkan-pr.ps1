# KerbVisionIR - NetKAN PR Submission Script
# Forks KSP-CKAN/NetKAN, adds KerbVisionIR.netkan, opens PR automatically
# Usage: .\submit-netkan-pr.ps1 -Token "ghp_yourtoken"

param(
    [Parameter(Mandatory=$true)]
    [string]$Token
)

$ErrorActionPreference = "Stop"
$Headers = @{
    Authorization = "Bearer $Token"
    Accept        = "application/vnd.github+json"
    "X-GitHub-Api-Version" = "2022-11-28"
}

$RepoRoot    = Split-Path $PSScriptRoot -Parent
$NetkanFile  = Join-Path $RepoRoot "CKAN\KerbVisionIR.netkan"
$NetkanContent = Get-Content $NetkanFile -Raw

Write-Host ""
Write-Host "=== KerbVisionIR NetKAN PR Submitter ===" -ForegroundColor Cyan
Write-Host ""

# 1. Get authenticated user
Write-Host "[1/5] Checking GitHub token..." -ForegroundColor Yellow
$user = Invoke-RestMethod -Uri "https://api.github.com/user" -Headers $Headers
Write-Host "      Logged in as: $($user.login)" -ForegroundColor Green

# 2. Fork KSP-CKAN/NetKAN
Write-Host "[2/5] Forking KSP-CKAN/NetKAN..." -ForegroundColor Yellow
try {
    $fork = Invoke-RestMethod -Uri "https://api.github.com/repos/KSP-CKAN/NetKAN/forks" `
        -Method POST -Headers $Headers -Body "{}" -ContentType "application/json"
    Write-Host "      Fork created: $($fork.full_name)" -ForegroundColor Green
} catch {
    if ($_.Exception.Response.StatusCode -eq 422) {
        Write-Host "      Fork already exists." -ForegroundColor Gray
    } else { throw }
}

# Wait for fork to be ready
Write-Host "      Waiting for fork to be ready..." -ForegroundColor Gray
Start-Sleep -Seconds 5

$forkOwner = $user.login
$forkRepo  = "NetKAN"

# 3. Get default branch SHA
Write-Host "[3/5] Getting branch info..." -ForegroundColor Yellow
$branch = Invoke-RestMethod `
    -Uri "https://api.github.com/repos/$forkOwner/$forkRepo/git/ref/heads/master" `
    -Headers $Headers
$sha = $branch.object.sha
Write-Host "      master SHA: $sha" -ForegroundColor Gray

# 4. Create file in fork
Write-Host "[4/5] Adding NetKAN/KerbVisionIR.netkan to fork..." -ForegroundColor Yellow
$encodedContent = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($NetkanContent))

$body = @{
    message = "Add KerbVisionIR (SpaceDock 4105)"
    content = $encodedContent
    branch  = "add-kerbvisionir"
} | ConvertTo-Json

# Create branch first
try {
    $newBranch = @{
        ref = "refs/heads/add-kerbvisionir"
        sha = $sha
    } | ConvertTo-Json
    Invoke-RestMethod -Uri "https://api.github.com/repos/$forkOwner/$forkRepo/git/refs" `
        -Method POST -Headers $Headers -Body $newBranch -ContentType "application/json" | Out-Null
    Write-Host "      Branch add-kerbvisionir created." -ForegroundColor Gray
} catch {
    Write-Host "      Branch already exists, continuing..." -ForegroundColor Gray
}

# Check if file already exists (get SHA for update)
$existingSha = $null
try {
    $existing = Invoke-RestMethod `
        -Uri "https://api.github.com/repos/$forkOwner/$forkRepo/contents/NetKAN/KerbVisionIR.netkan?ref=add-kerbvisionir" `
        -Headers $Headers
    $existingSha = $existing.sha
} catch {}

if ($existingSha) {
    $body = @{
        message = "Add KerbVisionIR (SpaceDock 4105)"
        content = $encodedContent
        sha     = $existingSha
        branch  = "add-kerbvisionir"
    } | ConvertTo-Json
}

Invoke-RestMethod `
    -Uri "https://api.github.com/repos/$forkOwner/$forkRepo/contents/NetKAN/KerbVisionIR.netkan" `
    -Method PUT -Headers $Headers -Body $body -ContentType "application/json" | Out-Null
Write-Host "      File added OK." -ForegroundColor Green

# 5. Create PR
Write-Host "[5/5] Opening Pull Request to KSP-CKAN/NetKAN..." -ForegroundColor Yellow
$pr = @{
    title = "Add KerbVisionIR"
    head  = "$($forkOwner):add-kerbvisionir"
    base  = "master"
    body  = "Adding KerbVisionIR night vision mod for KSP 1.12.x.`n`nSpaceDock: https://spacedock.info/mod/4105/KerbVision%20IR`nSource: https://github.com/garyblu71mods/SimpleNV"
} | ConvertTo-Json

$prResult = Invoke-RestMethod -Uri "https://api.github.com/repos/KSP-CKAN/NetKAN/pulls" `
    -Method POST -Headers $Headers -Body $pr -ContentType "application/json"

Write-Host ""
Write-Host "PR created!" -ForegroundColor Green
Write-Host "URL: $($prResult.html_url)" -ForegroundColor Cyan
Write-Host ""
Write-Host "Done. CKAN bot will process the PR automatically after it is merged." -ForegroundColor Cyan
