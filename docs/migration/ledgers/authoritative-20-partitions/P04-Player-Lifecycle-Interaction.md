# P04 玩家身份、生命周期与世界交互 - 权威模拟系统成员组件候选分区

> 来源：`docs/migration/ledgers/Version4权威模拟系统字段属性逐成员源码声明-更细子系统拆分-去除ID类文件.md`；本文件只做成员库存分区和候选组件边界记录。
> 这是候选组件拆分台账，不是运行时迁移完成、行为等价、API 兼容、网络闭合或持久化闭合证明。

## 1. 分区边界

本分区包含 13 个叶子子系统，字段 111 条、属性 1 条、成员合计 112 条；每个叶子子系统保持原正式父级归属，不跨分区拆行。

## 2. 分区覆盖概览

| 叶子子系统 | 正式父级 | 字段 | 属性 | 合计 | 源边界角色 |
|---|---|---:|---:|---:|---|
| `PlayerIdentityAndDeathRecordState` | `PlayerGameplay` | 10 | 0 | 10 | authoritative state/behavior |
| `PlayerRuntimeInteractionAndEffectState` | `PlayerGameplay` | 13 | 0 | 13 | authoritative state/behavior |
| `PlayerTeleportTransitionState` | `PlayerGameplay` | 4 | 0 | 4 | authoritative state/behavior |
| `PlayerDeathRespawnAndSaveState` | `PlayerGameplay` | 14 | 0 | 14 | authoritative state/behavior |
| `PlayerSpawnAndReturnState` | `PlayerGameplay` | 4 | 0 | 4 | authoritative state/behavior |
| `PlayerContainerAndWorldAnchorState` | `PlayerGameplay` | 17 | 0 | 17 | authoritative state/behavior |
| `PlayerPortalAndTargetingState` | `PlayerGameplay` | 11 | 0 | 11 | authoritative state/behavior |
| `PlayerItemActionTimingState` | `PlayerGameplay` | 12 | 0 | 12 | authoritative state/behavior |
| `PlayerItemCheckContext` | `PlayerGameplay` | 1 | 0 | 1 | authoritative state/behavior |
| `PlayerPettingState` | `PlayerGameplay` | 7 | 0 | 7 | authoritative state/behavior |
| `PlayerSittingState` | `PlayerGameplay` | 5 | 0 | 5 | authoritative state/behavior |
| `PlayerSleepingState` | `PlayerGameplay` | 6 | 1 | 7 | authoritative state/behavior |
| `PlayerRabbitOrderFrameState` | `PlayerGameplay` | 7 | 0 | 7 | registry/projection |

## 3. 候选组件归属与 seam

| 叶子子系统 | 候选组件边界 | 类型分类 | 最小 seam | 独立生命周期 | 证据状态 | 主要风险 |
|---|---|---|---|---|---|---|
| `PlayerIdentityAndDeathRecordState` | `PlayerIdentityAndDeathRecordStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerRuntimeInteractionAndEffectState` | `PlayerRuntimeInteractionAndEffectStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerTeleportTransitionState` | `PlayerTeleportTransitionStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerDeathRespawnAndSaveState` | `PlayerDeathRespawnAndSaveStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerSpawnAndReturnState` | `PlayerSpawnAndReturnStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerContainerAndWorldAnchorState` | `PlayerContainerAndWorldAnchorStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerPortalAndTargetingState` | `PlayerPortalAndTargetingStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerItemActionTimingState` | `PlayerItemActionTimingStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerItemCheckContext` | `PlayerItemCheckContextComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerPettingState` | `PlayerPettingStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerSittingState` | `PlayerSittingStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerSleepingState` | `PlayerSleepingStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerRabbitOrderFrameState` | `PlayerRabbitOrderFrameStateProjection`/`Adapter` | Projection/Adapter | 单向 Projection/Adapter seam；不得回写权威状态 | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |

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
- 本分区父级局部统计：字段 111；属性 1；合计 112；完整父级统计以源报告为准。

#### 4.13.1 细分子系统：`PlayerIdentityAndDeathRecordState`

