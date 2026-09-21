# P14 NPC 生成、资格判断与目标选择 - 权威模拟系统成员组件候选分区

> 来源：`docs/migration/ledgers/Version4权威模拟系统字段属性逐成员源码声明-更细子系统拆分-去除ID类文件.md`；本文件只做成员库存分区和候选组件边界记录。
> 这是候选组件拆分台账，不是运行时迁移完成、行为等价、API 兼容、网络闭合或持久化闭合证明。

## 1. 分区边界

本分区包含 12 个叶子子系统，字段 92 条、属性 25 条、成员合计 117 条；每个叶子子系统保持原正式父级归属，不跨分区拆行。

## 2. 分区覆盖概览

| 叶子子系统 | 正式父级 | 字段 | 属性 | 合计 | 源边界角色 |
|---|---|---:|---:|---:|---|
| `NpcSpawnAndCritterState` | `NpcAndTownSimulation` | 13 | 0 | 13 | authoritative state/behavior |
| `NpcSpawnBudgetAndActivityState` | `NpcAndTownSimulation` | 10 | 0 | 10 | authoritative state/behavior |
| `NpcSpawnCooldownAndEnvironment` | `NpcAndTownSimulation` | 10 | 0 | 10 | authoritative state/behavior |
| `NpcTargetAndIdentityProperties` | `NpcAndTownSimulation` | 0 | 16 | 16 | derived/query |
| `NpcProgressionAndEnvironmentProperties` | `NpcAndTownSimulation` | 0 | 9 | 9 | derived/query |
| `NpcSpawnContextAndCapacityInputs` | `NpcAndTownSimulation` | 10 | 0 | 10 | authoritative state/behavior |
| `NpcSpawnSpatialEligibilityInputs` | `NpcAndTownSimulation` | 10 | 0 | 10 | derived/query |
| `NpcSpawnBiomeAndDungeonEligibilityInputs` | `NpcAndTownSimulation` | 6 | 0 | 6 | derived/query |
| `NpcSpawnPolicyAndEventEligibilityInputs` | `NpcAndTownSimulation` | 11 | 0 | 11 | derived/query |
| `NpcSpawnBiomeZoneInputs` | `NpcAndTownSimulation` | 13 | 0 | 13 | derived/query |
| `NpcSpawnEventAndTowerInputs` | `NpcAndTownSimulation` | 8 | 0 | 8 | derived/query |
| `NpcSpawnTargetSelectionState` | `NpcAndTownSimulation` | 1 | 0 | 1 | derived/query |

## 3. 候选组件归属与 seam

| 叶子子系统 | 候选组件边界 | 类型分类 | 最小 seam | 独立生命周期 | 证据状态 | 主要风险 |
|---|---|---|---|---|---|---|
| `NpcSpawnAndCritterState` | `NpcSpawnAndCritterStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `NpcSpawnBudgetAndActivityState` | `NpcSpawnBudgetAndActivityStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `NpcSpawnCooldownAndEnvironment` | `NpcSpawnCooldownAndEnvironmentComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `NpcTargetAndIdentityProperties` | `NpcTargetAndIdentityPropertiesQuery` | Query | 纯资格或派生 Query；只读 | 候选纯读边界；仍需核对失效/缓存条件 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `NpcProgressionAndEnvironmentProperties` | `NpcProgressionAndEnvironmentPropertiesQuery` | Query | 纯资格或派生 Query；只读 | 候选纯读边界；仍需核对失效/缓存条件 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `NpcSpawnContextAndCapacityInputs` | `NpcSpawnContextAndCapacityInputsComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `NpcSpawnSpatialEligibilityInputs` | `NpcSpawnSpatialEligibilityInputsQuery` | Query | 纯资格或派生 Query；只读 | 候选纯读边界；仍需核对失效/缓存条件 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `NpcSpawnBiomeAndDungeonEligibilityInputs` | `NpcSpawnBiomeAndDungeonEligibilityInputsQuery` | Query | 纯资格或派生 Query；只读 | 候选纯读边界；仍需核对失效/缓存条件 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `NpcSpawnPolicyAndEventEligibilityInputs` | `NpcSpawnPolicyAndEventEligibilityInputsQuery` | Query | 纯资格或派生 Query；只读 | 候选纯读边界；仍需核对失效/缓存条件 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `NpcSpawnBiomeZoneInputs` | `NpcSpawnBiomeZoneInputsQuery` | Query | 纯资格或派生 Query；只读 | 候选纯读边界；仍需核对失效/缓存条件 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `NpcSpawnEventAndTowerInputs` | `NpcSpawnEventAndTowerInputsQuery` | Query | 纯资格或派生 Query；只读 | 候选纯读边界；仍需核对失效/缓存条件 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `NpcSpawnTargetSelectionState` | `NpcSpawnTargetSelectionStateQuery` | Query | 纯资格或派生 Query；只读 | 候选纯读边界；仍需核对失效/缓存条件 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |

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
- 本分区父级局部统计：字段 92；属性 25；合计 117；完整父级统计以源报告为准。

