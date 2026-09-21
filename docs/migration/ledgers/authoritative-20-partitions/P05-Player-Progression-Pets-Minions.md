# P05 玩家进度、召唤物、宠物与伙伴 - 权威模拟系统成员组件候选分区

> 来源：`docs/migration/ledgers/Version4权威模拟系统字段属性逐成员源码声明-更细子系统拆分-去除ID类文件.md`；本文件只做成员库存分区和候选组件边界记录。
> 这是候选组件拆分台账，不是运行时迁移完成、行为等价、API 兼容、网络闭合或持久化闭合证明。

## 1. 分区边界

本分区包含 17 个叶子子系统，字段 164 条、属性 0 条、成员合计 164 条；每个叶子子系统保持原正式父级归属，不跨分区拆行。

## 2. 分区覆盖概览

| 叶子子系统 | 正式父级 | 字段 | 属性 | 合计 | 源边界角色 |
|---|---|---:|---:|---:|---|
| `PlayerConsumedProgressionFlags` | `PlayerGameplay` | 6 | 0 | 6 | authoritative state/behavior |
| `PlayerFishingCapabilityState` | `PlayerGameplay` | 7 | 0 | 7 | authoritative state/behavior |
| `PlayerMinionCapacityState` | `PlayerGameplay` | 3 | 0 | 3 | authoritative state/behavior |
| `PlayerCoreMinionSummonFlags` | `PlayerGameplay` | 22 | 0 | 22 | authoritative state/behavior |
| `PlayerCrossoverMinionSummonFlags` | `PlayerGameplay` | 3 | 0 | 3 | authoritative state/behavior |
| `PlayerMinionDamageTrackingState` | `PlayerGameplay` | 2 | 0 | 2 | authoritative state/behavior |
| `PlayerUnlockProgressionState` | `PlayerGameplay` | 4 | 0 | 4 | authoritative state/behavior |
| `PlayerQuestAndEventCounters` | `PlayerGameplay` | 3 | 0 | 3 | authoritative state/behavior |
| `PlayerLegacyPetState` | `PlayerGameplay` | 21 | 0 | 21 | authoritative state/behavior |
| `PlayerBossPetFlags` | `PlayerGameplay` | 16 | 0 | 16 | authoritative state/behavior |
| `PlayerSeasonalAndEventPetFlags` | `PlayerGameplay` | 9 | 0 | 9 | authoritative state/behavior |
| `PlayerStandardNamedPetFlags` | `PlayerGameplay` | 13 | 0 | 13 | authoritative state/behavior |
| `PlayerCrossoverPetFlags` | `PlayerGameplay` | 13 | 0 | 13 | authoritative state/behavior |
| `PlayerWorldObjectPetFlags` | `PlayerGameplay` | 4 | 0 | 4 | authoritative state/behavior |
| `PlayerCompanionState` | `PlayerGameplay` | 14 | 0 | 14 | authoritative state/behavior |
| `PlayerMountAndMinecartEffects` | `PlayerGameplay` | 7 | 0 | 7 | authoritative state/behavior |
| `PlayerAccessoryProgressionEffects` | `PlayerGameplay` | 17 | 0 | 17 | authoritative state/behavior |

## 3. 候选组件归属与 seam

