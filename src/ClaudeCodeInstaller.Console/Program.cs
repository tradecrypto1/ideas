// Program.cs
using System;
using System.Threading.Tasks;
using ClaudeCodeInstaller.Core;
using SysConsole = System.Console;

namespace ClaudeCodeInstaller.Console
{
    class Program
    {
        private static InstallationService? _installationService;

        static async Task Main(string[] args)
        {
            SysConsole.ForegroundColor = ConsoleColor.Cyan;
            SysConsole.WriteLine("╔════════════════════════════════════════════════════════╗");
            SysConsole.WriteLine("║     Claude Code Installation Wizard for Windows 11     ║");
            SysConsole.WriteLine("║                    By Anthropic                        ║");
            SysConsole.WriteLine("╚════════════════════════════════════════════════════════╝");
            SysConsole.ResetColor();
            SysConsole.WriteLine();

            if (!InstallationService.IsWindows11OrLater())
            {
                SysConsole.ForegroundColor = ConsoleColor.Yellow;
                SysConsole.WriteLine("⚠️  Warning: This installer is optimized for Windows 11.");
                SysConsole.WriteLine("   Your system may not be Windows 11, but we'll try anyway.");
                SysConsole.ResetColor();
                SysConsole.WriteLine();
            }

            if (!InstallationService.IsAdministrator())
            {
                SysConsole.ForegroundColor = ConsoleColor.Yellow;
                SysConsole.WriteLine("⚠️  Note: Running without administrator privileges.");
                SysConsole.WriteLine("   Some installations may require elevation.");
                SysConsole.ResetColor();
                SysConsole.WriteLine();
            }

            _installationService = new InstallationService();

            try
            {
                SysConsole.WriteLine("🔍 Step 1: Checking prerequisites...\n");
                await CheckAndInstallPrerequisites();

                SysConsole.WriteLine("\n📥 Step 2: Downloading Claude Code...\n");
                string installerPath = await DownloadClaudeCode();

                SysConsole.WriteLine("\n🚀 Step 3: Installing Claude Code...\n");
                await InstallClaudeCode(installerPath);

                SysConsole.WriteLine("\n✅ Step 4: Verifying installation...\n");
                bool verified = await VerifyInstallation();

                if (verified)
                {
                    SysConsole.ForegroundColor = ConsoleColor.Green;
                    SysConsole.WriteLine("\n╔════════════════════════════════════════════════════════╗");
                    SysConsole.WriteLine("║  ✨ SUCCESS! Claude Code is installed and ready!  ✨   ║");
                    SysConsole.WriteLine("╚════════════════════════════════════════════════════════╝");
                    SysConsole.ResetColor();
                    SysConsole.WriteLine("\n📖 Quick Start Guide:");
                    SysConsole.WriteLine("   1. Open a new Command Prompt or PowerShell window");
                    SysConsole.WriteLine("   2. Type: claude-code");
                    SysConsole.WriteLine("   3. Follow the authentication prompts");
                    SysConsole.WriteLine("   4. Start coding with Claude!");
                    SysConsole.WriteLine("\n💡 Tip: Run 'claude-code --help' for all available commands");

                    // Ask if user wants to run Claude Code now
                    SysConsole.WriteLine("\n🚀 Would you like to run Claude Code now? (y/n): ");
                    var key = SysConsole.ReadKey(true);
                    if (key.KeyChar == 'y' || key.KeyChar == 'Y')
                    {
                        await _installationService.RunClaudeCodeAsync();
                    }
                }
                else
                {
                    SysConsole.ForegroundColor = ConsoleColor.Red;
                    SysConsole.WriteLine("\n⚠️  Installation completed but verification failed.");
                    SysConsole.WriteLine("   You may need to restart your terminal or add Claude Code to PATH manually.");
                    SysConsole.ResetColor();
                }
            }
            catch (Exception ex)
            {
                SysConsole.ForegroundColor = ConsoleColor.Red;
                SysConsole.WriteLine($"\n❌ Error: {ex.Message}");
                SysConsole.WriteLine("\n💬 Need help? Visit: https://support.claude.com");
                SysConsole.ResetColor();
                Environment.Exit(1);
            }

            SysConsole.WriteLine("\n\nPress any key to exit...");
            SysConsole.ReadKey();
        }

