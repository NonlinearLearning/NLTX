# Version4 非权威组件拆分分区 10/20：经济、配方、钓鱼与掉落

> 来源报告：`docs/migration/ledgers/Version4非权威模拟系统字段属性逐成员源码声明-更细子系统拆分-去除ID类文件.md`
> 来源报告 SHA-256：`b944441d92d5103f218b766a3e354200794528a19e65a862e44c2b40a36f1196`
> 本文件是并行组件拆分工作包，不是新的权威成员清单；成员声明事实直接保留来源报告原文。

## 1. 分区定位与评估

- 分区范围：商店、价值规则、配方、钓鱼上下文、捕获、掉落解析和战利品。
- 本分区组件化重点：将 definition、随机解析、结果命令和消费/经济事务分开并隔离随机性。
- 本分区包含 24 个完整细分子系统、321 条成员记录（字段 296、属性 25）。来源序号覆盖区间 `405..4023`，区间可能与其他分区交错，不以序号定义领域边界。
- 组件边界判定：细分子系统只是并行处理的最小库存边界，不等价于一个组件。必须继续按共同读者/写者、变更原因、生命周期、权威性和副作用分为 Component、System、Query、Command、Adapter 或 Projection。
- 证据状态：成员声明和统计已由来源报告校验；逐成员读者、写者、初始化/清理路径、持久化/网络语义和系统顺序仍需本分区会话补证，不得从名称或数量推断。

## 2. 分区统计

| 父级子系统 | 细分子系统数 | 字段 | 属性 | 合计 |
|---|---:|---:|---:|---:|
| `RuntimeComposition` | 1 | 7 | 0 | 7 |
| `SharedRuntimeMechanisms` | 23 | 289 | 25 | 314 |

| 原报告章节 | 父级 | 细分子系统 | 边界角色 | 字段 | 属性 | 合计 | 组件化处理 |
|---|---|---|---|---:|---:|---:|---|
| `4.1.28` | `RuntimeComposition` | `MainShopAndQuestSlots` | runtime state | 7 | 0 | 7 | 待按成员访问模式拆分 |
| `4.9.1` | `SharedRuntimeMechanisms` | `SharedFishingCatchEffects` | command/adapter | 3 | 0 | 3 | 待按成员访问模式拆分 |
| `4.9.18` | `SharedRuntimeMechanisms` | `SharedItemCommerceState` | definition/state | 6 | 0 | 6 | 待按成员访问模式拆分 |
| `4.9.21` | `SharedRuntimeMechanisms` | `SharedItemCommerceAndPricing` | definition/state | 19 | 1 | 20 | 待按成员访问模式拆分 |
| `4.9.48` | `SharedRuntimeMechanisms` | `SharedLootSimulation` | query | 9 | 0 | 9 | 待按成员访问模式拆分 |
| `4.9.49` | `SharedRuntimeMechanisms` | `SharedDropSourceAttribution` | value object | 3 | 0 | 3 | 待按成员访问模式拆分 |
| `4.9.99` | `SharedRuntimeMechanisms` | `CraftingRequestState` | state/command | 6 | 1 | 7 | 待按成员访问模式拆分 |
| `4.9.114` | `SharedRuntimeMechanisms` | `SharedDropRuleResolutionAndCatalogState` | query/adapter | 19 | 2 | 21 | 待按成员访问模式拆分 |
| `4.9.115` | `SharedRuntimeMechanisms` | `SharedFishingConditionContextState` | query/state | 13 | 0 | 13 | 待按成员访问模式拆分 |
| `4.9.144` | `SharedRuntimeMechanisms` | `RecipeDefinitionState` | definition/catalog | 23 | 2 | 25 | 待按成员访问模式拆分 |
| `4.9.145` | `SharedRuntimeMechanisms` | `RecipeGroupCatalogState` | definition/catalog | 7 | 1 | 8 | 待按成员访问模式拆分 |
| `4.9.150` | `SharedRuntimeMechanisms` | `FishingAttemptState` | state/query | 24 | 0 | 24 | 待按成员访问模式拆分 |
| `4.9.151` | `SharedRuntimeMechanisms` | `PlayerFishingConditionState` | query/state | 6 | 0 | 6 | 待按成员访问模式拆分 |
| `4.9.168` | `SharedRuntimeMechanisms` | `SharedDropRuleChainState` | definition/query | 3 | 18 | 21 | 待按成员访问模式拆分 |
| `4.9.170` | `SharedRuntimeMechanisms` | `SharedFishingDropResolutionState` | query/adapter | 8 | 0 | 8 | 待按成员访问模式拆分 |
| `4.9.178` | `SharedRuntimeMechanisms` | `SharedItemEconomyAndValueRules` | definition/catalog | 12 | 0 | 12 | 待按成员访问模式拆分 |
| `4.9.179` | `SharedRuntimeMechanisms` | `SharedItemUseTimingAndStackRules` | definition/catalog | 18 | 0 | 18 | 待按成员访问模式拆分 |
| `4.9.199` | `SharedRuntimeMechanisms` | `SharedFishingRarityConditionCatalogState` | definition/catalog | 11 | 0 | 11 | 待按成员访问模式拆分 |
| `4.9.200` | `SharedRuntimeMechanisms` | `SharedDropRuleConditionBranchState` | definition/query | 9 | 0 | 9 | 待按成员访问模式拆分 |
| `4.9.206` | `SharedRuntimeMechanisms` | `SharedFishingConditionCatalogPopulationState` | definition/catalog | 8 | 0 | 8 | 待按成员访问模式拆分 |
| `4.9.207` | `SharedRuntimeMechanisms` | `SharedFishingEnvironmentPredicateState` | definition/query | 31 | 0 | 31 | 待按成员访问模式拆分 |
| `4.9.208` | `SharedRuntimeMechanisms` | `SharedDropRuleChanceAndQuantityState` | definition/query | 16 | 0 | 16 | 待按成员访问模式拆分 |
| `4.9.209` | `SharedRuntimeMechanisms` | `SharedDropRuleOptionSelectionState` | definition/query | 20 | 0 | 20 | 待按成员访问模式拆分 |
| `4.9.214` | `SharedRuntimeMechanisms` | `SharedItemUseTimingAndConsumptionState` | definition/state | 15 | 0 | 15 | 待按成员访问模式拆分 |

## 3. 并行组件拆分契约

对本文件中的每个细分子系统和每条成员记录执行以下判断，并把结果写入对应的实现阶段设计/审计记录：

- 盘点声明类型、可见性、读者、写者、创建/更新/清理/持久化生命周期、访问频率和副作用；缺证据标记 `missing`，不可用命名猜测补齐。
- 标记权威状态、派生值、缓存、快照、兼容字段和表现数据；Component 只保存一个内聚概念的权威数据。
- 需要行为转换时放入显式 System/Command；纯资格判断和派生计算放入 Query；外部协议、文件、平台和第三方类型放入 Adapter；网络/持久化/日志/UI 输出放入单向 Projection。
- 分别建模实体引用、持久化 ID、网络 ID 和外部 ID；不得把 ID 类文件或外部对象直接塞入共享组件。
- 记录 Interface、Implementation、Seam、Depth、Leverage、Locality，以及必要的系统先后约束、失败归属和重试策略。

## 4. 逐成员源码声明

以下细分子系统块保持来源报告的成员表、声明、路径、行列和来源序号；只调整 Markdown 标题层级并增加分区追踪元数据。

### 4.1 细分子系统：`MainShopAndQuestSlots`

- 原报告章节：`4.1.28`
- 父级子系统：`RuntimeComposition`
- 分区工作包：`10` / `经济、配方、钓鱼与掉落`

- 上一级基线细分子系统：`MainShopAndQuestSlots`
- 细分职责：商店容器、旅行商店和渔夫任务槽。
- 边界角色：`runtime state`；最小 seam：shop/quest slot view；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：7；属性：0；合计：7。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（7）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 405 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 991 | 2 | shop | Terraria.Chest[] | `public Chest[] shop = new Chest[100];` | `public Chest[] shop = new Chest[100];` |
| 406 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 994 | 2 | TravelShopMaxSlots | int | `public static readonly int TravelShopMaxSlots = 40;` | `public static readonly int TravelShopMaxSlots = 40;` |
| 407 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 996 | 2 | travelShop | int[] | `public static int[] travelShop = new int[TravelShopMaxSlots];` | `public static int[] travelShop = new int[TravelShopMaxSlots];` |
| 408 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 998 | 2 | anglerWhoFinishedToday | System.Collections.Generic.List<string> | `public static List<string> anglerWhoFinishedToday = new List<string>();` | `public static List<string> anglerWhoFinishedToday = new List<string>();` |
| 409 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1000 | 2 | anglerQuestFinished | bool | `public static bool anglerQuestFinished;` | `public static bool anglerQuestFinished;` |
| 410 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1002 | 2 | anglerQuest | int | `public static int anglerQuest;` | `public static int anglerQuest;` |
| 411 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1004 | 2 | anglerQuestItemNetIDs | int[] | `public static int[] anglerQuestItemNetIDs = new int[41]  	{  		2450, 2451, 2452, 2453, 2454, 2455, 2456, 2457, 2458, 2459,  		2460, 2461, 2462, 2463, 2464, 2465, 2466, 2467, 2468, 2469,  		2470, 2471, 2472, 2473, 2474, 2475, 2476, 2477, 2478, 2479,  		2480, 2481, 2482, 2483, 2484, 2485, 2486, 2487, 2488, 4393,  		4394  	};` | `public static int[] anglerQuestItemNetIDs = new int[41] { 2450, 2451, 2452, 2453, 2454, 2455, 2456, 2457, 2458, 2459, 2460, 2461, 2462, 2463, 2464, 2465, 2466, 2467, 2468, 2469, 2470, 2471, 2472, 2473, 2474, 2475, 2476, 2477, 2478, 2479, 2480, 2481, 2482, 2483, 2484, 2485, 2486, 2487, 2488, 4393, 4394 };` |

#### 属性（0）

无该类型成员记录。


### 4.2 细分子系统：`SharedFishingCatchEffects`

- 原报告章节：`4.9.1`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`10` / `经济、配方、钓鱼与掉落`

- 上一级基线细分子系统：`SharedFishingCatchEffects`
- 细分职责：鱼获交付、鱼饵桶辅助和来源归因。
- 边界角色：`command/adapter`；最小 seam：fishing catch effect port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：2；声明类型数：2；字段：3；属性：0；合计：3。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（3）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1130 | field | Terraria.DataStructures.EntitySource_FishedOut | Terraria.DataStructures/EntitySource_FishedOut.cs | D:\TRbackup\Version4\Terraria.DataStructures\EntitySource_FishedOut.cs | 5 | 2 | Entity | Terraria.IEntitySourceTarget | `public readonly IEntitySourceTarget Entity;` | `public readonly IEntitySourceTarget Entity;` |
| 2350 | field | Terraria.GameContent.ChumBucketProjectileHelper | Terraria.GameContent/ChumBucketProjectileHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\ChumBucketProjectileHelper.cs | 8 | 2 | _chumCountsPendingForThisFrame | System.Collections.Generic.Dictionary<Point, int> | `private Dictionary<Point, int> _chumCountsPendingForThisFrame = new Dictionary<Point, int>();` | `private Dictionary<Point, int> _chumCountsPendingForThisFrame = new Dictionary<Point, int>();` |
| 2351 | field | Terraria.GameContent.ChumBucketProjectileHelper | Terraria.GameContent/ChumBucketProjectileHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\ChumBucketProjectileHelper.cs | 10 | 2 | _chumCountsFromLastFrame | System.Collections.Generic.Dictionary<Point, int> | `private Dictionary<Point, int> _chumCountsFromLastFrame = new Dictionary<Point, int>();` | `private Dictionary<Point, int> _chumCountsFromLastFrame = new Dictionary<Point, int>();` |

#### 属性（0）

无该类型成员记录。


### 4.3 细分子系统：`SharedItemCommerceState`

- 原报告章节：`4.9.18`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`10` / `经济、配方、钓鱼与掉落`

- 上一级基线细分子系统：`SharedItemCommerceState`
- 细分职责：商店资格、买卖价格和特殊货币状态。
- 边界角色：`definition/state`；最小 seam：item commerce port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：6；属性：0；合计：6。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（6）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3185 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 114 | 2 | isAShopItem | bool | `public bool isAShopItem;` | `public bool isAShopItem;` |
| 3248 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 240 | 2 | buyOnce | bool | `public bool buyOnce;` | `public bool buyOnce;` |
| 3252 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 248 | 2 | value | int | `public int value;` | `public int value;` |
| 3253 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 250 | 2 | buy | bool | `public bool buy;` | `public bool buy;` |
| 3263 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 270 | 2 | shopSpecialCurrency | int | `public int shopSpecialCurrency = -1;` | `public int shopSpecialCurrency = -1;` |
| 3264 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 272 | 2 | shopCustomPrice | int? | `public int? shopCustomPrice;` | `public int? shopCustomPrice;` |

#### 属性（0）

无该类型成员记录。


### 4.4 细分子系统：`SharedItemCommerceAndPricing`

- 原报告章节：`4.9.21`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`10` / `经济、配方、钓鱼与掉落`

- 上一级基线细分子系统：`SharedItemCommerceAndPricing`
- 细分职责：商店价格、回售和购物设置。
- 边界角色：`definition/state`；最小 seam：commerce pricing port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：3；声明类型数：4；字段：19；属性：1；合计：20。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（19）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2429 | field | Terraria.GameContent.ItemShopSellbackHelper.ItemMemo | Terraria.GameContent/ItemShopSellbackHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\ItemShopSellbackHelper.cs | 9 | 3 | type | int | `public readonly int type;` | `public readonly int type;` |
| 2430 | field | Terraria.GameContent.ItemShopSellbackHelper.ItemMemo | Terraria.GameContent/ItemShopSellbackHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\ItemShopSellbackHelper.cs | 11 | 3 | prefix | int | `public readonly int prefix;` | `public readonly int prefix;` |
| 2431 | field | Terraria.GameContent.ItemShopSellbackHelper.ItemMemo | Terraria.GameContent/ItemShopSellbackHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\ItemShopSellbackHelper.cs | 13 | 3 | stack | int | `public int stack;` | `public int stack;` |
| 2432 | field | Terraria.GameContent.ItemShopSellbackHelper | Terraria.GameContent/ItemShopSellbackHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\ItemShopSellbackHelper.cs | 23 | 2 | _memos | System.Collections.Generic.List<Terraria.GameContent.ItemShopSellbackHelper.ItemMemo> | `private List<ItemMemo> _memos = new List<ItemMemo>();` | `private List<ItemMemo> _memos = new List<ItemMemo>();` |
| 2506 | field | Terraria.GameContent.ShopHelper | Terraria.GameContent/ShopHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\ShopHelper.cs | 12 | 2 | LowestPossiblePriceMultiplier | float | `public const float LowestPossiblePriceMultiplier = 0.75f;` | `public const float LowestPossiblePriceMultiplier = 0.75f;` |
| 2507 | field | Terraria.GameContent.ShopHelper | Terraria.GameContent/ShopHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\ShopHelper.cs | 14 | 2 | MaxHappinessAchievementPriceMultiplier | float | `public const float MaxHappinessAchievementPriceMultiplier = 0.82f;` | `public const float MaxHappinessAchievementPriceMultiplier = 0.82f;` |
| 2508 | field | Terraria.GameContent.ShopHelper | Terraria.GameContent/ShopHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\ShopHelper.cs | 16 | 2 | HighestPossiblePriceMultiplier | float | `public const float HighestPossiblePriceMultiplier = 1.5f;` | `public const float HighestPossiblePriceMultiplier = 1.5f;` |
| 2509 | field | Terraria.GameContent.ShopHelper | Terraria.GameContent/ShopHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\ShopHelper.cs | 18 | 2 | _currentHappiness | string | `private string _currentHappiness;` | `private string _currentHappiness;` |
| 2510 | field | Terraria.GameContent.ShopHelper | Terraria.GameContent/ShopHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\ShopHelper.cs | 20 | 2 | _currentPriceAdjustment | float | `private float _currentPriceAdjustment;` | `private float _currentPriceAdjustment;` |
| 2511 | field | Terraria.GameContent.ShopHelper | Terraria.GameContent/ShopHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\ShopHelper.cs | 22 | 2 | _currentNPCBeingTalkedTo | Terraria.NPC | `private NPC _currentNPCBeingTalkedTo;` | `private NPC _currentNPCBeingTalkedTo;` |
| 2512 | field | Terraria.GameContent.ShopHelper | Terraria.GameContent/ShopHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\ShopHelper.cs | 24 | 2 | _currentPlayerTalking | Terraria.Player | `private Player _currentPlayerTalking;` | `private Player _currentPlayerTalking;` |
| 2513 | field | Terraria.GameContent.ShopHelper | Terraria.GameContent/ShopHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\ShopHelper.cs | 26 | 2 | _database | Terraria.GameContent.Personalities.PersonalityDatabase | `private PersonalityDatabase _database;` | `private PersonalityDatabase _database;` |
| 2514 | field | Terraria.GameContent.ShopHelper | Terraria.GameContent/ShopHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\ShopHelper.cs | 28 | 2 | _dangerousBiomes | Terraria.GameContent.Personalities.AShoppingBiome[] | `private AShoppingBiome[] _dangerousBiomes = new AShoppingBiome[3]  	{  		new CorruptionBiome(),  		new CrimsonBiome(),  		new DungeonBiome()  	};` | `private AShoppingBiome[] _dangerousBiomes = new AShoppingBiome[3] { new CorruptionBiome(), new CrimsonBiome(), new DungeonBiome() };` |
| 2515 | field | Terraria.GameContent.ShopHelper | Terraria.GameContent/ShopHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\ShopHelper.cs | 35 | 2 | likeValue | float | `private const float likeValue = 0.94f;` | `private const float likeValue = 0.94f;` |
| 2516 | field | Terraria.GameContent.ShopHelper | Terraria.GameContent/ShopHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\ShopHelper.cs | 37 | 2 | dislikeValue | float | `private const float dislikeValue = 1.06f;` | `private const float dislikeValue = 1.06f;` |
| 2517 | field | Terraria.GameContent.ShopHelper | Terraria.GameContent/ShopHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\ShopHelper.cs | 39 | 2 | loveValue | float | `private const float loveValue = 0.88f;` | `private const float loveValue = 0.88f;` |
| 2518 | field | Terraria.GameContent.ShopHelper | Terraria.GameContent/ShopHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\ShopHelper.cs | 41 | 2 | hateValue | float | `private const float hateValue = 1.12f;` | `private const float hateValue = 1.12f;` |
| 3485 | field | Terraria.ShoppingSettings | Terraria/ShoppingSettings.cs | D:\TRbackup\Version4\Terraria\ShoppingSettings.cs | 5 | 2 | PriceAdjustment | float | `public float PriceAdjustment;` | `public float PriceAdjustment;` |
| 3486 | field | Terraria.ShoppingSettings | Terraria/ShoppingSettings.cs | D:\TRbackup\Version4\Terraria\ShoppingSettings.cs | 7 | 2 | HappinessReport | string | `public string HappinessReport;` | `public string HappinessReport;` |