| 叶子子系统 | 候选组件边界 | 类型分类 | 最小 seam | 独立生命周期 | 证据状态 | 主要风险 |
|---|---|---|---|---|---|---|
| `PlayerConsumedProgressionFlags` | `PlayerConsumedProgressionFlagsComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerFishingCapabilityState` | `PlayerFishingCapabilityStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerMinionCapacityState` | `PlayerMinionCapacityStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerCoreMinionSummonFlags` | `PlayerCoreMinionSummonFlagsComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerCrossoverMinionSummonFlags` | `PlayerCrossoverMinionSummonFlagsComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerMinionDamageTrackingState` | `PlayerMinionDamageTrackingStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerUnlockProgressionState` | `PlayerUnlockProgressionStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerQuestAndEventCounters` | `PlayerQuestAndEventCountersComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerLegacyPetState` | `PlayerLegacyPetStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerBossPetFlags` | `PlayerBossPetFlagsComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerSeasonalAndEventPetFlags` | `PlayerSeasonalAndEventPetFlagsComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerStandardNamedPetFlags` | `PlayerStandardNamedPetFlagsComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerCrossoverPetFlags` | `PlayerCrossoverPetFlagsComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerWorldObjectPetFlags` | `PlayerWorldObjectPetFlagsComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerCompanionState` | `PlayerCompanionStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerMountAndMinecartEffects` | `PlayerMountAndMinecartEffectsComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerAccessoryProgressionEffects` | `PlayerAccessoryProgressionEffectsComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |

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
- 本分区父级局部统计：字段 164；属性 0；合计 164；完整父级统计以源报告为准。

#### 4.13.3 细分子系统：`PlayerConsumedProgressionFlags`

- 细分职责：一次性世界物品和进度消耗旗标。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；进度命令是唯一写入方向。
- 成员文件数：1；声明类型数：1；字段：6；属性：0；合计：6。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（6）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 468 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 525 | 2 | usedAegisCrystal | bool | `public bool usedAegisCrystal;` | `public bool usedAegisCrystal;` |
| 469 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 527 | 2 | usedAegisFruit | bool | `public bool usedAegisFruit;` | `public bool usedAegisFruit;` |
| 470 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 529 | 2 | usedArcaneCrystal | bool | `public bool usedArcaneCrystal;` | `public bool usedArcaneCrystal;` |
| 471 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 531 | 2 | usedGalaxyPearl | bool | `public bool usedGalaxyPearl;` | `public bool usedGalaxyPearl;` |
| 472 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 533 | 2 | usedGummyWorm | bool | `public bool usedGummyWorm;` | `public bool usedGummyWorm;` |
| 473 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 535 | 2 | usedAmbrosia | bool | `public bool usedAmbrosia;` | `public bool usedAmbrosia;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.18 细分子系统：`PlayerFishingCapabilityState`

- 细分职责：钓鱼能力、鱼饵辅助和特殊钓鱼配饰状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；钓鱼尝试读取能力快照并提交结果。
- 成员文件数：1；声明类型数：1；字段：7；属性：0；合计：7。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（7）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 613 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 818 | 2 | fishingSkill | int | `public int fishingSkill;` | `public int fishingSkill;` |
| 614 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 820 | 2 | cratePotion | bool | `public bool cratePotion;` | `public bool cratePotion;` |
| 615 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 822 | 2 | sonarPotion | bool | `public bool sonarPotion;` | `public bool sonarPotion;` |
| 616 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 824 | 2 | accFishingLine | bool | `public bool accFishingLine;` | `public bool accFishingLine;` |
| 617 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 826 | 2 | accFishingBobber | bool | `public bool accFishingBobber;` | `public bool accFishingBobber;` |
| 618 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 828 | 2 | accTackleBox | bool | `public bool accTackleBox;` | `public bool accTackleBox;` |
| 619 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 830 | 2 | accLavaFishing | bool | `public bool accLavaFishing;` | `public bool accLavaFishing;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.19 细分子系统：`PlayerMinionCapacityState`

- 细分职责：玩家召唤栏位上限、当前召唤数和分数槽容量。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；召唤容量由装备/效果命令更新。
- 成员文件数：1；声明类型数：1；字段：3；属性：0；合计：3。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（3）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 620 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 832 | 2 | maxMinions | int | `public int maxMinions = 1;` | `public int maxMinions = 1;` |
| 621 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 834 | 2 | numMinions | int | `public int numMinions;` | `public int numMinions;` |
| 622 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 836 | 2 | slotsMinions | float | `public float slotsMinions;` | `public float slotsMinions;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.20 细分子系统：`PlayerCoreMinionSummonFlags`

