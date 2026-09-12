# 非权威细分子系统同级拆分设计

## 目标

检查并拆解非权威成员清单中仍混合多个职责的细分子系统。保留 11 个父级子系统和
4,542 条成员事实，只调整细分导航、生成器和校验器，不修改 `src/`、Version4 参考源码或
正式 owner。

## 设计判定

拆分依据按以下顺序应用：

1. 独立的状态所有权或生命周期；
2. 稳定的源码目录、类型族或外部 I/O seam；
3. 读者只需要其中一组成员而不需要同组其他成员；
4. 已有仓库拆分报告提供的领域边界。

单一类型且生命周期一致的定义目录不因字段数量机械拆分。所有新组仍是源码库存导航，
不等于目标 ECS 组件已经实现，也不宣称读者、写者、调度、网络或持久化已经闭合。

## 拆分范围

`RuntimeComposition` 的 `Main.cs` 成员按平台/帧状态、启动和世界规则、相机与 UI、
线程和输入、背景/地图、世界几何与天气、Tile 元数据、笼具动画、实体池、玩家/商店、
内容服务、存档会话、入侵/NPC、网络/菜单和环境循环拆分。

`SharedRuntimeMechanisms` 的高密度聚合按源码能力拆分为物品实例、配方制作、堆叠转移、
商店、装备变体、地牢生成状态/查询/定义、天气与环境、粒子和 Gore、事件/入侵/对话、
Tile 放置/锚点/框架、Entity 与 WorldItem、Tile 着色/快照、战斗/物理、网络 Socket/包/模块、
本地化/聊天、输入绑定/控制、UI 控件/屏幕/货币、掉落规则/模拟、创意解锁、图鉴/个性、
工具与随机源、持久化/资源/文件系统、内容表现/校验等同级组。

客户端物品 UI 按物品槽、排序、工具提示分开。对仍缺少独立访问模式证据的内聚组保留
单一导航边界，不按字段数量机械切碎。

## 第二轮高密度组复核

继续逐类型、逐源码目录复核后，以下边界有独立生命周期、稳定类型族或稳定 I/O seam，已登记为同级细分：

| 原细分组 | 同级拆分依据 | 结果 |
|---|---|---|
| `Item.cs` 的 157 条成员 | 既有 `Item.cs` 证据已确认定义、堆叠、前缀、武器/工具、装备、资源、商店、Buff/坐骑和表现是不同变更原因；本轮按静态目录、身份/堆叠、装备/表现、进度/交互、使用/战斗、商店、效果和派生查询拆分 | `SharedItemStaticCatalogRules`、`SharedItemIdentityAndStackState`、`SharedItemEquipmentAndPresentationState`、`SharedItemProgressionAndWorldInteractionState`、`SharedItemUseToolAndCombatState`、`SharedItemCommerceState`、`SharedItemBuffMountAndConsumableEffects`、`SharedItemDerivedQueries` |
| `SharedDungeonLayoutDefinitions` 的 248 条成员 | `Dungeon/Rooms`、`Halls`、`Entrances`、`Features`、`LayoutProviders` 是稳定源码目录和 provider seam；根目录材料/边界/门/平台定义另保留 | 根布局、房间、大厅、入口、特征、布局 provider 六组 |
| `TimeLogger.cs` 的 111 条成员 | `DataSeries`、`TimeLogData`、`FormatPool` 与外层帧协调器是不同嵌套类型和访问生命周期 | 数据序列、条目、格式缓存、协调器四组 |
| `SceneMetrics`、扫描设置和 `SceneState` | 扫描请求、扫描快照和视觉过渡分别由不同类型表达；`SceneState.Update` 单向消费指标 | `SharedSceneScanSettings`、`SharedSceneMetricsSnapshot`、`SharedSceneVisualState` |
| 音频、钓鱼、天气、创意能力、物品槽和世界事件 | 分别存在播放引擎/定义/活动声音，规则/尝试/交付，粒子/天空风/闪电/瀑布，能力定义/管理器/UI，Context/转移/脉冲，以及入侵/季节事件/表现类型族 | 各按类型或稳定路径拆为同级组；成员事实不复制 |

以下高密度组经复核后刻意保留，不以字段数量继续机械切分：

