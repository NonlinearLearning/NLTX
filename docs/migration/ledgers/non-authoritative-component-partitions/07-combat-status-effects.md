# Version4 非权威组件拆分分区 07/20：战斗、伤害与状态效果

> 来源报告：`docs/migration/ledgers/Version4非权威模拟系统字段属性逐成员源码声明-更细子系统拆分-去除ID类文件.md`
> 来源报告 SHA-256：`b944441d92d5103f218b766a3e354200794528a19e65a862e44c2b40a36f1196`
> 本文件是并行组件拆分工作包，不是新的权威成员清单；成员声明事实直接保留来源报告原文。

## 1. 分区定位与评估

- 分区范围：战斗目标、免疫、命中、伤害能力、标签效果、锁定和战斗文本。
- 本分区组件化重点：分离权威战斗状态、纯资格查询、伤害命令和客户端表现。
- 本分区包含 6 个完整细分子系统、97 条成员记录（字段 91、属性 6）。来源序号覆盖区间 `1083..3870`，区间可能与其他分区交错，不以序号定义领域边界。
- 组件边界判定：细分子系统只是并行处理的最小库存边界，不等价于一个组件。必须继续按共同读者/写者、变更原因、生命周期、权威性和副作用分为 Component、System、Query、Command、Adapter 或 Projection。
- 证据状态：成员声明和统计已由来源报告校验；逐成员读者、写者、初始化/清理路径、持久化/网络语义和系统顺序仍需本分区会话补证，不得从名称或数量推断。

## 2. 分区统计

| 父级子系统 | 细分子系统数 | 字段 | 属性 | 合计 |
|---|---:|---:|---:|---:|
| `SharedRuntimeMechanisms` | 6 | 91 | 6 | 97 |

| 原报告章节 | 父级 | 细分子系统 | 边界角色 | 字段 | 属性 | 合计 | 组件化处理 |
|---|---|---|---|---:|---:|---:|---|
| `4.9.34` | `SharedRuntimeMechanisms` | `SharedCombatTargetingAndImmunity` | query/state | 15 | 3 | 18 | 待按成员访问模式拆分 |
| `4.9.36` | `SharedRuntimeMechanisms` | `SharedHitTileTracking` | state/query | 17 | 0 | 17 | 待按成员访问模式拆分 |
| `4.9.91` | `SharedRuntimeMechanisms` | `ItemTagEffectState` | state/definition | 13 | 1 | 14 | 待按成员访问模式拆分 |
| `4.9.107` | `SharedRuntimeMechanisms` | `LockOnTargetingState` | adapter/query | 5 | 2 | 7 | 待按成员访问模式拆分 |
| `4.9.119` | `SharedRuntimeMechanisms` | `SharedCombatTextState` | presentation | 23 | 0 | 23 | 待按成员访问模式拆分 |
| `4.9.125` | `SharedRuntimeMechanisms` | `SharedItemCombatAndDamageCapabilityState` | definition/state | 18 | 0 | 18 | 待按成员访问模式拆分 |

## 3. 并行组件拆分契约

对本文件中的每个细分子系统和每条成员记录执行以下判断，并把结果写入对应的实现阶段设计/审计记录：

- 盘点声明类型、可见性、读者、写者、创建/更新/清理/持久化生命周期、访问频率和副作用；缺证据标记 `missing`，不可用命名猜测补齐。
- 标记权威状态、派生值、缓存、快照、兼容字段和表现数据；Component 只保存一个内聚概念的权威数据。
- 需要行为转换时放入显式 System/Command；纯资格判断和派生计算放入 Query；外部协议、文件、平台和第三方类型放入 Adapter；网络/持久化/日志/UI 输出放入单向 Projection。
- 分别建模实体引用、持久化 ID、网络 ID 和外部 ID；不得把 ID 类文件或外部对象直接塞入共享组件。
- 记录 Interface、Implementation、Seam、Depth、Leverage、Locality，以及必要的系统先后约束、失败归属和重试策略。

## 4. 逐成员源码声明

以下细分子系统块保持来源报告的成员表、声明、路径、行列和来源序号；只调整 Markdown 标题层级并增加分区追踪元数据。

### 4.1 细分子系统：`SharedCombatTargetingAndImmunity`

- 原报告章节：`4.9.34`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`07` / `战斗、伤害与状态效果`

