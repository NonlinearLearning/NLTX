# Version4 非权威组件第二轮设计：P03 世界生成与地牢

partitionId: P03
sessionId: d1b8f2e5871c4032bc2b7966c88ae6c2
inputReport: D:\TRbackup\NLTX\docs\migration\ledgers\non-authoritative-component-partitions\03-world-generation-dungeons.md
componentDesignPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\non-authoritative\2026-09-11-version4-non-authoritative-P03-world-generation-dungeons-component-design.md
componentExecutionPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\non-authoritative\2026-09-11-version4-non-authoritative-P03-world-generation-dungeons-component-execution.md
previousSessionId: 056dd2e74c404b588e6bb025e3d8850c
handoffId: P03-session-rebind-20260912-0808
handoffStatus: handed-off
designStatus: proposed
executionStatus: planned
implementationStatus: in-progress
verificationStatus: partial
completedComponents: [C01, C02, C03, C04, C05, C06, C07, C08, C09, C10, C11, C12, C13, C14, C16]
currentComponent: C15
pendingComponents: [C15, C17, C18]
lastCheckpointUtc: 2026-09-12T09:25:30.9974588Z
evidence-gap:
- P03 第一轮 public-decomposition outputReport 不存在，因此本文件不能把第一轮结论当作事实；本轮使用输入库存、Version4 直接源码、完整参考源码的对应文件和公开 API 交叉证据重新建立边界。
- 输入库存的 485 条成员中，13 条行号落在属性或索引器的装饰/声明邻近行：1423、1424、1425、1429、1825、3738-3744、3747。成员名称和文件均存在，已标记为 version-drift，不把库存行号视为已闭合语义证据。
- Version4 的若干 Dungeon、Biome 和 WorldGen 方法是空实现或简化实现；完整参考源码只能补充同路径同类型的调用闭包，不能证明当前 NLTX 已有实现。
- Tile/WorldStorage 的提交事务、WorldGen 随机流的持久化、网络/存档格式、SceneMetrics 的最终 owner、NPC 生成 owner 和跨分区调度仍未闭合。
- 旧记录中 C01、C02 以及 C05-C14 的 verifier 成功输出不能作为当前工作树的有效证据；当前 `WorldGenerationVerification` 项目仍因缺失的 Commands、Queries 和 Systems 命名空间无法编译。
- C03 的两个 Component 已保存到 `src2/WorldGeneration/Biomes`；结构放置的 Query、System、Command、Tile 和 StructureMap 提交仍未实现或验证。
- C04 的三个 Component 已保存到 `src2/WorldGeneration/Terrain`；WorldGenRange 的世界缩放、pass 调度、terrain/ecology snapshot 和 Tile 提交仍未闭合。
- C16 的三个 Component 与 `ExtraSpawnType` 已保存到 `src2/WorldGeneration/Spawn`；spawn 计算、BinaryReader/BinaryWriter、网络/存档语义和 NPC owner 仍未闭合。
- 受影响的 `src2/WorldGeneration/Terraria.WorldGeneration.csproj` 已通过仓库串行包装器构建，exitCode 0、0 warnings、0 errors，产物为 `D:\TRbackup\NLTX\Build\bin\Terraria.WorldGeneration\Debug\net10.0\Terraria.WorldGeneration.dll`。
- 既有 `src2/WorldGenerationVerification/Terraria.WorldGenerationVerification.csproj` 的串行构建 exitCode 1、0 warnings、3 errors：`Program.cs(3,32)` 缺失 `Terraria.WorldGeneration.Commands`，`Program.cs(16,32)` 缺失 `Terraria.WorldGeneration.Queries`，`Program.cs(17,32)` 缺失 `Terraria.WorldGeneration.Systems`；验证器和其依赖未在本轮修改。
blocking-decision:
- 需要 integration-review 决定世界生成聚合实体、DungeonData/GenVars 的唯一写入 owner，以及 TileMap/WorldStorage 的事务提交和回滚协议。
- 需要 integration-review 决定 WorldGenerationProgress、SceneBiomeZone、ExtraSpawn 和 NPCSpawnParams 与其他分区的共享接口、快照版本和最终调度顺序。

implementationProgress:
- status: in-progress
- completedImplementationComponents: [C01, C02, C03, C04, C05, C06, C07, C08, C09, C10, C11, C12, C13, C14, C16]
- currentImplementationUnit: C15 orchestration and commit boundary (deferred)
- modifiedSourceRoot: D:\TRbackup\NLTX\src2
- modifiedFiles:
  - src2/WorldGeneration/Biomes/BiomeStructurePlacementDefinition.cs
  - src2/WorldGeneration/Biomes/CaveHouseBudgetState.cs
  - src2/WorldGeneration/Terrain/TerrainPassWorkState.cs
  - src2/WorldGeneration/Terrain/MagmaFlowWorkState.cs
  - src2/WorldGeneration/Terrain/DunesPlacementDefinition.cs
  - src2/WorldGeneration/Spawn/ExtraSpawnType.cs
  - src2/WorldGeneration/Spawn/ExtraSpawnPolicyDefinition.cs
  - src2/WorldGeneration/Spawn/ExtraSpawnPointState.cs
  - src2/WorldGeneration/Spawn/NpcSpawnOverrideValue.cs
- checkpoint:
  - component: C04
    status: implemented
    files:
      - src2/WorldGeneration/Terrain/TerrainPassWorkState.cs
      - src2/WorldGeneration/Terrain/MagmaFlowWorkState.cs
      - src2/WorldGeneration/Terrain/DunesPlacementDefinition.cs
    evidence: Version4 DesertBiome, DitherSnake, DunesBiome, GraniteBiome, MarbleBiome and TerrainPass declarations were read; pass work is isolated to explicit values, circular history and double buffers.
    deferred: TerrainAndBiomePassSystem, WorldGenRange scaling policy, terrain/ecology projection and Tile effects.
- checkpoint:
  - component: C16
    status: implemented
    files:
      - src2/WorldGeneration/Spawn/ExtraSpawnType.cs
      - src2/WorldGeneration/Spawn/ExtraSpawnPolicyDefinition.cs
      - src2/WorldGeneration/Spawn/ExtraSpawnPointState.cs
      - src2/WorldGeneration/Spawn/NpcSpawnOverrideValue.cs
    evidence: Version4 ExtraSpawnSettings and NPCSpawnParams declarations were read; policy, team points/candidates and NPC overrides are separate state values with defensive snapshots.
    deferred: ExtraSpawnGenerationSystem, ExtraSpawnBinaryAdapter, IExtraSpawnTransportPort, persistence/network encoding and cross-partition spawn ownership.
- checkpoint:
  - component: C03
    status: implemented
    files:
      - src2/WorldGeneration/Biomes/BiomeStructurePlacementDefinition.cs
      - src2/WorldGeneration/Biomes/CaveHouseBudgetState.cs
    evidence: Version4 HouseBuilderContext counts, HouseUtils blacklist/beelist and CaveHouseBiome chest-chance members were read; the Component owns only immutable placement definitions and bounded budgets.
    deferred: StructurePlacementEligibilityQuery, BiomeStructureGenerationSystem, StructurePlacementCommand, Tile and StructureMap effects.
- verificationStatus: partial
- verificationEvidence:
  - command: `& .\\Build\\Tools\\Invoke-SerialDotnet.ps1 -DotnetArguments @('build', '.\\src2\\WorldGeneration\\Terraria.WorldGeneration.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')`
    exitCode: 0
    warnings: 0
    errors: 0
    artifacts:
      - D:\\TRbackup\\NLTX\\Build\\bin\\Terraria.WorldGeneration\\Debug\\net10.0\\Terraria.WorldGeneration.dll
  - command: `& .\\Build\\Tools\\Invoke-SerialDotnet.ps1 -DotnetArguments @('build', './src2/WorldGenerationVerification/Terraria.WorldGenerationVerification.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')`
    exitCode: 1
    warnings: 0
    errors: 3
    errorsObserved:
      - `Program.cs(3,32)`: missing `Terraria.WorldGeneration.Commands` namespace.
      - `Program.cs(16,32)`: missing `Terraria.WorldGeneration.Queries` namespace.
      - `Program.cs(17,32)`: missing `Terraria.WorldGeneration.Systems` namespace.
    artifact: none accepted; the verifier project was not modified.
  - The no-build run that printed `P03 world generation verification passed.` is stale because it occurred before the fresh verifier build failure; it is not used as completion evidence.
  - No verifier run is accepted after the failed verifier build; no tests, verifier, System, Query, Command, Adapter or Projection code was added in this implementation checkpoint.

## 1. 设计定位

