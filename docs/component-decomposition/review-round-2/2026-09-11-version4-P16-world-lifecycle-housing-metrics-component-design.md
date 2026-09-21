# Version4 P16 世界生命周期、住房、指标与环境缓存组件设计

partitionId: P16
sessionId: e9e33a27331a40d3a159985a54bbf8eb
inputReport: D:\TRbackup\NLTX\docs\migration\ledgers\authoritative-20-partitions\P16-World-Lifecycle-Housing-Metrics.md
designPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\2026-09-11-version4-P16-world-lifecycle-housing-metrics-component-design.md
executionPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\2026-09-11-version4-P16-world-lifecycle-housing-metrics-component-execution.md
executionStatus: failed
implementationStatus: partial
verificationStatus: partially-verified
evidenceStatus: partial
designCompletedComponents: [C01, C02, C03, C04, C05, C06, C07, C08, C09, C10, C11, C12, C13]
completedComponents: [C03-WorldLoadLifecycleComponent, C03-WorldDimensionCompatibilityState, C03-WorldTransformTransactionComponent, C04-WorldProgressionEventStateComponent, C05-WorldHousingBuildDiagnosticState, C05-WorldNpcSpawnPacingState, C05-WorldDropSuppressionContext, C05-WorldObjectDestructionContext, C06-WorldTileMergePassContext, C06-WorldTileReframeCounter, C07-HousingScanBudgetDefinition, C07-HousingScanVisitedTilesContext, C07-HousingMaterialCounterSnapshot, C08-HousingRoomSearchContext, C08-HousingRoomTileClassificationContext, C08-HousingRoomRequirementResult, C08-HousingRoomDiagnosticFlags, C09-HousingWaterRuleDefinition, C09-CactusWaterEligibilityQuery, C09-WorldEvilRuleDefinition, C10-WorldSizeCatalogDefinition, C10-WorldDimensionSelectionQuery, C10-WorldSpreadBoundaryDefinition, C10-WorldMeteorGeometryQuery, C11-TrapGenerationDiagnosticScratch, C11-GemSelectionPassScratch, C11-MossPalettePassScratch, C11-WorldGenerationRandomPort, C12-WorldTerrainTileOverrideScope, C12-WorldHardmodeTilePolicyState, C12-WorldGrassSpreadState, C12-CrimsonHeartPlacementScratch, C12-WorldTerrainRuleDefinition, C12-WorldFossilShatterScope, C12-WorldBoulderRainState, C12-WorldStormSafeSpotScratch, C12-WorldTreeTopsStateComponent, C12-WorldBackgroundFlashCacheProjection, C12-WorldTerrainDestructionQueueComponent, C12-WorldTerrainCoatingStateComponent, C13-WorldOceanLevelQuery]
currentComponent: C12-WorldTerrainEffectsAndCaches
pendingComponents: [C03-WorldFileRecoveryAdapter, C03-WorldClearCoordinator, C03-TransformingWorldQueryIntegration, C04-WorldSpawnIntentQueue, C04-WorldProgressionPersistenceProjection, C04-WorldProgressionNetworkProjection, C04-WorldEventEffectAdapter, C05-WorldInfectionPolicyOwnerIntegration, C05-WorldNpcSpawnEffectAdapter, C05-WorldHousingDiagnosticProjection, C06-TileMergeQuery, C06-WorldTileFrameCommitPort, C08-WorldHousingRoomSearchState, C09-MysticLogsEventAdapter, C12-WorldTerrainEffectsAndCaches, C13-WorldGenerationRandomSourceAdapter, C01-WorldGenBiomeBackgroundAndDistanceMetrics, C02-WorldGenTileCountMetrics]
lastCheckpointUtc: 2026-09-12T12:30:07.9622729Z
evidence-gap: C03 的三个状态组件已隔离落地，但 WorldFile recovery、clear coordinator、volatile guard bridge、TransformingWorld query、取消/调度/IOLock 和旧尺寸清理调用图仍未闭合。C04 的 shadow-orb/altar 跨 tick 事实组件已落地，但 spawn intent、WorldFile/network projection、effect acknowledgment、事件并发和完整 round-trip 仍未闭合。C05 的住房诊断、NPC 节拍、掉落抑制和物体销毁 guard 已落地；感染策略发现已有近义 `WorldInfectionPolicyStateComponent`，但 owner 复用、UpdateWorld phase 和 NPC/effect adapter 仍需核对。C07 的扫描预算、访问上下文、材质计数快照、房间访问上下文、评分快照和优先级状态已以隔离组件落地；C08 的 room search context、房间 tile 分类上下文、不可变 requirement result 和诊断 flags 也已以隔离组件落地，但 C07/C08 的完整房间搜索与 flood-fill、RoomNeeds 资格查询、评分、房间失败原因与 diagnostics projection、alternate-spot、住房 assignment、NPC/TownManager key 映射、NPC/Town/network/progression effects 和运行时 parity 仍未闭合。C09 仙人掌规则/query、C10 尺寸 catalog/meteor geometry、C11 scratch/random port、C12 地形状态/缓存和 C13 ocean-level query 的既有 evidence gaps 继续有效；本次受影响的 WorldSession 串行构建已通过，但 P16 focused verification 仍未执行。
blocking-decision: C03 新组件只保存加载/清理事实、上一轮尺寸 footprint 和嵌套变换计数；不得在组件内实现 WorldFile、清理协调、跨线程发布、IOLock、取消或主线程 follow-up。C04 新组件只保存 shadow-orb/altar committed facts；不复制已有 Calendar pending-event 状态，不实现 spawn consumer、WorldFile/NetMessage projection 或 effect acknowledgment。C05 新组件只保存诊断、节拍和局部 scope；已有 `WorldInfectionPolicyStateComponent` 不在本分区重复创建，感染策略 owner 复用和 UpdateWorld 接线继续 pending。C07 新增住房扫描/房间评分组件只保存显式输入、短生命周期上下文和不可变结果；不得在组件内实现扫描、评分、资格查询、feedback、NPC/TownManager 关系、网络或持久化副作用。C08 新增的 room search context、房间 tile 分类上下文、不可变 requirement result 和诊断 flags 只保存一次尝试的递归/候选状态、去重分类状态、资格结果与两个 diagnostic bits；不得在组件内实现 flood-fill、RoomNeeds 查询、失败原因 projection、alternate-spot effect、住房 assignment 或 NPC/Town/network/progression effects。C09 WorldEvilRuleDefinition、C10 catalog/meteor geometry、C11 scratch/random port、C12 terrain state/cache 与 C13 ocean query 均保持无 legacy writer、无持久化/网络副作用；不得因局部实现或 focused verifier 证据替代完整运行时 parity 验证。所有外部 adapter、System、Query、Command、Projection、测试和 runtime parity 继续 pending；P16 总体 verificationStatus 保持 not-run。

## 1. 结论与范围

状态覆盖说明：文件前部 `evidence-gap` 与 `blocking-decision` 保留了早期计划阶段的历史措辞；以顶部 metadata 和第 10 节 Terminal Checkpoint 为当前状态来源。当前 P16 已完成局部隔离验证，但未完成运行时迁移、存档/网络兼容或行为等价验证。

P16 覆盖 Version4 `Terraria.WorldGen` 声明序列 2417..2659 中的 130 个字段和 3 个属性，共 133 个成员。权威报告将其分为 13 个叶子子系统；本设计保留这些来源分区作为审计键，但不把 13 组机械合并成一个 `World` 巨型组件。

最终建议按访问模式形成以下边界：

- 生命周期状态：加载/生成、世界变换、进度事件、住房生成诊断、刷怪节奏、感染扩散策略和 tile merge 各自有明确 owner。
- 住房状态：扫描预算/计数、房间搜索上下文、房间评分和住房规则/诊断分开；扫描结果通过显式命令写入 NPC 住房关系。
- 指标与派生：背景/距离、tile count、感染阈值、`TransformingWorld`、随机源和海平面是 Query、Snapshot、Projection 或 Definition，不反向成为权威状态。
- 地形效果与缓存：硬模式/草扩散/破坏队列等权威效果状态，与 `TreeTops`、背景闪烁、位集和 pass scratch 分开。
- `TownManager`、`Manifest`、`mysticLogsEvent`、NPC home fields、持久化和网络对象均通过 Adapter/Projection 或 `crossSubsystemOwner: integration-review` 交接。

本文件的设计主体记录提议边界；C09 仙人掌水体规则/query、C10 尺寸 catalog/selection/boundary
definition 和 C13 ocean-level query 的实际实现状态只在第 9 节 implementation checkpoint
中记录。除这些隔离单元外，本轮没有修改 `Test/`、`dome/`、Version4、权威报告或 ledger。

## 2. 证据等级和来源

| 来源 | 只读位置 | 用途 | 状态和限制 |
|---|---|---|---|
| Version4 `WorldGen` | `D:\TRbackup\Version4\Terraria\WorldGen.cs:4075-4372` | 133 个声明、可见性、原始类型和局部初始化 | `confirmed`；完整调用图仍为 `partial` |
| Version4 世界文件 | `D:\TRbackup\Version4\Terraria.IO\WorldFile.cs`，相关加载/保存段 | `crimson`、进度字段、`TreeTops`、`Manifest`、`TownManager`、加载失败和变换等待 | `partial`；尚未证明全部字段的 round-trip 格式 |
| Version4 主循环/网络 | `D:\TRbackup\Version4\Terraria\Main.cs`、`NetMessage.cs` | `QuickFindHome` 调用、SceneMetrics 调度、`tGood/tEvil/tBlood` 和住房状态网络投影 | `partial`；完整 client/server 权限仍待 focused verifier |
| Version4 场景指标 | `D:\TRbackup\Version4\Terraria\SceneMetrics.cs` | 实例级扫描、`Main.GameUpdateCount` 重建和缓存失效线索 | `confirmed` 为派生扫描模式；不证明 WorldGen 字段所有权 |
| Version4 TownRoomManager | `D:\TRbackup\Version4\Terraria\GameContent\TownRoomManager.cs` | NPC type key、房间关系、Save/Load/Clear 边界 | `confirmed` 为类型级事实；与 NLTX persistent ID key 的集成方式 `partial` |
| tModLoader API 镜像 | `D:\TRbackup\tmodloader-api-docs-stable\index.html`，`v2026.07` | `WorldGen`、`WorldFile`、`SceneMetrics`、`ModSystem`、`ModBiome` 的公开 API 交叉参考 | `partial`；不能替代 Version4 私有字段和运行时语义 |
| SS14 只读参考 | `C:\Users\shan\Downloads\ECS\space-station-14-master` | `BiomeComponent/System`、`GridPreloaderComponent/System`、`TileHistoryComponent/System` 的粒度和读写边界 | 仅支持组织模式，不复制命名或领域语义 |
| 当前 NLTX | `D:\TRbackup\NLTX\src\WorldSession\WorldGeneration`、`src\Town`、`dome\src`、`dome\Test` | 现有 proposed/partial 类型、生成 pass 顺序和住房数据结构 | `partial`；未把现有类型视为已接线实现 |

### 2.1 证据门槛结论

- 源报告已闭合成员库存、声明类型、来源路径、行号、列号、原始声明和叶子归属。
- `RoomNeeds` 与 `QuickFindHome` 的源码证据表明住房扫描会清理和写入搜索状态，并最终影响 `Main.npc[npc].homeTileX/homeTileY/homeless/homelessDespawn`；因此住房搜索不能建模成纯字段 Query。
- `WorldGen.StartHardmode` 使用后台任务和 `_transformingWorld`，`WorldFile` 保存前等待 `TransformingWorld`；世界变换是独立事务边界。
- `SceneMetrics` 按扫描中心和 `Main.GameUpdateCount` 重建实例级缓存；它是 Query/Projection/cache 参考，不是世界权威根。
- `WorldFile` 和 `NetMessage` 只证明部分持久化/网络范围，不能据此给所有成员添加 Save/Sync。
- `docs/flowstate/README.md` 与 `约束/公共拆分约束.md` 缺失，已记录为 evidence gap；不会以文件不存在推断额外架构规则。

## 3. 叶子分区与提议边界

| Code | 权威报告叶子子系统 | 成员 | 主要设计边界 | 候选类型 | 边界状态 |
|---|---|---:|---|---|---|
| C01 | `WorldGenBiomeBackgroundAndDistanceMetrics` | 21 | 背景/距离 Query、世界邪恶选择和外部世界注册表交接 | Query/Definition/Adapter | `proposed` |
| C02 | `WorldGenTileCountMetrics` | 14 | tile count snapshot、感染阈值网络 Projection、生成指标 Query | Snapshot/Projection/Query | `proposed` |
| C03 | `WorldLifecycleLoadAndTransformState` | 7 | load lifecycle 与 transform transaction 分开 | Component/System/Adapter | `proposed` |
| C04 | `WorldLifecycleProgressionAndEventState` | 6 | progression/event intent 与持久化 Projection | Component/System/Projection | `proposed` |
| C05 | `WorldLifecycleHousingAndSpawnPacingState` | 7 | 建房诊断、感染策略、刷怪节奏 | Component/System/Query | `proposed` |
| C06 | `WorldLifecycleTileMergeState` | 4 | 单次 tile merge 操作状态 | PassContext/System | `proposed` |
| C07 | `WorldHousingCountersAndScoringState` | 15 | 扫描预算、材质计数和评分 | Component/Query/Context | `proposed` |
| C08 | `WorldHousingRoomSearchState` | 19 | 房间扫描上下文、资格结果和 NPC 关系提交 | Context/Query/Command/Adapter | `proposed` |
| C09 | `WorldHousingRuleAndDiagnosticState` | 5 | 住房规则定义、仙人掌水体资格和事件 Adapter | Definition/Query/Adapter | `proposed` |
| C10 | `WorldGenerationDimensionsState` | 8 | 尺寸/边界 Definition，流星计数交接 | Definition/Query/Projection | `proposed` |
| C11 | `WorldGenerationScratchState` | 4 | 生成 pass 短生命周期 scratch | PassContext | `proposed` |
| C12 | `WorldTerrainEffectsAndCaches` | 20 | 地形效果、队列、TreeTops 持久化状态、缓存和 scratch 分离 | Component/Projection/Context/Definition | `proposed` |
| C13 | `WorldGenDerivedProperties` | 3 | 变换、随机源和海平面纯派生边界 | Query/Adapter | `proposed` |

## 4. Domain-first 文件组织和依赖方向

未进入第 9 节 implementation checkpoint 的新增路径只在迁移获批且证据闭合后创建；这些路径保持
`status: proposed`。已创建的隔离实现文件及其实际状态仅以第 9 节为准。

| 能力 | proposed 路径 | 命名空间 | 说明 |
|---|---|---|---|
| 生命周期 | `src/WorldSession/WorldGeneration/Lifecycle/` | `Terraria.WorldGeneration.Lifecycle` | C03-C06 的 owner state/system；稳定边界达到规模后再建目录 |
| 住房 | `src/WorldSession/WorldGeneration/Housing/` | `Terraria.WorldGeneration.Housing` | C07-C09 的扫描、规则、关系提交 |
| 指标 | `src/WorldSession/WorldGeneration/Metrics/` | `Terraria.WorldGeneration.Metrics` | C01-C02 的 Query/Snapshot/Projection |
| 尺寸与生成 pass | `src/WorldSession/WorldGeneration/Passes/` | `Terraria.WorldGeneration.Passes` | C10-C11 的 Definition/Context |
| 地形效果 | `src/WorldSession/WorldGeneration/Terrain/` | `Terraria.WorldGeneration.Terrain` | C12 的效果 owner、缓存和适配器 |
| 外部边界 | `src/WorldSession/WorldGeneration/Adapters/` | `Terraria.WorldGeneration.Adapters` | WorldFile、NetMessage、TownRoomManager、XNA/Version4 类型转换 |

不创建 `Shared/Components/`、`Common/` 或以文件顺序表达调度。每个提议 public type 使用同名 PascalCase 文件；常量/规则不会因为共享而放入泛型目录。

依赖方向：

```text
WorldFile/NetMessage/Main/Version4 callbacks
        -> Adapter or validated Command
        -> owner System
        -> authoritative Component or short-lived PassContext
        -> pure Query / immutable Snapshot
        -> Projection (save/network/diagnostic/presentation)
```

Query、Projection 和 Definition 不写回权威状态。需要先后关系的路径使用显式 scheduler contract 或 command/event，而不是目录顺序。

## 5. 完整 133 成员归属表

表中的 `source group` 是权威报告分区；`proposed owner` 是迁移后的候选 owner，不表示已经创建或接线。`evidence` 只表示目前证据等级。

### C01 `WorldGenBiomeBackgroundAndDistanceMetrics` (21)

