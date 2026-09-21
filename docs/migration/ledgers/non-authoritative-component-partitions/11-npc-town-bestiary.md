# Version4 非权威组件拆分分区 11/20：NPC、城镇与图鉴

> 来源报告：`docs/migration/ledgers/Version4非权威模拟系统字段属性逐成员源码声明-更细子系统拆分-去除ID类文件.md`
> 来源报告 SHA-256：`b944441d92d5103f218b766a3e354200794528a19e65a862e44c2b40a36f1196`
> 本文件是并行组件拆分工作包，不是新的权威成员清单；成员声明事实直接保留来源报告原文。

## 1. 分区定位与评估

- 分区范围：NPC 个性、城镇房间、条件对话、图鉴条目、解锁和筛选。
- 本分区组件化重点：确认 NPC/图鉴状态与纯筛选查询、文本投影及生命周期。
- 本分区包含 8 个完整细分子系统、93 条成员记录（字段 71、属性 22）。来源序号覆盖区间 `1357..3849`，区间可能与其他分区交错，不以序号定义领域边界。
- 组件边界判定：细分子系统只是并行处理的最小库存边界，不等价于一个组件。必须继续按共同读者/写者、变更原因、生命周期、权威性和副作用分为 Component、System、Query、Command、Adapter 或 Projection。
- 证据状态：成员声明和统计已由来源报告校验；逐成员读者、写者、初始化/清理路径、持久化/网络语义和系统顺序仍需本分区会话补证，不得从名称或数量推断。

## 2. 分区统计

| 父级子系统 | 细分子系统数 | 字段 | 属性 | 合计 |
|---|---:|---:|---:|---:|
| `SharedRuntimeMechanisms` | 8 | 71 | 22 | 93 |

| 原报告章节 | 父级 | 细分子系统 | 边界角色 | 字段 | 属性 | 合计 | 组件化处理 |
|---|---|---|---|---:|---:|---:|---|
| `4.9.27` | `SharedRuntimeMechanisms` | `SharedConditionalDialogueSupport` | definition/query | 8 | 1 | 9 | 待按成员访问模式拆分 |
| `4.9.28` | `SharedRuntimeMechanisms` | `SharedTownRoomState` | state | 3 | 0 | 3 | 待按成员访问模式拆分 |
| `4.9.52` | `SharedRuntimeMechanisms` | `SharedNpcPersonalityCatalog` | definition/catalog | 18 | 1 | 19 | 待按成员访问模式拆分 |
| `4.9.64` | `SharedRuntimeMechanisms` | `BestiaryCatalogAndEntries` | definition/catalog | 6 | 2 | 8 | 待按成员访问模式拆分 |
| `4.9.65` | `SharedRuntimeMechanisms` | `BestiaryUnlockTracking` | state/query | 14 | 1 | 15 | 待按成员访问模式拆分 |
| `4.9.66` | `SharedRuntimeMechanisms` | `BestiaryFiltersAndSorting` | query | 1 | 11 | 12 | 待按成员访问模式拆分 |
| `4.9.192` | `SharedRuntimeMechanisms` | `SharedBestiaryInfoElementState` | definition/presentation | 14 | 3 | 17 | 待按成员访问模式拆分 |
| `4.9.193` | `SharedRuntimeMechanisms` | `SharedBestiaryCollectionProviderState` | definition/presentation | 7 | 3 | 10 | 待按成员访问模式拆分 |

## 3. 并行组件拆分契约

对本文件中的每个细分子系统和每条成员记录执行以下判断，并把结果写入对应的实现阶段设计/审计记录：

- 盘点声明类型、可见性、读者、写者、创建/更新/清理/持久化生命周期、访问频率和副作用；缺证据标记 `missing`，不可用命名猜测补齐。
- 标记权威状态、派生值、缓存、快照、兼容字段和表现数据；Component 只保存一个内聚概念的权威数据。
- 需要行为转换时放入显式 System/Command；纯资格判断和派生计算放入 Query；外部协议、文件、平台和第三方类型放入 Adapter；网络/持久化/日志/UI 输出放入单向 Projection。
- 分别建模实体引用、持久化 ID、网络 ID 和外部 ID；不得把 ID 类文件或外部对象直接塞入共享组件。
- 记录 Interface、Implementation、Seam、Depth、Leverage、Locality，以及必要的系统先后约束、失败归属和重试策略。

## 4. 逐成员源码声明

以下细分子系统块保持来源报告的成员表、声明、路径、行列和来源序号；只调整 Markdown 标题层级并增加分区追踪元数据。

### 4.1 细分子系统：`SharedConditionalDialogueSupport`

- 原报告章节：`4.9.27`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`11` / `NPC、城镇与图鉴`

- 上一级基线细分子系统：`SharedConditionalDialogueSupport`
- 细分职责：条件对话和 Lucy 交互消息。
- 边界角色：`definition/query`；最小 seam：conditional dialogue view；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：2；声明类型数：3；字段：8；属性：1；合计：9。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（8）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2352 | field | Terraria.GameContent.ConditionalDialogue.ItemGroups | Terraria.GameContent/ConditionalDialogue.cs | D:\TRbackup\Version4\Terraria.GameContent\ConditionalDialogue.cs | 13 | 3 | Ore | Terraria.RecipeGroup | `public static RecipeGroup Ore = new RecipeGroup("RecipeGroups.Ore", 699, 12, 11, 700, 14, 701, 13, 702);` | `public static RecipeGroup Ore = new RecipeGroup("RecipeGroups.Ore", 699, 12, 11, 700, 14, 701, 13, 702);` |
| 2353 | field | Terraria.GameContent.ConditionalDialogue.ItemGroups | Terraria.GameContent/ConditionalDialogue.cs | D:\TRbackup\Version4\Terraria.GameContent\ConditionalDialogue.cs | 15 | 3 | Bars | Terraria.RecipeGroup | `public static RecipeGroup Bars = new RecipeGroup("RecipeGroups.Bar", 703, 20, 22, 704, 21, 705, 19, 706);` | `public static RecipeGroup Bars = new RecipeGroup("RecipeGroups.Bar", 703, 20, 22, 704, 21, 705, 19, 706);` |
| 2354 | field | Terraria.GameContent.ConditionalDialogue.ItemGroups | Terraria.GameContent/ConditionalDialogue.cs | D:\TRbackup\Version4\Terraria.GameContent\ConditionalDialogue.cs | 17 | 3 | Anvils | Terraria.RecipeGroup | `public static RecipeGroup Anvils = new RecipeGroup("ItemName.IronAnvil", 35, 716);` | `public static RecipeGroup Anvils = new RecipeGroup("ItemName.IronAnvil", 35, 716);` |
| 2355 | field | Terraria.GameContent.ConditionalDialogue.ItemGroups | Terraria.GameContent/ConditionalDialogue.cs | D:\TRbackup\Version4\Terraria.GameContent\ConditionalDialogue.cs | 19 | 3 | Whips | Terraria.RecipeGroup | `public static RecipeGroup Whips = new RecipeGroup("RecipeGroups.Whip");` | `public static RecipeGroup Whips = new RecipeGroup("RecipeGroups.Whip");` |
| 2356 | field | Terraria.GameContent.ConditionalDialogue.ItemGroups | Terraria.GameContent/ConditionalDialogue.cs | D:\TRbackup\Version4\Terraria.GameContent\ConditionalDialogue.cs | 21 | 3 | Mounts | Terraria.RecipeGroup | `public static RecipeGroup Mounts = new RecipeGroup("RecipeGroups.Mount");` | `public static RecipeGroup Mounts = new RecipeGroup("RecipeGroups.Mount");` |
| 2357 | field | Terraria.GameContent.ConditionalDialogue | Terraria.GameContent/ConditionalDialogue.cs | D:\TRbackup\Version4\Terraria.GameContent\ConditionalDialogue.cs | 55 | 2 | _registry | System.Collections.Generic.List<Terraria.GameContent.ConditionalDialogue>[] | `private static List<ConditionalDialogue>[] _registry = new List<ConditionalDialogue>[NPCID.Count];` | `private static List<ConditionalDialogue>[] _registry = new List<ConditionalDialogue>[NPCID.Count];` |
| 2358 | field | Terraria.GameContent.ConditionalDialogue | Terraria.GameContent/ConditionalDialogue.cs | D:\TRbackup\Version4\Terraria.GameContent\ConditionalDialogue.cs | 57 | 2 | ConditionsMet | System.Predicate<Terraria.NPC> | `public readonly Predicate<NPC> ConditionsMet;` | `public readonly Predicate<NPC> ConditionsMet;` |
| 2456 | field | Terraria.GameContent.LucyAxeMessage | Terraria.GameContent/LucyAxeMessage.cs | D:\TRbackup\Version4\Terraria.GameContent\LucyAxeMessage.cs | 24 | 2 | _messageCooldownsByType | int[] | `private static int[] _messageCooldownsByType = new int[7];` | `private static int[] _messageCooldownsByType = new int[7];` |

