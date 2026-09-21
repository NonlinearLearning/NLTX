# P15 投射物身份、生命周期、运动、战斗与查询 - 权威模拟系统成员组件候选分区

> 来源：`docs/migration/ledgers/Version4权威模拟系统字段属性逐成员源码声明-更细子系统拆分-去除ID类文件.md`；本文件只做成员库存分区和候选组件边界记录。
> 这是候选组件拆分台账，不是运行时迁移完成、行为等价、API 兼容、网络闭合或持久化闭合证明。

## 1. 分区边界

本分区包含 17 个叶子子系统，字段 118 条、属性 8 条、成员合计 126 条；每个叶子子系统保持原正式父级归属，不跨分区拆行。

## 2. 分区覆盖概览

| 叶子子系统 | 正式父级 | 字段 | 属性 | 合计 | 源边界角色 |
|---|---|---:|---:|---:|---|
| `ProjectileIdentityAndClassificationState` | `ProjectileSimulation` | 16 | 0 | 16 | authoritative state/behavior |
| `ProjectileAiState` | `ProjectileSimulation` | 4 | 0 | 4 | authoritative state/behavior |
| `ProjectileLifetimeAndRuntimeState` | `ProjectileSimulation` | 6 | 0 | 6 | authoritative state/behavior |
| `ProjectileCombatAndImmunity` | `ProjectileSimulation` | 14 | 0 | 14 | authoritative state/behavior |
| `ProjectileMovementAndCollisionState` | `ProjectileSimulation` | 9 | 0 | 9 | authoritative state/behavior |
| `ProjectileNetworkReplicationState` | `ProjectileSimulation` | 4 | 0 | 4 | registry/projection |
| `ProjectileMinionAndPresentationState` | `ProjectileSimulation` | 13 | 0 | 13 | authoritative state/behavior |
| `ProjectileDamageAndElementState` | `ProjectileSimulation` | 13 | 0 | 13 | authoritative state/behavior |
| `ProjectileAnimationAndDirectionState` | `ProjectileSimulation` | 3 | 0 | 3 | authoritative state/behavior |
| `ProjectileCollisionAndTargetingState` | `ProjectileSimulation` | 7 | 0 | 7 | authoritative state/behavior |
| `ProjectileCombatScalingState` | `ProjectileSimulation` | 2 | 0 | 2 | authoritative state/behavior |
| `ProjectileCollisionGeometryCache` | `ProjectileSimulation` | 8 | 0 | 8 | derived/query |
| `ProjectileTargetSelectionCache` | `ProjectileSimulation` | 7 | 0 | 7 | derived/query |
| `ProjectileFishingAndMiningQueryState` | `ProjectileSimulation` | 3 | 0 | 3 | derived/query |
| `ProjectileKiteAndLightningRules` | `ProjectileSimulation` | 2 | 0 | 2 | definition/query |
| `ProjectileDerivedProperties` | `ProjectileSimulation` | 0 | 8 | 8 | derived/query |
| `ProjectileStormDefinition` | `ProjectileSimulation` | 7 | 0 | 7 | definition/query |

## 3. 候选组件归属与 seam

| 叶子子系统 | 候选组件边界 | 类型分类 | 最小 seam | 独立生命周期 | 证据状态 | 主要风险 |
|---|---|---|---|---|---|---|
| `ProjectileIdentityAndClassificationState` | `ProjectileIdentityAndClassificationStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `ProjectileAiState` | `ProjectileAiStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `ProjectileLifetimeAndRuntimeState` | `ProjectileLifetimeAndRuntimeStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `ProjectileCombatAndImmunity` | `ProjectileCombatAndImmunityComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `ProjectileMovementAndCollisionState` | `ProjectileMovementAndCollisionStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `ProjectileNetworkReplicationState` | `ProjectileNetworkReplicationStateProjection`/`Adapter` | Projection/Adapter | 单向 Projection/Adapter seam；不得回写权威状态 | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `ProjectileMinionAndPresentationState` | `ProjectileMinionAndPresentationStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `ProjectileDamageAndElementState` | `ProjectileDamageAndElementStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `ProjectileAnimationAndDirectionState` | `ProjectileAnimationAndDirectionStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `ProjectileCollisionAndTargetingState` | `ProjectileCollisionAndTargetingStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `ProjectileCombatScalingState` | `ProjectileCombatScalingStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `ProjectileCollisionGeometryCache` | `ProjectileCollisionGeometryCacheQuery` | Query | 纯资格或派生 Query；只读 | 候选纯读边界；仍需核对失效/缓存条件 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `ProjectileTargetSelectionCache` | `ProjectileTargetSelectionCacheQuery` | Query | 纯资格或派生 Query；只读 | 候选纯读边界；仍需核对失效/缓存条件 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `ProjectileFishingAndMiningQueryState` | `ProjectileFishingAndMiningQueryStateQuery` | Query | 纯资格或派生 Query；只读 | 候选纯读边界；仍需核对失效/缓存条件 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `ProjectileKiteAndLightningRules` | `ProjectileKiteAndLightningRulesDefinition`/`Query` | Definition/Query | 只读 Definition/Catalog view | 候选纯读边界；仍需核对失效/缓存条件 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `ProjectileDerivedProperties` | `ProjectileDerivedPropertiesQuery` | Query | 纯资格或派生 Query；只读 | 候选纯读边界；仍需核对失效/缓存条件 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `ProjectileStormDefinition` | `ProjectileStormDefinitionDefinition`/`Query` | Definition/Query | 只读 Definition/Catalog view | 候选纯读边界；仍需核对失效/缓存条件 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |

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

