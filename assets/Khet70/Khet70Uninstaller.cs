using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace Khet70Uninstaller
{
    internal static class Program
    {
        private const string InstallRootName = "Khet70";
        private const string DisplayName = "Khet70";

        [STAThread]
        private static void Main()
        {
            try
            {
                var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var installDir = Path.Combine(localAppData, InstallRootName);
                if (!IsExactInstallDirectory(installDir, localAppData))
                {
                    MessageBox.Show("เส้นทางติดตั้งไม่ปลอดภัย ยกเลิกการถอนการติดตั้ง\n" + installDir, "Khet70");
                    return;
                }

                var result = MessageBox.Show(
                    "ถอนการติดตั้ง Khet70 หรือไม่?\n\nการยืนยันจะลบเฉพาะ install root ของแอป รวม Data, Template และ Output ภายในโฟลเดอร์นี้",
                    "ถอนการติดตั้ง Khet70",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);
                if (result != DialogResult.Yes) return;

                var appExe = Path.Combine(installDir, "Khet70.exe");
                var uninstallExe = Path.Combine(installDir, "Uninstall Khet70.exe");
                DeleteOwnedShortcut(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "Khet70.lnk"), appExe);

                var startMenuDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), DisplayName);
                DeleteOwnedShortcut(Path.Combine(startMenuDir, "Khet70.lnk"), appExe);
                DeleteOwnedShortcut(Path.Combine(startMenuDir, "Uninstall Khet70.lnk"), uninstallExe);
                DeleteDirectoryIfEmpty(startMenuDir);
                MessageBox.Show("ถอนการติดตั้ง Khet70 สำเร็จ", "Khet70");
                ScheduleSelfDelete(installDir);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ถอนการติดตั้ง Khet70 ไม่สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
        private static void ScheduleSelfDelete(string installDir)
        {
            var command = "/c ping 127.0.0.1 -n 3 > nul & rmdir /s /q \"" + installDir + "\"";
            Process.Start(new ProcessStartInfo
            {
                FileName = Environment.GetEnvironmentVariable("ComSpec"),
                Arguments = command,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            });
        }
        private static void DeleteDirectoryIfEmpty(string path)
        {
            if (Directory.Exists(path) && Directory.GetFileSystemEntries(path).Length == 0) Directory.Delete(path);
        }
    }
}

