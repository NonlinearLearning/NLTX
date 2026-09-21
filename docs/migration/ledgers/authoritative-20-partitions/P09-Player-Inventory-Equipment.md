# P09 玩家库存、装备、Buff 槽与防御装载 - 权威模拟系统成员组件候选分区

> 来源：`docs/migration/ledgers/Version4权威模拟系统字段属性逐成员源码声明-更细子系统拆分-去除ID类文件.md`；本文件只做成员库存分区和候选组件边界记录。
> 这是候选组件拆分台账，不是运行时迁移完成、行为等价、API 兼容、网络闭合或持久化闭合证明。

## 1. 分区边界

本分区包含 8 个叶子子系统，字段 105 条、属性 0 条、成员合计 105 条；每个叶子子系统保持原正式父级归属，不跨分区拆行。

## 2. 分区覆盖概览

| 叶子子系统 | 正式父级 | 字段 | 属性 | 合计 | 源边界角色 |
|---|---|---:|---:|---:|---|
| `PlayerInventoryAndContainerSlots` | `PlayerGameplay` | 8 | 0 | 8 | authoritative state/behavior |
| `PlayerEquipmentAndDyeSlots` | `PlayerGameplay` | 4 | 0 | 4 | authoritative state/behavior |
| `PlayerBuffAndResourceSlots` | `PlayerGameplay` | 11 | 0 | 11 | authoritative state/behavior |
| `PlayerEquipmentPresentationState` | `PlayerGameplay` | 20 | 0 | 20 | registry/projection |
| `PlayerEquipmentSelectionSlots` | `PlayerGameplay` | 21 | 0 | 21 | authoritative state/behavior |
| `PlayerAppearanceSelectionState` | `PlayerGameplay` | 6 | 0 | 6 | registry/projection |
| `PlayerEquipmentColorProjection` | `PlayerGameplay` | 20 | 0 | 20 | registry/projection |
| `PlayerDefenseLoadoutAndCloneState` | `PlayerGameplay` | 15 | 0 | 15 | authoritative state/behavior |

## 3. 候选组件归属与 seam

| 叶子子系统 | 候选组件边界 | 类型分类 | 最小 seam | 独立生命周期 | 证据状态 | 主要风险 |
|---|---|---|---|---|---|---|
| `PlayerInventoryAndContainerSlots` | `PlayerInventoryAndContainerSlotsComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerEquipmentAndDyeSlots` | `PlayerEquipmentAndDyeSlotsComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerBuffAndResourceSlots` | `PlayerBuffAndResourceSlotsComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerEquipmentPresentationState` | `PlayerEquipmentPresentationStateProjection`/`Adapter` | Projection/Adapter | 单向 Projection/Adapter seam；不得回写权威状态 | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerEquipmentSelectionSlots` | `PlayerEquipmentSelectionSlotsComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerAppearanceSelectionState` | `PlayerAppearanceSelectionStateProjection`/`Adapter` | Projection/Adapter | 单向 Projection/Adapter seam；不得回写权威状态 | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerEquipmentColorProjection` | `PlayerEquipmentColorProjectionProjection`/`Adapter` | Projection/Adapter | 单向 Projection/Adapter seam；不得回写权威状态 | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `PlayerDefenseLoadoutAndCloneState` | `PlayerDefenseLoadoutAndCloneStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |

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
- 本分区父级局部统计：字段 105；属性 0；合计 105；完整父级统计以源报告为准。

#### 4.13.28 细分子系统：`PlayerInventoryAndContainerSlots`

- 细分职责：玩家背包、银行、垃圾栏和虚空储存槽。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；容器写入集中在物品命令边界。
- 成员文件数：1；声明类型数：1；字段：8；属性：0；合计：8。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（8）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 712 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1021 | 2 | trashItem | Terraria.Item | `public Item trashItem = new Item();` | `public Item trashItem = new Item();` |
| 743 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1083 | 2 | inventory | Terraria.Item[] | `public Item[] inventory = new Item[59];` | `public Item[] inventory = new Item[59];` |
| 744 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1085 | 2 | inventoryChestStack | bool[] | `public bool[] inventoryChestStack = new bool[59];` | `public bool[] inventoryChestStack = new bool[59];` |
| 746 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1089 | 2 | bank | Terraria.Chest | `public readonly Chest bank = Chest.CreateBank(-2);` | `public readonly Chest bank = Chest.CreateBank(-2);` |
| 747 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1091 | 2 | bank2 | Terraria.Chest | `public readonly Chest bank2 = Chest.CreateBank(-3);` | `public readonly Chest bank2 = Chest.CreateBank(-3);` |
| 748 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1093 | 2 | bank3 | Terraria.Chest | `public readonly Chest bank3 = Chest.CreateBank(-4);` | `public readonly Chest bank3 = Chest.CreateBank(-4);` |
| 749 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1095 | 2 | bank4 | Terraria.Chest | `public readonly Chest bank4 = Chest.CreateBank(-5);` | `public readonly Chest bank4 = Chest.CreateBank(-5);` |
| 750 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1097 | 2 | voidVaultInfo | Terraria.BitsByte | `public BitsByte voidVaultInfo;` | `public BitsByte voidVaultInfo;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.29 细分子系统：`PlayerEquipmentAndDyeSlots`