#### 属性（1）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 4023 | property | Terraria.ShoppingSettings | Terraria/ShoppingSettings.cs | D:\TRbackup\Version4\Terraria\ShoppingSettings.cs | 9 | 2 | NotInShop | Terraria.ShoppingSettings | `public static ShoppingSettings NotInShop => new ShoppingSettings { PriceAdjustment = 1f, HappinessReport = "" };` | `public static ShoppingSettings NotInShop => new ShoppingSettings { PriceAdjustment = 1f, HappinessReport = "" };` |


### 4.5 细分子系统：`SharedLootSimulation`

- 原报告章节：`4.9.48`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`10` / `经济、配方、钓鱼与掉落`

- 上一级基线细分子系统：`SharedLootSimulation`
- 细分职责：离线掉落模拟和计数状态。
- 边界角色：`query`；最小 seam：loot simulation view；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：2；声明类型数：2；字段：9；属性：0；合计：9。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（9）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2130 | field | Terraria.GameContent.LootSimulation.LootSimulationItemCounter | Terraria.GameContent.LootSimulation/LootSimulationItemCounter.cs | D:\TRbackup\Version4\Terraria.GameContent.LootSimulation\LootSimulationItemCounter.cs | 9 | 2 | _itemCountsObtained | long[] | `private long[] _itemCountsObtained = new long[ItemID.Count];` | `private long[] _itemCountsObtained = new long[ItemID.Count];` |
| 2131 | field | Terraria.GameContent.LootSimulation.LootSimulationItemCounter | Terraria.GameContent.LootSimulation/LootSimulationItemCounter.cs | D:\TRbackup\Version4\Terraria.GameContent.LootSimulation\LootSimulationItemCounter.cs | 11 | 2 | _itemCountsObtainedExpert | long[] | `private long[] _itemCountsObtainedExpert = new long[ItemID.Count];` | `private long[] _itemCountsObtainedExpert = new long[ItemID.Count];` |
| 2132 | field | Terraria.GameContent.LootSimulation.SimulatorInfo | Terraria.GameContent.LootSimulation/SimulatorInfo.cs | D:\TRbackup\Version4\Terraria.GameContent.LootSimulation\SimulatorInfo.cs | 7 | 2 | player | Terraria.Player | `public Player player;` | `public Player player;` |
| 2133 | field | Terraria.GameContent.LootSimulation.SimulatorInfo | Terraria.GameContent.LootSimulation/SimulatorInfo.cs | D:\TRbackup\Version4\Terraria.GameContent.LootSimulation\SimulatorInfo.cs | 9 | 2 | _originalDayTimeCounter | double | `private double _originalDayTimeCounter;` | `private double _originalDayTimeCounter;` |
| 2134 | field | Terraria.GameContent.LootSimulation.SimulatorInfo | Terraria.GameContent.LootSimulation/SimulatorInfo.cs | D:\TRbackup\Version4\Terraria.GameContent.LootSimulation\SimulatorInfo.cs | 11 | 2 | _originalDayTimeFlag | bool | `private bool _originalDayTimeFlag;` | `private bool _originalDayTimeFlag;` |
| 2135 | field | Terraria.GameContent.LootSimulation.SimulatorInfo | Terraria.GameContent.LootSimulation/SimulatorInfo.cs | D:\TRbackup\Version4\Terraria.GameContent.LootSimulation\SimulatorInfo.cs | 13 | 2 | _originalPlayerPosition | Vector2 | `private Vector2 _originalPlayerPosition;` | `private Vector2 _originalPlayerPosition;` |
| 2136 | field | Terraria.GameContent.LootSimulation.SimulatorInfo | Terraria.GameContent.LootSimulation/SimulatorInfo.cs | D:\TRbackup\Version4\Terraria.GameContent.LootSimulation\SimulatorInfo.cs | 15 | 2 | runningExpertMode | bool | `public bool runningExpertMode;` | `public bool runningExpertMode;` |
| 2137 | field | Terraria.GameContent.LootSimulation.SimulatorInfo | Terraria.GameContent.LootSimulation/SimulatorInfo.cs | D:\TRbackup\Version4\Terraria.GameContent.LootSimulation\SimulatorInfo.cs | 17 | 2 | itemCounter | Terraria.GameContent.LootSimulation.LootSimulationItemCounter | `public LootSimulationItemCounter itemCounter;` | `public LootSimulationItemCounter itemCounter;` |
| 2138 | field | Terraria.GameContent.LootSimulation.SimulatorInfo | Terraria.GameContent.LootSimulation/SimulatorInfo.cs | D:\TRbackup\Version4\Terraria.GameContent.LootSimulation\SimulatorInfo.cs | 19 | 2 | npcVictim | Terraria.NPC | `public NPC npcVictim;` | `public NPC npcVictim;` |

#### 属性（0）

无该类型成员记录。


### 4.6 细分子系统：`SharedDropSourceAttribution`

- 原报告章节：`4.9.49`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`10` / `经济、配方、钓鱼与掉落`

- 上一级基线细分子系统：`SharedDropSourceAttribution`
- 细分职责：掉落和 Boss 生成来源值对象。
- 边界角色：`value object`；最小 seam：drop source attribution；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：3；声明类型数：3；字段：3；属性：0；合计：3。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（3）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1125 | field | Terraria.DataStructures.EntitySource_BossSpawn | Terraria.DataStructures/EntitySource_BossSpawn.cs | D:\TRbackup\Version4\Terraria.DataStructures\EntitySource_BossSpawn.cs | 5 | 2 | Entity | Terraria.IEntitySourceTarget | `public readonly IEntitySourceTarget Entity;` | `public readonly IEntitySourceTarget Entity;` |
| 1129 | field | Terraria.DataStructures.EntitySource_DropAsItem | Terraria.DataStructures/EntitySource_DropAsItem.cs | D:\TRbackup\Version4\Terraria.DataStructures\EntitySource_DropAsItem.cs | 5 | 2 | Entity | Terraria.IEntitySourceTarget | `public readonly IEntitySourceTarget Entity;` | `public readonly IEntitySourceTarget Entity;` |
| 1134 | field | Terraria.DataStructures.EntitySource_Loot | Terraria.DataStructures/EntitySource_Loot.cs | D:\TRbackup\Version4\Terraria.DataStructures\EntitySource_Loot.cs | 5 | 2 | Entity | Terraria.IEntitySourceTarget | `public readonly IEntitySourceTarget Entity;` | `public readonly IEntitySourceTarget Entity;` |

#### 属性（0）

无该类型成员记录。


### 4.7 细分子系统：`CraftingRequestState`

- 原报告章节：`4.9.99`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`10` / `经济、配方、钓鱼与掉落`

- 上一级基线细分子系统：`CraftingRequestState`
- 细分职责：远程制作请求和待处理制作队列状态。
- 边界角色：`state/command`；最小 seam：crafting request port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：2；字段：6；属性：1；合计：7。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（6）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2363 | field | Terraria.GameContent.CraftingRequests.RemoteCraftRequest | Terraria.GameContent/CraftingRequests.cs | D:\TRbackup\Version4\Terraria.GameContent\CraftingRequests.cs | 13 | 3 | recipe | Terraria.Recipe | `public Recipe recipe;` | `public Recipe recipe;` |
| 2364 | field | Terraria.GameContent.CraftingRequests.RemoteCraftRequest | Terraria.GameContent/CraftingRequests.cs | D:\TRbackup\Version4\Terraria.GameContent\CraftingRequests.cs | 15 | 3 | result | Terraria.Item | `public Item result;` | `public Item result;` |
| 2365 | field | Terraria.GameContent.CraftingRequests.RemoteCraftRequest | Terraria.GameContent/CraftingRequests.cs | D:\TRbackup\Version4\Terraria.GameContent\CraftingRequests.cs | 17 | 3 | consumed | System.Collections.Generic.List<Terraria.Item> | `public List<Item> consumed;` | `public List<Item> consumed;` |
| 2366 | field | Terraria.GameContent.CraftingRequests.RemoteCraftRequest | Terraria.GameContent/CraftingRequests.cs | D:\TRbackup\Version4\Terraria.GameContent\CraftingRequests.cs | 19 | 3 | requested | System.Collections.Generic.List<Terraria.Recipe.RequiredItemEntry> | `public List<Recipe.RequiredItemEntry> requested;` | `public List<Recipe.RequiredItemEntry> requested;` |
| 2367 | field | Terraria.GameContent.CraftingRequests.RemoteCraftRequest | Terraria.GameContent/CraftingRequests.cs | D:\TRbackup\Version4\Terraria.GameContent\CraftingRequests.cs | 21 | 3 | quickCraft | bool | `public bool quickCraft;` | `public bool quickCraft;` |
| 2368 | field | Terraria.GameContent.CraftingRequests | Terraria.GameContent/CraftingRequests.cs | D:\TRbackup\Version4\Terraria.GameContent\CraftingRequests.cs | 30 | 2 | _pendingCrafts | System.Collections.Generic.Queue<Terraria.GameContent.CraftingRequests.RemoteCraftRequest> | `private static Queue<RemoteCraftRequest> _pendingCrafts = new Queue<RemoteCraftRequest>();` | `private static Queue<RemoteCraftRequest> _pendingCrafts = new Queue<RemoteCraftRequest>();` |

#### 属性（1）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3850 | property | Terraria.GameContent.CraftingRequests | Terraria.GameContent/CraftingRequests.cs | D:\TRbackup\Version4\Terraria.GameContent\CraftingRequests.cs | 32 | 2 | HasPendingRequests | bool | `public static bool HasPendingRequests => _pendingCrafts.Count > 0;` | `public static bool HasPendingRequests => _pendingCrafts.Count > 0;` |


### 4.8 细分子系统：`SharedDropRuleResolutionAndCatalogState`

- 原报告章节：`4.9.114`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`10` / `经济、配方、钓鱼与掉落`

- 上一级基线细分子系统：`SharedDropRuleDefinitions`
- 细分职责：掉落尝试、概率结果、数据库和 resolver 解析边界。
- 边界角色：`query/adapter`；最小 seam：drop rule resolution port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：8；声明类型数：8；字段：19；属性：2；合计：21。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（19）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2049 | field | Terraria.GameContent.ItemDropRules.DropAttemptInfo | Terraria.GameContent.ItemDropRules/DropAttemptInfo.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\DropAttemptInfo.cs | 7 | 2 | npc | Terraria.NPC | `public NPC npc;` | `public NPC npc;` |
| 2050 | field | Terraria.GameContent.ItemDropRules.DropAttemptInfo | Terraria.GameContent.ItemDropRules/DropAttemptInfo.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\DropAttemptInfo.cs | 9 | 2 | player | Terraria.Player | `public Player player;` | `public Player player;` |
| 2051 | field | Terraria.GameContent.ItemDropRules.DropAttemptInfo | Terraria.GameContent.ItemDropRules/DropAttemptInfo.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\DropAttemptInfo.cs | 11 | 2 | rng | Terraria.Utilities.UnifiedRandom | `public UnifiedRandom rng;` | `public UnifiedRandom rng;` |
| 2052 | field | Terraria.GameContent.ItemDropRules.DropAttemptInfo | Terraria.GameContent.ItemDropRules/DropAttemptInfo.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\DropAttemptInfo.cs | 13 | 2 | IsInSimulation | bool | `public bool IsInSimulation;` | `public bool IsInSimulation;` |
| 2053 | field | Terraria.GameContent.ItemDropRules.DropAttemptInfo | Terraria.GameContent.ItemDropRules/DropAttemptInfo.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\DropAttemptInfo.cs | 15 | 2 | IsExpertMode | bool | `public bool IsExpertMode;` | `public bool IsExpertMode;` |
| 2054 | field | Terraria.GameContent.ItemDropRules.DropAttemptInfo | Terraria.GameContent.ItemDropRules/DropAttemptInfo.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\DropAttemptInfo.cs | 17 | 2 | IsMasterMode | bool | `public bool IsMasterMode;` | `public bool IsMasterMode;` |
| 2076 | field | Terraria.GameContent.ItemDropRules.DropRateInfo | Terraria.GameContent.ItemDropRules/DropRateInfo.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\DropRateInfo.cs | 7 | 2 | itemId | int | `public int itemId;` | `public int itemId;` |
| 2077 | field | Terraria.GameContent.ItemDropRules.DropRateInfo | Terraria.GameContent.ItemDropRules/DropRateInfo.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\DropRateInfo.cs | 9 | 2 | stackMin | int | `public int stackMin;` | `public int stackMin;` |
| 2078 | field | Terraria.GameContent.ItemDropRules.DropRateInfo | Terraria.GameContent.ItemDropRules/DropRateInfo.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\DropRateInfo.cs | 11 | 2 | stackMax | int | `public int stackMax;` | `public int stackMax;` |
| 2079 | field | Terraria.GameContent.ItemDropRules.DropRateInfo | Terraria.GameContent.ItemDropRules/DropRateInfo.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\DropRateInfo.cs | 13 | 2 | dropRate | float | `public float dropRate;` | `public float dropRate;` |
| 2080 | field | Terraria.GameContent.ItemDropRules.DropRateInfo | Terraria.GameContent.ItemDropRules/DropRateInfo.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\DropRateInfo.cs | 15 | 2 | conditions | System.Collections.Generic.List<Terraria.GameContent.ItemDropRules.IItemDropRuleCondition> | `public List<IItemDropRuleCondition> conditions;` | `public List<IItemDropRuleCondition> conditions;` |
| 2081 | field | Terraria.GameContent.ItemDropRules.DropRateInfoChainFeed | Terraria.GameContent.ItemDropRules/DropRateInfoChainFeed.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\DropRateInfoChainFeed.cs | 7 | 2 | parentDroprateChance | float | `public float parentDroprateChance;` | `public float parentDroprateChance;` |
| 2082 | field | Terraria.GameContent.ItemDropRules.DropRateInfoChainFeed | Terraria.GameContent.ItemDropRules/DropRateInfoChainFeed.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\DropRateInfoChainFeed.cs | 9 | 2 | conditions | System.Collections.Generic.List<Terraria.GameContent.ItemDropRules.IItemDropRuleCondition> | `public List<IItemDropRuleCondition> conditions;` | `public List<IItemDropRuleCondition> conditions;` |
| 2086 | field | Terraria.GameContent.ItemDropRules.ItemDropAttemptResult | Terraria.GameContent.ItemDropRules/ItemDropAttemptResult.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\ItemDropAttemptResult.cs | 5 | 2 | State | Terraria.GameContent.ItemDropRules.ItemDropAttemptResultState | `public ItemDropAttemptResultState State;` | `public ItemDropAttemptResultState State;` |
| 2087 | field | Terraria.GameContent.ItemDropRules.ItemDropDatabase | Terraria.GameContent.ItemDropRules/ItemDropDatabase.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\ItemDropDatabase.cs | 9 | 2 | _globalEntries | System.Collections.Generic.List<Terraria.GameContent.ItemDropRules.IItemDropRule> | `private List<IItemDropRule> _globalEntries = new List<IItemDropRule>();` | `private List<IItemDropRule> _globalEntries = new List<IItemDropRule>();` |
| 2088 | field | Terraria.GameContent.ItemDropRules.ItemDropDatabase | Terraria.GameContent.ItemDropRules/ItemDropDatabase.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\ItemDropDatabase.cs | 11 | 2 | _entriesByNpcNetId | System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<Terraria.GameContent.ItemDropRules.IItemDropRule>> | `private Dictionary<int, List<IItemDropRule>> _entriesByNpcNetId = new Dictionary<int, List<IItemDropRule>>();` | `private Dictionary<int, List<IItemDropRule>> _entriesByNpcNetId = new Dictionary<int, List<IItemDropRule>>();` |
| 2089 | field | Terraria.GameContent.ItemDropRules.ItemDropDatabase | Terraria.GameContent.ItemDropRules/ItemDropDatabase.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\ItemDropDatabase.cs | 13 | 2 | _npcNetIdsByType | System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<int>> | `private Dictionary<int, List<int>> _npcNetIdsByType = new Dictionary<int, List<int>>();` | `private Dictionary<int, List<int>> _npcNetIdsByType = new Dictionary<int, List<int>>();` |
| 2090 | field | Terraria.GameContent.ItemDropRules.ItemDropDatabase | Terraria.GameContent.ItemDropRules/ItemDropDatabase.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\ItemDropDatabase.cs | 15 | 2 | _masterModeDropRng | int | `private int _masterModeDropRng = 4;` | `private int _masterModeDropRng = 4;` |
| 2091 | field | Terraria.GameContent.ItemDropRules.ItemDropResolver | Terraria.GameContent.ItemDropRules/ItemDropResolver.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\ItemDropResolver.cs | 7 | 2 | _database | Terraria.GameContent.ItemDropRules.ItemDropDatabase | `private ItemDropDatabase _database;` | `private ItemDropDatabase _database;` |

