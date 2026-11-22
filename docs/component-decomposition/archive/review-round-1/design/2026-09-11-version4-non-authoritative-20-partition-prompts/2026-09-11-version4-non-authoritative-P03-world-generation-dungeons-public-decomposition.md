# Version4 非权威组件拆分分区 P03：世界生成与地牢 - public-decomposition 专属提示词

> 本文件只包含当前分区的专属任务内容，不复制公共提示词正文。
> 使用时：先提供公共提示词，再附加本文件；公共协议中的通用规则、证据门槛、报告结构、禁止项和 Integration Handoff 继续有效。

## 公共前置（仅引用）

- 公共提示词：`D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\design\2026-09-05-version4-public-decomposition-common-protocol.md`
- 必读技能：`D:\TRbackup\NLTX\.agents\skills\public-decomposition\SKILL.md`
- ECS 证据协议：`D:\TRbackup\NLTX\.agents\skills\public-decomposition\references\ecs-evidence-protocol.md`
- tModLoader 检索协议：`D:\TRbackup\NLTX\.agents\skills\public-decomposition\references\tmodloader-documentation-retrieval.md`
- 本文件不重复上述文件内容；执行前必须实际读取并遵守它们。
- 并行会话保障 skill：D:\\TRbackup\\NLTX\\.agents\\skills\\version4-non-authoritative-partition-session\\SKILL.md
- 并行领取/结算 runner：D:\\TRbackup\\NLTX\\Build\\Tools\\Invoke-Version4NonAuthoritativePartitionSession.ps1
- 必须先按 skill 使用 ClaimNext（或用户明确指定时使用 Claim -PartitionId P03），只写本文件的 outputReport，并用领取返回的同一 sessionId 完成 Complete/Fail/Abandon；不得手工编辑 ledger 或 lock。
- 本任务属于 20 个并行会话中的固定分区；currentTaskCount: 20。公共协议中遗留的 19 会话数量说明在本任务包中不适用。

## 任务元数据

- promptId: P03-non-authoritative-public-decomposition-20260911
- partitionId: P03
- inputPartitionReport: D:\TRbackup\NLTX\docs\migration\ledgers\non-authoritative-component-partitions\03-world-generation-dungeons.md
- outputReport: D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P03-world-generation-dungeons-public-decomposition.md
- reportType: public-decomposition
- executionMode: independent-read-only-partition
- currentTaskCount: 20
- formalParentSubsystems: RuntimeComposition, SharedRuntimeMechanisms
- leafSubsystemCount: 35
- fieldCount: 456
- propertyCount: 29
- memberCount: 485
- sourceReportSha256: b944441d92d5103f218b766a3e354200794528a19e65a862e44c2b40a36f1196
- sourceInventoryStatus: source-inventory-confirmed; member-level owner/lifecycle/side-effect evidence still requires this session to close

## 专属范围锁定

- 本会话只审查输入分区报告中的 35 个叶子子系统和 485 条成员记录。以下清单是本会话唯一的成员范围；不得读取或复制其他 P 分区的成员清单。
- 细分子系统仍只是库存边界，不等价于一个组件；允许在本分区内部按访问模式拆成多个 proposed Component、System、Query、Command、Adapter 或 Projection。
- 来源序号、路径、行号和声明是检索线索；必须回到 Version4 重新定位，不得把输入报告直接当作已确认的读者、写者、生命周期或 owner 证据。

