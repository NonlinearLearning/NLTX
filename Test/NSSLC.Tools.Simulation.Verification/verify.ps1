param(
  [Parameter(Mandatory = $true)]
  [string]$WorldPath,
  [string]$SecondWorldPath = '',
  [string]$OutputDirectory = '',
  [switch]$FixtureHostBuild
)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$worldPath = (Resolve-Path -LiteralPath $WorldPath).Path
$simulationProject = Join-Path $repoRoot 'src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj'
$clockVerificationProject = Join-Path $repoRoot 'Test/NSSLC.Application.Simulation.Verification/NSSLC.Application.Simulation.Verification.csproj'
$tileEntityFixtureVerificationProject = Join-Path $repoRoot 'Test/Terraria.TileEntityHostFixtureVerification/Terraria.TileEntityHostFixtureVerification.csproj'
$artifactRoot = Join-Path $repoRoot 'Build/bin'
$buildProperties = @()
if ($FixtureHostBuild) {
  $artifactRoot = Join-Path $artifactRoot 'FixtureHost'
  $buildProperties += '-p:FixtureHostBuild=true'
}
$clockVerificationDll = Join-Path $artifactRoot 'NSSLC.Application.Simulation.Verification/Debug/net10.0/NSSLC.Application.Simulation.Verification.dll'
$tileEntityFixtureVerificationDll = Join-Path $artifactRoot 'Terraria.TileEntityHostFixtureVerification/Debug/net10.0/Terraria.TileEntityHostFixtureVerification.dll'
$hostDll = Join-Path $artifactRoot 'NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll'

if ([string]::IsNullOrWhiteSpace($OutputDirectory)) {
  $OutputDirectory = Join-Path $repoRoot 'Build/diagnostics/NonCommunicationSimulationAudit/verification-run'
}
if (-not [string]::IsNullOrWhiteSpace($SecondWorldPath)) {
  $SecondWorldPath = (Resolve-Path -LiteralPath $SecondWorldPath).Path
}

$outputRoot = [System.IO.Path]::GetFullPath($OutputDirectory)
[System.IO.Directory]::CreateDirectory($outputRoot) | Out-Null
$buildLogPath = Join-Path $outputRoot 'build.log'
& dotnet build $simulationProject --nologo -v:minimal -clp:NoSummary @buildProperties *> $buildLogPath
$buildExitCode = $LASTEXITCODE
$buildWarningCount = (Select-String -LiteralPath $buildLogPath -Pattern ':\s+warning\s+[A-Z]+\d+:').Count
$buildErrorCount = (Select-String -LiteralPath $buildLogPath -Pattern ':\s+error\s+[A-Z]+\d+:').Count
if ($buildExitCode -ne 0) {
  throw "The non-communication simulation build failed with exit code $buildExitCode. See $buildLogPath"
}

if (-not (Test-Path -LiteralPath $hostDll -PathType Leaf)) {
  throw "The simulation host was not produced at '$hostDll'."
}

$clockBuildLogPath = Join-Path $outputRoot 'clock-verification-build.log'
& dotnet build $clockVerificationProject --nologo -v:minimal -clp:NoSummary @buildProperties *> $clockBuildLogPath
$clockBuildExitCode = $LASTEXITCODE
$clockBuildWarningCount = (Select-String -LiteralPath $clockBuildLogPath -Pattern ':\s+warning\s+[A-Z]+\d+:').Count
$clockBuildErrorCount = (Select-String -LiteralPath $clockBuildLogPath -Pattern ':\s+error\s+[A-Z]+\d+:').Count
if ($clockBuildExitCode -ne 0) {
  throw "The world-clock verifier build failed with exit code $clockBuildExitCode. See $clockBuildLogPath"
}
if (-not (Test-Path -LiteralPath $clockVerificationDll -PathType Leaf)) {
  throw "The world-clock verifier was not produced at '$clockVerificationDll'."
}

$clockRunLogPath = Join-Path $outputRoot 'clock-verification.log'
& dotnet $clockVerificationDll *> $clockRunLogPath
$clockRunExitCode = $LASTEXITCODE
if ($clockRunExitCode -ne 0) {
  throw "The world-clock verifier failed with exit code $clockRunExitCode. See $clockRunLogPath"
}

$tileEntityFixtureBuildLogPath = Join-Path $outputRoot 'tile-entity-fixture-build.log'
& dotnet build $tileEntityFixtureVerificationProject --nologo -v:minimal -clp:NoSummary @buildProperties *> $tileEntityFixtureBuildLogPath
$tileEntityFixtureBuildExitCode = $LASTEXITCODE
$tileEntityFixtureBuildWarningCount = (Select-String -LiteralPath $tileEntityFixtureBuildLogPath -Pattern ':\s+warning\s+[A-Z]+\d+:').Count
$tileEntityFixtureBuildErrorCount = (Select-String -LiteralPath $tileEntityFixtureBuildLogPath -Pattern ':\s+error\s+[A-Z]+\d+:').Count
if ($tileEntityFixtureBuildExitCode -ne 0) {
  throw "The TileEntity fixture verifier build failed with exit code $tileEntityFixtureBuildExitCode. See $tileEntityFixtureBuildLogPath"
}
if (-not (Test-Path -LiteralPath $tileEntityFixtureVerificationDll -PathType Leaf)) {
  throw "The TileEntity fixture verifier was not produced at '$tileEntityFixtureVerificationDll'."
}

function Invoke-Simulation {
  param(
    [string[]]$Arguments,
    [string]$ReportPath,
    [int]$ExpectedExitCode = 0
  )

  foreach ($stalePath in @($ReportPath, ($ReportPath + '.switch.json'))) {
    if (Test-Path -LiteralPath $stalePath -PathType Leaf) {
      Remove-Item -LiteralPath $stalePath -Force
    }
  }

  $processArguments = @($hostDll) + $Arguments
  $runLogPath = $ReportPath + '.run.log'
  & dotnet @processArguments *> $runLogPath
  $actualExitCode = $LASTEXITCODE
  if ($actualExitCode -ne $ExpectedExitCode) {
    throw "Simulation command exited $actualExitCode; expected $ExpectedExitCode. Report: $ReportPath. Output: $runLogPath"
  }
  if (-not (Test-Path -LiteralPath $ReportPath -PathType Leaf)) {
    throw "Simulation did not write its report: $ReportPath"
  }

  Get-Content -LiteralPath $ReportPath -Raw | ConvertFrom-Json
}

function Assert-True {
  param([bool]$Condition, [string]$Message)
  if (-not $Condition) {
    throw $Message
  }
}

$evidence = [System.Collections.Generic.List[object]]::new()
$evidence.Add([pscustomobject]@{
  Scenario = 'World clock pause, rates, and day/night boundaries'
  Project = $clockVerificationProject
  ExitCode = $clockRunExitCode
  Log = $clockRunLogPath
})
foreach ($playerCount in 0, 1, 2) {
  $reportPath = Join-Path $outputRoot "smoke-600-players-$playerCount.json"
  $spatialProbeArguments = @(
    '--spatial-entity-probe', 'true',
    '--spawn-npc', '1',
    '--spawn-npc', '1',
    '--spawn-npc', '2',
    '--spawn-npc', '3',
    '--spawn-npc', '16',
    '--spawn-npc', '22',
    '--spawn-npc', '37',
    '--spawn-npc', '4',
    '--spawn-npc', '488'
  )
  if ($playerCount -gt 0) {
    $spatialMovementFrames = [System.Collections.Generic.List[object]]::new()
    for ($tick = 1; $tick -le 90; $tick++) {
      for ($playerSlot = 0; $playerSlot -lt $playerCount; $playerSlot++) {
        $spatialMovementFrames.Add([ordered]@{
          tick = $tick
          playerSlot = $playerSlot
          horizontal = if ($tick -le 10) { 1 } else { 0 }
          jump = $tick -eq 12
          useItem = $false
        })
      }
    }
    $spatialInputPath = Join-Path $outputRoot "spatial-probe-input-$playerCount.json"
    [System.IO.File]::WriteAllText(
      $spatialInputPath,
      (ConvertTo-Json -InputObject @{ frames = $spatialMovementFrames } -Depth 5),
      [System.Text.UTF8Encoding]::new($false))
    $spatialProbeArguments += @('--input-script', $spatialInputPath)
  }
  if ($playerCount -eq 1) {
    $spatialProbeArguments += @('--npc-reset-preflight-probe', 'true')
  }
  if ($playerCount -eq 2) {
    $spatialProbeArguments += @('--player-cleanup-preflight-probe', 'true')
  }
  $simulationArguments = @(
    $worldPath, '600', '--players', "$playerCount", '--report', $reportPath
  ) + $spatialProbeArguments
  $report = Invoke-Simulation $simulationArguments $reportPath
  Assert-True ($report.Succeeded -and $report.IsPublished) "Player-$playerCount world did not publish."
  Assert-True ($report.TickNumber -eq 600) "Player-$playerCount run committed $($report.TickNumber) ticks."
  Assert-True ($report.LiquidTicks -eq $report.TickNumber) "Player-$playerCount run did not schedule liquid work on every committed tick."
  Assert-True ($report.PhaseOrder -contains 9) "Player-$playerCount run omitted the world-systems phase."
  Assert-True ($report.LocalPlayerCount -eq $playerCount) "Player-$playerCount run hydrated the wrong player count."
  Assert-True ($report.SpatialEntityProbeEnabled) "Player-$playerCount run omitted the spatial entity probe."
  if ($playerCount -eq 1) {
    Assert-True ($report.NpcResetPreflightProbe.BorrowedResetRejectedWithoutWrites -and $report.NpcResetPreflightProbe.ReleasedBorrowAllowedRetry -and $report.NpcResetPreflightProbe.StaleNpcReferenceRejected) 'The NPC reset preflight probe did not preserve shared roots or reject stale generations.'
  }
  if ($playerCount -eq 2) {
    Assert-True ($report.PlayerCleanupPreflightProbe.InitializeRejectedBorrowedItemWithoutWrites -and $report.PlayerCleanupPreflightProbe.ClearRejectedBorrowedPlayerWithoutWrites -and $report.PlayerCleanupPreflightProbe.DisposeRejectedBorrowedPlayerWithoutDisposingStore -and $report.PlayerCleanupPreflightProbe.ReleasedBorrowAllowedDestroyRetry) 'The Player cleanup preflight probe did not preserve state and allow retry after releasing borrows.'
  }
  Assert-True ($report.FinalTime -gt $report.InitialTime) "Player-$playerCount run did not advance world time."
  $evidence.Add([pscustomobject]@{
    Scenario = "600 ticks, $playerCount players"
    Report = $reportPath
    Ticks = $report.TickNumber
  })
}