- 细分职责：装备、染料和杂项装备槽。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；装备变更通过显式装备命令提交。
- 成员文件数：1；声明类型数：1；字段：4；属性：0；合计：4。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（4）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 708 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1013 | 2 | armor | Terraria.Item[] | `public Item[] armor = new Item[20];` | `public Item[] armor = new Item[20];` |
| 709 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1015 | 2 | dye | Terraria.Item[] | `public Item[] dye = new Item[10];` | `public Item[] dye = new Item[10];` |
| 710 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1017 | 2 | miscEquips | Terraria.Item[] | `public Item[] miscEquips = new Item[5];` | `public Item[] miscEquips = new Item[5];` |
| 711 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1019 | 2 | miscDyes | Terraria.Item[] | `public Item[] miscDyes = new Item[5];` | `public Item[] miscDyes = new Item[5];` |

##### 属性（0）

无该类型成员记录。


#### 4.13.30 细分子系统：`PlayerBuffAndResourceSlots`

- 细分职责：Buff 槽、呼吸、岩浆和水体资源槽。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；资源计时由状态系统推进。
- 成员文件数：1；声明类型数：1；字段：11；属性：0；合计：11。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（11）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 715 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1027 | 2 | maxBuffs | int | `public static readonly int maxBuffs = 44;` | `public static readonly int maxBuffs = 44;` |
| 716 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1029 | 2 | buffType | int[] | `public int[] buffType = new int[maxBuffs];` | `public int[] buffType = new int[maxBuffs];` |
| 717 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1031 | 2 | buffTime | int[] | `public int[] buffTime = new int[maxBuffs];` | `public int[] buffTime = new int[maxBuffs];` |
| 718 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1033 | 2 | buffImmune | bool[] | `public bool[] buffImmune = new bool[BuffID.Count];` | `public bool[] buffImmune = new bool[BuffID.Count];` |
| 720 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1037 | 2 | breathMax | int | `public int breathMax = 200;` | `public int breathMax = 200;` |
| 721 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1039 | 2 | breath | int | `public int breath = 200;` | `public int breath = 200;` |
| 722 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1041 | 2 | lavaMax | int | `public int lavaMax;` | `public int lavaMax;` |
| 723 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1043 | 2 | lavaTime | int | `public int lavaTime;` | `public int lavaTime;` |
| 724 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1045 | 2 | ignoreWater | bool | `public bool ignoreWater;` | `public bool ignoreWater;` |
| 725 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1047 | 2 | lavaVision | bool | `public bool lavaVision;` | `public bool lavaVision;` |
| 726 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1049 | 2 | lavaOpacity | float | `public float lavaOpacity = 1f;` | `public float lavaOpacity = 1f;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.31 细分子系统：`PlayerEquipmentPresentationState`

- 细分职责：装备效果表现、潜行和展示实体投影状态。
- 边界角色：`registry/projection`；最小 seam：Registry/Projection seam；表现读取不能反向修改装备权威状态。
- 成员文件数：1；声明类型数：1；字段：20；属性：0；合计：20。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（20）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 713 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1023 | 2 | itemRotation | float | `public float itemRotation;` | `public float itemRotation;` |
| 714 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1025 | 2 | itemLocation | Vector2 | `public Vector2 itemLocation;` | `public Vector2 itemLocation;` |
| 719 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1035 | 2 | heldProj | int | `public int heldProj = -1;` | `public int heldProj = -1;` |
| 727 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1051 | 2 | armorEffectDrawShadow | bool | `public bool armorEffectDrawShadow;` | `public bool armorEffectDrawShadow;` |
| 728 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1053 | 2 | armorEffectDrawShadowSubtle | bool | `public bool armorEffectDrawShadowSubtle;` | `public bool armorEffectDrawShadowSubtle;` |
| 729 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1055 | 2 | armorEffectDrawOutlines | bool | `public bool armorEffectDrawOutlines;` | `public bool armorEffectDrawOutlines;` |
| 730 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1057 | 2 | armorEffectDrawShadowLokis | bool | `public bool armorEffectDrawShadowLokis;` | `public bool armorEffectDrawShadowLokis;` |
| 731 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1059 | 2 | armorEffectDrawShadowBasilisk | bool | `public bool armorEffectDrawShadowBasilisk;` | `public bool armorEffectDrawShadowBasilisk;` |
| 732 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1061 | 2 | armorEffectDrawOutlinesForbidden | bool | `public bool armorEffectDrawOutlinesForbidden;` | `public bool armorEffectDrawOutlinesForbidden;` |
| 733 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1063 | 2 | armorEffectDrawShadowEOCShield | bool | `public bool armorEffectDrawShadowEOCShield;` | `public bool armorEffectDrawShadowEOCShield;` |
| 734 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1065 | 2 | socialShadowRocketBoots | bool | `public bool socialShadowRocketBoots;` | `public bool socialShadowRocketBoots;` |
| 735 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1067 | 2 | socialGhost | bool | `public bool socialGhost;` | `public bool socialGhost;` |
| 736 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1069 | 2 | shroomiteStealth | bool | `public bool shroomiteStealth;` | `public bool shroomiteStealth;` |
| 737 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1071 | 2 | ashWoodBonus | bool | `public bool ashWoodBonus;` | `public bool ashWoodBonus;` |
| 738 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1073 | 2 | socialIgnoreLight | bool | `public bool socialIgnoreLight;` | `public bool socialIgnoreLight;` |
| 739 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1075 | 2 | stealthTimer | int | `public int stealthTimer;` | `public int stealthTimer;` |
| 740 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1077 | 2 | stealth | float | `public float stealth = 1f;` | `public float stealth = 1f;` |
| 741 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1079 | 2 | isDisplayDollOrInanimate | bool | `public bool isDisplayDollOrInanimate;` | `public bool isDisplayDollOrInanimate;` |
| 742 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1081 | 2 | isHatRackDoll | bool | `public bool isHatRackDoll;` | `public bool isHatRackDoll;` |
| 745 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1087 | 2 | lastVisualizedSelectedItem | Terraria.Item | `public Item lastVisualizedSelectedItem;` | `public Item lastVisualizedSelectedItem;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.35 细分子系统：`PlayerEquipmentSelectionSlots`