| 叶子子系统 | 父级 | 原章节 | 边界角色 | 字段 | 属性 | 成员 | 细分职责 |
|---|---|---|---|---:|---:|---:|---|
| `MainGraphicsAndGenerationState` | `RuntimeComposition` | `4.1.12` | `presentation state` | 12 | 0 | 12 | 图形设备、渲染计数、世界生成进度和保存计时引用。 |
| `MainMenuAndWorldGenerationState` | `RuntimeComposition` | `4.1.40` | `session state` | 7 | 0 | 7 | 菜单物品缩放、世界名称、环境风和自动通过状态。 |
| `SharedWorldGenerationSupport` | `SharedRuntimeMechanisms` | `4.9.3` | `definition/query` | 11 | 4 | 15 | 生成形状、轨道、绘画和通用生成辅助。 |
| `SharedDungeonEntranceDefinitions` | `SharedRuntimeMechanisms` | `4.9.22` | `definition/catalog` | 13 | 0 | 13 | 地牢入口类型、预生成参数和入口设置定义。 |
| `SharedDungeonFeatureDefinitions` | `SharedRuntimeMechanisms` | `4.9.23` | `definition/catalog` | 5 | 0 | 5 | 地牢特征、全局家具、陷阱和装饰定义。 |
| `SharedDungeonLayoutProviderDefinitions` | `SharedRuntimeMechanisms` | `4.9.24` | `definition/catalog` | 17 | 0 | 17 | 地牢布局 provider、双地牢和旧布局 provider 设置定义。 |
| `SharedDungeonLayoutProviderState` | `SharedRuntimeMechanisms` | `4.9.62` | `definition/state` | 2 | 0 | 2 | 根级地牢布局 provider 及其设置状态。 |
| `SharedDungeonCrawlerRuntimeState` | `SharedRuntimeMechanisms` | `4.9.63` | `runtime state` | 1 | 1 | 2 | 地牢爬行器当前数据和生成运行状态。 |
| `WorldSpawnConfigurationState` | `SharedRuntimeMechanisms` | `4.9.100` | `definition/state` | 12 | 0 | 12 | 刷怪参数、额外出生点和 NPC 生成配置。 |
| `SharedBiomeCaveHouseAndStructureState` | `SharedRuntimeMechanisms` | `4.9.116` | `definition/query` | 5 | 7 | 12 | 洞穴房屋、结构放置和洞穴生物群落辅助数据。 |
| `SharedDungeonStyleSetCatalog` | `SharedRuntimeMechanisms` | `4.9.118` | `definition/catalog` | 15 | 0 | 15 | 地牢样式集合和生物群落样式索引。 |
| `SharedDungeonBoundsAndProgressionDefinitions` | `SharedRuntimeMechanisms` | `4.9.123` | `definition/query` | 11 | 9 | 20 | 地牢边界、中心和不可破坏墙进度层级定义。 |
| `SharedDungeonDoorAndPlatformDefinitions` | `SharedRuntimeMechanisms` | `4.9.124` | `definition/query` | 24 | 1 | 25 | 地牢门、平台及其空间检查和放置覆盖数据。 |
| `SharedDungeonHallLegacyAndGeometry` | `SharedRuntimeMechanisms` | `4.9.127` | `definition/catalog` | 14 | 0 | 14 | 旧大厅、阶梯大厅、正弦大厅和入口几何状态。 |
| `SharedDungeonGenerationCollectionsState` | `SharedRuntimeMechanisms` | `4.9.135` | `runtime state` | 19 | 1 | 20 | 地牢迭代、入口、房间、走廊和保护边界集合。 |
| `SharedDungeonGenerationScalarAndStyleState` | `SharedRuntimeMechanisms` | `4.9.136` | `runtime state` | 17 | 0 | 17 | 地牢样式、物品类型和生成强度比例状态。 |
| `SharedDungeonGeometryRuleQueries` | `SharedRuntimeMechanisms` | `4.9.137` | `query` | 8 | 0 | 8 | 大厅/房间深度、放置变化和马赛克规则查询。 |
| `SharedDungeonLegacyPlacementState` | `SharedRuntimeMechanisms` | `4.9.155` | `runtime state` | 17 | 0 | 17 | 旧地牢位置、砖墙类型、边界和入口放置状态。 |
| `SharedDungeonLegacyRuleState` | `SharedRuntimeMechanisms` | `4.9.156` | `runtime state` | 11 | 0 | 11 | 旧地牢样式、瓦片判定、预生成和战利品规则状态。 |
| `SharedDungeonRoomCoreState` | `SharedRuntimeMechanisms` | `4.9.163` | `runtime state` | 5 | 2 | 7 | 地牢房间实例的计算、生成、边界和处理生命周期状态。 |
| `SharedDungeonRoomSettingsState` | `SharedRuntimeMechanisms` | `4.9.164` | `definition/state` | 25 | 0 | 25 | 地牢房间类型、样式、进度和连接点配置。 |
| `SharedDungeonHallCoreState` | `SharedRuntimeMechanisms` | `4.9.165` | `runtime state` | 9 | 1 | 10 | 地牢大厅实例的计算、生成、端点和处理生命周期状态。 |
| `SharedDungeonHallSettingsState` | `SharedRuntimeMechanisms` | `4.9.166` | `definition/state` | 19 | 0 | 19 | 地牢大厅类型、样式、裂砖和生成策略配置。 |
| `SharedBiomeTerrainPassState` | `SharedRuntimeMechanisms` | `4.9.169` | `definition/query` | 14 | 2 | 16 | 生物群落、洞穴、沙漠和地形 pass 的生成数据。 |
| `SharedDungeonStyleMaterialAndGeometryState` | `SharedRuntimeMechanisms` | `4.9.174` | `definition/catalog` | 12 | 0 | 12 | 地牢样式砖、墙、液体、边缘和几何材料定义。 |
| `SharedDungeonRoomVariantCatalogState` | `SharedRuntimeMechanisms` | `4.9.177` | `definition/catalog` | 8 | 0 | 8 | 地牢房间 VARIANT、Biome 房间和最大变体目录。 |
| `SharedDungeonStyleObjectConstants` | `SharedRuntimeMechanisms` | `4.9.184` | `query/value object` | 21 | 0 | 21 | 地牢门、花盆、吊灯和平台对象常量查询。 |
| `SharedDungeonStyleBannerAndTrapConstants` | `SharedRuntimeMechanisms` | `4.9.185` | `query/value object` | 7 | 0 | 7 | 地牢旗帜样式和陷阱类型常量查询。 |
| `SharedDungeonTrapPlacementState` | `SharedRuntimeMechanisms` | `4.9.201` | `definition/query` | 22 | 0 | 22 | 死亡宝箱陷阱点、陷阱数量和放置尝试状态。 |
| `SharedDungeonControlLineGeometryState` | `SharedRuntimeMechanisms` | `4.9.202` | `definition/query` | 20 | 1 | 21 | 地牢控制线节点、切线、半径、方向和样式几何。 |
| `SharedDungeonRoomShapeGeometryState` | `SharedRuntimeMechanisms` | `4.9.210` | `definition/catalog` | 13 | 0 | 13 | 地牢房间内部/外部形状数据和形状尺寸变化。 |
| `SharedDungeonRoomPlacementGeometryState` | `SharedRuntimeMechanisms` | `4.9.211` | `definition/catalog` | 16 | 0 | 16 | 地牢房间位置、边界尺寸、墙深和强度/端点布局。 |
| `SharedDungeonStyleFurnitureCatalogState` | `SharedRuntimeMechanisms` | `4.9.212` | `definition/catalog` | 25 | 0 | 25 | 地牢箱体、门、平台、灯具、家具和装饰物品目录。 |
| `SharedDungeonStyleRoomVariantState` | `SharedRuntimeMechanisms` | `4.9.213` | `definition/catalog` | 2 | 0 | 2 | 地牢生物群落房间类型和子样式关系。 |
| `SharedSceneBiomeZoneDefinitionState` | `SharedRuntimeMechanisms` | `4.9.216` | `query/input` | 17 | 0 | 17 | 场景腐化、猩红、神圣、地形和生物群落区域定义。 |

