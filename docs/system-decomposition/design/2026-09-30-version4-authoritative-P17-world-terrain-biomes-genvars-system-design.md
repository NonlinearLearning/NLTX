# Version4 authoritative P17 World Terrain, Biomes, and GenVars System Design

| 项目 | 内容 |
| --- | --- |
| 分区 | `P17` (`WorldGenerationAndEcology`) |
| 设计状态 | `proposed` |
| 证据状态 | `partial` |
| 验证状态 | `not-run` |
| 源码变更 | `true`（仅局部 P17 组合切片） |
| 基线报告 | [`2026-09-18-system-decomposition-authoritative-P17-world-terrain-biomes-genvars.md`](../reports/2026-09-18-system-decomposition-authoritative-P17-world-terrain-biomes-genvars.md) |
| 原报告 session | `95cb4a271e0346218cb14565179c4840` |
| 当前目标源码 | `D:\TRbackup\Version4` |
| 完整参考源码 | `D:\TRbackup\无任何删减通过编译` |
| SS14 组织参考源码 | `C:\Users\shan\Downloads\ECS\space-station-14-master`（仅观察 System 组织方式） |
| 迁移目标目录 | `D:\TRbackup\NLTX\src\NSSLC` |

designStatus: proposed
evidenceStatus: partial
verificationStatus: not-run
sourceModified: true
implementationStatus: partial
coreSliceVerification: passed

`sourceModified: true` 反映工作区已经存在 P17 局部实现；这些切片覆盖 pass 控制边界、
SavedOreTiers 的 Read→repair→单次 commit 组合，以及 C09 三类 history 的显式接受/提交
结果边界，并有独立的约 10% 核心 verifier 历史记录。它们仍未接入真实生产调度；本次文档
修订只补充静态证据，不改变生产代码或验证状态。

## 1. 目的与边界

本文把 P17 System 报告转换为可执行的目标设计。目标是为 146 个 `GenVars` 成员定义
稳定的概念 System、唯一权威写者、只读 Query、修改 Command、外部 Adapter/Projection
和依赖顺序。它描述未来接入方式，不表示这些 System 已经进入 Version4 运行路径。

在范围内：

- P17 报告中的 14 个叶子组、143 个字段和 3 个属性，合计 146 个成员。
- world-generation session 内的配置、地形测量、生态约束、结构 scratch、dungeon 派生
  API 和七个 SavedOreTiers 值。
- 生成期 snapshot、generation identity、reset/rebuild、失败重试和跨分区 handoff 契约。
- 从 `WorldGen.GenerateWorld`、`WorldGen.Reset`、`WorldGen.AddPasses`、`WorldFile`、
  `Liquid`、`NetMessage`、`DungeonCrawler` 等旧入口到概念 API 的组合映射。

不在范围内：

- P17 不能决定全局 pass coordinator、secret-seed owner、Tile/Wall writer、Liquid
  simulation、NPC spawn、WorldFile/NetMessage 最终 owner 或 P18-P20 的跨分区决策。
- 不把完整参考树或 SS14 组织方式当成 Version4 的行为证明。
- 不把局部 P17 组合切片当成完整生产接入；P19 coordinator、P20 effects、WorldFile、
  Liquid、NetMessage 和其他跨分区 owner 仍不在本设计的实现范围内。
- 不以编译、局部 focused verifier、静态映射或文件存在宣称迁移成功。

## 2. 证据基线

### 2.1 报告与当前目标树

P17 报告已经用原始 authoritative session 结算；本文不重新 claim 分区，也不另造
`outputReport`。报告本身的 `designStatus` 是 `proposed`，`verificationStatus` 是
`not-run`，并明确当前 System 组合未接入真实 scheduler。

Version4 关键源码哈希如下。哈希用于记录本次阅读对象，不等同于完整 CPG snapshot 绑定：

| 文件 | SHA256 | 事实用途 |
| --- | --- | --- |
| `Terraria.WorldBuilding/GenVars.cs` | `E01C69435762ADB532F7CAD6F38AD5CAD97709DC2965060F4A41427FD804C67E` | 146 个 P17 成员声明、属性转发和 `CurrentDungeon` 下限夹紧 |
| `Terraria/WorldGen.cs` | `A06A8463E39EA065441FF1EF702A787D3450ADAE5CD3065CF30BF76784D3EA1D` | 生成入口、Reset、pass 注册、P17 写入/读取 |
| `Terraria.WorldBuilding/WorldGenerator.cs` | `917FFA37464C607EC4A205EAB0E0454A30AF606BCD15DE3500F160F969BF9774` | pass loop；`RunPass` 当前是 stub，效果与异常语义为 `unknown` |
| `Terraria.IO/WorldFile.cs` | `92DF79BB7AE89393327AE9EF6B7B78762909E5B2E197C0F3FCCFE709E152F289` | SavedOreTiers 读写、repair 和 load 后液体边界 |
| `Terraria/Liquid.cs` | `23C27E5B669B99FE225ECFACEBD6F5254A2BA63239B6906CEF2C070010E709B6` | `waterLine`/`lavaLine` 消费和 `waterLine` 写入 |
| `Terraria/NetMessage.cs` | `87B596BD8467B9C470DD1F2AD6963EBADE257689F87395163AB136C149BEBC9A` | 七个 SavedOreTiers 出站 short 顺序 |

### 2.2 完整参考树的使用方式

