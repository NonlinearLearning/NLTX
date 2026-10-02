# System Decomposition Report: authoritative P17

partitionId: P17
taskId: AUTH-SYS-P17
claimMode: manual
sessionId: 95cb4a271e0346218cb14565179c4840
inputReport: `docs/migration/ledgers/authoritative-20-partitions/P17-World-Terrain-Biomes-GenVars.md`
taskPrompt: `docs/component-decomposition/review-round-1/design/2026-09-11-version4-authoritative-20-partition-prompts/2026-09-11-version4-P17-world-terrain-biomes-genvars-public-decomposition.md`
outputReport: `docs/system-decomposition/reports/2026-09-18-system-decomposition-authoritative-P17-world-terrain-biomes-genvars.md`
designStatus: proposed
evidenceStatus: partial
verificationStatus: not-run
sourceModified: false

## Scope and Evidence

本报告只覆盖权威 P17 `WorldGenerationAndEcology` 的 14 个叶子组：143 个字段、3 个属性，共 146 个成员。完整成员清单列在「State Ownership and Write Closure」；不从相邻 P 分区补成员或决定跨分区 owner。生成执行、secret-seed policy、tile/liquid 执行、NPC spawn、存档协议和网络入口在本报告中只作为协作边界。

Version4 源码位置为 `D:\TRbackup\Version4`。本次读取的主要事实来自：

- `Terraria.WorldBuilding/GenVars.cs:11-305`：P17 的 `GenVars` 声明、静态可变字段和三个 dungeon 属性；当前文件 SHA256 `E01C69435762ADB532F7CAD6F38AD5CAD97709DC2965060F4A41427FD804C67E`。
- `Terraria/WorldGen.cs:3318-3333, 6267-6283, 6387-6492, 10096-10632`：SavedOreTiers 声明、callback、reset 和生成入口/早期 pass；当前文件 SHA256 `A06A8463E39EA065441FF1EF702A787D3450ADAE5CD3065CF30BF76784D3EA1D`。
- `Terraria.WorldBuilding/WorldGenerator.cs:291-343`：按 `_passes` 顺序推进、pause/abort 分支；`RunPass` 当前方法体为 stub，故执行 pass 的实际效果与异常语义为 `unknown`。
- `Terraria.IO/WorldFile.cs:728-753, 811-868, 1348-1355, 1423-1430, 2184-2186, 2395-2408, 3691-3708`：版本读取、SavedOreTiers 修复、读写顺序，以及加载后设置 `waterLine` 再运行液体处理。
- `Terraria/Liquid.cs:193-195, 1002`、`Terraria/NetMessage.cs:385-391`、`Terraria.GameContent/ExtraSpawnPointManager.cs:45-83`：液体边界读写、SavedOreTiers 出站数据和 landmass 消费者。
- `Terraria.GameContent.Generation.Dungeon/DungeonCrawler.cs:20-31, 34-120`、`DungeonUtils.cs:119-152, 192-228`、`Terraria.GameContent.Biomes/DungeonControlLine.cs:35`：dungeon list、当前索引协作、bounds 查询及 dual-dungeon backing state。

使用只读 Version4 CPG Query API `CpgEvidence.ps1`：`Find-CpgSymbols(GenVars)` 为 `complete`；`Get-CpgTypeSurface` 为 `complete`，返回 143 个 indexed `ContainsSymbol` 项；对配置、结构图、landmass、层级、边界、dungeon、湖泊和液体字段执行了 `Get-CpgMemberUses`。此前的广域查询笔记记录对 150 个 indexed member symbols 扫描 967 个 source shards；这仍留下多处 `AccessMode: Unknown` 与零命中 gap，表示索引扫描范围，不构成闭合读写集。索引 manifest SHA256 为 `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`，ProjectFingerprint 为 `521CBD4C11E7EB731256C232350A54099F431D0E59EBDF3C38880281F169C11B`，`SourceSnapshotId` 为空。CPG 导出旁没有 `src`，且 Version4 源目录不是 Git checkout；除上述文件哈希外，不能将索引绑定到完整源码 revision。

代表性 member-use 结果：`configuration` 为 16 项（1 Write、15 Unknown），`structures` 为 20 项（1 Write、19 Unknown），`landmassData` 为 10 项（均 Unknown），`worldSurface` 为 18 项（1 Write、17 Unknown），`waterLine` 为 10 项（2 Write、8 Unknown），`leftBeachEnd` 为 13 项（3 Write、2 ReadWrite、8 Unknown），`dungeonGenVars` 为 9 项（均 Unknown）。这些是查询扫描范围内的索引事实，不是完整 reader/writer 闭包；`hellChest` 的查询返回 `partial`、零命中及 `NoMatchingFactInScannedScope` gap，不据此推断无调用。Version4 源码回查在 `WorldGen.cs:29659` 确认它与 `hellChestItem` 一起参与地狱箱物品选择。

