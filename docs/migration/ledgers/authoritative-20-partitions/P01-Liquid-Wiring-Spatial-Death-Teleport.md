# P01 基础模拟、液体、机关、空间、死亡惩罚与传送 - 权威模拟系统成员组件候选分区

> 来源：`docs/migration/ledgers/Version4权威模拟系统字段属性逐成员源码声明-更细子系统拆分-去除ID类文件.md`；本文件只做成员库存分区和候选组件边界记录。
> 这是候选组件拆分台账，不是运行时迁移完成、行为等价、API 兼容、网络闭合或持久化闭合证明。

## 1. 分区边界

本分区包含 14 个叶子子系统，字段 111 条、属性 2 条、成员合计 113 条；每个叶子子系统保持原正式父级归属，不跨分区拆行。

## 2. 分区覆盖概览

| 叶子子系统 | 正式父级 | 字段 | 属性 | 合计 | 源边界角色 |
|---|---|---:|---:|---:|---|
| `LiquidFlowBudgetAndPanicState` | `LiquidSimulation` | 15 | 0 | 15 | authoritative state/behavior |
| `LiquidCellWorkItemState` | `LiquidSimulation` | 4 | 0 | 4 | authoritative state/behavior |
| `LiquidBufferQueueState` | `LiquidSimulation` | 3 | 0 | 3 | authoritative state/behavior |
| `LiquidChangePublication` | `LiquidSimulation` | 2 | 0 | 2 | registry/projection |
| `WiringPropagationAndGateState` | `WiringAndMechanisms` | 12 | 0 | 12 | authoritative state/behavior |
| `WiringTeleportAndPumpState` | `WiringAndMechanisms` | 9 | 0 | 9 | authoritative state/behavior |
| `WiringMechanismCooldowns` | `WiringAndMechanisms` | 9 | 0 | 9 | authoritative state/behavior |
| `CollisionQueryCache` | `SpatialSimulation` | 10 | 0 | 10 | authoritative state/behavior |
| `CollisionContactAndHurtResults` | `SpatialSimulation` | 9 | 0 | 9 | authoritative state/behavior |
| `RevengeMarkerExpirationAndIdentityState` | `DeathPenaltyAndRevenge` | 8 | 1 | 9 | authoritative state/behavior |
| `RevengeMarkerEnemyContextState` | `DeathPenaltyAndRevenge` | 10 | 0 | 10 | authoritative state/behavior |
| `RevengeMarkerValueAndRespawnState` | `DeathPenaltyAndRevenge` | 4 | 1 | 5 | authoritative state/behavior |
| `RevengeRegistryAndCache` | `DeathPenaltyAndRevenge` | 11 | 0 | 11 | registry/projection |
| `TeleportPylonRegistry` | `TeleportationAndTraversal` | 5 | 0 | 5 | registry/projection |

## 3. 候选组件归属与 seam

| 叶子子系统 | 候选组件边界 | 类型分类 | 最小 seam | 独立生命周期 | 证据状态 | 主要风险 |
|---|---|---|---|---|---|---|
| `LiquidFlowBudgetAndPanicState` | `LiquidFlowBudgetAndPanicStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `LiquidCellWorkItemState` | `LiquidCellWorkItemStateCommand` | Command payload | 显式 Command/CommitPort；不持有长期权威状态 | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `LiquidBufferQueueState` | `LiquidBufferQueueStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `LiquidChangePublication` | `LiquidChangePublicationProjection`/`Adapter` | Projection/Adapter | 单向 Projection/Adapter seam；不得回写权威状态 | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `WiringPropagationAndGateState` | `WiringPropagationAndGateStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `WiringTeleportAndPumpState` | `WiringTeleportAndPumpStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `WiringMechanismCooldowns` | `WiringMechanismCooldownsComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `CollisionQueryCache` | `CollisionQueryCacheComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `CollisionContactAndHurtResults` | `CollisionContactAndHurtResultsComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `RevengeMarkerExpirationAndIdentityState` | `RevengeMarkerExpirationAndIdentityStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `RevengeMarkerEnemyContextState` | `RevengeMarkerEnemyContextStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `RevengeMarkerValueAndRespawnState` | `RevengeMarkerValueAndRespawnStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `RevengeRegistryAndCache` | `RevengeRegistryAndCacheProjection`/`Adapter` | Projection/Adapter | 单向 Projection/Adapter seam；不得回写权威状态 | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `TeleportPylonRegistry` | `TeleportPylonRegistryProjection`/`Adapter` | Projection/Adapter | 单向 Projection/Adapter seam；不得回写权威状态 | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |

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

### 正式父级子系统：`LiquidSimulation`
- 父级职责：沿用源报告正式父级 `LiquidSimulation`；本分区只收录该父级中列出的叶子子系统。
- 本分区父级局部统计：字段 24；属性 0；合计 24；完整父级统计以源报告为准。

#### 4.1.1 细分子系统：`LiquidFlowBudgetAndPanicState`

