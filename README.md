# Claude Code Installer & Runner (Windows)

WinForms app to install, run, and manage **Claude Code** (official native installer) and **Claude Adapter** (npm) on Windows 10/11.

## Features

- **Claude Code** (no Node.js required)
  - Install/update via official script: `irm https://claude.ai/install.ps1 | iex`
  - Run Claude Code from the app
  - Uninstall (stops processes, removes `%USERPROFILE%\.local\bin\claude` and related files)
  - Check for installer updates
- **Claude Adapter**
  - Install/run **Claude Adapter** via npm; if Node.js is missing, the app can install Node.js LTS for you (winget or MSI fallback)
- **Advanced** tab: working directory, paths
- **Health Check**: installation status, prerequisites, OS

## Prerequisites

- **Windows 10/11**
- **.NET 8.0** Runtime or SDK — [Download](https://dotnet.microsoft.com/download/dotnet/8.0)  
  - Runtime: to run the pre-built app  
  - SDK: to build from source  
- **Node.js** is only required for Claude Adapter; the app can install it if missing.

## Quick start (pre-built)

1. Get `ClaudeCodeInstaller.WinForms.exe` (e.g. from [Releases](https://github.com/your-org/ideas/releases) or build with `.\build.ps1 -Publish`).
2. Run the exe.
3. Use **Install/Update Claude Code** or **Install Claude Adapter** as needed, then **Run** from the app.

## Build from source

### Full rebuild (recommended)

```powershell
.\build.ps1
```

- Clean, restore, build (Release), run tests.  
- **Skip tests:** `.\build.ps1 -SkipTests`  
- **Publish WinForms to `artifacts\winforms`:** `.\build.ps1 -Publish`

### Manual build

```powershell
dotnet restore
dotnet build --configuration Release
```

Run: `src\ClaudeCodeInstaller.WinForms\bin\Release\net8.0-windows\win-x64\ClaudeCodeInstaller.WinForms.exe`

### Single-file publish (distribution)

```powershell
dotnet publish src/ClaudeCodeInstaller.WinForms/ClaudeCodeInstaller.WinForms.csproj -c Release -r win-x64 --self-contained true -o artifacts/winforms
```

## What the app does

| Action | Description |
|--------|-------------|
| **Install/Update Claude Code** | Runs official PowerShell installer; installs to `%USERPROFILE%\.local\bin\claude.exe`. Stops any running Claude Code first. |
| **Run Claude Code** | Launches `claude` (by path if needed so it works before PATH is updated). |
| **Uninstall Claude Code** | Stops `claude`/`claude-code` processes, removes install dir and exe. |
| **Install Claude Adapter** | Global npm install of `claude-adapter`. If Node.js is missing, offers to install Node.js LTS (winget → MSI fallback). |
| **Run Claude Adapter** | Runs `claude-adapter` in a new console. |
| **Health Check** | Reports Claude Code install status, paths, OS. |

## Troubleshooting

| Issue | What to do |
|-------|------------|
| **"claude" not recognized** | Install via the app, then open a **new** terminal, or use **Run Claude Code** in the app (uses full path). |
| **npm not found for Claude Adapter** | Click **Install Claude Adapter**; when prompted, choose **Yes** to install Node.js LTS. Restart the app if npm still isn’t found. |
| **Access denied** | Run the app as Administrator if install/uninstall fails. |
| **Build: dotnet not found** | Install [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0). |

## Repo layout

- `src/ClaudeCodeInstaller.Core` — install/uninstall/verify Claude Code, Node.js install, npm/Claude Adapter, health.
- `src/ClaudeCodeInstaller.WinForms` — Main form (tabs: Main, Advanced).
- `tests/ClaudeCodeInstaller.Tests` — unit tests.
- `build.ps1` — full rebuild script.
- `docs/` — per-component and project docs.

## CI/CD

GitHub Actions (`.github/workflows/ci.yml`): build, test, security checks, publish WinForms artifact, virus scan; on push to `main`, RC tag and GitHub Release with changelog.

## License

This application is free for anyone to use, modify, and distribute under the **MIT License**. See [LICENSE](LICENSE) in the repo root.

Third-party components (e.g. Newtonsoft.Json, test libraries) have their own licenses; see [docs/THIRD_PARTY_LICENSES.md](docs/THIRD_PARTY_LICENSES.md) for details.

Claude Code is by Anthropic; Claude Adapter by its respective authors. This installer is not affiliated with or endorsed by Anthropic.