#### 属性（1）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3849 | property | Terraria.GameContent.ConditionalDialogue | Terraria.GameContent/ConditionalDialogue.cs | D:\TRbackup\Version4\Terraria.GameContent\ConditionalDialogue.cs | 59 | 2 | ShowIndicator | bool | `public bool ShowIndicator { get; private set; }` | `public bool ShowIndicator { get; private set; }` |


### 4.2 细分子系统：`SharedTownRoomState`

- 原报告章节：`4.9.28`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`11` / `NPC、城镇与图鉴`

- 上一级基线细分子系统：`SharedTownRoomState`
- 细分职责：城镇房间和 NPC 房屋状态。
- 边界角色：`state`；最小 seam：town room registry；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：3；属性：0；合计：3。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（3）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2574 | field | Terraria.GameContent.TownRoomManager | Terraria.GameContent/TownRoomManager.cs | D:\TRbackup\Version4\Terraria.GameContent\TownRoomManager.cs | 11 | 2 | EntityCreationLock | object | `public static object EntityCreationLock = new object();` | `public static object EntityCreationLock = new object();` |
| 2575 | field | Terraria.GameContent.TownRoomManager | Terraria.GameContent/TownRoomManager.cs | D:\TRbackup\Version4\Terraria.GameContent\TownRoomManager.cs | 13 | 2 | _roomLocationPairs | System.Collections.Generic.List<System.Tuple<int, Point>> | `private List<Tuple<int, Point>> _roomLocationPairs = new List<Tuple<int, Point>>();` | `private List<Tuple<int, Point>> _roomLocationPairs = new List<Tuple<int, Point>>();` |
| 2576 | field | Terraria.GameContent.TownRoomManager | Terraria.GameContent/TownRoomManager.cs | D:\TRbackup\Version4\Terraria.GameContent\TownRoomManager.cs | 15 | 2 | _hasRoom | bool[] | `private bool[] _hasRoom = new bool[NPCID.Count];` | `private bool[] _hasRoom = new bool[NPCID.Count];` |

#### 属性（0）

无该类型成员记录。


### 4.3 细分子系统：`SharedNpcPersonalityCatalog`

- 原报告章节：`4.9.52`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`11` / `NPC、城镇与图鉴`

