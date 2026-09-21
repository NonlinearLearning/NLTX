# Version4 非权威组件拆分分区 09/20：物品、库存与容器

> 来源报告：`docs/migration/ledgers/Version4非权威模拟系统字段属性逐成员源码声明-更细子系统拆分-去除ID类文件.md`
> 来源报告 SHA-256：`b944441d92d5103f218b766a3e354200794528a19e65a862e44c2b40a36f1196`
> 本文件是并行组件拆分工作包，不是新的权威成员清单；成员声明事实直接保留来源报告原文。

## 1. 分区定位与评估

- 分区范围：物品身份、堆叠、转移、容器、TileEntity 库存和世界物品 payload。
- 本分区组件化重点：按容器/库存访问模式拆分权威物品状态、转移命令和展示快照。
- 本分区包含 14 个完整细分子系统、191 条成员记录（字段 152、属性 39）。来源序号覆盖区间 `765..4053`，区间可能与其他分区交错，不以序号定义领域边界。
- 组件边界判定：细分子系统只是并行处理的最小库存边界，不等价于一个组件。必须继续按共同读者/写者、变更原因、生命周期、权威性和副作用分为 Component、System、Query、Command、Adapter 或 Projection。
- 证据状态：成员声明和统计已由来源报告校验；逐成员读者、写者、初始化/清理路径、持久化/网络语义和系统顺序仍需本分区会话补证，不得从名称或数量推断。

## 2. 分区统计

| 父级子系统 | 细分子系统数 | 字段 | 属性 | 合计 |
|---|---:|---:|---:|---:|
| `SharedRuntimeMechanisms` | 13 | 129 | 39 | 168 |
| `WorldStorage` | 1 | 23 | 0 | 23 |

| 原报告章节 | 父级 | 细分子系统 | 边界角色 | 字段 | 属性 | 合计 | 组件化处理 |
|---|---|---|---|---:|---:|---:|---|
| `4.5.1` | `WorldStorage` | `ChestContainerStorage` | authoritative snapshot | 23 | 0 | 23 | 待按成员访问模式拆分 |
| `4.9.16` | `SharedRuntimeMechanisms` | `SharedItemIdentityAndStackState` | state | 9 | 5 | 14 | 待按成员访问模式拆分 |
| `4.9.17` | `SharedRuntimeMechanisms` | `SharedItemProgressionAndWorldInteractionState` | definition/state | 9 | 0 | 9 | 待按成员访问模式拆分 |
| `4.9.19` | `SharedRuntimeMechanisms` | `SharedItemBuffMountAndConsumableEffects` | definition/state | 6 | 0 | 6 | 待按成员访问模式拆分 |
| `4.9.20` | `SharedRuntimeMechanisms` | `SharedItemDerivedQueries` | derived/query | 0 | 3 | 3 | 待按成员访问模式拆分 |
| `4.9.67` | `SharedRuntimeMechanisms` | `ItemQuickStackingState` | state/query | 17 | 0 | 17 | 待按成员访问模式拆分 |
| `4.9.68` | `SharedRuntimeMechanisms` | `ItemTransferSettings` | definition/state | 16 | 0 | 16 | 待按成员访问模式拆分 |
| `4.9.126` | `SharedRuntimeMechanisms` | `SharedWorldItemLifecycleState` | state | 15 | 1 | 16 | 待按成员访问模式拆分 |
| `4.9.132` | `SharedRuntimeMechanisms` | `SharedItemEquipmentSlotState` | definition/state | 21 | 0 | 21 | 待按成员访问模式拆分 |
| `4.9.188` | `SharedRuntimeMechanisms` | `SharedWorldItemIdentityAndEconomyPayloadState` | state/projection | 0 | 9 | 9 | 待按成员访问模式拆分 |
| `4.9.189` | `SharedRuntimeMechanisms` | `SharedWorldItemUseAndPresentationPayloadState` | state/projection | 0 | 19 | 19 | 待按成员访问模式拆分 |
| `4.9.190` | `SharedRuntimeMechanisms` | `SharedItemEmergencyStackingPolicyState` | state/query | 6 | 0 | 6 | 待按成员访问模式拆分 |
| `4.9.191` | `SharedRuntimeMechanisms` | `SharedItemEmergencyStackingTransferState` | state/query | 19 | 2 | 21 | 待按成员访问模式拆分 |
| `4.9.215` | `SharedRuntimeMechanisms` | `SharedItemToolPlacementCapabilityState` | definition/state | 11 | 0 | 11 | 待按成员访问模式拆分 |

## 3. 并行组件拆分契约

对本文件中的每个细分子系统和每条成员记录执行以下判断，并把结果写入对应的实现阶段设计/审计记录：

- 盘点声明类型、可见性、读者、写者、创建/更新/清理/持久化生命周期、访问频率和副作用；缺证据标记 `missing`，不可用命名猜测补齐。
- 标记权威状态、派生值、缓存、快照、兼容字段和表现数据；Component 只保存一个内聚概念的权威数据。
- 需要行为转换时放入显式 System/Command；纯资格判断和派生计算放入 Query；外部协议、文件、平台和第三方类型放入 Adapter；网络/持久化/日志/UI 输出放入单向 Projection。
- 分别建模实体引用、持久化 ID、网络 ID 和外部 ID；不得把 ID 类文件或外部对象直接塞入共享组件。
- 记录 Interface、Implementation、Seam、Depth、Leverage、Locality，以及必要的系统先后约束、失败归属和重试策略。

## 4. 逐成员源码声明

以下细分子系统块保持来源报告的成员表、声明、路径、行列和来源序号；只调整 Markdown 标题层级并增加分区追踪元数据。

### 4.1 细分子系统：`ChestContainerStorage`

- 原报告章节：`4.5.1`
- 父级子系统：`WorldStorage`
- 分区工作包：`09` / `物品、库存与容器`

