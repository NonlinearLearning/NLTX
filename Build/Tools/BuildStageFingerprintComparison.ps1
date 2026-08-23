$stagePath = 'D:\TRbackup\NLTX\Build\diagnostics\server-ecs-convergence\P9-worldgen\current-stage-trace\stage-fingerprints-full-profile.json'
$oraclePath = 'D:\TRbackup\NLTX\Build\diagnostics\server-ecs-convergence\P9-worldgen\current-stage-trace\legacy-stage-trace.jsonl'
$differentialPath = 'D:\TRbackup\NLTX\Build\diagnostics\server-ecs-convergence\P9-worldgen\current-full-differential\legacy-worldgen-differential.json'
$differentialPath = 'D:\TRbackup\NLTX\docs\worldgen\legacy-worldgen-differential.json'
$outputPath = 'D:\TRbackup\NLTX\Build\diagnostics\server-ecs-convergence\P9-worldgen\current-stage-trace\stage-fingerprint-comparison.json'

$generated = Get-Content -Raw $stagePath | ConvertFrom-Json
$oracleRows = Get-Content $oraclePath | Where-Object { $_.Trim() } | ForEach-Object { $_ | ConvertFrom-Json }
$oracle = @{}
foreach ($row in $oracleRows) {
  $oracle[$row.stage] = $row.fingerprint
}
$oracleProfile = [ordered]@{
  width = $oracleRows[0].width
  height = $oracleRows[0].height
  worldSurface = $oracleRows[0].worldSurface
  rockLayer = $oracleRows[0].rockLayer
  worldSurfaceLow = $oracleRows[0].worldSurfaceLow
  worldSurfaceHigh = $oracleRows[0].worldSurfaceHigh
  rockLayerLow = $oracleRows[0].rockLayerLow
  rockLayerHigh = $oracleRows[0].rockLayerHigh
  leftBeachEnd = $oracleRows[0].leftBeachEnd
  rightBeachStart = $oracleRows[0].rightBeachStart
  waterLine = $oracleRows[0].waterLine
  lavaLine = $oracleRows[0].lavaLine
}

$mapping = @{
  Terrain = 'Terrain'
  Cave = 'Mount Caves'
  Biome = 'Full Desert'
  Ore = 'Shinies'
  Structure = 'Dungeon'
  Tree = 'Planting Trees'
  Liquid = 'Settle Liquids'
  Framing = 'Tile Cleanup'
  Committed = 'Final Cleanup'
}
$stages = foreach ($stage in $generated) {
  $oracleStage = $null
  if ($mapping.ContainsKey($stage.stage)) {
    $oracleStage = $mapping[$stage.stage]
  }

  $oracleFingerprint = $null
  if ($null -ne $oracleStage -and $oracle.ContainsKey($oracleStage)) {
    $oracleFingerprint = $oracle[$oracleStage]
  }

  $compared = $null -ne $oracleFingerprint
  [ordered]@{
    name = $stage.stage
    generatedFingerprint = $stage.fingerprint
    oracleStage = $oracleStage
    oracleFingerprint = $oracleFingerprint
    compared = $compared
    matches = $compared -and ($stage.fingerprint -eq $oracleFingerprint)
    reason = if ($compared) { 'projected tile-state fingerprint comparison' } `
      else { 'legacy stage not captured by selected oracle filter' }
  }
}

$differential = Get-Content -Raw $differentialPath | ConvertFrom-Json
$result = [ordered]@{
  schemaVersion = 2
  status = if (($stages | Where-Object { $_.compared -and -not $_.matches }).Count -gt 0) `
    { 'projected-stage-mismatch' } else { 'projected-stage-comparison-partial' }
  oracleParity = 'projected-only-not-full-wld'
  projection = 'world-grid-tile-state-v1'
  oracleTerrainProfile = $oracleProfile
  finalOracleFingerprint = $differential.LegacyProjectedFingerprint
  finalGeneratedFingerprint = $differential.GeneratedFingerprint
  fullDifferential = [ordered]@{
    comparedTiles = $differential.ComparedTiles
    tileMismatches = $differential.MismatchTiles
    extendedStateMismatches = $differential.ExtendedStateMismatchTiles
  }
  stages = @($stages)
}
$result | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $outputPath -Encoding utf8
Write-Output ("wrote {0} stages; compared {1}" -f $stages.Count, @($stages | Where-Object compared).Count)