- 细分职责：玩家活动、主机身份、名称、死亡次数和死亡记录。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；玩家生命周期和死亡结算集中提交。
- 成员文件数：1；声明类型数：1；字段：10；属性：0；合计：10。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（10）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 445 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 478 | 2 | active | bool | `public bool active;` | `public bool active;` |
| 446 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 480 | 2 | host | bool | `public bool host;` | `public bool host;` |
| 454 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 497 | 2 | lostCoins | long | `public long lostCoins;` | `public long lostCoins;` |
| 455 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 499 | 2 | lostCoinString | string | `public string lostCoinString = "";` | `public string lostCoinString = "";` |
| 458 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 505 | 2 | name | string | `public string name = "";` | `public string name = "";` |
| 459 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 507 | 2 | numberOfDeathsPVE | int | `public int numberOfDeathsPVE;` | `public int numberOfDeathsPVE;` |
| 460 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 509 | 2 | numberOfDeathsPVP | int | `public int numberOfDeathsPVP;` | `public int numberOfDeathsPVP;` |
| 465 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 519 | 2 | lastDeathPostion | Vector2 | `public Vector2 lastDeathPostion;` | `public Vector2 lastDeathPostion;` |
| 466 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 521 | 2 | lastDeathTime | System.DateTime | `public DateTime lastDeathTime;` | `public DateTime lastDeathTime;` |
| 467 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 523 | 2 | showLastDeath | bool | `public bool showLastDeath;` | `public bool showLastDeath;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.2 细分子系统：`PlayerRuntimeInteractionAndEffectState`

- 细分职责：矿车、表情、建造器、抓钩、探测器和运行时效果状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；交互/效果系统通过显式命令更新。
- 成员文件数：1；声明类型数：1；字段：13；属性：0；合计：13。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（13）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 447 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 482 | 2 | MinecartSettings | Terraria.Minecart.Customization | `public Minecart.Customization MinecartSettings;` | `public Minecart.Customization MinecartSettings;` |
| 448 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 484 | 2 | emoteTime | int | `public int emoteTime;` | `public int emoteTime;` |
| 449 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 486 | 2 | creativeTracker | Terraria.GameContent.Creative.CreativeUnlocksTracker | `public CreativeUnlocksTracker creativeTracker;` | `public CreativeUnlocksTracker creativeTracker;` |
| 450 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 488 | 2 | chatOverhead | Terraria.Player.OverheadMessage | `public OverheadMessage chatOverhead;` | `public OverheadMessage chatOverhead;` |
| 451 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 491 | 2 | GoingDownWithGrapple | bool | `public bool GoingDownWithGrapple;` | `public bool GoingDownWithGrapple;` |
| 452 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 493 | 2 | spelunkerTimer | byte | `public byte spelunkerTimer;` | `public byte spelunkerTimer;` |
| 453 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 495 | 2 | builderAccStatus | int[] | `public int[] builderAccStatus = new int[BuilderAccToggleIDs.Count];` | `public int[] builderAccStatus = new int[BuilderAccToggleIDs.Count];` |
| 456 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 501 | 2 | soulDrain | int | `public int soulDrain;` | `public int soulDrain;` |
| 457 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 503 | 2 | dd2Accessory | bool | `public bool dd2Accessory;` | `public bool dd2Accessory;` |
| 461 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 511 | 2 | crystalLeafDamage | int | `public static int crystalLeafDamage = 100;` | `public static int crystalLeafDamage = 100;` |
| 462 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 513 | 2 | crystalLeafKB | int | `public static int crystalLeafKB = 10;` | `public static int crystalLeafKB = 10;` |
| 463 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 515 | 2 | basiliskCharge | float | `public float basiliskCharge;` | `public float basiliskCharge;` |
| 464 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 517 | 2 | PaladinsShieldRange | float | `public static float PaladinsShieldRange = 800f;` | `public static float PaladinsShieldRange = 800f;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.8 细分子系统：`PlayerTeleportTransitionState`

