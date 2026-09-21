# Version4 非权威组件第二轮实施计划：P03 世界生成与地牢

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
- P03 第一轮 public-decomposition outputReport 不存在；本计划只能引用输入库存、Version4 直接源码、完整参考源码和公开 API 证据，不能把缺失的第一轮结论当成事实。
- 输入库存中的 13 条行号漂移记录（1423、1424、1425、1429、1825、3738-3744、3747）已按成员名称保留映射，但在源报告修正前不能升级为已闭合的语义证据。
- Version4 的若干 Dungeon、Biome 和 WorldGen 方法为空实现或简化实现；完整参考源码只能补充同路径调用闭包，不能证明 NLTX 的行为或兼容性。
- Tile/WorldStorage 提交事务、WorldGen 随机流持久化、网络/存档格式、SceneMetrics 最终 owner、NPC 生成 owner 和跨分区调度仍未闭合。
- 旧记录中 C01、C02 以及 C05-C14 的 verifier 成功输出不能作为当前工作树的有效证据；当前 `WorldGenerationVerification` 项目仍因缺失的 Commands、Queries 和 Systems 命名空间无法编译。
- C03 的两个 Component 已保存到 `src2/WorldGeneration/Biomes`；结构放置的 Query、System、Command、Tile 和 StructureMap 提交仍未实现或验证。
- C04 的三个 Component 已保存到 `src2/WorldGeneration/Terrain`；WorldGenRange 的世界缩放、pass 调度、terrain/ecology snapshot 和 Tile 提交仍未闭合。
- C16 的三个 Component 与 `ExtraSpawnType` 已保存到 `src2/WorldGeneration/Spawn`；spawn 计算、BinaryReader/BinaryWriter、网络/存档语义和 NPC owner 仍未闭合。
- 受影响的 `src2/WorldGeneration/Terraria.WorldGeneration.csproj` 已通过仓库串行包装器构建，exitCode 0、0 warnings、0 errors，产物为 `D:\TRbackup\NLTX\Build\bin\Terraria.WorldGeneration\Debug\net10.0\Terraria.WorldGeneration.dll`。
- 既有 `src2/WorldGenerationVerification/Terraria.WorldGenerationVerification.csproj` 的串行构建 exitCode 1、0 warnings、3 errors：`Program.cs(3,32)` 缺失 `Terraria.WorldGeneration.Commands`，`Program.cs(16,32)` 缺失 `Terraria.WorldGeneration.Queries`，`Program.cs(17,32)` 缺失 `Terraria.WorldGeneration.Systems`；验证器和其依赖未在本轮修改。
blocking-decision:
- integration-review 必须选定 DungeonGenerationContext 与 legacy GenVars 的唯一写入 owner，并确定 TileMap/WorldStorage 的事务提交、回滚、checkpoint 和 retry 协议。
- integration-review 必须确定 WorldGenerationProgress、SceneBiomeZone、ExtraSpawn、NPCSpawnParams 的共享接口、快照版本、持久化/网络语义和最终调度顺序。

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

## 1. 计划性质与执行边界

本文件是 P03 的 proposed 后续实施计划，并附带当前非权威 Component 实现、编译和验证
checkpoint；它不是迁移完成报告、测试报告或行为等价证明。未实现的 System、Query、Command、
Adapter 和 Projection 仍是 proposed；实际已保存的 Component 文件只以本文件的
`modifiedFiles` 和 checkpoint 列表为准。

本轮实现阶段允许修改当前 P03 的 `src2` C# 项目和这两份文档；没有修改 `src`、Test、
Version4、完整参考源码、其他分区报告、其他会话文档、ledger 或 lock。编译/测试证据仍须
以实际串行命令为准，不得从源码存在推断验证通过。

`completedComponents` 表示设计阶段已为 C01-C18 建立 proposed checkpoint 和实施入口；
`implementationProgress` 另行记录已经保存到 `src2` 的实际实现单元。因而
`executionStatus` 继续表示后续计划仍有未执行阶段，`implementationStatus` 表示本轮已保存
部分 Component，`verificationStatus: partial` 表示受影响 Component 项目已编译，但完整
verifier 因既有缺失命名空间而未能编译。

## 2. 目标目录、命名空间和文件

目标根目录暂定为 `src2/WorldSession/WorldGeneration`。目录按世界生成能力、地牢领域和
外部边界组织；不按 485 条成员机械建文件，也不创建 `Shared/Components/` 之类的通用收纳
目录。每个拟新增文件只承载一个同名核心 public type；路径和 namespace 仍需经过 C# 风格、
ECS 文件组织约束和 integration-review 复核。

| Checkpoint | Proposed target directory | Proposed namespace | Proposed files and role |
| --- | --- | --- | --- |
| C01 | `WorldGeneration/Host`, `WorldGeneration/Projections`, `WorldGeneration/Adapters` | `Terraria.WorldGeneration.Host`, `.Projections`, `.Adapters` | `WorldGenerationHostRuntime.cs`、`WorldGenerationHostRuntimeSystem.cs`（宿主状态和效果顺序）、`WorldGenerationMenuSession.cs`（菜单 Component）、`WorldGenerationGraphicsSnapshot.cs`、`WorldGenerationProgressProjection.cs`（单向 Projections）、`IGenerationClockPort.cs`、`IGraphicsGenerationPort.cs`、`IProgressSink.cs`、`IAmbientWindGenerationPort.cs`、`IChumBucketProjectilePort.cs`（外部效果 ports） |
| C02 | `WorldGeneration/Support`, `WorldGeneration/Adapters` | `Terraria.WorldGeneration.Support`, `.Adapters` | `GenerationSupportWorkState.cs`、`GenerationFeedbackSnapshot.cs`、`PaintingDefinition.cs`、`ShapeFloodFillWorkState.cs`、`TrackGenerationWorkState.cs`、`GenerationFeedbackPort.cs`、`RoomCheckFeedbackAdapter.cs`（工作缓存、值定义、反馈边界） |
| C03 | `WorldGeneration/Biomes`, `WorldGeneration/Systems`, `WorldGeneration/Queries`, `WorldGeneration/Commands` | `Terraria.WorldGeneration.Biomes`, `.Systems`, `.Queries`, `.Commands` | `BiomeStructurePlacementDefinition.cs`、`CaveHouseBudgetState.cs`、`BiomeStructureGenerationSystem.cs`、`StructurePlacementEligibilityQuery.cs`、`StructurePlacementCommand.cs`（结构定义、预算、纯资格判断和提交意图） |
| C04 | `WorldGeneration/Terrain`, `WorldGeneration/Biomes`, `WorldGeneration/Systems` | `Terraria.WorldGeneration.Terrain`, `.Biomes`, `.Systems` | `TerrainPassWorkState.cs`、`MagmaFlowWorkState.cs`、`DunesPlacementDefinition.cs`、`TerrainAndBiomePassSystem.cs`（pass 工作状态、定义和显式系统） |
| C05 | `WorldGeneration/Dungeon/Bounds`, `WorldGeneration/Dungeon/Queries` | `Terraria.WorldGeneration.Dungeon.Bounds`, `.Dungeon.Queries` | `DungeonBoundsComponent.cs`、`DungeonWallProgressionDefinition.cs`、`DungeonGeometryRuleCatalog.cs`、`DungeonBoundsQuery.cs`、`DungeonBoundsMutationSystem.cs`（边界唯一写者、派生几何查询） |
| C06 | `WorldGeneration/Dungeon/Styles`, `WorldGeneration/Dungeon/Queries` | `Terraria.WorldGeneration.Dungeon.Styles`, `.Dungeon.Queries` | `DungeonStyleCatalog.cs`、`DungeonStyleMaterialDefinition.cs`、`DungeonStyleSetCatalog.cs`、`DungeonStyleSelectionQuery.cs`（只读样式和材质 catalog） |
| C07 | `WorldGeneration/Dungeon/Generation`, `WorldGeneration/Dungeon/Layout` | `Terraria.WorldGeneration.Dungeon.Generation`, `.Dungeon.Layout` | `DungeonCrawlerContext.cs`、`DungeonGenerationContextComponent.cs`、`DungeonGenerationCollectionsComponent.cs`、`DungeonGenerationScalarStateComponent.cs`、`DungeonLegacyPlacementStateComponent.cs`、`DungeonLegacyRuleStateComponent.cs`、`DungeonLayoutProviderDefinition.cs`、`DungeonLayoutProviderSettings.cs`（唯一生成上下文、legacy projection 输入和 layout 定义） |
| C08 | `WorldGeneration/Dungeon/Entrances`, `WorldGeneration/Dungeon/Features`, `WorldGeneration/Systems` | `Terraria.WorldGeneration.Dungeon.Entrances`, `.Dungeon.Features`, `.Systems` | `DungeonEntranceDefinition.cs`、`DungeonEntranceLifecycleComponent.cs`、`DungeonFeatureDefinition.cs`、`DungeonFeatureLifecycleComponent.cs`、`DungeonEntranceAndFeatureSystem.cs`（入口/feature 生命周期和命令生产） |
| C09 | `WorldGeneration/Dungeon/Rooms`, `WorldGeneration/Dungeon/Queries`, `WorldGeneration/Systems` | `Terraria.WorldGeneration.Dungeon.Rooms`, `.Dungeon.Queries`, `.Systems` | `DungeonRoomDefinition.cs`、`DungeonRoomLifecycleComponent.cs`、`DungeonRoomGeometryWorkState.cs`、`DungeonRoomQuery.cs`、`DungeonRoomCalculationSystem.cs`、`DungeonRoomGenerationSystem.cs`（room 定义、几何工作集、生命周期和命令生产） |
| C10 | `WorldGeneration/Dungeon/Halls`, `WorldGeneration/Dungeon/Queries`, `WorldGeneration/Systems` | `Terraria.WorldGeneration.Dungeon.Halls`, `.Dungeon.Queries`, `.Systems` | `DungeonHallDefinition.cs`、`DungeonHallLifecycleComponent.cs`、`DungeonHallGeometryWorkState.cs`、`DungeonHallQuery.cs`、`DungeonHallCalculationSystem.cs`、`DungeonHallGenerationSystem.cs`（hall 定义、几何工作集、生命周期和命令生产） |
| C11 | `WorldGeneration/Dungeon/Layout`, `WorldGeneration/Systems` | `Terraria.WorldGeneration.Dungeon.Layout`, `.Systems` | `DungeonLayoutGraphWorkState.cs`、`RoomEntry.cs`、`HallLine.cs`、`HallwayCalculatorState.cs`、`DungeonLayoutSystem.cs`（短期布局图和显式 provider 调度） |
| C12 | `WorldGeneration/Dungeon/Placement`, `WorldGeneration/Dungeon/Queries`, `WorldGeneration/Commands` | `Terraria.WorldGeneration.Dungeon.Placement`, `.Dungeon.Queries`, `.Commands` | `DungeonDoorPlacementRequest.cs`、`DungeonPlatformPlacementRequest.cs`、`DungeonTrapPlacementWorkState.cs`、`DungeonPlacementQuery.cs`、`DungeonPlacementCommandSystem.cs`、`DungeonTileMutationCommand.cs`（门、平台、陷阱提交意图；Tile 不在此处直接写） |
| C13 | `WorldGeneration/Dungeon/Catalogs`, `WorldGeneration/Dungeon/Queries` | `Terraria.WorldGeneration.Dungeon.Catalogs`, `.Dungeon.Queries` | `DungeonFurnitureCatalog.cs`、`DungeonObjectStyleCatalog.cs`、`DungeonBannerTrapCatalog.cs`、`DungeonRoomVariantCatalog.cs`、`DungeonObjectStyleQuery.cs`（家具、对象样式、旗帜/陷阱和房间 variant 定义） |
| C14 | `WorldGeneration/Dungeon/Layout`, `WorldGeneration/Dungeon/Queries`, `WorldGeneration/Systems` | `Terraria.WorldGeneration.Dungeon.Layout`, `.Dungeon.Queries`, `.Systems` | `DungeonControlLineComponent.cs`、`DungeonControlLineGeometryQuery.cs`、`DungeonControlLineSystem.cs`（control line 几何快照和关系索引） |
| C15 | `WorldGeneration/Systems`, `WorldGeneration/Commands`, `WorldGeneration/Adapters` | `Terraria.WorldGeneration.Systems`, `.Commands`, `.Adapters` | `WorldGenerationOrchestrationSystem.cs`、`DungeonGenerationSystem.cs`、`GenerationPassCommitSystem.cs`、`WorldTileCommitSystem.cs`、`DungeonTileMutationBatch.cs`、`IWorldTileMutationPort.cs`、`IGenerationCheckpointPort.cs`（显式顺序、批量提交和 checkpoint 边界） |
| C16 | `WorldGeneration/Spawn`, `WorldGeneration/Adapters`, `WorldGeneration/Systems` | `Terraria.WorldGeneration.Spawn`, `.Adapters`, `.Systems` | `ExtraSpawnPolicyDefinition.cs`、`ExtraSpawnPointState.cs`、`NpcSpawnOverrideValue.cs`、`ExtraSpawnGenerationSystem.cs`、`ExtraSpawnBinaryAdapter.cs`、`IExtraSpawnTransportPort.cs`（spawn policy、候选点和协议适配） |
| C17 | `WorldGeneration/Metrics`, `WorldGeneration/Projections`, `WorldGeneration/Adapters` | `Terraria.WorldGeneration.Metrics`, `.Projections`, `.Adapters` | `SceneBiomeZoneQueryPort.cs`、`SceneBiomeZoneSnapshotProjection.cs`、`GenerationPresentationProjection.cs`（只读 scene-zone 和生成表现快照） |
| C18 | `tests/WorldSession/WorldGeneration`, `Build/Tools`（后续才允许创建） | `Terraria.WorldSession.Tests` 或仓库既有测试 namespace（待确认） | `P03InventoryCoverageTests.cs`、`DungeonGenerationFocusedTests.cs`、`WorldGenerationStaticChecks.ps1` 均为 proposed 验证入口；本轮不创建。 |