- 细分职责：液体预算、循环、停滞和 panic 流程状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Liquid Flow System/CommitPort；流动 tick 集中维护预算和 panic。
- 成员文件数：1；声明类型数：1；字段：15；属性：0；合计：15。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（15）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1 | field | Terraria.Liquid | Terraria/Liquid.cs | D:\TRbackup\Version4\Terraria\Liquid.cs | 14 | 2 | maxLiquidBuffer | int | `public const int maxLiquidBuffer = 50000;` | `public const int maxLiquidBuffer = 50000;` |
| 2 | field | Terraria.Liquid | Terraria/Liquid.cs | D:\TRbackup\Version4\Terraria\Liquid.cs | 16 | 2 | maxLiquid | int | `public static int maxLiquid = 25000;` | `public static int maxLiquid = 25000;` |
| 3 | field | Terraria.Liquid | Terraria/Liquid.cs | D:\TRbackup\Version4\Terraria\Liquid.cs | 18 | 2 | skipCount | int | `public static int skipCount;` | `public static int skipCount;` |
| 4 | field | Terraria.Liquid | Terraria/Liquid.cs | D:\TRbackup\Version4\Terraria\Liquid.cs | 20 | 2 | stuckCount | int | `public static int stuckCount;` | `public static int stuckCount;` |
| 5 | field | Terraria.Liquid | Terraria/Liquid.cs | D:\TRbackup\Version4\Terraria\Liquid.cs | 22 | 2 | stuckAmount | int | `public static int stuckAmount;` | `public static int stuckAmount;` |
| 6 | field | Terraria.Liquid | Terraria/Liquid.cs | D:\TRbackup\Version4\Terraria\Liquid.cs | 24 | 2 | cycles | int | `public static int cycles = 10;` | `public static int cycles = 10;` |
| 7 | field | Terraria.Liquid | Terraria/Liquid.cs | D:\TRbackup\Version4\Terraria\Liquid.cs | 26 | 2 | curMaxLiquid | int | `public static int curMaxLiquid = 0;` | `public static int curMaxLiquid = 0;` |
| 8 | field | Terraria.Liquid | Terraria/Liquid.cs | D:\TRbackup\Version4\Terraria\Liquid.cs | 28 | 2 | numLiquid | int | `public static int numLiquid;` | `public static int numLiquid;` |
| 9 | field | Terraria.Liquid | Terraria/Liquid.cs | D:\TRbackup\Version4\Terraria\Liquid.cs | 30 | 2 | stuck | bool | `public static bool stuck;` | `public static bool stuck;` |
| 10 | field | Terraria.Liquid | Terraria/Liquid.cs | D:\TRbackup\Version4\Terraria\Liquid.cs | 32 | 2 | quickFall | bool | `public static bool quickFall;` | `public static bool quickFall;` |
| 11 | field | Terraria.Liquid | Terraria/Liquid.cs | D:\TRbackup\Version4\Terraria\Liquid.cs | 34 | 2 | quickSettle | bool | `public static bool quickSettle;` | `public static bool quickSettle;` |
| 12 | field | Terraria.Liquid | Terraria/Liquid.cs | D:\TRbackup\Version4\Terraria\Liquid.cs | 36 | 2 | wetCounter | int | `private static int wetCounter;` | `private static int wetCounter;` |
| 13 | field | Terraria.Liquid | Terraria/Liquid.cs | D:\TRbackup\Version4\Terraria\Liquid.cs | 38 | 2 | panicCounter | int | `public static int panicCounter;` | `public static int panicCounter;` |
| 14 | field | Terraria.Liquid | Terraria/Liquid.cs | D:\TRbackup\Version4\Terraria\Liquid.cs | 40 | 2 | panicMode | bool | `public static bool panicMode;` | `public static bool panicMode;` |
| 15 | field | Terraria.Liquid | Terraria/Liquid.cs | D:\TRbackup\Version4\Terraria\Liquid.cs | 42 | 2 | panicY | int | `public static int panicY;` | `public static int panicY;` |

##### 属性（0）

无该类型成员记录。


#### 4.1.2 细分子系统：`LiquidCellWorkItemState`

- 细分职责：单个液体工作项的坐标、清除和延迟状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Liquid Work Queue/Command；工作项通过显式队列消费。
- 成员文件数：1；声明类型数：1；字段：4；属性：0；合计：4。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（4）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 16 | field | Terraria.Liquid | Terraria/Liquid.cs | D:\TRbackup\Version4\Terraria\Liquid.cs | 44 | 2 | x | int | `public int x;` | `public int x;` |
| 17 | field | Terraria.Liquid | Terraria/Liquid.cs | D:\TRbackup\Version4\Terraria\Liquid.cs | 46 | 2 | y | int | `public int y;` | `public int y;` |
| 18 | field | Terraria.Liquid | Terraria/Liquid.cs | D:\TRbackup\Version4\Terraria\Liquid.cs | 48 | 2 | kill | int | `public int kill;` | `public int kill;` |
| 19 | field | Terraria.Liquid | Terraria/Liquid.cs | D:\TRbackup\Version4\Terraria\Liquid.cs | 50 | 2 | delay | int | `public int delay;` | `public int delay;` |

##### 属性（0）

无该类型成员记录。


#### 4.1.3 细分子系统：`LiquidBufferQueueState`

- 细分职责：液体缓冲队列的计数和坐标状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Liquid Buffer Queue/CommitPort；缓冲结构变化集中提交。
- 成员文件数：1；声明类型数：1；字段：3；属性：0；合计：3。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（3）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 22 | field | Terraria.LiquidBuffer | Terraria/LiquidBuffer.cs | D:\TRbackup\Version4\Terraria\LiquidBuffer.cs | 5 | 2 | numLiquidBuffer | int | `public static int numLiquidBuffer;` | `public static int numLiquidBuffer;` |
| 23 | field | Terraria.LiquidBuffer | Terraria/LiquidBuffer.cs | D:\TRbackup\Version4\Terraria\LiquidBuffer.cs | 7 | 2 | x | int | `public int x;` | `public int x;` |
| 24 | field | Terraria.LiquidBuffer | Terraria/LiquidBuffer.cs | D:\TRbackup\Version4\Terraria\LiquidBuffer.cs | 9 | 2 | y | int | `public int y;` | `public int y;` |

##### 属性（0）

无该类型成员记录。


#### 4.1.4 细分子系统：`LiquidChangePublication`