- 上一级基线细分子系统：`ChestContainerStorage`
- 细分职责：Chest 容器和槽位字段。
- 边界角色：`authoritative snapshot`；最小 seam：container storage port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：23；属性：0；合计：23。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（23）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 765 | field | Terraria.Chest | Terraria/Chest.cs | D:\TRbackup\Version4\Terraria\Chest.cs | 20 | 2 | chestStackRange | float | `public const float chestStackRange = 600f;` | `public const float chestStackRange = 600f;` |
| 766 | field | Terraria.Chest | Terraria/Chest.cs | D:\TRbackup\Version4\Terraria\Chest.cs | 22 | 2 | maxChestTypes | int | `public const int maxChestTypes = 52;` | `public const int maxChestTypes = 52;` |
| 767 | field | Terraria.Chest | Terraria/Chest.cs | D:\TRbackup\Version4\Terraria\Chest.cs | 24 | 2 | chestTypeToIcon | int[] | `public static int[] chestTypeToIcon = new int[52];` | `public static int[] chestTypeToIcon = new int[52];` |
| 768 | field | Terraria.Chest | Terraria/Chest.cs | D:\TRbackup\Version4\Terraria\Chest.cs | 26 | 2 | maxChestTypes2 | int | `public const int maxChestTypes2 = 38;` | `public const int maxChestTypes2 = 38;` |
| 769 | field | Terraria.Chest | Terraria/Chest.cs | D:\TRbackup\Version4\Terraria\Chest.cs | 28 | 2 | chestTypeToIcon2 | int[] | `public static int[] chestTypeToIcon2 = new int[38];` | `public static int[] chestTypeToIcon2 = new int[38];` |
| 770 | field | Terraria.Chest | Terraria/Chest.cs | D:\TRbackup\Version4\Terraria\Chest.cs | 30 | 2 | maxDresserTypes | int | `public const int maxDresserTypes = 65;` | `public const int maxDresserTypes = 65;` |
| 771 | field | Terraria.Chest | Terraria/Chest.cs | D:\TRbackup\Version4\Terraria\Chest.cs | 32 | 2 | dresserTypeToIcon | int[] | `public static int[] dresserTypeToIcon = new int[65];` | `public static int[] dresserTypeToIcon = new int[65];` |
| 772 | field | Terraria.Chest | Terraria/Chest.cs | D:\TRbackup\Version4\Terraria\Chest.cs | 34 | 2 | DefaultMaxItems | int | `public const int DefaultMaxItems = 40;` | `public const int DefaultMaxItems = 40;` |
| 773 | field | Terraria.Chest | Terraria/Chest.cs | D:\TRbackup\Version4\Terraria\Chest.cs | 36 | 2 | AbsoluteMaxItemsWeCanEverReachInAChestForNow | int | `public const int AbsoluteMaxItemsWeCanEverReachInAChestForNow = 200;` | `public const int AbsoluteMaxItemsWeCanEverReachInAChestForNow = 200;` |
| 774 | field | Terraria.Chest | Terraria/Chest.cs | D:\TRbackup\Version4\Terraria\Chest.cs | 38 | 2 | maxItems | int | `public int maxItems;` | `public int maxItems;` |
| 775 | field | Terraria.Chest | Terraria/Chest.cs | D:\TRbackup\Version4\Terraria\Chest.cs | 40 | 2 | MaxNameLength | int | `public const int MaxNameLength = 20;` | `public const int MaxNameLength = 20;` |
| 776 | field | Terraria.Chest | Terraria/Chest.cs | D:\TRbackup\Version4\Terraria\Chest.cs | 42 | 2 | item | Terraria.Item[] | `public Item[] item;` | `public Item[] item;` |
| 777 | field | Terraria.Chest | Terraria/Chest.cs | D:\TRbackup\Version4\Terraria\Chest.cs | 44 | 2 | x | int | `public readonly int x;` | `public readonly int x;` |
| 778 | field | Terraria.Chest | Terraria/Chest.cs | D:\TRbackup\Version4\Terraria\Chest.cs | 46 | 2 | y | int | `public readonly int y;` | `public readonly int y;` |
| 779 | field | Terraria.Chest | Terraria/Chest.cs | D:\TRbackup\Version4\Terraria\Chest.cs | 48 | 2 | index | int | `public readonly int index;` | `public readonly int index;` |
| 780 | field | Terraria.Chest | Terraria/Chest.cs | D:\TRbackup\Version4\Terraria\Chest.cs | 50 | 2 | bankChest | bool | `public bool bankChest;` | `public bool bankChest;` |
| 781 | field | Terraria.Chest | Terraria/Chest.cs | D:\TRbackup\Version4\Terraria\Chest.cs | 52 | 2 | name | string | `public string name;` | `public string name;` |
| 782 | field | Terraria.Chest | Terraria/Chest.cs | D:\TRbackup\Version4\Terraria\Chest.cs | 54 | 2 | frameCounter | int | `public int frameCounter;` | `public int frameCounter;` |
| 783 | field | Terraria.Chest | Terraria/Chest.cs | D:\TRbackup\Version4\Terraria\Chest.cs | 56 | 2 | frame | int | `public int frame;` | `public int frame;` |
| 784 | field | Terraria.Chest | Terraria/Chest.cs | D:\TRbackup\Version4\Terraria\Chest.cs | 58 | 2 | eatingAnimationTime | int | `public int eatingAnimationTime;` | `public int eatingAnimationTime;` |
| 785 | field | Terraria.Chest | Terraria/Chest.cs | D:\TRbackup\Version4\Terraria\Chest.cs | 60 | 2 | _itemsGotSet | bool | `private bool _itemsGotSet;` | `private bool _itemsGotSet;` |
| 786 | field | Terraria.Chest | Terraria/Chest.cs | D:\TRbackup\Version4\Terraria\Chest.cs | 62 | 2 | _chestsByCoords | System.Collections.Generic.Dictionary<Point, Terraria.Chest> | `private static Dictionary<Point, Chest> _chestsByCoords = new Dictionary<Point, Chest>();` | `private static Dictionary<Point, Chest> _chestsByCoords = new Dictionary<Point, Chest>();` |
| 787 | field | Terraria.Chest | Terraria/Chest.cs | D:\TRbackup\Version4\Terraria\Chest.cs | 66 | 2 | _chestInUse | System.Collections.Generic.HashSet<int> | `private static HashSet<int> _chestInUse = new HashSet<int>();` | `private static HashSet<int> _chestInUse = new HashSet<int>();` |

#### 属性（0）

无该类型成员记录。


### 4.2 细分子系统：`SharedItemIdentityAndStackState`

- 原报告章节：`4.9.16`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`09` / `物品、库存与容器`

- 上一级基线细分子系统：`SharedItemIdentityAndStackState`
- 细分职责：物品身份、实例数量、堆叠和变体身份。
- 边界角色：`state`；最小 seam：item identity/stack port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：9；属性：5；合计：14。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（9）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3138 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 20 | 2 | width | int | `public int width;` | `public int width;` |
| 3139 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 22 | 5 | height | int | `public int height;` | `public int height;` |
| 3144 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 32 | 2 | _nameOverride | string | `private string _nameOverride;` | `private string _nameOverride;` |
| 3189 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 122 | 2 | type | int | `public int type;` | `public int type;` |
| 3190 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 124 | 2 | favorited | bool | `public bool favorited;` | `public bool favorited;` |
| 3197 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 138 | 2 | stack | int | `public int stack;` | `public int stack;` |
| 3198 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 140 | 2 | maxStack | int | `public int maxStack;` | `public int maxStack;` |
| 3262 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 268 | 2 | uniqueStack | bool | `public bool uniqueStack;` | `public bool uniqueStack;` |
| 3271 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 286 | 2 | prefix | byte | `public byte prefix;` | `public byte prefix;` |

#### 属性（5）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3961 | property | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 317 | 2 | active | bool | `public bool active => type != 0;` | `public bool active => type != 0;` |
| 3962 | property | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 319 | 2 | Name | string | `public string Name => _nameOverride ?? Lang.GetItemNameValue(type);` | `public string Name => _nameOverride ?? Lang.GetItemNameValue(type);` |
| 3967 | property | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 339 | 2 | Variant | Terraria.GameContent.Items.ItemVariant | `public ItemVariant Variant { get; private set; }` | `public ItemVariant Variant { get; private set; }` |
| 3968 | property | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 341 | 2 | IsACoin | bool | `public bool IsACoin { get { int num = type; if ((uint)(num - 71) <= 3u) { return true; } return false; } }` | `public bool IsACoin { get { int num = type; if ((uint)(num - 71) <= 3u) { return true; } return false; } }` |
| 3969 | property | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 354 | 2 | IsAir | bool | `public bool IsAir { get { if (type > 0) { return stack <= 0; } return true; } }` | `public bool IsAir { get { if (type > 0) { return stack <= 0; } return true; } }` |


### 4.3 细分子系统：`SharedItemProgressionAndWorldInteractionState`

- 原报告章节：`4.9.17`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`09` / `物品、库存与容器`

- 上一级基线细分子系统：`SharedItemProgressionAndWorldInteractionState`
- 细分职责：任务、特殊使用、钓鱼、工具和 NPC 生成能力。
- 边界角色：`definition/state`；最小 seam：item progression/interaction port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：9；属性：0；合计：9。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（9）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3167 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 78 | 2 | questItem | bool | `public bool questItem;` | `public bool questItem;` |
| 3173 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 90 | 2 | flame | bool | `public bool flame;` | `public bool flame;` |
| 3174 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 92 | 2 | mech | bool | `public bool mech;` | `public bool mech;` |
| 3175 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 94 | 2 | tileWand | int | `public int tileWand = -1;` | `public int tileWand = -1;` |
| 3180 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 104 | 2 | fishingPole | int | `public int fishingPole = 1;` | `public int fishingPole = 1;` |
| 3181 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 106 | 2 | bait | int | `public int bait;` | `public int bait;` |
| 3182 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 108 | 2 | makeNPC | short | `public short makeNPC;` | `public short makeNPC;` |
| 3183 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 110 | 2 | expertOnly | bool | `public bool expertOnly;` | `public bool expertOnly;` |
| 3184 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 112 | 2 | expert | bool | `public bool expert;` | `public bool expert;` |

#### 属性（0）

无该类型成员记录。


### 4.4 细分子系统：`SharedItemBuffMountAndConsumableEffects`

- 原报告章节：`4.9.19`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`09` / `物品、库存与容器`