### 2.1 目标文件的依赖方向

Definitions、catalogs、值对象和 Components 只能保存内聚数据；Queries 只读取快照并返回
结果；Systems 把输入状态转换为下一个状态或 Command；Commands 只表达待提交意图；Adapters
隔离 Main、WorldGen、Tile、StructureMap、BinaryReader/BinaryWriter、网络、存档、时钟、图形
和日志；Projections 单向输出快照。任何 `DungeonData`、`DungeonGenVars`、SceneMetrics 或
ExtraSpawnPointManager 的直接引用都必须停留在明确的 Adapter/Projection 边界，不能扩散到
通用 Component。

## 3. 485 条 source member 的执行映射

以下映射以输入报告的 `source index` 为稳定审计键。每个分组中的每一条
`index Type.member` 都映射到该行给出的 proposed target file 和角色；这些只是后续执行的
目标，不是已创建文件。C15 和 C18 是编排/验证 checkpoint，输入报告没有额外成员，因此不
重复虚构 source index。

### C01 - Host and menu runtime

Target role/file: `WorldGenerationHostRuntime.cs`（宿主边界和 presentation adapter）、
`WorldGenerationMenuSession.cs`（菜单会话 Component）、`GenerationProgressProjection.cs`
（只读输出）；图形、计时器和 ambient system 通过 ports 注入。

- `MainGraphicsAndGenerationState`: 153 `Main.OurFavoriteColor`; 154 `Main.mapEnabled`; 155 `Main.IsEnginePreloaded`; 156 `Main._gameUpdateCount`; 157 `Main.SkipAssemblyLoad`; 158 `Main.renderCount`; 159 `Main.graphics`; 160 `Main.AutogenProgress`; 161 `Main.saveTime`; 162 `Main.shimmerAlpha`; 163 `Main.shimmerDarken`; 164 `Main.afterPartyOfDoom`
- `MainMenuAndWorldGenerationState`: 497 `Main.ChumBucketProjectileHelper`; 498 `Main.maxMenuItems`; 499 `Main.menuItemScale`; 500 `Main.menuMode`; 501 `Main.newWorldName`; 502 `Main._ambientWindSys`; 503 `Main.autoPass`

### C02 - Generation support

Target role/file: `GenerationSupportWorkState.cs`、`GenerationFeedbackSnapshot.cs`、
`PaintingDefinition.cs`、`ShapeFloodFillWorkState.cs`、`TrackGenerationWorkState.cs`、
`GenerationFeedbackPort.cs` 和 `RoomCheckFeedbackAdapter.cs`；反馈和日志不拥有生成状态。

- `SharedWorldGenerationSupport`: 1194 `NoRoomCheckFeedback.WithText`; 1195 `NoRoomCheckFeedback.WithoutText`; 2011 `PaintingEntry.tileType`; 2012 `PaintingEntry.style`; 2013 `ShapeFloodFill._maximumActions`; 2014 `TrackHistory.X`; 2015 `TrackHistory.Y`; 2016 `TrackHistory.Slope`; 2017 `TrackHistory.Mode`; 2018 `TrackGenerator._history`; 2019 `TrackGenerator._rewriteHistory`; 3691 `IRoomCheckFeedback_Spread.StopOnFail`; 3692 `IRoomCheckFeedback_Spread.DisplayText`; 3693 `NoRoomCheckFeedback.StopOnFail`; 3694 `NoRoomCheckFeedback.DisplayText`

### C03 - Cave house and structures

Target role/file: `BiomeStructurePlacementDefinition.cs`、`CaveHouseBudgetState.cs`、
`BiomeStructureGenerationSystem.cs`、`StructurePlacementEligibilityQuery.cs`、
`StructurePlacementCommand.cs`；blacklist/beelist 和 loot chance 进入定义/预算，实际 tile
和 StructureMap 改动只生成 Command。

- `SharedBiomeCaveHouseAndStructureState`: 1399 `HouseBuilderContext.SharpenerCount`; 1400 `HouseBuilderContext.ExtractinatorCount`; 1401 `HouseUtils.BlacklistedTiles`; 1402 `HouseUtils.BeelistedTiles`; 1403 `CaveHouseBiome._builderContext`; 3738 `CaveHouseBiome.IceChestChance`; 3739 `CaveHouseBiome.JungleChestChance`; 3740 `CaveHouseBiome.GoldChestChance`; 3741 `CaveHouseBiome.GraniteChestChance`; 3742 `CaveHouseBiome.MarbleChestChance`; 3743 `CaveHouseBiome.MushroomChestChance`; 3744 `CaveHouseBiome.DesertChestChance`

### C04 - Terrain and biome pass work

Target role/file: `TerrainPassWorkState.cs`、`MagmaFlowWorkState.cs`、
`DunesPlacementDefinition.cs`、`TerrainAndBiomePassSystem.cs`；数组、索引、向量和宽度是
pass 生命周期工作集，pass 完成后只发布 terrain/ecology snapshot。