### 正式父级子系统：`ProjectileSimulation`
- 父级职责：沿用源报告正式父级 `ProjectileSimulation`；本分区只收录该父级中列出的叶子子系统。
- 本分区父级局部统计：字段 118；属性 8；合计 126；完整父级统计以源报告为准。

#### 4.15.1 细分子系统：`ProjectileIdentityAndClassificationState`

- 细分职责：投射物激活、所有者、类型、分类和静态身份相关状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；生成命令建立并提交身份状态。
- 成员文件数：1；声明类型数：1；字段：16；属性：0；合计：16。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（16）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1918 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 90 | 2 | active | bool | `public bool active;` | `public bool active;` |
| 1919 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 92 | 2 | perIDStaticNPCImmunity | uint[][] | `public static uint[][] perIDStaticNPCImmunity = new uint[ProjectileID.Count][];` | `public static uint[][] perIDStaticNPCImmunity = new uint[ProjectileID.Count][];` |
| 1922 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 98 | 2 | ownerHitCheckDistance | float | `public float ownerHitCheckDistance = 1000f;` | `public float ownerHitCheckDistance = 1000f;` |
| 1923 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 100 | 2 | arrow | bool | `public bool arrow;` | `public bool arrow;` |
| 1924 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 102 | 2 | numHits | int | `public int numHits;` | `public int numHits;` |
| 1925 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 104 | 2 | bobber | bool | `public bool bobber;` | `public bool bobber;` |
| 1926 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 106 | 2 | netImportant | bool | `public bool netImportant;` | `public bool netImportant;` |
| 1927 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 108 | 2 | noDropItem | bool | `public bool noDropItem;` | `public bool noDropItem;` |
| 1929 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 112 | 2 | counterweight | bool | `public bool counterweight;` | `public bool counterweight;` |
| 1930 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 114 | 2 | scale | float | `public float scale = 1f;` | `public float scale = 1f;` |
| 1931 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 116 | 2 | rotation | float | `public float rotation;` | `public float rotation;` |
| 1932 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 118 | 2 | type | int | `public int type;` | `public int type;` |
| 1933 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 120 | 2 | alpha | int | `public int alpha;` | `public int alpha;` |
| 1934 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 122 | 2 | sentry | bool | `public bool sentry;` | `public bool sentry;` |
| 1935 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 124 | 2 | glowMask | short | `public short glowMask;` | `public short glowMask;` |
| 1936 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 126 | 2 | owner | int | `public int owner = 255;` | `public int owner = 255;` |

##### 属性（0）

无该类型成员记录。


#### 4.15.2 细分子系统：`ProjectileAiState`

- 细分职责：投射物 AI 数组、局部 AI 和 AI 风格状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；AI 系统是本组唯一行为写入者。
- 成员文件数：1；声明类型数：1；字段：4；属性：0；合计：4。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（4）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1928 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 110 | 2 | maxAI | int | `public static int maxAI = 3;` | `public static int maxAI = 3;` |
| 1937 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 128 | 2 | ai | float[] | `public float[] ai = new float[maxAI];` | `public float[] ai = new float[maxAI];` |
| 1938 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 130 | 2 | localAI | float[] | `public float[] localAI = new float[maxAI];` | `public float[] localAI = new float[maxAI];` |
| 1941 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 136 | 2 | aiStyle | int | `public int aiStyle;` | `public int aiStyle;` |

