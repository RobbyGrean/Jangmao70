using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace Khet70
{
    internal static class Khet70Config
    {
        private static readonly JavaScriptSerializer Serializer = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };

        public static T Load<T>(string path)
        {
            if (!File.Exists(path)) throw new FileNotFoundException("ไม่พบไฟล์ config", path);
            var json = File.ReadAllText(path, Encoding.UTF8);
            var result = Serializer.Deserialize<T>(json);
            if (result == null) throw new InvalidDataException("ไฟล์ config ว่าง: " + path);
            return result;
        }
    }

    internal sealed class ProcurementManifest
    {
        public int schemaVersion { get; set; }
        public string module { get; set; }
        public string root { get; set; }
        public ProcurementManifestDocument[] centralDocuments { get; set; }
        public ProcurementManifestRoute[] routes { get; set; }
    }

    internal sealed class ProcurementManifestDocument
    {
        public string id { get; set; }
        public string relativePath { get; set; }
        public int businessTagCount { get; set; }
        public int businessOccurrenceCount { get; set; }
        public string sha256 { get; set; }
    }

    internal sealed class ProcurementManifestRoute
    {
        public string id { get; set; }
        public string positionId { get; set; }
        public bool hasTeaching { get; set; }
        public bool worksAtTwoSchools { get; set; }
        public string tor { get; set; }
        public string quote { get; set; }
        public string torSha256 { get; set; }
        public string quoteSha256 { get; set; }
    }

    internal sealed class ProcurementPositionConfig
    {
        public string id { get; set; }
        public string dropdownLabel { get; set; }
        public string label { get; set; }
        public bool teachingFlag { get; set; }
        public bool twoSchoolsFlag { get; set; }
    }

    internal sealed class SalaryRateCatalog
    {
        public int schemaVersion { get; set; }
        public SalaryRateConfig[] rates { get; set; }
    }

    internal sealed class SalaryRateConfig
    {
        public string salary { get; set; }
        public string salaryText { get; set; }
        public string totalSalary { get; set; }
        public string totalSalaryText { get; set; }
        public string dutyTax { get; set; }
    }

    internal static class Khet70SalaryTable
    {
        public static SalaryRateConfig[] Load()
        {
            var candidates = new[]
            {
                Path.Combine(Khet70Paths.ShippedRoot, "Config", "salary_rates.json"),
                Path.Combine(Environment.CurrentDirectory, "Config", "salary_rates.json"),
                Path.Combine(Khet70Paths.ShippedRoot, "salary_rates.json"),
                Path.Combine(Environment.CurrentDirectory, "salary_rates.json")
            };
            var path = candidates.FirstOrDefault(File.Exists);
            if (path != null)
            {
                var catalog = Khet70Config.Load<SalaryRateCatalog>(path);
                var rates = (catalog.rates ?? new SalaryRateConfig[0])
                    .Where(x => x != null && !string.IsNullOrWhiteSpace(x.salary))
                    .ToArray();
                if (rates.Length > 0) return rates;
            }

            return new[]
            {
                new SalaryRateConfig { salary = "9,000", salaryText = "เก้าพันบาทถ้วน", totalSalary = "108,000", totalSalaryText = "หนึ่งแสนแปดพันบาทถ้วน", dutyTax = "108" },
                new SalaryRateConfig { salary = "15,000", salaryText = "หนึ่งหมื่นห้าพันบาทถ้วน", totalSalary = "180,000", totalSalaryText = "หนึ่งแสนแปดหมื่นบาทถ้วน", dutyTax = "180" },
                new SalaryRateConfig { salary = "18,000", salaryText = "หนึ่งหมื่นแปดพันบาทถ้วน", totalSalary = "216,000", totalSalaryText = "สองแสนหนึ่งหมื่นหกพันบาทถ้วน", dutyTax = "216" }
            };
        }
    }

    internal sealed class ProcurementPositionCatalog
    {
        public int schemaVersion { get; set; }
        public string module { get; set; }
        public ProcurementPositionConfig[] positions { get; set; }
    }
}

