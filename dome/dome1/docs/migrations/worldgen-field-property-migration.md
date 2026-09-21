# WorldGen.cs 字段与属性 ECS 迁移说明

**状态：** `Partial / In Progress`<br>
**审查日期：** 2026-08-30<br>
**事实源：** `D:\TRbackup\Version4物理删除了某些文件\Terraria\WorldGen.cs`<br>
**目标实现边界：** `src/Terraria.Dome.Simulation/WorldGeneration/`

## 1. 结论

本文件把 `WorldGen.cs` 的字段和属性从 God Object 责任中拆出，绑定到 ECS 的
`Definition -> Component/Snapshot -> System -> Typed Command -> Commit` 链路。
它不是把旧静态字段逐个复制到另一个静态类，也不宣称 WorldGen 行为已经完成迁移。

研究报告的 Version4 词法基线为 **230 个字段 + 46 个属性块 = 276 项**。源清单
`docs/worldgen/worldgen-source-inventory.json` 为 **233 个 FieldDeclaration**；对当前
源码做 Roslyn 语法树扫描得到 **31 个 PropertyDeclaration**。两者是扫描口径差异，不能
混加或用来伪造精确完成率。本说明以报告的 **276 项非 ID 声明基线**作为审计分母，并
以结构化清单的名称/行号作为字段责任映射来源。

WorldGen 基线没有命中本次 ID 排除规则的字段或属性，因此本文件的排除数为 **0**。
`worldID`、`netID`、`identity`、`whoAmI`、`Uid` 等身份成员若在后续版本出现，应从统计
分母排除，但不能从 ECS 身份、快照、持久化或协议契约中删除。

截至本说明日期，Roslyn 结构化 inventory 的原始 `Status` 仍由生成器维护，不能作为唯一
迁移状态来源；独立 overlay 已对 233 个字段名逐一给出 source/owner/status。已有的
`WorldGenerationStateComponent`、Terrain/Cave/Ore/Tree/Structure/Liquid 系统是目标 owner
和部分行为切片，不等于 276 项已经完成。完整差分仍失败，`canRemoveLegacyWorldGen=false`。

本轮已完成一个可验证切片：`SecretSeed.Variations` 的 22 个布尔派生属性和
`errorWorldAdjustment` 已迁移到 `SecretSeedVariationQuery`，输入为显式启用 seed 集合、
active count 与 skyblock 标志；该切片不读取 `Main`/`WorldGen` 静态状态，仍不代表其余
字段或运行时副作用完成迁移。

## 2. 证据与口径

| 项目 | 证据 |
| --- | --- |
| Version4 行数/大小 | `WorldGen.cs` 73,355 行，1,901,533 bytes |
| Version4 SHA-256 | `A06A8463E39EA065441FF1EF702A787D3450ADAE5CD3065CF30BF76784D3EA1D` |
| 研究报告基线 | `docs/research/2026-08-28-field-property-migration-comparison.md` |
| 结构化清单 | `docs/worldgen/worldgen-source-inventory.json`，233 个 Roslyn 字段；状态同步 overlay 为 `docs/worldgen/worldgen-field-status-overlay-2026-08-29.json` |
| 方法/字段删除门禁 | `docs/worldgen/worldgen-deletion-gate.json`，字段仍未全部映射，门禁关闭 |
| 当前差分状态 | `docs/worldgen/worldgen-parity-report.md`，Tile/扩展状态差异仍存在 |
| 当前生成入口 | `WorldGenerationRequest` -> `WorldGenerationPipeline` |
| 当前世界写入边界 | `WorldGrid` 的 `TileChangeCommitSystem`、液体提交边界、frame 提交边界 |

### 2.1 迁移判定

- `Complete`：字段有唯一 owner、默认值/可变性明确、所有读取和写入经过系统、需要时
  有 snapshot/persistence/protocol 投影，并有 focused verifier。
- `Partial`：已有组件、定义或系统，但只覆盖一部分值域、secret seed、随机顺序、
  side effect 或 legacy 分支。
- `Deferred`：已确认是服务端相关责任，但没有足够行为证据，暂不填充兼容字段。
- `Excluded`：客户端 UI、渲染、音效、旧宿主回调或明确不属于 Simulation 的责任；必须
  写明替代边界，不能用删除来伪装完成。
- `LegacyReference`：只允许作为 Oracle、导入/协议兼容参考，不是运行时 owner。

## 3. ECS 目标架构

```text
WorldGenerationRequest (冻结输入)
  -> WorldSeedComponent + WorldRuleSnapshotComponent + WorldBoundsComponent
  -> WorldGenerationStateComponent / GenerationCursorComponent
  -> Definition / Registry 查询
  -> Terrain/Cave/Biome/Ore/Structure/Tree/Liquid System
  -> TileChangeCommand / LiquidChangeCommand / StructurePlacementCommand / TileFrameCommand
  -> 唯一提交边界
  -> WorldGrid + section versions
  -> immutable WorldGridSnapshot / persistence / protocol projection
```

必须保持以下边界：

1. `WorldGrid` 是稠密 Tile 的权威存储；不得为每个 Tile 创建 ECS entity。
2. 生成系统只能读取不可变 `WorldGridSnapshot` 和冻结 request/component，不能读
   `Main.tile`、`Main.rand`、`Main.maxTilesX` 或旧全局数组。
3. 任何 Tile、Wall、Liquid、frame 变更必须产生 typed command，由提交器按
   `Priority -> Source -> Sequence -> X -> Y -> Kind`（具体比较器以实现为准）稳定提交。
4. secret seed、world size、difficulty、hardmode、spawn、surface/rock layer 和随机流
   必须是显式输入；不得由静态字段隐式获得。
5. 运行时世界状态与一次性生成状态分离。`WorldGen` 的 weather、spawn、meteor、房屋
   等字段若跨 Tick 存活，应迁移到对应 World/Environment/Lifecycle component，而不是
   塞进 `WorldGenerationStateComponent`。

现有可复用 owner：

| 责任 | 现有 owner/边界 | 适用字段族 |
| --- | --- | --- |
| 生成阶段与序列 | `WorldGenerationStateComponent`、`WorldGenerationRuntimeState` | `generatingWorld`、`isGeneratingOrLoadingWorld`、阶段游标、序列 |
| 种子与规则 | `WorldSeedComponent`、`WorldRuleSnapshotComponent`、`WorldGenerationRequest` | secret seed、seed variant、difficulty、hardmode、remix |
| 世界尺寸与高度 | `WorldBoundsComponent`、`TerrainProfileComponent`、`WorldMetadata` | 尺寸常量、surface/rock/ocean 高度、边界 |
| 地形/洞穴/矿物 | `TerrainProfileComponent`、`CaveCarvingComponent`、`OreDefinition`、对应 systems | `lastMaxTiles*`、`tileCounts`、ore tiers、洞穴游标 |
| 生物群系/规则 | `BiomeSurfaceComponent`、definition/query/system | `crimson`、感染、surface secret seed、背景选择 |
| 结构/房屋 | `StructureDefinition`、`StructurePlacementComponent`、housing queries | `TownManager`、room bounds、house flags、结构保护 |
| 树与植被 | `TreeDefinition`、`LegacyTreeProfileDefinition/Registry`、`TreePlacementComponent` | `GrowTreeSettings`、树背景、树冠/树干参数 |
| 液体 | `LiquidDefinition`、`LiquidChangeCommand`、`LiquidPropagationSession` | `extraLiquid`、液体统计、传播预算 |
| Tile/Wall/frame | `WorldTile`、`WorldGridSnapshot`、typed commands、commit systems | `hasTile`、`hasWall`、`roomTiles`、frame/merge scratch |
| 持久化/协议 | Compatibility/WLD projection、immutable snapshots | `spawnMeteor`、`TreeTops` 等有保存格式的状态 |

## 4. 字段迁移清单

下表按旧字段责任族分组。列出的旧名和行号来自结构化 inventory/Version4 源码；同组
字段必须作为一个不变量集合迁移，不能只迁移几个同名 bool。

### 4.1 SecretSeed 与生成规则

**旧 anchor：** `WorldGen.cs:340-424`。目标是不可变规则快照和显式策略查询。

