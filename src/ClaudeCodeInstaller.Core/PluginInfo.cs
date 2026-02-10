// PluginInfo.cs
using System;

namespace ClaudeCodeInstaller.Core
{
    public class PluginInfo
    {
        public string Name { get; set; } = string.Empty;
        public string PackageName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string GitHubUrl { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public bool IsInstalled { get; set; }
        public string InstalledVersion { get; set; } = string.Empty;
    }
}