| 序号 | 成员 | C# 类型 | 源行 | proposed owner | 状态 |
|---:|---|---|---:|---|---|
| 2417 | `TownManager` | `TownRoomManager` | 4075 | `TownHousingRegistryAdapter`; `crossSubsystemOwner: integration-review` | partial |
| 2418 | `Manifest` | `WorldManifest` | 4077 | `WorldManifestPersistenceAdapter`; `crossSubsystemOwner: integration-review` | partial |
| 2419 | `tileReframeCount` | `int` | 4079 | `WorldTerrainFrameEffectState` | partial |
| 2420 | `treeBG1` | `int` | 4081 | `WorldBackgroundStyleDefinition` | partial |
| 2421 | `treeBG2` | `int` | 4083 | `WorldBackgroundStyleDefinition` | partial |
| 2422 | `treeBG3` | `int` | 4085 | `WorldBackgroundStyleDefinition` | partial |
| 2423 | `treeBG4` | `int` | 4087 | `WorldBackgroundStyleDefinition` | partial |
| 2424 | `corruptBG` | `int` | 4089 | `WorldBackgroundStyleDefinition` | partial |
| 2425 | `jungleBG` | `int` | 4091 | `WorldBackgroundStyleDefinition` | partial |
| 2426 | `snowBG` | `int` | 4093 | `WorldBackgroundStyleDefinition` | partial |
| 2427 | `hallowBG` | `int` | 4095 | `WorldBackgroundStyleDefinition` | partial |
| 2428 | `crimsonBG` | `int` | 4097 | `WorldBackgroundStyleDefinition` | partial |
| 2429 | `desertBG` | `int` | 4099 | `WorldBackgroundStyleDefinition` | partial |
| 2430 | `oceanBG` | `int` | 4101 | `WorldBackgroundStyleDefinition` | partial |
| 2431 | `mushroomBG` | `int` | 4103 | `WorldBackgroundStyleDefinition` | partial |
| 2432 | `underworldBG` | `int` | 4105 | `WorldBackgroundStyleDefinition` | partial |
| 2433 | `oceanDistance` | `int` | 4107 | `WorldBiomeDistanceDefinition` | partial |
| 2434 | `beachDistance` | `int` | 4109 | `WorldBiomeDistanceDefinition` | partial |
| 2435 | `shimmerSafetyDistance` | `int` | 4111 | `WorldBiomeDistanceDefinition` | partial |
| 2436 | `crimson` | `bool` | 4113 | `WorldEvilSelectionState`; persistence owner requires integration review | partial |
| 2437 | `generatingRandomEvil` | `bool` | 4115 | `WorldEvilSelectionPassContext` | partial |

### C02 `WorldGenTileCountMetrics` (14)

| 序号 | 成员 | C# 类型 | 源行 | proposed owner | 状态 |
|---:|---|---|---:|---|---|
| 2438 | `tileCounts` | `int[]` | 4117 | `WorldTileCountSnapshot` | partial |
| 2439 | `totalEvil` | `int` | 4119 | `WorldTileCountSnapshot` | partial |
| 2440 | `totalBlood` | `int` | 4121 | `WorldTileCountSnapshot` | partial |
| 2441 | `totalGood` | `int` | 4123 | `WorldTileCountSnapshot` | partial |
| 2442 | `totalSolid` | `int` | 4125 | `WorldTileCountSnapshot` | partial |
| 2443 | `totalEvil2` | `int` | 4127 | `WorldTileCountSnapshot` | partial |
| 2444 | `totalBlood2` | `int` | 4129 | `WorldTileCountSnapshot` | partial |
| 2445 | `totalGood2` | `int` | 4131 | `WorldTileCountSnapshot` | partial |
| 2446 | `totalSolid2` | `int` | 4133 | `WorldTileCountSnapshot` | partial |
| 2447 | `tEvil` | `byte` | 4135 | `WorldInfectionThresholdNetworkProjection` | partial/network-confirmed |
| 2448 | `tBlood` | `byte` | 4137 | `WorldInfectionThresholdNetworkProjection` | partial/network-confirmed |
| 2449 | `tGood` | `byte` | 4139 | `WorldInfectionThresholdNetworkProjection` | partial/network-confirmed |
| 2450 | `totalX` | `int` | 4141 | `WorldTerrainMetricSnapshot` | partial |
| 2451 | `totalD` | `int` | 4143 | `WorldTerrainMetricSnapshot` | partial |

### C03 `WorldLifecycleLoadAndTransformState` (7)

| 序号 | 成员 | C# 类型 | 源行 | proposed owner | 状态 |
|---:|---|---|---:|---|---|
| 2452 | `_transformingWorld` | `int` | 4145 | `WorldTransformTransactionComponent` | partial |
| 2455 | `isGeneratingOrLoadingWorld` | `bool` | 4151 | `WorldLoadLifecycleComponent` | partial |
| 2462 | `loadFailed` | `bool` | 4165 | `WorldLoadLifecycleComponent` | partial |
| 2463 | `worldCleared` | `bool` | 4167 | `WorldLoadLifecycleComponent` | partial |
| 2464 | `worldBackup` | `bool` | 4169 | `WorldLoadLifecycleComponent` | partial |
| 2465 | `lastMaxTilesX` | `int` | 4171 | `WorldDimensionCompatibilityState` | partial |
| 2466 | `lastMaxTilesY` | `int` | 4173 | `WorldDimensionCompatibilityState` | partial |

### C04 `WorldLifecycleProgressionAndEventState` (6)

| 序号 | 成员 | C# 类型 | 源行 | proposed owner | 状态 |
|---:|---|---|---:|---|---|
| 2453 | `spawnEye` | `bool` | 4147 | `WorldProgressionEventStateComponent` | partial |
| 2454 | `spawnHardBoss` | `int` | 4149 | `WorldProgressionEventStateComponent` | partial |
| 2456 | `shadowOrbSmashed` | `bool` | 4153 | `WorldProgressionEventStateComponent` + WorldFile adapter | partial/persistence-confirmed |
| 2457 | `shadowOrbCount` | `int` | 4155 | `WorldProgressionEventStateComponent` + WorldFile adapter | partial/persistence-confirmed |
| 2458 | `altarCount` | `int` | 4157 | `WorldProgressionEventStateComponent` + WorldFile adapter | partial/persistence-confirmed |
| 2461 | `spawnMeteor` | `bool` | 4163 | `WorldProgressionEventStateComponent` + WorldFile adapter | partial/persistence-confirmed |

### C05 `WorldLifecycleHousingAndSpawnPacingState` (7)

| 序号 | 成员 | C# 类型 | 源行 | proposed owner | 状态 |
|---:|---|---|---:|---|---|
| 2459 | `builtHouseWithNoFurniture` | `bool` | 4159 | `WorldHousingBuildDiagnosticState` | partial |
| 2460 | `builtHouseWithNoLight` | `bool` | 4161 | `WorldHousingBuildDiagnosticState` | partial |
| 2471 | `stopDrops` | `bool` | 4183 | `WorldDropAndSpawnPolicyState` | partial |
| 2472 | `AllowedToSpreadInfections` | `bool` | 4185 | `WorldInfectionSpreadPolicyState` | partial |
| 2473 | `destroyObject` | `bool` | 4187 | `WorldTerrainDestructionIntentState` | partial |
| 2474 | `npcSpawnDelay` | `int` | 4189 | `WorldNpcSpawnPacingState` | partial |
| 2475 | `npcSpawnPeriod` | `int` | 4191 | `WorldNpcSpawnPacingState` | partial |

### C06 `WorldLifecycleTileMergeState` (4)

| 序号 | 成员 | C# 类型 | 源行 | proposed owner | 状态 |
|---:|---|---|---:|---|---|
| 2467 | `mergeUp` | `bool` | 4175 | `WorldTileMergePassContext` | partial |
| 2468 | `mergeDown` | `bool` | 4177 | `WorldTileMergePassContext` | partial |
| 2469 | `mergeLeft` | `bool` | 4179 | `WorldTileMergePassContext` | partial |
| 2470 | `mergeRight` | `bool` | 4181 | `WorldTileMergePassContext` | partial |

### C07 `WorldHousingCountersAndScoringState` (15)

| 序号 | 成员 | C# 类型 | 源行 | proposed owner | 状态 |
|---:|---|---|---:|---|---|
| 2476 | `prioritizedTownNPCType` | `int` | 4193 | `HousingAssignmentPriorityState` | partial |
| 2477 | `numTileCount` | `int` | 4195 | `HousingScanBudgetContext` | partial |
| 2478 | `maxTileCount` | `int` | 4197 | `HousingScanBudgetDefinition` | partial |
| 2479 | `maxWallOut2` | `int` | 4199 | `HousingScanBudgetDefinition` | partial |
| 2480 | `CountedTiles` | `Dictionary<Point, bool>` | 4201 | `HousingScanVisitedTilesContext` | partial |
| 2481 | `lavaCount` | `int` | 4203 | `HousingMaterialCounterSnapshot` | partial |
| 2482 | `iceCount` | `int` | 4205 | `HousingMaterialCounterSnapshot` | partial |
| 2483 | `sandCount` | `int` | 4207 | `HousingMaterialCounterSnapshot` | partial |
| 2484 | `rockCount` | `int` | 4209 | `HousingMaterialCounterSnapshot` | partial |
| 2485 | `shroomCount` | `int` | 4211 | `HousingMaterialCounterSnapshot` | partial |
| 2486 | `maxRoomTiles` | `int` | 4213 | `HousingRoomScoreDefinition` | partial |
| 2487 | `maxRoomSize` | `int` | 4215 | `HousingRoomScoreDefinition` | partial |
| 2488 | `roomTiles` | `BitSet2D` | 4217 | `HousingRoomVisitedTilesContext` | partial |
| 2489 | `numRoomTiles` | `int` | 4219 | `HousingRoomScoreSnapshot` | partial |
| 2498 | `hiScore` | `int` | 4237 | `HousingRoomScoreSnapshot` | partial |

### C08 `WorldHousingRoomSearchState` (19)

| 序号 | 成员 | C# 类型 | 源行 | proposed owner | 状态 |
|---:|---|---|---:|---|---|
| 2490 | `roomX1` | `int` | 4221 | `HousingRoomSearchContext` | partial |
| 2491 | `roomX2` | `int` | 4223 | `HousingRoomSearchContext` | partial |
| 2492 | `roomY1` | `int` | 4225 | `HousingRoomSearchContext` | partial |
| 2493 | `roomY2` | `int` | 4227 | `HousingRoomSearchContext` | partial |
| 2494 | `canSpawn` | `bool` | 4229 | `HousingEligibilityQueryResult` | partial |
| 2495 | `houseTile` | `bool[]` | 4231 | `HousingRoomTileClassificationContext` | partial |
| 2496 | `bestX` | `int` | 4233 | `HousingRoomCandidateSnapshot` | partial |
| 2497 | `bestY` | `int` | 4235 | `HousingRoomCandidateSnapshot` | partial |
| 2499 | `roomTorch` | `bool` | 4239 | `HousingRoomRequirementResult` | partial |
| 2500 | `roomDoor` | `bool` | 4241 | `HousingRoomRequirementResult` | partial |
| 2501 | `roomChair` | `bool` | 4243 | `HousingRoomRequirementResult` | partial |
| 2502 | `roomTable` | `bool` | 4245 | `HousingRoomRequirementResult` | partial |
| 2503 | `roomHasStinkbug` | `bool` | 4247 | `HousingRoomDiagnosticFlags` | partial |
| 2504 | `roomHasEchoStinkbug` | `bool` | 4249 | `HousingRoomDiagnosticFlags` | partial |
| 2510 | `LastFoundHouse` | `Point` | 4261 | `HousingRoomSearchContext` | partial |
| 2511 | `currentlyTryingToUseAlternateHousingSpot` | `bool` | 4263 | `HousingRoomSearchContext` | partial |
| 2512 | `sharedRoomX` | `int` | 4265 | `HousingRoomSearchContext` | partial |
| 2513 | `_roomCheckStack` | `Stack<Point>` | 4267 | `HousingRoomSearchContext` | partial |
| 2514 | `roomCheckFailureReason` | `TownNPCRoomCheckFailureReason` | 4269 | `HousingRoomDiagnosticProjection` | partial |

住房扫描的最小 seam 不是 `HousingScanStateComponent` 的任意可变字段，而是 `HousingRoomSnapshot` 输入、`HousingEligibilityResult` 输出和 `HousingAssignmentCommand` 提交。`RoomNeeds` 可以纯计算资格，但 `QuickFindHome` 必须把 NPC home 关系写入交给 owner system/adapter。现有 NLTX 的 `TownHousingResidentKey(int NpcType)` 与 `TownHousingAssignmentComponent(PersistentEntityId)` 不得在本轮自行裁决，保留 `crossSubsystemOwner: integration-review`。

### C09 `WorldHousingRuleAndDiagnosticState` (5)

| 序号 | 成员 | C# 类型 | 源行 | proposed owner | 状态 |
|---:|---|---|---:|---|---|
| 2505 | `WorldGenParam_Evil` | `int` | 4251 | `WorldEvilRuleDefinition` / validated command | partial |
| 2506 | `cactusWaterWidth` | `int` | 4253 | `HousingWaterRuleDefinition` | partial |
| 2507 | `cactusWaterHeight` | `int` | 4255 | `HousingWaterRuleDefinition` | partial |
| 2508 | `cactusWaterLimit` | `int` | 4257 | `HousingWaterRuleDefinition` | partial |
| 2509 | `mysticLogsEvent` | `MysticLogFairiesEvent` | 4259 | `MysticLogsEventAdapter`; `crossSubsystemOwner: integration-review` | partial |

### C10 `WorldGenerationDimensionsState` (8)

| 序号 | 成员 | C# 类型 | 源行 | proposed owner | 状态 |
|---:|---|---|---:|---|---|
| 2515 | `meteorShowerCount` | `int` | 4271 | `WorldProgressionEventStateComponent` / meteor adapter | partial |
| 2516 | `WorldSizeSmallX` | `int` | 4273 | `WorldSizeCatalogDefinition` | confirmed declaration |
| 2517 | `WorldSizeSmallY` | `int` | 4275 | `WorldSizeCatalogDefinition` | confirmed declaration |
| 2518 | `WorldSizeMediumX` | `int` | 4277 | `WorldSizeCatalogDefinition` | confirmed declaration |
| 2519 | `WorldSizeMediumY` | `int` | 4279 | `WorldSizeCatalogDefinition` | confirmed declaration |
| 2520 | `WorldSizeLargeX` | `int` | 4281 | `WorldSizeCatalogDefinition` | confirmed declaration |
| 2521 | `WorldSizeLargeY` | `int` | 4283 | `WorldSizeCatalogDefinition` | confirmed declaration |
| 2522 | `InfectionAndGrassSpreadOuterWorldBuffer` | `int` | 4285 | `WorldSpreadBoundaryDefinition` | confirmed declaration |

### C11 `WorldGenerationScratchState` (4)

| 序号 | 成员 | C# 类型 | 源行 | proposed owner | 状态 |
|---:|---|---|---:|---|---|
| 2525 | `trapDiag` | `int[,]` | 4292 | `WorldGenerationScratchContext` | partial |
| 2526 | `gem` | `bool[]` | 4294 | `WorldGenerationScratchContext` | partial |
| 2527 | `mossType` | `int[]` | 4296 | `WorldGenerationScratchContext` | partial |
| 2528 | `neonMossType` | `ushort` | 4298 | `WorldGenerationScratchContext` | partial |

### C12 `WorldTerrainEffectsAndCaches` (20)

| 序号 | 成员 | C# 类型 | 源行 | proposed owner | 状态 |
|---:|---|---|---:|---|---|
| 2543 | `tileSolidBackup` | `bool[]` | 4328 | `WorldTerrainPassScratchContext` | partial |
| 2544 | `ItemSpawnProtectionTime` | `int` | 4330 | `WorldItemSpawnProtectionDefinition` | confirmed declaration |
| 2545 | `_coatingColors` | `List<Color>` | 4332 | `WorldTerrainCoatingState` + XNA adapter | partial |
| 2546 | `catTailDistance` | `int` | 4334 | `WorldTerrainDecorationDefinition` | partial |
| 2547 | `TreeTops` | `TreeTopsInfo` | 4336 | `WorldTreeTopsStateComponent` + save/network adapters | partial/persistence/network-confirmed |
| 2548 | `BackgroundsCache` | `BackgroundChangeFlashInfo` | 4338 | `WorldBackgroundFlashCacheProjection` | partial/cache |
| 2549 | `fossilBreak` | `bool` | 4340 | `WorldTerrainEffectStateComponent` | partial |
| 2550 | `ExploitDestroyQueue` | `Queue<Point>` | 4342 | `WorldTerrainDestructionQueueComponent` | partial |
| 2551 | `hardModeWorldUpdates` | `bool` | 4344 | `WorldTransformEffectState` | partial |
| 2552 | `growGrassUnderground` | `bool` | 4346 | `WorldGrassSpreadEffectState` | partial |
| 2553 | `_isRainingBoulders` | `bool` | 4348 | `WorldTerrainEventEffectState` | partial |
| 2554 | `_SpawnThunderStorm_SafeSpots` | `List<Rectangle>` | 4350 | `WorldTerrainEventEffectState` + geometry adapter | partial |
| 2555 | `BUBBLES_SOLID_STATE_FOR_HOUSING` | `bool` | 4352 | `HousingTileSolidityDefinition` | confirmed declaration |
| 2556 | `grassSpread` | `int` | 4354 | `WorldGrassSpreadEffectState` | partial |
| 2557 | `heartPos` | `Point[]` | 4356 | `WorldTerrainEventEffectState` | partial |
| 2558 | `heartCount` | `int` | 4358 | `WorldTerrainEventEffectState` | partial |
| 2559 | `strip_w` | `int` | 4360 | `WorldTerrainPassDefinition` | confirmed declaration |
| 2560 | `strip_h` | `int` | 4362 | `WorldTerrainPassDefinition` | confirmed declaration |
| 2561 | `bitStrip` | `Vertical64BitStrips` | 4364 | `WorldTerrainPassScratchContext` | partial |
| 2562 | `_preventInfiniteRopeFraming` | `bool` | 4366 | `WorldTerrainFrameEffectState` | partial |