        static async Task CheckAndInstallPrerequisites()
        {
            if (_installationService == null) return;

            SysConsole.Write("   • Checking for Node.js... ");
            bool hasNodeJs = await _installationService.CheckPrerequisitesAsync();

            if (hasNodeJs)
            {
                SysConsole.ForegroundColor = ConsoleColor.Green;
                SysConsole.WriteLine("✓ Found");
                SysConsole.ResetColor();
            }
            else
            {
                SysConsole.ForegroundColor = ConsoleColor.Yellow;
                SysConsole.WriteLine("✗ Not found");
                SysConsole.ResetColor();
                SysConsole.WriteLine("     Claude Code requires Node.js v18 or later.");
                SysConsole.WriteLine("     Please install from: https://nodejs.org/");
                SysConsole.WriteLine("\n     Would you like to open the Node.js download page? (y/n)");

                var key = SysConsole.ReadKey(true);
                if (key.KeyChar == 'y' || key.KeyChar == 'Y')
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "https://nodejs.org/",
                        UseShellExecute = true
                    });
                }

                SysConsole.WriteLine("\n     Please install Node.js and run this installer again.");
                Environment.Exit(0);
            }

            SysConsole.Write("   • Checking for Git... ");
            bool hasGit = await _installationService.CheckCommandAsync("git --version");

            if (hasGit)
            {
                SysConsole.ForegroundColor = ConsoleColor.Green;
                SysConsole.WriteLine("✓ Found");
                SysConsole.ResetColor();
            }
            else
            {
                SysConsole.ForegroundColor = ConsoleColor.Yellow;
                SysConsole.WriteLine("✗ Not found (optional)");
                SysConsole.ResetColor();
                SysConsole.WriteLine("     Git is recommended for version control with Claude Code.");
            }
        }

        static async Task<string> DownloadClaudeCode()
        {
            if (_installationService == null)
                throw new InvalidOperationException("InstallationService not initialized");

            SysConsole.WriteLine($"   Downloading Claude Code...");
            SysConsole.Write("   Progress: ");

            var progress = new Progress<int>(percentage =>
            {
                SysConsole.Write($"\r   Progress: {percentage}%");
            });

            string installerPath = await _installationService.DownloadClaudeCodeAsync(progress: progress);
            SysConsole.WriteLine();
            SysConsole.ForegroundColor = ConsoleColor.Green;
            SysConsole.WriteLine("   ✓ Download complete!");
            SysConsole.ResetColor();
            return installerPath;
        }

        static async Task InstallClaudeCode(string installerPath)
        {
            if (_installationService == null)
                throw new InvalidOperationException("InstallationService not initialized");

            SysConsole.WriteLine("   Running installer...");
            SysConsole.WriteLine("   (This may take a few moments)");
            SysConsole.WriteLine();

            try
            {
                await _installationService.InstallClaudeCodeAsync(installerPath);
                SysConsole.ForegroundColor = ConsoleColor.Green;
                SysConsole.WriteLine("   ✓ Installation successful!");
                SysConsole.ResetColor();
            }
            finally
            {
                try
                {
                    if (System.IO.File.Exists(installerPath))
                        System.IO.File.Delete(installerPath);
                }
                catch { }
            }
        }

        static async Task<bool> VerifyInstallation()
        {
            if (_installationService == null)
                throw new InvalidOperationException("InstallationService not initialized");

            SysConsole.WriteLine("   Checking if 'claude-code' command is available...");

            bool verified = await _installationService.VerifyInstallationAsync();

            if (verified)
            {
                SysConsole.ForegroundColor = ConsoleColor.Green;
                SysConsole.WriteLine("   ✓ Claude Code command verified!");
                SysConsole.ResetColor();
            }
            else
            {
                SysConsole.ForegroundColor = ConsoleColor.Yellow;
                SysConsole.WriteLine("   ⚠️  Could not verify command immediately.");
                SysConsole.WriteLine("   Please restart your terminal and try: claude-code");
                SysConsole.ResetColor();
            }

            return verified;
        }
    }
}
