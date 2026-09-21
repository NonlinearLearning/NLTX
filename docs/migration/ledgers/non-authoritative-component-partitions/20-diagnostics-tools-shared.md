# Version4 非权威组件拆分分区 20/20：诊断、工具与共享机制

> 来源报告：`docs/migration/ledgers/Version4非权威模拟系统字段属性逐成员源码声明-更细子系统拆分-去除ID类文件.md`
> 来源报告 SHA-256：`b944441d92d5103f218b766a3e354200794528a19e65a862e44c2b40a36f1196`
> 本文件是并行组件拆分工作包，不是新的权威成员清单；成员声明事实直接保留来源报告原文。

## 1. 分区定位与评估

- 分区范围：时间序列、TimeLogger、Debug、随机/缓冲工具、Creative Power、问题报告和通用元数据。
- 本分区组件化重点：确认工具状态是否为投影/诊断快照，隔离时钟、随机、日志和其它外部副作用。
- 本分区包含 22 个完整细分子系统、250 条成员记录（字段 226、属性 24）。来源序号覆盖区间 `116..4024`，区间可能与其他分区交错，不以序号定义领域边界。
- 组件边界判定：细分子系统只是并行处理的最小库存边界，不等价于一个组件。必须继续按共同读者/写者、变更原因、生命周期、权威性和副作用分为 Component、System、Query、Command、Adapter 或 Projection。
- 证据状态：成员声明和统计已由来源报告校验；逐成员读者、写者、初始化/清理路径、持久化/网络语义和系统顺序仍需本分区会话补证，不得从名称或数量推断。

## 2. 分区统计

| 父级子系统 | 细分子系统数 | 字段 | 属性 | 合计 |
|---|---:|---:|---:|---:|
| `RuntimeComposition` | 2 | 11 | 0 | 11 |
| `SharedRuntimeMechanisms` | 20 | 215 | 24 | 239 |

| 原报告章节 | 父级 | 细分子系统 | 边界角色 | 字段 | 属性 | 合计 | 组件化处理 |
|---|---|---|---|---:|---:|---:|---|
| `4.1.9` | `RuntimeComposition` | `MainDiagnosticsAndSimulationRates` | runtime state | 7 | 0 | 7 | 待按成员访问模式拆分 |
| `4.1.39` | `RuntimeComposition` | `MainTickAndDiagnosticState` | runtime state | 4 | 0 | 4 | 待按成员访问模式拆分 |
| `4.9.12` | `SharedRuntimeMechanisms` | `SharedCallTrackingDiagnostics` | diagnostics | 3 | 0 | 3 | 待按成员访问模式拆分 |
| `4.9.13` | `SharedRuntimeMechanisms` | `SharedTimeSeriesDataSeriesState` | diagnostics state | 10 | 0 | 10 | 待按成员访问模式拆分 |
| `4.9.14` | `SharedRuntimeMechanisms` | `SharedTimeSeriesEntryState` | diagnostics state | 5 | 0 | 5 | 待按成员访问模式拆分 |
| `4.9.15` | `SharedRuntimeMechanisms` | `SharedTimeSeriesFormattingState` | diagnostics/query | 5 | 0 | 5 | 待按成员访问模式拆分 |
| `4.9.54` | `SharedRuntimeMechanisms` | `SharedRandomSources` | value object | 9 | 1 | 10 | 待按成员访问模式拆分 |
| `4.9.55` | `SharedRuntimeMechanisms` | `SharedBufferAndCollectionPools` | value object | 23 | 2 | 25 | 待按成员访问模式拆分 |
| `4.9.56` | `SharedRuntimeMechanisms` | `SharedRangeAndBitUtilities` | value object | 10 | 5 | 15 | 待按成员访问模式拆分 |
| `4.9.84` | `SharedRuntimeMechanisms` | `SharedGeneralDiagnosticsUtilities` | diagnostics | 0 | 5 | 5 | 待按成员访问模式拆分 |
| `4.9.85` | `SharedRuntimeMechanisms` | `SharedGeneralDelegateAndMetadataUtilities` | value object/metadata | 10 | 1 | 11 | 待按成员访问模式拆分 |
| `4.9.86` | `SharedRuntimeMechanisms` | `DebugCommandProtocol` | diagnostics/adapter | 12 | 8 | 20 | 待按成员访问模式拆分 |
| `4.9.87` | `SharedRuntimeMechanisms` | `DebugRuntimeOptions` | diagnostics/state | 10 | 0 | 10 | 待按成员访问模式拆分 |
| `4.9.88` | `SharedRuntimeMechanisms` | `DebugFrameTelemetry` | diagnostics/state | 16 | 0 | 16 | 待按成员访问模式拆分 |
| `4.9.89` | `SharedRuntimeMechanisms` | `DebugBuildStatus` | diagnostics/adapter | 1 | 1 | 2 | 待按成员访问模式拆分 |
| `4.9.113` | `SharedRuntimeMechanisms` | `SharedTimeLoggerFrameCoordinationState` | diagnostics state | 15 | 1 | 16 | 待按成员访问模式拆分 |
| `4.9.158` | `SharedRuntimeMechanisms` | `SharedGeneralRandomAndBufferUtilities` | query/value object | 8 | 0 | 8 | 待按成员访问模式拆分 |
| `4.9.167` | `SharedRuntimeMechanisms` | `SharedTimeLoggerDisplayFormattingState` | diagnostics/query | 9 | 0 | 9 | 待按成员访问模式拆分 |
| `4.9.198` | `SharedRuntimeMechanisms` | `SharedTimeLoggerEntityAndInterfacePhaseMetricsState` | diagnostics state | 20 | 0 | 20 | 待按成员访问模式拆分 |
| `4.9.204` | `SharedRuntimeMechanisms` | `SharedTimeLoggerTileAndLiquidRenderMetricsState` | diagnostics state | 24 | 0 | 24 | 待按成员访问模式拆分 |
| `4.9.205` | `SharedRuntimeMechanisms` | `SharedTimeLoggerLightingMapAndBackgroundMetricsState` | diagnostics state | 22 | 0 | 22 | 待按成员访问模式拆分 |
| `4.9.222` | `SharedRuntimeMechanisms` | `SharedIssueReportCatalogState` | diagnostics/adapter | 3 | 0 | 3 | 待按成员访问模式拆分 |

## 3. 并行组件拆分契约

对本文件中的每个细分子系统和每条成员记录执行以下判断，并把结果写入对应的实现阶段设计/审计记录：

- 盘点声明类型、可见性、读者、写者、创建/更新/清理/持久化生命周期、访问频率和副作用；缺证据标记 `missing`，不可用命名猜测补齐。
- 标记权威状态、派生值、缓存、快照、兼容字段和表现数据；Component 只保存一个内聚概念的权威数据。
- 需要行为转换时放入显式 System/Command；纯资格判断和派生计算放入 Query；外部协议、文件、平台和第三方类型放入 Adapter；网络/持久化/日志/UI 输出放入单向 Projection。
- 分别建模实体引用、持久化 ID、网络 ID 和外部 ID；不得把 ID 类文件或外部对象直接塞入共享组件。
- 记录 Interface、Implementation、Seam、Depth、Leverage、Locality，以及必要的系统先后约束、失败归属和重试策略。

## 4. 逐成员源码声明

以下细分子系统块保持来源报告的成员表、声明、路径、行列和来源序号；只调整 Markdown 标题层级并增加分区追踪元数据。

### 4.1 细分子系统：`MainDiagnosticsAndSimulationRates`

- 原报告章节：`4.1.9`
- 父级子系统：`RuntimeComposition`
- 分区工作包：`20` / `诊断、工具与共享机制`

- 上一级基线细分子系统：`MainDiagnosticsAndSimulationRates`
- 细分职责：网络诊断、启动显示、错误策略和模拟速率配置。
- 边界角色：`runtime state`；最小 seam：diagnostics/rate configuration；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：7；属性：0；合计：7。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（7）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 116 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 392 | 2 | _activeNetDiagnosticsUI | Terraria.UI.INetDiagnosticsUI | `private static INetDiagnosticsUI _activeNetDiagnosticsUI;` | `private static INetDiagnosticsUI _activeNetDiagnosticsUI;` |
| 117 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 394 | 2 | fpsTimer | System.Diagnostics.Stopwatch | `public static Stopwatch fpsTimer = new Stopwatch();` | `public static Stopwatch fpsTimer = new Stopwatch();` |
| 118 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 396 | 2 | showSplash | bool | `public static bool showSplash = true;` | `public static bool showSplash = true;` |
| 119 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 398 | 2 | ignoreErrors | bool | `public static bool ignoreErrors = true;` | `public static bool ignoreErrors = true;` |
| 120 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 400 | 2 | defaultIP | string | `public static string defaultIP = "";` | `public static string defaultIP = "";` |
| 121 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 402 | 2 | dayRate | int | `public static int dayRate = 1;` | `public static int dayRate = 1;` |
| 122 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 404 | 2 | desiredWorldTilesUpdateRate | int | `public static int desiredWorldTilesUpdateRate = 1;` | `public static int desiredWorldTilesUpdateRate = 1;` |

#### 属性（0）

无该类型成员记录。


### 4.2 细分子系统：`MainTickAndDiagnosticState`

- 原报告章节：`4.1.39`
- 父级子系统：`RuntimeComposition`
- 分区工作包：`20` / `诊断、工具与共享机制`

- 上一级基线细分子系统：`MainTickAndDiagnosticState`
- 细分职责：投射物循环索引、帧目标、更新计时和辅助服务。
- 边界角色：`runtime state`；最小 seam：tick/diagnostic view；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：4；属性：0；合计：4。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（4）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 493 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1256 | 2 | ProjectileUpdateLoopIndex | int | `public static int ProjectileUpdateLoopIndex = -1;` | `public static int ProjectileUpdateLoopIndex = -1;` |
| 494 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1258 | 2 | TARGET_FRAME_TIME | double | `public static readonly double TARGET_FRAME_TIME = 0.01666666753590107;` | `public static readonly double TARGET_FRAME_TIME = 0.01666666753590107;` |
| 495 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1260 | 2 | _worldUpdateTimeTester | System.Diagnostics.Stopwatch | `private Stopwatch _worldUpdateTimeTester = new Stopwatch();` | `private Stopwatch _worldUpdateTimeTester = new Stopwatch();` |
| 496 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1262 | 2 | SpelunkerProjectileHelper | Terraria.GameContent.SpelunkerProjectileHelper | `public SpelunkerProjectileHelper SpelunkerProjectileHelper = new SpelunkerProjectileHelper();` | `public SpelunkerProjectileHelper SpelunkerProjectileHelper = new SpelunkerProjectileHelper();` |

#### 属性（0）

无该类型成员记录。


### 4.3 细分子系统：`SharedCallTrackingDiagnostics`

- 原报告章节：`4.9.12`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`20` / `诊断、工具与共享机制`

