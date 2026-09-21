# P07 玩家跳跃、抓钩、飞行与移动穿越 - 权威模拟系统成员组件候选分区

> 来源：`docs/migration/ledgers/Version4权威模拟系统字段属性逐成员源码声明-更细子系统拆分-去除ID类文件.md`；本文件只做成员库存分区和候选组件边界记录。
> 这是候选组件拆分台账，不是运行时迁移完成、行为等价、API 兼容、网络闭合或持久化闭合证明。

## 1. 分区边界

本分区包含 11 个叶子子系统，字段 112 条、属性 0 条、成员合计 112 条；每个叶子子系统保持原正式父级归属，不跨分区拆行。

## 2. 分区覆盖概览

| 叶子子系统 | 正式父级 | 字段 | 属性 | 合计 | 源边界角色 |
|---|---|---:|---:|---:|---|
| `PlayerBeetleArmorState` | `PlayerGameplay` | 10 | 0 | 10 | authoritative state/behavior |
| `PlayerSolarAndNebulaArmorState` | `PlayerGameplay` | 10 | 0 | 10 | authoritative state/behavior |
| `PlayerMagnetAndUtilityAccessoryState` | `PlayerGameplay` | 8 | 0 | 8 | authoritative state/behavior |
| `PlayerDashAndGroundTraversalState` | `PlayerGameplay` | 12 | 0 | 12 | authoritative state/behavior |
| `PlayerRopeAndPulleyState` | `PlayerGameplay` | 10 | 0 | 10 | authoritative state/behavior |
| `PlayerSlideAndCarpetTraversalState` | `PlayerGameplay` | 10 | 0 | 10 | authoritative state/behavior |
| `PlayerWingsAndFlightState` | `PlayerGameplay` | 6 | 0 | 6 | authoritative state/behavior |
| `PlayerJumpAvailabilityState` | `PlayerGameplay` | 18 | 0 | 18 | authoritative state/behavior |
| `PlayerJumpExecutionState` | `PlayerGameplay` | 11 | 0 | 11 | authoritative state/behavior |
| `PlayerJumpMobilityModifiers` | `PlayerGameplay` | 5 | 0 | 5 | authoritative state/behavior |
| `PlayerGrappleAndRocketState` | `PlayerGameplay` | 12 | 0 | 12 | authoritative state/behavior |

## 3. 候选组件归属与 seam

| 叶子子系统 | 候选组件边界 | 类型分类 | 最小 seam | 独立生命周期 | 证据状态 | 主要风险 |
|---|---|---|---|---|---|---|
| `PlayerBeetleArmorState` | `PlayerBeetleArmorStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerSolarAndNebulaArmorState` | `PlayerSolarAndNebulaArmorStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerMagnetAndUtilityAccessoryState` | `PlayerMagnetAndUtilityAccessoryStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerDashAndGroundTraversalState` | `PlayerDashAndGroundTraversalStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerRopeAndPulleyState` | `PlayerRopeAndPulleyStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerSlideAndCarpetTraversalState` | `PlayerSlideAndCarpetTraversalStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerWingsAndFlightState` | `PlayerWingsAndFlightStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerJumpAvailabilityState` | `PlayerJumpAvailabilityStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerJumpExecutionState` | `PlayerJumpExecutionStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerJumpMobilityModifiers` | `PlayerJumpMobilityModifiersComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerGrappleAndRocketState` | `PlayerGrappleAndRocketStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |

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
- 本分区父级局部统计：字段 112；属性 0；合计 112；完整父级统计以源报告为准。

#### 4.13.5 细分子系统：`PlayerBeetleArmorState`

- 细分职责：甲虫套装球体、计数、攻防和动画状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；甲虫套装系统集中写入。
- 成员文件数：1；声明类型数：1；字段：10；属性：0；合计：10。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（10）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 488 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 565 | 2 | beetleOrbs | int | `public int beetleOrbs;` | `public int beetleOrbs;` |
| 489 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 567 | 2 | beetleCounter | float | `public float beetleCounter;` | `public float beetleCounter;` |
| 490 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 569 | 2 | beetleCountdown | int | `public int beetleCountdown;` | `public int beetleCountdown;` |
| 491 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 571 | 2 | beetleDefense | bool | `public bool beetleDefense;` | `public bool beetleDefense;` |
| 492 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 573 | 2 | beetleOffense | bool | `public bool beetleOffense;` | `public bool beetleOffense;` |
| 493 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 575 | 2 | beetleBuff | bool | `public bool beetleBuff;` | `public bool beetleBuff;` |
| 512 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 614 | 2 | beetlePos | Vector2[] | `public Vector2[] beetlePos = new Vector2[3];` | `public Vector2[] beetlePos = new Vector2[3];` |
| 513 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 616 | 2 | beetleVel | Vector2[] | `public Vector2[] beetleVel = new Vector2[3];` | `public Vector2[] beetleVel = new Vector2[3];` |
| 514 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 618 | 2 | beetleFrame | int | `public int beetleFrame;` | `public int beetleFrame;` |
| 515 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 620 | 2 | beetleFrameCounter | int | `public int beetleFrameCounter;` | `public int beetleFrameCounter;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.6 细分子系统：`PlayerSolarAndNebulaArmorState`

