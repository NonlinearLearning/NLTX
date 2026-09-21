[CmdletBinding()]
param(
  [Parameter(Mandatory = $true)]
  [string]$Ledger,
  [string]$TargetIndex,
  [string]$Output = 'Build/generated/version4-member-migration-audit.json'
)

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$ledgerPath = if ([IO.Path]::IsPathRooted($Ledger)) { [IO.Path]::GetFullPath($Ledger) } else { [IO.Path]::GetFullPath((Join-Path $repositoryRoot $Ledger)) }
$auditScript = Join-Path $repositoryRoot '.agents\skills\version4-member-migration-ledger\scripts\audit_ledger.py'
$outputPath = if ([IO.Path]::IsPathRooted($Output)) { [IO.Path]::GetFullPath($Output) } else { [IO.Path]::GetFullPath((Join-Path $repositoryRoot $Output)) }
$controlledRoots = @(
  [IO.Path]::GetFullPath((Join-Path $repositoryRoot 'Build\generated')),
  [IO.Path]::GetFullPath((Join-Path $repositoryRoot 'docs\migration\ledgers'))
)
if (-not ($controlledRoots | Where-Object { $outputPath -eq $_ -or $outputPath.StartsWith($_ + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase) })) {
  throw "Audit output must be under Build/generated or docs/migration/ledgers: $outputPath"
}
if (-not (Test-Path -LiteralPath $ledgerPath)) {
  throw "Ledger does not exist: $ledgerPath"
}
if (-not (Test-Path -LiteralPath $auditScript)) {
  throw "Audit script does not exist: $auditScript"
}

$arguments = @($auditScript, '--ledger', $ledgerPath, '--repo-root', $repositoryRoot, '--output', $outputPath)
if ($TargetIndex) {
  $targetIndexPath = if ([IO.Path]::IsPathRooted($TargetIndex)) { [IO.Path]::GetFullPath($TargetIndex) } else { [IO.Path]::GetFullPath((Join-Path $repositoryRoot $TargetIndex)) }
  $arguments += @('--target-index', $targetIndexPath)
}
& python @arguments
$exitCode = $LASTEXITCODE
if ($exitCode -ne 0) {
  throw "Version4 member migration audit failed with exit code $exitCode. See $outputPath"
}
Write-Output "Version4 member migration audit passed: $outputPath"
