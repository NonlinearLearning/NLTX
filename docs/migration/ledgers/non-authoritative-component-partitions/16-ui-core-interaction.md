# Version4 非权威组件拆分分区 16/20：UI 核心与交互

> 来源报告：`docs/migration/ledgers/Version4非权威模拟系统字段属性逐成员源码声明-更细子系统拆分-去除ID类文件.md`
> 来源报告 SHA-256：`b944441d92d5103f218b766a3e354200794528a19e65a862e44c2b40a36f1196`
> 本文件是并行组件拆分工作包，不是新的权威成员清单；成员声明事实直接保留来源报告原文。

## 1. 分区定位与评估

- 分区范围：UI 树、布局、事件、指针、内容界面、世界交互界面和进度容器。
- 本分区组件化重点：保持 UI 表现单向消费领域状态，避免 UI 树或事件 payload 反向成为权威状态。
- 本分区包含 11 个完整细分子系统、176 条成员记录（字段 154、属性 22）。来源序号覆盖区间 `2245..4542`，区间可能与其他分区交错，不以序号定义领域边界。
- 组件边界判定：细分子系统只是并行处理的最小库存边界，不等价于一个组件。必须继续按共同读者/写者、变更原因、生命周期、权威性和副作用分为 Component、System、Query、Command、Adapter 或 Projection。
- 证据状态：成员声明和统计已由来源报告校验；逐成员读者、写者、初始化/清理路径、持久化/网络语义和系统顺序仍需本分区会话补证，不得从名称或数量推断。

## 2. 分区统计

| 父级子系统 | 细分子系统数 | 字段 | 属性 | 合计 |
|---|---:|---:|---:|---:|
| `ClientPresentationAndTools` | 5 | 66 | 5 | 71 |
| `SharedRuntimeMechanisms` | 6 | 88 | 17 | 105 |

| 原报告章节 | 父级 | 细分子系统 | 边界角色 | 字段 | 属性 | 合计 | 组件化处理 |
|---|---|---|---|---:|---:|---:|---|
| `4.9.45` | `SharedRuntimeMechanisms` | `SharedContentUiScreens` | presentation | 8 | 0 | 8 | 待按成员访问模式拆分 |
| `4.9.46` | `SharedRuntimeMechanisms` | `SharedContentUiCurrency` | presentation | 7 | 0 | 7 | 待按成员访问模式拆分 |
| `4.9.47` | `SharedRuntimeMechanisms` | `SharedWorldInteractionUi` | presentation | 25 | 0 | 25 | 待按成员访问模式拆分 |
| `4.9.117` | `SharedRuntimeMechanisms` | `SharedContentUiContainersAndProgress` | presentation | 20 | 3 | 23 | 待按成员访问模式拆分 |
| `4.9.161` | `SharedRuntimeMechanisms` | `SharedContentUiOptionButtonState` | presentation/state | 19 | 6 | 25 | 待按成员访问模式拆分 |
| `4.9.162` | `SharedRuntimeMechanisms` | `SharedContentUiTextAndHeaderState` | presentation | 9 | 8 | 17 | 待按成员访问模式拆分 |
| `4.11.9` | `ClientPresentationAndTools` | `UiLayoutPrimitives` | presentation/value object | 11 | 1 | 12 | 待按成员访问模式拆分 |
| `4.11.10` | `ClientPresentationAndTools` | `UiEventPayloads` | presentation/event | 3 | 0 | 3 | 待按成员访问模式拆分 |
| `4.11.11` | `ClientPresentationAndTools` | `UiInputPointerState` | presentation/state | 20 | 0 | 20 | 待按成员访问模式拆分 |
| `4.11.17` | `ClientPresentationAndTools` | `UiElementLayoutAndDimensionsState` | presentation | 22 | 2 | 24 | 待按成员访问模式拆分 |
| `4.11.18` | `ClientPresentationAndTools` | `UiElementInteractionAndLifecycleState` | presentation | 10 | 2 | 12 | 待按成员访问模式拆分 |

## 3. 并行组件拆分契约

对本文件中的每个细分子系统和每条成员记录执行以下判断，并把结果写入对应的实现阶段设计/审计记录：

- 盘点声明类型、可见性、读者、写者、创建/更新/清理/持久化生命周期、访问频率和副作用；缺证据标记 `missing`，不可用命名猜测补齐。
- 标记权威状态、派生值、缓存、快照、兼容字段和表现数据；Component 只保存一个内聚概念的权威数据。
- 需要行为转换时放入显式 System/Command；纯资格判断和派生计算放入 Query；外部协议、文件、平台和第三方类型放入 Adapter；网络/持久化/日志/UI 输出放入单向 Projection。
- 分别建模实体引用、持久化 ID、网络 ID 和外部 ID；不得把 ID 类文件或外部对象直接塞入共享组件。
- 记录 Interface、Implementation、Seam、Depth、Leverage、Locality，以及必要的系统先后约束、失败归属和重试策略。

## 4. 逐成员源码声明

以下细分子系统块保持来源报告的成员表、声明、路径、行列和来源序号；只调整 Markdown 标题层级并增加分区追踪元数据。

### 4.1 细分子系统：`SharedContentUiScreens`

- 原报告章节：`4.9.45`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`16` / `UI 核心与交互`

- 上一级基线细分子系统：`SharedContentUiScreens`
- 细分职责：世界加载和世界选择界面状态。
- 边界角色：`presentation`；最小 seam：content screen view；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：2；声明类型数：2；字段：8；属性：0；合计：8。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（8）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2293 | field | Terraria.GameContent.UI.States.UIWorldLoad | Terraria.GameContent.UI.States/UIWorldLoad.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.States\UIWorldLoad.cs | 18 | 2 | _progressBar | Terraria.GameContent.UI.Elements.UIGenProgressBar | `private UIGenProgressBar _progressBar = new UIGenProgressBar();` | `private UIGenProgressBar _progressBar = new UIGenProgressBar();` |
| 2294 | field | Terraria.GameContent.UI.States.UIWorldLoad | Terraria.GameContent.UI.States/UIWorldLoad.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.States\UIWorldLoad.cs | 20 | 2 | _progressMessage | Terraria.GameContent.UI.Elements.UIHeader | `private UIHeader _progressMessage = new UIHeader();` | `private UIHeader _progressMessage = new UIHeader();` |
| 2295 | field | Terraria.GameContent.UI.States.UIWorldSelect | Terraria.GameContent.UI.States/UIWorldSelect.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.States\UIWorldSelect.cs | 20 | 2 | NewlyGeneratedWorld | Terraria.IO.WorldFileData | `public static WorldFileData NewlyGeneratedWorld;` | `public static WorldFileData NewlyGeneratedWorld;` |
| 2296 | field | Terraria.GameContent.UI.States.UIWorldSelect | Terraria.GameContent.UI.States/UIWorldSelect.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.States\UIWorldSelect.cs | 22 | 2 | _worldList | Terraria.GameContent.UI.Elements.UIList | `private UIList _worldList;` | `private UIList _worldList;` |
| 2297 | field | Terraria.GameContent.UI.States.UIWorldSelect | Terraria.GameContent.UI.States/UIWorldSelect.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.States\UIWorldSelect.cs | 24 | 2 | _containerPanel | Terraria.GameContent.UI.Elements.UIPanel | `private UIPanel _containerPanel;` | `private UIPanel _containerPanel;` |
| 2298 | field | Terraria.GameContent.UI.States.UIWorldSelect | Terraria.GameContent.UI.States/UIWorldSelect.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.States\UIWorldSelect.cs | 26 | 2 | _scrollbar | Terraria.GameContent.UI.Elements.UIScrollbar | `private UIScrollbar _scrollbar;` | `private UIScrollbar _scrollbar;` |
| 2299 | field | Terraria.GameContent.UI.States.UIWorldSelect | Terraria.GameContent.UI.States/UIWorldSelect.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.States\UIWorldSelect.cs | 28 | 2 | _isScrollbarAttached | bool | `private bool _isScrollbarAttached;` | `private bool _isScrollbarAttached;` |
| 2300 | field | Terraria.GameContent.UI.States.UIWorldSelect | Terraria.GameContent.UI.States/UIWorldSelect.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.States\UIWorldSelect.cs | 30 | 2 | favoritesCache | System.Collections.Generic.List<System.Tuple<string, bool>> | `private List<Tuple<string, bool>> favoritesCache = new List<Tuple<string, bool>>();` | `private List<Tuple<string, bool>> favoritesCache = new List<Tuple<string, bool>>();` |

#### 属性（0）

无该类型成员记录。


### 4.2 细分子系统：`SharedContentUiCurrency`

- 原报告章节：`4.9.46`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`16` / `UI 核心与交互`

