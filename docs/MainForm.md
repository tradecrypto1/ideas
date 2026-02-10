# MainForm (WinForms Application)

## Overview
Windows Forms application providing a graphical interface for installing, updating, and running Claude Code on Windows 11.

## Location
`src/ClaudeCodeInstaller.WinForms/MainForm.cs`

## Features
- **Install/Update**: Download and install Claude Code
- **Run**: Launch Claude Code directly from the application
- **Check Updates**: Verify if newer versions are available
- **Health Check**: Comprehensive system health verification
- **Progress Tracking**: Visual progress bar and status updates
- **Logging**: Detailed log output for troubleshooting

## UI Components

### Buttons
- **Install/Update Claude Code**: Initiates installation or update process
- **Run Claude Code**: Launches Claude Code (enabled when installed)
- **Check for Updates**: Checks for newer versions
- **Health Check**: Performs system health verification

### Status Elements
- **Version Label**: Shows installer version
- **Status Label**: Current operation status
- **Progress Bar**: Visual progress indicator
- **Log Text Box**: Detailed operation logs

## Installation Process

1. Checks for updates (if updating)
2. Verifies prerequisites (Node.js)
3. Downloads Claude Code installer
4. Installs silently
5. Verifies installation
6. Enables Run button on success

## Update Detection

- Checks for newer versions on startup
- Prompts user to update if available
- Uninstalls current version before installing new one

## Health Check

Provides comprehensive system status:
- Claude Code installation status
- Prerequisites availability
- OS version compatibility
- Administrator privileges

## Dependencies
- `ClaudeCodeInstaller.Core` - Core installation logic
- `System.Windows.Forms` - WinForms UI framework

## Usage
Double-click the executable or run from command line:
```powershell
.\ClaudeCodeInstaller.WinForms.exe
```

## Notes
- Provides user-friendly GUI alternative to console installer
- Can run Claude Code without opening terminal
- Shows detailed logs for troubleshooting
- Automatically checks for updates
