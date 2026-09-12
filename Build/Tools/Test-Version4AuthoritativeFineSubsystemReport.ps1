[CmdletBinding()]
param(
    [string]$InputPath = 'docs/迁移参考表/Version4权威模拟系统字段属性逐成员源码声明-去除ID类文件.md',
    [string]$ReportPath = 'docs/迁移参考表/Version4权威模拟系统字段属性逐成员源码声明-更细子系统拆分-去除ID类文件.md'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Resolve-RepositoryPath {
    param([Parameter(Mandatory)][string]$Path)

    if ([System.IO.Path]::IsPathRooted($Path)) {
        return [System.IO.Path]::GetFullPath($Path)
    }

    return [System.IO.Path]::GetFullPath((Join-Path (Get-Location) $Path))
}

function Split-MarkdownRow {
    param([Parameter(Mandatory)][string]$Line)

    $content = $Line.Trim()
    if (-not ($content.StartsWith('|') -and $content.EndsWith('|'))) {
        return @()
    }

    $content = $content.Substring(1, $content.Length - 2)
    return @($content -split '(?<!\\)\|' | ForEach-Object { $_.Trim() })
}

function Read-MemberRows {
    param(
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)][bool]$RequireFineSubsystem
    )

    $rows = [System.Collections.Generic.List[object]]::new()
    $parent = $null
    $fine = $null
    $lineNumber = 0
    $inMemberSection = $false

    foreach ($line in Get-Content -LiteralPath $Path) {
        $lineNumber++

        if ($line -eq '## 4. 逐成员源码声明') {
            $inMemberSection = $true
            continue
        }

        if ($line -match '^### \d+\.\d+ (?:父级子系统|子系统)：\x60([^\x60]+)\x60') {
            $parent = $Matches[1]
            $fine = $null
            continue
        }

        if ($line -match '^#### \d+\.\d+\.\d+ 细分子系统：\x60([^\x60]+)\x60') {
            $fine = $Matches[1]
            continue
        }

        if ($inMemberSection -and $line -match '^\|\s*(\d+)\s*\|') {
            $cells = @(Split-MarkdownRow $line)
            if ($cells.Count -ne 11) {
                throw "Malformed member row at $Path`:$lineNumber. Expected 11 cells, found $($cells.Count)."
            }

            if ([string]::IsNullOrWhiteSpace($parent)) {
                throw "Member row at $Path`:$lineNumber has no parent subsystem."
            }

            if ($RequireFineSubsystem -and [string]::IsNullOrWhiteSpace($fine)) {
                throw "Member row at $Path`:$lineNumber has no fine subsystem."
            }

            $rows.Add([pscustomobject]@{
                    Seq                 = [int]$cells[0]
                    Kind                = $cells[1]
                    Type                = $cells[2]
                    RelativePath        = $cells[3]
                    AbsolutePath        = $cells[4]
                    SourceLine          = [int]$cells[5]
                    SourceColumn        = [int]$cells[6]
                    Member              = $cells[7]
                    CSharpType          = $cells[8]
                    Declaration         = $cells[9]
                    OriginalDeclaration = $cells[10]
                    Cells               = [string[]]$cells
                    Parent              = $parent
                    Fine                = $fine
                })
        }
    }

    return @($rows)
}

function Get-CellSignature {
    param([Parameter(Mandatory)][pscustomobject]$Row)

    return [string]::Join([char]0x1f, $Row.Cells)
}

function Assert-Equal {
    param(
        [Parameter(Mandatory)][object]$Actual,
        [Parameter(Mandatory)][object]$Expected,
        [Parameter(Mandatory)][string]$Message
    )

    if ($Actual -ne $Expected) {
        throw "$Message. Actual: $Actual; expected: $Expected."
    }
}

$inputFullPath = Resolve-RepositoryPath $InputPath
$reportFullPath = Resolve-RepositoryPath $ReportPath
foreach ($path in @($inputFullPath, $reportFullPath)) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Required input does not exist: $path"
    }
}

$inputRows = @(Read-MemberRows -Path $inputFullPath -RequireFineSubsystem:$false)
$reportRows = @(Read-MemberRows -Path $reportFullPath -RequireFineSubsystem:$true)

Assert-Equal $inputRows.Count 2659 'Authoritative input row count mismatch'
Assert-Equal $reportRows.Count 2659 'Fine report row count mismatch'
Assert-Equal @($inputRows | Where-Object Kind -eq 'field').Count 2397 'Authoritative input field count mismatch'
Assert-Equal @($inputRows | Where-Object Kind -eq 'property').Count 262 'Authoritative input property count mismatch'
Assert-Equal @($reportRows | Where-Object Kind -eq 'field').Count 2397 'Fine report field count mismatch'
Assert-Equal @($reportRows | Where-Object Kind -eq 'property').Count 262 'Fine report property count mismatch'

