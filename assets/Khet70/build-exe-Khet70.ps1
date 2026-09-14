$ErrorActionPreference = 'Stop'

$root = (Resolve-Path $PSScriptRoot).Path
$outDir = Join-Path $root 'dist\Khet70'
$csc = 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path -LiteralPath $csc)) { $csc = 'C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe' }
if (-not (Test-Path -LiteralPath $csc)) { throw 'ไม่พบ C# compiler (csc.exe)' }

if (Test-Path -LiteralPath $outDir) { Remove-Item -LiteralPath $outDir -Recurse -Force }
New-Item -ItemType Directory -Path $outDir | Out-Null

$sourceFiles = @(
    (Join-Path $root 'Khet70App.cs'),
    (Join-Path $root 'Shared\Khet70Core.cs'),
    (Join-Path $root 'Shared\Khet70Config.cs'),
    (Join-Path $root 'Shared\TemplateTransfer.cs'),
    (Join-Path $root 'Shared\DocxRenderer.cs'),
    (Join-Path $root 'Modules\Procurement\ProcurementModule.cs'),
    (Join-Path $root 'Khet70Shell.cs')
)
foreach ($sourceFile in $sourceFiles) {
    if (-not (Test-Path -LiteralPath $sourceFile -PathType Leaf)) { throw "Source file not found: $sourceFile" }
}

& $csc /codepage:65001 /target:winexe /platform:anycpu /main:Khet70.Khet70ShellProgram /win32icon:"$root\app-icon.ico" /out:"$outDir\Khet70.exe" /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /reference:System.IO.Compression.dll /reference:System.IO.Compression.FileSystem.dll /reference:System.Web.Extensions.dll $sourceFiles
if ($LASTEXITCODE -ne 0) { throw "C# compiler failed with exit code $LASTEXITCODE" }

Copy-Item -LiteralPath (Join-Path $root 'template_tags.json') -Destination (Join-Path $outDir 'template_tags.json') -Force
Copy-Item -LiteralPath (Join-Path $root 'Config\Payroll\app_database.json') -Destination (Join-Path $outDir 'app_database.json') -Force
$defaultTemplateDir = Join-Path $outDir 'Defaults\Template'
New-Item -ItemType Directory -Force -Path (Join-Path $outDir 'Config'),(Join-Path $outDir 'Template'),$defaultTemplateDir | Out-Null
Copy-Item -Path (Join-Path $root 'Config\*') -Destination (Join-Path $outDir 'Config') -Recurse -Force
Copy-Item -Path (Join-Path $root 'Template\*') -Destination (Join-Path $outDir 'Template') -Recurse -Force
Copy-Item -Path (Join-Path $root 'Template\*') -Destination $defaultTemplateDir -Recurse -Force

Write-Host "Built: $outDir\Khet70.exe"