- 细分职责：日耀护盾和星云资源层级状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；套装效果按战斗事件提交。
- 成员文件数：1；声明类型数：1；字段：10；属性：0；合计：10。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（10）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 494 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 577 | 2 | solarShields | int | `public int solarShields;` | `public int solarShields;` |
| 495 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 579 | 2 | solarCounter | int | `public int solarCounter;` | `public int solarCounter;` |
| 496 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 582 | 2 | solarShieldPos | Vector2[] | `public Vector2[] solarShieldPos = new Vector2[3];` | `public Vector2[] solarShieldPos = new Vector2[3];` |
| 497 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 584 | 2 | solarShieldVel | Vector2[] | `public Vector2[] solarShieldVel = new Vector2[3];` | `public Vector2[] solarShieldVel = new Vector2[3];` |
| 498 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 586 | 2 | solarDashing | bool | `public bool solarDashing;` | `public bool solarDashing;` |
| 499 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 588 | 2 | solarDashConsumedFlare | bool | `public bool solarDashConsumedFlare;` | `public bool solarDashConsumedFlare;` |
| 500 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 590 | 2 | nebulaLevelLife | int | `public int nebulaLevelLife;` | `public int nebulaLevelLife;` |
| 501 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 592 | 2 | nebulaLevelMana | int | `public int nebulaLevelMana;` | `public int nebulaLevelMana;` |
| 502 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 594 | 2 | nebulaManaCounter | int | `public int nebulaManaCounter;` | `public int nebulaManaCounter;` |
| 503 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 596 | 2 | nebulaLevelDamage | int | `public int nebulaLevelDamage;` | `public int nebulaLevelDamage;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.7 细分子系统：`PlayerMagnetAndUtilityAccessoryState`

- 细分职责：磁力、生命力、工具速度和特殊配饰效果状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；通用配饰效果集中提交。
- 成员文件数：1；声明类型数：1；字段：8；属性：0；合计：8。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（8）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 504 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 598 | 2 | manaMagnet | bool | `public bool manaMagnet;` | `public bool manaMagnet;` |
| 505 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 600 | 2 | lifeMagnet | bool | `public bool lifeMagnet;` | `public bool lifeMagnet;` |
| 506 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 602 | 2 | treasureMagnet | bool | `public bool treasureMagnet;` | `public bool treasureMagnet;` |
| 507 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 604 | 2 | chiselSpeed | bool | `public bool chiselSpeed;` | `public bool chiselSpeed;` |
| 508 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 606 | 2 | lifeForce | bool | `public bool lifeForce;` | `public bool lifeForce;` |
| 509 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 608 | 2 | hasDeadCellsDownDash | bool | `public bool hasDeadCellsDownDash;` | `public bool hasDeadCellsDownDash;` |
| 510 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 610 | 2 | calmed | bool | `public bool calmed;` | `public bool calmed;` |
| 511 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 612 | 2 | inferno | bool | `public bool inferno;` | `public bool inferno;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.9 细分子系统：`PlayerDashAndGroundTraversalState`