- 细分职责：液体网络变更集合和交换集合的提交边界。
- 边界角色：`registry/projection`；最小 seam：Registry/Projection seam；提交后的事实单向输出，不把网络/UI/索引反写成权威状态。
- 成员文件数：1；声明类型数：1；字段：2；属性：0；合计：2。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（2）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 20 | field | Terraria.Liquid | Terraria/Liquid.cs | D:\TRbackup\Version4\Terraria\Liquid.cs | 52 | 2 | _netChangeSet | System.Collections.Generic.HashSet<int> | `private static HashSet<int> _netChangeSet = new HashSet<int>();` | `private static HashSet<int> _netChangeSet = new HashSet<int>();` |
| 21 | field | Terraria.Liquid | Terraria/Liquid.cs | D:\TRbackup\Version4\Terraria\Liquid.cs | 54 | 2 | _swapNetChangeSet | System.Collections.Generic.HashSet<int> | `private static HashSet<int> _swapNetChangeSet = new HashSet<int>();` | `private static HashSet<int> _swapNetChangeSet = new HashSet<int>();` |

##### 属性（0）

无该类型成员记录。


### 正式父级子系统：`WiringAndMechanisms`
- 父级职责：沿用源报告正式父级 `WiringAndMechanisms`；本分区只收录该父级中列出的叶子子系统。
- 本分区父级局部统计：字段 30；属性 0；合计 30；完整父级统计以源报告为准。

#### 4.4.1 细分子系统：`WiringPropagationAndGateState`

- 细分职责：电线传播队列、颜色、逻辑门/灯检查和当前机制上下文。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System 读取输入并通过显式 Command/CommitPort 写入；跨组只用事件、Query 或命令交接。
- 成员文件数：1；声明类型数：1；字段：12；属性：0；合计：12。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（12）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 159 | field | Terraria.Wiring | Terraria/Wiring.cs | D:\TRbackup\Version4\Terraria\Wiring.cs | 19 | 2 | running | bool | `public static bool running;` | `public static bool running;` |
| 160 | field | Terraria.Wiring | Terraria/Wiring.cs | D:\TRbackup\Version4\Terraria\Wiring.cs | 21 | 2 | _wireSkip | System.Collections.Generic.Dictionary<Terraria.DataStructures.Point16, bool> | `private static Dictionary<Point16, bool> _wireSkip;` | `private static Dictionary<Point16, bool> _wireSkip;` |
| 161 | field | Terraria.Wiring | Terraria/Wiring.cs | D:\TRbackup\Version4\Terraria\Wiring.cs | 23 | 2 | _wireList | Terraria.DataStructures.DoubleStack<Terraria.DataStructures.Point16> | `private static DoubleStack<Point16> _wireList;` | `private static DoubleStack<Point16> _wireList;` |
| 162 | field | Terraria.Wiring | Terraria/Wiring.cs | D:\TRbackup\Version4\Terraria\Wiring.cs | 25 | 2 | _wireDirectionList | Terraria.DataStructures.DoubleStack<byte> | `private static DoubleStack<byte> _wireDirectionList;` | `private static DoubleStack<byte> _wireDirectionList;` |
| 163 | field | Terraria.Wiring | Terraria/Wiring.cs | D:\TRbackup\Version4\Terraria\Wiring.cs | 27 | 2 | _toProcess | System.Collections.Generic.Dictionary<Terraria.DataStructures.Point16, byte> | `private static Dictionary<Point16, byte> _toProcess;` | `private static Dictionary<Point16, byte> _toProcess;` |
| 164 | field | Terraria.Wiring | Terraria/Wiring.cs | D:\TRbackup\Version4\Terraria\Wiring.cs | 29 | 2 | _GatesCurrent | System.Collections.Generic.Queue<Terraria.DataStructures.Point16> | `private static Queue<Point16> _GatesCurrent;` | `private static Queue<Point16> _GatesCurrent;` |
| 165 | field | Terraria.Wiring | Terraria/Wiring.cs | D:\TRbackup\Version4\Terraria\Wiring.cs | 31 | 2 | _LampsToCheck | System.Collections.Generic.Queue<Terraria.DataStructures.Point16> | `private static Queue<Point16> _LampsToCheck;` | `private static Queue<Point16> _LampsToCheck;` |
| 166 | field | Terraria.Wiring | Terraria/Wiring.cs | D:\TRbackup\Version4\Terraria\Wiring.cs | 33 | 2 | _GatesNext | System.Collections.Generic.Queue<Terraria.DataStructures.Point16> | `private static Queue<Point16> _GatesNext;` | `private static Queue<Point16> _GatesNext;` |
| 167 | field | Terraria.Wiring | Terraria/Wiring.cs | D:\TRbackup\Version4\Terraria\Wiring.cs | 35 | 2 | _GatesDone | System.Collections.Generic.Dictionary<Terraria.DataStructures.Point16, bool> | `private static Dictionary<Point16, bool> _GatesDone;` | `private static Dictionary<Point16, bool> _GatesDone;` |
| 168 | field | Terraria.Wiring | Terraria/Wiring.cs | D:\TRbackup\Version4\Terraria\Wiring.cs | 37 | 2 | _PixelBoxTriggers | System.Collections.Generic.Dictionary<Terraria.DataStructures.Point16, byte> | `private static Dictionary<Point16, byte> _PixelBoxTriggers;` | `private static Dictionary<Point16, byte> _PixelBoxTriggers;` |
| 182 | field | Terraria.Wiring | Terraria/Wiring.cs | D:\TRbackup\Version4\Terraria\Wiring.cs | 65 | 2 | _currentWireColor | int | `private static int _currentWireColor;` | `private static int _currentWireColor;` |
| 183 | field | Terraria.Wiring | Terraria/Wiring.cs | D:\TRbackup\Version4\Terraria\Wiring.cs | 67 | 2 | CurrentUser | int | `private static int CurrentUser = 255;` | `private static int CurrentUser = 255;` |

##### 属性（0）

无该类型成员记录。


#### 4.4.2 细分子系统：`WiringTeleportAndPumpState`

