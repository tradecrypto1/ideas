// InstallerUpdateService.cs
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.Versioning;
using System.Text.Json;
using System.Threading.Tasks;

namespace ClaudeCodeInstaller.Core
{
    [SupportedOSPlatform("windows")]
    public class InstallerUpdateService
    {
        private readonly HttpClient _httpClient;
        // Get repo from environment variable or use default
        // Set via: GITHUB_REPOSITORY environment variable (e.g., "owner/repo")
        private readonly string _githubRepo;
        private const string GITHUB_API_BASE = "https://api.github.com/repos";
        
        public InstallerUpdateService(HttpClient? httpClient = null, string? githubRepo = null)
        {
            _httpClient = httpClient ?? new HttpClient();
            _httpClient.Timeout = TimeSpan.FromMinutes(5);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "ClaudeCodeInstaller");
            
            // Try to get repo from parameter, environment, or use default
            _githubRepo = githubRepo ?? 
                         Environment.GetEnvironmentVariable("GITHUB_REPOSITORY") ?? 
                         "tradecrypto1/ideas";
        }

        public async Task<VersionInfo?> CheckForInstallerUpdateAsync(string currentVersion)
        {
            try
            {
                var url = $"{GITHUB_API_BASE}/{_githubRepo}/releases/latest";
                var response = await _httpClient.GetStringAsync(url);
                
                using (JsonDocument doc = JsonDocument.Parse(response))
                {
                    var root = doc.RootElement;
                    var latestVersion = root.GetProperty("tag_name").GetString()?.TrimStart('v') ?? "";
                    var downloadUrl = "";
                    var releaseDate = DateTime.UtcNow;

                    // Find Windows executable asset
                    // Look for .exe files, prefer WinForms but accept any .exe
                    if (root.TryGetProperty("assets", out var assets))
                    {
                        string? winFormsExe = null;
                        foreach (var asset in assets.EnumerateArray())
                        {
                            var name = asset.GetProperty("name").GetString() ?? "";
                            if (name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                            {
                                if (name.Contains("WinForms", StringComparison.OrdinalIgnoreCase))
                                {
                                    downloadUrl = asset.GetProperty("browser_download_url").GetString() ?? "";
                                    break; // Prefer WinForms exe
                                }
                                else if (winFormsExe == null)
                                {
                                    winFormsExe = asset.GetProperty("browser_download_url").GetString();
                                }
                            }
                        }
                        // If no WinForms exe found, use any .exe found
                        if (string.IsNullOrEmpty(downloadUrl) && !string.IsNullOrEmpty(winFormsExe))
                        {
                            downloadUrl = winFormsExe;
                        }
                    }

                    // Parse release date
                    if (root.TryGetProperty("published_at", out var publishedAt))
                    {
                        if (DateTime.TryParse(publishedAt.GetString(), out var date))
                        {
                            releaseDate = date;
                        }
                    }

                    if (string.IsNullOrEmpty(downloadUrl))
                    {
                        return null;
                    }

                    var isNewer = CompareVersions(latestVersion, currentVersion) > 0;

                    return new VersionInfo
                    {
                        Version = latestVersion,
                        ReleaseDate = releaseDate,
                        DownloadUrl = downloadUrl,
                        IsLatest = !isNewer
                    };
                }
            }
            catch
            {
                return null;
            }
        }

        public async Task<string> DownloadInstallerUpdateAsync(string downloadUrl, IProgress<int>? progress = null)
        {
            var tempPath = Path.Combine(Path.GetTempPath(), $"ClaudeCodeInstaller-{Guid.NewGuid()}.exe");

            using (var response = await _httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead))
            {
                response.EnsureSuccessStatusCode();

                var totalBytes = response.Content.Headers.ContentLength ?? -1L;
                var canReportProgress = totalBytes != -1;

                using (var contentStream = await response.Content.ReadAsStreamAsync())
                using (var fileStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
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

            return tempPath;
        }

        public async Task<bool> InstallUpdateAsync(string installerPath)
        {
            try
            {
                // Get current executable path
                var currentExe = Process.GetCurrentProcess().MainModule?.FileName;
                if (string.IsNullOrEmpty(currentExe))
                {
                    return false;
                }

                // Create update script
                var scriptPath = Path.Combine(Path.GetTempPath(), "update-installer.bat");
                var scriptContent = $@"
@echo off
timeout /t 2 /nobreak >nul
copy /Y ""{installerPath}"" ""{currentExe}""
del ""{installerPath}""
del ""%~f0""
start """" ""{currentExe}""
";

                await File.WriteAllTextAsync(scriptPath, scriptContent);

                // Run update script
                var startInfo = new ProcessStartInfo
                {
                    FileName = scriptPath,
                    UseShellExecute = true,
                    Verb = "runas", // Run as admin
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                Process.Start(startInfo);
                
                // Exit current application
                await Task.Delay(1000);
                Environment.Exit(0);
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        private int CompareVersions(string version1, string version2)
        {
            var v1Parts = version1.Split('.');
            var v2Parts = version2.Split('.');

            for (int i = 0; i < Math.Max(v1Parts.Length, v2Parts.Length); i++)
            {
                var v1Part = i < v1Parts.Length ? int.Parse(v1Parts[i]) : 0;
                var v2Part = i < v2Parts.Length ? int.Parse(v2Parts[i]) : 0;

                if (v1Part > v2Part) return 1;
                if (v1Part < v2Part) return -1;
            }

            return 0;
        }
    }
}
