# MainForm (WinForms)

## Overview

Main UI for the Claude Code Installer & Runner: install/run Claude Code, install/run Claude Adapter, advanced settings, health check.

## Location

`src/ClaudeCodeInstaller.WinForms/MainForm.cs`

## Tabs

### Main tab

| Control | Purpose |
|--------|---------|
| **Install/Update Claude Code** | Runs official native installer; enables Run when done. |
| **Run Claude Code** | Launches Claude Code (enabled when installed). |
| **Check for Updates** | Checks for newer installer release; can update the app. |
| **Uninstall Claude Code** | Red button; removes Claude Code (enabled when installed). |
| **Install Claude Adapter** | Blue; npm global install of `claude-adapter`. If Node.js is missing, offers to install Node.js LTS. |
| **Run Claude Adapter** | Green; runs `claude-adapter` in a console (enabled when adapter installed). |
| **Health Check** | Reports install status, paths, OS. |
| **Version / status / progress / log** | Installer version, status text, progress bar, log text box. |

### Advanced tab

- **Working directory** — path used for running tools; can browse and save.
- **Paths** — diagnostic (e.g. Node, npm, Claude) via `InstallationService`.

## Initialization

- Constructor: sets version from assembly, loads working directory, calls `InitializeComponent()`, `InitializeMainTab()`, `InitializeAdvancedTab()`, then creates `InstallationService`, `HealthCheckService`, `InstallerUpdateService`.
- `MainForm_Load`: fire-and-forget `LoadInitialStateAsync()` (verify Claude Code, adapter, enable/disable buttons) and `CheckInstallerUpdateAsync()`.

## Key flows

- **Install Claude Code:** Install button → stop if running → run official PowerShell install script → verify → enable Run/Uninstall.
- **Install Claude Adapter:** If npm missing → prompt “Install Node.js LTS now?” → `InstallNodeJsAsync` (winget or MSI) → recheck npm; if still missing, ask user to restart app. Then `InstallPluginAsync("claude-adapter")` and enable Run Claude Adapter.
- **Uninstall:** Red button → stop Claude Code → remove install dir/exe → refresh UI.

## Dependencies

- `ClaudeCodeInstaller.Core` — `InstallationService`, `HealthCheckService`, `InstallerUpdateService`
- `System.Windows.Forms`

## Notes

- All UI is built in code (`InitializeMainTab`, `InitializeAdvancedTab`); no separate designer file.
- Run Claude Code uses full path to `claude.exe` when present so it works before PATH is updated.
