using System;
using System.IO;
using System.Windows.Forms;

namespace ReimbursementDocUninstaller
{
    internal static class Program
    {
        private const string AppName = "ReimbursementDocApp";

        [STAThread]
        private static void Main()
        {
            try
            {
                var installDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppName);
                if (!IsSafeInstallDir(installDir))
                {
                    MessageBox.Show("Unsafe install path. Uninstall has been cancelled:\n" + installDir, "Uninstaller");
                    return;
                }

                var confirm = MessageBox.Show(
                    "Uninstall ReimbursementDocApp?\n\nOnly this folder and its own shortcuts will be removed:\n" + installDir,
                    "Uninstaller",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);
                if (confirm != DialogResult.Yes) return;

                DeleteFileIfSafeShortcut(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "ReimbursementDocApp.lnk"));

                var startMenuDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), AppName);
                DeleteFileIfSafeShortcut(Path.Combine(startMenuDir, "ReimbursementDocApp.lnk"));
                DeleteFileIfSafeShortcut(Path.Combine(startMenuDir, "Uninstall ReimbursementDocApp.lnk"));
                DeleteDirectoryIfExact(startMenuDir, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), AppName));

                if (Directory.Exists(installDir))
                {
                    Directory.Delete(installDir, true);
                }

                MessageBox.Show("Uninstalled successfully.", "Uninstaller");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Uninstaller Error");
            }
        }

        private static bool IsSafeInstallDir(string path)
        {
            var fullPath = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar);
            var localAppData = Path.GetFullPath(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)).TrimEnd(Path.DirectorySeparatorChar);
            return fullPath.Equals(Path.Combine(localAppData, AppName), StringComparison.OrdinalIgnoreCase)
                && fullPath.StartsWith(localAppData + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
                && Directory.Exists(fullPath);
        }

        private static void DeleteFileIfSafeShortcut(string path)
        {
            var fullPath = Path.GetFullPath(path);
            if (!fullPath.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase)) return;
            if (File.Exists(fullPath)) File.Delete(fullPath);
        }

        private static void DeleteDirectoryIfExact(string path, string expectedPath)
        {
            var fullPath = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar);
            var expectedFullPath = Path.GetFullPath(expectedPath).TrimEnd(Path.DirectorySeparatorChar);
            if (!fullPath.Equals(expectedFullPath, StringComparison.OrdinalIgnoreCase)) return;
            if (Directory.Exists(fullPath) && Directory.GetFileSystemEntries(fullPath).Length == 0)
            {
                Directory.Delete(fullPath);
            }
        }
    }
}
