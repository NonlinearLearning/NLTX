# P08 玩家环境、目标定位、物理与机动装备 - 权威模拟系统成员组件候选分区

> 来源：`docs/migration/ledgers/Version4权威模拟系统字段属性逐成员源码声明-更细子系统拆分-去除ID类文件.md`；本文件只做成员库存分区和候选组件边界记录。
> 这是候选组件拆分台账，不是运行时迁移完成、行为等价、API 兼容、网络闭合或持久化闭合证明。

## 1. 分区边界

本分区包含 9 个叶子子系统，字段 100 条、属性 0 条、成员合计 100 条；每个叶子子系统保持原正式父级归属，不跨分区拆行。

## 2. 分区覆盖概览

| 叶子子系统 | 正式父级 | 字段 | 属性 | 合计 | 源边界角色 |
|---|---|---:|---:|---:|---|
| `PlayerZoneAndEnvironmentState` | `PlayerGameplay` | 7 | 0 | 7 | authoritative state/behavior |
| `PlayerTileTargetingAndRangeState` | `PlayerGameplay` | 12 | 0 | 12 | authoritative state/behavior |
| `PlayerMovementPhysicsState` | `PlayerGameplay` | 8 | 0 | 8 | authoritative state/behavior |
| `PlayerEnvironmentMobilityState` | `PlayerGameplay` | 12 | 0 | 12 | authoritative state/behavior |
| `PlayerEnvironmentDetectionAndSpawnState` | `PlayerGameplay` | 13 | 0 | 13 | authoritative state/behavior |
| `PlayerArmorAndCombatEffects` | `PlayerGameplay` | 8 | 0 | 8 | authoritative state/behavior |
| `PlayerArmorSetAndTurretState` | `PlayerGameplay` | 19 | 0 | 19 | authoritative state/behavior |
| `PlayerGravityAndWaterTraversalState` | `PlayerGameplay` | 5 | 0 | 5 | authoritative state/behavior |
| `PlayerLuckAndRescanState` | `PlayerGameplay` | 16 | 0 | 16 | authoritative state/behavior |

## 3. 候选组件归属与 seam

| 叶子子系统 | 候选组件边界 | 类型分类 | 最小 seam | 独立生命周期 | 证据状态 | 主要风险 |
|---|---|---|---|---|---|---|
| `PlayerZoneAndEnvironmentState` | `PlayerZoneAndEnvironmentStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerTileTargetingAndRangeState` | `PlayerTileTargetingAndRangeStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerMovementPhysicsState` | `PlayerMovementPhysicsStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerEnvironmentMobilityState` | `PlayerEnvironmentMobilityStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerEnvironmentDetectionAndSpawnState` | `PlayerEnvironmentDetectionAndSpawnStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerArmorAndCombatEffects` | `PlayerArmorAndCombatEffectsComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerArmorSetAndTurretState` | `PlayerArmorSetAndTurretStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerGravityAndWaterTraversalState` | `PlayerGravityAndWaterTraversalStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerLuckAndRescanState` | `PlayerLuckAndRescanStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |

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

### 正式父级子系统：`PlayerGameplay`
- 父级职责：沿用源报告正式父级 `PlayerGameplay`；本分区只收录该父级中列出的叶子子系统。
- 本分区父级局部统计：字段 100；属性 0；合计 100；完整父级统计以源报告为准。

#### 4.13.24 细分子系统：`PlayerZoneAndEnvironmentState`

- 细分职责：区域位标、微光区域和环境免疫计时状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；区域扫描通过只读查询驱动状态提交。
- 成员文件数：1；声明类型数：1；字段：7；属性：0；合计：7。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（7）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 662 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 917 | 2 | environmentBuffImmunityTimer | int | `public int environmentBuffImmunityTimer;` | `public int environmentBuffImmunityTimer;` |
| 665 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 923 | 2 | zone1 | Terraria.BitsByte | `public BitsByte zone1 = (byte)0;` | `public BitsByte zone1 = (byte)0;` |
| 666 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 925 | 2 | zone2 | Terraria.BitsByte | `public BitsByte zone2 = (byte)0;` | `public BitsByte zone2 = (byte)0;` |
| 667 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 927 | 2 | zone3 | Terraria.BitsByte | `public BitsByte zone3 = (byte)0;` | `public BitsByte zone3 = (byte)0;` |
| 668 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 929 | 2 | zone4 | Terraria.BitsByte | `public BitsByte zone4 = (byte)0;` | `public BitsByte zone4 = (byte)0;` |
| 669 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 931 | 2 | zone5 | Terraria.BitsByte | `public BitsByte zone5 = (byte)0;` | `public BitsByte zone5 = (byte)0;` |
| 670 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 933 | 2 | _wasInShimmerZone | bool | `private bool _wasInShimmerZone;` | `private bool _wasInShimmerZone;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.65 细分子系统：`PlayerTileTargetingAndRangeState`

