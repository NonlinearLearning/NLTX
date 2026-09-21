# P13 NPC 城镇、事件、Boss 与世界进度 - 权威模拟系统成员组件候选分区

> 来源：`docs/migration/ledgers/Version4权威模拟系统字段属性逐成员源码声明-更细子系统拆分-去除ID类文件.md`；本文件只做成员库存分区和候选组件边界记录。
> 这是候选组件拆分台账，不是运行时迁移完成、行为等价、API 兼容、网络闭合或持久化闭合证明。

## 1. 分区边界

本分区包含 10 个叶子子系统，字段 106 条、属性 0 条、成员合计 106 条；每个叶子子系统保持原正式父级归属，不跨分区拆行。

## 2. 分区覆盖概览

| 叶子子系统 | 正式父级 | 字段 | 属性 | 合计 | 源边界角色 |
|---|---|---:|---:|---:|---|
| `NpcBossAndInvasionGlobalState` | `NpcAndTownSimulation` | 10 | 0 | 10 | authoritative state/behavior |
| `NpcBossAndInvasionState` | `NpcAndTownSimulation` | 5 | 0 | 5 | authoritative state/behavior |
| `NpcTownRescueState` | `NpcAndTownSimulation` | 8 | 0 | 8 | authoritative state/behavior |
| `NpcTownPetAdoptionState` | `NpcAndTownSimulation` | 3 | 0 | 3 | authoritative state/behavior |
| `NpcTownSpawnUnlockState` | `NpcAndTownSimulation` | 16 | 0 | 16 | authoritative state/behavior |
| `NpcTowerAndEventShieldState` | `NpcAndTownSimulation` | 10 | 0 | 10 | authoritative state/behavior |
| `NpcProgressionBookAndActiveRegistryState` | `NpcAndTownSimulation` | 4 | 0 | 4 | authoritative state/behavior |
| `NpcBossDefeatFlags` | `NpcAndTownSimulation` | 17 | 0 | 17 | authoritative state/behavior |
| `NpcEventDefeatFlags` | `NpcAndTownSimulation` | 14 | 0 | 14 | authoritative state/behavior |
| `NpcTownHousingAndBreathState` | `NpcAndTownSimulation` | 19 | 0 | 19 | authoritative state/behavior |

## 3. 候选组件归属与 seam

| 叶子子系统 | 候选组件边界 | 类型分类 | 最小 seam | 独立生命周期 | 证据状态 | 主要风险 |
|---|---|---|---|---|---|---|
| `NpcBossAndInvasionGlobalState` | `NpcBossAndInvasionGlobalStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `NpcBossAndInvasionState` | `NpcBossAndInvasionStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `NpcTownRescueState` | `NpcTownRescueStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `NpcTownPetAdoptionState` | `NpcTownPetAdoptionStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `NpcTownSpawnUnlockState` | `NpcTownSpawnUnlockStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `NpcTowerAndEventShieldState` | `NpcTowerAndEventShieldStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `NpcProgressionBookAndActiveRegistryState` | `NpcProgressionBookAndActiveRegistryStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `NpcBossDefeatFlags` | `NpcBossDefeatFlagsComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `NpcEventDefeatFlags` | `NpcEventDefeatFlagsComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `NpcTownHousingAndBreathState` | `NpcTownHousingAndBreathStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |

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
- 本分区父级局部统计：字段 106；属性 0；合计 106；完整父级统计以源报告为准。

#### 4.14.3 细分子系统：`NpcBossAndInvasionGlobalState`