- 细分职责：头身手部、饰品、背部和面部装备选择槽。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System/CommitPort；装备选择命令集中写入。
- 成员文件数：1；声明类型数：1；字段：21；属性：0；合计：21。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（21）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 783 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1164 | 2 | head | int | `public int head = -1;` | `public int head = -1;` |
| 784 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1166 | 2 | body | int | `public int body = -1;` | `public int body = -1;` |
| 785 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1168 | 2 | legs | int | `public int legs = -1;` | `public int legs = -1;` |
| 786 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1170 | 2 | coat | int | `public int coat = -1;` | `public int coat = -1;` |
| 787 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1172 | 2 | handon | sbyte | `public sbyte handon = -1;` | `public sbyte handon = -1;` |
| 788 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1174 | 2 | handoff | sbyte | `public sbyte handoff = -1;` | `public sbyte handoff = -1;` |
| 789 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1176 | 2 | back | sbyte | `public sbyte back = -1;` | `public sbyte back = -1;` |
| 790 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1178 | 2 | front | sbyte | `public sbyte front = -1;` | `public sbyte front = -1;` |
| 791 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1180 | 2 | shoe | sbyte | `public sbyte shoe = -1;` | `public sbyte shoe = -1;` |
| 792 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1182 | 2 | waist | sbyte | `public sbyte waist = -1;` | `public sbyte waist = -1;` |
| 793 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1184 | 2 | shield | sbyte | `public sbyte shield = -1;` | `public sbyte shield = -1;` |
| 794 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1186 | 2 | neck | sbyte | `public sbyte neck = -1;` | `public sbyte neck = -1;` |
| 795 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1188 | 2 | face | sbyte | `public sbyte face = -1;` | `public sbyte face = -1;` |
| 796 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1190 | 2 | balloon | sbyte | `public sbyte balloon = -1;` | `public sbyte balloon = -1;` |
| 797 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1192 | 2 | backpack | sbyte | `public sbyte backpack = -1;` | `public sbyte backpack = -1;` |
| 798 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1194 | 2 | tail | sbyte | `public sbyte tail = -1;` | `public sbyte tail = -1;` |
| 799 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1196 | 2 | faceHead | sbyte | `public sbyte faceHead = -1;` | `public sbyte faceHead = -1;` |
| 800 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1198 | 2 | faceFlower | sbyte | `public sbyte faceFlower = -1;` | `public sbyte faceFlower = -1;` |
| 801 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1200 | 2 | faceMask | sbyte | `public sbyte faceMask = -1;` | `public sbyte faceMask = -1;` |
| 802 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1202 | 2 | balloonFront | sbyte | `public sbyte balloonFront = -1;` | `public sbyte balloonFront = -1;` |
| 803 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1204 | 2 | beard | sbyte | `public sbyte beard = -1;` | `public sbyte beard = -1;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.36 细分子系统：`PlayerAppearanceSelectionState`

- 细分职责：跳跃帧、语音覆盖、隐藏配饰和身体动画帧选择状态。
- 边界角色：`registry/projection`；最小 seam：Registry/Projection seam；表现投影只读取选择状态。
- 成员文件数：1；声明类型数：1；字段：6；属性：0；合计：6。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（6）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 782 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1162 | 2 | jump | int | `public int jump;` | `public int jump;` |
| 804 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1206 | 2 | voiceOverride | sbyte | `public sbyte voiceOverride;` | `public sbyte voiceOverride;` |
| 805 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1208 | 2 | hideVisibleAccessory | bool[] | `public bool[] hideVisibleAccessory = new bool[10];` | `public bool[] hideVisibleAccessory = new bool[10];` |
| 806 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1210 | 2 | hideMisc | Terraria.BitsByte | `public BitsByte hideMisc = (byte)0;` | `public BitsByte hideMisc = (byte)0;` |
| 807 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1214 | 2 | bodyFrame | Rectangle | `public Rectangle bodyFrame;` | `public Rectangle bodyFrame;` |
| 808 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 1216 | 2 | legFrame | Rectangle | `public Rectangle legFrame;` | `public Rectangle legFrame;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.83 细分子系统：`PlayerEquipmentColorProjection`