##### 属性（0）

无该类型成员记录。


#### 4.15.3 细分子系统：`ProjectileLifetimeAndRuntimeState`

- 细分职责：投射物生命周期计时、运行步进、偏移和声音延迟状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；生命周期系统按 Tick 提交运行状态。
- 成员文件数：1；声明类型数：1；字段：6；属性：0；合计：6。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（6）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1920 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 94 | 2 | SentryLifeTime | int | `public const int SentryLifeTime = 36000;` | `public const int SentryLifeTime = 36000;` |
| 1921 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 96 | 2 | ArrowLifeTime | int | `public const int ArrowLifeTime = 1200;` | `public const int ArrowLifeTime = 1200;` |
| 1939 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 132 | 2 | gfxOffY | float | `public float gfxOffY;` | `public float gfxOffY;` |
| 1940 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 134 | 2 | stepSpeed | float | `public float stepSpeed = 1f;` | `public float stepSpeed = 1f;` |
| 1942 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 138 | 2 | timeLeft | int | `public int timeLeft;` | `public int timeLeft;` |
| 1943 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 140 | 2 | soundDelay | int | `public int soundDelay;` | `public int soundDelay;` |

##### 属性（0）

无该类型成员记录。


#### 4.15.4 细分子系统：`ProjectileCombatAndImmunity`

- 细分职责：伤害、友敌、穿透和 NPC/玩家免疫。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System 读取输入并通过显式 Command/CommitPort 写入；跨组只用事件、Query 或命令交接。
- 成员文件数：1；声明类型数：1；字段：14；属性：0；合计：14。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（14）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1944 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 142 | 2 | damage | int | `public int damage;` | `public int damage;` |
| 1945 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 144 | 2 | originalDamage | int | `public int originalDamage;` | `public int originalDamage;` |
| 1946 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 146 | 2 | spriteDirection | int | `public int spriteDirection = 1;` | `public int spriteDirection = 1;` |
| 1947 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 148 | 2 | hostile | bool | `public bool hostile;` | `public bool hostile;` |
| 1948 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 150 | 2 | reflected | bool | `public bool reflected;` | `public bool reflected;` |
| 1949 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 152 | 2 | knockBack | float | `public float knockBack;` | `public float knockBack;` |
| 1950 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 154 | 2 | friendly | bool | `public bool friendly;` | `public bool friendly;` |
| 1951 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 156 | 2 | penetrate | int | `public int penetrate = 1;` | `public int penetrate = 1;` |
| 1952 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 158 | 2 | localNPCImmunity | int[] | `public int[] localNPCImmunity = new int[Main.maxNPCs];` | `public int[] localNPCImmunity = new int[Main.maxNPCs];` |
| 1953 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 160 | 2 | usesLocalNPCImmunity | bool | `public bool usesLocalNPCImmunity;` | `public bool usesLocalNPCImmunity;` |
| 1954 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 162 | 2 | usesIDStaticNPCImmunity | bool | `public bool usesIDStaticNPCImmunity;` | `public bool usesIDStaticNPCImmunity;` |
| 1955 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 164 | 2 | appliesImmunityTimeOnSingleHits | bool | `public bool appliesImmunityTimeOnSingleHits;` | `public bool appliesImmunityTimeOnSingleHits;` |
| 1956 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 166 | 2 | maxPenetrate | int | `public int maxPenetrate = 1;` | `public int maxPenetrate = 1;` |
| 1957 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 168 | 2 | identity | int | `public int identity;` | `public int identity;` |

##### 属性（0）

无该类型成员记录。


#### 4.15.5 细分子系统：`ProjectileMovementAndCollisionState`

