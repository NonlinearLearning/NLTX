# P16 世界生命周期、住房、指标与环境缓存 - 权威模拟系统成员组件候选分区

> 来源：`docs/migration/ledgers/Version4权威模拟系统字段属性逐成员源码声明-更细子系统拆分-去除ID类文件.md`；本文件只做成员库存分区和候选组件边界记录。
> 这是候选组件拆分台账，不是运行时迁移完成、行为等价、API 兼容、网络闭合或持久化闭合证明。

## 1. 分区边界

本分区包含 13 个叶子子系统，字段 130 条、属性 3 条、成员合计 133 条；每个叶子子系统保持原正式父级归属，不跨分区拆行。

## 2. 分区覆盖概览

| 叶子子系统 | 正式父级 | 字段 | 属性 | 合计 | 源边界角色 |
|---|---|---:|---:|---:|---|
| `WorldGenBiomeBackgroundAndDistanceMetrics` | `WorldGenerationAndEcology` | 21 | 0 | 21 | derived/query |
| `WorldGenTileCountMetrics` | `WorldGenerationAndEcology` | 14 | 0 | 14 | derived/query |
| `WorldLifecycleLoadAndTransformState` | `WorldGenerationAndEcology` | 7 | 0 | 7 | authoritative state/behavior |
| `WorldLifecycleProgressionAndEventState` | `WorldGenerationAndEcology` | 6 | 0 | 6 | authoritative state/behavior |
| `WorldLifecycleHousingAndSpawnPacingState` | `WorldGenerationAndEcology` | 7 | 0 | 7 | authoritative state/behavior |
| `WorldLifecycleTileMergeState` | `WorldGenerationAndEcology` | 4 | 0 | 4 | authoritative state/behavior |
| `WorldHousingCountersAndScoringState` | `WorldGenerationAndEcology` | 15 | 0 | 15 | authoritative state/behavior |
| `WorldHousingRoomSearchState` | `WorldGenerationAndEcology` | 19 | 0 | 19 | authoritative state/behavior |
| `WorldHousingRuleAndDiagnosticState` | `WorldGenerationAndEcology` | 5 | 0 | 5 | definition/query |
| `WorldGenerationDimensionsState` | `WorldGenerationAndEcology` | 8 | 0 | 8 | authoritative state/behavior |
| `WorldGenerationScratchState` | `WorldGenerationAndEcology` | 4 | 0 | 4 | authoritative state/behavior |
| `WorldTerrainEffectsAndCaches` | `WorldGenerationAndEcology` | 20 | 0 | 20 | authoritative state/behavior |
| `WorldGenDerivedProperties` | `WorldGenerationAndEcology` | 0 | 3 | 3 | derived/query |

## 3. 候选组件归属与 seam

