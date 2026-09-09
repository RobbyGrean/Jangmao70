$ErrorActionPreference = 'Stop'

$root = $PSScriptRoot
$dist = Join-Path $root 'dist'
$desktop = [Environment]::GetFolderPath('Desktop')
$downloads = Join-Path $env:USERPROFILE 'Downloads'
$templateSources = @((Join-Path $dist 'Template'))
$templateSources += (Join-Path $root 'Template')
$templateSources += Get-ChildItem -LiteralPath $desktop -Directory -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -like 'Template*' } |
    Select-Object -ExpandProperty FullName
$templateSources += $downloads
$templateJson = [IO.File]::ReadAllText((Join-Path $root 'template_tags.json'), [Text.Encoding]::UTF8)
$templateFiles = [regex]::Matches($templateJson, '"([^"]+\.docx)"\s*:') |
    ForEach-Object { $_.Groups[1].Value }
$release = Join-Path $root 'release'
$payload = Join-Path $release 'Payload'
$payloadTemplate = Join-Path $payload 'Template'
$packageRoot = Join-Path $release 'mhs2jm'
$csc = 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path -LiteralPath $csc)) {
    $csc = 'C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe'
}

powershell -ExecutionPolicy Bypass -File (Join-Path $root 'build-exe.ps1')

if (Test-Path -LiteralPath $release) {
    Remove-Item -LiteralPath $release -Recurse -Force
}
New-Item -ItemType Directory -Path $payload -Force | Out-Null
New-Item -ItemType Directory -Path $payloadTemplate -Force | Out-Null

Copy-Item -LiteralPath (Join-Path $dist 'ReimbursementDocApp.exe') -Destination $payload -Force
Copy-Item -LiteralPath (Join-Path $dist 'template_tags.json') -Destination $payload -Force
Copy-Item -LiteralPath (Join-Path $dist 'app_database.json') -Destination $payload -Force
Copy-Item -LiteralPath (Join-Path $root 'USER_README_TH.txt') -Destination (Join-Path $release 'README-TH.txt') -Force
foreach ($templateFile in $templateFiles) {
    $source = $null
    foreach ($templateSource in $templateSources) {
        $candidatePath = Join-Path $templateSource $templateFile
        $candidate = Get-Item -LiteralPath $candidatePath -ErrorAction SilentlyContinue
        if ($candidate) {
            $source = $candidate.FullName
            break
        }
    }
    if (-not $source) {
        throw "Template not found: $templateFile"
    }
    Copy-Item -LiteralPath $source -Destination (Join-Path $payloadTemplate (Split-Path $source -Leaf)) -Force
}

& $csc `
    /codepage:65001 `
    /target:winexe `
    /platform:anycpu `
    /win32icon:"$root\app-icon.ico" `
    /out:"$payload\Uninstall ReimbursementDocApp.exe" `
    /reference:System.Windows.Forms.dll `
    "$root\Uninstaller.cs"

& $csc `
    /codepage:65001 `
    /target:winexe `
    /platform:anycpu `
    /win32icon:"$root\app-icon.ico" `
    /out:"$release\ReimbursementDocApp-Setup.exe" `
    /reference:System.Windows.Forms.dll `
    /reference:Microsoft.CSharp.dll `
    "$root\Installer.cs"

New-Item -ItemType Directory -Path $packageRoot -Force | Out-Null
Copy-Item -LiteralPath "$release\ReimbursementDocApp-Setup.exe" -Destination $packageRoot -Force
Copy-Item -LiteralPath "$release\README-TH.txt" -Destination $packageRoot -Force
Copy-Item -LiteralPath $payload -Destination $packageRoot -Recurse -Force

Compress-Archive -Path $packageRoot -DestinationPath "$release\ReimbursementDocApp-Installer.zip" -Force

$sourceZip = Join-Path $release 'ReimbursementDocApp-Source.zip'
$sourceItems = Get-ChildItem -LiteralPath $root -Force | Where-Object {
    $_.Name -notin @('.git', 'release', 'jmoney-repo', 'jmoney-site', 'jmoney-source-inspect', 'smoke-output')
}
Compress-Archive -Path $sourceItems.FullName -DestinationPath $sourceZip -Force

$siteDownloads = Join-Path $root 'jmoney-repo\assets\downloads'
if (Test-Path -LiteralPath $siteDownloads) {
    Copy-Item -LiteralPath "$release\ReimbursementDocApp-Installer.zip" -Destination $siteDownloads -Force
    Copy-Item -LiteralPath $sourceZip -Destination $siteDownloads -Force
    $siteReadme = Get-ChildItem -LiteralPath $siteDownloads -File |
        Where-Object { $_.Name -like 'README-*.txt' } |
        Select-Object -First 1
    if ($siteReadme) {
        Copy-Item -LiteralPath "$root\USER_README_TH.txt" -Destination $siteReadme.FullName -Force
    }
}

Write-Host "Installer folder built: $release"
Write-Host "Send this zip: $release\ReimbursementDocApp-Installer.zip"
Write-Host "Source zip: $sourceZip"