- 细分职责：头身手部、饰品和面部装备的颜色投影槽。
- 边界角色：`registry/projection`；最小 seam：Registry/Projection seam；颜色快照不反向修改装备状态。
- 成员文件数：1；声明类型数：1；字段：20；属性：0；合计：20。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（20）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1326 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2298 | 2 | cHead | int | `public int cHead;` | `public int cHead;` |
| 1327 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2300 | 2 | cBody | int | `public int cBody;` | `public int cBody;` |
| 1328 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2302 | 2 | cLegs | int | `public int cLegs;` | `public int cLegs;` |
| 1329 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2304 | 2 | cHandOn | int | `public int cHandOn;` | `public int cHandOn;` |
| 1330 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2306 | 2 | cHandOff | int | `public int cHandOff;` | `public int cHandOff;` |
| 1331 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2308 | 2 | cBack | int | `public int cBack;` | `public int cBack;` |
| 1332 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2310 | 2 | cFront | int | `public int cFront;` | `public int cFront;` |
| 1333 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2312 | 2 | cShoe | int | `public int cShoe;` | `public int cShoe;` |
| 1334 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2314 | 2 | cWaist | int | `public int cWaist;` | `public int cWaist;` |
| 1335 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2316 | 2 | cShield | int | `public int cShield;` | `public int cShield;` |
| 1336 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2318 | 2 | cNeck | int | `public int cNeck;` | `public int cNeck;` |
| 1337 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2320 | 2 | cFace | int | `public int cFace;` | `public int cFace;` |
| 1338 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2322 | 2 | cFaceHead | int | `public int cFaceHead;` | `public int cFaceHead;` |
| 1339 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2324 | 2 | cFaceFlower | int | `public int cFaceFlower;` | `public int cFaceFlower;` |
| 1340 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2326 | 2 | cFaceMask | int | `public int cFaceMask;` | `public int cFaceMask;` |
| 1341 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2328 | 2 | cBalloon | int | `public int cBalloon;` | `public int cBalloon;` |
| 1342 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2330 | 2 | cBalloonFront | int | `public int cBalloonFront;` | `public int cBalloonFront;` |
| 1346 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2338 | 2 | cBackpack | int | `public int cBackpack;` | `public int cBackpack;` |
| 1347 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2340 | 2 | cTail | int | `public int cTail;` | `public int cTail;` |
| 1348 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2342 | 2 | cShieldFallback | int | `public int cShieldFallback;` | `public int cShieldFallback;` |

