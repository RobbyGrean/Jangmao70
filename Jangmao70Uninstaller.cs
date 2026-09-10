using System;
using System.IO;
using System.Windows.Forms;

namespace Jangmao70Uninstaller
{
    internal static class Program
    {
        private const string InstallRootName = "Jangmao70";
        private const string DisplayName = "Jangmao70";

        [STAThread]
        private static void Main()
        {
            try
            {
                var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var installDir = Path.Combine(localAppData, InstallRootName);
                if (!IsExactInstallDirectory(installDir, localAppData))
                {
                    MessageBox.Show("เส้นทางติดตั้งไม่ปลอดภัย ยกเลิกการถอนการติดตั้ง\n" + installDir, "Jangmao70");
                    return;
                }

                var result = MessageBox.Show(
                    "ถอนการติดตั้ง Jangmao70 หรือไม่?\n\nการยืนยันจะลบเฉพาะ install root ของแอป รวม Data, Template และ Output ภายในโฟลเดอร์นี้",
                    "ถอนการติดตั้ง Jangmao70",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);
                if (result != DialogResult.Yes) return;

                var appExe = Path.Combine(installDir, "Jangmao70.exe");
                var uninstallExe = Path.Combine(installDir, "Uninstall Jangmao70.exe");
                DeleteOwnedShortcut(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "Jangmao70.lnk"), appExe);

                var startMenuDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), DisplayName);
                DeleteOwnedShortcut(Path.Combine(startMenuDir, "Jangmao70.lnk"), appExe);
                DeleteOwnedShortcut(Path.Combine(startMenuDir, "Uninstall Jangmao70.lnk"), uninstallExe);
                DeleteDirectoryIfEmpty(startMenuDir);

                Directory.Delete(installDir, true);
                MessageBox.Show("ถอนการติดตั้ง Jangmao70 สำเร็จ", "Jangmao70");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ถอนการติดตั้ง Jangmao70 ไม่สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static bool IsExactInstallDirectory(string path, string localAppData)
        {
            var fullPath = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar);
            var fullLocalAppData = Path.GetFullPath(localAppData).TrimEnd(Path.DirectorySeparatorChar);
            var expected = Path.Combine(fullLocalAppData, InstallRootName);
            return Directory.Exists(fullPath)
                && string.Equals(fullPath, expected, StringComparison.OrdinalIgnoreCase)
                && fullPath.StartsWith(fullLocalAppData + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
        }

        private static void DeleteOwnedShortcut(string shortcutPath, string expectedTarget)
        {
            if (!File.Exists(shortcutPath)) return;
            try
            {
                var shellType = Type.GetTypeFromProgID("WScript.Shell");
                if (shellType == null) return;
                dynamic shell = Activator.CreateInstance(shellType);
                var shortcut = shell.CreateShortcut(shortcutPath);
                var target = (string)shortcut.TargetPath;
                if (string.Equals(Path.GetFullPath(target), Path.GetFullPath(expectedTarget), StringComparison.OrdinalIgnoreCase)) File.Delete(shortcutPath);
            }
            catch
            {
                // Leave an unreadable shortcut in place; never delete an unverified target.
            }
        }

        private static void DeleteDirectoryIfEmpty(string path)
        {
            if (Directory.Exists(path) && Directory.GetFileSystemEntries(path).Length == 0) Directory.Delete(path);
        }
    }
}
