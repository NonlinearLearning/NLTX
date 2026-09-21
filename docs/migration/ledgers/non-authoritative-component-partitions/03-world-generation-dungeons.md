# Version4 非权威组件拆分分区 03/20：世界生成与地牢

> 来源报告：`docs/migration/ledgers/Version4非权威模拟系统字段属性逐成员源码声明-更细子系统拆分-去除ID类文件.md`
> 来源报告 SHA-256：`b944441d92d5103f218b766a3e354200794528a19e65a862e44c2b40a36f1196`
> 本文件是并行组件拆分工作包，不是新的权威成员清单；成员声明事实直接保留来源报告原文。

## 1. 分区定位与评估

- 分区范围：世界生成、生物群系、地形 pass、地牢定义、房间、走廊和布局规则。
- 本分区组件化重点：区分 definition/catalog、生成过程状态、几何查询和结构变更命令。
- 本分区包含 35 个完整细分子系统、485 条成员记录（字段 456、属性 29）。来源序号覆盖区间 `153..3800`，区间可能与其他分区交错，不以序号定义领域边界。
- 组件边界判定：细分子系统只是并行处理的最小库存边界，不等价于一个组件。必须继续按共同读者/写者、变更原因、生命周期、权威性和副作用分为 Component、System、Query、Command、Adapter 或 Projection。
- 证据状态：成员声明和统计已由来源报告校验；逐成员读者、写者、初始化/清理路径、持久化/网络语义和系统顺序仍需本分区会话补证，不得从名称或数量推断。

## 2. 分区统计

| 父级子系统 | 细分子系统数 | 字段 | 属性 | 合计 |
|---|---:|---:|---:|---:|
| `RuntimeComposition` | 2 | 19 | 0 | 19 |
| `SharedRuntimeMechanisms` | 33 | 437 | 29 | 466 |

| 原报告章节 | 父级 | 细分子系统 | 边界角色 | 字段 | 属性 | 合计 | 组件化处理 |
|---|---|---|---|---:|---:|---:|---|
| `4.1.12` | `RuntimeComposition` | `MainGraphicsAndGenerationState` | presentation state | 12 | 0 | 12 | 待按成员访问模式拆分 |
| `4.1.40` | `RuntimeComposition` | `MainMenuAndWorldGenerationState` | session state | 7 | 0 | 7 | 待按成员访问模式拆分 |
| `4.9.3` | `SharedRuntimeMechanisms` | `SharedWorldGenerationSupport` | definition/query | 11 | 4 | 15 | 待按成员访问模式拆分 |
| `4.9.22` | `SharedRuntimeMechanisms` | `SharedDungeonEntranceDefinitions` | definition/catalog | 13 | 0 | 13 | 待按成员访问模式拆分 |
| `4.9.23` | `SharedRuntimeMechanisms` | `SharedDungeonFeatureDefinitions` | definition/catalog | 5 | 0 | 5 | 待按成员访问模式拆分 |
| `4.9.24` | `SharedRuntimeMechanisms` | `SharedDungeonLayoutProviderDefinitions` | definition/catalog | 17 | 0 | 17 | 待按成员访问模式拆分 |
| `4.9.62` | `SharedRuntimeMechanisms` | `SharedDungeonLayoutProviderState` | definition/state | 2 | 0 | 2 | 待按成员访问模式拆分 |
| `4.9.63` | `SharedRuntimeMechanisms` | `SharedDungeonCrawlerRuntimeState` | runtime state | 1 | 1 | 2 | 待按成员访问模式拆分 |
| `4.9.100` | `SharedRuntimeMechanisms` | `WorldSpawnConfigurationState` | definition/state | 12 | 0 | 12 | 待按成员访问模式拆分 |
| `4.9.116` | `SharedRuntimeMechanisms` | `SharedBiomeCaveHouseAndStructureState` | definition/query | 5 | 7 | 12 | 待按成员访问模式拆分 |
| `4.9.118` | `SharedRuntimeMechanisms` | `SharedDungeonStyleSetCatalog` | definition/catalog | 15 | 0 | 15 | 待按成员访问模式拆分 |
| `4.9.123` | `SharedRuntimeMechanisms` | `SharedDungeonBoundsAndProgressionDefinitions` | definition/query | 11 | 9 | 20 | 待按成员访问模式拆分 |
| `4.9.124` | `SharedRuntimeMechanisms` | `SharedDungeonDoorAndPlatformDefinitions` | definition/query | 24 | 1 | 25 | 待按成员访问模式拆分 |
| `4.9.127` | `SharedRuntimeMechanisms` | `SharedDungeonHallLegacyAndGeometry` | definition/catalog | 14 | 0 | 14 | 待按成员访问模式拆分 |
| `4.9.135` | `SharedRuntimeMechanisms` | `SharedDungeonGenerationCollectionsState` | runtime state | 19 | 1 | 20 | 待按成员访问模式拆分 |
| `4.9.136` | `SharedRuntimeMechanisms` | `SharedDungeonGenerationScalarAndStyleState` | runtime state | 17 | 0 | 17 | 待按成员访问模式拆分 |
| `4.9.137` | `SharedRuntimeMechanisms` | `SharedDungeonGeometryRuleQueries` | query | 8 | 0 | 8 | 待按成员访问模式拆分 |
| `4.9.155` | `SharedRuntimeMechanisms` | `SharedDungeonLegacyPlacementState` | runtime state | 17 | 0 | 17 | 待按成员访问模式拆分 |
| `4.9.156` | `SharedRuntimeMechanisms` | `SharedDungeonLegacyRuleState` | runtime state | 11 | 0 | 11 | 待按成员访问模式拆分 |
| `4.9.163` | `SharedRuntimeMechanisms` | `SharedDungeonRoomCoreState` | runtime state | 5 | 2 | 7 | 待按成员访问模式拆分 |
| `4.9.164` | `SharedRuntimeMechanisms` | `SharedDungeonRoomSettingsState` | definition/state | 25 | 0 | 25 | 待按成员访问模式拆分 |
| `4.9.165` | `SharedRuntimeMechanisms` | `SharedDungeonHallCoreState` | runtime state | 9 | 1 | 10 | 待按成员访问模式拆分 |
| `4.9.166` | `SharedRuntimeMechanisms` | `SharedDungeonHallSettingsState` | definition/state | 19 | 0 | 19 | 待按成员访问模式拆分 |
| `4.9.169` | `SharedRuntimeMechanisms` | `SharedBiomeTerrainPassState` | definition/query | 14 | 2 | 16 | 待按成员访问模式拆分 |
| `4.9.174` | `SharedRuntimeMechanisms` | `SharedDungeonStyleMaterialAndGeometryState` | definition/catalog | 12 | 0 | 12 | 待按成员访问模式拆分 |
| `4.9.177` | `SharedRuntimeMechanisms` | `SharedDungeonRoomVariantCatalogState` | definition/catalog | 8 | 0 | 8 | 待按成员访问模式拆分 |
| `4.9.184` | `SharedRuntimeMechanisms` | `SharedDungeonStyleObjectConstants` | query/value object | 21 | 0 | 21 | 待按成员访问模式拆分 |
| `4.9.185` | `SharedRuntimeMechanisms` | `SharedDungeonStyleBannerAndTrapConstants` | query/value object | 7 | 0 | 7 | 待按成员访问模式拆分 |
| `4.9.201` | `SharedRuntimeMechanisms` | `SharedDungeonTrapPlacementState` | definition/query | 22 | 0 | 22 | 待按成员访问模式拆分 |
| `4.9.202` | `SharedRuntimeMechanisms` | `SharedDungeonControlLineGeometryState` | definition/query | 20 | 1 | 21 | 待按成员访问模式拆分 |
| `4.9.210` | `SharedRuntimeMechanisms` | `SharedDungeonRoomShapeGeometryState` | definition/catalog | 13 | 0 | 13 | 待按成员访问模式拆分 |
| `4.9.211` | `SharedRuntimeMechanisms` | `SharedDungeonRoomPlacementGeometryState` | definition/catalog | 16 | 0 | 16 | 待按成员访问模式拆分 |
| `4.9.212` | `SharedRuntimeMechanisms` | `SharedDungeonStyleFurnitureCatalogState` | definition/catalog | 25 | 0 | 25 | 待按成员访问模式拆分 |
| `4.9.213` | `SharedRuntimeMechanisms` | `SharedDungeonStyleRoomVariantState` | definition/catalog | 2 | 0 | 2 | 待按成员访问模式拆分 |
| `4.9.216` | `SharedRuntimeMechanisms` | `SharedSceneBiomeZoneDefinitionState` | query/input | 17 | 0 | 17 | 待按成员访问模式拆分 |

## 3. 并行组件拆分契约

对本文件中的每个细分子系统和每条成员记录执行以下判断，并把结果写入对应的实现阶段设计/审计记录：

- 盘点声明类型、可见性、读者、写者、创建/更新/清理/持久化生命周期、访问频率和副作用；缺证据标记 `missing`，不可用命名猜测补齐。
- 标记权威状态、派生值、缓存、快照、兼容字段和表现数据；Component 只保存一个内聚概念的权威数据。
- 需要行为转换时放入显式 System/Command；纯资格判断和派生计算放入 Query；外部协议、文件、平台和第三方类型放入 Adapter；网络/持久化/日志/UI 输出放入单向 Projection。
- 分别建模实体引用、持久化 ID、网络 ID 和外部 ID；不得把 ID 类文件或外部对象直接塞入共享组件。
- 记录 Interface、Implementation、Seam、Depth、Leverage、Locality，以及必要的系统先后约束、失败归属和重试策略。

## 4. 逐成员源码声明

以下细分子系统块保持来源报告的成员表、声明、路径、行列和来源序号；只调整 Markdown 标题层级并增加分区追踪元数据。

### 4.1 细分子系统：`MainGraphicsAndGenerationState`

- 原报告章节：`4.1.12`
- 父级子系统：`RuntimeComposition`
- 分区工作包：`03` / `世界生成与地牢`

- 上一级基线细分子系统：`MainGraphicsAndGenerationState`
- 细分职责：图形设备、渲染计数、世界生成进度和保存计时引用。
- 边界角色：`presentation state`；最小 seam：graphics/generation adapter；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：12；属性：0；合计：12。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（12）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 153 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 467 | 2 | OurFavoriteColor | Microsoft.Xna.Framework.Color | `public static Microsoft.Xna.Framework.Color OurFavoriteColor = new Microsoft.Xna.Framework.Color(255, 231, 69);` | `public static Microsoft.Xna.Framework.Color OurFavoriteColor = new Microsoft.Xna.Framework.Color(255, 231, 69);` |
| 154 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 469 | 2 | mapEnabled | bool | `public static bool mapEnabled = true;` | `public static bool mapEnabled = true;` |
| 155 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 472 | 2 | IsEnginePreloaded | bool | `private static bool IsEnginePreloaded;` | `private static bool IsEnginePreloaded;` |
| 156 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 474 | 2 | _gameUpdateCount | uint | `private static uint _gameUpdateCount;` | `private static uint _gameUpdateCount;` |
| 157 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 476 | 2 | SkipAssemblyLoad | bool | `public static bool SkipAssemblyLoad;` | `public static bool SkipAssemblyLoad;` |
| 158 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 478 | 2 | renderCount | int | `public static int renderCount = 99;` | `public static int renderCount = 99;` |
| 159 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 481 | 2 | graphics | GraphicsDeviceManager | `public static GraphicsDeviceManager graphics;` | `public static GraphicsDeviceManager graphics;` |
| 160 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 484 | 2 | AutogenProgress | Terraria.WorldBuilding.GenerationProgress | `public static GenerationProgress AutogenProgress = new GenerationProgress();` | `public static GenerationProgress AutogenProgress = new GenerationProgress();` |
| 161 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 486 | 2 | saveTime | System.Diagnostics.Stopwatch | `private static Stopwatch saveTime = new Stopwatch();` | `private static Stopwatch saveTime = new Stopwatch();` |
| 162 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 489 | 2 | shimmerAlpha | float | `public static float shimmerAlpha = 0f;` | `public static float shimmerAlpha = 0f;` |
| 163 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 491 | 2 | shimmerDarken | float | `public static float shimmerDarken = 0f;` | `public static float shimmerDarken = 0f;` |
| 164 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 493 | 2 | afterPartyOfDoom | bool | `public static bool afterPartyOfDoom = false;` | `public static bool afterPartyOfDoom = false;` |

#### 属性（0）

无该类型成员记录。


### 4.2 细分子系统：`MainMenuAndWorldGenerationState`

- 原报告章节：`4.1.40`
- 父级子系统：`RuntimeComposition`
- 分区工作包：`03` / `世界生成与地牢`

- 上一级基线细分子系统：`MainMenuAndWorldGenerationState`
- 细分职责：菜单物品缩放、世界名称、环境风和自动通过状态。
- 边界角色：`session state`；最小 seam：menu/world-generation view；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：7；属性：0；合计：7。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（7）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 497 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1264 | 2 | ChumBucketProjectileHelper | Terraria.GameContent.ChumBucketProjectileHelper | `public ChumBucketProjectileHelper ChumBucketProjectileHelper = new ChumBucketProjectileHelper();` | `public ChumBucketProjectileHelper ChumBucketProjectileHelper = new ChumBucketProjectileHelper();` |
| 498 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1266 | 2 | maxMenuItems | int | `private static int maxMenuItems = 16;` | `private static int maxMenuItems = 16;` |
| 499 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1268 | 2 | menuItemScale | float[] | `private float[] menuItemScale = new float[maxMenuItems];` | `private float[] menuItemScale = new float[maxMenuItems];` |
| 500 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1270 | 2 | menuMode | int | `public static int menuMode;` | `public static int menuMode;` |
| 501 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1274 | 2 | newWorldName | string | `public static string newWorldName = "";` | `public static string newWorldName = "";` |
| 502 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1276 | 2 | _ambientWindSys | Terraria.GameContent.AmbientWindSystem | `private AmbientWindSystem _ambientWindSys = new AmbientWindSystem();` | `private AmbientWindSystem _ambientWindSys = new AmbientWindSystem();` |
| 503 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1279 | 2 | autoPass | bool | `public static bool autoPass;` | `public static bool autoPass;` |

#### 属性（0）

无该类型成员记录。


### 4.3 细分子系统：`SharedWorldGenerationSupport`

- 原报告章节：`4.9.3`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`03` / `世界生成与地牢`

- 上一级基线细分子系统：`SharedWorldGenerationSupport`
- 细分职责：生成形状、轨道、绘画和通用生成辅助。
- 边界角色：`definition/query`；最小 seam：generation helper view；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：5；声明类型数：6；字段：11；属性：4；合计：15。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（11）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1194 | field | Terraria.DataStructures.NoRoomCheckFeedback | Terraria.DataStructures/NoRoomCheckFeedback.cs | D:\TRbackup\Version4\Terraria.DataStructures\NoRoomCheckFeedback.cs | 5 | 2 | WithText | Terraria.DataStructures.NoRoomCheckFeedback | `public static NoRoomCheckFeedback WithText = new NoRoomCheckFeedback(displayText: true);` | `public static NoRoomCheckFeedback WithText = new NoRoomCheckFeedback(displayText: true);` |
| 1195 | field | Terraria.DataStructures.NoRoomCheckFeedback | Terraria.DataStructures/NoRoomCheckFeedback.cs | D:\TRbackup\Version4\Terraria.DataStructures\NoRoomCheckFeedback.cs | 7 | 2 | WithoutText | Terraria.DataStructures.NoRoomCheckFeedback | `public static NoRoomCheckFeedback WithoutText = new NoRoomCheckFeedback(displayText: false);` | `public static NoRoomCheckFeedback WithoutText = new NoRoomCheckFeedback(displayText: false);` |
| 2011 | field | Terraria.GameContent.Generation.PaintingEntry | Terraria.GameContent.Generation/PaintingEntry.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation\PaintingEntry.cs | 5 | 2 | tileType | int | `public int tileType;` | `public int tileType;` |
| 2012 | field | Terraria.GameContent.Generation.PaintingEntry | Terraria.GameContent.Generation/PaintingEntry.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation\PaintingEntry.cs | 7 | 2 | style | int | `public int style;` | `public int style;` |
| 2013 | field | Terraria.GameContent.Generation.ShapeFloodFill | Terraria.GameContent.Generation/ShapeFloodFill.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation\ShapeFloodFill.cs | 10 | 2 | _maximumActions | int | `private int _maximumActions;` | `private int _maximumActions;` |
| 2014 | field | Terraria.GameContent.Generation.TrackGenerator.TrackHistory | Terraria.GameContent.Generation/TrackGenerator.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation\TrackGenerator.cs | 29 | 3 | X | short | `public short X;` | `public short X;` |
| 2015 | field | Terraria.GameContent.Generation.TrackGenerator.TrackHistory | Terraria.GameContent.Generation/TrackGenerator.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation\TrackGenerator.cs | 31 | 3 | Y | short | `public short Y;` | `public short Y;` |
| 2016 | field | Terraria.GameContent.Generation.TrackGenerator.TrackHistory | Terraria.GameContent.Generation/TrackGenerator.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation\TrackGenerator.cs | 33 | 3 | Slope | Terraria.GameContent.Generation.TrackGenerator.TrackSlope | `public TrackSlope Slope;` | `public TrackSlope Slope;` |
| 2017 | field | Terraria.GameContent.Generation.TrackGenerator.TrackHistory | Terraria.GameContent.Generation/TrackGenerator.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation\TrackGenerator.cs | 35 | 3 | Mode | Terraria.GameContent.Generation.TrackGenerator.TrackMode | `public TrackMode Mode;` | `public TrackMode Mode;` |
| 2018 | field | Terraria.GameContent.Generation.TrackGenerator | Terraria.GameContent.Generation/TrackGenerator.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation\TrackGenerator.cs | 46 | 2 | _history | Terraria.GameContent.Generation.TrackGenerator.TrackHistory[] | `private readonly TrackHistory[] _history = new TrackHistory[4096];` | `private readonly TrackHistory[] _history = new TrackHistory[4096];` |
| 2019 | field | Terraria.GameContent.Generation.TrackGenerator | Terraria.GameContent.Generation/TrackGenerator.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation\TrackGenerator.cs | 48 | 2 | _rewriteHistory | Terraria.GameContent.Generation.TrackGenerator.TrackHistory[] | `private readonly TrackHistory[] _rewriteHistory = new TrackHistory[25];` | `private readonly TrackHistory[] _rewriteHistory = new TrackHistory[25];` |

#### 属性（4）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3691 | property | Terraria.DataStructures.IRoomCheckFeedback_Spread | Terraria.DataStructures/IRoomCheckFeedback_Spread.cs | D:\TRbackup\Version4\Terraria.DataStructures\IRoomCheckFeedback_Spread.cs | 5 | 2 | StopOnFail | bool | `bool StopOnFail { get; }` | `bool StopOnFail { get; }` |
| 3692 | property | Terraria.DataStructures.IRoomCheckFeedback_Spread | Terraria.DataStructures/IRoomCheckFeedback_Spread.cs | D:\TRbackup\Version4\Terraria.DataStructures\IRoomCheckFeedback_Spread.cs | 7 | 2 | DisplayText | bool | `bool DisplayText { get; }` | `bool DisplayText { get; }` |
| 3693 | property | Terraria.DataStructures.NoRoomCheckFeedback | Terraria.DataStructures/NoRoomCheckFeedback.cs | D:\TRbackup\Version4\Terraria.DataStructures\NoRoomCheckFeedback.cs | 9 | 2 | StopOnFail | bool | `public bool StopOnFail => true;` | `public bool StopOnFail => true;` |
| 3694 | property | Terraria.DataStructures.NoRoomCheckFeedback | Terraria.DataStructures/NoRoomCheckFeedback.cs | D:\TRbackup\Version4\Terraria.DataStructures\NoRoomCheckFeedback.cs | 11 | 2 | DisplayText | bool | `public bool DisplayText { get; private set; }` | `public bool DisplayText { get; private set; }` |


### 4.4 细分子系统：`SharedDungeonEntranceDefinitions`

- 原报告章节：`4.9.22`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`03` / `世界生成与地牢`

- 上一级基线细分子系统：`SharedDungeonEntranceDefinitions`
- 细分职责：地牢入口类型、预生成参数和入口设置定义。
- 边界角色：`definition/catalog`；最小 seam：dungeon entrance definition view；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：3；声明类型数：3；字段：13；属性：0；合计：13。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（13）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1675 | field | Terraria.GameContent.Generation.Dungeon.Entrances.DungeonEntrance | Terraria.GameContent.Generation.Dungeon.Entrances/DungeonEntrance.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Entrances\DungeonEntrance.cs | 8 | 2 | settings | Terraria.GameContent.Generation.Dungeon.Entrances.DungeonEntranceSettings | `public DungeonEntranceSettings settings;` | `public DungeonEntranceSettings settings;` |
| 1676 | field | Terraria.GameContent.Generation.Dungeon.Entrances.DungeonEntrance | Terraria.GameContent.Generation.Dungeon.Entrances/DungeonEntrance.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Entrances\DungeonEntrance.cs | 10 | 2 | calculated | bool | `public bool calculated;` | `public bool calculated;` |
| 1677 | field | Terraria.GameContent.Generation.Dungeon.Entrances.DungeonEntrance | Terraria.GameContent.Generation.Dungeon.Entrances/DungeonEntrance.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Entrances\DungeonEntrance.cs | 12 | 2 | generated | bool | `public bool generated;` | `public bool generated;` |
| 1678 | field | Terraria.GameContent.Generation.Dungeon.Entrances.DungeonEntrance | Terraria.GameContent.Generation.Dungeon.Entrances/DungeonEntrance.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Entrances\DungeonEntrance.cs | 14 | 2 | Bounds | Terraria.GameContent.Generation.Dungeon.DungeonBounds | `public DungeonBounds Bounds = new DungeonBounds();` | `public DungeonBounds Bounds = new DungeonBounds();` |
| 1679 | field | Terraria.GameContent.Generation.Dungeon.Entrances.DungeonEntrance | Terraria.GameContent.Generation.Dungeon.Entrances/DungeonEntrance.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Entrances\DungeonEntrance.cs | 16 | 2 | OldManSpawn | Point | `public Point OldManSpawn;` | `public Point OldManSpawn;` |
| 1680 | field | Terraria.GameContent.Generation.Dungeon.Entrances.DungeonEntranceSettings | Terraria.GameContent.Generation.Dungeon.Entrances/DungeonEntranceSettings.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Entrances\DungeonEntranceSettings.cs | 5 | 2 | EntranceType | Terraria.GameContent.Generation.Dungeon.Entrances.DungeonEntranceType | `public DungeonEntranceType EntranceType;` | `public DungeonEntranceType EntranceType;` |
| 1681 | field | Terraria.GameContent.Generation.Dungeon.Entrances.DungeonEntranceSettings | Terraria.GameContent.Generation.Dungeon.Entrances/DungeonEntranceSettings.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Entrances\DungeonEntranceSettings.cs | 7 | 2 | RandomSeed | int | `public int RandomSeed;` | `public int RandomSeed;` |
| 1682 | field | Terraria.GameContent.Generation.Dungeon.Entrances.DungeonEntranceSettings | Terraria.GameContent.Generation.Dungeon.Entrances/DungeonEntranceSettings.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Entrances\DungeonEntranceSettings.cs | 9 | 2 | StyleData | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyleData | `public DungeonGenerationStyleData StyleData;` | `public DungeonGenerationStyleData StyleData;` |
| 1683 | field | Terraria.GameContent.Generation.Dungeon.Entrances.DungeonEntranceSettings | Terraria.GameContent.Generation.Dungeon.Entrances/DungeonEntranceSettings.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Entrances\DungeonEntranceSettings.cs | 11 | 2 | PrecalculateEntrancePosition | bool | `public bool PrecalculateEntrancePosition;` | `public bool PrecalculateEntrancePosition;` |
| 1684 | field | Terraria.GameContent.Generation.Dungeon.Entrances.PreGenDungeonEntranceSettings | Terraria.GameContent.Generation.Dungeon.Entrances/PreGenDungeonEntranceSettings.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Entrances\PreGenDungeonEntranceSettings.cs | 5 | 2 | BuriedEntranceYOffset | int | `public int BuriedEntranceYOffset;` | `public int BuriedEntranceYOffset;` |
| 1685 | field | Terraria.GameContent.Generation.Dungeon.Entrances.PreGenDungeonEntranceSettings | Terraria.GameContent.Generation.Dungeon.Entrances/PreGenDungeonEntranceSettings.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Entrances\PreGenDungeonEntranceSettings.cs | 7 | 2 | BuriedEntranceSandDugoutYOffset | int | `public int BuriedEntranceSandDugoutYOffset;` | `public int BuriedEntranceSandDugoutYOffset;` |
| 1686 | field | Terraria.GameContent.Generation.Dungeon.Entrances.PreGenDungeonEntranceSettings | Terraria.GameContent.Generation.Dungeon.Entrances/PreGenDungeonEntranceSettings.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Entrances\PreGenDungeonEntranceSettings.cs | 9 | 2 | RoughHeight | int | `public int RoughHeight;` | `public int RoughHeight;` |
| 1687 | field | Terraria.GameContent.Generation.Dungeon.Entrances.PreGenDungeonEntranceSettings | Terraria.GameContent.Generation.Dungeon.Entrances/PreGenDungeonEntranceSettings.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Entrances\PreGenDungeonEntranceSettings.cs | 11 | 2 | BuryEntrance | bool | `public bool BuryEntrance;` | `public bool BuryEntrance;` |

#### 属性（0）

无该类型成员记录。


### 4.5 细分子系统：`SharedDungeonFeatureDefinitions`

- 原报告章节：`4.9.23`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`03` / `世界生成与地牢`

- 上一级基线细分子系统：`SharedDungeonFeatureDefinitions`
- 细分职责：地牢特征、全局家具、陷阱和装饰定义。
- 边界角色：`definition/catalog`；最小 seam：dungeon feature definition view；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：2；声明类型数：2；字段：5；属性：0；合计：5。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（5）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1688 | field | Terraria.GameContent.Generation.Dungeon.Features.DungeonFeature | Terraria.GameContent.Generation.Dungeon.Features/DungeonFeature.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Features\DungeonFeature.cs | 5 | 2 | settings | Terraria.GameContent.Generation.Dungeon.Features.DungeonFeatureSettings | `public DungeonFeatureSettings settings;` | `public DungeonFeatureSettings settings;` |
| 1689 | field | Terraria.GameContent.Generation.Dungeon.Features.DungeonFeature | Terraria.GameContent.Generation.Dungeon.Features/DungeonFeature.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Features\DungeonFeature.cs | 7 | 2 | Bounds | Terraria.GameContent.Generation.Dungeon.DungeonBounds | `public DungeonBounds Bounds = new DungeonBounds();` | `public DungeonBounds Bounds = new DungeonBounds();` |
| 1690 | field | Terraria.GameContent.Generation.Dungeon.Features.DungeonFeature | Terraria.GameContent.Generation.Dungeon.Features/DungeonFeature.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Features\DungeonFeature.cs | 9 | 2 | generated | bool | `public bool generated;` | `public bool generated;` |
| 1691 | field | Terraria.GameContent.Generation.Dungeon.Features.GlobalDungeonFeature | Terraria.GameContent.Generation.Dungeon.Features/GlobalDungeonFeature.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Features\GlobalDungeonFeature.cs | 5 | 2 | settings | Terraria.GameContent.Generation.Dungeon.Features.DungeonFeatureSettings | `public DungeonFeatureSettings settings;` | `public DungeonFeatureSettings settings;` |
| 1692 | field | Terraria.GameContent.Generation.Dungeon.Features.GlobalDungeonFeature | Terraria.GameContent.Generation.Dungeon.Features/GlobalDungeonFeature.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Features\GlobalDungeonFeature.cs | 7 | 2 | generated | bool | `public bool generated;` | `public bool generated;` |

#### 属性（0）

无该类型成员记录。


### 4.6 细分子系统：`SharedDungeonLayoutProviderDefinitions`

- 原报告章节：`4.9.24`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`03` / `世界生成与地牢`

