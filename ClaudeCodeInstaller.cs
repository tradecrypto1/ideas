using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Security.Principal;
using System.Text.RegularExpressions;

namespace ClaudeCodeInstaller
{
    class Program
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private const string CLAUDE_CODE_DOWNLOAD_URL = "https://storage.googleapis.com/osprey-downloads-c1ecf5a2/claude_code_latest_windows_x86_64.exe";
        
        static async Task Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔════════════════════════════════════════════════════════╗");
            Console.WriteLine("║     Claude Code Installation Wizard for Windows 11     ║");
            Console.WriteLine("║                    By Anthropic                        ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();

            if (!IsWindows11OrLater())
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("⚠️  Warning: This installer is optimized for Windows 11.");
                Console.WriteLine("   Your system may not be Windows 11, but we'll try anyway.");
                Console.ResetColor();
                Console.WriteLine();
            }

            if (!IsAdministrator())
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("⚠️  Note: Running without administrator privileges.");
                Console.WriteLine("   Some installations may require elevation.");
                Console.ResetColor();
                Console.WriteLine();
            }

            try
            {
                Console.WriteLine("🔍 Step 1: Checking prerequisites...\n");
                await CheckAndInstallPrerequisites();

                Console.WriteLine("\n📥 Step 2: Downloading Claude Code...\n");
                string installerPath = await DownloadClaudeCode();

                Console.WriteLine("\n🚀 Step 3: Installing Claude Code...\n");
                await InstallClaudeCode(installerPath);

                Console.WriteLine("\n✅ Step 4: Verifying installation...\n");
                bool verified = await VerifyInstallation();

                if (verified)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\n╔════════════════════════════════════════════════════════╗");
                    Console.WriteLine("║  ✨ SUCCESS! Claude Code is installed and ready!  ✨   ║");
                    Console.WriteLine("╚════════════════════════════════════════════════════════╝");
                    Console.ResetColor();
                    Console.WriteLine("\n📖 Quick Start Guide:");
                    Console.WriteLine("   1. Open a new Command Prompt or PowerShell window");
                    Console.WriteLine("   2. Type: claude-code");
                    Console.WriteLine("   3. Follow the authentication prompts");
                    Console.WriteLine("   4. Start coding with Claude!");
                    Console.WriteLine("\n💡 Tip: Run 'claude-code --help' for all available commands");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n⚠️  Installation completed but verification failed.");
                    Console.WriteLine("   You may need to restart your terminal or add Claude Code to PATH manually.");
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n❌ Error: {ex.Message}");
                Console.WriteLine("\n💬 Need help? Visit: https://support.claude.com");
                Console.ResetColor();
                Environment.Exit(1);
            }

            Console.WriteLine("\n\nPress any key to exit...");
            Console.ReadKey();
        }

        static bool IsWindows11OrLater()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return false;

            var version = Environment.OSVersion.Version;
            // Windows 11 is version 10.0.22000 or higher
            return version.Major >= 10 && version.Build >= 22000;
        }

        static bool IsAdministrator()
        {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        static async Task CheckAndInstallPrerequisites()
        {
            // Check Node.js
            Console.Write("   • Checking for Node.js... ");
            bool hasNodeJs = await CheckCommand("node --version");
            
            if (hasNodeJs)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✓ Found");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("✗ Not found");
                Console.ResetColor();
                Console.WriteLine("     Claude Code requires Node.js v18 or later.");
                Console.WriteLine("     Please install from: https://nodejs.org/");
                Console.WriteLine("\n     Would you like to open the Node.js download page? (y/n)");
                
                var key = Console.ReadKey(true);
                if (key.KeyChar == 'y' || key.KeyChar == 'Y')
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "https://nodejs.org/",
                        UseShellExecute = true
                    });
                }
                
                Console.WriteLine("\n     Please install Node.js and run this installer again.");
                Environment.Exit(0);
            }

            // Check Git (optional but recommended)
            Console.Write("   • Checking for Git... ");
            bool hasGit = await CheckCommand("git --version");
            
            if (hasGit)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✓ Found");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("✗ Not found (optional)");
                Console.ResetColor();
                Console.WriteLine("     Git is recommended for version control with Claude Code.");
            }
        }

        static async Task<bool> CheckCommand(string command)
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
                    await process.WaitForExitAsync();
                    return process.ExitCode == 0;
                }
            }
            catch
            {
                return false;
            }
        }

        static async Task<string> DownloadClaudeCode()
        {
            string tempPath = Path.Combine(Path.GetTempPath(), "claude-code-installer.exe");
            
            Console.WriteLine($"   Downloading from: {CLAUDE_CODE_DOWNLOAD_URL}");
            Console.Write("   Progress: ");

            try
            {
                using (var response = await httpClient.GetAsync(CLAUDE_CODE_DOWNLOAD_URL, HttpCompletionOption.ResponseHeadersRead))
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

                                if (canReportProgress)
                                {
                                    var percentage = (int)((totalRead * 100) / totalBytes);
                                    Console.Write($"\r   Progress: {percentage}% ({totalRead / 1024 / 1024} MB / {totalBytes / 1024 / 1024} MB)");
                                }
                            }
                        } while (isMoreToRead);
                    }
                }

                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("   ✓ Download complete!");
                Console.ResetColor();
                return tempPath;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to download Claude Code: {ex.Message}");
            }
        }

        static async Task InstallClaudeCode(string installerPath)
        {
            Console.WriteLine("   Running installer...");
            Console.WriteLine("   (This may take a few moments)");
            Console.WriteLine();

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = installerPath,
                    Arguments = "/VERYSILENT /SUPPRESSMSGBOXES /NORESTART /SP-",
                    UseShellExecute = true,
                    Verb = "runas" // Request elevation if needed
                };

                using (var process = Process.Start(startInfo))
                {
                    if (process != null)
                    {
                        await process.WaitForExitAsync();
                        
                        if (process.ExitCode == 0)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("   ✓ Installation successful!");
                            Console.ResetColor();
                            
                            // Wait a moment for PATH to update
                            await Task.Delay(2000);
                        }
                        else
                        {
                            throw new Exception($"Installer exited with code {process.ExitCode}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Installation failed: {ex.Message}");
            }
            finally
            {
                // Clean up installer
                try
                {
                    if (File.Exists(installerPath))
                        File.Delete(installerPath);
                }
                catch { }
            }
        }

        static async Task<bool> VerifyInstallation()
        {
            Console.WriteLine("   Checking if 'claude-code' command is available...");
            
            // Try multiple times as PATH updates can be delayed
            for (int i = 0; i < 3; i++)
            {
                if (await CheckCommand("claude-code --version"))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("   ✓ Claude Code command verified!");
                    Console.ResetColor();
                    return true;
                }
                
                if (i < 2)
                {
                    Console.WriteLine($"   Waiting for PATH to update... (attempt {i + 1}/3)");
                    await Task.Delay(2000);
                }
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("   ⚠️  Could not verify command immediately.");
            Console.WriteLine("   Please restart your terminal and try: claude-code");
            Console.ResetColor();
            return false;
        }
    }
}
