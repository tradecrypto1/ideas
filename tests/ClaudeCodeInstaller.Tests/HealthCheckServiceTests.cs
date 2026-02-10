// HealthCheckServiceTests.cs
using System.Runtime.Versioning;
using System.Threading.Tasks;
using ClaudeCodeInstaller.Core;
using FluentAssertions;
using Moq;
using Xunit;

namespace ClaudeCodeInstaller.Tests
{
    public class HealthCheckServiceTests
    {
        [Fact]
        [SupportedOSPlatform("windows")]
        public async Task CheckHealthAsync_ShouldReturnHealthCheckResult()
        {
            var installationService = new InstallationService();
            var healthCheckService = new HealthCheckService(installationService);
            
            var startTime = System.DateTime.UtcNow;
            var result = await healthCheckService.CheckHealthAsync();
            var endTime = System.DateTime.UtcNow;
            
            result.Should().NotBeNull();
            // Check that timestamp is between start and end time (with buffer for async operations)
            result.Timestamp.Should().BeOnOrAfter(startTime.AddSeconds(-1));
            result.Timestamp.Should().BeOnOrBefore(endTime.AddSeconds(1));
        }
    }
}
