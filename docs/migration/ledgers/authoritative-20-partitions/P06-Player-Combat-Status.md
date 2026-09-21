# P06 玩家战斗、伤害、防御、状态与资源 - 权威模拟系统成员组件候选分区

> 来源：`docs/migration/ledgers/Version4权威模拟系统字段属性逐成员源码声明-更细子系统拆分-去除ID类文件.md`；本文件只做成员库存分区和候选组件边界记录。
> 这是候选组件拆分台账，不是运行时迁移完成、行为等价、API 兼容、网络闭合或持久化闭合证明。

## 1. 分区边界

本分区包含 22 个叶子子系统，字段 254 条、属性 0 条、成员合计 254 条；每个叶子子系统保持原正式父级归属，不跨分区拆行。

## 2. 分区覆盖概览

| 叶子子系统 | 正式父级 | 字段 | 属性 | 合计 | 源边界角色 |
|---|---|---:|---:|---:|---|
| `PlayerStringAndAccessoryEffectState` | `PlayerGameplay` | 14 | 0 | 14 | authoritative state/behavior |
| `PlayerCombatDamageProcState` | `PlayerGameplay` | 15 | 0 | 15 | authoritative state/behavior |
| `PlayerCombatDodgeAndImmunityState` | `PlayerGameplay` | 5 | 0 | 5 | authoritative state/behavior |
| `PlayerCombatBarrierAndRegenState` | `PlayerGameplay` | 4 | 0 | 4 | authoritative state/behavior |
| `PlayerManaAndAfkStatus` | `PlayerGameplay` | 10 | 0 | 10 | authoritative state/behavior |
| `PlayerDebuffAndRecoveryStatus` | `PlayerGameplay` | 16 | 0 | 16 | authoritative state/behavior |
| `PlayerDetectionAndCombatStatus` | `PlayerGameplay` | 11 | 0 | 11 | authoritative state/behavior |
| `PlayerSocialAndDefenseState` | `PlayerGameplay` | 20 | 0 | 20 | authoritative state/behavior |
| `PlayerFrameAndImmunityState` | `PlayerGameplay` | 11 | 0 | 11 | authoritative state/behavior |
| `PlayerVitalAndRegenState` | `PlayerGameplay` | 13 | 0 | 13 | authoritative state/behavior |
| `PlayerCombatModifierAndImmunityState` | `PlayerGameplay` | 16 | 0 | 16 | authoritative state/behavior |
| `PlayerAmmoAndAccessoryEffects` | `PlayerGameplay` | 12 | 0 | 12 | authoritative state/behavior |
| `PlayerElementalAndShimmerStatus` | `PlayerGameplay` | 21 | 0 | 21 | authoritative state/behavior |
| `PlayerSurvivalAndTransformationState` | `PlayerGameplay` | 15 | 0 | 15 | authoritative state/behavior |
| `PlayerDebuffStatusState` | `PlayerGameplay` | 8 | 0 | 8 | authoritative state/behavior |
| `PlayerAccessoryCombatModifierState` | `PlayerGameplay` | 9 | 0 | 9 | authoritative state/behavior |
| `PlayerAccessoryResourceAndInvulnerabilityState` | `PlayerGameplay` | 5 | 0 | 5 | authoritative state/behavior |
| `PlayerAccessoryDebuffAndDropState` | `PlayerGameplay` | 8 | 0 | 8 | authoritative state/behavior |
| `PlayerCombatDamageAndCritModifiers` | `PlayerGameplay` | 14 | 0 | 14 | authoritative state/behavior |
| `PlayerCombatSpeedRangeAndPermissionState` | `PlayerGameplay` | 9 | 0 | 9 | authoritative state/behavior |
| `PlayerLuckAndCommerceEffects` | `PlayerGameplay` | 13 | 0 | 13 | authoritative state/behavior |
| `PlayerDpsTelemetryState` | `PlayerGameplay` | 5 | 0 | 5 | authoritative state/behavior |

## 3. 候选组件归属与 seam