- 细分职责：Boss 战斗距离、倒计时和入侵波次全局状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；Boss/入侵阶段通过世界事件提交。
- 成员文件数：1；声明类型数：1；字段：10；属性：0；合计：10。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（10）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1587 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 5915 | 2 | MoonLordAttacksArray | int[,,,] | `public static readonly int[,,,] MoonLordAttacksArray = InitializeMoonLordAttacks();` | `public static readonly int[,,,] MoonLordAttacksArray = InitializeMoonLordAttacks();` |
| 1588 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 5917 | 2 | MoonLordAttacksArray2 | int[,] | `public static readonly int[,] MoonLordAttacksArray2 = InitializeMoonLordAttacks2();` | `public static readonly int[,] MoonLordAttacksArray2 = InitializeMoonLordAttacks2();` |
| 1589 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 5919 | 2 | MoonLordFightingDistance | int | `public static int MoonLordFightingDistance = 4500;` | `public static int MoonLordFightingDistance = 4500;` |
| 1590 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 5921 | 2 | MoonLordCountdown | int | `public static int MoonLordCountdown = 0;` | `public static int MoonLordCountdown = 0;` |
| 1591 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 5923 | 2 | MaxMoonLordCountdown | int | `public static int MaxMoonLordCountdown = 3600;` | `public static int MaxMoonLordCountdown = 3600;` |
| 1592 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 5925 | 2 | NaturalMoonlordCountdownTime | int | `public const int NaturalMoonlordCountdownTime = 3600;` | `public const int NaturalMoonlordCountdownTime = 3600;` |
| 1593 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 5927 | 2 | ItemMoonlordCountdownTime | int | `public const int ItemMoonlordCountdownTime = 720;` | `public const int ItemMoonlordCountdownTime = 720;` |
| 1598 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 5937 | 2 | totalInvasionPoints | float | `public static float totalInvasionPoints = 0f;` | `public static float totalInvasionPoints = 0f;` |
| 1599 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 5939 | 2 | waveKills | float | `public static float waveKills = 0f;` | `public static float waveKills = 0f;` |
| 1600 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 5941 | 2 | waveNumber | int | `public static int waveNumber = 0;` | `public static int waveNumber = 0;` |

##### 属性（0）

无该类型成员记录。


#### 4.14.5 细分子系统：`NpcBossAndInvasionState`

- 细分职责：Boss 计数、入侵积分/波次及相关 Boss 进度。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System 读取输入并通过显式 Command/CommitPort 写入；跨组只用事件、Query 或命令交接。
- 成员文件数：1；声明类型数：1；字段：5；属性：0；合计：5。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（5）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1633 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6007 | 2 | golemBoss | int | `public static int golemBoss = -1;` | `public static int golemBoss = -1;` |
| 1634 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6009 | 2 | plantBoss | int | `public static int plantBoss = -1;` | `public static int plantBoss = -1;` |
| 1635 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6011 | 2 | crimsonBoss | int | `public static int crimsonBoss = -1;` | `public static int crimsonBoss = -1;` |
| 1636 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6013 | 2 | deerclopsBoss | int | `public static int deerclopsBoss = -1;` | `public static int deerclopsBoss = -1;` |
| 1637 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6015 | 2 | netUpdate | bool | `public bool netUpdate;` | `public bool netUpdate;` |

##### 属性（0）

无该类型成员记录。


#### 4.14.14 细分子系统：`NpcTownRescueState`

- 细分职责：已救援城镇 NPC 的持久进度旗标。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；救援事件集中更新。
- 成员文件数：1；声明类型数：1；字段：8；属性：0；合计：8。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（8）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1705 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6151 | 2 | savedTaxCollector | bool | `public static bool savedTaxCollector = false;` | `public static bool savedTaxCollector = false;` |
| 1706 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6153 | 2 | savedGoblin | bool | `public static bool savedGoblin = false;` | `public static bool savedGoblin = false;` |
| 1707 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6155 | 2 | savedWizard | bool | `public static bool savedWizard = false;` | `public static bool savedWizard = false;` |
| 1708 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6157 | 2 | savedMech | bool | `public static bool savedMech = false;` | `public static bool savedMech = false;` |
| 1709 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6159 | 2 | savedAngler | bool | `public static bool savedAngler = false;` | `public static bool savedAngler = false;` |
| 1710 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6161 | 2 | savedStylist | bool | `public static bool savedStylist = false;` | `public static bool savedStylist = false;` |
| 1711 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6163 | 2 | savedBartender | bool | `public static bool savedBartender = false;` | `public static bool savedBartender = false;` |
| 1712 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6165 | 2 | savedGolfer | bool | `public static bool savedGolfer = false;` | `public static bool savedGolfer = false;` |

##### 属性（0）

