using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

internal static class DocxSmokeTest
{
    private static readonly Regex TagRegex = new Regex(@"\{[^{}\r\n]{1,80}\}", RegexOptions.Compiled);
    private static readonly Regex WordXmlRegex = new Regex(@"^word/(document|header\d+|footer\d+)\.xml$", RegexOptions.Compiled);
    private const string WordNs = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";

    private static int Main()
    {
        var root = AppDomain.CurrentDomain.BaseDirectory;
        var appExe = Path.Combine(root, "dist", "ReimbursementDocApp.exe");
        var templateDir = Path.Combine(root, "dist", "Template");
        var outputDir = Path.Combine(root, "smoke-output");
        var templateTagsPath = Path.Combine(root, "template_tags.json");

        if (!File.Exists(appExe) || !Directory.Exists(templateDir) || !File.Exists(templateTagsPath))
        {
            Console.WriteLine("Missing test inputs.");
            return 1;
        }

        Directory.CreateDirectory(outputDir);
        var templates = LoadTemplates(templateTagsPath);
        var values = BuildSampleValues();

        var assembly = Assembly.LoadFrom(appExe);
        var programType = assembly.GetType("ReimbursementDocApp.Program", true);
        var render = programType.GetMethod("RenderDocx", BindingFlags.NonPublic | BindingFlags.Static);
        if (render == null)
        {
            Console.WriteLine("RenderDocx not found.");
            return 1;
        }

        var failures = new List<string>();
        VerifyStaticSchoolMappings(Path.Combine(root, "dist", "app_database.json"), failures);
        VerifyDistrictNameNormalization(assembly, failures);
        foreach (var template in templates.Keys)
        {
            var templatePath = Path.Combine(templateDir, template);
            var outputPath = Path.Combine(outputDir, Path.GetFileNameWithoutExtension(template) + "_SMOKE.docx");
            if (!File.Exists(templatePath))
            {
                failures.Add("Missing template: " + template);
                continue;
            }

            render.Invoke(null, new object[] { templatePath, outputPath, values });
            var leftovers = FindUnreplacedTags(outputPath).ToArray();
            if (leftovers.Length > 0)
            {
                failures.Add(template + " has unreplaced tags: " + string.Join(", ", leftovers));
            }
            else
            {
                Console.WriteLine("OK: " + template);
            }
        }

        if (failures.Count > 0)
        {
            Console.WriteLine("FAIL");
            foreach (var failure in failures) Console.WriteLine(failure);
            return 2;
        }

        Console.WriteLine("PASS");
        return 0;
    }

    private static void VerifyDistrictNameNormalization(Assembly assembly, List<string> failures)
    {
        var programType = assembly.GetType("ReimbursementDocApp.Program", true);
        var mainFormType = programType.GetNestedType("MainForm", BindingFlags.NonPublic);
        var normalizer = mainFormType == null
            ? null
            : mainFormType.GetMethod("NormalizeDistrictName", BindingFlags.NonPublic | BindingFlags.Static);
        if (normalizer == null)
        {
            failures.Add("NormalizeDistrictName was not found.");
            return;
        }

        var shortName = (string)normalizer.Invoke(null, new object[] { "  สงขลา เขต 1  " });
        var fullName = (string)normalizer.Invoke(null, new object[] { "สำนักงานเขตพื้นที่การศึกษาประถมศึกษาสงขลา เขต 1" });
        var duplicateName = (string)normalizer.Invoke(null, new object[] { "สำนักงานเขตพื้นที่การศึกษาประถมศึกษา สำนักงานเขตพื้นที่การศึกษาประถมศึกษา สงขลา เขต 1" });
        var defaultValue = (string)normalizer.Invoke(null, new object[] { "   " });
        if (shortName != "สำนักงานเขตพื้นที่การศึกษาประถมศึกษาสงขลา เขต 1") failures.Add("District name prefix normalization failed.");
        if (fullName != "สำนักงานเขตพื้นที่การศึกษาประถมศึกษาสงขลา เขต 1") failures.Add("District name full value normalization failed.");
        if (duplicateName != "สำนักงานเขตพื้นที่การศึกษาประถมศึกษาสงขลา เขต 1") failures.Add("District name duplicate prefix normalization failed.");
        if (defaultValue != "สำนักงานเขตพื้นที่การศึกษาประถมศึกษาแม่ฮ่องสอน เขต 2") failures.Add("District name default value mismatch.");
    }