| 叶子子系统 | 候选组件边界 | 类型分类 | 最小 seam | 独立生命周期 | 证据状态 | 主要风险 |
|---|---|---|---|---|---|---|
| `PlayerStringAndAccessoryEffectState` | `PlayerStringAndAccessoryEffectStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerCombatDamageProcState` | `PlayerCombatDamageProcStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerCombatDodgeAndImmunityState` | `PlayerCombatDodgeAndImmunityStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerCombatBarrierAndRegenState` | `PlayerCombatBarrierAndRegenStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerManaAndAfkStatus` | `PlayerManaAndAfkStatusComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerDebuffAndRecoveryStatus` | `PlayerDebuffAndRecoveryStatusComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerDetectionAndCombatStatus` | `PlayerDetectionAndCombatStatusComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerSocialAndDefenseState` | `PlayerSocialAndDefenseStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerFrameAndImmunityState` | `PlayerFrameAndImmunityStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerVitalAndRegenState` | `PlayerVitalAndRegenStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerCombatModifierAndImmunityState` | `PlayerCombatModifierAndImmunityStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerAmmoAndAccessoryEffects` | `PlayerAmmoAndAccessoryEffectsComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerElementalAndShimmerStatus` | `PlayerElementalAndShimmerStatusComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerSurvivalAndTransformationState` | `PlayerSurvivalAndTransformationStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerDebuffStatusState` | `PlayerDebuffStatusStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerAccessoryCombatModifierState` | `PlayerAccessoryCombatModifierStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerAccessoryResourceAndInvulnerabilityState` | `PlayerAccessoryResourceAndInvulnerabilityStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerAccessoryDebuffAndDropState` | `PlayerAccessoryDebuffAndDropStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerCombatDamageAndCritModifiers` | `PlayerCombatDamageAndCritModifiersComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerCombatSpeedRangeAndPermissionState` | `PlayerCombatSpeedRangeAndPermissionStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerLuckAndCommerceEffects` | `PlayerLuckAndCommerceEffectsComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerDpsTelemetryState` | `PlayerDpsTelemetryStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |

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
- 本分区父级局部统计：字段 254；属性 0；合计 254；完整父级统计以源报告为准。

#### 4.13.4 细分子系统：`PlayerStringAndAccessoryEffectState`

- 细分职责：绳索、悠悠球、额外配饰和通用配饰效果状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；装备效果通过能力提交。
- 成员文件数：1；声明类型数：1；字段：14；属性：0；合计：14。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（14）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 474 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 537 | 2 | extraAccessorySlots | int | `public int extraAccessorySlots = 2;` | `public int extraAccessorySlots = 2;` |
| 475 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 539 | 2 | extraAccessory | bool | `public bool extraAccessory;` | `public bool extraAccessory;` |
| 476 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 541 | 2 | tankPet | int | `public int tankPet = -1;` | `public int tankPet = -1;` |
| 477 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 543 | 2 | tankPetReset | bool | `public bool tankPetReset;` | `public bool tankPetReset;` |
| 478 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 545 | 2 | stringColor | int | `public int stringColor;` | `public int stringColor;` |
| 479 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 547 | 2 | counterWeight | int | `public int counterWeight;` | `public int counterWeight;` |
| 480 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 549 | 2 | vanityCounterWeight | int | `public int vanityCounterWeight;` | `public int vanityCounterWeight;` |
| 481 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 551 | 2 | magicString | bool | `public bool magicString;` | `public bool magicString;` |
| 482 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 553 | 2 | yoyoString | bool | `public bool yoyoString;` | `public bool yoyoString;` |
| 483 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 555 | 2 | yoyoGlove | bool | `public bool yoyoGlove;` | `public bool yoyoGlove;` |
| 484 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 557 | 2 | rapidAttackBonus | float | `private float rapidAttackBonus;` | `private float rapidAttackBonus;` |
| 485 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 559 | 2 | stressBall | bool | `public bool stressBall;` | `public bool stressBall;` |
| 486 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 561 | 2 | stressBallPrevious | bool | `public bool stressBallPrevious;` | `public bool stressBallPrevious;` |
| 487 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 563 | 2 | staffOfRegrowthBonus | bool | `public bool staffOfRegrowthBonus;` | `public bool staffOfRegrowthBonus;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.12 细分子系统：`PlayerCombatDamageProcState`

- 细分职责：生命窃取、命中触发和伤害反应计时状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Combat Proc System/CommitPort；攻击事件是唯一写入边界。
- 成员文件数：1；声明类型数：1；字段：15；属性：0；合计：15。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（15）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 526 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 642 | 2 | lifeSteal | float | `public float lifeSteal = 99999f;` | `public float lifeSteal = 99999f;` |
| 527 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 644 | 2 | ghostDmg | float | `public float ghostDmg;` | `public float ghostDmg;` |
| 549 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 688 | 2 | eocDash | int | `public int eocDash;` | `public int eocDash;` |
| 550 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 690 | 2 | eocHit | int | `public int eocHit = -1;` | `public int eocHit = -1;` |
| 573 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 736 | 2 | infernoCounter | int | `public int infernoCounter;` | `public int infernoCounter;` |
| 574 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 739 | 2 | starCloakCooldown | int | `public int starCloakCooldown;` | `public int starCloakCooldown;` |
| 603 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 798 | 2 | onHitDodge | bool | `public bool onHitDodge;` | `public bool onHitDodge;` |
| 604 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 800 | 2 | onHitRegen | bool | `public bool onHitRegen;` | `public bool onHitRegen;` |
| 605 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 802 | 2 | onHitPetal | bool | `public bool onHitPetal;` | `public bool onHitPetal;` |
| 606 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 804 | 2 | onHitTitaniumStorm | bool | `public bool onHitTitaniumStorm;` | `public bool onHitTitaniumStorm;` |
| 607 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 806 | 2 | titaniumStormCooldown | int | `public int titaniumStormCooldown;` | `public int titaniumStormCooldown;` |
| 608 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 808 | 2 | hasTitaniumStormBuff | bool | `public bool hasTitaniumStormBuff;` | `public bool hasTitaniumStormBuff;` |
| 609 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 810 | 2 | petalTimer | int | `public int petalTimer;` | `public int petalTimer;` |
| 611 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 814 | 2 | boneGloveTimer | int | `public int boneGloveTimer;` | `public int boneGloveTimer;` |
| 612 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 816 | 2 | phantomPhoneixCounter | int | `public int phantomPhoneixCounter;` | `public int phantomPhoneixCounter;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.13 细分子系统：`PlayerCombatDodgeAndImmunityState`

- 细分职责：黑带、混乱之脑和闪避免疫状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Combat Defense System/CommitPort；闪避事件显式更新。
- 成员文件数：1；声明类型数：1；字段：5；属性：0；合计：5。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（5）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 561 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 712 | 2 | blackBelt | bool | `public bool blackBelt;` | `public bool blackBelt;` |
| 597 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 785 | 2 | brainOfConfusionItem | Terraria.Item | `public Item brainOfConfusionItem;` | `public Item brainOfConfusionItem;` |
| 598 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 787 | 2 | brainOfConfusionDodgeAnimationCounter | int | `public int brainOfConfusionDodgeAnimationCounter;` | `public int brainOfConfusionDodgeAnimationCounter;` |
| 601 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 793 | 2 | shadowDodge | bool | `public bool shadowDodge;` | `public bool shadowDodge;` |
| 610 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 812 | 2 | shadowDodgeTimer | int | `public int shadowDodgeTimer;` | `public int shadowDodgeTimer;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.14 细分子系统：`PlayerCombatBarrierAndRegenState`

