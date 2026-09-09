$ErrorActionPreference = 'Stop'
$csc = 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path -LiteralPath $csc)) { $csc = 'C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe' }
$root = $PSScriptRoot
$out = Join-Path $root 'dist\Document346ShellSmokeTest.exe'
$sourceFiles = @(
    (Join-Path $root 'ReimbursementDocApp346.cs'),
    (Join-Path $root 'Shared\Document346Core.cs'),
    (Join-Path $root 'Shared\Document346Config.cs'),
    (Join-Path $root 'Shared\DocxRenderer.cs'),
    (Join-Path $root 'Modules\Procurement\ProcurementModule.cs'),
    (Join-Path $root 'Document346Shell.cs'),
    (Join-Path $root 'Document346ShellSmokeTest.cs')
)
& $csc /codepage:65001 /target:exe /platform:anycpu /main:ReimbursementDocApp.Document346ShellSmokeTest /out:$out /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /reference:System.IO.Compression.dll /reference:System.IO.Compression.FileSystem.dll /reference:System.Web.Extensions.dll $sourceFiles
if ($LASTEXITCODE -ne 0) { throw "C# compiler failed with exit code $LASTEXITCODE" }
& $out
if ($LASTEXITCODE -ne 0) { throw "Shell smoke test failed with exit code $LASTEXITCODE" }
