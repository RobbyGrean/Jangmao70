$ErrorActionPreference = 'Stop'

$root = (Resolve-Path $PSScriptRoot).Path
$identity = Get-Content -Raw -Encoding UTF8 -LiteralPath (Join-Path $root 'Config\app_identity.json') | ConvertFrom-Json
$payrollManifest = Get-Content -Raw -Encoding UTF8 -LiteralPath (Join-Path $root 'Config\Payroll\template_manifest.json') | ConvertFrom-Json
$procurementManifest = Get-Content -Raw -Encoding UTF8 -LiteralPath (Join-Path $root 'Config\Procurement\template_manifest.json') | ConvertFrom-Json

function Resolve-ContainedPath([string]$relativePath) {
    if ([string]::IsNullOrWhiteSpace($relativePath) -or [IO.Path]::IsPathRooted($relativePath)) {
        throw "Manifest path must be relative: $relativePath"
    }
    $full = [IO.Path]::GetFullPath((Join-Path $root ($relativePath.Replace('/', '\'))))
    $rootWithSlash = $root.TrimEnd('\') + '\'
    if (-not $full.StartsWith($rootWithSlash, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Manifest path escapes workspace: $relativePath"
    }
    return $full
}

function Assert-Document([object]$document, [string]$moduleRoot) {
    $relative = [string]$document.relativePath
    if ([string]::IsNullOrWhiteSpace($relative) -or $relative.Contains('..')) {
        throw "Invalid $moduleRoot manifest path: $relative"
    }
    $path = Resolve-ContainedPath ((Join-Path $moduleRoot $relative).Replace('\', '/'))
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Missing manifest document: $path"
    }
    $actual = (Get-FileHash -Algorithm SHA256 -LiteralPath $path).Hash.ToLowerInvariant()
    if ($actual -ne ([string]$document.sha256).ToLowerInvariant()) {
        throw "Hash mismatch: $path actual=$actual expected=$($document.sha256)"
    }
    return $relative
}

$payrollRoot = [string]$identity.moduleRoots.Payroll.template
$payrollPaths = @($payrollManifest.documents | ForEach-Object { Assert-Document $_ $payrollRoot })
if ($payrollPaths.Count -ne 5 -or @($payrollPaths | Sort-Object -Unique).Count -ne 5) {
    throw 'Payroll manifest must contain five unique documents.'
}

$procurementRoot = [string]$identity.moduleRoots.Procurement.template
$procurementPaths = @()
$procurementPaths += @($procurementManifest.centralDocuments | ForEach-Object { Assert-Document $_ $procurementRoot })
foreach ($route in $procurementManifest.routes) {
    if ([string]::IsNullOrWhiteSpace([string]$route.tor) -or [string]::IsNullOrWhiteSpace([string]$route.quote)) {
        throw "Route is missing a TOR/quote pair: $($route.id)"
    }
    $tor = [pscustomobject]@{ relativePath = [string]$route.tor; sha256 = [string]$route.torSha256 }
    $quote = [pscustomobject]@{ relativePath = [string]$route.quote; sha256 = [string]$route.quoteSha256 }
    $procurementPaths += Assert-Document $tor $procurementRoot
    $procurementPaths += Assert-Document $quote $procurementRoot
}
if (@($procurementManifest.routes).Count -ne 13) { throw 'Procurement manifest must contain thirteen routes.' }
if ($procurementPaths.Count -ne 34 -or @($procurementPaths | Sort-Object -Unique).Count -ne 34) {
    throw 'Procurement manifest must resolve to thirty-four unique documents.'
}

$allDocx = @(Get-ChildItem -LiteralPath (Resolve-ContainedPath $procurementRoot) -Recurse -File -Filter '*.docx')
if ($allDocx.Count -ne 34) { throw "Procurement template root contains $($allDocx.Count) DOCX files; expected 34." }

$routeIds = @($procurementManifest.routes | ForEach-Object { [string]$_.id })
if (@($routeIds | Sort-Object -Unique).Count -ne 13) { throw 'Procurement route IDs must be unique.' }

Write-Output "Manifest OK: Payroll=$($payrollPaths.Count), Procurement=$($procurementPaths.Count), Routes=$(@($procurementManifest.routes).Count), AppId=$($identity.appId)"