- 细分职责：核心 Terraria 召唤物和召唤物旗标。
- 边界角色：`authoritative state/behavior`；最小 seam：Minion System/CommitPort；召唤物 Buff 事件集中写入。
- 成员文件数：1；声明类型数：1；字段：22；属性：0；合计：22。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（22）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 623 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 838 | 2 | pygmy | bool | `public bool pygmy;` | `public bool pygmy;` |
| 624 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 840 | 2 | raven | bool | `public bool raven;` | `public bool raven;` |
| 625 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 842 | 2 | slime | bool | `public bool slime;` | `public bool slime;` |
| 626 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 844 | 2 | hornetMinion | bool | `public bool hornetMinion;` | `public bool hornetMinion;` |
| 627 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 846 | 2 | impMinion | bool | `public bool impMinion;` | `public bool impMinion;` |
| 628 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 848 | 2 | twinsMinion | bool | `public bool twinsMinion;` | `public bool twinsMinion;` |
| 629 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 850 | 2 | spiderMinion | bool | `public bool spiderMinion;` | `public bool spiderMinion;` |
| 630 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 852 | 2 | pirateMinion | bool | `public bool pirateMinion;` | `public bool pirateMinion;` |
| 631 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 854 | 2 | sharknadoMinion | bool | `public bool sharknadoMinion;` | `public bool sharknadoMinion;` |
| 632 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 856 | 2 | UFOMinion | bool | `public bool UFOMinion;` | `public bool UFOMinion;` |
| 633 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 858 | 2 | DeadlySphereMinion | bool | `public bool DeadlySphereMinion;` | `public bool DeadlySphereMinion;` |
| 634 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 860 | 2 | stardustMinion | bool | `public bool stardustMinion;` | `public bool stardustMinion;` |
| 635 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 862 | 2 | stardustGuardian | bool | `public bool stardustGuardian;` | `public bool stardustGuardian;` |
| 636 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 864 | 2 | stardustDragon | bool | `public bool stardustDragon;` | `public bool stardustDragon;` |
| 637 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 866 | 2 | batsOfLight | bool | `public bool batsOfLight;` | `public bool batsOfLight;` |
| 638 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 868 | 2 | babyBird | bool | `public bool babyBird;` | `public bool babyBird;` |
| 639 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 870 | 2 | vampireFrog | bool | `public bool vampireFrog;` | `public bool vampireFrog;` |
| 640 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 872 | 2 | stormTiger | bool | `public bool stormTiger;` | `public bool stormTiger;` |
| 642 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 876 | 2 | smolstar | bool | `public bool smolstar;` | `public bool smolstar;` |
| 643 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 878 | 2 | empressBlade | bool | `public bool empressBlade;` | `public bool empressBlade;` |
| 644 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 880 | 2 | flinxMinion | bool | `public bool flinxMinion;` | `public bool flinxMinion;` |
| 645 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 882 | 2 | abigailMinion | bool | `public bool abigailMinion;` | `public bool abigailMinion;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.21 细分子系统：`PlayerCrossoverMinionSummonFlags`

- 细分职责：联动召唤物旗标。
- 边界角色：`authoritative state/behavior`；最小 seam：Crossover Minion System/CommitPort；联动召唤物状态单向提交。
- 成员文件数：1；声明类型数：1；字段：3；属性：0；合计：3。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（3）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 647 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 886 | 2 | deadCellsMushroomBoiMinion | bool | `public bool deadCellsMushroomBoiMinion;` | `public bool deadCellsMushroomBoiMinion;` |
| 648 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 888 | 2 | palworldCattivaMinion | bool | `public bool palworldCattivaMinion;` | `public bool palworldCattivaMinion;` |
| 649 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 890 | 2 | palworldFoxsparksMinion | bool | `public bool palworldFoxsparksMinion;` | `public bool palworldFoxsparksMinion;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.22 细分子系统：`PlayerMinionDamageTrackingState`

