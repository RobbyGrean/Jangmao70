# เอกสาร 346 — สถานะ Grill

สถานะ: READY_FOR_IMPLEMENTATION — Requirements confirmed
อัปเดต: 2026-09-09
ลำดับที่ผู้ใช้กำหนด: แชทถัดไปทำ grill เท่านั้น → แจ้งเมื่อข้อมูลครบ → ผู้ใช้เปิดแชทใหม่เพื่อ implementation
ผู้ใช้ยืนยันสรุปขอบเขตทั้งหมดแล้วเมื่อ 2026-09-09; แชทนี้จบที่ grill และห้ามเริ่มแก้โปรแกรม

## จุดเริ่ม

Grill จบแล้ว D01–D12 ยืนยันครบ และผ่าน Astra review ด้านเอกสาร 2026-09-09 แชท implementation อ่าน `ASTRA_REVIEW.md` เพิ่มจากชุดเดิมแล้วเริ่ม T01 ไม่เริ่ม GRILL_PROMPT/D01 ซ้ำ การอ่าน handoff ในรอบ review นี้ไม่ใช่คำสั่งให้เริ่ม implementation

## ทะเบียนคำตอบ

D01–D10 และ D11–D12 ใช้รายละเอียดจากคำตอบล่าสุดในรอบ Grill และหลักฐานไฟล์จริง
ตอนเริ่ม grill ให้เทียบหลักฐานจริงก่อนกำหนดสถานะของแต่ละรายการ