- 细分职责：冲刺、落阶、斜坡和地表加速状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；移动转换通过显式阶段更新。
- 成员文件数：1；声明类型数：1；字段：12；属性：0；合计：12。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（12）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 524 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 638 | 2 | stairFall | bool | `public bool stairFall;` | `public bool stairFall;` |
| 525 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 640 | 2 | outOfRange | bool | `public bool outOfRange;` | `public bool outOfRange;` |
| 532 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 654 | 2 | sloping | bool | `public bool sloping;` | `public bool sloping;` |
| 544 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 678 | 2 | dashType | int | `public int dashType;` | `public int dashType;` |
| 545 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 680 | 2 | dash | int | `public int dash;` | `public int dash;` |
| 546 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 682 | 2 | dashTime | int | `public int dashTime;` | `public int dashTime;` |
| 547 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 684 | 2 | timeSinceLastDashStarted | int | `public int timeSinceLastDashStarted;` | `public int timeSinceLastDashStarted;` |
| 548 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 686 | 2 | dashDelay | int | `public int dashDelay;` | `public int dashDelay;` |
| 551 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 692 | 2 | accRunSpeed | float | `public float accRunSpeed;` | `public float accRunSpeed;` |
| 582 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 755 | 2 | powerrun | bool | `public bool powerrun;` | `public bool powerrun;` |
| 583 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 757 | 2 | runningOnSand | bool | `public bool runningOnSand;` | `public bool runningOnSand;` |
| 584 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 759 | 2 | flapSound | bool | `public bool flapSound;` | `public bool flapSound;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.10 细分子系统：`PlayerRopeAndPulleyState`

- 细分职责：绳索、宝石钩、滑轮和绳索附加效果状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；抓取和滑轮输入通过移动命令交接。
- 成员文件数：1；声明类型数：1；字段：10；属性：0；合计：10。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（10）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 541 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 672 | 2 | ropeCount | int | `public int ropeCount;` | `public int ropeCount;` |
| 552 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 694 | 2 | cordage | bool | `public bool cordage;` | `public bool cordage;` |
| 553 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 696 | 2 | gem | int | `public int gem = -1;` | `public int gem = -1;` |
| 554 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 698 | 2 | gemCount | int | `public int gemCount;` | `public int gemCount;` |
| 555 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 700 | 2 | ownedLargeGems | Terraria.BitsByte | `public BitsByte ownedLargeGems;` | `public BitsByte ownedLargeGems;` |
| 556 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 702 | 2 | meleeEnchant | byte | `public byte meleeEnchant;` | `public byte meleeEnchant;` |
| 557 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 704 | 2 | pulleyDir | byte | `public byte pulleyDir;` | `public byte pulleyDir;` |
| 558 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 706 | 2 | pulley | bool | `public bool pulley;` | `public bool pulley;` |
| 559 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 708 | 2 | pulleyFrame | int | `public int pulleyFrame;` | `public int pulleyFrame;` |
| 560 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 710 | 2 | pulleyFrameCounter | float | `public float pulleyFrameCounter;` | `public float pulleyFrameCounter;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.11 细分子系统：`PlayerSlideAndCarpetTraversalState`

- 细分职责：滑行、冰鞋、飞毯和地表滑移状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；滑移状态由移动阶段统一提交。
- 成员文件数：1；声明类型数：1；字段：10；属性：0；合计：10。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（10）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 562 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 714 | 2 | sliding | bool | `public bool sliding;` | `public bool sliding;` |
| 563 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 716 | 2 | slideDir | int | `public int slideDir;` | `public int slideDir;` |
| 564 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 718 | 2 | snowBallLauncherInteractionCooldown | int | `public int snowBallLauncherInteractionCooldown;` | `public int snowBallLauncherInteractionCooldown;` |
| 565 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 720 | 2 | iceSkate | bool | `public bool iceSkate;` | `public bool iceSkate;` |
| 566 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 722 | 2 | carpet | bool | `public bool carpet;` | `public bool carpet;` |
| 567 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 724 | 2 | spikedBoots | int | `public int spikedBoots;` | `public int spikedBoots;` |
| 568 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 726 | 2 | carpetFrame | int | `public int carpetFrame = -1;` | `public int carpetFrame = -1;` |
| 569 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 728 | 2 | carpetFrameCounter | float | `public float carpetFrameCounter;` | `public float carpetFrameCounter;` |
| 570 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 730 | 2 | canCarpet | bool | `public bool canCarpet;` | `public bool canCarpet;` |
| 571 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 732 | 2 | carpetTime | int | `public int carpetTime;` | `public int carpetTime;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.23 细分子系统：`PlayerWingsAndFlightState`

- 细分职责：翅膀飞行时间、飞行帧和飞行能力状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；飞行阶段集中提交翅膀运行状态。
- 成员文件数：1；声明类型数：1；字段：6；属性：0；合计：6。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（6）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 650 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 892 | 2 | wingTime | float | `public float wingTime;` | `public float wingTime;` |
| 651 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 894 | 2 | wings | int | `public int wings;` | `public int wings;` |
| 652 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 896 | 2 | wingsLogic | int | `public int wingsLogic;` | `public int wingsLogic;` |
| 653 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 898 | 2 | wingTimeMax | int | `public int wingTimeMax;` | `public int wingTimeMax;` |
| 654 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 900 | 2 | wingFrame | int | `public int wingFrame;` | `public int wingFrame;` |
| 655 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 902 | 2 | wingFrameCounter | int | `public int wingFrameCounter;` | `public int wingFrameCounter;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.74 细分子系统：`PlayerJumpAvailabilityState`