foreach ($playerCount in 0, 2) {
  $reportPath = Join-Path $outputRoot "smoke-3600-players-$playerCount.json"
  $report = Invoke-Simulation @(
    $worldPath, '3600', '--players', "$playerCount", '--report', $reportPath
  ) $reportPath
  Assert-True ($report.Succeeded -and $report.IsPublished) "Long-run player-$playerCount world did not publish."
  Assert-True ($report.TickNumber -eq 3600 -and $report.ClockRevision -eq 3600) "Player-$playerCount run did not commit 3600 simulation and clock ticks."
  Assert-True ($report.LiquidTicks -eq 3600 -and $report.PhaseOrder -contains 9) "Player-$playerCount long run omitted an expected production phase."
  Assert-True ($report.LocalPlayerCount -eq $playerCount -and $report.RuntimeNpcsUpdatedAtFinalTick) "Player-$playerCount long run did not update the expected runtime owners."
  Assert-True ($report.SourceFileUnchanged) "Player-$playerCount long run modified its source world archive."
  $evidence.Add([pscustomobject]@{
    Scenario = "3600 ticks, $playerCount players"
    Report = $reportPath
    Ticks = $report.TickNumber
    Npcs = $report.RuntimeNpcCount
  })
}

$npcSlotReportPath = Join-Path $outputRoot 'npc-slot-capacity-reuse.json'
$npcSlots = Invoke-Simulation @(
  $worldPath,
  '1',
  '--players', '0',
  '--npc-slot-probe', 'true',
  '--report', $npcSlotReportPath
) $npcSlotReportPath
$npcSlotProbe = $npcSlots.NpcSlotProbe
Assert-True ($npcSlots.Succeeded -and $npcSlotProbe.CountAtCapacity -eq $npcSlotProbe.Capacity) 'The NPC owner did not fill every slot.'
Assert-True ($npcSlotProbe.OverflowSpawnRejected) 'The NPC owner accepted an allocation beyond capacity.'
Assert-True ($npcSlotProbe.ReleasedSlotWasEmpty -and $npcSlotProbe.ReusedSlot -eq $npcSlotProbe.ReleasedSlot) 'The NPC owner did not reuse the released slot.'
Assert-True ($npcSlotProbe.ReusedGeneration -gt $npcSlotProbe.ReleasedGeneration) 'NPC slot reuse did not advance its generation.'
Assert-True ($npcSlotProbe.InstanceIdChangedAfterReuse -and $npcSlotProbe.ReusedInstanceId -ne $npcSlotProbe.ReleasedInstanceId) 'NPC slot reuse did not issue a distinct instance identity.'
Assert-True ($npcSlotProbe.StaleHandleRejectedAfterReuse) 'A stale NPC handle affected the reused slot.'
$npcResetProbe = $npcSlotProbe.ResetProbe
Assert-True ($npcResetProbe.SameSlotReused -and $npcResetProbe.InstanceIdChanged) 'NPC store reset reused the previous instance identity.'
Assert-True ($npcResetProbe.StaleHandleRejected) 'A stale NPC handle affected an entity after store reset.'
Assert-True ($npcSlotProbe.FinalActiveCount -eq $npcSlotProbe.InitialActiveCount) 'The NPC slot probe leaked runtime entities.'
$evidence.Add([pscustomobject]@{
  Scenario = 'NPC capacity rejection and generation-safe slot reuse'
  Report = $npcSlotReportPath
  Capacity = $npcSlotProbe.Capacity
  ReleasedSlot = $npcSlotProbe.ReleasedSlot
  ReleasedGeneration = $npcSlotProbe.ReleasedGeneration
  ReusedGeneration = $npcSlotProbe.ReusedGeneration
  ReleasedInstanceId = $npcSlotProbe.ReleasedInstanceId
  ReusedInstanceId = $npcSlotProbe.ReusedInstanceId
  ResetProbe = $npcResetProbe
})

$npcDespawnPolicies = @(
  [pscustomobject]@{
    NetId = 1
    Policy = 'OutsideLivingPlayerRangeAfterGracePeriod'
    ExpectedRemoved = $true
  },
  [pscustomobject]@{
    NetId = 2
    Policy = 'OutsideLivingPlayerRangeAfterGracePeriod'
    ExpectedRemoved = $true
  },
  [pscustomobject]@{
    NetId = 3
    Policy = 'OutsideLivingPlayerRangeAfterGracePeriod'
    ExpectedRemoved = $true
  },
  [pscustomobject]@{
    NetId = 16
    Policy = 'OutsideLivingPlayerRangeAfterGracePeriod'
    ExpectedRemoved = $true
  },
  [pscustomobject]@{
    NetId = 22
    Policy = 'Persistent'
    ExpectedRemoved = $false
  },
  [pscustomobject]@{
    NetId = 37
    Policy = 'Persistent'
    ExpectedRemoved = $false
  },
  [pscustomobject]@{
    NetId = 488
    Policy = 'Persistent'
    ExpectedRemoved = $false
  }
)
foreach ($npcPolicy in $npcDespawnPolicies) {
  $npcDespawnReportPath = Join-Path $outputRoot "npc-natural-despawn-$($npcPolicy.NetId)-300.json"
  $npcDespawn = Invoke-Simulation @(
    $worldPath,
    '300',
    '--players', '0',
    '--npc-despawn-probe', 'true',
    '--npc-despawn-probe-net-id', [string]$npcPolicy.NetId,
    '--report', $npcDespawnReportPath
  ) $npcDespawnReportPath
  $expectedDespawnCount = if ($npcPolicy.ExpectedRemoved) { 1 } else { 0 }
  Assert-True ($npcDespawn.Succeeded -and $npcDespawn.TickNumber -eq 300) "The NPC $($npcPolicy.NetId) despawn run did not reach its grace period."
  Assert-True ($npcDespawn.NpcDespawnProbeNetId -eq $npcPolicy.NetId -and $npcDespawn.NpcDespawnProbePolicy -eq $npcPolicy.Policy) "The NPC $($npcPolicy.NetId) did not report its declared despawn policy."
  Assert-True ($npcDespawn.DespawnedCount -eq $expectedDespawnCount -and $npcDespawn.NpcDespawnProbeRemoved -eq $npcPolicy.ExpectedRemoved) "The NPC $($npcPolicy.NetId) did not follow its natural despawn policy."
  $evidence.Add([pscustomobject]@{
    Scenario = "Natural despawn policy for NPC $($npcPolicy.NetId)"
    Report = $npcDespawnReportPath
    Policy = $npcDespawn.NpcDespawnProbePolicy
    DespawnedCount = $npcDespawn.DespawnedCount
    Removed = $npcDespawn.NpcDespawnProbeRemoved
    Slot = $npcDespawn.NpcDespawnProbeSlot
  })
}