- 细分职责：Storm Tiger 和 Abigail 召唤物的原始伤害追踪值。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；召唤命中事件更新追踪值。
- 成员文件数：1；声明类型数：1；字段：2；属性：0；合计：2。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（2）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 641 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 874 | 2 | highestStormTigerGemOriginalDamage | int | `public int highestStormTigerGemOriginalDamage;` | `public int highestStormTigerGemOriginalDamage;` |
| 646 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 884 | 2 | highestAbigailCounterOriginalDamage | int | `public int highestAbigailCounterOriginalDamage;` | `public int highestAbigailCounterOriginalDamage;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.45 细分子系统：`PlayerUnlockProgressionState`

- 细分职责：玩家世界解锁、配方进度和超级矿车启用状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；解锁命令只写入本组。
- 成员文件数：1；声明类型数：1；字段：4；属性：0；合计：4。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（4）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 923 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1475 | 2 | unlockedBiomeTorches | bool | `public bool unlockedBiomeTorches;` | `public bool unlockedBiomeTorches;` |
| 924 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1477 | 2 | ateArtisanBread | bool | `public bool ateArtisanBread;` | `public bool ateArtisanBread;` |
| 925 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1479 | 2 | unlockedSuperCart | bool | `public bool unlockedSuperCart;` | `public bool unlockedSuperCart;` |
| 926 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1481 | 2 | enabledSuperCart | bool | `public bool enabledSuperCart = true;` | `public bool enabledSuperCart = true;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.40 细分子系统：`PlayerQuestAndEventCounters`

- 细分职责：钓鱼任务、建筑者积分和事件进度计数。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；任务和事件完成通过显式事件提交。
- 成员文件数：1；声明类型数：1；字段：3；属性：0；合计：3。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（3）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 858 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1344 | 2 | anglerQuestsFinished | int | `public int anglerQuestsFinished;` | `public int anglerQuestsFinished;` |
| 859 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1346 | 2 | golferScoreAccumulated | int | `public int golferScoreAccumulated;` | `public int golferScoreAccumulated;` |
| 860 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1349 | 2 | downedDD2EventAnyDifficulty | bool | `public bool downedDD2EventAnyDifficulty;` | `public bool downedDD2EventAnyDifficulty;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.46 细分子系统：`PlayerLegacyPetState`

- 细分职责：传统宠物、宠物增益和早期同伴状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；宠物选择和清理通过显式生命周期命令交接。
- 成员文件数：1；声明类型数：1；字段：21；属性：0；合计：21。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（21）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 927 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1483 | 2 | suspiciouslookingTentacle | bool | `public bool suspiciouslookingTentacle;` | `public bool suspiciouslookingTentacle;` |
| 928 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1485 | 2 | crimsonHeart | bool | `public bool crimsonHeart;` | `public bool crimsonHeart;` |
| 929 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1487 | 2 | lightOrb | bool | `public bool lightOrb;` | `public bool lightOrb;` |
| 930 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1489 | 2 | blueFairy | bool | `public bool blueFairy;` | `public bool blueFairy;` |
| 931 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1491 | 2 | redFairy | bool | `public bool redFairy;` | `public bool redFairy;` |
| 932 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1493 | 2 | greenFairy | bool | `public bool greenFairy;` | `public bool greenFairy;` |
| 933 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1495 | 2 | bunny | bool | `public bool bunny;` | `public bool bunny;` |
| 934 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1497 | 2 | turtle | bool | `public bool turtle;` | `public bool turtle;` |
| 935 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1499 | 2 | eater | bool | `public bool eater;` | `public bool eater;` |
| 936 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1501 | 2 | penguin | bool | `public bool penguin;` | `public bool penguin;` |
| 937 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1503 | 2 | HasGardenGnomeNearby | bool | `public bool HasGardenGnomeNearby;` | `public bool HasGardenGnomeNearby;` |
| 939 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1508 | 2 | magicLantern | bool | `public bool magicLantern;` | `public bool magicLantern;` |
| 940 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1510 | 2 | rabid | bool | `public bool rabid;` | `public bool rabid;` |
| 941 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1512 | 2 | sunflower | bool | `public bool sunflower;` | `public bool sunflower;` |
| 942 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1514 | 2 | wellFed | bool | `public bool wellFed;` | `public bool wellFed;` |
| 943 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1516 | 2 | puppy | bool | `public bool puppy;` | `public bool puppy;` |
| 944 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1518 | 2 | grinch | bool | `public bool grinch;` | `public bool grinch;` |
| 945 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1520 | 2 | miniMinotaur | bool | `public bool miniMinotaur;` | `public bool miniMinotaur;` |
| 962 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1554 | 2 | blackCat | bool | `public bool blackCat;` | `public bool blackCat;` |
| 963 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1556 | 2 | spider | bool | `public bool spider;` | `public bool spider;` |
| 964 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1558 | 2 | squashling | bool | `public bool squashling;` | `public bool squashling;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.47 细分子系统：`PlayerBossPetFlags`

