// HealthCheckService.cs
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ClaudeCodeInstaller.Core
{
    public class HealthCheckService
    {
        private readonly InstallationService _installationService;

        public HealthCheckService(InstallationService installationService)
        {
            _installationService = installationService;
        }

        public async Task<HealthCheckResult> CheckHealthAsync()
        {
            var result = new HealthCheckResult
            {
                Timestamp = DateTime.UtcNow
            };

            // Check if Claude Code is installed
            result.IsClaudeCodeInstalled = await _installationService.VerifyInstallationAsync();

            // Check prerequisites
            result.HasPrerequisites = await _installationService.CheckPrerequisitesAsync();

            // Check OS version
            result.IsWindows11 = InstallationService.IsWindows11OrLater();

            // Check admin privileges
            result.HasAdminRights = InstallationService.IsAdministrator();

            // Overall health
            result.IsHealthy = result.IsClaudeCodeInstalled && result.HasPrerequisites && result.IsWindows11;

            return result;
        }
    }

    public class HealthCheckResult
    {
        public DateTime Timestamp { get; set; }
        public bool IsHealthy { get; set; }
        public bool IsClaudeCodeInstalled { get; set; }
        public bool HasPrerequisites { get; set; }
        public bool IsWindows11 { get; set; }
        public bool HasAdminRights { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
