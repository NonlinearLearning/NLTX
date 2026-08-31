[CmdletBinding()]
param(
  [string]$RepositoryRoot = (Get-Location).Path
)

$ErrorActionPreference = 'Stop'

$inventoryPath = Join-Path $RepositoryRoot 'docs\worldgen\worldgen-source-inventory.json'
$overlayPath = Join-Path $RepositoryRoot 'docs\worldgen\worldgen-field-status-overlay-2026-08-29.json'

if (-not (Test-Path -LiteralPath $inventoryPath)) {
  throw "Inventory not found: $inventoryPath"
}
if (-not (Test-Path -LiteralPath $overlayPath)) {
  throw "Overlay not found: $overlayPath"
}
$inventory = Get-Content -LiteralPath $inventoryPath -Raw | ConvertFrom-Json
$overlay = Get-Content -LiteralPath $overlayPath -Raw | ConvertFrom-Json
$sourcePath = $inventory.SourcePath
if (-not (Test-Path -LiteralPath $sourcePath)) {
  throw "Source not found: $sourcePath"
}
$source = Get-Content -LiteralPath $sourcePath -Raw

$inventoryNames = @($inventory.Fields | ForEach-Object Name)
$overlayNames = @($overlay.Fields | ForEach-Object Name)
$duplicates = @($overlayNames | Group-Object | Where-Object Count -gt 1)
$missing = @($inventoryNames | Where-Object { $overlayNames -notcontains $_ })
$extra = @($overlayNames | Where-Object { $inventoryNames -notcontains $_ })
$invalidStatus = @($overlay.Fields | Where-Object {
    $_.Status -notin @('Partial', 'Deferred', 'Excluded/Deferred')
  })
$missingOwnerOrSource = @($overlay.Fields | Where-Object {
    [string]::IsNullOrWhiteSpace($_.Owner) -or [string]::IsNullOrWhiteSpace($_.Source)
  })
$sourceMisses = @($overlay.Fields | Where-Object {
    $source -notmatch [regex]::Escape($_.Name)
  })

Write-Output "inventory_fields=$($inventoryNames.Count)"
Write-Output "overlay_fields=$($overlayNames.Count)"
Write-Output "missing=$($missing.Count) extra=$($extra.Count) duplicates=$($duplicates.Count)"
Write-Output "invalid_status=$($invalidStatus.Count) missing_owner_or_source=$($missingOwnerOrSource.Count)"
Write-Output "source_name_misses=$($sourceMisses.Count)"

if ($inventoryNames.Count -ne 233 -or $overlayNames.Count -ne 233 -or
    $missing.Count -ne 0 -or $extra.Count -ne 0 -or $duplicates.Count -ne 0 -or
    $invalidStatus.Count -ne 0 -or $missingOwnerOrSource.Count -ne 0 -or
    $sourceMisses.Count -ne 0) {
  throw 'WorldGen field status overlay validation failed.'
}

Write-Output 'status_overlay_validation=passed'