- 细分职责：电线触发传送和液体泵坐标/计数状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System 读取输入并通过显式 Command/CommitPort 写入；跨组只用事件、Query 或命令交接。
- 成员文件数：1；声明类型数：1；字段：9；属性：0；合计：9。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（9）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 158 | field | Terraria.Wiring | Terraria/Wiring.cs | D:\TRbackup\Version4\Terraria\Wiring.cs | 17 | 2 | blockPlayerTeleportationForOneIteration | bool | `public static bool blockPlayerTeleportationForOneIteration;` | `public static bool blockPlayerTeleportationForOneIteration;` |
| 169 | field | Terraria.Wiring | Terraria/Wiring.cs | D:\TRbackup\Version4\Terraria\Wiring.cs | 39 | 2 | _teleport | Vector2[] | `private static Vector2[] _teleport;` | `private static Vector2[] _teleport;` |
| 170 | field | Terraria.Wiring | Terraria/Wiring.cs | D:\TRbackup\Version4\Terraria\Wiring.cs | 41 | 2 | MaxPump | int | `private const int MaxPump = 20;` | `private const int MaxPump = 20;` |
| 171 | field | Terraria.Wiring | Terraria/Wiring.cs | D:\TRbackup\Version4\Terraria\Wiring.cs | 43 | 2 | _inPumpX | int[] | `private static int[] _inPumpX;` | `private static int[] _inPumpX;` |
| 172 | field | Terraria.Wiring | Terraria/Wiring.cs | D:\TRbackup\Version4\Terraria\Wiring.cs | 45 | 2 | _inPumpY | int[] | `private static int[] _inPumpY;` | `private static int[] _inPumpY;` |
| 173 | field | Terraria.Wiring | Terraria/Wiring.cs | D:\TRbackup\Version4\Terraria\Wiring.cs | 47 | 2 | _numInPump | int | `private static int _numInPump;` | `private static int _numInPump;` |
| 174 | field | Terraria.Wiring | Terraria/Wiring.cs | D:\TRbackup\Version4\Terraria\Wiring.cs | 49 | 2 | _outPumpX | int[] | `private static int[] _outPumpX;` | `private static int[] _outPumpX;` |
| 175 | field | Terraria.Wiring | Terraria/Wiring.cs | D:\TRbackup\Version4\Terraria\Wiring.cs | 51 | 2 | _outPumpY | int[] | `private static int[] _outPumpY;` | `private static int[] _outPumpY;` |
| 176 | field | Terraria.Wiring | Terraria/Wiring.cs | D:\TRbackup\Version4\Terraria\Wiring.cs | 53 | 2 | _numOutPump | int | `private static int _numOutPump;` | `private static int _numOutPump;` |

##### 属性（0）

无该类型成员记录。


#### 4.4.3 细分子系统：`WiringMechanismCooldowns`

- 细分职责：机制队列、机制时间和炮台/漏斗冷却状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System 读取输入并通过显式 Command/CommitPort 写入；跨组只用事件、Query 或命令交接。
- 成员文件数：1；声明类型数：1；字段：9；属性：0；合计：9。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（9）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 177 | field | Terraria.Wiring | Terraria/Wiring.cs | D:\TRbackup\Version4\Terraria\Wiring.cs | 55 | 2 | MaxMech | int | `private const int MaxMech = 1000;` | `private const int MaxMech = 1000;` |
| 178 | field | Terraria.Wiring | Terraria/Wiring.cs | D:\TRbackup\Version4\Terraria\Wiring.cs | 57 | 2 | _mechX | int[] | `private static int[] _mechX;` | `private static int[] _mechX;` |
| 179 | field | Terraria.Wiring | Terraria/Wiring.cs | D:\TRbackup\Version4\Terraria\Wiring.cs | 59 | 2 | _mechY | int[] | `private static int[] _mechY;` | `private static int[] _mechY;` |
| 180 | field | Terraria.Wiring | Terraria/Wiring.cs | D:\TRbackup\Version4\Terraria\Wiring.cs | 61 | 2 | _numMechs | int | `private static int _numMechs;` | `private static int _numMechs;` |
| 181 | field | Terraria.Wiring | Terraria/Wiring.cs | D:\TRbackup\Version4\Terraria\Wiring.cs | 63 | 2 | _mechTime | int[] | `private static int[] _mechTime;` | `private static int[] _mechTime;` |
| 184 | field | Terraria.Wiring | Terraria/Wiring.cs | D:\TRbackup\Version4\Terraria\Wiring.cs | 69 | 2 | cannonCoolDown | int | `private static int cannonCoolDown = 0;` | `private static int cannonCoolDown = 0;` |
| 185 | field | Terraria.Wiring | Terraria/Wiring.cs | D:\TRbackup\Version4\Terraria\Wiring.cs | 71 | 2 | bunnyCannonCoolDown | int | `private static int bunnyCannonCoolDown = 0;` | `private static int bunnyCannonCoolDown = 0;` |
| 186 | field | Terraria.Wiring | Terraria/Wiring.cs | D:\TRbackup\Version4\Terraria\Wiring.cs | 73 | 2 | snowballCannonCoolDown | int | `private static int snowballCannonCoolDown = 0;` | `private static int snowballCannonCoolDown = 0;` |
| 187 | field | Terraria.Wiring | Terraria/Wiring.cs | D:\TRbackup\Version4\Terraria\Wiring.cs | 75 | 2 | HopperGrabHitboxSize | Vector2 | `public static readonly Vector2 HopperGrabHitboxSize = new Vector2(192f);` | `public static readonly Vector2 HopperGrabHitboxSize = new Vector2(192f);` |

##### 属性（0）

无该类型成员记录。


