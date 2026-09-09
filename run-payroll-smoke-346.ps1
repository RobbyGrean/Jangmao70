$ErrorActionPreference = 'Stop'
$csc = 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path -LiteralPath $csc)) { $csc = 'C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe' }
$root = $PSScriptRoot
$out = Join-Path $root 'dist\PayrollRendererSmokeTest.exe'
$sourceFiles = @((Join-Path $root 'ReimbursementDocApp346.cs'), (Join-Path $root 'PayrollRendererSmokeTest.cs'))
& $csc /codepage:65001 /target:exe /platform:anycpu /main:ReimbursementDocApp.PayrollRendererSmokeTest /out:$out /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /reference:System.IO.Compression.dll /reference:System.IO.Compression.FileSystem.dll /reference:System.Web.Extensions.dll $sourceFiles
if ($LASTEXITCODE -ne 0) { throw "C# compiler failed with exit code $LASTEXITCODE" }
& $out
if ($LASTEXITCODE -ne 0) { throw "Payroll smoke test failed with exit code $LASTEXITCODE" }