### C13 `WorldGenDerivedProperties` (3)

| 序号 | 成员 | C# 类型 | 源行 | proposed owner | 状态 |
|---:|---|---|---:|---|---|
| 2657 | `TransformingWorld` | `bool` | 4368 | `WorldTransformTransactionQuery` | confirmed getter; ownership partial |
| 2658 | `genRand` | `UnifiedRandom` | 4370 | `WorldGenerationRandomSourceAdapter` | confirmed getter; injection partial |
| 2659 | `oceanLevel` | `double` | 4372 | `WorldOceanLevelQuery` | confirmed getter; input ownership partial |

## 6. C01 设计检查点：背景、距离、世界注册表交接

### 6.1 Owner 与分类

- `treeBG1..underworldBG`、距离常量和 `generatingRandomEvil` 不能被包装为一个可变 `WorldGenMetricsComponent`。背景样式/距离是 Definition 或 Query 输入；随机邪恶是生成 pass context。
- `crimson` 由 `WorldFile` 持久化证据提升为权威世界邪恶选择候选，但其最终 owner 与 C04/C09 的 progression/rule 边界仍需 `integration-review`。
- `TownManager` 的实际类型是 `TownRoomManager`，以 NPC type 管理住房关系，不能放在背景 Query；由 Town housing adapter 接管其读写和 Save/Load 交接。
- `Manifest` 属于世界元数据/持久化边界，不由指标 Query 写入。
- `tileReframeCount` 归地形 frame effect，作为跨 C01/C12 的交接成员而非背景值。

### 6.2 读写、生命周期和副作用

| 边界 | 输入 | 输出 | 写入副作用 | 生命周期 |
|---|---|---|---|---|
| `WorldBackgroundStyleQuery` | 世界尺寸、Biome/evil 事实、背景 Definition | 不可变背景样式结果 | 无；不写 `WorldGen` | 计算时创建，快照消费后丢弃 |
| `WorldBiomeDistanceQuery` | world bounds、spawn/biome 坐标、距离 Definition | ocean/beach/shimmer 资格 | 无 | 纯计算 |
| `WorldEvilSelectionPass` | seed、validated `WorldGenParam_Evil`、随机源 | `WorldEvilSelectionCommand` | 唯一 owner 写 `crimson`/selection state | world create/load；失败回滚 |
| `TownHousingRegistryAdapter` | TownRoomManager、NPC housing command | registry snapshot / assignment event | 可能调用 `SetRoom`/`KickOut`/`Save`/`Load` | load -> runtime -> save |
| `WorldManifestPersistenceAdapter` | Manifest serialization boundary | load/save projection | 外部 file I/O only | world load/save |

副作用隔离要求：随机数由 adapter/port 注入，文件 I/O 由 WorldFile adapter 承担，日志和诊断只接收不可变 projection。Query 不访问 `Main`、不修改 Tile/NPC/TownManager。

### 6.3 兼容和 verifier

先保留旧 public 名称的 compatibility adapter，禁止 dual-write。focused verifier 必须覆盖：

1. 背景/距离 Query 对相同输入重复计算相同结果且不改变任何 component。
2. `crimson` 选择在 seed、显式 evil 参数和失败回滚下保持原始结果。
3. `TownManager` 的 key、`SetRoom`、`KickOut`、Save/Load 顺序与住房关系投影一致。
4. `Manifest` 和背景 Query 之间无反向写入。
5. tile reframe 命令只能由地形 frame owner 提交，不能由指标 Query 隐式触发。

当前只完成设计证据整理；上述 verifier 未执行，`verificationStatus: not-run`。

## 6.4 C02 设计检查点：Tile count snapshot 与感染阈值投影

### 6.4.1 Owner 与分类

- `tileCounts` 是扫描期间按 tile type 索引的可变累加数组；它的 `Array.Clear` 和填充边界表明它更适合作为 `WorldTileCountPassContext`，而不是长寿命 ECS authority component。
- `totalEvil2`、`totalBlood2`、`totalGood2`、`totalSolid2` 是扫描累加器；扫描结束后复制到 `totalEvil`、`totalBlood`、`totalGood`、`totalSolid`。两组字段不能在未确认读取窗口前合并为一个无版本 snapshot。
- `totalX`、`totalD` 是扫描游标/节拍状态；它们属于计数 pass scheduler context，不属于纯 Query 输出。
- `tEvil`、`tBlood`、`tGood` 由总计数比例推导并以 byte 通过 `NetMessage` 发送。提议 `WorldInfectionThresholdNetworkProjection` 只读 snapshot，由 server owner 产生，client 不得反向写入总计数。
- `SceneMetrics._tileCounts` 是另一个实例级数组，按扫描中心和 `Main.GameUpdateCount` 失效；它不是 `WorldGen.tileCounts` 的存储替代物，也不能被 C02 component 共享引用。

### 6.4.2 读写、生命周期和副作用

| 边界 | 输入 | 输出 | 唯一写入者 | 生命周期/失效 |
|---|---|---|---|---|
| `WorldTileCountPassContext` | tile grid、tile category catalog、扫描范围 | `WorldTileCountAccumulator` | world-count system | world creation/load scan；成功提交或失败清零 |
| `WorldTileCountSnapshot` | 完成的 accumulator | immutable totals + source revision | snapshot builder | 一个扫描 revision；不直接持久化，除非格式另行确认 |
| `WorldInfectionThresholdQuery` | totals、solid denominator、clamp policy | three byte threshold values | pure query | 每次 snapshot revision；相同输入必须确定性 |
| `WorldInfectionThresholdNetworkProjection` | server threshold snapshot | packet payload `tGood/tEvil/tBlood` | network adapter | send-on-revision or established packet cadence |
| `WorldSceneMetricQuery` | scan center、visual area、 NPC positions | camera/player-local metrics | SceneMetrics adapter | invalidated by tick or center; no WorldGen write-back |

副作用顺序必须显式：扫描累加 -> close accumulator -> build immutable snapshot -> derive thresholds -> network projection。不能在网络写出期间继续改变同一 snapshot，也不能让 `SceneMetrics` 读写 `WorldGen.tileCounts`。

### 6.4.3 兼容和 verifier

focused verifier 需要覆盖：

1. 空世界、全固体、只有 evil/blood/good tile、denominator 为零和极端计数的比例/clamp 结果。
2. `total*2` 在扫描中累加、扫描结束后复制到 `total*`，失败或重置时不泄漏上一世界数据。
3. `tileCounts` 的数组长度保持 `TileID.Count`，重复扫描不会累加旧数组；snapshot 不暴露可变数组引用。
4. `tGood/tEvil/tBlood` 的 byte 顺序、发送权限、重复 revision 和非法 client 回写拒绝。
5. `SceneMetrics` 的 tick/center 失效不会改变世界级计数，世界级 snapshot 也不会改变局部 SceneMetrics。

当前证据足以完成设计边界，但不足以实现网络/持久化迁移；verifier 未执行，`verificationStatus: not-run`。

## 6.5 C03 设计检查点：加载生命周期、世界变换与尺寸兼容

### 6.5.1 Owner 与分类

- `_transformingWorld` 是可嵌套的活动变换计数，不是普通生命周期 bool。提议 `WorldTransformTransactionComponent` 保存活动 revision/计数，`WorldTransformTransactionSystem` 负责 acquire/release；`TransformingWorld` 只是 Query。
- `isGeneratingOrLoadingWorld` 是跨线程读取的 guard。提议 `WorldLoadLifecycleComponent` 保存 `Loading`/`Generating`/`Ready`/`Failed`/`Clearing` 事实，但旧 volatile bool 在 adapter 移除前必须保持语义兼容。
- `loadFailed` 是失败结果，`worldBackup` 是 server load recovery 决策，不能把 backup 是否存在当作世界权威数据；文件存在性和复制/删除只能由 `WorldFileRecoveryAdapter` 负责。
- `worldCleared` 是清理完成事件/标记；清理动作会重置 TownManager、事件、NPC、tile、liquid 等多个外部 owner，不能由单个 Component 直接拥有。
- `lastMaxTilesX`/`lastMaxTilesY` 记录上一轮 tile allocation footprint，并被房间边界检查和世界清理使用；它们是 compatibility/cleanup state，不替代实际 world bounds component。

### 6.5.2 读写、生命周期和副作用

| 边界 | 输入 | 输出 | 唯一 owner/副作用 | 生命周期 |
|---|---|---|---|---|
| `WorldLoadLifecycleComponent` | load/generate command、file result、cancel/error | lifecycle snapshot | lifecycle system；通过 volatile-compatible adapter 发布 guard | create -> load/generate -> ready/failed -> clear |
| `WorldTransformTransactionComponent` | transform command、task completion | active count/revision、busy Query | transform system；task scheduler、IOLock、main-thread queue 为 ports | acquire -> background work -> follow-up -> release |
| `WorldDimensionCompatibilityState` | current dimensions、previous allocation footprint | cleanup bounds/room edge snapshot | load/clear system | retained until next clear; never persisted without evidence |
| `WorldFileRecoveryAdapter` | WorldFile exception/status、`.bak` existence | load failure/recovery command | file copy/delete and error reporting only | one load attempt/retry chain |
| `WorldClearCoordinator` | clear command | clear-completed event | explicit commands to tile/NPC/Town/liquid/map owners | clear start -> all owners reset -> completed |

`WorldFile.LoadWorld` 在解析成功后临时设置 loading guard，执行 `Liquid.QuickWater`、`WaterCheck` 和 quick settle，再恢复 false；异常路径设置 `loadFailed`。`CreateNewWorld` 在后台 task 前设置生成 guard。`TransformWorldOnBackgroundThread` 用 `Interlocked.Increment/Decrement`，在 `WorldFile.IOLock` 内运行变换，并把 main-thread follow-up 排队。保存必须等待 transform 计数归零。上述顺序必须在 System contract 中显式表示，不能由字段或文件顺序推导。

### 6.5.3 兼容和 verifier

focused verifier 需要覆盖：

1. `Loading`/`Generating` guard 下禁止普通 world update、liquid/network side effect 的行为保持；成功后只发布一次 ready。
2. 两个并发 transform 的计数 acquire/release、异常 finally、取消和 main-thread follow-up；计数不能提前归零或变为负数。
3. Save 在 transform active 时等待，transform 完成后继续；IOLock 不能被重复 adapter 持有造成死锁。
4. 首次 load 失败、backup 存在/不存在、backup 重试成功/失败的状态转换和 `loadFailed/worldBackup` 映射。
5. 清理大世界后用旧 dimensions 释放旧 tile，再用当前 dimensions 初始化新 tile；房间 edge check 读取的兼容 bounds 与清理记录一致。

未确认的取消传播、线程池调度和 `Main` 状态桥接在实现前必须补证；verifier 未执行，`verificationStatus: not-run`。

## 6.6 C04 设计检查点：世界进度事实与事件意图

### 6.6.1 Owner 与分类

- `shadowOrbSmashed`、`shadowOrbCount`、`altarCount` 是跨 tick、跨保存的世界进度事实候选；WorldFile 已确认它们的二进制顺序和 `bool/byte/int` 宽度，保存/加载只能由 persistence adapter 完成。
- `spawnEye`、`spawnHardBoss` 和 `spawnMeteor` 是由夜间主循环或战斗/世界逻辑消费的事件意图。`spawnEye` 和 `spawnHardBoss` 在成功消费或白天路径清零，`spawnMeteor` 在流星处理后清零；不应让网络/存档直接把它们当成永久进度。
- `spawnHardBoss` 的值 1/2/3 映射不同机械 Boss 组合，0 表示无待处理请求；它必须是受约束的 command payload，而不是无界 int。
- `shadowOrbSmashed` 和 `shadowOrbCount` 在击碎暗影珠时共同更新，计数达到阈值还会触发生成/事件逻辑；必须用一个 progression owner 保持不变量，禁止两个组件分别写入。
- `altarCount` 被硬模式事件和旧矿物层级兼容逻辑读取；其迁移不能仅由事件系统决定，需与 C10/C04 的 WorldFile adapter 交接。

### 6.6.2 读写、生命周期和副作用

| 边界 | 输入 | 输出 | 唯一 owner/副作用 | 生命周期 |
|---|---|---|---|---|
| `WorldProgressionEventStateComponent` | validated progression commands、load snapshot | committed progress facts | progression system；只写世界事件事实 | world load/create -> runtime -> clear |
| `WorldSpawnIntentQueue` | time/combat/world event facts | bounded spawn intent | event system；NPC spawn/meteor adapter 消费后 ack/clear | one night/event window |
| `WorldProgressionPersistenceProjection` | committed facts | legacy WorldFile field sequence | WorldFile adapter only | save/load; version-gated |
| `WorldProgressionNetworkProjection` | server progress snapshot | bits/packet fields where evidence exists | NetMessage adapter only | send-on-change/established sync cadence |
| `WorldEventEffectAdapter` | accepted intent | NPC/boss/meteor spawn, chat, achievement | external effects, then command ack | effect transaction; failure keeps/rejects intent per policy |

副作用顺序必须可见：验证事件命令 -> commit progression -> publish intent -> execute server-only effect -> ack/consume -> emit network/persistence projections。保存可以读取一个闭合 snapshot，但不能在事件 owner 正在修改时序列化半个状态。事件处理失败时不能静默丢失意图；保留/拒绝/重试语义需要明确端口。

### 6.6.3 兼容和 verifier

focused verifier 需要覆盖：

1. `shadowOrbSmashed`/`shadowOrbCount` 的首次击碎、重复击碎、阈值触发、计数归零和 clear reset 不变量。
2. `spawnHardBoss` 的 0/1/2/3 合法值、未知值拒绝、成功消费清零、白天清零和重复 command 幂等性。
3. `spawnEye` 和 `spawnMeteor` 的产生条件、消费窗口、无 surface 清除、重复 tick 不重复生成。
4. WorldFile legacy/current 版本的字段顺序和宽度 round-trip，尤其 `shadowOrbCount` 的 byte 截断风险和 `altarCount` 的 int 保真。
5. 事件 effect adapter 的 server-only 权限、失败重试/保留、网络/存档 projection 不反向写 owner。

当前证据足以描述边界，但不足以批准实现；完整事件权限和并发快照仍是 evidence gap，verifier 未执行，`verificationStatus: not-run`。

## 6.7 C05 设计检查点：住房生成诊断、感染策略与刷怪节奏

### 6.7.1 Owner 与分类

- `builtHouseWithNoFurniture` 与 `builtHouseWithNoLight` 是建房/生成诊断结果，不是住房关系本身；提议由 `WorldHousingBuildDiagnosticState` 收集并以只读诊断 projection 输出，clear 时清零。
- `AllowedToSpreadInfections` 在 `UpdateWorld` 开始时根据 creative power 重算，不能由单次 tile spread 调用永久修改；提议 `WorldInfectionSpreadPolicyState` 的 owner 每个 world update 生成闭合快照。
- `npcSpawnDelay` 是当前节拍计数，`npcSpawnPeriod` 由 world update rate 推导；两者共同属于 `WorldNpcSpawnPacingState`，但 NPC 生成 effect 由 NPC/WorldGen adapter 承担。
- `stopDrops` 在 meteor/局部生成事务中成对 true/false，表示当前 effect transaction 抑制掉落；不能暴露给任意系统，也不能跨事务残留。
- `destroyObject` 被大量家具/结构检查方法作为 nested guard 使用，且 Main update 会在非 loading 时重置；它更接近带作用域的 `WorldObjectDestructionContext`，不应成为长期世界布尔 authority。

### 6.7.2 读写、生命周期和副作用

| 边界 | 输入 | 输出 | 唯一 owner/副作用 | 生命周期 |
|---|---|---|---|---|
| `WorldHousingBuildDiagnosticState` | house construction result/failure facts | immutable diagnostic snapshot | housing generation system | world create/pass -> clear |
| `WorldInfectionSpreadPolicyState` | hardmode/world mode、creative power snapshot、policy definition | `CanSpread` snapshot | UpdateWorld policy owner | recompute per world update; no stale reuse after power change |
| `WorldNpcSpawnPacingState` | world update rate、time and spawn results | due/not-due decision | pacing system; NPC spawn adapter performs effect | world load -> tick -> clear |
| `WorldDropSuppressionContext` | scoped meteor/structure effect | suppression decision | owning effect system; item drop adapter observes | enter -> effect -> finally exit |
| `WorldObjectDestructionContext` | validated multi-tile object operation | guard/operation result | object placement/destruction system | one operation; reset on success/failure/cancel |