### 正式父级子系统：`SpatialSimulation`
- 父级职责：沿用源报告正式父级 `SpatialSimulation`；本分区只收录该父级中列出的叶子子系统。
- 本分区父级局部统计：字段 19；属性 0；合计 19；完整父级统计以源报告为准。

#### 4.11.1 细分子系统：`CollisionQueryCache`

- 细分职责：碰撞查询的缓存和环境接触开关。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System 读取输入并通过显式 Command/CommitPort 写入；跨组只用事件、Query 或命令交接。
- 成员文件数：1；声明类型数：1；字段：10；属性：0；合计：10。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（10）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 360 | field | Terraria.Collision | Terraria/Collision.cs | D:\TRbackup\Version4\Terraria\Collision.cs | 55 | 2 | stair | bool | `public static bool stair;` | `public static bool stair;` |
| 361 | field | Terraria.Collision | Terraria/Collision.cs | D:\TRbackup\Version4\Terraria\Collision.cs | 57 | 2 | stairFall | bool | `public static bool stairFall;` | `public static bool stairFall;` |
| 362 | field | Terraria.Collision | Terraria/Collision.cs | D:\TRbackup\Version4\Terraria\Collision.cs | 59 | 2 | honey | bool | `public static bool honey;` | `public static bool honey;` |
| 363 | field | Terraria.Collision | Terraria/Collision.cs | D:\TRbackup\Version4\Terraria\Collision.cs | 61 | 2 | shimmer | bool | `public static bool shimmer;` | `public static bool shimmer;` |
| 364 | field | Terraria.Collision | Terraria/Collision.cs | D:\TRbackup\Version4\Terraria\Collision.cs | 63 | 2 | sloping | bool | `public static bool sloping;` | `public static bool sloping;` |
| 365 | field | Terraria.Collision | Terraria/Collision.cs | D:\TRbackup\Version4\Terraria\Collision.cs | 65 | 2 | up | bool | `public static bool up;` | `public static bool up;` |
| 366 | field | Terraria.Collision | Terraria/Collision.cs | D:\TRbackup\Version4\Terraria\Collision.cs | 67 | 2 | down | bool | `public static bool down;` | `public static bool down;` |
| 367 | field | Terraria.Collision | Terraria/Collision.cs | D:\TRbackup\Version4\Terraria\Collision.cs | 69 | 2 | bottomFluff | int | `private const int bottomFluff = 40;` | `private const int bottomFluff = 40;` |
| 368 | field | Terraria.Collision | Terraria/Collision.cs | D:\TRbackup\Version4\Terraria\Collision.cs | 71 | 2 | contacts | System.Collections.Generic.List<Terraria.Collision.TileContact> | `private static List<TileContact> contacts = new List<TileContact>();` | `private static List<TileContact> contacts = new List<TileContact>();` |
| 369 | field | Terraria.Collision | Terraria/Collision.cs | D:\TRbackup\Version4\Terraria\Collision.cs | 73 | 2 | _cacheForConveyorBelts | System.Collections.Generic.List<Point> | `private static List<Point> _cacheForConveyorBelts = new List<Point>();` | `private static List<Point> _cacheForConveyorBelts = new List<Point>();` |

##### 属性（0）

无该类型成员记录。


#### 4.11.2 细分子系统：`CollisionContactAndHurtResults`

- 细分职责：碰撞接触结果和伤害 Tile 结果值对象。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System 读取输入并通过显式 Command/CommitPort 写入；跨组只用事件、Query 或命令交接。
- 成员文件数：1；声明类型数：2；字段：9；属性：0；合计：9。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（9）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 351 | field | Terraria.Collision.TileContact | Terraria/Collision.cs | D:\TRbackup\Version4\Terraria\Collision.cs | 23 | 3 | Side | Terraria.Collision.TileContactSide | `public TileContactSide Side;` | `public TileContactSide Side;` |
| 352 | field | Terraria.Collision.TileContact | Terraria/Collision.cs | D:\TRbackup\Version4\Terraria\Collision.cs | 25 | 3 | Overlap | int | `public int Overlap;` | `public int Overlap;` |
| 353 | field | Terraria.Collision.TileContact | Terraria/Collision.cs | D:\TRbackup\Version4\Terraria\Collision.cs | 27 | 3 | X | int | `public int X;` | `public int X;` |
| 354 | field | Terraria.Collision.TileContact | Terraria/Collision.cs | D:\TRbackup\Version4\Terraria\Collision.cs | 29 | 3 | Y | int | `public int Y;` | `public int Y;` |
| 355 | field | Terraria.Collision.TileContact | Terraria/Collision.cs | D:\TRbackup\Version4\Terraria\Collision.cs | 31 | 3 | Slope | int | `public int Slope;` | `public int Slope;` |
| 356 | field | Terraria.Collision.TileContact | Terraria/Collision.cs | D:\TRbackup\Version4\Terraria\Collision.cs | 33 | 3 | Type | int | `public int Type;` | `public int Type;` |
| 357 | field | Terraria.Collision.HurtTile | Terraria/Collision.cs | D:\TRbackup\Version4\Terraria\Collision.cs | 48 | 3 | type | int | `public int type;` | `public int type;` |
| 358 | field | Terraria.Collision.HurtTile | Terraria/Collision.cs | D:\TRbackup\Version4\Terraria\Collision.cs | 50 | 3 | x | int | `public int x;` | `public int x;` |
| 359 | field | Terraria.Collision.HurtTile | Terraria/Collision.cs | D:\TRbackup\Version4\Terraria\Collision.cs | 52 | 3 | y | int | `public int y;` | `public int y;` |

##### 属性（0）

无该类型成员记录。


### 正式父级子系统：`DeathPenaltyAndRevenge`
- 父级职责：沿用源报告正式父级 `DeathPenaltyAndRevenge`；本分区只收录该父级中列出的叶子子系统。
- 本分区父级局部统计：字段 33；属性 2；合计 35；完整父级统计以源报告为准。