- 细分职责：玩家传送过渡样式、计时和未确认传送状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；传送事务按阶段提交并等待确认。
- 成员文件数：1；声明类型数：1；字段：4；属性：0；合计：4。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（4）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 528 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 646 | 2 | teleporting | bool | `public bool teleporting;` | `public bool teleporting;` |
| 529 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 648 | 2 | teleportTime | float | `public float teleportTime;` | `public float teleportTime;` |
| 530 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 650 | 2 | teleportStyle | int | `public int teleportStyle;` | `public int teleportStyle;` |
| 531 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 652 | 2 | unacknowledgedTeleports | int | `public int unacknowledgedTeleports;` | `public int unacknowledgedTeleports;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.34 细分子系统：`PlayerDeathRespawnAndSaveState`

- 细分职责：死亡、观战、复活计时、保存时间和受击辅助状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；死亡与复活事务通过显式生命周期命令提交。
- 成员文件数：1；声明类型数：1；字段：14；属性：0；合计：14。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（14）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 768 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1134 | 2 | dead | bool | `public bool dead;` | `public bool dead;` |
| 769 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1136 | 2 | deadTime | int | `public int deadTime;` | `public int deadTime;` |
| 770 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1138 | 2 | spectating | int | `public int spectating = -1;` | `public int spectating = -1;` |
| 771 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1140 | 2 | respawnTimer | int | `public int respawnTimer;` | `public int respawnTimer;` |
| 772 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1142 | 2 | respawnTimerMax | int | `public static readonly int respawnTimerMax = 3600;` | `public static readonly int respawnTimerMax = 3600;` |
| 773 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1144 | 2 | DeadSpectatingLockoutTime | int | `public static readonly int DeadSpectatingLockoutTime = 60;` | `public static readonly int DeadSpectatingLockoutTime = 60;` |
| 774 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1146 | 2 | SpectatingLingerAfterDeath | int | `public static readonly int SpectatingLingerAfterDeath = 180;` | `public static readonly int SpectatingLingerAfterDeath = 180;` |
| 775 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1148 | 2 | lastTimePlayerWasSaved | long | `public long lastTimePlayerWasSaved;` | `public long lastTimePlayerWasSaved;` |
| 776 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1150 | 2 | attackCD | int | `public int attackCD;` | `public int attackCD;` |
| 777 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1152 | 2 | potionDelay | int | `public int potionDelay;` | `public int potionDelay;` |
| 778 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1154 | 2 | difficulty | byte | `public byte difficulty;` | `public byte difficulty;` |
| 779 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1156 | 2 | wetSlime | byte | `public byte wetSlime;` | `public byte wetSlime;` |
| 780 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1158 | 2 | hitTile | Terraria.HitTile | `public HitTile hitTile;` | `public HitTile hitTile;` |
| 781 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1160 | 2 | hitReplace | Terraria.HitTile | `public HitTile hitReplace;` | `public HitTile hitReplace;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.64 细分子系统：`PlayerSpawnAndReturnState`