来源成员的分区内序号线索范围：153..3800；序号不定义领域边界，必须逐条回到输入表和 Version4 覆盖。

## 分区专属目标

围绕“世界生成与地牢”完成独立只读的 Version4 成员审查和 ECS 拆分设计。
- 围绕世界生成 pass、形状/轨道/绘画辅助、地牢入口与布局 provider、房间/大厅几何、陷阱、样式目录、Biome/结构和刷怪配置，核对生成阶段的真实输入、临时状态、提交边界和失败恢复。
- 回到 Terraria/WorldGen.cs 及 Terraria.GameContent.Generation、Dungeon、Biome/Structure 相关源码，重新定位注册、调用闭包、随机使用、Tile 写入和生成进度报告。
- 把地牢定义/目录、运行时布局实例、房间/大厅工作集、几何 Query、对象常量和生成 pass System 分开评估，区分可复用定义、权威世界结果与一次性工作缓冲。
- 检查生成对世界存储、随机种子、事件、网络/加载进度、保存和失败重试的边界；生成阶段的顺序必须写成显式调度契约。

## 专属证据焦点

- 先完整读取本分区输入报告的成员表，再回到 D:\TRbackup\Version4 重新核对字段、属性、声明类型、初始化/注册、直接读者、直接写者、清理路径和副作用。
- 下列路径只作为本分区的检索种子；路径存在不等于成员语义已确认，必须按公共证据协议记录实际行号、符号、调用者、读者、写者、生命周期和 evidenceStatus。
  - `D:\TRbackup\Version4\Terraria.DataStructures/IRoomCheckFeedback_Spread.cs`
  - `D:\TRbackup\Version4\Terraria.DataStructures/NoRoomCheckFeedback.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Biomes.CaveHouse/HouseBuilderContext.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Biomes.CaveHouse/HouseUtils.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Biomes/CaveHouseBiome.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Biomes/DeadMansChestBiome.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Biomes/DesertBiome.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Biomes/DitherSnake.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Biomes/DunesBiome.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Biomes/DungeonControlLine.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Biomes/GraniteBiome.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Biomes/MarbleBiome.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Biomes/TerrainPass.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Entrances/DungeonEntrance.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Entrances/DungeonEntranceSettings.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Entrances/PreGenDungeonEntranceSettings.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Features/DungeonFeature.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Features/GlobalDungeonFeature.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Halls/DungeonHall.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Halls/DungeonHallSettings.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Halls/LegacyDungeonHall.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Halls/LegacyEntranceDungeonHall.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Halls/LegacyEntranceDungeonHallSettings.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Halls/RegularDungeonHallSettings.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Halls/SineDungeonHall.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Halls/SineDungeonHallSettings.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Halls/StairwellDungeonHallSettings.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Halls/StepBasedDungeonHallSettings.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.LayoutProviders/DualDungeonLayoutProvider.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.LayoutProviders/LegacyDungeonLayoutProviderSettings.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms/BiomeDungeonRoom.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms/BiomeRuggedDungeonRoom.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms/BiomeSquareDungeonRoom.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms/BiomeStructuredDungeonRoom.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms/DungeonRoom.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms/DungeonRoomSettings.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms/GenShapeDungeonRoom.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms/GenShapeDungeonRoomSettings.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms/LegacyDungeonRoom.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms/LegacyDungeonRoomSettings.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms/LivingTreeDungeonRoom.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms/RegularDungeonRoom.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms/StepBasedDungeonRoomSettings.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms/WormlikeDungeonRoom.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon/DualDungeonUnbreakableWallTiers.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon/DungeonBounds.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon/DungeonCrawler.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon/DungeonData.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon/DungeonDoorData.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon/DungeonGenerationStyleData.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon/DungeonGenerationStyles.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon/DungeonGenVars.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon/DungeonLayoutProvider.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon/DungeonLayoutProviderSettings.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon/DungeonPlatformData.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon/DungeonUtils.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Generation/PaintingEntry.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Generation/ShapeFloodFill.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Generation/TrackGenerator.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent/ExtraSpawnPointManager.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent/ExtraSpawnSettings.cs`
  - `D:\TRbackup\Version4\Terraria/Main.cs`
  - `D:\TRbackup\Version4\Terraria/NPCSpawnParams.cs`
  - `D:\TRbackup\Version4\Terraria/SceneMetrics.cs`