- 上一级基线细分子系统：`SharedNpcPersonalityCatalog`
- 细分职责：NPC 个性偏好和城镇档案。
- 边界角色：`definition/catalog`；最小 seam：personality catalog view；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：7；声明类型数：8；字段：18；属性：1；合计：19。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（18）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2161 | field | Terraria.GameContent.Personalities.BiomePreferenceListTrait.BiomePreference | Terraria.GameContent.Personalities/BiomePreferenceListTrait.cs | D:\TRbackup\Version4\Terraria.GameContent.Personalities\BiomePreferenceListTrait.cs | 10 | 3 | Affection | Terraria.GameContent.Personalities.AffectionLevel | `public AffectionLevel Affection;` | `public AffectionLevel Affection;` |
| 2162 | field | Terraria.GameContent.Personalities.BiomePreferenceListTrait.BiomePreference | Terraria.GameContent.Personalities/BiomePreferenceListTrait.cs | D:\TRbackup\Version4\Terraria.GameContent.Personalities\BiomePreferenceListTrait.cs | 12 | 3 | Biome | Terraria.GameContent.Personalities.AShoppingBiome | `public AShoppingBiome Biome;` | `public AShoppingBiome Biome;` |
| 2163 | field | Terraria.GameContent.Personalities.BiomePreferenceListTrait | Terraria.GameContent.Personalities/BiomePreferenceListTrait.cs | D:\TRbackup\Version4\Terraria.GameContent.Personalities\BiomePreferenceListTrait.cs | 21 | 2 | _preferences | System.Collections.Generic.List<Terraria.GameContent.Personalities.BiomePreferenceListTrait.BiomePreference> | `private List<BiomePreference> _preferences;` | `private List<BiomePreference> _preferences;` |
| 2164 | field | Terraria.GameContent.Personalities.HelperInfo | Terraria.GameContent.Personalities/HelperInfo.cs | D:\TRbackup\Version4\Terraria.GameContent.Personalities\HelperInfo.cs | 7 | 2 | player | Terraria.Player | `public Player player;` | `public Player player;` |
| 2165 | field | Terraria.GameContent.Personalities.HelperInfo | Terraria.GameContent.Personalities/HelperInfo.cs | D:\TRbackup\Version4\Terraria.GameContent.Personalities\HelperInfo.cs | 9 | 2 | npc | Terraria.NPC | `public NPC npc;` | `public NPC npc;` |
| 2166 | field | Terraria.GameContent.Personalities.HelperInfo | Terraria.GameContent.Personalities/HelperInfo.cs | D:\TRbackup\Version4\Terraria.GameContent.Personalities\HelperInfo.cs | 11 | 2 | NearbyNPCs | System.Collections.Generic.List<Terraria.NPC> | `public List<NPC> NearbyNPCs;` | `public List<NPC> NearbyNPCs;` |
| 2167 | field | Terraria.GameContent.Personalities.HelperInfo | Terraria.GameContent.Personalities/HelperInfo.cs | D:\TRbackup\Version4\Terraria.GameContent.Personalities\HelperInfo.cs | 13 | 2 | nearbyNPCsByType | bool[] | `public bool[] nearbyNPCsByType;` | `public bool[] nearbyNPCsByType;` |
| 2168 | field | Terraria.GameContent.Personalities.PersonalityDatabase | Terraria.GameContent.Personalities/PersonalityDatabase.cs | D:\TRbackup\Version4\Terraria.GameContent.Personalities\PersonalityDatabase.cs | 7 | 2 | _personalityProfiles | System.Collections.Generic.Dictionary<int, Terraria.GameContent.Personalities.PersonalityProfile> | `private Dictionary<int, PersonalityProfile> _personalityProfiles;` | `private Dictionary<int, PersonalityProfile> _personalityProfiles;` |
| 2169 | field | Terraria.GameContent.Personalities.PersonalityDatabase | Terraria.GameContent.Personalities/PersonalityDatabase.cs | D:\TRbackup\Version4\Terraria.GameContent.Personalities\PersonalityDatabase.cs | 9 | 2 | _trashEntry | Terraria.GameContent.Personalities.PersonalityProfile | `private PersonalityProfile _trashEntry = new PersonalityProfile();` | `private PersonalityProfile _trashEntry = new PersonalityProfile();` |
| 2170 | field | Terraria.GameContent.Personalities.PersonalityDatabasePopulator | Terraria.GameContent.Personalities/PersonalityDatabasePopulator.cs | D:\TRbackup\Version4\Terraria.GameContent.Personalities\PersonalityDatabasePopulator.cs | 5 | 2 | _currentDatabase | Terraria.GameContent.Personalities.PersonalityDatabase | `private PersonalityDatabase _currentDatabase;` | `private PersonalityDatabase _currentDatabase;` |
| 2171 | field | Terraria.GameContent.Personalities.PersonalityProfile | Terraria.GameContent.Personalities/PersonalityProfile.cs | D:\TRbackup\Version4\Terraria.GameContent.Personalities\PersonalityProfile.cs | 7 | 2 | ShopModifiers | System.Collections.Generic.List<Terraria.GameContent.Personalities.IShopPersonalityTrait> | `public List<IShopPersonalityTrait> ShopModifiers = new List<IShopPersonalityTrait>();` | `public List<IShopPersonalityTrait> ShopModifiers = new List<IShopPersonalityTrait>();` |
| 2567 | field | Terraria.GameContent.TownNPCProfiles | Terraria.GameContent/TownNPCProfiles.cs | D:\TRbackup\Version4\Terraria.GameContent\TownNPCProfiles.cs | 7 | 2 | DefaultNPCFileFolderPath | string | `private const string DefaultNPCFileFolderPath = "Images/TownNPCs/";` | `private const string DefaultNPCFileFolderPath = "Images/TownNPCs/";` |
| 2568 | field | Terraria.GameContent.TownNPCProfiles | Terraria.GameContent/TownNPCProfiles.cs | D:\TRbackup\Version4\Terraria.GameContent\TownNPCProfiles.cs | 9 | 2 | ShimmeredNPCFileFolderPath | string | `private const string ShimmeredNPCFileFolderPath = "Images/TownNPCs/Shimmered/";` | `private const string ShimmeredNPCFileFolderPath = "Images/TownNPCs/Shimmered/";` |
| 2569 | field | Terraria.GameContent.TownNPCProfiles | Terraria.GameContent/TownNPCProfiles.cs | D:\TRbackup\Version4\Terraria.GameContent\TownNPCProfiles.cs | 11 | 2 | CatHeadIDs | int[] | `private static readonly int[] CatHeadIDs = new int[6] { 27, 28, 29, 30, 31, 32 };` | `private static readonly int[] CatHeadIDs = new int[6] { 27, 28, 29, 30, 31, 32 };` |
| 2570 | field | Terraria.GameContent.TownNPCProfiles | Terraria.GameContent/TownNPCProfiles.cs | D:\TRbackup\Version4\Terraria.GameContent\TownNPCProfiles.cs | 13 | 2 | DogHeadIDs | int[] | `private static readonly int[] DogHeadIDs = new int[6] { 33, 34, 35, 36, 37, 38 };` | `private static readonly int[] DogHeadIDs = new int[6] { 33, 34, 35, 36, 37, 38 };` |
| 2571 | field | Terraria.GameContent.TownNPCProfiles | Terraria.GameContent/TownNPCProfiles.cs | D:\TRbackup\Version4\Terraria.GameContent\TownNPCProfiles.cs | 15 | 2 | BunnyHeadIDs | int[] | `private static readonly int[] BunnyHeadIDs = new int[6] { 39, 40, 41, 42, 43, 44 };` | `private static readonly int[] BunnyHeadIDs = new int[6] { 39, 40, 41, 42, 43, 44 };` |
| 2572 | field | Terraria.GameContent.TownNPCProfiles | Terraria.GameContent/TownNPCProfiles.cs | D:\TRbackup\Version4\Terraria.GameContent\TownNPCProfiles.cs | 17 | 2 | _townNPCProfiles | System.Collections.Generic.Dictionary<int, Terraria.GameContent.ITownNPCProfile> | `private Dictionary<int, ITownNPCProfile> _townNPCProfiles = new Dictionary<int, ITownNPCProfile>  	{  		{  			22,  			LegacyWithSimpleShimmer("Guide", 1, 72, uniquePartyTexture: false, uniquePartyTextureShimmered: false)  		},  		{  			20,  			LegacyWithSimpleShimmer("Dryad", 5, 73, uniquePartyTexture: false, uniquePartyTextureShimmered: false)  		},  		{  			19,  			LegacyWithSimpleShimmer("ArmsDealer", 6, 74, uniquePartyTexture: false, uniquePartyTextureShimmered: false)  		},  		{  			107,  			LegacyWithSimpleShimmer("GoblinTinkerer", 9, 75, uniquePartyTexture: false, uniquePartyTextureShimmered: false)  		},  		{  			160,  			LegacyWithSimpleShimmer("Truffle", 12, 76, uniquePartyTexture: false, uniquePartyTextureShimmered: false)  		},  		{  			208,  			LegacyWithSimpleShimmer("PartyGirl", 15, 77, uniquePartyTexture: false, uniquePartyTextureShimmered: false)  		},  		{  			228,  			LegacyWithSimpleShimmer("WitchDoctor", 18, 78, uniquePartyTexture: false, uniquePartyTextureShimmered: false)  		},  		{  			550,  			LegacyWithSimpleShimmer("Tavernkeep", 24, 79, uniquePartyTexture: false, uniquePartyTextureShimmered: false)  		},  		{  			369,  			LegacyWithSimpleShimmer("Angler", 22, 55, uniquePartyTexture: true, uniquePartyTextureShimmered: false)  		},  		{  			54,  			LegacyWithSimpleShimmer("Clothier", 7, 57, uniquePartyTexture: true, uniquePartyTextureShimmered: false)  		},  		{  			209,  			LegacyWithSimpleShimmer("Cyborg", 16, 58)  		},  		{  			38,  			LegacyWithSimpleShimmer("Demolitionist", 4, 59)  		},  		{  			207,  			LegacyWithSimpleShimmer("DyeTrader", 14, 60)  		},  		{  			588,  			LegacyWithSimpleShimmer("Golfer", 25, 61, uniquePartyTexture: true, uniquePartyTextureShimmered: false)  		},  		{  			124,  			LegacyWithSimpleShimmer("Mechanic", 8, 62)  		},  		{  			17,  			LegacyWithSimpleShimmer("Merchant", 2, 63)  		},  		{  			18,  			LegacyWithSimpleShimmer("Nurse", 3, 64)  		},  		{  			227,  			LegacyWithSimpleShimmer("Painter", 17, 65, uniquePartyTexture: true, uniquePartyTextureShimmered: false)  		},  		{  			229,  			LegacyWithSimpleShimmer("Pirate", 19, 66)  		},  		{  			142,  			LegacyWithSimpleShimmer("Santa", 11, 67)  		},  		{  			178,  			LegacyWithSimpleShimmer("Steampunker", 13, 68, uniquePartyTexture: true, uniquePartyTextureShimmered: false)  		},  		{  			353,  			LegacyWithSimpleShimmer("Stylist", 20, 69)  		},  		{  			441,  			LegacyWithSimpleShimmer("TaxCollector", 23, 70)  		},  		{  			108,  			LegacyWithSimpleShimmer("Wizard", 10, 71)  		},  		{  			663,  			LegacyWithSimpleShimmer("Princess", 45, 54)  		},  		{  			633,  			TransformableWithSimpleShimmer("BestiaryGirl", 26, 56, uniqueCreditTexture: true, uniqueCreditTextureShimmered: false)  		},  		{  			37,  			LegacyWithSimpleShimmer("OldMan", -1, -1, uniquePartyTexture: false, uniquePartyTextureShimmered: false)  		},  		{  			453,  			LegacyWithSimpleShimmer("SkeletonMerchant", -1, -1)  		},  		{  			368,  			LegacyWithSimpleShimmer("TravelingMerchant", 21, 80)  		},  		{  			637,  			new Profiles.VariantNPCProfile("Images/TownNPCs/Cat", "Cat", CatHeadIDs, "Siamese", "Black", "OrangeTabby", "RussianBlue", "Silver", "White")  		},  		{  			638,  			new Profiles.VariantNPCProfile("Images/TownNPCs/Dog", "Dog", DogHeadIDs, "Labrador", "PitBull", "Beagle", "Corgi", "Dalmation", "Husky")  		},  		{  			656,  			new Profiles.VariantNPCProfile("Images/TownNPCs/Bunny", "Bunny", BunnyHeadIDs, "White", "Angora", "Dutch", "Flemish", "Lop", "Silver")  		},  		{  			670,  			new Profiles.LegacyNPCProfile("Images/TownNPCs/SlimeBlue", 46, includeDefault: true, uniquePartyTexture: false)  		},  		{  			678,  			new Profiles.LegacyNPCProfile("Images/TownNPCs/SlimeGreen", 47)  		},  		{  			679,  			new Profiles.LegacyNPCProfile("Images/TownNPCs/SlimeOld", 48)  		},  		{  			680,  			new Profiles.LegacyNPCProfile("Images/TownNPCs/SlimePurple", 49)  		},  		{  			681,  			new Profiles.LegacyNPCProfile("Images/TownNPCs/SlimeRainbow", 50)  		},  		{  			682,  			new Profiles.LegacyNPCProfile("Images/TownNPCs/SlimeRed", 51)  		},  		{  			683,  			new Profiles.LegacyNPCProfile("Images/TownNPCs/SlimeYellow", 52)  		},  		{  			684,  			new Profiles.LegacyNPCProfile("Images/TownNPCs/SlimeCopper", 53)  		}  	};` | `private Dictionary<int, ITownNPCProfile> _townNPCProfiles = new Dictionary<int, ITownNPCProfile> { { 22, LegacyWithSimpleShimmer("Guide", 1, 72, uniquePartyTexture: false, uniquePartyTextureShimmered: false) }, { 20, LegacyWithSimpleShimmer("Dryad", 5, 73, uniquePartyTexture: false, uniquePartyTextureShimmered: false) }, { 19, LegacyWithSimpleShimmer("ArmsDealer", 6, 74, uniquePartyTexture: false, uniquePartyTextureShimmered: false) }, { 107, LegacyWithSimpleShimmer("GoblinTinkerer", 9, 75, uniquePartyTexture: false, uniquePartyTextureShimmered: false) }, { 160, LegacyWithSimpleShimmer("Truffle", 12, 76, uniquePartyTexture: false, uniquePartyTextureShimmered: false) }, { 208, LegacyWithSimpleShimmer("PartyGirl", 15, 77, uniquePartyTexture: false, uniquePartyTextureShimmered: false) }, { 228, LegacyWithSimpleShimmer("WitchDoctor", 18, 78, uniquePartyTexture: false, uniquePartyTextureShimmered: false) }, { 550, LegacyWithSimpleShimmer("Tavernkeep", 24, 79, uniquePartyTexture: false, uniquePartyTextureShimmered: false) }, { 369, LegacyWithSimpleShimmer("Angler", 22, 55, uniquePartyTexture: true, uniquePartyTextureShimmered: false) }, { 54, LegacyWithSimpleShimmer("Clothier", 7, 57, uniquePartyTexture: true, uniquePartyTextureShimmered: false) }, { 209, LegacyWithSimpleShimmer("Cyborg", 16, 58) }, { 38, LegacyWithSimpleShimmer("Demolitionist", 4, 59) }, { 207, LegacyWithSimpleShimmer("DyeTrader", 14, 60) }, { 588, LegacyWithSimpleShimmer("Golfer", 25, 61, uniquePartyTexture: true, uniquePartyTextureShimmered: false) }, { 124, LegacyWithSimpleShimmer("Mechanic", 8, 62) }, { 17, LegacyWithSimpleShimmer("Merchant", 2, 63) }, { 18, LegacyWithSimpleShimmer("Nurse", 3, 64) }, { 227, LegacyWithSimpleShimmer("Painter", 17, 65, uniquePartyTexture: true, uniquePartyTextureShimmered: false) }, { 229, LegacyWithSimpleShimmer("Pirate", 19, 66) }, { 142, LegacyWithSimpleShimmer("Santa", 11, 67) }, { 178, LegacyWithSimpleShimmer("Steampunker", 13, 68, uniquePartyTexture: true, uniquePartyTextureShimmered: false) }, { 353, LegacyWithSimpleShimmer("Stylist", 20, 69) }, { 441, LegacyWithSimpleShimmer("TaxCollector", 23, 70) }, { 108, LegacyWithSimpleShimmer("Wizard", 10, 71) }, { 663, LegacyWithSimpleShimmer("Princess", 45, 54) }, { 633, TransformableWithSimpleShimmer("BestiaryGirl", 26, 56, uniqueCreditTexture: true, uniqueCreditTextureShimmered: false) }, { 37, LegacyWithSimpleShimmer("OldMan", -1, -1, uniquePartyTexture: false, uniquePartyTextureShimmered: false) }, { 453, LegacyWithSimpleShimmer("SkeletonMerchant", -1, -1) }, { 368, LegacyWithSimpleShimmer("TravelingMerchant", 21, 80) }, { 637, new Profiles.VariantNPCProfile("Images/TownNPCs/Cat", "Cat", CatHeadIDs, "Siamese", "Black", "OrangeTabby", "RussianBlue", "Silver", "White") }, { 638, new Profiles.VariantNPCProfile("Images/TownNPCs/Dog", "Dog", DogHeadIDs, "Labrador", "PitBull", "Beagle", "Corgi", "Dalmation", "Husky") }, { 656, new Profiles.VariantNPCProfile("Images/TownNPCs/Bunny", "Bunny", BunnyHeadIDs, "White", "Angora", "Dutch", "Flemish", "Lop", "Silver") }, { 670, new Profiles.LegacyNPCProfile("Images/TownNPCs/SlimeBlue", 46, includeDefault: true, uniquePartyTexture: false) }, { 678, new Profiles.LegacyNPCProfile("Images/TownNPCs/SlimeGreen", 47) }, { 679, new Profiles.LegacyNPCProfile("Images/TownNPCs/SlimeOld", 48) }, { 680, new Profiles.LegacyNPCProfile("Images/TownNPCs/SlimePurple", 49) }, { 681, new Profiles.LegacyNPCProfile("Images/TownNPCs/SlimeRainbow", 50) }, { 682, new Profiles.LegacyNPCProfile("Images/TownNPCs/SlimeRed", 51) }, { 683, new Profiles.LegacyNPCProfile("Images/TownNPCs/SlimeYellow", 52) }, { 684, new Profiles.LegacyNPCProfile("Images/TownNPCs/SlimeCopper", 53) } };` |
| 2573 | field | Terraria.GameContent.TownNPCProfiles | Terraria.GameContent/TownNPCProfiles.cs | D:\TRbackup\Version4\Terraria.GameContent\TownNPCProfiles.cs | 181 | 2 | Instance | Terraria.GameContent.TownNPCProfiles | `public static TownNPCProfiles Instance = new TownNPCProfiles();` | `public static TownNPCProfiles Instance = new TownNPCProfiles();` |

