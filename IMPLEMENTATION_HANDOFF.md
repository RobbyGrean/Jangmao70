# เอกสาร 346 — คำสั่งรับช่วงงานสำหรับ Luna Xhigh / Terra / Sol

วันที่: 2026-09-09 | Requirements: READY_FOR_IMPLEMENTATION หลัง Astra review | T01: DONE | T02: IN_PROGRESS | T03: IN_PROGRESS | T04: IN_PROGRESS | T05: IN_PROGRESS | T06: IN_PROGRESS | T07: IN_PROGRESS
Workspace: `H:\CodexProjects\346`

เพิ่มเติม 2026-09-08: อ่านแผนสองแท็บที่ต้น ARCHITECTURE_PLAN ก่อนทำ T01–T07 ขยาย T01 ให้ตรวจ Payroll baseline และวาง shell/module boundary ก่อนงาน UI/data; T06/T07 ต้องตรวจทั้ง Payroll และ Procurement โดยเกณฑ์ 34 templates/13 variants เดิมเป็นของ Procurement เท่านั้น ยังไม่มีงาน implementation เสร็จจากการเพิ่มแผนครั้งนี้

## 1. Prompt สำหรับเริ่มงานครั้งต่อไป

**ลำดับล่าสุดจากผู้ใช้:** Grill จบแล้ว D01–D12 ยืนยันครบ; Astra review นี้ตรวจ source/DOCX แบบ read-only และปรับเฉพาะ Markdown ไม่เริ่ม implementation ผล review คง READY_FOR_IMPLEMENTATION และไม่มี OPEN ที่ต้องถามเพิ่ม แชท implementation ถัดไปอ่าน `ASTRA_REVIEW.md` และเริ่ม T01 โดยใช้หลักฐานเดิมต่อเมื่อ hash ตรง

ข้อความด้านล่างใช้เฉพาะแชท implementation หลัง `GRILL_HANDOFF.md` เป็น `READY_FOR_IMPLEMENTATION` แล้ว:

มี prompt แบบพร้อมใช้และคำสั่งเรียกสั้นใน `IMPLEMENTATION_PROMPT.md` สำหรับแนะนำบทบาทและวิธีทำงานของผู้รับช่วง implementation

> อ่าน GRILL_HANDOFF.md ก่อน ตามด้วย PROJECT_CONTEXT.md, ARCHITECTURE_PLAN.md, UX UI design plan.md, TAG_MAPPING.md, IMPLEMENTATION_HANDOFF.md, IMPLEMENTATION_PROMPT.md และ ASTRA_REVIEW.md ใน H:\CodexProjects\346 ใช้ D01–D12 ที่ยืนยันแล้ว ตรวจ hash และทำ T01–T07 จากงานที่ยังไม่เสร็จพร้อมเกณฑ์ V01–V12 ถ้าพบ blocker ใหม่จริงให้บันทึก OPEN พร้อมหลักฐานและถามเฉพาะส่วนนั้น ไม่ rewrite/ขยาย scope/แก้ source ต้นฉบับ/commit/push หรือเปลี่ยนปี template ขั้นตอน 9 เอง การเปิดอ่าน prompt ในรอบ review ไม่อนุมัติให้เริ่ม implementation

## 2. ข้อมูลที่ยืนยันแล้วและจุดตัดสินใจ

ทุกแถว D01–D12 เป็น CONFIRMED ตาม GRILL_HANDOFF.md วันที่ 2026-09-08/09 ไม่มีคำตอบเดิมที่ยังรอ; หากเกิดเรื่องใหม่ให้เพิ่ม ID ใหม่และสถานะ OPEN เฉพาะเรื่องนั้น ห้ามย้อนแถวเดิมเป็น OPEN โดยไม่มีหลักฐานใหม่

