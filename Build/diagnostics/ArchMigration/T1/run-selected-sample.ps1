$ErrorActionPreference = 'Stop'
$root = (Resolve-Path '.').Path
$project = 'Test/Terraria.Arch.Verification/Terraria.Arch.Verification.csproj'
$projectPath = Join-Path $root $project
$projectDirectory = Split-Path -Parent $projectPath
$diagnostics = Join-Path $root 'Build/diagnostics/ArchMigration/T1'
$outputDirectory = Join-Path $root 'Build/bin/Terraria.Arch.Verification'
$intermediateDirectory = Join-Path $root 'Build/obj/Terraria.Arch.Verification'
$logPath = Join-Path $diagnostics 'core-risk-sample-explicit-project.log'
$ledgerPath = Join-Path $diagnostics 'command-ledger.jsonl'
$caseArgs = @(
  '--case', 'world.id-reuse',
  '--case', 'world.isalive-worldid-boundary',
  '--case', 'component.struct-class-access-critical',
  '--case', 'query.composition-critical',
  '--case', 'command-buffer.create-and-groups-critical'
)
$commandArgs = @('--project', $projectPath, '--no-build', '--no-restore', '--') + $caseArgs
$command = "dotnet run --project $project --no-build --no-restore -- $($caseArgs -join ' ')"
$sourceFiles = @(
  Get-ChildItem -LiteralPath $projectDirectory -Recurse -File -Include '*.cs', '*.csproj' |
    ForEach-Object FullName
)
$sourceHashes = @(
  foreach ($path in ($sourceFiles | Sort-Object -Unique)) {
    [pscustomobject]@{
      path = [IO.Path]::GetRelativePath($root, $path)
      sha256 = (Get-FileHash -Algorithm SHA256 -LiteralPath $path).Hash
    }
  }
)
$sourceInput = ($sourceHashes | ForEach-Object { "$($_.path)=$($_.sha256)" }) -join "`n"
$sourceHash = [Convert]::ToHexString(
  [Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes($sourceInput)))
$inputHashes = @(
  foreach ($path in @(
    (Join-Path $root 'global.json'),
    (Join-Path $root 'Directory.Build.props'),
    (Join-Path $root 'Directory.Build.targets'),
    (Join-Path $root $project),
    (Join-Path $intermediateDirectory 'project.assets.json')
  )) {
    [pscustomobject]@{
      path = [IO.Path]::GetRelativePath($root, $path)
      sha256 = (Get-FileHash -Algorithm SHA256 -LiteralPath $path).Hash
    }
  }
)
$startedAt = Get-Date
$output = @(& dotnet run @commandArgs 2>&1)
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
$dllHashes = @(
  Get-ChildItem -LiteralPath $outputDirectory -Recurse -File -Filter '*.dll' |
    ForEach-Object { [pscustomobject]@{ path = [IO.Path]::GetRelativePath($root, $_.FullName); sha256 = (Get-FileHash -Algorithm SHA256 $_.FullName).Hash } }
)
$pdbHashes = @(
  Get-ChildItem -LiteralPath $outputDirectory -Recurse -File -Filter '*.pdb' |
    ForEach-Object { [pscustomobject]@{ path = [IO.Path]::GetRelativePath($root, $_.FullName); sha256 = (Get-FileHash -Algorithm SHA256 $_.FullName).Hash } }
)
$warningCount = [regex]::Matches($outputText, '(?im)\bwarning\s+[A-Z]{2,}\d+:').Count
$errorCount = [regex]::Matches($outputText, '(?im)\berror\s+[A-Z]{2,}\d+:').Count
$record = [pscustomobject]@{
  name = 'core-risk-sample-explicit-project'
  project = $project
  operation = 'run'
  command = $command
  exitCode = $exitCode
  warningCount = $warningCount
  errorCount = $errorCount
  outputPath = [IO.Path]::GetRelativePath($root, $logPath)
  sourceHash = $sourceHash
  sourceFiles = $sourceHashes
  dllHashes = $dllHashes
  pdbHashes = $pdbHashes
  inputHashes = $inputHashes
  startedAt = $startedAt.ToString('o')
  finishedAt = $finishedAt.ToString('o')
}
Add-Content -Encoding utf8 -LiteralPath $ledgerPath -Value ($record | ConvertTo-Json -Depth 7 -Compress)
$output | ForEach-Object { Write-Output $_ }
Write-Output "T1 command result: exit=$exitCode warnings=$warningCount errors=$errorCount log=$($record.outputPath)"
if ($exitCode -ne 0) {
  exit $exitCode
}