- 上一级基线细分子系统：`SharedContentUiCurrency`
- 细分职责：自定义货币 UI 状态。
- 边界角色：`presentation`；最小 seam：currency UI view；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：3；声明类型数：3；字段：7；属性：0；合计：7。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（7）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2301 | field | Terraria.GameContent.UI.CustomCurrencyManager | Terraria.GameContent.UI/CustomCurrencyManager.cs | D:\TRbackup\Version4\Terraria.GameContent.UI\CustomCurrencyManager.cs | 10 | 2 | _nextCurrencyIndex | int | `private static int _nextCurrencyIndex;` | `private static int _nextCurrencyIndex;` |
| 2302 | field | Terraria.GameContent.UI.CustomCurrencyManager | Terraria.GameContent.UI/CustomCurrencyManager.cs | D:\TRbackup\Version4\Terraria.GameContent.UI\CustomCurrencyManager.cs | 12 | 2 | _currencies | System.Collections.Generic.Dictionary<int, Terraria.GameContent.UI.CustomCurrencySystem> | `private static Dictionary<int, CustomCurrencySystem> _currencies = new Dictionary<int, CustomCurrencySystem>();` | `private static Dictionary<int, CustomCurrencySystem> _currencies = new Dictionary<int, CustomCurrencySystem>();` |
| 2303 | field | Terraria.GameContent.UI.CustomCurrencySingleCoin | Terraria.GameContent.UI/CustomCurrencySingleCoin.cs | D:\TRbackup\Version4\Terraria.GameContent.UI\CustomCurrencySingleCoin.cs | 14 | 2 | CurrencyDrawScale | float | `public float CurrencyDrawScale = 0.8f;` | `public float CurrencyDrawScale = 0.8f;` |
| 2304 | field | Terraria.GameContent.UI.CustomCurrencySingleCoin | Terraria.GameContent.UI/CustomCurrencySingleCoin.cs | D:\TRbackup\Version4\Terraria.GameContent.UI\CustomCurrencySingleCoin.cs | 16 | 2 | CurrencyTextKey | string | `public string CurrencyTextKey = "Currency.DefenderMedals";` | `public string CurrencyTextKey = "Currency.DefenderMedals";` |
| 2305 | field | Terraria.GameContent.UI.CustomCurrencySingleCoin | Terraria.GameContent.UI/CustomCurrencySingleCoin.cs | D:\TRbackup\Version4\Terraria.GameContent.UI\CustomCurrencySingleCoin.cs | 18 | 2 | CurrencyTextColor | Color | `public Color CurrencyTextColor = new Color(240, 100, 120);` | `public Color CurrencyTextColor = new Color(240, 100, 120);` |
| 2306 | field | Terraria.GameContent.UI.CustomCurrencySystem | Terraria.GameContent.UI/CustomCurrencySystem.cs | D:\TRbackup\Version4\Terraria.GameContent.UI\CustomCurrencySystem.cs | 10 | 2 | _valuePerUnit | System.Collections.Generic.Dictionary<int, int> | `protected Dictionary<int, int> _valuePerUnit = new Dictionary<int, int>();` | `protected Dictionary<int, int> _valuePerUnit = new Dictionary<int, int>();` |
| 2307 | field | Terraria.GameContent.UI.CustomCurrencySystem | Terraria.GameContent.UI/CustomCurrencySystem.cs | D:\TRbackup\Version4\Terraria.GameContent.UI\CustomCurrencySystem.cs | 12 | 2 | _currencyCap | long | `private long _currencyCap = 999999999L;` | `private long _currencyCap = 999999999L;` |

#### 属性（0）

无该类型成员记录。


### 4.3 细分子系统：`SharedWorldInteractionUi`

- 原报告章节：`4.9.47`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`16` / `UI 核心与交互`

- 上一级基线细分子系统：`SharedWorldInteractionUi`
- 细分职责：表情、线路、世界锚点和物品稀有度界面。
- 边界角色：`presentation`；最小 seam：world interaction UI；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：4；声明类型数：6；字段：25；属性：0；合计：25。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（25）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2308 | field | Terraria.GameContent.UI.EmoteBubble | Terraria.GameContent.UI/EmoteBubble.cs | D:\TRbackup\Version4\Terraria.GameContent.UI\EmoteBubble.cs | 13 | 2 | byID | System.Collections.Generic.Dictionary<int, Terraria.GameContent.UI.EmoteBubble> | `public static Dictionary<int, EmoteBubble> byID = new Dictionary<int, EmoteBubble>();` | `public static Dictionary<int, EmoteBubble> byID = new Dictionary<int, EmoteBubble>();` |
| 2309 | field | Terraria.GameContent.UI.EmoteBubble | Terraria.GameContent.UI/EmoteBubble.cs | D:\TRbackup\Version4\Terraria.GameContent.UI\EmoteBubble.cs | 15 | 2 | toClean | System.Collections.Generic.List<int> | `private static List<int> toClean = new List<int>();` | `private static List<int> toClean = new List<int>();` |
| 2310 | field | Terraria.GameContent.UI.EmoteBubble | Terraria.GameContent.UI/EmoteBubble.cs | D:\TRbackup\Version4\Terraria.GameContent.UI\EmoteBubble.cs | 17 | 2 | NextID | int | `public static int NextID;` | `public static int NextID;` |
| 2311 | field | Terraria.GameContent.UI.EmoteBubble | Terraria.GameContent.UI/EmoteBubble.cs | D:\TRbackup\Version4\Terraria.GameContent.UI\EmoteBubble.cs | 19 | 2 | ID | int | `public int ID;` | `public int ID;` |
| 2312 | field | Terraria.GameContent.UI.EmoteBubble | Terraria.GameContent.UI/EmoteBubble.cs | D:\TRbackup\Version4\Terraria.GameContent.UI\EmoteBubble.cs | 21 | 2 | anchor | Terraria.GameContent.UI.WorldUIAnchor | `public WorldUIAnchor anchor;` | `public WorldUIAnchor anchor;` |
| 2313 | field | Terraria.GameContent.UI.EmoteBubble | Terraria.GameContent.UI/EmoteBubble.cs | D:\TRbackup\Version4\Terraria.GameContent.UI\EmoteBubble.cs | 23 | 2 | lifeTime | int | `public int lifeTime;` | `public int lifeTime;` |
| 2314 | field | Terraria.GameContent.UI.EmoteBubble | Terraria.GameContent.UI/EmoteBubble.cs | D:\TRbackup\Version4\Terraria.GameContent.UI\EmoteBubble.cs | 25 | 2 | lifeTimeStart | int | `public int lifeTimeStart;` | `public int lifeTimeStart;` |
| 2315 | field | Terraria.GameContent.UI.EmoteBubble | Terraria.GameContent.UI/EmoteBubble.cs | D:\TRbackup\Version4\Terraria.GameContent.UI\EmoteBubble.cs | 27 | 2 | emote | int | `public int emote;` | `public int emote;` |
| 2316 | field | Terraria.GameContent.UI.EmoteBubble | Terraria.GameContent.UI/EmoteBubble.cs | D:\TRbackup\Version4\Terraria.GameContent.UI\EmoteBubble.cs | 29 | 2 | metadata | int | `public int metadata;` | `public int metadata;` |
| 2317 | field | Terraria.GameContent.UI.EmoteBubble | Terraria.GameContent.UI/EmoteBubble.cs | D:\TRbackup\Version4\Terraria.GameContent.UI\EmoteBubble.cs | 31 | 2 | frameSpeed | int | `private const int frameSpeed = 8;` | `private const int frameSpeed = 8;` |
| 2318 | field | Terraria.GameContent.UI.EmoteBubble | Terraria.GameContent.UI/EmoteBubble.cs | D:\TRbackup\Version4\Terraria.GameContent.UI\EmoteBubble.cs | 33 | 2 | frameCounter | int | `public int frameCounter;` | `public int frameCounter;` |
| 2319 | field | Terraria.GameContent.UI.EmoteBubble | Terraria.GameContent.UI/EmoteBubble.cs | D:\TRbackup\Version4\Terraria.GameContent.UI\EmoteBubble.cs | 35 | 2 | frame | int | `public int frame;` | `public int frame;` |
| 2320 | field | Terraria.GameContent.UI.EmoteBubble | Terraria.GameContent.UI/EmoteBubble.cs | D:\TRbackup\Version4\Terraria.GameContent.UI\EmoteBubble.cs | 37 | 2 | EMOTE_SHEET_HORIZONTAL_FRAMES | int | `public const int EMOTE_SHEET_HORIZONTAL_FRAMES = 8;` | `public const int EMOTE_SHEET_HORIZONTAL_FRAMES = 8;` |
| 2321 | field | Terraria.GameContent.UI.EmoteBubble | Terraria.GameContent.UI/EmoteBubble.cs | D:\TRbackup\Version4\Terraria.GameContent.UI\EmoteBubble.cs | 39 | 2 | EMOTE_SHEET_EMOTES_PER_ROW | int | `public const int EMOTE_SHEET_EMOTES_PER_ROW = 4;` | `public const int EMOTE_SHEET_EMOTES_PER_ROW = 4;` |
| 2322 | field | Terraria.GameContent.UI.EmoteBubble | Terraria.GameContent.UI/EmoteBubble.cs | D:\TRbackup\Version4\Terraria.GameContent.UI\EmoteBubble.cs | 41 | 2 | _temporaryBubble | Terraria.GameContent.UI.EmoteBubble | `private static EmoteBubble _temporaryBubble = new EmoteBubble(0, new WorldUIAnchor(), 0);` | `private static EmoteBubble _temporaryBubble = new EmoteBubble(0, new WorldUIAnchor(), 0);` |
| 2323 | field | Terraria.GameContent.UI.ItemRarity | Terraria.GameContent.UI/ItemRarity.cs | D:\TRbackup\Version4\Terraria.GameContent.UI\ItemRarity.cs | 9 | 2 | _rarities | System.Collections.Generic.Dictionary<int, Color> | `private static Dictionary<int, Color> _rarities = new Dictionary<int, Color>();` | `private static Dictionary<int, Color> _rarities = new Dictionary<int, Color>();` |
| 2324 | field | Terraria.GameContent.UI.WiresUI.Settings | Terraria.GameContent.UI/WiresUI.cs | D:\TRbackup\Version4\Terraria.GameContent.UI\WiresUI.cs | 23 | 3 | ToolMode | Terraria.GameContent.UI.WiresUI.Settings.MultiToolMode | `public static MultiToolMode ToolMode = MultiToolMode.Red;` | `public static MultiToolMode ToolMode = MultiToolMode.Red;` |
| 2325 | field | Terraria.GameContent.UI.WiresUI.WiresRadial | Terraria.GameContent.UI/WiresUI.cs | D:\TRbackup\Version4\Terraria.GameContent.UI\WiresUI.cs | 28 | 3 | position | Vector2 | `public Vector2 position;` | `public Vector2 position;` |
| 2326 | field | Terraria.GameContent.UI.WiresUI.WiresRadial | Terraria.GameContent.UI/WiresUI.cs | D:\TRbackup\Version4\Terraria.GameContent.UI\WiresUI.cs | 30 | 3 | active | bool | `public bool active;` | `public bool active;` |
| 2327 | field | Terraria.GameContent.UI.WiresUI.WiresRadial | Terraria.GameContent.UI/WiresUI.cs | D:\TRbackup\Version4\Terraria.GameContent.UI\WiresUI.cs | 32 | 3 | OnWiresMenu | bool | `public bool OnWiresMenu;` | `public bool OnWiresMenu;` |
| 2328 | field | Terraria.GameContent.UI.WiresUI | Terraria.GameContent.UI/WiresUI.cs | D:\TRbackup\Version4\Terraria.GameContent.UI\WiresUI.cs | 35 | 2 | radial | Terraria.GameContent.UI.WiresUI.WiresRadial | `private static WiresRadial radial = new WiresRadial();` | `private static WiresRadial radial = new WiresRadial();` |
| 2329 | field | Terraria.GameContent.UI.WorldUIAnchor | Terraria.GameContent.UI/WorldUIAnchor.cs | D:\TRbackup\Version4\Terraria.GameContent.UI\WorldUIAnchor.cs | 16 | 2 | type | Terraria.GameContent.UI.WorldUIAnchor.AnchorType | `public AnchorType type;` | `public AnchorType type;` |
| 2330 | field | Terraria.GameContent.UI.WorldUIAnchor | Terraria.GameContent.UI/WorldUIAnchor.cs | D:\TRbackup\Version4\Terraria.GameContent.UI\WorldUIAnchor.cs | 18 | 2 | entity | Terraria.Entity | `public Entity entity;` | `public Entity entity;` |
| 2331 | field | Terraria.GameContent.UI.WorldUIAnchor | Terraria.GameContent.UI/WorldUIAnchor.cs | D:\TRbackup\Version4\Terraria.GameContent.UI\WorldUIAnchor.cs | 20 | 2 | pos | Vector2 | `public Vector2 pos = Vector2.Zero;` | `public Vector2 pos = Vector2.Zero;` |
| 2332 | field | Terraria.GameContent.UI.WorldUIAnchor | Terraria.GameContent.UI/WorldUIAnchor.cs | D:\TRbackup\Version4\Terraria.GameContent.UI\WorldUIAnchor.cs | 22 | 2 | size | Vector2 | `public Vector2 size = Vector2.Zero;` | `public Vector2 size = Vector2.Zero;` |

