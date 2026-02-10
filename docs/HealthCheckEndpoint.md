# HealthCheckEndpoint

## Overview
HTTP endpoint service that provides health check functionality via REST API. Can be used for monitoring and integration with orchestration systems.

## Location
`src/ClaudeCodeInstaller.Core/HealthCheckEndpoint.cs`

## Features
- HTTP GET endpoint at `/health`
- Returns JSON health status
- Non-blocking request handling
- Proper HTTP status codes

## API Endpoint

### GET /health

Returns health check status in JSON format.

**Response (200 OK - Healthy):**
```json
{
  "status": "healthy",
  "timestamp": "2026-02-10T12:00:00Z",
  "checks": {
    "claudeCodeInstalled": true,
    "prerequisites": true,
    "windows11": true,
    "adminRights": false
  },
  "error": null
}
```

**Response (503 Service Unavailable - Unhealthy):**
```json
{
  "status": "unhealthy",
  "timestamp": "2026-02-10T12:00:00Z",
  "checks": {
    "claudeCodeInstalled": false,
    "prerequisites": true,
    "windows11": true,
    "adminRights": false
  },
  "error": "Claude Code is not installed"
}
```

## Methods

### `StartAsync(string url)`
Starts the HTTP listener on the specified URL.

**Parameters:**
- `url`: HTTP URL to listen on (default: "http://localhost:8080/health")

**Returns:** `Task` - Completes when listener is started

### `Stop()`
Stops the HTTP listener and closes connections.

## Usage Example
```csharp
var installationService = new InstallationService();
var healthCheckService = new HealthCheckService(installationService);
var endpoint = new HealthCheckEndpoint(healthCheckService);

await endpoint.StartAsync("http://localhost:8080/health");

// Health check available at http://localhost:8080/health

// Later, stop the endpoint
endpoint.Stop();
```

## Dependencies
- `HealthCheckService` - For performing health checks
- `System.Net.HttpListener` - For HTTP server functionality

## Notes
- Runs asynchronously in background thread
- Handles multiple concurrent requests
- Returns appropriate HTTP status codes
- JSON responses are formatted for readability

## Security Considerations
- Default binding is localhost only
- Consider authentication for production use
- May need firewall rules for external access