foreach ($rows in @($inputRows, $reportRows)) {
    $sequences = @($rows | ForEach-Object Seq)
    Assert-Equal ($sequences | Sort-Object -Unique).Count 2659 'Sequence uniqueness mismatch'
    Assert-Equal (($sequences | Measure-Object -Minimum).Minimum) 1 'Sequence minimum mismatch'
    Assert-Equal (($sequences | Measure-Object -Maximum).Maximum) 2659 'Sequence maximum mismatch'
    $expected = 1..2659
    Assert-Equal (($sequences | Sort-Object) -join ',') ($expected -join ',') 'Sequence closure mismatch'
}

$inputBySeq = @{}
foreach ($row in $inputRows) { $inputBySeq[$row.Seq] = $row }
$reportBySeq = @{}
foreach ($row in $reportRows) {
    if ($reportBySeq.ContainsKey($row.Seq)) {
        throw "Fine report has duplicate sequence $($row.Seq)."
    }
    $reportBySeq[$row.Seq] = $row
}

foreach ($seq in 1..2659) {
    $inputRow = $inputBySeq[$seq]
    $reportRow = $reportBySeq[$seq]
    if ($inputRow.Parent -ne $reportRow.Parent -or (Get-CellSignature $inputRow) -cne (Get-CellSignature $reportRow)) {
        throw "Fine report changes source declaration cells or parent at sequence $seq."
    }
}

$idRows = @($reportRows | Where-Object {
        $fileStem = [System.IO.Path]::GetFileNameWithoutExtension($_.RelativePath)
        $fileStem -cmatch '(?:ID|IDs)$'
    })
Assert-Equal $idRows.Count 0 'Fine report contains ID-class rows'

$fineNames = @($reportRows | ForEach-Object Fine | Sort-Object -Unique)
Assert-Equal @($fineNames).Count 221 'Refined fine subsystem count mismatch'

$expectedTop20Leaderboard = @(
    [pscustomobject]@{ Rank = 1; Name = 'PlayerPetAndCompanionState'; Fields = 90; Properties = 0; Total = 90 }
    [pscustomobject]@{ Rank = 2; Name = 'PlayerBuffAndStatusEffects'; Fields = 68; Properties = 0; Total = 68 }
    [pscustomobject]@{ Rank = 3; Name = 'MountDefinitionCatalog'; Fields = 59; Properties = 0; Total = 59 }
    [pscustomobject]@{ Rank = 4; Name = 'NpcSpawnEligibilityInputs'; Fields = 59; Properties = 0; Total = 59 }
    [pscustomobject]@{ Rank = 5; Name = 'GenVarsConfigurationAndTerrainLayers'; Fields = 50; Properties = 0; Total = 50 }
    [pscustomobject]@{ Rank = 6; Name = 'WorldSecretSeedRegistryState'; Fields = 43; Properties = 3; Total = 46 }
    [pscustomobject]@{ Rank = 7; Name = 'PlayerEquipmentAndAccessoryEffects'; Fields = 42; Properties = 0; Total = 42 }
    [pscustomobject]@{ Rank = 8; Name = 'WorldGenerationModifiersAndActions'; Fields = 42; Properties = 0; Total = 42 }
    [pscustomobject]@{ Rank = 9; Name = 'WorldHousingAndSpawnRules'; Fields = 39; Properties = 0; Total = 39 }
    [pscustomobject]@{ Rank = 10; Name = 'NpcStatusEffectAndRegenState'; Fields = 38; Properties = 0; Total = 38 }
    [pscustomobject]@{ Rank = 11; Name = 'PlayerAppearanceProjectionSlots'; Fields = 37; Properties = 0; Total = 37 }
    [pscustomobject]@{ Rank = 12; Name = 'PlayerStatusAndDebuffState'; Fields = 37; Properties = 0; Total = 37 }
    [pscustomobject]@{ Rank = 13; Name = 'WorldGenerationConfigurationAndOptions'; Fields = 7; Properties = 30; Total = 37 }
    [pscustomobject]@{ Rank = 14; Name = 'WorldGenerationTileActions'; Fields = 37; Properties = 0; Total = 37 }
    [pscustomobject]@{ Rank = 15; Name = 'WorldTerrainProfilesAndOreTiers'; Fields = 36; Properties = 1; Total = 37 }
    [pscustomobject]@{ Rank = 16; Name = 'GenVarsBiomeStructures'; Fields = 36; Properties = 0; Total = 36 }
    [pscustomobject]@{ Rank = 17; Name = 'NpcBossAndWorldProgressionFlags'; Fields = 35; Properties = 0; Total = 35 }
    [pscustomobject]@{ Rank = 18; Name = 'WorldGenBiomeMetricsAndCounts'; Fields = 35; Properties = 0; Total = 35 }
    [pscustomobject]@{ Rank = 19; Name = 'PlayerJumpVariantState'; Fields = 34; Properties = 0; Total = 34 }
    [pscustomobject]@{ Rank = 20; Name = 'PlayerBiomeAndZoneProperties'; Fields = 0; Properties = 31; Total = 31 }
)