#### 属性（0）

无该类型成员记录。


### 4.4 细分子系统：`SharedContentUiContainersAndProgress`

- 原报告章节：`4.9.117`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`16` / `UI 核心与交互`

- 上一级基线细分子系统：`SharedContentUiWidgets`
- 细分职责：内容 UI 面板、列表、滚动条和进度控件状态。
- 边界角色：`presentation`；最小 seam：content UI container progress port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：4；声明类型数：4；字段：20；属性：3；合计：23。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（20）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2264 | field | Terraria.GameContent.UI.Elements.UIGenProgressBar | Terraria.GameContent.UI.Elements/UIGenProgressBar.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Elements\UIGenProgressBar.cs | 11 | 2 | _targetOverallProgress | float | `private float _targetOverallProgress;` | `private float _targetOverallProgress;` |
| 2265 | field | Terraria.GameContent.UI.Elements.UIGenProgressBar | Terraria.GameContent.UI.Elements/UIGenProgressBar.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Elements\UIGenProgressBar.cs | 13 | 2 | _targetCurrentProgress | float | `private float _targetCurrentProgress;` | `private float _targetCurrentProgress;` |
| 2267 | field | Terraria.GameContent.UI.Elements.UIList | Terraria.GameContent.UI.Elements/UIList.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Elements\UIList.cs | 24 | 2 | _items | System.Collections.Generic.List<Terraria.UI.UIElement> | `protected List<UIElement> _items = new List<UIElement>();` | `protected List<UIElement> _items = new List<UIElement>();` |
| 2268 | field | Terraria.GameContent.UI.Elements.UIList | Terraria.GameContent.UI.Elements/UIList.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Elements\UIList.cs | 26 | 2 | _scrollbar | Terraria.GameContent.UI.Elements.UIScrollbar | `protected UIScrollbar _scrollbar;` | `protected UIScrollbar _scrollbar;` |
| 2269 | field | Terraria.GameContent.UI.Elements.UIList | Terraria.GameContent.UI.Elements/UIList.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Elements\UIList.cs | 28 | 2 | _innerList | Terraria.UI.UIElement | `private UIElement _innerList = new UIInnerList();` | `private UIElement _innerList = new UIInnerList();` |
| 2270 | field | Terraria.GameContent.UI.Elements.UIList | Terraria.GameContent.UI.Elements/UIList.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Elements\UIList.cs | 30 | 2 | ListPadding | float | `public float ListPadding = 5f;` | `public float ListPadding = 5f;` |
| 2271 | field | Terraria.GameContent.UI.Elements.UIList | Terraria.GameContent.UI.Elements/UIList.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Elements\UIList.cs | 32 | 2 | ManualSortMethod | System.Action<System.Collections.Generic.List<Terraria.UI.UIElement>> | `public Action<List<UIElement>> ManualSortMethod;` | `public Action<List<UIElement>> ManualSortMethod;` |
| 2272 | field | Terraria.GameContent.UI.Elements.UIPanel | Terraria.GameContent.UI.Elements/UIPanel.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Elements\UIPanel.cs | 10 | 2 | _cornerSize | int | `private int _cornerSize = 12;` | `private int _cornerSize = 12;` |
| 2273 | field | Terraria.GameContent.UI.Elements.UIPanel | Terraria.GameContent.UI.Elements/UIPanel.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Elements\UIPanel.cs | 12 | 2 | _barSize | int | `private int _barSize = 4;` | `private int _barSize = 4;` |
| 2274 | field | Terraria.GameContent.UI.Elements.UIPanel | Terraria.GameContent.UI.Elements/UIPanel.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Elements\UIPanel.cs | 14 | 2 | _borderTexture | Asset<Texture2D> | `private Asset<Texture2D> _borderTexture;` | `private Asset<Texture2D> _borderTexture;` |
| 2275 | field | Terraria.GameContent.UI.Elements.UIPanel | Terraria.GameContent.UI.Elements/UIPanel.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Elements\UIPanel.cs | 16 | 2 | _backgroundTexture | Asset<Texture2D> | `private Asset<Texture2D> _backgroundTexture;` | `private Asset<Texture2D> _backgroundTexture;` |
| 2276 | field | Terraria.GameContent.UI.Elements.UIPanel | Terraria.GameContent.UI.Elements/UIPanel.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Elements\UIPanel.cs | 18 | 2 | BorderColor | Color | `public Color BorderColor = Color.Black;` | `public Color BorderColor = Color.Black;` |
| 2277 | field | Terraria.GameContent.UI.Elements.UIPanel | Terraria.GameContent.UI.Elements/UIPanel.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Elements\UIPanel.cs | 20 | 2 | BackgroundColor | Color | `public Color BackgroundColor = new Color(63, 82, 151) * 0.7f;` | `public Color BackgroundColor = new Color(63, 82, 151) * 0.7f;` |
| 2278 | field | Terraria.GameContent.UI.Elements.UIScrollbar | Terraria.GameContent.UI.Elements/UIScrollbar.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Elements\UIScrollbar.cs | 17 | 2 | _viewPosition | float | `private float _viewPosition;` | `private float _viewPosition;` |
| 2279 | field | Terraria.GameContent.UI.Elements.UIScrollbar | Terraria.GameContent.UI.Elements/UIScrollbar.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Elements\UIScrollbar.cs | 19 | 2 | _viewSize | float | `private float _viewSize = 1f;` | `private float _viewSize = 1f;` |
| 2280 | field | Terraria.GameContent.UI.Elements.UIScrollbar | Terraria.GameContent.UI.Elements/UIScrollbar.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Elements\UIScrollbar.cs | 21 | 2 | _maxViewSize | float | `private float _maxViewSize = 20f;` | `private float _maxViewSize = 20f;` |
| 2281 | field | Terraria.GameContent.UI.Elements.UIScrollbar | Terraria.GameContent.UI.Elements/UIScrollbar.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Elements\UIScrollbar.cs | 23 | 2 | AutoHide | bool | `public bool AutoHide;` | `public bool AutoHide;` |
| 2282 | field | Terraria.GameContent.UI.Elements.UIScrollbar | Terraria.GameContent.UI.Elements/UIScrollbar.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Elements\UIScrollbar.cs | 25 | 2 | _texture | Asset<Texture2D> | `private Asset<Texture2D> _texture;` | `private Asset<Texture2D> _texture;` |
| 2283 | field | Terraria.GameContent.UI.Elements.UIScrollbar | Terraria.GameContent.UI.Elements/UIScrollbar.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Elements\UIScrollbar.cs | 27 | 2 | _innerTexture | Asset<Texture2D> | `private Asset<Texture2D> _innerTexture;` | `private Asset<Texture2D> _innerTexture;` |
| 2284 | field | Terraria.GameContent.UI.Elements.UIScrollbar | Terraria.GameContent.UI.Elements/UIScrollbar.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Elements\UIScrollbar.cs | 29 | 2 | _theme | Terraria.GameContent.UI.Elements.UIScrollbar.ColorTheme | `private ColorTheme _theme;` | `private ColorTheme _theme;` |