#### 属性（1）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3828 | property | Terraria.GameContent.Personalities.AShoppingBiome | Terraria.GameContent.Personalities/AShoppingBiome.cs | D:\TRbackup\Version4\Terraria.GameContent.Personalities\AShoppingBiome.cs | 5 | 2 | NameKey | string | `public string NameKey { get; protected set; }` | `public string NameKey { get; protected set; }` |


### 4.4 细分子系统：`BestiaryCatalogAndEntries`

- 原报告章节：`4.9.64`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`11` / `NPC、城镇与图鉴`

- 上一级基线细分子系统：`BestiaryCatalogAndEntries`
- 细分职责：图鉴数据库和图鉴条目目录。
- 边界角色：`definition/catalog`；最小 seam：bestiary catalog port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：2；声明类型数：2；字段：6；属性：2；合计：8。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（6）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1357 | field | Terraria.GameContent.Bestiary.BestiaryDatabase | Terraria.GameContent.Bestiary/BestiaryDatabase.cs | D:\TRbackup\Version4\Terraria.GameContent.Bestiary\BestiaryDatabase.cs | 11 | 2 | _entries | System.Collections.Generic.List<Terraria.GameContent.Bestiary.BestiaryEntry> | `private List<BestiaryEntry> _entries = new List<BestiaryEntry>();` | `private List<BestiaryEntry> _entries = new List<BestiaryEntry>();` |
| 1358 | field | Terraria.GameContent.Bestiary.BestiaryDatabase | Terraria.GameContent.Bestiary/BestiaryDatabase.cs | D:\TRbackup\Version4\Terraria.GameContent.Bestiary\BestiaryDatabase.cs | 13 | 2 | _filters | System.Collections.Generic.List<Terraria.GameContent.Bestiary.IBestiaryEntryFilter> | `private List<IBestiaryEntryFilter> _filters = new List<IBestiaryEntryFilter>();` | `private List<IBestiaryEntryFilter> _filters = new List<IBestiaryEntryFilter>();` |
| 1359 | field | Terraria.GameContent.Bestiary.BestiaryDatabase | Terraria.GameContent.Bestiary/BestiaryDatabase.cs | D:\TRbackup\Version4\Terraria.GameContent.Bestiary\BestiaryDatabase.cs | 15 | 2 | _sortSteps | System.Collections.Generic.List<Terraria.GameContent.Bestiary.IBestiarySortStep> | `private List<IBestiarySortStep> _sortSteps = new List<IBestiarySortStep>();` | `private List<IBestiarySortStep> _sortSteps = new List<IBestiarySortStep>();` |
| 1360 | field | Terraria.GameContent.Bestiary.BestiaryDatabase | Terraria.GameContent.Bestiary/BestiaryDatabase.cs | D:\TRbackup\Version4\Terraria.GameContent.Bestiary\BestiaryDatabase.cs | 17 | 2 | _byNpcId | System.Collections.Generic.Dictionary<int, Terraria.GameContent.Bestiary.BestiaryEntry> | `private Dictionary<int, BestiaryEntry> _byNpcId = new Dictionary<int, BestiaryEntry>();` | `private Dictionary<int, BestiaryEntry> _byNpcId = new Dictionary<int, BestiaryEntry>();` |
| 1361 | field | Terraria.GameContent.Bestiary.BestiaryDatabase | Terraria.GameContent.Bestiary/BestiaryDatabase.cs | D:\TRbackup\Version4\Terraria.GameContent.Bestiary\BestiaryDatabase.cs | 19 | 2 | _trashEntry | Terraria.GameContent.Bestiary.BestiaryEntry | `private BestiaryEntry _trashEntry = new BestiaryEntry();` | `private BestiaryEntry _trashEntry = new BestiaryEntry();` |
| 1362 | field | Terraria.GameContent.Bestiary.BestiaryEntry | Terraria.GameContent.Bestiary/BestiaryEntry.cs | D:\TRbackup\Version4\Terraria.GameContent.Bestiary\BestiaryEntry.cs | 12 | 2 | UIInfoProvider | Terraria.GameContent.Bestiary.IBestiaryUICollectionInfoProvider | `public IBestiaryUICollectionInfoProvider UIInfoProvider;` | `public IBestiaryUICollectionInfoProvider UIInfoProvider;` |