用户给定前提是 `D:\TRbackup\无任何删减通过编译` 为“无任何删减且通过编译”的完整参考项目；
本任务不重复构建，只把它作为完整源码关系校对对象。它的关键文件哈希为：

| 文件 | SHA256 | 本文使用的关系 |
| --- | --- | --- |
| `Terraria.WorldBuilding/GenVars.cs` | `BB7322DD8F0BDFEF90926B7BA59EF32FC9A8A1E3A9D4042A940E2E10C5DDD102` | 成员声明与属性形状校对 |
| `Terraria/WorldGen.cs` | `B9F7834CE1BC68C1DD9C656574A2272DB6F79E1407D934E1ADA33EDC3C930F82` | `GenerateWorld`、`Reset`、`AddPasses` 的完整语句顺序 |
| `Terraria.WorldBuilding/WorldGenerator.cs` | `56CA4E8F06B13368625CFB3C9B643DB757995B2F4BA1AC8043F9D0712AAB146F` | 非 stub `RunPass` 的行为候选 |
| `Terraria.IO/WorldFile.cs` | `92DF79BB7AE89393327AE9EF6B7B78762909E5B2E197C0F3FCCFE709E152F289` | 与 Version4 相同的 SavedOreTiers 文件布局 |
| `Terraria/Liquid.cs` | `87CA947492146B31D2A282A53114C1EBBB61C74B0129AE1795645B412A605841` | 液体 load barrier 关系校对 |
| `Terraria/NetMessage.cs` | `F3066C50715D7C49B8BF2CC852B015303AD7F4C12ADC191C72FC0FE46181E7E2` | 七值网络序列校对 |

参考树显示 `RunPass` 会处理 disabled pass、重置 `Main.rand`、开始/结束 progress、
捕获 pass exception 并返回 `GenPassResult`；Version4 的同名方法体是 stub，因此这些
行为在目标树中仍为 `unknown`，只能作为未来行为验证的候选观察向量。参考树的
`AddPasses` 也显示 `TerrainPass`、可选 Jungle/Skyblock、`DunesAndPyramidLocations`、
`OceanSand` 等注册顺序，但不替代 Version4 当前文件的绑定证据。

### 2.3 CPG 查询 API 记录

使用只读 `CpgEvidence.ps1`，数据库为
`D:\TRbackup\Version4-cpg-export\out-dop8-interproc.sqlite`。本次查询确认：

- `Find-CpgSymbols(GenVars)`：`complete`，唯一 type symbol；
  `Get-CpgTypeSurface`：`complete`，143 个直接成员。
- manifest SHA256：`6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`；
  ProjectFingerprint：`521CBD4C11E7EB731256C232350A54099F431D0E59EBDF3C38880281F169C11B`；
  `SourceSnapshotId: null`。
- 在选定的 `WorldGen`、`GenVars`、`WorldFile`、`Liquid`、`NetMessage`、Dungeon 源路径
  上，`configuration` 为 `16` 项（`Write=1`、`Unknown=15`），`structures` 为 `20`
  项（`Write=1`、`Unknown=19`），`worldSurface` 为 `18` 项（`Write=1`、`Unknown=17`），
  `waterLine` 为 `10` 项（`Write=2`、`Unknown=8`），`leftBeachEnd` 为 `13` 项（含
  `Write=3`、`ReadWrite=2`、`Unknown=8`），`CurrentDungeonGenVars` 为 `48` 项且均为
  `Unknown`。
- `hellChest`、`CurrentDungeon` getter/setter 和 dual-distance accessor 在选定范围内为
  零命中 `partial`，带 `NoMatchingFactInScannedScope`；不能解释为无调用。
- `Find-CpgCallSites` 找到 `WorldGen.GenerateWorld` 的两个选定调用点、`WorldGen.Reset`
  的两个选定调用点、`WorldGen.AddPasses` 的一个选定调用点和
  `WorldBuilding.WorldGenerator.GenerateWorld` 的一个选定调用点。调用点结果是静态
  selected-scope 事实，不是完整动态调用图。
- 本轮追加 `Find-CpgSymbols(CheckSavedOreTiers)`：WorldFile 方法符号查询为 `complete`；
  选定 `WorldFile.cs`/`WorldGen.cs` 范围的调用点查询返回一个 WorldFile 内部入口。对该
  方法执行 `Get-CpgCallableFacts` 为 `partial`，仅形成局部 CFG/operation 节点且
  `DirectCallTargets` 为空，gap 为 `CalleeEffectsNotExpanded`；因此只确认入口骨架，
  不把 CPG 当作 repair 或 Liquid 副作用闭包证明。
- 本轮又查询了 `GenVars.mCaveX` 与 `GenVars.mCaveY`：在选定的
  `Terraria/WorldGen.cs`、`Terraria.WorldBuilding/GenVars.cs`、`Terraria.IO/WorldFile.cs`
  范围内，`Get-CpgMemberUses` 分别返回 7 个和 3 个 `partial` member-use。所有结果的
  `AccessMode` 为 `Unknown`、`AccessClassification` 为 `NoAssignmentEvidence`，且
  `AliasMutation` 为 `unknown-without-callee-summary`；这些结果只能证明索引内存在使用点，
  不能闭合坐标数组的写入者或外部 cave effect。完整参考树的 `MountainCaves` pass 则显示
  `Mountinater(x, y)` 返回后才追加 `mCaveX/mCaveY` 并递增计数，但该调用没有成功返回值。
  因此 proposed API 保留显式 `caveCommitted`/成功信号。当前 NLTX 已增加
  `MountainCaveHistorySystem.AppendAfterSuccessfulCommit` 及其 bool 便捷入口，但底层
  `TryAppend` 仅保留为兼容入口，内部转发为调用方已确认成功的结果；真实 `Mountinater`
  effect port、失败补偿和调度接线仍是 `implementationStatus: partial` 的 integration review
  缺口。生产组合应调用带显式成功信号的入口。
