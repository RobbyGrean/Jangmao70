# Astra review ก่อน implementation เอกสาร 346

วันที่: 2026-09-09  
ผล: **READY_FOR_IMPLEMENTATION — ไม่มี OPEN ที่ต้องถามผู้ใช้เพิ่ม**  
ขอบเขตรอบนี้: review และแก้ Markdown เท่านั้น; งาน T01–T07 ยังไม่ถือว่า DONE  
ผู้อ่านถัดไป: Luna Xhigh หรือผู้ทำ implementation ตาม IMPLEMENTATION_PROMPT.md

## 1. ข้อสรุปและลำดับอำนาจของข้อมูล

อ่าน GRILL_HANDOFF.md ก่อนและพบ READY_FOR_IMPLEMENTATION พร้อมคำตอบ D01–D12 ที่ผู้ใช้ยืนยันแล้ว จากนั้นอ่าน PROJECT_CONTEXT.md, ARCHITECTURE_PLAN.md, UX UI design plan.md, TAG_MAPPING.md, IMPLEMENTATION_HANDOFF.md และ IMPLEMENTATION_PROMPT.md ครบ ตรวจ source/config/build/installer/smoke และ DOCX แบบ read-only

ใช้คำตอบผู้ใช้ล่าสุดและ D01–D12 เหนือข้อเสนอเก่าในแผน รายงานนี้แก้ข้อความที่ขัดกันและกำหนดรายละเอียดทางเทคนิคจากหลักฐาน ไม่ได้เพิ่มแบบเอกสาร ตำแหน่งงาน หรือข้อบังคับทางธุรกิจใหม่ ข้อบกพร่องใน source ยังรอแก้ใน implementation การคง READY หมายถึงพร้อมเริ่มงาน ไม่ใช่โปรแกรมพร้อมปล่อยใช้งาน

ไม่พบ AGENTS.md ใน H:\, H:\CodexProjects\, workspace root หรือภายใน workspace ใช้ code-reviewer และ documents skills สำหรับการตรวจ ไม่ใช้ข้อเสนอ C# รุ่นใหม่จาก skill มาบังคับเปลี่ยน .NET Framework/compiler เดิม

## 2. ข้อกำหนดที่ตรวจแล้ว

| เรื่อง | ข้อสรุปที่ใช้ต่อ |
|---|---|
| สองแท็บ | หนึ่ง EXE หนึ่งหน้าต่าง; เบิกเงินเดือนอยู่ซ้าย จัดซื้อจัดจ้างและสัญญาอยู่ขวา และเปิดที่ Procurement |
| ลูกจ้างและโรงเรียน | working record ร่วมหนึ่งชุดต่อการกรอก; โรงเรียน/บุคลากร/กรรมการอยู่กับ record นั้น เครื่องเดียวบันทึกหลายโรงเรียนได้ ไม่มี active-school หรือ school master ที่แก้แล้วเปลี่ยนทุกคน |
| กรรมการ | คนชุดเดียว A→A/B→B/C→C; A ประธาน; เก็บชื่อแยกส่วนและตำแหน่ง 3 คน ไม่ทำกรรมการ TOR ซ้ำอีกฐานหนึ่ง |
| ที่อยู่ | ถนน optional และจำร่วม; Procurement ไม่มีเฉพาะ tag ถนน ส่วนบ้านเลขที่/ตำบล/อำเภอ/จังหวัดมีทั้งใบเสนอราคา 13 แบบและใบสั่งจ้าง |
| เอกสาร | Procurement 34 templates = กลาง 8 + TOR 13 + ใบเสนอราคา 13; ชุดที่ route แล้ว 10 ไฟล์; Payroll ใช้ 5 ไฟล์ตาม manifest; รวมส่งมอบ 39 |
| Checkbox | Procurement แสดง 10 รายการของชุดที่ resolve แล้วและติ๊กครบ; เลือกบางไฟล์ได้ ไม่ให้ติ๊ก 34 variants พร้อมกัน; เลือก 0 ไฟล์ต้องแจ้ง |
| สัญญา | ใช้ 9. ใบสั่งจ้าง.docx ที่มีอยู่ ไม่เพิ่มเอกสารสัญญาใหม่ |
| เงินและตำแหน่ง | Procurement 5 ตำแหน่ง/static เงินรวม/อากรตาม context; Payroll คง catalog และกฎเดิมของตัวเอง |
| เพิ่ม/บันทึก/โหลด | บันทึก Template คือ snapshot ข้อมูลฟอร์ม ไม่ใช่สร้าง DOCX แม่แบบ; เพิ่มใหม่ถามก่อนและล้าง record ทั้งหมดหลังบันทึกสำเร็จหรือเลือกไม่บันทึก; โหลดเป็นสำเนา ไม่ auto-save ทับ |
| ปิด/เปิด | ตรวจ dirty ทั้งสองแท็บรวมข้อมูลร่วม; บันทึก/ไม่บันทึก; X กลับไปทำงาน; เปิดใหม่ไม่มีข้อมูลลูกจ้างหรือร่างเก่าถูกโหลดเอง |
| Output | ค่าเริ่มต้นใต้ app data root ของ 346: Output/เงินเดือน และ Output/เอกสารจัดจ้าง; ชื่อชนต่อ _2, _3; custom output ภายนอกไม่อยู่ในขอบเขตถอนติดตั้ง |
| ติดตั้ง/อัปเดต | ต่อ Windows user ใน LOCALAPPDATA; เริ่มฐาน 346 ใหม่ไม่ import baseline; update รุ่น 346 ต่อไปคงข้อมูล, user Template และ Output |
| ถอนติดตั้ง | ลบเฉพาะ root/shortcut ที่พิสูจน์ว่าเป็นของ 346; ต้องไม่ลบแอป baseline หรือ path ภายนอก |
| ไม่มี Word | generate DOCX ได้; ZIP/XML ผ่านไม่ใช่ layout ผ่าน; แยกสถานะ visual QA และ Windows 10/11 จริง |