当前目标树 `src/NSSLC/Component/WorldSession/WorldGeneration` 存在对应组件与 System 文件。静态读取的例子包括 `Systems/WorldLayerMetricsSystem.cs`（计算后按 generation id 校验并提交快照）、`Systems/WorldSpawnAndLandmassSystem.cs` 和 `Systems/OreTierSelectionSystem.cs`。这些文件存在和 API 形状可确认，调用注册、pass 调度、完整 legacy 写者替换和运行时行为未验证。SS14 参考仓库只用于观察 partial System 文件的组织方式；没有把 SS14 行为或调度规则推导到 Version4。

证据标签：`confirmed` 表示当前源码中可定位的声明/语句；`partial` 表示索引、静态路径或闭包不完整；`unknown` 表示缺少可绑定证据；所有下述新 owner 与 API 均为 `proposed`。

## Prior Component Decomposition Reconciliation

同分区 Component 文档为 `docs/component-decomposition/review-round-2/2026-09-11-version4-p17-world-terrain-biomes-genvars-component-design.md` 和 `.../2026-09-11-version4-p17-world-terrain-biomes-genvars-component-execution.md`。它们把 146 个成员分为 C01-C14，并报告 Component boundary 已覆盖；Execution 文档还保留了历史 focused build/run 状态，同时明确 RNG、pass 调度、tile/liquid/entity effects、persistence/network、restore order 和 legacy parity 仍属 integration gap。该历史结果不证明本 System 组合已运行或与 Version4 等价，本会话没有重跑其 verifier。

这些 Component 边界不机械变成 14 个 System。较贴近不变量与调用协作的合并/拆分为：配置加载与 reservation 分开；layer metrics 独立提交；spawn/landmass 与 liquid boundary 分开；beach 输入先于 ocean constraint；dungeon selection 与 floating-island placement 分开；Cave/Tunnel/OrePatch、Mushroom/Log、Lake/Oasis 依其独立历史/placement 提交边界协作；Jungle 区域、Pyramid 和 chest/loot 保持可独立验证的子能力；SavedOreTiers 与生成用 `GenVars.copper` 等保持分开。

路径对照存在重要差异：Component 文档记录的实现位置是旧的根级 `src/WorldSession/...`；当前该路径不存在，当前可见目标位于 `src/NSSLC/Component/WorldSession/WorldGeneration/...`。因此这里只把现存文件视为当前源码事实，不从历史 execution ledger 推断其完整搬迁、编译状态或调度接线。

## Conceptual Behaviors

| ConceptId | 可观察行为与不变量 | 生命周期/协作 |
|---|---|---|
| `P17.B01` 配置、保留区和生成矿石选择 | 同一 world-generation session 读取配置与 pass 参数；结构候选经 reservation 避让；矿石 tile 与 bar item 选择供后续 pass 使用。配置 hook 可通过 `ref` 改写配置。 | world generation 开始时建立，后续 pass 读取；reservation 被多个 biome/structure placement 协作使用。 |
| `P17.B02` 世界层级与 surface/ecology facts | surface/rock/snow/cloud 等测量值为后续地形与 biome pass 提供坐标约束；spawn、landmass、remix、感染、材料与液体线是不同不变量，不应以 `GenVars` 同类声明合并。 | 由生成 pass 逐步计算和提交；Liquid、ExtraSpawnPointManager、biome pass 是消费者或写入协作者。 |
| `P17.B03` beach/ocean 输入与限制 | beach 两侧边界、shell origins 和 ocean cave/avoidance scratch 限制候选位置、重复尝试与 treasure 坐标。 | beach 边界须先可见于依赖它的 dungeon/ocean/biome pass；精确随机调用和完整 pass 顺序仍需验证。 |
| `P17.B04` desert/jungle/dungeon/island 布局事实 | paired coordinates、bounds、counts、当前 dungeon index 和 per-dungeon records 必须保持索引及配对有效；reservation 与实际 tile placement 是不同副作用。 | pass 中建布局，后续 placement/查询消费；DungeonCrawler 和 DungeonUtils 跨对象共享 index/list 语义。 |
| `P17.B05` cave/tunnel/ore-patch/mushroom/log/lake/oasis scratch | 容量、used count、配对坐标、成功后追加及重置边界决定后续扫描/放置。 | generation-scoped scratch；具体 TileRunner、液体、花草及失败回滚由外部执行端确认。 |
| `P17.B06` hell/special structure and seed-derived facts | hell chest selection、statue/trap 规则、infection alignment、shimmer anchor 与混合 seed flags 控制结构选择和限制。 | 受 seed policy、RNG、物品/Tile 写入影响；secret seed owner 不在 P17 中裁决。 |
| `P17.B07` persistent/network ore-tier identity | 七个 tier 值表达存档世界里的 tile identity；旧版 repair、序列化顺序和 world-data packet 是外部可观察边界。 | world load/save/network；不能并入 B01 的生成期矿石选择。 |

