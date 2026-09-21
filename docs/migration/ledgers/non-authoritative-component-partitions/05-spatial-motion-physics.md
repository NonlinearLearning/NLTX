# Version4 非权威组件拆分分区 05/20：空间移动与物理

> 来源报告：`docs/migration/ledgers/Version4非权威模拟系统字段属性逐成员源码声明-更细子系统拆分-去除ID类文件.md`
> 来源报告 SHA-256：`b944441d92d5103f218b766a3e354200794528a19e65a862e44c2b40a36f1196`
> 本文件是并行组件拆分工作包，不是新的权威成员清单；成员声明事实直接保留来源报告原文。

## 1. 分区定位与评估

- 分区范围：空间查询、碰撞、传送、实体几何、载具、座位、投射物引用和移动轨道。
- 本分区组件化重点：确认移动状态、关系引用、碰撞查询和结构变更的访问方向。
- 本分区包含 10 个完整细分子系统、119 条成员记录（字段 99、属性 20）。来源序号覆盖区间 `229..3975`，区间可能与其他分区交错，不以序号定义领域边界。
- 组件边界判定：细分子系统只是并行处理的最小库存边界，不等价于一个组件。必须继续按共同读者/写者、变更原因、生命周期、权威性和副作用分为 Component、System、Query、Command、Adapter 或 Projection。
- 证据状态：成员声明和统计已由来源报告校验；逐成员读者、写者、初始化/清理路径、持久化/网络语义和系统顺序仍需本分区会话补证，不得从名称或数量推断。

## 2. 分区统计

| 父级子系统 | 细分子系统数 | 字段 | 属性 | 合计 |
|---|---:|---:|---:|---:|
| `RuntimeComposition` | 1 | 7 | 0 | 7 |
| `SharedRuntimeMechanisms` | 9 | 92 | 20 | 112 |

| 原报告章节 | 父级 | 细分子系统 | 边界角色 | 字段 | 属性 | 合计 | 组件化处理 |
|---|---|---|---|---:|---:|---:|---|
| `4.1.19` | `RuntimeComposition` | `MainSpawnAndProjectileCaches` | catalog reference | 7 | 0 | 7 | 待按成员访问模式拆分 |
| `4.9.5` | `SharedRuntimeMechanisms` | `SharedTeleportAndPortalSupport` | query/adapter | 12 | 1 | 13 | 待按成员访问模式拆分 |
| `4.9.35` | `SharedRuntimeMechanisms` | `SharedPhysicsCollisionQueries` | query | 11 | 0 | 11 | 待按成员访问模式拆分 |
| `4.9.93` | `SharedRuntimeMechanisms` | `MinecartCustomizationState` | definition/presentation | 3 | 1 | 4 | 待按成员访问模式拆分 |
| `4.9.94` | `SharedRuntimeMechanisms` | `TrackedProjectileReferenceState` | relation/state | 0 | 5 | 5 | 待按成员访问模式拆分 |
| `4.9.102` | `SharedRuntimeMechanisms` | `EnvironmentDamageAndSeatState` | state | 6 | 0 | 6 | 待按成员访问模式拆分 |
| `4.9.140` | `SharedRuntimeMechanisms` | `MinecartMotionAndTrackState` | state/query | 17 | 0 | 17 | 待按成员访问模式拆分 |
| `4.9.141` | `SharedRuntimeMechanisms` | `MinecartDecorationAndSwitchState` | definition/presentation | 16 | 0 | 16 | 待按成员访问模式拆分 |
| `4.9.157` | `SharedRuntimeMechanisms` | `SharedGeneralTeleportAndInterceptionUtilities` | query/value object | 20 | 0 | 20 | 待按成员访问模式拆分 |
| `4.9.195` | `SharedRuntimeMechanisms` | `EntityBoundsAndFluidState` | state | 7 | 13 | 20 | 待按成员访问模式拆分 |

## 3. 并行组件拆分契约

对本文件中的每个细分子系统和每条成员记录执行以下判断，并把结果写入对应的实现阶段设计/审计记录：

- 盘点声明类型、可见性、读者、写者、创建/更新/清理/持久化生命周期、访问频率和副作用；缺证据标记 `missing`，不可用命名猜测补齐。
- 标记权威状态、派生值、缓存、快照、兼容字段和表现数据；Component 只保存一个内聚概念的权威数据。
- 需要行为转换时放入显式 System/Command；纯资格判断和派生计算放入 Query；外部协议、文件、平台和第三方类型放入 Adapter；网络/持久化/日志/UI 输出放入单向 Projection。
- 分别建模实体引用、持久化 ID、网络 ID 和外部 ID；不得把 ID 类文件或外部对象直接塞入共享组件。
- 记录 Interface、Implementation、Seam、Depth、Leverage、Locality，以及必要的系统先后约束、失败归属和重试策略。

## 4. 逐成员源码声明

以下细分子系统块保持来源报告的成员表、声明、路径、行列和来源序号；只调整 Markdown 标题层级并增加分区追踪元数据。

### 4.1 细分子系统：`MainSpawnAndProjectileCaches`

- 原报告章节：`4.1.19`
- 父级子系统：`RuntimeComposition`
- 分区工作包：`05` / `空间移动与物理`

- 上一级基线细分子系统：`MainSpawnAndProjectileCaches`
- 细分职责：刷怪检查和投射物帧/宠物缓存。
- 边界角色：`catalog reference`；最小 seam：spawn/projectile cache view；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：7；属性：0；合计：7。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（7）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 229 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 626 | 2 | checkForSpawns | int | `public static int checkForSpawns;` | `public static int checkForSpawns;` |
| 230 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 628 | 2 | helpText | int | `public static int helpText;` | `public static int helpText;` |
| 231 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 630 | 2 | BartenderHelpTextIndex | int | `public static int BartenderHelpTextIndex = 0;` | `public static int BartenderHelpTextIndex = 0;` |
| 232 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 632 | 2 | autoGen | bool | `public static bool autoGen;` | `public static bool autoGen;` |
| 233 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 634 | 2 | projFrames | int[] | `public static int[] projFrames = new int[ProjectileID.Count];` | `public static int[] projFrames = new int[ProjectileID.Count];` |
| 234 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 636 | 2 | projPet | bool[] | `public static bool[] projPet = new bool[ProjectileID.Count];` | `public static bool[] projPet = new bool[ProjectileID.Count];` |
| 235 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 638 | 2 | demonTorch | float | `public static float demonTorch = 1f;` | `public static float demonTorch = 1f;` |

