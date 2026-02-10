# InstallationService

## Overview
`InstallationService` is the core service class responsible for downloading, installing, and managing Claude Code on Windows 11 systems.

## Location
`src/ClaudeCodeInstaller.Core/InstallationService.cs`

## Responsibilities
- Checking system prerequisites (Node.js, Git)
- Downloading Claude Code installer
- Installing Claude Code silently
- Verifying installation
- Checking for updates
- Running Claude Code

## Key Methods

### `CheckPrerequisitesAsync()`
Checks if Node.js is installed (required for Claude Code).

**Returns:** `Task<bool>` - True if prerequisites are met

### `DownloadClaudeCodeAsync(string? downloadPath, IProgress<int>? progress)`
Downloads the Claude Code installer from the official source.

**Parameters:**
- `downloadPath`: Optional path to save the installer (defaults to temp directory)
- `progress`: Optional progress reporter for download status

**Returns:** `Task<string>` - Path to downloaded installer

**Throws:** `HttpRequestException` if download fails

### `InstallClaudeCodeAsync(string installerPath)`
Runs the Claude Code installer silently with administrator privileges.

**Parameters:**
- `installerPath`: Path to the installer executable

**Throws:** `Exception` if installation fails

### `VerifyInstallationAsync()`
Verifies that Claude Code is properly installed and accessible via command line.

**Returns:** `Task<bool>` - True if installation is verified

### `CheckCommandAsync(string command)`
Checks if a command is available in the system PATH.

**Parameters:**
- `command`: Command to check (e.g., "node --version")

**Returns:** `Task<bool>` - True if command is available

### `GetLatestVersionAsync()`
Retrieves the latest available version of Claude Code.

**Returns:** `Task<string?>` - Latest version string or null if unavailable

### `IsUpdateAvailableAsync(string currentVersion)`
Checks if an update is available for Claude Code.

**Parameters:**
- `currentVersion`: Current installed version

**Returns:** `Task<bool>` - True if update is available

### `RunClaudeCodeAsync(string? arguments)`
Launches Claude Code with optional arguments.

**Parameters:**
- `arguments`: Optional command-line arguments

### Static Methods

#### `IsWindows11OrLater()`
Checks if the system is running Windows 11 or later.

**Returns:** `bool` - True if Windows 11+

#### `IsAdministrator()`
Checks if the current process is running with administrator privileges.

**Returns:** `bool` - True if running as administrator

## Dependencies
- `System.Net.Http.HttpClient` - For downloading files
- `System.Diagnostics.Process` - For running processes
- `System.Runtime.InteropServices` - For OS platform detection
- `System.Security.Principal` - For privilege checking

## Usage Example
```csharp
var service = new InstallationService();
bool hasPrereqs = await service.CheckPrerequisitesAsync();
if (hasPrereqs)
{
    string installerPath = await service.DownloadClaudeCodeAsync();
    await service.InstallClaudeCodeAsync(installerPath);
    bool verified = await service.VerifyInstallationAsync();
}
```

## Error Handling
All methods throw exceptions on failure. Callers should wrap calls in try-catch blocks.

## Thread Safety
This class is not thread-safe. Create separate instances for concurrent operations.

## Notes
- Downloads are saved to the system temp directory by default
- Installation requires administrator privileges
- PATH updates may take a few seconds after installation