- 上一级基线细分子系统：`SharedCombatTargetingAndImmunity`
- 细分职责：目标、命中框、免疫和击杀尝试数据。
- 边界角色：`query/state`；最小 seam：combat target query；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：4；声明类型数：4；字段：15；属性：3；合计：18。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（15）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1191 | field | Terraria.DataStructures.MultiPointHitbox | Terraria.DataStructures/MultiPointHitbox.cs | D:\TRbackup\Version4\Terraria.DataStructures\MultiPointHitbox.cs | 7 | 2 | PointSize | Point | `public readonly Point PointSize;` | `public readonly Point PointSize;` |
| 1192 | field | Terraria.DataStructures.MultiPointHitbox | Terraria.DataStructures/MultiPointHitbox.cs | D:\TRbackup\Version4\Terraria.DataStructures\MultiPointHitbox.cs | 9 | 2 | Points | Vector2[] | `public readonly Vector2[] Points;` | `public readonly Vector2[] Points;` |
| 1193 | field | Terraria.DataStructures.MultiPointHitbox | Terraria.DataStructures/MultiPointHitbox.cs | D:\TRbackup\Version4\Terraria.DataStructures\MultiPointHitbox.cs | 11 | 2 | BoundingRect | Rectangle | `public readonly Rectangle BoundingRect;` | `public readonly Rectangle BoundingRect;` |
| 1196 | field | Terraria.DataStructures.NPCAimedTarget | Terraria.DataStructures/NPCAimedTarget.cs | D:\TRbackup\Version4\Terraria.DataStructures\NPCAimedTarget.cs | 8 | 2 | Type | Terraria.Enums.NPCTargetType | `public NPCTargetType Type;` | `public NPCTargetType Type;` |
| 1197 | field | Terraria.DataStructures.NPCAimedTarget | Terraria.DataStructures/NPCAimedTarget.cs | D:\TRbackup\Version4\Terraria.DataStructures\NPCAimedTarget.cs | 10 | 2 | Hitbox | Rectangle | `public Rectangle Hitbox;` | `public Rectangle Hitbox;` |
| 1198 | field | Terraria.DataStructures.NPCAimedTarget | Terraria.DataStructures/NPCAimedTarget.cs | D:\TRbackup\Version4\Terraria.DataStructures\NPCAimedTarget.cs | 12 | 2 | Width | int | `public int Width;` | `public int Width;` |
| 1199 | field | Terraria.DataStructures.NPCAimedTarget | Terraria.DataStructures/NPCAimedTarget.cs | D:\TRbackup\Version4\Terraria.DataStructures\NPCAimedTarget.cs | 14 | 2 | Height | int | `public int Height;` | `public int Height;` |
| 1200 | field | Terraria.DataStructures.NPCAimedTarget | Terraria.DataStructures/NPCAimedTarget.cs | D:\TRbackup\Version4\Terraria.DataStructures\NPCAimedTarget.cs | 16 | 2 | Position | Vector2 | `public Vector2 Position;` | `public Vector2 Position;` |
| 1201 | field | Terraria.DataStructures.NPCAimedTarget | Terraria.DataStructures/NPCAimedTarget.cs | D:\TRbackup\Version4\Terraria.DataStructures\NPCAimedTarget.cs | 18 | 2 | Velocity | Vector2 | `public Vector2 Velocity;` | `public Vector2 Velocity;` |
| 1202 | field | Terraria.DataStructures.NPCDebuffImmunityData | Terraria.DataStructures/NPCDebuffImmunityData.cs | D:\TRbackup\Version4\Terraria.DataStructures\NPCDebuffImmunityData.cs | 7 | 2 | ImmuneToWhips | bool | `public bool ImmuneToWhips;` | `public bool ImmuneToWhips;` |
| 1203 | field | Terraria.DataStructures.NPCDebuffImmunityData | Terraria.DataStructures/NPCDebuffImmunityData.cs | D:\TRbackup\Version4\Terraria.DataStructures\NPCDebuffImmunityData.cs | 9 | 2 | ImmuneToAllBuffsThatAreNotWhips | bool | `public bool ImmuneToAllBuffsThatAreNotWhips;` | `public bool ImmuneToAllBuffsThatAreNotWhips;` |
| 1204 | field | Terraria.DataStructures.NPCDebuffImmunityData | Terraria.DataStructures/NPCDebuffImmunityData.cs | D:\TRbackup\Version4\Terraria.DataStructures\NPCDebuffImmunityData.cs | 11 | 2 | SpecificallyImmuneTo | int[] | `public int[] SpecificallyImmuneTo;` | `public int[] SpecificallyImmuneTo;` |
| 1205 | field | Terraria.DataStructures.NPCKillAttempt | Terraria.DataStructures/NPCKillAttempt.cs | D:\TRbackup\Version4\Terraria.DataStructures\NPCKillAttempt.cs | 5 | 2 | npc | Terraria.NPC | `public readonly NPC npc;` | `public readonly NPC npc;` |
| 1206 | field | Terraria.DataStructures.NPCKillAttempt | Terraria.DataStructures/NPCKillAttempt.cs | D:\TRbackup\Version4\Terraria.DataStructures\NPCKillAttempt.cs | 7 | 2 | netId | int | `public readonly int netId;` | `public readonly int netId;` |
| 1207 | field | Terraria.DataStructures.NPCKillAttempt | Terraria.DataStructures/NPCKillAttempt.cs | D:\TRbackup\Version4\Terraria.DataStructures\NPCKillAttempt.cs | 9 | 2 | active | bool | `public readonly bool active;` | `public readonly bool active;` |

