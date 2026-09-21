# Version4 非权威组件拆分分区 02/20：世界环境与事件

> 来源报告：`docs/migration/ledgers/Version4非权威模拟系统字段属性逐成员源码声明-更细子系统拆分-去除ID类文件.md`
> 来源报告 SHA-256：`b944441d92d5103f218b766a3e354200794528a19e65a862e44c2b40a36f1196`
> 本文件是并行组件拆分工作包，不是新的权威成员清单；成员声明事实直接保留来源报告原文。

## 1. 分区定位与评估

- 分区范围：天气、环境、季节、入侵、事件、天空和环境扫描状态。
- 本分区组件化重点：将持续世界状态、事件命令和表现投影分开，确认事件触发与清理顺序。
- 本分区包含 17 个完整细分子系统、225 条成员记录（字段 212、属性 13）。来源序号覆盖区间 `109..3855`，区间可能与其他分区交错，不以序号定义领域边界。
- 组件边界判定：细分子系统只是并行处理的最小库存边界，不等价于一个组件。必须继续按共同读者/写者、变更原因、生命周期、权威性和副作用分为 Component、System、Query、Command、Adapter 或 Projection。
- 证据状态：成员声明和统计已由来源报告校验；逐成员读者、写者、初始化/清理路径、持久化/网络语义和系统顺序仍需本分区会话补证，不得从名称或数量推断。

## 2. 分区统计

| 父级子系统 | 细分子系统数 | 字段 | 属性 | 合计 |
|---|---:|---:|---:|---:|
| `RuntimeComposition` | 5 | 54 | 0 | 54 |
| `SharedRuntimeMechanisms` | 12 | 158 | 13 | 171 |

| 原报告章节 | 父级 | 细分子系统 | 边界角色 | 字段 | 属性 | 合计 | 组件化处理 |
|---|---|---|---|---:|---:|---:|---|
| `4.1.15` | `RuntimeComposition` | `MainSlimeRainState` | runtime state | 8 | 0 | 8 | 待按成员访问模式拆分 |
| `4.1.18` | `RuntimeComposition` | `MainCalendarWeatherState` | runtime state | 16 | 0 | 16 | 待按成员访问模式拆分 |
| `4.1.21` | `RuntimeComposition` | `MainWeatherAndAmbientState` | runtime state | 11 | 0 | 11 | 待按成员访问模式拆分 |
| `4.1.31` | `RuntimeComposition` | `MainInvasionState` | runtime state | 12 | 0 | 12 | 待按成员访问模式拆分 |
| `4.1.44` | `RuntimeComposition` | `MainSeasonalAndTitleState` | presentation state | 7 | 0 | 7 | 待按成员访问模式拆分 |
| `4.9.25` | `SharedRuntimeMechanisms` | `SharedWorldEventPresentationState` | presentation state | 21 | 2 | 23 | 待按成员访问模式拆分 |
| `4.9.26` | `SharedRuntimeMechanisms` | `SharedInvasionAndBossTracking` | state/query | 10 | 4 | 14 | 待按成员访问模式拆分 |
| `4.9.29` | `SharedRuntimeMechanisms` | `SharedLightningGenerationState` | state/query | 23 | 0 | 23 | 待按成员访问模式拆分 |
| `4.9.30` | `SharedRuntimeMechanisms` | `SharedWaterfallState` | state/presentation | 11 | 0 | 11 | 待按成员访问模式拆分 |
| `4.9.103` | `SharedRuntimeMechanisms` | `WorldEnvironmentScanHelpers` | query | 9 | 0 | 9 | 待按成员访问模式拆分 |
| `4.9.152` | `SharedRuntimeMechanisms` | `SharedAmbientSkyCatalogState` | definition/presentation | 17 | 0 | 17 | 待按成员访问模式拆分 |
| `4.9.153` | `SharedRuntimeMechanisms` | `SharedAmbientSpawnAndWindState` | state/query | 11 | 0 | 11 | 待按成员访问模式拆分 |
| `4.9.159` | `SharedRuntimeMechanisms` | `SharedCelebrationAndLanternEvents` | state/definition | 14 | 2 | 16 | 待按成员访问模式拆分 |
| `4.9.160` | `SharedRuntimeMechanisms` | `SharedRitualAndStormEvents` | state/definition | 12 | 0 | 12 | 待按成员访问模式拆分 |
| `4.9.217` | `SharedRuntimeMechanisms` | `SharedSceneWeatherAndEventZoneState` | query/input | 9 | 0 | 9 | 待按成员访问模式拆分 |
| `4.9.220` | `SharedRuntimeMechanisms` | `SharedInvasionDamageTrackingState` | runtime state | 2 | 2 | 4 | 待按成员访问模式拆分 |
| `4.9.221` | `SharedRuntimeMechanisms` | `SharedInvasionWaveAndArenaState` | runtime state | 19 | 3 | 22 | 待按成员访问模式拆分 |

## 3. 并行组件拆分契约

对本文件中的每个细分子系统和每条成员记录执行以下判断，并把结果写入对应的实现阶段设计/审计记录：

- 盘点声明类型、可见性、读者、写者、创建/更新/清理/持久化生命周期、访问频率和副作用；缺证据标记 `missing`，不可用命名猜测补齐。
- 标记权威状态、派生值、缓存、快照、兼容字段和表现数据；Component 只保存一个内聚概念的权威数据。
- 需要行为转换时放入显式 System/Command；纯资格判断和派生计算放入 Query；外部协议、文件、平台和第三方类型放入 Adapter；网络/持久化/日志/UI 输出放入单向 Projection。
- 分别建模实体引用、持久化 ID、网络 ID 和外部 ID；不得把 ID 类文件或外部对象直接塞入共享组件。
- 记录 Interface、Implementation、Seam、Depth、Leverage、Locality，以及必要的系统先后约束、失败归属和重试策略。

## 4. 逐成员源码声明

以下细分子系统块保持来源报告的成员表、声明、路径、行列和来源序号；只调整 Markdown 标题层级并增加分区追踪元数据。

### 4.1 细分子系统：`MainSlimeRainState`

- 原报告章节：`4.1.15`
- 父级子系统：`RuntimeComposition`
- 分区工作包：`02` / `世界环境与事件`

- 上一级基线细分子系统：`MainSlimeRainState`
- 细分职责：史莱姆雨警告、槽位、计时和击杀进度。
- 边界角色：`runtime state`；最小 seam：slime-rain event view；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：8；属性：0；合计：8。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（8）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 185 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 536 | 2 | maxRain | int | `public static int maxRain = 750;` | `public static int maxRain = 750;` |
| 186 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 538 | 2 | slimeWarningTime | int | `public static int slimeWarningTime;` | `public static int slimeWarningTime;` |
| 187 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 540 | 2 | slimeWarningDelay | int | `public static int slimeWarningDelay = 420;` | `public static int slimeWarningDelay = 420;` |
| 188 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 542 | 2 | slimeRainNPCSlots | float | `public static float slimeRainNPCSlots = 0.65f;` | `public static float slimeRainNPCSlots = 0.65f;` |
| 189 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 544 | 2 | slimeRainNPC | bool[] | `public static bool[] slimeRainNPC = new bool[NPCID.Count];` | `public static bool[] slimeRainNPC = new bool[NPCID.Count];` |
| 190 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 546 | 2 | slimeRainTime | double | `public static double slimeRainTime;` | `public static double slimeRainTime;` |
| 191 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 548 | 2 | slimeRain | bool | `public static bool slimeRain;` | `public static bool slimeRain;` |
| 192 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 550 | 2 | slimeRainKillCount | int | `public static int slimeRainKillCount;` | `public static int slimeRainKillCount;` |

#### 属性（0）

无该类型成员记录。


### 4.2 细分子系统：`MainCalendarWeatherState`

- 原报告章节：`4.1.18`
- 父级子系统：`RuntimeComposition`
- 分区工作包：`02` / `世界环境与事件`

- 上一级基线细分子系统：`MainCalendarWeatherState`
- 细分职责：昼夜、月相、雨、血月和日食事实。
- 边界角色：`runtime state`；最小 seam：calendar/weather fact view；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：16；属性：0；合计：16。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（16）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 213 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 594 | 2 | dayTime | bool | `public static bool dayTime = true;` | `public static bool dayTime = true;` |
| 214 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 596 | 2 | time | double | `public static double time = 13500.0;` | `public static double time = 13500.0;` |
| 215 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 598 | 2 | timeForVisualEffects | double | `public static double timeForVisualEffects;` | `public static double timeForVisualEffects;` |
| 216 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 600 | 2 | moonPhase | int | `public static int moonPhase;` | `public static int moonPhase;` |
| 217 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 602 | 2 | sunModY | short | `public static short sunModY;` | `public static short sunModY;` |
| 218 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 604 | 2 | moonModY | short | `public static short moonModY;` | `public static short moonModY;` |
| 219 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 606 | 2 | bloodMoon | bool | `public static bool bloodMoon;` | `public static bool bloodMoon;` |
| 220 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 608 | 2 | pumpkinMoon | bool | `public static bool pumpkinMoon;` | `public static bool pumpkinMoon;` |
| 221 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 610 | 2 | snowMoon | bool | `public static bool snowMoon;` | `public static bool snowMoon;` |
| 222 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 612 | 2 | cloudAlpha | float | `public static float cloudAlpha;` | `public static float cloudAlpha;` |
| 223 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 614 | 2 | maxRaining | float | `public static float maxRaining;` | `public static float maxRaining;` |
| 224 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 616 | 2 | oldMaxRaining | float | `public static float oldMaxRaining;` | `public static float oldMaxRaining;` |
| 225 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 618 | 2 | rainTime | int | `public static int rainTime;` | `public static int rainTime;` |
| 226 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 620 | 2 | raining | bool | `public static bool raining;` | `public static bool raining;` |
| 227 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 622 | 2 | coinRain | int | `public static int coinRain;` | `public static int coinRain;` |
| 228 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 624 | 2 | eclipse | bool | `public static bool eclipse;` | `public static bool eclipse;` |

#### 属性（0）

无该类型成员记录。


### 4.3 细分子系统：`MainWeatherAndAmbientState`

- 原报告章节：`4.1.21`
- 父级子系统：`RuntimeComposition`
- 分区工作包：`02` / `世界环境与事件`