| 旧字段 | 目标 owner | 状态 | 迁移要求 |
| --- | --- | --- | --- |
| `AllSecretSeeds` | `SecretSeedDefinitionRegistry` + `SecretSeedRuleSnapshotSet` | Partial | 35 项注册顺序和只读定义已冻结；code/localization 校验与宿主解锁副作用仍 Deferred |
| `paintEverythingGray`、`paintEverythingNegative`、`coatEverythingEcho`、`coatEverythingIlluminant` | `WorldRuleSnapshotComponent` 的 typed secret-seed flags + definition registry | Partial | 将启用状态冻结到 request；视觉/音频副作用另行 Excluded |
| `noSurface`、`extraLivingTrees`、`extraFloatingIslands`、`errorWorld` | `WorldRuleSnapshotComponent` + Biome/Tree/Structure policies | Partial | 规则值与生成行为分离；不得让 Tree system 读取 secret seed singleton |
| `graveyardBloodmoonStart`、`surfaceIsInSpace`、`rainsForAYear`、`startInHardmode` | World rule/environment policy | Partial | 生成期规则与运行时天气/事件分开，`rainsForAYear` 需要持久化/运行时 owner |
| `biggerAbandonedHouses`、`randomSpawn`、`addTeleporters`、`portalGunInChests` | Structure/spawn/item placement policy | Partial | 只能发出结构、出生点或物品命令；不得直接改 NPC/Main/Chest |
| `noInfection`、`hallowOnTheSurface`、`worldIsInfected`、`surfaceIsMushrooms`、`surfaceIsDesert` | `BiomeSurfaceComponent` + biome definition/policy | Partial | 显式记录互斥/优先级；输出 Tile/Wall command |
| `pooEverywhere`、`noSpiderCaves`、`actuallyNoTraps`、`rainbowStuff`、`digExtraHoles`、`roundLandmasses`、`extraLiquid` | 对应 Tile/Cave/Liquid systems | Partial | 每个分支要有 source anchor、随机流版本和 differential 分类 |
| `worldIsFrozen`、`halloweenGen`、`endlessHalloween`、`endlessChristmas`、`vampirism`、`teamBasedSpawns`、`dualDungeons` | `SecretSeedRuntimeProjection` + `LegacySecretSeedFinalizePolicy` + `LegacyWorldIsFrozen`/`LegacyWorldIsFrozenFinishPolicy` + `HalloweenPumpkinGenerationPolicy` + runtime event policies | Partial | `worldIsFrozen` 的 tile/wall command 与 frozen chest/NPC replacement intents 已按 `LegacyWorldIsFrozen`、`LegacyWorldIsFrozenFinishPolicy` 接管（`WorldGen.cs:2287,2330`）；`halloweenGen || endlessHalloween` 的地表南瓜生成 gate 已按 `WorldGen.cs:20510-20520` 抽为 `HalloweenPumpkinGenerationPolicy`；其余 `vampirism`、`teamBasedSpawns`、`dualDungeons`、`endlessHalloween`、`endlessChristmas` 已进入显式 runtime projection/finalize action（`WorldGen.cs:566-680`）；季节事件副作用、社交规则、南瓜 Tile/结构提交与完整结构接线仍 Deferred；不把这些状态混入 Tile 组件 |
| `Localization`、`_code`、`_sound`、`_plaintext`、`TextThatWasUsedToUnlock` | Compatibility/UI boundary | Excluded/Deferred | Simulation 只保留稳定 rule key；本地化、音频、输入文本留在宿主适配层 |
| `_enabled`、`activeSecretSeedCount`、`anySecretSeedIsActive` | `SecretSeedRuleSnapshotSet` | Partial | enabled 从显式 variant set 派生，active count 按快照计算，`anySecretSeedIsActive` 由 `activeSecretSeedCount > 0` 派生（`WorldGen.cs:422-426`）；宿主解锁流程仍 Deferred |

属性 `SecretSeed.Variations.*`（源码 `44-326`）以及 `GenerateBiggerAbandonedHouses`
(`428`)、`GenerateRainbowGlowsticks` (`444`) 不应变成可写字段。它们应成为接受
`WorldRuleSnapshotComponent` 的纯 policy/query，结果可记录到 generation trace。

### 4.2 Skyblock 与 Tile 扫描状态

**旧 anchor：** `WorldGen.cs:3143-3163`、`3165-3179`。

| 旧字段 | 目标 owner | 状态 | 迁移要求 |
| --- | --- | --- | --- |
| `noAltars`、`noDungeon`、`noTemple`、`noHellstone`、`noFossils`、`noLifeCrystals`、`noHellforge` | `SkyblockRuleSnapshot` | Partial | 由 immutable scan + 显式 dungeon 类型集合构造，不允许系统直接改静态 bool |
| `lowTiles`、`currentActiveTiles` | `TilePresenceScanResult` | Partial | 扫描输入是 immutable snapshot；active count 保持 Version4 的 interior-scan / full-world-area 比率 |
| `hasTile`、`hasWall` | `TilePresenceScanResult` | Partial | 使用 immutable type set；不能暴露可变数组 |
| `denyFloatingIslands`、`denyAllGeneration`、`denySomeGeneration` | `SkyblockPolicyQuery` | Partial | 属性只接受显式 skyblock/rule inputs；不得读取 `skyblockWorldGen` 静态字段 |

`Skyblock.Calculate`/`ScanTiles` 还包含旧的 `Main` 与 `NetMessage` 副作用。迁移时必须
拆成 `Scan -> Calculate -> RuleSnapshot -> optional protocol event`，协议通知不能在
Simulation 生成系统中执行。

### 4.3 SavedOreTiers

**旧 anchor：** `WorldGen.cs:3320-3332`。

| 旧字段 | 目标 owner | 状态 |
| --- | --- | --- |
| `Copper`、`Iron`、`Silver`、`Gold` | `OreDefinition` / `OreDefinitionRegistry` | Partial |
| `Cobalt`、`Mythril`、`Adamantite` | `SavedOreTierDefaults` + `HardmodeOreTierSelectionPolicy` | Partial | 默认 tile ID 与 `SmashAltar` 的 `-1` 初始化、drunk-world alternate toggle 已按 `WorldGen.cs:41849-41945` 抽为 immutable state/policy；altar 生成数量、网络/UI 通知、hardmode guard、持久化和实际 ore placement 仍 Deferred |

这些值是 Tile definition/progression 数据，不是可变 World singleton。`SavedOreTierDefaults`
已冻结 7 个 Version4 默认 tile type，pre-hardmode 随机选择复用该值；Definition 仍需包含
深度、优先级、替代关系和 hardmode 条件；`OrePlacementSystem` 只输出矿脉命令。

### 4.4 GrowTreeSettings 与树相关缓存

**旧 anchor：** `WorldGen.cs:3842-4006`、`4336`。

| 旧字段/属性 | 目标 owner | 状态 | 迁移要求 |
| --- | --- | --- | --- |
| `GemTree_Ruby`、`GemTree_Diamond`、`GemTree_Topaz`、`GemTree_Amethyst`、`GemTree_Sapphire`、`GemTree_Emerald`、`GemTree_Amber` | `LegacyTreeProfileDefinition` + registry | Partial | 把 tile/sapling/高度/ground/wall test 变为纯 definition/query |
| `VanityTree_Sakura`、`VanityTree_Willow`、`Tree_Ash` | `LegacyTreeProfileDefinition` + registry | Partial | 保留 profile key 与默认值；树冠输出走 command |
| `TreeTileType`、`SaplingTileType`、`TreeHeightMin`、`TreeHeightMax`、`TreeTopPaddingNeeded` | `TreeDefinition` / `LegacyTreeProfileDefinition` | Partial | `SaplingTileType` 已由 profile definition 按 `WorldGen.cs:30980,4108` 接管；不保留可变 public struct；构造时校验范围 |
| `GroundTest`、`WallTest`、`IsGroundValid` | `TreeCheckSettingsQuery` + Tree/Tile validation queries | Partial | raw ID 通过显式 definitions 查询；禁止捕获 Main/Tile 可变引用 |
| `TreeTops`、`maxTreeShakes`、`numTreeShakes`、`treeShakeX`、`treeShakeY` | `TreeTopStyleSnapshot/Query` + `TreeShakeWorklistSnapshot/Policy` + WLD/协议 projection | Partial | `TreeTopsInfo` 的固定 13-area style snapshot/query 已按 `TreeTopsInfo.cs:7-42,96-101` 抽出；WLD `Save/Load` 的 13 个 int 与协议 `SyncSend/Receive` 的 13 个 byte 仍由 compatibility/host projection 负责；树抖动 worklist 的固定容量 500、底部坐标去重和 reset 边界已按 `WorldGen.cs:64207-64235` 抽为 immutable snapshot/policy；树冠判定、掉落/Tile mutation、FX/渲染副作用和完整 TreeTops round-trip 仍 Deferred |
| `treeBG1`、`treeBG2`、`treeBG3`、`treeBG4`、`corruptBG`、`jungleBG`、`snowBG`、`hallowBG`、`crimsonBG`、`desertBG`、`oceanBG`、`mushroomBG`、`underworldBG` | `ForestBackgroundSet`/`ForestBackgroundSetQuery` + compatibility background projection | Partial | 已有 `ForestBackgroundSetQuery` 覆盖 bounded style-to-background mapping，并由 `LegacyWorldBackgroundState` 承载协议 byte；渲染 ID、`Main.*BG` 数组更新和完整 `setBG`/WorldData round-trip 仍留在 host/compatibility 边界，不进入 Simulation Tile mutation |

