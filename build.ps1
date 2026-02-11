# build.ps1 - Full rebuild script for Claude Code Installer
# Usage: .\build.ps1 [-Publish] [-SkipTests]
#   -Publish    Also publish WinForms app to artifacts\winforms
#   -SkipTests  Skip running tests after build
#
# Optional code signing (when -Publish): set env vars before running:
#   CLAUDE_INSTALLER_SIGN_PFX      Path to .pfx file
#   CLAUDE_INSTALLER_SIGN_PASSWORD PFX password
# See docs/CODE_SIGNING.md for details.

param(
    [switch]$Publish,
    [switch]$SkipTests
)

$ErrorActionPreference = "Stop"
$slnPath = Join-Path $PSScriptRoot "ClaudeCodeInstaller.sln"
$winFormsProject = Join-Path $PSScriptRoot "src\ClaudeCodeInstaller.WinForms\ClaudeCodeInstaller.WinForms.csproj"
$artifactsDir = Join-Path $PSScriptRoot "artifacts"
$winformsOut = Join-Path $artifactsDir "winforms"

Write-Host "=== Full Rebuild ===" -ForegroundColor Cyan
Write-Host ""

# 1. Clean
Write-Host "[1/4] Cleaning..." -ForegroundColor Yellow
dotnet clean $slnPath --configuration Release -v q
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
Write-Host "  Clean done." -ForegroundColor Green
Write-Host ""

# 2. Restore
Write-Host "[2/4] Restoring packages..." -ForegroundColor Yellow
dotnet restore $slnPath
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
Write-Host "  Restore done." -ForegroundColor Green
Write-Host ""

# 3. Build
Write-Host "[3/4] Building (Release)..." -ForegroundColor Yellow
dotnet build $slnPath --configuration Release --no-incremental
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
Write-Host "  Build done." -ForegroundColor Green
Write-Host ""

# 4. Test (unless skipped)
if (-not $SkipTests) {
    Write-Host "[4/4] Running tests..." -ForegroundColor Yellow
    dotnet test $slnPath --configuration Release --no-build -v n
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    Write-Host "  Tests passed." -ForegroundColor Green
} else {
    Write-Host "[4/4] Skipping tests (-SkipTests)." -ForegroundColor Gray
}
Write-Host ""

# Optional: Publish WinForms
if ($Publish) {
    Write-Host "Publishing WinForms app to $winformsOut ..." -ForegroundColor Yellow
    New-Item -ItemType Directory -Force -Path $winformsOut | Out-Null
    dotnet publish $winFormsProject `
        --configuration Release `
        --runtime win-x64 `
        --self-contained true `
        --output $winformsOut
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    Write-Host "  Published to $winformsOut" -ForegroundColor Green

    # Optional: Authenticode sign the exe (reduces AV false positives)
    $signPfx = $env:CLAUDE_INSTALLER_SIGN_PFX
    $signPassword = $env:CLAUDE_INSTALLER_SIGN_PASSWORD
    $exePath = Join-Path $winformsOut "ClaudeCodeInstaller.WinForms.exe"
    if ($signPfx -and $signPassword -and (Test-Path $exePath)) {
        $signtool = $null
        $sdkRoot = "${env:ProgramFiles(x86)}\Windows Kits\10"
        if (Test-Path $sdkRoot) {
            $signtool = Get-ChildItem -Path $sdkRoot -Recurse -Filter "signtool.exe" -ErrorAction SilentlyContinue | Where-Object { $_.FullName -match "x64" } | Select-Object -First 1
        }
        if (-not $signtool) { $signtool = Get-Command signtool -ErrorAction SilentlyContinue }
        if ($signtool) {
            Write-Host "Signing executable (Authenticode)..." -ForegroundColor Yellow
            $signtoolPath = if ($null -ne $signtool.FullName) { $signtool.FullName } elseif ($signtool.Source) { $signtool.Source } else { $signtool.Path }
            & $signtoolPath sign /f $signPfx /p $signPassword /tr "http://timestamp.digicert.com" /td sha256 /fd sha256 $exePath
            if ($LASTEXITCODE -eq 0) { Write-Host "  Signed." -ForegroundColor Green } else { Write-Host "  Signing failed (exit $LASTEXITCODE)." -ForegroundColor Red }
        } else {
            Write-Host "  signtool not found; skipping signing. Install Windows SDK or set PATH." -ForegroundColor Gray
        }
    }
}

Write-Host ""
Write-Host "=== Rebuild complete ===" -ForegroundColor Cyan