本文件是 P03 的 proposed 组件设计，并附带当前非权威 Component 实现 checkpoint；它不是
迁移完成报告、行为等价证明、API 兼容证明或完整当前 NLTX 能力声明。P03 输入报告包含
35 个叶子子系统、456 个字段、29 个属性，共 485 条成员记录。本设计逐条保留输入报告的
source index 和成员名称，并为每条记录提出 Component、System、Query、Command、Adapter 或
Projection 归属。

本轮实现阶段只修改当前 P03 的两份文档和 `src2` 下的实现项目；没有修改 `src`、`Test`、
Version4、完整参考源码、其他分区报告、其他会话文档、ledger 或 lock。

## 2. 范围与排除

范围包括：

- Main 中与生成进度、生成宿主和世界生成菜单直接相连的 19 条状态成员；
- 通用生成辅助、反馈策略、绘画、ShapeFloodFill、TrackGenerator；
- CaveHouse、DeadMansChest、Desert、Dunes、Granite、Marble、TerrainPass 和 DungeonControlLine；
- Dungeon entrance、feature、layout provider、crawler、room、hall、bounds、style、家具、
  door/platform、trap 和 SceneMetrics biome-zone 成员；
- ExtraSpawnPointManager、ExtraSpawnSettings 和 NPCSpawnParams 的库存成员；
- 生成 pass 的确定性随机、进度报告、Tile/StructureMap 写入和失败恢复边界。

排除内容：

- P01-P02、P04-P20 的成员清单和设计，不复制其他分区 owner；
- 权威 P03 mount/vehicle 任务及其文档；
- 生产 C#、测试、csproj、生成器、协议、存档格式和任何实际迁移；
- 不在 P03 输入报告中的方法成员。方法只作为确认输入、输出、副作用和顺序的证据；
- 将静态样式目录、几何值、一次性候选点或生成进度错误提升为长期游戏实体。

## 3. 证据来源

| 来源 | 实际读取内容 | 结论与状态 |
| --- | --- | --- |
| P03 非权威库存 | D:\TRbackup\NLTX\docs\migration\ledgers\non-authoritative-component-partitions\03-world-generation-dungeons.md | 35 个叶子、485 条成员、来源路径存在；成员声明事实为 source-inventory-confirmed |
| Version4 地牢 | Terraria.GameContent.Generation.Dungeon\DungeonData.cs:12、DungeonBounds.cs:8、DungeonCrawler.cs:18、DungeonGenVars.cs:8、DungeonUtils.cs:15 | 确认 DungeonData 同时承载集合、运行时标量和 GenVars 代理；Bounds 有夹紧、hitbox 和 Reset；Crawler 负责 setup/layout/generate 生命周期 |
| Version4 地牢边界 | Dungeon\Entrances\DungeonEntrance.cs:5、Features\DungeonFeature.cs:3、Features\GlobalDungeonFeature.cs:3、Halls\DungeonHall.cs:8、Rooms\DungeonRoom.cs:9、LayoutProviders\DualDungeonLayoutProvider.cs:15 | 确认入口、feature、hall、room 和 layout provider 是不同的计算/生成阶段；不能合成单一 DungeonComponent |
| Version4 生成编排 | Terraria\WorldGen.cs:10553-10576、13840-13959、6267-6304 | 确认 Terrain/Dunes/Dungeon pass 注册、双地牢 setup、MakeDungeon 调用、生成结束保存和菜单状态切换；顺序必须由 System 契约表达 |
| Version4 生成辅助 | Terraria.GameContent.Biomes\CaveHouseBiome.cs:9、DeadMansChestBiome.cs:10、DunesBiome.cs:8、GraniteBiome.cs:12、MarbleBiome.cs:8、TerrainPass.cs:8；Generation\ShapeFloodFill.cs:6、TrackGenerator.cs:10 | 确认 biome Place 是外部结构和进度输入的副作用边界；陷阱和轨道包含一次性工作缓存；不把缓存注册为持久实体 |
| Version4 运行时边界 | Terraria\Main.cs:467-493、1262-1279、11151-11178；GameContent\ExtraSpawnPointManager.cs:9；SceneMetrics.cs:10；NPCSpawnParams.cs:3；DataStructures\IRoomCheckFeedback_Spread.cs:3 | 确认 Main/SceneMetrics/ExtraSpawn/NPCSpawn/feedback 分别连接表现、生成、网络/存档和查询边界；共享 owner 留给 integration-review |
| 完整参考源码 | D:\TRbackup\无任何删减通过编译\Terraria.GameContent.Generation.Dungeon\DungeonCrawler.cs、LayoutProviders\DualDungeonLayoutProvider.cs、Rooms\DungeonRoom.cs、Halls\DungeonHall.cs、Terraria\WorldGen.cs | 仅补充 Version4 同路径同类型的调用闭包：layout 先计算 room/hall，随后分阶段 GenerateRoom/GenerateHall；不替代 Version4 覆盖基线 |
| tModLoader stable API | D:\TRbackup\tmodloader-api-docs-stable\class_world_gen.html#ada40e37399d33f566e200ede8c59be399、class_generation_progress.html#a27c9b22cf8442d5f293559e859289019、class_gen_pass.html#aad7fbee611dd855bd33ec5ead6872718、class_scene_metrics.html#ad0dcf5f106e28e7e3042ce3826b8ab3d、struct_n_p_c_spawn_params.html#a6ec68dcb31d82a72b7d739b43dbf3629 | 页面页眉为 tModLoader v2026.07；只用于确认公开的 GenerateWorld、progress、GenPass、SceneMetrics.Reset 和 NPCSpawnParams.WithScale 边界，不能确认 Version4 私有调用顺序 |
| Space Station 14 | 对 C:\Users\shan\Downloads\ECS\space-station-14-master 的 WorldGen/Dungeon/GenerationProgress/GenPass 直接检索未得到对应证据 | 无直接对应证据；以下 Component/System/Query 粒度只由 Version4、完整参考和 NLTX 约束决定 |
| 当前 NLTX | src\WorldSession\WorldGeneration 下的既有世界生成类型，以及 src2\WorldGeneration 下本轮 Component | 既有世界生成类型仍在生产源树；本轮 C03、C04、C16 的 Component 已保存到 src2 并通过受影响 Component 项目编译，不代表 P03 已迁移 |

### 3.1 已确认的 Version4 数据流

1. WorldGen.AddPasses 注册 TerrainPass、DunesAndPyramidLocations、可选
   DualDungeonsDitherSnake 和 Dungeon pass。Dunes pass 初始化 DungeonGenVars；Dungeon pass
   逐 iteration 建立 DungeonData，之后调用 MakeDungeon，最后清理 dungeonData。
2. DungeonCrawler.MakeDungeon 读取 WorldGen.genRand，写入 GeneratingDungeon、入口位置、
   Bounds、强度和 progress；根据 Default/DualDungeon 选择 layout provider。完整参考显示
   provider 先计算 room/hall，再按 room、pre-hall feature、hall、late feature 等阶段写入
   Tile/结构。
3. DungeonData 的 dungeonRooms、dungeonHalls、dungeonFeatures、door/platform 和保护边界
   是一个生成 iteration 的工作集，不是存档实体集合。DungeonData.genVars 是通过 iteration
   访问 GenVars.dungeonGenVars 的代理，提出设计时必须避免两个写入 owner。
4. DungeonBounds 的 Left/Right/Top/Bottom 会按 Main.maxTilesX/Y 夹紧，CalculateHitbox
   保证正宽高；X/Y/Width/Height/Center 是派生读取，不应与边界权威值重复存储。
5. DitherSnake、DungeonControlLine 和 hallway calculator 共同提供空间查询与布局图生成；
   backLinks、forwardLinks、stairwells 和 controlLines 是短期布局图工作集。
6. ExtraSpawnPointManager 在 WorldGen 中收到 team-based、surface、remix、roundLandmass、
   skyblock、extraLiquid 配置，生成后还提供 BinaryReader/BinaryWriter 读写；这不是单纯
   的 Dungeon 或 NPC Component，必须通过 spawn/transport adapter 交接。
7. SceneMetrics.Scan 在 Main.UpdateSceneMetrics 中按 update count 和中心位置去重扫描，
   Reset 清理 zone、计数和 NPC 位置；P03 只提出读取投影，不能裁决 SceneMetrics 的实体 owner。

## 4. 领域边界与状态所有权

### 4.1 所有权原则

- 世界生成聚合状态的候选 owner 是一个 generation-session entity；其跨项目实体 ID、
  PersistentWorldId、WorldSectionId、TileCoordinate、NetworkId 和外部文件标识分别建模，
  不能用其中一个 ID 代替其他 ID。最终 owner 为 integration-review。
- 静态 DungeonGenerationStyleData、DungeonGenerationStyles、家具/门/平台/旗帜常量和
  progression tiers 是只读 definition/catalog；不能由生成结果反向修改。