- 上一级基线细分子系统：`SharedCallTrackingDiagnostics`
- 细分职责：调用队列、已记录方法和刷新计时。
- 边界角色：`diagnostics`；最小 seam：call tracking sink；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：3；属性：0；合计：3。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（3）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1010 | field | Terraria.CallTracker | CallTracker.cs | D:\TRbackup\Version4\CallTracker.cs | 11 | 3 | LogQueue | System.Collections.Concurrent.ConcurrentQueue<string> | `private static readonly ConcurrentQueue<string> LogQueue = new();` | `private static readonly ConcurrentQueue<string> LogQueue = new();` |
| 1011 | field | Terraria.CallTracker | CallTracker.cs | D:\TRbackup\Version4\CallTracker.cs | 12 | 3 | LoggedMethods | System.Collections.Concurrent.ConcurrentDictionary<string, bool> | `private static readonly ConcurrentDictionary<string, bool> LoggedMethods = new();` | `private static readonly ConcurrentDictionary<string, bool> LoggedMethods = new();` |
| 1012 | field | Terraria.CallTracker | CallTracker.cs | D:\TRbackup\Version4\CallTracker.cs | 13 | 3 | FlushTimer | System.Threading.Timer | `private static readonly Timer FlushTimer = new(FlushLogs, null, 5000, 5000);` | `private static readonly Timer FlushTimer = new(FlushLogs, null, 5000, 5000);` |

#### 属性（0）

无该类型成员记录。


### 4.4 细分子系统：`SharedTimeSeriesDataSeriesState`

- 原报告章节：`4.9.13`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`20` / `诊断、工具与共享机制`

- 上一级基线细分子系统：`SharedTimeSeriesDataSeriesState`
- 细分职责：帧窗口、分位数、最大值和时间序列聚合状态。
- 边界角色：`diagnostics state`；最小 seam：data-series aggregation port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：10；属性：0；合计：10。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（10）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3517 | field | Terraria.TimeLogger.DataSeries | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 20 | 3 | values | int[] | `private int[] values = new int[FrameCount];` | `private int[] values = new int[FrameCount];` |
| 3518 | field | Terraria.TimeLogger.DataSeries | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 22 | 3 | used | bool[] | `private bool[] used = new bool[FrameCount];` | `private bool[] used = new bool[FrameCount];` |
| 3519 | field | Terraria.TimeLogger.DataSeries | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 24 | 3 | next | int | `private int next;` | `private int next;` |
| 3520 | field | Terraria.TimeLogger.DataSeries | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 26 | 3 | count | int | `private int count;` | `private int count;` |
| 3521 | field | Terraria.TimeLogger.DataSeries | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 28 | 3 | usedCount | int | `private int usedCount;` | `private int usedCount;` |
| 3522 | field | Terraria.TimeLogger.DataSeries | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 30 | 3 | previous | int | `public int previous;` | `public int previous;` |
| 3523 | field | Terraria.TimeLogger.DataSeries | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 32 | 3 | median | int | `public int median;` | `public int median;` |
| 3524 | field | Terraria.TimeLogger.DataSeries | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 34 | 3 | p90 | int | `public int p90;` | `public int p90;` |
| 3525 | field | Terraria.TimeLogger.DataSeries | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 36 | 3 | max | int | `public int max;` | `public int max;` |
| 3526 | field | Terraria.TimeLogger.DataSeries | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 38 | 3 | _sort | int[] | `private static int[] _sort = new int[FrameCount];` | `private static int[] _sort = new int[FrameCount];` |

#### 属性（0）

无该类型成员记录。


### 4.5 细分子系统：`SharedTimeSeriesEntryState`

- 原报告章节：`4.9.14`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`20` / `诊断、工具与共享机制`

- 上一级基线细分子系统：`SharedTimeSeriesEntryState`
- 细分职责：单个计时条目、预算、格式化委托和双缓冲序列。
- 边界角色：`diagnostics state`；最小 seam：time-log entry port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：5；属性：0；合计：5。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（5）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3527 | field | Terraria.TimeLogger.TimeLogData | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 104 | 3 | name | string | `public string name;` | `public string name;` |
| 3528 | field | Terraria.TimeLogger.TimeLogData | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 106 | 3 | format | System.Func<int, string> | `public Func<int, string> format;` | `public Func<int, string> format;` |
| 3529 | field | Terraria.TimeLogger.TimeLogData | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 108 | 3 | budget | int | `public int budget;` | `public int budget;` |
| 3530 | field | Terraria.TimeLogger.TimeLogData | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 110 | 3 | pendingDisplay | bool | `public bool pendingDisplay;` | `public bool pendingDisplay;` |
| 3531 | field | Terraria.TimeLogger.TimeLogData | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 112 | 3 | data | Terraria.TimeLogger.DataSeries[] | `public DataSeries[] data = new DataSeries[2]  		{  			new DataSeries(),  			new DataSeries()  		};` | `public DataSeries[] data = new DataSeries[2] { new DataSeries(), new DataSeries() };` |

#### 属性（0）

无该类型成员记录。


### 4.6 细分子系统：`SharedTimeSeriesFormattingState`

- 原报告章节：`4.9.15`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`20` / `诊断、工具与共享机制`

- 上一级基线细分子系统：`SharedTimeSeriesFormattingState`
- 细分职责：性能计时和 CPU 显示格式缓存。
- 边界角色：`diagnostics/query`；最小 seam：diagnostic formatting cache；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：5；属性：0；合计：5。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（5）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3532 | field | Terraria.TimeLogger.FormatPool | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 146 | 3 | _format | string | `private readonly string _format;` | `private readonly string _format;` |
| 3533 | field | Terraria.TimeLogger.FormatPool | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 148 | 3 | _minValue | double | `private readonly double _minValue;` | `private readonly double _minValue;` |
| 3534 | field | Terraria.TimeLogger.FormatPool | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 150 | 3 | _rounding | double | `private readonly double _rounding;` | `private readonly double _rounding;` |
| 3535 | field | Terraria.TimeLogger.FormatPool | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 152 | 3 | _strings | string[] | `private readonly string[] _strings;` | `private readonly string[] _strings;` |
| 3536 | field | Terraria.TimeLogger.FormatPool | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 154 | 3 | _nullString | string | `private string _nullString;` | `private string _nullString;` |

#### 属性（0）

无该类型成员记录。


### 4.7 细分子系统：`SharedRandomSources`

- 原报告章节：`4.9.54`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`20` / `诊断、工具与共享机制`

- 上一级基线细分子系统：`SharedRandomSources`
- 细分职责：伪随机源和随机流实现。
- 边界角色：`value object`；最小 seam：random source port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：3；声明类型数：3；字段：9；属性：1；合计：10。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（9）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2957 | field | Terraria.Utilities.FastRandom | Terraria.Utilities/FastRandom.cs | D:\TRbackup\Version4\Terraria.Utilities\FastRandom.cs | 7 | 2 | RANDOM_MULTIPLIER | ulong | `private const ulong RANDOM_MULTIPLIER = 25214903917uL;` | `private const ulong RANDOM_MULTIPLIER = 25214903917uL;` |
| 2958 | field | Terraria.Utilities.FastRandom | Terraria.Utilities/FastRandom.cs | D:\TRbackup\Version4\Terraria.Utilities\FastRandom.cs | 9 | 2 | RANDOM_ADD | ulong | `private const ulong RANDOM_ADD = 11uL;` | `private const ulong RANDOM_ADD = 11uL;` |
| 2959 | field | Terraria.Utilities.FastRandom | Terraria.Utilities/FastRandom.cs | D:\TRbackup\Version4\Terraria.Utilities\FastRandom.cs | 11 | 2 | RANDOM_MASK | ulong | `private const ulong RANDOM_MASK = 281474976710655uL;` | `private const ulong RANDOM_MASK = 281474976710655uL;` |
| 2963 | field | Terraria.Utilities.LCG32Random | Terraria.Utilities/LCG32Random.cs | D:\TRbackup\Version4\Terraria.Utilities\LCG32Random.cs | 7 | 2 | state | uint | `public uint state;` | `public uint state;` |
| 2985 | field | Terraria.Utilities.UnifiedRandom | Terraria.Utilities/UnifiedRandom.cs | D:\TRbackup\Version4\Terraria.Utilities\UnifiedRandom.cs | 8 | 2 | MBIG | int | `private const int MBIG = int.MaxValue;` | `private const int MBIG = int.MaxValue;` |
| 2986 | field | Terraria.Utilities.UnifiedRandom | Terraria.Utilities/UnifiedRandom.cs | D:\TRbackup\Version4\Terraria.Utilities\UnifiedRandom.cs | 10 | 2 | MSEED | int | `private const int MSEED = 161803398;` | `private const int MSEED = 161803398;` |
| 2987 | field | Terraria.Utilities.UnifiedRandom | Terraria.Utilities/UnifiedRandom.cs | D:\TRbackup\Version4\Terraria.Utilities\UnifiedRandom.cs | 12 | 2 | MZ | int | `private const int MZ = 0;` | `private const int MZ = 0;` |
| 2988 | field | Terraria.Utilities.UnifiedRandom | Terraria.Utilities/UnifiedRandom.cs | D:\TRbackup\Version4\Terraria.Utilities\UnifiedRandom.cs | 14 | 2 | inext | uint | `private uint inext;` | `private uint inext;` |
| 2989 | field | Terraria.Utilities.UnifiedRandom | Terraria.Utilities/UnifiedRandom.cs | D:\TRbackup\Version4\Terraria.Utilities\UnifiedRandom.cs | 16 | 2 | SeedArray | int[] | `private int[] SeedArray = new int[56];` | `private int[] SeedArray = new int[56];` |

#### 属性（1）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3939 | property | Terraria.Utilities.FastRandom | Terraria.Utilities/FastRandom.cs | D:\TRbackup\Version4\Terraria.Utilities\FastRandom.cs | 13 | 2 | Seed | ulong | `public ulong Seed { get; private set; }` | `public ulong Seed { get; private set; }` |


### 4.8 细分子系统：`SharedBufferAndCollectionPools`

- 原报告章节：`4.9.55`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`20` / `诊断、工具与共享机制`

