# P02 拴系实体注册、行为与物种定义 - 权威模拟系统成员组件候选分区

> 来源：`docs/migration/ledgers/Version4权威模拟系统字段属性逐成员源码声明-更细子系统拆分-去除ID类文件.md`；本文件只做成员库存分区和候选组件边界记录。
> 这是候选组件拆分台账，不是运行时迁移完成、行为等价、API 兼容、网络闭合或持久化闭合证明。

## 1. 分区边界

本分区包含 8 个叶子子系统，字段 93 条、属性 5 条、成员合计 98 条；每个叶子子系统保持原正式父级归属，不跨分区拆行。

## 2. 分区覆盖概览

| 叶子子系统 | 正式父级 | 字段 | 属性 | 合计 | 源边界角色 |
|---|---|---:|---:|---:|---|
| `LeashedRegistryAndSections` | `LeashedEntitySimulation` | 12 | 3 | 15 | registry/projection |
| `LeashedCritterCoreState` | `LeashedEntitySimulation` | 15 | 0 | 15 | authoritative state/behavior |
| `LeashedWalkerBehavior` | `LeashedEntitySimulation` | 7 | 0 | 7 | authoritative state/behavior |
| `LeashedJumperBehavior` | `LeashedEntitySimulation` | 11 | 0 | 11 | authoritative state/behavior |
| `LeashedFlyerBehavior` | `LeashedEntitySimulation` | 11 | 0 | 11 | authoritative state/behavior |
| `LeashedKiteBehavior` | `LeashedEntitySimulation` | 19 | 1 | 20 | authoritative state/behavior |
| `LeashedButterflyVariants` | `LeashedEntitySimulation` | 5 | 1 | 6 | authoritative state/behavior |
| `LeashedSpeciesPrototypes` | `LeashedEntitySimulation` | 13 | 0 | 13 | authoritative state/behavior |

## 3. 候选组件归属与 seam

| 叶子子系统 | 候选组件边界 | 类型分类 | 最小 seam | 独立生命周期 | 证据状态 | 主要风险 |
|---|---|---|---|---|---|---|
| `LeashedRegistryAndSections` | `LeashedRegistryAndSectionsProjection`/`Adapter` | Projection/Adapter | 单向 Projection/Adapter seam；不得回写权威状态 | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `LeashedCritterCoreState` | `LeashedCritterCoreStateComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `LeashedWalkerBehavior` | `LeashedWalkerBehaviorComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `LeashedJumperBehavior` | `LeashedJumperBehaviorComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `LeashedFlyerBehavior` | `LeashedFlyerBehaviorComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `LeashedKiteBehavior` | `LeashedKiteBehaviorComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `LeashedButterflyVariants` | `LeashedButterflyVariantsComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |
| `LeashedSpeciesPrototypes` | `LeashedSpeciesPrototypesComponent` | Component | 唯一 Owner System/CommitPort | 待源码读写与生命周期闭合 | source-inventory-confirmed；运行时 ownership partial | 读者、写者、持久化、网络和调度证据仍未完全闭合 |

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

### 正式父级子系统：`LeashedEntitySimulation`
- 父级职责：沿用源报告正式父级 `LeashedEntitySimulation`；本分区只收录该父级中列出的叶子子系统。
- 本分区父级局部统计：字段 93；属性 5；合计 98；完整父级统计以源报告为准。

#### 4.3.1 细分子系统：`LeashedRegistryAndSections`

