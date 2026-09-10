<div align="center">

# Jangmao70

โปรแกรมช่วยกรอกข้อมูลและสร้างเอกสาร Word สำหรับงานจัดซื้อจัดจ้างและงานเบิกเงินเดือน

[ดาวน์โหลดโปรแกรม](./assets/downloads/Jangmao70-Installer.zip) · [อ่านคู่มือ](./GUIDE.md) · [ดูคู่มือเว็บ](./guide.html)

</div>

Jangmao70 รวมงานเอกสารสองแบบไว้ในโปรแกรมเดียว แต่แยกขั้นตอน เอกสารที่สร้าง และโฟลเดอร์ผลลัพธ์ออกจากกัน

## ดาวน์โหลด

| ไฟล์ | เหมาะสำหรับ |
| --- | --- |
| [Jangmao70-Installer.zip](./assets/downloads/Jangmao70-Installer.zip) | ผู้ใช้งานทั่วไป ใช้ติดตั้งโปรแกรมบน Windows |
| [Jangmao70-Source.zip](./assets/downloads/Jangmao70-Source.zip) | ผู้ที่ต้องการศึกษา source code และโครงสร้างโปรแกรม |

## เริ่มใช้งานอย่างย่อ

1. ดาวน์โหลด Installer แล้วแตก ZIP
2. เปิด `Jangmao70-Setup.exe`
3. เปิด Jangmao70 และเลือกแท็บงานที่ต้องการ
4. กรอกหรือโหลด Template → ตรวจข้อมูล → สร้างเอกสาร Word

รายละเอียดคำเตือนจาก Chrome/Windows และวิธีติดตั้งอยู่ใน [คู่มือการใช้งาน](./GUIDE.md) ส่วนเนื้อหาและข้อกำหนดสำหรับเว็บไซต์อยู่ใน [SITE_CONTENT_BRIEF.md](./SITE_CONTENT_BRIEF.md)

## Workflow ของผู้ใช้

![Workflow ของผู้ใช้ Jangmao70](./assets/readme/jangmao70-user-workflow.svg)

## การเชื่อมโยงข้อมูลภายใน

![การเชื่อมโยงข้อมูลภายในของ Jangmao70](./assets/readme/jangmao70-data-flow.svg)

## หลักการสำคัญ

- ข้อมูลร่วมที่จำเป็น เช่น ชื่อ ที่อยู่ และกรรมการ สามารถนำไปใช้กับอีกแท็บผ่านการโหลด Template ได้
- งวดเบิก รายการเอกสาร Template ที่บันทึก และ Output ยังคงแยกตามแท็บ
- ข้อมูลและเอกสารทำงานแบบ local ในเครื่อง ไม่ต้องใช้ npm หรือระบบ cloud เพื่อใช้งานโปรแกรม
- ช่องที่ไม่ได้กรอกอาจถูกแทนด้วยจุดในเอกสารตามกติกาของระบบ จึงควรตรวจเอกสารก่อนนำไปใช้จริง

## โครงสร้าง repository

```text
assets/
├─ downloads/       ไฟล์ Installer และ Source สำหรับดาวน์โหลด
├─ guide/           ภาพประกอบคำเตือนการดาวน์โหลด
├─ screenshots/     ภาพหน้าจออ้างอิงของทั้งสอง workflow
└─ readme/          diagram สำหรับ README
Config/             config ของ workflow และ Template
Modules/            โมดูลจัดซื้อจัดจ้าง
Shared/             core, renderer และการโอนข้อมูลระหว่าง Template
Template/           Template Word ของทั้งสอง workflow
Jangmao70*.cs       source ของตัวโปรแกรมและตัวติดตั้ง
GUIDE.md            คู่มือฉบับอ่านง่าย
SITE_CONTENT_BRIEF.md brief เนื้อหาจริงและคำสั่งสำหรับปรับเว็บไซต์
guide.html          คู่มือบนเว็บไซต์
index.html          เว็บไซต์หลัก
README.md           หน้าสรุปโครงการ
```

โปรเจกต์นี้ยังไม่มีระบบ updater หรือ npm installer การอัปเดตในตอนนี้ให้ดาวน์โหลด Installer รุ่นใหม่จาก GitHub แล้วติดตั้งทับตามคู่มือ