#### 4.14.4 细分子系统：`NpcSpawnAndCritterState`

- 细分职责：生成来源、替换资格、昆虫概率和城镇微光变体状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；生成资格由生成系统统一提交。
- 成员文件数：1；声明类型数：1；字段：13；属性：0；合计：13。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（13）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1596 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 5933 | 2 | maxAI | int | `public static int maxAI = 4;` | `public static int maxAI = 4;` |
| 1597 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 5935 | 2 | goldCritterChance | int | `public static int goldCritterChance = 400;` | `public static int goldCritterChance = 400;` |
| 1604 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 5949 | 2 | SpawnedFromStatue | bool | `public bool SpawnedFromStatue;` | `public bool SpawnedFromStatue;` |
| 1605 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 5951 | 2 | CanBeReplacedByOtherNPCs | bool | `public bool CanBeReplacedByOtherNPCs;` | `public bool CanBeReplacedByOtherNPCs;` |
| 1606 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 5953 | 2 | dripping | bool | `public bool dripping;` | `public bool dripping;` |
| 1607 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 5955 | 2 | drippingSlime | bool | `public bool drippingSlime;` | `public bool drippingSlime;` |
| 1608 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 5957 | 2 | drippingSparkleSlime | bool | `public bool drippingSparkleSlime;` | `public bool drippingSparkleSlime;` |
| 1609 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 5959 | 2 | ShimmeredTownNPCs | bool[] | `public static bool[] ShimmeredTownNPCs = new bool[NPCID.Count];` | `public static bool[] ShimmeredTownNPCs = new bool[NPCID.Count];` |
| 1625 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 5991 | 2 | fireFlyFriendly | int | `public static int fireFlyFriendly = 0;` | `public static int fireFlyFriendly = 0;` |
| 1626 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 5993 | 2 | fireFlyChance | int | `public static int fireFlyChance = 0;` | `public static int fireFlyChance = 0;` |
| 1627 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 5995 | 2 | fireFlyMultiple | int | `public static int fireFlyMultiple = 0;` | `public static int fireFlyMultiple = 0;` |
| 1628 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 5997 | 2 | butterflyChance | int | `public static int butterflyChance = 0;` | `public static int butterflyChance = 0;` |
| 1629 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 5999 | 2 | stinkBugChance | int | `public static int stinkBugChance = 0;` | `public static int stinkBugChance = 0;` |

##### 属性（0）

无该类型成员记录。


#### 4.14.7 细分子系统：`NpcSpawnBudgetAndActivityState`

- 细分职责：NPC 活跃范围、生成频率、生成容量和计数预算状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；生成调度阶段集中写入。
- 成员文件数：1；声明类型数：1；字段：10；属性：0；合计：10。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（10）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1651 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6043 | 2 | safeRangeX | int | `public static int safeRangeX = (int)((double)(sWidth / 16) * 0.52);` | `public static int safeRangeX = (int)((double)(sWidth / 16) * 0.52);` |
| 1652 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6045 | 2 | safeRangeY | int | `public static int safeRangeY = (int)((double)(sHeight / 16) * 0.52);` | `public static int safeRangeY = (int)((double)(sHeight / 16) * 0.52);` |
| 1653 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6047 | 2 | activeRangeX | int | `private static int activeRangeX = (int)((double)sWidth * 2.1);` | `private static int activeRangeX = (int)((double)sWidth * 2.1);` |
| 1654 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6049 | 2 | activeRangeY | int | `private static int activeRangeY = (int)((double)sHeight * 2.1);` | `private static int activeRangeY = (int)((double)sHeight * 2.1);` |
| 1655 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6051 | 2 | npcSlots | float | `public float npcSlots = 1f;` | `public float npcSlots = 1f;` |
| 1656 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6053 | 2 | noSpawnCycle | bool | `private static bool noSpawnCycle = false;` | `private static bool noSpawnCycle = false;` |
| 1657 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6055 | 2 | activeTime | int | `private static int activeTime = 750;` | `private static int activeTime = 750;` |
| 1658 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6057 | 2 | defaultSpawnRate | int | `private static int defaultSpawnRate = 600;` | `private static int defaultSpawnRate = 600;` |
| 1659 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6059 | 2 | defaultMaxSpawns | int | `private static int defaultMaxSpawns = 5;` | `private static int defaultMaxSpawns = 5;` |
| 1661 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6063 | 2 | dontCountMe | bool | `public bool dontCountMe;` | `public bool dontCountMe;` |

