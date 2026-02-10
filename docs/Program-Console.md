# Program (Console Application)

## Overview
Console-based installer for Claude Code on Windows 11. Provides a command-line interface for installing and managing Claude Code.

## Location
`src/ClaudeCodeInstaller.Console/Program.cs`

## Entry Point
`Main(string[] args)` - Application entry point

## Features
- Interactive console interface
- Step-by-step installation process
- Progress reporting
- Installation verification
- Option to run Claude Code after installation

## Installation Steps

1. **Check Prerequisites**
   - Verifies Node.js installation
   - Checks for Git (optional)

2. **Download Claude Code**
   - Downloads installer from official source
   - Shows download progress

3. **Install Claude Code**
   - Runs installer silently
   - Handles administrator elevation

4. **Verify Installation**
   - Confirms Claude Code is accessible
   - Provides troubleshooting tips if needed

## User Interaction

- Prompts user to open Node.js download page if missing
- Asks if user wants to run Claude Code after installation
- Provides clear status messages and error handling

## Dependencies
- `ClaudeCodeInstaller.Core` - Core installation logic

## Usage
```powershell
.\ClaudeCodeInstaller.Console.exe
```

## Error Handling
- Catches and displays exceptions
- Provides helpful error messages
- Exits with code 1 on failure

## Notes
- Requires .NET 8.0 Runtime
- Best for automated installations and CI/CD
- Provides detailed console output
