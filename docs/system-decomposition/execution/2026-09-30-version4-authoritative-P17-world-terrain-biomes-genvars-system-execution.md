# Version4 authoritative P17 World Terrain, Biomes, and GenVars System Execution Plan

| 项目 | 内容 |
| --- | --- |
| 分区 | `P17` (`WorldGenerationAndEcology`) |
| 执行计划状态 | `proposed` |
| 证据状态 | `partial` |
| 验证状态 | `not-run` |
| 源码变更 | `true`（仅局部 P17 组合切片） |
| 设计文档 | [`2026-09-30-version4-authoritative-P17-world-terrain-biomes-genvars-system-design.md`](../design/2026-09-30-version4-authoritative-P17-world-terrain-biomes-genvars-system-design.md) |
| 基线报告 | [`2026-09-18-system-decomposition-authoritative-P17-world-terrain-biomes-genvars.md`](../reports/2026-09-18-system-decomposition-authoritative-P17-world-terrain-biomes-genvars.md) |
| 原报告 session | `95cb4a271e0346218cb14565179c4840` |
| 目标源码 | `D:\TRbackup\Version4` |
| 完整参考项目 | `D:\TRbackup\无任何删减通过编译`（用户给定为无删减且通过编译；本任务不重跑构建） |
| SS14 组织参考项目 | `C:\Users\shan\Downloads\ECS\space-station-14-master`（仅观察 System 组织方式，不作为行为证据） |
| 迁移代码目录 | `D:\TRbackup\NLTX\src\NSSLC` |

designStatus: proposed
evidenceStatus: partial
verificationStatus: not-run
sourceModified: true
implementationStatus: partial
coreSliceVerification: passed

`sourceModified: true` 反映工作区已经存在 P17 局部实现；这些切片覆盖 pass 控制边界和
SavedOreTiers load boundary，并有独立的约 10% 核心 verifier 历史记录，未接入真实生产
调度。本次文档修订只补充完整参考项目与只读 CPG 的 Mushroom/fallen-log 证据，不修改
生产代码，也不启动 build/test。

## 1. 执行目标

本计划规定把 P17 的 14 个叶子组、146 个成员按唯一 owner、受控 API 和生成 session
顺序接入未来真实迁移项目。它是执行顺序和验收门槛，不是完整迁移结果。本轮实现了
生命周期/pass 控制边界和 SavedOreTiers load boundary，新增独立的约 10% 核心 verifier；
没有接入真实
`WorldGen.GenerateWorld`、跨分区 coordinator 或外部效果 owner。

执行成功的定义是：真实迁移项目的必需行为测试命中新 owner 和组合，并在相同 seed、
尺寸、配置和前序状态下通过完整观察向量。编译、局部 focused verifier、静态覆盖或
旧 facade 仍能调用，都不能单独升级为迁移成功。

## 2. 不可违反的执行不变量

1. **一分区一 owner 计划。** 本次只处理 P17；P18/P19/P20 和其他分区只通过 handoff。
2. **一个权威写者。** 迁移窗口不得让 legacy writer 与新 System 同时写同一不变量。
3. **先证据后接线。** 任何 `unknown` 关系先补目标源码或只读 CPG 查询；不能用文件名、
   类名或完整参考树猜测 caller、scheduler 或 dynamic dispatch。
4. **按 session 隔离。** 所有 P17 component、snapshot、reservation 和 history 带
   `WorldId`/`GenerationId`；stale commit 必须被拒绝或显式回滚。
5. **预约不等于效果。** `StructureMap`/reservation、Tile/Wall、Liquid、item/NPC 和
   网络/存档各自有 port/adapter；只有外部提交成功才能追加成功 history。
6. **不得双写或隐式并行。** 共享 RNG、pass-visible state、paired dungeon records、
   liquid barrier 或 reservation 的阶段默认串行，直到行为证据允许并行。
7. **缺口显式化。** 未闭合关系标 `unknown`，索引范围不足标 `partial`，未来设计标
   `proposed`；不把“无命中”写成“无调用”。

## 3. 前置证据冻结

在任何生产迁移任务开始前，保存以下只读输入的版本记录：