- 细分职责：出生点和回城药水原始位置状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Spawn/Return System/CommitPort；出生与回城事件集中写入。
- 成员文件数：1；声明类型数：1；字段：4；属性：0；合计：4。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（4）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1132 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1895 | 2 | SpawnX | int | `public int SpawnX = -1;` | `public int SpawnX = -1;` |
| 1133 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1897 | 2 | SpawnY | int | `public int SpawnY = -1;` | `public int SpawnY = -1;` |
| 1134 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1899 | 2 | PotionOfReturnOriginalUsePosition | Vector2? | `public Vector2? PotionOfReturnOriginalUsePosition;` | `public Vector2? PotionOfReturnOriginalUsePosition;` |
| 1135 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1901 | 2 | PotionOfReturnHomePosition | Vector2? | `public Vector2? PotionOfReturnHomePosition;` | `public Vector2? PotionOfReturnHomePosition;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.88 细分子系统：`PlayerContainerAndWorldAnchorState`

- 细分职责：箱体、TileEntity、住房交互、坐卧辅助和世界锚点状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；容器和世界交互通过命令提交。
- 成员文件数：1；声明类型数：1；字段：17；属性：0；合计：17。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（17）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1310 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2264 | 2 | lastChest | int | `public int lastChest;` | `public int lastChest;` |
| 1311 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2266 | 2 | piggyBankProjTracker | Terraria.DataStructures.TrackedProjectileReference | `public TrackedProjectileReference piggyBankProjTracker;` | `public TrackedProjectileReference piggyBankProjTracker;` |
| 1312 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2268 | 2 | voidLensChest | Terraria.DataStructures.TrackedProjectileReference | `public TrackedProjectileReference voidLensChest;` | `public TrackedProjectileReference voidLensChest;` |
| 1313 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2270 | 2 | chest | int | `public int chest = -1;` | `public int chest = -1;` |
| 1319 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2284 | 2 | petting | Terraria.GameContent.PlayerPettingInfo | `public PlayerPettingInfo petting;` | `public PlayerPettingInfo petting;` |
| 1320 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2286 | 2 | sitting | Terraria.GameContent.PlayerSittingHelper | `public PlayerSittingHelper sitting;` | `public PlayerSittingHelper sitting;` |
| 1321 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2288 | 2 | sleeping | Terraria.GameContent.PlayerSleepingHelper | `public PlayerSleepingHelper sleeping;` | `public PlayerSleepingHelper sleeping;` |
| 1322 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2290 | 2 | eyeHelper | Terraria.GameContent.PlayerEyeHelper | `public PlayerEyeHelper eyeHelper;` | `public PlayerEyeHelper eyeHelper;` |
| 1323 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2292 | 2 | tileEntityAnchor | Terraria.DataStructures.PlayerInteractionAnchor | `public PlayerInteractionAnchor tileEntityAnchor;` | `public PlayerInteractionAnchor tileEntityAnchor;` |
| 1324 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2294 | 2 | doorHelper | Terraria.GameContent.DoorOpeningHelper | `public DoorOpeningHelper doorHelper;` | `public DoorOpeningHelper doorHelper;` |
| 1325 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2296 | 2 | currentShoppingSettings | Terraria.ShoppingSettings | `public ShoppingSettings currentShoppingSettings = ShoppingSettings.NotInShop;` | `public ShoppingSettings currentShoppingSettings = ShoppingSettings.NotInShop;` |
| 1372 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2391 | 2 | TouchedTiles | System.Collections.Generic.List<Point> | `public List<Point> TouchedTiles = new List<Point>();` | `public List<Point> TouchedTiles = new List<Point>();` |
| 1381 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2410 | 2 | equippedAnyTileRangeAcc | bool | `public bool equippedAnyTileRangeAcc;` | `public bool equippedAnyTileRangeAcc;` |
| 1382 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2412 | 2 | equippedAnyTileSpeedAcc | bool | `public bool equippedAnyTileSpeedAcc;` | `public bool equippedAnyTileSpeedAcc;` |
| 1383 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2414 | 2 | equippedAnyWallSpeedAcc | bool | `public bool equippedAnyWallSpeedAcc;` | `public bool equippedAnyWallSpeedAcc;` |
| 1384 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2417 | 2 | behindBackWall | bool | `public bool behindBackWall;` | `public bool behindBackWall;` |
| 1385 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2419 | 2 | _funkytownAchievementCheckCooldown | int | `public int _funkytownAchievementCheckCooldown;` | `public int _funkytownAchievementCheckCooldown;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.86 细分子系统：`PlayerPortalAndTargetingState`

