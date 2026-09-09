using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ReimbursementDocInstaller
{
    internal static class Program
    {
        private const string AppName = "ReimbursementDocApp";

        [STAThread]
        private static void Main()
        {
            try
            {
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                var payload = Path.Combine(baseDir, "Payload");
                if (!Directory.Exists(payload))
                {
                    MessageBox.Show("Payload folder was not found. Put Payload next to ReimbursementDocApp-Setup.exe.", "Installer");
                    return;
                }

                var installDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppName);
                var templateDir = Path.Combine(installDir, "Template");
                var outputDir = Path.Combine(installDir, "output");
                var payloadTemplateDir = Path.Combine(payload, "Template");

                Directory.CreateDirectory(installDir);
                Directory.CreateDirectory(templateDir);
                Directory.CreateDirectory(outputDir);

                CopyRequiredFile(payload, installDir, "ReimbursementDocApp.exe");
                CopyRequiredFile(payload, installDir, "template_tags.json");
                CopyRequiredFile(payload, installDir, "app_database.json");
                CopyRequiredFile(payload, installDir, "Uninstall ReimbursementDocApp.exe");

                var templateSourceDir = Directory.Exists(payloadTemplateDir) ? payloadTemplateDir : payload;
                foreach (var file in Directory.GetFiles(templateSourceDir, "*.docx").OrderBy(x => x))
                {
                    File.Copy(file, Path.Combine(templateDir, Path.GetFileName(file)), true);
                }

                var appExe = Path.Combine(installDir, "ReimbursementDocApp.exe");
                var uninstallExe = Path.Combine(installDir, "Uninstall ReimbursementDocApp.exe");

                CreateShortcut(
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "ReimbursementDocApp.lnk"),
                    appExe,
                    installDir);

                var startMenuDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), AppName);
                Directory.CreateDirectory(startMenuDir);
                CreateShortcut(Path.Combine(startMenuDir, "ReimbursementDocApp.lnk"), appExe, installDir);
                CreateShortcut(Path.Combine(startMenuDir, "Uninstall ReimbursementDocApp.lnk"), uninstallExe, installDir);

                System.Diagnostics.Process.Start(appExe);
                MessageBox.Show("Installed successfully:\n" + installDir, "Installer");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Installer Error");
            }
        }

        private static void CopyRequiredFile(string sourceDir, string targetDir, string fileName)
        {
            var source = Path.Combine(sourceDir, fileName);
            if (!File.Exists(source)) throw new FileNotFoundException("Required file missing from Payload: " + fileName);
            File.Copy(source, Path.Combine(targetDir, fileName), true);
        }

        private static void CreateShortcut(string shortcutPath, string targetPath, string workingDirectory)
        {
            var shellType = Type.GetTypeFromProgID("WScript.Shell");
            dynamic shell = Activator.CreateInstance(shellType);
            var shortcut = shell.CreateShortcut(shortcutPath);
            shortcut.TargetPath = targetPath;
            shortcut.WorkingDirectory = workingDirectory;
            shortcut.Save();
        }
    }
}