## State Ownership and Write Closure

以下清单来自 P17 输入 ledger；System 名称仅为候选 owner。表中不代表目前已经形成唯一写入闭包。

| P17 叶子组 | 成员（146/146） | 候选 owner 与不变量 | Version4 读写证据 |
|---|---|---|---|
| `GenVarsConfigurationAndOreState` (10) | `configuration`, `structures`, `copper`, `iron`, `silver`, `gold`, `copperBar`, `ironBar`, `silverBar`, `goldBar` | `WorldGenerationConfigurationAdapter` 提供配置快照；`StructureReservationSystem` 单独维护 reservation；`GenerationOreSelectionSystem` 提交生成期选择。不得把三个职责折叠为一个 GenVars owner。 | `GenerateWorld` 读取配置、触发 `Hooks.ProcessWorldGenConfig(ref ...)` 并创建 generator；`Reset` 新建 `StructureMap` 并设定 tier 值。WorldGen 的 biome placements 读写 reservation。CPG member-use 多数方向 Unknown；hook 和 list/map 间接效果未闭合。 |
| `GenVarsWorldLayerMetrics` (13) | `lowestCloud`, `worldSurfaceLow`, `worldSurface`, `worldSurfaceHigh`, `rockLayerLow`, `rockLayer`, `rockLayerHigh`, `snowTop`, `snowBottom`, `snowOriginLeft`, `snowOriginRight`, `snowMinX`, `snowMaxX` | `WorldLayerMetricsSystem` 候选唯一提交者；snapshot/query 只读。标量、snow column arrays 和计数/边界需一起维护对应 generation id。 | `Reset` 初始化若干层级与重建 snow arrays；WorldGen 多处读取其区域范围。完整计算 writer 和每个数组的有效长度/清理点为 partial。 |
| `GenVarsSurfaceAndBiomeState` (15) | `worldSpawnHasBeenRandomized`, `landmassData`, `remixSurfaceLayerLow`, `remixSurfaceLayerHigh`, `remixMushroomLayerLow`, `remixMushroomLayerHigh`, `boulderPetsPlaced`, `crimStoneWall`, `crimStone`, `ebonStoneWall`, `ebonStone`, `mossTile`, `mossWall`, `lavaLine`, `waterLine` | 至少拆为 `WorldSpawnAndLandmassSystem`、surface/material facts、`WorldGenerationLiquidBoundarySystem`。material definition/query 不应拥有 Tile 写权；water/lava thresholds 的最终 owner 待 integration review。 | WorldGen 添加、清除 landmass；`ExtraSpawnPointManager` 遍历其数据。`Liquid.cs` 消费 lava/water line 且修改 waterLine；WorldFile load 也写 waterLine。CPG 对 list access 多为 Unknown。 |
| `GenVarsBeachAndOceanBoundaryState` (12) | `leftBeachEnd`, `rightBeachStart`, `beachBordersWidth`, `beachSandRandomCenter`, `beachSandRandomWidthRange`, `beachSandDungeonExtraWidth`, `beachSandJungleExtraWidth`, `shellStartXLeft`, `shellStartYLeft`, `shellStartXRight`, `shellStartYRight`, `oceanWaterStartRandomMin` | `BeachBoundarySystem` 管理一份 generation-scoped boundary snapshot；shell placement effects 走明确外部 port。 | `Reset` 设定初始常量，WorldGen 用 `genRand` 计算左右 beach 和 dungeon 起点；多个后续 pass 读取。`leftBeachEnd` CPG 同时发现 Write/ReadWrite/Unknown，具体闭包需源码逐点确认。 |
| `WorldGenBeachAndOceanBiomeState` (12) | `oceanWaterStartRandomMax`, `oceanWaterForcedJungleLength`, `evilBiomeBeachAvoidance`, `evilBiomeAvoidanceMidFixer`, `lakesBeachAvoidance`, `smallHolesBeachAvoidance`, `surfaceCavesBeachAvoidance`, `surfaceCavesBeachAvoidance2`, `maxOceanCaveTreasure`, `numOceanCaveTreasure`, `oceanCaveTreasure`, `skipDesertTileCheck` | `OceanBiomeConstraintSystem` 管制约束状态；treasure attempt recorder/placement projection 独立，外部执行结果回传后提交。 | reset constants、避让检查、attempt counters 分散在 WorldGen。CPG 仅能定位静态使用，失败尝试、重复坐标和 placement transaction 仍 partial。 |
| `WorldGenUndergroundDesertStructureState` (9) | `UndergroundDesertLocation`, `UndergroundDesertHiveLocation`, `desertHiveHigh`, `desertHiveLow`, `desertHiveLeft`, `desertHiveRight`, `numLarva`, `larvaY`, `larvaX` | `UndergroundDesertStructureSystem` 管布局/bounds；paired larva coordinates/count 由同一 placement owner 成功后原子提交。 | `Reset` 设置 hive bounds 初值；WorldGen 通过 desert placement、bounds 和 larva paths 使用这些值。reservation、solidity/tile/entity 执行与异常回滚不是 P17 已确认写闭包。 |
| `WorldGenJungleStructureState` (15) | `numPyr`, `PyrX`, `PyrY`, `extraBastStatueCount`, `extraBastStatueCountMax`, `jungleOriginX`, `jungleMinX`, `jungleMaxX`, `jungleHut`, `mudWall`, `JungleItemCount`, `gennedLivingMahoganyWands`, `JChestX`, `JChestY`, `numJChests` | `JungleRegionStructureSystem`、`PyramidPlacementSystem`、`JungleChestLootSystem` 按 region、paired location、loot cursor 分开提交；跨 owner 由 pass 组合排序。 | WorldGen reset、区域扫描、pyramid arrays 和 chest loot 操作可在源码定位；RNG 顺序、material/tile/item placement 和成功后追加语义未在 System 端闭合。 |
| `GenVarsDungeonAndIslands` (19) | `tLeft`, `tRight`, `tTop`, `tBottom`, `tRooms`, `lAltarX`, `lAltarY`, `dungeonGenVars`, `_currentDungeon`, `dungeonBeachPadding`, `skyLakes`, `generatedShadowKey`, `generatedRamRune`, `numIslandHouses`, `skyIslandHouseCount`, `skyLake`, `floatingIslandHouseX`, `floatingIslandHouseY`, `floatingIslandStyle` | `DungeonSelectionControlSystem` 管索引和 per-dungeon state；`FloatingIslandPlacementSystem` 管岛屿数据。Dungeon crawler 的记录与 GenVars 的 index 需高层协作。 | `Reset` 清理 dungeon list；WorldGen 建 list 并切换索引；DungeonCrawler 另有 `dungeonData[CurrentDungeon]`，DungeonUtils 遍历 GenVars list。两组 list 的同长度/顺序原子性未闭合。 |
| `GenVarsCaveTunnelAndOrePatchState` (9) | `numMCaves`, `mCaveX`, `mCaveY`, `maxTunnels`, `numTunnels`, `tunnelX`, `maxOrePatch`, `numOrePatch`, `orePatchX` | cave/tunnel/ore-patch history 作为独立 history owners；只在对应生成动作成功或旧行为要求的时点提交。 | WorldGen 的 scan、TileRunner 与 scratch readers/writers 分散；历史报告指出 overflow/reset 和 append-after-success 的 parity 尚未完整，数组与计数生命周期为 partial。 |
| `GenVarsMushroomBiomeAndLogState` (5) | `maxMushroomBiomes`, `numMushroomBiomes`, `mushroomBiomesPosition`, `logX`, `logY` | `MushroomBiomeGenerationSystem` 与 `FallenLogFlowerHandoffSystem` 分开；handoff 消费与坐标清理由单一 owner 保证。 | 静态字段来源已确认，旧 component plan 有 focused boundary 记录；ShroomPatch/Flowers、随机源与 tile/liquid effects 接线为 unknown/partial。 |
| `GenVarsLakeAndOasisState` (8) | `maxLakes`, `numLakes`, `LakeX`, `maxOasis`, `numOasis`, `oasisPosition`, `oasisWidth`, `oasisHeight` | `LakeGenerationSystem` 与 `OasisGenerationSystem` 各自维护 bounded state，执行成功后追加；两者 share pass coordinator。 | WorldGen 中可见 max/count/array 读写；候选扫描、下游 vegetation、液体写入、reset/restore 顺序未闭合。 |
| `GenVarsHellAndSpecialStructures` (9) | `hellChest`, `hellChestItem`, `statueList`, `StatuesWithTraps`, `crimsonLeft`, `shimmerPosition`, `notTheBeesAndForTheWorthyNoCelebration`, `noTrapsAndForTheWorthyNoCelebration`, `flipInfections` | hell chest, statue/trap catalog, shimmer anchors, infection alignment 各按不同不变量保留边界；seed-derived input 由 integration review 提供。 | `Reset` 构造 hell chest item sequence，WorldGen 选择箱内物品；seed flags 来自 Reset/options。CPG 对 `hellChest` 的零结果带 gap；动态 seed/config 回调与 RNG 顺序未闭合。 |
| `GenVarsDungeonDerivedProperties` (3) | `CurrentDungeon`, `CurrentDungeonGenVars`, `DualDungeon_NormalizedDistanceSafeFromDither` | `CurrentDungeon` 是有写副作用的选择 Command/API，查询当前 record 可只读；DualDungeon 属性需 command/write adapter 加只读 access，不能整体注册成纯 Query。 | setter 将 index 下限夹到 0；getter 直接 `dungeonGenVars[index]`；dual-dungeon getter/setter 转发 `DungeonControlLine.NormalizedDistanceSafeFromDither`。上界、集合一致性和 backing owner 未闭合。 |
| `WorldSavedOreTierState` (7) | `Copper`, `Iron`, `Silver`, `Gold`, `Cobalt`, `Mythril`, `Adamantite` | `WorldSavedOreTierSystem` 单独拥有七值；WorldFile version adapter 负责读写/legacy repair；NetMessage adapter 仅输出 projection。 | `CheckSavedOreTiers` 修复前四值；多个 WorldFile 版本分支读写七值；NetMessage 顺序发送七值。生成期 `GenVars.copper/iron/silver/gold` 是不同状态，不合并。 |