无该类型成员记录。


#### 4.14.15 细分子系统：`NpcTownPetAdoptionState`

- 细分职责：城镇宠物购买和领养解锁状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；购买事件单向提交。
- 成员文件数：1；声明类型数：1；字段：3；属性：0；合计：3。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（3）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1713 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6167 | 2 | boughtCat | bool | `public static bool boughtCat = false;` | `public static bool boughtCat = false;` |
| 1714 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6169 | 2 | boughtDog | bool | `public static bool boughtDog = false;` | `public static bool boughtDog = false;` |
| 1715 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6171 | 2 | boughtBunny | bool | `public static bool boughtBunny = false;` | `public static bool boughtBunny = false;` |

##### 属性（0）

无该类型成员记录。


#### 4.14.16 细分子系统：`NpcTownSpawnUnlockState`

- 细分职责：城镇 NPC 与特殊史莱姆生成解锁状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；生成资格 Query 只读消费解锁事实。
- 成员文件数：1；声明类型数：1；字段：16；属性：0；合计：16。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（16）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1716 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6173 | 2 | unlockedSlimeBlueSpawn | bool | `public static bool unlockedSlimeBlueSpawn = false;` | `public static bool unlockedSlimeBlueSpawn = false;` |
| 1717 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6175 | 2 | unlockedSlimeGreenSpawn | bool | `public static bool unlockedSlimeGreenSpawn = false;` | `public static bool unlockedSlimeGreenSpawn = false;` |
| 1718 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6177 | 2 | unlockedSlimeOldSpawn | bool | `public static bool unlockedSlimeOldSpawn = false;` | `public static bool unlockedSlimeOldSpawn = false;` |
| 1719 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6179 | 2 | unlockedSlimePurpleSpawn | bool | `public static bool unlockedSlimePurpleSpawn = false;` | `public static bool unlockedSlimePurpleSpawn = false;` |
| 1720 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6181 | 2 | unlockedSlimeRainbowSpawn | bool | `public static bool unlockedSlimeRainbowSpawn = false;` | `public static bool unlockedSlimeRainbowSpawn = false;` |
| 1721 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6183 | 2 | unlockedSlimeRedSpawn | bool | `public static bool unlockedSlimeRedSpawn = false;` | `public static bool unlockedSlimeRedSpawn = false;` |
| 1722 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6185 | 2 | unlockedSlimeYellowSpawn | bool | `public static bool unlockedSlimeYellowSpawn = false;` | `public static bool unlockedSlimeYellowSpawn = false;` |
| 1723 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6187 | 2 | unlockedSlimeCopperSpawn | bool | `public static bool unlockedSlimeCopperSpawn = false;` | `public static bool unlockedSlimeCopperSpawn = false;` |
| 1724 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6189 | 2 | unlockedMerchantSpawn | bool | `public static bool unlockedMerchantSpawn = false;` | `public static bool unlockedMerchantSpawn = false;` |
| 1725 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6191 | 2 | unlockedDemolitionistSpawn | bool | `public static bool unlockedDemolitionistSpawn = false;` | `public static bool unlockedDemolitionistSpawn = false;` |
| 1726 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6193 | 2 | unlockedPartyGirlSpawn | bool | `public static bool unlockedPartyGirlSpawn = false;` | `public static bool unlockedPartyGirlSpawn = false;` |
| 1727 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6195 | 2 | unlockedDyeTraderSpawn | bool | `public static bool unlockedDyeTraderSpawn = false;` | `public static bool unlockedDyeTraderSpawn = false;` |
| 1728 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6197 | 2 | unlockedTruffleSpawn | bool | `public static bool unlockedTruffleSpawn = false;` | `public static bool unlockedTruffleSpawn = false;` |
| 1729 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6199 | 2 | unlockedArmsDealerSpawn | bool | `public static bool unlockedArmsDealerSpawn = false;` | `public static bool unlockedArmsDealerSpawn = false;` |
| 1730 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6201 | 2 | unlockedNurseSpawn | bool | `public static bool unlockedNurseSpawn = false;` | `public static bool unlockedNurseSpawn = false;` |
| 1731 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6203 | 2 | unlockedPrincessSpawn | bool | `public static bool unlockedPrincessSpawn = false;` | `public static bool unlockedPrincessSpawn = false;` |