- 细分职责：拴系实体注册、活动区段索引和槽位生命周期。
- 边界角色：`registry/projection`；最小 seam：Registry/Projection seam；提交后的事实单向输出，不把网络/UI/索引反写成权威状态。
- 成员文件数：1；声明类型数：3；字段：12；属性：3；合计：15。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（12）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 141 | field | Terraria.GameContent.LeashedEntity.Registry | Terraria.GameContent/LeashedEntity.cs | D:\TRbackup\Version4\Terraria.GameContent\LeashedEntity.cs | 44 | 3 | Prototypes | System.Collections.Generic.List<Terraria.GameContent.LeashedEntity> | `private static readonly List<LeashedEntity> Prototypes = new List<LeashedEntity>();` | `private static readonly List<LeashedEntity> Prototypes = new List<LeashedEntity>();` |
| 142 | field | Terraria.GameContent.LeashedEntity.SectionEntityList | Terraria.GameContent/LeashedEntity.cs | D:\TRbackup\Version4\Terraria.GameContent\LeashedEntity.cs | 96 | 3 | coordinates | Point | `public readonly Point coordinates;` | `public readonly Point coordinates;` |
| 143 | field | Terraria.GameContent.LeashedEntity.SectionEntityList | Terraria.GameContent/LeashedEntity.cs | D:\TRbackup\Version4\Terraria.GameContent\LeashedEntity.cs | 98 | 3 | active | bool | `public bool active;` | `public bool active;` |
| 144 | field | Terraria.GameContent.LeashedEntity.SectionEntityList | Terraria.GameContent/LeashedEntity.cs | D:\TRbackup\Version4\Terraria.GameContent\LeashedEntity.cs | 100 | 3 | list | Terraria.GameContent.LeashedEntity[] | `public LeashedEntity[] list = new LeashedEntity[32];` | `public LeashedEntity[] list = new LeashedEntity[32];` |
| 145 | field | Terraria.GameContent.LeashedEntity.SectionEntityList | Terraria.GameContent/LeashedEntity.cs | D:\TRbackup\Version4\Terraria.GameContent\LeashedEntity.cs | 102 | 3 | count | int | `public int count;` | `public int count;` |
| 146 | field | Terraria.GameContent.LeashedEntity.SectionEntityList | Terraria.GameContent/LeashedEntity.cs | D:\TRbackup\Version4\Terraria.GameContent\LeashedEntity.cs | 104 | 3 | emptySlots | int | `private int emptySlots;` | `private int emptySlots;` |
| 147 | field | Terraria.GameContent.LeashedEntity | Terraria.GameContent/LeashedEntity.cs | D:\TRbackup\Version4\Terraria.GameContent\LeashedEntity.cs | 177 | 2 | BySection | Terraria.GameContent.LeashedEntity.SectionEntityList[,] | `private static readonly SectionEntityList[,] BySection;` | `private static readonly SectionEntityList[,] BySection;` |
| 148 | field | Terraria.GameContent.LeashedEntity | Terraria.GameContent/LeashedEntity.cs | D:\TRbackup\Version4\Terraria.GameContent\LeashedEntity.cs | 179 | 2 | ActiveSectionList | System.Collections.Generic.List<Terraria.GameContent.LeashedEntity.SectionEntityList> | `private static readonly List<SectionEntityList> ActiveSectionList;` | `private static readonly List<SectionEntityList> ActiveSectionList;` |
| 149 | field | Terraria.GameContent.LeashedEntity | Terraria.GameContent/LeashedEntity.cs | D:\TRbackup\Version4\Terraria.GameContent\LeashedEntity.cs | 181 | 2 | ByWhoAmI | System.Collections.Generic.List<Terraria.GameContent.LeashedEntity> | `private static readonly List<LeashedEntity> ByWhoAmI;` | `private static readonly List<LeashedEntity> ByWhoAmI;` |
| 150 | field | Terraria.GameContent.LeashedEntity | Terraria.GameContent/LeashedEntity.cs | D:\TRbackup\Version4\Terraria.GameContent\LeashedEntity.cs | 183 | 2 | sectionSlot | int | `private int sectionSlot;` | `private int sectionSlot;` |
| 151 | field | Terraria.GameContent.LeashedEntity | Terraria.GameContent/LeashedEntity.cs | D:\TRbackup\Version4\Terraria.GameContent\LeashedEntity.cs | 185 | 2 | active | bool | `public bool active;` | `public bool active;` |
| 152 | field | Terraria.GameContent.LeashedEntity | Terraria.GameContent/LeashedEntity.cs | D:\TRbackup\Version4\Terraria.GameContent\LeashedEntity.cs | 187 | 2 | whoAmI | int | `public int whoAmI;` | `public int whoAmI;` |

##### 属性（3）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 155 | property | Terraria.GameContent.LeashedEntity | Terraria.GameContent/LeashedEntity.cs | D:\TRbackup\Version4\Terraria.GameContent\LeashedEntity.cs | 190 | 2 | Type | int | `public int Type { get; private set; }` | `public int Type { get; private set; }` |
| 156 | property | Terraria.GameContent.LeashedEntity | Terraria.GameContent/LeashedEntity.cs | D:\TRbackup\Version4\Terraria.GameContent\LeashedEntity.cs | 192 | 2 | AnchorPosition | Terraria.DataStructures.Point16 | `public Point16 AnchorPosition { get; private set; }` | `public Point16 AnchorPosition { get; private set; }` |
| 157 | property | Terraria.GameContent.LeashedEntity | Terraria.GameContent/LeashedEntity.cs | D:\TRbackup\Version4\Terraria.GameContent\LeashedEntity.cs | 194 | 2 | SectionCoordinates | Point | `public Point SectionCoordinates => new Point(Netplay.GetSectionX(AnchorPosition.X), Netplay.GetSectionY(AnchorPosition.Y));` | `public Point SectionCoordinates => new Point(Netplay.GetSectionX(AnchorPosition.X), Netplay.GetSectionY(AnchorPosition.Y));` |


#### 4.3.2 细分子系统：`LeashedCritterCoreState`