$pausedReportPath = Join-Path $outputRoot 'paused-clock-60.json'
$paused = Invoke-Simulation @(
  $worldPath,
  '60',
  '--players', '0',
  '--world-time-rate', '0',
  '--report', $pausedReportPath
) $pausedReportPath
Assert-True ($paused.Succeeded -and $paused.TickNumber -eq 60) 'The paused-clock run did not commit its simulation ticks.'
Assert-True ($paused.FinalTime -eq $paused.InitialTime -and $paused.FinalDayTime -eq $paused.InitialDayTime) 'World time changed while --world-time-rate was zero.'
$evidence.Add([pscustomobject]@{
  Scenario = 'Production kernel with paused world clock'
  Report = $pausedReportPath
  Ticks = $paused.TickNumber
  InitialTime = $paused.InitialTime
  FinalTime = $paused.FinalTime
})

$expiryProbeReportPath = Join-Path $outputRoot 'world-item-expiry-probe.json'
$expiryProbe = Invoke-Simulation @(
  $worldPath,
  '1',
  '--players', '0',
  '--world-item-probe', 'expired',
  '--report', $expiryProbeReportPath
) $expiryProbeReportPath
Assert-True ($expiryProbe.ExpiredCount -eq 1 -and $expiryProbe.RuntimeWorldItemCount -eq 0) 'An expired world item retained its slot or payload.'
$evidence.Add([pscustomobject]@{
  Scenario = 'World item expiry releases the runtime slot'
  Report = $expiryProbeReportPath
  ExpiredCount = $expiryProbe.ExpiredCount
  ActiveCount = $expiryProbe.RuntimeWorldItemCount
})

$worldItemPhysicsTicks = 300
$physicsProbeReportPath = Join-Path $outputRoot 'world-item-physics-probe.json'
$physicsProbe = Invoke-Simulation @(
  $worldPath,
  "$worldItemPhysicsTicks",
  '--players', '0',
  '--world-item-probe', 'physics',
  '--report', $physicsProbeReportPath
) $physicsProbeReportPath
$physicsProbeItems = @($physicsProbe.WorldItemStates)
Assert-True ($physicsProbeItems.Count -eq 1 -and $physicsProbe.RuntimeWorldItemCount -eq 1) 'The world-item physics probe did not retain its item.'
Assert-True ($physicsProbeItems[0].Y -gt $physicsProbe.WorldItemProbeInitialY) 'The world item did not fall under gravity.'
Assert-True ([Math]::Abs($physicsProbeItems[0].VelocityY) -lt 0.001) 'The world item did not settle against the loaded tile collision map.'
Assert-True ($physicsProbeItems[0].TimeLeft -eq (6000 - $worldItemPhysicsTicks)) 'The world-item lifetime did not advance once per committed tick.'
$evidence.Add([pscustomobject]@{
  Scenario = 'World item gravity, tile collision, and lifetime advance'
  Report = $physicsProbeReportPath
  InitialY = $physicsProbe.WorldItemProbeInitialY
  FinalY = $physicsProbeItems[0].Y
  FinalVerticalVelocity = $physicsProbeItems[0].VelocityY
  TimeLeft = $physicsProbeItems[0].TimeLeft
})

$partialPickupReportPath = Join-Path $outputRoot 'world-item-partial-pickup-probe.json'
$partialPickup = Invoke-Simulation @(
  $worldPath,
  '1',
  '--players', '1',
  '--world-item-probe', 'partial-pickup',
  '--report', $partialPickupReportPath
) $partialPickupReportPath
$partialWorldItems = @($partialPickup.WorldItemStates)
Assert-True ($partialPickup.PlayerStates[0].Gel -eq 9999) 'The partial-pickup probe did not fill the destination stack to its maximum.'
Assert-True ($partialPickup.PartialPickupCount -eq 1 -and $partialPickup.RuntimeWorldItemCount -eq 1) 'A partial pickup did not retain the world-item slot.'
Assert-True ($partialWorldItems.Count -eq 1 -and $partialWorldItems[0].Stack -eq 4) 'The world item did not retain exactly the unaccepted stack.'
$evidence.Add([pscustomobject]@{
  Scenario = 'Partial world-item pickup preserves remaining stack'
  Report = $partialPickupReportPath
  RemainingStack = $partialWorldItems[0].Stack
  Gel = $partialPickup.PlayerStates[0].Gel
})

$fullInventoryReportPath = Join-Path $outputRoot 'world-item-full-inventory-probe.json'
$fullInventory = Invoke-Simulation @(
  $worldPath,
  '1',
  '--players', '1',
  '--world-item-probe', 'full-inventory',
  '--report', $fullInventoryReportPath
) $fullInventoryReportPath
$fullInventoryWorldItems = @($fullInventory.WorldItemStates)
Assert-True ($fullInventory.PickupCount -eq 0 -and $fullInventory.PartialPickupCount -eq 0) 'A full inventory accepted an item without available space.'
Assert-True ($fullInventoryWorldItems.Count -eq 1 -and $fullInventoryWorldItems[0].Stack -eq 5) 'A full inventory lost or changed the unaccepted world-item stack.'
$evidence.Add([pscustomobject]@{
  Scenario = 'Full inventory preserves the whole world-item stack'
  Report = $fullInventoryReportPath
  RemainingStack = $fullInventoryWorldItems[0].Stack
  InventorySlotsUsed = $fullInventory.PlayerStates[0].InventorySlotsUsed
})

$movementFrames = [System.Collections.Generic.List[object]]::new()
for ($tick = 1; $tick -le 90; $tick++) {
  $movementFrames.Add([ordered]@{
    tick = $tick
    playerSlot = 0
    horizontal = if ($tick -le 10) { 1 } else { 0 }
    jump = $tick -eq 12
    useItem = $false
  })
}
$movementInputPath = Join-Path $outputRoot 'movement-jump-input.json'
[System.IO.File]::WriteAllText(
  $movementInputPath,
  (ConvertTo-Json -InputObject @{ frames = $movementFrames } -Depth 5),
  [System.Text.UTF8Encoding]::new($false))
$movementReportPath = Join-Path $outputRoot 'movement-jump-landing-90.json'
$movement = Invoke-Simulation @(
  $worldPath,
  '90',
  '--players', '1',
  '--input-script', $movementInputPath,
  '--report', $movementReportPath
) $movementReportPath
$initialMovementPosition = $movement.InitialPlayerPositions[0]
$finalMovementPosition = $movement.PlayerPositions[0]
Assert-True ($finalMovementPosition.X -gt $initialMovementPosition.X + 1) 'Scripted horizontal input did not move the player across the map.'
Assert-True ($movement.PlayerStates[0].JumpCount -eq 1 -and $movement.PlayerStates[0].LandingCount -ge 2) 'The player did not jump and land on the world collision surface.'
Assert-True ([Math]::Abs($finalMovementPosition.Y - $initialMovementPosition.Y) -le 1) 'The player did not return to the starting ground height after landing.'
$evidence.Add([pscustomobject]@{
  Scenario = 'Scripted movement, jump, and landing'
  Report = $movementReportPath
  InitialPosition = $initialMovementPosition
  FinalPosition = $finalMovementPosition
  Jumps = $movement.PlayerStates[0].JumpCount
  Landings = $movement.PlayerStates[0].LandingCount
})

$twoPlayerFrames = [System.Collections.Generic.List[object]]::new()
for ($tick = 1; $tick -le 60; $tick++) {
  foreach ($playerSlot in 0, 1) {
    $twoPlayerFrames.Add([ordered]@{
      tick = $tick
      playerSlot = $playerSlot
      horizontal = 1
      jump = $false
      useItem = $false
    })
  }
}
$twoPlayerInputPath = Join-Path $outputRoot 'scripted-two-player-input.json'
[System.IO.File]::WriteAllText(
  $twoPlayerInputPath,
  (ConvertTo-Json -InputObject @{ frames = $twoPlayerFrames } -Depth 5),
  [System.Text.UTF8Encoding]::new($false))