- 细分职责：各类额外跳跃的资格和可再次跳跃窗口。
- 边界角色：`authoritative state/behavior`；最小 seam：Jump System/CommitPort；资格 Query 只读消费。
- 成员文件数：1；声明类型数：1；字段：18；属性：0；合计：18。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（18）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1208 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2060 | 2 | hasJumpOption_Cloud | bool | `public bool hasJumpOption_Cloud;` | `public bool hasJumpOption_Cloud;` |
| 1209 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2062 | 2 | canJumpAgain_Cloud | bool | `public bool canJumpAgain_Cloud;` | `public bool canJumpAgain_Cloud;` |
| 1211 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2066 | 2 | hasJumpOption_Sandstorm | bool | `public bool hasJumpOption_Sandstorm;` | `public bool hasJumpOption_Sandstorm;` |
| 1212 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2068 | 2 | canJumpAgain_Sandstorm | bool | `public bool canJumpAgain_Sandstorm;` | `public bool canJumpAgain_Sandstorm;` |
| 1214 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2072 | 2 | hasJumpOption_Blizzard | bool | `public bool hasJumpOption_Blizzard;` | `public bool hasJumpOption_Blizzard;` |
| 1215 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2074 | 2 | canJumpAgain_Blizzard | bool | `public bool canJumpAgain_Blizzard;` | `public bool canJumpAgain_Blizzard;` |
| 1217 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2078 | 2 | hasJumpOption_Fart | bool | `public bool hasJumpOption_Fart;` | `public bool hasJumpOption_Fart;` |
| 1218 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2080 | 2 | canJumpAgain_Fart | bool | `public bool canJumpAgain_Fart;` | `public bool canJumpAgain_Fart;` |
| 1220 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2084 | 2 | hasJumpOption_Sail | bool | `public bool hasJumpOption_Sail;` | `public bool hasJumpOption_Sail;` |
| 1221 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2086 | 2 | canJumpAgain_Sail | bool | `public bool canJumpAgain_Sail;` | `public bool canJumpAgain_Sail;` |
| 1223 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2090 | 2 | hasJumpOption_Unicorn | bool | `public bool hasJumpOption_Unicorn;` | `public bool hasJumpOption_Unicorn;` |
| 1224 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2092 | 2 | canJumpAgain_Unicorn | bool | `public bool canJumpAgain_Unicorn;` | `public bool canJumpAgain_Unicorn;` |
| 1226 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2096 | 2 | hasJumpOption_Santank | bool | `public bool hasJumpOption_Santank;` | `public bool hasJumpOption_Santank;` |
| 1227 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2098 | 2 | canJumpAgain_Santank | bool | `public bool canJumpAgain_Santank;` | `public bool canJumpAgain_Santank;` |
| 1229 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2102 | 2 | hasJumpOption_WallOfFleshGoat | bool | `public bool hasJumpOption_WallOfFleshGoat;` | `public bool hasJumpOption_WallOfFleshGoat;` |
| 1230 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2104 | 2 | canJumpAgain_WallOfFleshGoat | bool | `public bool canJumpAgain_WallOfFleshGoat;` | `public bool canJumpAgain_WallOfFleshGoat;` |
| 1232 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2108 | 2 | hasJumpOption_Basilisk | bool | `public bool hasJumpOption_Basilisk;` | `public bool hasJumpOption_Basilisk;` |
| 1233 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2110 | 2 | canJumpAgain_Basilisk | bool | `public bool canJumpAgain_Basilisk;` | `public bool canJumpAgain_Basilisk;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.75 细分子系统：`PlayerJumpExecutionState`

- 细分职责：额外跳跃、下冲和弹簧跳跃的执行状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Jump System/CommitPort；跳跃阶段按显式顺序提交。
- 成员文件数：1；声明类型数：1；字段：11；属性：0；合计：11。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（11）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1206 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2056 | 2 | isPerformingJump_DownDash | bool | `public bool isPerformingJump_DownDash;` | `public bool isPerformingJump_DownDash;` |
| 1210 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2064 | 2 | isPerformingJump_Cloud | bool | `public bool isPerformingJump_Cloud;` | `public bool isPerformingJump_Cloud;` |
| 1213 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2070 | 2 | isPerformingJump_Sandstorm | bool | `public bool isPerformingJump_Sandstorm;` | `public bool isPerformingJump_Sandstorm;` |
| 1216 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2076 | 2 | isPerformingJump_Blizzard | bool | `public bool isPerformingJump_Blizzard;` | `public bool isPerformingJump_Blizzard;` |
| 1219 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2082 | 2 | isPerformingJump_Fart | bool | `public bool isPerformingJump_Fart;` | `public bool isPerformingJump_Fart;` |
| 1222 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2088 | 2 | isPerformingJump_Sail | bool | `public bool isPerformingJump_Sail;` | `public bool isPerformingJump_Sail;` |
| 1225 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2094 | 2 | isPerformingJump_Unicorn | bool | `public bool isPerformingJump_Unicorn;` | `public bool isPerformingJump_Unicorn;` |
| 1228 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2100 | 2 | isPerformingJump_Santank | bool | `public bool isPerformingJump_Santank;` | `public bool isPerformingJump_Santank;` |
| 1231 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2106 | 2 | isPerformingJump_WallOfFleshGoat | bool | `public bool isPerformingJump_WallOfFleshGoat;` | `public bool isPerformingJump_WallOfFleshGoat;` |
| 1234 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2112 | 2 | isPerformingJump_Basilisk | bool | `public bool isPerformingJump_Basilisk;` | `public bool isPerformingJump_Basilisk;` |
| 1235 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2114 | 2 | isPerformingPogostickTricks | bool | `public bool isPerformingPogostickTricks;` | `public bool isPerformingPogostickTricks;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.76 细分子系统：`PlayerJumpMobilityModifiers`