##### 属性（0）

无该类型成员记录。


#### 4.14.27 细分子系统：`NpcSpawnCooldownAndEnvironment`

- 细分职责：事件波次所需分数、每日击杀和生成保护/冷却。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System 读取输入并通过显式 Command/CommitPort 写入；跨组只用事件、Query 或命令交接。
- 成员文件数：1；声明类型数：1；字段：10；属性：0；合计：10。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（10）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1860 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6461 | 2 | MoonEventRequiredPointsPerWaveLookup | int[] | `public static int[] MoonEventRequiredPointsPerWaveLookup = new int[21]  	{  		0, 25, 40, 50, 80, 100, 160, 180, 200, 250,  		300, 375, 450, 525, 675, 850, 1025, 1325, 1550, 2000,  		0  	};` | `public static int[] MoonEventRequiredPointsPerWaveLookup = new int[21] { 0, 25, 40, 50, 80, 100, 160, 180, 200, 250, 300, 375, 450, 525, 675, 850, 1025, 1325, 1550, 2000, 0 };` |
| 1861 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6468 | 2 | EoCKilledToday | bool | `private static bool EoCKilledToday;` | `private static bool EoCKilledToday;` |
| 1862 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6470 | 2 | WoFKilledToday | bool | `private static bool WoFKilledToday;` | `private static bool WoFKilledToday;` |
| 1863 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6472 | 2 | SPAWN_SLOT_PROTECTION_TIME | int | `public const int SPAWN_SLOT_PROTECTION_TIME = 2;` | `public const int SPAWN_SLOT_PROTECTION_TIME = 2;` |
| 1864 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6474 | 2 | ignorePlayerInteractions | int | `private static int ignorePlayerInteractions = 0;` | `private static int ignorePlayerInteractions = 0;` |
| 1865 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6476 | 2 | ladyBugGoodLuckTime | int | `public static int ladyBugGoodLuckTime = 43200;` | `public static int ladyBugGoodLuckTime = 43200;` |
| 1866 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6478 | 2 | ladyBugBadLuckTime | int | `public static int ladyBugBadLuckTime = -10800;` | `public static int ladyBugBadLuckTime = -10800;` |
| 1867 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6480 | 2 | ladyBugRainTime | int | `private static int ladyBugRainTime = 1800;` | `private static int ladyBugRainTime = 1800;` |
| 1868 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6482 | 2 | maximumAmountOfTimesLadyBugRainCanStack | int | `private static int maximumAmountOfTimesLadyBugRainCanStack = 10 * ladyBugRainTime;` | `private static int maximumAmountOfTimesLadyBugRainCanStack = 10 * ladyBugRainTime;` |
| 1869 | field | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6484 | 2 | offSetDelayTime | int | `public static int offSetDelayTime = 60;` | `public static int offSetDelayTime = 60;` |

##### 属性（0）

无该类型成员记录。


#### 4.14.28 细分子系统：`NpcTargetAndIdentityProperties`

- 细分职责：NPC 目标资格、名称、类型和可交谈资格派生属性。
- 边界角色：`derived/query`；最小 seam：只读快照/纯资格 Query；不得写入该分组或相邻权威状态。
- 成员文件数：1；声明类型数：1；字段：0；属性：16；合计：16。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（0）

无该类型成员记录。

