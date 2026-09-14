$ErrorActionPreference = 'Stop'

$root = $PSScriptRoot
$dist = Join-Path $root 'dist'
$release = Join-Path $root 'release\Jangmao70-Installer'
$payload = Join-Path $release 'Payload'
$payloadConfig = Join-Path $payload 'Config'
$payloadTemplate = Join-Path $payload 'Template'
$csc = 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path -LiteralPath $csc)) { $csc = 'C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe' }

function Require-BuiltFile([string]$path, [string]$label) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "$label is missing: $path" }
    $item = Get-Item -LiteralPath $path
    if ($item.Length -le 0) { throw "$label is empty: $path" }
    return $item
}

& (Join-Path $root 'verify-manifest.ps1')
& (Join-Path $root 'build-exe-Jangmao70.ps1')
if (-not (Test-Path -LiteralPath (Join-Path $dist 'Jangmao70.exe'))) { throw 'Built executable is missing' }

$procurementCount = @(Get-ChildItem -LiteralPath (Join-Path $dist 'Template\Procurement') -Filter '*.docx' -Recurse -File).Count
$payrollCount = @(Get-ChildItem -LiteralPath (Join-Path $dist 'Template\Payroll') -Filter '*.docx' -Recurse -File).Count
if ($procurementCount -ne 34 -or $payrollCount -ne 5) { throw "Template allowlist mismatch: Payroll=$payrollCount Procurement=$procurementCount" }

if (Test-Path -LiteralPath $release) { Remove-Item -LiteralPath $release -Recurse -Force }
New-Item -ItemType Directory -Path $payloadConfig -Force | Out-Null
New-Item -ItemType Directory -Path $payloadTemplate -Force | Out-Null
Copy-Item -LiteralPath (Join-Path $dist 'Jangmao70.exe') -Destination $payload -Force
Copy-Item -LiteralPath (Join-Path $dist 'template_tags.json') -Destination $payload -Force
Copy-Item -LiteralPath (Join-Path $dist 'app_database.json') -Destination $payload -Force
Copy-Item -Path (Join-Path $dist 'Config\*') -Destination $payloadConfig -Recurse -Force
Copy-Item -Path (Join-Path $dist 'Template\*') -Destination $payloadTemplate -Recurse -Force

$uninstallerPath = Join-Path $payload 'Uninstall Jangmao70.exe'
& $csc /codepage:65001 /target:winexe /platform:anycpu /win32icon:"$root\app-icon.ico" /out:"$uninstallerPath" /reference:System.Windows.Forms.dll /reference:Microsoft.CSharp.dll "$root\Jangmao70Uninstaller.cs"
if ($LASTEXITCODE -ne 0) { throw "Uninstaller compile failed with exit code $LASTEXITCODE" }
$uninstallerArtifact = Require-BuiltFile $uninstallerPath 'Uninstaller build output'

$setupPath = Join-Path $release 'Jangmao70-Setup.exe'
& $csc /codepage:65001 /target:winexe /platform:anycpu /win32icon:"$root\app-icon.ico" /out:"$setupPath" /reference:System.Windows.Forms.dll /reference:Microsoft.CSharp.dll "$root\Jangmao70Installer.cs"
if ($LASTEXITCODE -ne 0) { throw "Installer compile failed with exit code $LASTEXITCODE" }
$setupArtifact = Require-BuiltFile $setupPath 'Installer build output'

$zip = Join-Path $root 'release\Jangmao70-Installer.zip'
$publishedZip = Join-Path $root 'assets\downloads\Jangmao70-Installer.zip'
if (Test-Path -LiteralPath $zip) { Remove-Item -LiteralPath $zip -Force }
Compress-Archive -Path (Join-Path $release '*') -DestinationPath $zip -Force
Copy-Item -LiteralPath $zip -Destination $publishedZip -Force
Write-Host "Uninstaller built: $($uninstallerArtifact.FullName) ($($uninstallerArtifact.Length) bytes)"
Write-Host "Setup built: $($setupArtifact.FullName) ($($setupArtifact.Length) bytes)"
Write-Host "Installer built: $zip"
Write-Host "Published installer: $publishedZip"