### 4.5 TileMergeCullCache 与 Tile/frame 工作状态

**旧 anchor：** `WorldGen.cs:4018-4034`、`4079`、`4117-4217`、`4328`、`4342`、
`4354-4364`。

| 旧字段族 | 目标 owner | 状态 | 迁移要求 |
| --- | --- | --- | --- |
| `CullTop`、`CullBottom`、`CullLeft`、`CullRight`、`CullTopLeft`、`CullTopRight`、`CullBottomLeft`、`CullBottomRight` | `TileMergeCullMask` + `TileMergeCullingQuery` + `TileMergeCullApplyQuery` | Partial | 八方向 cull 布尔值已按 `WorldGen.cs:86304-86322` 的 null/invisibleBlock 差异抽为 immutable mask，并由 apply query 映射为邻域拒绝值；完整 frame pass 接线、TileFrameCommand 提交和 legacy parity 仍 Deferred，不做全局 cache |
| `tileReframeCount` | `TileFrameBudget` + `TileReframePolicy`/trace counter | Partial | `WorldGen.cs:86263-86279` 的 increment、`< 25` 邻域递归门和 balanced decrement 已抽为 immutable budget decision；预算与诊断计数分离，实际 frame pipeline 接线仍待完成 |
| `tileCounts`、`numTileCount`、`maxTileCount`、`CountedTiles`、`tileCounterNum`、`tileCounterMax`、`tileCounterX`、`tileCounterY` | `TileTypeCountSnapshot` + `TileCountSchedulingState/Policy` + `TileCountColumnRangePolicy` + `TileCountVisitPolicy` + `DirtCountVisitPolicy` + `TileCountVisitedSnapshot/Policy` + `TileCountCapacityPolicy` + bounded scan result + write budget | Partial | `tileCounts` 已通过固定 Version4 registry 长度的 immutable snapshot 建模并复制输入存储；`totalD/totalX` 的每 30 次 world update 扫描一列与列环回已按 `WorldGen.cs:59420-59431` 抽出 immutable 调度状态；`CountTiles` 两段 `40..worldSurface+1` / `worldSurface+1..height-40` 扫描边界及权重已按 `WorldGen.cs:59120-59155` 抽出；`nextCount` 的 interior edge、wall、shimmer-liquid、jungle/lava 访问门控及 typed rejection reason 已按 `WorldGen.cs:8818-8846` 抽出；`nextDirtCount` 的 interior edge、ice tile、protected wall、solid 和 wall `2/59` eligibility 已按 `WorldGen.cs:8908-8928` 抽出；`CountedTiles` 的 immutable visited-point 去重已按 `WorldGen.cs:8828,8884-8885` 抽出；`numTileCount/maxTileCount` 的开放、递增和饱和结果已按 `WorldGen.cs:8818-8824,8884-8885` 抽出；`tileCounterNum` 的四向递归计数、`tileCounterMax=20` 饱和和 `tileCounterX/Y` 去重坐标由同一 visited/capacity owner 覆盖（`WorldGen.cs:10328-10371`）；active tile 分类、run-length 聚合、各终止条件的完整接线和完整 UpdateWorld/CountTiles 仍 Deferred，禁止静态 Dictionary |
| `lastMaxTilesX`、`lastMaxTilesY` | `PreviousWorldBoundsSnapshot` + `WorldBoundsRestartQuery` | Partial | 上一次尺寸已建模为 immutable 快照；`previous.width > current.width || previous.height > current.height` 的缩小判定与 `WorldGen.cs:7330-7343` 一致；源码额外的 `Main.netMode == 1` 客户端清理分支及 tile-array 清空留在宿主/网络 I/O 边界，不作为隐式 Simulation 配置 |
| `WorldSizeSmallX`、`WorldSizeSmallY`、`WorldSizeMediumX`、`WorldSizeMediumY`、`WorldSizeLargeX`、`WorldSizeLargeY` | `WorldBoundsComponent` / world-size definition | Deferred | Version4 中为固定常量声明（`WorldGen.cs:4393-4403`），inventory 无消费者；尺寸输入应通过 `WorldGenerationRequest`/`WorldBoundsComponent` 传递，不复制为未使用的静态配置 |
| `houseTile`、`roomTiles`、`numRoomTiles`、`maxRoomTiles`、`maxRoomTilesForQuery`、`maxRoomSize`、`roomCheckFailureReason`、`roomTorch`、`roomDoor`、`roomChair`、`roomTable` | `HousingRoomStartPolicy` + `HousingBlockingTilePolicy` + `HousingWallSafetyPolicy` + `HousingRoomMinimumSizePolicy` + `HousingTestBounds`、`HousingHomeSpotQuery`、`HousingRoomOccupancyQuery`、`RoomNeedsQuery`、`RoomBoundaryValidationQuery` + Housing/Structure context | Partial | `roomCheckFailureReason` 的所有已观察赋值已映射到现有 immutable reason owner：起始 edge/solid、room-too-small、room-too-big、unsafe-wall、oversized-wall-hole（`WorldGen.cs:6206-6283,6306,6371-6376`）；`CheckRoom` 起始点的 edge/solid failure gate 已按 `WorldGen.cs:5722-5734` 抽为 immutable reason；solid tile 与开放门（tile 11/389/386）的 `BlockingWall`/`BlockingOpenGate` 判定已按 `WorldGen.cs:5828-5850` 抽出；wall safety 的 `TooManyUnsafeWalls`/`HoleInWallIsTooBig` 判定已按 `WorldGen.cs:5876-5894` 抽出；`numRoomTiles < 60` 的 `RoomIsTooSmall` spawn threshold 已按 `WorldGen.cs:5760` 抽出；`RoomNeeds` 的 chair/table/door/torch 四项 AND gate 已按 `WorldGen.cs:5314-5350` 由 immutable `RoomNeedsResult` 表达；footprint bounds、home-spot eligibility、occupancy 以及递归节点的 edge/out-of-bounds/tile-limit/size-limit 判定已有 immutable query；`maxRoomTilesForQuery=8100` 仅有声明（`WorldGen.cs:4327`），当前源码无读取点，保持未消费的 legacy boundary；房屋递归扫描、评分/候选竞争、家具实际提交、NPC spawn 与完整 legacy parity 仍 Deferred |
| `mergeUp`、`mergeDown`、`mergeLeft`、`mergeRight`、`stopDrops`、`destroyObject` | `TileMergeQuery` + `TileMergeCullingQuery`/`TileMergeCullApplyQuery` + `WorldDropPolicyQuery` + `ObjectDestructionGuardPolicy` | Partial | `merge*` 的四向反向映射和 frametest frame 请求已由 `TileMergeQuery.ApplyFrametest` 承载（`WorldGen.cs:67602-67687`）；八方向 null/invisible cull 已由 `TileMergeCullMask` 与 apply query 承载（`WorldGen.cs:86304-86322`）；`stopDrops` 在 `KillTile` 的 item/effect drop 分支已按 `WorldGen.cs:52596-52604,52884-52890` 抽为显式 gate；`destroyObject` 的 `Check1xX/Check2xX/CheckXmasTree` 入口拒绝嵌套检查、成功路径成对置位/复位，已按 `WorldGen.cs:31308-31311,31363-31386,31508-31511,31653-31809,32726-32779` 抽为 immutable reentrancy decision；完整 frame pass 接线、Tile/drop/network 副作用与 legacy parity 仍 Deferred，禁止跨生成复用 static scratch |
| `maxWallOut2` | `WallSpreadBudgetPolicy` | Partial | `Wall2` 的默认输出上限 `5000`、递增和 exhausted gate 已按 `WorldGen.cs:3448-3453,3490-3498,4199-4201` 抽为 immutable budget；队列去重、wall replacement predicates、Tile 写入、网络通知和完整 spread 接线仍 Deferred |
| `lavaCount`、`iceCount`、`sandCount`、`rockCount`、`shroomCount`、`trapDiag`、`gem`、`mossType`、`neonMossType`、`tileSolidBackup`、`_coatingColors`、`bitStrip` | `TileCountEnvironmentCounters` + `GemTileRandomPolicy` + `MossSelectionPolicy` + algorithm-local value state | Partial | `countTiles/nextCount` 的 active tile 分类增量（70/1/147/161/53/396/397）和 lava-liquid 增量已按 `WorldGen.cs:8853-8860,8867-8879` 抽为 immutable counters；`trapDiag` 仅在 trap 分支累加且当前无读取/持久化消费者，保持 diagnostic-only；`gem` 的六类启用筛选、1/20 宝石 tile 抽取和 tile ID 映射已由 `GemTileRandomPolicy` 按 `WorldGen.cs:9009-9030` 接管；`mossType` 三个互异索引与 `neonMossType` 四选一随机已由 `MossSelectionPolicy` 按 `WorldGen.cs:9028-9045` 接管；`tileSolidBackup` 仅在 `WorldGen.cs:11434,22980-22982` clone/restore `Main.tileSolid`，保持 host mutable boundary；`bitStrip` 仅为 `RefreshStrip` 的 `200x50` frame-refresh scratch，需先改为 per-call workspace；重试循环、分类结果消费及其余 algorithm-local 状态仍 Deferred，不升级为 singleton component |
| `ExploitDestroyQueue` | `ExploitDestroyQueueEntry` + `ExploitDestroyQueuePolicy` + `ExploitDestroyQueueDispatchPolicy` + `ExploitDestroyQueueFrameSystem` + `TerrariaPacketCodec.EncodeExploitDestroyTileSquare` | Partial | sandfall 队列声明与消费入口分别位于 `WorldGen.cs:4342`、`52940-52955`；typed entry 提供容量、序列、来源 discriminator 和坐标去重，dispatch policy 按 legacy active-tile gate 产出 frame request 与 `RemoteClient=-1`、`1x1` tile-square intent，并在 `destroyObject=false` 时显式 drain 队列、在 nested guard 时保留队列；frame system 已提交 typed frame command，V1456 codec 已将 broadcast intent 编码为 `TileSquare` payload 并拒绝非 broadcast recipient；真实 host send、冲突优先级和 legacy parity 仍 Deferred |
| `_preventInfiniteRopeFraming`、`BUBBLES_SOLID_STATE_FOR_HOUSING` | `TileHousingRuleSnapshot` / `TileHousingRuleQuery` | Partial | 常量进入命名 definition；运行时开关进入明确规则快照 |