- 细分职责：投射物运动历史、碰撞、更新步数和水体交互状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；运动系统集中提交碰撞与历史状态。
- 成员文件数：1；声明类型数：1；字段：9；属性：0；合计：9。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（9）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1963 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 180 | 2 | oldPos | Vector2[] | `public Vector2[] oldPos = new Vector2[10];` | `public Vector2[] oldPos = new Vector2[10];` |
| 1964 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 182 | 2 | oldRot | float[] | `public float[] oldRot = new float[10];` | `public float[] oldRot = new float[10];` |
| 1965 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 184 | 2 | oldSpriteDirection | int[] | `public int[] oldSpriteDirection = new int[10];` | `public int[] oldSpriteDirection = new int[10];` |
| 1969 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 192 | 2 | restrikeDelay | int | `public int restrikeDelay;` | `public int restrikeDelay;` |
| 1970 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 194 | 2 | tileCollide | bool | `public bool tileCollide;` | `public bool tileCollide;` |
| 1971 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 196 | 2 | extraUpdates | int | `public int extraUpdates;` | `public int extraUpdates;` |
| 1972 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 198 | 2 | stopsDealingDamageAfterPenetrateHits | bool | `public bool stopsDealingDamageAfterPenetrateHits;` | `public bool stopsDealingDamageAfterPenetrateHits;` |
| 1973 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 200 | 2 | numUpdates | int | `public int numUpdates;` | `public int numUpdates;` |
| 1974 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 202 | 2 | ignoreWater | bool | `public bool ignoreWater;` | `public bool ignoreWater;` |

##### 属性（0）

无该类型成员记录。


#### 4.15.6 细分子系统：`ProjectileNetworkReplicationState`

- 细分职责：投射物网络更新、网络节流和按玩家同步跳过状态。
- 边界角色：`registry/projection`；最小 seam：Registry/Projection seam；复制层只读取投射物权威快照。
- 成员文件数：1；声明类型数：1；字段：4；属性：0；合计：4。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（4）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1959 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 172 | 2 | netUpdate | bool | `public bool netUpdate;` | `public bool netUpdate;` |
| 1960 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 174 | 2 | netUpdate2 | bool | `public bool netUpdate2;` | `public bool netUpdate2;` |
| 1961 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 176 | 2 | netSpam | int | `public int netSpam;` | `public int netSpam;` |
| 1962 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 178 | 2 | netSyncSkippedForPlayer | bool[] | `internal bool[] netSyncSkippedForPlayer = new bool[255];` | `internal bool[] netSyncSkippedForPlayer = new bool[255];` |

##### 属性（0）

无该类型成员记录。


#### 4.15.7 细分子系统：`ProjectileMinionAndPresentationState`

- 细分职责：召唤物槽位、预览实体、绘制层和玩家免疫缓存。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；召唤物与表现适配器通过显式快照交接。
- 成员文件数：1；声明类型数：1；字段：13；属性：0；合计：13。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（13）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1958 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 170 | 2 | light | float | `public float light;` | `public float light;` |
| 1966 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 186 | 2 | minion | bool | `public bool minion;` | `public bool minion;` |
| 1967 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 188 | 2 | minionSlots | float | `public float minionSlots;` | `public float minionSlots;` |
| 1968 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 190 | 2 | minionPos | int | `public int minionPos;` | `public int minionPos;` |
| 1975 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 204 | 2 | isAPreviewDummy | bool | `public bool isAPreviewDummy;` | `public bool isAPreviewDummy;` |
| 1976 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 206 | 2 | isAPreviewDisplayDoll | bool | `public bool isAPreviewDisplayDoll;` | `public bool isAPreviewDisplayDoll;` |
| 1977 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 208 | 2 | MinionSpawnInfo | Terraria.DataStructures.MinionSpawnInfo | `public MinionSpawnInfo MinionSpawnInfo;` | `public MinionSpawnInfo MinionSpawnInfo;` |
| 1978 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 210 | 2 | drawLayer | int | `public int drawLayer;` | `public int drawLayer;` |
| 1979 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 212 | 2 | usesOwnerLight | bool | `public bool usesOwnerLight;` | `public bool usesOwnerLight;` |
| 1980 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 214 | 2 | hide | bool | `public bool hide;` | `public bool hide;` |
| 1981 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 216 | 2 | ownerHitCheck | bool | `public bool ownerHitCheck;` | `public bool ownerHitCheck;` |
| 1982 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 218 | 2 | usesOwnerMeleeHitCD | bool | `public bool usesOwnerMeleeHitCD;` | `public bool usesOwnerMeleeHitCD;` |
| 1983 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 220 | 2 | playerImmune | int[] | `public int[] playerImmune = new int[255];` | `public int[] playerImmune = new int[255];` |

##### 属性（0）

无该类型成员记录。


#### 4.15.8 细分子系统：`ProjectileDamageAndElementState`