- Room、Hall、Entrance、Feature 和 ControlLine 是生成期间的短期工作实体或工作集。
  calculated、generated、Processed 是状态机/查询结果，不是网络或存档事实。
- Geometry、bounds properties、style constants、SceneMetrics zone flags 和 generation progress
  是 Query 或 Projection 输出。Query 不写 Tile、StructureMap、random 或外部状态。
- Tile、wall、liquid、chest、wire、NPC spawn point 等结构变更必须通过 proposed Command
  进入单一 commit owner；任何 canPlaceHereCallback 都只能是受控 Query/Adapter 输入。

### 4.2 Component 组合

生成会话实体建议组合：

WorldGenerationPlanComponent
  + WorldGenerationPassStateComponent
  + WorldGenerationRandomStateComponent
  + WorldGenerationRulesComponent
  + proposed DungeonGenerationContextComponent
  + proposed TerrainPassWorkStateComponent
  + proposed BiomePlacementWorkStateComponent
  + proposed GenerationProgressProjectionState

每个 Dungeon room/hall/entrance/feature 工作实体建议分别组合：

DefinitionComponent + LifecycleComponent + Bounds/GeometryWorkState + PlacementCommandBuffer

其中 PlacementCommandBuffer 只保留当前 pass 的未提交意图；提交后清空或归档为只读
GenerationResultProjection。不得把 room、hall 或 feature 的继承树直接搬进 ECS。

## 5. Proposed checkpoint 与模块清单

所有路径、类型、接口和顺序均为 proposed。目标文件不存在不代表已创建。

| Checkpoint | Proposed 模块 | 主要角色 | 权威状态和不变量 | 读者/写者与 seam |
| --- | --- | --- | --- | --- |
| C01 | WorldGenerationHostRuntime、WorldGenerationMenuSession、WorldGenerationProgressProjection | Component、Adapter、Projection | Main 的生成宿主、菜单输入、进度和表现引用分离；graphics、Stopwatch、AmbientWindSystem 不进入核心 Component | WorldGen orchestrator 写入；UI/客户端读 Projection；IGraphicsGenerationPort、IGenerationClockPort、IProgressSink 为 seam；部分字段 crossSubsystemOwner: integration-review |
| C02 | GenerationSupportWorkState、GenerationFeedbackPort、PaintingDefinition、ShapeFloodFillWorkState、TrackGenerationWorkState | Component、Query、Command、Adapter | feedback 只报告结果；ShapeFloodFill/Track history 是 pass 生命周期缓存；PaintingEntry 是值对象 | Biome/Dungeon systems 读；Tile commit 写结构；RoomCheckFeedbackAdapter 隔离日志/UI |
| C03 | BiomeStructurePlacementDefinition、CaveHouseBudgetState、BiomeStructureGenerationSystem | Component、System、Query、Command | CaveHouse loot chance、blacklist/beelist 和 decoration counts 是定义/工作预算，不是世界结果 | WorldGen biome pass 写；StructureMap/TileWritePort 提交；随机和 progress 由 port 注入 |
| C04 | TerrainPassWorkState、MagmaFlowWorkState、DunesPlacementDefinition、TerrainAndBiomePassSystem | Component、System、Query | surface history、magma maps、normalised vectors、dunes width 在 pass 期间有效，pass 完成后只输出 terrain/ecology snapshot | Terrain/biome system 读写工作集；WorldTerrainStateComponent 接收 Projection；TileMap port 提交 |
| C05 | DungeonBoundsComponent、DungeonWallProgressionDefinition、DungeonBoundsQuery | Component、Query、Definition | Left/Right/Top/Bottom 是唯一边界写入；hitbox、X/Y/Width/Height/Center 派生；边界必须正且在 world bounds 内 | Room/Hall/Layout 查询；BoundsMutationSystem 唯一写者；WorldBounds 和 TileCoordinate 为 integration-review |
| C06 | DungeonStyleCatalog、DungeonStyleMaterialDefinition、DungeonStyleSelectionQuery | Definition、Component、Query | style、brick/wall/liquid、unbreakable tier 和 EdgeDither 只读且按版本注册；不得被生成结果覆盖 | Dungeon setup 读；Content/WorldProgression 提供候选；最终 catalog owner 为 integration-review |
| C07 | DungeonGenerationContext、DungeonGenerationCollections、DungeonGenerationScalarState、DungeonLegacyPlacementState、DungeonLegacyRuleState | Component、System、Projection | 将 DungeonData/GenVars 拆成 context、collections、scalars、legacy placement/rules；GenVars proxy 不形成第二写者 | DungeonGenerationSystem 唯一写；layout/room/hall 只读；RandomState、WorldRules、WorldDescriptor 通过查询输入 |
| C08 | DungeonEntranceDefinition、DungeonEntranceLifecycle、DungeonFeatureDefinition、DungeonFeatureLifecycle | Component、Definition、System | settings 与 calculated/generated/Bounds/OldManSpawn 分开；GlobalFeature 无 Bounds 时不能伪装成 room entity | Entrance/Feature systems 写；DungeonTileMutationCommand 提交；NPC/structure owner crossSubsystemOwner: integration-review |
| C09 | DungeonRoomDefinition、DungeonRoomLifecycle、DungeonRoomGeometryWorkState、DungeonRoomQuery | Component、Query、System | room settings、calculated/generated、Inner/OuterBounds、shape data、Positions 和 type-specific sizes 按生命周期拆开 | Layout system 创建；RoomCalculationSystem 写 geometry；RoomGenerationSystem 发 Command |
| C10 | DungeonHallDefinition、DungeonHallLifecycle、DungeonHallGeometryWorkState、DungeonHallQuery | Component、Query、System | hall settings、calculated/generated、Bounds/endpoint/direction、Legacy/Sine/Stairwell 参数分开；Processed 是派生状态 | Layout system 创建；HallCalculation/GenerationSystem 写；Tile commit 唯一落地 |
| C11 | DungeonLayoutProviderDefinition、DungeonLayoutGraphWorkState、DungeonCrawlerContext | Component、Query、System | RoomEntry/HallLine/link graph、controlLines、progress score 和 provider state 是短期布局图；不能持久化为 gameplay entity | DungeonLayoutSystem 生成；CrawlerContext 负责 iteration；最终 layout order integration-review |
| C12 | DungeonDoorPlacementRequest、DungeonPlatformPlacementRequest、DungeonTrapPlacementWorkState、DungeonPlacementQuery | Command、Component、Query、System | door/platform/attempt caches 只表达待提交意图；callback 通过 Query port；trap attempt 失败可清空重试 | Room/Hall/DeadMansChest 发命令；DungeonTileCommitSystem/WorldInteraction adapter 提交 |
| C13 | DungeonFurnitureCatalog、DungeonObjectStyleQuery、DungeonBannerTrapCatalog、DungeonRoomVariantCatalog | Definition、Query | item type arrays、door/pot/chandelier/platform/banner/trap constants、BiomeRoomType/SubStyles immutable；不注册为实体 | Room/Hall/Feature systems 只读；Content registry 为外部输入，owner integration-review |
| C14 | DungeonControlLineComponent、DungeonControlLineGeometryQuery | Component、Query | Start/End/tangent/normal/radius/length/progression/style 是一条 control line 的几何快照；Next/Prev 是关系，不复制对象 | DitherSnake/Layout system 写；room query 读；关系索引 owner integration-review |
| C15 | WorldGenerationOrchestrationSystem、DungeonGenerationSystem、GenerationPassCommitSystem | System、Command、Adapter | pass、layout、calculate、generate、commit、cleanup 顺序显式；失败时只提交完整 batch 或使用可验证补偿 | 读取现有 WorldGenerationPlan/Pass/Random；写 generation context；IWorldTileMutationPort、IWorldGenerationCheckpointPort |
| C16 | ExtraSpawnPolicyDefinition、ExtraSpawnPointState、ExtraSpawnGenerationSystem、ExtraSpawnBinaryAdapter、NpcSpawnOverrideValue | Component、Definition、System、Adapter、Projection | spawn policy、landmass candidates、team points、NPC override 分开；Read/Write 是协议适配，不是业务状态 | WorldGen/Player/NPC systems 读写；network/save adapter 隔离；spawn owner integration-review |
| C17 | SceneBiomeZoneQueryPort、SceneBiomeZoneSnapshotProjection、GenerationPresentationProjection | Query、Projection、Adapter | Zone flags 只作为带 revision/center/tick 的快照；不可由 P03 直接写 SceneMetrics 或反向驱动生成规则 | SceneMetrics system 产出；NPC/Player/UI 读取；最终 owner integration-review |
| C18 | Focused verifier、coverage checker、migration checkpoint ledger | 验证计划 | 完整验证仍 deferred；本轮仅记录受影响 Component 项目构建成功和既有 verifier 项目构建失败 | 当前 verificationStatus 为 partial；完整成员覆盖、确定性、事务和回滚检查仍未闭合 |