#### 属性（3）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3695 | property | Terraria.DataStructures.NPCAimedTarget | Terraria.DataStructures/NPCAimedTarget.cs | D:\TRbackup\Version4\Terraria.DataStructures\NPCAimedTarget.cs | 20 | 2 | Invalid | bool | `public bool Invalid => Type == NPCTargetType.None;` | `public bool Invalid => Type == NPCTargetType.None;` |
| 3696 | property | Terraria.DataStructures.NPCAimedTarget | Terraria.DataStructures/NPCAimedTarget.cs | D:\TRbackup\Version4\Terraria.DataStructures\NPCAimedTarget.cs | 22 | 2 | Center | Vector2 | `public Vector2 Center => Position + Size / 2f;` | `public Vector2 Center => Position + Size / 2f;` |
| 3697 | property | Terraria.DataStructures.NPCAimedTarget | Terraria.DataStructures/NPCAimedTarget.cs | D:\TRbackup\Version4\Terraria.DataStructures\NPCAimedTarget.cs | 24 | 2 | Size | Vector2 | `public Vector2 Size => new Vector2(Width, Height);` | `public Vector2 Size => new Vector2(Width, Height);` |


### 4.2 细分子系统：`SharedHitTileTracking`

- 原报告章节：`4.9.36`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`07` / `战斗、伤害与状态效果`

- 上一级基线细分子系统：`SharedHitTileTracking`
- 细分职责：Tile 命中和挖掘追踪状态。
- 边界角色：`state/query`；最小 seam：hit tile tracking；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：2；字段：17；属性：0；合计：17。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（17）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3120 | field | Terraria.HitTile.HitTileObject | Terraria/HitTile.cs | D:\TRbackup\Version4\Terraria\HitTile.cs | 13 | 3 | X | int | `public int X;` | `public int X;` |
| 3121 | field | Terraria.HitTile.HitTileObject | Terraria/HitTile.cs | D:\TRbackup\Version4\Terraria\HitTile.cs | 15 | 3 | Y | int | `public int Y;` | `public int Y;` |
| 3122 | field | Terraria.HitTile.HitTileObject | Terraria/HitTile.cs | D:\TRbackup\Version4\Terraria\HitTile.cs | 17 | 3 | damage | int | `public int damage;` | `public int damage;` |
| 3123 | field | Terraria.HitTile.HitTileObject | Terraria/HitTile.cs | D:\TRbackup\Version4\Terraria\HitTile.cs | 19 | 3 | type | int | `public int type;` | `public int type;` |
| 3124 | field | Terraria.HitTile.HitTileObject | Terraria/HitTile.cs | D:\TRbackup\Version4\Terraria\HitTile.cs | 21 | 3 | timeToLive | int | `public int timeToLive;` | `public int timeToLive;` |
| 3125 | field | Terraria.HitTile.HitTileObject | Terraria/HitTile.cs | D:\TRbackup\Version4\Terraria\HitTile.cs | 23 | 3 | crackStyle | int | `public int crackStyle;` | `public int crackStyle;` |
| 3126 | field | Terraria.HitTile.HitTileObject | Terraria/HitTile.cs | D:\TRbackup\Version4\Terraria\HitTile.cs | 25 | 3 | animationTimeElapsed | int | `public int animationTimeElapsed;` | `public int animationTimeElapsed;` |
| 3127 | field | Terraria.HitTile.HitTileObject | Terraria/HitTile.cs | D:\TRbackup\Version4\Terraria\HitTile.cs | 27 | 3 | animationDirection | Vector2 | `public Vector2 animationDirection;` | `public Vector2 animationDirection;` |
| 3128 | field | Terraria.HitTile | Terraria/HitTile.cs | D:\TRbackup\Version4\Terraria\HitTile.cs | 54 | 2 | UNUSED | int | `internal const int UNUSED = 0;` | `internal const int UNUSED = 0;` |
| 3129 | field | Terraria.HitTile | Terraria/HitTile.cs | D:\TRbackup\Version4\Terraria\HitTile.cs | 56 | 2 | TILE | int | `internal const int TILE = 1;` | `internal const int TILE = 1;` |
| 3130 | field | Terraria.HitTile | Terraria/HitTile.cs | D:\TRbackup\Version4\Terraria\HitTile.cs | 58 | 2 | WALL | int | `internal const int WALL = 2;` | `internal const int WALL = 2;` |
| 3131 | field | Terraria.HitTile | Terraria/HitTile.cs | D:\TRbackup\Version4\Terraria\HitTile.cs | 60 | 2 | MAX_HITTILES | int | `internal const int MAX_HITTILES = 500;` | `internal const int MAX_HITTILES = 500;` |
| 3132 | field | Terraria.HitTile | Terraria/HitTile.cs | D:\TRbackup\Version4\Terraria\HitTile.cs | 62 | 2 | TIMETOLIVE | int | `internal const int TIMETOLIVE = 60;` | `internal const int TIMETOLIVE = 60;` |
| 3133 | field | Terraria.HitTile | Terraria/HitTile.cs | D:\TRbackup\Version4\Terraria\HitTile.cs | 64 | 2 | rand | Terraria.Utilities.UnifiedRandom | `private static UnifiedRandom rand;` | `private static UnifiedRandom rand;` |
| 3134 | field | Terraria.HitTile | Terraria/HitTile.cs | D:\TRbackup\Version4\Terraria\HitTile.cs | 66 | 2 | lastCrack | int | `private static int lastCrack = -1;` | `private static int lastCrack = -1;` |
| 3135 | field | Terraria.HitTile | Terraria/HitTile.cs | D:\TRbackup\Version4\Terraria\HitTile.cs | 68 | 2 | data | Terraria.HitTile.HitTileObject[] | `public HitTileObject[] data;` | `public HitTileObject[] data;` |
| 3136 | field | Terraria.HitTile | Terraria/HitTile.cs | D:\TRbackup\Version4\Terraria\HitTile.cs | 70 | 2 | order | int[] | `private int[] order;` | `private int[] order;` |

