# Uruchom ten skrypt w PowerShell jako administrator
# Zamknie stary PR, zsynchronizuje fork i otworzy nowy czysty PR
# Uzycie: .\fix-netkan-pr.ps1 -Token "ghp_..."

param([Parameter(Mandatory=$true)][string]$Token)

$H = @{
    Authorization = "Bearer $Token"
    Accept = "application/vnd.github+json"
    "X-GitHub-Api-Version" = "2022-11-28"
}

$owner = "garyblu71mods"
$netkanContent = Get-Content (Join-Path (Split-Path $PSScriptRoot -Parent) "CKAN\KerbVisionIR.netkan") -Raw

Write-Host "=== Fix NetKAN PR ===" -ForegroundColor Cyan

# 1. Zamknij stary PR #10983
Write-Host "[1/5] Zamykam stary PR #10983..." -ForegroundColor Yellow
Invoke-RestMethod -Uri "https://api.github.com/repos/KSP-CKAN/NetKAN/pulls/10983" `
    -Method PATCH -Headers $H -ContentType "application/json" `
    -Body '{"state":"closed"}' | Out-Null
Write-Host "      Zamkniety." -ForegroundColor Green

# 2. Pobierz SHA upstream master
Write-Host "[2/5] Synchronizuje fork z upstream..." -ForegroundColor Yellow
$upstream = Invoke-RestMethod -Uri "https://api.github.com/repos/KSP-CKAN/NetKAN/git/ref/heads/master" -Headers $H
$upstreamSha = $upstream.object.sha
Write-Host "      Upstream SHA: $upstreamSha" -ForegroundColor Gray

# Sync fork master z upstream
Invoke-RestMethod -Uri "https://api.github.com/repos/$owner/NetKAN/merge-upstream" `
    -Method POST -Headers $H -ContentType "application/json" `
    -Body '{"branch":"master"}' | Out-Null
Write-Host "      Fork zsynchronizowany." -ForegroundColor Green

# 3. Usun stara galaz jesli istnieje
Write-Host "[3/5] Czyszcze stara galaz..." -ForegroundColor Yellow
try {
    Invoke-RestMethod -Uri "https://api.github.com/repos/$owner/NetKAN/git/refs/heads/add-kerbvisionir" `
        -Method DELETE -Headers $H | Out-Null
} catch {}
Write-Host "      OK." -ForegroundColor Green

# 4. Stworz nowa galaz z synced master
Write-Host "[4/5] Tworze nowa galaz i dodaje plik..." -ForegroundColor Yellow
$forkMaster = Invoke-RestMethod -Uri "https://api.github.com/repos/$owner/NetKAN/git/ref/heads/master" -Headers $H
$forkSha = $forkMaster.object.sha

Invoke-RestMethod -Uri "https://api.github.com/repos/$owner/NetKAN/git/refs" `
    -Method POST -Headers $H -ContentType "application/json" `
    -Body (@{ ref = "refs/heads/add-kerbvisionir-v2"; sha = $forkSha } | ConvertTo-Json) | Out-Null

# Sprawdz czy plik juz istnieje w fork
$existingSha = $null
try {
    $ex = Invoke-RestMethod -Uri "https://api.github.com/repos/$owner/NetKAN/contents/NetKAN/KerbVisionIR.netkan?ref=add-kerbvisionir-v2" -Headers $H
    $existingSha = $ex.sha
} catch {}

$body = @{
    message = "Add KerbVisionIR (SpaceDock 4105)"
    content = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($netkanContent))
    branch  = "add-kerbvisionir-v2"
}
if ($existingSha) { $body.sha = $existingSha }

Invoke-RestMethod -Uri "https://api.github.com/repos/$owner/NetKAN/contents/NetKAN/KerbVisionIR.netkan" `
    -Method PUT -Headers $H -ContentType "application/json" `
    -Body ($body | ConvertTo-Json) | Out-Null
Write-Host "      Plik dodany." -ForegroundColor Green

# 5. Otworz nowy PR
Write-Host "[5/5] Otwieram nowy PR..." -ForegroundColor Yellow
$pr = Invoke-RestMethod -Uri "https://api.github.com/repos/KSP-CKAN/NetKAN/pulls" `
    -Method POST -Headers $H -ContentType "application/json" `
    -Body (@{
        title = "Add KerbVisionIR"
        head  = "$($owner):add-kerbvisionir-v2"
        base  = "master"
        body  = "Adding KerbVisionIR night vision mod for KSP 1.12.x.`n`nSpaceDock: https://spacedock.info/mod/4105/KerbVision%20IR`nSource: https://github.com/garyblu71mods/SimpleNV"
    } | ConvertTo-Json)

Write-Host ""
Write-Host "Gotowe!" -ForegroundColor Green
Write-Host "Nowy PR: $($pr.html_url)" -ForegroundColor Cyan
