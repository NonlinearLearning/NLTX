# Version4 非权威组件拆分分区 14/20：持久化、恢复与配置

> 来源报告：`docs/migration/ledgers/Version4非权威模拟系统字段属性逐成员源码声明-更细子系统拆分-去除ID类文件.md`
> 来源报告 SHA-256：`b944441d92d5103f218b766a3e354200794528a19e65a862e44c2b40a36f1196`
> 本文件是并行组件拆分工作包，不是新的权威成员清单；成员声明事实直接保留来源报告原文。

## 1. 分区定位与评估

- 分区范围：玩家/世界文件、恢复版本、Tile header、保存会话、文件平台和配置适配器。
- 本分区组件化重点：区分持久化快照、运行时状态、文件协议适配和恢复事务，记录失败策略。
- 本分区包含 11 个完整细分子系统、160 条成员记录（字段 146、属性 14）。来源序号覆盖区间 `38..3892`，区间可能与其他分区交错，不以序号定义领域边界。
- 组件边界判定：细分子系统只是并行处理的最小库存边界，不等价于一个组件。必须继续按共同读者/写者、变更原因、生命周期、权威性和副作用分为 Component、System、Query、Command、Adapter 或 Projection。
- 证据状态：成员声明和统计已由来源报告校验；逐成员读者、写者、初始化/清理路径、持久化/网络语义和系统顺序仍需本分区会话补证，不得从名称或数量推断。

## 2. 分区统计

| 父级子系统 | 细分子系统数 | 字段 | 属性 | 合计 |
|---|---:|---:|---:|---:|
| `PersistenceAndRecovery` | 7 | 95 | 11 | 106 |
| `RuntimeComposition` | 2 | 25 | 0 | 25 |
| `SharedRuntimeMechanisms` | 2 | 26 | 3 | 29 |

| 原报告章节 | 父级 | 细分子系统 | 边界角色 | 字段 | 属性 | 合计 | 组件化处理 |
|---|---|---|---|---:|---:|---:|---|
| `4.1.5` | `RuntimeComposition` | `MainSaveFavoritesAndSessionRefs` | session state | 10 | 0 | 10 | 待按成员访问模式拆分 |
| `4.1.17` | `RuntimeComposition` | `MainWorldPersistenceAndMetadata` | session state | 15 | 0 | 15 | 待按成员访问模式拆分 |
| `4.2.1` | `PersistenceAndRecovery` | `PlayerFileMetadataAndSession` | snapshot state | 4 | 3 | 7 | 待按成员访问模式拆分 |
| `4.2.2` | `PersistenceAndRecovery` | `WorldFileMetadataIdentityState` | snapshot state | 14 | 0 | 14 | 待按成员访问模式拆分 |
| `4.2.3` | `PersistenceAndRecovery` | `WorldFileSessionAndValidityState` | snapshot state | 14 | 8 | 22 | 待按成员访问模式拆分 |
| `4.2.4` | `PersistenceAndRecovery` | `WorldFileTileHeaderCoreState` | adapter state | 19 | 0 | 19 | 待按成员访问模式拆分 |
| `4.2.5` | `PersistenceAndRecovery` | `WorldFileTileHeaderExtensionState` | adapter state | 16 | 0 | 16 | 待按成员访问模式拆分 |
| `4.2.6` | `PersistenceAndRecovery` | `WorldFileRecoveryVersionState` | adapter state | 5 | 0 | 5 | 待按成员访问模式拆分 |
| `4.2.7` | `PersistenceAndRecovery` | `WorldFileTemporaryEventState` | adapter state | 23 | 0 | 23 | 待按成员访问模式拆分 |
| `4.9.57` | `SharedRuntimeMechanisms` | `SharedSaveAndConfigurationAdapters` | adapter | 22 | 3 | 25 | 待按成员访问模式拆分 |
| `4.9.83` | `SharedRuntimeMechanisms` | `SharedGeneralFilePlatformUtilities` | adapter | 4 | 0 | 4 | 待按成员访问模式拆分 |

## 3. 并行组件拆分契约

对本文件中的每个细分子系统和每条成员记录执行以下判断，并把结果写入对应的实现阶段设计/审计记录：

- 盘点声明类型、可见性、读者、写者、创建/更新/清理/持久化生命周期、访问频率和副作用；缺证据标记 `missing`，不可用命名猜测补齐。
- 标记权威状态、派生值、缓存、快照、兼容字段和表现数据；Component 只保存一个内聚概念的权威数据。
- 需要行为转换时放入显式 System/Command；纯资格判断和派生计算放入 Query；外部协议、文件、平台和第三方类型放入 Adapter；网络/持久化/日志/UI 输出放入单向 Projection。
- 分别建模实体引用、持久化 ID、网络 ID 和外部 ID；不得把 ID 类文件或外部对象直接塞入共享组件。
- 记录 Interface、Implementation、Seam、Depth、Leverage、Locality，以及必要的系统先后约束、失败归属和重试策略。

## 4. 逐成员源码声明

以下细分子系统块保持来源报告的成员表、声明、路径、行列和来源序号；只调整 Markdown 标题层级并增加分区追踪元数据。

### 4.1 细分子系统：`MainSaveFavoritesAndSessionRefs`

- 原报告章节：`4.1.5`
- 父级子系统：`RuntimeComposition`
- 分区工作包：`14` / `持久化、恢复与配置`

- 上一级基线细分子系统：`MainSaveFavoritesAndSessionRefs`
- 细分职责：收藏、世界文件元数据、成就和会话对象引用。
- 边界角色：`session state`；最小 seam：favorites/session reference view；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：10；属性：0；合计：10。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（10）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 38 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 180 | 2 | LocalFavoriteData | Terraria.IO.FavoritesFile | `public static FavoritesFile LocalFavoriteData = new FavoritesFile(SavePath + "/favorites.json", isCloud: false);` | `public static FavoritesFile LocalFavoriteData = new FavoritesFile(SavePath + "/favorites.json", isCloud: false);` |
| 39 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 182 | 2 | CloudFavoritesData | Terraria.IO.FavoritesFile | `public static FavoritesFile CloudFavoritesData = new FavoritesFile("favorites.json", isCloud: true);` | `public static FavoritesFile CloudFavoritesData = new FavoritesFile("favorites.json", isCloud: true);` |
| 40 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 184 | 2 | WorldFileMetadata | Terraria.IO.FileMetadata | `public static FileMetadata WorldFileMetadata;` | `public static FileMetadata WorldFileMetadata;` |
| 41 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 186 | 2 | Pings | Terraria.Map.PingMapLayer | `public static PingMapLayer Pings = new PingMapLayer();` | `public static PingMapLayer Pings = new PingMapLayer();` |
| 42 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 188 | 2 | _achievements | Terraria.Achievements.AchievementManager | `private AchievementManager _achievements;` | `private AchievementManager _achievements;` |
| 43 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 190 | 2 | MenuUI | Terraria.UI.UserInterface | `public static UserInterface MenuUI = new UserInterface();` | `public static UserInterface MenuUI = new UserInterface();` |
| 44 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 192 | 2 | InGameUI | Terraria.UI.UserInterface | `public static UserInterface InGameUI = new UserInterface();` | `public static UserInterface InGameUI = new UserInterface();` |
| 45 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 194 | 2 | waterfallManager | Terraria.WaterfallManager | `public WaterfallManager waterfallManager;` | `public WaterfallManager waterfallManager;` |
| 46 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 196 | 2 | sectionManager | Terraria.WorldSections | `public static WorldSections sectionManager;` | `public static WorldSections sectionManager;` |
| 47 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 198 | 2 | ServerSideCharacter | bool | `public static bool ServerSideCharacter;` | `public static bool ServerSideCharacter;` |