#### 属性（0）

无该类型成员记录。


### 4.2 细分子系统：`SharedTeleportAndPortalSupport`

- 原报告章节：`4.9.5`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`05` / `空间移动与物理`

- 上一级基线细分子系统：`SharedTeleportAndPortalSupport`
- 细分职责：传送门、水晶塔和脱困支持。
- 边界角色：`query/adapter`；最小 seam：travel eligibility view；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：3；声明类型数：3；字段：12；属性：1；合计：13。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（12）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2457 | field | Terraria.GameContent.PortalHelper | Terraria.GameContent/PortalHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\PortalHelper.cs | 10 | 2 | PORTALS_PER_PERSON | int | `public const int PORTALS_PER_PERSON = 2;` | `public const int PORTALS_PER_PERSON = 2;` |
| 2458 | field | Terraria.GameContent.PortalHelper | Terraria.GameContent/PortalHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\PortalHelper.cs | 12 | 2 | FoundPortals | int[,] | `private static int[,] FoundPortals;` | `private static int[,] FoundPortals;` |
| 2459 | field | Terraria.GameContent.PortalHelper | Terraria.GameContent/PortalHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\PortalHelper.cs | 14 | 2 | PortalCooldownForPlayers | int[] | `private static int[] PortalCooldownForPlayers;` | `private static int[] PortalCooldownForPlayers;` |
| 2460 | field | Terraria.GameContent.PortalHelper | Terraria.GameContent/PortalHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\PortalHelper.cs | 16 | 2 | PortalCooldownForNPCs | int[] | `private static int[] PortalCooldownForNPCs;` | `private static int[] PortalCooldownForNPCs;` |
| 2461 | field | Terraria.GameContent.PortalHelper | Terraria.GameContent/PortalHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\PortalHelper.cs | 18 | 2 | EDGES | Vector2[] | `private static readonly Vector2[] EDGES;` | `private static readonly Vector2[] EDGES;` |
| 2462 | field | Terraria.GameContent.PortalHelper | Terraria.GameContent/PortalHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\PortalHelper.cs | 20 | 2 | SLOPE_EDGES | Vector2[] | `private static readonly Vector2[] SLOPE_EDGES;` | `private static readonly Vector2[] SLOPE_EDGES;` |
| 2463 | field | Terraria.GameContent.PortalHelper | Terraria.GameContent/PortalHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\PortalHelper.cs | 22 | 2 | SLOPE_OFFSETS | Point[] | `private static readonly Point[] SLOPE_OFFSETS;` | `private static readonly Point[] SLOPE_OFFSETS;` |
| 2464 | field | Terraria.GameContent.PortalHelper | Terraria.GameContent/PortalHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\PortalHelper.cs | 24 | 2 | anyPortalAtAll | bool | `private static bool anyPortalAtAll;` | `private static bool anyPortalAtAll;` |
| 2504 | field | Terraria.GameContent.ShimmerUnstuckHelper | Terraria.GameContent/ShimmerUnstuckHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\ShimmerUnstuckHelper.cs | 7 | 2 | TimeLeftUnstuck | int | `public int TimeLeftUnstuck;` | `public int TimeLeftUnstuck;` |
| 2505 | field | Terraria.GameContent.ShimmerUnstuckHelper | Terraria.GameContent/ShimmerUnstuckHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\ShimmerUnstuckHelper.cs | 9 | 2 | IndefiniteProtectionActive | bool | `public bool IndefiniteProtectionActive;` | `public bool IndefiniteProtectionActive;` |
| 2542 | field | Terraria.GameContent.TeleportPylonInfo | Terraria.GameContent/TeleportPylonInfo.cs | D:\TRbackup\Version4\Terraria.GameContent\TeleportPylonInfo.cs | 8 | 2 | PositionInTiles | Terraria.DataStructures.Point16 | `public Point16 PositionInTiles;` | `public Point16 PositionInTiles;` |
| 2543 | field | Terraria.GameContent.TeleportPylonInfo | Terraria.GameContent/TeleportPylonInfo.cs | D:\TRbackup\Version4\Terraria.GameContent\TeleportPylonInfo.cs | 10 | 2 | TypeOfPylon | Terraria.GameContent.TeleportPylonType | `public TeleportPylonType TypeOfPylon;` | `public TeleportPylonType TypeOfPylon;` |

#### 属性（1）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3856 | property | Terraria.GameContent.ShimmerUnstuckHelper | Terraria.GameContent/ShimmerUnstuckHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\ShimmerUnstuckHelper.cs | 11 | 2 | ShouldUnstuck | bool | `public bool ShouldUnstuck { get { if (!IndefiniteProtectionActive) { return TimeLeftUnstuck > 0; } return true; } }` | `public bool ShouldUnstuck { get { if (!IndefiniteProtectionActive) { return TimeLeftUnstuck > 0; } return true; } }` |


### 4.3 细分子系统：`SharedPhysicsCollisionQueries`

- 原报告章节：`4.9.35`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`05` / `空间移动与物理`