- 细分职责：Boss 宠物旗标及其宠物生成状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Pet System/CommitPort；Boss 宠物 Buff 生命周期集中写入。
- 成员文件数：1；声明类型数：1；字段：16；属性：0；合计：16。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（16）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 981 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1592 | 2 | petFlagKingSlimePet | bool | `public bool petFlagKingSlimePet;` | `public bool petFlagKingSlimePet;` |
| 982 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1594 | 2 | petFlagEyeOfCthulhuPet | bool | `public bool petFlagEyeOfCthulhuPet;` | `public bool petFlagEyeOfCthulhuPet;` |
| 983 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1596 | 2 | petFlagEaterOfWorldsPet | bool | `public bool petFlagEaterOfWorldsPet;` | `public bool petFlagEaterOfWorldsPet;` |
| 984 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1598 | 2 | petFlagBrainOfCthulhuPet | bool | `public bool petFlagBrainOfCthulhuPet;` | `public bool petFlagBrainOfCthulhuPet;` |
| 985 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1600 | 2 | petFlagSkeletronPet | bool | `public bool petFlagSkeletronPet;` | `public bool petFlagSkeletronPet;` |
| 986 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1602 | 2 | petFlagQueenBeePet | bool | `public bool petFlagQueenBeePet;` | `public bool petFlagQueenBeePet;` |
| 987 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1604 | 2 | petFlagDestroyerPet | bool | `public bool petFlagDestroyerPet;` | `public bool petFlagDestroyerPet;` |
| 988 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1606 | 2 | petFlagTwinsPet | bool | `public bool petFlagTwinsPet;` | `public bool petFlagTwinsPet;` |
| 989 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1608 | 2 | petFlagSkeletronPrimePet | bool | `public bool petFlagSkeletronPrimePet;` | `public bool petFlagSkeletronPrimePet;` |
| 990 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1610 | 2 | petFlagPlanteraPet | bool | `public bool petFlagPlanteraPet;` | `public bool petFlagPlanteraPet;` |
| 991 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1612 | 2 | petFlagGolemPet | bool | `public bool petFlagGolemPet;` | `public bool petFlagGolemPet;` |
| 992 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1614 | 2 | petFlagDukeFishronPet | bool | `public bool petFlagDukeFishronPet;` | `public bool petFlagDukeFishronPet;` |
| 993 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1616 | 2 | petFlagLunaticCultistPet | bool | `public bool petFlagLunaticCultistPet;` | `public bool petFlagLunaticCultistPet;` |
| 994 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1618 | 2 | petFlagMoonLordPet | bool | `public bool petFlagMoonLordPet;` | `public bool petFlagMoonLordPet;` |
| 995 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1620 | 2 | petFlagFairyQueenPet | bool | `public bool petFlagFairyQueenPet;` | `public bool petFlagFairyQueenPet;` |
| 1002 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1634 | 2 | petFlagQueenSlimePet | bool | `public bool petFlagQueenSlimePet;` | `public bool petFlagQueenSlimePet;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.48 细分子系统：`PlayerSeasonalAndEventPetFlags`

- 细分职责：季节事件、Old One Army 和事件宠物旗标。
- 边界角色：`authoritative state/behavior`；最小 seam：Event Pet System/CommitPort；事件 Buff 生命周期集中写入。
- 成员文件数：1；声明类型数：1；字段：9；属性：0；合计：9。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（9）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 965 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1560 | 2 | petFlagDD2Gato | bool | `public bool petFlagDD2Gato;` | `public bool petFlagDD2Gato;` |
| 966 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1562 | 2 | petFlagDD2Ghost | bool | `public bool petFlagDD2Ghost;` | `public bool petFlagDD2Ghost;` |
| 967 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1564 | 2 | petFlagDD2Dragon | bool | `public bool petFlagDD2Dragon;` | `public bool petFlagDD2Dragon;` |
| 996 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1622 | 2 | petFlagPumpkingPet | bool | `public bool petFlagPumpkingPet;` | `public bool petFlagPumpkingPet;` |
| 997 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1624 | 2 | petFlagEverscreamPet | bool | `public bool petFlagEverscreamPet;` | `public bool petFlagEverscreamPet;` |
| 998 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1626 | 2 | petFlagIceQueenPet | bool | `public bool petFlagIceQueenPet;` | `public bool petFlagIceQueenPet;` |
| 999 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1628 | 2 | petFlagMartianPet | bool | `public bool petFlagMartianPet;` | `public bool petFlagMartianPet;` |
| 1000 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1630 | 2 | petFlagDD2OgrePet | bool | `public bool petFlagDD2OgrePet;` | `public bool petFlagDD2OgrePet;` |
| 1001 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1632 | 2 | petFlagDD2BetsyPet | bool | `public bool petFlagDD2BetsyPet;` | `public bool petFlagDD2BetsyPet;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.49 细分子系统：`PlayerStandardNamedPetFlags`