- 上一级基线细分子系统：`MainWeatherAndAmbientState`
- 细分职责：星体、云层、风和环境对象状态。
- 边界角色：`runtime state`；最小 seam：weather/ambient view；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：11；属性：0；合计：11。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（11）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 240 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 649 | 2 | numStars | int | `public static int numStars;` | `public static int numStars;` |
| 241 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 652 | 2 | weatherCounter | int | `public static int weatherCounter;` | `public static int weatherCounter;` |
| 242 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 654 | 2 | numClouds | int | `public static int numClouds = 200;` | `public static int numClouds = 200;` |
| 243 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 656 | 2 | numCloudsTemp | int | `public static int numCloudsTemp = numClouds;` | `public static int numCloudsTemp = numClouds;` |
| 244 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 658 | 2 | windSpeedCurrent | float | `public static float windSpeedCurrent;` | `public static float windSpeedCurrent;` |
| 245 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 660 | 2 | windSpeedTarget | float | `public static float windSpeedTarget;` | `public static float windSpeedTarget;` |
| 246 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 662 | 2 | windCounter | int | `public static int windCounter;` | `public static int windCounter;` |
| 247 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 664 | 2 | extremeWindCounter | int | `public static int extremeWindCounter;` | `public static int extremeWindCounter;` |
| 248 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 666 | 2 | windPhysics | bool | `public static bool windPhysics = false;` | `public static bool windPhysics = false;` |
| 249 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 668 | 2 | windPhysicsStrength | float | `public static float windPhysicsStrength = 0.1f;` | `public static float windPhysicsStrength = 0.1f;` |
| 250 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 671 | 2 | cloud | Terraria.Cloud[] | `public static Cloud[] cloud = new Cloud[200];` | `public static Cloud[] cloud = new Cloud[200];` |

#### 属性（0）

无该类型成员记录。


### 4.4 细分子系统：`MainInvasionState`

- 原报告章节：`4.1.31`
- 父级子系统：`RuntimeComposition`
- 分区工作包：`02` / `世界环境与事件`

- 上一级基线细分子系统：`MainInvasionState`
- 细分职责：入侵类型、位置、波次和进度事实。
- 边界角色：`runtime state`；最小 seam：invasion fact view；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：12；属性：0；合计：12。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（12）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 433 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1059 | 2 | invasionType | int | `public static int invasionType;` | `public static int invasionType;` |
| 434 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1061 | 2 | invasionX | double | `public static double invasionX;` | `public static double invasionX;` |
| 435 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1063 | 2 | invasionSize | int | `public static int invasionSize;` | `public static int invasionSize;` |
| 436 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1065 | 2 | invasionDelay | int | `public static int invasionDelay;` | `public static int invasionDelay;` |
| 437 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1067 | 2 | invasionWarn | int | `public static int invasionWarn;` | `public static int invasionWarn;` |
| 438 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1069 | 2 | invasionSizeStart | int | `public static int invasionSizeStart;` | `public static int invasionSizeStart;` |
| 439 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1071 | 2 | invasionProgressIcon | int | `public static int invasionProgressIcon;` | `public static int invasionProgressIcon;` |
| 440 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1073 | 2 | invasionProgress | int | `public static int invasionProgress;` | `public static int invasionProgress;` |
| 441 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1075 | 2 | invasionProgressMax | int | `public static int invasionProgressMax;` | `public static int invasionProgressMax;` |
| 442 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1077 | 2 | invasionProgressWave | int | `public static int invasionProgressWave;` | `public static int invasionProgressWave;` |
| 443 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1079 | 2 | invasionProgressDisplayLeft | int | `public static int invasionProgressDisplayLeft;` | `public static int invasionProgressDisplayLeft;` |
| 444 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1081 | 2 | invasionProgressAlpha | float | `public static float invasionProgressAlpha;` | `public static float invasionProgressAlpha;` |

#### 属性（0）

无该类型成员记录。


### 4.5 细分子系统：`MainSeasonalAndTitleState`

- 原报告章节：`4.1.44`
- 父级子系统：`RuntimeComposition`
- 分区工作包：`02` / `世界环境与事件`

- 上一级基线细分子系统：`MainBackgroundAndSeasonalState`
- 细分职责：节日开关、强制节日和标题切换状态。
- 边界角色：`presentation state`；最小 seam：main seasonal title port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：7；属性：0；合计：7。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（7）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 109 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 378 | 2 | xMas | bool | `public static bool xMas;` | `public static bool xMas;` |
| 110 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 380 | 2 | halloween | bool | `public static bool halloween;` | `public static bool halloween;` |
| 111 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 382 | 2 | forceXMasForToday | bool | `public static bool forceXMasForToday;` | `public static bool forceXMasForToday;` |
| 112 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 384 | 2 | forceHalloweenForToday | bool | `public static bool forceHalloweenForToday;` | `public static bool forceHalloweenForToday;` |
| 113 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 386 | 2 | forceXMasForever | bool | `public static bool forceXMasForever;` | `public static bool forceXMasForever;` |
| 114 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 388 | 2 | forceHalloweenForever | bool | `public static bool forceHalloweenForever;` | `public static bool forceHalloweenForever;` |
| 115 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 390 | 2 | changeTheTitle | bool | `public static bool changeTheTitle;` | `public static bool changeTheTitle;` |

#### 属性（0）

无该类型成员记录。


### 4.6 细分子系统：`SharedWorldEventPresentationState`

- 原报告章节：`4.9.25`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`02` / `世界环境与事件`

- 上一级基线细分子系统：`SharedWorldEventPresentationState`
- 细分职责：月总死亡戏剧、制作人员名单和屏幕遮挡表现状态。
- 边界角色：`presentation state`；最小 seam：world event presentation port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：3；声明类型数：5；字段：21；属性：2；合计：23。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（21）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1541 | field | Terraria.GameContent.Events.CreditsRollEvent | Terraria.GameContent.Events/CreditsRollEvent.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\CreditsRollEvent.cs | 8 | 2 | MAX_TIME_FOR_CREDITS_ROLL_IN_FRAMES | int | `private const int MAX_TIME_FOR_CREDITS_ROLL_IN_FRAMES = 28800;` | `private const int MAX_TIME_FOR_CREDITS_ROLL_IN_FRAMES = 28800;` |
| 1542 | field | Terraria.GameContent.Events.CreditsRollEvent | Terraria.GameContent.Events/CreditsRollEvent.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\CreditsRollEvent.cs | 10 | 2 | _creditsRollRemainingTime | int | `private static int _creditsRollRemainingTime;` | `private static int _creditsRollRemainingTime;` |
| 1575 | field | Terraria.GameContent.Events.MoonlordDeathDrama.MoonlordPiece | Terraria.GameContent.Events/MoonlordDeathDrama.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\MoonlordDeathDrama.cs | 13 | 3 | _texture | Texture2D | `private Texture2D _texture;` | `private Texture2D _texture;` |
| 1576 | field | Terraria.GameContent.Events.MoonlordDeathDrama.MoonlordPiece | Terraria.GameContent.Events/MoonlordDeathDrama.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\MoonlordDeathDrama.cs | 15 | 3 | _position | Vector2 | `private Vector2 _position;` | `private Vector2 _position;` |
| 1577 | field | Terraria.GameContent.Events.MoonlordDeathDrama.MoonlordPiece | Terraria.GameContent.Events/MoonlordDeathDrama.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\MoonlordDeathDrama.cs | 17 | 3 | _velocity | Vector2 | `private Vector2 _velocity;` | `private Vector2 _velocity;` |
| 1578 | field | Terraria.GameContent.Events.MoonlordDeathDrama.MoonlordPiece | Terraria.GameContent.Events/MoonlordDeathDrama.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\MoonlordDeathDrama.cs | 19 | 3 | _origin | Vector2 | `private Vector2 _origin;` | `private Vector2 _origin;` |
| 1579 | field | Terraria.GameContent.Events.MoonlordDeathDrama.MoonlordPiece | Terraria.GameContent.Events/MoonlordDeathDrama.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\MoonlordDeathDrama.cs | 21 | 3 | _rotation | float | `private float _rotation;` | `private float _rotation;` |
| 1580 | field | Terraria.GameContent.Events.MoonlordDeathDrama.MoonlordPiece | Terraria.GameContent.Events/MoonlordDeathDrama.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\MoonlordDeathDrama.cs | 23 | 3 | _rotationVelocity | float | `private float _rotationVelocity;` | `private float _rotationVelocity;` |
| 1581 | field | Terraria.GameContent.Events.MoonlordDeathDrama.MoonlordExplosion | Terraria.GameContent.Events/MoonlordDeathDrama.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\MoonlordDeathDrama.cs | 61 | 3 | _texture | Texture2D | `private Texture2D _texture;` | `private Texture2D _texture;` |
| 1582 | field | Terraria.GameContent.Events.MoonlordDeathDrama.MoonlordExplosion | Terraria.GameContent.Events/MoonlordDeathDrama.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\MoonlordDeathDrama.cs | 63 | 3 | _position | Vector2 | `private Vector2 _position;` | `private Vector2 _position;` |
| 1583 | field | Terraria.GameContent.Events.MoonlordDeathDrama.MoonlordExplosion | Terraria.GameContent.Events/MoonlordDeathDrama.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\MoonlordDeathDrama.cs | 65 | 3 | _origin | Vector2 | `private Vector2 _origin;` | `private Vector2 _origin;` |
| 1584 | field | Terraria.GameContent.Events.MoonlordDeathDrama.MoonlordExplosion | Terraria.GameContent.Events/MoonlordDeathDrama.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\MoonlordDeathDrama.cs | 67 | 3 | _frame | Rectangle | `private Rectangle _frame;` | `private Rectangle _frame;` |
| 1585 | field | Terraria.GameContent.Events.MoonlordDeathDrama.MoonlordExplosion | Terraria.GameContent.Events/MoonlordDeathDrama.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\MoonlordDeathDrama.cs | 69 | 3 | _frameCounter | int | `private int _frameCounter;` | `private int _frameCounter;` |
| 1586 | field | Terraria.GameContent.Events.MoonlordDeathDrama.MoonlordExplosion | Terraria.GameContent.Events/MoonlordDeathDrama.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\MoonlordDeathDrama.cs | 71 | 3 | _frameSpeed | int | `private int _frameSpeed;` | `private int _frameSpeed;` |
| 1587 | field | Terraria.GameContent.Events.MoonlordDeathDrama | Terraria.GameContent.Events/MoonlordDeathDrama.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\MoonlordDeathDrama.cs | 105 | 2 | _pieces | System.Collections.Generic.List<Terraria.GameContent.Events.MoonlordDeathDrama.MoonlordPiece> | `private static List<MoonlordPiece> _pieces = new List<MoonlordPiece>();` | `private static List<MoonlordPiece> _pieces = new List<MoonlordPiece>();` |
| 1588 | field | Terraria.GameContent.Events.MoonlordDeathDrama | Terraria.GameContent.Events/MoonlordDeathDrama.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\MoonlordDeathDrama.cs | 107 | 2 | _explosions | System.Collections.Generic.List<Terraria.GameContent.Events.MoonlordDeathDrama.MoonlordExplosion> | `private static List<MoonlordExplosion> _explosions = new List<MoonlordExplosion>();` | `private static List<MoonlordExplosion> _explosions = new List<MoonlordExplosion>();` |
| 1589 | field | Terraria.GameContent.Events.MoonlordDeathDrama | Terraria.GameContent.Events/MoonlordDeathDrama.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\MoonlordDeathDrama.cs | 109 | 2 | _lightSources | System.Collections.Generic.List<Vector2> | `private static List<Vector2> _lightSources = new List<Vector2>();` | `private static List<Vector2> _lightSources = new List<Vector2>();` |
| 1590 | field | Terraria.GameContent.Events.MoonlordDeathDrama | Terraria.GameContent.Events/MoonlordDeathDrama.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\MoonlordDeathDrama.cs | 111 | 2 | whitening | float | `private static float whitening;` | `private static float whitening;` |
| 1591 | field | Terraria.GameContent.Events.MoonlordDeathDrama | Terraria.GameContent.Events/MoonlordDeathDrama.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\MoonlordDeathDrama.cs | 113 | 2 | requestedLight | float | `private static float requestedLight;` | `private static float requestedLight;` |
| 1602 | field | Terraria.GameContent.Events.ScreenObstruction | Terraria.GameContent.Events/ScreenObstruction.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\ScreenObstruction.cs | 8 | 2 | lastSpeed | float | `public static float lastSpeed = 0.1f;` | `public static float lastSpeed = 0.1f;` |
| 1603 | field | Terraria.GameContent.Events.ScreenObstruction | Terraria.GameContent.Events/ScreenObstruction.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\ScreenObstruction.cs | 10 | 2 | screenObstruction | float | `public static float screenObstruction;` | `public static float screenObstruction;` |