#### 属性（0）

无该类型成员记录。


### 4.2 细分子系统：`MainWorldPersistenceAndMetadata`

- 原报告章节：`4.1.17`
- 父级子系统：`RuntimeComposition`
- 分区工作包：`14` / `持久化、恢复与配置`

- 上一级基线细分子系统：`MainWorldPersistenceAndMetadata`
- 细分职责：回滚、地牢锚点、保存校验、路径和世界元数据。
- 边界角色：`session state`；最小 seam：world metadata/persistence view；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：15；属性：0；合计：15。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（15）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 198 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 563 | 2 | WorldRollingBackupsCountToKeep | int | `public static int WorldRollingBackupsCountToKeep = 2;` | `public static int WorldRollingBackupsCountToKeep = 2;` |
| 199 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 565 | 2 | dungeonX | int | `public static int dungeonX;` | `public static int dungeonX;` |
| 200 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 567 | 2 | dungeonY | int | `public static int dungeonY;` | `public static int dungeonY;` |
| 201 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 569 | 2 | liquid | Terraria.Liquid[] | `public static Liquid[] liquid = new Liquid[Liquid.maxLiquid];` | `public static Liquid[] liquid = new Liquid[Liquid.maxLiquid];` |
| 202 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 571 | 2 | liquidBuffer | Terraria.LiquidBuffer[] | `public static LiquidBuffer[] liquidBuffer = new LiquidBuffer[50000];` | `public static LiquidBuffer[] liquidBuffer = new LiquidBuffer[50000];` |
| 203 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 573 | 2 | dedServ | bool | `public static bool dedServ;` | `public static bool dedServ;` |
| 204 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 575 | 2 | showItemText | bool | `public static bool showItemText = true;` | `public static bool showItemText = true;` |
| 205 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 577 | 2 | validateSaves | bool | `public static bool validateSaves = true;` | `public static bool validateSaves = true;` |
| 206 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 579 | 2 | libPath | string | `public static string libPath = "";` | `public static string libPath = "";` |
| 207 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 581 | 2 | lo | int | `public static int lo;` | `public static int lo;` |
| 208 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 583 | 2 | statusText | string | `public static string statusText = "";` | `public static string statusText = "";` |
| 209 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 585 | 2 | worldName | string | `public static string worldName = "";` | `public static string worldName = "";` |
| 210 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 587 | 2 | worldSurface | double | `public static double worldSurface;` | `public static double worldSurface;` |
| 211 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 589 | 2 | rockLayer | double | `public static double rockLayer;` | `public static double rockLayer;` |
| 212 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 591 | 2 | teamColor | Microsoft.Xna.Framework.Color[] | `public static Microsoft.Xna.Framework.Color[] teamColor = new Microsoft.Xna.Framework.Color[6];` | `public static Microsoft.Xna.Framework.Color[] teamColor = new Microsoft.Xna.Framework.Color[6];` |

#### 属性（0）

无该类型成员记录。


### 4.3 细分子系统：`PlayerFileMetadataAndSession`

- 原报告章节：`4.2.1`
- 父级子系统：`PersistenceAndRecovery`
- 分区工作包：`14` / `持久化、恢复与配置`

- 上一级基线细分子系统：`PlayerFileMetadataAndSession`
- 细分职责：玩家存档元数据、路径和活动文件状态。
- 边界角色：`snapshot state`；最小 seam：player save metadata port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：4；属性：3；合计：7。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（4）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 543 | field | Terraria.IO.PlayerFileData | Terraria.IO/PlayerFileData.cs | D:\TRbackup\Version4\Terraria.IO\PlayerFileData.cs | 12 | 2 | _player | Terraria.Player | `private Player _player;` | `private Player _player;` |
| 544 | field | Terraria.IO.PlayerFileData | Terraria.IO/PlayerFileData.cs | D:\TRbackup\Version4\Terraria.IO\PlayerFileData.cs | 14 | 2 | _playTime | System.TimeSpan | `private TimeSpan _playTime = TimeSpan.Zero;` | `private TimeSpan _playTime = TimeSpan.Zero;` |
| 545 | field | Terraria.IO.PlayerFileData | Terraria.IO/PlayerFileData.cs | D:\TRbackup\Version4\Terraria.IO\PlayerFileData.cs | 16 | 2 | _timer | System.Diagnostics.Stopwatch | `private readonly Stopwatch _timer = new Stopwatch();` | `private readonly Stopwatch _timer = new Stopwatch();` |
| 546 | field | Terraria.IO.PlayerFileData | Terraria.IO/PlayerFileData.cs | D:\TRbackup\Version4\Terraria.IO\PlayerFileData.cs | 18 | 2 | _isTimerActive | bool | `private bool _isTimerActive;` | `private bool _isTimerActive;` |

#### 属性（3）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 638 | property | Terraria.IO.PlayerFileData | Terraria.IO/PlayerFileData.cs | D:\TRbackup\Version4\Terraria.IO\PlayerFileData.cs | 20 | 2 | Player | Terraria.Player | `public Player Player { get { return _player; } set { _player = value; if (value != null) { Name = _player.name; } } }` | `public Player Player { get { return _player; } set { _player = value; if (value != null) { Name = _player.name; } } }` |
| 639 | property | Terraria.IO.PlayerFileData | Terraria.IO/PlayerFileData.cs | D:\TRbackup\Version4\Terraria.IO\PlayerFileData.cs | 36 | 2 | ServerSideCharacter | bool | `public bool ServerSideCharacter { get; private set; }` | `public bool ServerSideCharacter { get; private set; }` |
| 640 | property | Terraria.IO.PlayerFileData | Terraria.IO/PlayerFileData.cs | D:\TRbackup\Version4\Terraria.IO\PlayerFileData.cs | 38 | 2 | LastPlayed | System.DateTime | `public DateTime LastPlayed => DateTime.FromBinary(Player.lastTimePlayerWasSaved);` | `public DateTime LastPlayed => DateTime.FromBinary(Player.lastTimePlayerWasSaved);` |


### 4.4 细分子系统：`WorldFileMetadataIdentityState`

- 原报告章节：`4.2.2`
- 父级子系统：`PersistenceAndRecovery`
- 分区工作包：`14` / `持久化、恢复与配置`