| ID | เรื่องที่ต้องได้คำตอบ | งานที่รอ / วิธีดำเนินการ |
|---|---|---|
| D01 | เครื่องเดียวบันทึกลูกจ้างจากหลายโรงเรียนได้; `{ชื่อโรงเรียน}` อยู่ใน record การจ้างรายบุคคล ไม่ทำ active-school เดียวหรือเมนูสลับโรงเรียนในรุ่นนี้ | ใช้กับ T02/T04; ไม่สร้าง UX สลับโรงเรียน |
| D02 | ข้อมูลผู้ใช้แยกตามบัญชี Windows ใต้ `%LOCALAPPDATA%`; ไม่แชร์ทุก user และไม่ทำ portable data ในรุ่นนี้ | ใช้กับ T02/T06/T07; ออกแบบ data root ต่อ Windows user |
| D03 | ค่าเริ่มต้นชื่อเขตคือ `สำนักงานเขตพื้นที่การศึกษาประถมศึกษาแม่ฮ่องสอน เขต 2`; ผู้ใช้เขตอื่นพิมพ์แก้ไขในช่องเขตได้เอง และไม่แสดงรายชื่อเขตอื่นเป็นตัวเลือกตั้งต้น | ใช้กับ T02/T04; คงช่องเขตเป็นแบบพิมพ์แก้ไขได้ |
| D04 | เริ่มข้อมูลใหม่ ไม่ย้าย record เก่าชื่อ “มั่วทดสอบ” จาก `saved_templates.json` baseline | ใช้กับ T02/T06; ไม่ทำ legacy preset migration และไม่ใส่ข้อมูลทดสอบใน payload |
| D05 | `{คำสั่งสเปค}` เว้นว่างได้ ให้เอกสารแสดงจุดว่าง และค่าที่กรอกจำกับ record การจ้างลูกจ้างรายบุคคล | ใช้กับ T02/T04/T05; ไม่ block การสร้างเพราะค่าว่าง |
| D06 | แสดงช่องป้าย `ถนน` ในข้อมูลส่วนตัว/ที่อยู่ของ record ลูกจ้างตั้งแต่ต้น; กรอกได้แต่ไม่บังคับและจำรายบุคคล; `{ถนนลูกจ้าง}` เป็น tag ภายในที่ Payroll ใช้ ส่วน Procurement ไม่ส่งออก tag นี้ | ใช้กับ T02/T04/T05; ไม่เพิ่ม tag ใน Procurement template |
| D07 | ข้อมูล record ลูกจ้างรายบุคคลใช้ร่วมกันระหว่าง Payroll/Procurement และแก้ในแท็บหนึ่งต้องเห็นอีกแท็บทันที; ข้อมูลเฉพาะ workflow แยก | ใช้กับ T02/T04; ต้องกำหนด field ownership ให้ครบก่อนทำ UI |
| D08 | Procurement ใช้ template 34 ไฟล์ที่ตรวจพบ; เอกสาร “สัญญา” คือ `9. ใบสั่งจ้าง.docx` ในชุดกลาง ไม่เพิ่มแบบสัญญาอื่น; checkbox เริ่มต้นติ๊กทุกไฟล์แต่ยกเลิกเลือกบางไฟล์ได้ | ใช้กับ T03/T04/T06/T07; catalog, routing และ packaging |
| D09 | เปิดโปรแกรมที่แท็บ `จัดซื้อจัดจ้างและสัญญา`; เมื่อปิดขณะมีข้อมูลค้างให้มีตัวเลือก `บันทึก` หรือ `ไม่บันทึก` และใช้กากบาทของกล่องถามเพื่อกลับไปทำงานต่อ; เปิดครั้งใหม่เป็นฟอร์มว่าง ไม่กู้ร่างอัตโนมัติ; ค่าเริ่มต้น output คือ `Output/เงินเดือน` และ `Output/เอกสารจัดจ้าง`; ชื่อซ้ำต่อท้าย `_2`, `_3` อัตโนมัติ | ใช้กับ T02/T04/T06; shell workflow, output และ close guard |
| D10 | รองรับ Windows 10/11; update ต้องเก็บข้อมูลลูกจ้าง/Template/Output เดิม; uninstall ลบได้ตามเจตนาเจ้าของแต่ต้องตรวจและลบเฉพาะ install root/shortcut ของแอป ไม่แตะ path อื่น; ไม่มี Word ก็สร้าง DOCX และตรวจ ZIP/XML ได้ แต่เปิดดูต้องใช้ Word/โปรแกรมอ่าน DOCX บนเครื่องอื่น | ใช้กับ T06/T07; installer/uninstaller และ Word handling |
| D11 | กรรมการ TOR และกรรมการตรวจรับเป็นบุคคลชุดเดียวกัน ลำดับ A/B/C เดียวกัน (A = ประธาน); map จากข้อมูลต้นทางร่วมไปยัง tag คนละชุด | ใช้กับ T05; คง `{กรรมการA/B/C}` และ `{คำนำหน้าสเปคA/B/C}` ตาม template เดิม |
| D12 | “เพิ่มลูกจ้างใหม่” ถามก่อนว่าจะบันทึก Template ตามชื่อ–นามสกุลหรือไม่; ใช้ flow บันทึก Template เดียวกับปุ่มหลักเมื่อเลือกบันทึก; ไม่ว่าบันทึกหรือไม่ให้ล้างฟอร์มของ record ปัจจุบันทั้งหมด; โหลด Template ต้องแจ้งว่าไม่แก้ record เดิมจนกว่าจะสั่งบันทึกทับชื่อเดิม | ใช้กับ T02/T04; add-record, clear และ save/load messaging |