#### 属性（2）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3784 | property | Terraria.GameContent.Events.MoonlordDeathDrama.MoonlordPiece | Terraria.GameContent.Events/MoonlordDeathDrama.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\MoonlordDeathDrama.cs | 25 | 3 | Dead | bool | `public bool Dead { get { if (!(_position.Y > (float)(Main.maxTilesY * 16) - 480f) && !(_position.X < 480f)) { return _position.X >= (float)(Main.maxTilesX * 16) - 480f; } return true; } }` | `public bool Dead { get { if (!(_position.Y > (float)(Main.maxTilesY * 16) - 480f) && !(_position.X < 480f)) { return _position.X >= (float)(Main.maxTilesX * 16) - 480f; } return true; } }` |
| 3785 | property | Terraria.GameContent.Events.MoonlordDeathDrama.MoonlordExplosion | Terraria.GameContent.Events/MoonlordDeathDrama.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\MoonlordDeathDrama.cs | 73 | 3 | Dead | bool | `public bool Dead { get { if (!(_position.Y > (float)(Main.maxTilesY * 16) - 480f) && !(_position.X < 480f) && !(_position.X >= (float)(Main.maxTilesX * 16) - 480f)) { return _frameCounter >= _frameSpeed * 7; } return true; } }` | `public bool Dead { get { if (!(_position.Y > (float)(Main.maxTilesY * 16) - 480f) && !(_position.X < 480f) && !(_position.X >= (float)(Main.maxTilesX * 16) - 480f)) { return _frameCounter >= _frameSpeed * 7; } return true; } }` |


### 4.7 细分子系统：`SharedInvasionAndBossTracking`

- 原报告章节：`4.9.26`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`02` / `世界环境与事件`

- 上一级基线细分子系统：`SharedInvasionAndBossTracking`
- 细分职责：入侵、Boss 伤害和旗帜进度跟踪。
- 边界角色：`state/query`；最小 seam：invasion tracking port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：3；声明类型数：3；字段：10；属性：4；合计：14。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（10）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2338 | field | Terraria.GameContent.BannerSystem | Terraria.GameContent/BannerSystem.cs | D:\TRbackup\Version4\Terraria.GameContent\BannerSystem.cs | 52 | 2 | MaxBannerTypes | int | `public static readonly int MaxBannerTypes = 293;` | `public static readonly int MaxBannerTypes = 293;` |
| 2339 | field | Terraria.GameContent.BannerSystem | Terraria.GameContent/BannerSystem.cs | D:\TRbackup\Version4\Terraria.GameContent\BannerSystem.cs | 54 | 2 | killCount | int[] | `private static int[] killCount = new int[MaxBannerTypes];` | `private static int[] killCount = new int[MaxBannerTypes];` |
| 2340 | field | Terraria.GameContent.BannerSystem | Terraria.GameContent/BannerSystem.cs | D:\TRbackup\Version4\Terraria.GameContent\BannerSystem.cs | 56 | 2 | claimableBanners | ushort[] | `private static ushort[] claimableBanners = new ushort[MaxBannerTypes];` | `private static ushort[] claimableBanners = new ushort[MaxBannerTypes];` |
| 2341 | field | Terraria.GameContent.BannerSystem | Terraria.GameContent/BannerSystem.cs | D:\TRbackup\Version4\Terraria.GameContent\BannerSystem.cs | 58 | 2 | AnyNewClaimableBanners | bool | `public static bool AnyNewClaimableBanners;` | `public static bool AnyNewClaimableBanners;` |
| 2342 | field | Terraria.GameContent.BossDamageTracker | Terraria.GameContent/BossDamageTracker.cs | D:\TRbackup\Version4\Terraria.GameContent\BossDamageTracker.cs | 7 | 2 | _type | int | `private readonly int _type;` | `private readonly int _type;` |
| 2343 | field | Terraria.GameContent.BossDamageTracker | Terraria.GameContent/BossDamageTracker.cs | D:\TRbackup\Version4\Terraria.GameContent\BossDamageTracker.cs | 9 | 2 | _overrides | Terraria.GameContent.NPCDamageTracker.CustomDefinition | `private readonly CustomDefinition _overrides;` | `private readonly CustomDefinition _overrides;` |
| 2344 | field | Terraria.GameContent.BossDamageTracker | Terraria.GameContent/BossDamageTracker.cs | D:\TRbackup\Version4\Terraria.GameContent\BossDamageTracker.cs | 11 | 2 | _killed | bool | `private bool _killed;` | `private bool _killed;` |
| 2426 | field | Terraria.GameContent.InvasionDamageTracker | Terraria.GameContent/InvasionDamageTracker.cs | D:\TRbackup\Version4\Terraria.GameContent\InvasionDamageTracker.cs | 8 | 2 | VanillaInvasionNameKeys | System.Collections.Generic.Dictionary<int, string> | `private static Dictionary<int, string> VanillaInvasionNameKeys = new Dictionary<int, string>  	{  		{ 1, "Bestiary_Invasions.Goblins" },  		{ 2, "Bestiary_Invasions.FrostLegion" },  		{ 3, "Bestiary_Invasions.Pirates" },  		{ 4, "Bestiary_Invasions.Martian" },  		{ -2, "Bestiary_Invasions.PumpkinMoon" },  		{ -1, "Bestiary_Invasions.FrostMoon" }  	};` | `private static Dictionary<int, string> VanillaInvasionNameKeys = new Dictionary<int, string> { { 1, "Bestiary_Invasions.Goblins" }, { 2, "Bestiary_Invasions.FrostLegion" }, { 3, "Bestiary_Invasions.Pirates" }, { 4, "Bestiary_Invasions.Martian" }, { -2, "Bestiary_Invasions.PumpkinMoon" }, { -1, "Bestiary_Invasions.FrostMoon" } };` |
| 2427 | field | Terraria.GameContent.InvasionDamageTracker | Terraria.GameContent/InvasionDamageTracker.cs | D:\TRbackup\Version4\Terraria.GameContent\InvasionDamageTracker.cs | 18 | 2 | _invasionGroup | int | `private readonly int _invasionGroup;` | `private readonly int _invasionGroup;` |
| 2428 | field | Terraria.GameContent.InvasionDamageTracker | Terraria.GameContent/InvasionDamageTracker.cs | D:\TRbackup\Version4\Terraria.GameContent\InvasionDamageTracker.cs | 20 | 2 | _name | Terraria.Localization.LocalizedText | `private readonly LocalizedText _name;` | `private readonly LocalizedText _name;` |

#### 属性（4）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3847 | property | Terraria.GameContent.BossDamageTracker | Terraria.GameContent/BossDamageTracker.cs | D:\TRbackup\Version4\Terraria.GameContent\BossDamageTracker.cs | 13 | 2 | Name | Terraria.Localization.LocalizedText | `public override LocalizedText Name { get { if (_overrides == null \|\| _overrides.Name == null) { return Lang.GetNPCName(_type); } return _overrides.Name; } }` | `public override LocalizedText Name { get { if (_overrides == null \|\| _overrides.Name == null) { return Lang.GetNPCName(_type); } return _overrides.Name; } }` |
| 3848 | property | Terraria.GameContent.BossDamageTracker | Terraria.GameContent/BossDamageTracker.cs | D:\TRbackup\Version4\Terraria.GameContent\BossDamageTracker.cs | 25 | 2 | KillTimeMessage | Terraria.Localization.LocalizedText | `public override LocalizedText KillTimeMessage => Language.GetText(_killed ? "BossDamageCommand.KillTime" : "BossDamageCommand.KillTimeEscaped");` | `public override LocalizedText KillTimeMessage => Language.GetText(_killed ? "BossDamageCommand.KillTime" : "BossDamageCommand.KillTimeEscaped");` |
| 3854 | property | Terraria.GameContent.InvasionDamageTracker | Terraria.GameContent/InvasionDamageTracker.cs | D:\TRbackup\Version4\Terraria.GameContent\InvasionDamageTracker.cs | 22 | 2 | Name | Terraria.Localization.LocalizedText | `public override LocalizedText Name => _name;` | `public override LocalizedText Name => _name;` |
| 3855 | property | Terraria.GameContent.InvasionDamageTracker | Terraria.GameContent/InvasionDamageTracker.cs | D:\TRbackup\Version4\Terraria.GameContent\InvasionDamageTracker.cs | 24 | 2 | KillTimeMessage | Terraria.Localization.LocalizedText | `public override LocalizedText KillTimeMessage => null;` | `public override LocalizedText KillTimeMessage => null;` |