#### 属性（0）

无该类型成员记录。


### 4.3 细分子系统：`ItemTagEffectState`

- 原报告章节：`4.9.91`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`07` / `战斗、伤害与状态效果`

- 上一级基线细分子系统：`ItemTagEffectState`
- 细分职责：鞭子标签效果、唯一标签效果和效果状态。
- 边界角色：`state/definition`；最小 seam：item tag effect port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：3；声明类型数：3；字段：13；属性：1；合计：14。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（13）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2117 | field | Terraria.GameContent.Items.TagEffectState | Terraria.GameContent.Items/TagEffectState.cs | D:\TRbackup\Version4\Terraria.GameContent.Items\TagEffectState.cs | 96 | 2 | _owner | Terraria.Player | `private readonly Player _owner;` | `private readonly Player _owner;` |
| 2118 | field | Terraria.GameContent.Items.TagEffectState | Terraria.GameContent.Items/TagEffectState.cs | D:\TRbackup\Version4\Terraria.GameContent.Items\TagEffectState.cs | 98 | 2 | _effect | Terraria.GameContent.Items.UniqueTagEffect | `private UniqueTagEffect _effect;` | `private UniqueTagEffect _effect;` |
| 2119 | field | Terraria.GameContent.Items.TagEffectState | Terraria.GameContent.Items/TagEffectState.cs | D:\TRbackup\Version4\Terraria.GameContent.Items\TagEffectState.cs | 100 | 2 | TimeLeftOnNPC | int[] | `private readonly int[] TimeLeftOnNPC = new int[Main.maxNPCs];` | `private readonly int[] TimeLeftOnNPC = new int[Main.maxNPCs];` |
| 2120 | field | Terraria.GameContent.Items.TagEffectState | Terraria.GameContent.Items/TagEffectState.cs | D:\TRbackup\Version4\Terraria.GameContent.Items\TagEffectState.cs | 102 | 2 | ProcTimeLeftOnNPC | int[] | `private readonly int[] ProcTimeLeftOnNPC = new int[Main.maxNPCs];` | `private readonly int[] ProcTimeLeftOnNPC = new int[Main.maxNPCs];` |
| 2121 | field | Terraria.GameContent.Items.UniqueTagEffect | Terraria.GameContent.Items/UniqueTagEffect.cs | D:\TRbackup\Version4\Terraria.GameContent.Items\UniqueTagEffect.cs | 5 | 2 | NetSync | bool | `public bool NetSync;` | `public bool NetSync;` |
| 2122 | field | Terraria.GameContent.Items.UniqueTagEffect | Terraria.GameContent.Items/UniqueTagEffect.cs | D:\TRbackup\Version4\Terraria.GameContent.Items\UniqueTagEffect.cs | 7 | 2 | SyncProcs | bool | `public bool SyncProcs;` | `public bool SyncProcs;` |
| 2123 | field | Terraria.GameContent.Items.UniqueTagEffect | Terraria.GameContent.Items/UniqueTagEffect.cs | D:\TRbackup\Version4\Terraria.GameContent.Items\UniqueTagEffect.cs | 9 | 2 | TagDuration | int | `public int TagDuration;` | `public int TagDuration;` |
| 2124 | field | Terraria.GameContent.Items.WhipTagEffect | Terraria.GameContent.Items/WhipTagEffect.cs | D:\TRbackup\Version4\Terraria.GameContent.Items\WhipTagEffect.cs | 7 | 2 | PlayerBuffId | int | `public int PlayerBuffId;` | `public int PlayerBuffId;` |
| 2125 | field | Terraria.GameContent.Items.WhipTagEffect | Terraria.GameContent.Items/WhipTagEffect.cs | D:\TRbackup\Version4\Terraria.GameContent.Items\WhipTagEffect.cs | 9 | 2 | PlayerBuffTime | int | `public int PlayerBuffTime;` | `public int PlayerBuffTime;` |
| 2126 | field | Terraria.GameContent.Items.WhipTagEffect | Terraria.GameContent.Items/WhipTagEffect.cs | D:\TRbackup\Version4\Terraria.GameContent.Items\WhipTagEffect.cs | 11 | 2 | PlayerBuffAppliedManually | bool | `public bool PlayerBuffAppliedManually;` | `public bool PlayerBuffAppliedManually;` |
| 2127 | field | Terraria.GameContent.Items.WhipTagEffect | Terraria.GameContent.Items/WhipTagEffect.cs | D:\TRbackup\Version4\Terraria.GameContent.Items\WhipTagEffect.cs | 13 | 2 | CritChance | int | `public int CritChance;` | `public int CritChance;` |
| 2128 | field | Terraria.GameContent.Items.WhipTagEffect | Terraria.GameContent.Items/WhipTagEffect.cs | D:\TRbackup\Version4\Terraria.GameContent.Items\WhipTagEffect.cs | 15 | 2 | TagDamage | int | `public int TagDamage;` | `public int TagDamage;` |
| 2129 | field | Terraria.GameContent.Items.WhipTagEffect | Terraria.GameContent.Items/WhipTagEffect.cs | D:\TRbackup\Version4\Terraria.GameContent.Items\WhipTagEffect.cs | 17 | 2 | generalWhipMarkDuration | int | `private const int generalWhipMarkDuration = 240;` | `private const int generalWhipMarkDuration = 240;` |