- 上一级基线细分子系统：`SharedDungeonLayoutProviderDefinitions`
- 细分职责：地牢布局 provider、双地牢和旧布局 provider 设置定义。
- 边界角色：`definition/catalog`；最小 seam：dungeon provider definition view；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：2；声明类型数：4；字段：17；属性：0；合计：17。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（17）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1735 | field | Terraria.GameContent.Generation.Dungeon.LayoutProviders.DualDungeonLayoutProvider.HallwayCalculator.RoomEntry | Terraria.GameContent.Generation.Dungeon.LayoutProviders/DualDungeonLayoutProvider.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.LayoutProviders\DualDungeonLayoutProvider.cs | 21 | 4 | room | Terraria.GameContent.Generation.Dungeon.Rooms.DungeonRoom | `public DungeonRoom room;` | `public DungeonRoom room;` |
| 1736 | field | Terraria.GameContent.Generation.Dungeon.LayoutProviders.DualDungeonLayoutProvider.HallwayCalculator.RoomEntry | Terraria.GameContent.Generation.Dungeon.LayoutProviders/DualDungeonLayoutProvider.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.LayoutProviders\DualDungeonLayoutProvider.cs | 23 | 4 | progressAlongSnake | double | `public double progressAlongSnake;` | `public double progressAlongSnake;` |
| 1737 | field | Terraria.GameContent.Generation.Dungeon.LayoutProviders.DualDungeonLayoutProvider.HallwayCalculator.RoomEntry | Terraria.GameContent.Generation.Dungeon.LayoutProviders/DualDungeonLayoutProvider.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.LayoutProviders\DualDungeonLayoutProvider.cs | 25 | 4 | backLinks | System.Collections.Generic.List<Terraria.GameContent.Generation.Dungeon.LayoutProviders.DualDungeonLayoutProvider.HallwayCalculator.RoomEntry> | `public List<RoomEntry> backLinks = new List<RoomEntry>();` | `public List<RoomEntry> backLinks = new List<RoomEntry>();` |
| 1738 | field | Terraria.GameContent.Generation.Dungeon.LayoutProviders.DualDungeonLayoutProvider.HallwayCalculator.RoomEntry | Terraria.GameContent.Generation.Dungeon.LayoutProviders/DualDungeonLayoutProvider.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.LayoutProviders\DualDungeonLayoutProvider.cs | 27 | 4 | forwardLinks | System.Collections.Generic.List<Terraria.GameContent.Generation.Dungeon.LayoutProviders.DualDungeonLayoutProvider.HallwayCalculator.RoomEntry> | `public List<RoomEntry> forwardLinks = new List<RoomEntry>();` | `public List<RoomEntry> forwardLinks = new List<RoomEntry>();` |
| 1739 | field | Terraria.GameContent.Generation.Dungeon.LayoutProviders.DualDungeonLayoutProvider.HallwayCalculator.HallLine | Terraria.GameContent.Generation.Dungeon.LayoutProviders/DualDungeonLayoutProvider.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.LayoutProviders\DualDungeonLayoutProvider.cs | 32 | 4 | source | Terraria.GameContent.Generation.Dungeon.LayoutProviders.DualDungeonLayoutProvider.HallwayCalculator.RoomEntry | `public RoomEntry source;` | `public RoomEntry source;` |
| 1740 | field | Terraria.GameContent.Generation.Dungeon.LayoutProviders.DualDungeonLayoutProvider.HallwayCalculator.HallLine | Terraria.GameContent.Generation.Dungeon.LayoutProviders/DualDungeonLayoutProvider.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.LayoutProviders\DualDungeonLayoutProvider.cs | 34 | 4 | target | Terraria.GameContent.Generation.Dungeon.LayoutProviders.DualDungeonLayoutProvider.HallwayCalculator.RoomEntry | `public RoomEntry target;` | `public RoomEntry target;` |
| 1741 | field | Terraria.GameContent.Generation.Dungeon.LayoutProviders.DualDungeonLayoutProvider.HallwayCalculator.HallLine | Terraria.GameContent.Generation.Dungeon.LayoutProviders/DualDungeonLayoutProvider.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.LayoutProviders\DualDungeonLayoutProvider.cs | 36 | 4 | sourcePoint | Vector2D | `public Vector2D sourcePoint;` | `public Vector2D sourcePoint;` |
| 1742 | field | Terraria.GameContent.Generation.Dungeon.LayoutProviders.DualDungeonLayoutProvider.HallwayCalculator.HallLine | Terraria.GameContent.Generation.Dungeon.LayoutProviders/DualDungeonLayoutProvider.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.LayoutProviders\DualDungeonLayoutProvider.cs | 38 | 4 | targetPoint | Vector2D | `public Vector2D targetPoint;` | `public Vector2D targetPoint;` |
| 1743 | field | Terraria.GameContent.Generation.Dungeon.LayoutProviders.DualDungeonLayoutProvider.HallwayCalculator | Terraria.GameContent.Generation.Dungeon.LayoutProviders/DualDungeonLayoutProvider.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.LayoutProviders\DualDungeonLayoutProvider.cs | 41 | 3 | data | Terraria.GameContent.Generation.Dungeon.DungeonData | `private readonly DungeonData data;` | `private readonly DungeonData data;` |
| 1744 | field | Terraria.GameContent.Generation.Dungeon.LayoutProviders.DualDungeonLayoutProvider.HallwayCalculator | Terraria.GameContent.Generation.Dungeon.LayoutProviders/DualDungeonLayoutProvider.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.LayoutProviders\DualDungeonLayoutProvider.cs | 43 | 3 | rooms | System.Collections.Generic.List<Terraria.GameContent.Generation.Dungeon.LayoutProviders.DualDungeonLayoutProvider.HallwayCalculator.RoomEntry> | `private readonly List<RoomEntry> rooms;` | `private readonly List<RoomEntry> rooms;` |
| 1745 | field | Terraria.GameContent.Generation.Dungeon.LayoutProviders.DualDungeonLayoutProvider.HallwayCalculator | Terraria.GameContent.Generation.Dungeon.LayoutProviders/DualDungeonLayoutProvider.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.LayoutProviders\DualDungeonLayoutProvider.cs | 45 | 3 | halls | System.Collections.Generic.List<Terraria.GameContent.Generation.Dungeon.Halls.DungeonHall> | `private readonly List<DungeonHall> halls = new List<DungeonHall>();` | `private readonly List<DungeonHall> halls = new List<DungeonHall>();` |
| 1746 | field | Terraria.GameContent.Generation.Dungeon.LayoutProviders.DualDungeonLayoutProvider.HallwayCalculator | Terraria.GameContent.Generation.Dungeon.LayoutProviders/DualDungeonLayoutProvider.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.LayoutProviders\DualDungeonLayoutProvider.cs | 47 | 3 | stairwells | System.Collections.Generic.List<Terraria.GameContent.Generation.Dungeon.LayoutProviders.DualDungeonLayoutProvider.HallwayCalculator.HallLine> | `private readonly List<HallLine> stairwells = new List<HallLine>();` | `private readonly List<HallLine> stairwells = new List<HallLine>();` |
| 1747 | field | Terraria.GameContent.Generation.Dungeon.LayoutProviders.DualDungeonLayoutProvider.HallwayCalculator | Terraria.GameContent.Generation.Dungeon.LayoutProviders/DualDungeonLayoutProvider.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.LayoutProviders\DualDungeonLayoutProvider.cs | 49 | 3 | controlLines | Terraria.GameContent.Biomes.DitherSnake | `private readonly DitherSnake controlLines;` | `private readonly DitherSnake controlLines;` |
| 1748 | field | Terraria.GameContent.Generation.Dungeon.LayoutProviders.DualDungeonLayoutProvider.HallwayCalculator | Terraria.GameContent.Generation.Dungeon.LayoutProviders/DualDungeonLayoutProvider.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.LayoutProviders\DualDungeonLayoutProvider.cs | 51 | 3 | maxProgressDelta | double | `private readonly double maxProgressDelta;` | `private readonly double maxProgressDelta;` |
| 1749 | field | Terraria.GameContent.Generation.Dungeon.LayoutProviders.DualDungeonLayoutProvider.HallwayCalculator | Terraria.GameContent.Generation.Dungeon.LayoutProviders/DualDungeonLayoutProvider.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.LayoutProviders\DualDungeonLayoutProvider.cs | 53 | 3 | avgLineLength | double | `private readonly double avgLineLength;` | `private readonly double avgLineLength;` |
| 1750 | field | Terraria.GameContent.Generation.Dungeon.LayoutProviders.LegacyDungeonLayoutProviderSettings | Terraria.GameContent.Generation.Dungeon.LayoutProviders/LegacyDungeonLayoutProviderSettings.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.LayoutProviders\LegacyDungeonLayoutProviderSettings.cs | 5 | 2 | Steps | int | `public int Steps;` | `public int Steps;` |
| 1751 | field | Terraria.GameContent.Generation.Dungeon.LayoutProviders.LegacyDungeonLayoutProviderSettings | Terraria.GameContent.Generation.Dungeon.LayoutProviders/LegacyDungeonLayoutProviderSettings.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.LayoutProviders\LegacyDungeonLayoutProviderSettings.cs | 7 | 2 | MaxSteps | int | `public int MaxSteps;` | `public int MaxSteps;` |

#### 属性（0）

无该类型成员记录。


### 4.7 细分子系统：`SharedDungeonLayoutProviderState`

- 原报告章节：`4.9.62`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`03` / `世界生成与地牢`

- 上一级基线细分子系统：`SharedDungeonLayoutProviderState`
- 细分职责：根级地牢布局 provider 及其设置状态。
- 边界角色：`definition/state`；最小 seam：dungeon layout provider port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：2；声明类型数：2；字段：2；属性：0；合计：2。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（2）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1959 | field | Terraria.GameContent.Generation.Dungeon.DungeonLayoutProvider | Terraria.GameContent.Generation.Dungeon/DungeonLayoutProvider.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonLayoutProvider.cs | 8 | 2 | settings | Terraria.GameContent.Generation.Dungeon.DungeonLayoutProviderSettings | `public DungeonLayoutProviderSettings settings;` | `public DungeonLayoutProviderSettings settings;` |
| 1960 | field | Terraria.GameContent.Generation.Dungeon.DungeonLayoutProviderSettings | Terraria.GameContent.Generation.Dungeon/DungeonLayoutProviderSettings.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonLayoutProviderSettings.cs | 5 | 2 | StyleData | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyleData | `public DungeonGenerationStyleData StyleData;` | `public DungeonGenerationStyleData StyleData;` |

#### 属性（0）

无该类型成员记录。


### 4.8 细分子系统：`SharedDungeonCrawlerRuntimeState`

- 原报告章节：`4.9.63`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`03` / `世界生成与地牢`

- 上一级基线细分子系统：`SharedDungeonCrawlerRuntimeState`
- 细分职责：地牢爬行器当前数据和生成运行状态。
- 边界角色：`runtime state`；最小 seam：dungeon crawler runtime port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：1；属性：1；合计：2。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（1）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1830 | field | Terraria.GameContent.Generation.Dungeon.DungeonCrawler | Terraria.GameContent.Generation.Dungeon/DungeonCrawler.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonCrawler.cs | 20 | 2 | dungeonData | System.Collections.Generic.List<Terraria.GameContent.Generation.Dungeon.DungeonData> | `public static List<DungeonData> dungeonData = new List<DungeonData>();` | `public static List<DungeonData> dungeonData = new List<DungeonData>();` |

#### 属性（1）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3798 | property | Terraria.GameContent.Generation.Dungeon.DungeonCrawler | Terraria.GameContent.Generation.Dungeon/DungeonCrawler.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonCrawler.cs | 22 | 2 | CurrentDungeonData | Terraria.GameContent.Generation.Dungeon.DungeonData | `public static DungeonData CurrentDungeonData { get { return dungeonData[GenVars.CurrentDungeon]; } set { dungeonData[GenVars.CurrentDungeon] = value; } }` | `public static DungeonData CurrentDungeonData { get { return dungeonData[GenVars.CurrentDungeon]; } set { dungeonData[GenVars.CurrentDungeon] = value; } }` |


### 4.9 细分子系统：`WorldSpawnConfigurationState`

- 原报告章节：`4.9.100`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`03` / `世界生成与地牢`

- 上一级基线细分子系统：`WorldSpawnConfigurationState`
- 细分职责：刷怪参数、额外出生点和 NPC 生成配置。
- 边界角色：`definition/state`；最小 seam：world spawn configuration port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：3；声明类型数：3；字段：12；属性：0；合计：12。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（12）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2410 | field | Terraria.GameContent.ExtraSpawnPointManager | Terraria.GameContent/ExtraSpawnPointManager.cs | D:\TRbackup\Version4\Terraria.GameContent\ExtraSpawnPointManager.cs | 12 | 2 | extraSpawnPoints | Point[] | `public static Point[] extraSpawnPoints = new Point[0];` | `public static Point[] extraSpawnPoints = new Point[0];` |
| 2411 | field | Terraria.GameContent.ExtraSpawnPointManager | Terraria.GameContent/ExtraSpawnPointManager.cs | D:\TRbackup\Version4\Terraria.GameContent\ExtraSpawnPointManager.cs | 14 | 2 | settings | Terraria.GameContent.ExtraSpawnSettings | `public static ExtraSpawnSettings settings = default(ExtraSpawnSettings);` | `public static ExtraSpawnSettings settings = default(ExtraSpawnSettings);` |
| 2412 | field | Terraria.GameContent.ExtraSpawnPointManager | Terraria.GameContent/ExtraSpawnPointManager.cs | D:\TRbackup\Version4\Terraria.GameContent\ExtraSpawnPointManager.cs | 16 | 2 | _listOfLandmasses | System.Collections.Generic.List<Terraria.WorldBuilding.LandmassData> | `private static List<LandmassData> _listOfLandmasses = new List<LandmassData>();` | `private static List<LandmassData> _listOfLandmasses = new List<LandmassData>();` |
| 2413 | field | Terraria.GameContent.ExtraSpawnSettings | Terraria.GameContent/ExtraSpawnSettings.cs | D:\TRbackup\Version4\Terraria.GameContent\ExtraSpawnSettings.cs | 5 | 2 | spawnType | Terraria.GameContent.ExtraSpawnType | `public ExtraSpawnType spawnType;` | `public ExtraSpawnType spawnType;` |
| 2414 | field | Terraria.GameContent.ExtraSpawnSettings | Terraria.GameContent/ExtraSpawnSettings.cs | D:\TRbackup\Version4\Terraria.GameContent\ExtraSpawnSettings.cs | 7 | 2 | surface | bool | `public bool surface;` | `public bool surface;` |
| 2415 | field | Terraria.GameContent.ExtraSpawnSettings | Terraria.GameContent/ExtraSpawnSettings.cs | D:\TRbackup\Version4\Terraria.GameContent\ExtraSpawnSettings.cs | 9 | 2 | remix | bool | `public bool remix;` | `public bool remix;` |
| 2416 | field | Terraria.GameContent.ExtraSpawnSettings | Terraria.GameContent/ExtraSpawnSettings.cs | D:\TRbackup\Version4\Terraria.GameContent\ExtraSpawnSettings.cs | 11 | 2 | roundLandmass | bool | `public bool roundLandmass;` | `public bool roundLandmass;` |
| 2417 | field | Terraria.GameContent.ExtraSpawnSettings | Terraria.GameContent/ExtraSpawnSettings.cs | D:\TRbackup\Version4\Terraria.GameContent\ExtraSpawnSettings.cs | 13 | 2 | skyblock | bool | `public bool skyblock;` | `public bool skyblock;` |
| 2418 | field | Terraria.GameContent.ExtraSpawnSettings | Terraria.GameContent/ExtraSpawnSettings.cs | D:\TRbackup\Version4\Terraria.GameContent\ExtraSpawnSettings.cs | 15 | 2 | extraLiquid | bool | `public bool extraLiquid;` | `public bool extraLiquid;` |
| 3349 | field | Terraria.NPCSpawnParams | Terraria/NPCSpawnParams.cs | D:\TRbackup\Version4\Terraria\NPCSpawnParams.cs | 5 | 2 | sizeScaleOverride | float? | `public float? sizeScaleOverride;` | `public float? sizeScaleOverride;` |
| 3350 | field | Terraria.NPCSpawnParams | Terraria/NPCSpawnParams.cs | D:\TRbackup\Version4\Terraria\NPCSpawnParams.cs | 7 | 2 | playerCountForMultiplayerDifficultyOverride | int? | `public int? playerCountForMultiplayerDifficultyOverride;` | `public int? playerCountForMultiplayerDifficultyOverride;` |
| 3351 | field | Terraria.NPCSpawnParams | Terraria/NPCSpawnParams.cs | D:\TRbackup\Version4\Terraria\NPCSpawnParams.cs | 9 | 2 | difficultyOverride | float? | `public float? difficultyOverride;` | `public float? difficultyOverride;` |

#### 属性（0）

无该类型成员记录。


### 4.10 细分子系统：`SharedBiomeCaveHouseAndStructureState`

- 原报告章节：`4.9.116`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`03` / `世界生成与地牢`

- 上一级基线细分子系统：`SharedWorldGenerationBiomeSupport`
- 细分职责：洞穴房屋、结构放置和洞穴生物群落辅助数据。
- 边界角色：`definition/query`；最小 seam：cave house structure generation port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：3；声明类型数：3；字段：5；属性：7；合计：12。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（5）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1399 | field | Terraria.GameContent.Biomes.CaveHouse.HouseBuilderContext | Terraria.GameContent.Biomes.CaveHouse/HouseBuilderContext.cs | D:\TRbackup\Version4\Terraria.GameContent.Biomes.CaveHouse\HouseBuilderContext.cs | 5 | 2 | SharpenerCount | int | `public int SharpenerCount;` | `public int SharpenerCount;` |
| 1400 | field | Terraria.GameContent.Biomes.CaveHouse.HouseBuilderContext | Terraria.GameContent.Biomes.CaveHouse/HouseBuilderContext.cs | D:\TRbackup\Version4\Terraria.GameContent.Biomes.CaveHouse\HouseBuilderContext.cs | 7 | 2 | ExtractinatorCount | int | `public int ExtractinatorCount;` | `public int ExtractinatorCount;` |
| 1401 | field | Terraria.GameContent.Biomes.CaveHouse.HouseUtils | Terraria.GameContent.Biomes.CaveHouse/HouseUtils.cs | D:\TRbackup\Version4\Terraria.GameContent.Biomes.CaveHouse\HouseUtils.cs | 11 | 2 | BlacklistedTiles | bool[] | `private static readonly bool[] BlacklistedTiles = TileID.Sets.Factory.CreateBoolSet(true, 225, 41, 43, 44, 226, 203, 112, 25, 151, 21, 467);` | `private static readonly bool[] BlacklistedTiles = TileID.Sets.Factory.CreateBoolSet(true, 225, 41, 43, 44, 226, 203, 112, 25, 151, 21, 467);` |
| 1402 | field | Terraria.GameContent.Biomes.CaveHouse.HouseUtils | Terraria.GameContent.Biomes.CaveHouse/HouseUtils.cs | D:\TRbackup\Version4\Terraria.GameContent.Biomes.CaveHouse\HouseUtils.cs | 13 | 2 | BeelistedTiles | bool[] | `private static readonly bool[] BeelistedTiles = TileID.Sets.Factory.CreateBoolSet(true, 41, 43, 44, 226, 203, 112, 25, 151, 21, 467);` | `private static readonly bool[] BeelistedTiles = TileID.Sets.Factory.CreateBoolSet(true, 41, 43, 44, 226, 203, 112, 25, 151, 21, 467);` |
| 1403 | field | Terraria.GameContent.Biomes.CaveHouseBiome | Terraria.GameContent.Biomes/CaveHouseBiome.cs | D:\TRbackup\Version4\Terraria.GameContent.Biomes\CaveHouseBiome.cs | 11 | 2 | _builderContext | Terraria.GameContent.Biomes.CaveHouse.HouseBuilderContext | `private readonly HouseBuilderContext _builderContext = new HouseBuilderContext();` | `private readonly HouseBuilderContext _builderContext = new HouseBuilderContext();` |

#### 属性（7）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3738 | property | Terraria.GameContent.Biomes.CaveHouseBiome | Terraria.GameContent.Biomes/CaveHouseBiome.cs | D:\TRbackup\Version4\Terraria.GameContent.Biomes\CaveHouseBiome.cs | 13 | 2 | IceChestChance | double | `[JsonProperty] public double IceChestChance { get; set; }` | `[JsonProperty] public double IceChestChance { get; set; }` |
| 3739 | property | Terraria.GameContent.Biomes.CaveHouseBiome | Terraria.GameContent.Biomes/CaveHouseBiome.cs | D:\TRbackup\Version4\Terraria.GameContent.Biomes\CaveHouseBiome.cs | 16 | 2 | JungleChestChance | double | `[JsonProperty] public double JungleChestChance { get; set; }` | `[JsonProperty] public double JungleChestChance { get; set; }` |
| 3740 | property | Terraria.GameContent.Biomes.CaveHouseBiome | Terraria.GameContent.Biomes/CaveHouseBiome.cs | D:\TRbackup\Version4\Terraria.GameContent.Biomes\CaveHouseBiome.cs | 19 | 2 | GoldChestChance | double | `[JsonProperty] public double GoldChestChance { get; set; }` | `[JsonProperty] public double GoldChestChance { get; set; }` |
| 3741 | property | Terraria.GameContent.Biomes.CaveHouseBiome | Terraria.GameContent.Biomes/CaveHouseBiome.cs | D:\TRbackup\Version4\Terraria.GameContent.Biomes\CaveHouseBiome.cs | 22 | 2 | GraniteChestChance | double | `[JsonProperty] public double GraniteChestChance { get; set; }` | `[JsonProperty] public double GraniteChestChance { get; set; }` |
| 3742 | property | Terraria.GameContent.Biomes.CaveHouseBiome | Terraria.GameContent.Biomes/CaveHouseBiome.cs | D:\TRbackup\Version4\Terraria.GameContent.Biomes\CaveHouseBiome.cs | 25 | 2 | MarbleChestChance | double | `[JsonProperty] public double MarbleChestChance { get; set; }` | `[JsonProperty] public double MarbleChestChance { get; set; }` |
| 3743 | property | Terraria.GameContent.Biomes.CaveHouseBiome | Terraria.GameContent.Biomes/CaveHouseBiome.cs | D:\TRbackup\Version4\Terraria.GameContent.Biomes\CaveHouseBiome.cs | 28 | 2 | MushroomChestChance | double | `[JsonProperty] public double MushroomChestChance { get; set; }` | `[JsonProperty] public double MushroomChestChance { get; set; }` |
| 3744 | property | Terraria.GameContent.Biomes.CaveHouseBiome | Terraria.GameContent.Biomes/CaveHouseBiome.cs | D:\TRbackup\Version4\Terraria.GameContent.Biomes\CaveHouseBiome.cs | 31 | 2 | DesertChestChance | double | `[JsonProperty] public double DesertChestChance { get; set; }` | `[JsonProperty] public double DesertChestChance { get; set; }` |


### 4.11 细分子系统：`SharedDungeonStyleSetCatalog`

- 原报告章节：`4.9.118`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`03` / `世界生成与地牢`