- 上一级基线细分子系统：`WorldFileMetadataAndSession`
- 细分职责：世界尺寸、创建时间、种子原文和世界标识元数据。
- 边界角色：`snapshot state`；最小 seam：world file metadata identity port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：14；属性：0；合计：14。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（14）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 610 | field | Terraria.IO.WorldFileData | Terraria.IO/WorldFileData.cs | D:\TRbackup\Version4\Terraria.IO\WorldFileData.cs | 17 | 2 | GUID_IN_WORLD_FILE_VERSION | ulong | `private const ulong GUID_IN_WORLD_FILE_VERSION = 777389080577uL;` | `private const ulong GUID_IN_WORLD_FILE_VERSION = 777389080577uL;` |
| 611 | field | Terraria.IO.WorldFileData | Terraria.IO/WorldFileData.cs | D:\TRbackup\Version4\Terraria.IO\WorldFileData.cs | 19 | 2 | MAX_USER_SEED_TEXT_LENGTH | int | `public static readonly int MAX_USER_SEED_TEXT_LENGTH = 40;` | `public static readonly int MAX_USER_SEED_TEXT_LENGTH = 40;` |
| 612 | field | Terraria.IO.WorldFileData | Terraria.IO/WorldFileData.cs | D:\TRbackup\Version4\Terraria.IO\WorldFileData.cs | 21 | 2 | CreationTime | System.DateTime | `public DateTime CreationTime;` | `public DateTime CreationTime;` |
| 613 | field | Terraria.IO.WorldFileData | Terraria.IO/WorldFileData.cs | D:\TRbackup\Version4\Terraria.IO\WorldFileData.cs | 23 | 2 | LastPlayed | System.DateTime | `public DateTime LastPlayed;` | `public DateTime LastPlayed;` |
| 614 | field | Terraria.IO.WorldFileData | Terraria.IO/WorldFileData.cs | D:\TRbackup\Version4\Terraria.IO\WorldFileData.cs | 25 | 2 | WorldSizeX | int | `public int WorldSizeX;` | `public int WorldSizeX;` |
| 615 | field | Terraria.IO.WorldFileData | Terraria.IO/WorldFileData.cs | D:\TRbackup\Version4\Terraria.IO\WorldFileData.cs | 27 | 2 | WorldSizeY | int | `public int WorldSizeY;` | `public int WorldSizeY;` |
| 616 | field | Terraria.IO.WorldFileData | Terraria.IO/WorldFileData.cs | D:\TRbackup\Version4\Terraria.IO\WorldFileData.cs | 29 | 2 | WorldGeneratorVersion | ulong | `public ulong WorldGeneratorVersion;` | `public ulong WorldGeneratorVersion;` |
| 617 | field | Terraria.IO.WorldFileData | Terraria.IO/WorldFileData.cs | D:\TRbackup\Version4\Terraria.IO\WorldFileData.cs | 31 | 2 | _seedText | string | `private string _seedText = "";` | `private string _seedText = "";` |
| 618 | field | Terraria.IO.WorldFileData | Terraria.IO/WorldFileData.cs | D:\TRbackup\Version4\Terraria.IO\WorldFileData.cs | 33 | 2 | _seed | int | `private int _seed;` | `private int _seed;` |
| 621 | field | Terraria.IO.WorldFileData | Terraria.IO/WorldFileData.cs | D:\TRbackup\Version4\Terraria.IO\WorldFileData.cs | 39 | 2 | UniqueId | System.Guid | `public Guid UniqueId;` | `public Guid UniqueId;` |
| 622 | field | Terraria.IO.WorldFileData | Terraria.IO/WorldFileData.cs | D:\TRbackup\Version4\Terraria.IO\WorldFileData.cs | 41 | 2 | WorldId | int | `public int WorldId;` | `public int WorldId;` |
| 623 | field | Terraria.IO.WorldFileData | Terraria.IO/WorldFileData.cs | D:\TRbackup\Version4\Terraria.IO\WorldFileData.cs | 43 | 2 | _worldSizeName | Terraria.Localization.LocalizedText | `public LocalizedText _worldSizeName;` | `public LocalizedText _worldSizeName;` |
| 624 | field | Terraria.IO.WorldFileData | Terraria.IO/WorldFileData.cs | D:\TRbackup\Version4\Terraria.IO\WorldFileData.cs | 45 | 2 | GameMode | int | `public int GameMode;` | `public int GameMode;` |
| 637 | field | Terraria.IO.WorldFileData | Terraria.IO/WorldFileData.cs | D:\TRbackup\Version4\Terraria.IO\WorldFileData.cs | 71 | 2 | seedOptionsInOrder | System.Collections.Generic.List<Terraria.WorldBuilding.AWorldGenerationOption> | `private static List<AWorldGenerationOption> seedOptionsInOrder = new List<AWorldGenerationOption>  	{  		WorldGenerationOptions.Get<WorldSeedOption_Drunk>(),  		WorldGenerationOptions.Get<WorldSeedOption_NotTheBees>(),  		WorldGenerationOptions.Get<WorldSeedOption_ForTheWorthy>(),  		WorldGenerationOptions.Get<WorldSeedOption_Anniversary>(),  		WorldGenerationOptions.Get<WorldSeedOption_DontStarve>(),  		WorldGenerationOptions.Get<WorldSeedOption_Remix>(),  		WorldGenerationOptions.Get<WorldSeedOption_NoTraps>(),  		WorldGenerationOptions.Get<WorldSeedOption_Everything>(),  		WorldGenerationOptions.Get<WorldSeedOption_Skyblock>()  	};` | `private static List<AWorldGenerationOption> seedOptionsInOrder = new List<AWorldGenerationOption> { WorldGenerationOptions.Get<WorldSeedOption_Drunk>(), WorldGenerationOptions.Get<WorldSeedOption_NotTheBees>(), WorldGenerationOptions.Get<WorldSeedOption_ForTheWorthy>(), WorldGenerationOptions.Get<WorldSeedOption_Anniversary>(), WorldGenerationOptions.Get<WorldSeedOption_DontStarve>(), WorldGenerationOptions.Get<WorldSeedOption_Remix>(), WorldGenerationOptions.Get<WorldSeedOption_NoTraps>(), WorldGenerationOptions.Get<WorldSeedOption_Everything>(), WorldGenerationOptions.Get<WorldSeedOption_Skyblock>() };` |

#### 属性（0）

无该类型成员记录。


### 4.5 细分子系统：`WorldFileSessionAndValidityState`

- 原报告章节：`4.2.3`
- 父级子系统：`PersistenceAndRecovery`
- 分区工作包：`14` / `持久化、恢复与配置`

- 上一级基线细分子系统：`WorldFileMetadataAndSession`
- 细分职责：世界加载状态、模式开关、有效性和地图路径状态。
- 边界角色：`snapshot state`；最小 seam：world file session validity port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：14；属性：8；合计：22。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（14）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 619 | field | Terraria.IO.WorldFileData | Terraria.IO/WorldFileData.cs | D:\TRbackup\Version4\Terraria.IO\WorldFileData.cs | 35 | 2 | LoadStatus | int | `public int LoadStatus = StatusID.Ok;` | `public int LoadStatus = StatusID.Ok;` |
| 620 | field | Terraria.IO.WorldFileData | Terraria.IO/WorldFileData.cs | D:\TRbackup\Version4\Terraria.IO\WorldFileData.cs | 37 | 2 | LoadException | System.Exception | `public Exception LoadException;` | `public Exception LoadException;` |
| 625 | field | Terraria.IO.WorldFileData | Terraria.IO/WorldFileData.cs | D:\TRbackup\Version4\Terraria.IO\WorldFileData.cs | 47 | 2 | DrunkWorld | bool | `public bool DrunkWorld;` | `public bool DrunkWorld;` |
| 626 | field | Terraria.IO.WorldFileData | Terraria.IO/WorldFileData.cs | D:\TRbackup\Version4\Terraria.IO\WorldFileData.cs | 49 | 2 | NotTheBees | bool | `public bool NotTheBees;` | `public bool NotTheBees;` |
| 627 | field | Terraria.IO.WorldFileData | Terraria.IO/WorldFileData.cs | D:\TRbackup\Version4\Terraria.IO\WorldFileData.cs | 51 | 2 | ForTheWorthy | bool | `public bool ForTheWorthy;` | `public bool ForTheWorthy;` |
| 628 | field | Terraria.IO.WorldFileData | Terraria.IO/WorldFileData.cs | D:\TRbackup\Version4\Terraria.IO\WorldFileData.cs | 53 | 2 | Anniversary | bool | `public bool Anniversary;` | `public bool Anniversary;` |
| 629 | field | Terraria.IO.WorldFileData | Terraria.IO/WorldFileData.cs | D:\TRbackup\Version4\Terraria.IO\WorldFileData.cs | 55 | 2 | DontStarve | bool | `public bool DontStarve;` | `public bool DontStarve;` |
| 630 | field | Terraria.IO.WorldFileData | Terraria.IO/WorldFileData.cs | D:\TRbackup\Version4\Terraria.IO\WorldFileData.cs | 57 | 2 | RemixWorld | bool | `public bool RemixWorld;` | `public bool RemixWorld;` |
| 631 | field | Terraria.IO.WorldFileData | Terraria.IO/WorldFileData.cs | D:\TRbackup\Version4\Terraria.IO\WorldFileData.cs | 59 | 2 | NoTrapsWorld | bool | `public bool NoTrapsWorld;` | `public bool NoTrapsWorld;` |
| 632 | field | Terraria.IO.WorldFileData | Terraria.IO/WorldFileData.cs | D:\TRbackup\Version4\Terraria.IO\WorldFileData.cs | 61 | 2 | ZenithWorld | bool | `public bool ZenithWorld;` | `public bool ZenithWorld;` |
| 633 | field | Terraria.IO.WorldFileData | Terraria.IO/WorldFileData.cs | D:\TRbackup\Version4\Terraria.IO\WorldFileData.cs | 63 | 2 | SkyblockWorld | bool | `public bool SkyblockWorld;` | `public bool SkyblockWorld;` |
| 634 | field | Terraria.IO.WorldFileData | Terraria.IO/WorldFileData.cs | D:\TRbackup\Version4\Terraria.IO\WorldFileData.cs | 65 | 2 | HasCorruption | bool | `public bool HasCorruption = true;` | `public bool HasCorruption = true;` |
| 635 | field | Terraria.IO.WorldFileData | Terraria.IO/WorldFileData.cs | D:\TRbackup\Version4\Terraria.IO\WorldFileData.cs | 67 | 2 | IsHardMode | bool | `public bool IsHardMode;` | `public bool IsHardMode;` |
| 636 | field | Terraria.IO.WorldFileData | Terraria.IO/WorldFileData.cs | D:\TRbackup\Version4\Terraria.IO\WorldFileData.cs | 69 | 2 | DefeatedMoonlord | bool | `public bool DefeatedMoonlord;` | `public bool DefeatedMoonlord;` |