### 4.8 细分子系统：`SharedLightningGenerationState`

- 原报告章节：`4.9.29`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`02` / `世界环境与事件`

- 上一级基线细分子系统：`SharedLightningGenerationState`
- 细分职责：闪电生成参数、分叉递归和 Tile 碰撞状态。
- 边界角色：`state/query`；最小 seam：lightning generation port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：3；字段：23；属性：0；合计：23。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（23）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2433 | field | Terraria.GameContent.LightningGenerator.Bolt | Terraria.GameContent/LightningGenerator.cs | D:\TRbackup\Version4\Terraria.GameContent\LightningGenerator.cs | 13 | 3 | positions | Vector2[] | `public Vector2[] positions;` | `public Vector2[] positions;` |
| 2434 | field | Terraria.GameContent.LightningGenerator.Bolt | Terraria.GameContent/LightningGenerator.cs | D:\TRbackup\Version4\Terraria.GameContent\LightningGenerator.cs | 15 | 3 | rotations | float[] | `public float[] rotations;` | `public float[] rotations;` |
| 2435 | field | Terraria.GameContent.LightningGenerator.Bolt | Terraria.GameContent/LightningGenerator.cs | D:\TRbackup\Version4\Terraria.GameContent\LightningGenerator.cs | 17 | 3 | progressRange | Terraria.Utilities.Terraria.Utilities.FloatRange | `public FloatRange progressRange;` | `public FloatRange progressRange;` |
| 2436 | field | Terraria.GameContent.LightningGenerator.Bolt | Terraria.GameContent/LightningGenerator.cs | D:\TRbackup\Version4\Terraria.GameContent\LightningGenerator.cs | 19 | 3 | forkDepth | int | `public int forkDepth;` | `public int forkDepth;` |
| 2437 | field | Terraria.GameContent.LightningGenerator.Bolt | Terraria.GameContent/LightningGenerator.cs | D:\TRbackup\Version4\Terraria.GameContent\LightningGenerator.cs | 21 | 3 | collidedWithTile | bool | `public bool collidedWithTile;` | `public bool collidedWithTile;` |
| 2438 | field | Terraria.GameContent.LightningGenerator.StormLightning | Terraria.GameContent/LightningGenerator.cs | D:\TRbackup\Version4\Terraria.GameContent\LightningGenerator.cs | 26 | 3 | Generator | Terraria.GameContent.LightningGenerator | `public static LightningGenerator Generator = new LightningGenerator  		{  			RotationStrength = 0.9f,  			StepSize = 8,  			Layers = 4,  			LayerStrengthFactor = 1.5f,  			PerpendicularDeviationFactor = 5f,  			ReduceRandomnessAfter = 0.8f,  			ForkGenerationThresholdAngleFraction = 0.65f,  			ForkReflectAngleMultiplier = 0.4f,  			ForkRotationStrengthMultiplier = 0.9f,  			ForkStepSizeMultiplier = 0.8f,  			ForkLengthMultiplier = 0.8f,  			MaxForksPerBolt = 2,  			MaxForkDepth = 2,  			ForkProgressRange = new FloatRange(0.3f, 0.8f),  			SolidTileCollision = true  		};` | `public static LightningGenerator Generator = new LightningGenerator { RotationStrength = 0.9f, StepSize = 8, Layers = 4, LayerStrengthFactor = 1.5f, PerpendicularDeviationFactor = 5f, ReduceRandomnessAfter = 0.8f, ForkGenerationThresholdAngleFraction = 0.65f, ForkReflectAngleMultiplier = 0.4f, ForkRotationStrengthMultiplier = 0.9f, ForkStepSizeMultiplier = 0.8f, ForkLengthMultiplier = 0.8f, MaxForksPerBolt = 2, MaxForkDepth = 2, ForkProgressRange = new FloatRange(0.3f, 0.8f), SolidTileCollision = true };` |
| 2439 | field | Terraria.GameContent.LightningGenerator.StormLightning | Terraria.GameContent/LightningGenerator.cs | D:\TRbackup\Version4\Terraria.GameContent\LightningGenerator.cs | 45 | 3 | SourceRotationLimit | float | `private static float SourceRotationLimit = (float)Math.PI / 9f;` | `private static float SourceRotationLimit = (float)Math.PI / 9f;` |
| 2440 | field | Terraria.GameContent.LightningGenerator.StormLightning | Terraria.GameContent/LightningGenerator.cs | D:\TRbackup\Version4\Terraria.GameContent\LightningGenerator.cs | 47 | 3 | Length | float | `private static float Length = 1000f;` | `private static float Length = 1000f;` |
| 2441 | field | Terraria.GameContent.LightningGenerator | Terraria.GameContent/LightningGenerator.cs | D:\TRbackup\Version4\Terraria.GameContent\LightningGenerator.cs | 65 | 2 | SolidTileCollision | bool | `public bool SolidTileCollision;` | `public bool SolidTileCollision;` |
| 2442 | field | Terraria.GameContent.LightningGenerator | Terraria.GameContent/LightningGenerator.cs | D:\TRbackup\Version4\Terraria.GameContent\LightningGenerator.cs | 67 | 2 | RotationStrength | float | `public float RotationStrength;` | `public float RotationStrength;` |
| 2443 | field | Terraria.GameContent.LightningGenerator | Terraria.GameContent/LightningGenerator.cs | D:\TRbackup\Version4\Terraria.GameContent\LightningGenerator.cs | 69 | 2 | StepSize | int | `public int StepSize;` | `public int StepSize;` |
| 2444 | field | Terraria.GameContent.LightningGenerator | Terraria.GameContent/LightningGenerator.cs | D:\TRbackup\Version4\Terraria.GameContent\LightningGenerator.cs | 71 | 2 | Layers | int | `public int Layers;` | `public int Layers;` |
| 2445 | field | Terraria.GameContent.LightningGenerator | Terraria.GameContent/LightningGenerator.cs | D:\TRbackup\Version4\Terraria.GameContent\LightningGenerator.cs | 73 | 2 | LayerStrengthFactor | float | `public float LayerStrengthFactor;` | `public float LayerStrengthFactor;` |
| 2446 | field | Terraria.GameContent.LightningGenerator | Terraria.GameContent/LightningGenerator.cs | D:\TRbackup\Version4\Terraria.GameContent\LightningGenerator.cs | 75 | 2 | PerpendicularDeviationFactor | float | `public float PerpendicularDeviationFactor;` | `public float PerpendicularDeviationFactor;` |
| 2447 | field | Terraria.GameContent.LightningGenerator | Terraria.GameContent/LightningGenerator.cs | D:\TRbackup\Version4\Terraria.GameContent\LightningGenerator.cs | 77 | 2 | ReduceRandomnessAfter | float | `public float ReduceRandomnessAfter;` | `public float ReduceRandomnessAfter;` |
| 2448 | field | Terraria.GameContent.LightningGenerator | Terraria.GameContent/LightningGenerator.cs | D:\TRbackup\Version4\Terraria.GameContent\LightningGenerator.cs | 79 | 2 | ForkGenerationThresholdAngleFraction | float | `public float ForkGenerationThresholdAngleFraction;` | `public float ForkGenerationThresholdAngleFraction;` |
| 2449 | field | Terraria.GameContent.LightningGenerator | Terraria.GameContent/LightningGenerator.cs | D:\TRbackup\Version4\Terraria.GameContent\LightningGenerator.cs | 81 | 2 | ForkReflectAngleMultiplier | float | `public float ForkReflectAngleMultiplier;` | `public float ForkReflectAngleMultiplier;` |
| 2450 | field | Terraria.GameContent.LightningGenerator | Terraria.GameContent/LightningGenerator.cs | D:\TRbackup\Version4\Terraria.GameContent\LightningGenerator.cs | 83 | 2 | ForkRotationStrengthMultiplier | float | `public float ForkRotationStrengthMultiplier;` | `public float ForkRotationStrengthMultiplier;` |
| 2451 | field | Terraria.GameContent.LightningGenerator | Terraria.GameContent/LightningGenerator.cs | D:\TRbackup\Version4\Terraria.GameContent\LightningGenerator.cs | 85 | 2 | ForkStepSizeMultiplier | float | `public float ForkStepSizeMultiplier;` | `public float ForkStepSizeMultiplier;` |
| 2452 | field | Terraria.GameContent.LightningGenerator | Terraria.GameContent/LightningGenerator.cs | D:\TRbackup\Version4\Terraria.GameContent\LightningGenerator.cs | 87 | 2 | ForkLengthMultiplier | float | `public float ForkLengthMultiplier;` | `public float ForkLengthMultiplier;` |
| 2453 | field | Terraria.GameContent.LightningGenerator | Terraria.GameContent/LightningGenerator.cs | D:\TRbackup\Version4\Terraria.GameContent\LightningGenerator.cs | 89 | 2 | MaxForksPerBolt | int | `public int MaxForksPerBolt;` | `public int MaxForksPerBolt;` |
| 2454 | field | Terraria.GameContent.LightningGenerator | Terraria.GameContent/LightningGenerator.cs | D:\TRbackup\Version4\Terraria.GameContent\LightningGenerator.cs | 91 | 2 | MaxForkDepth | int | `public int MaxForkDepth;` | `public int MaxForkDepth;` |
| 2455 | field | Terraria.GameContent.LightningGenerator | Terraria.GameContent/LightningGenerator.cs | D:\TRbackup\Version4\Terraria.GameContent\LightningGenerator.cs | 93 | 2 | ForkProgressRange | Terraria.Utilities.Terraria.Utilities.FloatRange | `public FloatRange ForkProgressRange;` | `public FloatRange ForkProgressRange;` |

#### 属性（0）

无该类型成员记录。


### 4.9 细分子系统：`SharedWaterfallState`

- 原报告章节：`4.9.30`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`02` / `世界环境与事件`