$expectedActiveTop20Leaderboard = @(
    [pscustomobject]@{ Rank = 1; Name = 'PlayerNamedPetFlagState'; Fields = 55; Properties = 0; Total = 55 }
    [pscustomobject]@{ Rank = 2; Name = 'WorldSecretSeedDefinitions'; Fields = 41; Properties = 0; Total = 41 }
    [pscustomobject]@{ Rank = 3; Name = 'MountRuntimeProjectionProperties'; Fields = 2; Properties = 27; Total = 29 }
    [pscustomobject]@{ Rank = 4; Name = 'NpcSpawnEnvironmentEligibilityInputs'; Fields = 27; Properties = 0; Total = 27 }
    [pscustomobject]@{ Rank = 5; Name = 'WorldGenerationTileMutationActions'; Fields = 27; Properties = 0; Total = 27 }
    [pscustomobject]@{ Rank = 6; Name = 'PlayerMinionSummonFlags'; Fields = 25; Properties = 0; Total = 25 }
    [pscustomobject]@{ Rank = 7; Name = 'WorldGenerationControllerState'; Fields = 15; Properties = 10; Total = 25 }
    [pscustomobject]@{ Rank = 8; Name = 'PlayerCombatProcAndImmunityState'; Fields = 24; Properties = 0; Total = 24 }
    [pscustomobject]@{ Rank = 9; Name = 'PlayerSpawnMovementAndTileTargeting'; Fields = 24; Properties = 0; Total = 24 }
    [pscustomobject]@{ Rank = 10; Name = 'RevengeMarkerState'; Fields = 22; Properties = 2; Total = 24 }
    [pscustomobject]@{ Rank = 11; Name = 'WorldLifecycleAndTransformState'; Fields = 24; Properties = 0; Total = 24 }
    [pscustomobject]@{ Rank = 12; Name = 'MountAnimationFrameCatalog'; Fields = 23; Properties = 0; Total = 23 }
    [pscustomobject]@{ Rank = 13; Name = 'NpcDamageAttributionAndCredits'; Fields = 14; Properties = 9; Total = 23 }
    [pscustomobject]@{ Rank = 14; Name = 'PlayerCombatModifiersAndRanges'; Fields = 23; Properties = 0; Total = 23 }
    [pscustomobject]@{ Rank = 15; Name = 'GenVarsCavesOresAndBiomes'; Fields = 22; Properties = 0; Total = 22 }
    [pscustomobject]@{ Rank = 16; Name = 'LiquidFlowAndBufferState'; Fields = 22; Properties = 0; Total = 22 }
    [pscustomobject]@{ Rank = 17; Name = 'NpcSpawnZoneAndEventEligibilityInputs'; Fields = 22; Properties = 0; Total = 22 }
    [pscustomobject]@{ Rank = 18; Name = 'PlayerAccessoryAndCombatStatus'; Fields = 22; Properties = 0; Total = 22 }
    [pscustomobject]@{ Rank = 19; Name = 'PlayerInformationAccessoryState'; Fields = 22; Properties = 0; Total = 22 }
    [pscustomobject]@{ Rank = 20; Name = 'ProjectileSpecializedQueriesAndCaches'; Fields = 22; Properties = 0; Total = 22 }
)