- 上一级基线细分子系统：`SharedBufferAndCollectionPools`
- 细分职责：缓冲池、缓存缓冲和集合排序容器。
- 边界角色：`value object`；最小 seam：buffer pool port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：4；声明类型数：4；字段：23；属性：2；合计：25。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（23）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1069 | field | Terraria.DataStructures.BufferPool | Terraria.DataStructures/BufferPool.cs | D:\TRbackup\Version4\Terraria.DataStructures\BufferPool.cs | 8 | 2 | SMALL_BUFFER_SIZE | int | `private const int SMALL_BUFFER_SIZE = 32;` | `private const int SMALL_BUFFER_SIZE = 32;` |
| 1070 | field | Terraria.DataStructures.BufferPool | Terraria.DataStructures/BufferPool.cs | D:\TRbackup\Version4\Terraria.DataStructures\BufferPool.cs | 10 | 2 | MEDIUM_BUFFER_SIZE | int | `private const int MEDIUM_BUFFER_SIZE = 256;` | `private const int MEDIUM_BUFFER_SIZE = 256;` |
| 1071 | field | Terraria.DataStructures.BufferPool | Terraria.DataStructures/BufferPool.cs | D:\TRbackup\Version4\Terraria.DataStructures\BufferPool.cs | 12 | 2 | LARGE_BUFFER_SIZE | int | `private const int LARGE_BUFFER_SIZE = 16384;` | `private const int LARGE_BUFFER_SIZE = 16384;` |
| 1072 | field | Terraria.DataStructures.BufferPool | Terraria.DataStructures/BufferPool.cs | D:\TRbackup\Version4\Terraria.DataStructures\BufferPool.cs | 14 | 2 | HUGE_BUFFER_SIZE | int | `private const int HUGE_BUFFER_SIZE = 65536;` | `private const int HUGE_BUFFER_SIZE = 65536;` |
| 1073 | field | Terraria.DataStructures.BufferPool | Terraria.DataStructures/BufferPool.cs | D:\TRbackup\Version4\Terraria.DataStructures\BufferPool.cs | 16 | 2 | bufferLock | object | `private static object bufferLock = new object();` | `private static object bufferLock = new object();` |
| 1074 | field | Terraria.DataStructures.BufferPool | Terraria.DataStructures/BufferPool.cs | D:\TRbackup\Version4\Terraria.DataStructures\BufferPool.cs | 18 | 2 | SmallBufferQueue | System.Collections.Generic.Queue<Terraria.DataStructures.CachedBuffer> | `private static Queue<CachedBuffer> SmallBufferQueue = new Queue<CachedBuffer>();` | `private static Queue<CachedBuffer> SmallBufferQueue = new Queue<CachedBuffer>();` |
| 1075 | field | Terraria.DataStructures.BufferPool | Terraria.DataStructures/BufferPool.cs | D:\TRbackup\Version4\Terraria.DataStructures\BufferPool.cs | 20 | 2 | MediumBufferQueue | System.Collections.Generic.Queue<Terraria.DataStructures.CachedBuffer> | `private static Queue<CachedBuffer> MediumBufferQueue = new Queue<CachedBuffer>();` | `private static Queue<CachedBuffer> MediumBufferQueue = new Queue<CachedBuffer>();` |
| 1076 | field | Terraria.DataStructures.BufferPool | Terraria.DataStructures/BufferPool.cs | D:\TRbackup\Version4\Terraria.DataStructures\BufferPool.cs | 22 | 2 | LargeBufferQueue | System.Collections.Generic.Queue<Terraria.DataStructures.CachedBuffer> | `private static Queue<CachedBuffer> LargeBufferQueue = new Queue<CachedBuffer>();` | `private static Queue<CachedBuffer> LargeBufferQueue = new Queue<CachedBuffer>();` |
| 1077 | field | Terraria.DataStructures.BufferPool | Terraria.DataStructures/BufferPool.cs | D:\TRbackup\Version4\Terraria.DataStructures\BufferPool.cs | 24 | 2 | HugeBufferQueue | System.Collections.Generic.Queue<Terraria.DataStructures.CachedBuffer> | `private static Queue<CachedBuffer> HugeBufferQueue = new Queue<CachedBuffer>();` | `private static Queue<CachedBuffer> HugeBufferQueue = new Queue<CachedBuffer>();` |
| 1078 | field | Terraria.DataStructures.CachedBuffer | Terraria.DataStructures/CachedBuffer.cs | D:\TRbackup\Version4\Terraria.DataStructures\CachedBuffer.cs | 7 | 2 | Data | byte[] | `public readonly byte[] Data;` | `public readonly byte[] Data;` |
| 1079 | field | Terraria.DataStructures.CachedBuffer | Terraria.DataStructures/CachedBuffer.cs | D:\TRbackup\Version4\Terraria.DataStructures\CachedBuffer.cs | 9 | 2 | Writer | System.IO.BinaryWriter | `public readonly BinaryWriter Writer;` | `public readonly BinaryWriter Writer;` |
| 1080 | field | Terraria.DataStructures.CachedBuffer | Terraria.DataStructures/CachedBuffer.cs | D:\TRbackup\Version4\Terraria.DataStructures\CachedBuffer.cs | 11 | 2 | Reader | System.IO.BinaryReader | `public readonly BinaryReader Reader;` | `public readonly BinaryReader Reader;` |
| 1081 | field | Terraria.DataStructures.CachedBuffer | Terraria.DataStructures/CachedBuffer.cs | D:\TRbackup\Version4\Terraria.DataStructures\CachedBuffer.cs | 13 | 2 | _memoryStream | System.IO.MemoryStream | `private readonly MemoryStream _memoryStream;` | `private readonly MemoryStream _memoryStream;` |
| 1082 | field | Terraria.DataStructures.CachedBuffer | Terraria.DataStructures/CachedBuffer.cs | D:\TRbackup\Version4\Terraria.DataStructures\CachedBuffer.cs | 15 | 2 | _isActive | bool | `private bool _isActive = true;` | `private bool _isActive = true;` |
| 1088 | field | Terraria.DataStructures.DoubleStack<T1> | Terraria.DataStructures/DoubleStack.cs | D:\TRbackup\Version4\Terraria.DataStructures\DoubleStack.cs | 7 | 2 | _segmentList | T1[][] | `private T1[][] _segmentList;` | `private T1[][] _segmentList;` |
| 1089 | field | Terraria.DataStructures.DoubleStack<T1> | Terraria.DataStructures/DoubleStack.cs | D:\TRbackup\Version4\Terraria.DataStructures\DoubleStack.cs | 9 | 2 | _segmentSize | int | `private readonly int _segmentSize;` | `private readonly int _segmentSize;` |
| 1090 | field | Terraria.DataStructures.DoubleStack<T1> | Terraria.DataStructures/DoubleStack.cs | D:\TRbackup\Version4\Terraria.DataStructures\DoubleStack.cs | 11 | 2 | _segmentCount | int | `private int _segmentCount;` | `private int _segmentCount;` |
| 1091 | field | Terraria.DataStructures.DoubleStack<T1> | Terraria.DataStructures/DoubleStack.cs | D:\TRbackup\Version4\Terraria.DataStructures\DoubleStack.cs | 13 | 2 | _segmentShiftPosition | int | `private readonly int _segmentShiftPosition;` | `private readonly int _segmentShiftPosition;` |
| 1092 | field | Terraria.DataStructures.DoubleStack<T1> | Terraria.DataStructures/DoubleStack.cs | D:\TRbackup\Version4\Terraria.DataStructures\DoubleStack.cs | 15 | 2 | _start | int | `private int _start;` | `private int _start;` |
| 1093 | field | Terraria.DataStructures.DoubleStack<T1> | Terraria.DataStructures/DoubleStack.cs | D:\TRbackup\Version4\Terraria.DataStructures\DoubleStack.cs | 17 | 2 | _end | int | `private int _end;` | `private int _end;` |
| 1094 | field | Terraria.DataStructures.DoubleStack<T1> | Terraria.DataStructures/DoubleStack.cs | D:\TRbackup\Version4\Terraria.DataStructures\DoubleStack.cs | 19 | 2 | _size | int | `private int _size;` | `private int _size;` |
| 1095 | field | Terraria.DataStructures.DoubleStack<T1> | Terraria.DataStructures/DoubleStack.cs | D:\TRbackup\Version4\Terraria.DataStructures\DoubleStack.cs | 21 | 2 | _last | int | `private int _last;` | `private int _last;` |
| 1140 | field | Terraria.DataStructures.EntrySorter<TEntryType, TStepType> | Terraria.DataStructures/EntrySorter.cs | D:\TRbackup\Version4\Terraria.DataStructures\EntrySorter.cs | 8 | 2 | Steps | System.Collections.Generic.List<TStepType> | `public List<TStepType> Steps = new List<TStepType>();` | `public List<TStepType> Steps = new List<TStepType>();` |

#### 属性（2）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3686 | property | Terraria.DataStructures.CachedBuffer | Terraria.DataStructures/CachedBuffer.cs | D:\TRbackup\Version4\Terraria.DataStructures\CachedBuffer.cs | 17 | 2 | Length | int | `public int Length => Data.Length;` | `public int Length => Data.Length;` |
| 3687 | property | Terraria.DataStructures.DoubleStack<T1> | Terraria.DataStructures/DoubleStack.cs | D:\TRbackup\Version4\Terraria.DataStructures\DoubleStack.cs | 23 | 2 | Count | int | `public int Count => _size;` | `public int Count => _size;` |


### 4.9 细分子系统：`SharedRangeAndBitUtilities`

- 原报告章节：`4.9.56`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`20` / `诊断、工具与共享机制`

- 上一级基线细分子系统：`SharedRangeAndBitUtilities`
- 细分职责：范围、位图和位集合值对象。
- 边界角色：`value object`；最小 seam：range/bit query；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：6；声明类型数：6；字段：10；属性：5；合计：15。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（10）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2951 | field | Terraria.Utilities.Terraria.Utilities.FloatRange | Terraria.Utilities.Terraria.Utilities/FloatRange.cs | D:\TRbackup\Version4\Terraria.Utilities.Terraria.Utilities\FloatRange.cs | 8 | 2 | Minimum | float | `[JsonProperty("Min")] public readonly float Minimum;` | `[JsonProperty("Min")] public readonly float Minimum;` |
| 2952 | field | Terraria.Utilities.Terraria.Utilities.FloatRange | Terraria.Utilities.Terraria.Utilities/FloatRange.cs | D:\TRbackup\Version4\Terraria.Utilities.Terraria.Utilities\FloatRange.cs | 11 | 2 | Maximum | float | `[JsonProperty("Max")] public readonly float Maximum;` | `[JsonProperty("Max")] public readonly float Maximum;` |
| 2953 | field | Terraria.Utilities.Bits64 | Terraria.Utilities/Bits64.cs | D:\TRbackup\Version4\Terraria.Utilities\Bits64.cs | 5 | 2 | v | ulong | `private ulong v;` | `private ulong v;` |
| 2954 | field | Terraria.Utilities.BitSet2D | Terraria.Utilities/BitSet2D.cs | D:\TRbackup\Version4\Terraria.Utilities\BitSet2D.cs | 8 | 2 | offset | Point | `private Point offset;` | `private Point offset;` |
| 2955 | field | Terraria.Utilities.BitSet2D | Terraria.Utilities/BitSet2D.cs | D:\TRbackup\Version4\Terraria.Utilities\BitSet2D.cs | 10 | 2 | size | int | `private int size;` | `private int size;` |
| 2956 | field | Terraria.Utilities.BitSet2D | Terraria.Utilities/BitSet2D.cs | D:\TRbackup\Version4\Terraria.Utilities\BitSet2D.cs | 12 | 2 | bits | Terraria.Utilities.Bits64[] | `private Bits64[] bits;` | `private Bits64[] bits;` |
| 2961 | field | Terraria.Utilities.IntRange | Terraria.Utilities/IntRange.cs | D:\TRbackup\Version4\Terraria.Utilities\IntRange.cs | 7 | 2 | Minimum | int | `[JsonProperty("Min")] public readonly int Minimum;` | `[JsonProperty("Min")] public readonly int Minimum;` |
| 2962 | field | Terraria.Utilities.IntRange | Terraria.Utilities/IntRange.cs | D:\TRbackup\Version4\Terraria.Utilities\IntRange.cs | 10 | 2 | Maximum | int | `[JsonProperty("Max")] public readonly int Maximum;` | `[JsonProperty("Max")] public readonly int Maximum;` |
| 2990 | field | Terraria.Utilities.Vertical64BitStrips | Terraria.Utilities/Vertical64BitStrips.cs | D:\TRbackup\Version4\Terraria.Utilities\Vertical64BitStrips.cs | 8 | 2 | arr | Terraria.Utilities.Bits64[] | `private Bits64[] arr;` | `private Bits64[] arr;` |
| 2997 | field | Terraria.BitsByte | Terraria/BitsByte.cs | D:\TRbackup\Version4\Terraria\BitsByte.cs | 10 | 2 | value | byte | `private byte value;` | `private byte value;` |