### 4.6 World runtime、事件与生成生命周期

**旧 anchor：** `WorldGen.cs:4075-4372`。

| 旧字段族 | 目标 owner | 状态 | 迁移要求 |
| --- | --- | --- | --- |
| `TownManager`、`roomX1`、`roomX2`、`roomY1`、`roomY2`、`bestX`、`bestY`、`hiScore`、`canSpawn`、`roomTorch`、`roomDoor`、`roomChair`、`roomTable`、`roomHasStinkbug`、`roomHasEchoStinkbug`、`LastFoundHouse`、`currentlyTryingToUseAlternateHousingSpot`、`sharedRoomX`、`_roomCheckStack`、`roomCheckFailureReason` | `HousingScoreEligibilityPolicy` + `HousingRoomQualityPolicy` + `HousingStinkbugSpawnPolicy` + `HousingAlternateSpotPolicy` + Housing/Structure ECS context、candidate/result snapshot | Partial | `hiScore > 0` 的最终 spawn gate 已按 `WorldGen.cs:5080` 抽为 immutable decision；`roomOccupied -> roomEvil -> !roomHasStandingSpace -> default` 的失败原因优先级已按 `WorldGen.cs:4888-4910` 抽为 immutable decision；`roomHasStinkbug/roomHasEchoStinkbug` 的任一 true 且目标非 town-pet room 时的 spawn block 已按同一 source anchor 抽为 immutable gate；`currentlyTryingToUseAlternateHousingSpot` 的 `TownManager.HasRoom && !guard` 递归入口已按 `WorldGen.cs:5569-5575` 抽为 immutable decision；`LastFoundHouse` 仍是 `SpawnTownNPC` 成功后的宿主坐标缓存（`WorldGen.cs:5159,5713`），`sharedRoomX` 仍依赖 `ScoreRoom` 的 NPC home tile（`WorldGen.cs:5937,6067`），ScoreRoom 计算、占用查询完整接线、NPC spawn 实体生命周期和其余 context 仍 Deferred |
| `Manifest` | Compatibility/WLD manifest projection | Excluded/Deferred | `WorldGen.Manifest` 在 reset 时写入 `Version`/`GitSHA`（`WorldGen.cs:10152-10153`），WLD 仅负责 `Serialize/Deserialize`（`WorldFile.cs:1465,2567`）；当前生成管线没有 Simulation 读取消费者，保留在 host/compatibility 边界，不复制旧可变 `WorldManifest` 引用；若未来需要 stage hash，应以 immutable manifest snapshot 和显式 projection 接入 |
| `statusText` | Host generation-progress/UI adapter | Excluded | `WorldGen.cs:7011,7167,7335,7353` 仅向 `Main.statusText` 写入本地化进度文本；不进入 Simulation snapshot、Tile command 或世界持久化 |
| `oceanDistance`、`beachDistance`、`shimmerSafetyDistance`、`cactusWaterWidth`、`cactusWaterHeight`、`cactusWaterLimit`、`InfectionAndGrassSpreadOuterWorldBuffer` | `WorldGenerationRequest.DistanceDefaults` + `WorldGenerationTrace` + boundary/cactus/shimmer queries | Partial | Version4 常量已冻结为 request 输入，并写入 trace provenance；ocean/beach 边界、cactus 水量累计/remix 例外与 shimmer 严格半径查询已接入；感染缓冲在 Version4 只有声明、无消费者引用，故不臆造策略；完整生成管线、行为对照和旧字段删除仍 Deferred |
| `crimson`、`generatingRandomEvil`、`WorldGenParam_Evil`、`totalEvil`、`totalBlood`、`totalGood`、`totalSolid`、`totalEvil2`、`totalBlood2`、`totalGood2`、`totalSolid2`、`tEvil`、`tBlood`、`tGood`、`totalX`、`totalD` | `WorldGenerationRequest.WorldGenParamEvil` + `WorldEvilSelectionPolicy` + `WorldInfectionAlignmentScanQuery/Policy` + `WorldInfectionAlignmentSnapshot/AccumulatorPolicy` | Partial | `WorldGenParam_Evil` 已成为 request 的显式 `-1/0/1` 输入，随机/强制分支已抽为 immutable policy；scan query 现在按 `WorldGen.cs:59085-59169` 的 40-tile outer buffer、surface 权重 5、cavern 权重 1 扫描单列，并按 `WorldGen.cs:59176-59214` 的基类与分类集合派生 solid denominator，同时保留 remix world 的 tile `474` evil / tile `195` crimson 分支；scan policy 已接入 legacy 30-update cadence、跨列 accumulator 和 column-0 publish-before-reset；分类注册、网络发布、stage 累计和完整感染转换管线仍 Deferred |
| `_transformingWorld`、`TransformingWorld` | `WorldTransformationStateSnapshot` / `WorldTransformationStateQuery` | Partial | 属性改为 snapshot query；转换操作发 command，禁止静态计数器 |
| `spawnEye`、`spawnHardBoss`、`shadowOrbSmashed`、`shadowOrbCount`、`altarCount`、`meteorShowerCount` | `ShadowOrbBreakProgression` + `AltarBreakProgression` + `MeteorShowerProgression` policies + World event/progression component | Partial | shadow orb 击碎标记、计数递增与第三颗/特殊世界 boss spawn 尝试条件，以及 altar 计数递增和 `% 3`/`/ 3` 周期算术已抽为 immutable policy；`meteorShowerCount` 已按 legacy `Next(650, 751) * 4` 初始化、reset/快进清零、成功 impact 后递减抽为独立 immutable policy；不与 `WorldProgressionState.IsMeteorScheduled` 合并。NPC 选择、事件副作用、持久化和其余事件字段仍 Deferred |
| `isGeneratingOrLoadingWorld`、`generatingWorld`、`generatingWorldOnThisThread` | `WorldGenerationLifecycleSnapshot` + `WorldGenerationTrace` + host lifecycle adapter | Partial | `generatingWorld` 与 `isGeneratingOrLoadingWorld` 已进入显式 snapshot/query；pipeline trace 现在同时记录生成开始 `(true,true)` 与完成 `(false,false)` 两个边界，线程标志留在宿主，不进入可复制世界状态；完整 host lifecycle、异常终止和旧字段删除仍 Deferred |
| `builtHouseWithNoFurniture`、`builtHouseWithNoLight`、`AllowedToSpreadInfections`、`hardModeWorldUpdates`、`growGrassUnderground`、`_isRainingBoulders`、`fossilBreak` | `WorldUpdatePolicySnapshot` + `RainingBoulderStatePolicy` + `FossilBreakPolicy` + housing/biome/runtime policies | Partial | `builtHouseWithNoFurniture` 与 `builtHouseWithNoLight` 在当前 Version4 仅有声明和 `Reset` 清零（`WorldGen.cs:4159-4161,6483-6484`），无读取消费者，保持未消费 Deferred；`AllowedToSpreadInfections`、`hardModeWorldUpdates`、`growGrassUnderground` 已按 `WorldGen.cs:59408-59512` 抽为 immutable phase policy；`fossilBreak` 已按 `WorldGen.cs:52533-52558` 抽出一次性 reentrancy gate 与邻域 roll 上限；`_isRainingBoulders` 已按 `WorldGen.cs:59590-59607` 抽出 drunk/good/non-remix/storm eligibility 与结束 transition；房屋结果、raining boulder 随机落点/成就副作用、fossil tile kill、完整 `UpdateWorld` 调度/Tile 提交仍 Deferred，不使用杂项 singleton |
| `spawnMeteor`、`worldCleared`、`worldBackup`、`loadFailed` | Compatibility/persistence + `WorldLoadRecoveryPolicy` + world lifecycle result | Partial | `spawnMeteor` 的持久化读写已由 `WorldFile.cs:1339,2156,3670` 映射到 `LegacyWorldMetadata.IsMeteorScheduled -> CompatibilityWorldMetadata -> WorldProgressionState.IsMeteorScheduled`，但 `WorldGen.cs` 内当前仅在 reset 清零、无生成期读取；`loadFailed`/`worldBackup` 在 `WorldGen.cs:6341-6367` 参与 primary retry、backup existence、restore/retry/final failure 决策，已抽为 immutable `WorldLoadRecoveryPolicy`；`worldCleared` 仅在 `WorldGen.cs:6667` 置位且无当前读取；文件复制/删除、日志、tile 初始化、persistence 和完整 host lifecycle 仍保留在适配层，均不得作为 Tile 生成输入或与 meteor schedule 合并 |
| `npcSpawnDelay`、`npcSpawnPeriod`、`prioritizedTownNPCType` | `TownNpcSpawnCadencePolicy` + `TownNpcSpawnSelectorQuery` + `SpecialTownNpcSpawningQuery` + NPC spawn policy snapshot | Partial | `npcSpawnPeriod=20*worldUpdateRate`、无 invasion/eclipse 时 delay 递增、达到 period 清零的 cadence 已按 `WorldGen.cs:72739,76169-76171` 抽为 immutable decision；`prioritizedTownNPCType` 的候选选择、已存在/房间/特殊条件过滤已由 `TownNpcSpawnSelectorQuery` 与 `SpecialTownNpcSpawningQuery` 覆盖；墙体随机、房屋查找、NPC 实体生命周期、网络通知和完整 `SpawnTownNPC` 接线仍 Deferred；WorldGen 只应产出 spawn intent |
| `grassSpread`、`SmallConsecutivesFound`、`SmallConsecutivesEliminated` | `GrassSpreadRecursionState/Policy` + `SmallConsecutiveClumpMetricsPolicy` + pass-local metrics/checkpoint | Partial | `grassSpread` 已按 `WorldGen.cs:62808-62812` 抽出 `repeat && depth < 1000` 递归保护与 balanced unwind；`SmallConsecutives*` 已按 `WorldGen.cs:10498-10504` 的 `0 < clumpLength < tileCounterMax` 与 `tileCounter < tileCounterMax` 条件抽为 immutable 累计结果；grass conversion、Tile mutation 和完整 UpdateWorld 接线仍 Deferred |
| `remixWorldGen`、`everythingWorldGen`、`noTrapsWorldGen`、`drunkWorldGen`、`getGoodWorldGen`、`tenthAnniversaryWorldGen`、`dontStarveWorldGen`、`notTheBees`、`skyblockWorldGen`、`drunkWorldGenText`、`placingTraps` | `WorldRuleSnapshotComponent` typed variants + `TrapGenerationGatePolicy` | Partial | 规则在 request 创建时归一化；`placingTraps` 已按 `WorldGen.cs:17785-17788` 抽出 skyblock/actually-no-traps/not-the-bees pass gate；文本字段与执行开关分离，陷阱数量、随机坐标、TNT/Tile placement 仍 Deferred |
| `_generator` | `WorldGenerationPipeline`/stage registry | Excluded | 旧 generator 对象不是状态 owner；保留接口由 pipeline 组合 systems |
| `mysticLogsEvent`、`BackgroundsCache`、`_SpawnThunderStorm_SafeSpots` | event/render/host compatibility | Excluded/Deferred | 音频、UI、渲染缓存和宿主事件不得进入 Simulation；安全点若影响生成则改为 immutable result |
| `_coatingColors`、`catTailDistance`、`heartPos`、`heartCount`、`strip_w`、`strip_h` | `CatTailDistancePolicy` + `CrimsonHeartPositionSnapshot/Policy` + algorithm-local value/definition | Partial | `catTailDistance=8` 的 `PlaceCatTail` 间距 `2..7` 与 `CheckCatTail` 超过 `8` 的清理边界已按 `WorldGen.cs:59639-59643,59755-59759` 抽为 immutable distance decision；`heartPos/heartCount` 已按 `WorldGen.cs:77199-77201` 的猩红心位置 append 和固定容量 100 抽为 immutable snapshot/policy，并对应 `CrimPlaceHearts` 的地形与 shadow-orb 消费（`WorldGen.cs:77034-77075`）；`_coatingColors` 仍为宿主 Color 列表缓存，实际 Tile/结构提交、完整猩红生成管线和其余 algorithm-local 状态仍 Deferred |
| `ItemSpawnProtectionTime` | `ItemSpawnProtectionPolicy` | Partial | Version4 duration 已冻结；通过 item/drop command 或 policy 传递，不读旧 Item/WorldItem |
| `genRand` | `GenerationRandomState` / `WorldGenerationRandomSnapshot` | Partial | `(seed, streamVersion, stage, cursor)` 定位；禁止 `Main.rand` |
| `oceanLevel` | `TerrainProfileComponent`/`OceanLevelQuery` + `WorldGenerationTrace.TerrainProfile` | Partial | 接受 surface/rock profile 和规则输入，作为纯派生查询；公式与实际生成 profile provenance 已覆盖（`WorldGenerationPipeline.cs:19-25,355-377`、`Program.cs:828-844`，产物 `Build/diagnostics/server-ecs-convergence/P9-worldgen/current-stage-trace/stage-fingerprints.json`）；完整 ocean-level 消费者和持久化/行为对照仍待完成 |