`UpdateWorld` 的副作用顺序需要拆成可见阶段：loading guard -> policy snapshot -> mechanism/TileEntity/liquid updates -> world tile effects -> NPC pacing/housing checks -> falling-object effects。policy Query 不能调用 creative power service 或写 tile；effect adapter 才能发 item drop、tile frame、NPC spawn、network 和日志。

### 6.7.3 兼容和 verifier

focused verifier 需要覆盖：

1. 建房缺家具/缺光的诊断 flag 在不同失败路径和 clearWorld 后准确清除，且诊断 projection 不能修改住房关系。
2. creative power 开关、hardmode、world update rate 变化对感染策略/刷怪周期的即时快照；loading/paused 时不误推进计数。
3. `npcSpawnDelay` 的达到、重置、invasion/eclipse 阻断和重复 tick；NPC spawn 失败时计数保留或重试策略明确。
4. `stopDrops` 和 `destroyObject` 在成功、提前 return、异常/取消、嵌套结构 operation 后均恢复，不污染下一个操作。
5. `UpdateWorld` 的系统顺序与 effect commit 顺序可通过 event log/spy ports 断言，网络/音效/掉落均不能从纯规则 Query 产生。

当前证据足以完成 C05 设计，但 `UpdateWorld` 全调用图、线程边界和 effect owner 仍是 partial；verifier 未执行，`verificationStatus: not-run`。

## 6.8 C06 设计检查点：Tile merge pass context

### 6.8.1 Owner 与分类

- `mergeUp`、`mergeDown`、`mergeLeft`、`mergeRight` 共同描述一次 tile framing 的邻居替换条件，不能拆为四个独立组件，也不应作为持久世界状态。
- `TileFrame` 开始阶段清零 flags，特定 frame pattern 设置 flags；邻居处理使用反向关系（例如 `mergeDown` 影响上邻居），该映射是行为契约，不能按名称直觉改写。
- cosmetic recursive framing 会复制并恢复四个 flags，`tileReframeCount` 还负责限制递归深度；提议 `WorldTileMergePassContext` 保存 flags 和 nesting token，由 `WorldTileFrameSystem` 唯一 owner 管理。
- `TileMergeQuery` 只根据 explicit neighbors、frame conditions 和 flags 返回替换后的 neighbors；写 tile/frame/network 的动作留给 effect/commit port。

### 6.8.2 读写、生命周期和副作用

| 边界 | 输入 | 输出 | 唯一 owner/副作用 | 生命周期 |
|---|---|---|---|---|
| `WorldTileMergePassContext` | frame call、directional merge flags、nesting depth | immutable flag snapshot | TileFrame system；创建/清零/暂存/恢复 | one TileFrame call and nested cosmetic calls |
| `TileMergeQuery` | neighbor snapshot、tile merge catalog、flag snapshot | replacement neighbor snapshot | no writes | pure per frame |
| `WorldTileFrameCommitPort` | query result、tile coordinate | framed tile and optional network update | tile/frame owner only | after query; failure must restore context |
| `WorldTileReframeCounter` | nested frame requests | recursion guard | frame owner; increments/decrements in finally | one root reframe operation |

不把 flags 放入 save/network/persistent ECS state。`tileReframeCount` 是 C01/C12 的交接事实，但 flag context 自身不可跨 tick；所有异常/early return 都必须清零或恢复上层 snapshot。

### 6.8.3 兼容和 verifier

focused verifier 需要覆盖：

1. 四方向 flag 的 16 个组合以及上/下、左/右反向映射与 Version4 结果一致。
2. TileFrame 开始清零、特定 pattern 设置、cosmetic recursion 暂存恢复和 root 结束清理。
3. 嵌套深度上限、`tileReframeCount` 增减配对、异常/取消后的 context 无泄漏。
4. pure Query 无 tile/frame/network 写入；commit port 只写一次并能发布必要 frame/network effect。
5. Dome `TileMergeQuery` 和 `Terraria.Dome.WorldGeneration.Verification` 的现有测试只作为 existing-evidence，必须重新确认它们覆盖 Version4 的全部调用语义后才可作为迁移 gate。

verifier 尚未执行，`verificationStatus: not-run`。

## 6.9 C07 设计检查点：住房计数、预算与房间评分

### 6.9.1 Owner 与分类

- `numTileCount`、`CountedTiles`、`lavaCount`、`iceCount`、`sandCount`、`rockCount`、`shroomCount` 是一次 `countTiles`/`nextCount` 递归调用的可变上下文，不是跨 tick 的住房注册表。入口必须清零，递归以 `Point` 去重，并在成功、截断、异常和取消路径结束时丢弃或归还上下文。
- `maxTileCount` 和 `maxWallOut2` 是调用方提供的预算/边界 Definition；它们不能被 `HousingScanVisitedTilesContext` 私自修改。`maxWallOut2` 的完整写者和墙体扩散调用链仍为 partial，先保留 `HousingScanBudgetDefinition` 的只读输入角色。
- 五类材质计数应在扫描结束时封装为不可变 `HousingMaterialCounterSnapshot`，供生成 pass 的筛选 Query 消费；不得让后续调用复用上一次扫描的计数。
- `maxRoomTiles`、`maxRoomSize` 是房间检查/评分的规则输入；`roomTiles`、`numRoomTiles` 和 `hiScore` 分别属于房间访问上下文与评分快照。`hiScore` 的初值、负值占用短路和候选更新必须由唯一评分 owner 管理。
- `prioritizedTownNPCType` 表达当前住房/城镇 NPC 选择意图，不能与房间材质统计合并。它和 `Main.npc`、TownManager 及 C08 的住房关系提交存在跨边界写入，保持 `HousingAssignmentPriorityState` + `crossSubsystemOwner: integration-review`。

### 6.9.2 读写、生命周期和副作用

| 边界 | 输入 | 输出 | 唯一 owner/副作用 | 生命周期 |
|---|---|---|---|---|
| `HousingScanBudgetDefinition` | `maxTileCount`、`maxWallOut2`、调用 pass 的目标 | 有界扫描预算 | budget provider；不得写 tile/NPC | 生成/住房扫描配置生命周期 |
| `HousingScanVisitedTilesContext` | 起点、world bounds、预算、tile/wall/liquid reader | 去重访问集合与 `numTileCount` | `HousingScanSystem`；仅在 pass 内写 context | one recursive scan |
| `HousingMaterialCounterSnapshot` | closed scan counters | lava/ice/sand/rock/shroom counts | snapshot builder；无外部写入 | one scan result |
| `HousingRoomVisitedTilesContext` | room start、`maxRoomSize`/`maxRoomTiles`、tile reader | bounds、`roomTiles`、`numRoomTiles` | room-check owner；不得直接提交 NPC assignment | one room check |
| `HousingRoomScoreSnapshot` | tested bounds、tile category counts、occupancy result、room context | `hiScore`、`bestX`、`bestY`、`sharedRoomX` outcome | `HousingRoomScoreSystem`；feedback 通过 port 输出 | one scoring attempt |
| `HousingAssignmentPriorityState` | NPC selection command | validated prioritized NPC type | housing/spawn owner；提交方向交给 C08 | one housing decision window |

最小 seam 是 `HousingScanInput -> HousingMaterialCounterSnapshot` 与 `HousingRoomScoreInput -> HousingRoomScoreSnapshot`。Query 只能读取显式 tile/occupancy snapshots；`ScoreRoom` 的 feedback、NPC home 写入、TownManager 关系和网络通知必须留在 System/Command/Adapter。不得把 `CountedTiles`、`roomTiles` 或 score scratch 放入存档/网络投影。

### 6.9.3 兼容、依赖影响和 verifier

拟议目标路径保持 domain-first：`src/WorldSession/WorldGeneration/Housing/` 下的 `HousingScanBudgetDefinition`、`HousingScanVisitedTilesContext`、`HousingMaterialCounterSnapshot`、`HousingRoomVisitedTilesContext`、`HousingRoomScoreSnapshot` 和 `HousingAssignmentPriorityState` 均为 `status: proposed`；不创建 `Shared/Components/`。依赖方向为 Version4 tile/wall readers -> Housing Query/System -> immutable snapshot -> C08 housing assignment command，且不允许 Query 回写 `WorldGen`、`Main.npc` 或 TownManager。

迁移影响必须记录：源成员为 `WorldGen.cs:4193-4219,4237` 的 15 项；目标是上述六个职责边界，调用方包括 `countTiles`/`nextCount`、`countDirtTiles`、生成 pass 和 `StartRoomCheck`/`ScoreRoom`/`SpawnTownNPC`。兼容层先保留旧字段名和原始调用顺序，避免 `countDirtTiles` 与 `countTiles` 共享可变 context 造成串数。

focused verifier 需要覆盖：

1. 空间边界、重复点、不可通行点、墙体/液体早停、预算恰好命中和超预算后的 `numTileCount == maxTileCount` 行为。
2. 五类材质计数只统计一次，`countTiles`/`countDirtTiles` 连续调用之间无泄漏，异常/取消后下一次扫描从零开始。
3. `StartRoomCheck` 的 `maxRoomTiles`/`maxRoomSize` 边界、`ScoreRoom` 占用短路 (`hiScore = -1`)、污染重罚提前返回、候选 tie 不覆盖和高分坐标提交。
4. `HousingRoomScoreSnapshot` 与 `IRoomCheckFeedback` 的输出顺序、唯一评分 writer，以及 Query 无 tile/NPC/TownManager 写入。
5. 与 C08 的 handoff 只提交 `HousingRoomScoreSnapshot`/priority command；NPC key 冲突仍失败到 `crossSubsystemOwner: integration-review`，不得通过重复写两个 registry 绕过。

当前 verifier 未执行，C07 证据等级仍为 `partial`；实现、编译、运行时行为等价和测试均未声明。

## 6.10 C08 设计检查点：房间搜索上下文、资格结果与住房关系提交

### 6.10.1 Owner 与分类

- `roomX1`、`roomX2`、`roomY1`、`roomY2`、`LastFoundHouse`、`currentlyTryingToUseAlternateHousingSpot`、`sharedRoomX` 和 `_roomCheckStack` 是一次房间搜索及其递归/替代房间尝试的短生命周期上下文。它们必须随搜索实例创建、在成功/失败/取消路径清理，不能作为世界级 registry 或跨 NPC 复用。
- `houseTile` 是本次房间扫描发现的 tile 分类 scratch；`roomTorch`、`roomDoor`、`roomChair`、`roomTable` 是 `RoomNeeds` 的四项资格结果；`roomHasStinkbug` 和 `roomHasEchoStinkbug` 是扫描诊断/刷怪限制输入。它们应分别落在 `HousingRoomTileClassificationContext`、不可变 `HousingRoomRequirementResult` 和 `HousingRoomDiagnosticFlags`，避免把“发现的 tile 类型”和“资格结论”混成一个可变组件。
- `canSpawn` 是当前房间流程的阶段性资格结果，不是 NPC 生成事实；`bestX`/`bestY` 是 `HousingRoomCandidateSnapshot`，必须只在 `ScoreRoom` 产生更高且有效的候选时更新。`roomCheckFailureReason` 是 `HousingRoomDiagnosticProjection`，不能由 UI/feedback 反向写回房间状态。
- `RoomNeeds` 在 Version4 中会重置并写入四个 requirement flags 及 `canSpawn`，因此核心迁移接口应是 `RoomNeedsQuery(houseTileSnapshot) -> HousingEligibilityResult`；旧 facade 的兼容写回只能存在于 owner System，不得把 Query 实现为修改全局 `WorldGen`。
- `QuickFindHome` 是编排行为而非 Query：它调用 `StartRoomCheck`、`RoomNeeds`、`ScoreRoom` 和 occupancy 检查，成功时写入 `Main.npc[npc].homeTileX/homeTileY`、`homeless=false`、`homelessDespawn=false`，失败时写 `homeless=true`。这些写入必须通过 `HousingAssignmentCommand` 交给 C08 owner System/外部 NPC Adapter。

### 6.10.2 读写、生命周期和副作用

| 边界 | 输入 | 输出 | 唯一 owner/副作用 | 生命周期 |
|---|---|---|---|---|
| `HousingRoomSearchContext` | 起点、world bounds、room limits、tile/wall reader | bounds、visited tiles、house tile classification、failure reason | `HousingRoomSearchSystem`；只写本次 context | one `StartRoomCheck` attempt |
| `HousingRoomRequirementQuery` | immutable house tile type snapshot、RoomNeeds registries | chair/table/door/torch flags 与 `CanSpawn` | pure Query；无全局写入 | per room result |
| `HousingRoomCandidateSnapshot` | score result、candidate coordinate | best home tile与 score revision | `HousingRoomScoreSystem`；不写 NPC | one score pass |
| `HousingRoomDiagnosticProjection` | failure callbacks、stinkbug flags、candidate trace | readable diagnostic event/reason | feedback/diagnostic adapter；只向外输出 | one attempt or diagnostic stream |
| `HousingAssignmentCommand` | NPC identity/key、home tile、homeless/homelessDespawn、source scan revision | accepted/rejected assignment | NPC/Town housing owner；写 home relation、TownManager adapter、必要 network/progression effects | one commit |
| `HousingAlternateSpotScope` | TownManager room fallback and recursion token | restored candidate/guard state | owner System; `finally` restores `currentlyTrying...`, `bestX`, `bestY` | nested spawn attempt |

`HousingRoomSearchContext` 只负责一次尝试的生命周期和递归/候选状态；`VisitedTiles`、`TileClassification` 和
`DiagnosticFlags` 分别复用已有的专责上下文，不重复持有 bounds、visited 集合或 tile 分类数组。它的 `Clear` 统一复位
这些短生命周期子状态，但不执行 flood-fill、RoomNeeds、评分、feedback 或外部提交。

`QuickFindHome` 还暂时把 `Main.tileSolid[379]` 置为 `true`，结束时恢复旧值。迁移必须把该操作包装为显式 `HousingTileSolidityOverrideScope`，并在异常、取消和递归返回中使用 `finally`；不能让房间 Query 直接修改全局 tile catalog。成功 assignment 的外部 effect 顺序应可观察：房间快照 -> eligibility -> score -> occupancy -> single assignment commit -> NPC/Town/network/progression projections。

### 6.10.3 外部 key、兼容策略和 verifier

Version4 `TownRoomManager` 的 `_roomLocationPairs` 与 `_hasRoom` 以 `int npcID/type` 为 key，`SetRoom` 替换同 type 的坐标，`Save`/`Load` 直接序列化该 type 和 `Point`；当前 NLTX 同时存在 `TownHousingResidentKey(int NpcType)` registry 与 `TownHousingAssignmentComponent(PersistentEntityId)` assignment。两者不能在本检查点互相替换、双写或按文件名裁决。`HousingAssignmentCommand` 先携带显式 `ResidentKeyMode`/映射证据，最终 owner 标记为 `crossSubsystemOwner: integration-review`。

目标路径继续为 `src/WorldSession/WorldGeneration/Housing/`（已落地的隔离类型为 `status: implemented-isolated`；未落地的搜索上下文、Query、Projection、Command 和 scope 仍为 `status: proposed`），NPC relation adapter 可以位于 `src/WorldSession/WorldGeneration/Adapters/`，但不把 `TownRoomManager`、`NPC`、XNA `Point` 渗入核心 Query。源成员为 `WorldGen.cs:4221-4269` 的 19 项；关键行为证据来自 `RoomNeeds:5310-5357`、`QuickFindHome:5361-5422`、`StartRoomCheck/CheckRoom:5704-5840`、`ScoreRoom` 的 occupancy handoff，以及 `TownRoomManager.cs` 的 type-key Save/Load。现有 `HousingScanStateComponent` 只能作为 partial 迁移参考，不能作为最终聚合组件。

focused verifier 需要覆盖：

1. `StartRoomCheck` 每次尝试清理 bounds、`roomTiles`、`houseTile`、stinkbug flags、failure reason 和 stack；边界、起始 solid tile、open gate、unsafe wall、room too small/too big 的 failure reason 具有稳定优先级。
2. `RoomNeedsQuery` 对 chair/table/door/torch 四项执行 AND，输入 snapshot 不被修改；旧 facade 的 flags 只由 owner System 发布。
3. `QuickFindHome` 的初始点、邻域点和半径回退顺序；成功仅提交一次 home assignment 并清除 homeless/homelessDespawn，失败保持 homeless；`tileSolid[379]` 在所有返回/异常路径恢复。
4. alternate housing recursion 的 guard、`bestX/bestY` 保存恢复、nested failure/exception 清理，以及 C07 score snapshot 到 C08 command 的单向 handoff。
5. NPC occupancy 的 ignore-self、TownManager compatibility key、PersistentEntityId 映射缺失、重复 assignment 和 assignment/homeless 互斥；禁止以双 registry 写入掩盖 key 冲突。
6. feedback/diagnostic 只接收事件，不改变资格/score；NPC home、TownManager、network 和 progression side effects 不从纯 Query 产生。

当前 verifier 未执行，C08 证据等级为 `partial`；key owner、完整 NPC authority、Save/Load 迁移映射和运行时 parity 仍未批准。