- 上一级基线细分子系统：`SharedDungeonStyleCatalog`
- 细分职责：地牢样式集合和生物群落样式索引。
- 边界角色：`definition/catalog`；最小 seam：dungeon style set catalog port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：15；属性：0；合计：15。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（15）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1916 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyles | Terraria.GameContent.Generation.Dungeon/DungeonGenerationStyles.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenerationStyles.cs | 43 | 2 | Shimmer | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyleData | `public static DungeonGenerationStyleData Shimmer = new ShimmerStyleData  	{  		Style = 11,  		UnbreakableWallProgressionTier = DualDungeonUnbreakableWallTiers.EarlyGame,  		BrickTileType = 667,  		BrickCrackedTileType = 123,  		BrickWallType = 322,  		WindowGlassWallType = 93,  		WindowClosedGlassWallType = 149,  		WindowEdgeWallType = 37,  		WindowPlatformItemTypes = new int[1] { 94 },  		PitTrapTileType = 123,  		LiquidType = 3,  		LockedBiomeChestType = -1,  		LockedBiomeChestStyle = -1,  		BiomeChestItemType = -1,  		BiomeChestLootItemType = -1,  		ChestItemTypes = new int[1] { 5556 },  		DoorItemTypes = new int[1] { 5558 },  		PlatformItemTypes = new int[1] { 5562 },  		ChandelierItemTypes = new int[1] { 5555 },  		LanternItemTypes = new int[1] { 5560 },  		TableItemTypes = new int[1] { 5565 },  		WorkbenchItemTypes = new int[1] { 5566 },  		CandleItemTypes = new int[1] { 5553 },  		VaseOrStatueItemTypes = null,  		BookcaseItemTypes = new int[1] { 5550 },  		ChairItemTypes = new int[1] { 5554 },  		BedItemTypes = new int[1] { 5549 },  		PianoItemTypes = new int[1] { 5561 },  		DresserItemTypes = new int[1] { 5551 },  		SofaItemTypes = new int[1] { 5564 },  		BathtubItemTypes = new int[1] { 5548 },  		LampItemTypes = new int[1] { 5559 },  		CandelabraItemTypes = new int[1] { 5552 },  		ClockItemTypes = new int[1] { 5557 },  		BannerItemTypes = new int[6] { 337, 339, 338, 340, 5497, 5498 },  		EdgeDither = true,  		BiomeRoomType = DungeonRoomType.BiomeRugged  	};` | `public static DungeonGenerationStyleData Shimmer = new ShimmerStyleData { Style = 11, UnbreakableWallProgressionTier = DualDungeonUnbreakableWallTiers.EarlyGame, BrickTileType = 667, BrickCrackedTileType = 123, BrickWallType = 322, WindowGlassWallType = 93, WindowClosedGlassWallType = 149, WindowEdgeWallType = 37, WindowPlatformItemTypes = new int[1] { 94 }, PitTrapTileType = 123, LiquidType = 3, LockedBiomeChestType = -1, LockedBiomeChestStyle = -1, BiomeChestItemType = -1, BiomeChestLootItemType = -1, ChestItemTypes = new int[1] { 5556 }, DoorItemTypes = new int[1] { 5558 }, PlatformItemTypes = new int[1] { 5562 }, ChandelierItemTypes = new int[1] { 5555 }, LanternItemTypes = new int[1] { 5560 }, TableItemTypes = new int[1] { 5565 }, WorkbenchItemTypes = new int[1] { 5566 }, CandleItemTypes = new int[1] { 5553 }, VaseOrStatueItemTypes = null, BookcaseItemTypes = new int[1] { 5550 }, ChairItemTypes = new int[1] { 5554 }, BedItemTypes = new int[1] { 5549 }, PianoItemTypes = new int[1] { 5561 }, DresserItemTypes = new int[1] { 5551 }, SofaItemTypes = new int[1] { 5564 }, BathtubItemTypes = new int[1] { 5548 }, LampItemTypes = new int[1] { 5559 }, CandelabraItemTypes = new int[1] { 5552 }, ClockItemTypes = new int[1] { 5557 }, BannerItemTypes = new int[6] { 337, 339, 338, 340, 5497, 5498 }, EdgeDither = true, BiomeRoomType = DungeonRoomType.BiomeRugged };` |
| 1917 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyles | Terraria.GameContent.Generation.Dungeon/DungeonGenerationStyles.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenerationStyles.cs | 84 | 2 | Spider | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyleData | `public static DungeonGenerationStyleData Spider = new DungeonGenerationStyleData  	{  		Style = 12,  		UnbreakableWallProgressionTier = DualDungeonUnbreakableWallTiers.EarlyGame,  		BrickTileType = 156,  		BrickCrackedTileType = 123,  		BrickWallType = 62,  		WindowGlassWallType = 21,  		WindowClosedGlassWallType = 4,  		WindowEdgeWallType = 36,  		WindowPlatformItemTypes = new int[1] { 94 },  		PitTrapTileType = 123,  		LockedBiomeChestType = -1,  		LockedBiomeChestStyle = -1,  		BiomeChestItemType = -1,  		BiomeChestLootItemType = -1,  		ChestItemTypes = new int[1] { 952 },  		DoorItemTypes = new int[1] { 4415 },  		PlatformItemTypes = new int[1] { 4416 },  		ChandelierItemTypes = new int[6] { 106, 107, 108, 710, 711, 712 },  		LanternItemTypes = new int[1] { 2037 },  		TableItemTypes = new int[1] { 32 },  		WorkbenchItemTypes = new int[1] { 36 },  		CandleItemTypes = new int[2] { 105, 713 },  		VaseOrStatueItemTypes = null,  		BookcaseItemTypes = new int[1] { 354 },  		ChairItemTypes = new int[1] { 34 },  		BedItemTypes = new int[1] { 224 },  		PianoItemTypes = new int[1] { 333 },  		DresserItemTypes = new int[1] { 334 },  		SofaItemTypes = new int[1] { 2397 },  		BathtubItemTypes = new int[1] { 336 },  		LampItemTypes = new int[1] { 342 },  		CandelabraItemTypes = new int[2] { 349, 714 },  		ClockItemTypes = new int[1] { 359 },  		BannerItemTypes = null,  		EdgeDither = true,  		BiomeRoomType = DungeonRoomType.BiomeRugged  	};` | `public static DungeonGenerationStyleData Spider = new DungeonGenerationStyleData { Style = 12, UnbreakableWallProgressionTier = DualDungeonUnbreakableWallTiers.EarlyGame, BrickTileType = 156, BrickCrackedTileType = 123, BrickWallType = 62, WindowGlassWallType = 21, WindowClosedGlassWallType = 4, WindowEdgeWallType = 36, WindowPlatformItemTypes = new int[1] { 94 }, PitTrapTileType = 123, LockedBiomeChestType = -1, LockedBiomeChestStyle = -1, BiomeChestItemType = -1, BiomeChestLootItemType = -1, ChestItemTypes = new int[1] { 952 }, DoorItemTypes = new int[1] { 4415 }, PlatformItemTypes = new int[1] { 4416 }, ChandelierItemTypes = new int[6] { 106, 107, 108, 710, 711, 712 }, LanternItemTypes = new int[1] { 2037 }, TableItemTypes = new int[1] { 32 }, WorkbenchItemTypes = new int[1] { 36 }, CandleItemTypes = new int[2] { 105, 713 }, VaseOrStatueItemTypes = null, BookcaseItemTypes = new int[1] { 354 }, ChairItemTypes = new int[1] { 34 }, BedItemTypes = new int[1] { 224 }, PianoItemTypes = new int[1] { 333 }, DresserItemTypes = new int[1] { 334 }, SofaItemTypes = new int[1] { 2397 }, BathtubItemTypes = new int[1] { 336 }, LampItemTypes = new int[1] { 342 }, CandelabraItemTypes = new int[2] { 349, 714 }, ClockItemTypes = new int[1] { 359 }, BannerItemTypes = null, EdgeDither = true, BiomeRoomType = DungeonRoomType.BiomeRugged };` |
| 1918 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyles | Terraria.GameContent.Generation.Dungeon/DungeonGenerationStyles.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenerationStyles.cs | 124 | 2 | LivingWood | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyleData | `public static DungeonGenerationStyleData LivingWood = new LivingWoodStyleData  	{  		Style = 13,  		UnbreakableWallProgressionTier = DualDungeonUnbreakableWallTiers.EarlyGame,  		BrickTileType = 191,  		BrickCrackedTileType = 192,  		BrickWallType = 244,  		WindowGlassWallType = 21,  		WindowClosedGlassWallType = 4,  		WindowEdgeWallType = 196,  		WindowPlatformItemTypes = new int[1] { 2629 },  		PitTrapTileType = 123,  		LockedBiomeChestType = -1,  		LockedBiomeChestStyle = -1,  		BiomeChestItemType = -1,  		BiomeChestLootItemType = -1,  		ChestItemTypes = new int[1] { 831 },  		DoorItemTypes = new int[1] { 819 },  		PlatformItemTypes = new int[1] { 2629 },  		ChandelierItemTypes = new int[1] { 2141 },  		LanternItemTypes = new int[1] { 2145 },  		TableItemTypes = new int[1] { 829 },  		WorkbenchItemTypes = new int[1] { 2633 },  		CandleItemTypes = new int[1] { 2153 },  		VaseOrStatueItemTypes = null,  		BookcaseItemTypes = new int[1] { 2135 },  		ChairItemTypes = new int[1] { 806 },  		BedItemTypes = new int[1] { 2139 },  		PianoItemTypes = new int[1] { 2245 },  		DresserItemTypes = new int[1] { 3914 },  		SofaItemTypes = new int[1] { 2636 },  		BathtubItemTypes = new int[1] { 2126 },  		LampItemTypes = new int[1] { 2131 },  		CandelabraItemTypes = new int[1] { 2149 },  		ClockItemTypes = new int[1] { 2596 },  		BannerItemTypes = null,  		EdgeDither = true,  		BiomeRoomType = DungeonRoomType.BiomeRugged  	};` | `public static DungeonGenerationStyleData LivingWood = new LivingWoodStyleData { Style = 13, UnbreakableWallProgressionTier = DualDungeonUnbreakableWallTiers.EarlyGame, BrickTileType = 191, BrickCrackedTileType = 192, BrickWallType = 244, WindowGlassWallType = 21, WindowClosedGlassWallType = 4, WindowEdgeWallType = 196, WindowPlatformItemTypes = new int[1] { 2629 }, PitTrapTileType = 123, LockedBiomeChestType = -1, LockedBiomeChestStyle = -1, BiomeChestItemType = -1, BiomeChestLootItemType = -1, ChestItemTypes = new int[1] { 831 }, DoorItemTypes = new int[1] { 819 }, PlatformItemTypes = new int[1] { 2629 }, ChandelierItemTypes = new int[1] { 2141 }, LanternItemTypes = new int[1] { 2145 }, TableItemTypes = new int[1] { 829 }, WorkbenchItemTypes = new int[1] { 2633 }, CandleItemTypes = new int[1] { 2153 }, VaseOrStatueItemTypes = null, BookcaseItemTypes = new int[1] { 2135 }, ChairItemTypes = new int[1] { 806 }, BedItemTypes = new int[1] { 2139 }, PianoItemTypes = new int[1] { 2245 }, DresserItemTypes = new int[1] { 3914 }, SofaItemTypes = new int[1] { 2636 }, BathtubItemTypes = new int[1] { 2126 }, LampItemTypes = new int[1] { 2131 }, CandelabraItemTypes = new int[1] { 2149 }, ClockItemTypes = new int[1] { 2596 }, BannerItemTypes = null, EdgeDither = true, BiomeRoomType = DungeonRoomType.BiomeRugged };` |
| 1919 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyles | Terraria.GameContent.Generation.Dungeon/DungeonGenerationStyles.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenerationStyles.cs | 164 | 2 | Cavern | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyleData | `public static DungeonGenerationStyleData Cavern = new DungeonGenerationStyleData  	{  		Style = 1,  		UnbreakableWallProgressionTier = DualDungeonUnbreakableWallTiers.EarlyGame,  		BrickTileType = 38,  		BrickCrackedTileType = 123,  		BrickWallType = 349,  		WindowGlassWallType = 21,  		WindowClosedGlassWallType = 4,  		WindowEdgeWallType = 5,  		WindowPlatformItemTypes = new int[2] { 94, 4416 },  		PitTrapTileType = 123,  		LockedBiomeChestType = -1,  		LockedBiomeChestStyle = -1,  		BiomeChestItemType = -1,  		BiomeChestLootItemType = -1,  		ChestItemTypes = new int[2] { 306, 5886 },  		DoorItemTypes = new int[2] { 25, 4415 },  		PlatformItemTypes = new int[2] { 94, 4416 },  		ChandelierItemTypes = new int[7] { 106, 107, 108, 710, 711, 712, 5885 },  		LanternItemTypes = new int[2] { 2037, 5890 },  		TableItemTypes = new int[2] { 32, 5894 },  		WorkbenchItemTypes = new int[2] { 36, 5896 },  		CandleItemTypes = new int[3] { 105, 713, 5883 },  		VaseOrStatueItemTypes = null,  		BookcaseItemTypes = new int[2] { 354, 5881 },  		ChairItemTypes = new int[2] { 34, 5884 },  		BedItemTypes = new int[2] { 224, 5880 },  		PianoItemTypes = new int[2] { 333, 5891 },  		DresserItemTypes = new int[2] { 334, 5888 },  		SofaItemTypes = new int[2] { 2397, 5893 },  		BathtubItemTypes = new int[2] { 336, 5879 },  		LampItemTypes = new int[2] { 342, 5889 },  		CandelabraItemTypes = new int[3] { 349, 714, 5882 },  		ClockItemTypes = new int[2] { 359, 5887 },  		BannerItemTypes = new int[6] { 337, 339, 338, 340, 5497, 5498 },  		EdgeDither = true,  		BiomeRoomType = DungeonRoomType.BiomeStructured,  		SubStyles = new List<DungeonGenerationStyleData> { Shimmer, Spider, LivingWood }  	};` | `public static DungeonGenerationStyleData Cavern = new DungeonGenerationStyleData { Style = 1, UnbreakableWallProgressionTier = DualDungeonUnbreakableWallTiers.EarlyGame, BrickTileType = 38, BrickCrackedTileType = 123, BrickWallType = 349, WindowGlassWallType = 21, WindowClosedGlassWallType = 4, WindowEdgeWallType = 5, WindowPlatformItemTypes = new int[2] { 94, 4416 }, PitTrapTileType = 123, LockedBiomeChestType = -1, LockedBiomeChestStyle = -1, BiomeChestItemType = -1, BiomeChestLootItemType = -1, ChestItemTypes = new int[2] { 306, 5886 }, DoorItemTypes = new int[2] { 25, 4415 }, PlatformItemTypes = new int[2] { 94, 4416 }, ChandelierItemTypes = new int[7] { 106, 107, 108, 710, 711, 712, 5885 }, LanternItemTypes = new int[2] { 2037, 5890 }, TableItemTypes = new int[2] { 32, 5894 }, WorkbenchItemTypes = new int[2] { 36, 5896 }, CandleItemTypes = new int[3] { 105, 713, 5883 }, VaseOrStatueItemTypes = null, BookcaseItemTypes = new int[2] { 354, 5881 }, ChairItemTypes = new int[2] { 34, 5884 }, BedItemTypes = new int[2] { 224, 5880 }, PianoItemTypes = new int[2] { 333, 5891 }, DresserItemTypes = new int[2] { 334, 5888 }, SofaItemTypes = new int[2] { 2397, 5893 }, BathtubItemTypes = new int[2] { 336, 5879 }, LampItemTypes = new int[2] { 342, 5889 }, CandelabraItemTypes = new int[3] { 349, 714, 5882 }, ClockItemTypes = new int[2] { 359, 5887 }, BannerItemTypes = new int[6] { 337, 339, 338, 340, 5497, 5498 }, EdgeDither = true, BiomeRoomType = DungeonRoomType.BiomeStructured, SubStyles = new List<DungeonGenerationStyleData> { Shimmer, Spider, LivingWood } };` |
| 1920 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyles | Terraria.GameContent.Generation.Dungeon/DungeonGenerationStyles.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenerationStyles.cs | 205 | 2 | Snow | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyleData | `public static DungeonGenerationStyleData Snow = new DungeonGenerationStyleData  	{  		Style = 2,  		UnbreakableWallProgressionTier = DualDungeonUnbreakableWallTiers.EarlyGame,  		BrickTileType = 161,  		BrickCrackedTileType = 224,  		BrickWallType = 71,  		WindowGlassWallType = 90,  		WindowClosedGlassWallType = 149,  		WindowEdgeWallType = 31,  		WindowPlatformItemTypes = new int[1] { 3908 },  		PitTrapTileType = 224,  		LockedBiomeChestType = 21,  		LockedBiomeChestStyle = 27,  		BiomeChestItemType = 1532,  		BiomeChestLootItemType = 1572,  		ChestItemTypes = new int[2] { 681, 5805 },  		DoorItemTypes = new int[2] { 2044, 5807 },  		PlatformItemTypes = new int[2] { 3908, 5812 },  		ChandelierItemTypes = new int[2] { 2059, 5804 },  		LanternItemTypes = new int[2] { 2040, 5810 },  		TableItemTypes = new int[2] { 2248, 5815 },  		WorkbenchItemTypes = new int[2] { 2252, 5817 },  		CandleItemTypes = new int[2] { 2049, 5802 },  		VaseOrStatueItemTypes = null,  		BookcaseItemTypes = new int[2] { 2031, 5800 },  		ChairItemTypes = new int[2] { 2288, 5803 },  		BedItemTypes = new int[2] { 2068, 5799 },  		PianoItemTypes = new int[2] { 2247, 5811 },  		DresserItemTypes = new int[2] { 3913, 5808 },  		SofaItemTypes = new int[2] { 2635, 5814 },  		BathtubItemTypes = new int[2] { 2076, 5798 },  		LampItemTypes = new int[2] { 2086, 5809 },  		CandelabraItemTypes = new int[2] { 2100, 5801 },  		ClockItemTypes = new int[2] { 2594, 5806 },  		BannerItemTypes = null,  		EdgeDither = true,  		BiomeRoomType = DungeonRoomType.BiomeRugged  	};` | `public static DungeonGenerationStyleData Snow = new DungeonGenerationStyleData { Style = 2, UnbreakableWallProgressionTier = DualDungeonUnbreakableWallTiers.EarlyGame, BrickTileType = 161, BrickCrackedTileType = 224, BrickWallType = 71, WindowGlassWallType = 90, WindowClosedGlassWallType = 149, WindowEdgeWallType = 31, WindowPlatformItemTypes = new int[1] { 3908 }, PitTrapTileType = 224, LockedBiomeChestType = 21, LockedBiomeChestStyle = 27, BiomeChestItemType = 1532, BiomeChestLootItemType = 1572, ChestItemTypes = new int[2] { 681, 5805 }, DoorItemTypes = new int[2] { 2044, 5807 }, PlatformItemTypes = new int[2] { 3908, 5812 }, ChandelierItemTypes = new int[2] { 2059, 5804 }, LanternItemTypes = new int[2] { 2040, 5810 }, TableItemTypes = new int[2] { 2248, 5815 }, WorkbenchItemTypes = new int[2] { 2252, 5817 }, CandleItemTypes = new int[2] { 2049, 5802 }, VaseOrStatueItemTypes = null, BookcaseItemTypes = new int[2] { 2031, 5800 }, ChairItemTypes = new int[2] { 2288, 5803 }, BedItemTypes = new int[2] { 2068, 5799 }, PianoItemTypes = new int[2] { 2247, 5811 }, DresserItemTypes = new int[2] { 3913, 5808 }, SofaItemTypes = new int[2] { 2635, 5814 }, BathtubItemTypes = new int[2] { 2076, 5798 }, LampItemTypes = new int[2] { 2086, 5809 }, CandelabraItemTypes = new int[2] { 2100, 5801 }, ClockItemTypes = new int[2] { 2594, 5806 }, BannerItemTypes = null, EdgeDither = true, BiomeRoomType = DungeonRoomType.BiomeRugged };` |
| 1921 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyles | Terraria.GameContent.Generation.Dungeon/DungeonGenerationStyles.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenerationStyles.cs | 245 | 2 | Desert | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyleData | `public static DungeonGenerationStyleData Desert = new DungeonGenerationStyleData  	{  		Style = 3,  		UnbreakableWallProgressionTier = DualDungeonUnbreakableWallTiers.EarlyGame,  		BrickTileType = 396,  		BrickCrackedTileType = 53,  		BrickWallType = 187,  		WindowGlassWallType = 89,  		WindowClosedGlassWallType = 151,  		WindowEdgeWallType = 34,  		WindowPlatformItemTypes = new int[1] { 4311 },  		PitTrapTileType = 53,  		LockedBiomeChestType = 467,  		LockedBiomeChestStyle = 13,  		BiomeChestItemType = 4712,  		BiomeChestLootItemType = 4607,  		ChestItemTypes = new int[1] { 4267 },  		DoorItemTypes = new int[1] { 4307 },  		PlatformItemTypes = new int[1] { 4311 },  		ChandelierItemTypes = new int[1] { 4305 },  		LanternItemTypes = new int[1] { 4309 },  		TableItemTypes = new int[1] { 4314 },  		WorkbenchItemTypes = new int[1] { 4315 },  		CandleItemTypes = new int[1] { 4303 },  		VaseOrStatueItemTypes = null,  		BookcaseItemTypes = new int[1] { 4300 },  		ChairItemTypes = new int[1] { 4304 },  		BedItemTypes = new int[1] { 4299 },  		PianoItemTypes = new int[1] { 4310 },  		DresserItemTypes = new int[1] { 4301 },  		SofaItemTypes = new int[1] { 4313 },  		BathtubItemTypes = new int[1] { 4298 },  		LampItemTypes = new int[1] { 4308 },  		CandelabraItemTypes = new int[1] { 4302 },  		ClockItemTypes = new int[1] { 4306 },  		BannerItemTypes = new int[3] { 790, 791, 789 },  		EdgeDither = false,  		BiomeRoomType = DungeonRoomType.BiomeRugged  	};` | `public static DungeonGenerationStyleData Desert = new DungeonGenerationStyleData { Style = 3, UnbreakableWallProgressionTier = DualDungeonUnbreakableWallTiers.EarlyGame, BrickTileType = 396, BrickCrackedTileType = 53, BrickWallType = 187, WindowGlassWallType = 89, WindowClosedGlassWallType = 151, WindowEdgeWallType = 34, WindowPlatformItemTypes = new int[1] { 4311 }, PitTrapTileType = 53, LockedBiomeChestType = 467, LockedBiomeChestStyle = 13, BiomeChestItemType = 4712, BiomeChestLootItemType = 4607, ChestItemTypes = new int[1] { 4267 }, DoorItemTypes = new int[1] { 4307 }, PlatformItemTypes = new int[1] { 4311 }, ChandelierItemTypes = new int[1] { 4305 }, LanternItemTypes = new int[1] { 4309 }, TableItemTypes = new int[1] { 4314 }, WorkbenchItemTypes = new int[1] { 4315 }, CandleItemTypes = new int[1] { 4303 }, VaseOrStatueItemTypes = null, BookcaseItemTypes = new int[1] { 4300 }, ChairItemTypes = new int[1] { 4304 }, BedItemTypes = new int[1] { 4299 }, PianoItemTypes = new int[1] { 4310 }, DresserItemTypes = new int[1] { 4301 }, SofaItemTypes = new int[1] { 4313 }, BathtubItemTypes = new int[1] { 4298 }, LampItemTypes = new int[1] { 4308 }, CandelabraItemTypes = new int[1] { 4302 }, ClockItemTypes = new int[1] { 4306 }, BannerItemTypes = new int[3] { 790, 791, 789 }, EdgeDither = false, BiomeRoomType = DungeonRoomType.BiomeRugged };` |
| 1922 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyles | Terraria.GameContent.Generation.Dungeon/DungeonGenerationStyles.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenerationStyles.cs | 285 | 2 | Corruption | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyleData | `public static DungeonGenerationStyleData Corruption = new DungeonGenerationStyleData  	{  		Style = 4,  		UnbreakableWallProgressionTier = DualDungeonUnbreakableWallTiers.EvilBoss,  		BrickTileType = 25,  		BrickCrackedTileType = 112,  		BrickWallType = 3,  		WindowGlassWallType = 88,  		WindowClosedGlassWallType = 41,  		WindowEdgeWallType = 33,  		WindowPlatformItemTypes = new int[1] { 631 },  		PitTrapTileType = 112,  		LockedBiomeChestType = 21,  		LockedBiomeChestStyle = 24,  		BiomeChestItemType = 1529,  		BiomeChestLootItemType = 1571,  		ChestItemTypes = new int[3] { 625, 3965, 5763 },  		DoorItemTypes = new int[3] { 650, 3967, 5765 },  		PlatformItemTypes = new int[3] { 631, 3957, 5770 },  		ChandelierItemTypes = new int[3] { 2056, 3964, 5762 },  		LanternItemTypes = new int[3] { 2033, 3970, 5768 },  		TableItemTypes = new int[3] { 638, 3974, 5773 },  		WorkbenchItemTypes = new int[3] { 635, 3975, 5775 },  		CandleItemTypes = new int[3] { 2046, 3962, 5760 },  		VaseOrStatueItemTypes = null,  		BookcaseItemTypes = new int[3] { 2021, 3960, 5758 },  		ChairItemTypes = new int[3] { 628, 3963, 5761 },  		BedItemTypes = new int[3] { 644, 3959, 5757 },  		PianoItemTypes = new int[3] { 641, 3971, 5769 },  		DresserItemTypes = new int[3] { 647, 3968, 5766 },  		SofaItemTypes = new int[3] { 2398, 3973, 5772 },  		BathtubItemTypes = new int[3] { 2073, 3958, 5756 },  		LampItemTypes = new int[3] { 2083, 3969, 5767 },  		CandelabraItemTypes = new int[3] { 2093, 3961, 5759 },  		ClockItemTypes = new int[3] { 2593, 3966, 5764 },  		BannerItemTypes = null,  		EdgeDither = true,  		BiomeRoomType = DungeonRoomType.BiomeRugged  	};` | `public static DungeonGenerationStyleData Corruption = new DungeonGenerationStyleData { Style = 4, UnbreakableWallProgressionTier = DualDungeonUnbreakableWallTiers.EvilBoss, BrickTileType = 25, BrickCrackedTileType = 112, BrickWallType = 3, WindowGlassWallType = 88, WindowClosedGlassWallType = 41, WindowEdgeWallType = 33, WindowPlatformItemTypes = new int[1] { 631 }, PitTrapTileType = 112, LockedBiomeChestType = 21, LockedBiomeChestStyle = 24, BiomeChestItemType = 1529, BiomeChestLootItemType = 1571, ChestItemTypes = new int[3] { 625, 3965, 5763 }, DoorItemTypes = new int[3] { 650, 3967, 5765 }, PlatformItemTypes = new int[3] { 631, 3957, 5770 }, ChandelierItemTypes = new int[3] { 2056, 3964, 5762 }, LanternItemTypes = new int[3] { 2033, 3970, 5768 }, TableItemTypes = new int[3] { 638, 3974, 5773 }, WorkbenchItemTypes = new int[3] { 635, 3975, 5775 }, CandleItemTypes = new int[3] { 2046, 3962, 5760 }, VaseOrStatueItemTypes = null, BookcaseItemTypes = new int[3] { 2021, 3960, 5758 }, ChairItemTypes = new int[3] { 628, 3963, 5761 }, BedItemTypes = new int[3] { 644, 3959, 5757 }, PianoItemTypes = new int[3] { 641, 3971, 5769 }, DresserItemTypes = new int[3] { 647, 3968, 5766 }, SofaItemTypes = new int[3] { 2398, 3973, 5772 }, BathtubItemTypes = new int[3] { 2073, 3958, 5756 }, LampItemTypes = new int[3] { 2083, 3969, 5767 }, CandelabraItemTypes = new int[3] { 2093, 3961, 5759 }, ClockItemTypes = new int[3] { 2593, 3966, 5764 }, BannerItemTypes = null, EdgeDither = true, BiomeRoomType = DungeonRoomType.BiomeRugged };` |
| 1923 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyles | Terraria.GameContent.Generation.Dungeon/DungeonGenerationStyles.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenerationStyles.cs | 325 | 2 | Crimson | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyleData | `public static DungeonGenerationStyleData Crimson = new DungeonGenerationStyleData  	{  		Style = 5,  		UnbreakableWallProgressionTier = DualDungeonUnbreakableWallTiers.EvilBoss,  		BrickTileType = 203,  		BrickCrackedTileType = 234,  		BrickWallType = 83,  		WindowGlassWallType = 92,  		WindowClosedGlassWallType = 85,  		WindowEdgeWallType = 174,  		WindowPlatformItemTypes = new int[1] { 913 },  		PitTrapTileType = 234,  		LockedBiomeChestType = 21,  		LockedBiomeChestStyle = 25,  		BiomeChestItemType = 1530,  		BiomeChestLootItemType = 1569,  		ChestItemTypes = new int[3] { 914, 2617, 5784 },  		DoorItemTypes = new int[3] { 912, 817, 5786 },  		PlatformItemTypes = new int[3] { 913, 3907, 5791 },  		ChandelierItemTypes = new int[3] { 2142, 2057, 5783 },  		LanternItemTypes = new int[3] { 2146, 2034, 5789 },  		TableItemTypes = new int[3] { 917, 828, 5794 },  		WorkbenchItemTypes = new int[3] { 916, 813, 5796 },  		CandleItemTypes = new int[3] { 2154, 2047, 5781 },  		VaseOrStatueItemTypes = null,  		BookcaseItemTypes = new int[3] { 2136, 2022, 5779 },  		ChairItemTypes = new int[3] { 915, 809, 5782 },  		BedItemTypes = new int[3] { 920, 2067, 5778 },  		PianoItemTypes = new int[3] { 919, 2246, 5790 },  		DresserItemTypes = new int[3] { 918, 2640, 5787 },  		SofaItemTypes = new int[3] { 2401, 2634, 5793 },  		BathtubItemTypes = new int[3] { 2127, 2074, 5777 },  		LampItemTypes = new int[3] { 2132, 2084, 5788 },  		CandelabraItemTypes = new int[3] { 2150, 2094, 5780 },  		ClockItemTypes = new int[3] { 2604, 2598, 5785 },  		BannerItemTypes = null,  		EdgeDither = true,  		BiomeRoomType = DungeonRoomType.BiomeRugged  	};` | `public static DungeonGenerationStyleData Crimson = new DungeonGenerationStyleData { Style = 5, UnbreakableWallProgressionTier = DualDungeonUnbreakableWallTiers.EvilBoss, BrickTileType = 203, BrickCrackedTileType = 234, BrickWallType = 83, WindowGlassWallType = 92, WindowClosedGlassWallType = 85, WindowEdgeWallType = 174, WindowPlatformItemTypes = new int[1] { 913 }, PitTrapTileType = 234, LockedBiomeChestType = 21, LockedBiomeChestStyle = 25, BiomeChestItemType = 1530, BiomeChestLootItemType = 1569, ChestItemTypes = new int[3] { 914, 2617, 5784 }, DoorItemTypes = new int[3] { 912, 817, 5786 }, PlatformItemTypes = new int[3] { 913, 3907, 5791 }, ChandelierItemTypes = new int[3] { 2142, 2057, 5783 }, LanternItemTypes = new int[3] { 2146, 2034, 5789 }, TableItemTypes = new int[3] { 917, 828, 5794 }, WorkbenchItemTypes = new int[3] { 916, 813, 5796 }, CandleItemTypes = new int[3] { 2154, 2047, 5781 }, VaseOrStatueItemTypes = null, BookcaseItemTypes = new int[3] { 2136, 2022, 5779 }, ChairItemTypes = new int[3] { 915, 809, 5782 }, BedItemTypes = new int[3] { 920, 2067, 5778 }, PianoItemTypes = new int[3] { 919, 2246, 5790 }, DresserItemTypes = new int[3] { 918, 2640, 5787 }, SofaItemTypes = new int[3] { 2401, 2634, 5793 }, BathtubItemTypes = new int[3] { 2127, 2074, 5777 }, LampItemTypes = new int[3] { 2132, 2084, 5788 }, CandelabraItemTypes = new int[3] { 2150, 2094, 5780 }, ClockItemTypes = new int[3] { 2604, 2598, 5785 }, BannerItemTypes = null, EdgeDither = true, BiomeRoomType = DungeonRoomType.BiomeRugged };` |
| 1924 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyles | Terraria.GameContent.Generation.Dungeon/DungeonGenerationStyles.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenerationStyles.cs | 365 | 2 | Crystal | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyleData | `public static DungeonGenerationStyleData Crystal = new ShimmerStyleData  	{  		Style = 15,  		UnbreakableWallProgressionTier = DualDungeonUnbreakableWallTiers.Hallow,  		BrickTileType = 385,  		BrickCrackedTileType = 116,  		BrickWallType = 186,  		WindowGlassWallType = 88,  		WindowClosedGlassWallType = 43,  		WindowEdgeWallType = 22,  		WindowPlatformItemTypes = new int[1] { 633 },  		PitTrapTileType = 116,  		LockedBiomeChestType = -1,  		LockedBiomeChestStyle = -1,  		BiomeChestItemType = -1,  		BiomeChestLootItemType = -1,  		ChestItemTypes = new int[1] { 3884 },  		DoorItemTypes = new int[1] { 3888 },  		PlatformItemTypes = new int[1] { 3903 },  		ChandelierItemTypes = new int[1] { 3894 },  		LanternItemTypes = new int[1] { 3891 },  		TableItemTypes = new int[1] { 3920 },  		WorkbenchItemTypes = new int[1] { 3909 },  		CandleItemTypes = new int[1] { 3890 },  		VaseOrStatueItemTypes = null,  		BookcaseItemTypes = new int[1] { 3917 },  		ChairItemTypes = new int[1] { 3889 },  		BedItemTypes = new int[1] { 3897 },  		PianoItemTypes = new int[1] { 3915 },  		DresserItemTypes = new int[1] { 3911 },  		SofaItemTypes = new int[1] { 3918 },  		BathtubItemTypes = new int[1] { 3895 },  		LampItemTypes = new int[1] { 3892 },  		CandelabraItemTypes = new int[1] { 3893 },  		ClockItemTypes = new int[1] { 3898 },  		BannerItemTypes = null,  		EdgeDither = false,  		BiomeRoomType = DungeonRoomType.BiomeStructured  	};` | `public static DungeonGenerationStyleData Crystal = new ShimmerStyleData { Style = 15, UnbreakableWallProgressionTier = DualDungeonUnbreakableWallTiers.Hallow, BrickTileType = 385, BrickCrackedTileType = 116, BrickWallType = 186, WindowGlassWallType = 88, WindowClosedGlassWallType = 43, WindowEdgeWallType = 22, WindowPlatformItemTypes = new int[1] { 633 }, PitTrapTileType = 116, LockedBiomeChestType = -1, LockedBiomeChestStyle = -1, BiomeChestItemType = -1, BiomeChestLootItemType = -1, ChestItemTypes = new int[1] { 3884 }, DoorItemTypes = new int[1] { 3888 }, PlatformItemTypes = new int[1] { 3903 }, ChandelierItemTypes = new int[1] { 3894 }, LanternItemTypes = new int[1] { 3891 }, TableItemTypes = new int[1] { 3920 }, WorkbenchItemTypes = new int[1] { 3909 }, CandleItemTypes = new int[1] { 3890 }, VaseOrStatueItemTypes = null, BookcaseItemTypes = new int[1] { 3917 }, ChairItemTypes = new int[1] { 3889 }, BedItemTypes = new int[1] { 3897 }, PianoItemTypes = new int[1] { 3915 }, DresserItemTypes = new int[1] { 3911 }, SofaItemTypes = new int[1] { 3918 }, BathtubItemTypes = new int[1] { 3895 }, LampItemTypes = new int[1] { 3892 }, CandelabraItemTypes = new int[1] { 3893 }, ClockItemTypes = new int[1] { 3898 }, BannerItemTypes = null, EdgeDither = false, BiomeRoomType = DungeonRoomType.BiomeStructured };` |
| 1925 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyles | Terraria.GameContent.Generation.Dungeon/DungeonGenerationStyles.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenerationStyles.cs | 405 | 2 | Hallow | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyleData | `public static DungeonGenerationStyleData Hallow = new DungeonGenerationStyleData  	{  		Style = 6,  		UnbreakableWallProgressionTier = DualDungeonUnbreakableWallTiers.Hallow,  		BrickTileType = 117,  		BrickCrackedTileType = 116,  		BrickWallType = 28,  		WindowGlassWallType = 91,  		WindowClosedGlassWallType = 43,  		WindowEdgeWallType = 22,  		WindowPlatformItemTypes = new int[1] { 633 },  		PitTrapTileType = 116,  		LockedBiomeChestType = 21,  		LockedBiomeChestStyle = 26,  		BiomeChestItemType = 1531,  		BiomeChestLootItemType = 1260,  		ChestItemTypes = new int[2] { 627, 3884 },  		DoorItemTypes = new int[2] { 652, 3888 },  		PlatformItemTypes = new int[2] { 633, 3903 },  		ChandelierItemTypes = new int[2] { 2061, 3894 },  		LanternItemTypes = new int[2] { 2039, 3891 },  		TableItemTypes = new int[2] { 640, 3920 },  		WorkbenchItemTypes = new int[2] { 637, 3909 },  		CandleItemTypes = new int[2] { 2051, 3890 },  		VaseOrStatueItemTypes = null,  		BookcaseItemTypes = new int[2] { 2027, 3917 },  		ChairItemTypes = new int[2] { 630, 3889 },  		BedItemTypes = new int[2] { 646, 3897 },  		PianoItemTypes = new int[2] { 643, 3915 },  		DresserItemTypes = new int[2] { 649, 3911 },  		SofaItemTypes = new int[2] { 2400, 3918 },  		BathtubItemTypes = new int[2] { 2078, 3895 },  		LampItemTypes = new int[2] { 2088, 3892 },  		CandelabraItemTypes = new int[2] { 2099, 3893 },  		ClockItemTypes = new int[2] { 2602, 3898 },  		BannerItemTypes = null,  		EdgeDither = true,  		BiomeRoomType = DungeonRoomType.BiomeRugged,  		SubStyles = new List<DungeonGenerationStyleData> { Crystal }  	};` | `public static DungeonGenerationStyleData Hallow = new DungeonGenerationStyleData { Style = 6, UnbreakableWallProgressionTier = DualDungeonUnbreakableWallTiers.Hallow, BrickTileType = 117, BrickCrackedTileType = 116, BrickWallType = 28, WindowGlassWallType = 91, WindowClosedGlassWallType = 43, WindowEdgeWallType = 22, WindowPlatformItemTypes = new int[1] { 633 }, PitTrapTileType = 116, LockedBiomeChestType = 21, LockedBiomeChestStyle = 26, BiomeChestItemType = 1531, BiomeChestLootItemType = 1260, ChestItemTypes = new int[2] { 627, 3884 }, DoorItemTypes = new int[2] { 652, 3888 }, PlatformItemTypes = new int[2] { 633, 3903 }, ChandelierItemTypes = new int[2] { 2061, 3894 }, LanternItemTypes = new int[2] { 2039, 3891 }, TableItemTypes = new int[2] { 640, 3920 }, WorkbenchItemTypes = new int[2] { 637, 3909 }, CandleItemTypes = new int[2] { 2051, 3890 }, VaseOrStatueItemTypes = null, BookcaseItemTypes = new int[2] { 2027, 3917 }, ChairItemTypes = new int[2] { 630, 3889 }, BedItemTypes = new int[2] { 646, 3897 }, PianoItemTypes = new int[2] { 643, 3915 }, DresserItemTypes = new int[2] { 649, 3911 }, SofaItemTypes = new int[2] { 2400, 3918 }, BathtubItemTypes = new int[2] { 2078, 3895 }, LampItemTypes = new int[2] { 2088, 3892 }, CandelabraItemTypes = new int[2] { 2099, 3893 }, ClockItemTypes = new int[2] { 2602, 3898 }, BannerItemTypes = null, EdgeDither = true, BiomeRoomType = DungeonRoomType.BiomeRugged, SubStyles = new List<DungeonGenerationStyleData> { Crystal } };` |
| 1926 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyles | Terraria.GameContent.Generation.Dungeon/DungeonGenerationStyles.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenerationStyles.cs | 446 | 2 | GlowingMushroom | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyleData | `public static DungeonGenerationStyleData GlowingMushroom = new DungeonGenerationStyleData  	{  		Style = 7,  		UnbreakableWallProgressionTier = DualDungeonUnbreakableWallTiers.JungleBoss,  		BrickTileType = 59,  		BrickGrassTileType = 70,  		BrickCrackedTileType = 123,  		BrickWallType = 80,  		WindowGlassWallType = 90,  		WindowClosedGlassWallType = 60,  		WindowEdgeWallType = 78,  		WindowPlatformItemTypes = new int[1] { 2549 },  		PitTrapTileType = 123,  		LockedBiomeChestType = -1,  		LockedBiomeChestStyle = -1,  		BiomeChestItemType = -1,  		BiomeChestLootItemType = -1,  		ChestItemTypes = new int[1] { 2544 },  		DoorItemTypes = new int[1] { 818 },  		PlatformItemTypes = new int[1] { 2549 },  		ChandelierItemTypes = new int[1] { 2543 },  		LanternItemTypes = new int[1] { 2546 },  		TableItemTypes = new int[1] { 2550 },  		WorkbenchItemTypes = new int[1] { 814 },  		CandleItemTypes = new int[1] { 2542 },  		VaseOrStatueItemTypes = null,  		BookcaseItemTypes = new int[1] { 2540 },  		ChairItemTypes = new int[1] { 810 },  		BedItemTypes = new int[1] { 2538 },  		PianoItemTypes = new int[1] { 2548 },  		DresserItemTypes = new int[1] { 2545 },  		SofaItemTypes = new int[1] { 2413 },  		BathtubItemTypes = new int[1] { 2537 },  		LampItemTypes = new int[1] { 2547 },  		CandelabraItemTypes = new int[1] { 2541 },  		ClockItemTypes = new int[1] { 2599 },  		BannerItemTypes = null,  		EdgeDither = true,  		BiomeRoomType = DungeonRoomType.BiomeRugged  	};` | `public static DungeonGenerationStyleData GlowingMushroom = new DungeonGenerationStyleData { Style = 7, UnbreakableWallProgressionTier = DualDungeonUnbreakableWallTiers.JungleBoss, BrickTileType = 59, BrickGrassTileType = 70, BrickCrackedTileType = 123, BrickWallType = 80, WindowGlassWallType = 90, WindowClosedGlassWallType = 60, WindowEdgeWallType = 78, WindowPlatformItemTypes = new int[1] { 2549 }, PitTrapTileType = 123, LockedBiomeChestType = -1, LockedBiomeChestStyle = -1, BiomeChestItemType = -1, BiomeChestLootItemType = -1, ChestItemTypes = new int[1] { 2544 }, DoorItemTypes = new int[1] { 818 }, PlatformItemTypes = new int[1] { 2549 }, ChandelierItemTypes = new int[1] { 2543 }, LanternItemTypes = new int[1] { 2546 }, TableItemTypes = new int[1] { 2550 }, WorkbenchItemTypes = new int[1] { 814 }, CandleItemTypes = new int[1] { 2542 }, VaseOrStatueItemTypes = null, BookcaseItemTypes = new int[1] { 2540 }, ChairItemTypes = new int[1] { 810 }, BedItemTypes = new int[1] { 2538 }, PianoItemTypes = new int[1] { 2548 }, DresserItemTypes = new int[1] { 2545 }, SofaItemTypes = new int[1] { 2413 }, BathtubItemTypes = new int[1] { 2537 }, LampItemTypes = new int[1] { 2547 }, CandelabraItemTypes = new int[1] { 2541 }, ClockItemTypes = new int[1] { 2599 }, BannerItemTypes = null, EdgeDither = true, BiomeRoomType = DungeonRoomType.BiomeRugged };` |
| 1927 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyles | Terraria.GameContent.Generation.Dungeon/DungeonGenerationStyles.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenerationStyles.cs | 487 | 2 | Beehive | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyleData | `public static DungeonGenerationStyleData Beehive = new BeehiveStyleData  	{  		Style = 9,  		UnbreakableWallProgressionTier = DualDungeonUnbreakableWallTiers.JungleBoss,  		BrickTileType = 225,  		BrickCrackedTileType = 123,  		BrickWallType = 86,  		WindowGlassWallType = 89,  		WindowClosedGlassWallType = 172,  		WindowEdgeWallType = 151,  		WindowPlatformItemTypes = new int[1] { 2630 },  		PitTrapTileType = 123,  		LiquidType = 2,  		LockedBiomeChestType = -1,  		LockedBiomeChestStyle = -1,  		BiomeChestItemType = -1,  		BiomeChestLootItemType = -1,  		ChestItemTypes = new int[1] { 2249 },  		DoorItemTypes = new int[1] { 1711 },  		PlatformItemTypes = new int[1] { 2630 },  		ChandelierItemTypes = new int[1] { 2058 },  		LanternItemTypes = new int[1] { 2035 },  		TableItemTypes = new int[1] { 1717 },  		WorkbenchItemTypes = new int[1] { 2251 },  		CandleItemTypes = new int[1] { 2648 },  		VaseOrStatueItemTypes = null,  		BookcaseItemTypes = new int[1] { 2023 },  		ChairItemTypes = new int[1] { 1707 },  		BedItemTypes = new int[1] { 1721 },  		PianoItemTypes = new int[1] { 2255 },  		DresserItemTypes = new int[1] { 2395 },  		SofaItemTypes = new int[1] { 2411 },  		BathtubItemTypes = new int[1] { 2124 },  		LampItemTypes = new int[1] { 2129 },  		CandelabraItemTypes = new int[1] { 2095 },  		ClockItemTypes = new int[1] { 2240 },  		BannerItemTypes = null,  		EdgeDither = true,  		BiomeRoomType = DungeonRoomType.BiomeRugged  	};` | `public static DungeonGenerationStyleData Beehive = new BeehiveStyleData { Style = 9, UnbreakableWallProgressionTier = DualDungeonUnbreakableWallTiers.JungleBoss, BrickTileType = 225, BrickCrackedTileType = 123, BrickWallType = 86, WindowGlassWallType = 89, WindowClosedGlassWallType = 172, WindowEdgeWallType = 151, WindowPlatformItemTypes = new int[1] { 2630 }, PitTrapTileType = 123, LiquidType = 2, LockedBiomeChestType = -1, LockedBiomeChestStyle = -1, BiomeChestItemType = -1, BiomeChestLootItemType = -1, ChestItemTypes = new int[1] { 2249 }, DoorItemTypes = new int[1] { 1711 }, PlatformItemTypes = new int[1] { 2630 }, ChandelierItemTypes = new int[1] { 2058 }, LanternItemTypes = new int[1] { 2035 }, TableItemTypes = new int[1] { 1717 }, WorkbenchItemTypes = new int[1] { 2251 }, CandleItemTypes = new int[1] { 2648 }, VaseOrStatueItemTypes = null, BookcaseItemTypes = new int[1] { 2023 }, ChairItemTypes = new int[1] { 1707 }, BedItemTypes = new int[1] { 1721 }, PianoItemTypes = new int[1] { 2255 }, DresserItemTypes = new int[1] { 2395 }, SofaItemTypes = new int[1] { 2411 }, BathtubItemTypes = new int[1] { 2124 }, LampItemTypes = new int[1] { 2129 }, CandelabraItemTypes = new int[1] { 2095 }, ClockItemTypes = new int[1] { 2240 }, BannerItemTypes = null, EdgeDither = true, BiomeRoomType = DungeonRoomType.BiomeRugged };` |
| 1928 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyles | Terraria.GameContent.Generation.Dungeon/DungeonGenerationStyles.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenerationStyles.cs | 528 | 2 | LivingMahogany | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyleData | `public static DungeonGenerationStyleData LivingMahogany = new LivingWoodStyleData  	{  		Style = 14,  		UnbreakableWallProgressionTier = DualDungeonUnbreakableWallTiers.JungleBoss,  		BrickTileType = 383,  		BrickCrackedTileType = 384,  		BrickWallType = 244,  		WindowGlassWallType = 21,  		WindowClosedGlassWallType = 42,  		WindowEdgeWallType = 196,  		WindowPlatformItemTypes = new int[1] { 2629 },  		PitTrapTileType = 123,  		LockedBiomeChestType = -1,  		LockedBiomeChestStyle = -1,  		BiomeChestItemType = -1,  		BiomeChestLootItemType = -1,  		ChestItemTypes = new int[1] { 831 },  		DoorItemTypes = new int[1] { 819 },  		PlatformItemTypes = new int[1] { 2629 },  		ChandelierItemTypes = new int[1] { 2141 },  		LanternItemTypes = new int[1] { 2145 },  		TableItemTypes = new int[1] { 829 },  		WorkbenchItemTypes = new int[1] { 2633 },  		CandleItemTypes = new int[1] { 2153 },  		VaseOrStatueItemTypes = null,  		BookcaseItemTypes = new int[1] { 2135 },  		ChairItemTypes = new int[1] { 806 },  		BedItemTypes = new int[1] { 2139 },  		PianoItemTypes = new int[1] { 2245 },  		DresserItemTypes = new int[1] { 3914 },  		SofaItemTypes = new int[1] { 2636 },  		BathtubItemTypes = new int[1] { 2126 },  		LampItemTypes = new int[1] { 2131 },  		CandelabraItemTypes = new int[1] { 2149 },  		ClockItemTypes = new int[1] { 2596 },  		BannerItemTypes = null,  		EdgeDither = true,  		BiomeRoomType = DungeonRoomType.BiomeRugged  	};` | `public static DungeonGenerationStyleData LivingMahogany = new LivingWoodStyleData { Style = 14, UnbreakableWallProgressionTier = DualDungeonUnbreakableWallTiers.JungleBoss, BrickTileType = 383, BrickCrackedTileType = 384, BrickWallType = 244, WindowGlassWallType = 21, WindowClosedGlassWallType = 42, WindowEdgeWallType = 196, WindowPlatformItemTypes = new int[1] { 2629 }, PitTrapTileType = 123, LockedBiomeChestType = -1, LockedBiomeChestStyle = -1, BiomeChestItemType = -1, BiomeChestLootItemType = -1, ChestItemTypes = new int[1] { 831 }, DoorItemTypes = new int[1] { 819 }, PlatformItemTypes = new int[1] { 2629 }, ChandelierItemTypes = new int[1] { 2141 }, LanternItemTypes = new int[1] { 2145 }, TableItemTypes = new int[1] { 829 }, WorkbenchItemTypes = new int[1] { 2633 }, CandleItemTypes = new int[1] { 2153 }, VaseOrStatueItemTypes = null, BookcaseItemTypes = new int[1] { 2135 }, ChairItemTypes = new int[1] { 806 }, BedItemTypes = new int[1] { 2139 }, PianoItemTypes = new int[1] { 2245 }, DresserItemTypes = new int[1] { 3914 }, SofaItemTypes = new int[1] { 2636 }, BathtubItemTypes = new int[1] { 2126 }, LampItemTypes = new int[1] { 2131 }, CandelabraItemTypes = new int[1] { 2149 }, ClockItemTypes = new int[1] { 2596 }, BannerItemTypes = null, EdgeDither = true, BiomeRoomType = DungeonRoomType.BiomeRugged };` |
| 1929 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyles | Terraria.GameContent.Generation.Dungeon/DungeonGenerationStyles.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenerationStyles.cs | 568 | 2 | Jungle | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyleData | `public static DungeonGenerationStyleData Jungle = new DungeonGenerationStyleData  	{  		Style = 8,  		UnbreakableWallProgressionTier = DualDungeonUnbreakableWallTiers.JungleBoss,  		BrickTileType = 59,  		BrickGrassTileType = 60,  		BrickCrackedTileType = 123,  		BrickWallType = 64,  		WindowGlassWallType = 91,  		WindowClosedGlassWallType = 42,  		WindowEdgeWallType = 24,  		WindowPlatformItemTypes = new int[1] { 632 },  		PitTrapTileType = 123,  		LockedBiomeChestType = 21,  		LockedBiomeChestStyle = 23,  		BiomeChestItemType = 1528,  		BiomeChestLootItemType = 1156,  		ChestItemTypes = new int[2] { 626, 680 },  		DoorItemTypes = new int[1] { 651 },  		PlatformItemTypes = new int[1] { 632 },  		ChandelierItemTypes = new int[1] { 2060 },  		LanternItemTypes = new int[2] { 2038, 4578 },  		TableItemTypes = new int[1] { 639 },  		WorkbenchItemTypes = new int[1] { 636 },  		CandleItemTypes = new int[1] { 2050 },  		VaseOrStatueItemTypes = null,  		BookcaseItemTypes = new int[1] { 2026 },  		ChairItemTypes = new int[1] { 629 },  		BedItemTypes = new int[1] { 645 },  		PianoItemTypes = new int[1] { 642 },  		DresserItemTypes = new int[1] { 648 },  		SofaItemTypes = new int[1] { 2399 },  		BathtubItemTypes = new int[1] { 2077 },  		LampItemTypes = new int[1] { 2087 },  		CandelabraItemTypes = new int[1] { 2098 },  		ClockItemTypes = new int[1] { 2597 },  		BannerItemTypes = null,  		EdgeDither = true,  		BiomeRoomType = DungeonRoomType.BiomeRugged,  		SubStyles = new List<DungeonGenerationStyleData> { Beehive, LivingMahogany }  	};` | `public static DungeonGenerationStyleData Jungle = new DungeonGenerationStyleData { Style = 8, UnbreakableWallProgressionTier = DualDungeonUnbreakableWallTiers.JungleBoss, BrickTileType = 59, BrickGrassTileType = 60, BrickCrackedTileType = 123, BrickWallType = 64, WindowGlassWallType = 91, WindowClosedGlassWallType = 42, WindowEdgeWallType = 24, WindowPlatformItemTypes = new int[1] { 632 }, PitTrapTileType = 123, LockedBiomeChestType = 21, LockedBiomeChestStyle = 23, BiomeChestItemType = 1528, BiomeChestLootItemType = 1156, ChestItemTypes = new int[2] { 626, 680 }, DoorItemTypes = new int[1] { 651 }, PlatformItemTypes = new int[1] { 632 }, ChandelierItemTypes = new int[1] { 2060 }, LanternItemTypes = new int[2] { 2038, 4578 }, TableItemTypes = new int[1] { 639 }, WorkbenchItemTypes = new int[1] { 636 }, CandleItemTypes = new int[1] { 2050 }, VaseOrStatueItemTypes = null, BookcaseItemTypes = new int[1] { 2026 }, ChairItemTypes = new int[1] { 629 }, BedItemTypes = new int[1] { 645 }, PianoItemTypes = new int[1] { 642 }, DresserItemTypes = new int[1] { 648 }, SofaItemTypes = new int[1] { 2399 }, BathtubItemTypes = new int[1] { 2077 }, LampItemTypes = new int[1] { 2087 }, CandelabraItemTypes = new int[1] { 2098 }, ClockItemTypes = new int[1] { 2597 }, BannerItemTypes = null, EdgeDither = true, BiomeRoomType = DungeonRoomType.BiomeRugged, SubStyles = new List<DungeonGenerationStyleData> { Beehive, LivingMahogany } };` |
| 1930 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyles | Terraria.GameContent.Generation.Dungeon/DungeonGenerationStyles.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenerationStyles.cs | 610 | 2 | Temple | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyleData | `public static DungeonGenerationStyleData Temple = new TempleStyleData  	{  		Style = 10,  		BrickTileType = 226,  		BrickCrackedTileType = 123,  		BrickWallType = 87,  		WindowGlassWallType = 92,  		WindowClosedGlassWallType = 42,  		WindowEdgeWallType = 24,  		WindowPlatformItemTypes = new int[1] { 3906 },  		PitTrapTileType = 123,  		LockedBiomeChestType = -1,  		LockedBiomeChestStyle = -1,  		BiomeChestItemType = -1,  		BiomeChestLootItemType = -1,  		ChestItemTypes = new int[1] { 1142 },  		DoorItemTypes = new int[1] { 1137 },  		PlatformItemTypes = new int[1] { 3906 },  		ChandelierItemTypes = new int[1] { 2062 },  		LanternItemTypes = new int[1] { 2041 },  		TableItemTypes = new int[1] { 1144 },  		WorkbenchItemTypes = new int[1] { 1145 },  		CandleItemTypes = new int[1] { 2052 },  		VaseOrStatueItemTypes = new int[3] { 1152, 1153, 1154 },  		BookcaseItemTypes = new int[1] { 2030 },  		ChairItemTypes = new int[1] { 1143 },  		BedItemTypes = new int[1] { 2069 },  		PianoItemTypes = new int[1] { 2385 },  		DresserItemTypes = new int[1] { 2396 },  		SofaItemTypes = new int[1] { 2416 },  		BathtubItemTypes = new int[1] { 2079 },  		LampItemTypes = new int[1] { 2089 },  		CandelabraItemTypes = new int[1] { 2101 },  		ClockItemTypes = new int[1] { 2595 },  		BannerItemTypes = null,  		EdgeDither = false,  		BiomeRoomType = DungeonRoomType.BiomeStructured  	};` | `public static DungeonGenerationStyleData Temple = new TempleStyleData { Style = 10, BrickTileType = 226, BrickCrackedTileType = 123, BrickWallType = 87, WindowGlassWallType = 92, WindowClosedGlassWallType = 42, WindowEdgeWallType = 24, WindowPlatformItemTypes = new int[1] { 3906 }, PitTrapTileType = 123, LockedBiomeChestType = -1, LockedBiomeChestStyle = -1, BiomeChestItemType = -1, BiomeChestLootItemType = -1, ChestItemTypes = new int[1] { 1142 }, DoorItemTypes = new int[1] { 1137 }, PlatformItemTypes = new int[1] { 3906 }, ChandelierItemTypes = new int[1] { 2062 }, LanternItemTypes = new int[1] { 2041 }, TableItemTypes = new int[1] { 1144 }, WorkbenchItemTypes = new int[1] { 1145 }, CandleItemTypes = new int[1] { 2052 }, VaseOrStatueItemTypes = new int[3] { 1152, 1153, 1154 }, BookcaseItemTypes = new int[1] { 2030 }, ChairItemTypes = new int[1] { 1143 }, BedItemTypes = new int[1] { 2069 }, PianoItemTypes = new int[1] { 2385 }, DresserItemTypes = new int[1] { 2396 }, SofaItemTypes = new int[1] { 2416 }, BathtubItemTypes = new int[1] { 2079 }, LampItemTypes = new int[1] { 2089 }, CandelabraItemTypes = new int[1] { 2101 }, ClockItemTypes = new int[1] { 2595 }, BannerItemTypes = null, EdgeDither = false, BiomeRoomType = DungeonRoomType.BiomeStructured };` |

