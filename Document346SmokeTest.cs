using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace ReimbursementDocApp
{
    internal static class Document346SmokeTest
    {
        private const string WordNs = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
        private static readonly Regex TagRegex = new Regex(@"\{[^{}\r\n]{1,80}\}", RegexOptions.Compiled);

        private static int Main()
        {
            var root = AppDomain.CurrentDomain.BaseDirectory;
            var templateRoot = Path.Combine(root, "Template", "Procurement");
            var manifestPath = Path.Combine(root, "Config", "Procurement", "template_manifest.json");
            var failures = new List<string>();
            if (!Directory.Exists(templateRoot)) failures.Add("Missing Procurement template root: " + templateRoot);
            if (!File.Exists(manifestPath)) failures.Add("Missing Procurement manifest: " + manifestPath);
            if (failures.Count == 0)
            {
                var manifest = Document346Config.Load<ProcurementManifest>(manifestPath);
                VerifyManifest(templateRoot, manifest, failures);
                VerifyCatalogAndRoutes(root, manifest, failures);
                VerifyRenderedDocuments(templateRoot, manifest, failures);
                VerifyTagLikeValueIsHandled(templateRoot, manifest, failures);
                VerifyWorkingRecordClone(failures);
                VerifyStoreRoundTrip(failures);
            }
            if (failures.Count > 0)
            {
                Console.WriteLine("FAIL");
                foreach (var failure in failures) Console.WriteLine(failure);
                return 2;
            }
            Console.WriteLine("PASS: Procurement manifest, routes, renderer and WorkingRecord clone");
            return 0;
        }

        private static void VerifyManifest(string templateRoot, ProcurementManifest manifest, List<string> failures)
        {
            var paths = new List<string>();
            paths.AddRange(manifest.centralDocuments.Select(x => x.relativePath));
            foreach (var route in manifest.routes)
            {
                paths.Add(route.tor);
                paths.Add(route.quote);
            }
            if (manifest.routes.Length != 13) failures.Add("Expected 13 route pairs, found " + manifest.routes.Length);
            if (paths.Count != 34 || paths.Distinct(StringComparer.Ordinal).Count() != 34) failures.Add("Manifest must resolve to 34 unique Procurement paths.");
            foreach (var path in paths)
            {
                var full = Document346Paths.ResolveUnder(templateRoot, path);
                if (!File.Exists(full)) failures.Add("Missing route/central template: " + path);
            }
        }

        private static void VerifyCatalogAndRoutes(string root, ProcurementManifest manifest, List<string> failures)
        {
            var catalogPath = Path.Combine(root, "Config", "Procurement", "position_catalog.json");
            if (!File.Exists(catalogPath))
            {
                failures.Add("Missing Procurement position catalog.");
                return;
            }
            var catalog = Document346Config.Load<ProcurementPositionCatalog>(catalogPath);
            if (catalog.positions == null || catalog.positions.Length != 5) failures.Add("Expected 5 active Procurement positions.");
            var positions = (catalog.positions ?? new ProcurementPositionConfig[0]).ToDictionary(x => x.id, StringComparer.Ordinal);
            foreach (var position in positions.Values)
            {
                if (string.IsNullOrWhiteSpace(position.salary) || string.IsNullOrWhiteSpace(position.totalSalary)) failures.Add("Position has missing salary mapping: " + position.id);
                foreach (var teaching in new[] { false, true })
                {
                    foreach (var twoSchools in new[] { false, true })
                    {
                        var route = manifest.routes.FirstOrDefault(x => x.positionId == position.id && x.hasTeaching == teaching && x.worksAtTwoSchools == twoSchools);
                        var supported = (!teaching || position.teachingFlag) && (!twoSchools || position.twoSchoolsFlag);
                        if (supported != (route != null)) failures.Add("Route support mismatch: " + position.id + " teaching=" + teaching + " twoSchools=" + twoSchools);
                        if (route != null && (string.IsNullOrWhiteSpace(route.tor) || string.IsNullOrWhiteSpace(route.quote))) failures.Add("Route has empty template path: " + route.id);
                    }
                }
            }
            if (manifest.routes.Select(x => x.id).Distinct(StringComparer.Ordinal).Count() != manifest.routes.Length) failures.Add("Route IDs must be unique.");
        }

        private static void VerifyRenderedDocuments(string templateRoot, ProcurementManifest manifest, List<string> failures)
        {
            var outputRoot = Path.Combine(Path.GetTempPath(), "document346-smoke-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(outputRoot);
            var values = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var path in AllManifestPaths(manifest))
            {
                var templatePath = Document346Paths.ResolveUnder(templateRoot, path);
                foreach (var tag in FindTags(templatePath))
                {
                    if (!values.ContainsKey(tag)) values[tag] = "ทดสอบ";
                }
            }
            values["{เลขประจำตัว}"] = "0123456789012";
            values["{คำสั่งสเปค}"] = "1/2570";
            var index = 0;
            foreach (var path in AllManifestPaths(manifest))
            {
                var templatePath = Document346Paths.ResolveUnder(templateRoot, path);
                var outputPath = Path.Combine(outputRoot, index.ToString("00") + "_" + Path.GetFileName(path));
                try
                {
                    var actual = Document346DocxRenderer.Render(templatePath, outputPath, values);
                    if (!File.Exists(actual)) failures.Add("Renderer did not emit: " + path);
                    if (Document346DocxRenderer.FindBusinessTags(actual).Count > 0) failures.Add("Generated DOCX has tags: " + path);
                    using (var archive = ZipFile.OpenRead(actual))
                    {
                        foreach (var entry in archive.Entries)
                        {
                            using (var stream = entry.Open()) { while (stream.ReadByte() >= 0) { } }
                        }
                    }
                }
                catch (Exception ex)
                {
                    failures.Add("Render failed for " + path + ": " + ex.Message);
                }
                index++;
            }
            Console.WriteLine("Rendered " + index + " Procurement templates to " + outputRoot);
        }

        private static IEnumerable<string> AllManifestPaths(ProcurementManifest manifest)
        {
            foreach (var document in manifest.centralDocuments) yield return document.relativePath;
            foreach (var route in manifest.routes) { yield return route.tor; yield return route.quote; }
        }

        private static IEnumerable<string> FindTags(string path)
        {
            var result = new HashSet<string>(StringComparer.Ordinal);
            using (var archive = ZipFile.OpenRead(path))
            {
                foreach (var entry in archive.Entries)
                {
                    if (entry.FullName != "word/document.xml" && !Regex.IsMatch(entry.FullName, @"^word/(header|footer)\d+\.xml$")) continue;
                    var xml = new XmlDocument { PreserveWhitespace = true };
                    using (var reader = new StreamReader(entry.Open(), Encoding.UTF8)) xml.Load(reader);
                    var manager = new XmlNamespaceManager(xml.NameTable);
                    manager.AddNamespace("w", WordNs);
                    foreach (XmlNode paragraph in xml.SelectNodes("//w:p[not(.//w:p)]", manager))
                    {
                        var text = string.Join("", paragraph.SelectNodes(".//w:t", manager).Cast<XmlNode>().Select(x => x.InnerText ?? "").ToArray());
                        foreach (Match match in TagRegex.Matches(text)) result.Add(match.Value);
                    }
                }
            }
            return result;
        }

        private static void VerifyWorkingRecordClone(List<string> failures)
        {
            var store = new Document346Store();
            var source = WorkingRecord.CreateEmpty();
            source.Employee.NationalId = "0123456789012";
            source.Employee.GivenName = "ชื่อ มีช่องว่าง";
            source.Employee.Road = "ถนนทดสอบ";
            source.Committee[0].Prefix = "ว่าที่ร้อยตรี";
            source.Committee[0].GivenName = "ก";
            var clone = store.Clone(source);
            source.Employee.NationalId = "9999999999999";
            source.Committee[0].GivenName = "เปลี่ยนต้นฉบับ";
            if (clone.Employee.NationalId != "0123456789012" || clone.Employee.Road != "ถนนทดสอบ" || clone.Committee[0].GivenName != "ก") failures.Add("WorkingRecord clone is not isolated or lost leading zero.");
        }

        private static void VerifyTagLikeValueIsHandled(string templateRoot, ProcurementManifest manifest, List<string> failures)
        {
            var template = AllManifestPaths(manifest).Select(x => Document346Paths.ResolveUnder(templateRoot, x)).FirstOrDefault(x => FindTags(x).Contains("{คำสั่งสเปค}"));
            if (template == null)
            {
                failures.Add("No template was available for tag-like replacement safety check.");
                return;
            }
            var values = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var tag in FindTags(template)) values[tag] = "ทดสอบ";
            values["{คำสั่งสเปค}"] = "{คำสั่งสเปค}";
            var output = Path.Combine(Path.GetTempPath(), "document346-tag-value-" + Guid.NewGuid().ToString("N") + ".docx");
            try
            {
                Document346DocxRenderer.Render(template, output, values);
                failures.Add("Renderer accepted a tag-like replacement without reporting the remaining literal tag.");
            }
            catch (InvalidDataException)
            {
                // The renderer must fail fast; literal business tags are not allowed in output.
            }
            catch (Exception ex)
            {
                failures.Add("Unexpected tag-like replacement failure: " + ex.Message);
            }
        }

        private static void VerifyStoreRoundTrip(List<string> failures)
        {
            var directory = Path.Combine(Path.GetTempPath(), "document346-store-" + Guid.NewGuid().ToString("N"));
            var store = new Document346Store(directory);
            var data = new Document346StoreData();
            data.Templates.Add(new SavedTemplateSnapshot { Name = "ชื่อ \"ไทย\"", Record = new WorkingRecord { Employee = new EmployeeRecord { NationalId = "0123456789012", Road = "ถนน\\ทดสอบ" } } });
            store.Save(data);
            var loaded = store.Load();
            if (loaded.Templates.Count != 1 || loaded.Templates[0].Record.Employee.NationalId != "0123456789012" || loaded.Templates[0].Record.Employee.Road != "ถนน\\ทดสอบ") failures.Add("Store round-trip failed for Thai text, quote, backslash or leading zero.");
            File.WriteAllText(Path.Combine(directory, "saved_templates.json"), "{", Encoding.UTF8);
            try
            {
                store.Load();
                failures.Add("Invalid store JSON was accepted.");
            }
            catch (Document346StoreException)
            {
            }
        }
    }
}
