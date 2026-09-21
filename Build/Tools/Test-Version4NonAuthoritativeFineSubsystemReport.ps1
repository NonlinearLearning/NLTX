[CmdletBinding()]
param(
    [string]$InputPath = 'docs/migration/ledgers/Version4非权威模拟系统字段属性逐成员源码声明-去除ID类文件.md',
    [string]$ReportPath = 'docs/migration/ledgers/Version4非权威模拟系统字段属性逐成员源码声明-更细子系统拆分-去除ID类文件.md',
    [string]$CombinedSourcePath = 'docs/migration/ledgers/Version4字段属性逐成员源码声明-去除ID类文件.md'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Split-MarkdownRow {
    param([Parameter(Mandatory)][string]$Line)

    $content = $Line.Trim()
    if (-not ($content.StartsWith('|') -and $content.EndsWith('|'))) {
        return @()
    }

    $content = $content.Substring(1, $content.Length - 2)
    return @($content -split '(?<!\\)\|' | ForEach-Object { $_.Trim() })
}

function Read-InputMembers {
    param([Parameter(Mandatory)][string]$Path)

    $parent = ''
    $members = [System.Collections.Generic.List[object]]::new()
    foreach ($line in Get-Content -LiteralPath $Path) {
        if ($line -match '^### \d+\.\d+ 子系统：`([^`]+)`') {
            $parent = $Matches[1]
            continue
        }
        if ($line -match '^\|\s*(\d+)\s*\|\s*(field|property)\s*\|') {
            $cells = Split-MarkdownRow -Line $line
            if ($cells.Count -ne 11) {
                throw "Input row does not have 11 cells: $line"
            }
            $members.Add([pscustomobject]@{
                Parent = $parent
                Seq = [int]$cells[0]
                Kind = $cells[1]
                Cells = $cells
            })
        }
    }
    return @($members)
}

function Read-FineMembers {
    param([Parameter(Mandatory)][string]$Path)

$parent = ''
$fine = ''
$baseline = ''
$previousPeer = ''
$members = [System.Collections.Generic.List[object]]::new()
    foreach ($line in Get-Content -LiteralPath $Path) {
        if ($line -match '^### \d+\.\d+ 父级子系统：`([^`]+)`') {
            $parent = $Matches[1]
            $fine = ''
            $baseline = ''
            $previousPeer = ''
            continue
        }
        if ($line -match '^#### \d+\.\d+\.\d+ 细分子系统：`([^`]+)`') {
            $fine = $Matches[1]
            $baseline = ''
            $previousPeer = ''
            continue
        }
        if ($line -match '^- 上一级基线细分子系统：`([^`]+)`$') {
            $baseline = $Matches[1]
            continue
        }
        if ($line -match '^- 上一级 peer 细分子系统：`([^`]+)`$') {
            $previousPeer = $Matches[1]
            continue
        }
        if ($line -match '^\|\s*(\d+)\s*\|\s*(field|property)\s*\|') {
            if ([string]::IsNullOrWhiteSpace($parent) -or [string]::IsNullOrWhiteSpace($fine) -or
                [string]::IsNullOrWhiteSpace($baseline)) {
                throw "Member row is outside a fine subsystem: $line"
            }
            $cells = Split-MarkdownRow -Line $line
            if ($cells.Count -ne 11) {
                throw "Fine report row does not have 11 cells: $line"
            }
            $members.Add([pscustomobject]@{
                Parent = $parent
                Fine = $fine
                Baseline = $baseline
                PreviousPeer = $previousPeer
                Seq = [int]$cells[0]
                Kind = $cells[1]
                Cells = $cells
            })
        }
    }
    return @($members)
}

function Remove-MarkdownCodeSpan {
    param([Parameter(Mandatory)][string]$Value)

    return $Value.Trim().Trim([char]0x60)
}

function Convert-SummaryCount {
    param(
        [Parameter(Mandatory)][string]$Value,
        [Parameter(Mandatory)][string]$Context
    )

    $trimmed = $Value.Trim()
    if ($trimmed -notmatch '^\d+$') {
        throw "Summary count is not a non-negative integer for ${Context}: '$Value'."
    }
    return [int]$trimmed
}

function Read-SummaryTables {
    param([Parameter(Mandatory)][string]$Path)

    $section = ''
    $parentRows = [System.Collections.Generic.List[object]]::new()
    $fineRows = [System.Collections.Generic.List[object]]::new()
    $top50Rows = [System.Collections.Generic.List[object]]::new()
    $fullRankingRows = [System.Collections.Generic.List[object]]::new()
    $topTotals = @{}
    $reportedFineCount = $null

    foreach ($line in Get-Content -LiteralPath $Path) {
        if ($line -match '^## 4\.') {
            $section = ''
            continue
        }
        if ($line -match '^### 3\.1 父级子系统统计') {
            $section = 'parent'
            continue
        }
        if ($line -match '^### 3\.2 细分子系统统计') {
            $section = 'fine'
            continue
        }
        if ($line -match '^### 3\.3 最终细分子系统字段/属性合计排行榜前 50') {
            $section = 'top50'
            continue
        }
        if ($line -match '^### 3\.4 最终细分子系统字段/属性合计完整排行榜（\d+）') {
            $section = 'fullRanking'
            continue
        }
        if ($line -match '^### 3\.5 (二次拆分前基线|原始细分基线)前 50 与最终 peer 汇总') {
            $section = ''
            continue
        }
        if ($line -match '^### 3\.6 第三层拆分前 peer 与最终 peer 汇总') {
            $section = ''
            continue
        }
        if ($line -match '^### 3\.7 第四层拆分前 peer 与最终 peer 汇总') {
            $section = ''
            continue
        }
        if ($line -match '^### 3\.8 第五层拆分前 peer 与最终 peer 汇总') {
            $section = ''
            continue
        }
        if ($line -match '^### 3\.9 第六层拆分前 peer 与最终 peer 汇总') {
            $section = ''
            continue
        }

        if ($line -match '^\| 本文最终细分子系统（有成员记录） \| (\d+) \|$') {
            $reportedFineCount = [int]$Matches[1]
            continue
        }
        if ($section -eq '' -and $line -match '^\| (field|property|合计) \| (\d+) \|$') {
            $topTotals[$Matches[1]] = [int]$Matches[2]
            continue
        }
        if ($section -eq '' -or -not $line.StartsWith('|')) {
            continue
        }

        $cells = Split-MarkdownRow -Line $line
        if ($cells.Count -eq 0 -or $cells[0] -like '---*' -or $cells[0] -eq '父级子系统') {
            continue
        }

        if ($section -eq 'parent') {
            if ($cells.Count -ne 5) {
                throw "Parent summary row does not have 5 cells: $line"
            }
            $parentRows.Add([pscustomobject]@{
                Parent = Remove-MarkdownCodeSpan -Value $cells[0]
                FineCount = Convert-SummaryCount -Value $cells[1] -Context "parent $($cells[0]) fine count"
                Field = Convert-SummaryCount -Value $cells[2] -Context "parent $($cells[0]) field count"
                Property = Convert-SummaryCount -Value $cells[3] -Context "parent $($cells[0]) property count"
                Total = Convert-SummaryCount -Value $cells[4] -Context "parent $($cells[0]) total"
            })
            continue
        }

        if ($section -eq 'fine') {
            if ($cells.Count -ne 6) {
                throw "Fine summary row does not have 6 cells: $line"
            }
            $fineRows.Add([pscustomobject]@{
                Parent = Remove-MarkdownCodeSpan -Value $cells[0]
                Fine = Remove-MarkdownCodeSpan -Value $cells[1]
                Role = $cells[2]
                Field = Convert-SummaryCount -Value $cells[3] -Context "fine $($cells[1]) field count"
                Property = Convert-SummaryCount -Value $cells[4] -Context "fine $($cells[1]) property count"
                Total = Convert-SummaryCount -Value $cells[5] -Context "fine $($cells[1]) total"
            })
            continue
        }

        if ($section -eq 'top50') {
            if ($cells.Count -ne 10) {
                throw "Top-50 summary row does not have 10 cells: $line"
            }
            if ($cells[0] -eq '排名') {
                continue
            }
            $top50Rows.Add([pscustomobject]@{
                Rank = Convert-SummaryCount -Value $cells[0] -Context 'top-50 rank'
                Parent = Remove-MarkdownCodeSpan -Value $cells[1]
                Fine = Remove-MarkdownCodeSpan -Value $cells[2]
                Baseline = Remove-MarkdownCodeSpan -Value $cells[3]
                Field = Convert-SummaryCount -Value $cells[4] -Context "top-50 $($cells[2]) field count"
                Property = Convert-SummaryCount -Value $cells[5] -Context "top-50 $($cells[2]) property count"
                Total = Convert-SummaryCount -Value $cells[6] -Context "top-50 $($cells[2]) total"
                Decision = $cells[7]
                Boundary = $cells[8]
                Evidence = $cells[9]
            })
            continue
        }

        if ($section -eq 'fullRanking') {
            if ($cells.Count -ne 7) {
                throw "Full ranking row does not have 7 cells: $line"
            }
            if ($cells[0] -eq '排名') {
                continue
            }
            $fullRankingRows.Add([pscustomobject]@{
                Rank = Convert-SummaryCount -Value $cells[0] -Context 'full-ranking rank'
                Parent = Remove-MarkdownCodeSpan -Value $cells[1]
                Fine = Remove-MarkdownCodeSpan -Value $cells[2]
                Baseline = Remove-MarkdownCodeSpan -Value $cells[3]
                Field = Convert-SummaryCount -Value $cells[4] -Context "full-ranking $($cells[2]) field count"
                Property = Convert-SummaryCount -Value $cells[5] -Context "full-ranking $($cells[2]) property count"
                Total = Convert-SummaryCount -Value $cells[6] -Context "full-ranking $($cells[2]) total"
            })
        }
    }

    if ($null -eq $reportedFineCount) {
        throw 'Fine report does not contain the reported fine-subsystem count.'
    }
    foreach ($name in @('field', 'property', '合计')) {
        if (-not $topTotals.ContainsKey($name)) {
            throw "Fine report does not contain top-level total '$name'."
        }
    }

    return [pscustomobject]@{
        ReportedFineCount = $reportedFineCount
        TopTotals = $topTotals
        ParentRows = @($parentRows)
        FineRows = @($fineRows)
        Top50Rows = @($top50Rows)
        FullRankingRows = @($fullRankingRows)
    }
}

function Assert-Equal {
    param(
        [Parameter(Mandatory)]$Actual,
        [Parameter(Mandatory)]$Expected,
        [Parameter(Mandatory)][string]$Message
    )
    if ($Actual -ne $Expected) {
        throw "$Message; expected '$Expected', actual '$Actual'."
    }
}

function Get-CountSum {
    param(
        [object[]]$Items,
        [Parameter(Mandatory)][string]$Property
    )

    if ($Items.Count -eq 0) {
        return 0
    }
    return [int](@($Items | Measure-Object -Property $Property -Sum)[0].Sum)
}

$inputFullPath = [System.IO.Path]::GetFullPath((Join-Path (Get-Location) $InputPath))
$reportFullPath = [System.IO.Path]::GetFullPath((Join-Path (Get-Location) $ReportPath))
$combinedSourceFullPath = [System.IO.Path]::GetFullPath((Join-Path (Get-Location) $CombinedSourcePath))
if (-not (Test-Path -LiteralPath $inputFullPath)) { throw "Input document not found: $inputFullPath" }
if (-not (Test-Path -LiteralPath $reportFullPath)) { throw "Fine report not found: $reportFullPath" }
if (-not (Test-Path -LiteralPath $combinedSourceFullPath)) { throw "Combined source document not found: $combinedSourceFullPath" }

$inputMembers = Read-InputMembers -Path $inputFullPath
$reportMembers = Read-FineMembers -Path $reportFullPath

Assert-Equal -Actual $inputMembers.Count -Expected 4542 -Message 'Input member count'
Assert-Equal -Actual $reportMembers.Count -Expected 4542 -Message 'Fine report member count'
Assert-Equal -Actual @($inputMembers | Where-Object Kind -eq 'field').Count -Expected 4017 -Message 'Input field count'
Assert-Equal -Actual @($inputMembers | Where-Object Kind -eq 'property').Count -Expected 525 -Message 'Input property count'
Assert-Equal -Actual @($reportMembers | Where-Object Kind -eq 'field').Count -Expected 4017 -Message 'Fine report field count'
Assert-Equal -Actual @($reportMembers | Where-Object Kind -eq 'property').Count -Expected 525 -Message 'Fine report property count'

$combinedSourceKeys = [System.Collections.Generic.HashSet[string]]::new()
$combinedSourceRows = 0
$combinedSourceFields = 0
$combinedSourceProperties = 0
foreach ($line in Get-Content -LiteralPath $combinedSourceFullPath) {
    if ($line -match '^\|\s*(\d+)\s*\|\s*(field|property)\s*\|') {
        $cells = Split-MarkdownRow -Line $line
        if ($cells.Count -ne 11) { throw "Combined source row does not have 11 cells: $line" }
        $combinedSourceRows++
        if ($cells[1] -eq 'field') { $combinedSourceFields++ } else { $combinedSourceProperties++ }
        [void]$combinedSourceKeys.Add(($cells[1..10] -join [char]31))
    }
}
Assert-Equal -Actual $combinedSourceRows -Expected 7201 -Message 'Combined source member count'
Assert-Equal -Actual $combinedSourceFields -Expected 6414 -Message 'Combined source field count'
Assert-Equal -Actual $combinedSourceProperties -Expected 787 -Message 'Combined source property count'
foreach ($member in $inputMembers) {
    $key = $member.Cells[1..10] -join [char]31
    if (-not $combinedSourceKeys.Contains($key)) {
        throw "Input member sequence $($member.Seq) is absent from the combined source declaration document."
    }
}

$inputBySeq = @{}
foreach ($member in $inputMembers) {
    if ($inputBySeq.ContainsKey($member.Seq)) { throw "Duplicate input sequence $($member.Seq)." }
    $inputBySeq[$member.Seq] = $member
}
$reportBySeq = @{}
foreach ($member in $reportMembers) {
    if ($reportBySeq.ContainsKey($member.Seq)) { throw "Duplicate fine report sequence $($member.Seq)." }
    $reportBySeq[$member.Seq] = $member
}

$expectedSequences = (1..4542) -join ','
Assert-Equal -Actual ((@($inputBySeq.Keys | Sort-Object) -join ',')) -Expected $expectedSequences -Message 'Input sequence closure'
Assert-Equal -Actual ((@($reportBySeq.Keys | Sort-Object) -join ',')) -Expected $expectedSequences -Message 'Fine report sequence closure'

foreach ($sequence in 1..4542) {
    $source = $inputBySeq[$sequence]
    $reported = $reportBySeq[$sequence]
    Assert-Equal -Actual $reported.Kind -Expected $source.Kind -Message "Kind for sequence $sequence"
    Assert-Equal -Actual $reported.Parent -Expected $source.Parent -Message "Parent for sequence $sequence"
    Assert-Equal -Actual ($reported.Cells -join [char]31) -Expected ($source.Cells -join [char]31) -Message "Declaration row for sequence $sequence"
}

$groupStats = @($reportMembers | Group-Object Parent, Fine | ForEach-Object {
    $items = $_.Group
    [pscustomobject]@{
        Parent = $items[0].Parent
        Fine = $items[0].Fine
        Baseline = $items[0].Baseline
        PreviousPeer = if ([string]::IsNullOrWhiteSpace($items[0].PreviousPeer)) { $items[0].Fine } else { $items[0].PreviousPeer }
        Field = @($items | Where-Object Kind -eq 'field').Count
        Property = @($items | Where-Object Kind -eq 'property').Count
        Total = $items.Count
        Files = @($items | ForEach-Object { $_.Cells[3] } | Select-Object -Unique).Count
    }
})
Assert-Equal -Actual $groupStats.Count -Expected 349 -Message 'Active fine-subsystem count'
$baselineStats = @($reportMembers | Group-Object Parent, Baseline | ForEach-Object {
    $items = $_.Group
    [pscustomobject]@{
        Parent = $items[0].Parent
        Fine = $items[0].Baseline
        Field = @($items | Where-Object Kind -eq 'field').Count
        Property = @($items | Where-Object Kind -eq 'property').Count
        Total = $items.Count
    }
})
Assert-Equal -Actual $baselineStats.Count -Expected 239 -Message 'Second-level baseline subsystem count'

$expectedThirdLevelPeerMap = [ordered]@{
    UiItemSortingDefinitions = @('UiItemSortingLayerCatalogState', 'UiItemSortingRegistryAndRankingState')
    MapTileEncodingAndStorageState = @('MapEncodingCatalogAndIoState', 'MapTileCellState')
    SharedContentUiTextAndOptionWidgets = @('SharedContentUiOptionButtonState', 'SharedContentUiTextAndHeaderState')
    SharedDungeonRoomCoreAndSettings = @('SharedDungeonRoomCoreState', 'SharedDungeonRoomSettingsState')
    SharedDungeonHallCoreAndSettings = @('SharedDungeonHallCoreState', 'SharedDungeonHallSettingsState')
}
$expectedThirdLevelStats = @{
    UiItemSortingLayerCatalogState = @{ Field = 54; Property = 0; Total = 54 }
    UiItemSortingRegistryAndRankingState = @{ Field = 8; Property = 1; Total = 9 }
    MapEncodingCatalogAndIoState = @{ Field = 36; Property = 0; Total = 36 }
    MapTileCellState = @{ Field = 3; Property = 3; Total = 6 }
    SharedContentUiOptionButtonState = @{ Field = 19; Property = 6; Total = 25 }
    SharedContentUiTextAndHeaderState = @{ Field = 9; Property = 8; Total = 17 }
    SharedDungeonRoomCoreState = @{ Field = 5; Property = 2; Total = 7 }
    SharedDungeonRoomSettingsState = @{ Field = 25; Property = 0; Total = 25 }
    SharedDungeonHallCoreState = @{ Field = 9; Property = 1; Total = 10 }
    SharedDungeonHallSettingsState = @{ Field = 19; Property = 0; Total = 19 }
}

$expectedFourthLevelPeerMap = [ordered]@{
    TileObjectDefinitionPlacementAndStyleState = @('TileObjectPlacementRuleState', 'TileObjectStyleAndDrawState')
    SharedTimeLoggerRenderMetricsState = @('SharedTimeLoggerPhaseMetricsState', 'SharedTimeLoggerDisplayFormattingState')
    SharedDropRuleSelectionAndChainState = @('SharedDropRuleSelectionAndConditionState', 'SharedDropRuleChainState')
    SharedBiomeDungeonAndTerrainState = @('SharedBiomeTerrainPassState', 'SharedDungeonControlAndTrapState')
    SharedFishingDropCatalogState = @('SharedFishingConditionCatalogState', 'SharedFishingDropResolutionState')
    MainCageBirdAndTerrestrialAnimationState = @('MainCageBirdAnimationState', 'MainCageTerrestrialCritterAnimationState')
    UiItemSortingLayerCatalogState = @('UiItemSortingCombatAndEquipmentCatalogState', 'UiItemSortingConsumableAndMiscCatalogState')
    SharedSceneScanAndZoneState = @('SharedSceneZoneDefinitionState', 'SharedSceneScanAccumulatorState')
    SharedCreativePowerDefinitionState = @('SharedCreativePerPlayerPowerState', 'SharedCreativeSharedPowerState')
    SharedDungeonStyleEntryDefinitions = @('SharedDungeonStyleMaterialAndGeometryState', 'SharedDungeonStyleFurnitureAndRoomState')
    SharedSceneAggregateAndDecorationState = @('SharedSceneTileAggregateState', 'SharedSceneDecorationAndAudioState')
    AudioLegacySoundInstanceState = @('SharedAudioLegacySoundCatalogInstanceState', 'SharedAudioLegacyTrackedInstanceState')
    SharedDungeonRoomShapeVariants = @('SharedDungeonRoomGeometryState', 'SharedDungeonRoomVariantCatalogState')
    AudioLegacySoundCatalogState = @('SharedAudioLegacySoundDefinitionCatalogState', 'SharedAudioLegacySoundServicesState')
    MapEncodingCatalogAndIoState = @('MapEncodingHeaderCatalogState', 'MapIoRuntimeState')
    MainTileBehaviorMetadata = @('MainTileBehaviorAndInteractionMetadata', 'MainTileLightingAndFrameMetadata')
    UiItemSlotEquipmentAndCreativeContexts = @('UiItemSlotEquipmentAndDisplayContexts', 'UiItemSlotCreativeCraftingAndUtilityContexts')
    SharedItemStaticEconomyAndTimingRules = @('SharedItemEconomyAndValueRules', 'SharedItemUseTimingAndStackRules')
    SharedPopupTextState = @('SharedPopupTextContentAndContextState', 'SharedPopupTextRenderLifecycleState')
    MainBackgroundLayerState = @('MainBackgroundLayerCatalogState', 'MainBackgroundParallaxAndStyleState')
    SharedShaderFamilyCatalogState = @('SharedShaderFamilyDataState', 'SharedShaderRegistryAndLookupState')
    SharedDungeonStyleConstantQueries = @('SharedDungeonStyleObjectConstants', 'SharedDungeonStyleBannerAndTrapConstants')
    SharedTilePlacementGeometryModules = @('SharedTilePlacementCoordinateAndDrawModules', 'SharedTilePlacementBaseAndStyleModules')
    SharedWorldItemPayloadState = @('SharedWorldItemIdentityAndEconomyPayloadState', 'SharedWorldItemUseAndPresentationPayloadState')
    NetworkRemoteClientState = @('NetworkRemoteClientConnectionAndStatusState', 'NetworkRemoteClientSectionAndRateLimitState')
    NetworkSessionCoordinatorState = @('NetworkSessionConfigurationState', 'NetworkSessionTransportAndThreadState')
    ItemEmergencyStackingState = @('SharedItemEmergencyStackingPolicyState', 'SharedItemEmergencyStackingTransferState')
    BestiaryInfoElementsAndProviders = @('SharedBestiaryInfoElementState', 'SharedBestiaryCollectionProviderState')
    EntityAuthoritativeState = @('EntityIdentityAndMotionState', 'EntityBoundsAndFluidState')
    SharedCreativePowerIconCatalogState = @('SharedCreativePowerIconLocationCatalogState', 'SharedCreativePowerIconLayoutState')
}
$expectedFifthLevelPeerMap = [ordered]@{
    SharedTimeLoggerPhaseMetricsState = @('SharedTimeLoggerWorldRenderPhaseMetricsState', 'SharedTimeLoggerEntityAndInterfacePhaseMetricsState')
    TileObjectStyleAndDrawState = @('TileObjectStyleCatalogState', 'TileObjectDrawGeometryState')
    SharedFishingConditionCatalogState = @('SharedFishingRarityConditionCatalogState', 'SharedFishingEnvironmentConditionCatalogState')
    SharedDropRuleSelectionAndConditionState = @('SharedDropRuleConditionBranchState', 'SharedDropRuleSelectionAndQuantityState')
    SharedDungeonControlAndTrapState = @('SharedDungeonTrapPlacementState', 'SharedDungeonControlLineGeometryState')
    SharedSceneZoneDefinitionState = @('SharedSceneZoneGeometryAndThresholdState', 'SharedSceneBiomeAndEventDefinitionState')
    TileObjectPlacementRuleState = @('TileObjectAnchorAndLiquidPlacementState', 'TileObjectPlacementHookAndBaseState')
    MainCageTerrestrialCritterAnimationState = @('MainCageMammalAndReptileAnimationState', 'MainCageInsectAndSmallCritterAnimationState')
    SharedAudioLegacySoundCatalogInstanceState = @('SharedAudioLegacyEnvironmentalInstanceState', 'SharedAudioLegacyPlayerAndInterfaceInstanceState')
    SharedAudioLegacySoundDefinitionCatalogState = @('SharedAudioLegacyGameplaySoundDefinitionCatalogState', 'SharedAudioLegacyInterfaceSoundDefinitionCatalogState')
    MapEncodingHeaderCatalogState = @('MapEncodingHeaderBitCatalogState', 'MapEncodingOptionLimitState')
    UiItemSlotCreativeCraftingAndUtilityContexts = @('UiItemSlotCreativeAndCraftingContextState', 'UiItemSlotHotbarDisplayAndUtilityContextState')
}
$expectedSixthLevelPeerMap = [ordered]@{
    SharedTimeLoggerWorldRenderPhaseMetricsState = @('SharedTimeLoggerTileAndLiquidRenderMetricsState', 'SharedTimeLoggerLightingMapAndBackgroundMetricsState')
    SharedFishingEnvironmentConditionCatalogState = @('SharedFishingConditionCatalogPopulationState', 'SharedFishingEnvironmentPredicateState')
    SharedDropRuleSelectionAndQuantityState = @('SharedDropRuleChanceAndQuantityState', 'SharedDropRuleOptionSelectionState')
    TileObjectStyleCatalogState = @('TileObjectStyleDefinitionCatalogState', 'TileObjectStyleSelectionAndOverrideState')
    SharedDungeonRoomGeometryState = @('SharedDungeonRoomShapeGeometryState', 'SharedDungeonRoomPlacementGeometryState')
    UiItemSortingCombatAndEquipmentCatalogState = @('UiItemSortingWeaponAndToolCatalogState', 'UiItemSortingArmorAndAccessoryCatalogState')
    SharedDungeonStyleFurnitureAndRoomState = @('SharedDungeonStyleFurnitureCatalogState', 'SharedDungeonStyleRoomVariantState')
    SharedAudioLegacyEnvironmentalInstanceState = @('SharedAudioLegacyWorldEnvironmentInstanceState', 'SharedAudioLegacyEntityFeedbackInstanceState')
    SharedItemUseAndToolCapabilityState = @('SharedItemUseTimingAndConsumptionState', 'SharedItemToolPlacementCapabilityState')
    SharedSceneBiomeAndEventDefinitionState = @('SharedSceneBiomeZoneDefinitionState', 'SharedSceneWeatherAndEventZoneState')
    SharedTilePaintState = @('SharedTilePaintRenderTargetState', 'SharedTilePaintVariationAndColorState')
    SharedInvasionEventState = @('SharedInvasionDamageTrackingState', 'SharedInvasionWaveAndArenaState')
    SharedStartupAndIssueReporting = @('SharedIssueReportCatalogState', 'SharedStartupAndRuntimeHostState')
}
$expectedFourthLevelStats = @{
    TileObjectPlacementRuleState = @{ Field = 18; Property = 20; Total = 38 }
    TileObjectStyleAndDrawState = @{ Field = 28; Property = 28; Total = 56 }
    SharedTimeLoggerPhaseMetricsState = @{ Field = 66; Property = 0; Total = 66 }
    SharedTimeLoggerDisplayFormattingState = @{ Field = 9; Property = 0; Total = 9 }
    SharedDropRuleSelectionAndConditionState = @{ Field = 45; Property = 0; Total = 45 }
    SharedDropRuleChainState = @{ Field = 3; Property = 18; Total = 21 }
    SharedBiomeTerrainPassState = @{ Field = 14; Property = 2; Total = 16 }
    SharedDungeonControlAndTrapState = @{ Field = 42; Property = 1; Total = 43 }
    SharedFishingConditionCatalogState = @{ Field = 50; Property = 0; Total = 50 }
    SharedFishingDropResolutionState = @{ Field = 8; Property = 0; Total = 8 }
    MainCageBirdAnimationState = @{ Field = 20; Property = 0; Total = 20 }
    MainCageTerrestrialCritterAnimationState = @{ Field = 37; Property = 0; Total = 37 }
    UiItemSortingCombatAndEquipmentCatalogState = @{ Field = 29; Property = 0; Total = 29 }
    UiItemSortingConsumableAndMiscCatalogState = @{ Field = 25; Property = 0; Total = 25 }
    SharedSceneZoneDefinitionState = @{ Field = 38; Property = 0; Total = 38 }
    SharedSceneScanAccumulatorState = @{ Field = 8; Property = 5; Total = 13 }
    SharedCreativePerPlayerPowerState = @{ Field = 11; Property = 9; Total = 20 }
    SharedCreativeSharedPowerState = @{ Field = 9; Property = 15; Total = 24 }
    SharedDungeonStyleMaterialAndGeometryState = @{ Field = 12; Property = 0; Total = 12 }
    SharedDungeonStyleFurnitureAndRoomState = @{ Field = 27; Property = 0; Total = 27 }
    SharedSceneTileAggregateState = @{ Field = 0; Property = 19; Total = 19 }
    SharedSceneDecorationAndAudioState = @{ Field = 0; Property = 20; Total = 20 }
    SharedAudioLegacySoundCatalogInstanceState = @{ Field = 35; Property = 0; Total = 35 }
    SharedAudioLegacyTrackedInstanceState = @{ Field = 2; Property = 0; Total = 2 }
    SharedDungeonRoomGeometryState = @{ Field = 29; Property = 0; Total = 29 }
    SharedDungeonRoomVariantCatalogState = @{ Field = 8; Property = 0; Total = 8 }
    SharedAudioLegacySoundDefinitionCatalogState = @{ Field = 34; Property = 0; Total = 34 }
    SharedAudioLegacySoundServicesState = @{ Field = 2; Property = 0; Total = 2 }
    MapEncodingHeaderCatalogState = @{ Field = 32; Property = 0; Total = 32 }
    MapIoRuntimeState = @{ Field = 4; Property = 0; Total = 4 }
    MainTileBehaviorAndInteractionMetadata = @{ Field = 20; Property = 0; Total = 20 }
    MainTileLightingAndFrameMetadata = @{ Field = 15; Property = 0; Total = 15 }
    UiItemSlotEquipmentAndDisplayContexts = @{ Field = 2; Property = 0; Total = 2 }
    UiItemSlotCreativeCraftingAndUtilityContexts = @{ Field = 30; Property = 0; Total = 30 }
    SharedItemEconomyAndValueRules = @{ Field = 12; Property = 0; Total = 12 }
    SharedItemUseTimingAndStackRules = @{ Field = 18; Property = 0; Total = 18 }
    SharedPopupTextContentAndContextState = @{ Field = 14; Property = 0; Total = 14 }
    SharedPopupTextRenderLifecycleState = @{ Field = 16; Property = 0; Total = 16 }
    MainBackgroundLayerCatalogState = @{ Field = 20; Property = 0; Total = 20 }
    MainBackgroundParallaxAndStyleState = @{ Field = 9; Property = 0; Total = 9 }
    SharedShaderFamilyDataState = @{ Field = 19; Property = 1; Total = 20 }
    SharedShaderRegistryAndLookupState = @{ Field = 9; Property = 0; Total = 9 }
    SharedDungeonStyleObjectConstants = @{ Field = 21; Property = 0; Total = 21 }
    SharedDungeonStyleBannerAndTrapConstants = @{ Field = 7; Property = 0; Total = 7 }
    SharedTilePlacementCoordinateAndDrawModules = @{ Field = 14; Property = 0; Total = 14 }
    SharedTilePlacementBaseAndStyleModules = @{ Field = 14; Property = 0; Total = 14 }
    SharedWorldItemIdentityAndEconomyPayloadState = @{ Field = 0; Property = 9; Total = 9 }
    SharedWorldItemUseAndPresentationPayloadState = @{ Field = 0; Property = 19; Total = 19 }
    NetworkRemoteClientConnectionAndStatusState = @{ Field = 13; Property = 0; Total = 13 }
    NetworkRemoteClientSectionAndRateLimitState = @{ Field = 13; Property = 1; Total = 14 }
    NetworkSessionConfigurationState = @{ Field = 11; Property = 1; Total = 12 }
    NetworkSessionTransportAndThreadState = @{ Field = 15; Property = 0; Total = 15 }
    SharedItemEmergencyStackingPolicyState = @{ Field = 6; Property = 0; Total = 6 }
    SharedItemEmergencyStackingTransferState = @{ Field = 19; Property = 2; Total = 21 }
    SharedBestiaryInfoElementState = @{ Field = 14; Property = 3; Total = 17 }
    SharedBestiaryCollectionProviderState = @{ Field = 7; Property = 3; Total = 10 }
    EntityIdentityAndMotionState = @{ Field = 7; Property = 0; Total = 7 }
    EntityBoundsAndFluidState = @{ Field = 7; Property = 13; Total = 20 }
    SharedCreativePowerIconLocationCatalogState = @{ Field = 23; Property = 0; Total = 23 }
    SharedCreativePowerIconLayoutState = @{ Field = 3; Property = 0; Total = 3 }
}
$expectedFifthLevelStats = @{
    SharedTimeLoggerWorldRenderPhaseMetricsState = @{ Field = 46; Property = 0; Total = 46 }
    SharedTimeLoggerEntityAndInterfacePhaseMetricsState = @{ Field = 20; Property = 0; Total = 20 }
    TileObjectStyleCatalogState = @{ Field = 26; Property = 10; Total = 36 }
    TileObjectDrawGeometryState = @{ Field = 2; Property = 18; Total = 20 }
    SharedFishingRarityConditionCatalogState = @{ Field = 11; Property = 0; Total = 11 }
    SharedFishingEnvironmentConditionCatalogState = @{ Field = 39; Property = 0; Total = 39 }
    SharedDropRuleConditionBranchState = @{ Field = 9; Property = 0; Total = 9 }
    SharedDropRuleSelectionAndQuantityState = @{ Field = 36; Property = 0; Total = 36 }
    SharedDungeonTrapPlacementState = @{ Field = 22; Property = 0; Total = 22 }
    SharedDungeonControlLineGeometryState = @{ Field = 20; Property = 1; Total = 21 }
    SharedSceneZoneGeometryAndThresholdState = @{ Field = 12; Property = 0; Total = 12 }
    SharedSceneBiomeAndEventDefinitionState = @{ Field = 26; Property = 0; Total = 26 }
    TileObjectAnchorAndLiquidPlacementState = @{ Field = 10; Property = 15; Total = 25 }
    TileObjectPlacementHookAndBaseState = @{ Field = 8; Property = 5; Total = 13 }
    MainCageMammalAndReptileAnimationState = @{ Field = 18; Property = 0; Total = 18 }
    MainCageInsectAndSmallCritterAnimationState = @{ Field = 19; Property = 0; Total = 19 }
    SharedAudioLegacyEnvironmentalInstanceState = @{ Field = 26; Property = 0; Total = 26 }
    SharedAudioLegacyPlayerAndInterfaceInstanceState = @{ Field = 9; Property = 0; Total = 9 }
    SharedAudioLegacyGameplaySoundDefinitionCatalogState = @{ Field = 25; Property = 0; Total = 25 }
    SharedAudioLegacyInterfaceSoundDefinitionCatalogState = @{ Field = 9; Property = 0; Total = 9 }
    MapEncodingHeaderBitCatalogState = @{ Field = 24; Property = 0; Total = 24 }
    MapEncodingOptionLimitState = @{ Field = 8; Property = 0; Total = 8 }
    UiItemSlotCreativeAndCraftingContextState = @{ Field = 6; Property = 0; Total = 6 }
    UiItemSlotHotbarDisplayAndUtilityContextState = @{ Field = 24; Property = 0; Total = 24 }
}
$expectedSixthLevelStats = @{
    SharedTimeLoggerTileAndLiquidRenderMetricsState = @{ Field = 24; Property = 0; Total = 24 }
    SharedTimeLoggerLightingMapAndBackgroundMetricsState = @{ Field = 22; Property = 0; Total = 22 }
    SharedFishingConditionCatalogPopulationState = @{ Field = 8; Property = 0; Total = 8 }
    SharedFishingEnvironmentPredicateState = @{ Field = 31; Property = 0; Total = 31 }
    SharedDropRuleChanceAndQuantityState = @{ Field = 16; Property = 0; Total = 16 }
    SharedDropRuleOptionSelectionState = @{ Field = 20; Property = 0; Total = 20 }
    TileObjectStyleDefinitionCatalogState = @{ Field = 24; Property = 4; Total = 28 }
    TileObjectStyleSelectionAndOverrideState = @{ Field = 2; Property = 6; Total = 8 }
    SharedDungeonRoomShapeGeometryState = @{ Field = 13; Property = 0; Total = 13 }
    SharedDungeonRoomPlacementGeometryState = @{ Field = 16; Property = 0; Total = 16 }
    UiItemSortingWeaponAndToolCatalogState = @{ Field = 21; Property = 0; Total = 21 }
    UiItemSortingArmorAndAccessoryCatalogState = @{ Field = 8; Property = 0; Total = 8 }
    SharedDungeonStyleFurnitureCatalogState = @{ Field = 25; Property = 0; Total = 25 }
    SharedDungeonStyleRoomVariantState = @{ Field = 2; Property = 0; Total = 2 }
    SharedAudioLegacyWorldEnvironmentInstanceState = @{ Field = 16; Property = 0; Total = 16 }
    SharedAudioLegacyEntityFeedbackInstanceState = @{ Field = 10; Property = 0; Total = 10 }
    SharedItemUseTimingAndConsumptionState = @{ Field = 15; Property = 0; Total = 15 }
    SharedItemToolPlacementCapabilityState = @{ Field = 11; Property = 0; Total = 11 }
    SharedSceneBiomeZoneDefinitionState = @{ Field = 17; Property = 0; Total = 17 }
    SharedSceneWeatherAndEventZoneState = @{ Field = 9; Property = 0; Total = 9 }
    SharedTilePaintRenderTargetState = @{ Field = 12; Property = 0; Total = 12 }
    SharedTilePaintVariationAndColorState = @{ Field = 14; Property = 0; Total = 14 }
    SharedInvasionDamageTrackingState = @{ Field = 2; Property = 2; Total = 4 }
    SharedInvasionWaveAndArenaState = @{ Field = 19; Property = 3; Total = 22 }
    SharedIssueReportCatalogState = @{ Field = 3; Property = 0; Total = 3 }
    SharedStartupAndRuntimeHostState = @{ Field = 12; Property = 11; Total = 23 }
}
$expectedThirdLevelIds = @($expectedThirdLevelPeerMap.Values | ForEach-Object { $_ })
foreach ($entry in $expectedThirdLevelPeerMap.GetEnumerator()) {
    $children = @($entry.Value)
    $activeDirectChildren = 0
    foreach ($child in $children) {
        if ($expectedFourthLevelPeerMap.Contains($child)) {
            $retiredNestedStats = @($groupStats | Where-Object Fine -eq $child)
            Assert-Equal -Actual $retiredNestedStats.Count -Expected 0 -Message "Retired fourth-level peer '$child' active count"
            continue
        }
        $activeDirectChildren++
        $childStats = @($groupStats | Where-Object Fine -eq $child)
        Assert-Equal -Actual $childStats.Count -Expected 1 -Message "Third-level child '$child' active count"
        Assert-Equal -Actual $childStats[0].PreviousPeer -Expected $entry.Key -Message "Immediate peer for third-level child '$child'"
        $expectedStats = $expectedThirdLevelStats[$child]
        Assert-Equal -Actual $childStats[0].Field -Expected $expectedStats.Field -Message "Field count for third-level child '$child'"
        Assert-Equal -Actual $childStats[0].Property -Expected $expectedStats.Property -Message "Property count for third-level child '$child'"
        Assert-Equal -Actual $childStats[0].Total -Expected $expectedStats.Total -Message "Total count for third-level child '$child'"
    }
    $retiredStats = @($groupStats | Where-Object Fine -eq $entry.Key)
    Assert-Equal -Actual $retiredStats.Count -Expected 0 -Message "Retired third-level peer '$($entry.Key)' active count"
    $activeChildren = @($groupStats | Where-Object PreviousPeer -eq $entry.Key)
    Assert-Equal -Actual $activeChildren.Count -Expected $activeDirectChildren -Message "Third-level peer '$($entry.Key)' direct child count"
    Assert-Equal -Actual (@($activeChildren | Select-Object -ExpandProperty Baseline -Unique).Count) -Expected 1 -Message "Third-level peer '$($entry.Key)' baseline count"
}
Assert-Equal -Actual $expectedThirdLevelIds.Count -Expected 10 -Message 'Third-level child definition count'
Assert-Equal -Actual $expectedFourthLevelPeerMap.Count -Expected 30 -Message 'Fourth-level retired peer definition count'
Assert-Equal -Actual (@($expectedFourthLevelPeerMap.Values | ForEach-Object { $_ })).Count -Expected 60 -Message 'Fourth-level child definition count'
Assert-Equal -Actual $expectedFifthLevelPeerMap.Count -Expected 12 -Message 'Fifth-level retired peer definition count'
Assert-Equal -Actual (@($expectedFifthLevelPeerMap.Values | ForEach-Object { $_ })).Count -Expected 24 -Message 'Fifth-level child definition count'
Assert-Equal -Actual $expectedSixthLevelPeerMap.Count -Expected 13 -Message 'Sixth-level retired peer definition count'
Assert-Equal -Actual (@($expectedSixthLevelPeerMap.Values | ForEach-Object { $_ })).Count -Expected 26 -Message 'Sixth-level child definition count'

foreach ($entry in $expectedFourthLevelPeerMap.GetEnumerator()) {
    $retiredStats = @($groupStats | Where-Object Fine -eq $entry.Key)
    Assert-Equal -Actual $retiredStats.Count -Expected 0 -Message "Retired fourth-level peer '$($entry.Key)' active count"
    $activeChildren = @($groupStats | Where-Object PreviousPeer -eq $entry.Key)
    $nestedRetiredChildren = @($entry.Value | Where-Object { $expectedFifthLevelPeerMap.Contains($_) -or $expectedSixthLevelPeerMap.Contains($_) })
    Assert-Equal -Actual $activeChildren.Count -Expected (2 - $nestedRetiredChildren.Count) -Message "Fourth-level peer '$($entry.Key)' direct child count"
    $baselineCandidates = @($activeChildren | Select-Object -ExpandProperty Baseline)
    foreach ($nestedChild in $nestedRetiredChildren) {
        $baselineCandidates += @($groupStats | Where-Object PreviousPeer -eq $nestedChild | Select-Object -ExpandProperty Baseline)
    }
    Assert-Equal -Actual (@($baselineCandidates | Select-Object -Unique).Count) -Expected 1 -Message "Fourth-level peer '$($entry.Key)' baseline count"
    foreach ($child in @($entry.Value)) {
        if ($expectedFifthLevelPeerMap.Contains($child) -or $expectedSixthLevelPeerMap.Contains($child)) {
            $nestedRetiredStats = @($groupStats | Where-Object Fine -eq $child)
            Assert-Equal -Actual $nestedRetiredStats.Count -Expected 0 -Message "Retired nested peer '$child' active count"
            continue
        }
        $childStats = @($groupStats | Where-Object Fine -eq $child)
        Assert-Equal -Actual $childStats.Count -Expected 1 -Message "Fourth-level child '$child' active count"
        Assert-Equal -Actual $childStats[0].PreviousPeer -Expected $entry.Key -Message "Immediate peer for fourth-level child '$child'"
        $expectedStats = $expectedFourthLevelStats[$child]
        Assert-Equal -Actual $childStats[0].Field -Expected $expectedStats.Field -Message "Field count for fourth-level child '$child'"
        Assert-Equal -Actual $childStats[0].Property -Expected $expectedStats.Property -Message "Property count for fourth-level child '$child'"
        Assert-Equal -Actual $childStats[0].Total -Expected $expectedStats.Total -Message "Total count for fourth-level child '$child'"
    }
}

foreach ($entry in $expectedFifthLevelPeerMap.GetEnumerator()) {
    $retiredStats = @($groupStats | Where-Object Fine -eq $entry.Key)
    Assert-Equal -Actual $retiredStats.Count -Expected 0 -Message "Retired fifth-level peer '$($entry.Key)' active count"
    $activeChildren = @($groupStats | Where-Object PreviousPeer -eq $entry.Key)
    $nestedRetiredChildren = @($entry.Value | Where-Object { $expectedSixthLevelPeerMap.Contains($_) })
    Assert-Equal -Actual $activeChildren.Count -Expected (2 - $nestedRetiredChildren.Count) -Message "Fifth-level peer '$($entry.Key)' direct child count"
    $baselineCandidates = @($activeChildren | Select-Object -ExpandProperty Baseline)
    foreach ($nestedChild in $nestedRetiredChildren) {
        $baselineCandidates += @($groupStats | Where-Object PreviousPeer -eq $nestedChild | Select-Object -ExpandProperty Baseline)
    }
    Assert-Equal -Actual (@($baselineCandidates | Select-Object -Unique).Count) -Expected 1 -Message "Fifth-level peer '$($entry.Key)' baseline count"
    foreach ($child in @($entry.Value)) {
        if ($expectedSixthLevelPeerMap.Contains($child)) {
            $nestedRetiredStats = @($groupStats | Where-Object Fine -eq $child)
            Assert-Equal -Actual $nestedRetiredStats.Count -Expected 0 -Message "Retired sixth-level peer '$child' active count"
            continue
        }
        $childStats = @($groupStats | Where-Object Fine -eq $child)
        Assert-Equal -Actual $childStats.Count -Expected 1 -Message "Fifth-level child '$child' active count"
        Assert-Equal -Actual $childStats[0].PreviousPeer -Expected $entry.Key -Message "Immediate peer for fifth-level child '$child'"
        $expectedStats = $expectedFifthLevelStats[$child]
        Assert-Equal -Actual $childStats[0].Field -Expected $expectedStats.Field -Message "Field count for fifth-level child '$child'"
        Assert-Equal -Actual $childStats[0].Property -Expected $expectedStats.Property -Message "Property count for fifth-level child '$child'"
        Assert-Equal -Actual $childStats[0].Total -Expected $expectedStats.Total -Message "Total count for fifth-level child '$child'"
    }
    $retiredNestedTotal = 0
    foreach ($nestedPeer in $nestedRetiredChildren) {
        foreach ($nestedChild in $expectedSixthLevelPeerMap[$nestedPeer]) {
            $retiredNestedTotal += $expectedSixthLevelStats[$nestedChild].Total
        }
    }
    $expectedPeerTotal = 0
    foreach ($child in $entry.Value) {
        if ($expectedSixthLevelPeerMap.Contains($child)) {
            foreach ($nestedChild in $expectedSixthLevelPeerMap[$child]) {
                $expectedPeerTotal += $expectedSixthLevelStats[$nestedChild].Total
            }
            continue
        }
        $expectedPeerTotal += $expectedFifthLevelStats[$child].Total
    }
    Assert-Equal -Actual ((Get-CountSum -Items $activeChildren -Property Total) + $retiredNestedTotal) -Expected $expectedPeerTotal -Message "Fifth-level member rollup for $($entry.Key)"
}

foreach ($entry in $expectedSixthLevelPeerMap.GetEnumerator()) {
    $retiredStats = @($groupStats | Where-Object Fine -eq $entry.Key)
    Assert-Equal -Actual $retiredStats.Count -Expected 0 -Message "Retired sixth-level peer '$($entry.Key)' active count"
    $activeChildren = @($groupStats | Where-Object PreviousPeer -eq $entry.Key)
    Assert-Equal -Actual $activeChildren.Count -Expected 2 -Message "Sixth-level peer '$($entry.Key)' child count"
    Assert-Equal -Actual (@($activeChildren | Select-Object -ExpandProperty Baseline -Unique).Count) -Expected 1 -Message "Sixth-level peer '$($entry.Key)' baseline count"
    foreach ($child in @($entry.Value)) {
        $childStats = @($groupStats | Where-Object Fine -eq $child)
        Assert-Equal -Actual $childStats.Count -Expected 1 -Message "Sixth-level child '$child' active count"
        Assert-Equal -Actual $childStats[0].PreviousPeer -Expected $entry.Key -Message "Immediate peer for sixth-level child '$child'"
        $expectedStats = $expectedSixthLevelStats[$child]
        Assert-Equal -Actual $childStats[0].Field -Expected $expectedStats.Field -Message "Field count for sixth-level child '$child'"
        Assert-Equal -Actual $childStats[0].Property -Expected $expectedStats.Property -Message "Property count for sixth-level child '$child'"
        Assert-Equal -Actual $childStats[0].Total -Expected $expectedStats.Total -Message "Total count for sixth-level child '$child'"
    }
    Assert-Equal -Actual (Get-CountSum -Items $activeChildren -Property Total) -Expected ($expectedSixthLevelStats[$entry.Value[0]].Total + $expectedSixthLevelStats[$entry.Value[1]].Total) -Message "Sixth-level member rollup for $($entry.Key)"
}

$summaryTables = Read-SummaryTables -Path $reportFullPath
Assert-Equal -Actual $summaryTables.ReportedFineCount -Expected 349 -Message 'Reported final fine-subsystem count'
Assert-Equal -Actual $summaryTables.FullRankingRows.Count -Expected 349 -Message 'Full ranking summary row count'
foreach ($stat in $groupStats) {
    Assert-Equal -Actual ($stat.Field + $stat.Property) -Expected $stat.Total -Message "field + property for $($stat.Fine)"
    $baselineIds = @($reportMembers | Where-Object { $_.Parent -eq $stat.Parent -and $_.Fine -eq $stat.Fine } | Select-Object -ExpandProperty Baseline -Unique)
    Assert-Equal -Actual $baselineIds.Count -Expected 1 -Message "single baseline for $($stat.Fine)"
    Assert-Equal -Actual $stat.Baseline -Expected $baselineIds[0] -Message "baseline identity for $($stat.Fine)"
    $previousPeerIds = @(
        $reportMembers |
            Where-Object { $_.Parent -eq $stat.Parent -and $_.Fine -eq $stat.Fine } |
            ForEach-Object { if ([string]::IsNullOrWhiteSpace($_.PreviousPeer)) { $_.Fine } else { $_.PreviousPeer } } |
            Select-Object -Unique
    )
    Assert-Equal -Actual $previousPeerIds.Count -Expected 1 -Message "single immediate peer for $($stat.Fine)"
    Assert-Equal -Actual $stat.PreviousPeer -Expected $previousPeerIds[0] -Message "immediate peer identity for $($stat.Fine)"
}

$expectedTop50BaselineOrder = @(
    'TileObjectDefinitionCatalog', 'SharedTimeLoggerCoordinatorState', 'SharedSceneMetricsSnapshot',
    'SharedDropRuleDefinitions', 'AudioLegacySoundAdapterState', 'MainCageAnimationState',
    'SharedFishingDropRuleDefinitions', 'SharedWorldGenerationBiomeSupport', 'UiItemSorting',
    'SharedDungeonRoomDefinitions', 'SharedContentUiWidgets', 'SharedDungeonStyleCatalog',
    'SharedPopupAndCombatText', 'SharedShaderData', 'SharedTilePlacementModules',
    'SharedCreativePowerDefinitions', 'UiItemSlotContextDefinitions', 'MapTileStorageAndUpdateState',
    'SharedDungeonGeometryAndPlacementDefinitions', 'SharedItemUseToolAndCombatState',
    'SharedWorldItemState', 'SharedDungeonHallDefinitions', 'MainTileAndWallMetadata',
    'SharedTileObjectPlacementDefinitions', 'LightingEngineState',
    'SharedItemEquipmentAndPresentationState', 'SharedItemStaticCatalogRules',
    'SharedDungeonGenerationDataState', 'MainBackgroundAndSeasonalState',
    'SharedDungeonGenerationQueries', 'SharedWeatherParticleState', 'UiElementTreeState',
    'WorldFileMetadataAndSession', 'WorldFileTilePacking', 'MinecartMotionState',
    'PlayerIntentAndMovementState', 'RecipeDefinitionCatalog', 'SharedParticleAndGoreEffects',
    'MainDerivedPropertiesAndEvents', 'SharedChatPresentation', 'SharedFishingAttemptAndConditions',
    'TileCellStorage', 'CameraAndVertexPresentation', 'WorldFileRecoveryIo',
    'SharedAmbientSkyAndWindState', 'SharedCreativePowerPresentation',
    'SharedDungeonLegacyGenerationGlobals', 'SharedGeneralPureUtilities',
    'SharedSeasonalWorldEventState', 'UiItemSlotPresentationAndPulseState'
)
$expectedBaselineTop50 = @($baselineStats | Sort-Object @{ Expression = 'Total'; Descending = $true }, @{ Expression = 'Field'; Descending = $true }, @{ Expression = 'Property'; Descending = $true }, Parent, Fine | Select-Object -First 50)
Assert-Equal -Actual $expectedBaselineTop50.Count -Expected 50 -Message 'Recomputed baseline top-50 count'
for ($index = 0; $index -lt $expectedBaselineTop50.Count; $index++) {
    Assert-Equal -Actual $expectedBaselineTop50[$index].Fine -Expected $expectedTop50BaselineOrder[$index] -Message "Baseline top-50 identity at position $($index + 1)"
}

$expectedTop50 = @($groupStats | Sort-Object @{ Expression = 'Total'; Descending = $true }, @{ Expression = 'Field'; Descending = $true }, @{ Expression = 'Property'; Descending = $true }, Parent, Fine | Select-Object -First 50)
Assert-Equal -Actual $summaryTables.Top50Rows.Count -Expected 50 -Message 'Top-50 summary row count'
for ($index = 0; $index -lt $expectedTop50.Count; $index++) {
    $expected = $expectedTop50[$index]
    $actual = $summaryTables.Top50Rows[$index]
    Assert-Equal -Actual $actual.Rank -Expected ($index + 1) -Message "Top-50 rank at position $($index + 1)"
    Assert-Equal -Actual $actual.Parent -Expected $expected.Parent -Message "Top-50 parent at rank $($index + 1)"
    Assert-Equal -Actual $actual.Fine -Expected $expected.Fine -Message "Top-50 fine subsystem at rank $($index + 1)"
    Assert-Equal -Actual $actual.Baseline -Expected $expected.Baseline -Message "Top-50 baseline identity at rank $($index + 1)"
    Assert-Equal -Actual $actual.Field -Expected $expected.Field -Message "Top-50 field total at rank $($index + 1)"
    Assert-Equal -Actual $actual.Property -Expected $expected.Property -Message "Top-50 property total at rank $($index + 1)"
    Assert-Equal -Actual $actual.Total -Expected $expected.Total -Message "Top-50 member total at rank $($index + 1)"
    if ([string]::IsNullOrWhiteSpace($actual.Decision) -or [string]::IsNullOrWhiteSpace($actual.Boundary) -or [string]::IsNullOrWhiteSpace($actual.Evidence)) {
        throw "Top-50 row at rank $($index + 1) does not contain a public-decomposition decision, boundary, and evidence status."
    }
    if ($actual.Boundary -notlike "*基线映射：$($expected.Baseline)*") {
        throw "Top-50 row at rank $($index + 1) does not retain baseline mapping '$($expected.Baseline)'."
    }
}

foreach ($expected in $expectedBaselineTop50) {
    $peerStats = @($groupStats | Where-Object Baseline -eq $expected.Fine)
    if ($peerStats.Count -lt 2) {
        throw "Baseline top-50 subsystem '$($expected.Fine)' has fewer than two final peer groups."
    }
    if (@($peerStats | Where-Object Fine -eq $expected.Fine).Count -ne 0) {
        throw "Baseline top-50 subsystem '$($expected.Fine)' is still active as a final fine subsystem."
    }
    Assert-Equal -Actual (Get-CountSum -Items $peerStats -Property Field) -Expected $expected.Field -Message "Peer field rollup for $($expected.Fine)"
    Assert-Equal -Actual (Get-CountSum -Items $peerStats -Property Property) -Expected $expected.Property -Message "Peer property rollup for $($expected.Fine)"
    Assert-Equal -Actual (Get-CountSum -Items $peerStats -Property Total) -Expected $expected.Total -Message "Peer member rollup for $($expected.Fine)"
}

$expectedFullRanking = @($groupStats | Sort-Object @{ Expression = 'Total'; Descending = $true }, @{ Expression = 'Field'; Descending = $true }, @{ Expression = 'Property'; Descending = $true }, Parent, Fine)
Assert-Equal -Actual $summaryTables.FullRankingRows.Count -Expected $expectedFullRanking.Count -Message 'Full final fine-subsystem ranking row count'
for ($index = 0; $index -lt $expectedFullRanking.Count; $index++) {
    $expected = $expectedFullRanking[$index]
    $actual = $summaryTables.FullRankingRows[$index]
    Assert-Equal -Actual $actual.Rank -Expected ($index + 1) -Message "Full ranking rank at position $($index + 1)"
    Assert-Equal -Actual $actual.Parent -Expected $expected.Parent -Message "Full ranking parent at position $($index + 1)"
    Assert-Equal -Actual $actual.Fine -Expected $expected.Fine -Message "Full ranking fine subsystem at position $($index + 1)"
    Assert-Equal -Actual $actual.Baseline -Expected $expected.Baseline -Message "Full ranking baseline at position $($index + 1)"
    Assert-Equal -Actual $actual.Field -Expected $expected.Field -Message "Full ranking field total at position $($index + 1)"
    Assert-Equal -Actual $actual.Property -Expected $expected.Property -Message "Full ranking property total at position $($index + 1)"
    Assert-Equal -Actual $actual.Total -Expected $expected.Total -Message "Full ranking member total at position $($index + 1)"
}

Assert-Equal -Actual $summaryTables.ReportedFineCount -Expected $groupStats.Count -Message 'Reported fine-subsystem count'
Assert-Equal -Actual $summaryTables.TopTotals['field'] -Expected @($inputMembers | Where-Object Kind -eq 'field').Count -Message 'Top-level field total'
Assert-Equal -Actual $summaryTables.TopTotals['property'] -Expected @($inputMembers | Where-Object Kind -eq 'property').Count -Message 'Top-level property total'
Assert-Equal -Actual $summaryTables.TopTotals['合计'] -Expected $inputMembers.Count -Message 'Top-level member total'

$summaryFineByKey = @{}
foreach ($summaryFine in $summaryTables.FineRows) {
    $key = "$($summaryFine.Parent)$([char]31)$($summaryFine.Fine)"
    if ($summaryFineByKey.ContainsKey($key)) {
        throw "Duplicate fine summary row for parent '$($summaryFine.Parent)', fine subsystem '$($summaryFine.Fine)'."
    }
    $summaryFineByKey[$key] = $summaryFine
}
Assert-Equal -Actual $summaryFineByKey.Count -Expected $groupStats.Count -Message 'Fine summary row count'
foreach ($stat in $groupStats) {
    $key = "$($stat.Parent)$([char]31)$($stat.Fine)"
    if (-not $summaryFineByKey.ContainsKey($key)) {
        throw "Fine summary is missing parent '$($stat.Parent)', fine subsystem '$($stat.Fine)'."
    }
    $summaryFine = $summaryFineByKey[$key]
    Assert-Equal -Actual $summaryFine.Field -Expected $stat.Field -Message "Fine summary field total for $($stat.Fine)"
    Assert-Equal -Actual $summaryFine.Property -Expected $stat.Property -Message "Fine summary property total for $($stat.Fine)"
    Assert-Equal -Actual $summaryFine.Total -Expected $stat.Total -Message "Fine summary member total for $($stat.Fine)"
    Assert-Equal -Actual ($summaryFine.Field + $summaryFine.Property) -Expected $summaryFine.Total -Message "Fine summary field + property for $($stat.Fine)"
}

$expectedParentOrder = @(
    'RuntimeComposition',
    'PersistenceAndRecovery',
    'NetworkSessionAndSectionStreaming',
    'ContentLifecycleAndRegistration',
    'WorldStorage',
    'ContentCatalog',
    'IntentAndInteraction',
    'ExternalBoundaries',
    'SharedRuntimeMechanisms',
    'ExternalDependencyOrGenerated',
    'ClientPresentationAndTools'
)
$summaryParentByName = @{}
foreach ($summaryParent in $summaryTables.ParentRows) {
    if ($summaryParentByName.ContainsKey($summaryParent.Parent)) {
        throw "Duplicate parent summary row for '$($summaryParent.Parent)'."
    }
    $summaryParentByName[$summaryParent.Parent] = $summaryParent
}
Assert-Equal -Actual $summaryParentByName.Count -Expected $expectedParentOrder.Count -Message 'Parent summary row count'
foreach ($parentName in $expectedParentOrder) {
    if (-not $summaryParentByName.ContainsKey($parentName)) {
        throw "Parent summary is missing '$parentName'."
    }
    $summaryParent = $summaryParentByName[$parentName]
    $sourceItems = @($inputMembers | Where-Object Parent -eq $parentName)
    $fineItems = @($groupStats | Where-Object Parent -eq $parentName)
    $expectedFields = @($sourceItems | Where-Object Kind -eq 'field').Count
    $expectedProperties = @($sourceItems | Where-Object Kind -eq 'property').Count
    $expectedTotal = $sourceItems.Count
    Assert-Equal -Actual $summaryParent.FineCount -Expected $fineItems.Count -Message "Parent summary fine count for $parentName"
    Assert-Equal -Actual $summaryParent.Field -Expected $expectedFields -Message "Parent summary field total for $parentName"
    Assert-Equal -Actual $summaryParent.Property -Expected $expectedProperties -Message "Parent summary property total for $parentName"
    Assert-Equal -Actual $summaryParent.Total -Expected $expectedTotal -Message "Parent summary member total for $parentName"
    Assert-Equal -Actual ($summaryParent.Field + $summaryParent.Property) -Expected $summaryParent.Total -Message "Parent summary field + property for $parentName"
    Assert-Equal -Actual (Get-CountSum -Items $fineItems -Property Field) -Expected $summaryParent.Field -Message "Fine-to-parent field rollup for $parentName"
    Assert-Equal -Actual (Get-CountSum -Items $fineItems -Property Property) -Expected $summaryParent.Property -Message "Fine-to-parent property rollup for $parentName"
    Assert-Equal -Actual (Get-CountSum -Items $fineItems -Property Total) -Expected $summaryParent.Total -Message "Fine-to-parent member rollup for $parentName"
}
foreach ($summaryParentName in $summaryParentByName.Keys) {
    if ($expectedParentOrder -notcontains $summaryParentName) {
        throw "Parent summary contains unexpected parent '$summaryParentName'."
    }
}

$parentStats = @($reportMembers | Group-Object Parent | ForEach-Object {
    $items = $_.Group
    [pscustomobject]@{
        Parent = $_.Name
        Field = @($items | Where-Object Kind -eq 'field').Count
        Property = @($items | Where-Object Kind -eq 'property').Count
        Total = $items.Count
    }
})
foreach ($stat in $parentStats) {
    $sourceItems = @($inputMembers | Where-Object Parent -eq $stat.Parent)
    Assert-Equal -Actual $stat.Field -Expected @($sourceItems | Where-Object Kind -eq 'field').Count -Message "field parent total for $($stat.Parent)"
    Assert-Equal -Actual $stat.Property -Expected @($sourceItems | Where-Object Kind -eq 'property').Count -Message "property parent total for $($stat.Parent)"
}

$idFileRows = @($reportMembers | Where-Object { $_.Cells[3] -cmatch '(^|/)[^/]*IDs?\.cs$' })
Assert-Equal -Actual $idFileRows.Count -Expected 0 -Message 'ID-class file rows'

$inputHash = (Get-FileHash -LiteralPath $inputFullPath -Algorithm SHA256).Hash.ToLowerInvariant()
$hashLine = Select-String -LiteralPath $reportFullPath -Pattern '^\| 非权威父级清单 \| ([0-9a-f]{64}) \|$' | Select-Object -First 1
if ($null -eq $hashLine) { throw 'Fine report does not contain the input SHA-256 trace line.' }
Assert-Equal -Actual $hashLine.Matches[0].Groups[1].Value -Expected $inputHash -Message 'Input SHA-256 trace'

Write-Output 'PASS: input and fine report each contain 4,542 rows.'
Write-Output 'PASS: 4,017 fields + 525 properties = 4,542 rows in both documents.'
Write-Output 'PASS: every input member declaration matches a row in the 7,201-row combined source document.'
Write-Output "PASS: $($groupStats.Count) fine subsystems have no duplicate or omitted sequence; every group satisfies field + property = total."
Write-Output 'PASS: final fine-subsystem top-50 and complete ranking match recomputed totals; second-level baseline top-50 peer rollups remain closed.'
Write-Output "PASS: 11 parent summary rows (including 2 zero-record parents) and $($summaryTables.FineRows.Count) fine summary rows match recomputed totals."
Write-Output "PASS: $(@($parentStats).Count) populated parent totals match the input; ID-class file rows = 0."
