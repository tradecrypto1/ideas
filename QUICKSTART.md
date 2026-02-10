# 🚀 QUICK START GUIDE

## For Users (Just Want to Install Claude Code)

### If you have the .exe file already:
1. Double-click `ClaudeCodeInstaller.exe`
2. Follow the prompts
3. Done! Open a terminal and type `claude-code`

### If you're building from source:
1. Install .NET 8.0 SDK: https://dotnet.microsoft.com/download/dotnet/8.0
2. Right-click `build.ps1` → Run with PowerShell
3. Choose build option (option 2 recommended for distribution)
4. Run the generated .exe file

---

## What You Need Before Installing Claude Code

✅ **Node.js v18+** - The installer will check and help you install this
✅ **Internet connection** - To download Claude Code
✅ **Windows 11** (or Windows 10 with updates)

---

## Super Simple Instructions

### Method 1: PowerShell Build Script (EASIEST)
```powershell
# Right-click build.ps1 and select "Run with PowerShell"
# OR open PowerShell in this folder and run:
.\build.ps1
```

### Method 2: Manual Build
```powershell
# For quick build:
dotnet build --configuration Release

# For standalone single-file (recommended):
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
```

---

## After Building

Your installer will be in one of these locations:

**Quick build:**
- `bin\Release\net8.0\ClaudeCodeInstaller.exe`
- Smaller file (~200 KB)
- Requires .NET runtime on the PC where you run it

**Single-file build:**
- `bin\Release\net8.0\win-x64\publish\ClaudeCodeInstaller.exe`
- Larger file (~70 MB)
- Runs on any Windows 11 PC (no .NET needed)
- **Best for sharing with others!**

---

## Troubleshooting Build Issues

**"dotnet command not found"**
→ Install .NET 8.0 SDK from https://dotnet.microsoft.com/download/dotnet/8.0

**"Cannot run scripts" error in PowerShell**
→ Run: `Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser`

**Build succeeds but exe won't run**
→ You built the quick version - either install .NET Runtime or build the single-file version

---

## What This Installer Does

✨ Checks if Node.js is installed (required for Claude Code)
✨ Downloads Claude Code from Anthropic's servers  
✨ Installs it automatically (silent installation)
✨ Verifies everything works
✨ Tells you exactly what to do next

**No manual configuration. No complex steps. Just works!**

---

## Support

- Claude Code docs: https://docs.claude.com
- Need help? https://support.claude.com
- Node.js issues? https://nodejs.org/

---

Made with ❤️ to save you time and hassle!