#### 属性（8）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 641 | property | Terraria.IO.WorldFileData | Terraria.IO/WorldFileData.cs | D:\TRbackup\Version4\Terraria.IO\WorldFileData.cs | 84 | 2 | SeedText | string | `public string SeedText => _seedText;` | `public string SeedText => _seedText;` |
| 642 | property | Terraria.IO.WorldFileData | Terraria.IO/WorldFileData.cs | D:\TRbackup\Version4\Terraria.IO\WorldFileData.cs | 86 | 2 | Seed | int | `public int Seed => _seed;` | `public int Seed => _seed;` |
| 643 | property | Terraria.IO.WorldFileData | Terraria.IO/WorldFileData.cs | D:\TRbackup\Version4\Terraria.IO\WorldFileData.cs | 88 | 2 | IsValid | bool | `public bool IsValid => LoadStatus == StatusID.Ok;` | `public bool IsValid => LoadStatus == StatusID.Ok;` |
| 644 | property | Terraria.IO.WorldFileData | Terraria.IO/WorldFileData.cs | D:\TRbackup\Version4\Terraria.IO\WorldFileData.cs | 90 | 2 | WorldSizeName | string | `public string WorldSizeName => _worldSizeName.Value;` | `public string WorldSizeName => _worldSizeName.Value;` |
| 645 | property | Terraria.IO.WorldFileData | Terraria.IO/WorldFileData.cs | D:\TRbackup\Version4\Terraria.IO\WorldFileData.cs | 92 | 2 | HasCrimson | bool | `public bool HasCrimson { get { return !HasCorruption; } set { HasCorruption = !value; } }` | `public bool HasCrimson { get { return !HasCorruption; } set { HasCorruption = !value; } }` |
| 646 | property | Terraria.IO.WorldFileData | Terraria.IO/WorldFileData.cs | D:\TRbackup\Version4\Terraria.IO\WorldFileData.cs | 104 | 2 | HasValidSeed | bool | `public bool HasValidSeed => WorldGeneratorVersion != 0;` | `public bool HasValidSeed => WorldGeneratorVersion != 0;` |
| 647 | property | Terraria.IO.WorldFileData | Terraria.IO/WorldFileData.cs | D:\TRbackup\Version4\Terraria.IO\WorldFileData.cs | 106 | 2 | UseGuidAsMapName | bool | `public bool UseGuidAsMapName => WorldGeneratorVersion >= 777389080577L;` | `public bool UseGuidAsMapName => WorldGeneratorVersion >= 777389080577L;` |
| 648 | property | Terraria.IO.WorldFileData | Terraria.IO/WorldFileData.cs | D:\TRbackup\Version4\Terraria.IO\WorldFileData.cs | 108 | 2 | MapFileName | string | `public string MapFileName { get { if (!UseGuidAsMapName) { return WorldId.ToString(); } return UniqueId.ToString(); } }` | `public string MapFileName { get { if (!UseGuidAsMapName) { return WorldId.ToString(); } return UniqueId.ToString(); } }` |


### 4.6 细分子系统：`WorldFileTileHeaderCoreState`

- 原报告章节：`4.2.4`
- 父级子系统：`PersistenceAndRecovery`
- 分区工作包：`14` / `持久化、恢复与配置`