## 5. 属性迁移清单

属性分为三类：纯派生查询、规则 policy、兼容/宿主边界。所有属性都应移除对旧静态
字段的隐式读取。

| 旧属性 | 源码行 | 目标形态 | 状态 |
| --- | ---: | --- | --- |
| `paintEverythingGrayJustTheSurface`、`paintEverythingGrayJustTreasure`、`paintEverythingGrayUseWhite` | 44, 60, 72 | `SecretSeedVariationQuery` | Partial |
| `paintEverythingNegativeJustUnderground`、`paintEverythingNegativeJustSomeThings` | 84, 100 | `SecretSeedVariationQuery` | Partial |
| `coatEverythingJustInnerBlocks`、`coatEverythingEchoJustSomeThings`、`coatEverythingIlluminantJustRandomSpots`、`coatEverythingIlluminantJustSomeThings` | 112-152 | `SecretSeedVariationQuery` | Partial |
| `noSurfaceNoFloatingIslands`、`noSurfaceNoLivingTrees`、`noSurfaceNoPyramids`、`noSurfaceNoSwordShrines` | 168-204 | `SecretSeedVariationQuery` | Partial |
| `extraLivingTreesReducedAmount`、`extraFloatingIslandsNormalAmount`、`extraFloatingIslandsReducedAmount` | 216-244 | `SecretSeedVariationQuery` | Partial |
| `errorWorldBalancedChests`、`noSpiderCavesActuallyNoSpiderCaves`、`noSpiderCavesILiedMoreSpiderCaves`、`actuallyNoTrapsForRealIMeanIt` | 256-292 | `SecretSeedVariationQuery` | Partial |
| `surfaceIsDesertNormalFunction`、`surfaceIsDesertSwapDesertAndSnowBiomes` | 304-316 | `SecretSeedVariationQuery` | Partial |
| `SecretSeed.Enabled` | 426 | immutable `SecretSeedRule.Enabled` | Partial |
| `GenerateBiggerAbandonedHouses`、`GenerateRainbowGlowsticks` | 428, 444 | `SecretSeedGenerationPolicyQuery` | Partial |
| `Skyblock.denyFloatingIslands`、`Skyblock.denyAllGeneration`、`Skyblock.denySomeGeneration`、`Skyblock.spawnSolidifier`、`Skyblock.spawnShimmerPool` | 3165-3199 | `SkyblockPolicyQuery` | Partial |
| `TransformingWorld` | 4368 | `WorldTransformationStateSnapshot.IsTransforming` | Partial |
| `genRand` | 4370 | `GenerationRandomState` passed by value/ref to system | Partial |
| `oceanLevel` | 4372 | `OceanLevelQuery(TerrainProfileComponent)` | Partial |