| 输入 | 必须保存 | 当前状态 |
| --- | --- | --- |
| P17 System 报告与 `outputReport` | 原始 `sessionId`、`designStatus`、`verificationStatus`、146 成员清单 | 已有；沿用 `95cb4a271e0346218cb14565179c4840`，`proposed`/`not-run`，不重写、不重复结算 |
| Version4 源码 | `GenVars.cs`、`WorldGen.cs`、`WorldGenerator.cs`、`WorldFile.cs`、`Liquid.cs`、`NetMessage.cs`、Dungeon 文件哈希 | 已读取；CPG 无 `SourceSnapshotId` |
| 完整参考项目 | `D:\TRbackup\无任何删减通过编译` 的对应文件哈希和阅读备注 | 已读取；只作关系校对，不替代 Version4 证据 |
| CPG 数据集 | manifest、ProjectFingerprint、查询范围、结果和 gaps | 已读取；只读 `complete`/`partial` 结果 |
| 当前 NLTX | `src/NSSLC/Component/WorldSession/WorldGeneration` 文件清单和 API 形状 | 局部存在；caller/scheduler 未证实 |

SS14 参考树只用于组织方式对照：阅读了 `Content.Shared/Teleportation/Systems/LinkedEntitySystem.cs`
和 `Content.Client/DisplacementMap/DisplacementMapSystem.cs`，确认其 System 以领域目录归属、
显式组件读写和公开行为 API 组织；没有把 SS14 的调度、事件或行为规则当作 Version4 证据。

### 3.1 证据补查顺序

若执行过程中发现关系不足，按下面顺序补证据，完成前不得接入依赖该关系的 System：

1. 先查目标 Version4 的声明、实现、所有直接使用点、注册点和相邻 lifecycle 入口。
2. 再用 `CpgEvidence.ps1` 的 `Find-CpgSymbols`、`Get-CpgTypeSurface`、
   `Get-CpgMemberUses`、`Find-CpgCallSites` 做有边界查询。
3. CPG `complete` 仅表示选定索引范围查询完成；`Unknown`、zero-hit 和缺 shard 保留为
   `partial`/`unknown`。
4. 最后用完整参考项目核对语句顺序和非 stub 行为，并标明它不是 Version4 runtime 证明。

### 3.2 完整参考项目静态核对

对 `D:\TRbackup\无任何删减通过编译` 的源码做了只读对照，不运行其构建，不把目录名中的
“通过编译”当作当前迁移验收：

| 关系 | Version4 | 完整参考项目 | 结论 |
| --- | --- | --- | --- |
| `WorldGen.GenerateWorld` / `Reset` / `AddPasses` | 均有静态声明和入口调用 | 均有对应声明和入口调用，行号因源码完整度不同而变化 | 可校对入口顺序；caller 闭包仍为 `partial` |
| `WorldGenerator.RunPass` | 方法体为返回空 `GenPassResult` 的 stub | 处理 disabled pass、随机源重置、progress、异常捕获和结果字段 | 参考行为向量；Version4 目标行为保持 `unknown` |
| `GenVars.CurrentDungeon` / `dungeonGenVars` | 有下限夹紧和双记录读写点 | 有相同概念，且存在更多读取点 | 只确认概念关系；索引/恢复契约仍需目标树验证 |
| `WorldFile.CheckSavedOreTiers` | 有 `-1` repair 与七值写入 | 对应布局可逐语句校对 | 保存语义仍需 NLTX 行为验证 |
| `Liquid` / `NetMessage` | 有边界消费和出站字段 | 对应关系可用于顺序校对 | 不闭合 inbound、调度和运行时副作用 |
| Mushroom anchor pass | `WorldGen.cs:11707-11726`：1+5 次 `ShroomPatch` 后写 anchor/count | `WorldGen.cs:12987-13006`：同一顺序 | `ShroomPatch` 是 void；只确认正常返回顺序，不能推出 effect 成功 |
| Fallen-log -> `Flowers` | producer `WorldGen.cs:17771-17772`；consumer `19627-19632,19691-19695` | producer `19051-19052`；consumer `20907-20912,20971-20975` | 两条消费分支只清 `logX`；可作为 handoff 目标，运行时可达性仍待验证 |

上述对照只补充设计候选和证据缺口，不升级 `proposed`、`partial`、`unknown` 或
`verificationStatus: not-run`。