#### 属性（2）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3718 | property | Terraria.GameContent.Bestiary.BestiaryDatabase | Terraria.GameContent.Bestiary/BestiaryDatabase.cs | D:\TRbackup\Version4\Terraria.GameContent.Bestiary\BestiaryDatabase.cs | 21 | 2 | Entries | System.Collections.Generic.List<Terraria.GameContent.Bestiary.BestiaryEntry> | `public List<BestiaryEntry> Entries => _entries;` | `public List<BestiaryEntry> Entries => _entries;` |
| 3719 | property | Terraria.GameContent.Bestiary.BestiaryEntry | Terraria.GameContent.Bestiary/BestiaryEntry.cs | D:\TRbackup\Version4\Terraria.GameContent.Bestiary\BestiaryEntry.cs | 14 | 2 | Info | System.Collections.Generic.List<Terraria.GameContent.Bestiary.IBestiaryInfoElement> | `public List<IBestiaryInfoElement> Info { get; private set; }` | `public List<IBestiaryInfoElement> Info { get; private set; }` |


### 4.5 细分子系统：`BestiaryUnlockTracking`

- 原报告章节：`4.9.65`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`11` / `NPC、城镇与图鉴`

- 上一级基线细分子系统：`BestiaryUnlockTracking`
- 细分职责：图鉴击杀、接近、对话和解锁进度跟踪。
- 边界角色：`state/query`；最小 seam：bestiary unlock port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：5；声明类型数：5；字段：14；属性：1；合计：15。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（14）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1365 | field | Terraria.GameContent.Bestiary.BestiaryUnlockProgressReport | Terraria.GameContent.Bestiary/BestiaryUnlockProgressReport.cs | D:\TRbackup\Version4\Terraria.GameContent.Bestiary\BestiaryUnlockProgressReport.cs | 5 | 2 | EntriesTotal | int | `public int EntriesTotal;` | `public int EntriesTotal;` |
| 1366 | field | Terraria.GameContent.Bestiary.BestiaryUnlockProgressReport | Terraria.GameContent.Bestiary/BestiaryUnlockProgressReport.cs | D:\TRbackup\Version4\Terraria.GameContent.Bestiary\BestiaryUnlockProgressReport.cs | 7 | 2 | CompletionAmountTotal | float | `public float CompletionAmountTotal;` | `public float CompletionAmountTotal;` |
| 1367 | field | Terraria.GameContent.Bestiary.BestiaryUnlocksTracker | Terraria.GameContent.Bestiary/BestiaryUnlocksTracker.cs | D:\TRbackup\Version4\Terraria.GameContent.Bestiary\BestiaryUnlocksTracker.cs | 7 | 2 | Kills | Terraria.GameContent.Bestiary.NPCKillsTracker | `public NPCKillsTracker Kills = new NPCKillsTracker();` | `public NPCKillsTracker Kills = new NPCKillsTracker();` |
| 1368 | field | Terraria.GameContent.Bestiary.BestiaryUnlocksTracker | Terraria.GameContent.Bestiary/BestiaryUnlocksTracker.cs | D:\TRbackup\Version4\Terraria.GameContent.Bestiary\BestiaryUnlocksTracker.cs | 9 | 2 | Sights | Terraria.GameContent.Bestiary.NPCWasNearPlayerTracker | `public NPCWasNearPlayerTracker Sights = new NPCWasNearPlayerTracker();` | `public NPCWasNearPlayerTracker Sights = new NPCWasNearPlayerTracker();` |
| 1369 | field | Terraria.GameContent.Bestiary.BestiaryUnlocksTracker | Terraria.GameContent.Bestiary/BestiaryUnlocksTracker.cs | D:\TRbackup\Version4\Terraria.GameContent.Bestiary\BestiaryUnlocksTracker.cs | 11 | 2 | Chats | Terraria.GameContent.Bestiary.NPCWasChatWithTracker | `public NPCWasChatWithTracker Chats = new NPCWasChatWithTracker();` | `public NPCWasChatWithTracker Chats = new NPCWasChatWithTracker();` |
| 1380 | field | Terraria.GameContent.Bestiary.NPCKillsTracker | Terraria.GameContent.Bestiary/NPCKillsTracker.cs | D:\TRbackup\Version4\Terraria.GameContent.Bestiary\NPCKillsTracker.cs | 12 | 2 | _entryCreationLock | object | `private object _entryCreationLock = new object();` | `private object _entryCreationLock = new object();` |
| 1381 | field | Terraria.GameContent.Bestiary.NPCKillsTracker | Terraria.GameContent.Bestiary/NPCKillsTracker.cs | D:\TRbackup\Version4\Terraria.GameContent.Bestiary\NPCKillsTracker.cs | 14 | 2 | POSITIVE_KILL_COUNT_CAP | int | `public const int POSITIVE_KILL_COUNT_CAP = 999999999;` | `public const int POSITIVE_KILL_COUNT_CAP = 999999999;` |
| 1382 | field | Terraria.GameContent.Bestiary.NPCKillsTracker | Terraria.GameContent.Bestiary/NPCKillsTracker.cs | D:\TRbackup\Version4\Terraria.GameContent.Bestiary\NPCKillsTracker.cs | 16 | 2 | _killCountsByNpcId | System.Collections.Generic.Dictionary<string, int> | `private Dictionary<string, int> _killCountsByNpcId;` | `private Dictionary<string, int> _killCountsByNpcId;` |
| 1392 | field | Terraria.GameContent.Bestiary.NPCWasChatWithTracker | Terraria.GameContent.Bestiary/NPCWasChatWithTracker.cs | D:\TRbackup\Version4\Terraria.GameContent.Bestiary\NPCWasChatWithTracker.cs | 12 | 2 | _entryCreationLock | object | `private object _entryCreationLock = new object();` | `private object _entryCreationLock = new object();` |
| 1393 | field | Terraria.GameContent.Bestiary.NPCWasChatWithTracker | Terraria.GameContent.Bestiary/NPCWasChatWithTracker.cs | D:\TRbackup\Version4\Terraria.GameContent.Bestiary\NPCWasChatWithTracker.cs | 14 | 2 | _chattedWithPlayer | System.Collections.Generic.HashSet<string> | `private HashSet<string> _chattedWithPlayer;` | `private HashSet<string> _chattedWithPlayer;` |
| 1394 | field | Terraria.GameContent.Bestiary.NPCWasNearPlayerTracker | Terraria.GameContent.Bestiary/NPCWasNearPlayerTracker.cs | D:\TRbackup\Version4\Terraria.GameContent.Bestiary\NPCWasNearPlayerTracker.cs | 13 | 2 | _entryCreationLock | object | `private object _entryCreationLock = new object();` | `private object _entryCreationLock = new object();` |
| 1395 | field | Terraria.GameContent.Bestiary.NPCWasNearPlayerTracker | Terraria.GameContent.Bestiary/NPCWasNearPlayerTracker.cs | D:\TRbackup\Version4\Terraria.GameContent.Bestiary\NPCWasNearPlayerTracker.cs | 15 | 2 | _wasNearPlayer | System.Collections.Generic.HashSet<string> | `private HashSet<string> _wasNearPlayer;` | `private HashSet<string> _wasNearPlayer;` |
| 1396 | field | Terraria.GameContent.Bestiary.NPCWasNearPlayerTracker | Terraria.GameContent.Bestiary/NPCWasNearPlayerTracker.cs | D:\TRbackup\Version4\Terraria.GameContent.Bestiary\NPCWasNearPlayerTracker.cs | 17 | 2 | _playerHitboxesForBestiary | System.Collections.Generic.List<Rectangle> | `private List<Rectangle> _playerHitboxesForBestiary;` | `private List<Rectangle> _playerHitboxesForBestiary;` |
| 1397 | field | Terraria.GameContent.Bestiary.NPCWasNearPlayerTracker | Terraria.GameContent.Bestiary/NPCWasNearPlayerTracker.cs | D:\TRbackup\Version4\Terraria.GameContent.Bestiary\NPCWasNearPlayerTracker.cs | 19 | 2 | _wasSeenNearPlayerByNetId | System.Collections.Generic.List<int> | `private List<int> _wasSeenNearPlayerByNetId;` | `private List<int> _wasSeenNearPlayerByNetId;` |