## 3. Findings และงานที่ต้องแก้ใน implementation

ทุก Rxx ด้านล่างเป็นงาน implementation หรือข้อแก้ไขเอกสาร ไม่มีรายการที่ต้องรอคำตอบธุรกิจใหม่

### R01 ข้อเสนอ SchoolProfile และการล้างข้อมูลขัดกับคำตอบล่าสุด — สูง

หลักฐาน: ARCHITECTURE_PLAN เดิมเสนอ SchoolId/SchoolProfile และชุดกรรมการระดับ store; PROJECT_CONTEXT เดิมมีปุ่มบันทึกโรงเรียน; UX เกณฑ์ท้ายไฟล์เคยระบุว่าโรงเรียนต้องไม่หายเมื่อเพิ่มใหม่ ซึ่งขัด D01/D07/D11/D12

แก้ Markdown ให้ใช้ record snapshot ที่มี school/signers/committee ของตนเอง และ working copy เดียวที่สองแท็บเห็นร่วมกัน ห้าม bind ตรงกับ object ใน saved list หรือให้ record หลายคนอ้าง committee mutable object เดียวกัน คงรายการ custom เช่นรายชื่อเขตได้ แต่ current school/committee ไม่ใช่ global default ของคนถัดไป

ตำแหน่ง/เงิน/งวด Payroll และ position/flags/โรงเรียนสอง/คำสั่ง TOR ของ Procurement เป็นข้อมูล workflow แยกภายใน record เดียว แม้บาง tag สะกดเหมือนกันก็ห้ามถือว่าเป็น field เดียวกันโดยอัตโนมัติ

### R02 ตาราง tag และกลุ่ม 2 โรงเรียนตกหล่น — สูง

พบและแก้ TAG_MAPPING.md แล้ว:
- ที่อยู่ 4 tags พบใน PQ ทั้ง 13 ไฟล์และขั้นตอน 9; ไม่มีเฉพาะถนนใน Procurement
- เลขประจำตัวพบทั้ง PQ และขั้นตอน 9
- PT2/PQ2 ต้องรวม suffix -สอน2รร ด้วย: .1.2/.1.4/.2.2/.2.4 ไม่ใช้ ends-with -2รร เพียงเงื่อนไขเดียว
- Payroll ตารางเดิมขาดชื่อ/นามสกุลหัวหน้าพัสดุ ทั้งที่อยู่ในไฟล์ 5 จริง
- Procurement พบ 56 business tags / 1,043 occurrences; ค่ารวม 1,052 เดิมไม่ตรงกับการสแกนครั้งนี้
- Payroll พบ 34 business tags / 127 occurrences; spelling ใน registry ไม่ตรง actual 3 registry-only และ 1 XML-only ตาม TAG_MAPPING

ไม่สร้าง field ให้ GUID ใน XML metadata และไม่ใช้จำนวน tag อย่างเดียวตัดสิน mapping ถูกต้อง

### R03 Alias ลูกจ้างทั้งไฟล์ทำให้คำนำหน้าผู้ลงนามผิด — สูง

หลักฐาน: ReimbursementDocApp.cs:1491–1497 ใช้ {คำนำหน้าชื่อ} = prefix ลูกจ้างทุก occurrence แต่ XML Payroll 5. บันทึกอนุมัติเบิกจ่าย.docx มี tag นี้ติดกับชื่อลูกจ้าง, ชื่อพัสดุ, ชื่อหพัสดุ และชื่อการเงิน อย่างละหนึ่งจุด Payroll ไฟล์ 4 ใช้กับลูกจ้างอย่างเดียว

ต้องใช้ occurrence mapping ตาม paragraph และ tag ของเจ้าของชื่อก่อน generic replacement ทำบนสำเนาในหน่วยความจำเท่านั้น ไม่แก้ DOCX ต้นทาง การไม่มี spelling {คำนำหน้าหพัสดุ}/{คำนำหน้าการเงิน} ใน XML ไม่ใช่เหตุให้ลบ input เพราะต้องส่งผ่าน contextual alias เหล่านี้

เกณฑ์: ใช้คำนำหน้าต่างกันทั้ง 4 บทบาท และตรวจว่าชื่อเต็มแต่ละบทบาทใน output ถูกต้อง ไม่ใช่ตรวจเพียงไม่มีวงเล็บปีกกา

### R04 Prefix โรงเรียนและ district normalization ยังผิดได้ — สูง

XML Payroll ไฟล์ 5 มี โรงเรียน{ชื่อโรงเรียน} สองจุดและ {ชื่อโรงเรียน} แบบต้องรับชื่อเต็มอีกสามจุด Source NormalizeSchoolName ที่ :1499 เติม prefix ให้ค่าเดียว จึงเสี่ยง โรงเรียนโรงเรียน แม้ข้อมูลผู้ใช้ normalize ถูกแล้ว

คง canonical school name ที่ normalize และส่งค่าที่ตัด prefix เฉพาะ occurrence ที่มี literal prefix นำอยู่ ตาม path/context ที่ตรวจแล้ว ไม่ replace ข้อความเหมารวมทั้งเอกสาร

Source GenerateDocuments :1266–1269 normalize และเขียนกลับ control ก่อนตรวจ required; NormalizeSchoolName("") คืนคำว่า โรงเรียน ทำให้ช่องว่างอาจผ่าน required ต้อง validate ข้อมูลผู้ใช้ที่มีความหมายก่อนเติม prefix และอย่าให้ snapshot normalization แก้ UI โดยไม่ตั้งใจ

NormalizeDistrictName :1510–1530 บังคับ branch ประถมศึกษา จึงทำให้ชื่อเต็มประเภทมัธยมศึกษาผิด ต้องแยกประเภทกับ suffix และทดสอบทุก default ทั้ง 3 รายการ, prefix ซ้ำ, ช่องว่าง และ normalize ซ้ำ ค่า default ลำดับที่ 2 ต้องสะกดประจวบคีรีขันธ์ตาม D03; อย่าอนุมานว่าลำดับที่ 2 ต้องถูกเลือกอัตโนมัติแทนลำดับแรก