- 上一级基线细分子系统：`SharedPhysicsCollisionQueries`
- 细分职责：球体碰撞和穿透查询事件。
- 边界角色：`query`；最小 seam：collision query；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：3；声明类型数：3；字段：11；属性：0；合计：11。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（11）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2880 | field | Terraria.Physics.BallCollisionEvent | Terraria.Physics/BallCollisionEvent.cs | D:\TRbackup\Version4\Terraria.Physics\BallCollisionEvent.cs | 7 | 2 | Normal | Vector2 | `public readonly Vector2 Normal;` | `public readonly Vector2 Normal;` |
| 2881 | field | Terraria.Physics.BallCollisionEvent | Terraria.Physics/BallCollisionEvent.cs | D:\TRbackup\Version4\Terraria.Physics\BallCollisionEvent.cs | 9 | 2 | ImpactPoint | Vector2 | `public readonly Vector2 ImpactPoint;` | `public readonly Vector2 ImpactPoint;` |
| 2882 | field | Terraria.Physics.BallCollisionEvent | Terraria.Physics/BallCollisionEvent.cs | D:\TRbackup\Version4\Terraria.Physics\BallCollisionEvent.cs | 11 | 2 | Tile | Terraria.Tile | `public readonly Tile Tile;` | `public readonly Tile Tile;` |
| 2883 | field | Terraria.Physics.BallCollisionEvent | Terraria.Physics/BallCollisionEvent.cs | D:\TRbackup\Version4\Terraria.Physics\BallCollisionEvent.cs | 13 | 2 | Entity | Terraria.Entity | `public readonly Entity Entity;` | `public readonly Entity Entity;` |
| 2884 | field | Terraria.Physics.BallCollisionEvent | Terraria.Physics/BallCollisionEvent.cs | D:\TRbackup\Version4\Terraria.Physics\BallCollisionEvent.cs | 15 | 2 | TimeScale | float | `public readonly float TimeScale;` | `public readonly float TimeScale;` |
| 2885 | field | Terraria.Physics.BallPassThroughEvent | Terraria.Physics/BallPassThroughEvent.cs | D:\TRbackup\Version4\Terraria.Physics\BallPassThroughEvent.cs | 5 | 2 | Tile | Terraria.Tile | `public readonly Tile Tile;` | `public readonly Tile Tile;` |
| 2886 | field | Terraria.Physics.BallPassThroughEvent | Terraria.Physics/BallPassThroughEvent.cs | D:\TRbackup\Version4\Terraria.Physics\BallPassThroughEvent.cs | 7 | 2 | Entity | Terraria.Entity | `public readonly Entity Entity;` | `public readonly Entity Entity;` |
| 2887 | field | Terraria.Physics.BallPassThroughEvent | Terraria.Physics/BallPassThroughEvent.cs | D:\TRbackup\Version4\Terraria.Physics\BallPassThroughEvent.cs | 9 | 2 | Type | Terraria.Physics.BallPassThroughType | `public readonly BallPassThroughType Type;` | `public readonly BallPassThroughType Type;` |
| 2888 | field | Terraria.Physics.BallPassThroughEvent | Terraria.Physics/BallPassThroughEvent.cs | D:\TRbackup\Version4\Terraria.Physics\BallPassThroughEvent.cs | 11 | 2 | TimeScale | float | `public readonly float TimeScale;` | `public readonly float TimeScale;` |
| 2889 | field | Terraria.Physics.PhysicsProperties | Terraria.Physics/PhysicsProperties.cs | D:\TRbackup\Version4\Terraria.Physics\PhysicsProperties.cs | 5 | 2 | Gravity | float | `public readonly float Gravity;` | `public readonly float Gravity;` |
| 2890 | field | Terraria.Physics.PhysicsProperties | Terraria.Physics/PhysicsProperties.cs | D:\TRbackup\Version4\Terraria.Physics\PhysicsProperties.cs | 7 | 2 | Drag | float | `public readonly float Drag;` | `public readonly float Drag;` |

#### 属性（0）

无该类型成员记录。


### 4.4 细分子系统：`MinecartCustomizationState`

- 原报告章节：`4.9.93`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`05` / `空间移动与物理`

- 上一级基线细分子系统：`MinecartCustomizationState`
- 细分职责：矿车纹理、轮距和定制表现状态。
- 边界角色：`definition/presentation`；最小 seam：minecart customization port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：3；属性：1；合计：4。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（3）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3313 | field | Terraria.Minecart.Customization | Terraria/Minecart.cs | D:\TRbackup\Version4\Terraria\Minecart.cs | 23 | 3 | MinecartTextureWidth | float | `public float MinecartTextureWidth;` | `public float MinecartTextureWidth;` |
| 3314 | field | Terraria.Minecart.Customization | Terraria/Minecart.cs | D:\TRbackup\Version4\Terraria\Minecart.cs | 25 | 3 | WheelOffset | Vector2 | `public Vector2 WheelOffset;` | `public Vector2 WheelOffset;` |
| 3315 | field | Terraria.Minecart.Customization | Terraria/Minecart.cs | D:\TRbackup\Version4\Terraria\Minecart.cs | 27 | 3 | MagnetOffset | Vector2 | `public Vector2 MagnetOffset;` | `public Vector2 MagnetOffset;` |

#### 属性（1）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3975 | property | Terraria.Minecart.Customization | Terraria/Minecart.cs | D:\TRbackup\Version4\Terraria\Minecart.cs | 29 | 3 | Default | Terraria.Minecart.Customization | `public static Customization Default => new Customization { MinecartTextureWidth = 50f, MagnetOffset = new Vector2(25f, 26f), WheelOffset = new Vector2(12f, 0f) };` | `public static Customization Default => new Customization { MinecartTextureWidth = 50f, MagnetOffset = new Vector2(25f, 26f), WheelOffset = new Vector2(12f, 0f) };` |


### 4.5 细分子系统：`TrackedProjectileReferenceState`

- 原报告章节：`4.9.94`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`05` / `空间移动与物理`

- 上一级基线细分子系统：`TrackedProjectileReferenceState`
- 细分职责：投射物本地索引和拥有者引用跟踪。
- 边界角色：`relation/state`；最小 seam：tracked projectile reference port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：0；属性：5；合计：5。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（0）

无该类型成员记录。