### 5.1 本轮切片验收

`SecretSeedVariationQuery.Evaluate` 已覆盖 Version4 的 22 个布尔派生结果，以及
`errorWorldAdjustment` 的 inactive 默认值和 active-count 缩放。验证命令为
`dotnet run --project Test/Terraria.Dome.WorldGeneration.Verification/Terraria.Dome.WorldGeneration.Verification.csproj -p:UseSharedCompilation=false --no-restore -- --reduced`，退出码为 0；该 reduced gate 完成 32/81 个测试节。完整 parity 与删除门禁仍未通过。

Skyblock 的 `ScanTiles`/`Calculate` 已拆为 `TilePresenceScanQuery`、
`SkyblockRuleQuery` 与 `SkyblockPolicyQuery`。扫描保持源码的 40-tile 内缩边界，
`lowTiles` 保持“内缩扫描活跃数 / 全世界面积”的原始分母。`Main.tileDungeon`、
`Main.wallDungeon`、`Main.skyblockWorld` 和 `NetMessage` 都不是 Simulation 依赖：dungeon
类型集合与 skyblock 标志由调用方显式提供。协议通知、dungeon 坐标复位和运行时调度仍为
Deferred。

Roslyn 扫描的 31 个属性节点中，部分 expression-bodied/嵌套属性不会被研究报告的
“属性块”正则稳定识别；因此属性表是语义清单，不用于重算报告的 276 项分母。

### 5.2 本轮感染 alignment 扫描切片

`WorldInfectionAlignmentScanQuery.ScanColumn` 按 `WorldGen.cs:59085-59169` 保留
40-tile outer buffer、surface 权重 5、cavern 权重 1，并按 `59095-59214` 的分类
和六个基础 tile ID 计算 solid denominator；remix world 的 tile `474` evil 与 tile
`195` crimson 分支也由显式输入保留。`WorldInfectionAlignmentScanPolicy` 连接
legacy 30-update cadence、跨列 accumulator 与 column-0 publish-before-reset。
本批 fresh reduced verifier 退出码为 0，evidence 为
`Build/diagnostics/worldgen-infection-alignment-pipeline-reduced-20260830.log`；
focused assertions 覆盖 weighted、remix、cadence、accumulation 和 publish reset。
完整感染转换、网络发布、全量 differential 与 legacy 删除门禁仍为 Deferred/blocked，
本切片不改变 `canRemoveLegacyWorldGen=false`。

### 5.3 本轮感染转换 typed command 切片

`LegacyWorldInfectionConversionCommandEmitter.AppendCommands` 按
`WorldGen.cs:2121-2173` 复刻 `DoWorldIsInfected` 的控制边界：skyblock 直接跳过，
每个 column 只消费一次 `genRand.Next(3)`，起点和 `drunkWorld`/`crimson` 的
conversion type 由显式 `LegacyWorldInfectionConversionInput` 冻结。扫描读取
`WorldGridSnapshot`，对空 Tile 早退，并把有效的 tile/wall 通道写成带 sequence 和
source 的 `LegacyWorldInfectionConversionCommand`；不会直接修改 Tile、读取 `Main`，
或发送 `NetMessage`。

这个批次只建立 conversion intent/command 边界，不把 `Convert` 的 22 个分支误报为
已迁移。`TileID.Sets`/`WallID.Sets` 的完整映射、thorn kill、torch/frame 副作用、
command 到 deterministic commit 的接线、网络通知和 full WorldGen differential 仍为
Deferred。focused verifier 已覆盖普通与保护通道、drunk 左右列、no-infection/no-surface
起点、skyblock gate、sequence 和不可变输入；本批 evidence 为
`Build/diagnostics/worldgen-infection-conversion-reduced-20260830.log`。

### 5.4 本轮 infection conversion registry 与 deterministic commit adapter

在上一批 intent 边界之上，`LegacyWorldInfectionConversionRegistry` 已冻结
Version4 `WorldGen.cs:47843-48989` 中 conversion type `1` 与 `4` 的常规 tile/wall
类别集合、目标 ID 和 `else-if` 优先级。tile registry 明确保留
`MossOrStone -> JungleGrass -> Grass -> Ice -> Sand -> HardenedSand -> Sandstone`
顺序；torch 与 thorn 规则仍记录为 Deferred。wall registry 覆盖 Grass、Stone、
HardenedSand、Sandstone 和 NewWall1..4；type `2` 及其 golf/thorn side effect 没有
被偷偷扩大到本批契约。

`LegacyWorldInfectionConversionCommitBoundary.TryCommit` 接受不可变源快照和 typed
conversion intents，先校验尺寸、坐标、序列唯一性、conversion type、section version
以及源 tile 一致性，再将支持的替换投影成 `TileChangeCommand`，唯一通过
`TileChangeCommitSystem` 提交。tile/wall 的 liquid、frame、wire、paint、coating、
active/inactive 等扩展状态由 projection 保留；已经是目标类型的值计为 no-op。
提交失败恢复 `WorldGenerationStateComponent` 的 sequence，不会直接写 `WorldGrid`。

torch frame、thorn kill、inactive/empty channel、未注册 tile/wall 类别均返回结构化
`LegacyWorldInfectionConversionDeferred` 和 reason，不会静默丢弃。focused verifier
覆盖 34 条 immutable rules、所有常规类别和目标保护、逆序输入确定性、14 个 tile 与
16 个 wall command 的提交、扩展状态保留、6 类 Deferred、stale snapshot rejection
以及 type-2 fail-closed。fresh reduced evidence 为
`Build/diagnostics/worldgen-infection-conversion-registry-reduced-20260830.log`；
Simulation/verifier serial builds 均 exit 0、0 warnings、0 errors；overlay/JSON 与
冻结 deletion gate 仍通过且 `canRemoveLegacyWorldGen=false`。

这仍然只是局部 registry/commit slice：完整 22 分支 `Convert`、完整 TileID/WallID
registry、torch/frame/thorn side effect、runtime stage/pipeline 接线、network
publication、full WorldGen differential 和 legacy `WorldGen.cs` 删除继续保持
Deferred，迁移状态仍为 `Partial / In Progress`。

### 5.5 本轮 conversion type-2 registry 扩展

根据 Version4 `WorldGen.cs:47936-48017`，registry 现在也冻结 conversion type `2`
的 17 条规则：tile 侧为 Torch（Deferred）、Moss/Stone、GolfGrass、Grass、Ice、
Sand、HardenedSand、Sandstone、Thorn（Deferred），wall 侧为 Grass、Stone、
HardenedSand、Sandstone 和 NewWall1..4。`GolfGrass` 保持在普通 Grass 之前的
`else-if` 优先级，477 明确转换到 492；type-2 的 wall targets 为
70/28/219/222/200/201/202/203，regular tile targets 为
117/492/109/164/116/402/403。

`LegacyWorldInfectionConversionCommand.IsValid` 现在显式允许 1/2/3/4，仍拒绝其余
未建模 conversion types。commit adapter 对 type-2 只提交上述 regular tile/wall
替换；Torch frame mutation、Thorn kill/network publication 继续返回 Deferred。源码
中 type-59 只有在四邻域含 Grass tile 109 时才触发清理；该上下文需要进一步的
side-effect contract，因此当前报告为 `AdjacentGrassCleanup` Deferred，无邻接
条件则保持 no-op。