- 上一级基线细分子系统：`SharedWaterfallState`
- 细分职责：瀑布槽位、长度限制和瀑布绘制资源状态。
- 边界角色：`state/presentation`；最小 seam：waterfall manager port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：2；字段：11；属性：0；合计：11。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（11）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3655 | field | Terraria.WaterfallManager.WaterfallData | Terraria/WaterfallManager.cs | D:\TRbackup\Version4\Terraria\WaterfallManager.cs | 15 | 3 | x | int | `public int x;` | `public int x;` |
| 3656 | field | Terraria.WaterfallManager.WaterfallData | Terraria/WaterfallManager.cs | D:\TRbackup\Version4\Terraria\WaterfallManager.cs | 17 | 3 | y | int | `public int y;` | `public int y;` |
| 3657 | field | Terraria.WaterfallManager.WaterfallData | Terraria/WaterfallManager.cs | D:\TRbackup\Version4\Terraria\WaterfallManager.cs | 19 | 3 | type | int | `public int type;` | `public int type;` |
| 3658 | field | Terraria.WaterfallManager.WaterfallData | Terraria/WaterfallManager.cs | D:\TRbackup\Version4\Terraria\WaterfallManager.cs | 21 | 3 | stopAtStep | int | `public int stopAtStep;` | `public int stopAtStep;` |
| 3659 | field | Terraria.WaterfallManager | Terraria/WaterfallManager.cs | D:\TRbackup\Version4\Terraria\WaterfallManager.cs | 24 | 2 | minWet | int | `private const int minWet = 160;` | `private const int minWet = 160;` |
| 3660 | field | Terraria.WaterfallManager | Terraria/WaterfallManager.cs | D:\TRbackup\Version4\Terraria\WaterfallManager.cs | 26 | 2 | maxWaterfallCountDefault | int | `private const int maxWaterfallCountDefault = 1000;` | `private const int maxWaterfallCountDefault = 1000;` |
| 3661 | field | Terraria.WaterfallManager | Terraria/WaterfallManager.cs | D:\TRbackup\Version4\Terraria\WaterfallManager.cs | 28 | 2 | maxLength | int | `private const int maxLength = 100;` | `private const int maxLength = 100;` |
| 3662 | field | Terraria.WaterfallManager | Terraria/WaterfallManager.cs | D:\TRbackup\Version4\Terraria\WaterfallManager.cs | 30 | 2 | maxTypes | int | `private const int maxTypes = 28;` | `private const int maxTypes = 28;` |
| 3663 | field | Terraria.WaterfallManager | Terraria/WaterfallManager.cs | D:\TRbackup\Version4\Terraria\WaterfallManager.cs | 32 | 2 | maxWaterfallCount | int | `public int maxWaterfallCount = 1000;` | `public int maxWaterfallCount = 1000;` |
| 3664 | field | Terraria.WaterfallManager | Terraria/WaterfallManager.cs | D:\TRbackup\Version4\Terraria\WaterfallManager.cs | 34 | 2 | waterfalls | Terraria.WaterfallManager.WaterfallData[] | `private WaterfallData[] waterfalls = new WaterfallData[1000];` | `private WaterfallData[] waterfalls = new WaterfallData[1000];` |
| 3665 | field | Terraria.WaterfallManager | Terraria/WaterfallManager.cs | D:\TRbackup\Version4\Terraria\WaterfallManager.cs | 36 | 2 | waterfallTexture | Asset<Texture2D>[] | `private Asset<Texture2D>[] waterfallTexture = new Asset<Texture2D>[28];` | `private Asset<Texture2D>[] waterfallTexture = new Asset<Texture2D>[28];` |

#### 属性（0）

无该类型成员记录。


### 4.10 细分子系统：`WorldEnvironmentScanHelpers`

- 原报告章节：`4.9.103`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`02` / `世界环境与事件`

- 上一级基线细分子系统：`WorldEnvironmentScanHelpers`
- 细分职责：洞察扫描、不可破坏墙扫描和虚空镜辅助查询。
- 边界角色：`query`；最小 seam：world environment scan port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：3；声明类型数：3；字段：9；属性：0；合计：9。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（9）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2538 | field | Terraria.GameContent.SpelunkerProjectileHelper | Terraria.GameContent/SpelunkerProjectileHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\SpelunkerProjectileHelper.cs | 8 | 2 | _positionsChecked | System.Collections.Generic.HashSet<Vector2> | `private HashSet<Vector2> _positionsChecked = new HashSet<Vector2>();` | `private HashSet<Vector2> _positionsChecked = new HashSet<Vector2>();` |
| 2539 | field | Terraria.GameContent.SpelunkerProjectileHelper | Terraria.GameContent/SpelunkerProjectileHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\SpelunkerProjectileHelper.cs | 10 | 2 | _tilesChecked | System.Collections.Generic.HashSet<Point> | `private HashSet<Point> _tilesChecked = new HashSet<Point>();` | `private HashSet<Point> _tilesChecked = new HashSet<Point>();` |
| 2540 | field | Terraria.GameContent.SpelunkerProjectileHelper | Terraria.GameContent/SpelunkerProjectileHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\SpelunkerProjectileHelper.cs | 12 | 2 | _clampBox | Rectangle | `private Rectangle _clampBox;` | `private Rectangle _clampBox;` |
| 2541 | field | Terraria.GameContent.SpelunkerProjectileHelper | Terraria.GameContent/SpelunkerProjectileHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\SpelunkerProjectileHelper.cs | 14 | 2 | _frameCounter | int | `private int _frameCounter;` | `private int _frameCounter;` |
| 2592 | field | Terraria.GameContent.UnbreakableWallScan | Terraria.GameContent/UnbreakableWallScan.cs | D:\TRbackup\Version4\Terraria.GameContent\UnbreakableWallScan.cs | 16 | 2 | ScanDistance | int | `public static readonly int ScanDistance = 250;` | `public static readonly int ScanDistance = 250;` |
| 2593 | field | Terraria.GameContent.UnbreakableWallScan | Terraria.GameContent/UnbreakableWallScan.cs | D:\TRbackup\Version4\Terraria.GameContent\UnbreakableWallScan.cs | 18 | 2 | Directions | Point[] | `public static readonly Point[] Directions = new Point[8]  	{  		new Point(1, 0),  		new Point(1, 1),  		new Point(0, 1),  		new Point(-1, 1),  		new Point(-1, 0),  		new Point(-1, -1),  		new Point(0, -1),  		new Point(1, -1)  	};` | `public static readonly Point[] Directions = new Point[8] { new Point(1, 0), new Point(1, 1), new Point(0, 1), new Point(-1, 1), new Point(-1, 0), new Point(-1, -1), new Point(0, -1), new Point(1, -1) };` |
| 2597 | field | Terraria.GameContent.VoidLensHelper | Terraria.GameContent/VoidLensHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\VoidLensHelper.cs | 13 | 2 | _position | Vector2 | `private readonly Vector2 _position;` | `private readonly Vector2 _position;` |
| 2598 | field | Terraria.GameContent.VoidLensHelper | Terraria.GameContent/VoidLensHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\VoidLensHelper.cs | 15 | 2 | _opacity | float | `private readonly float _opacity;` | `private readonly float _opacity;` |
| 2599 | field | Terraria.GameContent.VoidLensHelper | Terraria.GameContent/VoidLensHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\VoidLensHelper.cs | 17 | 2 | _frameNumber | int | `private readonly int _frameNumber;` | `private readonly int _frameNumber;` |

#### 属性（0）

无该类型成员记录。


### 4.11 细分子系统：`SharedAmbientSkyCatalogState`

- 原报告章节：`4.9.152`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`02` / `世界环境与事件`