- 细分职责：传送门物理、传送塔样式、召唤物目标和跨实体目标索引。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；目标选择通过 Query/Command 交接。
- 成员文件数：1；声明类型数：1；字段：11；属性：0；合计：11。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（11）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1363 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2372 | 2 | ownedProjectileCounts | int[] | `public int[] ownedProjectileCounts = new int[ProjectileID.Count];` | `public int[] ownedProjectileCounts = new int[ProjectileID.Count];` |
| 1364 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2374 | 2 | npcTypeNoAggro | bool[] | `public bool[] npcTypeNoAggro = new bool[NPCID.Count];` | `public bool[] npcTypeNoAggro = new bool[NPCID.Count];` |
| 1365 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2376 | 2 | lastPortalColorIndex | int | `public int lastPortalColorIndex;` | `public int lastPortalColorIndex;` |
| 1366 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2378 | 2 | _portalPhysicsTime | int | `public int _portalPhysicsTime;` | `public int _portalPhysicsTime;` |
| 1367 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2380 | 2 | portalPhysicsFlag | bool | `public bool portalPhysicsFlag;` | `public bool portalPhysicsFlag;` |
| 1368 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2382 | 2 | lastTeleportPylonStyleUsed | int | `public int lastTeleportPylonStyleUsed;` | `public int lastTeleportPylonStyleUsed;` |
| 1369 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2384 | 2 | MountFishronSpecialCounter | float | `public float MountFishronSpecialCounter;` | `public float MountFishronSpecialCounter;` |
| 1370 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2386 | 2 | MinionRestTargetPoint | Vector2 | `public Vector2 MinionRestTargetPoint = Vector2.Zero;` | `public Vector2 MinionRestTargetPoint = Vector2.Zero;` |
| 1371 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2388 | 2 | MinionAttackTargetNPC | int | `public int MinionAttackTargetNPC = -1;` | `public int MinionAttackTargetNPC = -1;` |
| 1379 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2406 | 2 | _blackListedTileCoordsForGrappling | System.Collections.Generic.HashSet<Point> | `private HashSet<Point> _blackListedTileCoordsForGrappling = new HashSet<Point>();` | `private HashSet<Point> _blackListedTileCoordsForGrappling = new HashSet<Point>();` |
| 1380 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2408 | 2 | makeStrongBee | bool | `private bool makeStrongBee;` | `private bool makeStrongBee;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.87 细分子系统：`PlayerItemActionTimingState`

- 细分职责：掉落、药水、工具、物品动作和机关交互计时。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；时间推进由显式 Tick 输入驱动。
- 成员文件数：1；声明类型数：1；字段：12；属性：0；合计：12。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（12）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1309 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2262 | 2 | wireOperationsCooldown | int | `public int wireOperationsCooldown;` | `public int wireOperationsCooldown;` |
| 1314 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2273 | 2 | fallStart | int | `public int fallStart;` | `public int fallStart;` |
| 1315 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2275 | 2 | fallStart2 | int | `public int fallStart2;` | `public int fallStart2;` |
| 1316 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2277 | 2 | potionDelayTime | int | `public int potionDelayTime = Item.potionDelay;` | `public int potionDelayTime = Item.potionDelay;` |
| 1317 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2279 | 2 | restorationDelayTime | int | `public int restorationDelayTime = Item.restorationDelay;` | `public int restorationDelayTime = Item.restorationDelay;` |
| 1318 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2282 | 2 | mushroomDelayTime | int | `public int mushroomDelayTime = Item.mushroomDelay;` | `public int mushroomDelayTime = Item.mushroomDelay;` |
| 1373 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2393 | 2 | itemAnimation | int | `public int itemAnimation;` | `public int itemAnimation;` |
| 1374 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2395 | 2 | itemAnimationMax | int | `public int itemAnimationMax;` | `public int itemAnimationMax;` |
| 1375 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2397 | 2 | itemTime | int | `public int itemTime;` | `public int itemTime;` |
| 1376 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2399 | 2 | itemTimeMax | int | `public int itemTimeMax;` | `public int itemTimeMax;` |
| 1377 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2401 | 2 | toolTime | int | `public int toolTime;` | `public int toolTime;` |
| 1378 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2403 | 2 | BlockInteractionWithProjectiles | int | `public static int BlockInteractionWithProjectiles = 3;` | `public static int BlockInteractionWithProjectiles = 3;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.102 细分子系统：`PlayerItemCheckContext`

