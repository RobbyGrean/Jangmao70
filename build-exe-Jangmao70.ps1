$ErrorActionPreference = 'Stop'
$csc = 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path -LiteralPath $csc)) { $csc = 'C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe' }
$outDir = Join-Path $PSScriptRoot 'dist'
if (-not (Test-Path -LiteralPath $outDir)) { New-Item -ItemType Directory -Path $outDir | Out-Null }
$sourceFiles = @(
    (Join-Path $PSScriptRoot 'Jangmao70App.cs'),
    (Join-Path $PSScriptRoot 'Shared\Jangmao70Core.cs'),
    (Join-Path $PSScriptRoot 'Shared\Jangmao70Config.cs'),
    (Join-Path $PSScriptRoot 'Shared\TemplateTransfer.cs'),
    (Join-Path $PSScriptRoot 'Shared\DocxRenderer.cs'),
    (Join-Path $PSScriptRoot 'Modules\Procurement\ProcurementModule.cs'),
    (Join-Path $PSScriptRoot 'Jangmao70Shell.cs')
)
foreach ($sourceFile in $sourceFiles) { if (-not (Test-Path -LiteralPath $sourceFile -PathType Leaf)) { throw "Source file not found: $sourceFile" } }
& $csc /codepage:65001 /target:winexe /platform:anycpu /main:Jangmao70.Jangmao70ShellProgram /win32icon:"$PSScriptRoot\app-icon.ico" /out:"$outDir\Jangmao70.exe" /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /reference:System.IO.Compression.dll /reference:System.IO.Compression.FileSystem.dll /reference:System.Web.Extensions.dll $sourceFiles
if ($LASTEXITCODE -ne 0) { throw "C# compiler failed with exit code $LASTEXITCODE" }
Copy-Item -LiteralPath "$PSScriptRoot\template_tags.json" -Destination "$outDir\template_tags.json" -Force
Copy-Item -LiteralPath "$PSScriptRoot\Config\Payroll\app_database.json" -Destination "$outDir\app_database.json" -Force
$nestedConfig = Join-Path $outDir 'Config\Config'
if (Test-Path -LiteralPath $nestedConfig) { Remove-Item -LiteralPath $nestedConfig -Recurse -Force }
New-Item -ItemType Directory -Path "$outDir\Config" -Force | Out-Null
Copy-Item -Path "$PSScriptRoot\Config\*" -Destination "$outDir\Config" -Recurse -Force
$nestedTemplate = Join-Path $outDir 'Template\Template'
if (Test-Path -LiteralPath $nestedTemplate) { Remove-Item -LiteralPath $nestedTemplate -Recurse -Force }
New-Item -ItemType Directory -Path "$outDir\Template" -Force | Out-Null
Copy-Item -Path "$PSScriptRoot\Template\*" -Destination "$outDir\Template" -Recurse -Force
Write-Host "Built: $outDir\Jangmao70.exe"