写闭包判定：Version4 事实表明 P17 字段大多由 `WorldGen` pass 共享修改，不构成一个自然的单 owner。`StructureMap` 的 `Place/CanPlace/AddProtectedStructure` 是间接共享效果；`List<T>` 的 `Add/Clear` 和 arrays 的元素写不一定被 CPG assignment shape 分类为 Write。CPG 的 `complete` 仅表示范围内查询完成，许多 access 是 Unknown；唯一权威 System owner 尚未 `confirmed`。Liquid、serialization、network、DungeonControlLine 与 seed hooks 均需 integration review，禁止双写。

## Boundary Decision

总体决策：对概念行为采用 `separate` 的 capability Systems；对仍需旧 API 过渡的入口允许 `partial` compatibility facade，但它不成为 scheduler node，也不能另存第二份权威状态。叶子组是 inventory 边界，不是固定 System 数量。

不保留一个包揽 146 项的 `GenVarsSystem`：配置、reservation、测量、历史 scratch、pass 执行、存档和协议有不同不变量、effects 与生命周期；单一 owner 会隐去外部写者并难以定义提交点。也不为每个字段或每个 C01-C14 机械新建 System：部分组内已有多个独立行为，另一些共享一个提交不变量。先用 System 组件快照/Commit API 保持唯一写者，再由明确 pass/session coordinator 排序。