แบบบันทึกคำตอบ: `Dxx | CONFIRMED | คำตอบ | วันที่/ที่มา | กระทบ Txx | แก้เอกสารแล้ว`

หากมีคำตอบใหม่ในอนาคต: บันทึกหลักฐาน → ปรับแผนเฉพาะส่วนที่กระทบ → ทำงานถัดไป ไม่เริ่มถามทั้งหมดใหม่หรือรีเซ็ตงานที่เสร็จแล้ว เรื่องที่แก้จากหลักฐาน source/XML ได้ให้ทำใน scope เดิมโดยไม่ถามผู้ใช้แทนการตรวจไฟล์

## 3. งานตามลำดับและเกณฑ์ตรวจรับ

| งาน | สิ่งที่ทำ | ผ่านเมื่อ | สถานะ |
|---|---|---|---|
| T01 ตรวจ baseline | revalidate hash/inventory ใน ASTRA_REVIEW, กำหนด manifest/13 route pairs/occurrence aliases และ app identity/module boundary ทั้งสองแท็บ | 39-file allowlist และ field/path ownership ชัด; baseline ไม่เปลี่ยน; ไม่ถือ review เป็น build/smoke ผ่าน | DONE |
| T02 Record/store | shared working record + saved snapshots ทั้งสอง workflows, JSON store/custom lists ต่อ Windows user; แยกจาก baseline | restart ฟอร์มว่างแต่โหลด snapshot ได้, 0 ไม่หาย, missing keys ไม่เหลือค่าคนก่อน, JSON/save failure คงข้อมูล; ไม่มี legacy import; update 346 รักษาของเดิม | IN_PROGRESS |
| T03 Catalog/routing | active 5 ตำแหน่ง, mapping เงิน, route 13 คู่, checkbox และโรงเรียนสอง | ทุก variant ชี้ exact path ถูก; invalid flags ถูกปฏิเสธ; ปิด flag แล้วค่าเก่าไม่รั่ว | IN_PROGRESS |
| T04 UI/validation | 5 sections, dropdown ตาม context, กรรมการ A/B/C, save/load/reset, optional fields, add-record save prompt | mandatory ขาดสร้างไม่ได้; optional ว่างเป็นจุด; เพิ่มลูกจ้างใหม่ถามก่อนเซฟ Template ตามชื่อ–นามสกุล แล้วล้างฟอร์มปัจจุบันทั้งหมดไม่ว่าผู้ใช้จะเซฟหรือไม่ และโหลด Template แจ้งว่าไม่แก้ record เดิมจนกว่าจะบันทึกทับ | IN_PROGRESS |
| T05 Tags/render | ผูก actual tags แยกสอง manifests, แก้ occurrence alias/prefix และ renderer ตาม R02–R05; D05/D06 ยืนยันแล้ว | semantic values ถูกบุคคล/โรงเรียน/variant; body/header/footer/text boxes ไม่มี tag; formatting/suffix ถูก; ไม่มี `(กรอกเอง)` | IN_PROGRESS |
| T06 Build/smoke | copy Template recursive; เอา fallback นอก workspace และ jmoney ออกจาก packaging; ทดสอบ artifact ใหม่ | ทุกกรณีใน §4 ผ่าน; compiler/test exit code ถูกตรวจ; payload ไม่มีข้อมูลผู้ใช้ | IN_PROGRESS |
| T07 Visual/installer | visual ทั้ง 39 แบบ/Windows 10–11 และ build installer จาก artifact ที่ smoke ผ่าน; fresh/update/uninstall ใน test root ของ workspace | 39 templates ครบ, layout/ภาษาไทยผ่านจริง, user data/template/output คงหลัง update; exact own root/shortcuts เท่านั้นถูกลบ; ไม่มี Word ยัง generate ได้ | IN_PROGRESS |

