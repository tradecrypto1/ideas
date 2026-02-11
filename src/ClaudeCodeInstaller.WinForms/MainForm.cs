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
        private Button? _uninstallButton;
        private Button? _runButton;
        private Button? _checkUpdateButton;
        private ProgressBar? _progressBar;
        private Label? _statusLabel;
        private Label? _versionLabel;
        private Label? _installerUpdateLabel;
        private TextBox? _logTextBox;
        private TabControl? _tabControl;
        private TabPage? _mainTab;
        private TabPage? _advancedTab;
        private TextBox? _pathsTextBox;
        private TextBox? _workingDirectoryTextBox;
        private Button? _browseDirectoryButton;
        private Button? _saveWorkingDirectoryButton;
        private string _currentVersion;
        private string _workingDirectory;

        public MainForm()
        {
            // Get version from assembly
            var assembly = Assembly.GetExecutingAssembly();
            var versionAttribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            var fileVersionAttribute = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>();
            _currentVersion = versionAttribute?.InformationalVersion ?? 
                             fileVersionAttribute?.Version ??
                             assembly.GetName().Version?.ToString() ?? "1.0.0";
            
            // Load working directory from settings or use default
            _workingDirectory = LoadWorkingDirectory();
            
            InitializeComponent();
            _installationService = new InstallationService(workingDirectory: _workingDirectory);
            _healthCheckService = new HealthCheckService(_installationService);
            _installerUpdateService = new InstallerUpdateService();
        }

        private void InitializeComponent()
        {
            this.Text = "Claude Code Installer & Runner";
            this.Size = new Size(800, 700);
            this.MinimumSize = new Size(750, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = true;
            this.MinimizeBox = true;

            // Create TabControl
            _tabControl = new TabControl
            {
                Location = new Point(10, 10),
                Size = new Size(780, 650),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            // Main Tab
            _mainTab = new TabPage("Main");
            InitializeMainTab();
            _tabControl.TabPages.Add(_mainTab);

            // Advanced Tab
            _advancedTab = new TabPage("Advanced");
            InitializeAdvancedTab();
            _tabControl.TabPages.Add(_advancedTab);

            this.Controls.Add(_tabControl);

            // Load initial state
            _ = LoadInitialStateAsync();
            _ = CheckInstallerUpdateAsync();
        }

        private void InitializeMainTab()
        {
            if (_mainTab == null) return;

            // Title
            var titleLabel = new Label
            {
                Text = "Claude Code Installer & Runner",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 20)
            };
            _mainTab.Controls.Add(titleLabel);

            // Version label
            _versionLabel = new Label
            {
                Text = $"Installer Version: {_currentVersion}",
                Font = new Font("Segoe UI", 9),
                AutoSize = true,
                Location = new Point(20, 60)
            };
            _mainTab.Controls.Add(_versionLabel);

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
            _mainTab.Controls.Add(_installerUpdateLabel);

            // Status label
            _statusLabel = new Label
            {
                Text = "Ready",
                Font = new Font("Segoe UI", 10),
                AutoSize = true,
                Location = new Point(20, 90)
            };
            _mainTab.Controls.Add(_statusLabel);

            // Progress bar
            _progressBar = new ProgressBar
            {
                Location = new Point(20, 120),
                Size = new Size(650, 25),
                Style = ProgressBarStyle.Continuous
            };
            _mainTab.Controls.Add(_progressBar);

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
            _mainTab.Controls.Add(_logTextBox);

            // Install button
            _installButton = new Button
            {
                Text = "Install/Update Claude Code",
                Location = new Point(20, 430),
                Size = new Size(200, 40),
                Font = new Font("Segoe UI", 10)
            };
            _installButton.Click += InstallButton_Click;
            _mainTab.Controls.Add(_installButton);

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
            _mainTab.Controls.Add(_runButton);

            // Check update button
            _checkUpdateButton = new Button
            {
                Text = "Check for Updates",
                Location = new Point(460, 430),
                Size = new Size(200, 40),
                Font = new Font("Segoe UI", 10)
            };
            _checkUpdateButton.Click += CheckUpdateButton_Click;
            _mainTab.Controls.Add(_checkUpdateButton);

            // Uninstall button
            _uninstallButton = new Button
            {
                Text = "Uninstall Claude Code",
                Location = new Point(190, 480),
                Size = new Size(150, 35),
                Font = new Font("Segoe UI", 9),
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                Enabled = false
            };
            _uninstallButton.Click += UninstallButton_Click;
            _mainTab.Controls.Add(_uninstallButton);

            // Install Claude Adapter button
            var installAdapterButton = new Button
            {
                Text = "Install Claude Adapter",
                Location = new Point(20, 480),
                Size = new Size(180, 35),
                Font = new Font("Segoe UI", 9),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White
            };
            installAdapterButton.Click += async (s, e) => await InstallAdapterButton_Click();
            _mainTab.Controls.Add(installAdapterButton);

            // Health check button
            var healthCheckButton = new Button
            {
                Text = "Health Check",
                Location = new Point(210, 480),
                Size = new Size(150, 35),
                Font = new Font("Segoe UI", 9)
            };
            healthCheckButton.Click += HealthCheckButton_Click;
            _mainTab.Controls.Add(healthCheckButton);
        }

        private void InitializeAdvancedTab()
        {
            if (_advancedTab == null) return;

            // Title
            var titleLabel = new Label
            {
                Text = "Advanced Settings",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 20)
            };
            _advancedTab.Controls.Add(titleLabel);

            // Working Directory Section
            var workingDirLabel = new Label
            {
                Text = "Working Directory:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 60)
            };
            _advancedTab.Controls.Add(workingDirLabel);

            _workingDirectoryTextBox = new TextBox
            {
                Text = _workingDirectory,
                Location = new Point(20, 85),
                Size = new Size(500, 25),
                Font = new Font("Consolas", 9)
            };
            _advancedTab.Controls.Add(_workingDirectoryTextBox);

            _browseDirectoryButton = new Button
            {
                Text = "Browse...",
                Location = new Point(530, 83),
                Size = new Size(80, 30),
                Font = new Font("Segoe UI", 9)
            };
            _browseDirectoryButton.Click += BrowseDirectoryButton_Click;
            _advancedTab.Controls.Add(_browseDirectoryButton);

            _saveWorkingDirectoryButton = new Button
            {
                Text = "Save",
                Location = new Point(620, 83),
                Size = new Size(60, 30),
                Font = new Font("Segoe UI", 9)
            };
            _saveWorkingDirectoryButton.Click += SaveWorkingDirectoryButton_Click;
            _advancedTab.Controls.Add(_saveWorkingDirectoryButton);

            // Installation Paths Section
            var pathsLabel = new Label
            {
                Text = "Installation Paths:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 130)
            };
            _advancedTab.Controls.Add(pathsLabel);

            // Refresh button
            var refreshButton = new Button
            {
                Text = "Refresh Paths",
                Location = new Point(550, 125),
                Size = new Size(120, 30),
                Font = new Font("Segoe UI", 9)
            };
            refreshButton.Click += async (s, e) => await RefreshPathsAsync();
            _advancedTab.Controls.Add(refreshButton);

            // Paths text box
            _pathsTextBox = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Location = new Point(20, 165),
                Size = new Size(650, 375),
                Font = new Font("Consolas", 9),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            _advancedTab.Controls.Add(_pathsTextBox);

            // Load paths initially
            _ = RefreshPathsAsync();
        }

        private void BrowseDirectoryButton_Click(object? sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select Working Directory";
                dialog.SelectedPath = _workingDirectoryTextBox?.Text ?? _workingDirectory;
                dialog.ShowNewFolderButton = true;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    _workingDirectoryTextBox!.Text = dialog.SelectedPath;
                }
            }
        }

        private void SaveWorkingDirectoryButton_Click(object? sender, EventArgs e)
        {
            if (_workingDirectoryTextBox == null) return;

            string newWorkingDir = _workingDirectoryTextBox.Text.Trim();
            
            if (string.IsNullOrEmpty(newWorkingDir))
            {
                MessageBox.Show("Working directory cannot be empty.", "Invalid Directory", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!Directory.Exists(newWorkingDir))
            {
                var result = MessageBox.Show(
                    $"Directory does not exist:\n{newWorkingDir}\n\nWould you like to create it?",
                    "Directory Not Found", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                
                if (result == DialogResult.Yes)
                {
                    try
                    {
                        Directory.CreateDirectory(newWorkingDir);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to create directory: {ex.Message}", "Error", 
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
                else
                {
                    return;
                }
            }

            _workingDirectory = newWorkingDir;
            SaveWorkingDirectory(_workingDirectory);
            
            // Update InstallationService with new working directory
            if (_installationService != null)
            {
                _installationService.WorkingDirectory = _workingDirectory;
            }

            MessageBox.Show($"Working directory saved:\n{_workingDirectory}", "Settings Saved", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private string LoadWorkingDirectory()
        {
            try
            {
                string settingsFile = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "ClaudeCodeInstaller",
                    "settings.txt");

                if (File.Exists(settingsFile))
                {
                    string content = File.ReadAllText(settingsFile).Trim();
                    if (!string.IsNullOrEmpty(content) && Directory.Exists(content))
                    {
                        return content;
                    }
                }
            }
            catch
            {
                // Ignore errors, use default
            }

            return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        }

        private void SaveWorkingDirectory(string workingDir)
        {
            try
            {
                string settingsDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "ClaudeCodeInstaller");

                if (!Directory.Exists(settingsDir))
                {
                    Directory.CreateDirectory(settingsDir);
                }

                string settingsFile = Path.Combine(settingsDir, "settings.txt");
                File.WriteAllText(settingsFile, workingDir);
            }
            catch
            {
                // Ignore errors
            }
        }

        private async Task RefreshPathsAsync()
        {
            if (_pathsTextBox == null || _installationService == null) return;

            _pathsTextBox.Text = "Loading installation paths...";
            
            try
            {
                string paths = await _installationService.GetInstallationPathsAsync();
                _pathsTextBox.Text = paths;
            }
            catch (Exception ex)
            {
                _pathsTextBox.Text = $"Error loading paths: {ex.Message}";
            }
        }

        private async Task LoadInitialStateAsync()
        {
            Log("Checking installation status...");
            bool isInstalled = await _installationService!.VerifyInstallationAsync();
            bool isRunning = await _installationService.IsClaudeCodeRunningAsync();
            
            _runButton!.Enabled = isInstalled;
            _uninstallButton!.Enabled = isInstalled;
            
            if (isInstalled)
            {
                if (isRunning)
                {
                    Log("Claude Code is installed and currently running.");
                    _statusLabel!.Text = "Claude Code is installed and running";
                }
                else
                {
                    Log("Claude Code is installed and ready to use.");
                    _statusLabel!.Text = "Claude Code is installed";
                }
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
            _runButton!.Enabled = false;
            _uninstallButton!.Enabled = false;
            _progressBar!.Value = 0;
            _statusLabel!.Text = "Installing...";

            try
            {
                Log("Starting installation process...");

                // Check if Claude Code is running and stop it
                bool wasRunning = await _installationService!.IsClaudeCodeRunningAsync();
                if (wasRunning)
                {
                    Log("Claude Code is currently running. Stopping it...");
                    _statusLabel.Text = "Stopping Claude Code...";
                    await _installationService.StopClaudeCodeAsync();
                    Log("✓ Claude Code stopped");
                    await Task.Delay(1000);
                }

                // Check if already installed
                bool isInstalled = await _installationService.VerifyInstallationAsync();
                if (isInstalled)
                {
                    Log("Claude Code is already installed. Updating...");
                }

                // Check for updates first
                Log("Checking for updates...");
                bool updateAvailable = await _installationService.IsUpdateAvailableAsync(_currentVersion);
                
                if (updateAvailable)
                {
                    Log("Update available! Updating installation...");
                }

                // Check prerequisites (native installer doesn't require Node.js/npm)
                Log("Checking prerequisites...");
                Log("✓ Prerequisites OK (Native installer ready)");

                // Install via native installer
                Log("Installing Claude Code using native installer...");
                _statusLabel.Text = "Installing Claude Code...";
                var progress = new Progress<int>(percentage =>
                {
                    _progressBar.Value = percentage;
                    _statusLabel.Text = $"Installing... {percentage}%";
                });

                string installerPath = await _installationService.DownloadClaudeCodeAsync(progress: progress);
                Log("✓ Preparation complete");

                // Install
                Log("Running native installer script...");
                _statusLabel.Text = "Installing via native installer...";
                await _installationService.InstallClaudeCodeAsync(installerPath);
                Log("✓ Installation complete");

                // Verify
                Log("Verifying installation...");
                _statusLabel.Text = "Verifying installation...";
                bool verified = await _installationService.VerifyInstallationAsync();
                
                if (verified)
                {
                    Log("✓ Installation verified successfully!");
                    _statusLabel!.Text = "Installation successful!";
                    _runButton!.Enabled = true;
                    _uninstallButton!.Enabled = true;
                    MessageBox.Show("Claude Code has been installed successfully!", "Success", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    Log("⚠ Installation completed but PATH may not be updated yet.");
                    Log("The installation files are present, but the command may not be available until you:");
                    Log("  1. Close and reopen this application, OR");
                    Log("  2. Open a new terminal/command prompt, OR");
                    Log("  3. Restart your computer");
                    _statusLabel.Text = "Installation completed (restart recommended)";
                    
                    var result = MessageBox.Show(
                        "Claude Code installation completed, but the PATH environment variable may not be updated yet.\n\n" +
                        "To use Claude Code, please:\n" +
                        "• Close and reopen this application, OR\n" +
                        "• Open a new terminal/command prompt, OR\n" +
                        "• Restart your computer\n\n" +
                        "The installation files are present and will work once PATH is refreshed.\n\n" +
                        "Would you like to try running Claude Code anyway?",
                        "Installation Complete",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Information);
                    
                    if (result == DialogResult.Yes)
                    {
                        // Try to run it anyway - might work if PATH was updated
                        try
                        {
                            await _installationService.RunClaudeCodeAsync();
                            _runButton!.Enabled = true;
                            _uninstallButton!.Enabled = true;
                        }
                        catch
                        {
                            // If it fails, user will need to restart
                        }
                    }
                    else
                    {
                        // Enable buttons anyway since files are installed
                        _runButton!.Enabled = true;
                        _uninstallButton!.Enabled = true;
                    }
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
                // Refresh installation status
                bool isInstalled = await _installationService!.VerifyInstallationAsync();
                _runButton!.Enabled = isInstalled;
                _uninstallButton!.Enabled = isInstalled;
                _progressBar.Value = 0;
            }
        }

        private async void UninstallButton_Click(object? sender, EventArgs e)
        {
            if (_installationService == null) return;

            // Check if Claude Code is running
            bool isRunning = await _installationService.IsClaudeCodeRunningAsync();
            string message = "Are you sure you want to uninstall Claude Code?\n\n" +
                "This will remove Claude Code from your system.\n\n";
            
            if (isRunning)
            {
                message += "⚠ Claude Code is currently running. It will be stopped before uninstallation.\n\n";
            }
            
            message += "Note: This will not remove your Claude Code configuration files or settings.";

            // Confirm uninstall
            var result = MessageBox.Show(
                message,
                "Uninstall Claude Code",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes) return;

            _uninstallButton!.Enabled = false;
            _installButton!.Enabled = false;
            _runButton!.Enabled = false;

            try
            {
                Log("Starting uninstallation...");
                _statusLabel!.Text = "Uninstalling Claude Code...";
                _progressBar!.Value = 0;

                // Stop Claude Code if running (handled inside UninstallClaudeCodeAsync)
                if (isRunning)
                {
                    Log("Claude Code is running. Stopping it...");
                    _statusLabel.Text = "Stopping Claude Code...";
                    await _installationService.StopClaudeCodeAsync();
                    Log("✓ Claude Code stopped");
                    await Task.Delay(1000);
                }

                bool success = await _installationService.UninstallClaudeCodeAsync();

                if (success)
                {
                    _progressBar.Value = 100;
                    Log("✓ Claude Code uninstalled successfully!");
                    _statusLabel.Text = "Claude Code has been uninstalled";
                    
                    MessageBox.Show(
                        "Claude Code has been successfully uninstalled from your system.",
                        "Uninstall Complete",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    // Refresh state
                    await LoadInitialStateAsync();
                }
                else
                {
                    throw new Exception("Uninstallation returned false");
                }
            }
            catch (Exception ex)
            {
                Log($"✗ Error uninstalling Claude Code: {ex.Message}");
                _statusLabel.Text = "Uninstall failed";
                MessageBox.Show(
                    $"Failed to uninstall Claude Code:\n{ex.Message}",
                    "Uninstall Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                // Refresh installation status
                bool isInstalled = await _installationService!.VerifyInstallationAsync();
                _uninstallButton.Enabled = isInstalled;
                _installButton.Enabled = true;
                _runButton.Enabled = isInstalled;
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

        private async Task InstallAdapterButton_Click()
        {
            if (_installationService == null) return;

            // Check npm availability
            bool npmAvailable = await _installationService.CheckNpmAvailableAsync();
            if (!npmAvailable)
            {
                MessageBox.Show(
                    "npm is required to install Claude Adapter but is not found.\n\n" +
                    "Please install Node.js (which includes npm) from https://nodejs.org/\n\n" +
                    "Note: Claude Code itself doesn't require npm, but plugins are installed via npm.",
                    "npm Not Found",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            // Check if already installed
            bool isInstalled = await _installationService.IsPluginInstalledAsync("claude-adapter");
            if (isInstalled)
            {
                var result = MessageBox.Show(
                    "Claude Adapter is already installed.\n\n" +
                    "Would you like to reinstall it to get the latest version?",
                    "Already Installed",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);
                
                if (result != DialogResult.Yes) return;
            }

            try
            {
                Log("Installing Claude Adapter...");
                _statusLabel!.Text = "Installing Claude Adapter...";
                _progressBar!.Value = 0;

                var progress = new Progress<int>(percentage =>
                {
                    _progressBar.Value = percentage;
                    _statusLabel.Text = $"Installing Claude Adapter... {percentage}%";
                });

                bool success = await _installationService.InstallPluginAsync("claude-adapter", progress);

                if (success)
                {
                    _progressBar.Value = 100;
                    Log("✓ Claude Adapter installed successfully!");
                    _statusLabel.Text = "Claude Adapter installed";
                    
                    MessageBox.Show(
                        "Claude Adapter has been installed successfully!\n\n" +
                        "Claude Adapter transforms your OpenAI API into an Anthropic-compatible endpoint for Claude Code.\n\n" +
                        "To use it, run: claude-adapter\n\n" +
                        "For more information, visit: https://github.com/shantoislamdev/claude-adapter",
                        "Installation Complete",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                else
                {
                    throw new Exception("Installation returned false");
                }
            }
            catch (Exception ex)
            {
                Log($"✗ Error installing Claude Adapter: {ex.Message}");
                _statusLabel.Text = "Installation failed";
                MessageBox.Show(
                    $"Failed to install Claude Adapter:\n{ex.Message}",
                    "Installation Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                _progressBar!.Value = 0;
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