#### 属性（3）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3837 | property | Terraria.GameContent.UI.Elements.UIList | Terraria.GameContent.UI.Elements/UIList.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Elements\UIList.cs | 34 | 2 | Count | int | `public int Count => _items.Count;` | `public int Count => _items.Count;` |
| 3838 | property | Terraria.GameContent.UI.Elements.UIScrollbar | Terraria.GameContent.UI.Elements/UIScrollbar.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Elements\UIScrollbar.cs | 31 | 2 | ViewPosition | float | `public float ViewPosition { get { return _viewPosition; } set { _viewPosition = MathHelper.Clamp(value, 0f, _maxViewSize - _viewSize); } }` | `public float ViewPosition { get { return _viewPosition; } set { _viewPosition = MathHelper.Clamp(value, 0f, _maxViewSize - _viewSize); } }` |
| 3839 | property | Terraria.GameContent.UI.Elements.UIScrollbar | Terraria.GameContent.UI.Elements/UIScrollbar.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Elements\UIScrollbar.cs | 43 | 2 | CanScroll | bool | `public bool CanScroll => _maxViewSize != _viewSize;` | `public bool CanScroll => _maxViewSize != _viewSize;` |


### 4.5 细分子系统：`SharedContentUiOptionButtonState`

- 原报告章节：`4.9.161`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`16` / `UI 核心与交互`

- 上一级基线细分子系统：`SharedContentUiWidgets`
- 上一级 peer 细分子系统：`SharedContentUiTextAndOptionWidgets`
- 细分职责：内容 UI 组选项按钮的选项、纹理、颜色和选择状态。
- 边界角色：`presentation/state`；最小 seam：content option button port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：19；属性：6；合计：25。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（19）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2245 | field | Terraria.GameContent.UI.Elements.GroupOptionButton<T> | Terraria.GameContent.UI.Elements/GroupOptionButton.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Elements\GroupOptionButton.cs | 14 | 2 | _currentOption | T | `private T _currentOption;` | `private T _currentOption;` |
| 2246 | field | Terraria.GameContent.UI.Elements.GroupOptionButton<T> | Terraria.GameContent.UI.Elements/GroupOptionButton.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Elements\GroupOptionButton.cs | 16 | 2 | _BasePanelTexture | Asset<Texture2D> | `private readonly Asset<Texture2D> _BasePanelTexture;` | `private readonly Asset<Texture2D> _BasePanelTexture;` |
| 2247 | field | Terraria.GameContent.UI.Elements.GroupOptionButton<T> | Terraria.GameContent.UI.Elements/GroupOptionButton.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Elements\GroupOptionButton.cs | 18 | 2 | _selectedBorderTexture | Asset<Texture2D> | `private readonly Asset<Texture2D> _selectedBorderTexture;` | `private readonly Asset<Texture2D> _selectedBorderTexture;` |
| 2248 | field | Terraria.GameContent.UI.Elements.GroupOptionButton<T> | Terraria.GameContent.UI.Elements/GroupOptionButton.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Elements\GroupOptionButton.cs | 20 | 2 | _hoveredBorderTexture | Asset<Texture2D> | `private readonly Asset<Texture2D> _hoveredBorderTexture;` | `private readonly Asset<Texture2D> _hoveredBorderTexture;` |
| 2249 | field | Terraria.GameContent.UI.Elements.GroupOptionButton<T> | Terraria.GameContent.UI.Elements/GroupOptionButton.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Elements\GroupOptionButton.cs | 22 | 2 | _iconTexture | Asset<Texture2D> | `private Asset<Texture2D> _iconTexture;` | `private Asset<Texture2D> _iconTexture;` |
| 2250 | field | Terraria.GameContent.UI.Elements.GroupOptionButton<T> | Terraria.GameContent.UI.Elements/GroupOptionButton.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Elements\GroupOptionButton.cs | 24 | 2 | _myOption | T | `private readonly T _myOption;` | `private readonly T _myOption;` |
| 2251 | field | Terraria.GameContent.UI.Elements.GroupOptionButton<T> | Terraria.GameContent.UI.Elements/GroupOptionButton.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Elements\GroupOptionButton.cs | 26 | 2 | _color | Color | `private Color _color;` | `private Color _color;` |
| 2252 | field | Terraria.GameContent.UI.Elements.GroupOptionButton<T> | Terraria.GameContent.UI.Elements/GroupOptionButton.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Elements\GroupOptionButton.cs | 28 | 2 | _borderColor | Color | `private Color _borderColor;` | `private Color _borderColor;` |
| 2253 | field | Terraria.GameContent.UI.Elements.GroupOptionButton<T> | Terraria.GameContent.UI.Elements/GroupOptionButton.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Elements\GroupOptionButton.cs | 30 | 2 | FadeFromBlack | float | `public float FadeFromBlack = 1f;` | `public float FadeFromBlack = 1f;` |
| 2254 | field | Terraria.GameContent.UI.Elements.GroupOptionButton<T> | Terraria.GameContent.UI.Elements/GroupOptionButton.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Elements\GroupOptionButton.cs | 32 | 2 | InnerHighlightRim | int | `public int InnerHighlightRim = 7;` | `public int InnerHighlightRim = 7;` |
| 2255 | field | Terraria.GameContent.UI.Elements.GroupOptionButton<T> | Terraria.GameContent.UI.Elements/GroupOptionButton.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Elements\GroupOptionButton.cs | 34 | 2 | ShowHighlightWhenSelected | bool | `public bool ShowHighlightWhenSelected = true;` | `public bool ShowHighlightWhenSelected = true;` |
| 2256 | field | Terraria.GameContent.UI.Elements.GroupOptionButton<T> | Terraria.GameContent.UI.Elements/GroupOptionButton.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Elements\GroupOptionButton.cs | 36 | 2 | _overrideUnpickedColor | Color | `private Color _overrideUnpickedColor = Color.White;` | `private Color _overrideUnpickedColor = Color.White;` |
| 2257 | field | Terraria.GameContent.UI.Elements.GroupOptionButton<T> | Terraria.GameContent.UI.Elements/GroupOptionButton.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Elements\GroupOptionButton.cs | 38 | 2 | _overridePickedColor | Color | `private Color _overridePickedColor = Color.White;` | `private Color _overridePickedColor = Color.White;` |
| 2258 | field | Terraria.GameContent.UI.Elements.GroupOptionButton<T> | Terraria.GameContent.UI.Elements/GroupOptionButton.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Elements\GroupOptionButton.cs | 40 | 2 | Description | Terraria.Localization.LocalizedText | `public readonly LocalizedText Description;` | `public readonly LocalizedText Description;` |
| 2259 | field | Terraria.GameContent.UI.Elements.GroupOptionButton<T> | Terraria.GameContent.UI.Elements/GroupOptionButton.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Elements\GroupOptionButton.cs | 42 | 2 | _title | Terraria.GameContent.UI.Elements.UIText | `private UIText _title;` | `private UIText _title;` |
| 2260 | field | Terraria.GameContent.UI.Elements.GroupOptionButton<T> | Terraria.GameContent.UI.Elements/GroupOptionButton.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Elements\GroupOptionButton.cs | 44 | 2 | _iconScale | float | `private float _iconScale = 1f;` | `private float _iconScale = 1f;` |
| 2261 | field | Terraria.GameContent.UI.Elements.GroupOptionButton<T> | Terraria.GameContent.UI.Elements/GroupOptionButton.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Elements\GroupOptionButton.cs | 46 | 2 | _iconOffset | Vector2 | `private Vector2 _iconOffset;` | `private Vector2 _iconOffset;` |
| 2262 | field | Terraria.GameContent.UI.Elements.GroupOptionButton<T> | Terraria.GameContent.UI.Elements/GroupOptionButton.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Elements\GroupOptionButton.cs | 48 | 2 | _iconFrame | Rectangle? | `private Rectangle? _iconFrame;` | `private Rectangle? _iconFrame;` |
| 2263 | field | Terraria.GameContent.UI.Elements.GroupOptionButton<T> | Terraria.GameContent.UI.Elements/GroupOptionButton.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Elements\GroupOptionButton.cs | 50 | 2 | _iconColor | Color | `private Color _iconColor = Color.White;` | `private Color _iconColor = Color.White;` |