- 细分职责：拴系生物通用锚点、行为状态、目标坐标和水生标记。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System 读取输入并通过显式 Command/CommitPort 写入；跨组只用事件、Query 或命令交接。
- 成员文件数：1；声明类型数：1；字段：15；属性：0；合计：15。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（15）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 93 | field | Terraria.GameContent.LeashedEntities.LeashedCritter | Terraria.GameContent.LeashedEntities/LeashedCritter.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\LeashedCritter.cs | 13 | 2 | _dummy | Terraria.NPC | `protected static NPC _dummy = new NPC();` | `protected static NPC _dummy = new NPC();` |
| 94 | field | Terraria.GameContent.LeashedEntities.LeashedCritter | Terraria.GameContent.LeashedEntities/LeashedCritter.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\LeashedCritter.cs | 15 | 2 | anchorStyle | int | `public int anchorStyle;` | `public int anchorStyle;` |
| 95 | field | Terraria.GameContent.LeashedEntities.LeashedCritter | Terraria.GameContent.LeashedEntities/LeashedCritter.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\LeashedCritter.cs | 17 | 2 | npcType | int | `protected int npcType;` | `protected int npcType;` |
| 96 | field | Terraria.GameContent.LeashedEntities.LeashedCritter | Terraria.GameContent.LeashedEntities/LeashedCritter.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\LeashedCritter.cs | 19 | 2 | spriteDirection | int | `protected int spriteDirection;` | `protected int spriteDirection;` |
| 97 | field | Terraria.GameContent.LeashedEntities.LeashedCritter | Terraria.GameContent.LeashedEntities/LeashedCritter.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\LeashedCritter.cs | 21 | 2 | frame | Rectangle | `protected Rectangle frame;` | `protected Rectangle frame;` |
| 98 | field | Terraria.GameContent.LeashedEntities.LeashedCritter | Terraria.GameContent.LeashedEntities/LeashedCritter.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\LeashedCritter.cs | 23 | 2 | frameCounter | double | `protected double frameCounter;` | `protected double frameCounter;` |
| 99 | field | Terraria.GameContent.LeashedEntities.LeashedCritter | Terraria.GameContent.LeashedEntities/LeashedCritter.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\LeashedCritter.cs | 25 | 2 | rand | Terraria.Utilities.LCG32Random | `protected LCG32Random rand;` | `protected LCG32Random rand;` |
| 100 | field | Terraria.GameContent.LeashedEntities.LeashedCritter | Terraria.GameContent.LeashedEntities/LeashedCritter.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\LeashedCritter.cs | 27 | 2 | WaitTime | short | `protected short WaitTime;` | `protected short WaitTime;` |
| 101 | field | Terraria.GameContent.LeashedEntities.LeashedCritter | Terraria.GameContent.LeashedEntities/LeashedCritter.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\LeashedCritter.cs | 29 | 2 | State | byte | `protected byte State;` | `protected byte State;` |
| 102 | field | Terraria.GameContent.LeashedEntities.LeashedCritter | Terraria.GameContent.LeashedEntities/LeashedCritter.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\LeashedCritter.cs | 31 | 2 | TargetPosition | Terraria.DataStructures.Point16 | `protected Point16 TargetPosition;` | `protected Point16 TargetPosition;` |
| 103 | field | Terraria.GameContent.LeashedEntities.LeashedCritter | Terraria.GameContent.LeashedEntities/LeashedCritter.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\LeashedCritter.cs | 33 | 2 | netOffset | Vector2 | `protected Vector2 netOffset;` | `protected Vector2 netOffset;` |
| 104 | field | Terraria.GameContent.LeashedEntities.LeashedCritter | Terraria.GameContent.LeashedEntities/LeashedCritter.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\LeashedCritter.cs | 35 | 2 | scale | float | `protected float scale = 1f;` | `protected float scale = 1f;` |
| 105 | field | Terraria.GameContent.LeashedEntities.LeashedCritter | Terraria.GameContent.LeashedEntities/LeashedCritter.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\LeashedCritter.cs | 37 | 2 | strayingRangeInBlocks | int | `protected int strayingRangeInBlocks;` | `protected int strayingRangeInBlocks;` |
| 106 | field | Terraria.GameContent.LeashedEntities.LeashedCritter | Terraria.GameContent.LeashedEntities/LeashedCritter.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\LeashedCritter.cs | 39 | 2 | isAquatic | bool | `protected bool isAquatic;` | `protected bool isAquatic;` |
| 107 | field | Terraria.GameContent.LeashedEntities.LeashedCritter | Terraria.GameContent.LeashedEntities/LeashedCritter.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\LeashedCritter.cs | 41 | 2 | RecallDuration | int | `protected const int RecallDuration = 20;` | `protected const int RecallDuration = 20;` |

##### 属性（0）

无该类型成员记录。


#### 4.3.3 细分子系统：`LeashedWalkerBehavior`

- 细分职责：步行拴系生物的站立、选向、行走、坠落和召回状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System 读取输入并通过显式 Command/CommitPort 写入；跨组只用事件、Query 或命令交接。
- 成员文件数：1；声明类型数：1；字段：7；属性：0；合计：7。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（7）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 132 | field | Terraria.GameContent.LeashedEntities.WalkerLeashedCritter | Terraria.GameContent.LeashedEntities/WalkerLeashedCritter.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\WalkerLeashedCritter.cs | 9 | 2 | Prototype | Terraria.GameContent.LeashedEntities.WalkerLeashedCritter | `public static WalkerLeashedCritter Prototype = new WalkerLeashedCritter();` | `public static WalkerLeashedCritter Prototype = new WalkerLeashedCritter();` |
| 133 | field | Terraria.GameContent.LeashedEntities.WalkerLeashedCritter | Terraria.GameContent.LeashedEntities/WalkerLeashedCritter.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\WalkerLeashedCritter.cs | 11 | 2 | State_Standing | int | `private const int State_Standing = 0;` | `private const int State_Standing = 0;` |
| 134 | field | Terraria.GameContent.LeashedEntities.WalkerLeashedCritter | Terraria.GameContent.LeashedEntities/WalkerLeashedCritter.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\WalkerLeashedCritter.cs | 13 | 2 | State_PickDirection | int | `private const int State_PickDirection = 1;` | `private const int State_PickDirection = 1;` |
| 135 | field | Terraria.GameContent.LeashedEntities.WalkerLeashedCritter | Terraria.GameContent.LeashedEntities/WalkerLeashedCritter.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\WalkerLeashedCritter.cs | 15 | 2 | State_Walking | int | `private const int State_Walking = 2;` | `private const int State_Walking = 2;` |
| 136 | field | Terraria.GameContent.LeashedEntities.WalkerLeashedCritter | Terraria.GameContent.LeashedEntities/WalkerLeashedCritter.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\WalkerLeashedCritter.cs | 17 | 2 | State_Falling | int | `private const int State_Falling = 3;` | `private const int State_Falling = 3;` |
| 137 | field | Terraria.GameContent.LeashedEntities.WalkerLeashedCritter | Terraria.GameContent.LeashedEntities/WalkerLeashedCritter.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\WalkerLeashedCritter.cs | 19 | 2 | State_Recalling | int | `private const int State_Recalling = 4;` | `private const int State_Recalling = 4;` |
| 138 | field | Terraria.GameContent.LeashedEntities.WalkerLeashedCritter | Terraria.GameContent.LeashedEntities/WalkerLeashedCritter.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\WalkerLeashedCritter.cs | 21 | 2 | walkingPace | float | `protected float walkingPace;` | `protected float walkingPace;` |

