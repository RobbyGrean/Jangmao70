using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace ReimbursementDocApp
{
    internal static class Document346ShellSmokeTest
    {
        [STAThread]
        private static int Main()
        {
            try
            {
                using (var shell = new Document346ShellForm())
                {
                    if (shell.Text != "Jangmao70") throw new InvalidOperationException("Program title was not renamed to Jangmao70.");
                    var tabs = shell.Controls.OfType<TabControl>().SingleOrDefault();
                    if (tabs == null || tabs.TabPages.Count != 2 || tabs.SelectedIndex != 0) throw new InvalidOperationException("Shell must have two tabs and open at Procurement.");
                    if (tabs.TabPages[0].Text != "จัดซื้อจัดจ้างและสัญญา" || tabs.TabPages[1].Text != "เบิกเงินเดือน") throw new InvalidOperationException("Tab labels are incorrect.");
                    if (tabs.TabPages[0].Controls.OfType<ProcurementModule>().SingleOrDefault() == null) throw new InvalidOperationException("Procurement module is not hosted in the first tab.");
                    var procurement = tabs.TabPages[0].Controls.OfType<ProcurementModule>().Single();
                    var sectionList = GetField<ListBox>(procurement, "sectionList");
                    if (sectionList == null || sectionList.Items.Count != 4) throw new InvalidOperationException("Procurement must expose four clear zones after merging employee information.");
                    if (sectionList.Items[1].ToString() != "2  กรรมการ TOR" || sectionList.Items[2].ToString() != "3  ข้อมูลลูกจ้าง" || sectionList.Items[3].ToString() != "4  สรุปชุดเอกสารที่ต้องการสร้าง") throw new InvalidOperationException("Procurement zone labels do not match the current UX decision.");
                    var procurementFields = GetField<Dictionary<string, Control>>(procurement, "fields");
                    if (procurementFields == null || !procurementFields.ContainsKey("procurement.specificationOrder")) throw new InvalidOperationException("TOR appointment order field is missing from the Procurement form.");
                    var sectionPanels = GetField<List<Panel>>(procurement, "sectionPanels");
                    var committeePanel = sectionPanels == null || sectionPanels.Count < 2 ? null : sectionPanels[1];
                    var committeeGroups = committeePanel == null ? new Panel[0] : committeePanel.Controls.OfType<Panel>().Where(x => x.Width == 700 && x.Height == 154).ToArray();
                    var orderBox = procurementFields["procurement.specificationOrder"] as TextBox;
                    if (committeeGroups.Length != 3 || committeeGroups.Any(x => x.Controls.OfType<ComboBox>().Count() != 2 || x.Controls.OfType<TextBox>().Count() != 2) || orderBox == null || orderBox.Width != 220 || procurementFields["procurement.specificationOrder"].Top <= committeeGroups.Max(x => x.Bottom) || !committeePanel.Controls.OfType<Label>().Any(x => x.Text == "(เช่น 1/2570)")) throw new InvalidOperationException("Each TOR committee must contain its own fields, and the short appointment-order field must sit below the three groups with an example hint.");
                    var employeePanel = sectionPanels == null || sectionPanels.Count < 3 ? null : sectionPanels[2];
                    var educationLevel = procurementFields == null || !procurementFields.ContainsKey("employee.educationLevel") ? null : procurementFields["employee.educationLevel"];
                    var qualification = procurementFields == null || !procurementFields.ContainsKey("employee.qualification") ? null : procurementFields["employee.qualification"];
                    var houseNumber = procurementFields == null || !procurementFields.ContainsKey("employee.houseNumber") ? null : procurementFields["employee.houseNumber"];
                    var expiryYear = procurementFields == null || !procurementFields.ContainsKey("employee.idExpiry.year") ? null : procurementFields["employee.idExpiry.year"];
                    if (employeePanel == null || educationLevel == null || qualification == null || houseNumber == null || expiryYear == null || educationLevel.Top >= houseNumber.Top || employeePanel.AutoScrollMinSize.Height < qualification.Bottom || employeePanel.AutoScrollMinSize.Height < expiryYear.Bottom + 80 || !employeePanel.Controls.OfType<Label>().Any(x => x.Text == "การศึกษา")) throw new InvalidOperationException("Employee education fields must be present in a clear zone before the address fields, and the final expiry-year row must remain reachable above the fixed footer.");
                    var secondSchoolRow = GetField<Panel>(procurement, "secondSchoolRow");
                    var secondSchoolBox = GetField<TextBox>(procurement, "secondSchoolBox");
                    if (secondSchoolRow == null || secondSchoolBox == null || secondSchoolBox.Parent != secondSchoolRow || secondSchoolRow.Bottom >= educationLevel.Top) throw new InvalidOperationException("The second-school field must stay in its own row above the education zone.");
                    var procurementPositionBox = GetField<ComboBox>(procurement, "positionBox");
                    var dateCombos = GetField<Dictionary<string, ComboBox>>(procurement, "dateCombos");
                    if (procurementPositionBox == null || procurementPositionBox.Items[0].ToString() != "-- เลือกตำแหน่ง --" || dateCombos == null || dateCombos["employee.birth.month"].Items[0].ToString() != "-- ไม่ระบุ --" || procurementFields["employee.prefix"].Text != "-- ไม่ระบุ --") throw new InvalidOperationException("Optional dropdowns must explain their empty state instead of showing a blank row. Actual position='" + (procurementPositionBox == null ? "<missing>" : procurementPositionBox.Text) + "', month='" + (dateCombos == null ? "<missing>" : dateCombos["employee.birth.month"].Items[0].ToString()) + "', prefix='" + (procurementFields == null ? "<missing>" : procurementFields["employee.prefix"].Text) + "'.");
                    dateCombos["employee.birth.month"].SelectedIndex = 0;
                    Invoke(procurement, "CaptureFromControls");
                    if (procurement.Record.Employee.BirthMonth != "" || procurement.Record.Procurement.PositionId != "") throw new InvalidOperationException("Dropdown helper labels must remain empty in the saved record.");

                    var payrollForm = GetField<Form>(shell, "payrollForm");
                    var positionBox = GetField<ComboBox>(payrollForm, "positionBox");
                    var fields = GetField<Dictionary<string, Control>>(payrollForm, "fieldBoxes");
                    var orderDay = GetField<ComboBox>(payrollForm, "orderDayBox");
                    var orderMonth = GetField<ComboBox>(payrollForm, "orderMonthBox");
                    var orderYear = GetField<ComboBox>(payrollForm, "orderYearBox");
                    if (positionBox == null || positionBox.Items.Count < 2 || fields == null || orderDay == null || orderMonth == null || orderYear == null) throw new InvalidOperationException("Payroll controls were not available for independent-state test.");
                    if (fields.ContainsKey("{เงินรวม}") || fields.ContainsKey("{เงินรวมTEXT}")) throw new InvalidOperationException("Unused twelve-month total salary fields must not be rendered in Payroll.");
                    if (orderDay.SelectedIndex != -1 || orderMonth.SelectedIndex != -1 || orderYear.SelectedIndex != -1 || fields["{วันที่สั่งจ้าง}"].Text != "") throw new InvalidOperationException("Payroll order date must start blank.");

                    procurementFields["school.name"].Text = "Procurement School";
                    Invoke(procurement, "CaptureFromControls");
                    if (fields["{ชื่อโรงเรียน}"].Text != "") throw new InvalidOperationException("Procurement input must not populate Payroll.");
                    fields["{ชื่อโรงเรียน}"].Text = "Payroll School";
                    if (procurement.Record.School.Name != "Procurement School") throw new InvalidOperationException("Payroll input must not alter Procurement.");

                    orderDay.SelectedIndex = 0;
                    orderMonth.SelectedIndex = 0;
                    orderYear.SelectedIndex = 0;
                    Invoke(payrollForm, "ApplyOrderDateInputs");
                    if (fields["{วันที่สั่งจ้าง}"].Text != "1 มกราคม 2569") throw new InvalidOperationException("Payroll order date was not built from the selected date fields.");
                    orderDay.SelectedIndex = -1;
                    orderMonth.SelectedIndex = -1;
                    orderYear.SelectedIndex = -1;
                    Invoke(payrollForm, "ApplyOrderDateInputs");
                    if (fields["{วันที่สั่งจ้าง}"].Text != "") throw new InvalidOperationException("Clearing the Payroll order date must keep its tag blank.");
                }
                Console.WriteLine("PASS: shell has independent Payroll + Procurement tabs, blank Payroll order date, and no unused total-salary fields");
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("FAIL: " + ex);
                return 2;
            }
        }

        private static T GetField<T>(object instance, string name) where T : class
        {
            if (instance == null) return null;
            var field = instance.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            return field == null ? null : field.GetValue(instance) as T;
        }

        private static void Invoke(object instance, string name)
        {
            var method = instance.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null) throw new InvalidOperationException("Missing shell method: " + name);
            method.Invoke(instance, null);
        }
    }
}