P17 不裁决全局 generation coordinator、secret seed owner、liquid simulation、tile/wall writer 或 WorldFile/NetMessage owner；这些决定的状态标注在 Integration Handoff。

## System API and Legacy Behavior Mapping

| Legacy behavior/入口 | Proposed composition | 兼容约束与当前状态 |
|---|---|---|
| `WorldGen.GenerateWorld` 配置与初始化 | external generation coordinator 调用配置 Adapter/hook，建立 generation-scoped snapshots，依次调用本分区 owner 的 reset/commit APIs；pass execution 仍由 integration review 的 scheduler 执行。 | 保留 `GenerateWorld` 的输入、配置 hook、seed、reset、pass 注册、成功布尔值与 finally flags。`GenerateWorld` 是协调入口，不迁成某个 P17 field owner。 |
| `GenVars.configuration`、`structures`、`copper` 等 | configuration Adapter/query；reservation owner 的 `CanReserve/Reserve` 意图；ore-selection owner 的 `CommitSelection/Snapshot`。 | 不复制 `WorldGenConfiguration` 业务规则到 Adapter；不把 `StructureMap` 的 reservation 当 Tile commit；保持 vanilla ore selection 与 tier IDs 独立。 |
| layer metrics 与 snow columns | 现存候选 `WorldLayerMetricsSystem.CalculateAndCommit` + `WorldLayerMetricsQuery.Snapshot`。 | generation identity 校验；保留全部 13 个结果与 array contents/order；当前文件存在，旧 caller routing 和 legacy parity 未验证。 |
| spawn、landmass、material、liquid boundary | 现存 `WorldSpawnAndLandmassSystem.Commit` / Query；单独 liquid-boundary owner；material catalog 只读 query。 | 保留 landmass 顺序/类型和 Remix values；waterLine load/reset 与 LiquidSimulation 的双向变化须由明确 adapter/owner 组合。 |
| beach/ocean | boundary calculation/commit 后，ocean constraints Query/commit；treasure placement 的外部成功结果再交 recorder。 | 顺序、随机 draw、失败后计数和 shell-origin side effects 必须保持；不得让 projection 直接改 tile/liquid。 |
| desert/jungle/dungeon/islands | per-capability layout/selection owner 返回 snapshot/query；placement command 经 reservation 和 tile/entity adapters 执行后提交 history。 | preserved counts, paired arrays, exact index semantics, reservation-before-placement, retry/abort behavior。当前 NSSLC 有多项对应 state/System 文件，整个调用链未见证。 |
| cave/tunnel/ore-patch/mushroom/log/lake/oasis | bounded history owners 的 query/append/clear；placement result 明确传回 System。 | 同输入同 seed 保留 append 时机、容量、overflow、重试和 stale value 行为；不根据 Component focused tests 推断已接入。 |
| `CurrentDungeon`, `CurrentDungeonGenVars`, dual dungeon property | `SelectDungeon(index)` command 写 owner；`GetCurrentDungeonSnapshot` 查询；dual-distance 只读 API 与写 command/adapter 分离。 | 保留 lower clamp、索引异常及 index/list 对齐；dual setter 必须转发到唯一 backing owner，不能创建副本或纯 Query。 |
| SavedOreTiers | `WorldSavedOreTierSystem` snapshot；`WorldFileOreTierAdapter` 做版本读写/legacy repair；`NetMessage` 使用只读七值 projection。 | 保留字段次序、Int32/Int16 wire width、版本条件、默认值、repair 条件和 packet 次序。adapter 不拥有 tier policy。 |