ไม่ต้องเพิ่ม test framework ใหม่หากขยาย DocxSmokeTest.cs ได้ ไม่ต้องออกแบบ API/service เพิ่ม และไม่ใช้ dotnet build โดยเดาว่าเป็น SDK project

## 4. ชุดตรวจรับที่จำเป็น

ใช้ **V01–V12 ใน ASTRA_REVIEW.md** เป็นรายละเอียดตรวจรับทั้งสองแท็บ รายการต่อไปนี้เดิมเน้น Procurement และห้ามใช้ลดขอบเขต Payroll:

- 13 routing variants สร้างชุดเต็มอย่างละ 10 ไฟล์ = 130 ไฟล์ทดสอบ; ครอบคลุม template ต้นทางทั้ง 34 ไฟล์ และตรวจ path/position/เงิน/โรงเรียนสองจริง ไม่ใช่แค่นับไฟล์
- ขาดชื่อ/นามสกุล/เลขประชาชน/โรงเรียน/ตำแหน่ง/กรรมการอย่างใดอย่างหนึ่ง → block; เลข 12/14 หลักหรือมีอักษร → block; 13 หลักขึ้นต้น 0 → เก็บและส่งออกครบ
- optional ว่าง, วัน/เดือนเลือกกรอกเอง, custom คำนำหน้า/คุณวุฒิ → แสดงค่าจริงหรือจุดตาม context; อายุไม่ถูกคำนวณเอง
- prefix ซ้ำ, เปลี่ยนตำแหน่งหลังติ๊ก flags, ปิดโรงเรียนสอง, เพิ่ม/โหลดลูกจ้างสลับคน → ไม่มีค่าค้างผิดคน/variant
- JSON save/load หลังปิดเปิดเป็นฟอร์มว่างจนผู้ใช้โหลด, update schema ของ 346 ซ้ำไม่สร้างข้อมูลซ้ำ, ไม่มี legacy import; ไฟล์เสีย/เขียนไม่ได้/cancel save → แจ้งตามจริงและรักษาข้อมูลเดิม ไม่ล้างฟอร์ม
- ทุก generated DOCX: ตรวจ body/header/footer รวม tag ที่ข้าม runs; synthetic fixture สำหรับตำแหน่งที่ template จริงยังไม่มี tag ใช้ข้อมูลทดสอบแทนข้อมูลจริง
- Visual QA เอกสารกลาง 8 + TOR/ใบเสนอราคา 26 + Payroll 5 แบบ ครบทุกหน้า: ตาราง, ลายเซ็น, font ไทย, หน้าเกิน/ข้อความล้น, ชื่อยาวและ optional ว่าง ถ้าไม่มีเครื่องมือ render ให้บันทึก BLOCKED เฉพาะ visual QA ห้ามอ้างว่าผ่านหรือบังคับผู้ใช้ติด Word เพื่อ generate
- Installer payload: relative paths และ hash ตรงกับ artifact ที่ smoke ผ่าน, มี 39 templates ตาม allowlist, ไม่มี fallback/source เก่าหรือ saved profiles จริง; ปีขั้นตอน 9 เป็นข้อยกเว้นตามผู้ใช้ ไม่แก้โดยพลการ

