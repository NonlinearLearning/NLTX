[CmdletBinding()]
param(
    [string]$ReportPath = 'docs/migration/ledgers/Version4非权威模拟系统字段属性逐成员源码声明-更细子系统拆分-去除ID类文件.md'
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
$fineStats = @{}
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
    if ($line -match '^\|\s*(\d+)\s*\|\s*(field|property)\s*\|') {
        if ([string]::IsNullOrWhiteSpace($currentFine)) {
            throw "Member row is outside a fine subsystem: $line"
        }
        if (-not $fineStats.ContainsKey($currentFine)) {
            $fineStats[$currentFine] = [pscustomobject]@{ Field = 0; Property = 0; Total = 0 }
        }
        if ($Matches[2] -eq 'field') {
            $fineStats[$currentFine].Field++
        } else {
            $fineStats[$currentFine].Property++
        }
        $fineStats[$currentFine].Total++
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
Assert-Equal -Actual $fineIds.Count -Expected 349 -Message 'Active fine-subsystem count'

$expectedThirdLevelPeerMap = [ordered]@{
    UiItemSortingDefinitions = @('UiItemSortingLayerCatalogState', 'UiItemSortingRegistryAndRankingState')
    MapTileEncodingAndStorageState = @('MapEncodingCatalogAndIoState', 'MapTileCellState')
    SharedContentUiTextAndOptionWidgets = @('SharedContentUiOptionButtonState', 'SharedContentUiTextAndHeaderState')
    SharedDungeonRoomCoreAndSettings = @('SharedDungeonRoomCoreState', 'SharedDungeonRoomSettingsState')
    SharedDungeonHallCoreAndSettings = @('SharedDungeonHallCoreState', 'SharedDungeonHallSettingsState')
}
$expectedThirdLevelIds = @($expectedThirdLevelPeerMap.Values | ForEach-Object { $_ })
Assert-Equal -Actual $expectedThirdLevelIds.Count -Expected 10 -Message 'Expected third-level peer boundary count'

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
$expectedFourthLevelTotals = @{
    TileObjectDefinitionPlacementAndStyleState = 94; SharedTimeLoggerRenderMetricsState = 75
    SharedDropRuleSelectionAndChainState = 66; SharedBiomeDungeonAndTerrainState = 59
    SharedFishingDropCatalogState = 58; MainCageBirdAndTerrestrialAnimationState = 57
    UiItemSortingLayerCatalogState = 54; SharedSceneScanAndZoneState = 51
    SharedCreativePowerDefinitionState = 44; SharedDungeonStyleEntryDefinitions = 39
    SharedSceneAggregateAndDecorationState = 39; AudioLegacySoundInstanceState = 37
    SharedDungeonRoomShapeVariants = 37; AudioLegacySoundCatalogState = 36
    MapEncodingCatalogAndIoState = 36; MainTileBehaviorMetadata = 35
    UiItemSlotEquipmentAndCreativeContexts = 32; SharedItemStaticEconomyAndTimingRules = 30
    SharedPopupTextState = 30; MainBackgroundLayerState = 29
    SharedShaderFamilyCatalogState = 29; SharedDungeonStyleConstantQueries = 28
    SharedTilePlacementGeometryModules = 28; SharedWorldItemPayloadState = 28
    NetworkRemoteClientState = 27; NetworkSessionCoordinatorState = 27
    ItemEmergencyStackingState = 27; BestiaryInfoElementsAndProviders = 27
    EntityAuthoritativeState = 27; SharedCreativePowerIconCatalogState = 26
}
$expectedFifthLevelTotals = @{
    SharedTimeLoggerPhaseMetricsState = 66
    TileObjectStyleAndDrawState = 56
    SharedFishingConditionCatalogState = 50
    SharedDropRuleSelectionAndConditionState = 45
    SharedDungeonControlAndTrapState = 43
    SharedSceneZoneDefinitionState = 38
    TileObjectPlacementRuleState = 38
    MainCageTerrestrialCritterAnimationState = 37
    SharedAudioLegacySoundCatalogInstanceState = 35
    SharedAudioLegacySoundDefinitionCatalogState = 34
    MapEncodingHeaderCatalogState = 32
    UiItemSlotCreativeCraftingAndUtilityContexts = 30
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
$expectedFourthLevelIds = @($expectedFourthLevelPeerMap.Values | ForEach-Object { $_ })
Assert-Equal -Actual $expectedFourthLevelPeerMap.Count -Expected 30 -Message 'Expected fourth-level retired peer count'
Assert-Equal -Actual $expectedFourthLevelIds.Count -Expected 60 -Message 'Expected fourth-level child boundary count'
Assert-Equal -Actual $expectedFifthLevelPeerMap.Count -Expected 12 -Message 'Expected fifth-level retired peer count'
Assert-Equal -Actual (@($expectedFifthLevelPeerMap.Values | ForEach-Object { $_ })).Count -Expected 24 -Message 'Expected fifth-level child boundary count'
Assert-Equal -Actual $expectedSixthLevelPeerMap.Count -Expected 13 -Message 'Expected sixth-level retired peer count'
Assert-Equal -Actual (@($expectedSixthLevelPeerMap.Values | ForEach-Object { $_ })).Count -Expected 26 -Message 'Expected sixth-level child boundary count'

foreach ($id in $expectedSplitIds) {
    if ($secondLevelBaselineIds -contains $id -or $expectedFourthLevelPeerMap.Contains($id) -or $expectedSixthLevelPeerMap.Contains($id)) {
        continue
    }
    if (-not $fineIds.Contains($id)) {
        throw "Expected split fine subsystem is absent: $id"
    }
}

foreach ($id in $expectedSecondLevelChildIds) {
    if ($expectedThirdLevelPeerMap.Contains($id) -or $expectedFourthLevelPeerMap.Contains($id) -or $expectedSixthLevelPeerMap.Contains($id)) {
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
    $activeDirectChildren = 0
    foreach ($child in @($entry.Value)) {
        if ($expectedFourthLevelPeerMap.Contains($child)) {
            if ($fineIds.Contains($child)) {
                throw "Fourth-level retired peer is still active: $child"
            }
            continue
        }
        $activeDirectChildren++
        if (-not $fineIds.Contains($child)) {
            throw "Expected third-level peer fine subsystem is absent: $child"
        }
        if (-not $fineToPreviousPeer.ContainsKey($child)) {
            throw "Third-level peer fine subsystem has no immediate-peer mapping: $child"
        }
        Assert-Equal -Actual $fineToPreviousPeer[$child] -Expected $entry.Key -Message "Immediate peer for $child"
    }
    $peerCount = @($fineToPreviousPeer.GetEnumerator() | Where-Object Value -eq $entry.Key).Count
    Assert-Equal -Actual $peerCount -Expected $activeDirectChildren -Message "Third-level direct peer count for $($entry.Key)"
}

foreach ($entry in $expectedFourthLevelPeerMap.GetEnumerator()) {
    if ($fineIds.Contains($entry.Key)) {
        throw "Fourth-level retired peer is still active: $($entry.Key)"
    }
    $childTotal = 0
    $activeDirectChildren = 0
    foreach ($child in @($entry.Value)) {
        if ($expectedFifthLevelPeerMap.Contains($child) -or $expectedSixthLevelPeerMap.Contains($child)) {
            if ($fineIds.Contains($child)) {
                throw "Nested retired peer is still active: $child"
            }
            continue
        }
        if (-not $fineIds.Contains($child)) {
            throw "Expected fourth-level child fine subsystem is absent: $child"
        }
        if (-not $fineToPreviousPeer.ContainsKey($child)) {
            throw "Fourth-level child fine subsystem has no immediate-peer mapping: $child"
        }
        Assert-Equal -Actual $fineToPreviousPeer[$child] -Expected $entry.Key -Message "Immediate peer for $child"
        if (-not $fineStats.ContainsKey($child)) {
            throw "Fourth-level child has no member statistics: $child"
        }
        if ($fineStats[$child].Total -le 0) {
            throw "Fourth-level child is empty: $child"
        }
        $activeDirectChildren++
        $childTotal += $fineStats[$child].Total
    }
    $retiredNestedTotal = 0
    foreach ($nestedPeer in @($entry.Value | Where-Object { $expectedFifthLevelPeerMap.Contains($_) })) {
        $retiredNestedTotal += $expectedFifthLevelTotals[$nestedPeer]
    }
    foreach ($nestedPeer in @($entry.Value | Where-Object { $expectedSixthLevelPeerMap.Contains($_) })) {
        foreach ($nestedChild in $expectedSixthLevelPeerMap[$nestedPeer]) {
            $retiredNestedTotal += $expectedSixthLevelStats[$nestedChild].Total
        }
    }
    Assert-Equal -Actual ($childTotal + $retiredNestedTotal) -Expected $expectedFourthLevelTotals[$entry.Key] -Message "Fourth-level member rollup for $($entry.Key)"
    $peerCount = @($fineToPreviousPeer.GetEnumerator() | Where-Object Value -eq $entry.Key).Count
    Assert-Equal -Actual $peerCount -Expected $activeDirectChildren -Message "Fourth-level direct peer count for $($entry.Key)"
}

foreach ($id in $expectedFourthLevelIds) {
    if ($expectedFifthLevelPeerMap.Contains($id) -or $expectedSixthLevelPeerMap.Contains($id)) {
        if ($fineIds.Contains($id)) {
            throw "Nested retired peer is still active: $id"
        }
        continue
    }
    if (-not $fineIds.Contains($id)) {
        throw "Expected fourth-level child fine subsystem is absent: $id"
    }
}

foreach ($entry in $expectedFifthLevelPeerMap.GetEnumerator()) {
    if ($fineIds.Contains($entry.Key)) {
        throw "Fifth-level retired peer is still active: $($entry.Key)"
    }
    $childTotal = 0
    $nestedRetiredChildren = @($entry.Value | Where-Object { $expectedSixthLevelPeerMap.Contains($_) })
    $activeDirectChildren = @($fineToPreviousPeer.GetEnumerator() | Where-Object Value -eq $entry.Key).Count
    Assert-Equal -Actual $activeDirectChildren -Expected (2 - $nestedRetiredChildren.Count) -Message "Fifth-level peer '$($entry.Key)' direct child count"
    foreach ($child in @($entry.Value)) {
        if ($expectedSixthLevelPeerMap.Contains($child)) {
            if ($fineIds.Contains($child)) {
                throw "Sixth-level retired peer is still active: $child"
            }
            continue
        }
        if (-not $fineIds.Contains($child)) {
            throw "Expected fifth-level child fine subsystem is absent: $child"
        }
        if (-not $fineToPreviousPeer.ContainsKey($child)) {
            throw "Fifth-level child fine subsystem has no immediate-peer mapping: $child"
        }
        Assert-Equal -Actual $fineToPreviousPeer[$child] -Expected $entry.Key -Message "Immediate peer for $child"
        if (-not $fineStats.ContainsKey($child)) {
            throw "Fifth-level child has no member statistics: $child"
        }
        if ($fineStats[$child].Total -le 0) {
            throw "Fifth-level child is empty: $child"
        }
        $childTotal += $fineStats[$child].Total
    }
    $retiredNestedTotal = 0
    foreach ($nestedPeer in $nestedRetiredChildren) {
        foreach ($nestedChild in $expectedSixthLevelPeerMap[$nestedPeer]) {
            $retiredNestedTotal += $expectedSixthLevelStats[$nestedChild].Total
        }
    }
    Assert-Equal -Actual ($childTotal + $retiredNestedTotal) -Expected $expectedFifthLevelTotals[$entry.Key] -Message "Fifth-level member rollup for $($entry.Key)"
}

foreach ($entry in $expectedSixthLevelPeerMap.GetEnumerator()) {
    if ($fineIds.Contains($entry.Key)) {
        throw "Sixth-level retired peer is still active: $($entry.Key)"
    }
    $childTotal = 0
    foreach ($child in @($entry.Value)) {
        if (-not $fineIds.Contains($child)) {
            throw "Expected sixth-level child fine subsystem is absent: $child"
        }
        if (-not $fineToPreviousPeer.ContainsKey($child)) {
            throw "Sixth-level child fine subsystem has no immediate-peer mapping: $child"
        }
        Assert-Equal -Actual $fineToPreviousPeer[$child] -Expected $entry.Key -Message "Immediate peer for sixth-level child '$child'"
        if (-not $fineStats.ContainsKey($child)) {
            throw "Sixth-level child has no member statistics: $child"
        }
        if ($fineStats[$child].Total -le 0) {
            throw "Sixth-level child is empty: $child"
        }
        $expectedStats = $expectedSixthLevelStats[$child]
        Assert-Equal -Actual $fineStats[$child].Field -Expected $expectedStats.Field -Message "Field count for sixth-level child '$child'"
        Assert-Equal -Actual $fineStats[$child].Property -Expected $expectedStats.Property -Message "Property count for sixth-level child '$child'"
        Assert-Equal -Actual $fineStats[$child].Total -Expected $expectedStats.Total -Message "Total count for sixth-level child '$child'"
        $childTotal += $fineStats[$child].Total
    }
    Assert-Equal -Actual $childTotal -Expected ($expectedSixthLevelStats[$entry.Value[0]].Total + $expectedSixthLevelStats[$entry.Value[1]].Total) -Message "Sixth-level member rollup for $($entry.Key)"
    $peerCount = @($fineToPreviousPeer.GetEnumerator() | Where-Object Value -eq $entry.Key).Count
    Assert-Equal -Actual $peerCount -Expected 2 -Message "Sixth-level peer count for $($entry.Key)"
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
Write-Output 'PASS: 30 current top-ranked peers are retired into 60 fourth-level peers.'
Write-Output 'PASS: 12 current top-ranked peers are retired into 24 fifth-level peers.'
Write-Output 'PASS: 13 current top-ranked peers are retired into 26 sixth-level peers.'
Write-Output 'PASS: report contains exactly 349 active fine subsystems.'