#### 属性（1）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3822 | property | Terraria.GameContent.Items.TagEffectState | Terraria.GameContent.Items/TagEffectState.cs | D:\TRbackup\Version4\Terraria.GameContent.Items\TagEffectState.cs | 104 | 2 | Type | int | `public int Type { get; private set; }` | `public int Type { get; private set; }` |


### 4.4 细分子系统：`LockOnTargetingState`

- 原报告章节：`4.9.107`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`07` / `战斗、伤害与状态效果`

- 上一级基线细分子系统：`LockOnTargetingState`
- 细分职责：锁定范围、保持时间和目标选择状态。
- 边界角色：`adapter/query`；最小 seam：lock-on targeting port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：5；属性：2；合计：7。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（5）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2601 | field | Terraria.GameInput.LockOnHelper | Terraria.GameInput/LockOnHelper.cs | D:\TRbackup\Version4\Terraria.GameInput\LockOnHelper.cs | 13 | 2 | LOCKON_RANGE | float | `private const float LOCKON_RANGE = 2000f;` | `private const float LOCKON_RANGE = 2000f;` |
| 2602 | field | Terraria.GameInput.LockOnHelper | Terraria.GameInput/LockOnHelper.cs | D:\TRbackup\Version4\Terraria.GameInput\LockOnHelper.cs | 15 | 2 | LOCKON_HOLD_LIFETIME | int | `private const int LOCKON_HOLD_LIFETIME = 40;` | `private const int LOCKON_HOLD_LIFETIME = 40;` |
| 2603 | field | Terraria.GameInput.LockOnHelper | Terraria.GameInput/LockOnHelper.cs | D:\TRbackup\Version4\Terraria.GameInput\LockOnHelper.cs | 17 | 2 | _canLockOn | bool | `private static bool _canLockOn;` | `private static bool _canLockOn;` |
| 2604 | field | Terraria.GameInput.LockOnHelper | Terraria.GameInput/LockOnHelper.cs | D:\TRbackup\Version4\Terraria.GameInput\LockOnHelper.cs | 19 | 2 | _targets | System.Collections.Generic.List<int> | `private static List<int> _targets = new List<int>();` | `private static List<int> _targets = new List<int>();` |
| 2605 | field | Terraria.GameInput.LockOnHelper | Terraria.GameInput/LockOnHelper.cs | D:\TRbackup\Version4\Terraria.GameInput\LockOnHelper.cs | 21 | 2 | _pickedTarget | int | `private static int _pickedTarget;` | `private static int _pickedTarget;` |