## 5. วิธีทิ้งงานให้โมเดลถัดไป

เมื่อจบแต่ละรอบ อัปเดตแถว Txx เป็น TODO / IN_PROGRESS / BLOCKED / DONE แล้วเติมบันทึกสั้นนี้:

```text
วันที่:
ทำเสร็จ: Txx — ไฟล์/พฤติกรรมที่เปลี่ยน
ตรวจแล้ว: คำสั่งจริง + ผล + path หลักฐานใน workspace
ยังไม่ตรวจ:
Decision ที่เพิ่ม/เปลี่ยน:
งานถัดไป: Txx — การกระทำแรกที่ชัดเจน
Blocker: ถ้ามี ระบุคำตอบ/ไฟล์ที่ขาด และบล็อกส่วนไหน
```

สถานะหลัง Astra review 2026-09-09: อ่านเอกสาร/source/build/test, diff baseline และตรวจ ZIP/XML/hash ของ 39 selected DOCX แล้ว; implementation T01 ทำซ้ำการตรวจบนไฟล์จริงและบันทึก module boundary/manifest แล้ว

บันทึก implementation 2026-09-09:
- T01 DONE: workspace hash เทียบค่าใน ASTRA_REVIEW ครบ 0 mismatch; source baseline ภายนอก `ReimbursementDocApp.cs`/`DocxSmokeTest.cs` ตรงค่าเดิม 0 mismatch
- ย้าย Procurement DOCX 34 ไฟล์ไป `Template/Procurement/` และคัดลอก Payroll manifest 5 ไฟล์ที่อนุญาตไป `Template/Payroll/`; ไม่คัดลอก backup/user data/binary จาก baseline
- เพิ่ม `Config/app_identity.json` กำหนด app identity `Document346`, runtime roots, Data/Template/Output ownership และ output แยกสองโมดูล
- เพิ่ม `Config/Payroll/template_manifest.json`, `Config/Payroll/occurrence_mappings.json`, `Config/Procurement/template_manifest.json` และ `Config/Procurement/position_catalog.json`; manifest ตรวจ hash/relative path/route 13 คู่ครบ
- เพิ่ม `verify-manifest.ps1`; ผลจริง: `Manifest OK: Payroll=5, Procurement=34, Routes=13, AppId=Document346`; scan จริง Procurement 56 tags/1,043 occurrences และ Payroll 34 tags/127 occurrences
- T01 ยังไม่ถือเป็นหลักฐาน build/smoke/visual/installer ผ่าน