#### 属性（2）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3812 | property | Terraria.GameContent.ItemDropRules.IItemDropRule | Terraria.GameContent.ItemDropRules/IItemDropRule.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\IItemDropRule.cs | 7 | 2 | ChainedRules | System.Collections.Generic.List<Terraria.GameContent.ItemDropRules.IItemDropRuleChainAttempt> | `List<IItemDropRuleChainAttempt> ChainedRules { get; }` | `List<IItemDropRuleChainAttempt> ChainedRules { get; }` |
| 3813 | property | Terraria.GameContent.ItemDropRules.IItemDropRuleChainAttempt | Terraria.GameContent.ItemDropRules/IItemDropRuleChainAttempt.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\IItemDropRuleChainAttempt.cs | 7 | 2 | RuleToChain | Terraria.GameContent.ItemDropRules.IItemDropRule | `IItemDropRule RuleToChain { get; }` | `IItemDropRule RuleToChain { get; }` |


### 4.9 细分子系统：`SharedFishingConditionContextState`

- 原报告章节：`4.9.115`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`10` / `经济、配方、钓鱼与掉落`

- 上一级基线细分子系统：`SharedFishingDropRuleDefinitions`
- 细分职责：钓鱼上下文、条件对象和任务鱼筛选状态。
- 边界角色：`query/state`；最小 seam：fishing condition context port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：3；声明类型数：4；字段：13；属性：0；合计：13。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（13）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1652 | field | Terraria.GameContent.FishDropRules.AFishingCondition | Terraria.GameContent.FishDropRules/AFishingCondition.cs | D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\AFishingCondition.cs | 5 | 2 | CanBeSkippedForDisplay | bool | `public bool CanBeSkippedForDisplay;` | `public bool CanBeSkippedForDisplay;` |
| 1659 | field | Terraria.GameContent.FishDropRules.FishingConditions.QuestFishCondition | Terraria.GameContent.FishDropRules/FishingConditions.cs | D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\FishingConditions.cs | 7 | 3 | CheckedType | int | `public int CheckedType;` | `public int CheckedType;` |
| 1660 | field | Terraria.GameContent.FishDropRules.FishingConditions.QuestFishConditionRemix | Terraria.GameContent.FishDropRules/FishingConditions.cs | D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\FishingConditions.cs | 15 | 3 | CheckedType | int | `public int CheckedType;` | `public int CheckedType;` |
| 1661 | field | Terraria.GameContent.FishDropRules.FishingContext | Terraria.GameContent.FishDropRules/FishingContext.cs | D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\FishingContext.cs | 8 | 2 | Random | Terraria.Utilities.UnifiedRandom | `public UnifiedRandom Random = new UnifiedRandom();` | `public UnifiedRandom Random = new UnifiedRandom();` |
| 1662 | field | Terraria.GameContent.FishDropRules.FishingContext | Terraria.GameContent.FishDropRules/FishingContext.cs | D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\FishingContext.cs | 10 | 2 | Fisher | Terraria.DataStructures.FishingAttempt | `public FishingAttempt Fisher;` | `public FishingAttempt Fisher;` |
| 1663 | field | Terraria.GameContent.FishDropRules.FishingContext | Terraria.GameContent.FishDropRules/FishingContext.cs | D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\FishingContext.cs | 12 | 2 | Player | Terraria.Player | `public Player Player;` | `public Player Player;` |
| 1664 | field | Terraria.GameContent.FishDropRules.FishingContext | Terraria.GameContent.FishDropRules/FishingContext.cs | D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\FishingContext.cs | 14 | 2 | RolledCorruption | bool | `public bool RolledCorruption;` | `public bool RolledCorruption;` |
| 1665 | field | Terraria.GameContent.FishDropRules.FishingContext | Terraria.GameContent.FishDropRules/FishingContext.cs | D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\FishingContext.cs | 16 | 2 | RolledCrimson | bool | `public bool RolledCrimson;` | `public bool RolledCrimson;` |
| 1666 | field | Terraria.GameContent.FishDropRules.FishingContext | Terraria.GameContent.FishDropRules/FishingContext.cs | D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\FishingContext.cs | 18 | 2 | RolledJungle | bool | `public bool RolledJungle;` | `public bool RolledJungle;` |
| 1667 | field | Terraria.GameContent.FishDropRules.FishingContext | Terraria.GameContent.FishDropRules/FishingContext.cs | D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\FishingContext.cs | 20 | 2 | RolledSnow | bool | `public bool RolledSnow;` | `public bool RolledSnow;` |
| 1668 | field | Terraria.GameContent.FishDropRules.FishingContext | Terraria.GameContent.FishDropRules/FishingContext.cs | D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\FishingContext.cs | 22 | 2 | RolledDesert | bool | `public bool RolledDesert;` | `public bool RolledDesert;` |
| 1669 | field | Terraria.GameContent.FishDropRules.FishingContext | Terraria.GameContent.FishDropRules/FishingContext.cs | D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\FishingContext.cs | 24 | 2 | RolledInfectedDesert | bool | `public bool RolledInfectedDesert;` | `public bool RolledInfectedDesert;` |
| 1670 | field | Terraria.GameContent.FishDropRules.FishingContext | Terraria.GameContent.FishDropRules/FishingContext.cs | D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\FishingContext.cs | 26 | 2 | RolledRemixOcean | bool | `public bool RolledRemixOcean;` | `public bool RolledRemixOcean;` |

#### 属性（0）

无该类型成员记录。


### 4.10 细分子系统：`RecipeDefinitionState`

- 原报告章节：`4.9.144`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`10` / `经济、配方、钓鱼与掉落`

- 上一级基线细分子系统：`RecipeDefinitionCatalog`
- 细分职责：配方输入、条件、制作材料和环境要求。
- 边界角色：`definition/catalog`；最小 seam：recipe definition port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：2；字段：23；属性：2；合计：25。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（23）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3398 | field | Terraria.Recipe.RequiredItemEntry | Terraria/Recipe.cs | D:\TRbackup\Version4\Terraria\Recipe.cs | 16 | 3 | itemIdOrRecipeGroup | int | `public int itemIdOrRecipeGroup;` | `public int itemIdOrRecipeGroup;` |
| 3399 | field | Terraria.Recipe.RequiredItemEntry | Terraria/Recipe.cs | D:\TRbackup\Version4\Terraria\Recipe.cs | 18 | 3 | stack | int | `public int stack;` | `public int stack;` |
| 3400 | field | Terraria.Recipe | Terraria/Recipe.cs | D:\TRbackup\Version4\Terraria\Recipe.cs | 45 | 2 | maxRequirements | int | `public static int maxRequirements = 15;` | `public static int maxRequirements = 15;` |
| 3401 | field | Terraria.Recipe | Terraria/Recipe.cs | D:\TRbackup\Version4\Terraria\Recipe.cs | 47 | 2 | currentRecipe | Terraria.Recipe | `private static Recipe currentRecipe = new Recipe();` | `private static Recipe currentRecipe = new Recipe();` |
| 3402 | field | Terraria.Recipe | Terraria/Recipe.cs | D:\TRbackup\Version4\Terraria\Recipe.cs | 49 | 2 | createItem | Terraria.Item | `public Item createItem = new Item();` | `public Item createItem = new Item();` |
| 3403 | field | Terraria.Recipe | Terraria/Recipe.cs | D:\TRbackup\Version4\Terraria\Recipe.cs | 51 | 2 | requiredItem | Terraria.Item[] | `public Item[] requiredItem = new Item[maxRequirements];` | `public Item[] requiredItem = new Item[maxRequirements];` |
| 3404 | field | Terraria.Recipe | Terraria/Recipe.cs | D:\TRbackup\Version4\Terraria\Recipe.cs | 53 | 2 | requiredTile | int | `public int requiredTile = -1;` | `public int requiredTile = -1;` |
| 3405 | field | Terraria.Recipe | Terraria/Recipe.cs | D:\TRbackup\Version4\Terraria\Recipe.cs | 55 | 2 | acceptedGroups | int[] | `public int[] acceptedGroups = new int[maxRequirements];` | `public int[] acceptedGroups = new int[maxRequirements];` |
| 3406 | field | Terraria.Recipe | Terraria/Recipe.cs | D:\TRbackup\Version4\Terraria\Recipe.cs | 57 | 2 | requiredItemQuickLookup | Terraria.Recipe.RequiredItemEntry[] | `public RequiredItemEntry[] requiredItemQuickLookup = new RequiredItemEntry[maxRequirements];` | `public RequiredItemEntry[] requiredItemQuickLookup = new RequiredItemEntry[maxRequirements];` |
| 3407 | field | Terraria.Recipe | Terraria/Recipe.cs | D:\TRbackup\Version4\Terraria\Recipe.cs | 59 | 2 | customShimmerResults | System.Collections.Generic.List<Terraria.Item> | `public List<Item> customShimmerResults;` | `public List<Item> customShimmerResults;` |
| 3408 | field | Terraria.Recipe | Terraria/Recipe.cs | D:\TRbackup\Version4\Terraria\Recipe.cs | 61 | 2 | needHoney | bool | `public bool needHoney;` | `public bool needHoney;` |
| 3409 | field | Terraria.Recipe | Terraria/Recipe.cs | D:\TRbackup\Version4\Terraria\Recipe.cs | 63 | 2 | needWater | bool | `public bool needWater;` | `public bool needWater;` |
| 3410 | field | Terraria.Recipe | Terraria/Recipe.cs | D:\TRbackup\Version4\Terraria\Recipe.cs | 65 | 2 | needLava | bool | `public bool needLava;` | `public bool needLava;` |
| 3411 | field | Terraria.Recipe | Terraria/Recipe.cs | D:\TRbackup\Version4\Terraria\Recipe.cs | 67 | 2 | needTorchGodsFavor | bool | `public bool needTorchGodsFavor;` | `public bool needTorchGodsFavor;` |
| 3412 | field | Terraria.Recipe | Terraria/Recipe.cs | D:\TRbackup\Version4\Terraria\Recipe.cs | 69 | 2 | alchemy | bool | `public bool alchemy;` | `public bool alchemy;` |
| 3413 | field | Terraria.Recipe | Terraria/Recipe.cs | D:\TRbackup\Version4\Terraria\Recipe.cs | 71 | 2 | needSnowBiome | bool | `public bool needSnowBiome;` | `public bool needSnowBiome;` |
| 3414 | field | Terraria.Recipe | Terraria/Recipe.cs | D:\TRbackup\Version4\Terraria\Recipe.cs | 73 | 2 | needGraveyardBiome | bool | `public bool needGraveyardBiome;` | `public bool needGraveyardBiome;` |
| 3415 | field | Terraria.Recipe | Terraria/Recipe.cs | D:\TRbackup\Version4\Terraria\Recipe.cs | 75 | 2 | needMechdusa | bool | `public bool needMechdusa;` | `public bool needMechdusa;` |
| 3416 | field | Terraria.Recipe | Terraria/Recipe.cs | D:\TRbackup\Version4\Terraria\Recipe.cs | 77 | 2 | notDecraftable | bool | `public bool notDecraftable;` | `public bool notDecraftable;` |
| 3417 | field | Terraria.Recipe | Terraria/Recipe.cs | D:\TRbackup\Version4\Terraria\Recipe.cs | 79 | 2 | crimson | bool | `public bool crimson;` | `public bool crimson;` |
| 3418 | field | Terraria.Recipe | Terraria/Recipe.cs | D:\TRbackup\Version4\Terraria\Recipe.cs | 81 | 2 | corruption | bool | `public bool corruption;` | `public bool corruption;` |
| 3419 | field | Terraria.Recipe | Terraria/Recipe.cs | D:\TRbackup\Version4\Terraria\Recipe.cs | 83 | 2 | _ownedItems | System.Collections.Generic.Dictionary<int, int> | `private static Dictionary<int, int> _ownedItems = new Dictionary<int, int>();` | `private static Dictionary<int, int> _ownedItems = new Dictionary<int, int>();` |
| 3420 | field | Terraria.Recipe | Terraria/Recipe.cs | D:\TRbackup\Version4\Terraria\Recipe.cs | 85 | 2 | _recipeChests | System.Collections.Generic.List<Terraria.Chest> | `internal static List<Chest> _recipeChests = new List<Chest>();` | `internal static List<Chest> _recipeChests = new List<Chest>();` |

#### 属性（2）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3976 | property | Terraria.Recipe.RequiredItemEntry | Terraria/Recipe.cs | D:\TRbackup\Version4\Terraria\Recipe.cs | 20 | 3 | IsRecipeGroup | bool | `public bool IsRecipeGroup => itemIdOrRecipeGroup >= RecipeGroup.FakeItemIdOffset;` | `public bool IsRecipeGroup => itemIdOrRecipeGroup >= RecipeGroup.FakeItemIdOffset;` |
| 3977 | property | Terraria.Recipe.RequiredItemEntry | Terraria/Recipe.cs | D:\TRbackup\Version4\Terraria\Recipe.cs | 22 | 3 | RecipeGroup | Terraria.RecipeGroup | `public RecipeGroup RecipeGroup => RecipeGroup.recipeGroups[itemIdOrRecipeGroup - RecipeGroup.FakeItemIdOffset];` | `public RecipeGroup RecipeGroup => RecipeGroup.recipeGroups[itemIdOrRecipeGroup - RecipeGroup.FakeItemIdOffset];` |


### 4.11 细分子系统：`RecipeGroupCatalogState`

- 原报告章节：`4.9.145`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`10` / `经济、配方、钓鱼与掉落`