- 对相邻 history 字段的 CPG 查询在选定 `WorldGen.cs`/`GenVars.cs` 范围内也返回
  `complete` 的索引请求，但每个 member-use 的 `AccessMode` 仍为 `Unknown`、证据状态为
  `partial`：`tunnelX` 2 项、`numTunnels` 4 项、`orePatchX` 3 项、`numOrePatch` 5 项。
  源码关系补查确认 Tunnels 在十点 scan 通过后先写 `tunnelX`/递增，再执行两组
  `TileRunner`；且 `numTunnels >= maxTunnels - 1` 时停止追加。Surface Ore 则只在
  `OrePatch(...)` 返回 true 后、`numOrePatch < maxOrePatch - 1` 时追加。当前 NLTX 已分别
  增加 `AppendAfterSuccessfulScan` 和结果型 `AppendAfterSuccessfulCommit`，但真实 scan、
  TileRunner、OrePatch effect port 仍未接线。
- `Find-CpgSymbols`/`Find-CpgCallSites` 在 Version4 `Terraria/WorldGen.cs` 中对
  `Mountinater`、`OrePatch` 各返回 1 个选定调用点，对 `TileRunner` 返回 74 个选定调用点；
  这些是静态 selected-scope 事实，未展开 callee effect，也不证明唯一运行时入口。
- 本次按 Mushroom/fallen-log 缺口追加查询：`mushroomBiomesPosition` 有 3 个、
  `numMushroomBiomes` 有 5 个、`logX` 有 8 个、`logY` 有 4 个 selected-scope
  member-use。前两组访问全部是 `partial`/`AccessMode: Unknown`；`logX` 的 4 个写入
  结果为 `confirmed`、其余 4 个为 `partial`；`logY` 的 2 个写入结果为 `confirmed`、
  其余 2 个为 `partial`。这些结果不能闭合数组别名、动态入口或 pass effect。
- `Find-CpgCallSites(ShroomPatch)` 在 `Terraria/WorldGen.cs` 选定范围内返回 2 个
  `internal-static-exact` 调用点。`Get-CpgCallableFacts(ShroomPatch)` 为 `partial`，
  gap 为 `CalleeEffectsNotExpanded`，所以不能从 CPG 推导 patch 是否“成功”或是否只写
  Tile。源码仍确认它的签名为 `void ShroomPatch(int,int)`，没有提交结果。
- 目标树和完整参考树均显示同一顺序：Mushroom pass 先检查距离和
  `numMushroomBiomes < maxMushroomBiomes`，调用一次主 `ShroomPatch` 与五次随机邻点
  `ShroomPatch`，随后写入 `mushroomBiomesPosition[count]` 的 X/Y 并递增 count。Reset
  将 `logX/logY` 设为 `-1`。fallen-log pass 只有在 `PlaceTile` 后检查 tile 为 488 且
  50% 随机命中时才写 `logX/logY`；`Flowers` 在 Remix 与普通分支都可读取 pending 坐标，
  覆盖扫描坐标并只把 `logX` 清为 `-1`。

因此本文把 `complete` 仅解释为索引查询完成，把 `Unknown` 和零命中 gap 原样带入设计；
CPG 不证明完整 reader/writer 闭包、动态分派、事件订阅、反射、调度或运行时副作用。

### 2.4 当前 NLTX 目标树事实

`src/NSSLC/Component/WorldSession/WorldGeneration` 已有带 `GenerationId`、snapshot、
commit/query、adapter 和 action 类型的局部边界，例如 `WorldLayerMetricsSystem`、
`WorldSpawnAndLandmassSystem`、`OreTierSelectionSystem`、`DungeonLayoutControlSystem`、
`LakeGenerationSystem` 和 `HellChestGenerationSystem`。这些文件的存在只证明局部 API
形状；scheduler 注册、完整 caller、legacy writer 替换和行为等价均未验证。

### 2.5 Mushroom 与 fallen-log 的关系闭包

这两个行为不能合并成一个“生态 System”：Mushroom anchor 是有容量的历史记录，fallen
log 是 producer 到 `Flowers` pass 的一次性坐标 handoff。当前目标 NLTX 已分别提供
`MushroomBiomeGenerationSystem` 和 `FallenLogFlowerHandoffSystem`，但它们仍是未接入
Version4 调度的 proposed seam。

| 行为 | Version4 目标源码 | 完整参考源码 | 设计结论 |
| --- | --- | --- | --- |
| Mushroom candidate 与 anchor | `Terraria/WorldGen.cs:11707-11726` | `Terraria/WorldGen.cs:12987-13006` | 先做距离/容量检查，再执行 1 次主 `ShroomPatch` + 5 次随机邻点 patch，最后写成对 X/Y 并递增 count。 |
| `ShroomPatch` effect | `Terraria/WorldGen.cs:66856` | `Terraria/WorldGen.cs:81240` | 签名为 `void`，内部直接改 Tile/liquid/wall 并调用 `TileRunner`；没有可读取的“成功”返回值。 |
| fallen-log producer | `Terraria/WorldGen.cs:17771-17772` | `Terraria/WorldGen.cs:19051-19052` | `PlaceTile` 后检查 488，再由 1/2 随机决定是否发布 X/Y。 |
| `Flowers` consumer | `Terraria/WorldGen.cs:19627-19632,19691-19695` | `Terraria/WorldGen.cs:20907-20912,20971-20975` | Remix 与普通分支均可消费；覆盖扫描坐标后只清 `logX = -1`，`logY` 保留旧值。 |
| reset | `Terraria/WorldGen.cs:10251-10252` | `Terraria/WorldGen.cs:11531-11532` | `logX` 与 `logY` 都以 `-1` 初始化。 |