- 上一级基线细分子系统：`SharedAmbientSkyAndWindState`
- 细分职责：树冠区域、天空变体和背景闪烁定义。
- 边界角色：`definition/presentation`；最小 seam：ambient sky catalog port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：2；声明类型数：3；字段：17；属性：0；合计：17。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（17）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2336 | field | Terraria.GameContent.BackgroundChangeFlashInfo | Terraria.GameContent/BackgroundChangeFlashInfo.cs | D:\TRbackup\Version4\Terraria.GameContent\BackgroundChangeFlashInfo.cs | 7 | 2 | _variations | int[] | `private int[] _variations = new int[TreeTopsInfo.AreaId.Count];` | `private int[] _variations = new int[TreeTopsInfo.AreaId.Count];` |
| 2337 | field | Terraria.GameContent.BackgroundChangeFlashInfo | Terraria.GameContent/BackgroundChangeFlashInfo.cs | D:\TRbackup\Version4\Terraria.GameContent\BackgroundChangeFlashInfo.cs | 9 | 2 | _flashPower | float[] | `private float[] _flashPower = new float[TreeTopsInfo.AreaId.Count];` | `private float[] _flashPower = new float[TreeTopsInfo.AreaId.Count];` |
| 2577 | field | Terraria.GameContent.TreeTopsInfo.AreaId | Terraria.GameContent/TreeTopsInfo.cs | D:\TRbackup\Version4\Terraria.GameContent\TreeTopsInfo.cs | 11 | 3 | Forest1 | int | `public const int Forest1 = 0;` | `public const int Forest1 = 0;` |
| 2578 | field | Terraria.GameContent.TreeTopsInfo.AreaId | Terraria.GameContent/TreeTopsInfo.cs | D:\TRbackup\Version4\Terraria.GameContent\TreeTopsInfo.cs | 13 | 3 | Forest2 | int | `public const int Forest2 = 1;` | `public const int Forest2 = 1;` |
| 2579 | field | Terraria.GameContent.TreeTopsInfo.AreaId | Terraria.GameContent/TreeTopsInfo.cs | D:\TRbackup\Version4\Terraria.GameContent\TreeTopsInfo.cs | 15 | 3 | Forest3 | int | `public const int Forest3 = 2;` | `public const int Forest3 = 2;` |
| 2580 | field | Terraria.GameContent.TreeTopsInfo.AreaId | Terraria.GameContent/TreeTopsInfo.cs | D:\TRbackup\Version4\Terraria.GameContent\TreeTopsInfo.cs | 17 | 3 | Forest4 | int | `public const int Forest4 = 3;` | `public const int Forest4 = 3;` |
| 2581 | field | Terraria.GameContent.TreeTopsInfo.AreaId | Terraria.GameContent/TreeTopsInfo.cs | D:\TRbackup\Version4\Terraria.GameContent\TreeTopsInfo.cs | 19 | 3 | Corruption | int | `public const int Corruption = 4;` | `public const int Corruption = 4;` |
| 2582 | field | Terraria.GameContent.TreeTopsInfo.AreaId | Terraria.GameContent/TreeTopsInfo.cs | D:\TRbackup\Version4\Terraria.GameContent\TreeTopsInfo.cs | 21 | 3 | Jungle | int | `public const int Jungle = 5;` | `public const int Jungle = 5;` |
| 2583 | field | Terraria.GameContent.TreeTopsInfo.AreaId | Terraria.GameContent/TreeTopsInfo.cs | D:\TRbackup\Version4\Terraria.GameContent\TreeTopsInfo.cs | 23 | 3 | Snow | int | `public const int Snow = 6;` | `public const int Snow = 6;` |
| 2584 | field | Terraria.GameContent.TreeTopsInfo.AreaId | Terraria.GameContent/TreeTopsInfo.cs | D:\TRbackup\Version4\Terraria.GameContent\TreeTopsInfo.cs | 25 | 3 | Hallow | int | `public const int Hallow = 7;` | `public const int Hallow = 7;` |
| 2585 | field | Terraria.GameContent.TreeTopsInfo.AreaId | Terraria.GameContent/TreeTopsInfo.cs | D:\TRbackup\Version4\Terraria.GameContent\TreeTopsInfo.cs | 27 | 3 | Crimson | int | `public const int Crimson = 8;` | `public const int Crimson = 8;` |
| 2586 | field | Terraria.GameContent.TreeTopsInfo.AreaId | Terraria.GameContent/TreeTopsInfo.cs | D:\TRbackup\Version4\Terraria.GameContent\TreeTopsInfo.cs | 29 | 3 | Desert | int | `public const int Desert = 9;` | `public const int Desert = 9;` |
| 2587 | field | Terraria.GameContent.TreeTopsInfo.AreaId | Terraria.GameContent/TreeTopsInfo.cs | D:\TRbackup\Version4\Terraria.GameContent\TreeTopsInfo.cs | 31 | 3 | Ocean | int | `public const int Ocean = 10;` | `public const int Ocean = 10;` |
| 2588 | field | Terraria.GameContent.TreeTopsInfo.AreaId | Terraria.GameContent/TreeTopsInfo.cs | D:\TRbackup\Version4\Terraria.GameContent\TreeTopsInfo.cs | 33 | 3 | GlowingMushroom | int | `public const int GlowingMushroom = 11;` | `public const int GlowingMushroom = 11;` |
| 2589 | field | Terraria.GameContent.TreeTopsInfo.AreaId | Terraria.GameContent/TreeTopsInfo.cs | D:\TRbackup\Version4\Terraria.GameContent\TreeTopsInfo.cs | 35 | 3 | Underworld | int | `public const int Underworld = 12;` | `public const int Underworld = 12;` |
| 2590 | field | Terraria.GameContent.TreeTopsInfo.AreaId | Terraria.GameContent/TreeTopsInfo.cs | D:\TRbackup\Version4\Terraria.GameContent\TreeTopsInfo.cs | 37 | 3 | Count | int | `public static readonly int Count = 13;` | `public static readonly int Count = 13;` |
| 2591 | field | Terraria.GameContent.TreeTopsInfo | Terraria.GameContent/TreeTopsInfo.cs | D:\TRbackup\Version4\Terraria.GameContent\TreeTopsInfo.cs | 40 | 2 | _variations | int[] | `private int[] _variations = new int[AreaId.Count];` | `private int[] _variations = new int[AreaId.Count];` |

#### 属性（0）

无该类型成员记录。


### 4.12 细分子系统：`SharedAmbientSpawnAndWindState`

- 原报告章节：`4.9.153`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`02` / `世界环境与事件`

- 上一级基线细分子系统：`SharedAmbientSkyAndWindState`
- 细分职责：环境实体生成条件、风点和尝试计时状态。
- 边界角色：`state/query`；最小 seam：ambient spawn wind port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：2；声明类型数：3；字段：11；属性：0；合计：11。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（11）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1349 | field | Terraria.GameContent.Ambience.AmbienceServer.AmbienceSpawnInfo | Terraria.GameContent.Ambience/AmbienceServer.cs | D:\TRbackup\Version4\Terraria.GameContent.Ambience\AmbienceServer.cs | 13 | 3 | skyEntityType | Terraria.GameContent.Ambience.SkyEntityType | `public SkyEntityType skyEntityType;` | `public SkyEntityType skyEntityType;` |
| 1350 | field | Terraria.GameContent.Ambience.AmbienceServer.AmbienceSpawnInfo | Terraria.GameContent.Ambience/AmbienceServer.cs | D:\TRbackup\Version4\Terraria.GameContent.Ambience\AmbienceServer.cs | 15 | 3 | targetPlayer | int | `public int targetPlayer;` | `public int targetPlayer;` |
| 1351 | field | Terraria.GameContent.Ambience.AmbienceServer | Terraria.GameContent.Ambience/AmbienceServer.cs | D:\TRbackup\Version4\Terraria.GameContent.Ambience\AmbienceServer.cs | 18 | 2 | MINIMUM_SECONDS_BETWEEN_SPAWNS | int | `private const int MINIMUM_SECONDS_BETWEEN_SPAWNS = 10;` | `private const int MINIMUM_SECONDS_BETWEEN_SPAWNS = 10;` |
| 1352 | field | Terraria.GameContent.Ambience.AmbienceServer | Terraria.GameContent.Ambience/AmbienceServer.cs | D:\TRbackup\Version4\Terraria.GameContent.Ambience\AmbienceServer.cs | 20 | 2 | MAXIMUM_SECONDS_BETWEEN_SPAWNS | int | `private const int MAXIMUM_SECONDS_BETWEEN_SPAWNS = 120;` | `private const int MAXIMUM_SECONDS_BETWEEN_SPAWNS = 120;` |
| 1353 | field | Terraria.GameContent.Ambience.AmbienceServer | Terraria.GameContent.Ambience/AmbienceServer.cs | D:\TRbackup\Version4\Terraria.GameContent.Ambience\AmbienceServer.cs | 22 | 2 | _spawnConditions | System.Collections.Generic.Dictionary<Terraria.GameContent.Ambience.SkyEntityType, System.Func<bool>> | `private readonly Dictionary<SkyEntityType, Func<bool>> _spawnConditions = new Dictionary<SkyEntityType, Func<bool>>();` | `private readonly Dictionary<SkyEntityType, Func<bool>> _spawnConditions = new Dictionary<SkyEntityType, Func<bool>>();` |
| 1354 | field | Terraria.GameContent.Ambience.AmbienceServer | Terraria.GameContent.Ambience/AmbienceServer.cs | D:\TRbackup\Version4\Terraria.GameContent.Ambience\AmbienceServer.cs | 24 | 2 | _secondarySpawnConditionsPerPlayer | System.Collections.Generic.Dictionary<Terraria.GameContent.Ambience.SkyEntityType, System.Func<Terraria.Player, bool>> | `private readonly Dictionary<SkyEntityType, Func<Player, bool>> _secondarySpawnConditionsPerPlayer = new Dictionary<SkyEntityType, Func<Player, bool>>();` | `private readonly Dictionary<SkyEntityType, Func<Player, bool>> _secondarySpawnConditionsPerPlayer = new Dictionary<SkyEntityType, Func<Player, bool>>();` |
| 1355 | field | Terraria.GameContent.Ambience.AmbienceServer | Terraria.GameContent.Ambience/AmbienceServer.cs | D:\TRbackup\Version4\Terraria.GameContent.Ambience\AmbienceServer.cs | 26 | 2 | _updatesUntilNextAttempt | int | `private int _updatesUntilNextAttempt;` | `private int _updatesUntilNextAttempt;` |
| 1356 | field | Terraria.GameContent.Ambience.AmbienceServer | Terraria.GameContent.Ambience/AmbienceServer.cs | D:\TRbackup\Version4\Terraria.GameContent.Ambience\AmbienceServer.cs | 28 | 2 | _forcedSpawns | System.Collections.Generic.List<Terraria.GameContent.Ambience.AmbienceServer.AmbienceSpawnInfo> | `private List<AmbienceSpawnInfo> _forcedSpawns = new List<AmbienceSpawnInfo>();` | `private List<AmbienceSpawnInfo> _forcedSpawns = new List<AmbienceSpawnInfo>();` |
| 2333 | field | Terraria.GameContent.AmbientWindSystem | Terraria.GameContent/AmbientWindSystem.cs | D:\TRbackup\Version4\Terraria.GameContent\AmbientWindSystem.cs | 9 | 2 | _random | Terraria.Utilities.UnifiedRandom | `private UnifiedRandom _random = new UnifiedRandom();` | `private UnifiedRandom _random = new UnifiedRandom();` |
| 2334 | field | Terraria.GameContent.AmbientWindSystem | Terraria.GameContent/AmbientWindSystem.cs | D:\TRbackup\Version4\Terraria.GameContent\AmbientWindSystem.cs | 11 | 2 | _spotsForAirboneWind | System.Collections.Generic.List<Point> | `private List<Point> _spotsForAirboneWind = new List<Point>();` | `private List<Point> _spotsForAirboneWind = new List<Point>();` |
| 2335 | field | Terraria.GameContent.AmbientWindSystem | Terraria.GameContent/AmbientWindSystem.cs | D:\TRbackup\Version4\Terraria.GameContent\AmbientWindSystem.cs | 13 | 2 | _updatesCounter | int | `private int _updatesCounter;` | `private int _updatesCounter;` |

#### 属性（0）

无该类型成员记录。


### 4.13 细分子系统：`SharedCelebrationAndLanternEvents`

- 原报告章节：`4.9.159`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`02` / `世界环境与事件`

