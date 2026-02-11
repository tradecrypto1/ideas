# build.ps1 - Full rebuild script for Claude Code Installer
# Usage: .\build.ps1 [-Publish] [-SkipTests]
#   -Publish    Also publish WinForms app to artifacts\winforms
#   -SkipTests  Skip running tests after build

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
}

Write-Host ""
Write-Host "=== Rebuild complete ===" -ForegroundColor Cyan