#### 4.2.1 细分子系统：`RevengeMarkerExpirationAndIdentityState`

- 细分职责：复仇标记的过期配置、唯一标识和标识投影。
- 边界角色：`authoritative state/behavior`；最小 seam：Revenge Marker Lifecycle/CommitPort；过期与身份由标记生命周期拥有。
- 成员文件数：1；声明类型数：1；字段：8；属性：1；合计：9。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（8）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 25 | field | Terraria.GameContent.CoinLossRevengeSystem.RevengeMarker | Terraria.GameContent/CoinLossRevengeSystem.cs | D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs | 18 | 3 | _uniqueIDCounter | int | `private static int _uniqueIDCounter = 0;` | `private static int _uniqueIDCounter = 0;` |
| 26 | field | Terraria.GameContent.CoinLossRevengeSystem.RevengeMarker | Terraria.GameContent/CoinLossRevengeSystem.cs | D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs | 20 | 3 | _expirationCompCopper | int | `private static readonly int _expirationCompCopper = Item.buyPrice(0, 0, 0, 1);` | `private static readonly int _expirationCompCopper = Item.buyPrice(0, 0, 0, 1);` |
| 27 | field | Terraria.GameContent.CoinLossRevengeSystem.RevengeMarker | Terraria.GameContent/CoinLossRevengeSystem.cs | D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs | 22 | 3 | _expirationCompSilver | int | `private static readonly int _expirationCompSilver = Item.buyPrice(0, 0, 1);` | `private static readonly int _expirationCompSilver = Item.buyPrice(0, 0, 1);` |
| 28 | field | Terraria.GameContent.CoinLossRevengeSystem.RevengeMarker | Terraria.GameContent/CoinLossRevengeSystem.cs | D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs | 24 | 3 | _expirationCompGold | int | `private static readonly int _expirationCompGold = Item.buyPrice(0, 1);` | `private static readonly int _expirationCompGold = Item.buyPrice(0, 1);` |
| 29 | field | Terraria.GameContent.CoinLossRevengeSystem.RevengeMarker | Terraria.GameContent/CoinLossRevengeSystem.cs | D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs | 26 | 3 | _expirationCompPlat | int | `private static readonly int _expirationCompPlat = Item.buyPrice(1);` | `private static readonly int _expirationCompPlat = Item.buyPrice(1);` |
| 30 | field | Terraria.GameContent.CoinLossRevengeSystem.RevengeMarker | Terraria.GameContent/CoinLossRevengeSystem.cs | D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs | 28 | 3 | ONE_MINUTE | int | `private const int ONE_MINUTE = 3600;` | `private const int ONE_MINUTE = 3600;` |
| 42 | field | Terraria.GameContent.CoinLossRevengeSystem.RevengeMarker | Terraria.GameContent/CoinLossRevengeSystem.cs | D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs | 52 | 3 | _expirationTime | int | `private readonly int _expirationTime;` | `private readonly int _expirationTime;` |
| 44 | field | Terraria.GameContent.CoinLossRevengeSystem.RevengeMarker | Terraria.GameContent/CoinLossRevengeSystem.cs | D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs | 56 | 3 | _uniqueID | int | `private readonly int _uniqueID;` | `private readonly int _uniqueID;` |

##### 属性（1）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 59 | property | Terraria.GameContent.CoinLossRevengeSystem.RevengeMarker | Terraria.GameContent/CoinLossRevengeSystem.cs | D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs | 64 | 3 | UniqueID | int | `public int UniqueID => _uniqueID;` | `public int UniqueID => _uniqueID;` |


#### 4.2.2 细分子系统：`RevengeMarkerEnemyContextState`

- 细分职责：敌人位置、碰撞框、生命比例和敌人类型上下文。
- 边界角色：`authoritative state/behavior`；最小 seam：Revenge Marker Context/Query；敌人上下文只读提供给复生资格。
- 成员文件数：1；声明类型数：1；字段：10；属性：0；合计：10。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（10）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 31 | field | Terraria.GameContent.CoinLossRevengeSystem.RevengeMarker | Terraria.GameContent/CoinLossRevengeSystem.cs | D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs | 30 | 3 | ENEMY_BOX_WIDTH | int | `private const int ENEMY_BOX_WIDTH = 2160;` | `private const int ENEMY_BOX_WIDTH = 2160;` |
| 32 | field | Terraria.GameContent.CoinLossRevengeSystem.RevengeMarker | Terraria.GameContent/CoinLossRevengeSystem.cs | D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs | 32 | 3 | ENEMY_BOX_HEIGHT | int | `private const int ENEMY_BOX_HEIGHT = 1440;` | `private const int ENEMY_BOX_HEIGHT = 1440;` |
| 33 | field | Terraria.GameContent.CoinLossRevengeSystem.RevengeMarker | Terraria.GameContent/CoinLossRevengeSystem.cs | D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs | 34 | 3 | EnemyBoxSize | Vector2 | `public static readonly Vector2 EnemyBoxSize = new Vector2(2160f, 1440f);` | `public static readonly Vector2 EnemyBoxSize = new Vector2(2160f, 1440f);` |
| 34 | field | Terraria.GameContent.CoinLossRevengeSystem.RevengeMarker | Terraria.GameContent/CoinLossRevengeSystem.cs | D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs | 36 | 3 | _location | Vector2 | `private readonly Vector2 _location;` | `private readonly Vector2 _location;` |
| 35 | field | Terraria.GameContent.CoinLossRevengeSystem.RevengeMarker | Terraria.GameContent/CoinLossRevengeSystem.cs | D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs | 38 | 3 | _hitbox | Rectangle | `private readonly Rectangle _hitbox;` | `private readonly Rectangle _hitbox;` |
| 36 | field | Terraria.GameContent.CoinLossRevengeSystem.RevengeMarker | Terraria.GameContent/CoinLossRevengeSystem.cs | D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs | 40 | 3 | _npcNetID | int | `private readonly int _npcNetID;` | `private readonly int _npcNetID;` |
| 37 | field | Terraria.GameContent.CoinLossRevengeSystem.RevengeMarker | Terraria.GameContent/CoinLossRevengeSystem.cs | D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs | 42 | 3 | _npcHPPercent | float | `private readonly float _npcHPPercent;` | `private readonly float _npcHPPercent;` |
| 40 | field | Terraria.GameContent.CoinLossRevengeSystem.RevengeMarker | Terraria.GameContent/CoinLossRevengeSystem.cs | D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs | 48 | 3 | _npcTypeAgainstDiscouragement | int | `private readonly int _npcTypeAgainstDiscouragement;` | `private readonly int _npcTypeAgainstDiscouragement;` |
| 41 | field | Terraria.GameContent.CoinLossRevengeSystem.RevengeMarker | Terraria.GameContent/CoinLossRevengeSystem.cs | D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs | 50 | 3 | _npcAIStyleAgainstDiscouragement | int | `private readonly int _npcAIStyleAgainstDiscouragement;` | `private readonly int _npcAIStyleAgainstDiscouragement;` |
| 43 | field | Terraria.GameContent.CoinLossRevengeSystem.RevengeMarker | Terraria.GameContent/CoinLossRevengeSystem.cs | D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs | 54 | 3 | _spawnedFromStatue | bool | `private readonly bool _spawnedFromStatue;` | `private readonly bool _spawnedFromStatue;` |