#### 属性（0）

无该类型成员记录。


### 4.12 细分子系统：`SharedDungeonBoundsAndProgressionDefinitions`

- 原报告章节：`4.9.123`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`03` / `世界生成与地牢`

- 上一级基线细分子系统：`SharedDungeonGeometryAndPlacementDefinitions`
- 细分职责：地牢边界、中心和不可破坏墙进度层级定义。
- 边界角色：`definition/query`；最小 seam：dungeon bounds progression port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：2；声明类型数：2；字段：11；属性：9；合计：20。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（11）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1819 | field | Terraria.GameContent.Generation.Dungeon.DualDungeonUnbreakableWallTiers | Terraria.GameContent.Generation.Dungeon/DualDungeonUnbreakableWallTiers.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DualDungeonUnbreakableWallTiers.cs | 5 | 2 | EarlyGame | int | `public static readonly int EarlyGame = 0;` | `public static readonly int EarlyGame = 0;` |
| 1820 | field | Terraria.GameContent.Generation.Dungeon.DualDungeonUnbreakableWallTiers | Terraria.GameContent.Generation.Dungeon/DualDungeonUnbreakableWallTiers.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DualDungeonUnbreakableWallTiers.cs | 7 | 2 | EvilBoss | int | `public static readonly int EvilBoss = 1;` | `public static readonly int EvilBoss = 1;` |
| 1821 | field | Terraria.GameContent.Generation.Dungeon.DualDungeonUnbreakableWallTiers | Terraria.GameContent.Generation.Dungeon/DualDungeonUnbreakableWallTiers.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DualDungeonUnbreakableWallTiers.cs | 9 | 2 | JungleBoss | int | `public static readonly int JungleBoss = 2;` | `public static readonly int JungleBoss = 2;` |
| 1822 | field | Terraria.GameContent.Generation.Dungeon.DualDungeonUnbreakableWallTiers | Terraria.GameContent.Generation.Dungeon/DualDungeonUnbreakableWallTiers.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DualDungeonUnbreakableWallTiers.cs | 11 | 2 | Dungeon | int | `public static readonly int Dungeon = 3;` | `public static readonly int Dungeon = 3;` |
| 1823 | field | Terraria.GameContent.Generation.Dungeon.DualDungeonUnbreakableWallTiers | Terraria.GameContent.Generation.Dungeon/DualDungeonUnbreakableWallTiers.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DualDungeonUnbreakableWallTiers.cs | 13 | 2 | Hallow | int | `public static readonly int Hallow = 4;` | `public static readonly int Hallow = 4;` |
| 1824 | field | Terraria.GameContent.Generation.Dungeon.DualDungeonUnbreakableWallTiers | Terraria.GameContent.Generation.Dungeon/DualDungeonUnbreakableWallTiers.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DualDungeonUnbreakableWallTiers.cs | 15 | 2 | Temple | int | `public static readonly int Temple = 5;` | `public static readonly int Temple = 5;` |
| 1825 | field | Terraria.GameContent.Generation.Dungeon.DungeonBounds | Terraria.GameContent.Generation.Dungeon/DungeonBounds.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonBounds.cs | 10 | 2 | _hitbox | Rectangle? | `[JsonProperty] private Rectangle? _hitbox;` | `[JsonProperty] private Rectangle? _hitbox;` |
| 1826 | field | Terraria.GameContent.Generation.Dungeon.DungeonBounds | Terraria.GameContent.Generation.Dungeon/DungeonBounds.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonBounds.cs | 13 | 2 | _boundsLeft | int | `private int _boundsLeft;` | `private int _boundsLeft;` |
| 1827 | field | Terraria.GameContent.Generation.Dungeon.DungeonBounds | Terraria.GameContent.Generation.Dungeon/DungeonBounds.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonBounds.cs | 15 | 2 | _boundsRight | int | `private int _boundsRight;` | `private int _boundsRight;` |
| 1828 | field | Terraria.GameContent.Generation.Dungeon.DungeonBounds | Terraria.GameContent.Generation.Dungeon/DungeonBounds.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonBounds.cs | 17 | 2 | _boundsTop | int | `private int _boundsTop;` | `private int _boundsTop;` |
| 1829 | field | Terraria.GameContent.Generation.Dungeon.DungeonBounds | Terraria.GameContent.Generation.Dungeon/DungeonBounds.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonBounds.cs | 19 | 2 | _boundsBottom | int | `private int _boundsBottom;` | `private int _boundsBottom;` |

#### 属性（9）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3789 | property | Terraria.GameContent.Generation.Dungeon.DungeonBounds | Terraria.GameContent.Generation.Dungeon/DungeonBounds.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonBounds.cs | 21 | 2 | X | int | `public int X => _boundsLeft;` | `public int X => _boundsLeft;` |
| 3790 | property | Terraria.GameContent.Generation.Dungeon.DungeonBounds | Terraria.GameContent.Generation.Dungeon/DungeonBounds.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonBounds.cs | 23 | 2 | Y | int | `public int Y => _boundsTop;` | `public int Y => _boundsTop;` |
| 3791 | property | Terraria.GameContent.Generation.Dungeon.DungeonBounds | Terraria.GameContent.Generation.Dungeon/DungeonBounds.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonBounds.cs | 25 | 2 | Width | int | `public int Width => _boundsRight - _boundsLeft;` | `public int Width => _boundsRight - _boundsLeft;` |
| 3792 | property | Terraria.GameContent.Generation.Dungeon.DungeonBounds | Terraria.GameContent.Generation.Dungeon/DungeonBounds.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonBounds.cs | 27 | 2 | Height | int | `public int Height => _boundsBottom - _boundsTop;` | `public int Height => _boundsBottom - _boundsTop;` |
| 3793 | property | Terraria.GameContent.Generation.Dungeon.DungeonBounds | Terraria.GameContent.Generation.Dungeon/DungeonBounds.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonBounds.cs | 29 | 2 | Left | int | `public int Left { get { return _boundsLeft; } set { _boundsLeft = (int)MathHelper.Clamp(value, 10f, Main.maxTilesX - 10); } }` | `public int Left { get { return _boundsLeft; } set { _boundsLeft = (int)MathHelper.Clamp(value, 10f, Main.maxTilesX - 10); } }` |
| 3794 | property | Terraria.GameContent.Generation.Dungeon.DungeonBounds | Terraria.GameContent.Generation.Dungeon/DungeonBounds.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonBounds.cs | 41 | 2 | Right | int | `public int Right { get { return _boundsRight; } set { _boundsRight = (int)MathHelper.Clamp(value, 10f, Main.maxTilesX - 10); } }` | `public int Right { get { return _boundsRight; } set { _boundsRight = (int)MathHelper.Clamp(value, 10f, Main.maxTilesX - 10); } }` |
| 3795 | property | Terraria.GameContent.Generation.Dungeon.DungeonBounds | Terraria.GameContent.Generation.Dungeon/DungeonBounds.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonBounds.cs | 53 | 2 | Top | int | `public int Top { get { return _boundsTop; } set { _boundsTop = (int)MathHelper.Clamp(value, 10f, Main.maxTilesY - 10); } }` | `public int Top { get { return _boundsTop; } set { _boundsTop = (int)MathHelper.Clamp(value, 10f, Main.maxTilesY - 10); } }` |
| 3796 | property | Terraria.GameContent.Generation.Dungeon.DungeonBounds | Terraria.GameContent.Generation.Dungeon/DungeonBounds.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonBounds.cs | 65 | 2 | Bottom | int | `public int Bottom { get { return _boundsBottom; } set { _boundsBottom = (int)MathHelper.Clamp(value, 10f, Main.maxTilesY - 10); } }` | `public int Bottom { get { return _boundsBottom; } set { _boundsBottom = (int)MathHelper.Clamp(value, 10f, Main.maxTilesY - 10); } }` |
| 3797 | property | Terraria.GameContent.Generation.Dungeon.DungeonBounds | Terraria.GameContent.Generation.Dungeon/DungeonBounds.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonBounds.cs | 77 | 2 | Center | Point | `public Point Center => new Point((Left + Right) / 2, (Top + Bottom) / 2);` | `public Point Center => new Point((Left + Right) / 2, (Top + Bottom) / 2);` |


### 4.13 细分子系统：`SharedDungeonDoorAndPlatformDefinitions`

- 原报告章节：`4.9.124`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`03` / `世界生成与地牢`

- 上一级基线细分子系统：`SharedDungeonGeometryAndPlacementDefinitions`
- 细分职责：地牢门、平台及其空间检查和放置覆盖数据。
- 边界角色：`definition/query`；最小 seam：dungeon door platform placement port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：2；声明类型数：2；字段：24；属性：1；合计：25。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（24）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1867 | field | Terraria.GameContent.Generation.Dungeon.DungeonDoorData | Terraria.GameContent.Generation.Dungeon/DungeonDoorData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonDoorData.cs | 7 | 2 | Position | Point | `public Point Position;` | `public Point Position;` |
| 1868 | field | Terraria.GameContent.Generation.Dungeon.DungeonDoorData | Terraria.GameContent.Generation.Dungeon/DungeonDoorData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonDoorData.cs | 9 | 2 | OverrideBrickTileType | ushort? | `public ushort? OverrideBrickTileType;` | `public ushort? OverrideBrickTileType;` |
| 1869 | field | Terraria.GameContent.Generation.Dungeon.DungeonDoorData | Terraria.GameContent.Generation.Dungeon/DungeonDoorData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonDoorData.cs | 11 | 2 | OverrideBrickWallType | ushort? | `public ushort? OverrideBrickWallType;` | `public ushort? OverrideBrickWallType;` |
| 1870 | field | Terraria.GameContent.Generation.Dungeon.DungeonDoorData | Terraria.GameContent.Generation.Dungeon/DungeonDoorData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonDoorData.cs | 13 | 2 | OverrideStyle | int? | `public int? OverrideStyle;` | `public int? OverrideStyle;` |
| 1871 | field | Terraria.GameContent.Generation.Dungeon.DungeonDoorData | Terraria.GameContent.Generation.Dungeon/DungeonDoorData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonDoorData.cs | 15 | 2 | Direction | int | `public int Direction;` | `public int Direction;` |
| 1872 | field | Terraria.GameContent.Generation.Dungeon.DungeonDoorData | Terraria.GameContent.Generation.Dungeon/DungeonDoorData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonDoorData.cs | 17 | 2 | InAHallway | bool | `public bool InAHallway;` | `public bool InAHallway;` |
| 1873 | field | Terraria.GameContent.Generation.Dungeon.DungeonDoorData | Terraria.GameContent.Generation.Dungeon/DungeonDoorData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonDoorData.cs | 19 | 2 | OverrideWidthFluff | int? | `public int? OverrideWidthFluff;` | `public int? OverrideWidthFluff;` |
| 1874 | field | Terraria.GameContent.Generation.Dungeon.DungeonDoorData | Terraria.GameContent.Generation.Dungeon/DungeonDoorData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonDoorData.cs | 21 | 2 | SkipOtherDoorsCheck | bool | `public bool SkipOtherDoorsCheck;` | `public bool SkipOtherDoorsCheck;` |
| 1875 | field | Terraria.GameContent.Generation.Dungeon.DungeonDoorData | Terraria.GameContent.Generation.Dungeon/DungeonDoorData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonDoorData.cs | 23 | 2 | SkipSpaceCheck | bool | `public bool SkipSpaceCheck;` | `public bool SkipSpaceCheck;` |
| 1876 | field | Terraria.GameContent.Generation.Dungeon.DungeonDoorData | Terraria.GameContent.Generation.Dungeon/DungeonDoorData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonDoorData.cs | 25 | 2 | AlwaysClearArea | bool | `public bool AlwaysClearArea;` | `public bool AlwaysClearArea;` |
| 1961 | field | Terraria.GameContent.Generation.Dungeon.DungeonPlatformData | Terraria.GameContent.Generation.Dungeon/DungeonPlatformData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonPlatformData.cs | 8 | 2 | Position | Point | `public Point Position;` | `public Point Position;` |
| 1962 | field | Terraria.GameContent.Generation.Dungeon.DungeonPlatformData | Terraria.GameContent.Generation.Dungeon/DungeonPlatformData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonPlatformData.cs | 10 | 2 | OverrideStyle | int? | `public int? OverrideStyle;` | `public int? OverrideStyle;` |
| 1963 | field | Terraria.GameContent.Generation.Dungeon.DungeonPlatformData | Terraria.GameContent.Generation.Dungeon/DungeonPlatformData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonPlatformData.cs | 12 | 2 | OverrideMaxLengthAllowed | int | `public int OverrideMaxLengthAllowed;` | `public int OverrideMaxLengthAllowed;` |
| 1964 | field | Terraria.GameContent.Generation.Dungeon.DungeonPlatformData | Terraria.GameContent.Generation.Dungeon/DungeonPlatformData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonPlatformData.cs | 14 | 2 | OverrideHeightFluff | int? | `public int? OverrideHeightFluff;` | `public int? OverrideHeightFluff;` |
| 1965 | field | Terraria.GameContent.Generation.Dungeon.DungeonPlatformData | Terraria.GameContent.Generation.Dungeon/DungeonPlatformData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonPlatformData.cs | 16 | 2 | InAHallway | bool | `public bool InAHallway;` | `public bool InAHallway;` |
| 1966 | field | Terraria.GameContent.Generation.Dungeon.DungeonPlatformData | Terraria.GameContent.Generation.Dungeon/DungeonPlatformData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonPlatformData.cs | 18 | 2 | ForcePlacement | bool | `public bool ForcePlacement;` | `public bool ForcePlacement;` |
| 1967 | field | Terraria.GameContent.Generation.Dungeon.DungeonPlatformData | Terraria.GameContent.Generation.Dungeon/DungeonPlatformData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonPlatformData.cs | 20 | 2 | SkipOtherPlatformsCheck | bool | `public bool SkipOtherPlatformsCheck;` | `public bool SkipOtherPlatformsCheck;` |
| 1968 | field | Terraria.GameContent.Generation.Dungeon.DungeonPlatformData | Terraria.GameContent.Generation.Dungeon/DungeonPlatformData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonPlatformData.cs | 22 | 2 | SkipSpaceCheck | bool | `public bool SkipSpaceCheck;` | `public bool SkipSpaceCheck;` |
| 1969 | field | Terraria.GameContent.Generation.Dungeon.DungeonPlatformData | Terraria.GameContent.Generation.Dungeon/DungeonPlatformData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonPlatformData.cs | 24 | 2 | PlaceBooksChance | double | `public double PlaceBooksChance;` | `public double PlaceBooksChance;` |
| 1970 | field | Terraria.GameContent.Generation.Dungeon.DungeonPlatformData | Terraria.GameContent.Generation.Dungeon/DungeonPlatformData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonPlatformData.cs | 26 | 2 | NoWaterbolt | bool | `public bool NoWaterbolt;` | `public bool NoWaterbolt;` |
| 1971 | field | Terraria.GameContent.Generation.Dungeon.DungeonPlatformData | Terraria.GameContent.Generation.Dungeon/DungeonPlatformData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonPlatformData.cs | 28 | 2 | PlacePotsChance | double | `public double PlacePotsChance;` | `public double PlacePotsChance;` |
| 1972 | field | Terraria.GameContent.Generation.Dungeon.DungeonPlatformData | Terraria.GameContent.Generation.Dungeon/DungeonPlatformData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonPlatformData.cs | 30 | 2 | PlaceWaterCandlesChance | double | `public double PlaceWaterCandlesChance;` | `public double PlaceWaterCandlesChance;` |
| 1973 | field | Terraria.GameContent.Generation.Dungeon.DungeonPlatformData | Terraria.GameContent.Generation.Dungeon/DungeonPlatformData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonPlatformData.cs | 32 | 2 | PlacePotionBottlesChance | double | `public double PlacePotionBottlesChance;` | `public double PlacePotionBottlesChance;` |
| 1974 | field | Terraria.GameContent.Generation.Dungeon.DungeonPlatformData | Terraria.GameContent.Generation.Dungeon/DungeonPlatformData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonPlatformData.cs | 34 | 2 | canPlaceHereCallback | System.Func<Terraria.GameContent.Generation.Dungeon.DungeonData, int, int, bool> | `public Func<DungeonData, int, int, bool> canPlaceHereCallback;` | `public Func<DungeonData, int, int, bool> canPlaceHereCallback;` |

#### 属性（1）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3800 | property | Terraria.GameContent.Generation.Dungeon.DungeonPlatformData | Terraria.GameContent.Generation.Dungeon/DungeonPlatformData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonPlatformData.cs | 36 | 2 | IsAShelf | bool | `public bool IsAShelf { get { if (!(PlaceBooksChance > 0.0) && !(PlacePotsChance > 0.0) && !(PlaceWaterCandlesChance > 0.0)) { return PlacePotionBottlesChance > 0.0; } return true; } }` | `public bool IsAShelf { get { if (!(PlaceBooksChance > 0.0) && !(PlacePotsChance > 0.0) && !(PlaceWaterCandlesChance > 0.0)) { return PlacePotionBottlesChance > 0.0; } return true; } }` |


### 4.14 细分子系统：`SharedDungeonHallLegacyAndGeometry`

- 原报告章节：`4.9.127`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`03` / `世界生成与地牢`

