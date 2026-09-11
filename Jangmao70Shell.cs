using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace Jangmao70
{
    internal static class Jangmao70ShellProgram
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Jangmao70ShellForm());
        }
    }

    internal sealed class Jangmao70ShellForm : Form
    {
        private readonly TabControl tabs = new TabControl();
        private Form payrollForm;
        private ProcurementModule procurementModule;
        private bool closingConfirmed;

        public Jangmao70ShellForm()
        {
            Text = "Jangmao70";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new System.Drawing.Size(1220, 850);
            MinimumSize = new System.Drawing.Size(1080, 760);
            BackColor = Color.FromArgb(243, 246, 248);
            Font = new Font("Segoe UI", 10f);
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
            var accent = e.Index == 0 ? Color.FromArgb(15, 118, 110) : Color.FromArgb(31, 94, 140);
            var background = selected ? accent : Color.White;
            var foreground = selected ? Color.White : Color.FromArgb(30, 41, 59);
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
            var page = new TabPage("เบิกเงินเดือน") { BackColor = Color.FromArgb(243, 246, 248), AutoScroll = true };
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
            var page = new TabPage("จัดซื้อจัดจ้างและสัญญา") { BackColor = Color.FromArgb(243, 246, 248) };
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
            var templateRoot = Jangmao70Paths.EnsureUserTemplateRoot(DocumentModule.Payroll);
            if (templateBox != null) templateBox.Text = templateRoot;
            if (outputBox != null) outputBox.Text = Jangmao70Paths.OutputRoot(DocumentModule.Payroll);
            return form;
        }

        private void HandleFormClosing(object sender, FormClosingEventArgs e)
        {
            if (closingConfirmed || procurementModule == null) return;
            var procurementHasChanges = procurementModule.IsDirty;
            var payrollHasChanges = IsPayrollDirty();
            if (!procurementHasChanges && !payrollHasChanges) return;
            var choice = MessageBox.Show("มีข้อมูลที่ยังไม่ได้บันทึก ต้องการบันทึกก่อนปิดหรือไม่?\nใช่ = บันทึก, ไม่ใช่ = ไม่บันทึก, ยกเลิกหรือกากบาท = กลับไปทำงาน", "ปิด Jangmao70", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (choice == DialogResult.Cancel)
            {
                e.Cancel = true;
                return;
            }
            if (choice == DialogResult.Yes)
            {
                if (payrollHasChanges && !SavePayrollDraftForExit())
                {
                    e.Cancel = true;
                    return;
                }
                if (procurementHasChanges)
                {
                    var result = procurementModule.SaveCurrentTemplate();
                    if (result != TemplateOperationResult.Saved)
                    {
                        e.Cancel = true;
                        return;
                    }
                }
            }
            closingConfirmed = true;
        }

        private bool IsPayrollDirty()
        {
            if (payrollForm == null) return false;
            var property = payrollForm.GetType().GetProperty("IsDirty", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (property == null) return false;
            var value = property.GetValue(payrollForm, null);
            return value is bool && (bool)value;
        }

        private bool SavePayrollDraftForExit()
        {
            if (payrollForm == null) return true;
            var method = payrollForm.GetType().GetMethod("SaveDraftForExit", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (method == null) return true;
            try
            {
                var result = method.Invoke(payrollForm, null);
                return !(result is bool) || (bool)result;
            }
            catch (TargetInvocationException ex)
            {
                var message = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                MessageBox.Show("บันทึกข้อมูลเบิกเงินเดือนก่อนปิดไม่สำเร็จ: " + message, "บันทึกข้อมูล", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("บันทึกข้อมูลเบิกเงินเดือนก่อนปิดไม่สำเร็จ: " + ex.Message, "บันทึกข้อมูล", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private static object GetPrivateField(object instance, string name)
        {
            if (instance == null) return null;
            var field = instance.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            return field == null ? null : field.GetValue(instance);
        }

    }
}