- 上一级基线细分子系统：`WorldFileTilePacking`
- 细分职责：世界 Tile 核心压缩头位和基础布局。
- 边界角色：`adapter state`；最小 seam：world tile core header port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：19；属性：0；合计：19。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（19）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 547 | field | Terraria.IO.WorldFile.TilePacker | Terraria.IO/WorldFile.cs | D:\TRbackup\Version4\Terraria.IO\WorldFile.cs | 25 | 3 | Header1_1 | int | `public const int Header1_1 = 1;` | `public const int Header1_1 = 1;` |
| 548 | field | Terraria.IO.WorldFile.TilePacker | Terraria.IO/WorldFile.cs | D:\TRbackup\Version4\Terraria.IO\WorldFile.cs | 27 | 3 | Header1_2 | int | `public const int Header1_2 = 2;` | `public const int Header1_2 = 2;` |
| 549 | field | Terraria.IO.WorldFile.TilePacker | Terraria.IO/WorldFile.cs | D:\TRbackup\Version4\Terraria.IO\WorldFile.cs | 29 | 3 | Header1_4 | int | `public const int Header1_4 = 4;` | `public const int Header1_4 = 4;` |
| 550 | field | Terraria.IO.WorldFile.TilePacker | Terraria.IO/WorldFile.cs | D:\TRbackup\Version4\Terraria.IO\WorldFile.cs | 31 | 3 | Header1_8 | int | `public const int Header1_8 = 8;` | `public const int Header1_8 = 8;` |
| 551 | field | Terraria.IO.WorldFile.TilePacker | Terraria.IO/WorldFile.cs | D:\TRbackup\Version4\Terraria.IO\WorldFile.cs | 33 | 3 | Header1_10 | int | `public const int Header1_10 = 16;` | `public const int Header1_10 = 16;` |
| 552 | field | Terraria.IO.WorldFile.TilePacker | Terraria.IO/WorldFile.cs | D:\TRbackup\Version4\Terraria.IO\WorldFile.cs | 35 | 3 | Header1_18 | int | `public const int Header1_18 = 24;` | `public const int Header1_18 = 24;` |
| 553 | field | Terraria.IO.WorldFile.TilePacker | Terraria.IO/WorldFile.cs | D:\TRbackup\Version4\Terraria.IO\WorldFile.cs | 37 | 3 | Header1_20 | int | `public const int Header1_20 = 32;` | `public const int Header1_20 = 32;` |
| 554 | field | Terraria.IO.WorldFile.TilePacker | Terraria.IO/WorldFile.cs | D:\TRbackup\Version4\Terraria.IO\WorldFile.cs | 39 | 3 | Header1_40 | int | `public const int Header1_40 = 64;` | `public const int Header1_40 = 64;` |
| 555 | field | Terraria.IO.WorldFile.TilePacker | Terraria.IO/WorldFile.cs | D:\TRbackup\Version4\Terraria.IO\WorldFile.cs | 41 | 3 | Header1_80 | int | `public const int Header1_80 = 128;` | `public const int Header1_80 = 128;` |
| 556 | field | Terraria.IO.WorldFile.TilePacker | Terraria.IO/WorldFile.cs | D:\TRbackup\Version4\Terraria.IO\WorldFile.cs | 43 | 3 | Header1_C0 | int | `public const int Header1_C0 = 192;` | `public const int Header1_C0 = 192;` |
| 557 | field | Terraria.IO.WorldFile.TilePacker | Terraria.IO/WorldFile.cs | D:\TRbackup\Version4\Terraria.IO\WorldFile.cs | 45 | 3 | Header2_1 | int | `public const int Header2_1 = 1;` | `public const int Header2_1 = 1;` |
| 558 | field | Terraria.IO.WorldFile.TilePacker | Terraria.IO/WorldFile.cs | D:\TRbackup\Version4\Terraria.IO\WorldFile.cs | 47 | 3 | Header2_2 | int | `public const int Header2_2 = 2;` | `public const int Header2_2 = 2;` |
| 559 | field | Terraria.IO.WorldFile.TilePacker | Terraria.IO/WorldFile.cs | D:\TRbackup\Version4\Terraria.IO\WorldFile.cs | 49 | 3 | Header2_4 | int | `public const int Header2_4 = 4;` | `public const int Header2_4 = 4;` |
| 560 | field | Terraria.IO.WorldFile.TilePacker | Terraria.IO/WorldFile.cs | D:\TRbackup\Version4\Terraria.IO\WorldFile.cs | 51 | 3 | Header2_8 | int | `public const int Header2_8 = 8;` | `public const int Header2_8 = 8;` |
| 561 | field | Terraria.IO.WorldFile.TilePacker | Terraria.IO/WorldFile.cs | D:\TRbackup\Version4\Terraria.IO\WorldFile.cs | 53 | 3 | Header2_10 | int | `public const int Header2_10 = 16;` | `public const int Header2_10 = 16;` |
| 562 | field | Terraria.IO.WorldFile.TilePacker | Terraria.IO/WorldFile.cs | D:\TRbackup\Version4\Terraria.IO\WorldFile.cs | 55 | 3 | Header2_20 | int | `public const int Header2_20 = 32;` | `public const int Header2_20 = 32;` |
| 563 | field | Terraria.IO.WorldFile.TilePacker | Terraria.IO/WorldFile.cs | D:\TRbackup\Version4\Terraria.IO\WorldFile.cs | 57 | 3 | Header2_40 | int | `public const int Header2_40 = 64;` | `public const int Header2_40 = 64;` |
| 564 | field | Terraria.IO.WorldFile.TilePacker | Terraria.IO/WorldFile.cs | D:\TRbackup\Version4\Terraria.IO\WorldFile.cs | 59 | 3 | Header2_70 | int | `public const int Header2_70 = 112;` | `public const int Header2_70 = 112;` |
| 565 | field | Terraria.IO.WorldFile.TilePacker | Terraria.IO/WorldFile.cs | D:\TRbackup\Version4\Terraria.IO\WorldFile.cs | 61 | 3 | Header2_80 | int | `public const int Header2_80 = 128;` | `public const int Header2_80 = 128;` |

#### 属性（0）

无该类型成员记录。


### 4.7 细分子系统：`WorldFileTileHeaderExtensionState`

- 原报告章节：`4.2.5`
- 父级子系统：`PersistenceAndRecovery`
- 分区工作包：`14` / `持久化、恢复与配置`

- 上一级基线细分子系统：`WorldFileTilePacking`
- 细分职责：世界 Tile 扩展压缩头位和高阶标记布局。
- 边界角色：`adapter state`；最小 seam：world tile extension header port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：16；属性：0；合计：16。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（16）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 566 | field | Terraria.IO.WorldFile.TilePacker | Terraria.IO/WorldFile.cs | D:\TRbackup\Version4\Terraria.IO\WorldFile.cs | 63 | 3 | Header3_1 | int | `public const int Header3_1 = 1;` | `public const int Header3_1 = 1;` |
| 567 | field | Terraria.IO.WorldFile.TilePacker | Terraria.IO/WorldFile.cs | D:\TRbackup\Version4\Terraria.IO\WorldFile.cs | 65 | 3 | Header3_2 | int | `public const int Header3_2 = 2;` | `public const int Header3_2 = 2;` |
| 568 | field | Terraria.IO.WorldFile.TilePacker | Terraria.IO/WorldFile.cs | D:\TRbackup\Version4\Terraria.IO\WorldFile.cs | 67 | 3 | Header3_4 | int | `public const int Header3_4 = 4;` | `public const int Header3_4 = 4;` |
| 569 | field | Terraria.IO.WorldFile.TilePacker | Terraria.IO/WorldFile.cs | D:\TRbackup\Version4\Terraria.IO\WorldFile.cs | 69 | 3 | Header3_8 | int | `public const int Header3_8 = 8;` | `public const int Header3_8 = 8;` |
| 570 | field | Terraria.IO.WorldFile.TilePacker | Terraria.IO/WorldFile.cs | D:\TRbackup\Version4\Terraria.IO\WorldFile.cs | 71 | 3 | Header3_10 | int | `public const int Header3_10 = 16;` | `public const int Header3_10 = 16;` |
| 571 | field | Terraria.IO.WorldFile.TilePacker | Terraria.IO/WorldFile.cs | D:\TRbackup\Version4\Terraria.IO\WorldFile.cs | 73 | 3 | Header3_20 | int | `public const int Header3_20 = 32;` | `public const int Header3_20 = 32;` |
| 572 | field | Terraria.IO.WorldFile.TilePacker | Terraria.IO/WorldFile.cs | D:\TRbackup\Version4\Terraria.IO\WorldFile.cs | 75 | 3 | Header3_40 | int | `public const int Header3_40 = 64;` | `public const int Header3_40 = 64;` |
| 573 | field | Terraria.IO.WorldFile.TilePacker | Terraria.IO/WorldFile.cs | D:\TRbackup\Version4\Terraria.IO\WorldFile.cs | 77 | 3 | Header3_80 | int | `public const int Header3_80 = 128;` | `public const int Header3_80 = 128;` |
| 574 | field | Terraria.IO.WorldFile.TilePacker | Terraria.IO/WorldFile.cs | D:\TRbackup\Version4\Terraria.IO\WorldFile.cs | 79 | 3 | Header4_1 | int | `public const int Header4_1 = 1;` | `public const int Header4_1 = 1;` |
| 575 | field | Terraria.IO.WorldFile.TilePacker | Terraria.IO/WorldFile.cs | D:\TRbackup\Version4\Terraria.IO\WorldFile.cs | 81 | 3 | Header4_2 | int | `public const int Header4_2 = 2;` | `public const int Header4_2 = 2;` |
| 576 | field | Terraria.IO.WorldFile.TilePacker | Terraria.IO/WorldFile.cs | D:\TRbackup\Version4\Terraria.IO\WorldFile.cs | 83 | 3 | Header4_4 | int | `public const int Header4_4 = 4;` | `public const int Header4_4 = 4;` |
| 577 | field | Terraria.IO.WorldFile.TilePacker | Terraria.IO/WorldFile.cs | D:\TRbackup\Version4\Terraria.IO\WorldFile.cs | 85 | 3 | Header4_8 | int | `public const int Header4_8 = 8;` | `public const int Header4_8 = 8;` |
| 578 | field | Terraria.IO.WorldFile.TilePacker | Terraria.IO/WorldFile.cs | D:\TRbackup\Version4\Terraria.IO\WorldFile.cs | 87 | 3 | Header4_10 | int | `public const int Header4_10 = 16;` | `public const int Header4_10 = 16;` |
| 579 | field | Terraria.IO.WorldFile.TilePacker | Terraria.IO/WorldFile.cs | D:\TRbackup\Version4\Terraria.IO\WorldFile.cs | 89 | 3 | Header4_20 | int | `public const int Header4_20 = 32;` | `public const int Header4_20 = 32;` |
| 580 | field | Terraria.IO.WorldFile.TilePacker | Terraria.IO/WorldFile.cs | D:\TRbackup\Version4\Terraria.IO\WorldFile.cs | 91 | 3 | Header4_40 | int | `public const int Header4_40 = 64;` | `public const int Header4_40 = 64;` |
| 581 | field | Terraria.IO.WorldFile.TilePacker | Terraria.IO/WorldFile.cs | D:\TRbackup\Version4\Terraria.IO\WorldFile.cs | 93 | 3 | Header4_80 | int | `public const int Header4_80 = 128;` | `public const int Header4_80 = 128;` |