- `SharedBiomeTerrainPassState`: 1426 `DesertBiome.ChanceOfEntrance`; 1427 `DitherSnake.CircleTestPoints`; 1428 `DitherSnake.ExtraBuffer`; 1429 `DunesBiome._singleDunesWidth`; 1450 `Magma.Pressure`; 1451 `Magma.Resistance`; 1452 `Magma.IsActive`; 1453 `GraniteBiome.MAX_MAGMA_ITERATIONS`; 1454 `GraniteBiome._sourceMagmaMap`; 1455 `GraniteBiome._targetMagmaMap`; 1456 `GraniteBiome._normalisedVectors`; 1457 `MarbleBiome.SCALE`; 1458 `SurfaceHistory._heights`; 1459 `SurfaceHistory._index`; 3745 `DunesBiome.MaximumWidth`; 3747 `SurfaceHistory.this[]`

### C05 - Bounds and progression

Target role/file: `DungeonBoundsComponent.cs`（唯一边界状态）、
`DungeonWallProgressionDefinition.cs` 和 `DungeonGeometryRuleCatalog.cs`（只读定义）、
`DungeonBoundsQuery.cs`（派生查询）、`DungeonBoundsMutationSystem.cs`（唯一写者）。
`Left/Right/Top/Bottom` 是 authority candidates；hitbox、X/Y/Width/Height/Center 是派生值。

- `SharedDungeonBoundsAndProgressionDefinitions`: 1819 `DualDungeonUnbreakableWallTiers.EarlyGame`; 1820 `DualDungeonUnbreakableWallTiers.EvilBoss`; 1821 `DualDungeonUnbreakableWallTiers.JungleBoss`; 1822 `DualDungeonUnbreakableWallTiers.Dungeon`; 1823 `DualDungeonUnbreakableWallTiers.Hallow`; 1824 `DualDungeonUnbreakableWallTiers.Temple`; 1825 `DungeonBounds._hitbox`; 1826 `DungeonBounds._boundsLeft`; 1827 `DungeonBounds._boundsRight`; 1828 `DungeonBounds._boundsTop`; 1829 `DungeonBounds._boundsBottom`; 3789 `DungeonBounds.X`; 3790 `DungeonBounds.Y`; 3791 `DungeonBounds.Width`; 3792 `DungeonBounds.Height`; 3793 `DungeonBounds.Left`; 3794 `DungeonBounds.Right`; 3795 `DungeonBounds.Top`; 3796 `DungeonBounds.Bottom`; 3797 `DungeonBounds.Center`
- `SharedDungeonGeometryRuleQueries`: 2003 `DungeonUtils.HALLWAY_DOOR_PLACEMENT_VARIANCE`; 2004 `DungeonUtils.DUNGEONHALL_DEFAULT_INNER_AREA_DEPTH`; 2005 `DungeonUtils.DUNGEONHALL_DEFAULT_OUTER_WALL_DEPTH`; 2006 `DungeonUtils.DUNGEONROOM_DEFAULT_INNER_AREA_DEPTH`; 2007 `DungeonUtils.DUNGEONROOM_DEFAULT_OUTER_WALL_DEPTH`; 2008 `DungeonUtils.MOSAIC_NONE`; 2009 `DungeonUtils.MOSAIC_SKELETRON`; 2010 `DungeonUtils.MOSAIC_MOONLORD`

### C06 - Style catalog and materials

Target role/file: `DungeonStyleCatalog.cs`、`DungeonStyleMaterialDefinition.cs`,
`DungeonStyleSetCatalog.cs` 和 `DungeonStyleSelectionQuery.cs`；catalog 初始化后只读，不能由
生成结果反向改写。

- `SharedDungeonStyleMaterialAndGeometryState`: 1877 `DungeonGenerationStyleData.Style`; 1878 `DungeonGenerationStyleData.UnbreakableWallProgressionTier`; 1879 `DungeonGenerationStyleData.BrickTileType`; 1880 `DungeonGenerationStyleData.BrickGrassTileType`; 1881 `DungeonGenerationStyleData.BrickCrackedTileType`; 1882 `DungeonGenerationStyleData.BrickWallType`; 1883 `DungeonGenerationStyleData.WindowGlassWallType`; 1884 `DungeonGenerationStyleData.WindowClosedGlassWallType`; 1885 `DungeonGenerationStyleData.WindowEdgeWallType`; 1887 `DungeonGenerationStyleData.PitTrapTileType`; 1888 `DungeonGenerationStyleData.LiquidType`; 1913 `DungeonGenerationStyleData.EdgeDither`
- `SharedDungeonStyleSetCatalog`: 1916 `DungeonGenerationStyles.Shimmer`; 1917 `DungeonGenerationStyles.Spider`; 1918 `DungeonGenerationStyles.LivingWood`; 1919 `DungeonGenerationStyles.Cavern`; 1920 `DungeonGenerationStyles.Snow`; 1921 `DungeonGenerationStyles.Desert`; 1922 `DungeonGenerationStyles.Corruption`; 1923 `DungeonGenerationStyles.Crimson`; 1924 `DungeonGenerationStyles.Crystal`; 1925 `DungeonGenerationStyles.Hallow`; 1926 `DungeonGenerationStyles.GlowingMushroom`; 1927 `DungeonGenerationStyles.Beehive`; 1928 `DungeonGenerationStyles.LivingMahogany`; 1929 `DungeonGenerationStyles.Jungle`; 1930 `DungeonGenerationStyles.Temple`

### C07 - Generation context and legacy state

Target role/file: `DungeonCrawlerContext.cs`、`DungeonGenerationContextComponent.cs`、
`DungeonGenerationCollectionsComponent.cs`、`DungeonGenerationScalarStateComponent.cs`、
`DungeonLegacyPlacementStateComponent.cs`、`DungeonLegacyRuleStateComponent.cs`、
`DungeonLayoutProviderDefinition.cs` 和 `DungeonLayoutProviderSettings.cs`。所有 legacy 字段
只允许由 `DungeonGenerationSystem` 的一个 owner 写入；`DungeonData.genVars` 只能成为受控
read projection，不能形成第二份 authority。

- `SharedDungeonCrawlerRuntimeState`: 1830 `DungeonCrawler.dungeonData`; 3798 `DungeonCrawler.CurrentDungeonData`
- `SharedDungeonGenerationCollectionsState`: 1831 `DungeonData.Type`; 1832 `DungeonData.Iteration`; 1833 `DungeonData.dungeonEntrance`; 1834 `DungeonData.dungeonRooms`; 1835 `DungeonData.dungeonHalls`; 1836 `DungeonData.dungeonFeatures`; 1837 `DungeonData.dungeonDoorData`; 1838 `DungeonData.dungeonPlatformData`; 1839 `DungeonData.protectedDungeonBounds`; 1840 `DungeonData.makeNextPitTrapFlooded`; 1841 `DungeonData.useSkewedDungeonEntranceHalls`; 1842 `DungeonData.createdDungeonEntranceOnSurface`; 1843 `DungeonData.dungeonEntranceStrengthX`; 1844 `DungeonData.dungeonEntranceStrengthY`; 1845 `DungeonData.dungeonEntranceStrengthX2`; 1846 `DungeonData.dungeonEntranceStrengthY2`; 1847 `DungeonData.lastDungeonHall`; 1848 `DungeonData.dungeonBounds`; 1849 `DungeonData.outerProgressionBounds`; 3799 `DungeonData.genVars`
- `SharedDungeonGenerationScalarAndStyleState`: 1850 `DungeonData.wallVariants`; 1851 `DungeonData.chandelierItemType`; 1852 `DungeonData.platformItemType`; 1853 `DungeonData.doorItemType`; 1854 `DungeonData.lanternStyles`; 1855 `DungeonData.shelfStyles`; 1856 `DungeonData.bannerStyles`; 1857 `DungeonData.globalFeatureScalar`; 1858 `DungeonData.dungeonStepScalar`; 1859 `DungeonData.hallStrengthScalar`; 1860 `DungeonData.hallStepScalar`; 1861 `DungeonData.hallInteriorToExteriorRatio`; 1862 `DungeonData.hallSlantVariantScalar`; 1863 `DungeonData.roomStrengthScalar`; 1864 `DungeonData.roomStepScalar`; 1865 `DungeonData.roomInteriorToExteriorRatio`; 1866 `DungeonData.roomSlantVariantScalar`
- `SharedDungeonLegacyPlacementState`: 1931 `DungeonGenVars.dungeonSide`; 1932 `DungeonGenVars.dungeonLocation`; 1933 `DungeonGenVars.dungeonColor`; 1934 `DungeonGenVars.brickTileType`; 1935 `DungeonGenVars.brickWallType`; 1936 `DungeonGenVars.brickCrackedTileType`; 1937 `DungeonGenVars.windowGlassWallType`; 1938 `DungeonGenVars.windowClosedGlassWallType`; 1939 `DungeonGenVars.windowEdgeWallType`; 1940 `DungeonGenVars.windowPlatformItemTypes`; 1941 `DungeonGenVars.generatingDungeonPositionX`; 1942 `DungeonGenVars.generatingDungeonPositionY`; 1943 `DungeonGenVars.generatingDungeonTopX`; 1944 `DungeonGenVars.dungeonLootStyle`; 1945 `DungeonGenVars.outerPotentialDungeonBounds`; 1946 `DungeonGenVars.innerPotentialDungeonBounds`; 1957 `DungeonGenVars.dungeonEntrancePosition`
- `SharedDungeonLegacyRuleState`: 1947 `DungeonGenVars.dungeonStyle`; 1948 `DungeonGenVars.dungeonGenerationStyles`; 1949 `DungeonGenVars.dungeonDitherSnake`; 1950 `DungeonGenVars.isCrackedBrick`; 1951 `DungeonGenVars.isPitTrapTile`; 1952 `DungeonGenVars.isDungeonTile`; 1953 `DungeonGenVars.isDungeonWall`; 1954 `DungeonGenVars.isDungeonWallGlass`; 1955 `DungeonGenVars.GeneratingDungeon`; 1956 `DungeonGenVars.preGenDungeonEntranceSettings`; 1958 `DungeonGenVars.desertChestLootState`
- `SharedDungeonLayoutProviderState`: 1959 `DungeonLayoutProvider.settings`; 1960 `DungeonLayoutProviderSettings.StyleData`