- 上一级基线细分子系统：`SharedDungeonHallDefinitions`
- 细分职责：旧大厅、阶梯大厅、正弦大厅和入口几何状态。
- 边界角色：`definition/catalog`；最小 seam：dungeon hall legacy geometry port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：5；声明类型数：5；字段：14；属性：0；合计：14。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（14）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1712 | field | Terraria.GameContent.Generation.Dungeon.Halls.LegacyDungeonHall | Terraria.GameContent.Generation.Dungeon.Halls/LegacyDungeonHall.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Halls\LegacyDungeonHall.cs | 11 | 2 | LastHall | Vector2D | `public Vector2D LastHall;` | `public Vector2D LastHall;` |
| 1713 | field | Terraria.GameContent.Generation.Dungeon.Halls.LegacyDungeonHall | Terraria.GameContent.Generation.Dungeon.Halls/LegacyDungeonHall.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Halls\LegacyDungeonHall.cs | 13 | 2 | Strength | int | `public int Strength;` | `public int Strength;` |
| 1714 | field | Terraria.GameContent.Generation.Dungeon.Halls.LegacyDungeonHall | Terraria.GameContent.Generation.Dungeon.Halls/LegacyDungeonHall.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Halls\LegacyDungeonHall.cs | 15 | 2 | Steps | int | `public int Steps;` | `public int Steps;` |
| 1715 | field | Terraria.GameContent.Generation.Dungeon.Halls.LegacyDungeonHall | Terraria.GameContent.Generation.Dungeon.Halls/LegacyDungeonHall.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Halls\LegacyDungeonHall.cs | 17 | 2 | OverrideStartPosition | Vector2D | `protected Vector2D OverrideStartPosition;` | `protected Vector2D OverrideStartPosition;` |
| 1716 | field | Terraria.GameContent.Generation.Dungeon.Halls.LegacyDungeonHall | Terraria.GameContent.Generation.Dungeon.Halls/LegacyDungeonHall.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Halls\LegacyDungeonHall.cs | 19 | 2 | OverrideEndPosition | Vector2D | `protected Vector2D OverrideEndPosition;` | `protected Vector2D OverrideEndPosition;` |
| 1717 | field | Terraria.GameContent.Generation.Dungeon.Halls.LegacyEntranceDungeonHall | Terraria.GameContent.Generation.Dungeon.Halls/LegacyEntranceDungeonHall.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Halls\LegacyEntranceDungeonHall.cs | 12 | 2 | Direction | int | `public int Direction;` | `public int Direction;` |
| 1718 | field | Terraria.GameContent.Generation.Dungeon.Halls.LegacyEntranceDungeonHallSettings | Terraria.GameContent.Generation.Dungeon.Halls/LegacyEntranceDungeonHallSettings.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Halls\LegacyEntranceDungeonHallSettings.cs | 5 | 2 | UsePrecalculatedEntrance | bool | `public bool UsePrecalculatedEntrance;` | `public bool UsePrecalculatedEntrance;` |
| 1721 | field | Terraria.GameContent.Generation.Dungeon.Halls.SineDungeonHall | Terraria.GameContent.Generation.Dungeon.Halls/SineDungeonHall.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Halls\SineDungeonHall.cs | 12 | 2 | PotentialPlatformPoints | System.Collections.Generic.List<System.Tuple<Vector2D, Vector2D>> | `public List<Tuple<Vector2D, Vector2D>> PotentialPlatformPoints = new List<Tuple<Vector2D, Vector2D>>();` | `public List<Tuple<Vector2D, Vector2D>> PotentialPlatformPoints = new List<Tuple<Vector2D, Vector2D>>();` |
| 1725 | field | Terraria.GameContent.Generation.Dungeon.Halls.StairwellDungeonHallSettings | Terraria.GameContent.Generation.Dungeon.Halls/StairwellDungeonHallSettings.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Halls\StairwellDungeonHallSettings.cs | 5 | 2 | MaxDistFromLine | int | `public int MaxDistFromLine = 20;` | `public int MaxDistFromLine = 20;` |
| 1726 | field | Terraria.GameContent.Generation.Dungeon.Halls.StairwellDungeonHallSettings | Terraria.GameContent.Generation.Dungeon.Halls/StairwellDungeonHallSettings.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Halls\StairwellDungeonHallSettings.cs | 7 | 2 | PointVariance | int | `public int PointVariance = 4;` | `public int PointVariance = 4;` |
| 1727 | field | Terraria.GameContent.Generation.Dungeon.Halls.StairwellDungeonHallSettings | Terraria.GameContent.Generation.Dungeon.Halls/StairwellDungeonHallSettings.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Halls\StairwellDungeonHallSettings.cs | 9 | 2 | InnerBoundsSize | int | `public int InnerBoundsSize = 3;` | `public int InnerBoundsSize = 3;` |
| 1728 | field | Terraria.GameContent.Generation.Dungeon.Halls.StairwellDungeonHallSettings | Terraria.GameContent.Generation.Dungeon.Halls/StairwellDungeonHallSettings.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Halls\StairwellDungeonHallSettings.cs | 11 | 2 | OuterBoundsSize | int | `public int OuterBoundsSize = 8;` | `public int OuterBoundsSize = 8;` |
| 1729 | field | Terraria.GameContent.Generation.Dungeon.Halls.StairwellDungeonHallSettings | Terraria.GameContent.Generation.Dungeon.Halls/StairwellDungeonHallSettings.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Halls\StairwellDungeonHallSettings.cs | 13 | 2 | Gradient | double | `public double Gradient = 0.35;` | `public double Gradient = 0.35;` |
| 1730 | field | Terraria.GameContent.Generation.Dungeon.Halls.StairwellDungeonHallSettings | Terraria.GameContent.Generation.Dungeon.Halls/StairwellDungeonHallSettings.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Halls\StairwellDungeonHallSettings.cs | 15 | 2 | IsEntranceHall | bool | `public bool IsEntranceHall;` | `public bool IsEntranceHall;` |

#### 属性（0）

无该类型成员记录。


### 4.15 细分子系统：`SharedDungeonGenerationCollectionsState`

- 原报告章节：`4.9.135`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`03` / `世界生成与地牢`

- 上一级基线细分子系统：`SharedDungeonGenerationDataState`
- 细分职责：地牢迭代、入口、房间、走廊和保护边界集合。
- 边界角色：`runtime state`；最小 seam：dungeon generation collections port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：19；属性：1；合计：20。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（19）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1831 | field | Terraria.GameContent.Generation.Dungeon.DungeonData | Terraria.GameContent.Generation.Dungeon/DungeonData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonData.cs | 14 | 2 | Type | Terraria.GameContent.Generation.Dungeon.DungeonType | `public DungeonType Type;` | `public DungeonType Type;` |
| 1832 | field | Terraria.GameContent.Generation.Dungeon.DungeonData | Terraria.GameContent.Generation.Dungeon/DungeonData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonData.cs | 16 | 2 | Iteration | int | `public int Iteration;` | `public int Iteration;` |
| 1833 | field | Terraria.GameContent.Generation.Dungeon.DungeonData | Terraria.GameContent.Generation.Dungeon/DungeonData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonData.cs | 18 | 2 | dungeonEntrance | Terraria.GameContent.Generation.Dungeon.Entrances.DungeonEntrance | `public DungeonEntrance dungeonEntrance;` | `public DungeonEntrance dungeonEntrance;` |
| 1834 | field | Terraria.GameContent.Generation.Dungeon.DungeonData | Terraria.GameContent.Generation.Dungeon/DungeonData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonData.cs | 20 | 2 | dungeonRooms | System.Collections.Generic.List<Terraria.GameContent.Generation.Dungeon.Rooms.DungeonRoom> | `public List<DungeonRoom> dungeonRooms = new List<DungeonRoom>();` | `public List<DungeonRoom> dungeonRooms = new List<DungeonRoom>();` |
| 1835 | field | Terraria.GameContent.Generation.Dungeon.DungeonData | Terraria.GameContent.Generation.Dungeon/DungeonData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonData.cs | 22 | 2 | dungeonHalls | System.Collections.Generic.List<Terraria.GameContent.Generation.Dungeon.Halls.DungeonHall> | `public List<DungeonHall> dungeonHalls = new List<DungeonHall>();` | `public List<DungeonHall> dungeonHalls = new List<DungeonHall>();` |
| 1836 | field | Terraria.GameContent.Generation.Dungeon.DungeonData | Terraria.GameContent.Generation.Dungeon/DungeonData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonData.cs | 24 | 2 | dungeonFeatures | System.Collections.Generic.List<Terraria.GameContent.Generation.Dungeon.Features.IDungeonFeature> | `public List<IDungeonFeature> dungeonFeatures = new List<IDungeonFeature>();` | `public List<IDungeonFeature> dungeonFeatures = new List<IDungeonFeature>();` |
| 1837 | field | Terraria.GameContent.Generation.Dungeon.DungeonData | Terraria.GameContent.Generation.Dungeon/DungeonData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonData.cs | 26 | 2 | dungeonDoorData | System.Collections.Generic.List<Terraria.GameContent.Generation.Dungeon.DungeonDoorData> | `public List<DungeonDoorData> dungeonDoorData = new List<DungeonDoorData>();` | `public List<DungeonDoorData> dungeonDoorData = new List<DungeonDoorData>();` |
| 1838 | field | Terraria.GameContent.Generation.Dungeon.DungeonData | Terraria.GameContent.Generation.Dungeon/DungeonData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonData.cs | 28 | 2 | dungeonPlatformData | System.Collections.Generic.List<Terraria.GameContent.Generation.Dungeon.DungeonPlatformData> | `public List<DungeonPlatformData> dungeonPlatformData = new List<DungeonPlatformData>();` | `public List<DungeonPlatformData> dungeonPlatformData = new List<DungeonPlatformData>();` |
| 1839 | field | Terraria.GameContent.Generation.Dungeon.DungeonData | Terraria.GameContent.Generation.Dungeon/DungeonData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonData.cs | 30 | 2 | protectedDungeonBounds | System.Collections.Generic.List<Terraria.GameContent.Generation.Dungeon.DungeonBounds> | `public List<DungeonBounds> protectedDungeonBounds = new List<DungeonBounds>();` | `public List<DungeonBounds> protectedDungeonBounds = new List<DungeonBounds>();` |
| 1840 | field | Terraria.GameContent.Generation.Dungeon.DungeonData | Terraria.GameContent.Generation.Dungeon/DungeonData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonData.cs | 32 | 2 | makeNextPitTrapFlooded | bool | `public bool makeNextPitTrapFlooded;` | `public bool makeNextPitTrapFlooded;` |
| 1841 | field | Terraria.GameContent.Generation.Dungeon.DungeonData | Terraria.GameContent.Generation.Dungeon/DungeonData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonData.cs | 34 | 2 | useSkewedDungeonEntranceHalls | bool | `public bool useSkewedDungeonEntranceHalls;` | `public bool useSkewedDungeonEntranceHalls;` |
| 1842 | field | Terraria.GameContent.Generation.Dungeon.DungeonData | Terraria.GameContent.Generation.Dungeon/DungeonData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonData.cs | 36 | 2 | createdDungeonEntranceOnSurface | bool | `public bool createdDungeonEntranceOnSurface;` | `public bool createdDungeonEntranceOnSurface;` |
| 1843 | field | Terraria.GameContent.Generation.Dungeon.DungeonData | Terraria.GameContent.Generation.Dungeon/DungeonData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonData.cs | 38 | 2 | dungeonEntranceStrengthX | double | `public double dungeonEntranceStrengthX;` | `public double dungeonEntranceStrengthX;` |
| 1844 | field | Terraria.GameContent.Generation.Dungeon.DungeonData | Terraria.GameContent.Generation.Dungeon/DungeonData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonData.cs | 40 | 2 | dungeonEntranceStrengthY | double | `public double dungeonEntranceStrengthY;` | `public double dungeonEntranceStrengthY;` |
| 1845 | field | Terraria.GameContent.Generation.Dungeon.DungeonData | Terraria.GameContent.Generation.Dungeon/DungeonData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonData.cs | 42 | 2 | dungeonEntranceStrengthX2 | double | `public double dungeonEntranceStrengthX2;` | `public double dungeonEntranceStrengthX2;` |
| 1846 | field | Terraria.GameContent.Generation.Dungeon.DungeonData | Terraria.GameContent.Generation.Dungeon/DungeonData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonData.cs | 44 | 2 | dungeonEntranceStrengthY2 | double | `public double dungeonEntranceStrengthY2;` | `public double dungeonEntranceStrengthY2;` |
| 1847 | field | Terraria.GameContent.Generation.Dungeon.DungeonData | Terraria.GameContent.Generation.Dungeon/DungeonData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonData.cs | 46 | 2 | lastDungeonHall | Vector2D | `public Vector2D lastDungeonHall = Vector2D.Zero;` | `public Vector2D lastDungeonHall = Vector2D.Zero;` |
| 1848 | field | Terraria.GameContent.Generation.Dungeon.DungeonData | Terraria.GameContent.Generation.Dungeon/DungeonData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonData.cs | 48 | 2 | dungeonBounds | Terraria.GameContent.Generation.Dungeon.DungeonBounds | `public DungeonBounds dungeonBounds = new DungeonBounds();` | `public DungeonBounds dungeonBounds = new DungeonBounds();` |
| 1849 | field | Terraria.GameContent.Generation.Dungeon.DungeonData | Terraria.GameContent.Generation.Dungeon/DungeonData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonData.cs | 50 | 2 | outerProgressionBounds | Terraria.GameContent.Generation.Dungeon.DungeonBounds[] | `public DungeonBounds[] outerProgressionBounds = new DungeonBounds[0];` | `public DungeonBounds[] outerProgressionBounds = new DungeonBounds[0];` |

#### 属性（1）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3799 | property | Terraria.GameContent.Generation.Dungeon.DungeonData | Terraria.GameContent.Generation.Dungeon/DungeonData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonData.cs | 86 | 2 | genVars | Terraria.GameContent.Generation.Dungeon.DungeonGenVars | `public DungeonGenVars genVars => GenVars.dungeonGenVars[Iteration];` | `public DungeonGenVars genVars => GenVars.dungeonGenVars[Iteration];` |


### 4.16 细分子系统：`SharedDungeonGenerationScalarAndStyleState`

- 原报告章节：`4.9.136`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`03` / `世界生成与地牢`

- 上一级基线细分子系统：`SharedDungeonGenerationDataState`
- 细分职责：地牢样式、物品类型和生成强度比例状态。
- 边界角色：`runtime state`；最小 seam：dungeon generation scalar style port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：17；属性：0；合计：17。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（17）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1850 | field | Terraria.GameContent.Generation.Dungeon.DungeonData | Terraria.GameContent.Generation.Dungeon/DungeonData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonData.cs | 52 | 2 | wallVariants | int[] | `public int[] wallVariants = new int[3];` | `public int[] wallVariants = new int[3];` |
| 1851 | field | Terraria.GameContent.Generation.Dungeon.DungeonData | Terraria.GameContent.Generation.Dungeon/DungeonData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonData.cs | 54 | 2 | chandelierItemType | int | `public int chandelierItemType;` | `public int chandelierItemType;` |
| 1852 | field | Terraria.GameContent.Generation.Dungeon.DungeonData | Terraria.GameContent.Generation.Dungeon/DungeonData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonData.cs | 56 | 2 | platformItemType | int | `public int platformItemType;` | `public int platformItemType;` |
| 1853 | field | Terraria.GameContent.Generation.Dungeon.DungeonData | Terraria.GameContent.Generation.Dungeon/DungeonData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonData.cs | 58 | 2 | doorItemType | int | `public int doorItemType;` | `public int doorItemType;` |
| 1854 | field | Terraria.GameContent.Generation.Dungeon.DungeonData | Terraria.GameContent.Generation.Dungeon/DungeonData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonData.cs | 60 | 2 | lanternStyles | int[] | `public int[] lanternStyles = new int[3];` | `public int[] lanternStyles = new int[3];` |
| 1855 | field | Terraria.GameContent.Generation.Dungeon.DungeonData | Terraria.GameContent.Generation.Dungeon/DungeonData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonData.cs | 62 | 2 | shelfStyles | int[] | `public int[] shelfStyles = new int[3];` | `public int[] shelfStyles = new int[3];` |
| 1856 | field | Terraria.GameContent.Generation.Dungeon.DungeonData | Terraria.GameContent.Generation.Dungeon/DungeonData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonData.cs | 64 | 2 | bannerStyles | int[] | `public int[] bannerStyles = new int[6];` | `public int[] bannerStyles = new int[6];` |
| 1857 | field | Terraria.GameContent.Generation.Dungeon.DungeonData | Terraria.GameContent.Generation.Dungeon/DungeonData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonData.cs | 66 | 2 | globalFeatureScalar | double | `public double globalFeatureScalar = 1.0;` | `public double globalFeatureScalar = 1.0;` |
| 1858 | field | Terraria.GameContent.Generation.Dungeon.DungeonData | Terraria.GameContent.Generation.Dungeon/DungeonData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonData.cs | 68 | 2 | dungeonStepScalar | double | `public double dungeonStepScalar = 1.0;` | `public double dungeonStepScalar = 1.0;` |
| 1859 | field | Terraria.GameContent.Generation.Dungeon.DungeonData | Terraria.GameContent.Generation.Dungeon/DungeonData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonData.cs | 70 | 2 | hallStrengthScalar | double | `public double hallStrengthScalar = 1.0;` | `public double hallStrengthScalar = 1.0;` |
| 1860 | field | Terraria.GameContent.Generation.Dungeon.DungeonData | Terraria.GameContent.Generation.Dungeon/DungeonData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonData.cs | 72 | 2 | hallStepScalar | double | `public double hallStepScalar = 1.0;` | `public double hallStepScalar = 1.0;` |
| 1861 | field | Terraria.GameContent.Generation.Dungeon.DungeonData | Terraria.GameContent.Generation.Dungeon/DungeonData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonData.cs | 74 | 2 | hallInteriorToExteriorRatio | double | `public double hallInteriorToExteriorRatio = 0.5;` | `public double hallInteriorToExteriorRatio = 0.5;` |
| 1862 | field | Terraria.GameContent.Generation.Dungeon.DungeonData | Terraria.GameContent.Generation.Dungeon/DungeonData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonData.cs | 76 | 2 | hallSlantVariantScalar | double | `public double hallSlantVariantScalar = 1.0;` | `public double hallSlantVariantScalar = 1.0;` |
| 1863 | field | Terraria.GameContent.Generation.Dungeon.DungeonData | Terraria.GameContent.Generation.Dungeon/DungeonData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonData.cs | 78 | 2 | roomStrengthScalar | double | `public double roomStrengthScalar = 1.0;` | `public double roomStrengthScalar = 1.0;` |
| 1864 | field | Terraria.GameContent.Generation.Dungeon.DungeonData | Terraria.GameContent.Generation.Dungeon/DungeonData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonData.cs | 80 | 2 | roomStepScalar | double | `public double roomStepScalar = 1.0;` | `public double roomStepScalar = 1.0;` |
| 1865 | field | Terraria.GameContent.Generation.Dungeon.DungeonData | Terraria.GameContent.Generation.Dungeon/DungeonData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonData.cs | 82 | 2 | roomInteriorToExteriorRatio | double | `public double roomInteriorToExteriorRatio = 0.5;` | `public double roomInteriorToExteriorRatio = 0.5;` |
| 1866 | field | Terraria.GameContent.Generation.Dungeon.DungeonData | Terraria.GameContent.Generation.Dungeon/DungeonData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonData.cs | 84 | 2 | roomSlantVariantScalar | double | `public double roomSlantVariantScalar = 1.0;` | `public double roomSlantVariantScalar = 1.0;` |

#### 属性（0）

无该类型成员记录。


### 4.17 细分子系统：`SharedDungeonGeometryRuleQueries`

- 原报告章节：`4.9.137`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`03` / `世界生成与地牢`

- 上一级基线细分子系统：`SharedDungeonGenerationQueries`
- 细分职责：大厅/房间深度、放置变化和马赛克规则查询。
- 边界角色：`query`；最小 seam：dungeon geometry rule query port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：8；属性：0；合计：8。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（8）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2003 | field | Terraria.GameContent.Generation.Dungeon.DungeonUtils | Terraria.GameContent.Generation.Dungeon/DungeonUtils.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonUtils.cs | 75 | 2 | HALLWAY_DOOR_PLACEMENT_VARIANCE | double | `public const double HALLWAY_DOOR_PLACEMENT_VARIANCE = 0.25;` | `public const double HALLWAY_DOOR_PLACEMENT_VARIANCE = 0.25;` |
| 2004 | field | Terraria.GameContent.Generation.Dungeon.DungeonUtils | Terraria.GameContent.Generation.Dungeon/DungeonUtils.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonUtils.cs | 77 | 2 | DUNGEONHALL_DEFAULT_INNER_AREA_DEPTH | int | `public const int DUNGEONHALL_DEFAULT_INNER_AREA_DEPTH = 3;` | `public const int DUNGEONHALL_DEFAULT_INNER_AREA_DEPTH = 3;` |
| 2005 | field | Terraria.GameContent.Generation.Dungeon.DungeonUtils | Terraria.GameContent.Generation.Dungeon/DungeonUtils.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonUtils.cs | 79 | 2 | DUNGEONHALL_DEFAULT_OUTER_WALL_DEPTH | int | `public const int DUNGEONHALL_DEFAULT_OUTER_WALL_DEPTH = 8;` | `public const int DUNGEONHALL_DEFAULT_OUTER_WALL_DEPTH = 8;` |
| 2006 | field | Terraria.GameContent.Generation.Dungeon.DungeonUtils | Terraria.GameContent.Generation.Dungeon/DungeonUtils.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonUtils.cs | 81 | 2 | DUNGEONROOM_DEFAULT_INNER_AREA_DEPTH | int | `public const int DUNGEONROOM_DEFAULT_INNER_AREA_DEPTH = 6;` | `public const int DUNGEONROOM_DEFAULT_INNER_AREA_DEPTH = 6;` |
| 2007 | field | Terraria.GameContent.Generation.Dungeon.DungeonUtils | Terraria.GameContent.Generation.Dungeon/DungeonUtils.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonUtils.cs | 83 | 2 | DUNGEONROOM_DEFAULT_OUTER_WALL_DEPTH | int | `public const int DUNGEONROOM_DEFAULT_OUTER_WALL_DEPTH = 8;` | `public const int DUNGEONROOM_DEFAULT_OUTER_WALL_DEPTH = 8;` |
| 2008 | field | Terraria.GameContent.Generation.Dungeon.DungeonUtils | Terraria.GameContent.Generation.Dungeon/DungeonUtils.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonUtils.cs | 85 | 2 | MOSAIC_NONE | int | `public const int MOSAIC_NONE = 0;` | `public const int MOSAIC_NONE = 0;` |
| 2009 | field | Terraria.GameContent.Generation.Dungeon.DungeonUtils | Terraria.GameContent.Generation.Dungeon/DungeonUtils.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonUtils.cs | 87 | 2 | MOSAIC_SKELETRON | int | `public const int MOSAIC_SKELETRON = 1;` | `public const int MOSAIC_SKELETRON = 1;` |
| 2010 | field | Terraria.GameContent.Generation.Dungeon.DungeonUtils | Terraria.GameContent.Generation.Dungeon/DungeonUtils.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonUtils.cs | 89 | 2 | MOSAIC_MOONLORD | int | `public const int MOSAIC_MOONLORD = 2;` | `public const int MOSAIC_MOONLORD = 2;` |

#### 属性（0）

无该类型成员记录。


### 4.18 细分子系统：`SharedDungeonLegacyPlacementState`

- 原报告章节：`4.9.155`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`03` / `世界生成与地牢`

- 上一级基线细分子系统：`SharedDungeonLegacyGenerationGlobals`
- 细分职责：旧地牢位置、砖墙类型、边界和入口放置状态。
- 边界角色：`runtime state`；最小 seam：legacy dungeon placement port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：17；属性：0；合计：17。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（17）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1931 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenVars | Terraria.GameContent.Generation.Dungeon/DungeonGenVars.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenVars.cs | 10 | 2 | dungeonSide | int | `public int dungeonSide;` | `public int dungeonSide;` |
| 1932 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenVars | Terraria.GameContent.Generation.Dungeon/DungeonGenVars.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenVars.cs | 12 | 2 | dungeonLocation | int | `public int dungeonLocation;` | `public int dungeonLocation;` |
| 1933 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenVars | Terraria.GameContent.Generation.Dungeon/DungeonGenVars.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenVars.cs | 14 | 2 | dungeonColor | Terraria.GameContent.Generation.Dungeon.DungeonColor | `public DungeonColor dungeonColor;` | `public DungeonColor dungeonColor;` |
| 1934 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenVars | Terraria.GameContent.Generation.Dungeon/DungeonGenVars.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenVars.cs | 16 | 2 | brickTileType | ushort | `public ushort brickTileType = 41;` | `public ushort brickTileType = 41;` |
| 1935 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenVars | Terraria.GameContent.Generation.Dungeon/DungeonGenVars.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenVars.cs | 18 | 2 | brickWallType | ushort | `public ushort brickWallType = 7;` | `public ushort brickWallType = 7;` |
| 1936 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenVars | Terraria.GameContent.Generation.Dungeon/DungeonGenVars.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenVars.cs | 20 | 2 | brickCrackedTileType | ushort | `public ushort brickCrackedTileType = 481;` | `public ushort brickCrackedTileType = 481;` |
| 1937 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenVars | Terraria.GameContent.Generation.Dungeon/DungeonGenVars.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenVars.cs | 22 | 2 | windowGlassWallType | ushort | `public ushort windowGlassWallType = 91;` | `public ushort windowGlassWallType = 91;` |
| 1938 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenVars | Terraria.GameContent.Generation.Dungeon/DungeonGenVars.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenVars.cs | 24 | 2 | windowClosedGlassWallType | ushort | `public ushort windowClosedGlassWallType = 149;` | `public ushort windowClosedGlassWallType = 149;` |
| 1939 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenVars | Terraria.GameContent.Generation.Dungeon/DungeonGenVars.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenVars.cs | 26 | 2 | windowEdgeWallType | ushort | `public ushort windowEdgeWallType = 8;` | `public ushort windowEdgeWallType = 8;` |
| 1940 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenVars | Terraria.GameContent.Generation.Dungeon/DungeonGenVars.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenVars.cs | 28 | 2 | windowPlatformItemTypes | int[] | `public int[] windowPlatformItemTypes;` | `public int[] windowPlatformItemTypes;` |
| 1941 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenVars | Terraria.GameContent.Generation.Dungeon/DungeonGenVars.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenVars.cs | 30 | 2 | generatingDungeonPositionX | int | `public int generatingDungeonPositionX;` | `public int generatingDungeonPositionX;` |
| 1942 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenVars | Terraria.GameContent.Generation.Dungeon/DungeonGenVars.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenVars.cs | 32 | 2 | generatingDungeonPositionY | int | `public int generatingDungeonPositionY;` | `public int generatingDungeonPositionY;` |
| 1943 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenVars | Terraria.GameContent.Generation.Dungeon/DungeonGenVars.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenVars.cs | 34 | 2 | generatingDungeonTopX | int | `public int generatingDungeonTopX;` | `public int generatingDungeonTopX;` |
| 1944 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenVars | Terraria.GameContent.Generation.Dungeon/DungeonGenVars.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenVars.cs | 36 | 2 | dungeonLootStyle | int | `public int dungeonLootStyle;` | `public int dungeonLootStyle;` |
| 1945 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenVars | Terraria.GameContent.Generation.Dungeon/DungeonGenVars.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenVars.cs | 38 | 2 | outerPotentialDungeonBounds | Terraria.GameContent.Generation.Dungeon.DungeonBounds | `public DungeonBounds outerPotentialDungeonBounds = new DungeonBounds();` | `public DungeonBounds outerPotentialDungeonBounds = new DungeonBounds();` |
| 1946 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenVars | Terraria.GameContent.Generation.Dungeon/DungeonGenVars.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenVars.cs | 40 | 2 | innerPotentialDungeonBounds | Terraria.GameContent.Generation.Dungeon.DungeonBounds | `public DungeonBounds innerPotentialDungeonBounds = new DungeonBounds();` | `public DungeonBounds innerPotentialDungeonBounds = new DungeonBounds();` |
| 1957 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenVars | Terraria.GameContent.Generation.Dungeon/DungeonGenVars.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenVars.cs | 62 | 2 | dungeonEntrancePosition | Vector2D | `public Vector2D dungeonEntrancePosition;` | `public Vector2D dungeonEntrancePosition;` |

#### 属性（0）

无该类型成员记录。


### 4.19 细分子系统：`SharedDungeonLegacyRuleState`

- 原报告章节：`4.9.156`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`03` / `世界生成与地牢`

- 上一级基线细分子系统：`SharedDungeonLegacyGenerationGlobals`
- 细分职责：旧地牢样式、瓦片判定、预生成和战利品规则状态。
- 边界角色：`runtime state`；最小 seam：legacy dungeon rule port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：11；属性：0；合计：11。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（11）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1947 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenVars | Terraria.GameContent.Generation.Dungeon/DungeonGenVars.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenVars.cs | 42 | 2 | dungeonStyle | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyleData | `public DungeonGenerationStyleData dungeonStyle;` | `public DungeonGenerationStyleData dungeonStyle;` |
| 1948 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenVars | Terraria.GameContent.Generation.Dungeon/DungeonGenVars.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenVars.cs | 44 | 2 | dungeonGenerationStyles | System.Collections.Generic.List<Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyleData> | `public List<DungeonGenerationStyleData> dungeonGenerationStyles = new List<DungeonGenerationStyleData>();` | `public List<DungeonGenerationStyleData> dungeonGenerationStyles = new List<DungeonGenerationStyleData>();` |
| 1949 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenVars | Terraria.GameContent.Generation.Dungeon/DungeonGenVars.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenVars.cs | 46 | 2 | dungeonDitherSnake | Terraria.GameContent.Biomes.DitherSnake | `public DitherSnake dungeonDitherSnake = new DitherSnake();` | `public DitherSnake dungeonDitherSnake = new DitherSnake();` |
| 1950 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenVars | Terraria.GameContent.Generation.Dungeon/DungeonGenVars.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenVars.cs | 48 | 2 | isCrackedBrick | bool[] | `public bool[] isCrackedBrick;` | `public bool[] isCrackedBrick;` |
| 1951 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenVars | Terraria.GameContent.Generation.Dungeon/DungeonGenVars.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenVars.cs | 50 | 2 | isPitTrapTile | bool[] | `public bool[] isPitTrapTile;` | `public bool[] isPitTrapTile;` |
| 1952 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenVars | Terraria.GameContent.Generation.Dungeon/DungeonGenVars.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenVars.cs | 52 | 2 | isDungeonTile | bool[] | `public bool[] isDungeonTile;` | `public bool[] isDungeonTile;` |
| 1953 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenVars | Terraria.GameContent.Generation.Dungeon/DungeonGenVars.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenVars.cs | 54 | 2 | isDungeonWall | bool[] | `public bool[] isDungeonWall;` | `public bool[] isDungeonWall;` |
| 1954 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenVars | Terraria.GameContent.Generation.Dungeon/DungeonGenVars.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenVars.cs | 56 | 2 | isDungeonWallGlass | bool[] | `public bool[] isDungeonWallGlass;` | `public bool[] isDungeonWallGlass;` |
| 1955 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenVars | Terraria.GameContent.Generation.Dungeon/DungeonGenVars.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenVars.cs | 58 | 2 | GeneratingDungeon | bool | `public bool GeneratingDungeon;` | `public bool GeneratingDungeon;` |
| 1956 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenVars | Terraria.GameContent.Generation.Dungeon/DungeonGenVars.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenVars.cs | 60 | 2 | preGenDungeonEntranceSettings | Terraria.GameContent.Generation.Dungeon.Entrances.PreGenDungeonEntranceSettings | `public PreGenDungeonEntranceSettings preGenDungeonEntranceSettings;` | `public PreGenDungeonEntranceSettings preGenDungeonEntranceSettings;` |
| 1958 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenVars | Terraria.GameContent.Generation.Dungeon/DungeonGenVars.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenVars.cs | 64 | 2 | desertChestLootState | bool | `public bool desertChestLootState;` | `public bool desertChestLootState;` |

#### 属性（0）

无该类型成员记录。


### 4.20 细分子系统：`SharedDungeonRoomCoreState`

- 原报告章节：`4.9.163`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`03` / `世界生成与地牢`

- 上一级基线细分子系统：`SharedDungeonRoomDefinitions`
- 上一级 peer 细分子系统：`SharedDungeonRoomCoreAndSettings`
- 细分职责：地牢房间实例的计算、生成、边界和处理生命周期状态。
- 边界角色：`runtime state`；最小 seam：dungeon room lifecycle port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：5；属性：2；合计：7。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（5）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1774 | field | Terraria.GameContent.Generation.Dungeon.Rooms.DungeonRoom | Terraria.GameContent.Generation.Dungeon.Rooms/DungeonRoom.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms\DungeonRoom.cs | 11 | 2 | settings | Terraria.GameContent.Generation.Dungeon.Rooms.DungeonRoomSettings | `public DungeonRoomSettings settings;` | `public DungeonRoomSettings settings;` |
| 1775 | field | Terraria.GameContent.Generation.Dungeon.Rooms.DungeonRoom | Terraria.GameContent.Generation.Dungeon.Rooms/DungeonRoom.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms\DungeonRoom.cs | 13 | 2 | calculated | bool | `public bool calculated;` | `public bool calculated;` |
| 1776 | field | Terraria.GameContent.Generation.Dungeon.Rooms.DungeonRoom | Terraria.GameContent.Generation.Dungeon.Rooms/DungeonRoom.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms\DungeonRoom.cs | 15 | 2 | generated | bool | `public bool generated;` | `public bool generated;` |
| 1777 | field | Terraria.GameContent.Generation.Dungeon.Rooms.DungeonRoom | Terraria.GameContent.Generation.Dungeon.Rooms/DungeonRoom.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms\DungeonRoom.cs | 17 | 2 | InnerBounds | Terraria.GameContent.Generation.Dungeon.DungeonBounds | `public DungeonBounds InnerBounds = new DungeonBounds();` | `public DungeonBounds InnerBounds = new DungeonBounds();` |
| 1778 | field | Terraria.GameContent.Generation.Dungeon.Rooms.DungeonRoom | Terraria.GameContent.Generation.Dungeon.Rooms/DungeonRoom.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms\DungeonRoom.cs | 19 | 2 | OuterBounds | Terraria.GameContent.Generation.Dungeon.DungeonBounds | `public DungeonBounds OuterBounds = new DungeonBounds();` | `public DungeonBounds OuterBounds = new DungeonBounds();` |