#### 属性（0）

无该类型成员记录。


### 4.8 细分子系统：`WorldFileRecoveryVersionState`

- 原报告章节：`4.2.6`
- 父级子系统：`PersistenceAndRecovery`
- 分区工作包：`14` / `持久化、恢复与配置`

- 上一级基线细分子系统：`WorldFileRecoveryIo`
- 细分职责：世界文件锁、版本和云端恢复异常状态。
- 边界角色：`adapter state`；最小 seam：world file recovery version port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：5；属性：0；合计：5。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（5）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 582 | field | Terraria.IO.WorldFile | Terraria.IO/WorldFile.cs | D:\TRbackup\Version4\Terraria.IO\WorldFile.cs | 96 | 2 | IOLock | object | `internal static readonly object IOLock = new object();` | `internal static readonly object IOLock = new object();` |
| 592 | field | Terraria.IO.WorldFile | Terraria.IO/WorldFile.cs | D:\TRbackup\Version4\Terraria.IO\WorldFile.cs | 116 | 2 | _versionNumber | int | `private static int _versionNumber;` | `private static int _versionNumber;` |
| 593 | field | Terraria.IO.WorldFile | Terraria.IO/WorldFile.cs | D:\TRbackup\Version4\Terraria.IO\WorldFile.cs | 118 | 2 | _isWorldOnCloud | bool | `private static bool _isWorldOnCloud;` | `private static bool _isWorldOnCloud;` |
| 608 | field | Terraria.IO.WorldFile | Terraria.IO/WorldFile.cs | D:\TRbackup\Version4\Terraria.IO\WorldFile.cs | 148 | 2 | LastThrownLoadException | System.Exception | `public static Exception LastThrownLoadException;` | `public static Exception LastThrownLoadException;` |
| 609 | field | Terraria.IO.WorldFile | Terraria.IO/WorldFile.cs | D:\TRbackup\Version4\Terraria.IO\WorldFile.cs | 150 | 2 | VersionNumberForChestRework | int | `private const int VersionNumberForChestRework = 294;` | `private const int VersionNumberForChestRework = 294;` |

#### 属性（0）

无该类型成员记录。


### 4.9 细分子系统：`WorldFileTemporaryEventState`

- 原报告章节：`4.2.7`
- 父级子系统：`PersistenceAndRecovery`
- 分区工作包：`14` / `持久化、恢复与配置`

