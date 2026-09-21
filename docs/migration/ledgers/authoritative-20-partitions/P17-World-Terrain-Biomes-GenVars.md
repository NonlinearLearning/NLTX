# P17 世界地形、生物群系与 GenVars 结构 - 权威模拟系统成员组件候选分区

> 来源：`docs/migration/ledgers/Version4权威模拟系统字段属性逐成员源码声明-更细子系统拆分-去除ID类文件.md`；本文件只做成员库存分区和候选组件边界记录。
> 这是候选组件拆分台账，不是运行时迁移完成、行为等价、API 兼容、网络闭合或持久化闭合证明。

## 1. 分区边界

本分区包含 14 个叶子子系统，字段 143 条、属性 3 条、成员合计 146 条；每个叶子子系统保持原正式父级归属，不跨分区拆行。

## 2. 分区覆盖概览

| 叶子子系统 | 正式父级 | 字段 | 属性 | 合计 | 源边界角色 |
|---|---|---:|---:|---:|---|
| `GenVarsConfigurationAndOreState` | `WorldGenerationAndEcology` | 10 | 0 | 10 | authoritative state/behavior |
| `GenVarsWorldLayerMetrics` | `WorldGenerationAndEcology` | 13 | 0 | 13 | authoritative state/behavior |
| `GenVarsSurfaceAndBiomeState` | `WorldGenerationAndEcology` | 15 | 0 | 15 | authoritative state/behavior |
| `GenVarsBeachAndOceanBoundaryState` | `WorldGenerationAndEcology` | 12 | 0 | 12 | authoritative state/behavior |
| `WorldGenBeachAndOceanBiomeState` | `WorldGenerationAndEcology` | 12 | 0 | 12 | authoritative state/behavior |
| `WorldGenUndergroundDesertStructureState` | `WorldGenerationAndEcology` | 9 | 0 | 9 | authoritative state/behavior |
| `WorldGenJungleStructureState` | `WorldGenerationAndEcology` | 15 | 0 | 15 | authoritative state/behavior |
| `GenVarsDungeonAndIslands` | `WorldGenerationAndEcology` | 19 | 0 | 19 | authoritative state/behavior |
| `GenVarsCaveTunnelAndOrePatchState` | `WorldGenerationAndEcology` | 9 | 0 | 9 | authoritative state/behavior |
| `GenVarsMushroomBiomeAndLogState` | `WorldGenerationAndEcology` | 5 | 0 | 5 | authoritative state/behavior |
| `GenVarsLakeAndOasisState` | `WorldGenerationAndEcology` | 8 | 0 | 8 | authoritative state/behavior |
| `GenVarsHellAndSpecialStructures` | `WorldGenerationAndEcology` | 9 | 0 | 9 | authoritative state/behavior |
| `GenVarsDungeonDerivedProperties` | `WorldGenerationAndEcology` | 0 | 3 | 3 | derived/query |
| `WorldSavedOreTierState` | `WorldGenerationAndEcology` | 7 | 0 | 7 | authoritative state/behavior |

## 3. 候选组件归属与 seam