`ShroomPatch` 的 proposed API 不能把“Tile 已经产生预期地貌”当成已知布尔结果。若要
保持旧组合，外部 effect adapter 至少要在六次 `void` 调用都正常返回后产生
`PatchBatchCompleted`；异常、部分 Tile 写入、`RunPass` 如何处理这些异常均保持
`unknown`。只有该显式结果存在时，`MushroomBiomeGenerationSystem` 才能追加 anchor；
无结果时必须停在 `unknown`，不能用无条件 append 伪造成功。

`FallenLogFlowerHandoffSystem.TryPublish` 的输入应来自“PlaceTile 后 tile=488”的事实与
同一 RNG 决策，而不是仅来自 `PlaceTile` 的返回值。`TryConsume` 必须是读取坐标和清除
pending 标记的原子组合，并保留旧行为中只清 `logX` 的可观察语义；不得把它改成同时清
`logY` 的新行为。`Flowers` 的两条消费分支、Remix 条件、循环可达性和消费后重复调用
行为仍需真实入口验证。

## 3. 设计原则

1. **一条不变量一名权威写者。** Component 是存储边界，不自动授予任意 System 写权限；
   Query 返回不可变 snapshot，Command 表达受控状态变更。
2. **生成身份贯穿所有 scratch。** 每个 generation-scoped component 携带 `GenerationId`；
   stale commit、跨 world 写入和重试残留必须拒绝或显式恢复。
3. **预约与实际效果分离。** `StructureMap` reservation、Tile/Wall/Liquid 写入、item/NPC
   放置和外部事件属于不同 port；预约成功不代表地形已经提交。
4. **顺序先于并行。** 同一 RNG stream、`StructureMap`、paired coordinates 或共享
   pass-visible state 的操作默认串行；未经行为证据不得并行化。
5. **兼容入口只做翻译。** Legacy facade/Adapter 可以保留调用面，但不保存第二份权威
   状态，也不在 adapter 内复制生成规则。
6. **证据状态和实现状态分离。** `confirmed`、`partial`、`unknown` 描述证据；`proposed`
   和 `not-run` 描述设计/验证状态。任何一项都不能被编译或局部 verifier 自动升级。

## 4. P17 System 组合

叶子组是 inventory 边界，不等于 System 数量。下表列出每个组的 146 个成员覆盖、
候选 owner 和外部契约。所有名称均为 proposed，除标记为当前存在的文件外，不表示已实现。

