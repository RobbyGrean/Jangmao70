using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace Jangmao70Installer
{
    internal static class Program
    {
        private const string InstallRootName = "Jangmao70";
        private const string DisplayName = "Jangmao70";
        private const string PayloadResourceName = "Jangmao70.Payload.zip";

        [STAThread]
        private static void Main()
        {
            string payload = null;
            try
            {
                payload = ExtractEmbeddedPayload();
                var payloadConfig = Path.Combine(payload, "Config");
                var payloadTemplate = Path.Combine(payload, "Template");
                RequireFile(Path.Combine(payload, "Jangmao70.exe"));
                RequireFile(Path.Combine(payload, "template_tags.json"));
                RequireFile(Path.Combine(payload, "app_database.json"));
                RequireDirectory(payloadConfig);
                RequireDirectory(payloadTemplate);

                var installDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), InstallRootName);
                var installConfig = Path.Combine(installDir, "Config");
                var installTemplate = Path.Combine(installDir, "Template");
                var appExe = Path.Combine(installDir, "Jangmao70.exe");
                if (!CloseRunningApplication(appExe)) return;

                Directory.CreateDirectory(installDir);
                Directory.CreateDirectory(Path.Combine(installDir, "Data"));
                Directory.CreateDirectory(Path.Combine(installDir, "Output", "เงินเดือน"));
                Directory.CreateDirectory(Path.Combine(installDir, "Output", "เอกสารจัดจ้าง"));

                CopyFile(Path.Combine(payload, "Jangmao70.exe"), Path.Combine(installDir, "Jangmao70.exe"), true);
                CopyFile(Path.Combine(payload, "template_tags.json"), Path.Combine(installDir, "template_tags.json"), true);
                CopyFile(Path.Combine(payload, "app_database.json"), Path.Combine(installDir, "app_database.json"), true);
                CopyDirectory(payloadConfig, installConfig, true);
                CopyDirectory(payloadTemplate, installTemplate, false);

                var uninstallPayload = Path.Combine(payload, "Uninstall Jangmao70.exe");
                if (File.Exists(uninstallPayload)) CopyFile(uninstallPayload, Path.Combine(installDir, Path.GetFileName(uninstallPayload)), true);

                var uninstallExe = Path.Combine(installDir, "Uninstall Jangmao70.exe");
                var desktopShortcut = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "Jangmao70.lnk");
                CreateShortcut(desktopShortcut, appExe, installDir);

                var startMenuDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), DisplayName);
                Directory.CreateDirectory(startMenuDir);
                CreateShortcut(Path.Combine(startMenuDir, "Jangmao70.lnk"), appExe, installDir);
                if (File.Exists(uninstallExe)) CreateShortcut(Path.Combine(startMenuDir, "Uninstall Jangmao70.lnk"), uninstallExe, installDir);

                var openApp = MessageBox.Show(
                    "ติดตั้ง Jangmao70 สำเร็จแล้ว\nต้องการเปิดโปรแกรมตอนนี้หรือไม่?",
                    "Jangmao70",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information);
                if (openApp == DialogResult.Yes)
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = appExe,
                        WorkingDirectory = installDir,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ติดตั้ง Jangmao70 ไม่สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                DeleteDirectory(payload);
            }
        }

        private static string ExtractEmbeddedPayload()
        {
            var staging = Path.Combine(Path.GetTempPath(), "Jangmao70-Installer-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(staging);
            try
            {
                var stagingRoot = Path.GetFullPath(staging).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
                var stagingFullPath = Path.GetFullPath(staging);
                using (var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(PayloadResourceName))
                {
                    if (resource == null) throw new InvalidOperationException("ไม่พบ Payload ที่ฝังอยู่ในตัวติดตั้ง");
                    using (var archive = new ZipArchive(resource, ZipArchiveMode.Read))
                    {
                        foreach (var entry in archive.Entries)
                        {
                            var entryPath = entry.FullName.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
                            var destination = Path.GetFullPath(Path.Combine(staging, entryPath));
                            if (!string.Equals(destination, stagingFullPath, StringComparison.OrdinalIgnoreCase) &&
                                !destination.StartsWith(stagingRoot, StringComparison.OrdinalIgnoreCase))
                            {
                                throw new InvalidDataException("Payload มี path ที่ไม่ปลอดภัย: " + entry.FullName);
                            }

                            if (string.IsNullOrEmpty(entry.Name))
                            {
                                Directory.CreateDirectory(destination);
                                continue;
                            }

                            var directory = Path.GetDirectoryName(destination);
                            if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);
                            using (var input = entry.Open())
                            using (var output = File.Create(destination)) input.CopyTo(output);
                        }
                    }
                }
                return staging;
            }
            catch
            {
                DeleteDirectory(staging);
                throw;
            }
        }

        private static void DeleteDirectory(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return;
            try
            {
                if (Directory.Exists(path)) Directory.Delete(path, true);
            }
            catch
            {
            }
        }

        private static void RequireFile(string path)
        {
            if (!File.Exists(path)) throw new FileNotFoundException("ไม่พบไฟล์ใน Payload", path);
        }

        private static bool CloseRunningApplication(string appExe)
        {
            var processName = Path.GetFileNameWithoutExtension(appExe);
            while (true)
            {
                var running = Process.GetProcessesByName(processName)
                    .Where(x => IsTargetProcess(x, appExe))
                    .ToList();
                if (running.Count == 0) return true;

                foreach (var process in running)
                {
                    try
                    {
                        if (!process.HasExited) process.CloseMainWindow();
                    }
                    catch
                    {
                    }
                }

                var closed = running.All(x =>
                {
                    try { return x.WaitForExit(5000); }
                    catch { return true; }
                });
                foreach (var process in running) process.Dispose();
                if (closed) return true;

                var retry = MessageBox.Show(
                    "กรุณาปิด Jangmao70 ก่อนติดตั้งรุ่นใหม่\nหากโปรแกรมกำลังถามให้บันทึกข้อมูล ให้จัดการหน้าต่างนั้นให้เสร็จแล้วกด Retry",
                    "ยังปิด Jangmao70 ไม่สำเร็จ",
                    MessageBoxButtons.RetryCancel,
                    MessageBoxIcon.Warning);
                if (retry != DialogResult.Retry) return false;
            }
        }

        private static bool IsTargetProcess(Process process, string appExe)
        {
            try
            {
                return string.Equals(
                    Path.GetFullPath(process.MainModule.FileName),
                    Path.GetFullPath(appExe),
                    StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private static void RequireDirectory(string path)
        {
            if (!Directory.Exists(path)) throw new DirectoryNotFoundException("ไม่พบโฟลเดอร์ใน Payload: " + path);
        }

        private static void CopyFile(string source, string target, bool overwrite)
        {
            var directory = Path.GetDirectoryName(target);
            if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);
            File.Copy(source, target, overwrite);
        }

        private static void CopyDirectory(string source, string target, bool overwriteExisting)
        {
            Directory.CreateDirectory(target);
            foreach (var directory in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
            {
                var relative = directory.Substring(source.TrimEnd(Path.DirectorySeparatorChar).Length + 1);
                Directory.CreateDirectory(Path.Combine(target, relative));
            }
            foreach (var file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
            {
                var relative = file.Substring(source.TrimEnd(Path.DirectorySeparatorChar).Length + 1);
                var destination = Path.Combine(target, relative);
                if (overwriteExisting || !File.Exists(destination)) CopyFile(file, destination, overwriteExisting);
            }
        }

        private static void CreateShortcut(string shortcutPath, string targetPath, string workingDirectory)
        {
            var shellType = Type.GetTypeFromProgID("WScript.Shell");
            if (shellType == null) throw new InvalidOperationException("ไม่พบ Windows Script Host สำหรับสร้าง shortcut");
            dynamic shell = Activator.CreateInstance(shellType);
            var shortcut = shell.CreateShortcut(shortcutPath);
            shortcut.TargetPath = targetPath;
            shortcut.WorkingDirectory = workingDirectory;
            shortcut.Save();
        }
    }
}