- 细分职责：冰障、冰障帧和钯金回复状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Combat Barrier System/CommitPort；护盾与回复按攻击结果提交。
- 成员文件数：1；声明类型数：1；字段：4；属性：0；合计：4。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（4）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 585 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 761 | 2 | iceBarrier | bool | `public bool iceBarrier;` | `public bool iceBarrier;` |
| 599 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 789 | 2 | iceBarrierFrame | byte | `public byte iceBarrierFrame;` | `public byte iceBarrierFrame;` |
| 600 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 791 | 2 | iceBarrierFrameCounter | byte | `public byte iceBarrierFrameCounter;` | `public byte iceBarrierFrameCounter;` |
| 602 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 796 | 2 | palladiumRegen | bool | `public bool palladiumRegen;` | `public bool palladiumRegen;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.15 细分子系统：`PlayerManaAndAfkStatus`

- 细分职责：法力疾病、法力回复和 AFK 计时状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；资源 Tick 负责唯一写入。
- 成员文件数：1；声明类型数：1；字段：10；属性：0；合计：10。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（10）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 516 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 622 | 2 | manaSickTime | int | `public static int manaSickTime = 300;` | `public static int manaSickTime = 300;` |
| 517 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 624 | 2 | manaSickLessDmg | float | `public static float manaSickLessDmg = 0.25f;` | `public static float manaSickLessDmg = 0.25f;` |
| 518 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 626 | 2 | manaSickReduction | float | `public float manaSickReduction;` | `public float manaSickReduction;` |
| 519 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 628 | 2 | manaSick | bool | `public bool manaSick;` | `public bool manaSick;` |
| 520 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 630 | 2 | afkCounter | int | `public int afkCounter;` | `public int afkCounter;` |
| 521 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 632 | 2 | AFKTimeNeededForNoWormSpawns | int | `public static readonly int AFKTimeNeededForNoWormSpawns = 300;` | `public static readonly int AFKTimeNeededForNoWormSpawns = 300;` |
| 522 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 634 | 2 | AFKTimeNeededForNoLuckyStars | int | `public static readonly int AFKTimeNeededForNoLuckyStars = 10800;` | `public static readonly int AFKTimeNeededForNoLuckyStars = 10800;` |
| 523 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 636 | 2 | afkCounterForKiting | int | `public int afkCounterForKiting;` | `public int afkCounterForKiting;` |
| 542 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 674 | 2 | manaRegenBonus | int | `public int manaRegenBonus;` | `public int manaRegenBonus;` |
| 543 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 676 | 2 | manaRegenDelayBonus | float | `public float manaRegenDelayBonus;` | `public float manaRegenDelayBonus;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.16 细分子系统：`PlayerDebuffAndRecoveryStatus`

- 细分职责：冻结、减益、恢复、幽灵和移动阻滞状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；状态事件和计时器显式更新。
- 成员文件数：1；声明类型数：1；字段：16；属性：0；合计：16。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（16）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 533 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 656 | 2 | chilled | bool | `public bool chilled;` | `public bool chilled;` |
| 534 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 658 | 2 | dazed | bool | `public bool dazed;` | `public bool dazed;` |
| 535 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 660 | 2 | frozen | bool | `public bool frozen;` | `public bool frozen;` |
| 536 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 662 | 2 | stoned | bool | `public bool stoned;` | `public bool stoned;` |
| 537 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 664 | 2 | ichor | bool | `public bool ichor;` | `public bool ichor;` |
| 538 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 666 | 2 | webbed | bool | `public bool webbed;` | `public bool webbed;` |
| 539 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 668 | 2 | tipsy | bool | `public bool tipsy;` | `public bool tipsy;` |
| 540 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 670 | 2 | noBuilding | bool | `public bool noBuilding;` | `public bool noBuilding;` |
| 572 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 734 | 2 | miscCounter | int | `public int miscCounter;` | `public int miscCounter;` |
| 575 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 741 | 2 | sandStorm | bool | `public bool sandStorm;` | `public bool sandStorm;` |
| 576 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 743 | 2 | crimsonRegen | bool | `public bool crimsonRegen;` | `public bool crimsonRegen;` |
| 577 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 745 | 2 | ghostHeal | bool | `public bool ghostHeal;` | `public bool ghostHeal;` |
| 578 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 747 | 2 | ghostHurt | bool | `public bool ghostHurt;` | `public bool ghostHurt;` |
| 579 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 749 | 2 | sticky | bool | `public bool sticky;` | `public bool sticky;` |
| 580 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 751 | 2 | slippy | bool | `public bool slippy;` | `public bool slippy;` |
| 581 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 753 | 2 | slippy2 | bool | `public bool slippy2;` | `public bool slippy2;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.17 细分子系统：`PlayerDetectionAndCombatStatus`

- 细分职责：危险感知、幸运、韧性、鞭子修正和战斗状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；战斗效果通过显式事件交接。
- 成员文件数：1；声明类型数：1；字段：11；属性：0；合计：11。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（11）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 586 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 763 | 2 | dangerSense | bool | `public bool dangerSense;` | `public bool dangerSense;` |
| 587 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 765 | 2 | luckPotion | byte | `public byte luckPotion;` | `public byte luckPotion;` |
| 588 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 767 | 2 | endurance | float | `public float endurance;` | `public float endurance;` |
| 589 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 769 | 2 | whipRangeMultiplier | float | `public float whipRangeMultiplier;` | `public float whipRangeMultiplier;` |
| 590 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 771 | 2 | whipUseTimeMultiplier | float | `public float whipUseTimeMultiplier;` | `public float whipUseTimeMultiplier;` |
| 591 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 773 | 2 | loveStruck | bool | `public bool loveStruck;` | `public bool loveStruck;` |
| 592 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 775 | 2 | stinky | bool | `public bool stinky;` | `public bool stinky;` |
| 593 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 777 | 2 | resistCold | bool | `public bool resistCold;` | `public bool resistCold;` |
| 594 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 779 | 2 | electrified | bool | `public bool electrified;` | `public bool electrified;` |
| 595 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 781 | 2 | dryadWard | bool | `public bool dryadWard;` | `public bool dryadWard;` |
| 596 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 783 | 2 | panic | bool | `public bool panic;` | `public bool panic;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.25 细分子系统：`PlayerSocialAndDefenseState`