#### 属性（5）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3931 | property | Terraria.Utilities.Bits64 | Terraria.Utilities/Bits64.cs | D:\TRbackup\Version4\Terraria.Utilities\Bits64.cs | 7 | 2 | this[] | bool | `public bool this[int i] { get { return (v & (ulong)(1L << i)) != 0; } set { if (value) { v \|= (ulong)(1L << i); } else { v &= (ulong)(~(1L << i)); } } }` | `public bool this[int i] { get { return (v & (ulong)(1L << i)) != 0; } set { if (value) { v \|= (ulong)(1L << i); } else { v &= (ulong)(~(1L << i)); } } }` |
| 3932 | property | Terraria.Utilities.Bits64 | Terraria.Utilities/Bits64.cs | D:\TRbackup\Version4\Terraria.Utilities\Bits64.cs | 26 | 2 | IsEmpty | bool | `public bool IsEmpty => v == 0;` | `public bool IsEmpty => v == 0;` |
| 3933 | property | Terraria.Utilities.BitSet2D | Terraria.Utilities/BitSet2D.cs | D:\TRbackup\Version4\Terraria.Utilities\BitSet2D.cs | 14 | 2 | this[] | bool | `public bool this[Point p] { get { int num = Coord(p); return bits[num >> 6][num & 0x3F]; } set { int num = Coord(p); bits[num >> 6][num & 0x3F] = value; } }` | `public bool this[Point p] { get { int num = Coord(p); return bits[num >> 6][num & 0x3F]; } set { int num = Coord(p); bits[num >> 6][num & 0x3F] = value; } }` |
| 3942 | property | Terraria.Utilities.Vertical64BitStrips | Terraria.Utilities/Vertical64BitStrips.cs | D:\TRbackup\Version4\Terraria.Utilities\Vertical64BitStrips.cs | 10 | 2 | this[] | Terraria.Utilities.Bits64 | `public Bits64 this[int x] { get { return arr[x]; } set { arr[x] = value; } }` | `public Bits64 this[int x] { get { return arr[x]; } set { arr[x] = value; } }` |
| 3943 | property | Terraria.BitsByte | Terraria/BitsByte.cs | D:\TRbackup\Version4\Terraria\BitsByte.cs | 12 | 2 | this[] | bool | `public bool this[int key] { get { return (value & (1 << key)) != 0; } set { if (value) { this.value \|= (byte)(1 << key); } else { this.value &= (byte)(~(1 << key)); } } }` | `public bool this[int key] { get { return (value & (1 << key)) != 0; } set { if (value) { this.value \|= (byte)(1 << key); } else { this.value &= (byte)(~(1 << key)); } } }` |


### 4.10 细分子系统：`SharedGeneralDiagnosticsUtilities`

- 原报告章节：`4.9.84`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`20` / `诊断、工具与共享机制`

- 上一级基线细分子系统：`SharedGeneralDiagnosticsUtilities`
- 细分职责：异常观察和通用诊断工具状态。
- 边界角色：`diagnostics`；最小 seam：general diagnostics sink；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：0；属性：5；合计：5。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（0）

无该类型成员记录。

#### 属性（5）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3934 | property | Terraria.Utilities.CrashWatcher | Terraria.Utilities/CrashWatcher.cs | D:\TRbackup\Version4\Terraria.Utilities\CrashWatcher.cs | 12 | 2 | LogAllExceptions | bool | `public static bool LogAllExceptions { get; set; }` | `public static bool LogAllExceptions { get; set; }` |
| 3935 | property | Terraria.Utilities.CrashWatcher | Terraria.Utilities/CrashWatcher.cs | D:\TRbackup\Version4\Terraria.Utilities\CrashWatcher.cs | 14 | 2 | DumpOnException | bool | `public static bool DumpOnException { get; set; }` | `public static bool DumpOnException { get; set; }` |
| 3936 | property | Terraria.Utilities.CrashWatcher | Terraria.Utilities/CrashWatcher.cs | D:\TRbackup\Version4\Terraria.Utilities\CrashWatcher.cs | 16 | 2 | DumpOnCrash | bool | `public static bool DumpOnCrash { get; private set; }` | `public static bool DumpOnCrash { get; private set; }` |
| 3937 | property | Terraria.Utilities.CrashWatcher | Terraria.Utilities/CrashWatcher.cs | D:\TRbackup\Version4\Terraria.Utilities\CrashWatcher.cs | 18 | 2 | CrashDumpOptions | Terraria.Utilities.CrashDump.Options | `public static CrashDump.Options CrashDumpOptions { get; private set; }` | `public static CrashDump.Options CrashDumpOptions { get; private set; }` |
| 3938 | property | Terraria.Utilities.CrashWatcher | Terraria.Utilities/CrashWatcher.cs | D:\TRbackup\Version4\Terraria.Utilities\CrashWatcher.cs | 20 | 2 | DumpPath | string | `private static string DumpPath => Path.Combine(Main.SavePath, "Dumps");` | `private static string DumpPath => Path.Combine(Main.SavePath, "Dumps");` |


### 4.11 细分子系统：`SharedGeneralDelegateAndMetadataUtilities`

- 原报告章节：`4.9.85`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`20` / `诊断、工具与共享机制`

- 上一级基线细分子系统：`SharedGeneralDelegateAndMetadataUtilities`
- 细分职责：委托方法、旧属性元数据和秘密值辅助。
- 边界角色：`value object/metadata`；最小 seam：delegate metadata utility port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：3；声明类型数：4；字段：10；属性：1；合计：11。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（10）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2965 | field | Terraria.Utilities.OldAttribute | Terraria.Utilities/OldAttribute.cs | D:\TRbackup\Version4\Terraria.Utilities\OldAttribute.cs | 7 | 2 | message | string | `private string message;` | `private string message;` |
| 2966 | field | Terraria.Utilities.Secrets | Terraria.Utilities/Secrets.cs | D:\TRbackup\Version4\Terraria.Utilities\Secrets.cs | 9 | 2 | _salt | byte[] | `private static readonly byte[] _salt;` | `private static readonly byte[] _salt;` |
| 3034 | field | Terraria.DelegateMethods.Minecart | Terraria/DelegateMethods.cs | D:\TRbackup\Version4\Terraria\DelegateMethods.cs | 234 | 3 | rotationOrigin | Vector2 | `public static Vector2 rotationOrigin;` | `public static Vector2 rotationOrigin;` |
| 3035 | field | Terraria.DelegateMethods.Minecart | Terraria/DelegateMethods.cs | D:\TRbackup\Version4\Terraria\DelegateMethods.cs | 236 | 3 | rotation | float | `public static float rotation;` | `public static float rotation;` |
| 3036 | field | Terraria.DelegateMethods | Terraria/DelegateMethods.cs | D:\TRbackup\Version4\Terraria\DelegateMethods.cs | 456 | 2 | v3_1 | Vector3 | `public static Vector3 v3_1 = Vector3.Zero;` | `public static Vector3 v3_1 = Vector3.Zero;` |
| 3037 | field | Terraria.DelegateMethods | Terraria/DelegateMethods.cs | D:\TRbackup\Version4\Terraria\DelegateMethods.cs | 458 | 2 | v2_1 | Vector2 | `public static Vector2 v2_1 = Vector2.Zero;` | `public static Vector2 v2_1 = Vector2.Zero;` |
| 3038 | field | Terraria.DelegateMethods | Terraria/DelegateMethods.cs | D:\TRbackup\Version4\Terraria\DelegateMethods.cs | 460 | 2 | f_1 | float | `public static float f_1 = 0f;` | `public static float f_1 = 0f;` |
| 3039 | field | Terraria.DelegateMethods | Terraria/DelegateMethods.cs | D:\TRbackup\Version4\Terraria\DelegateMethods.cs | 462 | 2 | CheckResultOut | bool | `public static bool CheckResultOut;` | `public static bool CheckResultOut;` |
| 3040 | field | Terraria.DelegateMethods | Terraria/DelegateMethods.cs | D:\TRbackup\Version4\Terraria\DelegateMethods.cs | 464 | 2 | tilecut_0 | Terraria.Enums.TileCuttingContext | `public static TileCuttingContext tilecut_0 = TileCuttingContext.Unknown;` | `public static TileCuttingContext tilecut_0 = TileCuttingContext.Unknown;` |
| 3041 | field | Terraria.DelegateMethods | Terraria/DelegateMethods.cs | D:\TRbackup\Version4\Terraria\DelegateMethods.cs | 466 | 2 | tileCutIgnore | bool[] | `public static bool[] tileCutIgnore = null;` | `public static bool[] tileCutIgnore = null;` |

#### 属性（1）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3940 | property | Terraria.Utilities.OldAttribute | Terraria.Utilities/OldAttribute.cs | D:\TRbackup\Version4\Terraria.Utilities\OldAttribute.cs | 9 | 2 | Message | string | `public string Message => message;` | `public string Message => message;` |


### 4.12 细分子系统：`DebugCommandProtocol`

- 原报告章节：`4.9.86`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`20` / `诊断、工具与共享机制`