##### 属性（16）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1886 | property | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6486 | 2 | CanTalk | bool | `public bool CanTalk { get { if (isLikeATownNPC && aiStyle == 7 && velocity.Y == 0f) { return !NPCID.Sets.IsTownPet[type]; } return false; } }` | `public bool CanTalk { get { if (isLikeATownNPC && aiStyle == 7 && velocity.Y == 0f) { return !NPCID.Sets.IsTownPet[type]; } return false; } }` |
| 1887 | property | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6498 | 2 | CanBeTalkedTo | bool | `public bool CanBeTalkedTo { get { if (isLikeATownNPC && aiStyle == 7) { return velocity.Y == 0f; } return false; } }` | `public bool CanBeTalkedTo { get { if (isLikeATownNPC && aiStyle == 7) { return velocity.Y == 0f; } return false; } }` |
| 1888 | property | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6510 | 2 | HasValidTarget | bool | `public bool HasValidTarget { get { if (!HasPlayerTarget \|\| !Main.player[target].active \|\| Main.player[target].dead \|\| Main.player[target].ghost) { if (SupportsNPCTargets && HasNPCTarget) { return Main.npc[TranslatedTargetIndex].active; } return false; } return true; } }` | `public bool HasValidTarget { get { if (!HasPlayerTarget \|\| !Main.player[target].active \|\| Main.player[target].dead \|\| Main.player[target].ghost) { if (SupportsNPCTargets && HasNPCTarget) { return Main.npc[TranslatedTargetIndex].active; } return false; } return true; } }` |
| 1889 | property | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6526 | 2 | HasPlayerTarget | bool | `public bool HasPlayerTarget { get { if (target >= 0) { return target < 255; } return false; } }` | `public bool HasPlayerTarget { get { if (target >= 0) { return target < 255; } return false; } }` |
| 1890 | property | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6538 | 2 | HasNPCTarget | bool | `public bool HasNPCTarget { get { if (target >= 300) { return target < 300 + Main.maxNPCs; } return false; } }` | `public bool HasNPCTarget { get { if (target >= 300) { return target < 300 + Main.maxNPCs; } return false; } }` |
| 1891 | property | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6550 | 2 | SupportsNPCTargets | bool | `public bool SupportsNPCTargets => NPCID.Sets.UsesNewTargeting[type];` | `public bool SupportsNPCTargets => NPCID.Sets.UsesNewTargeting[type];` |
| 1892 | property | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6552 | 2 | TranslatedTargetIndex | int | `public int TranslatedTargetIndex { get { if (HasNPCTarget) { return target - 300; } return target; } }` | `public int TranslatedTargetIndex { get { if (HasNPCTarget) { return target - 300; } return target; } }` |
| 1893 | property | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6564 | 2 | WhoAmIToTargetingIndex | int | `public int WhoAmIToTargetingIndex => whoAmI + 300;` | `public int WhoAmIToTargetingIndex => whoAmI + 300;` |
| 1894 | property | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6566 | 2 | IsShimmerVariant | bool | `public bool IsShimmerVariant { get { if (townNpcVariationIndex == 1) { return NPCID.Sets.ShimmerTownTransform[type]; } return false; } }` | `public bool IsShimmerVariant { get { if (townNpcVariationIndex == 1) { return NPCID.Sets.ShimmerTownTransform[type]; } return false; } }` |
| 1895 | property | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6578 | 2 | TypeName | string | `public string TypeName => Lang.GetNPCNameValue(netID);` | `public string TypeName => Lang.GetNPCNameValue(netID);` |
| 1896 | property | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6580 | 2 | FullName | string | `public string FullName { get { if (!HasGivenName) { return TypeName; } return Language.GetTextValue("Game.NPCTitle", _givenName, TypeName); } }` | `public string FullName { get { if (!HasGivenName) { return TypeName; } return Language.GetTextValue("Game.NPCTitle", _givenName, TypeName); } }` |
| 1897 | property | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6592 | 2 | HasGivenName | bool | `public bool HasGivenName => _givenName.Length != 0;` | `public bool HasGivenName => _givenName.Length != 0;` |
| 1898 | property | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6594 | 2 | GivenOrTypeName | string | `public string GivenOrTypeName { get { if (!HasGivenName) { return TypeName; } return _givenName; } }` | `public string GivenOrTypeName { get { if (!HasGivenName) { return TypeName; } return _givenName; } }` |
| 1899 | property | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6606 | 2 | GivenName | string | `public string GivenName { get { return _givenName; } set { _givenName = value ?? ""; } }` | `public string GivenName { get { return _givenName; } set { _givenName = value ?? ""; } }` |
| 1900 | property | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6618 | 2 | sWidth | int | `public static int sWidth => 1920;` | `public static int sWidth => 1920;` |
| 1901 | property | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6620 | 2 | sHeight | int | `public static int sHeight => 1200;` | `public static int sHeight => 1200;` |


#### 4.14.29 细分子系统：`NpcProgressionAndEnvironmentProperties`

- 细分职责：NPC Boss/环境/网络区段的派生属性。
- 边界角色：`derived/query`；最小 seam：只读快照/纯资格 Query；不得写入该分组或相邻权威状态。
- 成员文件数：1；声明类型数：1；字段：0；属性：9；合计：9。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（0）