## 6. 成员逐条 proposed 归属矩阵

以下矩阵逐条列出输入报告的 source index、声明类型和成员名称。每一项都已有 proposed
目标 checkpoint；它不是第一轮报告的替代品，也不是已实现文件列表。Source index 是输入
报告中的稳定审计标识，不承担运行时顺序含义。

### C01 - Host and menu runtime

- MainGraphicsAndGenerationState -> C01: 153 Main.OurFavoriteColor; 154 Main.mapEnabled; 155 Main.IsEnginePreloaded; 156 Main._gameUpdateCount; 157 Main.SkipAssemblyLoad; 158 Main.renderCount; 159 Main.graphics; 160 Main.AutogenProgress; 161 Main.saveTime; 162 Main.shimmerAlpha; 163 Main.shimmerDarken; 164 Main.afterPartyOfDoom
- MainMenuAndWorldGenerationState -> C01: 497 Main.ChumBucketProjectileHelper; 498 Main.maxMenuItems; 499 Main.menuItemScale; 500 Main.menuMode; 501 Main.newWorldName; 502 Main._ambientWindSys; 503 Main.autoPass

### C02 - Generation support

- SharedWorldGenerationSupport -> C02: 1194 NoRoomCheckFeedback.WithText; 1195 NoRoomCheckFeedback.WithoutText; 2011 PaintingEntry.tileType; 2012 PaintingEntry.style; 2013 ShapeFloodFill._maximumActions; 2014 TrackHistory.X; 2015 TrackHistory.Y; 2016 TrackHistory.Slope; 2017 TrackHistory.Mode; 2018 TrackGenerator._history; 2019 TrackGenerator._rewriteHistory; 3691 IRoomCheckFeedback_Spread.StopOnFail; 3692 IRoomCheckFeedback_Spread.DisplayText; 3693 NoRoomCheckFeedback.StopOnFail; 3694 NoRoomCheckFeedback.DisplayText

### C03 - Cave house and structures

- SharedBiomeCaveHouseAndStructureState -> C03: 1399 HouseBuilderContext.SharpenerCount; 1400 HouseBuilderContext.ExtractinatorCount; 1401 HouseUtils.BlacklistedTiles; 1402 HouseUtils.BeelistedTiles; 1403 CaveHouseBiome._builderContext; 3738 CaveHouseBiome.IceChestChance; 3739 CaveHouseBiome.JungleChestChance; 3740 CaveHouseBiome.GoldChestChance; 3741 CaveHouseBiome.GraniteChestChance; 3742 CaveHouseBiome.MarbleChestChance; 3743 CaveHouseBiome.MushroomChestChance; 3744 CaveHouseBiome.DesertChestChance

### C04 - Terrain and biome pass work

- SharedBiomeTerrainPassState -> C04: 1426 DesertBiome.ChanceOfEntrance; 1427 DitherSnake.CircleTestPoints; 1428 DitherSnake.ExtraBuffer; 1429 DunesBiome._singleDunesWidth; 1450 Magma.Pressure; 1451 Magma.Resistance; 1452 Magma.IsActive; 1453 GraniteBiome.MAX_MAGMA_ITERATIONS; 1454 GraniteBiome._sourceMagmaMap; 1455 GraniteBiome._targetMagmaMap; 1456 GraniteBiome._normalisedVectors; 1457 MarbleBiome.SCALE; 1458 SurfaceHistory._heights; 1459 SurfaceHistory._index; 3745 DunesBiome.MaximumWidth; 3747 SurfaceHistory.this[]

### C05 - Bounds and progression

- SharedDungeonBoundsAndProgressionDefinitions -> C05: 1819 DualDungeonUnbreakableWallTiers.EarlyGame; 1820 DualDungeonUnbreakableWallTiers.EvilBoss; 1821 DualDungeonUnbreakableWallTiers.JungleBoss; 1822 DualDungeonUnbreakableWallTiers.Dungeon; 1823 DualDungeonUnbreakableWallTiers.Hallow; 1824 DualDungeonUnbreakableWallTiers.Temple; 1825 DungeonBounds._hitbox; 1826 DungeonBounds._boundsLeft; 1827 DungeonBounds._boundsRight; 1828 DungeonBounds._boundsTop; 1829 DungeonBounds._boundsBottom; 3789 DungeonBounds.X; 3790 DungeonBounds.Y; 3791 DungeonBounds.Width; 3792 DungeonBounds.Height; 3793 DungeonBounds.Left; 3794 DungeonBounds.Right; 3795 DungeonBounds.Top; 3796 DungeonBounds.Bottom; 3797 DungeonBounds.Center
- SharedDungeonGeometryRuleQueries -> C05: 2003 DungeonUtils.HALLWAY_DOOR_PLACEMENT_VARIANCE; 2004 DungeonUtils.DUNGEONHALL_DEFAULT_INNER_AREA_DEPTH; 2005 DungeonUtils.DUNGEONHALL_DEFAULT_OUTER_WALL_DEPTH; 2006 DungeonUtils.DUNGEONROOM_DEFAULT_INNER_AREA_DEPTH; 2007 DungeonUtils.DUNGEONROOM_DEFAULT_OUTER_WALL_DEPTH; 2008 DungeonUtils.MOSAIC_NONE; 2009 DungeonUtils.MOSAIC_SKELETRON; 2010 DungeonUtils.MOSAIC_MOONLORD

### C06 - Style catalog and materials

- SharedDungeonStyleMaterialAndGeometryState -> C06: 1877 DungeonGenerationStyleData.Style; 1878 DungeonGenerationStyleData.UnbreakableWallProgressionTier; 1879 DungeonGenerationStyleData.BrickTileType; 1880 DungeonGenerationStyleData.BrickGrassTileType; 1881 DungeonGenerationStyleData.BrickCrackedTileType; 1882 DungeonGenerationStyleData.BrickWallType; 1883 DungeonGenerationStyleData.WindowGlassWallType; 1884 DungeonGenerationStyleData.WindowClosedGlassWallType; 1885 DungeonGenerationStyleData.WindowEdgeWallType; 1887 DungeonGenerationStyleData.PitTrapTileType; 1888 DungeonGenerationStyleData.LiquidType; 1913 DungeonGenerationStyleData.EdgeDither
- SharedDungeonStyleSetCatalog -> C06: 1916 DungeonGenerationStyles.Shimmer; 1917 DungeonGenerationStyles.Spider; 1918 DungeonGenerationStyles.LivingWood; 1919 DungeonGenerationStyles.Cavern; 1920 DungeonGenerationStyles.Snow; 1921 DungeonGenerationStyles.Desert; 1922 DungeonGenerationStyles.Corruption; 1923 DungeonGenerationStyles.Crimson; 1924 DungeonGenerationStyles.Crystal; 1925 DungeonGenerationStyles.Hallow; 1926 DungeonGenerationStyles.GlowingMushroom; 1927 DungeonGenerationStyles.Beehive; 1928 DungeonGenerationStyles.LivingMahogany; 1929 DungeonGenerationStyles.Jungle; 1930 DungeonGenerationStyles.Temple

### C07 - Generation context and legacy state