- 上一级基线细分子系统：`DebugCommandProtocol`
- 细分职责：调试命令接口、属性、消息和处理器协议。
- 边界角色：`diagnostics/adapter`；最小 seam：debug command protocol；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：4；声明类型数：5；字段：12；属性：8；合计：20。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（12）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2891 | field | Terraria.Testing.ChatCommands.DebugCommandAttribute.InternalDebugCommand | Terraria.Testing.ChatCommands/DebugCommandAttribute.cs | D:\TRbackup\Version4\Terraria.Testing.ChatCommands\DebugCommandAttribute.cs | 13 | 3 | _processMethod | Terraria.Testing.ChatCommands.DebugCommandAttribute.InternalDebugCommand.ProcessMethod | `private readonly ProcessMethod _processMethod;` | `private readonly ProcessMethod _processMethod;` |
| 2892 | field | Terraria.Testing.ChatCommands.DebugCommandAttribute | Terraria.Testing.ChatCommands/DebugCommandAttribute.cs | D:\TRbackup\Version4\Terraria.Testing.ChatCommands\DebugCommandAttribute.cs | 36 | 2 | Name | string | `public readonly string Name;` | `public readonly string Name;` |
| 2893 | field | Terraria.Testing.ChatCommands.DebugCommandAttribute | Terraria.Testing.ChatCommands/DebugCommandAttribute.cs | D:\TRbackup\Version4\Terraria.Testing.ChatCommands\DebugCommandAttribute.cs | 38 | 2 | Description | string | `public readonly string Description;` | `public readonly string Description;` |
| 2894 | field | Terraria.Testing.ChatCommands.DebugCommandAttribute | Terraria.Testing.ChatCommands/DebugCommandAttribute.cs | D:\TRbackup\Version4\Terraria.Testing.ChatCommands\DebugCommandAttribute.cs | 40 | 2 | Requirements | Terraria.Testing.ChatCommands.CommandRequirement | `public readonly CommandRequirement Requirements;` | `public readonly CommandRequirement Requirements;` |
| 2895 | field | Terraria.Testing.ChatCommands.DebugCommandAttribute | Terraria.Testing.ChatCommands/DebugCommandAttribute.cs | D:\TRbackup\Version4\Terraria.Testing.ChatCommands\DebugCommandAttribute.cs | 42 | 2 | HelpText | string | `public string HelpText;` | `public string HelpText;` |
| 2896 | field | Terraria.Testing.ChatCommands.DebugCommandProcessor | Terraria.Testing.ChatCommands/DebugCommandProcessor.cs | D:\TRbackup\Version4\Terraria.Testing.ChatCommands\DebugCommandProcessor.cs | 17 | 2 | _commands | System.Collections.Generic.Dictionary<string, Terraria.Testing.ChatCommands.IDebugCommand> | `private readonly Dictionary<string, IDebugCommand> _commands = new Dictionary<string, IDebugCommand>();` | `private readonly Dictionary<string, IDebugCommand> _commands = new Dictionary<string, IDebugCommand>();` |
| 2897 | field | Terraria.Testing.ChatCommands.DebugCommandProcessor | Terraria.Testing.ChatCommands/DebugCommandProcessor.cs | D:\TRbackup\Version4\Terraria.Testing.ChatCommands\DebugCommandProcessor.cs | 19 | 2 | MemoCommandsPath | string | `private static string MemoCommandsPath = Path.Combine(Main.SavePath, "MemoCommands");` | `private static string MemoCommandsPath = Path.Combine(Main.SavePath, "MemoCommands");` |
| 2898 | field | Terraria.Testing.ChatCommands.DebugMessage | Terraria.Testing.ChatCommands/DebugMessage.cs | D:\TRbackup\Version4\Terraria.Testing.ChatCommands\DebugMessage.cs | 11 | 2 | COMMAND_PREFIX | char | `private const char COMMAND_PREFIX = '/';` | `private const char COMMAND_PREFIX = '/';` |
| 2899 | field | Terraria.Testing.ChatCommands.DebugMessage | Terraria.Testing.ChatCommands/DebugMessage.cs | D:\TRbackup\Version4\Terraria.Testing.ChatCommands\DebugMessage.cs | 13 | 2 | Author | byte | `public readonly byte Author;` | `public readonly byte Author;` |
| 2900 | field | Terraria.Testing.ChatCommands.DebugMessage | Terraria.Testing.ChatCommands/DebugMessage.cs | D:\TRbackup\Version4\Terraria.Testing.ChatCommands\DebugMessage.cs | 15 | 2 | CommandName | string | `public readonly string CommandName = "";` | `public readonly string CommandName = "";` |
| 2901 | field | Terraria.Testing.ChatCommands.DebugMessage | Terraria.Testing.ChatCommands/DebugMessage.cs | D:\TRbackup\Version4\Terraria.Testing.ChatCommands\DebugMessage.cs | 17 | 2 | Arguments | string | `public readonly string Arguments = "";` | `public readonly string Arguments = "";` |
| 2902 | field | Terraria.Testing.ChatCommands.DebugMessage | Terraria.Testing.ChatCommands/DebugMessage.cs | D:\TRbackup\Version4\Terraria.Testing.ChatCommands\DebugMessage.cs | 19 | 2 | MousePosition | Vector2 | `public readonly Vector2 MousePosition;` | `public readonly Vector2 MousePosition;` |

#### 属性（8）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3922 | property | Terraria.Testing.ChatCommands.DebugCommandAttribute.InternalDebugCommand | Terraria.Testing.ChatCommands/DebugCommandAttribute.cs | D:\TRbackup\Version4\Terraria.Testing.ChatCommands\DebugCommandAttribute.cs | 15 | 3 | Name | string | `public string Name { get; private set; }` | `public string Name { get; private set; }` |
| 3923 | property | Terraria.Testing.ChatCommands.DebugCommandAttribute.InternalDebugCommand | Terraria.Testing.ChatCommands/DebugCommandAttribute.cs | D:\TRbackup\Version4\Terraria.Testing.ChatCommands\DebugCommandAttribute.cs | 17 | 3 | Description | string | `public string Description { get; private set; }` | `public string Description { get; private set; }` |
| 3924 | property | Terraria.Testing.ChatCommands.DebugCommandAttribute.InternalDebugCommand | Terraria.Testing.ChatCommands/DebugCommandAttribute.cs | D:\TRbackup\Version4\Terraria.Testing.ChatCommands\DebugCommandAttribute.cs | 19 | 3 | HelpText | string | `public string HelpText { get; private set; }` | `public string HelpText { get; private set; }` |
| 3925 | property | Terraria.Testing.ChatCommands.DebugCommandAttribute.InternalDebugCommand | Terraria.Testing.ChatCommands/DebugCommandAttribute.cs | D:\TRbackup\Version4\Terraria.Testing.ChatCommands\DebugCommandAttribute.cs | 21 | 3 | Requirements | Terraria.Testing.ChatCommands.CommandRequirement | `public CommandRequirement Requirements { get; private set; }` | `public CommandRequirement Requirements { get; private set; }` |
| 3926 | property | Terraria.Testing.ChatCommands.IDebugCommand | Terraria.Testing.ChatCommands/IDebugCommand.cs | D:\TRbackup\Version4\Terraria.Testing.ChatCommands\IDebugCommand.cs | 5 | 2 | Name | string | `string Name { get; }` | `string Name { get; }` |
| 3927 | property | Terraria.Testing.ChatCommands.IDebugCommand | Terraria.Testing.ChatCommands/IDebugCommand.cs | D:\TRbackup\Version4\Terraria.Testing.ChatCommands\IDebugCommand.cs | 7 | 2 | Description | string | `string Description { get; }` | `string Description { get; }` |
| 3928 | property | Terraria.Testing.ChatCommands.IDebugCommand | Terraria.Testing.ChatCommands/IDebugCommand.cs | D:\TRbackup\Version4\Terraria.Testing.ChatCommands\IDebugCommand.cs | 9 | 2 | HelpText | string | `string HelpText { get; }` | `string HelpText { get; }` |
| 3929 | property | Terraria.Testing.ChatCommands.IDebugCommand | Terraria.Testing.ChatCommands/IDebugCommand.cs | D:\TRbackup\Version4\Terraria.Testing.ChatCommands\IDebugCommand.cs | 11 | 2 | Requirements | Terraria.Testing.ChatCommands.CommandRequirement | `CommandRequirement Requirements { get; }` | `CommandRequirement Requirements { get; }` |


### 4.13 细分子系统：`DebugRuntimeOptions`

- 原报告章节：`4.9.87`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`20` / `诊断、工具与共享机制`

- 上一级基线细分子系统：`DebugRuntimeOptions`
- 细分职责：调试命令开关和运行时调试选项。
- 边界角色：`diagnostics/state`；最小 seam：debug runtime options；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：10；属性：0；合计：10。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（10）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2903 | field | Terraria.Testing.DebugOptions | Terraria.Testing/DebugOptions.cs | D:\TRbackup\Version4\Terraria.Testing\DebugOptions.cs | 8 | 2 | enableDebugCommands | bool | `public static bool enableDebugCommands = false;` | `public static bool enableDebugCommands = false;` |
| 2904 | field | Terraria.Testing.DebugOptions | Terraria.Testing/DebugOptions.cs | D:\TRbackup\Version4\Terraria.Testing\DebugOptions.cs | 10 | 2 | Shared_ReportCommandUsage | bool | `public static bool Shared_ReportCommandUsage = true;` | `public static bool Shared_ReportCommandUsage = true;` |
| 2905 | field | Terraria.Testing.DebugOptions | Terraria.Testing/DebugOptions.cs | D:\TRbackup\Version4\Terraria.Testing\DebugOptions.cs | 12 | 2 | Shared_ServerPing | int | `public static int Shared_ServerPing = 0;` | `public static int Shared_ServerPing = 0;` |
| 2906 | field | Terraria.Testing.DebugOptions | Terraria.Testing/DebugOptions.cs | D:\TRbackup\Version4\Terraria.Testing\DebugOptions.cs | 14 | 2 | UpdateWaitInMs | double | `public static double UpdateWaitInMs = 0.0;` | `public static double UpdateWaitInMs = 0.0;` |
| 2907 | field | Terraria.Testing.DebugOptions | Terraria.Testing/DebugOptions.cs | D:\TRbackup\Version4\Terraria.Testing\DebugOptions.cs | 16 | 2 | noLimits | bool | `public static bool noLimits;` | `public static bool noLimits;` |
| 2908 | field | Terraria.Testing.DebugOptions | Terraria.Testing/DebugOptions.cs | D:\TRbackup\Version4\Terraria.Testing\DebugOptions.cs | 18 | 2 | ShowNetOffsetDust | bool | `public static bool ShowNetOffsetDust;` | `public static bool ShowNetOffsetDust;` |
| 2909 | field | Terraria.Testing.DebugOptions | Terraria.Testing/DebugOptions.cs | D:\TRbackup\Version4\Terraria.Testing\DebugOptions.cs | 20 | 2 | FakeNetOffset | Vector2 | `public static Vector2 FakeNetOffset;` | `public static Vector2 FakeNetOffset;` |
| 2910 | field | Terraria.Testing.DebugOptions | Terraria.Testing/DebugOptions.cs | D:\TRbackup\Version4\Terraria.Testing\DebugOptions.cs | 22 | 2 | NoDamageVar | bool | `public static bool NoDamageVar;` | `public static bool NoDamageVar;` |
| 2911 | field | Terraria.Testing.DebugOptions | Terraria.Testing/DebugOptions.cs | D:\TRbackup\Version4\Terraria.Testing\DebugOptions.cs | 24 | 2 | LetProjectilesAimAtTargetDummies | bool | `public static bool LetProjectilesAimAtTargetDummies;` | `public static bool LetProjectilesAimAtTargetDummies;` |
| 2912 | field | Terraria.Testing.DebugOptions | Terraria.Testing/DebugOptions.cs | D:\TRbackup\Version4\Terraria.Testing\DebugOptions.cs | 26 | 2 | PracticeMode | bool | `public static bool PracticeMode;` | `public static bool PracticeMode;` |

#### 属性（0）

无该类型成员记录。


### 4.14 细分子系统：`DebugFrameTelemetry`

- 原报告章节：`4.9.88`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`20` / `诊断、工具与共享机制`