本轮为 pass 顺序和 SavedOreTiers load boundary 补查了 CPG：`Find-CpgSymbols(RunPass)` 在
`Terraria.WorldBuilding/WorldGenerator.cs` 返回 `complete` 的唯一方法符号；
`Get-CpgCallableFacts` 返回 `partial`，仅有 5 个 CFG/operation 节点、无 direct call target，
并带 `CalleeEffectsNotExpanded` gap。该结果与目标源码 `RunPass` 只返回空
`GenPassResult` 一致，因此只实现 manifest 顺序控制，不实现参考项目中的 pass effects、
progress、随机源或异常语义。
`Find-CpgSymbols(CheckSavedOreTiers)` 返回 `complete`，选定 WorldFile/WorldGen 范围的
调用点只提供静态入口事实；其 `Get-CpgCallableFacts` 仍为 `partial`，带
`CalleeEffectsNotExpanded`，所以 load boundary 只组合已确认的版本字段映射和 repair，
不宣称闭合真实文件 I/O 或 Liquid barrier。

本次文档补查 Mushroom/fallen-log 的 CPG 结果：`mushroomBiomesPosition=3`、
`numMushroomBiomes=5`、`logX=8`、`logY=4` 个 selected-scope member-use；前两组全为
`partial`/`AccessMode: Unknown`，`logX` 有 4 个 confirmed 写入与 4 个 partial 使用，
`logY` 有 2 个 confirmed 写入与 2 个 partial 使用。`ShroomPatch` 有 2 个
`internal-static-exact` 调用点，callable facts 为 `partial`，带
`CalleeEffectsNotExpanded` gap。查询结果只能证明索引内静态关系，不能闭合 alias、Tile/
liquid effects、异常或调度。

因此 Wave D 必须把六次 `void ShroomPatch` 的“正常返回”作为待实现的 effect-adapter
信号；没有该信号时 anchor append 保持 `unknown`。fallen-log publish 必须使用
`PlaceTile` 后 tile type 488 的事实和同一随机决策；`Flowers` 需要分别覆盖 Remix/普通
分支，并保留消费后只清 `logX=-1` 的旧观察值。

本轮同时查询 `GenVars.mCaveX/mCaveY` 的 member-use：在选定的
`WorldGen.cs`、`GenVars.cs`、`WorldFile.cs` 范围内分别返回 7/3 个 `partial` 结果；
`AccessMode`、alias mutation 和 callee effect 均未闭合。阅读 `D:\TRbackup\Version4` 与
`D:\TRbackup\无任何删减通过编译` 的 `WorldGen.cs` 后确认，完整参考树在
`Mountinater(x, y)` 调用之后才写入两个坐标数组并递增 `numMCaves`，但调用没有成功返回值。
CPG 在选定 `Terraria/WorldGen.cs` 范围内对 `Mountinater`、`OrePatch` 各返回 1 个调用点，
对 `TileRunner` 返回 74 个调用点；这些结果只说明 selected-scope 静态关系，未闭合 callee
effects 或完整运行时入口。
执行时必须先定义 cave effect 的提交信号，再允许 history append；本轮已在 NLTX
  `MountainCaveHistorySystem` 增加 `AppendAfterSuccessfulCommit` 结果边界和 bool 便捷入口，
  并保留 capacity-30、成对 X/Y 顺序和失败不变更。真实 `Mountinater`/Tile effect port 尚未
  接入；旧 `TryAppend` 入口保留为兼容转发，生产组合必须使用显式成功信号，因此仍不能
  记为真实调度已接线或行为等价。

## 4. 分阶段执行顺序

下表是唯一建议顺序。阶段未执行，`执行状态` 全部保持 `planned`。