| ID | สถานะ | คำตอบหรือข้อเท็จจริง | วันที่/ที่มา | ผลกระทบ/งานที่รอ |
|---|---|---|---|---|
| D01 | CONFIRMED | เครื่องเดียวบันทึกลูกจ้างจากหลายโรงเรียนได้; ชื่อโรงเรียนเป็น field `{ชื่อโรงเรียน}` ใน record การจ้างรายบุคคล ไม่ทำ active-school เดียวหรือเมนูสลับโรงเรียนในรุ่นนี้ | 2026-09-08 / คำตอบผู้ใช้ในแชท grill | T02/T04: data model, save/load และ UI รายบุคคล |
| D02 | CONFIRMED | แยกข้อมูลตามบัญชี Windows ใน `%LOCALAPPDATA%`; ไม่แชร์ทุก user และไม่ทำ portable data ในรุ่นนี้ | 2026-09-08 / คำตอบผู้ใช้ในแชท grill | T02/T06/T07: data root, update และ installer |
| D03 | CONFIRMED | ชื่อเขต default: `สำนักงานเขตพื้นที่การศึกษาประถมศึกษาประจวบคีรีขันธ์ เขต 2` | 2026-09-08 / คำตอบผู้ใช้ในแชท grill | T02/T04: district defaults และ normalization |
| D04 | CONFIRMED | เริ่มข้อมูลใหม่ ไม่ย้าย record เก่าชื่อ “มั่วทดสอบ” เข้าโปรแกรมรุ่นนี้ | 2026-09-08 / คำตอบผู้ใช้ + ตรวจไฟล์จริง | T02/T06: ไม่ทำ legacy preset migration และไม่ใส่ข้อมูลทดสอบใน payload |
| D05 | CONFIRMED | `{คำสั่งสเปค}` เว้นว่างได้ ให้เอกสารแสดงจุดว่าง และถ้ากรอกให้จำกับ record การจ้างลูกจ้างรายบุคคล | 2026-09-08 / คำตอบผู้ใช้ในแชท grill | T02/T04/T05: optional validation, save scope และ placeholder |
| D06 | CONFIRMED | มีช่อง `{ถนนลูกจ้าง}` ให้กรอกตั้งแต่หมวดข้อมูลส่วนตัว/ที่อยู่ของ record ลูกจ้าง; ไม่บังคับและจำรายบุคคล ใช้กับ Payroll ที่มี tag นี้ ส่วน Procurement ไม่แสดง tag ในเอกสาร | 2026-09-09 / คำตอบผู้ใช้ + สแกน DOCX/source จริง | T02/T04/T05: shared field, optional validation และ Payroll mapping |
| D07 | CONFIRMED | ข้อมูล record ลูกจ้างรายบุคคลใช้ร่วมกันข้าม Payroll/Procurement และแก้ในแท็บหนึ่งต้องเห็นอีกแท็บทันที; อย่างน้อยชื่อ เลขประชาชน ที่อยู่ และชุดกรรมการใช้แหล่งข้อมูลเดียวกัน โดยข้อมูลเฉพาะ workflow ของแต่ละแท็บยังแยก | 2026-09-08 / คำตอบผู้ใช้ในแชท grill | T02/T04/T05: shared record, sync และ field ownership |
| D08 | CONFIRMED | ขอบเขต Procurement ใช้ template 34 ไฟล์ที่ตรวจพบ; คำว่า “สัญญา” หมายถึงไฟล์กลาง `9. ใบสั่งจ้าง.docx` ที่มีอยู่แล้ว ไม่เพิ่มแบบสัญญาอื่น; การสร้างเอกสารเริ่มต้นติ๊กทุกไฟล์ แต่ผู้ใช้ยกเลิกเลือกบางไฟล์ได้ | 2026-09-09 / คำตอบผู้ใช้ + inventory DOCX จริง | T03/T04/T06/T07: catalog, checkbox, routing และ packaging |
| D09 | CONFIRMED | เปิดโปรแกรมที่แท็บชื่อเดิม `จัดซื้อจัดจ้างและสัญญา`; เมื่อปิดขณะมีข้อมูลค้างให้มีตัวเลือก “บันทึก” หรือ “ไม่บันทึก” และใช้กากบาทของหน้าต่างเพื่อปิดกล่องถามกลับไปทำงาน; เปิดครั้งใหม่เป็นฟอร์มว่างเสมอ ไม่กู้ร่างอัตโนมัติ; output แยกเป็น `Output/เงินเดือน` และ `Output/เอกสารจัดจ้าง`; ชื่อไฟล์ซ้ำต่อท้าย `_2`, `_3` อัตโนมัติ | 2026-09-09 / คำตอบผู้ใช้ในแชท grill + ตรวจ baseline | T02/T04/T06: shell workflow, output และชื่อไฟล์ |
| D10 | CONFIRMED | รองรับ Windows 10 และ Windows 11; การอัปเดตต้องเก็บข้อมูลลูกจ้าง/Template/Output เดิมทั้งหมด; การถอนติดตั้งลบข้อมูลของโปรแกรมได้ตามเจตนาเจ้าของ แต่ต้องตรวจ root path แบบ exact และลบเฉพาะโฟลเดอร์/shortcut ของโปรแกรม ไม่แตะไฟล์หรือโฟลเดอร์อื่น; ไม่มี Word ก็ยังสร้าง DOCX ได้ ตรวจได้เฉพาะความถูกต้องของ ZIP/XML และเปิดดูด้วย Word/โปรแกรมอ่าน DOCX บนเครื่องอื่นเมื่อมี | 2026-09-09 / คำตอบผู้ใช้ในแชท grill + ตรวจ Installer/Uninstaller baseline | T06/T07: compatibility, update/uninstall safety และ Word handling |
| D11 | CONFIRMED | กรรมการ TOR และกรรมการตรวจรับเป็นบุคคลชุดเดียวกัน; map A→A, B→B, C→C โดยใช้ข้อมูลต้นทางชุดเดียว แล้วส่งออกคนละชื่อ tag ตาม template | 2026-09-08 / คำตอบผู้ใช้ + ตรวจ source/template จริง | T05: tag mapping/renderer; ไม่แก้ tag ใน Word |
| D12 | CONFIRMED | เมื่อกด “เพิ่มลูกจ้างใหม่” ให้ถามก่อนว่าจะบันทึกข้อมูลปัจจุบันเป็น Template ของ `{ชื่อลูกจ้าง}` `{นามสกุลลูกจ้าง}` หรือไม่; ไม่ว่าบันทึกหรือไม่บันทึก หลังจบคำถามให้ล้างข้อมูลฟอร์มของ record ปัจจุบันทั้งหมดแล้วเริ่ม record ใหม่; ถ้าบันทึกให้ใช้ flow เดียวกับปุ่มบันทึก Template และให้ผู้ใช้ยืนยันชื่อไฟล์/ชื่อรายการ; การโหลด Template ต้องแจ้งว่าเป็นการนำข้อมูลมาใช้กับฟอร์มและไม่แก้ record เดิม เว้นแต่ผู้ใช้สั่งบันทึกทับชื่อเดิม | 2026-09-09 / คำตอบผู้ใช้ในแชท grill | T02/T04: add-record, clear, save/load template และข้อความเตือน |