#### 属性（2）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3787 | property | Terraria.GameContent.Generation.Dungeon.Rooms.DungeonRoom | Terraria.GameContent.Generation.Dungeon.Rooms/DungeonRoom.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms\DungeonRoom.cs | 21 | 2 | Processed | bool | `public bool Processed { get { if (!calculated) { return generated; } return true; } }` | `public bool Processed { get { if (!calculated) { return generated; } return true; } }` |
| 3788 | property | Terraria.GameContent.Generation.Dungeon.Rooms.DungeonRoom | Terraria.GameContent.Generation.Dungeon.Rooms/DungeonRoom.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms\DungeonRoom.cs | 33 | 2 | Center | Point | `public Point Center => InnerBounds.Center;` | `public Point Center => InnerBounds.Center;` |


### 4.21 细分子系统：`SharedDungeonRoomSettingsState`

- 原报告章节：`4.9.164`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`03` / `世界生成与地牢`

- 上一级基线细分子系统：`SharedDungeonRoomDefinitions`
- 上一级 peer 细分子系统：`SharedDungeonRoomCoreAndSettings`
- 细分职责：地牢房间类型、样式、进度和连接点配置。
- 边界角色：`definition/state`；最小 seam：dungeon room settings port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：4；声明类型数：4；字段：25；属性：0；合计：25。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（25）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1779 | field | Terraria.GameContent.Generation.Dungeon.Rooms.DungeonRoomSettings | Terraria.GameContent.Generation.Dungeon.Rooms/DungeonRoomSettings.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms\DungeonRoomSettings.cs | 8 | 2 | ControlLine | Terraria.GameContent.Biomes.DungeonControlLine | `public DungeonControlLine ControlLine;` | `public DungeonControlLine ControlLine;` |
| 1780 | field | Terraria.GameContent.Generation.Dungeon.Rooms.DungeonRoomSettings | Terraria.GameContent.Generation.Dungeon.Rooms/DungeonRoomSettings.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms\DungeonRoomSettings.cs | 10 | 2 | RoomPosition | Point | `public Point RoomPosition;` | `public Point RoomPosition;` |
| 1781 | field | Terraria.GameContent.Generation.Dungeon.Rooms.DungeonRoomSettings | Terraria.GameContent.Generation.Dungeon.Rooms/DungeonRoomSettings.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms\DungeonRoomSettings.cs | 12 | 2 | RoomType | Terraria.GameContent.Generation.Dungeon.Rooms.DungeonRoomType | `public DungeonRoomType RoomType;` | `public DungeonRoomType RoomType;` |
| 1782 | field | Terraria.GameContent.Generation.Dungeon.Rooms.DungeonRoomSettings | Terraria.GameContent.Generation.Dungeon.Rooms/DungeonRoomSettings.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms\DungeonRoomSettings.cs | 14 | 2 | RandomSeed | int | `public int RandomSeed;` | `public int RandomSeed;` |
| 1783 | field | Terraria.GameContent.Generation.Dungeon.Rooms.DungeonRoomSettings | Terraria.GameContent.Generation.Dungeon.Rooms/DungeonRoomSettings.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms\DungeonRoomSettings.cs | 16 | 2 | StyleData | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyleData | `public DungeonGenerationStyleData StyleData;` | `public DungeonGenerationStyleData StyleData;` |
| 1784 | field | Terraria.GameContent.Generation.Dungeon.Rooms.DungeonRoomSettings | Terraria.GameContent.Generation.Dungeon.Rooms/DungeonRoomSettings.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms\DungeonRoomSettings.cs | 18 | 2 | ProgressionStage | int | `public int ProgressionStage;` | `public int ProgressionStage;` |
| 1785 | field | Terraria.GameContent.Generation.Dungeon.Rooms.DungeonRoomSettings | Terraria.GameContent.Generation.Dungeon.Rooms/DungeonRoomSettings.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms\DungeonRoomSettings.cs | 20 | 2 | StartingRoom | bool | `public bool StartingRoom;` | `public bool StartingRoom;` |
| 1786 | field | Terraria.GameContent.Generation.Dungeon.Rooms.DungeonRoomSettings | Terraria.GameContent.Generation.Dungeon.Rooms/DungeonRoomSettings.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms\DungeonRoomSettings.cs | 22 | 2 | OverridePaintTile | int | `public int OverridePaintTile = -1;` | `public int OverridePaintTile = -1;` |
| 1787 | field | Terraria.GameContent.Generation.Dungeon.Rooms.DungeonRoomSettings | Terraria.GameContent.Generation.Dungeon.Rooms/DungeonRoomSettings.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms\DungeonRoomSettings.cs | 24 | 2 | OverridePaintWall | int | `public int OverridePaintWall = -1;` | `public int OverridePaintWall = -1;` |
| 1788 | field | Terraria.GameContent.Generation.Dungeon.Rooms.DungeonRoomSettings | Terraria.GameContent.Generation.Dungeon.Rooms/DungeonRoomSettings.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms\DungeonRoomSettings.cs | 26 | 2 | ForceStyleForDoorsAndPlatforms | bool | `public bool ForceStyleForDoorsAndPlatforms;` | `public bool ForceStyleForDoorsAndPlatforms;` |
| 1789 | field | Terraria.GameContent.Generation.Dungeon.Rooms.DungeonRoomSettings | Terraria.GameContent.Generation.Dungeon.Rooms/DungeonRoomSettings.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms\DungeonRoomSettings.cs | 28 | 2 | OnCurvedLine | bool | `public bool OnCurvedLine;` | `public bool OnCurvedLine;` |
| 1790 | field | Terraria.GameContent.Generation.Dungeon.Rooms.DungeonRoomSettings | Terraria.GameContent.Generation.Dungeon.Rooms/DungeonRoomSettings.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms\DungeonRoomSettings.cs | 30 | 2 | Orientation | Terraria.GameContent.Generation.Dungeon.SnakeOrientation | `public SnakeOrientation Orientation;` | `public SnakeOrientation Orientation;` |
| 1791 | field | Terraria.GameContent.Generation.Dungeon.Rooms.DungeonRoomSettings | Terraria.GameContent.Generation.Dungeon.Rooms/DungeonRoomSettings.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms\DungeonRoomSettings.cs | 32 | 2 | HallwayConnectionPointOverride | Terraria.GameContent.Generation.Dungeon.DungeonUtils.GetHallwayConnectionPoint | `public DungeonUtils.GetHallwayConnectionPoint HallwayConnectionPointOverride;` | `public DungeonUtils.GetHallwayConnectionPoint HallwayConnectionPointOverride;` |
| 1792 | field | Terraria.GameContent.Generation.Dungeon.Rooms.DungeonRoomSettings | Terraria.GameContent.Generation.Dungeon.Rooms/DungeonRoomSettings.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms\DungeonRoomSettings.cs | 34 | 2 | HallwayPointAdjuster | int? | `public int? HallwayPointAdjuster;` | `public int? HallwayPointAdjuster;` |
| 1795 | field | Terraria.GameContent.Generation.Dungeon.Rooms.GenShapeDungeonRoomSettings | Terraria.GameContent.Generation.Dungeon.Rooms/GenShapeDungeonRoomSettings.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms\GenShapeDungeonRoomSettings.cs | 7 | 2 | ShapeType | Terraria.GameContent.Generation.Dungeon.Rooms.GenShapeType | `public GenShapeType ShapeType;` | `public GenShapeType ShapeType;` |
| 1796 | field | Terraria.GameContent.Generation.Dungeon.Rooms.GenShapeDungeonRoomSettings | Terraria.GameContent.Generation.Dungeon.Rooms/GenShapeDungeonRoomSettings.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms\GenShapeDungeonRoomSettings.cs | 9 | 2 | InnerShape | Terraria.WorldBuilding.GenShape | `public GenShape InnerShape;` | `public GenShape InnerShape;` |
| 1797 | field | Terraria.GameContent.Generation.Dungeon.Rooms.GenShapeDungeonRoomSettings | Terraria.GameContent.Generation.Dungeon.Rooms/GenShapeDungeonRoomSettings.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms\GenShapeDungeonRoomSettings.cs | 11 | 2 | OuterShape | Terraria.WorldBuilding.GenShape | `public GenShape OuterShape;` | `public GenShape OuterShape;` |
| 1798 | field | Terraria.GameContent.Generation.Dungeon.Rooms.GenShapeDungeonRoomSettings | Terraria.GameContent.Generation.Dungeon.Rooms/GenShapeDungeonRoomSettings.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms\GenShapeDungeonRoomSettings.cs | 13 | 2 | BoundingRadius | int | `public int BoundingRadius;` | `public int BoundingRadius;` |
| 1804 | field | Terraria.GameContent.Generation.Dungeon.Rooms.LegacyDungeonRoomSettings | Terraria.GameContent.Generation.Dungeon.Rooms/LegacyDungeonRoomSettings.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms\LegacyDungeonRoomSettings.cs | 5 | 2 | IsEntranceRoom | bool | `public bool IsEntranceRoom;` | `public bool IsEntranceRoom;` |
| 1808 | field | Terraria.GameContent.Generation.Dungeon.Rooms.StepBasedDungeonRoomSettings | Terraria.GameContent.Generation.Dungeon.Rooms/StepBasedDungeonRoomSettings.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms\StepBasedDungeonRoomSettings.cs | 7 | 2 | OverrideStrength | int | `public int OverrideStrength;` | `public int OverrideStrength;` |
| 1809 | field | Terraria.GameContent.Generation.Dungeon.Rooms.StepBasedDungeonRoomSettings | Terraria.GameContent.Generation.Dungeon.Rooms/StepBasedDungeonRoomSettings.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms\StepBasedDungeonRoomSettings.cs | 9 | 2 | OverrideSteps | int | `public int OverrideSteps;` | `public int OverrideSteps;` |
| 1810 | field | Terraria.GameContent.Generation.Dungeon.Rooms.StepBasedDungeonRoomSettings | Terraria.GameContent.Generation.Dungeon.Rooms/StepBasedDungeonRoomSettings.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms\StepBasedDungeonRoomSettings.cs | 11 | 2 | OverrideStartPosition | Vector2D | `public Vector2D OverrideStartPosition;` | `public Vector2D OverrideStartPosition;` |
| 1811 | field | Terraria.GameContent.Generation.Dungeon.Rooms.StepBasedDungeonRoomSettings | Terraria.GameContent.Generation.Dungeon.Rooms/StepBasedDungeonRoomSettings.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms\StepBasedDungeonRoomSettings.cs | 13 | 2 | OverrideEndPosition | Vector2D | `public Vector2D OverrideEndPosition;` | `public Vector2D OverrideEndPosition;` |
| 1812 | field | Terraria.GameContent.Generation.Dungeon.Rooms.StepBasedDungeonRoomSettings | Terraria.GameContent.Generation.Dungeon.Rooms/StepBasedDungeonRoomSettings.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms\StepBasedDungeonRoomSettings.cs | 15 | 2 | OverrideVelocity | Vector2D | `public Vector2D OverrideVelocity;` | `public Vector2D OverrideVelocity;` |
| 1813 | field | Terraria.GameContent.Generation.Dungeon.Rooms.StepBasedDungeonRoomSettings | Terraria.GameContent.Generation.Dungeon.Rooms/StepBasedDungeonRoomSettings.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms\StepBasedDungeonRoomSettings.cs | 17 | 2 | OverrideInteriorToExteriorRatio | double | `public double OverrideInteriorToExteriorRatio;` | `public double OverrideInteriorToExteriorRatio;` |

#### 属性（0）

无该类型成员记录。


### 4.22 细分子系统：`SharedDungeonHallCoreState`

- 原报告章节：`4.9.165`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`03` / `世界生成与地牢`

- 上一级基线细分子系统：`SharedDungeonHallDefinitions`
- 上一级 peer 细分子系统：`SharedDungeonHallCoreAndSettings`
- 细分职责：地牢大厅实例的计算、生成、端点和处理生命周期状态。
- 边界角色：`runtime state`；最小 seam：dungeon hall lifecycle port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：9；属性：1；合计：10。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（9）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1693 | field | Terraria.GameContent.Generation.Dungeon.Halls.DungeonHall | Terraria.GameContent.Generation.Dungeon.Halls/DungeonHall.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Halls\DungeonHall.cs | 10 | 2 | settings | Terraria.GameContent.Generation.Dungeon.Halls.DungeonHallSettings | `public DungeonHallSettings settings;` | `public DungeonHallSettings settings;` |
| 1694 | field | Terraria.GameContent.Generation.Dungeon.Halls.DungeonHall | Terraria.GameContent.Generation.Dungeon.Halls/DungeonHall.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Halls\DungeonHall.cs | 12 | 2 | calculated | bool | `public bool calculated;` | `public bool calculated;` |
| 1695 | field | Terraria.GameContent.Generation.Dungeon.Halls.DungeonHall | Terraria.GameContent.Generation.Dungeon.Halls/DungeonHall.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Halls\DungeonHall.cs | 14 | 2 | generated | bool | `public bool generated;` | `public bool generated;` |
| 1696 | field | Terraria.GameContent.Generation.Dungeon.Halls.DungeonHall | Terraria.GameContent.Generation.Dungeon.Halls/DungeonHall.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Halls\DungeonHall.cs | 16 | 2 | Bounds | Terraria.GameContent.Generation.Dungeon.DungeonBounds | `public DungeonBounds Bounds = new DungeonBounds();` | `public DungeonBounds Bounds = new DungeonBounds();` |
| 1697 | field | Terraria.GameContent.Generation.Dungeon.Halls.DungeonHall | Terraria.GameContent.Generation.Dungeon.Halls/DungeonHall.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Halls\DungeonHall.cs | 18 | 2 | StartPosition | Vector2D | `public Vector2D StartPosition;` | `public Vector2D StartPosition;` |
| 1698 | field | Terraria.GameContent.Generation.Dungeon.Halls.DungeonHall | Terraria.GameContent.Generation.Dungeon.Halls/DungeonHall.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Halls\DungeonHall.cs | 20 | 2 | EndPosition | Vector2D | `public Vector2D EndPosition;` | `public Vector2D EndPosition;` |
| 1699 | field | Terraria.GameContent.Generation.Dungeon.Halls.DungeonHall | Terraria.GameContent.Generation.Dungeon.Halls/DungeonHall.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Halls\DungeonHall.cs | 22 | 2 | StartDirection | Vector2D | `public Vector2D StartDirection;` | `public Vector2D StartDirection;` |
| 1700 | field | Terraria.GameContent.Generation.Dungeon.Halls.DungeonHall | Terraria.GameContent.Generation.Dungeon.Halls/DungeonHall.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Halls\DungeonHall.cs | 24 | 2 | EndDirection | Vector2D | `public Vector2D EndDirection;` | `public Vector2D EndDirection;` |
| 1701 | field | Terraria.GameContent.Generation.Dungeon.Halls.DungeonHall | Terraria.GameContent.Generation.Dungeon.Halls/DungeonHall.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Halls\DungeonHall.cs | 26 | 2 | CrackedBrick | bool | `public bool CrackedBrick;` | `public bool CrackedBrick;` |

#### 属性（1）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3786 | property | Terraria.GameContent.Generation.Dungeon.Halls.DungeonHall | Terraria.GameContent.Generation.Dungeon.Halls/DungeonHall.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Halls\DungeonHall.cs | 28 | 2 | Processed | bool | `public bool Processed { get { if (!calculated) { return generated; } return true; } }` | `public bool Processed { get { if (!calculated) { return generated; } return true; } }` |


### 4.23 细分子系统：`SharedDungeonHallSettingsState`

- 原报告章节：`4.9.166`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`03` / `世界生成与地牢`

- 上一级基线细分子系统：`SharedDungeonHallDefinitions`
- 上一级 peer 细分子系统：`SharedDungeonHallCoreAndSettings`
- 细分职责：地牢大厅类型、样式、裂砖和生成策略配置。
- 边界角色：`definition/state`；最小 seam：dungeon hall settings port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：4；声明类型数：4；字段：19；属性：0；合计：19。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（19）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1702 | field | Terraria.GameContent.Generation.Dungeon.Halls.DungeonHallSettings | Terraria.GameContent.Generation.Dungeon.Halls/DungeonHallSettings.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Halls\DungeonHallSettings.cs | 5 | 2 | HallType | Terraria.GameContent.Generation.Dungeon.Halls.DungeonHallType | `public DungeonHallType HallType;` | `public DungeonHallType HallType;` |
| 1703 | field | Terraria.GameContent.Generation.Dungeon.Halls.DungeonHallSettings | Terraria.GameContent.Generation.Dungeon.Halls/DungeonHallSettings.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Halls\DungeonHallSettings.cs | 7 | 2 | RandomSeed | int | `public int RandomSeed;` | `public int RandomSeed;` |
| 1704 | field | Terraria.GameContent.Generation.Dungeon.Halls.DungeonHallSettings | Terraria.GameContent.Generation.Dungeon.Halls/DungeonHallSettings.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Halls\DungeonHallSettings.cs | 9 | 2 | StyleData | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyleData | `public DungeonGenerationStyleData StyleData;` | `public DungeonGenerationStyleData StyleData;` |
| 1705 | field | Terraria.GameContent.Generation.Dungeon.Halls.DungeonHallSettings | Terraria.GameContent.Generation.Dungeon.Halls/DungeonHallSettings.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Halls\DungeonHallSettings.cs | 11 | 2 | OverridePaintTile | int | `public int OverridePaintTile = -1;` | `public int OverridePaintTile = -1;` |
| 1706 | field | Terraria.GameContent.Generation.Dungeon.Halls.DungeonHallSettings | Terraria.GameContent.Generation.Dungeon.Halls/DungeonHallSettings.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Halls\DungeonHallSettings.cs | 13 | 2 | OverridePaintWall | int | `public int OverridePaintWall = -1;` | `public int OverridePaintWall = -1;` |
| 1707 | field | Terraria.GameContent.Generation.Dungeon.Halls.DungeonHallSettings | Terraria.GameContent.Generation.Dungeon.Halls/DungeonHallSettings.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Halls\DungeonHallSettings.cs | 15 | 2 | CrackedBrickChance | double | `public double CrackedBrickChance = 0.166;` | `public double CrackedBrickChance = 0.166;` |
| 1708 | field | Terraria.GameContent.Generation.Dungeon.Halls.DungeonHallSettings | Terraria.GameContent.Generation.Dungeon.Halls/DungeonHallSettings.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Halls\DungeonHallSettings.cs | 17 | 2 | PlaceOverProtectedBricks | bool | `public bool PlaceOverProtectedBricks;` | `public bool PlaceOverProtectedBricks;` |
| 1709 | field | Terraria.GameContent.Generation.Dungeon.Halls.DungeonHallSettings | Terraria.GameContent.Generation.Dungeon.Halls/DungeonHallSettings.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Halls\DungeonHallSettings.cs | 19 | 2 | ZigzagChance | double | `public double ZigzagChance = 0.66;` | `public double ZigzagChance = 0.66;` |
| 1710 | field | Terraria.GameContent.Generation.Dungeon.Halls.DungeonHallSettings | Terraria.GameContent.Generation.Dungeon.Halls/DungeonHallSettings.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Halls\DungeonHallSettings.cs | 21 | 2 | ForceStyleForDoorsAndPlatforms | bool | `public bool ForceStyleForDoorsAndPlatforms;` | `public bool ForceStyleForDoorsAndPlatforms;` |
| 1711 | field | Terraria.GameContent.Generation.Dungeon.Halls.DungeonHallSettings | Terraria.GameContent.Generation.Dungeon.Halls/DungeonHallSettings.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Halls\DungeonHallSettings.cs | 23 | 2 | CarveOnly | bool | `public bool CarveOnly;` | `public bool CarveOnly;` |
| 1719 | field | Terraria.GameContent.Generation.Dungeon.Halls.RegularDungeonHallSettings | Terraria.GameContent.Generation.Dungeon.Halls/RegularDungeonHallSettings.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Halls\RegularDungeonHallSettings.cs | 5 | 2 | OverrideInnerBoundsSize | int | `public int OverrideInnerBoundsSize;` | `public int OverrideInnerBoundsSize;` |
| 1720 | field | Terraria.GameContent.Generation.Dungeon.Halls.RegularDungeonHallSettings | Terraria.GameContent.Generation.Dungeon.Halls/RegularDungeonHallSettings.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Halls\RegularDungeonHallSettings.cs | 7 | 2 | OverrideOuterBoundsSize | int | `public int OverrideOuterBoundsSize;` | `public int OverrideOuterBoundsSize;` |
| 1722 | field | Terraria.GameContent.Generation.Dungeon.Halls.SineDungeonHallSettings | Terraria.GameContent.Generation.Dungeon.Halls/SineDungeonHallSettings.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Halls\SineDungeonHallSettings.cs | 5 | 2 | Magnitude | float | `public float Magnitude = 1f;` | `public float Magnitude = 1f;` |
| 1723 | field | Terraria.GameContent.Generation.Dungeon.Halls.SineDungeonHallSettings | Terraria.GameContent.Generation.Dungeon.Halls/SineDungeonHallSettings.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Halls\SineDungeonHallSettings.cs | 7 | 2 | Iterations | int | `public int Iterations = 1;` | `public int Iterations = 1;` |
| 1724 | field | Terraria.GameContent.Generation.Dungeon.Halls.SineDungeonHallSettings | Terraria.GameContent.Generation.Dungeon.Halls/SineDungeonHallSettings.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Halls\SineDungeonHallSettings.cs | 9 | 2 | FlipSine | bool | `public bool FlipSine;` | `public bool FlipSine;` |
| 1731 | field | Terraria.GameContent.Generation.Dungeon.Halls.StepBasedDungeonHallSettings | Terraria.GameContent.Generation.Dungeon.Halls/StepBasedDungeonHallSettings.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Halls\StepBasedDungeonHallSettings.cs | 5 | 2 | OverrideStrength | int | `public int OverrideStrength;` | `public int OverrideStrength;` |
| 1732 | field | Terraria.GameContent.Generation.Dungeon.Halls.StepBasedDungeonHallSettings | Terraria.GameContent.Generation.Dungeon.Halls/StepBasedDungeonHallSettings.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Halls\StepBasedDungeonHallSettings.cs | 7 | 2 | OverrideSteps | int | `public int OverrideSteps;` | `public int OverrideSteps;` |
| 1733 | field | Terraria.GameContent.Generation.Dungeon.Halls.StepBasedDungeonHallSettings | Terraria.GameContent.Generation.Dungeon.Halls/StepBasedDungeonHallSettings.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Halls\StepBasedDungeonHallSettings.cs | 9 | 2 | ForceHorizontal | bool | `public bool ForceHorizontal;` | `public bool ForceHorizontal;` |
| 1734 | field | Terraria.GameContent.Generation.Dungeon.Halls.StepBasedDungeonHallSettings | Terraria.GameContent.Generation.Dungeon.Halls/StepBasedDungeonHallSettings.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Halls\StepBasedDungeonHallSettings.cs | 11 | 2 | OverrideInteriorToExteriorRatio | double | `public double OverrideInteriorToExteriorRatio;` | `public double OverrideInteriorToExteriorRatio;` |

#### 属性（0）

无该类型成员记录。


### 4.24 细分子系统：`SharedBiomeTerrainPassState`

- 原报告章节：`4.9.169`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`03` / `世界生成与地牢`

- 上一级基线细分子系统：`SharedWorldGenerationBiomeSupport`
- 上一级 peer 细分子系统：`SharedBiomeDungeonAndTerrainState`
- 细分职责：生物群落、洞穴、沙漠和地形 pass 的生成数据。
- 边界角色：`definition/query`；最小 seam：biome terrain pass port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：6；声明类型数：7；字段：14；属性：2；合计：16。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（14）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1426 | field | Terraria.GameContent.Biomes.DesertBiome | Terraria.GameContent.Biomes/DesertBiome.cs | D:\TRbackup\Version4\Terraria.GameContent.Biomes\DesertBiome.cs | 10 | 2 | ChanceOfEntrance | double | `[JsonProperty("ChanceOfEntrance")] public double ChanceOfEntrance = 0.3333;` | `[JsonProperty("ChanceOfEntrance")] public double ChanceOfEntrance = 0.3333;` |
| 1427 | field | Terraria.GameContent.Biomes.DitherSnake | Terraria.GameContent.Biomes/DitherSnake.cs | D:\TRbackup\Version4\Terraria.GameContent.Biomes\DitherSnake.cs | 12 | 2 | CircleTestPoints | Vector2D[] | `private static readonly Vector2D[] CircleTestPoints = (from i in Enumerable.Range(0, 12)  		select Vector2D.UnitX.RotatedBy(Math.PI * 2.0 * (double)i / 12.0)).ToArray();` | `private static readonly Vector2D[] CircleTestPoints = (from i in Enumerable.Range(0, 12) select Vector2D.UnitX.RotatedBy(Math.PI * 2.0 * (double)i / 12.0)).ToArray();` |
| 1428 | field | Terraria.GameContent.Biomes.DitherSnake | Terraria.GameContent.Biomes/DitherSnake.cs | D:\TRbackup\Version4\Terraria.GameContent.Biomes\DitherSnake.cs | 15 | 2 | ExtraBuffer | double | `private static readonly double ExtraBuffer = 1.0 / Math.Cos(Math.PI / 6.0);` | `private static readonly double ExtraBuffer = 1.0 / Math.Cos(Math.PI / 6.0);` |
| 1429 | field | Terraria.GameContent.Biomes.DunesBiome | Terraria.GameContent.Biomes/DunesBiome.cs | D:\TRbackup\Version4\Terraria.GameContent.Biomes\DunesBiome.cs | 20 | 2 | _singleDunesWidth | Terraria.WorldBuilding.WorldGenRange | `[JsonProperty("SingleDunesWidth")] private WorldGenRange _singleDunesWidth = WorldGenRange.Empty;` | `[JsonProperty("SingleDunesWidth")] private WorldGenRange _singleDunesWidth = WorldGenRange.Empty;` |
| 1450 | field | Terraria.GameContent.Biomes.GraniteBiome.Magma | Terraria.GameContent.Biomes/GraniteBiome.cs | D:\TRbackup\Version4\Terraria.GameContent.Biomes\GraniteBiome.cs | 16 | 3 | Pressure | double | `public readonly double Pressure;` | `public readonly double Pressure;` |
| 1451 | field | Terraria.GameContent.Biomes.GraniteBiome.Magma | Terraria.GameContent.Biomes/GraniteBiome.cs | D:\TRbackup\Version4\Terraria.GameContent.Biomes\GraniteBiome.cs | 18 | 3 | Resistance | double | `public readonly double Resistance;` | `public readonly double Resistance;` |
| 1452 | field | Terraria.GameContent.Biomes.GraniteBiome.Magma | Terraria.GameContent.Biomes/GraniteBiome.cs | D:\TRbackup\Version4\Terraria.GameContent.Biomes\GraniteBiome.cs | 20 | 3 | IsActive | bool | `public readonly bool IsActive;` | `public readonly bool IsActive;` |
| 1453 | field | Terraria.GameContent.Biomes.GraniteBiome | Terraria.GameContent.Biomes/GraniteBiome.cs | D:\TRbackup\Version4\Terraria.GameContent.Biomes\GraniteBiome.cs | 30 | 2 | MAX_MAGMA_ITERATIONS | int | `private const int MAX_MAGMA_ITERATIONS = 300;` | `private const int MAX_MAGMA_ITERATIONS = 300;` |
| 1454 | field | Terraria.GameContent.Biomes.GraniteBiome | Terraria.GameContent.Biomes/GraniteBiome.cs | D:\TRbackup\Version4\Terraria.GameContent.Biomes\GraniteBiome.cs | 32 | 2 | _sourceMagmaMap | Terraria.GameContent.Biomes.GraniteBiome.Magma[,] | `private Magma[,] _sourceMagmaMap = new Magma[200, 200];` | `private Magma[,] _sourceMagmaMap = new Magma[200, 200];` |
| 1455 | field | Terraria.GameContent.Biomes.GraniteBiome | Terraria.GameContent.Biomes/GraniteBiome.cs | D:\TRbackup\Version4\Terraria.GameContent.Biomes\GraniteBiome.cs | 34 | 2 | _targetMagmaMap | Terraria.GameContent.Biomes.GraniteBiome.Magma[,] | `private Magma[,] _targetMagmaMap = new Magma[200, 200];` | `private Magma[,] _targetMagmaMap = new Magma[200, 200];` |
| 1456 | field | Terraria.GameContent.Biomes.GraniteBiome | Terraria.GameContent.Biomes/GraniteBiome.cs | D:\TRbackup\Version4\Terraria.GameContent.Biomes\GraniteBiome.cs | 36 | 2 | _normalisedVectors | Vector2D[] | `private static Vector2D[] _normalisedVectors = new Vector2D[9]  	{  		Vector2D.Normalize(new Vector2D(-1.0, -1.0)),  		Vector2D.Normalize(new Vector2D(-1.0, 0.0)),  		Vector2D.Normalize(new Vector2D(-1.0, 1.0)),  		Vector2D.Normalize(new Vector2D(0.0, -1.0)),  		new Vector2D(0.0, 0.0),  		Vector2D.Normalize(new Vector2D(0.0, 1.0)),  		Vector2D.Normalize(new Vector2D(1.0, -1.0)),  		Vector2D.Normalize(new Vector2D(1.0, 0.0)),  		Vector2D.Normalize(new Vector2D(1.0, 1.0))  	};` | `private static Vector2D[] _normalisedVectors = new Vector2D[9] { Vector2D.Normalize(new Vector2D(-1.0, -1.0)), Vector2D.Normalize(new Vector2D(-1.0, 0.0)), Vector2D.Normalize(new Vector2D(-1.0, 1.0)), Vector2D.Normalize(new Vector2D(0.0, -1.0)), new Vector2D(0.0, 0.0), Vector2D.Normalize(new Vector2D(0.0, 1.0)), Vector2D.Normalize(new Vector2D(1.0, -1.0)), Vector2D.Normalize(new Vector2D(1.0, 0.0)), Vector2D.Normalize(new Vector2D(1.0, 1.0)) };` |
| 1457 | field | Terraria.GameContent.Biomes.MarbleBiome | Terraria.GameContent.Biomes/MarbleBiome.cs | D:\TRbackup\Version4\Terraria.GameContent.Biomes\MarbleBiome.cs | 23 | 2 | SCALE | int | `private const int SCALE = 3;` | `private const int SCALE = 3;` |
| 1458 | field | Terraria.GameContent.Biomes.TerrainPass.SurfaceHistory | Terraria.GameContent.Biomes/TerrainPass.cs | D:\TRbackup\Version4\Terraria.GameContent.Biomes\TerrainPass.cs | 13 | 3 | _heights | double[] | `private readonly double[] _heights;` | `private readonly double[] _heights;` |
| 1459 | field | Terraria.GameContent.Biomes.TerrainPass.SurfaceHistory | Terraria.GameContent.Biomes/TerrainPass.cs | D:\TRbackup\Version4\Terraria.GameContent.Biomes\TerrainPass.cs | 15 | 3 | _index | int | `private int _index;` | `private int _index;` |

#### 属性（2）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3745 | property | Terraria.GameContent.Biomes.DunesBiome | Terraria.GameContent.Biomes/DunesBiome.cs | D:\TRbackup\Version4\Terraria.GameContent.Biomes\DunesBiome.cs | 23 | 2 | MaximumWidth | int | `public int MaximumWidth => _singleDunesWidth.ScaledMaximum * 2;` | `public int MaximumWidth => _singleDunesWidth.ScaledMaximum * 2;` |
| 3747 | property | Terraria.GameContent.Biomes.TerrainPass.SurfaceHistory | Terraria.GameContent.Biomes/TerrainPass.cs | D:\TRbackup\Version4\Terraria.GameContent.Biomes\TerrainPass.cs | 17 | 3 | this[] | double | `public double this[int index] { get { return _heights[(index + _index) % _heights.Length]; } set { _heights[(index + _index) % _heights.Length] = value; } }` | `public double this[int index] { get { return _heights[(index + _index) % _heights.Length]; } set { _heights[(index + _index) % _heights.Length] = value; } }` |