##### 属性（0）

无该类型成员记录。


#### 4.3.4 细分子系统：`LeashedJumperBehavior`

- 细分职责：跳跃拴系生物的跳跃窗口、冷却和水面资格。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System 读取输入并通过显式 Command/CommitPort 写入；跨组只用事件、Query 或命令交接。
- 成员文件数：1；声明类型数：1；字段：11；属性：0；合计：11。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（11）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 82 | field | Terraria.GameContent.LeashedEntities.JumperLeashedCritter | Terraria.GameContent.LeashedEntities/JumperLeashedCritter.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\JumperLeashedCritter.cs | 9 | 2 | Prototype | Terraria.GameContent.LeashedEntities.JumperLeashedCritter | `public static JumperLeashedCritter Prototype = new JumperLeashedCritter();` | `public static JumperLeashedCritter Prototype = new JumperLeashedCritter();` |
| 83 | field | Terraria.GameContent.LeashedEntities.JumperLeashedCritter | Terraria.GameContent.LeashedEntities/JumperLeashedCritter.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\JumperLeashedCritter.cs | 11 | 2 | State_Normal | int | `private const int State_Normal = 0;` | `private const int State_Normal = 0;` |
| 84 | field | Terraria.GameContent.LeashedEntities.JumperLeashedCritter | Terraria.GameContent.LeashedEntities/JumperLeashedCritter.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\JumperLeashedCritter.cs | 13 | 2 | State_Recalling | int | `private const int State_Recalling = 1;` | `private const int State_Recalling = 1;` |
| 85 | field | Terraria.GameContent.LeashedEntities.JumperLeashedCritter | Terraria.GameContent.LeashedEntities/JumperLeashedCritter.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\JumperLeashedCritter.cs | 15 | 2 | minWaitTime | int | `protected int minWaitTime;` | `protected int minWaitTime;` |
| 86 | field | Terraria.GameContent.LeashedEntities.JumperLeashedCritter | Terraria.GameContent.LeashedEntities/JumperLeashedCritter.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\JumperLeashedCritter.cs | 17 | 2 | maxWaitTime | int | `protected int maxWaitTime;` | `protected int maxWaitTime;` |
| 87 | field | Terraria.GameContent.LeashedEntities.JumperLeashedCritter | Terraria.GameContent.LeashedEntities/JumperLeashedCritter.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\JumperLeashedCritter.cs | 19 | 2 | maxJumpWidth | float | `protected float maxJumpWidth;` | `protected float maxJumpWidth;` |
| 88 | field | Terraria.GameContent.LeashedEntities.JumperLeashedCritter | Terraria.GameContent.LeashedEntities/JumperLeashedCritter.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\JumperLeashedCritter.cs | 21 | 2 | minJumpWidth | float | `protected float minJumpWidth;` | `protected float minJumpWidth;` |
| 89 | field | Terraria.GameContent.LeashedEntities.JumperLeashedCritter | Terraria.GameContent.LeashedEntities/JumperLeashedCritter.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\JumperLeashedCritter.cs | 23 | 2 | maxJumpHeight | float | `protected float maxJumpHeight;` | `protected float maxJumpHeight;` |
| 90 | field | Terraria.GameContent.LeashedEntities.JumperLeashedCritter | Terraria.GameContent.LeashedEntities/JumperLeashedCritter.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\JumperLeashedCritter.cs | 25 | 2 | maxJumpDuration | float | `protected float maxJumpDuration;` | `protected float maxJumpDuration;` |
| 91 | field | Terraria.GameContent.LeashedEntities.JumperLeashedCritter | Terraria.GameContent.LeashedEntities/JumperLeashedCritter.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\JumperLeashedCritter.cs | 27 | 2 | jumpCooldown | int | `protected int jumpCooldown;` | `protected int jumpCooldown;` |
| 92 | field | Terraria.GameContent.LeashedEntities.JumperLeashedCritter | Terraria.GameContent.LeashedEntities/JumperLeashedCritter.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\JumperLeashedCritter.cs | 29 | 2 | canStandOnWater | bool | `protected bool canStandOnWater;` | `protected bool canStandOnWater;` |

##### 属性（0）

无该类型成员记录。