## 6.11 C09 设计检查点：住房规则定义与世界事件诊断边界

### 6.11.1 Owner 与分类

- `WorldGenParam_Evil` 是生成请求参数，不是已经提交的 `crimson`/世界邪恶事实。输入边界只接受 Version4 已观察的 `-1`（随机）、`0`（腐化）和 `1`（猩红）；CLI/配置 Adapter 负责解析，`WorldEvilRuleDefinition` 负责值域检查，生成 owner 才能提交结果。
- `cactusWaterWidth`、`cactusWaterHeight` 和 `cactusWaterLimit` 是只读规则 Definition，不属于住房关系或世界可变状态。`CactusWaterEligibilityQuery` 读取显式 tile liquid snapshot、world bounds、remix/worldSurface 输入，并保留 Version4 的窗口范围、`InWorld` 边界和 `num3 / 255 > cactusWaterLimit` 整数除法语义。
- `mysticLogsEvent` 是 `MysticLogFairiesEvent` 外部事件对象的生命周期句柄，不是住房诊断组件。对象内部持有 `_canSpawnFairies`、delay 和 stump coordinates，并会改写 `NPC.Spawner.fairyLog`、扫描 tile 和触发 fairies 尝试；核心 ECS 只接收事件意图/诊断投影，不持有 XNA/Version4 对象。

### 6.11.2 读写、生命周期和副作用

| 边界 | 输入 | 输出 | 唯一 owner/副作用 | 生命周期 |
|---|---|---|---|---|
| `WorldEvilRuleDefinition` | validated `-1/0/1` generation request | immutable evil selection request | world-generation command owner；不得由 Query 改 `crimson` | one world creation request |
| `CactusWaterRuleDefinition` | width/height/limit constants | immutable rule catalog | generation definition owner；无写入 | process/world-generation catalog |
| `CactusWaterEligibilityQuery` | explicit liquid window, bounds, remix/worldSurface | allowed/blocked decision and measured liquid sum | pure Query；不种植、不改 tile | one cactus placement attempt |
| `MysticLogsEventAdapter` | world load/night/update/clear and fallen-log callbacks | event state/diagnostic projection | event adapter owns Version4 object, NPC spawner, clock and external effects | world lifetime; reset on clear |
| `MysticLogsEventProjection` | adapter callbacks and failure outcome | immutable diagnostic/event record | projection only; no reverse write | one event window |

`WorldGenParam_Evil` 的写入不得和 `WorldEvilSelectionPassContext` 或 C01 的 `crimson` 形成 dual-write；输入参数在生成开始后应冻结。仙人掌规则 Query 不能调用 `PlantCactus`/`GrowCactus`，而是把 placement decision 交给显式 terrain effect owner。`MysticLogsEventAdapter` 的 `StartWorld`、`StartNight`、`UpdateTime`、`WorldClear` 和 `FallenLogDestroyed` 需要显式调度端口，不能从住房 Query 或诊断 projection 隐式触发。

### 6.11.3 兼容、依赖影响和 verifier

目标路径建议为 `src/WorldSession/WorldGeneration/Housing/WorldEvilRuleDefinition.cs`、`HousingWaterRuleDefinition.cs`、`CactusWaterEligibilityQuery.cs` 和 `src/WorldSession/WorldGeneration/Adapters/MysticLogsEventAdapter.cs`；其中 `WorldEvilRuleDefinition`、`HousingWaterRuleDefinition`、`CactusWaterGridSnapshot`、`CactusWaterEligibilityResult` 和 `CactusWaterEligibilityQuery` 已以隔离实现落地，MysticLogs 边界仍为 `status: proposed`。不得把事件对象或液体扫描缓存放入泛型 `World` 组件。源成员为 `WorldGen.cs:4251-4259` 的 5 项；调用影响包括 `Main.cs` 的 evil 输入与昼夜调用、`WorldGen` 生成 pass/`GrowCactus`、world clear 和 `Hooks.OnWorldLoad`，以及 C08 住房资格的只读规则交接。

focused verifier 需要覆盖：

1. evil 参数 `-1/0/1` 接受、其他值拒绝、重复/晚到 command 不覆盖已冻结请求，并确认 `crimson` 仍只有经 integration review 的 owner。
2. cactus 窗口宽高、world-bound clipping、液体求和、`num3 / 255` 整数截断和 limit 边界（等于/超过）与 Version4 一致；Query 不写 tile 或生成状态。
3. Cactus decision 到 placement effect 的单向提交，remix/worldSurface 条件不从静态 Definition 偷读全局 mutable state。
4. Mystic logs 的 StartWorld/StartNight/UpdateTime/FallenLogDestroyed/WorldClear 顺序、delay 递减和 clear 后无旧 stump/event 状态；异常/取消不会留下 adapter-owned effect 状态。
5. adapter 是唯一 `MysticLogFairiesEvent`/`NPC.Spawner.fairyLog` 写者，event projection 不反向触发住房规则或 NPC home assignment；Main 调度每个 lifecycle callback 的次数可由 spy port 验证。

当前 verifier 未执行，C09 证据等级为 `partial`；evil committed-state authority、cactus 完整调用图、事件效果端口和运行时 parity 仍未批准。


## 6.12 C10 设计检查点：世界尺寸 catalog、扩散边界与陨石事件交接

### 6.12.1 Owner 与分类

- `WorldSizeSmallX/SmallY`、`WorldSizeMediumX/MediumY`、`WorldSizeLargeX/LargeY` 是不可变尺寸 catalog（4200x1200、6400x1800、8400x2400），不是当前世界的 `Main.maxTilesX/maxTilesY` 组件。尺寸选择、tile allocation、world edge floats 和 WorldFile load/save 必须有一个经审查的 dimension owner。
- `InfectionAndGrassSpreadOuterWorldBuffer` 是只读 spread-boundary Definition，当前只确认声明值 10，尚未在 Version4 源码中找到消费点；不能因为名称推断它已约束感染/草扩散，也不能在迁移中添加隐含使用。
- `meteorShowerCount` 是陨石事件的可变进度事实，`StartMeteorShower` 以随机 650..750 再乘 4 初始化，`dropMeteor` 在成功生成路径递减并在快进时清零。它归 C04 的 progression/event owner；C10 只提供 world-size/meteor spawn geometry 所需的显式输入和跨分区 handoff。

### 6.12.2 读写、生命周期和副作用

| 边界 | 输入 | 输出 | 唯一 owner/副作用 | 生命周期 |
|---|---|---|---|---|
| `WorldSizeCatalogDefinition` | fixed small/medium/large dimensions | validated size profile | dimension definition owner; no allocation | process/catalog lifetime |
| `WorldDimensionSelectionQuery` | size request, compatibility policy | `(maxTilesX, maxTilesY)` selection | pure Query; no `Main` writes | one world creation/load decision |
| `WorldSpreadBoundaryDefinition` | buffer value and explicit dimension profile | bounded spread rectangle/edge policy | spread policy owner once consumer is identified | one world/pass policy |
| `WorldMeteorGeometryQuery` | closed dimension snapshot, surface/sky constraints | valid meteor spawn coordinate/range | pure Query; projectile/terrain adapter commits effect | one meteor attempt |
| `MeteorProgressionHandoff` | `meteorShowerCount` event snapshot/command | C04 event command or read-only projection | C04 progression owner; WorldFile adapter only serializes proven fields | event window / save snapshot |

`WorldSizeCatalogDefinition` must be the single source for future size selection, but migration cannot silently replace Main's literal assignments until the auto-create, CLI, `clearWorld`, and WorldFile compatibility paths agree. `WorldFile` writes/reads world edge and max tile dimensions, while `meteorShowerCount` is currently observed through temporary metadata capture/restore; this is not enough evidence to invent a new persistent record or change file versioning.

### 6.12.3 兼容、依赖影响和 verifier

目标路径为 `src/WorldSession/WorldGeneration/Passes/WorldSizeCatalogDefinition.cs`、`WorldDimensionSelectionQuery.cs`、`WorldSpreadBoundaryDefinition.cs` 和 `WorldMeteorGeometryQuery.cs`；前三个类型及 `WorldMeteorGeometryQuery` 已在第 9 节以隔离实现落地，meteor adapter/Projection 仍为 `status: proposed`，并通过 C04 integration boundary，不把 event counter 复制到 C10。源成员为 `WorldGen.cs:4271-4285` 的 8 项；依赖影响包括 `Main.autoCreate`/CLI size selection、`WorldGen.clearWorld`、WorldFile edge/dimension I/O、spread policy consumers（尚未发现）和 C04 meteor event/WorldFile temporary state。

focused verifier 需要覆盖：

1. 三档尺寸 catalog 的精确值、无效 size 拒绝、size selection 不写 Main，以及 auto-create/CLI/clearWorld/WorldFile 的单一 owner 和一致映射。
2. world edge 与 `maxTilesX/Y` 的 tile/world-unit 换算、旧尺寸兼容、加载失败/尺寸不匹配回滚，以及 C03 `lastMaxTilesX/Y` footprint 的隔离。
3. spread buffer 的使用闭合：在没有发现 consumer 前 verifier 必须证明不会产生隐式约束；找到 consumer 后再验证边界内/外以及感染与草扩散的独立 owner。
4. meteor geometry 的边界、玩家/表面约束和随机坐标 Query 不写 counter；`StartMeteorShower` 初始化、成功 drop 递减、快进清零和失败保留行为由 C04 event verifier 覆盖。
5. WorldFile temporary capture/restore、save wait 与 event counter 的版本/并发交接；没有完整格式证据时，adapter 必须保持 blocked，不新增序列化字段。

当前 verifier 未执行，C10 证据等级为 `partial`；尺寸 owner、spread consumer、meteor persistence/version 和运行时 parity 仍未批准。

## 6.13 C11 设计检查点：生成阶段短生命周期 scratch

### 6.13.1 Owner 与分类

- `trapDiag` 是四类陷阱生成尝试的二维成功/失败计数 scratch。当前只观察到陷阱生成分支写入，未观察到读取、持久化或网络投影；不能把它升级成世界指标。提议 `TrapGenerationDiagnosticScratch` 只在一个生成 pass 中租用，并以 immutable diagnostic snapshot 向显式 feedback/trace port 输出（若后续证据确认存在 consumer）。
- `gem` 是六种宝石选择的布尔允许集。`gemCave` 入口先清零，再至少选中一个随机项并可能追加项；`randGem`/`randGemTile` 读取它。提议 `GemSelectionPassScratch` 在 `gemCave` scope 内独占，不能与 tile count 或 moss palette 共享可变数组。
- `mossType[3]` 是按横向区域使用的三个互异 moss palette index；`neonMossType` 是 neon palette tile id。`randMoss` 总是刷新 neon，只有 `justNeon == false` 才刷新三项普通 palette；`setMoss` 依赖普通 palette，`neonMossBiome` 依赖 neon palette。两者必须保持不同的 field lifetime，`justNeon` 调用不得清空普通 palette。
- 这四个成员的共同边界是“生成 pass scratch”，但不应创建一个可跨 pass、可存档的 `WorldGenerationScratchComponent`。按稳定访问者分别拆成 `TrapGenerationDiagnosticScratch`、`GemSelectionPassScratch` 和 `MossPalettePassScratch`，三个类型只由生成 pass owner 管理。

### 6.13.2 读写、生命周期和副作用

| 边界 | 输入 | 输出 | 唯一 owner/副作用 | 生命周期 |
|---|---|---|---|---|
| `TrapGenerationDiagnosticScratch` | trap type、attempt outcome | bounded success/failure counters、optional diagnostic snapshot | trap generation pass；不写 world save/network | one trap pass; reset at pass start/end once consumer is known |
| `GemSelectionPassScratch` | injected random source、gem cave attempt | allowed gem set | gem-cave pass；`Spread.Gem`/tile placement adapter consumes | one `gemCave` call; clear before selection and discard after |
| `MossPalettePassScratch` | injected random source、`justNeon` mode | neon palette and optional three-region moss palette | moss generation pass; `setMoss` publishes to GenVars through explicit effect port | Moss pass; full palette persists across just-neon subcalls only within pass |
| `WorldGenerationRandomPort` | seed/pass random stream | deterministic draws | adapter owns `UnifiedRandom` bridge; scratch never owns global RNG | pass scope / explicit stream lease |

这些 scratch 只允许被 pass owner 和显式 consumer 读取；Query 可以读取 immutable scratch snapshot，但不能修改数组或调用 `Spread`/tile placement。清理策略必须区分“入口清零”与“成功后消费”：`gem` 入口清零是已证实语义；`mossType` 在 `justNeon` 时保留是已证实语义；`trapDiag` 的 reset 时点仍需补证据，未经确认不得插入会改变现有累计行为的 clear。所有 lease 都要在 `finally` 归还，避免生成失败后下一个 pass 看到旧 palette/diagnostic。

### 6.13.3 兼容、依赖影响和 verifier

目标路径为 `src/WorldSession/WorldGeneration/Passes/` 下的 `TrapGenerationDiagnosticScratch.cs`、`GemSelectionPassScratch.cs`、`MossPalettePassScratch.cs` 和 `WorldGenerationRandomPort.cs`，均为 `status: proposed`；不创建通用 `Shared/Scratch` 目录。源成员为 `WorldGen.cs:4292-4298` 的 4 项；调用影响包括陷阱生成分支、`gemCave`/`randGem`/`randGemTile`、`randMoss`/`neonMossBiome`/`setMoss`、`Spread.Gem`/`SpreadGrass` 和 `GenVars.mossWall/mossTile` 的 effect boundary。

focused verifier 需要覆盖：

1. trap type 0..3、success/failure 计数方向、溢出/预算策略、pass start/end reset，以及异常/取消后的 scratch 生命周期；若无 consumer 证据，验证结果必须明确为“仅写入、无外部投影”。
2. `gemCave` 清零、至少一个 gem 被选中、额外随机选项、`randGem` 只返回允许项、`randGemTile` 的 1/20 宝石分支，以及重复 gem pass 不泄漏前一轮选择。
3. `randMoss(false)` 产生三个互异普通 palette index 并刷新 neon；`randMoss(true)` 只刷新 neon 且保留普通 palette；`setMoss` 横向 0/1/2 区域映射和 `neonMossBiome` 消费的 palette 不串用。
4. 随机 port 的 draw 顺序、seed/stream lease、异常返回和 pass 间隔离；禁止通过 global random 读写隐藏顺序依赖。
5. scratch 不进入 WorldFile/NetMessage/长期 ECS state；`GenVars.mossWall/mossTile` 和 tile placement 只通过明确 effect owner 提交，Query/Projection 不写回。

当前 verifier 未执行，C11 证据等级为 `partial`；trapDiag consumer/reset、随机端口和 GenVars/tile effect owner 仍未批准。

## 6.14 C12 设计检查点：地形效果、队列、TreeTops 与环境缓存

### 6.14.1 Owner 与分类

- `tileSolidBackup` 是世界生成期间对 `Main.tileSolid` 的 temporary backup：`Reset` 克隆，`RestoreTemporaryStateChanges` 恢复。它是 generation scope，不是长期组件；所有早退/异常路径必须仍能恢复原数组。
- `fossilBreak` 是 `AttemptFossilShattering` 的递归/嵌套 guard，`hardModeWorldUpdates` 是每次 `UpdateWorld` 从 Main/secret-seed 输入重算的 tile policy，`growGrassUnderground` 是 overground/underground 子阶段 scope，`grassSpread` 是 `SpreadGrass` 递归深度/上限计数。它们虽然都是 bool/int，生命周期和 owner 不同，不能合并为 `TerrainStateComponent`。
- `_isRainingBoulders` 是 falling-object event 的上一轮/当前轮状态，可能触发 achievement/projection；`_SpawnThunderStorm_SafeSpots` 是每次 storm update 清空并从 active players 重建的 safe-zone scratch。两者分别归 event state 与 update-pass context。
- `ExploitDestroyQueue` 是延迟 frame/network effect queue；`CheckExploitDestroyQueue` 在 `destroyObject` 时跳过 flush，正常路径 dequeue、重新 frame active tiles 并发送 tile square。当前未发现 enqueue 写者，不能声明 queue 的 producer 或持久化语义。
- `TreeTops` 是独立的 13-area authoritative world presentation state，已有 `WorldFile.Save/Load`（版本 <211 fallback）和 `NetMessage.SyncSend`；它不能与 `BackgroundsCache` 合并。`BackgroundsCache` 从 background style fields 更新 variation，并在 Main tick 衰减 flash power，是派生 cache/projection。
- `_coatingColors` 是 `coatingColors` 返回的复用 list scratch，必须通过 immutable/read-only copy 或明确借用协议暴露；`catTailDistance` 是 cat-tail rule definition。`ItemSpawnProtectionTime`、`BUBBLES_SOLID_STATE_FOR_HOUSING`、`strip_w`、`strip_h`、`bitStrip` 当前只有声明或无观察到 consumer，保留为 evidence gap，不凭名称添加行为。
- `heartPos`/`heartCount` 是 Crimson generation 的 bounded pass scratch：`Reset` 清零 count，`CrimEnt` 追加，`CrimPlaceHearts` 读取并放置。`_preventInfiniteRopeFraming` 只观察到 rope end framing 后清零，read/guard owner 尚未闭合；暂按 frame pass guard 暂缓实现。

