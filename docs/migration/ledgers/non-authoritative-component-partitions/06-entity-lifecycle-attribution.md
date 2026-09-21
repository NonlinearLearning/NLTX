# Version4 非权威组件拆分分区 06/20：实体生命周期与归因

> 来源报告：`docs/migration/ledgers/Version4非权威模拟系统字段属性逐成员源码声明-更细子系统拆分-去除ID类文件.md`
> 来源报告 SHA-256：`b944441d92d5103f218b766a3e354200794528a19e65a862e44c2b40a36f1196`
> 本文件是并行组件拆分工作包，不是新的权威成员清单；成员声明事实直接保留来源报告原文。

## 1. 分区定位与评估

- 分区范围：实体槽位、身份/运动基础状态、来源归因及世界物品生命周期关系。
- 本分区组件化重点：分别建模实体引用、持久化 ID、网络 ID 和外部 ID，确认创建/销毁所有权。
- 本分区包含 3 个完整细分子系统、44 条成员记录（字段 43、属性 1）。来源序号覆盖区间 `376..3698`，区间可能与其他分区交错，不以序号定义领域边界。
- 组件边界判定：细分子系统只是并行处理的最小库存边界，不等价于一个组件。必须继续按共同读者/写者、变更原因、生命周期、权威性和副作用分为 Component、System、Query、Command、Adapter 或 Projection。
- 证据状态：成员声明和统计已由来源报告校验；逐成员读者、写者、初始化/清理路径、持久化/网络语义和系统顺序仍需本分区会话补证，不得从名称或数量推断。

## 2. 分区统计

| 父级子系统 | 细分子系统数 | 字段 | 属性 | 合计 |
|---|---:|---:|---:|---:|
| `RuntimeComposition` | 1 | 14 | 0 | 14 |
| `SharedRuntimeMechanisms` | 2 | 29 | 1 | 30 |

| 原报告章节 | 父级 | 细分子系统 | 边界角色 | 字段 | 属性 | 合计 | 组件化处理 |
|---|---|---|---|---:|---:|---:|---|
| `4.1.25` | `RuntimeComposition` | `MainEntityPoolsAndWorldSlots` | runtime state | 14 | 0 | 14 | 待按成员访问模式拆分 |
| `4.9.6` | `SharedRuntimeMechanisms` | `SharedEntitySourceAndAttribution` | value object | 22 | 1 | 23 | 待按成员访问模式拆分 |
| `4.9.194` | `SharedRuntimeMechanisms` | `EntityIdentityAndMotionState` | state | 7 | 0 | 7 | 待按成员访问模式拆分 |

## 3. 并行组件拆分契约

对本文件中的每个细分子系统和每条成员记录执行以下判断，并把结果写入对应的实现阶段设计/审计记录：

- 盘点声明类型、可见性、读者、写者、创建/更新/清理/持久化生命周期、访问频率和副作用；缺证据标记 `missing`，不可用命名猜测补齐。
- 标记权威状态、派生值、缓存、快照、兼容字段和表现数据；Component 只保存一个内聚概念的权威数据。
- 需要行为转换时放入显式 System/Command；纯资格判断和派生计算放入 Query；外部协议、文件、平台和第三方类型放入 Adapter；网络/持久化/日志/UI 输出放入单向 Projection。
- 分别建模实体引用、持久化 ID、网络 ID 和外部 ID；不得把 ID 类文件或外部对象直接塞入共享组件。
- 记录 Interface、Implementation、Seam、Depth、Leverage、Locality，以及必要的系统先后约束、失败归属和重试策略。

## 4. 逐成员源码声明

以下细分子系统块保持来源报告的成员表、声明、路径、行列和来源序号；只调整 Markdown 标题层级并增加分区追踪元数据。

### 4.1 细分子系统：`MainEntityPoolsAndWorldSlots`

- 原报告章节：`4.1.25`
- 父级子系统：`RuntimeComposition`
- 分区工作包：`06` / `实体生命周期与归因`