#### 4.3.5 细分子系统：`LeashedFlyerBehavior`

- 细分职责：飞行拴系生物的悬停、加速、制动和垂直速度。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System 读取输入并通过显式 Command/CommitPort 写入；跨组只用事件、Query 或命令交接。
- 成员文件数：1；声明类型数：1；字段：11；属性：0；合计：11。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（11）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 70 | field | Terraria.GameContent.LeashedEntities.FlyerLeashedCritter | Terraria.GameContent.LeashedEntities/FlyerLeashedCritter.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\FlyerLeashedCritter.cs | 9 | 2 | Prototype | Terraria.GameContent.LeashedEntities.FlyerLeashedCritter | `public static FlyerLeashedCritter Prototype = new FlyerLeashedCritter();` | `public static FlyerLeashedCritter Prototype = new FlyerLeashedCritter();` |
| 71 | field | Terraria.GameContent.LeashedEntities.FlyerLeashedCritter | Terraria.GameContent.LeashedEntities/FlyerLeashedCritter.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\FlyerLeashedCritter.cs | 11 | 2 | minWaitTime | int | `protected int minWaitTime;` | `protected int minWaitTime;` |
| 72 | field | Terraria.GameContent.LeashedEntities.FlyerLeashedCritter | Terraria.GameContent.LeashedEntities/FlyerLeashedCritter.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\FlyerLeashedCritter.cs | 13 | 2 | maxWaitTime | int | `protected int maxWaitTime;` | `protected int maxWaitTime;` |
| 73 | field | Terraria.GameContent.LeashedEntities.FlyerLeashedCritter | Terraria.GameContent.LeashedEntities/FlyerLeashedCritter.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\FlyerLeashedCritter.cs | 15 | 2 | maxFlySpeed | float | `protected float maxFlySpeed;` | `protected float maxFlySpeed;` |
| 74 | field | Terraria.GameContent.LeashedEntities.FlyerLeashedCritter | Terraria.GameContent.LeashedEntities/FlyerLeashedCritter.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\FlyerLeashedCritter.cs | 17 | 2 | acceleration | float | `protected float acceleration;` | `protected float acceleration;` |
| 75 | field | Terraria.GameContent.LeashedEntities.FlyerLeashedCritter | Terraria.GameContent.LeashedEntities/FlyerLeashedCritter.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\FlyerLeashedCritter.cs | 19 | 2 | brakeDuration | int | `protected int brakeDuration;` | `protected int brakeDuration;` |
| 76 | field | Terraria.GameContent.LeashedEntities.FlyerLeashedCritter | Terraria.GameContent.LeashedEntities/FlyerLeashedCritter.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\FlyerLeashedCritter.cs | 21 | 2 | rotationScalar | float | `protected float rotationScalar;` | `protected float rotationScalar;` |
| 77 | field | Terraria.GameContent.LeashedEntities.FlyerLeashedCritter | Terraria.GameContent.LeashedEntities/FlyerLeashedCritter.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\FlyerLeashedCritter.cs | 23 | 2 | hoverAmplitude | float | `protected float hoverAmplitude;` | `protected float hoverAmplitude;` |
| 78 | field | Terraria.GameContent.LeashedEntities.FlyerLeashedCritter | Terraria.GameContent.LeashedEntities/FlyerLeashedCritter.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\FlyerLeashedCritter.cs | 25 | 2 | hoverPeriod | float | `protected float hoverPeriod;` | `protected float hoverPeriod;` |
| 79 | field | Terraria.GameContent.LeashedEntities.FlyerLeashedCritter | Terraria.GameContent.LeashedEntities/FlyerLeashedCritter.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\FlyerLeashedCritter.cs | 27 | 2 | hasGroundBias | bool | `protected bool hasGroundBias;` | `protected bool hasGroundBias;` |
| 80 | field | Terraria.GameContent.LeashedEntities.FlyerLeashedCritter | Terraria.GameContent.LeashedEntities/FlyerLeashedCritter.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\FlyerLeashedCritter.cs | 29 | 2 | HoverYVelocity | float | `private const float HoverYVelocity = 0.0001f;` | `private const float HoverYVelocity = 0.0001f;` |

##### 属性（0）

无该类型成员记录。


#### 4.3.6 细分子系统：`LeashedKiteBehavior`