- 细分职责：社交表现、PVP、护甲防御和环境保护状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；社交和防御事件分阶段提交。
- 成员文件数：1；声明类型数：1；字段：20；属性：0；合计：20。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（20）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 656 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 904 | 2 | skinVariant | int | `public int skinVariant;` | `public int skinVariant;` |
| 657 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 906 | 2 | voiceVariant | int | `public int voiceVariant;` | `public int voiceVariant;` |
| 658 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 908 | 2 | voicePitchOffset | float | `public float voicePitchOffset;` | `public float voicePitchOffset;` |
| 659 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 910 | 2 | ghost | bool | `public bool ghost;` | `public bool ghost;` |
| 660 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 912 | 2 | ghostFrame | int | `public int ghostFrame;` | `public int ghostFrame;` |
| 661 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 914 | 2 | ghostFrameCounter | int | `public int ghostFrameCounter;` | `public int ghostFrameCounter;` |
| 663 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 919 | 2 | _framesLeftEligibleForDeadmansChestDeathAchievement | int | `public int _framesLeftEligibleForDeadmansChestDeathAchievement;` | `public int _framesLeftEligibleForDeadmansChestDeathAchievement;` |
| 664 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 921 | 2 | pvpDeath | bool | `public bool pvpDeath;` | `public bool pvpDeath;` |
| 671 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 935 | 2 | boneArmor | bool | `public bool boneArmor;` | `public bool boneArmor;` |
| 672 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 937 | 2 | frostArmor | bool | `public bool frostArmor;` | `public bool frostArmor;` |
| 673 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 939 | 2 | honey | bool | `public bool honey;` | `public bool honey;` |
| 674 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 941 | 2 | crystalLeaf | bool | `public bool crystalLeaf;` | `public bool crystalLeaf;` |
| 675 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 943 | 2 | crystalLeafCooldown | int | `public int crystalLeafCooldown;` | `public int crystalLeafCooldown;` |
| 676 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 945 | 2 | portableStoolInfo | Terraria.DataStructures.PortableStoolUsage | `public PortableStoolUsage portableStoolInfo;` | `public PortableStoolUsage portableStoolInfo;` |
| 677 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 947 | 2 | preventAllItemPickups | bool | `public bool preventAllItemPickups;` | `public bool preventAllItemPickups;` |
| 678 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 949 | 2 | dontHurtCritters | bool | `public bool dontHurtCritters;` | `public bool dontHurtCritters;` |
| 679 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 951 | 2 | hasLucyTheAxe | bool | `public bool hasLucyTheAxe;` | `public bool hasLucyTheAxe;` |
| 680 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 953 | 2 | dontHurtNature | bool | `public bool dontHurtNature;` | `public bool dontHurtNature;` |
| 681 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 956 | 2 | defendedByPaladin | bool | `public bool defendedByPaladin;` | `public bool defendedByPaladin;` |
| 682 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 958 | 2 | hasPaladinShield | bool | `public bool hasPaladinShield;` | `public bool hasPaladinShield;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.26 细分子系统：`PlayerFrameAndImmunityState`

- 细分职责：玩家身体帧、无敌计时、闪烁和免疫打击状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；受伤状态转换集中写入。
- 成员文件数：1；声明类型数：1；字段：11；属性：0；合计：11。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（11）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 683 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 961 | 2 | townNPCs | int | `public int townNPCs;` | `public int townNPCs;` |
| 684 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 964 | 2 | bodyFrameCounter | double | `public double bodyFrameCounter;` | `public double bodyFrameCounter;` |
| 685 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 966 | 2 | legFrameCounter | double | `public double legFrameCounter;` | `public double legFrameCounter;` |
| 686 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 968 | 2 | immune | bool | `public bool immune;` | `public bool immune;` |
| 687 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 970 | 2 | immuneNoBlink | bool | `public bool immuneNoBlink;` | `public bool immuneNoBlink;` |
| 688 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 972 | 2 | immuneTime | int | `public int immuneTime;` | `public int immuneTime;` |
| 689 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 974 | 2 | immuneAlphaDirection | int | `public int immuneAlphaDirection;` | `public int immuneAlphaDirection;` |
| 690 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 976 | 2 | immuneAlpha | int | `public int immuneAlpha;` | `public int immuneAlpha;` |
| 692 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 980 | 2 | _timeSinceLastImmuneGet | int | `private int _timeSinceLastImmuneGet;` | `private int _timeSinceLastImmuneGet;` |
| 693 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 982 | 2 | _immuneStrikes | int | `private int _immuneStrikes;` | `private int _immuneStrikes;` |
| 695 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 986 | 2 | maxRegenDelay | float | `public float maxRegenDelay;` | `public float maxRegenDelay;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.41 细分子系统：`PlayerVitalAndRegenState`