$twoPlayerReportPath = Join-Path $outputRoot 'scripted-two-player-movement-60.json'
$twoPlayerRun = Invoke-Simulation @(
  $worldPath,
  '60',
  '--players', '2',
  '--input-script', $twoPlayerInputPath,
  '--report', $twoPlayerReportPath
) $twoPlayerReportPath
$playerZeroStart = @($twoPlayerRun.InitialPlayerPositions | Where-Object Slot -eq 0) | Select-Object -First 1
$playerZeroEnd = @($twoPlayerRun.PlayerPositions | Where-Object Slot -eq 0) | Select-Object -First 1
$playerOneStart = @($twoPlayerRun.InitialPlayerPositions | Where-Object Slot -eq 1) | Select-Object -First 1
$playerOneEnd = @($twoPlayerRun.PlayerPositions | Where-Object Slot -eq 1) | Select-Object -First 1
Assert-True ($twoPlayerRun.LocalPlayerCount -eq 2) 'The scripted two-player scenario created the wrong number of players.'
Assert-True ($null -ne $playerZeroEnd -and $playerZeroEnd.X -gt $playerZeroStart.X + 1) 'Scripted input did not move player slot 0.'
Assert-True ($null -ne $playerOneEnd -and $playerOneEnd.X -gt $playerOneStart.X + 1) 'Scripted input did not move player slot 1.'
$evidence.Add([pscustomobject]@{
  Scenario = 'Independent scripted movement for two players'
  Report = $twoPlayerReportPath
  PlayerZeroStart = $playerZeroStart
  PlayerZeroEnd = $playerZeroEnd
  PlayerOneStart = $playerOneStart
  PlayerOneEnd = $playerOneEnd
})

$neutralInputPath = Join-Path $outputRoot 'neutral-input.json'
[System.IO.File]::WriteAllText(
  $neutralInputPath,
  '{"frames":[]}',
  [System.Text.UTF8Encoding]::new($false))
$demonEyeReportPath = Join-Path $outputRoot 'demon-eye-ai-160.json'
$demonEye = Invoke-Simulation @(
  $worldPath,
  '160',
  '--players', '1',
  '--spawn-npc', '2',
  '--input-script', $neutralInputPath,
  '--report', $demonEyeReportPath
) $demonEyeReportPath
$initialDemonEye = @($demonEye.InitialRuntimeNpcStates | Where-Object NetId -eq 2) | Select-Object -First 1
$demonEyeState = @($demonEye.RuntimeNpcStates | Where-Object Slot -eq $initialDemonEye.Slot) | Select-Object -First 1
Assert-True ($demonEye.ContentSupportManifest -eq 'simulation-core-v3') 'The simulation did not publish the current NPC support manifest.'
Assert-True ($null -ne $demonEyeState -and $demonEyeState.Action -eq 0) 'The Demon Eye did not return to its hover phase after the dive cycle.'
$demonEyeStartX = $initialDemonEye.X
$demonEyeStartY = $initialDemonEye.Y
$demonEyeTravel = [Math]::Sqrt(
  [Math]::Pow($demonEyeState.X - $demonEyeStartX, 2) +
  [Math]::Pow($demonEyeState.Y - $demonEyeStartY, 2))
Assert-True ($demonEyeTravel -gt 32) 'The Demon Eye AI did not move from its spawn position.'
$evidence.Add([pscustomobject]@{
  Scenario = 'Demon Eye hover and dive behavior'
  Report = $demonEyeReportPath
  FinalAction = $demonEyeState.Action
  TravelPixels = $demonEyeTravel
})

$guideReportPath = Join-Path $outputRoot 'guide-town-ai-200.json'
$guide = Invoke-Simulation @(
  $worldPath,
  '200',
  '--players', '1',
  '--spawn-npc', '22',
  '--input-script', $neutralInputPath,
  '--report', $guideReportPath
) $guideReportPath
$expectedGuideStartX = $guide.InitialPlayerPositions[0].X + 48
$initialGuide = @(
  $guide.InitialRuntimeNpcStates |
    Where-Object { $_.NetId -eq 22 -and [Math]::Abs($_.X - $expectedGuideStartX) -lt 0.01 }
) | Select-Object -First 1
$guideState = @($guide.RuntimeNpcStates | Where-Object Slot -eq $initialGuide.Slot) | Select-Object -First 1
Assert-True ($null -ne $guideState -and $guideState.Action -eq -1) 'The Guide did not enter its leftward daytime patrol phase.'
$guideTravel = [Math]::Sqrt(
  [Math]::Pow($guideState.X - $initialGuide.X, 2) +
  [Math]::Pow($guideState.Y - $initialGuide.Y, 2))
Assert-True ($guideTravel -gt 24) 'The Guide patrol did not change its runtime position.'
$evidence.Add([pscustomobject]@{
  Scenario = 'Guide daytime patrol movement'
  Report = $guideReportPath
  FinalAction = $guideState.Action
  TravelPixels = $guideTravel
})

$guideNightReportPath = Join-Path $outputRoot 'guide-night-return-home-1.json'
$guideNight = Invoke-Simulation @(
  $worldPath,
  '1',
  '--players', '0',
  '--world-time-rate', '50000',
  '--npc-night-probe', 'true',
  '--report', $guideNightReportPath
) $guideNightReportPath
$guideNightProbe = $guideNight.NpcNightProbe
Assert-True (
  $null -ne $guideNightProbe -and
  $guideNightProbe.InitialDayTime -and
  -not $guideNightProbe.FinalDayTime -and
  $guideNightProbe.Task -eq 'GuideReturnHome' -and
  $guideNightProbe.TaskPhase -eq 'Running' -and
  $guideNightProbe.TaskCursor -eq 1) 'The Guide did not enter its nighttime return-home task after the dusk transition.'
$evidence.Add([pscustomobject]@{
  Scenario = 'Guide nighttime return-home task lifecycle'
  Report = $guideNightReportPath
  InitialDayTime = $guideNightProbe.InitialDayTime
  FinalDayTime = $guideNightProbe.FinalDayTime
  Task = $guideNightProbe.Task
  TaskPhase = $guideNightProbe.TaskPhase
  TaskCursor = $guideNightProbe.TaskCursor
})

$npcRelationReportPath = Join-Path $outputRoot 'npc-parent-relation-probe.json'
$npcRelationRun = Invoke-Simulation @(
  $worldPath,
  '1',
  '--players', '0',
  '--npc-relation-probe', 'true',
  '--report', $npcRelationReportPath
) $npcRelationReportPath
$npcRelationProbe = $npcRelationRun.NpcRelationProbe
Assert-True (
  $null -ne $npcRelationProbe -and
  $npcRelationProbe.Attached -and
  $npcRelationProbe.RelationReferenceMatched -and
  $npcRelationProbe.ParentHitApplied -and
  $npcRelationProbe.ParentLifeOwnerMatched -and
  $npcRelationProbe.ParentLifeChanged -and
  $npcRelationProbe.ChildMirroredParentLife -and
  $npcRelationProbe.ParentHitWasNonlethal -and
  $npcRelationProbe.ParentReleased -and
  $npcRelationProbe.RelationDetachedAfterParentRelease -and
  $npcRelationProbe.ParentSlotReused -and
  $npcRelationProbe.ReplacementHasFreshIdentity -and
  $npcRelationProbe.ChildHitAfterParentReleaseAppliedLocally -and
  $npcRelationProbe.ReplacementUnaffectedByChildHit -and
  $npcRelationProbe.ChildReleased -and
  $npcRelationProbe.ReplacementReleased -and
  $npcRelationProbe.LethalParentHitApplied -and
  $npcRelationProbe.LethalParentDropOwnerMatched -and
  $npcRelationProbe.LethalParentReleasedWithChild -and
  $npcRelationProbe.PartialPairRejectedAtCapacity -and
  $npcRelationProbe.CapacityCountRestoredAfterRejection -and
  $npcRelationProbe.CapacityFillersReleased -and
  $npcRelationProbe.FinalActiveCount -eq $npcRelationProbe.InitialActiveCount) 'The NPC parent relation probe did not attach, detach, and release its generated pair.'
$evidence.Add([pscustomobject]@{
  Scenario = 'NPC parent relation generation and parent-release cleanup'
  Report = $npcRelationReportPath
  ParentNetId = $npcRelationProbe.ParentNetId
  ChildNetId = $npcRelationProbe.ChildNetId
  ParentSlot = $npcRelationProbe.ParentSlot
  ChildSlot = $npcRelationProbe.ChildSlot
  Attached = $npcRelationProbe.Attached
  RelationReferenceMatched = $npcRelationProbe.RelationReferenceMatched
  ParentHitApplied = $npcRelationProbe.ParentHitApplied
  ParentLifeOwnerMatched = $npcRelationProbe.ParentLifeOwnerMatched
  ParentLifeChanged = $npcRelationProbe.ParentLifeChanged
  ChildMirroredParentLife = $npcRelationProbe.ChildMirroredParentLife
  RelationDetachedAfterParentRelease = $npcRelationProbe.RelationDetachedAfterParentRelease
  ParentSlotReused = $npcRelationProbe.ParentSlotReused
  ChildHitAfterParentReleaseAppliedLocally = $npcRelationProbe.ChildHitAfterParentReleaseAppliedLocally
  ReplacementUnaffectedByChildHit = $npcRelationProbe.ReplacementUnaffectedByChildHit
  LethalParentHitApplied = $npcRelationProbe.LethalParentHitApplied
  LethalParentDropOwnerMatched = $npcRelationProbe.LethalParentDropOwnerMatched
  LethalParentReleasedWithChild = $npcRelationProbe.LethalParentReleasedWithChild
  PartialPairRejectedAtCapacity = $npcRelationProbe.PartialPairRejectedAtCapacity
  CapacityCountRestoredAfterRejection = $npcRelationProbe.CapacityCountRestoredAfterRejection
  CapacityFillersReleased = $npcRelationProbe.CapacityFillersReleased
})