#### 属性（5）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3711 | property | Terraria.DataStructures.TrackedProjectileReference | Terraria.DataStructures/TrackedProjectileReference.cs | D:\TRbackup\Version4\Terraria.DataStructures\TrackedProjectileReference.cs | 7 | 2 | ProjectileLocalIndex | int | `public int ProjectileLocalIndex { get; private set; }` | `public int ProjectileLocalIndex { get; private set; }` |
| 3712 | property | Terraria.DataStructures.TrackedProjectileReference | Terraria.DataStructures/TrackedProjectileReference.cs | D:\TRbackup\Version4\Terraria.DataStructures\TrackedProjectileReference.cs | 9 | 2 | ProjectileOwnerIndex | int | `public int ProjectileOwnerIndex { get; private set; }` | `public int ProjectileOwnerIndex { get; private set; }` |
| 3713 | property | Terraria.DataStructures.TrackedProjectileReference | Terraria.DataStructures/TrackedProjectileReference.cs | D:\TRbackup\Version4\Terraria.DataStructures\TrackedProjectileReference.cs | 11 | 2 | ProjectileIdentity | int | `public int ProjectileIdentity { get; private set; }` | `public int ProjectileIdentity { get; private set; }` |
| 3714 | property | Terraria.DataStructures.TrackedProjectileReference | Terraria.DataStructures/TrackedProjectileReference.cs | D:\TRbackup\Version4\Terraria.DataStructures\TrackedProjectileReference.cs | 13 | 2 | ProjectileType | int | `public int ProjectileType { get; private set; }` | `public int ProjectileType { get; private set; }` |
| 3715 | property | Terraria.DataStructures.TrackedProjectileReference | Terraria.DataStructures/TrackedProjectileReference.cs | D:\TRbackup\Version4\Terraria.DataStructures\TrackedProjectileReference.cs | 15 | 2 | IsTrackingSomething | bool | `public bool IsTrackingSomething { get; private set; }` | `public bool IsTrackingSomething { get; private set; }` |


### 4.6 细分子系统：`EnvironmentDamageAndSeatState`

- 原报告章节：`4.9.102`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`05` / `空间移动与物理`

- 上一级基线细分子系统：`EnvironmentDamageAndSeatState`
- 细分职责：环境黑暗伤害和额外座位信息状态。
- 边界角色：`state`；最小 seam：environment damage seat port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：2；声明类型数：2；字段：6；属性：0；合计：6。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（6）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2369 | field | Terraria.GameContent.DontStarveDarknessDamageDealer | Terraria.GameContent/DontStarveDarknessDamageDealer.cs | D:\TRbackup\Version4\Terraria.GameContent\DontStarveDarknessDamageDealer.cs | 10 | 2 | DARKNESS_HIT_TIMER_MAX_BEFORE_HIT | int | `public const int DARKNESS_HIT_TIMER_MAX_BEFORE_HIT = 60;` | `public const int DARKNESS_HIT_TIMER_MAX_BEFORE_HIT = 60;` |
| 2370 | field | Terraria.GameContent.DontStarveDarknessDamageDealer | Terraria.GameContent/DontStarveDarknessDamageDealer.cs | D:\TRbackup\Version4\Terraria.GameContent\DontStarveDarknessDamageDealer.cs | 12 | 2 | darknessTimer | int | `public static int darknessTimer = -1;` | `public static int darknessTimer = -1;` |
| 2371 | field | Terraria.GameContent.DontStarveDarknessDamageDealer | Terraria.GameContent/DontStarveDarknessDamageDealer.cs | D:\TRbackup\Version4\Terraria.GameContent\DontStarveDarknessDamageDealer.cs | 14 | 2 | darknessHitTimer | int | `public static int darknessHitTimer = 0;` | `public static int darknessHitTimer = 0;` |
| 2372 | field | Terraria.GameContent.DontStarveDarknessDamageDealer | Terraria.GameContent/DontStarveDarknessDamageDealer.cs | D:\TRbackup\Version4\Terraria.GameContent\DontStarveDarknessDamageDealer.cs | 16 | 2 | saidMessage | bool | `public static bool saidMessage = false;` | `public static bool saidMessage = false;` |
| 2373 | field | Terraria.GameContent.DontStarveDarknessDamageDealer | Terraria.GameContent/DontStarveDarknessDamageDealer.cs | D:\TRbackup\Version4\Terraria.GameContent\DontStarveDarknessDamageDealer.cs | 18 | 2 | lastFrameWasTooBright | bool | `public static bool lastFrameWasTooBright = true;` | `public static bool lastFrameWasTooBright = true;` |
| 2409 | field | Terraria.GameContent.ExtraSeatInfo | Terraria.GameContent/ExtraSeatInfo.cs | D:\TRbackup\Version4\Terraria.GameContent\ExtraSeatInfo.cs | 5 | 2 | IsAToilet | bool | `public bool IsAToilet;` | `public bool IsAToilet;` |

#### 属性（0）

无该类型成员记录。


### 4.7 细分子系统：`MinecartMotionAndTrackState`

- 原报告章节：`4.9.140`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`05` / `空间移动与物理`

