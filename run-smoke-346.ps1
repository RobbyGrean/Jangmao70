$ErrorActionPreference = 'Stop'
$csc = 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path -LiteralPath $csc)) { $csc = 'C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe' }
$root = $PSScriptRoot
$out = Join-Path $root 'dist\Document346SmokeTest.exe'
$sourceFiles = @(
    (Join-Path $root 'Shared\Document346Core.cs'),
    (Join-Path $root 'Shared\Document346Config.cs'),
    (Join-Path $root 'Shared\DocxRenderer.cs'),
    (Join-Path $root 'Document346SmokeTest.cs')
)
& $csc /codepage:65001 /target:exe /platform:anycpu /out:$out /reference:System.IO.Compression.dll /reference:System.IO.Compression.FileSystem.dll /reference:System.Web.Extensions.dll $sourceFiles
if ($LASTEXITCODE -ne 0) { throw "C# compiler failed with exit code $LASTEXITCODE" }
& $out
if ($LASTEXITCODE -ne 0) { throw "Smoke test failed with exit code $LASTEXITCODE" }
