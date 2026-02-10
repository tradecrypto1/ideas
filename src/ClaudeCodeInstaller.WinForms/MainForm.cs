// MainForm.cs
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClaudeCodeInstaller.Core;

namespace ClaudeCodeInstaller.WinForms
{
    public partial class MainForm : Form
    {
        private InstallationService? _installationService;
        private HealthCheckService? _healthCheckService;
        private InstallerUpdateService? _installerUpdateService;
        private Button? _installButton;
        private Button? _runButton;
        private Button? _checkUpdateButton;
        private ProgressBar? _progressBar;
        private Label? _statusLabel;
        private Label? _versionLabel;
        private Label? _installerUpdateLabel;
        private TextBox? _logTextBox;
        private string _currentVersion;

        public MainForm()
        {
            // Get version from assembly
            var assembly = Assembly.GetExecutingAssembly();
            var versionAttribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            var fileVersionAttribute = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>();
            _currentVersion = versionAttribute?.InformationalVersion ?? 
                             fileVersionAttribute?.Version ??
                             assembly.GetName().Version?.ToString() ?? "1.0.0";
            
            InitializeComponent();
            _installationService = new InstallationService();
            _healthCheckService = new HealthCheckService(_installationService);
            _installerUpdateService = new InstallerUpdateService();
        }

        private void InitializeComponent()
        {
            this.Text = "Claude Code Installer & Runner";
            this.Size = new Size(700, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Title
            var titleLabel = new Label
            {
                Text = "Claude Code Installer & Runner",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 20)
            };
            this.Controls.Add(titleLabel);

            // Version label
            _versionLabel = new Label
            {
                Text = $"Installer Version: {_currentVersion}",
                Font = new Font("Segoe UI", 9),
                AutoSize = true,
                Location = new Point(20, 60)
            };
            this.Controls.Add(_versionLabel);

            // Installer update label (initially hidden)
            _installerUpdateLabel = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.Orange,
                AutoSize = true,
                Location = new Point(250, 60),
                Visible = false
            };
            this.Controls.Add(_installerUpdateLabel);

            // Status label
            _statusLabel = new Label
            {
                Text = "Ready",
                Font = new Font("Segoe UI", 10),
                AutoSize = true,
                Location = new Point(20, 90)
            };
            this.Controls.Add(_statusLabel);

            // Progress bar
            _progressBar = new ProgressBar
            {
                Location = new Point(20, 120),
                Size = new Size(650, 25),
                Style = ProgressBarStyle.Continuous
            };
            this.Controls.Add(_progressBar);

            // Log text box
            _logTextBox = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Location = new Point(20, 160),
                Size = new Size(650, 250),
                Font = new Font("Consolas", 9)
            };
            this.Controls.Add(_logTextBox);

            // Install button
            _installButton = new Button
            {
                Text = "Install/Update Claude Code",
                Location = new Point(20, 430),
                Size = new Size(200, 40),
                Font = new Font("Segoe UI", 10)
            };
            _installButton.Click += InstallButton_Click;
            this.Controls.Add(_installButton);

            // Run button
            _runButton = new Button
            {
                Text = "Run Claude Code",
                Location = new Point(240, 430),
                Size = new Size(200, 40),
                Font = new Font("Segoe UI", 10),
                Enabled = false
            };
            _runButton.Click += RunButton_Click;
            this.Controls.Add(_runButton);

            // Check update button
            _checkUpdateButton = new Button
            {
                Text = "Check for Updates",
                Location = new Point(460, 430),
                Size = new Size(200, 40),
                Font = new Font("Segoe UI", 10)
            };
            _checkUpdateButton.Click += CheckUpdateButton_Click;
            this.Controls.Add(_checkUpdateButton);

            // Health check button
            var healthCheckButton = new Button
            {
                Text = "Health Check",
                Location = new Point(20, 480),
                Size = new Size(150, 35),
                Font = new Font("Segoe UI", 9)
            };
            healthCheckButton.Click += HealthCheckButton_Click;
            this.Controls.Add(healthCheckButton);