### C08 - Entrance and feature lifecycle

Target role/file: `DungeonEntranceDefinition.cs`、`DungeonEntranceLifecycleComponent.cs`,
`DungeonFeatureDefinition.cs`、`DungeonFeatureLifecycleComponent.cs` 和
`DungeonEntranceAndFeatureSystem.cs`；OldMan spawn、chest、NPC 和 structure owner 的跨分区
交接仍标记 `crossSubsystemOwner: integration-review`。

- `SharedDungeonEntranceDefinitions`: 1675 `DungeonEntrance.settings`; 1676 `DungeonEntrance.calculated`; 1677 `DungeonEntrance.generated`; 1678 `DungeonEntrance.Bounds`; 1679 `DungeonEntrance.OldManSpawn`; 1680 `DungeonEntranceSettings.EntranceType`; 1681 `DungeonEntranceSettings.RandomSeed`; 1682 `DungeonEntranceSettings.StyleData`; 1683 `DungeonEntranceSettings.PrecalculateEntrancePosition`; 1684 `PreGenDungeonEntranceSettings.BuriedEntranceYOffset`; 1685 `PreGenDungeonEntranceSettings.BuriedEntranceSandDugoutYOffset`; 1686 `PreGenDungeonEntranceSettings.RoughHeight`; 1687 `PreGenDungeonEntranceSettings.BuryEntrance`
- `SharedDungeonFeatureDefinitions`: 1688 `DungeonFeature.settings`; 1689 `DungeonFeature.Bounds`; 1690 `DungeonFeature.generated`; 1691 `GlobalDungeonFeature.settings`; 1692 `GlobalDungeonFeature.generated`

### C09 - Room lifecycle, settings and geometry

Target role/file: `DungeonRoomDefinition.cs`、`DungeonRoomLifecycleComponent.cs`,
`DungeonRoomGeometryWorkState.cs`、`DungeonRoomQuery.cs`、`DungeonRoomCalculationSystem.cs` 和
`DungeonRoomGenerationSystem.cs`；room system 只写 room 自己的生命周期/几何和 Command buffer。

- `SharedDungeonRoomCoreState`: 1774 `DungeonRoom.settings`; 1775 `DungeonRoom.calculated`; 1776 `DungeonRoom.generated`; 1777 `DungeonRoom.InnerBounds`; 1778 `DungeonRoom.OuterBounds`; 3787 `DungeonRoom.Processed`; 3788 `DungeonRoom.Center`
- `SharedDungeonRoomSettingsState`: 1779 `DungeonRoomSettings.ControlLine`; 1780 `DungeonRoomSettings.RoomPosition`; 1781 `DungeonRoomSettings.RoomType`; 1782 `DungeonRoomSettings.RandomSeed`; 1783 `DungeonRoomSettings.StyleData`; 1784 `DungeonRoomSettings.ProgressionStage`; 1785 `DungeonRoomSettings.StartingRoom`; 1786 `DungeonRoomSettings.OverridePaintTile`; 1787 `DungeonRoomSettings.OverridePaintWall`; 1788 `DungeonRoomSettings.ForceStyleForDoorsAndPlatforms`; 1789 `DungeonRoomSettings.OnCurvedLine`; 1790 `DungeonRoomSettings.Orientation`; 1791 `DungeonRoomSettings.HallwayConnectionPointOverride`; 1792 `DungeonRoomSettings.HallwayPointAdjuster`; 1795 `GenShapeDungeonRoomSettings.ShapeType`; 1796 `GenShapeDungeonRoomSettings.InnerShape`; 1797 `GenShapeDungeonRoomSettings.OuterShape`; 1798 `GenShapeDungeonRoomSettings.BoundingRadius`; 1804 `LegacyDungeonRoomSettings.IsEntranceRoom`; 1808 `StepBasedDungeonRoomSettings.OverrideStrength`; 1809 `StepBasedDungeonRoomSettings.OverrideSteps`; 1810 `StepBasedDungeonRoomSettings.OverrideStartPosition`; 1811 `StepBasedDungeonRoomSettings.OverrideEndPosition`; 1812 `StepBasedDungeonRoomSettings.OverrideVelocity`; 1813 `StepBasedDungeonRoomSettings.OverrideInteriorToExteriorRatio`
- `SharedDungeonRoomShapeGeometryState`: 1755 `BiomeDungeonRoom._innerShapeData`; 1756 `BiomeDungeonRoom._outerShapeData`; 1793 `GenShapeDungeonRoom._innerShapeData`; 1794 `GenShapeDungeonRoom._outerShapeData`; 1799 `LegacyDungeonRoom._innerShapeData`; 1800 `LegacyDungeonRoom._outerShapeData`; 1805 `LivingTreeDungeonRoom._innerShapeData`; 1806 `LivingTreeDungeonRoom._outerShapeData`; 1814 `WormlikeDungeonRoom._innerShapeData`; 1815 `WormlikeDungeonRoom._outerShapeData`; 1816 `WormlikeDungeonRoom.InnerBoundsSizeMin`; 1817 `WormlikeDungeonRoom.InnerBoundsSizeMax`; 1818 `WormlikeDungeonRoom.Positions`
- `SharedDungeonRoomPlacementGeometryState`: 1757 `BiomeRuggedDungeonRoom.Position`; 1758 `BiomeRuggedDungeonRoom.RoomInnerSize`; 1759 `BiomeRuggedDungeonRoom.RoomOuterSize`; 1760 `BiomeRuggedDungeonRoom.WallDepth`; 1761 `BiomeSquareDungeonRoom.Position`; 1762 `BiomeSquareDungeonRoom.RoomInnerSize`; 1763 `BiomeSquareDungeonRoom.RoomOuterSize`; 1764 `BiomeSquareDungeonRoom.WallDepth`; 1770 `BiomeStructuredDungeonRoom.Position`; 1771 `BiomeStructuredDungeonRoom.RoomInnerSize`; 1772 `BiomeStructuredDungeonRoom.RoomOuterSize`; 1773 `BiomeStructuredDungeonRoom.WallDepth`; 1801 `LegacyDungeonRoom.StartPosition`; 1802 `LegacyDungeonRoom.EndPosition`; 1803 `LegacyDungeonRoom.Strength`; 1807 `RegularDungeonRoom._innerBoundsSize`

### C10 - Hall lifecycle, settings and geometry

Target role/file: `DungeonHallDefinition.cs`、`DungeonHallLifecycleComponent.cs`,
`DungeonHallGeometryWorkState.cs`、`DungeonHallQuery.cs`、`DungeonHallCalculationSystem.cs` 和
`DungeonHallGenerationSystem.cs`；hall system 只写 hall lifecycle/geometry 和待提交命令。

- `SharedDungeonHallCoreState`: 1693 `DungeonHall.settings`; 1694 `DungeonHall.calculated`; 1695 `DungeonHall.generated`; 1696 `DungeonHall.Bounds`; 1697 `DungeonHall.StartPosition`; 1698 `DungeonHall.EndPosition`; 1699 `DungeonHall.StartDirection`; 1700 `DungeonHall.EndDirection`; 1701 `DungeonHall.CrackedBrick`; 3786 `DungeonHall.Processed`
- `SharedDungeonHallSettingsState`: 1702 `DungeonHallSettings.HallType`; 1703 `DungeonHallSettings.RandomSeed`; 1704 `DungeonHallSettings.StyleData`; 1705 `DungeonHallSettings.OverridePaintTile`; 1706 `DungeonHallSettings.OverridePaintWall`; 1707 `DungeonHallSettings.CrackedBrickChance`; 1708 `DungeonHallSettings.PlaceOverProtectedBricks`; 1709 `DungeonHallSettings.ZigzagChance`; 1710 `DungeonHallSettings.ForceStyleForDoorsAndPlatforms`; 1711 `DungeonHallSettings.CarveOnly`; 1719 `RegularDungeonHallSettings.OverrideInnerBoundsSize`; 1720 `RegularDungeonHallSettings.OverrideOuterBoundsSize`; 1722 `SineDungeonHallSettings.Magnitude`; 1723 `SineDungeonHallSettings.Iterations`; 1724 `SineDungeonHallSettings.FlipSine`; 1731 `StepBasedDungeonHallSettings.OverrideStrength`; 1732 `StepBasedDungeonHallSettings.OverrideSteps`; 1733 `StepBasedDungeonHallSettings.ForceHorizontal`; 1734 `StepBasedDungeonHallSettings.OverrideInteriorToExteriorRatio`
- `SharedDungeonHallLegacyAndGeometry`: 1712 `LegacyDungeonHall.LastHall`; 1713 `LegacyDungeonHall.Strength`; 1714 `LegacyDungeonHall.Steps`; 1715 `LegacyDungeonHall.OverrideStartPosition`; 1716 `LegacyDungeonHall.OverrideEndPosition`; 1717 `LegacyEntranceDungeonHall.Direction`; 1718 `LegacyEntranceDungeonHallSettings.UsePrecalculatedEntrance`; 1721 `SineDungeonHall.PotentialPlatformPoints`; 1725 `StairwellDungeonHallSettings.MaxDistFromLine`; 1726 `StairwellDungeonHallSettings.PointVariance`; 1727 `StairwellDungeonHallSettings.InnerBoundsSize`; 1728 `StairwellDungeonHallSettings.OuterBoundsSize`; 1729 `StairwellDungeonHallSettings.Gradient`; 1730 `StairwellDungeonHallSettings.IsEntranceHall`