| 叶子子系统 | 候选组件边界 | 类型分类 | 最小 seam | 独立生命周期 | 证据状态 | 主要风险 |
|---|---|---|---|---|---|---|
| `WorldGenBiomeBackgroundAndDistanceMetrics` | `WorldGenBiomeBackgroundAndDistanceMetricsQuery` | Query | 纯资格或派生 Query；只读 | 候选纯读边界；仍需核对失效/缓存条件 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `WorldGenTileCountMetrics` | `WorldGenTileCountMetricsQuery` | Query | 纯资格或派生 Query；只读 | 候选纯读边界；仍需核对失效/缓存条件 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `WorldLifecycleLoadAndTransformState` | `WorldLifecycleLoadAndTransformStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `WorldLifecycleProgressionAndEventState` | `WorldLifecycleProgressionAndEventStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `WorldLifecycleHousingAndSpawnPacingState` | `WorldLifecycleHousingAndSpawnPacingStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `WorldLifecycleTileMergeState` | `WorldLifecycleTileMergeStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `WorldHousingCountersAndScoringState` | `WorldHousingCountersAndScoringStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `WorldHousingRoomSearchState` | `WorldHousingRoomSearchStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `WorldHousingRuleAndDiagnosticState` | `WorldHousingRuleAndDiagnosticStateDefinition`/`Query` | Definition/Query | 只读 Definition/Catalog view | 候选纯读边界；仍需核对失效/缓存条件 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `WorldGenerationDimensionsState` | `WorldGenerationDimensionsStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `WorldGenerationScratchState` | `WorldGenerationScratchStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `WorldTerrainEffectsAndCaches` | `WorldTerrainEffectsAndCachesComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `WorldGenDerivedProperties` | `WorldGenDerivedPropertiesQuery` | Query | 纯资格或派生 Query；只读 | 候选纯读边界；仍需核对失效/缓存条件 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |

## 4. 分区内依赖方向

- 输入、网络命令或世界配置先进入意图/资格 Query，再由本分区的唯一 Owner System 通过 Command/CommitPort 写入权威 Component。
- `derived/query` 和 `definition/query` 只能读权威状态或定义；`registry/projection`、快照、复制和表现边界只做单向输出。
- 跨分区交接使用只读 Query、显式 Command、Event 或 Projection；Markdown 文件顺序不表达运行时执行顺序。

## 5. 拆分前证据缺口

- 源报告确认了成员、声明类型、来源路径、行列、字段/属性及候选细分归属，但没有闭合每个成员的完整读者、写者、创建/清理/持久化/网络生命周期。
- `confirmed` 仅表示 `source-inventory-confirmed`；组件是否需要拆成多个结构、是否为缓存或兼容投影，必须在迁移前补充 Version4 调用点和 focused verifier 证据。
- 外部 SS14 证据只用于组件/System/Query 的组织粒度；tModLoader `v2026.07` 只用于公开 API 边界交叉参考，不替代 Version4 私有语义证据。

## 6. 兼容策略与验证计划

- 兼容策略：先保持 Version4 原始声明、公共 API、命名空间和外部类型边界不变；每次只迁移一个叶子边界，并以 Adapter/Projection 保留旧读路径，确认新 Owner System 的写入闭合后再移除兼容层。
- 暂不拆分项：源报告已经按声明类型、生命周期或访问边界分开的叶子组不再按字段数量机械切块；`Actions`、`WorkItem` 和短生命周期参数保持 Command payload，定义/catalog/profile/rule 保持只读 Definition/Query。
- focused verifier：权威状态验证状态转换、唯一写者、重复 Command 和清理边界；纯 Query 验证确定性与无写回；Projection/Adapter 验证单向输出；所有成员迁移批次验证来源序号、声明行和原始类型闭包。
- 验证状态：以上是迁移前计划；本分区只完成成员库存逐行一致性检查，未执行 C# 编译、运行时行为、网络复制或持久化恢复测试。

## 7. 逐成员源码声明

### 正式父级子系统：`WorldGenerationAndEcology`
- 父级职责：沿用源报告正式父级 `WorldGenerationAndEcology`；本分区只收录该父级中列出的叶子子系统。
- 本分区父级局部统计：字段 130；属性 3；合计 133；完整父级统计以源报告为准。

#### 4.20.1 细分子系统：`WorldGenBiomeBackgroundAndDistanceMetrics`

- 细分职责：Biome 背景、距离、安全边界和生成随机性指标。
- 边界角色：`derived/query`；最小 seam：只读快照/纯 Query；指标不反向成为生成权威状态。
- 成员文件数：1；声明类型数：1；字段：21；属性：0；合计：21。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（21）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2417 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4075 | 2 | TownManager | Terraria.GameContent.TownRoomManager | `public static TownRoomManager TownManager = new TownRoomManager();` | `public static TownRoomManager TownManager = new TownRoomManager();` |
| 2418 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4077 | 2 | Manifest | Terraria.WorldBuilding.WorldManifest | `public static WorldManifest Manifest;` | `public static WorldManifest Manifest;` |
| 2419 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4079 | 2 | tileReframeCount | int | `public static int tileReframeCount;` | `public static int tileReframeCount;` |
| 2420 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4081 | 2 | treeBG1 | int | `public static int treeBG1;` | `public static int treeBG1;` |
| 2421 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4083 | 2 | treeBG2 | int | `public static int treeBG2;` | `public static int treeBG2;` |
| 2422 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4085 | 2 | treeBG3 | int | `public static int treeBG3;` | `public static int treeBG3;` |
| 2423 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4087 | 2 | treeBG4 | int | `public static int treeBG4;` | `public static int treeBG4;` |
| 2424 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4089 | 2 | corruptBG | int | `public static int corruptBG;` | `public static int corruptBG;` |
| 2425 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4091 | 2 | jungleBG | int | `public static int jungleBG;` | `public static int jungleBG;` |
| 2426 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4093 | 2 | snowBG | int | `public static int snowBG;` | `public static int snowBG;` |
| 2427 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4095 | 2 | hallowBG | int | `public static int hallowBG;` | `public static int hallowBG;` |
| 2428 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4097 | 2 | crimsonBG | int | `public static int crimsonBG;` | `public static int crimsonBG;` |
| 2429 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4099 | 2 | desertBG | int | `public static int desertBG;` | `public static int desertBG;` |
| 2430 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4101 | 2 | oceanBG | int | `public static int oceanBG;` | `public static int oceanBG;` |
| 2431 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4103 | 2 | mushroomBG | int | `public static int mushroomBG;` | `public static int mushroomBG;` |
| 2432 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4105 | 2 | underworldBG | int | `public static int underworldBG;` | `public static int underworldBG;` |
| 2433 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4107 | 2 | oceanDistance | int | `public static readonly int oceanDistance = 250;` | `public static readonly int oceanDistance = 250;` |
| 2434 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4109 | 2 | beachDistance | int | `public static readonly int beachDistance = 380;` | `public static readonly int beachDistance = 380;` |
| 2435 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4111 | 2 | shimmerSafetyDistance | int | `public static readonly int shimmerSafetyDistance = 150;` | `public static readonly int shimmerSafetyDistance = 150;` |
| 2436 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4113 | 2 | crimson | bool | `public static bool crimson;` | `public static bool crimson;` |
| 2437 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4115 | 2 | generatingRandomEvil | bool | `public static bool generatingRandomEvil;` | `public static bool generatingRandomEvil;` |

##### 属性（0）

无该类型成员记录。


#### 4.20.2 细分子系统：`WorldGenTileCountMetrics`

- 细分职责：Tile、邪恶、血腥、善良和固体数量统计指标。
- 边界角色：`derived/query`；最小 seam：只读快照/纯 Query；统计窗口由生成阶段显式刷新。
- 成员文件数：1；声明类型数：1；字段：14；属性：0；合计：14。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（14）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2438 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4117 | 2 | tileCounts | int[] | `public static int[] tileCounts = new int[TileID.Count];` | `public static int[] tileCounts = new int[TileID.Count];` |
| 2439 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4119 | 2 | totalEvil | int | `public static int totalEvil;` | `public static int totalEvil;` |
| 2440 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4121 | 2 | totalBlood | int | `public static int totalBlood;` | `public static int totalBlood;` |
| 2441 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4123 | 2 | totalGood | int | `public static int totalGood;` | `public static int totalGood;` |
| 2442 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4125 | 2 | totalSolid | int | `public static int totalSolid;` | `public static int totalSolid;` |
| 2443 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4127 | 2 | totalEvil2 | int | `public static int totalEvil2;` | `public static int totalEvil2;` |
| 2444 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4129 | 2 | totalBlood2 | int | `public static int totalBlood2;` | `public static int totalBlood2;` |
| 2445 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4131 | 2 | totalGood2 | int | `public static int totalGood2;` | `public static int totalGood2;` |
| 2446 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4133 | 2 | totalSolid2 | int | `public static int totalSolid2;` | `public static int totalSolid2;` |
| 2447 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4135 | 2 | tEvil | byte | `public static byte tEvil;` | `public static byte tEvil;` |
| 2448 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4137 | 2 | tBlood | byte | `public static byte tBlood;` | `public static byte tBlood;` |
| 2449 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4139 | 2 | tGood | byte | `public static byte tGood;` | `public static byte tGood;` |
| 2450 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4141 | 2 | totalX | int | `public static int totalX;` | `public static int totalX;` |
| 2451 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4143 | 2 | totalD | int | `public static int totalD;` | `public static int totalD;` |

##### 属性（0）

无该类型成员记录。


#### 4.20.3 细分子系统：`WorldLifecycleLoadAndTransformState`

- 细分职责：世界加载、变换、失败和备份生命周期状态。
- 边界角色：`authoritative state/behavior`；最小 seam：World Lifecycle System/CommitPort；加载和变换阶段集中提交。
- 成员文件数：1；声明类型数：1；字段：7；属性：0；合计：7。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（7）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2452 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4145 | 2 | _transformingWorld | int | `private static int _transformingWorld;` | `private static int _transformingWorld;` |
| 2455 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4151 | 2 | isGeneratingOrLoadingWorld | bool | `public static volatile bool isGeneratingOrLoadingWorld;` | `public static volatile bool isGeneratingOrLoadingWorld;` |
| 2462 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4165 | 2 | loadFailed | bool | `public static bool loadFailed = false;` | `public static bool loadFailed = false;` |
| 2463 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4167 | 2 | worldCleared | bool | `public static bool worldCleared;` | `public static bool worldCleared;` |
| 2464 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4169 | 2 | worldBackup | bool | `public static bool worldBackup;` | `public static bool worldBackup;` |
| 2465 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4171 | 2 | lastMaxTilesX | int | `private static int lastMaxTilesX;` | `private static int lastMaxTilesX;` |
| 2466 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4173 | 2 | lastMaxTilesY | int | `private static int lastMaxTilesY;` | `private static int lastMaxTilesY;` |

##### 属性（0）

无该类型成员记录。


#### 4.20.4 细分子系统：`WorldLifecycleProgressionAndEventState`

- 细分职责：Boss、祭坛、暗影球和陨石等世界进度事件状态。
- 边界角色：`authoritative state/behavior`；最小 seam：World Progression System/CommitPort；进度事件单向写入。
- 成员文件数：1；声明类型数：1；字段：6；属性：0；合计：6。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（6）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2453 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4147 | 2 | spawnEye | bool | `public static bool spawnEye;` | `public static bool spawnEye;` |
| 2454 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4149 | 2 | spawnHardBoss | int | `public static int spawnHardBoss;` | `public static int spawnHardBoss;` |
| 2456 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4153 | 2 | shadowOrbSmashed | bool | `public static bool shadowOrbSmashed;` | `public static bool shadowOrbSmashed;` |
| 2457 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4155 | 2 | shadowOrbCount | int | `public static int shadowOrbCount;` | `public static int shadowOrbCount;` |
| 2458 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4157 | 2 | altarCount | int | `public static int altarCount;` | `public static int altarCount;` |
| 2461 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4163 | 2 | spawnMeteor | bool | `public static bool spawnMeteor;` | `public static bool spawnMeteor;` |

##### 属性（0）

无该类型成员记录。


#### 4.20.5 细分子系统：`WorldLifecycleHousingAndSpawnPacingState`

- 细分职责：住房诊断、掉落许可、感染传播和 NPC 生成节奏状态。
- 边界角色：`authoritative state/behavior`；最小 seam：World Rules System/CommitPort；世界规则按生命周期阶段更新。
- 成员文件数：1；声明类型数：1；字段：7；属性：0；合计：7。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（7）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2459 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4159 | 2 | builtHouseWithNoFurniture | bool | `public static bool builtHouseWithNoFurniture;` | `public static bool builtHouseWithNoFurniture;` |
| 2460 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4161 | 2 | builtHouseWithNoLight | bool | `public static bool builtHouseWithNoLight;` | `public static bool builtHouseWithNoLight;` |
| 2471 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4183 | 2 | stopDrops | bool | `private static bool stopDrops;` | `private static bool stopDrops;` |
| 2472 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4185 | 2 | AllowedToSpreadInfections | bool | `public static bool AllowedToSpreadInfections = true;` | `public static bool AllowedToSpreadInfections = true;` |
| 2473 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4187 | 2 | destroyObject | bool | `public static bool destroyObject;` | `public static bool destroyObject;` |
| 2474 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4189 | 2 | npcSpawnDelay | int | `public static int npcSpawnDelay;` | `public static int npcSpawnDelay;` |
| 2475 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4191 | 2 | npcSpawnPeriod | int | `public static int npcSpawnPeriod;` | `public static int npcSpawnPeriod;` |

##### 属性（0）

无该类型成员记录。


#### 4.20.6 细分子系统：`WorldLifecycleTileMergeState`

- 细分职责：Tile 合并方向和合并过程状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Tile Merge System/CommitPort；合并方向由 Tile 系统集中维护。
- 成员文件数：1；声明类型数：1；字段：4；属性：0；合计：4。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（4）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2467 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4175 | 2 | mergeUp | bool | `private static bool mergeUp;` | `private static bool mergeUp;` |
| 2468 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4177 | 2 | mergeDown | bool | `private static bool mergeDown;` | `private static bool mergeDown;` |
| 2469 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4179 | 2 | mergeLeft | bool | `private static bool mergeLeft;` | `private static bool mergeLeft;` |
| 2470 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4181 | 2 | mergeRight | bool | `private static bool mergeRight;` | `private static bool mergeRight;` |

##### 属性（0）

无该类型成员记录。


#### 4.20.7 细分子系统：`WorldHousingCountersAndScoringState`

- 细分职责：住房扫描计数、容量阈值和房间评分状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Housing System/CommitPort；扫描阶段集中更新计数和评分。
- 成员文件数：1；声明类型数：1；字段：15；属性：0；合计：15。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（15）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2476 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4193 | 2 | prioritizedTownNPCType | int | `public static int prioritizedTownNPCType;` | `public static int prioritizedTownNPCType;` |
| 2477 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4195 | 2 | numTileCount | int | `public static int numTileCount;` | `public static int numTileCount;` |
| 2478 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4197 | 2 | maxTileCount | int | `public static int maxTileCount = 3500;` | `public static int maxTileCount = 3500;` |
| 2479 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4199 | 2 | maxWallOut2 | int | `public static int maxWallOut2 = 5000;` | `public static int maxWallOut2 = 5000;` |
| 2480 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4201 | 2 | CountedTiles | System.Collections.Generic.Dictionary<Point, bool> | `public static Dictionary<Point, bool> CountedTiles = new Dictionary<Point, bool>(maxTileCount);` | `public static Dictionary<Point, bool> CountedTiles = new Dictionary<Point, bool>(maxTileCount);` |
| 2481 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4203 | 2 | lavaCount | int | `public static int lavaCount;` | `public static int lavaCount;` |
| 2482 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4205 | 2 | iceCount | int | `public static int iceCount;` | `public static int iceCount;` |
| 2483 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4207 | 2 | sandCount | int | `public static int sandCount;` | `public static int sandCount;` |
| 2484 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4209 | 2 | rockCount | int | `public static int rockCount;` | `public static int rockCount;` |
| 2485 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4211 | 2 | shroomCount | int | `public static int shroomCount;` | `public static int shroomCount;` |
| 2486 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4213 | 2 | maxRoomTiles | int | `public static int maxRoomTiles = 750;` | `public static int maxRoomTiles = 750;` |
| 2487 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4215 | 2 | maxRoomSize | int | `public static int maxRoomSize = 100;` | `public static int maxRoomSize = 100;` |
| 2488 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4217 | 2 | roomTiles | Terraria.Utilities.BitSet2D | `public static BitSet2D roomTiles = new BitSet2D();` | `public static BitSet2D roomTiles = new BitSet2D();` |
| 2489 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4219 | 2 | numRoomTiles | int | `public static int numRoomTiles;` | `public static int numRoomTiles;` |
| 2498 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4237 | 2 | hiScore | int | `public static int hiScore;` | `public static int hiScore;` |

##### 属性（0）

无该类型成员记录。


#### 4.20.8 细分子系统：`WorldHousingRoomSearchState`

- 细分职责：房间坐标、门桌椅、候选点和搜索失败状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Housing Query/System seam；搜索过程通过显式快照和结果提交。
- 成员文件数：1；声明类型数：1；字段：19；属性：0；合计：19。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（19）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2490 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4221 | 2 | roomX1 | int | `public static int roomX1;` | `public static int roomX1;` |
| 2491 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4223 | 2 | roomX2 | int | `public static int roomX2;` | `public static int roomX2;` |
| 2492 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4225 | 2 | roomY1 | int | `public static int roomY1;` | `public static int roomY1;` |
| 2493 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4227 | 2 | roomY2 | int | `public static int roomY2;` | `public static int roomY2;` |
| 2494 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4229 | 2 | canSpawn | bool | `public static bool canSpawn;` | `public static bool canSpawn;` |
| 2495 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4231 | 2 | houseTile | bool[] | `public static bool[] houseTile = new bool[TileID.Count];` | `public static bool[] houseTile = new bool[TileID.Count];` |
| 2496 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4233 | 2 | bestX | int | `public static int bestX;` | `public static int bestX;` |
| 2497 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4235 | 2 | bestY | int | `public static int bestY;` | `public static int bestY;` |
| 2499 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4239 | 2 | roomTorch | bool | `private static bool roomTorch;` | `private static bool roomTorch;` |
| 2500 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4241 | 2 | roomDoor | bool | `private static bool roomDoor;` | `private static bool roomDoor;` |
| 2501 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4243 | 2 | roomChair | bool | `private static bool roomChair;` | `private static bool roomChair;` |
| 2502 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4245 | 2 | roomTable | bool | `private static bool roomTable;` | `private static bool roomTable;` |
| 2503 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4247 | 2 | roomHasStinkbug | bool | `private static bool roomHasStinkbug;` | `private static bool roomHasStinkbug;` |
| 2504 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4249 | 2 | roomHasEchoStinkbug | bool | `private static bool roomHasEchoStinkbug;` | `private static bool roomHasEchoStinkbug;` |
| 2510 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4261 | 2 | LastFoundHouse | Point | `private static Point LastFoundHouse;` | `private static Point LastFoundHouse;` |
| 2511 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4263 | 2 | currentlyTryingToUseAlternateHousingSpot | bool | `private static bool currentlyTryingToUseAlternateHousingSpot;` | `private static bool currentlyTryingToUseAlternateHousingSpot;` |
| 2512 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4265 | 2 | sharedRoomX | int | `private static int sharedRoomX;` | `private static int sharedRoomX;` |
| 2513 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4267 | 2 | _roomCheckStack | System.Collections.Generic.Stack<Point> | `private static Stack<Point> _roomCheckStack = new Stack<Point>();` | `private static Stack<Point> _roomCheckStack = new Stack<Point>();` |
| 2514 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4269 | 2 | roomCheckFailureReason | Terraria.Enums.TownNPCRoomCheckFailureReason | `public static TownNPCRoomCheckFailureReason roomCheckFailureReason = TownNPCRoomCheckFailureReason.None;` | `public static TownNPCRoomCheckFailureReason roomCheckFailureReason = TownNPCRoomCheckFailureReason.None;` |

##### 属性（0）

无该类型成员记录。


#### 4.20.9 细分子系统：`WorldHousingRuleAndDiagnosticState`

- 细分职责：世界邪恶规则、仙人掌水体约束和诊断事件边界。
- 边界角色：`definition/query`；最小 seam：Definition/Diagnostics seam；日志事件不反向驱动住房权威状态。
- 成员文件数：1；声明类型数：1；字段：5；属性：0；合计：5。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（5）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2505 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4251 | 2 | WorldGenParam_Evil | int | `public static int WorldGenParam_Evil = -1;` | `public static int WorldGenParam_Evil = -1;` |
| 2506 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4253 | 2 | cactusWaterWidth | int | `public static readonly int cactusWaterWidth = 50;` | `public static readonly int cactusWaterWidth = 50;` |
| 2507 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4255 | 2 | cactusWaterHeight | int | `public static readonly int cactusWaterHeight = 25;` | `public static readonly int cactusWaterHeight = 25;` |
| 2508 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4257 | 2 | cactusWaterLimit | int | `public static readonly int cactusWaterLimit = 25;` | `public static readonly int cactusWaterLimit = 25;` |
| 2509 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4259 | 2 | mysticLogsEvent | Terraria.GameContent.Events.MysticLogFairiesEvent | `public static MysticLogFairiesEvent mysticLogsEvent = new MysticLogFairiesEvent();` | `public static MysticLogFairiesEvent mysticLogsEvent = new MysticLogFairiesEvent();` |

##### 属性（0）

无该类型成员记录。


#### 4.20.10 细分子系统：`WorldGenerationDimensionsState`

- 细分职责：世界尺寸、扩散边界和流星生成计数配置。
- 边界角色：`authoritative state/behavior`；最小 seam：WorldGen System/CommitPort；世界配置阶段集中提交。
- 成员文件数：1；声明类型数：1；字段：8；属性：0；合计：8。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（8）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2515 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4271 | 2 | meteorShowerCount | int | `public static int meteorShowerCount;` | `public static int meteorShowerCount;` |
| 2516 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4273 | 2 | WorldSizeSmallX | int | `public const int WorldSizeSmallX = 4200;` | `public const int WorldSizeSmallX = 4200;` |
| 2517 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4275 | 2 | WorldSizeSmallY | int | `public const int WorldSizeSmallY = 1200;` | `public const int WorldSizeSmallY = 1200;` |
| 2518 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4277 | 2 | WorldSizeMediumX | int | `public const int WorldSizeMediumX = 6400;` | `public const int WorldSizeMediumX = 6400;` |
| 2519 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4279 | 2 | WorldSizeMediumY | int | `public const int WorldSizeMediumY = 1800;` | `public const int WorldSizeMediumY = 1800;` |
| 2520 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4281 | 2 | WorldSizeLargeX | int | `public const int WorldSizeLargeX = 8400;` | `public const int WorldSizeLargeX = 8400;` |
| 2521 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4283 | 2 | WorldSizeLargeY | int | `public const int WorldSizeLargeY = 2400;` | `public const int WorldSizeLargeY = 2400;` |
| 2522 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4285 | 2 | InfectionAndGrassSpreadOuterWorldBuffer | int | `public const int InfectionAndGrassSpreadOuterWorldBuffer = 10;` | `public const int InfectionAndGrassSpreadOuterWorldBuffer = 10;` |

##### 属性（0）

无该类型成员记录。


#### 4.20.13 细分子系统：`WorldGenerationScratchState`

- 细分职责：陷阱、宝石和苔藓生成过程的临时工作数组与类型缓存。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；生成 pass 内部拥有并清理临时状态。
- 成员文件数：1；声明类型数：1；字段：4；属性：0；合计：4。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（4）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2525 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4292 | 2 | trapDiag | int[,] | `private static int[,] trapDiag = new int[4, 2];` | `private static int[,] trapDiag = new int[4, 2];` |
| 2526 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4294 | 2 | gem | bool[] | `private static bool[] gem = new bool[6];` | `private static bool[] gem = new bool[6];` |
| 2527 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4296 | 2 | mossType | int[] | `private static int[] mossType = new int[3];` | `private static int[] mossType = new int[3];` |
| 2528 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4298 | 2 | neonMossType | ushort | `private static ushort neonMossType;` | `private static ushort neonMossType;` |

##### 属性（0）

无该类型成员记录。


#### 4.20.14 细分子系统：`WorldTerrainEffectsAndCaches`

- 细分职责：地形覆盖、树冠/背景缓存、草扩散和结构性生成队列。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System 读取输入并通过显式 Command/CommitPort 写入；跨组只用事件、Query 或命令交接。
- 成员文件数：1；声明类型数：1；字段：20；属性：0；合计：20。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（20）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2543 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4328 | 2 | tileSolidBackup | bool[] | `private static bool[] tileSolidBackup;` | `private static bool[] tileSolidBackup;` |
| 2544 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4330 | 2 | ItemSpawnProtectionTime | int | `private const int ItemSpawnProtectionTime = 18000;` | `private const int ItemSpawnProtectionTime = 18000;` |
| 2545 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4332 | 2 | _coatingColors | System.Collections.Generic.List<Color> | `private static List<Color> _coatingColors = new List<Color>();` | `private static List<Color> _coatingColors = new List<Color>();` |
| 2546 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4334 | 2 | catTailDistance | int | `private static int catTailDistance = 8;` | `private static int catTailDistance = 8;` |
| 2547 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4336 | 2 | TreeTops | Terraria.GameContent.TreeTopsInfo | `public static TreeTopsInfo TreeTops = new TreeTopsInfo();` | `public static TreeTopsInfo TreeTops = new TreeTopsInfo();` |
| 2548 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4338 | 2 | BackgroundsCache | Terraria.GameContent.BackgroundChangeFlashInfo | `public static BackgroundChangeFlashInfo BackgroundsCache = new BackgroundChangeFlashInfo();` | `public static BackgroundChangeFlashInfo BackgroundsCache = new BackgroundChangeFlashInfo();` |
| 2549 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4340 | 2 | fossilBreak | bool | `private static bool fossilBreak = false;` | `private static bool fossilBreak = false;` |
| 2550 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4342 | 2 | ExploitDestroyQueue | System.Collections.Generic.Queue<Point> | `public static Queue<Point> ExploitDestroyQueue = new Queue<Point>();` | `public static Queue<Point> ExploitDestroyQueue = new Queue<Point>();` |
| 2551 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4344 | 2 | hardModeWorldUpdates | bool | `private static bool hardModeWorldUpdates = false;` | `private static bool hardModeWorldUpdates = false;` |
| 2552 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4346 | 2 | growGrassUnderground | bool | `private static bool growGrassUnderground = false;` | `private static bool growGrassUnderground = false;` |
| 2553 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4348 | 2 | _isRainingBoulders | bool | `private static bool _isRainingBoulders = false;` | `private static bool _isRainingBoulders = false;` |
| 2554 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4350 | 2 | _SpawnThunderStorm_SafeSpots | System.Collections.Generic.List<Rectangle> | `private static List<Rectangle> _SpawnThunderStorm_SafeSpots = new List<Rectangle>();` | `private static List<Rectangle> _SpawnThunderStorm_SafeSpots = new List<Rectangle>();` |
| 2555 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4352 | 2 | BUBBLES_SOLID_STATE_FOR_HOUSING | bool | `public const bool BUBBLES_SOLID_STATE_FOR_HOUSING = true;` | `public const bool BUBBLES_SOLID_STATE_FOR_HOUSING = true;` |
| 2556 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4354 | 2 | grassSpread | int | `public static int grassSpread;` | `public static int grassSpread;` |
| 2557 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4356 | 2 | heartPos | Point[] | `private static Point[] heartPos = new Point[100];` | `private static Point[] heartPos = new Point[100];` |
| 2558 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4358 | 2 | heartCount | int | `private static int heartCount;` | `private static int heartCount;` |
| 2559 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4360 | 2 | strip_w | int | `private const int strip_w = 200;` | `private const int strip_w = 200;` |
| 2560 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4362 | 2 | strip_h | int | `private const int strip_h = 50;` | `private const int strip_h = 50;` |
| 2561 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4364 | 2 | bitStrip | Terraria.Utilities.Vertical64BitStrips | `private static readonly Vertical64BitStrips bitStrip = new Vertical64BitStrips(202);` | `private static readonly Vertical64BitStrips bitStrip = new Vertical64BitStrips(202);` |
| 2562 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4366 | 2 | _preventInfiniteRopeFraming | bool | `public static bool _preventInfiniteRopeFraming = false;` | `public static bool _preventInfiniteRopeFraming = false;` |

##### 属性（0）

无该类型成员记录。


#### 4.20.15 细分子系统：`WorldGenDerivedProperties`

- 细分职责：世界生成随机源、转换状态和海平面派生属性。
- 边界角色：`derived/query`；最小 seam：只读快照/纯资格 Query；不得写入该分组或相邻权威状态。
- 成员文件数：1；声明类型数：1；字段：0；属性：3；合计：3。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（0）

无该类型成员记录。

##### 属性（3）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2657 | property | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4368 | 2 | TransformingWorld | bool | `public static bool TransformingWorld => _transformingWorld > 0;` | `public static bool TransformingWorld => _transformingWorld > 0;` |
| 2658 | property | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4370 | 2 | genRand | Terraria.Utilities.UnifiedRandom | `public static UnifiedRandom genRand => Main.rand;` | `public static UnifiedRandom genRand => Main.rand;` |
| 2659 | property | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4372 | 2 | oceanLevel | double | `public static double oceanLevel => (Main.worldSurface + Main.rockLayer) / 2.0 + 40.0;` | `public static double oceanLevel => (Main.worldSurface + Main.rockLayer) / 2.0 + 40.0;` |


## 8. 本分区自检

- 叶子子系统：13 个；指定组均已写入且没有重复分区。
- 字段/属性/成员：130 / 3 / 133。
- 来源序号范围：2417..2659；本分区内唯一序号：True。
- 成员声明表逐组从源报告提取，未改写来源序号、类型、路径、行列、成员、C# 类型、成员声明或原始声明。
- 运行时读写者、生命周期、持久化、网络和调度：未验证，保留为迁移前风险。