- 输入分区抽取出的声明类型焦点：
  - `Terraria.DataStructures.IRoomCheckFeedback_Spread`
  - `Terraria.DataStructures.NoRoomCheckFeedback`
  - `Terraria.GameContent.Biomes.CaveHouse.HouseBuilderContext`
  - `Terraria.GameContent.Biomes.CaveHouse.HouseUtils`
  - `Terraria.GameContent.Biomes.CaveHouseBiome`
  - `Terraria.GameContent.Biomes.DeadMansChestBiome`
  - `Terraria.GameContent.Biomes.DeadMansChestBiome.BoulderPlacementAttempt`
  - `Terraria.GameContent.Biomes.DeadMansChestBiome.DartTrapPlacementAttempt`
  - `Terraria.GameContent.Biomes.DeadMansChestBiome.ExplosivePlacementAttempt`
  - `Terraria.GameContent.Biomes.DeadMansChestBiome.WirePlacementAttempt`
  - `Terraria.GameContent.Biomes.DesertBiome`
  - `Terraria.GameContent.Biomes.DitherSnake`
  - `Terraria.GameContent.Biomes.DunesBiome`
  - `Terraria.GameContent.Biomes.DungeonControlLine`
  - `Terraria.GameContent.Biomes.GraniteBiome`
  - `Terraria.GameContent.Biomes.GraniteBiome.Magma`
  - `Terraria.GameContent.Biomes.MarbleBiome`
  - `Terraria.GameContent.Biomes.TerrainPass.SurfaceHistory`
  - `Terraria.GameContent.ExtraSpawnPointManager`
  - `Terraria.GameContent.ExtraSpawnSettings`
  - `Terraria.GameContent.Generation.Dungeon.DualDungeonUnbreakableWallTiers`
  - `Terraria.GameContent.Generation.Dungeon.DungeonBounds`
  - `Terraria.GameContent.Generation.Dungeon.DungeonCrawler`
  - `Terraria.GameContent.Generation.Dungeon.DungeonData`
  - `Terraria.GameContent.Generation.Dungeon.DungeonDoorData`
  - `Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyleData`
  - `Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyles`
  - `Terraria.GameContent.Generation.Dungeon.DungeonGenVars`
  - `Terraria.GameContent.Generation.Dungeon.DungeonLayoutProvider`
  - `Terraria.GameContent.Generation.Dungeon.DungeonLayoutProviderSettings`
  - `Terraria.GameContent.Generation.Dungeon.DungeonPlatformData`
  - `Terraria.GameContent.Generation.Dungeon.DungeonUtils`
  - `Terraria.GameContent.Generation.Dungeon.Entrances.DungeonEntrance`
  - `Terraria.GameContent.Generation.Dungeon.Entrances.DungeonEntranceSettings`
  - `Terraria.GameContent.Generation.Dungeon.Entrances.PreGenDungeonEntranceSettings`
  - `Terraria.GameContent.Generation.Dungeon.Features.DungeonFeature`
  - `Terraria.GameContent.Generation.Dungeon.Features.GlobalDungeonFeature`
  - `Terraria.GameContent.Generation.Dungeon.Halls.DungeonHall`
  - `Terraria.GameContent.Generation.Dungeon.Halls.DungeonHallSettings`
  - `Terraria.GameContent.Generation.Dungeon.Halls.LegacyDungeonHall`
  - `Terraria.GameContent.Generation.Dungeon.Halls.LegacyEntranceDungeonHall`
  - `Terraria.GameContent.Generation.Dungeon.Halls.LegacyEntranceDungeonHallSettings`
  - `Terraria.GameContent.Generation.Dungeon.Halls.RegularDungeonHallSettings`
  - `Terraria.GameContent.Generation.Dungeon.Halls.SineDungeonHall`
  - `Terraria.GameContent.Generation.Dungeon.Halls.SineDungeonHallSettings`
  - `Terraria.GameContent.Generation.Dungeon.Halls.StairwellDungeonHallSettings`
  - `Terraria.GameContent.Generation.Dungeon.Halls.StepBasedDungeonHallSettings`
  - `Terraria.GameContent.Generation.Dungeon.LayoutProviders.DualDungeonLayoutProvider.HallwayCalculator`
  - `Terraria.GameContent.Generation.Dungeon.LayoutProviders.DualDungeonLayoutProvider.HallwayCalculator.HallLine`
  - `Terraria.GameContent.Generation.Dungeon.LayoutProviders.DualDungeonLayoutProvider.HallwayCalculator.RoomEntry`
  - `Terraria.GameContent.Generation.Dungeon.LayoutProviders.LegacyDungeonLayoutProviderSettings`
  - `Terraria.GameContent.Generation.Dungeon.Rooms.BiomeDungeonRoom`
  - `Terraria.GameContent.Generation.Dungeon.Rooms.BiomeRuggedDungeonRoom`
  - `Terraria.GameContent.Generation.Dungeon.Rooms.BiomeSquareDungeonRoom`
  - `Terraria.GameContent.Generation.Dungeon.Rooms.BiomeStructuredDungeonRoom`
  - `Terraria.GameContent.Generation.Dungeon.Rooms.DungeonRoom`
  - `Terraria.GameContent.Generation.Dungeon.Rooms.DungeonRoomSettings`
  - `Terraria.GameContent.Generation.Dungeon.Rooms.GenShapeDungeonRoom`
  - `Terraria.GameContent.Generation.Dungeon.Rooms.GenShapeDungeonRoomSettings`
  - `Terraria.GameContent.Generation.Dungeon.Rooms.LegacyDungeonRoom`
  - `Terraria.GameContent.Generation.Dungeon.Rooms.LegacyDungeonRoomSettings`
  - `Terraria.GameContent.Generation.Dungeon.Rooms.LivingTreeDungeonRoom`
  - `Terraria.GameContent.Generation.Dungeon.Rooms.RegularDungeonRoom`
  - `Terraria.GameContent.Generation.Dungeon.Rooms.StepBasedDungeonRoomSettings`
  - `Terraria.GameContent.Generation.Dungeon.Rooms.WormlikeDungeonRoom`
  - `Terraria.GameContent.Generation.PaintingEntry`
  - `Terraria.GameContent.Generation.ShapeFloodFill`
  - `Terraria.GameContent.Generation.TrackGenerator`
  - `Terraria.GameContent.Generation.TrackGenerator.TrackHistory`
  - `Terraria.Main`
  - `Terraria.NPCSpawnParams`
  - `Terraria.SceneMetrics`