API 名称为描述性 proposed 契约，不表示这些 API 已实现或成为运行入口。现存 `WorldLayerMetricsSystem`、`WorldSpawnAndLandmassSystem`、`OreTierSelectionSystem` 只证明目标树中有局部 commit API，不证明新组合覆盖旧入口。

## Call and Dependency DAG

当前源码可支持的静态骨架：

```text
WorldGen.worldGenCallback
  -> WorldGen.GenerateWorld
       -> load WorldGenConfiguration
       -> Hooks.ProcessWorldGenConfig(ref configuration)
       -> construct WorldGenerator(seed, configuration)
       -> clearWorld
       -> WorldGen.Reset
            -> initialize seed/special-seed flags and RNG
            -> new StructureMap; reset selected P17 values/lists/arrays
       -> WorldGen.AddPasses
       -> DisablePassesForSpecialSeeds
       -> WorldGenerator.GenerateWorld
            -> iterate _passes in list order, pause/abort checks
            -> RunPass(_currentPass) [body stub: actual execution unknown]
       -> WorldGen.Finish
       -> finally: restore flags/temporary state
  -> SaveNewWorld only when generation result is true
```

`WorldGen.AddPasses` 先 append `TerrainPass`，再按条件 append Jungle/Skyblock 和后续 pass；`WorldGenerator.GenerateWorld` 使用 `_passes[PassResults.Count]` 顺序取项。由于 `RunPass` 是 stub，本报告只确认注册及循环结构，不把它升级为实际 tile generation/runtime ordering 的完整证明。

可定位的数据/效果依赖：