| 阶段 | 输入 | 唯一 owner/输出 | 静态门槛 | 后续行为门槛 | 执行状态 |
| --- | --- | --- | --- | --- | --- |
| 0. 证据冻结 | 报告、Version4、CPG、完整参考树、NLTX 文件 | `EvidenceBaseline`；不写生产状态 | 146/146 成员、哈希、查询范围和 gaps 可追溯 | 同一源码快照下重跑关键查询并记录结果 | `planned` |
| 1. 集成决策 | P18/P19/P20/Liquid/WorldFile/NetMessage handoff | `IntegrationDecisionRecord`；确定 owner、phase、barrier、rollback | 每个共享状态只有一个 owner，未知项有负责人和补证据动作 | 通过跨分区 contract review；未决项不能进入 cutover | `blocked-by-integration-review` |
| 2. Session 基础 | world/seed/dimension/config 输入 | `WorldGenerationSession` 与 `GenerationId` | 每个 P17 snapshot 带 identity；stale commit 有拒绝路径 | 新建、重建、abort、unload、多 world 隔离测试 | `planned` |
| 3. 纯状态与 Query | C01-C14 component/snapshot 形状 | 各 capability owner 的 immutable snapshot/query | Query 不写 backing、无 RNG/clock/event/cache side effect；数组/List 拷贝 | Query 结果、异常、边界和重复调用行为对比 | `planned` |
| 4. 外部 Adapter/Projection | 配置 hook、DungeonControlLine、WorldFile、NetMessage、material/liquid ports | 边界 adapter/projection；不持有领域副本 | 版本、字段顺序、signed width、alias backing 记录完整 | save/load/network/liquid/dungeon round-trip 和错误路径 | `planned` |
| 5. Reset/Restore/Reservation | legacy Reset、StructureMap、history、dungeon records | `ResetSystem`、`RestoreSystem`、`StructureReservationSystem` | reset 覆盖清单、原子 snapshot replace、无旧 generation 残留 | 失败重试、overflow、reservation 冲突、restore rollback | `planned` |
| 6. 生成依赖波 | layer -> surface/beach -> biome/structure -> history/special | P17 capability Systems 与 pass coordinator | DAG 顺序、visibility、RNG stream、append-after-success 门槛明确 | 同 seed 分阶段比较、tile/liquid/effect 顺序 | `planned` |
| 7. 生命周期接入 | `WorldGen.GenerateWorld`、`WorldGenerator.GenerateWorld`、callback/save | P19 coordinator 调用 P17 APIs | true/false/abort/pause/finally 与 SaveNewWorld 条件映射 | 真实入口命中新组合，控制/异常/重入测试 | `planned` |
| 8. 单 writer cutover | 旧入口、read adapter、新 owner | 新 owner 成为唯一写者 | 无双写、旧 writer 只读或已移除，rollback 开关可用 | legacy/new observation vector 全量通过 | `planned` |
| 9. 删除门禁 | behavior test ledger、rollback 记录 | 移除旧 writer/facade（另行授权） | 所有 handoff、保存、网络、multi-world gate 通过 | 真实迁移项目回归通过后才可删除 | `planned` |

## 5. P17 capability 执行顺序

### 5.1 Wave A：输入、配置与基础测量

1. `WorldGenerationConfigurationAdapter` 先读取配置并执行已确认的 hook 组合；它只产出
   immutable configuration snapshot。`configuration` 的 CPG 15 个 `Unknown` use 不得被
   当成完整 writer 闭包。
2. `StructureReservationSystem` 建立 reservation intent/query/commit。预约成功只发布
   reservation token，不改 Tile/Wall。
3. `GenerationOreSelectionSystem` 提交 `copper/iron/silver/gold` 与 bar IDs；不要把
   这些 generation values 和 C14 SavedOreTiers 合并。
4. `WorldLayerMetricsSystem` 计算并一次性提交 13 个 layer/snow 成员；下游只读 snapshot。

静态门槛：配置 hook 输入输出、reservation token、RNG source、generation ID 和数组有效
长度均有明确字段；所有 writer 可定位到一个 owner。行为门槛：同 seed 下 layer 数值、
snow columns、矿石选择、reservation 成败和随机消费位置一致。

### 5.2 Wave B：surface、liquid、beach 和 ocean

1. `WorldSpawnAndLandmassSystem` 只提交 spawn/landmass snapshot；`ExtraSpawnPointManager`
   的消费作为外部 handoff。
2. `SurfaceMaterialQuery` 只提供 material facts；Tile/Wall mutation 由 P20/Tile owner。
3. 在 integration review 决定 `waterLine`/`lavaLine` 唯一 writer 和 load barrier 后，才接入
   `WorldGenerationLiquidBoundarySystem`。
4. `BeachBoundarySystem` 提交左右 boundary、shell origins 和输入常量；保留 Version4
   inclusive/exclusive 区间和 RNG 顺序。
5. `OceanBiomeConstraintSystem` 读取 beach snapshot，记录 avoidance/treasure attempts；
   `OceanCaveTreasureSystem` 仅在 placement effect 成功后递增 `numOceanCaveTreasure`。

