// InstallationService.cs
using System;
using System.Collections.Generic;
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
        
        public string WorkingDirectory { get; set; }

        public InstallationService(HttpClient? httpClient = null, string? workingDirectory = null)
        {
            _httpClient = httpClient ?? new HttpClient();
            _httpClient.Timeout = TimeSpan.FromMinutes(10);
            WorkingDirectory = workingDirectory ?? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        }

        public async Task<bool> CheckPrerequisitesAsync()
        {
            // Native installer doesn't require Node.js/npm
            // Just check if PowerShell is available (required for native installer)
            return true; // PowerShell is always available on Windows 11
        }

        public async Task<string> DownloadClaudeCodeAsync(string? downloadPath = null, IProgress<int>? progress = null)
        {
            // Claude Code uses native installer script
            // No download needed - installer script handles everything
            progress?.Report(0);
            await Task.Delay(100); // Small delay for UI feedback
            progress?.Report(50);
            
            // Return placeholder - actual installation happens in InstallClaudeCodeAsync
            return "native-install";
        }

        public async Task<bool> IsClaudeCodeRunningAsync()
        {
            try
            {
                var processes = Process.GetProcessesByName("claude");
                return processes.Length > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> StopClaudeCodeAsync()
        {
            try
            {
                var processes = Process.GetProcessesByName("claude");
                bool stopped = false;

                foreach (var process in processes)
                {
                    try
                    {
                        if (!process.HasExited)
                        {
                            process.Kill();
                            using (var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(5)))
                            {
                                try
                                {
                                    await process.WaitForExitAsync(cts.Token);
                                }
                                catch (System.Threading.Tasks.TaskCanceledException)
                                {
                                    // Process didn't exit in time, but we killed it
                                }
                            }
                            stopped = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Failed to stop Claude Code process (PID {process.Id}): {ex.Message}");
                    }
                    finally
                    {
                        process.Dispose();
                    }
                }

                // Also check for claude-code process (legacy npm install)
                var legacyProcesses = Process.GetProcessesByName("claude-code");
                foreach (var process in legacyProcesses)
                {
                    try
                    {
                        if (!process.HasExited)
                        {
                            process.Kill();
                            using (var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(5)))
                            {
                                try
                                {
                                    await process.WaitForExitAsync(cts.Token);
                                }
                                catch (System.Threading.Tasks.TaskCanceledException)
                                {
                                    // Process didn't exit in time, but we killed it
                                }
                            }
                            stopped = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Failed to stop Claude Code process (PID {process.Id}): {ex.Message}");
                    }
                    finally
                    {
                        process.Dispose();
                    }
                }

                if (stopped)
                {
                    await Task.Delay(1000); // Give system time to release resources
                }

                return stopped;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to stop Claude Code: {ex.Message}");
            }
        }

        public async Task InstallClaudeCodeAsync(string installerPath)
        {
            // Check if Claude Code is running and stop it
            bool wasRunning = await IsClaudeCodeRunningAsync();
            if (wasRunning)
            {
                await StopClaudeCodeAsync();
                await Task.Delay(1000); // Wait for processes to fully terminate
            }

            // Install Claude Code using native installer script
            // Uses PowerShell to download and run the installer script
            var startInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = "-NoProfile -ExecutionPolicy Bypass -Command \"irm https://claude.ai/install.ps1 | iex\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = WorkingDirectory
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
                        throw new Exception($"Native installer failed with code {process.ExitCode}. Error: {error}");
                    }

                    // Wait longer for installation to complete and PATH to update
                    await Task.Delay(3000);
                    
                    // Try to manually add Claude Code to PATH for current process
                    // The native installer should have added it to user PATH, but current process won't see it
                    try
                    {
                        string localBin = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "bin");
                        if (Directory.Exists(localBin))
                        {
                            var currentPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process);
                            if (!string.IsNullOrEmpty(currentPath) && !currentPath.Contains(localBin))
                            {
                                // Add to process PATH so we can use it immediately
                                Environment.SetEnvironmentVariable("PATH", currentPath + Path.PathSeparator + localBin, EnvironmentVariableTarget.Process);
                            }
                        }
                    }
                    catch
                    {
                        // Ignore - PATH refresh is best effort
                    }
                }
                else
                {
                    throw new Exception("Failed to start installer process");
                }
            }
        }

        public async Task<bool> VerifyInstallationAsync()
        {
            // First check if files exist directly (faster and more reliable)
            string localBin = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "bin");
            string claudeExe = Path.Combine(localBin, "claude.exe");
            
            if (File.Exists(claudeExe))
            {
                // File exists, but verify it's executable by checking version
                try
                {
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = claudeExe,
                        Arguments = "--version",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        WorkingDirectory = WorkingDirectory
                    };

                    using (var process = Process.Start(startInfo))
                    {
                        if (process != null)
                        {
                            await process.WaitForExitAsync();
                            if (process.ExitCode == 0)
                            {
                                return true;
                            }
                        }
                    }
                }
                catch
                {
                    // If direct execution fails, file might exist but PATH not updated yet
                    // Continue to PATH-based checks
                }
            }

            // Try multiple verification methods via PATH (with longer delays for PATH refresh)
            for (int i = 0; i < 8; i++)
            {
                // Method 1: Check if claude command is available (native installer)
                if (await CheckCommandAsync("claude --version"))
                {
                    return true;
                }

                // Method 2: Fallback - check if claude-code command is available (legacy npm install)
                if (await CheckCommandAsync("claude-code --version"))
                {
                    return true;
                }

                if (i < 7)
                {
                    // Longer delay for PATH to update (Windows can be slow)
                    await Task.Delay(3000);
                }
            }

            // Final check: if file exists, consider it installed even if PATH isn't updated
            return File.Exists(claudeExe);
        }

        public async Task<bool> UninstallClaudeCodeAsync()
        {
            try
            {
                // Check if Claude Code is running and stop it
                bool wasRunning = await IsClaudeCodeRunningAsync();
                if (wasRunning)
                {
                    await StopClaudeCodeAsync();
                    await Task.Delay(1000); // Wait for processes to fully terminate
                }

                // Remove native installer files (Windows)
                string localBin = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "bin");
                string claudeExe = Path.Combine(localBin, "claude.exe");
                string claudeDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "share", "claude");

                bool removed = false;

                // Remove executable
                if (File.Exists(claudeExe))
                {
                    try
                    {
                        File.Delete(claudeExe);
                        removed = true;
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Failed to remove Claude Code executable: {ex.Message}");
                    }
                }

                // Remove share directory
                if (Directory.Exists(claudeDir))
                {
                    try
                    {
                        Directory.Delete(claudeDir, true);
                        removed = true;
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Failed to remove Claude Code directory: {ex.Message}");
                    }
                }

                // Also check for legacy npm installation
                string? legacyPath = await FindClaudeCodePathAsync();
                if (!string.IsNullOrEmpty(legacyPath) && File.Exists(legacyPath))
                {
                    try
                    {
                        // Try to uninstall via npm
                        var startInfo = new ProcessStartInfo
                        {
                            FileName = "cmd.exe",
                            Arguments = "/c npm uninstall -g @anthropic-ai/claude-code",
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            CreateNoWindow = true,
                            WorkingDirectory = WorkingDirectory
                        };

                        using (var process = Process.Start(startInfo))
                        {
                            if (process != null)
                            {
                                await process.StandardOutput.ReadToEndAsync();
                                var error = await process.StandardError.ReadToEndAsync();
                                await process.WaitForExitAsync();
                                
                                if (process.ExitCode == 0)
                                {
                                    removed = true;
                                }
                                else if (!string.IsNullOrEmpty(error) && !error.Contains("not found"))
                                {
                                    throw new Exception($"npm uninstall failed: {error}");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // If npm uninstall fails, try to remove files directly
                        try
                        {
                            File.Delete(legacyPath);
                            removed = true;
                        }
                        catch
                        {
                            throw new Exception($"Failed to remove legacy installation: {ex.Message}");
                        }
                    }
                }

                if (!removed)
                {
                    throw new Exception("Claude Code installation not found or already removed");
                }

                await Task.Delay(1000); // Give system time to update PATH
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to uninstall Claude Code: {ex.Message}");
            }
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
                    WorkingDirectory = WorkingDirectory
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
                    WorkingDirectory = WorkingDirectory
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
                    WorkingDirectory = WorkingDirectory
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
            // First try to find the direct path to claude.exe
            string localBin = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "bin");
            string claudeExe = Path.Combine(localBin, "claude.exe");
            
            ProcessStartInfo startInfo;
            
            if (File.Exists(claudeExe))
            {
                // Use direct path - more reliable than PATH
                startInfo = new ProcessStartInfo
                {
                    FileName = claudeExe,
                    Arguments = arguments ?? "",
                    UseShellExecute = true,
                    CreateNoWindow = false,
                    WindowStyle = ProcessWindowStyle.Normal,
                    WorkingDirectory = WorkingDirectory
                };
            }
            else
            {
                // Fallback: try via PATH (might work if PATH was updated)
                // Also try legacy npm installation
                startInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/k claude {arguments ?? ""}",
                    UseShellExecute = true,
                    CreateNoWindow = false,
                    WindowStyle = ProcessWindowStyle.Normal,
                    WorkingDirectory = WorkingDirectory
                };
            }

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
            else
            {
                throw new Exception($"Failed to start Claude Code. Please ensure it is installed and try restarting the application.");
            }
        }

        public async Task RunClaudeAdapterAsync(string? arguments = null)
        {
            // Run claude-adapter via cmd so npm global bin is on PATH
            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/k claude-adapter {arguments ?? ""}",
                UseShellExecute = true,
                CreateNoWindow = false,
                WindowStyle = ProcessWindowStyle.Normal,
                WorkingDirectory = WorkingDirectory
            };

            var process = Process.Start(startInfo);
            if (process != null)
            {
                try
                {
                    await Task.Delay(500);
                }
                catch
                {
                    // Ignore
                }
            }
            else
            {
                throw new Exception("Failed to start Claude Adapter. Ensure it is installed (Install Claude Adapter button) and Node.js/npm are on PATH.");
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
                    WorkingDirectory = WorkingDirectory
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
                    WorkingDirectory = WorkingDirectory
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

        public async Task<string> GetInstallationPathsAsync()
        {
            var paths = new System.Text.StringBuilder();
            paths.AppendLine("Installation Paths:");
            paths.AppendLine("==================");
            paths.AppendLine();

            // Node.js path
            try
            {
                var nodeInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/c where node",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = WorkingDirectory
                };

                using (var process = Process.Start(nodeInfo))
                {
                    if (process != null)
                    {
                        var output = await process.StandardOutput.ReadToEndAsync();
                        await process.WaitForExitAsync();
                        if (process.ExitCode == 0 && !string.IsNullOrWhiteSpace(output))
                        {
                            var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                            if (lines.Length > 0)
                            {
                                paths.AppendLine($"Node.js: {lines[0].Trim()}");
                            }
                        }
                    }
                }
            }
            catch
            {
                paths.AppendLine("Node.js: Not found");
            }

            // npm path
            string? npmPath = await FindNpmPathAsync();
            paths.AppendLine($"npm: {npmPath ?? "Not found"}");

            // npm version
            try
            {
                var versionInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/c npm --version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = WorkingDirectory
                };

                using (var process = Process.Start(versionInfo))
                {
                    if (process != null)
                    {
                        var output = await process.StandardOutput.ReadToEndAsync();
                        await process.WaitForExitAsync();
                        if (process.ExitCode == 0 && !string.IsNullOrWhiteSpace(output))
                        {
                            string npmVersion = output.Trim();
                            paths.AppendLine($"npm Version: {npmVersion}");
                            
                            // Check if version is outdated
                            var versionStatus = CheckNpmVersion(npmVersion);
                            paths.AppendLine($"  Status: {versionStatus.Status}");
                            if (!string.IsNullOrEmpty(versionStatus.Recommendation))
                            {
                                paths.AppendLine($"  Recommendation: {versionStatus.Recommendation}");
                            }
                        }
                        else
                        {
                            paths.AppendLine("npm Version: Unable to determine");
                        }
                    }
                }
            }
            catch
            {
                paths.AppendLine("npm Version: Unable to check");
            }

            // npm prefix
            string? npmPrefix = await GetNpmPrefixAsync();
            paths.AppendLine($"npm Prefix: {npmPrefix ?? "Not found"}");

            // npm global bin
            try
            {
                var binInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/c npm bin -g",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = WorkingDirectory
                };

                using (var process = Process.Start(binInfo))
                {
                    if (process != null)
                    {
                        var output = await process.StandardOutput.ReadToEndAsync();
                        await process.WaitForExitAsync();
                        if (process.ExitCode == 0 && !string.IsNullOrWhiteSpace(output))
                        {
                            paths.AppendLine($"npm Global Bin: {output.Trim()}");
                        }
                        else
                        {
                            paths.AppendLine("npm Global Bin: Not found");
                        }
                    }
                }
            }
            catch
            {
                paths.AppendLine("npm Global Bin: Not found");
            }

            // AppData npm directory
            string appDataNpm = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "npm");
            paths.AppendLine($"AppData npm: {appDataNpm}");
            paths.AppendLine($"  (Exists: {(Directory.Exists(appDataNpm) ? "Yes" : "No")})");

            // Claude Code installation location
            paths.AppendLine();
            paths.AppendLine($"Claude Code:");
            
            // Check native installation first
            string localBin = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "bin");
            string nativeExe = Path.Combine(localBin, "claude.exe");
            if (File.Exists(nativeExe))
            {
                paths.AppendLine($"  Installation Type: Native Installer");
                paths.AppendLine($"  Executable: {nativeExe}");
                
                // Try to get version
                try
                {
                    var versionInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = "/c claude --version",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        WorkingDirectory = WorkingDirectory
                    };

                    using (var process = Process.Start(versionInfo))
                    {
                        if (process != null)
                        {
                            var output = await process.StandardOutput.ReadToEndAsync();
                            await process.WaitForExitAsync();
                            if (process.ExitCode == 0 && !string.IsNullOrWhiteSpace(output))
                            {
                                paths.AppendLine($"  Version: {output.Trim()}");
                            }
                        }
                    }
                }
                catch
                {
                    paths.AppendLine("  Version: Unable to determine");
                }
            }
            else
            {
                // Check for legacy npm installation
                string? claudeCodePath = await FindClaudeCodePathAsync();
                if (!string.IsNullOrEmpty(claudeCodePath) && File.Exists(claudeCodePath))
                {
                    paths.AppendLine($"  Installation Type: Legacy npm install");
                    paths.AppendLine($"  Executable: {claudeCodePath}");
                }
                else
                {
                    paths.AppendLine("  Executable: Not found");
                    paths.AppendLine("  Installation Type: Not installed");
                }
            }

            return paths.ToString();
        }

        private (string Status, string Recommendation) CheckNpmVersion(string version)
        {
            try
            {
                // Parse version (format: major.minor.patch)
                var parts = version.Split('.');
                if (parts.Length >= 1 && int.TryParse(parts[0], out int major))
                {
                    // npm 10+ is considered current/new
                    // npm 9 is recent but may have updates
                    // npm 8 and below are older
                    if (major >= 10)
                    {
                        return ("✓ Current", "");
                    }
                    else if (major == 9)
                    {
                        return ("⚠ Recent (consider updating)", "npm 10+ is available with latest features and security updates");
                    }
                    else if (major >= 7)
                    {
                        return ("⚠ Outdated", "Consider upgrading to npm 10+ for latest features and security updates. Run: npm install -g npm@latest");
                    }
                    else
                    {
                        return ("✗ Very Old", "Strongly recommend upgrading to npm 10+. Run: npm install -g npm@latest");
                    }
                }
            }
            catch
            {
                // Ignore parsing errors
            }

            return ("Unknown", "");
        }

        public async Task<bool> CheckNpmAvailableAsync()
        {
            return await CheckCommandAsync("npm --version");
        }

        public async Task<bool> IsPluginInstalledAsync(string packageName)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c npm list -g {packageName} --depth=0",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = WorkingDirectory
                };

                using (var process = Process.Start(startInfo))
                {
                    if (process != null)
                    {
                        var output = await process.StandardOutput.ReadToEndAsync();
                        await process.WaitForExitAsync();
                        return process.ExitCode == 0 && output.Contains(packageName);
                    }
                }
            }
            catch
            {
                // Ignore errors
            }

            return false;
        }

        public async Task<bool> InstallPluginAsync(string packageName, IProgress<int>? progress = null)
        {
            // Check if npm is available
            if (!await CheckNpmAvailableAsync())
            {
                throw new Exception("npm is required to install plugins but is not found. Please install Node.js (which includes npm) from https://nodejs.org/");
            }

            try
            {
                progress?.Report(0);

                var startInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c npm install -g {packageName}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = WorkingDirectory
                };

                progress?.Report(50);

                using (var process = Process.Start(startInfo))
                {
                    if (process != null)
                    {
                        var output = await process.StandardOutput.ReadToEndAsync();
                        var error = await process.StandardError.ReadToEndAsync();
                        await process.WaitForExitAsync();

                        progress?.Report(100);

                        if (process.ExitCode == 0)
                        {
                            return true;
                        }
                        else
                        {
                            throw new Exception($"npm install failed: {error}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to install plugin: {ex.Message}");
            }

            return false;
        }

        public async Task<bool> UninstallPluginAsync(string packageName)
        {
            // Check if npm is available
            if (!await CheckNpmAvailableAsync())
            {
                throw new Exception("npm is required to uninstall plugins but is not found. Please install Node.js (which includes npm) from https://nodejs.org/");
            }

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c npm uninstall -g {packageName}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = WorkingDirectory
                };

                using (var process = Process.Start(startInfo))
                {
                    if (process != null)
                    {
                        var output = await process.StandardOutput.ReadToEndAsync();
                        var error = await process.StandardError.ReadToEndAsync();
                        await process.WaitForExitAsync();

                        return process.ExitCode == 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to uninstall plugin: {ex.Message}");
            }

            return false;
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