##### 属性（0）

无该类型成员记录。


#### 4.14.17 细分子系统：`NpcTowerAndEventShieldState`

- 细分职责：天界塔活动、护盾和事件阶段状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；事件阶段由世界进度命令提交。
- 成员文件数：1；声明类型数：1；字段：10；属性：0；合计：10。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（10）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1762 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6265 | 2 | ShieldStrengthTowerSolar | int | `public static int ShieldStrengthTowerSolar = 0;` | `public static int ShieldStrengthTowerSolar = 0;` |
| 1763 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6267 | 2 | ShieldStrengthTowerVortex | int | `public static int ShieldStrengthTowerVortex = 0;` | `public static int ShieldStrengthTowerVortex = 0;` |
| 1764 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6269 | 2 | ShieldStrengthTowerNebula | int | `public static int ShieldStrengthTowerNebula = 0;` | `public static int ShieldStrengthTowerNebula = 0;` |
| 1765 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6271 | 2 | ShieldStrengthTowerStardust | int | `public static int ShieldStrengthTowerStardust = 0;` | `public static int ShieldStrengthTowerStardust = 0;` |
| 1766 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6273 | 2 | LunarShieldPowerNormal | int | `public static int LunarShieldPowerNormal = 100;` | `public static int LunarShieldPowerNormal = 100;` |
| 1767 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6275 | 2 | TowerActiveSolar | bool | `public static bool TowerActiveSolar = false;` | `public static bool TowerActiveSolar = false;` |
| 1768 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6277 | 2 | TowerActiveVortex | bool | `public static bool TowerActiveVortex = false;` | `public static bool TowerActiveVortex = false;` |
| 1769 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6279 | 2 | TowerActiveNebula | bool | `public static bool TowerActiveNebula = false;` | `public static bool TowerActiveNebula = false;` |
| 1770 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6281 | 2 | TowerActiveStardust | bool | `public static bool TowerActiveStardust = false;` | `public static bool TowerActiveStardust = false;` |
| 1771 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6283 | 2 | LunarApocalypseIsUp | bool | `public static bool LunarApocalypseIsUp = false;` | `public static bool LunarApocalypseIsUp = false;` |

##### 属性（0）

无该类型成员记录。


#### 4.14.18 细分子系统：`NpcProgressionBookAndActiveRegistryState`

- 细分职责：战斗手册、商贩背包和活动检查登记状态。
- 边界角色：`authoritative state/behavior`；最小 seam：WorldEvent/Registry seam；进度事件单向提交。
- 成员文件数：1；声明类型数：1；字段：4；属性：0；合计：4。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（4）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1732 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6205 | 2 | combatBookWasUsed | bool | `public static bool combatBookWasUsed = false;` | `public static bool combatBookWasUsed = false;` |
| 1733 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6207 | 2 | combatBookVolumeTwoWasUsed | bool | `public static bool combatBookVolumeTwoWasUsed = false;` | `public static bool combatBookVolumeTwoWasUsed = false;` |
| 1734 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6209 | 2 | peddlersSatchelWasUsed | bool | `public static bool peddlersSatchelWasUsed = false;` | `public static bool peddlersSatchelWasUsed = false;` |
| 1776 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6293 | 2 | npcsFoundForCheckActive | bool[] | `public static bool[] npcsFoundForCheckActive = new bool[NPCID.Count];` | `public static bool[] npcsFoundForCheckActive = new bool[NPCID.Count];` |

##### 属性（0）

无该类型成员记录。


#### 4.14.19 细分子系统：`NpcBossDefeatFlags`