- 上一级基线细分子系统：`DebugFrameTelemetry`
- 细分职责：详细 FPS 帧、事件和采样状态。
- 边界角色：`diagnostics/state`；最小 seam：frame telemetry port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：3；字段：16；属性：0；合计：16。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（16）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2913 | field | Terraria.Testing.DetailedFPS.Frame.Event | Terraria.Testing/DetailedFPS.cs | D:\TRbackup\Version4\Terraria.Testing\DetailedFPS.cs | 30 | 4 | category | Terraria.Testing.DetailedFPS.OperationCategory | `public OperationCategory category;` | `public OperationCategory category;` |
| 2914 | field | Terraria.Testing.DetailedFPS.Frame.Event | Terraria.Testing/DetailedFPS.cs | D:\TRbackup\Version4\Terraria.Testing\DetailedFPS.cs | 32 | 4 | timestamp | long | `public long timestamp;` | `public long timestamp;` |
| 2915 | field | Terraria.Testing.DetailedFPS.Frame | Terraria.Testing/DetailedFPS.cs | D:\TRbackup\Version4\Terraria.Testing\DetailedFPS.cs | 41 | 3 | events | System.Collections.Generic.List<Terraria.Testing.DetailedFPS.Frame.Event> | `public List<Event> events;` | `public List<Event> events;` |
| 2916 | field | Terraria.Testing.DetailedFPS.Frame | Terraria.Testing/DetailedFPS.cs | D:\TRbackup\Version4\Terraria.Testing\DetailedFPS.cs | 43 | 3 | CollectionCount | int[] | `public int[] CollectionCount;` | `public int[] CollectionCount;` |
| 2917 | field | Terraria.Testing.DetailedFPS.Frame | Terraria.Testing/DetailedFPS.cs | D:\TRbackup\Version4\Terraria.Testing\DetailedFPS.cs | 45 | 3 | Allocated | long | `public long Allocated;` | `public long Allocated;` |
| 2918 | field | Terraria.Testing.DetailedFPS | Terraria.Testing/DetailedFPS.cs | D:\TRbackup\Version4\Terraria.Testing\DetailedFPS.cs | 107 | 2 | FrameCount | int | `public static readonly int FrameCount;` | `public static readonly int FrameCount;` |
| 2919 | field | Terraria.Testing.DetailedFPS | Terraria.Testing/DetailedFPS.cs | D:\TRbackup\Version4\Terraria.Testing\DetailedFPS.cs | 109 | 2 | Frames | Terraria.Testing.DetailedFPS.Frame[] | `private static Frame[] Frames;` | `private static Frame[] Frames;` |
| 2920 | field | Terraria.Testing.DetailedFPS | Terraria.Testing/DetailedFPS.cs | D:\TRbackup\Version4\Terraria.Testing\DetailedFPS.cs | 111 | 2 | oldest | int | `private static int oldest;` | `private static int oldest;` |
| 2921 | field | Terraria.Testing.DetailedFPS | Terraria.Testing/DetailedFPS.cs | D:\TRbackup\Version4\Terraria.Testing\DetailedFPS.cs | 113 | 2 | newest | int | `private static int newest;` | `private static int newest;` |
| 2922 | field | Terraria.Testing.DetailedFPS | Terraria.Testing/DetailedFPS.cs | D:\TRbackup\Version4\Terraria.Testing\DetailedFPS.cs | 115 | 2 | LastGCPauseTime | System.TimeSpan | `private static TimeSpan LastGCPauseTime;` | `private static TimeSpan LastGCPauseTime;` |
| 2923 | field | Terraria.Testing.DetailedFPS | Terraria.Testing/DetailedFPS.cs | D:\TRbackup\Version4\Terraria.Testing\DetailedFPS.cs | 117 | 2 | LastCollectionCount | int[] | `private static int[] LastCollectionCount;` | `private static int[] LastCollectionCount;` |
| 2924 | field | Terraria.Testing.DetailedFPS | Terraria.Testing/DetailedFPS.cs | D:\TRbackup\Version4\Terraria.Testing\DetailedFPS.cs | 119 | 2 | LastAllocatedBytes | long | `private static long LastAllocatedBytes;` | `private static long LastAllocatedBytes;` |
| 2925 | field | Terraria.Testing.DetailedFPS | Terraria.Testing/DetailedFPS.cs | D:\TRbackup\Version4\Terraria.Testing\DetailedFPS.cs | 121 | 2 | PixelsPerMs | int | `private const int PixelsPerMs = 6;` | `private const int PixelsPerMs = 6;` |
| 2926 | field | Terraria.Testing.DetailedFPS | Terraria.Testing/DetailedFPS.cs | D:\TRbackup\Version4\Terraria.Testing\DetailedFPS.cs | 123 | 2 | FrameWidth | int | `private const int FrameWidth = 2;` | `private const int FrameWidth = 2;` |
| 2927 | field | Terraria.Testing.DetailedFPS | Terraria.Testing/DetailedFPS.cs | D:\TRbackup\Version4\Terraria.Testing\DetailedFPS.cs | 125 | 2 | BoxHeight | int | `private const int BoxHeight = 100;` | `private const int BoxHeight = 100;` |
| 2928 | field | Terraria.Testing.DetailedFPS | Terraria.Testing/DetailedFPS.cs | D:\TRbackup\Version4\Terraria.Testing\DetailedFPS.cs | 127 | 2 | _gcGenText | string[] | `private static string[] _gcGenText;` | `private static string[] _gcGenText;` |

#### 属性（0）

无该类型成员记录。


### 4.15 细分子系统：`DebugBuildStatus`

- 原报告章节：`4.9.89`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`20` / `诊断、工具与共享机制`

- 上一级基线细分子系统：`DebugBuildStatus`
- 细分职责：源码版本和 Git 构建状态信息。
- 边界角色：`diagnostics/adapter`；最小 seam：build status port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：1；属性：1；合计：2。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（1）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2929 | field | Terraria.Testing.GitStatus | Terraria.Testing/GitStatus.cs | D:\TRbackup\Version4\Terraria.Testing\GitStatus.cs | 11 | 2 | _gitSHA | string | `private static string _gitSHA = "";` | `private static string _gitSHA = "";` |

#### 属性（1）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3930 | property | Terraria.Testing.GitStatus | Terraria.Testing/GitStatus.cs | D:\TRbackup\Version4\Terraria.Testing\GitStatus.cs | 13 | 2 | GitSHA | string | `public static string GitSHA { get { Init(); return _gitSHA; } }` | `public static string GitSHA { get { Init(); return _gitSHA; } }` |


### 4.16 细分子系统：`SharedTimeLoggerFrameCoordinationState`

- 原报告章节：`4.9.113`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`20` / `诊断、工具与共享机制`

- 上一级基线细分子系统：`SharedTimeLoggerCoordinatorState`
- 细分职责：计时器帧边界、条目注册和下一帧控制状态。
- 边界角色：`diagnostics state`；最小 seam：time logger frame coordination port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：15；属性：1；合计：16。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（15）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3537 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 184 | 2 | FrameCount | int | `public static readonly int FrameCount;` | `public static readonly int FrameCount;` |
| 3538 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 186 | 2 | logWriter | System.IO.StreamWriter | `private static StreamWriter logWriter;` | `private static StreamWriter logWriter;` |
| 3539 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 188 | 2 | logBuilder | System.Text.StringBuilder | `private static StringBuilder logBuilder;` | `private static StringBuilder logBuilder;` |
| 3540 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 190 | 2 | framesToLog | int | `private static int framesToLog;` | `private static int framesToLog;` |
| 3541 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 192 | 2 | currentFrame | int | `private static int currentFrame;` | `private static int currentFrame;` |
| 3542 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 194 | 2 | startLoggingNextFrame | bool | `private static bool startLoggingNextFrame;` | `private static bool startLoggingNextFrame;` |
| 3543 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 196 | 2 | endLoggingThisFrame | bool | `private static bool endLoggingThisFrame;` | `private static bool endLoggingThisFrame;` |
| 3544 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 198 | 2 | currentlyLogging | bool | `private static bool currentlyLogging;` | `private static bool currentlyLogging;` |
| 3545 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 200 | 2 | DataSeriesHeaders | string[] | `private static string[] DataSeriesHeaders;` | `private static string[] DataSeriesHeaders;` |
| 3546 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 202 | 2 | activeDataSeries | int | `private static int activeDataSeries;` | `private static int activeDataSeries;` |
| 3547 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 204 | 2 | entries | System.Collections.Generic.List<Terraria.TimeLogger.TimeLogData> | `private static List<TimeLogData> entries;` | `private static List<TimeLogData> entries;` |
| 3614 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 338 | 2 | _onNextFrame | System.Collections.Generic.Queue<System.Action> | `private static Queue<Action> _onNextFrame;` | `private static Queue<Action> _onNextFrame;` |
| 3615 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 340 | 2 | ABTestMode | int | `public static int ABTestMode;` | `public static int ABTestMode;` |
| 3616 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 342 | 2 | ABTestName | string | `public static readonly string ABTestName;` | `public static readonly string ABTestName;` |
| 3617 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 344 | 2 | _entriesToDraw | System.Collections.Generic.Queue<Terraria.TimeLogger.TimeLogData> | `private static Queue<TimeLogData> _entriesToDraw;` | `private static Queue<TimeLogData> _entriesToDraw;` |

#### 属性（1）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 4024 | property | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 364 | 2 | ABTestFlag | bool | `public static bool ABTestFlag { get { return TileDrawingBase.DrawOwnBlacks; } set { TileDrawingBase.DrawOwnBlacks = value; } }` | `public static bool ABTestFlag { get { return TileDrawingBase.DrawOwnBlacks; } set { TileDrawingBase.DrawOwnBlacks = value; } }` |


### 4.17 细分子系统：`SharedGeneralRandomAndBufferUtilities`

- 原报告章节：`4.9.158`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`20` / `诊断、工具与共享机制`