$npcRelationChainReportPath = Join-Path $outputRoot 'npc-relation-chain-probe.json'
$npcRelationChainRun = Invoke-Simulation @(
  $worldPath,
  '1',
  '--players', '0',
  '--npc-relation-chain-probe', 'true',
  '--report', $npcRelationChainReportPath
) $npcRelationChainReportPath
$npcRelationChainProbe = $npcRelationChainRun.NpcRelationChainProbe
Assert-True (
  $null -ne $npcRelationChainProbe -and
  $npcRelationChainProbe.Attached -and
  $npcRelationChainProbe.ReferencesStable -and
  $npcRelationChainProbe.TailHitApplied -and
  $npcRelationChainProbe.TailHitResolvedToRoot -and
  $npcRelationChainProbe.RootLifeChanged -and
  $npcRelationChainProbe.RootDeathReleasedChain -and
  $npcRelationChainProbe.RootSlotReused -and
  $npcRelationChainProbe.ReplacementHasFreshIdentity -and
  $npcRelationChainProbe.StaleTailRejectedAfterReuse -and
  $npcRelationChainProbe.MiddleNodeReleased -and
  $npcRelationChainProbe.MiddleReleaseDetachedTail -and
  $npcRelationChainProbe.DetachedTailHitAppliedLocally -and
  $npcRelationChainProbe.RootUnaffectedByDetachedTailHit -and
  $npcRelationChainProbe.SeveredChainActiveCountRestored -and
  $npcRelationChainProbe.PartialChainRejectedAtCapacity -and
  $npcRelationChainProbe.CapacityCountRestoredAfterRejection -and
  $npcRelationChainProbe.CapacityFillersReleased -and
  $npcRelationChainProbe.FinalActiveCount -eq $npcRelationChainProbe.InitialActiveCount) 'The NPC relation chain probe did not preserve adjacent references, root ownership, or capacity cleanup.'
$evidence.Add([pscustomobject]@{
  Scenario = 'NPC relation chain root ownership and partial-chain rollback'
  Report = $npcRelationChainReportPath
  NetIds = $npcRelationChainProbe.NetIds
  Slots = $npcRelationChainProbe.Slots
  Attached = $npcRelationChainProbe.Attached
  ReferencesStable = $npcRelationChainProbe.ReferencesStable
  TailHitResolvedToRoot = $npcRelationChainProbe.TailHitResolvedToRoot
  RootDeathReleasedChain = $npcRelationChainProbe.RootDeathReleasedChain
  RootSlotReused = $npcRelationChainProbe.RootSlotReused
  StaleTailRejectedAfterReuse = $npcRelationChainProbe.StaleTailRejectedAfterReuse
  MiddleNodeReleased = $npcRelationChainProbe.MiddleNodeReleased
  MiddleReleaseDetachedTail = $npcRelationChainProbe.MiddleReleaseDetachedTail
  DetachedTailHitAppliedLocally = $npcRelationChainProbe.DetachedTailHitAppliedLocally
  RootUnaffectedByDetachedTailHit = $npcRelationChainProbe.RootUnaffectedByDetachedTailHit
  SeveredChainActiveCountRestored = $npcRelationChainProbe.SeveredChainActiveCountRestored
  PartialChainRejectedAtCapacity = $npcRelationChainProbe.PartialChainRejectedAtCapacity
  CapacityCountRestoredAfterRejection = $npcRelationChainProbe.CapacityCountRestoredAfterRejection
  CapacityFillersReleased = $npcRelationChainProbe.CapacityFillersReleased
})

$tileEntityRemovalReportPath = Join-Path $outputRoot 'tile-entity-removal-probe.json'
$tileEntityRemoval = Invoke-Simulation @(
  $worldPath,
  '3',
  '--players', '0',
  '--tile-entity-removal-probe', 'true',
  '--report', $tileEntityRemovalReportPath
) $tileEntityRemovalReportPath
Assert-True ($tileEntityRemoval.TileEntityRemovalProbeEnabled -and $tileEntityRemoval.TileEntityProbeRemoved) 'The invalidated TileEntity remained in the owner store.'
Assert-True (
  -not $tileEntityRemoval.TileEntityProbeScheduled -and
  $tileEntityRemoval.TileEntityUpdatePassCount -eq 4 -and
  $tileEntityRemoval.TrainingDummyTileEntityBound -and
  $tileEntityRemoval.TrainingDummyTileEntityRemoved -and
  $tileEntityRemoval.TrainingDummyNpcReleased -and
  $tileEntityRemoval.TrainingDummyActiveNpcCountRestored) 'The removed TileEntity or its TrainingDummy binding was still scheduled after its cleanup tick.'
$evidence.Add([pscustomobject]@{
  Scenario = 'PlayerAbove Logic Sensor transition, anchored removal, and TrainingDummy release'
  Report = $tileEntityRemovalReportPath
  LogicSensorCheck = 'PlayerAbove'
  LogicSensorExpectedTransition = 'on -> off before anchor invalidation'
  Removed = $tileEntityRemoval.TileEntityProbeRemoved
  Scheduled = $tileEntityRemoval.TileEntityProbeScheduled
  UpdatePasses = $tileEntityRemoval.TileEntityUpdatePassCount
})

$tileEntityFixturePath = Join-Path $outputRoot 'tile-entity-host-roundtrip-fixture.wld'
$tileEntityFixtureManifestPath = Join-Path $outputRoot 'tile-entity-host-roundtrip-fixture.json'
$tileEntityFixturePrepareLogPath = Join-Path $outputRoot 'tile-entity-host-roundtrip-fixture-prepare.json'
& dotnet $tileEntityFixtureVerificationDll prepare `
  $worldPath $tileEntityFixturePath $tileEntityFixtureManifestPath *> $tileEntityFixturePrepareLogPath
$tileEntityFixturePrepareExitCode = $LASTEXITCODE
if ($tileEntityFixturePrepareExitCode -ne 0) {
  throw "The TileEntity host fixture preparation failed with exit code $tileEntityFixturePrepareExitCode. See $tileEntityFixturePrepareLogPath"
}
$tileEntityFixtureManifest = Get-Content -LiteralPath $tileEntityFixtureManifestPath -Raw | ConvertFrom-Json
$tileEntityFixtureReportPath = Join-Path $outputRoot 'tile-entity-host-roundtrip.json'
$tileEntityFixtureSavePath = Join-Path $outputRoot 'tile-entity-host-roundtrip.wld'
$tileEntityFixtureHostReportPath = Join-Path $outputRoot 'tile-entity-host-roundtrip-host.json'
$tileEntityFixtureHost = Invoke-Simulation @(
  $tileEntityFixturePath,
  '1',
  '--players', '0',
  '--save', $tileEntityFixtureSavePath,
  '--switch-world', $tileEntityFixtureSavePath,
  '--tile-entity-reload-probe', 'true',
  '--report', $tileEntityFixtureHostReportPath
) $tileEntityFixtureHostReportPath
Assert-True (
  $tileEntityFixtureHost.Succeeded -and
  $tileEntityFixtureHost.SaveCommitted -and
  $tileEntityFixtureHost.TileEntityUpdatePassCount -eq $tileEntityFixtureManifest.ExpectedTileEntityUpdatePassCount -and
  $tileEntityFixtureHost.RuntimeNpcCount -eq 1) 'The TileEntity fixture did not tick and save the sensor and TrainingDummy owners.'
$tileEntityFixtureSaveVerifyLogPath = Join-Path $outputRoot 'tile-entity-host-roundtrip-save-verify.json'
& dotnet $tileEntityFixtureVerificationDll verify `
  $tileEntityFixtureSavePath $tileEntityFixtureManifestPath 'first-save' *> $tileEntityFixtureSaveVerifyLogPath
