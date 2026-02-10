# VersionInfo

## Overview
`VersionInfo` represents version information for Claude Code releases.

## Location
`src/ClaudeCodeInstaller.Core/VersionInfo.cs`

## Properties

- `Version`: Version string (e.g., "1.0.0")
- `ReleaseDate`: DateTime when the version was released
- `DownloadUrl`: URL to download this version
- `IsLatest`: Whether this is the latest available version

## Usage Example
```csharp
var versionInfo = new VersionInfo
{
    Version = "1.0.0",
    ReleaseDate = DateTime.UtcNow,
    DownloadUrl = "https://example.com/claude-code.exe",
    IsLatest = true
};
```

## Notes
- Used for version comparison and update checking
- Dates are stored in UTC
