using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;

namespace ReimbursementDocApp
{
    internal static class Program
    {
        private const string WordNs = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
        private const string XmlNs = "http://www.w3.org/XML/1998/namespace";
        private static readonly Regex TagRegex = new Regex(@"\{[^{}\r\n]{1,80}\}", RegexOptions.Compiled);
        private static readonly Regex WordXmlRegex = new Regex(@"^word/(document|header\d+|footer\d+)\.xml$", RegexOptions.Compiled);
        private static readonly Regex TemplateBlockRegex = new Regex("\"([^\"]+)\"\\s*:\\s*\\[(.*?)\\]", RegexOptions.Singleline | RegexOptions.Compiled);
        private static readonly Regex JsonStringRegex = new Regex("\"([^\"]+)\"", RegexOptions.Compiled);

        private static readonly string[] ThaiMonths = { "", "\u0e21\u0e01\u0e23\u0e32\u0e04\u0e21", "\u0e01\u0e38\u0e21\u0e20\u0e32\u0e1e\u0e31\u0e19\u0e18\u0e4c", "\u0e21\u0e35\u0e19\u0e32\u0e04\u0e21", "\u0e40\u0e21\u0e29\u0e32\u0e22\u0e19", "\u0e1e\u0e24\u0e29\u0e20\u0e32\u0e04\u0e21", "\u0e21\u0e34\u0e16\u0e38\u0e19\u0e32\u0e22\u0e19", "\u0e01\u0e23\u0e01\u0e0e\u0e32\u0e04\u0e21", "\u0e2a\u0e34\u0e07\u0e2b\u0e32\u0e04\u0e21", "\u0e01\u0e31\u0e19\u0e22\u0e32\u0e22\u0e19", "\u0e15\u0e38\u0e25\u0e32\u0e04\u0e21", "\u0e1e\u0e24\u0e28\u0e08\u0e34\u0e01\u0e32\u0e22\u0e19", "\u0e18\u0e31\u0e19\u0e27\u0e32\u0e04\u0e21" };
        private static readonly string[] ThaiMonthShort = { "", "\u0e21.\u0e04.", "\u0e01.\u0e1e.", "\u0e21\u0e35.\u0e04.", "\u0e40\u0e21.\u0e22.", "\u0e1e.\u0e04.", "\u0e21\u0e34.\u0e22.", "\u0e01.\u0e04.", "\u0e2a.\u0e04.", "\u0e01.\u0e22.", "\u0e15.\u0e04.", "\u0e1e.\u0e22.", "\u0e18.\u0e04." };
        private static readonly string TagDeliveryMonth = "{\u0e40\u0e14\u0e37\u0e2d\u0e19\u0e17\u0e35\u0e48\u0e2a\u0e48\u0e07\u0e21\u0e2d\u0e1a}";
        private static readonly string TagDeliveryMonthShort = "{\u0e22\u0e48\u0e2d\u0e40\u0e14\u0e37\u0e2d\u0e19\u0e17\u0e35\u0e48\u0e2a\u0e48\u0e07\u0e21\u0e2d\u0e1a}";
        private static readonly string TagPayDate = "{\u0e27\u0e31\u0e19\u0e17\u0e35\u0e48\u0e2a\u0e48\u0e07\u0e40\u0e1a\u0e34\u0e01}";
        private static readonly string TagPurchaseOrderYear = "{\u0e1b\u0e35\u0e43\u0e1a\u0e2a\u0e31\u0e48\u0e07\u0e08\u0e49\u0e32\u0e07}";
        private static readonly string TagFiscalYear = "{\u0e1b\u0e35\u0e07\u0e1a\u0e1b\u0e23\u0e30\u0e21\u0e32\u0e13}";
        private static readonly string TagOrderDate = "{\u0e27\u0e31\u0e19\u0e17\u0e35\u0e48\u0e2a\u0e31\u0e48\u0e07\u0e08\u0e49\u0e32\u0e07}";
        private static readonly string TagHeadFinanceZone = "{\u0e42\u0e0b\u0e19\u0e2b\u0e31\u0e27\u0e2b\u0e19\u0e49\u0e32\u0e01\u0e32\u0e23\u0e40\u0e07\u0e34\u0e19}";
        private static readonly string TagHeadFinanceTitle = "{\u0e2b\u0e31\u0e27\u0e2b\u0e19\u0e49\u0e32\u0e01\u0e32\u0e23\u0e40\u0e07\u0e34\u0e19}";
        private static readonly string TagEmployeeName = "{\u0e0a\u0e37\u0e48\u0e2d\u0e25\u0e39\u0e01\u0e08\u0e49\u0e32\u0e07}";
        private static readonly string TagEmployeeHouseNo = "{\u0e1a\u0e49\u0e32\u0e19\u0e40\u0e25\u0e02\u0e17\u0e35\u0e48\u0e25\u0e39\u0e01\u0e08\u0e49\u0e32\u0e07}";
        private static readonly string TagEmployeeRoad = "{\u0e16\u0e19\u0e19\u0e25\u0e39\u0e01\u0e08\u0e49\u0e32\u0e07}";
        private static readonly string TagEmployeeSubdistrict = "{\u0e15\u0e33\u0e1a\u0e25\u0e25\u0e39\u0e01\u0e08\u0e49\u0e32\u0e07}";
        private static readonly string TagEmployeeDistrict = "{\u0e2d\u0e33\u0e40\u0e20\u0e2d\u0e25\u0e39\u0e01\u0e08\u0e49\u0e32\u0e07}";
        private static readonly string TagEmployeeProvince = "{\u0e08\u0e31\u0e07\u0e2b\u0e27\u0e31\u0e14\u0e25\u0e39\u0e01\u0e08\u0e49\u0e32\u0e07}";
        private static readonly string TagSchoolName = "{\u0e0a\u0e37\u0e48\u0e2d\u0e42\u0e23\u0e07\u0e40\u0e23\u0e35\u0e22\u0e19}";
        private static readonly string TagDistrictName = "{\u0e0a\u0e37\u0e48\u0e2d\u0e40\u0e02\u0e15}";
        private static readonly string TagSalary = "{\u0e40\u0e07\u0e34\u0e19\u0e40\u0e14\u0e37\u0e2d\u0e19}";
        private static readonly string TagSalaryText = "{\u0e40\u0e07\u0e34\u0e19\u0e40\u0e14\u0e37\u0e2d\u0e19TEXT}";
        private static readonly string TagTotalSalary = "{\u0e40\u0e07\u0e34\u0e19\u0e23\u0e27\u0e21}";
        private static readonly string TagTotalSalaryText = "{\u0e40\u0e07\u0e34\u0e19\u0e23\u0e27\u0e21TEXT}";
        private const string OptionalSpacingValue = "................................";
        private const string DistrictOfficePrefix = "สำนักงานเขตพื้นที่การศึกษา";
        private const string DistrictNamePrefix = DistrictOfficePrefix + "ประถมศึกษา";
        private const string DefaultDistrictName = DistrictNamePrefix + "แม่ฮ่องสอน เขต 2";
        private const string SavedTemplatesFileName = "saved_templates.json";

        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm(LoadTemplates()));
        }

        private static Dictionary<string, string[]> LoadTemplates()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var candidates = new[]
            {
                Path.Combine(baseDir, "template_tags.json"),
                Path.Combine(Directory.GetParent(baseDir.TrimEnd(Path.DirectorySeparatorChar)).FullName, "template_tags.json"),
                Path.Combine(Environment.CurrentDirectory, "template_tags.json")
            };
            var path = candidates.FirstOrDefault(File.Exists);
            if (path == null) throw new FileNotFoundException("template_tags.json not found. Put it next to the exe.");

            var json = File.ReadAllText(path, Encoding.UTF8);
            var result = new Dictionary<string, string[]>();
            foreach (Match block in TemplateBlockRegex.Matches(json))
            {
                var name = UnescapeJson(block.Groups[1].Value);
                var tags = JsonStringRegex.Matches(block.Groups[2].Value).Cast<Match>().Select(x => UnescapeJson(x.Groups[1].Value)).ToArray();
                result[name] = tags;
            }
            return result;
        }

        private static string UnescapeJson(string value)
        {
            return Regex.Replace(value, @"\\u([0-9a-fA-F]{4})", m => ((char)Convert.ToInt32(m.Groups[1].Value, 16)).ToString())
                .Replace("\\\"", "\"")
                .Replace("\\\\", "\\");
        }

        private sealed class MainForm : Form
        {
            private readonly Color Navy = Color.FromArgb(54, 102, 180);
            private readonly Color NavyMid = Color.FromArgb(255, 132, 168);
            private readonly Color Gold = Color.FromArgb(255, 194, 94);
            private readonly Color Bg = Color.FromArgb(248, 245, 255);
            private readonly Color Border = Color.FromArgb(218, 226, 238);
            private readonly Font UiFont = new Font("Tahoma", 10.5f, FontStyle.Regular);
            private readonly Font LabelFont = new Font("Tahoma", 10.0f, FontStyle.Bold);
            private readonly Dictionary<string, string[]> templates;
            private readonly TextBox templateBox = new TextBox();
            private readonly TextBox outputBox = new TextBox();
            private readonly ComboBox fiscalMonthBox = new ComboBox();
            private readonly NumericUpDown fiscalYearBox = new NumericUpDown();
            private readonly Label fiscalPreview = new Label();
            private readonly Label statusLabel = new Label();
            private readonly Label monthGuideLabel = new Label();
            private readonly ErrorProvider errorProvider = new ErrorProvider();
            private readonly Dictionary<string, CheckBox> templateChecks = new Dictionary<string, CheckBox>();
            private readonly Dictionary<string, Control> fieldBoxes = new Dictionary<string, Control>();
            private readonly List<string> prefixes = new List<string>();
            private readonly List<MonthOption> monthOptions = new List<MonthOption>();
            private readonly List<PositionOption> schoolPositions = new List<PositionOption>();
            private readonly List<SavedTemplateRecord> savedTemplates = new List<SavedTemplateRecord>();
            // Only the template selected by the quick-load action advances after a successful generation.
            private SavedTemplateRecord quickLoadedTemplate;
            private ComboBox positionBox;
            private TextBox salaryBox;
            private TextBox salaryTextBox;
            private readonly CheckBox hasHeadFinanceBox = new CheckBox();
            private OptionalPersonRow headFinanceZoneRow;
            private ComboBox orderDayBox;
            private ComboBox orderMonthBox;
            private ComboBox orderYearBox;
            private bool suppressMonthGuideReset;

            public MainForm(Dictionary<string, string[]> templates)
            {
                this.templates = templates;
                LoadLocalDatabase();
                Text = "\u0e2d\u0e2d\u0e01\u0e40\u0e2d\u0e01\u0e2a\u0e32\u0e23\u0e01\u0e32\u0e23\u0e40\u0e1a\u0e34\u0e01\u0e08\u0e48\u0e32\u0e22";
                Size = new Size(1120, 780);
                MinimumSize = new Size(980, 700);
                StartPosition = FormStartPosition.CenterScreen;
                BackColor = Bg;
                Font = UiFont;
                SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
                UpdateStyles();
                SuspendLayout();

                BuildHeader();
                BuildTopInputs();
                BuildFiscalControls();
                BuildTemplateList();
                BuildFields();
                BuildChecklist();
                BuildActions();
                LoadSavedTemplates();
                ApplyFiscalValues();
                NormalizeControlText(this);
                EnableDoubleBuffering(this);
                ResumeLayout(true);
            }

            private void BuildHeader()
            {
                var header = new Panel { Location = new Point(0, 0), Size = new Size(1120, 82), BackColor = Color.FromArgb(83, 151, 226), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
                Controls.Add(header);

                var title = new Label
                {
                    Text = "ระบบออกเอกสารเบิกจ่าย  ✿",
                    ForeColor = Color.White,
                    Font = new Font("Tahoma", 18f, FontStyle.Bold),
                    Location = new Point(24, 14),
                    Size = new Size(460, 30)
                };
                header.Controls.Add(title);

                var sub = new Label
                {
                    Text = "กรอกข้อมูลครั้งเดียว ตรวจสอบก่อนสร้าง แล้วออกเอกสาร Word จาก template เดิม",
                    ForeColor = Color.FromArgb(250, 252, 255),
                    Location = new Point(26, 48),
                    Size = new Size(680, 22)
                };
                header.Controls.Add(sub);

                statusLabel.Text = "พร้อมใช้งาน";
                statusLabel.TextAlign = ContentAlignment.MiddleRight;
                statusLabel.ForeColor = Color.White;
                statusLabel.Font = new Font("Tahoma", 10.5f, FontStyle.Bold);
                statusLabel.Location = new Point(760, 26);
                statusLabel.Size = new Size(330, 28);
                statusLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                header.Controls.Add(statusLabel);

                var flowers = new Label
                {
                    Text = "✿  ❀  ✿",
                    ForeColor = Color.FromArgb(255, 245, 180),
                    Font = new Font("Tahoma", 18f, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleRight,
                    Location = new Point(950, 48),
                    Size = new Size(140, 26),
                    Anchor = AnchorStyles.Top | AnchorStyles.Right
                };
                header.Controls.Add(flowers);
            }

            private void BuildTopInputs()
            {
                var panel = CreateCard(new Point(16, 96), new Size(1070, 78), "ตำแหน่งไฟล์");
                Controls.Add(panel);

                templateBox.Text = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Template");
                panel.Controls.Add(new Label { Text = "Template", Font = LabelFont, Location = new Point(16, 32), Size = new Size(80, 22) });
                panel.Controls.Add(new Label { Text = "ใช้ Template จากโฟลเดอร์ติดตั้งอัตโนมัติ", Location = new Point(100, 32), Size = new Size(330, 22), ForeColor = Color.FromArgb(80, 95, 115) });

                panel.Controls.Add(new Label { Text = "Output", Font = LabelFont, Location = new Point(456, 32), Size = new Size(70, 22) });
                outputBox.Text = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "output");
                outputBox.Location = new Point(528, 29);
                outputBox.Size = new Size(400, 24);
                panel.Controls.Add(outputBox);

                var outputButton = new Button { Text = "\u0e40\u0e25\u0e37\u0e2d\u0e01", Location = new Point(940, 27), Size = new Size(80, 28) };
                outputButton.Click += delegate { PickFolder(outputBox); };
                panel.Controls.Add(outputButton);
            }

            private void BuildFiscalControls()
            {
                var group = CreateCard(new Point(16, 188), new Size(1070, 86), "\u0e07\u0e27\u0e14\u0e2a\u0e48\u0e07\u0e21\u0e2d\u0e1a / \u0e1b\u0e35\u0e07\u0e1a\u0e1b\u0e23\u0e30\u0e21\u0e32\u0e13");
                Controls.Add(group);
                group.Controls.Add(new Label { Text = "\u0e40\u0e14\u0e37\u0e2d\u0e19\u0e2a\u0e48\u0e07\u0e21\u0e2d\u0e1a", Font = LabelFont, Location = new Point(16, 38), Size = new Size(115, 22) });

                fiscalMonthBox.DropDownStyle = ComboBoxStyle.DropDownList;
                if (monthOptions.Count > 0)
                {
                    foreach (var month in monthOptions) fiscalMonthBox.Items.Add(month.Month);
                    var currentMonthName = ThaiMonths[DateTime.Today.Month];
                    var currentMonthIndex = monthOptions.FindIndex(x => x.Month == currentMonthName);
                    fiscalMonthBox.SelectedIndex = currentMonthIndex >= 0 ? currentMonthIndex : 0;
                }
                else
                {
                    for (var i = 1; i <= 12; i++) fiscalMonthBox.Items.Add(ThaiMonths[i]);
                    fiscalMonthBox.SelectedIndex = DateTime.Today.Month - 1;
                }
                fiscalMonthBox.Location = new Point(136, 34);
                fiscalMonthBox.Size = new Size(160, 24);
                fiscalMonthBox.SelectedIndexChanged += delegate
                {
                    ApplyFiscalValues();
                    ResetMonthGuideIfNeeded();
                };
                group.Controls.Add(fiscalMonthBox);

                group.Controls.Add(new Label { Text = "\u0e1b\u0e35 \u0e1e.\u0e28.", Font = LabelFont, Location = new Point(318, 38), Size = new Size(70, 22) });
                fiscalYearBox.Minimum = 2500;
                fiscalYearBox.Maximum = 2700;
                fiscalYearBox.Value = DateTime.Today.Year + 543;
                fiscalYearBox.Location = new Point(390, 34);
                fiscalYearBox.Size = new Size(90, 24);
                fiscalYearBox.ValueChanged += delegate { ApplyFiscalValues(); };
                group.Controls.Add(fiscalYearBox);

                fiscalPreview.Location = new Point(500, 34);
                fiscalPreview.Size = new Size(400, 26);
                fiscalPreview.Font = new Font("Tahoma", 10.5f, FontStyle.Bold);
                fiscalPreview.ForeColor = Navy;
                group.Controls.Add(fiscalPreview);

                monthGuideLabel.Text = "ตรวจแค่เดือนก่อนสร้างเอกสาร";
                monthGuideLabel.Location = new Point(500, 56);
                monthGuideLabel.Size = new Size(260, 20);
                monthGuideLabel.ForeColor = Color.FromArgb(177, 92, 0);
                monthGuideLabel.Visible = false;
                group.Controls.Add(monthGuideLabel);
            }

            private void BuildTemplateList()
            {
                var group = CreateCard(new Point(16, 288), new Size(320, 320), "\u0e40\u0e2d\u0e01\u0e2a\u0e32\u0e23\u0e17\u0e35\u0e48\u0e08\u0e30\u0e2a\u0e23\u0e49\u0e32\u0e07");
                Controls.Add(group);
                var y = 28;
                foreach (var name in templates.Keys)
                {
                    var check = new CheckBox { Text = name, Checked = true, Location = new Point(12, y), Size = new Size(270, 24) };
                    group.Controls.Add(check);
                    templateChecks[name] = check;
                    y += 30;
                }
            }

            private void BuildFields()
            {
                var panel = new Panel { Location = new Point(352, 288), Size = new Size(734, 400), AutoScroll = true, BackColor = Bg, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom };
                Controls.Add(panel);
                var y = 0;

                AddDeliveryCard(panel, ref y);
                AddSignerCard(panel, ref y);
                AddCommitteeCard(panel, ref y);
            }

            private void AddPositionCard(Panel parent, ref int y)
            {
                var card = AddSection(parent, ref y, "ข้อมูลตำแหน่ง (โรงเรียน)", Color.FromArgb(255, 145, 77), 126);
                AddLabel(card, "ตำแหน่งในโรงเรียน *", 22, 42, 210);
                positionBox = AddCombo(card, 22, 66, 250);
                positionBox.Items.Add("-- เลือกตำแหน่ง --");
                foreach (var p in schoolPositions) positionBox.Items.Add(p);
                positionBox.SelectedIndex = 0;
                positionBox.SelectedIndexChanged += delegate { ApplyPositionAutoFill(); };

                AddLabel(card, "เงินเดือน", 290, 42, 120);
                salaryBox = AddText(card, TagSalary, 290, 66, 160, true);
                AddLabel(card, "คำอ่าน", 470, 42, 120);
                salaryTextBox = AddText(card, TagSalaryText, 470, 66, 220, true);
                fieldBoxes["{ตำแหน่ง}"] = positionBox;
            }

            private void AddSchoolCard(Panel parent, ref int y)
            {
                var card = AddSection(parent, ref y, "ชื่อโรงเรียน / หน่วยงาน", Color.FromArgb(244, 112, 132), 96);
                AddLabel(card, "ชื่อหน่วยงานที่ระบุในเอกสาร *", 22, 42, 240);
                AddText(card, TagSchoolName, 22, 66, 668, false);
            }

            private void AddDeliveryCard(Panel parent, ref int y)
            {
                var card = AddSection(parent, ref y, "1 · ข้อมูลการส่งมอบ ใบสั่งจ้าง และผู้รับจ้าง", Color.FromArgb(61, 128, 214), 570);
                AddLabel(card, "เดือนที่ส่งมอบ", 22, 42, 160);
                AddText(card, TagDeliveryMonth, 22, 66, 190, true);
                AddLabel(card, "งวดงาน", 230, 42, 100);
                AddText(card, TagDeliveryMonthShort, 230, 66, 90, true);
                AddLabel(card, "วันที่ส่งเบิก", 340, 42, 140);
                AddText(card, TagPayDate, 340, 66, 180, true);
                AddLabel(card, "ปีใบสั่งจ้าง", 540, 42, 140);
                AddText(card, TagPurchaseOrderYear, 540, 66, 140, true);

                AddLabel(card, "เลขที่ใบสั่งจ้าง *", 22, 108, 160);
                AddText(card, "{ใบสั่งจ้าง}", 22, 132, 180, false);
                AddLabel(card, "เช่น 25/2569", 22, 158, 160).ForeColor = Color.FromArgb(100, 110, 125);
                AddLabel(card, "ลงวันที่ในใบสั่งจ้าง *", 230, 108, 180);
                AddOrderDateInputs(card, 230, 132);

                AddLabel(card, "ชื่อโรงเรียนที่ระบุในเอกสาร *", 22, 198, 320);
                AddText(card, TagSchoolName, 22, 222, 320, false);
                AddLabel(card, "ชื่อเขตพื้นที่การศึกษา *", 370, 198, 300);
                var districtBox = AddText(card, TagDistrictName, 370, 222, 300, false);
                districtBox.Text = DefaultDistrictName;

                AddLabel(card, "ตำแหน่งในโรงเรียน *", 22, 264, 210);
                positionBox = AddCombo(card, 22, 288, 250);
                positionBox.Items.Add("-- เลือกตำแหน่ง --");
                foreach (var p in schoolPositions) positionBox.Items.Add(p);
                positionBox.SelectedIndex = 0;
                positionBox.SelectedIndexChanged += delegate { ApplyPositionAutoFill(); };
                AddLabel(card, "เงินเดือน", 290, 264, 120);
                salaryBox = AddText(card, TagSalary, 290, 288, 160, true);
                AddLabel(card, "คำอ่าน", 470, 264, 120);
                salaryTextBox = AddText(card, TagSalaryText, 470, 288, 220, true);
                fieldBoxes["{ตำแหน่ง}"] = positionBox;

                AddPersonRow(card, 22, 330, "ผู้รับจ้าง", "{คำนำหน้าลูกจ้าง}", TagEmployeeName, "{นามสกุลลูกจ้าง}");
                AddAddressFields(card, 22, 440);
            }

            private void AddSignerCard(Panel parent, ref int y)
            {
                var card = AddSection(parent, ref y, "2 · รายชื่อเจ้าหน้าที่ผู้ลงนาม", Color.FromArgb(130, 124, 204), 430);
                AddPersonRow(card, 22, 64, "ผู้อำนวยการโรงเรียน", "{คำนำหน้าผอ}", "{ชื่อผอ}", "{นามสกุลผอ}");
                AddPersonRow(card, 22, 136, "เจ้าหน้าที่พัสดุ", "{คำนำหน้าพัสดุ}", "{ชื่อพัสดุ}", "{นามสกุลพัสดุ}");
                AddPersonRow(card, 22, 208, "หัวหน้าเจ้าหน้าที่พัสดุ", "{คำนำหน้าหพัสดุ}", "{ชื่อหพัสดุ}", "{นามสกุลหพัสดุ}");
                AddPersonRow(card, 22, 280, "เจ้าหน้าที่การเงิน", "{คำนำหน้าการเงิน}", "{ชื่อการเงิน}", "{นามสกุลการเงิน}");
                hasHeadFinanceBox.Text = "มีหัวหน้าการเงิน";
                hasHeadFinanceBox.Location = new Point(22, 344);
                hasHeadFinanceBox.Size = new Size(180, 24);
                hasHeadFinanceBox.CheckedChanged += delegate { ToggleHeadFinance(); };
                card.Controls.Add(hasHeadFinanceBox);
                headFinanceZoneRow = AddOptionalCombinedPersonRow(card, 22, 374, "หัวหน้าการเงิน", TagHeadFinanceZone);
                ToggleHeadFinance();
            }

            private void AddCommitteeCard(Panel parent, ref int y)
            {
                var card = AddSection(parent, ref y, "3 · คณะกรรมการตรวจรับพัสดุ", Color.FromArgb(79, 181, 139), 260);
                AddCommitteeRow(card, 22, 64, "ประธานกรรมการ (A)", "{กรรมการA}");
                AddCommitteeRow(card, 22, 136, "กรรมการ (B)", "{กรรมการB}");
                AddCommitteeRow(card, 22, 208, "กรรมการ (C)", "{กรรมการC}");
            }

            private Panel AddSection(Panel parent, ref int y, string title, Color color, int height)
            {
                var card = new Panel { Location = new Point(0, y), Size = new Size(706, height), BackColor = Color.FromArgb(255, 255, 255), BorderStyle = BorderStyle.FixedSingle };
                parent.Controls.Add(card);
                var head = new Label { Text = title, Location = new Point(0, 0), Size = new Size(706, 36), BackColor = color, ForeColor = Color.White, Font = new Font("Tahoma", 11f, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(18, 0, 0, 0) };
                card.Controls.Add(head);
                y += height + 16;
                return card;
            }

            private void AddPersonRow(Panel card, int x, int y, string title, string prefixTag, string nameTag, string surnameTag)
            {
                if (y == 390) y -= 12;
                if (y == 136 || y == 208 || y == 280) AddRowDivider(card, y - 26);
                AddLabel(card, title, x, y - 22, 230);
                AddLabel(card, "คำนำหน้า", x, y - 4, 120);
                AddLabel(card, "ชื่อ", x + 136, y - 4, 120);
                AddLabel(card, "นามสกุล", x + 452, y - 4, 120);
                y += 18;
                AddPrefixCombo(card, prefixTag, x, y, 120);
                AddText(card, nameTag, x + 136, y, 300, false);
                AddText(card, surnameTag, x + 452, y, 216, false);
            }

            private void AddCommitteeRow(Panel card, int x, int y, string title, string tag)
            {
                if (y == 136 || y == 208) AddRowDivider(card, y - 26);
                AddLabel(card, title, x, y - 22, 220);
                AddLabel(card, "คำนำหน้า", x, y - 4, 120);
                AddLabel(card, "ชื่อ", x + 136, y - 4, 120);
                AddLabel(card, "นามสกุล", x + 452, y - 4, 120);
                AddCombinedPersonControl(card, tag, x, y + 18, 668);
            }

            private OptionalPersonRow AddOptionalCombinedPersonRow(Panel card, int x, int y, string title, string tag)
            {
                var titleLabel = AddLabel(card, title, x, y - 22, 220);
                var prefixLabel = AddLabel(card, "คำนำหน้า", x, y - 4, 120);
                var nameLabel = AddLabel(card, "ชื่อ", x + 136, y - 4, 120);
                var surnameLabel = AddLabel(card, "นามสกุล", x + 452, y - 4, 120);
                var prefix = AddPrefixCombo(card, tag + "#prefix", x, y + 18, 120);
                var name = AddText(card, tag + "#name", x + 136, y + 18, 300, false);
                var surname = AddText(card, tag + "#surname", x + 452, y + 18, 216, false);
                fieldBoxes.Remove(FixThai(tag + "#prefix"));
                fieldBoxes.Remove(FixThai(tag + "#name"));
                fieldBoxes.Remove(FixThai(tag + "#surname"));
                var combined = new CombinedPersonControl(prefix, name, surname);
                fieldBoxes[FixThai(tag)] = combined;
                return new OptionalPersonRow(titleLabel, prefixLabel, nameLabel, surnameLabel, combined);
            }

            private void AddAddressFields(Panel card, int x, int y)
            {
                AddRowDivider(card, y - 28);
                AddLabel(card, "ที่อยู่ผู้รับจ้าง (เว้นว่างได้ ระบบจะเติมเส้นจุดให้)", x, y - 22, 380);

                AddLabel(card, "บ้านเลขที่", x, y + 4, 100);
                AddText(card, TagEmployeeHouseNo, x, y + 28, 120, false);
                AddLabel(card, "ถนน", x + 140, y + 4, 80);
                AddText(card, TagEmployeeRoad, x + 140, y + 28, 160, false);
                AddLabel(card, "ตำบล", x + 320, y + 4, 80);
                AddText(card, TagEmployeeSubdistrict, x + 320, y + 28, 140, false);
                AddLabel(card, "อำเภอ", x + 480, y + 4, 80);
                AddText(card, TagEmployeeDistrict, x + 480, y + 28, 140, false);

                AddLabel(card, "จังหวัด", x, y + 72, 100);
                AddText(card, TagEmployeeProvince, x, y + 96, 200, false);
                AddLabel(card, "ปล่อยว่างได้ และถ้ากรอกไว้ ระบบจะบันทึกไว้ใช้ในครั้งถัดไปได้", x + 220, y + 98, 430).ForeColor = Color.FromArgb(100, 110, 125);
            }

            private void AddRowDivider(Control parent, int y)
            {
                var divider = new Panel { Location = new Point(22, y), Size = new Size(646, 1), BackColor = Color.FromArgb(226, 232, 240) };
                parent.Controls.Add(divider);
                divider.BringToFront();
            }

            private Label AddLabel(Control parent, string text, int x, int y, int width)
            {
                var label = new Label { Text = text, Font = LabelFont, Location = new Point(x, y), Size = new Size(width, 20), ForeColor = Color.FromArgb(30, 45, 65) };
                parent.Controls.Add(label);
                return label;
            }

            private TextBox AddText(Control parent, string tag, int x, int y, int width, bool readOnly)
            {
                var box = new TextBox { Location = new Point(x, y), Size = new Size(width, 26), ReadOnly = readOnly, BorderStyle = BorderStyle.FixedSingle };
                if (readOnly)
                {
                    box.BackColor = Color.FromArgb(238, 246, 255);
                    box.ForeColor = Navy;
                    box.Font = new Font("Tahoma", 10.5f, FontStyle.Bold);
                }
                box.TextChanged += delegate { errorProvider.SetError(box, ""); };
                parent.Controls.Add(box);
                fieldBoxes[FixThai(tag)] = box;
                return box;
            }

            private ComboBox AddCombo(Control parent, int x, int y, int width)
            {
                var box = new ComboBox { Location = new Point(x, y), Size = new Size(width, 26), DropDownStyle = ComboBoxStyle.DropDownList };
                parent.Controls.Add(box);
                return box;
            }

            private ComboBox AddPrefixCombo(Control parent, string tag, int x, int y, int width)
            {
                var box = AddCombo(parent, x, y, width);
                foreach (var prefix in GetOrderedPrefixes()) box.Items.Add(FixThai(prefix));
                if (box.Items.Count > 0) box.SelectedIndex = 0;
                box.SelectedIndexChanged += delegate { errorProvider.SetError(box, ""); };
                fieldBoxes[FixThai(tag)] = box;
                return box;
            }

            private IEnumerable<string> GetOrderedPrefixes()
            {
                var preferredOrder = new[] { "--", "นาย", "นาง", "นางสาว", "ว่าที่ร้อยตรี", "ดร.", "........" };
                var ordered = new List<string>();

                foreach (var prefix in preferredOrder)
                {
                    var match = prefixes.FirstOrDefault(x => string.Equals(x, prefix, StringComparison.Ordinal));
                    if (!string.IsNullOrWhiteSpace(match) && !ordered.Contains(match)) ordered.Add(match);
                }

                foreach (var prefix in prefixes)
                {
                    if (!ordered.Contains(prefix)) ordered.Add(prefix);
                }

                return ordered;
            }

            private void AddCombinedPersonControl(Control parent, string tag, int x, int y, int width)
            {
                var prefix = AddPrefixCombo(parent, tag + "#prefix", x, y, 120);
                var name = AddText(parent, tag + "#name", x + 136, y, 300, false);
                var surname = AddText(parent, tag + "#surname", x + 452, y, width - 452, false);
                fieldBoxes.Remove(FixThai(tag + "#prefix"));
                fieldBoxes.Remove(FixThai(tag + "#name"));
                fieldBoxes.Remove(FixThai(tag + "#surname"));
                fieldBoxes[FixThai(tag)] = new CombinedPersonControl(prefix, name, surname);
            }

            private void AddOrderDateInputs(Control parent, int x, int y)
            {
                orderDayBox = AddCombo(parent, x, y, 64);
                for (var i = 1; i <= 31; i++) orderDayBox.Items.Add(i.ToString());
                orderDayBox.SelectedIndex = -1;

                orderMonthBox = AddCombo(parent, x + 72, y, 118);
                for (var i = 1; i <= 12; i++) orderMonthBox.Items.Add(ThaiMonths[i]);
                orderMonthBox.SelectedIndex = -1;

                orderYearBox = AddCombo(parent, x + 198, y, 86);
                for (var year = 2569; year <= 2574; year++) orderYearBox.Items.Add(year.ToString());
                orderYearBox.SelectedIndex = -1;

                orderDayBox.SelectedIndexChanged += delegate { ApplyOrderDateInputs(); };
                orderMonthBox.SelectedIndexChanged += delegate { ApplyOrderDateInputs(); };
                orderYearBox.SelectedIndexChanged += delegate { ApplyOrderDateInputs(); };

                var hidden = new TextBox { Visible = false };
                parent.Controls.Add(hidden);
                fieldBoxes[TagOrderDate] = hidden;
                ApplyOrderDateInputs();
            }

            private void ApplyOrderDateInputs()
            {
                if (orderDayBox == null || orderMonthBox == null || orderYearBox == null) return;
                if (orderDayBox.SelectedItem == null || orderMonthBox.SelectedItem == null || orderYearBox.SelectedItem == null)
                {
                    SetField(TagOrderDate, "");
                    return;
                }
                var selectedYear = orderYearBox.SelectedItem.ToString();
                if (!string.Equals(selectedYear, "2569", StringComparison.Ordinal))
                {
                    SetField(TagOrderDate, "");
                    return;
                }
                SetField(TagOrderDate, orderDayBox.SelectedItem + " " + orderMonthBox.SelectedItem + " " + selectedYear);
            }

            private void ToggleHeadFinance()
            {
                if (headFinanceZoneRow == null) return;
                headFinanceZoneRow.SetVisible(hasHeadFinanceBox.Checked);
                if (!hasHeadFinanceBox.Checked) headFinanceZoneRow.Combined.Text = "";
            }

            private void ApplyPositionAutoFill()
            {
                if (positionBox == null) return;
                if (positionBox.SelectedIndex <= 0)
                {
                    return;
                }
                var match = positionBox.SelectedItem as PositionOption;
                if (match == null)
                {
                    return;
                }
                salaryBox.Text = match.Salary;
                salaryTextBox.Text = match.SalaryText;
            }

            private void LoadLocalDatabase()
            {
                prefixes.Clear();
                monthOptions.Clear();
                schoolPositions.Clear();

                var databasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app_database.json");
                if (!File.Exists(databasePath)) databasePath = Path.Combine(Environment.CurrentDirectory, "app_database.json");
                var json = File.Exists(databasePath) ? File.ReadAllText(databasePath, Encoding.UTF8) : "";

                prefixes.AddRange(LoadStringArray(json, "prefixes"));
                if (prefixes.Count == 0) prefixes.AddRange(new[] { "--", "นาย", "นาง", "นางสาว", "ว่าที่ร้อยตรี", "ดร.", "........" });
                monthOptions.AddRange(LoadMonthArray(json, "months"));
                schoolPositions.AddRange(LoadPositionArray(json, "schoolPositions"));
            }

            private List<MonthOption> LoadMonthArray(string json, string key)
            {
                var values = new List<MonthOption>();
                var section = Regex.Match(json, "\"" + key + "\"\\s*:\\s*\\[(.*?)\\]\\s*(,|})", RegexOptions.Singleline);
                if (!section.Success) return values;
                foreach (var body in ExtractJsonObjects(section.Groups[1].Value))
                {
                    values.Add(new MonthOption
                    {
                        Month = ExtractJsonValue(body, "month"),
                        PayDate = ExtractJsonValue(body, "payDate"),
                        ShortMonth = ExtractJsonValue(body, "shortMonth"),
                        OrderYear = ExtractJsonValue(body, "orderYear"),
                        PayDatesByFiscalYear = ExtractJsonMap(body, "payDatesByFiscalYear")
                    });
                }
                return values.Where(x => !string.IsNullOrWhiteSpace(x.Month)).ToList();
            }

            private List<string> LoadStringArray(string json, string key)
            {
                var values = new List<string>();
                var match = Regex.Match(json, "\"" + key + "\"\\s*:\\s*\\[(.*?)\\]", RegexOptions.Singleline);
                if (!match.Success) return values;
                foreach (Match item in JsonStringRegex.Matches(match.Groups[1].Value))
                {
                    values.Add(UnescapeJson(item.Groups[1].Value));
                }
                return values;
            }

            private List<PositionOption> LoadPositionArray(string json, string key)
            {
                var values = new List<PositionOption>();
                var section = Regex.Match(json, "\"" + key + "\"\\s*:\\s*\\[(.*?)\\]\\s*(,|})", RegexOptions.Singleline);
                if (!section.Success) return values;
                foreach (var body in ExtractJsonObjects(section.Groups[1].Value))
                {
                    values.Add(new PositionOption
                    {
                        Position = ExtractJsonValue(body, "position"),
                        Salary = ExtractJsonValue(body, "salary"),
                        SalaryText = ExtractJsonValue(body, "salaryText"),
                        TotalSalary = ExtractJsonValue(body, "totalSalary"),
                        TotalSalaryText = ExtractJsonValue(body, "totalSalaryText")
                    });
                }
                return values.Where(x => !string.IsNullOrWhiteSpace(x.Position)).ToList();
            }

            private string ExtractJsonValue(string text, string key)
            {
                var match = Regex.Match(text, "\"" + key + "\"\\s*:\\s*\"([^\"]*)\"");
                return match.Success ? UnescapeJson(match.Groups[1].Value) : "";
            }

            private Dictionary<string, string> ExtractJsonMap(string text, string key)
            {
                var result = new Dictionary<string, string>();
                var match = Regex.Match(text, "\"" + key + "\"\\s*:\\s*\\{(.*?)\\}", RegexOptions.Singleline);
                if (!match.Success) return result;
                foreach (Match pair in Regex.Matches(match.Groups[1].Value, "\"([^\"]+)\"\\s*:\\s*\"([^\"]*)\""))
                {
                    result[UnescapeJson(pair.Groups[1].Value)] = UnescapeJson(pair.Groups[2].Value);
                }
                return result;
            }

            private List<string> ExtractJsonObjects(string text)
            {
                var results = new List<string>();
                var depth = 0;
                var start = -1;

                for (var i = 0; i < text.Length; i++)
                {
                    var ch = text[i];
                    if (ch == '{')
                    {
                        if (depth == 0) start = i + 1;
                        depth++;
                    }
                    else if (ch == '}')
                    {
                        depth--;
                        if (depth == 0 && start >= 0)
                        {
                            results.Add(text.Substring(start, i - start));
                            start = -1;
                        }
                    }
                }

                return results;
            }

            private string GetSavedTemplatesPath()
            {
                return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SavedTemplatesFileName);
            }

            private void LoadSavedTemplates()
            {
                savedTemplates.Clear();
                var path = GetSavedTemplatesPath();
                if (!File.Exists(path)) return;
                var json = File.ReadAllText(path, Encoding.UTF8);
                foreach (Match templateMatch in Regex.Matches(json, "\\{\\s*\"id\"\\s*:\\s*\"([^\"]*)\"(.*?)\"data\"\\s*:\\s*\\{(.*?)\\}\\s*\\}", RegexOptions.Singleline))
                {
                    var templateJson = templateMatch.Value;
                    var record = new SavedTemplateRecord
                    {
                        Id = ExtractJsonValue(templateJson, "id"),
                        Name = ExtractJsonValue(templateJson, "name"),
                        Note = ExtractJsonValue(templateJson, "note"),
                        CreatedAt = ExtractJsonValue(templateJson, "createdAt"),
                        UpdatedAt = ExtractJsonValue(templateJson, "updatedAt"),
                        LastGeneratedFiscalMonth = ExtractJsonValue(templateJson, "lastGeneratedFiscalMonth"),
                        LastGeneratedFiscalYear = ExtractJsonValue(templateJson, "lastGeneratedFiscalYear"),
                        LastGeneratedAt = ExtractJsonValue(templateJson, "lastGeneratedAt")
                    };
                    foreach (Match pair in Regex.Matches(templateMatch.Groups[3].Value, "\"([^\"]+)\"\\s*:\\s*\"([^\"]*)\""))
                    {
                        record.Data[UnescapeJson(pair.Groups[1].Value)] = UnescapeJson(pair.Groups[2].Value);
                    }
                    if (!string.IsNullOrWhiteSpace(record.Name)) savedTemplates.Add(record);
                }
            }

            private void SaveSavedTemplates()
            {
                var path = GetSavedTemplatesPath();
                var builder = new StringBuilder();
                builder.AppendLine("{");
                builder.AppendLine("  \"version\": 1,");
                builder.AppendLine("  \"templates\": [");
                for (var i = 0; i < savedTemplates.Count; i++)
                {
                    var template = savedTemplates[i];
                    builder.AppendLine("    {");
                    builder.AppendLine("      \"id\": \"" + EscapeJson(template.Id) + "\",");
                    builder.AppendLine("      \"name\": \"" + EscapeJson(template.Name) + "\",");
                    builder.AppendLine("      \"note\": \"" + EscapeJson(template.Note) + "\",");
                    builder.AppendLine("      \"createdAt\": \"" + EscapeJson(template.CreatedAt) + "\",");
                    builder.AppendLine("      \"updatedAt\": \"" + EscapeJson(template.UpdatedAt) + "\",");
                    builder.AppendLine("      \"lastGeneratedFiscalMonth\": \"" + EscapeJson(template.LastGeneratedFiscalMonth) + "\",");
                    builder.AppendLine("      \"lastGeneratedFiscalYear\": \"" + EscapeJson(template.LastGeneratedFiscalYear) + "\",");
                    builder.AppendLine("      \"lastGeneratedAt\": \"" + EscapeJson(template.LastGeneratedAt) + "\",");
                    builder.AppendLine("      \"data\": {");
                    var pairs = template.Data.OrderBy(x => x.Key, StringComparer.Ordinal).ToArray();
                    for (var p = 0; p < pairs.Length; p++)
                    {
                        builder.Append("        \"" + EscapeJson(pairs[p].Key) + "\": \"" + EscapeJson(pairs[p].Value) + "\"");
                        builder.AppendLine(p == pairs.Length - 1 ? "" : ",");
                    }
                    builder.AppendLine("      }");
                    builder.Append("    }");
                    builder.AppendLine(i == savedTemplates.Count - 1 ? "" : ",");
                }
                builder.AppendLine("  ]");
                builder.AppendLine("}");
                File.WriteAllText(path, builder.ToString(), new UTF8Encoding(false));
            }

            private static string EscapeJson(string value)
            {
                return (value ?? "")
                    .Replace("\\", "\\\\")
                    .Replace("\"", "\\\"")
                    .Replace("\r", "\\r")
                    .Replace("\n", "\\n");
            }

            private TemplateFormData CaptureFormData()
            {
                ApplyFiscalValues();
                ApplyOrderDateInputs();
                var data = new TemplateFormData();
                foreach (var item in fieldBoxes)
                {
                    if (ShouldPersistField(item.Key)) data.Values[item.Key] = GetControlValue(item.Value);
                }
                data.FiscalMonth = fiscalMonthBox.SelectedItem == null ? "" : fiscalMonthBox.SelectedItem.ToString();
                data.FiscalYear = ((int)fiscalYearBox.Value).ToString();
                data.OrderDay = orderDayBox == null || orderDayBox.SelectedItem == null ? "" : orderDayBox.SelectedItem.ToString();
                data.OrderMonth = orderMonthBox == null || orderMonthBox.SelectedItem == null ? "" : orderMonthBox.SelectedItem.ToString();
                data.OrderYear = orderYearBox == null || orderYearBox.SelectedItem == null ? "" : orderYearBox.SelectedItem.ToString();
                data.HasHeadFinance = hasHeadFinanceBox.Checked ? "true" : "false";
                return data;
            }

            private void ApplyTemplateData(TemplateFormData data, bool focusMonthAfterLoad)
            {
                if (data == null) return;

                suppressMonthGuideReset = true;
                try
                {
                    SelectComboText(fiscalMonthBox, data.FiscalMonth);
                    int fiscalYearValue;
                    if (int.TryParse(data.FiscalYear, out fiscalYearValue) && fiscalYearValue >= fiscalYearBox.Minimum && fiscalYearValue <= fiscalYearBox.Maximum)
                    {
                        fiscalYearBox.Value = fiscalYearValue;
                    }

                    if (!SelectComboText(orderDayBox, data.OrderDay)) orderDayBox.SelectedIndex = -1;
                    if (!SelectComboText(orderMonthBox, data.OrderMonth)) orderMonthBox.SelectedIndex = -1;
                    if (!SelectComboText(orderYearBox, data.OrderYear)) orderYearBox.SelectedIndex = -1;
                    ApplyOrderDateInputs();

                    hasHeadFinanceBox.Checked = string.Equals(data.HasHeadFinance, "true", StringComparison.OrdinalIgnoreCase);
                    ToggleHeadFinance();

                    foreach (var item in data.Values)
                    {
                        if (!ShouldPersistField(item.Key)) continue;
                        ApplySavedField(item.Key, item.Value);
                    }

                    ApplyFiscalValues();
                    ApplyPositionFromSavedValue(data.Values);
                    NormalizeLiveFieldsAfterApply();
                }
                finally
                {
                    suppressMonthGuideReset = false;
                }

                if (focusMonthAfterLoad) HighlightMonthGuide();
                else ResetMonthGuide();
            }

            private void ApplySavedField(string key, string value)
            {
                Control control;
                if (!fieldBoxes.TryGetValue(key, out control)) return;

                var combo = control as ComboBox;
                if (combo != null)
                {
                    if (!SelectComboText(combo, value)) combo.SelectedIndex = combo.Items.Count > 0 ? 0 : -1;
                    return;
                }

                control.Text = value ?? "";
            }

            private void ApplyPositionFromSavedValue(Dictionary<string, string> values)
            {
                string positionValue;
                if (positionBox == null || !values.TryGetValue("{ตำแหน่ง}", out positionValue)) return;
                if (SelectPosition(positionValue))
                {
                    ApplyPositionAutoFill();
                    string salaryValue;
                    if (values.TryGetValue(TagSalary, out salaryValue) && !string.IsNullOrWhiteSpace(salaryValue)) salaryBox.Text = salaryValue;
                    string salaryTextValue;
                    if (values.TryGetValue(TagSalaryText, out salaryTextValue) && !string.IsNullOrWhiteSpace(salaryTextValue)) salaryTextBox.Text = salaryTextValue;
                }
            }

            private void NormalizeLiveFieldsAfterApply()
            {
                var schoolName = GetControlValue(fieldBoxes[TagSchoolName]);
                if (!string.IsNullOrWhiteSpace(schoolName)) SetField(TagSchoolName, NormalizeSchoolName(schoolName));
                SetField(TagDistrictName, NormalizeDistrictName(GetControlValue(fieldBoxes[TagDistrictName])));
            }

            private bool ShouldPersistField(string key)
            {
                return key != TagDeliveryMonth
                    && key != TagDeliveryMonthShort
                    && key != TagPayDate
                    && key != TagPurchaseOrderYear
                    && key != TagHeadFinanceTitle
                    && key != TagOrderDate
                    && key != TagTotalSalary
                    && key != TagTotalSalaryText;
            }

            private bool SelectPosition(string value)
            {
                if (positionBox == null) return false;
                for (var i = 0; i < positionBox.Items.Count; i++)
                {
                    var option = positionBox.Items[i] as PositionOption;
                    if (option != null && string.Equals(option.Position, value, StringComparison.Ordinal))
                    {
                        positionBox.SelectedIndex = i;
                        return true;
                    }
                }
                return false;
            }

            private bool SelectComboText(ComboBox combo, string value)
            {
                if (combo == null || combo.Items.Count == 0) return false;
                for (var i = 0; i < combo.Items.Count; i++)
                {
                    var item = combo.Items[i];
                    if (string.Equals(item == null ? "" : item.ToString(), value ?? "", StringComparison.Ordinal))
                    {
                        combo.SelectedIndex = i;
                        return true;
                    }
                }
                return false;
            }

            private void SaveTemplateFlow()
            {
                using (var modeDialog = new TemplateSaveModeDialog())
                {
                    if (modeDialog.ShowDialog(this) != DialogResult.OK) return;
                    if (modeDialog.SaveMode == TemplateSaveMode.SaveNew)
                    {
                        SaveTemplateAsNew();
                    }
                    else
                    {
                        OverwriteExistingTemplate();
                    }
                }
            }

            private void SaveTemplateAsNew()
            {
                using (var editor = new TemplateEditorDialog("บันทึกเป็น Template", "", ""))
                {
                    if (editor.ShowDialog(this) != DialogResult.OK) return;
                    if (savedTemplates.Any(x => string.Equals(x.Name, editor.TemplateName, StringComparison.OrdinalIgnoreCase)))
                    {
                        MessageBox.Show("มีชื่อ Template นี้แล้ว กรุณาใช้ชื่ออื่น", "ชื่อซ้ำ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    var now = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
                    var data = CaptureFormData();
                    savedTemplates.Add(new SavedTemplateRecord
                    {
                        Id = Guid.NewGuid().ToString("N"),
                        Name = editor.TemplateName,
                        Note = editor.TemplateNote,
                        CreatedAt = now,
                        UpdatedAt = now,
                        LastGeneratedFiscalMonth = data.FiscalMonth,
                        LastGeneratedFiscalYear = data.FiscalYear,
                        Data = data.ToDictionary()
                    });
                    SaveSavedTemplates();
                    MessageBox.Show("บันทึก Template แล้ว", "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }

            private void OverwriteExistingTemplate()
            {
                if (!savedTemplates.Any())
                {
                    MessageBox.Show("ยังไม่มี Template ให้บันทึกทับ", "ยังไม่มี Template", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using (var picker = new TemplatePickerDialog(savedTemplates, "เลือก Template ที่ต้องการบันทึกทับ", true))
                {
                    if (picker.ShowDialog(this) != DialogResult.OK || picker.SelectedTemplate == null) return;
                    if (MessageBox.Show("ยืนยันการบันทึกทับ Template \"" + picker.SelectedTemplate.Name + "\" ?", "ยืนยัน", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

                    picker.SelectedTemplate.Note = picker.SelectedTemplate.Note;
                    picker.SelectedTemplate.UpdatedAt = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
                    picker.SelectedTemplate.Data = CaptureFormData().ToDictionary();
                    SaveSavedTemplates();
                    MessageBox.Show("บันทึกทับ Template แล้ว", "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }

            private void QuickLoadTemplateFlow()
            {
                if (!savedTemplates.Any())
                {
                    MessageBox.Show("ยังไม่มี Template ที่บันทึกไว้", "ยังไม่มี Template", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using (var picker = new TemplatePickerDialog(savedTemplates, "เลือก Template เดิม", false))
                {
                    if (picker.ShowDialog(this) != DialogResult.OK || picker.SelectedTemplate == null) return;

                    string nextMonth;
                    int nextYear;
                    GetNextPeriodForQuickLoad(picker.SelectedTemplate, out nextMonth, out nextYear);
                    using (var confirmation = new QuickLoadConfirmationDialog(nextMonth, nextYear))
                    {
                        if (confirmation.ShowDialog(this) != DialogResult.OK) return;
                    }

                    ApplyTemplateData(TemplateFormData.FromDictionary(picker.SelectedTemplate.Data), true);
                    ApplyLastGeneratedPeriodForQuickLoad(picker.SelectedTemplate);
                    quickLoadedTemplate = picker.SelectedTemplate;
                    AdvanceMonthForQuickLoad();
                    var payDateMissing = string.IsNullOrWhiteSpace(GetControlValue(fieldBoxes[TagPayDate]));
                    MessageBox.Show(
                        payDateMissing
                            ? "โหลด Template แล้ว เลื่อนเดือนไปเดือนถัดไปให้แล้ว แต่ยังไม่มีวันที่ส่งเบิกของปีงบนี้ในฐานข้อมูล กรุณาตรวจเอง"
                            : "โหลด Template แล้ว เลื่อนเดือนไปเดือนถัดไปให้แล้ว กรุณาตรวจเดือนก่อนสร้างเอกสาร",
                        "พร้อมแก้ไข",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }

            private void OpenTemplateManager()
            {
                quickLoadedTemplate = null;
                using (var manager = new TemplateManagerDialog(savedTemplates))
                {
                    var result = manager.ShowDialog(this);
                    if (manager.IsDirty) SaveSavedTemplates();
                    if (result == DialogResult.OK && manager.SelectedTemplate != null)
                    {
                        ApplyTemplateData(TemplateFormData.FromDictionary(manager.SelectedTemplate.Data), false);
                        MessageBox.Show("โหลด Template แล้ว", "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }

            private void HighlightMonthGuide()
            {
                monthGuideLabel.Visible = true;
                fiscalMonthBox.BackColor = Color.FromArgb(255, 248, 204);
                fiscalMonthBox.Focus();
            }

            private void ResetMonthGuide()
            {
                monthGuideLabel.Visible = false;
                fiscalMonthBox.BackColor = SystemColors.Window;
            }

            private void ResetMonthGuideIfNeeded()
            {
                if (suppressMonthGuideReset) return;
                ResetMonthGuide();
            }

            private void AdvanceMonthForQuickLoad()
            {
                if (fiscalMonthBox.Items.Count == 0 || fiscalMonthBox.SelectedIndex < 0) return;

                var currentMonthName = fiscalMonthBox.SelectedItem == null ? "" : fiscalMonthBox.SelectedItem.ToString();
                var currentMonthNumber = GetMonthNumber(currentMonthName);
                if (currentMonthNumber == 0) return;

                var nextMonthNumber = currentMonthNumber == 12 ? 1 : currentMonthNumber + 1;
                var nextYearValue = (int)fiscalYearBox.Value + (currentMonthNumber == 12 ? 1 : 0);

                suppressMonthGuideReset = true;
                try
                {
                    if (nextYearValue >= fiscalYearBox.Minimum && nextYearValue <= fiscalYearBox.Maximum)
                    {
                        fiscalYearBox.Value = nextYearValue;
                    }
                    SelectComboText(fiscalMonthBox, ThaiMonths[nextMonthNumber]);
                    ApplyFiscalValues();
                }
                finally
                {
                    suppressMonthGuideReset = false;
                }

                HighlightMonthGuide();
            }

            private void ApplyLastGeneratedPeriodForQuickLoad(SavedTemplateRecord template)
            {
                if (template == null) return;

                // Old templates do not yet have this metadata. Their saved period is the safe migration fallback.
                var month = template.LastGeneratedFiscalMonth;
                var year = template.LastGeneratedFiscalYear;
                if (string.IsNullOrWhiteSpace(month) || string.IsNullOrWhiteSpace(year))
                {
                    var savedData = TemplateFormData.FromDictionary(template.Data);
                    month = savedData.FiscalMonth;
                    year = savedData.FiscalYear;
                }

                suppressMonthGuideReset = true;
                try
                {
                    SelectComboText(fiscalMonthBox, month);
                    int yearValue;
                    if (int.TryParse(year, out yearValue) && yearValue >= fiscalYearBox.Minimum && yearValue <= fiscalYearBox.Maximum)
                    {
                        fiscalYearBox.Value = yearValue;
                    }
                    ApplyFiscalValues();
                }
                finally
                {
                    suppressMonthGuideReset = false;
                }
            }

            private void GetNextPeriodForQuickLoad(SavedTemplateRecord template, out string nextMonth, out int nextYear)
            {
                var month = template == null ? "" : template.LastGeneratedFiscalMonth;
                var year = template == null ? "" : template.LastGeneratedFiscalYear;
                if (string.IsNullOrWhiteSpace(month) || string.IsNullOrWhiteSpace(year))
                {
                    var savedData = TemplateFormData.FromDictionary(template.Data);
                    month = savedData.FiscalMonth;
                    year = savedData.FiscalYear;
                }

                var monthNumber = GetMonthNumber(month);
                if (monthNumber == 0) monthNumber = DateTime.Today.Month;
                int parsedYear;
                if (!int.TryParse(year, out parsedYear)) parsedYear = DateTime.Today.Year + 543;

                var nextMonthNumber = monthNumber == 12 ? 1 : monthNumber + 1;
                nextMonth = ThaiMonths[nextMonthNumber];
                nextYear = parsedYear + (monthNumber == 12 ? 1 : 0);
            }

            private void UpdateQuickLoadedTemplatePeriodAfterGenerate()
            {
                if (quickLoadedTemplate == null) return;

                var month = fiscalMonthBox.SelectedItem == null ? "" : fiscalMonthBox.SelectedItem.ToString();
                var year = (int)fiscalYearBox.Value;
                var savedMonthNumber = GetMonthNumber(quickLoadedTemplate.LastGeneratedFiscalMonth);
                int savedYear;
                int.TryParse(quickLoadedTemplate.LastGeneratedFiscalYear, out savedYear);
                var currentMonthNumber = GetMonthNumber(month);
                if (savedMonthNumber > 0 && savedYear > 0 && currentMonthNumber > 0
                    && (year < savedYear || (year == savedYear && currentMonthNumber < savedMonthNumber))) return;

                quickLoadedTemplate.LastGeneratedFiscalMonth = month;
                quickLoadedTemplate.LastGeneratedFiscalYear = year.ToString();
                quickLoadedTemplate.LastGeneratedAt = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
                SaveSavedTemplates();
            }

            private void BuildChecklist()
            {
                var panel = CreateCard(new Point(16, 622), new Size(320, 116), "\u0e40\u0e15\u0e37\u0e2d\u0e19\u0e01\u0e48\u0e2d\u0e19\u0e2a\u0e23\u0e49\u0e32\u0e07");
                Controls.Add(panel);
                var text = new Label
                {
                    Text = "\u2022 \u0e15\u0e23\u0e27\u0e08\u0e40\u0e14\u0e37\u0e2d\u0e19/\u0e1b\u0e35\u0e07\u0e1a\u0e1b\u0e23\u0e30\u0e21\u0e32\u0e13\u0e01\u0e48\u0e2d\u0e19\u0e2a\u0e23\u0e49\u0e32\u0e07\n\u2022 \u0e1b\u0e35\u0e43\u0e1a\u0e2a\u0e31\u0e48\u0e07\u0e08\u0e49\u0e32\u0e07\u0e2d\u0e34\u0e07\u0e1b\u0e35\u0e07\u0e1a\u0e1b\u0e23\u0e30\u0e21\u0e32\u0e13\n\u2022 \u0e2b\u0e32\u0e01\u0e41\u0e01\u0e49 template \u0e43\u0e2b\u0e49\u0e1b\u0e34\u0e14 Word \u0e01\u0e48\u0e2d\u0e19 generate",
                    Location = new Point(16, 30),
                    Size = new Size(285, 72),
                    ForeColor = Color.FromArgb(100, 72, 0)
                };
                panel.Controls.Add(text);
            }

            private void BuildActions()
            {
                var manageButton = new Button { Text = "จัดการ Template", Location = new Point(352, 700), Size = new Size(160, 44), BackColor = Color.White, ForeColor = Navy, FlatStyle = FlatStyle.Flat, Font = new Font("Tahoma", 10.5f, FontStyle.Bold) };
                manageButton.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
                manageButton.FlatAppearance.BorderColor = Border;
                manageButton.Click += delegate { OpenTemplateManager(); };
                Controls.Add(manageButton);

                var saveButton = new Button { Text = "บันทึกเป็น Template", Location = new Point(526, 700), Size = new Size(180, 44), BackColor = Gold, ForeColor = Color.FromArgb(65, 40, 0), FlatStyle = FlatStyle.Flat, Font = new Font("Tahoma", 10.5f, FontStyle.Bold) };
                saveButton.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
                saveButton.FlatAppearance.BorderSize = 0;
                saveButton.Click += delegate { SaveTemplateFlow(); };
                Controls.Add(saveButton);

                var quickLoadButton = new Button { Text = "เหมือนเดิม! แค่เปลี่ยนเดือน!", Location = new Point(720, 700), Size = new Size(220, 44), BackColor = Color.FromArgb(255, 237, 181), ForeColor = Color.FromArgb(100, 64, 0), FlatStyle = FlatStyle.Flat, Font = new Font("Tahoma", 10.5f, FontStyle.Bold) };
                quickLoadButton.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
                quickLoadButton.FlatAppearance.BorderSize = 0;
                quickLoadButton.Click += delegate { QuickLoadTemplateFlow(); };
                Controls.Add(quickLoadButton);

                var button = new Button { Text = "\u0e15\u0e23\u0e27\u0e08\u0e2a\u0e2d\u0e1a\u0e41\u0e25\u0e49\u0e27\u0e2a\u0e23\u0e49\u0e32\u0e07 Word", Location = new Point(950, 700), Size = new Size(136, 44), BackColor = NavyMid, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Tahoma", 10.5f, FontStyle.Bold) };
                button.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
                button.FlatAppearance.BorderSize = 0;
                button.Click += delegate { GenerateDocuments(); };
                Controls.Add(button);
            }

            private void ApplyFiscalValues()
            {
                if (fiscalMonthBox.SelectedIndex < 0) return;
                var selectedMonth = fiscalMonthBox.SelectedItem == null ? "" : fiscalMonthBox.SelectedItem.ToString();
                var month = GetMonthNumber(selectedMonth);
                if (month == 0) month = fiscalMonthBox.SelectedIndex + 1;
                var buddhistYear = (int)fiscalYearBox.Value;
                var fiscalYear = month >= 10 ? buddhistYear + 1 : buddhistYear;
                var period = month >= 10 ? month - 9 : month + 3;
                var dbMonth = monthOptions.FirstOrDefault(x => x.Month == selectedMonth);
                SetField(TagDeliveryMonth, string.IsNullOrWhiteSpace(selectedMonth) ? ThaiMonths[month] : selectedMonth);
                SetField(TagDeliveryMonthShort, dbMonth == null || string.IsNullOrWhiteSpace(dbMonth.ShortMonth) ? ThaiMonthShort[month] : dbMonth.ShortMonth);
                SetField(TagPayDate, GetPayDateForFiscalYear(dbMonth, fiscalYear));
                SetField(TagPurchaseOrderYear, fiscalYear.ToString());
                fiscalPreview.Text = "\u0e07\u0e27\u0e14\u0e17\u0e35\u0e48 " + period + " / \u0e1b\u0e35\u0e07\u0e1a\u0e1b\u0e23\u0e30\u0e21\u0e32\u0e13 " + fiscalYear;
                statusLabel.Text = "\u0e07\u0e27\u0e14 " + period + " | FY " + fiscalYear;
            }

            private string GetPayDateForFiscalYear(MonthOption month, int fiscalYear)
            {
                if (month == null) return "";

                string payDate;
                if (month.PayDatesByFiscalYear != null && month.PayDatesByFiscalYear.TryGetValue(fiscalYear.ToString(), out payDate))
                {
                    return payDate;
                }

                return month.PayDate ?? "";
            }

            private int GetMonthNumber(string monthName)
            {
                for (var i = 1; i < ThaiMonths.Length; i++)
                {
                    if (ThaiMonths[i] == monthName) return i;
                }
                return 0;
            }

            private void SetField(string tag, string value)
            {
                Control box;
                if (fieldBoxes.TryGetValue(tag, out box)) box.Text = value;
            }

            private static void PickFolder(TextBox box)
            {
                using (var dialog = new FolderBrowserDialog { SelectedPath = box.Text })
                {
                    if (dialog.ShowDialog() == DialogResult.OK) box.Text = dialog.SelectedPath;
                }
            }

            private void EnableDoubleBuffering(Control root)
            {
                var prop = typeof(Control).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (prop != null) prop.SetValue(root, true, null);
                foreach (Control child in root.Controls) EnableDoubleBuffering(child);
            }

            private void GenerateDocuments()
            {
                ApplyFiscalValues();
                errorProvider.Clear();
                var values = fieldBoxes.ToDictionary(x => x.Key, x => GetControlValue(x.Value));
                NormalizeValues(values);
                var missingFields = fieldBoxes.Where(x => IsRequiredField(x.Key) && IsEmptyControl(x.Value)).Take(8).ToArray();
                var missing = missingFields.Select(x => x.Key.Trim('{', '}')).ToArray();
                if (missing.Length > 0)
                {
                    foreach (var item in missingFields) errorProvider.SetError(item.Value, "\u0e08\u0e33\u0e40\u0e1b\u0e47\u0e19\u0e15\u0e49\u0e2d\u0e07\u0e01\u0e23\u0e2d\u0e01");
                    MessageBox.Show("\u0e01\u0e23\u0e38\u0e13\u0e32\u0e01\u0e23\u0e2d\u0e01: " + string.Join(", ", missing), "\u0e02\u0e49\u0e2d\u0e21\u0e39\u0e25\u0e22\u0e31\u0e07\u0e44\u0e21\u0e48\u0e04\u0e23\u0e1a");
                    return;
                }
                if (!Directory.Exists(templateBox.Text))
                {
                    MessageBox.Show("\u0e44\u0e21\u0e48\u0e1e\u0e1a\u0e42\u0e1f\u0e25\u0e40\u0e14\u0e2d\u0e23\u0e4c Template\n" + templateBox.Text, "Template");
                    return;
                }
                if (!templateChecks.Values.Any(x => x.Checked))
                {
                    MessageBox.Show("\u0e01\u0e23\u0e38\u0e13\u0e32\u0e40\u0e25\u0e37\u0e2d\u0e01\u0e40\u0e2d\u0e01\u0e2a\u0e32\u0e23\u0e2d\u0e22\u0e48\u0e32\u0e07\u0e19\u0e49\u0e2d\u0e22 1 \u0e23\u0e32\u0e22\u0e01\u0e32\u0e23", "\u0e22\u0e31\u0e07\u0e44\u0e21\u0e48\u0e44\u0e14\u0e49\u0e40\u0e25\u0e37\u0e2d\u0e01\u0e40\u0e2d\u0e01\u0e2a\u0e32\u0e23");
                    return;
                }

                var confirm = MessageBox.Show(BuildPreview(values), "\u0e15\u0e23\u0e27\u0e08\u0e17\u0e32\u0e19\u0e01\u0e48\u0e2d\u0e19\u0e2a\u0e23\u0e49\u0e32\u0e07", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                if (confirm != DialogResult.OK) return;

                var created = 0;
                Cursor.Current = Cursors.WaitCursor;
                statusLabel.Text = "\u0e01\u0e33\u0e25\u0e31\u0e07\u0e2a\u0e23\u0e49\u0e32\u0e07...";
                try
                {
                    foreach (var item in templateChecks)
                    {
                        if (!item.Value.Checked) continue;
                        var templatePath = Path.Combine(templateBox.Text, item.Key);
                        if (!File.Exists(templatePath))
                        {
                            MessageBox.Show("\u0e44\u0e21\u0e48\u0e1e\u0e1a Template\n" + templatePath, "Error");
                            return;
                        }
                        RenderDocx(templatePath, Path.Combine(outputBox.Text, BuildOutputFileName(item.Key, values)), values);
                        created++;
                    }
                }
                finally
                {
                    Cursor.Current = Cursors.Default;
                    statusLabel.Text = "\u0e2a\u0e23\u0e49\u0e32\u0e07\u0e40\u0e2a\u0e23\u0e47\u0e08 " + created + " \u0e44\u0e1f\u0e25\u0e4c";
                }
                if (created > 0) UpdateQuickLoadedTemplatePeriodAfterGenerate();
                var openOutput = MessageBox.Show(
                    "\u0e2a\u0e23\u0e49\u0e32\u0e07\u0e40\u0e2d\u0e01\u0e2a\u0e32\u0e23 " + created + " \u0e44\u0e1f\u0e25\u0e4c\u0e41\u0e25\u0e49\u0e27\n" + outputBox.Text + "\n\n\u0e15\u0e49\u0e2d\u0e07\u0e01\u0e32\u0e23\u0e40\u0e1b\u0e34\u0e14\u0e42\u0e1f\u0e25\u0e40\u0e14\u0e2d\u0e23\u0e4c output \u0e15\u0e2d\u0e19\u0e19\u0e35\u0e49\u0e2b\u0e23\u0e37\u0e2d\u0e44\u0e21\u0e48?",
                    "\u0e2a\u0e33\u0e40\u0e23\u0e47\u0e08",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information);
                if (openOutput == DialogResult.Yes)
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = outputBox.Text,
                        UseShellExecute = true
                    });
                }
            }

            private Panel CreateCard(Point location, Size size, string title)
            {
                var panel = new Panel { Location = location, Size = size, BackColor = Color.FromArgb(255, 255, 255), BorderStyle = BorderStyle.FixedSingle };
                panel.Controls.Add(new Label { Text = FixThai(title), Font = new Font("Tahoma", 10.5f, FontStyle.Bold), ForeColor = Navy, Location = new Point(12, 6), Size = new Size(size.Width - 24, 22) });
                return panel;
            }

            private void NormalizeControlText(Control root)
            {
                root.Text = FixThai(root.Text);
                foreach (Control child in root.Controls)
                {
                    child.Text = FixThai(child.Text);
                    NormalizeControlText(child);
                }
            }

            private string FixThai(string value)
            {
                if (string.IsNullOrEmpty(value) || !LooksMojibake(value)) return value;
                try
                {
                    return Encoding.UTF8.GetString(Encoding.GetEncoding(874).GetBytes(value));
                }
                catch
                {
                    return value;
                }
            }

            private bool LooksMojibake(string value)
            {
                return value.IndexOf("เธ", StringComparison.Ordinal) >= 0 || value.IndexOf("เน", StringComparison.Ordinal) >= 0;
            }

            private TabPage CreateTab(string title)
            {
                var page = new TabPage(title) { BackColor = Color.White };
                page.Controls.Add(new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.White });
                return page;
            }

            private TabPage PickFieldPage(string tag, TabPage main, TabPage people, TabPage money, TabPage other)
            {
                if (tag.Contains("\u0e40\u0e07\u0e34\u0e19") || tag.Contains("\u0e43\u0e1a\u0e2a\u0e31\u0e48\u0e07") || tag.Contains("\u0e27\u0e31\u0e19\u0e17\u0e35\u0e48")) return money;
                if (tag.Contains("\u0e0a\u0e37\u0e48\u0e2d") || tag.Contains("\u0e19\u0e32\u0e21\u0e2a\u0e01\u0e38\u0e25") || tag.Contains("\u0e04\u0e33\u0e19\u0e33\u0e2b\u0e19\u0e49\u0e32") || tag.Contains("\u0e01\u0e23\u0e23\u0e21\u0e01\u0e32\u0e23")) return people;
                if (tag == TagDeliveryMonth || tag == TagDeliveryMonthShort || tag.Contains("\u0e42\u0e23\u0e07\u0e40\u0e23\u0e35\u0e22\u0e19") || tag.Contains("\u0e15\u0e33\u0e41\u0e2b\u0e19\u0e48\u0e07")) return main;
                return other;
            }

            private string BuildPreview(Dictionary<string, string> values)
            {
                var selected = templateChecks.Count(x => x.Value.Checked);
                return "\u0e01\u0e33\u0e25\u0e31\u0e07\u0e08\u0e30\u0e2a\u0e23\u0e49\u0e32\u0e07 " + selected + " \u0e40\u0e2d\u0e01\u0e2a\u0e32\u0e23\n\n"
                    + "\u0e40\u0e14\u0e37\u0e2d\u0e19\u0e2a\u0e48\u0e07\u0e21\u0e2d\u0e1a: " + SafeValue(values, TagDeliveryMonth) + "\n"
                    + "\u0e1b\u0e35\u0e43\u0e1a\u0e2a\u0e31\u0e48\u0e07\u0e08\u0e49\u0e32\u0e07/FY: " + SafeValue(values, TagPurchaseOrderYear) + "\n"
                    + "\u0e42\u0e1f\u0e25\u0e40\u0e14\u0e2d\u0e23\u0e4c\u0e1b\u0e25\u0e32\u0e22\u0e17\u0e32\u0e07: " + outputBox.Text + "\n\n"
                    + "\u0e01\u0e14 OK \u0e40\u0e1e\u0e37\u0e48\u0e2d\u0e2a\u0e23\u0e49\u0e32\u0e07 \u0e2b\u0e23\u0e37\u0e2d Cancel \u0e40\u0e1e\u0e37\u0e48\u0e2d\u0e01\u0e25\u0e31\u0e1a\u0e44\u0e1b\u0e41\u0e01\u0e49\u0e44\u0e02";
            }

            private string SafeValue(Dictionary<string, string> values, string tag)
            {
                string value;
                return values.TryGetValue(tag, out value) ? value : "-";
            }

            private bool IsRequiredField(string tag)
            {
                return tag != TagHeadFinanceZone
                    && tag != TagEmployeeHouseNo
                    && tag != TagEmployeeRoad
                    && tag != TagEmployeeSubdistrict
                    && tag != TagEmployeeDistrict
                    && tag != TagEmployeeProvince;
            }

            private string GetControlValue(Control control)
            {
                var combo = control as ComboBox;
                if (combo != null)
                {
                    var position = combo.SelectedItem as PositionOption;
                    if (position != null) return position.Position;
                }
                var value = control.Text == null ? "" : control.Text.Trim();
                return value == "--" || value.StartsWith("-- ") ? "" : value;
            }

            private bool IsEmptyControl(Control control)
            {
                return string.IsNullOrWhiteSpace(GetControlValue(control));
            }

            private void NormalizeValues(Dictionary<string, string> values)
            {
                if (values.ContainsKey(TagHeadFinanceZone) && string.IsNullOrWhiteSpace(values[TagHeadFinanceZone]))
                {
                    values[TagHeadFinanceZone] = "";
                }
                values[TagHeadFinanceTitle] = string.IsNullOrWhiteSpace(SafeValue(values, TagHeadFinanceZone))
                    ? ""
                    : "\u0e2b\u0e31\u0e27\u0e2b\u0e19\u0e49\u0e32\u0e40\u0e08\u0e49\u0e32\u0e2b\u0e19\u0e49\u0e32\u0e17\u0e35\u0e48\u0e01\u0e32\u0e23\u0e40\u0e07\u0e34\u0e19";
                if (values.ContainsKey(TagSchoolName))
                {
                    values[TagSchoolName] = NormalizeSchoolName(values[TagSchoolName]);
                    SetField(TagSchoolName, values[TagSchoolName]);
                }
                if (values.ContainsKey(TagDistrictName))
                {
                    values[TagDistrictName] = NormalizeDistrictName(values[TagDistrictName]);
                    SetField(TagDistrictName, values[TagDistrictName]);
                }
                if (values.ContainsKey(TagOrderDate))
                {
                    values[TagOrderDate] = SmartFormatThaiDate(values[TagOrderDate]);
                    SetField(TagOrderDate, values[TagOrderDate]);
                }
                values[TagTotalSalary] = "";
                values[TagTotalSalaryText] = "";
                ApplyOptionalSpacing(values, TagEmployeeHouseNo);
                ApplyOptionalSpacing(values, TagEmployeeRoad);
                ApplyOptionalSpacing(values, TagEmployeeSubdistrict);
                ApplyOptionalSpacing(values, TagEmployeeDistrict);
                ApplyOptionalSpacing(values, TagEmployeeProvince);
                NormalizePrefixSpacing(values);
                AddTemplateAliases(values);
            }

            private void NormalizePrefixSpacing(Dictionary<string, string> values)
            {
                var prefixTags = new[]
                {
                    "{คำนำหน้าผอ}", "{คำนำหน้าพัสดุ}", "{คำนำหน้าหพัสดุ}",
                    "{คำนำหน้าการเงิน}", "{คำนำหน้าลูกจ้าง}"
                };
                foreach (var tag in prefixTags)
                {
                    string prefix;
                    if (!values.TryGetValue(tag, out prefix)) continue;
                    prefix = (prefix ?? "").Trim();
                    if (prefix.Contains("ร้อยตรี") || prefix == "ดร.") values[tag] = prefix + " ";
                }
            }

            private void ApplyOptionalSpacing(Dictionary<string, string> values, string tag)
            {
                if (!values.ContainsKey(tag) || string.IsNullOrWhiteSpace(values[tag]))
                {
                    values[tag] = OptionalSpacingValue;
                }
            }

            private void AddTemplateAliases(Dictionary<string, string> values)
            {
            values["{คำนำหน้าชื่อ}"] = SafeValue(values, "{คำนำหน้าลูกจ้าง}");
                values["{กรรมการ B}"] = SafeValue(values, "{กรรมการB}");
                values["{กรรมการ C}"] = SafeValue(values, "{กรรมการC}");
                values[TagFiscalYear] = SafeValue(values, TagPurchaseOrderYear);
            }

        private string NormalizeSchoolName(string input)
        {
            var value = (input ?? "").Trim();
            if (value.Length == 0) return "";
            while (value.StartsWith("โรงเรียนโรงเรียน", StringComparison.Ordinal))
                {
                    value = value.Substring("โรงเรียน".Length).TrimStart();
                }
                if (!value.StartsWith("โรงเรียน", StringComparison.Ordinal)) value = "โรงเรียน" + value;
                return value;
            }

        private static string NormalizeDistrictName(string input)
        {
            var value = Regex.Replace((input ?? "").Trim(), @"\s+", " ");
            if (string.IsNullOrWhiteSpace(value)) return DefaultDistrictName;
            if (value.StartsWith(DistrictOfficePrefix, StringComparison.Ordinal))
            {
                var remainder = value.Substring(DistrictOfficePrefix.Length).TrimStart();
                var educationPrefix = remainder.StartsWith("มัธยมศึกษา", StringComparison.Ordinal) ? "มัธยมศึกษา" : "ประถมศึกษา";
                if (remainder.StartsWith(educationPrefix, StringComparison.Ordinal)) remainder = remainder.Substring(educationPrefix.Length).TrimStart();
                return DistrictOfficePrefix + educationPrefix + remainder;
            }
            if (value.StartsWith("มัธยมศึกษา", StringComparison.Ordinal)) return DistrictOfficePrefix + value;
            if (value.StartsWith("ประถมศึกษา", StringComparison.Ordinal)) return DistrictOfficePrefix + value;
            return DistrictNamePrefix + value;
        }

            private string BuildOutputFileName(string templateName, Dictionary<string, string> values)
            {
                var documentName = SafeFilePart(Path.GetFileNameWithoutExtension(templateName));
                var employeeName = SafeFilePart(SafeValue(values, TagEmployeeName));
                var datePart = BuildOutputDatePart();
                return employeeName + "_" + documentName + "_" + datePart + ".docx";
            }

            private string SafeFilePart(string value)
            {
                var safe = string.IsNullOrWhiteSpace(value) ? "-" : value.Trim();
                foreach (var c in Path.GetInvalidFileNameChars()) safe = safe.Replace(c, '_');
                return safe.Length > 80 ? safe.Substring(0, 80) : safe;
            }

            private string SmartFormatThaiDate(string input)
            {
                if (string.IsNullOrWhiteSpace(input)) return "";
                DateTime date;
                var value = input.Trim();
                var formats = new[] { "d/M/yyyy", "dd/MM/yyyy", "yyyy-MM-dd", "d-M-yyyy", "dd-MM-yyyy" };
                if (!DateTime.TryParseExact(value, formats, null, System.Globalization.DateTimeStyles.None, out date)
                    && !DateTime.TryParse(value, out date))
                {
                    return value;
                }
                var year = date.Year < 2450 ? date.Year + 543 : date.Year;
                return date.Day + " " + ThaiMonths[date.Month] + " " + year;
            }

            private string FormatThaiDate(DateTime date)
            {
                return date.Day + " " + ThaiMonths[date.Month] + " " + (date.Year + 543);
            }

            private string BuildOutputDatePart()
            {
                var now = DateTime.Now;
                var buddhistYear = now.Year + 543;
                return now.ToString("ddMM") + (buddhistYear % 100).ToString("00");
            }
        }

        private enum TemplateSaveMode
        {
            SaveNew,
            Overwrite
        }

        private sealed class TemplateFormData
        {
            public readonly Dictionary<string, string> Values = new Dictionary<string, string>();
            public string FiscalMonth = "";
            public string FiscalYear = "";
            public string OrderDay = "";
            public string OrderMonth = "";
            public string OrderYear = "";
            public string HasHeadFinance = "false";

            public Dictionary<string, string> ToDictionary()
            {
                var data = new Dictionary<string, string>(Values);
                data["__fiscalMonth"] = FiscalMonth ?? "";
                data["__fiscalYear"] = FiscalYear ?? "";
                data["__orderDay"] = OrderDay ?? "";
                data["__orderMonth"] = OrderMonth ?? "";
                data["__orderYear"] = OrderYear ?? "";
                data["__hasHeadFinance"] = HasHeadFinance ?? "false";
                return data;
            }

            public static TemplateFormData FromDictionary(Dictionary<string, string> data)
            {
                var result = new TemplateFormData();
                if (data == null) return result;
                foreach (var item in data)
                {
                    switch (item.Key)
                    {
                        case "__fiscalMonth": result.FiscalMonth = item.Value; break;
                        case "__fiscalYear": result.FiscalYear = item.Value; break;
                        case "__orderDay": result.OrderDay = item.Value; break;
                        case "__orderMonth": result.OrderMonth = item.Value; break;
                        case "__orderYear": result.OrderYear = item.Value; break;
                        case "__hasHeadFinance": result.HasHeadFinance = item.Value; break;
                        default: result.Values[item.Key] = item.Value; break;
                    }
                }
                return result;
            }
        }

        private sealed class SavedTemplateRecord
        {
            public string Id = "";
            public string Name = "";
            public string Note = "";
            public string CreatedAt = "";
            public string UpdatedAt = "";
            public string LastGeneratedFiscalMonth = "";
            public string LastGeneratedFiscalYear = "";
            public string LastGeneratedAt = "";
            public Dictionary<string, string> Data = new Dictionary<string, string>();

            public override string ToString()
            {
                return Name;
            }
        }

        private sealed class QuickLoadConfirmationDialog : Form
        {
            public QuickLoadConfirmationDialog(string month, int year)
            {
                Text = "ยืนยันการใช้ข้อมูลเดิม";
                FormBorderStyle = FormBorderStyle.FixedDialog;
                StartPosition = FormStartPosition.CenterParent;
                MaximizeBox = false;
                MinimizeBox = false;
                ShowInTaskbar = false;
                ClientSize = new Size(510, 190);

                Controls.Add(new Label
                {
                    Text = "ยืนยันที่จะใช้ข้อมูลเดียวกันนี้ เพื่อส่งเบิกในเดือน",
                    Location = new Point(28, 30),
                    Size = new Size(455, 30),
                    Font = new Font("Tahoma", 11f, FontStyle.Regular),
                    TextAlign = ContentAlignment.MiddleCenter
                });
                Controls.Add(new Label
                {
                    Text = month + " " + year,
                    Location = new Point(28, 66),
                    Size = new Size(455, 38),
                    Font = new Font("Tahoma", 15f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(35, 105, 180),
                    TextAlign = ContentAlignment.MiddleCenter
                });

                var confirm = new Button
                {
                    Text = "ยืนยัน",
                    Location = new Point(272, 125),
                    Size = new Size(105, 36),
                    BackColor = Color.FromArgb(62, 133, 207),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    DialogResult = DialogResult.OK
                };
                confirm.FlatAppearance.BorderColor = Color.FromArgb(35, 105, 180);
                Controls.Add(confirm);
                Controls.Add(new Button
                {
                    Text = "ยกเลิก",
                    Location = new Point(387, 125),
                    Size = new Size(95, 36),
                    DialogResult = DialogResult.Cancel
                });
                AcceptButton = confirm;
                CancelButton = Controls[Controls.Count - 1] as Button;
            }
        }

        private sealed class TemplateSaveModeDialog : Form
        {
            public TemplateSaveMode SaveMode = TemplateSaveMode.SaveNew;

            public TemplateSaveModeDialog()
            {
                Text = "บันทึก Template";
                FormBorderStyle = FormBorderStyle.FixedDialog;
                StartPosition = FormStartPosition.CenterParent;
                MinimizeBox = false;
                MaximizeBox = false;
                ClientSize = new Size(360, 170);

                var saveNew = new RadioButton { Text = "บันทึกเป็น preset ใหม่", Location = new Point(24, 24), Size = new Size(220, 24), Checked = true };
                var overwrite = new RadioButton { Text = "บันทึกทับ preset เดิม", Location = new Point(24, 58), Size = new Size(220, 24) };
                Controls.Add(saveNew);
                Controls.Add(overwrite);

                var ok = new Button { Text = "ตกลง", Location = new Point(184, 112), Size = new Size(72, 30) };
                var cancel = new Button { Text = "ยกเลิก", Location = new Point(268, 112), Size = new Size(72, 30) };
                ok.Click += delegate
                {
                    SaveMode = overwrite.Checked ? TemplateSaveMode.Overwrite : TemplateSaveMode.SaveNew;
                    DialogResult = DialogResult.OK;
                    Close();
                };
                cancel.Click += delegate { DialogResult = DialogResult.Cancel; Close(); };
                Controls.Add(ok);
                Controls.Add(cancel);
                AcceptButton = ok;
                CancelButton = cancel;
            }
        }

        private sealed class TemplateEditorDialog : Form
        {
            private readonly TextBox nameBox = new TextBox();
            private readonly TextBox noteBox = new TextBox();

            public string TemplateName { get { return nameBox.Text.Trim(); } }
            public string TemplateNote { get { return noteBox.Text.Trim(); } }

            public TemplateEditorDialog(string title, string name, string note)
            {
                Text = title;
                FormBorderStyle = FormBorderStyle.FixedDialog;
                StartPosition = FormStartPosition.CenterParent;
                MinimizeBox = false;
                MaximizeBox = false;
                ClientSize = new Size(420, 220);

                Controls.Add(new Label { Text = "ชื่อ Template *", Location = new Point(20, 20), Size = new Size(140, 20) });
                nameBox.Location = new Point(20, 46);
                nameBox.Size = new Size(376, 24);
                nameBox.Text = name ?? "";
                Controls.Add(nameBox);

                Controls.Add(new Label { Text = "หมายเหตุ *", Location = new Point(20, 84), Size = new Size(140, 20) });
                noteBox.Location = new Point(20, 110);
                noteBox.Size = new Size(376, 54);
                noteBox.Multiline = true;
                noteBox.Text = note ?? "";
                Controls.Add(noteBox);

                var ok = new Button { Text = "บันทึก", Location = new Point(240, 178), Size = new Size(72, 30) };
                var cancel = new Button { Text = "ยกเลิก", Location = new Point(324, 178), Size = new Size(72, 30) };
                ok.Click += delegate
                {
                    if (TemplateName.Length == 0)
                    {
                        MessageBox.Show("กรุณากรอกชื่อ Template", "ข้อมูลไม่ครบ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (TemplateNote.Length == 0)
                    {
                        MessageBox.Show("กรุณากรอกหมายเหตุ", "ข้อมูลไม่ครบ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    DialogResult = DialogResult.OK;
                    Close();
                };
                cancel.Click += delegate { DialogResult = DialogResult.Cancel; Close(); };
                Controls.Add(ok);
                Controls.Add(cancel);
                AcceptButton = ok;
                CancelButton = cancel;
            }
        }

        private sealed class TemplatePickerDialog : Form
        {
            private readonly ListBox list = new ListBox();
            private readonly List<SavedTemplateRecord> templates;
            public SavedTemplateRecord SelectedTemplate { get; private set; }

            public TemplatePickerDialog(List<SavedTemplateRecord> templates, string title, bool overwriteMode)
            {
                this.templates = templates;
                Text = title;
                FormBorderStyle = FormBorderStyle.FixedDialog;
                StartPosition = FormStartPosition.CenterParent;
                MinimizeBox = false;
                MaximizeBox = false;
                ClientSize = new Size(460, 320);

                list.Location = new Point(20, 20);
                list.Size = new Size(420, 220);
                list.DisplayMember = "Name";
                list.DataSource = templates.Select(x => x).ToList();
                Controls.Add(list);

                var hint = new Label
                {
                    Text = overwriteMode ? "เลือก preset ที่จะบันทึกทับ" : "เลือก preset ที่จะโหลดเข้าฟอร์ม",
                    Location = new Point(20, 248),
                    Size = new Size(300, 20)
                };
                Controls.Add(hint);

                var ok = new Button { Text = overwriteMode ? "เลือก" : "โหลด", Location = new Point(284, 278), Size = new Size(72, 30) };
                var cancel = new Button { Text = "ยกเลิก", Location = new Point(368, 278), Size = new Size(72, 30) };
                ok.Click += delegate
                {
                    SelectedTemplate = list.SelectedItem as SavedTemplateRecord;
                    if (SelectedTemplate == null)
                    {
                        MessageBox.Show("กรุณาเลือกรายการ", "ยังไม่ได้เลือก", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    DialogResult = DialogResult.OK;
                    Close();
                };
                cancel.Click += delegate { DialogResult = DialogResult.Cancel; Close(); };
                Controls.Add(ok);
                Controls.Add(cancel);
                AcceptButton = ok;
                CancelButton = cancel;
            }
        }

        private sealed class TemplateManagerDialog : Form
        {
            private readonly List<SavedTemplateRecord> templates;
            private readonly ListBox list = new ListBox();
            private readonly TextBox previewBox = new TextBox();
            public bool IsDirty { get; private set; }
            public SavedTemplateRecord SelectedTemplate { get; private set; }

            public TemplateManagerDialog(List<SavedTemplateRecord> templates)
            {
                this.templates = templates;
                Text = "จัดการ Template";
                FormBorderStyle = FormBorderStyle.FixedDialog;
                StartPosition = FormStartPosition.CenterParent;
                MinimizeBox = false;
                MaximizeBox = false;
                ClientSize = new Size(760, 420);

                Controls.Add(new Label { Text = "รายการ Template", Location = new Point(20, 16), Size = new Size(160, 20) });
                list.Location = new Point(20, 44);
                list.Size = new Size(280, 300);
                list.DisplayMember = "Name";
                list.SelectedIndexChanged += delegate { RefreshPreview(); };
                Controls.Add(list);

                Controls.Add(new Label { Text = "รายละเอียด", Location = new Point(320, 16), Size = new Size(120, 20) });
                previewBox.Location = new Point(320, 44);
                previewBox.Size = new Size(420, 300);
                previewBox.Multiline = true;
                previewBox.ReadOnly = true;
                previewBox.ScrollBars = ScrollBars.Vertical;
                Controls.Add(previewBox);

                var apply = new Button { Text = "โหลดเข้าฟอร์ม", Location = new Point(320, 360), Size = new Size(110, 30) };
                var rename = new Button { Text = "แก้ชื่อ/หมายเหตุ", Location = new Point(442, 360), Size = new Size(120, 30) };
                var delete = new Button { Text = "ลบ", Location = new Point(574, 360), Size = new Size(72, 30) };
                var close = new Button { Text = "ปิด", Location = new Point(668, 360), Size = new Size(72, 30) };

                apply.Click += delegate
                {
                    SelectedTemplate = list.SelectedItem as SavedTemplateRecord;
                    if (SelectedTemplate == null)
                    {
                        MessageBox.Show("กรุณาเลือกรายการ", "ยังไม่ได้เลือก", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    DialogResult = DialogResult.OK;
                    Close();
                };
                rename.Click += delegate
                {
                    var record = list.SelectedItem as SavedTemplateRecord;
                    if (record == null)
                    {
                        MessageBox.Show("กรุณาเลือกรายการ", "ยังไม่ได้เลือก", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    using (var editor = new TemplateEditorDialog("แก้ไข Template", record.Name, record.Note))
                    {
                        if (editor.ShowDialog(this) != DialogResult.OK) return;
                        if (templates.Any(x => !ReferenceEquals(x, record) && string.Equals(x.Name, editor.TemplateName, StringComparison.OrdinalIgnoreCase)))
                        {
                            MessageBox.Show("มีชื่อ Template นี้แล้ว", "ชื่อซ้ำ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        record.Name = editor.TemplateName;
                        record.Note = editor.TemplateNote;
                        record.UpdatedAt = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
                        IsDirty = true;
                        RebindList(record);
                    }
                };
                delete.Click += delegate
                {
                    var record = list.SelectedItem as SavedTemplateRecord;
                    if (record == null)
                    {
                        MessageBox.Show("กรุณาเลือกรายการ", "ยังไม่ได้เลือก", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (MessageBox.Show("ลบ Template \"" + record.Name + "\" ?", "ยืนยัน", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
                    templates.Remove(record);
                    IsDirty = true;
                    RebindList(null);
                };
                close.Click += delegate { DialogResult = DialogResult.Cancel; Close(); };

                Controls.Add(apply);
                Controls.Add(rename);
                Controls.Add(delete);
                Controls.Add(close);

                RebindList(templates.FirstOrDefault());
                CancelButton = close;
            }

            private void RebindList(SavedTemplateRecord selected)
            {
                list.DataSource = null;
                list.DataSource = templates.Select(x => x).ToList();
                list.DisplayMember = "Name";
                if (selected != null)
                {
                    for (var i = 0; i < list.Items.Count; i++)
                    {
                        if (ReferenceEquals(list.Items[i], selected))
                        {
                            list.SelectedIndex = i;
                            break;
                        }
                    }
                }
                if (list.SelectedIndex < 0 && list.Items.Count > 0) list.SelectedIndex = 0;
                RefreshPreview();
            }

            private void RefreshPreview()
            {
                var record = list.SelectedItem as SavedTemplateRecord;
                if (record == null)
                {
                    previewBox.Text = "ยังไม่มีรายการ";
                    return;
                }
                previewBox.Text = "ชื่อ: " + record.Name + Environment.NewLine
                    + "หมายเหตุ: " + record.Note + Environment.NewLine
                    + "แก้ไขล่าสุด: " + record.UpdatedAt + Environment.NewLine
                    + Environment.NewLine
                    + "ฟิลด์ที่บันทึกไว้: " + record.Data.Keys.Count(x => !x.StartsWith("__", StringComparison.Ordinal));
            }
        }

        private sealed class CombinedPersonControl : Control
        {
            private readonly ComboBox prefix;
            private readonly TextBox name;
            private readonly TextBox surname;

            public CombinedPersonControl(ComboBox prefix, TextBox name, TextBox surname)
            {
                this.prefix = prefix;
                this.name = name;
                this.surname = surname;
            }

            public override string Text
            {
                get
                {
                    var title = Clean(prefix.Text);
                    var givenName = Clean(name.Text);
                    var familyName = Clean(surname.Text);
                    var fullName = title;
                    if (givenName.Length > 0)
                    {
                        fullName += title.Length == 0 || title.Contains("ร้อยตรี") || title == "ดร." ? " " + givenName : givenName;
                    }
                    if (familyName.Length > 0)
                    {
                        fullName += (fullName.Length == 0 ? "" : " ") + familyName;
                    }
                    return fullName.Trim();
                }
                set
                {
                    var fullName = Clean(value);
                    var matchedPrefix = prefix.Items.Cast<object>()
                        .Select(x => Clean(x == null ? "" : x.ToString()))
                        .Where(x => x.Length > 0 && x != "--" && x != "........" && fullName.StartsWith(x, StringComparison.Ordinal))
                        .OrderByDescending(x => x.Length)
                        .FirstOrDefault();

                    if (!string.IsNullOrWhiteSpace(matchedPrefix))
                    {
                        for (var i = 0; i < prefix.Items.Count; i++)
                        {
                            if (string.Equals(Clean(prefix.Items[i] == null ? "" : prefix.Items[i].ToString()), matchedPrefix, StringComparison.Ordinal))
                            {
                                prefix.SelectedIndex = i;
                                break;
                            }
                        }
                        fullName = fullName.Substring(matchedPrefix.Length).Trim();
                    }
                    else if (prefix.Items.Count > 0)
                    {
                        prefix.SelectedIndex = 0;
                    }

                    var parts = fullName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    name.Text = parts.Length > 0 ? parts[0] : "";
                    surname.Text = parts.Length > 1 ? string.Join(" ", parts.Skip(1).ToArray()) : "";
                }
            }

            public void SetChildrenVisible(bool visible)
            {
                prefix.Visible = visible;
                name.Visible = visible;
                surname.Visible = visible;
            }

            private static string Clean(string value)
            {
                value = (value ?? "").Trim();
                return value == "--" || value.StartsWith("-- ") ? "" : value;
            }
        }

        private sealed class OptionalPersonRow
        {
            private readonly Control titleLabel;
            private readonly Control prefixLabel;
            private readonly Control nameLabel;
            private readonly Control surnameLabel;
            public readonly CombinedPersonControl Combined;

            public OptionalPersonRow(Control titleLabel, Control prefixLabel, Control nameLabel, Control surnameLabel, CombinedPersonControl combined)
            {
                this.titleLabel = titleLabel;
                this.prefixLabel = prefixLabel;
                this.nameLabel = nameLabel;
                this.surnameLabel = surnameLabel;
                Combined = combined;
            }

            public void SetVisible(bool visible)
            {
                titleLabel.Visible = visible;
                prefixLabel.Visible = visible;
                nameLabel.Visible = visible;
                surnameLabel.Visible = visible;
                Combined.SetChildrenVisible(visible);
            }
        }

        private sealed class TextSpan
        {
            public int Start;
            public int End;
            public XmlNode Node;
        }

        private sealed class PositionOption
        {
            public string Position;
            public string Salary;
            public string SalaryText;
            public string TotalSalary;
            public string TotalSalaryText;

            public override string ToString()
            {
                if (Position == "ครูในโรงเรียนโครงการตามพระราชดำริและโรงเรียนเฉลิมพระเกียรติ 9,000") return "ครูพระราชดำริ 9,000";
                if (Position == "ครูในโรงเรียนโครงการตามพระราชดำริและโรงเรียนเฉลิมพระเกียรติ 15,000") return "ครูพระราชดำริ 15,000";
                return Position;
            }
        }

        private sealed class MonthOption
        {
            public string Month;
            public string PayDate;
            public string ShortMonth;
            public string OrderYear;
            public Dictionary<string, string> PayDatesByFiscalYear = new Dictionary<string, string>();
        }

        private static void RenderDocx(string templatePath, string outputPath, Dictionary<string, string> values)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
            outputPath = EnsureUniqueOutputPath(outputPath);
            using (var source = ZipFile.OpenRead(templatePath))
            using (var target = ZipFile.Open(outputPath, ZipArchiveMode.Create))
            {
                foreach (var entry in source.Entries)
                {
                    var newEntry = target.CreateEntry(entry.FullName, CompressionLevel.Optimal);
                    byte[] bytes;
                    using (var input = entry.Open())
                    using (var memory = new MemoryStream())
                    {
                        input.CopyTo(memory);
                        bytes = memory.ToArray();
                    }
                    if (WordXmlRegex.IsMatch(entry.FullName)) bytes = ReplaceTagsInXml(bytes, values, Path.GetFileName(templatePath));
                    using (var output = newEntry.Open()) output.Write(bytes, 0, bytes.Length);
                }
            }
        }

        private static string EnsureUniqueOutputPath(string outputPath)
        {
            if (!File.Exists(outputPath)) return outputPath;

            var directory = Path.GetDirectoryName(outputPath);
            var fileName = Path.GetFileNameWithoutExtension(outputPath);
            var extension = Path.GetExtension(outputPath);
            var counter = 2;
            string candidate;
            do
            {
                candidate = Path.Combine(directory, fileName + "_" + counter + extension);
                counter++;
            }
            while (File.Exists(candidate));
            return candidate;
        }

        private static byte[] ReplaceTagsInXml(byte[] bytes, Dictionary<string, string> values, string templateName)
        {
            var xml = new XmlDocument { PreserveWhitespace = true };
            xml.LoadXml(Encoding.UTF8.GetString(bytes));
            var nsm = new XmlNamespaceManager(xml.NameTable);
            nsm.AddNamespace("w", WordNs);
            foreach (XmlNode paragraph in xml.SelectNodes("//w:p[not(.//w:p)]", nsm))
            {
                ReplaceParagraphTags(xml, nsm, paragraph, values, templateName);
            }
            CenterSignatureParagraphs(xml, nsm);
            using (var memory = new MemoryStream())
            {
                var settings = new XmlWriterSettings { Encoding = new UTF8Encoding(false), OmitXmlDeclaration = false };
                using (var writer = XmlWriter.Create(memory, settings)) xml.Save(writer);
                return memory.ToArray();
            }
        }

        private static void ReplaceParagraphTags(XmlDocument xml, XmlNamespaceManager nsm, XmlNode paragraph, Dictionary<string, string> values, string templateName)
        {
            var initialNodes = paragraph.SelectNodes(".//w:t", nsm).Cast<XmlNode>().ToList();
            var initialText = string.Join("", initialNodes.Select(x => x.InnerText ?? "").ToArray());
            var contextualValues = new Dictionary<string, string>(values);
            if (templateName == "5. บันทึกอนุมัติเบิกจ่าย.docx")
            {
                if (initialText.Contains("{ชื่อพัสดุ}")) contextualValues["{คำนำหน้าชื่อ}"] = GetValue(values, "{คำนำหน้าพัสดุ}");
                else if (initialText.Contains("{ชื่อหพัสดุ}")) contextualValues["{คำนำหน้าชื่อ}"] = GetValue(values, "{คำนำหน้าหพัสดุ}");
                else if (initialText.Contains("{ชื่อการเงิน}")) contextualValues["{คำนำหน้าชื่อ}"] = GetValue(values, "{คำนำหน้าการเงิน}");
                else contextualValues["{คำนำหน้าชื่อ}"] = GetValue(values, "{คำนำหน้าลูกจ้าง}");
                if (initialText.Contains("โรงเรียน{ชื่อโรงเรียน}")) contextualValues["{ชื่อโรงเรียน}"] = RemoveSchoolPrefix(GetValue(values, "{ชื่อโรงเรียน}"));
            }
            while (true)
            {
                var nodes = paragraph.SelectNodes(".//w:t", nsm).Cast<XmlNode>().ToList();
                var text = string.Join("", nodes.Select(x => x.InnerText ?? "").ToArray());
                var matches = TagRegex.Matches(text).Cast<Match>().Where(x => contextualValues.ContainsKey(x.Value)).ToArray();
                if (matches.Length == 0) return;
                var match = matches[matches.Length - 1];
                var spans = new List<TextSpan>();
                var cursor = 0;
                foreach (var node in nodes)
                {
                    var nodeText = node.InnerText ?? "";
                    spans.Add(new TextSpan { Start = cursor, End = cursor + nodeText.Length, Node = node });
                    cursor += nodeText.Length;
                }
                var touched = spans.Where(x => x.Start < match.Index + match.Length && x.End > match.Index).ToArray();
                if (touched.Length == 0) return;
                var first = touched[0];
                var last = touched[touched.Length - 1];
                var firstText = first.Node.InnerText ?? "";
                var lastText = last.Node.InnerText ?? "";
                var prefixLength = Math.Max(0, match.Index - first.Start);
                var prefix = prefixLength <= firstText.Length ? firstText.Substring(0, prefixLength) : "";
                var suffixStart = Math.Max(0, match.Index + match.Length - last.Start);
                var suffix = suffixStart < lastText.Length ? lastText.Substring(suffixStart) : "";
                first.Node.InnerText = prefix + (contextualValues[match.Value] ?? "") + suffix;
                var attr = xml.CreateAttribute("xml", "space", XmlNs);
                attr.Value = "preserve";
                first.Node.Attributes.SetNamedItem(attr);
                for (var i = 1; i < touched.Length; i++) touched[i].Node.InnerText = "";
            }
        }

        private static string RemoveSchoolPrefix(string value)
        {
            value = (value ?? "").Trim();
            return value.StartsWith("โรงเรียน", StringComparison.Ordinal) ? value.Substring("โรงเรียน".Length).TrimStart() : value;
        }

        private static string GetValue(Dictionary<string, string> values, string key)
        {
            string value;
            return values.TryGetValue(key, out value) ? value : "";
        }

        private static void CenterSignatureParagraphs(XmlDocument xml, XmlNamespaceManager nsm)
        {
            var paragraphs = xml.SelectNodes("//w:p", nsm);
            if (paragraphs == null) return;
            foreach (XmlNode paragraph in paragraphs)
            {
                var textNodes = paragraph.SelectNodes(".//w:t", nsm);
                if (textNodes == null || textNodes.Count == 0) continue;
                var builder = new StringBuilder();
                foreach (XmlNode node in textNodes) builder.Append(node.InnerText ?? "");
                if (ShouldCenterSignatureLine(builder.ToString())) SetParagraphAlignment(xml, nsm, paragraph, "center");
            }
        }

        private static bool ShouldCenterSignatureLine(string text)
        {
            var value = (text ?? "").Trim();
            if (value.Length == 0 || value.Length > 110) return false;
            if (value.StartsWith("ลงชื่อ")) return false;
            if (value.StartsWith("เรียน")) return false;
            if (value.StartsWith("ตามที่")) return false;
            if (value.StartsWith("บัดนี้")) return false;
            if (value.StartsWith("จึง")) return false;
            if (value.StartsWith("(") && value.EndsWith(")")) return true;
            return value == "เจ้าหน้าที่"
                || value == "เจ้าหน้าที่พัสดุ"
                || value == "หัวหน้าเจ้าหน้าที่พัสดุ"
                || value == "เจ้าหน้าที่การเงิน"
                || value == "หัวหน้าเจ้าหน้าที่การเงิน"
                || value == "กรรมการตรวจรับพัสดุ"
                || value == "ประธานกรรมการตรวจรับพัสดุ"
                || value.StartsWith("ผู้อำนวยการโรงเรียน");
        }

        private static void SetParagraphAlignment(XmlDocument xml, XmlNamespaceManager nsm, XmlNode paragraph, string alignment)
        {
            var pPr = paragraph.SelectSingleNode("./w:pPr", nsm);
            if (pPr == null)
            {
                pPr = xml.CreateElement("w", "pPr", WordNs);
                paragraph.PrependChild(pPr);
            }
            var jc = pPr.SelectSingleNode("./w:jc", nsm);
            if (jc == null)
            {
                jc = xml.CreateElement("w", "jc", WordNs);
                pPr.AppendChild(jc);
            }
            var attr = jc.Attributes["w:val"];
            if (attr == null) attr = xml.CreateAttribute("w", "val", WordNs);
            attr.Value = alignment;
            jc.Attributes.SetNamedItem(attr);
        }
    }
}