##### 属性（0）

无该类型成员记录。


#### 4.2.3 细分子系统：`RevengeMarkerValueAndRespawnState`

- 细分职责：金币价值和过期/复生尝试控制状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Revenge Respawn System/CommitPort；复生尝试与价值结算显式交接。
- 成员文件数：1；声明类型数：1；字段：4；属性：1；合计：5。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（4）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 38 | field | Terraria.GameContent.CoinLossRevengeSystem.RevengeMarker | Terraria.GameContent/CoinLossRevengeSystem.cs | D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs | 44 | 3 | _baseValue | float | `private readonly float _baseValue;` | `private readonly float _baseValue;` |
| 39 | field | Terraria.GameContent.CoinLossRevengeSystem.RevengeMarker | Terraria.GameContent/CoinLossRevengeSystem.cs | D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs | 46 | 3 | _coinsValue | int | `private readonly int _coinsValue;` | `private readonly int _coinsValue;` |
| 45 | field | Terraria.GameContent.CoinLossRevengeSystem.RevengeMarker | Terraria.GameContent/CoinLossRevengeSystem.cs | D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs | 58 | 3 | _forceExpire | bool | `private bool _forceExpire;` | `private bool _forceExpire;` |
| 46 | field | Terraria.GameContent.CoinLossRevengeSystem.RevengeMarker | Terraria.GameContent/CoinLossRevengeSystem.cs | D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs | 60 | 3 | _attemptedRespawn | bool | `private bool _attemptedRespawn;` | `private bool _attemptedRespawn;` |

##### 属性（1）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 58 | property | Terraria.GameContent.CoinLossRevengeSystem.RevengeMarker | Terraria.GameContent/CoinLossRevengeSystem.cs | D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs | 62 | 3 | RespawnAttemptLocked | bool | `public bool RespawnAttemptLocked => _attemptedRespawn;` | `public bool RespawnAttemptLocked => _attemptedRespawn;` |


#### 4.2.4 细分子系统：`RevengeRegistryAndCache`

- 细分职责：复仇标记注册表、缓存阈值、锁和时间推进。
- 边界角色：`registry/projection`；最小 seam：Registry/Projection seam；提交后的事实单向输出，不把网络/UI/索引反写成权威状态。
- 成员文件数：1；声明类型数：1；字段：11；属性：0；合计：11。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（11）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 47 | field | Terraria.GameContent.CoinLossRevengeSystem | Terraria.GameContent/CoinLossRevengeSystem.cs | D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs | 279 | 2 | DisplayCaching | bool | `public static bool DisplayCaching = false;` | `public static bool DisplayCaching = false;` |
| 48 | field | Terraria.GameContent.CoinLossRevengeSystem | Terraria.GameContent/CoinLossRevengeSystem.cs | D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs | 281 | 2 | MinimumCoinsForCaching | int | `public static int MinimumCoinsForCaching = Item.buyPrice(0, 0, 10);` | `public static int MinimumCoinsForCaching = Item.buyPrice(0, 0, 10);` |
| 49 | field | Terraria.GameContent.CoinLossRevengeSystem | Terraria.GameContent/CoinLossRevengeSystem.cs | D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs | 283 | 2 | PLAYER_BOX_WIDTH_INNER | int | `private const int PLAYER_BOX_WIDTH_INNER = 1968;` | `private const int PLAYER_BOX_WIDTH_INNER = 1968;` |
| 50 | field | Terraria.GameContent.CoinLossRevengeSystem | Terraria.GameContent/CoinLossRevengeSystem.cs | D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs | 285 | 2 | PLAYER_BOX_HEIGHT_INNER | int | `private const int PLAYER_BOX_HEIGHT_INNER = 1200;` | `private const int PLAYER_BOX_HEIGHT_INNER = 1200;` |
| 51 | field | Terraria.GameContent.CoinLossRevengeSystem | Terraria.GameContent/CoinLossRevengeSystem.cs | D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs | 287 | 2 | PLAYER_BOX_WIDTH_OUTER | int | `private const int PLAYER_BOX_WIDTH_OUTER = 2608;` | `private const int PLAYER_BOX_WIDTH_OUTER = 2608;` |
| 52 | field | Terraria.GameContent.CoinLossRevengeSystem | Terraria.GameContent/CoinLossRevengeSystem.cs | D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs | 289 | 2 | PLAYER_BOX_HEIGHT_OUTER | int | `private const int PLAYER_BOX_HEIGHT_OUTER = 1840;` | `private const int PLAYER_BOX_HEIGHT_OUTER = 1840;` |
| 53 | field | Terraria.GameContent.CoinLossRevengeSystem | Terraria.GameContent/CoinLossRevengeSystem.cs | D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs | 291 | 2 | _playerBoxSizeInner | Vector2 | `private static readonly Vector2 _playerBoxSizeInner = new Vector2(1968f, 1200f);` | `private static readonly Vector2 _playerBoxSizeInner = new Vector2(1968f, 1200f);` |
| 54 | field | Terraria.GameContent.CoinLossRevengeSystem | Terraria.GameContent/CoinLossRevengeSystem.cs | D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs | 293 | 2 | _playerBoxSizeOuter | Vector2 | `private static readonly Vector2 _playerBoxSizeOuter = new Vector2(2608f, 1840f);` | `private static readonly Vector2 _playerBoxSizeOuter = new Vector2(2608f, 1840f);` |
| 55 | field | Terraria.GameContent.CoinLossRevengeSystem | Terraria.GameContent/CoinLossRevengeSystem.cs | D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs | 295 | 2 | _markers | System.Collections.Generic.List<Terraria.GameContent.CoinLossRevengeSystem.RevengeMarker> | `private List<RevengeMarker> _markers;` | `private List<RevengeMarker> _markers;` |
| 56 | field | Terraria.GameContent.CoinLossRevengeSystem | Terraria.GameContent/CoinLossRevengeSystem.cs | D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs | 297 | 2 | _markersLock | object | `private readonly object _markersLock = new object();` | `private readonly object _markersLock = new object();` |
| 57 | field | Terraria.GameContent.CoinLossRevengeSystem | Terraria.GameContent/CoinLossRevengeSystem.cs | D:\TRbackup\Version4\Terraria.GameContent\CoinLossRevengeSystem.cs | 299 | 2 | _gameTime | int | `private int _gameTime;` | `private int _gameTime;` |