- 上一级基线细分子系统：`MinecartMotionState`
- 细分职责：矿车速度、轨道连接、加速和轨道类型状态。
- 边界角色：`state/query`；最小 seam：minecart motion track port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：17；属性：0；合计：17。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（17）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3321 | field | Terraria.Minecart | Terraria/Minecart.cs | D:\TRbackup\Version4\Terraria\Minecart.cs | 47 | 2 | Flag_OnTrack | int | `public const int Flag_OnTrack = 0;` | `public const int Flag_OnTrack = 0;` |
| 3322 | field | Terraria.Minecart | Terraria/Minecart.cs | D:\TRbackup\Version4\Terraria\Minecart.cs | 49 | 2 | Flag_BouncyBumper | int | `public const int Flag_BouncyBumper = 1;` | `public const int Flag_BouncyBumper = 1;` |
| 3323 | field | Terraria.Minecart | Terraria/Minecart.cs | D:\TRbackup\Version4\Terraria\Minecart.cs | 51 | 2 | Flag_UsedRamp | int | `public const int Flag_UsedRamp = 2;` | `public const int Flag_UsedRamp = 2;` |
| 3324 | field | Terraria.Minecart | Terraria/Minecart.cs | D:\TRbackup\Version4\Terraria\Minecart.cs | 53 | 2 | Flag_HitSwitch | int | `public const int Flag_HitSwitch = 3;` | `public const int Flag_HitSwitch = 3;` |
| 3325 | field | Terraria.Minecart | Terraria/Minecart.cs | D:\TRbackup\Version4\Terraria\Minecart.cs | 55 | 2 | Flag_BoostLeft | int | `public const int Flag_BoostLeft = 4;` | `public const int Flag_BoostLeft = 4;` |
| 3326 | field | Terraria.Minecart | Terraria/Minecart.cs | D:\TRbackup\Version4\Terraria\Minecart.cs | 57 | 2 | Flag_BoostRight | int | `public const int Flag_BoostRight = 5;` | `public const int Flag_BoostRight = 5;` |
| 3335 | field | Terraria.Minecart | Terraria/Minecart.cs | D:\TRbackup\Version4\Terraria\Minecart.cs | 75 | 2 | BoosterSpeed | float | `public const float BoosterSpeed = 4f;` | `public const float BoosterSpeed = 4f;` |
| 3336 | field | Terraria.Minecart | Terraria/Minecart.cs | D:\TRbackup\Version4\Terraria\Minecart.cs | 77 | 2 | Type_Normal | int | `private const int Type_Normal = 0;` | `private const int Type_Normal = 0;` |
| 3337 | field | Terraria.Minecart | Terraria/Minecart.cs | D:\TRbackup\Version4\Terraria\Minecart.cs | 79 | 2 | Type_Pressure | int | `private const int Type_Pressure = 1;` | `private const int Type_Pressure = 1;` |
| 3338 | field | Terraria.Minecart | Terraria/Minecart.cs | D:\TRbackup\Version4\Terraria\Minecart.cs | 81 | 2 | Type_Booster | int | `private const int Type_Booster = 2;` | `private const int Type_Booster = 2;` |
| 3339 | field | Terraria.Minecart | Terraria/Minecart.cs | D:\TRbackup\Version4\Terraria\Minecart.cs | 83 | 2 | _leftSideConnection | int[] | `private static int[] _leftSideConnection;` | `private static int[] _leftSideConnection;` |
| 3340 | field | Terraria.Minecart | Terraria/Minecart.cs | D:\TRbackup\Version4\Terraria\Minecart.cs | 85 | 2 | _rightSideConnection | int[] | `private static int[] _rightSideConnection;` | `private static int[] _rightSideConnection;` |
| 3341 | field | Terraria.Minecart | Terraria/Minecart.cs | D:\TRbackup\Version4\Terraria\Minecart.cs | 87 | 2 | _trackType | int[] | `private static int[] _trackType;` | `private static int[] _trackType;` |
| 3342 | field | Terraria.Minecart | Terraria/Minecart.cs | D:\TRbackup\Version4\Terraria\Minecart.cs | 89 | 2 | _boostLeft | bool[] | `private static bool[] _boostLeft;` | `private static bool[] _boostLeft;` |
| 3344 | field | Terraria.Minecart | Terraria/Minecart.cs | D:\TRbackup\Version4\Terraria\Minecart.cs | 93 | 2 | _firstPressureFrame | short | `private static short _firstPressureFrame;` | `private static short _firstPressureFrame;` |
| 3345 | field | Terraria.Minecart | Terraria/Minecart.cs | D:\TRbackup\Version4\Terraria\Minecart.cs | 95 | 2 | _firstLeftBoostFrame | short | `private static short _firstLeftBoostFrame;` | `private static short _firstLeftBoostFrame;` |
| 3346 | field | Terraria.Minecart | Terraria/Minecart.cs | D:\TRbackup\Version4\Terraria\Minecart.cs | 97 | 2 | _firstRightBoostFrame | short | `private static short _firstRightBoostFrame;` | `private static short _firstRightBoostFrame;` |

#### 属性（0）

无该类型成员记录。


### 4.8 细分子系统：`MinecartDecorationAndSwitchState`

- 原报告章节：`4.9.141`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`05` / `空间移动与物理`

- 上一级基线细分子系统：`MinecartMotionState`
- 细分职责：矿车装饰帧、端点、纹理和轨道切换状态。
- 边界角色：`definition/presentation`；最小 seam：minecart decoration switch port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：16；属性：0；合计：16。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（16）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3316 | field | Terraria.Minecart | Terraria/Minecart.cs | D:\TRbackup\Version4\Terraria\Minecart.cs | 37 | 2 | TotalFrames | int | `private const int TotalFrames = 36;` | `private const int TotalFrames = 36;` |
| 3317 | field | Terraria.Minecart | Terraria/Minecart.cs | D:\TRbackup\Version4\Terraria\Minecart.cs | 39 | 2 | LeftDownDecoration | int | `public const int LeftDownDecoration = 36;` | `public const int LeftDownDecoration = 36;` |
| 3318 | field | Terraria.Minecart | Terraria/Minecart.cs | D:\TRbackup\Version4\Terraria\Minecart.cs | 41 | 2 | RightDownDecoration | int | `public const int RightDownDecoration = 37;` | `public const int RightDownDecoration = 37;` |
| 3319 | field | Terraria.Minecart | Terraria/Minecart.cs | D:\TRbackup\Version4\Terraria\Minecart.cs | 43 | 2 | BouncyBumperDecoration | int | `public const int BouncyBumperDecoration = 38;` | `public const int BouncyBumperDecoration = 38;` |
| 3320 | field | Terraria.Minecart | Terraria/Minecart.cs | D:\TRbackup\Version4\Terraria\Minecart.cs | 45 | 2 | RegularBumperDecoration | int | `public const int RegularBumperDecoration = 39;` | `public const int RegularBumperDecoration = 39;` |
| 3327 | field | Terraria.Minecart | Terraria/Minecart.cs | D:\TRbackup\Version4\Terraria\Minecart.cs | 59 | 2 | NoConnection | int | `private const int NoConnection = -1;` | `private const int NoConnection = -1;` |
| 3328 | field | Terraria.Minecart | Terraria/Minecart.cs | D:\TRbackup\Version4\Terraria\Minecart.cs | 61 | 2 | TopConnection | int | `private const int TopConnection = 0;` | `private const int TopConnection = 0;` |
| 3329 | field | Terraria.Minecart | Terraria/Minecart.cs | D:\TRbackup\Version4\Terraria\Minecart.cs | 63 | 2 | MiddleConnection | int | `private const int MiddleConnection = 1;` | `private const int MiddleConnection = 1;` |
| 3330 | field | Terraria.Minecart | Terraria/Minecart.cs | D:\TRbackup\Version4\Terraria\Minecart.cs | 65 | 2 | BottomConnection | int | `private const int BottomConnection = 2;` | `private const int BottomConnection = 2;` |
| 3331 | field | Terraria.Minecart | Terraria/Minecart.cs | D:\TRbackup\Version4\Terraria\Minecart.cs | 67 | 2 | BumperEnd | int | `private const int BumperEnd = -1;` | `private const int BumperEnd = -1;` |
| 3332 | field | Terraria.Minecart | Terraria/Minecart.cs | D:\TRbackup\Version4\Terraria\Minecart.cs | 69 | 2 | BouncyEnd | int | `private const int BouncyEnd = -2;` | `private const int BouncyEnd = -2;` |
| 3333 | field | Terraria.Minecart | Terraria/Minecart.cs | D:\TRbackup\Version4\Terraria\Minecart.cs | 71 | 2 | RampEnd | int | `private const int RampEnd = -3;` | `private const int RampEnd = -3;` |
| 3334 | field | Terraria.Minecart | Terraria/Minecart.cs | D:\TRbackup\Version4\Terraria\Minecart.cs | 73 | 2 | OpenEnd | int | `private const int OpenEnd = -4;` | `private const int OpenEnd = -4;` |
| 3343 | field | Terraria.Minecart | Terraria/Minecart.cs | D:\TRbackup\Version4\Terraria\Minecart.cs | 91 | 2 | _texturePosition | Vector2[] | `private static Vector2[] _texturePosition;` | `private static Vector2[] _texturePosition;` |
| 3347 | field | Terraria.Minecart | Terraria/Minecart.cs | D:\TRbackup\Version4\Terraria\Minecart.cs | 99 | 2 | _trackSwitchOptions | int[][] | `private static int[][] _trackSwitchOptions;` | `private static int[][] _trackSwitchOptions;` |
| 3348 | field | Terraria.Minecart | Terraria/Minecart.cs | D:\TRbackup\Version4\Terraria\Minecart.cs | 101 | 2 | _tileHeight | int[][] | `private static int[][] _tileHeight;` | `private static int[][] _tileHeight;` |