- 细分职责：风筝实体的风场、计时、轨迹、帧和锚点派生状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System 读取输入并通过显式 Command/CommitPort 写入；跨组只用事件、Query 或命令交接。
- 成员文件数：1；声明类型数：1；字段：19；属性：1；合计：20。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（19）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 108 | field | Terraria.GameContent.LeashedEntities.LeashedKite | Terraria.GameContent.LeashedEntities/LeashedKite.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\LeashedKite.cs | 11 | 2 | Prototype | Terraria.GameContent.LeashedEntities.LeashedKite | `public static LeashedKite Prototype;` | `public static LeashedKite Prototype;` |
| 109 | field | Terraria.GameContent.LeashedEntities.LeashedKite | Terraria.GameContent.LeashedEntities/LeashedKite.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\LeashedKite.cs | 13 | 2 | _dummy | Terraria.Projectile | `private static Projectile _dummy = new Projectile();` | `private static Projectile _dummy = new Projectile();` |
| 110 | field | Terraria.GameContent.LeashedEntities.LeashedKite | Terraria.GameContent.LeashedEntities/LeashedKite.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\LeashedKite.cs | 15 | 2 | projType | int | `public int projType;` | `public int projType;` |
| 111 | field | Terraria.GameContent.LeashedEntities.LeashedKite | Terraria.GameContent.LeashedEntities/LeashedKite.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\LeashedKite.cs | 17 | 2 | frame | int | `public int frame;` | `public int frame;` |
| 112 | field | Terraria.GameContent.LeashedEntities.LeashedKite | Terraria.GameContent.LeashedEntities/LeashedKite.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\LeashedKite.cs | 19 | 2 | frameCounter | int | `public int frameCounter;` | `public int frameCounter;` |
| 113 | field | Terraria.GameContent.LeashedEntities.LeashedKite | Terraria.GameContent.LeashedEntities/LeashedKite.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\LeashedKite.cs | 21 | 2 | rotation | float | `public float rotation;` | `public float rotation;` |
| 114 | field | Terraria.GameContent.LeashedEntities.LeashedKite | Terraria.GameContent.LeashedEntities/LeashedKite.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\LeashedKite.cs | 23 | 2 | spriteDirection | int | `public int spriteDirection = 1;` | `public int spriteDirection = 1;` |
| 115 | field | Terraria.GameContent.LeashedEntities.LeashedKite | Terraria.GameContent.LeashedEntities/LeashedKite.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\LeashedKite.cs | 25 | 2 | kiteDistance | float | `public float kiteDistance = 250f;` | `public float kiteDistance = 250f;` |
| 116 | field | Terraria.GameContent.LeashedEntities.LeashedKite | Terraria.GameContent.LeashedEntities/LeashedKite.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\LeashedKite.cs | 27 | 2 | windTarget | float | `public float windTarget;` | `public float windTarget;` |
| 117 | field | Terraria.GameContent.LeashedEntities.LeashedKite | Terraria.GameContent.LeashedEntities/LeashedKite.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\LeashedKite.cs | 29 | 2 | windCurrent | float | `public float windCurrent;` | `public float windCurrent;` |
| 118 | field | Terraria.GameContent.LeashedEntities.LeashedKite | Terraria.GameContent.LeashedEntities/LeashedKite.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\LeashedKite.cs | 31 | 2 | timeCounter | float | `public float timeCounter;` | `public float timeCounter;` |
| 119 | field | Terraria.GameContent.LeashedEntities.LeashedKite | Terraria.GameContent.LeashedEntities/LeashedKite.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\LeashedKite.cs | 33 | 2 | cloudAlpha | float | `public float cloudAlpha;` | `public float cloudAlpha;` |
| 120 | field | Terraria.GameContent.LeashedEntities.LeashedKite | Terraria.GameContent.LeashedEntities/LeashedKite.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\LeashedKite.cs | 35 | 2 | timeWithoutWind | int | `public int timeWithoutWind;` | `public int timeWithoutWind;` |
| 121 | field | Terraria.GameContent.LeashedEntities.LeashedKite | Terraria.GameContent.LeashedEntities/LeashedKite.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\LeashedKite.cs | 37 | 2 | projectileLocalAI0 | float | `public float projectileLocalAI0;` | `public float projectileLocalAI0;` |
| 122 | field | Terraria.GameContent.LeashedEntities.LeashedKite | Terraria.GameContent.LeashedEntities/LeashedKite.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\LeashedKite.cs | 39 | 2 | projectileLocalAI1 | float | `public float projectileLocalAI1;` | `public float projectileLocalAI1;` |
| 123 | field | Terraria.GameContent.LeashedEntities.LeashedKite | Terraria.GameContent.LeashedEntities/LeashedKite.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\LeashedKite.cs | 41 | 2 | oldPos | Vector2[] | `public Vector2[] oldPos;` | `public Vector2[] oldPos;` |
| 124 | field | Terraria.GameContent.LeashedEntities.LeashedKite | Terraria.GameContent.LeashedEntities/LeashedKite.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\LeashedKite.cs | 43 | 2 | oldRot | float[] | `public float[] oldRot;` | `public float[] oldRot;` |
| 125 | field | Terraria.GameContent.LeashedEntities.LeashedKite | Terraria.GameContent.LeashedEntities/LeashedKite.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\LeashedKite.cs | 45 | 2 | oldSpriteDirection | int[] | `public int[] oldSpriteDirection;` | `public int[] oldSpriteDirection;` |
| 126 | field | Terraria.GameContent.LeashedEntities.LeashedKite | Terraria.GameContent.LeashedEntities/LeashedKite.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\LeashedKite.cs | 47 | 2 | netOffset | Vector2 | `public Vector2 netOffset;` | `public Vector2 netOffset;` |

##### 属性（1）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 154 | property | Terraria.GameContent.LeashedEntities.LeashedKite | Terraria.GameContent.LeashedEntities/LeashedKite.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\LeashedKite.cs | 49 | 2 | AnchorWorldPosition | Vector2 | `private Vector2 AnchorWorldPosition => base.AnchorPosition.ToWorldCoordinates();` | `private Vector2 AnchorWorldPosition => base.AnchorPosition.ToWorldCoordinates();` |


#### 4.3.7 细分子系统：`LeashedButterflyVariants`