##### 属性（0）

无该类型成员记录。


### 正式父级子系统：`TeleportationAndTraversal`
- 父级职责：沿用源报告正式父级 `TeleportationAndTraversal`；本分区只收录该父级中列出的叶子子系统。
- 本分区父级局部统计：字段 5；属性 0；合计 5；完整父级统计以源报告为准。

#### 4.16.1 细分子系统：`TeleportPylonRegistry`

- 细分职责：传送水晶塔注册、刷新冷却和场景指标快照。
- 边界角色：`registry/projection`；最小 seam：Registry/Projection seam；提交后的事实单向输出，不把网络/UI/索引反写成权威状态。
- 成员文件数：1；声明类型数：1；字段：5；属性：0；合计：5。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（5）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2037 | field | Terraria.GameContent.TeleportPylonsSystem | Terraria.GameContent/TeleportPylonsSystem.cs | D:\TRbackup\Version4\Terraria.GameContent\TeleportPylonsSystem.cs | 15 | 2 | _pylons | System.Collections.Generic.List<Terraria.GameContent.TeleportPylonInfo> | `private List<TeleportPylonInfo> _pylons = new List<TeleportPylonInfo>();` | `private List<TeleportPylonInfo> _pylons = new List<TeleportPylonInfo>();` |
| 2038 | field | Terraria.GameContent.TeleportPylonsSystem | Terraria.GameContent/TeleportPylonsSystem.cs | D:\TRbackup\Version4\Terraria.GameContent\TeleportPylonsSystem.cs | 17 | 2 | _pylonsOld | System.Collections.Generic.List<Terraria.GameContent.TeleportPylonInfo> | `private List<TeleportPylonInfo> _pylonsOld = new List<TeleportPylonInfo>();` | `private List<TeleportPylonInfo> _pylonsOld = new List<TeleportPylonInfo>();` |
| 2039 | field | Terraria.GameContent.TeleportPylonsSystem | Terraria.GameContent/TeleportPylonsSystem.cs | D:\TRbackup\Version4\Terraria.GameContent\TeleportPylonsSystem.cs | 19 | 2 | _cooldownForUpdatingPylonsList | int | `private int _cooldownForUpdatingPylonsList;` | `private int _cooldownForUpdatingPylonsList;` |
| 2040 | field | Terraria.GameContent.TeleportPylonsSystem | Terraria.GameContent/TeleportPylonsSystem.cs | D:\TRbackup\Version4\Terraria.GameContent\TeleportPylonsSystem.cs | 21 | 2 | CooldownTimePerPylonsListUpdate | int | `private const int CooldownTimePerPylonsListUpdate = int.MaxValue;` | `private const int CooldownTimePerPylonsListUpdate = int.MaxValue;` |
| 2041 | field | Terraria.GameContent.TeleportPylonsSystem | Terraria.GameContent/TeleportPylonsSystem.cs | D:\TRbackup\Version4\Terraria.GameContent\TeleportPylonsSystem.cs | 23 | 2 | _sceneMetrics | Terraria.SceneMetrics | `private SceneMetrics _sceneMetrics = new SceneMetrics();` | `private SceneMetrics _sceneMetrics = new SceneMetrics();` |

##### 属性（0）

无该类型成员记录。


## 8. 本分区自检

- 叶子子系统：14 个；指定组均已写入且没有重复分区。
- 字段/属性/成员：111 / 2 / 113。
- 来源序号范围：1..2041；本分区内唯一序号：True。
- 成员声明表逐组从源报告提取，未改写来源序号、类型、路径、行列、成员、C# 类型、成员声明或原始声明。
- 运行时读写者、生命周期、持久化、网络和调度：未验证，保留为迁移前风险。