#### 属性（0）

无该类型成员记录。


### 4.9 细分子系统：`SharedGeneralTeleportAndInterceptionUtilities`

- 原报告章节：`4.9.157`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`05` / `空间移动与物理`

- 上一级基线细分子系统：`SharedGeneralPureUtilities`
- 细分职责：传送候选、追逐结果和拦截计算值对象。
- 边界角色：`query/value object`；最小 seam：general teleport interception port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：3；字段：20；属性：0；合计：20。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（20）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3627 | field | Terraria.Utils.RandomTeleportationAttemptSettings | Terraria/Utils.cs | D:\TRbackup\Version4\Terraria\Utils.cs | 40 | 3 | teleporteeSize | Vector2 | `public Vector2 teleporteeSize;` | `public Vector2 teleporteeSize;` |
| 3628 | field | Terraria.Utils.RandomTeleportationAttemptSettings | Terraria/Utils.cs | D:\TRbackup\Version4\Terraria\Utils.cs | 42 | 3 | teleporteeVelocity | Vector2 | `public Vector2 teleporteeVelocity;` | `public Vector2 teleporteeVelocity;` |
| 3629 | field | Terraria.Utils.RandomTeleportationAttemptSettings | Terraria/Utils.cs | D:\TRbackup\Version4\Terraria\Utils.cs | 44 | 3 | teleporteeGravityDirection | float | `public float teleporteeGravityDirection;` | `public float teleporteeGravityDirection;` |
| 3630 | field | Terraria.Utils.RandomTeleportationAttemptSettings | Terraria/Utils.cs | D:\TRbackup\Version4\Terraria\Utils.cs | 46 | 3 | mostlySolidFloor | bool | `public bool mostlySolidFloor;` | `public bool mostlySolidFloor;` |
| 3631 | field | Terraria.Utils.RandomTeleportationAttemptSettings | Terraria/Utils.cs | D:\TRbackup\Version4\Terraria\Utils.cs | 48 | 3 | avoidLava | bool | `public bool avoidLava;` | `public bool avoidLava;` |
| 3632 | field | Terraria.Utils.RandomTeleportationAttemptSettings | Terraria/Utils.cs | D:\TRbackup\Version4\Terraria\Utils.cs | 50 | 3 | avoidAnyLiquid | bool | `public bool avoidAnyLiquid;` | `public bool avoidAnyLiquid;` |
| 3633 | field | Terraria.Utils.RandomTeleportationAttemptSettings | Terraria/Utils.cs | D:\TRbackup\Version4\Terraria\Utils.cs | 52 | 3 | avoidHurtTiles | bool | `public bool avoidHurtTiles;` | `public bool avoidHurtTiles;` |
| 3634 | field | Terraria.Utils.RandomTeleportationAttemptSettings | Terraria/Utils.cs | D:\TRbackup\Version4\Terraria\Utils.cs | 54 | 3 | avoidWalls | bool | `public bool avoidWalls;` | `public bool avoidWalls;` |
| 3635 | field | Terraria.Utils.RandomTeleportationAttemptSettings | Terraria/Utils.cs | D:\TRbackup\Version4\Terraria\Utils.cs | 56 | 3 | attemptsBeforeGivingUp | int | `public int attemptsBeforeGivingUp;` | `public int attemptsBeforeGivingUp;` |
| 3636 | field | Terraria.Utils.RandomTeleportationAttemptSettings | Terraria/Utils.cs | D:\TRbackup\Version4\Terraria\Utils.cs | 58 | 3 | maximumFallDistanceFromOrignalPoint | int | `public int maximumFallDistanceFromOrignalPoint;` | `public int maximumFallDistanceFromOrignalPoint;` |
| 3637 | field | Terraria.Utils.RandomTeleportationAttemptSettings | Terraria/Utils.cs | D:\TRbackup\Version4\Terraria\Utils.cs | 60 | 3 | strictRange | bool | `public bool strictRange;` | `public bool strictRange;` |
| 3638 | field | Terraria.Utils.RandomTeleportationAttemptSettings | Terraria/Utils.cs | D:\TRbackup\Version4\Terraria\Utils.cs | 62 | 3 | tilesToAvoid | int[] | `public int[] tilesToAvoid;` | `public int[] tilesToAvoid;` |
| 3639 | field | Terraria.Utils.RandomTeleportationAttemptSettings | Terraria/Utils.cs | D:\TRbackup\Version4\Terraria\Utils.cs | 64 | 3 | tilesToAvoidRange | int | `public int tilesToAvoidRange;` | `public int tilesToAvoidRange;` |
| 3640 | field | Terraria.Utils.RandomTeleportationAttemptSettings | Terraria/Utils.cs | D:\TRbackup\Version4\Terraria\Utils.cs | 66 | 3 | allowSolidTopFloor | bool | `public bool allowSolidTopFloor;` | `public bool allowSolidTopFloor;` |
| 3641 | field | Terraria.Utils.RandomTeleportationAttemptSettings | Terraria/Utils.cs | D:\TRbackup\Version4\Terraria\Utils.cs | 68 | 3 | specializedConditions | System.Func<Terraria.Tile, int, int, bool> | `public Func<Tile, int, int, bool> specializedConditions;` | `public Func<Tile, int, int, bool> specializedConditions;` |
| 3642 | field | Terraria.Utils.ChaseResults | Terraria/Utils.cs | D:\TRbackup\Version4\Terraria\Utils.cs | 73 | 3 | InterceptionHappens | bool | `public bool InterceptionHappens;` | `public bool InterceptionHappens;` |
| 3643 | field | Terraria.Utils.ChaseResults | Terraria/Utils.cs | D:\TRbackup\Version4\Terraria\Utils.cs | 75 | 3 | InterceptionPosition | Vector2 | `public Vector2 InterceptionPosition;` | `public Vector2 InterceptionPosition;` |
| 3644 | field | Terraria.Utils.ChaseResults | Terraria/Utils.cs | D:\TRbackup\Version4\Terraria\Utils.cs | 77 | 3 | InterceptionTime | float | `public float InterceptionTime;` | `public float InterceptionTime;` |
| 3645 | field | Terraria.Utils.ChaseResults | Terraria/Utils.cs | D:\TRbackup\Version4\Terraria\Utils.cs | 79 | 3 | ChaserVelocity | Vector2 | `public Vector2 ChaserVelocity;` | `public Vector2 ChaserVelocity;` |
| 3646 | field | Terraria.Utils | Terraria/Utils.cs | D:\TRbackup\Version4\Terraria\Utils.cs | 82 | 2 | MaxCoins | long | `public const long MaxCoins = 9999999999L;` | `public const long MaxCoins = 9999999999L;` |