- 上一级基线细分子系统：`SharedItemBuffMountAndConsumableEffects`
- 细分职责：Buff、坐骑、宠物和特殊消耗效果状态。
- 边界角色：`definition/state`；最小 seam：item effect port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：6；属性：0；合计：6。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（6）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3258 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 260 | 2 | buffType | int | `public int buffType;` | `public int buffType;` |
| 3259 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 262 | 2 | buffTime | int | `public int buffTime;` | `public int buffTime;` |
| 3260 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 264 | 2 | mountType | int | `public int mountType = -1;` | `public int mountType = -1;` |
| 3261 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 266 | 2 | cartTrack | bool | `public bool cartTrack;` | `public bool cartTrack;` |
| 3266 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 276 | 2 | chlorophyteExtractinatorConsumable | bool | `public bool chlorophyteExtractinatorConsumable;` | `public bool chlorophyteExtractinatorConsumable;` |
| 3267 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 278 | 2 | DD2Summon | bool | `public bool DD2Summon;` | `public bool DD2Summon;` |

#### 属性（0）

无该类型成员记录。


### 4.5 细分子系统：`SharedItemDerivedQueries`

- 原报告章节：`4.9.20`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`09` / `物品、库存与容器`

- 上一级基线细分子系统：`SharedItemDerivedQueries`
- 细分职责：从内容目录或实例状态派生的物品查询属性。
- 边界角色：`derived/query`；最小 seam：item derived query；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：0；属性：3；合计：3。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（0）

无该类型成员记录。

#### 属性（3）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3964 | property | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 333 | 2 | OriginalRarity | int | `public int OriginalRarity => ContentSamples.ItemsByType[type].rare;` | `public int OriginalRarity => ContentSamples.ItemsByType[type].rare;` |
| 3965 | property | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 335 | 2 | OriginalDamage | int | `public int OriginalDamage => ContentSamples.ItemsByType[type].damage;` | `public int OriginalDamage => ContentSamples.ItemsByType[type].damage;` |
| 3966 | property | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 337 | 2 | OriginalDefense | int | `public int OriginalDefense => ContentSamples.ItemsByType[type].defense;` | `public int OriginalDefense => ContentSamples.ItemsByType[type].defense;` |


### 4.6 细分子系统：`ItemQuickStackingState`

- 原报告章节：`4.9.67`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`09` / `物品、库存与容器`

- 上一级基线细分子系统：`ItemQuickStackingState`
- 细分职责：快速堆叠源、目标和匹配缓存状态。
- 边界角色：`state/query`；最小 seam：quick stacking port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：5；字段：17；属性：0；合计：17。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（17）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2487 | field | Terraria.GameContent.QuickStacking.DestinationHelper | Terraria.GameContent/QuickStacking.cs | D:\TRbackup\Version4\Terraria.GameContent\QuickStacking.cs | 15 | 3 | _chest | Terraria.GameContent.PositionedChest | `private PositionedChest _chest;` | `private PositionedChest _chest;` |
| 2488 | field | Terraria.GameContent.QuickStacking.DestinationHelper | Terraria.GameContent/QuickStacking.cs | D:\TRbackup\Version4\Terraria.GameContent\QuickStacking.cs | 17 | 3 | items | Terraria.Item[] | `public Item[] items;` | `public Item[] items;` |
| 2489 | field | Terraria.GameContent.QuickStacking.DestinationHelper | Terraria.GameContent/QuickStacking.cs | D:\TRbackup\Version4\Terraria.GameContent\QuickStacking.cs | 19 | 3 | itemCount | int | `public int itemCount;` | `public int itemCount;` |
| 2490 | field | Terraria.GameContent.QuickStacking.DestinationHelper | Terraria.GameContent/QuickStacking.cs | D:\TRbackup\Version4\Terraria.GameContent\QuickStacking.cs | 21 | 3 | locked | bool | `public bool locked;` | `public bool locked;` |
| 2491 | field | Terraria.GameContent.QuickStacking.DestinationHelper | Terraria.GameContent/QuickStacking.cs | D:\TRbackup\Version4\Terraria.GameContent\QuickStacking.cs | 23 | 3 | transferBlocked | bool | `public bool transferBlocked;` | `public bool transferBlocked;` |
| 2492 | field | Terraria.GameContent.QuickStacking.MatchingItemTypeDestinationList.LinkedEntry | Terraria.GameContent/QuickStacking.cs | D:\TRbackup\Version4\Terraria.GameContent\QuickStacking.cs | 38 | 4 | value | Terraria.GameContent.QuickStacking.DestinationHelper | `public DestinationHelper value;` | `public DestinationHelper value;` |
| 2493 | field | Terraria.GameContent.QuickStacking.MatchingItemTypeDestinationList.LinkedEntry | Terraria.GameContent/QuickStacking.cs | D:\TRbackup\Version4\Terraria.GameContent\QuickStacking.cs | 40 | 4 | next | int | `public int next;` | `public int next;` |
| 2494 | field | Terraria.GameContent.QuickStacking.MatchingItemTypeDestinationList | Terraria.GameContent/QuickStacking.cs | D:\TRbackup\Version4\Terraria.GameContent\QuickStacking.cs | 43 | 3 | entries | Terraria.GameContent.QuickStacking.MatchingItemTypeDestinationList.LinkedEntry[] | `private LinkedEntry[] entries = new LinkedEntry[1000];` | `private LinkedEntry[] entries = new LinkedEntry[1000];` |
| 2495 | field | Terraria.GameContent.QuickStacking.MatchingItemTypeDestinationList | Terraria.GameContent/QuickStacking.cs | D:\TRbackup\Version4\Terraria.GameContent\QuickStacking.cs | 45 | 3 | firstEntryForType | int[] | `private int[] firstEntryForType = new int[ItemID.Count];` | `private int[] firstEntryForType = new int[ItemID.Count];` |
| 2496 | field | Terraria.GameContent.QuickStacking.SourceInventory | Terraria.GameContent/QuickStacking.cs | D:\TRbackup\Version4\Terraria.GameContent\QuickStacking.cs | 64 | 3 | items | Terraria.Item[] | `public Item[] items;` | `public Item[] items;` |
| 2497 | field | Terraria.GameContent.QuickStacking.SourceInventory | Terraria.GameContent/QuickStacking.cs | D:\TRbackup\Version4\Terraria.GameContent\QuickStacking.cs | 66 | 3 | numItems | int | `public int numItems;` | `public int numItems;` |
| 2498 | field | Terraria.GameContent.QuickStacking.SourceInventory | Terraria.GameContent/QuickStacking.cs | D:\TRbackup\Version4\Terraria.GameContent\QuickStacking.cs | 68 | 3 | slots | Terraria.ID.PlayerItemSlotID.SlotReference[] | `public PlayerItemSlotID.SlotReference[] slots;` | `public PlayerItemSlotID.SlotReference[] slots;` |
| 2499 | field | Terraria.GameContent.QuickStacking.SourceInventory | Terraria.GameContent/QuickStacking.cs | D:\TRbackup\Version4\Terraria.GameContent\QuickStacking.cs | 70 | 3 | transferBlocked | bool[] | `public bool[] transferBlocked;` | `public bool[] transferBlocked;` |
| 2500 | field | Terraria.GameContent.QuickStacking.SourceInventory | Terraria.GameContent/QuickStacking.cs | D:\TRbackup\Version4\Terraria.GameContent\QuickStacking.cs | 72 | 3 | position | Vector2 | `public Vector2 position;` | `public Vector2 position;` |
| 2501 | field | Terraria.GameContent.QuickStacking | Terraria.GameContent/QuickStacking.cs | D:\TRbackup\Version4\Terraria.GameContent\QuickStacking.cs | 75 | 2 | destHelperListScratch | System.Collections.Generic.List<Terraria.GameContent.QuickStacking.DestinationHelper> | `private static List<DestinationHelper> destHelperListScratch = new List<DestinationHelper>();` | `private static List<DestinationHelper> destHelperListScratch = new List<DestinationHelper>();` |
| 2502 | field | Terraria.GameContent.QuickStacking | Terraria.GameContent/QuickStacking.cs | D:\TRbackup\Version4\Terraria.GameContent\QuickStacking.cs | 77 | 2 | matchingItemTypeScratch | Terraria.GameContent.QuickStacking.MatchingItemTypeDestinationList | `private static MatchingItemTypeDestinationList matchingItemTypeScratch = new MatchingItemTypeDestinationList();` | `private static MatchingItemTypeDestinationList matchingItemTypeScratch = new MatchingItemTypeDestinationList();` |
| 2503 | field | Terraria.GameContent.QuickStacking | Terraria.GameContent/QuickStacking.cs | D:\TRbackup\Version4\Terraria.GameContent\QuickStacking.cs | 79 | 2 | _blockedChests | System.Collections.Generic.List<int> | `private static List<int> _blockedChests = new List<int>();` | `private static List<int> _blockedChests = new List<int>();` |