- 上一级基线细分子系统：`WorldFileRecoveryIo`
- 细分职责：世界文件恢复期间的天气、节日和事件临时值。
- 边界角色：`adapter state`；最小 seam：world file temporary event port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：23；属性：0；合计：23。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（23）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 583 | field | Terraria.IO.WorldFile | Terraria.IO/WorldFile.cs | D:\TRbackup\Version4\Terraria.IO\WorldFile.cs | 98 | 2 | _tempTime | double | `private static double _tempTime = Main.time;` | `private static double _tempTime = Main.time;` |
| 584 | field | Terraria.IO.WorldFile | Terraria.IO/WorldFile.cs | D:\TRbackup\Version4\Terraria.IO\WorldFile.cs | 100 | 2 | _tempRaining | bool | `private static bool _tempRaining;` | `private static bool _tempRaining;` |
| 585 | field | Terraria.IO.WorldFile | Terraria.IO/WorldFile.cs | D:\TRbackup\Version4\Terraria.IO\WorldFile.cs | 102 | 2 | _tempMaxRain | float | `private static float _tempMaxRain;` | `private static float _tempMaxRain;` |
| 586 | field | Terraria.IO.WorldFile | Terraria.IO/WorldFile.cs | D:\TRbackup\Version4\Terraria.IO\WorldFile.cs | 104 | 2 | _tempRainTime | int | `private static int _tempRainTime;` | `private static int _tempRainTime;` |
| 587 | field | Terraria.IO.WorldFile | Terraria.IO/WorldFile.cs | D:\TRbackup\Version4\Terraria.IO\WorldFile.cs | 106 | 2 | _tempDayTime | bool | `private static bool _tempDayTime = Main.dayTime;` | `private static bool _tempDayTime = Main.dayTime;` |
| 588 | field | Terraria.IO.WorldFile | Terraria.IO/WorldFile.cs | D:\TRbackup\Version4\Terraria.IO\WorldFile.cs | 108 | 2 | _tempBloodMoon | bool | `private static bool _tempBloodMoon = Main.bloodMoon;` | `private static bool _tempBloodMoon = Main.bloodMoon;` |
| 589 | field | Terraria.IO.WorldFile | Terraria.IO/WorldFile.cs | D:\TRbackup\Version4\Terraria.IO\WorldFile.cs | 110 | 2 | _tempEclipse | bool | `private static bool _tempEclipse = Main.eclipse;` | `private static bool _tempEclipse = Main.eclipse;` |
| 590 | field | Terraria.IO.WorldFile | Terraria.IO/WorldFile.cs | D:\TRbackup\Version4\Terraria.IO\WorldFile.cs | 112 | 2 | _tempMoonPhase | int | `private static int _tempMoonPhase = Main.moonPhase;` | `private static int _tempMoonPhase = Main.moonPhase;` |
| 591 | field | Terraria.IO.WorldFile | Terraria.IO/WorldFile.cs | D:\TRbackup\Version4\Terraria.IO\WorldFile.cs | 114 | 2 | _tempCultistDelay | int | `private static int _tempCultistDelay = CultistRitual.delay;` | `private static int _tempCultistDelay = CultistRitual.delay;` |
| 594 | field | Terraria.IO.WorldFile | Terraria.IO/WorldFile.cs | D:\TRbackup\Version4\Terraria.IO\WorldFile.cs | 120 | 2 | _tempPartyGenuine | bool | `private static bool _tempPartyGenuine;` | `private static bool _tempPartyGenuine;` |
| 595 | field | Terraria.IO.WorldFile | Terraria.IO/WorldFile.cs | D:\TRbackup\Version4\Terraria.IO\WorldFile.cs | 122 | 2 | _tempPartyManual | bool | `private static bool _tempPartyManual;` | `private static bool _tempPartyManual;` |
| 596 | field | Terraria.IO.WorldFile | Terraria.IO/WorldFile.cs | D:\TRbackup\Version4\Terraria.IO\WorldFile.cs | 124 | 2 | _tempPartyCooldown | int | `private static int _tempPartyCooldown;` | `private static int _tempPartyCooldown;` |
| 597 | field | Terraria.IO.WorldFile | Terraria.IO/WorldFile.cs | D:\TRbackup\Version4\Terraria.IO\WorldFile.cs | 126 | 2 | TempPartyCelebratingNPCs | System.Collections.Generic.List<int> | `private static readonly List<int> TempPartyCelebratingNPCs = new List<int>();` | `private static readonly List<int> TempPartyCelebratingNPCs = new List<int>();` |
| 598 | field | Terraria.IO.WorldFile | Terraria.IO/WorldFile.cs | D:\TRbackup\Version4\Terraria.IO\WorldFile.cs | 128 | 2 | _tempSandstormHappening | bool | `private static bool _tempSandstormHappening;` | `private static bool _tempSandstormHappening;` |
| 599 | field | Terraria.IO.WorldFile | Terraria.IO/WorldFile.cs | D:\TRbackup\Version4\Terraria.IO\WorldFile.cs | 130 | 2 | _tempSandstormTimeLeft | int | `private static int _tempSandstormTimeLeft;` | `private static int _tempSandstormTimeLeft;` |
| 600 | field | Terraria.IO.WorldFile | Terraria.IO/WorldFile.cs | D:\TRbackup\Version4\Terraria.IO\WorldFile.cs | 132 | 2 | _tempSandstormSeverity | float | `private static float _tempSandstormSeverity;` | `private static float _tempSandstormSeverity;` |
| 601 | field | Terraria.IO.WorldFile | Terraria.IO/WorldFile.cs | D:\TRbackup\Version4\Terraria.IO\WorldFile.cs | 134 | 2 | _tempSandstormIntendedSeverity | float | `private static float _tempSandstormIntendedSeverity;` | `private static float _tempSandstormIntendedSeverity;` |
| 602 | field | Terraria.IO.WorldFile | Terraria.IO/WorldFile.cs | D:\TRbackup\Version4\Terraria.IO\WorldFile.cs | 136 | 2 | _tempLanternNightGenuine | bool | `private static bool _tempLanternNightGenuine;` | `private static bool _tempLanternNightGenuine;` |
| 603 | field | Terraria.IO.WorldFile | Terraria.IO/WorldFile.cs | D:\TRbackup\Version4\Terraria.IO\WorldFile.cs | 138 | 2 | _tempLanternNightManual | bool | `private static bool _tempLanternNightManual;` | `private static bool _tempLanternNightManual;` |
| 604 | field | Terraria.IO.WorldFile | Terraria.IO/WorldFile.cs | D:\TRbackup\Version4\Terraria.IO\WorldFile.cs | 140 | 2 | _tempLanternNightNextNightIsGenuine | bool | `private static bool _tempLanternNightNextNightIsGenuine;` | `private static bool _tempLanternNightNextNightIsGenuine;` |
| 605 | field | Terraria.IO.WorldFile | Terraria.IO/WorldFile.cs | D:\TRbackup\Version4\Terraria.IO\WorldFile.cs | 142 | 2 | _tempLanternNightCooldown | int | `private static int _tempLanternNightCooldown;` | `private static int _tempLanternNightCooldown;` |
| 606 | field | Terraria.IO.WorldFile | Terraria.IO/WorldFile.cs | D:\TRbackup\Version4\Terraria.IO\WorldFile.cs | 144 | 2 | _tempCoinRain | int | `private static int _tempCoinRain;` | `private static int _tempCoinRain;` |
| 607 | field | Terraria.IO.WorldFile | Terraria.IO/WorldFile.cs | D:\TRbackup\Version4\Terraria.IO\WorldFile.cs | 146 | 2 | _tempMeteorShowerCount | int | `private static int _tempMeteorShowerCount;` | `private static int _tempMeteorShowerCount;` |

#### 属性（0）

无该类型成员记录。


### 4.10 细分子系统：`SharedSaveAndConfigurationAdapters`

- 原报告章节：`4.9.57`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`14` / `持久化、恢复与配置`

- 上一级基线细分子系统：`SharedSaveAndConfigurationAdapters`
- 细分职责：存档元数据、收藏和配置文件适配。
- 边界角色：`adapter`；最小 seam：save/config file port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：5；声明类型数：5；字段：22；属性：3；合计：25。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（22）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2731 | field | Terraria.IO.FavoritesFile | Terraria.IO/FavoritesFile.cs | D:\TRbackup\Version4\Terraria.IO\FavoritesFile.cs | 12 | 2 | Path | string | `public readonly string Path;` | `public readonly string Path;` |
| 2732 | field | Terraria.IO.FavoritesFile | Terraria.IO/FavoritesFile.cs | D:\TRbackup\Version4\Terraria.IO\FavoritesFile.cs | 14 | 2 | IsCloudSave | bool | `public readonly bool IsCloudSave;` | `public readonly bool IsCloudSave;` |
| 2733 | field | Terraria.IO.FavoritesFile | Terraria.IO/FavoritesFile.cs | D:\TRbackup\Version4\Terraria.IO\FavoritesFile.cs | 16 | 2 | _data | System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, bool>> | `private Dictionary<string, Dictionary<string, bool>> _data = new Dictionary<string, Dictionary<string, bool>>();` | `private Dictionary<string, Dictionary<string, bool>> _data = new Dictionary<string, Dictionary<string, bool>>();` |
| 2734 | field | Terraria.IO.FavoritesFile | Terraria.IO/FavoritesFile.cs | D:\TRbackup\Version4\Terraria.IO\FavoritesFile.cs | 18 | 2 | _ourEncoder | System.Text.UTF8Encoding | `private UTF8Encoding _ourEncoder = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true, throwOnInvalidBytes: true);` | `private UTF8Encoding _ourEncoder = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true, throwOnInvalidBytes: true);` |
| 2735 | field | Terraria.IO.FileData | Terraria.IO/FileData.cs | D:\TRbackup\Version4\Terraria.IO\FileData.cs | 7 | 2 | _path | string | `protected string _path;` | `protected string _path;` |
| 2736 | field | Terraria.IO.FileData | Terraria.IO/FileData.cs | D:\TRbackup\Version4\Terraria.IO\FileData.cs | 9 | 2 | _isCloudSave | bool | `protected bool _isCloudSave;` | `protected bool _isCloudSave;` |
| 2737 | field | Terraria.IO.FileData | Terraria.IO/FileData.cs | D:\TRbackup\Version4\Terraria.IO\FileData.cs | 11 | 2 | Metadata | Terraria.IO.FileMetadata | `public FileMetadata Metadata;` | `public FileMetadata Metadata;` |
| 2738 | field | Terraria.IO.FileData | Terraria.IO/FileData.cs | D:\TRbackup\Version4\Terraria.IO\FileData.cs | 13 | 2 | Name | string | `public string Name;` | `public string Name;` |
| 2739 | field | Terraria.IO.FileData | Terraria.IO/FileData.cs | D:\TRbackup\Version4\Terraria.IO\FileData.cs | 15 | 2 | Type | string | `public readonly string Type;` | `public readonly string Type;` |
| 2740 | field | Terraria.IO.FileData | Terraria.IO/FileData.cs | D:\TRbackup\Version4\Terraria.IO\FileData.cs | 17 | 2 | _isFavorite | bool | `protected bool _isFavorite;` | `protected bool _isFavorite;` |
| 2741 | field | Terraria.IO.FileMetadata | Terraria.IO/FileMetadata.cs | D:\TRbackup\Version4\Terraria.IO\FileMetadata.cs | 8 | 2 | MAGIC_NUMBER | ulong | `public const ulong MAGIC_NUMBER = 27981915666277746uL;` | `public const ulong MAGIC_NUMBER = 27981915666277746uL;` |
| 2742 | field | Terraria.IO.FileMetadata | Terraria.IO/FileMetadata.cs | D:\TRbackup\Version4\Terraria.IO\FileMetadata.cs | 10 | 2 | SIZE | int | `public const int SIZE = 20;` | `public const int SIZE = 20;` |
| 2743 | field | Terraria.IO.FileMetadata | Terraria.IO/FileMetadata.cs | D:\TRbackup\Version4\Terraria.IO\FileMetadata.cs | 12 | 2 | Type | Terraria.IO.FileType | `public FileType Type;` | `public FileType Type;` |
| 2744 | field | Terraria.IO.FileMetadata | Terraria.IO/FileMetadata.cs | D:\TRbackup\Version4\Terraria.IO\FileMetadata.cs | 14 | 2 | Revision | uint | `public uint Revision;` | `public uint Revision;` |
| 2745 | field | Terraria.IO.FileMetadata | Terraria.IO/FileMetadata.cs | D:\TRbackup\Version4\Terraria.IO\FileMetadata.cs | 16 | 2 | IsFavorite | bool | `public bool IsFavorite;` | `public bool IsFavorite;` |
| 2746 | field | Terraria.IO.GameConfiguration | Terraria.IO/GameConfiguration.cs | D:\TRbackup\Version4\Terraria.IO\GameConfiguration.cs | 7 | 2 | _root | JObject | `private readonly JObject _root;` | `private readonly JObject _root;` |
| 2747 | field | Terraria.IO.Preferences | Terraria.IO/Preferences.cs | D:\TRbackup\Version4\Terraria.IO\Preferences.cs | 16 | 2 | _data | System.Collections.Generic.Dictionary<string, object> | `private Dictionary<string, object> _data = new Dictionary<string, object>();` | `private Dictionary<string, object> _data = new Dictionary<string, object>();` |
| 2748 | field | Terraria.IO.Preferences | Terraria.IO/Preferences.cs | D:\TRbackup\Version4\Terraria.IO\Preferences.cs | 18 | 2 | _path | string | `private readonly string _path;` | `private readonly string _path;` |
| 2749 | field | Terraria.IO.Preferences | Terraria.IO/Preferences.cs | D:\TRbackup\Version4\Terraria.IO\Preferences.cs | 20 | 2 | _serializerSettings | JsonSerializerSettings | `private readonly JsonSerializerSettings _serializerSettings;` | `private readonly JsonSerializerSettings _serializerSettings;` |
| 2750 | field | Terraria.IO.Preferences | Terraria.IO/Preferences.cs | D:\TRbackup\Version4\Terraria.IO\Preferences.cs | 22 | 2 | UseBson | bool | `public readonly bool UseBson;` | `public readonly bool UseBson;` |
| 2751 | field | Terraria.IO.Preferences | Terraria.IO/Preferences.cs | D:\TRbackup\Version4\Terraria.IO\Preferences.cs | 24 | 2 | _lock | object | `private readonly object _lock = new object();` | `private readonly object _lock = new object();` |
| 2752 | field | Terraria.IO.Preferences | Terraria.IO/Preferences.cs | D:\TRbackup\Version4\Terraria.IO\Preferences.cs | 26 | 2 | AutoSave | bool | `public bool AutoSave;` | `public bool AutoSave;` |