- 细分职责：Tile 交互范围、目标坐标、邻接标记和物品吸取范围。
- 边界角色：`authoritative state/behavior`；最小 seam：Tile Interaction System/CommitPort；交互阶段显式提交范围。
- 成员文件数：1；声明类型数：1；字段：12；属性：0；合计：12。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（12）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1136 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1905 | 2 | DefaultTileRangeX | int | `public static readonly int DefaultTileRangeX = 5;` | `public static readonly int DefaultTileRangeX = 5;` |
| 1137 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1907 | 2 | DefaultTileRangeY | int | `public static readonly int DefaultTileRangeY = 3;` | `public static readonly int DefaultTileRangeY = 3;` |
| 1138 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1909 | 2 | tileRangeX | int | `public static int tileRangeX = DefaultTileRangeX;` | `public static int tileRangeX = DefaultTileRangeX;` |
| 1139 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1911 | 2 | tileRangeY | int | `public static int tileRangeY = DefaultTileRangeY;` | `public static int tileRangeY = DefaultTileRangeY;` |
| 1140 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1913 | 2 | lastTileRangeX | int | `public int lastTileRangeX;` | `public int lastTileRangeX;` |
| 1141 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1915 | 2 | lastTileRangeY | int | `public int lastTileRangeY;` | `public int lastTileRangeY;` |
| 1142 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1917 | 2 | tileTargetX | int | `public static int tileTargetX;` | `public static int tileTargetX;` |
| 1143 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1919 | 2 | tileTargetY | int | `public static int tileTargetY;` | `public static int tileTargetY;` |
| 1152 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1939 | 2 | adjTile | bool[] | `public bool[] adjTile = new bool[TileID.Count];` | `public bool[] adjTile = new bool[TileID.Count];` |
| 1153 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1941 | 2 | defaultItemGrabRange | int | `public static int defaultItemGrabRange = 42;` | `public static int defaultItemGrabRange = 42;` |
| 1154 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1943 | 2 | itemGrabSpeed | float | `private static float itemGrabSpeed = 0.45f;` | `private static float itemGrabSpeed = 0.45f;` |
| 1155 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1945 | 2 | itemGrabSpeedMax | float | `private static float itemGrabSpeedMax = 4f;` | `private static float itemGrabSpeedMax = 4f;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.66 细分子系统：`PlayerMovementPhysicsState`

- 细分职责：重力、跳跃、下落和奔跑物理参数。
- 边界角色：`authoritative state/behavior`；最小 seam：Movement System/CommitPort；移动 tick 集中维护物理参数。
- 成员文件数：1；声明类型数：1；字段：8；属性：0；合计：8。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（8）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1144 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1921 | 2 | defaultGravity | float | `public static float defaultGravity = 0.4f;` | `public static float defaultGravity = 0.4f;` |
| 1145 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1923 | 2 | jumpHeight | int | `public static int jumpHeight = 15;` | `public static int jumpHeight = 15;` |
| 1146 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1925 | 2 | jumpSpeed | float | `public static float jumpSpeed = 5.01f;` | `public static float jumpSpeed = 5.01f;` |
| 1147 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1927 | 2 | gravity | float | `public float gravity = defaultGravity;` | `public float gravity = defaultGravity;` |
| 1148 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1929 | 2 | maxFallSpeed | float | `public float maxFallSpeed = 10f;` | `public float maxFallSpeed = 10f;` |
| 1149 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1931 | 2 | maxRunSpeed | float | `public float maxRunSpeed = 3f;` | `public float maxRunSpeed = 3f;` |
| 1150 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1933 | 2 | runAcceleration | float | `public float runAcceleration = 0.08f;` | `public float runAcceleration = 0.08f;` |
| 1151 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1935 | 2 | runSlowdown | float | `public float runSlowdown = 0.2f;` | `public float runSlowdown = 0.2f;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.78 细分子系统：`PlayerEnvironmentMobilityState`

