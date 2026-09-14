using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Khet70
{
    internal enum TemplateOperationResult
    {
        Saved,
        Declined,
        Cancelled,
        Failed
    }

    internal sealed class ProcurementModule : UserControl
    {
        private const string DefaultDistrict = "";
        private const string DistrictPrefix = "สำนักงานเขตพื้นที่การศึกษา";
        private const string EmptyValue = ".............";
        private const string UnspecifiedOption = "-- ไม่ระบุ --";
        private const string PositionPlaceholder = "-- เลือกตำแหน่ง --";
        private const string CommitteeLevelPlaceholder = "-- เลือกระดับ --";
        private const string CommitteeNoLevelOption = "-- ไม่ใช้ระดับ --";
        private const string CommitteeCustomPositionOption = "......(กรอกตำแหน่งเอง)";
        private static readonly string[] CommitteeLevelledPositions =
        {
            "เจ้าพนักงานธุรการ", "นักจัดการงานทั่วไป", "นักวิชาการตรวจสอบภายใน", "นักวิชาการพัสดุ",
            "นักวิชาการเงินและบัญชี", "นักวิชาการศึกษา", "นักประชาสัมพันธ์", "นักวิเคราะห์นโยบายและแผน",
            "นักทรัพยากรบุคคล", "นักวิชาการคอมพิวเตอร์", "ศึกษานิเทศก์", "นิติกร"
        };
        private static readonly string[] CommitteeDirectorPositions =
        {
            "ผู้อำนวยการกลุ่มอำนวยการ", "ผู้อำนวยการกลุ่มบริหารงานการเงินและสินทรัพย์", "ผู้อำนวยการกลุ่มบริหารงานบุคคล",
            "ผู้อำนวยการกลุ่มวิเคราะห์นโยบายและแผน", "ผู้อำนวยการกลุ่มนิเทศ ติดตาม และประเมินผลการจัดการศึกษา",
            "ผู้อำนวยการกลุ่มพัฒนาครูและบุคลากรทางการศึกษา", "ผู้อำนวยการกลุ่มกฎหมายและคดี",
            "ผู้อำนวยการกลุ่มส่งเสริมการจัดการศึกษา", "ผู้อำนวยการหน่วยตรวจสอบภายใน",
            "ผู้อำนวยการกลุ่มส่งเสริมการศึกษาทางไกล เทคโนโลยีสารสนเทศและการสื่อสาร"
        };
        private static readonly string[] CommitteeGeneralLevels =
        {
            "ปฏิบัติงาน", "ชำนาญงาน", "ปฏิบัติการ", "ชำนาญการ", "ชำนาญการพิเศษ", "เชี่ยวชาญ"
        };
        private static readonly string[] CommitteeElectricianLevels = { "ช.1", "ช.2", "ช.3", "ช.4" };
        private static readonly string[] DistrictOptions = { "สำนักงานเขตพื้นที่การศึกษาประถมศึกษาแม่ฮ่องสอน เขต 2", "สำนักงานเขตพื้นที่การศึกษาประถมศึกษาพิษณุโลก เขต 2", "สำนักงานเขตพื้นที่การศึกษาประถมศึกษาประจวบคีรีขันธ์ เขต 2", "สำนักงานเขตพื้นที่การศึกษามัธยมศึกษาลำปางลำพูน", "สำนักงานเขตพื้นที่การศึกษามัธยมศึกษากาญจนบุรี" };
        private static readonly Color ModuleAccent = Color.FromArgb(15, 118, 110);
        private static readonly Color ModuleAccentDark = Color.FromArgb(20, 83, 45);
        private static readonly Color ModuleAccentLight = Color.FromArgb(234, 245, 238);
        private static readonly Color ModuleBackground = Color.FromArgb(243, 246, 248);
        private static readonly Color ModuleBorder = Color.FromArgb(214, 224, 232);
        private static readonly Color TextColor = Color.FromArgb(30, 41, 59);
        private static readonly Color MutedText = Color.FromArgb(100, 116, 139);
        private static readonly Color Navy = Color.FromArgb(24, 56, 82);
        private static readonly Color PrimaryDark = Color.FromArgb(22, 72, 108);
        private static readonly string[] ThaiMonths = { "", "มกราคม", "กุมภาพันธ์", "มีนาคม", "เมษายน", "พฤษภาคม", "มิถุนายน", "กรกฎาคม", "สิงหาคม", "กันยายน", "ตุลาคม", "พฤศจิกายน", "ธันวาคม" };

        private readonly WorkingRecord record;
        private readonly Action sharedChanged;
        private readonly Khet70Store store = new Khet70Store(DocumentModule.Procurement);
        private readonly ErrorProvider errorProvider = new ErrorProvider();
        private readonly Dictionary<string, Control> fields = new Dictionary<string, Control>();
        private readonly Dictionary<string, ComboBox> dateCombos = new Dictionary<string, ComboBox>();
        private readonly Dictionary<string, TextBox> dateCustomBoxes = new Dictionary<string, TextBox>();
        private readonly Dictionary<string, ComboBox> committeePositionBoxes = new Dictionary<string, ComboBox>();
        private readonly Dictionary<string, ComboBox> committeeLevelBoxes = new Dictionary<string, ComboBox>();
        private readonly Dictionary<string, Label> committeeCustomPositionLabels = new Dictionary<string, Label>();
        private readonly Dictionary<string, TextBox> committeeCustomPositionBoxes = new Dictionary<string, TextBox>();
        private readonly List<Panel> sectionPanels = new List<Panel>();
        private readonly List<CheckBox> documentChecks = new List<CheckBox>();
        private readonly List<ProcurementPositionConfig> positions = new List<ProcurementPositionConfig>();
        private readonly List<SalaryRateOption> salaryRates = new List<SalaryRateOption>();
        private readonly ProcurementManifest manifest;
        private Khet70StoreData storeData;
        private ComboBox districtBox;
        private ComboBox positionBox;
        private ComboBox salaryBox;
        private Label salaryLabel;
        private Label totalSalaryLabel;
        private Panel documentPanel;
        private ListBox sectionList;
        private Panel contentHost;
        private bool suppressChanges;
        private bool isDirty;

        public bool IsDirty { get { return isDirty; } }
        public WorkingRecord Record { get { return record; } }

        public ProcurementModule(WorkingRecord sharedRecord, Action sharedChangedCallback)
        {
            record = sharedRecord ?? WorkingRecord.CreateEmpty();
            sharedChanged = sharedChangedCallback;
            Dock = DockStyle.Fill;
            BackColor = ModuleBackground;
            Font = new Font("Segoe UI", 10.0f);
            AutoScaleMode = AutoScaleMode.Dpi;
            manifest = LoadManifest();
            LoadCatalog();
            LoadSalaryRates();
            Khet70Paths.EnsureUserTemplateRoot(DocumentModule.Procurement);
            try
            {
                storeData = store.Load();
            }
            catch (Khet70StoreException ex)
            {
                storeData = new Khet70StoreData();
                MessageBox.Show(ex.Message, "ข้อมูล Template", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            BuildShell();
            SetDefaults();
            RefreshFromRecord();
        }

        public void RefreshFromSharedRecord()
        {
            RefreshFromRecord();
        }

        public void RefreshSharedFieldsFromRecord()
        {
            suppressChanges = true;
            try
            {
                Set("school.district", record.School.District);
                Set("school.districtShort", record.School.DistrictShort);
                SetPerson("school.director", record.School.Director);
                SetPerson("school.supply", record.School.SupplyOfficer);
                SetPerson("school.headSupply", record.School.HeadSupplyOfficer);
                SetPerson("school.financeDirector", record.School.FinanceDirector);
                SetPerson("school.deputyDistrict", record.School.DeputyDistrictDirector);
                Set("employee.prefix", record.Employee.Prefix);
                Set("employee.given", record.Employee.GivenName);
                Set("employee.surname", record.Employee.Surname);
                Set("employee.nationalId", record.Employee.NationalId);
                SetDate("employee.birth.day", "employee.birth.dayCustom", record.Employee.BirthDay);
                SetDate("employee.birth.month", "employee.birth.monthCustom", record.Employee.BirthMonth);
                Set("employee.birth.year", record.Employee.BirthYear);
                Set("employee.age", record.Employee.Age);
                Set("employee.nationality", record.Employee.Nationality);
                Set("employee.race", record.Employee.Race);
                Set("employee.religion", record.Employee.Religion);
                Set("employee.houseNumber", record.Employee.HouseNumber);
                Set("employee.road", record.Employee.Road);
                Set("employee.subdistrict", record.Employee.Subdistrict);
                Set("employee.district", record.Employee.District);
                Set("employee.province", record.Employee.Province);
                Set("employee.idIssueDistrict", record.Employee.IdIssueDistrict);
                Set("employee.idIssueProvince", record.Employee.IdIssueProvince);
                SetDate("employee.idIssue.day", "employee.idIssue.dayCustom", record.Employee.IdIssueDay);
                SetDate("employee.idIssue.month", "employee.idIssue.monthCustom", record.Employee.IdIssueMonth);
                Set("employee.idIssue.year", record.Employee.IdIssueYear);
                SetDate("employee.idExpiry.day", "employee.idExpiry.dayCustom", record.Employee.IdExpiryDay);
                SetDate("employee.idExpiry.month", "employee.idExpiry.monthCustom", record.Employee.IdExpiryMonth);
                Set("employee.idExpiry.year", record.Employee.IdExpiryYear);
                Set("employee.educationLevel", record.Employee.EducationLevel);
                Set("employee.qualification", record.Employee.Qualification);
                for (var i = 0; i < 3; i++) SetPerson("committee." + i, record.Committee[i]);
            }
            finally
            {
                suppressChanges = false;
            }
        }

        public void MarkSharedRecordClean()
        {
            isDirty = false;
        }

        public TemplateOperationResult SaveCurrentTemplate()
        {
            CaptureFromControls();
            using (var editor = new ProcurementTemplateDialog("บันทึก Template", ""))
            {
                if (editor.ShowDialog(this) != DialogResult.OK) return TemplateOperationResult.Cancelled;
                var name = editor.TemplateName;
                var copy = store.Clone(storeData);
                if (copy.Templates == null) copy.Templates = new List<SavedTemplateSnapshot>();
                var existing = copy.Templates.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
                if (existing != null && MessageBox.Show("มีชื่อรายการนี้แล้ว ต้องการบันทึกทับหรือไม่?", "ชื่อซ้ำ", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return TemplateOperationResult.Cancelled;
                }

                var now = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
                var snapshot = existing ?? new SavedTemplateSnapshot();
                if (string.IsNullOrWhiteSpace(snapshot.CreatedAt)) snapshot.CreatedAt = now;
                snapshot.Name = name;
                snapshot.Note = editor.TemplateNote;
                snapshot.UpdatedAt = now;
                snapshot.Record = store.Clone(record);
                if (existing == null) copy.Templates.Add(snapshot);

                try
                {
                    store.Save(copy);
                    storeData = copy;
                    isDirty = false;
                    return TemplateOperationResult.Saved;
                }
                catch (Khet70StoreException ex)
                {
                    MessageBox.Show(ex.Message, "บันทึก Template ไม่สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return TemplateOperationResult.Failed;
                }
            }
        }

        private ProcurementManifest LoadManifest()
        {
            var path = Path.Combine(Khet70Paths.ShippedConfigRoot(DocumentModule.Procurement), "template_manifest.json");
            return Khet70Config.Load<ProcurementManifest>(path);
        }

        private Button CreateSecondaryButton(string text, Point location, Size size)
        {
            var button = new Button
            {
                Text = text,
                Location = location,
                Size = size,
                BackColor = Color.White,
                ForeColor = PrimaryDark,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            button.FlatAppearance.BorderColor = ModuleBorder;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(241, 247, 251);
            return button;
        }

        private Button CreatePrimaryButton(string text, Point location, Size size)
        {
            var button = new Button
            {
                Text = text,
                Location = location,
                Size = size,
                BackColor = ModuleAccent,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = ModuleAccentDark;
            return button;
        }

        private void LoadCatalog()
        {
            var path = Path.Combine(Khet70Paths.ShippedConfigRoot(DocumentModule.Procurement), "position_catalog.json");
            var catalog = Khet70Config.Load<ProcurementPositionCatalog>(path);
            if (catalog.positions == null) throw new InvalidDataException("Procurement position catalog ไม่มีรายการ positions");
            positions.AddRange(catalog.positions);
        }

        private void LoadSalaryRates()
        {
            foreach (var rate in Khet70SalaryTable.Load())
            {
                salaryRates.Add(new SalaryRateOption
                {
                    Salary = rate.salary,
                    SalaryText = rate.salaryText,
                    TotalSalary = rate.totalSalary,
                    TotalSalaryText = rate.totalSalaryText,
                    DutyTax = rate.dutyTax
                });
            }
        }

        private void BuildShell()
        {
            var header = new Panel { Dock = DockStyle.Top, Height = 76, BackColor = ModuleAccent, Padding = new Padding(24, 12, 24, 10) };
            var accentBar = new Panel { Dock = DockStyle.Left, Width = 5, BackColor = Color.FromArgb(185, 230, 204) };
            var title = new Label { Text = "Khet70  |  จัดซื้อจัดจ้างและสัญญา", Font = new Font("Segoe UI", 17f, FontStyle.Bold), ForeColor = Color.White, Dock = DockStyle.Left, Width = 520, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(12, 0, 0, 0) };
            var subtitle = new Label { Text = "สร้างเอกสารจากแม่แบบที่ตรวจสอบแล้ว — ทำงานแบบออฟไลน์และเก็บข้อมูลไว้ในเครื่อง", Font = new Font("Segoe UI", 9.5f), ForeColor = Color.FromArgb(240, 250, 244), TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill };
            header.Controls.Add(subtitle);
            header.Controls.Add(title);
            header.Controls.Add(accentBar);

            var footer = new Panel { Dock = DockStyle.Bottom, Height = 68, BackColor = Color.White, Padding = new Padding(20, 12, 20, 12) };
            footer.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 1, BackColor = ModuleBorder });
            var clear = CreateSecondaryButton("ล้างข้อมูลหมวดนี้", new Point(20, 12), new Size(145, 42));
            clear.Dock = DockStyle.Left;
            clear.Click += delegate { ClearCurrentSection(); };
            footer.Controls.Add(clear);
            var generate = CreatePrimaryButton("ตรวจสอบและสร้างเอกสาร", new Point(0, 12), new Size(206, 42));
            generate.Dock = DockStyle.Right;
            generate.Click += delegate { GenerateDocuments(); };
            footer.Controls.Add(generate);
            var newEmployee = CreateSecondaryButton("เพิ่มลูกจ้างใหม่", new Point(0, 12), new Size(145, 42));
            newEmployee.Dock = DockStyle.Right;
            newEmployee.Click += delegate { AddNewEmployee(); };
            footer.Controls.Add(newEmployee);
            var load = CreateSecondaryButton("โหลด Template", new Point(0, 12), new Size(125, 42));
            load.Dock = DockStyle.Right;
            load.Click += delegate { LoadTemplateFlow(); };
            footer.Controls.Add(load);
            var save = CreateSecondaryButton("บันทึก Template", new Point(0, 12), new Size(135, 42));
            save.Dock = DockStyle.Right;
            save.Click += delegate { SaveCurrentTemplate(); };
            footer.Controls.Add(save);
            var deleteTemplate = CreateSecondaryButton("ลบ Template", new Point(0, 12), new Size(115, 42));
            deleteTemplate.Dock = DockStyle.Right;
            deleteTemplate.Click += delegate { DeleteTemplateFlow(); };
            footer.Controls.Add(deleteTemplate);

            var body = new Panel { Dock = DockStyle.Fill, BackColor = ModuleBackground };
            var navigation = new Panel { Dock = DockStyle.Left, Width = 350, BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Padding = new Padding(20, 16, 16, 16) };
            var navigationTitle = new Label { Text = "ขั้นตอนการกรอกข้อมูล", Dock = DockStyle.Top, Height = 30, Font = new Font("Segoe UI", 10.5f, FontStyle.Bold), ForeColor = Navy };
            var navigationHint = new Label { Text = "เลือกหมวดเพื่อแก้ไขข้อมูล", Dock = DockStyle.Top, Height = 30, ForeColor = MutedText, Font = new Font("Segoe UI", 9.5f) };
            sectionList = new ListBox { Dock = DockStyle.Fill, BorderStyle = BorderStyle.None, BackColor = Color.White, Font = new Font("Segoe UI", 10.0f), IntegralHeight = false, DrawMode = DrawMode.OwnerDrawFixed, ItemHeight = 42 };
            sectionList.Items.AddRange(new object[] { "1  ข้อมูลเขตพื้นที่และผู้ลงนาม", "2  กรรมการ TOR", "3  ลูกจ้าง / ตำแหน่ง / เงินเดือน", "4  สรุปชุดเอกสารที่ต้องการสร้าง" });
            sectionList.SelectedIndexChanged += delegate { ShowSection(sectionList.SelectedIndex); };
            sectionList.DrawItem += DrawSectionItem;
            var folderActions = new Panel { Dock = DockStyle.Bottom, Height = 144, BackColor = Color.White };
            var openTemplates = CreateSecondaryButton("เปิดโฟลเดอร์ TEMPLATES เอกสาร", new Point(0, 0), new Size(300, 42));
            openTemplates.Dock = DockStyle.Top;
            openTemplates.Click += delegate { OpenTemplateFolder(); };
            folderActions.Controls.Add(openTemplates);
            var restoreTemplates = CreateSecondaryButton("คืนค่า TEMPLATES เริ่มต้น", new Point(0, 0), new Size(300, 42));
            restoreTemplates.Dock = DockStyle.Top;
            restoreTemplates.Click += delegate { RestoreTemplateFolder(); };
            folderActions.Controls.Add(restoreTemplates);
            var openOutput = CreateSecondaryButton("เปิดโฟลเดอร์เอกสารที่สร้างแล้ว", new Point(0, 0), new Size(300, 42));
            openOutput.Dock = DockStyle.Bottom;
            openOutput.Click += delegate { OpenOutputFolder(); };
            folderActions.Controls.Add(openOutput);
            navigation.Controls.Add(sectionList);
            navigation.Controls.Add(folderActions);
            navigation.Controls.Add(navigationHint);
            navigation.Controls.Add(navigationTitle);

            contentHost = new Panel { Dock = DockStyle.Fill, AutoScroll = false, BackColor = ModuleBackground, Padding = new Padding(16, 16, 20, 14) };
            for (var i = 0; i < 4; i++)
            {
                var panel = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Visible = false };
                contentHost.Controls.Add(panel);
                sectionPanels.Add(panel);
            }
            body.Controls.Add(contentHost);
            body.Controls.Add(navigation);

            // Dock order is intentional: the last control is laid out first in WinForms.
            // Put the fill body first so it cannot cover the fixed header or action bar.
            Controls.Add(body);
            Controls.Add(footer);
            Controls.Add(header);
            BuildSchoolSection(sectionPanels[0]);
            BuildCommitteeSection(sectionPanels[1]);
            BuildEmployeeSection(sectionPanels[2]);
            BuildSummarySection(sectionPanels[3]);
            sectionList.SelectedIndex = 0;
        }

        private void DrawSectionItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= sectionList.Items.Count) return;
            var selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            var background = selected ? ModuleAccentLight : Color.White;
            var foreground = selected ? ModuleAccentDark : TextColor;
            var bounds = new Rectangle(e.Bounds.Left + 10, e.Bounds.Top, e.Bounds.Width - 18, e.Bounds.Height);
            using (var backgroundBrush = new SolidBrush(background))
            {
                e.Graphics.FillRectangle(backgroundBrush, e.Bounds);
            }
            var hasStatus = e.Index < 3;
            var statusBounds = new Rectangle(e.Bounds.Right - 76, e.Bounds.Top, 68, e.Bounds.Height);
            using (var font = new Font(sectionList.Font, selected ? FontStyle.Bold : FontStyle.Regular))
            using (var brush = new SolidBrush(foreground))
            {
                var titleRight = hasStatus ? statusBounds.Left - 8 : e.Bounds.Right - 8;
                e.Graphics.DrawString(sectionList.Items[e.Index].ToString(), font, brush, new Rectangle(bounds.Left, bounds.Top, Math.Max(1, titleRight - bounds.Left), bounds.Height), new StringFormat { LineAlignment = StringAlignment.Center });
            }
            if (hasStatus)
            {
                var complete = IsSectionComplete(e.Index);
                var statusText = complete ? "ครบ" : "ไม่ครบ";
                var statusColor = complete ? Color.FromArgb(22, 163, 74) : Color.FromArgb(220, 38, 38);
                using (var statusBrush = new SolidBrush(statusColor))
                using (var statusFont = new Font("Segoe UI", 8.5f, FontStyle.Bold))
                {
                    var dot = new Rectangle(statusBounds.Left + 2, statusBounds.Top + 16, 10, 10);
                    e.Graphics.FillEllipse(statusBrush, dot);
                    e.Graphics.DrawString(statusText, statusFont, statusBrush, new Rectangle(statusBounds.Left + 16, statusBounds.Top, statusBounds.Width - 16, statusBounds.Height), new StringFormat { LineAlignment = StringAlignment.Center });
                }
            }
            if (selected)
            {
                using (var accentBrush = new SolidBrush(ModuleAccent))
                {
                    e.Graphics.FillRectangle(accentBrush, new Rectangle(e.Bounds.Left, e.Bounds.Top, 4, e.Bounds.Height));
                }
            }
            e.DrawFocusRectangle();
        }

        private void BuildSchoolSection(Panel parent)
        {
            AddHeading(parent, "ข้อมูลสำนักงานเขตพื้นที่และผู้ลงนาม", "กรอกข้อมูลเขตพื้นที่และผู้ลงนามสำหรับเอกสารของสำนักงานเขตพื้นที่การศึกษา");
            districtBox = AddEditableCombo(parent, "เขตพื้นที่การศึกษา *", "school.district", 24, 150, 390);
            districtBox.Items.AddRange(DistrictOptions);
            districtBox.Text = DefaultDistrict;
            AddText(parent, "ชื่อเขตย่อ *", "school.districtShort", 434, 150, 270);
            AddPersonFields(parent, "ผู้อำนวยการสำนักงานเขตพื้นที่", "school.director", 224);
            AddPersonFields(parent, "เจ้าหน้าที่พัสดุ", "school.supply", 584);
            AddPersonFields(parent, "หัวหน้าเจ้าหน้าที่พัสดุ", "school.headSupply", 704);
            AddPersonFields(parent, "ผู้อำนวยการกลุ่มบริหารงานการเงินและสินทรัพย์", "school.financeDirector", 464);
            AddPersonFields(parent, "รองผู้อำนวยการสำนักงานเขตพื้นที่", "school.deputyDistrict", 344);
        }

        private void BuildCommitteeSection(Panel parent)
        {
            AddHeading(parent, "คณะกรรมการกำหนด TOR", "กรอกกรรมการ 1–3 ให้ครบ โดยกรรมการ 1 เป็นประธาน และเลือกตำแหน่งกับระดับให้ตรงกับคำสั่ง");
            AddCommitteeFields(parent, 0, 92, "กรรมการ 1 (ประธาน)");
            AddCommitteeFields(parent, 1, 322, "กรรมการ 2");
            AddCommitteeFields(parent, 2, 552, "กรรมการ 3");
            AddText(parent, "เลขที่คำสั่งแต่งตั้ง TORและตรวจรับ", "procurement.specificationOrder", 24, 798, 220);
            parent.Controls.Add(new Label { Text = "(เช่น 1/2570)", Location = new Point(256, 822), Size = new Size(130, 26), ForeColor = Color.FromArgb(80, 95, 115), TextAlign = ContentAlignment.MiddleLeft });
        }

        private void BuildEmployeeSection(Panel parent)
        {
            AddHeading(parent, "ข้อมูลลูกจ้าง · ตำแหน่ง · เงินเดือน", "ช่องที่มี * จำเป็นสำหรับการสร้างเอกสาร | ช่องที่ไม่ได้กรอกจะแทนที่ด้วย ..............");
            AddZoneHeading(parent, "ข้อมูลส่วนตัว", 92);
            AddEditableCombo(parent, "คำนำหน้า", "employee.prefix", 24, 126, 150).Items.AddRange(new object[] { UnspecifiedOption, "นาย", "นาง", "นางสาว", "ว่าที่ร้อยตรี", "......(กรอกเอง)" });
            AddText(parent, "ชื่อ *", "employee.given", 194, 126, 250);
            AddText(parent, "นามสกุล *", "employee.surname", 464, 126, 250);
            AddText(parent, "เลขประจำตัวประชาชน *", "employee.nationalId", 24, 206, 360);
            AddDateField(parent, "วันเกิด", "employee.birth.day", "employee.birth.dayCustom", 24, 286);
            AddDateField(parent, "เดือนเกิด", "employee.birth.month", "employee.birth.monthCustom", 244, 286);
            AddText(parent, "ปีเกิด", "employee.birth.year", 464, 286, 120);
            AddText(parent, "อายุ", "employee.age", 604, 286, 70);
            parent.Controls.Add(new Label { Text = "ปี", Location = new Point(680, 310), Size = new Size(24, 20), ForeColor = Color.FromArgb(31, 41, 55) });
            AddText(parent, "สัญชาติ", "employee.nationality", 24, 366, 210);
            AddText(parent, "เชื้อชาติ", "employee.race", 254, 366, 210);
            AddText(parent, "ศาสนา", "employee.religion", 484, 366, 230);

            AddZoneHeading(parent, "ตำแหน่งและอัตราเงินเดือน", 446);
            positionBox = AddCombo(parent, "ตำแหน่ง *", "procurement.position", 24, 480, 330);
            positionBox.Items.Add(new PositionView(null));
            foreach (var position in positions) positionBox.Items.Add(new PositionView(position));
            positionBox.SelectedIndexChanged += delegate { UpdateRouteControls(); OnUiChanged(); };
            salaryBox = AddSalaryCombo(parent, "เงินเดือน *", "procurement.salary", 374, 480, 220);
            salaryLabel = AddReadOnlyValue(parent, "เงินเดือนTEXT", 24, 580, 220);
            totalSalaryLabel = AddReadOnlyValue(parent, "เงินรวม 12 เดือน / บาทอากร", 264, 580, 300);
            AddZoneHeading(parent, "การศึกษา", 672);
            AddEditableCombo(parent, "สำเร็จการศึกษาระดับ", "employee.educationLevel", 24, 706, 330).Items.AddRange(new object[] { UnspecifiedOption, "ประถมศึกษาตอนต้น", "ประถมศึกษาตอนปลาย", "มัธยมศึกษาตอนต้น", "มัธยมศึกษาตอนปลาย", "ปวช", "ปวส", "ปริญญาตรี", "ปริญญาโท", "ปริญญาเอก", "......(กรอกเอง)" });
            AddEditableCombo(parent, "คุณวุฒิการศึกษา", "employee.qualification", 374, 706, 340).Items.AddRange(new object[] { UnspecifiedOption, "ปวช.", "ปวส.", "ครุศาสตรบัณฑิต (ค.บ.)", "ศึกษาศาสตรบัณฑิต (ศษ.บ.)", "ศิลปศาสตรบัณฑิต (ศศ.บ.)", "วิทยาศาสตรบัณฑิต (วท.บ.)", "วิศวกรรมศาสตรบัณฑิต (วศ.บ.)", "บริหารธุรกิจบัณฑิต (บธ.บ.)", "นิติศาสตรบัณฑิต (น.บ.)", "รัฐศาสตรบัณฑิต (ร.บ.)", "รัฐประศาสนศาสตรบัณฑิต (รป.บ.)", "สาธารณสุขศาสตรบัณฑิต (ส.บ.)", "พยาบาลศาสตรบัณฑิต (พย.บ.)", "ปริญญาโท", "ปริญญาเอก", "อื่น ๆ (กรอกเอง)" });
            AddZoneHeading(parent, "ที่อยู่ / บัตรประชาชน", 818);
            AddText(parent, "บ้านเลขที่", "employee.houseNumber", 24, 852, 150);
            AddText(parent, "ถนน", "employee.road", 194, 852, 220);
            AddText(parent, "ตำบล", "employee.subdistrict", 434, 852, 170);
            AddText(parent, "อำเภอ", "employee.district", 24, 932, 200);
            AddText(parent, "จังหวัด", "employee.province", 244, 932, 200);
            AddZoneHeading(parent, "สถานที่ออกบัตร", 1012);
            AddText(parent, "ออกอำเภอ", "employee.idIssueDistrict", 24, 1050, 250);
            AddText(parent, "ออกจังหวัด", "employee.idIssueProvince", 294, 1050, 250);
            AddZoneHeading(parent, "วันออกบัตร", 1132);
            AddDateField(parent, "วัน", "employee.idIssue.day", "employee.idIssue.dayCustom", 24, 1170);
            AddDateField(parent, "เดือน", "employee.idIssue.month", "employee.idIssue.monthCustom", 244, 1170);
            AddText(parent, "ปี", "employee.idIssue.year", 464, 1170, 150);
            AddZoneHeading(parent, "วันหมดอายุบัตร", 1252);
            AddDateField(parent, "วัน", "employee.idExpiry.day", "employee.idExpiry.dayCustom", 24, 1290);
            AddDateField(parent, "เดือน", "employee.idExpiry.month", "employee.idExpiry.monthCustom", 244, 1290);
            AddText(parent, "ปี", "employee.idExpiry.year", 464, 1290, 150);
            parent.AutoScrollMinSize = new Size(0, 1432);
        }

        private void BuildSummarySection(Panel parent)
        {
            AddHeading(parent, "สรุปชุดเอกสารที่ต้องการสร้าง", "ตรวจรายการเอกสารที่ต้องการก่อนกดตรวจสอบและสร้างเอกสาร");
            documentPanel = new Panel { Location = new Point(24, 96), Size = new Size(690, 560), AutoScroll = true, BorderStyle = BorderStyle.FixedSingle, BackColor = Color.White };
            parent.Controls.Add(documentPanel);
            UpdateRouteControls();
        }

        private void OpenTemplateFolder()
        {
            var templateRoot = Khet70Paths.EnsureUserTemplateRoot(DocumentModule.Procurement);
            Process.Start(new ProcessStartInfo
            {
                FileName = templateRoot,
                UseShellExecute = true
            });
        }

        private void RestoreTemplateFolder()
        {
            if (MessageBox.Show("คืนค่าโฟลเดอร์ TEMPLATES เป็นชุดเริ่มต้นหรือไม่?\n\nไฟล์ที่แก้ไขอยู่จะถูกสำรองไว้ก่อน แล้วแทนที่ด้วยชุดเริ่มต้นของโปรแกรม", "คืนค่า TEMPLATES", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            {
                return;
            }
            try
            {
                var backup = Khet70Paths.RestoreUserTemplateRoot(DocumentModule.Procurement);
                var message = "คืนค่า TEMPLATES เรียบร้อยแล้ว";
                if (!string.IsNullOrWhiteSpace(backup)) message += "\n\nสำรองไฟล์เดิมไว้ที่:\n" + backup;
                MessageBox.Show(message, "คืนค่า TEMPLATES สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("คืนค่า TEMPLATES ไม่สำเร็จ: " + ex.Message, "คืนค่า TEMPLATES", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenOutputFolder()
        {
            var outputRoot = Khet70Paths.OutputRoot(DocumentModule.Procurement);
            Directory.CreateDirectory(outputRoot);
            Process.Start(new ProcessStartInfo
            {
                FileName = outputRoot,
                UseShellExecute = true
            });
        }

        private void AddZoneHeading(Control parent, string title, int y)
        {
            parent.Controls.Add(new Panel { Location = new Point(24, y), Size = new Size(680, 1), BackColor = ModuleBorder });
            parent.Controls.Add(new Label { Text = title, Font = new Font("Segoe UI", 10.5f, FontStyle.Bold), Location = new Point(24, y + 8), Size = new Size(680, 22), ForeColor = ModuleAccentDark });
        }

        private void AddHeading(Panel parent, string title, string hint)
        {
            parent.Controls.Add(new Label { Text = title, Font = new Font("Segoe UI", 15f, FontStyle.Bold), Location = new Point(24, 14), Size = new Size(680, 30), ForeColor = TextColor });
            var hintBox = new Panel { Location = new Point(24, 44), Size = new Size(680, 28), BackColor = ModuleAccentLight, BorderStyle = BorderStyle.FixedSingle };
            hintBox.Controls.Add(new Label { Text = hint, Font = new Font("Segoe UI", 8.5f), Location = new Point(9, 3), Size = new Size(658, 20), ForeColor = ModuleAccentDark, AutoEllipsis = true, TextAlign = ContentAlignment.MiddleLeft });
            parent.Controls.Add(hintBox);
            parent.Controls.Add(new Panel { Location = new Point(24, 76), Size = new Size(680, 2), BackColor = ModuleAccent });
        }

        private TextBox AddText(Control parent, string label, string key, int x, int y, int width)
        {
            parent.Controls.Add(new Label { Text = label, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), Location = new Point(x, y), Size = new Size(width, 20), ForeColor = TextColor });
            var box = new TextBox { Location = new Point(x, y + 24), Size = new Size(width, 26), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.White };
            box.TextChanged += delegate { OnUiChanged(); };
            parent.Controls.Add(box);
            fields[key] = box;
            return box;
        }

        private Label AddReadOnlyValue(Control parent, string label, int x, int y, int width)
        {
            parent.Controls.Add(new Label { Text = label, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), Location = new Point(x, y), Size = new Size(width, 20), ForeColor = TextColor });
            var value = new Label { Location = new Point(x, y + 24), Size = new Size(width, 26), BorderStyle = BorderStyle.FixedSingle, BackColor = ModuleAccentLight, ForeColor = ModuleAccentDark, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(6, 0, 0, 0) };
            parent.Controls.Add(value);
            return value;
        }

        private ComboBox AddCombo(Control parent, string label, string key, int x, int y, int width)
        {
            parent.Controls.Add(new Label { Text = label, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), Location = new Point(x, y), Size = new Size(width, 20), ForeColor = TextColor });
            var box = new ComboBox { Location = new Point(x, y + 24), Size = new Size(width, 26), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.White };
            box.SelectedIndexChanged += delegate { OnUiChanged(); };
            parent.Controls.Add(box);
            fields[key] = box;
            return box;
        }

        private ComboBox AddSalaryCombo(Control parent, string label, string key, int x, int y, int width)
        {
            var box = AddCombo(parent, label, key, x, y, width);
            foreach (var rate in salaryRates) box.Items.Add(rate);
            box.SelectedIndexChanged += delegate { UpdateSalaryPreview(); OnUiChanged(); };
            return box;
        }

        private ComboBox AddEditableCombo(Control parent, string label, string key, int x, int y, int width)
        {
            var box = AddCombo(parent, label, key, x, y, width);
            box.DropDownStyle = ComboBoxStyle.DropDown;
            box.TextChanged += delegate { OnUiChanged(); };
            return box;
        }

        private void AddPersonFields(Control parent, string title, string prefix, int y)
        {
            parent.Controls.Add(new Panel { Location = new Point(24, y - 10), Size = new Size(680, 1), BackColor = ModuleBorder });
            parent.Controls.Add(new Label { Text = title, Font = new Font("Segoe UI", 10f, FontStyle.Bold), Location = new Point(24, y), Size = new Size(680, 20), ForeColor = ModuleAccentDark });
            var prefixBox = AddEditableCombo(parent, "คำนำหน้า", prefix + ".prefix", 24, y + 24, 150);
            prefixBox.Items.AddRange(new object[] { UnspecifiedOption, "นาย", "นาง", "นางสาว", "ว่าที่ร้อยตรี", "......(กรอกเอง)" });
            AddText(parent, "ชื่อ", prefix + ".given", 194, y + 24, 250);
            AddText(parent, "นามสกุล", prefix + ".surname", 464, y + 24, 250);
        }

        private void AddCommitteeFields(Control parent, int index, int y, string title)
        {
            var prefix = "committee." + index;
            var group = new Panel { Location = new Point(16, y - 8), Size = new Size(700, 220), BackColor = Color.FromArgb(250, 253, 251), BorderStyle = BorderStyle.FixedSingle, TabStop = false };
            parent.Controls.Add(group);
            group.Controls.Add(new Panel { Location = new Point(0, 0), Size = new Size(5, 36), BackColor = ModuleAccent });
            group.Controls.Add(new Label { Text = title, Font = new Font("Segoe UI", 10f, FontStyle.Bold), Location = new Point(5, 0), Size = new Size(689, 36), BackColor = Color.FromArgb(248, 250, 252), ForeColor = TextColor, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(12, 0, 0, 0) });
            var prefixBox = AddEditableCombo(group, "คำนำหน้า", prefix + ".prefix", 12, 42, 150);
            prefixBox.Items.AddRange(new object[] { UnspecifiedOption, "นาย", "นาง", "นางสาว", "ว่าที่ร้อยตรี", "......(กรอกเอง)" });
            AddText(group, "ชื่อ", prefix + ".given", 182, 42, 220);
            AddText(group, "นามสกุล", prefix + ".surname", 424, 42, 180);

            var position = AddCombo(group, "ตำแหน่ง", prefix + ".positionType", 12, 100, 330);
            var level = AddCombo(group, "ระดับ", prefix + ".positionLevel", 354, 100, 250);
            committeePositionBoxes[prefix] = position;
            committeeLevelBoxes[prefix] = level;
            position.Items.Add(PositionPlaceholder);
            position.Items.AddRange(CommitteeLevelledPositions);
            position.Items.Add("นายช่างไฟฟ้า");
            position.Items.AddRange(CommitteeDirectorPositions);
            position.Items.Add(CommitteeCustomPositionOption);

            var customLabel = new Label { Text = "ตำแหน่งเต็ม (กรอกเอง)", Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), Location = new Point(12, 158), Size = new Size(580, 20), ForeColor = TextColor, Visible = false };
            var customBox = new TextBox { Location = new Point(12, 182), Size = new Size(592, 26), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.White, Visible = false };
            customBox.TextChanged += delegate { OnUiChanged(); };
            group.Controls.Add(customLabel);
            group.Controls.Add(customBox);
            fields[prefix + ".positionCustom"] = customBox;
            committeeCustomPositionLabels[prefix] = customLabel;
            committeeCustomPositionBoxes[prefix] = customBox;
            position.SelectedIndexChanged += delegate { ConfigureCommitteePosition(prefix); OnUiChanged(); };
            ConfigureCommitteePosition(prefix);
        }

        private void ConfigureCommitteePosition(string prefix)
        {
            ComboBox position;
            ComboBox level;
            if (!committeePositionBoxes.TryGetValue(prefix, out position) || !committeeLevelBoxes.TryGetValue(prefix, out level)) return;

            var selected = position.SelectedItem == null ? "" : position.SelectedItem.ToString();
            level.Items.Clear();
            if (CommitteeLevelledPositions.Contains(selected))
            {
                level.Items.Add(CommitteeLevelPlaceholder);
                level.Items.AddRange(CommitteeGeneralLevels);
                level.Enabled = true;
            }
            else if (selected == "นายช่างไฟฟ้า")
            {
                level.Items.Add(CommitteeLevelPlaceholder);
                level.Items.AddRange(CommitteeElectricianLevels);
                level.Enabled = true;
            }
            else
            {
                level.Items.Add(CommitteeNoLevelOption);
                level.Enabled = false;
            }
            level.SelectedIndex = 0;

            Label customLabel;
            TextBox customBox;
            var isCustom = selected == CommitteeCustomPositionOption;
            if (committeeCustomPositionLabels.TryGetValue(prefix, out customLabel)) customLabel.Visible = isCustom;
            if (committeeCustomPositionBoxes.TryGetValue(prefix, out customBox))
            {
                customBox.Visible = isCustom;
                if (!isCustom) customBox.Text = "";
            }
        }

        private string ReadCommitteePosition(string prefix)
        {
            ComboBox position;
            if (!committeePositionBoxes.TryGetValue(prefix, out position)) return "";
            var selected = position.SelectedItem == null ? "" : position.SelectedItem.ToString();
            if (selected == CommitteeCustomPositionOption)
            {
                TextBox customBox;
                return committeeCustomPositionBoxes.TryGetValue(prefix, out customBox) ? CleanInput(customBox.Text) : "";
            }
            if (CommitteeLevelledPositions.Contains(selected) || selected == "นายช่างไฟฟ้า")
            {
                ComboBox level;
                if (!committeeLevelBoxes.TryGetValue(prefix, out level)) return "";
                var selectedLevel = level.SelectedItem == null ? "" : level.SelectedItem.ToString();
                if (string.IsNullOrWhiteSpace(selectedLevel) || selectedLevel == CommitteeLevelPlaceholder) return "";
                return selected + selectedLevel;
            }
            if (CommitteeDirectorPositions.Contains(selected)) return selected;
            return "";
        }

        private void SetCommitteePosition(string prefix, string value)
        {
            ComboBox position;
            if (!committeePositionBoxes.TryGetValue(prefix, out position)) return;
            var cleaned = (value ?? "").Trim();
            if (string.IsNullOrWhiteSpace(cleaned))
            {
                position.SelectedIndex = 0;
                ConfigureCommitteePosition(prefix);
                return;
            }

            foreach (var candidate in CommitteeLevelledPositions)
            {
                foreach (var level in CommitteeGeneralLevels)
                {
                    if (!string.Equals(cleaned, candidate + level, StringComparison.Ordinal)) continue;
                    position.SelectedItem = candidate;
                    ConfigureCommitteePosition(prefix);
                    committeeLevelBoxes[prefix].SelectedItem = level;
                    return;
                }
            }
            foreach (var level in CommitteeElectricianLevels)
            {
                if (!string.Equals(cleaned, "นายช่างไฟฟ้า" + level, StringComparison.Ordinal)) continue;
                position.SelectedItem = "นายช่างไฟฟ้า";
                ConfigureCommitteePosition(prefix);
                committeeLevelBoxes[prefix].SelectedItem = level;
                return;
            }
            foreach (var candidate in CommitteeDirectorPositions)
            {
                if (!string.Equals(cleaned, candidate, StringComparison.Ordinal)) continue;
                position.SelectedItem = candidate;
                ConfigureCommitteePosition(prefix);
                return;
            }

            position.SelectedItem = CommitteeCustomPositionOption;
            ConfigureCommitteePosition(prefix);
            TextBox customBox;
            if (committeeCustomPositionBoxes.TryGetValue(prefix, out customBox)) customBox.Text = cleaned;
        }
        private void AddDateField(Control parent, string label, string key, string customKey, int x, int y)
        {
            parent.Controls.Add(new Label { Text = label, Location = new Point(x, y), Size = new Size(200, 20) });
            var box = new ComboBox { Location = new Point(x, y + 24), Size = new Size(108, 26), DropDownStyle = ComboBoxStyle.DropDown, BackColor = Color.White };
            box.Items.Add(UnspecifiedOption);
            if (key.EndsWith(".month", StringComparison.Ordinal))
            {
                for (var i = 1; i < ThaiMonths.Length; i++) box.Items.Add(ThaiMonths[i]);
            }
            else
            {
                for (var i = 1; i <= 31; i++) box.Items.Add(i.ToString());
            }
            box.Items.Add("......(กรอกเอง)");
            var custom = new TextBox { Location = new Point(x + 114, y + 24), Size = new Size(96, 26), Visible = false, BackColor = Color.White };
            box.SelectedIndexChanged += delegate { custom.Visible = box.Text.StartsWith("......", StringComparison.Ordinal); OnUiChanged(); };
            box.TextChanged += delegate { custom.Visible = box.Text.StartsWith("......", StringComparison.Ordinal); OnUiChanged(); };
            custom.TextChanged += delegate { OnUiChanged(); };
            parent.Controls.Add(box);
            parent.Controls.Add(custom);
            dateCombos[key] = box;
            dateCustomBoxes[customKey] = custom;
        }

        private void SetDefaults()
        {
            suppressChanges = true;
            try
            {
                record.School.District = DefaultDistrict;
                if (districtBox != null) districtBox.Text = DefaultDistrict;
                if (positionBox != null) positionBox.SelectedIndex = 0;
            }
            finally
            {
                suppressChanges = false;
            }
        }

        private void ShowSection(int index)
        {
            for (var i = 0; i < sectionPanels.Count; i++) sectionPanels[i].Visible = i == index;
        }

        private void OnUiChanged()
        {
            if (suppressChanges) return;
            CaptureFromControls();
            isDirty = true;
            if (sectionList != null) sectionList.Invalidate();
            if (sharedChanged != null) sharedChanged();
        }

        private bool IsSectionComplete(int index)
        {
            CaptureFromControls();
            if (index == 0)
            {
                return AllFieldsComplete(fields.Keys.Where(x => x.StartsWith("school.", StringComparison.Ordinal) && x != "school.name"));
            }
            if (index == 1)
            {
                return CommitteeFieldsComplete() && HasValue("procurement.specificationOrder");
            }
            if (index == 2)
            {
                var keys = fields.Keys
                    .Where(x => x.StartsWith("employee.", StringComparison.Ordinal))
                    .Concat(new[]
                    {
                        "employee.birth.day", "employee.birth.month",
                        "employee.idIssue.day", "employee.idIssue.month",
                        "employee.idExpiry.day", "employee.idExpiry.month",
                        "procurement.position", "procurement.salary"
                    });

                return AllFieldsComplete(keys);
            }
            return false;
        }

        private bool CommitteeFieldsComplete()
        {
            for (var i = 0; i < 3; i++)
            {
                var prefix = "committee." + i;
                if (!HasValue(prefix + ".prefix") || !HasValue(prefix + ".given") || !HasValue(prefix + ".surname")) return false;
                if (string.IsNullOrWhiteSpace(ReadCommitteePosition(prefix))) return false;
            }
            return true;
        }
        private bool AllFieldsComplete(IEnumerable<string> keys)
        {
            return keys.All(HasValue);
        }

        private bool HasValue(string key)
        {
            if (dateCombos.ContainsKey(key)) return !string.IsNullOrWhiteSpace(ReadDate(key, key + "Custom"));
            return !string.IsNullOrWhiteSpace(Read(key));
        }

        private void CaptureFromControls()
        {
            record.School.Name = "";
            record.School.District = Read("school.district");
            record.School.DistrictShort = Read("school.districtShort");
            record.School.Director = ReadPerson("school.director");
            record.School.SupplyOfficer = ReadPerson("school.supply");
            record.School.HeadSupplyOfficer = ReadPerson("school.headSupply");
            record.School.FinanceDirector = ReadPerson("school.financeDirector");
            record.School.DeputyDistrictDirector = ReadPerson("school.deputyDistrict");
            record.Employee.Prefix = Read("employee.prefix");
            record.Employee.GivenName = Read("employee.given");
            record.Employee.Surname = Read("employee.surname");
            record.Employee.NationalId = Read("employee.nationalId");
            record.Employee.BirthDay = ReadDate("employee.birth.day", "employee.birth.dayCustom");
            record.Employee.BirthMonth = ReadDate("employee.birth.month", "employee.birth.monthCustom");
            record.Employee.BirthYear = Read("employee.birth.year");
            record.Employee.Age = Read("employee.age");
            record.Employee.Nationality = Read("employee.nationality");
            record.Employee.Race = Read("employee.race");
            record.Employee.Religion = Read("employee.religion");
            record.Employee.HouseNumber = Read("employee.houseNumber");
            record.Employee.Road = Read("employee.road");
            record.Employee.Subdistrict = Read("employee.subdistrict");
            record.Employee.District = Read("employee.district");
            record.Employee.Province = Read("employee.province");
            record.Employee.IdIssueDistrict = Read("employee.idIssueDistrict");
            record.Employee.IdIssueProvince = Read("employee.idIssueProvince");
            record.Employee.IdIssueDay = ReadDate("employee.idIssue.day", "employee.idIssue.dayCustom");
            record.Employee.IdIssueMonth = ReadDate("employee.idIssue.month", "employee.idIssue.monthCustom");
            record.Employee.IdIssueYear = Read("employee.idIssue.year");
            record.Employee.IdExpiryDay = ReadDate("employee.idExpiry.day", "employee.idExpiry.dayCustom");
            record.Employee.IdExpiryMonth = ReadDate("employee.idExpiry.month", "employee.idExpiry.monthCustom");
            record.Employee.IdExpiryYear = Read("employee.idExpiry.year");
            record.Employee.EducationLevel = Read("employee.educationLevel");
            record.Employee.Qualification = Read("employee.qualification");
            for (var i = 0; i < 3; i++) record.Committee[i] = ReadCommitteePerson("committee." + i);
            var selectedPosition = positionBox == null ? null : positionBox.SelectedItem as PositionView;
            record.Procurement.PositionId = selectedPosition == null || selectedPosition.Config == null ? "" : selectedPosition.Config.id;
            record.Procurement.Salary = Read("procurement.salary");
            record.Procurement.HasTeaching = false;
            record.Procurement.WorksAtTwoSchools = false;
            record.Procurement.SecondSchoolName = "";
            record.Procurement.SpecificationOrder = Read("procurement.specificationOrder");
            record.Procurement.SelectedDocuments = documentChecks.Where(x => x.Checked).Select(x => (string)x.Tag).ToList();
        }

        private PersonRecord ReadPerson(string prefix)
        {
            return new PersonRecord { Prefix = Read(prefix + ".prefix"), GivenName = Read(prefix + ".given"), Surname = Read(prefix + ".surname"), Position = Read(prefix + ".position") };
        }

        private CommitteeMemberRecord ReadCommitteePerson(string prefix)
        {
            return new CommitteeMemberRecord
            {
                Prefix = Read(prefix + ".prefix"),
                GivenName = Read(prefix + ".given"),
                Surname = Read(prefix + ".surname"),
                Position = ReadCommitteePosition(prefix)
            };
        }

        private string Read(string key)
        {
            Control control;
            if (!fields.TryGetValue(key, out control)) return "";
            var combo = control as ComboBox;
            var value = combo == null ? control.Text : combo.Text;
            return CleanInput(value);
        }

        private string ReadDate(string key, string customKey)
        {
            ComboBox combo;
            if (!dateCombos.TryGetValue(key, out combo)) return "";
            var value = combo.Text.StartsWith("......", StringComparison.Ordinal) ? dateCustomBoxes[customKey].Text : combo.Text;
            return CleanInput(value);
        }

        private static string CleanInput(string value)
        {
            value = (value ?? "").Trim();
            if (value.StartsWith("......", StringComparison.Ordinal) || value == "--" || value == UnspecifiedOption || value == PositionPlaceholder) return "";
            return value;
        }

        private void RefreshFromRecord()
        {
            suppressChanges = true;
            try
            {
                Set("school.district", record.School.District);
                Set("school.districtShort", record.School.DistrictShort);
                SetPerson("school.director", record.School.Director);
                SetPerson("school.supply", record.School.SupplyOfficer);
                SetPerson("school.headSupply", record.School.HeadSupplyOfficer);
                SetPerson("school.financeDirector", record.School.FinanceDirector);
                SetPerson("school.deputyDistrict", record.School.DeputyDistrictDirector);
                Set("employee.prefix", record.Employee.Prefix);
                Set("employee.given", record.Employee.GivenName);
                Set("employee.surname", record.Employee.Surname);
                Set("employee.nationalId", record.Employee.NationalId);
                SetDate("employee.birth.day", "employee.birth.dayCustom", record.Employee.BirthDay);
                SetDate("employee.birth.month", "employee.birth.monthCustom", record.Employee.BirthMonth);
                Set("employee.birth.year", record.Employee.BirthYear);
                Set("employee.age", record.Employee.Age);
                Set("employee.nationality", record.Employee.Nationality);
                Set("employee.race", record.Employee.Race);
                Set("employee.religion", record.Employee.Religion);
                Set("employee.houseNumber", record.Employee.HouseNumber);
                Set("employee.road", record.Employee.Road);
                Set("employee.subdistrict", record.Employee.Subdistrict);
                Set("employee.district", record.Employee.District);
                Set("employee.province", record.Employee.Province);
                Set("employee.idIssueDistrict", record.Employee.IdIssueDistrict);
                Set("employee.idIssueProvince", record.Employee.IdIssueProvince);
                SetDate("employee.idIssue.day", "employee.idIssue.dayCustom", record.Employee.IdIssueDay);
                SetDate("employee.idIssue.month", "employee.idIssue.monthCustom", record.Employee.IdIssueMonth);
                Set("employee.idIssue.year", record.Employee.IdIssueYear);
                SetDate("employee.idExpiry.day", "employee.idExpiry.dayCustom", record.Employee.IdExpiryDay);
                SetDate("employee.idExpiry.month", "employee.idExpiry.monthCustom", record.Employee.IdExpiryMonth);
                Set("employee.idExpiry.year", record.Employee.IdExpiryYear);
                Set("employee.educationLevel", record.Employee.EducationLevel);
                Set("employee.qualification", record.Employee.Qualification);
                for (var i = 0; i < 3; i++) SetPerson("committee." + i, record.Committee[i]);
                SelectPosition(record.Procurement.PositionId);
                SelectSalary(record.Procurement.Salary);
                record.Procurement.HasTeaching = false;
                record.Procurement.WorksAtTwoSchools = false;
                record.Procurement.SecondSchoolName = "";
                Set("procurement.specificationOrder", record.Procurement.SpecificationOrder);
                UpdateRouteControls();
                isDirty = false;
            }
            finally
            {
                suppressChanges = false;
            }
            if (sectionList != null) sectionList.Invalidate();
        }

        private void SetPerson(string prefix, PersonRecord person)
        {
            person = person ?? new PersonRecord();
            Set(prefix + ".prefix", person.Prefix);
            Set(prefix + ".given", person.GivenName);
            Set(prefix + ".surname", person.Surname);
            if (committeePositionBoxes.ContainsKey(prefix)) SetCommitteePosition(prefix, person.Position);
            else Set(prefix + ".position", person.Position);
        }

        private void Set(string key, string value)
        {
            Control control;
            if (!fields.TryGetValue(key, out control)) return;
            var combo = control as ComboBox;
            if (combo != null && (string.IsNullOrWhiteSpace(value) || value.Trim() == "--") && combo.Items.Contains(UnspecifiedOption))
            {
                combo.SelectedIndex = 0;
                return;
            }
            control.Text = value ?? "";
        }

        private void SetDate(string key, string customKey, string value)
        {
            ComboBox combo;
            if (!dateCombos.TryGetValue(key, out combo)) return;
            var text = value ?? "";
            if (text.Length == 0) combo.SelectedIndex = 0;
            else if (combo.Items.Contains(text)) combo.Text = text;
            else
            {
                combo.Text = "......(กรอกเอง)";
                dateCustomBoxes[customKey].Text = text;
            }
            dateCustomBoxes[customKey].Visible = combo.Text.StartsWith("......", StringComparison.Ordinal);
        }

        private void SelectSalary(string value)
        {
            if (salaryBox == null) return;
            var cleaned = (value ?? "").Replace(",", "").Trim();
            for (var i = 0; i < salaryBox.Items.Count; i++)
            {
                var rate = salaryBox.Items[i] as SalaryRateOption;
                if (rate != null && (rate.Salary ?? "").Replace(",", "").Trim() == cleaned) { salaryBox.SelectedIndex = i; return; }
            }
            salaryBox.SelectedIndex = -1;
        }

        private void SelectPosition(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                positionBox.SelectedIndex = 0;
                return;
            }
            for (var i = 0; i < positionBox.Items.Count; i++)
            {
                var item = positionBox.Items[i] as PositionView;
                if (item != null && item.Config != null && item.Config.id == id) { positionBox.SelectedIndex = i; return; }
            }
            positionBox.SelectedIndex = -1;
        }

        private ProcurementPositionConfig CurrentPosition()
        {
            var item = positionBox == null ? null : positionBox.SelectedItem as PositionView;
            return item == null ? null : item.Config;
        }

        private ProcurementManifestRoute CurrentRoute()
        {
            var position = CurrentPosition();
            if (position == null || manifest.routes == null) return null;
            var route = manifest.routes.FirstOrDefault(x => x.positionId == position.id && !x.hasTeaching && !x.worksAtTwoSchools);
            return route;
        }

        private void UpdateSalaryPreview()
        {
            var rate = salaryBox == null ? null : salaryBox.SelectedItem as SalaryRateOption;
            if (salaryLabel != null) salaryLabel.Text = rate == null ? "" : rate.SalaryText;
            if (totalSalaryLabel != null) totalSalaryLabel.Text = rate == null ? "" : rate.TotalSalary + " / " + rate.DutyTax;
        }

        private void UpdateRouteControls()
        {
            if (positionBox == null) return;
            UpdateSalaryPreview();
            BuildDocumentChecklist();
        }

        private void BuildDocumentChecklist()
        {
            if (documentPanel == null) return;
            documentPanel.Controls.Clear();
            documentChecks.Clear();
            documentPanel.Controls.Add(new Label { Text = "สรุปชุดเอกสารที่ต้องการสร้าง (เริ่มต้นเลือกครบ 10 รายการ)", Location = new Point(14, 12), Size = new Size(640, 24), Font = new Font("Tahoma", 10.5f, FontStyle.Bold), ForeColor = ModuleAccentDark });
            var route = CurrentRoute();
            var paths = new List<string>();
            if (manifest.centralDocuments != null) paths.AddRange(manifest.centralDocuments.Select(x => x.relativePath));
            if (route != null) paths.Add(route.tor);
            if (route != null) paths.Add(route.quote);
            paths = paths.Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(GetDocumentSortKey)
                .ThenBy(x => x, StringComparer.OrdinalIgnoreCase)
                .ToList();
            var y = 46;
            foreach (var path in paths)
            {
                var check = new CheckBox { Text = Path.GetFileName(path), Tag = path, Checked = true, Location = new Point(14, y), Size = new Size(620, 24) };
                check.CheckedChanged += delegate { OnUiChanged(); };
                documentPanel.Controls.Add(check);
                documentChecks.Add(check);
                y += 28;
            }
            if (paths.Count == 0) documentPanel.Controls.Add(new Label { Text = "เลือกตำแหน่งเพื่อแสดงชุดเอกสาร", Location = new Point(14, y), Size = new Size(400, 24) });
        }

        private static string GetDocumentSortKey(string path)
        {
            var fileName = Path.GetFileName(path ?? "");
            var match = Regex.Match(fileName, "^\\s*(\\d+(?:\\.\\d+)*)");
            if (!match.Success) return "99999999";
            var parts = match.Groups[1].Value.Split('.');
            return string.Join(".", parts.Select(x => int.Parse(x).ToString("D8")).ToArray());
        }

        private List<string> ValidateRecord(WorkingRecord candidate)
        {
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(candidate.School.District)) errors.Add("ชื่อเขตพื้นที่การศึกษา");
            if (string.IsNullOrWhiteSpace(candidate.School.DistrictShort)) errors.Add("ชื่อเขตย่อ");
            if (!PersonComplete(candidate.School.FinanceDirector)) errors.Add("ผู้อำนวยการกลุ่มบริหารงานการเงินและสินทรัพย์");
            if (!PersonComplete(candidate.School.DeputyDistrictDirector)) errors.Add("รองผู้อำนวยการสำนักงานเขตพื้นที่");
            if (string.IsNullOrWhiteSpace(candidate.Employee.GivenName)) errors.Add("ชื่อลูกจ้าง");
            if (string.IsNullOrWhiteSpace(candidate.Employee.Surname)) errors.Add("นามสกุลลูกจ้าง");
            if (!Regex.IsMatch(candidate.Employee.NationalId ?? "", "^[0-9]{13}$")) errors.Add("เลขประจำตัวประชาชนต้องเป็นตัวเลข 13 หลัก");
            var position = positions.FirstOrDefault(x => x.id == candidate.Procurement.PositionId);
            if (position == null) errors.Add("ตำแหน่ง");
            if (FindSalaryRate(candidate.Procurement.Salary) == null) errors.Add("เงินเดือน");
            if (candidate.Procurement.SelectedDocuments == null || candidate.Procurement.SelectedDocuments.Count == 0) errors.Add("เลือกเอกสารอย่างน้อย 1 รายการ");
            for (var i = 0; i < candidate.Committee.Length; i++)
            {
                var member = candidate.Committee[i];
                if (member == null || string.IsNullOrWhiteSpace(member.Prefix) || string.IsNullOrWhiteSpace(member.GivenName) || string.IsNullOrWhiteSpace(member.Surname) || string.IsNullOrWhiteSpace(member.Position)) errors.Add("กรรมการ " + (i + 1));
            }
            return errors;
        }

        private static bool PersonComplete(PersonRecord person)
        {
            return person != null && !string.IsNullOrWhiteSpace(person.Prefix) && !string.IsNullOrWhiteSpace(person.GivenName) && !string.IsNullOrWhiteSpace(person.Surname);
        }

        private void GenerateDocuments()
        {
            CaptureFromControls();
            var candidate = store.Clone(record);
            NormalizeRecord(candidate);
            var errors = ValidateRecord(candidate);
            errorProvider.Clear();
            if (errors.Count > 0)
            {
                MessageBox.Show("กรุณาแก้ไข:\n- " + string.Join("\n- ", errors), "ข้อมูลยังไม่ครบ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var route = manifest.routes.FirstOrDefault(x => x.positionId == candidate.Procurement.PositionId && !x.hasTeaching && !x.worksAtTwoSchools);
            if (route == null)
            {
                MessageBox.Show("ไม่พบ route ที่ตรงกับตำแหน่งและตัวเลือกที่เลือก", "Routing", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var values = BuildValues(candidate);
            var templateRoot = Khet70Paths.EnsureUserTemplateRoot(DocumentModule.Procurement);
            var outputRoot = Khet70Paths.OutputRoot(DocumentModule.Procurement);
            var created = new List<string>();
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                foreach (var relative in candidate.Procurement.SelectedDocuments)
                {
                    var source = Khet70Paths.ResolveUnder(templateRoot, relative);
                    var requested = Path.Combine(outputRoot, SafeFilePart(candidate.Employee.GivenName) + "_" + SafeFilePart(Path.GetFileNameWithoutExtension(relative)) + ".docx");
                    created.Add(Khet70DocxRenderer.Render(source, requested, values));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("สร้างเอกสารไม่สำเร็จ: " + ex.Message, "สร้างเอกสาร", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
            var openOutput = MessageBox.Show(
                "สร้างเอกสารครบ " + created.Count + " ไฟล์แล้ว\n" + outputRoot + "\n\nกรุณาตรวจสอบข้อมูล และจัดหน้ากระดาษให้เรียบร้อยด้วยนะจ๊ะ\n\nต้องการเปิดโฟลเดอร์ output ตอนนี้หรือไม่?",
                "สำเร็จ",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Information);
            if (openOutput == DialogResult.Yes)
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = outputRoot,
                    UseShellExecute = true
                });
            }
        }

        private Dictionary<string, string> BuildValues(WorkingRecord source)
        {
            var position = positions.First(x => x.id == source.Procurement.PositionId);
            var values = new Dictionary<string, string>();
            Put(values, "{ชื่อโรงเรียน}", "");
            Put(values, "{ชื่อโรงเรียนสอง}", "");
            Put(values, "{ชื่อเขต}", NormalizeDistrictName(source.School.District));
            Put(values, "{ชื่อเขตย่อ}", source.School.DistrictShort);
            PutPerson(values, "ผอ", source.School.Director);
            PutPerson(values, "พัสดุ", source.School.SupplyOfficer);
            PutPerson(values, "หพัสดุ", source.School.HeadSupplyOfficer);
            PutPerson(values, "ผอกง", source.School.FinanceDirector);
            PutPerson(values, "รองเขต", source.School.DeputyDistrictDirector);
            Put(values, "{คำนำหน้าลูกจ้าง}", source.Employee.Prefix);
            Put(values, "{ชื่อลูกจ้าง}", source.Employee.GivenName);
            Put(values, "{นามสกุลลูกจ้าง}", source.Employee.Surname);
            Put(values, "{เลขประจำตัว}", source.Employee.NationalId, false);
            var rate = FindSalaryRate(source.Procurement.Salary);
            Put(values, "{ตำแหน่ง}", position.label);
            Put(values, "{เงินเดือน}", rate == null ? "" : rate.Salary);
            Put(values, "{เงินเดือนTEXT}", rate == null ? "" : rate.SalaryText);
            Put(values, "{เงินรวม}", rate == null ? "" : rate.TotalSalary);
            Put(values, "{เงินรวมTEXT}", rate == null ? "" : rate.TotalSalaryText);
            Put(values, "{บาทอากร}", rate == null ? "" : rate.DutyTax, false);
            Put(values, "{คำสั่งสเปค}", source.Procurement.SpecificationOrder);
            for (var i = 0; i < 3; i++)
            {
                var member = source.Committee[i] ?? new CommitteeMemberRecord();
                var suffix = ((char)('A' + i)).ToString();
                Put(values, "{คำนำหน้าสเปค" + suffix + "}", member.Prefix);
                Put(values, "{ชื่อสเปค" + suffix + "}", member.GivenName);
                Put(values, "{นามสกุลสเปค" + suffix + "}", member.Surname);
                Put(values, "{ตำแหน่งสเปค" + suffix + "}", member.Position);
            }
            Put(values, "{สัญชาติ}", source.Employee.Nationality);
            Put(values, "{เชื้อชาติ}", source.Employee.Race);
            Put(values, "{ศาสนา}", source.Employee.Religion);
            Put(values, "{เกิดวันที่}", source.Employee.BirthDay);
            Put(values, "{เดือนเกิด}", source.Employee.BirthMonth);
            Put(values, "{ปีเกิด}", source.Employee.BirthYear);
            Put(values, "{อายุลูกจ้าง}", source.Employee.Age);
            Put(values, "{ออกอำเภอ}", StripPrefix(source.Employee.IdIssueDistrict, "อำเภอ"));
            Put(values, "{ออกจังหวัด}", StripPrefix(source.Employee.IdIssueProvince, "จังหวัด"));
            Put(values, "{วันที่ออกบัตร}", source.Employee.IdIssueDay);
            Put(values, "{เดือนออกบัตร}", source.Employee.IdIssueMonth);
            Put(values, "{ปีออกบัตร}", source.Employee.IdIssueYear);
            Put(values, "{วันบัตรหมดอายุ}", source.Employee.IdExpiryDay);
            Put(values, "{เดือนบัตรหมดอายุ}", source.Employee.IdExpiryMonth);
            Put(values, "{ปีบัตรหมดอายุ}", source.Employee.IdExpiryYear);
            Put(values, "{สำเร็จการศึกษาระดับ}", source.Employee.EducationLevel);
            Put(values, "{คุณวุฒิการศึกษา}", source.Employee.Qualification);
            Put(values, "{บ้านเลขที่ลูกจ้าง}", source.Employee.HouseNumber);
            Put(values, "{ตำบลลูกจ้าง}", StripPrefix(source.Employee.Subdistrict, "ตำบล"));
            Put(values, "{อำเภอลูกจ้าง}", StripPrefix(source.Employee.District, "อำเภอ"));
            Put(values, "{จังหวัดลูกจ้าง}", StripPrefix(source.Employee.Province, "จังหวัด"));
            return values;
        }

        private void PutPerson(Dictionary<string, string> values, string suffix, PersonRecord person)
        {
            person = person ?? new PersonRecord();
            Put(values, "{คำนำหน้า" + suffix + "}", person.Prefix);
            Put(values, "{ชื่อ" + suffix + "}", person.GivenName);
            Put(values, "{นามสกุล" + suffix + "}", person.Surname);
        }

        private static void Put(Dictionary<string, string> values, string tag, string value, bool optional = true)
        {
            value = (value ?? "").Trim();
            values[tag] = optional && value.Length == 0 ? EmptyValue : value;
        }

        private SalaryRateOption FindSalaryRate(string value)
        {
            var cleaned = (value ?? "").Replace(",", "").Trim();
            return salaryRates.FirstOrDefault(x => (x.Salary ?? "").Replace(",", "").Trim() == cleaned);
        }

        private static void NormalizeRecord(WorkingRecord source)
        {
            source.School.Name = "";
            source.School.District = NormalizeDistrictName(source.School.District);
            source.School.DistrictShort = (source.School.DistrictShort ?? "").Trim();
            source.Procurement.HasTeaching = false;
            source.Procurement.WorksAtTwoSchools = false;
            source.Procurement.SecondSchoolName = "";
            source.Employee.NationalId = (source.Employee.NationalId ?? "").Trim();
        }

        private static string NormalizeDistrictName(string input)
        {
            var value = Regex.Replace((input ?? "").Trim(), @"\s+", " ");
            if (value.Length == 0) return "";
            if (value.StartsWith("สำนักงานเขตพื้นที่การศึกษา", StringComparison.Ordinal)) return value;
            if (value.StartsWith("ประถมศึกษา", StringComparison.Ordinal) || value.StartsWith("มัธยมศึกษา", StringComparison.Ordinal)) return "สำนักงานเขตพื้นที่การศึกษา" + value;
            return "สำนักงานเขตพื้นที่การศึกษาประถมศึกษา" + value;
        }

private static string StripPrefix(string input, string prefix)
        {
            var value = (input ?? "").Trim();
            while (value.StartsWith(prefix + prefix, StringComparison.Ordinal)) value = value.Substring(prefix.Length).TrimStart();
            return value.StartsWith(prefix, StringComparison.Ordinal) ? value.Substring(prefix.Length).TrimStart() : value;
        }

        private static string DutyTax(string totalSalary)
        {
            decimal amount;
            if (!decimal.TryParse((totalSalary ?? "").Replace(",", ""), out amount)) return "";
            return decimal.Truncate(amount / 1000m).ToString("0");
        }

        private static string SafeFilePart(string value)
        {
            var safe = string.IsNullOrWhiteSpace(value) ? "-" : value.Trim();
            foreach (var invalid in Path.GetInvalidFileNameChars()) safe = safe.Replace(invalid, '_');
            return safe.Length > 80 ? safe.Substring(0, 80) : safe;
        }

        private void ClearCurrentSection()
        {
            var index = sectionList == null ? 0 : sectionList.SelectedIndex;
            suppressChanges = true;
            try
            {
                var keys = fields.Keys.Where(x => (index == 0 && x.StartsWith("school.", StringComparison.Ordinal))
                    || (index == 1 && (x.StartsWith("committee.", StringComparison.Ordinal) || x == "procurement.specificationOrder"))
                    || (index == 2 && (x.StartsWith("employee.", StringComparison.Ordinal) || x.StartsWith("procurement.", StringComparison.Ordinal)) && x != "procurement.specificationOrder")).ToArray();
                foreach (var key in keys) Set(key, "");
                if (index == 2)
                {
                    foreach (var combo in dateCombos.Values) combo.SelectedIndex = 0;
                    foreach (var custom in dateCustomBoxes.Values) custom.Text = "";
                }
                if (index == 2)
                {
                    positionBox.SelectedIndex = 0;

                    UpdateRouteControls();
                }
                if (index == 3)
                {
                    foreach (var check in documentChecks) check.Checked = true;
                }
                CaptureFromControls();
                isDirty = true;
            }
            finally
            {
                suppressChanges = false;
            }
            if (sharedChanged != null) sharedChanged();
        }

        private void AddNewEmployee()
        {
            CaptureFromControls();
            var answer = MessageBox.Show("ต้องการบันทึกข้อมูลปัจจุบันเป็น Template ของ " + (record.Employee.GivenName + " " + record.Employee.Surname).Trim() + " ก่อนเริ่มลูกจ้างใหม่หรือไม่?", "เพิ่มลูกจ้างใหม่", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (answer == DialogResult.Cancel) return;
            if (answer == DialogResult.Yes)
            {
                var result = SaveCurrentTemplate();
                if (result != TemplateOperationResult.Saved) return;
            }
            var empty = WorkingRecord.CreateEmpty();
            record.RecordId = empty.RecordId;
            record.School = empty.School;
            record.Employee = empty.Employee;
            record.Committee = empty.Committee;
            record.Payroll = empty.Payroll;
            record.Procurement = empty.Procurement;
            SetDefaults();
            RefreshFromRecord();
            isDirty = false;
            if (sharedChanged != null) sharedChanged();
        }

        private void LoadTemplateFlow()
        {
            var procurementTemplates = (storeData.Templates ?? new List<SavedTemplateSnapshot>())
                .Select(TemplateTransferService.FromProcurement)
                .Where(x => !string.IsNullOrWhiteSpace(x.Name))
                .ToList();
            var payrollTemplates = TemplateTransferService.LoadPayrollTemplates();
            if (procurementTemplates.Count == 0 && payrollTemplates.Count == 0)
            {
                MessageBox.Show("ยังไม่มี Template ให้เลือกโหลด", "โหลด Template", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var picker = new CrossTemplatePickerDialog(DocumentModule.Procurement, procurementTemplates, payrollTemplates))
            {
                if (picker.ShowDialog(this) != DialogResult.OK || picker.Selected == null) return;
                var selected = picker.Selected;
                if (selected.SourceModule == DocumentModule.Procurement)
                {
                    var local = (storeData.Templates ?? new List<SavedTemplateSnapshot>()).FirstOrDefault(x => x.Id == selected.Id);
                    if (local == null) return;
                    ApplyLoadedRecord(store.Clone(local.Record));
                    MessageBox.Show("โหลด Template จัดซื้อจัดจ้างฯ แล้ว", "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var report = TemplateTransferService.BuildReport(selected, DocumentModule.Procurement);
                using (var notice = new CrossTemplateImportNoticeDialog(selected, DocumentModule.Procurement, report))
                {
                    if (notice.ShowDialog(this) != DialogResult.OK) return;
                }
                ApplyImportedProcurementValues(selected);
                MessageBox.Show("โหลดข้อมูลร่วมจาก Template เบิกเงินเดือนแล้ว และเขียนทับข้อมูลร่วมทั้งหมดตามรายการ", "โหลด Template สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void DeleteTemplateFlow()
        {
            if (storeData.Templates == null || !storeData.Templates.Any(x => x != null && !string.IsNullOrWhiteSpace(x.Name)))
            {
                MessageBox.Show("ยังไม่มี Template ให้ลบ", "ลบ Template", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var draft = store.Clone(storeData);
            using (var dialog = new ProcurementTemplateDeleteDialog(draft.Templates))
            {
                dialog.ShowDialog(this);
                if (!dialog.IsDirty) return;
                try
                {
                    store.Save(draft);
                    storeData = draft;
                    MessageBox.Show("ลบ Template แล้ว", "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Khet70StoreException ex)
                {
                    MessageBox.Show(ex.Message, "ลบ Template ไม่สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ApplyLoadedRecord(WorkingRecord loaded)
        {
            if (loaded == null) return;
            record.RecordId = loaded.RecordId;
            record.School = loaded.School;
            record.Employee = loaded.Employee;
            record.Committee = loaded.Committee;
            record.Payroll = loaded.Payroll;
            record.Procurement = loaded.Procurement;
            RefreshFromRecord();
            isDirty = false;
            if (sharedChanged != null) sharedChanged();
        }

        private void ApplyImportedProcurementValues(TemplateTransferItem item)
        {
            var values = TemplateTransferService.GetImportableValues(item);
            if (values.Count == 0)
            {
                MessageBox.Show("Template ต้นทางไม่มีข้อมูลร่วมที่โหลดได้", "โหลด Template", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string value;
            if (values.TryGetValue("school.district", out value)) record.School.District = value;
            if (values.TryGetValue("school.districtShort", out value)) record.School.DistrictShort = value;
            if (record.School.Director == null) record.School.Director = new PersonRecord();
            if (record.School.SupplyOfficer == null) record.School.SupplyOfficer = new PersonRecord();
            if (record.School.HeadSupplyOfficer == null) record.School.HeadSupplyOfficer = new PersonRecord();
            ApplyImportedPerson(values, "school.director", record.School.Director);
            ApplyImportedPerson(values, "school.supply", record.School.SupplyOfficer);
            ApplyImportedPerson(values, "school.headSupply", record.School.HeadSupplyOfficer);
            if (record.School.FinanceDirector == null) record.School.FinanceDirector = new PersonRecord();
            if (record.School.DeputyDistrictDirector == null) record.School.DeputyDistrictDirector = new PersonRecord();
            ApplyImportedPerson(values, "school.financeDirector", record.School.FinanceDirector);
            ApplyImportedPerson(values, "school.deputyDistrict", record.School.DeputyDistrictDirector);

            if (values.TryGetValue("employee.prefix", out value)) record.Employee.Prefix = value;
            if (values.TryGetValue("employee.given", out value)) record.Employee.GivenName = value;
            if (values.TryGetValue("employee.surname", out value)) record.Employee.Surname = value;
            if (values.TryGetValue("employee.salary", out value)) record.Procurement.Salary = value;
            if (values.TryGetValue("employee.position", out value))
            {
                var positionId = TemplateTransferService.MapPayrollPositionToProcurement(value);
                if (!string.IsNullOrWhiteSpace(positionId)) SelectPosition(positionId);
            }
            if (values.TryGetValue("employee.nationalId", out value)) record.Employee.NationalId = value;
            if (values.TryGetValue("employee.birth.day", out value)) record.Employee.BirthDay = value;
            if (values.TryGetValue("employee.birth.month", out value)) record.Employee.BirthMonth = value;
            if (values.TryGetValue("employee.birth.year", out value)) record.Employee.BirthYear = value;
            if (values.TryGetValue("employee.age", out value)) record.Employee.Age = value;
            if (values.TryGetValue("employee.nationality", out value)) record.Employee.Nationality = value;
            if (values.TryGetValue("employee.race", out value)) record.Employee.Race = value;
            if (values.TryGetValue("employee.religion", out value)) record.Employee.Religion = value;
            if (values.TryGetValue("employee.idIssueDistrict", out value)) record.Employee.IdIssueDistrict = value;
            if (values.TryGetValue("employee.idIssueProvince", out value)) record.Employee.IdIssueProvince = value;
            if (values.TryGetValue("employee.idIssue.day", out value)) record.Employee.IdIssueDay = value;
            if (values.TryGetValue("employee.idIssue.month", out value)) record.Employee.IdIssueMonth = value;
            if (values.TryGetValue("employee.idIssue.year", out value)) record.Employee.IdIssueYear = value;
            if (values.TryGetValue("employee.idExpiry.day", out value)) record.Employee.IdExpiryDay = value;
            if (values.TryGetValue("employee.idExpiry.month", out value)) record.Employee.IdExpiryMonth = value;
            if (values.TryGetValue("employee.idExpiry.year", out value)) record.Employee.IdExpiryYear = value;
            if (values.TryGetValue("employee.educationLevel", out value)) record.Employee.EducationLevel = value;
            if (values.TryGetValue("employee.qualification", out value)) record.Employee.Qualification = value;
            if (values.TryGetValue("employee.houseNumber", out value)) record.Employee.HouseNumber = value;
            if (values.TryGetValue("employee.road", out value)) record.Employee.Road = value;
            if (values.TryGetValue("employee.subdistrict", out value)) record.Employee.Subdistrict = value;
            if (values.TryGetValue("employee.district", out value)) record.Employee.District = value;
            if (values.TryGetValue("employee.province", out value)) record.Employee.Province = value;
            for (var i = 0; i < 3; i++)
            {
                if (record.Committee[i] == null) record.Committee[i] = new CommitteeMemberRecord();
                ApplyImportedPerson(values, "committee." + i, record.Committee[i]);
            }

            RefreshFromRecord();
            isDirty = true;
            if (sharedChanged != null) sharedChanged();
        }

        private void ApplyImportedPerson(Dictionary<string, string> values, string key, PersonRecord target)
        {
            if (target == null) return;
            string value;
            if (values.TryGetValue(key + ".prefix", out value)) target.Prefix = value;
            if (values.TryGetValue(key + ".given", out value)) target.GivenName = value;
            if (values.TryGetValue(key + ".surname", out value)) target.Surname = value;
        }

        private sealed class SalaryRateOption
        {
            public string Salary;
            public string SalaryText;
            public string TotalSalary;
            public string TotalSalaryText;
            public string DutyTax;
            public override string ToString() { return Salary; }
        }
        private sealed class PositionView
        {
            public readonly ProcurementPositionConfig Config;
            public PositionView(ProcurementPositionConfig config) { Config = config; }
            public override string ToString() { return Config == null ? PositionPlaceholder : (string.IsNullOrWhiteSpace(Config.dropdownLabel) ? Config.label : Config.dropdownLabel); }
        }
    }

    internal sealed class ProcurementTemplateDialog : Form
    {
        private readonly TextBox nameBox = new TextBox();
        private readonly TextBox noteBox = new TextBox();
        public string TemplateName { get { return nameBox.Text.Trim(); } }
        public string TemplateNote { get { return noteBox.Text.Trim(); } }

        public ProcurementTemplateDialog(string title, string initialName)
        {
            Text = title;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MinimizeBox = false;
            MaximizeBox = false;
            ClientSize = new Size(460, 235);
            Controls.Add(new Label { Text = "ชื่อรายการ Template *", Location = new Point(20, 20), Size = new Size(190, 22) });
            nameBox.Location = new Point(20, 48); nameBox.Size = new Size(420, 26); nameBox.Text = initialName ?? ""; Controls.Add(nameBox);
            Controls.Add(new Label { Text = "หมายเหตุ (ถ้ามี)", Location = new Point(20, 88), Size = new Size(190, 22) });
            noteBox.Location = new Point(20, 116); noteBox.Size = new Size(420, 54); noteBox.Multiline = true; Controls.Add(noteBox);
            var ok = new Button { Text = "บันทึก", Location = new Point(274, 188), Size = new Size(80, 30) };
            var cancel = new Button { Text = "ยกเลิก", Location = new Point(360, 188), Size = new Size(80, 30), DialogResult = DialogResult.Cancel };
            ok.Click += delegate { if (TemplateName.Length == 0) { MessageBox.Show("กรุณากรอกชื่อรายการ", "ข้อมูลไม่ครบ", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; } DialogResult = DialogResult.OK; Close(); };
            Controls.Add(ok); Controls.Add(cancel); AcceptButton = ok; CancelButton = cancel;
        }
    }

    internal sealed class ProcurementTemplateDeleteDialog : Form
    {
        private readonly IList<SavedTemplateSnapshot> templates;
        private readonly ListBox list = new ListBox();
        public bool IsDirty { get; private set; }

        public ProcurementTemplateDeleteDialog(IList<SavedTemplateSnapshot> templates)
        {
            this.templates = templates ?? new List<SavedTemplateSnapshot>();
            Text = "ลบ Template";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MinimizeBox = false; MaximizeBox = false; ClientSize = new Size(480, 330);
            Controls.Add(new Label { Text = "เลือก Template ที่ต้องการลบ", Location = new Point(20, 16), Size = new Size(240, 20) });
            list.Location = new Point(20, 44);
            list.Size = new Size(440, 220);
            list.DisplayMember = "Name";
            Controls.Add(list);

            var delete = new Button { Text = "ลบรายการที่เลือก", Location = new Point(260, 280), Size = new Size(110, 30) };
            var cancel = new Button { Text = "ปิด", Location = new Point(380, 280), Size = new Size(80, 30), DialogResult = DialogResult.Cancel };
            delete.Click += delegate
            {
                var selected = list.SelectedItem as SavedTemplateSnapshot;
                if (selected == null)
                {
                    MessageBox.Show("กรุณาเลือกรายการ", "ยังไม่ได้เลือก", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (MessageBox.Show("ยืนยันการลบ Template \"" + selected.Name + "\" ?", "ยืนยันการลบ", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
                templates.Remove(selected);
                IsDirty = true;
                RebindList();
            };
            Controls.Add(delete);
            Controls.Add(cancel);
            CancelButton = cancel;
            RebindList();
        }

        private void RebindList()
        {
            list.DataSource = null;
            list.DataSource = templates.Where(x => x != null && !string.IsNullOrWhiteSpace(x.Name)).ToList();
            list.DisplayMember = "Name";
        }
    }
}