- 细分职责：生命、魔力和生命/魔力回复资源状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；资源 Tick 是本组唯一权威写入路径。
- 成员文件数：1；声明类型数：1；字段：13；属性：0；合计：13。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（13）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 864 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1357 | 2 | statLifeMax | int | `public int statLifeMax = 100;` | `public int statLifeMax = 100;` |
| 865 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1359 | 2 | statLifeMax2 | int | `public int statLifeMax2 = 100;` | `public int statLifeMax2 = 100;` |
| 866 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1361 | 2 | statLife | int | `public int statLife = 100;` | `public int statLife = 100;` |
| 867 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1363 | 2 | statMana | int | `public int statMana;` | `public int statMana;` |
| 868 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1365 | 2 | statManaMax | int | `public int statManaMax;` | `public int statManaMax;` |
| 869 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1367 | 2 | statManaMax2 | int | `public int statManaMax2;` | `public int statManaMax2;` |
| 870 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1369 | 2 | lifeRegen | int | `public int lifeRegen;` | `public int lifeRegen;` |
| 871 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1371 | 2 | lifeRegenCount | int | `public int lifeRegenCount;` | `public int lifeRegenCount;` |
| 872 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1373 | 2 | lifeRegenTime | float | `public float lifeRegenTime;` | `public float lifeRegenTime;` |
| 873 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1375 | 2 | manaRegen | int | `public int manaRegen;` | `public int manaRegen;` |
| 874 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1377 | 2 | manaRegenCount | int | `public int manaRegenCount;` | `public int manaRegenCount;` |
| 875 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1379 | 2 | manaRegenDelay | float | `public float manaRegenDelay;` | `public float manaRegenDelay;` |
| 876 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1381 | 2 | manaRegenBuff | bool | `public bool manaRegenBuff;` | `public bool manaRegenBuff;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.42 细分子系统：`PlayerCombatModifierAndImmunityState`

- 细分职责：护甲穿透、防御、免疫和战斗修正状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；战斗事件单向更新修正状态。
- 成员文件数：1；声明类型数：1；字段：16；属性：0；合计：16。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（16）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 861 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1351 | 2 | armorPenetration | int | `public int armorPenetration;` | `public int armorPenetration;` |
| 862 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1353 | 2 | meleeArmorPenetration | int | `public int meleeArmorPenetration;` | `public int meleeArmorPenetration;` |
| 863 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1355 | 2 | statDefense | int | `public int statDefense;` | `public int statDefense;` |
| 877 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1383 | 2 | noKnockback | bool | `public bool noKnockback;` | `public bool noKnockback;` |
| 878 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1385 | 2 | shimmerImmune | bool | `private bool shimmerImmune;` | `private bool shimmerImmune;` |
| 879 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1387 | 2 | spaceGun | bool | `public bool spaceGun;` | `public bool spaceGun;` |
| 880 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1389 | 2 | gravDir | float | `public float gravDir = 1f;` | `public float gravDir = 1f;` |
| 893 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1415 | 2 | chaosState | bool | `public bool chaosState;` | `public bool chaosState;` |
| 894 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1417 | 2 | strongBees | bool | `public bool strongBees;` | `public bool strongBees;` |
| 895 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1419 | 2 | sporeSac | bool | `public bool sporeSac;` | `public bool sporeSac;` |
| 896 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1421 | 2 | shinyStone | bool | `public bool shinyStone;` | `public bool shinyStone;` |
| 897 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1423 | 2 | empressBrooch | bool | `public bool empressBrooch;` | `public bool empressBrooch;` |
| 898 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1425 | 2 | volatileGelatin | bool | `public bool volatileGelatin;` | `public bool volatileGelatin;` |
| 899 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1427 | 2 | volatileGelatinCounter | int | `public int volatileGelatinCounter;` | `public int volatileGelatinCounter;` |
| 900 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1429 | 2 | hasMagiluminescence | bool | `public bool hasMagiluminescence;` | `public bool hasMagiluminescence;` |
| 901 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1431 | 2 | shadowArmor | bool | `public bool shadowArmor;` | `public bool shadowArmor;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.43 细分子系统：`PlayerAmmoAndAccessoryEffects`

- 细分职责：弹药消耗、箭袋、药剂和武器配饰效果状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；装备效果通过显式能力提交。
- 成员文件数：1；声明类型数：1；字段：12；属性：0；合计：12。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（12）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 881 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1391 | 2 | chloroAmmoCost80 | bool | `public bool chloroAmmoCost80;` | `public bool chloroAmmoCost80;` |
| 882 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1393 | 2 | huntressAmmoCost90 | bool | `public bool huntressAmmoCost90;` | `public bool huntressAmmoCost90;` |
| 883 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1395 | 2 | ammoCost80 | bool | `public bool ammoCost80;` | `public bool ammoCost80;` |
| 884 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1397 | 2 | ammoCost75 | bool | `public bool ammoCost75;` | `public bool ammoCost75;` |
| 885 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1399 | 2 | stickyBreak | int | `public int stickyBreak;` | `public int stickyBreak;` |
| 886 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1401 | 2 | magicQuiver | bool | `public bool magicQuiver;` | `public bool magicQuiver;` |
| 887 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1403 | 2 | magmaStone | bool | `public bool magmaStone;` | `public bool magmaStone;` |
| 888 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1405 | 2 | lavaRose | bool | `public bool lavaRose;` | `public bool lavaRose;` |
| 889 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1407 | 2 | hasMoltenQuiver | bool | `public bool hasMoltenQuiver;` | `public bool hasMoltenQuiver;` |
| 890 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1409 | 2 | phantasmTime | int | `public int phantasmTime;` | `public int phantasmTime;` |
| 891 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1411 | 2 | ammoBox | bool | `public bool ammoBox;` | `public bool ammoBox;` |
| 892 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1413 | 2 | ammoPotion | bool | `public bool ammoPotion;` | `public bool ammoPotion;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.55 细分子系统：`PlayerElementalAndShimmerStatus`

- 细分职责：元素、微光和环境持续状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；状态 Tick 负责唯一写入并维护失效时间。
- 成员文件数：1；声明类型数：1；字段：21；属性：0；合计：21。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（21）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1041 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1712 | 2 | archery | bool | `public bool archery;` | `public bool archery;` |
| 1042 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1714 | 2 | poisoned | bool | `public bool poisoned;` | `public bool poisoned;` |
| 1043 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1716 | 2 | venom | bool | `public bool venom;` | `public bool venom;` |
| 1044 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1718 | 2 | blind | bool | `public bool blind;` | `public bool blind;` |
| 1045 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1720 | 2 | blackout | bool | `public bool blackout;` | `public bool blackout;` |
| 1046 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1722 | 2 | headcovered | bool | `public bool headcovered;` | `public bool headcovered;` |
| 1047 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1724 | 2 | frostBurn | bool | `public bool frostBurn;` | `public bool frostBurn;` |
| 1048 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1726 | 2 | onFrostBurn | bool | `public bool onFrostBurn;` | `public bool onFrostBurn;` |
| 1049 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1728 | 2 | onFrostBurn2 | bool | `public bool onFrostBurn2;` | `public bool onFrostBurn2;` |
| 1050 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1730 | 2 | burned | bool | `public bool burned;` | `public bool burned;` |
| 1051 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1732 | 2 | shimmering | bool | `public bool shimmering;` | `public bool shimmering;` |
| 1052 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1734 | 2 | timeShimmering | int | `public int timeShimmering;` | `public int timeShimmering;` |
| 1053 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1736 | 2 | shimmerTransparency | float | `public float shimmerTransparency;` | `public float shimmerTransparency;` |
| 1054 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1738 | 2 | shimmerUnstuckHelper | Terraria.GameContent.ShimmerUnstuckHelper | `public ShimmerUnstuckHelper shimmerUnstuckHelper;` | `public ShimmerUnstuckHelper shimmerUnstuckHelper;` |
| 1055 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1740 | 2 | suffocating | bool | `public bool suffocating;` | `public bool suffocating;` |
| 1056 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1743 | 2 | dripping | bool | `public bool dripping;` | `public bool dripping;` |
| 1057 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1745 | 2 | drippingSlime | bool | `public bool drippingSlime;` | `public bool drippingSlime;` |
| 1058 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1747 | 2 | drippingSparkleSlime | bool | `public bool drippingSparkleSlime;` | `public bool drippingSparkleSlime;` |
| 1059 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1749 | 2 | onFire | bool | `public bool onFire;` | `public bool onFire;` |
| 1060 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1751 | 2 | onFire2 | bool | `public bool onFire2;` | `public bool onFire2;` |
| 1061 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1753 | 2 | onFire3 | bool | `public bool onFire3;` | `public bool onFire3;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.56 细分子系统：`PlayerSurvivalAndTransformationState`

