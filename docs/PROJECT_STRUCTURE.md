# Project Structure

## Overview

WinForms app for installing and running **Claude Code** (native installer) and **Claude Adapter** (npm) on Windows 10/11.

## Directory structure

```
ideas/
├── .cursor/rules/           # Cursor rules (e.g. conventional commits)
├── .github/workflows/       # CI/CD (ci.yml: build, test, publish, release)
├── docs/                   # Documentation
│   ├── CONVENTIONAL_COMMITS.md
│   ├── HealthCheckEndpoint.md, HealthCheckService.md, HealthCheckEndpoint.md
│   ├── InstallationService.md, MainForm.md, Program-WinForms.md
│   ├── PROJECT_STRUCTURE.md, TESTING.md, VersionInfo.md
│   └── ...
├── src/
│   ├── ClaudeCodeInstaller.Core/    # Core library
│   │   ├── InstallationService.cs  # Install/uninstall/verify Claude Code; Node.js install; npm/Claude Adapter
│   │   ├── HealthCheckService.cs, HealthCheckEndpoint.cs
│   │   ├── InstallerUpdateService.cs
│   │   └── VersionInfo.cs
│   └── ClaudeCodeInstaller.WinForms/
│       ├── Program.cs       # Entry point
│       └── MainForm.cs      # Main + Advanced tabs (all UI in code)
├── tests/
│   └── ClaudeCodeInstaller.Tests/
├── build.ps1                # Full rebuild: clean, restore, build, test; -Publish → artifacts/winforms
├── ClaudeCodeInstaller.sln
├── QUICKSTART.md, README.md
└── tasks.md
```

## Projects

| Project | Purpose |
|---------|--------|
| **ClaudeCodeInstaller.Core** | Installation/uninstall/verify Claude Code; install Node.js; npm plugins (Claude Adapter); health check; installer update. |
| **ClaudeCodeInstaller.WinForms** | GUI: Main tab (Install/Run/Uninstall Claude Code, Install/Run Claude Adapter, Health Check), Advanced tab (working dir, paths). |
| **ClaudeCodeInstaller.Tests** | Unit tests (xUnit, FluentAssertions). |

## Build outputs

- **Development:** `src/ClaudeCodeInstaller.WinForms/bin/Release/net8.0-windows/win-x64/ClaudeCodeInstaller.WinForms.exe`
- **Publish:** `.\build.ps1 -Publish` → `artifacts/winforms/` (win-x64, self-contained).

## CI/CD (ci.yml)

- Checkout, .NET setup, NuGet cache, restore
- Build with security analyzers; outdated/vulnerable package checks
- Build solution; publish WinForms to `artifacts/winforms` (single-file)
- Tests with coverage; Codecov upload
- Windows Defender scan of artifacts
- On push to `main`: version from Core csproj, previous tag, changelog, RC tag, GitHub Release with artifacts

## Features

- Claude Code: native install, run, uninstall, verification (no Node.js)
- Claude Adapter: npm install/run; optional Node.js LTS install (winget/MSI)
- Advanced: working directory, path diagnostics
- Health check, installer self-update, RC tagging, releases