| 叶子组 | 成员数与成员 | Proposed System / owner | 主要 API 与效果边界 |
| --- | --- | --- | --- |
| `GenVarsConfigurationAndOreState` | 10：`configuration`, `structures`, `copper`, `iron`, `silver`, `gold`, `copperBar`, `ironBar`, `silverBar`, `goldBar` | `WorldGenerationConfigurationAdapter`；`StructureReservationSystem`；`GenerationOreSelectionSystem` | `LoadConfiguration`/`Snapshot`；`CanReserve`/`Reserve`；`CommitSelection`/`Query`。配置 hook、预约和矿石选择分开；不得把 reservation 当 Tile commit。 |
| `GenVarsWorldLayerMetrics` | 13：`lowestCloud`, `worldSurfaceLow`, `worldSurface`, `worldSurfaceHigh`, `rockLayerLow`, `rockLayer`, `rockLayerHigh`, `snowTop`, `snowBottom`, `snowOriginLeft`, `snowOriginRight`, `snowMinX`, `snowMaxX` | `WorldLayerMetricsSystem`（当前有候选文件） | `CalculateAndCommit(generationId, input)`、`Snapshot(generationId)`；标量、snow arrays 和有效长度一起提交。 |
| `GenVarsSurfaceAndBiomeState` | 15：`worldSpawnHasBeenRandomized`, `landmassData`, `remixSurfaceLayerLow`, `remixSurfaceLayerHigh`, `remixMushroomLayerLow`, `remixMushroomLayerHigh`, `boulderPetsPlaced`, `crimStoneWall`, `crimStone`, `ebonStoneWall`, `ebonStone`, `mossTile`, `mossWall`, `lavaLine`, `waterLine` | `WorldSpawnAndLandmassSystem`；`SurfaceMaterialQuery`；`WorldGenerationLiquidBoundarySystem` | spawn/landmass snapshot、material read-only projection、liquid threshold command。`waterLine` 最终 writer 需 integration review。 |
| `GenVarsBeachAndOceanBoundaryState` | 12：`leftBeachEnd`, `rightBeachStart`, `beachBordersWidth`, `beachSandRandomCenter`, `beachSandRandomWidthRange`, `beachSandDungeonExtraWidth`, `beachSandJungleExtraWidth`, `shellStartXLeft`, `shellStartYLeft`, `shellStartXRight`, `shellStartYRight`, `oceanWaterStartRandomMin` | `BeachBoundarySystem`（当前有候选文件） | `Calculate`、`CommitBoundary`、`Snapshot`；shell placement 通过 effect port；保留 inclusive/exclusive 和 RNG 顺序。 |
| `WorldGenBeachAndOceanBiomeState` | 12：`oceanWaterStartRandomMax`, `oceanWaterForcedJungleLength`, `evilBiomeBeachAvoidance`, `evilBiomeAvoidanceMidFixer`, `lakesBeachAvoidance`, `smallHolesBeachAvoidance`, `surfaceCavesBeachAvoidance`, `surfaceCavesBeachAvoidance2`, `maxOceanCaveTreasure`, `numOceanCaveTreasure`, `oceanCaveTreasure`, `skipDesertTileCheck` | `OceanBiomeConstraintSystem`；`OceanCaveTreasureSystem` | 约束 Query/Command 与 attempt recorder 分离；只有 placement 成功后提交 treasure 结果。 |
| `WorldGenUndergroundDesertStructureState` | 9：`UndergroundDesertLocation`, `UndergroundDesertHiveLocation`, `desertHiveHigh`, `desertHiveLow`, `desertHiveLeft`, `desertHiveRight`, `numLarva`, `larvaY`, `larvaX` | `UndergroundDesertStructureSystem` | bounds、paired larva 和 reservation/solidity projection；失败重试不得提前增加 count。 |
| `WorldGenJungleStructureState` | 15：`numPyr`, `PyrX`, `PyrY`, `extraBastStatueCount`, `extraBastStatueCountMax`, `jungleOriginX`, `jungleMinX`, `jungleMaxX`, `jungleHut`, `mudWall`, `JungleItemCount`, `gennedLivingMahoganyWands`, `JChestX`, `JChestY`, `numJChests` | `JungleRegionStructureSystem`；`PyramidPlacementSystem`；`JungleChestLootSystem` | region、paired pyramid coordinates、loot cursor 分开提交；material/RNG/Tile/item effects 外置。 |
| `GenVarsDungeonAndIslands` | 19：`tLeft`, `tRight`, `tTop`, `tBottom`, `tRooms`, `lAltarX`, `lAltarY`, `dungeonGenVars`, `_currentDungeon`, `dungeonBeachPadding`, `skyLakes`, `generatedShadowKey`, `generatedRamRune`, `numIslandHouses`, `skyIslandHouseCount`, `skyLake`, `floatingIslandHouseX`, `floatingIslandHouseY`, `floatingIslandStyle` | `DungeonSelectionControlSystem`；`FloatingIslandPlacementSystem` | `SelectCurrentDungeon`、`AppendRecord`、`Snapshot`；浮岛 history 独立；必须与 `DungeonCrawler.dungeonData` 保持同一 index 配对。 |
| `GenVarsCaveTunnelAndOrePatchState` | 9：`numMCaves`, `mCaveX`, `mCaveY`, `maxTunnels`, `numTunnels`, `tunnelX`, `maxOrePatch`, `numOrePatch`, `orePatchX` | `CaveTunnelHistorySystem`；`OrePatchHistorySystem` | bounded history、`TryAppendAfterSuccess`、overflow/retry/reset；扫描和 TileRunner 通过 port。 |
| `GenVarsMushroomBiomeAndLogState` | 5：`maxMushroomBiomes`, `numMushroomBiomes`, `mushroomBiomesPosition`, `logX`, `logY` | `MushroomBiomeGenerationSystem`；`FallenLogFlowerHandoffSystem` | anchor/history 与 log/flower handoff 分开；消费后清理由单一 owner 负责。`ShroomPatch` 为 void，必须由外部 effect adapter 提供“调用正常完成”的显式结果；不能臆造 tile 成功布尔值。 |
| `GenVarsLakeAndOasisState` | 8：`maxLakes`, `numLakes`, `LakeX`, `maxOasis`, `numOasis`, `oasisPosition`, `oasisWidth`, `oasisHeight` | `LakeGenerationSystem`；`OasisGenerationSystem` | bounded candidate、成功后追加、liquid/vegetation effect port；两者由 pass coordinator 排序。 |
| `GenVarsHellAndSpecialStructures` | 9：`hellChest`, `hellChestItem`, `statueList`, `StatuesWithTraps`, `crimsonLeft`, `shimmerPosition`, `notTheBeesAndForTheWorthyNoCelebration`, `noTrapsAndForTheWorthyNoCelebration`, `flipInfections` | `HellChestGenerationSystem`；`StatueCatalogSystem`；`InfectionAlignmentSystem`；`ShimmerAnchorSystem` | seed-derived input、RNG、item/Tile/NPC effects 外置；`hellChest` 零命中 gap 不能当作无调用。 |
| `GenVarsDungeonDerivedProperties` | 3：`CurrentDungeon`, `CurrentDungeonGenVars`, `DualDungeon_NormalizedDistanceSafeFromDither` | `DungeonDerivedPropertiesSystem` + `LegacyDungeonDerivedPropertiesAdapter` | `CurrentDungeon` 是 Command/带写副作用 API；record 是纯 Query；dual-distance setter 必须写明 `DungeonControlLine` backing。 |
| `WorldSavedOreTierState` | 7：`Copper`, `Iron`, `Silver`, `Gold`, `Cobalt`, `Mythril`, `Adamantite` | `WorldSavedOreTierSystem`；`WorldFileSavedOreTierAdapter`；`NetMessageSavedOreTierProjection` | 一个七值 owner；WorldFile 负责版本映射/repair，NetMessage 只做固定顺序 projection，不接管 inbound state。 |