### C11 and C14 - Layout graph and control line

Target role/file: `DungeonLayoutGraphWorkState.cs`、`RoomEntry.cs`、`HallLine.cs`,
`HallwayCalculatorState.cs`、`DungeonLayoutSystem.cs`；control-line-specific members also map to
`DungeonControlLineComponent.cs`、`DungeonControlLineGeometryQuery.cs` 和
`DungeonControlLineSystem.cs`。C11/C14 的关系 ID 和最终 provider order 标记
`crossSubsystemOwner: integration-review`。

- `SharedDungeonLayoutProviderDefinitions` -> C11: 1735 `RoomEntry.room`; 1736 `RoomEntry.progressAlongSnake`; 1737 `RoomEntry.backLinks`; 1738 `RoomEntry.forwardLinks`; 1739 `HallLine.source`; 1740 `HallLine.target`; 1741 `HallLine.sourcePoint`; 1742 `HallLine.targetPoint`; 1743 `HallwayCalculator.data`; 1744 `HallwayCalculator.rooms`; 1745 `HallwayCalculator.halls`; 1746 `HallwayCalculator.stairwells`; 1747 `HallwayCalculator.controlLines`; 1748 `HallwayCalculator.maxProgressDelta`; 1749 `HallwayCalculator.avgLineLength`; 1750 `LegacyDungeonLayoutProviderSettings.Steps`; 1751 `LegacyDungeonLayoutProviderSettings.MaxSteps`
- `SharedDungeonControlLineGeometryState` -> C14: 1430 `DungeonControlLine.Index`; 1431 `DungeonControlLine.Next`; 1432 `DungeonControlLine.Prev`; 1433 `DungeonControlLine.Start`; 1434 `DungeonControlLine.End`; 1435 `DungeonControlLine.StartTangent`; 1436 `DungeonControlLine.EndTangent`; 1437 `DungeonControlLine.StartNormal`; 1438 `DungeonControlLine.EndNormal`; 1439 `DungeonControlLine.CrossTangent`; 1440 `DungeonControlLine.StartRadius`; 1441 `DungeonControlLine.EndRadius`; 1442 `DungeonControlLine.NormalizedDistanceSafeFromDither`; 1443 `DungeonControlLine.StyleTransitionDitherWidth`; 1444 `DungeonControlLine.BorderWidth`; 1445 `DungeonControlLine.NormalizedLineDirection`; 1446 `DungeonControlLine.LineLength`; 1447 `DungeonControlLine.Style`; 1448 `DungeonControlLine.ProgressionStage`; 1449 `DungeonControlLine.CurveLine`; 3746 `DungeonControlLine.Center`

### C12 - Door, platform and trap placement

Target role/file: `DungeonDoorPlacementRequest.cs`、`DungeonPlatformPlacementRequest.cs`,
`DungeonTrapPlacementWorkState.cs`、`DungeonPlacementQuery.cs`、
`DungeonPlacementCommandSystem.cs` 和 `DungeonTileMutationCommand.cs`；callback 只能作为
placement Query port 输入，不能持有可变 tile writer。

- `SharedDungeonDoorAndPlatformDefinitions`: 1867 `DungeonDoorData.Position`; 1868 `DungeonDoorData.OverrideBrickTileType`; 1869 `DungeonDoorData.OverrideBrickWallType`; 1870 `DungeonDoorData.OverrideStyle`; 1871 `DungeonDoorData.Direction`; 1872 `DungeonDoorData.InAHallway`; 1873 `DungeonDoorData.OverrideWidthFluff`; 1874 `DungeonDoorData.SkipOtherDoorsCheck`; 1875 `DungeonDoorData.SkipSpaceCheck`; 1876 `DungeonDoorData.AlwaysClearArea`; 1961 `DungeonPlatformData.Position`; 1962 `DungeonPlatformData.OverrideStyle`; 1963 `DungeonPlatformData.OverrideMaxLengthAllowed`; 1964 `DungeonPlatformData.OverrideHeightFluff`; 1965 `DungeonPlatformData.InAHallway`; 1966 `DungeonPlatformData.ForcePlacement`; 1967 `DungeonPlatformData.SkipOtherPlatformsCheck`; 1968 `DungeonPlatformData.SkipSpaceCheck`; 1969 `DungeonPlatformData.PlaceBooksChance`; 1970 `DungeonPlatformData.NoWaterbolt`; 1971 `DungeonPlatformData.PlacePotsChance`; 1972 `DungeonPlatformData.PlaceWaterCandlesChance`; 1973 `DungeonPlatformData.PlacePotionBottlesChance`; 1974 `DungeonPlatformData.canPlaceHereCallback`; 3800 `DungeonPlatformData.IsAShelf`
- `SharedDungeonTrapPlacementState`: 1404 `DartTrapPlacementAttempt.directionX`; 1405 `DartTrapPlacementAttempt.xPush`; 1406 `DartTrapPlacementAttempt.x`; 1407 `DartTrapPlacementAttempt.y`; 1408 `DartTrapPlacementAttempt.position`; 1409 `DartTrapPlacementAttempt.t`; 1410 `BoulderPlacementAttempt.position`; 1411 `BoulderPlacementAttempt.yPush`; 1412 `BoulderPlacementAttempt.requiredHeight`; 1413 `BoulderPlacementAttempt.bestType`; 1414 `WirePlacementAttempt.position`; 1415 `WirePlacementAttempt.dirX`; 1416 `WirePlacementAttempt.dirY`; 1417 `WirePlacementAttempt.steps`; 1418 `ExplosivePlacementAttempt.position`; 1419 `DeadMansChestBiome._dartTrapPlacementSpots`; 1420 `DeadMansChestBiome._wirePlacementSpots`; 1421 `DeadMansChestBiome._boulderPlacementSpots`; 1422 `DeadMansChestBiome._explosivePlacementAttempt`; 1423 `DeadMansChestBiome._numberOfDartTraps`; 1424 `DeadMansChestBiome._numberOfBoulderTraps`; 1425 `DeadMansChestBiome._numberOfStepsBetweenBoulderTraps`

### C13 - Furniture, style objects and room variants

Target role/file: `DungeonFurnitureCatalog.cs`、`DungeonObjectStyleCatalog.cs`,
`DungeonBannerTrapCatalog.cs`、`DungeonRoomVariantCatalog.cs` 和 `DungeonObjectStyleQuery.cs`；
这些成员全部是 definition/catalog/query 输入，不能注册为 gameplay entity。

- `SharedDungeonStyleFurnitureCatalogState`: 1886 `DungeonGenerationStyleData.WindowPlatformItemTypes`; 1889 `DungeonGenerationStyleData.LockedBiomeChestType`; 1890 `DungeonGenerationStyleData.LockedBiomeChestStyle`; 1891 `DungeonGenerationStyleData.BiomeChestItemType`; 1892 `DungeonGenerationStyleData.BiomeChestLootItemType`; 1893 `DungeonGenerationStyleData.ChestItemTypes`; 1894 `DungeonGenerationStyleData.DoorItemTypes`; 1895 `DungeonGenerationStyleData.PlatformItemTypes`; 1896 `DungeonGenerationStyleData.ChandelierItemTypes`; 1897 `DungeonGenerationStyleData.LanternItemTypes`; 1898 `DungeonGenerationStyleData.TableItemTypes`; 1899 `DungeonGenerationStyleData.WorkbenchItemTypes`; 1900 `DungeonGenerationStyleData.CandleItemTypes`; 1901 `DungeonGenerationStyleData.VaseOrStatueItemTypes`; 1902 `DungeonGenerationStyleData.BookcaseItemTypes`; 1903 `DungeonGenerationStyleData.ChairItemTypes`; 1904 `DungeonGenerationStyleData.BedItemTypes`; 1905 `DungeonGenerationStyleData.PianoItemTypes`; 1906 `DungeonGenerationStyleData.DresserItemTypes`; 1907 `DungeonGenerationStyleData.SofaItemTypes`; 1908 `DungeonGenerationStyleData.BathtubItemTypes`; 1909 `DungeonGenerationStyleData.LampItemTypes`; 1910 `DungeonGenerationStyleData.CandelabraItemTypes`; 1911 `DungeonGenerationStyleData.ClockItemTypes`; 1912 `DungeonGenerationStyleData.BannerItemTypes`
- `SharedDungeonStyleObjectConstants`: 1975 `DungeonUtils.DOORSTYLE_WOODEN`; 1976 `DungeonUtils.DOORSTYLE_BLUEBRICK`; 1977 `DungeonUtils.DOORSTYLE_GREENBRICK`; 1978 `DungeonUtils.DOORSTYLE_PINKBRICK`; 1979 `DungeonUtils.POTSTYLE_NORMAL_1`; 1980 `DungeonUtils.POTSTYLE_NORMAL_2`; 1981 `DungeonUtils.POTSTYLE_NORMAL_3`; 1982 `DungeonUtils.POTSTYLE_NORMAL_4`; 1983 `DungeonUtils.POTSTYLE_SKULL_1`; 1984 `DungeonUtils.POTSTYLE_SKULL_2`; 1985 `DungeonUtils.POTSTYLE_SKULL_3`; 1986 `DungeonUtils.CHANDELIERSTYLE_BLUEBRICK`; 1987 `DungeonUtils.CHANDELIERSTYLE_GREENBRICK`; 1988 `DungeonUtils.CHANDELIERSTYLE_PINKBRICK`; 1989 `DungeonUtils.PLATFORMSTYLE_BLUEBRICK`; 1990 `DungeonUtils.PLATFORMSTYLE_GREENBRICK`; 1991 `DungeonUtils.PLATFORMSTYLE_PINKBRICK`; 1992 `DungeonUtils.PLATFORMSTYLE_METALSHELF`; 1993 `DungeonUtils.PLATFORMSTYLE_BRASSSHELF`; 1994 `DungeonUtils.PLATFORMSTYLE_WOODSHELF`; 1995 `DungeonUtils.PLATFORMSTYLE_DUNGEONSHELF`
- `SharedDungeonStyleBannerAndTrapConstants`: 1996 `DungeonUtils.BANNERSTYLE_BRICK_MARCHINGBONES`; 1997 `DungeonUtils.BANNERSTYLE_BRICK_NECROMANTICSIGN`; 1998 `DungeonUtils.BANNERSTYLE_SLAB_RUGGEDCOMPANY`; 1999 `DungeonUtils.BANNERSTYLE_SLAB_RAGGEDBROTHERHOOD`; 2000 `DungeonUtils.BANNERSTYLE_TILES_MOLTENLEGION`; 2001 `DungeonUtils.BANNERSTYLE_TILES_DIABOLICSIGIL`; 2002 `DungeonUtils.TRAPTYPE_DART`
- `SharedDungeonRoomVariantCatalogState`: 1752 `BiomeDungeonRoom.BIOMEROOM_INNER_SIZE_BASE`; 1753 `BiomeDungeonRoom.BIOMEROOM_INNER_SIZE_BASE_TEMPLE`; 1754 `BiomeDungeonRoom.BIOMEROOM_WALL_DEPTH`; 1765 `BiomeStructuredDungeonRoom.VARIANT_DOUBLEDIAMOND`; 1766 `BiomeStructuredDungeonRoom.VARIANT_ROUNDED`; 1767 `BiomeStructuredDungeonRoom.VARIANT_CANDY`; 1768 `BiomeStructuredDungeonRoom.VARIANT_WIGGLED`; 1769 `BiomeStructuredDungeonRoom.MAX_VARIANTS`
- `SharedDungeonStyleRoomVariantState`: 1914 `DungeonGenerationStyleData.BiomeRoomType`; 1915 `DungeonGenerationStyleData.SubStyles`