- 细分职责：物品检查阶段的消费跳过上下文。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System 读取输入并通过显式 Command/CommitPort 写入；跨组只用事件、Query 或命令交接。
- 成员文件数：1；声明类型数：1；字段：1；属性：0；合计：1。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（1）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 429 | field | Terraria.Player.ItemCheckContext | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 284 | 3 | SkipItemConsumption | bool | `public bool SkipItemConsumption;` | `public bool SkipItemConsumption;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.104 细分子系统：`PlayerPettingState`

- 细分职责：玩家抚摸 NPC、投射物或坐骑目标的交互状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；交互命令维护目标引用和生命周期。
- 成员文件数：1；声明类型数：1；字段：7；属性：0；合计：7。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（7）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 373 | field | Terraria.GameContent.PlayerPettingInfo | Terraria.GameContent/PlayerPettingInfo.cs | D:\TRbackup\Version4\Terraria.GameContent\PlayerPettingInfo.cs | 7 | 2 | isPetting | bool | `public bool isPetting;` | `public bool isPetting;` |
| 374 | field | Terraria.GameContent.PlayerPettingInfo | Terraria.GameContent/PlayerPettingInfo.cs | D:\TRbackup\Version4\Terraria.GameContent\PlayerPettingInfo.cs | 9 | 2 | npc | int | `public int npc;` | `public int npc;` |
| 375 | field | Terraria.GameContent.PlayerPettingInfo | Terraria.GameContent/PlayerPettingInfo.cs | D:\TRbackup\Version4\Terraria.GameContent\PlayerPettingInfo.cs | 11 | 2 | proj | int | `public int proj;` | `public int proj;` |
| 376 | field | Terraria.GameContent.PlayerPettingInfo | Terraria.GameContent/PlayerPettingInfo.cs | D:\TRbackup\Version4\Terraria.GameContent\PlayerPettingInfo.cs | 13 | 2 | type | int | `public int type;` | `public int type;` |
| 377 | field | Terraria.GameContent.PlayerPettingInfo | Terraria.GameContent/PlayerPettingInfo.cs | D:\TRbackup\Version4\Terraria.GameContent\PlayerPettingInfo.cs | 15 | 2 | mount | bool | `public bool mount;` | `public bool mount;` |
| 378 | field | Terraria.GameContent.PlayerPettingInfo | Terraria.GameContent/PlayerPettingInfo.cs | D:\TRbackup\Version4\Terraria.GameContent\PlayerPettingInfo.cs | 17 | 2 | offsetFromPet | Vector2 | `public Vector2 offsetFromPet;` | `public Vector2 offsetFromPet;` |
| 379 | field | Terraria.GameContent.PlayerPettingInfo | Terraria.GameContent/PlayerPettingInfo.cs | D:\TRbackup\Version4\Terraria.GameContent\PlayerPettingInfo.cs | 19 | 2 | isPetSmall | bool | `public bool isPetSmall;` | `public bool isPetSmall;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.105 细分子系统：`PlayerSittingState`

