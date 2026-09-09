using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace ReimbursementDocApp
{
    internal static class PayrollRendererSmokeTest
    {
        private const string WordNs = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
        private static readonly Regex TagRegex = new Regex(@"\{[^{}\r\n]{1,80}\}", RegexOptions.Compiled);

        private static int Main()
        {
            var root = AppDomain.CurrentDomain.BaseDirectory;
            var templatePath = Path.Combine(root, "Template", "Payroll", "5. บันทึกอนุมัติเบิกจ่าย.docx");
            var outputPath = Path.Combine(Path.GetTempPath(), "document346-payroll-" + Guid.NewGuid().ToString("N") + ".docx");
            try
            {
                var values = new Dictionary<string, string>
                {
                    { "{คำนำหน้าชื่อ}", "นาย" }, { "{คำนำหน้าลูกจ้าง}", "นาย" }, { "{ชื่อลูกจ้าง}", "สมชาย" }, { "{นามสกุลลูกจ้าง}", "ใจดี" },
                    { "{คำนำหน้าพัสดุ}", "นาง" }, { "{ชื่อพัสดุ}", "พัสดุ" }, { "{นามสกุลพัสดุ}", "ทดสอบ" },
                    { "{คำนำหน้าหพัสดุ}", "นาย" }, { "{ชื่อหพัสดุ}", "หัวหน้า" }, { "{นามสกุลหพัสดุ}", "พัสดุ" },
                    { "{คำนำหน้าการเงิน}", "นางสาว" }, { "{ชื่อการเงิน}", "การเงิน" }, { "{นามสกุลการเงิน}", "ทดสอบ" },
                    { "{โซนหัวหน้าการเงิน}", "" }, { "{หัวหน้าการเงิน}", "นาย หัวหน้าการเงิน ทดสอบ" },
                    { "{ชื่อโรงเรียน}", "โรงเรียนบ้านช่างหม้อ" }, { "{ชื่อผอ}", "ผอ" }, { "{นามสกุลผอ}", "ทดสอบ" },
                    { "{คำนำหน้าผอ}", "นาย" }, { "{ตำแหน่ง}", "ธุรการโรงเรียน 9,000" }, { "{เงินเดือน}", "9,000" }, { "{เงินเดือนTEXT}", "เก้าพันบาทถ้วน" },
                    { "{ชื่อเขต}", "สำนักงานเขตพื้นที่การศึกษาประถมศึกษาแม่ฮ่องสอน เขต 2" }, { "{วันที่ส่งเบิก}", "1 ตุลาคม 2569" }, { "{ใบสั่งจ้าง}", "1/2570" },
                    { "{วันที่สั่งจ้าง}", "1 ตุลาคม 2569" }, { "{กรรมการA}", "นาย ประธาน ทดสอบ" }
                };
                var render = typeof(Program).GetMethod("RenderDocx", BindingFlags.NonPublic | BindingFlags.Static);
                if (render == null) throw new InvalidOperationException("Legacy Payroll renderer was not found.");
                render.Invoke(null, new object[] { templatePath, outputPath, values });
                if (!File.Exists(outputPath)) throw new InvalidOperationException("Payroll renderer did not emit output.");
                var text = ReadParagraphText(outputPath).ToArray();
                var all = string.Join("\n", text);
                if (!all.Contains("นาง") || !all.Contains("พัสดุ") || !all.Contains("นางสาว") || !all.Contains("การเงิน")) throw new InvalidOperationException("Payroll contextual signer values were not emitted.");
                if (all.Contains("โรงเรียนโรงเรียน")) throw new InvalidOperationException("Payroll school prefix was duplicated.");
                if (FindTags(outputPath).Any()) throw new InvalidOperationException("Payroll output has unreplaced tags.");
                Console.WriteLine("PASS: Payroll contextual prefixes and school literal-prefix mapping");
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("FAIL: " + ex.Message);
                return 2;
            }
        }

        private static IEnumerable<string> ReadParagraphText(string path)
        {
            using (var archive = ZipFile.OpenRead(path))
            {
                foreach (var entry in archive.Entries.Where(x => x.FullName == "word/document.xml" || Regex.IsMatch(x.FullName, @"^word/(header|footer)\d+\.xml$")))
                {
                    var xml = new XmlDocument { PreserveWhitespace = true };
                    using (var reader = new StreamReader(entry.Open(), Encoding.UTF8)) xml.Load(reader);
                    var manager = new XmlNamespaceManager(xml.NameTable); manager.AddNamespace("w", WordNs);
                    foreach (XmlNode paragraph in xml.SelectNodes("//w:p[not(.//w:p)]", manager)) yield return string.Join("", paragraph.SelectNodes(".//w:t", manager).Cast<XmlNode>().Select(x => x.InnerText ?? "").ToArray());
                }
            }
        }

        private static IEnumerable<string> FindTags(string path)
        {
            foreach (var text in ReadParagraphText(path)) foreach (Match match in TagRegex.Matches(text)) yield return match.Value;
        }
    }
}