静态门槛：`waterLine` 的 generation、WorldFile、Liquid 三方写点有唯一 owner 方案；
beach/ocean 的输入输出和失败重试可追踪。行为门槛：load 后 `waterLine=Main.maxTilesY`、
`Liquid.QuickWater`、`WorldGen.WaterCheck` 顺序，beach boundaries 和 treasure attempts
与旧行为一致。

### 5.3 Wave C：dungeon、island、desert 和 jungle

1. `DungeonSelectionControlSystem` 先创建/清理 P17 dungeon records，再提交
   `CurrentDungeon`；每次 index 切换必须同时能索引 `GenVars.dungeonGenVars` 与
   `DungeonCrawler.dungeonData`。
2. `FloatingIslandPlacementSystem` 管 island house/lake arrays；不得写 dungeon records。
3. `UndergroundDesertStructureSystem` 计算 bounds，larva paired arrays 只在 placement
   成功后 append；reservation/solidity 走外部 port。
4. `JungleRegionStructureSystem` 先提交 region；`PyramidPlacementSystem` 管 paired
   `PyrX/PyrY`；`JungleChestLootSystem` 管 item cursor/chest coordinates。三者按 pass
   顺序串行，不能共享隐式 `GenVars` 写权。

静态门槛：dungeon 双 List 的长度/index/restore contract、`CurrentDungeon` 下限规则、
dual-distance backing owner 和各 paired coordinate 原子提交可定位。行为门槛：双 dungeon
切换、bounds、pyramid/chest/larva 成功与失败重试、DungeonUtils 读取和异常边界一致。

### 5.4 Wave D：history、hell 与 special structures

1. `CaveTunnelHistorySystem` 与 `OrePatchHistorySystem` 使用 bounded `TryAppendAfterSuccess`；
   overflow 不得静默扩容或改变旧异常语义。MountainCave 的局部 recorder 已先把
   `Mountinater`/cave effect 的成功结果抽象为 `caveCommitted`，再调用 history append；
   真实信号缺失时仍维持 `unknown`/`planned`，不得用无条件 `TryAppend` 假设成功。
2. `MushroomBiomeGenerationSystem` 只维护 anchor history，不执行 `ShroomPatch`。接线时由
   effect adapter 顺序调用 1 次主 patch 和 5 次邻点 patch；六次 `void` 调用全部正常返回后
   才传入显式 `PatchBatchCompleted`，再 append 成对 anchor/count。不得把 Tile 写入猜成
   patch success；异常、部分写入和 `RunPass` 处理仍保持 `unknown`。
3. `FallenLogFlowerHandoffSystem` 的 producer 先检查 `PlaceTile` 后 tile type 488，再消费
   同一次 1/2 RNG 决策后发布 `logX/logY`。`Flowers` 的 Remix 与普通分支都必须通过一个
   原子 `TryConsume` seam 覆盖 pending 坐标、扫描坐标替换和 `logX=-1` 清理；`logY` 保留
   旧值。没有两条真实分支的入口证据时保持 `planned`。
4. `LakeGenerationSystem`、`OasisGenerationSystem` 各自维护 bounded history，由 coordinator
   排序；liquid/vegetation effects 不从 history component 直接发生。
5. `HellChestGenerationSystem` 管 item sequence/cursor；`StatueCatalogSystem`、
   `InfectionAlignmentSystem`、`ShimmerAnchorSystem` 分别管理其不变量。secret-seed 输入、
   RNG stream 和 NPC/Tile effects 都要显式 handoff。

静态门槛：每个 count/array 的 capacity、成功 append、reset/restore 和 generation ID 规则
有审计记录；`hellChest` 的 zero-hit gap 已补源码或继续标 `unknown`。行为门槛：overflow、
retry、特殊 seed、随机顺序、item/NPC/Tile effect 次数和失败补偿一致。

### 5.5 Wave E：派生 API 与持久化/网络

1. `DungeonDerivedPropertiesSystem` 把 `CurrentDungeon` setter 当 Command；getter 和
   `CurrentDungeonGenVars` 通过 immutable snapshot Query；dual-distance setter 走唯一
   `DungeonControlLine` adapter。