| 叶子子系统 | 候选组件边界 | 类型分类 | 最小 seam | 独立生命周期 | 证据状态 | 主要风险 |
|---|---|---|---|---|---|---|
| `GenVarsConfigurationAndOreState` | `GenVarsConfigurationAndOreStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `GenVarsWorldLayerMetrics` | `GenVarsWorldLayerMetricsComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `GenVarsSurfaceAndBiomeState` | `GenVarsSurfaceAndBiomeStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `GenVarsBeachAndOceanBoundaryState` | `GenVarsBeachAndOceanBoundaryStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `WorldGenBeachAndOceanBiomeState` | `WorldGenBeachAndOceanBiomeStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `WorldGenUndergroundDesertStructureState` | `WorldGenUndergroundDesertStructureStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `WorldGenJungleStructureState` | `WorldGenJungleStructureStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `GenVarsDungeonAndIslands` | `GenVarsDungeonAndIslandsComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `GenVarsCaveTunnelAndOrePatchState` | `GenVarsCaveTunnelAndOrePatchStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `GenVarsMushroomBiomeAndLogState` | `GenVarsMushroomBiomeAndLogStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `GenVarsLakeAndOasisState` | `GenVarsLakeAndOasisStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `GenVarsHellAndSpecialStructures` | `GenVarsHellAndSpecialStructuresComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `GenVarsDungeonDerivedProperties` | `GenVarsDungeonDerivedPropertiesQuery` | Query | 纯资格或派生 Query；只读 | 候选纯读边界；仍需核对失效/缓存条件 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `WorldSavedOreTierState` | `WorldSavedOreTierStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |

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
- 本分区父级局部统计：字段 143；属性 3；合计 146；完整父级统计以源报告为准。

#### 4.20.16 细分子系统：`GenVarsConfigurationAndOreState`

- 细分职责：生成配置、结构注册和矿石层级状态。
- 边界角色：`authoritative state/behavior`；最小 seam：WorldGen System/CommitPort；生成阶段按 pass 顺序写入。
- 成员文件数：1；声明类型数：1；字段：10；属性：0；合计：10。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（10）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2099 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 13 | 2 | configuration | Terraria.WorldBuilding.WorldGenConfiguration | `[JsonIgnore] public static WorldGenConfiguration configuration;` | `[JsonIgnore] public static WorldGenConfiguration configuration;` |
| 2100 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 16 | 2 | structures | Terraria.WorldBuilding.StructureMap | `public static StructureMap structures;` | `public static StructureMap structures;` |
| 2101 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 18 | 2 | copper | int | `public static int copper;` | `public static int copper;` |
| 2102 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 20 | 2 | iron | int | `public static int iron;` | `public static int iron;` |
| 2103 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 22 | 2 | silver | int | `public static int silver;` | `public static int silver;` |
| 2104 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 24 | 2 | gold | int | `public static int gold;` | `public static int gold;` |
| 2105 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 26 | 2 | copperBar | int | `public static int copperBar = 20;` | `public static int copperBar = 20;` |
| 2106 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 28 | 2 | ironBar | int | `public static int ironBar = 22;` | `public static int ironBar = 22;` |
| 2107 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 30 | 2 | silverBar | int | `public static int silverBar = 21;` | `public static int silverBar = 21;` |
| 2108 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 32 | 2 | goldBar | int | `public static int goldBar = 19;` | `public static int goldBar = 19;` |

##### 属性（0）

无该类型成员记录。


#### 4.20.17 细分子系统：`GenVarsWorldLayerMetrics`

- 细分职责：云层、世界表面、岩层和积雪边界测量值。
- 边界角色：`authoritative state/behavior`；最小 seam：WorldGen System/CommitPort；地层 pass 计算后提交。
- 成员文件数：1；声明类型数：1；字段：13；属性：0；合计：13。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（13）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2115 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 46 | 2 | lowestCloud | int | `public static int lowestCloud = -1;` | `public static int lowestCloud = -1;` |
| 2125 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 66 | 2 | worldSurfaceLow | double | `public static double worldSurfaceLow;` | `public static double worldSurfaceLow;` |
| 2126 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 68 | 2 | worldSurface | double | `public static double worldSurface;` | `public static double worldSurface;` |
| 2127 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 70 | 2 | worldSurfaceHigh | double | `public static double worldSurfaceHigh;` | `public static double worldSurfaceHigh;` |
| 2128 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 72 | 2 | rockLayerLow | double | `public static double rockLayerLow;` | `public static double rockLayerLow;` |
| 2129 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 74 | 2 | rockLayer | double | `public static double rockLayer;` | `public static double rockLayer;` |
| 2130 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 76 | 2 | rockLayerHigh | double | `public static double rockLayerHigh;` | `public static double rockLayerHigh;` |
| 2131 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 78 | 2 | snowTop | int | `public static int snowTop;` | `public static int snowTop;` |
| 2132 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 80 | 2 | snowBottom | int | `public static int snowBottom;` | `public static int snowBottom;` |
| 2133 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 82 | 2 | snowOriginLeft | int | `public static int snowOriginLeft;` | `public static int snowOriginLeft;` |
| 2134 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 84 | 2 | snowOriginRight | int | `public static int snowOriginRight;` | `public static int snowOriginRight;` |
| 2135 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 86 | 2 | snowMinX | int[] | `public static int[] snowMinX;` | `public static int[] snowMinX;` |
| 2136 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 88 | 2 | snowMaxX | int[] | `public static int[] snowMaxX;` | `public static int[] snowMaxX;` |

##### 属性（0）

无该类型成员记录。


#### 4.20.18 细分子系统：`GenVarsSurfaceAndBiomeState`

- 细分职责：出生点、地貌、感染、苔藓和液体线等表面/生态状态。
- 边界角色：`authoritative state/behavior`；最小 seam：WorldGen System/CommitPort；表面与生态 pass 按阶段写入。
- 成员文件数：1；声明类型数：1；字段：15；属性：0；合计：15。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（15）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2109 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 34 | 2 | worldSpawnHasBeenRandomized | bool | `public static bool worldSpawnHasBeenRandomized = false;` | `public static bool worldSpawnHasBeenRandomized = false;` |
| 2110 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 36 | 2 | landmassData | System.Collections.Generic.List<Terraria.WorldBuilding.LandmassData> | `public static List<LandmassData> landmassData = new List<LandmassData>();` | `public static List<LandmassData> landmassData = new List<LandmassData>();` |
| 2111 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 38 | 2 | remixSurfaceLayerLow | int | `public static int remixSurfaceLayerLow;` | `public static int remixSurfaceLayerLow;` |
| 2112 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 40 | 2 | remixSurfaceLayerHigh | int | `public static int remixSurfaceLayerHigh;` | `public static int remixSurfaceLayerHigh;` |
| 2113 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 42 | 2 | remixMushroomLayerLow | int | `public static int remixMushroomLayerLow;` | `public static int remixMushroomLayerLow;` |
| 2114 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 44 | 2 | remixMushroomLayerHigh | int | `public static int remixMushroomLayerHigh;` | `public static int remixMushroomLayerHigh;` |
| 2116 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 48 | 2 | boulderPetsPlaced | int | `public static int boulderPetsPlaced = 0;` | `public static int boulderPetsPlaced = 0;` |
| 2117 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 50 | 2 | crimStoneWall | ushort | `public static ushort crimStoneWall = 83;` | `public static ushort crimStoneWall = 83;` |
| 2118 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 52 | 2 | crimStone | ushort | `public static ushort crimStone = 203;` | `public static ushort crimStone = 203;` |
| 2119 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 54 | 2 | ebonStoneWall | ushort | `public static ushort ebonStoneWall = 3;` | `public static ushort ebonStoneWall = 3;` |
| 2120 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 56 | 2 | ebonStone | ushort | `public static ushort ebonStone = 25;` | `public static ushort ebonStone = 25;` |
| 2121 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 58 | 2 | mossTile | ushort | `public static ushort mossTile = 179;` | `public static ushort mossTile = 179;` |
| 2122 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 60 | 2 | mossWall | ushort | `public static ushort mossWall = 54;` | `public static ushort mossWall = 54;` |
| 2123 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 62 | 2 | lavaLine | int | `public static int lavaLine;` | `public static int lavaLine;` |
| 2124 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 64 | 2 | waterLine | int | `public static int waterLine;` | `public static int waterLine;` |

##### 属性（0）

无该类型成员记录。


#### 4.20.19 细分子系统：`GenVarsBeachAndOceanBoundaryState`

- 细分职责：海滩、贝壳起点和海洋边界随机参数。
- 边界角色：`authoritative state/behavior`；最小 seam：WorldGen System/CommitPort；海岸线 pass 是唯一写入者。
- 成员文件数：1；声明类型数：1；字段：12；属性：0；合计：12。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（12）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2137 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 90 | 2 | leftBeachEnd | int | `public static int leftBeachEnd;` | `public static int leftBeachEnd;` |
| 2138 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 92 | 2 | rightBeachStart | int | `public static int rightBeachStart;` | `public static int rightBeachStart;` |
| 2139 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 94 | 2 | beachBordersWidth | int | `public static int beachBordersWidth;` | `public static int beachBordersWidth;` |
| 2140 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 96 | 2 | beachSandRandomCenter | int | `public static int beachSandRandomCenter;` | `public static int beachSandRandomCenter;` |
| 2141 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 98 | 2 | beachSandRandomWidthRange | int | `public static int beachSandRandomWidthRange;` | `public static int beachSandRandomWidthRange;` |
| 2142 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 100 | 2 | beachSandDungeonExtraWidth | int | `public static int beachSandDungeonExtraWidth;` | `public static int beachSandDungeonExtraWidth;` |
| 2143 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 102 | 2 | beachSandJungleExtraWidth | int | `public static int beachSandJungleExtraWidth;` | `public static int beachSandJungleExtraWidth;` |
| 2144 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 104 | 2 | shellStartXLeft | int | `public static int shellStartXLeft;` | `public static int shellStartXLeft;` |
| 2145 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 106 | 2 | shellStartYLeft | int | `public static int shellStartYLeft;` | `public static int shellStartYLeft;` |
| 2146 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 108 | 2 | shellStartXRight | int | `public static int shellStartXRight;` | `public static int shellStartXRight;` |
| 2147 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 110 | 2 | shellStartYRight | int | `public static int shellStartYRight;` | `public static int shellStartYRight;` |
| 2148 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 112 | 2 | oceanWaterStartRandomMin | int | `public static int oceanWaterStartRandomMin;` | `public static int oceanWaterStartRandomMin;` |

##### 属性（0）

无该类型成员记录。


#### 4.20.20 细分子系统：`WorldGenBeachAndOceanBiomeState`

- 细分职责：海滩、海洋洞穴和沿岸生物群系生成状态。
- 边界角色：`authoritative state/behavior`；最小 seam：WorldGen System/CommitPort；海岸 pass 集中写入。
- 成员文件数：1；声明类型数：1；字段：12；属性：0；合计：12。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（12）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2149 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 114 | 2 | oceanWaterStartRandomMax | int | `public static int oceanWaterStartRandomMax;` | `public static int oceanWaterStartRandomMax;` |
| 2150 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 116 | 2 | oceanWaterForcedJungleLength | int | `public static int oceanWaterForcedJungleLength;` | `public static int oceanWaterForcedJungleLength;` |
| 2151 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 118 | 2 | evilBiomeBeachAvoidance | int | `public static int evilBiomeBeachAvoidance;` | `public static int evilBiomeBeachAvoidance;` |
| 2152 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 120 | 2 | evilBiomeAvoidanceMidFixer | int | `public static int evilBiomeAvoidanceMidFixer;` | `public static int evilBiomeAvoidanceMidFixer;` |
| 2153 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 122 | 2 | lakesBeachAvoidance | int | `public static int lakesBeachAvoidance;` | `public static int lakesBeachAvoidance;` |
| 2154 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 124 | 2 | smallHolesBeachAvoidance | int | `public static int smallHolesBeachAvoidance;` | `public static int smallHolesBeachAvoidance;` |
| 2155 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 126 | 2 | surfaceCavesBeachAvoidance | int | `public static int surfaceCavesBeachAvoidance;` | `public static int surfaceCavesBeachAvoidance;` |
| 2156 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 128 | 2 | surfaceCavesBeachAvoidance2 | int | `public static int surfaceCavesBeachAvoidance2;` | `public static int surfaceCavesBeachAvoidance2;` |
| 2157 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 130 | 2 | maxOceanCaveTreasure | int | `public static readonly int maxOceanCaveTreasure = 2;` | `public static readonly int maxOceanCaveTreasure = 2;` |
| 2158 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 132 | 2 | numOceanCaveTreasure | int | `public static int numOceanCaveTreasure = 0;` | `public static int numOceanCaveTreasure = 0;` |
| 2159 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 134 | 2 | oceanCaveTreasure | Point[] | `public static Point[] oceanCaveTreasure = new Point[maxOceanCaveTreasure];` | `public static Point[] oceanCaveTreasure = new Point[maxOceanCaveTreasure];` |
| 2160 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 136 | 2 | skipDesertTileCheck | bool | `public static bool skipDesertTileCheck = false;` | `public static bool skipDesertTileCheck = false;` |

##### 属性（0）

无该类型成员记录。


#### 4.20.21 细分子系统：`WorldGenUndergroundDesertStructureState`

- 细分职责：地下沙漠、蜂巢和幼虫结构生成状态。
- 边界角色：`authoritative state/behavior`；最小 seam：WorldGen System/CommitPort；地下沙漠 pass 负责唯一写入。
- 成员文件数：1；声明类型数：1；字段：9；属性：0；合计：9。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（9）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2161 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 138 | 2 | UndergroundDesertLocation | Rectangle | `public static Rectangle UndergroundDesertLocation = Rectangle.Empty;` | `public static Rectangle UndergroundDesertLocation = Rectangle.Empty;` |
| 2162 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 140 | 2 | UndergroundDesertHiveLocation | Rectangle | `public static Rectangle UndergroundDesertHiveLocation = Rectangle.Empty;` | `public static Rectangle UndergroundDesertHiveLocation = Rectangle.Empty;` |
| 2163 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 142 | 2 | desertHiveHigh | int | `public static int desertHiveHigh;` | `public static int desertHiveHigh;` |
| 2164 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 144 | 2 | desertHiveLow | int | `public static int desertHiveLow;` | `public static int desertHiveLow;` |
| 2165 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 146 | 2 | desertHiveLeft | int | `public static int desertHiveLeft;` | `public static int desertHiveLeft;` |
| 2166 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 148 | 2 | desertHiveRight | int | `public static int desertHiveRight;` | `public static int desertHiveRight;` |
| 2167 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 150 | 2 | numLarva | int | `public static int numLarva;` | `public static int numLarva;` |
| 2168 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 152 | 2 | larvaY | int[] | `public static int[] larvaY = new int[100];` | `public static int[] larvaY = new int[100];` |
| 2169 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 154 | 2 | larvaX | int[] | `public static int[] larvaX = new int[100];` | `public static int[] larvaX = new int[100];` |

##### 属性（0）

无该类型成员记录。


#### 4.20.22 细分子系统：`WorldGenJungleStructureState`

- 细分职责：丛林神庙、生命红木和丛林宝箱结构状态。
- 边界角色：`authoritative state/behavior`；最小 seam：WorldGen System/CommitPort；丛林结构 pass 负责唯一写入。
- 成员文件数：1；声明类型数：1；字段：15；属性：0；合计：15。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（15）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2170 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 156 | 2 | numPyr | int | `public static int numPyr;` | `public static int numPyr;` |
| 2171 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 158 | 2 | PyrX | int[] | `public static int[] PyrX;` | `public static int[] PyrX;` |
| 2172 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 160 | 2 | PyrY | int[] | `public static int[] PyrY;` | `public static int[] PyrY;` |
| 2173 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 162 | 2 | extraBastStatueCount | int | `public static int extraBastStatueCount;` | `public static int extraBastStatueCount;` |
| 2174 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 164 | 2 | extraBastStatueCountMax | int | `public static int extraBastStatueCountMax;` | `public static int extraBastStatueCountMax;` |
| 2175 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 166 | 2 | jungleOriginX | int | `public static int jungleOriginX;` | `public static int jungleOriginX;` |
| 2176 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 168 | 2 | jungleMinX | int | `public static int jungleMinX;` | `public static int jungleMinX;` |
| 2177 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 170 | 2 | jungleMaxX | int | `public static int jungleMaxX;` | `public static int jungleMaxX;` |
| 2178 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 172 | 2 | jungleHut | ushort | `public static ushort jungleHut;` | `public static ushort jungleHut;` |
| 2179 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 174 | 2 | mudWall | bool | `public static bool mudWall;` | `public static bool mudWall;` |
| 2180 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 176 | 2 | JungleItemCount | int | `public static int JungleItemCount;` | `public static int JungleItemCount;` |
| 2181 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 178 | 2 | gennedLivingMahoganyWands | bool | `public static bool gennedLivingMahoganyWands;` | `public static bool gennedLivingMahoganyWands;` |
| 2182 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 180 | 2 | JChestX | int[] | `public static int[] JChestX = new int[100];` | `public static int[] JChestX = new int[100];` |
| 2183 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 182 | 2 | JChestY | int[] | `public static int[] JChestY = new int[100];` | `public static int[] JChestY = new int[100];` |
| 2184 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 184 | 2 | numJChests | int | `public static int numJChests;` | `public static int numJChests;` |

##### 属性（0）

无该类型成员记录。


#### 4.20.23 细分子系统：`GenVarsDungeonAndIslands`

- 细分职责：地牢、天空湖、浮空岛和岛屋布局参数。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System 读取输入并通过显式 Command/CommitPort 写入；跨组只用事件、Query 或命令交接。
- 成员文件数：1；声明类型数：1；字段：19；属性：0；合计：19。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（19）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2185 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 186 | 2 | tLeft | int | `public static int tLeft;` | `public static int tLeft;` |
| 2186 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 188 | 2 | tRight | int | `public static int tRight;` | `public static int tRight;` |
| 2187 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 190 | 2 | tTop | int | `public static int tTop;` | `public static int tTop;` |
| 2188 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 192 | 2 | tBottom | int | `public static int tBottom;` | `public static int tBottom;` |
| 2189 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 194 | 2 | tRooms | int | `public static int tRooms;` | `public static int tRooms;` |
| 2190 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 196 | 2 | lAltarX | int | `public static int lAltarX;` | `public static int lAltarX;` |
| 2191 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 198 | 2 | lAltarY | int | `public static int lAltarY;` | `public static int lAltarY;` |
| 2192 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 200 | 2 | dungeonGenVars | System.Collections.Generic.List<Terraria.GameContent.Generation.Dungeon.DungeonGenVars> | `public static List<DungeonGenVars> dungeonGenVars = new List<DungeonGenVars>();` | `public static List<DungeonGenVars> dungeonGenVars = new List<DungeonGenVars>();` |
| 2193 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 202 | 2 | _currentDungeon | int | `private static int _currentDungeon;` | `private static int _currentDungeon;` |
| 2194 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 204 | 2 | dungeonBeachPadding | int | `public static readonly int dungeonBeachPadding = 50;` | `public static readonly int dungeonBeachPadding = 50;` |
| 2195 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 206 | 2 | skyLakes | int | `public static int skyLakes;` | `public static int skyLakes;` |
| 2196 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 208 | 2 | generatedShadowKey | bool | `public static bool generatedShadowKey;` | `public static bool generatedShadowKey;` |
| 2197 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 210 | 2 | generatedRamRune | bool | `public static bool generatedRamRune;` | `public static bool generatedRamRune;` |
| 2198 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 212 | 2 | numIslandHouses | int | `public static int numIslandHouses;` | `public static int numIslandHouses;` |
| 2199 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 214 | 2 | skyIslandHouseCount | int | `public static int skyIslandHouseCount;` | `public static int skyIslandHouseCount;` |
| 2200 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 216 | 2 | skyLake | bool[] | `public static bool[] skyLake = new bool[300];` | `public static bool[] skyLake = new bool[300];` |
| 2201 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 218 | 2 | floatingIslandHouseX | int[] | `public static int[] floatingIslandHouseX = new int[300];` | `public static int[] floatingIslandHouseX = new int[300];` |
| 2202 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 220 | 2 | floatingIslandHouseY | int[] | `public static int[] floatingIslandHouseY = new int[300];` | `public static int[] floatingIslandHouseY = new int[300];` |
| 2203 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 222 | 2 | floatingIslandStyle | int[] | `public static int[] floatingIslandStyle = new int[300];` | `public static int[] floatingIslandStyle = new int[300];` |

##### 属性（0）

无该类型成员记录。


#### 4.20.24 细分子系统：`GenVarsCaveTunnelAndOrePatchState`

- 细分职责：微型洞穴、隧道和矿脉 patch 生成计数与坐标。
- 边界角色：`authoritative state/behavior`；最小 seam：WorldGen Cave/Ore System/CommitPort；生成 pass 集中更新。
- 成员文件数：1；声明类型数：1；字段：9；属性：0；合计：9。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（9）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2204 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 224 | 2 | numMCaves | int | `public static int numMCaves;` | `public static int numMCaves;` |
| 2205 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 226 | 2 | mCaveX | int[] | `public static int[] mCaveX = new int[30];` | `public static int[] mCaveX = new int[30];` |
| 2206 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 228 | 2 | mCaveY | int[] | `public static int[] mCaveY = new int[30];` | `public static int[] mCaveY = new int[30];` |
| 2207 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 230 | 2 | maxTunnels | int | `public static readonly int maxTunnels = 50;` | `public static readonly int maxTunnels = 50;` |
| 2208 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 232 | 2 | numTunnels | int | `public static int numTunnels;` | `public static int numTunnels;` |
| 2209 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 234 | 2 | tunnelX | int[] | `public static int[] tunnelX = new int[maxTunnels];` | `public static int[] tunnelX = new int[maxTunnels];` |
| 2210 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 236 | 2 | maxOrePatch | int | `public static readonly int maxOrePatch = 50;` | `public static readonly int maxOrePatch = 50;` |
| 2211 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 238 | 2 | numOrePatch | int | `public static int numOrePatch;` | `public static int numOrePatch;` |
| 2212 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 240 | 2 | orePatchX | int[] | `public static int[] orePatchX = new int[maxOrePatch];` | `public static int[] orePatchX = new int[maxOrePatch];` |

##### 属性（0）

无该类型成员记录。


#### 4.20.25 细分子系统：`GenVarsMushroomBiomeAndLogState`

- 细分职责：蘑菇生物群系和树木日志生成状态。
- 边界角色：`authoritative state/behavior`；最小 seam：WorldGen Biome System/CommitPort；蘑菇/树木 pass 集中更新。
- 成员文件数：1；声明类型数：1；字段：5；属性：0；合计：5。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（5）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2213 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 242 | 2 | maxMushroomBiomes | int | `public static readonly int maxMushroomBiomes = 50;` | `public static readonly int maxMushroomBiomes = 50;` |
| 2214 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 244 | 2 | numMushroomBiomes | int | `public static int numMushroomBiomes = 0;` | `public static int numMushroomBiomes = 0;` |
| 2215 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 246 | 2 | mushroomBiomesPosition | Point[] | `public static Point[] mushroomBiomesPosition = new Point[maxMushroomBiomes];` | `public static Point[] mushroomBiomesPosition = new Point[maxMushroomBiomes];` |
| 2216 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 248 | 2 | logX | int | `public static int logX;` | `public static int logX;` |
| 2217 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 250 | 2 | logY | int | `public static int logY;` | `public static int logY;` |

##### 属性（0）

无该类型成员记录。


#### 4.20.26 细分子系统：`GenVarsLakeAndOasisState`

- 细分职责：湖泊和绿洲生成计数、坐标及尺寸。
- 边界角色：`authoritative state/behavior`；最小 seam：WorldGen Water/Biome System/CommitPort；水体 pass 集中更新。
- 成员文件数：1；声明类型数：1；字段：8；属性：0；合计：8。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（8）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2218 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 252 | 2 | maxLakes | int | `public static readonly int maxLakes = 50;` | `public static readonly int maxLakes = 50;` |
| 2219 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 254 | 2 | numLakes | int | `public static int numLakes = 0;` | `public static int numLakes = 0;` |
| 2220 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 256 | 2 | LakeX | int[] | `public static int[] LakeX = new int[maxLakes];` | `public static int[] LakeX = new int[maxLakes];` |
| 2221 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 258 | 2 | maxOasis | int | `public static readonly int maxOasis = 20;` | `public static readonly int maxOasis = 20;` |
| 2222 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 260 | 2 | numOasis | int | `public static int numOasis = 0;` | `public static int numOasis = 0;` |
| 2223 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 262 | 2 | oasisPosition | Point[] | `public static Point[] oasisPosition = new Point[maxOasis];` | `public static Point[] oasisPosition = new Point[maxOasis];` |
| 2224 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 264 | 2 | oasisWidth | int[] | `public static int[] oasisWidth = new int[maxOasis];` | `public static int[] oasisWidth = new int[maxOasis];` |
| 2225 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 266 | 2 | oasisHeight | int | `public static readonly int oasisHeight = 20;` | `public static readonly int oasisHeight = 20;` |

##### 属性（0）

无该类型成员记录。


#### 4.20.27 细分子系统：`GenVarsHellAndSpecialStructures`

- 细分职责：地狱宝箱、雕像、Shimmer 和特殊种子结构标志。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System 读取输入并通过显式 Command/CommitPort 写入；跨组只用事件、Query 或命令交接。
- 成员文件数：1；声明类型数：1；字段：9；属性：0；合计：9。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（9）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2226 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 268 | 2 | hellChest | int | `public static int hellChest;` | `public static int hellChest;` |
| 2227 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 270 | 2 | hellChestItem | int[] | `public static int[] hellChestItem;` | `public static int[] hellChestItem;` |
| 2228 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 272 | 2 | statueList | Terraria.DataStructures.Point16[] | `public static Point16[] statueList;` | `public static Point16[] statueList;` |
| 2229 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 274 | 2 | StatuesWithTraps | System.Collections.Generic.List<int> | `public static List<int> StatuesWithTraps = new List<int>(new int[4] { 4, 7, 10, 18 });` | `public static List<int> StatuesWithTraps = new List<int>(new int[4] { 4, 7, 10, 18 });` |
| 2230 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 276 | 2 | crimsonLeft | bool | `public static bool crimsonLeft = true;` | `public static bool crimsonLeft = true;` |
| 2231 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 278 | 2 | shimmerPosition | Vector2D | `public static Vector2D shimmerPosition;` | `public static Vector2D shimmerPosition;` |
| 2232 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 280 | 2 | notTheBeesAndForTheWorthyNoCelebration | bool | `public static bool notTheBeesAndForTheWorthyNoCelebration;` | `public static bool notTheBeesAndForTheWorthyNoCelebration;` |
| 2233 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 282 | 2 | noTrapsAndForTheWorthyNoCelebration | bool | `public static bool noTrapsAndForTheWorthyNoCelebration;` | `public static bool noTrapsAndForTheWorthyNoCelebration;` |
| 2234 | field | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 284 | 2 | flipInfections | bool | `public static bool flipInfections;` | `public static bool flipInfections;` |

##### 属性（0）

无该类型成员记录。


#### 4.20.28 细分子系统：`GenVarsDungeonDerivedProperties`

- 细分职责：当前地牢及双地牢距离的只读派生属性。
- 边界角色：`derived/query`；最小 seam：只读快照/纯资格 Query；不得写入该分组或相邻权威状态。
- 成员文件数：1；声明类型数：1；字段：0；属性：3；合计：3。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（0）

无该类型成员记录。

##### 属性（3）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2581 | property | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 286 | 2 | CurrentDungeon | int | `public static int CurrentDungeon { get { return _currentDungeon; } set { _currentDungeon = (int)MathHelper.Max(0f, value); } }` | `public static int CurrentDungeon { get { return _currentDungeon; } set { _currentDungeon = (int)MathHelper.Max(0f, value); } }` |
| 2582 | property | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 298 | 2 | CurrentDungeonGenVars | Terraria.GameContent.Generation.Dungeon.DungeonGenVars | `public static DungeonGenVars CurrentDungeonGenVars => dungeonGenVars[CurrentDungeon];` | `public static DungeonGenVars CurrentDungeonGenVars => dungeonGenVars[CurrentDungeon];` |
| 2583 | property | Terraria.WorldBuilding.GenVars | Terraria.WorldBuilding/GenVars.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenVars.cs | 300 | 2 | DualDungeon_NormalizedDistanceSafeFromDither | double | `public static double DualDungeon_NormalizedDistanceSafeFromDither { get { return DungeonControlLine.NormalizedDistanceSafeFromDither; } set { DungeonControlLine.NormalizedDistanceSafeFromDither = value; } }` | `public static double DualDungeon_NormalizedDistanceSafeFromDither { get { return DungeonControlLine.NormalizedDistanceSafeFromDither; } set { DungeonControlLine.NormalizedDistanceSafeFromDither = value; } }` |


#### 4.20.48 细分子系统：`WorldSavedOreTierState`

- 细分职责：存档矿石层级和矿石替换状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；矿石层级在生成/加载边界集中写入。
- 成员文件数：1；声明类型数：1；字段：7；属性：0；合计：7。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（7）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2384 | field | Terraria.WorldGen.SavedOreTiers | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 3320 | 3 | Copper | int | `public static int Copper = 7;` | `public static int Copper = 7;` |
| 2385 | field | Terraria.WorldGen.SavedOreTiers | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 3322 | 3 | Iron | int | `public static int Iron = 6;` | `public static int Iron = 6;` |
| 2386 | field | Terraria.WorldGen.SavedOreTiers | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 3324 | 3 | Silver | int | `public static int Silver = 9;` | `public static int Silver = 9;` |
| 2387 | field | Terraria.WorldGen.SavedOreTiers | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 3326 | 3 | Gold | int | `public static int Gold = 8;` | `public static int Gold = 8;` |
| 2388 | field | Terraria.WorldGen.SavedOreTiers | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 3328 | 3 | Cobalt | int | `public static int Cobalt = 107;` | `public static int Cobalt = 107;` |
| 2389 | field | Terraria.WorldGen.SavedOreTiers | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 3330 | 3 | Mythril | int | `public static int Mythril = 108;` | `public static int Mythril = 108;` |
| 2390 | field | Terraria.WorldGen.SavedOreTiers | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 3332 | 3 | Adamantite | int | `public static int Adamantite = 111;` | `public static int Adamantite = 111;` |

##### 属性（0）

无该类型成员记录。


## 8. 本分区自检

- 叶子子系统：14 个；指定组均已写入且没有重复分区。
- 字段/属性/成员：143 / 3 / 146。
- 来源序号范围：2099..2583；本分区内唯一序号：True。
- 成员声明表逐组从源报告提取，未改写来源序号、类型、路径、行列、成员、C# 类型、成员声明或原始声明。
- 运行时读写者、生命周期、持久化、网络和调度：未验证，保留为迁移前风险。