#### 属性（0）

无该类型成员记录。


### 4.10 细分子系统：`EntityBoundsAndFluidState`

- 原报告章节：`4.9.195`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`05` / `空间移动与物理`

- 上一级基线细分子系统：`EntityAuthoritativeState`
- 上一级 peer 细分子系统：`EntityAuthoritativeState`
- 细分职责：实体尺寸、碰撞边界、液体状态和空间范围。
- 边界角色：`state`；最小 seam：entity bounds fluid port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：7；属性：13；合计：20。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（7）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3067 | field | Terraria.Entity | Terraria/Entity.cs | D:\TRbackup\Version4\Terraria\Entity.cs | 22 | 2 | width | int | `public int width;` | `public int width;` |
| 3068 | field | Terraria.Entity | Terraria/Entity.cs | D:\TRbackup\Version4\Terraria\Entity.cs | 24 | 2 | height | int | `public int height;` | `public int height;` |
| 3069 | field | Terraria.Entity | Terraria/Entity.cs | D:\TRbackup\Version4\Terraria\Entity.cs | 26 | 2 | wet | bool | `public bool wet;` | `public bool wet;` |
| 3070 | field | Terraria.Entity | Terraria/Entity.cs | D:\TRbackup\Version4\Terraria\Entity.cs | 28 | 2 | shimmerWet | bool | `public bool shimmerWet;` | `public bool shimmerWet;` |
| 3071 | field | Terraria.Entity | Terraria/Entity.cs | D:\TRbackup\Version4\Terraria\Entity.cs | 30 | 2 | honeyWet | bool | `public bool honeyWet;` | `public bool honeyWet;` |
| 3072 | field | Terraria.Entity | Terraria/Entity.cs | D:\TRbackup\Version4\Terraria\Entity.cs | 32 | 2 | wetCount | byte | `public byte wetCount;` | `public byte wetCount;` |
| 3073 | field | Terraria.Entity | Terraria/Entity.cs | D:\TRbackup\Version4\Terraria\Entity.cs | 34 | 2 | lavaWet | bool | `public bool lavaWet;` | `public bool lavaWet;` |