## บันทึกรอบ grill

- 2026-09-08: ผู้ใช้สั่งอ่านและทำตาม GRILL_PROMPT.md เก็บคำตอบลงไฟล์ และห้ามเขียนโค้ดในแชทนี้
- อ่าน PROJECT_CONTEXT.md → ARCHITECTURE_PLAN.md → UX UI design plan.md → IMPLEMENTATION_HANDOFF.md และ GRILL_HANDOFF.md ตามลำดับแล้ว
- ไม่พบ AGENTS.md ที่ H:\\, H:\\CodexProjects\\ และ workspace root; การค้นไฟล์ภายใน workspace ไม่พบ AGENTS.md
- เริ่มคำถาม D01 ตามถ้อยคำที่กำหนด ยังไม่ถือข้อเสนอในแผนเป็นคำตอบผู้ใช้
- ตรวจ baseline `C:\Users\MSI\Documents\App creation\dist\saved_templates.json` แล้วพบว่า record ของ Template เดียวเก็บ `{ชื่อโรงเรียน}` ร่วมกับข้อมูลลูกจ้าง/กรรมการ/ตำแหน่ง และข้อมูลอื่นจริง จึงยืนยันได้ว่าโปรแกรมเดิมจำชื่อโรงเรียนผ่าน saved template อยู่แล้ว; ยังไม่ถือเป็นคำตอบว่า Procurement ต้องมี school profile เดียวหรือหลาย profile
- ผู้ใช้ยืนยัน 2026-09-08: หน่วยบันทึกหลักคือข้อมูลการจ้าง/การเบิกของลูกจ้างรายบุคคล และข้อมูลลูกจ้างชุดเดียวต้องใช้ร่วมกันได้ทั้งแท็บ Payroll และ Procurement; ต้องแยกบทบาทกรรมการ Payroll ออกจากกรรมการ TOR Procurement
- ผู้ใช้ยืนยัน 2026-09-08 เพิ่มเติม: เครื่องเดียวเก็บลูกจ้างจากหลายโรงเรียนได้ เพราะชื่อโรงเรียนเป็นข้อมูลใน record รายบุคคลและใช้เพื่อแทน `{ชื่อโรงเรียน}` ไม่ต้องมี active school เดียวหรือเมนูสลับโรงเรียน
- ผู้ใช้ยืนยัน 2026-09-08 เพิ่มเติม: ข้อมูลร่วมของลูกจ้างแก้ที่แท็บหนึ่งแล้วอีกแท็บต้องเห็นทันที; กรรมการ A เป็นประธานอยู่บนสุดเสมอ และกรรมการ B/C อยู่ลำดับเดิมเสมอ
- ผู้ใช้ยืนยันเพิ่มเติม 2026-09-08: ในการใช้งานจริงกรรมการ TOR และกรรมการตรวจรับเป็นคนเดียวกัน แม้ template ปัจจุบันใช้ tag คนละชุด; แนวทางที่เสนอคือเก็บบุคคล/ลำดับเป็นข้อมูลต้นทางชุดเดียว แล้วทำ tag mapping ตอน render (`{กรรมการA/B/C}` ของ Payroll กับ `{คำนำหน้าสเปคA/B/C}`, `{ชื่อสเปคA/B/C}`, `{นามสกุลสเปคA/B/C}`, `{ตำแหน่งสเปคA/B/C}` ของ Procurement) โดยไม่แก้ template เอง
- ผู้ใช้ยืนยัน 2026-09-08: `{คำสั่งสเปค}` เว้นว่างได้ ให้เอกสารแสดงจุดว่าง และค่าที่กรอกต้องจำกับ record การจ้างลูกจ้างรายบุคคล
- ตรวจ source workspace 2026-09-08: `AddCommitteeRow` สร้าง `{กรรมการA}`, `{กรรมการB}`, `{กรรมการC}` (บรรทัด 384–386); `AddCombinedPersonControl` เก็บช่องย่อยเป็น `#prefix/#name/#surname` แล้วรวมเป็น `CombinedPersonControl` (บรรทัด 524–532); `CombinedPersonControl.Text` เป็นโค้ดประกอบชื่อเต็ม (บรรทัด 1968–2043); `CaptureFormData` เก็บค่าผ่าน `fieldBoxes` (บรรทัด 790 เป็นต้นไป); `AddTemplateAliases` มีเพียง alias `{กรรมการ B/C}` ไม่ได้ map จาก `สเปคA/B/C`; `RenderDocx`/`ReplaceTagsInXml` เป็น renderer ทั่วไป ไม่มีกฎกรรมการเฉพาะ
- ตรวจ DOCX จริง 2026-09-08: `Template\2.1 ขอตั้งกกTOR.docx` มี tag กรรมการแบบแยก 12 ตัว (คำนำหน้า/ชื่อ/นามสกุล/ตำแหน่ง A/B/C); baseline `C:\Users\MSI\Documents\App creation\dist\Template\4. ใบตรวจรับ.docx` มี `{กรรมการA}`, `{กรรมการB}`, `{กรรมการC}` เท่านั้นสำหรับกรรมการ; จึงทำ mapping ตอนสร้างค่าได้ โดยคง template เดิม
- ตรวจ D02 2026-09-08: baseline ติดตั้งที่ `%LOCALAPPDATA%\ReimbursementDocApp`; output และ `saved_templates.json` อยู่ข้าง executable ใน installation directory; ผู้ใช้ยืนยันให้ข้อมูลแยกตามบัญชี Windows ใน `%LOCALAPPDATA%` ไม่แชร์ทุก user และไม่ทำ portable data
- ตรวจ D03 2026-09-08: ไม่พบคำว่า “ประจวบ” ใน source/config ก่อนถาม; ผู้ใช้ยืนยันชื่อ default เป็น `สำนักงานเขตพื้นที่การศึกษาประถมศึกษาประจวบคีรีขันธ์ เขต 2`
- ตรวจ D04 2026-09-08: `C:\Users\MSI\Documents\App creation\dist\saved_templates.json` มี 1 record ชื่อ “มั่วทดสอบ” และค่า `lastGeneratedFiscalMonth=สิงหาคม`, `lastGeneratedFiscalYear=2569`; ผู้ใช้ยืนยันให้เริ่มข้อมูลใหม่และไม่ย้าย record นี้
- ตรวจ D06 2026-09-08: สแกน `H:\CodexProjects\346\Template` ครบ 34 DOCX รวม body/header/footer และ tag ที่ข้าม runs แล้วไม่พบ `{ถนนลูกจ้าง}`; `ReimbursementDocApp.cs` ของ baseline ยังมี field นี้และ Payroll ใบสำคัญรับเงินเดิมใช้ tag ดังกล่าว
- ชี้แจง D06 2026-09-08: `{ถนนลูกจ้าง}` ไม่ได้มาจาก Procurement template ใหม่; มาจาก `PROJECT_CONTEXT.md` §7 และ baseline Payroll จริง โดย source มี `TagEmployeeRoad`/ช่องกรอกถนน และ template Payroll `8. ใบสำคัญรับเงิน.docx` มี tag นี้ ส่วน Procurement 34 ไฟล์ไม่มี tag และยังไม่ควรเพิ่ม field/tag ให้ฝั่งนั้นเอง
- ตรวจย้ำ D06 2026-09-09: `C:\Users\MSI\Documents\App creation\template_tags.json` ระบุ `{ถนนลูกจ้าง}` เฉพาะ `8. ใบสำคัญรับเงิน.docx`; source baseline มีช่องกรอกถนนใน `AddAddressFields`; ตรวจ XML ของไฟล์ Payroll แล้วพบ tag จริง
- ผลกระทบที่ผู้ใช้ชี้ 2026-09-09: หากไม่แสดงช่องถนนในข้อมูลลูกจ้างร่วม ค่าจะว่างและ Payroll `8. ใบสำคัญรับเงิน.docx` จะแสดงจุดแทน ดังนั้นแนวทางที่รอยืนยันคือมีช่อง `{ถนนลูกจ้าง}` ในส่วนข้อมูลที่อยู่ของ record ลูกจ้างร่วม (กรอกได้/จำรายบุคคล) แม้ Procurement template จะไม่ใช้ tag นี้ และให้ Payroll นำค่าไปใช้ตาม baseline
- ผู้ใช้ยืนยัน 2026-09-09: ระบบต้องมีช่องกรอก `{ถนนลูกจ้าง}` ตั้งแต่ต้นในส่วนข้อมูลส่วนตัว/ที่อยู่ของลูกจ้าง กรอกได้แต่ไม่บังคับ และจำกับ record รายบุคคล เพื่อให้ Payroll ใช้ได้โดยไม่ต้องเพิ่ม tag ใน Procurement
- ผู้ใช้ยืนยันรูปแบบ UI 2026-09-09: ป้ายช่องให้แสดงเป็นภาษาคนว่า `ถนน` แล้วตามด้วยช่องกรอก; `{ถนนลูกจ้าง}` เป็นชื่อ tag ภายในสำหรับ Payroll เท่านั้น ไม่แสดงให้ผู้ใช้กรอกหรืออ่านในหน้าจอ
- ผู้ใช้ยืนยัน workflow 2026-09-09: “เพิ่มลูกจ้างใหม่” ต้องถามก่อนว่าจะเซฟ Template ของชื่อ–นามสกุลปัจจุบันหรือไม่; ไม่ว่าจะเซฟหรือไม่เซฟ หลังจากนั้นล้างข้อมูลฟอร์มของ record ปัจจุบันทั้งหมด; หากเซฟให้ใช้ฟังก์ชัน/flow เดียวกับปุ่มบันทึก Template และถามชื่อไฟล์/ชื่อรายการ; ตอนโหลด Template ต้องแจ้งว่าโหลดมาใช้ไม่กระทบข้อมูลเดิม เว้นแต่ผู้ใช้เลือกบันทึกทับชื่อข้อมูลเดิม
- ผู้ใช้ยืนยัน D09 2026-09-09: เปิดโปรแกรมที่แท็บชื่อเดิม `จัดซื้อจัดจ้างและสัญญา`; เมื่อมีข้อมูลค้างตอนปิด ให้เลือกเพียง `บันทึก` หรือ `ไม่บันทึก` และใช้กากบาทของกล่องถามเพื่อกลับไปทำงานต่อ ไม่เพิ่มปุ่มชื่อ “ยกเลิกการปิด”; เปิดโปรแกรมใหม่ต้องเป็นฟอร์มว่าง ไม่กู้คืนร่างอัตโนมัติ; ใช้ `Output/เงินเดือน` และ `Output/เอกสารจัดจ้าง` แยกกัน และชื่อซ้ำต่อท้าย `_2`, `_3`
- ผู้ใช้ยืนยัน D10 2026-09-09: รองรับ Windows 10/11; update เก็บข้อมูลเดิมทั้งหมด; uninstall ลบได้ตามเจตนาเจ้าของแต่ต้องจำกัด exact root/shortcut ของแอป; ยืนยันว่าหากไม่มี Word ให้สร้าง DOCX ต่อได้และตรวจได้เฉพาะ ZIP/XML ก่อนนำไปเปิดตรวจบนเครื่องที่มี Word
- ตรวจ D08 2026-09-09: `H:\CodexProjects\346\Template` มี DOCX Procurement 34 ไฟล์ แบ่งเป็นเอกสารกลาง 8 ไฟล์, TOR 13 variants และใบเสนอราคา 13 variants; ไม่พบไฟล์สัญญาแยกต่างหาก โดยผู้ใช้ยืนยันว่า `9. ใบสั่งจ้าง.docx` คือเอกสารสัญญาในขอบเขตนี้และไม่เพิ่มแบบอื่น; การเลือกเอกสารใช้ checkbox ติ๊กครบเป็นค่าเริ่มต้นแต่ยกเลิกบางไฟล์ได้
- ตรวจ D09 2026-09-09: source workspace มี checkbox เลือกเอกสารและ default เป็นเลือกทั้งหมด, มี preset/quick-load เดิม และป้องกันชื่อ output ซ้ำด้วย suffix `_2`, `_3`; ไม่พบ handler เก็บ draft เมื่อปิดโปรแกรมหรือถามก่อนปิด; ผู้ใช้จึงยืนยันให้เปิดแท็บ `จัดซื้อจัดจ้างและสัญญา`, ปิดขณะมีข้อมูลค้างด้วยตัวเลือก `บันทึก`/`ไม่บันทึก` (กากบาทของกล่องถามกลับไปทำงาน), เปิดใหม่เป็นฟอร์มว่าง, และใช้ `Output/เงินเดือน` กับ `Output/เอกสารจัดจ้าง`
- ตรวจ D10 2026-09-09: baseline ติดตั้งต่อบัญชีที่ `%LOCALAPPDATA%\\ReimbursementDocApp`, installer copy ทับไฟล์โปรแกรม/template/config, uninstaller ลบโฟลเดอร์ติดตั้งทั้งชุด; เอกสารผู้ใช้ระบุการใช้ Word ภายนอกและให้ปิดไฟล์ก่อน generate; ผู้ใช้ยืนยัน Windows 10/11, update ต้องรักษาข้อมูลเดิม, uninstall ลบได้เฉพาะ root/shortcut ของโปรแกรมตามเจตนาเจ้าของ และไม่มี Word ก็สร้าง DOCX/ตรวจ ZIP/XML ได้ก่อนนำไปเปิดตรวจบนเครื่องอื่น

