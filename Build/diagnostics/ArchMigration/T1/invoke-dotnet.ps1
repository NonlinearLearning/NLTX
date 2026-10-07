param(
  [Parameter(Mandatory = $true)]
  [string] $Name,
  [Parameter(Mandatory = $true)]
  [ValidateSet('restore', 'build', 'run')]
  [string] $Operation,
  [Parameter(Mandatory = $true)]
  [string] $Project,
  [string[]] $ExtraArgs = @()
)

$ErrorActionPreference = 'Stop'
$root = (Resolve-Path '.').Path
$projectPath = Join-Path $root $Project
$projectDirectory = Split-Path -Parent $projectPath
$diagnosticsDirectory = Join-Path $root 'Build/diagnostics/ArchMigration/T1'
$logPath = Join-Path $diagnosticsDirectory "$Name.log"
$ledgerPath = Join-Path $diagnosticsDirectory 'command-ledger.jsonl'
$outputDirectory = Join-Path $root 'Build/bin/Terraria.Arch.Verification'
$intermediateDirectory = Join-Path $root 'Build/obj/Terraria.Arch.Verification'

function Get-FileHashes([string[]] $Paths) {
  foreach ($path in ($Paths | Sort-Object -Unique)) {
    if (Test-Path -LiteralPath $path -PathType Leaf) {
      $file = Get-Item -LiteralPath $path
      [pscustomobject]@{
        path = [System.IO.Path]::GetRelativePath($root, $file.FullName)
        sha256 = (Get-FileHash -Algorithm SHA256 -LiteralPath $file.FullName).Hash
      }
    }
  }
}

$sourcePaths = @(
  Get-ChildItem -LiteralPath $projectDirectory -Recurse -File -Include '*.cs', '*.csproj' |
    ForEach-Object FullName
)
$sourceHashesBefore = @(Get-FileHashes $sourcePaths)
$sourceHashInput = ($sourceHashesBefore | ForEach-Object { "$($_.path)=$($_.sha256)" }) -join "`n"
$sourceAggregateBefore = [Convert]::ToHexString(
  [Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes($sourceHashInput)))

$inputPaths = @(
  $sourcePaths
  (Join-Path $root 'global.json')
  (Join-Path $root 'Directory.Build.props')
  (Join-Path $root 'Directory.Build.targets')
  (Join-Path $intermediateDirectory 'project.assets.json')
)
$inputHashesBefore = @(Get-FileHashes $inputPaths)
$command = "dotnet $Operation $Project $($ExtraArgs -join ' ')".Trim()
$commandArgs = @($Operation, $projectPath) + $ExtraArgs
$startedAt = Get-Date
$output = @(& dotnet @commandArgs 2>&1)
$exitCode = $LASTEXITCODE
$finishedAt = Get-Date
$outputText = ($output | ForEach-Object { "$_" }) -join "`n"
@(
  "Command: $command"
  "Started: $($startedAt.ToString('o'))"
  "Finished: $($finishedAt.ToString('o'))"
  "ExitCode: $exitCode"
  ''
  $outputText
) | Set-Content -Encoding utf8 -LiteralPath $logPath

$dllHashes = @(Get-FileHashes @((Get-ChildItem -LiteralPath $outputDirectory -Recurse -File -Filter '*.dll' -ErrorAction SilentlyContinue | ForEach-Object FullName)))
$pdbHashes = @(Get-FileHashes @((Get-ChildItem -LiteralPath $outputDirectory -Recurse -File -Filter '*.pdb' -ErrorAction SilentlyContinue | ForEach-Object FullName)))
$assetHashes = @(Get-FileHashes @((Join-Path $intermediateDirectory 'project.assets.json')))
$warningCount = [regex]::Matches($outputText, '(?im)\bwarning\s+[A-Z]{2,}\d+:').Count
$errorCount = [regex]::Matches($outputText, '(?im)\berror\s+[A-Z]{2,}\d+:').Count
$record = [pscustomobject]@{
  name = $Name
  project = $Project
  operation = $Operation
  command = $command
  exitCode = $exitCode
  warningCount = $warningCount
  errorCount = $errorCount
  outputPath = [System.IO.Path]::GetRelativePath($root, $logPath)
  sourceHashBefore = $sourceAggregateBefore
  sourceFilesBefore = $sourceHashesBefore
  dllHashes = $dllHashes
  pdbHashes = $pdbHashes
  inputHashesBefore = $inputHashesBefore
  assetsHashesAfter = $assetHashes
  startedAt = $startedAt.ToString('o')
  finishedAt = $finishedAt.ToString('o')
}
Add-Content -Encoding utf8 -LiteralPath $ledgerPath -Value ($record | ConvertTo-Json -Depth 8 -Compress)

$output | ForEach-Object { Write-Output $_ }
Write-Output "T1 command result: exit=$exitCode warnings=$warningCount errors=$errorCount log=$($record.outputPath)"
if ($exitCode -ne 0) {
  exit $exitCode
}