#### 属性（2）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3869 | property | Terraria.GameInput.LockOnHelper | Terraria.GameInput/LockOnHelper.cs | D:\TRbackup\Version4\Terraria.GameInput\LockOnHelper.cs | 23 | 2 | AimedTarget | Terraria.NPC | `public static NPC AimedTarget { get { if (_pickedTarget == -1 \|\| _targets.Count < 1) { return null; } return Main.npc[_targets[_pickedTarget]]; } }` | `public static NPC AimedTarget { get { if (_pickedTarget == -1 \|\| _targets.Count < 1) { return null; } return Main.npc[_targets[_pickedTarget]]; } }` |
| 3870 | property | Terraria.GameInput.LockOnHelper | Terraria.GameInput/LockOnHelper.cs | D:\TRbackup\Version4\Terraria.GameInput\LockOnHelper.cs | 35 | 2 | PredictedPosition | Vector2 | `public static Vector2 PredictedPosition { get { NPC aimedTarget = AimedTarget; if (aimedTarget == null) { return Vector2.Zero; } Vector2 vector = aimedTarget.Center; if (NPC.GetNPCLocation(_targets[_pickedTarget], seekHead: true, averageDirection: false, out var index, out var pos)) { vector = pos; vector += Main.npc[index].Distance(Main.player[Main.myPlayer].Center) / 2000f * Main.npc[index].velocity * 45f; } Player player = Main.player[Main.myPlayer]; int num = ItemID.Sets.LockOnAimAbove[player.inventory[player.selectedItem].type]; while (num > 0 && vector.Y > 100f) { Point point = vector.ToTileCoordinates(); point.Y -= 4; if (!WorldGen.InWorld(point.X, point.Y, 10) \|\| WorldGen.SolidTile(point.X, point.Y)) { break; } vector.Y -= 16f; num--; } float? num2 = ItemID.Sets.LockOnAimCompensation[player.inventory[player.selectedItem].type]; if (num2.HasValue) { vector.Y -= aimedTarget.height / 2; Vector2 v = vector - player.Center; Vector2 vector2 = v.SafeNormalize(Vector2.Zero); vector2.Y -= 1f; float num3 = v.Length(); num3 = (float)Math.Pow(num3 / 700f, 2.0) * 700f; vector.Y += vector2.Y * num3 * num2.Value * 1f; vector.X += (0f - vector2.X) * num3 * num2.Value * 1f; } return vector; } }` | `public static Vector2 PredictedPosition { get { NPC aimedTarget = AimedTarget; if (aimedTarget == null) { return Vector2.Zero; } Vector2 vector = aimedTarget.Center; if (NPC.GetNPCLocation(_targets[_pickedTarget], seekHead: true, averageDirection: false, out var index, out var pos)) { vector = pos; vector += Main.npc[index].Distance(Main.player[Main.myPlayer].Center) / 2000f * Main.npc[index].velocity * 45f; } Player player = Main.player[Main.myPlayer]; int num = ItemID.Sets.LockOnAimAbove[player.inventory[player.selectedItem].type]; while (num > 0 && vector.Y > 100f) { Point point = vector.ToTileCoordinates(); point.Y -= 4; if (!WorldGen.InWorld(point.X, point.Y, 10) \|\| WorldGen.SolidTile(point.X, point.Y)) { break; } vector.Y -= 16f; num--; } float? num2 = ItemID.Sets.LockOnAimCompensation[player.inventory[player.selectedItem].type]; if (num2.HasValue) { vector.Y -= aimedTarget.height / 2; Vector2 v = vector - player.Center; Vector2 vector2 = v.SafeNormalize(Vector2.Zero); vector2.Y -= 1f; float num3 = v.Length(); num3 = (float)Math.Pow(num3 / 700f, 2.0) * 700f; vector.Y += vector2.Y * num3 * num2.Value * 1f; vector.X += (0f - vector2.X) * num3 * num2.Value * 1f; } return vector; } }` |


### 4.5 细分子系统：`SharedCombatTextState`

- 原报告章节：`4.9.119`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`07` / `战斗、伤害与状态效果`

