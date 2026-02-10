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
        private TabControl? _tabControl;
        private TabPage? _mainTab;
        private TabPage? _advancedTab;
        private TabPage? _pluginsTab;
        private TextBox? _pathsTextBox;
        private TextBox? _workingDirectoryTextBox;
        private Button? _browseDirectoryButton;
        private Button? _saveWorkingDirectoryButton;
        private ListBox? _pluginsListBox;
        private TextBox? _pluginDescriptionTextBox;
        private Button? _installPluginButton;
        private Button? _uninstallPluginButton;
        private Button? _refreshPluginsButton;
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
            this.Size = new Size(700, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Create TabControl
            _tabControl = new TabControl
            {
                Location = new Point(10, 10),
                Size = new Size(680, 600),
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

            // Plugins Tab
            _pluginsTab = new TabPage("Plugins");
            InitializePluginsTab();
            _tabControl.TabPages.Add(_pluginsTab);

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

            // Health check button
            var healthCheckButton = new Button
            {
                Text = "Health Check",
                Location = new Point(20, 480),
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

        private void InitializePluginsTab()
        {
            if (_pluginsTab == null) return;

            // Title
            var titleLabel = new Label
            {
                Text = "Claude Code Plugins",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 20)
            };
            _pluginsTab.Controls.Add(titleLabel);

            // Refresh button
            _refreshPluginsButton = new Button
            {
                Text = "Refresh",
                Location = new Point(600, 15),
                Size = new Size(70, 30),
                Font = new Font("Segoe UI", 9)
            };
            _refreshPluginsButton.Click += async (s, e) => await RefreshPluginsAsync();
            _pluginsTab.Controls.Add(_refreshPluginsButton);

            // Plugins list box
            _pluginsListBox = new ListBox
            {
                Location = new Point(20, 60),
                Size = new Size(300, 400),
                Font = new Font("Segoe UI", 9),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left
            };
            _pluginsListBox.SelectedIndexChanged += PluginsListBox_SelectedIndexChanged;
            _pluginsTab.Controls.Add(_pluginsListBox);

            // Description text box
            _pluginDescriptionTextBox = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Location = new Point(340, 60),
                Size = new Size(330, 300),
                Font = new Font("Segoe UI", 9),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            _pluginsTab.Controls.Add(_pluginDescriptionTextBox);

            // Install button
            _installPluginButton = new Button
            {
                Text = "Install Plugin",
                Location = new Point(340, 380),
                Size = new Size(150, 35),
                Font = new Font("Segoe UI", 9),
                Enabled = false
            };
            _installPluginButton.Click += async (s, e) => await InstallPluginButton_Click();
            _pluginsTab.Controls.Add(_installPluginButton);

            // Uninstall button
            _uninstallPluginButton = new Button
            {
                Text = "Uninstall Plugin",
                Location = new Point(520, 380),
                Size = new Size(150, 35),
                Font = new Font("Segoe UI", 9),
                Enabled = false
            };
            _uninstallPluginButton.Click += async (s, e) => await UninstallPluginButton_Click();
            _pluginsTab.Controls.Add(_uninstallPluginButton);

            // Load plugins initially
            _ = RefreshPluginsAsync();
        }

        private List<PluginInfo>? _availablePlugins;

        private async Task RefreshPluginsAsync()
        {
            if (_pluginsListBox == null || _installationService == null) return;

            _pluginsListBox.Items.Clear();
            _pluginDescriptionTextBox!.Text = "Loading plugins...";

            try
            {
                _availablePlugins = await _installationService.GetAvailablePluginsAsync();

                foreach (var plugin in _availablePlugins)
                {
                    string displayText = plugin.IsInstalled 
                        ? $"{plugin.Name} ✓ (v{plugin.InstalledVersion})"
                        : plugin.Name;
                    _pluginsListBox.Items.Add(displayText);
                }

                if (_availablePlugins.Count > 0)
                {
                    _pluginsListBox.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                _pluginDescriptionTextBox.Text = $"Error loading plugins: {ex.Message}";
            }
        }

        private void PluginsListBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_pluginsListBox == null || _availablePlugins == null || _pluginDescriptionTextBox == null) return;

            int selectedIndex = _pluginsListBox.SelectedIndex;
            if (selectedIndex >= 0 && selectedIndex < _availablePlugins.Count)
            {
                var plugin = _availablePlugins[selectedIndex];
                
                string description = $"Name: {plugin.Name}\n";
                description += $"Package: {plugin.PackageName}\n";
                description += $"Status: {(plugin.IsInstalled ? $"Installed (v{plugin.InstalledVersion})" : "Not Installed")}\n";
                description += $"GitHub: {plugin.GitHubUrl}\n\n";
                description += $"Description:\n{plugin.Description}";

                _pluginDescriptionTextBox.Text = description;

                _installPluginButton!.Enabled = !plugin.IsInstalled;
                _uninstallPluginButton!.Enabled = plugin.IsInstalled;
            }
        }

        private async Task InstallPluginButton_Click()
        {
            if (_pluginsListBox == null || _availablePlugins == null || _installationService == null) return;

            int selectedIndex = _pluginsListBox.SelectedIndex;
            if (selectedIndex < 0 || selectedIndex >= _availablePlugins.Count) return;

            var plugin = _availablePlugins[selectedIndex];

            if (plugin.IsInstalled)
            {
                MessageBox.Show($"{plugin.Name} is already installed.", "Already Installed", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            _installPluginButton!.Enabled = false;
            _uninstallPluginButton!.Enabled = false;

            try
            {
                var result = MessageBox.Show(
                    $"Install {plugin.Name}?\n\nPackage: {plugin.PackageName}\n\nThis will install the plugin globally via npm.",
                    "Install Plugin", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    Log($"Installing {plugin.Name}...");
                    var progress = new Progress<int>(percentage =>
                    {
                        // Could update a progress bar here if needed
                    });

                    bool success = await _installationService.InstallPluginAsync(plugin.PackageName, progress);

                    if (success)
                    {
                        Log($"✓ {plugin.Name} installed successfully!");
                        MessageBox.Show($"{plugin.Name} has been installed successfully!", "Success", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await RefreshPluginsAsync();
                    }
                    else
                    {
                        throw new Exception("Installation returned false");
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"✗ Error installing {plugin.Name}: {ex.Message}");
                MessageBox.Show($"Failed to install {plugin.Name}:\n{ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _installPluginButton.Enabled = true;
            }
        }

        private async Task UninstallPluginButton_Click()
        {
            if (_pluginsListBox == null || _availablePlugins == null || _installationService == null) return;

            int selectedIndex = _pluginsListBox.SelectedIndex;
            if (selectedIndex < 0 || selectedIndex >= _availablePlugins.Count) return;

            var plugin = _availablePlugins[selectedIndex];

            if (!plugin.IsInstalled)
            {
                MessageBox.Show($"{plugin.Name} is not installed.", "Not Installed", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            _installPluginButton!.Enabled = false;
            _uninstallPluginButton!.Enabled = false;

            try
            {
                var result = MessageBox.Show(
                    $"Uninstall {plugin.Name}?\n\nPackage: {plugin.PackageName}\n\nThis will remove the plugin from your system.",
                    "Uninstall Plugin", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    Log($"Uninstalling {plugin.Name}...");
                    bool success = await _installationService.UninstallPluginAsync(plugin.PackageName);

                    if (success)
                    {
                        Log($"✓ {plugin.Name} uninstalled successfully!");
                        MessageBox.Show($"{plugin.Name} has been uninstalled successfully!", "Success", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await RefreshPluginsAsync();
                    }
                    else
                    {
                        throw new Exception("Uninstallation returned false");
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"✗ Error uninstalling {plugin.Name}: {ex.Message}");
                MessageBox.Show($"Failed to uninstall {plugin.Name}:\n{ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _installPluginButton.Enabled = true;
            }
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