- 上一级基线细分子系统：`MainEntityPoolsAndWorldSlots`
- 细分职责：尘埃、星体、物品、NPC、投射物、容器和动画槽。
- 边界角色：`runtime state`；最小 seam：entity-pool view；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：14；属性：0；合计：14。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（14）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 376 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 930 | 2 | dust | Terraria.Dust[] | `public static Dust[] dust = new Dust[6001];` | `public static Dust[] dust = new Dust[6001];` |
| 377 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 932 | 2 | star | Terraria.Star[] | `public static Star[] star = new Star[400];` | `public static Star[] star = new Star[400];` |
| 378 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 934 | 2 | item | Terraria.WorldItem[] | `public static WorldItem[] item = new WorldItem[401];` | `public static WorldItem[] item = new WorldItem[401];` |
| 379 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 936 | 2 | timeItemSlotCannotBeReusedFor | int[] | `public static int[] timeItemSlotCannotBeReusedFor = new int[401];` | `public static int[] timeItemSlotCannotBeReusedFor = new int[401];` |
| 380 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 938 | 2 | npc | Terraria.NPC[] | `public static NPC[] npc = new NPC[maxNPCs + 1];` | `public static NPC[] npc = new NPC[maxNPCs + 1];` |
| 381 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 940 | 2 | gore | Terraria.Gore[] | `public static Gore[] gore = new Gore[601];` | `public static Gore[] gore = new Gore[601];` |
| 382 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 942 | 2 | rain | Terraria.Rain[] | `public static Rain[] rain = new Rain[maxRain + 1];` | `public static Rain[] rain = new Rain[maxRain + 1];` |
| 383 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 944 | 2 | projectile | Terraria.Projectile[] | `public static Projectile[] projectile = new Projectile[1001];` | `public static Projectile[] projectile = new Projectile[1001];` |
| 384 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 946 | 2 | projectileIdentity | int[,] | `public static int[,] projectileIdentity = new int[256, 1001];` | `public static int[,] projectileIdentity = new int[256, 1001];` |
| 385 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 948 | 2 | combatText | Terraria.CombatText[] | `public static CombatText[] combatText = new CombatText[100];` | `public static CombatText[] combatText = new CombatText[100];` |
| 386 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 950 | 2 | chest | Terraria.Chest[] | `public static Chest[] chest = new Chest[8000];` | `public static Chest[] chest = new Chest[8000];` |
| 387 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 952 | 2 | sign | Terraria.Sign[] | `public static Sign[] sign = new Sign[32000];` | `public static Sign[] sign = new Sign[32000];` |
| 388 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 954 | 2 | itemAnimations | Terraria.DataStructures.DrawAnimation[] | `public static DrawAnimation[] itemAnimations = new DrawAnimation[ItemID.Count];` | `public static DrawAnimation[] itemAnimations = new DrawAnimation[ItemID.Count];` |
| 389 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 957 | 2 | itemAnimationsRegistered | System.Collections.Generic.List<int> | `public static List<int> itemAnimationsRegistered = new List<int>();` | `public static List<int> itemAnimationsRegistered = new List<int>();` |

#### 属性（0）

无该类型成员记录。


### 4.2 细分子系统：`SharedEntitySourceAndAttribution`

- 原报告章节：`4.9.6`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`06` / `实体生命周期与归因`

