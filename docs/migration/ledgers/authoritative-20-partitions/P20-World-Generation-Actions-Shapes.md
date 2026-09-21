# P20 世界生成动作、条件、形状与结构规划 - 权威模拟系统成员组件候选分区

> 来源：`docs/migration/ledgers/Version4权威模拟系统字段属性逐成员源码声明-更细子系统拆分-去除ID类文件.md`；本文件只做成员库存分区和候选组件边界记录。
> 这是候选组件拆分台账，不是运行时迁移完成、行为等价、API 兼容、网络闭合或持久化闭合证明。

## 1. 分区边界

本分区包含 12 个叶子子系统，字段 115 条、属性 4 条、成员合计 119 条；每个叶子子系统保持原正式父级归属，不跨分区拆行。

## 2. 分区覆盖概览

| 叶子子系统 | 正式父级 | 字段 | 属性 | 合计 | 源边界角色 |
|---|---|---:|---:|---:|---|
| `WorldTileMergeCullState` | `WorldGenerationAndEcology` | 8 | 0 | 8 | derived/query |
| `WorldGenerationTileSetActions` | `WorldGenerationAndEcology` | 12 | 0 | 12 | authoritative state/behavior |
| `WorldGenerationWallMutationActions` | `WorldGenerationAndEcology` | 7 | 0 | 7 | authoritative state/behavior |
| `WorldGenerationTilePlacementAndPaintActions` | `WorldGenerationAndEcology` | 5 | 0 | 5 | authoritative state/behavior |
| `WorldGenerationLiquidAndNeighborActions` | `WorldGenerationAndEcology` | 3 | 0 | 3 | authoritative state/behavior |
| `WorldGenerationTileScanAndControlActions` | `WorldGenerationAndEcology` | 7 | 0 | 7 | derived/query |
| `WorldGenerationTileFramingAndDebugActions` | `WorldGenerationAndEcology` | 3 | 0 | 3 | registry/projection |
| `WorldGenerationConditionsAndSearches` | `WorldGenerationAndEcology` | 11 | 0 | 11 | derived/query |
| `WorldGenerationShapeData` | `WorldGenerationAndEcology` | 8 | 2 | 10 | definition/query |
| `WorldGenerationShapeModifierState` | `WorldGenerationAndEcology` | 22 | 0 | 22 | definition/query |
| `WorldGenerationTileWallConditionState` | `WorldGenerationAndEcology` | 20 | 0 | 20 | derived/query |
| `WorldStructurePlanningAndMasks` | `WorldGenerationAndEcology` | 9 | 2 | 11 | authoritative state/behavior |

## 3. 候选组件归属与 seam