- 上一级基线细分子系统：`RecipeDefinitionCatalog`
- 细分职责：配方组集合、文本格式和注册 ID 目录。
- 边界角色：`definition/catalog`；最小 seam：recipe group catalog port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：7；属性：1；合计：8。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（7）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3421 | field | Terraria.RecipeGroup | Terraria/RecipeGroup.cs | D:\TRbackup\Version4\Terraria\RecipeGroup.cs | 11 | 2 | FakeItemIdOffset | int | `public static readonly int FakeItemIdOffset = 1000000;` | `public static readonly int FakeItemIdOffset = 1000000;` |
| 3422 | field | Terraria.RecipeGroup | Terraria/RecipeGroup.cs | D:\TRbackup\Version4\Terraria\RecipeGroup.cs | 13 | 2 | DefaultCombineFormat | Terraria.Localization.LocalizedText | `public static LocalizedText DefaultCombineFormat = Language.GetText("CombineFormat.RecipeGroup");` | `public static LocalizedText DefaultCombineFormat = Language.GetText("CombineFormat.RecipeGroup");` |
| 3423 | field | Terraria.RecipeGroup | Terraria/RecipeGroup.cs | D:\TRbackup\Version4\Terraria\RecipeGroup.cs | 15 | 2 | GetText | System.Func<string> | `public Func<string> GetText;` | `public Func<string> GetText;` |
| 3424 | field | Terraria.RecipeGroup | Terraria/RecipeGroup.cs | D:\TRbackup\Version4\Terraria\RecipeGroup.cs | 17 | 2 | ValidItems | System.Collections.Generic.HashSet<int> | `public HashSet<int> ValidItems = new HashSet<int>();` | `public HashSet<int> ValidItems = new HashSet<int>();` |
| 3425 | field | Terraria.RecipeGroup | Terraria/RecipeGroup.cs | D:\TRbackup\Version4\Terraria\RecipeGroup.cs | 19 | 2 | Items | System.Collections.Generic.List<int> | `public List<int> Items = new List<int>();` | `public List<int> Items = new List<int>();` |
| 3426 | field | Terraria.RecipeGroup | Terraria/RecipeGroup.cs | D:\TRbackup\Version4\Terraria\RecipeGroup.cs | 21 | 2 | DecraftItemId | int | `public int DecraftItemId;` | `public int DecraftItemId;` |
| 3427 | field | Terraria.RecipeGroup | Terraria/RecipeGroup.cs | D:\TRbackup\Version4\Terraria\RecipeGroup.cs | 23 | 2 | recipeGroups | System.Collections.Generic.Dictionary<int, Terraria.RecipeGroup> | `public static Dictionary<int, RecipeGroup> recipeGroups = new Dictionary<int, RecipeGroup>();` | `public static Dictionary<int, RecipeGroup> recipeGroups = new Dictionary<int, RecipeGroup>();` |

#### 属性（1）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3978 | property | Terraria.RecipeGroup | Terraria/RecipeGroup.cs | D:\TRbackup\Version4\Terraria\RecipeGroup.cs | 25 | 2 | RegisteredId | int | `public int RegisteredId { get; private set; }` | `public int RegisteredId { get; private set; }` |


### 4.12 细分子系统：`FishingAttemptState`

- 原报告章节：`4.9.150`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`10` / `经济、配方、钓鱼与掉落`

- 上一级基线细分子系统：`SharedFishingAttemptAndConditions`
- 细分职责：一次钓鱼尝试的地点、概率、环境和结果状态。
- 边界角色：`state/query`；最小 seam：fishing attempt port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：24；属性：0；合计：24。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（24）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1141 | field | Terraria.DataStructures.FishingAttempt | Terraria.DataStructures/FishingAttempt.cs | D:\TRbackup\Version4\Terraria.DataStructures\FishingAttempt.cs | 5 | 2 | playerFishingConditions | Terraria.DataStructures.PlayerFishingConditions | `public PlayerFishingConditions playerFishingConditions;` | `public PlayerFishingConditions playerFishingConditions;` |
| 1142 | field | Terraria.DataStructures.FishingAttempt | Terraria.DataStructures/FishingAttempt.cs | D:\TRbackup\Version4\Terraria.DataStructures\FishingAttempt.cs | 7 | 2 | X | int | `public int X;` | `public int X;` |
| 1143 | field | Terraria.DataStructures.FishingAttempt | Terraria.DataStructures/FishingAttempt.cs | D:\TRbackup\Version4\Terraria.DataStructures\FishingAttempt.cs | 9 | 2 | Y | int | `public int Y;` | `public int Y;` |
| 1144 | field | Terraria.DataStructures.FishingAttempt | Terraria.DataStructures/FishingAttempt.cs | D:\TRbackup\Version4\Terraria.DataStructures\FishingAttempt.cs | 11 | 2 | bobberType | int | `public int bobberType;` | `public int bobberType;` |
| 1145 | field | Terraria.DataStructures.FishingAttempt | Terraria.DataStructures/FishingAttempt.cs | D:\TRbackup\Version4\Terraria.DataStructures\FishingAttempt.cs | 13 | 2 | common | bool | `public bool common;` | `public bool common;` |
| 1146 | field | Terraria.DataStructures.FishingAttempt | Terraria.DataStructures/FishingAttempt.cs | D:\TRbackup\Version4\Terraria.DataStructures\FishingAttempt.cs | 15 | 2 | uncommon | bool | `public bool uncommon;` | `public bool uncommon;` |
| 1147 | field | Terraria.DataStructures.FishingAttempt | Terraria.DataStructures/FishingAttempt.cs | D:\TRbackup\Version4\Terraria.DataStructures\FishingAttempt.cs | 17 | 2 | rare | bool | `public bool rare;` | `public bool rare;` |
| 1148 | field | Terraria.DataStructures.FishingAttempt | Terraria.DataStructures/FishingAttempt.cs | D:\TRbackup\Version4\Terraria.DataStructures\FishingAttempt.cs | 19 | 2 | veryrare | bool | `public bool veryrare;` | `public bool veryrare;` |
| 1149 | field | Terraria.DataStructures.FishingAttempt | Terraria.DataStructures/FishingAttempt.cs | D:\TRbackup\Version4\Terraria.DataStructures\FishingAttempt.cs | 21 | 2 | legendary | bool | `public bool legendary;` | `public bool legendary;` |
| 1150 | field | Terraria.DataStructures.FishingAttempt | Terraria.DataStructures/FishingAttempt.cs | D:\TRbackup\Version4\Terraria.DataStructures\FishingAttempt.cs | 23 | 2 | crate | bool | `public bool crate;` | `public bool crate;` |
| 1151 | field | Terraria.DataStructures.FishingAttempt | Terraria.DataStructures/FishingAttempt.cs | D:\TRbackup\Version4\Terraria.DataStructures\FishingAttempt.cs | 25 | 2 | junk | bool | `public bool junk;` | `public bool junk;` |
| 1152 | field | Terraria.DataStructures.FishingAttempt | Terraria.DataStructures/FishingAttempt.cs | D:\TRbackup\Version4\Terraria.DataStructures\FishingAttempt.cs | 27 | 2 | inLava | bool | `public bool inLava;` | `public bool inLava;` |
| 1153 | field | Terraria.DataStructures.FishingAttempt | Terraria.DataStructures/FishingAttempt.cs | D:\TRbackup\Version4\Terraria.DataStructures\FishingAttempt.cs | 29 | 2 | inHoney | bool | `public bool inHoney;` | `public bool inHoney;` |
| 1154 | field | Terraria.DataStructures.FishingAttempt | Terraria.DataStructures/FishingAttempt.cs | D:\TRbackup\Version4\Terraria.DataStructures\FishingAttempt.cs | 31 | 2 | waterTilesCount | int | `public int waterTilesCount;` | `public int waterTilesCount;` |
| 1155 | field | Terraria.DataStructures.FishingAttempt | Terraria.DataStructures/FishingAttempt.cs | D:\TRbackup\Version4\Terraria.DataStructures\FishingAttempt.cs | 33 | 2 | waterNeededToFish | int | `public int waterNeededToFish;` | `public int waterNeededToFish;` |
| 1156 | field | Terraria.DataStructures.FishingAttempt | Terraria.DataStructures/FishingAttempt.cs | D:\TRbackup\Version4\Terraria.DataStructures\FishingAttempt.cs | 35 | 2 | waterQuality | float | `public float waterQuality;` | `public float waterQuality;` |
| 1157 | field | Terraria.DataStructures.FishingAttempt | Terraria.DataStructures/FishingAttempt.cs | D:\TRbackup\Version4\Terraria.DataStructures\FishingAttempt.cs | 37 | 2 | chumsInWater | int | `public int chumsInWater;` | `public int chumsInWater;` |
| 1158 | field | Terraria.DataStructures.FishingAttempt | Terraria.DataStructures/FishingAttempt.cs | D:\TRbackup\Version4\Terraria.DataStructures\FishingAttempt.cs | 39 | 2 | fishingLevel | int | `public int fishingLevel;` | `public int fishingLevel;` |
| 1159 | field | Terraria.DataStructures.FishingAttempt | Terraria.DataStructures/FishingAttempt.cs | D:\TRbackup\Version4\Terraria.DataStructures\FishingAttempt.cs | 41 | 2 | CanFishInLava | bool | `public bool CanFishInLava;` | `public bool CanFishInLava;` |
| 1160 | field | Terraria.DataStructures.FishingAttempt | Terraria.DataStructures/FishingAttempt.cs | D:\TRbackup\Version4\Terraria.DataStructures\FishingAttempt.cs | 43 | 2 | atmo | float | `public float atmo;` | `public float atmo;` |
| 1161 | field | Terraria.DataStructures.FishingAttempt | Terraria.DataStructures/FishingAttempt.cs | D:\TRbackup\Version4\Terraria.DataStructures\FishingAttempt.cs | 45 | 2 | questFish | int | `public int questFish;` | `public int questFish;` |
| 1162 | field | Terraria.DataStructures.FishingAttempt | Terraria.DataStructures/FishingAttempt.cs | D:\TRbackup\Version4\Terraria.DataStructures\FishingAttempt.cs | 47 | 2 | heightLevel | int | `public int heightLevel;` | `public int heightLevel;` |
| 1163 | field | Terraria.DataStructures.FishingAttempt | Terraria.DataStructures/FishingAttempt.cs | D:\TRbackup\Version4\Terraria.DataStructures\FishingAttempt.cs | 49 | 2 | rolledItemDrop | int | `public int rolledItemDrop;` | `public int rolledItemDrop;` |
| 1164 | field | Terraria.DataStructures.FishingAttempt | Terraria.DataStructures/FishingAttempt.cs | D:\TRbackup\Version4\Terraria.DataStructures\FishingAttempt.cs | 51 | 2 | rolledEnemySpawn | int | `public int rolledEnemySpawn;` | `public int rolledEnemySpawn;` |

#### 属性（0）

无该类型成员记录。


### 4.13 细分子系统：`PlayerFishingConditionState`

- 原报告章节：`4.9.151`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`10` / `经济、配方、钓鱼与掉落`

- 上一级基线细分子系统：`SharedFishingAttemptAndConditions`
- 细分职责：玩家鱼竿、鱼饵和最终钓鱼等级条件。
- 边界角色：`query/state`；最小 seam：player fishing condition port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：6；属性：0；合计：6。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（6）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1224 | field | Terraria.DataStructures.PlayerFishingConditions | Terraria.DataStructures/PlayerFishingConditions.cs | D:\TRbackup\Version4\Terraria.DataStructures\PlayerFishingConditions.cs | 5 | 2 | PolePower | int | `public int PolePower;` | `public int PolePower;` |
| 1225 | field | Terraria.DataStructures.PlayerFishingConditions | Terraria.DataStructures/PlayerFishingConditions.cs | D:\TRbackup\Version4\Terraria.DataStructures\PlayerFishingConditions.cs | 7 | 2 | PoleItemType | int | `public int PoleItemType;` | `public int PoleItemType;` |
| 1226 | field | Terraria.DataStructures.PlayerFishingConditions | Terraria.DataStructures/PlayerFishingConditions.cs | D:\TRbackup\Version4\Terraria.DataStructures\PlayerFishingConditions.cs | 9 | 2 | BaitPower | int | `public int BaitPower;` | `public int BaitPower;` |
| 1227 | field | Terraria.DataStructures.PlayerFishingConditions | Terraria.DataStructures/PlayerFishingConditions.cs | D:\TRbackup\Version4\Terraria.DataStructures\PlayerFishingConditions.cs | 11 | 2 | BaitItemType | int | `public int BaitItemType;` | `public int BaitItemType;` |
| 1228 | field | Terraria.DataStructures.PlayerFishingConditions | Terraria.DataStructures/PlayerFishingConditions.cs | D:\TRbackup\Version4\Terraria.DataStructures\PlayerFishingConditions.cs | 13 | 2 | LevelMultipliers | float | `public float LevelMultipliers;` | `public float LevelMultipliers;` |
| 1229 | field | Terraria.DataStructures.PlayerFishingConditions | Terraria.DataStructures/PlayerFishingConditions.cs | D:\TRbackup\Version4\Terraria.DataStructures\PlayerFishingConditions.cs | 15 | 2 | FinalFishingLevel | int | `public int FinalFishingLevel;` | `public int FinalFishingLevel;` |

#### 属性（0）

无该类型成员记录。


### 4.14 细分子系统：`SharedDropRuleChainState`

- 原报告章节：`4.9.168`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`10` / `经济、配方、钓鱼与掉落`

- 上一级基线细分子系统：`SharedDropRuleDefinitions`
- 上一级 peer 细分子系统：`SharedDropRuleSelectionAndChainState`
- 细分职责：掉落规则链、链式结果和链隐藏策略。
- 边界角色：`definition/query`；最小 seam：drop rule chain port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：16；声明类型数：18；字段：3；属性：18；合计：21。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（3）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2036 | field | Terraria.GameContent.ItemDropRules.Chains.TryIfFailedRandomRoll | Terraria.GameContent.ItemDropRules/Chains.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\Chains.cs | 9 | 3 | hideLootReport | bool | `public bool hideLootReport;` | `public bool hideLootReport;` |
| 2037 | field | Terraria.GameContent.ItemDropRules.Chains.TryIfSucceeded | Terraria.GameContent.ItemDropRules/Chains.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\Chains.cs | 39 | 3 | hideLootReport | bool | `public bool hideLootReport;` | `public bool hideLootReport;` |
| 2038 | field | Terraria.GameContent.ItemDropRules.Chains.TryIfDoesntFillConditions | Terraria.GameContent.ItemDropRules/Chains.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\Chains.cs | 69 | 3 | hideLootReport | bool | `public bool hideLootReport;` | `public bool hideLootReport;` |