### 6.14.2 读写、生命周期和副作用

| 边界 | 输入 | 输出 | 唯一 owner/副作用 | 生命周期 |
|---|---|---|---|---|
| `WorldTerrainTileOverrideScope` | tile-solid catalog snapshot、generation transaction | restored tile-solid state | generation owner；finally restore `Main.tileSolid` | one GenerateWorld/Reset scope |
| `WorldFossilShatterScope` | fossil tile、random/effect port | nested shatter decisions | fossil system；tile kill/network effects through port | one fossil attempt |
| `WorldHardmodeTilePolicyState` | Main hardmode/seed flags | tile conversion policy | UpdateWorld owner; no save/network | one world update |
| `WorldGrassSpreadEffectState` | update phase, recursive spread request | underground policy and bounded recursion counter | grass owner; tile/frame effects through port | one phase/recursive call |
| `WorldTerrainEventEffectState` | weather/players/world state | boulder event state, heart placement input | event owner; projectile/achievement/tile effects | update or generation event |
| `WorldTerrainDestructionQueueComponent` | validated point commands | deferred frame/network effects | queue owner; `destroyObject` gates flush | one update/operation window; not persisted |
| `WorldTreeTopsStateComponent` | area style mutations | 13-area tree style snapshot | TreeTops owner; WorldFile/NetMessage adapters | world lifetime; explicit save/sync |
| `WorldBackgroundFlashCacheProjection` | background style snapshot and tick | variation/flash view | cache owner; no authoritative writes | derived cache with invalidation/tick decay |
| `WorldTerrainPassScratchContext` | coating, safe spots, heart positions, bit strips | pass-local diagnostics/geometry | pass owner; no save/network | one pass/update; clear/return in finally |
| `WorldTerrainRuleDefinition` | cat tail/item/housing/strip constants | immutable rule/catalog values | definition owner; no writes | process/catalog lifetime |

Effect ordering must be explicit: `UpdateWorld` derives hardmode/grass policy, runs tile/liquid/house/spawn phases, flushes allowed destruction effects and publishes required network changes; `GenerateWorld` snapshots/restores tile solidity around passes; fossil/rope/grass recursion guards restore on every exit; storm safe spots are rebuilt before lightning attempts; Crimson hearts are collected before placement. None of these ordering facts may be encoded by Markdown/file order.

### 6.14.3 兼容、依赖影响和 verifier

目标路径按 capability 分开，均为 `status: proposed`：`src/WorldSession/WorldGeneration/Terrain/` 下的 `WorldTerrainTileOverrideScope`、effect state/queue/pass context，`src/WorldSession/WorldGeneration/Terrain/TreeTops/WorldTreeTopsStateComponent.cs` 与其 `Adapters/WorldFileTreeTopsAdapter.cs`、`Adapters/NetTreeTopsProjection.cs`，以及 `Metrics/WorldBackgroundFlashCacheProjection.cs`。不创建 `Shared/Components/`，不把 XNA `Point/Rectangle/Color` 直接暴露给核心 Query。

源成员为 `WorldGen.cs:4328-4366` 的 20 项。依赖影响必须记录：`Reset`/`GenerateWorld`/`RestoreTemporaryStateChanges` 的 tileSolid backup；`UpdateWorld`、`SpreadGrass`、`AttemptFossilShattering`、`HandleRopeEndFraming`、`SpawnFallingObjects`、`SpawnStormLightning`、`CrimEnt`/`CrimPlaceHearts` 的 effect/context owners；`CheckExploitDestroyQueue` 的 unknown producer；TreeTops WorldFile/NetMessage adapters；Main flash tick；以及 C05 `destroyObject` gate、C06/C01 frame/background handoffs。未观察到的 constants/bitStrip/queue enqueue 必须保持 deferred。

focused verifier 需要覆盖：

1. GenerateWorld success/failure/cancel 后 `tileSolidBackup` 必须恢复；数组长度、重复 Reset、并发/重入和 `Main.tileSolid` 外部变更不得静默覆盖，直到 owner policy 明确。
2. fossil nested guard、hardmode policy、grass underground phase、`grassSpread` <1000 recursion bound、boulder transition/achievement and rope guard 的 state/effect 顺序与 exception/finally cleanup。
3. destruction queue 的 producer/consumer闭合、`destroyObject` 阻塞时保留队列、正常 flush 每点 frame/network 一次、重复点和异常路径；无 enqueue 证据时 verifier 必须报告 blocked 而非假设 producer。
4. TreeTops 13-area Save/Load round-trip、version <211 fallback、length over/under-read、SyncSend byte projection、server authority 和 style mutation 的 single writer；BackgroundsCache 的 style invalidation、flash decay 和与 TreeTops 独立存储。
5. `coatingColors` 不泄漏可变 list、catTailDistance boundary、未消费 ItemSpawn/BUBBLES/strip/bitStrip 常量不产生隐式行为；缺 consumer 时记录 `evidence-gap`。
6. storm safe spots 每次更新清空重建、inactive/dead player 排除；heartCount 不超过 100、CrimEnt -> CrimPlaceHearts 顺序和 world reset 清理；pass scratch 不进入 save/network。

当前 verifier 未执行，C12 证据等级为 `partial`；queue producer、frame guard、未消费 declarations、TreeTops 完整权限和 runtime parity 仍未批准。

## 6.15 C13 设计检查点：WorldGen 派生属性与外部输入端口

### 6.15.1 Owner 与分类

- `TransformingWorld` 是 `_transformingWorld > 0` 的只读 Query，不是第二个事务状态。C03 的 `WorldTransformTransactionComponent/System` 负责 `Interlocked.Increment/Decrement` 和 `finally` 清理；C13 只暴露 busy/settled 结果，不能自行递增、递减或缓存一个可能过期的 bool。
- `genRand` 是 `Main.rand` 的随机源 Adapter getter。Version4 同时大量直接使用 `Main.rand`，并在 `Reset`/world generation、WorldFile load fallback 等路径重新初始化它；迁移必须显式传入 `WorldGenerationRandomPort` 或保留兼容 adapter，同时保持同一个 stream identity 和调用顺序，不能因“派生属性”而复制、重播或隐式 fork 随机源。
- `oceanLevel` 是 `WorldOceanLevelQuery` 的纯派生值，精确计算为 `(worldSurface + rockLayer) / 2.0 + 40.0`。`oceanDepths` 读取该值后再与 `beachDistance` 和 x 边界判断；C13 不拥有、缓存或写回 `worldSurface`/`rockLayer`。

### 6.15.2 读写、生命周期和副作用

| 边界 | 输入 | 输出 | 唯一 owner/副作用 | 生命周期 |
|---|---|---|---|---|
| `TransformingWorldQuery` | C03 transaction count snapshot | `IsTransforming` busy result | C03 transaction owner publishes; Query does not mutate | instantaneous read; save wait may poll |
| `WorldGenerationRandomSourceAdapter` | Main/world seed initialization and explicit pass scope | `UnifiedRandom`/random port view | Main/random adapter owns instance; callers consume draws; no hidden reset | world or pass lifetime according to existing owner |
| `WorldOceanLevelQuery` | explicit `worldSurface`, `rockLayer` values | exact ocean-level double | pure Query; no tile/world writes | per calculation; no cache required |

`WorldFile._SaveWorld` 的 wait loop只能读取 `TransformingWorld`，不能通过 Query 改变 transform state；后台 transform 必须先持有 `WorldFile.IOLock` 执行 effect，再在 `finally` 减计数并排队主线程 follow-up。随机 port 的注入只改变依赖方向，不得改变 Version4 的 `Main.rand`/`genRand` draw order；海平面 Query 的 double 运算不得提前截断为 int。

### 6.15.3 兼容、依赖影响和 verifier

目标路径建议为 `src/WorldSession/WorldGeneration/Lifecycle/TransformingWorldQuery.cs`（与 C03 owner 交接）、`src/WorldSession/WorldGeneration/Adapters/WorldGenerationRandomSourceAdapter.cs` 和 `src/WorldSession/WorldGeneration/Metrics/WorldOceanLevelQuery.cs`；`WorldOceanLevelQuery` 已在第 9 节以隔离实现落地，TransformingWorldQuery 与 WorldGenerationRandomSourceAdapter 仍为 `status: proposed`。源成员为 `WorldGen.cs:4368-4372` 的 3 个属性；依赖影响包括 `TransformWorldOnBackgroundThread`/`StartHardmode`、WorldFile save wait、`Main.rand` 的 generation/load initialization、所有使用 `genRand` 的 generation/terrain/housing callers，以及 `oceanDepths`/TreeTops/biome boundary consumers。

focused verifier 需要覆盖：

1. transform count 为 0/1/n、嵌套后台 transform、异常/取消和 main-thread follow-up；每个 increment 都有 matching decrement，WorldFile save wait 只读 busy Query，不能死锁或读到提前清零的状态。
2. `genRand` 与 `Main.rand` identity/seed initialization、pass injection、draw order、重复调用和异常返回；禁止产生第二个未声明的 RNG owner 或把 `Main.rand`/`genRand` 混成可持久化组件。
3. ocean level 对整数/小数/边界输入的精确 double 结果、`oceanDepths` 的 y 与 beach x 条件、worldSurface/rockLayer 更新后的即时可见性；Query 无写回、无隐式 cache。
4. 跨 C03/C10/C12 的 handoff 只使用事务 Query、尺寸/边界 snapshot 和 random/effect ports；文件顺序不作为调度依据。

当前 verifier 未执行，C13 证据等级为 `partial`；事务 owner、随机初始化/序列、surface/rock layer owner 和运行时 parity 仍未批准。

## 7. 跨分区交接、不拆分项与风险

- `crossSubsystemOwner: integration-review`：`TownManager`、`Manifest`、`crimson` 的持久化 owner、`mysticLogsEvent`、NPC home relation key、`TreeTops` 的网络/持久化格式、`tGood/tEvil/tBlood` 的 server authority。
- 不拆分项：短生命周期 pass context 内部的固定 scratch 数组不为了字段数量继续拆文件；`WorldSize*` 与 buffer 作为同一只读 catalog；`TreeTops` 不与背景 flash cache 合并。
- 不能从现有 NLTX `TownHousingRegistryComponent` 的 NPC type key 推断 `TownHousingAssignmentComponent` 的 PersistentEntityId key 可直接替换；必须有迁移映射和唯一 owner。
- 不把 `SceneMetrics` 或 `WorldGen.tileCounts` 统称为 `WorldMetricsComponent`：扫描中心、缓存失效、网络和持久化范围不同。
- 行为风险集中在 `WorldFile` load/save 等待 transform、住房扫描回写 NPC、后台 hardmode transform、网络 threshold bytes 和地形队列清理。

## 8. 设计状态

本文件仍不是运行时迁移完成证明。C01-C13 的设计检查点已完成；实现阶段已落地 C03 加载/变换/尺寸兼容状态组件、C04 进度事实组件、C05 诊断/节拍/作用域状态、C08 房间 tile 分类上下文、不可变 requirement result 和诊断 flags、C09 WorldEvilRuleDefinition、仙人掌水体规则/query、C10 尺寸 catalog/selection/boundary definition、C11 四个生成 pass 隔离子单元、C12 局部状态单元、terrain rule 常量、fossil nested guard、boulder transition、storm safe-spot scratch 和 TreeTops 13-area state/snapshot、以及 C13 ocean-level query，未替换任何 legacy writer，且 P16 总体 focused verification 尚未执行。当前状态为 `executionStatus: in-progress`、`implementationStatus: in-progress`、`verificationStatus: not-run`。

## 9. Implementation Checkpoint (2026-09-12)

### C09 `WorldHousingRuleAndDiagnosticState` - WorldEvilRuleDefinition

- 实际新增源码：
  - `src/WorldSession/WorldGeneration/Housing/WorldEvilRuleDefinition.cs`
- `WorldEvilRuleDefinition` 只接受 Version4 的 `WorldGenParam_Evil` 值 `-1/0/1`，并提供随机、
  腐化和猩红请求的不可变 Definition；无效值抛出 `ArgumentOutOfRangeException`。
- 该 Definition 不写入 `crimson`、不改变 `WorldGenerationRulesComponent`，不接入 CLI、WorldFile、
  随机源或生成事件，避免在 owner 未闭合时形成第二个权威 writer。
- 依赖影响：仅新增 `Terraria.WorldSession` 项目内的 `WorldGeneration.Housing` 文件；没有修改既有
  公共 API、注册键、网络协议、持久化格式或其他分区文档。
- 未完成：CLI/配置解析、生成提交 owner、`crimson` integration review 以及 P16 focused verifier。

### C09 `WorldHousingRuleAndDiagnosticState` - cactus water rule/query

- 实际新增源码：
  - `src/WorldSession/WorldGeneration/Housing/HousingWaterRuleDefinition.cs`
  - `src/WorldSession/WorldGeneration/Housing/CactusWaterGridSnapshot.cs`
  - `src/WorldSession/WorldGeneration/Housing/CactusWaterEligibilityResult.cs`
  - `src/WorldSession/WorldGeneration/Housing/CactusWaterEligibilityQuery.cs`
- `HousingWaterRuleDefinition` 固定 Version4 的 `cactusWaterWidth=50`、
  `cactusWaterHeight=25`、`cactusWaterLimit=25`，并校验非负输入。
- `CactusWaterGridSnapshot` 对液体数组做防御性复制，要求 `width * height` 精确匹配，
  只提供有界只读访问。
- `CactusWaterEligibilityQuery` 使用显式快照和规则输入，保持 `[x-width,x+width)`、
  `[y-height,y+height)` 窗口、世界边界裁剪、液体 `byte` 累加以及
  `liquidUnits / 255 > limit` 整数除法语义；`ShouldBlockGrowth` 只计算
  `result.ExceedsLimit && (!remixWorld || candidateY <= worldSurfaceY)`，不调用种植、Tile、NPC、
  网络或存档 API。
- 2026-09-11T18:41:30.3747866Z 修正了先前高度比较方向：Version4 `GrowCactus` 的原始条件是
  `(!Main.remixWorld || !(j > Main.worldSurface)) && num3 / 255 > cactusWaterLimit`，因此
  remix 世界只有候选位置高于地表时才绕过水量阻断。
- 依赖影响：仅新增 `Terraria.WorldSession` 项目内的 `WorldGeneration.Housing` 领域文件；
  没有改动既有公共 API、注册键、WorldFile、网络协议、Main 或其他分区。
- 未完成：`crimson`、`mysticLogsEvent` 及其余 P16 边界；没有 P16 focused verifier 或运行时行为
  等价证据。受影响 WorldSession 项目构建已通过；最新既有回归 verifier 在既有
  `WorldLayerMetrics` 状态保持断言处失败，不覆盖这些新类型，也不能作为 C09 通过证据。

### C10 `WorldGenerationDimensionsState` - size catalog/selection/boundary definition

- 实际新增源码：
  - `src/WorldSession/WorldGeneration/Passes/WorldSizeProfile.cs`
  - `src/WorldSession/WorldGeneration/Passes/WorldSizeCatalogDefinition.cs`
  - `src/WorldSession/WorldGeneration/Passes/WorldDimensionSelectionQuery.cs`
  - `src/WorldSession/WorldGeneration/Passes/WorldSpreadBoundaryDefinition.cs`
- `WorldSizeCatalogDefinition` 保留 Version4 三档尺寸 `4200x1200`、`6400x1800`、`8400x2400`；
  外部 `autoCreate`/CLI 使用 `1/2/3`，而内部 `WorldGen.GetWorldSize`/`SetWorldSize` 使用
  `0/1/2`，两者不能共用未标注语义的 legacy index；`WorldDimensionSelectionQuery` 只返回 profile 或映射值，
  不写 `Main.maxTilesX/maxTilesY`。
- `WorldSpreadBoundaryDefinition` 只保存已确认的 `10` 声明值；在未发现 Version4 consumer
  前不把该值隐式应用到感染或草扩散。
- 依赖影响：只新增 `WorldSession/WorldGeneration/Passes` 文件，未接入 Main、WorldFile、
  clearWorld、meteor counter 或 C03 dimension compatibility owner。
- 未完成：实际尺寸分配/边界换算、旧存档映射、spread consumer、meteor 进度交接、spawn tile/solid/
  platform/density scan、Collision/meteor effect 和运行时行为等价证据。`WorldMeteorGeometryQuery`
  已完成隔离实现和 focused verifier，但尚未接入 WorldGen 调用图。

### C10 `WorldGenerationDimensionsState` - WorldMeteorGeometryQuery

- 实际新增源码：
  - `src/WorldSession/WorldGeneration/Passes/WorldMeteorGeometryQuery.cs`
  - `src/WorldSession/WorldGeneration/Passes/WorldMeteorSpawnGeometry.cs`
- 实际修改验证器：
  - `src/WorldTerrainC12FocusedVerifier/Terraria.WorldTerrainC12FocusedVerifier.csproj`
  - `src/WorldTerrainC12FocusedVerifier/Program.cs`