- 细分职责：伤害类型、附魔限制、陷阱来源和 Tag 效果状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；战斗事件集中提交伤害标签与元素状态。
- 成员文件数：1；声明类型数：1；字段：13；属性：0；合计：13。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（13）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1984 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 222 | 2 | miscText | string | `public string miscText = "";` | `public string miscText = "";` |
| 1985 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 224 | 2 | melee | bool | `public bool melee;` | `public bool melee;` |
| 1986 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 226 | 2 | ranged | bool | `public bool ranged;` | `public bool ranged;` |
| 1987 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 228 | 2 | magic | bool | `public bool magic;` | `public bool magic;` |
| 1988 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 230 | 2 | coldDamage | bool | `public bool coldDamage;` | `public bool coldDamage;` |
| 1989 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 232 | 2 | noEnchantments | bool | `public bool noEnchantments;` | `public bool noEnchantments;` |
| 1990 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 234 | 2 | noEnchantmentVisuals | bool | `public bool noEnchantmentVisuals;` | `public bool noEnchantmentVisuals;` |
| 1991 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 236 | 2 | trap | bool | `public bool trap;` | `public bool trap;` |
| 1992 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 238 | 2 | npcProj | bool | `public bool npcProj;` | `public bool npcProj;` |
| 1993 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 240 | 2 | originatedFromActivableTile | bool | `public bool originatedFromActivableTile;` | `public bool originatedFromActivableTile;` |
| 2004 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 262 | 2 | tagEffectType | int | `public int tagEffectType;` | `public int tagEffectType;` |
| 2005 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 264 | 2 | bonusTagDamage | int | `public int bonusTagDamage;` | `public int bonusTagDamage;` |
| 2006 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 266 | 2 | armorPenetration | int | `public int armorPenetration;` | `public int armorPenetration;` |

##### 属性（0）

无该类型成员记录。


#### 4.15.9 细分子系统：`ProjectileAnimationAndDirectionState`

- 细分职责：投射物帧计数、帧索引和手动方向切换状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；表现行为消费并提交帧状态。
- 成员文件数：1；声明类型数：1；字段：3；属性：0；合计：3。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（3）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1994 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 242 | 2 | frameCounter | int | `public int frameCounter;` | `public int frameCounter;` |
| 1995 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 244 | 2 | frame | int | `public int frame;` | `public int frame;` |
| 1996 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 246 | 2 | manualDirectionChange | bool | `public bool manualDirectionChange;` | `public bool manualDirectionChange;` |

##### 属性（0）

无该类型成员记录。


#### 4.15.10 细分子系统：`ProjectileCollisionAndTargetingState`

- 细分职责：斜坡碰撞、穿透方向、目标命中冷却和 Banner/UUID 目标状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；目标选择与碰撞命令显式提交。
- 成员文件数：1；声明类型数：1；字段：7；属性：0；合计：7。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（7）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1997 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 248 | 2 | projUUID | int | `public int projUUID = -1;` | `public int projUUID = -1;` |
| 1998 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 250 | 2 | correctSlopeCollision | bool | `public bool correctSlopeCollision;` | `public bool correctSlopeCollision;` |
| 1999 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 252 | 2 | decidesManualFallThrough | bool | `public bool decidesManualFallThrough;` | `public bool decidesManualFallThrough;` |
| 2000 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 254 | 2 | shouldFallThrough | bool | `public bool shouldFallThrough;` | `public bool shouldFallThrough;` |
| 2001 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 256 | 2 | localNPCHitCooldown | int | `public int localNPCHitCooldown = -2;` | `public int localNPCHitCooldown = -2;` |
| 2002 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 258 | 2 | idStaticNPCHitCooldown | int | `public int idStaticNPCHitCooldown = -1;` | `public int idStaticNPCHitCooldown = -1;` |
| 2003 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 260 | 2 | bannerIdToRespondTo | int | `public int bannerIdToRespondTo;` | `public int bannerIdToRespondTo;` |

##### 属性（0）

无该类型成员记录。


#### 4.15.11 细分子系统：`ProjectileCombatScalingState`

- 细分职责：投射物暴击和敌对伤害缩放修正。
- 边界角色：`authoritative state/behavior`；最小 seam：Projectile Combat System/CommitPort；难度和战斗事件集中提交。
- 成员文件数：1；声明类型数：1；字段：2；属性：0；合计：2。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（2）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2007 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 268 | 2 | bonusCritChance | int | `public int bonusCritChance;` | `public int bonusCritChance;` |
| 2008 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 270 | 2 | hostileDamageScaling | Terraria.DataStructures.GameDifficultyData.LinearCurve | `public GameDifficultyData.LinearCurve hostileDamageScaling = GameDifficultyData.HostileProjectileDamageMultiplier;` | `public GameDifficultyData.LinearCurve hostileDamageScaling = GameDifficultyData.HostileProjectileDamageMultiplier;` |

