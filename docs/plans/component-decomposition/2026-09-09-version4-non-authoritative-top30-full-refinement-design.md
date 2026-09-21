# Version4 Non-Authoritative Top-30 Full Refinement Design

## Goal

对当前非权威细分排行榜前 30 个活动 peer 各执行至少一次职责拆分。每个目标 peer
退役并替换为两个有成员的最终 peer，因此本轮新增 30 次拆分、60 个新 peer；成员事实、
父级归属和原始 239 组基线保持不变。

## Scope

这是源码库存导航和拆分边界更新，不修改 `src/`、`Test/`、Version4 参考源码或运行时
ECS 实现。生成器增加下一层 peer 映射，报告保留原始基线、已有即时 peer，并新增本轮
30 个退休 peer 到 60 个最终 peer 的追溯表。

预期数量：

- 当前活动 peer：294；本轮退休：30；本轮新增：60；最终活动 peer：324。
- 成员事实：4,017 字段、525 属性、4,542 条记录不变。
- 原始基线：239 组不变；原有五个第三层退休 peer 和十个第三层子组不变。

## Evidence And Boundary Decisions

成员路径、声明类型和成员名来自当前生成报告及 `D:\TRbackup\Version4` 的只读源码库存。
这些边界确认的是声明所有权和职责内聚度；读者/写者、生命周期、持久化、网络和调度仍
标记为 partial，不能被本报告误读为运行时 ECS 已完成。

| 当前 peer | 新 peer A / B | 拆分证据与边界 |
|---|---|---|
| `TileObjectDefinitionPlacementAndStyleState` | `TileObjectPlacementRuleState` / `TileObjectStyleAndDrawState` | `TileObjectData` 的锚点、液体、放置 hook 与 style/draw/coordinate 成员族。 |
| `SharedTimeLoggerRenderMetricsState` | `SharedTimeLoggerPhaseMetricsState` / `SharedTimeLoggerDisplayFormattingState` | `TimeLogger.cs` 的阶段计时字段与末尾 CPU/百分比/毫秒格式缓存。 |
| `SharedDropRuleSelectionAndChainState` | `SharedDropRuleSelectionAndConditionState` / `SharedDropRuleChainState` | ItemDropRules 的选择/条件字段与 `ChainedRules`、`Chains.cs` 链接字段。 |
| `SharedBiomeDungeonAndTerrainState` | `SharedBiomeTerrainPassState` / `SharedDungeonControlAndTrapState` | `DungeonControlLine.cs`、`DeadMansChestBiome.cs` 与其他 Biomes/TerrainPass 文件边界。 |
| `SharedFishingDropCatalogState` | `SharedFishingConditionCatalogState` / `SharedFishingDropResolutionState` | populator/rarity/condition 目录与 `FishDropRule`、可能性条目、规则列表。 |
| `MainCageBirdAndTerrestrialAnimationState` | `MainCageBirdAnimationState` / `MainCageTerrestrialCritterAnimationState` | `Main.cs` 中鸟类 cage 字段与其他陆生/昆虫/小动物帧字段族。 |
| `UiItemSortingLayerCatalogState` | `UiItemSortingCombatAndEquipmentCatalogState` / `UiItemSortingConsumableAndMiscCatalogState` | `ItemSortingLayers` 的武器、工具、装备层与药剂、杂项、末尾层。 |
| `SharedSceneScanAndZoneState` | `SharedSceneZoneDefinitionState` / `SharedSceneScanAccumulatorState` | `SceneMetrics` 的区域常量与扫描结果、计数缓存、位置快照。 |
| `SharedCreativePowerDefinitionState` | `SharedCreativePerPlayerPowerState` / `SharedCreativeSharedPowerState` | CreativePowers 的 per-player 类型族与 shared power 类型族。 |
| `SharedDungeonStyleEntryDefinitions` | `SharedDungeonStyleMaterialAndGeometryState` / `SharedDungeonStyleFurnitureAndRoomState` | `DungeonGenerationStyleData` 的砖/墙/液体/几何成员与家具、箱体、房间子样式成员。 |
| `SharedSceneAggregateAndDecorationState` | `SharedSceneTileAggregateState` / `SharedSceneDecorationAndAudioState` | `SceneMetrics` 的 Tile/液体/实体计数与音乐、蜡烛、纪念碑、装饰标志。 |
| `AudioLegacySoundInstanceState` | `SharedAudioLegacySoundCatalogInstanceState` / `SharedAudioLegacyTrackedInstanceState` | `LegacySoundPlayer` 的声音实例字段与可跟踪实例集合/回收状态。 |
| `SharedDungeonRoomShapeVariants` | `SharedDungeonRoomGeometryState` / `SharedDungeonRoomVariantCatalogState` | 各 DungeonRoom 类型的几何/位置数据与 `VARIANT_*`、`BIOMEROOM_*` 常量族。 |
| `AudioLegacySoundCatalogState` | `SharedAudioLegacySoundDefinitionCatalogState` / `SharedAudioLegacySoundServicesState` | `Sound*` 旧音效定义与 `TrackableSounds`、`_services` 外部服务边界。 |
| `MapEncodingCatalogAndIoState` | `MapEncodingHeaderCatalogState` / `MapIoRuntimeState` | `MapHelper` 的 header/选项常量与 I/O 锁、场景缓存、zlib 状态。 |
| `MainTileBehaviorMetadata` | `MainTileBehaviorAndInteractionMetadata` / `MainTileLightingAndFrameMetadata` | `Main` 的交互/碰撞/放置标志与光照、框架、砖墙表现元数据。 |
| `UiItemSlotEquipmentAndCreativeContexts` | `UiItemSlotEquipmentAndDisplayContexts` / `UiItemSlotCreativeCraftingAndUtilityContexts` | `ItemSlot.Context` 的装备/展示架族与创意、制作、快捷/调试上下文。 |
| `SharedItemStaticEconomyAndTimingRules` | `SharedItemEconomyAndValueRules` / `SharedItemUseTimingAndStackRules` | `Item` 的货币/价格常量与拾取、药剂时序、食物、堆叠和替换规则。 |
| `SharedPopupTextState` | `SharedPopupTextContentAndContextState` / `SharedPopupTextRenderLifecycleState` | `PopupText` 的文本/金币/声纳上下文与位置、透明度、生命周期、渲染缓存。 |
| `MainBackgroundLayerState` | `MainBackgroundLayerCatalogState` / `MainBackgroundParallaxAndStyleState` | `Main` 的背景图层/场景集合与偏移、风格、视差状态。 |
| `SharedShaderFamilyCatalogState` | `SharedShaderFamilyDataState` / `SharedShaderRegistryAndLookupState` | Hair/Misc/Armor shader 数据类型与 `GameShaders`、`*ShaderDataSet` 注册索引。 |
| `SharedDungeonStyleConstantQueries` | `SharedDungeonStyleObjectConstants` / `SharedDungeonStyleBannerAndTrapConstants` | `DungeonUtils` 的门、花盆、吊灯、平台对象常量与旗帜/陷阱常量。 |
| `SharedTilePlacementGeometryModules` | `SharedTilePlacementCoordinateAndDrawModules` / `SharedTilePlacementBaseAndStyleModules` | `TileObjectCoordinatesModule`/`DrawModule` 与 `BaseModule`/`StyleModule` 类型边界。 |
| `SharedWorldItemPayloadState` | `SharedWorldItemIdentityAndEconomyPayloadState` / `SharedWorldItemUseAndPresentationPayloadState` | `WorldItem` 的物品身份、堆叠、价值投影与使用、伤害、表现字段。 |
| `NetworkRemoteClientState` | `NetworkRemoteClientConnectionAndStatusState` / `NetworkRemoteClientSectionAndRateLimitState` | `RemoteClient` 的连接/状态字段与区段、读缓冲、反垃圾限制字段。 |
| `NetworkSessionCoordinatorState` | `NetworkSessionConfigurationState` / `NetworkSessionTransportAndThreadState` | `Netplay` 的服务器策略/端点配置与客户端、监听器、线程、广播运行态。 |
| `ItemEmergencyStackingState` | `SharedItemEmergencyStackingPolicyState` / `SharedItemEmergencyStackingTransferState` | `EmergencyStacking` 策略配置与 `Group`/`StackableItem`/`Transfer` 传输运行态。 |
| `BestiaryInfoElementsAndProviders` | `SharedBestiaryInfoElementState` / `SharedBestiaryCollectionProviderState` | `*InfoElement` 展示事实与 UICollection provider、集合信息、接口契约。 |
| `EntityAuthoritativeState` | `EntityIdentityAndMotionState` / `EntityBoundsAndFluidState` | `Entity` 的身份/运动权威字段与尺寸、液体和几何投影属性。 |
| `SharedCreativePowerIconCatalogState` | `SharedCreativePowerIconLocationCatalogState` / `SharedCreativePowerIconLayoutState` | `CreativePowersHelper` 的图标位置枚举与网格尺寸/选中颜色布局元数据。 |