- SharedDungeonCrawlerRuntimeState -> C07: 1830 DungeonCrawler.dungeonData; 3798 DungeonCrawler.CurrentDungeonData
- SharedDungeonGenerationCollectionsState -> C07: 1831 DungeonData.Type; 1832 DungeonData.Iteration; 1833 DungeonData.dungeonEntrance; 1834 DungeonData.dungeonRooms; 1835 DungeonData.dungeonHalls; 1836 DungeonData.dungeonFeatures; 1837 DungeonData.dungeonDoorData; 1838 DungeonData.dungeonPlatformData; 1839 DungeonData.protectedDungeonBounds; 1840 DungeonData.makeNextPitTrapFlooded; 1841 DungeonData.useSkewedDungeonEntranceHalls; 1842 DungeonData.createdDungeonEntranceOnSurface; 1843 DungeonData.dungeonEntranceStrengthX; 1844 DungeonData.dungeonEntranceStrengthY; 1845 DungeonData.dungeonEntranceStrengthX2; 1846 DungeonData.dungeonEntranceStrengthY2; 1847 DungeonData.lastDungeonHall; 1848 DungeonData.dungeonBounds; 1849 DungeonData.outerProgressionBounds; 3799 DungeonData.genVars
- SharedDungeonGenerationScalarAndStyleState -> C07: 1850 DungeonData.wallVariants; 1851 DungeonData.chandelierItemType; 1852 DungeonData.platformItemType; 1853 DungeonData.doorItemType; 1854 DungeonData.lanternStyles; 1855 DungeonData.shelfStyles; 1856 DungeonData.bannerStyles; 1857 DungeonData.globalFeatureScalar; 1858 DungeonData.dungeonStepScalar; 1859 DungeonData.hallStrengthScalar; 1860 DungeonData.hallStepScalar; 1861 DungeonData.hallInteriorToExteriorRatio; 1862 DungeonData.hallSlantVariantScalar; 1863 DungeonData.roomStrengthScalar; 1864 DungeonData.roomStepScalar; 1865 DungeonData.roomInteriorToExteriorRatio; 1866 DungeonData.roomSlantVariantScalar
- SharedDungeonLegacyPlacementState -> C07: 1931 DungeonGenVars.dungeonSide; 1932 DungeonGenVars.dungeonLocation; 1933 DungeonGenVars.dungeonColor; 1934 DungeonGenVars.brickTileType; 1935 DungeonGenVars.brickWallType; 1936 DungeonGenVars.brickCrackedTileType; 1937 DungeonGenVars.windowGlassWallType; 1938 DungeonGenVars.windowClosedGlassWallType; 1939 DungeonGenVars.windowEdgeWallType; 1940 DungeonGenVars.windowPlatformItemTypes; 1941 DungeonGenVars.generatingDungeonPositionX; 1942 DungeonGenVars.generatingDungeonPositionY; 1943 DungeonGenVars.generatingDungeonTopX; 1944 DungeonGenVars.dungeonLootStyle; 1945 DungeonGenVars.outerPotentialDungeonBounds; 1946 DungeonGenVars.innerPotentialDungeonBounds; 1957 DungeonGenVars.dungeonEntrancePosition
- SharedDungeonLegacyRuleState -> C07: 1947 DungeonGenVars.dungeonStyle; 1948 DungeonGenVars.dungeonGenerationStyles; 1949 DungeonGenVars.dungeonDitherSnake; 1950 DungeonGenVars.isCrackedBrick; 1951 DungeonGenVars.isPitTrapTile; 1952 DungeonGenVars.isDungeonTile; 1953 DungeonGenVars.isDungeonWall; 1954 DungeonGenVars.isDungeonWallGlass; 1955 DungeonGenVars.GeneratingDungeon; 1956 DungeonGenVars.preGenDungeonEntranceSettings; 1958 DungeonGenVars.desertChestLootState
- SharedDungeonLayoutProviderState -> C07/C11: 1959 DungeonLayoutProvider.settings; 1960 DungeonLayoutProviderSettings.StyleData

### C08 - Entrance and feature lifecycle

- SharedDungeonEntranceDefinitions -> C08: 1675 DungeonEntrance.settings; 1676 DungeonEntrance.calculated; 1677 DungeonEntrance.generated; 1678 DungeonEntrance.Bounds; 1679 DungeonEntrance.OldManSpawn; 1680 DungeonEntranceSettings.EntranceType; 1681 DungeonEntranceSettings.RandomSeed; 1682 DungeonEntranceSettings.StyleData; 1683 DungeonEntranceSettings.PrecalculateEntrancePosition; 1684 PreGenDungeonEntranceSettings.BuriedEntranceYOffset; 1685 PreGenDungeonEntranceSettings.BuriedEntranceSandDugoutYOffset; 1686 PreGenDungeonEntranceSettings.RoughHeight; 1687 PreGenDungeonEntranceSettings.BuryEntrance
- SharedDungeonFeatureDefinitions -> C08: 1688 DungeonFeature.settings; 1689 DungeonFeature.Bounds; 1690 DungeonFeature.generated; 1691 GlobalDungeonFeature.settings; 1692 GlobalDungeonFeature.generated

### C09 - Room lifecycle, settings and geometry

- SharedDungeonRoomCoreState -> C09: 1774 DungeonRoom.settings; 1775 DungeonRoom.calculated; 1776 DungeonRoom.generated; 1777 DungeonRoom.InnerBounds; 1778 DungeonRoom.OuterBounds; 3787 DungeonRoom.Processed; 3788 DungeonRoom.Center
- SharedDungeonRoomSettingsState -> C09: 1779 DungeonRoomSettings.ControlLine; 1780 DungeonRoomSettings.RoomPosition; 1781 DungeonRoomSettings.RoomType; 1782 DungeonRoomSettings.RandomSeed; 1783 DungeonRoomSettings.StyleData; 1784 DungeonRoomSettings.ProgressionStage; 1785 DungeonRoomSettings.StartingRoom; 1786 DungeonRoomSettings.OverridePaintTile; 1787 DungeonRoomSettings.OverridePaintWall; 1788 DungeonRoomSettings.ForceStyleForDoorsAndPlatforms; 1789 DungeonRoomSettings.OnCurvedLine; 1790 DungeonRoomSettings.Orientation; 1791 DungeonRoomSettings.HallwayConnectionPointOverride; 1792 DungeonRoomSettings.HallwayPointAdjuster; 1795 GenShapeDungeonRoomSettings.ShapeType; 1796 GenShapeDungeonRoomSettings.InnerShape; 1797 GenShapeDungeonRoomSettings.OuterShape; 1798 GenShapeDungeonRoomSettings.BoundingRadius; 1804 LegacyDungeonRoomSettings.IsEntranceRoom; 1808 StepBasedDungeonRoomSettings.OverrideStrength; 1809 StepBasedDungeonRoomSettings.OverrideSteps; 1810 StepBasedDungeonRoomSettings.OverrideStartPosition; 1811 StepBasedDungeonRoomSettings.OverrideEndPosition; 1812 StepBasedDungeonRoomSettings.OverrideVelocity; 1813 StepBasedDungeonRoomSettings.OverrideInteriorToExteriorRatio
- SharedDungeonRoomShapeGeometryState -> C09: 1755 BiomeDungeonRoom._innerShapeData; 1756 BiomeDungeonRoom._outerShapeData; 1793 GenShapeDungeonRoom._innerShapeData; 1794 GenShapeDungeonRoom._outerShapeData; 1799 LegacyDungeonRoom._innerShapeData; 1800 LegacyDungeonRoom._outerShapeData; 1805 LivingTreeDungeonRoom._innerShapeData; 1806 LivingTreeDungeonRoom._outerShapeData; 1814 WormlikeDungeonRoom._innerShapeData; 1815 WormlikeDungeonRoom._outerShapeData; 1816 WormlikeDungeonRoom.InnerBoundsSizeMin; 1817 WormlikeDungeonRoom.InnerBoundsSizeMax; 1818 WormlikeDungeonRoom.Positions
- SharedDungeonRoomPlacementGeometryState -> C09: 1757 BiomeRuggedDungeonRoom.Position; 1758 BiomeRuggedDungeonRoom.RoomInnerSize; 1759 BiomeRuggedDungeonRoom.RoomOuterSize; 1760 BiomeRuggedDungeonRoom.WallDepth; 1761 BiomeSquareDungeonRoom.Position; 1762 BiomeSquareDungeonRoom.RoomInnerSize; 1763 BiomeSquareDungeonRoom.RoomOuterSize; 1764 BiomeSquareDungeonRoom.WallDepth; 1770 BiomeStructuredDungeonRoom.Position; 1771 BiomeStructuredDungeonRoom.RoomInnerSize; 1772 BiomeStructuredDungeonRoom.RoomOuterSize; 1773 BiomeStructuredDungeonRoom.WallDepth; 1801 LegacyDungeonRoom.StartPosition; 1802 LegacyDungeonRoom.EndPosition; 1803 LegacyDungeonRoom.Strength; 1807 RegularDungeonRoom._innerBoundsSize

### C10 - Hall lifecycle, settings and geometry