$expectedSecondSplitSeeds = [ordered]@{
    PlayerPetAndCompanionState = @('PlayerLegacyPetState', 'PlayerNamedPetFlagState', 'PlayerCompanionState')
    PlayerBuffAndStatusEffects = @('PlayerElementalAndShimmerStatus', 'PlayerSurvivalAndControlStatus', 'PlayerAccessoryAndCombatStatus')
    MountDefinitionCatalog = @('MountGeometryAndFrameCatalog', 'MountMovementAndAbilityCatalog', 'MountVehicleAndPresentationCatalog')
    NpcSpawnEligibilityInputs = @('NpcSpawnContextAndCapacityInputs', 'NpcSpawnEnvironmentEligibilityInputs', 'NpcSpawnZoneAndEventEligibilityInputs')
    GenVarsConfigurationAndTerrainLayers = @('GenVarsConfigurationAndOreState', 'GenVarsWorldLayerAndSurfaceState', 'GenVarsBeachAndOceanBoundaryState')
    WorldSecretSeedRegistryState = @('WorldSecretSeedDefinitions', 'WorldSecretSeedRuntimeRegistry', 'WorldSecretSeedDerivedOptions')
    PlayerEquipmentAndAccessoryEffects = @('PlayerStringAndAccessoryEffectState', 'PlayerBeetleArmorState', 'PlayerSolarAndNebulaArmorState', 'PlayerMagnetAndUtilityAccessoryState')
    WorldGenerationModifiersAndActions = @('WorldGenerationShapeModifierState', 'WorldGenerationTileWallConditionState')
    WorldHousingAndSpawnRules = @('WorldHousingCountersAndScoringState', 'WorldHousingRoomSearchState', 'WorldHousingRuleAndDiagnosticState')
    NpcStatusEffectAndRegenState = @('NpcStatusEffectFlags', 'NpcRegenerationAndProtectionState')
    PlayerAppearanceProjectionSlots = @('PlayerAppearanceEquipmentProjection', 'PlayerAppearanceCompanionAndEffectProjection')
    PlayerStatusAndDebuffState = @('PlayerManaAndAfkStatus', 'PlayerDebuffAndRecoveryStatus', 'PlayerDetectionAndCombatStatus')
    WorldGenerationConfigurationAndOptions = @('WorldGenerationOptionBaseState', 'WorldGenerationOptionRegistry', 'WorldSeedOptionCatalog')
    WorldGenerationTileActions = @('WorldGenerationTileMutationActions', 'WorldGenerationTileScanAndControlActions', 'WorldGenerationTileFramingAndDebugActions')
    WorldTerrainProfilesAndOreTiers = @('WorldLandmassAndTreeProfiles', 'WorldSavedOreTierState', 'WorldTileMergeCullState')
    GenVarsBiomeStructures = @('WorldGenBeachAndOceanBiomeState', 'WorldGenUndergroundDesertStructureState', 'WorldGenJungleStructureState')
    NpcBossAndWorldProgressionFlags = @('NpcProgressionBookAndActiveRegistryState', 'NpcBossDefeatProgressionState')
    WorldGenBiomeMetricsAndCounts = @('WorldGenBiomeBackgroundAndDistanceMetrics', 'WorldGenTileCountMetrics')
    PlayerJumpVariantState = @('PlayerJumpAvailabilityState', 'PlayerJumpExecutionState', 'PlayerJumpMobilityModifiers')
    PlayerBiomeAndZoneProperties = @('PlayerIdentityAndDerivedProperties', 'PlayerBiomeZoneProperties', 'PlayerVerticalAndWeatherZoneProperties', 'PlayerEventAndShoppingZoneProperties')
}

$currentTopTwentySplitSeeds = @(
    'MountGeometryAndFrameCatalog', 'NpcBossDefeatProgressionState',
    'MountStaticAndDrillConstants', 'NpcStatusEffectFlags',
    'PlayerCompanionAndRestState', 'PlayerMinionCapacityAndSummonState',
    'PlayerIdentityAndLifecycleState', 'GenVarsWorldLayerAndSurfaceState',
    'WorldGenerationDimensionsAndExecution', 'NpcTownRescueAndSpawnUnlocks',
    'PlayerAppearanceEquipmentSelection', 'PlayerAppearanceEquipmentProjection',
    'NpcNetworkAndSpawnState', 'PlayerFrameImmunityAndInteractionState',
    'PlayerSurvivalAndControlStatus'
)

$rankingLines = @(Get-Content -LiteralPath $reportFullPath)
if ($rankingLines -notcontains '### 3.3 当前活动细分子系统字段属性数量排行榜（全部 221）') {
    throw 'Fine report is missing the complete active 221-subsystem leaderboard section.'
}
$activeLeaderboardStart = [array]::IndexOf($rankingLines, '### 3.3 当前活动细分子系统字段属性数量排行榜（全部 221）')
$activeLeaderboardEnd = [array]::IndexOf($rankingLines, '### 3.4 二次细分目标与新子系统')
if ($activeLeaderboardStart -lt 0 -or $activeLeaderboardEnd -le $activeLeaderboardStart) {
    throw 'Fine report has invalid active leaderboard section boundaries.'
}

$activeLeaderboardRows = @($rankingLines[($activeLeaderboardStart + 1)..($activeLeaderboardEnd - 1)] | Where-Object {
        $_ -match '^\|\s*\d+\s*\|\s*\x60[^\x60]+\x60\s*\|'
    })
Assert-Equal $activeLeaderboardRows.Count 221 'Complete active leaderboard row count mismatch'

$activeStatsByFine = @{}
foreach ($row in $reportRows) {
    if (-not $activeStatsByFine.ContainsKey($row.Fine)) {
        $activeStatsByFine[$row.Fine] = @{ field = 0; property = 0 }
    }
    $activeStatsByFine[$row.Fine][$row.Kind]++
}