    private static Dictionary<string, string[]> LoadTemplates(string path)
    {
        var json = File.ReadAllText(path, Encoding.UTF8);
        var blockRegex = new Regex("\"([^\"]+)\"\\s*:\\s*\\[(.*?)\\]", RegexOptions.Singleline);
        var stringRegex = new Regex("\"([^\"]+)\"");
        var result = new Dictionary<string, string[]>();
        foreach (Match block in blockRegex.Matches(json))
        {
            var name = UnescapeJson(block.Groups[1].Value);
            var tags = stringRegex.Matches(block.Groups[2].Value).Cast<Match>().Select(x => UnescapeJson(x.Groups[1].Value)).ToArray();
            result[name] = tags;
        }
        return result;
    }

    private static void VerifyStaticSchoolMappings(string databasePath, List<string> failures)
    {
        if (!File.Exists(databasePath))
        {
            failures.Add("app_database.json not found for static school position mapping test.");
            return;
        }

        var json = File.ReadAllText(databasePath, Encoding.UTF8);
        if (Regex.IsMatch(json, "\"khetPositions\"\\s*:")) failures.Add("khetPositions must not be included in the school application database.");

        var section = Regex.Match(json, "\"schoolPositions\"\\s*:\\s*\\[(.*?)\\]\\s*(,|})", RegexOptions.Singleline);
        if (!section.Success)
        {
            failures.Add("schoolPositions section not found.");
            return;
        }

        var expected = new Dictionary<string, string[]>
        {
            { "ธุรการโรงเรียน 9,000", new[] { "9,000", "เก้าพันบาทถ้วน", "108,000", "หนึ่งแสนแปดพันบาทถ้วน" } },
            { "ธุรการโรงเรียน 15,000", new[] { "15,000", "หนึ่งหมื่นห้าพันบาทถ้วน", "180,000", "หนึ่งแสนแปดหมื่นบาทถ้วน" } },
            { "นักการภารโรง", new[] { "9,000", "เก้าพันบาทถ้วน", "108,000", "หนึ่งแสนแปดพันบาทถ้วน" } },
            { "พี่เลี้ยงเด็กพิการ", new[] { "9,000", "เก้าพันบาทถ้วน", "108,000", "หนึ่งแสนแปดพันบาทถ้วน" } },
            { "ครูพักนอน", new[] { "9,000", "เก้าพันบาทถ้วน", "108,000", "หนึ่งแสนแปดพันบาทถ้วน" } },
            { "ครูผู้ทรงคุณค่าแห่งแผ่นดิน", new[] { "17,000", "หนึ่งหมื่นเจ็ดพันบาทถ้วน", "161,500", "หนึ่งแสนหกหมื่นหนึ่งพันห้าร้อยบาทถ้วน" } }
        };

        var count = Regex.Matches(section.Groups[1].Value, "\"position\"\\s*:").Count;
        if (count != expected.Count) failures.Add("Expected exactly 6 school positions, found " + count + ".");

        foreach (var item in expected)
        {
            var pattern = "\"position\"\\s*:\\s*\"" + Regex.Escape(item.Key) + "\".*?"
                + "\"salary\"\\s*:\\s*\"" + Regex.Escape(item.Value[0]) + "\".*?"
                + "\"salaryText\"\\s*:\\s*\"" + Regex.Escape(item.Value[1]) + "\".*?"
                + "\"totalSalary\"\\s*:\\s*\"" + Regex.Escape(item.Value[2]) + "\".*?"
                + "\"totalSalaryText\"\\s*:\\s*\"" + Regex.Escape(item.Value[3]) + "\"";
            if (!Regex.IsMatch(section.Groups[1].Value, pattern, RegexOptions.Singleline))
            {
                failures.Add("Static mapping mismatch for position: " + item.Key);
            }
        }
    }