- 上一级基线细分子系统：`SharedPopupAndCombatText`
- 细分职责：伤害、治疗和暴击战斗文本状态。
- 边界角色：`presentation`；最小 seam：combat text presentation port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：2；声明类型数：2；字段：23；属性：0；合计：23。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（23）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1083 | field | Terraria.DataStructures.CachedProjectileCounterBuffTextHandler | Terraria.DataStructures/CachedProjectileCounterBuffTextHandler.cs | D:\TRbackup\Version4\Terraria.DataStructures\CachedProjectileCounterBuffTextHandler.cs | 5 | 2 | projectilesToLookFor | int[] | `private int[] projectilesToLookFor;` | `private int[] projectilesToLookFor;` |
| 3012 | field | Terraria.CombatText | Terraria/CombatText.cs | D:\TRbackup\Version4\Terraria\CombatText.cs | 8 | 2 | DamagedFriendly | Color | `public static readonly Color DamagedFriendly = new Color(255, 80, 90, 255);` | `public static readonly Color DamagedFriendly = new Color(255, 80, 90, 255);` |
| 3013 | field | Terraria.CombatText | Terraria/CombatText.cs | D:\TRbackup\Version4\Terraria\CombatText.cs | 10 | 2 | DamagedFriendlyCrit | Color | `public static readonly Color DamagedFriendlyCrit = new Color(255, 100, 30, 255);` | `public static readonly Color DamagedFriendlyCrit = new Color(255, 100, 30, 255);` |
| 3014 | field | Terraria.CombatText | Terraria/CombatText.cs | D:\TRbackup\Version4\Terraria\CombatText.cs | 12 | 2 | DamagedHostile | Color | `public static readonly Color DamagedHostile = new Color(255, 160, 80, 255);` | `public static readonly Color DamagedHostile = new Color(255, 160, 80, 255);` |
| 3015 | field | Terraria.CombatText | Terraria/CombatText.cs | D:\TRbackup\Version4\Terraria\CombatText.cs | 14 | 2 | DamagedHostileCrit | Color | `public static readonly Color DamagedHostileCrit = new Color(255, 100, 30, 255);` | `public static readonly Color DamagedHostileCrit = new Color(255, 100, 30, 255);` |
| 3016 | field | Terraria.CombatText | Terraria/CombatText.cs | D:\TRbackup\Version4\Terraria\CombatText.cs | 16 | 2 | OthersDamagedHostile | Color | `public static readonly Color OthersDamagedHostile = DamagedHostile * 0.4f;` | `public static readonly Color OthersDamagedHostile = DamagedHostile * 0.4f;` |
| 3017 | field | Terraria.CombatText | Terraria/CombatText.cs | D:\TRbackup\Version4\Terraria\CombatText.cs | 18 | 2 | OthersDamagedHostileCrit | Color | `public static readonly Color OthersDamagedHostileCrit = DamagedHostileCrit * 0.4f;` | `public static readonly Color OthersDamagedHostileCrit = DamagedHostileCrit * 0.4f;` |
| 3018 | field | Terraria.CombatText | Terraria/CombatText.cs | D:\TRbackup\Version4\Terraria\CombatText.cs | 20 | 2 | HealLife | Color | `public static readonly Color HealLife = new Color(100, 255, 100, 255);` | `public static readonly Color HealLife = new Color(100, 255, 100, 255);` |
| 3019 | field | Terraria.CombatText | Terraria/CombatText.cs | D:\TRbackup\Version4\Terraria\CombatText.cs | 22 | 2 | HealMana | Color | `public static readonly Color HealMana = new Color(100, 100, 255, 255);` | `public static readonly Color HealMana = new Color(100, 100, 255, 255);` |
| 3020 | field | Terraria.CombatText | Terraria/CombatText.cs | D:\TRbackup\Version4\Terraria\CombatText.cs | 24 | 2 | LifeRegen | Color | `public static readonly Color LifeRegen = new Color(255, 60, 70, 255);` | `public static readonly Color LifeRegen = new Color(255, 60, 70, 255);` |
| 3021 | field | Terraria.CombatText | Terraria/CombatText.cs | D:\TRbackup\Version4\Terraria\CombatText.cs | 26 | 2 | LifeRegenNegative | Color | `public static readonly Color LifeRegenNegative = new Color(255, 140, 40, 255);` | `public static readonly Color LifeRegenNegative = new Color(255, 140, 40, 255);` |
| 3022 | field | Terraria.CombatText | Terraria/CombatText.cs | D:\TRbackup\Version4\Terraria\CombatText.cs | 28 | 2 | position | Vector2 | `public Vector2 position;` | `public Vector2 position;` |
| 3023 | field | Terraria.CombatText | Terraria/CombatText.cs | D:\TRbackup\Version4\Terraria\CombatText.cs | 30 | 2 | velocity | Vector2 | `public Vector2 velocity;` | `public Vector2 velocity;` |
| 3024 | field | Terraria.CombatText | Terraria/CombatText.cs | D:\TRbackup\Version4\Terraria\CombatText.cs | 32 | 2 | alpha | float | `public float alpha;` | `public float alpha;` |
| 3025 | field | Terraria.CombatText | Terraria/CombatText.cs | D:\TRbackup\Version4\Terraria\CombatText.cs | 34 | 2 | alphaDir | int | `public int alphaDir = 1;` | `public int alphaDir = 1;` |
| 3026 | field | Terraria.CombatText | Terraria/CombatText.cs | D:\TRbackup\Version4\Terraria\CombatText.cs | 36 | 2 | text | string | `public string text = "";` | `public string text = "";` |
| 3027 | field | Terraria.CombatText | Terraria/CombatText.cs | D:\TRbackup\Version4\Terraria\CombatText.cs | 38 | 2 | scale | float | `public float scale = 1f;` | `public float scale = 1f;` |
| 3028 | field | Terraria.CombatText | Terraria/CombatText.cs | D:\TRbackup\Version4\Terraria\CombatText.cs | 40 | 2 | rotation | float | `public float rotation;` | `public float rotation;` |
| 3029 | field | Terraria.CombatText | Terraria/CombatText.cs | D:\TRbackup\Version4\Terraria\CombatText.cs | 42 | 2 | color | Color | `public Color color;` | `public Color color;` |
| 3030 | field | Terraria.CombatText | Terraria/CombatText.cs | D:\TRbackup\Version4\Terraria\CombatText.cs | 44 | 2 | active | bool | `public bool active;` | `public bool active;` |
| 3031 | field | Terraria.CombatText | Terraria/CombatText.cs | D:\TRbackup\Version4\Terraria\CombatText.cs | 46 | 2 | lifeTime | int | `public int lifeTime;` | `public int lifeTime;` |
| 3032 | field | Terraria.CombatText | Terraria/CombatText.cs | D:\TRbackup\Version4\Terraria\CombatText.cs | 48 | 2 | crit | bool | `public bool crit;` | `public bool crit;` |
| 3033 | field | Terraria.CombatText | Terraria/CombatText.cs | D:\TRbackup\Version4\Terraria\CombatText.cs | 50 | 2 | dot | bool | `public bool dot;` | `public bool dot;` |