#### 属性（18）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3801 | property | Terraria.GameContent.ItemDropRules.Chains.TryIfFailedRandomRoll | Terraria.GameContent.ItemDropRules/Chains.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\Chains.cs | 11 | 3 | RuleToChain | Terraria.GameContent.ItemDropRules.IItemDropRule | `public IItemDropRule RuleToChain { get; private set; }` | `public IItemDropRule RuleToChain { get; private set; }` |
| 3802 | property | Terraria.GameContent.ItemDropRules.Chains.TryIfSucceeded | Terraria.GameContent.ItemDropRules/Chains.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\Chains.cs | 41 | 3 | RuleToChain | Terraria.GameContent.ItemDropRules.IItemDropRule | `public IItemDropRule RuleToChain { get; private set; }` | `public IItemDropRule RuleToChain { get; private set; }` |
| 3803 | property | Terraria.GameContent.ItemDropRules.Chains.TryIfDoesntFillConditions | Terraria.GameContent.ItemDropRules/Chains.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\Chains.cs | 71 | 3 | RuleToChain | Terraria.GameContent.ItemDropRules.IItemDropRule | `public IItemDropRule RuleToChain { get; private set; }` | `public IItemDropRule RuleToChain { get; private set; }` |
| 3804 | property | Terraria.GameContent.ItemDropRules.CommonDrop | Terraria.GameContent.ItemDropRules/CommonDrop.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\CommonDrop.cs | 17 | 2 | ChainedRules | System.Collections.Generic.List<Terraria.GameContent.ItemDropRules.IItemDropRuleChainAttempt> | `public List<IItemDropRuleChainAttempt> ChainedRules { get; private set; }` | `public List<IItemDropRuleChainAttempt> ChainedRules { get; private set; }` |
| 3805 | property | Terraria.GameContent.ItemDropRules.DropBasedOnExpertMode | Terraria.GameContent.ItemDropRules/DropBasedOnExpertMode.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\DropBasedOnExpertMode.cs | 11 | 2 | ChainedRules | System.Collections.Generic.List<Terraria.GameContent.ItemDropRules.IItemDropRuleChainAttempt> | `public List<IItemDropRuleChainAttempt> ChainedRules { get; private set; }` | `public List<IItemDropRuleChainAttempt> ChainedRules { get; private set; }` |
| 3806 | property | Terraria.GameContent.ItemDropRules.DropBasedOnExtraGel | Terraria.GameContent.ItemDropRules/DropBasedOnExtraGel.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\DropBasedOnExtraGel.cs | 11 | 2 | ChainedRules | System.Collections.Generic.List<Terraria.GameContent.ItemDropRules.IItemDropRuleChainAttempt> | `public List<IItemDropRuleChainAttempt> ChainedRules { get; private set; }` | `public List<IItemDropRuleChainAttempt> ChainedRules { get; private set; }` |
| 3807 | property | Terraria.GameContent.ItemDropRules.DropBasedOnMasterAndExpertMode | Terraria.GameContent.ItemDropRules/DropBasedOnMasterAndExpertMode.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\DropBasedOnMasterAndExpertMode.cs | 13 | 2 | ChainedRules | System.Collections.Generic.List<Terraria.GameContent.ItemDropRules.IItemDropRuleChainAttempt> | `public List<IItemDropRuleChainAttempt> ChainedRules { get; private set; }` | `public List<IItemDropRuleChainAttempt> ChainedRules { get; private set; }` |
| 3808 | property | Terraria.GameContent.ItemDropRules.DropBasedOnMasterMode | Terraria.GameContent.ItemDropRules/DropBasedOnMasterMode.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\DropBasedOnMasterMode.cs | 11 | 2 | ChainedRules | System.Collections.Generic.List<Terraria.GameContent.ItemDropRules.IItemDropRuleChainAttempt> | `public List<IItemDropRuleChainAttempt> ChainedRules { get; private set; }` | `public List<IItemDropRuleChainAttempt> ChainedRules { get; private set; }` |
| 3809 | property | Terraria.GameContent.ItemDropRules.DropNothing | Terraria.GameContent.ItemDropRules/DropNothing.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\DropNothing.cs | 7 | 2 | ChainedRules | System.Collections.Generic.List<Terraria.GameContent.ItemDropRules.IItemDropRuleChainAttempt> | `public List<IItemDropRuleChainAttempt> ChainedRules { get; private set; }` | `public List<IItemDropRuleChainAttempt> ChainedRules { get; private set; }` |
| 3810 | property | Terraria.GameContent.ItemDropRules.DropOneByOne | Terraria.GameContent.ItemDropRules/DropOneByOne.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\DropOneByOne.cs | 37 | 2 | ChainedRules | System.Collections.Generic.List<Terraria.GameContent.ItemDropRules.IItemDropRuleChainAttempt> | `public List<IItemDropRuleChainAttempt> ChainedRules { get; private set; }` | `public List<IItemDropRuleChainAttempt> ChainedRules { get; private set; }` |
| 3811 | property | Terraria.GameContent.ItemDropRules.FromOptionsWithoutRepeatsDropRule | Terraria.GameContent.ItemDropRules/FromOptionsWithoutRepeatsDropRule.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\FromOptionsWithoutRepeatsDropRule.cs | 13 | 2 | ChainedRules | System.Collections.Generic.List<Terraria.GameContent.ItemDropRules.IItemDropRuleChainAttempt> | `public List<IItemDropRuleChainAttempt> ChainedRules { get; private set; }` | `public List<IItemDropRuleChainAttempt> ChainedRules { get; private set; }` |
| 3814 | property | Terraria.GameContent.ItemDropRules.LeadingConditionRule | Terraria.GameContent.ItemDropRules/LeadingConditionRule.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\LeadingConditionRule.cs | 9 | 2 | ChainedRules | System.Collections.Generic.List<Terraria.GameContent.ItemDropRules.IItemDropRuleChainAttempt> | `public List<IItemDropRuleChainAttempt> ChainedRules { get; private set; }` | `public List<IItemDropRuleChainAttempt> ChainedRules { get; private set; }` |
| 3815 | property | Terraria.GameContent.ItemDropRules.MechBossSpawnersDropRule | Terraria.GameContent.ItemDropRules/MechBossSpawnersDropRule.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\MechBossSpawnersDropRule.cs | 9 | 2 | ChainedRules | System.Collections.Generic.List<Terraria.GameContent.ItemDropRules.IItemDropRuleChainAttempt> | `public List<IItemDropRuleChainAttempt> ChainedRules { get; private set; }` | `public List<IItemDropRuleChainAttempt> ChainedRules { get; private set; }` |
| 3816 | property | Terraria.GameContent.ItemDropRules.OneFromOptionsDropRule | Terraria.GameContent.ItemDropRules/OneFromOptionsDropRule.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\OneFromOptionsDropRule.cs | 13 | 2 | ChainedRules | System.Collections.Generic.List<Terraria.GameContent.ItemDropRules.IItemDropRuleChainAttempt> | `public List<IItemDropRuleChainAttempt> ChainedRules { get; private set; }` | `public List<IItemDropRuleChainAttempt> ChainedRules { get; private set; }` |
| 3817 | property | Terraria.GameContent.ItemDropRules.OneFromOptionsNotScaledWithLuckDropRule | Terraria.GameContent.ItemDropRules/OneFromOptionsNotScaledWithLuckDropRule.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\OneFromOptionsNotScaledWithLuckDropRule.cs | 13 | 2 | ChainedRules | System.Collections.Generic.List<Terraria.GameContent.ItemDropRules.IItemDropRuleChainAttempt> | `public List<IItemDropRuleChainAttempt> ChainedRules { get; private set; }` | `public List<IItemDropRuleChainAttempt> ChainedRules { get; private set; }` |
| 3818 | property | Terraria.GameContent.ItemDropRules.OneFromRulesRule | Terraria.GameContent.ItemDropRules/OneFromRulesRule.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\OneFromRulesRule.cs | 11 | 2 | ChainedRules | System.Collections.Generic.List<Terraria.GameContent.ItemDropRules.IItemDropRuleChainAttempt> | `public List<IItemDropRuleChainAttempt> ChainedRules { get; private set; }` | `public List<IItemDropRuleChainAttempt> ChainedRules { get; private set; }` |
| 3819 | property | Terraria.GameContent.ItemDropRules.SlimeBodyItemDropRule | Terraria.GameContent.ItemDropRules/SlimeBodyItemDropRule.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\SlimeBodyItemDropRule.cs | 8 | 2 | ChainedRules | System.Collections.Generic.List<Terraria.GameContent.ItemDropRules.IItemDropRuleChainAttempt> | `public List<IItemDropRuleChainAttempt> ChainedRules { get; private set; }` | `public List<IItemDropRuleChainAttempt> ChainedRules { get; private set; }` |
| 3820 | property | Terraria.GameContent.ItemDropRules.StatueMimicItemDropRule | Terraria.GameContent.ItemDropRules/StatueMimicItemDropRule.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\StatueMimicItemDropRule.cs | 8 | 2 | ChainedRules | System.Collections.Generic.List<Terraria.GameContent.ItemDropRules.IItemDropRuleChainAttempt> | `public List<IItemDropRuleChainAttempt> ChainedRules { get; private set; }` | `public List<IItemDropRuleChainAttempt> ChainedRules { get; private set; }` |


### 4.15 细分子系统：`SharedFishingDropResolutionState`

- 原报告章节：`4.9.170`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`10` / `经济、配方、钓鱼与掉落`

- 上一级基线细分子系统：`SharedFishingDropRuleDefinitions`
- 上一级 peer 细分子系统：`SharedFishingDropCatalogState`
- 细分职责：FishDropRule、可能性条目和鱼获规则解析结果。
- 边界角色：`query/adapter`；最小 seam：fishing drop resolution port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：3；声明类型数：3；字段：8；属性：0；合计：8。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（8）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1653 | field | Terraria.GameContent.FishDropRules.FishDropRule | Terraria.GameContent.FishDropRules/FishDropRule.cs | D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\FishDropRule.cs | 5 | 2 | PossibleItems | int[] | `public int[] PossibleItems;` | `public int[] PossibleItems;` |
| 1654 | field | Terraria.GameContent.FishDropRules.FishDropRule | Terraria.GameContent.FishDropRules/FishDropRule.cs | D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\FishDropRule.cs | 7 | 2 | ChanceNumerator | int | `public int ChanceNumerator = 1;` | `public int ChanceNumerator = 1;` |
| 1655 | field | Terraria.GameContent.FishDropRules.FishDropRule | Terraria.GameContent.FishDropRules/FishDropRule.cs | D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\FishDropRule.cs | 9 | 2 | ChanceDenominator | int | `public int ChanceDenominator = 1;` | `public int ChanceDenominator = 1;` |
| 1656 | field | Terraria.GameContent.FishDropRules.FishDropRule | Terraria.GameContent.FishDropRules/FishDropRule.cs | D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\FishDropRule.cs | 11 | 2 | Conditions | Terraria.GameContent.FishDropRules.AFishingCondition[] | `public AFishingCondition[] Conditions;` | `public AFishingCondition[] Conditions;` |
| 1657 | field | Terraria.GameContent.FishDropRules.FishDropRule | Terraria.GameContent.FishDropRules/FishDropRule.cs | D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\FishDropRule.cs | 13 | 2 | Rarity | Terraria.GameContent.FishDropRules.FishRarityCondition | `public FishRarityCondition Rarity;` | `public FishRarityCondition Rarity;` |
| 1658 | field | Terraria.GameContent.FishDropRules.FishDropRuleList | Terraria.GameContent.FishDropRules/FishDropRuleList.cs | D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\FishDropRuleList.cs | 8 | 2 | _rules | System.Collections.Generic.List<Terraria.GameContent.FishDropRules.FishDropRule> | `private List<FishDropRule> _rules = new List<FishDropRule>();` | `private List<FishDropRule> _rules = new List<FishDropRule>();` |
| 1671 | field | Terraria.GameContent.FishDropRules.FishPossibilityEntry | Terraria.GameContent.FishDropRules/FishPossibilityEntry.cs | D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\FishPossibilityEntry.cs | 5 | 2 | ItemType | int | `public int ItemType;` | `public int ItemType;` |
| 1672 | field | Terraria.GameContent.FishDropRules.FishPossibilityEntry | Terraria.GameContent.FishDropRules/FishPossibilityEntry.cs | D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\FishPossibilityEntry.cs | 7 | 2 | Frequency | float | `public float Frequency;` | `public float Frequency;` |

#### 属性（0）

无该类型成员记录。


### 4.16 细分子系统：`SharedItemEconomyAndValueRules`

- 原报告章节：`4.9.178`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`10` / `经济、配方、钓鱼与掉落`

- 上一级基线细分子系统：`SharedItemStaticCatalogRules`
- 上一级 peer 细分子系统：`SharedItemStaticEconomyAndTimingRules`
- 细分职责：物品货币、价格、稀有度和经济价值规则。
- 边界角色：`definition/catalog`；最小 seam：item economy value rules port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：12；属性：0；合计：12。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（12）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3149 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 42 | 2 | copper | int | `public const int copper = 1;` | `public const int copper = 1;` |
| 3150 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 44 | 2 | silver | int | `public const int silver = 100;` | `public const int silver = 100;` |
| 3151 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 46 | 2 | gold | int | `public const int gold = 10000;` | `public const int gold = 10000;` |
| 3152 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 48 | 2 | platinum | int | `public const int platinum = 1000000;` | `public const int platinum = 1000000;` |
| 3153 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 50 | 2 | goldCritterRarityColor | int | `public const int goldCritterRarityColor = 3;` | `public const int goldCritterRarityColor = 3;` |
| 3154 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 52 | 2 | shadowOrbPrice | int | `private readonly int shadowOrbPrice = sellPrice(0, 1, 50);` | `private readonly int shadowOrbPrice = sellPrice(0, 1, 50);` |
| 3155 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 54 | 2 | dungeonPrice | int | `private readonly int dungeonPrice = sellPrice(0, 1, 75);` | `private readonly int dungeonPrice = sellPrice(0, 1, 75);` |
| 3156 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 56 | 2 | queenBeePrice | int | `private readonly int queenBeePrice = sellPrice(0, 2);` | `private readonly int queenBeePrice = sellPrice(0, 2);` |
| 3157 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 58 | 2 | hellPrice | int | `private readonly int hellPrice = sellPrice(0, 2, 50);` | `private readonly int hellPrice = sellPrice(0, 2, 50);` |
| 3158 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 60 | 2 | eclipsePrice | int | `private readonly int eclipsePrice = sellPrice(0, 7, 50);` | `private readonly int eclipsePrice = sellPrice(0, 7, 50);` |
| 3159 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 62 | 2 | eclipsePostPlanteraPrice | int | `private readonly int eclipsePostPlanteraPrice = sellPrice(0, 10);` | `private readonly int eclipsePostPlanteraPrice = sellPrice(0, 10);` |
| 3160 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 64 | 2 | eclipseMothronPrice | int | `private readonly int eclipseMothronPrice = sellPrice(0, 12, 50);` | `private readonly int eclipseMothronPrice = sellPrice(0, 12, 50);` |

#### 属性（0）

无该类型成员记录。


### 4.17 细分子系统：`SharedItemUseTimingAndStackRules`

- 原报告章节：`4.9.179`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`10` / `经济、配方、钓鱼与掉落`

- 上一级基线细分子系统：`SharedItemStaticCatalogRules`
- 上一级 peer 细分子系统：`SharedItemStaticEconomyAndTimingRules`
- 细分职责：物品使用时序、拾取、食物、冷却和堆叠规则。
- 边界角色：`definition/catalog`；最小 seam：item use timing stack port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：18；属性：0；合计：18。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（18）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3140 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 24 | 2 | coinGrabRange | int | `public static int coinGrabRange = 350;` | `public static int coinGrabRange = 350;` |
| 3141 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 26 | 2 | manaGrabRange | int | `public static int manaGrabRange = 300;` | `public static int manaGrabRange = 300;` |
| 3142 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 28 | 2 | lifeGrabRange | int | `public static int lifeGrabRange = 250;` | `public static int lifeGrabRange = 250;` |
| 3143 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 30 | 2 | treasureGrabRange | int | `public static int treasureGrabRange = 150;` | `public static int treasureGrabRange = 150;` |
| 3145 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 34 | 2 | luckPotionDuration1 | int | `public const int luckPotionDuration1 = 18000;` | `public const int luckPotionDuration1 = 18000;` |
| 3146 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 36 | 2 | luckPotionDuration2 | int | `public const int luckPotionDuration2 = 36000;` | `public const int luckPotionDuration2 = 36000;` |
| 3147 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 38 | 2 | luckPotionDuration3 | int | `public const int luckPotionDuration3 = 54000;` | `public const int luckPotionDuration3 = 54000;` |
| 3148 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 40 | 2 | flaskTime | int | `public const int flaskTime = 72000;` | `public const int flaskTime = 72000;` |
| 3161 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 66 | 2 | CommonMaxStack | int | `public static int CommonMaxStack = 9999;` | `public static int CommonMaxStack = 9999;` |
| 3163 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 70 | 2 | potionDelay | int | `public static int potionDelay = 3600;` | `public static int potionDelay = 3600;` |
| 3164 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 72 | 2 | restorationDelay | int | `public static int restorationDelay = 2700;` | `public static int restorationDelay = 2700;` |
| 3165 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 74 | 2 | eggnogDelay | int | `public static int eggnogDelay = 2400;` | `public static int eggnogDelay = 2400;` |
| 3166 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 76 | 2 | mushroomDelay | int | `public static int mushroomDelay = 1800;` | `public static int mushroomDelay = 1800;` |
| 3280 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 305 | 2 | foodWidth | int | `private const int foodWidth = 22;` | `private const int foodWidth = 22;` |
| 3281 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 307 | 2 | foodHeight | int | `private const int foodHeight = 22;` | `private const int foodHeight = 22;` |
| 3282 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 309 | 2 | WALL_PLACEMENT_USETIME | int | `public const int WALL_PLACEMENT_USETIME = 7;` | `public const int WALL_PLACEMENT_USETIME = 7;` |
| 3284 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 313 | 2 | PickupReplacementTime | int | `public static readonly int PickupReplacementTime = 1200;` | `public static readonly int PickupReplacementTime = 1200;` |
| 3285 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 315 | 2 | SlotsRemainingBeforeEmergencyStackingInMultiplayer | int | `public static readonly int SlotsRemainingBeforeEmergencyStackingInMultiplayer = 40;` | `public static readonly int SlotsRemainingBeforeEmergencyStackingInMultiplayer = 40;` |

#### 属性（0）

无该类型成员记录。


### 4.18 细分子系统：`SharedFishingRarityConditionCatalogState`

- 原报告章节：`4.9.199`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`10` / `经济、配方、钓鱼与掉落`