- 细分职责：主要 Boss、机械 Boss 和事件后 Boss 的击败进度标志。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；Boss 结算事件是唯一进度写入方向。
- 成员文件数：1；声明类型数：1；字段：17；属性：0；合计：17。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（17）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1735 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6211 | 2 | downedBoss1 | bool | `public static bool downedBoss1 = false;` | `public static bool downedBoss1 = false;` |
| 1736 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6213 | 2 | downedBoss2 | bool | `public static bool downedBoss2 = false;` | `public static bool downedBoss2 = false;` |
| 1737 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6215 | 2 | downedBoss3 | bool | `public static bool downedBoss3 = false;` | `public static bool downedBoss3 = false;` |
| 1738 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6217 | 2 | downedQueenBee | bool | `public static bool downedQueenBee = false;` | `public static bool downedQueenBee = false;` |
| 1739 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6219 | 2 | downedSlimeKing | bool | `public static bool downedSlimeKing = false;` | `public static bool downedSlimeKing = false;` |
| 1744 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6229 | 2 | downedPlantBoss | bool | `public static bool downedPlantBoss = false;` | `public static bool downedPlantBoss = false;` |
| 1745 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6231 | 2 | downedGolemBoss | bool | `public static bool downedGolemBoss = false;` | `public static bool downedGolemBoss = false;` |
| 1747 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6235 | 2 | downedFishron | bool | `public static bool downedFishron = false;` | `public static bool downedFishron = false;` |
| 1753 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6247 | 2 | downedAncientCultist | bool | `public static bool downedAncientCultist = false;` | `public static bool downedAncientCultist = false;` |
| 1754 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6249 | 2 | downedMoonlord | bool | `public static bool downedMoonlord = false;` | `public static bool downedMoonlord = false;` |
| 1759 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6259 | 2 | downedEmpressOfLight | bool | `public static bool downedEmpressOfLight = false;` | `public static bool downedEmpressOfLight = false;` |
| 1760 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6261 | 2 | downedQueenSlime | bool | `public static bool downedQueenSlime = false;` | `public static bool downedQueenSlime = false;` |
| 1761 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6263 | 2 | downedDeerclops | bool | `public static bool downedDeerclops = false;` | `public static bool downedDeerclops = false;` |
| 1772 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6285 | 2 | downedMechBossAny | bool | `public static bool downedMechBossAny = false;` | `public static bool downedMechBossAny = false;` |
| 1773 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6287 | 2 | downedMechBoss1 | bool | `public static bool downedMechBoss1 = false;` | `public static bool downedMechBoss1 = false;` |
| 1774 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6289 | 2 | downedMechBoss2 | bool | `public static bool downedMechBoss2 = false;` | `public static bool downedMechBoss2 = false;` |
| 1775 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6291 | 2 | downedMechBoss3 | bool | `public static bool downedMechBoss3 = false;` | `public static bool downedMechBoss3 = false;` |

##### 属性（0）

无该类型成员记录。


#### 4.14.20 细分子系统：`NpcEventDefeatFlags`

- 细分职责：入侵、节日和天界塔事件击败进度标志。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；事件结算与世界事件查询单向交接。
- 成员文件数：1；声明类型数：1；字段：14；属性：0；合计：14。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（14）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1740 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6221 | 2 | downedGoblins | bool | `public static bool downedGoblins = false;` | `public static bool downedGoblins = false;` |
| 1741 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6223 | 2 | downedFrost | bool | `public static bool downedFrost = false;` | `public static bool downedFrost = false;` |
| 1742 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6225 | 2 | downedPirates | bool | `public static bool downedPirates = false;` | `public static bool downedPirates = false;` |
| 1743 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6227 | 2 | downedClown | bool | `public static bool downedClown = false;` | `public static bool downedClown = false;` |
| 1746 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6233 | 2 | downedMartians | bool | `public static bool downedMartians = false;` | `public static bool downedMartians = false;` |
| 1748 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6237 | 2 | downedHalloweenTree | bool | `public static bool downedHalloweenTree = false;` | `public static bool downedHalloweenTree = false;` |
| 1749 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6239 | 2 | downedHalloweenKing | bool | `public static bool downedHalloweenKing = false;` | `public static bool downedHalloweenKing = false;` |
| 1750 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6241 | 2 | downedChristmasIceQueen | bool | `public static bool downedChristmasIceQueen = false;` | `public static bool downedChristmasIceQueen = false;` |
| 1751 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6243 | 2 | downedChristmasTree | bool | `public static bool downedChristmasTree = false;` | `public static bool downedChristmasTree = false;` |
| 1752 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6245 | 2 | downedChristmasSantank | bool | `public static bool downedChristmasSantank = false;` | `public static bool downedChristmasSantank = false;` |
| 1755 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6251 | 2 | downedTowerSolar | bool | `public static bool downedTowerSolar = false;` | `public static bool downedTowerSolar = false;` |
| 1756 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6253 | 2 | downedTowerVortex | bool | `public static bool downedTowerVortex = false;` | `public static bool downedTowerVortex = false;` |
| 1757 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6255 | 2 | downedTowerNebula | bool | `public static bool downedTowerNebula = false;` | `public static bool downedTowerNebula = false;` |
| 1758 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6257 | 2 | downedTowerStardust | bool | `public static bool downedTowerStardust = false;` | `public static bool downedTowerStardust = false;` |

