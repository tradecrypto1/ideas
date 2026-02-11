# Quick start

## Run the app

- **If you have the .exe:** double-click `ClaudeCodeInstaller.WinForms.exe` (or the single-file build from `artifacts\winforms`).
- **From source:** install [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0), then:
  ```powershell
  .\build.ps1
  ```
  Run: `src\ClaudeCodeInstaller.WinForms\bin\Release\net8.0-windows\win-x64\ClaudeCodeInstaller.WinForms.exe`

## Build options

| Command | What it does |
|---------|----------------|
| `.\build.ps1` | Clean, restore, build (Release), run tests |
| `.\build.ps1 -SkipTests` | Same but skips tests |
| `.\build.ps1 -Publish` | Same + publish WinForms to `artifacts\winforms` (win-x64, self-contained) |

## What you need

- **Windows 10/11**
- **.NET 8** (Runtime to run; SDK to build)
- **Node.js** only for Claude Adapter — the app can install it for you when you click **Install Claude Adapter**

## First-time flow

1. **Claude Code:** Click **Install/Update Claude Code** → wait → use **Run Claude Code** or open a new terminal and run `claude`.
2. **Claude Adapter:** Click **Install Claude Adapter** (install Node.js when prompted if needed) → **Run Claude Adapter** when ready.

## Troubleshooting

- **PowerShell script execution:** `Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser`
- **npm not found after installing Node:** Restart the app so it picks up the new PATH.