- 细分职责：自动跳跃、最近跳跃、速度加成、额外下落和下冲计时。
- 边界角色：`authoritative state/behavior`；最小 seam：Movement System/CommitPort；移动阶段集中写入修正。
- 成员文件数：1；声明类型数：1；字段：5；属性：0；合计：5。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（5）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1207 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2058 | 2 | downDashTime | int | `public int downDashTime;` | `public int downDashTime;` |
| 1236 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2116 | 2 | autoJump | bool | `public bool autoJump;` | `public bool autoJump;` |
| 1237 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2118 | 2 | justJumped | bool | `public bool justJumped;` | `public bool justJumped;` |
| 1238 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2120 | 2 | jumpSpeedBoost | float | `public float jumpSpeedBoost;` | `public float jumpSpeedBoost;` |
| 1239 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2122 | 2 | extraFall | int | `public int extraFall;` | `public int extraFall;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.77 细分子系统：`PlayerGrappleAndRocketState`

- 细分职责：抓钩索引、火箭靴计时和火箭释放状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；装备输入通过显式命令进入。
- 成员文件数：1；声明类型数：1；字段：12；属性：0；合计：12。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（12）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1246 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2136 | 2 | grappling | int[] | `public int[] grappling = new int[20];` | `public int[] grappling = new int[20];` |
| 1247 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2138 | 2 | grapCount | int | `public int grapCount;` | `public int grapCount;` |
| 1248 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2140 | 2 | rocketTime | int | `public int rocketTime;` | `public int rocketTime;` |
| 1249 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2142 | 2 | rocketTimeMax | int | `public int rocketTimeMax = 7;` | `public int rocketTimeMax = 7;` |
| 1250 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2144 | 2 | rocketDelay | int | `public int rocketDelay;` | `public int rocketDelay;` |
| 1251 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2146 | 2 | rocketDelay2 | int | `public int rocketDelay2;` | `public int rocketDelay2;` |
| 1252 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2148 | 2 | rocketSoundDelay | int | `public int rocketSoundDelay;` | `public int rocketSoundDelay;` |
| 1253 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2150 | 2 | rocketRelease | bool | `public bool rocketRelease;` | `public bool rocketRelease;` |
| 1254 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2152 | 2 | rocketFrame | bool | `public bool rocketFrame;` | `public bool rocketFrame;` |
| 1255 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2154 | 2 | rocketBoots | int | `public int rocketBoots;` | `public int rocketBoots;` |
| 1256 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2156 | 2 | vanityRocketBoots | int | `public int vanityRocketBoots;` | `public int vanityRocketBoots;` |
| 1257 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2158 | 2 | canRocket | bool | `public bool canRocket;` | `public bool canRocket;` |

##### 属性（0）

无该类型成员记录。


## 8. 本分区自检

- 叶子子系统：11 个；指定组均已写入且没有重复分区。
- 字段/属性/成员：112 / 0 / 112。
- 来源序号范围：488..1257；本分区内唯一序号：True。
- 成员声明表逐组从源报告提取，未改写来源序号、类型、路径、行列、成员、C# 类型、成员声明或原始声明。
- 运行时读写者、生命周期、持久化、网络和调度：未验证，保留为迁移前风险。