- 上一级基线细分子系统：`SharedEntitySourceAndAttribution`
- 细分职责：实体来源链、死亡原因和归因上下文。
- 边界角色：`value object`；最小 seam：source attribution query；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：11；声明类型数：11；字段：22；属性：1；合计：23。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（22）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1030 | field | Terraria.DataStructures.AEntitySource_OnHit | Terraria.DataStructures/AEntitySource_OnHit.cs | D:\TRbackup\Version4\Terraria.DataStructures\AEntitySource_OnHit.cs | 5 | 2 | EntityStriking | Terraria.IEntitySourceTarget | `public readonly IEntitySourceTarget EntityStriking;` | `public readonly IEntitySourceTarget EntityStriking;` |
| 1031 | field | Terraria.DataStructures.AEntitySource_OnHit | Terraria.DataStructures/AEntitySource_OnHit.cs | D:\TRbackup\Version4\Terraria.DataStructures\AEntitySource_OnHit.cs | 7 | 2 | EntityStruck | Terraria.IEntitySourceTarget | `public readonly IEntitySourceTarget EntityStruck;` | `public readonly IEntitySourceTarget EntityStruck;` |
| 1032 | field | Terraria.DataStructures.AEntitySource_Tile | Terraria.DataStructures/AEntitySource_Tile.cs | D:\TRbackup\Version4\Terraria.DataStructures\AEntitySource_Tile.cs | 7 | 2 | TileCoords | Point | `public readonly Point TileCoords;` | `public readonly Point TileCoords;` |
| 1126 | field | Terraria.DataStructures.EntitySource_ByItemSourceId | Terraria.DataStructures/EntitySource_ByItemSourceId.cs | D:\TRbackup\Version4\Terraria.DataStructures\EntitySource_ByItemSourceId.cs | 5 | 2 | Entity | Terraria.IEntitySourceTarget | `public readonly IEntitySourceTarget Entity;` | `public readonly IEntitySourceTarget Entity;` |
| 1127 | field | Terraria.DataStructures.EntitySource_ByItemSourceId | Terraria.DataStructures/EntitySource_ByItemSourceId.cs | D:\TRbackup\Version4\Terraria.DataStructures\EntitySource_ByItemSourceId.cs | 7 | 2 | SourceId | int | `public readonly int SourceId;` | `public readonly int SourceId;` |
| 1128 | field | Terraria.DataStructures.EntitySource_ByProjectileSourceId | Terraria.DataStructures/EntitySource_ByProjectileSourceId.cs | D:\TRbackup\Version4\Terraria.DataStructures\EntitySource_ByProjectileSourceId.cs | 5 | 2 | SourceId | int | `public readonly int SourceId;` | `public readonly int SourceId;` |
| 1131 | field | Terraria.DataStructures.EntitySource_ItemUse_WithAmmo | Terraria.DataStructures/EntitySource_ItemUse_WithAmmo.cs | D:\TRbackup\Version4\Terraria.DataStructures\EntitySource_ItemUse_WithAmmo.cs | 5 | 2 | AmmoItemIdUsed | int | `public readonly int AmmoItemIdUsed;` | `public readonly int AmmoItemIdUsed;` |
| 1132 | field | Terraria.DataStructures.EntitySource_ItemUse | Terraria.DataStructures/EntitySource_ItemUse.cs | D:\TRbackup\Version4\Terraria.DataStructures\EntitySource_ItemUse.cs | 5 | 2 | Entity | Terraria.IEntitySourceTarget | `public readonly IEntitySourceTarget Entity;` | `public readonly IEntitySourceTarget Entity;` |
| 1133 | field | Terraria.DataStructures.EntitySource_ItemUse | Terraria.DataStructures/EntitySource_ItemUse.cs | D:\TRbackup\Version4\Terraria.DataStructures\EntitySource_ItemUse.cs | 7 | 2 | Item | Terraria.Item | `public readonly Item Item;` | `public readonly Item Item;` |
| 1135 | field | Terraria.DataStructures.EntitySource_Mount | Terraria.DataStructures/EntitySource_Mount.cs | D:\TRbackup\Version4\Terraria.DataStructures\EntitySource_Mount.cs | 5 | 2 | Entity | Terraria.IEntitySourceTarget | `public readonly IEntitySourceTarget Entity;` | `public readonly IEntitySourceTarget Entity;` |
| 1136 | field | Terraria.DataStructures.EntitySource_Mount | Terraria.DataStructures/EntitySource_Mount.cs | D:\TRbackup\Version4\Terraria.DataStructures\EntitySource_Mount.cs | 7 | 2 | MountId | int | `public readonly int MountId;` | `public readonly int MountId;` |
| 1137 | field | Terraria.DataStructures.EntitySource_OverfullChest | Terraria.DataStructures/EntitySource_OverfullChest.cs | D:\TRbackup\Version4\Terraria.DataStructures\EntitySource_OverfullChest.cs | 5 | 2 | Chest | Terraria.Chest | `public readonly Chest Chest;` | `public readonly Chest Chest;` |
| 1138 | field | Terraria.DataStructures.EntitySource_Parent | Terraria.DataStructures/EntitySource_Parent.cs | D:\TRbackup\Version4\Terraria.DataStructures\EntitySource_Parent.cs | 5 | 2 | Entity | Terraria.IEntitySourceTarget | `public readonly IEntitySourceTarget Entity;` | `public readonly IEntitySourceTarget Entity;` |
| 1139 | field | Terraria.DataStructures.EntitySource_TileInteraction | Terraria.DataStructures/EntitySource_TileInteraction.cs | D:\TRbackup\Version4\Terraria.DataStructures\EntitySource_TileInteraction.cs | 5 | 2 | Entity | Terraria.IEntitySourceTarget | `public readonly IEntitySourceTarget Entity;` | `public readonly IEntitySourceTarget Entity;` |
| 1216 | field | Terraria.DataStructures.PlayerDeathReason | Terraria.DataStructures/PlayerDeathReason.cs | D:\TRbackup\Version4\Terraria.DataStructures\PlayerDeathReason.cs | 8 | 2 | _sourcePlayerIndex | int | `private int _sourcePlayerIndex = -1;` | `private int _sourcePlayerIndex = -1;` |
| 1217 | field | Terraria.DataStructures.PlayerDeathReason | Terraria.DataStructures/PlayerDeathReason.cs | D:\TRbackup\Version4\Terraria.DataStructures\PlayerDeathReason.cs | 10 | 2 | _sourceNPCIndex | int | `private int _sourceNPCIndex = -1;` | `private int _sourceNPCIndex = -1;` |
| 1218 | field | Terraria.DataStructures.PlayerDeathReason | Terraria.DataStructures/PlayerDeathReason.cs | D:\TRbackup\Version4\Terraria.DataStructures\PlayerDeathReason.cs | 12 | 2 | _sourceProjectileLocalIndex | int | `private int _sourceProjectileLocalIndex = -1;` | `private int _sourceProjectileLocalIndex = -1;` |
| 1219 | field | Terraria.DataStructures.PlayerDeathReason | Terraria.DataStructures/PlayerDeathReason.cs | D:\TRbackup\Version4\Terraria.DataStructures\PlayerDeathReason.cs | 14 | 2 | _sourceOtherIndex | int | `private int _sourceOtherIndex = -1;` | `private int _sourceOtherIndex = -1;` |
| 1220 | field | Terraria.DataStructures.PlayerDeathReason | Terraria.DataStructures/PlayerDeathReason.cs | D:\TRbackup\Version4\Terraria.DataStructures\PlayerDeathReason.cs | 16 | 2 | _sourceProjectileType | int | `private int _sourceProjectileType;` | `private int _sourceProjectileType;` |
| 1221 | field | Terraria.DataStructures.PlayerDeathReason | Terraria.DataStructures/PlayerDeathReason.cs | D:\TRbackup\Version4\Terraria.DataStructures\PlayerDeathReason.cs | 18 | 2 | _sourceItemType | int | `private int _sourceItemType;` | `private int _sourceItemType;` |
| 1222 | field | Terraria.DataStructures.PlayerDeathReason | Terraria.DataStructures/PlayerDeathReason.cs | D:\TRbackup\Version4\Terraria.DataStructures\PlayerDeathReason.cs | 20 | 2 | _sourceItemPrefix | int | `private int _sourceItemPrefix;` | `private int _sourceItemPrefix;` |
| 1223 | field | Terraria.DataStructures.PlayerDeathReason | Terraria.DataStructures/PlayerDeathReason.cs | D:\TRbackup\Version4\Terraria.DataStructures\PlayerDeathReason.cs | 22 | 2 | _sourceCustomReason | string | `private string _sourceCustomReason;` | `private string _sourceCustomReason;` |