#### 属性（0）

无该类型成员记录。


### 4.7 细分子系统：`ItemTransferSettings`

- 原报告章节：`4.9.68`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`09` / `物品、库存与容器`

- 上一级基线细分子系统：`ItemTransferSettings`
- 细分职责：物品领取和转移行为设置。
- 边界角色：`definition/state`；最小 seam：item transfer settings port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：16；属性：0；合计：16。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（16）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3090 | field | Terraria.GetItemSettings | Terraria/GetItemSettings.cs | D:\TRbackup\Version4\Terraria\GetItemSettings.cs | 7 | 2 | GiftRecieved | Terraria.GetItemSettings | `public static GetItemSettings GiftRecieved = new GetItemSettings(LongText: true);` | `public static GetItemSettings GiftRecieved = new GetItemSettings(LongText: true);` |
| 3091 | field | Terraria.GetItemSettings | Terraria/GetItemSettings.cs | D:\TRbackup\Version4\Terraria\GetItemSettings.cs | 9 | 2 | LootAllFromBank | Terraria.GetItemSettings | `public static GetItemSettings LootAllFromBank = default(GetItemSettings);` | `public static GetItemSettings LootAllFromBank = default(GetItemSettings);` |
| 3092 | field | Terraria.GetItemSettings | Terraria/GetItemSettings.cs | D:\TRbackup\Version4\Terraria\GetItemSettings.cs | 11 | 2 | LootAllFromChest | Terraria.GetItemSettings | `public static GetItemSettings LootAllFromChest = new GetItemSettings(LongText: false, NoText: false, CanGoIntoVoidVault: true);` | `public static GetItemSettings LootAllFromChest = new GetItemSettings(LongText: false, NoText: false, CanGoIntoVoidVault: true);` |
| 3093 | field | Terraria.GetItemSettings | Terraria/GetItemSettings.cs | D:\TRbackup\Version4\Terraria\GetItemSettings.cs | 13 | 2 | PickupItemFromWorld | Terraria.GetItemSettings | `public static GetItemSettings PickupItemFromWorld = new GetItemSettings(LongText: false, NoText: false, CanGoIntoVoidVault: true);` | `public static GetItemSettings PickupItemFromWorld = new GetItemSettings(LongText: false, NoText: false, CanGoIntoVoidVault: true);` |
| 3094 | field | Terraria.GetItemSettings | Terraria/GetItemSettings.cs | D:\TRbackup\Version4\Terraria\GetItemSettings.cs | 15 | 2 | QuickTransferFromSlot | Terraria.GetItemSettings | `public static GetItemSettings QuickTransferFromSlot = new GetItemSettings(LongText: false, NoText: true);` | `public static GetItemSettings QuickTransferFromSlot = new GetItemSettings(LongText: false, NoText: true);` |
| 3095 | field | Terraria.GetItemSettings | Terraria/GetItemSettings.cs | D:\TRbackup\Version4\Terraria\GetItemSettings.cs | 17 | 2 | ReturnItemFromSlot | Terraria.GetItemSettings | `public static GetItemSettings ReturnItemFromSlot = new GetItemSettings(LongText: false, NoText: true);` | `public static GetItemSettings ReturnItemFromSlot = new GetItemSettings(LongText: false, NoText: true);` |
| 3096 | field | Terraria.GetItemSettings | Terraria/GetItemSettings.cs | D:\TRbackup\Version4\Terraria\GetItemSettings.cs | 19 | 2 | ReturnItemShowAsNew | Terraria.GetItemSettings | `public static GetItemSettings ReturnItemShowAsNew = new GetItemSettings(LongText: false, NoText: true, CanGoIntoVoidVault: false, NoSound: false, MakeNewAndShiny);` | `public static GetItemSettings ReturnItemShowAsNew = new GetItemSettings(LongText: false, NoText: true, CanGoIntoVoidVault: false, NoSound: false, MakeNewAndShiny);` |
| 3097 | field | Terraria.GetItemSettings | Terraria/GetItemSettings.cs | D:\TRbackup\Version4\Terraria\GetItemSettings.cs | 21 | 2 | ItemCreatedFromItemUsage | Terraria.GetItemSettings | `public static GetItemSettings ItemCreatedFromItemUsage = default(GetItemSettings);` | `public static GetItemSettings ItemCreatedFromItemUsage = default(GetItemSettings);` |
| 3098 | field | Terraria.GetItemSettings | Terraria/GetItemSettings.cs | D:\TRbackup\Version4\Terraria\GetItemSettings.cs | 23 | 2 | RefundConsumedItem | Terraria.GetItemSettings | `public static GetItemSettings RefundConsumedItem = new GetItemSettings(LongText: false, NoText: true, CanGoIntoVoidVault: true, NoSound: true);` | `public static GetItemSettings RefundConsumedItem = new GetItemSettings(LongText: false, NoText: true, CanGoIntoVoidVault: true, NoSound: true);` |
| 3099 | field | Terraria.GetItemSettings | Terraria/GetItemSettings.cs | D:\TRbackup\Version4\Terraria\GetItemSettings.cs | 25 | 2 | ReturnItemShowAsNewNoCoinMerge | Terraria.GetItemSettings | `public static GetItemSettings ReturnItemShowAsNewNoCoinMerge = new GetItemSettings(LongText: false, NoText: true, CanGoIntoVoidVault: false, NoSound: false, MakeNewAndShiny, NoCoinMerge: true);` | `public static GetItemSettings ReturnItemShowAsNewNoCoinMerge = new GetItemSettings(LongText: false, NoText: true, CanGoIntoVoidVault: false, NoSound: false, MakeNewAndShiny, NoCoinMerge: true);` |
| 3100 | field | Terraria.GetItemSettings | Terraria/GetItemSettings.cs | D:\TRbackup\Version4\Terraria\GetItemSettings.cs | 27 | 2 | LongText | bool | `public readonly bool LongText;` | `public readonly bool LongText;` |
| 3101 | field | Terraria.GetItemSettings | Terraria/GetItemSettings.cs | D:\TRbackup\Version4\Terraria\GetItemSettings.cs | 29 | 2 | NoText | bool | `public readonly bool NoText;` | `public readonly bool NoText;` |
| 3102 | field | Terraria.GetItemSettings | Terraria/GetItemSettings.cs | D:\TRbackup\Version4\Terraria\GetItemSettings.cs | 31 | 2 | CanGoIntoVoidVault | bool | `public readonly bool CanGoIntoVoidVault;` | `public readonly bool CanGoIntoVoidVault;` |
| 3103 | field | Terraria.GetItemSettings | Terraria/GetItemSettings.cs | D:\TRbackup\Version4\Terraria\GetItemSettings.cs | 33 | 2 | NoSound | bool | `public readonly bool NoSound;` | `public readonly bool NoSound;` |
| 3104 | field | Terraria.GetItemSettings | Terraria/GetItemSettings.cs | D:\TRbackup\Version4\Terraria\GetItemSettings.cs | 35 | 2 | NoCoinMerge | bool | `public readonly bool NoCoinMerge;` | `public readonly bool NoCoinMerge;` |
| 3105 | field | Terraria.GetItemSettings | Terraria/GetItemSettings.cs | D:\TRbackup\Version4\Terraria\GetItemSettings.cs | 37 | 2 | StepAfterHandlingSlotNormally | System.Action<Terraria.Item> | `public readonly Action<Item> StepAfterHandlingSlotNormally;` | `public readonly Action<Item> StepAfterHandlingSlotNormally;` |

#### 属性（0）

无该类型成员记录。


### 4.8 细分子系统：`SharedWorldItemLifecycleState`

- 原报告章节：`4.9.126`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`09` / `物品、库存与容器`