无该类型成员记录。

##### 属性（9）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1902 | property | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6622 | 2 | DownedAnyPreHardmodeBoss | bool | `public static bool DownedAnyPreHardmodeBoss { get { if (!downedSlimeKing && !downedBoss1 && !downedBoss2 && !downedBoss3 && !downedQueenBee && !downedDeerclops) { return Main.hardMode; } return true; } }` | `public static bool DownedAnyPreHardmodeBoss { get { if (!downedSlimeKing && !downedBoss1 && !downedBoss2 && !downedBoss3 && !downedQueenBee && !downedDeerclops) { return Main.hardMode; } return true; } }` |
| 1903 | property | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6634 | 2 | ShieldStrengthTowerMax | int | `public static int ShieldStrengthTowerMax { get { int num = LunarShieldPowerNormal; if (downedMoonlord) { num /= 2; } return num; } }` | `public static int ShieldStrengthTowerMax { get { int num = LunarShieldPowerNormal; if (downedMoonlord) { num /= 2; } return num; } }` |
| 1904 | property | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6647 | 2 | Opacity | float | `public float Opacity { get { return 1f - (float)alpha / 255f; } set { alpha = (int)MathHelper.Clamp((1f - value) * 255f, 0f, 255f); } }` | `public float Opacity { get { return 1f - (float)alpha / 255f; } set { alpha = (int)MathHelper.Clamp((1f - value) * 255f, 0f, 255f); } }` |
| 1905 | property | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6659 | 2 | TreatedAsABossForRainbowBoulders | bool | `public bool TreatedAsABossForRainbowBoulders { get { if (!boss) { return NPCID.Sets.ShouldBeCountedAsBossForRainbowBoulders[type]; } return true; } }` | `public bool TreatedAsABossForRainbowBoulders { get { if (!boss) { return NPCID.Sets.ShouldBeCountedAsBossForRainbowBoulders[type]; } return true; } }` |
| 1906 | property | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6671 | 2 | isLikeATownNPC | bool | `public bool isLikeATownNPC { get { if (type == 453) { return true; } return townNPC; } }` | `public bool isLikeATownNPC { get { if (type == 453) { return true; } return townNPC; } }` |
| 1907 | property | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6683 | 2 | IsMechQueenUp | bool | `public static bool IsMechQueenUp { get { if (mechQueen >= 0 && mechQueen < Main.maxNPCs) { if (Main.npc[mechQueen].active && Main.npc[mechQueen].type == 127) { return true; } mechQueen = -1; return false; } return false; } }` | `public static bool IsMechQueenUp { get { if (mechQueen >= 0 && mechQueen < Main.maxNPCs) { if (Main.npc[mechQueen].active && Main.npc[mechQueen].type == 127) { return true; } mechQueen = -1; return false; } return false; } }` |
| 1908 | property | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6700 | 2 | TooWindyForButterflies | bool | `public static bool TooWindyForButterflies => Math.Abs(Main.windSpeedTarget) >= 0.4f;` | `public static bool TooWindyForButterflies => Math.Abs(Main.windSpeedTarget) >= 0.4f;` |
| 1909 | property | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6702 | 2 | CountsAsACritter | bool | `public bool CountsAsACritter { get { if (lifeMax <= 5 && damage == 0 && type != 594) { return type != 686; } return false; } }` | `public bool CountsAsACritter { get { if (lifeMax <= 5 && damage == 0 && type != 594) { return type != 686; } return false; } }` |
| 1910 | property | Terraria.NPC | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 6714 | 2 | NetSectionCoordinates | Point | `public Point NetSectionCoordinates => new Point(Netplay.GetSectionX((int)position.X >> 4), Netplay.GetSectionY((int)position.Y >> 4));` | `public Point NetSectionCoordinates => new Point(Netplay.GetSectionX((int)position.X >> 4), Netplay.GetSectionY((int)position.Y >> 4));` |


#### 4.14.30 细分子系统：`NpcSpawnContextAndCapacityInputs`

