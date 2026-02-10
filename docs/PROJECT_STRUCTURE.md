# Project Structure

## Overview
This project provides a WinForms application for installing and running Claude Code on Windows 11.

## Directory Structure

```
ideas/
├── .cursor/
│   └── rules/
│       └── conventional-commits.mdc    # Cursor rules for conventional commits
├── .github/
│   └── workflows/
│       └── ci.yml                       # CI/CD pipeline (build, test, Docker, deploy)
├── docs/                                # Documentation
│   ├── CONVENTIONAL_COMMITS.md         # Commit message conventions
│   ├── TESTING.md                      # Testing guide
│   ├── InstallationService.md          # Core service documentation
│   ├── HealthCheckService.md           # Health check documentation
│   ├── HealthCheckEndpoint.md          # HTTP endpoint documentation
│   ├── VersionInfo.md                  # Version info documentation
│   ├── MainForm.md                     # WinForms app documentation
│   ├── Program-WinForms.md             # WinForms entry point documentation
│   └── PROJECT_STRUCTURE.md            # This file
├── src/                                 # Source code
│   ├── ClaudeCodeInstaller.Core/       # Core library (shared code)
│   │   ├── InstallationService.cs    # Installation logic
│   │   ├── HealthCheckService.cs       # Health checking
│   │   ├── HealthCheckEndpoint.cs      # HTTP health endpoint
│   │   └── VersionInfo.cs              # Version information
│   └── ClaudeCodeInstaller.WinForms/   # WinForms application
│       ├── Program.cs                   # WinForms entry point
│       └── MainForm.cs                  # Main UI form
├── tests/                               # Test projects
│   └── ClaudeCodeInstaller.Tests/      # Unit tests
│       ├── InstallationServiceTests.cs
│       └── HealthCheckServiceTests.cs
├── .dockerignore                       # Docker ignore rules
├── .gitignore                          # Git ignore rules
├── build.ps1                           # Build script
├── ClaudeCodeInstaller.sln              # Solution file
├── Dockerfile                          # Docker build file
├── QUICKSTART.md                       # Quick start guide
├── README.md                           # Main readme
└── tasks.md                            # Task list

```

## Projects

### ClaudeCodeInstaller.Core
Shared library containing:
- Installation logic
- Health checking
- Version management
- HTTP health endpoint

### ClaudeCodeInstaller.WinForms
Windows Forms application with:
- Graphical UI
- Install/Update functionality
- Version checking
- Auto-update capability
- Run Claude Code button
- Health check feature
- Detailed logging

### ClaudeCodeInstaller.Tests
Unit tests using:
- xUnit
- Moq (mocking)
- FluentAssertions

## Build Outputs

### WinForms Application
- `src/ClaudeCodeInstaller.WinForms/bin/Release/net8.0-windows/win-x64/ClaudeCodeInstaller.WinForms.exe`

## CI/CD Pipeline

The GitHub Actions workflow (`.github/workflows/ci.yml`) includes:
1. **Build**: Compiles all projects
2. **Test**: Runs unit tests with coverage
3. **Docker Build**: Creates Docker images
4. **Version Tag**: Creates RC tags on main branch commits
5. **Deploy**: Pushes to Azure Container Registry (ACR)

## Key Features

- ✅ Organized source code structure
- ✅ Comprehensive documentation
- ✅ Test-driven development
- ✅ Conventional commits
- ✅ CI/CD pipeline
- ✅ Health check endpoint
- ✅ WinForms installer with GUI
- ✅ Version checking and auto-update
- ✅ Runner functionality
- ✅ RC version tagging
- ✅ Docker support
- ✅ ACR deployment ready

## Development Workflow

1. Make changes following conventional commits
2. Write/update tests
3. Build: `dotnet build`
4. Test: `dotnet test`
5. Commit with conventional commit message
6. Push triggers CI/CD pipeline
7. RC tag created automatically
8. Docker image built and pushed to ACR