#### 属性（6）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3830 | property | Terraria.GameContent.UI.Elements.GroupOptionButton<T> | Terraria.GameContent.UI.Elements/GroupOptionButton.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Elements\GroupOptionButton.cs | 52 | 2 | OptionValue | T | `public T OptionValue => _myOption;` | `public T OptionValue => _myOption;` |
| 3831 | property | Terraria.GameContent.UI.Elements.GroupOptionButton<T> | Terraria.GameContent.UI.Elements/GroupOptionButton.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Elements\GroupOptionButton.cs | 54 | 2 | IsSelected | bool | `public bool IsSelected => EqualityComparer<T>.Default.Equals(_currentOption, _myOption);` | `public bool IsSelected => EqualityComparer<T>.Default.Equals(_currentOption, _myOption);` |
| 3832 | property | Terraria.GameContent.UI.Elements.GroupOptionButton<T> | Terraria.GameContent.UI.Elements/GroupOptionButton.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Elements\GroupOptionButton.cs | 56 | 2 | Icon | Texture2D | `public Texture2D Icon { get { if (_iconTexture == null) { return null; } return _iconTexture.Value; } }` | `public Texture2D Icon { get { if (_iconTexture == null) { return null; } return _iconTexture.Value; } }` |
| 3833 | property | Terraria.GameContent.UI.Elements.GroupOptionButton<T> | Terraria.GameContent.UI.Elements/GroupOptionButton.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Elements\GroupOptionButton.cs | 68 | 2 | IconScale | float | `public float IconScale { get { return _iconScale; } set { _iconScale = value; } }` | `public float IconScale { get { return _iconScale; } set { _iconScale = value; } }` |
| 3834 | property | Terraria.GameContent.UI.Elements.GroupOptionButton<T> | Terraria.GameContent.UI.Elements/GroupOptionButton.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Elements\GroupOptionButton.cs | 80 | 2 | IconOffset | Vector2 | `public Vector2 IconOffset { get { return _iconOffset; } set { _iconOffset = value; } }` | `public Vector2 IconOffset { get { return _iconOffset; } set { _iconOffset = value; } }` |
| 3835 | property | Terraria.GameContent.UI.Elements.GroupOptionButton<T> | Terraria.GameContent.UI.Elements/GroupOptionButton.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Elements\GroupOptionButton.cs | 92 | 2 | IconColor | Color | `public Color IconColor { get { return _iconColor; } set { _iconColor = value; } }` | `public Color IconColor { get { return _iconColor; } set { _iconColor = value; } }` |


### 4.6 细分子系统：`SharedContentUiTextAndHeaderState`

- 原报告章节：`4.9.162`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`16` / `UI 核心与交互`

- 上一级基线细分子系统：`SharedContentUiWidgets`
- 上一级 peer 细分子系统：`SharedContentUiTextAndOptionWidgets`
- 细分职责：内容 UI 文本和标题的文本、颜色、换行及布局状态。
- 边界角色：`presentation`；最小 seam：content text header port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：2；声明类型数：2；字段：9；属性：8；合计：17。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（9）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2266 | field | Terraria.GameContent.UI.Elements.UIHeader | Terraria.GameContent.UI.Elements/UIHeader.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Elements\UIHeader.cs | 10 | 2 | _text | string | `private string _text;` | `private string _text;` |
| 2285 | field | Terraria.GameContent.UI.Elements.UIText | Terraria.GameContent.UI.Elements/UIText.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Elements\UIText.cs | 15 | 2 | _text | object | `private object _text = "";` | `private object _text = "";` |
| 2286 | field | Terraria.GameContent.UI.Elements.UIText | Terraria.GameContent.UI.Elements/UIText.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Elements\UIText.cs | 17 | 2 | _textScale | float | `private float _textScale = 1f;` | `private float _textScale = 1f;` |
| 2287 | field | Terraria.GameContent.UI.Elements.UIText | Terraria.GameContent.UI.Elements/UIText.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Elements\UIText.cs | 19 | 2 | _textSize | Vector2 | `private Vector2 _textSize = Vector2.Zero;` | `private Vector2 _textSize = Vector2.Zero;` |
| 2288 | field | Terraria.GameContent.UI.Elements.UIText | Terraria.GameContent.UI.Elements/UIText.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Elements\UIText.cs | 21 | 2 | _isLarge | bool | `private bool _isLarge;` | `private bool _isLarge;` |
| 2289 | field | Terraria.GameContent.UI.Elements.UIText | Terraria.GameContent.UI.Elements/UIText.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Elements\UIText.cs | 23 | 2 | _color | Color | `private Color _color = Color.White;` | `private Color _color = Color.White;` |
| 2290 | field | Terraria.GameContent.UI.Elements.UIText | Terraria.GameContent.UI.Elements/UIText.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Elements\UIText.cs | 25 | 2 | _shadowColor | Color | `private Color _shadowColor = Color.Black;` | `private Color _shadowColor = Color.Black;` |
| 2291 | field | Terraria.GameContent.UI.Elements.UIText | Terraria.GameContent.UI.Elements/UIText.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Elements\UIText.cs | 27 | 2 | _isWrapped | bool | `private bool _isWrapped;` | `private bool _isWrapped;` |
| 2292 | field | Terraria.GameContent.UI.Elements.UIText | Terraria.GameContent.UI.Elements/UIText.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Elements\UIText.cs | 29 | 2 | DynamicallyScaleDownToWidth | bool | `public bool DynamicallyScaleDownToWidth;` | `public bool DynamicallyScaleDownToWidth;` |

#### 属性（8）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3836 | property | Terraria.GameContent.UI.Elements.UIHeader | Terraria.GameContent.UI.Elements/UIHeader.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Elements\UIHeader.cs | 12 | 2 | Text | string | `public string Text { get { return _text; } set { if (_text != value) { _text = value; Width.Precent = 0f; Height.Precent = 0f; Recalculate(); } } }` | `public string Text { get { return _text; } set { if (_text != value) { _text = value; Width.Precent = 0f; Height.Precent = 0f; Recalculate(); } } }` |
| 3840 | property | Terraria.GameContent.UI.Elements.UIText | Terraria.GameContent.UI.Elements/UIText.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Elements\UIText.cs | 31 | 2 | Text | string | `public string Text => _text.ToString();` | `public string Text => _text.ToString();` |
| 3841 | property | Terraria.GameContent.UI.Elements.UIText | Terraria.GameContent.UI.Elements/UIText.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Elements\UIText.cs | 33 | 2 | TextOriginX | float | `public float TextOriginX { get; set; }` | `public float TextOriginX { get; set; }` |
| 3842 | property | Terraria.GameContent.UI.Elements.UIText | Terraria.GameContent.UI.Elements/UIText.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Elements\UIText.cs | 35 | 2 | TextOriginY | float | `public float TextOriginY { get; set; }` | `public float TextOriginY { get; set; }` |
| 3843 | property | Terraria.GameContent.UI.Elements.UIText | Terraria.GameContent.UI.Elements/UIText.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Elements\UIText.cs | 37 | 2 | WrappedTextBottomPadding | float | `public float WrappedTextBottomPadding { get; set; }` | `public float WrappedTextBottomPadding { get; set; }` |
| 3844 | property | Terraria.GameContent.UI.Elements.UIText | Terraria.GameContent.UI.Elements/UIText.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Elements\UIText.cs | 39 | 2 | IsWrapped | bool | `public bool IsWrapped { get { return _isWrapped; } set { _isWrapped = value; InternalSetText(_text, _textScale, _isLarge); } }` | `public bool IsWrapped { get { return _isWrapped; } set { _isWrapped = value; InternalSetText(_text, _textScale, _isLarge); } }` |
| 3845 | property | Terraria.GameContent.UI.Elements.UIText | Terraria.GameContent.UI.Elements/UIText.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Elements\UIText.cs | 52 | 2 | TextColor | Color | `public Color TextColor { get { return _color; } set { _color = value; } }` | `public Color TextColor { get { return _color; } set { _color = value; } }` |
| 3846 | property | Terraria.GameContent.UI.Elements.UIText | Terraria.GameContent.UI.Elements/UIText.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Elements\UIText.cs | 64 | 2 | ShadowColor | Color | `public Color ShadowColor { get { return _shadowColor; } set { _shadowColor = value; } }` | `public Color ShadowColor { get { return _shadowColor; } set { _shadowColor = value; } }` |


### 4.7 细分子系统：`UiLayoutPrimitives`

- 原报告章节：`4.11.9`
- 父级子系统：`ClientPresentationAndTools`
- 分区工作包：`16` / `UI 核心与交互`