- beach boundaries -> dungeon locations 和 ocean/biome avoidance；layer metrics -> 之后的地形、biome、cave 与 ore placement。
- `GenVars.structures` -> biome placement/reservation APIs；`landmassData` -> `ExtraSpawnPointManager` 的 extra spawn candidate list。
- `dungeonGenVars/current index` -> `DungeonCrawler` 与 `DungeonUtils`；另一份 `DungeonCrawler.dungeonData` 以同一个 index 访问，需保持配对。
- `lavaLine/waterLine` -> `Liquid`; load path 先设置 `waterLine = Main.maxTilesY` 再 `Liquid.QuickWater/WaterCheck`。
- `SavedOreTiers` -> WorldFile serialization/version repair -> NetMessage outbound world-data；packet receive/negotiation 不在已确认闭包内。

CPG member-use 只提供静态 use operations；无完整 caller、event subscriber、dynamic dispatch 或 pass-edge graph。未证实的边保持 `partial`/`unknown`，不根据文件顺序追加依赖边。

## Lifecycle and Side Effects

- **Create/rebuild:** 生成入口先载入配置与 hook，再 clear world、按 seed reset 状态、建立 passes。`Reset` 设置 `Main.rand` 并重建 `StructureMap`，同时清理/初始化部分 P17 collections、bounds、counts 和 arrays。逐字段 reset completeness 尚未闭合。
- **Pass/commit:** 当前 legacy 数据为全局 static；在 proposed NSSLC 设计中候选状态应有 generation identity，commit 要拒绝 stale generation。不要把独立 pass 仅因同帧而并行；RNG stream、reservation 可见性和前后 pass 顺序未知。
- **Abort/failure:** `WorldGenerator.GenerateWorld` 有 pause 与 queued-abort 分支；outer `GenerateWorld` finally 清理 generation flags，callback 仅对 true 结果保存。`RunPass` 体被清空，pass failure、部分 Tile writes、retry/rollback 及 generator 内部清理为 `unknown`。
- **Load/persistence:** `WorldFile.LoadWorld` 依据版本选择读取路径，load 后修复 saved tiers，再进入液体恢复；P17 其他 scratch 状态是否重建、持久化或应丢弃未逐成员确认。不可把 generation snapshot 全量自动持久化。
- **Network:** 已确认 NetMessage 对七个 SavedOreTiers 以 short 顺序输出；接收路径、版本协商和连接中途状态可见性为 `unknown`。
- **Unload/multi-world:** legacy `GenVars` 为 static，目标 owner 需要 generation/world scope。并发多世界、重入、卸载、异常恢复和旧 session snapshot 是否仍可读取均未验证。
- **External effects:** Tile/Wall/Liquid edits、chest/item/NPC placement、global RNG、hook callbacks、progress/UI、file/network 是显式协作点；Component、Query 和 Projection 不直接接管这些 effect。

## Integration Handoff

以下共享边界均保留 `crossSubsystemOwner: integration-review`，P17 不定最终 owner：

- `WorldGenerator` pass coordinator、pass configuration、pause/abort/resume 与 P17 commit 可见时点：`crossSubsystemOwner: integration-review`。
- Secret-seed/options/hook inputs，包括 `flipInfections`、dual dungeon 与 generation pass selection：`crossSubsystemOwner: integration-review`。
- Tile/Wall/material catalog、`StructureMap` reservation 与具体 placement transaction：`crossSubsystemOwner: integration-review`。
- liquid thresholds、`Liquid.QuickWater/WaterCheck` 和水/岩浆传播：`crossSubsystemOwner: integration-review`。
- spawn/landmass snapshot 与 extra-spawn/NPC/world-load 的消费：`crossSubsystemOwner: integration-review`。
- DungeonGenVars/index 与 DungeonCrawler records、DungeonControlLine dual-distance backing：`crossSubsystemOwner: integration-review`。
- WorldFile version adapter、saved tier repair/save ordering、NetMessage send/receive/capability contract：`crossSubsystemOwner: integration-review`。
- global RNG stream、seed reset、random consumption order、重跑和多世界隔离：`crossSubsystemOwner: integration-review`。

每个 handoff 都需先确定唯一写者和最小输入/输出契约，再合并进跨分区调用图；不能把本报告中的 proposed owner 当成 integration decision。

## Migration Behavior Contract

迁移组合对同一 world configuration、seed、world dimensions 和前序状态必须保持如下观察向量：

