// VersionInfo.cs
using System;

namespace ClaudeCodeInstaller.Core
{
    public class VersionInfo
    {
        public string Version { get; set; } = "1.0.0";
        public DateTime ReleaseDate { get; set; } = DateTime.UtcNow;
        public string DownloadUrl { get; set; } = string.Empty;
        public bool IsLatest { get; set; }
    }
}