- 细分职责：生成位置、时间、天气、玩家数量和入侵容量输入。
- 边界角色：`authoritative state/behavior`；最小 seam：Spawn System/Query seam；输入快照只读，资格结果通过命令提交。
- 成员文件数：1；声明类型数：1；字段：10；属性：0；合计：10。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（10）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1519 | field | Terraria.NPC.Spawner | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 41 | 3 | spawnSpaceX | int | `public static int spawnSpaceX = 2;` | `public static int spawnSpaceX = 2;` |
| 1520 | field | Terraria.NPC.Spawner | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 43 | 3 | spawnSpaceY | int | `public static int spawnSpaceY = 3;` | `public static int spawnSpaceY = 3;` |
| 1521 | field | Terraria.NPC.Spawner | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 45 | 3 | fairyLog | bool | `public static bool fairyLog = false;` | `public static bool fairyLog = false;` |
| 1522 | field | Terraria.NPC.Spawner | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 47 | 3 | numberOfActivePlayers | int | `public int numberOfActivePlayers;` | `public int numberOfActivePlayers;` |
| 1523 | field | Terraria.NPC.Spawner | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 49 | 3 | reachedInvasionBossCap | bool | `public bool reachedInvasionBossCap;` | `public bool reachedInvasionBossCap;` |
| 1524 | field | Terraria.NPC.Spawner | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 51 | 3 | pX | int | `public int pX;` | `public int pX;` |
| 1525 | field | Terraria.NPC.Spawner | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 53 | 3 | pY | int | `public int pY;` | `public int pY;` |
| 1526 | field | Terraria.NPC.Spawner | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 55 | 3 | luck | float | `public float luck;` | `public float luck;` |
| 1527 | field | Terraria.NPC.Spawner | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 57 | 3 | dayTime | bool | `public bool dayTime;` | `public bool dayTime;` |
| 1528 | field | Terraria.NPC.Spawner | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 59 | 3 | raining | bool | `public bool raining;` | `public bool raining;` |

##### 属性（0）

无该类型成员记录。


#### 4.14.31 细分子系统：`NpcSpawnSpatialEligibilityInputs`

- 细分职责：地表、深度、海滩、天空和树木空间资格输入。
- 边界角色：`derived/query`；最小 seam：Spawn Eligibility Query；从位置快照纯计算资格。
- 成员文件数：1；声明类型数：1；字段：10；属性：0；合计：10。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（10）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1540 | field | Terraria.NPC.Spawner | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 83 | 3 | surfaceSpawn | bool | `public bool surfaceSpawn;` | `public bool surfaceSpawn;` |
| 1541 | field | Terraria.NPC.Spawner | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 85 | 3 | spawnUndergroundDesert | bool | `public bool spawnUndergroundDesert;` | `public bool spawnUndergroundDesert;` |
| 1542 | field | Terraria.NPC.Spawner | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 87 | 3 | hardDungeon | bool | `public bool hardDungeon;` | `public bool hardDungeon;` |
| 1543 | field | Terraria.NPC.Spawner | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 89 | 3 | deeperThanRockLayer | bool | `public bool deeperThanRockLayer;` | `public bool deeperThanRockLayer;` |
| 1544 | field | Terraria.NPC.Spawner | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 91 | 3 | underGround | bool | `public bool underGround;` | `public bool underGround;` |
| 1545 | field | Terraria.NPC.Spawner | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 93 | 3 | isOcean | bool | `public bool isOcean;` | `public bool isOcean;` |
| 1546 | field | Terraria.NPC.Spawner | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 95 | 3 | isBeach | bool | `public bool isBeach;` | `public bool isBeach;` |
| 1548 | field | Terraria.NPC.Spawner | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 99 | 3 | skyBehindPlayer | bool | `public bool skyBehindPlayer;` | `public bool skyBehindPlayer;` |
| 1549 | field | Terraria.NPC.Spawner | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 101 | 3 | livingTree | bool | `public bool livingTree;` | `public bool livingTree;` |
| 1553 | field | Terraria.NPC.Spawner | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 109 | 3 | inRemixStartingArea | bool | `public bool inRemixStartingArea;` | `public bool inRemixStartingArea;` |

##### 属性（0）

无该类型成员记录。


#### 4.14.32 细分子系统：`NpcSpawnBiomeAndDungeonEligibilityInputs`

- 细分职责：水体、特殊生物群系和双地牢资格输入。
- 边界角色：`derived/query`；最小 seam：Spawn Eligibility Query；区域和地牢条件只读合并。
- 成员文件数：1；声明类型数：1；字段：6；属性：0；合计：6。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（6）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1536 | field | Terraria.NPC.Spawner | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 75 | 3 | waterTile | bool | `public bool waterTile;` | `public bool waterTile;` |
| 1537 | field | Terraria.NPC.Spawner | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 77 | 3 | nearGranite | bool | `public bool nearGranite;` | `public bool nearGranite;` |
| 1538 | field | Terraria.NPC.Spawner | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 79 | 3 | nearMarble | bool | `public bool nearMarble;` | `public bool nearMarble;` |
| 1550 | field | Terraria.NPC.Spawner | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 103 | 3 | dualDungeonsSpawnRules | bool | `public bool dualDungeonsSpawnRules;` | `public bool dualDungeonsSpawnRules;` |
| 1551 | field | Terraria.NPC.Spawner | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 105 | 3 | inDualDungeon | bool | `public bool inDualDungeon;` | `public bool inDualDungeon;` |
| 1552 | field | Terraria.NPC.Spawner | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 107 | 3 | tresspassingDualDungeon | bool | `public bool tresspassingDualDungeon;` | `public bool tresspassingDualDungeon;` |