- `WorldMeteorGeometryQuery` 是纯 Query，只接收显式尺寸、地表/岩层、下界、出生点和地下模式输入；
  保留普通模式 `(int)(worldSurface * 0.3)`、地下模式 `(int)(worldSurface + rockLayer) / 2` 的
  Version4 cast/division 顺序，生成水平候选范围 `[150, maxTilesX - 150)`、垂直范围
  `[verticalMinInclusive, maxTilesY)`，并以 `maxTilesX * 0.08` 与严格 `>`/`<` 表达出生点排除区。
- `WorldMeteorSpawnGeometry` 是只读 `record struct`，提供有界候选判断和排除判断；输入尺寸、有限数值、
  空范围和地下层边界均显式校验。实现不读取或修改 `Main`、Tile、Projectile、Collision、随机源或
  `meteorShowerCount`，不执行 meteor effect，也不替代 C04 progression owner。
- 依赖影响：新增两个 `Terraria.WorldSession` 领域文件，并扩展既有 focused verifier 的源码包含和边界断言；
  没有修改既有公共 API、注册键、WorldFile、网络协议、权威报告、ledger 或其他分区文档。
- focused verifier build/run 均通过，覆盖普通/地下起点、水平/垂直半开区间、出生点排除严格边界、空范围和
  非有限输入；验证 artifact 位于 `Build/bin/WorldTerrainC12FocusedVerifier/Debug/net10.0/`。
- 未完成：tile solid、平台排除、15 格密度扫描、随机坐标选择、Collision、实际 `meteor(...)` effect、
  `meteorShowerCount` 初始化/递减/快进清零及 C04 handoff；这些仍保持 pending，P16 总体
  `verificationStatus: not-run` 不变。

### C12 `WorldTerrainEffectsAndCaches` - isolated terrain rules

- 实际新增源码：
  - `src/WorldSession/WorldGeneration/Terrain/WorldTerrainRuleDefinition.cs`
- `WorldTerrainRuleDefinition` 保存 Version4 已确认的 `ItemSpawnProtectionTime=18000`、
  `catTailDistance=8`、`BUBBLES_SOLID_STATE_FOR_HOUSING=true`、`strip_w=200` 和 `strip_h=50`，
  并以 PascalCase 常量名暴露为只读 Definition。
- 这些声明目前没有已批准的运行时消费者或唯一 writer；实现不接入 `WorldGen`、Tile、住房、
  存档、网络或 strip/bit-strip effect，避免依据字段名称猜测行为。
- 依赖影响：仅新增 `Terraria.WorldSession` 项目内的 `WorldGeneration.Terrain` 文件；没有改动既有
  公共 API、注册键、WorldFile、网络协议或其他分区文档。
- 未完成：C12 的 fossil effect adapter、boulder/storm effect、queue/TreeTops/background/coating/bit-strip/rope 边界和
  P16 focused verifier；本 Definition 仍为 `verificationStatus: not-run`。

### C12 `WorldTerrainEffectsAndCaches` - WorldFossilShatterScope

- 实际新增源码：
  - `src/WorldSession/WorldGeneration/Terrain/WorldFossilShatterScope.cs`
- `WorldFossilShatterScope` 只接受已确认的 fossil tile type `404`，并把一次 fossil shatter
  尝试的递归准入和释放限制在局部 scope。未激活时只允许 fossil tile 进入；scope 已激活时拒绝
  嵌套进入；`Dispose` 释放 guard 且可重复调用。它不拥有随机选择、Tile kill、掉落、NetMessage
  或其他 terrain effect。
- 依赖影响：仅新增 `Terraria.WorldSession` 项目内的 `WorldGeneration.Terrain` 文件；没有修改
  既有公共 API、注册键、WorldFile、网络协议或其他分区文档。该类型只能由已确认的 fossil
  effect owner 在明确的 attempt 生命周期中使用。
- focused verifier：新增隔离项目
  `src/WorldTerrainC12FocusedVerifier/Terraria.WorldTerrainC12FocusedVerifier.csproj` 和
  `src/WorldTerrainC12FocusedVerifier/Program.cs`，仅编译并验证 fossil scope 的 tile gate、
  nested guard、Dispose 幂等和重新进入行为；build 与 run 均通过。
- 未完成：fossil scope 尚未接入 Tile kill、随机、掉落或网络 effect；C12 的 boulder/storm/queue/
  TreeTops/background/coating/bit-strip/rope owner 与调用图仍未闭合。

### C12 `WorldTerrainEffectsAndCaches` - WorldStormSafeSpotScratch

- 实际新增源码：
  - `src/WorldSession/WorldGeneration/Terrain/WorldStormSafeSpot.cs`
  - `src/WorldSession/WorldGeneration/Terrain/WorldStormSafeSpotScratch.cs`
  - `src/WorldTerrainC12FocusedVerifier/Terraria.WorldTerrainC12FocusedVerifier.csproj`
  - `src/WorldTerrainC12FocusedVerifier/Program.cs`
- `WorldStormSafeSpot` 使用显式 `X/Y/Width/Height` 表达一个 storm safe rectangle，并以左/上
  包含、右/下排除的边界提供纯 `Contains` 判断；使用 `long` 计算右/下边界避免整数溢出改变资格。
  `WorldStormSafeSpotScratch` 只保存当前更新周期的值，支持 `Add`、`Get` 和 `Clear`，并对读取
  下标和负宽高拒绝；没有暴露内部可变 list。
- 该边界对应 Version4 `SpawnStormLightning` 的已确认阶段：storm update 先清空列表，再从
  `active && !dead` 玩家生成安全区。玩家筛选、速度换算、随机抽样、雷击投射物、Tile 和网络
  effect 仍由未确认的外部 owner 负责，本单元不接管。
- focused verifier 覆盖当前值的包含边界、右/下排除、跨更新清空和非法宽度；build 与 run
  均通过。
- 未完成：storm safe-spot geometry adapter、lightning effect owner、boulder transition/achievement、
  queue、TreeTops、background/coating/bit-strip/rope 仍未闭合。

### C12 `WorldTerrainEffectsAndCaches` - WorldBoulderRainState

- 实际新增源码：
  - `src/WorldSession/WorldGeneration/Terrain/WorldBoulderRainState.cs`
  - `src/WorldSession/WorldGeneration/Terrain/WorldBoulderRainTransition.cs`
  - `src/WorldTerrainC12FocusedVerifier/Terraria.WorldTerrainC12FocusedVerifier.csproj`
  - `src/WorldTerrainC12FocusedVerifier/Program.cs`
- `WorldBoulderRainState.Advance` 保持 Version4 `SpawnFallingObjects` 的已确认状态公式：只有
  `drunkWorld && goodWorld && !remixWorld && storming` 才进入 raining 状态；没有 world surface
  时保持上一状态并立即返回，匹配 Version4 的 early return。transition 同时返回上一状态、当前
  状态和 `ShouldNotifyProgression` intent；只有从 raining 转为 inactive 才产生 intent。
- `Reset` 对应 Version4 `clearWorld` 的 `_isRainingBoulders = false`。该类型不调用
  `AchievementsHelper`，不抽样随机、不执行 Collision、不创建 Projectile，也不写 Tile 或网络；
  progression intent 由外部 adapter/系统消费。
- focused verifier 覆盖开始、结束、转换通知、无地表 early return 和 reset；build 与 run 均通过。
- 未完成：boulder 的随机位置、Collision、Projectile、achievement adapter 以及 storm lightning
  effect 仍未闭合；queue、TreeTops、background/coating/bit-strip/rope 仍保持 pending。

### C12 `WorldTerrainEffectsAndCaches` - WorldTreeTopsStateComponent

- 实际新增源码：
  - `src/WorldSession/WorldGeneration/Terrain/TreeTops/WorldTreeTopsStateComponent.cs`
  - `src/WorldSession/WorldGeneration/Terrain/TreeTops/WorldTreeTopsStateSnapshot.cs`
- `WorldTreeTopsStateComponent` 按 Version4 `TreeTopsInfo.AreaId.Count=13` 保存 13 个 `int`
  variation 槽位，提供显式 area id 读取/写入和越界拒绝；不猜测各区域 style 的合法值域。
- `WorldTreeTopsStateSnapshot` 复制全部 13 个槽位，并通过只读包装暴露，后续状态写入不会改变已创建
  snapshot，外部也不能通过 `IList<int>` 写回内部数组。
- 该状态单元只拥有 TreeTops 的内存状态；WorldFile `Save/Load`、旧版本 `<211` fallback、
  `NetMessage.SyncSend/SyncReceive`、随机样式选择、Tree FX 和 server authority 仍留在未实现的
  adapter/owner 边界，没有修改既有 API、注册键、持久化格式或网络路径。
- focused verifier 已覆盖 13 区域数量、区域 0/12 读写、snapshot 隔离、越界拒绝和只读暴露；
  build 与 no-build run 均通过。TreeTops adapter、格式 round-trip 和运行时行为等价仍未验证。

### C12 `WorldTerrainEffectsAndCaches` - WorldBackgroundFlashCacheProjection

- 实际新增源码：
  - `src/WorldSession/WorldGeneration/Metrics/WorldBackgroundFlashCacheProjection.cs`
- 投影按 Version4 `BackgroundChangeFlashInfo` 保存 13 个 background variation 和 13 个 flash 值。
  `UpdateCache` 只接收显式样式快照；样式变化在非 game menu 时将对应 flash 置为 `1f`，菜单中
  不启动 flash；`UpdateFlashValues` 每次按 `0.05f` 衰减并 clamp 到 `[0,1]`。
- `GetVariation`/`GetFlashPower` 是只读派生访问，输入数量和 area id 显式校验；类型不引用
  `WorldGen`、TreeTops、XNA 或渲染 API，不成为背景样式权威 writer。
- focused verifier 已覆盖样式失效、菜单条件、tick 衰减、零值 clamp、13-area 及错误边界；
  build 与 no-build run 均通过。Version4 `WorldGen.setBG`/`Main` 调度 adapter 和实际渲染仍未接线。

### C13 `WorldGenDerivedProperties` - ocean-level query

- 实际新增源码：
  - `src/WorldSession/WorldGeneration/Metrics/WorldOceanLevelQuery.cs`
- `WorldOceanLevelQuery` 接受显式的 `worldSurface` 与 `rockLayer`，拒绝非有限输入，并返回
  `(worldSurface + rockLayer) / 2.0 + 40.0`。实现保持 Version4 `oceanLevel` getter 的
  double 精度，不缓存、不写回，也不接管 `worldSurface`/`rockLayer` 的生命周期或持久化。
- 依赖影响：仅新增 `Terraria.WorldSession` 项目内的 `WorldGeneration.Metrics` 文件；
  没有接入 `oceanDepths`、Main、WorldFile、网络、注册键或其他分区文档。
- 未完成：`TransformingWorldQuery` 与 `WorldGenerationRandomSourceAdapter` 仍因变换调用/取消、
  `Main.rand` 唯一 owner、跨 pass draw order 和 focused verifier 证据不足而阻塞；本查询已随
  WorldSession 项目构建通过，但尚无 P16 focused verifier 或运行时行为等价证据。

### C12 `WorldTerrainEffectsAndCaches` - WorldTerrainDestructionQueueComponent

- 实际新增源码：
  - `src/WorldSession/WorldGeneration/Terrain/WorldTerrainDestructionQueueComponent.cs`
- `WorldTerrainDestructionQueueComponent` owns the in-memory ordered `TilePosition` queue corresponding
  to Version4 `ExploitDestroyQueue`. It exposes `Count`, `IsEmpty`, `Enqueue`, `TryDequeue`, and `Clear`;
  duplicate positions remain ordered and are not silently deduplicated.
- The component does not expose XNA `Point`, call `TileFrame`, send network messages, inspect
  `destroyObject`, or infer a producer. Those effects remain at the unresolved terrain queue owner seam.
- Dependency impact: only the `Terraria.WorldSession` project source boundary is extended; no legacy
  writer, queue producer, frame/network consumer, persistence format, registration key, or other
  partition source was changed. The existing `TilePosition` value type is reused.
- Evidence gap: Version4 confirms the dequeue/flush consumer but the enqueue writer, duplicate policy,
  failure handling, and lifecycle window are still not closed. The component is therefore
  `implemented-isolated`, not a completed runtime migration.
- Verification: not run for this unit; no test, verifier, System, Command, Query, Adapter, or Projection
  was added or modified. P16 overall `verificationStatus` remains `not-run`.

### C12 `WorldTerrainEffectsAndCaches` - WorldTerrainCoatingStateComponent

- 实际新增源码：
  - `src/WorldSession/WorldGeneration/Terrain/WorldTerrainCoatingStateComponent.cs`
- `WorldTerrainCoatingStateComponent` owns the current coating-color state using the existing
  `Terraria.Content.ColorRgba` value type. `Replace` copies every input value into private storage;
  `Colors` exposes an immutable read-only wrapper and `Clear` resets the state to an empty list.
- The component does not inspect tiles, decide block/wall coating eligibility, convert to XNA `Color`,
  or execute paint/frame/effect behavior. Those responsibilities remain at the unresolved coating
  adapter/owner boundary.
- Dependency impact: only the `Terraria.WorldSession` source boundary is extended; no legacy writer,
  XNA adapter, network or persistence format, registration key, System, Query, Command, Projection,
  test, or other partition source was changed.
- Evidence gap: Version4 confirms the reusable `_coatingColors` scratch list and its two observed color
  outputs, but the complete tile predicate, borrow lifetime, effect consumer and adapter ownership are
  not closed. The component is `implemented-isolated`; P16 overall `verificationStatus` remains
  `not-run`.

### C03 `WorldLifecycleLoadAndTransformState` - isolated state components

- 实际新增源码：
  - `src/WorldSession/WorldGeneration/WorldLoadLifecycleComponent.cs`
  - `src/WorldSession/WorldGeneration/WorldDimensionCompatibilityState.cs`
  - `src/WorldSession/WorldGeneration/WorldTransformTransactionComponent.cs`
- `WorldLoadLifecycleComponent` 保存 Version4 已确认的
  `IsGeneratingOrLoadingWorld`、`LoadFailed`、`WorldCleared` 和 `WorldBackup` 四个事实；方法只更新
  本地字段，不执行 WorldFile I/O、备份复制/删除、清理协调或跨线程发布。
- `WorldDimensionCompatibilityState` 保存上一轮 tile allocation footprint 的
  `LastMaxTilesX`/`LastMaxTilesY`，显式保留零初始化的“无旧尺寸”状态，并提供 capture/clear；它
  不替代当前 world bounds，也没有未经证实的持久化语义。
- `WorldTransformTransactionComponent` 保存非负嵌套 `ActiveCount`、单调 `Revision` 和派生的
  `IsTransforming`。`Begin`/`End` 只执行本地计数配对校验；scheduler、取消、I/O lock、
  `Interlocked` 发布和主线程 follow-up 仍属于外部 owner。
- 依赖影响：仅新增三个 `Terraria.WorldSession` 组件文件；没有修改 System、Query、Command、
  Adapter、Projection、测试、项目文件、legacy writer、持久化格式、网络路径或其他分区源码。
- 这些隔离单元的 `implementationStatus` 为 `implemented-isolated`。WorldFile recovery、clear
  coordinator、volatile guard compatibility、TransformingWorld query integration 和运行时行为等价
  仍未完成；未新增 focused verifier，因此 P16 `verificationStatus` 仍为 `not-run`。

### C04 `WorldLifecycleProgressionAndEventState` - committed progression facts

- 实际新增源码：
  - `src/WorldSession/WorldProgression/WorldProgressionEventStateComponent.cs`
- `WorldProgressionEventStateComponent` 保存 Version4 已确认的跨 tick 世界进度事实
  `ShadowOrbSmashed`、`ShadowOrbCount` 和 `AltarCount`；`Replace` 拒绝负计数，`Reset` 清除一个
  世界的本地进度状态。
- 该组件不复制 `Calendar.PendingWorldEventStateComponent` 中的 spawn intent，不实现暗影珠阈值
  事件、不写 NPC/achievement、不执行 WorldFile Save/Load、网络 projection 或 effect ack；这些
  仍属于 C04 的外部 owner 和 integration review。
- 依赖影响：仅新增 `Terraria.WorldSession` 内的 progression component 文件；没有修改已有
  Calendar 组件、System、Query、Command、Adapter、Projection、测试、项目文件或持久化/网络格式。
- 隔离单元的 `implementationStatus` 为 `implemented-isolated`；事件命令、并发快照、持久化/网络
  round-trip 和 effect failure policy 未闭合，P16 `verificationStatus` 仍为 `not-run`。

### C05 `WorldLifecycleHousingAndSpawnPacingState` - diagnostics, pacing and scoped state

- 实际新增源码：
  - `src/WorldSession/WorldGeneration/WorldHousingBuildDiagnosticState.cs`
  - `src/WorldSession/WorldGeneration/WorldNpcSpawnPacingState.cs`
  - `src/WorldSession/WorldGeneration/WorldDropSuppressionContext.cs`
  - `src/WorldSession/WorldGeneration/WorldObjectDestructionContext.cs`