- 上一级基线细分子系统：`SharedWorldItemState`
- 细分职责：世界物品保留、拾取、传送带和生命周期状态。
- 边界角色：`state`；最小 seam：world item lifecycle port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：15；属性：1；合计：16。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（15）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3667 | field | Terraria.WorldItem | Terraria/WorldItem.cs | D:\TRbackup\Version4\Terraria\WorldItem.cs | 17 | 2 | inner | Terraria.Item | `public Item inner = new Item();` | `public Item inner = new Item();` |
| 3668 | field | Terraria.WorldItem | Terraria/WorldItem.cs | D:\TRbackup\Version4\Terraria\WorldItem.cs | 19 | 2 | ownTime | int | `public int ownTime;` | `public int ownTime;` |
| 3669 | field | Terraria.WorldItem | Terraria/WorldItem.cs | D:\TRbackup\Version4\Terraria\WorldItem.cs | 21 | 2 | playerIndexTheItemIsReservedFor | int | `public int playerIndexTheItemIsReservedFor = 255;` | `public int playerIndexTheItemIsReservedFor = 255;` |
| 3670 | field | Terraria.WorldItem | Terraria/WorldItem.cs | D:\TRbackup\Version4\Terraria\WorldItem.cs | 23 | 2 | noGrabDelay | int | `public int noGrabDelay;` | `public int noGrabDelay;` |
| 3671 | field | Terraria.WorldItem | Terraria/WorldItem.cs | D:\TRbackup\Version4\Terraria\WorldItem.cs | 25 | 2 | shimmered | bool | `public bool shimmered;` | `public bool shimmered;` |
| 3672 | field | Terraria.WorldItem | Terraria/WorldItem.cs | D:\TRbackup\Version4\Terraria\WorldItem.cs | 27 | 2 | shimmerTime | float | `public float shimmerTime;` | `public float shimmerTime;` |
| 3673 | field | Terraria.WorldItem | Terraria/WorldItem.cs | D:\TRbackup\Version4\Terraria\WorldItem.cs | 29 | 2 | instanced | bool | `public bool instanced;` | `public bool instanced;` |
| 3674 | field | Terraria.WorldItem | Terraria/WorldItem.cs | D:\TRbackup\Version4\Terraria\WorldItem.cs | 31 | 2 | ownIgnore | int | `public int ownIgnore = -1;` | `public int ownIgnore = -1;` |
| 3675 | field | Terraria.WorldItem | Terraria/WorldItem.cs | D:\TRbackup\Version4\Terraria\WorldItem.cs | 33 | 2 | timeSinceTheItemHasBeenReservedForSomeone | int | `public int timeSinceTheItemHasBeenReservedForSomeone;` | `public int timeSinceTheItemHasBeenReservedForSomeone;` |
| 3676 | field | Terraria.WorldItem | Terraria/WorldItem.cs | D:\TRbackup\Version4\Terraria\WorldItem.cs | 35 | 2 | timeLeftInWhichTheItemCannotBeTakenByEnemies | int | `public int timeLeftInWhichTheItemCannotBeTakenByEnemies;` | `public int timeLeftInWhichTheItemCannotBeTakenByEnemies;` |
| 3677 | field | Terraria.WorldItem | Terraria/WorldItem.cs | D:\TRbackup\Version4\Terraria\WorldItem.cs | 37 | 2 | timeSinceItemSpawned | int | `public int timeSinceItemSpawned;` | `public int timeSinceItemSpawned;` |
| 3678 | field | Terraria.WorldItem | Terraria/WorldItem.cs | D:\TRbackup\Version4\Terraria\WorldItem.cs | 39 | 2 | beingGrabbed | bool | `public bool beingGrabbed;` | `public bool beingGrabbed;` |
| 3679 | field | Terraria.WorldItem | Terraria/WorldItem.cs | D:\TRbackup\Version4\Terraria\WorldItem.cs | 41 | 2 | onConveyor | bool | `public bool onConveyor;` | `public bool onConveyor;` |
| 3680 | field | Terraria.WorldItem | Terraria/WorldItem.cs | D:\TRbackup\Version4\Terraria\WorldItem.cs | 43 | 2 | keepTime | int | `public int keepTime;` | `public int keepTime;` |
| 3681 | field | Terraria.WorldItem | Terraria/WorldItem.cs | D:\TRbackup\Version4\Terraria\WorldItem.cs | 45 | 2 | _sceneMetrics | Terraria.SceneMetrics | `private static SceneMetrics _sceneMetrics;` | `private static SceneMetrics _sceneMetrics;` |

#### 属性（1）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 4025 | property | Terraria.WorldItem | Terraria/WorldItem.cs | D:\TRbackup\Version4\Terraria\WorldItem.cs | 47 | 2 | active | bool | `public bool active => inner.active;` | `public bool active => inner.active;` |


### 4.9 细分子系统：`SharedItemEquipmentSlotState`

- 原报告章节：`4.9.132`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`09` / `物品、库存与容器`

- 上一级基线细分子系统：`SharedItemEquipmentAndPresentationState`
- 细分职责：装备栏位、染料栏位和穿戴槽状态。
- 边界角色：`definition/state`；最小 seam：item equipment slot port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：21；属性：0；合计：21。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（21）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3176 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 96 | 2 | wornArmor | bool | `public bool wornArmor;` | `public bool wornArmor;` |
| 3221 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 186 | 2 | headSlot | int | `public int headSlot = -1;` | `public int headSlot = -1;` |
| 3222 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 188 | 2 | bodySlot | int | `public int bodySlot = -1;` | `public int bodySlot = -1;` |
| 3223 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 190 | 2 | legSlot | int | `public int legSlot = -1;` | `public int legSlot = -1;` |
| 3224 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 192 | 2 | handOnSlot | sbyte | `public sbyte handOnSlot = -1;` | `public sbyte handOnSlot = -1;` |
| 3225 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 194 | 2 | handOffSlot | sbyte | `public sbyte handOffSlot = -1;` | `public sbyte handOffSlot = -1;` |
| 3226 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 196 | 2 | backSlot | sbyte | `public sbyte backSlot = -1;` | `public sbyte backSlot = -1;` |
| 3227 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 198 | 2 | frontSlot | sbyte | `public sbyte frontSlot = -1;` | `public sbyte frontSlot = -1;` |
| 3228 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 200 | 2 | shoeSlot | sbyte | `public sbyte shoeSlot = -1;` | `public sbyte shoeSlot = -1;` |
| 3229 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 202 | 2 | waistSlot | sbyte | `public sbyte waistSlot = -1;` | `public sbyte waistSlot = -1;` |
| 3230 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 204 | 2 | wingSlot | sbyte | `public sbyte wingSlot = -1;` | `public sbyte wingSlot = -1;` |
| 3231 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 206 | 2 | shieldSlot | sbyte | `public sbyte shieldSlot = -1;` | `public sbyte shieldSlot = -1;` |
| 3232 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 208 | 2 | neckSlot | sbyte | `public sbyte neckSlot = -1;` | `public sbyte neckSlot = -1;` |
| 3233 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 210 | 2 | faceSlot | sbyte | `public sbyte faceSlot = -1;` | `public sbyte faceSlot = -1;` |
| 3234 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 212 | 2 | balloonSlot | sbyte | `public sbyte balloonSlot = -1;` | `public sbyte balloonSlot = -1;` |
| 3235 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 214 | 2 | beardSlot | sbyte | `public sbyte beardSlot = -1;` | `public sbyte beardSlot = -1;` |
| 3236 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 216 | 2 | voiceSlot | sbyte | `public sbyte voiceSlot;` | `public sbyte voiceSlot;` |
| 3254 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 252 | 2 | social | bool | `public bool social;` | `public bool social;` |
| 3255 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 254 | 2 | vanity | bool | `public bool vanity;` | `public bool vanity;` |
| 3278 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 300 | 2 | newAndShiny | bool | `public bool newAndShiny;` | `public bool newAndShiny;` |
| 3279 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 302 | 2 | hasVanityEffects | bool | `[Old("This is used to allow items to be discerned as vanity even if they didn't have visual slots to poll against")] public bool hasVanityEffects;` | `[Old("This is used to allow items to be discerned as vanity even if they didn't have visual slots to poll against")] public bool hasVanityEffects;` |