- 若路径、类型或行号漂移，记录 version-drift 或 evidence-mismatch；不得静默沿用库存报告行号。Version4 是 Terraria 私有实现的事实来源，完整源码只补证 Version4 已存在文件，tModLoader 只交叉验证公开边界，SS14 只参考结构粒度。

## 专属审查问题

- WorldGen pass、Dungeon layout provider、Room/Hall 实例和 Geometry Query 的状态所有权分别是什么？哪些状态跨 pass 存活，哪些应在 pass 完成后销毁？
- 样式、家具、陷阱、门、平台和房间变体是定义目录、规则 Query 还是世界写入 Command 的输入？如何防止目录状态被误当作世界权威状态？
- 随机源、生成预算、碰撞/空间检查和 Tile 提交如何保证确定性、失败可重试和部分生成不泄漏？
- 地牢与世界存储、TileObject、事件、NPC 生成及持久化之间哪些接口必须由整合会话决定，不能在本分区单方面定 owner？

## 专属不拆分边界

- 不要把所有地牢定义、房间几何、生成集合和 pass 计时器合成一个巨型 DungeonComponent。
- 不要把 Shape/Geometry 值对象、样式常量、查询结果、单次放置尝试或生成进度日志升格为长期实体组件。
- 不要用文件或目录顺序替代 WorldGen pass 的运行时调度顺序；顺序必须由 System 契约和证据支持。