| 保留组 | 不拆分理由 |
|---|---|
| `SharedTimeLoggerCoordinatorState` | 外层 `TimeLogger` 的条目注册、帧边界、日志缓冲、渲染计时和格式池引用共同由同一静态协调器驱动；已将其嵌套数据类型拆出，继续按字段切段会破坏注册/帧不变量。 |
| `SharedSceneMetricsSnapshot` | `SceneMetrics.Reset`、`Scan`、聚合和区域计算共同维护同一个带失效条件的派生快照；扫描输入和视觉状态已分离，剩余成员不能仅按字段名分割。 |
| `TileObjectDefinitionCatalog` | `TileObjectData` 是内容加载后只读的定义门面，锚点、尺寸、放置 hook 和 alternates 通过同一对象的继承/写时复制规则保持一致；其 `Terraria.Modules` 实现已在其他细分组导航。 |
| `SharedFishingDropRuleDefinitions` | 鱼获规则、条件、稀有度和 populator 共用启动期目录/规则契约；尝试输入和交付效果已经分离。完整 resolver 仍标记为 `partial`。 |
| `SharedDropRuleDefinitions` | 各规则类型通过同一个 `IItemDropRule`/chain/resolver 合同交互；现有成员库存没有足够独立 owner 或生命周期证据，继续按规则类切分会过度原子化。 |
| `UiItemSorting` | 排序层定义、白名单、缓存和一次排序批次共同构成一个 UI 排序事务；没有独立写者或持久生命周期。 |
| `MainCageAnimationState`、`SharedWorldGenerationBiomeSupport` | 分别是 `Main` 的单一笼具帧注册表和同一生成/生态定义查询域；目前没有额外稳定 owner seam。 |

## 本轮前 50 组实际同级拆分

前 50 个高字段细分组中，本轮对 23 个已经存在稳定同级边界的旧组执行实际重分组；其余
18 个仍只有候选证据的组和 9 个已经确认应保持单一 owner/lifecycle 的组暂不继续拆分。
下面的映射是生成器的唯一归属契约，每条成员只进入一个新组：

| 原细分组 | 本轮新边界 |
|---|---|
| `SharedDungeonRootLayoutDefinitions` | `SharedDungeonStyleCatalog`、`SharedDungeonLayoutProviderState`、`SharedDungeonGeometryAndPlacementDefinitions` |
| `AudioPlaybackEngineState` | `AudioLegacySoundAdapterState`、`AudioPlaybackCoordinatorState`、`AudioTrackedSoundState` |
| `UiLayoutAndInteractionTree` | `UiLayoutPrimitives`、`UiElementTreeState`、`UiEventPayloads`、`UiInputPointerState` |
| `NetworkSessionAndRemotePeers` | `NetworkSessionCoordinatorState`、`NetworkRemoteClientState`、`NetworkRemoteServerState`、`NetworkRemoteIpRequestAdapter` |
| `SharedDungeonGenerationState` | `SharedDungeonCrawlerRuntimeState`、`SharedDungeonGenerationDataState`、`SharedDungeonLegacyGenerationGlobals` |
| `MapAndWorldMapPresentation` | `MapTileStorageAndUpdateState`、`MapOverlayAndLayerPresentation`、`WorldMapState` |
| `SharedBestiaryCatalog` | `BestiaryCatalogAndEntries`、`BestiaryUnlockTracking`、`BestiaryInfoElementsAndProviders`、`BestiaryFiltersAndSorting` |
| `SharedItemTransferAndStacking` | `ItemEmergencyStackingState`、`ItemQuickStackingState`、`ItemTransferSettings` |
| `SharedLocalizationCatalog` | `LocalizationCultureAndLanguageState`、`LocalizedTextValueState`、`LegacyLanguageCatalogState` |
| `SharedTileEntitiesAndWorldObjects` | `TileEntityRegistryAndBaseState`、`TileEntityDisplayAndInventoryState`、`TileEntityAnchorAndSensorState`、`TileEntityWorldInteractionState` |
| `SharedLightingAndLightMaps` | `LightingEngineState`、`LightMapCacheState`、`TileLightScannerState` |
| `SharedPlayerIntentAndMovementData` | `PlayerIntentAndMovementState`、`PlayerItemPickupAndRespawnState`、`PlayerPreviewAndRejectionState` |
| `SharedDrawAndAnimationPresentation` | `DrawAnimationAndFrameState`、`DrawCommandAndBatchState`、`WorldDrawingAuxiliaryState` |
| `SharedGeneralUtilityFunctions` | `SharedGeneralPureUtilities`、`SharedGeneralFilePlatformUtilities`、`SharedGeneralDiagnosticsUtilities`、`SharedGeneralDelegateAndMetadataUtilities` |
| `SharedTestingAndDebugTools` | `DebugCommandProtocol`、`DebugRuntimeOptions`、`DebugFrameTelemetry`、`DebugBuildStatus` |
| `SharedItemVariantsAndPrefixes` | `ItemVariantDefinitions`、`ItemTagEffectState`、`LegacyItemPrefixCatalog` |
| `SharedMinecartAndMotion` | `MinecartMotionState`、`MinecartCustomizationState`、`TrackedProjectileReferenceState` |
| `SharedDirectWorldInteractionHelpers` | `DoorOpeningInteractionState`、`SmartCursorInteractionState`、`PressurePlateInteractionState`、`CursorAndChestInteractionState` |
| `SharedRecipeAndCraftingRules` | `RecipeDefinitionCatalog`、`CraftingRequestState` |
| `SharedWorldEnvironmentAndSpawnRules` | `WorldSpawnConfigurationState`、`WorldSeedAndExploitRules`、`EnvironmentDamageAndSeatState`、`WorldEnvironmentScanHelpers` |
| `SharedInputBindings` | `InputProfilesAndConfiguration`、`InputTriggerState`、`PlayerInputRuntimeState`、`LockOnTargetingState` |
| `SharedEntityBaseState` | `EntityAuthoritativeState`、`EntityShadowPresentationState` |
| `SharedEquipmentAndArmorDefinitions` | `ArmorSetBonusDefinitions`、`ArmorSetBonusCatalog`、`WingStatsDefinition`、`EquipmentLoadoutState` |