- SharedDungeonHallCoreState -> C10: 1693 DungeonHall.settings; 1694 DungeonHall.calculated; 1695 DungeonHall.generated; 1696 DungeonHall.Bounds; 1697 DungeonHall.StartPosition; 1698 DungeonHall.EndPosition; 1699 DungeonHall.StartDirection; 1700 DungeonHall.EndDirection; 1701 DungeonHall.CrackedBrick; 3786 DungeonHall.Processed
- SharedDungeonHallSettingsState -> C10: 1702 DungeonHallSettings.HallType; 1703 DungeonHallSettings.RandomSeed; 1704 DungeonHallSettings.StyleData; 1705 DungeonHallSettings.OverridePaintTile; 1706 DungeonHallSettings.OverridePaintWall; 1707 DungeonHallSettings.CrackedBrickChance; 1708 DungeonHallSettings.PlaceOverProtectedBricks; 1709 DungeonHallSettings.ZigzagChance; 1710 DungeonHallSettings.ForceStyleForDoorsAndPlatforms; 1711 DungeonHallSettings.CarveOnly; 1719 RegularDungeonHallSettings.OverrideInnerBoundsSize; 1720 RegularDungeonHallSettings.OverrideOuterBoundsSize; 1722 SineDungeonHallSettings.Magnitude; 1723 SineDungeonHallSettings.Iterations; 1724 SineDungeonHallSettings.FlipSine; 1731 StepBasedDungeonHallSettings.OverrideStrength; 1732 StepBasedDungeonHallSettings.OverrideSteps; 1733 StepBasedDungeonHallSettings.ForceHorizontal; 1734 StepBasedDungeonHallSettings.OverrideInteriorToExteriorRatio
- SharedDungeonHallLegacyAndGeometry -> C10: 1712 LegacyDungeonHall.LastHall; 1713 LegacyDungeonHall.Strength; 1714 LegacyDungeonHall.Steps; 1715 LegacyDungeonHall.OverrideStartPosition; 1716 LegacyDungeonHall.OverrideEndPosition; 1717 LegacyEntranceDungeonHall.Direction; 1718 LegacyEntranceDungeonHallSettings.UsePrecalculatedEntrance; 1721 SineDungeonHall.PotentialPlatformPoints; 1725 StairwellDungeonHallSettings.MaxDistFromLine; 1726 StairwellDungeonHallSettings.PointVariance; 1727 StairwellDungeonHallSettings.InnerBoundsSize; 1728 StairwellDungeonHallSettings.OuterBoundsSize; 1729 StairwellDungeonHallSettings.Gradient; 1730 StairwellDungeonHallSettings.IsEntranceHall

### C11 and C14 - Layout graph and control line

- SharedDungeonLayoutProviderDefinitions -> C11: 1735 RoomEntry.room; 1736 RoomEntry.progressAlongSnake; 1737 RoomEntry.backLinks; 1738 RoomEntry.forwardLinks; 1739 HallLine.source; 1740 HallLine.target; 1741 HallLine.sourcePoint; 1742 HallLine.targetPoint; 1743 HallwayCalculator.data; 1744 HallwayCalculator.rooms; 1745 HallwayCalculator.halls; 1746 HallwayCalculator.stairwells; 1747 HallwayCalculator.controlLines; 1748 HallwayCalculator.maxProgressDelta; 1749 HallwayCalculator.avgLineLength; 1750 LegacyDungeonLayoutProviderSettings.Steps; 1751 LegacyDungeonLayoutProviderSettings.MaxSteps
- SharedDungeonControlLineGeometryState -> C14: 1430 DungeonControlLine.Index; 1431 DungeonControlLine.Next; 1432 DungeonControlLine.Prev; 1433 DungeonControlLine.Start; 1434 DungeonControlLine.End; 1435 DungeonControlLine.StartTangent; 1436 DungeonControlLine.EndTangent; 1437 DungeonControlLine.StartNormal; 1438 DungeonControlLine.EndNormal; 1439 DungeonControlLine.CrossTangent; 1440 DungeonControlLine.StartRadius; 1441 DungeonControlLine.EndRadius; 1442 DungeonControlLine.NormalizedDistanceSafeFromDither; 1443 DungeonControlLine.StyleTransitionDitherWidth; 1444 DungeonControlLine.BorderWidth; 1445 DungeonControlLine.NormalizedLineDirection; 1446 DungeonControlLine.LineLength; 1447 DungeonControlLine.Style; 1448 DungeonControlLine.ProgressionStage; 1449 DungeonControlLine.CurveLine; 3746 DungeonControlLine.Center

### C12 - Door, platform and trap placement

- SharedDungeonDoorAndPlatformDefinitions -> C12: 1867 DungeonDoorData.Position; 1868 DungeonDoorData.OverrideBrickTileType; 1869 DungeonDoorData.OverrideBrickWallType; 1870 DungeonDoorData.OverrideStyle; 1871 DungeonDoorData.Direction; 1872 DungeonDoorData.InAHallway; 1873 DungeonDoorData.OverrideWidthFluff; 1874 DungeonDoorData.SkipOtherDoorsCheck; 1875 DungeonDoorData.SkipSpaceCheck; 1876 DungeonDoorData.AlwaysClearArea; 1961 DungeonPlatformData.Position; 1962 DungeonPlatformData.OverrideStyle; 1963 DungeonPlatformData.OverrideMaxLengthAllowed; 1964 DungeonPlatformData.OverrideHeightFluff; 1965 DungeonPlatformData.InAHallway; 1966 DungeonPlatformData.ForcePlacement; 1967 DungeonPlatformData.SkipOtherPlatformsCheck; 1968 DungeonPlatformData.SkipSpaceCheck; 1969 DungeonPlatformData.PlaceBooksChance; 1970 DungeonPlatformData.NoWaterbolt; 1971 DungeonPlatformData.PlacePotsChance; 1972 DungeonPlatformData.PlaceWaterCandlesChance; 1973 DungeonPlatformData.PlacePotionBottlesChance; 1974 DungeonPlatformData.canPlaceHereCallback; 3800 DungeonPlatformData.IsAShelf
- SharedDungeonTrapPlacementState -> C12: 1404 DartTrapPlacementAttempt.directionX; 1405 DartTrapPlacementAttempt.xPush; 1406 DartTrapPlacementAttempt.x; 1407 DartTrapPlacementAttempt.y; 1408 DartTrapPlacementAttempt.position; 1409 DartTrapPlacementAttempt.t; 1410 BoulderPlacementAttempt.position; 1411 BoulderPlacementAttempt.yPush; 1412 BoulderPlacementAttempt.requiredHeight; 1413 BoulderPlacementAttempt.bestType; 1414 WirePlacementAttempt.position; 1415 WirePlacementAttempt.dirX; 1416 WirePlacementAttempt.dirY; 1417 WirePlacementAttempt.steps; 1418 ExplosivePlacementAttempt.position; 1419 DeadMansChestBiome._dartTrapPlacementSpots; 1420 DeadMansChestBiome._wirePlacementSpots; 1421 DeadMansChestBiome._boulderPlacementSpots; 1422 DeadMansChestBiome._explosivePlacementAttempt; 1423 DeadMansChestBiome._numberOfDartTraps; 1424 DeadMansChestBiome._numberOfBoulderTraps; 1425 DeadMansChestBiome._numberOfStepsBetweenBoulderTraps

### C13 - Furniture, style objects and room variants