#### 属性（0）

无该类型成员记录。


### 4.10 细分子系统：`SharedWorldItemIdentityAndEconomyPayloadState`

- 原报告章节：`4.9.188`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`09` / `物品、库存与容器`

- 上一级基线细分子系统：`SharedWorldItemState`
- 上一级 peer 细分子系统：`SharedWorldItemPayloadState`
- 细分职责：世界物品身份、堆叠、价值和经济投影载荷。
- 边界角色：`state/projection`；最小 seam：world item identity economy payload port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：0；属性：9；合计：9。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（0）

无该类型成员记录。

#### 属性（9）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 4026 | property | Terraria.WorldItem | Terraria/WorldItem.cs | D:\TRbackup\Version4\Terraria\WorldItem.cs | 49 | 2 | type | int | `public int type { get { return inner.type; } set { inner.type = value; } }` | `public int type { get { return inner.type; } set { inner.type = value; } }` |
| 4027 | property | Terraria.WorldItem | Terraria/WorldItem.cs | D:\TRbackup\Version4\Terraria\WorldItem.cs | 61 | 2 | stack | int | `public int stack { get { return inner.stack; } set { inner.stack = value; } }` | `public int stack { get { return inner.stack; } set { inner.stack = value; } }` |
| 4030 | property | Terraria.WorldItem | Terraria/WorldItem.cs | D:\TRbackup\Version4\Terraria\WorldItem.cs | 97 | 2 | favorited | bool | `public bool favorited { get { return inner.favorited; } set { inner.favorited = value; } }` | `public bool favorited { get { return inner.favorited; } set { inner.favorited = value; } }` |
| 4032 | property | Terraria.WorldItem | Terraria/WorldItem.cs | D:\TRbackup\Version4\Terraria\WorldItem.cs | 121 | 2 | value | int | `public int value => inner.value;` | `public int value => inner.value;` |
| 4036 | property | Terraria.WorldItem | Terraria/WorldItem.cs | D:\TRbackup\Version4\Terraria\WorldItem.cs | 129 | 2 | maxStack | int | `public int maxStack => inner.maxStack;` | `public int maxStack => inner.maxStack;` |
| 4044 | property | Terraria.WorldItem | Terraria/WorldItem.cs | D:\TRbackup\Version4\Terraria\WorldItem.cs | 145 | 2 | rare | int | `public int rare => inner.rare;` | `public int rare => inner.rare;` |
| 4049 | property | Terraria.WorldItem | Terraria/WorldItem.cs | D:\TRbackup\Version4\Terraria\WorldItem.cs | 155 | 2 | Name | string | `public string Name => inner.Name;` | `public string Name => inner.Name;` |
| 4052 | property | Terraria.WorldItem | Terraria/WorldItem.cs | D:\TRbackup\Version4\Terraria\WorldItem.cs | 161 | 2 | IsACoin | bool | `public bool IsACoin => inner.IsACoin;` | `public bool IsACoin => inner.IsACoin;` |
| 4053 | property | Terraria.WorldItem | Terraria/WorldItem.cs | D:\TRbackup\Version4\Terraria\WorldItem.cs | 163 | 2 | IsAir | bool | `public bool IsAir => inner.IsAir;` | `public bool IsAir => inner.IsAir;` |


### 4.11 细分子系统：`SharedWorldItemUseAndPresentationPayloadState`

- 原报告章节：`4.9.189`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`09` / `物品、库存与容器`

- 上一级基线细分子系统：`SharedWorldItemState`
- 上一级 peer 细分子系统：`SharedWorldItemPayloadState`
- 细分职责：世界物品使用、伤害、工具和表现投影载荷。
- 边界角色：`state/projection`；最小 seam：world item use presentation payload port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：0；属性：19；合计：19。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（0）

无该类型成员记录。

#### 属性（19）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 4028 | property | Terraria.WorldItem | Terraria/WorldItem.cs | D:\TRbackup\Version4\Terraria\WorldItem.cs | 73 | 2 | newAndShiny | bool | `public bool newAndShiny { get { return inner.newAndShiny; } set { inner.newAndShiny = value; } }` | `public bool newAndShiny { get { return inner.newAndShiny; } set { inner.newAndShiny = value; } }` |
| 4029 | property | Terraria.WorldItem | Terraria/WorldItem.cs | D:\TRbackup\Version4\Terraria\WorldItem.cs | 85 | 2 | color | Color | `public Color color { get { return inner.color; } set { inner.color = value; } }` | `public Color color { get { return inner.color; } set { inner.color = value; } }` |
| 4031 | property | Terraria.WorldItem | Terraria/WorldItem.cs | D:\TRbackup\Version4\Terraria\WorldItem.cs | 109 | 2 | makeNPC | short | `public short makeNPC { get { return inner.makeNPC; } set { inner.makeNPC = value; } }` | `public short makeNPC { get { return inner.makeNPC; } set { inner.makeNPC = value; } }` |
| 4033 | property | Terraria.WorldItem | Terraria/WorldItem.cs | D:\TRbackup\Version4\Terraria\WorldItem.cs | 123 | 2 | useTime | int | `public int useTime => inner.useTime;` | `public int useTime => inner.useTime;` |
| 4034 | property | Terraria.WorldItem | Terraria/WorldItem.cs | D:\TRbackup\Version4\Terraria\WorldItem.cs | 125 | 2 | useAnimation | int | `public int useAnimation => inner.useAnimation;` | `public int useAnimation => inner.useAnimation;` |
| 4035 | property | Terraria.WorldItem | Terraria/WorldItem.cs | D:\TRbackup\Version4\Terraria\WorldItem.cs | 127 | 2 | useAmmo | int | `public int useAmmo => inner.useAmmo;` | `public int useAmmo => inner.useAmmo;` |
| 4037 | property | Terraria.WorldItem | Terraria/WorldItem.cs | D:\TRbackup\Version4\Terraria\WorldItem.cs | 131 | 2 | damage | int | `public int damage => inner.damage;` | `public int damage => inner.damage;` |
| 4038 | property | Terraria.WorldItem | Terraria/WorldItem.cs | D:\TRbackup\Version4\Terraria\WorldItem.cs | 133 | 2 | knockBack | float | `public float knockBack => inner.knockBack;` | `public float knockBack => inner.knockBack;` |
| 4039 | property | Terraria.WorldItem | Terraria/WorldItem.cs | D:\TRbackup\Version4\Terraria\WorldItem.cs | 135 | 2 | shootSpeed | float | `public float shootSpeed => inner.shootSpeed;` | `public float shootSpeed => inner.shootSpeed;` |
| 4040 | property | Terraria.WorldItem | Terraria/WorldItem.cs | D:\TRbackup\Version4\Terraria\WorldItem.cs | 137 | 2 | scale | float | `public float scale => inner.scale;` | `public float scale => inner.scale;` |
| 4041 | property | Terraria.WorldItem | Terraria/WorldItem.cs | D:\TRbackup\Version4\Terraria\WorldItem.cs | 139 | 2 | ammo | int | `public int ammo => inner.ammo;` | `public int ammo => inner.ammo;` |
| 4042 | property | Terraria.WorldItem | Terraria/WorldItem.cs | D:\TRbackup\Version4\Terraria\WorldItem.cs | 141 | 2 | notAmmo | bool | `public bool notAmmo => inner.notAmmo;` | `public bool notAmmo => inner.notAmmo;` |
| 4043 | property | Terraria.WorldItem | Terraria/WorldItem.cs | D:\TRbackup\Version4\Terraria\WorldItem.cs | 143 | 2 | shoot | int | `public int shoot => inner.shoot;` | `public int shoot => inner.shoot;` |
| 4045 | property | Terraria.WorldItem | Terraria/WorldItem.cs | D:\TRbackup\Version4\Terraria\WorldItem.cs | 147 | 2 | placeStyle | int | `public int placeStyle => inner.placeStyle;` | `public int placeStyle => inner.placeStyle;` |
| 4046 | property | Terraria.WorldItem | Terraria/WorldItem.cs | D:\TRbackup\Version4\Terraria\WorldItem.cs | 149 | 2 | createTile | int | `public int createTile => inner.createTile;` | `public int createTile => inner.createTile;` |
| 4047 | property | Terraria.WorldItem | Terraria/WorldItem.cs | D:\TRbackup\Version4\Terraria\WorldItem.cs | 151 | 2 | glowMask | int | `public int glowMask => inner.glowMask;` | `public int glowMask => inner.glowMask;` |
| 4048 | property | Terraria.WorldItem | Terraria/WorldItem.cs | D:\TRbackup\Version4\Terraria\WorldItem.cs | 153 | 2 | expert | bool | `public bool expert => inner.expert;` | `public bool expert => inner.expert;` |
| 4050 | property | Terraria.WorldItem | Terraria/WorldItem.cs | D:\TRbackup\Version4\Terraria\WorldItem.cs | 157 | 2 | alpha | int | `public int alpha => inner.alpha;` | `public int alpha => inner.alpha;` |
| 4051 | property | Terraria.WorldItem | Terraria/WorldItem.cs | D:\TRbackup\Version4\Terraria\WorldItem.cs | 159 | 2 | buffType | int | `public int buffType => inner.buffType;` | `public int buffType => inner.buffType;` |


