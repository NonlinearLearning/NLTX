# P19 世界生成执行、Controller、快照与选项注册 - 权威模拟系统成员组件候选分区

> 来源：`docs/migration/ledgers/Version4权威模拟系统字段属性逐成员源码声明-更细子系统拆分-去除ID类文件.md`；本文件只做成员库存分区和候选组件边界记录。
> 这是候选组件拆分台账，不是运行时迁移完成、行为等价、API 兼容、网络闭合或持久化闭合证明。

## 1. 分区边界

本分区包含 10 个叶子子系统，字段 47 条、属性 37 条、成员合计 84 条；每个叶子子系统保持原正式父级归属，不跨分区拆行。

## 2. 分区覆盖概览

| 叶子子系统 | 正式父级 | 字段 | 属性 | 合计 | 源边界角色 |
|---|---|---:|---:|---:|---|
| `WorldGenerationExecutionState` | `WorldGenerationAndEcology` | 6 | 0 | 6 | authoritative state/behavior |
| `WorldGenerationProgressAndPassState` | `WorldGenerationAndEcology` | 7 | 6 | 13 | authoritative state/behavior |
| `WorldGenerationControllerPassState` | `WorldGenerationAndEcology` | 4 | 3 | 7 | authoritative state/behavior |
| `WorldGenerationControllerPauseAndHashState` | `WorldGenerationAndEcology` | 1 | 6 | 7 | authoritative state/behavior |
| `WorldGenerationGeneratorExecutionState` | `WorldGenerationAndEcology` | 10 | 1 | 11 | authoritative state/behavior |
| `WorldGenerationSnapshotState` | `WorldGenerationAndEcology` | 7 | 6 | 13 | registry/projection |
| `WorldGenerationManifestAndPassResults` | `WorldGenerationAndEcology` | 2 | 6 | 8 | registry/projection |
| `WorldGenerationOptionBaseState` | `WorldGenerationAndEcology` | 4 | 8 | 12 | definition/query |
| `WorldGenerationOptionRegistry` | `WorldGenerationAndEcology` | 2 | 1 | 3 | registry/projection |
| `WorldGenerationSupportTypes` | `WorldGenerationAndEcology` | 4 | 0 | 4 | definition/query |

## 3. 候选组件归属与 seam