上述计数合计 `10+13+15+12+12+9+15+19+9+5+8+9+3+7 = 146`。`GenVars.copper` 等
生成期值与 `WorldGen.SavedOreTiers` 的持久化值保持两个 owner，不能因为 tile ID 相似而合并。

## 5. 权威状态与 API 合约

### 5.1 Session 与快照

目标组合使用一个 `WorldGenerationSession` 概念上下文（具体类型待 integration review），
包含 `WorldId`、`GenerationId`、seed/dimension/config snapshot、RNG stream identity、
phase 和取消/abort 状态。P17 System 只读 session 输入并提交自己的 snapshot；它不直接
拥有全局 `Main`、Tile grid、Liquid buffer、网络连接或文件流。

每个 P17 component 至少提供：

```text
Create(generationId, defaults)
Snapshot(generationId) -> immutable snapshot
Reset(generationId, reason)
Restore(generationId, snapshot) -> one atomic replacement
```

`Restore` 必须先检查 `WorldId`/`GenerationId`/schema，再一次性替换对应状态。数组、List
和 paired coordinates 不能让调用者拿到内部可变引用。恢复失败时旧 snapshot 仍然可用；
真正的 tile/file rollback 由外部事务 owner 决定。

### 5.2 Query、Command、Adapter、Projection

| 形态 | 允许职责 | 禁止职责 |
| --- | --- | --- |
| Query | 返回不可变层级、边界、history、dungeon 或 saved-tier snapshot；可暴露明确的索引异常契约 | 写 static backing、随机、缓存、事件、日志或隐式 lazy state |
| Command | 预约、选择当前 dungeon、append-after-success、threshold commit、saved-tier commit 和 reset/restore | 绕过 generation identity、直接改外部 Tile/文件/网络、隐式双写 |
| Adapter | 配置 hook、WorldFile 版本字段、DungeonControlLine、legacy facade 参数翻译 | 复制领域规则或持有第二份权威状态 |
| Projection | SavedOreTiers 七值出站编码、landmass/material/treasure 只读视图 | 反向修改 owner 或把出站成功当作领域提交 |
| Coordination API | 按既定 pass 顺序组合多个 owner，定义 visibility、retry 和 barrier | 把所有 P17 字段集中成一个万能 `GenVarsSystem` |

### 5.3 关键不变量

- `CurrentDungeon` setter 继续执行下限到 `0` 的规则；上界和两个 record collection 的
  长度校验仍为 `unknown`，必须在集成前决定异常行为。
- `CurrentDungeonGenVars` 的读取与 `DungeonCrawler.dungeonData[CurrentDungeon]`、
  `GenVars.dungeonGenVars[CurrentDungeon]` 共享同一 index；不能仅迁一份 List。
- `DualDungeon_NormalizedDistanceSafeFromDither` getter/setter 不能标为纯 Query；setter
  写 `DungeonControlLine.NormalizedDistanceSafeFromDither`，需要明确单一 backing owner。
- `waterLine` 在 generation、WorldFile load 和 Liquid 路径均出现写入；最终 owner、load
  barrier 与 `Liquid.QuickWater/WaterCheck` 顺序由 integration review 决定。
- SavedOreTiers 四个低阶值在 `-1` 时按同一 tile-count snapshot repair，三个高阶值保留
  版本/altar sentinel 规则；保存和网络字段顺序不能交给普通 Query。
- Mushroom anchor 的 append 顺序必须是“六次 `ShroomPatch` 正常返回 -> 写成对 anchor ->
  count++”；`ShroomPatch` 是 void，`patchesCommitted` 只能是外部 adapter 的显式完成信号，
  不能由静态字段写入或名称推导。
- fallen-log handoff 的发布顺序必须是“PlaceTile 后确认 tile type 488 -> RNG 接受 -> 写
  `logX/logY`”；`Flowers` 消费要覆盖 Remix/普通两条分支，并在消费成功后只清 `logX`，
  以保留 Version4 的 sentinel 语义。

## 6. 依赖与调度 DAG

### 6.1 已确认的入口骨架

```text
WorldGen.worldGenCallback
  -> WorldGen.GenerateWorld
       -> load configuration
       -> Hooks.ProcessWorldGenConfig(ref configuration)
       -> new WorldGenerator(seed, configuration)
       -> clearWorld
       -> WorldGen.Reset
       -> WorldGen.AddPasses
       -> DisablePassesForSpecialSeeds
       -> WorldGenerator.GenerateWorld
            -> _passes[PassResults.Count]
            -> pause/abort check
            -> RunPass (Version4 body stub)
       -> Finish
       -> finally restore flags
  -> SaveNewWorld only after true result
```

CPG 直接确认的 selected call sites 支持 `WorldGen.GenerateWorld`、`Reset`、`AddPasses` 和
`WorldBuilding.WorldGenerator.GenerateWorld` 的静态边；参考树补充了完整 `RunPass` 的
候选语义，但不能解除 Version4 stub 的 `unknown`。

当前 NLTX 局部实现提供了带 manifest 前缀的 `BeginPass(plan, state, manifest, passId, ...)`
组合。它要求 `passId` 等于 `manifest.PassResults.Count` 对应的下一个 descriptor，因而
拒绝跳过前序 pass；无 manifest 的旧 overload 仍保留作兼容边界，不能单独证明调度顺序。