- 细分职责：蝴蝶拴系实体的变体、淡出和透明度状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System 读取输入并通过显式 Command/CommitPort 写入；跨组只用事件、Query 或命令交接。
- 成员文件数：2；声明类型数：2；字段：5；属性：1；合计：6。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（5）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 64 | field | Terraria.GameContent.LeashedEntities.EmpressButterflyLeashedCritter | Terraria.GameContent.LeashedEntities/EmpressButterflyLeashedCritter.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\EmpressButterflyLeashedCritter.cs | 7 | 2 | Prototype | Terraria.GameContent.LeashedEntities.EmpressButterflyLeashedCritter | `public new static EmpressButterflyLeashedCritter Prototype = new EmpressButterflyLeashedCritter();` | `public new static EmpressButterflyLeashedCritter Prototype = new EmpressButterflyLeashedCritter();` |
| 65 | field | Terraria.GameContent.LeashedEntities.EmpressButterflyLeashedCritter | Terraria.GameContent.LeashedEntities/EmpressButterflyLeashedCritter.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\EmpressButterflyLeashedCritter.cs | 9 | 2 | fadeAmount | float | `private float fadeAmount;` | `private float fadeAmount;` |
| 66 | field | Terraria.GameContent.LeashedEntities.EmpressButterflyLeashedCritter | Terraria.GameContent.LeashedEntities/EmpressButterflyLeashedCritter.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\EmpressButterflyLeashedCritter.cs | 11 | 2 | FadeAwayCap | int | `private const int FadeAwayCap = 50;` | `private const int FadeAwayCap = 50;` |
| 127 | field | Terraria.GameContent.LeashedEntities.NormalButterflyLeashedCritter | Terraria.GameContent.LeashedEntities/NormalButterflyLeashedCritter.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\NormalButterflyLeashedCritter.cs | 7 | 2 | Prototype | Terraria.GameContent.LeashedEntities.NormalButterflyLeashedCritter | `public new static NormalButterflyLeashedCritter Prototype = new NormalButterflyLeashedCritter();` | `public new static NormalButterflyLeashedCritter Prototype = new NormalButterflyLeashedCritter();` |
| 128 | field | Terraria.GameContent.LeashedEntities.NormalButterflyLeashedCritter | Terraria.GameContent.LeashedEntities/NormalButterflyLeashedCritter.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\NormalButterflyLeashedCritter.cs | 9 | 2 | variant | byte | `protected byte variant;` | `protected byte variant;` |

##### 属性（1）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 153 | property | Terraria.GameContent.LeashedEntities.EmpressButterflyLeashedCritter | Terraria.GameContent.LeashedEntities/EmpressButterflyLeashedCritter.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\EmpressButterflyLeashedCritter.cs | 13 | 2 | Opacity | float | `private float Opacity => Utils.GetLerpValue(60f, 25f, fadeAmount, clamped: true);` | `private float Opacity => Utils.GetLerpValue(60f, 25f, fadeAmount, clamped: true);` |


#### 4.3.8 细分子系统：`LeashedSpeciesPrototypes`

- 细分职责：物种原型引用；不承担拴系实体运行时状态。
- 边界角色：`authoritative state/behavior`；最小 seam：Owner System 读取输入并通过显式 Command/CommitPort 写入；跨组只用事件、Query 或命令交接。
- 成员文件数：13；声明类型数：13；字段：13；属性：0；合计：13。
- 归属证据状态：``source-inventory-confirmed``；完整读者/写者、生命周期、持久化、网络和运行时调度仍需单独闭合。