$tileEntityFixtureSaveVerifyExitCode = $LASTEXITCODE
if ($tileEntityFixtureSaveVerifyExitCode -ne 0) {
  throw "The saved TileEntity fixture verification failed with exit code $tileEntityFixtureSaveVerifyExitCode. See $tileEntityFixtureSaveVerifyLogPath"
}
$tileEntityFixtureSwitchReportPath = $tileEntityFixtureHostReportPath + '.switch.json'
$tileEntityFixtureSwitchReport = Get-Content -LiteralPath $tileEntityFixtureSwitchReportPath -Raw | ConvertFrom-Json
Assert-True (
  $tileEntityFixtureSwitchReport.Succeeded -and
  $tileEntityFixtureSwitchReport.Switches.Count -eq 1) 'The saved TileEntity fixture did not reload into a fresh world session.'
$tileEntityReload = $tileEntityFixtureSwitchReport.Switches[0].TileEntityReload
Assert-True (
  $tileEntityReload.RuntimeReferencesChanged -and
  $tileEntityReload.PreviousReferencesRejected -and
  $tileEntityReload.ReloadedReferencesResolve -and
  $tileEntityReload.ScheduleMatchesRecords -and
  $tileEntityReload.TrainingDummyBindingValid -and
  $tileEntityReload.LogicSensorCheck -eq $tileEntityFixtureManifest.LogicSensor.LogicCheck -and
  -not $tileEntityReload.LogicSensorOn -and
  $tileEntityReload.UpdatePassCount -eq $tileEntityFixtureManifest.ExpectedTileEntityUpdatePassCount) 'TileEntity roots, schedule, sensor state, or TrainingDummy binding did not survive fresh-session reload.'
$tileEntityFixtureReloadVerifyLogPath = Join-Path $outputRoot 'tile-entity-host-roundtrip-reload-verify.json'
& dotnet $tileEntityFixtureVerificationDll verify `
  $tileEntityFixtureSavePath $tileEntityFixtureManifestPath 'fresh-session-reload' *> $tileEntityFixtureReloadVerifyLogPath
$tileEntityFixtureReloadVerifyExitCode = $LASTEXITCODE
if ($tileEntityFixtureReloadVerifyExitCode -ne 0) {
  throw "The reloaded TileEntity fixture verification failed with exit code $tileEntityFixtureReloadVerifyExitCode. See $tileEntityFixtureReloadVerifyLogPath"
}
$evidence.Add([pscustomobject]@{
  Scenario = 'TileEntity state, typed roots, schedule, and TrainingDummy save/reload'
  Fixture = $tileEntityFixturePath
  Manifest = $tileEntityFixtureManifestPath
  HostReport = $tileEntityFixtureHostReportPath
  SavedWorld = $tileEntityFixtureSavePath
  SaveVerification = $tileEntityFixtureSaveVerifyLogPath
  FreshSessionSwitchReport = $tileEntityFixtureSwitchReportPath
  ReloadVerification = $tileEntityFixtureReloadVerifyLogPath
  SensorCheck = $tileEntityFixtureManifest.LogicSensor.LogicCheck
  SensorStateAfterTick = $tileEntityReload.LogicSensorOn
  TileEntityUpdatePasses = $tileEntityReload.UpdatePassCount
  RuntimeReferencesChanged = $tileEntityReload.RuntimeReferencesChanged
  TrainingDummyBindingValid = $tileEntityReload.TrainingDummyBindingValid
})

$chestItemSavePath = Join-Path $outputRoot 'chest-item-owner-save.wld'
$chestItemSetReportPath = Join-Path $outputRoot 'chest-item-owner-set.json'
$chestItemSet = Invoke-Simulation @(
  $worldPath,
  '1',
  '--players', '0',
  '--chest-item-probe', 'set',
  '--save', $chestItemSavePath,
  '--report', $chestItemSetReportPath
) $chestItemSetReportPath
Assert-True ($chestItemSet.SaveCommitted -and $chestItemSet.ChestItemProbe.Type -eq 23) 'The runtime chest item mutation was not saved.'
Assert-True ($chestItemSet.ChestItemProbe.Slot -eq 0 -and $chestItemSet.ChestItemProbe.Stack -in 7, 8) 'The runtime chest owner did not commit the expected probe stack.'
$chestItemReloadReportPath = Join-Path $outputRoot 'chest-item-owner-reload.json'
$chestItemReload = Invoke-Simulation @(
  $chestItemSavePath,
  '0',
  '--players', '0',
  '--chest-item-probe', 'inspect',
  '--report', $chestItemReloadReportPath
) $chestItemReloadReportPath
Assert-True ($chestItemReload.IsPublished -and $chestItemReload.ChestItemProbe.Type -eq $chestItemSet.ChestItemProbe.Type) 'The fresh session did not reload the mutated chest item.'
Assert-True ($chestItemReload.ChestItemProbe.Anchor.X -eq $chestItemSet.ChestItemProbe.Anchor.X -and $chestItemReload.ChestItemProbe.Anchor.Y -eq $chestItemSet.ChestItemProbe.Anchor.Y) 'The fresh session inspected a different chest anchor.'
Assert-True ($chestItemReload.ChestItemProbe.Slot -eq $chestItemSet.ChestItemProbe.Slot -and $chestItemReload.ChestItemProbe.Stack -eq $chestItemSet.ChestItemProbe.Stack) 'The chest item slot or stack changed across save/reload.'
$evidence.Add([pscustomobject]@{
  Scenario = 'Runtime chest item mutation and fresh reload'
  Report = $chestItemSetReportPath
  ReloadReport = $chestItemReloadReportPath
  Anchor = $chestItemReload.ChestItemProbe.Anchor
  Slot = $chestItemReload.ChestItemProbe.Slot
  ItemType = $chestItemReload.ChestItemProbe.Type
  Stack = $chestItemReload.ChestItemProbe.Stack
})

$pressurePlateReportPath = Join-Path $outputRoot 'pressure-plate-probe-60.json'
$pressurePlateSavePath = Join-Path $outputRoot 'pressure-plate-probe.wld'
$pressurePlate = Invoke-Simulation @(
  $worldPath,
  '60',
  '--players', '1',
  '--pressure-plate-probe', 'true',
  '--save', $pressurePlateSavePath,
  '--report', $pressurePlateReportPath
) $pressurePlateReportPath
Assert-True ($pressurePlate.PressurePlateProbeEnabled) 'The pressure-plate probe was not installed.'
Assert-True ($pressurePlate.PressurePlateActivations -gt 0) 'The player did not press the wired pressure plate.'
Assert-True ($pressurePlate.WiredActuatorToggleCount -gt 0) 'The pressure plate did not toggle its wired actuator.'
$pressurePlateUnsupportedDevices = @($pressurePlate.RecognizedUnsupportedWiredDeviceTileTypes)
Assert-True ($pressurePlateUnsupportedDevices.Count -eq 1 -and $pressurePlateUnsupportedDevices[0] -eq 144) 'The triggered circuit did not report its unsupported timer device.'
$pressurePlateAnchor = $pressurePlate.PressurePlateProbeAnchor
$actuatorX = [int]$pressurePlateAnchor.X + 5
$actuatorInspection = "{0},{1}" -f $actuatorX, $pressurePlateAnchor.Y
$pressurePlateReloadPath = Join-Path $outputRoot 'pressure-plate-reload.json'
$pressurePlateReload = Invoke-Simulation @(
  $pressurePlateSavePath,
  '0',
  '--players', '0',
  '--inspect-tile-area', $actuatorInspection,
  '--report', $pressurePlateReloadPath
) $pressurePlateReloadPath
$reloadedPressurePlate = @($pressurePlateReload.PressurePlateAnchors | Where-Object {
  $_.X -eq $pressurePlateAnchor.X -and $_.Y -eq $pressurePlateAnchor.Y
})
Assert-True ($pressurePlate.SaveCommitted -and $reloadedPressurePlate.Count -eq 1) 'The pressure-plate anchor did not survive save/reload.'
$reloadedActuator = $pressurePlateReload.TileAreaSample | Where-Object {
  $_.X -eq $actuatorX -and $_.Y -eq $pressurePlateAnchor.Y
} | Select-Object -First 1
Assert-True ($null -ne $reloadedActuator -and $reloadedActuator.HasActuator -and $reloadedActuator.IsActuated) 'The actuated tile state did not survive save/reload.'
$evidence.Add([pscustomobject]@{
  Scenario = 'Pressure plate and wired actuator save/reload'
  Report = $pressurePlateReportPath
  Activations = $pressurePlate.PressurePlateActivations
  ActuatorToggles = $pressurePlate.WiredActuatorToggleCount
  RecognizedUnsupportedWiredDeviceTileTypes = $pressurePlateUnsupportedDevices
  ReloadReport = $pressurePlateReloadPath
  PersistedAnchorCount = $reloadedPressurePlate.Count
  PersistedActuator = $reloadedActuator.IsActuated
})

$frames = [System.Collections.Generic.List[object]]::new()
for ($tick = 1; $tick -le 3600; $tick++) {
  $horizontal = if ($tick -ge 2100 -and $tick -le 2180) { 1 } else { 0 }
  $frames.Add([ordered]@{
    tick = $tick
    playerSlot = 0
    horizontal = $horizontal
    jump = $false
    useItem = $true
  })
}
$inputScriptPath = Join-Path $outputRoot 'combat-drop-pickup-input.json'
$inputJson = ConvertTo-Json -InputObject @{ frames = $frames } -Depth 5
[System.IO.File]::WriteAllText(
  $inputScriptPath,
  $inputJson,
  [System.Text.UTF8Encoding]::new($false))

$gameplayReportPath = Join-Path $outputRoot 'gameplay-3600.json'
$gameplaySavePath = Join-Path $outputRoot 'gameplay-3600.wld'
$gameplay = Invoke-Simulation @(
  $worldPath,
  '3600',
  '--players', '1',
  '--spawn-npc', '3',
  '--liquid-probe', 'true',
  '--liquid-checksum', 'true',
  '--input-script', $inputScriptPath,
  '--save', $gameplaySavePath,
  '--report', $gameplayReportPath
) $gameplayReportPath
Assert-True ($gameplay.Succeeded -and $gameplay.TickNumber -eq 3600) 'The gameplay run did not complete 3600 ticks.'
Assert-True ($gameplay.LiquidTicks -eq 3600) 'The gameplay run did not schedule liquid work on all ticks.'
Assert-True ($null -ne $gameplay.LiquidProbeCoordinate) 'The gameplay run did not activate a liquid tile for the owner-writeback probe.'
Assert-True ($gameplay.LiquidStateChangeCount -gt 0) 'The liquid probe did not commit a changed liquid amount back to the world owner.'
$gameplayProbeLiquidAmount = ($gameplay.TileAreaSample | Measure-Object -Property LiquidAmount -Sum).Sum
Assert-True ($gameplayProbeLiquidAmount -gt 0) 'The settled liquid probe did not leave water in its test container.'
Assert-True ($gameplay.NpcDeathDropCount -gt 0) 'The scripted Zombie did not drop Gel.'
Assert-True (@($gameplay.NaturalSpawnedNpcTypes).Count -gt 0) 'The production natural-spawn pass did not allocate an NPC.'
Assert-True ($gameplay.ShotsFired -gt 0 -and $gameplay.AcceptedNpcHitCount -gt 0) 'The ordinary arrow projectile did not hit an NPC.'
Assert-True ($gameplay.ProjectileTileCollisionCount -gt 0) 'The ordinary arrow projectile did not exercise tile collision.'
Assert-True ($gameplay.PickupCount -gt 0 -and $gameplay.PlayerStates[0].Gel -gt 0) 'The player did not collect the Gel drop.'
Assert-True ($gameplay.PlayerPositions[0].X -gt $gameplay.InitialPlayerPositions[0].X) 'Scripted horizontal input did not move the player during gameplay.'
Assert-True ($gameplay.RuntimeWorldItemCount -eq 0) 'A picked-up world item remained active.'
Assert-True ($gameplay.LastPickupDistanceToPlayerHitbox -gt 0 -and $gameplay.LastPickupDistanceToPlayerHitbox -le 48) 'The near-range pickup path was not exercised.'
Assert-True ($gameplay.SaveCommitted -and $gameplay.SourceFileUnchanged) 'The gameplay world snapshot did not save cleanly.'
$evidence.Add([pscustomobject]@{
  Scenario = '3600 ticks, combat, drop, pickup, save'
  Report = $gameplayReportPath
  NpcDeaths = $gameplay.NpcDeathDropCount
  Pickups = $gameplay.PickupCount
  Gel = $gameplay.PlayerStates[0].Gel
})

$contactInputPath = Join-Path $outputRoot 'npc-contact-input.json'
$contactInput = ConvertTo-Json -InputObject @{ frames = @([ordered]@{
  tick = 1
  playerSlot = 0
  horizontal = 0
  jump = $false
  useItem = $false
}) } -Depth 4
[System.IO.File]::WriteAllText(
  $contactInputPath,
  $contactInput,
  [System.Text.UTF8Encoding]::new($false))
$deathReportPath = Join-Path $outputRoot 'npc-contact-deaths-3600.json'
$deathReport = Invoke-Simulation @(
  $worldPath,
  '3600',
  '--players', '1',
  '--spawn-npc', '3',
  '--input-script', $contactInputPath,
  '--report', $deathReportPath
) $deathReportPath
Assert-True ($deathReport.Succeeded -and $deathReport.TickNumber -eq 3600) 'The NPC contact scenario did not complete.'
Assert-True ($deathReport.PlayerStates[0].PveDeathCount -ge 2) 'NPC contact did not exercise death and respawn cycles.'
$initialPlayerRoot = $deathReport.InitialPlayerRootReferences | Where-Object Slot -eq 0
$finalPlayerRoot = $deathReport.PlayerStates | Where-Object Slot -eq 0
Assert-True ($null -ne $initialPlayerRoot -and $initialPlayerRoot.Reference.EntityId.Value -eq $finalPlayerRoot.RootReference.EntityId.Value -and $initialPlayerRoot.Reference.RuntimeId.Value -eq $finalPlayerRoot.RootReference.RuntimeId.Value) 'An ordinary respawn replaced the Player root identity.'
$initialPlayerPosition = $deathReport.InitialPlayerPositions | Where-Object Slot -eq 0
$finalPlayerPosition = $deathReport.PlayerPositions | Where-Object Slot -eq 0
Assert-True ([Math]::Abs($initialPlayerPosition.X - $finalPlayerPosition.X) -lt 0.01 -and [Math]::Abs($initialPlayerPosition.Y - $finalPlayerPosition.Y) -lt 0.01) 'An ordinary respawn did not return the Player to its spawn point.'
$evidence.Add([pscustomobject]@{
  Scenario = 'NPC contact damage, death, and respawn'
  Report = $deathReportPath
  PveDeaths = $deathReport.PlayerStates[0].PveDeathCount
  PlayerRootRetained = $true
  RespawnPositionRestored = $true
  Lifecycle = $deathReport.PlayerStates[0].Lifecycle
})

$roundTripReportPath = Join-Path $outputRoot 'save-reload.json'
$liquidProbeInspection = "{0},{1}" -f `
  $gameplay.LiquidProbeCoordinate.X,
  $gameplay.LiquidProbeCoordinate.Y