### 4.25 细分子系统：`SharedDungeonStyleMaterialAndGeometryState`

- 原报告章节：`4.9.174`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`03` / `世界生成与地牢`

- 上一级基线细分子系统：`SharedDungeonStyleCatalog`
- 上一级 peer 细分子系统：`SharedDungeonStyleEntryDefinitions`
- 细分职责：地牢样式砖、墙、液体、边缘和几何材料定义。
- 边界角色：`definition/catalog`；最小 seam：dungeon style material geometry port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：12；属性：0；合计：12。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（12）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1877 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyleData | Terraria.GameContent.Generation.Dungeon/DungeonGenerationStyleData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenerationStyleData.cs | 11 | 2 | Style | byte | `public byte Style;` | `public byte Style;` |
| 1878 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyleData | Terraria.GameContent.Generation.Dungeon/DungeonGenerationStyleData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenerationStyleData.cs | 13 | 2 | UnbreakableWallProgressionTier | int | `public int UnbreakableWallProgressionTier = -1;` | `public int UnbreakableWallProgressionTier = -1;` |
| 1879 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyleData | Terraria.GameContent.Generation.Dungeon/DungeonGenerationStyleData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenerationStyleData.cs | 15 | 2 | BrickTileType | ushort | `public ushort BrickTileType;` | `public ushort BrickTileType;` |
| 1880 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyleData | Terraria.GameContent.Generation.Dungeon/DungeonGenerationStyleData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenerationStyleData.cs | 17 | 2 | BrickGrassTileType | ushort? | `public ushort? BrickGrassTileType;` | `public ushort? BrickGrassTileType;` |
| 1881 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyleData | Terraria.GameContent.Generation.Dungeon/DungeonGenerationStyleData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenerationStyleData.cs | 19 | 2 | BrickCrackedTileType | ushort | `public ushort BrickCrackedTileType;` | `public ushort BrickCrackedTileType;` |
| 1882 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyleData | Terraria.GameContent.Generation.Dungeon/DungeonGenerationStyleData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenerationStyleData.cs | 21 | 2 | BrickWallType | ushort | `public ushort BrickWallType;` | `public ushort BrickWallType;` |
| 1883 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyleData | Terraria.GameContent.Generation.Dungeon/DungeonGenerationStyleData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenerationStyleData.cs | 23 | 2 | WindowGlassWallType | ushort | `public ushort WindowGlassWallType;` | `public ushort WindowGlassWallType;` |
| 1884 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyleData | Terraria.GameContent.Generation.Dungeon/DungeonGenerationStyleData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenerationStyleData.cs | 25 | 2 | WindowClosedGlassWallType | ushort | `public ushort WindowClosedGlassWallType;` | `public ushort WindowClosedGlassWallType;` |
| 1885 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyleData | Terraria.GameContent.Generation.Dungeon/DungeonGenerationStyleData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenerationStyleData.cs | 27 | 2 | WindowEdgeWallType | ushort | `public ushort WindowEdgeWallType;` | `public ushort WindowEdgeWallType;` |
| 1887 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyleData | Terraria.GameContent.Generation.Dungeon/DungeonGenerationStyleData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenerationStyleData.cs | 31 | 2 | PitTrapTileType | ushort | `public ushort PitTrapTileType;` | `public ushort PitTrapTileType;` |
| 1888 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyleData | Terraria.GameContent.Generation.Dungeon/DungeonGenerationStyleData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenerationStyleData.cs | 33 | 2 | LiquidType | int | `public int LiquidType = -1;` | `public int LiquidType = -1;` |
| 1913 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyleData | Terraria.GameContent.Generation.Dungeon/DungeonGenerationStyleData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenerationStyleData.cs | 83 | 2 | EdgeDither | bool | `public bool EdgeDither;` | `public bool EdgeDither;` |

#### 属性（0）

无该类型成员记录。


### 4.26 细分子系统：`SharedDungeonRoomVariantCatalogState`

- 原报告章节：`4.9.177`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`03` / `世界生成与地牢`

- 上一级基线细分子系统：`SharedDungeonRoomDefinitions`
- 上一级 peer 细分子系统：`SharedDungeonRoomShapeVariants`
- 细分职责：地牢房间 VARIANT、Biome 房间和最大变体目录。
- 边界角色：`definition/catalog`；最小 seam：dungeon room variant catalog port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：2；声明类型数：2；字段：8；属性：0；合计：8。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（8）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1752 | field | Terraria.GameContent.Generation.Dungeon.Rooms.BiomeDungeonRoom | Terraria.GameContent.Generation.Dungeon.Rooms/BiomeDungeonRoom.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms\BiomeDungeonRoom.cs | 12 | 2 | BIOMEROOM_INNER_SIZE_BASE | int | `protected const int BIOMEROOM_INNER_SIZE_BASE = 32;` | `protected const int BIOMEROOM_INNER_SIZE_BASE = 32;` |
| 1753 | field | Terraria.GameContent.Generation.Dungeon.Rooms.BiomeDungeonRoom | Terraria.GameContent.Generation.Dungeon.Rooms/BiomeDungeonRoom.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms\BiomeDungeonRoom.cs | 14 | 2 | BIOMEROOM_INNER_SIZE_BASE_TEMPLE | int | `protected const int BIOMEROOM_INNER_SIZE_BASE_TEMPLE = 50;` | `protected const int BIOMEROOM_INNER_SIZE_BASE_TEMPLE = 50;` |
| 1754 | field | Terraria.GameContent.Generation.Dungeon.Rooms.BiomeDungeonRoom | Terraria.GameContent.Generation.Dungeon.Rooms/BiomeDungeonRoom.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms\BiomeDungeonRoom.cs | 16 | 2 | BIOMEROOM_WALL_DEPTH | int | `protected const int BIOMEROOM_WALL_DEPTH = 8;` | `protected const int BIOMEROOM_WALL_DEPTH = 8;` |
| 1765 | field | Terraria.GameContent.Generation.Dungeon.Rooms.BiomeStructuredDungeonRoom | Terraria.GameContent.Generation.Dungeon.Rooms/BiomeStructuredDungeonRoom.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms\BiomeStructuredDungeonRoom.cs | 11 | 2 | VARIANT_DOUBLEDIAMOND | int | `public const int VARIANT_DOUBLEDIAMOND = 0;` | `public const int VARIANT_DOUBLEDIAMOND = 0;` |
| 1766 | field | Terraria.GameContent.Generation.Dungeon.Rooms.BiomeStructuredDungeonRoom | Terraria.GameContent.Generation.Dungeon.Rooms/BiomeStructuredDungeonRoom.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms\BiomeStructuredDungeonRoom.cs | 13 | 2 | VARIANT_ROUNDED | int | `public const int VARIANT_ROUNDED = 1;` | `public const int VARIANT_ROUNDED = 1;` |
| 1767 | field | Terraria.GameContent.Generation.Dungeon.Rooms.BiomeStructuredDungeonRoom | Terraria.GameContent.Generation.Dungeon.Rooms/BiomeStructuredDungeonRoom.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms\BiomeStructuredDungeonRoom.cs | 15 | 2 | VARIANT_CANDY | int | `public const int VARIANT_CANDY = 2;` | `public const int VARIANT_CANDY = 2;` |
| 1768 | field | Terraria.GameContent.Generation.Dungeon.Rooms.BiomeStructuredDungeonRoom | Terraria.GameContent.Generation.Dungeon.Rooms/BiomeStructuredDungeonRoom.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms\BiomeStructuredDungeonRoom.cs | 17 | 2 | VARIANT_WIGGLED | int | `public const int VARIANT_WIGGLED = 3;` | `public const int VARIANT_WIGGLED = 3;` |
| 1769 | field | Terraria.GameContent.Generation.Dungeon.Rooms.BiomeStructuredDungeonRoom | Terraria.GameContent.Generation.Dungeon.Rooms/BiomeStructuredDungeonRoom.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms\BiomeStructuredDungeonRoom.cs | 19 | 2 | MAX_VARIANTS | int | `public const int MAX_VARIANTS = 4;` | `public const int MAX_VARIANTS = 4;` |

#### 属性（0）

无该类型成员记录。


### 4.27 细分子系统：`SharedDungeonStyleObjectConstants`

- 原报告章节：`4.9.184`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`03` / `世界生成与地牢`

- 上一级基线细分子系统：`SharedDungeonGenerationQueries`
- 上一级 peer 细分子系统：`SharedDungeonStyleConstantQueries`
- 细分职责：地牢门、花盆、吊灯和平台对象常量查询。
- 边界角色：`query/value object`；最小 seam：dungeon style object constants port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：21；属性：0；合计：21。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（21）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1975 | field | Terraria.GameContent.Generation.Dungeon.DungeonUtils | Terraria.GameContent.Generation.Dungeon/DungeonUtils.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonUtils.cs | 19 | 2 | DOORSTYLE_WOODEN | int | `public const int DOORSTYLE_WOODEN = 13;` | `public const int DOORSTYLE_WOODEN = 13;` |
| 1976 | field | Terraria.GameContent.Generation.Dungeon.DungeonUtils | Terraria.GameContent.Generation.Dungeon/DungeonUtils.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonUtils.cs | 21 | 2 | DOORSTYLE_BLUEBRICK | int | `public const int DOORSTYLE_BLUEBRICK = 16;` | `public const int DOORSTYLE_BLUEBRICK = 16;` |
| 1977 | field | Terraria.GameContent.Generation.Dungeon.DungeonUtils | Terraria.GameContent.Generation.Dungeon/DungeonUtils.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonUtils.cs | 23 | 2 | DOORSTYLE_GREENBRICK | int | `public const int DOORSTYLE_GREENBRICK = 17;` | `public const int DOORSTYLE_GREENBRICK = 17;` |
| 1978 | field | Terraria.GameContent.Generation.Dungeon.DungeonUtils | Terraria.GameContent.Generation.Dungeon/DungeonUtils.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonUtils.cs | 25 | 2 | DOORSTYLE_PINKBRICK | int | `public const int DOORSTYLE_PINKBRICK = 18;` | `public const int DOORSTYLE_PINKBRICK = 18;` |
| 1979 | field | Terraria.GameContent.Generation.Dungeon.DungeonUtils | Terraria.GameContent.Generation.Dungeon/DungeonUtils.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonUtils.cs | 27 | 2 | POTSTYLE_NORMAL_1 | int | `public const int POTSTYLE_NORMAL_1 = 0;` | `public const int POTSTYLE_NORMAL_1 = 0;` |
| 1980 | field | Terraria.GameContent.Generation.Dungeon.DungeonUtils | Terraria.GameContent.Generation.Dungeon/DungeonUtils.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonUtils.cs | 29 | 2 | POTSTYLE_NORMAL_2 | int | `public const int POTSTYLE_NORMAL_2 = 1;` | `public const int POTSTYLE_NORMAL_2 = 1;` |
| 1981 | field | Terraria.GameContent.Generation.Dungeon.DungeonUtils | Terraria.GameContent.Generation.Dungeon/DungeonUtils.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonUtils.cs | 31 | 2 | POTSTYLE_NORMAL_3 | int | `public const int POTSTYLE_NORMAL_3 = 2;` | `public const int POTSTYLE_NORMAL_3 = 2;` |
| 1982 | field | Terraria.GameContent.Generation.Dungeon.DungeonUtils | Terraria.GameContent.Generation.Dungeon/DungeonUtils.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonUtils.cs | 33 | 2 | POTSTYLE_NORMAL_4 | int | `public const int POTSTYLE_NORMAL_4 = 3;` | `public const int POTSTYLE_NORMAL_4 = 3;` |
| 1983 | field | Terraria.GameContent.Generation.Dungeon.DungeonUtils | Terraria.GameContent.Generation.Dungeon/DungeonUtils.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonUtils.cs | 35 | 2 | POTSTYLE_SKULL_1 | int | `public const int POTSTYLE_SKULL_1 = 10;` | `public const int POTSTYLE_SKULL_1 = 10;` |
| 1984 | field | Terraria.GameContent.Generation.Dungeon.DungeonUtils | Terraria.GameContent.Generation.Dungeon/DungeonUtils.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonUtils.cs | 37 | 2 | POTSTYLE_SKULL_2 | int | `public const int POTSTYLE_SKULL_2 = 11;` | `public const int POTSTYLE_SKULL_2 = 11;` |
| 1985 | field | Terraria.GameContent.Generation.Dungeon.DungeonUtils | Terraria.GameContent.Generation.Dungeon/DungeonUtils.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonUtils.cs | 39 | 2 | POTSTYLE_SKULL_3 | int | `public const int POTSTYLE_SKULL_3 = 12;` | `public const int POTSTYLE_SKULL_3 = 12;` |
| 1986 | field | Terraria.GameContent.Generation.Dungeon.DungeonUtils | Terraria.GameContent.Generation.Dungeon/DungeonUtils.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonUtils.cs | 41 | 2 | CHANDELIERSTYLE_BLUEBRICK | int | `public const int CHANDELIERSTYLE_BLUEBRICK = 27;` | `public const int CHANDELIERSTYLE_BLUEBRICK = 27;` |
| 1987 | field | Terraria.GameContent.Generation.Dungeon.DungeonUtils | Terraria.GameContent.Generation.Dungeon/DungeonUtils.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonUtils.cs | 43 | 2 | CHANDELIERSTYLE_GREENBRICK | int | `public const int CHANDELIERSTYLE_GREENBRICK = 28;` | `public const int CHANDELIERSTYLE_GREENBRICK = 28;` |
| 1988 | field | Terraria.GameContent.Generation.Dungeon.DungeonUtils | Terraria.GameContent.Generation.Dungeon/DungeonUtils.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonUtils.cs | 45 | 2 | CHANDELIERSTYLE_PINKBRICK | int | `public const int CHANDELIERSTYLE_PINKBRICK = 29;` | `public const int CHANDELIERSTYLE_PINKBRICK = 29;` |
| 1989 | field | Terraria.GameContent.Generation.Dungeon.DungeonUtils | Terraria.GameContent.Generation.Dungeon/DungeonUtils.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonUtils.cs | 47 | 2 | PLATFORMSTYLE_BLUEBRICK | int | `public const int PLATFORMSTYLE_BLUEBRICK = 6;` | `public const int PLATFORMSTYLE_BLUEBRICK = 6;` |
| 1990 | field | Terraria.GameContent.Generation.Dungeon.DungeonUtils | Terraria.GameContent.Generation.Dungeon/DungeonUtils.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonUtils.cs | 49 | 2 | PLATFORMSTYLE_GREENBRICK | int | `public const int PLATFORMSTYLE_GREENBRICK = 8;` | `public const int PLATFORMSTYLE_GREENBRICK = 8;` |
| 1991 | field | Terraria.GameContent.Generation.Dungeon.DungeonUtils | Terraria.GameContent.Generation.Dungeon/DungeonUtils.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonUtils.cs | 51 | 2 | PLATFORMSTYLE_PINKBRICK | int | `public const int PLATFORMSTYLE_PINKBRICK = 7;` | `public const int PLATFORMSTYLE_PINKBRICK = 7;` |
| 1992 | field | Terraria.GameContent.Generation.Dungeon.DungeonUtils | Terraria.GameContent.Generation.Dungeon/DungeonUtils.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonUtils.cs | 53 | 2 | PLATFORMSTYLE_METALSHELF | int | `public const int PLATFORMSTYLE_METALSHELF = 9;` | `public const int PLATFORMSTYLE_METALSHELF = 9;` |
| 1993 | field | Terraria.GameContent.Generation.Dungeon.DungeonUtils | Terraria.GameContent.Generation.Dungeon/DungeonUtils.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonUtils.cs | 55 | 2 | PLATFORMSTYLE_BRASSSHELF | int | `public const int PLATFORMSTYLE_BRASSSHELF = 10;` | `public const int PLATFORMSTYLE_BRASSSHELF = 10;` |
| 1994 | field | Terraria.GameContent.Generation.Dungeon.DungeonUtils | Terraria.GameContent.Generation.Dungeon/DungeonUtils.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonUtils.cs | 57 | 2 | PLATFORMSTYLE_WOODSHELF | int | `public const int PLATFORMSTYLE_WOODSHELF = 11;` | `public const int PLATFORMSTYLE_WOODSHELF = 11;` |
| 1995 | field | Terraria.GameContent.Generation.Dungeon.DungeonUtils | Terraria.GameContent.Generation.Dungeon/DungeonUtils.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonUtils.cs | 59 | 2 | PLATFORMSTYLE_DUNGEONSHELF | int | `public const int PLATFORMSTYLE_DUNGEONSHELF = 12;` | `public const int PLATFORMSTYLE_DUNGEONSHELF = 12;` |

#### 属性（0）

无该类型成员记录。


### 4.28 细分子系统：`SharedDungeonStyleBannerAndTrapConstants`

- 原报告章节：`4.9.185`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`03` / `世界生成与地牢`

- 上一级基线细分子系统：`SharedDungeonGenerationQueries`
- 上一级 peer 细分子系统：`SharedDungeonStyleConstantQueries`
- 细分职责：地牢旗帜样式和陷阱类型常量查询。
- 边界角色：`query/value object`；最小 seam：dungeon style banner trap constants port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：7；属性：0；合计：7。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（7）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1996 | field | Terraria.GameContent.Generation.Dungeon.DungeonUtils | Terraria.GameContent.Generation.Dungeon/DungeonUtils.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonUtils.cs | 61 | 2 | BANNERSTYLE_BRICK_MARCHINGBONES | int | `public const int BANNERSTYLE_BRICK_MARCHINGBONES = 10;` | `public const int BANNERSTYLE_BRICK_MARCHINGBONES = 10;` |
| 1997 | field | Terraria.GameContent.Generation.Dungeon.DungeonUtils | Terraria.GameContent.Generation.Dungeon/DungeonUtils.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonUtils.cs | 63 | 2 | BANNERSTYLE_BRICK_NECROMANTICSIGN | int | `public const int BANNERSTYLE_BRICK_NECROMANTICSIGN = 11;` | `public const int BANNERSTYLE_BRICK_NECROMANTICSIGN = 11;` |
| 1998 | field | Terraria.GameContent.Generation.Dungeon.DungeonUtils | Terraria.GameContent.Generation.Dungeon/DungeonUtils.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonUtils.cs | 65 | 2 | BANNERSTYLE_SLAB_RUGGEDCOMPANY | int | `public const int BANNERSTYLE_SLAB_RUGGEDCOMPANY = 12;` | `public const int BANNERSTYLE_SLAB_RUGGEDCOMPANY = 12;` |
| 1999 | field | Terraria.GameContent.Generation.Dungeon.DungeonUtils | Terraria.GameContent.Generation.Dungeon/DungeonUtils.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonUtils.cs | 67 | 2 | BANNERSTYLE_SLAB_RAGGEDBROTHERHOOD | int | `public const int BANNERSTYLE_SLAB_RAGGEDBROTHERHOOD = 13;` | `public const int BANNERSTYLE_SLAB_RAGGEDBROTHERHOOD = 13;` |
| 2000 | field | Terraria.GameContent.Generation.Dungeon.DungeonUtils | Terraria.GameContent.Generation.Dungeon/DungeonUtils.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonUtils.cs | 69 | 2 | BANNERSTYLE_TILES_MOLTENLEGION | int | `public const int BANNERSTYLE_TILES_MOLTENLEGION = 14;` | `public const int BANNERSTYLE_TILES_MOLTENLEGION = 14;` |
| 2001 | field | Terraria.GameContent.Generation.Dungeon.DungeonUtils | Terraria.GameContent.Generation.Dungeon/DungeonUtils.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonUtils.cs | 71 | 2 | BANNERSTYLE_TILES_DIABOLICSIGIL | int | `public const int BANNERSTYLE_TILES_DIABOLICSIGIL = 15;` | `public const int BANNERSTYLE_TILES_DIABOLICSIGIL = 15;` |
| 2002 | field | Terraria.GameContent.Generation.Dungeon.DungeonUtils | Terraria.GameContent.Generation.Dungeon/DungeonUtils.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonUtils.cs | 73 | 2 | TRAPTYPE_DART | int | `public const int TRAPTYPE_DART = 0;` | `public const int TRAPTYPE_DART = 0;` |

#### 属性（0）

无该类型成员记录。


### 4.29 细分子系统：`SharedDungeonTrapPlacementState`

- 原报告章节：`4.9.201`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`03` / `世界生成与地牢`

- 上一级基线细分子系统：`SharedWorldGenerationBiomeSupport`
- 上一级 peer 细分子系统：`SharedDungeonControlAndTrapState`
- 细分职责：死亡宝箱陷阱点、陷阱数量和放置尝试状态。
- 边界角色：`definition/query`；最小 seam：dungeon trap placement port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：5；字段：22；属性：0；合计：22。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（22）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1404 | field | Terraria.GameContent.Biomes.DeadMansChestBiome.DartTrapPlacementAttempt | Terraria.GameContent.Biomes/DeadMansChestBiome.cs | D:\TRbackup\Version4\Terraria.GameContent.Biomes\DeadMansChestBiome.cs | 14 | 3 | directionX | int | `public int directionX;` | `public int directionX;` |
| 1405 | field | Terraria.GameContent.Biomes.DeadMansChestBiome.DartTrapPlacementAttempt | Terraria.GameContent.Biomes/DeadMansChestBiome.cs | D:\TRbackup\Version4\Terraria.GameContent.Biomes\DeadMansChestBiome.cs | 16 | 3 | xPush | int | `public int xPush;` | `public int xPush;` |
| 1406 | field | Terraria.GameContent.Biomes.DeadMansChestBiome.DartTrapPlacementAttempt | Terraria.GameContent.Biomes/DeadMansChestBiome.cs | D:\TRbackup\Version4\Terraria.GameContent.Biomes\DeadMansChestBiome.cs | 18 | 3 | x | int | `public int x;` | `public int x;` |
| 1407 | field | Terraria.GameContent.Biomes.DeadMansChestBiome.DartTrapPlacementAttempt | Terraria.GameContent.Biomes/DeadMansChestBiome.cs | D:\TRbackup\Version4\Terraria.GameContent.Biomes\DeadMansChestBiome.cs | 20 | 3 | y | int | `public int y;` | `public int y;` |
| 1408 | field | Terraria.GameContent.Biomes.DeadMansChestBiome.DartTrapPlacementAttempt | Terraria.GameContent.Biomes/DeadMansChestBiome.cs | D:\TRbackup\Version4\Terraria.GameContent.Biomes\DeadMansChestBiome.cs | 22 | 3 | position | Point | `public Point position;` | `public Point position;` |
| 1409 | field | Terraria.GameContent.Biomes.DeadMansChestBiome.DartTrapPlacementAttempt | Terraria.GameContent.Biomes/DeadMansChestBiome.cs | D:\TRbackup\Version4\Terraria.GameContent.Biomes\DeadMansChestBiome.cs | 24 | 3 | t | Terraria.Tile | `public Tile t;` | `public Tile t;` |
| 1410 | field | Terraria.GameContent.Biomes.DeadMansChestBiome.BoulderPlacementAttempt | Terraria.GameContent.Biomes/DeadMansChestBiome.cs | D:\TRbackup\Version4\Terraria.GameContent.Biomes\DeadMansChestBiome.cs | 39 | 3 | position | Point | `public Point position;` | `public Point position;` |
| 1411 | field | Terraria.GameContent.Biomes.DeadMansChestBiome.BoulderPlacementAttempt | Terraria.GameContent.Biomes/DeadMansChestBiome.cs | D:\TRbackup\Version4\Terraria.GameContent.Biomes\DeadMansChestBiome.cs | 41 | 3 | yPush | int | `public int yPush;` | `public int yPush;` |
| 1412 | field | Terraria.GameContent.Biomes.DeadMansChestBiome.BoulderPlacementAttempt | Terraria.GameContent.Biomes/DeadMansChestBiome.cs | D:\TRbackup\Version4\Terraria.GameContent.Biomes\DeadMansChestBiome.cs | 43 | 3 | requiredHeight | int | `public int requiredHeight;` | `public int requiredHeight;` |
| 1413 | field | Terraria.GameContent.Biomes.DeadMansChestBiome.BoulderPlacementAttempt | Terraria.GameContent.Biomes/DeadMansChestBiome.cs | D:\TRbackup\Version4\Terraria.GameContent.Biomes\DeadMansChestBiome.cs | 45 | 3 | bestType | int | `public int bestType;` | `public int bestType;` |
| 1414 | field | Terraria.GameContent.Biomes.DeadMansChestBiome.WirePlacementAttempt | Terraria.GameContent.Biomes/DeadMansChestBiome.cs | D:\TRbackup\Version4\Terraria.GameContent.Biomes\DeadMansChestBiome.cs | 58 | 3 | position | Point | `public Point position;` | `public Point position;` |
| 1415 | field | Terraria.GameContent.Biomes.DeadMansChestBiome.WirePlacementAttempt | Terraria.GameContent.Biomes/DeadMansChestBiome.cs | D:\TRbackup\Version4\Terraria.GameContent.Biomes\DeadMansChestBiome.cs | 60 | 3 | dirX | int | `public int dirX;` | `public int dirX;` |
| 1416 | field | Terraria.GameContent.Biomes.DeadMansChestBiome.WirePlacementAttempt | Terraria.GameContent.Biomes/DeadMansChestBiome.cs | D:\TRbackup\Version4\Terraria.GameContent.Biomes\DeadMansChestBiome.cs | 62 | 3 | dirY | int | `public int dirY;` | `public int dirY;` |
| 1417 | field | Terraria.GameContent.Biomes.DeadMansChestBiome.WirePlacementAttempt | Terraria.GameContent.Biomes/DeadMansChestBiome.cs | D:\TRbackup\Version4\Terraria.GameContent.Biomes\DeadMansChestBiome.cs | 64 | 3 | steps | int | `public int steps;` | `public int steps;` |
| 1418 | field | Terraria.GameContent.Biomes.DeadMansChestBiome.ExplosivePlacementAttempt | Terraria.GameContent.Biomes/DeadMansChestBiome.cs | D:\TRbackup\Version4\Terraria.GameContent.Biomes\DeadMansChestBiome.cs | 77 | 3 | position | Point | `public Point position;` | `public Point position;` |
| 1419 | field | Terraria.GameContent.Biomes.DeadMansChestBiome | Terraria.GameContent.Biomes/DeadMansChestBiome.cs | D:\TRbackup\Version4\Terraria.GameContent.Biomes\DeadMansChestBiome.cs | 85 | 2 | _dartTrapPlacementSpots | System.Collections.Generic.List<Terraria.GameContent.Biomes.DeadMansChestBiome.DartTrapPlacementAttempt> | `private List<DartTrapPlacementAttempt> _dartTrapPlacementSpots = new List<DartTrapPlacementAttempt>();` | `private List<DartTrapPlacementAttempt> _dartTrapPlacementSpots = new List<DartTrapPlacementAttempt>();` |
| 1420 | field | Terraria.GameContent.Biomes.DeadMansChestBiome | Terraria.GameContent.Biomes/DeadMansChestBiome.cs | D:\TRbackup\Version4\Terraria.GameContent.Biomes\DeadMansChestBiome.cs | 87 | 2 | _wirePlacementSpots | System.Collections.Generic.List<Terraria.GameContent.Biomes.DeadMansChestBiome.WirePlacementAttempt> | `private List<WirePlacementAttempt> _wirePlacementSpots = new List<WirePlacementAttempt>();` | `private List<WirePlacementAttempt> _wirePlacementSpots = new List<WirePlacementAttempt>();` |
| 1421 | field | Terraria.GameContent.Biomes.DeadMansChestBiome | Terraria.GameContent.Biomes/DeadMansChestBiome.cs | D:\TRbackup\Version4\Terraria.GameContent.Biomes\DeadMansChestBiome.cs | 89 | 2 | _boulderPlacementSpots | System.Collections.Generic.List<Terraria.GameContent.Biomes.DeadMansChestBiome.BoulderPlacementAttempt> | `private List<BoulderPlacementAttempt> _boulderPlacementSpots = new List<BoulderPlacementAttempt>();` | `private List<BoulderPlacementAttempt> _boulderPlacementSpots = new List<BoulderPlacementAttempt>();` |
| 1422 | field | Terraria.GameContent.Biomes.DeadMansChestBiome | Terraria.GameContent.Biomes/DeadMansChestBiome.cs | D:\TRbackup\Version4\Terraria.GameContent.Biomes\DeadMansChestBiome.cs | 91 | 2 | _explosivePlacementAttempt | System.Collections.Generic.List<Terraria.GameContent.Biomes.DeadMansChestBiome.ExplosivePlacementAttempt> | `private List<ExplosivePlacementAttempt> _explosivePlacementAttempt = new List<ExplosivePlacementAttempt>();` | `private List<ExplosivePlacementAttempt> _explosivePlacementAttempt = new List<ExplosivePlacementAttempt>();` |
| 1423 | field | Terraria.GameContent.Biomes.DeadMansChestBiome | Terraria.GameContent.Biomes/DeadMansChestBiome.cs | D:\TRbackup\Version4\Terraria.GameContent.Biomes\DeadMansChestBiome.cs | 93 | 2 | _numberOfDartTraps | Terraria.Utilities.IntRange | `[JsonProperty("NumberOfDartTraps")] private IntRange _numberOfDartTraps = new IntRange(3, 6);` | `[JsonProperty("NumberOfDartTraps")] private IntRange _numberOfDartTraps = new IntRange(3, 6);` |
| 1424 | field | Terraria.GameContent.Biomes.DeadMansChestBiome | Terraria.GameContent.Biomes/DeadMansChestBiome.cs | D:\TRbackup\Version4\Terraria.GameContent.Biomes\DeadMansChestBiome.cs | 96 | 2 | _numberOfBoulderTraps | Terraria.Utilities.IntRange | `[JsonProperty("NumberOfBoulderTraps")] private IntRange _numberOfBoulderTraps = new IntRange(2, 4);` | `[JsonProperty("NumberOfBoulderTraps")] private IntRange _numberOfBoulderTraps = new IntRange(2, 4);` |
| 1425 | field | Terraria.GameContent.Biomes.DeadMansChestBiome | Terraria.GameContent.Biomes/DeadMansChestBiome.cs | D:\TRbackup\Version4\Terraria.GameContent.Biomes\DeadMansChestBiome.cs | 99 | 2 | _numberOfStepsBetweenBoulderTraps | Terraria.Utilities.IntRange | `[JsonProperty("NumberOfStepsBetweenBoulderTraps")] private IntRange _numberOfStepsBetweenBoulderTraps = new IntRange(2, 4);` | `[JsonProperty("NumberOfStepsBetweenBoulderTraps")] private IntRange _numberOfStepsBetweenBoulderTraps = new IntRange(2, 4);` |

#### 属性（0）

无该类型成员记录。


### 4.30 细分子系统：`SharedDungeonControlLineGeometryState`

- 原报告章节：`4.9.202`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`03` / `世界生成与地牢`