### R05 Renderer รองรับ split runs แต่ยังไม่ได้พิสูจน์ formatting — สูง

ส่วนตั้งแต่ RenderDocx จนจบไฟล์ของ workspace ตรงกับ baseline เดิมทุกตัวอักษร ไม่ได้หมายความว่าปลอดข้อบกพร่อง

ReplaceTagsInXml :2145–2189 รวม w:t ทั้ง XML part, ใส่ replacement และ suffix ไว้ใน first node แล้วล้าง touched nodes อื่น:
- suffix ที่เดิมมี font/bold/underline ต่างจาก first run อาจย้าย formatting
- ไทยใน tag อยู่ใน run ที่มี w:cs/font hint ต่างจาก run เครื่องหมาย { จริง เช่นลายเซ็นพัสดุ Payroll ไฟล์ 5; ใช้ brace run เป็น formatting donor โดยอัตโนมัติจึงยังพิสูจน์ความถูกต้องไม่ได้
- การรวมทั้ง part อาจสร้าง match ข้าม paragraph/table cell ที่ไม่ได้เป็น tag เดียวจริง
- CenterSignatureParagraphs :2192 เป็นต้นไปเปลี่ยน paragraph alignment ตามข้อความทั่วไป รวม paragraph ที่ไม่ได้มี tag เช่นบรรทัดวงเล็บ การนำไปใช้ Procurement ต้องตรวจไม่ให้จัดหน้าใหม่โดยปริยาย

เกณฑ์: แทนภายใน text stream ของ paragraph ที่ถูกต้อง (รวม text box paragraphs โดยไม่อ่านซ้ำจาก outer paragraph), คง suffix ที่ node เดิมและคง markup/font ที่ไม่เกี่ยวข้อง; เลือก formatting ของ replacement จาก tag text ที่เหมาะสม; ตรวจ mixed runs, adjacent tags, XML special characters, body/header/footer/text boxes และไม่จับ token ข้าม cell/paragraph เอกสารจริงยังต้อง visual QA

### R06 Manifest/build paths ยังเป็น Payroll ชุดเก่า — สูง

root template_tags.json และ dist/template_tags.json มี hash เท่ากับ baseline registry และระบุเพียง Payroll 5 ชื่อ; ไม่มี Procurement 34 paths ทั้งที่ root Template มี Procurement ทั้งหมด ปัจจุบัน dist มี EXE + JSON สองไฟล์ ไม่มี dist/Template และ release ว่าง จึงยังใช้ artifact เดิมพิสูจน์รุ่นสองแท็บไม่ได้

LoadTemplates :60–80 ยัง fallback parent/current directory; build-installer.ps1:7–12 ค้น dist/root/Desktop/Downloads และ :50 flatten ชื่อไฟล์; Installer.cs:40 อ่าน DOCX เฉพาะชั้นแรก Build-exe ไม่ copy Template

ต้องแยก module manifests/roots พร้อม loader, routing, build และ installer ในการเปลี่ยน path ชุดเดียว ปฏิเสธ absolute path, traversal .. และการ resolve ออกนอก root ไม่ fallback ข้าม module คัดลอก recursive ตาม allowlist 39 ไฟล์ โดย Payroll เลือกเฉพาะ manifest 5 ไฟล์ เพราะ baseline dist/Template มี handover-template.backup-before-rebuild.docx อีกไฟล์หนึ่ง

### R07 Catalog, tests และกฎ Payroll ต้องแยกจาก Procurement — สูง

workspace app_database.json ยังมี Procurement 6 schoolPositions และ DocxSmokeTest.cs:147 บังคับ exactly 6 ซึ่งขัด scope 5; sample ใน smoke :211 ยังเลือกครูผู้ทรงคุณค่า ส่วน baseline มี schoolPositions 11 รายการและ khetPositions 12 รายการใน config โดยหน้าใช้งานหลักใช้ schoolPositions

Payroll ต้องคง 11 schoolPositions และพฤติกรรม baseline ที่ใช้งานอยู่ เก็บ config baseline ที่จำเป็นโดยไม่ขยาย UI ของ khetPositions ใหม่ ห้ามเอา active-5 test ของ Procurement ไปครอบ Payroll

คงกฎ Payroll จาก baseline HANDOFF §9/source: ต.ค.–ธ.ค. fiscal year +1; ก.ย.งวด 12/ต.ค.งวด 1; quick-load เสนอเดือนหลัง successful-period ล่าสุด ไม่ใช่เดือนวันนี้; normal load ไม่เลื่อน; generate ย้อนหลังไม่ลด latest; วันที่ส่งเบิกตามฐานปี ไม่มีข้อมูลต้องไม่เดา; order-date derived text ที่เลือกปีไม่ใช่ 2569 ว่างตาม baseline เดิม ไม่ปรับเป็นปี 2570 โดยเหมา

### R08 การโหลด/บันทึก/เพิ่มใหม่ต้องรักษา record และข้อมูลเมื่อผิดพลาด — สูง

ApplyTemplateData :808–846 เขียนเฉพาะ keys ที่มีใน saved data จึงอาจเหลือค่าคนก่อนเมื่อ key หายหรือ module ไม่มีข้อมูล; CombinedPersonControl.Text setter :1995 เป็นต้นไปเดา prefix และแยกชื่อด้วยช่องว่าง; SaveSavedTemplates :746–778 เขียนไฟล์หลักตรงและการ Save/Overwrite เปลี่ยน list ก่อน disk write; SaveTemplateFlow :927 คืน void ทำให้ผู้เรียกแยก success/cancel/failure ไม่ได้

ต้องโหลดจาก fresh empty model แล้ว apply snapshot ทั้ง shared + workflow ของแต่ละแท็บ; ใช้ typed person fields ไม่ parse ชื่อเต็มเป็น canonical; snapshot ที่บันทึกใช้ ID ภายในและ display name ตามผู้ใช้ยืนยัน; write temp + replace/backup เมื่อสำเร็จ และใช้ JSON parser จริงที่รองรับ quotes/newline/Thai ไม่ใช้ regex เป็น datastore parser

เพิ่มใหม่และปิดต้องรู้ผล Saved / Declined / Cancelled / Failed:
- Saved: บันทึก snapshot ครบทั้งสองแท็บสำเร็จแล้วจึงล้าง record หรือปิด
- Declined: ผู้ใช้เลือกไม่บันทึก จึงล้าง record หรือปิด
- Cancelled/X: กลับไปทำงาน คงค่าทั้งหมด; ปิดกล่องตั้งชื่อระหว่าง save ไม่ถือว่าเลือกไม่บันทึก
- Failed: แจ้งเหตุผลและคงค่ากับข้อมูลบันทึกเดิม; ไม่ล้างหรือปิด
อนุญาต save Template ที่ข้อมูลสร้างเอกสารยังไม่ครบ โดยต้องมีชื่อรายการที่ยืนยัน; ไม่บังคับ generate validation ตอน save

D12 ไม่ได้ยกเลิก quick-load metadata เดิม: normal load/edit ห้ามแก้ saved field snapshot; หลัง quick Payroll generate สำเร็จครบ อนุญาตอัปเดตเฉพาะ latest-period metadata ของ preset ที่ผูกอยู่ ไม่บันทึกฟอร์มทับเอง ไม่เปลี่ยน metadata เมื่อ Procurement generate/เกิด failure/สร้างย้อนหลัง Disconnect quick binding เมื่อเพิ่มใหม่/โหลดคนอื่น/บันทึกเป็นรายการใหม่ และอย่าปรับ metadata ผิดคน

### R09 Identity/update/uninstall ยังไม่ตอบ D04/D10 ใน source — สูง

Installer.cs:10 และ Uninstaller.cs:9 ยังใช้ AppName ReimbursementDocApp ซึ่งเป็น install root และ shortcut ของ baseline จริง การใช้ชื่อเดิมกับรุ่น 346 เสี่ยงโหลดข้อมูลเดิมโดยไม่ตั้งใจและถอน baseline ไปด้วย ต้องมี app identity/root/shortcuts ของ 346 ที่แยกชัดก่อน T02/T07; ชื่อภายในเป็นรายละเอียด implementation ไม่ต้องถามชื่อธุรกิจใหม่

แบบเป้าหมายที่เสนอใน ARCHITECTURE_PLAN: LOCALAPPDATA/Document346 เป็น owned root; App สำหรับ binaries/config/shipped defaults, Data สำหรับ saved snapshots/custom lists, Template สำหรับ user DOCX ทั้งสอง module, Output แยกตาม D09
- fresh 346 install ไม่ import saved_templates.json ของ baseline
- update เปลี่ยน app-owned assets ได้ แต่ user Data/Template/Output ต้องคงอยู่; existing user DOCX ต้องไม่ถูก File.Copy(..., true) ทับอย่าง source :42
- seed user Template เฉพาะไฟล์ที่ยังไม่มี; shipped defaults แยกเป็นของแอป หาก template ที่คงไว้ไม่เข้ากับ manifest ใหม่ให้แจ้งไฟล์ที่ต้องตรวจ ไม่แทนที่เงียบ
- current working directory และโฟลเดอร์ EXE ที่คัดลอกไปที่อื่นต้องไม่เปลี่ยน data owner หรือโหลด baseline

Uninstaller.cs:17 และ :50 มี exact-string check แต่ยังไม่ตรวจ own marker/executable identity/reparse points; :59 ตรวจเพียง .lnk ไม่ตรวจ target; :39 ลบ root รวม executable ที่กำลังรันอยู่ จึงยังเสี่ยงลบไม่ครบหรือรายงานผิด
- ตรวจ canonical exact root ที่คาดจาก Windows user + app identity, marker ของ 346 และ resolved path; ห้าม root ไดรฟ์/LOCALAPPDATA ทั้งหมด/parent/ชื่อคล้ายกัน
- ปฏิเสธหรือจัดการ junction/symlink/reparse อย่างไม่เดินตามไปลบภายนอก ทั้ง root/ancestor/ลูกภายใน
- ลบ shortcut เฉพาะ exact path และ target ของ 346; shortcut ชื่อเหมือนแต่ target อื่นต้องคงอยู่
- ทดลองวิธีลบตนเองหลัง process ออกผ่าน helper ที่จำกัด target เดิม พร้อมรายงานผลจริง; ห้ามฆ่าแอปอื่นหรือลบ root ใดที่ส่งเข้ามาได้อย่างอิสระ
- app ทำงานหรือไฟล์ถูกล็อกต้องแจ้ง/หยุดอย่างปลอดภัย; คงตัวชี้วัดติดตั้งสำเร็จจน copy/validation ครบ
ทดสอบด้วย disposable root ใน workspace ที่เป็นโหมดทดสอบจำกัดเท่านั้น ไม่รัน installer/uninstaller เดิมกับ LOCALAPPDATA จริงในการ review นี้

### R10 Smoke/build อาจตรวจ artifact ผิดหรือผ่านทั้งที่ mapping ผิด — สูง

DocxSmokeTest.cs:53–58 กำหนด outputPath ชื่อคงที่ แต่ RenderDocx :2107 อาจเลือก suffix _2 แล้วคืน void ทำให้ smoke ไปตรวจไฟล์รอบก่อน จึงเกิด false pass ได้ Source GenerateDocuments :1311–1315 ตั้งข้อความ “สร้างเสร็จ” ใน finally แม้จบไม่ครบ

build-exe.ps1 และ build-installer.ps1 ไม่ตรวจ LASTEXITCODE ของ compiler/child build; installer script rebuild EXE ก่อน package และ source archive :80 ใช้ blacklist ทำให้ dist หรือข้อมูลผู้ใช้ใหม่ที่ไม่ได้อยู่ใน blacklist อาจถูกบรรจุ

ต้องได้ actual emitted paths จาก renderer หรือใช้ output directory ใหม่ต่อ test; ตรวจ semantic expected values ด้วย ไม่ใช่แค่ไม่มี tags; complete สำเร็จเมื่อ selected files ครบและทุก ZIP/XML ผ่าน; แยก generation failure ออกจาก metadata save failure และเปิดโฟลเดอร์ไม่ได้

build ต้องหยุดเมื่อ exit code != 0 และห้ามหยิบ EXE เก่ามาใช้; payload allowlist เท่านั้น ใช้ EXE/config/templates ชุดเดียวกับ smoke ผ่าน ถ้า rebuild หรือเปลี่ยน bytes ต้องตรวจ artifact ใหม่นั้นก่อน package

## 4. ลำดับงานแรกสำหรับ Luna Xhigh

1. **T01 revalidate:** อ่าน GRILL และเอกสารที่แก้แล้ว + รายงานนี้; เทียบ hash แถวท้าย ถ้าเท่าเดิมใช้ผล inventory นี้ต่อ ไม่ grill ซ้ำ ตรวจ module manifest และบันทึก expected aliases/occurrence mappings/13 route pairs ก่อนแตะ source
2. **T01 boundary → T02:** เลือกชื่อ internal app identity ของ 346 ให้ไม่ชน baseline บันทึก paths จริง; วาง shared working record, saved snapshots และ workflow ownership ตาม architecture; แยก Payroll baseline ที่จำเป็นลง workspace โดยไม่คัดลอก backup/binary/user data
3. **T02/I02:** แยกสอง module roots/configs, shell และการโหลด config ที่ fail เฉพาะ module; คง Payroll baseline 5 documents/11 positions พร้อม regression; สร้าง persist/load/reset ที่ไม่รั่วข้าม record
4. **T03–T05/I03:** Procurement catalog 5/13 routes/5 หมวด, validation และ full checkbox set 10; เชื่อม shared fields/committee; แก้ mappings R02–R05 และทดสอบ values ก่อน generate
5. **T06/I04:** build ใหม่, smoke ทั้งสอง module, data/UX/negative cases และตรวจ actual output paths; ไม่มีผลผ่านจาก dist เก่า
6. **T07/I05:** visual QA/Windows 10 และ 11/installer update-uninstall ใน isolated test root; package artifact เดียวกับที่ผ่าน และบันทึกข้อจำกัดที่ยังตรวจไม่ได้

R01–R10 เป็น acceptance ของ Txx เดิม ไม่ใช่งานเพิ่มต่างโครงการ ไม่ต้องทำ service/framework/API ใหม่ และไม่ต้องทำทุก refactor ก่อนทดสอบ module แรก

## 5. Acceptance/verification ที่จำเป็น

| ชุด | สิ่งที่ต้องพิสูจน์ |
|---|---|
| V01 Inventory/routing | Source 39 DOCX ครบตาม allowlist; Procurement 13 routes × 10 = 130 outputs; 3/6 จับคู่ถูกทั้ง position/flags; 8 school-two source files; subset 1/หลายไฟล์/0 ไฟล์; invalid flags ไม่ fallback |
| V02 Shared record | กรอกคน A โรงเรียนหนึ่งแล้วแก้ชื่อ/ที่อยู่/ถนน/กรรมการจากทั้งสองแท็บเห็นตรงกัน; โหลดคน B อีกโรงเรียนที่บาง key ว่างแล้วไม่มีค่า A ค้าง; saved A ไม่เปลี่ยน; เพิ่มใหม่ล้าง school/signers/committee/both workflows และ quick binding |
| V03 Save/close | save new/overwrite/rename/normal load/quick load; ยกเลิกตั้งชื่อและเขียนไม่ได้ไม่ทำข้อมูลหาย; ปิดตอน dirty รวมสองแท็บ: Save/Don't Save/X; restart ฟอร์มว่างแต่โหลด saved snapshots ได้; incomplete form save ได้ |
| V04 Store | 0 นำหน้า/Thai/custom prefix/ชื่อมีช่องว่าง/quotes/backslash/newline round-trip; JSON เสีย/empty/truncated/unknown schema ไม่ถูกทับด้วยฐานว่าง; atomic-save failure และ concurrent writer ไม่ทำ snapshot หาย; upgrade data รุ่น 346 คงเดิม |
| V05 Procurement values | mandatory ครบ, ID 12/14/อักษรถูก block; 13 ขึ้นต้น 0 ถูก; optional ว่างเป็นจุดรวมคำสั่ง TOR/ที่อยู่; custom วันเดือน/การศึกษาไม่ออกคำว่า “กรอกเอง”; อายุไม่คำนวณ; เงิน/คำอ่าน/อากรตรง catalog |
| V06 Payroll regression | 5 documents, 11 baseline school positions และ required baseline; เส้น ต.ค./ก.ย. และ ธ.ค./ม.ค.; normal load ไม่เลื่อน; quick load ผูก latest preset; ย้อนหลังไม่ลด latest; missing date ไม่เดา; order year !=2569 คงพฤติกรรมเดิม; สลับแท็บไม่เปลี่ยนงวด |
| V07 Semantic DOCX | prefixes 4 คนต่างกันใน Payroll ไฟล์ 5 ถูกตามบทบาท; โรงเรียนไม่ซ้ำ 2 literal-prefix occurrences และอีก 3 full-name occurrences; กรรมการ A/B/C ชื่อเต็มตรง split names/positions; road เฉพาะ Payroll; address/ID ออกขั้นตอน 9 ด้วย |
| V08 XML/formatting | ZIP CRC + parse XML/.rels ทุก part; ไม่มี business tags ค้างใน document/header/footer รวม text boxes; ไม่แตะ GUID metadata/relationships; fixtures mixed font/bold/underline/suffix/adjacent tags/special XML chars; ไม่สร้าง match ข้าม paragraph/cell; ใช้ donor run ภาษาไทยที่ถูก |
| V09 Failure/output | ชื่อซ้ำ _2/_3 ไม่เขียนทับ; ตรวจไฟล์ที่สร้างครั้งนี้จริง; template หาย/manifest เสีย/module หนึ่งเสียไม่มี cross fallback; locked/readonly/พื้นที่เขียนไม่ได้ไม่มี success ปลอม; partial generation ไม่ advance Payroll และไม่ทำให้เข้าใจว่าครบ |
| V10 Packaging/update | compiler/child exit code ถูกตรวจ; manifest/relative paths/hash ของ artifact และ payload ตรงกัน; allowlist ไม่มี saved data/backup/old dist/ไฟล์ส่วนตัว; update จากชุดเก็บข้อมูลสมมติ+user DOCX แก้เอง+outputs แล้ว hash ของผู้ใช้คงเดิม |
| V11 Uninstall | correct root สำเร็จและลบตนเองครบ; reject parent/sibling/same-prefix/wrong marker/คัดลอก uninstaller; junction ไป external sentinel ต้องไม่แตะ sentinel; shortcut target อื่นคงอยู่; external output/baseline install ไม่เปลี่ยน; locked files รายงานตามจริง |
| V12 Visual/platform | เปิด UI จริง Windows 10/11 ตามความพร้อม; keyboard/DPI/1366×768 100%/1920×1080 125–150%; Visual เอกสารกลาง 8 + variants 26 + Payroll 5 ครบทุกแบบและทุกหน้า ตรวจไทย/font/ตาราง/ลายเซ็น/overflow พร้อมชื่อยาวและ optional ว่าง |

หลักฐาน runtime/visual ที่ยังทำไม่ได้ให้คง NOT_RUN/BLOCKED เฉพาะ gate นั้น ไม่เปลี่ยน requirements เป็น OPEN และไม่รายงาน PASS แทน

ไม่มี Word เป็นกรณีใช้งานที่รองรับ ไม่ block generate DOCX และไม่เพิ่ม COM dependency เพื่อ generate แต่การไม่เกิด exception หรือการ parse XML ผ่านไม่พิสูจน์ layout ถ้าจะรับรองเอกสารพร้อมใช้ต้องเปิด/render ด้วย Word/โปรแกรมอ่าน DOCX ที่มีและเก็บผลตามจริง

ปีที่ hard-code ในขั้นตอน 9 เช่น 27 กุมภาพันธ์ 2569 เป็น **known user-directed exception** ตาม D08/context §10: คง source bytes และไม่ปรับปีเอง V12 ต้องจด exception นี้ไว้ ไม่ตัดสินว่า “ทุกปีเป็น 2570” และไม่ย้อนกลับไปถามเรื่องเดิม

## 6. หลักฐานการตรวจครั้งนี้และข้อจำกัด

ทำจริง: อ่านไฟล์, diff baseline source/config/test ในหน่วยความจำ, SHA-256, เปิด ZIP แบบ read-only, ตรวจ CRC และ parse XML/.rels ของ 39 selected DOCX, ตรวจ tag จาก w:t และบริบทรอบ tag/paragraph

วิธีสแกน: ใช้ bundled Python 26.905.11957 แบบ stdin กับ -X utf8 -B, zipfile + lxml; regex business-token ตรวจเฉพาะข้อความ w:t ใน document/header/footer ไม่จับ XML attributes; เทียบ all-text scan กับ paragraph scan ที่เลือกเฉพาะ nearest paragraph เพื่อไม่ double-count text boxes สองวิธีให้ 1,043 Procurement และ 127 Payroll ตรงกัน ไม่ใช้ภาพที่ render เพื่ออ้างอิงเลขหน้า

ผลจริง: 39/39 selected DOCX ไม่พบ CRC/XML parse error; ไม่มี business tag ใน header/footer ที่มีอยู่ แต่ Payroll ไฟล์ 1 มี header1.xml จึงยังต้องคง renderer/test support; tag ข้าม runs พบใน 39/39 ไฟล์ รูปแบบและ layout ยังไม่ได้รับรอง

ไม่ได้ทำ: compile/build/run EXE, smoke execution, สร้าง/แก้ output DOCX, visual render, installer/uninstaller execution, Windows compatibility run, commit/push หรือแก้ source/config/template/build ใด ๆ

## 7. Inventory พร้อม hash ก่อน implementation

Path ฝั่ง Procurement เป็น relative จาก workspace Template; หลังย้าย root ให้คงส่วน relative นี้ใต้ Template/Procurement ส่วน Payroll เป็น relative จาก baseline dist/Template และคัดลอกไป Template/Payroll เฉพาะ 5 แถวที่ระบุ

| Module | Relative DOCX path | Unique tags | Occurrences | SHA-256 |
|---|---|---:|---:|---|
| Procurement | 1.ขอจ้างต่อเนื่อง 630.docx | 18 | 30 | `ea7f4736076f4076bdd989b0ef5ad088599246c99888c7744de6f63dd901d0e5` |
| Procurement | 2.1 ขอตั้งกกTOR.docx | 24 | 40 | `df481edc32e9978fa13f08ce5c27670213dc865bffaf042fa9ccea7959b7aac0` |
| Procurement | 2.2 คำสั่งTOR.docx | 19 | 28 | `9dec366c196822729fa69b66ae3743b71c47a6c1ff5ba9d9bbae6dd37a929fcd` |
| Procurement | 3.TOR/3.1.1 TORธุรการ9000.docx | 12 | 20 | `c6869efdb1b30e876947c959db76d5134fa1c2971b8a3730f00d3ab7f1c154b9` |
| Procurement | 3.TOR/3.1.2 TORธุรการ9000-2รร.docx | 13 | 22 | `a952e78383676623c78f852a4372dd32c6d1d8639d5431ae0516532126b067d4` |
| Procurement | 3.TOR/3.1.3 TORธุรการ9000-สอน.docx | 12 | 20 | `3b576f66f6be46a8cfbed6b43f65d8bd3578c030076d4ae3d12ee261049bc8f9` |
| Procurement | 3.TOR/3.1.4 TORธุรการ9000-สอน2รร.docx | 13 | 22 | `5c5dba0f4f50950c54924f384174720caba1f0fbc78d4ebc5126138e39a1194a` |
| Procurement | 3.TOR/3.2.1 TORธุรการ15000.docx | 12 | 20 | `6a70c4c359768c095d4a2e9261c6cd304c75eaefa35846bc42ebc545478716c4` |
| Procurement | 3.TOR/3.2.2 TORธุรการ15000-2รร.docx | 13 | 22 | `a16b5570743b5c03feb85c0d027f29f7d612486843af24ba68c72577ee734526` |
| Procurement | 3.TOR/3.2.3 TORธุรการ15000-สอน.docx | 12 | 20 | `0a50b143c86f73da8bceeb32ff7622e5f289b1d2dbaab57e7ecb104b9f414d16` |
| Procurement | 3.TOR/3.2.4 TORธุรการ15000-สอน2รร.docx | 13 | 22 | `ec4607a502a1f3a3c2fd5fb626dc249102c1607c7efa9f1b75f60a95aad208b6` |
| Procurement | 3.TOR/3.3 TORนักการภารโรง.docx | 12 | 20 | `cc24a78c4a44e9536d36da888e11ee63fbec140003888c5c4f38d1512b21b786` |
| Procurement | 3.TOR/3.4.1 TORพี่เลี้ยงเด็กพิการ.docx | 12 | 20 | `ff4de94e6093b173f91930d41d20a0f9861191c14da4a006f62ccdc25e41a9f6` |
| Procurement | 3.TOR/3.4.2 TORพี่เลี้ยงเด็กพิการ-สอน.docx | 12 | 20 | `2f9116470dff0edb54a5a347780ebb78b3a9b17138df06a4510fdc85d4b3a1ab` |
| Procurement | 3.TOR/3.5.1 TORครูพักนอน.docx | 12 | 20 | `73c5f914844ff0cc191487e45e77ed7e6af3de31143d9dd850d3264ac5a17729` |
| Procurement | 3.TOR/3.5.2 TORครูพักนอน-สอน.docx | 12 | 20 | `1b3cfafe36261c71d1cdc856317443946b49cb7a8df389e8a04b8e3e9e3a0ebf` |
| Procurement | 4.รายงานผล TOR.docx | 21 | 43 | `97844c5c7ec928f3d8b1a44d0722c1c0b4d4bd9cefe7f93ddde2be00ac60d1be` |
| Procurement | 5. รายงานขอจ้าง.docx | 13 | 31 | `97ad5cfbf56ad3f4dadfa0f40fe7ed0aa0e4699e453551031a639d51fcbfaa68` |
| Procurement | 6. ใบเสนอราคา/6.1.1 ธุรการ9000.docx | 29 | 38 | `d367323a4c41e6dcd9f0a8516db65ab205594f67d6c953b9db2be41b2f05fb52` |
| Procurement | 6. ใบเสนอราคา/6.1.2 ธุรการ9000-2รร.docx | 30 | 40 | `514b6be1595ece0daf4a7e19e540a003ce3cb5d28fbe7775ec28ef70e22ced35` |
| Procurement | 6. ใบเสนอราคา/6.1.3 ธุรการ9000-สอน.docx | 29 | 38 | `0c20da3c6f2874a87ca63e87b7a0fe30226d6fe6c6e31daefe9d449d6545aaaa` |
| Procurement | 6. ใบเสนอราคา/6.1.4 ธุรการ9000-สอน2รร.docx | 30 | 40 | `7bc28122fcaae3cefb87d396d5721366ba7cbd90981f4f98dec3abb977b8b60e` |
| Procurement | 6. ใบเสนอราคา/6.2.1 ธุรการ15000.docx | 29 | 38 | `f1a20a9f4bed57f8817fde0fd4872da14bb404cdd700be7980821d3628c6a302` |
| Procurement | 6. ใบเสนอราคา/6.2.2 ธุรการ15000-2รร.docx | 30 | 40 | `b9c5c971eb31354b874b621e7449cae0c8bafd7126ab022ba7291908797e0d55` |
| Procurement | 6. ใบเสนอราคา/6.2.3 ธุรการ15000-สอน.docx | 29 | 38 | `9e45fcadce958f515d57b222d173eeb675e8d7fa6265acb63f66e35784a1fa2a` |
| Procurement | 6. ใบเสนอราคา/6.2.4 ธุรการ15000-สอน2รร.docx | 30 | 40 | `06121354fba1a4cfd1188c8ec0f493f0e927be7e4248de2732d7b08b09609491` |
| Procurement | 6. ใบเสนอราคา/6.3 นักการภารโรง.docx | 29 | 38 | `12326de68efa68435d79a3dfbbbd64fdd0eaae9ba7f7145e0f12d64543e5eab4` |
| Procurement | 6. ใบเสนอราคา/6.4.1 พี่เลี้ยงเด็กพิการ.docx | 29 | 38 | `0b6db1f4129f9d4661edf79bcc3f88e0d5991077325e8f97efa81d2e1ff907d1` |
| Procurement | 6. ใบเสนอราคา/6.4.2 พี่เลี้ยงเด็กพิการ-สอน.docx | 29 | 38 | `6f730b5e30aa4e4b411f2e9ab65091cfda29df9bc47105cbd75aa37fb7f44f88` |
| Procurement | 6. ใบเสนอราคา/6.5.1 ครูพักนอน.docx | 29 | 38 | `089e121a404ecbd541bf10ba15814129599c1ef59b8d734a2f2d50ce42127d69` |
| Procurement | 6. ใบเสนอราคา/6.5.2 ครูพักนอน-สอน.docx | 29 | 38 | `d09e1b5437d760bc1790071b1c3b2de2c6e0d19aa481a54a513c9cfbb5ab547d` |
| Procurement | 7. รายงานผลพิจารณา.docx | 18 | 40 | `fe280a361dd4efcf5b3fbea7a11aeddb0fab5c77a47393b4b0f59d5f5ec94fe0` |
| Procurement | 8. ประกาศผู้ชนะ.docx | 10 | 17 | `de3db15f4cb20b1f3f8c2f01170ff05b8035086609d10acefb796361dfe816d6` |
| Procurement | 9. ใบสั่งจ้าง.docx | 17 | 44 | `efa5883472c09f8b6eb7d1d1a56548feb6fc2f25ade715fbfc10a2198e082299` |
| Payroll | 1. หนังสือส่งเบิกจ้างเหมา.docx | 11 | 19 | `562ecb4e590a83b43a4cf1dce6e169637220497dcc606aec9b6e26657a064df7` |
| Payroll | 3. ใบส่งมอบงาน.docx | 19 | 36 | `6b965cc07d135487383274307ebdafc4df57e43fa5e6459895f55265f31593f8` |
| Payroll | 4. ใบตรวจรับ.docx | 15 | 23 | `23a4a2e915b952808098e7f36fcdeb38ccd5dbd33c82e87a89049a2185435f0a` |
| Payroll | 5. บันทึกอนุมัติเบิกจ่าย.docx | 19 | 29 | `4c08371a5b8152aa2115a3ce76be8c3a832391010688b0bd4df73f8004c2615a` |
| Payroll | 8. ใบสำคัญรับเงิน.docx | 13 | 20 | `efe4fb489a18968ee1f982e07e933f86f444da9dedcd95953c63e0337164c1eb` |

Payroll baseline มีไฟล์นอก allowlist: handover-template.backup-before-rebuild.docx ห้าม package โดยนับ DOCX ทั้งโฟลเดอร์

### Hash source/config/build ที่ตรวจ

Path workspace คือ H:\CodexProjects\346; Path baseline คือ C:\Users\MSI\Documents\App creation

| File | Workspace SHA-256 | Baseline SHA-256 |
|---|---|---|
| ReimbursementDocApp.cs | `9c550e907919eb65925192ff5f678dec863164ce93bdb6d6d7ec2b734826fd73` | `af39cda8455b9517453dcdaab6a956b63ab65747de439cc7f72826016878f35a` |
| DocxSmokeTest.cs | `983206bf0b7b93ebda987c7675081b54ed361564965809d65674853e7f0b7061` | `398b033fc2efc9ba004c22d199368a277afb2336571ebbdba301cbbda940431e` |
| app_database.json | `5e972945248b83336dbdc542857f506ffe03a0b24d9e40dbe5a927b2a3649eaa` | `a6d845fabecbdadf8ed046557db21b2783a251f486afa9a8d9f9236ec5ecd573` |
| template_tags.json | `918e6c242ffb6fd684f9b78e13fe3a98bf09207ca656b6efa1c4198acadd07b7` | `918e6c242ffb6fd684f9b78e13fe3a98bf09207ca656b6efa1c4198acadd07b7` |
| build-exe.ps1 | `f36d3e5af095ea852d6ff8f620f3e447db94de76134104469508b51853ca55b8` | `f36d3e5af095ea852d6ff8f620f3e447db94de76134104469508b51853ca55b8` |
| build-installer.ps1 | `4ad6a84ae35c0ea3df4180927be4729094fed1deb6b0f6546cfb5e44c2618079` | `556bcb671592e7370b64ebdd92e472360d25087498763a25c06c7eadfb776bd2` |
| Installer.cs | `f2af0cf7e8fb3de45b7ede2fbda047a0f4da0c7cfa250f4cf4d4f8eca85d8230` | `f2af0cf7e8fb3de45b7ede2fbda047a0f4da0c7cfa250f4cf4d4f8eca85d8230` |
| Uninstaller.cs | `41b0384cf66903faf3ef0f84ad02539c870a5c3760a5b1d379621c4b603e21aa` | `41b0384cf66903faf3ef0f84ad02539c870a5c3760a5b1d379621c4b603e21aa` |

### การแก้เอกสารในรอบนี้

- GRILL_HANDOFF: เพิ่มผล Astra review และยืนยัน READY โดยไม่เปลี่ยน D01–D12
- PROJECT_CONTEXT: ลบข้อเสนอ school-profile/save-school ที่ขัดการเก็บรายบุคคล, อธิบาย snapshot/clear semantics, ชี้ข้อยกเว้น baseline และหลักฐาน review
- ARCHITECTURE_PLAN: กำหนด shared ownership, module states, paths/update/uninstall และ generate acceptance ที่ไม่ขัดกัน
- UX UI design plan: ให้ save เป็น record เดียว, clear ทั้งสองแท็บ, ระบุ cancelled/failed save, แก้เกณฑ์ที่เคยให้คงโรงเรียนตอนเพิ่มใหม่
- TAG_MAPPING: แก้ address/school-two/ID/head-supply entries และ per-occurrence prefix/school handling
- IMPLEMENTATION_HANDOFF/IMPLEMENTATION_PROMPT: เลิกคำสั่ง grill ซ้ำ, ชี้ T01 และเกณฑ์ review, ปรับจำนวน 39 และสองโมดูลให้ชัด
- ASTRA_REVIEW: รายงาน findings/evidence/acceptance/hash นี้

Integrity verification 2026-09-09: เทียบ SHA-256 ก่อน/หลังแก้ Markdown ของ workspace non-Markdown ทั้ง 48 ไฟล์ และ baseline ที่อ่าน 16 ไฟล์ รวม 64 ไฟล์ — changed 0, missing 0; workspace non-Markdown added 0 / removed 0 เปลี่ยนเฉพาะ Markdown เดิม 7 ไฟล์และเพิ่ม ASTRA_REVIEW.md 1 ไฟล์ ไม่แตะ GRILL_PROMPT.md
