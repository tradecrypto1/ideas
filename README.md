# Claude Code Easy Installer for Windows 11

**The simplest way to install Claude Code on Windows 11!** 🚀

This automated installer handles everything for you:
- ✅ Checks for required prerequisites (Node.js)
- ✅ Downloads the latest Claude Code installer
- ✅ Installs Claude Code automatically
- ✅ Verifies the installation
- ✅ Provides clear next steps

## Prerequisites

Before running the installer, you need:

1. **Windows 11** (or Windows 10 with recent updates)
2. **.NET 8.0 Runtime or SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
   - If you just want to run the installer: Download .NET 8.0 Runtime
   - If you want to build from source: Download .NET 8.0 SDK

## Quick Start (Using Pre-Built Executable)

### Option A: Run Pre-Built Installer (Easiest)

If someone has already built the executable for you:

1. Download `ClaudeCodeInstaller.exe`
2. Double-click to run
3. Follow the on-screen instructions
4. That's it! 🎉

## Building from Source

### Step 1: Install .NET SDK

1. Download and install [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
2. Verify installation by opening PowerShell and running:
   ```powershell
   dotnet --version
   ```

### Step 2: Build the Installer

1. Open PowerShell or Command Prompt
2. Navigate to the folder containing these files:
   ```powershell
   cd C:\path\to\ClaudeCodeInstaller
   ```

3. Build the project:
   ```powershell
   dotnet build --configuration Release
   ```

4. The executable will be in: `bin\Release\net8.0\ClaudeCodeInstaller.exe`

### Step 3: Run the Installer

Run the built executable:
```powershell
.\bin\Release\net8.0\ClaudeCodeInstaller.exe
```

Or for a single-file executable (recommended for distribution):
```powershell
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
```

The single-file executable will be in: `bin\Release\net8.0\win-x64\publish\ClaudeCodeInstaller.exe`

## What the Installer Does

1. **Checks Prerequisites**
   - Verifies Node.js is installed (required for Claude Code)
   - Checks for Git (optional but recommended)
   - Opens download pages if anything is missing

2. **Downloads Claude Code**
   - Fetches the latest Windows installer from Anthropic
   - Shows download progress
   - Saves to a temporary location

3. **Installs Claude Code**
   - Runs the installer silently
   - Handles administrator elevation if needed
   - Cleans up temporary files

4. **Verifies Installation**
   - Confirms the `claude-code` command is available
   - Provides troubleshooting tips if needed

## After Installation

Once installation is complete:

1. **Open a new terminal** (Command Prompt, PowerShell, or Windows Terminal)
2. Run:
   ```powershell
   claude-code
   ```
3. Follow the authentication prompts to link your Anthropic account
4. Start using Claude Code!

## Troubleshooting

### "Node.js not found"
- Install Node.js from https://nodejs.org/ (v18 or later required)
- Restart the installer after installing Node.js

### "claude-code command not found" after installation
- Close and reopen your terminal
- The PATH environment variable needs to refresh
- If still not working, restart your computer

### "Access denied" errors
- Right-click the installer and select "Run as Administrator"

### Download fails
- Check your internet connection
- Verify you can access: https://storage.googleapis.com
- Try disabling VPN or proxy temporarily

## What is Claude Code?

Claude Code is a command-line tool that lets you delegate coding tasks to Claude AI directly from your terminal. Perfect for:
- 🔨 Building applications and scripts
- 🐛 Debugging code
- 📝 Writing documentation
- 🔄 Refactoring projects
- 🎓 Learning programming concepts

## System Requirements

- **OS**: Windows 11 (or Windows 10 version 1909 or later)
- **Node.js**: v18.0.0 or later
- **.NET**: 8.0 Runtime (only to run this installer)
- **Disk Space**: ~200 MB for Claude Code
- **Internet**: Required for download and Claude Code authentication

## Advanced Options

### Silent Build and Run
```powershell
# Build
dotnet build -c Release

# Run with no user interaction (auto-accepts defaults)
.\bin\Release\net8.0\ClaudeCodeInstaller.exe
```

### Customize Download Location
Edit the `CLAUDE_CODE_DOWNLOAD_URL` constant in `ClaudeCodeInstaller.cs` if you need to use a different source.

## Security Notes

- This installer downloads Claude Code from Anthropic's official CDN
- The installer requests administrator privileges only when needed
- All downloads are verified during installation
- No data is collected or transmitted by this installer

## Support

- **Claude Code Documentation**: https://docs.claude.com
- **Support**: https://support.claude.com
- **Node.js Help**: https://nodejs.org/

## License

This installer is provided as-is for convenience. Claude Code itself is subject to Anthropic's terms of service.

---

**Made with ❤️ to make Claude Code installation 100000% easier!**