### 6.2 Proposed P17 phase DAG

```text
SessionStart
  -> ConfigurationLoad
  -> SeedAndRngCommit (P18/integration input)
  -> WorldClearAndP17Reset
  -> ReservationReady
  -> LayerMetrics
  -> SpawnAndSurfaceFacts
  -> BeachBoundary
  -> OceanConstraints
  -> DungeonSelection + IslandHistory
  -> Desert/Jungle/Cave/Mushroom/Lake/Oasis histories
  -> FallenLog placement handoff
  -> HellAndSpecialFacts
  -> ExternalTileLiquidEntityEffects
  -> Flowers consumes optional fallen-log handoff
  -> SavedOreTierCommit
  -> PassResultCommit
  -> FinishAndCleanup
  -> Save/Network Projections
```

必须保持的顺序是：配置/seed 在 pass 前冻结；layer metrics 在依赖它的 surface/biome 前
可见；beach 在 ocean/dungeon 位置前可见；dungeon index 与 record append 原子配对；
history 只在 placement 成功后追加；SavedOreTiers repair/load barrier 在 Liquid 与网络
projection 前完成。具体 pass 边和并行许可为 `proposed`，因为 Version4 `RunPass` stub、
RNG 调用闭包和 scheduler 注册尚未确认。

## 7. 生命周期、失败与外部效果

| 阶段 | P17 动作 | 必须保持的状态语义 | 当前证据 |
| --- | --- | --- | --- |
| create | 建立 world/generation identity，加载配置和 seed 输入 | 不允许旧 generation snapshot 写入新 world | `GenerationId` 组件存在；完整入口接线 `unknown` |
| reset/rebuild | 重建 reservation、history、数组、dungeon records、hell chest sequence、saved-tier defaults | Reset 是一次可重复的边界；数组和 List 不能残留前次 generation | `WorldGen.Reset`/参考树可见；逐成员闭包 `partial` |
| pass | 按注册顺序计算并 commit snapshot | commit visibility、RNG consumption、reservation 和外部写入顺序固定 | pass loop 确认；`RunPass` 目标树效果 `unknown` |
| retry | 失败候选重试，成功后才 append | 失败不增加 count、不重复 event、不泄漏 tile/liquid effect | Component 计划有 append gate；legacy parity `unknown` |
| abort/pause | 控制器停止或暂停 pass | flags、current pass、progress、snapshot 和外部写入的中间可见性需恢复 | control branches 存在；回调实现缺口 |
| finish | 完成 P17 snapshot，清除 temporary flags | `SaveNewWorld` 只在生成返回 true 后发生 | Version4 入口顺序确认 |
| load | WorldFile 版本读取、SavedOreTiers repair、`waterLine=Main.maxTilesY`、Liquid settling | repair 必须先于 Liquid barrier；失败不发布半修复 state | WorldFile/Liquid 源码确认 |
| network | SavedOreTiers 七值出站 projection | 固定顺序和 signed short 宽度；入站 consumer/capability 未定 | NetMessage 出站确认，入站 `unknown` |
| unload/multi-world | 释放 session 和 static compatibility facade | 不允许 concurrent world 共享 RNG/GenVars | legacy static 状态确认，隔离语义 `unknown` |

Tile/Wall/Liquid、item/chest/NPC、RNG、hook、progress/UI、file/network 都通过显式 port 或
外部 integration owner；P17 System 不直接包住这些副作用。

## 8. 跨分区交接

以下条目必须标记 `crossSubsystemOwner: integration-review`，在集成决策前不能进入
`confirmed`：

| 交接 | P17 提供 | 外部需要决定 |
| --- | --- | --- |
| P18 seed/options | 归一化 seed、secret flag、RNG stream identity | seed owner、启用时点、重放和随机消费顺序 |
| P19 pass execution | P17 snapshot/commit API | pass registry、phase、pause/abort、RunPass 效果和结果提交 |
| P20 actions/shapes | reservation/placement intent、history candidate | Tile/Wall/action chain 的提交与失败补偿 |
| Liquid | water/lava threshold snapshot | `waterLine` 唯一写者、load barrier、QuickWater/WaterCheck 顺序 |
| Dungeon | `CurrentDungeon`、P17 records | `DungeonCrawler.dungeonData` 配对、bounds 和 dual-distance backing |
| WorldFile | seven-tier snapshot 与 repair query | 版本门槛、保存顺序、原子恢复和错误处理 |
| NetMessage | seven-tier projection | 入站消费、capability negotiation、连接中途可见性 |
| Spawn/NPC | landmass snapshot | extra spawn、NPC 生成和 load 后消费时点 |

## 9. 兼容、回滚与删除门禁

迁移采用“只读 adapter -> 单一新 writer -> 行为比较 -> 删除旧 writer”的顺序。不得同时让
legacy `GenVars` writer 和 NSSLC owner 写同一不变量；shadow comparison 只能读取旧结果，
不能形成第二套权威状态。

回滚开关必须停用新 owner 的 commit/projection，并将调用恢复到 legacy adapter；它不应
删除已加载 world 的 tile/file/network 数据。只有在以下全部满足时才可删除旧 writer 或
facade：

1. 对应 ConceptId 的真实 pass/API 入口已经命中新组合；
2. generation identity、reset/restore、异常、retry、pause/abort 和多 world 观察向量通过；
3. Tile/Wall/Liquid、WorldFile、NetMessage、DungeonCrawler 等 handoff 已有唯一 owner；
4. 同 seed/尺寸/配置下的必需行为测试覆盖状态 delta、事件/effect、顺序、错误、生命周期、
   重试和幂等；