## Data Flow And Lineage

```text
input member inventory
        ↓
239 original baseline groups
        ↓
existing second/third-level peers
        ↓
30 current top-30 peers retired → 60 new peers
        ↓
ranking + summary + per-member projection
```

新 peer 的 `Baseline` 仍来自原始 239 组；新 peer 的 `PreviousPeer` 指向本轮退休的
30 个当前 peer。成员不复制、不删除，只改变最终分组位置。

## Non-Splits And Risks

本轮不改变前 30 之外的活动组，也不把字段范围切成无语义的单字段组件。所有 30 个
边界目前属于 `source-inventory-confirmed`，但读写者/生命周期仍是 partial；后续真正实现
ECS 时必须为 state、query、adapter、projection 建立明确 owner 和 focused verifier。

仓库声明的 `约束/公共拆分约束.md` 当前缺失，已记录为文档风险；本设计以技能正文、ECS
证据协议、现有报告约束和 Version4 只读源码为当前有效依据。

## Verification

- focused verifier：断言 30 个退休 peer、60 个新 peer、每个目标恰有两个非空子组和最终活动数 324。
- full verifier：断言成员序号 `1..4,542`、字段/属性总数、父级和原始基线汇总、完整排行榜、所有 lineage 与 ID 类文件排除。
- independent audit：重新聚合 60 个新 peer 的统计，并检查每个新边界的路径/类型/成员规则没有漏项。
- `git diff --check`：检查生成脚本、计划和报告。