- 上一级基线细分子系统：`SharedFishingDropRuleDefinitions`
- 上一级 peer 细分子系统：`SharedFishingConditionCatalogState`
- 细分职责：鱼获稀有度枚举、稀有度条件 delegate 和视觉频率定义。
- 边界角色：`definition/catalog`；最小 seam：fishing rarity condition port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：2；声明类型数：3；字段：11；属性：0；合计：11。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（11）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1605 | field | Terraria.GameContent.FishDropRules.AFishDropRulePopulator.DelegateFishingRarityCondition | Terraria.GameContent.FishDropRules/AFishDropRulePopulator.cs | D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\AFishDropRulePopulator.cs | 26 | 3 | _condition | Terraria.GameContent.FishDropRules.AFishDropRulePopulator.DelegateFishingRarityCondition.MatchCondition | `private MatchCondition _condition;` | `private MatchCondition _condition;` |
| 1606 | field | Terraria.GameContent.FishDropRules.AFishDropRulePopulator.Rarity | Terraria.GameContent.FishDropRules/AFishDropRulePopulator.cs | D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\AFishDropRulePopulator.cs | 39 | 3 | Any | Terraria.GameContent.FishDropRules.FishRarityCondition | `public static FishRarityCondition Any = new DelegateFishingRarityCondition((FishingContext context) => true)  		{  			HackedIsAny = true,  			FrequencyOfAppearanceForVisuals = 1f  		};` | `public static FishRarityCondition Any = new DelegateFishingRarityCondition((FishingContext context) => true) { HackedIsAny = true, FrequencyOfAppearanceForVisuals = 1f };` |
| 1607 | field | Terraria.GameContent.FishDropRules.AFishDropRulePopulator.Rarity | Terraria.GameContent.FishDropRules/AFishDropRulePopulator.cs | D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\AFishDropRulePopulator.cs | 45 | 3 | Legendary | Terraria.GameContent.FishDropRules.FishRarityCondition | `public static FishRarityCondition Legendary = new DelegateFishingRarityCondition((FishingContext context) => context.Fisher.legendary)  		{  			FrequencyOfAppearanceForVisuals = 0.1f  		};` | `public static FishRarityCondition Legendary = new DelegateFishingRarityCondition((FishingContext context) => context.Fisher.legendary) { FrequencyOfAppearanceForVisuals = 0.1f };` |
| 1608 | field | Terraria.GameContent.FishDropRules.AFishDropRulePopulator.Rarity | Terraria.GameContent.FishDropRules/AFishDropRulePopulator.cs | D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\AFishDropRulePopulator.cs | 50 | 3 | VeryRare | Terraria.GameContent.FishDropRules.FishRarityCondition | `public static FishRarityCondition VeryRare = new DelegateFishingRarityCondition((FishingContext context) => context.Fisher.veryrare)  		{  			FrequencyOfAppearanceForVisuals = 0.25f  		};` | `public static FishRarityCondition VeryRare = new DelegateFishingRarityCondition((FishingContext context) => context.Fisher.veryrare) { FrequencyOfAppearanceForVisuals = 0.25f };` |
| 1609 | field | Terraria.GameContent.FishDropRules.AFishDropRulePopulator.Rarity | Terraria.GameContent.FishDropRules/AFishDropRulePopulator.cs | D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\AFishDropRulePopulator.cs | 55 | 3 | Rare | Terraria.GameContent.FishDropRules.FishRarityCondition | `public static FishRarityCondition Rare = new DelegateFishingRarityCondition((FishingContext context) => context.Fisher.rare)  		{  			FrequencyOfAppearanceForVisuals = 0.4f  		};` | `public static FishRarityCondition Rare = new DelegateFishingRarityCondition((FishingContext context) => context.Fisher.rare) { FrequencyOfAppearanceForVisuals = 0.4f };` |
| 1610 | field | Terraria.GameContent.FishDropRules.AFishDropRulePopulator.Rarity | Terraria.GameContent.FishDropRules/AFishDropRulePopulator.cs | D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\AFishDropRulePopulator.cs | 60 | 3 | Uncommon | Terraria.GameContent.FishDropRules.FishRarityCondition | `public static FishRarityCondition Uncommon = new DelegateFishingRarityCondition((FishingContext context) => context.Fisher.uncommon)  		{  			FrequencyOfAppearanceForVisuals = 0.8f  		};` | `public static FishRarityCondition Uncommon = new DelegateFishingRarityCondition((FishingContext context) => context.Fisher.uncommon) { FrequencyOfAppearanceForVisuals = 0.8f };` |
| 1611 | field | Terraria.GameContent.FishDropRules.AFishDropRulePopulator.Rarity | Terraria.GameContent.FishDropRules/AFishDropRulePopulator.cs | D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\AFishDropRulePopulator.cs | 65 | 3 | Common | Terraria.GameContent.FishDropRules.FishRarityCondition | `public static FishRarityCondition Common = new DelegateFishingRarityCondition((FishingContext context) => context.Fisher.common)  		{  			FrequencyOfAppearanceForVisuals = 1f  		};` | `public static FishRarityCondition Common = new DelegateFishingRarityCondition((FishingContext context) => context.Fisher.common) { FrequencyOfAppearanceForVisuals = 1f };` |
| 1612 | field | Terraria.GameContent.FishDropRules.AFishDropRulePopulator.Rarity | Terraria.GameContent.FishDropRules/AFishDropRulePopulator.cs | D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\AFishDropRulePopulator.cs | 70 | 3 | BombRarityOfNotLegendaryAndNotVeryRareAndUncommon | Terraria.GameContent.FishDropRules.FishRarityCondition | `public static FishRarityCondition BombRarityOfNotLegendaryAndNotVeryRareAndUncommon = new DelegateFishingRarityCondition((FishingContext context) => !context.Fisher.legendary && !context.Fisher.veryrare && context.Fisher.uncommon)  		{  			FrequencyOfAppearanceForVisuals = 0.6f  		};` | `public static FishRarityCondition BombRarityOfNotLegendaryAndNotVeryRareAndUncommon = new DelegateFishingRarityCondition((FishingContext context) => !context.Fisher.legendary && !context.Fisher.veryrare && context.Fisher.uncommon) { FrequencyOfAppearanceForVisuals = 0.6f };` |
| 1613 | field | Terraria.GameContent.FishDropRules.AFishDropRulePopulator.Rarity | Terraria.GameContent.FishDropRules/AFishDropRulePopulator.cs | D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\AFishDropRulePopulator.cs | 75 | 3 | UncommonOrCommon | Terraria.GameContent.FishDropRules.FishRarityCondition | `public static FishRarityCondition UncommonOrCommon = new DelegateFishingRarityCondition((FishingContext context) => context.Fisher.uncommon \|\| context.Fisher.common)  		{  			FrequencyOfAppearanceForVisuals = 1f  		};` | `public static FishRarityCondition UncommonOrCommon = new DelegateFishingRarityCondition((FishingContext context) => context.Fisher.uncommon \|\| context.Fisher.common) { FrequencyOfAppearanceForVisuals = 1f };` |
| 1673 | field | Terraria.GameContent.FishDropRules.FishRarityCondition | Terraria.GameContent.FishDropRules/FishRarityCondition.cs | D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\FishRarityCondition.cs | 5 | 2 | FrequencyOfAppearanceForVisuals | float | `public float FrequencyOfAppearanceForVisuals;` | `public float FrequencyOfAppearanceForVisuals;` |
| 1674 | field | Terraria.GameContent.FishDropRules.FishRarityCondition | Terraria.GameContent.FishDropRules/FishRarityCondition.cs | D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\FishRarityCondition.cs | 7 | 2 | HackedIsAny | bool | `public bool HackedIsAny;` | `public bool HackedIsAny;` |

#### 属性（0）

无该类型成员记录。


### 4.19 细分子系统：`SharedDropRuleConditionBranchState`

- 原报告章节：`4.9.200`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`10` / `经济、配方、钓鱼与掉落`

- 上一级基线细分子系统：`SharedDropRuleDefinitions`
- 上一级 peer 细分子系统：`SharedDropRuleSelectionAndConditionState`
- 细分职责：掉落条件类型、条件字段和模式分支资格判断。
- 边界角色：`definition/query`；最小 seam：drop rule condition branch port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：6；声明类型数：8；字段：9；属性：0；合计：9。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（9）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2045 | field | Terraria.GameContent.ItemDropRules.Conditions.IsUsingSpecificAIValues | Terraria.GameContent.ItemDropRules/Conditions.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\Conditions.cs | 23 | 3 | aiSlotToCheck | int | `public int aiSlotToCheck;` | `public int aiSlotToCheck;` |
| 2046 | field | Terraria.GameContent.ItemDropRules.Conditions.IsUsingSpecificAIValues | Terraria.GameContent.ItemDropRules/Conditions.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\Conditions.cs | 25 | 3 | valueToMatch | float | `public float valueToMatch;` | `public float valueToMatch;` |
| 2047 | field | Terraria.GameContent.ItemDropRules.Conditions.FromCertainWaveAndAbove | Terraria.GameContent.ItemDropRules/Conditions.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\Conditions.cs | 105 | 3 | neededWave | int | `public int neededWave;` | `public int neededWave;` |
| 2048 | field | Terraria.GameContent.ItemDropRules.Conditions.NamedNPC | Terraria.GameContent.ItemDropRules/Conditions.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\Conditions.cs | 600 | 3 | neededName | string | `public string neededName;` | `public string neededName;` |
| 2064 | field | Terraria.GameContent.ItemDropRules.DropLocalPerClientAndResetsNPCMoneyTo0 | Terraria.GameContent.ItemDropRules/DropLocalPerClientAndResetsNPCMoneyTo0.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\DropLocalPerClientAndResetsNPCMoneyTo0.cs | 5 | 2 | condition | Terraria.GameContent.ItemDropRules.IItemDropRuleCondition | `public IItemDropRuleCondition condition;` | `public IItemDropRuleCondition condition;` |
| 2075 | field | Terraria.GameContent.ItemDropRules.DropPerPlayerOnThePlayer | Terraria.GameContent.ItemDropRules/DropPerPlayerOnThePlayer.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\DropPerPlayerOnThePlayer.cs | 5 | 2 | condition | Terraria.GameContent.ItemDropRules.IItemDropRuleCondition | `public IItemDropRuleCondition condition;` | `public IItemDropRuleCondition condition;` |
| 2092 | field | Terraria.GameContent.ItemDropRules.ItemDropWithConditionRule | Terraria.GameContent.ItemDropRules/ItemDropWithConditionRule.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\ItemDropWithConditionRule.cs | 7 | 2 | condition | Terraria.GameContent.ItemDropRules.IItemDropRuleCondition | `public IItemDropRuleCondition condition;` | `public IItemDropRuleCondition condition;` |
| 2093 | field | Terraria.GameContent.ItemDropRules.LeadingConditionRule | Terraria.GameContent.ItemDropRules/LeadingConditionRule.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\LeadingConditionRule.cs | 7 | 2 | condition | Terraria.GameContent.ItemDropRules.IItemDropRuleCondition | `public IItemDropRuleCondition condition;` | `public IItemDropRuleCondition condition;` |
| 2094 | field | Terraria.GameContent.ItemDropRules.MechBossSpawnersDropRule | Terraria.GameContent.ItemDropRules/MechBossSpawnersDropRule.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\MechBossSpawnersDropRule.cs | 7 | 2 | dummyCondition | Terraria.GameContent.ItemDropRules.Conditions.MechanicalBossesDummyCondition | `public Conditions.MechanicalBossesDummyCondition dummyCondition = new Conditions.MechanicalBossesDummyCondition();` | `public Conditions.MechanicalBossesDummyCondition dummyCondition = new Conditions.MechanicalBossesDummyCondition();` |

#### 属性（0）

无该类型成员记录。


### 4.20 细分子系统：`SharedFishingConditionCatalogPopulationState`

- 原报告章节：`4.9.206`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`10` / `经济、配方、钓鱼与掉落`

- 上一级基线细分子系统：`SharedFishingDropRuleDefinitions`
- 上一级 peer 细分子系统：`SharedFishingEnvironmentConditionCatalogState`
- 细分职责：钓鱼条件目录的注册、集合和模式/资格元数据。
- 边界角色：`definition/catalog`；最小 seam：fishing condition catalog population port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：2；字段：8；属性：0；合计：8。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（8）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1604 | field | Terraria.GameContent.FishDropRules.AFishDropRulePopulator.DelegateFishingCondition | Terraria.GameContent.FishDropRules/AFishDropRulePopulator.cs | D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\AFishDropRulePopulator.cs | 11 | 3 | _condition | Terraria.GameContent.FishDropRules.AFishDropRulePopulator.DelegateFishingCondition.MatchCondition | `private MatchCondition _condition;` | `private MatchCondition _condition;` |
| 1614 | field | Terraria.GameContent.FishDropRules.AFishDropRulePopulator | Terraria.GameContent.FishDropRules/AFishDropRulePopulator.cs | D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\AFishDropRulePopulator.cs | 81 | 2 | _list | Terraria.GameContent.FishDropRules.FishDropRuleList | `private FishDropRuleList _list;` | `private FishDropRuleList _list;` |
| 1615 | field | Terraria.GameContent.FishDropRules.AFishDropRulePopulator | Terraria.GameContent.FishDropRules/AFishDropRulePopulator.cs | D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\AFishDropRulePopulator.cs | 83 | 2 | HardMode | Terraria.GameContent.FishDropRules.AFishingCondition | `protected AFishingCondition HardMode = new DelegateFishingCondition((FishingContext context) => IsHardmode(state: true));` | `protected AFishingCondition HardMode = new DelegateFishingCondition((FishingContext context) => IsHardmode(state: true));` |
| 1616 | field | Terraria.GameContent.FishDropRules.AFishDropRulePopulator | Terraria.GameContent.FishDropRules/AFishDropRulePopulator.cs | D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\AFishDropRulePopulator.cs | 85 | 2 | EarlyMode | Terraria.GameContent.FishDropRules.AFishingCondition | `protected AFishingCondition EarlyMode = new DelegateFishingCondition((FishingContext context) => IsHardmode(state: false));` | `protected AFishingCondition EarlyMode = new DelegateFishingCondition((FishingContext context) => IsHardmode(state: false));` |
| 1619 | field | Terraria.GameContent.FishDropRules.AFishDropRulePopulator | Terraria.GameContent.FishDropRules/AFishDropRulePopulator.cs | D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\AFishDropRulePopulator.cs | 91 | 2 | Junk | Terraria.GameContent.FishDropRules.AFishingCondition | `protected AFishingCondition Junk = new DelegateFishingCondition((FishingContext context) => context.Fisher.junk);` | `protected AFishingCondition Junk = new DelegateFishingCondition((FishingContext context) => context.Fisher.junk);` |
| 1620 | field | Terraria.GameContent.FishDropRules.AFishDropRulePopulator | Terraria.GameContent.FishDropRules/AFishDropRulePopulator.cs | D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\AFishDropRulePopulator.cs | 93 | 2 | Crate | Terraria.GameContent.FishDropRules.AFishingCondition | `protected AFishingCondition Crate = new DelegateFishingCondition((FishingContext context) => context.Fisher.crate);` | `protected AFishingCondition Crate = new DelegateFishingCondition((FishingContext context) => context.Fisher.crate);` |
| 1621 | field | Terraria.GameContent.FishDropRules.AFishDropRulePopulator | Terraria.GameContent.FishDropRules/AFishDropRulePopulator.cs | D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\AFishDropRulePopulator.cs | 95 | 2 | AnyEnemies | Terraria.GameContent.FishDropRules.AFishingCondition | `protected AFishingCondition AnyEnemies = new DelegateFishingCondition((FishingContext context) => context.Fisher.rolledEnemySpawn > 0);` | `protected AFishingCondition AnyEnemies = new DelegateFishingCondition((FishingContext context) => context.Fisher.rolledEnemySpawn > 0);` |
| 1651 | field | Terraria.GameContent.FishDropRules.AFishDropRulePopulator | Terraria.GameContent.FishDropRules/AFishDropRulePopulator.cs | D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\AFishDropRulePopulator.cs | 155 | 2 | DidNotUseCombatBook | Terraria.GameContent.FishDropRules.AFishingCondition | `protected AFishingCondition DidNotUseCombatBook = new DelegateFishingCondition((FishingContext context) => !NPC.combatBookWasUsed);` | `protected AFishingCondition DidNotUseCombatBook = new DelegateFishingCondition((FishingContext context) => !NPC.combatBookWasUsed);` |

#### 属性（0）

无该类型成员记录。


### 4.21 细分子系统：`SharedFishingEnvironmentPredicateState`

- 原报告章节：`4.9.207`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`10` / `经济、配方、钓鱼与掉落`