##### 属性（0）

无该类型成员记录。


#### 4.13.90 细分子系统：`PlayerDefenseLoadoutAndCloneState`

- 细分职责：盾牌招架、伤害冷却、装备方案和视觉克隆适配资源。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System 读取输入并通过显式 Command/CommitPort 写入；跨组只用事件、Query 或命令交接。
- 成员文件数：1；声明类型数：1；字段：15；属性：0；合计：15。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（15）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1402 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2461 | 2 | hasRaisableShield | bool | `public bool hasRaisableShield;` | `public bool hasRaisableShield;` |
| 1403 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2463 | 2 | shieldRaised | bool | `public bool shieldRaised;` | `public bool shieldRaised;` |
| 1404 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2465 | 2 | shieldParryTimeLeft | int | `public int shieldParryTimeLeft;` | `public int shieldParryTimeLeft;` |
| 1405 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2467 | 2 | shield_parry_cooldown | int | `public int shield_parry_cooldown;` | `public int shield_parry_cooldown;` |
| 1406 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2471 | 2 | _lockTileInteractionsTimer | int | `private int _lockTileInteractionsTimer;` | `private int _lockTileInteractionsTimer;` |
| 1407 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2473 | 2 | hoveredChestIndex | int | `public int hoveredChestIndex = -1;` | `public int hoveredChestIndex = -1;` |
| 1408 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2475 | 2 | hurtCooldowns | int[] | `public int[] hurtCooldowns = new int[ImmunityCooldownID.Count];` | `public int[] hurtCooldowns = new int[ImmunityCooldownID.Count];` |
| 1409 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2477 | 2 | GetItemLogger | Terraria.DataStructures.PlayerGetItemLogger | `public static PlayerGetItemLogger GetItemLogger = new PlayerGetItemLogger();` | `public static PlayerGetItemLogger GetItemLogger = new PlayerGetItemLogger();` |
| 1410 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2481 | 2 | meleeNPCHitCooldown | int[] | `public int[] meleeNPCHitCooldown = new int[Main.maxNPCs];` | `public int[] meleeNPCHitCooldown = new int[Main.maxNPCs];` |
| 1411 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2484 | 2 | Loadouts | Terraria.EquipmentLoadout[] | `public EquipmentLoadout[] Loadouts = new EquipmentLoadout[3]  	{  		new EquipmentLoadout(),  		new EquipmentLoadout(),  		new EquipmentLoadout()  	};` | `public EquipmentLoadout[] Loadouts = new EquipmentLoadout[3] { new EquipmentLoadout(), new EquipmentLoadout(), new EquipmentLoadout() };` |
| 1412 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2491 | 2 | CurrentLoadoutIndex | int | `public int CurrentLoadoutIndex;` | `public int CurrentLoadoutIndex;` |
| 1413 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2495 | 2 | _visualCloneDummyData | Terraria.IO.PlayerFileData | `private static readonly PlayerFileData _visualCloneDummyData = new PlayerFileData();` | `private static readonly PlayerFileData _visualCloneDummyData = new PlayerFileData();` |
| 1414 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2497 | 2 | _visualCloneStream | System.IO.MemoryStream | `private static readonly MemoryStream _visualCloneStream = new MemoryStream();` | `private static readonly MemoryStream _visualCloneStream = new MemoryStream();` |
| 1415 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2499 | 2 | _visualCloneWriter | System.IO.BinaryWriter | `private static readonly BinaryWriter _visualCloneWriter = new BinaryWriter(_visualCloneStream);` | `private static readonly BinaryWriter _visualCloneWriter = new BinaryWriter(_visualCloneStream);` |
| 1416 | field | Terraria.Player | Terraria/Player.cs | D:\TRbackup\Version4\Terraria\Player.cs | 2501 | 2 | _visualCloneReader | System.IO.BinaryReader | `private static readonly BinaryReader _visualCloneReader = new BinaryReader(_visualCloneStream);` | `private static readonly BinaryReader _visualCloneReader = new BinaryReader(_visualCloneStream);` |

##### 属性（0）

无该类型成员记录。


## 8. 本分区自检

- 叶子子系统：8 个；指定组均已写入且没有重复分区。
- 字段/属性/成员：105 / 0 / 105。
- 来源序号范围：708..1416；本分区内唯一序号：True。
- 成员声明表逐组从源报告提取，未改写来源序号、类型、路径、行列、成员、C# 类型、成员声明或原始声明。
- 运行时读写者、生命周期、持久化、网络和调度：未验证，保留为迁移前风险。