focused verifier 已覆盖全部 type-2 regular rules、GolfGrass 优先级、self-target
no-op、通道/扩展状态保留、type-2 torch/thorn/contextual cleanup Deferred，以及
unsupported type fail-closed。fresh evidence 为
`Build/diagnostics/worldgen-infection-conversion-type2-reduced-20260830.log`；
Simulation/verifier 串行 build 均 exit 0、0 warnings、0 errors。overlay/JSON 检查和
冻结 deletion baseline 仍通过；`canRemoveLegacyWorldGen=false`。

本扩展不等于完整 conversion parity：conversion types 0、5-21、完整 registry、
torch/frame/thorn 的实际副作用、runtime 接线、network publication、全量 differential
以及 legacy `WorldGen.cs` 删除仍 Deferred，迁移文档保持 `Partial / In Progress`。

### 5.6 本轮 conversion type-3 registry 扩展

根据 Version4 `WorldGen.cs:48094-48116`，type-3 的 wall 分支先于 tile 分支执行：
当显式 `walls` 通道打开且 wall 属于 `WallID.Sets.CanBeConvertedToGlowingMushroom`
时，目标 wall 为 `80`；源码集合由 `WallID.cs:40` 冻结为 `15/64/67/247`。当
`tiles` 通道关闭时，源码在 wall 处理后直接退出，不会触发 tile 分支。tile 分支的
常规映射只接受精确的 tile `60`，目标为 `70`；type-3 的 Torch（目标 frame style
`22`）以及 Thorn 的 `KillTile`/`NetMessage.SendData` 属于尚未建模的副作用。
需要注意，`DoWorldIsInfected` 外层在 `WorldGen.cs:2170` 将 tile `60` 排除在
`tiles` 通道之外，因此本批是对 `Convert` type-3 分支的直接 registry/commit 建模，
并没有声称现有 emitter 已把该分支接入运行时。

`LegacyWorldInfectionConversionRegistry` 新增 immutable `MushroomGrass`（source
`60` -> `70`）和 `GlowingMushroomWall`（source `15/64/67/247` -> `80`）规则，
并把 type-3 Torch/Thorn 保留为结构化 Deferred。`LegacyWorldInfectionConversionCommand`
的输入契约显式允许 conversion type `3`；commit adapter 只把常规 tile/wall 替换投影为
`TileChangeCommand`，通过 `TileChangeCommitSystem` 确定性提交，wall-only/tile-only
flags 和 self-target no-op 均保持隔离，liquid/frame/wire/paint/coating 等非所属状态
继续由投影保留。

focused verifier 直接校验两个 source set、规则 priority 和目标 ID，并覆盖四个 wall
source -> `80`、tile `60` -> `70`、tile/wall self-target no-op、逆序 intent 的
deterministic commit、扩展状态保留，以及 Torch/Thorn 各一个 Deferred。该批次实际
提交 `1` 个 tile 与 `4` 个 wall，记录 `2` 个 no-op、`2` 个 Deferred，
`WorldGenerationStateComponent` sequence 从 `9000` 前进到 `9005`；fresh evidence
为 `Build/diagnostics/worldgen-infection-conversion-type3-reduced-20260830.log`。
Simulation 与 WorldGeneration verifier 串行 build 均 exit 0、0 warnings、0 errors；
overlay/JSON 检查和冻结 deletion gate 仍通过且 `canRemoveLegacyWorldGen=false`。

当前 adapter 会在 commit 时从 `WorldGenerationStateComponent` 重新分配输出
`TileChangeCommand` sequence；intent sequence 只负责输入排序。一个 intent 同时拥有
tile/wall 两个可变通道时会产生两个 child command，因此 intent/commit 两个序列域、
双通道的全局排序以及与其他 generation systems 的 sequence 协调仍需后续显式 contract
复核；本批保留现有行为，不把 focused sequence 结果误报为该设计风险已解决。

这仍然只是局部 type-3 registry/commit slice：完整 conversion types `0`、`5-21`、
complete TileID/WallID registry、torch/frame/thorn 实际副作用、runtime stage/pipeline
接线、network publication、full WorldGen differential 和 legacy `WorldGen.cs` 删除
继续 Deferred，迁移状态仍为 `Partial / In Progress`。

### 5.7 本轮 conversion type-8/9/10 registry 扩展

根据冻结的 Version4 `WorldGen.cs:48407-48503`（source SHA-256
`A06A8463E39EA065441FF1EF702A787D3450ADAE5CD3065CF30BF76784D3EA1D`），type-8/9/10
是 `Convert` 的 tile-only switch 分支。type-8 将 tile `59/60` 转为 `211`；type-9
按源码顺序将 `2/23/109/199/477/492/661/662 -> 60`、`0 -> 59`、`25/203 -> 1`、
`112/234 -> 53`、`398/399 -> 397`、`400/401 -> 396`，并对
`24/32/201/205/352/636` 执行尚未建模的 `KillTile`/网络通知；type-10 使用较窄的
草类集合 `23/199/661/662 -> 60`，其余五组常规目标与 type-9 相同，KillTile 集合
相同。type-9 的广义草类规则与 type-10 的次级草类规则在 registry 中分开，避免把
分支条件错误合并。

`LegacyWorldInfectionConversionRegistry` 现在通过 14 条 immutable tile rules 冻结上述
集合与 priority；`LegacyWorldInfectionConversionCommand.IsValid` 显式允许
conversion type `8/9/10`，仍拒绝 `0`、`5-7`、`11-21` 等尚未建模类型。常规 tile
替换只投影为 `TileChangeCommand(UpdateTileType)`，经过既有
`TileChangeCommitSystem` 提交并保留 liquid/frame/wire/paint/coating 等非所属状态。
type-9/10 的 KillTile/`NetMessage.SendData` 返回结构化
`ChlorophyteKillMutation` Deferred；这三个源码分支没有 wall 处理，若 typed intent
错误地打开 walls 通道则返回 `UnsupportedChannel` Deferred，不伪造墙体转换。

同时调整 commit adapter 的目标解析顺序：先解析 source rule，再处理 `IsDeferred` 或
regular self-target，最后才使用 target lookup 判定未带 source set 的目标 no-op。这样
不会让 deferred target `0` 把 type-9 的常规 `0 -> 59` 规则错误计为 no-op。

focused verifier 直接检查 type-8/9/10 的 12 条常规规则、精确 source sets、type-9/10
priority、type-10 对 type-9 广义草类的排除、self-target no-op、tile-only channel、
扩展状态保留、逆序/正序 deterministic commit，以及两个 KillTile 和一个 wall-channel
Deferred。fresh full verifier evidence 为
`Build/diagnostics/worldgen-infection-conversion-type8-10-20260830.log`；Simulation 与
WorldGeneration verifier serial build 均 exit 0、0 warnings、0 errors，overlay/JSON
检查和冻结 deletion gate 也保持通过。

本批仍是 direct registry/commit slice：`hardUpdateWorld`/`ChlorophyteDefense` runtime
接线、其随机流和递归调用顺序、KillTile/network 实际副作用、完整 conversion matrix、
full WorldGen differential 以及 legacy `WorldGen.cs` 删除继续 Deferred，
`canRemoveLegacyWorldGen=false`，迁移状态仍为 `Partial / In Progress`。

### 5.8 本轮 conversion type-5/6 registry 扩展

根据冻结的 Version4 `WorldGen.cs:48117-48193`（source SHA-256
`A06A8463E39EA065441FF1EF702A787D3450ADAE5CD3065CF30BF76784D3EA1D`），type-5/6 的
wall 分支先于 tile 分支，并且各自保持源码的 `else-if` 优先级。type-5 的常规 tile
目标为 `53`、`397`、`396`：Grass/Sand/Snow/Dirt 默认转为 `53`，当不可变
`WorldGridSnapshot` 的下方 tile 构成 solid support 时转为 `397`；HardenedSand 转为
`397`，Moss/Stone/Ice/Sandstone 转为 `396`。type-6 将 Grass/Sand/HardenedSand/
Snow/Dirt 转为 `147`，Moss/Stone/Ice/Sandstone 转为 `161`。已经是目标类型的 tile
或 wall 仍按源码条件保持 no-op。

两个分支的 wall 规则相同地按 source family 合并：Stone/NewWall1/NewWall2/NewWall3/
NewWall4/Ice/Sandstone 分别在 type-5/type-6 转为 `187`/`71`，HardenedSand/Dirt/
Snow 分别转为 `216`/`40`。registry 新增 13 条 immutable rules（type-5 tile 5 条、
wall 2 条；type-6 tile 4 条、wall 2 条），总规则数从 69 增至 82。verifier 对每个
合并 family 使用 `FrozenSet<ushort>` 做精确数值断言：Grass/Sand/Snow/Dirt 为
`0,2,23,53,109,112,116,147,199,234,477,492`；type-6 增加
`397,398,399,402`；Moss/Stone/Ice/Sandstone 为
`1,25,117,161,163,164,179,180,181,182,183,200,203,381,396,400,401,403,534,536,539,625,627`。
wall 的两个合并 family 则分别为 Stone/NewWall1..4/Ice/Sandstone 的 62 项集合，以及
HardenedSand/Dirt/Snow 的 `2,16,40,216,217,218,219,249,304,305,306,307` 集合。