| Behavior slice | 必须保持的观察值 |
|---|---|
| 配置/结构/矿石选择 | 配置 hook 输入输出，selection 值，reservation 成败与顺序，随机数消费位置，失败/重试行为。 |
| layer/surface/beach/ocean | 所有 scalar/array 值、bounds inclusive/exclusive 行为、paired coordinates、tile/liquid mutations、后续 pass 可见时间。 |
| structure/scratch histories | layout与尝试次序、容量及 overflow、成功后追加规则、count 与数组长度/顺序、Tile/Wall/item/entity effects。 |
| dungeon derived API | lower-bound clamp、index/list exception behavior、DungeonCrawler 数据同步、alias setter 写到同一 backing state。 |
| SavedOreTiers | 新旧版本 load defaults、`-1` repair gate、tile-count repair 结果、save 序列化字段顺序/宽度、packet 顺序/宽度。 |
| generation lifecycle | true/false/abort 返回值、SaveNewWorld 条件、finally cleanup、failed generation 后状态、seed replay 与第二次 generation。 |

建议的实施顺序是先固定 generation-scoped snapshots 与唯一 commits，再迁配置/reservation、metrics/surface/beach constraints、各结构/history owners，最后迁 Dungeon alias 与 WorldFile/NetMessage adapters。每一步记录旧入口命中的新组合；不要并行跑仍依赖同一 RNG stream 或 StructureMap 的行为；切换期间不得双写。真实迁移成功门槛是必需行为测试命中新 owner 和新组合后通过。当前没有行为等价结论，也未宣称迁移成功。

## Evidence Gaps and Blocking Decisions

- **Blocking:** 当前 Version4 CPG 没有 `SourceSnapshotId`，导出边无相邻源码树；源目录也无 Git revision。需为关键 pass/serializer 建立可绑定源码快照后才能宣称完整关系闭包。
- **Blocking:** `WorldGenerator.RunPass` stub 使 P17 pass 调用、Tile write、外部 effects 与 pass exception 行为 `unknown`。需获取可信实现或行为 oracle 后定 scheduler ordering 与 parity。
- **Blocking:** `waterLine` 同时被 generation/load/Liquid 路径写；`DualDungeon_NormalizedDistanceSafeFromDither` alias writes `DungeonControlLine`; dungeon index 同时索引两个 records collection。需要 integration review 指定唯一 backing owners、恢复顺序和事务边界。
- **Blocking:** SavedOreTiers 的三个 hardmode 值与四个 pre-hardmode 值位于多个版本分支和 packet/save 边界；receiver 与完整 capability/restore contract 未闭合。
- **Unknown:** 全部 P17 成员的 inbound callers、indirect List/array mutations、reflection/delegate/hook entry points、exception/unload lifecycle、multi-world scope、generation abort/retry 与 state reset closure。
- **Unknown:** `hellChest` CPG 查询零命中带 gap；静态 source read 找到消费处，但完整所有读写点仍未知。
- **Unknown:** 当前 `src/NSSLC` 各 System 的外部 callsites、scheduler registration、compat facade 保留情况和全部 Version4 writer 替换情况。历史 Component build/run 状态不是本轮 evidence。
- **Decision for integration review:** reservation/state ownership、Liquid threshold writer、Dungeon record pairing、seed/RNG ownership、save/network version contract，均不可由 P17 单独决定；未决定前依赖这些决策的系统接入保持 proposed。

## Verification Plan

`verificationStatus: not-run`。本会话只读取查询/源码并编写报告；没有修改 C#、测试或项目文件，也没有运行 build、test、run 或 behavior verifier。以下为以后需要执行的计划，不代表已经通过：

1. 对每个 capability owner 用真实 pass/API 入口验证唯一写者、generation identity、snapshot 顺序、reset/restore 与失败后的状态；确认新入口已接入调度。
2. 同 seed、尺寸、规则下逐阶段比较 layer metrics、beach/ocean constraints、spawn/landmass、paired structure coordinates、history counts 和最终 world/tile hash；比较 RNG consumption 顺序与副作用次数。
3. 验证 Liquid、StructureMap reservation、Tile/Wall/material、chest/item/NPC effects 的提交顺序、失败和 retry 路径。
4. 对 WorldFile 覆盖版本阈值、四 tier `-1` repair、三个 hardmode tier defaults、保存/读取顺序；对 NetMessage 覆盖七值宽度与 send/receive 端顺序。
5. 验证 `CurrentDungeon` 边界、index 超界错误、dual index records 配对、alias setter 单写、跨 world/rebuild/unload 隔离，以及 generation abort 后再次执行。
6. 只有真实迁移项目的必需行为测试通过且命中新 owner/API composition 后，才可升级 `designStatus` 或称为 migration success；编译、旧 Component focused verifier、静态映射和文件存在都不能替代该门槛。
