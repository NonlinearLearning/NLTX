# P12 NPC 身份、AI、战斗、状态、网络、伤害与经济 - 权威模拟系统成员组件候选分区

> 来源：`docs/migration/ledgers/Version4权威模拟系统字段属性逐成员源码声明-更细子系统拆分-去除ID类文件.md`；本文件只做成员库存分区和候选组件边界记录。
> 这是候选组件拆分台账，不是运行时迁移完成、行为等价、API 兼容、网络闭合或持久化闭合证明。

## 1. 分区边界

本分区包含 19 个叶子子系统，字段 173 条、属性 16 条、成员合计 189 条；每个叶子子系统保持原正式父级归属，不跨分区拆行。

## 2. 分区覆盖概览

| 叶子子系统 | 正式父级 | 字段 | 属性 | 合计 | 源边界角色 |
|---|---|---:|---:|---:|---|
| `NpcIdentityInteractionAndPresentationState` | `NpcAndTownSimulation` | 18 | 0 | 18 | authoritative state/behavior |
| `NpcTargetAndMovementHistoryState` | `NpcAndTownSimulation` | 14 | 0 | 14 | authoritative state/behavior |
| `NpcIdentityAndStatusState` | `NpcAndTownSimulation` | 4 | 0 | 4 | authoritative state/behavior |
| `NpcAiTargetAndIdentityState` | `NpcAndTownSimulation` | 13 | 0 | 13 | authoritative state/behavior |
| `NpcCombatAndLifeState` | `NpcAndTownSimulation` | 19 | 0 | 19 | authoritative state/behavior |
| `NpcCollisionAndPresentationState` | `NpcAndTownSimulation` | 17 | 0 | 17 | authoritative state/behavior |
| `NpcPortalAndSpecialBehaviorState` | `NpcAndTownSimulation` | 11 | 0 | 11 | authoritative state/behavior |
| `NpcBuffSlotAndImmunityState` | `NpcAndTownSimulation` | 4 | 0 | 4 | authoritative state/behavior |
| `NpcElementalDebuffState` | `NpcAndTownSimulation` | 17 | 0 | 17 | authoritative state/behavior |
| `NpcControlAndSocialEffectState` | `NpcAndTownSimulation` | 4 | 0 | 4 | authoritative state/behavior |
| `NpcWhipAndSpecialEffectState` | `NpcAndTownSimulation` | 9 | 0 | 9 | authoritative state/behavior |
| `NpcRegenerationAndProtectionState` | `NpcAndTownSimulation` | 8 | 0 | 8 | authoritative state/behavior |
| `NpcLifecycleAndCrossDomainRefs` | `NpcAndTownSimulation` | 4 | 0 | 4 | authoritative state/behavior |
| `NpcNetworkReplicationState` | `NpcAndTownSimulation` | 11 | 0 | 11 | registry/projection |
| `NpcNetworkSyncState` | `NpcAndTownSimulation` | 2 | 0 | 2 | registry/projection |
| `NpcDamageDefinitionRegistry` | `NpcAndTownSimulation` | 4 | 0 | 4 | registry/projection |
| `NpcDamageRuntimeTracking` | `NpcAndTownSimulation` | 9 | 3 | 12 | authoritative state/behavior |
| `NpcDamageCreditProjection` | `NpcAndTownSimulation` | 1 | 6 | 7 | derived/query |
| `NpcInteractionAndCommerce` | `NpcAndTownSimulation` | 4 | 7 | 11 | authoritative state/behavior |

## 3. 候选组件归属与 seam