- 上一级基线细分子系统：`UiLayoutPrimitives`
- 细分职责：UI 尺寸、计算样式和吸附点值对象。
- 边界角色：`presentation/value object`；最小 seam：UI layout value port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：3；声明类型数：3；字段：11；属性：1；合计：12。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（11）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 4272 | field | Terraria.UI.CalculatedStyle | Terraria.UI/CalculatedStyle.cs | D:\TRbackup\Version4\Terraria.UI\CalculatedStyle.cs | 7 | 2 | X | float | `public float X;` | `public float X;` |
| 4273 | field | Terraria.UI.CalculatedStyle | Terraria.UI/CalculatedStyle.cs | D:\TRbackup\Version4\Terraria.UI\CalculatedStyle.cs | 9 | 2 | Y | float | `public float Y;` | `public float Y;` |
| 4274 | field | Terraria.UI.CalculatedStyle | Terraria.UI/CalculatedStyle.cs | D:\TRbackup\Version4\Terraria.UI\CalculatedStyle.cs | 11 | 2 | Width | float | `public float Width;` | `public float Width;` |
| 4275 | field | Terraria.UI.CalculatedStyle | Terraria.UI/CalculatedStyle.cs | D:\TRbackup\Version4\Terraria.UI\CalculatedStyle.cs | 13 | 2 | Height | float | `public float Height;` | `public float Height;` |
| 4434 | field | Terraria.UI.SnapPoint | Terraria.UI/SnapPoint.cs | D:\TRbackup\Version4\Terraria.UI\SnapPoint.cs | 9 | 2 | Name | string | `public string Name;` | `public string Name;` |
| 4435 | field | Terraria.UI.SnapPoint | Terraria.UI/SnapPoint.cs | D:\TRbackup\Version4\Terraria.UI\SnapPoint.cs | 11 | 2 | _anchor | Vector2 | `private Vector2 _anchor;` | `private Vector2 _anchor;` |
| 4436 | field | Terraria.UI.SnapPoint | Terraria.UI/SnapPoint.cs | D:\TRbackup\Version4\Terraria.UI\SnapPoint.cs | 13 | 2 | _offset | Vector2 | `private Vector2 _offset;` | `private Vector2 _offset;` |
| 4437 | field | Terraria.UI.StyleDimension | Terraria.UI/StyleDimension.cs | D:\TRbackup\Version4\Terraria.UI\StyleDimension.cs | 5 | 2 | Fill | Terraria.UI.StyleDimension | `public static StyleDimension Fill = new StyleDimension(0f, 1f);` | `public static StyleDimension Fill = new StyleDimension(0f, 1f);` |
| 4438 | field | Terraria.UI.StyleDimension | Terraria.UI/StyleDimension.cs | D:\TRbackup\Version4\Terraria.UI\StyleDimension.cs | 7 | 2 | Empty | Terraria.UI.StyleDimension | `public static StyleDimension Empty = new StyleDimension(0f, 0f);` | `public static StyleDimension Empty = new StyleDimension(0f, 0f);` |
| 4439 | field | Terraria.UI.StyleDimension | Terraria.UI/StyleDimension.cs | D:\TRbackup\Version4\Terraria.UI\StyleDimension.cs | 9 | 2 | Pixels | float | `public float Pixels;` | `public float Pixels;` |
| 4440 | field | Terraria.UI.StyleDimension | Terraria.UI/StyleDimension.cs | D:\TRbackup\Version4\Terraria.UI\StyleDimension.cs | 11 | 2 | Precent | float | `public float Precent;` | `public float Precent;` |

#### 属性（1）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 4538 | property | Terraria.UI.SnapPoint | Terraria.UI/SnapPoint.cs | D:\TRbackup\Version4\Terraria.UI\SnapPoint.cs | 15 | 2 | Id | int | `public int Id { get; private set; }` | `public int Id { get; private set; }` |


### 4.8 细分子系统：`UiEventPayloads`

- 原报告章节：`4.11.10`
- 父级子系统：`ClientPresentationAndTools`
- 分区工作包：`16` / `UI 核心与交互`

- 上一级基线细分子系统：`UiEventPayloads`
- 细分职责：UI 事件、鼠标事件和滚轮事件载荷。
- 边界角色：`presentation/event`；最小 seam：UI event payload port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：3；声明类型数：3；字段：3；属性：0；合计：3。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（3）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 4472 | field | Terraria.UI.UIEvent | Terraria.UI/UIEvent.cs | D:\TRbackup\Version4\Terraria.UI\UIEvent.cs | 5 | 2 | Target | Terraria.UI.UIElement | `public readonly UIElement Target;` | `public readonly UIElement Target;` |
| 4473 | field | Terraria.UI.UIMouseEvent | Terraria.UI/UIMouseEvent.cs | D:\TRbackup\Version4\Terraria.UI\UIMouseEvent.cs | 7 | 2 | MousePosition | Vector2 | `public readonly Vector2 MousePosition;` | `public readonly Vector2 MousePosition;` |
| 4474 | field | Terraria.UI.UIScrollWheelEvent | Terraria.UI/UIScrollWheelEvent.cs | D:\TRbackup\Version4\Terraria.UI\UIScrollWheelEvent.cs | 7 | 2 | ScrollWheelValue | int | `public readonly int ScrollWheelValue;` | `public readonly int ScrollWheelValue;` |

#### 属性（0）

无该类型成员记录。


### 4.9 细分子系统：`UiInputPointerState`

- 原报告章节：`4.11.11`
- 父级子系统：`ClientPresentationAndTools`
- 分区工作包：`16` / `UI 核心与交互`