- 上一级基线细分子系统：`SharedFishingDropRuleDefinitions`
- 上一级 peer 细分子系统：`SharedFishingEnvironmentConditionCatalogState`
- 细分职责：钓鱼液体、深度、生物群落、海洋和世界事件环境条件。
- 边界角色：`definition/query`；最小 seam：fishing environment predicate port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：31；属性：0；合计：31。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（31）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1617 | field | Terraria.GameContent.FishDropRules.AFishDropRulePopulator | Terraria.GameContent.FishDropRules/AFishDropRulePopulator.cs | D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\AFishDropRulePopulator.cs | 87 | 2 | InLava | Terraria.GameContent.FishDropRules.AFishingCondition | `protected AFishingCondition InLava = new DelegateFishingCondition((FishingContext context) => context.Fisher.inLava);` | `protected AFishingCondition InLava = new DelegateFishingCondition((FishingContext context) => context.Fisher.inLava);` |
| 1618 | field | Terraria.GameContent.FishDropRules.AFishDropRulePopulator | Terraria.GameContent.FishDropRules/AFishDropRulePopulator.cs | D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\AFishDropRulePopulator.cs | 89 | 2 | InHoney | Terraria.GameContent.FishDropRules.AFishingCondition | `protected AFishingCondition InHoney = new DelegateFishingCondition((FishingContext context) => context.Fisher.inHoney);` | `protected AFishingCondition InHoney = new DelegateFishingCondition((FishingContext context) => context.Fisher.inHoney);` |
| 1622 | field | Terraria.GameContent.FishDropRules.AFishDropRulePopulator | Terraria.GameContent.FishDropRules/AFishDropRulePopulator.cs | D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\AFishDropRulePopulator.cs | 97 | 2 | CanFishInLava | Terraria.GameContent.FishDropRules.AFishingCondition | `protected AFishingCondition CanFishInLava = new DelegateFishingCondition((FishingContext context) => context.Fisher.CanFishInLava);` | `protected AFishingCondition CanFishInLava = new DelegateFishingCondition((FishingContext context) => context.Fisher.CanFishInLava);` |
| 1623 | field | Terraria.GameContent.FishDropRules.AFishDropRulePopulator | Terraria.GameContent.FishDropRules/AFishDropRulePopulator.cs | D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\AFishDropRulePopulator.cs | 99 | 2 | Dungeon | Terraria.GameContent.FishDropRules.AFishingCondition | `protected AFishingCondition Dungeon = new DelegateFishingCondition((FishingContext context) => context.Player.ZoneDungeon && NPC.downedBoss3);` | `protected AFishingCondition Dungeon = new DelegateFishingCondition((FishingContext context) => context.Player.ZoneDungeon && NPC.downedBoss3);` |
| 1624 | field | Terraria.GameContent.FishDropRules.AFishDropRulePopulator | Terraria.GameContent.FishDropRules/AFishDropRulePopulator.cs | D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\AFishDropRulePopulator.cs | 101 | 2 | Beach | Terraria.GameContent.FishDropRules.AFishingCondition | `protected AFishingCondition Beach = new DelegateFishingCondition((FishingContext context) => context.Player.ZoneBeach);` | `protected AFishingCondition Beach = new DelegateFishingCondition((FishingContext context) => context.Player.ZoneBeach);` |
| 1625 | field | Terraria.GameContent.FishDropRules.AFishDropRulePopulator | Terraria.GameContent.FishDropRules/AFishDropRulePopulator.cs | D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\AFishDropRulePopulator.cs | 103 | 2 | Hallow | Terraria.GameContent.FishDropRules.AFishingCondition | `protected AFishingCondition Hallow = new DelegateFishingCondition((FishingContext context) => context.Player.ZoneHallow);` | `protected AFishingCondition Hallow = new DelegateFishingCondition((FishingContext context) => context.Player.ZoneHallow);` |
| 1626 | field | Terraria.GameContent.FishDropRules.AFishDropRulePopulator | Terraria.GameContent.FishDropRules/AFishDropRulePopulator.cs | D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\AFishDropRulePopulator.cs | 105 | 2 | GlowingMushrooms | Terraria.GameContent.FishDropRules.AFishingCondition | `protected AFishingCondition GlowingMushrooms = new DelegateFishingCondition((FishingContext context) => context.Player.ZoneGlowshroom);` | `protected AFishingCondition GlowingMushrooms = new DelegateFishingCondition((FishingContext context) => context.Player.ZoneGlowshroom);` |
| 1627 | field | Terraria.GameContent.FishDropRules.AFishDropRulePopulator | Terraria.GameContent.FishDropRules/AFishDropRulePopulator.cs | D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\AFishDropRulePopulator.cs | 107 | 2 | TrueDesert | Terraria.GameContent.FishDropRules.AFishingCondition | `protected AFishingCondition TrueDesert = new DelegateFishingCondition((FishingContext context) => context.Player.ZoneDesert);` | `protected AFishingCondition TrueDesert = new DelegateFishingCondition((FishingContext context) => context.Player.ZoneDesert);` |
| 1628 | field | Terraria.GameContent.FishDropRules.AFishDropRulePopulator | Terraria.GameContent.FishDropRules/AFishDropRulePopulator.cs | D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\AFishDropRulePopulator.cs | 109 | 2 | TrueSnow | Terraria.GameContent.FishDropRules.AFishingCondition | `protected AFishingCondition TrueSnow = new DelegateFishingCondition((FishingContext context) => context.Player.ZoneSnow);` | `protected AFishingCondition TrueSnow = new DelegateFishingCondition((FishingContext context) => context.Player.ZoneSnow);` |
| 1629 | field | Terraria.GameContent.FishDropRules.AFishDropRulePopulator | Terraria.GameContent.FishDropRules/AFishDropRulePopulator.cs | D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\AFishDropRulePopulator.cs | 111 | 2 | Remix | Terraria.GameContent.FishDropRules.AFishingCondition | `protected AFishingCondition Remix = new DelegateFishingCondition((FishingContext context) => Main.remixWorld);` | `protected AFishingCondition Remix = new DelegateFishingCondition((FishingContext context) => Main.remixWorld);` |
| 1630 | field | Terraria.GameContent.FishDropRules.AFishDropRulePopulator | Terraria.GameContent.FishDropRules/AFishDropRulePopulator.cs | D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\AFishDropRulePopulator.cs | 113 | 2 | Height1 | Terraria.GameContent.FishDropRules.AFishingCondition | `protected AFishingCondition Height1 = new DelegateFishingCondition((FishingContext context) => context.Fisher.heightLevel == 1);` | `protected AFishingCondition Height1 = new DelegateFishingCondition((FishingContext context) => context.Fisher.heightLevel == 1);` |
| 1631 | field | Terraria.GameContent.FishDropRules.AFishDropRulePopulator | Terraria.GameContent.FishDropRules/AFishDropRulePopulator.cs | D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\AFishDropRulePopulator.cs | 115 | 2 | Height1And2 | Terraria.GameContent.FishDropRules.AFishingCondition | `protected AFishingCondition Height1And2 = new DelegateFishingCondition((FishingContext context) => context.Fisher.heightLevel == 1 \|\| context.Fisher.heightLevel == 2);` | `protected AFishingCondition Height1And2 = new DelegateFishingCondition((FishingContext context) => context.Fisher.heightLevel == 1 \|\| context.Fisher.heightLevel == 2);` |
| 1632 | field | Terraria.GameContent.FishDropRules.AFishDropRulePopulator | Terraria.GameContent.FishDropRules/AFishDropRulePopulator.cs | D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\AFishDropRulePopulator.cs | 117 | 2 | HeightAbove1 | Terraria.GameContent.FishDropRules.AFishingCondition | `protected AFishingCondition HeightAbove1 = new DelegateFishingCondition((FishingContext context) => context.Fisher.heightLevel > 1);` | `protected AFishingCondition HeightAbove1 = new DelegateFishingCondition((FishingContext context) => context.Fisher.heightLevel > 1);` |
| 1633 | field | Terraria.GameContent.FishDropRules.AFishDropRulePopulator | Terraria.GameContent.FishDropRules/AFishDropRulePopulator.cs | D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\AFishDropRulePopulator.cs | 119 | 2 | HeightAboveAnd1 | Terraria.GameContent.FishDropRules.AFishingCondition | `protected AFishingCondition HeightAboveAnd1 = new DelegateFishingCondition((FishingContext context) => context.Fisher.heightLevel >= 1);` | `protected AFishingCondition HeightAboveAnd1 = new DelegateFishingCondition((FishingContext context) => context.Fisher.heightLevel >= 1);` |
| 1634 | field | Terraria.GameContent.FishDropRules.AFishDropRulePopulator | Terraria.GameContent.FishDropRules/AFishDropRulePopulator.cs | D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\AFishDropRulePopulator.cs | 121 | 2 | HeightUnder2 | Terraria.GameContent.FishDropRules.AFishingCondition | `protected AFishingCondition HeightUnder2 = new DelegateFishingCondition((FishingContext context) => context.Fisher.heightLevel < 2);` | `protected AFishingCondition HeightUnder2 = new DelegateFishingCondition((FishingContext context) => context.Fisher.heightLevel < 2);` |
| 1635 | field | Terraria.GameContent.FishDropRules.AFishDropRulePopulator | Terraria.GameContent.FishDropRules/AFishDropRulePopulator.cs | D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\AFishDropRulePopulator.cs | 123 | 2 | HeightAbove2 | Terraria.GameContent.FishDropRules.AFishingCondition | `protected AFishingCondition HeightAbove2 = new DelegateFishingCondition((FishingContext context) => context.Fisher.heightLevel > 2);` | `protected AFishingCondition HeightAbove2 = new DelegateFishingCondition((FishingContext context) => context.Fisher.heightLevel > 2);` |
| 1636 | field | Terraria.GameContent.FishDropRules.AFishDropRulePopulator | Terraria.GameContent.FishDropRules/AFishDropRulePopulator.cs | D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\AFishDropRulePopulator.cs | 125 | 2 | Height0 | Terraria.GameContent.FishDropRules.AFishingCondition | `protected AFishingCondition Height0 = new DelegateFishingCondition((FishingContext context) => context.Fisher.heightLevel == 0);` | `protected AFishingCondition Height0 = new DelegateFishingCondition((FishingContext context) => context.Fisher.heightLevel == 0);` |
| 1637 | field | Terraria.GameContent.FishDropRules.AFishDropRulePopulator | Terraria.GameContent.FishDropRules/AFishDropRulePopulator.cs | D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\AFishDropRulePopulator.cs | 127 | 2 | Height2 | Terraria.GameContent.FishDropRules.AFishingCondition | `protected AFishingCondition Height2 = new DelegateFishingCondition((FishingContext context) => context.Fisher.heightLevel == 2);` | `protected AFishingCondition Height2 = new DelegateFishingCondition((FishingContext context) => context.Fisher.heightLevel == 2);` |
| 1638 | field | Terraria.GameContent.FishDropRules.AFishDropRulePopulator | Terraria.GameContent.FishDropRules/AFishDropRulePopulator.cs | D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\AFishDropRulePopulator.cs | 129 | 2 | Height3 | Terraria.GameContent.FishDropRules.AFishingCondition | `protected AFishingCondition Height3 = new DelegateFishingCondition((FishingContext context) => context.Fisher.heightLevel == 3);` | `protected AFishingCondition Height3 = new DelegateFishingCondition((FishingContext context) => context.Fisher.heightLevel == 3);` |
| 1639 | field | Terraria.GameContent.FishDropRules.AFishDropRulePopulator | Terraria.GameContent.FishDropRules/AFishDropRulePopulator.cs | D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\AFishDropRulePopulator.cs | 131 | 2 | UnderRockLayer | Terraria.GameContent.FishDropRules.AFishingCondition | `protected AFishingCondition UnderRockLayer = new DelegateFishingCondition((FishingContext context) => (double)context.Fisher.Y >= Main.rockLayer);` | `protected AFishingCondition UnderRockLayer = new DelegateFishingCondition((FishingContext context) => (double)context.Fisher.Y >= Main.rockLayer);` |
| 1640 | field | Terraria.GameContent.FishDropRules.AFishDropRulePopulator | Terraria.GameContent.FishDropRules/AFishDropRulePopulator.cs | D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\AFishDropRulePopulator.cs | 133 | 2 | Corruption | Terraria.GameContent.FishDropRules.AFishingCondition | `protected AFishingCondition Corruption = new DelegateFishingCondition((FishingContext context) => context.RolledCorruption);` | `protected AFishingCondition Corruption = new DelegateFishingCondition((FishingContext context) => context.RolledCorruption);` |
| 1641 | field | Terraria.GameContent.FishDropRules.AFishDropRulePopulator | Terraria.GameContent.FishDropRules/AFishDropRulePopulator.cs | D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\AFishDropRulePopulator.cs | 135 | 2 | Crimson | Terraria.GameContent.FishDropRules.AFishingCondition | `protected AFishingCondition Crimson = new DelegateFishingCondition((FishingContext context) => context.RolledCrimson);` | `protected AFishingCondition Crimson = new DelegateFishingCondition((FishingContext context) => context.RolledCrimson);` |
| 1642 | field | Terraria.GameContent.FishDropRules.AFishDropRulePopulator | Terraria.GameContent.FishDropRules/AFishDropRulePopulator.cs | D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\AFishDropRulePopulator.cs | 137 | 2 | Jungle | Terraria.GameContent.FishDropRules.AFishingCondition | `protected AFishingCondition Jungle = new DelegateFishingCondition((FishingContext context) => context.RolledJungle);` | `protected AFishingCondition Jungle = new DelegateFishingCondition((FishingContext context) => context.RolledJungle);` |
| 1643 | field | Terraria.GameContent.FishDropRules.AFishDropRulePopulator | Terraria.GameContent.FishDropRules/AFishDropRulePopulator.cs | D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\AFishDropRulePopulator.cs | 139 | 2 | Snow | Terraria.GameContent.FishDropRules.AFishingCondition | `protected AFishingCondition Snow = new DelegateFishingCondition((FishingContext context) => context.RolledSnow);` | `protected AFishingCondition Snow = new DelegateFishingCondition((FishingContext context) => context.RolledSnow);` |
| 1644 | field | Terraria.GameContent.FishDropRules.AFishDropRulePopulator | Terraria.GameContent.FishDropRules/AFishDropRulePopulator.cs | D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\AFishDropRulePopulator.cs | 141 | 2 | Desert | Terraria.GameContent.FishDropRules.AFishingCondition | `protected AFishingCondition Desert = new DelegateFishingCondition((FishingContext context) => context.RolledDesert);` | `protected AFishingCondition Desert = new DelegateFishingCondition((FishingContext context) => context.RolledDesert);` |
| 1645 | field | Terraria.GameContent.FishDropRules.AFishDropRulePopulator | Terraria.GameContent.FishDropRules/AFishDropRulePopulator.cs | D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\AFishDropRulePopulator.cs | 143 | 2 | RolledHallowDesert | Terraria.GameContent.FishDropRules.AFishingCondition | `protected AFishingCondition RolledHallowDesert = new DelegateFishingCondition((FishingContext context) => context.RolledInfectedDesert && context.Player.ZoneHallow);` | `protected AFishingCondition RolledHallowDesert = new DelegateFishingCondition((FishingContext context) => context.RolledInfectedDesert && context.Player.ZoneHallow);` |
| 1646 | field | Terraria.GameContent.FishDropRules.AFishDropRulePopulator | Terraria.GameContent.FishDropRules/AFishDropRulePopulator.cs | D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\AFishDropRulePopulator.cs | 145 | 2 | OriginalOcean | Terraria.GameContent.FishDropRules.AFishingCondition | `protected AFishingCondition OriginalOcean = new DelegateFishingCondition((FishingContext context) => IsOriginalOcean(context));` | `protected AFishingCondition OriginalOcean = new DelegateFishingCondition((FishingContext context) => IsOriginalOcean(context));` |
| 1647 | field | Terraria.GameContent.FishDropRules.AFishDropRulePopulator | Terraria.GameContent.FishDropRules/AFishDropRulePopulator.cs | D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\AFishDropRulePopulator.cs | 147 | 2 | RemixOcean | Terraria.GameContent.FishDropRules.AFishingCondition | `protected AFishingCondition RemixOcean = new DelegateFishingCondition((FishingContext context) => context.RolledRemixOcean);` | `protected AFishingCondition RemixOcean = new DelegateFishingCondition((FishingContext context) => context.RolledRemixOcean);` |
| 1648 | field | Terraria.GameContent.FishDropRules.AFishDropRulePopulator | Terraria.GameContent.FishDropRules/AFishDropRulePopulator.cs | D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\AFishDropRulePopulator.cs | 149 | 2 | Ocean | Terraria.GameContent.FishDropRules.AFishingCondition | `protected AFishingCondition Ocean = new DelegateFishingCondition((FishingContext context) => context.RolledRemixOcean \|\| IsOriginalOcean(context));` | `protected AFishingCondition Ocean = new DelegateFishingCondition((FishingContext context) => context.RolledRemixOcean \|\| IsOriginalOcean(context));` |
| 1649 | field | Terraria.GameContent.FishDropRules.AFishDropRulePopulator | Terraria.GameContent.FishDropRules/AFishDropRulePopulator.cs | D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\AFishDropRulePopulator.cs | 151 | 2 | Water1000 | Terraria.GameContent.FishDropRules.AFishingCondition | `protected AFishingCondition Water1000 = new DelegateFishingCondition((FishingContext context) => context.Fisher.waterTilesCount > 1000);` | `protected AFishingCondition Water1000 = new DelegateFishingCondition((FishingContext context) => context.Fisher.waterTilesCount > 1000);` |
| 1650 | field | Terraria.GameContent.FishDropRules.AFishDropRulePopulator | Terraria.GameContent.FishDropRules/AFishDropRulePopulator.cs | D:\TRbackup\Version4\Terraria.GameContent.FishDropRules\AFishDropRulePopulator.cs | 153 | 2 | BloodMoon | Terraria.GameContent.FishDropRules.AFishingCondition | `protected AFishingCondition BloodMoon = new DelegateFishingCondition((FishingContext context) => Main.bloodMoon);` | `protected AFishingCondition BloodMoon = new DelegateFishingCondition((FishingContext context) => Main.bloodMoon);` |

#### 属性（0）

无该类型成员记录。


### 4.22 细分子系统：`SharedDropRuleChanceAndQuantityState`

- 原报告章节：`4.9.208`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`10` / `经济、配方、钓鱼与掉落`