| 叶子子系统 | 候选组件边界 | 类型分类 | 最小 seam | 独立生命周期 | 证据状态 | 主要风险 |
|---|---|---|---|---|---|---|
| `NpcIdentityInteractionAndPresentationState` | `NpcIdentityInteractionAndPresentationStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `NpcTargetAndMovementHistoryState` | `NpcTargetAndMovementHistoryStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `NpcIdentityAndStatusState` | `NpcIdentityAndStatusStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `NpcAiTargetAndIdentityState` | `NpcAiTargetAndIdentityStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `NpcCombatAndLifeState` | `NpcCombatAndLifeStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `NpcCollisionAndPresentationState` | `NpcCollisionAndPresentationStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `NpcPortalAndSpecialBehaviorState` | `NpcPortalAndSpecialBehaviorStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `NpcBuffSlotAndImmunityState` | `NpcBuffSlotAndImmunityStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `NpcElementalDebuffState` | `NpcElementalDebuffStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `NpcControlAndSocialEffectState` | `NpcControlAndSocialEffectStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `NpcWhipAndSpecialEffectState` | `NpcWhipAndSpecialEffectStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `NpcRegenerationAndProtectionState` | `NpcRegenerationAndProtectionStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `NpcLifecycleAndCrossDomainRefs` | `NpcLifecycleAndCrossDomainRefsComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `NpcNetworkReplicationState` | `NpcNetworkReplicationStateProjection`/`Adapter` | Projection/Adapter | 单向 Projection/Adapter seam；不得回写权威状态 | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `NpcNetworkSyncState` | `NpcNetworkSyncStateProjection`/`Adapter` | Projection/Adapter | 单向 Projection/Adapter seam；不得回写权威状态 | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `NpcDamageDefinitionRegistry` | `NpcDamageDefinitionRegistryProjection`/`Adapter` | Projection/Adapter | 单向 Projection/Adapter seam；不得回写权威状态 | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `NpcDamageRuntimeTracking` | `NpcDamageRuntimeTrackingComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `NpcDamageCreditProjection` | `NpcDamageCreditProjectionQuery` | Query | 纯资格或派生 Query；只读 | 候选纯读边界；仍需核对失效/缓存条件 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `NpcInteractionAndCommerce` | `NpcInteractionAndCommerceComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |

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

### 正式父级子系统：`NpcAndTownSimulation`
- 父级职责：沿用源报告正式父级 `NpcAndTownSimulation`；本分区只收录该父级中列出的叶子子系统。
- 本分区父级局部统计：字段 173；属性 16；合计 189；完整父级统计以源报告为准。

#### 4.14.1 细分子系统：`NpcIdentityInteractionAndPresentationState`

- 细分职责：NPC 身份、交互、名字表现和玩家交互历史状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；交互命令只写入 NPC 身份与交互边界。
- 成员文件数：1；声明类型数：1；字段：18；属性：0；合计：18。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（18）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1578 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 5897 | 2 | active | bool | `public bool active;` | `public bool active;` |
| 1579 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 5899 | 2 | NPC_TARGETS_START | int | `private const int NPC_TARGETS_START = 300;` | `private const int NPC_TARGETS_START = 300;` |
| 1580 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 5901 | 2 | IsABestiaryIconDummy | bool | `public bool IsABestiaryIconDummy;` | `public bool IsABestiaryIconDummy;` |
| 1581 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 5903 | 2 | IsAPortraitDummy | bool | `public bool IsAPortraitDummy;` | `public bool IsAPortraitDummy;` |
| 1582 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 5905 | 2 | ForcePartyHatOn | bool | `public bool ForcePartyHatOn;` | `public bool ForcePartyHatOn;` |
| 1601 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 5943 | 2 | nameOverIncrement | float | `public const float nameOverIncrement = 0.025f;` | `public const float nameOverIncrement = 0.025f;` |
| 1602 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 5945 | 2 | nameOverDistance | float | `public const float nameOverDistance = 350f;` | `public const float nameOverDistance = 350f;` |
| 1603 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 5947 | 2 | nameOver | float | `public float nameOver;` | `public float nameOver;` |
| 1610 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 5961 | 2 | altTexture | int | `public int altTexture;` | `public int altTexture;` |
| 1611 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 5963 | 2 | townNpcVariationIndex | int | `public int townNpcVariationIndex;` | `public int townNpcVariationIndex;` |
| 1612 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 5965 | 2 | catchItem | short | `public short catchItem;` | `public short catchItem;` |
| 1613 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 5967 | 2 | releaseOwner | short | `public short releaseOwner = 255;` | `public short releaseOwner = 255;` |
| 1614 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 5969 | 2 | rarity | int | `public int rarity;` | `public int rarity;` |
| 1615 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 5971 | 2 | taxCollector | bool | `public static bool taxCollector = false;` | `public static bool taxCollector = false;` |
| 1616 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 5973 | 2 | playerInteraction | bool[] | `public bool[] playerInteraction = new bool[256];` | `public bool[] playerInteraction = new bool[256];` |
| 1617 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 5975 | 2 | lastInteraction | int | `public int lastInteraction = 255;` | `public int lastInteraction = 255;` |
| 1618 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 5977 | 2 | takenDamageMultiplier | float | `public float takenDamageMultiplier = 1f;` | `public float takenDamageMultiplier = 1f;` |
| 1619 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 5979 | 2 | freeCake | bool | `public static bool freeCake = false;` | `public static bool freeCake = false;` |

##### 属性（0）

无该类型成员记录。


#### 4.14.2 细分子系统：`NpcTargetAndMovementHistoryState`

- 细分职责：NPC 目标移动参数、传送、重力和移动历史缓存。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；移动系统通过显式阶段提交历史状态。
- 成员文件数：1；声明类型数：1；字段：14；属性：0；合计：14。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（14）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1583 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 5907 | 2 | waterMovementSpeed | float | `public float waterMovementSpeed = 0.5f;` | `public float waterMovementSpeed = 0.5f;` |
| 1584 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 5909 | 2 | lavaMovementSpeed | float | `public float lavaMovementSpeed = 0.5f;` | `public float lavaMovementSpeed = 0.5f;` |
| 1585 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 5911 | 2 | honeyMovementSpeed | float | `public float honeyMovementSpeed = 0.25f;` | `public float honeyMovementSpeed = 0.25f;` |
| 1586 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 5913 | 2 | shimmerMovementSpeed | float | `public float shimmerMovementSpeed = 0.375f;` | `public float shimmerMovementSpeed = 0.375f;` |
| 1594 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 5929 | 2 | teleportStyle | int | `public int teleportStyle;` | `public int teleportStyle;` |
| 1595 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 5931 | 2 | teleportTime | float | `public float teleportTime;` | `public float teleportTime;` |
| 1620 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 5981 | 2 | gfxOffY | float | `public float gfxOffY;` | `public float gfxOffY;` |
| 1621 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 5983 | 2 | stepSpeed | float | `public float stepSpeed;` | `public float stepSpeed;` |
| 1622 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 5985 | 2 | gravity | float | `private static float gravity = 0.3f;` | `private static float gravity = 0.3f;` |
| 1623 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 5987 | 2 | teleporting | bool | `public bool teleporting;` | `public bool teleporting;` |
| 1624 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 5989 | 2 | stairFall | bool | `public bool stairFall;` | `public bool stairFall;` |
| 1630 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6001 | 2 | oldPos | Vector2[] | `public Vector2[] oldPos = new Vector2[10];` | `public Vector2[] oldPos = new Vector2[10];` |
| 1631 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6003 | 2 | oldRot | float[] | `public float[] oldRot = new float[10];` | `public float[] oldRot = new float[10];` |
| 1632 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6005 | 2 | setFrameSize | bool | `public bool setFrameSize;` | `public bool setFrameSize;` |

##### 属性（0）

无该类型成员记录。


#### 4.14.8 细分子系统：`NpcIdentityAndStatusState`

- 细分职责：NPC 关联实体、名称、微光透明度和 Buff 容量状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；实体生命周期系统维护身份和容量。
- 成员文件数：1；声明类型数：1；字段：4；属性：0；合计：4。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（4）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1649 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6039 | 2 | realLife | int | `public int realLife = -1;` | `public int realLife = -1;` |
| 1650 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6041 | 2 | _givenName | string | `private string _givenName = "";` | `private string _givenName = "";` |
| 1660 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6061 | 2 | shimmerTransparency | float | `public float shimmerTransparency;` | `public float shimmerTransparency;` |
| 1662 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6065 | 2 | maxBuffs | int | `public static readonly int maxBuffs = 20;` | `public static readonly int maxBuffs = 20;` |

##### 属性（0）

无该类型成员记录。


#### 4.14.22 细分子系统：`NpcAiTargetAndIdentityState`

- 细分职责：NPC 类型、网络身份、AI、目标和生命期行为状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；AI 只通过查询读取战斗和世界输入。
- 成员文件数：1；声明类型数：1；字段：13；属性：0；合计：13。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（13）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1781 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6303 | 2 | immune | int[] | `public int[] immune = new int[256];` | `public int[] immune = new int[256];` |
| 1782 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6305 | 2 | directionY | int | `public int directionY = 1;` | `public int directionY = 1;` |
| 1783 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6307 | 2 | type | int | `public int type;` | `public int type;` |
| 1784 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6309 | 2 | ai | float[] | `public float[] ai = new float[maxAI];` | `public float[] ai = new float[maxAI];` |
| 1785 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6311 | 2 | localAI | float[] | `public float[] localAI = new float[maxAI];` | `public float[] localAI = new float[maxAI];` |
| 1786 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6313 | 2 | aiAction | int | `public int aiAction;` | `public int aiAction;` |
| 1787 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6315 | 2 | aiStyle | int | `public int aiStyle;` | `public int aiStyle;` |
| 1788 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6317 | 2 | justHit | bool | `public bool justHit;` | `public bool justHit;` |
| 1789 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6319 | 2 | timeLeft | int | `public int timeLeft;` | `public int timeLeft;` |
| 1790 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6321 | 2 | target | int | `public int target = -1;` | `public int target = -1;` |
| 1810 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6361 | 2 | oldDirectionY | int | `public int oldDirectionY;` | `public int oldDirectionY;` |
| 1811 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6363 | 2 | oldTarget | int | `public int oldTarget;` | `public int oldTarget;` |
| 1825 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6391 | 2 | netID | int | `public int netID;` | `public int netID;` |

##### 属性（0）

无该类型成员记录。


#### 4.14.23 细分子系统：`NpcCombatAndLifeState`

- 细分职责：NPC 伤害、防御、生命、友敌、抗性和受击状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；战斗提交集中于本组。
- 成员文件数：1；声明类型数：1；字段：19；属性：0；合计：19。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（19）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1791 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6323 | 2 | damage | int | `public int damage;` | `public int damage;` |
| 1792 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6325 | 2 | defense | int | `public int defense;` | `public int defense;` |
| 1793 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6327 | 2 | defDamage | int | `public int defDamage;` | `public int defDamage;` |
| 1794 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6329 | 2 | defDefense | int | `public int defDefense;` | `public int defDefense;` |
| 1795 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6331 | 2 | defLifeMax | int | `public int defLifeMax;` | `public int defLifeMax;` |
| 1796 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6333 | 2 | coldDamage | bool | `public bool coldDamage;` | `public bool coldDamage;` |
| 1797 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6335 | 2 | trapImmune | bool | `public bool trapImmune;` | `public bool trapImmune;` |
| 1817 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6375 | 2 | boss | bool | `public bool boss;` | `public bool boss;` |
| 1820 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6381 | 2 | lavaImmune | bool | `public bool lavaImmune;` | `public bool lavaImmune;` |
| 1821 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6383 | 2 | value | float | `public float value;` | `public float value;` |
| 1822 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6385 | 2 | extraValue | int | `public int extraValue;` | `public int extraValue;` |
| 1823 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6387 | 2 | dontTakeDamage | bool | `public bool dontTakeDamage;` | `public bool dontTakeDamage;` |
| 1824 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6389 | 2 | catchableNPCTempImmunityCounter | int | `private int catchableNPCTempImmunityCounter;` | `private int catchableNPCTempImmunityCounter;` |
| 1826 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6393 | 2 | statsAreScaledForThisManyPlayers | int | `public int statsAreScaledForThisManyPlayers;` | `public int statsAreScaledForThisManyPlayers;` |
| 1827 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6395 | 2 | difficulty | float | `public float difficulty = 1f;` | `public float difficulty = 1f;` |
| 1841 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6423 | 2 | friendly | bool | `public bool friendly;` | `public bool friendly;` |
| 1845 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6431 | 2 | friendlyRegen | int | `public int friendlyRegen;` | `public int friendlyRegen;` |
| 1849 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6439 | 2 | reflectsProjectiles | bool | `public bool reflectsProjectiles;` | `public bool reflectsProjectiles;` |
| 1853 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6447 | 2 | CommonMasterBossLifeReduction | double | `public static readonly double CommonMasterBossLifeReduction = 0.85;` | `public static readonly double CommonMasterBossLifeReduction = 0.85;` |

##### 属性（0）

无该类型成员记录。


#### 4.14.24 细分子系统：`NpcCollisionAndPresentationState`

- 细分职责：NPC 碰撞、帧、朝向、缩放和显示状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；碰撞和表现读取共享只读快照。
- 成员文件数：1；声明类型数：1；字段：17；属性：0；合计：17。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（17）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1800 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6341 | 2 | life | int | `public int life;` | `public int life;` |
| 1801 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6343 | 2 | lifeMax | int | `public int lifeMax;` | `public int lifeMax;` |
| 1802 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6345 | 2 | targetRect | Rectangle | `public Rectangle targetRect;` | `public Rectangle targetRect;` |
| 1803 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6347 | 2 | frameCounter | double | `public double frameCounter;` | `public double frameCounter;` |
| 1804 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6349 | 2 | frame | Rectangle | `public Rectangle frame;` | `public Rectangle frame;` |
| 1805 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6351 | 2 | color | Color | `public Color color;` | `public Color color;` |
| 1806 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6353 | 2 | alpha | int | `public int alpha;` | `public int alpha;` |
| 1807 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6355 | 2 | hide | bool | `public bool hide;` | `public bool hide;` |
| 1808 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6357 | 2 | scale | float | `public float scale = 1f;` | `public float scale = 1f;` |
| 1809 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6359 | 2 | knockBackResist | float | `public float knockBackResist = 1f;` | `public float knockBackResist = 1f;` |
| 1812 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6365 | 2 | rotation | float | `public float rotation;` | `public float rotation;` |
| 1813 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6367 | 2 | noGravity | bool | `public bool noGravity;` | `public bool noGravity;` |
| 1814 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6369 | 2 | noTileCollide | bool | `public bool noTileCollide;` | `public bool noTileCollide;` |
| 1815 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6371 | 2 | collideX | bool | `public bool collideX;` | `public bool collideX;` |
| 1816 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6373 | 2 | collideY | bool | `public bool collideY;` | `public bool collideY;` |
| 1818 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6377 | 2 | spriteDirection | int | `public int spriteDirection = -1;` | `public int spriteDirection = -1;` |
| 1819 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6379 | 2 | behindTiles | bool | `public bool behindTiles;` | `public bool behindTiles;` |

##### 属性（0）

无该类型成员记录。


#### 4.14.26 细分子系统：`NpcPortalAndSpecialBehaviorState`

- 细分职责：NPC 传送、事件特化、洞穴类型和 Boss 特殊行为状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；特殊行为由事件命令驱动。
- 成员文件数：1；声明类型数：1；字段：11；属性：0；合计：11。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（11）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1798 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6337 | 2 | HitSound | Terraria.Audio.LegacySoundStyle | `public LegacySoundStyle HitSound;` | `public LegacySoundStyle HitSound;` |
| 1799 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6339 | 2 | DeathSound | Terraria.Audio.LegacySoundStyle | `public LegacySoundStyle DeathSound;` | `public LegacySoundStyle DeathSound;` |
| 1850 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6441 | 2 | lastPortalColorIndex | int | `public int lastPortalColorIndex;` | `public int lastPortalColorIndex;` |
| 1851 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6443 | 2 | despawnEncouraged | bool | `public bool despawnEncouraged;` | `public bool despawnEncouraged;` |
| 1852 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6445 | 2 | cavernMonsterType | int[,] | `public static int[,] cavernMonsterType = new int[2, 3];` | `public static int[,] cavernMonsterType = new int[2, 3];` |
| 1854 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6449 | 2 | mechQueen | int | `public static int mechQueen = -1;` | `public static int mechQueen = -1;` |
| 1855 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6451 | 2 | brainOfGravity | int | `public static int brainOfGravity = -1;` | `public static int brainOfGravity = -1;` |
| 1856 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6453 | 2 | kingSlimePointCacheSize | int | `private static int kingSlimePointCacheSize = 0;` | `private static int kingSlimePointCacheSize = 0;` |
| 1857 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6455 | 2 | kingSlimePointCacheSizeMax | int | `private static int kingSlimePointCacheSizeMax = 50;` | `private static int kingSlimePointCacheSizeMax = 50;` |
| 1858 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6457 | 2 | kingSlimePointCache | Point[] | `private static Point[] kingSlimePointCache = new Point[kingSlimePointCacheSizeMax];` | `private static Point[] kingSlimePointCache = new Point[kingSlimePointCacheSizeMax];` |
| 1859 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6459 | 2 | empressRageMode | bool | `public static bool empressRageMode = false;` | `public static bool empressRageMode = false;` |

##### 属性（0）

无该类型成员记录。


#### 4.14.9 细分子系统：`NpcBuffSlotAndImmunityState`

- 细分职责：NPC Buff 槽、Buff 时间、免疫和 Buff 展示开关。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；Buff 施加与清理集中于本组。
- 成员文件数：1；声明类型数：1；字段：4；属性：0；合计：4。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（4）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1663 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6067 | 2 | buffType | int[] | `public int[] buffType = new int[maxBuffs];` | `public int[] buffType = new int[maxBuffs];` |
| 1664 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6069 | 2 | buffTime | int[] | `public int[] buffTime = new int[maxBuffs];` | `public int[] buffTime = new int[maxBuffs];` |
| 1665 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6071 | 2 | buffImmune | bool[] | `public bool[] buffImmune = new bool[BuffID.Count];` | `public bool[] buffImmune = new bool[BuffID.Count];` |
| 1666 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6073 | 2 | canDisplayBuffs | bool | `public bool canDisplayBuffs = true;` | `public bool canDisplayBuffs = true;` |

##### 属性（0）

无该类型成员记录。


#### 4.14.10 细分子系统：`NpcElementalDebuffState`

- 细分职责：元素、伤害、火焰、毒性和微光效果标志。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；状态 Tick 和命中事件通过显式提交更新。
- 成员文件数：1；声明类型数：1；字段：17；属性：0；合计：17。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（17）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1667 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6075 | 2 | midas | bool | `public bool midas;` | `public bool midas;` |
| 1668 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6077 | 2 | ichor | bool | `public bool ichor;` | `public bool ichor;` |
| 1669 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6079 | 2 | brokenArmor | bool | `public bool brokenArmor;` | `public bool brokenArmor;` |
| 1670 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6081 | 2 | onFire | bool | `public bool onFire;` | `public bool onFire;` |
| 1671 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6083 | 2 | onFire2 | bool | `public bool onFire2;` | `public bool onFire2;` |
| 1672 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6085 | 2 | onFire3 | bool | `public bool onFire3;` | `public bool onFire3;` |
| 1673 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6087 | 2 | onFrostBurn | bool | `public bool onFrostBurn;` | `public bool onFrostBurn;` |
| 1674 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6089 | 2 | onFrostBurn2 | bool | `public bool onFrostBurn2;` | `public bool onFrostBurn2;` |
| 1675 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6091 | 2 | poisoned | bool | `public bool poisoned;` | `public bool poisoned;` |
| 1676 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6093 | 2 | venom | bool | `public bool venom;` | `public bool venom;` |
| 1677 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6095 | 2 | tipsy | bool | `public bool tipsy;` | `public bool tipsy;` |
| 1678 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6097 | 2 | bleeding | bool | `public bool bleeding;` | `public bool bleeding;` |
| 1679 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6099 | 2 | hemorrhage | bool | `public bool hemorrhage;` | `public bool hemorrhage;` |
| 1682 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6105 | 2 | shadowFlame | bool | `public bool shadowFlame;` | `public bool shadowFlame;` |
| 1683 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6107 | 2 | soulDrain | bool | `public bool soulDrain;` | `public bool soulDrain;` |
| 1684 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6109 | 2 | shimmering | bool | `public bool shimmering;` | `public bool shimmering;` |
| 1703 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6147 | 2 | oiled | bool | `public bool oiled;` | `public bool oiled;` |

##### 属性（0）

无该类型成员记录。


#### 4.14.11 细分子系统：`NpcControlAndSocialEffectState`

- 细分职责：混乱、魅惑、气味和 Dryad Ward 控制/社交效果标志。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；控制效果事件单向写入。
- 成员文件数：1；声明类型数：1；字段：4；属性：0；合计：4。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（4）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1688 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6117 | 2 | confused | bool | `public bool confused;` | `public bool confused;` |
| 1689 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6119 | 2 | loveStruck | bool | `public bool loveStruck;` | `public bool loveStruck;` |
| 1690 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6121 | 2 | stinky | bool | `public bool stinky;` | `public bool stinky;` |
| 1691 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6123 | 2 | dryadWard | bool | `public bool dryadWard;` | `public bool dryadWard;` |

##### 属性（0）

无该类型成员记录。


#### 4.14.12 细分子系统：`NpcWhipAndSpecialEffectState`

- 细分职责：鞭类标记、特殊武器标记和 Betsy/Daybreak 效果标志。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；特殊命中事件单向更新。
- 成员文件数：1；声明类型数：1；字段：9；属性：0；合计：9。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（9）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1680 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6101 | 2 | markedByScytheWhip | bool | `public bool markedByScytheWhip;` | `public bool markedByScytheWhip;` |
| 1681 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6103 | 2 | markedByEelWhip | bool | `public bool markedByEelWhip;` | `public bool markedByEelWhip;` |
| 1695 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6131 | 2 | javelined | bool | `public bool javelined;` | `public bool javelined;` |
| 1696 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6133 | 2 | tentacleSpiked | bool | `public bool tentacleSpiked;` | `public bool tentacleSpiked;` |
| 1697 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6135 | 2 | bloodButchered | bool | `public bool bloodButchered;` | `public bool bloodButchered;` |
| 1698 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6137 | 2 | celled | bool | `public bool celled;` | `public bool celled;` |
| 1699 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6139 | 2 | dryadBane | bool | `public bool dryadBane;` | `public bool dryadBane;` |
| 1700 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6141 | 2 | daybreak | bool | `public bool daybreak;` | `public bool daybreak;` |
| 1702 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6145 | 2 | betsysCurse | bool | `public bool betsysCurse;` | `public bool betsysCurse;` |

##### 属性（0）

无该类型成员记录。


#### 4.14.13 细分子系统：`NpcRegenerationAndProtectionState`

- 细分职责：生命回复、不可受伤、可追击和特殊保护状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；回复和保护规则显式排序。
- 成员文件数：1；声明类型数：1；字段：8；属性：0；合计：8。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（8）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1685 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6111 | 2 | lifeRegen | int | `public int lifeRegen;` | `public int lifeRegen;` |
| 1686 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6113 | 2 | lifeRegenCount | int | `public int lifeRegenCount;` | `public int lifeRegenCount;` |
| 1687 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6115 | 2 | lifeRegenExpectedLossPerSecond | int | `public int lifeRegenExpectedLossPerSecond = -1;` | `public int lifeRegenExpectedLossPerSecond = -1;` |
| 1692 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6125 | 2 | immortal | bool | `public bool immortal;` | `public bool immortal;` |
| 1693 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6127 | 2 | chaseable | bool | `public bool chaseable = true;` | `public bool chaseable = true;` |
| 1694 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6129 | 2 | canGhostHeal | bool | `public bool canGhostHeal = true;` | `public bool canGhostHeal = true;` |
| 1701 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6143 | 2 | dontTakeDamageFromHostiles | bool | `public bool dontTakeDamageFromHostiles;` | `public bool dontTakeDamageFromHostiles;` |
| 1704 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6149 | 2 | electricEelCounter | int | `public int electricEelCounter;` | `public int electricEelCounter;` |

##### 属性（0）

无该类型成员记录。


#### 4.14.21 细分子系统：`NpcLifecycleAndCrossDomainRefs`

- 细分职责：NPC 发现/拥有对象缓存和复仇管理器引用。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System 读取输入并通过显式 Command/CommitPort 写入；跨组只用事件、Query 或命令交接。
- 成员文件数：1；声明类型数：1；字段：4；属性：0；合计：4。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（4）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1777 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6295 | 2 | lazyNPCOwnedProjectileSearchArray | int[] | `public static int[] lazyNPCOwnedProjectileSearchArray = new int[InitData.MaxNPCs];` | `public static int[] lazyNPCOwnedProjectileSearchArray = new int[InitData.MaxNPCs];` |
| 1778 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6297 | 2 | spawnSlotProtected | int[] | `public static int[] spawnSlotProtected = new int[InitData.MaxNPCs];` | `public static int[] spawnSlotProtected = new int[InitData.MaxNPCs];` |
| 1779 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6299 | 2 | soundDelay | int | `public int soundDelay;` | `public int soundDelay;` |
| 1780 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6301 | 2 | RevengeManager | Terraria.GameContent.CoinLossRevengeSystem | `public static CoinLossRevengeSystem RevengeManager = new CoinLossRevengeSystem();` | `public static CoinLossRevengeSystem RevengeManager = new CoinLossRevengeSystem();` |

##### 属性（0）

无该类型成员记录。


#### 4.14.6 细分子系统：`NpcNetworkReplicationState`

- 细分职责：NPC 网络更新节流、同步流和玩家同步状态。
- 边界角色：`registry/projection`；最小 seam：Network Projection seam；网络投影只读取 NPC 权威状态。
- 成员文件数：1；声明类型数：1；字段：11；属性：0；合计：11。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（11）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1638 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6017 | 2 | netUpdatePendingSpamCooldown | bool | `internal bool netUpdatePendingSpamCooldown;` | `internal bool netUpdatePendingSpamCooldown;` |
| 1639 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6019 | 2 | netUpdatePendingFullSpamCooldown | bool | `internal bool netUpdatePendingFullSpamCooldown;` | `internal bool netUpdatePendingFullSpamCooldown;` |
| 1640 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6021 | 2 | netSpamPacketLimit | int | `public readonly int netSpamPacketLimit = 3;` | `public readonly int netSpamPacketLimit = 3;` |
| 1641 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6023 | 2 | netSpamTicksPerPacket | int | `public readonly int netSpamTicksPerPacket = 30;` | `public readonly int netSpamTicksPerPacket = 30;` |
| 1642 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6025 | 2 | netSpamTicksPerPacketForBosses | int | `public readonly int netSpamTicksPerPacketForBosses = 5;` | `public readonly int netSpamTicksPerPacketForBosses = 5;` |
| 1643 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6027 | 2 | netSpam | int | `public int netSpam;` | `public int netSpam;` |
| 1644 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6029 | 2 | netAlways | bool | `public bool netAlways;` | `public bool netAlways;` |
| 1645 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6031 | 2 | spawnNeedsSyncing | bool | `public bool spawnNeedsSyncing;` | `public bool spawnNeedsSyncing;` |
| 1646 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6033 | 2 | netStream | int | `internal int netStream;` | `internal int netStream;` |
| 1647 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6035 | 2 | playerNetSyncState | Terraria.NPC.PlayerNetSyncState[] | `internal PlayerNetSyncState[] playerNetSyncState = new PlayerNetSyncState[255];` | `internal PlayerNetSyncState[] playerNetSyncState = new PlayerNetSyncState[255];` |
| 1648 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6037 | 2 | netOffset | Vector2 | `public Vector2 netOffset = Vector2.Zero;` | `public Vector2 netOffset = Vector2.Zero;` |

##### 属性（0）

无该类型成员记录。


#### 4.14.41 细分子系统：`NpcNetworkSyncState`

- 细分职责：玩家对 NPC 同步流的节流状态。
- 边界角色：`registry/projection`；最小 seam：Registry/Projection seam；提交后的事实单向输出，不把网络/UI/索引反写成权威状态。
- 成员文件数：1；声明类型数：1；字段：2；属性：0；合计：2。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（2）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1517 | field | Terraria.NPC.PlayerNetSyncState | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 34 | 3 | skippedSyncs | byte | `public byte skippedSyncs;` | `public byte skippedSyncs;` |
| 1518 | field | Terraria.NPC.PlayerNetSyncState | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 36 | 3 | streamCounter | byte | `public byte streamCounter;` | `public byte streamCounter;` |

##### 属性（0）

无该类型成员记录。


#### 4.14.37 细分子系统：`NpcDamageDefinitionRegistry`

- 细分职责：Boss 类型和复合 NPC 定义注册表。
- 边界角色：`registry/projection`；最小 seam：Damage Definition Registry；注册结果只读提供给追踪系统。
- 成员文件数：1；声明类型数：2；字段：4；属性：0；合计：4。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（4）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1499 | field | Terraria.GameContent.NPCDamageTracker.CustomDefinition | Terraria.GameContent/NPCDamageTracker.cs | D:\TRbackup\Version4\Terraria.GameContent\NPCDamageTracker.cs | 14 | 3 | NPCTypes | System.Collections.Generic.List<int> | `public List<int> NPCTypes;` | `public List<int> NPCTypes;` |
| 1500 | field | Terraria.GameContent.NPCDamageTracker.CustomDefinition | Terraria.GameContent/NPCDamageTracker.cs | D:\TRbackup\Version4\Terraria.GameContent\NPCDamageTracker.cs | 16 | 3 | Name | Terraria.Localization.LocalizedText | `public LocalizedText Name;` | `public LocalizedText Name;` |
| 1502 | field | Terraria.GameContent.NPCDamageTracker | Terraria.GameContent/NPCDamageTracker.cs | D:\TRbackup\Version4\Terraria.GameContent\NPCDamageTracker.cs | 46 | 2 | CustomBossDefinitions | Terraria.GameContent.NPCDamageTracker.CustomDefinition[] | `public static CustomDefinition[] CustomBossDefinitions;` | `public static CustomDefinition[] CustomBossDefinitions;` |
| 1503 | field | Terraria.GameContent.NPCDamageTracker | Terraria.GameContent/NPCDamageTracker.cs | D:\TRbackup\Version4\Terraria.GameContent\NPCDamageTracker.cs | 48 | 2 | BossTypeForMob | int[] | `public static int[] BossTypeForMob;` | `public static int[] BossTypeForMob;` |

##### 属性（0）

无该类型成员记录。


#### 4.14.38 细分子系统：`NpcDamageRuntimeTracking`

- 细分职责：活动/已完成追踪器、攻击者和命中时间运行时状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Damage Tracker System/CommitPort；命中事件顺序化更新追踪状态。
- 成员文件数：1；声明类型数：1；字段：9；属性：3；合计：12。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（9）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1504 | field | Terraria.GameContent.NPCDamageTracker | Terraria.GameContent/NPCDamageTracker.cs | D:\TRbackup\Version4\Terraria.GameContent\NPCDamageTracker.cs | 50 | 2 | _activeTrackers | System.Collections.Generic.List<Terraria.GameContent.NPCDamageTracker> | `private static List<NPCDamageTracker> _activeTrackers;` | `private static List<NPCDamageTracker> _activeTrackers;` |
| 1505 | field | Terraria.GameContent.NPCDamageTracker | Terraria.GameContent/NPCDamageTracker.cs | D:\TRbackup\Version4\Terraria.GameContent\NPCDamageTracker.cs | 52 | 2 | _recentFinishedTrackers | System.Collections.Generic.List<Terraria.GameContent.NPCDamageTracker> | `private static List<NPCDamageTracker> _recentFinishedTrackers;` | `private static List<NPCDamageTracker> _recentFinishedTrackers;` |
| 1506 | field | Terraria.GameContent.NPCDamageTracker | Terraria.GameContent/NPCDamageTracker.cs | D:\TRbackup\Version4\Terraria.GameContent\NPCDamageTracker.cs | 54 | 2 | MAX_RECENT_TRACKERS | int | `private static readonly int MAX_RECENT_TRACKERS;` | `private static readonly int MAX_RECENT_TRACKERS;` |
| 1507 | field | Terraria.GameContent.NPCDamageTracker | Terraria.GameContent/NPCDamageTracker.cs | D:\TRbackup\Version4\Terraria.GameContent\NPCDamageTracker.cs | 56 | 2 | EXTRA_RECENT_TRACKER_EXPIRY_TIME | int | `private static readonly int EXTRA_RECENT_TRACKER_EXPIRY_TIME;` | `private static readonly int EXTRA_RECENT_TRACKER_EXPIRY_TIME;` |
| 1508 | field | Terraria.GameContent.NPCDamageTracker | Terraria.GameContent/NPCDamageTracker.cs | D:\TRbackup\Version4\Terraria.GameContent\NPCDamageTracker.cs | 58 | 2 | _list | System.Collections.Generic.List<Terraria.GameContent.NPCDamageTracker.CreditEntry> | `private readonly List<CreditEntry> _list = new List<CreditEntry>(255);` | `private readonly List<CreditEntry> _list = new List<CreditEntry>(255);` |
| 1509 | field | Terraria.GameContent.NPCDamageTracker | Terraria.GameContent/NPCDamageTracker.cs | D:\TRbackup\Version4\Terraria.GameContent\NPCDamageTracker.cs | 60 | 2 | _worldCredit | Terraria.GameContent.NPCDamageTracker.WorldCreditEntry | `private WorldCreditEntry _worldCredit;` | `private WorldCreditEntry _worldCredit;` |
| 1510 | field | Terraria.GameContent.NPCDamageTracker | Terraria.GameContent/NPCDamageTracker.cs | D:\TRbackup\Version4\Terraria.GameContent\NPCDamageTracker.cs | 62 | 2 | _lastAttacker | Terraria.GameContent.NPCDamageTracker.CreditEntry | `private CreditEntry _lastAttacker;` | `private CreditEntry _lastAttacker;` |
| 1511 | field | Terraria.GameContent.NPCDamageTracker | Terraria.GameContent/NPCDamageTracker.cs | D:\TRbackup\Version4\Terraria.GameContent\NPCDamageTracker.cs | 64 | 2 | _ticks | int | `private int _ticks;` | `private int _ticks;` |
| 1512 | field | Terraria.GameContent.NPCDamageTracker | Terraria.GameContent/NPCDamageTracker.cs | D:\TRbackup\Version4\Terraria.GameContent\NPCDamageTracker.cs | 66 | 2 | _lastHitTime | int | `private int _lastHitTime;` | `private int _lastHitTime;` |

##### 属性（3）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1874 | property | Terraria.GameContent.NPCDamageTracker | Terraria.GameContent/NPCDamageTracker.cs | D:\TRbackup\Version4\Terraria.GameContent\NPCDamageTracker.cs | 68 | 2 | IsEmpty | bool | `public bool IsEmpty => _list.Count == 0;` | `public bool IsEmpty => _list.Count == 0;` |
| 1875 | property | Terraria.GameContent.NPCDamageTracker | Terraria.GameContent/NPCDamageTracker.cs | D:\TRbackup\Version4\Terraria.GameContent\NPCDamageTracker.cs | 70 | 2 | Duration | int | `public int Duration => _lastHitTime;` | `public int Duration => _lastHitTime;` |
| 1876 | property | Terraria.GameContent.NPCDamageTracker | Terraria.GameContent/NPCDamageTracker.cs | D:\TRbackup\Version4\Terraria.GameContent\NPCDamageTracker.cs | 72 | 2 | TimeSinceLastHit | int | `public int TimeSinceLastHit => _ticks - _lastHitTime;` | `public int TimeSinceLastHit => _ticks - _lastHitTime;` |


#### 4.14.39 细分子系统：`NpcDamageCreditProjection`

- 细分职责：玩家、世界和击杀时间的伤害 credit 投影。
- 边界角色：`derived/query`；最小 seam：Credit Projection/Query；投影只读消费追踪快照。
- 成员文件数：1；声明类型数：4；字段：1；属性：6；合计：7。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（1）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1501 | field | Terraria.GameContent.NPCDamageTracker.PlayerCreditEntry | Terraria.GameContent/NPCDamageTracker.cs | D:\TRbackup\Version4\Terraria.GameContent\NPCDamageTracker.cs | 31 | 3 | PlayerName | string | `public readonly string PlayerName;` | `public readonly string PlayerName;` |

##### 属性（6）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1870 | property | Terraria.GameContent.NPCDamageTracker.CreditEntry | Terraria.GameContent/NPCDamageTracker.cs | D:\TRbackup\Version4\Terraria.GameContent\NPCDamageTracker.cs | 21 | 3 | Damage | int | `public int Damage { get; set; }` | `public int Damage { get; set; }` |
| 1871 | property | Terraria.GameContent.NPCDamageTracker.CreditEntry | Terraria.GameContent/NPCDamageTracker.cs | D:\TRbackup\Version4\Terraria.GameContent\NPCDamageTracker.cs | 23 | 3 | Name | Terraria.Localization.NetworkText | `public abstract NetworkText Name { get; }` | `public abstract NetworkText Name { get; }` |
| 1872 | property | Terraria.GameContent.NPCDamageTracker.PlayerCreditEntry | Terraria.GameContent/NPCDamageTracker.cs | D:\TRbackup\Version4\Terraria.GameContent\NPCDamageTracker.cs | 33 | 3 | Name | Terraria.Localization.NetworkText | `public override NetworkText Name => NetworkText.FromLiteral(PlayerName);` | `public override NetworkText Name => NetworkText.FromLiteral(PlayerName);` |
| 1873 | property | Terraria.GameContent.NPCDamageTracker.WorldCreditEntry | Terraria.GameContent/NPCDamageTracker.cs | D:\TRbackup\Version4\Terraria.GameContent\NPCDamageTracker.cs | 43 | 3 | Name | Terraria.Localization.NetworkText | `public override NetworkText Name => NetworkText.FromKey("BossDamageCommand.WorldCreditName");` | `public override NetworkText Name => NetworkText.FromKey("BossDamageCommand.WorldCreditName");` |
| 1877 | property | Terraria.GameContent.NPCDamageTracker | Terraria.GameContent/NPCDamageTracker.cs | D:\TRbackup\Version4\Terraria.GameContent\NPCDamageTracker.cs | 74 | 2 | Name | Terraria.Localization.LocalizedText | `public abstract LocalizedText Name { get; }` | `public abstract LocalizedText Name { get; }` |
| 1878 | property | Terraria.GameContent.NPCDamageTracker | Terraria.GameContent/NPCDamageTracker.cs | D:\TRbackup\Version4\Terraria.GameContent\NPCDamageTracker.cs | 76 | 2 | KillTimeMessage | Terraria.Localization.LocalizedText | `public abstract LocalizedText KillTimeMessage { get; }` | `public abstract LocalizedText KillTimeMessage { get; }` |


#### 4.14.40 细分子系统：`NpcInteractionAndCommerce`

- 细分职责：NPC 对话、回家、商店和 Angler 任务交互数据。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System 读取输入并通过显式 Command/CommitPort 写入；跨组只用事件、Query 或命令交接。
- 成员文件数：2；声明类型数：6；字段：4；属性：7；合计：11。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（4）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1513 | field | Terraria.GameContent.NPCInteractions.Actions.OpenShop | Terraria.GameContent/NPCInteractions.cs | D:\TRbackup\Version4\Terraria.GameContent\NPCInteractions.cs | 27 | 4 | _shopIndex | int | `private int _shopIndex;` | `private int _shopIndex;` |
| 1514 | field | Terraria.GameContent.NPCInteractions.Actions.OpenShop | Terraria.GameContent/NPCInteractions.cs | D:\TRbackup\Version4\Terraria.GameContent\NPCInteractions.cs | 29 | 4 | _npcType | int | `private int _npcType;` | `private int _npcType;` |
| 1515 | field | Terraria.GameContent.NPCInteractions.Actions.OpenShop | Terraria.GameContent/NPCInteractions.cs | D:\TRbackup\Version4\Terraria.GameContent\NPCInteractions.cs | 31 | 4 | _customTextKey | string | `private string _customTextKey;` | `private string _customTextKey;` |
| 1516 | field | Terraria.GameContent.NPCInteractions | Terraria.GameContent/NPCInteractions.cs | D:\TRbackup\Version4\Terraria.GameContent\NPCInteractions.cs | 233 | 2 | All | System.Collections.Generic.List<Terraria.GameContent.NPCInteraction> | `public static List<NPCInteraction> All = new List<NPCInteraction>();` | `public static List<NPCInteraction> All = new List<NPCInteraction>();` |

##### 属性（7）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1879 | property | Terraria.GameContent.NPCInteraction | Terraria.GameContent/NPCInteraction.cs | D:\TRbackup\Version4\Terraria.GameContent\NPCInteraction.cs | 7 | 2 | ShowExcalmation | bool | `public virtual bool ShowExcalmation => false;` | `public virtual bool ShowExcalmation => false;` |
| 1880 | property | Terraria.GameContent.NPCInteraction | Terraria.GameContent/NPCInteraction.cs | D:\TRbackup\Version4\Terraria.GameContent\NPCInteraction.cs | 9 | 2 | LocalPlayer | Terraria.Player | `public Player LocalPlayer => Main.LocalPlayer;` | `public Player LocalPlayer => Main.LocalPlayer;` |
| 1881 | property | Terraria.GameContent.NPCInteraction | Terraria.GameContent/NPCInteraction.cs | D:\TRbackup\Version4\Terraria.GameContent\NPCInteraction.cs | 11 | 2 | TalkNPC | Terraria.NPC | `public NPC TalkNPC => Main.npc[LocalPlayer.talkNPC];` | `public NPC TalkNPC => Main.npc[LocalPlayer.talkNPC];` |
| 1882 | property | Terraria.GameContent.NPCInteraction | Terraria.GameContent/NPCInteraction.cs | D:\TRbackup\Version4\Terraria.GameContent\NPCInteraction.cs | 13 | 2 | TalkNPCType | int | `public int TalkNPCType { get { if (LocalPlayer.talkNPC == -1) { return 0; } return TalkNPC.type; } }` | `public int TalkNPCType { get { if (LocalPlayer.talkNPC == -1) { return 0; } return TalkNPC.type; } }` |
| 1883 | property | Terraria.GameContent.NPCInteractions.Actions.StardewValleyBit | Terraria.GameContent/NPCInteractions.cs | D:\TRbackup\Version4\Terraria.GameContent\NPCInteractions.cs | 50 | 4 | ShowExcalmation | bool | `public override bool ShowExcalmation => true;` | `public override bool ShowExcalmation => true;` |
| 1884 | property | Terraria.GameContent.NPCInteractions.Actions.AnglerQuest | Terraria.GameContent/NPCInteractions.cs | D:\TRbackup\Version4\Terraria.GameContent\NPCInteractions.cs | 72 | 4 | ShowExcalmation | bool | `public override bool ShowExcalmation => !Main.anglerQuestFinished;` | `public override bool ShowExcalmation => !Main.anglerQuestFinished;` |
| 1885 | property | Terraria.GameContent.NPCInteractions.Actions.RequestHome | Terraria.GameContent/NPCInteractions.cs | D:\TRbackup\Version4\Terraria.GameContent\NPCInteractions.cs | 162 | 4 | ShowExcalmation | bool | `public override bool ShowExcalmation => true;` | `public override bool ShowExcalmation => true;` |


## 8. 本分区自检

- 叶子子系统：19 个；指定组均已写入且没有重复分区。
- 字段/属性/成员：173 / 16 / 189。
- 来源序号范围：1499..1885；本分区内唯一序号：True。
- 成员声明表逐组从源报告提取，未改写来源序号、类型、路径、行列、成员、C# 类型、成员声明或原始声明。
- 运行时读写者、生命周期、持久化、网络和调度：未验证，保留为迁移前风险。