##### 属性（0）

无该类型成员记录。


#### 4.15.12 细分子系统：`ProjectileCollisionGeometryCache`

- 细分职责：投射物碰撞条件、长矛/鞭子/闪电几何缓存。
- 边界角色：`derived/query`；最小 seam：Projectile Collision Query/Cache；缓存失效由碰撞系统管理。
- 成员文件数：1；声明类型数：1；字段：8；属性：0；合计：8。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（8）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2009 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 272 | 2 | _cachedConditions_solid | Terraria.WorldBuilding.Conditions.IsSolid | `private static Conditions.IsSolid _cachedConditions_solid = new Conditions.IsSolid();` | `private static Conditions.IsSolid _cachedConditions_solid = new Conditions.IsSolid();` |
| 2010 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 274 | 2 | _cachedConditions_notNull | Terraria.WorldBuilding.Conditions.NotNull | `private static Conditions.NotNull _cachedConditions_notNull = new Conditions.NotNull();` | `private static Conditions.NotNull _cachedConditions_notNull = new Conditions.NotNull();` |
| 2011 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 276 | 2 | _javelinsMax6 | Point[] | `private static Point[] _javelinsMax6 = new Point[6];` | `private static Point[] _javelinsMax6 = new Point[6];` |
| 2012 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 278 | 2 | _javelinsMax8 | Point[] | `private static Point[] _javelinsMax8 = new Point[8];` | `private static Point[] _javelinsMax8 = new Point[8];` |
| 2013 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 280 | 2 | _javelinsMax10 | Point[] | `private static Point[] _javelinsMax10 = new Point[10];` | `private static Point[] _javelinsMax10 = new Point[10];` |
| 2014 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 282 | 2 | WhipPointsForCollision | System.Collections.Generic.List<Vector2> | `public List<Vector2> WhipPointsForCollision = new List<Vector2>();` | `public List<Vector2> WhipPointsForCollision = new List<Vector2>();` |
| 2015 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 284 | 2 | _lanceHitboxBounds | Rectangle | `private static Rectangle _lanceHitboxBounds = new Rectangle(0, 0, 300, 300);` | `private static Rectangle _lanceHitboxBounds = new Rectangle(0, 0, 300, 300);` |
| 2016 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 286 | 2 | _lightningCollisionBounds | Terraria.DataStructures.MultiPointHitbox | `private static MultiPointHitbox _lightningCollisionBounds;` | `private static MultiPointHitbox _lightningCollisionBounds;` |

##### 属性（0）

无该类型成员记录。


#### 4.15.13 细分子系统：`ProjectileTargetSelectionCache`

- 细分职责：彩虹巨石、Medusa 和 AI 黑名单目标缓存。
- 边界角色：`derived/query`；最小 seam：Projectile Target Query/Cache；目标缓存只读服务选择查询。
- 成员文件数：1；声明类型数：1；字段：7；属性：0；合计：7。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（7）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2017 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 288 | 2 | _rainbowBoulderTargetsAny | System.Collections.Generic.List<Terraria.NPC> | `private static List<NPC> _rainbowBoulderTargetsAny = new List<NPC>();` | `private static List<NPC> _rainbowBoulderTargetsAny = new List<NPC>();` |
| 2018 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 290 | 2 | _rainbowBoulderTargetsFar | System.Collections.Generic.List<Terraria.NPC> | `private static List<NPC> _rainbowBoulderTargetsFar = new List<NPC>();` | `private static List<NPC> _rainbowBoulderTargetsFar = new List<NPC>();` |
| 2022 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 298 | 2 | _medusaHeadTargetList | System.Collections.Generic.List<System.Tuple<int, float>> | `private static List<Tuple<int, float>> _medusaHeadTargetList = new List<Tuple<int, float>>();` | `private static List<Tuple<int, float>> _medusaHeadTargetList = new List<Tuple<int, float>>();` |
| 2023 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 300 | 2 | _medusaTargetComparer | Terraria.Projectile.NPCDistanceByIndexComparator | `private static NPCDistanceByIndexComparator _medusaTargetComparer = new NPCDistanceByIndexComparator();` | `private static NPCDistanceByIndexComparator _medusaTargetComparer = new NPCDistanceByIndexComparator();` |
| 2024 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 302 | 2 | _ai164_blacklistedTargets | System.Collections.Generic.List<int> | `private static List<int> _ai164_blacklistedTargets = new List<int>();` | `private static List<int> _ai164_blacklistedTargets = new List<int>();` |
| 2026 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 306 | 2 | _ai158_blacklistedTargets | System.Collections.Generic.List<int> | `private static List<int> _ai158_blacklistedTargets = new List<int>();` | `private static List<int> _ai158_blacklistedTargets = new List<int>();` |
| 2028 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 310 | 2 | _ai156_blacklistedTargets | System.Collections.Generic.List<int> | `private static List<int> _ai156_blacklistedTargets = new List<int>();` | `private static List<int> _ai156_blacklistedTargets = new List<int>();` |