2. `WorldSavedOreTierSystem` 是七值唯一 owner。`WorldFileSavedOreTierAdapter` 处理 v23、
   v54、v216 等版本门槛、旧 sentinel 和四值 repair；repair 完成后一次性发布 state。
3. `NetMessageSavedOreTierProjection` 只编码 Copper/Iron/Silver/Gold/Cobalt/Mythril/
   Adamantite 的固定顺序和 signed `short` 宽度。入站消费、capability negotiation 和
   连接中途可见性必须先由 integration review 定义。

静态门槛：保存/读取字段顺序、版本分支、repair 输入、网络 wire order、alias backing 和
   inbound owner 均有源代码证据。行为门槛：v23/v54/v216、`-1` repair、altar sentinel、
   high-tier save order、packet round-trip、dungeon index exception 和 alias write 测试。

## 6. 生命周期和故障处理步骤

### 6.1 Create、Reset、Rebuild

1. coordinator 创建 `WorldId`/`GenerationId`，冻结 seed、dimension、configuration 和
   RNG stream identity。
2. 清理 Tile/StructureMap 等外部状态后调用各 P17 ResetSystem；Reset 必须生成新数组/List
   或清空到明确 sentinel，不复用旧引用。
3. 建立 dungeon record pair、hell chest sequence、saved-tier defaults 和 history capacity。
4. 只有 Reset snapshot 校验通过后才允许 AddPasses/调度 pass。

### 6.2 Pass、Pause、Abort、Retry

- pass coordinator 先发布当前 pass identity 和输入 snapshot，再调用 capability System。
- pass 结果在下一个 pass 可见前完成 P17 commit；涉及 RNG/reservation/paired state 的节点
  不并行。
- pause/abort 必须能停止新 commit；已完成的外部 effect 如何补偿由 P19/P20 integration
  owner 决定，未决定时标 `unknown`。
- 候选失败时恢复 candidate-local state；只有 tile/liquid/entity transaction 成功后才
  append history 或发布 treasure/chest result。
- 目标 Version4 `RunPass` 是 stub；参考树的 exception/progress/`GenPassResult` 行为只
  作为未来 gate，不能在本计划中直接实现为已证实契约。

### 6.3 Finish、Unload、Multi-world

- 生成返回 true 后才执行旧入口的 `SaveNewWorld` 条件；finally 必须清掉 generation flags。
- unload/rebuild 销毁 session-scoped components、取消 pending commands、使旧 snapshot
  失效；不能让 static compatibility facade 指向新 world。
- 在没有并发隔离证据前，按单 active world 执行；如果要支持多 world，先增加 world identity
  和 RNG/Tile/WorldFile ownership 行为测试。

## 7. 持久化、网络与回滚计划

### 7.1 WorldFile

先实现纯 `Read`/`PrepareSave` adapter，再由唯一 WorldStorage owner 调用。四个低阶 tier
缺失时基于同一 tile-count snapshot 计算并一次性 commit；三个高阶 tier 遵循现有版本/altar
分支。读取、repair、Liquid load barrier 和保存必须是明确的阶段，不允许让 adapter 直接
写 `GenVars` 与新 component 两处。

### 7.2 NetMessage

出站 projection 只读 C14 snapshot 并返回七个 signed `short` 字段。发送成功不改变 owner；
入站 decode、版本协商、能力声明和连接中途 state visibility 未有完整证据，先保持
`crossSubsystemOwner: integration-review`。

### 7.3 回滚

回滚点按以下顺序保存：

1. legacy facade/read adapter 仍可读取的旧状态快照；
2. P17 immutable snapshots 和 session metadata；
3. 外部 reservation/effect transaction（若外部 owner 支持）；
4. WorldFile/network projection 的边界日志。

任一 behavior gate 失败时停用新 commit，恢复旧 writer 和旧入口；不得删除用户 world、
   不得回写半修复 SavedOreTiers、不得重复发送已成功的外部 effect。若外部 effect 无法
   回滚，则在 integration decision 中禁止该切换，不能靠文档假设补偿。

## 8. 验证计划与本轮结果

全量行为验证仍未执行；完整参考项目的既有通过编译状态只作为输入前提，不作为 NLTX
验证结果。下列结果只覆盖约 10% 的核心切片，不能升级 `designStatus`，也不能证明迁移
成功。

### 8.1 已执行的核心切片验证

