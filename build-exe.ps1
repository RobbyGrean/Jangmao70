$ErrorActionPreference = 'Stop'
$csc = 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path -LiteralPath $csc)) {
    $csc = 'C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe'
}

$outDir = Join-Path $PSScriptRoot 'dist'
if (-not (Test-Path -LiteralPath $outDir)) {
    New-Item -ItemType Directory -Path $outDir | Out-Null
}

& $csc `
    /codepage:65001 `
    /target:winexe `
    /platform:anycpu `
    /win32icon:"$PSScriptRoot\app-icon.ico" `
    /out:"$outDir\ReimbursementDocApp.exe" `
    /reference:System.Windows.Forms.dll `
    /reference:System.Drawing.dll `
    /reference:System.IO.Compression.dll `
    /reference:System.IO.Compression.FileSystem.dll `
    "$PSScriptRoot\ReimbursementDocApp.cs"

Copy-Item -LiteralPath "$PSScriptRoot\template_tags.json" -Destination "$outDir\template_tags.json" -Force
Copy-Item -LiteralPath "$PSScriptRoot\app_database.json" -Destination "$outDir\app_database.json" -Force
Write-Host "Built: $outDir\ReimbursementDocApp.exe"
