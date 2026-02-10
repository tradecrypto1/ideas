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
            // Always use cmd.exe to run npm since npm on Windows is a .cmd file
            // cmd.exe properly handles .cmd/.bat files and PATH resolution
            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/c npm install -g @anthropic-ai/claude-code",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
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
            // Try multiple verification methods
            for (int i = 0; i < 5; i++)
            {
                // Method 1: Check if claude-code command is available
                if (await CheckCommandAsync("claude-code --version"))
                {
                    return true;
                }

                // Method 2: Check npm global list for @anthropic-ai/claude-code
                if (await CheckNpmGlobalPackageAsync())
                {
                    return true;
                }

                if (i < 4)
                {
                    await Task.Delay(2000);
                }
            }

            return false;
        }

        private async Task<bool> CheckNpmGlobalPackageAsync()
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/c npm list -g @anthropic-ai/claude-code --depth=0",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
                };

                using (var process = Process.Start(startInfo))
                {
                    if (process != null)
                    {
                        var output = await process.StandardOutput.ReadToEndAsync();
                        await process.WaitForExitAsync();
                        
                        // Check if package is listed (npm list returns 0 if found)
                        if (process.ExitCode == 0 && output.Contains("@anthropic-ai/claude-code"))
                        {
                            return true;
                        }
                    }
                }
            }
            catch
            {
                // Ignore errors
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
                    CreateNoWindow = true,
                    WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
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

        private async Task<string?> FindNpmPathAsync()
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/c where npm",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
                };

                using (var process = Process.Start(startInfo))
                {
                    if (process != null)
                    {
                        var output = await process.StandardOutput.ReadToEndAsync();
                        await process.WaitForExitAsync();
                        
                        if (process.ExitCode == 0 && !string.IsNullOrWhiteSpace(output))
                        {
                            // Get first line (where command returns first match)
                            var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                            if (lines.Length > 0)
                            {
                                return lines[0].Trim();
                            }
                        }
                    }
                }
            }
            catch
            {
                // Ignore errors
            }

            return null;
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
            if (latestVersion == null) 
            {
                await Task.CompletedTask;
                return false;
            }
            
            // Simple version comparison - enhance as needed
            return latestVersion != currentVersion;
        }

        public async Task RunClaudeCodeAsync(string? arguments = null)
        {
            // Use npx to run claude-code - this works regardless of PATH
            // npx will find and execute the globally installed package
            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/k npx -y @anthropic-ai/claude-code {arguments ?? ""}",
                UseShellExecute = true,
                CreateNoWindow = false,
                WindowStyle = ProcessWindowStyle.Normal,
                WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
            };

            var process = Process.Start(startInfo);
            if (process != null)
            {
                // Give process time to start
                try
                {
                    await Task.Delay(500);
                }
                catch
                {
                    // Ignore errors
                }
            }
        }

        private async Task<string?> FindClaudeCodePathAsync()
        {
            try
            {
                // Get npm's global bin directory
                var binInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/c npm bin -g",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
                };

                using (var process = Process.Start(binInfo))
                {
                    if (process != null)
                    {
                        var output = await process.StandardOutput.ReadToEndAsync();
                        await process.WaitForExitAsync();
                        
                        if (process.ExitCode == 0 && !string.IsNullOrWhiteSpace(output))
                        {
                            string binDir = output.Trim();
                            // Check for claude-code in the bin directory
                            string[] possiblePaths = new[]
                            {
                                Path.Combine(binDir, "claude-code.cmd"),
                                Path.Combine(binDir, "claude-code.exe"),
                                Path.Combine(binDir, "claude-code")
                            };

                            foreach (var path in possiblePaths)
                            {
                                if (File.Exists(path))
                                {
                                    return path;
                                }
                            }
                        }
                    }
                }

                // Fallback: Check AppData\npm (common Windows location)
                string appDataNpm = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "npm");
                string[] fallbackPaths = new[]
                {
                    Path.Combine(appDataNpm, "claude-code.cmd"),
                    Path.Combine(appDataNpm, "claude-code.exe"),
                    Path.Combine(appDataNpm, "claude-code")
                };

                foreach (var path in fallbackPaths)
                {
                    if (File.Exists(path))
                    {
                        return path;
                    }
                }
            }
            catch
            {
                // Ignore errors
            }

            return null;
        }

        private async Task<string?> GetNpmPrefixAsync()
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/c npm config get prefix",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
                };

                using (var process = Process.Start(startInfo))
                {
                    if (process != null)
                    {
                        var output = await process.StandardOutput.ReadToEndAsync();
                        await process.WaitForExitAsync();
                        
                        if (process.ExitCode == 0 && !string.IsNullOrWhiteSpace(output))
                        {
                            return output.Trim();
                        }
                    }
                }
            }
            catch
            {
                // Ignore errors
            }

            return null;
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
