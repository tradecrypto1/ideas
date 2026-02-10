# Program (WinForms Application)

## Overview
Entry point for the WinForms installer application. Initializes the Windows Forms application and displays the main form.

## Location
`src/ClaudeCodeInstaller.WinForms/Program.cs`

## Entry Point
`Main()` - Application entry point (STAThread required for WinForms)

## Responsibilities
- Enable visual styles
- Set compatible text rendering
- Launch MainForm

## Dependencies
- `System.Windows.Forms` - Windows Forms framework
- `ClaudeCodeInstaller.WinForms.MainForm` - Main application form

## Notes
- Uses STAThread attribute (required for WinForms)
- Minimal code - delegates to MainForm for functionality
