[CmdletBinding()]
param(
    [string]$ReportPath = 'docs/迁移参考表/Version4非权威模拟系统字段属性逐成员源码声明-更细子系统拆分-去除ID类文件.md'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

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

$reportFullPath = [System.IO.Path]::GetFullPath((Join-Path (Get-Location) $ReportPath))
if (-not (Test-Path -LiteralPath $reportFullPath)) {
    throw "Fine report not found: $reportFullPath"
}

$fineIds = [System.Collections.Generic.HashSet[string]]::new()
$fineToBaseline = @{}
$fineToPreviousPeer = @{}
$currentFine = ''
$tick = [char]0x60
foreach ($line in Get-Content -LiteralPath $reportFullPath) {
    if ($line -match "^#### \d+\.\d+\.\d+ 细分子系统：$tick([^$tick]+)$tick") {
        $currentFine = $Matches[1]
        [void]$fineIds.Add($currentFine)
        continue
    }
    if ($line -match "^- 上一级基线细分子系统：$tick([^$tick]+)$tick$") {
        if ([string]::IsNullOrWhiteSpace($currentFine)) {
            throw "Baseline mapping is outside a fine subsystem: $line"
        }
        $fineToBaseline[$currentFine] = $Matches[1]
    }
    if ($line -match "^- 上一级 peer 细分子系统：$tick([^$tick]+)$tick$") {
        if ([string]::IsNullOrWhiteSpace($currentFine)) {
            throw "Immediate-peer mapping is outside a fine subsystem: $line"
        }
        $fineToPreviousPeer[$currentFine] = $Matches[1]
    }
}

$expectedSplitIds = @(
    'SharedDungeonStyleCatalog', 'SharedDungeonLayoutProviderState', 'SharedDungeonGeometryAndPlacementDefinitions',
    'AudioLegacySoundAdapterState', 'AudioPlaybackCoordinatorState', 'AudioTrackedSoundState',
    'UiLayoutPrimitives', 'UiElementTreeState', 'UiEventPayloads', 'UiInputPointerState',
    'NetworkSessionCoordinatorState', 'NetworkRemoteClientState', 'NetworkRemoteServerState', 'NetworkRemoteIpRequestAdapter',
    'SharedDungeonCrawlerRuntimeState', 'SharedDungeonGenerationDataState', 'SharedDungeonLegacyGenerationGlobals',
    'MapTileStorageAndUpdateState', 'MapOverlayAndLayerPresentation', 'WorldMapState',
    'BestiaryCatalogAndEntries', 'BestiaryUnlockTracking', 'BestiaryInfoElementsAndProviders', 'BestiaryFiltersAndSorting',
    'ItemEmergencyStackingState', 'ItemQuickStackingState', 'ItemTransferSettings',
    'LocalizationCultureAndLanguageState', 'LocalizedTextValueState', 'LegacyLanguageCatalogState',
    'TileEntityRegistryAndBaseState', 'TileEntityDisplayAndInventoryState', 'TileEntityAnchorAndSensorState', 'TileEntityWorldInteractionState',
    'LightingEngineState', 'LightMapCacheState', 'TileLightScannerState',
    'PlayerIntentAndMovementState', 'PlayerItemPickupAndRespawnState', 'PlayerPreviewAndRejectionState',
    'DrawAnimationAndFrameState', 'DrawCommandAndBatchState', 'WorldDrawingAuxiliaryState',
    'SharedGeneralPureUtilities', 'SharedGeneralFilePlatformUtilities', 'SharedGeneralDiagnosticsUtilities', 'SharedGeneralDelegateAndMetadataUtilities',
    'DebugCommandProtocol', 'DebugRuntimeOptions', 'DebugFrameTelemetry', 'DebugBuildStatus',
    'ItemVariantDefinitions', 'ItemTagEffectState', 'LegacyItemPrefixCatalog',
    'MinecartMotionState', 'MinecartCustomizationState', 'TrackedProjectileReferenceState',
    'DoorOpeningInteractionState', 'SmartCursorInteractionState', 'PressurePlateInteractionState', 'CursorAndChestInteractionState',
    'RecipeDefinitionCatalog', 'CraftingRequestState',
    'WorldSpawnConfigurationState', 'WorldSeedAndExploitRules', 'EnvironmentDamageAndSeatState', 'WorldEnvironmentScanHelpers',
    'InputProfilesAndConfiguration', 'InputTriggerState', 'PlayerInputRuntimeState', 'LockOnTargetingState',
    'EntityAuthoritativeState', 'EntityShadowPresentationState',
    'ArmorSetBonusDefinitions', 'ArmorSetBonusCatalog', 'WingStatsDefinition', 'EquipmentLoadoutState'
)

$retiredIds = @(
    'SharedDungeonRootLayoutDefinitions', 'AudioPlaybackEngineState', 'UiLayoutAndInteractionTree',
    'NetworkSessionAndRemotePeers', 'SharedDungeonGenerationState', 'MapAndWorldMapPresentation',
    'SharedBestiaryCatalog', 'SharedItemTransferAndStacking', 'SharedLocalizationCatalog',
    'SharedTileEntitiesAndWorldObjects', 'SharedLightingAndLightMaps', 'SharedPlayerIntentAndMovementData',
    'SharedDrawAndAnimationPresentation', 'SharedGeneralUtilityFunctions', 'SharedTestingAndDebugTools',
    'SharedItemVariantsAndPrefixes', 'SharedMinecartAndMotion', 'SharedDirectWorldInteractionHelpers',
    'SharedRecipeAndCraftingRules', 'SharedWorldEnvironmentAndSpawnRules', 'SharedInputBindings',
    'SharedEntityBaseState', 'SharedEquipmentAndArmorDefinitions'
)

$secondLevelBaselineIds = @(
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

$expectedSecondLevelChildIds = @(
    'TileObjectDefinitionInheritanceState', 'TileObjectDefinitionPlacementAndStyleState',
    'SharedTimeLoggerFrameCoordinationState', 'SharedTimeLoggerRenderMetricsState',
    'SharedSceneScanAndZoneState', 'SharedSceneAggregateAndDecorationState',
    'SharedDropRuleResolutionAndCatalogState', 'SharedDropRuleSelectionAndChainState',
    'AudioLegacySoundCatalogState', 'AudioLegacySoundInstanceState',
    'MainCageBirdAndTerrestrialAnimationState', 'MainCageAquaticAndAmphibianAnimationState',
    'SharedFishingDropCatalogState', 'SharedFishingConditionContextState',
    'SharedBiomeDungeonAndTerrainState', 'SharedBiomeCaveHouseAndStructureState',
    'UiItemSortingDefinitions', 'UiItemSortingExecutionState',
    'SharedDungeonRoomCoreAndSettings', 'SharedDungeonRoomShapeVariants',
    'SharedContentUiTextAndOptionWidgets', 'SharedContentUiContainersAndProgress',
    'SharedDungeonStyleSetCatalog', 'SharedDungeonStyleEntryDefinitions',
    'SharedPopupTextState', 'SharedCombatTextState',
    'SharedShaderBaseParameterState', 'SharedShaderFamilyCatalogState',
    'SharedTilePlacementGeometryModules', 'SharedTilePlacementAnchorAndHookModules',
    'SharedCreativePowerContracts', 'SharedCreativePowerDefinitionState',
    'UiItemSlotStorageAndCraftingContexts', 'UiItemSlotEquipmentAndCreativeContexts',
    'MapTileEncodingAndStorageState', 'MapTileUpdateQueueState',
    'SharedDungeonBoundsAndProgressionDefinitions', 'SharedDungeonDoorAndPlatformDefinitions',
    'SharedItemUseAndToolCapabilityState', 'SharedItemCombatAndDamageCapabilityState',
    'SharedWorldItemLifecycleState', 'SharedWorldItemPayloadState',
    'SharedDungeonHallCoreAndSettings', 'SharedDungeonHallLegacyAndGeometry',
    'MainTileBehaviorMetadata', 'MainWallAndGlobalTileMetadata',
    'SharedTileObjectPreviewState', 'SharedTileObjectPlacementValueState',
    'SharedLightingCoordinatorState', 'SharedLegacyLightingState',
    'SharedItemEquipmentSlotState', 'SharedItemAppearanceAndTooltipState',
    'SharedItemStaticEconomyAndTimingRules', 'SharedItemStaticCapabilityRules',
    'SharedDungeonGenerationCollectionsState', 'SharedDungeonGenerationScalarAndStyleState',
    'MainBackgroundLayerState', 'MainSeasonalAndTitleState',
    'SharedDungeonStyleConstantQueries', 'SharedDungeonGeometryRuleQueries',
    'SharedStarParticleState', 'SharedCloudAndRainParticleState',
    'UiElementLayoutAndDimensionsState', 'UiElementInteractionAndLifecycleState',
    'WorldFileMetadataIdentityState', 'WorldFileSessionAndValidityState',
    'WorldFileTileHeaderCoreState', 'WorldFileTileHeaderExtensionState',
    'MinecartMotionAndTrackState', 'MinecartDecorationAndSwitchState',
    'PlayerMovementCapabilityState', 'PlayerIntentAndInteractionState',
    'RecipeDefinitionState', 'RecipeGroupCatalogState',
    'SharedDustParticleState', 'SharedGoreEffectState',
    'MainDerivedWorldAndSessionQueries', 'MainDerivedInputAndPresentationQueries',
    'SharedChatSnippetPresentationState', 'SharedChatMonitorAndCommandState',
    'FishingAttemptState', 'PlayerFishingConditionState',
    'TileCellMaterialAndLiquidState', 'TileCellFrameAndBitState',
    'CameraMatrixAndViewportState', 'VertexStripAndColorState',
    'WorldFileRecoveryVersionState', 'WorldFileTemporaryEventState',
    'SharedAmbientSkyCatalogState', 'SharedAmbientSpawnAndWindState',
    'SharedCreativePowerIconCatalogState', 'SharedCreativePowerUiLayoutState',
    'SharedDungeonLegacyPlacementState', 'SharedDungeonLegacyRuleState',
    'SharedGeneralTeleportAndInterceptionUtilities', 'SharedGeneralRandomAndBufferUtilities',
    'SharedCelebrationAndLanternEvents', 'SharedRitualAndStormEvents',
    'UiItemSlotPulseAndHighlightState', 'UiItemSlotDisplayAndInteractionState'
)

Assert-Equal -Actual $expectedSplitIds.Count -Expected 77 -Message 'Expected first-round split boundary count'
Assert-Equal -Actual $secondLevelBaselineIds.Count -Expected 50 -Message 'Expected second-level baseline count'
Assert-Equal -Actual $expectedSecondLevelChildIds.Count -Expected 100 -Message 'Expected second-level peer boundary count'
Assert-Equal -Actual $fineIds.Count -Expected 294 -Message 'Active fine-subsystem count'

$expectedThirdLevelPeerMap = [ordered]@{
    UiItemSortingDefinitions = @('UiItemSortingLayerCatalogState', 'UiItemSortingRegistryAndRankingState')
    MapTileEncodingAndStorageState = @('MapEncodingCatalogAndIoState', 'MapTileCellState')
    SharedContentUiTextAndOptionWidgets = @('SharedContentUiOptionButtonState', 'SharedContentUiTextAndHeaderState')
    SharedDungeonRoomCoreAndSettings = @('SharedDungeonRoomCoreState', 'SharedDungeonRoomSettingsState')
    SharedDungeonHallCoreAndSettings = @('SharedDungeonHallCoreState', 'SharedDungeonHallSettingsState')
}
$expectedThirdLevelIds = @($expectedThirdLevelPeerMap.Values | ForEach-Object { $_ })
Assert-Equal -Actual $expectedThirdLevelIds.Count -Expected 10 -Message 'Expected third-level peer boundary count'

foreach ($id in $expectedSplitIds) {
    if ($secondLevelBaselineIds -contains $id) {
        continue
    }
    if (-not $fineIds.Contains($id)) {
        throw "Expected split fine subsystem is absent: $id"
    }
}

foreach ($id in $expectedSecondLevelChildIds) {
    if ($expectedThirdLevelPeerMap.Contains($id)) {
        continue
    }
    if (-not $fineIds.Contains($id)) {
        throw "Expected second-level peer fine subsystem is absent: $id"
    }
    if (-not $fineToBaseline.ContainsKey($id)) {
        throw "Second-level peer fine subsystem has no baseline mapping: $id"
    }
}

foreach ($entry in $expectedThirdLevelPeerMap.GetEnumerator()) {
    if ($fineIds.Contains($entry.Key)) {
        throw "Third-level retired peer is still active: $($entry.Key)"
    }
    foreach ($child in @($entry.Value)) {
        if (-not $fineIds.Contains($child)) {
            throw "Expected third-level peer fine subsystem is absent: $child"
        }
        if (-not $fineToPreviousPeer.ContainsKey($child)) {
            throw "Third-level peer fine subsystem has no immediate-peer mapping: $child"
        }
        Assert-Equal -Actual $fineToPreviousPeer[$child] -Expected $entry.Key -Message "Immediate peer for $child"
    }
    $peerCount = @($fineToPreviousPeer.GetEnumerator() | Where-Object Value -eq $entry.Key).Count
    Assert-Equal -Actual $peerCount -Expected 2 -Message "Third-level peer count for $($entry.Key)"
}

foreach ($id in $secondLevelBaselineIds) {
    if ($fineIds.Contains($id)) {
        throw "Second-level baseline fine subsystem is still active: $id"
    }
    $peerCount = @($fineToBaseline.GetEnumerator() | Where-Object Value -eq $id).Count
    if ($peerCount -lt 2) {
        throw "Second-level baseline fine subsystem has fewer than two active peers: $id"
    }
}

foreach ($id in $retiredIds) {
    if ($fineIds.Contains($id)) {
        throw "Retired fine subsystem is still active: $id"
    }
}

Write-Output 'PASS: 77 expected same-level split boundaries are present.'
Write-Output 'PASS: 23 previous mixed fine subsystems are retired.'
Write-Output 'PASS: 50 previous top-ranked baseline subsystems are retired into 100 peer boundaries.'
Write-Output 'PASS: five current peer boundaries are retired into ten third-level peers.'
Write-Output 'PASS: report contains exactly 294 active fine subsystems.'