- 上一级基线细分子系统：`SharedSeasonalWorldEventState`
- 细分职责：派对、灯笼夜和环境精灵季节事件状态。
- 边界角色：`state/definition`；最小 seam：celebration lantern event port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：3；声明类型数：3；字段：14；属性：2；合计：16。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（14）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1536 | field | Terraria.GameContent.Events.BirthdayParty | Terraria.GameContent.Events/BirthdayParty.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\BirthdayParty.cs | 13 | 2 | ManualParty | bool | `public static bool ManualParty;` | `public static bool ManualParty;` |
| 1537 | field | Terraria.GameContent.Events.BirthdayParty | Terraria.GameContent.Events/BirthdayParty.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\BirthdayParty.cs | 15 | 2 | GenuineParty | bool | `public static bool GenuineParty;` | `public static bool GenuineParty;` |
| 1538 | field | Terraria.GameContent.Events.BirthdayParty | Terraria.GameContent.Events/BirthdayParty.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\BirthdayParty.cs | 17 | 2 | PartyDaysOnCooldown | int | `public static int PartyDaysOnCooldown;` | `public static int PartyDaysOnCooldown;` |
| 1539 | field | Terraria.GameContent.Events.BirthdayParty | Terraria.GameContent.Events/BirthdayParty.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\BirthdayParty.cs | 19 | 2 | CelebratingNPCs | System.Collections.Generic.List<int> | `public static List<int> CelebratingNPCs = new List<int>();` | `public static List<int> CelebratingNPCs = new List<int>();` |
| 1540 | field | Terraria.GameContent.Events.BirthdayParty | Terraria.GameContent.Events/BirthdayParty.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\BirthdayParty.cs | 21 | 2 | _wasCelebrating | bool | `private static bool _wasCelebrating;` | `private static bool _wasCelebrating;` |
| 1570 | field | Terraria.GameContent.Events.LanternNight | Terraria.GameContent.Events/LanternNight.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\LanternNight.cs | 8 | 2 | ManualLanterns | bool | `public static bool ManualLanterns;` | `public static bool ManualLanterns;` |
| 1571 | field | Terraria.GameContent.Events.LanternNight | Terraria.GameContent.Events/LanternNight.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\LanternNight.cs | 10 | 2 | GenuineLanterns | bool | `public static bool GenuineLanterns;` | `public static bool GenuineLanterns;` |
| 1572 | field | Terraria.GameContent.Events.LanternNight | Terraria.GameContent.Events/LanternNight.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\LanternNight.cs | 12 | 2 | NextNightIsLanternNight | bool | `public static bool NextNightIsLanternNight;` | `public static bool NextNightIsLanternNight;` |
| 1573 | field | Terraria.GameContent.Events.LanternNight | Terraria.GameContent.Events/LanternNight.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\LanternNight.cs | 14 | 2 | LanternNightsOnCooldown | int | `public static int LanternNightsOnCooldown;` | `public static int LanternNightsOnCooldown;` |
| 1574 | field | Terraria.GameContent.Events.LanternNight | Terraria.GameContent.Events/LanternNight.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\LanternNight.cs | 16 | 2 | _wasLanternNight | bool | `private static bool _wasLanternNight;` | `private static bool _wasLanternNight;` |
| 1592 | field | Terraria.GameContent.Events.MysticLogFairiesEvent | Terraria.GameContent.Events/MysticLogFairiesEvent.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\MysticLogFairiesEvent.cs | 11 | 2 | _canSpawnFairies | bool | `private bool _canSpawnFairies;` | `private bool _canSpawnFairies;` |
| 1593 | field | Terraria.GameContent.Events.MysticLogFairiesEvent | Terraria.GameContent.Events/MysticLogFairiesEvent.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\MysticLogFairiesEvent.cs | 13 | 2 | _delayUntilNextAttempt | int | `private int _delayUntilNextAttempt;` | `private int _delayUntilNextAttempt;` |
| 1594 | field | Terraria.GameContent.Events.MysticLogFairiesEvent | Terraria.GameContent.Events/MysticLogFairiesEvent.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\MysticLogFairiesEvent.cs | 15 | 2 | DELAY_BETWEEN_ATTEMPTS | int | `private const int DELAY_BETWEEN_ATTEMPTS = 60;` | `private const int DELAY_BETWEEN_ATTEMPTS = 60;` |
| 1595 | field | Terraria.GameContent.Events.MysticLogFairiesEvent | Terraria.GameContent.Events/MysticLogFairiesEvent.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\MysticLogFairiesEvent.cs | 17 | 2 | _stumpCoords | System.Collections.Generic.List<Point> | `private List<Point> _stumpCoords = new List<Point>();` | `private List<Point> _stumpCoords = new List<Point>();` |

#### 属性（2）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3777 | property | Terraria.GameContent.Events.BirthdayParty | Terraria.GameContent.Events/BirthdayParty.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\BirthdayParty.cs | 23 | 2 | PartyIsUp | bool | `public static bool PartyIsUp { get { if (!GenuineParty) { return ManualParty; } return true; } }` | `public static bool PartyIsUp { get { if (!GenuineParty) { return ManualParty; } return true; } }` |
| 3783 | property | Terraria.GameContent.Events.LanternNight | Terraria.GameContent.Events/LanternNight.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\LanternNight.cs | 18 | 2 | LanternsUp | bool | `public static bool LanternsUp { get { if (!GenuineLanterns) { return ManualLanterns; } return true; } }` | `public static bool LanternsUp { get { if (!GenuineLanterns) { return ManualLanterns; } return true; } }` |


### 4.14 细分子系统：`SharedRitualAndStormEvents`

- 原报告章节：`4.9.160`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`02` / `世界环境与事件`

- 上一级基线细分子系统：`SharedSeasonalWorldEventState`
- 细分职责：邪教仪式和沙尘暴事件计时及强度状态。
- 边界角色：`state/definition`；最小 seam：ritual storm event port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：2；声明类型数：2；字段：12；属性：0；合计：12。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（12）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1543 | field | Terraria.GameContent.Events.CultistRitual | Terraria.GameContent.Events/CultistRitual.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\CultistRitual.cs | 9 | 2 | delayStart | int | `public const int delayStart = 86400;` | `public const int delayStart = 86400;` |
| 1544 | field | Terraria.GameContent.Events.CultistRitual | Terraria.GameContent.Events/CultistRitual.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\CultistRitual.cs | 11 | 2 | respawnDelay | int | `public const int respawnDelay = 43200;` | `public const int respawnDelay = 43200;` |
| 1545 | field | Terraria.GameContent.Events.CultistRitual | Terraria.GameContent.Events/CultistRitual.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\CultistRitual.cs | 13 | 2 | timePerCultist | int | `private const int timePerCultist = 3600;` | `private const int timePerCultist = 3600;` |
| 1546 | field | Terraria.GameContent.Events.CultistRitual | Terraria.GameContent.Events/CultistRitual.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\CultistRitual.cs | 15 | 2 | recheckStart | int | `private const int recheckStart = 600;` | `private const int recheckStart = 600;` |
| 1547 | field | Terraria.GameContent.Events.CultistRitual | Terraria.GameContent.Events/CultistRitual.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\CultistRitual.cs | 17 | 2 | delay | int | `public static int delay;` | `public static int delay;` |
| 1548 | field | Terraria.GameContent.Events.CultistRitual | Terraria.GameContent.Events/CultistRitual.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\CultistRitual.cs | 19 | 2 | recheck | int | `public static int recheck;` | `public static int recheck;` |
| 1596 | field | Terraria.GameContent.Events.Sandstorm | Terraria.GameContent.Events/Sandstorm.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\Sandstorm.cs | 10 | 2 | SANDSTORM_DURATION_MINIMUM | int | `private const int SANDSTORM_DURATION_MINIMUM = 28800;` | `private const int SANDSTORM_DURATION_MINIMUM = 28800;` |
| 1597 | field | Terraria.GameContent.Events.Sandstorm | Terraria.GameContent.Events/Sandstorm.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\Sandstorm.cs | 12 | 2 | SANDSTORM_DURATION_MAXIMUM | int | `private const int SANDSTORM_DURATION_MAXIMUM = 86400;` | `private const int SANDSTORM_DURATION_MAXIMUM = 86400;` |
| 1598 | field | Terraria.GameContent.Events.Sandstorm | Terraria.GameContent.Events/Sandstorm.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\Sandstorm.cs | 14 | 2 | Happening | bool | `public static bool Happening;` | `public static bool Happening;` |
| 1599 | field | Terraria.GameContent.Events.Sandstorm | Terraria.GameContent.Events/Sandstorm.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\Sandstorm.cs | 16 | 2 | TimeLeft | int | `public static int TimeLeft;` | `public static int TimeLeft;` |
| 1600 | field | Terraria.GameContent.Events.Sandstorm | Terraria.GameContent.Events/Sandstorm.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\Sandstorm.cs | 18 | 2 | Severity | float | `public static float Severity;` | `public static float Severity;` |
| 1601 | field | Terraria.GameContent.Events.Sandstorm | Terraria.GameContent.Events/Sandstorm.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\Sandstorm.cs | 20 | 2 | IntendedSeverity | float | `public static float IntendedSeverity;` | `public static float IntendedSeverity;` |

#### 属性（0）

无该类型成员记录。


### 4.15 细分子系统：`SharedSceneWeatherAndEventZoneState`

- 原报告章节：`4.9.217`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`02` / `世界环境与事件`

- 上一级基线细分子系统：`SharedSceneMetricsSnapshot`
- 上一级 peer 细分子系统：`SharedSceneBiomeAndEventDefinitionState`
- 细分职责：场景天气、微光、蜡烛和事件小游戏区域定义。
- 边界角色：`query/input`；最小 seam：scene weather event zone port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：9；属性：0；合计：9。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（9）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3459 | field | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 72 | 2 | ZoneRain | bool | `public bool ZoneRain;` | `public bool ZoneRain;` |
| 3460 | field | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 74 | 2 | ZoneSandstorm | bool | `public bool ZoneSandstorm;` | `public bool ZoneSandstorm;` |
| 3461 | field | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 76 | 2 | SurfaceAtmospherics | bool | `public bool SurfaceAtmospherics;` | `public bool SurfaceAtmospherics;` |
| 3462 | field | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 78 | 2 | UndergroundForShimmering | bool | `public bool UndergroundForShimmering;` | `public bool UndergroundForShimmering;` |
| 3463 | field | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 80 | 2 | ZoneShimmer | bool | `public bool ZoneShimmer;` | `public bool ZoneShimmer;` |
| 3464 | field | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 82 | 2 | ZoneWaterCandle | bool | `public bool ZoneWaterCandle;` | `public bool ZoneWaterCandle;` |
| 3465 | field | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 84 | 2 | ZonePeaceCandle | bool | `public bool ZonePeaceCandle;` | `public bool ZonePeaceCandle;` |
| 3466 | field | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 86 | 2 | ZoneShadowCandle | bool | `public bool ZoneShadowCandle;` | `public bool ZoneShadowCandle;` |
| 3467 | field | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 88 | 2 | InTorchGodMinigame | bool | `public bool InTorchGodMinigame;` | `public bool InTorchGodMinigame;` |

#### 属性（0）

无该类型成员记录。


### 4.16 细分子系统：`SharedInvasionDamageTrackingState`

- 原报告章节：`4.9.220`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`02` / `世界环境与事件`