### C16 - Spawn configuration

Target role/file: `ExtraSpawnPolicyDefinition.cs`、`ExtraSpawnPointState.cs`,
`ExtraSpawnGenerationSystem.cs`、`ExtraSpawnBinaryAdapter.cs`、`IExtraSpawnTransportPort.cs` 和
`NpcSpawnOverrideValue.cs`。计算、持久化和网络编码分离；最终 spawn owner 标记
`crossSubsystemOwner: integration-review`。

- `WorldSpawnConfigurationState`: 2410 `ExtraSpawnPointManager.extraSpawnPoints`; 2411 `ExtraSpawnPointManager.settings`; 2412 `ExtraSpawnPointManager._listOfLandmasses`; 2413 `ExtraSpawnSettings.spawnType`; 2414 `ExtraSpawnSettings.surface`; 2415 `ExtraSpawnSettings.remix`; 2416 `ExtraSpawnSettings.roundLandmass`; 2417 `ExtraSpawnSettings.skyblock`; 2418 `ExtraSpawnSettings.extraLiquid`; 3349 `NPCSpawnParams.sizeScaleOverride`; 3350 `NPCSpawnParams.playerCountForMultiplayerDifficultyOverride`; 3351 `NPCSpawnParams.difficultyOverride`

### C17 - Scene biome-zone projection

Target role/file: `SceneBiomeZoneQueryPort.cs`、`SceneBiomeZoneSnapshotProjection.cs` 和
`GenerationPresentationProjection.cs`。P03 只产生带 center/tick/revision 的读取快照，不直接
写 SceneMetrics，也不让 scene projection 反向驱动生成。

- `SharedSceneBiomeZoneDefinitionState`: 3442 `SceneMetrics.ZoneCorrupt`; 3443 `SceneMetrics.ZoneCrimson`; 3444 `SceneMetrics.ZoneHallow`; 3445 `SceneMetrics.ZoneJungle`; 3446 `SceneMetrics.ZoneSnow`; 3447 `SceneMetrics.ZoneDesert`; 3448 `SceneMetrics.ZoneGlowshroom`; 3449 `SceneMetrics.ZoneMeteor`; 3450 `SceneMetrics.ZoneGraveyard`; 3451 `SceneMetrics.ZoneDungeon`; 3452 `SceneMetrics.ZoneLihzhardTemple`; 3453 `SceneMetrics.ZoneGranite`; 3454 `SceneMetrics.ZoneMarble`; 3455 `SceneMetrics.ZoneHive`; 3456 `SceneMetrics.ZoneGemCave`; 3457 `SceneMetrics.ZoneBeach`; 3458 `SceneMetrics.ZoneUndergroundDesert`

### 3.1 Mapping invariants

- The mapping is intended to contain exactly 485 source records: 456 fields and 29 properties.
- Every listed source index is mapped once at its source-inventory location. C13/C12 and C13/C06
  labels describe a proposed consuming role, not duplicate ownership of the source member.
- Indexes 1423, 1424, 1425, 1429, 1825, 3738-3744 and 3747 remain `version-drift`; a later
  source-report regeneration must confirm declaration and lifecycle before implementation.
- Source index order is an audit key only. It must never define ECS registration order or System
  scheduling order.

## 4. Implementation order and checkpoint gates

The implementation is proposed as small reviewable steps. Each step must be completed and recorded
in both second-round documents before the next one begins. No step may change `executionStatus` to
`implemented` in this plan until the corresponding code, focused tests and integration decision are
available.

| Order | Checkpoints | Planned action | Entry gate | Exit gate |
| ---: | --- | --- | --- | --- |
| 0 | C18 preflight | Freeze inventory hash, regenerate or explicitly accept the missing first-round evidence gap, and confirm all cross-subsystem owners. | Runner keeps P03 claimed; no active compile process is touched. | Coverage report and owner decision exist, or the work stops as blocked before code. |
| 1 | C01-C02 | Add host/menu/progress ports and support work-state shapes. Keep Main, graphics, time, logging and feedback behind adapters. | Existing WorldGeneration plan/pass/random components are read and their ownership is recorded. | Pure support tests pass; no Dungeon or Tile write is reachable from a Query. |
| 2 | C03-C04 | Add biome structure definitions, terrain/magma/dunes work state and pass systems. Emit structure/tile Commands only. | StructureMap and Tile mutation port contract is approved. | Terrain/ecology snapshot and command batch boundaries are deterministic and explicit. |
| 3 | C05-C06 | Add bounds/progression queries and immutable Dungeon style/material catalogs. | World bounds and content catalog IDs are approved. | Bounds invariant tests pass; no derived property is independently stored as authority. |
| 4 | C07 | Add one Dungeon generation context owner and legacy projection. Remove any possibility of two writable `DungeonData`/`GenVars` representations. | integration-review selects context/GenVars owner and compatibility direction. | Setup, reset, retry and cleanup tests pass without hidden static state. |
| 5 | C08-C11 | Add entrance/feature, room, hall and layout graph work entities. Keep graph, geometry and lifecycle short-lived. | Entity relation IDs and ephemeral lifetime are approved. | Calculate-before-generate and explicit layout order are testable; all work buffers have cleanup. |
| 6 | C12-C14 | Add door/platform/trap Commands, furniture catalogs and control-line queries. | Placement eligibility and callback adapter contract is approved. | Command batches contain no direct world writes; catalog data remains immutable. |
| 7 | C15 | Integrate orchestration and the sole Tile/Wall/Liquid/StructureMap commit owner. Add checkpoint and compensation protocol. | Transaction, revision, rollback and retry decisions are closed. | One successful batch produces one world revision and one projection; failed batches leave revision unchanged. |
| 8 | C16-C17 | Add ExtraSpawn calculation and transport adapters, then SceneMetrics read projections. | Spawn persistence/network owner and SceneMetrics snapshot owner are approved. | Protocol round trips and snapshot invalidation tests pass; projections cannot write generation state. |
| 9 | C18 | Run focused static checks, tests, affected-project serial build, artifact check and no-build verifier. | All evidence gaps that affect code are closed or explicitly recorded as blocking. | Verification report records command, project, exit code, warning/error counts and `Build/bin` artifact. |

### 4.1 Explicit System order

The following order is a proposed scheduling contract and must be encoded by an orchestrator or
explicit dependencies, never by file or directory order:

1. `WorldGenerationSessionSystem` creates the generation aggregate, validates WorldDescriptor and
   WorldGenerationPlan, binds the deterministic random stream and progress sink.
2. `TerrainAndBiomePassSystem` runs terrain/biome passes, stages mutations and publishes terrain
   and ecology snapshots only after a pass commit.
3. `DungeonRuleSetupSystem` resolves GenVars compatibility inputs and style snapshots without
   mutating the style catalogs.
4. `DungeonControlLineSystem` creates and indexes control lines when the dual-dungeon plan needs
   them.
5. `DungeonLayoutSystem` selects a provider, calculates the room/hall graph and records explicit
   links.
6. `DungeonRoomCalculationSystem` calculates room bounds, shapes and connection queries.
7. `DungeonRoomGenerationSystem` emits room feature, furniture, door/platform and trap Commands.
8. `DungeonHallCalculationSystem` calculates hall geometry and protected-area constraints.
9. `DungeonHallGenerationSystem` emits hall tile/wall/platform Commands.
10. `DungeonFeatureAndTrapSystem` validates and batches feature, chest, wire, boulder and explosive
    placement Commands.
11. `WorldTileCommitSystem` is the sole proposed writer for Tile, wall, liquid, frame and
    StructureMap mutations.