- 上一级基线细分子系统：`UiInputPointerState`
- 细分职责：用户界面输入指针缓存和状态变更协调。
- 边界角色：`presentation/state`；最小 seam：UI pointer input port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：2；字段：20；属性：0；合计：20。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（20）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 4476 | field | Terraria.UI.UserInterface.InputPointerCache | Terraria.UI/UserInterface.cs | D:\TRbackup\Version4\Terraria.UI\UserInterface.cs | 16 | 3 | LastTimeDown | double | `public double LastTimeDown;` | `public double LastTimeDown;` |
| 4477 | field | Terraria.UI.UserInterface.InputPointerCache | Terraria.UI/UserInterface.cs | D:\TRbackup\Version4\Terraria.UI\UserInterface.cs | 18 | 3 | WasDown | bool | `public bool WasDown;` | `public bool WasDown;` |
| 4478 | field | Terraria.UI.UserInterface.InputPointerCache | Terraria.UI/UserInterface.cs | D:\TRbackup\Version4\Terraria.UI\UserInterface.cs | 20 | 3 | LastDown | Terraria.UI.UIElement | `public UIElement LastDown;` | `public UIElement LastDown;` |
| 4479 | field | Terraria.UI.UserInterface.InputPointerCache | Terraria.UI/UserInterface.cs | D:\TRbackup\Version4\Terraria.UI\UserInterface.cs | 22 | 3 | LastClicked | Terraria.UI.UIElement | `public UIElement LastClicked;` | `public UIElement LastClicked;` |
| 4480 | field | Terraria.UI.UserInterface.InputPointerCache | Terraria.UI/UserInterface.cs | D:\TRbackup\Version4\Terraria.UI\UserInterface.cs | 24 | 3 | MouseDownEvent | Terraria.UI.UserInterface.MouseElementEvent | `public MouseElementEvent MouseDownEvent;` | `public MouseElementEvent MouseDownEvent;` |
| 4481 | field | Terraria.UI.UserInterface.InputPointerCache | Terraria.UI/UserInterface.cs | D:\TRbackup\Version4\Terraria.UI\UserInterface.cs | 26 | 3 | MouseUpEvent | Terraria.UI.UserInterface.MouseElementEvent | `public MouseElementEvent MouseUpEvent;` | `public MouseElementEvent MouseUpEvent;` |
| 4482 | field | Terraria.UI.UserInterface.InputPointerCache | Terraria.UI/UserInterface.cs | D:\TRbackup\Version4\Terraria.UI\UserInterface.cs | 28 | 3 | ClickEvent | Terraria.UI.UserInterface.MouseElementEvent | `public MouseElementEvent ClickEvent;` | `public MouseElementEvent ClickEvent;` |
| 4483 | field | Terraria.UI.UserInterface.InputPointerCache | Terraria.UI/UserInterface.cs | D:\TRbackup\Version4\Terraria.UI\UserInterface.cs | 30 | 3 | DoubleClickEvent | Terraria.UI.UserInterface.MouseElementEvent | `public MouseElementEvent DoubleClickEvent;` | `public MouseElementEvent DoubleClickEvent;` |
| 4484 | field | Terraria.UI.UserInterface | Terraria.UI/UserInterface.cs | D:\TRbackup\Version4\Terraria.UI\UserInterface.cs | 42 | 2 | DOUBLE_CLICK_TIME | double | `private const double DOUBLE_CLICK_TIME = 500.0;` | `private const double DOUBLE_CLICK_TIME = 500.0;` |
| 4485 | field | Terraria.UI.UserInterface | Terraria.UI/UserInterface.cs | D:\TRbackup\Version4\Terraria.UI\UserInterface.cs | 44 | 2 | STATE_CHANGE_CLICK_DISABLE_TIME | double | `private const double STATE_CHANGE_CLICK_DISABLE_TIME = 200.0;` | `private const double STATE_CHANGE_CLICK_DISABLE_TIME = 200.0;` |
| 4486 | field | Terraria.UI.UserInterface | Terraria.UI/UserInterface.cs | D:\TRbackup\Version4\Terraria.UI\UserInterface.cs | 46 | 2 | MAX_HISTORY_SIZE | int | `private const int MAX_HISTORY_SIZE = 32;` | `private const int MAX_HISTORY_SIZE = 32;` |
| 4487 | field | Terraria.UI.UserInterface | Terraria.UI/UserInterface.cs | D:\TRbackup\Version4\Terraria.UI\UserInterface.cs | 48 | 2 | HISTORY_PRUNE_SIZE | int | `private const int HISTORY_PRUNE_SIZE = 4;` | `private const int HISTORY_PRUNE_SIZE = 4;` |
| 4488 | field | Terraria.UI.UserInterface | Terraria.UI/UserInterface.cs | D:\TRbackup\Version4\Terraria.UI\UserInterface.cs | 50 | 2 | ActiveInstance | Terraria.UI.UserInterface | `public static UserInterface ActiveInstance = new UserInterface();` | `public static UserInterface ActiveInstance = new UserInterface();` |
| 4489 | field | Terraria.UI.UserInterface | Terraria.UI/UserInterface.cs | D:\TRbackup\Version4\Terraria.UI\UserInterface.cs | 52 | 2 | _history | System.Collections.Generic.List<Terraria.UI.UIState> | `private List<UIState> _history = new List<UIState>();` | `private List<UIState> _history = new List<UIState>();` |
| 4490 | field | Terraria.UI.UserInterface | Terraria.UI/UserInterface.cs | D:\TRbackup\Version4\Terraria.UI\UserInterface.cs | 54 | 2 | LeftMouse | Terraria.UI.UserInterface.InputPointerCache | `private InputPointerCache LeftMouse = new InputPointerCache  	{  		MouseDownEvent = delegate(UIElement element, UIMouseEvent evt)  		{  			element.LeftMouseDown(evt);  		},  		MouseUpEvent = delegate(UIElement element, UIMouseEvent evt)  		{  			element.LeftMouseUp(evt);  		},  		ClickEvent = delegate(UIElement element, UIMouseEvent evt)  		{  			element.LeftClick(evt);  		},  		DoubleClickEvent = delegate(UIElement element, UIMouseEvent evt)  		{  			element.LeftDoubleClick(evt);  		}  	};` | `private InputPointerCache LeftMouse = new InputPointerCache { MouseDownEvent = delegate(UIElement element, UIMouseEvent evt) { element.LeftMouseDown(evt); }, MouseUpEvent = delegate(UIElement element, UIMouseEvent evt) { element.LeftMouseUp(evt); }, ClickEvent = delegate(UIElement element, UIMouseEvent evt) { element.LeftClick(evt); }, DoubleClickEvent = delegate(UIElement element, UIMouseEvent evt) { element.LeftDoubleClick(evt); } };` |
| 4491 | field | Terraria.UI.UserInterface | Terraria.UI/UserInterface.cs | D:\TRbackup\Version4\Terraria.UI\UserInterface.cs | 74 | 2 | RightMouse | Terraria.UI.UserInterface.InputPointerCache | `private InputPointerCache RightMouse = new InputPointerCache  	{  		MouseDownEvent = delegate(UIElement element, UIMouseEvent evt)  		{  			element.RightMouseDown(evt);  		},  		MouseUpEvent = delegate(UIElement element, UIMouseEvent evt)  		{  			element.RightMouseUp(evt);  		},  		ClickEvent = delegate(UIElement element, UIMouseEvent evt)  		{  			element.RightClick(evt);  		},  		DoubleClickEvent = delegate(UIElement element, UIMouseEvent evt)  		{  			element.RightDoubleClick(evt);  		}  	};` | `private InputPointerCache RightMouse = new InputPointerCache { MouseDownEvent = delegate(UIElement element, UIMouseEvent evt) { element.RightMouseDown(evt); }, MouseUpEvent = delegate(UIElement element, UIMouseEvent evt) { element.RightMouseUp(evt); }, ClickEvent = delegate(UIElement element, UIMouseEvent evt) { element.RightClick(evt); }, DoubleClickEvent = delegate(UIElement element, UIMouseEvent evt) { element.RightDoubleClick(evt); } };` |
| 4492 | field | Terraria.UI.UserInterface | Terraria.UI/UserInterface.cs | D:\TRbackup\Version4\Terraria.UI\UserInterface.cs | 94 | 2 | MousePosition | Vector2 | `public Vector2 MousePosition;` | `public Vector2 MousePosition;` |
| 4493 | field | Terraria.UI.UserInterface | Terraria.UI/UserInterface.cs | D:\TRbackup\Version4\Terraria.UI\UserInterface.cs | 96 | 2 | _lastElementHover | Terraria.UI.UIElement | `private UIElement _lastElementHover;` | `private UIElement _lastElementHover;` |
| 4494 | field | Terraria.UI.UserInterface | Terraria.UI/UserInterface.cs | D:\TRbackup\Version4\Terraria.UI\UserInterface.cs | 98 | 2 | IsVisible | bool | `public bool IsVisible;` | `public bool IsVisible;` |
| 4495 | field | Terraria.UI.UserInterface | Terraria.UI/UserInterface.cs | D:\TRbackup\Version4\Terraria.UI\UserInterface.cs | 100 | 2 | _currentState | Terraria.UI.UIState | `private UIState _currentState;` | `private UIState _currentState;` |

#### 属性（0）

无该类型成员记录。


### 4.10 细分子系统：`UiElementLayoutAndDimensionsState`

- 原报告章节：`4.11.17`
- 父级子系统：`ClientPresentationAndTools`
- 分区工作包：`16` / `UI 核心与交互`

- 上一级基线细分子系统：`UiElementTreeState`
- 细分职责：UI 元素树的尺寸、边距、对齐和父子布局状态。
- 边界角色：`presentation`；最小 seam：UI element layout port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：22；属性：2；合计：24。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（22）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 4441 | field | Terraria.UI.UIElement | Terraria.UI/UIElement.cs | D:\TRbackup\Version4\Terraria.UI\UIElement.cs | 23 | 2 | Elements | System.Collections.Generic.List<Terraria.UI.UIElement> | `protected readonly List<UIElement> Elements = new List<UIElement>();` | `protected readonly List<UIElement> Elements = new List<UIElement>();` |
| 4442 | field | Terraria.UI.UIElement | Terraria.UI/UIElement.cs | D:\TRbackup\Version4\Terraria.UI\UIElement.cs | 25 | 2 | Top | Terraria.UI.StyleDimension | `public StyleDimension Top;` | `public StyleDimension Top;` |
| 4443 | field | Terraria.UI.UIElement | Terraria.UI/UIElement.cs | D:\TRbackup\Version4\Terraria.UI\UIElement.cs | 27 | 2 | Left | Terraria.UI.StyleDimension | `public StyleDimension Left;` | `public StyleDimension Left;` |
| 4444 | field | Terraria.UI.UIElement | Terraria.UI/UIElement.cs | D:\TRbackup\Version4\Terraria.UI\UIElement.cs | 29 | 2 | Width | Terraria.UI.StyleDimension | `public StyleDimension Width;` | `public StyleDimension Width;` |
| 4445 | field | Terraria.UI.UIElement | Terraria.UI/UIElement.cs | D:\TRbackup\Version4\Terraria.UI\UIElement.cs | 31 | 2 | Height | Terraria.UI.StyleDimension | `public StyleDimension Height;` | `public StyleDimension Height;` |
| 4446 | field | Terraria.UI.UIElement | Terraria.UI/UIElement.cs | D:\TRbackup\Version4\Terraria.UI\UIElement.cs | 33 | 2 | MaxWidth | Terraria.UI.StyleDimension | `public StyleDimension MaxWidth = StyleDimension.Fill;` | `public StyleDimension MaxWidth = StyleDimension.Fill;` |
| 4447 | field | Terraria.UI.UIElement | Terraria.UI/UIElement.cs | D:\TRbackup\Version4\Terraria.UI\UIElement.cs | 35 | 2 | MaxHeight | Terraria.UI.StyleDimension | `public StyleDimension MaxHeight = StyleDimension.Fill;` | `public StyleDimension MaxHeight = StyleDimension.Fill;` |
| 4448 | field | Terraria.UI.UIElement | Terraria.UI/UIElement.cs | D:\TRbackup\Version4\Terraria.UI\UIElement.cs | 37 | 2 | MinWidth | Terraria.UI.StyleDimension | `public StyleDimension MinWidth = StyleDimension.Empty;` | `public StyleDimension MinWidth = StyleDimension.Empty;` |
| 4449 | field | Terraria.UI.UIElement | Terraria.UI/UIElement.cs | D:\TRbackup\Version4\Terraria.UI\UIElement.cs | 39 | 2 | MinHeight | Terraria.UI.StyleDimension | `public StyleDimension MinHeight = StyleDimension.Empty;` | `public StyleDimension MinHeight = StyleDimension.Empty;` |
| 4455 | field | Terraria.UI.UIElement | Terraria.UI/UIElement.cs | D:\TRbackup\Version4\Terraria.UI\UIElement.cs | 51 | 2 | PaddingTop | float | `public float PaddingTop;` | `public float PaddingTop;` |
| 4456 | field | Terraria.UI.UIElement | Terraria.UI/UIElement.cs | D:\TRbackup\Version4\Terraria.UI\UIElement.cs | 53 | 2 | PaddingLeft | float | `public float PaddingLeft;` | `public float PaddingLeft;` |
| 4457 | field | Terraria.UI.UIElement | Terraria.UI/UIElement.cs | D:\TRbackup\Version4\Terraria.UI\UIElement.cs | 55 | 2 | PaddingRight | float | `public float PaddingRight;` | `public float PaddingRight;` |
| 4458 | field | Terraria.UI.UIElement | Terraria.UI/UIElement.cs | D:\TRbackup\Version4\Terraria.UI\UIElement.cs | 57 | 2 | PaddingBottom | float | `public float PaddingBottom;` | `public float PaddingBottom;` |
| 4459 | field | Terraria.UI.UIElement | Terraria.UI/UIElement.cs | D:\TRbackup\Version4\Terraria.UI\UIElement.cs | 59 | 2 | MarginTop | float | `public float MarginTop;` | `public float MarginTop;` |
| 4460 | field | Terraria.UI.UIElement | Terraria.UI/UIElement.cs | D:\TRbackup\Version4\Terraria.UI\UIElement.cs | 61 | 2 | MarginLeft | float | `public float MarginLeft;` | `public float MarginLeft;` |
| 4461 | field | Terraria.UI.UIElement | Terraria.UI/UIElement.cs | D:\TRbackup\Version4\Terraria.UI\UIElement.cs | 63 | 2 | MarginRight | float | `public float MarginRight;` | `public float MarginRight;` |
| 4462 | field | Terraria.UI.UIElement | Terraria.UI/UIElement.cs | D:\TRbackup\Version4\Terraria.UI\UIElement.cs | 65 | 2 | MarginBottom | float | `public float MarginBottom;` | `public float MarginBottom;` |
| 4463 | field | Terraria.UI.UIElement | Terraria.UI/UIElement.cs | D:\TRbackup\Version4\Terraria.UI\UIElement.cs | 67 | 2 | HAlign | float | `public float HAlign;` | `public float HAlign;` |
| 4464 | field | Terraria.UI.UIElement | Terraria.UI/UIElement.cs | D:\TRbackup\Version4\Terraria.UI\UIElement.cs | 69 | 2 | VAlign | float | `public float VAlign;` | `public float VAlign;` |
| 4465 | field | Terraria.UI.UIElement | Terraria.UI/UIElement.cs | D:\TRbackup\Version4\Terraria.UI\UIElement.cs | 71 | 2 | _innerDimensions | Terraria.UI.CalculatedStyle | `private CalculatedStyle _innerDimensions;` | `private CalculatedStyle _innerDimensions;` |
| 4466 | field | Terraria.UI.UIElement | Terraria.UI/UIElement.cs | D:\TRbackup\Version4\Terraria.UI\UIElement.cs | 73 | 2 | _dimensions | Terraria.UI.CalculatedStyle | `private CalculatedStyle _dimensions;` | `private CalculatedStyle _dimensions;` |
| 4467 | field | Terraria.UI.UIElement | Terraria.UI/UIElement.cs | D:\TRbackup\Version4\Terraria.UI\UIElement.cs | 75 | 2 | _outerDimensions | Terraria.UI.CalculatedStyle | `private CalculatedStyle _outerDimensions;` | `private CalculatedStyle _outerDimensions;` |