- 上一级基线细分子系统：`SharedGeneralPureUtilities`
- 细分职责：随机常量、正则缓存和 flood-fill 工作缓冲区。
- 边界角色：`query/value object`；最小 seam：general random buffer port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：8；属性：0；合计：8。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（8）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3647 | field | Terraria.Utils | Terraria/Utils.cs | D:\TRbackup\Version4\Terraria\Utils.cs | 84 | 2 | charLengths | System.Collections.Generic.Dictionary<DynamicSpriteFont, float[]> | `public static Dictionary<DynamicSpriteFont, float[]> charLengths = new Dictionary<DynamicSpriteFont, float[]>();` | `public static Dictionary<DynamicSpriteFont, float[]> charLengths = new Dictionary<DynamicSpriteFont, float[]>();` |
| 3648 | field | Terraria.Utils | Terraria/Utils.cs | D:\TRbackup\Version4\Terraria\Utils.cs | 86 | 2 | _substitutionRegex | System.Text.RegularExpressions.Regex | `private static Regex _substitutionRegex = new Regex("{(\\?(?:!)?)?([a-zA-Z][\\w\\.]*)}", RegexOptions.Compiled);` | `private static Regex _substitutionRegex = new Regex("{(\\?(?:!)?)?([a-zA-Z][\\w\\.]*)}", RegexOptions.Compiled);` |
| 3649 | field | Terraria.Utils | Terraria/Utils.cs | D:\TRbackup\Version4\Terraria\Utils.cs | 88 | 2 | RANDOM_MULTIPLIER | ulong | `private const ulong RANDOM_MULTIPLIER = 25214903917uL;` | `private const ulong RANDOM_MULTIPLIER = 25214903917uL;` |
| 3650 | field | Terraria.Utils | Terraria/Utils.cs | D:\TRbackup\Version4\Terraria\Utils.cs | 90 | 2 | RANDOM_ADD | ulong | `private const ulong RANDOM_ADD = 11uL;` | `private const ulong RANDOM_ADD = 11uL;` |
| 3651 | field | Terraria.Utils | Terraria/Utils.cs | D:\TRbackup\Version4\Terraria\Utils.cs | 92 | 2 | RANDOM_MASK | ulong | `private const ulong RANDOM_MASK = 281474976710655uL;` | `private const ulong RANDOM_MASK = 281474976710655uL;` |
| 3652 | field | Terraria.Utils | Terraria/Utils.cs | D:\TRbackup\Version4\Terraria\Utils.cs | 94 | 2 | _floodFillQueue1 | System.Collections.Generic.List<Point> | `private static readonly List<Point> _floodFillQueue1 = new List<Point>(2500);` | `private static readonly List<Point> _floodFillQueue1 = new List<Point>(2500);` |
| 3653 | field | Terraria.Utils | Terraria/Utils.cs | D:\TRbackup\Version4\Terraria\Utils.cs | 96 | 2 | _floodFillQueue2 | System.Collections.Generic.List<Point> | `private static readonly List<Point> _floodFillQueue2 = new List<Point>(2500);` | `private static readonly List<Point> _floodFillQueue2 = new List<Point>(2500);` |
| 3654 | field | Terraria.Utils | Terraria/Utils.cs | D:\TRbackup\Version4\Terraria\Utils.cs | 98 | 2 | _floodFillBitset | Terraria.Utilities.BitSet2D | `private static readonly BitSet2D _floodFillBitset = new BitSet2D();` | `private static readonly BitSet2D _floodFillBitset = new BitSet2D();` |

#### 属性（0）

无该类型成员记录。


### 4.18 细分子系统：`SharedTimeLoggerDisplayFormattingState`

- 原报告章节：`4.9.167`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`20` / `诊断、工具与共享机制`

- 上一级基线细分子系统：`SharedTimeLoggerCoordinatorState`
- 上一级 peer 细分子系统：`SharedTimeLoggerRenderMetricsState`
- 细分职责：TimeLogger 的 CPU、百分比、毫秒和显示格式缓存。
- 边界角色：`diagnostics/query`；最小 seam：time logger display formatting port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：9；属性：0；合计：9。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（9）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3618 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 346 | 2 | _PinnedCPUFormat | Terraria.TimeLogger.FormatPool | `private static FormatPool _PinnedCPUFormat;` | `private static FormatPool _PinnedCPUFormat;` |
| 3619 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 348 | 2 | _AssignedCPUFormat | Terraria.TimeLogger.FormatPool | `private static FormatPool _AssignedCPUFormat;` | `private static FormatPool _AssignedCPUFormat;` |
| 3620 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 350 | 2 | _procThrottleFormat | Terraria.TimeLogger.FormatPool | `private static FormatPool _procThrottleFormat;` | `private static FormatPool _procThrottleFormat;` |
| 3621 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 352 | 2 | _expectedCPUFormat | Terraria.TimeLogger.FormatPool | `private static FormatPool _expectedCPUFormat;` | `private static FormatPool _expectedCPUFormat;` |
| 3622 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 354 | 2 | _terrariaCPUFormat | Terraria.TimeLogger.FormatPool | `private static FormatPool _terrariaCPUFormat;` | `private static FormatPool _terrariaCPUFormat;` |
| 3623 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 356 | 2 | _pendingCPUFormat | Terraria.TimeLogger.FormatPool | `private static FormatPool _pendingCPUFormat;` | `private static FormatPool _pendingCPUFormat;` |
| 3624 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 358 | 2 | _percentFormat | Terraria.TimeLogger.FormatPool | `private static FormatPool _percentFormat;` | `private static FormatPool _percentFormat;` |
| 3625 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 360 | 2 | _msFormat | Terraria.TimeLogger.FormatPool | `private static FormatPool _msFormat;` | `private static FormatPool _msFormat;` |
| 3626 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 362 | 2 | _intFormat | Terraria.TimeLogger.FormatPool | `private static FormatPool _intFormat;` | `private static FormatPool _intFormat;` |

#### 属性（0）

无该类型成员记录。


### 4.19 细分子系统：`SharedTimeLoggerEntityAndInterfacePhaseMetricsState`

- 原报告章节：`4.9.198`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`20` / `诊断、工具与共享机制`

- 上一级基线细分子系统：`SharedTimeLoggerCoordinatorState`
- 上一级 peer 细分子系统：`SharedTimeLoggerPhaseMetricsState`
- 细分职责：TimeLogger 实体绘制、界面、菜单和诊断阶段指标。
- 边界角色：`diagnostics state`；最小 seam：time logger entity interface metrics port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：20；属性：0；合计：20。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（20）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3593 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 296 | 2 | PlayerChat | Terraria.TimeLogger.TimeLogData | `public static TimeLogData PlayerChat;` | `public static TimeLogData PlayerChat;` |
| 3595 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 300 | 2 | NPCs | Terraria.TimeLogger.TimeLogData | `public static TimeLogData NPCs;` | `public static TimeLogData NPCs;` |
| 3596 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 302 | 2 | Projectiles | Terraria.TimeLogger.TimeLogData | `public static TimeLogData Projectiles;` | `public static TimeLogData Projectiles;` |
| 3597 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 304 | 2 | Players | Terraria.TimeLogger.TimeLogData | `public static TimeLogData Players;` | `public static TimeLogData Players;` |
| 3598 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 306 | 2 | Items | Terraria.TimeLogger.TimeLogData | `public static TimeLogData Items;` | `public static TimeLogData Items;` |
| 3599 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 308 | 2 | Rain | Terraria.TimeLogger.TimeLogData | `public static TimeLogData Rain;` | `public static TimeLogData Rain;` |
| 3600 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 310 | 2 | Gore | Terraria.TimeLogger.TimeLogData | `public static TimeLogData Gore;` | `public static TimeLogData Gore;` |
| 3601 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 312 | 2 | Dust | Terraria.TimeLogger.TimeLogData | `public static TimeLogData Dust;` | `public static TimeLogData Dust;` |
| 3602 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 314 | 2 | Particles | Terraria.TimeLogger.TimeLogData | `public static TimeLogData Particles;` | `public static TimeLogData Particles;` |
| 3603 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 316 | 2 | LeashedEntities | Terraria.TimeLogger.TimeLogData | `public static TimeLogData LeashedEntities;` | `public static TimeLogData LeashedEntities;` |
| 3604 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 318 | 2 | Interface | Terraria.TimeLogger.TimeLogData | `public static TimeLogData Interface;` | `public static TimeLogData Interface;` |
| 3605 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 320 | 2 | DrawFPSGraph | Terraria.TimeLogger.TimeLogData | `public static TimeLogData DrawFPSGraph;` | `public static TimeLogData DrawFPSGraph;` |
| 3606 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 322 | 2 | DrawTimeLogger | Terraria.TimeLogger.TimeLogData | `public static TimeLogData DrawTimeLogger;` | `public static TimeLogData DrawTimeLogger;` |
| 3607 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 324 | 2 | Overlays | Terraria.TimeLogger.TimeLogData | `public static TimeLogData Overlays;` | `public static TimeLogData Overlays;` |
| 3608 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 326 | 2 | Filters | Terraria.TimeLogger.TimeLogData | `public static TimeLogData Filters;` | `public static TimeLogData Filters;` |
| 3609 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 328 | 2 | SunVisibility | Terraria.TimeLogger.TimeLogData | `public static TimeLogData SunVisibility;` | `public static TimeLogData SunVisibility;` |
| 3610 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 330 | 2 | MenuDrawTime | Terraria.TimeLogger.TimeLogData | `public static TimeLogData MenuDrawTime;` | `public static TimeLogData MenuDrawTime;` |
| 3611 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 332 | 2 | SplashDrawTime | Terraria.TimeLogger.TimeLogData | `public static TimeLogData SplashDrawTime;` | `public static TimeLogData SplashDrawTime;` |
| 3612 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 334 | 2 | DrawFullscreenMap | Terraria.TimeLogger.TimeLogData | `public static TimeLogData DrawFullscreenMap;` | `public static TimeLogData DrawFullscreenMap;` |
| 3613 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 336 | 2 | GCPause | Terraria.TimeLogger.TimeLogData | `public static TimeLogData GCPause;` | `public static TimeLogData GCPause;` |

#### 属性（0）

无该类型成员记录。


### 4.20 细分子系统：`SharedTimeLoggerTileAndLiquidRenderMetricsState`

- 原报告章节：`4.9.204`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`20` / `诊断、工具与共享机制`