12. `ExtraSpawnGenerationSystem` calculates spawn points after landmass candidates are stable;
    binary/network projection runs only after successful world commit.
13. `WorldGenerationProgressProjection` and `SceneBiomeZoneSnapshotProjection` publish read-only
    snapshots.
14. `WorldGenerationFinalizeSystem` marks the pass complete, releases transient work buffers and
    hands committed results to persistence, liquid and NPC integration ports.

The final order of terrain, dungeon, liquid, persistence, NPC, content and diagnostics remains
`crossSubsystemOwner: integration-review` until the dependent partitions agree.

## 5. State ownership and single-write rules

| State or effect | Single proposed writer | Readers | Required rule |
| --- | --- | --- | --- |
| World generation lifecycle and pass stage | `WorldGenerationOrchestrationSystem` | pass systems, progress projection | Only the orchestrator advances stage; progress callbacks cannot advance it. |
| Deterministic random cursor | existing `WorldGenerationRandomStateComponent` through `IGenerationRandomPort` | setup, layout, room/hall/feature systems | No direct `WorldGen.genRand` access from Components or Queries; retry names the cursor/revision explicitly. |
| Dungeon context and GenVars compatibility state | `DungeonGenerationSystem` | layout, room, hall, feature systems | One authority; legacy view is read-only and invalidated at iteration cleanup. `crossSubsystemOwner: integration-review`. |
| Dungeon bounds | `DungeonBoundsMutationSystem` | bounds/placement/layout Queries | Left/Right/Top/Bottom are written once per mutation; hitbox and derived values are recalculated, not independently written. |
| Style/material/furniture catalogs | content/catalog adapter during initialization | all dungeon calculation systems | Immutable after registration; generated rooms cannot change catalog entries. `crossSubsystemOwner: integration-review`. |
| Room/hall/feature lifecycle | respective lifecycle system | layout and generation systems | Calculate and Generate are explicit transitions; `Processed` is derived and cannot be used as a second authority. |
| Placement commands | room/hall/feature/placement systems append to a revisioned batch | placement validator and commit system | Commands have no external effects until accepted by the sole commit owner. |
| Tile/wall/liquid/StructureMap/chest/wire effects | `WorldTileCommitSystem` through `IWorldTileMutationPort` | projections, persistence and integration ports | No direct mutation from Query, catalog, room, hall or feedback code. Transaction and rollback are integration blockers. |
| Extra spawn calculation | `ExtraSpawnGenerationSystem` | transport and NPC integration | Calculation does not serialize or send; adapter performs protocol work after commit. `crossSubsystemOwner: integration-review`. |
| Scene biome flags | SceneMetrics owner outside P03; P03 `SceneBiomeZoneSnapshotProjection` only | NPC, player, UI and diagnostics | P03 cannot write SceneMetrics or use the projection as a generation input. `crossSubsystemOwner: integration-review`. |
| Progress, logging, clocks, graphics and UI | adapters/projections | host and client/UI | Effects are ordered after state transition and cannot become authoritative state. |

Queries must be referentially transparent with respect to world state: geometry, bounds,
placement eligibility, style selection and scene-zone queries must not write Tiles, StructureMap,
random state, progress, persistence or logs. Adapters may perform I/O only at named ports, with
failure and retry outcomes returned to the owning System.

## 6. Compatibility, dual-write and migration policy

The migration must not run two authoritative implementations indefinitely.

1. **Read-only legacy bridge.** During the compatibility window, a proposed
   `DungeonLegacyReadProjection` may expose the new context to existing Version4-shaped callers.
   It is generated from `DungeonGenerationContextComponent` at setup and iteration boundaries. A
   caller may not mutate the projection or use it as a second store.
2. **Single calculation path.** The active path must be selected per generation session. A shadow
   path may calculate a room/hall/style/command result for comparison, but it must use an isolated
   random cursor and must never write Tile, StructureMap, progress, persistence or network state.
3. **No production dual-write.** The legacy `DungeonData`/`DungeonGenVars` view and the new ECS
   context must not both write the same field. If a legacy caller still requires a value, the
   context owner refreshes the read projection; the caller does not write back.
4. **Pass-level cutover.** Cut over at a whole generation pass or iteration boundary, never in the
   middle of a room/hall commit. The pass records implementation mode, seed, random cursor,
   context revision and command-batch revision for diagnostics.
5. **Compatibility serialization.** A versioned adapter may read an existing legacy save into a
   migration input DTO, validate it, and construct the new snapshot. After the agreed format
   version, only the new canonical snapshot is written. The adapter owns field renames and missing
   defaults; Components do not know BinaryReader/BinaryWriter or file layout.
6. **Network compatibility.** Clients receive progress, committed world revisions and immutable
   spawn/scene snapshots. They never receive mutable DungeonData and never issue direct Tile
   mutations. Network IDs remain distinct from EntityId, PersistentWorldId and TileCoordinate.
7. **Observability.** Compatibility comparisons record structured mismatches with pass, iteration,
   source index, random cursor and revision. Logging cannot alter retry or commit decisions.

The 13 version-drift indexes must stay in the compatibility report as unresolved until the source
inventory is regenerated or integration-review explicitly accepts their declaration evidence.

## 7. Persistence, snapshot and network migration

| Data | Proposed representation | Migration rule | Owner status |
| --- | --- | --- | --- |
| Generation session | `WorldGenerationPlan` + lifecycle/pass/random snapshots | Persist seed, plan identity, pass/revision and deterministic cursor only when restart semantics are approved. | Existing WorldSession owner plus `integration-review`. |
| Dungeon context | context snapshot containing definitions, scalar state and iteration metadata | Persist only fields needed for an approved restart or final world metadata; never persist room/hall geometry caches by default. | `DungeonGenerationSystem` candidate; unresolved. |
| Room/hall/feature work | transient ECS/work records with revision and relation IDs | Rebuild on retry/load from approved seed and inputs, or explicitly version a checkpoint; do not silently serialize transient inheritance graphs. | Unresolved. |
| Tile/wall/liquid/StructureMap | committed world section revisions | Serialize only after `WorldTileCommitSystem` success; failed batches must not advance storage revision. | WorldStorage/Tile owner, `crossSubsystemOwner: integration-review`. |
| Extra spawn points | versioned value payload through `ExtraSpawnBinaryAdapter` and transport port | Decide whether points persist, replicate, or regenerate. Empty and incompatible payloads must be explicit states. | `crossSubsystemOwner: integration-review`. |
| Scene biome zones | immutable snapshot `{center, tick, revision, flags}` | Do not persist as authoritative world data unless SceneMetrics owner approves a cache contract. | `crossSubsystemOwner: integration-review`. |
| Progress and diagnostics | projection/event stream | Never restore progress as world authority; restart from a named checkpoint or a new generation session. | Adapter/projection boundary. |

Required snapshot invariants:

- EntityId is transient ECS identity; PersistentWorldId is durable world identity; WorldSectionId
  identifies storage scope; TileCoordinate identifies a location; NetworkId identifies transport;
  external seed/file/cloud IDs remain adapter data. None substitutes for another.
- A snapshot carries schema/version, owner revision and source mode. Unknown fields are rejected or
  preserved only by the adapter according to the approved compatibility policy.
- A snapshot cannot contain mutable references to Main, WorldGen, SceneMetrics, Tile, StructureMap,
  `DungeonData`, `DungeonGenVars`, BinaryReader or BinaryWriter.

## 8. Failure, rollback and recovery

### 8.1 Proposed failure conditions

Stop before implementation or integration if any of the following remains unresolved:

- inventory coverage is not exactly 485 records or a source index appears twice;
- a Component, Query or Projection obtains a direct write path to Tile, StructureMap, random,
  progress, persistence, network or UI;
- two systems can write Dungeon context, bounds, GenVars, style catalogs or SceneMetrics;
- room/hall relation IDs, world transaction revisions or ExtraSpawn serialization semantics are
  missing;
- deterministic random consumption differs between the approved path and the compatibility path;
- a failed command batch can partially commit without a defined compensating operation;
- a focused verifier cannot establish Calculate-before-Generate, cleanup, retry idempotence or
  snapshot revision behavior;
- the affected project does not build through the repository serial runner or its artifact is not
  under `Build/bin/`.

### 8.2 Proposed recovery steps

1. Stop at the last accepted checkpoint; do not start the next System or write path.
2. Preserve the failing input: seed/plan, pass and iteration, source index, random cursor, owner
   revision, command batch and error code. Do not mutate the world to make the failure disappear.
3. If no Tile commit occurred, discard the transient room/hall/feature work buffers and restore the
   previous context/random checkpoint. A retry must state whether it reuses the cursor or starts a
   new generation ID.
4. If a Tile commit failed before the owner revision advanced, the transaction adapter must prove
   that no partial change is visible before retry. If it cannot, stop and escalate to
   `integration-review`; do not guess a rollback by rereading mutable global state.
5. If a committed revision is valid but a later projection/transport failed, keep the committed
   world revision and retry the idempotent projection/transport with the same revision. Never
   regenerate geometry solely to resend a projection.
6. During development, revert only the migration change set owned by this task using ordinary
   reviewable commits. Do not reset the shared checkout, delete broad output directories, edit the
   ledger by hand, delete the runner lock, or revert unrelated user/session changes.
7. Record the failure and recovery result in the next approved execution checkpoint before
   resuming. If the owner decision is still missing, use the same claimed session to report a
   blocker rather than implementing a speculative owner.

## 9. Focused verification and static checks

The full inventory, domain, and static verifier checks remain planned and were not run in this
checkpoint. The affected Component project build and the existing verifier-project build attempt
are recorded in `verificationEvidence` above; the latter failed before a verifier run.

### 9.1 Inventory and design checks