- 细分职责：常规命名宠物和常规召唤物宠物旗标。
- 边界角色：`authoritative state/behavior`；最小 seam：Pet System/CommitPort；常规宠物效果按 Buff 事件提交。
- 成员文件数：1；声明类型数：1；字段：13；属性：0；合计：13。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（13）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 968 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1566 | 2 | petFlagUpbeatStar | bool | `public bool petFlagUpbeatStar;` | `public bool petFlagUpbeatStar;` |
| 969 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1568 | 2 | petFlagSugarGlider | bool | `public bool petFlagSugarGlider;` | `public bool petFlagSugarGlider;` |
| 970 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1570 | 2 | petFlagBabyShark | bool | `public bool petFlagBabyShark;` | `public bool petFlagBabyShark;` |
| 971 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1572 | 2 | petFlagLilHarpy | bool | `public bool petFlagLilHarpy;` | `public bool petFlagLilHarpy;` |
| 972 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1574 | 2 | petFlagFennecFox | bool | `public bool petFlagFennecFox;` | `public bool petFlagFennecFox;` |
| 973 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1576 | 2 | petFlagGlitteryButterfly | bool | `public bool petFlagGlitteryButterfly;` | `public bool petFlagGlitteryButterfly;` |
| 974 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1578 | 2 | petFlagBabyImp | bool | `public bool petFlagBabyImp;` | `public bool petFlagBabyImp;` |
| 975 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1580 | 2 | petFlagBabyRedPanda | bool | `public bool petFlagBabyRedPanda;` | `public bool petFlagBabyRedPanda;` |
| 976 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1582 | 2 | petFlagPlantero | bool | `public bool petFlagPlantero;` | `public bool petFlagPlantero;` |
| 977 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1584 | 2 | petFlagDynamiteKitten | bool | `public bool petFlagDynamiteKitten;` | `public bool petFlagDynamiteKitten;` |
| 978 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1586 | 2 | petFlagBabyWerewolf | bool | `public bool petFlagBabyWerewolf;` | `public bool petFlagBabyWerewolf;` |
| 979 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1588 | 2 | petFlagShadowMimic | bool | `public bool petFlagShadowMimic;` | `public bool petFlagShadowMimic;` |
| 980 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1590 | 2 | petFlagVoltBunny | bool | `public bool petFlagVoltBunny;` | `public bool petFlagVoltBunny;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.50 细分子系统：`PlayerCrossoverPetFlags`

- 细分职责：联动内容和跨游戏宠物旗标。
- 边界角色：`authoritative state/behavior`；最小 seam：Crossover Pet System/CommitPort；联动内容状态单向提交。
- 成员文件数：1；声明类型数：1；字段：13；属性：0；合计：13。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（13）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1003 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1636 | 2 | petFlagBerniePet | bool | `public bool petFlagBerniePet;` | `public bool petFlagBerniePet;` |
| 1004 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1638 | 2 | petFlagGlommerPet | bool | `public bool petFlagGlommerPet;` | `public bool petFlagGlommerPet;` |
| 1005 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1640 | 2 | petFlagDeerclopsPet | bool | `public bool petFlagDeerclopsPet;` | `public bool petFlagDeerclopsPet;` |
| 1006 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1642 | 2 | petFlagPigPet | bool | `public bool petFlagPigPet;` | `public bool petFlagPigPet;` |
| 1007 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1644 | 2 | petFlagChesterPet | bool | `public bool petFlagChesterPet;` | `public bool petFlagChesterPet;` |
| 1008 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1646 | 2 | petFlagJunimoPet | bool | `public bool petFlagJunimoPet;` | `public bool petFlagJunimoPet;` |
| 1009 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1648 | 2 | petFlagBlueChickenPet | bool | `public bool petFlagBlueChickenPet;` | `public bool petFlagBlueChickenPet;` |
| 1010 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1650 | 2 | petFlagSpiffo | bool | `public bool petFlagSpiffo;` | `public bool petFlagSpiffo;` |
| 1011 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1652 | 2 | petFlagCaveling | bool | `public bool petFlagCaveling;` | `public bool petFlagCaveling;` |
| 1015 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1660 | 2 | petFlagDeadCellsSwarmBiter | bool | `public bool petFlagDeadCellsSwarmBiter;` | `public bool petFlagDeadCellsSwarmBiter;` |
| 1016 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1662 | 2 | petFlagPufferfish | bool | `public bool petFlagPufferfish;` | `public bool petFlagPufferfish;` |
| 1018 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1666 | 2 | petFlagChillet | bool | `public bool petFlagChillet;` | `public bool petFlagChillet;` |
| 1019 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1668 | 2 | petFlagChilletIgnis | bool | `public bool petFlagChilletIgnis;` | `public bool petFlagChilletIgnis;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.51 细分子系统：`PlayerWorldObjectPetFlags`