$expectedActiveLeaderboard = @($activeStatsByFine.Keys | ForEach-Object {
        [pscustomobject]@{
            Name       = $_
            Fields     = $activeStatsByFine[$_].field
            Properties = $activeStatsByFine[$_].property
            Total      = $activeStatsByFine[$_].field + $activeStatsByFine[$_].property
        }
    } | Sort-Object -Property @{Expression = 'Total'; Descending = $true}, @{Expression = 'Name'; Descending = $false })

for ($rank = 1; $rank -le $expectedActiveLeaderboard.Count; $rank++) {
    $entry = $expectedActiveLeaderboard[$rank - 1]
    $expectedLine = "| $rank | $([char]0x60)$($entry.Name)$([char]0x60) | $($entry.Fields) | $($entry.Properties) | $($entry.Total) |"
    if ($activeLeaderboardRows -notcontains $expectedLine) {
        throw "Fine report has incorrect active leaderboard entry ${rank}: $($entry.Name)."
    }
}

foreach ($entry in $expectedActiveTop20Leaderboard) {
    $expectedLine = "| $($entry.Rank) | $([char]0x60)$($entry.Name)$([char]0x60) | $($entry.Fields) | $($entry.Properties) | $($entry.Total) |"
    if ($rankingLines -notcontains $expectedLine) {
        throw "Fine report is missing active leaderboard entry $($entry.Rank): $($entry.Name)."
    }
}
if ($rankingLines -notcontains '### 3.5 当前排行榜前 20 的再次细分审查') {
    throw 'Fine report is missing the current top-20 refinement audit section.'
}
if ($rankingLines -notcontains '### 3.6 二次细分前基线来源组排行榜（前 20，仅追溯）') {
    throw 'Fine report is missing the retired-source leaderboard trace section.'
}
foreach ($entry in $expectedTop20Leaderboard) {
    $expectedLine = "| $($entry.Rank) | $([char]0x60)$($entry.Name)$([char]0x60) | $($entry.Fields) | $($entry.Properties) | $($entry.Total) |"
    if ($rankingLines -notcontains $expectedLine) {
        throw "Fine report is missing retired-source trace entry $($entry.Rank): $($entry.Name)."
    }
}

$expectedSecondFineParents = @{
    PlayerLegacyPetState = 'PlayerGameplay'
    PlayerNamedPetFlagState = 'PlayerGameplay'
    PlayerCompanionState = 'PlayerGameplay'
    PlayerElementalAndShimmerStatus = 'PlayerGameplay'
    PlayerSurvivalAndControlStatus = 'PlayerGameplay'
    PlayerAccessoryAndCombatStatus = 'PlayerGameplay'
    MountGeometryAndFrameCatalog = 'MountAndVehicleSimulation'
    MountMovementAndAbilityCatalog = 'MountAndVehicleSimulation'
    MountVehicleAndPresentationCatalog = 'MountAndVehicleSimulation'
    NpcSpawnContextAndCapacityInputs = 'NpcAndTownSimulation'
    NpcSpawnEnvironmentEligibilityInputs = 'NpcAndTownSimulation'
    NpcSpawnZoneAndEventEligibilityInputs = 'NpcAndTownSimulation'
    GenVarsConfigurationAndOreState = 'WorldGenerationAndEcology'
    GenVarsWorldLayerAndSurfaceState = 'WorldGenerationAndEcology'
    GenVarsBeachAndOceanBoundaryState = 'WorldGenerationAndEcology'
    WorldSecretSeedDefinitions = 'WorldGenerationAndEcology'
    WorldSecretSeedRuntimeRegistry = 'WorldGenerationAndEcology'
    WorldSecretSeedDerivedOptions = 'WorldGenerationAndEcology'
    PlayerStringAndAccessoryEffectState = 'PlayerGameplay'
    PlayerBeetleArmorState = 'PlayerGameplay'
    PlayerSolarAndNebulaArmorState = 'PlayerGameplay'
    PlayerMagnetAndUtilityAccessoryState = 'PlayerGameplay'
    WorldGenerationShapeModifierState = 'WorldGenerationAndEcology'
    WorldGenerationTileWallConditionState = 'WorldGenerationAndEcology'
    WorldHousingCountersAndScoringState = 'WorldGenerationAndEcology'
    WorldHousingRoomSearchState = 'WorldGenerationAndEcology'
    WorldHousingRuleAndDiagnosticState = 'WorldGenerationAndEcology'
    NpcStatusEffectFlags = 'NpcAndTownSimulation'
    NpcRegenerationAndProtectionState = 'NpcAndTownSimulation'
    PlayerAppearanceEquipmentProjection = 'PlayerGameplay'
    PlayerAppearanceCompanionAndEffectProjection = 'PlayerGameplay'
    PlayerManaAndAfkStatus = 'PlayerGameplay'
    PlayerDebuffAndRecoveryStatus = 'PlayerGameplay'
    PlayerDetectionAndCombatStatus = 'PlayerGameplay'
    WorldGenerationOptionBaseState = 'WorldGenerationAndEcology'
    WorldGenerationOptionRegistry = 'WorldGenerationAndEcology'
    WorldSeedOptionCatalog = 'WorldGenerationAndEcology'
    WorldGenerationTileMutationActions = 'WorldGenerationAndEcology'
    WorldGenerationTileScanAndControlActions = 'WorldGenerationAndEcology'
    WorldGenerationTileFramingAndDebugActions = 'WorldGenerationAndEcology'
    WorldLandmassAndTreeProfiles = 'WorldGenerationAndEcology'
    WorldSavedOreTierState = 'WorldGenerationAndEcology'
    WorldTileMergeCullState = 'WorldGenerationAndEcology'
    WorldGenBeachAndOceanBiomeState = 'WorldGenerationAndEcology'
    WorldGenUndergroundDesertStructureState = 'WorldGenerationAndEcology'
    WorldGenJungleStructureState = 'WorldGenerationAndEcology'
    NpcProgressionBookAndActiveRegistryState = 'NpcAndTownSimulation'
    NpcBossDefeatProgressionState = 'NpcAndTownSimulation'
    WorldGenBiomeBackgroundAndDistanceMetrics = 'WorldGenerationAndEcology'
    WorldGenTileCountMetrics = 'WorldGenerationAndEcology'
    PlayerJumpAvailabilityState = 'PlayerGameplay'
    PlayerJumpExecutionState = 'PlayerGameplay'
    PlayerJumpMobilityModifiers = 'PlayerGameplay'
    PlayerIdentityAndDerivedProperties = 'PlayerGameplay'
    PlayerBiomeZoneProperties = 'PlayerGameplay'
    PlayerVerticalAndWeatherZoneProperties = 'PlayerGameplay'
    PlayerEventAndShoppingZoneProperties = 'PlayerGameplay'
}