- Parse the input report and count 456 fields plus 29 properties.
- Extract source indexes from this execution mapping and the design mapping; assert exactly 485
  unique indexes, no missing source index and no duplicate source index.
- Assert the 13 version-drift indexes are still explicitly marked and do not silently become
  verified.
- Assert both documents have identical `partitionId`, `sessionId`, `inputReport`, checkpoint list,
  `currentComponent`, `pendingComponents` and `lastCheckpointUtc`.
- Assert `executionStatus: planned`, `implementationStatus: in-progress` and
  `verificationStatus: partial` match the synchronized implementation checkpoint until the
  remaining Component and verification work is separately accepted.

### 9.2 Domain tests

- Bounds: clamp to world dimensions; reject non-positive rectangles; calculate a positive hitbox;
  verify Reset, Contains, Intersects and derived X/Y/Width/Height/Center.
- Determinism: same world seed, pass, iteration and random cursor produce the same style, room,
  hall, trap and ExtraSpawn decisions; different named cursors do not share hidden state.
- Layout: provider selection is explicit; RoomEntry/HallLine relations are valid; protected
  bounds are respected; catalog definitions are not mutated.
- Lifecycle: Calculate precedes Generate; Processed is derived; cleanup invalidates transient
  revisions; retry does not duplicate a committed command.
- Query purity: geometry, bounds, eligibility, style and scene-zone queries write no authority or
  effects.
- Tile transaction: rejected/failed batch leaves world revision unchanged; successful batch
  advances exactly one revision and emits one immutable projection.
- ExtraSpawn: team point count, coordinate range, empty state, binary round trip and network mode.
- Progress/projection: monotonic progress per pass, idempotent reset, and scene-zone snapshots
  include center/tick/revision.

### 9.3 Static repository checks

The future static pass should reject:

- direct `WorldGen.genRand`, Tile, wall, liquid, StructureMap, BinaryReader/BinaryWriter,
  SceneMetrics or Main writes from proposed Components, Queries or Projections;
- a public type whose core file name does not match its PascalCase type;
- a proposed generic `Shared/Components/` directory or a file that combines unrelated domain
  responsibilities;
- a second writer for `DungeonGenerationContext`, `DungeonBounds`, style catalogs, GenVars,
  ExtraSpawn policy or scene-zone snapshots;
- Query methods that mutate fields, append commands, log, advance progress or consume random state;
- implicit ordering that depends on directory enumeration or file name sorting;
- persistence/network payloads that conflate EntityId, PersistentWorldId, WorldSectionId,
  TileCoordinate, NetworkId or external IDs.

## 10. Serial build and test plan

No compile-capable command was run in this session. When implementation exists and integration
owners approve the contracts, every compile-capable command must run from repository root through
the serial runner, after inspecting active `dotnet.exe`/`csc.exe` processes and waiting for any
unclear owner. Do not use raw `dotnet`, parallel jobs, background builds, solution-wide builds or
`-t:Rebuild` for incremental work.

Planned affected project:

`src2/WorldSession/Terraria.WorldSession.csproj`

Planned commands, to be executed one at a time only after code exists:

```powershell
pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 `
  build .\src2\WorldSession\Terraria.WorldSession.csproj `
  -m:1 -nr:false `
  -p:UseSharedCompilation=false `
  -p:MSBuildNodeReuse=false `
  -p:BuildInParallel=false
```

Then verify the expected affected-project artifact is under `Build/bin/`, record the exact build
command, project, exit code, warning count, error count and artifact path, and run focused verifiers
with `--no-build --no-restore` through the same runner. The exact test project and test filter are
deferred until the repository test layout and proposed C# files exist. If compile-capable commands
overlap, treat `Build/bin/` and `Build/obj/` as untrusted, wait for all owners, rebuild the affected
project serially and rerun its verifier.

The build gate does not close any of the current ownership/evidence gaps. A successful compile alone
would not prove behavior equivalence, network compatibility, persistence compatibility or the
correctness of Tile transaction rollback.

## 11. Evidence gaps and blocking decisions

### Evidence gaps

- The first-round P03 public-decomposition output report is missing, so no first-round member-level
  reader/writer/lifecycle audit can be treated as existing evidence.
- Version4 stubs and reduced method bodies leave some generation behavior only partially observed;
  complete-reference code supplies call shape, not NLTX implementation proof.
- The 13 version-drift rows require source-report regeneration or explicit acceptance.
- Tile/WorldStorage transaction boundaries, StructureMap/chest/wire ownership, world-section
  revisions, random persistence, checkpoint recovery and ExtraSpawn payload compatibility are not
  closed.
- SceneMetrics, NPCSpawnParams, AmbientWindSystem, ChumBucketProjectileHelper and after-party
  state cross domain boundaries; P03 proposes ports/projections but cannot assign their final owner.

### Blocking decisions

- Select one authoritative writer for DungeonGenerationContext and legacy GenVars projection.
- Decide whether room/hall/feature work entities are ephemeral ECS entities or indexed world records,
  and define their relation IDs and cleanup semantics.
- Define TileMap/WorldStorage commit, rollback, partial-success, checkpoint-revision and retry
  semantics before C15 implementation.
- Decide whether ExtraSpawn points are persistent, replicated, or regenerated from rules on load;
  keep the answer consistent with BinaryReader/BinaryWriter and network adapters.
- Confirm whether SceneBiomeZone flags are a P03 read projection or a P16-owned authoritative
  scene snapshot, and define revision/tick invalidation.
- Confirm final pass order with WorldSession, WorldStorage, Liquid, NPC, Content and diagnostics
  integration sessions. Directory/file order is not an acceptable substitute.

## 12. Integration Handoff

subsystemId: P03-world-generation-dungeons
taskNumber: 03/20
reportPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\non-authoritative\2026-09-11-version4-non-authoritative-P03-world-generation-dungeons-component-design.md
executionPlanPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\non-authoritative\2026-09-11-version4-non-authoritative-P03-world-generation-dungeons-component-execution.md

evidenceStatus: source inventory confirmed; Version4 and complete-reference boundaries partially confirmed; first-round artifact missing
nltxStatus: current NLTX has world-generation plan/pass/random/terrain/ecology/descriptor and liquid handoff types, plus implemented P03 C01-C04, C05-C14 and C16 local boundaries; C15, C17 and C18 remain pending
verificationStatus: partial

confirmedOwners:
- Version4 `DungeonBounds` owns observed clamped bounds and derived hitbox behavior inside the
  source dungeon generation code.
- Version4 `DungeonCrawler` owns the observed setup/layout/generate lifecycle inside the source
  reference.
- Version4 `WorldGen` owns the observed pass registration and `MakeDungeon` invocation boundary.
- Version4 `SceneMetrics` owns observed scan/reset behavior, while the final NLTX owner remains
  unresolved.

proposedExecutionOwners:
- `WorldGenerationOrchestrationSystem`: generation session and pass lifecycle only.
- `DungeonGenerationSystem`: one Dungeon context and legacy GenVars projection writer.
- `DungeonBoundsMutationSystem`: one bounds writer.
- `WorldTileCommitSystem` through `IWorldTileMutationPort`: one Tile/wall/liquid/StructureMap
  transaction writer.
- `ExtraSpawnGenerationSystem`: spawn calculation; binary/network adapters remain external.
- `SceneBiomeZoneSnapshotProjection`: read-only scene-zone output; it is not SceneMetrics owner.

sharedTypesForIntegrationReview:
- EntityId, PersistentWorldId, WorldSectionId, TileCoordinate, NetworkId and generation revision;
- WorldGenerationProgress, SceneBiomeZoneSnapshot, Dungeon style/content identifiers;
- Tile mutation batch, StructureMap/chest/wire identities and ExtraSpawn payload;
- room/hall/feature relation IDs and checkpoint schema/version.

crossSubsystemReaders:
- WorldStorage/liquid handoff, NPC spawn, Player/SceneMetrics, Content definitions, UI/client
  progress, persistence/network adapters and diagnostics.

crossSubsystemWriters:
- WorldSession plan/rules, the Tile/WorldStorage commit boundary, scene scan, NPC/spawn
  integration, persistence/network adapters and final scheduler.

orderingConstraints:
- Terrain/biome setup precedes dungeon setup; setup precedes control-line/layout; calculation
  precedes generation; validation precedes Tile commit; commit precedes persistence/network
  projection.
- DualDungeon control-line and layout ordering requires explicit integration-review confirmation.

boundaryChallenges:
- Do not retain `DungeonData` as a giant Component; split collections, scalars, definitions, work
  graphs and lifecycle.
- Do not persist geometry caches, style constants, trap attempts or progress as gameplay state.
- Do not let Query, Projection, logging or progress callbacks mutate world generation.
- Do not permit two authoritative writers during a compatibility period.

notImplemented:
- C15、C17、C18 尚未实现；C03、C04、C16 仅完成 Component 部分，C01-C02 与 C05-C14 的 focused verifier、完整编译和运行证据仅覆盖当前本地 verifier，不证明行为等价或跨分区 owner 已闭合。
- Runtime registration, network protocol, save-format migration, Tile/WorldStorage transaction,
  behavior-equivalence and cross-partition owner decisions remain deferred.

verifierPlan:
- Inventory coverage, bounds, deterministic random, layout graph, lifecycle, Query purity, Tile
  transaction, ExtraSpawn protocol and progress/projection checks remain not-run; the affected
  Component project build and failed verifier-project build are recorded above.

本文件描述 proposed 后续实施顺序、目标边界、映射、回滚和验证计划，并记录当前已保存的
Component 与实际构建结果。它不是已迁移完成、已测试、行为等价、API/网络/持久化闭合或
跨分区 owner 已确定的声明。
