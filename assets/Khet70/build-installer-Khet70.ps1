$ErrorActionPreference = 'Stop'

$root = (Resolve-Path $PSScriptRoot).Path
$dist = Join-Path $root 'dist\Khet70'
$release = Join-Path $root 'release\Khet70-Installer'
$payload = Join-Path $release 'Payload'
$payloadConfig = Join-Path $payload 'Config'
$payloadTemplate = Join-Path $payload 'Template'
$payloadDefaultTemplate = Join-Path $payload 'Defaults\Template'
$csc = 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path -LiteralPath $csc)) { $csc = 'C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe' }
if (-not (Test-Path -LiteralPath $csc)) { throw 'ไม่พบ C# compiler (csc.exe)' }

& (Join-Path $root 'verify-manifest.ps1')
& (Join-Path $root 'build-exe-Khet70.ps1')
if (-not (Test-Path -LiteralPath (Join-Path $dist 'Khet70.exe'))) { throw 'Built executable is missing' }

$docxFiles = @(Get-ChildItem -LiteralPath (Join-Path $dist 'Template') -Filter '*.docx' -Recurse -File)
if ($docxFiles.Count -ne 49) { throw "Expected 49 packaged DOCX templates, found $($docxFiles.Count)." }

if (Test-Path -LiteralPath $release) { Remove-Item -LiteralPath $release -Recurse -Force }
New-Item -ItemType Directory -Path $payloadConfig,$payloadTemplate,$payloadDefaultTemplate | Out-Null
Copy-Item -LiteralPath (Join-Path $dist 'Khet70.exe') -Destination $payload -Force
Copy-Item -LiteralPath (Join-Path $dist 'template_tags.json') -Destination $payload -Force
Copy-Item -LiteralPath (Join-Path $dist 'app_database.json') -Destination $payload -Force
Copy-Item -Path (Join-Path $dist 'Config\*') -Destination $payloadConfig -Recurse -Force
Copy-Item -Path (Join-Path $dist 'Template\*') -Destination $payloadTemplate -Recurse -Force
Copy-Item -Path (Join-Path $dist 'Defaults\Template\*') -Destination $payloadDefaultTemplate -Recurse -Force

& $csc /codepage:65001 /target:winexe /platform:anycpu /win32icon:"$root\app-icon.ico" /out:"$payload\Uninstall Khet70.exe" /reference:System.Windows.Forms.dll /reference:Microsoft.CSharp.dll (Join-Path $root 'Khet70Uninstaller.cs')
if ($LASTEXITCODE -ne 0) { throw "Uninstaller compile failed with exit code $LASTEXITCODE" }
& $csc /codepage:65001 /target:winexe /platform:anycpu /win32icon:"$root\app-icon.ico" /out:"$release\Khet70-Setup.exe" /reference:System.Windows.Forms.dll /reference:Microsoft.CSharp.dll (Join-Path $root 'Khet70Installer.cs')
if ($LASTEXITCODE -ne 0) { throw "Installer compile failed with exit code $LASTEXITCODE" }

$zip = Join-Path $root 'release\Khet70-Installer.zip'
if (Test-Path -LiteralPath $zip) { Remove-Item -LiteralPath $zip -Force }
Compress-Archive -Path (Join-Path $release '*') -DestinationPath $zip -Force
Write-Host "Installer built: $release\Khet70-Setup.exe"