#### 属性（1）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3720 | property | Terraria.GameContent.Bestiary.BestiaryUnlockProgressReport | Terraria.GameContent.Bestiary/BestiaryUnlockProgressReport.cs | D:\TRbackup\Version4\Terraria.GameContent.Bestiary\BestiaryUnlockProgressReport.cs | 9 | 2 | CompletionPercent | float | `public float CompletionPercent { get { if (EntriesTotal == 0) { return 1f; } return CompletionAmountTotal / (float)EntriesTotal; } }` | `public float CompletionPercent { get { if (EntriesTotal == 0) { return 1f; } return CompletionAmountTotal / (float)EntriesTotal; } }` |


### 4.6 细分子系统：`BestiaryFiltersAndSorting`

- 原报告章节：`4.9.66`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`11` / `NPC、城镇与图鉴`

- 上一级基线细分子系统：`BestiaryFiltersAndSorting`
- 细分职责：图鉴筛选器和排序步骤。
- 边界角色：`query`；最小 seam：bestiary filter/sort query；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：2；声明类型数：11；字段：1；属性：11；合计：12。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（1）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1374 | field | Terraria.GameContent.Bestiary.Filters.ByInfoElement | Terraria.GameContent.Bestiary/Filters.cs | D:\TRbackup\Version4\Terraria.GameContent.Bestiary\Filters.cs | 72 | 3 | _element | Terraria.GameContent.Bestiary.IBestiaryInfoElement | `private IBestiaryInfoElement _element;` | `private IBestiaryInfoElement _element;` |

#### 属性（11）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3721 | property | Terraria.GameContent.Bestiary.Filters.BySearch | Terraria.GameContent.Bestiary/Filters.cs | D:\TRbackup\Version4\Terraria.GameContent.Bestiary\Filters.cs | 15 | 3 | ForcedDisplay | bool? | `public bool? ForcedDisplay => true;` | `public bool? ForcedDisplay => true;` |
| 3722 | property | Terraria.GameContent.Bestiary.Filters.ByUnlockState | Terraria.GameContent.Bestiary/Filters.cs | D:\TRbackup\Version4\Terraria.GameContent.Bestiary\Filters.cs | 30 | 3 | ForcedDisplay | bool? | `public bool? ForcedDisplay => true;` | `public bool? ForcedDisplay => true;` |
| 3723 | property | Terraria.GameContent.Bestiary.Filters.ByRareCreature | Terraria.GameContent.Bestiary/Filters.cs | D:\TRbackup\Version4\Terraria.GameContent.Bestiary\Filters.cs | 44 | 3 | ForcedDisplay | bool? | `public bool? ForcedDisplay => null;` | `public bool? ForcedDisplay => null;` |
| 3724 | property | Terraria.GameContent.Bestiary.Filters.ByBoss | Terraria.GameContent.Bestiary/Filters.cs | D:\TRbackup\Version4\Terraria.GameContent.Bestiary\Filters.cs | 58 | 3 | ForcedDisplay | bool? | `public bool? ForcedDisplay => null;` | `public bool? ForcedDisplay => null;` |
| 3725 | property | Terraria.GameContent.Bestiary.Filters.ByInfoElement | Terraria.GameContent.Bestiary/Filters.cs | D:\TRbackup\Version4\Terraria.GameContent.Bestiary\Filters.cs | 74 | 3 | ForcedDisplay | bool? | `public bool? ForcedDisplay => null;` | `public bool? ForcedDisplay => null;` |
| 3732 | property | Terraria.GameContent.Bestiary.SortingSteps.ByNetId | Terraria.GameContent.Bestiary/SortingSteps.cs | D:\TRbackup\Version4\Terraria.GameContent.Bestiary\SortingSteps.cs | 13 | 3 | HiddenFromSortOptions | bool | `public bool HiddenFromSortOptions => true;` | `public bool HiddenFromSortOptions => true;` |
| 3733 | property | Terraria.GameContent.Bestiary.SortingSteps.ByUnlockState | Terraria.GameContent.Bestiary/SortingSteps.cs | D:\TRbackup\Version4\Terraria.GameContent.Bestiary\SortingSteps.cs | 24 | 3 | HiddenFromSortOptions | bool | `public bool HiddenFromSortOptions => true;` | `public bool HiddenFromSortOptions => true;` |
| 3734 | property | Terraria.GameContent.Bestiary.SortingSteps.ByBestiarySortingId | Terraria.GameContent.Bestiary/SortingSteps.cs | D:\TRbackup\Version4\Terraria.GameContent.Bestiary\SortingSteps.cs | 35 | 3 | HiddenFromSortOptions | bool | `public bool HiddenFromSortOptions => false;` | `public bool HiddenFromSortOptions => false;` |
| 3735 | property | Terraria.GameContent.Bestiary.SortingSteps.ByBestiaryRarity | Terraria.GameContent.Bestiary/SortingSteps.cs | D:\TRbackup\Version4\Terraria.GameContent.Bestiary\SortingSteps.cs | 46 | 3 | HiddenFromSortOptions | bool | `public bool HiddenFromSortOptions => false;` | `public bool HiddenFromSortOptions => false;` |
| 3736 | property | Terraria.GameContent.Bestiary.SortingSteps.Alphabetical | Terraria.GameContent.Bestiary/SortingSteps.cs | D:\TRbackup\Version4\Terraria.GameContent.Bestiary\SortingSteps.cs | 57 | 3 | HiddenFromSortOptions | bool | `public bool HiddenFromSortOptions => false;` | `public bool HiddenFromSortOptions => false;` |
| 3737 | property | Terraria.GameContent.Bestiary.SortingSteps.ByStat | Terraria.GameContent.Bestiary/SortingSteps.cs | D:\TRbackup\Version4\Terraria.GameContent.Bestiary\SortingSteps.cs | 68 | 3 | HiddenFromSortOptions | bool | `public bool HiddenFromSortOptions => false;` | `public bool HiddenFromSortOptions => false;` |


