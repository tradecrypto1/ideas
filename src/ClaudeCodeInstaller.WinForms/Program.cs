// Program.cs
using System;
using System.Windows.Forms;
using ClaudeCodeInstaller.Core;

namespace ClaudeCodeInstaller.WinForms
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
