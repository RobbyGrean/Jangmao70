# Prompt สำหรับ implementation — เอกสาร 346

ใช้กับ Luna Xhigh / Terra / Sol ได้ คัดลอกข้อความในกรอบด้านล่างเพื่อเริ่มงาน
ไฟล์นี้เป็นคำสั่งเตรียมไว้ การเปิดอ่านระหว่างงานวางแผนไม่ใช่การอนุมัติให้เริ่ม implementation

ลำดับล่าสุด: Grill และ Astra review จบแล้ว `GRILL_HANDOFF.md` คง `READY_FOR_IMPLEMENTATION`; D01–D12 ยืนยันครบ ไฟล์นี้ใช้ในแชท implementation ใหม่ ไม่เริ่ม implementation ภายในรอบ review และไม่ grill ซ้ำ

```text
คุณเป็นผู้รับผิดชอบ implementation โปรแกรม “เอกสาร 346” ใน H:\CodexProjects\346
เป้าหมายคือทำโปรแกรม Windows C# สองแท็บ: เบิกเงินเดือนตามฐานเดิม และจัดซื้อจัดจ้างและสัญญาตามแผนใหม่ โดยรักษาพฤติกรรมงานเก่าและแยก state/template/config ตามส่วนเพิ่มเติมล่าสุดใน ARCHITECTURE_PLAN

เริ่มต้น:
0. อ่าน GRILL_HANDOFF.md ตรวจสถานะ READY_FOR_IMPLEMENTATION และหลักฐานสรุปที่ผู้ใช้ยืนยัน หากยังไม่ครบให้แจ้งว่าต้องจบ grill ก่อน ห้ามลงมือแก้โค้ด ตรวจ TAG_MAPPING.md และ inventory ที่ส่งต่อด้วย
1. อ่าน PROJECT_CONTEXT.md → ARCHITECTURE_PLAN.md → UX UI design plan.md → TAG_MAPPING.md → IMPLEMENTATION_HANDOFF.md → ASTRA_REVIEW.md และ AGENTS.md ถ้ามี
2. ตรวจสถานะไฟล์จริงและคำตอบล่าสุดของผู้ใช้ก่อนแก้ไข แยกสิ่งที่ยืนยันแล้วออกจากข้อเสนอ/ข้อมูลที่ยังขาด
3. สรุปสั้น ๆ ว่างานถึงขั้นไหน จะเริ่ม Txx อะไร และมีคำถามที่บล็อกจริงหรือไม่ แล้วลงมือในขอบเขตที่พร้อม

เมื่อพบข้อมูลใหม่ที่อาจบล็อก:
- D01–D12 ตอบแล้วทั้งหมด ห้ามเริ่มถามเรื่องหลายโรงเรียน/ถนน/กรรมการ/Template scope/add-new/output/Word/update/uninstall ซ้ำ
- ใช้ R01–R10 และ V01–V12 ใน ASTRA_REVIEW เป็นรายละเอียดงานเดิม ข้อคลาดเคลื่อนที่แก้จาก XML/source ได้ให้แก้โดยไม่ถามผู้ใช้แทนการตรวจ
- ถ้าพบ blocker ธุรกิจใหม่จริงที่ตอบจากหลักฐานไม่ได้ ให้บันทึก OPEN พร้อมหลักฐาน งานที่บล็อก และคำถามเฉพาะเรื่อง; หยุดเฉพาะส่วนที่พึ่งคำตอบนั้น
- ห้ามเดาข้อมูลโรงเรียน บุคคล ชื่อเขตทางการ หรือเงื่อนไขงานจ้าง ใช้ข้อมูลสมมติที่ระบุชัดสำหรับทดสอบเท่านั้น
- เมื่อข้อมูลใหม่มาถึง อัปเดต decision และแผนเฉพาะส่วนที่กระทบก่อนทำต่อ ไม่เริ่มวางแผนทั้งหมดใหม่

วิธีทำงาน:
- ทำ T01–T07 ตาม dependency และสถานะใน IMPLEMENTATION_HANDOFF.md ตรวจของที่ทำแล้วก่อนทำซ้ำ
- คง WinForms/compiler เดิม ใช้ ReimbursementDocApp.cs เป็น source หลัก แยกหน้าที่เท่าที่จำเป็น ไม่ rewrite หรือเพิ่ม framework โดยไม่มีเหตุจำเป็น
- ฝั่งจัดซื้อจัดจ้างมี schoolPositions 5 ตำแหน่งและ routing 13 variants ส่วนฝั่งเบิกเงินเดือนคง catalog และ workflow baseline ของตัวเอง ใช้ชื่อไฟล์/tag ตาม inventory ของแต่ละ module ไม่เดาเส้นทาง
- T01 เริ่มด้วยตรวจ hash/boundary/manifest ทั้งสอง module และ app identity 346 ที่ไม่ชน baseline ก่อนแก้ UI; คัดลอกเฉพาะ Payroll source/config ที่จำเป็นและ 5 DOCX ตาม allowlist จาก baseline dist/Template เข้าสู่ workspace ไม่คัดลอก backup/binary/user data
- ใช้ shared working record แต่ saved Template เป็น snapshot เต็มสอง workflows; school/committee อยู่ในแต่ละ record ไม่เป็น global master; save/load/add-new/close ต้องคงข้อมูลเมื่อ cancelled/failed ตาม architecture
- คง DOCX ต้นทาง; แก้ Payroll contextual aliases/school prefix ในหน่วยความจำและทดสอบค่าผู้ลงนามแต่ละคน; แก้ smoke ให้ตรวจ actual emitted paths ไม่อ่านไฟล์รอบก่อน
- ยึดกฎเงิน, validation, optional placeholder, การล้างข้อมูล และ DOCX formatting ตาม PROJECT_CONTEXT
- แก้และตรวจทีละส่วนให้จบ ทำงานต่อได้โดยไม่ขออนุมัติซ้ำในขอบเขตที่ผู้ใช้สั่งแล้ว
- ใช้ test เดิมหรือเพิ่มเฉพาะกรณีที่พิสูจน์พฤติกรรมสำคัญ ทดสอบตามเกณฑ์ของแต่ละ Txx ไม่รันทดสอบซ้ำเมื่อไม่มีการเปลี่ยนแปลงหรือข้อสงสัยใหม่

ข้อห้าม:
- ทุกการแก้ไขอยู่ใน H:\CodexProjects\346 ห้ามแก้/ลบ/สร้างไฟล์ใน C:\Users\MSI\Documents\App creation
- runtime/build/package ใช้ assets ที่คัดลอกและตรวจใน workspace เท่านั้น ไม่มี fallback ไปต้นทาง/Desktop/Downloads/jmoney และไม่ใส่ข้อมูลผู้ใช้จริงใน package
- ไม่ commit/push โดยไม่มีคำสั่ง ไม่แก้ปีใน template ขั้นตอน 9 เอง และไม่ build installer ก่อน smoke test ผ่าน
- ไม่อ้างว่า build/test/visual QA ผ่านหากไม่ได้ตรวจจริง หากขาดเครื่องมือให้ระบุขั้นที่ยังตรวจไม่ได้
- ส่งมอบ template รวม 39 ตาม allowlist; ไม่มี Word ยัง generate DOCX ได้ แต่ ZIP/XML ผ่านไม่เท่ากับ visual QA ผ่าน; installer/update/uninstall ต้องผ่านเกณฑ์ทั้ง data preservation และ exact owned root ใน isolated test root ก่อนใช้จริง

การส่งต่องาน:
- อัปเดต IMPLEMENTATION_HANDOFF.md: สถานะ Txx, decision ที่ยืนยัน, ไฟล์ที่เปลี่ยน, คำสั่งตรวจและผลจริง, blocker และการกระทำถัดไป
- ถ้าข้อกำหนดเปลี่ยน อัปเดต PROJECT_CONTEXT.md ด้วยโดยคงเรื่องที่ยังไม่ตอบให้เห็นชัด
- รายงานกระชับ: ทำอะไรแล้ว / ตรวจอะไรผ่าน / เหลืออะไร ไม่ overthinking ไม่ขยาย scope และไม่เขียนแผนยาวซ้ำ
- ทำต่อจนจบขอบเขตที่ได้รับมอบหมาย หรือถึงจุดที่ต้องใช้คำตอบ/เครื่องมือภายนอกจริง ๆ
```

## คำสั่งสั้นสำหรับเรียกใช้

```text
เริ่ม implementation เอกสาร 346 โดยอ่านและทำตาม H:\CodexProjects\346\IMPLEMENTATION_PROMPT.md
ใช้ข้อมูลล่าสุดใน PROJECT_CONTEXT.md และสถานะใน IMPLEMENTATION_HANDOFF.md ทำต่อจากงานที่ยังไม่เสร็จ ถ้าข้อมูลสำคัญยังขาดให้ถามเฉพาะจุดที่บล็อก กระชับและไม่ขยาย scope
```