เพิ่ม ID ใหม่ได้เมื่อพบ gap จริง ไม่บังคับให้ทุกเรื่องใหม่เป็นคำถามหากตรวจจากไฟล์ได้

- ผู้ใช้ยืนยันสรุปขอบเขตสุดท้าย 2026-09-09: ไม่มี blocker ที่ต้องถามเพิ่ม และพร้อมส่งต่อ implementation ตาม handoff นี้

## หลักฐานความพร้อม

- Template inventory ล่าสุด: ตรวจแล้ว 34 Procurement DOCX และ Payroll baseline 5 DOCX; Procurement มี 56 business tags, ทั้งสองฝั่งตรวจ body/header/footer และ split runs แล้ว; ไม่พบ `{ถนนลูกจ้าง}` ในฝั่ง Procurement
- TAG_MAPPING.md: จัดทำแล้ว; inventory ครบ Procurement 56 business tags และ Payroll actual/registry compatibility พร้อม alias และ path groups
- Blockers: ไม่มี OPEN ที่บล็อก scope; ผู้ใช้ยืนยันสรุปสุดท้ายแล้ว
- ขอบเขตที่เลื่อนโดยผู้ใช้: ยังไม่บันทึก
- การยืนยันสรุปจากผู้ใช้: CONFIRMED 2026-09-09
- สถานะพร้อมส่งต่อ: READY_FOR_IMPLEMENTATION
- งานแรกสำหรับ implementation: `T01` — ตรวจ baseline/source/config/template ทั้งสองแท็บ, บันทึก hash และยืนยัน manifest/path ก่อนแตะ T02–T07

