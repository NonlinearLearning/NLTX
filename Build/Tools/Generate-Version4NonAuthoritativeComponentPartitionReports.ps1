[CmdletBinding()]
param(
    [string]$SourceReportPath = 'docs/migration/ledgers/Version4非权威模拟系统字段属性逐成员源码声明-更细子系统拆分-去除ID类文件.md',
    [string]$OutputDirectory = 'docs/migration/ledgers/non-authoritative-component-partitions'
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

function Get-SourcePath {
    param([Parameter(Mandatory)][string]$Path)

    if ([System.IO.Path]::IsPathRooted($Path)) {
        return [System.IO.Path]::GetFullPath($Path)
    }

    return [System.IO.Path]::GetFullPath((Join-Path (Get-Location) $Path))
}

function Test-AnyPattern {
    param(
        [Parameter(Mandatory)][string]$Value,
        [Parameter(Mandatory)][string[]]$Patterns
    )

    foreach ($pattern in $Patterns) {
        if ($Value -match $pattern) {
            return $true
        }
    }

    return $false
}

$partitionDefinitions = @(
    [pscustomobject]@{ Id = '01'; Slug = 'world-session-runtime'; Title = '世界会话与运行时'; Scope = '启动、世界会话、帧时钟、规则控制、随机种子和运行时宿主状态。'; Focus = '确认会话状态、规则状态和派生查询的权威所有权、生命周期及系统顺序。' }
    [pscustomobject]@{ Id = '02'; Slug = 'world-environment-events'; Title = '世界环境与事件'; Scope = '天气、环境、季节、入侵、事件、天空和环境扫描状态。'; Focus = '将持续世界状态、事件命令和表现投影分开，确认事件触发与清理顺序。' }
    [pscustomobject]@{ Id = '03'; Slug = 'world-generation-dungeons'; Title = '世界生成与地牢'; Scope = '世界生成、生物群系、地形 pass、地牢定义、房间、走廊和布局规则。'; Focus = '区分 definition/catalog、生成过程状态、几何查询和结构变更命令。' }
    [pscustomobject]@{ Id = '04'; Slug = 'world-tiles-storage'; Title = '地块、液体与世界存储'; Scope = 'Tile、液体、地块框架、TileEntity、放置、绘制辅助和世界网格存储。'; Focus = '确认地块/液体权威状态与缓存、快照、查询和结构变更边界。' }
    [pscustomobject]@{ Id = '05'; Slug = 'spatial-motion-physics'; Title = '空间移动与物理'; Scope = '空间查询、碰撞、传送、实体几何、载具、座位、投射物引用和移动轨道。'; Focus = '确认移动状态、关系引用、碰撞查询和结构变更的访问方向。' }
    [pscustomobject]@{ Id = '06'; Slug = 'entity-lifecycle-attribution'; Title = '实体生命周期与归因'; Scope = '实体槽位、身份/运动基础状态、来源归因及世界物品生命周期关系。'; Focus = '分别建模实体引用、持久化 ID、网络 ID 和外部 ID，确认创建/销毁所有权。' }
    [pscustomobject]@{ Id = '07'; Slug = 'combat-status-effects'; Title = '战斗、伤害与状态效果'; Scope = '战斗目标、免疫、命中、伤害能力、标签效果、锁定和战斗文本。'; Focus = '分离权威战斗状态、纯资格查询、伤害命令和客户端表现。' }
    [pscustomobject]@{ Id = '08'; Slug = 'player-input-gameplay'; Title = '玩家输入与玩法'; Scope = '玩家输入、移动能力、交互、智能光标、装备能力、拾取和玩家拒绝状态。'; Focus = '确认输入意图、玩法状态、交互命令和玩家表现数据的方向。' }
    [pscustomobject]@{ Id = '09'; Slug = 'item-inventory-containers'; Title = '物品、库存与容器'; Scope = '物品身份、堆叠、转移、容器、TileEntity 库存和世界物品 payload。'; Focus = '按容器/库存访问模式拆分权威物品状态、转移命令和展示快照。' }
    [pscustomobject]@{ Id = '10'; Slug = 'economy-crafting-fishing-loot'; Title = '经济、配方、钓鱼与掉落'; Scope = '商店、价值规则、配方、钓鱼上下文、捕获、掉落解析和战利品。'; Focus = '将 definition、随机解析、结果命令和消费/经济事务分开并隔离随机性。' }
    [pscustomobject]@{ Id = '11'; Slug = 'npc-town-bestiary'; Title = 'NPC、城镇与图鉴'; Scope = 'NPC 个性、城镇房间、条件对话、图鉴条目、解锁和筛选。'; Focus = '确认 NPC/图鉴状态与纯筛选查询、文本投影及生命周期。' }
    [pscustomobject]@{ Id = '12'; Slug = 'content-definitions-catalogs'; Title = '内容定义与目录'; Scope = '内容样本、Set、颜色着色器、TileObject、物品变体、前缀、护甲和能力目录。'; Focus = '优先识别只读 definition/catalog 与运行时引用，不把目录字段当作实体状态。' }
    [pscustomobject]@{ Id = '13'; Slug = 'network-protocol-session'; Title = '网络协议与会话'; Scope = '网络会话、消息缓冲、socket、数据包、区段流、远端连接、聊天协议和网络投影。'; Focus = 'Adapter/Projection 单向消费权威状态，明确线程、限流、发送失败和重试边界。' }
    [pscustomobject]@{ Id = '14'; Slug = 'persistence-recovery-configuration'; Title = '持久化、恢复与配置'; Scope = '玩家/世界文件、恢复版本、Tile header、保存会话、文件平台和配置适配器。'; Focus = '区分持久化快照、运行时状态、文件协议适配和恢复事务，记录失败策略。' }
    [pscustomobject]@{ Id = '15'; Slug = 'external-platform-boundaries'; Title = '外部平台与协议边界'; Scope = 'Social API、IPC、Workshop/join、加密、NAT 和资源包外部适配。'; Focus = '外部第三方类型停留在 Adapter 边界，确认输入验证、错误、重试和取消。' }
    [pscustomobject]@{ Id = '16'; Slug = 'ui-core-interaction'; Title = 'UI 核心与交互'; Scope = 'UI 树、布局、事件、指针、内容界面、世界交互界面和进度容器。'; Focus = '保持 UI 表现单向消费领域状态，避免 UI 树或事件 payload 反向成为权威状态。' }
    [pscustomobject]@{ Id = '17'; Slug = 'ui-item-localization'; Title = 'UI 物品、排序与本地化'; Scope = 'ItemSlot、物品排序、提示、装备展示、内容 UI 辅助、成就展示和语言文本。'; Focus = '区分 UI 临时状态、目录、纯排序查询、本地化值和领域物品权威状态。' }
    [pscustomobject]@{ Id = '18'; Slug = 'map-camera-rendering'; Title = '地图、相机与绘制'; Scope = '地图编码/存储、地图层、相机矩阵、顶点、绘制批次、光照、shader 和场景聚合。'; Focus = '将渲染缓存/投影与世界权威地块状态分离，声明缓存失效条件和帧顺序。' }
    [pscustomobject]@{ Id = '19'; Slug = 'audio-particles-cinematics'; Title = '音频、粒子与演出'; Scope = '音频播放、旧音效实例、粒子、背景、天空表现、弹出文本和 cinematic timeline。'; Focus = '表现层只读消费状态，隔离音频/图形副作用、资源生命周期和客户端清理。' }
    [pscustomobject]@{ Id = '20'; Slug = 'diagnostics-tools-shared'; Title = '诊断、工具与共享机制'; Scope = '时间序列、TimeLogger、Debug、随机/缓冲工具、Creative Power、问题报告和通用元数据。'; Focus = '确认工具状态是否为投影/诊断快照，隔离时钟、随机、日志和其它外部副作用。' }
)

$runtimeRules = @(
    @{ Id = '01'; Patterns = @(
        '^MainFrameActivityState$', '^MainBootstrapAndWorldRules$', '^MainClockAndFrameScheduling$',
        '^MainFrameAndWorldRuleControl$', '^MainRandomAndSeedState$', '^MainSaveAndWorldSessionState$',
        '^MainDerivedWorldAndSessionQueries$', '^MainWindowAndShutdownState$', '^MainTimeSkipState$'
    ) }
    @{ Id = '02'; Patterns = @(
        '^MainCalendarWeatherState$', '^MainSlimeRainState$', '^MainWeatherAndAmbientState$',
        '^MainInvasionState$', '^MainSeasonalAndTitleState$'
    ) }
    @{ Id = '03'; Patterns = @('^MainMenuAndWorldGenerationState$', '^MainGraphicsAndGenerationState$') }
    @{ Id = '04'; Patterns = @(
        '^MainWorldGeometryAndCapacity$', '^MainCameraAndLiquidState$', '^MainTileFrameAndCatchMetadata$',
        '^MainWorldMapAndTileStore$', '^MainWallAndGlobalTileMetadata$', '^MainTileBehaviorAndInteractionMetadata$',
        '^MainTileLightingAndFrameMetadata$'
    ) }
    @{ Id = '05'; Patterns = @('^MainSpawnAndProjectileCaches$') }
    @{ Id = '06'; Patterns = @('^MainEntityPoolsAndWorldSlots$') }
    @{ Id = '08'; Patterns = @(
        '^MainScreenAndInputState$', '^MainPlayerAndSpawnState$', '^MainMenuAndInputSettings$',
        '^MainInputAndThreadScheduling$', '^MainInputAndEventFlags$', '^MainDerivedInputAndPresentationQueries$'
    ) }
    @{ Id = '10'; Patterns = @('^MainShopAndQuestSlots$') }
    @{ Id = '12'; Patterns = @('^MainContentAbilityTables$', '^MainContentCatalogAndSimulationServices$') }
    @{ Id = '13'; Patterns = @('^MainRecentServerAndMapState$', '^MainNetworkSessionState$') }
    @{ Id = '14'; Patterns = @('^MainSaveFavoritesAndSessionRefs$', '^MainWorldPersistenceAndMetadata$') }
    @{ Id = '18'; Patterns = @('^MainCameraAndUiScaleState$', '^MainCameraAndVisualOffsets$', '^MainSceneMetricsState$') }
    @{ Id = '19'; Patterns = @(
        '^MainParticlePools$', '^MainAmbientEffectsAndChatState$', '^MainNpcFrameState$',
        '^MainCageAquaticAndAmphibianAnimationState$', '^MainCageBirdAnimationState$',
        '^MainCageMammalAndReptileAnimationState$', '^MainCageInsectAndSmallCritterAnimationState$',
        '^MainBackgroundLayerCatalogState$', '^MainBackgroundParallaxAndStyleState$'
    ) }
    @{ Id = '20'; Patterns = @('^MainDiagnosticsAndSimulationRates$', '^MainTickAndDiagnosticState$') }
    @{ Id = '15'; Patterns = @('^MainPlatformExecutionAdapter$') }
)

$sharedRules = @(
    @{ Id = '01'; Patterns = @('^SharedDifficultyAndRuleMetadata$', '^SharedStartupAndRuntimeHostState$') }
    @{ Id = '01'; Patterns = @('^WorldSeedAndExploitRules$') }
    @{ Id = '02'; Patterns = @(
        '^SharedWorldEventPresentationState$', '^SharedInvasionAndBossTracking$', '^SharedLightningGenerationState$',
        '^SharedWaterfallState$', '^SharedAmbientSkyCatalogState$', '^SharedAmbientSpawnAndWindState$',
        '^SharedCelebrationAndLanternEvents$', '^SharedRitualAndStormEvents$', '^SharedSceneWeatherAndEventZoneState$',
        '^SharedInvasionDamageTrackingState$', '^SharedInvasionWaveAndArenaState$', '^WorldEnvironmentScanHelpers$'
    ) }
    @{ Id = '03'; Patterns = @(
        '^SharedWorldGenerationSupport$', '^SharedBiomeCaveHouseAndStructureState$', '^SharedBiomeTerrainPassState$',
        '^SharedSceneBiomeZoneDefinitionState$', '^WorldSpawnConfigurationState$', '^SharedDungeon.*$'
    ) }
    @{ Id = '04'; Patterns = @(
        '^SharedTileAnchorAndReachQueries$', '^SharedTileFramingAndSignState$', '^SharedTileSnapshots$',
        '^SharedTilePlacementAnchorAndHookModules$', '^SharedTileObjectPreviewState$',
        '^SharedTileObjectPlacementValueState$', '^SharedTilePlacementCoordinateAndDrawModules$',
        '^SharedTilePlacementBaseAndStyleModules$', '^SharedTilePaintRenderTargetState$',
        '^SharedTilePaintVariationAndColorState$', '^TileEntityRegistryAndBaseState$',
        '^TileEntityDisplayAndInventoryState$', '^TileEntityAnchorAndSensorState$', '^TileEntityWorldInteractionState$'
    ) }
    @{ Id = '05'; Patterns = @(
        '^SharedTeleportAndPortalSupport$', '^SharedPhysicsCollisionQueries$',
        '^SharedGeneralTeleportAndInterceptionUtilities$', '^EntityBoundsAndFluidState$',
        '^MinecartCustomizationState$', '^MinecartMotionAndTrackState$', '^MinecartDecorationAndSwitchState$',
        '^EnvironmentDamageAndSeatState$', '^TrackedProjectileReferenceState$'
    ) }
    @{ Id = '06'; Patterns = @('^SharedEntitySourceAndAttribution$', '^EntityIdentityAndMotionState$') }
    @{ Id = '07'; Patterns = @(
        '^SharedCombatTargetingAndImmunity$', '^SharedHitTileTracking$',
        '^SharedItemCombatAndDamageCapabilityState$', '^SharedCombatTextState$', '^ItemTagEffectState$',
        '^LockOnTargetingState$'
    ) }
    @{ Id = '08'; Patterns = @(
        '^SharedGolfState$', '^SharedSmartInteractionQueries$', '^SharedControlFocusHelpers$',
        '^InputProfilesAndConfiguration$', '^InputTriggerState$', '^PlayerInputRuntimeState$',
        '^DoorOpeningInteractionState$', '^SmartCursorInteractionState$', '^PressurePlateInteractionState$',
        '^CursorAndChestInteractionState$', '^PlayerMovementCapabilityState$', '^PlayerIntentAndInteractionState$',
        '^PlayerItemPickupAndRespawnState$', '^PlayerPreviewAndRejectionState$', '^EquipmentLoadoutState$',
        '^SharedCreativePowerRuntimeManager$', '^SharedCreativeUnlockProgress$', '^SharedCreativePowerContracts$',
        '^SharedCreativePerPlayerPowerState$', '^SharedCreativeSharedPowerState$'
    ) }
    @{ Id = '09'; Patterns = @(
        '^SharedItemIdentityAndStackState$', '^SharedItemProgressionAndWorldInteractionState$',
        '^SharedItemBuffMountAndConsumableEffects$', '^SharedItemDerivedQueries$', '^SharedWorldItemLifecycleState$',
        '^SharedWorldItemIdentityAndEconomyPayloadState$', '^SharedWorldItemUseAndPresentationPayloadState$',
        '^SharedItemEmergencyStackingPolicyState$', '^SharedItemEmergencyStackingTransferState$',
        '^SharedItemEquipmentSlotState$', '^SharedItemToolPlacementCapabilityState$', '^ItemQuickStackingState$',
        '^ItemTransferSettings$'
    ) }
    @{ Id = '10'; Patterns = @(
        '^SharedFishingCatchEffects$', '^SharedLootSimulation$', '^SharedDropSourceAttribution$',
        '^SharedItemCommerceState$', '^SharedItemCommerceAndPricing$', '^SharedItemEconomyAndValueRules$',
        '^SharedItemUseTimingAndStackRules$', '^SharedItemUseTimingAndConsumptionState$',
        '^SharedDropRule.*$', '^SharedFishing.*$', '^FishingAttemptState$', '^PlayerFishingConditionState$',
        '^CraftingRequestState$', '^RecipeDefinitionState$', '^RecipeGroupCatalogState$'
    ) }
    @{ Id = '11'; Patterns = @(
        '^SharedConditionalDialogueSupport$', '^SharedTownRoomState$', '^SharedNpcPersonalityCatalog$',
        '^BestiaryCatalogAndEntries$', '^BestiaryUnlockTracking$', '^BestiaryFiltersAndSorting$',
        '^SharedBestiaryInfoElementState$', '^SharedBestiaryCollectionProviderState$'
    ) }
    @{ Id = '12'; Patterns = @('^SharedContentValidation$', '^SharedContentPresentationCatalog$', '^SharedItemStaticCapabilityRules$') }
    @{ Id = '12'; Patterns = @('^ItemVariantDefinitions$', '^ArmorSetBonusDefinitions$', '^ArmorSetBonusCatalog$', '^WingStatsDefinition$', '^LegacyItemPrefixCatalog$') }
    @{ Id = '13'; Patterns = @(
        '^SharedNetworkSocketTransport$', '^SharedNetworkPacketPrimitives$', '^SharedContentNetworkModules$',
        '^SharedNetworkSectionProjections$', '^SharedChatAndCommandProtocol$', '^SharedChatSnippetPresentationState$',
        '^SharedChatMonitorAndCommandState$'
    ) }
    @{ Id = '14'; Patterns = @('^SharedSaveAndConfigurationAdapters$', '^SharedGeneralFilePlatformUtilities$') }
    @{ Id = '15'; Patterns = @('^SharedResourcePackAdapters$') }
    @{ Id = '16'; Patterns = @(
        '^SharedContentUiScreens$', '^SharedContentUiCurrency$', '^SharedWorldInteractionUi$',
        '^SharedContentUiContainersAndProgress$', '^SharedContentUiOptionButtonState$',
        '^SharedContentUiTextAndHeaderState$', '^UiLayoutPrimitives$', '^UiEventPayloads$', '^UiInputPointerState$',
        '^UiElementLayoutAndDimensionsState$', '^UiElementInteractionAndLifecycleState$'
    ) }
    @{ Id = '17'; Patterns = @(
        '^SharedAchievementProgressSupport$', '^SharedCreativePowerUiLayoutState$',
        '^SharedCreativePowerIconLocationCatalogState$', '^SharedCreativePowerIconLayoutState$',
        '^SharedItemAppearanceAndTooltipState$', '^LocalizationCultureAndLanguageState$',
        '^LocalizedTextValueState$', '^LegacyLanguageCatalogState$'
    ) }
    @{ Id = '18'; Patterns = @(
        '^SharedSceneScanSettings$', '^SharedSceneVisualState$', '^SharedCaptureAndCameraSupport$',
        '^SharedSceneTileAggregateState$', '^SharedSceneScanAccumulatorState$', '^SharedSceneZoneGeometryAndThresholdState$', '^LightMapCacheState$',
        '^TileLightScannerState$', '^DrawAnimationAndFrameState$', '^DrawCommandAndBatchState$',
        '^WorldDrawingAuxiliaryState$', '^SharedLightingCoordinatorState$', '^SharedLegacyLightingState$',
        '^SharedShaderBaseParameterState$', '^SharedShaderFamilyDataState$', '^SharedShaderRegistryAndLookupState$',
        '^EntityShadowPresentationState$'
    ) }
    @{ Id = '19'; Patterns = @(
        '^SharedAudioAndSoundData$', '^SharedEffectAndSkyPresentation$', '^SharedParticlePresentation$',
        '^SharedBackgroundPresentationDefinitions$', '^SharedStarParticleState$', '^SharedCloudAndRainParticleState$',
        '^SharedDustParticleState$', '^SharedGoreEffectState$', '^SharedSceneDecorationAndAudioState$',
        '^SharedPopupTextContentAndContextState$', '^SharedPopupTextRenderLifecycleState$',
        '^SharedAudioLegacy.*$'
    ) }
    @{ Id = '20'; Patterns = @(
        '^SharedCallTrackingDiagnostics$', '^SharedTimeSeriesDataSeriesState$', '^SharedTimeSeriesEntryState$',
        '^SharedTimeSeriesFormattingState$', '^SharedGeneralDiagnosticsUtilities$', '^DebugCommandProtocol$',
        '^DebugRuntimeOptions$', '^DebugFrameTelemetry$', '^DebugBuildStatus$', '^SharedTimeLogger.*$',
        '^SharedIssueReportCatalogState$', '^SharedRandomSources$', '^SharedBufferAndCollectionPools$',
        '^SharedRangeAndBitUtilities$', '^SharedGeneralDelegateAndMetadataUtilities$',
        '^SharedGeneralRandomAndBufferUtilities$'
    ) }
)

$clientRules = @(
    @{ Id = '16'; Patterns = @(
        '^UiLayoutPrimitives$', '^UiEventPayloads$', '^UiInputPointerState$',
        '^UiElementLayoutAndDimensionsState$', '^UiElementInteractionAndLifecycleState$'
    ) }
    @{ Id = '17'; Patterns = @(
        '^AchievementPresentationState$', '^UiItemSlotTransferState$', '^UiItemTooltipState$',
        '^UiItemSortingExecutionState$', '^UiItemSlotStorageAndCraftingContexts$',
        '^UiItemSlotEquipmentAndDisplayContexts$', '^UiItemSlotPulseAndHighlightState$',
        '^UiItemSlotDisplayAndInteractionState$', '^UiItemSortingRegistryAndRankingState$',
        '^UiItemSortingConsumableAndMiscCatalogState$', '^UiItemSlotCreativeAndCraftingContextState$',
        '^UiItemSlotHotbarDisplayAndUtilityContextState$', '^UiItemSortingWeaponAndToolCatalogState$',
        '^UiItemSortingArmorAndAccessoryCatalogState$'
    ) }
    @{ Id = '18'; Patterns = @(
        '^MapOverlayAndLayerPresentation$', '^WorldMapState$', '^MapTileUpdateQueueState$', '^MapTileCellState$',
        '^MapIoRuntimeState$', '^MapEncodingHeaderBitCatalogState$', '^MapEncodingOptionLimitState$',
        '^CameraMatrixAndViewportState$', '^VertexStripAndColorState$'
    ) }
    @{ Id = '19'; Patterns = @(
        '^AudioDefinitionAndTrackState$', '^AudioActiveSoundState$', '^CinematicTimelineState$',
        '^AudioPlaybackCoordinatorState$', '^AudioTrackedSoundState$', '^SharedAudioLegacy.*$'
    ) }
)

function Get-PartitionId {
    param(
        [Parameter(Mandatory)][string]$Parent,
        [Parameter(Mandatory)][string]$Fine
    )

    switch ($Parent) {
        'PersistenceAndRecovery' { return '14' }
        'NetworkSessionAndSectionStreaming' { return '13' }
        'WorldStorage' {
            if ($Fine -eq 'ChestContainerStorage') { return '09' }
            return '04'
        }
        'ContentCatalog' { return '12' }
        'ExternalBoundaries' { return '15' }
        'ExternalDependencyOrGenerated' { return '15' }
        'RuntimeComposition' {
            foreach ($rule in $runtimeRules) {
                if (Test-AnyPattern -Value $Fine -Patterns $rule.Patterns) {
                    return $rule.Id
                }
            }
            throw "Runtime fine subsystem has no partition rule: $Fine"
        }
        'SharedRuntimeMechanisms' {
            foreach ($rule in $sharedRules) {
                if (Test-AnyPattern -Value $Fine -Patterns $rule.Patterns) {
                    return $rule.Id
                }
            }
            throw "Shared fine subsystem has no partition rule: $Fine"
        }
        'ClientPresentationAndTools' {
            foreach ($rule in $clientRules) {
                if (Test-AnyPattern -Value $Fine -Patterns $rule.Patterns) {
                    return $rule.Id
                }
            }
            throw "Client fine subsystem has no partition rule: $Fine"
        }
        default { throw "Unexpected parent subsystem for fine subsystem '$Fine': $Parent" }
    }
}

$sourcePath = Get-SourcePath -Path $SourceReportPath
$outputPath = Get-SourcePath -Path $OutputDirectory
if (-not (Test-Path -LiteralPath $sourcePath -PathType Leaf)) {
    throw "Source report does not exist: $sourcePath"
}

$sourceLines = [System.IO.File]::ReadAllLines($sourcePath)
$fineBlocks = [System.Collections.Generic.List[object]]::new()
$currentParent = ''
$currentBlock = $null

for ($index = 0; $index -lt $sourceLines.Count; $index++) {
    $line = $sourceLines[$index]
    if ($line -match '^### 4\.\d+ 父级子系统：`([^`]+)`') {
        if ($null -ne $currentBlock) {
            $currentBlock.End = $index - 1
            $fineBlocks.Add($currentBlock)
            $currentBlock = $null
        }
        $currentParent = $Matches[1]
        continue
    }

    if ($line -match '^#### (4\.\d+\.\d+) 细分子系统：`([^`]+)`') {
        if ($null -ne $currentBlock) {
            $currentBlock.End = $index - 1
            $fineBlocks.Add($currentBlock)
        }
        $currentBlock = [pscustomobject]@{
            Parent = $currentParent
            OriginalSection = $Matches[1]
            Fine = $Matches[2]
            Start = $index
            End = $null
            PartitionId = $null
            Lines = @()
            Members = @()
            Field = 0
            Property = 0
            Total = 0
            Role = ''
            Baseline = ''
        }
    }

    if ($line -match '^## 5\. 追溯与验收$' -and $null -ne $currentBlock) {
        $currentBlock.End = $index - 1
        $fineBlocks.Add($currentBlock)
        $currentBlock = $null
    }
}

if ($null -ne $currentBlock) {
    $currentBlock.End = $sourceLines.Count - 1
    $fineBlocks.Add($currentBlock)
}

if ($fineBlocks.Count -ne 349) {
    throw "Expected 349 fine subsystem blocks, found $($fineBlocks.Count)."
}

foreach ($block in $fineBlocks) {
    $block.PartitionId = Get-PartitionId -Parent $block.Parent -Fine $block.Fine
    $block.Lines = @($sourceLines[$block.Start..$block.End])
    $memberMatches = @($block.Lines | ForEach-Object {
        if ($_ -match '^\|\s*(\d+)\s*\|\s*(field|property)\s*\|') {
            [pscustomobject]@{ Seq = [int]$Matches[1]; Kind = $Matches[2]; Line = $_ }
        }
    })
    $block.Members = $memberMatches
    $block.Field = @($memberMatches | Where-Object Kind -eq 'field').Count
    $block.Property = @($memberMatches | Where-Object Kind -eq 'property').Count
    $block.Total = $memberMatches.Count
    $roleLine = $block.Lines | Where-Object { $_ -match '^- 边界角色：' } | Select-Object -First 1
    if ($roleLine -match '^- 边界角色：`([^`]+)`') {
        $block.Role = $Matches[1]
    } else {
        throw "Fine subsystem has no boundary role: $($block.Fine)"
    }
    $baselineLine = $block.Lines | Where-Object { $_ -match '^- 上一级基线细分子系统：' } | Select-Object -First 1
    if ($baselineLine -match '^- 上一级基线细分子系统：`([^`]+)`') {
        $block.Baseline = $Matches[1]
    } else {
        throw "Fine subsystem has no baseline label: $($block.Fine)"
    }
}

$allMembers = @($fineBlocks | ForEach-Object { $_.Members })
$sourceSequences = @($allMembers | ForEach-Object { $_.Seq })
if ($sourceSequences.Count -ne 4542) {
    throw "Expected 4,542 member rows in fine report, found $($sourceSequences.Count)."
}
$uniqueSequences = @($sourceSequences | Sort-Object -Unique)
if ($uniqueSequences.Count -ne 4542 -or $uniqueSequences[0] -ne 1 -or $uniqueSequences[-1] -ne 4542) {
    throw 'Fine report source sequence coverage is not exactly 1..4,542.'
}
for ($seq = 1; $seq -le 4542; $seq++) {
    if ($uniqueSequences[$seq - 1] -ne $seq) {
        throw "Fine report source sequence gap or reorder at expected sequence $seq."
    }
}

$sourceHash = (Get-FileHash -LiteralPath $sourcePath -Algorithm SHA256).Hash.ToLowerInvariant()
$sourceDisplayPath = $SourceReportPath.Replace('\', '/')
$definitionById = @{}
foreach ($definition in $partitionDefinitions) {
    $definitionById[$definition.Id] = $definition
}

$groupsByPartition = @{}
foreach ($definition in $partitionDefinitions) {
    $groupsByPartition[$definition.Id] = @($fineBlocks | Where-Object PartitionId -eq $definition.Id)
}

if (Test-Path -LiteralPath $outputPath -PathType Leaf) {
    throw "Output directory path is a file: $outputPath"
}
if (-not (Test-Path -LiteralPath $outputPath -PathType Container)) {
    New-Item -ItemType Directory -Path $outputPath | Out-Null
}

$expectedFileNames = @($partitionDefinitions | ForEach-Object { "$($_.Id)-$($_.Slug).md" })
$unexpectedFiles = @(Get-ChildItem -LiteralPath $outputPath -File | Where-Object { $expectedFileNames -notcontains $_.Name })
if ($unexpectedFiles.Count -gt 0) {
    throw "Output directory contains unexpected files: $($unexpectedFiles.Name -join ', ')"
}

$utf8NoBom = [System.Text.UTF8Encoding]::new($false)
$manifestRows = @($partitionDefinitions | ForEach-Object {
    $definition = $_
    $blocks = $groupsByPartition[$definition.Id]
    $members = @($blocks | ForEach-Object { $_.Members })
    $fields = @($members | Where-Object Kind -eq 'field').Count
    $properties = @($members | Where-Object Kind -eq 'property').Count
    "| $($definition.Id) | ``$($definition.Title)`` | $($blocks.Count) | $fields | $properties | $($members.Count) | [$($definition.Id)-$($definition.Slug).md]($($definition.Id)-$($definition.Slug).md) |"
})

foreach ($definition in $partitionDefinitions) {
    $blocks = $groupsByPartition[$definition.Id]
    if ($blocks.Count -eq 0) {
        throw "Partition $($definition.Id) has no fine subsystem blocks."
    }

    $members = @($blocks | ForEach-Object { $_.Members })
    $fields = @($members | Where-Object Kind -eq 'field').Count
    $properties = @($members | Where-Object Kind -eq 'property').Count
    $minSeq = ($members | Measure-Object -Property Seq -Minimum).Minimum
    $maxSeq = ($members | Measure-Object -Property Seq -Maximum).Maximum
    $parentRows = @($blocks | Group-Object Parent | ForEach-Object {
        $parentBlocks = @($_.Group)
        $parentMembers = @($parentBlocks | ForEach-Object { $_.Members })
        $parentFields = @($parentMembers | Where-Object Kind -eq 'field').Count
        $parentProperties = @($parentMembers | Where-Object Kind -eq 'property').Count
        "| ``$($_.Name)`` | $($parentBlocks.Count) | $parentFields | $parentProperties | $($parentMembers.Count) |"
    })
    $fineRows = @($blocks | ForEach-Object {
        "| ``$($_.OriginalSection)`` | ``$($_.Parent)`` | ``$($_.Fine)`` | $($_.Role) | $($_.Field) | $($_.Property) | $($_.Total) | 待按成员访问模式拆分 |"
    })

    $lines = [System.Collections.Generic.List[string]]::new()
    $lines.Add("# Version4 非权威组件拆分分区 $($definition.Id)/20：$($definition.Title)")
    $lines.Add('')
    $lines.Add(('> 来源报告：`{0}`' -f $sourceDisplayPath))
    $lines.Add(('> 来源报告 SHA-256：`{0}`' -f $sourceHash))
    $lines.Add("> 本文件是并行组件拆分工作包，不是新的权威成员清单；成员声明事实直接保留来源报告原文。")
    $lines.Add('')
    $lines.Add('## 1. 分区定位与评估')
    $lines.Add('')
    $lines.Add("- 分区范围：$($definition.Scope)")
    $lines.Add("- 本分区组件化重点：$($definition.Focus)")
    $lines.Add("- 本分区包含 $($blocks.Count) 个完整细分子系统、$($members.Count) 条成员记录（字段 $fields、属性 $properties）。来源序号覆盖区间 ``$minSeq..$maxSeq``，区间可能与其他分区交错，不以序号定义领域边界。")
    $lines.Add('- 组件边界判定：细分子系统只是并行处理的最小库存边界，不等价于一个组件。必须继续按共同读者/写者、变更原因、生命周期、权威性和副作用分为 Component、System、Query、Command、Adapter 或 Projection。')
    $lines.Add('- 证据状态：成员声明和统计已由来源报告校验；逐成员读者、写者、初始化/清理路径、持久化/网络语义和系统顺序仍需本分区会话补证，不得从名称或数量推断。')
    if ($definition.Id -eq '01') {
        $lines.Add('- 总体评估：来源报告作为源码库存导航是可用的。现有验证已确认 `1..4542` 序号闭合、`4017/525` 字段/属性计数、349 个细分组和父级汇总一致；但它尚未达到可直接生成 ECS 组件的证据门槛，尤其缺少逐成员 owner/writer/lifecycle/side-effect 闭合。')
        $lines.Add('- 已知风险：报告中存在混合语义组（例如图形与生成、相机与液体、网络发布与声音、场景装饰与音频），并行拆分时必须允许一个细分组进一步分成多个组件或非组件角色。')
    }
    $lines.Add('')
    $lines.Add('## 2. 分区统计')
    $lines.Add('')
    $lines.Add('| 父级子系统 | 细分子系统数 | 字段 | 属性 | 合计 |')
    $lines.Add('|---|---:|---:|---:|---:|')
    foreach ($row in $parentRows) { $lines.Add($row) }
    $lines.Add('')
    $lines.Add('| 原报告章节 | 父级 | 细分子系统 | 边界角色 | 字段 | 属性 | 合计 | 组件化处理 |')
    $lines.Add('|---|---|---|---|---:|---:|---:|---|')
    foreach ($row in $fineRows) { $lines.Add($row) }
    if ($definition.Id -eq '01') {
        $lines.Add('')
        $lines.Add('### 2.1 20 个并行分区总览')
        $lines.Add('')
        $lines.Add('| 分区 | 领域 | 细分组 | 字段 | 属性 | 合计 | 文件 |')
        $lines.Add('|---:|---|---:|---:|---:|---:|---|')
        foreach ($row in $manifestRows) { $lines.Add($row) }
        $lines.Add('')
        $lines.Add('零成员父级 `ContentLifecycleAndRegistration` 和 `IntentAndInteraction` 保留在来源报告的父级统计中，不生成伪造成员分区；对应边界说明分别放在内容目录分区和玩家输入与玩法分区。')
    }
    if ($definition.Id -eq '08') {
        $lines.Add('')
        $lines.Add('- 空成员父级说明：来源报告中的 `IntentAndInteraction` 为 0 条成员记录，本分区不新增成员，只保留该空边界供后续交互命令设计追踪。')
    }
    if ($definition.Id -eq '12') {
        $lines.Add('')
        $lines.Add('- 空成员父级说明：来源报告中的 `ContentLifecycleAndRegistration` 为 0 条成员记录，本分区不新增成员，只保留该空边界供后续注册生命周期设计追踪。')
    }
    $lines.Add('')
    $lines.Add('## 3. 并行组件拆分契约')
    $lines.Add('')
    $lines.Add('对本文件中的每个细分子系统和每条成员记录执行以下判断，并把结果写入对应的实现阶段设计/审计记录：')
    $lines.Add('')
    $lines.Add('- 盘点声明类型、可见性、读者、写者、创建/更新/清理/持久化生命周期、访问频率和副作用；缺证据标记 `missing`，不可用命名猜测补齐。')
    $lines.Add('- 标记权威状态、派生值、缓存、快照、兼容字段和表现数据；Component 只保存一个内聚概念的权威数据。')
    $lines.Add('- 需要行为转换时放入显式 System/Command；纯资格判断和派生计算放入 Query；外部协议、文件、平台和第三方类型放入 Adapter；网络/持久化/日志/UI 输出放入单向 Projection。')
    $lines.Add('- 分别建模实体引用、持久化 ID、网络 ID 和外部 ID；不得把 ID 类文件或外部对象直接塞入共享组件。')
    $lines.Add('- 记录 Interface、Implementation、Seam、Depth、Leverage、Locality，以及必要的系统先后约束、失败归属和重试策略。')
    $lines.Add('')
    $lines.Add('## 4. 逐成员源码声明')
    $lines.Add('')
    $lines.Add('以下细分子系统块保持来源报告的成员表、声明、路径、行列和来源序号；只调整 Markdown 标题层级并增加分区追踪元数据。')
    $lines.Add('')

    $localFineNumber = 0
    foreach ($block in $blocks) {
        $localFineNumber++
        $lines.Add(('### 4.{0} 细分子系统：`{1}`' -f $localFineNumber, $block.Fine))
        $lines.Add('')
        $lines.Add(('- 原报告章节：`{0}`' -f $block.OriginalSection))
        $lines.Add(('- 父级子系统：`{0}`' -f $block.Parent))
        $lines.Add(('- 分区工作包：`{0}` / `{1}`' -f $definition.Id, $definition.Title))
        foreach ($sourceLine in $block.Lines) {
            if ($sourceLine -match '^#### \d+\.\d+\.\d+ 细分子系统：') {
                continue
            }
            if ($sourceLine -match '^##### (.*)$') {
                $lines.Add("#### $($Matches[1])")
                continue
            }
            $lines.Add($sourceLine)
        }
        $lines.Add('')
    }

    $lines.Add('## 5. 追溯与验收')
    $lines.Add('')
    $lines.Add(('> 来源报告 SHA-256：`{0}`' -f $sourceHash))
    $lines.Add("- 本分区细分组数：$($blocks.Count)；成员数：$($members.Count)；字段：$fields；属性：$properties。")
    $lines.Add('- 预期不变量：本文件只出现完整细分子系统；不跨分区复制成员；所有成员的来源序号、原始声明、路径、行列和 C# 类型必须与来源报告一致。')
    $lines.Add('- 验证方式：重新读取 20 个文件，检查文件数、细分组唯一性、来源序号恰好覆盖 `1..4542`、字段/属性合计和逐成员行文本一致；本次分区生成不运行 `dotnet`，因为没有修改 C# 或项目文件。')
    $lines.Add('')
    $lines.Add('生成器：`Build/Tools/Generate-Version4NonAuthoritativeComponentPartitionReports.ps1`。')

    $targetFile = Join-Path $outputPath "$($definition.Id)-$($definition.Slug).md"
    [System.IO.File]::WriteAllText($targetFile, ($lines -join [Environment]::NewLine) + [Environment]::NewLine, $utf8NoBom)
}

$generatedFiles = @(Get-ChildItem -LiteralPath $outputPath -File | Where-Object { $expectedFileNames -contains $_.Name })
if ($generatedFiles.Count -ne $partitionDefinitions.Count) {
    throw "Expected $($partitionDefinitions.Count) generated partition files, found $($generatedFiles.Count)."
}

$outputMembers = [System.Collections.Generic.List[object]]::new()
$outputFines = [System.Collections.Generic.List[string]]::new()
foreach ($file in $generatedFiles) {
    foreach ($line in [System.IO.File]::ReadAllLines($file.FullName)) {
        if ($line -match '^### 4\.\d+ 细分子系统：`([^`]+)`') {
            $outputFines.Add($Matches[1])
        }
        if ($line -match '^\|\s*(\d+)\s*\|\s*(field|property)\s*\|') {
            $outputMembers.Add([pscustomobject]@{ Seq = [int]$Matches[1]; Kind = $Matches[2]; Line = $line })
        }
    }
}

if ($outputFines.Count -ne 349 -or @($outputFines | Sort-Object -Unique).Count -ne 349) {
    throw 'Generated partitions do not contain exactly 349 unique fine subsystems.'
}
if ($outputMembers.Count -ne 4542) {
    throw "Generated partitions contain $($outputMembers.Count) member rows instead of 4,542."
}
$outputSequences = @($outputMembers | Select-Object -ExpandProperty Seq | Sort-Object -Unique)
if ($outputSequences.Count -ne 4542 -or $outputSequences[0] -ne 1 -or $outputSequences[-1] -ne 4542) {
    throw 'Generated partition source sequence coverage is not exactly 1..4,542.'
}

$expectedFields = @($allMembers | Where-Object Kind -eq 'field').Count
$expectedProperties = @($allMembers | Where-Object Kind -eq 'property').Count
$actualFields = @($outputMembers | Where-Object Kind -eq 'field').Count
$actualProperties = @($outputMembers | Where-Object Kind -eq 'property').Count
if ($actualFields -ne $expectedFields -or $actualProperties -ne $expectedProperties) {
    throw "Generated partition field/property totals differ: expected $expectedFields/$expectedProperties, got $actualFields/$actualProperties."
}

Write-Output "PASS: generated $($generatedFiles.Count) partition Markdown files under $outputPath."
Write-Output 'PASS: 349 complete fine subsystems and 4,542 member rows are represented exactly once.'
Write-Output "PASS: fields=$actualFields, properties=$actualProperties, total=$($outputMembers.Count); source SHA-256=$sourceHash."