$roundTrip = Invoke-Simulation @(
  $gameplaySavePath, '1', '--players', '0', '--liquid-checksum', 'true',
  '--inspect-tile-area', $liquidProbeInspection, '--report', $roundTripReportPath
) $roundTripReportPath
Assert-True ($roundTrip.Succeeded -and $roundTrip.IsPublished) 'The saved world did not publish after reload.'
Assert-True ($roundTrip.InitialTime -eq $gameplay.FinalTime) 'The saved world time did not survive reload.'
$gameplayProbeState = ConvertTo-Json -InputObject $gameplay.TileAreaSample -Depth 5 -Compress
$reloadedProbeState = ConvertTo-Json -InputObject $roundTrip.TileAreaSample -Depth 5 -Compress
Assert-True ($reloadedProbeState -eq $gameplayProbeState) 'The committed liquid probe tiles did not survive WorldFile reload.'
$evidence.Add([pscustomobject]@{
  Scenario = 'Save and reload world time'
  Report = $roundTripReportPath
  SavedTime = $gameplay.FinalTime
  ReloadedTime = $roundTrip.InitialTime
  ProbeLiquidAmount = $gameplayProbeLiquidAmount
  SavedLiquidChecksumBeforeLoadRecovery = $gameplay.FinalLiquidStateChecksum
  ReloadedLiquidChecksumAfterLoadRecovery = $roundTrip.InitialLiquidStateChecksum
})

$unsupportedWorldPath = Join-Path $repoRoot 'src/World/科研.wld'
if (Test-Path -LiteralPath $unsupportedWorldPath -PathType Leaf) {
  $unsupportedReportPath = Join-Path $outputRoot 'unsupported-world-version.json'
  $unsupported = Invoke-Simulation @(
    $unsupportedWorldPath, '600', '--report', $unsupportedReportPath
  ) $unsupportedReportPath 1
  Assert-True (-not $unsupported.Succeeded -and $unsupported.TickNumber -eq 0) 'An unsupported world version entered simulation.'
  Assert-True ($unsupported.LastLoadFailureDetail -like '*UnsupportedFormatVersion*') 'The unsupported world version did not report its format failure.'
  $evidence.Add([pscustomobject]@{
    Scenario = 'Reject unsupported WorldFile version before simulation'
    Report = $unsupportedReportPath
    LoadFailureKind = $unsupported.LastLoadFailureKind
  })
}