- `WorldHousingBuildDiagnosticState` 保存缺少家具/光照的累计诊断事实，并可显式 `Reset`；不写
  住房关系或诊断 projection。
- `WorldNpcSpawnPacingState` 保存 `NpcSpawnDelay`/`NpcSpawnPeriod`，拒绝负值并支持显式替换/清理；
  不推进时间、不生成 NPC，也不决定 invasion/eclipse 阻断。
- `WorldDropSuppressionContext` 以嵌套深度表达局部 `stopDrops` 作用域，`WorldObjectDestructionContext`
  以单活动 guard 表达 `destroyObject` 的递归准入；两者都只执行本地配对和 reset，不执行掉落、
  Tile、物体破坏、frame、网络或日志副作用。
- 现有 `src/WorldSession/WorldProgression/WorldInfectionPolicyStateComponent.cs` 是
  `AllowedToSpreadInfections` 的近义状态类型；本轮不创建第二个同义组件，感染策略 owner 复用、
  creative power 输入和 UpdateWorld phase 仍待 integration review。
- 依赖影响：仅新增四个 `Terraria.WorldSession` 组件/上下文文件；没有修改现有感染策略类型、
  System、Query、Command、Adapter、Projection、测试、项目文件或其他分区源码。
- 隔离单元的 `implementationStatus` 为 `implemented-isolated`；UpdateWorld 调度、NPC/effect
  adapter、诊断 projection、异常/取消调用图和运行时 parity 仍未闭合，P16 `verificationStatus`
  保持 `not-run`。

### C07 `WorldHousingCountersAndScoringState` - HousingScanBudgetDefinition

- 实际新增源码：`src/WorldSession/WorldGeneration/Housing/HousingScanBudgetDefinition.cs`。
- `HousingScanBudgetDefinition` 保存 `MaxTileCount` 与 `MaxWallOut2` 两个扫描预算输入，拒绝零值和负值；
  不读取 Tile、修改计数器、写入 NPC/TownManager 或参与递归扫描。
- 依赖影响：仅新增 `Terraria.WorldSession` 的住房领域 Definition；未修改 System、Query、Command、
  Adapter、Projection、测试、项目文件或旧 `HousingScanStateComponent` 聚合类型。扫描调用图、
  `maxWallOut2` 唯一 writer 和运行时行为仍未闭合；该隔离单元为 `implemented-isolated`，P16
  `verificationStatus` 保持 `not-run`。

### C07 `WorldHousingCountersAndScoringState` - HousingScanVisitedTilesContext

- 实际新增源码：`src/WorldSession/WorldGeneration/Housing/HousingScanVisitedTilesContext.cs`。
- `HousingScanVisitedTilesContext` 只拥有一次 `countTiles`/`nextCount` 尝试的 `TilePosition` 去重集合、
  `NumTileCount` 预算计数和 `LavaCount`、`IceCount`、`SandCount`、`RockCount`、`ShroomCount` 五类
  材料计数；`Clear` 会丢弃本次扫描状态，`TryAdd` 拒绝预算耗尽或重复位置。
- 它接收不可变 `HousingScanBudgetDefinition`，不读取 Tile/wall/liquid，不执行递归，不修改
  `WorldGen`/NPC/TownManager，也不把集合暴露给外部。`countDirtTiles` 的独立 context、异常/取消
  生命周期和 snapshot close 仍待 owner 核对。
- 依赖影响：仅新增住房领域组件上下文文件；未修改已有聚合组件、System、Query、Command、Adapter、
  Projection、测试、项目文件或其他分区源码。该单元为 `implemented-isolated`，P16
  `verificationStatus` 保持 `not-run`。

### C07 `WorldHousingCountersAndScoringState` - HousingMaterialCounterSnapshot

- 实际新增源码：`src/WorldSession/WorldGeneration/Housing/HousingMaterialCounterSnapshot.cs`。
- `HousingMaterialCounterSnapshot` 是一次住房扫描结束后的不可变五类材质计数快照，保存
  `LavaCount`、`IceCount`、`SandCount`、`RockCount` 和 `ShroomCount`，拒绝负数输入。
  它不复用或清理下一次扫描上下文，也不读取 Tile、写入 WorldGen、NPC、TownManager、存档或网络。
- 依赖影响：仅新增住房领域的不可变快照类型；未修改 `HousingScanVisitedTilesContext`、旧
  `HousingScanStateComponent` 聚合类型、System、Query、Command、Adapter、Projection、测试、项目文件或
  其他分区源码。扫描结束时的 snapshot builder、计数顺序、异常/取消关闭和生成 pass 消费者仍待 owner 核对。
  该单元为 `implemented-isolated`，P16 `verificationStatus` 保持 `not-run`。

### C07 `WorldHousingCountersAndScoringState` - HousingRoomVisitedTilesContext

- 实际新增源码：`src/WorldSession/WorldGeneration/Housing/HousingRoomVisitedTilesContext.cs`。
- `HousingRoomVisitedTilesContext` 保存一次房间检查的 `MaxRoomTiles`、`MaxRoomSize`、去重
  `TilePosition` 集合、`NumRoomTiles` 以及 `RoomX1`/`RoomX2`/`RoomY1`/`RoomY2` 边界。`TryAdd`
  拒绝重复点、达到 tile 上限或使当前边界跨度达到房间尺寸上限；`Clear` 清理访问集合、计数并恢复起始点边界。
- 它不读取 Tile/wall、不执行 flood fill、不发布 feedback、不判断家具资格、不修改 NPC/TownManager，
  也不暴露可变集合。异常/取消路径的 finally 生命周期、open gate/solid wall/too-small/too-big 优先级和
  Version4 `BitSet2D` 精确映射仍待 `HousingRoomSearchSystem` owner 核对。该单元为
  `implemented-isolated`，P16 `verificationStatus` 保持 `not-run`。

### C07 `WorldHousingCountersAndScoringState` - HousingRoomScoreSnapshot

- 实际新增源码：`src/WorldSession/WorldGeneration/Housing/HousingRoomScoreSnapshot.cs`。
- `HousingRoomScoreSnapshot` 是不可变房间评分结果，保存 `NumRoomTiles`、允许 `hiScore = -1`
  占用短路的 `HighScore`、成对的可选 `BestX`/`BestY` 候选坐标以及可选 `SharedRoomX` 结果；单边候选坐标会被拒绝。
- 它不执行评分、occupancy 查询、feedback、NPC home 写入或 TownManager/network/progression effect，
  也不把 score scratch 写入存档/网络。唯一评分 writer、污染重罚、tie 不覆盖和候选坐标提交仍待 owner
  核对。该单元为 `implemented-isolated`，P16 `verificationStatus` 保持 `not-run`。

### C07 `WorldHousingCountersAndScoringState` - HousingAssignmentPriorityState

- 实际新增源码：`src/WorldSession/WorldGeneration/Housing/HousingAssignmentPriorityState.cs`。
- `HousingAssignmentPriorityState` 只保存当前住房决策窗口的可选 `PrioritizedTownNpcType`；设置时拒绝负值，
  `Clear` 表示没有优先级。它不拥有 NPC 实体、TownManager key、住房关系或网络/进度副作用。
- 依赖影响：仅新增住房领域状态类型；没有修改现有 `TownHousingResidentKey`、
  `TownHousingAssignmentComponent`、`WorldEcologyScheduleState` 或 C08 的关系提交边界。NPC 类型语义、
  key mode 映射、assignment command 和 owner 仍为 `crossSubsystemOwner: integration-review`。该单元为
  `implemented-isolated`，P16 `verificationStatus` 保持 `not-run`。

### C08 `WorldHousingRoomSearchState` - HousingRoomTileClassificationContext

- 实际新增源码：`src/WorldSession/WorldGeneration/Housing/HousingRoomTileClassificationContext.cs`。
- `HousingRoomTileClassificationContext` 拥有 `houseTile` 风格的 tile type 分类集合，使用固定大小的布尔数组去重，
  并维护 `TileTypeCount` 与 `ClassifiedTileTypeCount`。`Contains` 和 `Mark` 只操作已配置范围内的 tile type；
  `Clear` 清空分类集合并将计数复位为零。
- 该组件不读取 Tile 状态、不执行房屋资格判断、不实现房间搜索或诊断投影，也不写入 NPC、TownManager、网络或持久化。
  tile type 的实际来源、扫描调用图和 `houseTile` 生命周期仍属于 C08 搜索 owner。
- 依赖影响：仅新增上述住房领域组件文件；没有修改 System、Query、Command、Adapter、Projection、测试、项目文件或
  既有住房聚合。该隔离单元状态为 `implemented-isolated`，完整房间搜索、资格查询和运行时 parity 仍未验证。

### C08 `WorldHousingRoomSearchState` - HousingRoomSearchContext

- 实际新增源码：`src/WorldSession/WorldGeneration/Housing/HousingRoomSearchContext.cs`。
- `HousingRoomSearchContext` 组织一次房间检查尝试的短生命周期状态：它创建并持有
  `HousingRoomVisitedTilesContext`、`HousingRoomTileClassificationContext` 和 `HousingRoomDiagnosticFlags`，并保存
  `LastFoundHouse`、`IsTryingAlternateHousingSpot`、`SharedRoomX` 以及使用 `TilePosition` 的递归检查栈。
- `PushRoomCheck`/`TryPopRoomCheck` 只提供栈状态访问，`Clear` 清理栈、候选字段和三个专责子上下文；该类型不读取 Tile、
  执行 flood-fill、计算 RoomNeeds、提交评分/住房关系或发布 diagnostics/network/persistence effects。
- 依赖影响：仅新增上述住房领域上下文文件；没有修改 System、Query、Command、Adapter、Projection、测试、项目文件或
  其他分区源码。该隔离单元状态为 `implemented-isolated`，搜索 owner、失败原因 projection、alternate-spot scope 和
  运行时 parity 仍未验证。

### C08 `WorldHousingRoomSearchState` - HousingRoomRequirementResult

- 实际新增源码：`src/WorldSession/WorldGeneration/Housing/HousingRoomRequirementResult.cs`。
- `HousingRoomRequirementResult` 是每次房间资格判断的不可变输出，按 Version4 字段顺序保存
  `HasTorch`、`HasDoor`、`HasChair`、`HasTable` 和 `CanSpawn` 五个布尔结果；`false` 在该结果中表示对应资格未满足，
  不表示尚未扫描。
- 该结果类型不读取 `houseTile`、不执行 `RoomNeeds` 规则计算、不写回 legacy flags，也不产生 NPC、TownManager、网络或
  持久化副作用。资格 registry、查询算法和旧 facade 兼容写回仍属于待实现的 C08 Query/System 边界。
- 依赖影响：仅新增上述住房领域结果文件；没有修改 System、Query、Command、Adapter、Projection、测试、项目文件或
  其他分区源码。该隔离单元状态为 `implemented-isolated`，P16 `verificationStatus` 保持 `not-run`。

### C08 `WorldHousingRoomSearchState` - HousingRoomDiagnosticFlags

- 实际新增源码：`src/WorldSession/WorldGeneration/Housing/HousingRoomDiagnosticFlags.cs`。
- `HousingRoomDiagnosticFlags` 拥有 `HasStinkbug` 与 `HasEchoStinkbug` 两个房间诊断标记；
  `MarkStinkbug`/`MarkEchoStinkbug` 只置位对应标记，`Replace` 原子替换两个值，`Clear` 将两个值复位为 `false`。
- 该组件不发出诊断 projection、不影响刷怪、不读取 NPC/TownManager 状态，也不承担房间搜索、feedback、网络或持久化副作用。
  两个标记的生成条件、消费方和失败原因投影仍待 C08 owner 与外部 adapter 核对。
- 依赖影响：仅新增上述住房领域组件文件；没有修改 System、Query、Command、Adapter、Projection、测试、项目文件或
  其他分区源码。该隔离单元状态为 `implemented-isolated`，P16 `verificationStatus` 保持 `not-run`。

### Verification checkpoint - isolated P16 units

- Affected project build:
  `pwsh -NoProfile -Command "& '.\Build\Tools\Invoke-SerialDotnet.ps1' -DotnetArguments @('build', '.\src\WorldSession\Terraria.WorldSession.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"`
  latest run completed with exit code `0`, `0` warnings, and `0` errors. Artifact:
  `Build/bin/Terraria.WorldSession/Debug/net10.0/Terraria.WorldSession.dll`.
- P16 fossil focused verifier build:
  `pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 build .\src\WorldTerrainC12FocusedVerifier\Terraria.WorldTerrainC12FocusedVerifier.csproj -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false`
  completed with exit code `0`, `0` warnings, and `0` errors. Artifact:
  `Build/bin/WorldTerrainC12FocusedVerifier/Debug/net10.0/WorldTerrainC12FocusedVerifier.dll`.
- P16 fossil focused verifier run:
  `pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 run --project .\src\WorldTerrainC12FocusedVerifier\Terraria.WorldTerrainC12FocusedVerifier.csproj --no-build --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false`
  completed with exit code `0` and printed `WorldFossilShatterScope C12 focused verifier passed.`
- P16 storm safe-spot focused verifier uses the same isolated project. Its build completed with exit
  code `0`, `0` warnings, and `0` errors; artifact:
  `Build/bin/WorldTerrainC12FocusedVerifier/Debug/net10.0/WorldTerrainC12FocusedVerifier.dll`.
  The no-build run completed with exit code `0` and printed
  `WorldStormSafeSpotScratch C12 focused verifier passed.`
- Existing regression verifier:
  `pwsh -NoProfile -Command "& '.\Build\Tools\Invoke-SerialDotnet.ps1' -DotnetArguments @('run', '--project', '.\Test\Terraria.WorldSession.WorldGeneration.Verification\Terraria.WorldSession.WorldGeneration.Verification.csproj', '--no-build', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"`
  latest attempt ran without compiler output for approximately three minutes while its verifier
  process remained CPU-active, so the session owner interrupted it; the attempt ended with exit code
  `1` and did not produce a verifier diagnostic. A prior recorded attempt failed at
  `Test/Terraria.WorldSession.WorldGeneration.Verification/Program.cs:380` on the pre-existing
  `WorldLayerMetrics` state-preservation assertion. This verifier does not cover the new P16 types and
  does not close the P16 full runtime, persistence/network, or behavior-equivalence verification gate.

## 10. Terminal Checkpoint (2026-09-12)

- 当前有效会话为 `e9e33a27331a40d3a159985a54bbf8eb`；旧文档中的
  `1b3b5585d92d44b4ab883e4ad66332dd` 仅是历史记录，不再用于 runner settlement。
- C12 组件边界复核完成。`WorldTerrainTileOverrideScope`、`WorldHardmodeTilePolicyState`、
  `WorldGrassSpreadState`、`CrimsonHeartPlacementScratch`、`WorldTerrainRuleDefinition`、
  `WorldFossilShatterScope`、`WorldBoulderRainState`、`WorldStormSafeSpotScratch`、
  `WorldTreeTopsStateComponent`、`WorldBackgroundFlashCacheProjection`、
  `WorldTerrainDestructionQueueComponent` 和 `WorldTerrainCoatingStateComponent` 均已在
  `completedComponents` 中以独立边界记录。
- 对 `src/WorldSession` 和现有 verifier 的源码检索没有发现 `bitStrip` 或
  `_preventInfiniteRopeFraming` 的已确认 consumer；队列检索只发现隔离组件自身的 `Enqueue`，
  没有外部 producer。因而没有新增重复组件、巨型聚合组件或猜测性的 frame/queue owner。
- 本检查点没有新增或修改 `src/` 源码；剩余 C12 项目属于 producer/consumer、effect owner、
  WorldFile/NetMessage adapter、XNA adapter 或运行时权限边界，超出本轮 component-only 安全范围。
- 验证结果：
  - `pwsh -NoProfile -Command "& '.\Build\Tools\Invoke-SerialDotnet.ps1' -DotnetArguments @('build', '.\src\WorldSession\Terraria.WorldSession.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"`：退出码 `0`，`0` warnings，`0` errors；产物为 `Build/bin/Terraria.WorldSession/Debug/net10.0/Terraria.WorldSession.dll`。
  - `pwsh -NoProfile -Command "& '.\Build\Tools\Invoke-SerialDotnet.ps1' -DotnetArguments @('build', '.\src\WorldTerrainC12FocusedVerifier\Terraria.WorldTerrainC12FocusedVerifier.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"`：退出码 `0`，`0` warnings，`0` errors；产物为 `Build/bin/Terraria.WorldTerrainC12FocusedVerifier/Debug/net10.0/Terraria.WorldTerrainC12FocusedVerifier.dll`。
  - verifier 使用 `run --project ... --no-build --no-restore` 串行执行，退出码 `0`；输出通过了 fossil scope、boulder state、TreeTops state、background flash cache、storm safe-spot scratch 和 meteor geometry 场景。
- 这些是局部隔离验证证据，不代表完整 P16 运行时迁移、存档/网络兼容或行为等价已完成。因此本分区终态为
  `executionStatus: failed`、`implementationStatus: partial`、`verificationStatus: partially-verified`；
  失败原因是剩余任务需要尚未裁决的外部 owner/adapter/system/query/projection 和运行时调用图。
