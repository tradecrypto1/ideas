# InstallationService

## Overview

`InstallationService` handles installing, uninstalling, verifying, and running **Claude Code** (native installer) and **Claude Adapter** (npm) on Windows.

## Location

`src/ClaudeCodeInstaller.Core/InstallationService.cs`

## Claude Code (native installer)

Claude Code is installed via the official PowerShell script (`irm https://claude.ai/install.ps1 | iex`), not npm. Node.js is **not** required for Claude Code.

### Key methods

| Method | Description |
|--------|-------------|
| `InstallClaudeCodeAsync()` | Runs the official install script. Stops any running `claude`/`claude-code` first. |
| `VerifyInstallationAsync()` | Checks for `claude.exe` on disk and/or `claude`/`claude-code` on PATH; runs `claude --version` when possible. |
| `RunClaudeCodeAsync(string? arguments)` | Launches Claude Code. Uses `%USERPROFILE%\.local\bin\claude.exe` when present so it works before PATH is updated. |
| `UninstallClaudeCodeAsync()` | Stops `claude`/`claude-code` processes, removes `%USERPROFILE%\.local\bin\claude.exe` and `%USERPROFILE%\.local\share\claude`. Supports legacy npm uninstall. |
| `IsClaudeCodeRunningAsync()` | Returns true if a `claude` or `claude-code` process is running. |
| `StopClaudeCodeAsync()` | Kills all `claude` and `claude-code` processes. |

### Prerequisites

- `CheckPrerequisitesAsync()` — currently only verifies PowerShell is available (always true on Windows 11).

## Node.js and Claude Adapter

Node.js/npm are required only for **Claude Adapter** (and other npm-based plugins). The app can install Node.js if missing.

| Method | Description |
|--------|-------------|
| `CheckNpmAvailableAsync()` | Returns true if `npm --version` succeeds. |
| `InstallNodeJsAsync(IProgress<int>?, Action<string>?)` | Installs Node.js LTS: tries **winget** (`OpenJS.NodeJS.LTS`), then fallback download of Node LTS MSI from nodejs.org and silent install. Progress and optional log callback. Caller may need to restart app for PATH. |
| `InstallPluginAsync(string packageName, IProgress<int>?)` | Global `npm install -g` (e.g. `claude-adapter`). Throws if npm not available. |
| `UninstallPluginAsync(string packageName)` | Global `npm uninstall -g`. Throws if npm not available. |
| `IsPluginInstalledAsync(string packageName)` | Returns true if `npm list -g <package> --depth=0` contains the package. |
| `RunClaudeAdapterAsync(string? arguments)` | Runs `claude-adapter` via `cmd.exe /k claude-adapter` (optional arguments). |

## Other methods

| Method | Description |
|--------|-------------|
| `CheckCommandAsync(string command)` | Returns true if the command runs (e.g. `npm --version`). |
| `IsWindows11OrLater()` | Static; true if Windows 10 build ≥ 22000. |
| `IsAdministrator()` | Static; true if current process is elevated. |

## Dependencies

- `HttpClient` — downloads (e.g. Node MSI fallback)
- `Process` — running installers, npm, claude, winget
- `RuntimeInformation` / `Environment` — OS and paths

## Error handling

Methods throw on failure. Callers should use try/catch. Install/run methods may throw with messages like "npm is required..." or "Failed to start Claude Adapter...".

## Thread safety

Not thread-safe. Use one instance per logical flow.