### 4.7 细分子系统：`SharedBestiaryInfoElementState`

- 原报告章节：`4.9.192`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`11` / `NPC、城镇与图鉴`

- 上一级基线细分子系统：`BestiaryInfoElementsAndProviders`
- 上一级 peer 细分子系统：`BestiaryInfoElementsAndProviders`
- 细分职责：图鉴信息元素、显示事实和元素索引。
- 边界角色：`definition/presentation`；最小 seam：bestiary info element port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：8；声明类型数：8；字段：14；属性：3；合计：17。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（14）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1375 | field | Terraria.GameContent.Bestiary.FlavorTextBestiaryInfoElement | Terraria.GameContent.Bestiary/FlavorTextBestiaryInfoElement.cs | D:\TRbackup\Version4\Terraria.GameContent.Bestiary\FlavorTextBestiaryInfoElement.cs | 11 | 2 | _key | string | `private string _key;` | `private string _key;` |
| 1376 | field | Terraria.GameContent.Bestiary.ItemDropBestiaryInfoElement | Terraria.GameContent.Bestiary/ItemDropBestiaryInfoElement.cs | D:\TRbackup\Version4\Terraria.GameContent.Bestiary\ItemDropBestiaryInfoElement.cs | 10 | 2 | _droprateInfo | Terraria.GameContent.ItemDropRules.DropRateInfo | `protected DropRateInfo _droprateInfo;` | `protected DropRateInfo _droprateInfo;` |
| 1377 | field | Terraria.GameContent.Bestiary.NamePlateInfoElement | Terraria.GameContent.Bestiary/NamePlateInfoElement.cs | D:\TRbackup\Version4\Terraria.GameContent.Bestiary\NamePlateInfoElement.cs | 9 | 2 | _key | string | `private string _key;` | `private string _key;` |
| 1378 | field | Terraria.GameContent.Bestiary.NamePlateInfoElement | Terraria.GameContent.Bestiary/NamePlateInfoElement.cs | D:\TRbackup\Version4\Terraria.GameContent.Bestiary\NamePlateInfoElement.cs | 11 | 2 | _npcNetId | int | `private int _npcNetId;` | `private int _npcNetId;` |
| 1379 | field | Terraria.GameContent.Bestiary.NPCKillCounterInfoElement | Terraria.GameContent.Bestiary/NPCKillCounterInfoElement.cs | D:\TRbackup\Version4\Terraria.GameContent.Bestiary\NPCKillCounterInfoElement.cs | 13 | 2 | _instance | Terraria.NPC | `private NPC _instance;` | `private NPC _instance;` |
| 1383 | field | Terraria.GameContent.Bestiary.NPCPortraitInfoElement | Terraria.GameContent.Bestiary/NPCPortraitInfoElement.cs | D:\TRbackup\Version4\Terraria.GameContent.Bestiary\NPCPortraitInfoElement.cs | 14 | 2 | _filledStarsCount | int? | `private int? _filledStarsCount;` | `private int? _filledStarsCount;` |
| 1384 | field | Terraria.GameContent.Bestiary.NPCStatsReportInfoElement | Terraria.GameContent.Bestiary/NPCStatsReportInfoElement.cs | D:\TRbackup\Version4\Terraria.GameContent.Bestiary\NPCStatsReportInfoElement.cs | 13 | 2 | NpcId | int | `public int NpcId;` | `public int NpcId;` |
| 1385 | field | Terraria.GameContent.Bestiary.NPCStatsReportInfoElement | Terraria.GameContent.Bestiary/NPCStatsReportInfoElement.cs | D:\TRbackup\Version4\Terraria.GameContent.Bestiary\NPCStatsReportInfoElement.cs | 15 | 2 | Damage | int | `public int Damage;` | `public int Damage;` |
| 1386 | field | Terraria.GameContent.Bestiary.NPCStatsReportInfoElement | Terraria.GameContent.Bestiary/NPCStatsReportInfoElement.cs | D:\TRbackup\Version4\Terraria.GameContent.Bestiary\NPCStatsReportInfoElement.cs | 17 | 2 | LifeMax | int | `public int LifeMax;` | `public int LifeMax;` |
| 1387 | field | Terraria.GameContent.Bestiary.NPCStatsReportInfoElement | Terraria.GameContent.Bestiary/NPCStatsReportInfoElement.cs | D:\TRbackup\Version4\Terraria.GameContent.Bestiary\NPCStatsReportInfoElement.cs | 19 | 2 | MonetaryValue | float | `public float MonetaryValue;` | `public float MonetaryValue;` |
| 1388 | field | Terraria.GameContent.Bestiary.NPCStatsReportInfoElement | Terraria.GameContent.Bestiary/NPCStatsReportInfoElement.cs | D:\TRbackup\Version4\Terraria.GameContent.Bestiary\NPCStatsReportInfoElement.cs | 21 | 2 | Defense | int | `public int Defense;` | `public int Defense;` |
| 1389 | field | Terraria.GameContent.Bestiary.NPCStatsReportInfoElement | Terraria.GameContent.Bestiary/NPCStatsReportInfoElement.cs | D:\TRbackup\Version4\Terraria.GameContent.Bestiary\NPCStatsReportInfoElement.cs | 23 | 2 | KnockbackResist | float | `public float KnockbackResist;` | `public float KnockbackResist;` |
| 1390 | field | Terraria.GameContent.Bestiary.NPCStatsReportInfoElement | Terraria.GameContent.Bestiary/NPCStatsReportInfoElement.cs | D:\TRbackup\Version4\Terraria.GameContent.Bestiary\NPCStatsReportInfoElement.cs | 25 | 2 | _instance | Terraria.NPC | `private NPC _instance;` | `private NPC _instance;` |
| 1391 | field | Terraria.GameContent.Bestiary.NPCStatsReportInfoElement | Terraria.GameContent.Bestiary/NPCStatsReportInfoElement.cs | D:\TRbackup\Version4\Terraria.GameContent.Bestiary\NPCStatsReportInfoElement.cs | 27 | 2 | HideStats | bool | `public bool HideStats;` | `public bool HideStats;` |