### 4.12 细分子系统：`SharedItemEmergencyStackingPolicyState`

- 原报告章节：`4.9.190`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`09` / `物品、库存与容器`

- 上一级基线细分子系统：`ItemEmergencyStackingState`
- 上一级 peer 细分子系统：`ItemEmergencyStackingState`
- 细分职责：紧急堆叠候选、距离和策略配置。
- 边界角色：`state/query`；最小 seam：emergency stacking policy port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：6；属性：0；合计：6。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（6）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2403 | field | Terraria.GameContent.EmergencyStacking | Terraria.GameContent/EmergencyStacking.cs | D:\TRbackup\Version4\Terraria.GameContent\EmergencyStacking.cs | 130 | 2 | PreservationOrder | System.Collections.Generic.List<Terraria.GameContent.EmergencyStacking.Group> | `public static readonly List<Group> PreservationOrder = new List<Group>  	{  		Group.RareCurrency,  		Group.Equipment,  		Group.SilverCoins,  		Group.CopperCoins,  		Group.FallenStars,  		Group.Default  	};` | `public static readonly List<Group> PreservationOrder = new List<Group> { Group.RareCurrency, Group.Equipment, Group.SilverCoins, Group.CopperCoins, Group.FallenStars, Group.Default };` |
| 2404 | field | Terraria.GameContent.EmergencyStacking | Terraria.GameContent/EmergencyStacking.cs | D:\TRbackup\Version4\Terraria.GameContent\EmergencyStacking.cs | 140 | 2 | PlayerViewRectSize | Point | `private static readonly Point PlayerViewRectSize = new Point(2320, 1600);` | `private static readonly Point PlayerViewRectSize = new Point(2320, 1600);` |
| 2405 | field | Terraria.GameContent.EmergencyStacking | Terraria.GameContent/EmergencyStacking.cs | D:\TRbackup\Version4\Terraria.GameContent\EmergencyStacking.cs | 142 | 2 | ItemsToStackEachTime | int | `private static readonly int ItemsToStackEachTime = 20;` | `private static readonly int ItemsToStackEachTime = 20;` |
| 2406 | field | Terraria.GameContent.EmergencyStacking | Terraria.GameContent/EmergencyStacking.cs | D:\TRbackup\Version4\Terraria.GameContent\EmergencyStacking.cs | 144 | 2 | PendingTransfers | System.Collections.Generic.List<Terraria.GameContent.EmergencyStacking.Transfer> | `private static readonly List<Transfer> PendingTransfers = new List<Transfer>(ItemsToStackEachTime);` | `private static readonly List<Transfer> PendingTransfers = new List<Transfer>(ItemsToStackEachTime);` |
| 2407 | field | Terraria.GameContent.EmergencyStacking | Terraria.GameContent/EmergencyStacking.cs | D:\TRbackup\Version4\Terraria.GameContent\EmergencyStacking.cs | 146 | 2 | HasPendingTransfer | bool[] | `private static readonly bool[] HasPendingTransfer = new bool[401];` | `private static readonly bool[] HasPendingTransfer = new bool[401];` |
| 2408 | field | Terraria.GameContent.EmergencyStacking | Terraria.GameContent/EmergencyStacking.cs | D:\TRbackup\Version4\Terraria.GameContent\EmergencyStacking.cs | 148 | 2 | playerViewRectsScratch | System.Collections.Generic.List<Rectangle> | `private static readonly List<Rectangle> playerViewRectsScratch = new List<Rectangle>(255);` | `private static readonly List<Rectangle> playerViewRectsScratch = new List<Rectangle>(255);` |

#### 属性（0）

无该类型成员记录。


### 4.13 细分子系统：`SharedItemEmergencyStackingTransferState`

- 原报告章节：`4.9.191`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`09` / `物品、库存与容器`