- 细分职责：水体、跳跃、移动能力和环境移动约束。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；环境查询作为只读输入，能力状态集中提交。
- 成员文件数：1；声明类型数：1；字段：12；属性：0；合计：12。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（12）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1240 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2124 | 2 | canFloatInWater | bool | `public bool canFloatInWater;` | `public bool canFloatInWater;` |
| 1241 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2126 | 2 | hasFloatingTube | bool | `public bool hasFloatingTube;` | `public bool hasFloatingTube;` |
| 1242 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2128 | 2 | frogLegJumpBoost | bool | `public bool frogLegJumpBoost;` | `public bool frogLegJumpBoost;` |
| 1243 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2130 | 2 | skyStoneEffects | bool | `public bool skyStoneEffects;` | `public bool skyStoneEffects;` |
| 1244 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2132 | 2 | spawnMax | bool | `public bool spawnMax;` | `public bool spawnMax;` |
| 1245 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2134 | 2 | blockRange | int | `public int blockRange;` | `public int blockRange;` |
| 1258 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2160 | 2 | jumpBoost | bool | `public bool jumpBoost;` | `public bool jumpBoost;` |
| 1259 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2162 | 2 | noFallDmg | bool | `public bool noFallDmg;` | `public bool noFallDmg;` |
| 1260 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2164 | 2 | swimTime | int | `public int swimTime;` | `public int swimTime;` |
| 1266 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2176 | 2 | lavaImmune | bool | `public bool lavaImmune;` | `public bool lavaImmune;` |
| 1267 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2178 | 2 | gills | bool | `public bool gills;` | `public bool gills;` |
| 1268 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2180 | 2 | slowFall | bool | `public bool slowFall;` | `public bool slowFall;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.79 细分子系统：`PlayerEnvironmentDetectionAndSpawnState`

- 细分职责：环境感知、生成规则和环境交互效果状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；环境扫描结果通过显式快照输入。
- 成员文件数：1；声明类型数：1；字段：13；属性：0；合计：13。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（13）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1261 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2166 | 2 | killGuide | bool | `public bool killGuide;` | `public bool killGuide;` |
| 1262 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2168 | 2 | killClothier | bool | `public bool killClothier;` | `public bool killClothier;` |
| 1263 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2170 | 2 | equipmentBasedLuckBonus | float | `public float equipmentBasedLuckBonus;` | `public float equipmentBasedLuckBonus;` |
| 1264 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2172 | 2 | lastEquipmentBasedLuckBonus | float | `public float lastEquipmentBasedLuckBonus;` | `public float lastEquipmentBasedLuckBonus;` |
| 1265 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2174 | 2 | hasCreditsSceneMusicBox | bool | `public bool hasCreditsSceneMusicBox;` | `public bool hasCreditsSceneMusicBox;` |
| 1269 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2182 | 2 | findTreasure | bool | `public bool findTreasure;` | `public bool findTreasure;` |
| 1270 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2184 | 2 | biomeSight | bool | `public bool biomeSight;` | `public bool biomeSight;` |
| 1271 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2186 | 2 | invis | bool | `public bool invis;` | `public bool invis;` |
| 1272 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2188 | 2 | detectCreature | bool | `public bool detectCreature;` | `public bool detectCreature;` |
| 1273 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2190 | 2 | nightVision | bool | `public bool nightVision;` | `public bool nightVision;` |
| 1274 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2192 | 2 | enemySpawns | bool | `public bool enemySpawns;` | `public bool enemySpawns;` |
| 1282 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2208 | 2 | insideUnbreakableWalls | bool | `public bool insideUnbreakableWalls;` | `public bool insideUnbreakableWalls;` |
| 1283 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2210 | 2 | CanSeeInvisibleBlocks | bool | `public bool CanSeeInvisibleBlocks;` | `public bool CanSeeInvisibleBlocks;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.80 细分子系统：`PlayerArmorAndCombatEffects`

