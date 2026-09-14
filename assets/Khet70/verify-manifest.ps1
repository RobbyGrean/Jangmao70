$ErrorActionPreference = 'Stop'

$root = (Resolve-Path $PSScriptRoot).Path
$identity = Get-Content -Raw -Encoding UTF8 -LiteralPath (Join-Path $root 'Config\app_identity.json') | ConvertFrom-Json
$payrollManifest = Get-Content -Raw -Encoding UTF8 -LiteralPath (Join-Path $root 'Config\Payroll\template_manifest.json') | ConvertFrom-Json
$procurementManifest = Get-Content -Raw -Encoding UTF8 -LiteralPath (Join-Path $root 'Config\Procurement\template_manifest.json') | ConvertFrom-Json

if ([string]$identity.productName -ne 'Khet70' -or [string]$identity.appId -ne 'Khet70' -or [string]$identity.installRootName -ne 'Khet70') {
    throw 'Khet70 identity is incomplete or still points to another product.'
}

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
    if ([string]::IsNullOrWhiteSpace([string]$document.sha256) -or $actual -ne ([string]$document.sha256).ToLowerInvariant()) {
        throw "Hash mismatch: $path actual=$actual expected=$($document.sha256)"
    }
    return $relative
}

$payrollRoot = [string]$identity.moduleRoots.Payroll.template
$payrollPaths = @($payrollManifest.documents | ForEach-Object { Assert-Document $_ $payrollRoot })
if (@($payrollPaths | Sort-Object -Unique).Count -ne $payrollPaths.Count) { throw 'Payroll manifest contains duplicate documents.' }

$procurementRoot = [string]$identity.moduleRoots.Procurement.template
$procurementPaths = @()
$procurementPaths += @($procurementManifest.centralDocuments | ForEach-Object { Assert-Document $_ $procurementRoot })
$routeIds = @()
foreach ($route in @($procurementManifest.routes)) {
    if ([string]::IsNullOrWhiteSpace([string]$route.id) -or [string]::IsNullOrWhiteSpace([string]$route.positionId)) {
        throw 'Procurement route is missing an id or positionId.'
    }
    if ([string]::IsNullOrWhiteSpace([string]$route.tor) -or [string]::IsNullOrWhiteSpace([string]$route.quote)) {
        throw "Route is missing a TOR/quote pair: $($route.id)"
    }
    $routeIds += [string]$route.id
    $procurementPaths += Assert-Document ([pscustomobject]@{ relativePath = [string]$route.tor; sha256 = [string]$route.torSha256 }) $procurementRoot
    $procurementPaths += Assert-Document ([pscustomobject]@{ relativePath = [string]$route.quote; sha256 = [string]$route.quoteSha256 }) $procurementRoot
}
if (@($routeIds | Sort-Object -Unique).Count -ne $routeIds.Count) { throw 'Procurement route IDs must be unique.' }
if (@($procurementPaths | Sort-Object -Unique).Count -ne $procurementPaths.Count) { throw 'Procurement manifest contains duplicate documents.' }

$allDocx = @(Get-ChildItem -LiteralPath (Resolve-ContainedPath $procurementRoot) -Recurse -File -Filter '*.docx')
$allDocx += @(Get-ChildItem -LiteralPath (Resolve-ContainedPath $payrollRoot) -Recurse -File -Filter '*.docx')
$expectedDocx = @($payrollPaths + $procurementPaths | Sort-Object -Unique)
if ($allDocx.Count -ne $expectedDocx.Count) {
    throw "Template folders contain $($allDocx.Count) DOCX files, but manifests resolve to $($expectedDocx.Count)."
}

Write-Output "Manifest OK: Payroll=$($payrollPaths.Count), Procurement=$($procurementPaths.Count), Routes=$($routeIds.Count), AppId=$($identity.appId)"