- 上一级基线细分子系统：`SharedWorldGenerationBiomeSupport`
- 上一级 peer 细分子系统：`SharedDungeonControlAndTrapState`
- 细分职责：地牢控制线节点、切线、半径、方向和样式几何。
- 边界角色：`definition/query`；最小 seam：dungeon control line geometry port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：20；属性：1；合计：21。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（20）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1430 | field | Terraria.GameContent.Biomes.DungeonControlLine | Terraria.GameContent.Biomes/DungeonControlLine.cs | D:\TRbackup\Version4\Terraria.GameContent.Biomes\DungeonControlLine.cs | 11 | 2 | Index | int | `public int Index;` | `public int Index;` |
| 1431 | field | Terraria.GameContent.Biomes.DungeonControlLine | Terraria.GameContent.Biomes/DungeonControlLine.cs | D:\TRbackup\Version4\Terraria.GameContent.Biomes\DungeonControlLine.cs | 13 | 2 | Next | Terraria.GameContent.Biomes.DungeonControlLine | `public DungeonControlLine Next;` | `public DungeonControlLine Next;` |
| 1432 | field | Terraria.GameContent.Biomes.DungeonControlLine | Terraria.GameContent.Biomes/DungeonControlLine.cs | D:\TRbackup\Version4\Terraria.GameContent.Biomes\DungeonControlLine.cs | 15 | 2 | Prev | Terraria.GameContent.Biomes.DungeonControlLine | `public DungeonControlLine Prev;` | `public DungeonControlLine Prev;` |
| 1433 | field | Terraria.GameContent.Biomes.DungeonControlLine | Terraria.GameContent.Biomes/DungeonControlLine.cs | D:\TRbackup\Version4\Terraria.GameContent.Biomes\DungeonControlLine.cs | 17 | 2 | Start | Vector2D | `public Vector2D Start;` | `public Vector2D Start;` |
| 1434 | field | Terraria.GameContent.Biomes.DungeonControlLine | Terraria.GameContent.Biomes/DungeonControlLine.cs | D:\TRbackup\Version4\Terraria.GameContent.Biomes\DungeonControlLine.cs | 19 | 2 | End | Vector2D | `public Vector2D End;` | `public Vector2D End;` |
| 1435 | field | Terraria.GameContent.Biomes.DungeonControlLine | Terraria.GameContent.Biomes/DungeonControlLine.cs | D:\TRbackup\Version4\Terraria.GameContent.Biomes\DungeonControlLine.cs | 21 | 2 | StartTangent | Vector2D | `public Vector2D StartTangent;` | `public Vector2D StartTangent;` |
| 1436 | field | Terraria.GameContent.Biomes.DungeonControlLine | Terraria.GameContent.Biomes/DungeonControlLine.cs | D:\TRbackup\Version4\Terraria.GameContent.Biomes\DungeonControlLine.cs | 23 | 2 | EndTangent | Vector2D | `public Vector2D EndTangent;` | `public Vector2D EndTangent;` |
| 1437 | field | Terraria.GameContent.Biomes.DungeonControlLine | Terraria.GameContent.Biomes/DungeonControlLine.cs | D:\TRbackup\Version4\Terraria.GameContent.Biomes\DungeonControlLine.cs | 25 | 2 | StartNormal | Vector2D | `public Vector2D StartNormal;` | `public Vector2D StartNormal;` |
| 1438 | field | Terraria.GameContent.Biomes.DungeonControlLine | Terraria.GameContent.Biomes/DungeonControlLine.cs | D:\TRbackup\Version4\Terraria.GameContent.Biomes\DungeonControlLine.cs | 27 | 2 | EndNormal | Vector2D | `public Vector2D EndNormal;` | `public Vector2D EndNormal;` |
| 1439 | field | Terraria.GameContent.Biomes.DungeonControlLine | Terraria.GameContent.Biomes/DungeonControlLine.cs | D:\TRbackup\Version4\Terraria.GameContent.Biomes\DungeonControlLine.cs | 29 | 2 | CrossTangent | double | `public double CrossTangent;` | `public double CrossTangent;` |
| 1440 | field | Terraria.GameContent.Biomes.DungeonControlLine | Terraria.GameContent.Biomes/DungeonControlLine.cs | D:\TRbackup\Version4\Terraria.GameContent.Biomes\DungeonControlLine.cs | 31 | 2 | StartRadius | double | `public double StartRadius;` | `public double StartRadius;` |
| 1441 | field | Terraria.GameContent.Biomes.DungeonControlLine | Terraria.GameContent.Biomes/DungeonControlLine.cs | D:\TRbackup\Version4\Terraria.GameContent.Biomes\DungeonControlLine.cs | 33 | 2 | EndRadius | double | `public double EndRadius;` | `public double EndRadius;` |
| 1442 | field | Terraria.GameContent.Biomes.DungeonControlLine | Terraria.GameContent.Biomes/DungeonControlLine.cs | D:\TRbackup\Version4\Terraria.GameContent.Biomes\DungeonControlLine.cs | 35 | 2 | NormalizedDistanceSafeFromDither | double | `public static double NormalizedDistanceSafeFromDither;` | `public static double NormalizedDistanceSafeFromDither;` |
| 1443 | field | Terraria.GameContent.Biomes.DungeonControlLine | Terraria.GameContent.Biomes/DungeonControlLine.cs | D:\TRbackup\Version4\Terraria.GameContent.Biomes\DungeonControlLine.cs | 37 | 2 | StyleTransitionDitherWidth | double | `private const double StyleTransitionDitherWidth = 0.5;` | `private const double StyleTransitionDitherWidth = 0.5;` |
| 1444 | field | Terraria.GameContent.Biomes.DungeonControlLine | Terraria.GameContent.Biomes/DungeonControlLine.cs | D:\TRbackup\Version4\Terraria.GameContent.Biomes\DungeonControlLine.cs | 39 | 2 | BorderWidth | int | `private const int BorderWidth = 4;` | `private const int BorderWidth = 4;` |
| 1445 | field | Terraria.GameContent.Biomes.DungeonControlLine | Terraria.GameContent.Biomes/DungeonControlLine.cs | D:\TRbackup\Version4\Terraria.GameContent.Biomes\DungeonControlLine.cs | 41 | 2 | NormalizedLineDirection | Vector2D | `public Vector2D NormalizedLineDirection;` | `public Vector2D NormalizedLineDirection;` |
| 1446 | field | Terraria.GameContent.Biomes.DungeonControlLine | Terraria.GameContent.Biomes/DungeonControlLine.cs | D:\TRbackup\Version4\Terraria.GameContent.Biomes\DungeonControlLine.cs | 43 | 2 | LineLength | double | `public double LineLength;` | `public double LineLength;` |
| 1447 | field | Terraria.GameContent.Biomes.DungeonControlLine | Terraria.GameContent.Biomes/DungeonControlLine.cs | D:\TRbackup\Version4\Terraria.GameContent.Biomes\DungeonControlLine.cs | 45 | 2 | Style | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyleData | `public DungeonGenerationStyleData Style;` | `public DungeonGenerationStyleData Style;` |
| 1448 | field | Terraria.GameContent.Biomes.DungeonControlLine | Terraria.GameContent.Biomes/DungeonControlLine.cs | D:\TRbackup\Version4\Terraria.GameContent.Biomes\DungeonControlLine.cs | 47 | 2 | ProgressionStage | int | `public int ProgressionStage;` | `public int ProgressionStage;` |
| 1449 | field | Terraria.GameContent.Biomes.DungeonControlLine | Terraria.GameContent.Biomes/DungeonControlLine.cs | D:\TRbackup\Version4\Terraria.GameContent.Biomes\DungeonControlLine.cs | 49 | 2 | CurveLine | bool | `public bool CurveLine;` | `public bool CurveLine;` |

#### 属性（1）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3746 | property | Terraria.GameContent.Biomes.DungeonControlLine | Terraria.GameContent.Biomes/DungeonControlLine.cs | D:\TRbackup\Version4\Terraria.GameContent.Biomes\DungeonControlLine.cs | 51 | 2 | Center | Vector2D | `public Vector2D Center => (End + Start) / 2.0;` | `public Vector2D Center => (End + Start) / 2.0;` |


### 4.31 细分子系统：`SharedDungeonRoomShapeGeometryState`

- 原报告章节：`4.9.210`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`03` / `世界生成与地牢`

- 上一级基线细分子系统：`SharedDungeonRoomDefinitions`
- 上一级 peer 细分子系统：`SharedDungeonRoomGeometryState`
- 细分职责：地牢房间内部/外部形状数据和形状尺寸变化。
- 边界角色：`definition/catalog`；最小 seam：dungeon room shape geometry port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：5；声明类型数：5；字段：13；属性：0；合计：13。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（13）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1755 | field | Terraria.GameContent.Generation.Dungeon.Rooms.BiomeDungeonRoom | Terraria.GameContent.Generation.Dungeon.Rooms/BiomeDungeonRoom.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms\BiomeDungeonRoom.cs | 18 | 2 | _innerShapeData | Terraria.WorldBuilding.ShapeData | `protected ShapeData _innerShapeData;` | `protected ShapeData _innerShapeData;` |
| 1756 | field | Terraria.GameContent.Generation.Dungeon.Rooms.BiomeDungeonRoom | Terraria.GameContent.Generation.Dungeon.Rooms/BiomeDungeonRoom.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms\BiomeDungeonRoom.cs | 20 | 2 | _outerShapeData | Terraria.WorldBuilding.ShapeData | `protected ShapeData _outerShapeData;` | `protected ShapeData _outerShapeData;` |
| 1793 | field | Terraria.GameContent.Generation.Dungeon.Rooms.GenShapeDungeonRoom | Terraria.GameContent.Generation.Dungeon.Rooms/GenShapeDungeonRoom.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms\GenShapeDungeonRoom.cs | 12 | 2 | _innerShapeData | Terraria.WorldBuilding.ShapeData | `private ShapeData _innerShapeData = new ShapeData();` | `private ShapeData _innerShapeData = new ShapeData();` |
| 1794 | field | Terraria.GameContent.Generation.Dungeon.Rooms.GenShapeDungeonRoom | Terraria.GameContent.Generation.Dungeon.Rooms/GenShapeDungeonRoom.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms\GenShapeDungeonRoom.cs | 14 | 2 | _outerShapeData | Terraria.WorldBuilding.ShapeData | `private ShapeData _outerShapeData = new ShapeData();` | `private ShapeData _outerShapeData = new ShapeData();` |
| 1799 | field | Terraria.GameContent.Generation.Dungeon.Rooms.LegacyDungeonRoom | Terraria.GameContent.Generation.Dungeon.Rooms/LegacyDungeonRoom.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms\LegacyDungeonRoom.cs | 11 | 2 | _innerShapeData | Terraria.WorldBuilding.ShapeData | `private ShapeData _innerShapeData = new ShapeData();` | `private ShapeData _innerShapeData = new ShapeData();` |
| 1800 | field | Terraria.GameContent.Generation.Dungeon.Rooms.LegacyDungeonRoom | Terraria.GameContent.Generation.Dungeon.Rooms/LegacyDungeonRoom.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms\LegacyDungeonRoom.cs | 13 | 2 | _outerShapeData | Terraria.WorldBuilding.ShapeData | `private ShapeData _outerShapeData = new ShapeData();` | `private ShapeData _outerShapeData = new ShapeData();` |
| 1805 | field | Terraria.GameContent.Generation.Dungeon.Rooms.LivingTreeDungeonRoom | Terraria.GameContent.Generation.Dungeon.Rooms/LivingTreeDungeonRoom.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms\LivingTreeDungeonRoom.cs | 10 | 2 | _innerShapeData | Terraria.WorldBuilding.ShapeData | `private ShapeData _innerShapeData = new ShapeData();` | `private ShapeData _innerShapeData = new ShapeData();` |
| 1806 | field | Terraria.GameContent.Generation.Dungeon.Rooms.LivingTreeDungeonRoom | Terraria.GameContent.Generation.Dungeon.Rooms/LivingTreeDungeonRoom.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms\LivingTreeDungeonRoom.cs | 12 | 2 | _outerShapeData | Terraria.WorldBuilding.ShapeData | `private ShapeData _outerShapeData = new ShapeData();` | `private ShapeData _outerShapeData = new ShapeData();` |
| 1814 | field | Terraria.GameContent.Generation.Dungeon.Rooms.WormlikeDungeonRoom | Terraria.GameContent.Generation.Dungeon.Rooms/WormlikeDungeonRoom.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms\WormlikeDungeonRoom.cs | 13 | 2 | _innerShapeData | Terraria.WorldBuilding.ShapeData | `private ShapeData _innerShapeData = new ShapeData();` | `private ShapeData _innerShapeData = new ShapeData();` |
| 1815 | field | Terraria.GameContent.Generation.Dungeon.Rooms.WormlikeDungeonRoom | Terraria.GameContent.Generation.Dungeon.Rooms/WormlikeDungeonRoom.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms\WormlikeDungeonRoom.cs | 15 | 2 | _outerShapeData | Terraria.WorldBuilding.ShapeData | `private ShapeData _outerShapeData = new ShapeData();` | `private ShapeData _outerShapeData = new ShapeData();` |
| 1816 | field | Terraria.GameContent.Generation.Dungeon.Rooms.WormlikeDungeonRoom | Terraria.GameContent.Generation.Dungeon.Rooms/WormlikeDungeonRoom.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms\WormlikeDungeonRoom.cs | 17 | 2 | InnerBoundsSizeMin | int | `public int InnerBoundsSizeMin;` | `public int InnerBoundsSizeMin;` |
| 1817 | field | Terraria.GameContent.Generation.Dungeon.Rooms.WormlikeDungeonRoom | Terraria.GameContent.Generation.Dungeon.Rooms/WormlikeDungeonRoom.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms\WormlikeDungeonRoom.cs | 19 | 2 | InnerBoundsSizeMax | int | `public int InnerBoundsSizeMax;` | `public int InnerBoundsSizeMax;` |
| 1818 | field | Terraria.GameContent.Generation.Dungeon.Rooms.WormlikeDungeonRoom | Terraria.GameContent.Generation.Dungeon.Rooms/WormlikeDungeonRoom.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms\WormlikeDungeonRoom.cs | 21 | 2 | Positions | Vector2[] | `public Vector2[] Positions;` | `public Vector2[] Positions;` |

#### 属性（0）

无该类型成员记录。


### 4.32 细分子系统：`SharedDungeonRoomPlacementGeometryState`

- 原报告章节：`4.9.211`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`03` / `世界生成与地牢`

- 上一级基线细分子系统：`SharedDungeonRoomDefinitions`
- 上一级 peer 细分子系统：`SharedDungeonRoomGeometryState`
- 细分职责：地牢房间位置、边界尺寸、墙深和强度/端点布局。
- 边界角色：`definition/catalog`；最小 seam：dungeon room placement geometry port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：5；声明类型数：5；字段：16；属性：0；合计：16。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（16）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1757 | field | Terraria.GameContent.Generation.Dungeon.Rooms.BiomeRuggedDungeonRoom | Terraria.GameContent.Generation.Dungeon.Rooms/BiomeRuggedDungeonRoom.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms\BiomeRuggedDungeonRoom.cs | 11 | 2 | Position | Vector2 | `public Vector2 Position;` | `public Vector2 Position;` |
| 1758 | field | Terraria.GameContent.Generation.Dungeon.Rooms.BiomeRuggedDungeonRoom | Terraria.GameContent.Generation.Dungeon.Rooms/BiomeRuggedDungeonRoom.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms\BiomeRuggedDungeonRoom.cs | 13 | 2 | RoomInnerSize | int | `public int RoomInnerSize;` | `public int RoomInnerSize;` |
| 1759 | field | Terraria.GameContent.Generation.Dungeon.Rooms.BiomeRuggedDungeonRoom | Terraria.GameContent.Generation.Dungeon.Rooms/BiomeRuggedDungeonRoom.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms\BiomeRuggedDungeonRoom.cs | 15 | 2 | RoomOuterSize | int | `public int RoomOuterSize;` | `public int RoomOuterSize;` |
| 1760 | field | Terraria.GameContent.Generation.Dungeon.Rooms.BiomeRuggedDungeonRoom | Terraria.GameContent.Generation.Dungeon.Rooms/BiomeRuggedDungeonRoom.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms\BiomeRuggedDungeonRoom.cs | 17 | 2 | WallDepth | int | `public int WallDepth;` | `public int WallDepth;` |
| 1761 | field | Terraria.GameContent.Generation.Dungeon.Rooms.BiomeSquareDungeonRoom | Terraria.GameContent.Generation.Dungeon.Rooms/BiomeSquareDungeonRoom.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms\BiomeSquareDungeonRoom.cs | 9 | 2 | Position | Vector2 | `public Vector2 Position;` | `public Vector2 Position;` |
| 1762 | field | Terraria.GameContent.Generation.Dungeon.Rooms.BiomeSquareDungeonRoom | Terraria.GameContent.Generation.Dungeon.Rooms/BiomeSquareDungeonRoom.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms\BiomeSquareDungeonRoom.cs | 11 | 2 | RoomInnerSize | int | `public int RoomInnerSize;` | `public int RoomInnerSize;` |
| 1763 | field | Terraria.GameContent.Generation.Dungeon.Rooms.BiomeSquareDungeonRoom | Terraria.GameContent.Generation.Dungeon.Rooms/BiomeSquareDungeonRoom.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms\BiomeSquareDungeonRoom.cs | 13 | 2 | RoomOuterSize | int | `public int RoomOuterSize;` | `public int RoomOuterSize;` |
| 1764 | field | Terraria.GameContent.Generation.Dungeon.Rooms.BiomeSquareDungeonRoom | Terraria.GameContent.Generation.Dungeon.Rooms/BiomeSquareDungeonRoom.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms\BiomeSquareDungeonRoom.cs | 15 | 2 | WallDepth | int | `public int WallDepth;` | `public int WallDepth;` |
| 1770 | field | Terraria.GameContent.Generation.Dungeon.Rooms.BiomeStructuredDungeonRoom | Terraria.GameContent.Generation.Dungeon.Rooms/BiomeStructuredDungeonRoom.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms\BiomeStructuredDungeonRoom.cs | 21 | 2 | Position | Vector2 | `public Vector2 Position;` | `public Vector2 Position;` |
| 1771 | field | Terraria.GameContent.Generation.Dungeon.Rooms.BiomeStructuredDungeonRoom | Terraria.GameContent.Generation.Dungeon.Rooms/BiomeStructuredDungeonRoom.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms\BiomeStructuredDungeonRoom.cs | 23 | 2 | RoomInnerSize | int | `public int RoomInnerSize;` | `public int RoomInnerSize;` |
| 1772 | field | Terraria.GameContent.Generation.Dungeon.Rooms.BiomeStructuredDungeonRoom | Terraria.GameContent.Generation.Dungeon.Rooms/BiomeStructuredDungeonRoom.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms\BiomeStructuredDungeonRoom.cs | 25 | 2 | RoomOuterSize | int | `public int RoomOuterSize;` | `public int RoomOuterSize;` |
| 1773 | field | Terraria.GameContent.Generation.Dungeon.Rooms.BiomeStructuredDungeonRoom | Terraria.GameContent.Generation.Dungeon.Rooms/BiomeStructuredDungeonRoom.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms\BiomeStructuredDungeonRoom.cs | 27 | 2 | WallDepth | int | `public int WallDepth;` | `public int WallDepth;` |
| 1801 | field | Terraria.GameContent.Generation.Dungeon.Rooms.LegacyDungeonRoom | Terraria.GameContent.Generation.Dungeon.Rooms/LegacyDungeonRoom.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms\LegacyDungeonRoom.cs | 15 | 2 | StartPosition | Vector2D | `public Vector2D StartPosition;` | `public Vector2D StartPosition;` |
| 1802 | field | Terraria.GameContent.Generation.Dungeon.Rooms.LegacyDungeonRoom | Terraria.GameContent.Generation.Dungeon.Rooms/LegacyDungeonRoom.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms\LegacyDungeonRoom.cs | 17 | 2 | EndPosition | Vector2D | `public Vector2D EndPosition;` | `public Vector2D EndPosition;` |
| 1803 | field | Terraria.GameContent.Generation.Dungeon.Rooms.LegacyDungeonRoom | Terraria.GameContent.Generation.Dungeon.Rooms/LegacyDungeonRoom.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms\LegacyDungeonRoom.cs | 19 | 2 | Strength | int | `public int Strength;` | `public int Strength;` |
| 1807 | field | Terraria.GameContent.Generation.Dungeon.Rooms.RegularDungeonRoom | Terraria.GameContent.Generation.Dungeon.Rooms/RegularDungeonRoom.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon.Rooms\RegularDungeonRoom.cs | 9 | 2 | _innerBoundsSize | int | `public int _innerBoundsSize;` | `public int _innerBoundsSize;` |

#### 属性（0）

无该类型成员记录。


### 4.33 细分子系统：`SharedDungeonStyleFurnitureCatalogState`

- 原报告章节：`4.9.212`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`03` / `世界生成与地牢`

- 上一级基线细分子系统：`SharedDungeonStyleCatalog`
- 上一级 peer 细分子系统：`SharedDungeonStyleFurnitureAndRoomState`
- 细分职责：地牢箱体、门、平台、灯具、家具和装饰物品目录。
- 边界角色：`definition/catalog`；最小 seam：dungeon furniture catalog port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：25；属性：0；合计：25。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（25）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1886 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyleData | Terraria.GameContent.Generation.Dungeon/DungeonGenerationStyleData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenerationStyleData.cs | 29 | 2 | WindowPlatformItemTypes | int[] | `public int[] WindowPlatformItemTypes;` | `public int[] WindowPlatformItemTypes;` |
| 1889 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyleData | Terraria.GameContent.Generation.Dungeon/DungeonGenerationStyleData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenerationStyleData.cs | 35 | 2 | LockedBiomeChestType | int | `public int LockedBiomeChestType;` | `public int LockedBiomeChestType;` |
| 1890 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyleData | Terraria.GameContent.Generation.Dungeon/DungeonGenerationStyleData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenerationStyleData.cs | 37 | 2 | LockedBiomeChestStyle | int | `public int LockedBiomeChestStyle;` | `public int LockedBiomeChestStyle;` |
| 1891 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyleData | Terraria.GameContent.Generation.Dungeon/DungeonGenerationStyleData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenerationStyleData.cs | 39 | 2 | BiomeChestItemType | int | `public int BiomeChestItemType;` | `public int BiomeChestItemType;` |
| 1892 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyleData | Terraria.GameContent.Generation.Dungeon/DungeonGenerationStyleData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenerationStyleData.cs | 41 | 2 | BiomeChestLootItemType | int | `public int BiomeChestLootItemType;` | `public int BiomeChestLootItemType;` |
| 1893 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyleData | Terraria.GameContent.Generation.Dungeon/DungeonGenerationStyleData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenerationStyleData.cs | 43 | 2 | ChestItemTypes | int[] | `public int[] ChestItemTypes;` | `public int[] ChestItemTypes;` |
| 1894 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyleData | Terraria.GameContent.Generation.Dungeon/DungeonGenerationStyleData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenerationStyleData.cs | 45 | 2 | DoorItemTypes | int[] | `public int[] DoorItemTypes;` | `public int[] DoorItemTypes;` |
| 1895 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyleData | Terraria.GameContent.Generation.Dungeon/DungeonGenerationStyleData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenerationStyleData.cs | 47 | 2 | PlatformItemTypes | int[] | `public int[] PlatformItemTypes;` | `public int[] PlatformItemTypes;` |
| 1896 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyleData | Terraria.GameContent.Generation.Dungeon/DungeonGenerationStyleData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenerationStyleData.cs | 49 | 2 | ChandelierItemTypes | int[] | `public int[] ChandelierItemTypes;` | `public int[] ChandelierItemTypes;` |
| 1897 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyleData | Terraria.GameContent.Generation.Dungeon/DungeonGenerationStyleData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenerationStyleData.cs | 51 | 2 | LanternItemTypes | int[] | `public int[] LanternItemTypes;` | `public int[] LanternItemTypes;` |
| 1898 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyleData | Terraria.GameContent.Generation.Dungeon/DungeonGenerationStyleData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenerationStyleData.cs | 53 | 2 | TableItemTypes | int[] | `public int[] TableItemTypes;` | `public int[] TableItemTypes;` |
| 1899 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyleData | Terraria.GameContent.Generation.Dungeon/DungeonGenerationStyleData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenerationStyleData.cs | 55 | 2 | WorkbenchItemTypes | int[] | `public int[] WorkbenchItemTypes;` | `public int[] WorkbenchItemTypes;` |
| 1900 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyleData | Terraria.GameContent.Generation.Dungeon/DungeonGenerationStyleData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenerationStyleData.cs | 57 | 2 | CandleItemTypes | int[] | `public int[] CandleItemTypes;` | `public int[] CandleItemTypes;` |
| 1901 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyleData | Terraria.GameContent.Generation.Dungeon/DungeonGenerationStyleData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenerationStyleData.cs | 59 | 2 | VaseOrStatueItemTypes | int[] | `public int[] VaseOrStatueItemTypes;` | `public int[] VaseOrStatueItemTypes;` |
| 1902 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyleData | Terraria.GameContent.Generation.Dungeon/DungeonGenerationStyleData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenerationStyleData.cs | 61 | 2 | BookcaseItemTypes | int[] | `public int[] BookcaseItemTypes;` | `public int[] BookcaseItemTypes;` |
| 1903 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyleData | Terraria.GameContent.Generation.Dungeon/DungeonGenerationStyleData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenerationStyleData.cs | 63 | 2 | ChairItemTypes | int[] | `public int[] ChairItemTypes;` | `public int[] ChairItemTypes;` |
| 1904 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyleData | Terraria.GameContent.Generation.Dungeon/DungeonGenerationStyleData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenerationStyleData.cs | 65 | 2 | BedItemTypes | int[] | `public int[] BedItemTypes;` | `public int[] BedItemTypes;` |
| 1905 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyleData | Terraria.GameContent.Generation.Dungeon/DungeonGenerationStyleData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenerationStyleData.cs | 67 | 2 | PianoItemTypes | int[] | `public int[] PianoItemTypes;` | `public int[] PianoItemTypes;` |
| 1906 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyleData | Terraria.GameContent.Generation.Dungeon/DungeonGenerationStyleData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenerationStyleData.cs | 69 | 2 | DresserItemTypes | int[] | `public int[] DresserItemTypes;` | `public int[] DresserItemTypes;` |
| 1907 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyleData | Terraria.GameContent.Generation.Dungeon/DungeonGenerationStyleData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenerationStyleData.cs | 71 | 2 | SofaItemTypes | int[] | `public int[] SofaItemTypes;` | `public int[] SofaItemTypes;` |
| 1908 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyleData | Terraria.GameContent.Generation.Dungeon/DungeonGenerationStyleData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenerationStyleData.cs | 73 | 2 | BathtubItemTypes | int[] | `public int[] BathtubItemTypes;` | `public int[] BathtubItemTypes;` |
| 1909 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyleData | Terraria.GameContent.Generation.Dungeon/DungeonGenerationStyleData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenerationStyleData.cs | 75 | 2 | LampItemTypes | int[] | `public int[] LampItemTypes;` | `public int[] LampItemTypes;` |
| 1910 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyleData | Terraria.GameContent.Generation.Dungeon/DungeonGenerationStyleData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenerationStyleData.cs | 77 | 2 | CandelabraItemTypes | int[] | `public int[] CandelabraItemTypes;` | `public int[] CandelabraItemTypes;` |
| 1911 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyleData | Terraria.GameContent.Generation.Dungeon/DungeonGenerationStyleData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenerationStyleData.cs | 79 | 2 | ClockItemTypes | int[] | `public int[] ClockItemTypes;` | `public int[] ClockItemTypes;` |
| 1912 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyleData | Terraria.GameContent.Generation.Dungeon/DungeonGenerationStyleData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenerationStyleData.cs | 81 | 2 | BannerItemTypes | int[] | `public int[] BannerItemTypes;` | `public int[] BannerItemTypes;` |

#### 属性（0）

无该类型成员记录。


### 4.34 细分子系统：`SharedDungeonStyleRoomVariantState`

- 原报告章节：`4.9.213`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`03` / `世界生成与地牢`

- 上一级基线细分子系统：`SharedDungeonStyleCatalog`
- 上一级 peer 细分子系统：`SharedDungeonStyleFurnitureAndRoomState`
- 细分职责：地牢生物群落房间类型和子样式关系。
- 边界角色：`definition/catalog`；最小 seam：dungeon room variant port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：2；属性：0；合计：2。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（2）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1914 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyleData | Terraria.GameContent.Generation.Dungeon/DungeonGenerationStyleData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenerationStyleData.cs | 85 | 2 | BiomeRoomType | Terraria.GameContent.Generation.Dungeon.Rooms.DungeonRoomType | `public DungeonRoomType BiomeRoomType;` | `public DungeonRoomType BiomeRoomType;` |
| 1915 | field | Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyleData | Terraria.GameContent.Generation.Dungeon/DungeonGenerationStyleData.cs | D:\TRbackup\Version4\Terraria.GameContent.Generation.Dungeon\DungeonGenerationStyleData.cs | 87 | 2 | SubStyles | System.Collections.Generic.List<Terraria.GameContent.Generation.Dungeon.DungeonGenerationStyleData> | `public List<DungeonGenerationStyleData> SubStyles;` | `public List<DungeonGenerationStyleData> SubStyles;` |

#### 属性（0）

无该类型成员记录。


### 4.35 细分子系统：`SharedSceneBiomeZoneDefinitionState`

- 原报告章节：`4.9.216`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`03` / `世界生成与地牢`

- 上一级基线细分子系统：`SharedSceneMetricsSnapshot`
- 上一级 peer 细分子系统：`SharedSceneBiomeAndEventDefinitionState`
- 细分职责：场景腐化、猩红、神圣、地形和生物群落区域定义。
- 边界角色：`query/input`；最小 seam：scene biome zone definition port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：17；属性：0；合计：17。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（17）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3442 | field | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 38 | 2 | ZoneCorrupt | bool | `public bool ZoneCorrupt;` | `public bool ZoneCorrupt;` |
| 3443 | field | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 40 | 2 | ZoneCrimson | bool | `public bool ZoneCrimson;` | `public bool ZoneCrimson;` |
| 3444 | field | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 42 | 2 | ZoneHallow | bool | `public bool ZoneHallow;` | `public bool ZoneHallow;` |
| 3445 | field | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 44 | 2 | ZoneJungle | bool | `public bool ZoneJungle;` | `public bool ZoneJungle;` |
| 3446 | field | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 46 | 2 | ZoneSnow | bool | `public bool ZoneSnow;` | `public bool ZoneSnow;` |
| 3447 | field | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 48 | 2 | ZoneDesert | bool | `public bool ZoneDesert;` | `public bool ZoneDesert;` |
| 3448 | field | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 50 | 2 | ZoneGlowshroom | bool | `public bool ZoneGlowshroom;` | `public bool ZoneGlowshroom;` |
| 3449 | field | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 52 | 2 | ZoneMeteor | bool | `public bool ZoneMeteor;` | `public bool ZoneMeteor;` |
| 3450 | field | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 54 | 2 | ZoneGraveyard | bool | `public bool ZoneGraveyard;` | `public bool ZoneGraveyard;` |
| 3451 | field | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 56 | 2 | ZoneDungeon | bool | `public bool ZoneDungeon;` | `public bool ZoneDungeon;` |
| 3452 | field | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 58 | 2 | ZoneLihzhardTemple | bool | `public bool ZoneLihzhardTemple;` | `public bool ZoneLihzhardTemple;` |
| 3453 | field | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 60 | 2 | ZoneGranite | bool | `public bool ZoneGranite;` | `public bool ZoneGranite;` |
| 3454 | field | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 62 | 2 | ZoneMarble | bool | `public bool ZoneMarble;` | `public bool ZoneMarble;` |
| 3455 | field | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 64 | 2 | ZoneHive | bool | `public bool ZoneHive;` | `public bool ZoneHive;` |
| 3456 | field | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 66 | 2 | ZoneGemCave | bool | `public bool ZoneGemCave;` | `public bool ZoneGemCave;` |
| 3457 | field | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 68 | 2 | ZoneBeach | bool | `public bool ZoneBeach;` | `public bool ZoneBeach;` |
| 3458 | field | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 70 | 2 | ZoneUndergroundDesert | bool | `public bool ZoneUndergroundDesert;` | `public bool ZoneUndergroundDesert;` |

#### 属性（0）

无该类型成员记录。


## 5. 追溯与验收

> 来源报告 SHA-256：`b944441d92d5103f218b766a3e354200794528a19e65a862e44c2b40a36f1196`
- 本分区细分组数：35；成员数：485；字段：456；属性：29。
- 预期不变量：本文件只出现完整细分子系统；不跨分区复制成员；所有成员的来源序号、原始声明、路径、行列和 C# 类型必须与来源报告一致。
- 验证方式：重新读取 20 个文件，检查文件数、细分组唯一性、来源序号恰好覆盖 `1..4542`、字段/属性合计和逐成员行文本一致；本次分区生成不运行 `dotnet`，因为没有修改 C# 或项目文件。

生成器：`Build/Tools/Generate-Version4NonAuthoritativeComponentPartitionReports.ps1`。