#### 属性（3）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3890 | property | Terraria.IO.FileData | Terraria.IO/FileData.cs | D:\TRbackup\Version4\Terraria.IO\FileData.cs | 19 | 2 | Path | string | `public string Path => _path;` | `public string Path => _path;` |
| 3891 | property | Terraria.IO.FileData | Terraria.IO/FileData.cs | D:\TRbackup\Version4\Terraria.IO\FileData.cs | 21 | 2 | IsCloudSave | bool | `public bool IsCloudSave => _isCloudSave;` | `public bool IsCloudSave => _isCloudSave;` |
| 3892 | property | Terraria.IO.FileData | Terraria.IO/FileData.cs | D:\TRbackup\Version4\Terraria.IO\FileData.cs | 23 | 2 | IsFavorite | bool | `public bool IsFavorite => _isFavorite;` | `public bool IsFavorite => _isFavorite;` |


### 4.11 细分子系统：`SharedGeneralFilePlatformUtilities`

- 原报告章节：`4.9.83`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`14` / `持久化、恢复与配置`

- 上一级基线细分子系统：`SharedGeneralFilePlatformUtilities`
- 细分职责：文件浏览、文件操作和运行时平台辅助工具。
- 边界角色：`adapter`；最小 seam：file platform utility port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：3；声明类型数：3；字段：4；属性：0；合计：4。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（4）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2949 | field | Terraria.Utilities.FileBrowser.ExtensionFilter | Terraria.Utilities.FileBrowser/ExtensionFilter.cs | D:\TRbackup\Version4\Terraria.Utilities.FileBrowser\ExtensionFilter.cs | 5 | 2 | Name | string | `public string Name;` | `public string Name;` |
| 2950 | field | Terraria.Utilities.FileBrowser.ExtensionFilter | Terraria.Utilities.FileBrowser/ExtensionFilter.cs | D:\TRbackup\Version4\Terraria.Utilities.FileBrowser\ExtensionFilter.cs | 7 | 2 | Extensions | string[] | `public string[] Extensions;` | `public string[] Extensions;` |
| 2960 | field | Terraria.Utilities.FileUtilities | Terraria.Utilities/FileUtilities.cs | D:\TRbackup\Version4\Terraria.Utilities\FileUtilities.cs | 12 | 2 | FileNameRegex | System.Text.RegularExpressions.Regex | `private static Regex FileNameRegex = new Regex("^(?<path>.*[\\\\\\/])?(?:$\|(?<fileName>.+?)(?:(?<extension>\\.[^.]*$)\|$))", RegexOptions.IgnoreCase \| RegexOptions.Compiled);` | `private static Regex FileNameRegex = new Regex("^(?<path>.*[\\\\\\/])?(?:$\|(?<fileName>.+?)(?:(?<extension>\\.[^.]*$)\|$))", RegexOptions.IgnoreCase \| RegexOptions.Compiled);` |
| 2964 | field | Terraria.Utilities.NewRuntimeMethods | Terraria.Utilities/NewRuntimeMethods.cs | D:\TRbackup\Version4\Terraria.Utilities\NewRuntimeMethods.cs | 8 | 2 | IsNet45OrNewer | bool | `private static bool IsNet45OrNewer = Type.GetType("System.Reflection.ReflectionContext", throwOnError: false) != null;` | `private static bool IsNet45OrNewer = Type.GetType("System.Reflection.ReflectionContext", throwOnError: false) != null;` |

#### 属性（0）

无该类型成员记录。


## 5. 追溯与验收

> 来源报告 SHA-256：`b944441d92d5103f218b766a3e354200794528a19e65a862e44c2b40a36f1196`
- 本分区细分组数：11；成员数：160；字段：146；属性：14。
- 预期不变量：本文件只出现完整细分子系统；不跨分区复制成员；所有成员的来源序号、原始声明、路径、行列和 C# 类型必须与来源报告一致。
- 验证方式：重新读取 20 个文件，检查文件数、细分组唯一性、来源序号恰好覆盖 `1..4542`、字段/属性合计和逐成员行文本一致；本次分区生成不运行 `dotnet`，因为没有修改 C# 或项目文件。

生成器：`Build/Tools/Generate-Version4NonAuthoritativeComponentPartitionReports.ps1`。