- 上一级基线细分子系统：`SharedDropRuleDefinitions`
- 上一级 peer 细分子系统：`SharedDropRuleSelectionAndQuantityState`
- 细分职责：掉落规则的概率、重掷、最小/最大数量和逐个掉落参数。
- 边界角色：`definition/query`；最小 seam：drop rule chance quantity port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：3；声明类型数：4；字段：16；属性：0；合计：16。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（16）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2039 | field | Terraria.GameContent.ItemDropRules.CommonDrop | Terraria.GameContent.ItemDropRules/CommonDrop.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\CommonDrop.cs | 7 | 2 | itemId | int | `public int itemId;` | `public int itemId;` |
| 2040 | field | Terraria.GameContent.ItemDropRules.CommonDrop | Terraria.GameContent.ItemDropRules/CommonDrop.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\CommonDrop.cs | 9 | 2 | chanceDenominator | int | `public int chanceDenominator;` | `public int chanceDenominator;` |
| 2041 | field | Terraria.GameContent.ItemDropRules.CommonDrop | Terraria.GameContent.ItemDropRules/CommonDrop.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\CommonDrop.cs | 11 | 2 | amountDroppedMinimum | int | `public int amountDroppedMinimum;` | `public int amountDroppedMinimum;` |
| 2042 | field | Terraria.GameContent.ItemDropRules.CommonDrop | Terraria.GameContent.ItemDropRules/CommonDrop.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\CommonDrop.cs | 13 | 2 | amountDroppedMaximum | int | `public int amountDroppedMaximum;` | `public int amountDroppedMaximum;` |
| 2043 | field | Terraria.GameContent.ItemDropRules.CommonDrop | Terraria.GameContent.ItemDropRules/CommonDrop.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\CommonDrop.cs | 15 | 2 | chanceNumerator | int | `public int chanceNumerator;` | `public int chanceNumerator;` |
| 2044 | field | Terraria.GameContent.ItemDropRules.CommonDropWithRerolls | Terraria.GameContent.ItemDropRules/CommonDropWithRerolls.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\CommonDropWithRerolls.cs | 7 | 2 | timesToRoll | int | `public int timesToRoll;` | `public int timesToRoll;` |
| 2065 | field | Terraria.GameContent.ItemDropRules.DropOneByOne.Parameters | Terraria.GameContent.ItemDropRules/DropOneByOne.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\DropOneByOne.cs | 9 | 3 | ChanceNumerator | int | `public int ChanceNumerator;` | `public int ChanceNumerator;` |
| 2066 | field | Terraria.GameContent.ItemDropRules.DropOneByOne.Parameters | Terraria.GameContent.ItemDropRules/DropOneByOne.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\DropOneByOne.cs | 11 | 3 | ChanceDenominator | int | `public int ChanceDenominator;` | `public int ChanceDenominator;` |
| 2067 | field | Terraria.GameContent.ItemDropRules.DropOneByOne.Parameters | Terraria.GameContent.ItemDropRules/DropOneByOne.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\DropOneByOne.cs | 13 | 3 | MinimumItemDropsCount | int | `public int MinimumItemDropsCount;` | `public int MinimumItemDropsCount;` |
| 2068 | field | Terraria.GameContent.ItemDropRules.DropOneByOne.Parameters | Terraria.GameContent.ItemDropRules/DropOneByOne.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\DropOneByOne.cs | 15 | 3 | MaximumItemDropsCount | int | `public int MaximumItemDropsCount;` | `public int MaximumItemDropsCount;` |
| 2069 | field | Terraria.GameContent.ItemDropRules.DropOneByOne.Parameters | Terraria.GameContent.ItemDropRules/DropOneByOne.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\DropOneByOne.cs | 17 | 3 | MinimumStackPerChunkBase | int | `public int MinimumStackPerChunkBase;` | `public int MinimumStackPerChunkBase;` |
| 2070 | field | Terraria.GameContent.ItemDropRules.DropOneByOne.Parameters | Terraria.GameContent.ItemDropRules/DropOneByOne.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\DropOneByOne.cs | 19 | 3 | MaximumStackPerChunkBase | int | `public int MaximumStackPerChunkBase;` | `public int MaximumStackPerChunkBase;` |
| 2071 | field | Terraria.GameContent.ItemDropRules.DropOneByOne.Parameters | Terraria.GameContent.ItemDropRules/DropOneByOne.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\DropOneByOne.cs | 21 | 3 | BonusMinDropsPerChunkPerPlayer | int | `public int BonusMinDropsPerChunkPerPlayer;` | `public int BonusMinDropsPerChunkPerPlayer;` |
| 2072 | field | Terraria.GameContent.ItemDropRules.DropOneByOne.Parameters | Terraria.GameContent.ItemDropRules/DropOneByOne.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\DropOneByOne.cs | 23 | 3 | BonusMaxDropsPerChunkPerPlayer | int | `public int BonusMaxDropsPerChunkPerPlayer;` | `public int BonusMaxDropsPerChunkPerPlayer;` |
| 2073 | field | Terraria.GameContent.ItemDropRules.DropOneByOne | Terraria.GameContent.ItemDropRules/DropOneByOne.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\DropOneByOne.cs | 33 | 2 | itemId | int | `public int itemId;` | `public int itemId;` |
| 2074 | field | Terraria.GameContent.ItemDropRules.DropOneByOne | Terraria.GameContent.ItemDropRules/DropOneByOne.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\DropOneByOne.cs | 35 | 2 | parameters | Terraria.GameContent.ItemDropRules.DropOneByOne.Parameters | `public Parameters parameters;` | `public Parameters parameters;` |

#### 属性（0）

无该类型成员记录。


### 4.23 细分子系统：`SharedDropRuleOptionSelectionState`

- 原报告章节：`4.9.209`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`10` / `经济、配方、钓鱼与掉落`

- 上一级基线细分子系统：`SharedDropRuleDefinitions`
- 上一级 peer 细分子系统：`SharedDropRuleSelectionAndQuantityState`
- 细分职责：按模式、选项集合和候选规则选择掉落项的状态。
- 边界角色：`definition/query`；最小 seam：drop rule option selection port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：8；声明类型数：8；字段：20；属性：0；合计：20。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（20）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2055 | field | Terraria.GameContent.ItemDropRules.DropBasedOnExpertMode | Terraria.GameContent.ItemDropRules/DropBasedOnExpertMode.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\DropBasedOnExpertMode.cs | 7 | 2 | ruleForNormalMode | Terraria.GameContent.ItemDropRules.IItemDropRule | `public IItemDropRule ruleForNormalMode;` | `public IItemDropRule ruleForNormalMode;` |
| 2056 | field | Terraria.GameContent.ItemDropRules.DropBasedOnExpertMode | Terraria.GameContent.ItemDropRules/DropBasedOnExpertMode.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\DropBasedOnExpertMode.cs | 9 | 2 | ruleForExpertMode | Terraria.GameContent.ItemDropRules.IItemDropRule | `public IItemDropRule ruleForExpertMode;` | `public IItemDropRule ruleForExpertMode;` |
| 2057 | field | Terraria.GameContent.ItemDropRules.DropBasedOnExtraGel | Terraria.GameContent.ItemDropRules/DropBasedOnExtraGel.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\DropBasedOnExtraGel.cs | 7 | 2 | ruleForNormal | Terraria.GameContent.ItemDropRules.IItemDropRule | `public IItemDropRule ruleForNormal;` | `public IItemDropRule ruleForNormal;` |
| 2058 | field | Terraria.GameContent.ItemDropRules.DropBasedOnExtraGel | Terraria.GameContent.ItemDropRules/DropBasedOnExtraGel.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\DropBasedOnExtraGel.cs | 9 | 2 | ruleForExtraGel | Terraria.GameContent.ItemDropRules.IItemDropRule | `public IItemDropRule ruleForExtraGel;` | `public IItemDropRule ruleForExtraGel;` |
| 2059 | field | Terraria.GameContent.ItemDropRules.DropBasedOnMasterAndExpertMode | Terraria.GameContent.ItemDropRules/DropBasedOnMasterAndExpertMode.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\DropBasedOnMasterAndExpertMode.cs | 7 | 2 | ruleForDefault | Terraria.GameContent.ItemDropRules.IItemDropRule | `public IItemDropRule ruleForDefault;` | `public IItemDropRule ruleForDefault;` |
| 2060 | field | Terraria.GameContent.ItemDropRules.DropBasedOnMasterAndExpertMode | Terraria.GameContent.ItemDropRules/DropBasedOnMasterAndExpertMode.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\DropBasedOnMasterAndExpertMode.cs | 9 | 2 | ruleForExpertmode | Terraria.GameContent.ItemDropRules.IItemDropRule | `public IItemDropRule ruleForExpertmode;` | `public IItemDropRule ruleForExpertmode;` |
| 2061 | field | Terraria.GameContent.ItemDropRules.DropBasedOnMasterAndExpertMode | Terraria.GameContent.ItemDropRules/DropBasedOnMasterAndExpertMode.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\DropBasedOnMasterAndExpertMode.cs | 11 | 2 | ruleForMasterMode | Terraria.GameContent.ItemDropRules.IItemDropRule | `public IItemDropRule ruleForMasterMode;` | `public IItemDropRule ruleForMasterMode;` |
| 2062 | field | Terraria.GameContent.ItemDropRules.DropBasedOnMasterMode | Terraria.GameContent.ItemDropRules/DropBasedOnMasterMode.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\DropBasedOnMasterMode.cs | 7 | 2 | ruleForDefault | Terraria.GameContent.ItemDropRules.IItemDropRule | `public IItemDropRule ruleForDefault;` | `public IItemDropRule ruleForDefault;` |
| 2063 | field | Terraria.GameContent.ItemDropRules.DropBasedOnMasterMode | Terraria.GameContent.ItemDropRules/DropBasedOnMasterMode.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\DropBasedOnMasterMode.cs | 9 | 2 | ruleForMasterMode | Terraria.GameContent.ItemDropRules.IItemDropRule | `public IItemDropRule ruleForMasterMode;` | `public IItemDropRule ruleForMasterMode;` |
| 2083 | field | Terraria.GameContent.ItemDropRules.FromOptionsWithoutRepeatsDropRule | Terraria.GameContent.ItemDropRules/FromOptionsWithoutRepeatsDropRule.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\FromOptionsWithoutRepeatsDropRule.cs | 7 | 2 | dropIds | int[] | `public int[] dropIds;` | `public int[] dropIds;` |
| 2084 | field | Terraria.GameContent.ItemDropRules.FromOptionsWithoutRepeatsDropRule | Terraria.GameContent.ItemDropRules/FromOptionsWithoutRepeatsDropRule.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\FromOptionsWithoutRepeatsDropRule.cs | 9 | 2 | dropCount | int | `public int dropCount;` | `public int dropCount;` |
| 2085 | field | Terraria.GameContent.ItemDropRules.FromOptionsWithoutRepeatsDropRule | Terraria.GameContent.ItemDropRules/FromOptionsWithoutRepeatsDropRule.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\FromOptionsWithoutRepeatsDropRule.cs | 11 | 2 | _temporaryAvailableItems | System.Collections.Generic.List<int> | `private List<int> _temporaryAvailableItems = new List<int>();` | `private List<int> _temporaryAvailableItems = new List<int>();` |
| 2095 | field | Terraria.GameContent.ItemDropRules.OneFromOptionsDropRule | Terraria.GameContent.ItemDropRules/OneFromOptionsDropRule.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\OneFromOptionsDropRule.cs | 7 | 2 | dropIds | int[] | `public int[] dropIds;` | `public int[] dropIds;` |
| 2096 | field | Terraria.GameContent.ItemDropRules.OneFromOptionsDropRule | Terraria.GameContent.ItemDropRules/OneFromOptionsDropRule.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\OneFromOptionsDropRule.cs | 9 | 2 | chanceDenominator | int | `public int chanceDenominator;` | `public int chanceDenominator;` |
| 2097 | field | Terraria.GameContent.ItemDropRules.OneFromOptionsDropRule | Terraria.GameContent.ItemDropRules/OneFromOptionsDropRule.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\OneFromOptionsDropRule.cs | 11 | 2 | chanceNumerator | int | `public int chanceNumerator;` | `public int chanceNumerator;` |
| 2098 | field | Terraria.GameContent.ItemDropRules.OneFromOptionsNotScaledWithLuckDropRule | Terraria.GameContent.ItemDropRules/OneFromOptionsNotScaledWithLuckDropRule.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\OneFromOptionsNotScaledWithLuckDropRule.cs | 7 | 2 | dropIds | int[] | `public int[] dropIds;` | `public int[] dropIds;` |
| 2099 | field | Terraria.GameContent.ItemDropRules.OneFromOptionsNotScaledWithLuckDropRule | Terraria.GameContent.ItemDropRules/OneFromOptionsNotScaledWithLuckDropRule.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\OneFromOptionsNotScaledWithLuckDropRule.cs | 9 | 2 | chanceDenominator | int | `public int chanceDenominator;` | `public int chanceDenominator;` |
| 2100 | field | Terraria.GameContent.ItemDropRules.OneFromOptionsNotScaledWithLuckDropRule | Terraria.GameContent.ItemDropRules/OneFromOptionsNotScaledWithLuckDropRule.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\OneFromOptionsNotScaledWithLuckDropRule.cs | 11 | 2 | chanceNumerator | int | `public int chanceNumerator;` | `public int chanceNumerator;` |
| 2101 | field | Terraria.GameContent.ItemDropRules.OneFromRulesRule | Terraria.GameContent.ItemDropRules/OneFromRulesRule.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\OneFromRulesRule.cs | 7 | 2 | options | Terraria.GameContent.ItemDropRules.IItemDropRule[] | `public IItemDropRule[] options;` | `public IItemDropRule[] options;` |
| 2102 | field | Terraria.GameContent.ItemDropRules.OneFromRulesRule | Terraria.GameContent.ItemDropRules/OneFromRulesRule.cs | D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules\OneFromRulesRule.cs | 9 | 2 | chanceDenominator | int | `public int chanceDenominator;` | `public int chanceDenominator;` |

#### 属性（0）

无该类型成员记录。


### 4.24 细分子系统：`SharedItemUseTimingAndConsumptionState`

- 原报告章节：`4.9.214`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`10` / `经济、配方、钓鱼与掉落`

- 上一级基线细分子系统：`SharedItemUseToolAndCombatState`
- 上一级 peer 细分子系统：`SharedItemUseAndToolCapabilityState`
- 细分职责：物品使用样式、时序、消耗、复用和使用表现能力。
- 边界角色：`definition/state`；最小 seam：item use timing consumption port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：15；属性：0；合计：15。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（15）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3191 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 126 | 2 | holdStyle | int | `public int holdStyle;` | `public int holdStyle;` |
| 3192 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 128 | 2 | useStyle | int | `public int useStyle;` | `public int useStyle;` |
| 3193 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 130 | 2 | channel | bool | `public bool channel;` | `public bool channel;` |
| 3194 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 132 | 2 | accessory | bool | `public bool accessory;` | `public bool accessory;` |
| 3195 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 134 | 2 | useAnimation | int | `public int useAnimation;` | `public int useAnimation;` |
| 3196 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 136 | 2 | useTime | int | `public int useTime;` | `public int useTime;` |
| 3210 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 164 | 2 | potion | bool | `public bool potion;` | `public bool potion;` |
| 3211 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 166 | 2 | consumable | bool | `public bool consumable;` | `public bool consumable;` |
| 3212 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 168 | 2 | autoReuse | bool | `public bool autoReuse;` | `public bool autoReuse;` |
| 3213 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 170 | 2 | useTurn | bool | `public bool useTurn;` | `public bool useTurn;` |
| 3250 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 244 | 2 | noUseGraphic | bool | `public bool noUseGraphic;` | `public bool noUseGraphic;` |
| 3251 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 246 | 2 | noMelee | bool | `public bool noMelee;` | `public bool noMelee;` |
| 3257 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 258 | 2 | noWet | bool | `public bool noWet;` | `public bool noWet;` |
| 3265 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 274 | 2 | shootsEveryUse | bool | `public bool shootsEveryUse;` | `public bool shootsEveryUse;` |
| 3277 | field | Terraria.Item | Terraria/Item.cs | D:\TRbackup\Version4\Terraria\Item.cs | 298 | 2 | reuseDelay | int | `public int reuseDelay;` | `public int reuseDelay;` |

#### 属性（0）

无该类型成员记录。


## 5. 追溯与验收

> 来源报告 SHA-256：`b944441d92d5103f218b766a3e354200794528a19e65a862e44c2b40a36f1196`
- 本分区细分组数：24；成员数：321；字段：296；属性：25。
- 预期不变量：本文件只出现完整细分子系统；不跨分区复制成员；所有成员的来源序号、原始声明、路径、行列和 C# 类型必须与来源报告一致。
- 验证方式：重新读取 20 个文件，检查文件数、细分组唯一性、来源序号恰好覆盖 `1..4542`、字段/属性合计和逐成员行文本一致；本次分区生成不运行 `dotnet`，因为没有修改 C# 或项目文件。

生成器：`Build/Tools/Generate-Version4NonAuthoritativeComponentPartitionReports.ps1`。
