# HealthCheckService

## Overview
`HealthCheckService` provides health checking functionality for the Claude Code installation and system environment.

## Location
`src/ClaudeCodeInstaller.Core/HealthCheckService.cs`

## Responsibilities
- Checking overall system health
- Verifying Claude Code installation status
- Checking prerequisites
- Validating OS version
- Checking administrator privileges

## Key Methods

### `CheckHealthAsync()`
Performs a comprehensive health check of the system and Claude Code installation.

**Returns:** `Task<HealthCheckResult>` - Health check results

## HealthCheckResult Properties

- `Timestamp`: DateTime when the check was performed
- `IsHealthy`: Overall health status (true if all checks pass)
- `IsClaudeCodeInstalled`: Whether Claude Code is installed and accessible
- `HasPrerequisites`: Whether required prerequisites (Node.js) are installed
- `IsWindows11`: Whether the system is Windows 11 or later
- `HasAdminRights`: Whether the process has administrator privileges
- `ErrorMessage`: Optional error message if health check failed

## Dependencies
- `InstallationService` - For checking installation status and prerequisites

## Usage Example
```csharp
var installationService = new InstallationService();
var healthCheckService = new HealthCheckService(installationService);
var result = await healthCheckService.CheckHealthAsync();

if (result.IsHealthy)
{
    Console.WriteLine("System is healthy!");
}
else
{
    Console.WriteLine($"Issues found: {result.ErrorMessage}");
}
```

## Health Check Criteria

A system is considered healthy if:
1. Claude Code is installed and accessible
2. Prerequisites (Node.js) are installed
3. System is Windows 11 or later

Administrator rights are checked but not required for healthy status.

## Notes
- Health checks are non-destructive (read-only operations)
- Results are point-in-time snapshots
- May take a few seconds to complete due to command execution
