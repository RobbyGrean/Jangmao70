using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace ReimbursementDocApp
{
    internal static class Document346ShellProgram
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Document346ShellForm());
        }
    }

    internal sealed class Document346ShellForm : Form
    {
        private readonly TabControl tabs = new TabControl();
        private Form payrollForm;
        private ProcurementModule procurementModule;
        private bool closingConfirmed;

        public Document346ShellForm()
        {
            Text = "Jangmao70";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new System.Drawing.Size(1160, 800);
            MinimumSize = new System.Drawing.Size(980, 700);
            BackColor = Color.White;
            Font = new Font("Tahoma", 10.5f);
            tabs.Dock = DockStyle.Fill;
            tabs.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabs.SizeMode = TabSizeMode.Fixed;
            tabs.ItemSize = new Size(210, 30);
            tabs.Padding = new Point(16, 2);
            tabs.DrawItem += DrawTab;
            Controls.Add(tabs);

            BuildProcurementTab();
            BuildPayrollTab();
            tabs.SelectedIndex = 0;
            FormClosing += HandleFormClosing;
        }

        private void DrawTab(object sender, DrawItemEventArgs e)
        {
            var selected = e.Index == tabs.SelectedIndex;
            var accent = e.Index == 0 ? Color.FromArgb(76, 149, 108) : Color.FromArgb(83, 151, 226);
            var background = selected ? accent : Color.White;
            var foreground = selected ? Color.White : Color.FromArgb(45, 55, 72);
            using (var backgroundBrush = new SolidBrush(background))
            using (var foregroundBrush = new SolidBrush(foreground))
                using (var font = new Font(tabs.Font, selected ? FontStyle.Bold : FontStyle.Regular))
            {
                e.Graphics.FillRectangle(backgroundBrush, e.Bounds);
                var textBounds = new Rectangle(e.Bounds.Left + 8, Math.Max(0, e.Bounds.Top + 3), e.Bounds.Width - 16, Math.Max(1, e.Bounds.Height - 6));
                e.Graphics.DrawString(tabs.TabPages[e.Index].Text, font, foregroundBrush, textBounds, new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
            }
            e.DrawFocusRectangle();
        }

        private void BuildPayrollTab()
        {
            var page = new TabPage("เบิกเงินเดือน") { BackColor = System.Drawing.Color.White, AutoScroll = true };
            payrollForm = CreateLegacyPayrollForm();
            payrollForm.TopLevel = false;
            payrollForm.FormBorderStyle = FormBorderStyle.None;
            payrollForm.Dock = DockStyle.Fill;
            page.Controls.Add(payrollForm);
            payrollForm.Show();
            tabs.TabPages.Add(page);
        }

        private void BuildProcurementTab()
        {
            var page = new TabPage("จัดซื้อจัดจ้างและสัญญา") { BackColor = System.Drawing.Color.FromArgb(245, 247, 250) };
            procurementModule = new ProcurementModule(WorkingRecord.CreateEmpty(), null);
            page.Controls.Add(procurementModule);
            tabs.TabPages.Add(page);
        }

        private Form CreateLegacyPayrollForm()
        {
            var programType = typeof(Program);
            var loadTemplates = programType.GetMethod("LoadTemplates", BindingFlags.NonPublic | BindingFlags.Static);
            var templates = loadTemplates.Invoke(null, null);
            var mainFormType = programType.GetNestedType("MainForm", BindingFlags.NonPublic);
            var form = (Form)Activator.CreateInstance(mainFormType, new[] { templates });
            var templateBox = GetPrivateField(form, "templateBox") as TextBox;
            var outputBox = GetPrivateField(form, "outputBox") as TextBox;
            var templateRoot = Document346Paths.EnsureUserTemplateRoot(DocumentModule.Payroll);
            if (templateBox != null) templateBox.Text = templateRoot;
            if (outputBox != null) outputBox.Text = Document346Paths.OutputRoot(DocumentModule.Payroll);
            return form;
        }

        private Dictionary<string, Control> GetPayrollFieldBoxes()
        {
            return GetPrivateField(payrollForm, "fieldBoxes") as Dictionary<string, Control>;
        }
        private void HandleFormClosing(object sender, FormClosingEventArgs e)
        {
            if (closingConfirmed || procurementModule == null) return;
            if (!procurementModule.HasMeaningfulData() && !HasPayrollMeaningfulData()) return;
            var choice = MessageBox.Show("มีข้อมูลที่ยังไม่ได้บันทึก ต้องการบันทึกก่อนปิดหรือไม่?\nใช่ = บันทึก, ไม่ใช่ = ไม่บันทึก, ยกเลิกหรือกากบาท = กลับไปทำงาน", "ปิด Jangmao70", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (choice == DialogResult.Cancel)
            {
                e.Cancel = true;
                return;
            }
            if (choice == DialogResult.Yes)
            {
                var result = procurementModule.SaveCurrentTemplate();
                if (result != TemplateOperationResult.Saved)
                {
                    e.Cancel = true;
                    return;
                }
            }
            closingConfirmed = true;
        }

        private bool HasPayrollMeaningfulData()
        {
            var fields = GetPayrollFieldBoxes();
            if (fields == null) return false;
            return !string.IsNullOrWhiteSpace(ReadField(fields, "{ชื่อลูกจ้าง}"))
                || !string.IsNullOrWhiteSpace(ReadField(fields, "{นามสกุลลูกจ้าง}"))
                || !string.IsNullOrWhiteSpace(ReadField(fields, "{ชื่อโรงเรียน}"))
                || !string.IsNullOrWhiteSpace(ReadField(fields, "{ใบสั่งจ้าง}"));
        }

        private static object GetPrivateField(object instance, string name)
        {
            if (instance == null) return null;
            var field = instance.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            return field == null ? null : field.GetValue(instance);
        }

        private static string ReadField(Dictionary<string, Control> fields, string key)
        {
            Control control;
            return fields.TryGetValue(key, out control) && control != null ? (control.Text ?? "").Trim() : "";
        }

    }
}