| 叶子子系统 | 候选组件边界 | 类型分类 | 最小 seam | 独立生命周期 | 证据状态 | 主要风险 |
|---|---|---|---|---|---|---|
| `WorldGenerationExecutionState` | `WorldGenerationExecutionStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `WorldGenerationProgressAndPassState` | `WorldGenerationProgressAndPassStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `WorldGenerationControllerPassState` | `WorldGenerationControllerPassStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `WorldGenerationControllerPauseAndHashState` | `WorldGenerationControllerPauseAndHashStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `WorldGenerationGeneratorExecutionState` | `WorldGenerationGeneratorExecutionStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `WorldGenerationSnapshotState` | `WorldGenerationSnapshotStateProjection`/`Adapter` | Projection/Adapter | 单向 Projection/Adapter seam；不得回写权威状态 | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `WorldGenerationManifestAndPassResults` | `WorldGenerationManifestAndPassResultsProjection`/`Adapter` | Projection/Adapter | 单向 Projection/Adapter seam；不得回写权威状态 | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `WorldGenerationOptionBaseState` | `WorldGenerationOptionBaseStateDefinition`/`Query` | Definition/Query | 只读 Definition/Catalog view | 候选纯读边界；仍需核对失效/缓存条件 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `WorldGenerationOptionRegistry` | `WorldGenerationOptionRegistryProjection`/`Adapter` | Projection/Adapter | 单向 Projection/Adapter seam；不得回写权威状态 | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `WorldGenerationSupportTypes` | `WorldGenerationSupportTypesDefinition`/`Query` | Definition/Query | 只读 Definition/Catalog view | 候选纯读边界；仍需核对失效/缓存条件 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |

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
- 本分区父级局部统计：字段 47；属性 37；合计 84；完整父级统计以源报告为准。

#### 4.20.11 细分子系统：`WorldGenerationExecutionState`

- 细分职责：生成线程、生成器实例、连续地形统计和陷阱放置阶段状态。
- 边界角色：`authoritative state/behavior`；最小 seam：WorldGen System/CommitPort；生成调度阶段唯一写入。
- 成员文件数：1；声明类型数：1；字段：6；属性：0；合计：6。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（6）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2523 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4287 | 2 | generatingWorld | bool | `public static bool generatingWorld = false;` | `public static bool generatingWorld = false;` |
| 2524 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4289 | 2 | generatingWorldOnThisThread | bool | `[ThreadStatic] public static bool generatingWorldOnThisThread;` | `[ThreadStatic] public static bool generatingWorldOnThisThread;` |
| 2529 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4300 | 2 | _generator | Terraria.WorldBuilding.WorldGenerator | `private static WorldGenerator _generator;` | `private static WorldGenerator _generator;` |
| 2530 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4302 | 2 | SmallConsecutivesFound | int | `public static int SmallConsecutivesFound = 0;` | `public static int SmallConsecutivesFound = 0;` |
| 2531 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4304 | 2 | SmallConsecutivesEliminated | int | `public static int SmallConsecutivesEliminated = 0;` | `public static int SmallConsecutivesEliminated = 0;` |
| 2542 | field | Terraria.WorldGen | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4326 | 2 | placingTraps | bool | `public static bool placingTraps = false;` | `public static bool placingTraps = false;` |

##### 属性（0）

无该类型成员记录。


#### 4.20.38 细分子系统：`WorldGenerationProgressAndPassState`

- 细分职责：生成进度、Pass 定义和当前 Pass 权重状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；生成阶段按显式 Pass 调度更新。
- 成员文件数：2；声明类型数：2；字段：7；属性：6；合计：13。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（7）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2088 | field | Terraria.WorldBuilding.GenerationProgress | Terraria.WorldBuilding/GenerationProgress.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenerationProgress.cs | 5 | 2 | _message | string | `private string _message = "";` | `private string _message = "";` |
| 2089 | field | Terraria.WorldBuilding.GenerationProgress | Terraria.WorldBuilding/GenerationProgress.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenerationProgress.cs | 7 | 2 | _value | double | `private double _value;` | `private double _value;` |
| 2090 | field | Terraria.WorldBuilding.GenerationProgress | Terraria.WorldBuilding/GenerationProgress.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenerationProgress.cs | 9 | 2 | _totalWeightedProgress | double | `private double _totalWeightedProgress;` | `private double _totalWeightedProgress;` |
| 2091 | field | Terraria.WorldBuilding.GenerationProgress | Terraria.WorldBuilding/GenerationProgress.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenerationProgress.cs | 11 | 2 | TotalWeight | double | `public double TotalWeight;` | `public double TotalWeight;` |
| 2092 | field | Terraria.WorldBuilding.GenerationProgress | Terraria.WorldBuilding/GenerationProgress.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenerationProgress.cs | 13 | 2 | CurrentPassWeight | double | `public double CurrentPassWeight = 1.0;` | `public double CurrentPassWeight = 1.0;` |
| 2094 | field | Terraria.WorldBuilding.GenPass | Terraria.WorldBuilding/GenPass.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenPass.cs | 7 | 2 | Name | string | `public string Name;` | `public string Name;` |
| 2095 | field | Terraria.WorldBuilding.GenPass | Terraria.WorldBuilding/GenPass.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenPass.cs | 9 | 2 | Weight | double | `public double Weight;` | `public double Weight;` |

##### 属性（6）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2572 | property | Terraria.WorldBuilding.GenerationProgress | Terraria.WorldBuilding/GenerationProgress.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenerationProgress.cs | 15 | 2 | Message | string | `public string Message { get { return string.Format(_message, Value); } set { _message = value.Replace("%", "{0:0.0%}"); } }` | `public string Message { get { return string.Format(_message, Value); } set { _message = value.Replace("%", "{0:0.0%}"); } }` |
| 2573 | property | Terraria.WorldBuilding.GenerationProgress | Terraria.WorldBuilding/GenerationProgress.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenerationProgress.cs | 27 | 2 | MessageNoFormatting | string | `public string MessageNoFormatting { get { return _message; } set { _message = value; } }` | `public string MessageNoFormatting { get { return _message; } set { _message = value; } }` |
| 2574 | property | Terraria.WorldBuilding.GenerationProgress | Terraria.WorldBuilding/GenerationProgress.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenerationProgress.cs | 39 | 2 | Value | double | `public double Value { get { return _value; } set { _value = Utils.Clamp(value, 0.0, 1.0); } }` | `public double Value { get { return _value; } set { _value = Utils.Clamp(value, 0.0, 1.0); } }` |
| 2575 | property | Terraria.WorldBuilding.GenerationProgress | Terraria.WorldBuilding/GenerationProgress.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenerationProgress.cs | 51 | 2 | TotalWeightedProgress | double | `public double TotalWeightedProgress { set { _totalWeightedProgress = value; } }` | `public double TotalWeightedProgress { set { _totalWeightedProgress = value; } }` |
| 2576 | property | Terraria.WorldBuilding.GenerationProgress | Terraria.WorldBuilding/GenerationProgress.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenerationProgress.cs | 59 | 2 | TotalProgress | double | `public double TotalProgress { get { if (TotalWeight == 0.0) { return 0.0; } return (Value * CurrentPassWeight + _totalWeightedProgress) / TotalWeight; } }` | `public double TotalProgress { get { if (TotalWeight == 0.0) { return 0.0; } return (Value * CurrentPassWeight + _totalWeightedProgress) / TotalWeight; } }` |
| 2577 | property | Terraria.WorldBuilding.GenPass | Terraria.WorldBuilding/GenPass.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenPass.cs | 11 | 2 | Enabled | bool | `public bool Enabled { get; private set; }` | `public bool Enabled { get; private set; }` |


#### 4.20.39 细分子系统：`WorldGenerationControllerPassState`

- 细分职责：生成 pass、当前 pass 和已完成 pass 的控制投影。
- 边界角色：`authoritative state/behavior`；最小 seam：WorldGen Controller/PassPort；pass 生命周期集中维护。
- 成员文件数：1；声明类型数：1；字段：4；属性：3；合计：7。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（4）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2301 | field | Terraria.WorldBuilding.WorldGenerator.Controller | Terraria.WorldBuilding/WorldGenerator.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldGenerator.cs | 26 | 3 | _previousManifest | Terraria.WorldBuilding.WorldManifest | `private WorldManifest _previousManifest;` | `private WorldManifest _previousManifest;` |
| 2302 | field | Terraria.WorldBuilding.WorldGenerator.Controller | Terraria.WorldBuilding/WorldGenerator.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldGenerator.cs | 28 | 3 | _snapshots | System.Collections.Generic.Dictionary<Terraria.WorldBuilding.GenPass, Terraria.WorldBuilding.WorldGenSnapshot> | `private Dictionary<GenPass, WorldGenSnapshot> _snapshots;` | `private Dictionary<GenPass, WorldGenSnapshot> _snapshots;` |
| 2303 | field | Terraria.WorldBuilding.WorldGenerator.Controller | Terraria.WorldBuilding/WorldGenerator.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldGenerator.cs | 30 | 3 | OnPassesLoaded | System.Action<Terraria.WorldBuilding.WorldGenerator.Controller> | `public Action<Controller> OnPassesLoaded;` | `public Action<Controller> OnPassesLoaded;` |
| 2304 | field | Terraria.WorldBuilding.WorldGenerator.Controller | Terraria.WorldBuilding/WorldGenerator.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldGenerator.cs | 32 | 3 | _generator | Terraria.WorldBuilding.WorldGenerator | `private WorldGenerator _generator;` | `private WorldGenerator _generator;` |

##### 属性（3）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2587 | property | Terraria.WorldBuilding.WorldGenerator.Controller | Terraria.WorldBuilding/WorldGenerator.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldGenerator.cs | 36 | 3 | Passes | System.Collections.Generic.List<Terraria.WorldBuilding.GenPass> | `public List<GenPass> Passes => _generator._passes;` | `public List<GenPass> Passes => _generator._passes;` |
| 2588 | property | Terraria.WorldBuilding.WorldGenerator.Controller | Terraria.WorldBuilding/WorldGenerator.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldGenerator.cs | 38 | 3 | CurrentPass | Terraria.WorldBuilding.GenPass | `public GenPass CurrentPass => _generator._currentPass;` | `public GenPass CurrentPass => _generator._currentPass;` |
| 2589 | property | Terraria.WorldBuilding.WorldGenerator.Controller | Terraria.WorldBuilding/WorldGenerator.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldGenerator.cs | 40 | 3 | LastCompletedPass | Terraria.WorldBuilding.GenPass | `public GenPass LastCompletedPass { get { if (PassResults.Count != 0) { return Passes[PassResults.Count - 1]; } return null; } }` | `public GenPass LastCompletedPass { get { if (PassResults.Count != 0) { return Passes[PassResults.Count - 1]; } return null; } }` |


#### 4.20.40 细分子系统：`WorldGenerationControllerPauseAndHashState`

- 细分职责：暂停、哈希不一致和中止控制状态。
- 边界角色：`authoritative state/behavior`；最小 seam：WorldGen Controller/ControlPort；控制命令通过锁定边界提交。
- 成员文件数：1；声明类型数：1；字段：1；属性：6；合计：7。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（1）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2305 | field | Terraria.WorldBuilding.WorldGenerator.Controller | Terraria.WorldBuilding/WorldGenerator.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldGenerator.cs | 34 | 3 | _paused | bool | `private bool _paused;` | `private bool _paused;` |

##### 属性（6）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2590 | property | Terraria.WorldBuilding.WorldGenerator.Controller | Terraria.WorldBuilding/WorldGenerator.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldGenerator.cs | 52 | 3 | PauseAfterPass | Terraria.WorldBuilding.GenPass | `public GenPass PauseAfterPass { get; set; }` | `public GenPass PauseAfterPass { get; set; }` |
| 2591 | property | Terraria.WorldBuilding.WorldGenerator.Controller | Terraria.WorldBuilding/WorldGenerator.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldGenerator.cs | 54 | 3 | PauseOnHashMismatch | bool | `public bool PauseOnHashMismatch { get; set; }` | `public bool PauseOnHashMismatch { get; set; }` |
| 2592 | property | Terraria.WorldBuilding.WorldGenerator.Controller | Terraria.WorldBuilding/WorldGenerator.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldGenerator.cs | 56 | 3 | PausedDueToHashMismatch | bool | `public bool PausedDueToHashMismatch { get; set; }` | `public bool PausedDueToHashMismatch { get; set; }` |
| 2593 | property | Terraria.WorldBuilding.WorldGenerator.Controller | Terraria.WorldBuilding/WorldGenerator.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldGenerator.cs | 58 | 3 | SnapshotFrequency | Terraria.WorldBuilding.WorldGenerator.SnapshotFrequency | `public SnapshotFrequency SnapshotFrequency { get; set; }` | `public SnapshotFrequency SnapshotFrequency { get; set; }` |
| 2594 | property | Terraria.WorldBuilding.WorldGenerator.Controller | Terraria.WorldBuilding/WorldGenerator.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldGenerator.cs | 60 | 3 | Paused | bool | `public bool Paused { get { return _paused; } set { _paused = value; if (value) { PauseAfterPass = null; } else { PausedDueToHashMismatch = false; } } }` | `public bool Paused { get { return _paused; } set { _paused = value; if (value) { PauseAfterPass = null; } else { PausedDueToHashMismatch = false; } } }` |
| 2595 | property | Terraria.WorldBuilding.WorldGenerator.Controller | Terraria.WorldBuilding/WorldGenerator.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldGenerator.cs | 80 | 3 | QueuedAbort | bool | `public bool QueuedAbort { get; set; }` | `public bool QueuedAbort { get; set; }` |


#### 4.20.41 细分子系统：`WorldGenerationGeneratorExecutionState`

- 细分职责：生成器配置、进度、锁、种子和结果执行状态。
- 边界角色：`authoritative state/behavior`；最小 seam：WorldGen Execution System/CommitPort；执行状态按生成阶段更新。
- 成员文件数：1；声明类型数：1；字段：10；属性：1；合计：11。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（10）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2306 | field | Terraria.WorldBuilding.WorldGenerator | Terraria.WorldBuilding/WorldGenerator.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldGenerator.cs | 262 | 2 | _passes | System.Collections.Generic.List<Terraria.WorldBuilding.GenPass> | `internal readonly List<GenPass> _passes = new List<GenPass>();` | `internal readonly List<GenPass> _passes = new List<GenPass>();` |
| 2307 | field | Terraria.WorldBuilding.WorldGenerator | Terraria.WorldBuilding/WorldGenerator.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldGenerator.cs | 264 | 2 | _seed | int | `private readonly int _seed;` | `private readonly int _seed;` |
| 2308 | field | Terraria.WorldBuilding.WorldGenerator | Terraria.WorldBuilding/WorldGenerator.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldGenerator.cs | 266 | 2 | _configuration | Terraria.WorldBuilding.WorldGenConfiguration | `private readonly WorldGenConfiguration _configuration;` | `private readonly WorldGenConfiguration _configuration;` |
| 2309 | field | Terraria.WorldBuilding.WorldGenerator | Terraria.WorldBuilding/WorldGenerator.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldGenerator.cs | 268 | 2 | _progress | Terraria.WorldBuilding.GenerationProgress | `private readonly GenerationProgress _progress;` | `private readonly GenerationProgress _progress;` |
| 2310 | field | Terraria.WorldBuilding.WorldGenerator | Terraria.WorldBuilding/WorldGenerator.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldGenerator.cs | 270 | 2 | _controller | Terraria.WorldBuilding.WorldGenerator.Controller | `private readonly Controller _controller;` | `private readonly Controller _controller;` |
| 2311 | field | Terraria.WorldBuilding.WorldGenerator | Terraria.WorldBuilding/WorldGenerator.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldGenerator.cs | 272 | 2 | _controlLock | object | `private readonly object _controlLock = new object();` | `private readonly object _controlLock = new object();` |
| 2312 | field | Terraria.WorldBuilding.WorldGenerator | Terraria.WorldBuilding/WorldGenerator.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldGenerator.cs | 274 | 2 | _currentPass | Terraria.WorldBuilding.GenPass | `private GenPass _currentPass;` | `private GenPass _currentPass;` |
| 2313 | field | Terraria.WorldBuilding.WorldGenerator | Terraria.WorldBuilding/WorldGenerator.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldGenerator.cs | 276 | 2 | CurrentGenerationProgress | Terraria.WorldBuilding.GenerationProgress | `public static GenerationProgress CurrentGenerationProgress;` | `public static GenerationProgress CurrentGenerationProgress;` |
| 2314 | field | Terraria.WorldBuilding.WorldGenerator | Terraria.WorldBuilding/WorldGenerator.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldGenerator.cs | 278 | 2 | CurrentController | Terraria.WorldBuilding.WorldGenerator.Controller | `public static Controller CurrentController;` | `public static Controller CurrentController;` |
| 2315 | field | Terraria.WorldBuilding.WorldGenerator | Terraria.WorldBuilding/WorldGenerator.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldGenerator.cs | 280 | 2 | _hashTime | System.Diagnostics.Stopwatch | `private static Stopwatch _hashTime = new Stopwatch();` | `private static Stopwatch _hashTime = new Stopwatch();` |

##### 属性（1）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2596 | property | Terraria.WorldBuilding.WorldGenerator | Terraria.WorldBuilding/WorldGenerator.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldGenerator.cs | 282 | 2 | PassResults | System.Collections.Generic.List<Terraria.WorldBuilding.GenPassResult> | `public static List<GenPassResult> PassResults => WorldGen.Manifest.GenPassResults;` | `public static List<GenPassResult> PassResults => WorldGen.Manifest.GenPassResults;` |


#### 4.20.42 细分子系统：`WorldGenerationSnapshotState`

- 细分职责：WorldGenSnapshot 数据、序列化配置和恢复索引。
- 边界角色：`registry/projection`；最小 seam：Snapshot/Projection seam；快照只输出可恢复视图，不成为实时权威状态。
- 成员文件数：1；声明类型数：2；字段：7；属性：6；合计：13。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（7）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2320 | field | Terraria.WorldBuilding.WorldGenSnapshot.SnapshotGenVars | Terraria.WorldBuilding/WorldGenSnapshot.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldGenSnapshot.cs | 20 | 3 | SerializerSettings | JsonSerializerSettings | `public static JsonSerializerSettings SerializerSettings = new JsonSerializerSettings  		{  			ContractResolver = new EasyDeserializationJsonContractResolver(),  			PreserveReferencesHandling = PreserveReferencesHandling.Objects,  			ReferenceLoopHandling = ReferenceLoopHandling.Serialize,  			TypeNameHandling = TypeNameHandling.Auto  		};` | `public static JsonSerializerSettings SerializerSettings = new JsonSerializerSettings { ContractResolver = new EasyDeserializationJsonContractResolver(), PreserveReferencesHandling = PreserveReferencesHandling.Objects, ReferenceLoopHandling = ReferenceLoopHandling.Serialize, TypeNameHandling = TypeNameHandling.Auto };` |
| 2321 | field | Terraria.WorldBuilding.WorldGenSnapshot.SnapshotGenVars | Terraria.WorldBuilding/WorldGenSnapshot.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldGenSnapshot.cs | 28 | 3 | fieldsAndProperties | System.Collections.Generic.Dictionary<string, System.Reflection.MemberInfo> | `private static Dictionary<string, MemberInfo> fieldsAndProperties = (from m in ((IEnumerable<MemberInfo>)typeof(GenVars).GetFields(BindingFlags.Static \| BindingFlags.Public)).Concat((IEnumerable<MemberInfo>)typeof(GenVars).GetProperties(BindingFlags.Static \| BindingFlags.Public))  			where !(m is PropertyInfo) \|\| ((PropertyInfo)m).CanWrite  			where !(m is FieldInfo) \|\| !((FieldInfo)m).IsInitOnly  			where !m.GetCustomAttributes(typeof(JsonIgnoreAttribute), inherit: true).Any()  			select m).ToDictionary((MemberInfo m) => m.Name);` | `private static Dictionary<string, MemberInfo> fieldsAndProperties = (from m in ((IEnumerable<MemberInfo>)typeof(GenVars).GetFields(BindingFlags.Static \| BindingFlags.Public)).Concat((IEnumerable<MemberInfo>)typeof(GenVars).GetProperties(BindingFlags.Static \| BindingFlags.Public)) where !(m is PropertyInfo) \|\| ((PropertyInfo)m).CanWrite where !(m is FieldInfo) \|\| !((FieldInfo)m).IsInitOnly where !m.GetCustomAttributes(typeof(JsonIgnoreAttribute), inherit: true).Any() select m).ToDictionary((MemberInfo m) => m.Name);` |
| 2322 | field | Terraria.WorldBuilding.WorldGenSnapshot | Terraria.WorldBuilding/WorldGenSnapshot.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldGenSnapshot.cs | 58 | 2 | _dataOffset | int | `private int _dataOffset;` | `private int _dataOffset;` |
| 2323 | field | Terraria.WorldBuilding.WorldGenSnapshot | Terraria.WorldBuilding/WorldGenSnapshot.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldGenSnapshot.cs | 60 | 2 | _matchingPasses | System.Collections.Generic.List<Terraria.WorldBuilding.GenPass> | `private List<GenPass> _matchingPasses;` | `private List<GenPass> _matchingPasses;` |
| 2324 | field | Terraria.WorldBuilding.WorldGenSnapshot | Terraria.WorldBuilding/WorldGenSnapshot.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldGenSnapshot.cs | 62 | 2 | SnapshotFolderSuffix | string | `private static string SnapshotFolderSuffix = "_gensnapshots";` | `private static string SnapshotFolderSuffix = "_gensnapshots";` |
| 2325 | field | Terraria.WorldBuilding.WorldGenSnapshot | Terraria.WorldBuilding/WorldGenSnapshot.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldGenSnapshot.cs | 64 | 2 | Extension | string | `private static string Extension = ".gensnapshot";` | `private static string Extension = ".gensnapshot";` |
| 2326 | field | Terraria.WorldBuilding.WorldGenSnapshot | Terraria.WorldBuilding/WorldGenSnapshot.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldGenSnapshot.cs | 66 | 2 | _snapshotSizeCache | System.Collections.Generic.IDictionary<string, long> | `private static IDictionary<string, long> _snapshotSizeCache = new Dictionary<string, long>();` | `private static IDictionary<string, long> _snapshotSizeCache = new Dictionary<string, long>();` |

##### 属性（6）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2599 | property | Terraria.WorldBuilding.WorldGenSnapshot | Terraria.WorldBuilding/WorldGenSnapshot.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldGenSnapshot.cs | 68 | 2 | Manifest | Terraria.WorldBuilding.WorldManifest | `public WorldManifest Manifest { get; private set; }` | `public WorldManifest Manifest { get; private set; }` |
| 2600 | property | Terraria.WorldBuilding.WorldGenSnapshot | Terraria.WorldBuilding/WorldGenSnapshot.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldGenSnapshot.cs | 70 | 2 | Path | string | `private string Path { get; set; }` | `private string Path { get; set; }` |
| 2601 | property | Terraria.WorldBuilding.WorldGenSnapshot | Terraria.WorldBuilding/WorldGenSnapshot.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldGenSnapshot.cs | 72 | 2 | GenVarsJson | string | `private string GenVarsJson { get; set; }` | `private string GenVarsJson { get; set; }` |
| 2602 | property | Terraria.WorldBuilding.WorldGenSnapshot | Terraria.WorldBuilding/WorldGenSnapshot.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldGenSnapshot.cs | 74 | 2 | GenPassResults | System.Collections.Generic.List<Terraria.WorldBuilding.GenPassResult> | `public List<GenPassResult> GenPassResults => Manifest.GenPassResults;` | `public List<GenPassResult> GenPassResults => Manifest.GenPassResults;` |
| 2603 | property | Terraria.WorldBuilding.WorldGenSnapshot | Terraria.WorldBuilding/WorldGenSnapshot.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldGenSnapshot.cs | 76 | 2 | Outdated | bool | `public bool Outdated { get { if (!(Manifest.GitSHA != GitStatus.GitSHA) && !(Manifest.Version != Main.versionNumber)) { return !_matchingPasses.Zip(GenPassResults, (GenPass p, GenPassResult r) => p.Enabled == !r.Skipped).All((bool x) => x); } return true; } }` | `public bool Outdated { get { if (!(Manifest.GitSHA != GitStatus.GitSHA) && !(Manifest.Version != Main.versionNumber)) { return !_matchingPasses.Zip(GenPassResults, (GenPass p, GenPassResult r) => p.Enabled == !r.Skipped).All((bool x) => x); } return true; } }` |
| 2604 | property | Terraria.WorldBuilding.WorldGenSnapshot | Terraria.WorldBuilding/WorldGenSnapshot.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldGenSnapshot.cs | 88 | 2 | PathForActiveWorld | string | `private static string PathForActiveWorld => System.IO.Path.ChangeExtension(Main.ActiveWorldFileData.Path, null) + SnapshotFolderSuffix;` | `private static string PathForActiveWorld => System.IO.Path.ChangeExtension(Main.ActiveWorldFileData.Path, null) + SnapshotFolderSuffix;` |


#### 4.20.43 细分子系统：`WorldGenerationManifestAndPassResults`

- 细分职责：生成 Manifest、Pass 结果、哈希和耗时结果。
- 边界角色：`registry/projection`；最小 seam：Manifest/Projection seam；结果单向发布到持久化或诊断边界。
- 成员文件数：2；声明类型数：2；字段：2；属性：6；合计：8。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（2）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2327 | field | Terraria.WorldBuilding.WorldManifest | Terraria.WorldBuilding/WorldManifest.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldManifest.cs | 11 | 2 | GenPassResults | System.Collections.Generic.List<Terraria.WorldBuilding.GenPassResult> | `public List<GenPassResult> GenPassResults = new List<GenPassResult>();` | `public List<GenPassResult> GenPassResults = new List<GenPassResult>();` |
| 2328 | field | Terraria.WorldBuilding.WorldManifest | Terraria.WorldBuilding/WorldManifest.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldManifest.cs | 13 | 2 | SerializerSettings | JsonSerializerSettings | `public static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings  	{  		TypeNameHandling = TypeNameHandling.Auto  	};` | `public static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };` |

##### 属性（6）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2578 | property | Terraria.WorldBuilding.GenPassResult | Terraria.WorldBuilding/GenPassResult.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenPassResult.cs | 6 | 2 | DurationMs | int | `public int DurationMs { get; set; }` | `public int DurationMs { get; set; }` |
| 2579 | property | Terraria.WorldBuilding.GenPassResult | Terraria.WorldBuilding/GenPassResult.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenPassResult.cs | 8 | 2 | Hash | uint? | `public uint? Hash { get; set; }` | `public uint? Hash { get; set; }` |
| 2580 | property | Terraria.WorldBuilding.GenPassResult | Terraria.WorldBuilding/GenPassResult.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenPassResult.cs | 10 | 2 | Skipped | bool | `public bool Skipped { get; set; }` | `public bool Skipped { get; set; }` |
| 2605 | property | Terraria.WorldBuilding.WorldManifest | Terraria.WorldBuilding/WorldManifest.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldManifest.cs | 18 | 2 | Version | string | `public string Version { get; set; }` | `public string Version { get; set; }` |
| 2606 | property | Terraria.WorldBuilding.WorldManifest | Terraria.WorldBuilding/WorldManifest.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldManifest.cs | 20 | 2 | GitSHA | string | `public string GitSHA { get; set; }` | `public string GitSHA { get; set; }` |
| 2607 | property | Terraria.WorldBuilding.WorldManifest | Terraria.WorldBuilding/WorldManifest.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldManifest.cs | 22 | 2 | FinalHash | uint? | `public uint? FinalHash { get { if (GenPassResults.Count <= 0) { return null; } return GenPassResults[GenPassResults.Count - 1].Hash; } }` | `public uint? FinalHash { get { if (GenPassResults.Count <= 0) { return null; } return GenPassResults[GenPassResults.Count - 1].Hash; } }` |


#### 4.20.44 细分子系统：`WorldGenerationOptionBaseState`

- 细分职责：世界生成选项基类、配置根和启用、名称、描述、展示定义。
- 边界角色：`definition/query`；最小 seam：只读 Definition/Catalog view；选项系统负责生命周期。
- 成员文件数：2；声明类型数：2；字段：4；属性：8；合计：12。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（4）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2079 | field | Terraria.WorldBuilding.AWorldGenerationOption | Terraria.WorldBuilding/AWorldGenerationOption.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\AWorldGenerationOption.cs | 12 | 2 | _enabled | bool | `private bool _enabled;` | `private bool _enabled;` |
| 2080 | field | Terraria.WorldBuilding.AWorldGenerationOption | Terraria.WorldBuilding/AWorldGenerationOption.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\AWorldGenerationOption.cs | 14 | 2 | AutoGenEnabled | bool | `public bool AutoGenEnabled;` | `public bool AutoGenEnabled;` |
| 2296 | field | Terraria.WorldBuilding.WorldGenConfiguration | Terraria.WorldBuilding/WorldGenConfiguration.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldGenConfiguration.cs | 11 | 2 | _biomeRoot | JObject | `private readonly JObject _biomeRoot;` | `private readonly JObject _biomeRoot;` |
| 2297 | field | Terraria.WorldBuilding.WorldGenConfiguration | Terraria.WorldBuilding/WorldGenConfiguration.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldGenConfiguration.cs | 13 | 2 | _passRoot | JObject | `private readonly JObject _passRoot;` | `private readonly JObject _passRoot;` |

##### 属性（8）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2563 | property | Terraria.WorldBuilding.AWorldGenerationOption | Terraria.WorldBuilding/AWorldGenerationOption.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\AWorldGenerationOption.cs | 16 | 2 | Enabled | bool | `public bool Enabled { get { return _enabled; } set { if (_enabled != value) { _enabled = value; OnEnabledStateChanged(); AWorldGenerationOption.OnOptionStateChanged(this); } } }` | `public bool Enabled { get { return _enabled; } set { if (_enabled != value) { _enabled = value; OnEnabledStateChanged(); AWorldGenerationOption.OnOptionStateChanged(this); } } }` |
| 2564 | property | Terraria.WorldBuilding.AWorldGenerationOption | Terraria.WorldBuilding/AWorldGenerationOption.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\AWorldGenerationOption.cs | 33 | 2 | KeyName | string | `protected abstract string KeyName { get; }` | `protected abstract string KeyName { get; }` |
| 2565 | property | Terraria.WorldBuilding.AWorldGenerationOption | Terraria.WorldBuilding/AWorldGenerationOption.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\AWorldGenerationOption.cs | 35 | 2 | ServerConfigName | string | `public abstract string ServerConfigName { get; }` | `public abstract string ServerConfigName { get; }` |
| 2566 | property | Terraria.WorldBuilding.AWorldGenerationOption | Terraria.WorldBuilding/AWorldGenerationOption.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\AWorldGenerationOption.cs | 37 | 2 | SpecialSeedNames | string[] | `public string[] SpecialSeedNames { get; protected set; }` | `public string[] SpecialSeedNames { get; protected set; }` |
| 2567 | property | Terraria.WorldBuilding.AWorldGenerationOption | Terraria.WorldBuilding/AWorldGenerationOption.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\AWorldGenerationOption.cs | 39 | 2 | SpecialSeedValues | int[] | `public int[] SpecialSeedValues { get; protected set; }` | `public int[] SpecialSeedValues { get; protected set; }` |
| 2568 | property | Terraria.WorldBuilding.AWorldGenerationOption | Terraria.WorldBuilding/AWorldGenerationOption.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\AWorldGenerationOption.cs | 41 | 2 | Description | Terraria.Localization.LocalizedText | `public LocalizedText Description { get; private set; }` | `public LocalizedText Description { get; private set; }` |
| 2569 | property | Terraria.WorldBuilding.AWorldGenerationOption | Terraria.WorldBuilding/AWorldGenerationOption.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\AWorldGenerationOption.cs | 43 | 2 | Title | Terraria.Localization.LocalizedText | `public LocalizedText Title { get; private set; }` | `public LocalizedText Title { get; private set; }` |
| 2570 | property | Terraria.WorldBuilding.AWorldGenerationOption | Terraria.WorldBuilding/AWorldGenerationOption.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\AWorldGenerationOption.cs | 45 | 2 | Texture | Asset<Texture2D> | `protected Asset<Texture2D> Texture { get; private set; }` | `protected Asset<Texture2D> Texture { get; private set; }` |


#### 4.20.45 细分子系统：`WorldGenerationOptionRegistry`

- 细分职责：世界生成选项列表和选项注册表投影。
- 边界角色：`registry/projection`；最小 seam：Registry/Projection seam；注册表只提供只读枚举。
- 成员文件数：1；声明类型数：1；字段：2；属性：1；合计：3。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（2）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2299 | field | Terraria.WorldBuilding.WorldGenerationOptions | Terraria.WorldBuilding/WorldGenerationOptions.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldGenerationOptions.cs | 16 | 2 | _options | System.Collections.Generic.List<Terraria.WorldBuilding.AWorldGenerationOption> | `private static List<AWorldGenerationOption> _options;` | `private static List<AWorldGenerationOption> _options;` |
| 2300 | field | Terraria.WorldBuilding.WorldGenerationOptions | Terraria.WorldBuilding/WorldGenerationOptions.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldGenerationOptions.cs | 18 | 2 | _powerPermissionsLineHeader | string | `private const string _powerPermissionsLineHeader = "seed_";` | `private const string _powerPermissionsLineHeader = "seed_";` |

##### 属性（1）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2586 | property | Terraria.WorldBuilding.WorldGenerationOptions | Terraria.WorldBuilding/WorldGenerationOptions.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldGenerationOptions.cs | 20 | 2 | Options | System.Collections.Generic.IEnumerable<Terraria.WorldBuilding.AWorldGenerationOption> | `public static IEnumerable<AWorldGenerationOption> Options => _options;` | `public static IEnumerable<AWorldGenerationOption> Options => _options;` |


#### 4.20.61 细分子系统：`WorldGenerationSupportTypes`

- 细分职责：生成流水线的通用 Action、Shape 和 Tree 检查辅助状态。
- 边界角色：`definition/query`；最小 seam：只读 Definition/Catalog view；规则 System 消费，外部配置通过 Adapter 转换。
- 成员文件数：3；声明类型数：3；字段：4；属性：0；合计：4。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（4）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2086 | field | Terraria.WorldBuilding.GenAction | Terraria.WorldBuilding/GenAction.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenAction.cs | 7 | 2 | NextAction | Terraria.WorldBuilding.GenAction | `public GenAction NextAction;` | `public GenAction NextAction;` |
| 2087 | field | Terraria.WorldBuilding.GenAction | Terraria.WorldBuilding/GenAction.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\GenAction.cs | 9 | 2 | OutputData | Terraria.WorldBuilding.ShapeData | `public ShapeData OutputData;` | `public ShapeData OutputData;` |
| 2298 | field | Terraria.WorldBuilding.WorldGenerationOptions.OptionStorage<T> | Terraria.WorldBuilding/WorldGenerationOptions.cs | D:\TRbackup\Version4\Terraria.WorldBuilding\WorldGenerationOptions.cs | 13 | 3 | Instance | T | `public static T Instance;` | `public static T Instance;` |
| 2408 | field | Terraria.WorldGen.CheckTreeSettings | Terraria/WorldGen.cs | D:\TRbackup\Version4\Terraria\WorldGen.cs | 4013 | 3 | IsGroundValid | Terraria.WorldGen.CheckTreeSettings.GroundValidTest | `public GroundValidTest IsGroundValid;` | `public GroundValidTest IsGroundValid;` |

##### 属性（0）

无该类型成员记录。


## 8. 本分区自检

- 叶子子系统：10 个；指定组均已写入且没有重复分区。
- 字段/属性/成员：47 / 37 / 84。
- 来源序号范围：2079..2607；本分区内唯一序号：True。
- 成员声明表逐组从源报告提取，未改写来源序号、类型、路径、行列、成员、C# 类型、成员声明或原始声明。
- 运行时读写者、生命周期、持久化、网络和调度：未验证，保留为迁移前风险。