$cancelReportPath = Join-Path $outputRoot 'load-canceled.json'
$cancel = Invoke-Simulation @(
  $worldPath, '600', '--players', '0', '--cancel-after-ms', '1000', '--report', $cancelReportPath
) $cancelReportPath 130
Assert-True (-not $cancel.Succeeded -and $cancel.Canceled -and $cancel.TickNumber -eq 0) 'Load cancellation entered simulation.'
$evidence.Add([pscustomobject]@{
  Scenario = 'Cancel during load'
  Report = $cancelReportPath
  LoadAttempts = $cancel.LoadAttempts
})

$saveCancelReportPath = Join-Path $outputRoot 'run-canceled-after-120-ticks.json'
$saveCancelPath = Join-Path $outputRoot 'run-canceled-after-120-ticks.wld'
$saveCancel = Invoke-Simulation @(
  $worldPath,
  '1000',
  '--players', '0',
  '--save', $saveCancelPath,
  '--cancel-after-ticks', '120',
  '--report', $saveCancelReportPath
) $saveCancelReportPath 130
Assert-True ($saveCancel.Canceled -and $saveCancel.TickNumber -eq 120) 'Runtime cancellation did not stop after the requested committed tick.'
Assert-True ($saveCancel.SaveCommitted -and $saveCancel.SnapshotRevision -eq 120 -and $saveCancel.SourceFileUnchanged) 'Runtime cancellation did not save the latest committed owner snapshot.'
$saveCancelReloadPath = Join-Path $outputRoot 'run-canceled-after-120-ticks-reload.json'
$saveCancelReload = Invoke-Simulation @(
  $saveCancelPath,
  '1',
  '--players', '0',
  '--report', $saveCancelReloadPath
) $saveCancelReloadPath
Assert-True ($saveCancelReload.IsPublished -and $saveCancelReload.InitialTime -eq $saveCancel.FinalTime) 'The canceled run snapshot did not survive a fresh session reload.'
$evidence.Add([pscustomobject]@{
  Scenario = 'Runtime cancellation saves and reloads the last committed tick'
  Report = $saveCancelReportPath
  ReloadReport = $saveCancelReloadPath
  Tick = $saveCancel.TickNumber
  SavedTime = $saveCancel.FinalTime
  ReloadedTime = $saveCancelReload.InitialTime
})

$lockedSavePath = Join-Path $outputRoot 'locked-save.wld'
Copy-Item -LiteralPath $worldPath -Destination $lockedSavePath -Force
$originalLockedSaveHash = (Get-FileHash -LiteralPath $lockedSavePath -Algorithm SHA256).Hash
$saveFailureReportPath = Join-Path $outputRoot 'save-failure.json'
$lockedFile = [System.IO.File]::Open(
  $lockedSavePath,
  [System.IO.FileMode]::Open,
  [System.IO.FileAccess]::Read,
  [System.IO.FileShare]::Read)
try {
  $saveFailure = Invoke-Simulation @(
    $worldPath, '1', '--players', '0', '--save', $lockedSavePath, '--report', $saveFailureReportPath
  ) $saveFailureReportPath 1
}
finally {
  $lockedFile.Dispose()
}
$finalLockedSaveHash = (Get-FileHash -LiteralPath $lockedSavePath -Algorithm SHA256).Hash
Assert-True (-not $saveFailure.SaveCommitted -and $saveFailure.SaveFailureKind -eq 'IoFailure') 'The locked save did not fail as expected.'
Assert-True ($originalLockedSaveHash -eq $finalLockedSaveHash -and $saveFailure.PreviousSavePreserved) 'A failed save changed the previous valid world file.'
$evidence.Add([pscustomobject]@{
  Scenario = 'Save failure preserves previous world'
  Report = $saveFailureReportPath
  PreviousSavePreserved = $saveFailure.PreviousSavePreserved
})

if (-not [string]::IsNullOrWhiteSpace($SecondWorldPath)) {
  $switchReportPath = Join-Path $outputRoot 'repeated-switch.json'
  $initialReportPath = Join-Path $outputRoot 'repeated-switch-initial.json'
  $null = Invoke-Simulation @(
    $worldPath,
    '1',
    '--players', '1',
    '--report', $initialReportPath,
    '--switch-world', $SecondWorldPath,
    '--switch-world', $worldPath
  ) $initialReportPath
  $switchReport = Get-Content -LiteralPath ($initialReportPath + '.switch.json') -Raw | ConvertFrom-Json
  Assert-True ($switchReport.Succeeded -and $switchReport.Switches.Count -eq 2) 'Repeated world switches did not complete.'
  Assert-True ($switchReport.PlayerRootLifecycles.Count -eq 2) 'Repeated world switches did not report both Player root lifecycles.'
  foreach ($playerRootLifecycle in $switchReport.PlayerRootLifecycles) {
    Assert-True ($playerRootLifecycle.PreviousPlayerCount -eq 1) 'The Player root world-switch probe did not start with one Player.'
    Assert-True ($playerRootLifecycle.OldPlayerReferencesRejected -and $playerRootLifecycle.OldInventoryOwnersRejected) 'A world switch retained an old Player root reference or inventory owner.'
    Assert-True ($playerRootLifecycle.ReplacementPlayerReferencesChanged -and $playerRootLifecycle.ReplacementPlayerReferencesResolve) 'A world switch did not create a fresh, resolvable Player root.'
  }
  foreach ($switch in $switchReport.Switches) {
    Assert-True ($switch.SessionIdentityChanged -and -not $switch.PreviousSessionIsActive) 'A world switch retained its previous active session.'
    Assert-True ($switch.NewSessionIsActive -and $switch.IsPublished -and $switch.TickCommitted) 'The switched world did not publish and tick.'
    Assert-True ($switch.PreviousNpcInstanceIds.Count -gt 0 -and $switch.SwitchedNpcInstanceIds.Count -gt 0) 'The world switch identity probe did not contain NPC instances on both sides.'
    Assert-True ($switch.NpcInstanceIdentitiesDidNotOverlap -and $switch.PreviousNpcHandleRejected) 'A world switch reused an NPC instance identity or accepted an old-world handle.'
    Assert-True ($switch.LiquidTicks -eq 1) 'A switched world did not initialize and schedule its liquid phase.'
  }
  $evidence.Add([pscustomobject]@{
    Scenario = 'Repeated world switch'
    Report = $initialReportPath + '.switch.json'
    Switches = $switchReport.Switches.Count
    NpcInstanceIdentityIsolation = $true
    PlayerRootLifecycleIsolation = $true
  })
}

$summaryPath = Join-Path $outputRoot 'summary.json'
$summary = [pscustomobject]@{
  Succeeded = $true
  Build = [pscustomobject]@{
    Project = $simulationProject
    ExitCode = $buildExitCode
    WarningCount = $buildWarningCount
    ErrorCount = $buildErrorCount
    Artifact = $hostDll
    Log = $buildLogPath
  }
  ClockVerificationBuild = [pscustomobject]@{
    Project = $clockVerificationProject
    ExitCode = $clockBuildExitCode
    WarningCount = $clockBuildWarningCount
    ErrorCount = $clockBuildErrorCount
    Artifact = $clockVerificationDll
    Log = $clockBuildLogPath
    RunExitCode = $clockRunExitCode
    RunLog = $clockRunLogPath
  }
  TileEntityFixtureVerificationBuild = [pscustomobject]@{
    Project = $tileEntityFixtureVerificationProject
    ExitCode = $tileEntityFixtureBuildExitCode
    WarningCount = $tileEntityFixtureBuildWarningCount
    ErrorCount = $tileEntityFixtureBuildErrorCount
    Artifact = $tileEntityFixtureVerificationDll
    Log = $tileEntityFixtureBuildLogPath
  }
  WorldPath = $worldPath
  SecondWorldPath = if ([string]::IsNullOrWhiteSpace($SecondWorldPath)) { $null } else { $SecondWorldPath }
  Evidence = $evidence
}
[System.IO.File]::WriteAllText(
  $summaryPath,
  (ConvertTo-Json -InputObject $summary -Depth 6),
  [System.Text.UTF8Encoding]::new($false))
Write-Output "PASS: non-communication simulation verification. Evidence: $summaryPath"
exit 0