##### 属性（0）

无该类型成员记录。


#### 4.14.25 细分子系统：`NpcTownHousingAndBreathState`

- 细分职责：NPC 城镇住房、门交互和呼吸状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；城镇和环境生命周期单向更新。
- 成员文件数：1；声明类型数：1；字段：19；属性：0；合计：19。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（19）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1828 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6397 | 2 | townNPC | bool | `public bool townNPC;` | `public bool townNPC;` |
| 1829 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6399 | 2 | nextDialogue | Terraria.GameContent.ConditionalDialogue | `public ConditionalDialogue nextDialogue;` | `public ConditionalDialogue nextDialogue;` |
| 1830 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6401 | 2 | travelNPC | bool | `public static bool travelNPC = false;` | `public static bool travelNPC = false;` |
| 1831 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6403 | 2 | homeless | bool | `public bool homeless;` | `public bool homeless;` |
| 1832 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6405 | 2 | homelessDespawn | bool | `public bool homelessDespawn;` | `public bool homelessDespawn;` |
| 1833 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6407 | 2 | lookForHomeTimeout | int | `public int lookForHomeTimeout;` | `public int lookForHomeTimeout;` |
| 1834 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6409 | 2 | KickOutLookForHomeTimeout | int | `public static readonly int KickOutLookForHomeTimeout = 3600;` | `public static readonly int KickOutLookForHomeTimeout = 3600;` |
| 1835 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6411 | 2 | homeTileX | int | `public int homeTileX = -1;` | `public int homeTileX = -1;` |
| 1836 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6413 | 2 | homeTileY | int | `public int homeTileY = -1;` | `public int homeTileY = -1;` |
| 1837 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6415 | 2 | housingCategory | int | `public int housingCategory;` | `public int housingCategory;` |
| 1838 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6417 | 2 | oldHomeless | bool | `public bool oldHomeless;` | `public bool oldHomeless;` |
| 1839 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6419 | 2 | oldHomeTileX | int | `public int oldHomeTileX = -1;` | `public int oldHomeTileX = -1;` |
| 1840 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6421 | 2 | oldHomeTileY | int | `public int oldHomeTileY = -1;` | `public int oldHomeTileY = -1;` |
| 1842 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6425 | 2 | closeDoor | bool | `public bool closeDoor;` | `public bool closeDoor;` |
| 1843 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6427 | 2 | doorX | int | `public int doorX;` | `public int doorX;` |
| 1844 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6429 | 2 | doorY | int | `public int doorY;` | `public int doorY;` |
| 1846 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6433 | 2 | breath | int | `public int breath;` | `public int breath;` |
| 1847 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6435 | 2 | breathMax | int | `public const int breathMax = 200;` | `public const int breathMax = 200;` |
| 1848 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6437 | 2 | breathCounter | int | `public int breathCounter;` | `public int breathCounter;` |

##### 属性（0）

无该类型成员记录。


## 8. 本分区自检

- 叶子子系统：10 个；指定组均已写入且没有重复分区。
- 字段/属性/成员：106 / 0 / 106。
- 来源序号范围：1587..1848；本分区内唯一序号：True。
- 成员声明表逐组从源报告提取，未改写来源序号、类型、路径、行列、成员、C# 类型、成员声明或原始声明。
- 运行时读写者、生命周期、持久化、网络和调度：未验证，保留为迁移前风险。