$expectedSplitSeeds = [ordered]@{
    PlayerInputAndActionIntent = @('PlayerControlAndReleaseInput', 'PlayerItemUseAndChannelIntent', 'PlayerShadowAndArmPresentation', 'PlayerQuestAndEventCounters')
    PlayerAppearanceAndInformationAccessories = @('PlayerAppearanceCustomizationState', 'PlayerInformationAccessoryState', 'PlayerDpsTelemetryState', 'PlayerLuckAndCommerceEffects')
    PlayerFishingAndMinionCapacity = @('PlayerFishingCapabilityState', 'PlayerMinionCapacityAndSummonState')
    PlayerDashRopeAndCarpetTraversal = @('PlayerTeleportTransitionState', 'PlayerDashAndGroundTraversalState', 'PlayerRopeAndPulleyState', 'PlayerSlideAndCarpetTraversalState')
    PlayerEnvironmentAndMobilityEffects = @('PlayerEnvironmentMobilityState', 'PlayerEnvironmentDetectionAndSpawnState', 'PlayerArmorAndCombatEffects')
    PlayerWingsZonesAndSocialState = @('PlayerWingsAndFlightState', 'PlayerZoneAndEnvironmentState', 'PlayerSocialAndDefenseState')
    PlayerPoseNetworkAndRespawnState = @('PlayerPoseAndAnimationState', 'PlayerNetworkCameraState', 'PlayerDeathRespawnAndSaveState')
    PlayerVitalAndCombatStats = @('PlayerVitalAndRegenState', 'PlayerCombatModifierAndImmunityState', 'PlayerAmmoAndAccessoryEffects')
    NpcIdentityTargetAndMovementState = @('NpcIdentityInteractionAndPresentationState', 'NpcTargetAndMovementHistoryState', 'NpcBossAndInvasionGlobalState', 'NpcSpawnAndCritterState')
    NpcBuffAndStatusState = @('NpcBuffSlotAndImmunityState', 'NpcStatusEffectAndRegenState')
    ProjectileIdentityAiAndLifetime = @('ProjectileIdentityAndClassificationState', 'ProjectileAiState', 'ProjectileLifetimeAndRuntimeState')
    ProjectileMovementAndReplication = @('ProjectileMovementAndCollisionState', 'ProjectileNetworkReplicationState', 'ProjectileMinionAndPresentationState')
    ProjectileBehaviorAndTargeting = @('ProjectileDamageAndElementState', 'ProjectileAnimationAndDirectionState', 'ProjectileCollisionAndTargetingState')
}