##### 属性（0）

无该类型成员记录。


#### 4.15.14 细分子系统：`ProjectileFishingAndMiningQueryState`

- 细分职责：钓鱼上下文、鱼类展示和采矿跳过点查询缓存。
- 边界角色：`derived/query`；最小 seam：Projectile Tool Query/Cache；工具查询结果按调用周期失效。
- 成员文件数：1；声明类型数：1；字段：3；属性：0；合计：3。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（3）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2019 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 292 | 2 | _availableFishTypesToShow | System.Collections.Generic.List<Terraria.GameContent.FishDropRules.FishPossibilityEntry> | `private static List<FishPossibilityEntry> _availableFishTypesToShow = new List<FishPossibilityEntry>();` | `private static List<FishPossibilityEntry> _availableFishTypesToShow = new List<FishPossibilityEntry>();` |
| 2020 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 294 | 2 | _context | Terraria.GameContent.FishDropRules.FishingContext | `private static FishingContext _context = new FishingContext();` | `private static FishingContext _context = new FishingContext();` |
| 2027 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 308 | 2 | _miningHelperPointsToSkip | System.Collections.Generic.List<Point> | `private static List<Point> _miningHelperPointsToSkip = new List<Point>();` | `private static List<Point> _miningHelperPointsToSkip = new List<Point>();` |

##### 属性（0）

无该类型成员记录。


#### 4.15.15 细分子系统：`ProjectileKiteAndLightningRules`

- 细分职责：风筝飞行阈值和闪电液体伤害半径规则。
- 边界角色：`definition/query`；最小 seam：Projectile Specialized Definition/Query；规则只读提供给专用行为。
- 成员文件数：1；声明类型数：1；字段：2；属性：0；合计：2。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（2）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2021 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 296 | 2 | StormLightningLiquidDamageRadius | int | `public const int StormLightningLiquidDamageRadius = 500;` | `public const int StormLightningLiquidDamageRadius = 500;` |
| 2025 | field | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 304 | 2 | MinimumWindStrengthToFlyKite | float | `public const float MinimumWindStrengthToFlyKite = 0.2f;` | `public const float MinimumWindStrengthToFlyKite = 0.2f;` |

##### 属性（0）

无该类型成员记录。


#### 4.15.16 细分子系统：`ProjectileDerivedProperties`

- 细分职责：名称、更新次数、归属和网络区段的只读属性。
- 边界角色：`derived/query`；最小 seam：只读快照/纯资格 Query；不得写入该分组或相邻权威状态。
- 成员文件数：1；声明类型数：1；字段：0；属性：8；合计：8。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（0）

无该类型成员记录。