- 细分职责：生存饥饿、风推、变身、鱼人和三叉戟控制状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；生存/变身效果事件集中提交。
- 成员文件数：1；声明类型数：1；字段：15；属性：0；合计：15。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（15）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1062 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1755 | 2 | noItems | bool | `public bool noItems;` | `public bool noItems;` |
| 1064 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1759 | 2 | hungry | bool | `public bool hungry;` | `public bool hungry;` |
| 1065 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1761 | 2 | starving | bool | `public bool starving;` | `public bool starving;` |
| 1066 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1763 | 2 | heartyMeal | bool | `public bool heartyMeal;` | `public bool heartyMeal;` |
| 1067 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1765 | 2 | windPushed | bool | `public bool windPushed;` | `public bool windPushed;` |
| 1068 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1767 | 2 | wereWolf | bool | `public bool wereWolf;` | `public bool wereWolf;` |
| 1069 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1769 | 2 | wolfAcc | bool | `public bool wolfAcc;` | `public bool wolfAcc;` |
| 1070 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1771 | 2 | hideMerman | bool | `public bool hideMerman;` | `public bool hideMerman;` |
| 1071 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1773 | 2 | hideWolf | bool | `public bool hideWolf;` | `public bool hideWolf;` |
| 1072 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1775 | 2 | forceMerman | bool | `public bool forceMerman;` | `public bool forceMerman;` |
| 1073 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1777 | 2 | forceWerewolf | bool | `public bool forceWerewolf;` | `public bool forceWerewolf;` |
| 1074 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1779 | 2 | sunScorchCounter | int | `public int sunScorchCounter;` | `public int sunScorchCounter;` |
| 1079 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1789 | 2 | accMerman | bool | `public bool accMerman;` | `public bool accMerman;` |
| 1080 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1791 | 2 | merman | bool | `public bool merman;` | `public bool merman;` |
| 1081 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1793 | 2 | trident | bool | `public bool trident;` | `public bool trident;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.57 细分子系统：`PlayerDebuffStatusState`

- 细分职责：诅咒、流血、混乱、破甲、沉默、迟缓和舌头状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；减益 Tick 和命中事件单向更新。
- 成员文件数：1；声明类型数：1；字段：8；属性：0；合计：8。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（8）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1063 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1757 | 2 | cursed | bool | `public bool cursed;` | `public bool cursed;` |
| 1077 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1785 | 2 | bleed | bool | `public bool bleed;` | `public bool bleed;` |
| 1078 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1787 | 2 | confused | bool | `public bool confused;` | `public bool confused;` |
| 1082 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1795 | 2 | brokenArmor | bool | `public bool brokenArmor;` | `public bool brokenArmor;` |
| 1083 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1797 | 2 | silence | bool | `public bool silence;` | `public bool silence;` |
| 1084 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1799 | 2 | slow | bool | `public bool slow;` | `public bool slow;` |
| 1085 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1801 | 2 | gross | bool | `public bool gross;` | `public bool gross;` |
| 1086 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1803 | 2 | tongued | bool | `public bool tongued;` | `public bool tongued;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.59 细分子系统：`PlayerAccessoryCombatModifierState`