- 上一级基线细分子系统：`ItemEmergencyStackingState`
- 上一级 peer 细分子系统：`ItemEmergencyStackingState`
- 细分职责：紧急堆叠 Group、StackableItem 和 Transfer 运行态。
- 边界角色：`state/query`；最小 seam：emergency stacking transfer port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：3；字段：19；属性：2；合计：21。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（19）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2384 | field | Terraria.GameContent.EmergencyStacking.Group | Terraria.GameContent/EmergencyStacking.cs | D:\TRbackup\Version4\Terraria.GameContent\EmergencyStacking.cs | 13 | 3 | DefaultStackDistanceStepSize | int | `public static readonly int DefaultStackDistanceStepSize = 160;` | `public static readonly int DefaultStackDistanceStepSize = 160;` |
| 2385 | field | Terraria.GameContent.EmergencyStacking.Group | Terraria.GameContent/EmergencyStacking.cs | D:\TRbackup\Version4\Terraria.GameContent\EmergencyStacking.cs | 15 | 3 | DistanceStepSize | int | `public int DistanceStepSize = DefaultStackDistanceStepSize;` | `public int DistanceStepSize = DefaultStackDistanceStepSize;` |
| 2386 | field | Terraria.GameContent.EmergencyStacking.Group | Terraria.GameContent/EmergencyStacking.cs | D:\TRbackup\Version4\Terraria.GameContent\EmergencyStacking.cs | 17 | 3 | Conditions | System.Collections.Generic.List<System.Predicate<Terraria.Item>> | `private List<Predicate<Item>> Conditions = new List<Predicate<Item>>();` | `private List<Predicate<Item>> Conditions = new List<Predicate<Item>>();` |
| 2387 | field | Terraria.GameContent.EmergencyStacking.Group | Terraria.GameContent/EmergencyStacking.cs | D:\TRbackup\Version4\Terraria.GameContent\EmergencyStacking.cs | 19 | 3 | StackingPriority | int | `internal int StackingPriority;` | `internal int StackingPriority;` |
| 2388 | field | Terraria.GameContent.EmergencyStacking.Group | Terraria.GameContent/EmergencyStacking.cs | D:\TRbackup\Version4\Terraria.GameContent\EmergencyStacking.cs | 21 | 3 | FallenStars | Terraria.GameContent.EmergencyStacking.Group | `public static Group FallenStars = new Group(75)  		{  			DistanceStepSize = DefaultStackDistanceStepSize * 4  		};` | `public static Group FallenStars = new Group(75) { DistanceStepSize = DefaultStackDistanceStepSize * 4 };` |
| 2389 | field | Terraria.GameContent.EmergencyStacking.Group | Terraria.GameContent/EmergencyStacking.cs | D:\TRbackup\Version4\Terraria.GameContent\EmergencyStacking.cs | 26 | 3 | CopperCoins | Terraria.GameContent.EmergencyStacking.Group | `public static Group CopperCoins = new Group(71);` | `public static Group CopperCoins = new Group(71);` |
| 2390 | field | Terraria.GameContent.EmergencyStacking.Group | Terraria.GameContent/EmergencyStacking.cs | D:\TRbackup\Version4\Terraria.GameContent\EmergencyStacking.cs | 28 | 3 | SilverCoins | Terraria.GameContent.EmergencyStacking.Group | `public static Group SilverCoins = new Group(72);` | `public static Group SilverCoins = new Group(72);` |
| 2391 | field | Terraria.GameContent.EmergencyStacking.Group | Terraria.GameContent/EmergencyStacking.cs | D:\TRbackup\Version4\Terraria.GameContent\EmergencyStacking.cs | 30 | 3 | Equipment | Terraria.GameContent.EmergencyStacking.Group | `public static Group Equipment = new Group((Item item) => item.OnlyNeedOneInInventory());` | `public static Group Equipment = new Group((Item item) => item.OnlyNeedOneInInventory());` |
| 2392 | field | Terraria.GameContent.EmergencyStacking.Group | Terraria.GameContent/EmergencyStacking.cs | D:\TRbackup\Version4\Terraria.GameContent\EmergencyStacking.cs | 32 | 3 | RareCurrency | Terraria.GameContent.EmergencyStacking.Group | `public static Group RareCurrency = new Group  		{  			DistanceStepSize = DefaultStackDistanceStepSize / 4  		}.Add(73).Add(74).Add(3822);` | `public static Group RareCurrency = new Group { DistanceStepSize = DefaultStackDistanceStepSize / 4 }.Add(73).Add(74).Add(3822);` |
| 2393 | field | Terraria.GameContent.EmergencyStacking.Group | Terraria.GameContent/EmergencyStacking.cs | D:\TRbackup\Version4\Terraria.GameContent\EmergencyStacking.cs | 37 | 3 | Default | Terraria.GameContent.EmergencyStacking.Group | `public static Group Default = new Group((Item item) => true);` | `public static Group Default = new Group((Item item) => true);` |
| 2394 | field | Terraria.GameContent.EmergencyStacking.StackableItem | Terraria.GameContent/EmergencyStacking.cs | D:\TRbackup\Version4\Terraria.GameContent\EmergencyStacking.cs | 78 | 3 | type | int | `public int type;` | `public int type;` |
| 2395 | field | Terraria.GameContent.EmergencyStacking.StackableItem | Terraria.GameContent/EmergencyStacking.cs | D:\TRbackup\Version4\Terraria.GameContent\EmergencyStacking.cs | 80 | 3 | age | int | `public int age;` | `public int age;` |
| 2396 | field | Terraria.GameContent.EmergencyStacking.StackableItem | Terraria.GameContent/EmergencyStacking.cs | D:\TRbackup\Version4\Terraria.GameContent\EmergencyStacking.cs | 82 | 3 | isOnScreen | bool | `public bool isOnScreen;` | `public bool isOnScreen;` |
| 2397 | field | Terraria.GameContent.EmergencyStacking.StackableItem | Terraria.GameContent/EmergencyStacking.cs | D:\TRbackup\Version4\Terraria.GameContent\EmergencyStacking.cs | 84 | 3 | item | Terraria.WorldItem | `public WorldItem item;` | `public WorldItem item;` |
| 2398 | field | Terraria.GameContent.EmergencyStacking.Transfer | Terraria.GameContent/EmergencyStacking.cs | D:\TRbackup\Version4\Terraria.GameContent\EmergencyStacking.cs | 89 | 3 | src | Terraria.WorldItem | `public WorldItem src;` | `public WorldItem src;` |
| 2399 | field | Terraria.GameContent.EmergencyStacking.Transfer | Terraria.GameContent/EmergencyStacking.cs | D:\TRbackup\Version4\Terraria.GameContent\EmergencyStacking.cs | 91 | 3 | dst | Terraria.WorldItem | `public WorldItem dst;` | `public WorldItem dst;` |
| 2400 | field | Terraria.GameContent.EmergencyStacking.Transfer | Terraria.GameContent/EmergencyStacking.cs | D:\TRbackup\Version4\Terraria.GameContent\EmergencyStacking.cs | 93 | 3 | distanceOrder | int | `public int distanceOrder;` | `public int distanceOrder;` |
| 2401 | field | Terraria.GameContent.EmergencyStacking.Transfer | Terraria.GameContent/EmergencyStacking.cs | D:\TRbackup\Version4\Terraria.GameContent\EmergencyStacking.cs | 95 | 3 | preservationOrder | int | `public int preservationOrder;` | `public int preservationOrder;` |
| 2402 | field | Terraria.GameContent.EmergencyStacking.Transfer | Terraria.GameContent/EmergencyStacking.cs | D:\TRbackup\Version4\Terraria.GameContent\EmergencyStacking.cs | 97 | 3 | distance | int | `public int distance;` | `public int distance;` |

#### 属性（2）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3851 | property | Terraria.GameContent.EmergencyStacking.Transfer | Terraria.GameContent/EmergencyStacking.cs | D:\TRbackup\Version4\Terraria.GameContent\EmergencyStacking.cs | 99 | 3 | HasOwnership | bool | `public bool HasOwnership { get { if (src.playerIndexTheItemIsReservedFor == Main.myPlayer) { return dst.playerIndexTheItemIsReservedFor == Main.myPlayer; } return false; } }` | `public bool HasOwnership { get { if (src.playerIndexTheItemIsReservedFor == Main.myPlayer) { return dst.playerIndexTheItemIsReservedFor == Main.myPlayer; } return false; } }` |
| 3852 | property | Terraria.GameContent.EmergencyStacking.Transfer | Terraria.GameContent/EmergencyStacking.cs | D:\TRbackup\Version4\Terraria.GameContent\EmergencyStacking.cs | 111 | 3 | NumToTransfer | int | `public int NumToTransfer { get { if (!Item.CanStack(src.inner, dst.inner)) { return 0; } return Math.Min(src.stack, dst.maxStack - dst.stack); } }` | `public int NumToTransfer { get { if (!Item.CanStack(src.inner, dst.inner)) { return 0; } return Math.Min(src.stack, dst.maxStack - dst.stack); } }` |


### 4.14 细分子系统：`SharedItemToolPlacementCapabilityState`

- 原报告章节：`4.9.215`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`09` / `物品、库存与容器`

- 上一级基线细分子系统：`SharedItemUseToolAndCombatState`
- 上一级 peer 细分子系统：`SharedItemUseAndToolCapabilityState`
- 细分职责：物品采掘、放置、弹药和工具能力。
- 边界角色：`definition/state`；最小 seam：item tool placement capability port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：11；属性：0；合计：11。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（11）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3199 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 142 | 2 | pick | int | `public int pick;` | `public int pick;` |
| 3200 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 144 | 2 | axe | int | `public int axe;` | `public int axe;` |
| 3201 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 146 | 2 | hammer | int | `public int hammer;` | `public int hammer;` |
| 3202 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 148 | 2 | tileBoost | int | `public int tileBoost;` | `public int tileBoost;` |
| 3203 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 150 | 2 | createTile | int | `public int createTile = -1;` | `public int createTile = -1;` |
| 3204 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 152 | 2 | createWall | int | `public int createWall = -1;` | `public int createWall = -1;` |
| 3205 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 154 | 2 | placeStyle | int | `public int placeStyle;` | `public int placeStyle;` |
| 3243 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 230 | 2 | ammo | int | `public int ammo = AmmoID.None;` | `public int ammo = AmmoID.None;` |
| 3244 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 232 | 2 | notAmmo | bool | `public bool notAmmo;` | `public bool notAmmo;` |
| 3245 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 234 | 2 | useAmmo | int | `public int useAmmo = AmmoID.None;` | `public int useAmmo = AmmoID.None;` |
| 3256 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 256 | 2 | material | bool | `public bool material;` | `public bool material;` |

#### 属性（0）

无该类型成员记录。


## 5. 追溯与验收

> 来源报告 SHA-256：`b944441d92d5103f218b766a3e354200794528a19e65a862e44c2b40a36f1196`
- 本分区细分组数：14；成员数：191；字段：152；属性：39。
- 预期不变量：本文件只出现完整细分子系统；不跨分区复制成员；所有成员的来源序号、原始声明、路径、行列和 C# 类型必须与来源报告一致。
- 验证方式：重新读取 20 个文件，检查文件数、细分组唯一性、来源序号恰好覆盖 `1..4542`、字段/属性合计和逐成员行文本一致；本次分区生成不运行 `dotnet`，因为没有修改 C# 或项目文件。

生成器：`Build/Tools/Generate-Version4NonAuthoritativeComponentPartitionReports.ps1`。
