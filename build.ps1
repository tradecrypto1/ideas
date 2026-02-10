# Claude Code Installer - Build Script
# This script automates the build process

Write-Host "╔════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║    Claude Code Installer - Build Script               ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# Check if .NET SDK is installed
Write-Host "🔍 Checking for .NET SDK..." -NoNewline
try {
    $dotnetVersion = dotnet --version 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host " ✓ Found (v$dotnetVersion)" -ForegroundColor Green
    } else {
        throw "Not found"
    }
} catch {
    Write-Host " ✗ Not found" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please install .NET 8.0 SDK from:" -ForegroundColor Yellow
    Write-Host "https://dotnet.microsoft.com/download/dotnet/8.0" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Press any key to open download page..."
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    Start-Process "https://dotnet.microsoft.com/download/dotnet/8.0"
    exit 1
}

Write-Host ""
Write-Host "Choose build option:" -ForegroundColor Cyan
Write-Host "  1. Quick build (requires .NET runtime on target PC)"
Write-Host "  2. Single-file build (standalone, larger file size)"
Write-Host "  3. Both"
Write-Host ""
Write-Host "Enter choice (1-3): " -NoNewline -ForegroundColor Yellow

$choice = Read-Host

Write-Host ""

function Build-Quick {
    Write-Host "📦 Building quick version..." -ForegroundColor Cyan
    dotnet build --configuration Release
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Build successful!" -ForegroundColor Green
        Write-Host "   Location: bin\Release\net8.0\ClaudeCodeInstaller.exe" -ForegroundColor Gray
        return $true
    } else {
        Write-Host "✗ Build failed" -ForegroundColor Red
        return $false
    }
}

function Build-SingleFile {
    Write-Host "📦 Building single-file version (this may take a minute)..." -ForegroundColor Cyan
    dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:PublishTrimmed=true
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Build successful!" -ForegroundColor Green
        Write-Host "   Location: bin\Release\net8.0\win-x64\publish\ClaudeCodeInstaller.exe" -ForegroundColor Gray
        
        $fileSize = (Get-Item "bin\Release\net8.0\win-x64\publish\ClaudeCodeInstaller.exe").Length / 1MB
        Write-Host "   Size: $([math]::Round($fileSize, 2)) MB" -ForegroundColor Gray
        return $true
    } else {
        Write-Host "✗ Build failed" -ForegroundColor Red
        return $false
    }
}

$success = $false

switch ($choice) {
    "1" {
        $success = Build-Quick
    }
    "2" {
        $success = Build-SingleFile
    }
    "3" {
        $quick = Build-Quick
        Write-Host ""
        $single = Build-SingleFile
        $success = $quick -or $single
    }
    default {
        Write-Host "Invalid choice. Building quick version..." -ForegroundColor Yellow
        Write-Host ""
        $success = Build-Quick
    }
}

Write-Host ""
if ($success) {
    Write-Host "╔════════════════════════════════════════════════════════╗" -ForegroundColor Green
    Write-Host "║              Build completed successfully!             ║" -ForegroundColor Green
    Write-Host "╚════════════════════════════════════════════════════════╝" -ForegroundColor Green
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Cyan
    Write-Host "  • Run the installer from the bin folder"
    Write-Host "  • Or distribute it to install Claude Code on other PCs"
    Write-Host ""
    
    Write-Host "Would you like to run the installer now? (y/n): " -NoNewline -ForegroundColor Yellow
    $runNow = Read-Host
    
    if ($runNow -eq "y" -or $runNow -eq "Y") {
        Write-Host ""
        if (Test-Path "bin\Release\net8.0\win-x64\publish\ClaudeCodeInstaller.exe") {
            Start-Process "bin\Release\net8.0\win-x64\publish\ClaudeCodeInstaller.exe"
        } elseif (Test-Path "bin\Release\net8.0\ClaudeCodeInstaller.exe") {
            Start-Process "bin\Release\net8.0\ClaudeCodeInstaller.exe"
        }
    }
} else {
    Write-Host "╔════════════════════════════════════════════════════════╗" -ForegroundColor Red
    Write-Host "║                  Build failed!                         ║" -ForegroundColor Red
    Write-Host "╚════════════════════════════════════════════════════════╝" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please check the error messages above." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Press any key to exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
