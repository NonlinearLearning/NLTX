[CmdletBinding()]
param(
    [string]$InputPath = 'docs/migration/ledgers/Version4非权威模拟系统字段属性逐成员源码声明-去除ID类文件.md',
    [string]$OutputPath = 'docs/migration/ledgers/Version4非权威模拟系统字段属性逐成员源码声明-更细子系统拆分-去除ID类文件.md'
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

function Get-BaseFineSubsystemId {
    param([Parameter(Mandatory)][pscustomobject]$Row)

    $path = $Row.RelativePath

    switch ($Row.Parent) {
        'RuntimeComposition' {
            if ($Row.SourceLine -le 96) { return 'MainFrameActivityState' }
            if ($Row.SourceLine -le 117) { return 'MainPlatformExecutionAdapter' }
            if ($Row.SourceLine -ge 124 -and $Row.SourceLine -le 138) { return 'MainCameraAndUiScaleState' }
            if ($Row.SourceLine -le 178) { return 'MainBootstrapAndWorldRules' }
            if ($Row.SourceLine -le 198) { return 'MainSaveFavoritesAndSessionRefs' }
            if ($Row.SourceLine -le 229) { return 'MainClockAndFrameScheduling' }
            if ($Row.SourceLine -le 296) { return 'MainCameraAndVisualOffsets' }
            if ($Row.SourceLine -le 318) { return 'MainInputAndThreadScheduling' }
            if ($Row.SourceLine -le 390) { return 'MainBackgroundAndSeasonalState' }
            if ($Row.SourceLine -le 405) { return 'MainDiagnosticsAndSimulationRates' }
            if ($Row.SourceLine -le 428) { return 'MainContentAbilityTables' }
            if ($Row.SourceLine -le 464) { return 'MainRecentServerAndMapState' }
            if ($Row.SourceLine -le 493) { return 'MainGraphicsAndGenerationState' }
            if ($Row.SourceLine -le 512) { return 'MainFrameAndWorldRuleControl' }
            if ($Row.SourceLine -le 534) { return 'MainWorldGeometryAndCapacity' }
            if ($Row.SourceLine -le 550) { return 'MainSlimeRainState' }
            if ($Row.SourceLine -le 561) { return 'MainCameraAndLiquidState' }
            if ($Row.SourceLine -le 591) { return 'MainWorldPersistenceAndMetadata' }
            if ($Row.SourceLine -le 624) { return 'MainCalendarWeatherState' }
            if ($Row.SourceLine -le 638) { return 'MainSpawnAndProjectileCaches' }
            if ($Row.SourceLine -le 646) { return 'MainSceneMetricsState' }
            if ($Row.SourceLine -le 671) { return 'MainWeatherAndAmbientState' }
            if ($Row.SourceLine -le 686) { return 'MainRandomAndSeedState' }
            if ($Row.SourceLine -le 767) { return 'MainTileAndWallMetadata' }
            if ($Row.SourceLine -le 913) { return 'MainCageAnimationState' }
            if ($Row.SourceLine -le 925) { return 'MainTileFrameAndCatchMetadata' }
            if ($Row.SourceLine -le 928) { return 'MainWorldMapAndTileStore' }
            if ($Row.SourceLine -le 958) { return 'MainEntityPoolsAndWorldSlots' }
            if ($Row.SourceLine -le 978) { return 'MainScreenAndInputState' }
            if ($Row.SourceLine -le 989) { return 'MainPlayerAndSpawnState' }
            if ($Row.SourceLine -le 1012) { return 'MainShopAndQuestSlots' }
            if ($Row.SourceLine -le 1033) { return 'MainContentCatalogAndSimulationServices' }
            if ($Row.SourceLine -le 1057) { return 'MainSaveAndWorldSessionState' }
            if ($Row.SourceLine -le 1081) { return 'MainInvasionState' }
            if ($Row.SourceLine -le 1159) { return 'MainNpcFrameState' }
            if ($Row.SourceLine -le 1178) { return 'MainNetworkSessionState' }
            if ($Row.SourceLine -le 1188) { return 'MainMenuAndInputSettings' }
            if ($Row.SourceLine -le 1192) { return 'MainParticlePools' }
            if ($Row.SourceLine -le 1207) { return 'MainWindowAndShutdownState' }
            if ($Row.SourceLine -le 1215) { return 'MainTimeSkipState' }
            if ($Row.SourceLine -le 1254) { return 'MainAmbientEffectsAndChatState' }
            if ($Row.SourceLine -le 1262) { return 'MainTickAndDiagnosticState' }
            if ($Row.SourceLine -le 1279) { return 'MainMenuAndWorldGenerationState' }
            if ($Row.SourceLine -le 1296) { return 'MainInputAndEventFlags' }
            return 'MainDerivedPropertiesAndEvents'
        }
        'PersistenceAndRecovery' {
            if ($Row.Type -eq 'Terraria.IO.WorldFile.TilePacker') { return 'WorldFileTilePacking' }
            if ($Row.Type -eq 'Terraria.IO.WorldFile') { return 'WorldFileRecoveryIo' }
            if ($Row.Type -eq 'Terraria.IO.WorldFileData') { return 'WorldFileMetadataAndSession' }
            if ($Row.Type -eq 'Terraria.IO.PlayerFileData') { return 'PlayerFileMetadataAndSession' }
        }
        'NetworkSessionAndSectionStreaming' {
            if ($Row.Type -like 'Terraria.WorldSections*') { return 'SectionStreamingState' }
            if ($Row.Type -eq 'Terraria.MessageBuffer') { return 'NetworkMessageBufferAndDispatch' }
            if ($Row.Type -like 'Terraria.NetMessage*') { return 'NetworkPublicationAndSound' }
            if ($Row.Type -eq 'Terraria.Netplay.SetRemoteIPRequestInfo') { return 'NetworkRemoteIpRequestAdapter' }
            if ($Row.Type -eq 'Terraria.Netplay') { return 'NetworkSessionCoordinatorState' }
            if ($Row.Type -eq 'Terraria.RemoteClient') { return 'NetworkRemoteClientState' }
            if ($Row.Type -eq 'Terraria.RemoteServer') { return 'NetworkRemoteServerState' }
        }
        'ContentLifecycleAndRegistration' {
            return $null
        }
        'WorldStorage' {
            if ($Row.Type -eq 'Terraria.Tile') { return 'TileCellStorage' }
            if ($Row.Type -eq 'Terraria.Chest') { return 'ChestContainerStorage' }
        }
        'ContentCatalog' {
            if ($Row.Type -like 'Terraria.ObjectData.TileObjectData') { return 'TileObjectDefinitionCatalog' }
            if ($Row.Type -like 'Terraria.ID.ContentSamples*') { return 'ContentSamplesCatalog' }
            if ($Row.Type -eq 'Terraria.ID.SetFactory') { return 'ContentSetFactory' }
            if ($Row.Type -in @('Terraria.ID.Colors', 'Terraria.ID.DyeShaderIDs')) { return 'ColorAndShaderSetCatalog' }
        }
        'IntentAndInteraction' {
            return $null
        }
        'ExternalBoundaries' {
            if ($Row.Type -eq 'Terraria.Social.SocialAPI') { return 'SocialApiRegistry' }
            if ($Row.Type -eq 'Terraria.Social.WeGame.IPCBase') { return 'SocialTransportIpc' }
            if ($Row.Type -like 'Terraria.Social.Base.*') { return 'WorkshopAndJoinBoundaryData' }
        }
        'ExternalDependencyOrGenerated' {
            if ($Row.Type -eq 'BCrypt.Net.BCrypt') { return 'CryptographicDependency' }
            if ($Row.Type -like 'NATUPNPLib.*') { return 'NatPortMappingInterop' }
        }
        'ClientPresentationAndTools' {
            if ($path -like 'Terraria.Achievements/*') { return 'AchievementPresentationState' }
            if ($path -like 'Terraria.Audio/*') {
                if ($Row.Type -eq 'Terraria.Audio.LegacySoundPlayer') { return 'AudioLegacySoundAdapterState' }
                if ($Row.Type -eq 'Terraria.Audio.SoundEngine') { return 'AudioPlaybackCoordinatorState' }
                if ($Row.Type -eq 'Terraria.Audio.SoundPlayer') { return 'AudioTrackedSoundState' }
                if ($Row.Type -in @('Terraria.Audio.ActiveSound', 'Terraria.Audio.VampireSizzleTracker')) {
                    return 'AudioActiveSoundState'
                }
                return 'AudioDefinitionAndTrackState'
            }
            if ($path -like 'Terraria.Cinematics/*') { return 'CinematicTimelineState' }
            if ($path -like 'Terraria.Graphics/*') { return 'CameraAndVertexPresentation' }
            if ($path -like 'Terraria.Map/*') {
                if ($Row.Type -in @('Terraria.Map.MapHelper', 'Terraria.Map.MapTile', 'Terraria.Map.MapUpdateQueue')) {
                    return 'MapTileStorageAndUpdateState'
                }
                if ($Row.Type -in @('Terraria.Map.MapOverlayDrawContext', 'Terraria.Map.PingMapLayer', 'Terraria.Map.PingMapLayer.Ping', 'Terraria.Map.TeleportPylonsMapLayer')) {
                    return 'MapOverlayAndLayerPresentation'
                }
                if ($Row.Type -eq 'Terraria.Map.WorldMap') { return 'WorldMapState' }
            }
            if ($path -like 'Terraria.UI/ItemSlot.cs') {
                if ($Row.Type -eq 'Terraria.UI.ItemSlot.Context') { return 'UiItemSlotContextDefinitions' }
                if ($Row.Type -in @('Terraria.UI.ItemSlot.AlternateClickAction', 'Terraria.UI.ItemSlot.ItemTransferInfo')) {
                    return 'UiItemSlotTransferState'
                }
                return 'UiItemSlotPresentationAndPulseState'
            }
            if ($path -like 'Terraria.UI/ItemSorting.cs') { return 'UiItemSorting' }
            if ($path -like 'Terraria.UI/ItemTooltip.cs') { return 'UiItemTooltipState' }
            if ($path -like 'Terraria.UI/*') {
                if ($Row.Type -in @('Terraria.UI.CalculatedStyle', 'Terraria.UI.SnapPoint', 'Terraria.UI.StyleDimension')) {
                    return 'UiLayoutPrimitives'
                }
                if ($Row.Type -in @('Terraria.UI.UIElement', 'Terraria.UI.UIState')) { return 'UiElementTreeState' }
                if ($Row.Type -in @('Terraria.UI.UIEvent', 'Terraria.UI.UIMouseEvent', 'Terraria.UI.UIScrollWheelEvent')) {
                    return 'UiEventPayloads'
                }
                if ($Row.Type -in @('Terraria.UI.UserInterface', 'Terraria.UI.UserInterface.InputPointerCache')) {
                    return 'UiInputPointerState'
                }
            }
        }
        'SharedRuntimeMechanisms' {
            if ($path -eq 'CallTracker.cs') { return 'SharedCallTrackingDiagnostics' }
            if ($path -eq 'Terraria/TimeLogger.cs') {
                if ($Row.Type -eq 'Terraria.TimeLogger.DataSeries') { return 'SharedTimeSeriesDataSeriesState' }
                if ($Row.Type -eq 'Terraria.TimeLogger.TimeLogData') { return 'SharedTimeSeriesEntryState' }
                if ($Row.Type -eq 'Terraria.TimeLogger.FormatPool') { return 'SharedTimeSeriesFormattingState' }
                if ($Row.Type -eq 'Terraria.TimeLogger') { return 'SharedTimeLoggerCoordinatorState' }
                throw "No TimeLogger fine subsystem mapping for type '$($Row.Type)'."
            }
            if ($path -eq 'Terraria/Item.cs') {
                $itemIdentityMembers = @(
                    'width', 'height', '_nameOverride', 'type', 'favorited', 'stack', 'maxStack', 'uniqueStack', 'prefix',
                    'active', 'Name', 'Variant', 'IsACoin', 'IsAir'
                )
                $itemPresentationMembers = @(
                    'wornArmor', 'tooltipContext', 'tooltipSlot', 'dye', 'hairDye', 'paint', 'paintCoating',
                    'color', 'alpha', 'glowMask', 'scale', 'UseSound', 'useSoundPitch', 'defense',
                    'headSlot', 'bodySlot', 'legSlot', 'handOnSlot', 'handOffSlot', 'backSlot', 'frontSlot',
                    'shoeSlot', 'waistSlot', 'wingSlot', 'shieldSlot', 'neckSlot', 'faceSlot', 'balloonSlot',
                    'beardSlot', 'voiceSlot', 'stringColor', 'ToolTip', 'BestiaryNotes', 'social', 'vanity',
                    'newAndShiny', 'hasVanityEffects', 'PaintOrCoating'
                )
                $itemProgressionAndWorldInteractionMembers = @(
                    'questItem', 'flame', 'mech', 'tileWand', 'fishingPole', 'bait', 'makeNPC', 'expertOnly',
                    'expert'
                )
                $itemCommerceMembers = @('isAShopItem', 'buyOnce', 'value', 'buy', 'shopSpecialCurrency', 'shopCustomPrice')
                $itemEffectMembers = @('buffType', 'buffTime', 'mountType', 'cartTrack', 'chlorophyteExtractinatorConsumable', 'DD2Summon')
                $itemStaticCatalogMembers = @('headType', 'bodyType', 'legType', 'staff', 'claw')
                $itemDerivedMembers = @('OriginalRarity', 'OriginalDamage', 'OriginalDefense')
                if ($itemIdentityMembers -contains $Row.Member) { return 'SharedItemIdentityAndStackState' }
                if ($itemPresentationMembers -contains $Row.Member) { return 'SharedItemEquipmentAndPresentationState' }
                if ($itemProgressionAndWorldInteractionMembers -contains $Row.Member) { return 'SharedItemProgressionAndWorldInteractionState' }
                if ($itemCommerceMembers -contains $Row.Member) { return 'SharedItemCommerceState' }
                if ($itemEffectMembers -contains $Row.Member) { return 'SharedItemBuffMountAndConsumableEffects' }
                if ($itemDerivedMembers -contains $Row.Member) { return 'SharedItemDerivedQueries' }
                if ($itemStaticCatalogMembers -contains $Row.Member) { return 'SharedItemStaticCatalogRules' }
                if (($Row.SourceLine -le 76) -or ($Row.SourceLine -ge 305 -and $Row.SourceLine -le 315)) {
                    return 'SharedItemStaticCatalogRules'
                }
                if ($Row.SourceLine -ge 78 -and $Row.SourceLine -le 302) {
                    return 'SharedItemUseToolAndCombatState'
                }
                throw "No Item.cs fine subsystem mapping for member '$($Row.Member)' at line $($Row.SourceLine)."
            }
            if ($path -match '^Terraria/(Recipe|RecipeGroup)\.cs$' -or $path -eq 'Terraria.GameContent/CraftingRequests.cs') {
                if ($path -eq 'Terraria.GameContent/CraftingRequests.cs') { return 'CraftingRequestState' }
                return 'RecipeDefinitionCatalog'
            }
            if ($path -match '^Terraria.GameContent/EmergencyStacking\.cs$') { return 'ItemEmergencyStackingState' }
            if ($path -match '^Terraria.GameContent/QuickStacking\.cs$') { return 'ItemQuickStackingState' }
            if ($path -eq 'Terraria/GetItemSettings.cs') {
                return 'ItemTransferSettings'
            }
            if ($path -match '^Terraria.GameContent/(ItemShopSellbackHelper|ShopHelper)\.cs$' -or $path -eq 'Terraria/ShoppingSettings.cs') {
                return 'SharedItemCommerceAndPricing'
            }
            if ($path -like 'Terraria.GameContent.Items/*') {
                if ($path -match '/(ItemVariants|ItemVariant|ItemVariantCondition)\.cs$') { return 'ItemVariantDefinitions' }
                if ($path -match '/(WhipTagEffect|TagEffectState|UniqueTagEffect)\.cs$') { return 'ItemTagEffectState' }
            }
            if ($path -like 'Terraria.GameContent.Prefixes/*') {
                return 'LegacyItemPrefixCatalog'
            }
            if ($path -eq 'Terraria/EquipmentLoadout.cs' -or $path -match '^Terraria.DataStructures/(ArmorSetBonus|ArmorSetBonuses|WingStats)\.cs$') {
                if ($path -eq 'Terraria/EquipmentLoadout.cs') { return 'EquipmentLoadoutState' }
                if ($path -eq 'Terraria.DataStructures/ArmorSetBonuses.cs') { return 'ArmorSetBonusCatalog' }
                if ($path -eq 'Terraria.DataStructures/WingStats.cs') { return 'WingStatsDefinition' }
                return 'ArmorSetBonusDefinitions'
            }
            if ($path -eq 'Terraria.GameContent.Generation.Dungeon/DungeonData.cs') {
                return 'SharedDungeonGenerationDataState'
            }
            if ($path -eq 'Terraria.GameContent.Generation.Dungeon/DungeonGenVars.cs') {
                return 'SharedDungeonLegacyGenerationGlobals'
            }
            if ($path -eq 'Terraria.GameContent.Generation.Dungeon/DungeonCrawler.cs') {
                return 'SharedDungeonCrawlerRuntimeState'
            }
            if ($path -eq 'Terraria.GameContent.Generation.Dungeon/DungeonUtils.cs') {
                return 'SharedDungeonGenerationQueries'
            }
            if ($path -like 'Terraria.GameContent.Generation.Dungeon.Entrances/*') { return 'SharedDungeonEntranceDefinitions' }
            if ($path -like 'Terraria.GameContent.Generation.Dungeon.Features/*') { return 'SharedDungeonFeatureDefinitions' }
            if ($path -like 'Terraria.GameContent.Generation.Dungeon.Halls/*') { return 'SharedDungeonHallDefinitions' }
            if ($path -like 'Terraria.GameContent.Generation.Dungeon.LayoutProviders/*') { return 'SharedDungeonLayoutProviderDefinitions' }
            if ($path -like 'Terraria.GameContent.Generation.Dungeon.Rooms/*') { return 'SharedDungeonRoomDefinitions' }
            if ($path -match '^Terraria.GameContent.Generation.Dungeon/(DungeonGenerationStyleData|DungeonGenerationStyles)\.cs$') {
                return 'SharedDungeonStyleCatalog'
            }
            if ($path -match '^Terraria.GameContent.Generation.Dungeon/(DungeonLayoutProvider|DungeonLayoutProviderSettings)\.cs$') {
                return 'SharedDungeonLayoutProviderState'
            }
            if ($path -match '^Terraria.GameContent.Generation.Dungeon/(DungeonBounds|DungeonDoorData|DungeonPlatformData|DualDungeonUnbreakableWallTiers)\.cs$') {
                return 'SharedDungeonGeometryAndPlacementDefinitions'
            }
            if ($path -like 'Terraria.GameContent.Generation.Dungeon/*') { throw "Unexpected dungeon-root path '$path'." }
            if ($path -eq 'Terraria.GameContent.Events/DD2Event.cs') { return 'SharedInvasionEventState' }
            if ($path -match '^Terraria.GameContent.Events/(BirthdayParty|CultistRitual|LanternNight|MysticLogFairiesEvent|Sandstorm)\.cs$') {
                return 'SharedSeasonalWorldEventState'
            }
            if ($path -match '^Terraria.GameContent.Events/(CreditsRollEvent|MoonlordDeathDrama|ScreenObstruction)\.cs$') {
                return 'SharedWorldEventPresentationState'
            }
            if ($path -like 'Terraria.GameContent.Events/*') {
                throw "No world-event fine subsystem mapping for path '$path'."
            }
            if ($path -match '^Terraria.GameContent/(BannerSystem|BossDamageTracker|InvasionDamageTracker)\.cs$') {
                return 'SharedInvasionAndBossTracking'
            }
            if ($path -match '^Terraria.GameContent/(ConditionalDialogue|LucyAxeMessage)\.cs$') {
                return 'SharedConditionalDialogueSupport'
            }
            if ($path -eq 'Terraria.GameContent/TownRoomManager.cs') {
                return 'SharedTownRoomState'
            }
            if ($path -eq 'Terraria.GameContent/LightningGenerator.cs') { return 'SharedLightningGenerationState' }
            if ($path -eq 'Terraria/WaterfallManager.cs') { return 'SharedWaterfallState' }
            if ($path -match '^Terraria/(Cloud|Rain|Star)\.cs$') { return 'SharedWeatherParticleState' }
            if ($path -like 'Terraria.GameContent.Ambience/*' -or $path -match '^Terraria.GameContent/(AmbientWindSystem|BackgroundChangeFlashInfo|TreeTopsInfo)\.cs$') {
                return 'SharedAmbientSkyAndWindState'
            }
            if ($path -match '^Terraria/(Dust|Gore)\.cs$') {
                return 'SharedParticleAndGoreEffects'
            }
            if ($path -match '^Terraria.GameContent/(ExtraSpawnPointManager|ExtraSpawnSettings)\.cs$' -or $path -eq 'Terraria/NPCSpawnParams.cs') {
                return 'WorldSpawnConfigurationState'
            }
            if ($path -match '^Terraria.GameContent/(FixExploitManEaters|SpecialSeedFeatures)\.cs$') {
                return 'WorldSeedAndExploitRules'
            }
            if ($path -match '^Terraria.GameContent/(DontStarveDarknessDamageDealer|ExtraSeatInfo)\.cs$') {
                return 'EnvironmentDamageAndSeatState'
            }
            if ($path -match '^Terraria.GameContent/(SpelunkerProjectileHelper|UnbreakableWallScan|VoidLensHelper)\.cs$') {
                return 'WorldEnvironmentScanHelpers'
            }
            if ($path -match '^Terraria.DataStructures/(PlacementDetails|PlacementHook|TileObjectPreviewData)\.cs$' -or $path -eq 'Terraria/TileObject.cs') {
                return 'SharedTileObjectPlacementDefinitions'
            }
            if ($path -match '^Terraria.DataStructures/(AnchorData|AnchoredEntitiesCollection|Point16|TileReachCheckSettings)\.cs$') {
                return 'SharedTileAnchorAndReachQueries'
            }
            if ($path -match '^Terraria/(Framing|Sign)\.cs$') {
                return 'SharedTileFramingAndSignState'
            }
            if ($path -like 'Terraria.Modules/*') {
                return 'SharedTilePlacementModules'
            }
            if ($path -eq 'Terraria/Entity.cs') { return 'EntityAuthoritativeState' }
            if ($path -eq 'Terraria.DataStructures/EntityShadowInfo.cs') { return 'EntityShadowPresentationState' }
            if ($path -eq 'Terraria/WorldItem.cs') {
                return 'SharedWorldItemState'
            }
            if ($path -match '^Terraria.GameContent/TilePaintSystemV2\.cs$' -or $path -eq 'Terraria/TileColorCache.cs') {
                return 'SharedTilePaintState'
            }
            if ($path -eq 'Terraria.Utilities/TileSnapshot.cs') {
                return 'SharedTileSnapshots'
            }
            if ($path -match '^Terraria.DataStructures/(MultiPointHitbox|NPCAimedTarget|NPCDebuffImmunityData|NPCKillAttempt)\.cs$') {
                return 'SharedCombatTargetingAndImmunity'
            }
            if ($path -like 'Terraria.Physics/*') {
                return 'SharedPhysicsCollisionQueries'
            }
            if ($path -eq 'Terraria/HitTile.cs') {
                return 'SharedHitTileTracking'
            }
            if ($path -like 'Terraria.Net.Sockets/*') {
                return 'SharedNetworkSocketTransport'
            }
            if ($path -like 'Terraria.Net/*') {
                return 'SharedNetworkPacketPrimitives'
            }
            if ($path -like 'Terraria.GameContent.NetModules/*') {
                return 'SharedContentNetworkModules'
            }
            if ($path -match '^Terraria.DataStructures/(ActiveSections|FlowerPacketInfo)\.cs$') {
                return 'SharedNetworkSectionProjections'
            }
            if ($path -eq 'Terraria/Lang.cs') { return 'LegacyLanguageCatalogState' }
            if ($Row.Type -in @('Terraria.Localization.GameCulture', 'Terraria.Localization.Language', 'Terraria.Localization.LanguageManager')) {
                return 'LocalizationCultureAndLanguageState'
            }
            if ($Row.Type -in @('Terraria.Localization.LocalizedText', 'Terraria.Localization.NetworkText', 'Terraria.Localization.VariableText', 'Terraria.Localization.VariableText.Condition')) {
                return 'LocalizedTextValueState'
            }
            if ($path -like 'Terraria.Chat/*' -or $path -like 'Terraria.Chat.Commands/*') {
                return 'SharedChatAndCommandProtocol'
            }
            if ($path -like 'Terraria.UI.Chat/*' -or $path -like 'Terraria.GameContent.UI.Chat/*') {
                return 'SharedChatPresentation'
            }
            if ($Row.Type -in @('Terraria.GameInput.PlayerInputProfile', 'Terraria.GameInput.KeyConfiguration')) {
                return 'InputProfilesAndConfiguration'
            }
            if ($Row.Type -in @('Terraria.GameInput.TriggersSet', 'Terraria.GameInput.TriggersPack')) {
                return 'InputTriggerState'
            }
            if ($Row.Type -eq 'Terraria.GameInput.PlayerInput') { return 'PlayerInputRuntimeState' }
            if ($Row.Type -eq 'Terraria.GameInput.LockOnHelper') { return 'LockOnTargetingState' }
            if ($path -eq 'Terraria/FocusHelper.cs' -or $path -eq 'Terraria.GameInput/LockOnHelper.cs') {
                return 'SharedControlFocusHelpers'
            }
            if ($path -like 'Terraria.Graphics.Shaders/*') {
                return 'SharedShaderData'
            }
            if ($path -like 'Terraria.Graphics.Effects/*' -or $path -like 'Terraria.GameContent.Skies/*') {
                return 'SharedEffectAndSkyPresentation'
            }
            if ($path -like 'Terraria.Graphics.Renderers/*') {
                return 'SharedParticlePresentation'
            }
            if ($Row.Type -in @('Terraria.DataStructures.Animation', 'Terraria.DataStructures.DrawAnimation', 'Terraria.DataStructures.DrawAnimationVertical', 'Terraria.DataStructures.SpriteFrame', 'Terraria.Animation')) {
                return 'DrawAnimationAndFrameState'
            }
            if ($Row.Type -in @('Terraria.DataStructures.DrawData', 'Terraria.DataStructures.SpriteBatchBeginner')) {
                return 'DrawCommandAndBatchState'
            }
            if ($Row.Type -in @('Terraria.GameContent.Drawing.ParticleOrchestraSettings', 'Terraria.GameContent.Drawing.TileDrawingBase', 'Terraria.GameContent.Drawing.HorizonHelper', 'Terraria.GameContent.Drawing.NextHorizonRenderer')) {
                return 'WorldDrawingAuxiliaryState'
            }
            if ($path -like 'Terraria.GameContent.UI.Elements/*') {
                return 'SharedContentUiWidgets'
            }
            if ($path -like 'Terraria.GameContent.UI.States/*') {
                return 'SharedContentUiScreens'
            }
            if ($path -match '^Terraria.GameContent.UI/CustomCurrency') {
                return 'SharedContentUiCurrency'
            }
            if ($path -match '^Terraria.GameContent.UI/(EmoteBubble|WiresUI|WorldUIAnchor|ItemRarity)\.cs$') {
                return 'SharedWorldInteractionUi'
            }
            if ($path -like 'Terraria.GameContent.ItemDropRules/*') {
                return 'SharedDropRuleDefinitions'
            }
            if ($path -like 'Terraria.GameContent.LootSimulation/*') {
                return 'SharedLootSimulation'
            }
            if ($path -match '^Terraria.DataStructures/EntitySource_(BossSpawn|DropAsItem|Loot)\.cs$') {
                return 'SharedDropSourceAttribution'
            }
            if ($path -like 'Terraria.GameContent.Creative/*') {
                if ($path -match '(ItemsSacrificedUnlocksTracker|CreativeItemSacrificesCatalog|CreativeUnlocksTracker)\.cs$') {
                    return 'SharedCreativeUnlockProgress'
                }
                if ($path -match '^(Terraria.GameContent.Creative/CreativePowers\.cs|Terraria.GameContent.Creative/ICreativePower\.cs)$') {
                    return 'SharedCreativePowerDefinitions'
                }
                if ($path -eq 'Terraria.GameContent.Creative/CreativePowerManager.cs') {
                    return 'SharedCreativePowerRuntimeManager'
                }
                if ($path -match '^(Terraria.GameContent.Creative/CreativePowersHelper\.cs|Terraria.GameContent.Creative/CreativePowerUIElementRequestInfo\.cs)$') {
                    return 'SharedCreativePowerPresentation'
                }
                throw "No creative-power fine subsystem mapping for path '$path'."
            }
            if ($path -like 'Terraria.GameContent.Bestiary/*') {
                if ($Row.Type -in @('Terraria.GameContent.Bestiary.BestiaryDatabase', 'Terraria.GameContent.Bestiary.BestiaryEntry')) {
                    return 'BestiaryCatalogAndEntries'
                }
                if ($Row.Type -in @('Terraria.GameContent.Bestiary.BestiaryUnlockProgressReport', 'Terraria.GameContent.Bestiary.BestiaryUnlocksTracker', 'Terraria.GameContent.Bestiary.NPCKillsTracker', 'Terraria.GameContent.Bestiary.NPCWasNearPlayerTracker', 'Terraria.GameContent.Bestiary.NPCWasChatWithTracker')) {
                    return 'BestiaryUnlockTracking'
                }
                if ($path -match '/(Filters|SortingSteps)\.cs$') { return 'BestiaryFiltersAndSorting' }
                return 'BestiaryInfoElementsAndProviders'
            }
            if ($path -like 'Terraria.GameContent.Personalities/*' -or $path -eq 'Terraria.GameContent/TownNPCProfiles.cs') {
                return 'SharedNpcPersonalityCatalog'
            }
            if ($path -like 'Terraria.GameContent.ObjectInteractions/*') {
                return 'SharedSmartInteractionQueries'
            }
            if ($path -eq 'Terraria.GameContent/DoorOpeningHelper.cs') { return 'DoorOpeningInteractionState' }
            if ($path -eq 'Terraria.GameContent/SmartCursorHelper.cs') { return 'SmartCursorInteractionState' }
            if ($path -eq 'Terraria.GameContent/PressurePlateHelper.cs') { return 'PressurePlateInteractionState' }
            if ($path -match '^Terraria.GameContent/(FakeCursorItem|PositionedChest)\.cs$') { return 'CursorAndChestInteractionState' }
            if ($path -match '^Terraria.Utilities/(FastRandom|LCG32Random|UnifiedRandom)\.cs$') {
                return 'SharedRandomSources'
            }
            if ($path -match '^Terraria.DataStructures/(BufferPool|CachedBuffer|DoubleStack|EntrySorter)\.cs$') {
                return 'SharedBufferAndCollectionPools'
            }
            if ($path -match '^Terraria.Utilities.Terraria.Utilities/FloatRange\.cs$' -or $path -match '^Terraria.Utilities/(Bits64|BitSet2D|IntRange|Vertical64BitStrips)\.cs$' -or $path -eq 'Terraria/BitsByte.cs') {
                return 'SharedRangeAndBitUtilities'
            }
            if ($path -like 'Terraria.Utilities.FileBrowser/*' -or $path -eq 'Terraria.Utilities/FileUtilities.cs') {
                return 'SharedGeneralFilePlatformUtilities'
            }
            if ($path -eq 'Terraria.Utilities/CrashWatcher.cs') { return 'SharedGeneralDiagnosticsUtilities' }
            if ($path -eq 'Terraria.Utilities/NewRuntimeMethods.cs') { return 'SharedGeneralFilePlatformUtilities' }
            if ($path -eq 'Terraria/DelegateMethods.cs' -or $path -eq 'Terraria.Utilities/OldAttribute.cs' -or $path -eq 'Terraria.Utilities/Secrets.cs') {
                return 'SharedGeneralDelegateAndMetadataUtilities'
            }
            if ($path -eq 'Terraria/Utils.cs' -or $path -like 'Terraria.Utilities/*' -or $path -like 'Terraria.Utilities.*/*') {
                return 'SharedGeneralPureUtilities'
            }
            if ($path -match '^Terraria.IO/(FavoritesFile|FileData|FileMetadata|GameConfiguration|Preferences)\.cs$') {
                return 'SharedSaveAndConfigurationAdapters'
            }
            if ($path -match '^Terraria.IO/(ResourcePack|ResourcePackList)\.cs$') {
                return 'SharedResourcePackAdapters'
            }
            if ($path -like 'Terraria.Utilities.FileBrowser/*' -or $path -eq 'Terraria.Utilities/FileUtilities.cs') {
                return 'SharedFileSystemAdapters'
            }
            if ($path -match '^Terraria.DataStructures/(BackgroundVariant|BackgroundVariantSet)\.cs$') {
                return 'SharedBackgroundPresentationDefinitions'
            }
            if ($path -match '^Terraria.GameContent/(ChildSafety|ContentRejectionFromSize|VanillaContentValidator)\.cs$') {
                return 'SharedContentValidation'
            }
            if ($path -match '^Terraria.GameContent/(FontAssets|HairstyleUnlocksHelper|Profiles)\.cs$' -or $path -match '^Terraria.DataStructures/(ColorSlidersSet|IConfigKeyHolder)\.cs$') {
                return 'SharedContentPresentationCatalog'
            }
            if ($path -like 'Terraria.Testing.ChatCommands/*') { return 'DebugCommandProtocol' }
            if ($path -eq 'Terraria.Testing/DebugOptions.cs') { return 'DebugRuntimeOptions' }
            if ($path -eq 'Terraria.Testing/DetailedFPS.cs') { return 'DebugFrameTelemetry' }
            if ($path -eq 'Terraria.Testing/GitStatus.cs') { return 'DebugBuildStatus' }
            if ($path -eq 'Terraria.Server/Game.cs' -or $path -eq 'Terraria/Program.cs' -or $path -eq 'Terraria/WindowsLaunch.cs' -or $path -eq 'Terraria/Ref.cs' -or $path -eq 'Terraria/InitData.cs' -or $path -like 'Terraria.DataStructures/IssueReport.cs' -or $path -like 'Terraria.DataStructures/GeneralIssueReporter.cs') {
                return 'SharedStartupAndIssueReporting'
            }
            if ($path -like 'Terraria.Testing/*' -or $path -like 'Terraria.Testing.ChatCommands/*' -or $path -eq 'Terraria.Utilities/CrashWatcher.cs' -or $path -eq 'Terraria.Server/Game.cs' -or $path -eq 'Terraria/Program.cs' -or $path -eq 'Terraria/WindowsLaunch.cs' -or $path -eq 'Terraria/Ref.cs' -or $path -eq 'Terraria/InitData.cs' -or $path -like 'Terraria.DataStructures/IssueReport.cs' -or $path -like 'Terraria.DataStructures/GeneralIssueReporter.cs') {
                return 'SharedVerificationAndDebugTools'
            }
            if ($path -match '^Terraria/(Item|Recipe|RecipeGroup|GetItemSettings|ShoppingSettings)\.cs$' -or $path -match '^Terraria\.GameContent/(QuickStacking|CraftingRequests|ItemShopSellbackHelper|ShopHelper|EmergencyStacking)\.cs$') {
                return 'SharedItemAndRecipeState'
            }
            if ($path -like 'Terraria.GameContent.Items/*' -or $path -like 'Terraria.GameContent.Prefixes/*' -or $path -match '^Terraria/(EquipmentLoadout)\.cs$' -or $path -match '^Terraria.DataStructures/(ArmorSetBonus|ArmorSetBonuses|WingStats)\.cs$') {
                return 'SharedItemVariantsAndEquipment'
            }
            if ($path -like 'Terraria.GameContent.FishDropRules/*') { return 'SharedFishingDropRuleDefinitions' }
            if ($path -match '^Terraria.DataStructures/(FishingAttempt|PlayerFishingConditions)\.cs$') {
                return 'SharedFishingAttemptAndConditions'
            }
            if ($path -eq 'Terraria.GameContent/ChumBucketProjectileHelper.cs' -or $path -eq 'Terraria.DataStructures/EntitySource_FishedOut.cs') {
                return 'SharedFishingCatchEffects'
            }
            if ($path -like 'Terraria.GameContent.ItemDropRules/*' -or $path -like 'Terraria.GameContent.LootSimulation/*' -or $path -match '^Terraria.DataStructures/EntitySource_(Loot|DropAsItem|BossSpawn)\.cs$') {
                return 'SharedLootAndDropRules'
            }
            if ($path -like 'Terraria.GameContent.Bestiary/*' -or $path -like 'Terraria.GameContent.Personalities/*' -or $path -eq 'Terraria.GameContent/TownNPCProfiles.cs') {
                return 'SharedBestiaryAndPersonalityCatalog'
            }
            if ($path -like 'Terraria.GameContent.Achievements/*') { return 'SharedAchievementProgressSupport' }
            if ($path -like 'Terraria.GameContent.Generation.Dungeon*/*') { return 'SharedWorldGenerationDungeonDefinitions' }
            if ($path -like 'Terraria.GameContent.Biomes/*' -or $path -like 'Terraria.GameContent.Biomes.CaveHouse/*') {
                return 'SharedWorldGenerationBiomeSupport'
            }
            if ($path -like 'Terraria.GameContent.Generation/*' -or $path -match '^Terraria.DataStructures/(NoRoomCheckFeedback|IRoomCheckFeedback_Spread)\.cs$') { return 'SharedWorldGenerationSupport' }
            if ($path -like 'Terraria.GameContent.Events/*' -or $path -match '^Terraria.GameContent/(BossDamageTracker|InvasionDamageTracker|BannerSystem|ConditionalDialogue|TownRoomManager|LucyAxeMessage)\.cs$') {
                return 'SharedWorldEventsAndInvasionSupport'
            }
            if ($path -match '^Terraria.DataStructures/GameDifficulty(Data|Level)\.cs$') {
                return 'SharedDifficultyAndRuleMetadata'
            }
            if ($path -like 'Terraria.GameContent.Ambience/*' -or $path -match '^Terraria.GameContent/(AmbientWindSystem|BackgroundChangeFlashInfo|LightningGenerator|TreeTopsInfo|DontStarveDarknessDamageDealer|ExtraSpawnPointManager|ExtraSpawnSettings|ExtraSeatInfo|ScreenObstruction|FixExploitManEaters|SpelunkerProjectileHelper|UnbreakableWallScan|VoidLensHelper|SpecialSeedFeatures)\.cs$' -or $path -match '^Terraria/(Cloud|Dust|Gore|NPCSpawnParams|Rain|Star|WaterfallManager)\.cs$') {
                return 'SharedWorldEnvironmentAndWeather'
            }
            if ($path -match '^Terraria.GameContent/(PortalHelper|TeleportPylonInfo|ShimmerUnstuckHelper)\.cs$') { return 'SharedTeleportAndPortalSupport' }
            if ($path -like 'Terraria.GameContent.ObjectInteractions/*' -or $path -match '^Terraria.GameContent/(DoorOpeningHelper|PressurePlateHelper|SmartCursorHelper|FakeCursorItem|PositionedChest)\.cs$') {
                return 'SharedWorldInteractionHelpers'
            }
            if ($path -like 'Terraria.Modules/*' -or $path -match '^Terraria/(TileObject|Sign|Framing)\.cs$' -or $path -match '^Terraria.DataStructures/(AnchorData|AnchoredEntitiesCollection|PlacementDetails|PlacementHook|TileObjectPreviewData|TileReachCheckSettings|Point16)\.cs$') {
                return 'SharedTilePlacementAndAnchors'
            }
            if ($Row.Type -in @('Terraria.DataStructures.TileEntity', 'Terraria.DataStructures.TileEntitiesManager') -or $Row.Type -like 'Terraria.DataStructures.TileEntityType*') {
                return 'TileEntityRegistryAndBaseState'
            }
            if ($Row.Type -in @('Terraria.GameContent.Tile_Entities.TEDisplayDoll', 'Terraria.GameContent.Tile_Entities.TEHatRack', 'Terraria.GameContent.Tile_Entities.TEWeaponsRack', 'Terraria.GameContent.Tile_Entities.TEDeadCellsDisplayJar', 'Terraria.GameContent.Tile_Entities.TEFoodPlatter', 'Terraria.GameContent.Tile_Entities.TEItemFrame', 'Terraria.GameContent.Tile_Entities.TEDisplayDoll.DisplayDollPose')) {
                return 'TileEntityDisplayAndInventoryState'
            }
            if ($Row.Type -in @('Terraria.GameContent.Tile_Entities.TELogicSensor', 'Terraria.GameContent.Tile_Entities.TECritterAnchor', 'Terraria.GameContent.Tile_Entities.TEKiteAnchor')) {
                return 'TileEntityAnchorAndSensorState'
            }
            if ($Row.Type -in @('Terraria.GameContent.Tile_Entities.TETrainingDummy', 'Terraria.GameContent.Tile_Entities.TETeleportationPylon', 'Terraria.GameContent.Tile_Entities.TELeashedEntityAnchorWithItem')) {
                return 'TileEntityWorldInteractionState'
            }
            if ($path -eq 'Terraria.GameContent/TilePaintSystemV2.cs' -or $path -eq 'Terraria/TileColorCache.cs' -or $path -eq 'Terraria.Utilities/TileSnapshot.cs') {
                return 'SharedTilePaintAndSnapshots'
            }
            if ($path -match '^Terraria/(Entity|WorldItem)\.cs$' -or $path -eq 'Terraria.DataStructures/EntityShadowInfo.cs') {
                return 'SharedEntityAndWorldItemState'
            }
            if ($path -like 'Terraria.DataStructures/EntitySource_*.cs' -or $path -like 'Terraria.DataStructures/AEntitySource_*.cs' -or $path -eq 'Terraria.DataStructures/PlayerDeathReason.cs') {
                return 'SharedEntitySourceAndAttribution'
            }
            if ($path -match '^Terraria.DataStructures/(MinionRespawner|MinionSpawnFromInventoryItem|PlayerGetItemLogger)\.cs$') {
                return 'PlayerItemPickupAndRespawnState'
            }
            if ($path -match '^Terraria.DataStructures/(RejectionMenuInfo|SettingsForCharacterPreview)\.cs$') {
                return 'PlayerPreviewAndRejectionState'
            }
            if ($path -match '^Terraria.DataStructures/(PlayerIntentionGuesser|PlayerInteractionAnchor|PlayerMovementAccsCache|PortableStoolUsage)\.cs$') {
                return 'PlayerIntentAndMovementState'
            }
            if ($path -match '^Terraria.DataStructures/(MultiPointHitbox|NPCAimedTarget|NPCDebuffImmunityData|NPCKillAttempt)\.cs$' -or $path -eq 'Terraria/HitTile.cs' -or $path -like 'Terraria.Physics/*') {
                return 'SharedCombatTargetingAndDamageData'
            }
            if ($path -eq 'Terraria/SceneMetrics.cs') { return 'SharedSceneMetricsSnapshot' }
            if ($path -eq 'Terraria/SceneMetricsScanSettings.cs') { return 'SharedSceneScanSettings' }
            if ($path -eq 'Terraria/SceneState.cs') { return 'SharedSceneVisualState' }
            if ($path -like 'Terraria.Net/*' -or $path -like 'Terraria.Net.Sockets/*' -or $path -like 'Terraria.GameContent.NetModules/*' -or $path -match '^Terraria.DataStructures/(ActiveSections|FlowerPacketInfo)\.cs$') {
                return 'SharedNetworkPrimitives'
            }
            if ($path -like 'Terraria.Localization/*' -or $path -like 'Terraria.Chat/*' -or $path -like 'Terraria.Chat.Commands/*' -or $path -like 'Terraria.UI.Chat/*' -or $path -like 'Terraria.GameContent.UI.Chat/*' -or $path -eq 'Terraria/Lang.cs') {
                return 'SharedLocalizationAndChat'
            }
            if ($path -like 'Terraria.GameInput/*' -or $path -eq 'Terraria/FocusHelper.cs') { return 'SharedInputAndControl' }
            if ($Row.Type -eq 'Terraria.Graphics.Light.LightMap') { return 'LightMapCacheState' }
            if ($Row.Type -eq 'Terraria.Graphics.Light.TileLightScanner') { return 'TileLightScannerState' }
            if ($path -like 'Terraria.Graphics.Light/*' -or $path -eq 'Terraria/Lighting.cs') { return 'LightingEngineState' }
            if ($path -like 'Terraria.Graphics.Shaders/*' -or $path -like 'Terraria.Graphics.Effects/*' -or $path -like 'Terraria.GameContent.Skies/*') {
                return 'SharedShaderAndEffectState'
            }
            if ($path -like 'Terraria.Graphics.Renderers/*' -or $path -like 'Terraria.GameContent.Drawing/*' -or $path -like 'Terraria.Graphics/Vertex*' -or $path -match '^Terraria.DataStructures/(DrawData|DrawAnimation|DrawAnimationVertical|SpriteBatchBeginner|SpriteFrame)\.cs$' -or $path -eq 'Terraria/Animation.cs') {
                return 'SharedParticleAndVertexRendering'
            }
            if ($path -like 'Terraria.Graphics.Capture/*' -or $path -match '^Terraria.DataStructures/(DroneCameraTracker|DrillDebugDraw)\.cs$') { return 'SharedCaptureAndCameraSupport' }
            if ($path -like 'Terraria.GameContent.UI.Elements/*' -or $path -like 'Terraria.GameContent.UI.States/*' -or $path -like 'Terraria.GameContent.UI/*') {
                return 'SharedGameContentUiAndWidgets'
            }
            if ($path -like 'Terraria.GameContent.Golf/*') { return 'SharedGolfState' }
            if ($path -match '^Terraria.GameContent/(Profiles|FontAssets|HairstyleUnlocksHelper|ChildSafety|ContentRejectionFromSize|VanillaContentValidator)\.cs$' -or $path -match '^Terraria.DataStructures/(BackgroundVariant|BackgroundVariantSet|ColorSlidersSet|IConfigKeyHolder)\.cs$') {
                return 'SharedContentPresentationDefinitions'
            }
            if ($path -match '^Terraria/(PopupText|CombatText)\.cs' -or $path -eq 'Terraria.DataStructures/CachedProjectileCounterBuffTextHandler.cs') { return 'SharedPopupAndCombatText' }
            if ($path -eq 'Terraria.DataStructures/TrackedProjectileReference.cs') { return 'TrackedProjectileReferenceState' }
            if ($Row.Type -eq 'Terraria.Minecart.Customization') { return 'MinecartCustomizationState' }
            if ($path -eq 'Terraria/Minecart.cs') { return 'MinecartMotionState' }
            if ($path -like 'Terraria.IO/*' -or $path -like 'Terraria.Utilities.FileBrowser/*' -or $path -eq 'Terraria.Utilities/FileUtilities.cs') {
                return 'SharedPersistenceAndResourceAdapters'
            }
            if ($path -eq 'Terraria/Utils.cs' -or $path -like 'Terraria.Utilities/*' -or $path -like 'Terraria.Utilities.*/*' -or $path -match '^Terraria.DataStructures/(BufferPool|CachedBuffer|DoubleStack|EntrySorter)\.cs$' -or $path -eq 'Terraria/BitsByte.cs') {
                return 'SharedUtilityAndRandomness'
            }
            if ($path -eq 'Terraria/DelegateMethods.cs' -or $path -eq 'Terraria/DataStructures/SoundPlaySet.cs') { return 'SharedUtilityAndRandomness' }
            if ($path -eq 'Terraria.DataStructures/SoundPlaySet.cs') { return 'SharedAudioAndSoundData' }
            if ($path -eq 'Terraria.GameContent/FontAssets.cs') { return 'SharedContentPresentationDefinitions' }
        }
    }

    throw "No fine subsystem mapping for parent '$($Row.Parent)', path '$($Row.RelativePath)', type '$($Row.Type)', line $($Row.SourceLine), member '$($Row.Member)'."
}

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

function Get-SecondLevelFineSubsystemId {
    param(
        [Parameter(Mandatory)][pscustomobject]$Row,
        [Parameter(Mandatory)][string]$BaseFineSubsystem
    )

    $path = $Row.RelativePath

    switch ($BaseFineSubsystem) {
        'TileObjectDefinitionCatalog' {
            $inheritanceMembers = @(
                '_parent', '_linkedAlternates', '_alternates', '_hasOwnAlternates', '_data', '_baseObject',
                'readOnlyData', 'newTile', 'newSubTile', 'newAlternate', 'LinkedAlternates', 'Alternates',
                'AlternatesCount'
            )
            if ($inheritanceMembers -contains $Row.Member) {
                return 'TileObjectDefinitionInheritanceState'
            }
            return 'TileObjectDefinitionPlacementAndStyleState'
        }
        'SharedTimeLoggerCoordinatorState' {
            $coordinationMembers = @(
                'FrameCount', 'logWriter', 'logBuilder', 'framesToLog', 'currentFrame',
                'startLoggingNextFrame', 'endLoggingThisFrame', 'currentlyLogging', 'DataSeriesHeaders',
                'activeDataSeries', 'entries', '_onNextFrame', 'ABTestMode', 'ABTestName', 'ABTestFlag',
                '_entriesToDraw'
            )
            if ($coordinationMembers -contains $Row.Member) {
                return 'SharedTimeLoggerFrameCoordinationState'
            }
            return 'SharedTimeLoggerRenderMetricsState'
        }
        'SharedSceneMetricsSnapshot' {
            $aggregateMembers = @(
                'ShimmerTileCount', 'EvilTileCount', 'HolyTileCount', 'HoneyBlockCount', 'ActiveMusicBox',
                'MusicBoxSilence', 'SandTileCount', 'MushroomTileCount', 'SnowTileCount', 'WaterCandleCount',
                'PeaceCandleCount', 'ShadowCandleCount', 'PartyMonolithCount', 'MeteorTileCount',
                'BloodTileCount', 'JungleTileCount', 'DungeonTileCount', 'HasSunflower', 'HasGardenGnome',
                'HasClock', 'HasCampfire', 'HasStarInBottle', 'HasHeartLantern', 'ActiveFountainColor',
                'ActiveMonolithType', 'BloodMoonMonolith', 'MoonLordMonolith', 'EchoMonolith',
                'ShimmerMonolithState', 'CRTMonolith', 'RetroMonolith', 'NoirMonolith', 'RadioThingMonolith',
                'HasCatBast', 'GraveyardTileCount', 'DesertSandTileCount', 'OceanSandTileCount',
                'BehindBackwall', 'TownNPCCount'
            )
            if ($aggregateMembers -contains $Row.Member) {
                return 'SharedSceneAggregateAndDecorationState'
            }
            return 'SharedSceneScanAndZoneState'
        }
        'SharedDropRuleDefinitions' {
            $resolutionPaths = @(
                'Terraria.GameContent.ItemDropRules/DropAttemptInfo.cs',
                'Terraria.GameContent.ItemDropRules/DropRateInfo.cs',
                'Terraria.GameContent.ItemDropRules/DropRateInfoChainFeed.cs',
                'Terraria.GameContent.ItemDropRules/ItemDropAttemptResult.cs',
                'Terraria.GameContent.ItemDropRules/IItemDropRule.cs',
                'Terraria.GameContent.ItemDropRules/IItemDropRuleChainAttempt.cs',
                'Terraria.GameContent.ItemDropRules/ItemDropDatabase.cs',
                'Terraria.GameContent.ItemDropRules/ItemDropResolver.cs'
            )
            if ($resolutionPaths -contains $path) {
                return 'SharedDropRuleResolutionAndCatalogState'
            }
            return 'SharedDropRuleSelectionAndChainState'
        }
        'AudioLegacySoundAdapterState' {
            if ($Row.Member -match '^(SoundInstance|TrackableSoundInstances$|_trackedInstances$)') {
                return 'AudioLegacySoundInstanceState'
            }
            return 'AudioLegacySoundCatalogState'
        }
        'MainCageAnimationState' {
            if ($Row.Member -match '^(fishBowl|lavaFishBowl|frogCage|jellyfishCage|waterStriderCage|seahorseCage|pufferfishCage)') {
                return 'MainCageAquaticAndAmphibianAnimationState'
            }
            return 'MainCageBirdAndTerrestrialAnimationState'
        }
        'SharedFishingDropRuleDefinitions' {
            $catalogPaths = @(
                'Terraria.GameContent.FishDropRules/AFishDropRulePopulator.cs',
                'Terraria.GameContent.FishDropRules/FishDropRule.cs',
                'Terraria.GameContent.FishDropRules/FishPossibilityEntry.cs',
                'Terraria.GameContent.FishDropRules/FishRarityCondition.cs',
                'Terraria.GameContent.FishDropRules/FishDropRuleList.cs'
            )
            if ($catalogPaths -contains $path) {
                return 'SharedFishingDropCatalogState'
            }
            return 'SharedFishingConditionContextState'
        }
        'SharedWorldGenerationBiomeSupport' {
            if ($path -eq 'Terraria.GameContent.Biomes/CaveHouseBiome.cs' -or
                $path -like 'Terraria.GameContent.Biomes.CaveHouse/*') {
                return 'SharedBiomeCaveHouseAndStructureState'
            }
            return 'SharedBiomeDungeonAndTerrainState'
        }
        'UiItemSorting' {
            if ($Row.Member -match '^(_sort_|_fillAmmoFromInventory_)') {
                return 'UiItemSortingExecutionState'
            }
            return 'UiItemSortingDefinitions'
        }
        'SharedDungeonRoomDefinitions' {
            if ($path -match '/(DungeonRoom|DungeonRoomSettings|StepBasedDungeonRoomSettings|GenShapeDungeonRoomSettings|LegacyDungeonRoomSettings)\.cs$') {
                return 'SharedDungeonRoomCoreAndSettings'
            }
            return 'SharedDungeonRoomShapeVariants'
        }
        'SharedContentUiWidgets' {
            if ($path -match '/(GroupOptionButton|UIText|UIHeader)\.cs$') {
                return 'SharedContentUiTextAndOptionWidgets'
            }
            return 'SharedContentUiContainersAndProgress'
        }
        'SharedDungeonStyleCatalog' {
            if ($path -eq 'Terraria.GameContent.Generation.Dungeon/DungeonGenerationStyles.cs') {
                return 'SharedDungeonStyleSetCatalog'
            }
            return 'SharedDungeonStyleEntryDefinitions'
        }
        'SharedPopupAndCombatText' {
            if ($path -eq 'Terraria/PopupText.cs') {
                return 'SharedPopupTextState'
            }
            return 'SharedCombatTextState'
        }
        'SharedShaderData' {
            if ($path -match '/(ShaderData|ScreenShaderData)\.cs$') {
                return 'SharedShaderBaseParameterState'
            }
            return 'SharedShaderFamilyCatalogState'
        }
        'SharedTilePlacementModules' {
            $geometryPaths = @(
                'Terraria.Modules/TileObjectCoordinatesModule.cs',
                'Terraria.Modules/TileObjectBaseModule.cs',
                'Terraria.Modules/TileObjectStyleModule.cs',
                'Terraria.Modules/TileObjectDrawModule.cs'
            )
            if ($geometryPaths -contains $path) {
                return 'SharedTilePlacementGeometryModules'
            }
            return 'SharedTilePlacementAnchorAndHookModules'
        }
        'SharedCreativePowerDefinitions' {
            if ($path -eq 'Terraria.GameContent.Creative/ICreativePower.cs') {
                return 'SharedCreativePowerContracts'
            }
            return 'SharedCreativePowerDefinitionState'
        }
        'UiItemSlotContextDefinitions' {
            if ($Row.Member -match '^(Inventory|Chest|Bank|Trash|Guide|Shop|Mouse|Crafting|InWorld|Void)') {
                return 'UiItemSlotStorageAndCraftingContexts'
            }
            return 'UiItemSlotEquipmentAndCreativeContexts'
        }
        'MapTileStorageAndUpdateState' {
            if ($path -eq 'Terraria.Map/MapUpdateQueue.cs') {
                return 'MapTileUpdateQueueState'
            }
            return 'MapTileEncodingAndStorageState'
        }
        'SharedDungeonGeometryAndPlacementDefinitions' {
            if ($path -in @(
                'Terraria.GameContent.Generation.Dungeon/DungeonBounds.cs',
                'Terraria.GameContent.Generation.Dungeon/DualDungeonUnbreakableWallTiers.cs'
            )) {
                return 'SharedDungeonBoundsAndProgressionDefinitions'
            }
            return 'SharedDungeonDoorAndPlatformDefinitions'
        }
        'SharedItemUseToolAndCombatState' {
            $combatMembers = @(
                'damage', 'knockBack', 'healLife', 'healMana', 'rare', 'shoot', 'shootSpeed', 'lifeRegen',
                'manaIncrease', 'mana', 'crit', 'armorPenetration', 'bonusTagDamage', 'melee', 'magic',
                'ranged', 'summon', 'sentry'
            )
            if ($combatMembers -contains $Row.Member) {
                return 'SharedItemCombatAndDamageCapabilityState'
            }
            return 'SharedItemUseAndToolCapabilityState'
        }
        'SharedWorldItemState' {
            $lifecycleMembers = @(
                'inner', 'ownTime', 'playerIndexTheItemIsReservedFor', 'noGrabDelay', 'shimmered', 'shimmerTime',
                'instanced', 'ownIgnore', 'timeSinceTheItemHasBeenReservedForSomeone',
                'timeLeftInWhichTheItemCannotBeTakenByEnemies', 'timeSinceItemSpawned', 'beingGrabbed',
                'onConveyor', 'keepTime', '_sceneMetrics', 'active'
            )
            if ($lifecycleMembers -contains $Row.Member) {
                return 'SharedWorldItemLifecycleState'
            }
            return 'SharedWorldItemPayloadState'
        }
        'SharedDungeonHallDefinitions' {
            if ($path -match '/(DungeonHall|DungeonHallSettings|RegularDungeonHallSettings|StepBasedDungeonHallSettings|SineDungeonHallSettings)\.cs$') {
                return 'SharedDungeonHallCoreAndSettings'
            }
            return 'SharedDungeonHallLegacyAndGeometry'
        }
        'MainTileAndWallMetadata' {
            if ($Row.Member -match '^wall' -or $Row.Member -eq 'musicFade') {
                return 'MainWallAndGlobalTileMetadata'
            }
            return 'MainTileBehaviorMetadata'
        }
        'SharedTileObjectPlacementDefinitions' {
            if ($path -eq 'Terraria.DataStructures/TileObjectPreviewData.cs') {
                return 'SharedTileObjectPreviewState'
            }
            return 'SharedTileObjectPlacementValueState'
        }
        'LightingEngineState' {
            if ($path -eq 'Terraria.Graphics.Light/LegacyLighting.cs') {
                return 'SharedLegacyLightingState'
            }
            return 'SharedLightingCoordinatorState'
        }
        'SharedItemEquipmentAndPresentationState' {
            $equipmentMembers = @(
                'wornArmor', 'headSlot', 'bodySlot', 'legSlot', 'handOnSlot', 'handOffSlot', 'backSlot',
                'frontSlot', 'shoeSlot', 'waistSlot', 'wingSlot', 'shieldSlot', 'neckSlot', 'faceSlot',
                'balloonSlot', 'beardSlot', 'voiceSlot', 'social', 'vanity', 'newAndShiny', 'hasVanityEffects'
            )
            if ($equipmentMembers -contains $Row.Member) {
                return 'SharedItemEquipmentSlotState'
            }
            return 'SharedItemAppearanceAndTooltipState'
        }
        'SharedItemStaticCatalogRules' {
            $capabilityMembers = @('cachedItemSpawnsByType', 'headType', 'bodyType', 'legType', 'staff', 'claw', '_phaseColors')
            if ($capabilityMembers -contains $Row.Member) {
                return 'SharedItemStaticCapabilityRules'
            }
            return 'SharedItemStaticEconomyAndTimingRules'
        }
        'SharedDungeonGenerationDataState' {
            $scalarMembers = @(
                'wallVariants', 'chandelierItemType', 'platformItemType', 'doorItemType', 'lanternStyles',
                'shelfStyles', 'bannerStyles', 'globalFeatureScalar', 'dungeonStepScalar', 'hallStrengthScalar',
                'hallStepScalar', 'hallInteriorToExteriorRatio', 'hallSlantVariantScalar', 'roomStrengthScalar',
                'roomStepScalar', 'roomInteriorToExteriorRatio', 'roomSlantVariantScalar'
            )
            if ($scalarMembers -contains $Row.Member) {
                return 'SharedDungeonGenerationScalarAndStyleState'
            }
            return 'SharedDungeonGenerationCollectionsState'
        }
        'MainBackgroundAndSeasonalState' {
            if ($Row.Member -match '^(xMas|halloween|forceXMasForToday|forceHalloweenForToday|forceXMasForever|forceHalloweenForever|changeTheTitle)$') {
                return 'MainSeasonalAndTitleState'
            }
            return 'MainBackgroundLayerState'
        }
        'SharedDungeonGenerationQueries' {
            if ($Row.Member -match '^(DOORSTYLE|POTSTYLE|CHANDELIERSTYLE|PLATFORMSTYLE|BANNERSTYLE|TRAPTYPE)_') {
                return 'SharedDungeonStyleConstantQueries'
            }
            return 'SharedDungeonGeometryRuleQueries'
        }
        'SharedWeatherParticleState' {
            if ($path -eq 'Terraria/Star.cs') {
                return 'SharedStarParticleState'
            }
            return 'SharedCloudAndRainParticleState'
        }
        'UiElementTreeState' {
            $interactionMembers = @(
                '_isInitialized', 'IgnoresMouseInteraction', 'PassThroughMouseInteraction', 'OverflowHidden',
                'OverrideSamplerState', 'OverflowHiddenRasterizerState', 'UseImmediateMode', '_snapPoint',
                '_idCounter', 'UniqueId', 'IsMouseHovering'
            )
            if ($Row.Type -eq 'Terraria.UI.UIState' -or $interactionMembers -contains $Row.Member) {
                return 'UiElementInteractionAndLifecycleState'
            }
            return 'UiElementLayoutAndDimensionsState'
        }
        'WorldFileMetadataAndSession' {
            $sessionMembers = @(
                'LoadStatus', 'LoadException', 'DrunkWorld', 'NotTheBees', 'ForTheWorthy', 'Anniversary',
                'DontStarve', 'RemixWorld', 'NoTrapsWorld', 'ZenithWorld', 'SkyblockWorld', 'HasCorruption',
                'IsHardMode', 'DefeatedMoonlord', 'SeedText', 'Seed', 'IsValid', 'WorldSizeName', 'HasCrimson',
                'HasValidSeed', 'UseGuidAsMapName', 'MapFileName'
            )
            if ($sessionMembers -contains $Row.Member) {
                return 'WorldFileSessionAndValidityState'
            }
            return 'WorldFileMetadataIdentityState'
        }
        'WorldFileTilePacking' {
            if ($Row.Member -match '^Header[34]_') {
                return 'WorldFileTileHeaderExtensionState'
            }
            return 'WorldFileTileHeaderCoreState'
        }
        'MinecartMotionState' {
            $decorationMembers = @(
                'TotalFrames', 'LeftDownDecoration', 'RightDownDecoration', 'BouncyBumperDecoration',
                'RegularBumperDecoration', 'NoConnection', 'TopConnection', 'MiddleConnection', 'BottomConnection',
                'BumperEnd', 'BouncyEnd', 'RampEnd', 'OpenEnd', '_texturePosition', '_trackSwitchOptions',
                '_tileHeight'
            )
            if ($decorationMembers -contains $Row.Member) {
                return 'MinecartDecorationAndSwitchState'
            }
            return 'MinecartMotionAndTrackState'
        }
        'PlayerIntentAndMovementState' {
            if ($path -match '/(PlayerMovementAccsCache|PortableStoolUsage)\.cs$') {
                return 'PlayerMovementCapabilityState'
            }
            return 'PlayerIntentAndInteractionState'
        }
        'RecipeDefinitionCatalog' {
            if ($path -eq 'Terraria/RecipeGroup.cs') {
                return 'RecipeGroupCatalogState'
            }
            return 'RecipeDefinitionState'
        }
        'SharedParticleAndGoreEffects' {
            if ($path -eq 'Terraria/Dust.cs') {
                return 'SharedDustParticleState'
            }
            return 'SharedGoreEffectState'
        }
        'MainDerivedPropertiesAndEvents' {
            $inputAndPresentationMembers = @(
                'UIScale', 'IsItRaining', 'MouseScreen', 'MouseWorld', 'ActiveNetDiagnosticsUI',
                'PlayerSceneMetrics', 'WindForVisuals', 'ChatLineWidthLimit', 'BlackFadeDist', 'IsRainingForever'
            )
            if ($inputAndPresentationMembers -contains $Row.Member) {
                return 'MainDerivedInputAndPresentationQueries'
            }
            return 'MainDerivedWorldAndSessionQueries'
        }
        'SharedChatPresentation' {
            if ($path -match '/(ChatManager|ChatMessageContainer|RemadeChatMonitor)\.cs$') {
                return 'SharedChatMonitorAndCommandState'
            }
            return 'SharedChatSnippetPresentationState'
        }
        'SharedFishingAttemptAndConditions' {
            if ($path -eq 'Terraria.DataStructures/PlayerFishingConditions.cs') {
                return 'PlayerFishingConditionState'
            }
            return 'FishingAttemptState'
        }
        'TileCellStorage' {
            if ($Row.Member -match '^(sTileHeader|bTileHeader|frameX|frameY|Bit|Type_)') {
                return 'TileCellFrameAndBitState'
            }
            return 'TileCellMaterialAndLiquidState'
        }
        'CameraAndVertexPresentation' {
            if ($path -match '/(VertexStrip|VertexColors)\.cs$') {
                return 'VertexStripAndColorState'
            }
            return 'CameraMatrixAndViewportState'
        }
        'WorldFileRecoveryIo' {
            if ($Row.Member -match '^(_temp|TempParty)') {
                return 'WorldFileTemporaryEventState'
            }
            return 'WorldFileRecoveryVersionState'
        }
        'SharedAmbientSkyAndWindState' {
            if ($path -match '/(AmbienceServer|AmbientWindSystem)\.cs$') {
                return 'SharedAmbientSpawnAndWindState'
            }
            return 'SharedAmbientSkyCatalogState'
        }
        'SharedCreativePowerPresentation' {
            if ($path -eq 'Terraria.GameContent.Creative/CreativePowerUIElementRequestInfo.cs') {
                return 'SharedCreativePowerUiLayoutState'
            }
            return 'SharedCreativePowerIconCatalogState'
        }
        'SharedDungeonLegacyGenerationGlobals' {
            $ruleMembers = @(
                'dungeonStyle', 'dungeonGenerationStyles', 'dungeonDitherSnake', 'isCrackedBrick',
                'isPitTrapTile', 'isDungeonTile', 'isDungeonWall', 'isDungeonWallGlass', 'GeneratingDungeon',
                'preGenDungeonEntranceSettings', 'desertChestLootState'
            )
            if ($ruleMembers -contains $Row.Member) {
                return 'SharedDungeonLegacyRuleState'
            }
            return 'SharedDungeonLegacyPlacementState'
        }
        'SharedGeneralPureUtilities' {
            if ($Row.Member -match '^(charLengths|_substitutionRegex|RANDOM_|_floodFill)') {
                return 'SharedGeneralRandomAndBufferUtilities'
            }
            return 'SharedGeneralTeleportAndInterceptionUtilities'
        }
        'SharedSeasonalWorldEventState' {
            if ($path -match '/(BirthdayParty|LanternNight|MysticLogFairiesEvent)\.cs$') {
                return 'SharedCelebrationAndLanternEvents'
            }
            return 'SharedRitualAndStormEvents'
        }
        'UiItemSlotPresentationAndPulseState' {
            if ($Row.Type -like '*.PulseEffect' -or $Row.Member -match '(?i)(Pulse|Glow|Highlight)') {
                return 'UiItemSlotPulseAndHighlightState'
            }
            return 'UiItemSlotDisplayAndInteractionState'
        }
        default {
            throw "No second-level fine subsystem mapping for baseline '$BaseFineSubsystem', path '$path', type '$($Row.Type)', member '$($Row.Member)'."
        }
    }
}

$thirdLevelBaselineIds = @(
    'UiItemSortingDefinitions', 'MapTileEncodingAndStorageState',
    'SharedContentUiTextAndOptionWidgets', 'SharedDungeonRoomCoreAndSettings',
    'SharedDungeonHallCoreAndSettings'
)

$fourthLevelBaselineIds = @(
    'TileObjectDefinitionPlacementAndStyleState', 'SharedTimeLoggerRenderMetricsState',
    'SharedDropRuleSelectionAndChainState', 'SharedBiomeDungeonAndTerrainState',
    'SharedFishingDropCatalogState', 'MainCageBirdAndTerrestrialAnimationState',
    'UiItemSortingLayerCatalogState', 'SharedSceneScanAndZoneState',
    'SharedCreativePowerDefinitionState', 'SharedDungeonStyleEntryDefinitions',
    'SharedSceneAggregateAndDecorationState', 'AudioLegacySoundInstanceState',
    'SharedDungeonRoomShapeVariants', 'AudioLegacySoundCatalogState',
    'MapEncodingCatalogAndIoState', 'MainTileBehaviorMetadata',
    'UiItemSlotEquipmentAndCreativeContexts', 'SharedItemStaticEconomyAndTimingRules',
    'SharedPopupTextState', 'MainBackgroundLayerState',
    'SharedShaderFamilyCatalogState', 'SharedDungeonStyleConstantQueries',
    'SharedTilePlacementGeometryModules', 'SharedWorldItemPayloadState',
    'NetworkRemoteClientState', 'NetworkSessionCoordinatorState',
    'ItemEmergencyStackingState', 'BestiaryInfoElementsAndProviders',
    'EntityAuthoritativeState', 'SharedCreativePowerIconCatalogState'
)

$fifthLevelBaselineIds = @(
    'SharedTimeLoggerPhaseMetricsState', 'TileObjectStyleAndDrawState',
    'SharedFishingConditionCatalogState', 'SharedDropRuleSelectionAndConditionState',
    'SharedDungeonControlAndTrapState', 'SharedSceneZoneDefinitionState',
    'TileObjectPlacementRuleState', 'MainCageTerrestrialCritterAnimationState',
    'SharedAudioLegacySoundCatalogInstanceState', 'SharedAudioLegacySoundDefinitionCatalogState',
    'MapEncodingHeaderCatalogState', 'UiItemSlotCreativeCraftingAndUtilityContexts'
)

$sixthLevelBaselineIds = @(
    'SharedTimeLoggerWorldRenderPhaseMetricsState',
    'SharedFishingEnvironmentConditionCatalogState',
    'SharedDropRuleSelectionAndQuantityState',
    'TileObjectStyleCatalogState',
    'SharedDungeonRoomGeometryState',
    'UiItemSortingCombatAndEquipmentCatalogState',
    'SharedDungeonStyleFurnitureAndRoomState',
    'SharedAudioLegacyEnvironmentalInstanceState',
    'SharedItemUseAndToolCapabilityState',
    'SharedSceneBiomeAndEventDefinitionState',
    'SharedTilePaintState',
    'SharedInvasionEventState',
    'SharedStartupAndIssueReporting'
)

function Get-ThirdLevelFineSubsystemId {
    param(
        [Parameter(Mandatory)][pscustomobject]$Row,
        [Parameter(Mandatory)][string]$SecondLevelFineSubsystem
    )

    switch ($SecondLevelFineSubsystem) {
        'UiItemSortingDefinitions' {
            if ($Row.Type -in @('Terraria.UI.ItemSorting.ItemSortingLayer', 'Terraria.UI.ItemSorting.ItemSortingLayers')) {
                return 'UiItemSortingLayerCatalogState'
            }
            return 'UiItemSortingRegistryAndRankingState'
        }
        'MapTileEncodingAndStorageState' {
            if ($Row.Type -eq 'Terraria.Map.MapHelper') {
                return 'MapEncodingCatalogAndIoState'
            }
            return 'MapTileCellState'
        }
        'SharedContentUiTextAndOptionWidgets' {
            if ($Row.Type -like 'Terraria.GameContent.UI.Elements.GroupOptionButton*') {
                return 'SharedContentUiOptionButtonState'
            }
            return 'SharedContentUiTextAndHeaderState'
        }
        'SharedDungeonRoomCoreAndSettings' {
            if ($Row.Type -eq 'Terraria.GameContent.Generation.Dungeon.Rooms.DungeonRoom') {
                return 'SharedDungeonRoomCoreState'
            }
            return 'SharedDungeonRoomSettingsState'
        }
        'SharedDungeonHallCoreAndSettings' {
            if ($Row.Type -eq 'Terraria.GameContent.Generation.Dungeon.Halls.DungeonHall') {
                return 'SharedDungeonHallCoreState'
            }
            return 'SharedDungeonHallSettingsState'
        }
        default {
            throw "No third-level fine subsystem mapping for peer '$SecondLevelFineSubsystem', path '$($Row.RelativePath)', type '$($Row.Type)', member '$($Row.Member)'."
        }
    }
}

function Get-FourthLevelFineSubsystemId {
    param(
        [Parameter(Mandatory)][pscustomobject]$Row,
        [Parameter(Mandatory)][string]$ThirdLevelFineSubsystem
    )

    $member = $Row.Member
    $type = $Row.Type
    $path = $Row.RelativePath

    switch ($ThirdLevelFineSubsystem) {
        'TileObjectDefinitionPlacementAndStyleState' {
            if ($member -match '(?i)(Style|Draw|Coordinate|Width|Height|Origin|Direction|RandomStyle|SpecificRandom|FlattenAnchors)') {
                return 'TileObjectStyleAndDrawState'
            }
            return 'TileObjectPlacementRuleState'
        }
        'SharedTimeLoggerRenderMetricsState' {
            if ($member -match 'Format$') {
                return 'SharedTimeLoggerDisplayFormattingState'
            }
            return 'SharedTimeLoggerPhaseMetricsState'
        }
        'SharedDropRuleSelectionAndChainState' {
            if (($member -match '^(ChainedRules|RuleToChain|hideLootReport)$') -or ($type -like '*.Chains.*')) {
                return 'SharedDropRuleChainState'
            }
            return 'SharedDropRuleSelectionAndConditionState'
        }
        'SharedBiomeDungeonAndTerrainState' {
            if ($path -match '/(DungeonControlLine|DeadMansChestBiome)\.cs$') {
                return 'SharedDungeonControlAndTrapState'
            }
            return 'SharedBiomeTerrainPassState'
        }
        'SharedFishingDropCatalogState' {
            if ($type -match '(FishDropRule|FishPossibilityEntry|FishDropRuleList)$') {
                return 'SharedFishingDropResolutionState'
            }
            return 'SharedFishingConditionCatalogState'
        }
        'MainCageBirdAndTerrestrialAnimationState' {
            if ($member -match '^(mallard|duck|grebe|seagull|bird|redBird|blueBird|macaw|penguin|owl)Cage') {
                return 'MainCageBirdAnimationState'
            }
            return 'MainCageTerrestrialCritterAnimationState'
        }
        'UiItemSortingLayerCatalogState' {
            if ($member -match '^(Weapons|Tools|Armor|Equip|Name$|SortingMethod$)') {
                return 'UiItemSortingCombatAndEquipmentCatalogState'
            }
            return 'UiItemSortingConsumableAndMiscCatalogState'
        }
        'SharedSceneScanAndZoneState' {
            if ($member -match '^(BestOreType|NPCBannerBuff|hasBanner|CanPlayCreditsRoll|ClosestNPCPosition|_dummyPlayer|_tileCounts|_liquidCounts|LastScanTime|Center|TileCenter|BestOrePosition|PerspectivePlayer)$') {
                return 'SharedSceneScanAccumulatorState'
            }
            return 'SharedSceneZoneDefinitionState'
        }
        'SharedCreativePowerDefinitionState' {
            if (($type -match '\.APerPlayer') -or ($type -match 'SpawnRateSliderPerPlayerPower$')) {
                return 'SharedCreativePerPlayerPowerState'
            }
            return 'SharedCreativeSharedPowerState'
        }
        'SharedDungeonStyleEntryDefinitions' {
            if ($member -match '^(Style|UnbreakableWallProgressionTier|BrickTileType|BrickGrassTileType|BrickCrackedTileType|BrickWallType|WindowGlassWallType|WindowClosedGlassWallType|WindowEdgeWallType|PitTrapTileType|LiquidType|EdgeDither)$') {
                return 'SharedDungeonStyleMaterialAndGeometryState'
            }
            return 'SharedDungeonStyleFurnitureAndRoomState'
        }
        'SharedSceneAggregateAndDecorationState' {
            if ($member -match '^(ActiveMusicBox|MusicBoxSilence|WaterCandle|PeaceCandle|ShadowCandle|PartyMonolith|HasSunflower|HasGardenGnome|HasClock|HasCampfire|HasStarInBottle|HasHeartLantern|ActiveFountainColor|ActiveMonolithType|BloodMoonMonolith|MoonLordMonolith|EchoMonolith|ShimmerMonolithState|CRTMonolith|RetroMonolith|NoirMonolith|RadioThingMonolith|HasCatBast|BehindBackwall)$') {
                return 'SharedSceneDecorationAndAudioState'
            }
            return 'SharedSceneTileAggregateState'
        }
        'AudioLegacySoundInstanceState' {
            if ($member -match '^(_trackedInstances|TrackableSoundInstances)$') {
                return 'SharedAudioLegacyTrackedInstanceState'
            }
            return 'SharedAudioLegacySoundCatalogInstanceState'
        }
        'SharedDungeonRoomShapeVariants' {
            if ($member -match '^(VARIANT_|MAX_VARIANTS|BIOMEROOM_)') {
                return 'SharedDungeonRoomVariantCatalogState'
            }
            return 'SharedDungeonRoomGeometryState'
        }
        'AudioLegacySoundCatalogState' {
            if ($member -match '^(_services|TrackableSounds)$') {
                return 'SharedAudioLegacySoundServicesState'
            }
            return 'SharedAudioLegacySoundDefinitionCatalogState'
        }
        'MapEncodingCatalogAndIoState' {
            if ($member -match '^(IOLock|sceneArea|sceneSnowiness|zlibDecompress)$') {
                return 'MapIoRuntimeState'
            }
            return 'MapEncodingHeaderCatalogState'
        }
        'MainTileBehaviorMetadata' {
            if ($member -match '^(tileLighted|tileShine|tileShine2|tileBlockLight|tileNoSunLight|tileDungeon|tileSpelunker|tileLargeFrames|tileBrick|tileMoss|tileCracked|tileObsidianKill|tileFrameImportant|tilePile|tileBlendAll)$') {
                return 'MainTileLightingAndFrameMetadata'
            }
            return 'MainTileBehaviorAndInteractionMetadata'
        }
        'UiItemSlotEquipmentAndCreativeContexts' {
            if ($member -match '^(PrefixItem|Equip|DisplayDoll|HatRack|GoldDebug)$') {
                return 'UiItemSlotEquipmentAndDisplayContexts'
            }
            return 'UiItemSlotCreativeCraftingAndUtilityContexts'
        }
        'SharedItemStaticEconomyAndTimingRules' {
            if ($member -match '^(copper|silver|gold|platinum|goldCritterRarityColor|shadowOrbPrice|dungeonPrice|queenBeePrice|hellPrice|eclipsePrice|eclipsePostPlanteraPrice|eclipseMothronPrice)$') {
                return 'SharedItemEconomyAndValueRules'
            }
            return 'SharedItemUseTimingAndStackRules'
        }
        'SharedPopupTextState' {
            if ($member -match '^(maxItemText|popupText|name|displayText|stack|coinText|coinValue|sonarText|expert|master|sonar|context|npcNetID|freeAdvanced)$') {
                return 'SharedPopupTextContentAndContextState'
            }
            return 'SharedPopupTextRenderLifecycleState'
        }
        'MainBackgroundLayerState' {
            if ($member -match '^(essScale|essDir|treeX|treeStyle|caveBackX|caveBackStyle|iceBackStyle|hellBackStyle|jungleBackStyle)$') {
                return 'MainBackgroundParallaxAndStyleState'
            }
            return 'MainBackgroundLayerCatalogState'
        }
        'SharedShaderFamilyCatalogState' {
            if (($type -match 'GameShaders$') -or ($type -match 'ShaderDataSet$')) {
                return 'SharedShaderRegistryAndLookupState'
            }
            return 'SharedShaderFamilyDataState'
        }
        'SharedDungeonStyleConstantQueries' {
            if ($member -match '^(BANNERSTYLE_|TRAPTYPE_)') {
                return 'SharedDungeonStyleBannerAndTrapConstants'
            }
            return 'SharedDungeonStyleObjectConstants'
        }
        'SharedTilePlacementGeometryModules' {
            if ($type -match '(TileObjectCoordinatesModule|TileObjectDrawModule)$') {
                return 'SharedTilePlacementCoordinateAndDrawModules'
            }
            return 'SharedTilePlacementBaseAndStyleModules'
        }
        'SharedWorldItemPayloadState' {
            if ($member -match '^(type|stack|favorited|maxStack|value|rare|Name|IsACoin|IsAir)$') {
                return 'SharedWorldItemIdentityAndEconomyPayloadState'
            }
            return 'SharedWorldItemUseAndPresentationPayloadState'
        }
        'NetworkRemoteClientState' {
            if ($member -match '^(Socket|Id|Name|IsActive|PendingTermination|PendingTerminationApproved|IsAnnouncementCompleted|State|TimeOutTimer|StatusText|StatusText2|StatusCount|StatusMax)$') {
                return 'NetworkRemoteClientConnectionAndStatusState'
            }
            return 'NetworkRemoteClientSectionAndRateLimitState'
        }
        'NetworkSessionCoordinatorState' {
            if ($member -match '^(MaxConnections|NetBufferSize|DefaultPort|BanFilePath|ServerPassword|ServerIP|ServerIPText|IsHostAndPlay|HostToken|UseUPNP|SaveOnServerExit|HandshakeLoggingEnabled)$') {
                return 'NetworkSessionConfigurationState'
            }
            return 'NetworkSessionTransportAndThreadState'
        }
        'ItemEmergencyStackingState' {
            if ($type -eq 'Terraria.GameContent.EmergencyStacking') {
                return 'SharedItemEmergencyStackingPolicyState'
            }
            return 'SharedItemEmergencyStackingTransferState'
        }
        'BestiaryInfoElementsAndProviders' {
            if ($type -match 'InfoElement$') {
                return 'SharedBestiaryInfoElementState'
            }
            return 'SharedBestiaryCollectionProviderState'
        }
        'EntityAuthoritativeState' {
            if ($member -match '^(whoAmI|position|velocity|oldPosition|oldVelocity|oldDirection|direction)$') {
                return 'EntityIdentityAndMotionState'
            }
            return 'EntityBoundsAndFluidState'
        }
        'SharedCreativePowerIconCatalogState' {
            if ($member -match '^(TextureIconColumns|TextureIconRows|CommonSelectedColor)$') {
                return 'SharedCreativePowerIconLayoutState'
            }
            return 'SharedCreativePowerIconLocationCatalogState'
        }
        default {
            throw "No fourth-level fine subsystem mapping for peer '$ThirdLevelFineSubsystem', path '$path', type '$type', member '$member'."
        }
    }
}

function Get-FifthLevelFineSubsystemId {
    param(
        [Parameter(Mandatory)][pscustomobject]$Row,
        [Parameter(Mandatory)][string]$FourthLevelFineSubsystem
    )

    $member = $Row.Member
    $type = $Row.Type
    $path = $Row.RelativePath

    switch ($FourthLevelFineSubsystem) {
        'SharedTimeLoggerPhaseMetricsState' {
            if ($member -match '^(PlayerChat|NPCs|Projectiles|Players|Items|Rain|Gore|Dust|Particles|LeashedEntities|Interface|DrawFPSGraph|DrawTimeLogger|Overlays|Filters|SunVisibility|MenuDrawTime|SplashDrawTime|DrawFullscreenMap|GCPause)$') {
                return 'SharedTimeLoggerEntityAndInterfacePhaseMetricsState'
            }
            if ($member -match '^(TotalDrawAndUpdate|DrawSolidTiles|FlushSolidTiles|SolidDrawCalls|DrawNonSolidTiles|FlushNonSolidTiles|NonSolidDrawCalls|DrawBlackTiles|DrawWallTiles|FlushWallTiles|WallDrawCalls|DrawWaterTiles|LiquidDrawCalls|DrawBackgroundWaterTiles|LiquidBackgroundDrawCalls|DrawUndergroundBackground|DrawOldUndergroundBackground|DrawWireTiles|ClothingRacks|TileExtras|Nature|RenderSolidTiles|RenderNonSolidTiles|RenderBlacksAndWalls|RenderUndergroundBackground|RenderBackgroundLiquid|RenderLiquid|TotalDrawByRenderCount|TotalDrawRenderNow|TotalDraw|Lighting|LightingInit|LightingByPass|FindPaintedTiles|PrepareRequests|FindingWaterfalls|MapChanges|MapSectionUpdate|MapUpdate|SectionFraming|SectionRefresh|SkyBackground|SunMoonStars|SurfaceBackground|Map|Waterfalls)$') {
                return 'SharedTimeLoggerWorldRenderPhaseMetricsState'
            }
        }
        'TileObjectStyleAndDrawState' {
            if (($member -match '^Style') -or ($member -match '^styleLineSkip') -or
                ($member -match '^(_tileObjectStyle|_hasOwnTileObjectStyle|GetStyleOverride|RandomStyleRange|SpecificRandomStyles)$')) {
                return 'TileObjectStyleCatalogState'
            }
            if ($member -match '^(_tileObjectDraw|_hasOwnTileObjectDraw|DrawYOffset|DrawXOffset|DrawFlipHorizontal|DrawFlipVertical|DrawStepDown|Width|Height|Origin|Direction|FlattenAnchors|CoordinateHeights|DrawFrameOffsets|CoordinateWidth|CoordinatePadding|CoordinatePaddingFix|CoordinateFullWidth|CoordinateFullHeight|DrawStyleOffset)$') {
                return 'TileObjectDrawGeometryState'
            }
        }
        'SharedFishingConditionCatalogState' {
            if ($type -match '(\.Rarity|FishRarityCondition|DelegateFishingRarityCondition)$') {
                return 'SharedFishingRarityConditionCatalogState'
            }
            if ($type -match 'AFishDropRulePopulator(?:$|\.DelegateFishingCondition$)') {
                return 'SharedFishingEnvironmentConditionCatalogState'
            }
        }
        'SharedDropRuleSelectionAndConditionState' {
            if (($path -match '/Conditions\.cs$') -or ($member -match '^(condition|dummyCondition)$')) {
                return 'SharedDropRuleConditionBranchState'
            }
            if ($path -match '^Terraria\.GameContent\.ItemDropRules/') {
                return 'SharedDropRuleSelectionAndQuantityState'
            }
        }
        'SharedDungeonControlAndTrapState' {
            if ($path -match '/DeadMansChestBiome\.cs$') {
                return 'SharedDungeonTrapPlacementState'
            }
            if ($path -match '/DungeonControlLine\.cs$') {
                return 'SharedDungeonControlLineGeometryState'
            }
        }
        'SharedSceneZoneDefinitionState' {
            if ($member -match '^(AssumedConstantScreenSize|ZoneScanPadding|ZoneScanSize|TownNPCRectSize|SnowTileMax|MushroomTileThreshold|BelowSurface|ZoneSkyHeight|ZoneOverworldHeight|ZoneDirtLayerHeight|ZoneRockLayerHeight|ZoneUnderworldHeight)$') {
                return 'SharedSceneZoneGeometryAndThresholdState'
            }
            if ($member -match '^(ZoneCorrupt|ZoneCrimson|ZoneHallow|ZoneJungle|ZoneSnow|ZoneDesert|ZoneGlowshroom|ZoneMeteor|ZoneGraveyard|ZoneDungeon|ZoneLihzhardTemple|ZoneGranite|ZoneMarble|ZoneHive|ZoneGemCave|ZoneBeach|ZoneUndergroundDesert|ZoneRain|ZoneSandstorm|SurfaceAtmospherics|UndergroundForShimmering|ZoneShimmer|ZoneWaterCandle|ZonePeaceCandle|ZoneShadowCandle|InTorchGodMinigame)$') {
                return 'SharedSceneBiomeAndEventDefinitionState'
            }
        }
        'TileObjectPlacementRuleState' {
            if ($member -match '^(_usesCustomCanPlace|_useGlobalLiquidChecks|_anchor|_anchorTiles|_liquidDeath|_liquidPlacement|_hasOwnAnchor|_hasOwnAnchorTiles|_hasOwnLiquidDeath|_hasOwnLiquidPlacement)$' -or
                $member -match '^(UsesCustomCanPlace|UsesGlobalLiquidChecks|Anchor|AnchorTop|AnchorBottom|AnchorLeft|AnchorRight|AnchorWall|AnchorValidTiles|AnchorInvalidTiles|AnchorAlternateTiles|AnchorValidWalls|WaterDeath|LavaDeath|WaterPlacement|LavaPlacement)$') {
                return 'TileObjectAnchorAndLiquidPlacementState'
            }
            if ($member -match '^(_placementHooks|_subTiles|_tileObjectBase|_tileObjectCoords|_hasOwnPlacementHooks|_hasOwnSubTiles|_hasOwnTileObjectBase|_hasOwnTileObjectCoords|HookCheckIfCanPlace|HookPostPlaceEveryone|HookPostPlaceMyPlayer|HookPlaceOverride|SubTiles)$') {
                return 'TileObjectPlacementHookAndBaseState'
            }
        }
        'MainCageTerrestrialCritterAnimationState' {
            if ($member -match '^(cageFrames|critterCage|bunnyCage|squirrelCage|snailCage|snail2Cage|mouseCage|turtleCage|ratCage)') {
                return 'MainCageMammalAndReptileAnimationState'
            }
            if ($member -match '^(butterflyCage|dragonflyJar|scorpionCage|fairyJar|wormCage|maggotCage|ladybugCage|slugCage|grasshopperCage)') {
                return 'MainCageInsectAndSmallCritterAnimationState'
            }
        }
        'SharedAudioLegacySoundCatalogInstanceState' {
            if ($member -match '^SoundInstance(PlayerHit|FemaleHit|PlayerKilled|MenuOpen|MenuClose|MenuTick|Camera|Chat|MaxMana)$') {
                return 'SharedAudioLegacyPlayerAndInterfaceInstanceState'
            }
            if ($member -match '^SoundInstance') {
                return 'SharedAudioLegacyEnvironmentalInstanceState'
            }
        }
        'SharedAudioLegacySoundDefinitionCatalogState' {
            if ($member -match '^Sound(PlayerHit|FemaleHit|PlayerKilled|MenuOpen|MenuClose|MenuTick|Camera|Chat|MaxMana)$') {
                return 'SharedAudioLegacyInterfaceSoundDefinitionCatalogState'
            }
            if ($member -match '^Sound') {
                return 'SharedAudioLegacyGameplaySoundDefinitionCatalogState'
            }
        }
        'MapEncodingHeaderCatalogState' {
            if ($member -match '^Header') {
                return 'MapEncodingHeaderBitCatalogState'
            }
            if ($member -match '^(drawLoopMilliseconds|maxTileOptions|maxWallOptions|maxLiquidTypes|maxSkyGradients|maxDirtGradients|maxRockGradients|MapChunkSize)$') {
                return 'MapEncodingOptionLimitState'
            }
        }
        'UiItemSlotCreativeCraftingAndUtilityContexts' {
            if ($member -match '^(CreativeInfinite|CreativeSacrifice|CreativeInfiniteLocked|NewCraftingUIRecipe|NewCraftingUICraftSlot|NewCraftingUIMaterial)$') {
                return 'UiItemSlotCreativeAndCraftingContextState'
            }
            if ($member -match '^(Equip|Hotbar|ChatItem|DisplayDoll|HatRack|EquipMiscDye|BannerClaiming|OverdrawGlow|Count)') {
                return 'UiItemSlotHotbarDisplayAndUtilityContextState'
            }
        }
        default {
            throw "No fifth-level fine subsystem mapping for peer '$FourthLevelFineSubsystem', path '$path', type '$type', member '$member'."
        }
    }

    throw "No fifth-level fine subsystem mapping for peer '$FourthLevelFineSubsystem', path '$path', type '$type', member '$member'."
}

function Get-SixthLevelFineSubsystemId {
    param(
        [Parameter(Mandatory)][pscustomobject]$Row,
        [Parameter(Mandatory)][string]$FifthLevelFineSubsystem
    )

    $member = $Row.Member
    $type = $Row.Type
    $path = $Row.RelativePath

    switch ($FifthLevelFineSubsystem) {
        'SharedTimeLoggerWorldRenderPhaseMetricsState' {
            if ($member -match '^(TotalDrawAndUpdate|DrawSolidTiles|FlushSolidTiles|SolidDrawCalls|DrawNonSolidTiles|FlushNonSolidTiles|NonSolidDrawCalls|DrawBlackTiles|DrawWallTiles|FlushWallTiles|WallDrawCalls|DrawWaterTiles|LiquidDrawCalls|DrawBackgroundWaterTiles|LiquidBackgroundDrawCalls|DrawWireTiles|ClothingRacks|TileExtras|Nature|RenderSolidTiles|RenderNonSolidTiles|RenderBlacksAndWalls|RenderBackgroundLiquid|RenderLiquid)$') {
                return 'SharedTimeLoggerTileAndLiquidRenderMetricsState'
            }
            return 'SharedTimeLoggerLightingMapAndBackgroundMetricsState'
        }
        'SharedFishingEnvironmentConditionCatalogState' {
            if ($member -in @('_condition', '_list', 'HardMode', 'EarlyMode', 'Junk', 'Crate', 'AnyEnemies', 'DidNotUseCombatBook')) {
                return 'SharedFishingConditionCatalogPopulationState'
            }
            return 'SharedFishingEnvironmentPredicateState'
        }
        'SharedDropRuleSelectionAndQuantityState' {
            if ($type -match '\.(CommonDrop|CommonDropWithRerolls|DropOneByOne)(\.Parameters)?$') {
                return 'SharedDropRuleChanceAndQuantityState'
            }
            return 'SharedDropRuleOptionSelectionState'
        }
        'TileObjectStyleCatalogState' {
            if ($member -match '^(_tileObjectStyle|_hasOwnTileObjectStyle|GetStyleOverride|RandomStyleRange|SpecificRandomStyles|styleLineSkipVisualOverride|StyleLineSkip|StyleWrapLimitVisualOverride)$') {
                return 'TileObjectStyleSelectionAndOverrideState'
            }
            return 'TileObjectStyleDefinitionCatalogState'
        }
        'SharedDungeonRoomGeometryState' {
            if ($member -match '^(Position|RoomInnerSize|RoomOuterSize|WallDepth|StartPosition|EndPosition|Strength|_innerBoundsSize)$') {
                return 'SharedDungeonRoomPlacementGeometryState'
            }
            return 'SharedDungeonRoomShapeGeometryState'
        }
        'UiItemSortingCombatAndEquipmentCatalogState' {
            if ($member -match '^(Armor|Equip)') {
                return 'UiItemSortingArmorAndAccessoryCatalogState'
            }
            return 'UiItemSortingWeaponAndToolCatalogState'
        }
        'SharedDungeonStyleFurnitureAndRoomState' {
            if ($member -match '^(BiomeRoomType|SubStyles)$') {
                return 'SharedDungeonStyleRoomVariantState'
            }
            return 'SharedDungeonStyleFurnitureCatalogState'
        }
        'SharedAudioLegacyEnvironmentalInstanceState' {
            if ($member -match '^SoundInstance(Grab|Item|NpcHit|NpcKilled|Zombie|Roar|DoubleJump|Run|Coins|Unlock)$') {
                return 'SharedAudioLegacyEntityFeedbackInstanceState'
            }
            return 'SharedAudioLegacyWorldEnvironmentInstanceState'
        }
        'SharedItemUseAndToolCapabilityState' {
            if ($member -match '^(pick|axe|hammer|tileBoost|createTile|createWall|placeStyle|ammo|notAmmo|useAmmo|material)$') {
                return 'SharedItemToolPlacementCapabilityState'
            }
            return 'SharedItemUseTimingAndConsumptionState'
        }
        'SharedSceneBiomeAndEventDefinitionState' {
            if ($member -match '^(ZoneRain|ZoneSandstorm|SurfaceAtmospherics|UndergroundForShimmering|ZoneShimmer|ZoneWaterCandle|ZonePeaceCandle|ZoneShadowCandle|InTorchGodMinigame)$') {
                return 'SharedSceneWeatherAndEventZoneState'
            }
            return 'SharedSceneBiomeZoneDefinitionState'
        }
        'SharedTilePaintState' {
            if ($type -match '(RenderTargetHolder|TilePaintSystemV2)$') {
                return 'SharedTilePaintRenderTargetState'
            }
            return 'SharedTilePaintVariationAndColorState'
        }
        'SharedInvasionEventState' {
            if (($type -match 'DamageTracker$') -or ($member -eq '_damageTracker')) {
                return 'SharedInvasionDamageTrackingState'
            }
            return 'SharedInvasionWaveAndArenaState'
        }
        'SharedStartupAndIssueReporting' {
            if ($path -match '(GeneralIssueReporter|IssueReport)\.cs$') {
                return 'SharedIssueReportCatalogState'
            }
            return 'SharedStartupAndRuntimeHostState'
        }
        default {
            throw "No sixth-level fine subsystem mapping for peer '$FifthLevelFineSubsystem', path '$path', type '$type', member '$member'."
        }
    }
}

function Get-FineSubsystemId {
    param([Parameter(Mandatory)][pscustomobject]$Row)

    $baseFineSubsystem = Get-BaseFineSubsystemId -Row $Row
    $secondLevelFineSubsystem = $baseFineSubsystem
    if ($secondLevelBaselineIds -contains $baseFineSubsystem) {
        $secondLevelFineSubsystem = Get-SecondLevelFineSubsystemId -Row $Row -BaseFineSubsystem $baseFineSubsystem
    }
    if ($thirdLevelBaselineIds -contains $secondLevelFineSubsystem) {
        $thirdLevelFineSubsystem = Get-ThirdLevelFineSubsystemId -Row $Row -SecondLevelFineSubsystem $secondLevelFineSubsystem
    } else {
        $thirdLevelFineSubsystem = $secondLevelFineSubsystem
    }
    $fourthLevelFineSubsystem = $thirdLevelFineSubsystem
    if ($fourthLevelBaselineIds -contains $thirdLevelFineSubsystem) {
        $fourthLevelFineSubsystem = Get-FourthLevelFineSubsystemId -Row $Row -ThirdLevelFineSubsystem $thirdLevelFineSubsystem
    }
    $fifthLevelFineSubsystem = $fourthLevelFineSubsystem
    if ($fifthLevelBaselineIds -contains $fourthLevelFineSubsystem) {
        $fifthLevelFineSubsystem = Get-FifthLevelFineSubsystemId -Row $Row -FourthLevelFineSubsystem $fourthLevelFineSubsystem
    }
    if ($sixthLevelBaselineIds -contains $fifthLevelFineSubsystem) {
        return Get-SixthLevelFineSubsystemId -Row $Row -FifthLevelFineSubsystem $fifthLevelFineSubsystem
    }
    return $fifthLevelFineSubsystem
}

$fineDefinitions = @(
    [pscustomobject]@{ Id = 'MainFrameAndPlatformState'; Parent = 'RuntimeComposition'; Role = 'runtime state'; Seam = 'global frame snapshot / platform adapter'; Responsibility = '帧级活动计数和平台调用边界。' }
    [pscustomobject]@{ Id = 'MainBootstrapAndSessionFlags'; Parent = 'RuntimeComposition'; Role = 'runtime state'; Seam = 'startup/session state view'; Responsibility = '启动、版本、种子、收藏和全局会话引用。' }
    [pscustomobject]@{ Id = 'MainCameraAndVisualOffsets'; Parent = 'RuntimeComposition'; Role = 'presentation state'; Seam = 'camera/input projection'; Responsibility = '相机插值、手持/头饰偏移和屏幕平移数据。' }
    [pscustomobject]@{ Id = 'MainInputAndThreadScheduling'; Parent = 'RuntimeComposition'; Role = 'runtime state'; Seam = 'main-thread command queue'; Responsibility = '输入、主线程动作队列、帧率和背景参数。' }
    [pscustomobject]@{ Id = 'MainBackgroundAndMapPresentation'; Parent = 'RuntimeComposition'; Role = 'presentation state'; Seam = 'map/background projection'; Responsibility = '背景层、地图刷新和图形设备表现状态。' }
    [pscustomobject]@{ Id = 'MainWorldBoundsAndSimulationRates'; Parent = 'RuntimeComposition'; Role = 'runtime state'; Seam = 'world geometry/rate view'; Responsibility = '世界边界、容量、生成和模拟速率配置。' }
    [pscustomobject]@{ Id = 'MainSceneWeatherAndCalendarState'; Parent = 'RuntimeComposition'; Role = 'runtime state'; Seam = 'scene/weather query'; Responsibility = '场景指标、昼夜、月相、天气和风状态。' }
    [pscustomobject]@{ Id = 'MainTileMetadataAndCageFrames'; Parent = 'RuntimeComposition'; Role = 'runtime state'; Seam = 'tile metadata view'; Responsibility = 'Tile 元数据数组、捕获物容器帧和格点表现缓存。' }
    [pscustomobject]@{ Id = 'MainEntityPoolsAndWorldSlots'; Parent = 'RuntimeComposition'; Role = 'runtime state'; Seam = 'entity-pool view'; Responsibility = '实体池、容器槽和动画注册表。' }
    [pscustomobject]@{ Id = 'MainPlayerInputAndInventorySlots'; Parent = 'RuntimeComposition'; Role = 'runtime state'; Seam = 'local-player/input view'; Responsibility = '屏幕输入、鼠标物品、本地玩家和商店槽位。' }
    [pscustomobject]@{ Id = 'MainContentCatalogAndSimulationServices'; Parent = 'RuntimeComposition'; Role = 'catalog reference'; Seam = 'read-only service/catalog ports'; Responsibility = '掉落、图鉴、传送、商店和高尔夫服务引用。' }
    [pscustomobject]@{ Id = 'MainSaveAndWorldSessionState'; Parent = 'RuntimeComposition'; Role = 'session state'; Seam = 'save/world session port'; Responsibility = '世界准备、存档路径、活动玩家和世界列表。' }
    [pscustomobject]@{ Id = 'MainInvasionAndNpcFrameState'; Parent = 'RuntimeComposition'; Role = 'runtime state'; Seam = 'invasion/NPC frame view'; Responsibility = '入侵进度、NPC 帧计数和客户端玩家引用。' }
    [pscustomobject]@{ Id = 'MainNetworkAndMenuState'; Parent = 'RuntimeComposition'; Role = 'runtime state'; Seam = 'network/menu adapter'; Responsibility = '网络模式、菜单、智能光标和运行时配置开关。' }
    [pscustomobject]@{ Id = 'MainAmbientEffectsAndLoopState'; Parent = 'RuntimeComposition'; Role = 'runtime state'; Seam = 'tick/ambient effect port'; Responsibility = '风雨环境、音乐、循环计时和窗口/菜单状态。' }
    [pscustomobject]@{ Id = 'MainDerivedPropertiesAndEvents'; Parent = 'RuntimeComposition'; Role = 'derived/query'; Seam = 'read-only property/event projection'; Responsibility = 'UIScale、世界资格、相机、风、玩家等派生属性和事件声明。' }

    [pscustomobject]@{ Id = 'WorldFileTilePacking'; Parent = 'PersistenceAndRecovery'; Role = 'adapter state'; Seam = 'tile serialization port'; Responsibility = '世界 Tile 打包和解包所需的中间状态。' }
    [pscustomobject]@{ Id = 'WorldFileRecoveryIo'; Parent = 'PersistenceAndRecovery'; Role = 'adapter state'; Seam = 'world file I/O port'; Responsibility = '世界文件恢复、校验和 I/O 协作状态。' }
    [pscustomobject]@{ Id = 'WorldFileMetadataAndSession'; Parent = 'PersistenceAndRecovery'; Role = 'snapshot state'; Seam = 'world metadata snapshot'; Responsibility = '世界文件元数据、路径和会话摘要。' }
    [pscustomobject]@{ Id = 'PlayerFileMetadataAndSession'; Parent = 'PersistenceAndRecovery'; Role = 'snapshot state'; Seam = 'player save metadata port'; Responsibility = '玩家存档元数据、路径和活动文件状态。' }

    [pscustomobject]@{ Id = 'NetworkSessionAndRemotePeers'; Parent = 'NetworkSessionAndSectionStreaming'; Role = 'adapter state'; Seam = 'session transport port'; Responsibility = '网络模式、远端客户端/服务器和连接端点状态。' }
    [pscustomobject]@{ Id = 'NetworkMessageBufferAndDispatch'; Parent = 'NetworkSessionAndSectionStreaming'; Role = 'adapter state'; Seam = 'message decode/dispatch port'; Responsibility = '消息缓冲、读写游标和分发上下文。' }
    [pscustomobject]@{ Id = 'NetworkPublicationAndSound'; Parent = 'NetworkSessionAndSectionStreaming'; Role = 'projection state'; Seam = 'network publication port'; Responsibility = '网络消息发布和声音消息载荷状态。' }
    [pscustomobject]@{ Id = 'SectionStreamingState'; Parent = 'NetworkSessionAndSectionStreaming'; Role = 'adapter state'; Seam = 'section streaming port'; Responsibility = '世界区段迭代和客户端区段加载状态。' }

    [pscustomobject]@{ Id = 'TileCellStorage'; Parent = 'WorldStorage'; Role = 'authoritative snapshot'; Seam = 'tile storage port'; Responsibility = '单格 Tile 存储字段。' }
    [pscustomobject]@{ Id = 'ChestContainerStorage'; Parent = 'WorldStorage'; Role = 'authoritative snapshot'; Seam = 'container storage port'; Responsibility = 'Chest 容器和槽位字段。' }

    [pscustomobject]@{ Id = 'TileObjectDefinitionCatalog'; Parent = 'ContentCatalog'; Role = 'definition/catalog'; Seam = 'read-only tile definition view'; Responsibility = 'TileObjectData 定义和放置元数据目录。' }
    [pscustomobject]@{ Id = 'ContentSamplesCatalog'; Parent = 'ContentCatalog'; Role = 'definition/catalog'; Seam = 'content sample view'; Responsibility = '内容样本、创意分类和排序索引。' }
    [pscustomobject]@{ Id = 'ContentSetFactory'; Parent = 'ContentCatalog'; Role = 'definition/catalog'; Seam = 'set factory port'; Responsibility = '按内容类型构造集合的工厂状态。' }
    [pscustomobject]@{ Id = 'ColorAndShaderSetCatalog'; Parent = 'ContentCatalog'; Role = 'definition/catalog'; Seam = 'color/shader catalog view'; Responsibility = '颜色和染料着色器集合。' }

    [pscustomobject]@{ Id = 'SocialApiRegistry'; Parent = 'ExternalBoundaries'; Role = 'adapter'; Seam = 'social provider port'; Responsibility = '社交提供程序注册和全局 API 状态。' }
    [pscustomobject]@{ Id = 'SocialTransportIpc'; Parent = 'ExternalBoundaries'; Role = 'adapter'; Seam = 'IPC transport port'; Responsibility = 'WeGame IPC 传输边界。' }
    [pscustomobject]@{ Id = 'WorkshopAndJoinBoundaryData'; Parent = 'ExternalBoundaries'; Role = 'adapter DTO'; Seam = 'workshop/join adapter'; Responsibility = '创意工坊、联机请求和富状态边界数据。' }

    [pscustomobject]@{ Id = 'CryptographicDependency'; Parent = 'ExternalDependencyOrGenerated'; Role = 'external dependency'; Seam = 'crypto adapter'; Responsibility = 'BCrypt 第三方实现字段。' }
    [pscustomobject]@{ Id = 'NatPortMappingInterop'; Parent = 'ExternalDependencyOrGenerated'; Role = 'external adapter'; Seam = 'UPnP COM adapter'; Responsibility = 'NAT/UPnP 端口映射互操作属性。' }

    [pscustomobject]@{ Id = 'AchievementPresentationState'; Parent = 'ClientPresentationAndTools'; Role = 'presentation'; Seam = 'achievement view'; Responsibility = '成就定义、跟踪和客户端展示状态。' }
    [pscustomobject]@{ Id = 'AudioPlaybackEngineState'; Parent = 'ClientPresentationAndTools'; Role = 'presentation/adapter'; Seam = 'audio playback engine port'; Responsibility = '音效播放引擎、声音跟踪和音频支持状态。' }
    [pscustomobject]@{ Id = 'AudioDefinitionAndTrackState'; Parent = 'ClientPresentationAndTools'; Role = 'definition/presentation'; Seam = 'audio definition/track view'; Responsibility = '声音样式、音频轨道和播放覆盖定义。' }
    [pscustomobject]@{ Id = 'AudioActiveSoundState'; Parent = 'ClientPresentationAndTools'; Role = 'presentation'; Seam = 'active sound projection'; Responsibility = '活动声音实例和特定环境声音跟踪。' }
    [pscustomobject]@{ Id = 'CinematicTimelineState'; Parent = 'ClientPresentationAndTools'; Role = 'presentation'; Seam = 'cinematic projection'; Responsibility = '电影序列、帧事件和播放状态。' }
    [pscustomobject]@{ Id = 'CameraAndVertexPresentation'; Parent = 'ClientPresentationAndTools'; Role = 'presentation'; Seam = 'graphics projection'; Responsibility = '相机、矩阵、顶点颜色和顶点条表现状态。' }
    [pscustomobject]@{ Id = 'MapAndWorldMapPresentation'; Parent = 'ClientPresentationAndTools'; Role = 'presentation'; Seam = 'map projection'; Responsibility = '地图瓦片、覆盖层、标记和更新队列。' }
    [pscustomobject]@{ Id = 'UiItemInteractionState'; Parent = 'ClientPresentationAndTools'; Role = 'presentation'; Seam = 'item UI interaction port'; Responsibility = '物品槽、排序、工具提示和脉冲效果状态。' }
    [pscustomobject]@{ Id = 'UiLayoutAndInteractionTree'; Parent = 'ClientPresentationAndTools'; Role = 'presentation'; Seam = 'UI tree/input port'; Responsibility = 'UI 元素树、状态、事件、布局和指针状态。' }

    [pscustomobject]@{ Id = 'SharedRuntimeInstrumentation'; Parent = 'SharedRuntimeMechanisms'; Role = 'diagnostics'; Seam = 'time/call event sink'; Responsibility = '时间序列、调用追踪和运行时诊断计时。' }
    [pscustomobject]@{ Id = 'SharedVerificationAndDebugTools'; Parent = 'SharedRuntimeMechanisms'; Role = 'diagnostics/tooling'; Seam = 'isolated diagnostic sink'; Responsibility = '测试、调试、启动和问题报告支持。' }
    [pscustomobject]@{ Id = 'SharedItemAndRecipeState'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/state'; Seam = 'item/recipe read-write port'; Responsibility = '物品、配方、快速堆叠、商店和制作支持。' }
    [pscustomobject]@{ Id = 'SharedItemVariantsAndEquipment'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/state'; Seam = 'equipment/variant view'; Responsibility = '物品变体、前缀、套装和装备载荷。' }
    [pscustomobject]@{ Id = 'SharedFishingDropRuleDefinitions'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/query'; Seam = 'fishing drop rule view'; Responsibility = '钓鱼渔获规则、条件、稀有度和填充目录。' }
    [pscustomobject]@{ Id = 'SharedFishingAttemptAndConditions'; Parent = 'SharedRuntimeMechanisms'; Role = 'state/query'; Seam = 'fishing attempt query'; Responsibility = '鱼漂尝试输入、玩家钓鱼条件和资格快照。' }
    [pscustomobject]@{ Id = 'SharedFishingCatchEffects'; Parent = 'SharedRuntimeMechanisms'; Role = 'command/adapter'; Seam = 'fishing catch effect port'; Responsibility = '鱼获交付、鱼饵桶辅助和来源归因。' }
    [pscustomobject]@{ Id = 'SharedLootAndDropRules'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/query'; Seam = 'loot rule resolver view'; Responsibility = '掉落规则、链、概率和离线掉落模拟。' }
    [pscustomobject]@{ Id = 'SharedCreativePowersAndUnlocks'; Parent = 'SharedRuntimeMechanisms'; Role = 'state/definition'; Seam = 'creative power port'; Responsibility = '创意能力、牺牲解锁和权限状态。' }
    [pscustomobject]@{ Id = 'SharedBestiaryAndPersonalityCatalog'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/catalog'; Seam = 'bestiary/personality view'; Responsibility = '图鉴、NPC 个性和城镇档案目录。' }
    [pscustomobject]@{ Id = 'SharedAchievementProgressSupport'; Parent = 'SharedRuntimeMechanisms'; Role = 'state/query'; Seam = 'achievement progress view'; Responsibility = '成就条件、跟踪和完成进度支持。' }
    [pscustomobject]@{ Id = 'SharedWorldGenerationDungeonDefinitions'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/query'; Seam = 'dungeon generation view'; Responsibility = '地牢房间、走廊、入口、布局和生成设置。' }
    [pscustomobject]@{ Id = 'SharedWorldGenerationBiomeSupport'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/query'; Seam = 'biome generation view'; Responsibility = 'Biome、洞穴房屋和地形生成支持。' }
    [pscustomobject]@{ Id = 'SharedWorldGenerationSupport'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/query'; Seam = 'generation helper view'; Responsibility = '生成形状、轨道、绘画和通用生成辅助。' }
    [pscustomobject]@{ Id = 'SharedWorldEventsAndInvasionSupport'; Parent = 'SharedRuntimeMechanisms'; Role = 'state/definition'; Seam = 'world event fact port'; Responsibility = '世界事件、入侵、Boss 伤害跟踪和条件对话。' }
    [pscustomobject]@{ Id = 'SharedDifficultyAndRuleMetadata'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/query'; Seam = 'difficulty/rule metadata view'; Responsibility = '难度等级、曲线和规则元数据。' }
    [pscustomobject]@{ Id = 'SharedWorldEnvironmentAndWeather'; Parent = 'SharedRuntimeMechanisms'; Role = 'state/presentation'; Seam = 'environment query'; Responsibility = '环境风雨、背景、云尘、天气对象和环境辅助。' }
    [pscustomobject]@{ Id = 'SharedTeleportAndPortalSupport'; Parent = 'SharedRuntimeMechanisms'; Role = 'query/adapter'; Seam = 'travel eligibility view'; Responsibility = '传送门、水晶塔和脱困支持。' }
    [pscustomobject]@{ Id = 'SharedWorldInteractionHelpers'; Parent = 'SharedRuntimeMechanisms'; Role = 'query'; Seam = 'interaction candidate query'; Responsibility = '智能交互、门、压力板、游标和交互辅助。' }
    [pscustomobject]@{ Id = 'SharedTilePlacementAndAnchors'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/query'; Seam = 'placement/anchor query'; Responsibility = 'Tile 放置、锚点、模块、框架和可达性支持。' }
    [pscustomobject]@{ Id = 'SharedTileEntitiesAndWorldObjects'; Parent = 'SharedRuntimeMechanisms'; Role = 'state'; Seam = 'tile entity storage port'; Responsibility = 'TileEntity 注册、容器锚点和世界对象状态。' }
    [pscustomobject]@{ Id = 'SharedTilePaintAndSnapshots'; Parent = 'SharedRuntimeMechanisms'; Role = 'snapshot/query'; Seam = 'tile visual snapshot'; Responsibility = 'Tile 着色、快照和帧/颜色缓存。' }
    [pscustomobject]@{ Id = 'SharedEntityAndWorldItemState'; Parent = 'SharedRuntimeMechanisms'; Role = 'state'; Seam = 'entity/world item port'; Responsibility = 'Entity、世界物品和实体阴影基础状态。' }
    [pscustomobject]@{ Id = 'SharedEntitySourceAndAttribution'; Parent = 'SharedRuntimeMechanisms'; Role = 'value object'; Seam = 'source attribution query'; Responsibility = '实体来源链、死亡原因和归因上下文。' }
    [pscustomobject]@{ Id = 'SharedPlayerIntentAndMovementData'; Parent = 'SharedRuntimeMechanisms'; Role = 'state/query'; Seam = 'player intent view'; Responsibility = '玩家意图、移动辅助、交互锚点和拾取日志。' }
    [pscustomobject]@{ Id = 'SharedCombatTargetingAndDamageData'; Parent = 'SharedRuntimeMechanisms'; Role = 'query/state'; Seam = 'combat target/hit query'; Responsibility = '命中、碰撞、目标、免疫和战斗数据结构。' }
    [pscustomobject]@{ Id = 'SharedSceneMetricsSnapshot'; Parent = 'SharedRuntimeMechanisms'; Role = 'query/cache'; Seam = 'scene metrics snapshot'; Responsibility = '场景扫描后的区域、计数和最近实体派生快照。' }
    [pscustomobject]@{ Id = 'SharedSceneScanSettings'; Parent = 'SharedRuntimeMechanisms'; Role = 'query input'; Seam = 'scene scan request'; Responsibility = '场景扫描中心、可视区域和玩家视角输入。' }
    [pscustomobject]@{ Id = 'SharedSceneVisualState'; Parent = 'SharedRuntimeMechanisms'; Role = 'presentation state'; Seam = 'scene visual projection'; Responsibility = '场景光照衰减、天气表现强度和视觉过渡状态。' }
    [pscustomobject]@{ Id = 'SharedNetworkPrimitives'; Parent = 'SharedRuntimeMechanisms'; Role = 'adapter'; Seam = 'packet/socket port'; Responsibility = '共享网络包、Socket、区段和网络模块原语。' }
    [pscustomobject]@{ Id = 'SharedLocalizationAndChat'; Parent = 'SharedRuntimeMechanisms'; Role = 'projection/adapter'; Seam = 'localized text/chat port'; Responsibility = '本地化文本、聊天消息和命令辅助。' }
    [pscustomobject]@{ Id = 'SharedInputAndControl'; Parent = 'SharedRuntimeMechanisms'; Role = 'adapter state'; Seam = 'input profile port'; Responsibility = '输入配置、触发器、锁定和焦点控制。' }
    [pscustomobject]@{ Id = 'SharedLightingAndLightMaps'; Parent = 'SharedRuntimeMechanisms'; Role = 'query/cache'; Seam = 'lighting query'; Responsibility = '光照引擎、光照图和 Tile 光照扫描。' }
    [pscustomobject]@{ Id = 'SharedShaderAndEffectState'; Parent = 'SharedRuntimeMechanisms'; Role = 'presentation'; Seam = 'shader/effect projection'; Responsibility = 'Shader、Effect、Sky 和覆盖层状态。' }
    [pscustomobject]@{ Id = 'SharedParticleAndVertexRendering'; Parent = 'SharedRuntimeMechanisms'; Role = 'presentation'; Seam = 'draw/particle projection'; Responsibility = '粒子、顶点、绘制数据和动画表现原语。' }
    [pscustomobject]@{ Id = 'SharedCaptureAndCameraSupport'; Parent = 'SharedRuntimeMechanisms'; Role = 'presentation'; Seam = 'capture/camera adapter'; Responsibility = '截图、无人机相机和钻头调试绘制支持。' }
    [pscustomobject]@{ Id = 'SharedGameContentUiAndWidgets'; Parent = 'SharedRuntimeMechanisms'; Role = 'presentation'; Seam = 'content UI view'; Responsibility = '内容 UI 控件、货币、表情和世界加载界面。' }
    [pscustomobject]@{ Id = 'SharedGolfState'; Parent = 'SharedRuntimeMechanisms'; Role = 'state/definition'; Seam = 'golf state view'; Responsibility = '高尔夫状态、轨迹和规则辅助。' }
    [pscustomobject]@{ Id = 'SharedContentPresentationDefinitions'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/presentation'; Seam = 'content presentation view'; Responsibility = '资源、档案、字体、安全和内容校验定义。' }
    [pscustomobject]@{ Id = 'SharedPopupAndCombatText'; Parent = 'SharedRuntimeMechanisms'; Role = 'presentation'; Seam = 'floating text projection'; Responsibility = '弹出文本和战斗文本显示状态。' }
    [pscustomobject]@{ Id = 'SharedMinecartAndMotion'; Parent = 'SharedRuntimeMechanisms'; Role = 'state/query'; Seam = 'vehicle/motion query'; Responsibility = '矿车运动和投射物引用跟踪支持。' }
    [pscustomobject]@{ Id = 'SharedPersistenceAndResourceAdapters'; Parent = 'SharedRuntimeMechanisms'; Role = 'adapter'; Seam = 'resource/file adapter'; Responsibility = '资源包、偏好设置和通用文件适配支持。' }
    [pscustomobject]@{ Id = 'SharedUtilityAndRandomness'; Parent = 'SharedRuntimeMechanisms'; Role = 'query/value object'; Seam = 'pure utility port'; Responsibility = '范围、位集合、随机数、缓冲和通用计算工具。' }
    [pscustomobject]@{ Id = 'SharedAudioAndSoundData'; Parent = 'SharedRuntimeMechanisms'; Role = 'presentation'; Seam = 'sound data projection'; Responsibility = '共享声音播放载荷和音效数据结构。' }
)

$retiredFineSubsystemIds = @(
    'MainFrameAndPlatformState', 'MainBootstrapAndSessionFlags', 'MainCameraAndVisualOffsets',
    'MainInputAndThreadScheduling', 'MainBackgroundAndMapPresentation', 'MainWorldBoundsAndSimulationRates',
    'MainSceneWeatherAndCalendarState', 'MainTileMetadataAndCageFrames', 'MainEntityPoolsAndWorldSlots',
    'MainPlayerInputAndInventorySlots', 'MainContentCatalogAndSimulationServices', 'MainSaveAndWorldSessionState',
    'MainInvasionAndNpcFrameState', 'MainNetworkAndMenuState', 'MainAmbientEffectsAndLoopState',
    'MainDerivedPropertiesAndEvents', 'SharedRuntimeInstrumentation', 'SharedVerificationAndDebugTools',
    'SharedItemAndRecipeState', 'SharedItemVariantsAndEquipment', 'SharedLootAndDropRules',
    'SharedItemDefinitionAndInstanceState', 'SharedDungeonLayoutDefinitions',
    'SharedTimeSeriesInstrumentation', 'SharedFishingAndCatchSupport', 'SharedSceneMetricsAndWorldQueries',
    'SharedWeatherAndAmbientState', 'SharedWorldEventState', 'SharedCreativePowerState', 'AudioPlaybackState',
    'UiItemSlotInteraction',
    'SharedCreativePowersAndUnlocks', 'SharedBestiaryAndPersonalityCatalog',
    'SharedWorldGenerationDungeonDefinitions', 'SharedWorldEventsAndInvasionSupport',
    'SharedWorldEnvironmentAndWeather', 'SharedTilePlacementAndAnchors', 'SharedEntityAndWorldItemState',
    'SharedTilePaintAndSnapshots', 'SharedCombatTargetingAndDamageData', 'SharedNetworkPrimitives',
    'SharedLocalizationAndChat', 'SharedInputAndControl', 'SharedShaderAndEffectState',
    'SharedParticleAndVertexRendering', 'SharedGameContentUiAndWidgets', 'SharedPersistenceAndResourceAdapters',
    'SharedUtilityAndRandomness', 'SharedContentPresentationDefinitions', 'SharedWorldInteractionHelpers',
    'SharedCreativePowersAndUnlocks', 'SharedBestiaryAndPersonalityCatalog', 'SharedRuntimeInstrumentation',
    'UiItemInteractionState',
    'SharedDungeonRootLayoutDefinitions', 'AudioPlaybackEngineState', 'UiLayoutAndInteractionTree',
    'NetworkSessionAndRemotePeers', 'SharedDungeonGenerationState', 'MapAndWorldMapPresentation',
    'SharedBestiaryCatalog', 'SharedItemTransferAndStacking', 'SharedLocalizationCatalog',
    'SharedTileEntitiesAndWorldObjects', 'SharedLightingAndLightMaps', 'SharedPlayerIntentAndMovementData',
    'SharedDrawAndAnimationPresentation', 'SharedGeneralUtilityFunctions', 'SharedTestingAndDebugTools',
    'SharedItemVariantsAndPrefixes', 'SharedMinecartAndMotion', 'SharedDirectWorldInteractionHelpers',
    'SharedRecipeAndCraftingRules', 'SharedWorldEnvironmentAndSpawnRules', 'SharedInputBindings',
    'SharedEntityBaseState', 'SharedEquipmentAndArmorDefinitions'
)

$splitFineDefinitions = @(
    [pscustomobject]@{ Id = 'MainFrameActivityState'; Parent = 'RuntimeComposition'; Role = 'runtime state'; Seam = 'frame activity snapshot'; Responsibility = '帧级玩家、Boss 和可交互对象活动事实。' }
    [pscustomobject]@{ Id = 'MainPlatformExecutionAdapter'; Parent = 'RuntimeComposition'; Role = 'platform adapter'; Seam = 'native execution-state port'; Responsibility = '平台线程保持常量和原生调用边界。' }
    [pscustomobject]@{ Id = 'MainBootstrapAndWorldRules'; Parent = 'RuntimeComposition'; Role = 'runtime state'; Seam = 'startup/world-rules view'; Responsibility = '启动引用、版本、秘密种子和世界规则开关。' }
    [pscustomobject]@{ Id = 'MainCameraAndUiScaleState'; Parent = 'RuntimeComposition'; Role = 'presentation state'; Seam = 'camera/UI transform view'; Responsibility = '相机对象、视图矩阵和 UI 缩放状态。' }
    [pscustomobject]@{ Id = 'MainSaveFavoritesAndSessionRefs'; Parent = 'RuntimeComposition'; Role = 'session state'; Seam = 'favorites/session reference view'; Responsibility = '收藏、世界文件元数据、成就和会话对象引用。' }
    [pscustomobject]@{ Id = 'MainClockAndFrameScheduling'; Parent = 'RuntimeComposition'; Role = 'runtime state'; Seam = 'clock/frame scheduler port'; Responsibility = '全局时钟、延迟处理、帧率和服务器调度参数。' }
    [pscustomobject]@{ Id = 'MainCameraAndVisualOffsets'; Parent = 'RuntimeComposition'; Role = 'presentation state'; Seam = 'camera offset view'; Responsibility = '相机插值、手持/头饰偏移和屏幕平移数据。' }
    [pscustomobject]@{ Id = 'MainInputAndThreadScheduling'; Parent = 'RuntimeComposition'; Role = 'runtime state'; Seam = 'main-thread action queue'; Responsibility = '输入坐标、主线程动作队列和帧级交互计时。' }
    [pscustomobject]@{ Id = 'MainBackgroundAndSeasonalState'; Parent = 'RuntimeComposition'; Role = 'presentation state'; Seam = 'background/season view'; Responsibility = '背景数组、节日开关和背景表现参数。' }
    [pscustomobject]@{ Id = 'MainDiagnosticsAndSimulationRates'; Parent = 'RuntimeComposition'; Role = 'runtime state'; Seam = 'diagnostics/rate configuration'; Responsibility = '网络诊断、启动显示、错误策略和模拟速率配置。' }
    [pscustomobject]@{ Id = 'MainContentAbilityTables'; Parent = 'RuntimeComposition'; Role = 'catalog reference'; Seam = 'content capability table'; Responsibility = '投射物和 Buff 能力分类表。' }
    [pscustomobject]@{ Id = 'MainRecentServerAndMapState'; Parent = 'RuntimeComposition'; Role = 'presentation state'; Seam = 'recent-server/map view'; Responsibility = '最近服务器、背景层和地图刷新状态。' }
    [pscustomobject]@{ Id = 'MainGraphicsAndGenerationState'; Parent = 'RuntimeComposition'; Role = 'presentation state'; Seam = 'graphics/generation adapter'; Responsibility = '图形设备、渲染计数、世界生成进度和保存计时引用。' }
    [pscustomobject]@{ Id = 'MainFrameAndWorldRuleControl'; Parent = 'RuntimeComposition'; Role = 'runtime state'; Seam = 'pause/world-rule control'; Responsibility = '暂停、世界难度、帧计数和自动加入控制。' }
    [pscustomobject]@{ Id = 'MainWorldGeometryAndCapacity'; Parent = 'RuntimeComposition'; Role = 'runtime state'; Seam = 'world geometry/capacity view'; Responsibility = '世界边界、Tile 尺寸、区段和实体容量。' }
    [pscustomobject]@{ Id = 'MainSlimeRainState'; Parent = 'RuntimeComposition'; Role = 'runtime state'; Seam = 'slime-rain event view'; Responsibility = '史莱姆雨警告、槽位、计时和击杀进度。' }
    [pscustomobject]@{ Id = 'MainCameraAndLiquidState'; Parent = 'RuntimeComposition'; Role = 'runtime state'; Seam = 'camera/liquid view'; Responsibility = '相机坐标、液体透明度、样式和缓冲区。' }
    [pscustomobject]@{ Id = 'MainWorldPersistenceAndMetadata'; Parent = 'RuntimeComposition'; Role = 'session state'; Seam = 'world metadata/persistence view'; Responsibility = '回滚、地牢锚点、保存校验、路径和世界元数据。' }
    [pscustomobject]@{ Id = 'MainCalendarWeatherState'; Parent = 'RuntimeComposition'; Role = 'runtime state'; Seam = 'calendar/weather fact view'; Responsibility = '昼夜、月相、雨、血月和日食事实。' }
    [pscustomobject]@{ Id = 'MainSpawnAndProjectileCaches'; Parent = 'RuntimeComposition'; Role = 'catalog reference'; Seam = 'spawn/projectile cache view'; Responsibility = '刷怪检查和投射物帧/宠物缓存。' }
    [pscustomobject]@{ Id = 'MainSceneMetricsState'; Parent = 'RuntimeComposition'; Role = 'derived/query'; Seam = 'scene metrics cache'; Responsibility = '相机和玩家场景指标缓存。' }
    [pscustomobject]@{ Id = 'MainWeatherAndAmbientState'; Parent = 'RuntimeComposition'; Role = 'runtime state'; Seam = 'weather/ambient view'; Responsibility = '星体、云层、风和环境对象状态。' }
    [pscustomobject]@{ Id = 'MainRandomAndSeedState'; Parent = 'RuntimeComposition'; Role = 'runtime state'; Seam = 'random/seed port'; Responsibility = '随机源、月亮类型和实验/种子配置。' }
    [pscustomobject]@{ Id = 'MainTileAndWallMetadata'; Parent = 'RuntimeComposition'; Role = 'catalog reference'; Seam = 'tile metadata view'; Responsibility = 'Tile、墙和放置能力元数据数组。' }
    [pscustomobject]@{ Id = 'MainCageAnimationState'; Parent = 'RuntimeComposition'; Role = 'presentation state'; Seam = 'cage animation view'; Responsibility = '捕获物笼具、罐体帧和动画计数。' }
    [pscustomobject]@{ Id = 'MainTileFrameAndCatchMetadata'; Parent = 'RuntimeComposition'; Role = 'runtime state'; Seam = 'tile-frame/catch view'; Responsibility = 'Tile 砂土、火焰、可捕获标记和帧缓存。' }
    [pscustomobject]@{ Id = 'MainWorldMapAndTileStore'; Parent = 'RuntimeComposition'; Role = 'runtime state'; Seam = 'world map/tile store'; Responsibility = '世界地图和全局 Tile 存储引用。' }
    [pscustomobject]@{ Id = 'MainEntityPoolsAndWorldSlots'; Parent = 'RuntimeComposition'; Role = 'runtime state'; Seam = 'entity-pool view'; Responsibility = '尘埃、星体、物品、NPC、投射物、容器和动画槽。' }
    [pscustomobject]@{ Id = 'MainScreenAndInputState'; Parent = 'RuntimeComposition'; Role = 'presentation state'; Seam = 'screen/input view'; Responsibility = '屏幕尺寸、输入接管、鼠标物品和 UI 颜色。' }
    [pscustomobject]@{ Id = 'MainPlayerAndSpawnState'; Parent = 'RuntimeComposition'; Role = 'runtime state'; Seam = 'player/spawn registry'; Responsibility = '本地玩家、玩家池、出生点和玩法归属槽。' }
    [pscustomobject]@{ Id = 'MainShopAndQuestSlots'; Parent = 'RuntimeComposition'; Role = 'runtime state'; Seam = 'shop/quest slot view'; Responsibility = '商店容器、旅行商店和渔夫任务槽。' }
    [pscustomobject]@{ Id = 'MainContentCatalogAndSimulationServices'; Parent = 'RuntimeComposition'; Role = 'catalog reference'; Seam = 'read-only service/catalog ports'; Responsibility = '掉落、鱼、图鉴、传送、商店和高尔夫服务引用。' }
    [pscustomobject]@{ Id = 'MainSaveAndWorldSessionState'; Parent = 'RuntimeComposition'; Role = 'session state'; Seam = 'save/world session port'; Responsibility = '世界准备、存档路径、活动文件和世界列表。' }
    [pscustomobject]@{ Id = 'MainInvasionState'; Parent = 'RuntimeComposition'; Role = 'runtime state'; Seam = 'invasion fact view'; Responsibility = '入侵类型、位置、波次和进度事实。' }
    [pscustomobject]@{ Id = 'MainNpcFrameState'; Parent = 'RuntimeComposition'; Role = 'presentation state'; Seam = 'NPC frame view'; Responsibility = 'NPC 帧计数和动画分类。' }
    [pscustomobject]@{ Id = 'MainNetworkSessionState'; Parent = 'RuntimeComposition'; Role = 'runtime state'; Seam = 'network session adapter'; Responsibility = '网络模式切换、玩家更新和连接会话状态。' }
    [pscustomobject]@{ Id = 'MainMenuAndInputSettings'; Parent = 'RuntimeComposition'; Role = 'presentation state'; Seam = 'menu/input settings view'; Responsibility = '菜单快捷键、智能游标和运行时资源设置。' }
    [pscustomobject]@{ Id = 'MainParticlePools'; Parent = 'RuntimeComposition'; Role = 'presentation state'; Seam = 'particle pool view'; Responsibility = '世界粒子渲染器引用。' }
    [pscustomobject]@{ Id = 'MainWindowAndShutdownState'; Parent = 'RuntimeComposition'; Role = 'runtime state'; Seam = 'window/shutdown adapter'; Responsibility = '窗口、锚点管理、退出和自动生成路径状态。' }
    [pscustomobject]@{ Id = 'MainTimeSkipState'; Parent = 'RuntimeComposition'; Role = 'runtime state'; Seam = 'time-skip command view'; Responsibility = '日晷/月晷快速推进和冷却状态。' }
    [pscustomobject]@{ Id = 'MainAmbientEffectsAndChatState'; Parent = 'RuntimeComposition'; Role = 'presentation state'; Seam = 'ambient/chat projection'; Responsibility = '风雨音乐、环境瀑布、聊天监视器和循环效果。' }
    [pscustomobject]@{ Id = 'MainTickAndDiagnosticState'; Parent = 'RuntimeComposition'; Role = 'runtime state'; Seam = 'tick/diagnostic view'; Responsibility = '投射物循环索引、帧目标、更新计时和辅助服务。' }
    [pscustomobject]@{ Id = 'MainMenuAndWorldGenerationState'; Parent = 'RuntimeComposition'; Role = 'session state'; Seam = 'menu/world-generation view'; Responsibility = '菜单物品缩放、世界名称、环境风和自动通过状态。' }
    [pscustomobject]@{ Id = 'MainInputAndEventFlags'; Parent = 'RuntimeComposition'; Role = 'runtime state'; Seam = 'input/event flag view'; Responsibility = '鼠标、绘制、陨石和环境伤害开关。' }
    [pscustomobject]@{ Id = 'MainDerivedPropertiesAndEvents'; Parent = 'RuntimeComposition'; Role = 'derived/query'; Seam = 'read-only property/event projection'; Responsibility = 'UI 缩放、世界资格、相机、风和玩家派生属性。' }

    [pscustomobject]@{ Id = 'SharedCallTrackingDiagnostics'; Parent = 'SharedRuntimeMechanisms'; Role = 'diagnostics'; Seam = 'call tracking sink'; Responsibility = '调用队列、已记录方法和刷新计时。' }
    [pscustomobject]@{ Id = 'SharedTimeSeriesDataSeriesState'; Parent = 'SharedRuntimeMechanisms'; Role = 'diagnostics state'; Seam = 'data-series aggregation port'; Responsibility = '帧窗口、分位数、最大值和时间序列聚合状态。' }
    [pscustomobject]@{ Id = 'SharedTimeSeriesEntryState'; Parent = 'SharedRuntimeMechanisms'; Role = 'diagnostics state'; Seam = 'time-log entry port'; Responsibility = '单个计时条目、预算、格式化委托和双缓冲序列。' }
    [pscustomobject]@{ Id = 'SharedTimeSeriesFormattingState'; Parent = 'SharedRuntimeMechanisms'; Role = 'diagnostics/query'; Seam = 'diagnostic formatting cache'; Responsibility = '性能计时和 CPU 显示格式缓存。' }
    [pscustomobject]@{ Id = 'SharedTimeLoggerCoordinatorState'; Parent = 'SharedRuntimeMechanisms'; Role = 'diagnostics'; Seam = 'time-logger coordinator port'; Responsibility = '计时条目注册、帧边界、详细日志和渲染计时协调。' }
    [pscustomobject]@{ Id = 'SharedItemStaticCatalogRules'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/catalog'; Seam = 'item static definition view'; Responsibility = '物品常量、全局能力表、价格和默认规则常量。' }
    [pscustomobject]@{ Id = 'SharedItemIdentityAndStackState'; Parent = 'SharedRuntimeMechanisms'; Role = 'state'; Seam = 'item identity/stack port'; Responsibility = '物品身份、实例数量、堆叠和变体身份。' }
    [pscustomobject]@{ Id = 'SharedItemEquipmentAndPresentationState'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/presentation'; Seam = 'equipment/presentation view'; Responsibility = '装备槽、染料、工具提示、声音和物品表现状态。' }
    [pscustomobject]@{ Id = 'SharedItemUseToolAndCombatState'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/state'; Seam = 'item capability port'; Responsibility = '物品使用、工具、弹药、武器、资源和战斗能力。' }
    [pscustomobject]@{ Id = 'SharedItemProgressionAndWorldInteractionState'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/state'; Seam = 'item progression/interaction port'; Responsibility = '任务、特殊使用、钓鱼、工具和 NPC 生成能力。' }
    [pscustomobject]@{ Id = 'SharedItemCommerceState'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/state'; Seam = 'item commerce port'; Responsibility = '商店资格、买卖价格和特殊货币状态。' }
    [pscustomobject]@{ Id = 'SharedItemBuffMountAndConsumableEffects'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/state'; Seam = 'item effect port'; Responsibility = 'Buff、坐骑、宠物和特殊消耗效果状态。' }
    [pscustomobject]@{ Id = 'SharedItemDerivedQueries'; Parent = 'SharedRuntimeMechanisms'; Role = 'derived/query'; Seam = 'item derived query'; Responsibility = '从内容目录或实例状态派生的物品查询属性。' }
    [pscustomobject]@{ Id = 'SharedRecipeAndCraftingRules'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/query'; Seam = 'recipe/crafting view'; Responsibility = '配方、配方组和制作请求规则。' }
    [pscustomobject]@{ Id = 'SharedItemTransferAndStacking'; Parent = 'SharedRuntimeMechanisms'; Role = 'state/query'; Seam = 'item transfer port'; Responsibility = '快速堆叠、紧急堆叠和物品领取设置。' }
    [pscustomobject]@{ Id = 'SharedItemCommerceAndPricing'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/state'; Seam = 'commerce pricing port'; Responsibility = '商店价格、回售和购物设置。' }
    [pscustomobject]@{ Id = 'SharedItemVariantsAndPrefixes'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/state'; Seam = 'variant definition view'; Responsibility = '物品变体、前缀和标签效果定义。' }
    [pscustomobject]@{ Id = 'SharedEquipmentAndArmorDefinitions'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/state'; Seam = 'equipment definition view'; Responsibility = '装备栏、套装加成和翅膀属性。' }
    [pscustomobject]@{ Id = 'SharedDungeonGenerationState'; Parent = 'SharedRuntimeMechanisms'; Role = 'runtime state'; Seam = 'dungeon generation state'; Responsibility = '地牢生成数据、变量和爬行器状态。' }
    [pscustomobject]@{ Id = 'SharedDungeonGenerationQueries'; Parent = 'SharedRuntimeMechanisms'; Role = 'query'; Seam = 'dungeon rule query'; Responsibility = '地牢进度、Tile/墙判断和生成辅助查询。' }
    [pscustomobject]@{ Id = 'SharedDungeonRootLayoutDefinitions'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/catalog'; Seam = 'dungeon material/geometry catalog'; Responsibility = '地牢根级样式、材料、边界、门、平台和保护定义。' }
    [pscustomobject]@{ Id = 'SharedDungeonRoomDefinitions'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/catalog'; Seam = 'dungeon room definition view'; Responsibility = '地牢房间类型、形状、房间设置和房间边界定义。' }
    [pscustomobject]@{ Id = 'SharedDungeonHallDefinitions'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/catalog'; Seam = 'dungeon hall definition view'; Responsibility = '地牢大厅类型、曲线、阶梯和大厅设置定义。' }
    [pscustomobject]@{ Id = 'SharedDungeonEntranceDefinitions'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/catalog'; Seam = 'dungeon entrance definition view'; Responsibility = '地牢入口类型、预生成参数和入口设置定义。' }
    [pscustomobject]@{ Id = 'SharedDungeonFeatureDefinitions'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/catalog'; Seam = 'dungeon feature definition view'; Responsibility = '地牢特征、全局家具、陷阱和装饰定义。' }
    [pscustomobject]@{ Id = 'SharedDungeonLayoutProviderDefinitions'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/catalog'; Seam = 'dungeon provider definition view'; Responsibility = '地牢布局 provider、双地牢和旧布局 provider 设置定义。' }
    [pscustomobject]@{ Id = 'SharedInvasionEventState'; Parent = 'SharedRuntimeMechanisms'; Role = 'runtime state'; Seam = 'invasion event port'; Responsibility = 'DD2 入侵波次、竞技场和事件进度状态。' }
    [pscustomobject]@{ Id = 'SharedSeasonalWorldEventState'; Parent = 'SharedRuntimeMechanisms'; Role = 'runtime state'; Seam = 'seasonal event scheduler port'; Responsibility = '派对、灯笼夜、沙尘暴、邪教和仙女事件计时状态。' }
    [pscustomobject]@{ Id = 'SharedWorldEventPresentationState'; Parent = 'SharedRuntimeMechanisms'; Role = 'presentation state'; Seam = 'world event presentation port'; Responsibility = '月总死亡戏剧、制作人员名单和屏幕遮挡表现状态。' }
    [pscustomobject]@{ Id = 'SharedInvasionAndBossTracking'; Parent = 'SharedRuntimeMechanisms'; Role = 'state/query'; Seam = 'invasion tracking port'; Responsibility = '入侵、Boss 伤害和旗帜进度跟踪。' }
    [pscustomobject]@{ Id = 'SharedConditionalDialogueSupport'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/query'; Seam = 'conditional dialogue view'; Responsibility = '条件对话和 Lucy 交互消息。' }
    [pscustomobject]@{ Id = 'SharedTownRoomState'; Parent = 'SharedRuntimeMechanisms'; Role = 'state'; Seam = 'town room registry'; Responsibility = '城镇房间和 NPC 房屋状态。' }
    [pscustomobject]@{ Id = 'SharedWeatherParticleState'; Parent = 'SharedRuntimeMechanisms'; Role = 'state/presentation'; Seam = 'weather particle port'; Responsibility = '云、雨和星体粒子的运动、生命周期和表现状态。' }
    [pscustomobject]@{ Id = 'SharedAmbientSkyAndWindState'; Parent = 'SharedRuntimeMechanisms'; Role = 'state/presentation'; Seam = 'ambient sky/wind port'; Responsibility = '环境实体生成、风点、树顶变体和天空闪烁状态。' }
    [pscustomobject]@{ Id = 'SharedLightningGenerationState'; Parent = 'SharedRuntimeMechanisms'; Role = 'state/query'; Seam = 'lightning generation port'; Responsibility = '闪电生成参数、分叉递归和 Tile 碰撞状态。' }
    [pscustomobject]@{ Id = 'SharedWaterfallState'; Parent = 'SharedRuntimeMechanisms'; Role = 'state/presentation'; Seam = 'waterfall manager port'; Responsibility = '瀑布槽位、长度限制和瀑布绘制资源状态。' }
    [pscustomobject]@{ Id = 'SharedParticleAndGoreEffects'; Parent = 'SharedRuntimeMechanisms'; Role = 'presentation'; Seam = 'particle/gore projection'; Responsibility = '尘粒和 Gore 表现状态。' }
    [pscustomobject]@{ Id = 'SharedWorldEnvironmentAndSpawnRules'; Parent = 'SharedRuntimeMechanisms'; Role = 'query/state'; Seam = 'environment/spawn query'; Responsibility = '刷怪、秘密种子、环境伤害和世界环境辅助。' }
    [pscustomobject]@{ Id = 'SharedTileObjectPlacementDefinitions'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/query'; Seam = 'tile placement definition'; Responsibility = '多格 Tile 对象、预览和放置钩子定义。' }
    [pscustomobject]@{ Id = 'SharedTileAnchorAndReachQueries'; Parent = 'SharedRuntimeMechanisms'; Role = 'query/state'; Seam = 'anchor/reach query'; Responsibility = '锚点、可达性、坐标和值对象支持。' }
    [pscustomobject]@{ Id = 'SharedTileFramingAndSignState'; Parent = 'SharedRuntimeMechanisms'; Role = 'state/query'; Seam = 'framing/sign view'; Responsibility = 'Tile 框架和告示牌状态。' }
    [pscustomobject]@{ Id = 'SharedTilePlacementModules'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/query'; Seam = 'placement module view'; Responsibility = 'Tile 放置、液体、锚点和替代样式模块。' }
    [pscustomobject]@{ Id = 'SharedEntityBaseState'; Parent = 'SharedRuntimeMechanisms'; Role = 'state'; Seam = 'entity base view'; Responsibility = '通用实体和实体阴影基础状态。' }
    [pscustomobject]@{ Id = 'SharedWorldItemState'; Parent = 'SharedRuntimeMechanisms'; Role = 'state'; Seam = 'world item view'; Responsibility = '世界物品实例状态。' }
    [pscustomobject]@{ Id = 'SharedTilePaintState'; Parent = 'SharedRuntimeMechanisms'; Role = 'state/query'; Seam = 'tile paint cache'; Responsibility = 'Tile 着色和颜色缓存。' }
    [pscustomobject]@{ Id = 'SharedTileSnapshots'; Parent = 'SharedRuntimeMechanisms'; Role = 'snapshot/query'; Seam = 'tile snapshot view'; Responsibility = 'Tile 快照值和只读投影。' }
    [pscustomobject]@{ Id = 'SharedCombatTargetingAndImmunity'; Parent = 'SharedRuntimeMechanisms'; Role = 'query/state'; Seam = 'combat target query'; Responsibility = '目标、命中框、免疫和击杀尝试数据。' }
    [pscustomobject]@{ Id = 'SharedPhysicsCollisionQueries'; Parent = 'SharedRuntimeMechanisms'; Role = 'query'; Seam = 'collision query'; Responsibility = '球体碰撞和穿透查询事件。' }
    [pscustomobject]@{ Id = 'SharedHitTileTracking'; Parent = 'SharedRuntimeMechanisms'; Role = 'state/query'; Seam = 'hit tile tracking'; Responsibility = 'Tile 命中和挖掘追踪状态。' }
    [pscustomobject]@{ Id = 'SharedNetworkSocketTransport'; Parent = 'SharedRuntimeMechanisms'; Role = 'adapter'; Seam = 'socket transport port'; Responsibility = 'Socket、调试流和 TCP 传输。' }
    [pscustomobject]@{ Id = 'SharedNetworkPacketPrimitives'; Parent = 'SharedRuntimeMechanisms'; Role = 'adapter'; Seam = 'packet buffer port'; Responsibility = '网络包、地址和缓冲池原语。' }
    [pscustomobject]@{ Id = 'SharedContentNetworkModules'; Parent = 'SharedRuntimeMechanisms'; Role = 'adapter'; Seam = 'content network module'; Responsibility = '内容能力和液体网络模块。' }
    [pscustomobject]@{ Id = 'SharedNetworkSectionProjections'; Parent = 'SharedRuntimeMechanisms'; Role = 'projection'; Seam = 'section projection'; Responsibility = '区段和花朵包的网络投影载荷。' }
    [pscustomobject]@{ Id = 'SharedLocalizationCatalog'; Parent = 'SharedRuntimeMechanisms'; Role = 'projection'; Seam = 'localized text catalog'; Responsibility = '语言、文化、本地化文本和 Lang 目录。' }
    [pscustomobject]@{ Id = 'SharedChatAndCommandProtocol'; Parent = 'SharedRuntimeMechanisms'; Role = 'adapter'; Seam = 'chat command port'; Responsibility = '聊天消息、颜色和命令处理协议。' }
    [pscustomobject]@{ Id = 'SharedChatPresentation'; Parent = 'SharedRuntimeMechanisms'; Role = 'presentation'; Seam = 'chat rendering view'; Responsibility = '聊天片段、标签和 UI 文本表现。' }
    [pscustomobject]@{ Id = 'SharedInputBindings'; Parent = 'SharedRuntimeMechanisms'; Role = 'adapter'; Seam = 'input binding port'; Responsibility = '输入配置、触发器和玩家输入档案。' }
    [pscustomobject]@{ Id = 'SharedControlFocusHelpers'; Parent = 'SharedRuntimeMechanisms'; Role = 'query/adapter'; Seam = 'focus/lock-on query'; Responsibility = '焦点和锁定控制辅助。' }
    [pscustomobject]@{ Id = 'SharedShaderData'; Parent = 'SharedRuntimeMechanisms'; Role = 'presentation'; Seam = 'shader data view'; Responsibility = 'Shader 数据、集合和游戏 Shader 注册。' }
    [pscustomobject]@{ Id = 'SharedEffectAndSkyPresentation'; Parent = 'SharedRuntimeMechanisms'; Role = 'presentation'; Seam = 'effect/sky projection'; Responsibility = 'Effect、覆盖层和天空表现。' }
    [pscustomobject]@{ Id = 'SharedParticlePresentation'; Parent = 'SharedRuntimeMechanisms'; Role = 'presentation'; Seam = 'particle renderer port'; Responsibility = '粒子接口、池化粒子和粒子渲染器。' }
    [pscustomobject]@{ Id = 'SharedDrawAndAnimationPresentation'; Parent = 'SharedRuntimeMechanisms'; Role = 'presentation'; Seam = 'draw/animation projection'; Responsibility = '绘制数据、动画、顶点和绘制辅助。' }
    [pscustomobject]@{ Id = 'SharedContentUiWidgets'; Parent = 'SharedRuntimeMechanisms'; Role = 'presentation'; Seam = 'content widget view'; Responsibility = '内容 UI 基础控件和列表布局。' }
    [pscustomobject]@{ Id = 'SharedContentUiScreens'; Parent = 'SharedRuntimeMechanisms'; Role = 'presentation'; Seam = 'content screen view'; Responsibility = '世界加载和世界选择界面状态。' }
    [pscustomobject]@{ Id = 'SharedContentUiCurrency'; Parent = 'SharedRuntimeMechanisms'; Role = 'presentation'; Seam = 'currency UI view'; Responsibility = '自定义货币 UI 状态。' }
    [pscustomobject]@{ Id = 'SharedWorldInteractionUi'; Parent = 'SharedRuntimeMechanisms'; Role = 'presentation'; Seam = 'world interaction UI'; Responsibility = '表情、线路、世界锚点和物品稀有度界面。' }
    [pscustomobject]@{ Id = 'SharedDropRuleDefinitions'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/query'; Seam = 'drop rule resolver'; Responsibility = '掉落规则、条件、链和概率定义。' }
    [pscustomobject]@{ Id = 'SharedLootSimulation'; Parent = 'SharedRuntimeMechanisms'; Role = 'query'; Seam = 'loot simulation view'; Responsibility = '离线掉落模拟和计数状态。' }
    [pscustomobject]@{ Id = 'SharedDropSourceAttribution'; Parent = 'SharedRuntimeMechanisms'; Role = 'value object'; Seam = 'drop source attribution'; Responsibility = '掉落和 Boss 生成来源值对象。' }
    [pscustomobject]@{ Id = 'SharedCreativePowerDefinitions'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/state'; Seam = 'creative power definition port'; Responsibility = '创意能力基类、滑杆/开关/按钮能力和能力接口。' }
    [pscustomobject]@{ Id = 'SharedCreativePowerRuntimeManager'; Parent = 'SharedRuntimeMechanisms'; Role = 'state'; Seam = 'creative power manager port'; Responsibility = '创意能力注册、按玩家存储和运行时管理。' }
    [pscustomobject]@{ Id = 'SharedCreativePowerPresentation'; Parent = 'SharedRuntimeMechanisms'; Role = 'presentation'; Seam = 'creative power UI port'; Responsibility = '创意能力图标位置和 UI 请求数据。' }
    [pscustomobject]@{ Id = 'SharedCreativeUnlockProgress'; Parent = 'SharedRuntimeMechanisms'; Role = 'state/query'; Seam = 'creative unlock view'; Responsibility = '牺牲目录和创意解锁进度。' }
    [pscustomobject]@{ Id = 'SharedBestiaryCatalog'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/catalog'; Seam = 'bestiary catalog view'; Responsibility = '图鉴条目、筛选、排序和解锁跟踪。' }
    [pscustomobject]@{ Id = 'SharedNpcPersonalityCatalog'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/catalog'; Seam = 'personality catalog view'; Responsibility = 'NPC 个性偏好和城镇档案。' }
    [pscustomobject]@{ Id = 'SharedSmartInteractionQueries'; Parent = 'SharedRuntimeMechanisms'; Role = 'query'; Seam = 'smart interaction query'; Responsibility = '智能交互候选和扫描查询。' }
    [pscustomobject]@{ Id = 'SharedDirectWorldInteractionHelpers'; Parent = 'SharedRuntimeMechanisms'; Role = 'query/adapter'; Seam = 'world interaction helper'; Responsibility = '门、压力板、游标和定位容器辅助。' }
    [pscustomobject]@{ Id = 'SharedRandomSources'; Parent = 'SharedRuntimeMechanisms'; Role = 'value object'; Seam = 'random source port'; Responsibility = '伪随机源和随机流实现。' }
    [pscustomobject]@{ Id = 'SharedBufferAndCollectionPools'; Parent = 'SharedRuntimeMechanisms'; Role = 'value object'; Seam = 'buffer pool port'; Responsibility = '缓冲池、缓存缓冲和集合排序容器。' }
    [pscustomobject]@{ Id = 'SharedRangeAndBitUtilities'; Parent = 'SharedRuntimeMechanisms'; Role = 'value object'; Seam = 'range/bit query'; Responsibility = '范围、位图和位集合值对象。' }
    [pscustomobject]@{ Id = 'SharedGeneralUtilityFunctions'; Parent = 'SharedRuntimeMechanisms'; Role = 'query/value object'; Seam = 'pure utility port'; Responsibility = '通用计算、委托和剩余工具方法。' }
    [pscustomobject]@{ Id = 'SharedSaveAndConfigurationAdapters'; Parent = 'SharedRuntimeMechanisms'; Role = 'adapter'; Seam = 'save/config file port'; Responsibility = '存档元数据、收藏和配置文件适配。' }
    [pscustomobject]@{ Id = 'SharedResourcePackAdapters'; Parent = 'SharedRuntimeMechanisms'; Role = 'adapter'; Seam = 'resource pack port'; Responsibility = '资源包及资源包列表适配。' }
    [pscustomobject]@{ Id = 'SharedFileSystemAdapters'; Parent = 'SharedRuntimeMechanisms'; Role = 'adapter'; Seam = 'file system port'; Responsibility = '文件浏览和通用文件操作适配。' }
    [pscustomobject]@{ Id = 'SharedBackgroundPresentationDefinitions'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/presentation'; Seam = 'background definition view'; Responsibility = '背景变体和背景集合定义。' }
    [pscustomobject]@{ Id = 'SharedContentValidation'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/query'; Seam = 'content validation query'; Responsibility = '内容安全、拒绝规则和合法性校验。' }
    [pscustomobject]@{ Id = 'SharedContentPresentationCatalog'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/presentation'; Seam = 'content presentation view'; Responsibility = '档案、字体、发型和视觉配置定义。' }
    [pscustomobject]@{ Id = 'SharedTestingAndDebugTools'; Parent = 'SharedRuntimeMechanisms'; Role = 'diagnostics/tooling'; Seam = 'test/debug sink'; Responsibility = '测试命令、调试选项、FPS 和崩溃观察。' }
    [pscustomobject]@{ Id = 'SharedStartupAndIssueReporting'; Parent = 'SharedRuntimeMechanisms'; Role = 'diagnostics/adapter'; Seam = 'startup/issue port'; Responsibility = '启动、服务器入口、初始化和问题报告。' }

    [pscustomobject]@{ Id = 'UiItemSlotContextDefinitions'; Parent = 'ClientPresentationAndTools'; Role = 'definition/catalog'; Seam = 'item slot context catalog'; Responsibility = '物品槽上下文、合法槽位类别和 UI 计数定义。' }
    [pscustomobject]@{ Id = 'UiItemSlotTransferState'; Parent = 'ClientPresentationAndTools'; Role = 'presentation/command'; Seam = 'item slot transfer port'; Responsibility = '替代点击、物品转移和槽位交互载荷。' }
    [pscustomobject]@{ Id = 'UiItemSlotPresentationAndPulseState'; Parent = 'ClientPresentationAndTools'; Role = 'presentation'; Seam = 'item slot draw/pulse port'; Responsibility = '物品槽显示键、脉冲、高亮和绘制缓存状态。' }
    [pscustomobject]@{ Id = 'UiItemSorting'; Parent = 'ClientPresentationAndTools'; Role = 'presentation'; Seam = 'item sorting view'; Responsibility = '物品排序层和排序缓存。' }
    [pscustomobject]@{ Id = 'UiItemTooltipState'; Parent = 'ClientPresentationAndTools'; Role = 'presentation'; Seam = 'item tooltip view'; Responsibility = '物品工具提示文本和校验状态。' }

    [pscustomobject]@{ Id = 'SharedDungeonStyleCatalog'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/catalog'; Seam = 'dungeon style catalog'; Responsibility = '地牢生成样式和样式集合定义。' }
    [pscustomobject]@{ Id = 'SharedDungeonLayoutProviderState'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/state'; Seam = 'dungeon layout provider port'; Responsibility = '根级地牢布局 provider 及其设置状态。' }
    [pscustomobject]@{ Id = 'SharedDungeonGeometryAndPlacementDefinitions'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/catalog'; Seam = 'dungeon geometry placement view'; Responsibility = '地牢边界、平台、门和不可破坏墙进阶定义。' }
    [pscustomobject]@{ Id = 'AudioLegacySoundAdapterState'; Parent = 'ClientPresentationAndTools'; Role = 'presentation/adapter'; Seam = 'legacy sound adapter port'; Responsibility = '旧音效播放器及其声音实例适配状态。' }
    [pscustomobject]@{ Id = 'AudioPlaybackCoordinatorState'; Parent = 'ClientPresentationAndTools'; Role = 'presentation/adapter'; Seam = 'audio playback coordinator'; Responsibility = '音频引擎和播放协调器状态。' }
    [pscustomobject]@{ Id = 'AudioTrackedSoundState'; Parent = 'ClientPresentationAndTools'; Role = 'presentation/state'; Seam = 'tracked sound port'; Responsibility = '被跟踪声音集合和声音播放器状态。' }
    [pscustomobject]@{ Id = 'UiLayoutPrimitives'; Parent = 'ClientPresentationAndTools'; Role = 'presentation/value object'; Seam = 'UI layout value port'; Responsibility = 'UI 尺寸、计算样式和吸附点值对象。' }
    [pscustomobject]@{ Id = 'UiElementTreeState'; Parent = 'ClientPresentationAndTools'; Role = 'presentation/state'; Seam = 'UI element tree port'; Responsibility = 'UI 元素树、根状态和子元素关系。' }
    [pscustomobject]@{ Id = 'UiEventPayloads'; Parent = 'ClientPresentationAndTools'; Role = 'presentation/event'; Seam = 'UI event payload port'; Responsibility = 'UI 事件、鼠标事件和滚轮事件载荷。' }
    [pscustomobject]@{ Id = 'UiInputPointerState'; Parent = 'ClientPresentationAndTools'; Role = 'presentation/state'; Seam = 'UI pointer input port'; Responsibility = '用户界面输入指针缓存和状态变更协调。' }
    [pscustomobject]@{ Id = 'NetworkSessionCoordinatorState'; Parent = 'NetworkSessionAndSectionStreaming'; Role = 'adapter state'; Seam = 'network session coordinator'; Responsibility = '网络模式、连接上限和会话协调配置。' }
    [pscustomobject]@{ Id = 'NetworkRemoteClientState'; Parent = 'NetworkSessionAndSectionStreaming'; Role = 'adapter state'; Seam = 'remote client transport port'; Responsibility = '远端客户端连接、发送队列和客户端状态。' }
    [pscustomobject]@{ Id = 'NetworkRemoteServerState'; Parent = 'NetworkSessionAndSectionStreaming'; Role = 'adapter state'; Seam = 'remote server transport port'; Responsibility = '远端服务器连接、活动状态和服务器端点。' }
    [pscustomobject]@{ Id = 'NetworkRemoteIpRequestAdapter'; Parent = 'NetworkSessionAndSectionStreaming'; Role = 'adapter DTO'; Seam = 'remote IP request callback'; Responsibility = '远端 IP 请求标识、回调和结果载荷。' }
    [pscustomobject]@{ Id = 'SharedDungeonCrawlerRuntimeState'; Parent = 'SharedRuntimeMechanisms'; Role = 'runtime state'; Seam = 'dungeon crawler runtime port'; Responsibility = '地牢爬行器当前数据和生成运行状态。' }
    [pscustomobject]@{ Id = 'SharedDungeonGenerationDataState'; Parent = 'SharedRuntimeMechanisms'; Role = 'runtime state'; Seam = 'dungeon generation data port'; Responsibility = '地牢生成数据、迭代和阶段状态。' }
    [pscustomobject]@{ Id = 'SharedDungeonLegacyGenerationGlobals'; Parent = 'SharedRuntimeMechanisms'; Role = 'runtime state'; Seam = 'legacy dungeon globals'; Responsibility = '旧地牢生成全局变量和位置/方向状态。' }
    [pscustomobject]@{ Id = 'MapTileStorageAndUpdateState'; Parent = 'ClientPresentationAndTools'; Role = 'presentation/state'; Seam = 'map tile update port'; Responsibility = '地图瓦片、地图帮助器和增量更新队列。' }
    [pscustomobject]@{ Id = 'MapOverlayAndLayerPresentation'; Parent = 'ClientPresentationAndTools'; Role = 'presentation'; Seam = 'map overlay layer view'; Responsibility = '地图覆盖层、Ping 和传送晶塔图层表现。' }
    [pscustomobject]@{ Id = 'WorldMapState'; Parent = 'ClientPresentationAndTools'; Role = 'presentation/state'; Seam = 'world map storage port'; Responsibility = '世界地图尺寸和地图持久状态。' }
    [pscustomobject]@{ Id = 'BestiaryCatalogAndEntries'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/catalog'; Seam = 'bestiary catalog port'; Responsibility = '图鉴数据库和图鉴条目目录。' }
    [pscustomobject]@{ Id = 'BestiaryUnlockTracking'; Parent = 'SharedRuntimeMechanisms'; Role = 'state/query'; Seam = 'bestiary unlock port'; Responsibility = '图鉴击杀、接近、对话和解锁进度跟踪。' }
    [pscustomobject]@{ Id = 'BestiaryInfoElementsAndProviders'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/presentation'; Seam = 'bestiary info provider view'; Responsibility = '图鉴信息元素、显示索引和 UI 集合 provider。' }
    [pscustomobject]@{ Id = 'BestiaryFiltersAndSorting'; Parent = 'SharedRuntimeMechanisms'; Role = 'query'; Seam = 'bestiary filter/sort query'; Responsibility = '图鉴筛选器和排序步骤。' }
    [pscustomobject]@{ Id = 'ItemEmergencyStackingState'; Parent = 'SharedRuntimeMechanisms'; Role = 'state/query'; Seam = 'emergency stacking port'; Responsibility = '紧急堆叠候选、传输和距离策略状态。' }
    [pscustomobject]@{ Id = 'ItemQuickStackingState'; Parent = 'SharedRuntimeMechanisms'; Role = 'state/query'; Seam = 'quick stacking port'; Responsibility = '快速堆叠源、目标和匹配缓存状态。' }
    [pscustomobject]@{ Id = 'ItemTransferSettings'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/state'; Seam = 'item transfer settings port'; Responsibility = '物品领取和转移行为设置。' }
    [pscustomobject]@{ Id = 'LocalizationCultureAndLanguageState'; Parent = 'SharedRuntimeMechanisms'; Role = 'projection/state'; Seam = 'culture language port'; Responsibility = '当前语言、文化注册表和语言管理器状态。' }
    [pscustomobject]@{ Id = 'LocalizedTextValueState'; Parent = 'SharedRuntimeMechanisms'; Role = 'projection/value object'; Seam = 'localized text value port'; Responsibility = '本地化文本、变量文本和网络文本值对象。' }
    [pscustomobject]@{ Id = 'LegacyLanguageCatalogState'; Parent = 'SharedRuntimeMechanisms'; Role = 'projection/catalog'; Seam = 'legacy language catalog'; Responsibility = '旧 Lang 文本目录和前缀文本组合状态。' }
    [pscustomobject]@{ Id = 'TileEntityRegistryAndBaseState'; Parent = 'SharedRuntimeMechanisms'; Role = 'state'; Seam = 'tile entity registry port'; Responsibility = 'TileEntity 基类、注册表、类型标识和实体 ID。' }
    [pscustomobject]@{ Id = 'TileEntityDisplayAndInventoryState'; Parent = 'SharedRuntimeMechanisms'; Role = 'state/presentation'; Seam = 'tile entity display inventory port'; Responsibility = '展示架、物品框、食物盘和展示容器状态。' }
    [pscustomobject]@{ Id = 'TileEntityAnchorAndSensorState'; Parent = 'SharedRuntimeMechanisms'; Role = 'state/query'; Seam = 'tile entity anchor sensor port'; Responsibility = '逻辑传感器和生物/风筝锚点状态。' }
    [pscustomobject]@{ Id = 'TileEntityWorldInteractionState'; Parent = 'SharedRuntimeMechanisms'; Role = 'state/query'; Seam = 'tile entity interaction port'; Responsibility = '训练假人、传送晶塔和带物品的实体锚点交互状态。' }
    [pscustomobject]@{ Id = 'LightingEngineState'; Parent = 'SharedRuntimeMechanisms'; Role = 'state/query'; Seam = 'lighting engine port'; Responsibility = '新旧光照引擎和全局光照协调状态。' }
    [pscustomobject]@{ Id = 'LightMapCacheState'; Parent = 'SharedRuntimeMechanisms'; Role = 'cache'; Seam = 'light map cache port'; Responsibility = '光照图颜色和掩码缓存。' }
    [pscustomobject]@{ Id = 'TileLightScannerState'; Parent = 'SharedRuntimeMechanisms'; Role = 'query'; Seam = 'tile light scanner port'; Responsibility = 'Tile 光照扫描和随机扫描辅助状态。' }
    [pscustomobject]@{ Id = 'PlayerIntentAndMovementState'; Parent = 'SharedRuntimeMechanisms'; Role = 'state/query'; Seam = 'player intent movement port'; Responsibility = '玩家意图推断、移动辅助、交互锚点和便携凳状态。' }
    [pscustomobject]@{ Id = 'PlayerItemPickupAndRespawnState'; Parent = 'SharedRuntimeMechanisms'; Role = 'state'; Seam = 'player pickup respawn port'; Responsibility = '玩家物品领取日志、召唤物生成和重生状态。' }
    [pscustomobject]@{ Id = 'PlayerPreviewAndRejectionState'; Parent = 'SharedRuntimeMechanisms'; Role = 'presentation/state'; Seam = 'player preview rejection port'; Responsibility = '角色预览设置和拒绝菜单载荷。' }
    [pscustomobject]@{ Id = 'DrawAnimationAndFrameState'; Parent = 'SharedRuntimeMechanisms'; Role = 'presentation/state'; Seam = 'draw animation frame port'; Responsibility = '动画、绘制动画和精灵帧状态。' }
    [pscustomobject]@{ Id = 'DrawCommandAndBatchState'; Parent = 'SharedRuntimeMechanisms'; Role = 'presentation/command'; Seam = 'draw command batch port'; Responsibility = '绘制数据命令和 SpriteBatch 批处理状态。' }
    [pscustomobject]@{ Id = 'WorldDrawingAuxiliaryState'; Parent = 'SharedRuntimeMechanisms'; Role = 'presentation'; Seam = 'world drawing auxiliary port'; Responsibility = '粒子编排、Tile 绘制、地平线和辅助渲染状态。' }
    [pscustomobject]@{ Id = 'SharedGeneralPureUtilities'; Parent = 'SharedRuntimeMechanisms'; Role = 'query/value object'; Seam = 'pure general utility port'; Responsibility = '通用计算、传送尝试和拦截结果等纯工具值。' }
    [pscustomobject]@{ Id = 'SharedGeneralFilePlatformUtilities'; Parent = 'SharedRuntimeMechanisms'; Role = 'adapter'; Seam = 'file platform utility port'; Responsibility = '文件浏览、文件操作和运行时平台辅助工具。' }
    [pscustomobject]@{ Id = 'SharedGeneralDiagnosticsUtilities'; Parent = 'SharedRuntimeMechanisms'; Role = 'diagnostics'; Seam = 'general diagnostics sink'; Responsibility = '异常观察和通用诊断工具状态。' }
    [pscustomobject]@{ Id = 'SharedGeneralDelegateAndMetadataUtilities'; Parent = 'SharedRuntimeMechanisms'; Role = 'value object/metadata'; Seam = 'delegate metadata utility port'; Responsibility = '委托方法、旧属性元数据和秘密值辅助。' }
    [pscustomobject]@{ Id = 'DebugCommandProtocol'; Parent = 'SharedRuntimeMechanisms'; Role = 'diagnostics/adapter'; Seam = 'debug command protocol'; Responsibility = '调试命令接口、属性、消息和处理器协议。' }
    [pscustomobject]@{ Id = 'DebugRuntimeOptions'; Parent = 'SharedRuntimeMechanisms'; Role = 'diagnostics/state'; Seam = 'debug runtime options'; Responsibility = '调试命令开关和运行时调试选项。' }
    [pscustomobject]@{ Id = 'DebugFrameTelemetry'; Parent = 'SharedRuntimeMechanisms'; Role = 'diagnostics/state'; Seam = 'frame telemetry port'; Responsibility = '详细 FPS 帧、事件和采样状态。' }
    [pscustomobject]@{ Id = 'DebugBuildStatus'; Parent = 'SharedRuntimeMechanisms'; Role = 'diagnostics/adapter'; Seam = 'build status port'; Responsibility = '源码版本和 Git 构建状态信息。' }
    [pscustomobject]@{ Id = 'ItemVariantDefinitions'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/catalog'; Seam = 'item variant definition port'; Responsibility = '物品变体、变体条件和变体条目定义。' }
    [pscustomobject]@{ Id = 'ItemTagEffectState'; Parent = 'SharedRuntimeMechanisms'; Role = 'state/definition'; Seam = 'item tag effect port'; Responsibility = '鞭子标签效果、唯一标签效果和效果状态。' }
    [pscustomobject]@{ Id = 'LegacyItemPrefixCatalog'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/catalog'; Seam = 'legacy prefix catalog'; Responsibility = '旧物品前缀和前缀物品集合目录。' }
    [pscustomobject]@{ Id = 'MinecartMotionState'; Parent = 'SharedRuntimeMechanisms'; Role = 'state/query'; Seam = 'minecart motion port'; Responsibility = '矿车轨道运动、帧和装饰状态。' }
    [pscustomobject]@{ Id = 'MinecartCustomizationState'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/presentation'; Seam = 'minecart customization port'; Responsibility = '矿车纹理、轮距和定制表现状态。' }
    [pscustomobject]@{ Id = 'TrackedProjectileReferenceState'; Parent = 'SharedRuntimeMechanisms'; Role = 'relation/state'; Seam = 'tracked projectile reference port'; Responsibility = '投射物本地索引和拥有者引用跟踪。' }
    [pscustomobject]@{ Id = 'DoorOpeningInteractionState'; Parent = 'SharedRuntimeMechanisms'; Role = 'state/query'; Seam = 'door interaction port'; Responsibility = '门开启/关闭候选、玩家信息和切换状态。' }
    [pscustomobject]@{ Id = 'SmartCursorInteractionState'; Parent = 'SharedRuntimeMechanisms'; Role = 'query/state'; Seam = 'smart cursor interaction port'; Responsibility = '智能游标目标、抓钩目标和使用信息。' }
    [pscustomobject]@{ Id = 'PressurePlateInteractionState'; Parent = 'SharedRuntimeMechanisms'; Role = 'state/query'; Seam = 'pressure plate interaction port'; Responsibility = '压力板检测锁和被按压集合状态。' }
    [pscustomobject]@{ Id = 'CursorAndChestInteractionState'; Parent = 'SharedRuntimeMechanisms'; Role = 'state/query'; Seam = 'cursor chest interaction port'; Responsibility = '虚拟游标物品和定位 Chest 交互状态。' }
    [pscustomobject]@{ Id = 'RecipeDefinitionCatalog'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/catalog'; Seam = 'recipe definition port'; Responsibility = '配方、配方组和必需物品条目目录。' }
    [pscustomobject]@{ Id = 'CraftingRequestState'; Parent = 'SharedRuntimeMechanisms'; Role = 'state/command'; Seam = 'crafting request port'; Responsibility = '远程制作请求和待处理制作队列状态。' }
    [pscustomobject]@{ Id = 'WorldSpawnConfigurationState'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/state'; Seam = 'world spawn configuration port'; Responsibility = '刷怪参数、额外出生点和 NPC 生成配置。' }
    [pscustomobject]@{ Id = 'WorldSeedAndExploitRules'; Parent = 'SharedRuntimeMechanisms'; Role = 'state/query'; Seam = 'world seed exploit rules'; Responsibility = '特殊种子规则和吞噬者漏洞保护状态。' }
    [pscustomobject]@{ Id = 'EnvironmentDamageAndSeatState'; Parent = 'SharedRuntimeMechanisms'; Role = 'state'; Seam = 'environment damage seat port'; Responsibility = '环境黑暗伤害和额外座位信息状态。' }
    [pscustomobject]@{ Id = 'WorldEnvironmentScanHelpers'; Parent = 'SharedRuntimeMechanisms'; Role = 'query'; Seam = 'world environment scan port'; Responsibility = '洞察扫描、不可破坏墙扫描和虚空镜辅助查询。' }
    [pscustomobject]@{ Id = 'InputProfilesAndConfiguration'; Parent = 'SharedRuntimeMechanisms'; Role = 'adapter/state'; Seam = 'input profile configuration port'; Responsibility = '玩家输入档案和按键配置。' }
    [pscustomobject]@{ Id = 'InputTriggerState'; Parent = 'SharedRuntimeMechanisms'; Role = 'adapter/state'; Seam = 'input trigger port'; Responsibility = '触发器集合、当前输入模式和触发器打包状态。' }
    [pscustomobject]@{ Id = 'PlayerInputRuntimeState'; Parent = 'SharedRuntimeMechanisms'; Role = 'adapter/state'; Seam = 'player input runtime port'; Responsibility = '玩家输入运行时屏幕、按键和输入接管状态。' }
    [pscustomobject]@{ Id = 'LockOnTargetingState'; Parent = 'SharedRuntimeMechanisms'; Role = 'adapter/query'; Seam = 'lock-on targeting port'; Responsibility = '锁定范围、保持时间和目标选择状态。' }
    [pscustomobject]@{ Id = 'EntityAuthoritativeState'; Parent = 'SharedRuntimeMechanisms'; Role = 'state'; Seam = 'entity authoritative port'; Responsibility = '实体位置、旋转、尺寸和身份等权威基础状态。' }
    [pscustomobject]@{ Id = 'EntityShadowPresentationState'; Parent = 'SharedRuntimeMechanisms'; Role = 'presentation/snapshot'; Seam = 'entity shadow projection'; Responsibility = '实体阴影的位置、旋转和表现快照。' }
    [pscustomobject]@{ Id = 'ArmorSetBonusDefinitions'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/catalog'; Seam = 'armor set bonus definition port'; Responsibility = '套装加成效果、Builder 和查询上下文定义。' }
    [pscustomobject]@{ Id = 'ArmorSetBonusCatalog'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/catalog'; Seam = 'armor set bonus catalog'; Responsibility = '套装加成集合和按物品查询目录。' }
    [pscustomobject]@{ Id = 'WingStatsDefinition'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/value object'; Seam = 'wing stats definition port'; Responsibility = '翅膀飞行时间、速度和悬停属性定义。' }
    [pscustomobject]@{ Id = 'EquipmentLoadoutState'; Parent = 'SharedRuntimeMechanisms'; Role = 'state'; Seam = 'equipment loadout port'; Responsibility = '装备栏和染料栏位状态。' }
)

$secondLevelSplitDefinitions = @(
    [pscustomobject]@{ Id = 'TileObjectDefinitionInheritanceState'; Baseline = 'TileObjectDefinitionCatalog'; Parent = 'ContentCatalog'; Role = 'definition/catalog'; Seam = 'tile definition inheritance port'; Responsibility = 'TileObjectData 的父对象、alternates 和写时复制关系。' }
    [pscustomobject]@{ Id = 'TileObjectDefinitionPlacementAndStyleState'; Baseline = 'TileObjectDefinitionCatalog'; Parent = 'ContentCatalog'; Role = 'definition/catalog'; Seam = 'tile placement/style definition port'; Responsibility = 'TileObjectData 的锚点、液体、放置 hook、尺寸和绘制样式。' }
    [pscustomobject]@{ Id = 'SharedTimeLoggerFrameCoordinationState'; Baseline = 'SharedTimeLoggerCoordinatorState'; Parent = 'SharedRuntimeMechanisms'; Role = 'diagnostics state'; Seam = 'time logger frame coordination port'; Responsibility = '计时器帧边界、条目注册和下一帧控制状态。' }
    [pscustomobject]@{ Id = 'SharedTimeLoggerRenderMetricsState'; Baseline = 'SharedTimeLoggerCoordinatorState'; Parent = 'SharedRuntimeMechanisms'; Role = 'diagnostics state'; Seam = 'time logger render metrics port'; Responsibility = '绘制阶段计时指标、格式缓存和实验显示状态。' }
    [pscustomobject]@{ Id = 'SharedSceneScanAndZoneState'; Baseline = 'SharedSceneMetricsSnapshot'; Parent = 'SharedRuntimeMechanisms'; Role = 'query/cache'; Seam = 'scene scan and zone port'; Responsibility = '场景扫描输入、区域资格和扫描位置快照。' }
    [pscustomobject]@{ Id = 'SharedSceneAggregateAndDecorationState'; Baseline = 'SharedSceneMetricsSnapshot'; Parent = 'SharedRuntimeMechanisms'; Role = 'query/cache'; Seam = 'scene aggregate decoration port'; Responsibility = 'Tile、实体、音乐和装饰物聚合计数及视觉标记。' }
    [pscustomobject]@{ Id = 'SharedDropRuleResolutionAndCatalogState'; Baseline = 'SharedDropRuleDefinitions'; Parent = 'SharedRuntimeMechanisms'; Role = 'query/adapter'; Seam = 'drop rule resolution port'; Responsibility = '掉落尝试、概率结果、数据库和 resolver 解析边界。' }
    [pscustomobject]@{ Id = 'SharedDropRuleSelectionAndChainState'; Baseline = 'SharedDropRuleDefinitions'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/query'; Seam = 'drop rule selection chain port'; Responsibility = '规则选择、条件分支、选项数量和链式规则定义。' }
    [pscustomobject]@{ Id = 'AudioLegacySoundCatalogState'; Baseline = 'AudioLegacySoundAdapterState'; Parent = 'ClientPresentationAndTools'; Role = 'definition/presentation'; Seam = 'legacy sound catalog port'; Responsibility = '旧音效定义、可跟踪声音目录和播放服务引用。' }
    [pscustomobject]@{ Id = 'AudioLegacySoundInstanceState'; Baseline = 'AudioLegacySoundAdapterState'; Parent = 'ClientPresentationAndTools'; Role = 'presentation/adapter'; Seam = 'legacy sound instance port'; Responsibility = '旧音效实例、可跟踪实例和活动声音回收状态。' }
    [pscustomobject]@{ Id = 'MainCageBirdAndTerrestrialAnimationState'; Baseline = 'MainCageAnimationState'; Parent = 'RuntimeComposition'; Role = 'presentation state'; Seam = 'bird terrestrial cage animation port'; Responsibility = '鸟类和陆生捕获物的笼具帧与动画计数。' }
    [pscustomobject]@{ Id = 'MainCageAquaticAndAmphibianAnimationState'; Baseline = 'MainCageAnimationState'; Parent = 'RuntimeComposition'; Role = 'presentation state'; Seam = 'aquatic amphibian cage animation port'; Responsibility = '水生和两栖捕获物的笼具、鱼缸与罐体帧。' }
    [pscustomobject]@{ Id = 'SharedFishingDropCatalogState'; Baseline = 'SharedFishingDropRuleDefinitions'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/catalog'; Seam = 'fishing drop catalog port'; Responsibility = '鱼获候选、频率、稀有度和规则填充目录。' }
    [pscustomobject]@{ Id = 'SharedFishingConditionContextState'; Baseline = 'SharedFishingDropRuleDefinitions'; Parent = 'SharedRuntimeMechanisms'; Role = 'query/state'; Seam = 'fishing condition context port'; Responsibility = '钓鱼上下文、条件对象和任务鱼筛选状态。' }
    [pscustomobject]@{ Id = 'SharedBiomeDungeonAndTerrainState'; Baseline = 'SharedWorldGenerationBiomeSupport'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/query'; Seam = 'biome terrain generation port'; Responsibility = '地牢控制线、花岗岩、沙漠和地形 pass 生成状态。' }
    [pscustomobject]@{ Id = 'SharedBiomeCaveHouseAndStructureState'; Baseline = 'SharedWorldGenerationBiomeSupport'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/query'; Seam = 'cave house structure generation port'; Responsibility = '洞穴房屋、结构放置和洞穴生物群落辅助数据。' }
    [pscustomobject]@{ Id = 'UiItemSortingDefinitions'; Baseline = 'UiItemSorting'; Parent = 'ClientPresentationAndTools'; Role = 'presentation/query'; Seam = 'item sorting definition port'; Responsibility = '物品排序层、白名单和伤害类型排序定义。' }
    [pscustomobject]@{ Id = 'UiItemSortingExecutionState'; Baseline = 'UiItemSorting'; Parent = 'ClientPresentationAndTools'; Role = 'presentation state'; Seam = 'item sorting execution port'; Responsibility = '一次排序批次、缓存槽位和弹药填充工作集。' }
    [pscustomobject]@{ Id = 'SharedDungeonRoomCoreAndSettings'; Baseline = 'SharedDungeonRoomDefinitions'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/catalog'; Seam = 'dungeon room core settings port'; Responsibility = '地牢房间核心实例、边界和房间设置。' }
    [pscustomobject]@{ Id = 'SharedDungeonRoomShapeVariants'; Baseline = 'SharedDungeonRoomDefinitions'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/catalog'; Seam = 'dungeon room shape variant port'; Responsibility = '地牢房间形状、Biome 变体和旧布局形状数据。' }
    [pscustomobject]@{ Id = 'SharedContentUiTextAndOptionWidgets'; Baseline = 'SharedContentUiWidgets'; Parent = 'SharedRuntimeMechanisms'; Role = 'presentation'; Seam = 'content UI text option port'; Responsibility = '内容 UI 文本、标题和组选项控件状态。' }
    [pscustomobject]@{ Id = 'SharedContentUiContainersAndProgress'; Baseline = 'SharedContentUiWidgets'; Parent = 'SharedRuntimeMechanisms'; Role = 'presentation'; Seam = 'content UI container progress port'; Responsibility = '内容 UI 面板、列表、滚动条和进度控件状态。' }
    [pscustomobject]@{ Id = 'SharedDungeonStyleSetCatalog'; Baseline = 'SharedDungeonStyleCatalog'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/catalog'; Seam = 'dungeon style set catalog port'; Responsibility = '地牢样式集合和生物群落样式索引。' }
    [pscustomobject]@{ Id = 'SharedDungeonStyleEntryDefinitions'; Baseline = 'SharedDungeonStyleCatalog'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/catalog'; Seam = 'dungeon style entry port'; Responsibility = '单个地牢样式的砖、墙、家具和房间定义。' }
    [pscustomobject]@{ Id = 'SharedPopupTextState'; Baseline = 'SharedPopupAndCombatText'; Parent = 'SharedRuntimeMechanisms'; Role = 'presentation'; Seam = 'popup text presentation port'; Responsibility = '物品、金币、声纳和通用弹出文本状态。' }
    [pscustomobject]@{ Id = 'SharedCombatTextState'; Baseline = 'SharedPopupAndCombatText'; Parent = 'SharedRuntimeMechanisms'; Role = 'presentation'; Seam = 'combat text presentation port'; Responsibility = '伤害、治疗和暴击战斗文本状态。' }
    [pscustomobject]@{ Id = 'SharedShaderBaseParameterState'; Baseline = 'SharedShaderData'; Parent = 'SharedRuntimeMechanisms'; Role = 'presentation'; Seam = 'shader parameter port'; Responsibility = '基础 shader、屏幕参数和效果参数缓存。' }
    [pscustomobject]@{ Id = 'SharedShaderFamilyCatalogState'; Baseline = 'SharedShaderData'; Parent = 'SharedRuntimeMechanisms'; Role = 'presentation'; Seam = 'shader family catalog port'; Responsibility = 'Armor、Hair、Misc shader 家族和集合目录。' }
    [pscustomobject]@{ Id = 'SharedTilePlacementGeometryModules'; Baseline = 'SharedTilePlacementModules'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/query'; Seam = 'tile placement geometry port'; Responsibility = 'Tile 尺寸、原点、坐标、样式和绘制模块。' }
    [pscustomobject]@{ Id = 'SharedTilePlacementAnchorAndHookModules'; Baseline = 'SharedTilePlacementModules'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/query'; Seam = 'tile placement anchor hook port'; Responsibility = '锚点、液体规则、交替 Tile 和放置 hook 模块。' }
    [pscustomobject]@{ Id = 'SharedCreativePowerContracts'; Baseline = 'SharedCreativePowerDefinitions'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/adapter'; Seam = 'creative power contract port'; Responsibility = '创意能力公开接口、权限和服务器配置契约。' }
    [pscustomobject]@{ Id = 'SharedCreativePowerDefinitionState'; Baseline = 'SharedCreativePowerDefinitions'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/state'; Seam = 'creative power definition port'; Responsibility = '创意能力基础类、滑杆/开关缓存和目标值状态。' }
    [pscustomobject]@{ Id = 'UiItemSlotStorageAndCraftingContexts'; Baseline = 'UiItemSlotContextDefinitions'; Parent = 'ClientPresentationAndTools'; Role = 'presentation/query'; Seam = 'item slot storage context port'; Responsibility = '背包、容器、商店、鼠标和制作上下文定义。' }
    [pscustomobject]@{ Id = 'UiItemSlotEquipmentAndCreativeContexts'; Baseline = 'UiItemSlotContextDefinitions'; Parent = 'ClientPresentationAndTools'; Role = 'presentation/query'; Seam = 'item slot equipment context port'; Responsibility = '装备、展示架、创意模式和世界交互上下文定义。' }
    [pscustomobject]@{ Id = 'MapTileEncodingAndStorageState'; Baseline = 'MapTileStorageAndUpdateState'; Parent = 'ClientPresentationAndTools'; Role = 'presentation/state'; Seam = 'map tile encoding storage port'; Responsibility = '地图 Tile 编码、颜色头和地图文件存储状态。' }
    [pscustomobject]@{ Id = 'MapTileUpdateQueueState'; Baseline = 'MapTileStorageAndUpdateState'; Parent = 'ClientPresentationAndTools'; Role = 'presentation/state'; Seam = 'map tile update queue port'; Responsibility = '地图区域更新队列、锁和待处理更新状态。' }
    [pscustomobject]@{ Id = 'SharedDungeonBoundsAndProgressionDefinitions'; Baseline = 'SharedDungeonGeometryAndPlacementDefinitions'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/query'; Seam = 'dungeon bounds progression port'; Responsibility = '地牢边界、中心和不可破坏墙进度层级定义。' }
    [pscustomobject]@{ Id = 'SharedDungeonDoorAndPlatformDefinitions'; Baseline = 'SharedDungeonGeometryAndPlacementDefinitions'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/query'; Seam = 'dungeon door platform placement port'; Responsibility = '地牢门、平台及其空间检查和放置覆盖数据。' }
    [pscustomobject]@{ Id = 'SharedItemUseAndToolCapabilityState'; Baseline = 'SharedItemUseToolAndCombatState'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/state'; Seam = 'item use tool capability port'; Responsibility = '物品使用时序、工具强度、放置和弹药能力。' }
    [pscustomobject]@{ Id = 'SharedItemCombatAndDamageCapabilityState'; Baseline = 'SharedItemUseToolAndCombatState'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/state'; Seam = 'item combat capability port'; Responsibility = '物品伤害、命中、恢复和职业伤害能力。' }
    [pscustomobject]@{ Id = 'SharedWorldItemLifecycleState'; Baseline = 'SharedWorldItemState'; Parent = 'SharedRuntimeMechanisms'; Role = 'state'; Seam = 'world item lifecycle port'; Responsibility = '世界物品保留、拾取、传送带和生命周期状态。' }
    [pscustomobject]@{ Id = 'SharedWorldItemPayloadState'; Baseline = 'SharedWorldItemState'; Parent = 'SharedRuntimeMechanisms'; Role = 'state/projection'; Seam = 'world item payload port'; Responsibility = '世界物品内嵌的物品身份、堆叠和能力投影。' }
    [pscustomobject]@{ Id = 'SharedDungeonHallCoreAndSettings'; Baseline = 'SharedDungeonHallDefinitions'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/catalog'; Seam = 'dungeon hall core settings port'; Responsibility = '大厅实例、通用设置和规则化大厅参数。' }
    [pscustomobject]@{ Id = 'SharedDungeonHallLegacyAndGeometry'; Baseline = 'SharedDungeonHallDefinitions'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/catalog'; Seam = 'dungeon hall legacy geometry port'; Responsibility = '旧大厅、阶梯大厅、正弦大厅和入口几何状态。' }
    [pscustomobject]@{ Id = 'MainTileBehaviorMetadata'; Baseline = 'MainTileAndWallMetadata'; Parent = 'RuntimeComposition'; Role = 'catalog reference'; Seam = 'main tile behavior metadata port'; Responsibility = 'Main 的 Tile 行为、光照、可放置和框架元数据。' }
    [pscustomobject]@{ Id = 'MainWallAndGlobalTileMetadata'; Baseline = 'MainTileAndWallMetadata'; Parent = 'RuntimeComposition'; Role = 'catalog reference'; Seam = 'main wall metadata port'; Responsibility = 'Main 的墙体、全局合并和音乐淡出元数据。' }
    [pscustomobject]@{ Id = 'SharedTileObjectPreviewState'; Baseline = 'SharedTileObjectPlacementDefinitions'; Parent = 'SharedRuntimeMechanisms'; Role = 'query/snapshot'; Seam = 'tile object preview port'; Responsibility = 'TileObject 放置预览、缓存和有效性百分比。' }
    [pscustomobject]@{ Id = 'SharedTileObjectPlacementValueState'; Baseline = 'SharedTileObjectPlacementDefinitions'; Parent = 'SharedRuntimeMechanisms'; Role = 'value object/query'; Seam = 'tile object placement value port'; Responsibility = 'TileObject 坐标、样式、placement hook 和放置结果值。' }
    [pscustomobject]@{ Id = 'SharedLightingCoordinatorState'; Baseline = 'LightingEngineState'; Parent = 'SharedRuntimeMechanisms'; Role = 'state/query'; Seam = 'lighting coordinator port'; Responsibility = '新旧光照引擎切换、活动引擎和逐帧光照状态。' }
    [pscustomobject]@{ Id = 'SharedLegacyLightingState'; Baseline = 'LightingEngineState'; Parent = 'SharedRuntimeMechanisms'; Role = 'state/query'; Seam = 'legacy lighting port'; Responsibility = '旧光照扫描、临时光源和旧光照图状态。' }
    [pscustomobject]@{ Id = 'SharedItemEquipmentSlotState'; Baseline = 'SharedItemEquipmentAndPresentationState'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/state'; Seam = 'item equipment slot port'; Responsibility = '装备栏位、染料栏位和穿戴槽状态。' }
    [pscustomobject]@{ Id = 'SharedItemAppearanceAndTooltipState'; Baseline = 'SharedItemEquipmentAndPresentationState'; Parent = 'SharedRuntimeMechanisms'; Role = 'presentation'; Seam = 'item appearance tooltip port'; Responsibility = '物品颜色、声音、工具提示和外观表现状态。' }
    [pscustomobject]@{ Id = 'SharedItemStaticEconomyAndTimingRules'; Baseline = 'SharedItemStaticCatalogRules'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/catalog'; Seam = 'item economy timing rules port'; Responsibility = '物品价格、拾取半径、冷却和堆叠时序常量。' }
    [pscustomobject]@{ Id = 'SharedItemStaticCapabilityRules'; Baseline = 'SharedItemStaticCatalogRules'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/catalog'; Seam = 'item static capability port'; Responsibility = '物品静态能力表、装备槽类型和相位颜色目录。' }
    [pscustomobject]@{ Id = 'SharedDungeonGenerationCollectionsState'; Baseline = 'SharedDungeonGenerationDataState'; Parent = 'SharedRuntimeMechanisms'; Role = 'runtime state'; Seam = 'dungeon generation collections port'; Responsibility = '地牢迭代、入口、房间、走廊和保护边界集合。' }
    [pscustomobject]@{ Id = 'SharedDungeonGenerationScalarAndStyleState'; Baseline = 'SharedDungeonGenerationDataState'; Parent = 'SharedRuntimeMechanisms'; Role = 'runtime state'; Seam = 'dungeon generation scalar style port'; Responsibility = '地牢样式、物品类型和生成强度比例状态。' }
    [pscustomobject]@{ Id = 'MainBackgroundLayerState'; Baseline = 'MainBackgroundAndSeasonalState'; Parent = 'RuntimeComposition'; Role = 'presentation state'; Seam = 'main background layer port'; Responsibility = '背景图层、树木样式和洞穴/地狱背景状态。' }
    [pscustomobject]@{ Id = 'MainSeasonalAndTitleState'; Baseline = 'MainBackgroundAndSeasonalState'; Parent = 'RuntimeComposition'; Role = 'presentation state'; Seam = 'main seasonal title port'; Responsibility = '节日开关、强制节日和标题切换状态。' }
    [pscustomobject]@{ Id = 'SharedDungeonStyleConstantQueries'; Baseline = 'SharedDungeonGenerationQueries'; Parent = 'SharedRuntimeMechanisms'; Role = 'query/value object'; Seam = 'dungeon style constant query port'; Responsibility = '门、花盆、吊灯、平台、旗帜和陷阱样式常量。' }
    [pscustomobject]@{ Id = 'SharedDungeonGeometryRuleQueries'; Baseline = 'SharedDungeonGenerationQueries'; Parent = 'SharedRuntimeMechanisms'; Role = 'query'; Seam = 'dungeon geometry rule query port'; Responsibility = '大厅/房间深度、放置变化和马赛克规则查询。' }
    [pscustomobject]@{ Id = 'SharedStarParticleState'; Baseline = 'SharedWeatherParticleState'; Parent = 'SharedRuntimeMechanisms'; Role = 'presentation state'; Seam = 'star particle port'; Responsibility = '星体位置、坠落、闪烁和淡入动画状态。' }
    [pscustomobject]@{ Id = 'SharedCloudAndRainParticleState'; Baseline = 'SharedWeatherParticleState'; Parent = 'SharedRuntimeMechanisms'; Role = 'presentation state'; Seam = 'cloud rain particle port'; Responsibility = '云层和雨滴位置、速度、透明度及回收状态。' }
    [pscustomobject]@{ Id = 'UiElementLayoutAndDimensionsState'; Baseline = 'UiElementTreeState'; Parent = 'ClientPresentationAndTools'; Role = 'presentation'; Seam = 'UI element layout port'; Responsibility = 'UI 元素树的尺寸、边距、对齐和父子布局状态。' }
    [pscustomobject]@{ Id = 'UiElementInteractionAndLifecycleState'; Baseline = 'UiElementTreeState'; Parent = 'ClientPresentationAndTools'; Role = 'presentation'; Seam = 'UI element interaction lifecycle port'; Responsibility = 'UI 初始化、鼠标交互、快照器和 UIState 生命周期状态。' }
    [pscustomobject]@{ Id = 'WorldFileMetadataIdentityState'; Baseline = 'WorldFileMetadataAndSession'; Parent = 'PersistenceAndRecovery'; Role = 'snapshot state'; Seam = 'world file metadata identity port'; Responsibility = '世界尺寸、创建时间、种子原文和世界标识元数据。' }
    [pscustomobject]@{ Id = 'WorldFileSessionAndValidityState'; Baseline = 'WorldFileMetadataAndSession'; Parent = 'PersistenceAndRecovery'; Role = 'snapshot state'; Seam = 'world file session validity port'; Responsibility = '世界加载状态、模式开关、有效性和地图路径状态。' }
    [pscustomobject]@{ Id = 'WorldFileTileHeaderCoreState'; Baseline = 'WorldFileTilePacking'; Parent = 'PersistenceAndRecovery'; Role = 'adapter state'; Seam = 'world tile core header port'; Responsibility = '世界 Tile 核心压缩头位和基础布局。' }
    [pscustomobject]@{ Id = 'WorldFileTileHeaderExtensionState'; Baseline = 'WorldFileTilePacking'; Parent = 'PersistenceAndRecovery'; Role = 'adapter state'; Seam = 'world tile extension header port'; Responsibility = '世界 Tile 扩展压缩头位和高阶标记布局。' }
    [pscustomobject]@{ Id = 'MinecartMotionAndTrackState'; Baseline = 'MinecartMotionState'; Parent = 'SharedRuntimeMechanisms'; Role = 'state/query'; Seam = 'minecart motion track port'; Responsibility = '矿车速度、轨道连接、加速和轨道类型状态。' }
    [pscustomobject]@{ Id = 'MinecartDecorationAndSwitchState'; Baseline = 'MinecartMotionState'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/presentation'; Seam = 'minecart decoration switch port'; Responsibility = '矿车装饰帧、端点、纹理和轨道切换状态。' }
    [pscustomobject]@{ Id = 'PlayerMovementCapabilityState'; Baseline = 'PlayerIntentAndMovementState'; Parent = 'SharedRuntimeMechanisms'; Role = 'state/query'; Seam = 'player movement capability port'; Responsibility = '飞行、翅膀、跳跃和便携座椅运动能力状态。' }
    [pscustomobject]@{ Id = 'PlayerIntentAndInteractionState'; Baseline = 'PlayerIntentAndMovementState'; Parent = 'SharedRuntimeMechanisms'; Role = 'state/query'; Seam = 'player intent interaction port'; Responsibility = '玩家意图推断和交互锚点状态。' }
    [pscustomobject]@{ Id = 'RecipeDefinitionState'; Baseline = 'RecipeDefinitionCatalog'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/catalog'; Seam = 'recipe definition port'; Responsibility = '配方输入、条件、制作材料和环境要求。' }
    [pscustomobject]@{ Id = 'RecipeGroupCatalogState'; Baseline = 'RecipeDefinitionCatalog'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/catalog'; Seam = 'recipe group catalog port'; Responsibility = '配方组集合、文本格式和注册 ID 目录。' }
    [pscustomobject]@{ Id = 'SharedDustParticleState'; Baseline = 'SharedParticleAndGoreEffects'; Parent = 'SharedRuntimeMechanisms'; Role = 'presentation state'; Seam = 'dust particle port'; Responsibility = 'Dust 粒子运动、光照、shader 和帧状态。' }
    [pscustomobject]@{ Id = 'SharedGoreEffectState'; Baseline = 'SharedParticleAndGoreEffects'; Parent = 'SharedRuntimeMechanisms'; Role = 'presentation state'; Seam = 'gore effect port'; Responsibility = 'Gore 运动、寿命、贴图帧和回收状态。' }
    [pscustomobject]@{ Id = 'MainDerivedWorldAndSessionQueries'; Baseline = 'MainDerivedPropertiesAndEvents'; Parent = 'RuntimeComposition'; Role = 'derived/query'; Seam = 'main derived world session port'; Responsibility = '世界模式、路径、资格和会话对象的只读派生查询。' }
    [pscustomobject]@{ Id = 'MainDerivedInputAndPresentationQueries'; Baseline = 'MainDerivedPropertiesAndEvents'; Parent = 'RuntimeComposition'; Role = 'derived/query'; Seam = 'main derived input presentation port'; Responsibility = 'UI 缩放、鼠标、天气表现和诊断投影查询。' }
    [pscustomobject]@{ Id = 'SharedChatSnippetPresentationState'; Baseline = 'SharedChatPresentation'; Parent = 'SharedRuntimeMechanisms'; Role = 'projection/presentation'; Seam = 'chat snippet presentation port'; Responsibility = '文本、标签、字形和定位片段表现状态。' }
    [pscustomobject]@{ Id = 'SharedChatMonitorAndCommandState'; Baseline = 'SharedChatPresentation'; Parent = 'SharedRuntimeMechanisms'; Role = 'projection/adapter'; Seam = 'chat monitor command port'; Responsibility = '聊天监视器、消息缓存和命令格式化处理状态。' }
    [pscustomobject]@{ Id = 'FishingAttemptState'; Baseline = 'SharedFishingAttemptAndConditions'; Parent = 'SharedRuntimeMechanisms'; Role = 'state/query'; Seam = 'fishing attempt port'; Responsibility = '一次钓鱼尝试的地点、概率、环境和结果状态。' }
    [pscustomobject]@{ Id = 'PlayerFishingConditionState'; Baseline = 'SharedFishingAttemptAndConditions'; Parent = 'SharedRuntimeMechanisms'; Role = 'query/state'; Seam = 'player fishing condition port'; Responsibility = '玩家鱼竿、鱼饵和最终钓鱼等级条件。' }
    [pscustomobject]@{ Id = 'TileCellMaterialAndLiquidState'; Baseline = 'TileCellStorage'; Parent = 'WorldStorage'; Role = 'authoritative snapshot'; Seam = 'tile material liquid port'; Responsibility = '单格 Tile 类型、墙体和液体材料状态。' }
    [pscustomobject]@{ Id = 'TileCellFrameAndBitState'; Baseline = 'TileCellStorage'; Parent = 'WorldStorage'; Role = 'authoritative snapshot'; Seam = 'tile frame bit port'; Responsibility = '单格 Tile 帧坐标、头位和形状位状态。' }
    [pscustomobject]@{ Id = 'CameraMatrixAndViewportState'; Baseline = 'CameraAndVertexPresentation'; Parent = 'ClientPresentationAndTools'; Role = 'presentation'; Seam = 'camera matrix viewport port'; Responsibility = '相机视口、缩放、平移和图形变换矩阵状态。' }
    [pscustomobject]@{ Id = 'VertexStripAndColorState'; Baseline = 'CameraAndVertexPresentation'; Parent = 'ClientPresentationAndTools'; Role = 'presentation'; Seam = 'vertex strip color port'; Responsibility = '顶点条、顶点声明、索引和顶点颜色状态。' }
    [pscustomobject]@{ Id = 'WorldFileRecoveryVersionState'; Baseline = 'WorldFileRecoveryIo'; Parent = 'PersistenceAndRecovery'; Role = 'adapter state'; Seam = 'world file recovery version port'; Responsibility = '世界文件锁、版本和云端恢复异常状态。' }
    [pscustomobject]@{ Id = 'WorldFileTemporaryEventState'; Baseline = 'WorldFileRecoveryIo'; Parent = 'PersistenceAndRecovery'; Role = 'adapter state'; Seam = 'world file temporary event port'; Responsibility = '世界文件恢复期间的天气、节日和事件临时值。' }
    [pscustomobject]@{ Id = 'SharedAmbientSkyCatalogState'; Baseline = 'SharedAmbientSkyAndWindState'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/presentation'; Seam = 'ambient sky catalog port'; Responsibility = '树冠区域、天空变体和背景闪烁定义。' }
    [pscustomobject]@{ Id = 'SharedAmbientSpawnAndWindState'; Baseline = 'SharedAmbientSkyAndWindState'; Parent = 'SharedRuntimeMechanisms'; Role = 'state/query'; Seam = 'ambient spawn wind port'; Responsibility = '环境实体生成条件、风点和尝试计时状态。' }
    [pscustomobject]@{ Id = 'SharedCreativePowerIconCatalogState'; Baseline = 'SharedCreativePowerPresentation'; Parent = 'SharedRuntimeMechanisms'; Role = 'presentation/catalog'; Seam = 'creative power icon catalog port'; Responsibility = '创意能力分类、图标网格和选中颜色目录。' }
    [pscustomobject]@{ Id = 'SharedCreativePowerUiLayoutState'; Baseline = 'SharedCreativePowerPresentation'; Parent = 'SharedRuntimeMechanisms'; Role = 'presentation'; Seam = 'creative power UI layout port'; Responsibility = '创意能力 UI 元素请求尺寸和布局参数。' }
    [pscustomobject]@{ Id = 'SharedDungeonLegacyPlacementState'; Baseline = 'SharedDungeonLegacyGenerationGlobals'; Parent = 'SharedRuntimeMechanisms'; Role = 'runtime state'; Seam = 'legacy dungeon placement port'; Responsibility = '旧地牢位置、砖墙类型、边界和入口放置状态。' }
    [pscustomobject]@{ Id = 'SharedDungeonLegacyRuleState'; Baseline = 'SharedDungeonLegacyGenerationGlobals'; Parent = 'SharedRuntimeMechanisms'; Role = 'runtime state'; Seam = 'legacy dungeon rule port'; Responsibility = '旧地牢样式、瓦片判定、预生成和战利品规则状态。' }
    [pscustomobject]@{ Id = 'SharedGeneralTeleportAndInterceptionUtilities'; Baseline = 'SharedGeneralPureUtilities'; Parent = 'SharedRuntimeMechanisms'; Role = 'query/value object'; Seam = 'general teleport interception port'; Responsibility = '传送候选、追逐结果和拦截计算值对象。' }
    [pscustomobject]@{ Id = 'SharedGeneralRandomAndBufferUtilities'; Baseline = 'SharedGeneralPureUtilities'; Parent = 'SharedRuntimeMechanisms'; Role = 'query/value object'; Seam = 'general random buffer port'; Responsibility = '随机常量、正则缓存和 flood-fill 工作缓冲区。' }
    [pscustomobject]@{ Id = 'SharedCelebrationAndLanternEvents'; Baseline = 'SharedSeasonalWorldEventState'; Parent = 'SharedRuntimeMechanisms'; Role = 'state/definition'; Seam = 'celebration lantern event port'; Responsibility = '派对、灯笼夜和环境精灵季节事件状态。' }
    [pscustomobject]@{ Id = 'SharedRitualAndStormEvents'; Baseline = 'SharedSeasonalWorldEventState'; Parent = 'SharedRuntimeMechanisms'; Role = 'state/definition'; Seam = 'ritual storm event port'; Responsibility = '邪教仪式和沙尘暴事件计时及强度状态。' }
    [pscustomobject]@{ Id = 'UiItemSlotPulseAndHighlightState'; Baseline = 'UiItemSlotPresentationAndPulseState'; Parent = 'ClientPresentationAndTools'; Role = 'presentation state'; Seam = 'item slot pulse highlight port'; Responsibility = '物品槽高亮、发光颜色和脉冲动画状态。' }
    [pscustomobject]@{ Id = 'UiItemSlotDisplayAndInteractionState'; Baseline = 'UiItemSlotPresentationAndPulseState'; Parent = 'ClientPresentationAndTools'; Role = 'presentation state'; Seam = 'item slot display interaction port'; Responsibility = '物品槽显示选项、槽位引用和激活状态。' }
)

$thirdLevelSplitDefinitions = @(
    [pscustomobject]@{ Id = 'UiItemSortingLayerCatalogState'; Baseline = 'UiItemSorting'; PreviousPeer = 'UiItemSortingDefinitions'; Parent = 'ClientPresentationAndTools'; Role = 'definition/catalog'; Seam = 'item sorting layer catalog port'; Responsibility = '物品排序层类型和按类别注册的排序目录。' }
    [pscustomobject]@{ Id = 'UiItemSortingRegistryAndRankingState'; Baseline = 'UiItemSorting'; PreviousPeer = 'UiItemSortingDefinitions'; Parent = 'ClientPresentationAndTools'; Role = 'presentation/query'; Seam = 'item sorting registry ranking port'; Responsibility = '排序层注册表、伤害类型排名和排序层索引状态。' }
    [pscustomobject]@{ Id = 'MapEncodingCatalogAndIoState'; Baseline = 'MapTileStorageAndUpdateState'; PreviousPeer = 'MapTileEncodingAndStorageState'; Parent = 'ClientPresentationAndTools'; Role = 'adapter state'; Seam = 'map encoding and file I/O port'; Responsibility = '地图编码头、选项上限、压缩和地图文件 I/O 状态。' }
    [pscustomobject]@{ Id = 'MapTileCellState'; Baseline = 'MapTileStorageAndUpdateState'; PreviousPeer = 'MapTileEncodingAndStorageState'; Parent = 'ClientPresentationAndTools'; Role = 'snapshot state'; Seam = 'map tile cell snapshot port'; Responsibility = '单个地图 Tile 的类型、光照、颜色和更新标志。' }
    [pscustomobject]@{ Id = 'SharedContentUiOptionButtonState'; Baseline = 'SharedContentUiWidgets'; PreviousPeer = 'SharedContentUiTextAndOptionWidgets'; Parent = 'SharedRuntimeMechanisms'; Role = 'presentation/state'; Seam = 'content option button port'; Responsibility = '内容 UI 组选项按钮的选项、纹理、颜色和选择状态。' }
    [pscustomobject]@{ Id = 'SharedContentUiTextAndHeaderState'; Baseline = 'SharedContentUiWidgets'; PreviousPeer = 'SharedContentUiTextAndOptionWidgets'; Parent = 'SharedRuntimeMechanisms'; Role = 'presentation'; Seam = 'content text header port'; Responsibility = '内容 UI 文本和标题的文本、颜色、换行及布局状态。' }
    [pscustomobject]@{ Id = 'SharedDungeonRoomCoreState'; Baseline = 'SharedDungeonRoomDefinitions'; PreviousPeer = 'SharedDungeonRoomCoreAndSettings'; Parent = 'SharedRuntimeMechanisms'; Role = 'runtime state'; Seam = 'dungeon room lifecycle port'; Responsibility = '地牢房间实例的计算、生成、边界和处理生命周期状态。' }
    [pscustomobject]@{ Id = 'SharedDungeonRoomSettingsState'; Baseline = 'SharedDungeonRoomDefinitions'; PreviousPeer = 'SharedDungeonRoomCoreAndSettings'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/state'; Seam = 'dungeon room settings port'; Responsibility = '地牢房间类型、样式、进度和连接点配置。' }
    [pscustomobject]@{ Id = 'SharedDungeonHallCoreState'; Baseline = 'SharedDungeonHallDefinitions'; PreviousPeer = 'SharedDungeonHallCoreAndSettings'; Parent = 'SharedRuntimeMechanisms'; Role = 'runtime state'; Seam = 'dungeon hall lifecycle port'; Responsibility = '地牢大厅实例的计算、生成、端点和处理生命周期状态。' }
    [pscustomobject]@{ Id = 'SharedDungeonHallSettingsState'; Baseline = 'SharedDungeonHallDefinitions'; PreviousPeer = 'SharedDungeonHallCoreAndSettings'; Parent = 'SharedRuntimeMechanisms'; Role = 'definition/state'; Seam = 'dungeon hall settings port'; Responsibility = '地牢大厅类型、样式、裂砖和生成策略配置。' }
)

$fourthLevelSplitDefinitions = @(
    [pscustomobject]@{ Id = 'TileObjectPlacementRuleState'; PreviousPeer = 'TileObjectDefinitionPlacementAndStyleState'; Role = 'definition/catalog'; Seam = 'tile placement rule port'; Responsibility = 'TileObjectData 的锚点、液体限制、放置 hook 和放置规则。' }
    [pscustomobject]@{ Id = 'TileObjectStyleAndDrawState'; PreviousPeer = 'TileObjectDefinitionPlacementAndStyleState'; Role = 'definition/catalog'; Seam = 'tile style draw port'; Responsibility = 'TileObjectData 的尺寸、坐标、样式、随机样式和绘制状态。' }
    [pscustomobject]@{ Id = 'SharedTimeLoggerPhaseMetricsState'; PreviousPeer = 'SharedTimeLoggerRenderMetricsState'; Role = 'diagnostics state'; Seam = 'time logger phase metrics port'; Responsibility = 'TimeLogger 各阶段计时、预算和帧级性能指标。' }
    [pscustomobject]@{ Id = 'SharedTimeLoggerDisplayFormattingState'; PreviousPeer = 'SharedTimeLoggerRenderMetricsState'; Role = 'diagnostics/query'; Seam = 'time logger display formatting port'; Responsibility = 'TimeLogger 的 CPU、百分比、毫秒和显示格式缓存。' }
    [pscustomobject]@{ Id = 'SharedDropRuleSelectionAndConditionState'; PreviousPeer = 'SharedDropRuleSelectionAndChainState'; Role = 'definition/query'; Seam = 'drop rule selection condition port'; Responsibility = '掉落规则选择、条件分支、概率选项和筛选状态。' }
    [pscustomobject]@{ Id = 'SharedDropRuleChainState'; PreviousPeer = 'SharedDropRuleSelectionAndChainState'; Role = 'definition/query'; Seam = 'drop rule chain port'; Responsibility = '掉落规则链、链式结果和链隐藏策略。' }
    [pscustomobject]@{ Id = 'SharedBiomeTerrainPassState'; PreviousPeer = 'SharedBiomeDungeonAndTerrainState'; Role = 'definition/query'; Seam = 'biome terrain pass port'; Responsibility = '生物群落、洞穴、沙漠和地形 pass 的生成数据。' }
    [pscustomobject]@{ Id = 'SharedDungeonControlAndTrapState'; PreviousPeer = 'SharedBiomeDungeonAndTerrainState'; Role = 'definition/query'; Seam = 'dungeon control trap port'; Responsibility = '地牢控制线、死亡宝箱和陷阱地形的生成规则。' }
    [pscustomobject]@{ Id = 'SharedFishingConditionCatalogState'; PreviousPeer = 'SharedFishingDropCatalogState'; Role = 'definition/catalog'; Seam = 'fishing condition catalog port'; Responsibility = '鱼获条件、populator、稀有度和候选目录。' }
    [pscustomobject]@{ Id = 'SharedFishingDropResolutionState'; PreviousPeer = 'SharedFishingDropCatalogState'; Role = 'query/adapter'; Seam = 'fishing drop resolution port'; Responsibility = 'FishDropRule、可能性条目和鱼获规则解析结果。' }
    [pscustomobject]@{ Id = 'MainCageBirdAnimationState'; PreviousPeer = 'MainCageBirdAndTerrestrialAnimationState'; Role = 'presentation state'; Seam = 'bird cage animation port'; Responsibility = '鸟类捕获物的笼具帧、鱼缸帧和动画计数。' }
    [pscustomobject]@{ Id = 'MainCageTerrestrialCritterAnimationState'; PreviousPeer = 'MainCageBirdAndTerrestrialAnimationState'; Role = 'presentation state'; Seam = 'terrestrial critter cage animation port'; Responsibility = '陆生小动物和昆虫捕获物的笼具帧与动画计数。' }
    [pscustomobject]@{ Id = 'UiItemSortingCombatAndEquipmentCatalogState'; PreviousPeer = 'UiItemSortingLayerCatalogState'; Role = 'definition/catalog'; Seam = 'item sorting combat equipment port'; Responsibility = '武器、工具、装备和战斗类别的排序层目录。' }
    [pscustomobject]@{ Id = 'UiItemSortingConsumableAndMiscCatalogState'; PreviousPeer = 'UiItemSortingLayerCatalogState'; Role = 'definition/catalog'; Seam = 'item sorting consumable misc port'; Responsibility = '药剂、消耗品、杂项和通用类别的排序层目录。' }
    [pscustomobject]@{ Id = 'SharedSceneZoneDefinitionState'; PreviousPeer = 'SharedSceneScanAndZoneState'; Role = 'query/input'; Seam = 'scene zone definition port'; Responsibility = '场景区域资格、区域常量和区域判定输入。' }
    [pscustomobject]@{ Id = 'SharedSceneScanAccumulatorState'; PreviousPeer = 'SharedSceneScanAndZoneState'; Role = 'query/cache'; Seam = 'scene scan accumulator port'; Responsibility = '场景扫描计数、位置快照、最近实体和扫描缓存。' }
    [pscustomobject]@{ Id = 'SharedCreativePerPlayerPowerState'; PreviousPeer = 'SharedCreativePowerDefinitionState'; Role = 'definition/state'; Seam = 'creative per-player power port'; Responsibility = '按玩家创意能力的参数、滑杆和目标值状态。' }
    [pscustomobject]@{ Id = 'SharedCreativeSharedPowerState'; PreviousPeer = 'SharedCreativePowerDefinitionState'; Role = 'definition/state'; Seam = 'creative shared power port'; Responsibility = '共享创意能力的权限、开关和全局目标值状态。' }
    [pscustomobject]@{ Id = 'SharedDungeonStyleMaterialAndGeometryState'; PreviousPeer = 'SharedDungeonStyleEntryDefinitions'; Role = 'definition/catalog'; Seam = 'dungeon style material geometry port'; Responsibility = '地牢样式砖、墙、液体、边缘和几何材料定义。' }
    [pscustomobject]@{ Id = 'SharedDungeonStyleFurnitureAndRoomState'; PreviousPeer = 'SharedDungeonStyleEntryDefinitions'; Role = 'definition/catalog'; Seam = 'dungeon style furniture room port'; Responsibility = '地牢样式家具、箱体、房间和空间子样式定义。' }
    [pscustomobject]@{ Id = 'SharedSceneTileAggregateState'; PreviousPeer = 'SharedSceneAggregateAndDecorationState'; Role = 'query/cache'; Seam = 'scene tile aggregate port'; Responsibility = '场景 Tile、液体、实体和资源聚合计数。' }
    [pscustomobject]@{ Id = 'SharedSceneDecorationAndAudioState'; PreviousPeer = 'SharedSceneAggregateAndDecorationState'; Role = 'presentation/query'; Seam = 'scene decoration audio port'; Responsibility = '场景音乐、蜡烛、纪念碑、装饰物和音频标记。' }
    [pscustomobject]@{ Id = 'SharedAudioLegacySoundCatalogInstanceState'; PreviousPeer = 'AudioLegacySoundInstanceState'; Role = 'presentation/adapter'; Seam = 'legacy sound catalog instance port'; Responsibility = '旧音效实例的定义、资源和目录关联状态。' }
    [pscustomobject]@{ Id = 'SharedAudioLegacyTrackedInstanceState'; PreviousPeer = 'AudioLegacySoundInstanceState'; Role = 'presentation/adapter'; Seam = 'legacy tracked instance port'; Responsibility = '旧音效可跟踪实例集合、回收和活动引用状态。' }
    [pscustomobject]@{ Id = 'SharedDungeonRoomGeometryState'; PreviousPeer = 'SharedDungeonRoomShapeVariants'; Role = 'definition/catalog'; Seam = 'dungeon room geometry port'; Responsibility = '地牢房间位置、尺寸、边界和几何形状数据。' }
    [pscustomobject]@{ Id = 'SharedDungeonRoomVariantCatalogState'; PreviousPeer = 'SharedDungeonRoomShapeVariants'; Role = 'definition/catalog'; Seam = 'dungeon room variant catalog port'; Responsibility = '地牢房间 VARIANT、Biome 房间和最大变体目录。' }
    [pscustomobject]@{ Id = 'SharedAudioLegacySoundDefinitionCatalogState'; PreviousPeer = 'AudioLegacySoundCatalogState'; Role = 'definition/catalog'; Seam = 'legacy sound definition catalog port'; Responsibility = '旧音效定义、声音资源和分类目录。' }
    [pscustomobject]@{ Id = 'SharedAudioLegacySoundServicesState'; PreviousPeer = 'AudioLegacySoundCatalogState'; Role = 'presentation/adapter'; Seam = 'legacy sound services port'; Responsibility = '旧音效 TrackableSounds、服务集合和外部播放适配。' }
    [pscustomobject]@{ Id = 'MapEncodingHeaderCatalogState'; PreviousPeer = 'MapEncodingCatalogAndIoState'; Role = 'adapter state'; Seam = 'map encoding header catalog port'; Responsibility = '地图编码头、颜色选项和编码上限目录。' }
    [pscustomobject]@{ Id = 'MapIoRuntimeState'; PreviousPeer = 'MapEncodingCatalogAndIoState'; Role = 'adapter state'; Seam = 'map I/O runtime port'; Responsibility = '地图文件锁、场景缓存、雪量和压缩运行状态。' }
    [pscustomobject]@{ Id = 'MainTileBehaviorAndInteractionMetadata'; PreviousPeer = 'MainTileBehaviorMetadata'; Role = 'catalog reference'; Seam = 'main tile behavior interaction port'; Responsibility = 'Main 的 Tile 交互、碰撞、放置和行为元数据。' }
    [pscustomobject]@{ Id = 'MainTileLightingAndFrameMetadata'; PreviousPeer = 'MainTileBehaviorMetadata'; Role = 'catalog reference'; Seam = 'main tile lighting frame port'; Responsibility = 'Main 的 Tile 光照、框架、砖墙和表现元数据。' }
    [pscustomobject]@{ Id = 'UiItemSlotEquipmentAndDisplayContexts'; PreviousPeer = 'UiItemSlotEquipmentAndCreativeContexts'; Role = 'presentation/query'; Seam = 'item slot equipment display port'; Responsibility = '装备、染料、展示架和展示相关槽位上下文。' }
    [pscustomobject]@{ Id = 'UiItemSlotCreativeCraftingAndUtilityContexts'; PreviousPeer = 'UiItemSlotEquipmentAndCreativeContexts'; Role = 'presentation/query'; Seam = 'item slot creative crafting utility port'; Responsibility = '创意、制作、快捷、调试和世界交互槽位上下文。' }
    [pscustomobject]@{ Id = 'SharedItemEconomyAndValueRules'; PreviousPeer = 'SharedItemStaticEconomyAndTimingRules'; Role = 'definition/catalog'; Seam = 'item economy value rules port'; Responsibility = '物品货币、价格、稀有度和经济价值规则。' }
    [pscustomobject]@{ Id = 'SharedItemUseTimingAndStackRules'; PreviousPeer = 'SharedItemStaticEconomyAndTimingRules'; Role = 'definition/catalog'; Seam = 'item use timing stack port'; Responsibility = '物品使用时序、拾取、食物、冷却和堆叠规则。' }
    [pscustomobject]@{ Id = 'SharedPopupTextContentAndContextState'; PreviousPeer = 'SharedPopupTextState'; Role = 'presentation'; Seam = 'popup text content context port'; Responsibility = '弹出文本内容、金币、声纳和来源上下文。' }
    [pscustomobject]@{ Id = 'SharedPopupTextRenderLifecycleState'; PreviousPeer = 'SharedPopupTextState'; Role = 'presentation state'; Seam = 'popup text render lifecycle port'; Responsibility = '弹出文本位置、透明度、生命周期和渲染缓存。' }
    [pscustomobject]@{ Id = 'MainBackgroundLayerCatalogState'; PreviousPeer = 'MainBackgroundLayerState'; Role = 'presentation state'; Seam = 'main background layer catalog port'; Responsibility = 'Main 背景图层、场景集合和背景目录状态。' }
    [pscustomobject]@{ Id = 'MainBackgroundParallaxAndStyleState'; PreviousPeer = 'MainBackgroundLayerState'; Role = 'presentation state'; Seam = 'main background parallax style port'; Responsibility = 'Main 背景偏移、视差、树木和风格参数状态。' }
    [pscustomobject]@{ Id = 'SharedShaderFamilyDataState'; PreviousPeer = 'SharedShaderFamilyCatalogState'; Role = 'presentation'; Seam = 'shader family data port'; Responsibility = 'Hair、Misc、Armor 等 shader 家族数据和参数。' }
    [pscustomobject]@{ Id = 'SharedShaderRegistryAndLookupState'; PreviousPeer = 'SharedShaderFamilyCatalogState'; Role = 'presentation/query'; Seam = 'shader registry lookup port'; Responsibility = 'GameShaders、ShaderDataSet 注册、索引和查找状态。' }
    [pscustomobject]@{ Id = 'SharedDungeonStyleObjectConstants'; PreviousPeer = 'SharedDungeonStyleConstantQueries'; Role = 'query/value object'; Seam = 'dungeon style object constants port'; Responsibility = '地牢门、花盆、吊灯和平台对象常量查询。' }
    [pscustomobject]@{ Id = 'SharedDungeonStyleBannerAndTrapConstants'; PreviousPeer = 'SharedDungeonStyleConstantQueries'; Role = 'query/value object'; Seam = 'dungeon style banner trap constants port'; Responsibility = '地牢旗帜样式和陷阱类型常量查询。' }
    [pscustomobject]@{ Id = 'SharedTilePlacementCoordinateAndDrawModules'; PreviousPeer = 'SharedTilePlacementGeometryModules'; Role = 'definition/query'; Seam = 'tile placement coordinate draw port'; Responsibility = 'Tile 放置坐标、绘制模块和几何输出。' }
    [pscustomobject]@{ Id = 'SharedTilePlacementBaseAndStyleModules'; PreviousPeer = 'SharedTilePlacementGeometryModules'; Role = 'definition/query'; Seam = 'tile placement base style port'; Responsibility = 'Tile 放置基础、样式和共享几何输入模块。' }
    [pscustomobject]@{ Id = 'SharedWorldItemIdentityAndEconomyPayloadState'; PreviousPeer = 'SharedWorldItemPayloadState'; Role = 'state/projection'; Seam = 'world item identity economy payload port'; Responsibility = '世界物品身份、堆叠、价值和经济投影载荷。' }
    [pscustomobject]@{ Id = 'SharedWorldItemUseAndPresentationPayloadState'; PreviousPeer = 'SharedWorldItemPayloadState'; Role = 'state/projection'; Seam = 'world item use presentation payload port'; Responsibility = '世界物品使用、伤害、工具和表现投影载荷。' }
    [pscustomobject]@{ Id = 'NetworkRemoteClientConnectionAndStatusState'; PreviousPeer = 'NetworkRemoteClientState'; Role = 'adapter state'; Seam = 'remote client connection status port'; Responsibility = '远端客户端 Socket、连接、身份和状态文本。' }
    [pscustomobject]@{ Id = 'NetworkRemoteClientSectionAndRateLimitState'; PreviousPeer = 'NetworkRemoteClientState'; Role = 'adapter state'; Seam = 'remote client section rate-limit port'; Responsibility = '远端客户端区段、读取缓冲和反垃圾限制状态。' }
    [pscustomobject]@{ Id = 'NetworkSessionConfigurationState'; PreviousPeer = 'NetworkSessionCoordinatorState'; Role = 'adapter state'; Seam = 'network session configuration port'; Responsibility = '网络端口、连接上限、服务器策略和会话配置。' }
    [pscustomobject]@{ Id = 'NetworkSessionTransportAndThreadState'; PreviousPeer = 'NetworkSessionCoordinatorState'; Role = 'adapter state'; Seam = 'network session transport thread port'; Responsibility = '网络客户端、监听器、线程、广播和传输运行状态。' }
    [pscustomobject]@{ Id = 'SharedItemEmergencyStackingPolicyState'; PreviousPeer = 'ItemEmergencyStackingState'; Role = 'state/query'; Seam = 'emergency stacking policy port'; Responsibility = '紧急堆叠候选、距离和策略配置。' }
    [pscustomobject]@{ Id = 'SharedItemEmergencyStackingTransferState'; PreviousPeer = 'ItemEmergencyStackingState'; Role = 'state/query'; Seam = 'emergency stacking transfer port'; Responsibility = '紧急堆叠 Group、StackableItem 和 Transfer 运行态。' }
    [pscustomobject]@{ Id = 'SharedBestiaryInfoElementState'; PreviousPeer = 'BestiaryInfoElementsAndProviders'; Role = 'definition/presentation'; Seam = 'bestiary info element port'; Responsibility = '图鉴信息元素、显示事实和元素索引。' }
    [pscustomobject]@{ Id = 'SharedBestiaryCollectionProviderState'; PreviousPeer = 'BestiaryInfoElementsAndProviders'; Role = 'definition/presentation'; Seam = 'bestiary collection provider port'; Responsibility = '图鉴集合信息、UICollection provider 和集合接口。' }
    [pscustomobject]@{ Id = 'EntityIdentityAndMotionState'; PreviousPeer = 'EntityAuthoritativeState'; Role = 'state'; Seam = 'entity identity motion port'; Responsibility = '实体身份、位置、速度、方向和运动历史。' }
    [pscustomobject]@{ Id = 'EntityBoundsAndFluidState'; PreviousPeer = 'EntityAuthoritativeState'; Role = 'state'; Seam = 'entity bounds fluid port'; Responsibility = '实体尺寸、碰撞边界、液体状态和空间范围。' }
    [pscustomobject]@{ Id = 'SharedCreativePowerIconLocationCatalogState'; PreviousPeer = 'SharedCreativePowerIconCatalogState'; Role = 'presentation/catalog'; Seam = 'creative power icon location catalog port'; Responsibility = '创意能力图标分类、位置和图标资源目录。' }
    [pscustomobject]@{ Id = 'SharedCreativePowerIconLayoutState'; PreviousPeer = 'SharedCreativePowerIconCatalogState'; Role = 'presentation'; Seam = 'creative power icon layout port'; Responsibility = '创意能力图标行列、布局尺寸和选中颜色状态。' }
)

$fifthLevelSplitDefinitions = @(
    [pscustomobject]@{ Id = 'SharedTimeLoggerWorldRenderPhaseMetricsState'; PreviousPeer = 'SharedTimeLoggerPhaseMetricsState'; Role = 'diagnostics state'; Seam = 'time logger world render metrics port'; Responsibility = 'TimeLogger 世界绘制、光照、地图和背景阶段指标。' }
    [pscustomobject]@{ Id = 'SharedTimeLoggerEntityAndInterfacePhaseMetricsState'; PreviousPeer = 'SharedTimeLoggerPhaseMetricsState'; Role = 'diagnostics state'; Seam = 'time logger entity interface metrics port'; Responsibility = 'TimeLogger 实体绘制、界面、菜单和诊断阶段指标。' }
    [pscustomobject]@{ Id = 'TileObjectStyleCatalogState'; PreviousPeer = 'TileObjectStyleAndDrawState'; Role = 'definition/catalog'; Seam = 'tile style catalog port'; Responsibility = 'TileObjectData 的样式常量、样式覆盖、样式步进和随机样式目录。' }
    [pscustomobject]@{ Id = 'TileObjectDrawGeometryState'; PreviousPeer = 'TileObjectStyleAndDrawState'; Role = 'definition/catalog'; Seam = 'tile draw geometry port'; Responsibility = 'TileObjectData 的绘制偏移、尺寸、坐标、原点和几何布局。' }
    [pscustomobject]@{ Id = 'SharedFishingRarityConditionCatalogState'; PreviousPeer = 'SharedFishingConditionCatalogState'; Role = 'definition/catalog'; Seam = 'fishing rarity condition port'; Responsibility = '鱼获稀有度枚举、稀有度条件 delegate 和视觉频率定义。' }
    [pscustomobject]@{ Id = 'SharedFishingEnvironmentConditionCatalogState'; PreviousPeer = 'SharedFishingConditionCatalogState'; Role = 'definition/catalog'; Seam = 'fishing environment condition port'; Responsibility = '鱼获环境、深度、生物群落和世界状态条件 populator。' }
    [pscustomobject]@{ Id = 'SharedDropRuleConditionBranchState'; PreviousPeer = 'SharedDropRuleSelectionAndConditionState'; Role = 'definition/query'; Seam = 'drop rule condition branch port'; Responsibility = '掉落条件类型、条件字段和模式分支资格判断。' }
    [pscustomobject]@{ Id = 'SharedDropRuleSelectionAndQuantityState'; PreviousPeer = 'SharedDropRuleSelectionAndConditionState'; Role = 'definition/query'; Seam = 'drop rule selection quantity port'; Responsibility = '掉落选项、概率、数量、重掷和可用候选选择状态。' }
    [pscustomobject]@{ Id = 'SharedDungeonTrapPlacementState'; PreviousPeer = 'SharedDungeonControlAndTrapState'; Role = 'definition/query'; Seam = 'dungeon trap placement port'; Responsibility = '死亡宝箱陷阱点、陷阱数量和放置尝试状态。' }
    [pscustomobject]@{ Id = 'SharedDungeonControlLineGeometryState'; PreviousPeer = 'SharedDungeonControlAndTrapState'; Role = 'definition/query'; Seam = 'dungeon control line geometry port'; Responsibility = '地牢控制线节点、切线、半径、方向和样式几何。' }
    [pscustomobject]@{ Id = 'SharedSceneZoneGeometryAndThresholdState'; PreviousPeer = 'SharedSceneZoneDefinitionState'; Role = 'query/input'; Seam = 'scene zone geometry threshold port'; Responsibility = '场景扫描窗口、层高、阈值和区域几何输入。' }
    [pscustomobject]@{ Id = 'SharedSceneBiomeAndEventDefinitionState'; PreviousPeer = 'SharedSceneZoneDefinitionState'; Role = 'query/input'; Seam = 'scene biome event definition port'; Responsibility = '场景生物群落、天气、蜡烛和事件区域定义。' }
    [pscustomobject]@{ Id = 'TileObjectAnchorAndLiquidPlacementState'; PreviousPeer = 'TileObjectPlacementRuleState'; Role = 'definition/catalog'; Seam = 'tile anchor liquid placement port'; Responsibility = 'TileObjectData 的锚点、有效瓦片、液体死亡和液体放置规则。' }
    [pscustomobject]@{ Id = 'TileObjectPlacementHookAndBaseState'; PreviousPeer = 'TileObjectPlacementRuleState'; Role = 'definition/catalog'; Seam = 'tile placement hook base port'; Responsibility = 'TileObjectData 的放置 hook、基础对象、坐标模块和子瓦片。' }
    [pscustomobject]@{ Id = 'MainCageMammalAndReptileAnimationState'; PreviousPeer = 'MainCageTerrestrialCritterAnimationState'; Role = 'presentation state'; Seam = 'mammal reptile cage animation port'; Responsibility = 'Main 中兔、松鼠、蜗牛、鼠、龟和大鼠捕获物动画状态。' }
    [pscustomobject]@{ Id = 'MainCageInsectAndSmallCritterAnimationState'; PreviousPeer = 'MainCageTerrestrialCritterAnimationState'; Role = 'presentation state'; Seam = 'insect small critter cage animation port'; Responsibility = 'Main 中蝴蝶、蜻蜓、蝎、仙女、蠕虫和其他小动物动画状态。' }
    [pscustomobject]@{ Id = 'SharedAudioLegacyEnvironmentalInstanceState'; PreviousPeer = 'SharedAudioLegacySoundCatalogInstanceState'; Role = 'presentation/adapter'; Seam = 'legacy environmental instance port'; Responsibility = '旧音效环境、世界交互和实体反馈声音实例。' }
    [pscustomobject]@{ Id = 'SharedAudioLegacyPlayerAndInterfaceInstanceState'; PreviousPeer = 'SharedAudioLegacySoundCatalogInstanceState'; Role = 'presentation/adapter'; Seam = 'legacy player interface instance port'; Responsibility = '旧音效玩家反馈、菜单、相机和界面声音实例。' }
    [pscustomobject]@{ Id = 'SharedAudioLegacyGameplaySoundDefinitionCatalogState'; PreviousPeer = 'SharedAudioLegacySoundDefinitionCatalogState'; Role = 'definition/catalog'; Seam = 'legacy gameplay sound definition port'; Responsibility = '旧音效环境、世界交互和实体反馈声音定义目录。' }
    [pscustomobject]@{ Id = 'SharedAudioLegacyInterfaceSoundDefinitionCatalogState'; PreviousPeer = 'SharedAudioLegacySoundDefinitionCatalogState'; Role = 'definition/catalog'; Seam = 'legacy interface sound definition port'; Responsibility = '旧音效玩家反馈、菜单、相机和界面声音定义目录。' }
    [pscustomobject]@{ Id = 'MapEncodingHeaderBitCatalogState'; PreviousPeer = 'MapEncodingHeaderCatalogState'; Role = 'adapter state'; Seam = 'map header bit layout port'; Responsibility = '地图编码 Header 位布局、保留位和颜色位定义。' }
    [pscustomobject]@{ Id = 'MapEncodingOptionLimitState'; PreviousPeer = 'MapEncodingHeaderCatalogState'; Role = 'adapter state'; Seam = 'map encoding option limit port'; Responsibility = '地图编码绘制循环、选项上限、渐变上限和区块尺寸。' }
    [pscustomobject]@{ Id = 'UiItemSlotCreativeAndCraftingContextState'; PreviousPeer = 'UiItemSlotCreativeCraftingAndUtilityContexts'; Role = 'presentation/query'; Seam = 'item slot creative crafting context port'; Responsibility = '创意无限、牺牲和新制作界面槽位上下文。' }
    [pscustomobject]@{ Id = 'UiItemSlotHotbarDisplayAndUtilityContextState'; PreviousPeer = 'UiItemSlotCreativeCraftingAndUtilityContexts'; Role = 'presentation/query'; Seam = 'item slot hotbar display utility port'; Responsibility = '快捷栏、聊天、装备展示、旗帜和通用槽位上下文。' }
)

$sixthLevelSplitDefinitions = @(
    [pscustomobject]@{ Id = 'SharedTimeLoggerTileAndLiquidRenderMetricsState'; PreviousPeer = 'SharedTimeLoggerWorldRenderPhaseMetricsState'; Role = 'diagnostics state'; Seam = 'time logger tile liquid render metrics port'; Responsibility = 'TimeLogger 固体、液体、墙体、线和 Tile 附加绘制阶段指标。' }
    [pscustomobject]@{ Id = 'SharedTimeLoggerLightingMapAndBackgroundMetricsState'; PreviousPeer = 'SharedTimeLoggerWorldRenderPhaseMetricsState'; Role = 'diagnostics state'; Seam = 'time logger lighting map background metrics port'; Responsibility = 'TimeLogger 光照、地图、瀑布、天空和背景阶段指标。' }
    [pscustomobject]@{ Id = 'SharedFishingConditionCatalogPopulationState'; PreviousPeer = 'SharedFishingEnvironmentConditionCatalogState'; Role = 'definition/catalog'; Seam = 'fishing condition catalog population port'; Responsibility = '钓鱼条件目录的注册、集合和模式/资格元数据。' }
    [pscustomobject]@{ Id = 'SharedFishingEnvironmentPredicateState'; PreviousPeer = 'SharedFishingEnvironmentConditionCatalogState'; Role = 'definition/query'; Seam = 'fishing environment predicate port'; Responsibility = '钓鱼液体、深度、生物群落、海洋和世界事件环境条件。' }
    [pscustomobject]@{ Id = 'SharedDropRuleChanceAndQuantityState'; PreviousPeer = 'SharedDropRuleSelectionAndQuantityState'; Role = 'definition/query'; Seam = 'drop rule chance quantity port'; Responsibility = '掉落规则的概率、重掷、最小/最大数量和逐个掉落参数。' }
    [pscustomobject]@{ Id = 'SharedDropRuleOptionSelectionState'; PreviousPeer = 'SharedDropRuleSelectionAndQuantityState'; Role = 'definition/query'; Seam = 'drop rule option selection port'; Responsibility = '按模式、选项集合和候选规则选择掉落项的状态。' }
    [pscustomobject]@{ Id = 'TileObjectStyleDefinitionCatalogState'; PreviousPeer = 'TileObjectStyleCatalogState'; Role = 'definition/catalog'; Seam = 'tile object style definition port'; Responsibility = 'TileObjectData 的固定样式定义、样式常量和样式布局目录。' }
    [pscustomobject]@{ Id = 'TileObjectStyleSelectionAndOverrideState'; PreviousPeer = 'TileObjectStyleCatalogState'; Role = 'definition/query'; Seam = 'tile object style selection port'; Responsibility = 'TileObjectData 的样式覆盖、随机选择和样式步进状态。' }
    [pscustomobject]@{ Id = 'SharedDungeonRoomShapeGeometryState'; PreviousPeer = 'SharedDungeonRoomGeometryState'; Role = 'definition/catalog'; Seam = 'dungeon room shape geometry port'; Responsibility = '地牢房间内部/外部形状数据和形状尺寸变化。' }
    [pscustomobject]@{ Id = 'SharedDungeonRoomPlacementGeometryState'; PreviousPeer = 'SharedDungeonRoomGeometryState'; Role = 'definition/catalog'; Seam = 'dungeon room placement geometry port'; Responsibility = '地牢房间位置、边界尺寸、墙深和强度/端点布局。' }
    [pscustomobject]@{ Id = 'UiItemSortingWeaponAndToolCatalogState'; PreviousPeer = 'UiItemSortingCombatAndEquipmentCatalogState'; Role = 'definition/catalog'; Seam = 'item sorting weapon tool catalog port'; Responsibility = '物品排序的武器与工具层目录及其通用层定义。' }
    [pscustomobject]@{ Id = 'UiItemSortingArmorAndAccessoryCatalogState'; PreviousPeer = 'UiItemSortingCombatAndEquipmentCatalogState'; Role = 'definition/catalog'; Seam = 'item sorting armor accessory catalog port'; Responsibility = '物品排序的护甲、时装和装备层目录。' }
    [pscustomobject]@{ Id = 'SharedDungeonStyleFurnitureCatalogState'; PreviousPeer = 'SharedDungeonStyleFurnitureAndRoomState'; Role = 'definition/catalog'; Seam = 'dungeon furniture catalog port'; Responsibility = '地牢箱体、门、平台、灯具、家具和装饰物品目录。' }
    [pscustomobject]@{ Id = 'SharedDungeonStyleRoomVariantState'; PreviousPeer = 'SharedDungeonStyleFurnitureAndRoomState'; Role = 'definition/catalog'; Seam = 'dungeon room variant port'; Responsibility = '地牢生物群落房间类型和子样式关系。' }
    [pscustomobject]@{ Id = 'SharedAudioLegacyWorldEnvironmentInstanceState'; PreviousPeer = 'SharedAudioLegacyEnvironmentalInstanceState'; Role = 'presentation/adapter'; Seam = 'legacy world environment instance port'; Responsibility = '旧音效液体、机械、天气、地形和世界交互声音实例。' }
    [pscustomobject]@{ Id = 'SharedAudioLegacyEntityFeedbackInstanceState'; PreviousPeer = 'SharedAudioLegacyEnvironmentalInstanceState'; Role = 'presentation/adapter'; Seam = 'legacy entity feedback instance port'; Responsibility = '旧音效物品、NPC、移动、跳跃和交互反馈声音实例。' }
    [pscustomobject]@{ Id = 'SharedItemUseTimingAndConsumptionState'; PreviousPeer = 'SharedItemUseAndToolCapabilityState'; Role = 'definition/state'; Seam = 'item use timing consumption port'; Responsibility = '物品使用样式、时序、消耗、复用和使用表现能力。' }
    [pscustomobject]@{ Id = 'SharedItemToolPlacementCapabilityState'; PreviousPeer = 'SharedItemUseAndToolCapabilityState'; Role = 'definition/state'; Seam = 'item tool placement capability port'; Responsibility = '物品采掘、放置、弹药和工具能力。' }
    [pscustomobject]@{ Id = 'SharedSceneBiomeZoneDefinitionState'; PreviousPeer = 'SharedSceneBiomeAndEventDefinitionState'; Role = 'query/input'; Seam = 'scene biome zone definition port'; Responsibility = '场景腐化、猩红、神圣、地形和生物群落区域定义。' }
    [pscustomobject]@{ Id = 'SharedSceneWeatherAndEventZoneState'; PreviousPeer = 'SharedSceneBiomeAndEventDefinitionState'; Role = 'query/input'; Seam = 'scene weather event zone port'; Responsibility = '场景天气、微光、蜡烛和事件小游戏区域定义。' }
    [pscustomobject]@{ Id = 'SharedTilePaintRenderTargetState'; PreviousPeer = 'SharedTilePaintState'; Role = 'presentation state'; Seam = 'tile paint render target port'; Responsibility = 'TilePaintSystemV2 的渲染目标、缓存集合和绘制请求状态。' }
    [pscustomobject]@{ Id = 'SharedTilePaintVariationAndColorState'; PreviousPeer = 'SharedTilePaintState'; Role = 'definition/state'; Seam = 'tile paint variation color port'; Responsibility = 'Tile/墙/树/笼样式变体键与颜色缓存状态。' }
    [pscustomobject]@{ Id = 'SharedInvasionDamageTrackingState'; PreviousPeer = 'SharedInvasionEventState'; Role = 'runtime state'; Seam = 'invasion damage tracking port'; Responsibility = 'DD2 入侵伤害跟踪器及其胜利/击杀时间投影。' }
    [pscustomobject]@{ Id = 'SharedInvasionWaveAndArenaState'; PreviousPeer = 'SharedInvasionEventState'; Role = 'runtime state'; Seam = 'invasion wave arena port'; Responsibility = 'DD2 入侵波次、竞技场、生成暂停和掉落进度状态。' }
    [pscustomobject]@{ Id = 'SharedIssueReportCatalogState'; PreviousPeer = 'SharedStartupAndIssueReporting'; Role = 'diagnostics/adapter'; Seam = 'issue report catalog port'; Responsibility = '问题报告集合、报告时间和报告正文。' }
    [pscustomobject]@{ Id = 'SharedStartupAndRuntimeHostState'; PreviousPeer = 'SharedStartupAndIssueReporting'; Role = 'diagnostics/adapter'; Seam = 'startup runtime host port'; Responsibility = '启动参数、运行时宿主、平台句柄和服务依赖状态。' }
)

$currentPeerDefinitions = @($fineDefinitions + $splitFineDefinitions + $secondLevelSplitDefinitions + $thirdLevelSplitDefinitions)
foreach ($definition in $fourthLevelSplitDefinitions) {
    $peerDefinitions = @($currentPeerDefinitions | Where-Object Id -eq $definition.PreviousPeer)
    if ($peerDefinitions.Count -ne 1) {
        throw "Fourth-level previous peer '$($definition.PreviousPeer)' is not uniquely defined."
    }
    $baseline = ''
    if ($peerDefinitions[0].PSObject.Properties.Name -contains 'Baseline') {
        $baseline = [string]$peerDefinitions[0].Baseline
    }
    if ([string]::IsNullOrWhiteSpace($baseline)) {
        $baseline = $peerDefinitions[0].Id
    }
    $definition | Add-Member -NotePropertyName Baseline -NotePropertyValue $baseline
    $definition | Add-Member -NotePropertyName Parent -NotePropertyValue $peerDefinitions[0].Parent
}
$currentPeerDefinitions = @($currentPeerDefinitions + $fourthLevelSplitDefinitions)
foreach ($definition in $fifthLevelSplitDefinitions) {
    $peerDefinitions = @($currentPeerDefinitions | Where-Object Id -eq $definition.PreviousPeer)
    if ($peerDefinitions.Count -ne 1) {
        throw "Fifth-level previous peer '$($definition.PreviousPeer)' is not uniquely defined."
    }
    $baseline = ''
    if ($peerDefinitions[0].PSObject.Properties.Name -contains 'Baseline') {
        $baseline = [string]$peerDefinitions[0].Baseline
    }
    if ([string]::IsNullOrWhiteSpace($baseline)) {
        $baseline = $peerDefinitions[0].Id
    }
    $definition | Add-Member -NotePropertyName Baseline -NotePropertyValue $baseline
    $definition | Add-Member -NotePropertyName Parent -NotePropertyValue $peerDefinitions[0].Parent
}
$currentPeerDefinitions = @($currentPeerDefinitions + $fifthLevelSplitDefinitions)
foreach ($definition in $sixthLevelSplitDefinitions) {
    $peerDefinitions = @($currentPeerDefinitions | Where-Object Id -eq $definition.PreviousPeer)
    if ($peerDefinitions.Count -ne 1) {
        throw "Sixth-level previous peer '$($definition.PreviousPeer)' is not uniquely defined."
    }
    $baseline = ''
    if ($peerDefinitions[0].PSObject.Properties.Name -contains 'Baseline') {
        $baseline = [string]$peerDefinitions[0].Baseline
    }
    if ([string]::IsNullOrWhiteSpace($baseline)) {
        $baseline = $peerDefinitions[0].Id
    }
    $definition | Add-Member -NotePropertyName Baseline -NotePropertyValue $baseline
    $definition | Add-Member -NotePropertyName Parent -NotePropertyValue $peerDefinitions[0].Parent
}

$baselineFineDefinitions = @($fineDefinitions | Where-Object { $retiredFineSubsystemIds -notcontains $_.Id }) + @($splitFineDefinitions)
foreach ($baselineId in $secondLevelBaselineIds) {
    if (@($baselineFineDefinitions | Where-Object Id -eq $baselineId).Count -ne 1) {
        throw "Second-level baseline fine subsystem is not uniquely defined: $baselineId."
    }
}
$secondLevelFineDefinitions = @($baselineFineDefinitions | Where-Object { $secondLevelBaselineIds -notcontains $_.Id }) + @($secondLevelSplitDefinitions)
foreach ($baselineId in $thirdLevelBaselineIds) {
    if (@($secondLevelFineDefinitions | Where-Object Id -eq $baselineId).Count -ne 1) {
        throw "Third-level baseline fine subsystem is not uniquely defined: $baselineId."
    }
}
$thirdLevelFineDefinitions = @($secondLevelFineDefinitions | Where-Object { $thirdLevelBaselineIds -notcontains $_.Id }) + @($thirdLevelSplitDefinitions)
foreach ($baselineId in $fourthLevelBaselineIds) {
    if (@($thirdLevelFineDefinitions | Where-Object Id -eq $baselineId).Count -ne 1) {
        throw "Fourth-level baseline fine subsystem is not uniquely defined: $baselineId."
    }
}
$fineDefinitions = @($thirdLevelFineDefinitions | Where-Object { $fourthLevelBaselineIds -notcontains $_.Id }) + @($fourthLevelSplitDefinitions)
$fourthLevelFineDefinitions = @($fineDefinitions | Where-Object { $fifthLevelBaselineIds -notcontains $_.Id }) + @($fifthLevelSplitDefinitions)
$fineDefinitions = @($fourthLevelFineDefinitions | Where-Object { $sixthLevelBaselineIds -notcontains $_.Id }) + @($sixthLevelSplitDefinitions)

$definitionById = @{}
foreach ($definition in $fineDefinitions) {
    if ($definitionById.ContainsKey($definition.Id)) {
        throw "Duplicate fine subsystem definition: $($definition.Id)."
    }
    $definitionById[$definition.Id] = $definition
}

$inputFullPath = [System.IO.Path]::GetFullPath((Join-Path (Get-Location) $InputPath))
$outputFullPath = [System.IO.Path]::GetFullPath((Join-Path (Get-Location) $OutputPath))
if (-not (Test-Path -LiteralPath $inputFullPath)) {
    throw "Input document not found: $inputFullPath"
}

$rows = [System.Collections.Generic.List[object]]::new()
$parent = ''
foreach ($line in Get-Content -LiteralPath $inputFullPath) {
    if ($line -match '^### \d+\.\d+ 子系统：`([^`]+)`') {
        $parent = $Matches[1]
        continue
    }

    if ($line -match '^\|\s*(\d+)\s*\|\s*(field|property)\s*\|') {
        $cells = Split-MarkdownRow -Line $line
        if ($cells.Count -lt 11) {
            throw "Member row has fewer than 11 cells at parent '$parent': $line"
        }

        $rows.Add([pscustomobject]@{
            Parent = $parent
            Seq = [int]$cells[0]
            Kind = $cells[1]
            Type = $cells[2]
            RelativePath = $cells[3]
            AbsolutePath = $cells[4]
            SourceLine = [int]$cells[5]
            SourceColumn = [int]$cells[6]
            Member = $cells[7]
            CSharpType = $cells[8]
            Declaration = $cells[9]
            OriginalDeclaration = $cells[10]
            Cells = $cells
        })
    }
}

if ($rows.Count -ne 4542) {
    throw "Expected 4542 non-authoritative members, parsed $($rows.Count)."
}
if ((($rows | Where-Object Kind -eq 'field').Count) -ne 4017) {
    throw 'Expected 4017 fields.'
}
if ((($rows | Where-Object Kind -eq 'property').Count) -ne 525) {
    throw 'Expected 525 properties.'
}
$seqs = @($rows | Sort-Object Seq | Select-Object -ExpandProperty Seq)
if (($seqs -join ',') -ne ((1..4542) -join ',')) {
    throw 'Input sequence must cover exactly 1..4542.'
}

foreach ($row in $rows) {
    $baseFineSubsystem = Get-BaseFineSubsystemId -Row $row
    $secondLevelFineSubsystem = $baseFineSubsystem
    if ($secondLevelBaselineIds -contains $baseFineSubsystem) {
        $secondLevelFineSubsystem = Get-SecondLevelFineSubsystemId -Row $row -BaseFineSubsystem $baseFineSubsystem
    }
    $thirdLevelFineSubsystem = $secondLevelFineSubsystem
    if ($thirdLevelBaselineIds -contains $secondLevelFineSubsystem) {
        $thirdLevelFineSubsystem = Get-ThirdLevelFineSubsystemId -Row $row -SecondLevelFineSubsystem $secondLevelFineSubsystem
    }
    $fourthLevelFineSubsystem = $thirdLevelFineSubsystem
    if ($fourthLevelBaselineIds -contains $thirdLevelFineSubsystem) {
        $fourthLevelFineSubsystem = Get-FourthLevelFineSubsystemId -Row $row -ThirdLevelFineSubsystem $thirdLevelFineSubsystem
    }
    $previousPeerSubsystem = $secondLevelFineSubsystem
    if ($fourthLevelBaselineIds -contains $thirdLevelFineSubsystem) {
        $previousPeerSubsystem = $thirdLevelFineSubsystem
    } elseif ($thirdLevelBaselineIds -contains $secondLevelFineSubsystem) {
        $previousPeerSubsystem = $secondLevelFineSubsystem
    }
    if ($fifthLevelBaselineIds -contains $fourthLevelFineSubsystem) {
        $previousPeerSubsystem = $fourthLevelFineSubsystem
    }
    $fifthLevelFineSubsystem = $fourthLevelFineSubsystem
    if ($fifthLevelBaselineIds -contains $fourthLevelFineSubsystem) {
        $fifthLevelFineSubsystem = Get-FifthLevelFineSubsystemId -Row $row -FourthLevelFineSubsystem $fourthLevelFineSubsystem
    }
    if ($sixthLevelBaselineIds -contains $fifthLevelFineSubsystem) {
        $previousPeerSubsystem = $fifthLevelFineSubsystem
    }
    $fineSubsystem = Get-FineSubsystemId -Row $row
    $row | Add-Member -NotePropertyName BaseFineSubsystem -NotePropertyValue $baseFineSubsystem
    $row | Add-Member -NotePropertyName SecondLevelFineSubsystem -NotePropertyValue $secondLevelFineSubsystem
    $row | Add-Member -NotePropertyName ThirdLevelFineSubsystem -NotePropertyValue $thirdLevelFineSubsystem
    $row | Add-Member -NotePropertyName FourthLevelFineSubsystem -NotePropertyValue $fourthLevelFineSubsystem
    $row | Add-Member -NotePropertyName PreviousPeerSubsystem -NotePropertyValue $previousPeerSubsystem
    $row | Add-Member -NotePropertyName FineSubsystem -NotePropertyValue $fineSubsystem
    if (-not $definitionById.ContainsKey($row.FineSubsystem)) {
        throw "Mapped to undefined fine subsystem '$($row.FineSubsystem)'."
    }
}

$parentOrder = @(
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
$parentResponsibilities = @{
    RuntimeComposition = '启动、全局运行时状态、循环协调和组合引用；不把字段库存当成已实现调度器。'
    PersistenceAndRecovery = '世界/玩家文件恢复和存档元数据。'
    NetworkSessionAndSectionStreaming = '会话、消息、区段流和网络发布边界。'
    ContentLifecycleAndRegistration = '内容生命周期和注册；当前非权威成员库存无主记录。'
    WorldStorage = 'Tile 与容器的世界存储数据。'
    ContentCatalog = 'Tile 定义、内容样本和集合工厂。'
    IntentAndInteraction = '意图和交互；当前非权威成员库存无主记录。'
    ExternalBoundaries = '社交、创意工坊和外部联机适配。'
    SharedRuntimeMechanisms = '跨域复用的值类型、定义、查询、表现和诊断机制；本层继续细分以避免一个 catch-all。'
    ExternalDependencyOrGenerated = '第三方、平台互操作和生成/外部依赖。'
    ClientPresentationAndTools = '成就、音频、电影、图形、地图和 UI 表现。'
}

function Get-Stats {
    param([object[]]$Items)
    $fieldCount = @($Items | Where-Object Kind -eq 'field').Count
    $propertyCount = @($Items | Where-Object Kind -eq 'property').Count
    [pscustomobject]@{ Field = $fieldCount; Property = $propertyCount; Total = $fieldCount + $propertyCount }
}

$top50PeerGroups = @{
    AudioLegacySoundAdapterState = 'AudioPlaybackCoordinatorState、AudioTrackedSoundState'
    SharedDungeonRoomDefinitions = 'SharedDungeonHallDefinitions、SharedDungeonEntranceDefinitions、SharedDungeonFeatureDefinitions、SharedDungeonLayoutProviderDefinitions、SharedDungeonStyleCatalog、SharedDungeonGeometryAndPlacementDefinitions'
    SharedContentUiWidgets = 'SharedContentUiScreens、SharedContentUiCurrency、SharedWorldInteractionUi'
    SharedDungeonStyleCatalog = 'SharedDungeonRoomDefinitions、SharedDungeonHallDefinitions、SharedDungeonEntranceDefinitions、SharedDungeonFeatureDefinitions、SharedDungeonLayoutProviderDefinitions、SharedDungeonGeometryAndPlacementDefinitions'
    SharedShaderData = 'SharedEffectAndSkyPresentation、SharedParticlePresentation'
    SharedTilePlacementModules = 'SharedTileObjectPlacementDefinitions、SharedTileAnchorAndReachQueries、SharedTileFramingAndSignState'
    SharedCreativePowerDefinitions = 'SharedCreativePowerRuntimeManager、SharedCreativePowerPresentation、SharedCreativeUnlockProgress'
    UiItemSlotContextDefinitions = 'UiItemSlotTransferState、UiItemSlotPresentationAndPulseState、UiItemSorting、UiItemTooltipState'
    MapTileStorageAndUpdateState = 'MapOverlayAndLayerPresentation、WorldMapState'
    SharedDungeonGeometryAndPlacementDefinitions = 'SharedDungeonStyleCatalog、SharedDungeonRoomDefinitions、SharedDungeonHallDefinitions、SharedDungeonEntranceDefinitions、SharedDungeonFeatureDefinitions、SharedDungeonLayoutProviderDefinitions'
    SharedItemUseToolAndCombatState = 'SharedItemStaticCatalogRules、SharedItemIdentityAndStackState、SharedItemEquipmentAndPresentationState、SharedItemProgressionAndWorldInteractionState、SharedItemCommerceState、SharedItemBuffMountAndConsumableEffects、SharedItemDerivedQueries'
    SharedWorldItemState = 'EntityAuthoritativeState、EntityShadowPresentationState'
    SharedDungeonHallDefinitions = 'SharedDungeonRoomDefinitions、SharedDungeonEntranceDefinitions、SharedDungeonFeatureDefinitions、SharedDungeonLayoutProviderDefinitions、SharedDungeonStyleCatalog、SharedDungeonGeometryAndPlacementDefinitions'
    SharedTileObjectPlacementDefinitions = 'SharedTilePlacementModules、SharedTileAnchorAndReachQueries、SharedTileFramingAndSignState'
    LightingEngineState = 'LightMapCacheState、TileLightScannerState'
    SharedItemEquipmentAndPresentationState = 'SharedItemStaticCatalogRules、SharedItemIdentityAndStackState、SharedItemUseToolAndCombatState、SharedItemProgressionAndWorldInteractionState、SharedItemCommerceState、SharedItemBuffMountAndConsumableEffects、SharedItemDerivedQueries'
    SharedItemStaticCatalogRules = 'SharedItemIdentityAndStackState、SharedItemEquipmentAndPresentationState、SharedItemUseToolAndCombatState、SharedItemProgressionAndWorldInteractionState、SharedItemCommerceState、SharedItemBuffMountAndConsumableEffects、SharedItemDerivedQueries'
    SharedDungeonGenerationDataState = 'SharedDungeonCrawlerRuntimeState、SharedDungeonLegacyGenerationGlobals'
    SharedWeatherParticleState = 'SharedAmbientSkyAndWindState、SharedLightningGenerationState、SharedWaterfallState'
    UiElementTreeState = 'UiLayoutPrimitives、UiEventPayloads、UiInputPointerState'
    WorldFileMetadataAndSession = 'WorldFileTilePacking、WorldFileRecoveryIo、PlayerFileMetadataAndSession'
    MinecartMotionState = 'MinecartCustomizationState、TrackedProjectileReferenceState'
    PlayerIntentAndMovementState = 'PlayerItemPickupAndRespawnState、PlayerPreviewAndRejectionState'
    RecipeDefinitionCatalog = 'CraftingRequestState'
    SharedParticleAndGoreEffects = 'SharedShaderData、SharedParticlePresentation'
    SharedChatPresentation = 'SharedChatAndCommandProtocol、LocalizationCultureAndLanguageState、LocalizedTextValueState、LegacyLanguageCatalogState'
    SharedFishingAttemptAndConditions = 'SharedFishingDropRuleDefinitions、SharedFishingCatchEffects'
    CameraAndVertexPresentation = 'SharedCaptureAndCameraSupport、SharedShaderData、SharedParticleAndVertexRendering'
    WorldFileRecoveryIo = 'WorldFileTilePacking、WorldFileMetadataAndSession、PlayerFileMetadataAndSession'
    SharedAmbientSkyAndWindState = 'SharedWeatherParticleState、SharedLightningGenerationState、SharedWaterfallState'
    SharedCreativePowerPresentation = 'SharedCreativePowerDefinitions、SharedCreativePowerRuntimeManager、SharedCreativeUnlockProgress'
    SharedDungeonLegacyGenerationGlobals = 'SharedDungeonCrawlerRuntimeState、SharedDungeonGenerationDataState'
    SharedGeneralPureUtilities = 'SharedGeneralFilePlatformUtilities、SharedGeneralDiagnosticsUtilities、SharedGeneralDelegateAndMetadataUtilities'
    SharedSeasonalWorldEventState = 'SharedInvasionEventState、SharedWorldEventPresentationState、SharedInvasionAndBossTracking、SharedConditionalDialogueSupport、SharedTownRoomState'
    UiItemSlotPresentationAndPulseState = 'UiItemSlotContextDefinitions、UiItemSlotTransferState、UiItemSorting、UiItemTooltipState'
}

$top50TerminalReasons = @{
    TileObjectDefinitionCatalog = 'TileObjectData 的定义、锚点、尺寸、放置 hook 和 alternates 共用加载后只读生命周期。'
    SharedTimeLoggerCoordinatorState = '外层 TimeLogger 的注册、帧边界、日志缓冲和格式池共同维护同一静态协调不变量。'
    SharedSceneMetricsSnapshot = 'Reset、Scan、聚合和区域计算共同维护带失效条件的派生快照。'
    SharedDropRuleDefinitions = 'IItemDropRule、chain 和 resolver 共用同一个规则契约，继续按规则类型切分会制造跨组件不变量。'
    MainCageAnimationState = 'Main 的笼具帧注册表由单一动画生命周期驱动，当前没有独立 owner seam。'
    SharedFishingDropRuleDefinitions = '鱼获规则、条件、稀有度和 populator 共用启动期目录契约，尝试和交付已经有同级边界。'
    SharedWorldGenerationBiomeSupport = 'Biome、洞穴房屋和地形生成查询仍共享定义目录与加载生命周期。'
    UiItemSorting = '排序定义、白名单、缓存和一次排序批次共同构成一个 UI 排序事务。'
    SharedPopupAndCombatText = '弹出文本和战斗文本由同一表现载荷与生命周期消费，尚无独立状态 owner 证据。'
    MainTileAndWallMetadata = 'Main 的 Tile/墙元数据是成组初始化和只读查询的目录数组，字段切开会破坏初始化契约。'
    MainBackgroundAndSeasonalState = '背景和节日状态仍由同一 Main 表现更新路径维护，缺少独立写者证据。'
    SharedDungeonGenerationQueries = '地牢生成资格和几何查询是纯查询边界，继续按字段切分不会降低读取耦合。'
    WorldFileTilePacking = 'Tile 打包字段共同服务同一序列化格式和临时缓冲生命周期。'
    SharedParticleAndGoreEffects = '粒子/Gore 表现载荷由同一效果池消费，现阶段没有独立权威状态。'
    MainDerivedPropertiesAndEvents = '只读派生属性和事件声明共同形成 Main 的兼容投影，不拥有权威状态。'
    TileCellStorage = '单格 Tile 存储字段共同构成一个固定布局快照，按字段切分会放大访问和一致性成本。'
}

$secondLevelPeerGroups = @{}
foreach ($baselineId in $secondLevelBaselineIds) {
    $children = @(
        $secondLevelSplitDefinitions |
            Where-Object Baseline -eq $baselineId |
            Select-Object -ExpandProperty Id
    )
    if ($children.Count -lt 2) {
        throw "Second-level baseline '$baselineId' has fewer than two peer groups."
    }
    $secondLevelPeerGroups[$baselineId] = $children -join '、'
}

$thirdLevelPeerGroups = @{}
foreach ($baselineId in $thirdLevelBaselineIds) {
    $children = @(
        $thirdLevelSplitDefinitions |
            Where-Object PreviousPeer -eq $baselineId |
            Select-Object -ExpandProperty Id
    )
    if ($children.Count -lt 2) {
        throw "Third-level baseline '$baselineId' has fewer than two peer groups."
    }
    $thirdLevelPeerGroups[$baselineId] = $children -join '、'
}

$fourthLevelPeerGroups = @{}
foreach ($baselineId in $fourthLevelBaselineIds) {
    $children = @(
        $fourthLevelSplitDefinitions |
            Where-Object PreviousPeer -eq $baselineId |
            Select-Object -ExpandProperty Id
    )
    if ($children.Count -ne 2) {
        throw "Fourth-level peer '$baselineId' does not have exactly two child definitions."
    }
    $fourthLevelPeerGroups[$baselineId] = $children -join '、'
}

$fifthLevelPeerGroups = @{}
foreach ($baselineId in $fifthLevelBaselineIds) {
    $children = @(
        $fifthLevelSplitDefinitions |
            Where-Object PreviousPeer -eq $baselineId |
            Select-Object -ExpandProperty Id
    )
    if ($children.Count -ne 2) {
        throw "Fifth-level peer '$baselineId' does not have exactly two child definitions."
    }
    $fifthLevelPeerGroups[$baselineId] = $children -join '、'
}

$sixthLevelPeerGroups = @{}
foreach ($baselineId in $sixthLevelBaselineIds) {
    $children = @(
        $sixthLevelSplitDefinitions |
            Where-Object PreviousPeer -eq $baselineId |
            Select-Object -ExpandProperty Id
    )
    if ($children.Count -ne 2) {
        throw "Sixth-level peer '$baselineId' does not have exactly two child definitions."
    }
    $sixthLevelPeerGroups[$baselineId] = $children -join '、'
}

function Get-PublicDecompositionAssessment {
    param(
        [Parameter(Mandatory)][string]$FineSubsystemId,
        [string]$PreviousPeerId = ''
    )

    if (-not [string]::IsNullOrWhiteSpace($PreviousPeerId) -and $sixthLevelPeerGroups.ContainsKey($PreviousPeerId)) {
        return [pscustomobject]@{
            Decision = 'sixth-level-peer-split-complete'
            Boundary = "上一级 peer：$PreviousPeerId；同级边界：$($sixthLevelPeerGroups[$PreviousPeerId])"
            Evidence = 'declaration-type/source-path/member-family-confirmed; reader/writer/lifecycle-partial'
        }
    }

    if (-not [string]::IsNullOrWhiteSpace($PreviousPeerId) -and $fifthLevelPeerGroups.ContainsKey($PreviousPeerId)) {
        return [pscustomobject]@{
            Decision = 'fifth-level-peer-split-complete'
            Boundary = "上一级 peer：$PreviousPeerId；同级边界：$($fifthLevelPeerGroups[$PreviousPeerId])"
            Evidence = 'declaration-type/source-path/member-family-confirmed; reader/writer/lifecycle-partial'
        }
    }

    if (-not [string]::IsNullOrWhiteSpace($PreviousPeerId) -and $fourthLevelPeerGroups.ContainsKey($PreviousPeerId)) {
        return [pscustomobject]@{
            Decision = 'next-level-peer-split-complete'
            Boundary = "上一级 peer：$PreviousPeerId；同级边界：$($fourthLevelPeerGroups[$PreviousPeerId])"
            Evidence = 'declaration-type/source-path-confirmed; reader/writer/lifecycle-partial'
        }
    }

    if (-not [string]::IsNullOrWhiteSpace($PreviousPeerId) -and $thirdLevelPeerGroups.ContainsKey($PreviousPeerId)) {
        return [pscustomobject]@{
            Decision = 'third-level-peer-split-complete'
            Boundary = "上一级 peer：$PreviousPeerId；同级边界：$($thirdLevelPeerGroups[$PreviousPeerId])"
            Evidence = 'declaration-type/source-path-confirmed; reader/writer/lifecycle-partial'
        }
    }

    if (-not $secondLevelPeerGroups.ContainsKey($FineSubsystemId)) {
        $finalDefinition = @($fineDefinitions | Where-Object Id -eq $FineSubsystemId)
        if ($finalDefinition.Count -ne 1) {
            throw "Missing final fine-subsystem definition for baseline '$FineSubsystemId'."
        }

        return [pscustomobject]@{
            Decision = 'first-level-peer-split-complete'
            Boundary = "同级边界：$FineSubsystemId（首轮同级细分，未进入二次 peer 拆分清单）"
            Evidence = 'source-path/type-family-confirmed; reader/writer/lifecycle-partial'
        }
    }

    return [pscustomobject]@{
        Decision = 'second-level-peer-split-complete'
        Boundary = "同级边界：$($secondLevelPeerGroups[$FineSubsystemId])"
        Evidence = 'source-path/type-family-confirmed; reader/writer/lifecycle-partial'
    }
}

$allFineRows = @($rows | Where-Object FineSubsystem)
$allFineIds = @($allFineRows | Select-Object -ExpandProperty FineSubsystem -Unique)
if ($allFineIds.Count -ne 349) {
    throw "Expected 349 final fine subsystems with members, parsed $($allFineIds.Count)."
}
if ($allFineIds.Count -ne @($fineDefinitions | Where-Object { $allFineRows.FineSubsystem -contains $_.Id }).Count) {
    throw 'Fine subsystem definition and row set are inconsistent.'
}

$baselineStats = @(
    $rows |
        Group-Object Parent, BaseFineSubsystem |
        ForEach-Object {
            $items = $_.Group
            $stats = Get-Stats -Items $items
            [pscustomobject]@{
                Parent = $items[0].Parent
                Fine = $items[0].BaseFineSubsystem
                Field = $stats.Field
                Property = $stats.Property
                Total = $stats.Total
            }
        }
)
if ($baselineStats.Count -ne 239) {
    throw "Expected 239 second-level baseline subsystems, parsed $($baselineStats.Count)."
}

$fineStats = @(
    foreach ($definition in $fineDefinitions) {
        $fineRows = @($allFineRows | Where-Object FineSubsystem -eq $definition.Id)
        if ($fineRows.Count -eq 0) { continue }
        $stats = Get-Stats -Items $fineRows
            [pscustomobject]@{
                Parent = $definition.Parent
                Fine = $definition.Id
                Baseline = $fineRows[0].BaseFineSubsystem
                PreviousPeer = $fineRows[0].PreviousPeerSubsystem
                Role = $definition.Role
                Seam = $definition.Seam
            Field = $stats.Field
            Property = $stats.Property
            Total = $stats.Total
        }
    }
)

function Get-SplitChildIds {
    param([Parameter(Mandatory)][string]$PeerId)

    if ($sixthLevelPeerGroups.ContainsKey($PeerId)) {
        return @($sixthLevelSplitDefinitions | Where-Object PreviousPeer -eq $PeerId | Select-Object -ExpandProperty Id)
    }
    if ($fifthLevelPeerGroups.ContainsKey($PeerId)) {
        return @($fifthLevelSplitDefinitions | Where-Object PreviousPeer -eq $PeerId | Select-Object -ExpandProperty Id)
    }
    if ($fourthLevelPeerGroups.ContainsKey($PeerId)) {
        return @($fourthLevelSplitDefinitions | Where-Object PreviousPeer -eq $PeerId | Select-Object -ExpandProperty Id)
    }
    if ($thirdLevelPeerGroups.ContainsKey($PeerId)) {
        return @($thirdLevelSplitDefinitions | Where-Object PreviousPeer -eq $PeerId | Select-Object -ExpandProperty Id)
    }
    return @()
}

function Get-ActiveDescendantFineStats {
    param([Parameter(Mandatory)][string]$PeerId)

    $splitChildren = @(Get-SplitChildIds -PeerId $PeerId)
    if ($splitChildren.Count -eq 0) {
        return @($fineStats | Where-Object PreviousPeer -eq $PeerId | Sort-Object Fine)
    }

    $activeDescendants = [System.Collections.Generic.List[object]]::new()
    foreach ($childId in $splitChildren) {
        $directStats = @($fineStats | Where-Object Fine -eq $childId)
        if ($directStats.Count -gt 1) {
            throw "Fine subsystem '$childId' has multiple statistics rows."
        }
        if ($directStats.Count -eq 1) {
            $activeDescendants.Add($directStats[0])
            continue
        }
        foreach ($descendant in @(Get-ActiveDescendantFineStats -PeerId $childId)) {
            $activeDescendants.Add($descendant)
        }
    }
    return @($activeDescendants)
}

$rankedFineStats = @($fineStats | Sort-Object @{ Expression = 'Total'; Descending = $true }, @{ Expression = 'Field'; Descending = $true }, @{ Expression = 'Property'; Descending = $true }, Parent, Fine)
$allFineRanking = @(
    $rank = 0
    foreach ($stat in $rankedFineStats) {
        $rank++
        [pscustomobject]@{
            Rank = $rank
            Parent = $stat.Parent
            Fine = $stat.Fine
            Baseline = $stat.Baseline
            Field = $stat.Field
            Property = $stat.Property
            Total = $stat.Total
        }
    }
)
$top50FineStats = @(
    $rank = 0
    foreach ($stat in ($rankedFineStats | Select-Object -First 50)) {
    $rank++
        $assessment = Get-PublicDecompositionAssessment -FineSubsystemId $stat.Baseline -PreviousPeerId $stat.PreviousPeer
        [pscustomobject]@{
            Rank = $rank
            Parent = $stat.Parent
            Fine = $stat.Fine
            Baseline = $stat.Baseline
            PreviousPeer = $stat.PreviousPeer
            Field = $stat.Field
            Property = $stat.Property
            Total = $stat.Total
            Decision = $assessment.Decision
            Boundary = "基线映射：$($stat.Baseline)；$($assessment.Boundary)"
            Evidence = $assessment.Evidence
        }
    }
)
if ($top50FineStats.Count -ne 50) {
    throw "Expected 50 ranked final fine subsystems, parsed $($top50FineStats.Count)."
}
$top50BaselineStats = @(
    $rank = 0
    foreach ($stat in ($baselineStats | Sort-Object @{ Expression = 'Total'; Descending = $true }, @{ Expression = 'Field'; Descending = $true }, @{ Expression = 'Property'; Descending = $true }, Parent, Fine | Select-Object -First 50)) {
        $rank++
        $assessment = Get-PublicDecompositionAssessment -FineSubsystemId $stat.Fine
        $peerStats = @($fineStats | Where-Object Baseline -eq $stat.Fine | Sort-Object Fine)
        [pscustomobject]@{
            Rank = $rank
            Parent = $stat.Parent
            Fine = $stat.Fine
            Field = $stat.Field
            Property = $stat.Property
            Total = $stat.Total
            PeerStats = $peerStats
            Decision = $assessment.Decision
            Boundary = $assessment.Boundary
            Evidence = $assessment.Evidence
        }
    }
)
if ($top50BaselineStats.Count -ne 50) {
    throw "Expected 50 ranked baseline subsystems, parsed $($top50BaselineStats.Count)."
}
foreach ($stat in $top50BaselineStats) {
    if ($stat.PeerStats.Count -lt 2) {
        throw "Top-50 baseline '$($stat.Fine)' has fewer than two final peer groups."
    }
}

$inputHash = (Get-FileHash -LiteralPath $inputFullPath -Algorithm SHA256).Hash.ToLowerInvariant()
$combinedSourcePath = [System.IO.Path]::GetFullPath((Join-Path (Get-Location) 'docs/migration/ledgers/Version4字段属性逐成员源码声明-去除ID类文件.md'))
$splitReportPath = [System.IO.Path]::GetFullPath((Join-Path (Get-Location) 'docs/migration/ledgers/Version4字段属性按系统子系统拆分报告-去除ID类文件.md'))
$combinedHash = (Get-FileHash -LiteralPath $combinedSourcePath -Algorithm SHA256).Hash.ToLowerInvariant()
$splitReportHash = (Get-FileHash -LiteralPath $splitReportPath -Algorithm SHA256).Hash.ToLowerInvariant()
$generationDate = (Get-Date).ToString('yyyy-MM-dd')

$builder = [System.Text.StringBuilder]::new()
[void]$builder.AppendLine('# Version4 非权威模拟系统字段和属性逐成员源码声明（更细子系统拆分，去除 ID 类文件）')
[void]$builder.AppendLine()
[void]$builder.AppendLine('## 1. 文档目的与边界')
[void]$builder.AppendLine()
[void]$builder.AppendLine('本文档是在《Version4 非权威模拟系统字段属性逐成员源码声明-去除ID类文件.md》基础上，对非权威成员库存继续按职责、声明类型、生命周期和访问模式做的多层细分导航。原文档的 11 个父级子系统仍是主归属；本文的“细分子系统”不自动修改 `Version4子系统索引.json`、`Version4源码覆盖.tsv`，也不宣布新的正式 owner。')
[void]$builder.AppendLine()
[void]$builder.AppendLine('每条字段或属性记录只进入一个父级和一个细分子系统。细分不是按行数平均切块：`Main.cs` 使用声明行段与成员语义，`SharedRuntimeMechanisms` 使用共同命名空间/能力边界，网络、持久化、外部适配和客户端表现按稳定 I/O seam 分组。')
[void]$builder.AppendLine()
[void]$builder.AppendLine('本文是源码成员库存与边界导航，不是迁移完成、行为等价、API 兼容、网络/持久化闭合或运行时可用性证明。没有修改 `src/`、Version4 参考源码、Component、System、Query、Command、Adapter、Projection 或测试实现。')
[void]$builder.AppendLine()
[void]$builder.AppendLine('## 2. 证据与拆分规则')
[void]$builder.AppendLine()
[void]$builder.AppendLine('- 成员事实直接继承输入清单；类、相对/绝对路径、行列、成员、C# 类型、完整声明和原始声明不改写。')
[void]$builder.AppendLine('- 父级系统/子系统主归属沿用输入清单；`related_subsystems` 或语义关联不复制成员、不增加计数。')
[void]$builder.AppendLine('- `state`/`runtime state` 细分预留唯一 Owner System/CommitPort；`derived/query` 细分只读；`definition/catalog` 细分只读；`adapter` 负责外部协议转换；`presentation`/`projection` 只能单向消费事实。上述是边界契约，不代表目标实现已经存在。')
[void]$builder.AppendLine('- SS14 只作为 ECS 组织参考：`C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Server\Wires\WiresSystem.cs` 展示 System 订阅事件并负责状态转换，`Content.Shared\Atmos\Components\MapAtmosphereComponent.cs` 展示组件保存内聚数据；不复制其命名或领域语义。')
[void]$builder.AppendLine('- tModLoader 公开 API 仅作边界交叉参考：`D:\TRbackup\tmodloader-api-docs-stable\index.html`，页眉版本 `tModLoader v2026.07`；它不能确认 Version4 私有字段的实际读写者、调度或持久化。')
[void]$builder.AppendLine()
[void]$builder.AppendLine('### 2.1 边界和调用方向（文字契约）')
[void]$builder.AppendLine()
[void]$builder.AppendLine('```text')
[void]$builder.AppendLine('输入/外部协议/文件/平台')
[void]$builder.AppendLine('        ↓')
[void]$builder.AppendLine('Adapter / Session / Persistence 子系统 → runtime state / definition / query inventory')
[void]$builder.AppendLine('        ↓')
[void]$builder.AppendLine('显式 Owner System / CommitPort（目标实现约束，不由字段表自动提供）')
[void]$builder.AppendLine('        ↓')
[void]$builder.AppendLine('Network / Persistence / Client presentation / Diagnostics projection（单向）')
[void]$builder.AppendLine('```')
[void]$builder.AppendLine()
[void]$builder.AppendLine('必要的运行顺序、写者、生命周期、持久化和网络闭合仍需在实现阶段建立 focused verifier；本清单的 `Role` 和 `Seam` 是替换/测试接缝，不是现有运行时证据。')
[void]$builder.AppendLine()
[void]$builder.AppendLine('### 2.2 证据记录')
[void]$builder.AppendLine()
[void]$builder.AppendLine('| 来源 | 版本 | 查询 | 命中 / 证据 | 缺口 | 停止原因 |')
[void]$builder.AppendLine('|---|---|---|---|---|---|')
[void]$builder.AppendLine('| `C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Server\Wires\WiresSystem.cs` | local checkout snapshot | `WiresSystem.Initialize`、`SubscribeLocalEvent` | 类声明第 25 行；事件订阅第 47-58 行，System 订阅事件并负责状态转换 | 只能证明组织模式，不能证明 Terraria 字段读写者 | 已取得 ECS System/Component 边界参考后停止 |')
[void]$builder.AppendLine('| `C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Shared\Atmos\Components\MapAtmosphereComponent.cs` | local checkout snapshot | `MapAtmosphereComponent`、`Mixture`、`Space`、`Overlay` | 类声明第 9 行；内聚组件数据第 15、21、23 行 | 只能证明组件保存内聚状态，不能映射 Version4 语义 | 已取得组件粒度参考后停止 |')
[void]$builder.AppendLine('| `D:\TRbackup\tmodloader-api-docs-stable\index.html`、`annotated.html` | tModLoader v2026.07 | `Main`、`Item`、`TileObjectData`、`TimeLogger`、`SceneMetrics`、`ItemSlot` | 实际类型页分别为 `class_main.html`、`class_item.html`、`class_tile_object_data.html`、`class_time_logger.html`、`class_scene_metrics.html`、`class_item_slot.html`；首页页眉确认版本 | 公开 API 文档不能确认 Version4 私有字段的实际写者、调度、持久化或网络语义 | 仅用于边界交叉核对，成员证据保留 `partial` 或 `missing` |')
[void]$builder.AppendLine('| `D:\TRbackup\Version4` 与输入成员清单 | Version4 source inventory | 逐成员声明、相对路径、声明类型和行列 | 4,542 条输入成员事实直接继承，来源序号闭合 `1..4,542` | 当前清单没有完整读者/写者、生命周期和效果顺序 | 只输出源码库存导航，不宣称运行时 ECS 迁移完成 |')
[void]$builder.AppendLine()
[void]$builder.AppendLine('## 3. 数量封存')
[void]$builder.AppendLine()
[void]$builder.AppendLine('| 指标 | 数量 |')
[void]$builder.AppendLine('|---|---:|')
[void]$builder.AppendLine('| 父级子系统（输入清单口径） | 11 |')
[void]$builder.AppendLine("| 有成员记录的父级子系统 | $(@($rows | Select-Object -ExpandProperty Parent -Unique).Count) |")
[void]$builder.AppendLine("| 原始细分基线子系统（有成员记录） | $($baselineStats.Count) |")
[void]$builder.AppendLine("| 本文最终细分子系统（有成员记录） | $($allFineIds.Count) |")
[void]$builder.AppendLine('| field | 4017 |')
[void]$builder.AppendLine('| property | 525 |')
[void]$builder.AppendLine('| 合计 | 4542 |')
[void]$builder.AppendLine()
[void]$builder.AppendLine('数量验收等式：`4,017 个字段 + 525 个属性 = 4,542 条非权威模拟系统成员记录`；所有有成员细分子系统的合计必须再次等于 4,542。')
[void]$builder.AppendLine()
[void]$builder.AppendLine('### 3.1 父级子系统统计')
[void]$builder.AppendLine()
[void]$builder.AppendLine('| 父级子系统 | 细分数 | 字段 | 属性 | 合计 |')
[void]$builder.AppendLine('|---|---:|---:|---:|---:|')
foreach ($parentId in $parentOrder) {
    $parentRows = @($rows | Where-Object Parent -eq $parentId)
    $stats = Get-Stats -Items $parentRows
    $childCount = @($fineDefinitions | Where-Object { $_.Parent -eq $parentId -and @($allFineRows | Where-Object FineSubsystem -eq $_.Id).Count -gt 0 }).Count
    [void]$builder.AppendLine("| ``$parentId`` | $childCount | $($stats.Field) | $($stats.Property) | $($stats.Total) |")
}
[void]$builder.AppendLine()
[void]$builder.AppendLine('### 3.2 细分子系统统计')
[void]$builder.AppendLine()
[void]$builder.AppendLine('| 父级子系统 | 细分子系统 | 边界角色 | 字段 | 属性 | 合计 |')
[void]$builder.AppendLine('|---|---|---|---:|---:|---:|')
foreach ($definition in $fineDefinitions) {
    $fineRows = @($allFineRows | Where-Object FineSubsystem -eq $definition.Id)
    if ($fineRows.Count -eq 0) { continue }
    $stats = Get-Stats -Items $fineRows
    [void]$builder.AppendLine("| ``$($definition.Parent)`` | ``$($definition.Id)`` | $($definition.Role) | $($stats.Field) | $($stats.Property) | $($stats.Total) |")
}
[void]$builder.AppendLine()
[void]$builder.AppendLine('### 3.3 最终细分子系统字段/属性合计排行榜前 50')
[void]$builder.AppendLine()
[void]$builder.AppendLine("排名口径：这里展示本文最终有成员的细分子系统（当前 $($allFineIds.Count) 个 peer 组）的字段/属性数量，`合计 = 字段 + 属性`，按合计降序；合计相同按字段数、属性数降序，再按父级子系统名称和最终细分子系统名称升序裁决。`Baseline` 列保留原始细分基线归属；`public-decomposition` 判定描述各级 peer 边界的源码库存导航，不等于目标 ECS 组件已经实现。全部 $($allFineIds.Count) 个最终 peer 组见 3.4；原始细分基线前 50 及其 peer 汇总见 3.5；本轮第三层拆分的即时 peer 汇总见 3.6；本轮第四层拆分的即时 peer 汇总见 3.7；本轮第五层拆分的即时 peer 汇总见 3.8；本轮第六层拆分的即时 peer 汇总见 3.9。")
[void]$builder.AppendLine()
[void]$builder.AppendLine('| 排名 | 父级子系统 | 最终细分子系统 | 基线细分子系统 | 字段 | 属性 | 合计 | public-decomposition 判定 | 同级边界 / 不拆分理由 | 证据状态 |')
[void]$builder.AppendLine('|---:|---|---|---|---:|---:|---:|---|---|---|')
foreach ($stat in $top50FineStats) {
    [void]$builder.AppendLine("| $($stat.Rank) | ``$($stat.Parent)`` | ``$($stat.Fine)`` | ``$($stat.Baseline)`` | $($stat.Field) | $($stat.Property) | $($stat.Total) | $($stat.Decision) | $($stat.Boundary) | $($stat.Evidence) |")
}
[void]$builder.AppendLine()
[void]$builder.AppendLine('3.3 是最终 peer 子系统的当前排行榜；成员已经按唯一来源序号归属到最终 peer 组。每个原始基线组的历史排名、拆分前数量和最终 peer 汇总见下表。')
[void]$builder.AppendLine()
[void]$builder.AppendLine("### 3.4 最终细分子系统字段/属性合计完整排行榜（$($allFineIds.Count)）")
[void]$builder.AppendLine()
[void]$builder.AppendLine("本表列出全部 $($allFineIds.Count) 个有成员的最终 peer 细分子系统；排序规则与 3.3 相同，成员归属和统计由逐成员明细重新聚合得到。")
[void]$builder.AppendLine()
[void]$builder.AppendLine('| 排名 | 父级子系统 | 最终细分子系统 | 基线细分子系统 | 字段 | 属性 | 合计 |')
[void]$builder.AppendLine('|---:|---|---|---|---:|---:|---:|')
foreach ($stat in $allFineRanking) {
    [void]$builder.AppendLine("| $($stat.Rank) | ``$($stat.Parent)`` | ``$($stat.Fine)`` | ``$($stat.Baseline)`` | $($stat.Field) | $($stat.Property) | $($stat.Total) |")
}
[void]$builder.AppendLine()
[void]$builder.AppendLine('### 3.5 原始细分基线前 50 与最终 peer 汇总')
[void]$builder.AppendLine()
[void]$builder.AppendLine('| 排名 | 基线细分子系统 | 最终 peer 组及统计 |')
[void]$builder.AppendLine('|---:|---|---|')
foreach ($stat in $top50BaselineStats) {
    $peerSummary = @(
        foreach ($peerStat in $stat.PeerStats) {
            "``$($peerStat.Fine)``（字段 $($peerStat.Field)；属性 $($peerStat.Property)；合计 $($peerStat.Total)）"
        }
    ) -join '；'
    [void]$builder.AppendLine("| $($stat.Rank) | ``$($stat.Fine)`` | $peerSummary |")
}
[void]$builder.AppendLine()
[void]$builder.AppendLine('### 3.6 第三层拆分前 peer 与最终 peer 汇总')
[void]$builder.AppendLine()
[void]$builder.AppendLine('本表记录五个第三层即时 peer 及其仍活动的最终后代 peer；其中若第三层子组继续进入第四层或第五层拆分，则递归展开到活动终端 peer。原始 239 组基线仍由 3.5 和逐成员章节中的“上一级基线细分子系统”保留。')
[void]$builder.AppendLine()
[void]$builder.AppendLine('| 原即时 peer | 原始基线细分子系统 | 最终 peer 组及统计 |')
[void]$builder.AppendLine('|---|---|---|')
foreach ($previousPeer in $thirdLevelBaselineIds) {
    $peerStats = @(Get-ActiveDescendantFineStats -PeerId $previousPeer)
    if ($peerStats.Count -lt 2) {
        throw "Third-level peer '$previousPeer' does not have at least two active descendants."
    }
    $baselineId = @($peerStats | Select-Object -ExpandProperty Baseline -Unique)
    if ($baselineId.Count -ne 1) {
        throw "Third-level peer '$previousPeer' does not map to exactly one original baseline."
    }
    $peerSummary = @(
        foreach ($peerStat in $peerStats) {
            "``$($peerStat.Fine)``（字段 $($peerStat.Field)；属性 $($peerStat.Property)；合计 $($peerStat.Total)）"
        }
    ) -join '；'
    [void]$builder.AppendLine("| ``$previousPeer`` | ``$($baselineId[0])`` | $peerSummary |")
}
[void]$builder.AppendLine()
[void]$builder.AppendLine('### 3.7 第四层拆分前 peer 与最终 peer 汇总')
[void]$builder.AppendLine()
[void]$builder.AppendLine('本表记录本轮从当前排行榜前 30 中退休的 30 个即时 peer 及其最终活动后代 peer；若第四层子组又进入第五层拆分，则递归展开其第五层活动子组。每个即时 peer 至少对应两个非空最终后代，原始 239 组基线仍由 3.5 和逐成员章节中的“上一级基线细分子系统”保留。')
[void]$builder.AppendLine()
[void]$builder.AppendLine('| 原即时 peer | 原始基线细分子系统 | 最终 peer 组及统计 |')
[void]$builder.AppendLine('|---|---|---|')
foreach ($previousPeer in $fourthLevelBaselineIds) {
    $peerStats = @(Get-ActiveDescendantFineStats -PeerId $previousPeer)
    if ($peerStats.Count -lt 2) {
        throw "Fourth-level peer '$previousPeer' does not have at least two active descendants."
    }
    $baselineId = @($peerStats | Select-Object -ExpandProperty Baseline -Unique)
    if ($baselineId.Count -ne 1) {
        throw "Fourth-level peer '$previousPeer' does not map to exactly one original baseline."
    }
    $peerSummary = @(
        foreach ($peerStat in $peerStats) {
            "``$($peerStat.Fine)``（字段 $($peerStat.Field)；属性 $($peerStat.Property)；合计 $($peerStat.Total)）"
        }
    ) -join '；'
    [void]$builder.AppendLine("| ``$previousPeer`` | ``$($baselineId[0])`` | $peerSummary |")
}
[void]$builder.AppendLine()
[void]$builder.AppendLine('### 3.8 第五层拆分前 peer 与最终 peer 汇总')
[void]$builder.AppendLine()
[void]$builder.AppendLine('本表记录本轮从当前排行榜前 12 中退休的 12 个即时 peer 及其最终活动后代 peer；若第五层子组又进入第六层拆分，则递归展开到活动终端 peer。每个即时 peer 至少对应两个非空最终后代。`MapEncodingHeaderCatalogState` 是嵌套在第四层 `MapEncodingCatalogAndIoState` 下的第五层 peer，仍保留完整的直接上一级映射。')
[void]$builder.AppendLine()
[void]$builder.AppendLine('| 原即时 peer | 原始基线细分子系统 | 最终 peer 组及统计 |')
[void]$builder.AppendLine('|---|---|---|')
foreach ($previousPeer in $fifthLevelBaselineIds) {
    $peerStats = @(Get-ActiveDescendantFineStats -PeerId $previousPeer)
    if ($peerStats.Count -lt 2) {
        throw "Fifth-level peer '$previousPeer' does not have at least two active descendants."
    }
    $baselineId = @($peerStats | Select-Object -ExpandProperty Baseline -Unique)
    if ($baselineId.Count -ne 1) {
        throw "Fifth-level peer '$previousPeer' does not map to exactly one original baseline."
    }
    $peerSummary = @(
        foreach ($peerStat in $peerStats) {
            "``$($peerStat.Fine)``（字段 $($peerStat.Field)；属性 $($peerStat.Property)；合计 $($peerStat.Total)）"
        }
    ) -join '；'
    [void]$builder.AppendLine("| ``$previousPeer`` | ``$($baselineId[0])`` | $peerSummary |")
}
[void]$builder.AppendLine()
[void]$builder.AppendLine('### 3.9 第六层拆分前 peer 与最终 peer 汇总')
[void]$builder.AppendLine()
[void]$builder.AppendLine('本表记录本轮从当前最终排行榜前 13 中退休的 13 个即时 peer 及其 26 个最终 peer；每个即时 peer 恰好对应两个非空子组。若即时 peer 本身是第五层子组，仍保留其原始基线和直接上一级映射。')
[void]$builder.AppendLine()
[void]$builder.AppendLine('| 原即时 peer | 原始基线细分子系统 | 最终 peer 组及统计 |')
[void]$builder.AppendLine('|---|---|---|')
foreach ($previousPeer in $sixthLevelBaselineIds) {
    $peerStats = @(Get-ActiveDescendantFineStats -PeerId $previousPeer)
    if ($peerStats.Count -ne 2) {
        throw "Sixth-level peer '$previousPeer' does not have exactly two active children."
    }
    $baselineId = @($peerStats | Select-Object -ExpandProperty Baseline -Unique)
    if ($baselineId.Count -ne 1) {
        throw "Sixth-level peer '$previousPeer' does not map to exactly one original baseline."
    }
    $peerSummary = @(
        foreach ($peerStat in $peerStats) {
            "``$($peerStat.Fine)``（字段 $($peerStat.Field)；属性 $($peerStat.Property)；合计 $($peerStat.Total)）"
        }
    ) -join '；'
    [void]$builder.AppendLine("| ``$previousPeer`` | ``$($baselineId[0])`` | $peerSummary |")
}
[void]$builder.AppendLine()
[void]$builder.AppendLine('## 4. 逐成员源码声明')
[void]$builder.AppendLine()
[void]$builder.AppendLine('以下按输入清单父级顺序展开，再按细分子系统展开。表内“来源序号”是输入清单的稳定局部序号；重新分组只改变位置，不改变成员事实。')
[void]$builder.AppendLine()

$parentIndex = 0
foreach ($parentId in $parentOrder) {
    $parentIndex++
    $parentRows = @($rows | Where-Object Parent -eq $parentId)
    $parentStats = Get-Stats -Items $parentRows
    $definitionsForParent = @($fineDefinitions | Where-Object Parent -eq $parentId)
    $activeDefinitions = @($definitionsForParent | Where-Object { @($allFineRows | Where-Object FineSubsystem -eq $_.Id).Count -gt 0 })
    [void]$builder.AppendLine("### 4.$parentIndex 父级子系统：``$parentId``")
    [void]$builder.AppendLine()
    [void]$builder.AppendLine("- 父级职责：$($parentResponsibilities[$parentId])")
    [void]$builder.AppendLine("- 父级统计：字段 $($parentStats.Field)；属性 $($parentStats.Property)；合计 $($parentStats.Total)；细分数 $($activeDefinitions.Count)。")
    if ($parentRows.Count -eq 0) {
        [void]$builder.AppendLine('- 本输入成员库存没有该父级的主归属记录；保留 0 统计，不为满足拆分数量虚构成员。')
        [void]$builder.AppendLine()
        continue
    }
    [void]$builder.AppendLine()
    [void]$builder.AppendLine('| 细分子系统 | 边界角色 | 字段 | 属性 | 合计 |')
    [void]$builder.AppendLine('|---|---|---:|---:|---:|')
    foreach ($definition in $activeDefinitions) {
        $fineRows = @($allFineRows | Where-Object FineSubsystem -eq $definition.Id)
        $stats = Get-Stats -Items $fineRows
        [void]$builder.AppendLine("| ``$($definition.Id)`` | $($definition.Role) | $($stats.Field) | $($stats.Property) | $($stats.Total) |")
    }
    [void]$builder.AppendLine()

    $childIndex = 0
    foreach ($definition in $activeDefinitions) {
        $childIndex++
        $fineRows = @($allFineRows | Where-Object FineSubsystem -eq $definition.Id | Sort-Object Seq)
        $stats = Get-Stats -Items $fineRows
        $fileCount = @($fineRows | Select-Object -ExpandProperty RelativePath -Unique).Count
        $typeCount = @($fineRows | Select-Object -ExpandProperty Type -Unique).Count
        [void]$builder.AppendLine("#### 4.$parentIndex.$childIndex 细分子系统：``$($definition.Id)``")
        [void]$builder.AppendLine()
        $baselineIds = @($fineRows | Select-Object -ExpandProperty BaseFineSubsystem -Unique)
        if ($baselineIds.Count -ne 1) {
            throw "Final fine subsystem '$($definition.Id)' does not map to exactly one baseline subsystem."
        }
        [void]$builder.AppendLine("- 上一级基线细分子系统：``$($baselineIds[0])``")
        if ($definition.PSObject.Properties.Name -contains 'PreviousPeer') {
            [void]$builder.AppendLine("- 上一级 peer 细分子系统：``$($definition.PreviousPeer)``")
        }
        [void]$builder.AppendLine("- 细分职责：$($definition.Responsibility)")
        [void]$builder.AppendLine("- 边界角色：``$($definition.Role)``；最小 seam：$($definition.Seam)；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。")
        [void]$builder.AppendLine("- 成员文件数：$fileCount；声明类型数：$typeCount；字段：$($stats.Field)；属性：$($stats.Property)；合计：$($stats.Total)。")
        [void]$builder.AppendLine('- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。')
        [void]$builder.AppendLine()
        foreach ($kind in @('field', 'property')) {
            $kindRows = @($fineRows | Where-Object Kind -eq $kind)
            $label = if ($kind -eq 'field') { '字段' } else { '属性' }
            [void]$builder.AppendLine("##### $label（$($kindRows.Count)）")
            [void]$builder.AppendLine()
            if ($kindRows.Count -eq 0) {
                [void]$builder.AppendLine('无该类型成员记录。')
                [void]$builder.AppendLine()
                continue
            }
            [void]$builder.AppendLine('| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |')
            [void]$builder.AppendLine('|---:|---|---|---|---|---:|---:|---|---|---|---|')
            foreach ($row in $kindRows) {
                [void]$builder.AppendLine('| ' + ($row.Cells -join ' | ') + ' |')
            }
            [void]$builder.AppendLine()
        }
    }
}

[void]$builder.AppendLine('## 5. 追溯与验收')
[void]$builder.AppendLine()
[void]$builder.AppendLine('| 输入 | SHA-256 |')
[void]$builder.AppendLine('|---|---|')
[void]$builder.AppendLine("| 非权威父级清单 | $inputHash |")
[void]$builder.AppendLine("| 合并字段属性源文档 | $combinedHash |")
[void]$builder.AppendLine("| 系统子系统拆分报告 | $splitReportHash |")
[void]$builder.AppendLine()
[void]$builder.AppendLine('验收要求：')
[void]$builder.AppendLine()
[void]$builder.AppendLine('- 来源序号必须恰好覆盖 `1..4,542`；不得出现重复、遗漏或新增序号。')
[void]$builder.AppendLine('- 每个细分子系统必须满足 `field + property = 合计`；父级统计必须等于其细分子系统统计；所有细分子系统合计必须等于 `4,542`。')
[void]$builder.AppendLine('- 每条成员行必须与输入清单保持一致；本文只改变分组位置和表头中的序号语义，不改变源码声明事实。')
[void]$builder.AppendLine('- 0 记录父级保留标题和统计，不凭空生成成员；跨域关联不复制记录。')
[void]$builder.AppendLine('- 统计一致性不等于迁移完成或运行时行为等价；本生成步骤不运行编译、运行时回归或 focused verifier。')
[void]$builder.AppendLine()
[void]$builder.AppendLine("生成日期：$generationDate。")

$outputDirectory = Split-Path -Parent $outputFullPath
if (-not (Test-Path -LiteralPath $outputDirectory)) {
    New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
}
[System.IO.File]::WriteAllText($outputFullPath, $builder.ToString(), [System.Text.UTF8Encoding]::new($false))

$overallStats = Get-Stats -Items $allFineRows
if ($overallStats.Field -ne 4017 -or $overallStats.Property -ne 525 -or $overallStats.Total -ne 4542) {
    throw "Generated fine totals are inconsistent: field=$($overallStats.Field), property=$($overallStats.Property), total=$($overallStats.Total)."
}

Write-Output "Generated: $outputFullPath"
Write-Output "Fine subsystems with members: $($allFineIds.Count)"
Write-Output "Totals: field=$($overallStats.Field), property=$($overallStats.Property), total=$($overallStats.Total)"