项目：
`Test/Terraria.WorldSession.WorldGeneration.P17.CoreVerification/Terraria.WorldSession.WorldGeneration.P17.CoreVerification.csproj`

构建前检查了共享 `dotnet.exe`/`csc.exe` 进程，使用临时 PATH 指向
`D:\TRbackup\dotnet-sdk-10.0.400`，并通过 `Build/Tools/Invoke-SerialDotnet.ps1` 串行执行：

```text
$env:PATH = 'D:\TRbackup\dotnet-sdk-10.0.400;' + $env:PATH
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @('restore', '.\Test\Terraria.WorldSession.WorldGeneration.P17.CoreVerification\Terraria.WorldSession.WorldGeneration.P17.CoreVerification.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @('build', '.\Test\Terraria.WorldSession.WorldGeneration.P17.CoreVerification\Terraria.WorldSession.WorldGeneration.P17.CoreVerification.csproj', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @('run', '--project', '.\Test\Terraria.WorldSession.WorldGeneration.P17.CoreVerification\Terraria.WorldSession.WorldGeneration.P17.CoreVerification.csproj', '--no-build', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')
```

结果：restore 退出码 `0`；build 退出码 `0`，0 warnings、0 errors；产物位于
`Build/bin/Terraria.WorldSession.WorldGeneration.P17.CoreVerification/Debug/net10.0`；run
退出码 `0`，输出：

```text
PASS: P17 lifecycle, ordered pass commit, C02, C13, and C14 core slice
```

覆盖内容为 lifecycle 合法迁移/stale generation、disabled/enabled pass 与 ordered result
commit、active 状态控制拒绝、checkpoint、C02 snapshot 拷贝、C13 dungeon 下限/record index、
C14 v23/altar sentinel、`-1` repair/tie、load 失败保留旧 snapshot、七值 commit 和七值
short projection，以及 C09 Mountain Cave 的成功提交门禁、成对坐标顺序、capacity-30，
Surface Tunnel 的 scan-accept 门禁/effective capacity-49，和 OrePatch 的成功提交门禁/
effective capacity-49。

本轮第一次增量 build 因 verifier 缺少 `Terraria.WorldGeneration` using 退出码 `1`（4 个
CS0103，0 warnings）；补齐 using 后按同一串行命令重跑，最终 build 退出码 `0`、0 warnings、
0 errors。该失败只属于测试入口编译修正，不改变生产代码证据状态。

### 8.2 未执行的完整验证

未来实施任务必须按真实入口执行以下验证，结果单独记录命中的 owner 和组合：

1. 静态唯一 writer/reader 盘点：每个 P17 成员、indirect List/array mutation、hook、
   reflection、event、serialization 和 network path。
2. API contract：Query 无副作用，Command 的 generation check、幂等、异常、返回值和可见时点。
3. Reset/restore：新建、第二次 generation、失败重试、abort、pause、unload、跨 world 隔离。
4. 同 seed/尺寸/配置逐阶段比较 layer/surface/beach/ocean/dungeon/history/special state，
   以及 RNG consumption、pass order、reservation 和 paired coordinates。
5. Mushroom/fallen-log 专项：比较六次 patch 的正常返回/异常路径、anchor append 顺序和
   capacity；验证 `PlaceTile` 后 type 488、1/2 随机门、Flowers Remix/普通两分支、只清
   `logX`、重复消费和 reset sentinel。只读 CPG 不能替代这些行为测试。
6. Tile/Wall/Liquid/item/NPC effects：提交顺序、次数、失败补偿、Liquid load barrier。
7. WorldFile：版本阈值、`-1` repair、三个 high-tier defaults、读写顺序和损坏文件恢复。
8. NetMessage：七值 wire order、signed width、inbound consumer、capability negotiation。
9. 只有必需行为测试全部通过且新路径被真实入口调用，才允许把对应项目升级为 verified；
   在此之前保持 `proposed`/`not-run`。

## 9. 删除与切换门禁

删除任何 legacy writer、`GenVars` facade 或旧 adapter 前，执行者必须在台账中提供：

- ConceptId 到新组合的真实调用证据；
- 唯一 owner、读写集、phase/barrier 和 rollback 点；
- 受影响的 persistence/network/liquid/dungeon handoff 已由 integration owner 签收；
- 同 seed 观察向量、异常/重试/幂等、多 world/session 和外部 effect 测试通过；
- 回滚演练记录，确认没有双写、旧 state 残留或重复 packet/event。

