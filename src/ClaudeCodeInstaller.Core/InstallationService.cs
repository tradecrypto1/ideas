// InstallationService.cs
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Runtime.Versioning;

namespace ClaudeCodeInstaller.Core
{
    public class InstallationService
    {
        private readonly HttpClient _httpClient;
        // Claude Code is installed via npm, not direct download
        // Using npm install method as it's the official installation method
        private const string VERSION_CHECK_URL = "https://api.github.com/repos/anthropics/claude-code/releases/latest";

        public InstallationService(HttpClient? httpClient = null)
        {
            _httpClient = httpClient ?? new HttpClient();
            _httpClient.Timeout = TimeSpan.FromMinutes(10);
        }

        public async Task<bool> CheckPrerequisitesAsync()
        {
            // Check for both Node.js and npm (npm comes with Node.js)
            bool hasNode = await CheckCommandAsync("node --version");
            bool hasNpm = await CheckCommandAsync("npm --version");
            return hasNode && hasNpm;
        }

        public async Task<string> DownloadClaudeCodeAsync(string? downloadPath = null, IProgress<int>? progress = null)
        {
            // Claude Code is installed via npm, not downloaded as an exe
            // This method now installs via npm
            progress?.Report(0);
            
            // Check if npm is available
            if (!await CheckCommandAsync("npm --version"))
            {
                throw new Exception("npm is required but not found. Please install Node.js which includes npm.");
            }

            progress?.Report(50);
            
            // Return a placeholder path since we're using npm install
            return "npm-install";
        }

        public async Task InstallClaudeCodeAsync(string installerPath)
        {
            // Install Claude Code via npm
            var startInfo = new ProcessStartInfo
            {
                FileName = "npm",
                Arguments = "install -g @anthropic-ai/claude-code",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (var process = Process.Start(startInfo))
            {
                if (process != null)
                {
                    // Read output for progress
                    string? output = await process.StandardOutput.ReadToEndAsync();
                    string? error = await process.StandardError.ReadToEndAsync();
                    
                    await process.WaitForExitAsync();

                    if (process.ExitCode != 0)
                    {
                        throw new Exception($"npm install failed with code {process.ExitCode}. Error: {error}");
                    }

                    await Task.Delay(2000); // Wait for PATH to update
                }
                else
                {
                    throw new Exception("Failed to start npm process");
                }
            }
        }

        public async Task<bool> VerifyInstallationAsync()
        {
            for (int i = 0; i < 3; i++)
            {
                if (await CheckCommandAsync("claude-code --version"))
                {
                    return true;
                }

                if (i < 2)
                {
                    await Task.Delay(2000);
                }
            }

            return false;
        }

        public async Task<bool> CheckCommandAsync(string command)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c {command}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(startInfo))
                {
                    if (process != null)
                    {
                        await process.WaitForExitAsync();
                        return process.ExitCode == 0;
                    }
                }
            }
            catch
            {
                // Ignore errors
            }

            return false;
        }

        public async Task<string?> GetLatestVersionAsync()
        {
            try
            {
                var response = await _httpClient.GetStringAsync(VERSION_CHECK_URL);
                await Task.CompletedTask; // Ensure async
                // Parse JSON to get version - simplified for now
                // In production, use proper JSON parsing
                return "latest";
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> IsUpdateAvailableAsync(string currentVersion)
        {
            var latestVersion = await GetLatestVersionAsync();
            if (latestVersion == null) return false;
            
            // Simple version comparison - enhance as needed
            return latestVersion != currentVersion;
        }

        public async Task RunClaudeCodeAsync(string? arguments = null)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "claude-code",
                Arguments = arguments ?? "",
                UseShellExecute = true
            };

            Process.Start(startInfo);
        }

        public static bool IsWindows11OrLater()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return false;

            var version = Environment.OSVersion.Version;
            return version.Major >= 10 && version.Build >= 22000;
        }

        [SupportedOSPlatform("windows")]
        public static bool IsAdministrator()
        {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }
    }
}
