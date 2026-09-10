using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Jangmao70
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
        private const string DefaultDistrict = "สำนักงานเขตพื้นที่การศึกษาประถมศึกษาแม่ฮ่องสอน เขต 2";
        private const string DistrictPrefix = "สำนักงานเขตพื้นที่การศึกษา";
        private const string EmptyValue = ".............";
        private const string UnspecifiedOption = "-- ไม่ระบุ --";
        private const string PositionPlaceholder = "-- เลือกตำแหน่ง --";
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
        private readonly Jangmao70Store store = new Jangmao70Store(DocumentModule.Procurement);
        private readonly ErrorProvider errorProvider = new ErrorProvider();
        private readonly Dictionary<string, Control> fields = new Dictionary<string, Control>();
        private readonly Dictionary<string, ComboBox> dateCombos = new Dictionary<string, ComboBox>();
        private readonly Dictionary<string, TextBox> dateCustomBoxes = new Dictionary<string, TextBox>();
        private readonly List<Panel> sectionPanels = new List<Panel>();
        private readonly List<CheckBox> documentChecks = new List<CheckBox>();
        private readonly List<ProcurementPositionConfig> positions = new List<ProcurementPositionConfig>();
        private readonly ProcurementManifest manifest;
        private Jangmao70StoreData storeData;
        private ComboBox districtBox;
        private ComboBox positionBox;
        private CheckBox teachingBox;
        private CheckBox twoSchoolsBox;
        private Panel secondSchoolRow;
        private TextBox secondSchoolBox;
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
            try
            {
                storeData = store.Load();
            }
            catch (Jangmao70StoreException ex)
            {
                storeData = new Jangmao70StoreData();
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
                Set("school.name", record.School.Name);
                Set("school.district", record.School.District);
                SetPerson("school.director", record.School.Director);
                SetPerson("school.supply", record.School.SupplyOfficer);
                SetPerson("school.headSupply", record.School.HeadSupplyOfficer);
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

        public bool HasMeaningfulData()
        {
            CaptureFromControls();
            return !string.IsNullOrWhiteSpace(record.Employee.GivenName)
                || !string.IsNullOrWhiteSpace(record.Employee.Surname)
                || !string.IsNullOrWhiteSpace(record.Employee.NationalId)
                || !string.IsNullOrWhiteSpace(record.School.Name)
                || record.Procurement.SelectedDocuments.Count > 0;
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
                catch (Jangmao70StoreException ex)
                {
                    MessageBox.Show(ex.Message, "บันทึก Template ไม่สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return TemplateOperationResult.Failed;
                }
            }
        }

        private ProcurementManifest LoadManifest()
        {
            var path = Path.Combine(Jangmao70Paths.ShippedConfigRoot(DocumentModule.Procurement), "template_manifest.json");
            return Jangmao70Config.Load<ProcurementManifest>(path);
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
            var path = Path.Combine(Jangmao70Paths.ShippedConfigRoot(DocumentModule.Procurement), "position_catalog.json");
            var catalog = Jangmao70Config.Load<ProcurementPositionCatalog>(path);
            if (catalog.positions == null || catalog.positions.Length != 5) throw new InvalidDataException("Procurement position catalog ต้องมี 5 ตำแหน่ง");
            positions.AddRange(catalog.positions);
        }

        private void BuildShell()
        {
            var header = new Panel { Dock = DockStyle.Top, Height = 76, BackColor = ModuleAccent, Padding = new Padding(24, 12, 24, 10) };
            var accentBar = new Panel { Dock = DockStyle.Left, Width = 5, BackColor = Color.FromArgb(185, 230, 204) };
            var title = new Label { Text = "Jangmao70  |  จัดซื้อจัดจ้างและสัญญา", Font = new Font("Segoe UI", 17f, FontStyle.Bold), ForeColor = Color.White, Dock = DockStyle.Left, Width = 520, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(12, 0, 0, 0) };
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

            var body = new Panel { Dock = DockStyle.Fill, BackColor = ModuleBackground };
            var navigation = new Panel { Dock = DockStyle.Left, Width = 350, BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Padding = new Padding(20, 16, 16, 16) };
            var navigationTitle = new Label { Text = "ขั้นตอนการกรอกข้อมูล", Dock = DockStyle.Top, Height = 30, Font = new Font("Segoe UI", 10.5f, FontStyle.Bold), ForeColor = Navy };
            var navigationHint = new Label { Text = "เลือกหมวดเพื่อแก้ไขข้อมูล", Dock = DockStyle.Top, Height = 30, ForeColor = MutedText, Font = new Font("Segoe UI", 9.5f) };
            sectionList = new ListBox { Dock = DockStyle.Fill, BorderStyle = BorderStyle.None, BackColor = Color.White, Font = new Font("Segoe UI", 10.0f), IntegralHeight = false, DrawMode = DrawMode.OwnerDrawFixed, ItemHeight = 42 };
            sectionList.Items.AddRange(new object[] { "1  ข้อมูลโรงเรียน", "2  กรรมการ TOR", "3  ข้อมูลลูกจ้าง", "4  สรุปชุดเอกสารที่ต้องการสร้าง" });
            sectionList.SelectedIndexChanged += delegate { ShowSection(sectionList.SelectedIndex); };
            sectionList.DrawItem += DrawSectionItem;
            navigation.Controls.Add(sectionList);
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
            using (var font = new Font(sectionList.Font, selected ? FontStyle.Bold : FontStyle.Regular))
            using (var brush = new SolidBrush(foreground))
            {
                e.Graphics.DrawString(sectionList.Items[e.Index].ToString(), font, brush, bounds, new StringFormat { LineAlignment = StringAlignment.Center });
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
            AddHeading(parent, "ข้อมูลโรงเรียนและผู้ลงนามของ record นี้", "ชื่อโรงเรียนและบุคลากรจะไม่กลายเป็นค่า global ของลูกจ้างคนอื่น");
            AddText(parent, "ชื่อโรงเรียน *", "school.name", 24, 82, 620);
            districtBox = AddEditableCombo(parent, "เขตพื้นที่การศึกษา *", "school.district", 24, 150, 620);
            districtBox.Items.Add(DefaultDistrict);
            districtBox.SelectedIndex = 0;
            AddPersonFields(parent, "ผู้อำนวยการโรงเรียน", "school.director", 224);
            AddPersonFields(parent, "เจ้าหน้าที่พัสดุ", "school.supply", 344);
            AddPersonFields(parent, "หัวหน้าเจ้าหน้าที่พัสดุ", "school.headSupply", 464);
        }

        private void BuildCommitteeSection(Panel parent)
        {
            AddHeading(parent, "คณะกรรมการกำหนด TOR", "กรอกกรรมการ 1–3 ให้ครบ โดยกรรมการ 1 เป็นประธาน และใช้ชุดเดียวกับเอกสารเบิกเงินเดือน");
            AddCommitteeFields(parent, 0, 92, "กรรมการ 1 (ประธาน)");
            AddCommitteeFields(parent, 1, 252, "กรรมการ 2");
            AddCommitteeFields(parent, 2, 412, "กรรมการ 3");
            AddText(parent, "เลขที่คำสั่งแต่งตั้ง TORและตรวจรับ", "procurement.specificationOrder", 24, 580, 220);
            parent.Controls.Add(new Label { Text = "(เช่น 1/2570)", Location = new Point(256, 604), Size = new Size(130, 26), ForeColor = Color.FromArgb(80, 95, 115), TextAlign = ContentAlignment.MiddleLeft });
        }

        private void BuildEmployeeSection(Panel parent)
        {
            AddHeading(parent, "ข้อมูลลูกจ้าง", "ช่องที่มี * จำเป็นสำหรับการสร้างเอกสาร | ช่องที่ไม่ได้กรอกจะแทนที่ด้วย ..............");
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

            AddZoneHeading(parent, "ตำแหน่งและค่าตอบแทน", 446);
            positionBox = AddCombo(parent, "ตำแหน่ง *", "procurement.position", 24, 480, 330);
            positionBox.Items.Add(new PositionView(null));
            foreach (var position in positions) positionBox.Items.Add(new PositionView(position));
            positionBox.SelectedIndexChanged += delegate { UpdateRouteControls(); OnUiChanged(); };
            teachingBox = new CheckBox { Text = "มีงานสอน", Location = new Point(374, 482), Width = 130 };
            teachingBox.CheckedChanged += delegate { UpdateRouteControls(); OnUiChanged(); };
            parent.Controls.Add(teachingBox);
            twoSchoolsBox = new CheckBox { Text = "ทำงาน 2 โรงเรียน", Location = new Point(514, 482), Width = 170 };
            twoSchoolsBox.CheckedChanged += delegate { UpdateRouteControls(); OnUiChanged(); };
            parent.Controls.Add(twoSchoolsBox);
            secondSchoolRow = new Panel { Location = new Point(24, 536), Size = new Size(620, 76), BackColor = Color.White, Visible = false };
            secondSchoolRow.Controls.Add(new Label { Text = "ชื่อโรงเรียนที่สอง", Location = new Point(0, 0), Size = new Size(500, 20), ForeColor = Color.FromArgb(31, 41, 55) });
            secondSchoolBox = new TextBox { Location = new Point(0, 24), Size = new Size(500, 26), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.White };
            secondSchoolBox.TextChanged += delegate { OnUiChanged(); };
            secondSchoolRow.Controls.Add(secondSchoolBox);
            parent.Controls.Add(secondSchoolRow);
            fields["procurement.secondSchool"] = secondSchoolBox;
            salaryLabel = AddReadOnlyValue(parent, "เงินเดือน", 24, 640, 220);
            totalSalaryLabel = AddReadOnlyValue(parent, "เงินรวม 12 เดือน / ค่าอากรแสตมป์", 264, 640, 300);

            AddZoneHeading(parent, "การศึกษา", 720);
            AddEditableCombo(parent, "สำเร็จการศึกษาระดับ", "employee.educationLevel", 24, 754, 330).Items.AddRange(new object[] { UnspecifiedOption, "ประถมศึกษาตอนต้น", "ประถมศึกษาตอนปลาย", "มัธยมศึกษาตอนต้น", "มัธยมศึกษาตอนปลาย", "ปวช", "ปวส", "ปริญญาตรี", "ปริญญาโท", "ปริญญาเอก", "......(กรอกเอง)" });
            AddEditableCombo(parent, "คุณวุฒิการศึกษา", "employee.qualification", 374, 754, 340).Items.AddRange(new object[] { UnspecifiedOption, "ปวช.", "ปวส.", "ครุศาสตรบัณฑิต (ค.บ.)", "ศึกษาศาสตรบัณฑิต (ศษ.บ.)", "ศิลปศาสตรบัณฑิต (ศศ.บ.)", "วิทยาศาสตรบัณฑิต (วท.บ.)", "วิศวกรรมศาสตรบัณฑิต (วศ.บ.)", "บริหารธุรกิจบัณฑิต (บธ.บ.)", "นิติศาสตรบัณฑิต (น.บ.)", "รัฐศาสตรบัณฑิต (ร.บ.)", "รัฐประศาสนศาสตรบัณฑิต (รป.บ.)", "สาธารณสุขศาสตรบัณฑิต (ส.บ.)", "พยาบาลศาสตรบัณฑิต (พย.บ.)", "ปริญญาโท", "ปริญญาเอก", "อื่น ๆ (กรอกเอง)" });

            AddZoneHeading(parent, "ที่อยู่ / บัตรประชาชน", 866);
            AddText(parent, "บ้านเลขที่", "employee.houseNumber", 24, 900, 150);
            AddText(parent, "ถนน", "employee.road", 194, 900, 220);
            AddText(parent, "ตำบล", "employee.subdistrict", 434, 900, 170);
            AddText(parent, "อำเภอ", "employee.district", 24, 980, 200);
            AddText(parent, "จังหวัด", "employee.province", 244, 980, 200);
            AddZoneHeading(parent, "สถานที่ออกบัตร", 1060);
            AddText(parent, "ออกอำเภอ", "employee.idIssueDistrict", 24, 1098, 250);
            AddText(parent, "ออกจังหวัด", "employee.idIssueProvince", 294, 1098, 250);

            AddZoneHeading(parent, "วันออกบัตร", 1180);
            AddDateField(parent, "วัน", "employee.idIssue.day", "employee.idIssue.dayCustom", 24, 1218);
            AddDateField(parent, "เดือน", "employee.idIssue.month", "employee.idIssue.monthCustom", 244, 1218);
            AddText(parent, "ปี", "employee.idIssue.year", 464, 1218, 150);

            AddZoneHeading(parent, "วันหมดอายุบัตร", 1300);
            AddDateField(parent, "วัน", "employee.idExpiry.day", "employee.idExpiry.dayCustom", 24, 1338);
            AddDateField(parent, "เดือน", "employee.idExpiry.month", "employee.idExpiry.monthCustom", 244, 1338);
            AddText(parent, "ปี", "employee.idExpiry.year", 464, 1338, 150);
            parent.AutoScrollMinSize = new Size(0, 1480);
        }

        private void BuildSummarySection(Panel parent)
        {
            AddHeading(parent, "สรุปชุดเอกสารที่ต้องการสร้าง", "ตรวจรายการเอกสารที่ต้องการก่อนกดตรวจสอบและสร้างเอกสาร");
            documentPanel = new Panel { Location = new Point(24, 96), Size = new Size(690, 560), AutoScroll = true, BorderStyle = BorderStyle.FixedSingle, BackColor = Color.White };
            parent.Controls.Add(documentPanel);
            UpdateRouteControls();
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
            var group = new Panel { Location = new Point(16, y - 8), Size = new Size(700, 154), BackColor = Color.FromArgb(250, 253, 251), BorderStyle = BorderStyle.FixedSingle, TabStop = false };
            parent.Controls.Add(group);
            group.Controls.Add(new Panel { Location = new Point(0, 0), Size = new Size(5, 36), BackColor = ModuleAccent });
            group.Controls.Add(new Label { Text = title, Font = new Font("Segoe UI", 10f, FontStyle.Bold), Location = new Point(5, 0), Size = new Size(689, 36), BackColor = Color.FromArgb(248, 250, 252), ForeColor = TextColor, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(12, 0, 0, 0) });
            var prefixBox = AddEditableCombo(group, "คำนำหน้า", prefix + ".prefix", 12, 42, 150);
            prefixBox.Items.AddRange(new object[] { UnspecifiedOption, "นาย", "นาง", "นางสาว", "ว่าที่ร้อยตรี", "......(กรอกเอง)" });
            AddText(group, "ชื่อ", prefix + ".given", 182, 42, 220);
            AddText(group, "นามสกุล", prefix + ".surname", 424, 42, 180);
            var position = AddEditableCombo(group, "ตำแหน่ง", prefix + ".position", 12, 100, 580);
            position.Items.AddRange(new object[] { UnspecifiedOption, "ครูผู้ช่วย", "ครู", "ครูชำนาญการ", "ครูชำนาญการพิเศษ", "รองผู้อำนวยการโรงเรียน", "ผู้อำนวยการโรงเรียน", "......(กรอกเอง)" });
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
                if (teachingBox != null) teachingBox.Checked = false;
                if (twoSchoolsBox != null) twoSchoolsBox.Checked = false;
                if (secondSchoolRow != null) secondSchoolRow.Visible = false;
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
            if (sharedChanged != null) sharedChanged();
        }

        private void CaptureFromControls()
        {
            record.School.Name = Read("school.name");
            record.School.District = Read("school.district");
            record.School.Director = ReadPerson("school.director");
            record.School.SupplyOfficer = ReadPerson("school.supply");
            record.School.HeadSupplyOfficer = ReadPerson("school.headSupply");
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
            record.Procurement.HasTeaching = teachingBox != null && teachingBox.Checked;
            record.Procurement.WorksAtTwoSchools = twoSchoolsBox != null && twoSchoolsBox.Checked;
            record.Procurement.SecondSchoolName = Read("procurement.secondSchool");
            record.Procurement.SpecificationOrder = Read("procurement.specificationOrder");
            record.Procurement.SelectedDocuments = documentChecks.Where(x => x.Checked).Select(x => (string)x.Tag).ToList();
        }

        private PersonRecord ReadPerson(string prefix)
        {
            return new PersonRecord { Prefix = Read(prefix + ".prefix"), GivenName = Read(prefix + ".given"), Surname = Read(prefix + ".surname"), Position = Read(prefix + ".position") };
        }

        private CommitteeMemberRecord ReadCommitteePerson(string prefix)
        {
            var person = ReadPerson(prefix);
            return new CommitteeMemberRecord { Prefix = person.Prefix, GivenName = person.GivenName, Surname = person.Surname, Position = person.Position };
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
                Set("school.name", record.School.Name);
                Set("school.district", record.School.District);
                SetPerson("school.director", record.School.Director);
                SetPerson("school.supply", record.School.SupplyOfficer);
                SetPerson("school.headSupply", record.School.HeadSupplyOfficer);
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
                teachingBox.Checked = record.Procurement.HasTeaching;
                twoSchoolsBox.Checked = record.Procurement.WorksAtTwoSchools;
                Set("procurement.secondSchool", record.Procurement.SecondSchoolName);
                Set("procurement.specificationOrder", record.Procurement.SpecificationOrder);
                UpdateRouteControls();
                isDirty = false;
            }
            finally
            {
                suppressChanges = false;
            }
        }

        private void SetPerson(string prefix, PersonRecord person)
        {
            person = person ?? new PersonRecord();
            Set(prefix + ".prefix", person.Prefix);
            Set(prefix + ".given", person.GivenName);
            Set(prefix + ".surname", person.Surname);
            Set(prefix + ".position", person.Position);
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
            var route = manifest.routes.FirstOrDefault(x => x.positionId == position.id && x.hasTeaching == teachingBox.Checked && x.worksAtTwoSchools == twoSchoolsBox.Checked);
            return route;
        }

        private void UpdateRouteControls()
        {
            if (positionBox == null) return;
            var position = CurrentPosition();
            var teachingEnabled = position != null && position.teachingFlag;
            var twoSchoolsEnabled = position != null && position.twoSchoolsFlag;
            teachingBox.Enabled = teachingEnabled;
            twoSchoolsBox.Enabled = twoSchoolsEnabled;
            if (!teachingEnabled) teachingBox.Checked = false;
            if (!twoSchoolsEnabled) twoSchoolsBox.Checked = false;
            secondSchoolRow.Visible = twoSchoolsBox.Checked;
            if (!twoSchoolsBox.Checked) secondSchoolBox.Text = "";
            if (position == null)
            {
                salaryLabel.Text = "";
                totalSalaryLabel.Text = "";
            }
            else
            {
                salaryLabel.Text = position.salary + " บาท";
                totalSalaryLabel.Text = position.totalSalary + " / " + DutyTax(position.totalSalary);
            }
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
            if (string.IsNullOrWhiteSpace(candidate.School.Name)) errors.Add("ชื่อโรงเรียน");
            if (string.IsNullOrWhiteSpace(candidate.Employee.GivenName)) errors.Add("ชื่อลูกจ้าง");
            if (string.IsNullOrWhiteSpace(candidate.Employee.Surname)) errors.Add("นามสกุลลูกจ้าง");
            if (!Regex.IsMatch(candidate.Employee.NationalId ?? "", "^[0-9]{13}$")) errors.Add("เลขประจำตัวประชาชนต้องเป็นตัวเลข 13 หลัก");
            var position = positions.FirstOrDefault(x => x.id == candidate.Procurement.PositionId);
            if (position == null) errors.Add("ตำแหน่ง");
            if (candidate.Procurement.SelectedDocuments == null || candidate.Procurement.SelectedDocuments.Count == 0) errors.Add("เลือกเอกสารอย่างน้อย 1 รายการ");
            if (position != null && position.twoSchoolsFlag && candidate.Procurement.WorksAtTwoSchools && string.IsNullOrWhiteSpace(candidate.Procurement.SecondSchoolName)) errors.Add("ชื่อโรงเรียนที่สอง");
            for (var i = 0; i < candidate.Committee.Length; i++)
            {
                var member = candidate.Committee[i];
                if (member == null || string.IsNullOrWhiteSpace(member.Prefix) || string.IsNullOrWhiteSpace(member.GivenName) || string.IsNullOrWhiteSpace(member.Surname) || string.IsNullOrWhiteSpace(member.Position)) errors.Add("กรรมการ " + (i + 1));
            }
            return errors;
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
            var route = manifest.routes.FirstOrDefault(x => x.positionId == candidate.Procurement.PositionId && x.hasTeaching == candidate.Procurement.HasTeaching && x.worksAtTwoSchools == candidate.Procurement.WorksAtTwoSchools);
            if (route == null)
            {
                MessageBox.Show("ไม่พบ route ที่ตรงกับตำแหน่งและตัวเลือกที่เลือก", "Routing", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var values = BuildValues(candidate);
            var templateRoot = Jangmao70Paths.EnsureUserTemplateRoot(DocumentModule.Procurement);
            var outputRoot = Jangmao70Paths.OutputRoot(DocumentModule.Procurement);
            var created = new List<string>();
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                foreach (var relative in candidate.Procurement.SelectedDocuments)
                {
                    var source = Jangmao70Paths.ResolveUnder(templateRoot, relative);
                    var requested = Path.Combine(outputRoot, SafeFilePart(candidate.Employee.GivenName) + "_" + SafeFilePart(Path.GetFileNameWithoutExtension(relative)) + ".docx");
                    created.Add(Jangmao70DocxRenderer.Render(source, requested, values));
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
            Put(values, "{ชื่อโรงเรียน}", NormalizeSchoolName(source.School.Name));
            Put(values, "{ชื่อโรงเรียนสอง}", source.Procurement.WorksAtTwoSchools ? NormalizeSchoolName(source.Procurement.SecondSchoolName) : "");
            Put(values, "{ชื่อเขต}", NormalizeDistrictName(source.School.District));
            PutPerson(values, "ผอ", source.School.Director);
            PutPerson(values, "พัสดุ", source.School.SupplyOfficer);
            PutPerson(values, "หพัสดุ", source.School.HeadSupplyOfficer);
            Put(values, "{คำนำหน้าลูกจ้าง}", source.Employee.Prefix);
            Put(values, "{ชื่อลูกจ้าง}", source.Employee.GivenName);
            Put(values, "{นามสกุลลูกจ้าง}", source.Employee.Surname);
            Put(values, "{เลขประจำตัว}", source.Employee.NationalId, false);
            Put(values, "{ตำแหน่ง}", position.label);
            Put(values, "{เงินเดือน}", position.salary);
            Put(values, "{เงินเดือนTEXT}", position.salaryText);
            Put(values, "{เงินรวม}", position.totalSalary);
            Put(values, "{เงินรวมTEXT}", position.totalSalaryText);
            Put(values, "{บาทอากร}", DutyTax(position.totalSalary), false);
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

        private static void NormalizeRecord(WorkingRecord source)
        {
            source.School.Name = NormalizeSchoolName(source.School.Name);
            source.School.District = NormalizeDistrictName(source.School.District);
            source.Procurement.SecondSchoolName = NormalizeSchoolName(source.Procurement.SecondSchoolName);
            source.Employee.NationalId = (source.Employee.NationalId ?? "").Trim();
        }

        private static string NormalizeSchoolName(string input)
        {
            var value = (input ?? "").Trim();
            if (value.Length == 0) return "";
            while (value.StartsWith("โรงเรียนโรงเรียน", StringComparison.Ordinal)) value = value.Substring("โรงเรียน".Length).TrimStart();
            return value.StartsWith("โรงเรียน", StringComparison.Ordinal) ? value : "โรงเรียน" + value;
        }

        private static string NormalizeDistrictName(string input)
        {
            var value = Regex.Replace((input ?? "").Trim(), "\\s+", " ");
            if (value.Length == 0) return DefaultDistrict;
            if (value.StartsWith(DistrictPrefix, StringComparison.Ordinal)) return value;
            if (value.StartsWith("ประถมศึกษา", StringComparison.Ordinal) || value.StartsWith("มัธยมศึกษา", StringComparison.Ordinal)) return DistrictPrefix + value;
            return DistrictPrefix + "ประถมศึกษา" + value;
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
                    teachingBox.Checked = false;
                    twoSchoolsBox.Checked = false;
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
            if (values.TryGetValue("school.name", out value)) record.School.Name = value;
            if (values.TryGetValue("school.district", out value)) record.School.District = value;
            if (record.School.Director == null) record.School.Director = new PersonRecord();
            if (record.School.SupplyOfficer == null) record.School.SupplyOfficer = new PersonRecord();
            if (record.School.HeadSupplyOfficer == null) record.School.HeadSupplyOfficer = new PersonRecord();
            ApplyImportedPerson(values, "school.director", record.School.Director);
            ApplyImportedPerson(values, "school.supply", record.School.SupplyOfficer);
            ApplyImportedPerson(values, "school.headSupply", record.School.HeadSupplyOfficer);

            if (values.TryGetValue("employee.prefix", out value)) record.Employee.Prefix = value;
            if (values.TryGetValue("employee.given", out value)) record.Employee.GivenName = value;
            if (values.TryGetValue("employee.surname", out value)) record.Employee.Surname = value;
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

        private sealed class PositionView
        {
            public readonly ProcurementPositionConfig Config;
            public PositionView(ProcurementPositionConfig config) { Config = config; }
            public override string ToString() { return Config == null ? PositionPlaceholder : Config.label; }
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

    internal sealed class ProcurementTemplatePickerDialog : Form
    {
        private readonly ListBox list = new ListBox();
        public SavedTemplateSnapshot Selected { get; private set; }

        public ProcurementTemplatePickerDialog(IEnumerable<SavedTemplateSnapshot> templates)
        {
            Text = "โหลด Template";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MinimizeBox = false; MaximizeBox = false; ClientSize = new Size(480, 330);
            list.Location = new Point(20, 20); list.Size = new Size(440, 240); list.DisplayMember = "Name"; list.DataSource = templates.ToList(); Controls.Add(list);
            var load = new Button { Text = "โหลด", Location = new Point(292, 280), Size = new Size(80, 30) };
            var cancel = new Button { Text = "ยกเลิก", Location = new Point(380, 280), Size = new Size(80, 30), DialogResult = DialogResult.Cancel };
            load.Click += delegate { Selected = list.SelectedItem as SavedTemplateSnapshot; if (Selected == null) { MessageBox.Show("กรุณาเลือกรายการ", "ยังไม่ได้เลือก", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; } DialogResult = DialogResult.OK; Close(); };
            Controls.Add(load); Controls.Add(cancel); AcceptButton = load; CancelButton = cancel;
        }
    }
}