บันทึก implementation เพิ่มเติม 2026-09-09:
- T02/T03/T04/T05 IN_PROGRESS: เพิ่ม `Shared/Document346Core.cs`, `Shared/Document346Config.cs`, `Shared/DocxRenderer.cs`, `Modules/Procurement/ProcurementModule.cs` และ `Document346Shell.cs`; shell มี 2 tabs โดยเปิด Procurement และใช้ WorkingRecord ร่วมกับ Payroll baseline
- แก้ Payroll renderer ไฟล์ 5 ให้เลือก `{คำนำหน้าพัสดุ}`, `{คำนำหน้าหพัสดุ}`, `{คำนำหน้าการเงิน}` ตาม paragraph จริง และถอด literal `โรงเรียน` ก่อนแทน `{ชื่อโรงเรียน}`; คง split-run/suffix handling
- แก้ clear section ไม่ล้าง date fields ของ section อื่น และรองรับเดือนภาษาไทย/ค่า custom โดยไม่ส่ง sentinel `(กรอกเอง)` ออกไป
- T06 smoke จริง: `run-smoke-346.ps1` ผ่าน render Procurement 34/34 + store/clone; `run-shell-smoke-346.ps1` ผ่าน; `run-payroll-smoke-346.ps1` ผ่าน; `verify-manifest.ps1` ผ่าน; `build-exe-346.ps1` ผ่าน และ artifact ตรวจ Payroll 5/Procurement 34
- T07 IN_PROGRESS: เพิ่ม `Document346Installer.cs`, `Document346Uninstaller.cs`, `build-installer-346.ps1`; installer zip รุ่นล่าสุดใช้ชื่อ `release/Jangmao70-Installer.zip`, payload ตรวจพบ DOCX 39 และ Config/Template ไม่มี nested duplicate/fallback; ยังไม่ได้รับรอง visual QA หรือ execute fresh/update/uninstall จริง
- แก้ packaging ให้ copy Config/Template ลงตรง root ไม่สร้าง `Config/Config` หรือ `Template/Template`; เพิ่ม `template_tags.json` และ `app_database.json` ใน payload เพราะ Payroll baseline ที่ shell โฮสต์ยังอ่านไฟล์สองรายการนี้ข้าง EXE
- เพิ่ม `USER_README_346_TH.txt` ให้ตรงกับ app identity `Document346`, สองแท็บ, output แยก workflow และขั้นตอน add/load/save Template
- Regression ล่าสุด: `run-smoke-346.ps1` ผ่าน 34/34 Procurement + catalog/routes/store/clone; `run-shell-smoke-346.ps1` ผ่าน; `run-payroll-smoke-346.ps1` ผ่าน; `build-installer-346.ps1` ผ่าน และ payload ตรวจ DOCX 39/39
- ปรับตามคำขอล่าสุด: Procurement (`จัดซื้อจัดจ้างและสัญญา`) เป็นแท็บแรกและแท็บเริ่มต้น (`SelectedIndex = 0`); Payroll เป็นแท็บที่สอง; shell smoke ตรวจลำดับและ initial tab แล้ว
- Astra Light review รอบล่าสุด: ไม่พบ blocker; พบ P1 committee position หายตอน Payroll sync และ P2 Payroll state restore/renderer loop risk แล้วแก้ใน `Document346Shell.cs` และ `Shared/DocxRenderer.cs`; เพิ่ม shell/core regression ให้ตรวจทั้งสามประเด็นโดยไม่ re-review วนซ้ำ
- เปลี่ยนชื่อโปรแกรมตามคำขอเป็น `Jangmao70` ใน title, EXE, shortcut, installer, readme และ `Config/app_identity.json.productName`; คง internal app/data identity `Document346` ไว้เพื่อรักษาข้อมูลเดิมระหว่าง update

งานถัดไป: T06/T07 — เพิ่ม acceptance smoke สำหรับ route/validation และทำ visual/fresh-update-uninstall QA ใน test root ที่แยกจากข้อมูลผู้ใช้จริง

Blocker: ไม่มี OPEN ที่ต้องถามผู้ใช้; R01–R10 เป็นข้อที่ต้องแก้ใน T01–T07 ตาม scope เดิม และยังไม่ถือว่า source ได้แก้แล้ว

PROJECT_CONTEXT ที่ใช้วางแผน: อัปเดตในไฟล์ 2026-09-07; SHA-256 `7270C5394907C7F7CBCEFF07DF0DA55339657104DA8458BF53E28B39AD90A4DD` ใช้สังเกตว่าข้อมูลเปลี่ยน ไม่ใช่เหตุให้ปฏิเสธ context ใหม่