专属跨域提醒：重点记录与 Tile/WorldStorage、世界会话、NPC 生成、事件、随机/诊断和持久化的 integration-risk；地牢布局、生成结果、Tile 写入和共享随机 owner 均只提出 candidate，标 crossSubsystemOwner: integration-review。

## 输出要求

- 将完整研究报告只写入：D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P03-world-generation-dungeons-public-decomposition.md
- 不要写入输入分区报告、公共提示词、其他分区报告、源代码、测试、项目文件或迁移文件。
- 报告必须逐条覆盖本分区成员，给出成员归属表、状态所有权、Component/System/Query/Command/Adapter/Projection 边界、接口契约、依赖方向、顺序约束、不拆分项、兼容策略、evidence-gap、blocking-decision、focused verifier 计划和 Integration Handoff。
- 本轮只读审查不运行编译型命令；没有实际执行的 verifier 必须写 verificationStatus: not-run。
- proposed 类型、路径和接口必须明确标记 proposed；不得写成当前 NLTX 已存在或已经迁移。
- 跨两个或以上分区使用的类型、接口、ID、快照、值对象和 System 顺序只能提出候选，必须写 crossSubsystemOwner: integration-review，不得在本报告单方面裁决。
- 不得修改任何生产代码、测试、项目文件、生成器、公共协议、分区报告或其他并行会话输出。

## 分区完成检查

- [ ] 逐条覆盖本分区全部 485 条成员，成员事实来自输入表但关键语义已回到 Version4 复核。
- [ ] 只使用本分区叶子清单；没有复制、重新设计或宣布其他 P 分区成员。
- [ ] Component 只承载内聚权威状态；派生值、缓存、查询、短期 payload、日志、网络/存档/UI 输出按正确角色隔离。
- [ ] 所有共享候选写 crossSubsystemOwner: integration-review；没有把 proposed 设计写成已实现能力。
- [ ] 未执行的构建、测试和 verifier 均明确 verificationStatus: not-run；证据缺口、冲突和 blocking-decision 已单列。
- [ ] 输出文件路径唯一为：D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P03-world-generation-dungeons-public-decomposition.md

## 会话结束回报

最终消息只回报以下字段：
- partitionId: P03
- outputReport: D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P03-world-generation-dungeons-public-decomposition.md
- evidenceSourcesRead: 实际读取的 Version4 文件/类型、必要的完整源码补证、tModLoader 页面和 SS14 参考
- evidenceGaps: 仍缺失或冲突的关键证据
- blockingDecisions: 需要最终整合会话裁决的 owner/顺序/事务边界
- verificationStatus: not-run
- productionCodeModified: no

本提示词只定义本分区的专属范围和审查抓手；公共协议负责共同证据规则、报告结构、禁止项和 Integration Handoff。