#### 属性（3）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3729 | property | Terraria.GameContent.Bestiary.NPCNetIdBestiaryInfoElement | Terraria.GameContent.Bestiary/NPCNetIdBestiaryInfoElement.cs | D:\TRbackup\Version4\Terraria.GameContent.Bestiary\NPCNetIdBestiaryInfoElement.cs | 8 | 2 | NetId | int | `public int NetId { get; private set; }` | `public int NetId { get; private set; }` |
| 3730 | property | Terraria.GameContent.Bestiary.NPCNetIdBestiaryInfoElement | Terraria.GameContent.Bestiary/NPCNetIdBestiaryInfoElement.cs | D:\TRbackup\Version4\Terraria.GameContent.Bestiary\NPCNetIdBestiaryInfoElement.cs | 10 | 2 | BestiaryDisplayIndex | int | `public int BestiaryDisplayIndex => ContentSamples.NpcBestiarySortingId[NetId];` | `public int BestiaryDisplayIndex => ContentSamples.NpcBestiarySortingId[NetId];` |
| 3731 | property | Terraria.GameContent.Bestiary.RareSpawnBestiaryInfoElement | Terraria.GameContent.Bestiary/RareSpawnBestiaryInfoElement.cs | D:\TRbackup\Version4\Terraria.GameContent.Bestiary\RareSpawnBestiaryInfoElement.cs | 8 | 2 | RarityLevel | int | `public int RarityLevel { get; private set; }` | `public int RarityLevel { get; private set; }` |


### 4.8 细分子系统：`SharedBestiaryCollectionProviderState`

- 原报告章节：`4.9.193`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`11` / `NPC、城镇与图鉴`

- 上一级基线细分子系统：`BestiaryInfoElementsAndProviders`
- 上一级 peer 细分子系统：`BestiaryInfoElementsAndProviders`
- 细分职责：图鉴集合信息、UICollection provider 和集合接口。
- 边界角色：`definition/presentation`；最小 seam：bestiary collection provider port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：7；声明类型数：7；字段：7；属性：3；合计：10。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（7）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1363 | field | Terraria.GameContent.Bestiary.BestiaryUICollectionInfo | Terraria.GameContent.Bestiary/BestiaryUICollectionInfo.cs | D:\TRbackup\Version4\Terraria.GameContent.Bestiary\BestiaryUICollectionInfo.cs | 5 | 2 | OwnerEntry | Terraria.GameContent.Bestiary.BestiaryEntry | `public BestiaryEntry OwnerEntry;` | `public BestiaryEntry OwnerEntry;` |
| 1364 | field | Terraria.GameContent.Bestiary.BestiaryUICollectionInfo | Terraria.GameContent.Bestiary/BestiaryUICollectionInfo.cs | D:\TRbackup\Version4\Terraria.GameContent.Bestiary\BestiaryUICollectionInfo.cs | 7 | 2 | UnlockState | Terraria.GameContent.Bestiary.BestiaryEntryUnlockState | `public BestiaryEntryUnlockState UnlockState;` | `public BestiaryEntryUnlockState UnlockState;` |
| 1370 | field | Terraria.GameContent.Bestiary.CommonEnemyUICollectionInfoProvider | Terraria.GameContent.Bestiary/CommonEnemyUICollectionInfoProvider.cs | D:\TRbackup\Version4\Terraria.GameContent.Bestiary\CommonEnemyUICollectionInfoProvider.cs | 8 | 2 | _persistentIdentifierToCheck | string | `private string _persistentIdentifierToCheck;` | `private string _persistentIdentifierToCheck;` |
| 1371 | field | Terraria.GameContent.Bestiary.CommonEnemyUICollectionInfoProvider | Terraria.GameContent.Bestiary/CommonEnemyUICollectionInfoProvider.cs | D:\TRbackup\Version4\Terraria.GameContent.Bestiary\CommonEnemyUICollectionInfoProvider.cs | 10 | 2 | _quickUnlock | bool | `private bool _quickUnlock;` | `private bool _quickUnlock;` |
| 1372 | field | Terraria.GameContent.Bestiary.CommonEnemyUICollectionInfoProvider | Terraria.GameContent.Bestiary/CommonEnemyUICollectionInfoProvider.cs | D:\TRbackup\Version4\Terraria.GameContent.Bestiary\CommonEnemyUICollectionInfoProvider.cs | 12 | 2 | _killCountNeededToFullyUnlock | int | `private int _killCountNeededToFullyUnlock;` | `private int _killCountNeededToFullyUnlock;` |
| 1373 | field | Terraria.GameContent.Bestiary.CritterUICollectionInfoProvider | Terraria.GameContent.Bestiary/CritterUICollectionInfoProvider.cs | D:\TRbackup\Version4\Terraria.GameContent.Bestiary\CritterUICollectionInfoProvider.cs | 7 | 2 | _persistentIdentifierToCheck | string | `private string _persistentIdentifierToCheck;` | `private string _persistentIdentifierToCheck;` |
| 1398 | field | Terraria.GameContent.Bestiary.TownNPCUICollectionInfoProvider | Terraria.GameContent.Bestiary/TownNPCUICollectionInfoProvider.cs | D:\TRbackup\Version4\Terraria.GameContent.Bestiary\TownNPCUICollectionInfoProvider.cs | 7 | 2 | _persistentIdentifierToCheck | string | `private string _persistentIdentifierToCheck;` | `private string _persistentIdentifierToCheck;` |

#### 属性（3）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3726 | property | Terraria.GameContent.Bestiary.IBestiaryEntryDisplayIndex | Terraria.GameContent.Bestiary/IBestiaryEntryDisplayIndex.cs | D:\TRbackup\Version4\Terraria.GameContent.Bestiary\IBestiaryEntryDisplayIndex.cs | 5 | 2 | BestiaryDisplayIndex | int | `int BestiaryDisplayIndex { get; }` | `int BestiaryDisplayIndex { get; }` |
| 3727 | property | Terraria.GameContent.Bestiary.IBestiaryEntryFilter | Terraria.GameContent.Bestiary/IBestiaryEntryFilter.cs | D:\TRbackup\Version4\Terraria.GameContent.Bestiary\IBestiaryEntryFilter.cs | 7 | 2 | ForcedDisplay | bool? | `bool? ForcedDisplay { get; }` | `bool? ForcedDisplay { get; }` |
| 3728 | property | Terraria.GameContent.Bestiary.IBestiarySortStep | Terraria.GameContent.Bestiary/IBestiarySortStep.cs | D:\TRbackup\Version4\Terraria.GameContent.Bestiary\IBestiarySortStep.cs | 8 | 2 | HiddenFromSortOptions | bool | `bool HiddenFromSortOptions { get; }` | `bool HiddenFromSortOptions { get; }` |


## 5. 追溯与验收

> 来源报告 SHA-256：`b944441d92d5103f218b766a3e354200794528a19e65a862e44c2b40a36f1196`
- 本分区细分组数：8；成员数：93；字段：71；属性：22。
- 预期不变量：本文件只出现完整细分子系统；不跨分区复制成员；所有成员的来源序号、原始声明、路径、行列和 C# 类型必须与来源报告一致。
- 验证方式：重新读取 20 个文件，检查文件数、细分组唯一性、来源序号恰好覆盖 `1..4542`、字段/属性合计和逐成员行文本一致；本次分区生成不运行 `dotnet`，因为没有修改 C# 或项目文件。

生成器：`Build/Tools/Generate-Version4NonAuthoritativeComponentPartitionReports.ps1`。