## Prompt เปิดแชท implementation

เริ่ม implementation เอกสาร 346 ใน `H:\CodexProjects\346` โดยอ่าน `GRILL_HANDOFF.md` ก่อน
ตรวจว่าเป็น `READY_FOR_IMPLEMENTATION` และอ่าน `PROJECT_CONTEXT.md`, `ARCHITECTURE_PLAN.md`,
`UX UI design plan.md`, `TAG_MAPPING.md`, `IMPLEMENTATION_HANDOFF.md`, `IMPLEMENTATION_PROMPT.md` และ `ASTRA_REVIEW.md`
ทำตามคำตอบที่ยืนยันแล้ว เริ่มจากงานถัดไปใน handoff ไม่ grill ซ้ำทั้งชุด
ถ้า source/template เปลี่ยนหลัง grill ให้ตรวจผลกระทบและถามเฉพาะ blocker ใหม่ก่อนทำส่วนที่เกี่ยวข้อง

## ผล Astra review ก่อน implementation — 2026-09-09

- สถานะหลัง review: **READY_FOR_IMPLEMENTATION**; D01–D12 ยังคง CONFIRMED และไม่มี OPEN ที่ต้องถามผู้ใช้เพิ่ม
- ตรวจ source/config/build/installer/smoke แบบ read-only, diff กับ Payroll baseline, ตรวจ CRC/XML และ SHA-256 ของ Procurement 34 + Payroll manifest 5 DOCX; ไม่ compile/run/build/แก้ Word/config หรือเริ่ม implementation
- พบข้อคลาดเคลื่อนใน handoff และแก้เฉพาะ Markdown: school-profile/clear semantics, address mapping ที่ผิดทั้งกลุ่ม, suffix สอน2รร, เลขประจำตัวขั้นตอน 9, Payroll หัวหน้าพัสดุ และจำนวน occurrences
- พบ source risks ที่ต้องแก้ใน T01–T07: Payroll prefix alias ผิดบทบาทในไฟล์ 5, literal โรงเรียนซ้ำ, split-run formatting, manifest/build routing ยังเป็น baseline, stale smoke output, JSON/save failure, app identity/update/uninstall safety ดู R01–R10 ใน `ASTRA_REVIEW.md`
- Procurement ตรวจพบ 56 business tags / 1,043 occurrences; Payroll 34 / 127; ทุก selected DOCX มี split runs และผ่าน ZIP/XML parse แต่ยังไม่ได้รับรอง layout
- Baseline dist/Template มี DOCX สำรองเพิ่ม 1 ไฟล์; scope Payroll ยังคงเฉพาะ 5 ไฟล์ตาม manifest ห้าม copy ทั้งโฟลเดอร์โดยนับเป็น 6
- เกณฑ์ตรวจรับใหม่ที่ทำให้ข้อกำหนดครบอยู่ใน V01–V12 ของรายงาน; เป็นรายละเอียดของ scope เดิม ไม่เพิ่มฟีเจอร์/แบบสัญญาหรือเปลี่ยนคำตอบผู้ใช้
- งานถัดไปสำหรับ Luna Xhigh: T01 เทียบ hash/allowlist/occurrence mappings แล้วกำหนด app identity 346 และ shared record/module paths ก่อน T02; T01–T07 ยัง TODO ไม่มี build/smoke/visual/installer ผ่านในรอบ review นี้
- ตรวจ integrity หลังแก้ Markdown: workspace non-Markdown 48 ไฟล์และ baseline ที่อ่าน 16 ไฟล์ SHA-256 คงเดิมทั้งหมด ไม่มี non-Markdown ถูกเพิ่ม/ลบ เปลี่ยนเฉพาะ Markdown เดิม 7 ไฟล์และเพิ่ม ASTRA_REVIEW.md