- 细分职责：手套、星云/星璇披风和无人机视野等战斗配饰修正。
- 边界角色：`authoritative state/behavior`；最小 seam：Accessory Combat System/CommitPort；装备效果集中提交。
- 成员文件数：1；声明类型数：1；字段：9；属性：0；合计：9。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（9）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1087 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1805 | 2 | kbGlove | bool | `public bool kbGlove;` | `public bool kbGlove;` |
| 1088 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1807 | 2 | autoReuseGlove | bool | `public bool autoReuseGlove;` | `public bool autoReuseGlove;` |
| 1089 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1809 | 2 | meleeScaleGlove | bool | `public bool meleeScaleGlove;` | `public bool meleeScaleGlove;` |
| 1090 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1811 | 2 | kbBuff | bool | `public bool kbBuff;` | `public bool kbBuff;` |
| 1091 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1813 | 2 | remoteVisionForDrone | bool | `public bool remoteVisionForDrone;` | `public bool remoteVisionForDrone;` |
| 1092 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1815 | 2 | starCloakItem | Terraria.Item | `public Item starCloakItem;` | `public Item starCloakItem;` |
| 1093 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1817 | 2 | starCloakItem_manaCloakOverrideItem | Terraria.Item | `public Item starCloakItem_manaCloakOverrideItem;` | `public Item starCloakItem_manaCloakOverrideItem;` |
| 1094 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1819 | 2 | starCloakItem_starVeilOverrideItem | Terraria.Item | `public Item starCloakItem_starVeilOverrideItem;` | `public Item starCloakItem_starVeilOverrideItem;` |
| 1095 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1821 | 2 | starCloakItem_beeCloakOverrideItem | Terraria.Item | `public Item starCloakItem_beeCloakOverrideItem;` | `public Item starCloakItem_beeCloakOverrideItem;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.60 细分子系统：`PlayerAccessoryResourceAndInvulnerabilityState`

- 细分职责：长时间无敌、哲学之石和魔力花等资源/免疫效果。
- 边界角色：`authoritative state/behavior`；最小 seam：Accessory Resource System/CommitPort；资源效果按装备快照更新。
- 成员文件数：1；声明类型数：1；字段：5；属性：0；合计：5。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（5）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1096 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1823 | 2 | longInvince | bool | `public bool longInvince;` | `public bool longInvince;` |
| 1097 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1825 | 2 | pStone | bool | `public bool pStone;` | `public bool pStone;` |
| 1098 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1827 | 2 | PhilosopherStoneDurationMultiplier | float | `public static readonly float PhilosopherStoneDurationMultiplier = 0.75f;` | `public static readonly float PhilosopherStoneDurationMultiplier = 0.75f;` |
| 1099 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1829 | 2 | manaFlower | bool | `public bool manaFlower;` | `public bool manaFlower;` |
| 1100 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1831 | 2 | moonLeech | bool | `public bool moonLeech;` | `public bool moonLeech;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.61 细分子系统：`PlayerAccessoryDebuffAndDropState`

- 细分职责：减益来源、枯萎、招架和掉落事件状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Accessory Status System/CommitPort；战斗状态与掉落事件显式交接。
- 成员文件数：1；声明类型数：1；字段：8；属性：0；合计：8。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（8）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1101 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1833 | 2 | vortexDebuff | bool | `public bool vortexDebuff;` | `public bool vortexDebuff;` |
| 1102 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1835 | 2 | trapDebuffSource | bool | `public bool trapDebuffSource;` | `public bool trapDebuffSource;` |
| 1103 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1837 | 2 | witheredArmor | bool | `public bool witheredArmor;` | `public bool witheredArmor;` |
| 1104 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1839 | 2 | witheredWeapon | bool | `public bool witheredWeapon;` | `public bool witheredWeapon;` |
| 1105 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1841 | 2 | slowOgreSpit | bool | `public bool slowOgreSpit;` | `public bool slowOgreSpit;` |
| 1106 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1843 | 2 | parryDamageBuff | bool | `public bool parryDamageBuff;` | `public bool parryDamageBuff;` |
| 1107 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1845 | 2 | ballistaPanic | bool | `public bool ballistaPanic;` | `public bool ballistaPanic;` |
| 1108 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1847 | 2 | JustDroppedAnItem | bool | `public bool JustDroppedAnItem;` | `public bool JustDroppedAnItem;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.62 细分子系统：`PlayerCombatDamageAndCritModifiers`

- 细分职责：武器类别伤害、暴击和召唤物击退修正。
- 边界角色：`authoritative state/behavior`；最小 seam：Combat Modifier System/CommitPort；装备计算完成后集中提交。
- 成员文件数：1；声明类型数：1；字段：14；属性：0；合计：14。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（14）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1110 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1851 | 2 | meleeCrit | int | `public int meleeCrit = 4;` | `public int meleeCrit = 4;` |
| 1111 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1853 | 2 | magicCrit | int | `public int magicCrit = 4;` | `public int magicCrit = 4;` |
| 1112 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1855 | 2 | rangedCrit | int | `public int rangedCrit = 4;` | `public int rangedCrit = 4;` |
| 1113 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1857 | 2 | meleeDamage | float | `public float meleeDamage = 1f;` | `public float meleeDamage = 1f;` |
| 1114 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1859 | 2 | magicDamage | float | `public float magicDamage = 1f;` | `public float magicDamage = 1f;` |
| 1115 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1861 | 2 | rangedDamage | float | `public float rangedDamage = 1f;` | `public float rangedDamage = 1f;` |
| 1116 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1863 | 2 | rangedMultDamage | float | `public float rangedMultDamage = 1f;` | `public float rangedMultDamage = 1f;` |
| 1117 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1865 | 2 | arrowDamageAdditiveStack | float | `public float arrowDamageAdditiveStack;` | `public float arrowDamageAdditiveStack;` |
| 1118 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1867 | 2 | arrowDamage | float | `public float arrowDamage = 1f;` | `public float arrowDamage = 1f;` |
| 1119 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1869 | 2 | bulletDamage | float | `public float bulletDamage = 1f;` | `public float bulletDamage = 1f;` |
| 1120 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1871 | 2 | rocketDamage | float | `public float rocketDamage = 1f;` | `public float rocketDamage = 1f;` |
| 1121 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1873 | 2 | minionDamage | float | `public float minionDamage = 1f;` | `public float minionDamage = 1f;` |
| 1122 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1875 | 2 | minionKB | float | `public float minionKB;` | `public float minionKB;` |
| 1123 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1877 | 2 | revolverCritChanceBonus | int | `public int revolverCritChanceBonus;` | `public int revolverCritChanceBonus;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.63 细分子系统：`PlayerCombatSpeedRangeAndPermissionState`