$expectedFineParents = @{
    PlayerControlAndReleaseInput = 'PlayerGameplay'
    PlayerItemUseAndChannelIntent = 'PlayerGameplay'
    PlayerShadowAndArmPresentation = 'PlayerGameplay'
    PlayerQuestAndEventCounters = 'PlayerGameplay'
    PlayerAppearanceCustomizationState = 'PlayerGameplay'
    PlayerInformationAccessoryState = 'PlayerGameplay'
    PlayerDpsTelemetryState = 'PlayerGameplay'
    PlayerLuckAndCommerceEffects = 'PlayerGameplay'
    PlayerFishingCapabilityState = 'PlayerGameplay'
    PlayerMinionCapacityAndSummonState = 'PlayerGameplay'
    PlayerTeleportTransitionState = 'PlayerGameplay'
    PlayerDashAndGroundTraversalState = 'PlayerGameplay'
    PlayerRopeAndPulleyState = 'PlayerGameplay'
    PlayerSlideAndCarpetTraversalState = 'PlayerGameplay'
    PlayerEnvironmentMobilityState = 'PlayerGameplay'
    PlayerEnvironmentDetectionAndSpawnState = 'PlayerGameplay'
    PlayerArmorAndCombatEffects = 'PlayerGameplay'
    PlayerWingsAndFlightState = 'PlayerGameplay'
    PlayerZoneAndEnvironmentState = 'PlayerGameplay'
    PlayerSocialAndDefenseState = 'PlayerGameplay'
    PlayerPoseAndAnimationState = 'PlayerGameplay'
    PlayerNetworkCameraState = 'PlayerGameplay'
    PlayerDeathRespawnAndSaveState = 'PlayerGameplay'
    PlayerVitalAndRegenState = 'PlayerGameplay'
    PlayerCombatModifierAndImmunityState = 'PlayerGameplay'
    PlayerAmmoAndAccessoryEffects = 'PlayerGameplay'
    NpcIdentityInteractionAndPresentationState = 'NpcAndTownSimulation'
    NpcTargetAndMovementHistoryState = 'NpcAndTownSimulation'
    NpcBossAndInvasionGlobalState = 'NpcAndTownSimulation'
    NpcSpawnAndCritterState = 'NpcAndTownSimulation'
    NpcBuffSlotAndImmunityState = 'NpcAndTownSimulation'
    NpcStatusEffectAndRegenState = 'NpcAndTownSimulation'
    ProjectileIdentityAndClassificationState = 'ProjectileSimulation'
    ProjectileAiState = 'ProjectileSimulation'
    ProjectileLifetimeAndRuntimeState = 'ProjectileSimulation'
    ProjectileMovementAndCollisionState = 'ProjectileSimulation'
    ProjectileNetworkReplicationState = 'ProjectileSimulation'
    ProjectileMinionAndPresentationState = 'ProjectileSimulation'
    ProjectileDamageAndElementState = 'ProjectileSimulation'
    ProjectileAnimationAndDirectionState = 'ProjectileSimulation'
    ProjectileCollisionAndTargetingState = 'ProjectileSimulation'
}

foreach ($seed in $expectedSplitSeeds.Keys) {
    $members = @($reportRows | Where-Object Fine -in $expectedSplitSeeds[$seed])
    if ($members.Count -eq 0) {
        throw "Approved split seed $seed has no refined members."
    }

    foreach ($fineName in $expectedSplitSeeds[$seed]) {
        $fineMembers = @($members | Where-Object Fine -eq $fineName)
        if ($expectedSecondSplitSeeds.Contains($fineName) -or $currentTopTwentySplitSeeds -contains $fineName) {
            Assert-Equal $fineMembers.Count 0 "Retired refined sibling $fineName still owns members in the first-level assertion"
            continue
        }

        if ($fineMembers.Count -eq 0) {
            throw "Approved sibling $fineName has no members."
        }

        if (@($fineMembers | Where-Object Parent -ne $expectedFineParents[$fineName]).Count -gt 0) {
            throw "Approved sibling $fineName belongs to the wrong parent."
        }
    }
}

$retiredBroadNames = @($expectedSplitSeeds.Keys) + @($expectedSecondSplitSeeds.Keys) + $currentTopTwentySplitSeeds
$retiredRows = @($reportRows | Where-Object Fine -in $retiredBroadNames)
Assert-Equal $retiredRows.Count 0 'Retired broad fine subsystem still owns members'

$expectedCrossAssignments = @(
    [pscustomobject]@{ Parent = 'PlayerGameplay'; Member = 'chest'; TargetFine = 'PlayerContainerAndWorldAnchorState'; WrongFine = 'PlayerAppearanceProjectionSlots' },
    [pscustomobject]@{ Parent = 'PlayerGameplay'; Member = 'currentShoppingSettings'; TargetFine = 'PlayerContainerAndWorldAnchorState'; WrongFine = 'PlayerAppearanceProjectionSlots' },
    [pscustomobject]@{ Parent = 'NpcAndTownSimulation'; Member = 'savedTaxCollector'; TargetFine = 'NpcTownRescueState'; WrongFine = 'NpcBuffAndStatusState' }
)