- SharedDungeonStyleFurnitureCatalogState -> C13: 1886 DungeonGenerationStyleData.WindowPlatformItemTypes; 1889 DungeonGenerationStyleData.LockedBiomeChestType; 1890 DungeonGenerationStyleData.LockedBiomeChestStyle; 1891 DungeonGenerationStyleData.BiomeChestItemType; 1892 DungeonGenerationStyleData.BiomeChestLootItemType; 1893 DungeonGenerationStyleData.ChestItemTypes; 1894 DungeonGenerationStyleData.DoorItemTypes; 1895 DungeonGenerationStyleData.PlatformItemTypes; 1896 DungeonGenerationStyleData.ChandelierItemTypes; 1897 DungeonGenerationStyleData.LanternItemTypes; 1898 DungeonGenerationStyleData.TableItemTypes; 1899 DungeonGenerationStyleData.WorkbenchItemTypes; 1900 DungeonGenerationStyleData.CandleItemTypes; 1901 DungeonGenerationStyleData.VaseOrStatueItemTypes; 1902 DungeonGenerationStyleData.BookcaseItemTypes; 1903 DungeonGenerationStyleData.ChairItemTypes; 1904 DungeonGenerationStyleData.BedItemTypes; 1905 DungeonGenerationStyleData.PianoItemTypes; 1906 DungeonGenerationStyleData.DresserItemTypes; 1907 DungeonGenerationStyleData.SofaItemTypes; 1908 DungeonGenerationStyleData.BathtubItemTypes; 1909 DungeonGenerationStyleData.LampItemTypes; 1910 DungeonGenerationStyleData.CandelabraItemTypes; 1911 DungeonGenerationStyleData.ClockItemTypes; 1912 DungeonGenerationStyleData.BannerItemTypes
- SharedDungeonStyleObjectConstants -> C13: 1975 DungeonUtils.DOORSTYLE_WOODEN; 1976 DungeonUtils.DOORSTYLE_BLUEBRICK; 1977 DungeonUtils.DOORSTYLE_GREENBRICK; 1978 DungeonUtils.DOORSTYLE_PINKBRICK; 1979 DungeonUtils.POTSTYLE_NORMAL_1; 1980 DungeonUtils.POTSTYLE_NORMAL_2; 1981 DungeonUtils.POTSTYLE_NORMAL_3; 1982 DungeonUtils.POTSTYLE_NORMAL_4; 1983 DungeonUtils.POTSTYLE_SKULL_1; 1984 DungeonUtils.POTSTYLE_SKULL_2; 1985 DungeonUtils.POTSTYLE_SKULL_3; 1986 DungeonUtils.CHANDELIERSTYLE_BLUEBRICK; 1987 DungeonUtils.CHANDELIERSTYLE_GREENBRICK; 1988 DungeonUtils.CHANDELIERSTYLE_PINKBRICK; 1989 DungeonUtils.PLATFORMSTYLE_BLUEBRICK; 1990 DungeonUtils.PLATFORMSTYLE_GREENBRICK; 1991 DungeonUtils.PLATFORMSTYLE_PINKBRICK; 1992 DungeonUtils.PLATFORMSTYLE_METALSHELF; 1993 DungeonUtils.PLATFORMSTYLE_BRASSSHELF; 1994 DungeonUtils.PLATFORMSTYLE_WOODSHELF; 1995 DungeonUtils.PLATFORMSTYLE_DUNGEONSHELF
- SharedDungeonStyleBannerAndTrapConstants -> C13/C12: 1996 DungeonUtils.BANNERSTYLE_BRICK_MARCHINGBONES; 1997 DungeonUtils.BANNERSTYLE_BRICK_NECROMANTICSIGN; 1998 DungeonUtils.BANNERSTYLE_SLAB_RUGGEDCOMPANY; 1999 DungeonUtils.BANNERSTYLE_SLAB_RAGGEDBROTHERHOOD; 2000 DungeonUtils.BANNERSTYLE_TILES_MOLTENLEGION; 2001 DungeonUtils.BANNERSTYLE_TILES_DIABOLICSIGIL; 2002 DungeonUtils.TRAPTYPE_DART
- SharedDungeonRoomVariantCatalogState -> C13: 1752 BiomeDungeonRoom.BIOMEROOM_INNER_SIZE_BASE; 1753 BiomeDungeonRoom.BIOMEROOM_INNER_SIZE_BASE_TEMPLE; 1754 BiomeDungeonRoom.BIOMEROOM_WALL_DEPTH; 1765 BiomeStructuredDungeonRoom.VARIANT_DOUBLEDIAMOND; 1766 BiomeStructuredDungeonRoom.VARIANT_ROUNDED; 1767 BiomeStructuredDungeonRoom.VARIANT_CANDY; 1768 BiomeStructuredDungeonRoom.VARIANT_WIGGLED; 1769 BiomeStructuredDungeonRoom.MAX_VARIANTS
- SharedDungeonStyleRoomVariantState -> C13/C06: 1914 DungeonGenerationStyleData.BiomeRoomType; 1915 DungeonGenerationStyleData.SubStyles

### C16 - Spawn configuration

- WorldSpawnConfigurationState -> C16: 2410 ExtraSpawnPointManager.extraSpawnPoints; 2411 ExtraSpawnPointManager.settings; 2412 ExtraSpawnPointManager._listOfLandmasses; 2413 ExtraSpawnSettings.spawnType; 2414 ExtraSpawnSettings.surface; 2415 ExtraSpawnSettings.remix; 2416 ExtraSpawnSettings.roundLandmass; 2417 ExtraSpawnSettings.skyblock; 2418 ExtraSpawnSettings.extraLiquid; 3349 NPCSpawnParams.sizeScaleOverride; 3350 NPCSpawnParams.playerCountForMultiplayerDifficultyOverride; 3351 NPCSpawnParams.difficultyOverride

### C17 - Scene biome-zone projection

- SharedSceneBiomeZoneDefinitionState -> C17: 3442 SceneMetrics.ZoneCorrupt; 3443 SceneMetrics.ZoneCrimson; 3444 SceneMetrics.ZoneHallow; 3445 SceneMetrics.ZoneJungle; 3446 SceneMetrics.ZoneSnow; 3447 SceneMetrics.ZoneDesert; 3448 SceneMetrics.ZoneGlowshroom; 3449 SceneMetrics.ZoneMeteor; 3450 SceneMetrics.ZoneGraveyard; 3451 SceneMetrics.ZoneDungeon; 3452 SceneMetrics.ZoneLihzhardTemple; 3453 SceneMetrics.ZoneGranite; 3454 SceneMetrics.ZoneMarble; 3455 SceneMetrics.ZoneHive; 3456 SceneMetrics.ZoneGemCave; 3457 SceneMetrics.ZoneBeach; 3458 SceneMetrics.ZoneUndergroundDesert

Coverage check: the matrix contains all 485 source indices from the input report. It includes
the 13 version-drift rows explicitly and assigns them to a proposed target; it does not upgrade
their evidence status.

## 7. Proposed interfaces and effect boundaries

The following are interface sketches, not compilable code and not created files.

| Proposed seam | Inputs | Outputs | Side effects and owner |
| --- | --- | --- | --- |
| IGenerationRandomPort | generation id, pass id, cursor | deterministic random value/next cursor | Reads/writes WorldGenerationRandomStateComponent; seed ownership crossSubsystemOwner: integration-review |
| IGenerationProgressPort | pass id, message, fraction | accepted progress event | Projection to GenerationProgress/Main.statusText; UI/logging must not mutate generation |
| IWorldBoundsQuery | world bounds, candidate rectangle | clamped/valid bounds result | Pure Query; uses WorldDescriptor/WorldBounds read-only snapshot |
| IRoomGeometryQuery | room definition, control line, geometry work state | bounds, containment, connection point | Pure Query; no Tile or random write |
| IDungeonPlacementQuery | candidate coordinate, request, snapshot revision | admissible/rejected result | Pure Query; StructureMap/Tile read through adapter; result carries evidence revision |
| IWorldTileMutationPort | validated TileMutationBatch | commit result and revision | Single write owner for Tile/wall/liquid/frame/structure changes; transaction/rollback unresolved |
| IGenerationCheckpointPort | pass state, staged result, random cursor | checkpoint/restore result | Persistence boundary; no proposed implementation in this session |
| IExtraSpawnTransportPort | spawn points and protocol mode | encoded/decoded points | BinaryReader/BinaryWriter/network adapter; does not own spawn policy |
| ISceneBiomeZoneQueryPort | center, tick, revision | immutable zone snapshot | Reads SceneMetrics projection; P03 cannot write source metrics |

## 8. System order contract

The following order is proposed and must be reviewed by integration-review. It is a contract
between systems, not a consequence of file order:

1. WorldGenerationSessionSystem creates the generation aggregate, validates WorldDescriptor and
   WorldGenerationPlan, and binds the random stream and progress sink.
2. TerrainAndBiomePassSystem runs TerrainPass and biome passes, staging tile/structure mutations
   and publishing WorldTerrainState/biome ecology snapshots only after a pass commit.
3. DungeonRuleSetupSystem resolves DungeonGenVars placement/rules/style snapshots for each
   iteration. It must not mutate the style catalog.
4. DungeonControlLineSystem creates and indexes proposed ControlLine components when the dual
   dungeon plan requires them.
5. DungeonLayoutSystem creates short-lived RoomEntry/HallLine graph work, selects the provider,
   calculates room/hall definitions and records explicit links.
6. DungeonRoomCalculationSystem calculates bounds/shape/connection queries for all rooms.
7. DungeonRoomGenerationSystem emits room feature, furniture, door/platform and trap commands.
8. DungeonHallCalculationSystem calculates hall geometry and protection constraints.
9. DungeonHallGenerationSystem emits hall tile/wall/platform commands. Cracked/spider hall
   suborders must be explicit if the Version4 behavior requires them.
10. DungeonFeatureAndTrapSystem validates and batches feature, chest, wire, boulder and explosive
    placement commands.
11. WorldTileCommitSystem is the only proposed writer for the Tile/StructureMap transaction.
12. ExtraSpawnGenerationSystem calculates team spawn points after landmass candidates are stable;
    its binary/network projection runs only after successful world commit.
13. WorldGenerationProgressProjection and SceneBiomeZoneProjection publish read-only snapshots.
14. WorldGenerationFinalizeSystem marks the pass complete, releases transient work buffers and
    hands the committed result to persistence/liquid/NPC integration ports.

Failure and retry contract:

- A pure Query failure rejects a candidate without changing authoritative state.
- A generation System may append commands to a batch but may not partially commit Tile state.
- Commit failure records a stable failure code and checkpoint revision; retry uses an explicit
  policy and the same seed/pass cursor or a new generation id, never an implicit random replay.
- Progress, logs and diagnostics observe the result and cannot advance a pass.
- Cleanup always clears room/hall/attempt caches and invalidates their revision; completed
  world snapshots are immutable projections.

## 9. ID, persistence, network and client boundaries