| 叶子子系统 | 候选组件边界 | 类型分类 | 最小 seam | 独立生命周期 | 证据状态 | 主要风险 |
|---|---|---|---|---|---|---|
| `WorldTileMergeCullState` | `WorldTileMergeCullStateQuery` | Query | 纯资格或派生 Query；只读 | 候选纯读边界；仍需核对失效/缓存条件 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `WorldGenerationTileSetActions` | `WorldGenerationTileSetActionsCommand` | Command payload | 显式 Command/CommitPort；不持有长期权威状态 | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `WorldGenerationWallMutationActions` | `WorldGenerationWallMutationActionsCommand` | Command payload | 显式 Command/CommitPort；不持有长期权威状态 | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `WorldGenerationTilePlacementAndPaintActions` | `WorldGenerationTilePlacementAndPaintActionsCommand` | Command payload | 显式 Command/CommitPort；不持有长期权威状态 | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `WorldGenerationLiquidAndNeighborActions` | `WorldGenerationLiquidAndNeighborActionsCommand` | Command payload | 显式 Command/CommitPort；不持有长期权威状态 | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `WorldGenerationTileScanAndControlActions` | `WorldGenerationTileScanAndControlActionsCommand` | Command payload | 显式 Command/CommitPort；不持有长期权威状态 | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `WorldGenerationTileFramingAndDebugActions` | `WorldGenerationTileFramingAndDebugActionsCommand` | Command payload | 显式 Command/CommitPort；不持有长期权威状态 | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `WorldGenerationConditionsAndSearches` | `WorldGenerationConditionsAndSearchesQuery` | Query | 纯资格或派生 Query；只读 | 候选纯读边界；仍需核对失效/缓存条件 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `WorldGenerationShapeData` | `WorldGenerationShapeDataDefinition`/`Query` | Definition/Query | 只读 Definition/Catalog view | 候选纯读边界；仍需核对失效/缓存条件 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `WorldGenerationShapeModifierState` | `WorldGenerationShapeModifierStateDefinition`/`Query` | Definition/Query | 只读 Definition/Catalog view | 候选纯读边界；仍需核对失效/缓存条件 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `WorldGenerationTileWallConditionState` | `WorldGenerationTileWallConditionStateQuery` | Query | 纯资格或派生 Query；只读 | 候选纯读边界；仍需核对失效/缓存条件 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `WorldStructurePlanningAndMasks` | `WorldStructurePlanningAndMasksComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |

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
- 本分区父级局部统计：字段 115；属性 4；合计 119；完整父级统计以源报告为准。

#### 4.20.49 细分子系统：`WorldTileMergeCullState`

- 细分职责：Tile 合并剔除方向和边界缓存。
- 边界角色：`derived/query`；最小 seam：纯查询/缓存 seam；失效条件由 framing 系统显式管理。
- 成员文件数：1；声明类型数：1；字段：8；属性：0；合计：8。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（8）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2409 | field | Terraria.WorldGen.TileMergeCullCache | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4020 | 3 | CullTop | bool | `public bool CullTop;` | `public bool CullTop;` |
| 2410 | field | Terraria.WorldGen.TileMergeCullCache | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4022 | 3 | CullBottom | bool | `public bool CullBottom;` | `public bool CullBottom;` |
| 2411 | field | Terraria.WorldGen.TileMergeCullCache | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4024 | 3 | CullLeft | bool | `public bool CullLeft;` | `public bool CullLeft;` |
| 2412 | field | Terraria.WorldGen.TileMergeCullCache | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4026 | 3 | CullRight | bool | `public bool CullRight;` | `public bool CullRight;` |
| 2413 | field | Terraria.WorldGen.TileMergeCullCache | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4028 | 3 | CullTopLeft | bool | `public bool CullTopLeft;` | `public bool CullTopLeft;` |
| 2414 | field | Terraria.WorldGen.TileMergeCullCache | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4030 | 3 | CullTopRight | bool | `public bool CullTopRight;` | `public bool CullTopRight;` |
| 2415 | field | Terraria.WorldGen.TileMergeCullCache | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4032 | 3 | CullBottomLeft | bool | `public bool CullBottomLeft;` | `public bool CullBottomLeft;` |
| 2416 | field | Terraria.WorldGen.TileMergeCullCache | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4034 | 3 | CullBottomRight | bool | `public bool CullBottomRight;` | `public bool CullBottomRight;` |

##### 属性（0）

无该类型成员记录。


#### 4.20.50 细分子系统：`WorldGenerationTileSetActions`

- 细分职责：Tile 设置、清除、形状和固体替换动作参数。
- 边界角色：`authoritative state/behavior`；最小 seam：Tile Change Command/CommitPort；Tile 变更统一排序提交。
- 成员文件数：1；声明类型数：7；字段：12；属性：0；合计：12。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（12）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2048 | field | Terraria.WorldBuilding.Actions.ClearTile | Terraria.WorldBuilding/Actions.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Actions.cs | 105 | 3 | _frameNeighbors | bool | `private bool _frameNeighbors;` | `private bool _frameNeighbors;` |
| 2050 | field | Terraria.WorldBuilding.Actions.HalfBlock | Terraria.WorldBuilding/Actions.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Actions.cs | 131 | 3 | _value | bool | `private bool _value;` | `private bool _value;` |
| 2051 | field | Terraria.WorldBuilding.Actions.SetTile | Terraria.WorldBuilding/Actions.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Actions.cs | 144 | 3 | _type | ushort | `private ushort _type;` | `private ushort _type;` |
| 2052 | field | Terraria.WorldBuilding.Actions.SetTile | Terraria.WorldBuilding/Actions.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Actions.cs | 146 | 3 | _doFraming | bool | `private bool _doFraming;` | `private bool _doFraming;` |
| 2053 | field | Terraria.WorldBuilding.Actions.SetTile | Terraria.WorldBuilding/Actions.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Actions.cs | 148 | 3 | _doNeighborFraming | bool | `private bool _doNeighborFraming;` | `private bool _doNeighborFraming;` |
| 2054 | field | Terraria.WorldBuilding.Actions.SetTile | Terraria.WorldBuilding/Actions.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Actions.cs | 150 | 3 | _clearTile | bool | `private bool _clearTile;` | `private bool _clearTile;` |
| 2059 | field | Terraria.WorldBuilding.Actions.SetTileKeepWall | Terraria.WorldBuilding/Actions.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Actions.cs | 188 | 3 | _type | ushort | `private ushort _type;` | `private ushort _type;` |
| 2060 | field | Terraria.WorldBuilding.Actions.SetTileKeepWall | Terraria.WorldBuilding/Actions.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Actions.cs | 190 | 3 | _doFraming | bool | `private bool _doFraming;` | `private bool _doFraming;` |
| 2061 | field | Terraria.WorldBuilding.Actions.SetTileKeepWall | Terraria.WorldBuilding/Actions.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Actions.cs | 192 | 3 | _doNeighborFraming | bool | `private bool _doNeighborFraming;` | `private bool _doNeighborFraming;` |
| 2065 | field | Terraria.WorldBuilding.Actions.SetSlope | Terraria.WorldBuilding/Actions.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Actions.cs | 236 | 3 | _slope | int | `private int _slope;` | `private int _slope;` |
| 2066 | field | Terraria.WorldBuilding.Actions.SetHalfTile | Terraria.WorldBuilding/Actions.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Actions.cs | 249 | 3 | _halfTile | bool | `private bool _halfTile;` | `private bool _halfTile;` |
| 2076 | field | Terraria.WorldBuilding.Actions.SwapSolidTile | Terraria.WorldBuilding/Actions.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Actions.cs | 379 | 3 | _type | ushort | `private ushort _type;` | `private ushort _type;` |

##### 属性（0）

无该类型成员记录。


#### 4.20.51 细分子系统：`WorldGenerationWallMutationActions`

- 细分职责：Wall 清除、设置和放置动作参数。
- 边界角色：`authoritative state/behavior`；最小 seam：Wall Change Command/CommitPort；Wall 变更显式提交。
- 成员文件数：1；声明类型数：3；字段：7；属性：0；合计：7。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（7）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2049 | field | Terraria.WorldBuilding.Actions.ClearWall | Terraria.WorldBuilding/Actions.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Actions.cs | 118 | 3 | _frameNeighbors | bool | `private bool _frameNeighbors;` | `private bool _frameNeighbors;` |
| 2055 | field | Terraria.WorldBuilding.Actions.SetWall | Terraria.WorldBuilding/Actions.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Actions.cs | 166 | 3 | _type | ushort | `private ushort _type;` | `private ushort _type;` |
| 2056 | field | Terraria.WorldBuilding.Actions.SetWall | Terraria.WorldBuilding/Actions.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Actions.cs | 168 | 3 | _doFraming | bool | `private bool _doFraming;` | `private bool _doFraming;` |
| 2057 | field | Terraria.WorldBuilding.Actions.SetWall | Terraria.WorldBuilding/Actions.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Actions.cs | 170 | 3 | _doNeighborFraming | bool | `private bool _doNeighborFraming;` | `private bool _doNeighborFraming;` |
| 2058 | field | Terraria.WorldBuilding.Actions.SetWall | Terraria.WorldBuilding/Actions.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Actions.cs | 172 | 3 | _clearTile | bool | `private bool _clearTile;` | `private bool _clearTile;` |
| 2072 | field | Terraria.WorldBuilding.Actions.PlaceWall | Terraria.WorldBuilding/Actions.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Actions.cs | 347 | 3 | _type | ushort | `private ushort _type;` | `private ushort _type;` |
| 2073 | field | Terraria.WorldBuilding.Actions.PlaceWall | Terraria.WorldBuilding/Actions.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Actions.cs | 349 | 3 | _neighbors | bool | `private bool _neighbors;` | `private bool _neighbors;` |

##### 属性（0）

无该类型成员记录。


#### 4.20.52 细分子系统：`WorldGenerationTilePlacementAndPaintActions`

- 细分职责：Tile 放置与 Tile/Wall 涂料动作参数。
- 边界角色：`authoritative state/behavior`；最小 seam：Tile Presentation Command/CommitPort；绘制与放置保持可追踪顺序。
- 成员文件数：1；声明类型数：4；字段：5；属性：0；合计：5。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（5）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2067 | field | Terraria.WorldBuilding.Actions.SetTilePaint | Terraria.WorldBuilding/Actions.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Actions.cs | 262 | 3 | paintID | byte | `private byte paintID;` | `private byte paintID;` |
| 2068 | field | Terraria.WorldBuilding.Actions.SetWallPaint | Terraria.WorldBuilding/Actions.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Actions.cs | 281 | 3 | paintID | byte | `private byte paintID;` | `private byte paintID;` |
| 2069 | field | Terraria.WorldBuilding.Actions.SetTileAndWallPaint | Terraria.WorldBuilding/Actions.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Actions.cs | 300 | 3 | paintID | byte | `private byte paintID;` | `private byte paintID;` |
| 2070 | field | Terraria.WorldBuilding.Actions.PlaceTile | Terraria.WorldBuilding/Actions.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Actions.cs | 325 | 3 | _type | ushort | `private ushort _type;` | `private ushort _type;` |
| 2071 | field | Terraria.WorldBuilding.Actions.PlaceTile | Terraria.WorldBuilding/Actions.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Actions.cs | 327 | 3 | _style | int | `private int _style;` | `private int _style;` |

##### 属性（0）

无该类型成员记录。


#### 4.20.53 细分子系统：`WorldGenerationLiquidAndNeighborActions`

- 细分职责：液体设置和邻接平滑动作参数。
- 边界角色：`authoritative state/behavior`；最小 seam：Liquid/Framing Command/CommitPort；邻接处理顺序显式维护。
- 成员文件数：1；声明类型数：2；字段：3；属性：0；合计：3。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（3）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2074 | field | Terraria.WorldBuilding.Actions.SetLiquid | Terraria.WorldBuilding/Actions.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Actions.cs | 363 | 3 | _type | int | `private int _type;` | `private int _type;` |
| 2075 | field | Terraria.WorldBuilding.Actions.SetLiquid | Terraria.WorldBuilding/Actions.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Actions.cs | 365 | 3 | _value | byte | `private byte _value;` | `private byte _value;` |
| 2078 | field | Terraria.WorldBuilding.Actions.Smooth | Terraria.WorldBuilding/Actions.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Actions.cs | 405 | 3 | _applyToNeighbors | bool | `private bool _applyToNeighbors;` | `private bool _applyToNeighbors;` |

##### 属性（0）

无该类型成员记录。


#### 4.20.54 细分子系统：`WorldGenerationTileScanAndControlActions`

- 细分职责：扫描、计数、自定义动作和执行边界控制状态。
- 边界角色：`derived/query`；最小 seam：纯查询或显式执行命令 seam；不直接持有世界权威状态。
- 成员文件数：1；声明类型数：6；字段：7；属性：0；合计：7。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（7）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2042 | field | Terraria.WorldBuilding.Actions.ContinueWrapper | Terraria.WorldBuilding/Actions.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Actions.cs | 14 | 3 | _action | Terraria.WorldBuilding.GenAction | `private GenAction _action;` | `private GenAction _action;` |
| 2043 | field | Terraria.WorldBuilding.Actions.Count | Terraria.WorldBuilding/Actions.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Actions.cs | 27 | 3 | _count | Terraria.Ref<int> | `private Ref<int> _count;` | `private Ref<int> _count;` |
| 2044 | field | Terraria.WorldBuilding.Actions.Scanner | Terraria.WorldBuilding/Actions.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Actions.cs | 40 | 3 | _count | Terraria.Ref<int> | `private Ref<int> _count;` | `private Ref<int> _count;` |
| 2045 | field | Terraria.WorldBuilding.Actions.TileScanner | Terraria.WorldBuilding/Actions.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Actions.cs | 53 | 3 | _tileIds | ushort[] | `private ushort[] _tileIds;` | `private ushort[] _tileIds;` |
| 2046 | field | Terraria.WorldBuilding.Actions.TileScanner | Terraria.WorldBuilding/Actions.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Actions.cs | 55 | 3 | _tileCounts | System.Collections.Generic.Dictionary<ushort, int> | `private Dictionary<ushort, int> _tileCounts;` | `private Dictionary<ushort, int> _tileCounts;` |
| 2047 | field | Terraria.WorldBuilding.Actions.Custom | Terraria.WorldBuilding/Actions.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Actions.cs | 80 | 3 | _perUnit | Terraria.WorldBuilding.GenBase.CustomPerUnitAction | `private CustomPerUnitAction _perUnit;` | `private CustomPerUnitAction _perUnit;` |
| 2062 | field | Terraria.WorldBuilding.Actions.UpdateBounds | Terraria.WorldBuilding/Actions.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Actions.cs | 207 | 3 | _bounds | Terraria.GameContent.Generation.Dungeon.DungeonBounds | `private DungeonBounds _bounds;` | `private DungeonBounds _bounds;` |

##### 属性（0）

无该类型成员记录。


#### 4.20.55 细分子系统：`WorldGenerationTileFramingAndDebugActions`

- 细分职责：Tile framing、调试绘制和表现辅助参数。
- 边界角色：`registry/projection`；最小 seam：Projection/diagnostics seam；调试输出不反向修改生成结果。
- 成员文件数：1；声明类型数：2；字段：3；属性：0；合计：3。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（3）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2063 | field | Terraria.WorldBuilding.Actions.DebugDraw | Terraria.WorldBuilding/Actions.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Actions.cs | 220 | 3 | _color | Color | `private Color _color;` | `private Color _color;` |
| 2064 | field | Terraria.WorldBuilding.Actions.DebugDraw | Terraria.WorldBuilding/Actions.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Actions.cs | 222 | 3 | _spriteBatch | SpriteBatch | `private SpriteBatch _spriteBatch;` | `private SpriteBatch _spriteBatch;` |
| 2077 | field | Terraria.WorldBuilding.Actions.SetFrames | Terraria.WorldBuilding/Actions.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Actions.cs | 392 | 3 | _frameNeighbors | bool | `private bool _frameNeighbors;` | `private bool _frameNeighbors;` |

##### 属性（0）

无该类型成员记录。


#### 4.20.56 细分子系统：`WorldGenerationConditionsAndSearches`

- 细分职责：世界生成条件、搜索方向和搜索约束。
- 边界角色：`derived/query`；最小 seam：只读快照/纯资格 Query；条件和搜索不持有跨阶段可变状态。
- 成员文件数：3；声明类型数：9；字段：11；属性：0；合计：11。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（11）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2081 | field | Terraria.WorldBuilding.Conditions.IsTile | Terraria.WorldBuilding/Conditions.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Conditions.cs | 7 | 3 | _types | ushort[] | `private ushort[] _types;` | `private ushort[] _types;` |
| 2082 | field | Terraria.WorldBuilding.Conditions.BoolCheck | Terraria.WorldBuilding/Conditions.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Conditions.cs | 26 | 3 | _theBool | bool | `private bool _theBool;` | `private bool _theBool;` |
| 2083 | field | Terraria.WorldBuilding.Conditions.InWorld | Terraria.WorldBuilding/Conditions.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Conditions.cs | 45 | 3 | _fluff | int | `private int _fluff;` | `private int _fluff;` |
| 2096 | field | Terraria.WorldBuilding.GenSearch | Terraria.WorldBuilding/GenSearch.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenSearch.cs | 7 | 2 | NOT_FOUND | Point | `public static Point NOT_FOUND = new Point(int.MaxValue, int.MaxValue);` | `public static Point NOT_FOUND = new Point(int.MaxValue, int.MaxValue);` |
| 2097 | field | Terraria.WorldBuilding.GenSearch | Terraria.WorldBuilding/GenSearch.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenSearch.cs | 9 | 2 | _conditions | Terraria.WorldBuilding.GenCondition[] | `private GenCondition[] _conditions;` | `private GenCondition[] _conditions;` |
| 2286 | field | Terraria.WorldBuilding.Searches.Left | Terraria.WorldBuilding/Searches.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Searches.cs | 9 | 3 | _maxDistance | int | `private int _maxDistance;` | `private int _maxDistance;` |
| 2287 | field | Terraria.WorldBuilding.Searches.Right | Terraria.WorldBuilding/Searches.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Searches.cs | 22 | 3 | _maxDistance | int | `private int _maxDistance;` | `private int _maxDistance;` |
| 2288 | field | Terraria.WorldBuilding.Searches.Down | Terraria.WorldBuilding/Searches.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Searches.cs | 35 | 3 | _maxDistance | int | `private int _maxDistance;` | `private int _maxDistance;` |
| 2289 | field | Terraria.WorldBuilding.Searches.Up | Terraria.WorldBuilding/Searches.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Searches.cs | 48 | 3 | _maxDistance | int | `private int _maxDistance;` | `private int _maxDistance;` |
| 2290 | field | Terraria.WorldBuilding.Searches.Rectangle | Terraria.WorldBuilding/Searches.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Searches.cs | 61 | 3 | _width | int | `private int _width;` | `private int _width;` |
| 2291 | field | Terraria.WorldBuilding.Searches.Rectangle | Terraria.WorldBuilding/Searches.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Searches.cs | 63 | 3 | _height | int | `private int _height;` | `private int _height;` |

##### 属性（0）

无该类型成员记录。


#### 4.20.57 细分子系统：`WorldGenerationShapeData`

- 细分职责：生成形状、ShapeData 和形状轮廓输入。
- 边界角色：`definition/query`；最小 seam：只读 Definition/Shape view；形状查询通过值对象交接。
- 成员文件数：5；声明类型数：6；字段：8；属性：2；合计：10。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（8）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2093 | field | Terraria.WorldBuilding.GenModShape | Terraria.WorldBuilding/GenModShape.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenModShape.cs | 5 | 2 | _data | Terraria.WorldBuilding.ShapeData | `protected ShapeData _data;` | `protected ShapeData _data;` |
| 2098 | field | Terraria.WorldBuilding.GenShape | Terraria.WorldBuilding/GenShape.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenShape.cs | 8 | 2 | _quitOnFail | bool | `protected bool _quitOnFail;` | `protected bool _quitOnFail;` |
| 2281 | field | Terraria.WorldBuilding.ModShapes.OuterOutline | Terraria.WorldBuilding/ModShapes.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\ModShapes.cs | 21 | 3 | POINT_OFFSETS | int[] | `private static readonly int[] POINT_OFFSETS = new int[16]  		{  			1, 0, -1, 0, 0, 1, 0, -1, 1, 1,  			1, -1, -1, 1, -1, -1  		};` | `private static readonly int[] POINT_OFFSETS = new int[16] { 1, 0, -1, 0, 0, 1, 0, -1, 1, 1, 1, -1, -1, 1, -1, -1 };` |
| 2282 | field | Terraria.WorldBuilding.ModShapes.OuterOutline | Terraria.WorldBuilding/ModShapes.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\ModShapes.cs | 27 | 3 | _useDiagonals | bool | `private bool _useDiagonals;` | `private bool _useDiagonals;` |
| 2283 | field | Terraria.WorldBuilding.ModShapes.OuterOutline | Terraria.WorldBuilding/ModShapes.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\ModShapes.cs | 29 | 3 | _useInterior | bool | `private bool _useInterior;` | `private bool _useInterior;` |
| 2284 | field | Terraria.WorldBuilding.ModShapes.InnerOutline | Terraria.WorldBuilding/ModShapes.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\ModShapes.cs | 44 | 3 | POINT_OFFSETS | int[] | `private static readonly int[] POINT_OFFSETS = new int[16]  		{  			1, 0, -1, 0, 0, 1, 0, -1, 1, 1,  			1, -1, -1, 1, -1, -1  		};` | `private static readonly int[] POINT_OFFSETS = new int[16] { 1, 0, -1, 0, 0, 1, 0, -1, 1, 1, 1, -1, -1, 1, -1, -1 };` |
| 2285 | field | Terraria.WorldBuilding.ModShapes.InnerOutline | Terraria.WorldBuilding/ModShapes.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\ModShapes.cs | 50 | 3 | _useDiagonals | bool | `private bool _useDiagonals;` | `private bool _useDiagonals;` |
| 2292 | field | Terraria.WorldBuilding.ShapeData | Terraria.WorldBuilding/ShapeData.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\ShapeData.cs | 11 | 2 | _points | System.Collections.Generic.HashSet<Terraria.DataStructures.Point16> | `private HashSet<Point16> _points;` | `private HashSet<Point16> _points;` |

##### 属性（2）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2571 | property | Terraria.WorldBuilding.GenBase | Terraria.WorldBuilding/GenBase.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenBase.cs | 9 | 2 | _tiles | Terraria.Tile[,] | `protected static Tile[,] _tiles => Main.tile;` | `protected static Tile[,] _tiles => Main.tile;` |
| 2585 | property | Terraria.WorldBuilding.ShapeData | Terraria.WorldBuilding/ShapeData.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\ShapeData.cs | 13 | 2 | Count | int | `public int Count => _points.Count;` | `public int Count => _points.Count;` |


#### 4.20.58 细分子系统：`WorldGenerationShapeModifierState`

- 细分职责：几何形状、膨胀、翻转、抖动和形状遮罩 Modifier 参数。
- 边界角色：`definition/query`；最小 seam：只读 Definition/Shape view；执行由 WorldGen System 控制。
- 成员文件数：1；声明类型数：11；字段：22；属性：0；合计：22。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（22）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2239 | field | Terraria.WorldBuilding.Modifiers.ShapeScale | Terraria.WorldBuilding/Modifiers.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Modifiers.cs | 11 | 3 | _scale | int | `private int _scale;` | `private int _scale;` |
| 2240 | field | Terraria.WorldBuilding.Modifiers.Expand | Terraria.WorldBuilding/Modifiers.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Modifiers.cs | 24 | 3 | _xExpansion | int | `private int _xExpansion;` | `private int _xExpansion;` |
| 2241 | field | Terraria.WorldBuilding.Modifiers.Expand | Terraria.WorldBuilding/Modifiers.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Modifiers.cs | 26 | 3 | _yExpansion | int | `private int _yExpansion;` | `private int _yExpansion;` |
| 2242 | field | Terraria.WorldBuilding.Modifiers.RadialDither | Terraria.WorldBuilding/Modifiers.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Modifiers.cs | 46 | 3 | _innerRadius | double | `private double _innerRadius;` | `private double _innerRadius;` |
| 2243 | field | Terraria.WorldBuilding.Modifiers.RadialDither | Terraria.WorldBuilding/Modifiers.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Modifiers.cs | 48 | 3 | _outerRadius | double | `private double _outerRadius;` | `private double _outerRadius;` |
| 2244 | field | Terraria.WorldBuilding.Modifiers.Blotches | Terraria.WorldBuilding/Modifiers.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Modifiers.cs | 62 | 3 | _minX | int | `private int _minX;` | `private int _minX;` |
| 2245 | field | Terraria.WorldBuilding.Modifiers.Blotches | Terraria.WorldBuilding/Modifiers.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Modifiers.cs | 64 | 3 | _minY | int | `private int _minY;` | `private int _minY;` |
| 2246 | field | Terraria.WorldBuilding.Modifiers.Blotches | Terraria.WorldBuilding/Modifiers.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Modifiers.cs | 66 | 3 | _maxX | int | `private int _maxX;` | `private int _maxX;` |
| 2247 | field | Terraria.WorldBuilding.Modifiers.Blotches | Terraria.WorldBuilding/Modifiers.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Modifiers.cs | 68 | 3 | _maxY | int | `private int _maxY;` | `private int _maxY;` |
| 2248 | field | Terraria.WorldBuilding.Modifiers.Blotches | Terraria.WorldBuilding/Modifiers.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Modifiers.cs | 70 | 3 | _chance | double | `private double _chance;` | `private double _chance;` |
| 2249 | field | Terraria.WorldBuilding.Modifiers.InShape | Terraria.WorldBuilding/Modifiers.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Modifiers.cs | 105 | 3 | _shapeData | Terraria.WorldBuilding.ShapeData | `private readonly ShapeData _shapeData;` | `private readonly ShapeData _shapeData;` |
| 2250 | field | Terraria.WorldBuilding.Modifiers.NotInShape | Terraria.WorldBuilding/Modifiers.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Modifiers.cs | 118 | 3 | _shapeData | Terraria.WorldBuilding.ShapeData | `private readonly ShapeData _shapeData;` | `private readonly ShapeData _shapeData;` |
| 2254 | field | Terraria.WorldBuilding.Modifiers.Checkerboard | Terraria.WorldBuilding/Modifiers.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Modifiers.cs | 170 | 3 | _percentile | int | `private int _percentile;` | `private int _percentile;` |
| 2272 | field | Terraria.WorldBuilding.Modifiers.RectangleMask | Terraria.WorldBuilding/Modifiers.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Modifiers.cs | 357 | 3 | _xMin | int | `private int _xMin;` | `private int _xMin;` |
| 2273 | field | Terraria.WorldBuilding.Modifiers.RectangleMask | Terraria.WorldBuilding/Modifiers.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Modifiers.cs | 359 | 3 | _yMin | int | `private int _yMin;` | `private int _yMin;` |
| 2274 | field | Terraria.WorldBuilding.Modifiers.RectangleMask | Terraria.WorldBuilding/Modifiers.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Modifiers.cs | 361 | 3 | _xMax | int | `private int _xMax;` | `private int _xMax;` |
| 2275 | field | Terraria.WorldBuilding.Modifiers.RectangleMask | Terraria.WorldBuilding/Modifiers.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Modifiers.cs | 363 | 3 | _yMax | int | `private int _yMax;` | `private int _yMax;` |
| 2276 | field | Terraria.WorldBuilding.Modifiers.Offset | Terraria.WorldBuilding/Modifiers.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Modifiers.cs | 379 | 3 | _xOffset | int | `private int _xOffset;` | `private int _xOffset;` |
| 2277 | field | Terraria.WorldBuilding.Modifiers.Offset | Terraria.WorldBuilding/Modifiers.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Modifiers.cs | 381 | 3 | _yOffset | int | `private int _yOffset;` | `private int _yOffset;` |
| 2278 | field | Terraria.WorldBuilding.Modifiers.Dither | Terraria.WorldBuilding/Modifiers.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Modifiers.cs | 395 | 3 | _failureChance | double | `private double _failureChance;` | `private double _failureChance;` |
| 2279 | field | Terraria.WorldBuilding.Modifiers.Flip | Terraria.WorldBuilding/Modifiers.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Modifiers.cs | 408 | 3 | _flipX | bool | `private bool _flipX;` | `private bool _flipX;` |
| 2280 | field | Terraria.WorldBuilding.Modifiers.Flip | Terraria.WorldBuilding/Modifiers.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Modifiers.cs | 410 | 3 | _flipY | bool | `private bool _flipY;` | `private bool _flipY;` |

##### 属性（0）

无该类型成员记录。


#### 4.20.59 细分子系统：`WorldGenerationTileWallConditionState`

- 细分职责：Tile、Wall、液体、高度和接触条件 Modifier 参数。
- 边界角色：`derived/query`；最小 seam：纯资格 Query；条件不持有跨阶段可变状态。
- 成员文件数：1；声明类型数：12；字段：20；属性：0；合计：20。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（20）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2251 | field | Terraria.WorldBuilding.Modifiers.Conditions | Terraria.WorldBuilding/Modifiers.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Modifiers.cs | 131 | 3 | _conditions | Terraria.WorldBuilding.GenCondition[] | `private readonly GenCondition[] _conditions;` | `private readonly GenCondition[] _conditions;` |
| 2252 | field | Terraria.WorldBuilding.Modifiers.OnlyWalls | Terraria.WorldBuilding/Modifiers.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Modifiers.cs | 144 | 3 | _types | ushort[] | `private ushort[] _types;` | `private ushort[] _types;` |
| 2253 | field | Terraria.WorldBuilding.Modifiers.OnlyTiles | Terraria.WorldBuilding/Modifiers.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Modifiers.cs | 157 | 3 | _types | ushort[] | `private ushort[] _types;` | `private ushort[] _types;` |
| 2255 | field | Terraria.WorldBuilding.Modifiers.IsTouching | Terraria.WorldBuilding/Modifiers.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Modifiers.cs | 183 | 3 | DIRECTIONS | int[] | `private static readonly int[] DIRECTIONS = new int[16]  		{  			0, -1, 1, 0, -1, 0, 0, 1, -1, -1,  			1, -1, -1, 1, 1, 1  		};` | `private static readonly int[] DIRECTIONS = new int[16] { 0, -1, 1, 0, -1, 0, 0, 1, -1, -1, 1, -1, -1, 1, 1, 1 };` |
| 2256 | field | Terraria.WorldBuilding.Modifiers.IsTouching | Terraria.WorldBuilding/Modifiers.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Modifiers.cs | 189 | 3 | _useDiagonals | bool | `private bool _useDiagonals;` | `private bool _useDiagonals;` |
| 2257 | field | Terraria.WorldBuilding.Modifiers.IsTouching | Terraria.WorldBuilding/Modifiers.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Modifiers.cs | 191 | 3 | _tileIds | ushort[] | `private ushort[] _tileIds;` | `private ushort[] _tileIds;` |
| 2258 | field | Terraria.WorldBuilding.Modifiers.NotTouching | Terraria.WorldBuilding/Modifiers.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Modifiers.cs | 205 | 3 | DIRECTIONS | int[] | `private static readonly int[] DIRECTIONS = new int[16]  		{  			0, -1, 1, 0, -1, 0, 0, 1, -1, -1,  			1, -1, -1, 1, 1, 1  		};` | `private static readonly int[] DIRECTIONS = new int[16] { 0, -1, 1, 0, -1, 0, 0, 1, -1, -1, 1, -1, -1, 1, 1, 1 };` |
| 2259 | field | Terraria.WorldBuilding.Modifiers.NotTouching | Terraria.WorldBuilding/Modifiers.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Modifiers.cs | 211 | 3 | _useDiagonals | bool | `private bool _useDiagonals;` | `private bool _useDiagonals;` |
| 2260 | field | Terraria.WorldBuilding.Modifiers.NotTouching | Terraria.WorldBuilding/Modifiers.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Modifiers.cs | 213 | 3 | _tileIds | ushort[] | `private ushort[] _tileIds;` | `private ushort[] _tileIds;` |
| 2261 | field | Terraria.WorldBuilding.Modifiers.IsTouchingAir | Terraria.WorldBuilding/Modifiers.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Modifiers.cs | 227 | 3 | DIRECTIONS | int[] | `private static readonly int[] DIRECTIONS = new int[16]  		{  			0, -1, 1, 0, -1, 0, 0, 1, -1, -1,  			1, -1, -1, 1, 1, 1  		};` | `private static readonly int[] DIRECTIONS = new int[16] { 0, -1, 1, 0, -1, 0, 0, 1, -1, -1, 1, -1, -1, 1, 1, 1 };` |
| 2262 | field | Terraria.WorldBuilding.Modifiers.IsTouchingAir | Terraria.WorldBuilding/Modifiers.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Modifiers.cs | 233 | 3 | _useDiagonals | bool | `private bool _useDiagonals;` | `private bool _useDiagonals;` |
| 2263 | field | Terraria.WorldBuilding.Modifiers.SkipTiles | Terraria.WorldBuilding/Modifiers.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Modifiers.cs | 246 | 3 | _types | ushort[] | `private ushort[] _types;` | `private ushort[] _types;` |
| 2264 | field | Terraria.WorldBuilding.Modifiers.HasLiquid | Terraria.WorldBuilding/Modifiers.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Modifiers.cs | 259 | 3 | _liquidType | int | `private int _liquidType;` | `private int _liquidType;` |
| 2265 | field | Terraria.WorldBuilding.Modifiers.HasLiquid | Terraria.WorldBuilding/Modifiers.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Modifiers.cs | 261 | 3 | _liquidLevel | int | `private int _liquidLevel;` | `private int _liquidLevel;` |
| 2266 | field | Terraria.WorldBuilding.Modifiers.NoLiquid | Terraria.WorldBuilding/Modifiers.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Modifiers.cs | 275 | 3 | _liquidType | int | `private int _liquidType;` | `private int _liquidType;` |
| 2267 | field | Terraria.WorldBuilding.Modifiers.SkipWalls | Terraria.WorldBuilding/Modifiers.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Modifiers.cs | 288 | 3 | _types | ushort[] | `private ushort[] _types;` | `private ushort[] _types;` |
| 2268 | field | Terraria.WorldBuilding.Modifiers.IsAboveHeight | Terraria.WorldBuilding/Modifiers.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Modifiers.cs | 307 | 3 | _y | int | `private int _y;` | `private int _y;` |
| 2269 | field | Terraria.WorldBuilding.Modifiers.IsAboveHeight | Terraria.WorldBuilding/Modifiers.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Modifiers.cs | 309 | 3 | _inclusive | bool | `private bool _inclusive;` | `private bool _inclusive;` |
| 2270 | field | Terraria.WorldBuilding.Modifiers.IsBelowHeight | Terraria.WorldBuilding/Modifiers.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Modifiers.cs | 323 | 3 | _y | int | `private int _y;` | `private int _y;` |
| 2271 | field | Terraria.WorldBuilding.Modifiers.IsBelowHeight | Terraria.WorldBuilding/Modifiers.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\Modifiers.cs | 325 | 3 | _inclusive | bool | `private bool _inclusive;` | `private bool _inclusive;` |

##### 属性（0）

无该类型成员记录。


#### 4.20.60 细分子系统：`WorldStructurePlanningAndMasks`

- 细分职责：结构注册、地形遮罩、范围和 Dungeon 侧信息。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System 读取输入并通过显式 Command/CommitPort 写入；跨组只用事件、Query 或命令交接。
- 成员文件数：3；声明类型数：3；字段：9；属性：2；合计：11。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（9）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2084 | field | Terraria.WorldBuilding.DungeonSide | Terraria.WorldBuilding/DungeonSide.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\DungeonSide.cs | 5 | 2 | Left | short | `public static short Left = -1;` | `public static short Left = -1;` |
| 2085 | field | Terraria.WorldBuilding.DungeonSide | Terraria.WorldBuilding/DungeonSide.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\DungeonSide.cs | 7 | 2 | Right | short | `public static short Right = 1;` | `public static short Right = 1;` |
| 2293 | field | Terraria.WorldBuilding.StructureMap | Terraria.WorldBuilding/StructureMap.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\StructureMap.cs | 11 | 2 | _structures | System.Collections.Generic.List<Rectangle> | `[JsonProperty] private readonly List<Rectangle> _structures = new List<Rectangle>(2048);` | `[JsonProperty] private readonly List<Rectangle> _structures = new List<Rectangle>(2048);` |
| 2294 | field | Terraria.WorldBuilding.StructureMap | Terraria.WorldBuilding/StructureMap.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\StructureMap.cs | 14 | 2 | _protectedStructures | System.Collections.Generic.List<Rectangle> | `[JsonProperty] private readonly List<Rectangle> _protectedStructures = new List<Rectangle>(2048);` | `[JsonProperty] private readonly List<Rectangle> _protectedStructures = new List<Rectangle>(2048);` |
| 2295 | field | Terraria.WorldBuilding.StructureMap | Terraria.WorldBuilding/StructureMap.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\StructureMap.cs | 17 | 2 | _lock | object | `private readonly object _lock = new object();` | `private readonly object _lock = new object();` |
| 2316 | field | Terraria.WorldBuilding.WorldGenRange | Terraria.WorldBuilding/WorldGenRange.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldGenRange.cs | 16 | 2 | Empty | Terraria.WorldBuilding.WorldGenRange | `public static readonly WorldGenRange Empty = new WorldGenRange(0, 0);` | `public static readonly WorldGenRange Empty = new WorldGenRange(0, 0);` |
| 2317 | field | Terraria.WorldBuilding.WorldGenRange | Terraria.WorldBuilding/WorldGenRange.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldGenRange.cs | 18 | 2 | Minimum | int | `[JsonProperty("Min")] public readonly int Minimum;` | `[JsonProperty("Min")] public readonly int Minimum;` |
| 2318 | field | Terraria.WorldBuilding.WorldGenRange | Terraria.WorldBuilding/WorldGenRange.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldGenRange.cs | 21 | 2 | Maximum | int | `[JsonProperty("Max")] public readonly int Maximum;` | `[JsonProperty("Max")] public readonly int Maximum;` |
| 2319 | field | Terraria.WorldBuilding.WorldGenRange | Terraria.WorldBuilding/WorldGenRange.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldGenRange.cs | 24 | 2 | ScaleWith | Terraria.WorldBuilding.WorldGenRange.ScalingMode | `[JsonProperty] [JsonConverter(typeof(StringEnumConverter))] public readonly ScalingMode ScaleWith;` | `[JsonProperty] [JsonConverter(typeof(StringEnumConverter))] public readonly ScalingMode ScaleWith;` |

##### 属性（2）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2597 | property | Terraria.WorldBuilding.WorldGenRange | Terraria.WorldBuilding/WorldGenRange.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldGenRange.cs | 28 | 2 | ScaledMinimum | int | `public int ScaledMinimum => ScaleValue(Minimum);` | `public int ScaledMinimum => ScaleValue(Minimum);` |
| 2598 | property | Terraria.WorldBuilding.WorldGenRange | Terraria.WorldBuilding/WorldGenRange.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldGenRange.cs | 30 | 2 | ScaledMaximum | int | `public int ScaledMaximum => ScaleValue(Maximum);` | `public int ScaledMaximum => ScaleValue(Maximum);` |


## 8. 本分区自检

- 叶子子系统：12 个；指定组均已写入且没有重复分区。
- 字段/属性/成员：115 / 4 / 119。
- 来源序号范围：2042..2598；本分区内唯一序号：True。
- 成员声明表逐组从源报告提取，未改写来源序号、类型、路径、行列、成员、C# 类型、成员声明或原始声明。
- 运行时读写者、生命周期、持久化、网络和调度：未验证，保留为迁移前风险。
