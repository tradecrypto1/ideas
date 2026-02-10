// HealthCheckServiceTests.cs
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
        public async Task CheckHealthAsync_ShouldReturnHealthCheckResult()
        {
            var installationService = new InstallationService();
            var healthCheckService = new HealthCheckService(installationService);
            
            var result = await healthCheckService.CheckHealthAsync();
            
            result.Should().NotBeNull();
            result.Timestamp.Should().BeCloseTo(System.DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }
    }
}