##### 属性（8）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2029 | property | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 312 | 2 | Name | string | `public string Name => Lang.GetProjectileName(type).Value;` | `public string Name => Lang.GetProjectileName(type).Value;` |
| 2030 | property | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 314 | 2 | WipableTurret | bool | `public bool WipableTurret { get { if (owner == Main.myPlayer && sentry) { return !TurretShouldPersist(); } return false; } }` | `public bool WipableTurret { get { if (owner == Main.myPlayer && sentry) { return !TurretShouldPersist(); } return false; } }` |
| 2031 | property | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 326 | 2 | Opacity | float | `public float Opacity { get { return 1f - (float)alpha / 255f; } set { alpha = (int)MathHelper.Clamp((1f - value) * 255f, 0f, 255f); } }` | `public float Opacity { get { return 1f - (float)alpha / 255f; } set { alpha = (int)MathHelper.Clamp((1f - value) * 255f, 0f, 255f); } }` |
| 2032 | property | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 338 | 2 | MaxUpdates | int | `public int MaxUpdates { get { return extraUpdates + 1; } set { extraUpdates = value - 1; } }` | `public int MaxUpdates { get { return extraUpdates + 1; } set { extraUpdates = value - 1; } }` |
| 2033 | property | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 350 | 2 | OwnerMinionAttackTargetNPC | Terraria.NPC | `public NPC OwnerMinionAttackTargetNPC { get { if (Main.player[owner].MinionAttackTargetNPC < 0) { return null; } return Main.npc[Main.player[owner].MinionAttackTargetNPC]; } }` | `public NPC OwnerMinionAttackTargetNPC { get { if (Main.player[owner].MinionAttackTargetNPC < 0) { return null; } return Main.npc[Main.player[owner].MinionAttackTargetNPC]; } }` |
| 2034 | property | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 362 | 2 | OwnedBySomeone | bool | `public bool OwnedBySomeone { get { if (!npcProj) { return !trap; } return false; } }` | `public bool OwnedBySomeone { get { if (!npcProj) { return !trap; } return false; } }` |
| 2035 | property | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 374 | 2 | CareForAttackCD | bool | `public bool CareForAttackCD { get { if (usesOwnerMeleeHitCD && OwnedBySomeone) { return owner < 255; } return false; } }` | `public bool CareForAttackCD { get { if (usesOwnerMeleeHitCD && OwnedBySomeone) { return owner < 255; } return false; } }` |
| 2036 | property | Terraria.Projectile | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 386 | 2 | NetSectionCoordinates | Point | `public Point NetSectionCoordinates => new Point(Netplay.GetSectionX((int)position.X >> 4), Netplay.GetSectionY((int)position.Y >> 4));` | `public Point NetSectionCoordinates => new Point(Netplay.GetSectionX((int)position.X >> 4), Netplay.GetSectionY((int)position.Y >> 4));` |


#### 4.15.17 细分子系统：`ProjectileStormDefinition`

- 细分职责：Hallow Boss pellet storm 的静态弹幕定义。
- 边界角色：`definition/query`；最小 seam：只读 Definition/Catalog view；规则 System 消费，外部配置通过 Adapter 转换。
- 成员文件数：1；声明类型数：1；字段：7；属性：0；合计：7。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（7）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1911 | field | Terraria.Projectile.HallowBossPelletStormInfo | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 42 | 3 | StartAngle | float | `public float StartAngle;` | `public float StartAngle;` |
| 1912 | field | Terraria.Projectile.HallowBossPelletStormInfo | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 44 | 3 | AnglePerBullet | float | `public float AnglePerBullet;` | `public float AnglePerBullet;` |
| 1913 | field | Terraria.Projectile.HallowBossPelletStormInfo | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 46 | 3 | BulletsInStorm | int | `public int BulletsInStorm;` | `public int BulletsInStorm;` |
| 1914 | field | Terraria.Projectile.HallowBossPelletStormInfo | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 48 | 3 | BulletsProgressInStormStartNormalized | float | `public float BulletsProgressInStormStartNormalized;` | `public float BulletsProgressInStormStartNormalized;` |
| 1915 | field | Terraria.Projectile.HallowBossPelletStormInfo | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 50 | 3 | BulletsProgressInStormBonusByIndexNormalized | float | `public float BulletsProgressInStormBonusByIndexNormalized;` | `public float BulletsProgressInStormBonusByIndexNormalized;` |
| 1916 | field | Terraria.Projectile.HallowBossPelletStormInfo | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 52 | 3 | StormTotalRange | float | `public float StormTotalRange;` | `public float StormTotalRange;` |
| 1917 | field | Terraria.Projectile.HallowBossPelletStormInfo | Terraria/Projectile.cs | D:\TRbackup\Version4\Terraria\Projectile.cs | 54 | 3 | BulletSize | Vector2 | `public Vector2 BulletSize;` | `public Vector2 BulletSize;` |

##### 属性（0）

无该类型成员记录。


## 8. 本分区自检

- 叶子子系统：17 个；指定组均已写入且没有重复分区。
- 字段/属性/成员：118 / 8 / 126。
- 来源序号范围：1911..2036；本分区内唯一序号：True。
- 成员声明表逐组从源报告提取，未改写来源序号、类型、路径、行列、成员、C# 类型、成员声明或原始声明。
- 运行时读写者、生命周期、持久化、网络和调度：未验证，保留为迁移前风险。