缺一项就保持旧 writer 和 proposed adapter，不能以“完整参考项目通过编译”替代当前
Version4/NLTX 行为证据。

## 10. 当前执行记录

以下局部实现与核心 verifier 条目是工作区已有的历史记录，不是本次文档修订新执行的
代码或构建动作。

- 已完成 Version4、完整参考项目和 CPG 的只读证据补查；原始 P17 runner session
  `95cb4a271e0346218cb14565179c4840` 已是 `completed`，未重复结算。
- 本轮修改了 `D:\TRbackup\NLTX\src\NSSLC` 的 P17 pass 控制边界：新增 manifest-aware
  `BeginPass` 组合，拒绝跳过 manifest 前缀之外的 pass；并保留 disabled/active/failure/
  abort 检查；`IWorldGenerationPassRunner` 只作为外部执行 seam 接收显式结果，不实现
  Version4 `RunPass` 缺失的 pass effects；新增独立核心 verifier；
  未修改 `D:\TRbackup\Version4`、`D:\TRbackup\无任何删减通过编译`、P17 输入报告、ledger
  或 runner 状态；既有验证入口保持原始内容。
- 本轮新增 `WorldSavedOreTierLoadSystem`，把版本化 WorldFile 字段读取、低阶 repair 和
  state 替换组合为一个显式边界；它不接管真实文件 I/O，也不宣称闭合 `waterLine`/Liquid
  load barrier。核心 verifier 新增 v23/altar sentinel、直接字段缺失时不改旧快照和单次
  load commit 检查。
- 本轮新增 `MountainCaveHistoryAppendStatus`、`MountainCaveHistoryAppendResult` 和
  `MountainCaveHistorySystem.AppendAfterSuccessfulCommit`；核心 verifier 覆盖失败提交不
  追加、成功提交的成对坐标顺序和 capacity-30 拒绝边界。真实 `Mountinater` effect port、
  scheduler、rollback 和 legacy parity 仍未执行。
- 本轮补齐 C09 的三个提交边界：`SurfaceTunnelHistorySystem.AppendAfterSuccessfulScan`
  只接受十点 scan 成功结果，`SurfaceOrePatchHistorySystem.AppendAfterSuccessfulCommit`
  返回显式 rejected/appended 结果；两个组件的原始 append 收紧为程序集内部，避免绕过
  System 门禁。核心 verifier 覆盖 scan/commit 失败不变更、插入顺序和两个 effective
  capacity-49 边界。
- 本轮已执行受影响项目的 build/run 核心切片验证；build 退出码 `0`、0 warnings、0 errors，
  run 退出码 `0` 并输出 `PASS: P17 lifecycle, ordered pass commit, C02, C13, and C14 core slice`；
  完整迁移入口、行为等价、跨分区
  handoff、WorldFile/Liquid/NetMessage/network inbound、multi-world 与删除门禁仍未执行。
- 当前状态：`executionStatus: partial`、`implementationStatus: partial`、
  `coreSliceVerification: passed`、`verificationStatus: not-run`（全量行为验证）。
- 因此不称迁移成功，不称行为等价，不称生产接入完成。
- 本次仅补查 `mushroomBiomesPosition`、`numMushroomBiomes`、`logX`、`logY`、
  `ShroomPatch` 的只读 CPG 结果，并阅读目标树与完整参考树的 Mushroom、fallen-log、
  `Flowers` 顺序；未修改 `src/`、`Test/`、项目文件或 runner 状态，未启动 build、test
  或 run。

## 11. 交接清单

开始真实实施前，下一执行者必须取得以下明确输入：

1. integration-review 对 pass coordinator、seed/RNG、Tile/Wall、Liquid、Dungeon 双 List、
   WorldFile、NetMessage、NPC/spawn 和 multi-world owner 的决策。
2. 可绑定的 Version4 source snapshot 或针对 stub/动态入口的行为 oracle。
3. 当前 NLTX scheduler/注册/调用者清单，以及每个 capability 的实际入口。
4. 允许执行的行为测试范围、artifact 路径和 rollback 操作授权。

在这些输入到位前，本文件保持 `proposed`，所有未决项保持 `unknown` 或 `partial`。