            // Load initial state
            _ = LoadInitialStateAsync();
            _ = CheckInstallerUpdateAsync();
        }

        private async Task LoadInitialStateAsync()
        {
            Log("Checking installation status...");
            bool isInstalled = await _installationService!.VerifyInstallationAsync();
            _runButton!.Enabled = isInstalled;
            
            if (isInstalled)
            {
                Log("✓ Claude Code is installed and ready!");
                _statusLabel!.Text = "Claude Code is installed";
            }
            else
            {
                Log("Claude Code is not installed. Click 'Install/Update' to install.");
                _statusLabel!.Text = "Ready to install";
            }
        }

        private async void InstallButton_Click(object? sender, EventArgs e)
        {
            _installButton!.Enabled = false;
            _checkUpdateButton!.Enabled = false;
            _progressBar!.Value = 0;
            _statusLabel!.Text = "Installing...";

            try
            {
                Log("Starting installation process...");

                // Check for updates first
                Log("Checking for updates...");
                bool updateAvailable = await _installationService!.IsUpdateAvailableAsync(_currentVersion);
                
                if (updateAvailable)
                {
                    Log("Update available! Uninstalling current version...");
                    // Uninstall logic would go here
                }

                // Check prerequisites
                Log("Checking prerequisites...");
                bool hasNode = await _installationService!.CheckCommandAsync("node --version");
                bool hasNpm = await _installationService.CheckCommandAsync("npm --version");
                
                if (!hasNode || !hasNpm)
                {
                    string missing = !hasNode ? "Node.js" : "npm";
                    Log($"✗ {missing} not found. Please install Node.js first.");
                    MessageBox.Show($"{missing} is required but not found. Please install Node.js v18 or later (which includes npm) from https://nodejs.org/", 
                        "Prerequisites Missing", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _installButton.Enabled = true;
                    _checkUpdateButton.Enabled = true;
                    return;
                }
                Log("✓ Prerequisites OK (Node.js and npm found)");

                // Install via npm (download is handled internally)
                Log("Installing Claude Code via npm...");
                _statusLabel.Text = "Installing Claude Code...";
                var progress = new Progress<int>(percentage =>
                {
                    _progressBar.Value = percentage;
                    _statusLabel.Text = $"Installing... {percentage}%";
                });

                string installerPath = await _installationService.DownloadClaudeCodeAsync(progress: progress);
                Log("✓ Download/preparation complete");

                // Install
                Log("Running npm install...");
                _statusLabel.Text = "Installing via npm...";
                await _installationService.InstallClaudeCodeAsync(installerPath);
                Log("✓ Installation complete");

                // Verify
                Log("Verifying installation...");
                bool verified = await _installationService.VerifyInstallationAsync();
                
                if (verified)
                {
                    Log("✓ Installation verified successfully!");
                    _statusLabel!.Text = "Installation successful!";
                    _runButton!.Enabled = true;
                    MessageBox.Show("Claude Code has been installed successfully!", "Success", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    Log("⚠ Installation completed but verification failed. You may need to restart.");
                    _statusLabel.Text = "Installation completed (verification pending)";
                    MessageBox.Show("Installation completed but verification failed. Please restart your computer and try running Claude Code.", 
                        "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                Log($"✗ Error: {ex.Message}");
                _statusLabel.Text = "Installation failed";
                MessageBox.Show($"Installation failed: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _installButton.Enabled = true;
                _checkUpdateButton.Enabled = true;
                _progressBar.Value = 0;
            }
        }

        private async void RunButton_Click(object? sender, EventArgs e)
        {
            try
            {
                Log("Starting Claude Code...");
                _statusLabel!.Text = "Running Claude Code...";
                await _installationService!.RunClaudeCodeAsync();
                Log("✓ Claude Code started");
                _statusLabel.Text = "Claude Code is running";
            }
            catch (Exception ex)
            {
                Log($"✗ Error running Claude Code: {ex.Message}");
                MessageBox.Show($"Failed to run Claude Code: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void CheckUpdateButton_Click(object? sender, EventArgs e)
        {
            _checkUpdateButton!.Enabled = false;
            _statusLabel!.Text = "Checking for updates...";
            Log("Checking for updates...");

            try
            {
                // Check for Claude Code updates
                bool claudeCodeUpdateAvailable = await _installationService!.IsUpdateAvailableAsync(_currentVersion);
                
                // Check for installer updates
                var installerUpdateInfo = await _installerUpdateService!.CheckForInstallerUpdateAsync(_currentVersion);
                bool installerUpdateAvailable = installerUpdateInfo != null && !installerUpdateInfo.IsLatest;

                if (installerUpdateAvailable)
                {
                    Log($"Installer update available: {installerUpdateInfo!.Version}");
                    var result = MessageBox.Show(
                        $"A newer version of the installer ({installerUpdateInfo.Version}) is available.\n\n" +
                        $"Current version: {_currentVersion}\n" +
                        $"Latest version: {installerUpdateInfo.Version}\n\n" +
                        "Would you like to update the installer now?",
                        "Installer Update Available", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                    
                    if (result == DialogResult.Yes)
                    {
                        await UpdateInstallerAsync(installerUpdateInfo);
                    }
                }
                else if (claudeCodeUpdateAvailable)
                {
                    Log("Claude Code update available!");
                    var result = MessageBox.Show("A newer version of Claude Code is available. Would you like to install it now?", 
                        "Update Available", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                    
                    if (result == DialogResult.Yes)
                    {
                        InstallButton_Click(sender, e);
                    }
                }
                else
                {
                    Log("✓ You have the latest versions");
                    MessageBox.Show("You have the latest version of both the installer and Claude Code.", "No Updates", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                Log($"✗ Error checking for updates: {ex.Message}");
                MessageBox.Show($"Failed to check for updates: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _checkUpdateButton.Enabled = true;
            }
        }

        private async Task CheckInstallerUpdateAsync()
        {
            try
            {
                var updateInfo = await _installerUpdateService!.CheckForInstallerUpdateAsync(_currentVersion);
                if (updateInfo != null && !updateInfo.IsLatest)
                {
                    _installerUpdateLabel!.Text = $"⚠ Installer update available: {updateInfo.Version}";
                    _installerUpdateLabel.Visible = true;
                    _installerUpdateLabel.ForeColor = Color.Orange;
                    Log($"Installer update available: {updateInfo.Version}");
                }
            }
            catch
            {
                // Silently fail - don't show error on startup
            }
        }

        private async Task UpdateInstallerAsync(VersionInfo updateInfo)
        {
            try
            {
                _checkUpdateButton!.Enabled = false;
                _installButton!.Enabled = false;
                _runButton!.Enabled = false;
                _statusLabel!.Text = "Downloading installer update...";
                Log($"Downloading installer update {updateInfo.Version}...");

                var progress = new Progress<int>(percentage =>
                {
                    _progressBar!.Value = percentage;
                    _statusLabel.Text = $"Downloading update... {percentage}%";
                });

                string installerPath = await _installerUpdateService!.DownloadInstallerUpdateAsync(updateInfo.DownloadUrl, progress);
                Log("✓ Download complete");

                _statusLabel.Text = "Installing update...";
                Log("Installing update...");

                var result = MessageBox.Show(
                    "The installer update has been downloaded.\n\n" +
                    "The application will close and restart with the new version.\n\n" +
                    "Click OK to continue.",
                    "Ready to Update", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);

                if (result == DialogResult.OK)
                {
                    await _installerUpdateService.InstallUpdateAsync(installerPath);
                }
                else
                {
                    // Clean up downloaded file
                    try
                    {
                        if (File.Exists(installerPath))
                            File.Delete(installerPath);
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                Log($"✗ Error updating installer: {ex.Message}");
                MessageBox.Show($"Failed to update installer: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _checkUpdateButton!.Enabled = true;
                _installButton!.Enabled = true;
                _runButton!.Enabled = true;
            }
        }

        private async void HealthCheckButton_Click(object? sender, EventArgs e)
        {
            Log("Running health check...");
            var result = await _healthCheckService!.CheckHealthAsync();
            
            Log($"Health Check Results:");
            Log($"  Timestamp: {result.Timestamp}");
            Log($"  Overall Health: {(result.IsHealthy ? "✓ Healthy" : "✗ Unhealthy")}");
            Log($"  Claude Code Installed: {(result.IsClaudeCodeInstalled ? "✓ Yes" : "✗ No")}");
            Log($"  Prerequisites: {(result.HasPrerequisites ? "✓ OK" : "✗ Missing")}");
            Log($"  Windows 11: {(result.IsWindows11 ? "✓ Yes" : "✗ No")}");
            Log($"  Admin Rights: {(result.HasAdminRights ? "✓ Yes" : "✗ No")}");
            
            if (!string.IsNullOrEmpty(result.ErrorMessage))
            {
                Log($"  Error: {result.ErrorMessage}");
            }

            MessageBox.Show($"Health Check Complete!\n\nOverall: {(result.IsHealthy ? "Healthy" : "Issues Found")}\n\nSee log for details.", 
                "Health Check", MessageBoxButtons.OK, 
                result.IsHealthy ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        }

        private void Log(string message)
        {
            if (_logTextBox != null)
            {
                if (_logTextBox.InvokeRequired)
                {
                    _logTextBox.Invoke(new Action(() => Log(message)));
                    return;
                }
                
                _logTextBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\r\n");
                _logTextBox.SelectionStart = _logTextBox.Text.Length;
                _logTextBox.ScrollToCaret();
            }
        }
    }
}
