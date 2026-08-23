$tracePath = 'D:\TRbackup\NLTX\Build\diagnostics\server-ecs-convergence\P9-worldgen\current-stage-trace\legacy-stage-trace.jsonl'
$stdoutPath = 'D:\TRbackup\NLTX\Build\diagnostics\server-ecs-convergence\P9-worldgen\current-stage-trace\legacy-stage-trace.stdout.log'
$stderrPath = 'D:\TRbackup\NLTX\Build\diagnostics\server-ecs-convergence\P9-worldgen\current-stage-trace\legacy-stage-trace.stderr.log'
$executablePath = 'D:\TRbackup\NLTX\Build\bin\TerrariaServer\Debug\net40\TerrariaServer.exe'
$configPath = 'D:\TRbackup\NLTX\Build\worldgen-oracle\stage-trace-1456.config.txt'

Remove-Item -LiteralPath $tracePath, $stdoutPath, $stderrPath -Force -ErrorAction SilentlyContinue
$env:NLTX_WORLDGEN_STAGE_TRACE_PATH = $tracePath
$env:NLTX_WORLDGEN_STAGE_TRACE_FILTER = 'Terrain,Mount Caves,Dirt Layer Caves,Rock Layer Caves,Surface Caves,Wavy Caves,Generate Ice Biome,Full Desert,Shinies,Dungeon,Planting Trees,Settle Liquids,Tile Cleanup,Final Cleanup'
$process = Start-Process -FilePath $executablePath -ArgumentList '-config', $configPath `
  -WorkingDirectory (Split-Path -Parent $executablePath) -WindowStyle Hidden `
  -RedirectStandardOutput $stdoutPath -RedirectStandardError $stderrPath -PassThru

$deadline = (Get-Date).AddSeconds(600)
while ((Get-Date) -lt $deadline) {
  if (Test-Path -LiteralPath $tracePath) {
    $lineCount = (Get-Content -LiteralPath $tracePath | Measure-Object -Line).Lines
    $hasFinalStage = Select-String -LiteralPath $tracePath -Pattern '"stage":"Final Cleanup"' -Quiet
    if ($hasFinalStage) {
      Start-Sleep -Seconds 5
      break
    }
  }

  if ($process.HasExited) {
    break
  }

  Start-Sleep -Seconds 2
}

$traceLines = 0
if (Test-Path -LiteralPath $tracePath) {
  $traceLines = (Get-Content -LiteralPath $tracePath | Measure-Object -Line).Lines
}

[pscustomobject]@{
  ProcessId = $process.Id
  Exited = $process.HasExited
  ExitCode = if ($process.HasExited) { $process.ExitCode } else { $null }
  TraceExists = Test-Path -LiteralPath $tracePath
  TraceLines = $traceLines
} | Format-List

if (-not $process.HasExited) {
  Stop-Process -Id $process.Id -Force
  Write-Output 'oracle-process-stopped-after-trace-capture'
}

Get-Content -LiteralPath $stdoutPath -Tail 30 -ErrorAction SilentlyContinue