- 细分职责：玩家椅子坐姿、座位偏移和堆叠索引状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；座椅交互事件单向提交。
- 成员文件数：1；声明类型数：1；字段：5；属性：0；合计：5。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（5）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 380 | field | Terraria.GameContent.PlayerSittingHelper | Terraria.GameContent/PlayerSittingHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\PlayerSittingHelper.cs | 8 | 2 | ChairSittingMaxDistance | int | `public const int ChairSittingMaxDistance = 40;` | `public const int ChairSittingMaxDistance = 40;` |
| 381 | field | Terraria.GameContent.PlayerSittingHelper | Terraria.GameContent/PlayerSittingHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\PlayerSittingHelper.cs | 10 | 2 | isSitting | bool | `public bool isSitting;` | `public bool isSitting;` |
| 382 | field | Terraria.GameContent.PlayerSittingHelper | Terraria.GameContent/PlayerSittingHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\PlayerSittingHelper.cs | 12 | 2 | details | Terraria.GameContent.ExtraSeatInfo | `public ExtraSeatInfo details;` | `public ExtraSeatInfo details;` |
| 383 | field | Terraria.GameContent.PlayerSittingHelper | Terraria.GameContent/PlayerSittingHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\PlayerSittingHelper.cs | 14 | 2 | offsetForSeat | Vector2 | `public Vector2 offsetForSeat;` | `public Vector2 offsetForSeat;` |
| 384 | field | Terraria.GameContent.PlayerSittingHelper | Terraria.GameContent/PlayerSittingHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\PlayerSittingHelper.cs | 16 | 2 | sittingIndex | int | `public int sittingIndex;` | `public int sittingIndex;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.106 细分子系统：`PlayerSleepingState`

- 细分职责：玩家睡眠、入睡计时和床面投影状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；睡眠生命周期事件集中写入。
- 成员文件数：1；声明类型数：1；字段：6；属性：1；合计：7。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（6）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 385 | field | Terraria.GameContent.PlayerSleepingHelper | Terraria.GameContent/PlayerSleepingHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\PlayerSleepingHelper.cs | 9 | 2 | BedSleepingMaxDistance | int | `public const int BedSleepingMaxDistance = 96;` | `public const int BedSleepingMaxDistance = 96;` |
| 386 | field | Terraria.GameContent.PlayerSleepingHelper | Terraria.GameContent/PlayerSleepingHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\PlayerSleepingHelper.cs | 11 | 2 | TimeToFullyFallAsleep | int | `public const int TimeToFullyFallAsleep = 120;` | `public const int TimeToFullyFallAsleep = 120;` |
| 387 | field | Terraria.GameContent.PlayerSleepingHelper | Terraria.GameContent/PlayerSleepingHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\PlayerSleepingHelper.cs | 13 | 2 | isSleeping | bool | `public bool isSleeping;` | `public bool isSleeping;` |
| 388 | field | Terraria.GameContent.PlayerSleepingHelper | Terraria.GameContent/PlayerSleepingHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\PlayerSleepingHelper.cs | 15 | 2 | sleepingIndex | int | `public int sleepingIndex;` | `public int sleepingIndex;` |
| 389 | field | Terraria.GameContent.PlayerSleepingHelper | Terraria.GameContent/PlayerSleepingHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\PlayerSleepingHelper.cs | 17 | 2 | timeSleeping | int | `public int timeSleeping;` | `public int timeSleeping;` |
| 390 | field | Terraria.GameContent.PlayerSleepingHelper | Terraria.GameContent/PlayerSleepingHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\PlayerSleepingHelper.cs | 19 | 2 | visualOffsetOfBedBase | Vector2 | `public Vector2 visualOffsetOfBedBase;` | `public Vector2 visualOffsetOfBedBase;` |

##### 属性（1）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1418 | property | Terraria.GameContent.PlayerSleepingHelper | Terraria.GameContent/PlayerSleepingHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\PlayerSleepingHelper.cs | 21 | 2 | FullyFallenAsleep | bool | `public bool FullyFallenAsleep { get { if (isSleeping) { return timeSleeping >= 120; } return false; } }` | `public bool FullyFallenAsleep { get { if (isSleeping) { return timeSleeping >= 120; } return false; } }` |


#### 4.13.107 细分子系统：`PlayerRabbitOrderFrameState`

- 细分职责：兔子指令帧状态机及其表现帧计数。
- 边界角色：`registry/projection`；最小 seam：Registry/Projection seam；帧状态由表现更新消费。
- 成员文件数：1；声明类型数：1；字段：7；属性：0；合计：7。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（7）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 411 | field | Terraria.Player.RabbitOrderFrameHelper | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 142 | 3 | DisplayFrame | int | `public int DisplayFrame;` | `public int DisplayFrame;` |
| 412 | field | Terraria.Player.RabbitOrderFrameHelper | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 144 | 3 | _frameCounter | int | `private int _frameCounter;` | `private int _frameCounter;` |
| 413 | field | Terraria.Player.RabbitOrderFrameHelper | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 146 | 3 | _aiState | int | `private int _aiState;` | `private int _aiState;` |
| 414 | field | Terraria.Player.RabbitOrderFrameHelper | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 148 | 3 | AIState_Idle | int | `private const int AIState_Idle = 0;` | `private const int AIState_Idle = 0;` |
| 415 | field | Terraria.Player.RabbitOrderFrameHelper | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 150 | 3 | AIState_LookingAtCamera | int | `private const int AIState_LookingAtCamera = 1;` | `private const int AIState_LookingAtCamera = 1;` |
| 416 | field | Terraria.Player.RabbitOrderFrameHelper | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 152 | 3 | AIState_Resting | int | `private const int AIState_Resting = 2;` | `private const int AIState_Resting = 2;` |
| 417 | field | Terraria.Player.RabbitOrderFrameHelper | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 154 | 3 | AIState_EatingCarrot | int | `private const int AIState_EatingCarrot = 3;` | `private const int AIState_EatingCarrot = 3;` |

##### 属性（0）

无该类型成员记录。


## 8. 本分区自检

- 叶子子系统：13 个；指定组均已写入且没有重复分区。
- 字段/属性/成员：111 / 1 / 112。
- 来源序号范围：373..1418；本分区内唯一序号：True。
- 成员声明表逐组从源报告提取，未改写来源序号、类型、路径、行列、成员、C# 类型、成员声明或原始声明。
- 运行时读写者、生命周期、持久化、网络和调度：未验证，保留为迁移前风险。