- 上一级基线细分子系统：`SharedTimeLoggerCoordinatorState`
- 上一级 peer 细分子系统：`SharedTimeLoggerWorldRenderPhaseMetricsState`
- 细分职责：TimeLogger 固体、液体、墙体、线和 Tile 附加绘制阶段指标。
- 边界角色：`diagnostics state`；最小 seam：time logger tile liquid render metrics port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：24；属性：0；合计：24。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（24）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3548 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 206 | 2 | TotalDrawAndUpdate | Terraria.TimeLogger.TimeLogData | `public static TimeLogData TotalDrawAndUpdate;` | `public static TimeLogData TotalDrawAndUpdate;` |
| 3549 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 208 | 2 | DrawSolidTiles | Terraria.TimeLogger.TimeLogData | `public static TimeLogData DrawSolidTiles;` | `public static TimeLogData DrawSolidTiles;` |
| 3550 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 210 | 2 | FlushSolidTiles | Terraria.TimeLogger.TimeLogData | `public static TimeLogData FlushSolidTiles;` | `public static TimeLogData FlushSolidTiles;` |
| 3551 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 212 | 2 | SolidDrawCalls | Terraria.TimeLogger.TimeLogData | `public static TimeLogData SolidDrawCalls;` | `public static TimeLogData SolidDrawCalls;` |
| 3552 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 214 | 2 | DrawNonSolidTiles | Terraria.TimeLogger.TimeLogData | `public static TimeLogData DrawNonSolidTiles;` | `public static TimeLogData DrawNonSolidTiles;` |
| 3553 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 216 | 2 | FlushNonSolidTiles | Terraria.TimeLogger.TimeLogData | `public static TimeLogData FlushNonSolidTiles;` | `public static TimeLogData FlushNonSolidTiles;` |
| 3554 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 218 | 2 | NonSolidDrawCalls | Terraria.TimeLogger.TimeLogData | `public static TimeLogData NonSolidDrawCalls;` | `public static TimeLogData NonSolidDrawCalls;` |
| 3555 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 220 | 2 | DrawBlackTiles | Terraria.TimeLogger.TimeLogData | `public static TimeLogData DrawBlackTiles;` | `public static TimeLogData DrawBlackTiles;` |
| 3556 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 222 | 2 | DrawWallTiles | Terraria.TimeLogger.TimeLogData | `public static TimeLogData DrawWallTiles;` | `public static TimeLogData DrawWallTiles;` |
| 3557 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 224 | 2 | FlushWallTiles | Terraria.TimeLogger.TimeLogData | `public static TimeLogData FlushWallTiles;` | `public static TimeLogData FlushWallTiles;` |
| 3558 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 226 | 2 | WallDrawCalls | Terraria.TimeLogger.TimeLogData | `public static TimeLogData WallDrawCalls;` | `public static TimeLogData WallDrawCalls;` |
| 3559 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 228 | 2 | DrawWaterTiles | Terraria.TimeLogger.TimeLogData | `public static TimeLogData DrawWaterTiles;` | `public static TimeLogData DrawWaterTiles;` |
| 3560 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 230 | 2 | LiquidDrawCalls | Terraria.TimeLogger.TimeLogData | `public static TimeLogData LiquidDrawCalls;` | `public static TimeLogData LiquidDrawCalls;` |
| 3561 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 232 | 2 | DrawBackgroundWaterTiles | Terraria.TimeLogger.TimeLogData | `public static TimeLogData DrawBackgroundWaterTiles;` | `public static TimeLogData DrawBackgroundWaterTiles;` |
| 3562 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 234 | 2 | LiquidBackgroundDrawCalls | Terraria.TimeLogger.TimeLogData | `public static TimeLogData LiquidBackgroundDrawCalls;` | `public static TimeLogData LiquidBackgroundDrawCalls;` |
| 3565 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 240 | 2 | DrawWireTiles | Terraria.TimeLogger.TimeLogData | `public static TimeLogData DrawWireTiles;` | `public static TimeLogData DrawWireTiles;` |
| 3566 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 242 | 2 | ClothingRacks | Terraria.TimeLogger.TimeLogData | `public static TimeLogData ClothingRacks;` | `public static TimeLogData ClothingRacks;` |
| 3567 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 244 | 2 | TileExtras | Terraria.TimeLogger.TimeLogData | `public static TimeLogData TileExtras;` | `public static TimeLogData TileExtras;` |
| 3568 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 246 | 2 | Nature | Terraria.TimeLogger.TimeLogData | `public static TimeLogData Nature;` | `public static TimeLogData Nature;` |
| 3569 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 248 | 2 | RenderSolidTiles | Terraria.TimeLogger.TimeLogData | `public static TimeLogData RenderSolidTiles;` | `public static TimeLogData RenderSolidTiles;` |
| 3570 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 250 | 2 | RenderNonSolidTiles | Terraria.TimeLogger.TimeLogData | `public static TimeLogData RenderNonSolidTiles;` | `public static TimeLogData RenderNonSolidTiles;` |
| 3571 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 252 | 2 | RenderBlacksAndWalls | Terraria.TimeLogger.TimeLogData | `public static TimeLogData RenderBlacksAndWalls;` | `public static TimeLogData RenderBlacksAndWalls;` |
| 3573 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 256 | 2 | RenderBackgroundLiquid | Terraria.TimeLogger.TimeLogData | `public static TimeLogData RenderBackgroundLiquid;` | `public static TimeLogData RenderBackgroundLiquid;` |
| 3574 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 258 | 2 | RenderLiquid | Terraria.TimeLogger.TimeLogData | `public static TimeLogData RenderLiquid;` | `public static TimeLogData RenderLiquid;` |

#### 属性（0）

无该类型成员记录。


### 4.21 细分子系统：`SharedTimeLoggerLightingMapAndBackgroundMetricsState`

- 原报告章节：`4.9.205`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`20` / `诊断、工具与共享机制`

- 上一级基线细分子系统：`SharedTimeLoggerCoordinatorState`
- 上一级 peer 细分子系统：`SharedTimeLoggerWorldRenderPhaseMetricsState`
- 细分职责：TimeLogger 光照、地图、瀑布、天空和背景阶段指标。
- 边界角色：`diagnostics state`；最小 seam：time logger lighting map background metrics port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：22；属性：0；合计：22。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（22）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3563 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 236 | 2 | DrawUndergroundBackground | Terraria.TimeLogger.TimeLogData | `public static TimeLogData DrawUndergroundBackground;` | `public static TimeLogData DrawUndergroundBackground;` |
| 3564 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 238 | 2 | DrawOldUndergroundBackground | Terraria.TimeLogger.TimeLogData | `public static TimeLogData DrawOldUndergroundBackground;` | `public static TimeLogData DrawOldUndergroundBackground;` |
| 3572 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 254 | 2 | RenderUndergroundBackground | Terraria.TimeLogger.TimeLogData | `public static TimeLogData RenderUndergroundBackground;` | `public static TimeLogData RenderUndergroundBackground;` |
| 3575 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 260 | 2 | TotalDrawByRenderCount | Terraria.TimeLogger.TimeLogData[] | `public static TimeLogData[] TotalDrawByRenderCount;` | `public static TimeLogData[] TotalDrawByRenderCount;` |
| 3576 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 262 | 2 | TotalDrawRenderNow | Terraria.TimeLogger.TimeLogData | `public static TimeLogData TotalDrawRenderNow;` | `public static TimeLogData TotalDrawRenderNow;` |
| 3577 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 264 | 2 | TotalDraw | Terraria.TimeLogger.TimeLogData | `public static TimeLogData TotalDraw;` | `public static TimeLogData TotalDraw;` |
| 3578 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 266 | 2 | Lighting | Terraria.TimeLogger.TimeLogData | `public static TimeLogData Lighting;` | `public static TimeLogData Lighting;` |
| 3579 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 268 | 2 | LightingInit | Terraria.TimeLogger.TimeLogData | `public static TimeLogData LightingInit;` | `public static TimeLogData LightingInit;` |
| 3580 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 270 | 2 | LightingByPass | Terraria.TimeLogger.TimeLogData[] | `public static TimeLogData[] LightingByPass;` | `public static TimeLogData[] LightingByPass;` |
| 3581 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 272 | 2 | FindPaintedTiles | Terraria.TimeLogger.TimeLogData | `public static TimeLogData FindPaintedTiles;` | `public static TimeLogData FindPaintedTiles;` |
| 3582 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 274 | 2 | PrepareRequests | Terraria.TimeLogger.TimeLogData | `public static TimeLogData PrepareRequests;` | `public static TimeLogData PrepareRequests;` |
| 3583 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 276 | 2 | FindingWaterfalls | Terraria.TimeLogger.TimeLogData | `public static TimeLogData FindingWaterfalls;` | `public static TimeLogData FindingWaterfalls;` |
| 3584 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 278 | 2 | MapChanges | Terraria.TimeLogger.TimeLogData | `public static TimeLogData MapChanges;` | `public static TimeLogData MapChanges;` |
| 3585 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 280 | 2 | MapSectionUpdate | Terraria.TimeLogger.TimeLogData | `public static TimeLogData MapSectionUpdate;` | `public static TimeLogData MapSectionUpdate;` |
| 3586 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 282 | 2 | MapUpdate | Terraria.TimeLogger.TimeLogData | `public static TimeLogData MapUpdate;` | `public static TimeLogData MapUpdate;` |
| 3587 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 284 | 2 | SectionFraming | Terraria.TimeLogger.TimeLogData | `public static TimeLogData SectionFraming;` | `public static TimeLogData SectionFraming;` |
| 3588 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 286 | 2 | SectionRefresh | Terraria.TimeLogger.TimeLogData | `public static TimeLogData SectionRefresh;` | `public static TimeLogData SectionRefresh;` |
| 3589 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 288 | 2 | SkyBackground | Terraria.TimeLogger.TimeLogData | `public static TimeLogData SkyBackground;` | `public static TimeLogData SkyBackground;` |
| 3590 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 290 | 2 | SunMoonStars | Terraria.TimeLogger.TimeLogData | `public static TimeLogData SunMoonStars;` | `public static TimeLogData SunMoonStars;` |
| 3591 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 292 | 2 | SurfaceBackground | Terraria.TimeLogger.TimeLogData | `public static TimeLogData SurfaceBackground;` | `public static TimeLogData SurfaceBackground;` |
| 3592 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 294 | 2 | Map | Terraria.TimeLogger.TimeLogData | `public static TimeLogData Map;` | `public static TimeLogData Map;` |
| 3594 | field | Terraria.TimeLogger | Terraria/TimeLogger.cs | D:\TRbackup\Version4\Terraria\TimeLogger.cs | 298 | 2 | Waterfalls | Terraria.TimeLogger.TimeLogData | `public static TimeLogData Waterfalls;` | `public static TimeLogData Waterfalls;` |

#### 属性（0）

无该类型成员记录。


### 4.22 细分子系统：`SharedIssueReportCatalogState`

- 原报告章节：`4.9.222`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`20` / `诊断、工具与共享机制`

- 上一级基线细分子系统：`SharedStartupAndIssueReporting`
- 上一级 peer 细分子系统：`SharedStartupAndIssueReporting`
- 细分职责：问题报告集合、报告时间和报告正文。
- 边界角色：`diagnostics/adapter`；最小 seam：issue report catalog port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：2；声明类型数：2；字段：3；属性：0；合计：3。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（3）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1185 | field | Terraria.DataStructures.GeneralIssueReporter | Terraria.DataStructures/GeneralIssueReporter.cs | D:\TRbackup\Version4\Terraria.DataStructures\GeneralIssueReporter.cs | 7 | 2 | _reports | System.Collections.Generic.List<Terraria.DataStructures.IssueReport> | `private List<IssueReport> _reports = new List<IssueReport>();` | `private List<IssueReport> _reports = new List<IssueReport>();` |
| 1186 | field | Terraria.DataStructures.IssueReport | Terraria.DataStructures/IssueReport.cs | D:\TRbackup\Version4\Terraria.DataStructures\IssueReport.cs | 7 | 2 | timeReported | System.DateTime | `public DateTime timeReported;` | `public DateTime timeReported;` |
| 1187 | field | Terraria.DataStructures.IssueReport | Terraria.DataStructures/IssueReport.cs | D:\TRbackup\Version4\Terraria.DataStructures\IssueReport.cs | 9 | 2 | reportText | string | `public string reportText;` | `public string reportText;` |

#### 属性（0）

无该类型成员记录。


## 5. 追溯与验收

> 来源报告 SHA-256：`b944441d92d5103f218b766a3e354200794528a19e65a862e44c2b40a36f1196`
- 本分区细分组数：22；成员数：250；字段：226；属性：24。
- 预期不变量：本文件只出现完整细分子系统；不跨分区复制成员；所有成员的来源序号、原始声明、路径、行列和 C# 类型必须与来源报告一致。
- 验证方式：重新读取 20 个文件，检查文件数、细分组唯一性、来源序号恰好覆盖 `1..4542`、字段/属性合计和逐成员行文本一致；本次分区生成不运行 `dotnet`，因为没有修改 C# 或项目文件。

生成器：`Build/Tools/Generate-Version4NonAuthoritativeComponentPartitionReports.ps1`。