- 细分职责：方块、巨石和特殊世界物件宠物旗标。
- 边界角色：`authoritative state/behavior`；最小 seam：World Object Pet System/CommitPort；世界物件效果集中写入。
- 成员文件数：1；声明类型数：1；字段：4；属性：0；合计：4。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（4）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1012 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1654 | 2 | petFlagDirtiestBlock | bool | `public bool petFlagDirtiestBlock;` | `public bool petFlagDirtiestBlock;` |
| 1013 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1656 | 2 | petFlagBoulderPet | bool | `public bool petFlagBoulderPet;` | `public bool petFlagBoulderPet;` |
| 1014 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1658 | 2 | petFlagRainbowBoulderPet | bool | `public bool petFlagRainbowBoulderPet;` | `public bool petFlagRainbowBoulderPet;` |
| 1017 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1664 | 2 | petFlagAxeFairyPet | bool | `public bool petFlagAxeFairyPet;` | `public bool petFlagAxeFairyPet;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.52 细分子系统：`PlayerCompanionState`

- 细分职责：同伴、坐骑宠物和特殊陪伴实体状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；同伴生成通过显式实体命令提交。
- 成员文件数：1；声明类型数：1；字段：14；属性：0；合计：14。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（14）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1020 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1670 | 2 | companionCube | bool | `public bool companionCube;` | `public bool companionCube;` |
| 1021 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1672 | 2 | babyFaceMonster | bool | `public bool babyFaceMonster;` | `public bool babyFaceMonster;` |
| 1028 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1686 | 2 | snowman | bool | `public bool snowman;` | `public bool snowman;` |
| 1030 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1690 | 2 | dino | bool | `public bool dino;` | `public bool dino;` |
| 1031 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1692 | 2 | skeletron | bool | `public bool skeletron;` | `public bool skeletron;` |
| 1032 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1694 | 2 | hornet | bool | `public bool hornet;` | `public bool hornet;` |
| 1033 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1696 | 2 | zephyrfish | bool | `public bool zephyrfish;` | `public bool zephyrfish;` |
| 1034 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1698 | 2 | tiki | bool | `public bool tiki;` | `public bool tiki;` |
| 1035 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1700 | 2 | parrot | bool | `public bool parrot;` | `public bool parrot;` |
| 1036 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1702 | 2 | truffle | bool | `public bool truffle;` | `public bool truffle;` |
| 1037 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1704 | 2 | sapling | bool | `public bool sapling;` | `public bool sapling;` |
| 1038 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1706 | 2 | cSapling | bool | `public bool cSapling;` | `public bool cSapling;` |
| 1039 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1708 | 2 | wisp | bool | `public bool wisp;` | `public bool wisp;` |
| 1040 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1710 | 2 | lizard | bool | `public bool lizard;` | `public bool lizard;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.53 细分子系统：`PlayerMountAndMinecartEffects`