##### 属性（0）

无该类型成员记录。


#### 4.14.33 细分子系统：`NpcSpawnPolicyAndEventEligibilityInputs`

- 细分职责：城镇、入侵、蠕虫、墙体和特殊事件政策输入。
- 边界角色：`derived/query`；最小 seam：Spawn Policy Query；政策条件不直接写入生成结果。
- 成员文件数：1；声明类型数：1；字段：11；属性：0；合计：11。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（11）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1529 | field | Terraria.NPC.Spawner | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 61 | 3 | townNPCs | int | `public int townNPCs;` | `public int townNPCs;` |
| 1530 | field | Terraria.NPC.Spawner | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 63 | 3 | skyMob | bool | `public bool skyMob;` | `public bool skyMob;` |
| 1531 | field | Terraria.NPC.Spawner | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 65 | 3 | noWorms | bool | `public bool noWorms;` | `public bool noWorms;` |
| 1532 | field | Terraria.NPC.Spawner | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 67 | 3 | noGroundWorms | bool | `public bool noGroundWorms;` | `public bool noGroundWorms;` |
| 1533 | field | Terraria.NPC.Spawner | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 69 | 3 | invaders | bool | `public bool invaders;` | `public bool invaders;` |
| 1534 | field | Terraria.NPC.Spawner | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 71 | 3 | spawnFriendly | bool | `public bool spawnFriendly;` | `public bool spawnFriendly;` |
| 1535 | field | Terraria.NPC.Spawner | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 73 | 3 | ignoreSafeWalls | bool | `public bool ignoreSafeWalls;` | `public bool ignoreSafeWalls;` |
| 1539 | field | Terraria.NPC.Spawner | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 81 | 3 | spawnSpider | bool | `public bool spawnSpider;` | `public bool spawnSpider;` |
| 1547 | field | Terraria.NPC.Spawner | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 97 | 3 | isSpawningInWindDirection | bool | `public bool isSpawningInWindDirection;` | `public bool isSpawningInWindDirection;` |
| 1554 | field | Terraria.NPC.Spawner | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 111 | 3 | offensiveToTim | bool | `public bool offensiveToTim;` | `public bool offensiveToTim;` |
| 1555 | field | Terraria.NPC.Spawner | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 113 | 3 | playerHasStartingHealth | bool | `public bool playerHasStartingHealth;` | `public bool playerHasStartingHealth;` |

##### 属性（0）

无该类型成员记录。


#### 4.14.34 细分子系统：`NpcSpawnBiomeZoneInputs`

- 细分职责：生物群系、地形和天气区域生成资格输入。
- 边界角色：`derived/query`；最小 seam：Spawn Zone Query；区域快照纯计算资格。
- 成员文件数：1；声明类型数：1；字段：13；属性：0；合计：13。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（13）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1556 | field | Terraria.NPC.Spawner | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 115 | 3 | ZoneCorrupt | bool | `public bool ZoneCorrupt;` | `public bool ZoneCorrupt;` |
| 1557 | field | Terraria.NPC.Spawner | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 117 | 3 | ZoneCrimson | bool | `public bool ZoneCrimson;` | `public bool ZoneCrimson;` |
| 1558 | field | Terraria.NPC.Spawner | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 119 | 3 | ZoneHallow | bool | `public bool ZoneHallow;` | `public bool ZoneHallow;` |
| 1559 | field | Terraria.NPC.Spawner | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 121 | 3 | ZoneJungle | bool | `public bool ZoneJungle;` | `public bool ZoneJungle;` |
| 1560 | field | Terraria.NPC.Spawner | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 123 | 3 | ZoneSnow | bool | `public bool ZoneSnow;` | `public bool ZoneSnow;` |
| 1561 | field | Terraria.NPC.Spawner | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 125 | 3 | ZoneGlowshroom | bool | `public bool ZoneGlowshroom;` | `public bool ZoneGlowshroom;` |
| 1562 | field | Terraria.NPC.Spawner | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 127 | 3 | ZoneMeteor | bool | `public bool ZoneMeteor;` | `public bool ZoneMeteor;` |
| 1563 | field | Terraria.NPC.Spawner | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 129 | 3 | ZoneGraveyard | bool | `public bool ZoneGraveyard;` | `public bool ZoneGraveyard;` |
| 1564 | field | Terraria.NPC.Spawner | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 131 | 3 | ZoneDungeon | bool | `public bool ZoneDungeon;` | `public bool ZoneDungeon;` |
| 1565 | field | Terraria.NPC.Spawner | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 133 | 3 | ZoneLihzhardTemple | bool | `public bool ZoneLihzhardTemple;` | `public bool ZoneLihzhardTemple;` |
| 1566 | field | Terraria.NPC.Spawner | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 135 | 3 | ZoneGranite | bool | `public bool ZoneGranite;` | `public bool ZoneGranite;` |
| 1567 | field | Terraria.NPC.Spawner | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 137 | 3 | ZoneMarble | bool | `public bool ZoneMarble;` | `public bool ZoneMarble;` |
| 1568 | field | Terraria.NPC.Spawner | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 139 | 3 | ZoneSandstorm | bool | `public bool ZoneSandstorm;` | `public bool ZoneSandstorm;` |