    private static IEnumerable<string> FindUnreplacedTags(string docxPath)
    {
        using (var archive = ZipFile.OpenRead(docxPath))
        {
            foreach (var entry in archive.Entries.Where(x => WordXmlRegex.IsMatch(x.FullName)))
            {
                using (var stream = entry.Open())
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    var xml = new XmlDocument { PreserveWhitespace = true };
                    xml.LoadXml(reader.ReadToEnd());
                    var nsm = new XmlNamespaceManager(xml.NameTable);
                    nsm.AddNamespace("w", WordNs);
                    var textNodes = xml.SelectNodes("//w:t", nsm);
                    if (textNodes == null) continue;
                    var combinedText = new StringBuilder();
                    foreach (XmlNode node in textNodes)
                    {
                        combinedText.Append(node.InnerText ?? "");
                    }
                    foreach (Match match in TagRegex.Matches(combinedText.ToString()))
                    {
                        yield return match.Value;
                    }
                }
            }
        }
    }

    private static string UnescapeJson(string value)
    {
        return Regex.Replace(value, @"\\u([0-9a-fA-F]{4})", m => ((char)Convert.ToInt32(m.Groups[1].Value, 16)).ToString())
            .Replace("\\\"", "\"")
            .Replace("\\\\", "\\");
    }

    private static Dictionary<string, string> BuildSampleValues()
    {
        return new Dictionary<string, string>
        {
            { "{เดือนที่ส่งมอบ}", "กรกฎาคม" },
            { "{ย่อเดือนที่ส่งมอบ}", "ก.ค." },
            { "{วันที่ส่งเบิก}", "3 สิงหาคม 2569" },
            { "{ปีใบสั่งจ้าง}", "2569" },
            { "{วันที่สั่งจ้าง}", "1 กรกฎาคม 2569" },
            { "{ใบสั่งจ้าง}", "25/2569" },
            { "{ชื่อโรงเรียน}", "โรงเรียนบ้านช่างหม้อ" },
            { "{ชื่อเขต}", "สำนักงานเขตพื้นที่การศึกษาประถมศึกษาแม่ฮ่องสอน เขต 2" },
            { "{ตำแหน่ง}", "ครูผู้ทรงคุณค่าแห่งแผ่นดิน" },
            { "{เงินเดือน}", "17,000" },
            { "{เงินเดือนTEXT}", "หนึ่งหมื่นเจ็ดพันบาทถ้วน" },
            { "{เงินรวม}", "161,500" },
            { "{เงินรวมTEXT}", "หนึ่งแสนหกหมื่นหนึ่งพันห้าร้อยบาทถ้วน" },
            { "{คำนำหน้าผอ}", "นาย" },
            { "{ชื่อผอ}", "ผอทดสอบ" },
            { "{นามสกุลผอ}", "โรงเรียน" },
            { "{คำนำหน้าพัสดุ}", "นาง" },
            { "{ชื่อพัสดุ}", "พัสดุ" },
            { "{นามสกุลพัสดุ}", "ทดสอบ" },
            { "{คำนำหน้าหพัสดุ}", "นาย" },
            { "{ชื่อหพัสดุ}", "หัวหน้า" },
            { "{นามสกุลหพัสดุ}", "พัสดุ" },
            { "{คำนำหน้าการเงิน}", "นางสาว" },
            { "{ชื่อการเงิน}", "การเงิน" },
            { "{นามสกุลการเงิน}", "ทดสอบ" },
            { "{คำนำหน้าลูกจ้าง}", "นาย" },
            { "{คำนำหน้าชื่อ}", "นาย" },
            { "{ชื่อลูกจ้าง}", "สมชาย" },
            { "{นามสกุลลูกจ้าง}", "ใจดี" },
            { "{กรรมการA}", "นาย ประธาน หนึ่ง" },
            { "{กรรมการ B}", "นาย กรรมการ สอง" },
            { "{กรรมการ C}", "นาย กรรมการ สาม" },
            { "{กรรมการB}", "นาย กรรมการ สอง" },
            { "{กรรมการC}", "นาย กรรมการ สาม" },
            { "{โซนหัวหน้าการเงิน}", "" }
        };
    }
}