| Identifier or value | Proposed meaning | Owner |
| --- | --- | --- |
| EntityId | transient ECS identity for generation aggregate, room, hall or feature work entity | crossSubsystemOwner: integration-review |
| PersistentWorldId | durable world identity in WorldSession/WorldStorage | crossSubsystemOwner: integration-review |
| WorldSectionId | storage/streaming partition for committed tile data | crossSubsystemOwner: integration-review |
| TileCoordinate | coordinate value used by geometry and mutation commands | crossSubsystemOwner: integration-review |
| NetworkId | transport identity for any post-generation spawn or snapshot | crossSubsystemOwner: integration-review |
| External file/seed id | world file, seed text or platform/cloud identifier | Adapter/Projection only; never a Component substitute |
| Generation revision | optimistic transaction/checkpoint revision for staged result | proposed integration contract; not present in Version4 inventory |

World generation is primarily server/host-side. Client UI may receive progress and final snapshots
but must not receive mutable DungeonData or issue direct Tile mutations. Persistence must serialize
validated world results and explicit world metadata, not transient ShapeData, TrackHistory,
RoomEntry, HallLine or trap-attempt caches. Network serialization of ExtraSpawnPointManager is
an adapter concern and requires compatibility evidence before implementation.

## 10. Focused verifier plan

The following verifiers are proposed and were not run:

1. Inventory coverage checker: parse the input report and assert exactly 485 source members,
   then assert every source index appears once in the two output mappings.
2. Bounds property verifier: test clamping, positive hitbox, Reset, Contains/Intersects and
   derived X/Y/Width/Height/Center without duplicating authoritative fields.
3. Deterministic random verifier: same world seed, pass id and cursor produce identical style,
   room, hall, trap and extra-spawn decisions; different pass cursors do not share hidden state.
4. Layout graph verifier: every RoomEntry/HallLine relation is acyclic or explicitly allowed,
   no hall crosses protected bounds, and the provider cannot mutate catalog definitions.
5. Room/hall lifecycle verifier: Calculate precedes Generate, Processed is derived, failed
   generation clears transient command buffers, and retry does not duplicate committed mutations.
6. Query purity verifier: geometry, bounds, placement eligibility and scene-zone queries do not
   write Tile, StructureMap, random, progress or persistence.
7. Tile transaction verifier: a rejected or failed batch leaves the world revision unchanged;
   successful commit increments one revision and emits one projection.
8. Extra spawn protocol verifier: team point count, coordinate range, empty state, binary
   round-trip and networking mode are covered.
9. Progress/projection verifier: progress is monotonic per pass, reset is idempotent, and
   SceneBiomeZone snapshot carries center/tick/revision rather than mutable SceneMetrics.
10. Serial repository verification plan: after implementation, use the root
    Build/Tools/Invoke-SerialDotnet.ps1 runner, build only the affected project, verify Build/bin,
    then run focused tests with no-build/no-restore. This is a future plan, not an execution result.

## 11. Evidence gaps and blocking decisions

### Evidence gaps

- Missing first-round P03 output means no first-round member-level reader/writer/lifecycle audit
  can be cited as an existing artifact.
- Version4 has empty or reduced method bodies in several generation classes. The complete
  reference confirms the shape of the call chain but not NLTX behavior.
- The 13 version-drift rows need a source-row regeneration or an explicit report correction
  before implementation. Their names are still covered above.
- The exact ownership and serialization of Tile mutations, StructureMap, chest/wire objects,
  world sections, progress checkpoints and extra spawn points is not closed by P03 evidence.
- SceneMetrics, NPCSpawnParams, AmbientWindSystem, ChumBucketProjectileHelper and after-party
  state cross into other domains; P03 only proposes ports and projections.

### Blocking decisions

- Select one authoritative writer for DungeonGenerationContext and the legacy GenVars projection.
- Decide whether room/hall/feature work entities are ephemeral ECS entities or indexed world
  records, and define their relation IDs.
- Define the transaction boundary between layout/commands and TileMap/WorldStorage, including
  rollback, partial success, checkpoint revision and retry semantics.
- Decide whether extra spawn points are world-persistent, network-replicated, or regenerated
  from rules on load; keep the answer consistent with the BinaryReader/BinaryWriter boundary.
- Confirm whether SceneBiomeZone flags are a P03 read projection or a P16-owned authoritative
  scene snapshot, and define revision/tick invalidation.
- Confirm exact pass order with WorldSession, WorldStorage, Liquid, NPC, Content and diagnostics
  integration sessions. No file or directory order may be used as a substitute.

## 12. Integration Handoff

subsystemId: P03-world-generation-dungeons
taskNumber: 03/20
reportPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\non-authoritative\2026-09-11-version4-non-authoritative-P03-world-generation-dungeons-component-design.md

evidenceStatus: source inventory confirmed; Version4 and complete-reference boundaries partially confirmed; first-round artifact missing
nltxStatus: current NLTX contains world-generation plan/pass/random/terrain/ecology components and the implemented P03 C01-C04, C05-C14 and C16 local boundaries; C15, C17 and C18 remain pending.
verificationStatus: partial

confirmedOwners:
- Version4 DungeonBounds owns clamped bounds and derived hitbox behavior within the Dungeon generation code
- Version4 DungeonCrawler owns the observed setup/layout/generate lifecycle within the source reference
- Version4 WorldGen owns pass registration and the observed MakeDungeon invocation boundary
- Version4 SceneMetrics owns the observed scan/reset behavior, while final NLTX owner remains unresolved

proposedTypes:
- proposed WorldGenerationHostStateComponent, WorldGenerationMenuSessionComponent, GenerationProgressProjection
- proposed TerrainPassWorkState, MagmaFlowWorkState, BiomeStructurePlacementDefinition
- proposed DungeonGenerationContextComponent, DungeonGenerationCollectionsComponent, DungeonBoundsComponent
- proposed DungeonStyleCatalog, DungeonRoomDefinitionComponent, DungeonHallDefinitionComponent
- proposed DungeonLayoutGraphWorkState, DungeonControlLineComponent, DungeonDoorPlacementRequest
- proposed WorldGenerationOrchestrationSystem, DungeonLayoutSystem, DungeonRoomGenerationSystem, DungeonHallGenerationSystem
- proposed DungeonTileMutationCommand, IWorldTileMutationPort, ExtraSpawnBinaryAdapter
- proposed SceneBiomeZoneSnapshotProjection and GenerationResultProjection

sharedTypesForIntegrationReview:
- EntityId, PersistentWorldId, WorldSectionId, TileCoordinate, NetworkId, generation revision
- WorldGenerationProgress, SceneBiomeZoneSnapshot, Dungeon style/content item identifiers
- Tile mutation batch, StructureMap/chest/wire identities, ExtraSpawnPoint payload

crossSubsystemReaders:
- WorldStorage/liquid handoff, NPC spawn, Player/SceneMetrics, Content definitions, UI/client progress, diagnostics

crossSubsystemWriters:
- WorldSession plan/rules, Tile/WorldStorage commit, scene scan, NPC/spawn integration, persistence/network adapters

orderingConstraints:
- Terrain/biome setup precedes dungeon setup; setup precedes control-line/layout; calculation precedes generation; validation precedes Tile commit; commit precedes persistence/network projection
- DualDungeon control-line and layout ordering requires explicit integration-review confirmation

boundaryChallenges:
- Do not retain DungeonData as a giant component; split collections, scalars, definitions, work graphs and lifecycle
- Do not persist geometry caches, style constants, attempts or progress as gameplay state
- Do not let Query, Projection, logging or progress callbacks mutate world generation

evidenceGaps:
- Missing first-round P03 report; Version4 stubs; 13 version-drift rows; unresolved Tile/WorldStorage transaction; unresolved Scene/NPC/ExtraSpawn owners

blockingDecisions:
- Unique context/GenVars writer; room/hall work entity identity; Tile transaction and rollback; extra-spawn persistence/network semantics; SceneMetrics snapshot owner; final cross-partition pass order

notImplemented:
- C15、C17、C18 尚未实现；C03、C04、C16 仅完成 Component 部分，C01-C02 与 C05-C14 的 focused verifier、完整编译和运行证据仅覆盖当前本地 verifier，不证明行为等价或跨分区 owner 已闭合。
- 目标运行时注册、网络协议、存档格式、Tile/WorldStorage 事务和跨分区 owner 仍未实现。

verifierPlan:
- Coverage, bounds, deterministic random, layout graph, lifecycle, Query purity, Tile transaction, ExtraSpawn protocol, progress and serial build/test plans listed in Section 10

本文件仅提出 proposed 组件边界和后续设计 checkpoint。它不是迁移完成报告、行为等价证明、
API/网络/持久化闭合证明，也不是当前 NLTX 已实现能力的声明。