##### 属性（0）

无该类型成员记录。


#### 4.14.35 细分子系统：`NpcSpawnEventAndTowerInputs`

- 细分职责：塔、旧日军、蜡烛和事件区域生成资格输入。
- 边界角色：`derived/query`；最小 seam：Spawn Event Query；事件区域只读提供资格。
- 成员文件数：1；声明类型数：1；字段：8；属性：0；合计：8。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（8）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1569 | field | Terraria.NPC.Spawner | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 141 | 3 | ZoneTowerSolar | bool | `public bool ZoneTowerSolar;` | `public bool ZoneTowerSolar;` |
| 1570 | field | Terraria.NPC.Spawner | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 143 | 3 | ZoneTowerVortex | bool | `public bool ZoneTowerVortex;` | `public bool ZoneTowerVortex;` |
| 1571 | field | Terraria.NPC.Spawner | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 145 | 3 | ZoneTowerNebula | bool | `public bool ZoneTowerNebula;` | `public bool ZoneTowerNebula;` |
| 1572 | field | Terraria.NPC.Spawner | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 147 | 3 | ZoneTowerStardust | bool | `public bool ZoneTowerStardust;` | `public bool ZoneTowerStardust;` |
| 1573 | field | Terraria.NPC.Spawner | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 149 | 3 | ZoneOldOneArmy | bool | `public bool ZoneOldOneArmy;` | `public bool ZoneOldOneArmy;` |
| 1574 | field | Terraria.NPC.Spawner | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 151 | 3 | ZoneWaterCandle | bool | `public bool ZoneWaterCandle;` | `public bool ZoneWaterCandle;` |
| 1575 | field | Terraria.NPC.Spawner | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 153 | 3 | ZonePeaceCandle | bool | `public bool ZonePeaceCandle;` | `public bool ZonePeaceCandle;` |
| 1576 | field | Terraria.NPC.Spawner | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 155 | 3 | ZoneShadowCandle | bool | `public bool ZoneShadowCandle;` | `public bool ZoneShadowCandle;` |

##### 属性（0）

无该类型成员记录。


#### 4.14.36 细分子系统：`NpcSpawnTargetSelectionState`

- 细分职责：生成目标 NPC 选择状态。
- 边界角色：`derived/query`；最小 seam：Spawn Target Query；目标选择不修改生成上下文。
- 成员文件数：1；声明类型数：1；字段：1；属性：0；合计：1。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（1）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1577 | field | Terraria.NPC.Spawner | Terraria/NPC.cs | D:\TRbackup\Version4\Terraria\NPC.cs | 157 | 3 | defaultTarget | int | `public int defaultTarget = 255;` | `public int defaultTarget = 255;` |

##### 属性（0）

无该类型成员记录。


## 8. 本分区自检

- 叶子子系统：12 个；指定组均已写入且没有重复分区。
- 字段/属性/成员：92 / 25 / 117。
- 来源序号范围：1519..1910；本分区内唯一序号：True。
- 成员声明表逐组从源报告提取，未改写来源序号、类型、路径、行列、成员、C# 类型、成员声明或原始声明。
- 运行时读写者、生命周期、持久化、网络和调度：未验证，保留为迁移前风险。