##### 字段（13）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 60 | field | Terraria.GameContent.LeashedEntities.BirdLeashedCritter | Terraria.GameContent.LeashedEntities/BirdLeashedCritter.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\BirdLeashedCritter.cs | 5 | 2 | Prototype | Terraria.GameContent.LeashedEntities.BirdLeashedCritter | `public new static BirdLeashedCritter Prototype = new BirdLeashedCritter();` | `public new static BirdLeashedCritter Prototype = new BirdLeashedCritter();` |
| 61 | field | Terraria.GameContent.LeashedEntities.CrawlerLeashedCritter | Terraria.GameContent.LeashedEntities/CrawlerLeashedCritter.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\CrawlerLeashedCritter.cs | 5 | 2 | Prototype | Terraria.GameContent.LeashedEntities.CrawlerLeashedCritter | `public new static CrawlerLeashedCritter Prototype = new CrawlerLeashedCritter();` | `public new static CrawlerLeashedCritter Prototype = new CrawlerLeashedCritter();` |
| 62 | field | Terraria.GameContent.LeashedEntities.CrawlingFlyLeashedCritter | Terraria.GameContent.LeashedEntities/CrawlingFlyLeashedCritter.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\CrawlingFlyLeashedCritter.cs | 5 | 2 | Prototype | Terraria.GameContent.LeashedEntities.CrawlingFlyLeashedCritter | `public new static CrawlingFlyLeashedCritter Prototype = new CrawlingFlyLeashedCritter();` | `public new static CrawlingFlyLeashedCritter Prototype = new CrawlingFlyLeashedCritter();` |
| 63 | field | Terraria.GameContent.LeashedEntities.DragonflyLeashedCritter | Terraria.GameContent.LeashedEntities/DragonflyLeashedCritter.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\DragonflyLeashedCritter.cs | 5 | 2 | Prototype | Terraria.GameContent.LeashedEntities.DragonflyLeashedCritter | `public new static DragonflyLeashedCritter Prototype = new DragonflyLeashedCritter();` | `public new static DragonflyLeashedCritter Prototype = new DragonflyLeashedCritter();` |
| 67 | field | Terraria.GameContent.LeashedEntities.FairyLeashedCritter | Terraria.GameContent.LeashedEntities/FairyLeashedCritter.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\FairyLeashedCritter.cs | 7 | 2 | Prototype | Terraria.GameContent.LeashedEntities.FairyLeashedCritter | `public new static FairyLeashedCritter Prototype = new FairyLeashedCritter();` | `public new static FairyLeashedCritter Prototype = new FairyLeashedCritter();` |
| 68 | field | Terraria.GameContent.LeashedEntities.FireflyLeashedCritter | Terraria.GameContent.LeashedEntities/FireflyLeashedCritter.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\FireflyLeashedCritter.cs | 5 | 2 | Prototype | Terraria.GameContent.LeashedEntities.FireflyLeashedCritter | `public new static FireflyLeashedCritter Prototype = new FireflyLeashedCritter();` | `public new static FireflyLeashedCritter Prototype = new FireflyLeashedCritter();` |
| 69 | field | Terraria.GameContent.LeashedEntities.FishLeashedCritter | Terraria.GameContent.LeashedEntities/FishLeashedCritter.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\FishLeashedCritter.cs | 7 | 2 | Prototype | Terraria.GameContent.LeashedEntities.FishLeashedCritter | `public new static FishLeashedCritter Prototype = new FishLeashedCritter();` | `public new static FishLeashedCritter Prototype = new FishLeashedCritter();` |
| 81 | field | Terraria.GameContent.LeashedEntities.HellButterflyLeashedCritter | Terraria.GameContent.LeashedEntities/HellButterflyLeashedCritter.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\HellButterflyLeashedCritter.cs | 5 | 2 | Prototype | Terraria.GameContent.LeashedEntities.HellButterflyLeashedCritter | `public new static HellButterflyLeashedCritter Prototype = new HellButterflyLeashedCritter();` | `public new static HellButterflyLeashedCritter Prototype = new HellButterflyLeashedCritter();` |
| 129 | field | Terraria.GameContent.LeashedEntities.RunnerLeashedCritter | Terraria.GameContent.LeashedEntities/RunnerLeashedCritter.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\RunnerLeashedCritter.cs | 5 | 2 | Prototype | Terraria.GameContent.LeashedEntities.RunnerLeashedCritter | `public new static RunnerLeashedCritter Prototype = new RunnerLeashedCritter();` | `public new static RunnerLeashedCritter Prototype = new RunnerLeashedCritter();` |
| 130 | field | Terraria.GameContent.LeashedEntities.ShimmerFlyLeashedCritter | Terraria.GameContent.LeashedEntities/ShimmerFlyLeashedCritter.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\ShimmerFlyLeashedCritter.cs | 8 | 2 | Prototype | Terraria.GameContent.LeashedEntities.ShimmerFlyLeashedCritter | `public new static ShimmerFlyLeashedCritter Prototype = new ShimmerFlyLeashedCritter();` | `public new static ShimmerFlyLeashedCritter Prototype = new ShimmerFlyLeashedCritter();` |
| 131 | field | Terraria.GameContent.LeashedEntities.SnailLeashedCritter | Terraria.GameContent.LeashedEntities/SnailLeashedCritter.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\SnailLeashedCritter.cs | 5 | 2 | Prototype | Terraria.GameContent.LeashedEntities.SnailLeashedCritter | `public new static SnailLeashedCritter Prototype = new SnailLeashedCritter();` | `public new static SnailLeashedCritter Prototype = new SnailLeashedCritter();` |
| 139 | field | Terraria.GameContent.LeashedEntities.WaterfowlLeashedCritter | Terraria.GameContent.LeashedEntities/WaterfowlLeashedCritter.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\WaterfowlLeashedCritter.cs | 5 | 2 | Prototype | Terraria.GameContent.LeashedEntities.WaterfowlLeashedCritter | `public new static WaterfowlLeashedCritter Prototype = new WaterfowlLeashedCritter();` | `public new static WaterfowlLeashedCritter Prototype = new WaterfowlLeashedCritter();` |
| 140 | field | Terraria.GameContent.LeashedEntities.WaterStriderLeashedCritter | Terraria.GameContent.LeashedEntities/WaterStriderLeashedCritter.cs | D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\WaterStriderLeashedCritter.cs | 7 | 2 | Prototype | Terraria.GameContent.LeashedEntities.WaterStriderLeashedCritter | `public new static WaterStriderLeashedCritter Prototype = new WaterStriderLeashedCritter();` | `public new static WaterStriderLeashedCritter Prototype = new WaterStriderLeashedCritter();` |

##### 属性（0）

无该类型成员记录。


## 8. 本分区自检

- 叶子子系统：8 个；指定组均已写入且没有重复分区。
- 字段/属性/成员：93 / 5 / 98。
- 来源序号范围：60..157；本分区内唯一序号：True。
- 成员声明表逐组从源报告提取，未改写来源序号、类型、路径、行列、成员、C# 类型、成员声明或原始声明。
- 运行时读写者、生命周期、持久化、网络和调度：未验证，保留为迁移前风险。
