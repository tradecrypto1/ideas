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
        private const string CLAUDE_CODE_DOWNLOAD_URL = "https://storage.googleapis.com/osprey-downloads-c1ecf5a2/claude_code_latest_windows_x86_64.exe";
        private const string VERSION_CHECK_URL = "https://api.github.com/repos/anthropics/claude-code/releases/latest";

        public InstallationService(HttpClient? httpClient = null)
        {
            _httpClient = httpClient ?? new HttpClient();
            _httpClient.Timeout = TimeSpan.FromMinutes(10);
        }

        public async Task<bool> CheckPrerequisitesAsync()
        {
            return await CheckCommandAsync("node --version");
        }

        public async Task<string> DownloadClaudeCodeAsync(string? downloadPath = null, IProgress<int>? progress = null)
        {
            downloadPath ??= Path.Combine(Path.GetTempPath(), "claude-code-installer.exe");

            using (var response = await _httpClient.GetAsync(CLAUDE_CODE_DOWNLOAD_URL, HttpCompletionOption.ResponseHeadersRead))
            {
                response.EnsureSuccessStatusCode();

                var totalBytes = response.Content.Headers.ContentLength ?? -1L;
                var canReportProgress = totalBytes != -1;

                using (var contentStream = await response.Content.ReadAsStreamAsync())
                using (var fileStream = new FileStream(downloadPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                {
                    var totalRead = 0L;
                    var buffer = new byte[8192];
                    var isMoreToRead = true;

                    do
                    {
                        var read = await contentStream.ReadAsync(buffer, 0, buffer.Length);
                        if (read == 0)
                        {
                            isMoreToRead = false;
                        }
                        else
                        {
                            await fileStream.WriteAsync(buffer, 0, read);
                            totalRead += read;

                            if (canReportProgress && progress != null)
                            {
                                var percentage = (int)((totalRead * 100) / totalBytes);
                                progress.Report(percentage);
                            }
                        }
                    } while (isMoreToRead);
                }
            }

            return downloadPath;
        }

        public async Task InstallClaudeCodeAsync(string installerPath)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = installerPath,
                Arguments = "/VERYSILENT /SUPPRESSMSGBOXES /NORESTART /SP-",
                UseShellExecute = true,
                Verb = "runas"
            };

            using (var process = Process.Start(startInfo))
            {
                if (process != null)
                {
                    await process.WaitForExitAsync();

                    if (process.ExitCode != 0)
                    {
                        throw new Exception($"Installer exited with code {process.ExitCode}");
                    }

                    await Task.Delay(2000); // Wait for PATH to update
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
            await Task.CompletedTask; // Ensure async
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