#### 属性（0）

无该类型成员记录。


### 4.6 细分子系统：`SharedItemCombatAndDamageCapabilityState`

- 原报告章节：`4.9.125`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`07` / `战斗、伤害与状态效果`

- 上一级基线细分子系统：`SharedItemUseToolAndCombatState`
- 细分职责：物品伤害、命中、恢复和职业伤害能力。
- 边界角色：`definition/state`；最小 seam：item combat capability port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：18；属性：0；合计：18。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（18）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3206 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 156 | 2 | damage | int | `public int damage;` | `public int damage;` |
| 3207 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 158 | 2 | knockBack | float | `public float knockBack;` | `public float knockBack;` |
| 3208 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 160 | 2 | healLife | int | `public int healLife;` | `public int healLife;` |
| 3209 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 162 | 2 | healMana | int | `public int healMana;` | `public int healMana;` |
| 3240 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 224 | 2 | rare | int | `public int rare;` | `public int rare;` |
| 3241 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 226 | 2 | shoot | int | `public int shoot;` | `public int shoot;` |
| 3242 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 228 | 2 | shootSpeed | float | `public float shootSpeed;` | `public float shootSpeed;` |
| 3246 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 236 | 2 | lifeRegen | int | `public int lifeRegen;` | `public int lifeRegen;` |
| 3247 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 238 | 2 | manaIncrease | int | `public int manaIncrease;` | `public int manaIncrease;` |
| 3249 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 242 | 2 | mana | int | `public int mana;` | `public int mana;` |
| 3268 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 280 | 2 | crit | int | `public int crit;` | `public int crit;` |
| 3269 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 282 | 2 | armorPenetration | int | `public int armorPenetration;` | `public int armorPenetration;` |
| 3270 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 284 | 2 | bonusTagDamage | int | `public int bonusTagDamage;` | `public int bonusTagDamage;` |
| 3272 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 288 | 2 | melee | bool | `public bool melee;` | `public bool melee;` |
| 3273 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 290 | 2 | magic | bool | `public bool magic;` | `public bool magic;` |
| 3274 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 292 | 2 | ranged | bool | `public bool ranged;` | `public bool ranged;` |
| 3275 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 294 | 2 | summon | bool | `public bool summon;` | `public bool summon;` |
| 3276 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 296 | 2 | sentry | bool | `public bool sentry;` | `public bool sentry;` |

#### 属性（0）

无该类型成员记录。


## 5. 追溯与验收

> 来源报告 SHA-256：`b944441d92d5103f218b766a3e354200794528a19e65a862e44c2b40a36f1196`
- 本分区细分组数：6；成员数：97；字段：91；属性：6。
- 预期不变量：本文件只出现完整细分子系统；不跨分区复制成员；所有成员的来源序号、原始声明、路径、行列和 C# 类型必须与来源报告一致。
- 验证方式：重新读取 20 个文件，检查文件数、细分组唯一性、来源序号恰好覆盖 `1..4542`、字段/属性合计和逐成员行文本一致；本次分区生成不运行 `dotnet`，因为没有修改 C# 或项目文件。

生成器：`Build/Tools/Generate-Version4NonAuthoritativeComponentPartitionReports.ps1`。