#### 属性（1）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3698 | property | Terraria.DataStructures.PlayerDeathReason | Terraria.DataStructures/PlayerDeathReason.cs | D:\TRbackup\Version4\Terraria.DataStructures\PlayerDeathReason.cs | 24 | 2 | SourceProjectileType | int? | `public int? SourceProjectileType { get { if (_sourceProjectileLocalIndex == -1) { return null; } return _sourceProjectileType; } }` | `public int? SourceProjectileType { get { if (_sourceProjectileLocalIndex == -1) { return null; } return _sourceProjectileType; } }` |


### 4.3 细分子系统：`EntityIdentityAndMotionState`

- 原报告章节：`4.9.194`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`06` / `实体生命周期与归因`

- 上一级基线细分子系统：`EntityAuthoritativeState`
- 上一级 peer 细分子系统：`EntityAuthoritativeState`
- 细分职责：实体身份、位置、速度、方向和运动历史。
- 边界角色：`state`；最小 seam：entity identity motion port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：7；属性：0；合计：7。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（7）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3060 | field | Terraria.Entity | Terraria/Entity.cs | D:\TRbackup\Version4\Terraria\Entity.cs | 8 | 2 | whoAmI | int | `public int whoAmI;` | `public int whoAmI;` |
| 3061 | field | Terraria.Entity | Terraria/Entity.cs | D:\TRbackup\Version4\Terraria\Entity.cs | 10 | 2 | position | Vector2 | `public Vector2 position;` | `public Vector2 position;` |
| 3062 | field | Terraria.Entity | Terraria/Entity.cs | D:\TRbackup\Version4\Terraria\Entity.cs | 12 | 2 | velocity | Vector2 | `public Vector2 velocity;` | `public Vector2 velocity;` |
| 3063 | field | Terraria.Entity | Terraria/Entity.cs | D:\TRbackup\Version4\Terraria\Entity.cs | 14 | 2 | oldPosition | Vector2 | `public Vector2 oldPosition;` | `public Vector2 oldPosition;` |
| 3064 | field | Terraria.Entity | Terraria/Entity.cs | D:\TRbackup\Version4\Terraria\Entity.cs | 16 | 2 | oldVelocity | Vector2 | `public Vector2 oldVelocity;` | `public Vector2 oldVelocity;` |
| 3065 | field | Terraria.Entity | Terraria/Entity.cs | D:\TRbackup\Version4\Terraria\Entity.cs | 18 | 2 | oldDirection | int | `public int oldDirection;` | `public int oldDirection;` |
| 3066 | field | Terraria.Entity | Terraria/Entity.cs | D:\TRbackup\Version4\Terraria\Entity.cs | 20 | 2 | direction | int | `public int direction = 1;` | `public int direction = 1;` |

#### 属性（0）

无该类型成员记录。


## 5. 追溯与验收

> 来源报告 SHA-256：`b944441d92d5103f218b766a3e354200794528a19e65a862e44c2b40a36f1196`
- 本分区细分组数：3；成员数：44；字段：43；属性：1。
- 预期不变量：本文件只出现完整细分子系统；不跨分区复制成员；所有成员的来源序号、原始声明、路径、行列和 C# 类型必须与来源报告一致。
- 验证方式：重新读取 20 个文件，检查文件数、细分组唯一性、来源序号恰好覆盖 `1..4542`、字段/属性合计和逐成员行文本一致；本次分区生成不运行 `dotnet`，因为没有修改 C# 或项目文件。

生成器：`Build/Tools/Generate-Version4NonAuthoritativeComponentPartitionReports.ps1`。