#### 属性（13）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3944 | property | Terraria.Entity | Terraria/Entity.cs | D:\TRbackup\Version4\Terraria\Entity.cs | 36 | 2 | AnyWet | bool | `public bool AnyWet { get { if (!wet && !lavaWet && !honeyWet) { return shimmerWet; } return true; } }` | `public bool AnyWet { get { if (!wet && !lavaWet && !honeyWet) { return shimmerWet; } return true; } }` |
| 3945 | property | Terraria.Entity | Terraria/Entity.cs | D:\TRbackup\Version4\Terraria\Entity.cs | 48 | 2 | VisualPosition | Vector2 | `public virtual Vector2 VisualPosition => position;` | `public virtual Vector2 VisualPosition => position;` |
| 3946 | property | Terraria.Entity | Terraria/Entity.cs | D:\TRbackup\Version4\Terraria\Entity.cs | 50 | 2 | Center | Vector2 | `public Vector2 Center { get { return new Vector2(position.X + (float)width / 2f, position.Y + (float)height / 2f); } set { position = new Vector2(value.X - (float)width / 2f, value.Y - (float)height / 2f); } }` | `public Vector2 Center { get { return new Vector2(position.X + (float)width / 2f, position.Y + (float)height / 2f); } set { position = new Vector2(value.X - (float)width / 2f, value.Y - (float)height / 2f); } }` |
| 3947 | property | Terraria.Entity | Terraria/Entity.cs | D:\TRbackup\Version4\Terraria\Entity.cs | 62 | 2 | Left | Vector2 | `public Vector2 Left { get { return new Vector2(position.X, position.Y + (float)height / 2f); } set { position = new Vector2(value.X, value.Y - (float)height / 2f); } }` | `public Vector2 Left { get { return new Vector2(position.X, position.Y + (float)height / 2f); } set { position = new Vector2(value.X, value.Y - (float)height / 2f); } }` |
| 3948 | property | Terraria.Entity | Terraria/Entity.cs | D:\TRbackup\Version4\Terraria\Entity.cs | 74 | 2 | Right | Vector2 | `public Vector2 Right { get { return new Vector2(position.X + (float)width, position.Y + (float)height / 2f); } set { position = new Vector2(value.X - (float)width, value.Y - (float)height / 2f); } }` | `public Vector2 Right { get { return new Vector2(position.X + (float)width, position.Y + (float)height / 2f); } set { position = new Vector2(value.X - (float)width, value.Y - (float)height / 2f); } }` |
| 3949 | property | Terraria.Entity | Terraria/Entity.cs | D:\TRbackup\Version4\Terraria\Entity.cs | 86 | 2 | Top | Vector2 | `public Vector2 Top { get { return new Vector2(position.X + (float)width / 2f, position.Y); } set { position = new Vector2(value.X - (float)width / 2f, value.Y); } }` | `public Vector2 Top { get { return new Vector2(position.X + (float)width / 2f, position.Y); } set { position = new Vector2(value.X - (float)width / 2f, value.Y); } }` |
| 3950 | property | Terraria.Entity | Terraria/Entity.cs | D:\TRbackup\Version4\Terraria\Entity.cs | 98 | 2 | TopLeft | Vector2 | `public Vector2 TopLeft { get { return position; } set { position = value; } }` | `public Vector2 TopLeft { get { return position; } set { position = value; } }` |
| 3951 | property | Terraria.Entity | Terraria/Entity.cs | D:\TRbackup\Version4\Terraria\Entity.cs | 110 | 2 | TopRight | Vector2 | `public Vector2 TopRight { get { return new Vector2(position.X + (float)width, position.Y); } set { position = new Vector2(value.X - (float)width, value.Y); } }` | `public Vector2 TopRight { get { return new Vector2(position.X + (float)width, position.Y); } set { position = new Vector2(value.X - (float)width, value.Y); } }` |
| 3952 | property | Terraria.Entity | Terraria/Entity.cs | D:\TRbackup\Version4\Terraria\Entity.cs | 122 | 2 | Bottom | Vector2 | `public Vector2 Bottom { get { return new Vector2(position.X + (float)width / 2f, position.Y + (float)height); } set { position = new Vector2(value.X - (float)width / 2f, value.Y - (float)height); } }` | `public Vector2 Bottom { get { return new Vector2(position.X + (float)width / 2f, position.Y + (float)height); } set { position = new Vector2(value.X - (float)width / 2f, value.Y - (float)height); } }` |
| 3953 | property | Terraria.Entity | Terraria/Entity.cs | D:\TRbackup\Version4\Terraria\Entity.cs | 134 | 2 | BottomLeft | Vector2 | `public Vector2 BottomLeft { get { return new Vector2(position.X, position.Y + (float)height); } set { position = new Vector2(value.X, value.Y - (float)height); } }` | `public Vector2 BottomLeft { get { return new Vector2(position.X, position.Y + (float)height); } set { position = new Vector2(value.X, value.Y - (float)height); } }` |
| 3954 | property | Terraria.Entity | Terraria/Entity.cs | D:\TRbackup\Version4\Terraria\Entity.cs | 146 | 2 | BottomRight | Vector2 | `public Vector2 BottomRight { get { return new Vector2(position.X + (float)width, position.Y + (float)height); } set { position = new Vector2(value.X - (float)width, value.Y - (float)height); } }` | `public Vector2 BottomRight { get { return new Vector2(position.X + (float)width, position.Y + (float)height); } set { position = new Vector2(value.X - (float)width, value.Y - (float)height); } }` |
| 3955 | property | Terraria.Entity | Terraria/Entity.cs | D:\TRbackup\Version4\Terraria\Entity.cs | 158 | 2 | Size | Vector2 | `public Vector2 Size { get { return new Vector2(width, height); } set { width = (int)value.X; height = (int)value.Y; } }` | `public Vector2 Size { get { return new Vector2(width, height); } set { width = (int)value.X; height = (int)value.Y; } }` |
| 3956 | property | Terraria.Entity | Terraria/Entity.cs | D:\TRbackup\Version4\Terraria\Entity.cs | 171 | 2 | Hitbox | Rectangle | `public Rectangle Hitbox { get { return new Rectangle((int)position.X, (int)position.Y, width, height); } set { position = new Vector2(value.X, value.Y); width = value.Width; height = value.Height; } }` | `public Rectangle Hitbox { get { return new Rectangle((int)position.X, (int)position.Y, width, height); } set { position = new Vector2(value.X, value.Y); width = value.Width; height = value.Height; } }` |


## 5. 追溯与验收

> 来源报告 SHA-256：`b944441d92d5103f218b766a3e354200794528a19e65a862e44c2b40a36f1196`
- 本分区细分组数：10；成员数：119；字段：99；属性：20。
- 预期不变量：本文件只出现完整细分子系统；不跨分区复制成员；所有成员的来源序号、原始声明、路径、行列和 C# 类型必须与来源报告一致。
- 验证方式：重新读取 20 个文件，检查文件数、细分组唯一性、来源序号恰好覆盖 `1..4542`、字段/属性合计和逐成员行文本一致；本次分区生成不运行 `dotnet`，因为没有修改 C# 或项目文件。

生成器：`Build/Tools/Generate-Version4NonAuthoritativeComponentPartitionReports.ps1`。