- 细分职责：护甲反伤、日照、荆棘和战斗效果状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；受击与装备事件单向驱动效果更新。
- 成员文件数：1；声明类型数：1；字段：8；属性：0；合计：8。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（8）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1275 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2194 | 2 | thorns | float | `public float thorns;` | `public float thorns;` |
| 1276 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2196 | 2 | turtleArmor | bool | `public bool turtleArmor;` | `public bool turtleArmor;` |
| 1277 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2198 | 2 | turtleThorns | bool | `public bool turtleThorns;` | `public bool turtleThorns;` |
| 1278 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2200 | 2 | cactusThorns | bool | `public bool cactusThorns;` | `public bool cactusThorns;` |
| 1279 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2202 | 2 | spiderArmor | bool | `public bool spiderArmor;` | `public bool spiderArmor;` |
| 1280 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2204 | 2 | anglerSetSpawnReduction | bool | `public bool anglerSetSpawnReduction;` | `public bool anglerSetSpawnReduction;` |
| 1281 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2206 | 2 | vampireBurningInSunlight | bool | `public bool vampireBurningInSunlight;` | `public bool vampireBurningInSunlight;` |
| 1308 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2260 | 2 | honeyCombItem | Terraria.Item | `public Item honeyCombItem;` | `public Item honeyCombItem;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.81 细分子系统：`PlayerArmorSetAndTurretState`

- 细分职责：套装效果、炮塔容量和 Vortex 隐身状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；套装效果按能力阶段提交。
- 成员文件数：1；声明类型数：1；字段：19；属性：0；合计：19。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（19）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1284 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2212 | 2 | setSolar | bool | `public bool setSolar;` | `public bool setSolar;` |
| 1285 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2214 | 2 | setVortex | bool | `public bool setVortex;` | `public bool setVortex;` |
| 1286 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2216 | 2 | setNebula | bool | `public bool setNebula;` | `public bool setNebula;` |
| 1287 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2218 | 2 | nebulaCD | int | `public int nebulaCD;` | `public int nebulaCD;` |
| 1288 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2220 | 2 | setStardust | bool | `public bool setStardust;` | `public bool setStardust;` |
| 1289 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2222 | 2 | setForbidden | bool | `public bool setForbidden;` | `public bool setForbidden;` |
| 1290 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2224 | 2 | setForbiddenCooldownLocked | bool | `public bool setForbiddenCooldownLocked;` | `public bool setForbiddenCooldownLocked;` |
| 1291 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2226 | 2 | setChlorophyte | bool | `public bool setChlorophyte;` | `public bool setChlorophyte;` |
| 1292 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2228 | 2 | setSquireT3 | bool | `public bool setSquireT3;` | `public bool setSquireT3;` |
| 1293 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2230 | 2 | setHuntressT3 | bool | `public bool setHuntressT3;` | `public bool setHuntressT3;` |
| 1294 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2232 | 2 | setApprenticeT3 | bool | `public bool setApprenticeT3;` | `public bool setApprenticeT3;` |
| 1295 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2234 | 2 | setMonkT3 | bool | `public bool setMonkT3;` | `public bool setMonkT3;` |
| 1296 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2236 | 2 | setSquireT2 | bool | `public bool setSquireT2;` | `public bool setSquireT2;` |
| 1297 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2238 | 2 | setHuntressT2 | bool | `public bool setHuntressT2;` | `public bool setHuntressT2;` |
| 1298 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2240 | 2 | setApprenticeT2 | bool | `public bool setApprenticeT2;` | `public bool setApprenticeT2;` |
| 1299 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2242 | 2 | setMonkT2 | bool | `public bool setMonkT2;` | `public bool setMonkT2;` |
| 1300 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2244 | 2 | maxTurrets | int | `public int maxTurrets = 1;` | `public int maxTurrets = 1;` |
| 1301 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2246 | 2 | maxTurretsOld | int | `public int maxTurretsOld = 1;` | `public int maxTurretsOld = 1;` |
| 1302 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2248 | 2 | vortexStealthActive | bool | `public bool vortexStealthActive;` | `public bool vortexStealthActive;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.82 细分子系统：`PlayerGravityAndWaterTraversalState`

