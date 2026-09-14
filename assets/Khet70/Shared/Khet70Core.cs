using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;

namespace Khet70
{
    internal enum DocumentModule
    {
        Payroll,
        Procurement
    }

    internal static class Khet70Paths
    {
        public const string AppId = "Khet70";
        public const int CurrentSchemaVersion = 1;

        public static string UserRoot
        {
            get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppId); }
        }

        public static string DataRoot
        {
            get { return Path.Combine(UserRoot, "Data"); }
        }

        public static string ModuleDataRoot(DocumentModule module)
        {
            return Path.Combine(DataRoot, module == DocumentModule.Payroll ? "Payroll" : "Procurement");
        }

        public static string TemplateRoot(DocumentModule module)
        {
            return Path.Combine(UserRoot, "Template", module == DocumentModule.Payroll ? "Payroll" : "Procurement");
        }

        public static string OutputRoot(DocumentModule module)
        {
            return Path.Combine(UserRoot, "Output", module == DocumentModule.Payroll ? "เงินเดือน" : "เอกสารจัดจ้าง");
        }

        public static string ShippedRoot
        {
            get { return AppDomain.CurrentDomain.BaseDirectory; }
        }

        public static string ShippedTemplateRoot(DocumentModule module)
        {
            var defaultRoot = Path.Combine(ShippedRoot, "Defaults", "Template", module == DocumentModule.Payroll ? "Payroll" : "Procurement");
            if (Directory.Exists(defaultRoot)) return defaultRoot;
            return Path.Combine(ShippedRoot, "Template", module == DocumentModule.Payroll ? "Payroll" : "Procurement");
        }

        public static string ShippedConfigRoot(DocumentModule module)
        {
            return Path.Combine(ShippedRoot, "Config", module == DocumentModule.Payroll ? "Payroll" : "Procurement");
        }

        public static string EnsureUserTemplateRoot(DocumentModule module)
        {
            var target = TemplateRoot(module);
            Directory.CreateDirectory(target);
            var shipped = ShippedTemplateRoot(module);
            if (!Directory.Exists(shipped)) return target;

            foreach (var sourceFile in Directory.GetFiles(shipped, "*", SearchOption.AllDirectories))
            {
                var relative = sourceFile.Substring(shipped.TrimEnd(Path.DirectorySeparatorChar).Length + 1);
                var targetFile = ResolveUnder(target, relative);
                var targetDirectory = Path.GetDirectoryName(targetFile);
                if (!string.IsNullOrWhiteSpace(targetDirectory)) Directory.CreateDirectory(targetDirectory);
                if (!File.Exists(targetFile))
                {
                    // Normal startup only seeds missing files; Restore is the explicit overwrite action.
                    File.Copy(sourceFile, targetFile, true);
                }
            }
            return target;
        }

        public static string RestoreUserTemplateRoot(DocumentModule module)
        {
            var target = TemplateRoot(module);
            var shipped = ShippedTemplateRoot(module);
            if (!Directory.Exists(shipped)) throw new DirectoryNotFoundException("ไม่พบชุด Template เริ่มต้น: " + shipped);
            if (string.Equals(Path.GetFullPath(target).TrimEnd(Path.DirectorySeparatorChar), Path.GetFullPath(shipped).TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("โฟลเดอร์ Template สำหรับแก้ไขต้องแยกจากชุดเริ่มต้น");
            }
            var backup = "";
            if (Directory.Exists(target))
            {
                var moduleName = module == DocumentModule.Payroll ? "Payroll" : "Procurement";
                var backupRoot = Path.Combine(DataRoot, "TemplateBackups", moduleName + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmss"));
                Directory.CreateDirectory(Path.GetDirectoryName(backupRoot));
                CopyDirectory(target, backupRoot);
                backup = backupRoot;
                Directory.Delete(target, true);
            }
            CopyDirectory(shipped, target);
            return backup;
        }

        private static void CopyDirectory(string source, string target)
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
                var destinationDirectory = Path.GetDirectoryName(destination);
                if (!string.IsNullOrWhiteSpace(destinationDirectory)) Directory.CreateDirectory(destinationDirectory);
                File.Copy(file, destination, true);
            }
        }

        public static string SavedTemplatesPath
        {
            get { return Path.Combine(DataRoot, "saved_templates.json"); }
        }

        public static string SavedTemplatesPathFor(DocumentModule module)
        {
            return Path.Combine(ModuleDataRoot(module), "saved_templates.json");
        }

        public static string PreferencesPath
        {
            get { return Path.Combine(DataRoot, "preferences.json"); }
        }

        public static string ResolveUnder(string root, string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath) || Path.IsPathRooted(relativePath))
            {
                throw new InvalidDataException("Path must be relative: " + relativePath);
            }

            var normalizedRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            var fullPath = Path.GetFullPath(Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar)));
            if (!fullPath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException("Path escapes its module root: " + relativePath);
            }
            return fullPath;
        }
    }

    internal class PersonRecord
    {
        public string Prefix { get; set; }
        public string GivenName { get; set; }
        public string Surname { get; set; }
        public string Position { get; set; }

        public PersonRecord()
        {
            Prefix = "";
            GivenName = "";
            Surname = "";
            Position = "";
        }
    }

    internal sealed class CommitteeMemberRecord : PersonRecord
    {
    }

    internal sealed class SchoolRecord
    {
        public string Name { get; set; }
        public string District { get; set; }
        public string DistrictShort { get; set; }
        public PersonRecord Director { get; set; }
        public PersonRecord SupplyOfficer { get; set; }
        public PersonRecord HeadSupplyOfficer { get; set; }
        public PersonRecord FinanceDirector { get; set; }
        public PersonRecord DeputyDistrictDirector { get; set; }

        public SchoolRecord()
        {
            Name = "";
            District = "";
            DistrictShort = "";
            Director = new PersonRecord();
            SupplyOfficer = new PersonRecord();
            HeadSupplyOfficer = new PersonRecord();
            FinanceDirector = new PersonRecord();
            DeputyDistrictDirector = new PersonRecord();
        }
    }

    internal sealed class EmployeeRecord
    {
        public string Prefix { get; set; }
        public string GivenName { get; set; }
        public string Surname { get; set; }
        public string NationalId { get; set; }
        public string BirthDay { get; set; }
        public string BirthMonth { get; set; }
        public string BirthYear { get; set; }
        public string Age { get; set; }
        public string Nationality { get; set; }
        public string Race { get; set; }
        public string Religion { get; set; }
        public string IdIssueDistrict { get; set; }
        public string IdIssueProvince { get; set; }
        public string IdIssueDay { get; set; }
        public string IdIssueMonth { get; set; }
        public string IdIssueYear { get; set; }
        public string IdExpiryDay { get; set; }
        public string IdExpiryMonth { get; set; }
        public string IdExpiryYear { get; set; }
        public string EducationLevel { get; set; }
        public string Qualification { get; set; }
        public string HouseNumber { get; set; }
        public string Road { get; set; }
        public string Subdistrict { get; set; }
        public string District { get; set; }
        public string Province { get; set; }

        public EmployeeRecord()
        {
            Prefix = "";
            GivenName = "";
            Surname = "";
            NationalId = "";
            BirthDay = "";
            BirthMonth = "";
            BirthYear = "";
            Age = "";
            Nationality = "";
            Race = "";
            Religion = "";
            IdIssueDistrict = "";
            IdIssueProvince = "";
            IdIssueDay = "";
            IdIssueMonth = "";
            IdIssueYear = "";
            IdExpiryDay = "";
            IdExpiryMonth = "";
            IdExpiryYear = "";
            EducationLevel = "";
            Qualification = "";
            HouseNumber = "";
            Road = "";
            Subdistrict = "";
            District = "";
            Province = "";
        }
    }

    internal sealed class PayrollState
    {
        public string PositionId { get; set; }
        public string FiscalMonth { get; set; }
        public string FiscalYear { get; set; }
        public string OrderNumber { get; set; }
        public string OrderDate { get; set; }
        public bool HasHeadFinance { get; set; }
        public PersonRecord HeadFinance { get; set; }
        public List<string> SelectedDocuments { get; set; }
        public string QuickLoadedTemplateId { get; set; }
        public string LastGeneratedFiscalMonth { get; set; }
        public string LastGeneratedFiscalYear { get; set; }
        public string LastGeneratedAt { get; set; }

        public PayrollState()
        {
            PositionId = "";
            FiscalMonth = "";
            FiscalYear = "";
            OrderNumber = "";
            OrderDate = "";
            HasHeadFinance = false;
            HeadFinance = new PersonRecord();
            SelectedDocuments = new List<string>();
            QuickLoadedTemplateId = "";
            LastGeneratedFiscalMonth = "";
            LastGeneratedFiscalYear = "";
            LastGeneratedAt = "";
        }
    }

    internal sealed class ProcurementState
    {
        public string PositionId { get; set; }
        public string Salary { get; set; }
        public bool HasTeaching { get; set; }
        public bool WorksAtTwoSchools { get; set; }
        public string SecondSchoolName { get; set; }
        public string SpecificationOrder { get; set; }
        public List<string> SelectedDocuments { get; set; }

        public ProcurementState()
        {
            PositionId = "";
            Salary = "";
            HasTeaching = false;
            WorksAtTwoSchools = false;
            SecondSchoolName = "";
            SpecificationOrder = "";
            SelectedDocuments = new List<string>();
        }
    }

    internal sealed class WorkingRecord
    {
        public int SchemaVersion { get; set; }
        public string RecordId { get; set; }
        public SchoolRecord School { get; set; }
        public EmployeeRecord Employee { get; set; }
        public CommitteeMemberRecord[] Committee { get; set; }
        public PayrollState Payroll { get; set; }
        public ProcurementState Procurement { get; set; }

        public WorkingRecord()
        {
            SchemaVersion = Khet70Paths.CurrentSchemaVersion;
            RecordId = Guid.NewGuid().ToString("N");
            School = new SchoolRecord();
            Employee = new EmployeeRecord();
            Committee = new[] { new CommitteeMemberRecord(), new CommitteeMemberRecord(), new CommitteeMemberRecord() };
            Payroll = new PayrollState();
            Procurement = new ProcurementState();
        }

        public static WorkingRecord CreateEmpty()
        {
            return new WorkingRecord();
        }
    }

    internal sealed class SavedTemplateSnapshot
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
        public string CreatedAt { get; set; }
        public string UpdatedAt { get; set; }
        public string LastGeneratedFiscalMonth { get; set; }
        public string LastGeneratedFiscalYear { get; set; }
        public string LastGeneratedAt { get; set; }
        public WorkingRecord Record { get; set; }

        public SavedTemplateSnapshot()
        {
            Id = Guid.NewGuid().ToString("N");
            Name = "";
            Note = "";
            CreatedAt = "";
            UpdatedAt = "";
            LastGeneratedFiscalMonth = "";
            LastGeneratedFiscalYear = "";
            LastGeneratedAt = "";
            Record = WorkingRecord.CreateEmpty();
        }
    }

    internal sealed class Khet70StoreData
    {
        public int SchemaVersion { get; set; }
        public List<SavedTemplateSnapshot> Templates { get; set; }
        public List<string> CustomDistricts { get; set; }
        public List<string> CustomPrefixes { get; set; }
        public List<string> CustomQualifications { get; set; }

        public Khet70StoreData()
        {
            SchemaVersion = Khet70Paths.CurrentSchemaVersion;
            Templates = new List<SavedTemplateSnapshot>();
            CustomDistricts = new List<string>();
            CustomPrefixes = new List<string>();
            CustomQualifications = new List<string>();
        }
    }

    internal sealed class Khet70StoreException : Exception
    {
        public Khet70StoreException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    internal sealed class Khet70Store
    {
        private readonly JavaScriptSerializer serializer = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
        private readonly string dataRoot;
        private readonly string legacyDataRoot;

        public Khet70Store()
            : this(Khet70Paths.DataRoot, "")
        {
        }

        internal Khet70Store(string testDataRoot)
            : this(testDataRoot, "")
        {
        }

        internal Khet70Store(DocumentModule module)
            : this(Khet70Paths.ModuleDataRoot(module), Khet70Paths.DataRoot)
        {
        }

        private Khet70Store(string root, string legacyRoot)
        {
            dataRoot = string.IsNullOrWhiteSpace(root) ? Khet70Paths.DataRoot : root;
            legacyDataRoot = legacyRoot ?? "";
        }

        public Khet70StoreData Load()
        {
            var path = Path.Combine(dataRoot, "saved_templates.json");
            if (!File.Exists(path) && !string.IsNullOrWhiteSpace(legacyDataRoot))
            {
                var legacyPath = Path.Combine(legacyDataRoot, "saved_templates.json");
                if (File.Exists(legacyPath)) path = legacyPath;
            }
            if (!File.Exists(path)) return new Khet70StoreData();

            try
            {
                var json = File.ReadAllText(path, Encoding.UTF8);
                var data = serializer.Deserialize<Khet70StoreData>(json);
                if (data == null) throw new InvalidDataException("The saved template file is empty.");
                if (data.SchemaVersion > Khet70Paths.CurrentSchemaVersion)
                {
                    throw new InvalidDataException("The saved template file uses a newer schema.");
                }
                Normalize(data);
                return data;
            }
            catch (Exception ex)
            {
                throw new Khet70StoreException("ไม่สามารถอ่านข้อมูล Template ของเอกสาร Khet70 ได้ ไฟล์เดิมยังคงอยู่", ex);
            }
        }

        public void Save(Khet70StoreData data)
        {
            if (data == null) throw new ArgumentNullException("data");
            Normalize(data);
            var directory = dataRoot;
            Directory.CreateDirectory(directory);
            var path = Path.Combine(dataRoot, "saved_templates.json");
            var tempPath = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            var backupPath = path + ".bak";

            try
            {
                var json = serializer.Serialize(data);
                File.WriteAllText(tempPath, json, new UTF8Encoding(false));
                if (File.Exists(path))
                {
                    File.Replace(tempPath, path, backupPath, true);
                }
                else
                {
                    File.Move(tempPath, path);
                }
            }
            catch (Exception ex)
            {
                try
                {
                    if (File.Exists(tempPath)) File.Delete(tempPath);
                }
                catch
                {
                }
                throw new Khet70StoreException("ไม่สามารถบันทึกข้อมูล Template ของเอกสาร Khet70 ได้ ข้อมูลในฟอร์มยังคงอยู่", ex);
            }
        }

        public T Clone<T>(T value)
        {
            if (value == null) return default(T);
            return serializer.Deserialize<T>(serializer.Serialize(value));
        }

        private void Normalize(Khet70StoreData data)
        {
            if (data.SchemaVersion <= 0) data.SchemaVersion = Khet70Paths.CurrentSchemaVersion;
            if (data.Templates == null) data.Templates = new List<SavedTemplateSnapshot>();
            if (data.CustomDistricts == null) data.CustomDistricts = new List<string>();
            if (data.CustomPrefixes == null) data.CustomPrefixes = new List<string>();
            if (data.CustomQualifications == null) data.CustomQualifications = new List<string>();
            foreach (var template in data.Templates)
            {
                if (template == null) continue;
                if (string.IsNullOrWhiteSpace(template.Id)) template.Id = Guid.NewGuid().ToString("N");
                if (template.Record == null) template.Record = WorkingRecord.CreateEmpty();
                Normalize(template.Record);
            }
        }

        private void Normalize(WorkingRecord record)
        {
            if (record.School == null) record.School = new SchoolRecord();
            if (record.Employee == null) record.Employee = new EmployeeRecord();
            if (record.Committee == null || record.Committee.Length != 3)
            {
                record.Committee = new[] { new CommitteeMemberRecord(), new CommitteeMemberRecord(), new CommitteeMemberRecord() };
            }
            for (var i = 0; i < record.Committee.Length; i++)
            {
                if (record.Committee[i] == null) record.Committee[i] = new CommitteeMemberRecord();
            }
            if (record.Payroll == null) record.Payroll = new PayrollState();
            if (record.Procurement == null) record.Procurement = new ProcurementState();
            if (record.Payroll.HeadFinance == null) record.Payroll.HeadFinance = new PersonRecord();
            if (record.Payroll.SelectedDocuments == null) record.Payroll.SelectedDocuments = new List<string>();
            if (record.Procurement.SelectedDocuments == null) record.Procurement.SelectedDocuments = new List<string>();
            if (string.IsNullOrWhiteSpace(record.RecordId)) record.RecordId = Guid.NewGuid().ToString("N");
        }
    }
}

