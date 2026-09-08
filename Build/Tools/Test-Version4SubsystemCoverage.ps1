[CmdletBinding()]
param(
  [string]$Version4Root = 'D:\TRbackup\Version4',
  [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$docs = Join-Path $RepositoryRoot 'docs'
$referenceDocs = Join-Path $docs '迁移参考表'
$tsvPath = Join-Path $referenceDocs 'Version4源码覆盖.tsv'
$jsonPath = Join-Path $referenceDocs 'Version4子系统索引.json'
$allowedClassifications = @('subsystem-evidence','shared-runtime-mechanism','external-dependency-or-generated','excluded')
$allowedReferenceStatuses = @('version4-confirmed','full-reference-supplemented','source-gap')
$allowedNltxStatuses = @('confirmed','partial','missing','excluded')

$baseline = Get-ChildItem -LiteralPath $Version4Root -Recurse -File -Filter '*.cs' |
  Where-Object { $_.FullName -notmatch '[\\/](bin|obj)[\\/]' } |
  ForEach-Object { $_.FullName.Substring($Version4Root.Length + 1).Replace('\', '/') } |
  Sort-Object
$rows = Import-Csv -LiteralPath $tsvPath -Delimiter "`t"
$index = Get-Content -Raw -LiteralPath $jsonPath | ConvertFrom-Json
$rowPaths = @($rows | ForEach-Object relative_path)
$unclassified = @($baseline | Where-Object { $_ -notin $rowPaths })
$pathDrift = @($rowPaths | Where-Object { $_ -notin $baseline })
$duplicatePaths = @($rows | Group-Object relative_path | Where-Object Count -gt 1)
$missingOwner = @($rows | Where-Object { [string]::IsNullOrWhiteSpace($_.primary_owner) })
$invalidRows = @($rows | Where-Object {
  $_.classification -notin $allowedClassifications -or $_.reference_status -notin $allowedReferenceStatuses -or
  ($_.reference_status -eq 'full-reference-supplemented' -and [string]::IsNullOrWhiteSpace($_.full_reference_path))
})
$invalidSubsystems = @($index.subsystems | Where-Object {
  [string]::IsNullOrWhiteSpace($_.id) -or [string]::IsNullOrWhiteSpace($_.responsibility) -or
  @($_.qualificationCriteriaMet).Count -lt 3 -or @($_.version4Evidence).Count -lt 1 -or
  $_.nltxMapping.status -notin $allowedNltxStatuses -or $null -eq $_.relatedSubsystems -or $null -eq $_.excludedCandidates
})
$unknownOwners = @($rows | Where-Object { $_.primary_owner -notin @($index.subsystems | ForEach-Object id) })
$summary = [pscustomobject]@{
  baseline_files = $baseline.Count
  tsv_rows = $rows.Count
  unclassified = $unclassified.Count
  duplicate_primary_owner = $duplicatePaths.Count
  path_drift = $pathDrift.Count
  missing_primary_owner = $missingOwner.Count
  invalid_classification_or_status = $invalidRows.Count
  invalid_subsystems = $invalidSubsystems.Count
  unknown_owners = $unknownOwners.Count
}
$summary | Format-List | Out-String | Write-Output
if ($summary.unclassified -or $summary.duplicate_primary_owner -or $summary.path_drift -or $summary.missing_primary_owner -or $summary.invalid_classification_or_status -or $summary.invalid_subsystems -or $summary.unknown_owners -or $summary.baseline_files -ne $summary.tsv_rows) {
  throw 'Version4 subsystem coverage validation failed.'
}
Write-Output 'Version4 subsystem coverage validation passed.'