边界选择优先使用源码路径和声明类型族；同一类型内部只有在已有成员语义列表能够稳定区分
变更原因时才拆分。`SharedGeneralUtilityFunctions` 特别先匹配文件/平台、诊断和元数据路径，
再处理 `Terraria.Utilities/*` 的剩余纯工具，防止宽匹配吞掉新边界。TileEntity 按 registry/base、
展示/库存、锚点/传感器和世界交互四个类型族分组，不按每个 TileEntity 类型单独建组。

## 第二轮前 50 基线再拆

第二轮固定上一轮生成的前 50 个细分子系统作为基线，不用二次拆分后的新组重新排名。
这 50 个基线组全部退休为源码库存标识，并各自映射到至少两个最终 peer 组；最终报告的
3.3 节保留基线字段/属性排行榜，3.4 节列出每个基线的 peer 统计，逐成员小节记录上一级
基线标识。成员仍只归属一个父级、一个最终细分组和一个基线组，不复制记录。

边界继续按 public-decomposition 的所有权和访问模式规则选择：源码路径/声明类型族优先，
单类型混合组使用成员语义集合；例如 `TileObjectData` 分成继承/alternates 与放置/样式，
`TimeLogger` 分成帧协调与渲染指标，`WorldFile` 分成恢复版本与事件临时值，`ItemSlot`
分成高亮脉冲与显示交互。每个最终组登记 Role、最小 seam 和责任说明，但缺少完整读者、
写者、生命周期、调度、网络和持久化证据的部分仍标记为 `partial`，不等于已实现 ECS 组件。

第二轮的确定性映射由生成器中的 `Get-BaseFineSubsystemId` 和
`Get-SecondLevelFineSubsystemId` 共同定义；完整校验器按最终组反向聚合基线统计，focused
verifier 检查 50 个旧基线退休、100 个新 peer 边界存在和每个基线至少两个 peer。

## 数据流和兼容性

生成器从输入成员表按确定性路径/类型/`Main.cs` 声明行映射到唯一细分组。输出成员的
来源序号、类型、路径、行列、C# 类型和声明文本完全继承输入表。没有复制记录，也不改变
父级主归属。

本轮生成结果为 239 个有成员细分子系统，较 185 个基线增加 54 个；这是 `23` 个旧组替换
为 `77` 个新边界的净变化：`185 - 23 + 77 = 239`。成员总数封存不变：`4,017` 字段、
`525` 属性、合计 `4,542`。

第二轮在这 239 个基线组上退休前 50 并加入 100 个 peer，最终得到 `289` 个有成员细分子系统：
`239 - 50 + 100 = 289`。字段、属性和成员总量仍保持 `4,017`、`525`、`4,542`。

## 验收

- 输入和输出仍覆盖 `1..4,542`，字段/属性分别为 `4,017/525`。
- 每个细分组和父级的字段、属性、合计与逐成员明细一致。
- 摘要统计表和明细统计由校验器交叉核对。
- ID 类文件仍为零，三条 SHA-256 追溯记录仍存在。
- focused verifier 检查第一轮 77 个边界、第二轮 100 个 peer 边界存在，23 个第一轮旧组和
  50 个前 50 基线组退休，且活动细分总数为 289。
- 只运行 PowerShell 生成器、focused verifier 和完整校验器；本轮不运行编译或运行时回归。
- 读写者、生命周期、调度、网络和持久化证据仍未闭合；因此不能把本轮结果表述为运行时 ECS
  组件迁移完成。