- 上一级基线细分子系统：`SharedInvasionEventState`
- 上一级 peer 细分子系统：`SharedInvasionEventState`
- 细分职责：DD2 入侵伤害跟踪器及其胜利/击杀时间投影。
- 边界角色：`runtime state`；最小 seam：invasion damage tracking port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：2；字段：2；属性：2；合计：4。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（2）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1549 | field | Terraria.GameContent.Events.DD2Event.DamageTracker | Terraria.GameContent.Events/DD2Event.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\DD2Event.cs | 19 | 3 | _won | bool | `private bool _won;` | `private bool _won;` |
| 1561 | field | Terraria.GameContent.Events.DD2Event | Terraria.GameContent.Events/DD2Event.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\DD2Event.cs | 59 | 2 | _damageTracker | Terraria.GameContent.Events.DD2Event.DamageTracker | `private static DamageTracker _damageTracker;` | `private static DamageTracker _damageTracker;` |

#### 属性（2）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3778 | property | Terraria.GameContent.Events.DD2Event.DamageTracker | Terraria.GameContent.Events/DD2Event.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\DD2Event.cs | 21 | 3 | Name | Terraria.Localization.LocalizedText | `public override LocalizedText Name => Language.GetText("Bestiary_Invasions.OldOnesArmy");` | `public override LocalizedText Name => Language.GetText("Bestiary_Invasions.OldOnesArmy");` |
| 3779 | property | Terraria.GameContent.Events.DD2Event.DamageTracker | Terraria.GameContent.Events/DD2Event.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\DD2Event.cs | 23 | 3 | KillTimeMessage | Terraria.Localization.LocalizedText | `public override LocalizedText KillTimeMessage => Language.GetText(_won ? "BossDamageCommand.KillTimeDefeated" : "BossDamageCommand.KillTimeLost");` | `public override LocalizedText KillTimeMessage => Language.GetText(_won ? "BossDamageCommand.KillTimeDefeated" : "BossDamageCommand.KillTimeLost");` |


### 4.17 细分子系统：`SharedInvasionWaveAndArenaState`

- 原报告章节：`4.9.221`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`02` / `世界环境与事件`

- 上一级基线细分子系统：`SharedInvasionEventState`
- 上一级 peer 细分子系统：`SharedInvasionEventState`
- 细分职责：DD2 入侵波次、竞技场、生成暂停和掉落进度状态。
- 边界角色：`runtime state`；最小 seam：invasion wave arena port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：19；属性：3；合计：22。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（19）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1550 | field | Terraria.GameContent.Events.DD2Event | Terraria.GameContent.Events/DD2Event.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\DD2Event.cs | 37 | 2 | INFO_NEW_WAVE_COLOR | Color | `private static readonly Color INFO_NEW_WAVE_COLOR = new Color(175, 55, 255);` | `private static readonly Color INFO_NEW_WAVE_COLOR = new Color(175, 55, 255);` |
| 1551 | field | Terraria.GameContent.Events.DD2Event | Terraria.GameContent.Events/DD2Event.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\DD2Event.cs | 39 | 2 | INFO_START_INVASION_COLOR | Color | `private static readonly Color INFO_START_INVASION_COLOR = ChatColors.World;` | `private static readonly Color INFO_START_INVASION_COLOR = ChatColors.World;` |
| 1552 | field | Terraria.GameContent.Events.DD2Event | Terraria.GameContent.Events/DD2Event.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\DD2Event.cs | 41 | 2 | INFO_FAILURE_INVASION_COLOR | Color | `private static readonly Color INFO_FAILURE_INVASION_COLOR = new Color(255, 0, 0);` | `private static readonly Color INFO_FAILURE_INVASION_COLOR = new Color(255, 0, 0);` |
| 1553 | field | Terraria.GameContent.Events.DD2Event | Terraria.GameContent.Events/DD2Event.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\DD2Event.cs | 43 | 2 | INVASION_ID | int | `private const int INVASION_ID = 3;` | `private const int INVASION_ID = 3;` |
| 1554 | field | Terraria.GameContent.Events.DD2Event | Terraria.GameContent.Events/DD2Event.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\DD2Event.cs | 45 | 2 | DownedInvasionT1 | bool | `public static bool DownedInvasionT1;` | `public static bool DownedInvasionT1;` |
| 1555 | field | Terraria.GameContent.Events.DD2Event | Terraria.GameContent.Events/DD2Event.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\DD2Event.cs | 47 | 2 | DownedInvasionT2 | bool | `public static bool DownedInvasionT2;` | `public static bool DownedInvasionT2;` |
| 1556 | field | Terraria.GameContent.Events.DD2Event | Terraria.GameContent.Events/DD2Event.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\DD2Event.cs | 49 | 2 | DownedInvasionT3 | bool | `public static bool DownedInvasionT3;` | `public static bool DownedInvasionT3;` |
| 1557 | field | Terraria.GameContent.Events.DD2Event | Terraria.GameContent.Events/DD2Event.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\DD2Event.cs | 51 | 2 | LostThisRun | bool | `public static bool LostThisRun;` | `public static bool LostThisRun;` |
| 1558 | field | Terraria.GameContent.Events.DD2Event | Terraria.GameContent.Events/DD2Event.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\DD2Event.cs | 53 | 2 | WonThisRun | bool | `public static bool WonThisRun;` | `public static bool WonThisRun;` |
| 1559 | field | Terraria.GameContent.Events.DD2Event | Terraria.GameContent.Events/DD2Event.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\DD2Event.cs | 55 | 2 | LaneSpawnRate | int | `public static int LaneSpawnRate = 60;` | `public static int LaneSpawnRate = 60;` |
| 1560 | field | Terraria.GameContent.Events.DD2Event | Terraria.GameContent.Events/DD2Event.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\DD2Event.cs | 57 | 2 | Ongoing | bool | `public static bool Ongoing;` | `public static bool Ongoing;` |
| 1562 | field | Terraria.GameContent.Events.DD2Event | Terraria.GameContent.Events/DD2Event.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\DD2Event.cs | 61 | 2 | ArenaHitbox | Rectangle | `public static Rectangle ArenaHitbox;` | `public static Rectangle ArenaHitbox;` |
| 1563 | field | Terraria.GameContent.Events.DD2Event | Terraria.GameContent.Events/DD2Event.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\DD2Event.cs | 63 | 2 | _arenaHitboxingCooldown | int | `private static int _arenaHitboxingCooldown;` | `private static int _arenaHitboxingCooldown;` |
| 1564 | field | Terraria.GameContent.Events.DD2Event | Terraria.GameContent.Events/DD2Event.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\DD2Event.cs | 65 | 2 | OngoingDifficulty | int | `public static int OngoingDifficulty;` | `public static int OngoingDifficulty;` |
| 1565 | field | Terraria.GameContent.Events.DD2Event | Terraria.GameContent.Events/DD2Event.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\DD2Event.cs | 67 | 2 | _deadGoblinSpots | System.Collections.Generic.List<Vector2> | `private static List<Vector2> _deadGoblinSpots = new List<Vector2>();` | `private static List<Vector2> _deadGoblinSpots = new List<Vector2>();` |
| 1566 | field | Terraria.GameContent.Events.DD2Event | Terraria.GameContent.Events/DD2Event.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\DD2Event.cs | 69 | 2 | _crystalsDropping_lastWave | int | `private static int _crystalsDropping_lastWave;` | `private static int _crystalsDropping_lastWave;` |
| 1567 | field | Terraria.GameContent.Events.DD2Event | Terraria.GameContent.Events/DD2Event.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\DD2Event.cs | 71 | 2 | _crystalsDropping_toDrop | int | `private static int _crystalsDropping_toDrop;` | `private static int _crystalsDropping_toDrop;` |
| 1568 | field | Terraria.GameContent.Events.DD2Event | Terraria.GameContent.Events/DD2Event.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\DD2Event.cs | 73 | 2 | _crystalsDropping_alreadyDropped | int | `private static int _crystalsDropping_alreadyDropped;` | `private static int _crystalsDropping_alreadyDropped;` |
| 1569 | field | Terraria.GameContent.Events.DD2Event | Terraria.GameContent.Events/DD2Event.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\DD2Event.cs | 75 | 2 | _timeLeftUntilSpawningBegins | int | `private static int _timeLeftUntilSpawningBegins;` | `private static int _timeLeftUntilSpawningBegins;` |

#### 属性（3）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3780 | property | Terraria.GameContent.Events.DD2Event | Terraria.GameContent.Events/DD2Event.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\DD2Event.cs | 77 | 2 | ReadyToFindBartender | bool | `public static bool ReadyToFindBartender => NPC.downedBoss2;` | `public static bool ReadyToFindBartender => NPC.downedBoss2;` |
| 3781 | property | Terraria.GameContent.Events.DD2Event | Terraria.GameContent.Events/DD2Event.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\DD2Event.cs | 79 | 2 | TimeLeftBetweenWaves | int | `public static int TimeLeftBetweenWaves { get { return _timeLeftUntilSpawningBegins; } set { _timeLeftUntilSpawningBegins = value; } }` | `public static int TimeLeftBetweenWaves { get { return _timeLeftUntilSpawningBegins; } set { _timeLeftUntilSpawningBegins = value; } }` |
| 3782 | property | Terraria.GameContent.Events.DD2Event | Terraria.GameContent.Events/DD2Event.cs | D:\TRbackup\Version4\Terraria.GameContent.Events\DD2Event.cs | 91 | 2 | EnemySpawningIsOnHold | bool | `public static bool EnemySpawningIsOnHold => _timeLeftUntilSpawningBegins != 0;` | `public static bool EnemySpawningIsOnHold => _timeLeftUntilSpawningBegins != 0;` |


## 5. 追溯与验收

> 来源报告 SHA-256：`b944441d92d5103f218b766a3e354200794528a19e65a862e44c2b40a36f1196`
- 本分区细分组数：17；成员数：225；字段：212；属性：13。
- 预期不变量：本文件只出现完整细分子系统；不跨分区复制成员；所有成员的来源序号、原始声明、路径、行列和 C# 类型必须与来源报告一致。
- 验证方式：重新读取 20 个文件，检查文件数、细分组唯一性、来源序号恰好覆盖 `1..4542`、字段/属性合计和逐成员行文本一致；本次分区生成不运行 `dotnet`，因为没有修改 C# 或项目文件。

生成器：`Build/Tools/Generate-Version4NonAuthoritativeComponentPartitionReports.ps1`。