- 细分职责：水面行走、重力方向和重力控制状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；移动系统通过 Query 读取，不跨组隐式写入。
- 成员文件数：1；声明类型数：1；字段：5；属性：0；合计：5。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（5）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1303 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2250 | 2 | waterWalk | bool | `public bool waterWalk;` | `public bool waterWalk;` |
| 1304 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2252 | 2 | waterWalk2 | bool | `public bool waterWalk2;` | `public bool waterWalk2;` |
| 1305 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2254 | 2 | forcedGravity | int | `public int forcedGravity;` | `public int forcedGravity;` |
| 1306 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2256 | 2 | gravControl | bool | `public bool gravControl;` | `public bool gravControl;` |
| 1307 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2258 | 2 | gravControl2 | bool | `public bool gravControl2;` | `public bool gravControl2;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.89 细分子系统：`PlayerLuckAndRescanState`

- 细分职责：幸运状态、墙体扫描缓存和相关音效/重扫描状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System 读取输入并通过显式 Command/CommitPort 写入；跨组只用事件、Query 或命令交接。
- 成员文件数：1；声明类型数：1；字段：16；属性：0；合计：16。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（16）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1386 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2423 | 2 | torchLuck | float | `public float torchLuck;` | `public float torchLuck;` |
| 1387 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2425 | 2 | happyFunTorchTime | bool | `public bool happyFunTorchTime;` | `public bool happyFunTorchTime;` |
| 1388 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2429 | 2 | ladyBugLuckTimeLeft | int | `public int ladyBugLuckTimeLeft;` | `public int ladyBugLuckTimeLeft;` |
| 1389 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2431 | 2 | luck | float | `public float luck;` | `public float luck;` |
| 1390 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2433 | 2 | luckMinimumCap | float | `public float luckMinimumCap = -0.7f;` | `public float luckMinimumCap = -0.7f;` |
| 1391 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2435 | 2 | luckMaximumCap | float | `public float luckMaximumCap = 1f;` | `public float luckMaximumCap = 1f;` |
| 1392 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2437 | 2 | coinLuck | float | `public float coinLuck;` | `public float coinLuck;` |
| 1393 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2439 | 2 | kiteLuckLevel | byte | `public byte kiteLuckLevel;` | `public byte kiteLuckLevel;` |
| 1394 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2441 | 2 | luckNeedsSync | bool | `public bool luckNeedsSync;` | `public bool luckNeedsSync;` |
| 1395 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2443 | 2 | disableVoidBag | int | `public int disableVoidBag = -1;` | `public int disableVoidBag = -1;` |
| 1396 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2445 | 2 | movementAbilitiesCache | Terraria.DataStructures.PlayerMovementAccsCache | `public PlayerMovementAccsCache movementAbilitiesCache;` | `public PlayerMovementAccsCache movementAbilitiesCache;` |
| 1397 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2449 | 2 | UnbreakableWallRescanPeriod | int | `private static readonly int UnbreakableWallRescanPeriod = 20;` | `private static readonly int UnbreakableWallRescanPeriod = 20;` |
| 1398 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2451 | 2 | UnbreakableWallRescanDistance | int | `private static readonly int UnbreakableWallRescanDistance = 256;` | `private static readonly int UnbreakableWallRescanDistance = 256;` |
| 1399 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2453 | 2 | _unbreakableWallScanCooldown | int | `private int _unbreakableWallScanCooldown;` | `private int _unbreakableWallScanCooldown;` |
| 1400 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2455 | 2 | _unbreakableWallScanLastPosition | Vector2 | `private Vector2 _unbreakableWallScanLastPosition;` | `private Vector2 _unbreakableWallScanLastPosition;` |
| 1401 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2457 | 2 | _sizzleAudioHandle | SlotId | `private SlotId _sizzleAudioHandle;` | `private SlotId _sizzleAudioHandle;` |

##### 属性（0）

无该类型成员记录。


## 8. 本分区自检

- 叶子子系统：9 个；指定组均已写入且没有重复分区。
- 字段/属性/成员：100 / 0 / 100。
- 来源序号范围：662..1401；本分区内唯一序号：True。
- 成员声明表逐组从源报告提取，未改写来源序号、类型、路径、行列、成员、C# 类型、成员声明或原始声明。
- 运行时读写者、生命周期、持久化、网络和调度：未验证，保留为迁移前风险。
