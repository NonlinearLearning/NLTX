[CmdletBinding()]
param(
  [Parameter(Mandatory = $true)]
  [string]$TargetManifest,
  [Parameter(Mandatory = $true)]
  [string]$SourceOutput,
  [Parameter(Mandatory = $true)]
  [string]$TargetOutput,
  [string]$SourceRoot = 'D:\TRbackup\Version4',
  [string]$TargetRoot = '.',
  [string]$ProjectOrAssembly = 'unknown',
  [string]$CoverageFile = 'docs\迁移参考表\Version4源码覆盖.tsv',
  [string]$MemberScope,
  [ValidateRange(1, 64)]
  [int]$MaxDegreeOfParallelism = 4,
  [switch]$IncludeGenerated
)

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$scannerProject = Join-Path $PSScriptRoot 'Version4MemberMigrationScanner\Version4MemberMigrationScanner.csproj'
$scannerDll = Join-Path $repositoryRoot 'Build\bin\Version4MemberMigrationScanner\Debug\net10.0\Version4MemberMigrationScanner.dll'
$serialDotnetWrapper = Join-Path $repositoryRoot 'Build\Tools\Invoke-SerialDotnet.ps1'
$sourceOutputPath = if ([IO.Path]::IsPathRooted($SourceOutput)) { [IO.Path]::GetFullPath($SourceOutput) } else { [IO.Path]::GetFullPath((Join-Path $repositoryRoot $SourceOutput)) }
$targetOutputPath = if ([IO.Path]::IsPathRooted($TargetOutput)) { [IO.Path]::GetFullPath($TargetOutput) } else { [IO.Path]::GetFullPath((Join-Path $repositoryRoot $TargetOutput)) }
$targetManifestPath = if ([IO.Path]::IsPathRooted($TargetManifest)) { [IO.Path]::GetFullPath($TargetManifest) } else { [IO.Path]::GetFullPath((Join-Path $repositoryRoot $TargetManifest)) }
$coverageFilePath = if ($CoverageFile) { if ([IO.Path]::IsPathRooted($CoverageFile)) { [IO.Path]::GetFullPath($CoverageFile) } else { [IO.Path]::GetFullPath((Join-Path $repositoryRoot $CoverageFile)) } } else { $null }
$memberScopePath = if ($MemberScope) { if ([IO.Path]::IsPathRooted($MemberScope)) { [IO.Path]::GetFullPath($MemberScope) } else { [IO.Path]::GetFullPath((Join-Path $repositoryRoot $MemberScope)) } } else { $null }
$targetRootPath = if ([IO.Path]::IsPathRooted($TargetRoot)) { [IO.Path]::GetFullPath($TargetRoot) } else { [IO.Path]::GetFullPath((Join-Path $repositoryRoot $TargetRoot)) }

$controlledRoots = @(
  [IO.Path]::GetFullPath((Join-Path $repositoryRoot 'Build\generated')),
  [IO.Path]::GetFullPath((Join-Path $repositoryRoot 'docs\migrations'))
)
foreach ($outputPath in @($sourceOutputPath, $targetOutputPath)) {
  if (-not ($controlledRoots | Where-Object { $outputPath -eq $_ -or $outputPath.StartsWith($_ + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase) })) {
    throw "Scanner output must be under Build/generated or docs/migrations: $outputPath"
  }
}

if (-not (Test-Path -LiteralPath $scannerDll)) {
  throw "Scanner is not built. Build only $scannerProject through Invoke-SerialDotnet.ps1 before running this command."
}
if (-not (Test-Path -LiteralPath $serialDotnetWrapper)) {
  throw "Serialized dotnet wrapper does not exist: $serialDotnetWrapper"
}
if (-not (Test-Path -LiteralPath $targetManifestPath)) {
  throw "Target manifest does not exist: $targetManifestPath"
}
if ($coverageFilePath -and -not (Test-Path -LiteralPath $coverageFilePath)) {
  throw "Coverage file does not exist: $coverageFilePath"
}
if ($memberScopePath -and -not (Test-Path -LiteralPath $memberScopePath)) {
  throw "Member scope does not exist: $memberScopePath"
}
if (-not (Test-Path -LiteralPath $SourceRoot -PathType Container)) {
  throw "Source root does not exist: $SourceRoot"
}
if (-not (Test-Path -LiteralPath $targetRootPath -PathType Container)) {
  throw "Target root does not exist: $targetRootPath"
}

$arguments = @('--mode', 'source', '--root', (Resolve-Path $SourceRoot).Path, '--output', $sourceOutputPath)
if ($coverageFilePath) {
  $arguments += @('--coverage-file', $coverageFilePath)
}
if ($memberScopePath) {
  $arguments += @('--member-scope', $memberScopePath)
}
$arguments += @('--max-degree-of-parallelism', $MaxDegreeOfParallelism.ToString([Globalization.CultureInfo]::InvariantCulture))
if ($IncludeGenerated) {
  $arguments += '--include-generated'
}
$sourceScannerArguments = @($scannerDll) + $arguments
& $serialDotnetWrapper @sourceScannerArguments
if ($LASTEXITCODE -ne 0) {
  throw "Source scan failed with exit code $LASTEXITCODE"
}

$targetScannerArguments = @(
  $scannerDll,
  '--mode', 'target',
  '--root', $targetRootPath,
  '--target-manifest', $targetManifestPath,
  '--output', $targetOutputPath,
  '--target-root', 'src',
  '--project-or-assembly', $ProjectOrAssembly,
  '--max-degree-of-parallelism', $MaxDegreeOfParallelism.ToString([Globalization.CultureInfo]::InvariantCulture)
)
& $serialDotnetWrapper @targetScannerArguments
if ($LASTEXITCODE -ne 0) {
  throw "Target scan failed with exit code $LASTEXITCODE"
}

Write-Output "Snapshots written: $sourceOutputPath and $targetOutputPath"
Write-Output "This command intentionally does not invent or overwrite Version4-member-migration-map.json. Populate the explicit memberScope and decisions from the snapshots, then audit them."