foreach ($assignment in $expectedCrossAssignments) {
    $matches = @($reportRows | Where-Object { $_.Parent -eq $assignment.Parent -and $_.Member -eq $assignment.Member })
    Assert-Equal $matches.Count 1 "Cross-seed member count mismatch for $($assignment.Member)"
    Assert-Equal $matches[0].Fine $assignment.TargetFine "Cross-seed target mismatch for $($assignment.Member)"
    if (@($reportRows | Where-Object { $_.Parent -eq $assignment.Parent -and $_.Member -eq $assignment.Member -and $_.Fine -eq $assignment.WrongFine }).Count -ne 0) {
        throw "Cross-seed member $($assignment.Member) remains in $($assignment.WrongFine)."
    }
}

$parentTotals = @{}
$fineTotals = @{}
foreach ($row in $inputRows) {
    if (-not $parentTotals.ContainsKey($row.Parent)) { $parentTotals[$row.Parent] = @{ field = 0; property = 0 } }
    $parentTotals[$row.Parent][$row.Kind]++
}
foreach ($row in $reportRows) {
    if (-not $fineTotals.ContainsKey($row.Fine)) { $fineTotals[$row.Fine] = @{ Parent = $row.Parent; field = 0; property = 0 } }
    if ($fineTotals[$row.Fine].Parent -ne $row.Parent) {
        throw "Fine subsystem $($row.Fine) has multiple parents."
    }
    $fineTotals[$row.Fine][$row.Kind]++
}

foreach ($fine in $fineTotals.Keys) {
    Assert-Equal ($fineTotals[$fine].field + $fineTotals[$fine].property) @($reportRows | Where-Object Fine -eq $fine).Count "Fine total mismatch for $fine"
}

$parentFineTotals = @{}
foreach ($fine in $fineTotals.Keys) {
    $parent = $fineTotals[$fine].Parent
    if (-not $parentFineTotals.ContainsKey($parent)) { $parentFineTotals[$parent] = @{ field = 0; property = 0 } }
    $parentFineTotals[$parent].field += $fineTotals[$fine].field
    $parentFineTotals[$parent].property += $fineTotals[$fine].property
}
foreach ($parent in $parentTotals.Keys) {
    if (-not $parentFineTotals.ContainsKey($parent)) {
        Assert-Equal 0 ($parentTotals[$parent].field + $parentTotals[$parent].property) "Missing fine totals for $parent"
        continue
    }
    Assert-Equal $parentFineTotals[$parent].field $parentTotals[$parent].field "Parent field total mismatch for $parent"
    Assert-Equal $parentFineTotals[$parent].property $parentTotals[$parent].property "Parent property total mismatch for $parent"
}

$inputHash = (Get-FileHash -LiteralPath $inputFullPath -Algorithm SHA256).Hash.ToLowerInvariant()
$reportText = Get-Content -Raw -LiteralPath $reportFullPath
if ($reportText -notmatch "\| 权威成员库存输入 \| $inputHash \|") {
    throw 'Fine report does not contain the current authoritative input SHA-256 trace.'
}

$requiredSourceFacts = @(
    'Terraria.WorldGen.SecretSeed',
    'Terraria.WorldGen.SecretSeed.Variations',
    'Terraria.WorldGen.Skyblock',
    'Terraria.WorldBuilding.WorldGenerator.Controller',
    'Terraria.WorldBuilding.WorldGenSnapshot',
    'Terraria.WorldBuilding.Modifiers.ShapeScale'
)
foreach ($type in $requiredSourceFacts) {
    if (@($reportRows | Where-Object Type -eq $type).Count -eq 0) {
        throw "Required source type evidence is absent from the report: $type"
    }
}

Write-Output 'PASS: authoritative input and fine report contain 2659 rows (2397 fields, 262 properties).'
Write-Output 'PASS: source sequence closure is exactly 1..2659 and every report row matches the input cells.'
Write-Output 'PASS: every member has exactly one parent and fine owner; parent/fine totals reconcile.'
Write-Output 'PASS: no ID-class rows are present and the authoritative SHA-256 trace matches.'
Write-Output "Fine subsystems: $($fineNames.Count)"
foreach ($seed in $expectedSplitSeeds.Keys | Sort-Object) {
    $summary = foreach ($fineName in $expectedSplitSeeds[$seed]) {
        "$fineName=$(@($reportRows | Where-Object Fine -eq $fineName).Count)"
    }
    Write-Output ("Split {0}: {1}" -f $seed, ($summary -join ', '))
}