`LegacyWorldInfectionConversionCommand.IsValid` 现在允许 conversion type `5/6`，
仍对未建模的 type `7`（以及 `0`、`11-21`）fail-closed。常规 tile/wall intent 只
投影为 `TileChangeCommand(UpdateTileType)`，通过既有 `TileChangeCommitSystem`
确定性提交；type-5 的 `53/397` 选择只读取 immutable snapshot 和
`TileDefinitionRegistry`，不访问 `Main.tile`。提交保留 liquid、frame、wire、paint、
coating、fullbright 等不属于转换目标的扩展状态。type-5 的 Torch frame style `16`、
type-6 的 style `9` 以及 Thorn 的 `KillTile`/`NetMessage.SendData` 继续返回结构化
`TorchFrameMutation`/`ThornKillMutation` Deferred；错误打开另一通道或未注册类别也不会静默改变世界。

focused verifier 覆盖 13 条新规则的 priority/target/source set、open-below 与
solid-below 的 `397/53` 选择、type-6 的 `147/161`、wall `187/216/71/40`、self-target
no-op、tile/wall channel、stale snapshot、扩展状态、逆序/正序 deterministic commit
以及 Torch/Thorn Deferred。fixture 的 regular commit 计数为 4 tile、3 wall、1 no-op、
4 Deferred，sequence 从 `12000` 到 `12007`。本轮 fresh evidence 位于
`Build/diagnostics/worldgen-infection-conversion-type5-6-20260830-rerun2/`；其中
Simulation 与 WorldGeneration verifier serial build、reduced/full verifier 均 exit 0，
overlay/JSON、forbidden-dependency、样式与 scoped diff checks 均通过，冻结 deletion
baseline 仍为 `5,040,000 / 3,190,404 / 1,046,843`。

这仍是局部 type-5/6 registry/commit slice：`hardUpdateWorld`/infection runtime
pipeline 接线、global pass 调度与随机顺序、Torch frame 实际变更、Thorn `KillTile`、
`NetMessage` publication、conversion type `0/7/11-21`、完整 TileID/WallID registry、
full WorldGen differential 和 legacy `WorldGen.cs` 删除继续 Deferred；迁移状态保持
`Partial / In Progress`，`canRemoveLegacyWorldGen=false`。

## 6. 实施顺序

### 批次 A：冻结契约

1. 扩展现有 `WorldGenerationRequest`、`WorldSeedComponent`、`WorldRuleSnapshotComponent`
   表达 seed variant、secret seed、difficulty、hardmode、world size、spawn、surface、
   rock layer 和 random stream version。
2. 新增/确认 `SecretSeedDefinitionRegistry`、`SkyblockRuleSnapshot`、
   `TilePresenceScanResult` 的只读形状；禁止保留旧 public mutable arrays/List。
3. 为每个字段补齐 `SourceAnchor`、`TargetOwner`、`Mutability`、`DefaultValue`、
   `Persistence`、`ProtocolProjection`、`Verification` 七列机器可读清单。

### 批次 B：低耦合定义和派生属性

1. 先迁移 SavedOreTiers、GrowTreeSettings、世界尺寸/距离常量和 ocean level。
2. 将 SecretSeed.Variations 与 Skyblock 属性改为纯 query，并对互斥规则做表驱动测试。
3. 将 `genRand` 改为版本化随机状态；记录每阶段 cursor 和命令 sequence。

### 批次 C：扫描、结构和事件状态

1. 迁移 Tile/Wall presence、感染/evil 统计、房屋 footprint 和 TileMerge cull。
2. 将 `TownManager`、room scratch、spawn flags 拆成 housing/structure/NPC/world-event
   的独立 component 和 snapshot。
3. 对 `spawnMeteor`、`TreeTops` 等已有 WLD 字段建立
   `LegacyWorldMetadata -> CompatibilityWorldMetadata -> Dome projection` 证据链。

### 批次 D：行为和删除门禁

1. 每个字段族绑定生成 system，所有写入进入 typed command 和 deterministic commit。
2. 使用固定 seed、world size、rules、spawn 和 random stream 做 stage replay 与
   immutable snapshot differential。
3. 只有 status overlay 对全部字段给出批准的 `Partial`/`Deferred`/`Excluded` 状态、完整
   差分无未解释差异、forbidden dependency scan 与 focused/full verifier 均通过后，才重新评估
   `canRemoveLegacyWorldGen`。

## 7. 验收标准

字段迁移不能以“新代码中出现同名字段”作为通过条件。每一项至少需要：

| 验收维度 | 必须证明 |
| --- | --- |
| Owner | 唯一的 Definition、Component、Snapshot 或 Compatibility owner |
| 输入 | 默认值、范围、互斥规则和可变性在生成开始前冻结 |
| 读取 | 读取只来自 request、component 或 immutable snapshot |
| 写入 | 通过 typed command，提交器负责边界、冲突、revision 和排序 |
| 生命周期 | 一次性生成状态、跨 Tick 世界状态、宿主状态三者分离 |
| 随机 | 可由 seed/version/stage/cursor 重放，不能依赖 `Main.rand` 顺序 |
| 保存/协议 | 需要持久化或复制的字段有显式 projection；客户端字段有 Excluded 证据 |
| 验证 | 有 source anchor、focused verifier、必要时 WLD/oracle differential |
| 删除 | overlay 无未解释字段；删除门禁、完整差分和完整 build 均通过 |

推荐的最小 focused verifier 场景：

- 默认 seed：阶段顺序、`WorldGenerationStateComponent`、命令 sequence 单调性。
- secret seed 组合：Variation 属性与 policy query 的真值表。
- Skyblock：Tile/Wall 扫描结果、低 Tile 阈值和规则快照不可变性。
- Tree/Ore：definition registry、边界/保护 footprint 和 deterministic placement。
- Liquid/Tile frame：command 冲突排序、section version、失败回滚。
- Persistence：`spawnMeteor`/`TreeTops` 的 round-trip 与 projection parity。

## 8. 当前明确缺口

- 结构化字段清单的 233 项均已在本矩阵中具备文本映射（`TEXTUAL_MISSING=0`）；状态 overlay
  已为全部 233 个字段提供机器可读 owner/source/status，这不等于
  行为完成，仍有大量条目标记为 `Partial`、`Deferred` 或 `Excluded/Deferred`，inventory
  的行为状态不得因文本覆盖而自动升级。
- Secret seed 注册/启用生命周期、完整 `NPCID`/Tile/Wall 静态表、完整 biome/structure
  参数和全量生成 pass 顺序仍未等价。
- `Main`/`Tile`/`Liquid`/`NetMessage` 副作用、音效、渲染、UI、宿主事件和旧线程状态不能
  直接迁入 Simulation。
- 完整 WorldGen differential 仍有 Tile 与 extended-state mismatch，故不得删除旧入口。
- `WorldGen` 本次没有 ID 字段/属性需要统计排除；后续源版本必须重新按语义规则审查，不能
  只按 `int` 类型排除。

## 9. 复核命令

从仓库根目录执行，生成新证据路径，不覆盖既有报告：

```powershell
$source = 'D:\TRbackup\Version4物理删除了某些文件\Terraria\WorldGen.cs'
Get-FileHash $source -Algorithm SHA256
$inventory = Get-Content -Raw 'docs/worldgen/worldgen-source-inventory.json' | ConvertFrom-Json
$inventory.Fields.Count

powershell -NoProfile -ExecutionPolicy Bypass -File .\Build\Tools\ValidateWorldGenFieldStatusOverlay.ps1

dotnet build .\src\Terraria.Dome.Simulation\Terraria.Dome.Simulation.csproj `
  -p:UseSharedCompilation=false

dotnet run --project .\test\Terraria.Dome.WorldGeneration.Verification\Terraria.Dome.WorldGeneration.Verification.csproj `
  -p:UseSharedCompilation=false
```

验证结果必须记录 exit code、warning/error、source hash、字段清单状态、stage trace、
snapshot fingerprint 和 differential mismatch；仅编译通过不能宣称字段语义迁移完成。