5. 回滚演练可在失败前后恢复到旧入口，且没有双写或重复外部 effect。

## 10. 未决缺口

- Version4 CPG 没有 `SourceSnapshotId`，CPG 导出旁没有 `src`，无法把索引边绑定到完整
  revision。
- Version4 `RunPass` stub 阻止 pass 内部 Tile 写入、异常、progress 和 retry 结论。
- `waterLine`、dual-distance setter、dungeon 双 List 的最终 owner 和恢复事务未决定。
- `hellChest` 查询 zero-hit 带 gap；完整 inbound caller、List/array alias、hook/reflection、
  unload、multi-world 和 scheduler 注册未知。
- 当前 NLTX System 仅有局部源码事实；没有本任务范围内的生产调用者或行为验证。
- `mCaveX/mCaveY` 的 CPG member-use 访问模式、alias 写入和 `Mountinater` 的成功语义未闭合；
  当前局部 API 已要求 `caveCommitted` 才能走 append-after-success，但真实 effect port
  尚未接入，兼容 `TryAppend` 不能被当作生产调度证明。
- Mushroom/fallen-log 仍有独立缺口：`ShroomPatch` 的 callable facts 是 `partial`，且其
  `void` 签名没有成功语义；`mushroomBiomesPosition`/`numMushroomBiomes` 的 CPG 访问方向
  未知；`Flowers` 的实际注册调度、两条消费分支可达性和异常/重试行为未被目标运行时验证。
  当前 `MushroomBiomeGenerationSystem` 与 `FallenLogFlowerHandoffSystem` 的参数只是一条
  受控 seam，不证明旧入口已命中新组合。

## 11. 局部实施切片

依据本设计在 `D:\TRbackup\NLTX\src\NSSLC` 增加了受控协调 System：

- `WorldGenerationLifecycleSystem`：为单一 `GenerationId` 维护
  `Uninitialized -> Loading -> Generating -> Ready` 及失败、卸载、重置路径；每次合法
  transition 递增 `GenerationRevision`，错误 generation 会被拒绝。
- `WorldGenerationPassExecutionSystem`：组合既有 `WorldGenerationPlanComponent` 与
  `WorldGenerationPassStateComponent`，处理 enabled/disabled pass、descriptor version、
  active pass、checkpoint、pause/abort 和失败原因；它不执行 Tile、Wall、Liquid、文件、
  网络或实体副作用。
- `WorldSavedOreTierLoadSystem`：组合 `WorldFileSavedOreTierAdapter.Read`、低阶 `-1`
  repair 和 `WorldSavedOreTierStateComponent` 的单次替换；读取或 repair 失败时不发布半
  修复 state，也不负责真实 WorldFile I/O、`waterLine` 或 Liquid settling。
- `MountainCaveHistorySystem`：新增 `AppendAfterSuccessfulCommit` 结果边界；失败的
  `Mountinater` 提交不会增加 history，成功提交受 capacity-30 限制并保留成对 X/Y 顺序。
  该 API 尚未调用真实 `Mountinater` 或 Tile effect。
- `SurfaceTunnelHistorySystem` 和 `SurfaceOrePatchHistorySystem`：分别新增 scan-accepted
  与 patch-committed 结果边界，保留 Version4 的 effective capacity-49；真实 TileRunner/
  OrePatch effects 仍由 integration-review port 承担。
- `MushroomBiomeGenerationSystem` 和 `FallenLogFlowerHandoffSystem`：已有 anchor append、
  log publish/one-shot consume 的局部 seam；真实六次 `ShroomPatch` 组合、fallen-log pass、
  `Flowers` 两条消费分支和 scheduler 仍未接入，不能把现有方法视为迁移结果。

局部 verifier 位于
`D:\TRbackup\NLTX\Test\Terraria.WorldSession.WorldGeneration.P17.CoreVerification`，覆盖
生命周期、manifest-aware ordered pass commit、C02 layer metrics、C13 dungeon derived index
和 C14 SavedOreTiers。C14 verifier 另外覆盖了 v23/altar sentinel、低阶 tie repair、直接
读取字段缺失时的旧快照保持和单次 load commit。`WorldGenerationPassExecutionSystem` 同时修正了 disabled pass 在
active/failed/aborted 状态下绕过控制检查的边界，并拒绝从 manifest 前缀之外开始 pass。
当前还保留 `IWorldGenerationPassRunner`/`WorldGenerationPassRunOutput` 作为外部执行 seam；
它只承载显式返回的耗时、hash 和随机检查点，不实现 Version4 `RunPass` 的 stub 缺失效果。
局部切片没有接入 Version4 真实
`WorldGen.GenerateWorld` 调用路径，也没有关闭 CPG 的动态分派、scheduler、`RunPass`
stub 或跨分区 handoff 缺口。

## 12. 设计验收状态

本设计覆盖 P17 的 14 组和 146 个成员，定义了 proposed System/API/DAG/lifecycle，保留
所有 `partial`、`unknown` 和跨分区 handoff。完整参考项目只用于语句和关系校对；本轮
只验证了独立核心切片，未接入真实入口。全量真实入口、行为等价、跨分区 handoff 和
删除门禁仍未验证，因此设计继续保持 `designStatus: proposed`、`evidenceStatus: partial`、
`verificationStatus: not-run`、`coreSliceVerification: passed`；不称迁移成功。