- 细分职责：攻击/移动/挖掘速度、建造自动化和持物许可修正。
- 边界角色：`authoritative state/behavior`；最小 seam：Combat/Interaction Modifier System/CommitPort；速度和范围修正显式合并。
- 成员文件数：1；声明类型数：1；字段：9；属性：0；合计：9。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（9）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1109 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1849 | 2 | IsAllowedToHoldItems | bool | `public bool IsAllowedToHoldItems = true;` | `public bool IsAllowedToHoldItems = true;` |
| 1124 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1879 | 2 | meleeSpeed | float | `public float meleeSpeed = 1f;` | `public float meleeSpeed = 1f;` |
| 1125 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1881 | 2 | summonerWeaponSpeedBonus | float | `public float summonerWeaponSpeedBonus;` | `public float summonerWeaponSpeedBonus;` |
| 1126 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1883 | 2 | moveSpeed | float | `public float moveSpeed = 1f;` | `public float moveSpeed = 1f;` |
| 1127 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1885 | 2 | pickSpeed | float | `public float pickSpeed = 1f;` | `public float pickSpeed = 1f;` |
| 1128 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1887 | 2 | wallSpeed | float | `public float wallSpeed = 1f;` | `public float wallSpeed = 1f;` |
| 1129 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1889 | 2 | tileSpeed | float | `public float tileSpeed = 1f;` | `public float tileSpeed = 1f;` |
| 1130 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1891 | 2 | autoPaint | bool | `public bool autoPaint;` | `public bool autoPaint;` |
| 1131 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1893 | 2 | autoActuator | bool | `public bool autoActuator;` | `public bool autoActuator;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.73 细分子系统：`PlayerLuckAndCommerceEffects`

- 细分职责：幸运、折扣、金钱配饰和相关效果状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；经济和幸运效果由显式效果提交。
- 成员文件数：1；声明类型数：1；字段：13；属性：0；合计：13。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（13）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1193 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2030 | 2 | discountEquipped | bool | `public bool discountEquipped;` | `public bool discountEquipped;` |
| 1194 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2032 | 2 | discountAvailable | bool | `public bool discountAvailable;` | `public bool discountAvailable;` |
| 1195 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2034 | 2 | hasLuckyCoin | bool | `public bool hasLuckyCoin;` | `public bool hasLuckyCoin;` |
| 1196 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2036 | 2 | boneGloveItem | Terraria.Item | `public Item boneGloveItem;` | `public Item boneGloveItem;` |
| 1197 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2038 | 2 | goldRing | bool | `public bool goldRing;` | `public bool goldRing;` |
| 1198 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2040 | 2 | accDivingHelm | bool | `public bool accDivingHelm;` | `public bool accDivingHelm;` |
| 1199 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2042 | 2 | accFlipper | bool | `public bool accFlipper;` | `public bool accFlipper;` |
| 1200 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2044 | 2 | deadCellsPotionStation | bool | `public bool deadCellsPotionStation;` | `public bool deadCellsPotionStation;` |
| 1201 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2046 | 2 | hasLuck_LuckyCoin | bool | `public bool hasLuck_LuckyCoin;` | `public bool hasLuck_LuckyCoin;` |
| 1202 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2048 | 2 | hasLuck_LuckyHorseshoe | bool | `public bool hasLuck_LuckyHorseshoe;` | `public bool hasLuck_LuckyHorseshoe;` |
| 1203 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2050 | 2 | hasLuck_LuckyClover | bool | `public bool hasLuck_LuckyClover;` | `public bool hasLuck_LuckyClover;` |
| 1204 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2052 | 2 | hasLuck_WiltedClover | bool | `public bool hasLuck_WiltedClover;` | `public bool hasLuck_WiltedClover;` |
| 1205 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2054 | 2 | hasLuck_RavenFeather | bool | `public bool hasLuck_RavenFeather;` | `public bool hasLuck_RavenFeather;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.72 细分子系统：`PlayerDpsTelemetryState`

- 细分职责：伤害统计窗口、最近命中和 DPS 累积状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；战斗事件驱动统计窗口更新。
- 成员文件数：1；声明类型数：1；字段：5；属性：0；合计：5。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（5）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1188 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2018 | 2 | dpsStart | System.DateTime | `public DateTime dpsStart;` | `public DateTime dpsStart;` |
| 1189 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2020 | 2 | dpsEnd | System.DateTime | `public DateTime dpsEnd;` | `public DateTime dpsEnd;` |
| 1190 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2022 | 2 | dpsLastHit | System.DateTime | `public DateTime dpsLastHit;` | `public DateTime dpsLastHit;` |
| 1191 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2024 | 2 | dpsDamage | int | `public int dpsDamage;` | `public int dpsDamage;` |
| 1192 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2026 | 2 | dpsStarted | bool | `public bool dpsStarted;` | `public bool dpsStarted;` |

##### 属性（0）

无该类型成员记录。


## 8. 本分区自检

- 叶子子系统：22 个；指定组均已写入且没有重复分区。
- 字段/属性/成员：254 / 0 / 254。
- 来源序号范围：474..1205；本分区内唯一序号：True。
- 成员声明表逐组从源报告提取，未改写来源序号、类型、路径、行列、成员、C# 类型、成员声明或原始声明。
- 运行时读写者、生命周期、持久化、网络和调度：未验证，保留为迁移前风险。