#### 属性（2）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 4539 | property | Terraria.UI.UIElement | Terraria.UI/UIElement.cs | D:\TRbackup\Version4\Terraria.UI\UIElement.cs | 89 | 2 | Parent | Terraria.UI.UIElement | `public UIElement Parent { get; private set; }` | `public UIElement Parent { get; private set; }` |
| 4541 | property | Terraria.UI.UIElement | Terraria.UI/UIElement.cs | D:\TRbackup\Version4\Terraria.UI\UIElement.cs | 93 | 2 | Children | System.Collections.Generic.IEnumerable<Terraria.UI.UIElement> | `public IEnumerable<UIElement> Children => Elements;` | `public IEnumerable<UIElement> Children => Elements;` |


### 4.11 细分子系统：`UiElementInteractionAndLifecycleState`

- 原报告章节：`4.11.18`
- 父级子系统：`ClientPresentationAndTools`
- 分区工作包：`16` / `UI 核心与交互`

- 上一级基线细分子系统：`UiElementTreeState`
- 细分职责：UI 初始化、鼠标交互、快照器和 UIState 生命周期状态。
- 边界角色：`presentation`；最小 seam：UI element interaction lifecycle port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：2；声明类型数：2；字段：10；属性：2；合计：12。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（10）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 4450 | field | Terraria.UI.UIElement | Terraria.UI/UIElement.cs | D:\TRbackup\Version4\Terraria.UI\UIElement.cs | 41 | 2 | _isInitialized | bool | `private bool _isInitialized;` | `private bool _isInitialized;` |
| 4451 | field | Terraria.UI.UIElement | Terraria.UI/UIElement.cs | D:\TRbackup\Version4\Terraria.UI\UIElement.cs | 43 | 2 | IgnoresMouseInteraction | bool | `public bool IgnoresMouseInteraction;` | `public bool IgnoresMouseInteraction;` |
| 4452 | field | Terraria.UI.UIElement | Terraria.UI/UIElement.cs | D:\TRbackup\Version4\Terraria.UI\UIElement.cs | 45 | 2 | PassThroughMouseInteraction | bool | `public bool PassThroughMouseInteraction;` | `public bool PassThroughMouseInteraction;` |
| 4453 | field | Terraria.UI.UIElement | Terraria.UI/UIElement.cs | D:\TRbackup\Version4\Terraria.UI\UIElement.cs | 47 | 2 | OverflowHidden | bool | `public bool OverflowHidden;` | `public bool OverflowHidden;` |
| 4454 | field | Terraria.UI.UIElement | Terraria.UI/UIElement.cs | D:\TRbackup\Version4\Terraria.UI\UIElement.cs | 49 | 2 | OverrideSamplerState | SamplerState | `public SamplerState OverrideSamplerState;` | `public SamplerState OverrideSamplerState;` |
| 4468 | field | Terraria.UI.UIElement | Terraria.UI/UIElement.cs | D:\TRbackup\Version4\Terraria.UI\UIElement.cs | 77 | 2 | OverflowHiddenRasterizerState | RasterizerState | `private static readonly RasterizerState OverflowHiddenRasterizerState = new RasterizerState  	{  		CullMode = CullMode.None,  		ScissorTestEnable = true  	};` | `private static readonly RasterizerState OverflowHiddenRasterizerState = new RasterizerState { CullMode = CullMode.None, ScissorTestEnable = true };` |
| 4469 | field | Terraria.UI.UIElement | Terraria.UI/UIElement.cs | D:\TRbackup\Version4\Terraria.UI\UIElement.cs | 83 | 2 | UseImmediateMode | bool | `public bool UseImmediateMode;` | `public bool UseImmediateMode;` |
| 4470 | field | Terraria.UI.UIElement | Terraria.UI/UIElement.cs | D:\TRbackup\Version4\Terraria.UI\UIElement.cs | 85 | 2 | _snapPoint | Terraria.UI.SnapPoint | `private SnapPoint _snapPoint;` | `private SnapPoint _snapPoint;` |
| 4471 | field | Terraria.UI.UIElement | Terraria.UI/UIElement.cs | D:\TRbackup\Version4\Terraria.UI\UIElement.cs | 87 | 2 | _idCounter | int | `private static int _idCounter = 0;` | `private static int _idCounter = 0;` |
| 4475 | field | Terraria.UI.UIState | Terraria.UI/UIState.cs | D:\TRbackup\Version4\Terraria.UI\UIState.cs | 5 | 2 | NoGamepadSupport | bool | `public bool NoGamepadSupport;` | `public bool NoGamepadSupport;` |

#### 属性（2）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 4540 | property | Terraria.UI.UIElement | Terraria.UI/UIElement.cs | D:\TRbackup\Version4\Terraria.UI\UIElement.cs | 91 | 2 | UniqueId | int | `public int UniqueId { get; private set; }` | `public int UniqueId { get; private set; }` |
| 4542 | property | Terraria.UI.UIElement | Terraria.UI/UIElement.cs | D:\TRbackup\Version4\Terraria.UI\UIElement.cs | 95 | 2 | IsMouseHovering | bool | `public bool IsMouseHovering { get; private set; }` | `public bool IsMouseHovering { get; private set; }` |


## 5. 追溯与验收

> 来源报告 SHA-256：`b944441d92d5103f218b766a3e354200794528a19e65a862e44c2b40a36f1196`
- 本分区细分组数：11；成员数：176；字段：154；属性：22。
- 预期不变量：本文件只出现完整细分子系统；不跨分区复制成员；所有成员的来源序号、原始声明、路径、行列和 C# 类型必须与来源报告一致。
- 验证方式：重新读取 20 个文件，检查文件数、细分组唯一性、来源序号恰好覆盖 `1..4542`、字段/属性合计和逐成员行文本一致；本次分区生成不运行 `dotnet`，因为没有修改 C# 或项目文件。

生成器：`Build/Tools/Generate-Version4NonAuthoritativeComponentPartitionReports.ps1`。
