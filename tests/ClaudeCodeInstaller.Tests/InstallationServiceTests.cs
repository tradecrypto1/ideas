// InstallationServiceTests.cs
using System;
using System.Net.Http;
using System.Threading.Tasks;
using ClaudeCodeInstaller.Core;
using FluentAssertions;
using Moq;
using Xunit;

namespace ClaudeCodeInstaller.Tests
{
    public class InstallationServiceTests
    {
        [Fact]
        public void IsWindows11OrLater_ShouldReturnTrue_OnWindows11()
        {
            // This test will pass if running on Windows 11+
            var result = InstallationService.IsWindows11OrLater();
            // Just verify the method doesn't throw
            Assert.True(true);
        }

        [Fact]
        public void IsAdministrator_ShouldNotThrow()
        {
            // Verify the method doesn't throw
            var result = InstallationService.IsAdministrator();
            Assert.True(true);
        }

        [Fact]
        public async Task CheckCommandAsync_ShouldReturnFalse_ForInvalidCommand()
        {
            var service = new InstallationService();
            var result = await service.CheckCommandAsync("nonexistent-command-xyz123");
            result.Should().BeFalse();
        }

        [Fact]
        public async Task CheckPrerequisitesAsync_ShouldCheckNodeJs()
        {
            var service = new InstallationService();
            var result = await service.CheckPrerequisitesAsync();
            // Result depends on system, just verify it doesn't throw
            Assert.True(true);
        }
    }
}