- 细分职责：玩家坐骑、轨道和矿车运行效果标志。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；坐骑运行输入从 Mount 查询读取。
- 成员文件数：1；声明类型数：1；字段：7；属性：0；合计：7。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（7）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 955 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1540 | 2 | onWrongGround | bool | `public bool onWrongGround;` | `public bool onWrongGround;` |
| 956 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1542 | 2 | onTrack | bool | `public bool onTrack;` | `public bool onTrack;` |
| 957 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1544 | 2 | cartRampTime | int | `public int cartRampTime;` | `public int cartRampTime;` |
| 958 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1546 | 2 | cartFlip | bool | `public bool cartFlip;` | `public bool cartFlip;` |
| 959 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1548 | 2 | trackBoost | float | `public float trackBoost;` | `public float trackBoost;` |
| 960 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1550 | 2 | lastBoost | Vector2 | `public Vector2 lastBoost = Vector2.Zero;` | `public Vector2 lastBoost = Vector2.Zero;` |
| 961 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1552 | 2 | mount | Terraria.Mount | `public Mount mount;` | `public Mount mount;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.54 细分子系统：`PlayerAccessoryProgressionEffects`

- 细分职责：玩家配饰、套装前置和进度效果标志。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；效果计算与进度状态分离。
- 成员文件数：1；声明类型数：1；字段：17；属性：0；合计：17。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（17）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 938 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1505 | 2 | brokenMirrorBadLuck | bool | `public bool brokenMirrorBadLuck;` | `public bool brokenMirrorBadLuck;` |
| 946 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1522 | 2 | flowerBoots | bool | `public bool flowerBoots;` | `public bool flowerBoots;` |
| 947 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1524 | 2 | fairyBoots | bool | `public bool fairyBoots;` | `public bool fairyBoots;` |
| 948 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1526 | 2 | hellfireTreads | bool | `public bool hellfireTreads;` | `public bool hellfireTreads;` |
| 949 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1528 | 2 | moonLordLegs | bool | `public bool moonLordLegs;` | `public bool moonLordLegs;` |
| 950 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1530 | 2 | deadMansSweater | bool | `public bool deadMansSweater;` | `public bool deadMansSweater;` |
| 951 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1532 | 2 | arcticDivingGear | bool | `public bool arcticDivingGear;` | `public bool arcticDivingGear;` |
| 952 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1534 | 2 | coolWhipBuff | bool | `public bool coolWhipBuff;` | `public bool coolWhipBuff;` |
| 953 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1536 | 2 | cobWhipBuff | bool | `public bool cobWhipBuff;` | `public bool cobWhipBuff;` |
| 954 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1538 | 2 | wearsRobe | bool | `public bool wearsRobe;` | `public bool wearsRobe;` |
| 1022 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1674 | 2 | magicCuffs | bool | `public bool magicCuffs;` | `public bool magicCuffs;` |
| 1023 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1676 | 2 | coldDash | bool | `public bool coldDash;` | `public bool coldDash;` |
| 1024 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1678 | 2 | sailDash | bool | `public bool sailDash;` | `public bool sailDash;` |
| 1025 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1680 | 2 | desertDash | bool | `public bool desertDash;` | `public bool desertDash;` |
| 1026 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1682 | 2 | desertBoots | bool | `public bool desertBoots;` | `public bool desertBoots;` |
| 1027 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1684 | 2 | eyeSpring | bool | `public bool eyeSpring;` | `public bool eyeSpring;` |
| 1029 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1688 | 2 | scope | bool | `public bool scope;` | `public bool scope;` |

##### 属性（0）

无该类型成员记录。


## 8. 本分区自检

- 叶子子系统：17 个；指定组均已写入且没有重复分区。
- 字段/属性/成员：164 / 0 / 164。
- 来源序号范围：468..1040；本分区内唯一序号：True。
- 成员声明表逐组从源报告提取，未改写来源序号、类型、路径、行列、成员、C# 类型、成员声明或原始声明。
- 运行时读写者、生命周期、持久化、网络和调度：未验证，保留为迁移前风险。
