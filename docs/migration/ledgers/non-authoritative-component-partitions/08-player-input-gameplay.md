# Version4 非权威组件拆分分区 08/20：玩家输入与玩法

> 来源报告：`docs/migration/ledgers/Version4非权威模拟系统字段属性逐成员源码声明-更细子系统拆分-去除ID类文件.md`
> 来源报告 SHA-256：`b944441d92d5103f218b766a3e354200794528a19e65a862e44c2b40a36f1196`
> 本文件是并行组件拆分工作包，不是新的权威成员清单；成员声明事实直接保留来源报告原文。

## 1. 分区定位与评估

- 分区范围：玩家输入、移动能力、交互、智能光标、装备能力、拾取和玩家拒绝状态。
- 本分区组件化重点：确认输入意图、玩法状态、交互命令和玩家表现数据的方向。
- 本分区包含 26 个完整细分子系统、283 条成员记录（字段 232、属性 51）。来源序号覆盖区间 `69..3960`，区间可能与其他分区交错，不以序号定义领域边界。
- 组件边界判定：细分子系统只是并行处理的最小库存边界，不等价于一个组件。必须继续按共同读者/写者、变更原因、生命周期、权威性和副作用分为 Component、System、Query、Command、Adapter 或 Projection。
- 证据状态：成员声明和统计已由来源报告校验；逐成员读者、写者、初始化/清理路径、持久化/网络语义和系统顺序仍需本分区会话补证，不得从名称或数量推断。

## 2. 分区统计

| 父级子系统 | 细分子系统数 | 字段 | 属性 | 合计 |
|---|---:|---:|---:|---:|
| `RuntimeComposition` | 6 | 38 | 10 | 48 |
| `SharedRuntimeMechanisms` | 20 | 194 | 41 | 235 |

| 原报告章节 | 父级 | 细分子系统 | 边界角色 | 字段 | 属性 | 合计 | 组件化处理 |
|---|---|---|---|---:|---:|---:|---|
| `4.1.8` | `RuntimeComposition` | `MainInputAndThreadScheduling` | runtime state | 11 | 0 | 11 | 待按成员访问模式拆分 |
| `4.1.26` | `RuntimeComposition` | `MainScreenAndInputState` | presentation state | 10 | 0 | 10 | 待按成员访问模式拆分 |
| `4.1.27` | `RuntimeComposition` | `MainPlayerAndSpawnState` | runtime state | 5 | 0 | 5 | 待按成员访问模式拆分 |
| `4.1.34` | `RuntimeComposition` | `MainMenuAndInputSettings` | presentation state | 5 | 0 | 5 | 待按成员访问模式拆分 |
| `4.1.41` | `RuntimeComposition` | `MainInputAndEventFlags` | runtime state | 7 | 0 | 7 | 待按成员访问模式拆分 |
| `4.1.46` | `RuntimeComposition` | `MainDerivedInputAndPresentationQueries` | derived/query | 0 | 10 | 10 | 待按成员访问模式拆分 |
| `4.9.10` | `SharedRuntimeMechanisms` | `SharedGolfState` | state/definition | 16 | 0 | 16 | 待按成员访问模式拆分 |
| `4.9.42` | `SharedRuntimeMechanisms` | `SharedControlFocusHelpers` | query/adapter | 1 | 4 | 5 | 待按成员访问模式拆分 |
| `4.9.50` | `SharedRuntimeMechanisms` | `SharedCreativePowerRuntimeManager` | state | 9 | 0 | 9 | 待按成员访问模式拆分 |
| `4.9.51` | `SharedRuntimeMechanisms` | `SharedCreativeUnlockProgress` | state/query | 9 | 1 | 10 | 待按成员访问模式拆分 |
| `4.9.53` | `SharedRuntimeMechanisms` | `SharedSmartInteractionQueries` | query | 16 | 5 | 21 | 待按成员访问模式拆分 |
| `4.9.78` | `SharedRuntimeMechanisms` | `PlayerItemPickupAndRespawnState` | state | 9 | 0 | 9 | 待按成员访问模式拆分 |
| `4.9.79` | `SharedRuntimeMechanisms` | `PlayerPreviewAndRejectionState` | presentation/state | 11 | 0 | 11 | 待按成员访问模式拆分 |
| `4.9.95` | `SharedRuntimeMechanisms` | `DoorOpeningInteractionState` | state/query | 10 | 0 | 10 | 待按成员访问模式拆分 |
| `4.9.96` | `SharedRuntimeMechanisms` | `SmartCursorInteractionState` | query/state | 19 | 0 | 19 | 待按成员访问模式拆分 |
| `4.9.97` | `SharedRuntimeMechanisms` | `PressurePlateInteractionState` | state/query | 5 | 0 | 5 | 待按成员访问模式拆分 |
| `4.9.98` | `SharedRuntimeMechanisms` | `CursorAndChestInteractionState` | state/query | 6 | 1 | 7 | 待按成员访问模式拆分 |
| `4.9.104` | `SharedRuntimeMechanisms` | `InputProfilesAndConfiguration` | adapter/state | 16 | 0 | 16 | 待按成员访问模式拆分 |
| `4.9.105` | `SharedRuntimeMechanisms` | `InputTriggerState` | adapter/state | 9 | 0 | 9 | 待按成员访问模式拆分 |
| `4.9.106` | `SharedRuntimeMechanisms` | `PlayerInputRuntimeState` | adapter/state | 3 | 1 | 4 | 待按成员访问模式拆分 |
| `4.9.112` | `SharedRuntimeMechanisms` | `EquipmentLoadoutState` | state | 3 | 0 | 3 | 待按成员访问模式拆分 |
| `4.9.122` | `SharedRuntimeMechanisms` | `SharedCreativePowerContracts` | definition/adapter | 0 | 4 | 4 | 待按成员访问模式拆分 |
| `4.9.142` | `SharedRuntimeMechanisms` | `PlayerMovementCapabilityState` | state/query | 18 | 0 | 18 | 待按成员访问模式拆分 |
| `4.9.143` | `SharedRuntimeMechanisms` | `PlayerIntentAndInteractionState` | state/query | 14 | 1 | 15 | 待按成员访问模式拆分 |
| `4.9.172` | `SharedRuntimeMechanisms` | `SharedCreativePerPlayerPowerState` | definition/state | 11 | 9 | 20 | 待按成员访问模式拆分 |
| `4.9.173` | `SharedRuntimeMechanisms` | `SharedCreativeSharedPowerState` | definition/state | 9 | 15 | 24 | 待按成员访问模式拆分 |

- 空成员父级说明：来源报告中的 `IntentAndInteraction` 为 0 条成员记录，本分区不新增成员，只保留该空边界供后续交互命令设计追踪。

## 3. 并行组件拆分契约

对本文件中的每个细分子系统和每条成员记录执行以下判断，并把结果写入对应的实现阶段设计/审计记录：

- 盘点声明类型、可见性、读者、写者、创建/更新/清理/持久化生命周期、访问频率和副作用；缺证据标记 `missing`，不可用命名猜测补齐。
- 标记权威状态、派生值、缓存、快照、兼容字段和表现数据；Component 只保存一个内聚概念的权威数据。
- 需要行为转换时放入显式 System/Command；纯资格判断和派生计算放入 Query；外部协议、文件、平台和第三方类型放入 Adapter；网络/持久化/日志/UI 输出放入单向 Projection。
- 分别建模实体引用、持久化 ID、网络 ID 和外部 ID；不得把 ID 类文件或外部对象直接塞入共享组件。
- 记录 Interface、Implementation、Seam、Depth、Leverage、Locality，以及必要的系统先后约束、失败归属和重试策略。

## 4. 逐成员源码声明

以下细分子系统块保持来源报告的成员表、声明、路径、行列和来源序号；只调整 Markdown 标题层级并增加分区追踪元数据。

### 4.1 细分子系统：`MainInputAndThreadScheduling`

- 原报告章节：`4.1.8`
- 父级子系统：`RuntimeComposition`
- 分区工作包：`08` / `玩家输入与玩法`

- 上一级基线细分子系统：`MainInputAndThreadScheduling`
- 细分职责：输入坐标、主线程动作队列和帧级交互计时。
- 边界角色：`runtime state`；最小 seam：main-thread action queue；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：11；属性：0；合计：11。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（11）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 69 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 297 | 2 | verboseNetplay | bool | `public static bool verboseNetplay = false;` | `public static bool verboseNetplay = false;` |
| 70 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 299 | 2 | stopTimeOuts | bool | `public static bool stopTimeOuts = false;` | `public static bool stopTimeOuts = false;` |
| 71 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 301 | 2 | townNPCCanSpawn | bool[] | `public static bool[] townNPCCanSpawn = new bool[NPCID.Count];` | `public static bool[] townNPCCanSpawn = new bool[NPCID.Count];` |
| 72 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 303 | 2 | upTimer | float | `public static float upTimer;` | `public static float upTimer;` |
| 73 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 305 | 2 | upTimerMax | float | `public static float upTimerMax;` | `public static float upTimerMax;` |
| 74 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 307 | 2 | upTimerMaxDelay | float | `public static float upTimerMaxDelay;` | `public static float upTimerMaxDelay;` |
| 75 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 309 | 2 | renderNow | bool | `public static bool renderNow;` | `public static bool renderNow;` |
| 76 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 311 | 2 | mouseX | int | `public static int mouseX;` | `public static int mouseX;` |
| 77 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 313 | 2 | mouseY | int | `public static int mouseY;` | `public static int mouseY;` |
| 78 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 316 | 2 | _mainThreadActions | System.Collections.Concurrent.ConcurrentQueue<System.Action> | `private static ConcurrentQueue<Action> _mainThreadActions = new ConcurrentQueue<Action>();` | `private static ConcurrentQueue<Action> _mainThreadActions = new ConcurrentQueue<Action>();` |
| 79 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 318 | 2 | mouseRight | bool | `public static bool mouseRight;` | `public static bool mouseRight;` |

#### 属性（0）

无该类型成员记录。


### 4.2 细分子系统：`MainScreenAndInputState`

- 原报告章节：`4.1.26`
- 父级子系统：`RuntimeComposition`
- 分区工作包：`08` / `玩家输入与玩法`

- 上一级基线细分子系统：`MainScreenAndInputState`
- 细分职责：屏幕尺寸、输入接管、鼠标物品和 UI 颜色。
- 边界角色：`presentation state`；最小 seam：screen/input view；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：10；属性：0；合计：10。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（10）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 390 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 959 | 2 | screenPosition | Vector2 | `public static Vector2 screenPosition;` | `public static Vector2 screenPosition;` |
| 391 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 961 | 2 | screenWidth | int | `public static int screenWidth = 1152;` | `public static int screenWidth = 1152;` |
| 392 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 963 | 2 | screenHeight | int | `public static int screenHeight = 864;` | `public static int screenHeight = 864;` |
| 393 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 965 | 2 | multiplayerNPCSmoothingRange | int | `public static int multiplayerNPCSmoothingRange = 300;` | `public static int multiplayerNPCSmoothingRange = 300;` |
| 394 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 967 | 2 | Setting_UseReducedMaxLiquids | bool | `public static bool Setting_UseReducedMaxLiquids = false;` | `public static bool Setting_UseReducedMaxLiquids = false;` |
| 395 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 969 | 2 | PlayerOverheadChatMessageDisplayTime | int | `public static int PlayerOverheadChatMessageDisplayTime = 400;` | `public static int PlayerOverheadChatMessageDisplayTime = 400;` |
| 396 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 971 | 2 | CurrentInputTextTakerOverride | object | `public static object CurrentInputTextTakerOverride;` | `public static object CurrentInputTextTakerOverride;` |
| 397 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 973 | 2 | mouseTextColor | byte | `public static byte mouseTextColor;` | `public static byte mouseTextColor;` |
| 398 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 975 | 2 | mouseRightRelease | bool | `public static bool mouseRightRelease;` | `public static bool mouseRightRelease;` |
| 399 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 977 | 2 | mouseItem | Terraria.Item | `public static Item mouseItem = new Item();` | `public static Item mouseItem = new Item();` |

#### 属性（0）

无该类型成员记录。


### 4.3 细分子系统：`MainPlayerAndSpawnState`

- 原报告章节：`4.1.27`
- 父级子系统：`RuntimeComposition`
- 分区工作包：`08` / `玩家输入与玩法`

- 上一级基线细分子系统：`MainPlayerAndSpawnState`
- 细分职责：本地玩家、玩家池、出生点和玩法归属槽。
- 边界角色：`runtime state`；最小 seam：player/spawn registry；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：5；属性：0；合计：5。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（5）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 400 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 980 | 2 | myPlayer | int | `public static int myPlayer;` | `public static int myPlayer;` |
| 401 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 982 | 2 | player | Terraria.Player[] | `public static Player[] player = new Player[256];` | `public static Player[] player = new Player[256];` |
| 402 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 984 | 2 | countsAsHostForGameplay | bool[] | `public static bool[] countsAsHostForGameplay = new bool[256];` | `public static bool[] countsAsHostForGameplay = new bool[256];` |
| 403 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 986 | 2 | spawnTileX | int | `public static int spawnTileX;` | `public static int spawnTileX;` |
| 404 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 988 | 2 | spawnTileY | int | `public static int spawnTileY;` | `public static int spawnTileY;` |

#### 属性（0）

无该类型成员记录。


### 4.4 细分子系统：`MainMenuAndInputSettings`

- 原报告章节：`4.1.34`
- 父级子系统：`RuntimeComposition`
- 分区工作包：`08` / `玩家输入与玩法`

- 上一级基线细分子系统：`MainMenuAndInputSettings`
- 细分职责：菜单快捷键、智能游标和运行时资源设置。
- 边界角色：`presentation state`；最小 seam：menu/input settings view；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：5；属性：0；合计：5。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（5）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 456 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1180 | 2 | cInv | string | `public static string cInv = "Escape";` | `public static string cInv = "Escape";` |
| 457 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1182 | 2 | SmartCursorWanted_Mouse | bool | `public static bool SmartCursorWanted_Mouse = false;` | `public static bool SmartCursorWanted_Mouse = false;` |
| 458 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1184 | 2 | SmartCursorWanted_GamePad | bool | `public static bool SmartCursorWanted_GamePad = false;` | `public static bool SmartCursorWanted_GamePad = false;` |
| 459 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1186 | 2 | NoPooling | bool | `public static bool NoPooling = false;` | `public static bool NoPooling = false;` |
| 460 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1188 | 2 | CollectGen0EveryFrame | bool | `public static bool CollectGen0EveryFrame = false;` | `public static bool CollectGen0EveryFrame = false;` |

#### 属性（0）

无该类型成员记录。


### 4.5 细分子系统：`MainInputAndEventFlags`

- 原报告章节：`4.1.41`
- 父级子系统：`RuntimeComposition`
- 分区工作包：`08` / `玩家输入与玩法`

- 上一级基线细分子系统：`MainInputAndEventFlags`
- 细分职责：鼠标、绘制、陨石和环境伤害开关。
- 边界角色：`runtime state`；最小 seam：input/event flag view；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：7；属性：0；合计：7。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（7）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 504 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1282 | 2 | blockMouse | bool | `public static bool blockMouse;` | `public static bool blockMouse;` |
| 505 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1285 | 2 | _isDrawingOrUpdating | bool | `private bool _isDrawingOrUpdating;` | `private bool _isDrawingOrUpdating;` |
| 506 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1288 | 2 | disableDontStarveDarknessDamage | bool | `public static bool disableDontStarveDarknessDamage = false;` | `public static bool disableDontStarveDarknessDamage = false;` |
| 507 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1290 | 2 | starGame | bool | `public static bool starGame = false;` | `public static bool starGame = false;` |
| 508 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1292 | 2 | starsHit | int | `public static int starsHit = 0;` | `public static int starsHit = 0;` |
| 509 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1294 | 2 | ladyBugRainBoost | int | `public static int ladyBugRainBoost = 0;` | `public static int ladyBugRainBoost = 0;` |
| 510 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1296 | 2 | _canShowMeteorFall | bool | `private static bool _canShowMeteorFall;` | `private static bool _canShowMeteorFall;` |

#### 属性（0）

无该类型成员记录。


### 4.6 细分子系统：`MainDerivedInputAndPresentationQueries`

- 原报告章节：`4.1.46`
- 父级子系统：`RuntimeComposition`
- 分区工作包：`08` / `玩家输入与玩法`

- 上一级基线细分子系统：`MainDerivedPropertiesAndEvents`
- 细分职责：UI 缩放、鼠标、天气表现和诊断投影查询。
- 边界角色：`derived/query`；最小 seam：main derived input presentation port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：0；属性：10；合计：10。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（0）

无该类型成员记录。

#### 属性（10）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 511 | property | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1298 | 2 | UIScale | float | `public static float UIScale { get { return _uiScaleUsed; } set { _uiScaleWanted = value; _uiScaleUsed = value; _uiScaleMatrix = Matrix.CreateScale(value, value, 1f); } }` | `public static float UIScale { get { return _uiScaleUsed; } set { _uiScaleWanted = value; _uiScaleUsed = value; _uiScaleMatrix = Matrix.CreateScale(value, value, 1f); } }` |
| 512 | property | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1314 | 2 | IsItRaining | bool | `public static bool IsItRaining => cloudAlpha > 0f;` | `public static bool IsItRaining => cloudAlpha > 0f;` |
| 524 | property | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1401 | 2 | MouseScreen | Vector2 | `public static Vector2 MouseScreen => new Vector2(mouseX, mouseY);` | `public static Vector2 MouseScreen => new Vector2(mouseX, mouseY);` |
| 525 | property | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1403 | 2 | MouseWorld | Vector2 | `public static Vector2 MouseWorld { get { Vector2 result = MouseScreen + screenPosition; if (player[myPlayer].gravDir == -1f) { result.Y = screenPosition.Y + (float)screenHeight - (float)mouseY; } return result; } }` | `public static Vector2 MouseWorld { get { Vector2 result = MouseScreen + screenPosition; if (player[myPlayer].gravDir == -1f) { result.Y = screenPosition.Y + (float)screenHeight - (float)mouseY; } return result; } }` |
| 526 | property | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1416 | 2 | ActiveNetDiagnosticsUI | Terraria.UI.INetDiagnosticsUI | `public static INetDiagnosticsUI ActiveNetDiagnosticsUI { get { if (_activeNetDiagnosticsUI == null) { INetDiagnosticsUI activeNetDiagnosticsUI; INetDiagnosticsUI netDiagnosticsUI = new EmptyDiagnosticsUI(); activeNetDiagnosticsUI = netDiagnosticsUI; _activeNetDiagnosticsUI = activeNetDiagnosticsUI; } return _activeNetDiagnosticsUI; } }` | `public static INetDiagnosticsUI ActiveNetDiagnosticsUI { get { if (_activeNetDiagnosticsUI == null) { INetDiagnosticsUI activeNetDiagnosticsUI; INetDiagnosticsUI netDiagnosticsUI = new EmptyDiagnosticsUI(); activeNetDiagnosticsUI = netDiagnosticsUI; _activeNetDiagnosticsUI = activeNetDiagnosticsUI; } return _activeNetDiagnosticsUI; } }` |
| 531 | property | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1440 | 2 | PlayerSceneMetrics | Terraria.SceneMetrics | `public static SceneMetrics PlayerSceneMetrics => _playerSceneMetrics;` | `public static SceneMetrics PlayerSceneMetrics => _playerSceneMetrics;` |
| 533 | property | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1454 | 2 | WindForVisuals | float | `public static float WindForVisuals => windSpeedCurrent;` | `public static float WindForVisuals => windSpeedCurrent;` |
| 534 | property | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1456 | 2 | ChatLineWidthLimit | int | `public static int ChatLineWidthLimit => (int)((float)screenWidth * (1f / UIScale)) - 320;` | `public static int ChatLineWidthLimit => (int)((float)screenWidth * (1f / UIScale)) - 320;` |
| 541 | property | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1470 | 2 | BlackFadeDist | float | `public static float BlackFadeDist => Camera.UnscaledSize.Length() / 2f + 100f;` | `public static float BlackFadeDist => Camera.UnscaledSize.Length() / 2f + 100f;` |
| 542 | property | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1472 | 2 | IsRainingForever | bool | `public static bool IsRainingForever => rainTime >= 5184000;` | `public static bool IsRainingForever => rainTime >= 5184000;` |


### 4.7 细分子系统：`SharedGolfState`

- 原报告章节：`4.9.10`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`08` / `玩家输入与玩法`

- 上一级基线细分子系统：`SharedGolfState`
- 细分职责：高尔夫状态、轨迹和规则辅助。
- 边界角色：`state/definition`；最小 seam：golf state view；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：3；声明类型数：3；字段：16；属性：0；合计：16。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（16）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2020 | field | Terraria.GameContent.Golf.GolfBallTrackRecord | Terraria.GameContent.Golf/GolfBallTrackRecord.cs | D:\TRbackup\Version4\Terraria.GameContent.Golf\GolfBallTrackRecord.cs | 8 | 2 | _hitLocations | System.Collections.Generic.List<Vector2> | `private List<Vector2> _hitLocations = new List<Vector2>();` | `private List<Vector2> _hitLocations = new List<Vector2>();` |
| 2021 | field | Terraria.GameContent.Golf.GolfHelper | Terraria.GameContent.Golf/GolfHelper.cs | D:\TRbackup\Version4\Terraria.GameContent.Golf\GolfHelper.cs | 23 | 2 | PointsNeededForLevel1 | int | `public const int PointsNeededForLevel1 = 500;` | `public const int PointsNeededForLevel1 = 500;` |
| 2022 | field | Terraria.GameContent.Golf.GolfHelper | Terraria.GameContent.Golf/GolfHelper.cs | D:\TRbackup\Version4\Terraria.GameContent.Golf\GolfHelper.cs | 25 | 2 | PointsNeededForLevel2 | int | `public const int PointsNeededForLevel2 = 1000;` | `public const int PointsNeededForLevel2 = 1000;` |
| 2023 | field | Terraria.GameContent.Golf.GolfHelper | Terraria.GameContent.Golf/GolfHelper.cs | D:\TRbackup\Version4\Terraria.GameContent.Golf\GolfHelper.cs | 27 | 2 | PointsNeededForLevel3 | int | `public const int PointsNeededForLevel3 = 2000;` | `public const int PointsNeededForLevel3 = 2000;` |
| 2024 | field | Terraria.GameContent.Golf.GolfHelper | Terraria.GameContent.Golf/GolfHelper.cs | D:\TRbackup\Version4\Terraria.GameContent.Golf\GolfHelper.cs | 29 | 2 | PhysicsProperties | Terraria.Physics.PhysicsProperties | `public static readonly PhysicsProperties PhysicsProperties = new PhysicsProperties(0.3f, 0.99f);` | `public static readonly PhysicsProperties PhysicsProperties = new PhysicsProperties(0.3f, 0.99f);` |
| 2025 | field | Terraria.GameContent.Golf.GolfHelper | Terraria.GameContent.Golf/GolfHelper.cs | D:\TRbackup\Version4\Terraria.GameContent.Golf\GolfHelper.cs | 31 | 2 | Listener | Terraria.GameContent.Golf.GolfHelper.ContactListener | `public static readonly ContactListener Listener = new ContactListener();` | `public static readonly ContactListener Listener = new ContactListener();` |
| 2026 | field | Terraria.GameContent.Golf.GolfState | Terraria.GameContent.Golf/GolfState.cs | D:\TRbackup\Version4\Terraria.GameContent.Golf\GolfState.cs | 8 | 2 | BALL_RETURN_PENALTY | int | `private const int BALL_RETURN_PENALTY = 1;` | `private const int BALL_RETURN_PENALTY = 1;` |
| 2027 | field | Terraria.GameContent.Golf.GolfState | Terraria.GameContent.Golf/GolfState.cs | D:\TRbackup\Version4\Terraria.GameContent.Golf\GolfState.cs | 10 | 2 | golfScoreTime | int | `private int golfScoreTime;` | `private int golfScoreTime;` |
| 2028 | field | Terraria.GameContent.Golf.GolfState | Terraria.GameContent.Golf/GolfState.cs | D:\TRbackup\Version4\Terraria.GameContent.Golf\GolfState.cs | 12 | 2 | golfScoreTimeMax | int | `private int golfScoreTimeMax = 3600;` | `private int golfScoreTimeMax = 3600;` |
| 2029 | field | Terraria.GameContent.Golf.GolfState | Terraria.GameContent.Golf/GolfState.cs | D:\TRbackup\Version4\Terraria.GameContent.Golf\GolfState.cs | 14 | 2 | golfScoreDelay | int | `private int golfScoreDelay = 90;` | `private int golfScoreDelay = 90;` |
| 2030 | field | Terraria.GameContent.Golf.GolfState | Terraria.GameContent.Golf/GolfState.cs | D:\TRbackup\Version4\Terraria.GameContent.Golf\GolfState.cs | 16 | 2 | _lastRecordedBallTime | double | `private double _lastRecordedBallTime;` | `private double _lastRecordedBallTime;` |
| 2031 | field | Terraria.GameContent.Golf.GolfState | Terraria.GameContent.Golf/GolfState.cs | D:\TRbackup\Version4\Terraria.GameContent.Golf\GolfState.cs | 18 | 2 | _lastRecordedBallLocation | Vector2? | `private Vector2? _lastRecordedBallLocation;` | `private Vector2? _lastRecordedBallLocation;` |
| 2032 | field | Terraria.GameContent.Golf.GolfState | Terraria.GameContent.Golf/GolfState.cs | D:\TRbackup\Version4\Terraria.GameContent.Golf\GolfState.cs | 20 | 2 | _waitingForBallToSettle | bool | `private bool _waitingForBallToSettle;` | `private bool _waitingForBallToSettle;` |
| 2033 | field | Terraria.GameContent.Golf.GolfState | Terraria.GameContent.Golf/GolfState.cs | D:\TRbackup\Version4\Terraria.GameContent.Golf\GolfState.cs | 22 | 2 | _lastHitGolfBall | Terraria.Projectile | `private Projectile _lastHitGolfBall;` | `private Projectile _lastHitGolfBall;` |
| 2034 | field | Terraria.GameContent.Golf.GolfState | Terraria.GameContent.Golf/GolfState.cs | D:\TRbackup\Version4\Terraria.GameContent.Golf\GolfState.cs | 24 | 2 | _lastRecordedSwingCount | int | `private int _lastRecordedSwingCount;` | `private int _lastRecordedSwingCount;` |
| 2035 | field | Terraria.GameContent.Golf.GolfState | Terraria.GameContent.Golf/GolfState.cs | D:\TRbackup\Version4\Terraria.GameContent.Golf\GolfState.cs | 26 | 2 | _hitRecords | Terraria.GameContent.Golf.GolfBallTrackRecord[] | `private GolfBallTrackRecord[] _hitRecords = new GolfBallTrackRecord[1000];` | `private GolfBallTrackRecord[] _hitRecords = new GolfBallTrackRecord[1000];` |

#### 属性（0）

无该类型成员记录。


### 4.8 细分子系统：`SharedControlFocusHelpers`

- 原报告章节：`4.9.42`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`08` / `玩家输入与玩法`

- 上一级基线细分子系统：`SharedControlFocusHelpers`
- 细分职责：焦点和锁定控制辅助。
- 边界角色：`query/adapter`；最小 seam：focus/lock-on query；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：1；属性：4；合计：5。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（1）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3077 | field | Terraria.FocusHelper | Terraria/FocusHelper.cs | D:\TRbackup\Version4\Terraria\FocusHelper.cs | 8 | 2 | IsSelectedApplication | bool | `public static bool IsSelectedApplication;` | `public static bool IsSelectedApplication;` |

#### 属性（4）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3957 | property | Terraria.FocusHelper | Terraria/FocusHelper.cs | D:\TRbackup\Version4\Terraria\FocusHelper.cs | 10 | 2 | GameplayActive | bool | `public static bool GameplayActive { get { if (IsSelectedApplication) { return !Main.gamePaused; } return false; } }` | `public static bool GameplayActive { get { if (IsSelectedApplication) { return !Main.gamePaused; } return false; } }` |
| 3958 | property | Terraria.FocusHelper | Terraria/FocusHelper.cs | D:\TRbackup\Version4\Terraria\FocusHelper.cs | 22 | 2 | UpdateVisualEffects | bool | `public static bool UpdateVisualEffects => GameplayActive;` | `public static bool UpdateVisualEffects => GameplayActive;` |
| 3959 | property | Terraria.FocusHelper | Terraria/FocusHelper.cs | D:\TRbackup\Version4\Terraria\FocusHelper.cs | 24 | 2 | AllowRain | bool | `public static bool AllowRain => GameplayActive;` | `public static bool AllowRain => GameplayActive;` |
| 3960 | property | Terraria.FocusHelper | Terraria/FocusHelper.cs | D:\TRbackup\Version4\Terraria\FocusHelper.cs | 26 | 2 | AllowCountingPlayerTime | bool | `public static bool AllowCountingPlayerTime { get { bool flag = Main.gamePaused && !IsSelectedApplication; bool result = Main.instance.IsActive && !flag; if (Main.gameMenu) { result = false; } return result; } }` | `public static bool AllowCountingPlayerTime { get { bool flag = Main.gamePaused && !IsSelectedApplication; bool result = Main.instance.IsActive && !flag; if (Main.gameMenu) { result = false; } return result; } }` |


### 4.9 细分子系统：`SharedCreativePowerRuntimeManager`

- 原报告章节：`4.9.50`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`08` / `玩家输入与玩法`

- 上一级基线细分子系统：`SharedCreativePowerRuntimeManager`
- 细分职责：创意能力注册、按玩家存储和运行时管理。
- 边界角色：`state`；最小 seam：creative power manager port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：2；字段：9；属性：0；合计：9。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（9）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1462 | field | Terraria.GameContent.Creative.CreativePowerManager.PowerTypeStorage<T> | Terraria.GameContent.Creative/CreativePowerManager.cs | D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowerManager.cs | 12 | 3 | Id | ushort | `public static ushort Id;` | `public static ushort Id;` |
| 1463 | field | Terraria.GameContent.Creative.CreativePowerManager.PowerTypeStorage<T> | Terraria.GameContent.Creative/CreativePowerManager.cs | D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowerManager.cs | 14 | 3 | Name | string | `public static string Name;` | `public static string Name;` |
| 1464 | field | Terraria.GameContent.Creative.CreativePowerManager.PowerTypeStorage<T> | Terraria.GameContent.Creative/CreativePowerManager.cs | D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowerManager.cs | 16 | 3 | Power | T | `public static T Power;` | `public static T Power;` |
| 1465 | field | Terraria.GameContent.Creative.CreativePowerManager | Terraria.GameContent.Creative/CreativePowerManager.cs | D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowerManager.cs | 19 | 2 | Instance | Terraria.GameContent.Creative.CreativePowerManager | `public static readonly CreativePowerManager Instance = new CreativePowerManager();` | `public static readonly CreativePowerManager Instance = new CreativePowerManager();` |
| 1466 | field | Terraria.GameContent.Creative.CreativePowerManager | Terraria.GameContent.Creative/CreativePowerManager.cs | D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowerManager.cs | 21 | 2 | _powersById | System.Collections.Generic.Dictionary<ushort, Terraria.GameContent.Creative.ICreativePower> | `private Dictionary<ushort, ICreativePower> _powersById = new Dictionary<ushort, ICreativePower>();` | `private Dictionary<ushort, ICreativePower> _powersById = new Dictionary<ushort, ICreativePower>();` |
| 1467 | field | Terraria.GameContent.Creative.CreativePowerManager | Terraria.GameContent.Creative/CreativePowerManager.cs | D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowerManager.cs | 23 | 2 | _powersByName | System.Collections.Generic.Dictionary<string, Terraria.GameContent.Creative.ICreativePower> | `private Dictionary<string, ICreativePower> _powersByName = new Dictionary<string, ICreativePower>();` | `private Dictionary<string, ICreativePower> _powersByName = new Dictionary<string, ICreativePower>();` |
| 1468 | field | Terraria.GameContent.Creative.CreativePowerManager | Terraria.GameContent.Creative/CreativePowerManager.cs | D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowerManager.cs | 25 | 2 | _powersCount | ushort | `private ushort _powersCount;` | `private ushort _powersCount;` |
| 1469 | field | Terraria.GameContent.Creative.CreativePowerManager | Terraria.GameContent.Creative/CreativePowerManager.cs | D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowerManager.cs | 27 | 2 | _initialized | bool | `private static bool _initialized = false;` | `private static bool _initialized = false;` |
| 1470 | field | Terraria.GameContent.Creative.CreativePowerManager | Terraria.GameContent.Creative/CreativePowerManager.cs | D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowerManager.cs | 29 | 2 | _powerPermissionsLineHeader | string | `private const string _powerPermissionsLineHeader = "journeypermission_";` | `private const string _powerPermissionsLineHeader = "journeypermission_";` |

#### 属性（0）

无该类型成员记录。


### 4.10 细分子系统：`SharedCreativeUnlockProgress`

- 原报告章节：`4.9.51`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`08` / `玩家输入与玩法`

- 上一级基线细分子系统：`SharedCreativeUnlockProgress`
- 细分职责：牺牲目录和创意解锁进度。
- 边界角色：`state/query`；最小 seam：creative unlock view；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：3；声明类型数：3；字段：9；属性：1；合计：10。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（9）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1460 | field | Terraria.GameContent.Creative.CreativeItemSacrificesCatalog | Terraria.GameContent.Creative/CreativeItemSacrificesCatalog.cs | D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativeItemSacrificesCatalog.cs | 10 | 2 | Instance | Terraria.GameContent.Creative.CreativeItemSacrificesCatalog | `public static CreativeItemSacrificesCatalog Instance = new CreativeItemSacrificesCatalog();` | `public static CreativeItemSacrificesCatalog Instance = new CreativeItemSacrificesCatalog();` |
| 1461 | field | Terraria.GameContent.Creative.CreativeItemSacrificesCatalog | Terraria.GameContent.Creative/CreativeItemSacrificesCatalog.cs | D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativeItemSacrificesCatalog.cs | 12 | 2 | _sacrificeCountNeededByItemId | System.Collections.Generic.Dictionary<int, int> | `private Dictionary<int, int> _sacrificeCountNeededByItemId = new Dictionary<int, int>();` | `private Dictionary<int, int> _sacrificeCountNeededByItemId = new Dictionary<int, int>();` |
| 1519 | field | Terraria.GameContent.Creative.CreativeUnlocksTracker | Terraria.GameContent.Creative/CreativeUnlocksTracker.cs | D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativeUnlocksTracker.cs | 7 | 2 | ItemSacrifices | Terraria.GameContent.Creative.ItemsSacrificedUnlocksTracker | `public ItemsSacrificedUnlocksTracker ItemSacrifices = new ItemsSacrificedUnlocksTracker();` | `public ItemsSacrificedUnlocksTracker ItemSacrifices = new ItemsSacrificedUnlocksTracker();` |
| 1520 | field | Terraria.GameContent.Creative.ItemsSacrificedUnlocksTracker | Terraria.GameContent.Creative/ItemsSacrificedUnlocksTracker.cs | D:\TRbackup\Version4\Terraria.GameContent.Creative\ItemsSacrificedUnlocksTracker.cs | 10 | 2 | POSITIVE_SACRIFICE_COUNT_CAP | int | `public const int POSITIVE_SACRIFICE_COUNT_CAP = 9999;` | `public const int POSITIVE_SACRIFICE_COUNT_CAP = 9999;` |
| 1521 | field | Terraria.GameContent.Creative.ItemsSacrificedUnlocksTracker | Terraria.GameContent.Creative/ItemsSacrificedUnlocksTracker.cs | D:\TRbackup\Version4\Terraria.GameContent.Creative\ItemsSacrificedUnlocksTracker.cs | 12 | 2 | _sacrificeCountByItemPersistentId | System.Collections.Generic.Dictionary<string, int> | `private Dictionary<string, int> _sacrificeCountByItemPersistentId;` | `private Dictionary<string, int> _sacrificeCountByItemPersistentId;` |
| 1522 | field | Terraria.GameContent.Creative.ItemsSacrificedUnlocksTracker | Terraria.GameContent.Creative/ItemsSacrificedUnlocksTracker.cs | D:\TRbackup\Version4\Terraria.GameContent.Creative\ItemsSacrificedUnlocksTracker.cs | 14 | 2 | _sacrificesCountByItemIdCache | System.Collections.Generic.Dictionary<int, int> | `private Dictionary<int, int> _sacrificesCountByItemIdCache;` | `private Dictionary<int, int> _sacrificesCountByItemIdCache;` |
| 1523 | field | Terraria.GameContent.Creative.ItemsSacrificedUnlocksTracker | Terraria.GameContent.Creative/ItemsSacrificedUnlocksTracker.cs | D:\TRbackup\Version4\Terraria.GameContent.Creative\ItemsSacrificedUnlocksTracker.cs | 16 | 2 | _unlockedByTeammate | System.Collections.Generic.Dictionary<int, string> | `private Dictionary<int, string> _unlockedByTeammate;` | `private Dictionary<int, string> _unlockedByTeammate;` |
| 1524 | field | Terraria.GameContent.Creative.ItemsSacrificedUnlocksTracker | Terraria.GameContent.Creative/ItemsSacrificedUnlocksTracker.cs | D:\TRbackup\Version4\Terraria.GameContent.Creative\ItemsSacrificedUnlocksTracker.cs | 18 | 2 | _newlyUnlocked | System.Collections.Generic.HashSet<int> | `private HashSet<int> _newlyUnlocked;` | `private HashSet<int> _newlyUnlocked;` |
| 1525 | field | Terraria.GameContent.Creative.ItemsSacrificedUnlocksTracker | Terraria.GameContent.Creative/ItemsSacrificedUnlocksTracker.cs | D:\TRbackup\Version4\Terraria.GameContent.Creative\ItemsSacrificedUnlocksTracker.cs | 20 | 2 | AnyNewUnlocksFromTeammates | bool | `public bool AnyNewUnlocksFromTeammates;` | `public bool AnyNewUnlocksFromTeammates;` |

#### 属性（1）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3776 | property | Terraria.GameContent.Creative.ItemsSacrificedUnlocksTracker | Terraria.GameContent.Creative/ItemsSacrificedUnlocksTracker.cs | D:\TRbackup\Version4\Terraria.GameContent.Creative\ItemsSacrificedUnlocksTracker.cs | 22 | 2 | LastEditId | int | `public int LastEditId { get; private set; }` | `public int LastEditId { get; private set; }` |


### 4.11 细分子系统：`SharedSmartInteractionQueries`

- 原报告章节：`4.9.53`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`08` / `玩家输入与玩法`

- 上一级基线细分子系统：`SharedSmartInteractionQueries`
- 细分职责：智能交互候选和扫描查询。
- 边界角色：`query`；最小 seam：smart interaction query；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：7；声明类型数：11；字段：16；属性：5；合计：21。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（16）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2145 | field | Terraria.GameContent.ObjectInteractions.NPCSmartInteractCandidateProvider | Terraria.GameContent.ObjectInteractions/NPCSmartInteractCandidateProvider.cs | D:\TRbackup\Version4\Terraria.GameContent.ObjectInteractions\NPCSmartInteractCandidateProvider.cs | 16 | 2 | _candidate | Terraria.GameContent.ObjectInteractions.NPCSmartInteractCandidateProvider.ReusableCandidate | `private ReusableCandidate _candidate = new ReusableCandidate();` | `private ReusableCandidate _candidate = new ReusableCandidate();` |
| 2146 | field | Terraria.GameContent.ObjectInteractions.PotionOfReturnSmartInteractCandidateProvider | Terraria.GameContent.ObjectInteractions/PotionOfReturnSmartInteractCandidateProvider.cs | D:\TRbackup\Version4\Terraria.GameContent.ObjectInteractions\PotionOfReturnSmartInteractCandidateProvider.cs | 14 | 2 | _candidate | Terraria.GameContent.ObjectInteractions.PotionOfReturnSmartInteractCandidateProvider.ReusableCandidate | `private ReusableCandidate _candidate = new ReusableCandidate();` | `private ReusableCandidate _candidate = new ReusableCandidate();` |
| 2147 | field | Terraria.GameContent.ObjectInteractions.ProjectileSmartInteractCandidateProvider | Terraria.GameContent.ObjectInteractions/ProjectileSmartInteractCandidateProvider.cs | D:\TRbackup\Version4\Terraria.GameContent.ObjectInteractions\ProjectileSmartInteractCandidateProvider.cs | 16 | 2 | _candidate | Terraria.GameContent.ObjectInteractions.ProjectileSmartInteractCandidateProvider.ReusableCandidate | `private ReusableCandidate _candidate = new ReusableCandidate();` | `private ReusableCandidate _candidate = new ReusableCandidate();` |
| 2148 | field | Terraria.GameContent.ObjectInteractions.SmartInteractScanSettings | Terraria.GameContent.ObjectInteractions/SmartInteractScanSettings.cs | D:\TRbackup\Version4\Terraria.GameContent.ObjectInteractions\SmartInteractScanSettings.cs | 7 | 2 | player | Terraria.Player | `public Player player;` | `public Player player;` |
| 2149 | field | Terraria.GameContent.ObjectInteractions.SmartInteractScanSettings | Terraria.GameContent.ObjectInteractions/SmartInteractScanSettings.cs | D:\TRbackup\Version4\Terraria.GameContent.ObjectInteractions\SmartInteractScanSettings.cs | 9 | 2 | DemandOnlyZeroDistanceTargets | bool | `public bool DemandOnlyZeroDistanceTargets;` | `public bool DemandOnlyZeroDistanceTargets;` |
| 2150 | field | Terraria.GameContent.ObjectInteractions.SmartInteractScanSettings | Terraria.GameContent.ObjectInteractions/SmartInteractScanSettings.cs | D:\TRbackup\Version4\Terraria.GameContent.ObjectInteractions\SmartInteractScanSettings.cs | 11 | 2 | FullInteraction | bool | `public bool FullInteraction;` | `public bool FullInteraction;` |
| 2151 | field | Terraria.GameContent.ObjectInteractions.SmartInteractScanSettings | Terraria.GameContent.ObjectInteractions/SmartInteractScanSettings.cs | D:\TRbackup\Version4\Terraria.GameContent.ObjectInteractions\SmartInteractScanSettings.cs | 13 | 2 | mousevec | Vector2 | `public Vector2 mousevec;` | `public Vector2 mousevec;` |
| 2152 | field | Terraria.GameContent.ObjectInteractions.SmartInteractScanSettings | Terraria.GameContent.ObjectInteractions/SmartInteractScanSettings.cs | D:\TRbackup\Version4\Terraria.GameContent.ObjectInteractions\SmartInteractScanSettings.cs | 15 | 2 | LX | int | `public int LX;` | `public int LX;` |
| 2153 | field | Terraria.GameContent.ObjectInteractions.SmartInteractScanSettings | Terraria.GameContent.ObjectInteractions/SmartInteractScanSettings.cs | D:\TRbackup\Version4\Terraria.GameContent.ObjectInteractions\SmartInteractScanSettings.cs | 17 | 2 | HX | int | `public int HX;` | `public int HX;` |
| 2154 | field | Terraria.GameContent.ObjectInteractions.SmartInteractScanSettings | Terraria.GameContent.ObjectInteractions/SmartInteractScanSettings.cs | D:\TRbackup\Version4\Terraria.GameContent.ObjectInteractions\SmartInteractScanSettings.cs | 19 | 2 | LY | int | `public int LY;` | `public int LY;` |
| 2155 | field | Terraria.GameContent.ObjectInteractions.SmartInteractScanSettings | Terraria.GameContent.ObjectInteractions/SmartInteractScanSettings.cs | D:\TRbackup\Version4\Terraria.GameContent.ObjectInteractions\SmartInteractScanSettings.cs | 21 | 2 | HY | int | `public int HY;` | `public int HY;` |
| 2156 | field | Terraria.GameContent.ObjectInteractions.SmartInteractSystem | Terraria.GameContent.ObjectInteractions/SmartInteractSystem.cs | D:\TRbackup\Version4\Terraria.GameContent.ObjectInteractions\SmartInteractSystem.cs | 7 | 2 | _candidateProvidersByOrderOfPriority | System.Collections.Generic.List<Terraria.GameContent.ObjectInteractions.ISmartInteractCandidateProvider> | `private List<ISmartInteractCandidateProvider> _candidateProvidersByOrderOfPriority = new List<ISmartInteractCandidateProvider>();` | `private List<ISmartInteractCandidateProvider> _candidateProvidersByOrderOfPriority = new List<ISmartInteractCandidateProvider>();` |
| 2157 | field | Terraria.GameContent.ObjectInteractions.SmartInteractSystem | Terraria.GameContent.ObjectInteractions/SmartInteractSystem.cs | D:\TRbackup\Version4\Terraria.GameContent.ObjectInteractions\SmartInteractSystem.cs | 9 | 2 | _blockProviders | System.Collections.Generic.List<Terraria.GameContent.ObjectInteractions.ISmartInteractBlockReasonProvider> | `private List<ISmartInteractBlockReasonProvider> _blockProviders = new List<ISmartInteractBlockReasonProvider>();` | `private List<ISmartInteractBlockReasonProvider> _blockProviders = new List<ISmartInteractBlockReasonProvider>();` |
| 2158 | field | Terraria.GameContent.ObjectInteractions.SmartInteractSystem | Terraria.GameContent.ObjectInteractions/SmartInteractSystem.cs | D:\TRbackup\Version4\Terraria.GameContent.ObjectInteractions\SmartInteractSystem.cs | 11 | 2 | _candidates | System.Collections.Generic.List<Terraria.GameContent.ObjectInteractions.ISmartInteractCandidate> | `private List<ISmartInteractCandidate> _candidates = new List<ISmartInteractCandidate>();` | `private List<ISmartInteractCandidate> _candidates = new List<ISmartInteractCandidate>();` |
| 2159 | field | Terraria.GameContent.ObjectInteractions.TileSmartInteractCandidateProvider | Terraria.GameContent.ObjectInteractions/TileSmartInteractCandidateProvider.cs | D:\TRbackup\Version4\Terraria.GameContent.ObjectInteractions\TileSmartInteractCandidateProvider.cs | 16 | 2 | targets | System.Collections.Generic.List<System.Tuple<int, int>> | `private List<Tuple<int, int>> targets = new List<Tuple<int, int>>();` | `private List<Tuple<int, int>> targets = new List<Tuple<int, int>>();` |
| 2160 | field | Terraria.GameContent.ObjectInteractions.TileSmartInteractCandidateProvider | Terraria.GameContent.ObjectInteractions/TileSmartInteractCandidateProvider.cs | D:\TRbackup\Version4\Terraria.GameContent.ObjectInteractions\TileSmartInteractCandidateProvider.cs | 18 | 2 | _candidate | Terraria.GameContent.ObjectInteractions.TileSmartInteractCandidateProvider.ReusableCandidate | `private ReusableCandidate _candidate = new ReusableCandidate();` | `private ReusableCandidate _candidate = new ReusableCandidate();` |

#### 属性（5）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3823 | property | Terraria.GameContent.ObjectInteractions.ISmartInteractCandidate | Terraria.GameContent.ObjectInteractions/ISmartInteractCandidate.cs | D:\TRbackup\Version4\Terraria.GameContent.ObjectInteractions\ISmartInteractCandidate.cs | 5 | 2 | DistanceFromCursor | float | `float DistanceFromCursor { get; }` | `float DistanceFromCursor { get; }` |
| 3824 | property | Terraria.GameContent.ObjectInteractions.NPCSmartInteractCandidateProvider.ReusableCandidate | Terraria.GameContent.ObjectInteractions/NPCSmartInteractCandidateProvider.cs | D:\TRbackup\Version4\Terraria.GameContent.ObjectInteractions\NPCSmartInteractCandidateProvider.cs | 11 | 3 | DistanceFromCursor | float | `public float DistanceFromCursor { get; private set; }` | `public float DistanceFromCursor { get; private set; }` |
| 3825 | property | Terraria.GameContent.ObjectInteractions.PotionOfReturnSmartInteractCandidateProvider.ReusableCandidate | Terraria.GameContent.ObjectInteractions/PotionOfReturnSmartInteractCandidateProvider.cs | D:\TRbackup\Version4\Terraria.GameContent.ObjectInteractions\PotionOfReturnSmartInteractCandidateProvider.cs | 9 | 3 | DistanceFromCursor | float | `public float DistanceFromCursor { get; private set; }` | `public float DistanceFromCursor { get; private set; }` |
| 3826 | property | Terraria.GameContent.ObjectInteractions.ProjectileSmartInteractCandidateProvider.ReusableCandidate | Terraria.GameContent.ObjectInteractions/ProjectileSmartInteractCandidateProvider.cs | D:\TRbackup\Version4\Terraria.GameContent.ObjectInteractions\ProjectileSmartInteractCandidateProvider.cs | 11 | 3 | DistanceFromCursor | float | `public float DistanceFromCursor { get; private set; }` | `public float DistanceFromCursor { get; private set; }` |
| 3827 | property | Terraria.GameContent.ObjectInteractions.TileSmartInteractCandidateProvider.ReusableCandidate | Terraria.GameContent.ObjectInteractions/TileSmartInteractCandidateProvider.cs | D:\TRbackup\Version4\Terraria.GameContent.ObjectInteractions\TileSmartInteractCandidateProvider.cs | 13 | 3 | DistanceFromCursor | float | `public float DistanceFromCursor { get; private set; }` | `public float DistanceFromCursor { get; private set; }` |


### 4.12 细分子系统：`PlayerItemPickupAndRespawnState`

- 原报告章节：`4.9.78`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`08` / `玩家输入与玩法`

- 上一级基线细分子系统：`PlayerItemPickupAndRespawnState`
- 细分职责：玩家物品领取日志、召唤物生成和重生状态。
- 边界角色：`state`；最小 seam：player pickup respawn port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：3；声明类型数：4；字段：9；属性：0；合计：9。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（9）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1188 | field | Terraria.DataStructures.MinionRespawner | Terraria.DataStructures/MinionRespawner.cs | D:\TRbackup\Version4\Terraria.DataStructures\MinionRespawner.cs | 7 | 2 | _minions | System.Collections.Generic.List<Terraria.DataStructures.MinionSpawnInfo> | `private List<MinionSpawnInfo> _minions = new List<MinionSpawnInfo>();` | `private List<MinionSpawnInfo> _minions = new List<MinionSpawnInfo>();` |
| 1189 | field | Terraria.DataStructures.MinionSpawnFromInventoryItem | Terraria.DataStructures/MinionSpawnFromInventoryItem.cs | D:\TRbackup\Version4\Terraria.DataStructures\MinionSpawnFromInventoryItem.cs | 5 | 2 | ItemType | int | `public int ItemType;` | `public int ItemType;` |
| 1190 | field | Terraria.DataStructures.MinionSpawnFromInventoryItem | Terraria.DataStructures/MinionSpawnFromInventoryItem.cs | D:\TRbackup\Version4\Terraria.DataStructures\MinionSpawnFromInventoryItem.cs | 7 | 2 | ItemPrefix | int | `public int ItemPrefix;` | `public int ItemPrefix;` |
| 1230 | field | Terraria.DataStructures.PlayerGetItemLogger.GetItemLoggerEntry | Terraria.DataStructures/PlayerGetItemLogger.cs | D:\TRbackup\Version4\Terraria.DataStructures\PlayerGetItemLogger.cs | 9 | 3 | TargetArray | Terraria.Item[] | `public Item[] TargetArray;` | `public Item[] TargetArray;` |
| 1231 | field | Terraria.DataStructures.PlayerGetItemLogger.GetItemLoggerEntry | Terraria.DataStructures/PlayerGetItemLogger.cs | D:\TRbackup\Version4\Terraria.DataStructures\PlayerGetItemLogger.cs | 11 | 3 | TargetSlot | int | `public int TargetSlot;` | `public int TargetSlot;` |
| 1232 | field | Terraria.DataStructures.PlayerGetItemLogger.GetItemLoggerEntry | Terraria.DataStructures/PlayerGetItemLogger.cs | D:\TRbackup\Version4\Terraria.DataStructures\PlayerGetItemLogger.cs | 13 | 3 | TargetItemSlotContext | int | `public int TargetItemSlotContext;` | `public int TargetItemSlotContext;` |
| 1233 | field | Terraria.DataStructures.PlayerGetItemLogger.GetItemLoggerEntry | Terraria.DataStructures/PlayerGetItemLogger.cs | D:\TRbackup\Version4\Terraria.DataStructures\PlayerGetItemLogger.cs | 15 | 3 | Stack | int | `public int Stack;` | `public int Stack;` |
| 1234 | field | Terraria.DataStructures.PlayerGetItemLogger | Terraria.DataStructures/PlayerGetItemLogger.cs | D:\TRbackup\Version4\Terraria.DataStructures\PlayerGetItemLogger.cs | 18 | 2 | Entries | System.Collections.Generic.List<Terraria.DataStructures.PlayerGetItemLogger.GetItemLoggerEntry> | `public List<GetItemLoggerEntry> Entries = new List<GetItemLoggerEntry>();` | `public List<GetItemLoggerEntry> Entries = new List<GetItemLoggerEntry>();` |
| 1235 | field | Terraria.DataStructures.PlayerGetItemLogger | Terraria.DataStructures/PlayerGetItemLogger.cs | D:\TRbackup\Version4\Terraria.DataStructures\PlayerGetItemLogger.cs | 20 | 2 | _enabled | bool | `private bool _enabled;` | `private bool _enabled;` |

#### 属性（0）

无该类型成员记录。


### 4.13 细分子系统：`PlayerPreviewAndRejectionState`

- 原报告章节：`4.9.79`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`08` / `玩家输入与玩法`

- 上一级基线细分子系统：`PlayerPreviewAndRejectionState`
- 细分职责：角色预览设置和拒绝菜单载荷。
- 边界角色：`presentation/state`；最小 seam：player preview rejection port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：2；声明类型数：3；字段：11；属性：0；合计：11。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（11）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1272 | field | Terraria.DataStructures.RejectionMenuInfo | Terraria.DataStructures/RejectionMenuInfo.cs | D:\TRbackup\Version4\Terraria.DataStructures\RejectionMenuInfo.cs | 7 | 2 | ExitAction | Terraria.DataStructures.ReturnFromRejectionMenuAction | `public ReturnFromRejectionMenuAction ExitAction;` | `public ReturnFromRejectionMenuAction ExitAction;` |
| 1273 | field | Terraria.DataStructures.RejectionMenuInfo | Terraria.DataStructures/RejectionMenuInfo.cs | D:\TRbackup\Version4\Terraria.DataStructures\RejectionMenuInfo.cs | 9 | 2 | TextToShow | string | `public string TextToShow;` | `public string TextToShow;` |
| 1274 | field | Terraria.DataStructures.SettingsForCharacterPreview.SelectionBasedSettings | Terraria.DataStructures/SettingsForCharacterPreview.cs | D:\TRbackup\Version4\Terraria.DataStructures\SettingsForCharacterPreview.cs | 11 | 3 | StartFrame | int | `public int StartFrame;` | `public int StartFrame;` |
| 1275 | field | Terraria.DataStructures.SettingsForCharacterPreview.SelectionBasedSettings | Terraria.DataStructures/SettingsForCharacterPreview.cs | D:\TRbackup\Version4\Terraria.DataStructures\SettingsForCharacterPreview.cs | 13 | 3 | FrameCount | int | `public int FrameCount;` | `public int FrameCount;` |
| 1276 | field | Terraria.DataStructures.SettingsForCharacterPreview.SelectionBasedSettings | Terraria.DataStructures/SettingsForCharacterPreview.cs | D:\TRbackup\Version4\Terraria.DataStructures\SettingsForCharacterPreview.cs | 15 | 3 | DelayPerFrame | int | `public int DelayPerFrame;` | `public int DelayPerFrame;` |
| 1277 | field | Terraria.DataStructures.SettingsForCharacterPreview.SelectionBasedSettings | Terraria.DataStructures/SettingsForCharacterPreview.cs | D:\TRbackup\Version4\Terraria.DataStructures\SettingsForCharacterPreview.cs | 17 | 3 | BounceLoop | bool | `public bool BounceLoop;` | `public bool BounceLoop;` |
| 1278 | field | Terraria.DataStructures.SettingsForCharacterPreview | Terraria.DataStructures/SettingsForCharacterPreview.cs | D:\TRbackup\Version4\Terraria.DataStructures\SettingsForCharacterPreview.cs | 20 | 2 | Offset | Vector2 | `public Vector2 Offset;` | `public Vector2 Offset;` |
| 1279 | field | Terraria.DataStructures.SettingsForCharacterPreview | Terraria.DataStructures/SettingsForCharacterPreview.cs | D:\TRbackup\Version4\Terraria.DataStructures\SettingsForCharacterPreview.cs | 22 | 2 | Selected | Terraria.DataStructures.SettingsForCharacterPreview.SelectionBasedSettings | `public SelectionBasedSettings Selected;` | `public SelectionBasedSettings Selected;` |
| 1280 | field | Terraria.DataStructures.SettingsForCharacterPreview | Terraria.DataStructures/SettingsForCharacterPreview.cs | D:\TRbackup\Version4\Terraria.DataStructures\SettingsForCharacterPreview.cs | 24 | 2 | NotSelected | Terraria.DataStructures.SettingsForCharacterPreview.SelectionBasedSettings | `public SelectionBasedSettings NotSelected;` | `public SelectionBasedSettings NotSelected;` |
| 1281 | field | Terraria.DataStructures.SettingsForCharacterPreview | Terraria.DataStructures/SettingsForCharacterPreview.cs | D:\TRbackup\Version4\Terraria.DataStructures\SettingsForCharacterPreview.cs | 26 | 2 | SpriteDirection | int | `public int SpriteDirection = 1;` | `public int SpriteDirection = 1;` |
| 1282 | field | Terraria.DataStructures.SettingsForCharacterPreview | Terraria.DataStructures/SettingsForCharacterPreview.cs | D:\TRbackup\Version4\Terraria.DataStructures\SettingsForCharacterPreview.cs | 28 | 2 | CustomAnimation | Terraria.DataStructures.SettingsForCharacterPreview.CustomAnimationCode | `public CustomAnimationCode CustomAnimation;` | `public CustomAnimationCode CustomAnimation;` |

#### 属性（0）

无该类型成员记录。


### 4.14 细分子系统：`DoorOpeningInteractionState`

- 原报告章节：`4.9.95`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`08` / `玩家输入与玩法`

- 上一级基线细分子系统：`DoorOpeningInteractionState`
- 细分职责：门开启/关闭候选、玩家信息和切换状态。
- 边界角色：`state/query`；最小 seam：door interaction port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：4；字段：10；属性：0；合计：10。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（10）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2374 | field | Terraria.GameContent.DoorOpeningHelper.DoorOpenCloseTogglingInfo | Terraria.GameContent/DoorOpeningHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\DoorOpeningHelper.cs | 21 | 3 | tileCoordsForToggling | Point | `public Point tileCoordsForToggling;` | `public Point tileCoordsForToggling;` |
| 2375 | field | Terraria.GameContent.DoorOpeningHelper.DoorOpenCloseTogglingInfo | Terraria.GameContent/DoorOpeningHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\DoorOpeningHelper.cs | 23 | 3 | handler | Terraria.GameContent.DoorOpeningHelper.DoorAutoHandler | `public DoorAutoHandler handler;` | `public DoorAutoHandler handler;` |
| 2376 | field | Terraria.GameContent.DoorOpeningHelper.PlayerInfoForOpeningDoors | Terraria.GameContent/DoorOpeningHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\DoorOpeningHelper.cs | 28 | 3 | hitboxToOpenDoor | Rectangle | `public Rectangle hitboxToOpenDoor;` | `public Rectangle hitboxToOpenDoor;` |
| 2377 | field | Terraria.GameContent.DoorOpeningHelper.PlayerInfoForOpeningDoors | Terraria.GameContent/DoorOpeningHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\DoorOpeningHelper.cs | 30 | 3 | intendedOpeningDirection | int | `public int intendedOpeningDirection;` | `public int intendedOpeningDirection;` |
| 2378 | field | Terraria.GameContent.DoorOpeningHelper.PlayerInfoForOpeningDoors | Terraria.GameContent/DoorOpeningHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\DoorOpeningHelper.cs | 32 | 3 | playerGravityDirection | int | `public int playerGravityDirection;` | `public int playerGravityDirection;` |
| 2379 | field | Terraria.GameContent.DoorOpeningHelper.PlayerInfoForOpeningDoors | Terraria.GameContent/DoorOpeningHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\DoorOpeningHelper.cs | 34 | 3 | tileCoordSpaceForCheckingForDoors | Rectangle | `public Rectangle tileCoordSpaceForCheckingForDoors;` | `public Rectangle tileCoordSpaceForCheckingForDoors;` |
| 2380 | field | Terraria.GameContent.DoorOpeningHelper.PlayerInfoForClosingDoors | Terraria.GameContent/DoorOpeningHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\DoorOpeningHelper.cs | 39 | 3 | hitboxToNotCloseDoor | Rectangle | `public Rectangle hitboxToNotCloseDoor;` | `public Rectangle hitboxToNotCloseDoor;` |
| 2381 | field | Terraria.GameContent.DoorOpeningHelper | Terraria.GameContent/DoorOpeningHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\DoorOpeningHelper.cs | 77 | 2 | _handlerByTileType | System.Collections.Generic.Dictionary<int, Terraria.GameContent.DoorOpeningHelper.DoorAutoHandler> | `private Dictionary<int, DoorAutoHandler> _handlerByTileType = new Dictionary<int, DoorAutoHandler>  	{  		{  			10,  			new CommonDoorOpeningInfoProvider()  		},  		{  			388,  			new TallGateOpeningInfoProvider()  		}  	};` | `private Dictionary<int, DoorAutoHandler> _handlerByTileType = new Dictionary<int, DoorAutoHandler> { { 10, new CommonDoorOpeningInfoProvider() }, { 388, new TallGateOpeningInfoProvider() } };` |
| 2382 | field | Terraria.GameContent.DoorOpeningHelper | Terraria.GameContent/DoorOpeningHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\DoorOpeningHelper.cs | 89 | 2 | _ongoingOpenDoors | System.Collections.Generic.List<Terraria.GameContent.DoorOpeningHelper.DoorOpenCloseTogglingInfo> | `private List<DoorOpenCloseTogglingInfo> _ongoingOpenDoors = new List<DoorOpenCloseTogglingInfo>();` | `private List<DoorOpenCloseTogglingInfo> _ongoingOpenDoors = new List<DoorOpenCloseTogglingInfo>();` |
| 2383 | field | Terraria.GameContent.DoorOpeningHelper | Terraria.GameContent/DoorOpeningHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\DoorOpeningHelper.cs | 91 | 2 | _timeWeCanOpenDoorsUsingVelocityAlone | int | `private int _timeWeCanOpenDoorsUsingVelocityAlone;` | `private int _timeWeCanOpenDoorsUsingVelocityAlone;` |

#### 属性（0）

无该类型成员记录。


### 4.15 细分子系统：`SmartCursorInteractionState`

- 原报告章节：`4.9.96`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`08` / `玩家输入与玩法`

- 上一级基线细分子系统：`SmartCursorInteractionState`
- 细分职责：智能游标目标、抓钩目标和使用信息。
- 边界角色：`query/state`；最小 seam：smart cursor interaction port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：2；字段：19；属性：0；合计：19。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（19）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2519 | field | Terraria.GameContent.SmartCursorHelper.SmartCursorUsageInfo | Terraria.GameContent/SmartCursorHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\SmartCursorHelper.cs | 16 | 3 | player | Terraria.Player | `public Player player;` | `public Player player;` |
| 2520 | field | Terraria.GameContent.SmartCursorHelper.SmartCursorUsageInfo | Terraria.GameContent/SmartCursorHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\SmartCursorHelper.cs | 18 | 3 | item | Terraria.Item | `public Item item;` | `public Item item;` |
| 2521 | field | Terraria.GameContent.SmartCursorHelper.SmartCursorUsageInfo | Terraria.GameContent/SmartCursorHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\SmartCursorHelper.cs | 20 | 3 | mouse | Vector2 | `public Vector2 mouse;` | `public Vector2 mouse;` |
| 2522 | field | Terraria.GameContent.SmartCursorHelper.SmartCursorUsageInfo | Terraria.GameContent/SmartCursorHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\SmartCursorHelper.cs | 22 | 3 | position | Vector2 | `public Vector2 position;` | `public Vector2 position;` |
| 2523 | field | Terraria.GameContent.SmartCursorHelper.SmartCursorUsageInfo | Terraria.GameContent/SmartCursorHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\SmartCursorHelper.cs | 24 | 3 | Center | Vector2 | `public Vector2 Center;` | `public Vector2 Center;` |
| 2524 | field | Terraria.GameContent.SmartCursorHelper.SmartCursorUsageInfo | Terraria.GameContent/SmartCursorHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\SmartCursorHelper.cs | 26 | 3 | screenTargetX | int | `public int screenTargetX;` | `public int screenTargetX;` |
| 2525 | field | Terraria.GameContent.SmartCursorHelper.SmartCursorUsageInfo | Terraria.GameContent/SmartCursorHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\SmartCursorHelper.cs | 28 | 3 | screenTargetY | int | `public int screenTargetY;` | `public int screenTargetY;` |
| 2526 | field | Terraria.GameContent.SmartCursorHelper.SmartCursorUsageInfo | Terraria.GameContent/SmartCursorHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\SmartCursorHelper.cs | 30 | 3 | reachableStartX | int | `public int reachableStartX;` | `public int reachableStartX;` |
| 2527 | field | Terraria.GameContent.SmartCursorHelper.SmartCursorUsageInfo | Terraria.GameContent/SmartCursorHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\SmartCursorHelper.cs | 32 | 3 | reachableEndX | int | `public int reachableEndX;` | `public int reachableEndX;` |
| 2528 | field | Terraria.GameContent.SmartCursorHelper.SmartCursorUsageInfo | Terraria.GameContent/SmartCursorHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\SmartCursorHelper.cs | 34 | 3 | reachableStartY | int | `public int reachableStartY;` | `public int reachableStartY;` |
| 2529 | field | Terraria.GameContent.SmartCursorHelper.SmartCursorUsageInfo | Terraria.GameContent/SmartCursorHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\SmartCursorHelper.cs | 36 | 3 | reachableEndY | int | `public int reachableEndY;` | `public int reachableEndY;` |
| 2530 | field | Terraria.GameContent.SmartCursorHelper.SmartCursorUsageInfo | Terraria.GameContent/SmartCursorHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\SmartCursorHelper.cs | 38 | 3 | paintLookup | int | `public int paintLookup;` | `public int paintLookup;` |
| 2531 | field | Terraria.GameContent.SmartCursorHelper.SmartCursorUsageInfo | Terraria.GameContent/SmartCursorHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\SmartCursorHelper.cs | 40 | 3 | paintCoatingLookup | int | `public int paintCoatingLookup;` | `public int paintCoatingLookup;` |
| 2532 | field | Terraria.GameContent.SmartCursorHelper | Terraria.GameContent/SmartCursorHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\SmartCursorHelper.cs | 43 | 2 | _targets | System.Collections.Generic.List<Point> | `private static List<Point> _targets = new List<Point>();` | `private static List<Point> _targets = new List<Point>();` |
| 2533 | field | Terraria.GameContent.SmartCursorHelper | Terraria.GameContent/SmartCursorHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\SmartCursorHelper.cs | 45 | 2 | _grappleTargets | System.Collections.Generic.List<Point> | `private static List<Point> _grappleTargets = new List<Point>();` | `private static List<Point> _grappleTargets = new List<Point>();` |
| 2534 | field | Terraria.GameContent.SmartCursorHelper | Terraria.GameContent/SmartCursorHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\SmartCursorHelper.cs | 47 | 2 | _points | System.Collections.Generic.List<Point> | `private static List<Point> _points = new List<Point>();` | `private static List<Point> _points = new List<Point>();` |
| 2535 | field | Terraria.GameContent.SmartCursorHelper | Terraria.GameContent/SmartCursorHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\SmartCursorHelper.cs | 49 | 2 | _endpoints | System.Collections.Generic.List<Point> | `private static List<Point> _endpoints = new List<Point>();` | `private static List<Point> _endpoints = new List<Point>();` |
| 2536 | field | Terraria.GameContent.SmartCursorHelper | Terraria.GameContent/SmartCursorHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\SmartCursorHelper.cs | 51 | 2 | _toRemove | System.Collections.Generic.List<Point> | `private static List<Point> _toRemove = new List<Point>();` | `private static List<Point> _toRemove = new List<Point>();` |
| 2537 | field | Terraria.GameContent.SmartCursorHelper | Terraria.GameContent/SmartCursorHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\SmartCursorHelper.cs | 53 | 2 | _targets2 | System.Collections.Generic.List<Point> | `private static List<Point> _targets2 = new List<Point>();` | `private static List<Point> _targets2 = new List<Point>();` |

#### 属性（0）

无该类型成员记录。


### 4.16 细分子系统：`PressurePlateInteractionState`

- 原报告章节：`4.9.97`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`08` / `玩家输入与玩法`

- 上一级基线细分子系统：`PressurePlateInteractionState`
- 细分职责：压力板检测锁和被按压集合状态。
- 边界角色：`state/query`；最小 seam：pressure plate interaction port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：5；属性：0；合计：5。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（5）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2467 | field | Terraria.GameContent.PressurePlateHelper | Terraria.GameContent/PressurePlateHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\PressurePlateHelper.cs | 9 | 2 | EntityCreationLock | object | `public static object EntityCreationLock = new object();` | `public static object EntityCreationLock = new object();` |
| 2468 | field | Terraria.GameContent.PressurePlateHelper | Terraria.GameContent/PressurePlateHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\PressurePlateHelper.cs | 11 | 2 | PressurePlatesPressed | System.Collections.Generic.Dictionary<Point, bool[]> | `public static Dictionary<Point, bool[]> PressurePlatesPressed = new Dictionary<Point, bool[]>();` | `public static Dictionary<Point, bool[]> PressurePlatesPressed = new Dictionary<Point, bool[]>();` |
| 2469 | field | Terraria.GameContent.PressurePlateHelper | Terraria.GameContent/PressurePlateHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\PressurePlateHelper.cs | 13 | 2 | NeedsFirstUpdate | bool | `public static bool NeedsFirstUpdate;` | `public static bool NeedsFirstUpdate;` |
| 2470 | field | Terraria.GameContent.PressurePlateHelper | Terraria.GameContent/PressurePlateHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\PressurePlateHelper.cs | 15 | 2 | PlayerLastPosition | Vector2[] | `private static Vector2[] PlayerLastPosition = new Vector2[255];` | `private static Vector2[] PlayerLastPosition = new Vector2[255];` |
| 2471 | field | Terraria.GameContent.PressurePlateHelper | Terraria.GameContent/PressurePlateHelper.cs | D:\TRbackup\Version4\Terraria.GameContent\PressurePlateHelper.cs | 17 | 2 | pressurePlateBounds | Rectangle | `private static Rectangle pressurePlateBounds = new Rectangle(0, 0, 16, 10);` | `private static Rectangle pressurePlateBounds = new Rectangle(0, 0, 16, 10);` |

#### 属性（0）

无该类型成员记录。


### 4.17 细分子系统：`CursorAndChestInteractionState`

- 原报告章节：`4.9.98`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`08` / `玩家输入与玩法`

- 上一级基线细分子系统：`CursorAndChestInteractionState`
- 细分职责：虚拟游标物品和定位 Chest 交互状态。
- 边界角色：`state/query`；最小 seam：cursor chest interaction port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：2；声明类型数：2；字段：6；属性：1；合计：7。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（6）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2419 | field | Terraria.GameContent.FakeCursorItem | Terraria.GameContent/FakeCursorItem.cs | D:\TRbackup\Version4\Terraria.GameContent\FakeCursorItem.cs | 5 | 2 | _type | int | `private static int _type;` | `private static int _type;` |
| 2420 | field | Terraria.GameContent.FakeCursorItem | Terraria.GameContent/FakeCursorItem.cs | D:\TRbackup\Version4\Terraria.GameContent\FakeCursorItem.cs | 7 | 2 | _stack | int | `private static int _stack;` | `private static int _stack;` |
| 2421 | field | Terraria.GameContent.FakeCursorItem | Terraria.GameContent/FakeCursorItem.cs | D:\TRbackup\Version4\Terraria.GameContent\FakeCursorItem.cs | 9 | 2 | _prefix | int | `private static int _prefix;` | `private static int _prefix;` |
| 2422 | field | Terraria.GameContent.FakeCursorItem | Terraria.GameContent/FakeCursorItem.cs | D:\TRbackup\Version4\Terraria.GameContent\FakeCursorItem.cs | 11 | 2 | _item | Terraria.Item | `private static Item _item = new Item();` | `private static Item _item = new Item();` |
| 2465 | field | Terraria.GameContent.PositionedChest | Terraria.GameContent/PositionedChest.cs | D:\TRbackup\Version4\Terraria.GameContent\PositionedChest.cs | 7 | 2 | chest | Terraria.Chest | `public Chest chest;` | `public Chest chest;` |
| 2466 | field | Terraria.GameContent.PositionedChest | Terraria.GameContent/PositionedChest.cs | D:\TRbackup\Version4\Terraria.GameContent\PositionedChest.cs | 9 | 2 | position | Vector2 | `public Vector2 position;` | `public Vector2 position;` |

#### 属性（1）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3853 | property | Terraria.GameContent.FakeCursorItem | Terraria.GameContent/FakeCursorItem.cs | D:\TRbackup\Version4\Terraria.GameContent\FakeCursorItem.cs | 13 | 2 | Item | Terraria.Item | `public static Item Item { get { int num = ((!Main.mouseItem.IsAir) ? Main.mouseItem.stack : 0); if (_type != _item.type) { _item.SetDefaults(_type); } else { _item.Refresh(); } if (_prefix != _item.prefix) { _item.Prefix(_prefix); } _item.stack = _stack + num; return _item; } }` | `public static Item Item { get { int num = ((!Main.mouseItem.IsAir) ? Main.mouseItem.stack : 0); if (_type != _item.type) { _item.SetDefaults(_type); } else { _item.Refresh(); } if (_prefix != _item.prefix) { _item.Prefix(_prefix); } _item.stack = _stack + num; return _item; } }` |


### 4.18 细分子系统：`InputProfilesAndConfiguration`

- 原报告章节：`4.9.104`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`08` / `玩家输入与玩法`

- 上一级基线细分子系统：`InputProfilesAndConfiguration`
- 细分职责：玩家输入档案和按键配置。
- 边界角色：`adapter/state`；最小 seam：input profile configuration port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：2；声明类型数：2；字段：16；属性：0；合计：16。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（16）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2600 | field | Terraria.GameInput.KeyConfiguration | Terraria.GameInput/KeyConfiguration.cs | D:\TRbackup\Version4\Terraria.GameInput\KeyConfiguration.cs | 8 | 2 | KeyStatus | System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<string>> | `public Dictionary<string, List<string>> KeyStatus = new Dictionary<string, List<string>>();` | `public Dictionary<string, List<string>> KeyStatus = new Dictionary<string, List<string>>();` |
| 2609 | field | Terraria.GameInput.PlayerInputProfile | Terraria.GameInput/PlayerInputProfile.cs | D:\TRbackup\Version4\Terraria.GameInput\PlayerInputProfile.cs | 11 | 2 | InputModes | System.Collections.Generic.Dictionary<Terraria.GameInput.InputMode, Terraria.GameInput.KeyConfiguration> | `public Dictionary<InputMode, KeyConfiguration> InputModes = new Dictionary<InputMode, KeyConfiguration>  	{  		{  			InputMode.Keyboard,  			new KeyConfiguration()  		},  		{  			InputMode.KeyboardUI,  			new KeyConfiguration()  		},  		{  			InputMode.XBoxGamepad,  			new KeyConfiguration()  		},  		{  			InputMode.XBoxGamepadUI,  			new KeyConfiguration()  		}  	};` | `public Dictionary<InputMode, KeyConfiguration> InputModes = new Dictionary<InputMode, KeyConfiguration> { { InputMode.Keyboard, new KeyConfiguration() }, { InputMode.KeyboardUI, new KeyConfiguration() }, { InputMode.XBoxGamepad, new KeyConfiguration() }, { InputMode.XBoxGamepadUI, new KeyConfiguration() } };` |
| 2610 | field | Terraria.GameInput.PlayerInputProfile | Terraria.GameInput/PlayerInputProfile.cs | D:\TRbackup\Version4\Terraria.GameInput\PlayerInputProfile.cs | 31 | 2 | Name | string | `public string Name = "";` | `public string Name = "";` |
| 2611 | field | Terraria.GameInput.PlayerInputProfile | Terraria.GameInput/PlayerInputProfile.cs | D:\TRbackup\Version4\Terraria.GameInput\PlayerInputProfile.cs | 33 | 2 | AllowEditing | bool | `public bool AllowEditing = true;` | `public bool AllowEditing = true;` |
| 2612 | field | Terraria.GameInput.PlayerInputProfile | Terraria.GameInput/PlayerInputProfile.cs | D:\TRbackup\Version4\Terraria.GameInput\PlayerInputProfile.cs | 35 | 2 | HotbarRadialHoldTimeRequired | int | `public int HotbarRadialHoldTimeRequired = 16;` | `public int HotbarRadialHoldTimeRequired = 16;` |
| 2613 | field | Terraria.GameInput.PlayerInputProfile | Terraria.GameInput/PlayerInputProfile.cs | D:\TRbackup\Version4\Terraria.GameInput\PlayerInputProfile.cs | 37 | 2 | TriggersDeadzone | float | `public float TriggersDeadzone = 0.3f;` | `public float TriggersDeadzone = 0.3f;` |
| 2614 | field | Terraria.GameInput.PlayerInputProfile | Terraria.GameInput/PlayerInputProfile.cs | D:\TRbackup\Version4\Terraria.GameInput\PlayerInputProfile.cs | 39 | 2 | InterfaceDeadzoneX | float | `public float InterfaceDeadzoneX = 0.2f;` | `public float InterfaceDeadzoneX = 0.2f;` |
| 2615 | field | Terraria.GameInput.PlayerInputProfile | Terraria.GameInput/PlayerInputProfile.cs | D:\TRbackup\Version4\Terraria.GameInput\PlayerInputProfile.cs | 41 | 2 | LeftThumbstickDeadzoneX | float | `public float LeftThumbstickDeadzoneX = 0.25f;` | `public float LeftThumbstickDeadzoneX = 0.25f;` |
| 2616 | field | Terraria.GameInput.PlayerInputProfile | Terraria.GameInput/PlayerInputProfile.cs | D:\TRbackup\Version4\Terraria.GameInput\PlayerInputProfile.cs | 43 | 2 | LeftThumbstickDeadzoneY | float | `public float LeftThumbstickDeadzoneY = 0.4f;` | `public float LeftThumbstickDeadzoneY = 0.4f;` |
| 2617 | field | Terraria.GameInput.PlayerInputProfile | Terraria.GameInput/PlayerInputProfile.cs | D:\TRbackup\Version4\Terraria.GameInput\PlayerInputProfile.cs | 45 | 2 | RightThumbstickDeadzoneX | float | `public float RightThumbstickDeadzoneX;` | `public float RightThumbstickDeadzoneX;` |
| 2618 | field | Terraria.GameInput.PlayerInputProfile | Terraria.GameInput/PlayerInputProfile.cs | D:\TRbackup\Version4\Terraria.GameInput\PlayerInputProfile.cs | 47 | 2 | RightThumbstickDeadzoneY | float | `public float RightThumbstickDeadzoneY;` | `public float RightThumbstickDeadzoneY;` |
| 2619 | field | Terraria.GameInput.PlayerInputProfile | Terraria.GameInput/PlayerInputProfile.cs | D:\TRbackup\Version4\Terraria.GameInput\PlayerInputProfile.cs | 49 | 2 | LeftThumbstickInvertX | bool | `public bool LeftThumbstickInvertX;` | `public bool LeftThumbstickInvertX;` |
| 2620 | field | Terraria.GameInput.PlayerInputProfile | Terraria.GameInput/PlayerInputProfile.cs | D:\TRbackup\Version4\Terraria.GameInput\PlayerInputProfile.cs | 51 | 2 | LeftThumbstickInvertY | bool | `public bool LeftThumbstickInvertY;` | `public bool LeftThumbstickInvertY;` |
| 2621 | field | Terraria.GameInput.PlayerInputProfile | Terraria.GameInput/PlayerInputProfile.cs | D:\TRbackup\Version4\Terraria.GameInput\PlayerInputProfile.cs | 53 | 2 | RightThumbstickInvertX | bool | `public bool RightThumbstickInvertX;` | `public bool RightThumbstickInvertX;` |
| 2622 | field | Terraria.GameInput.PlayerInputProfile | Terraria.GameInput/PlayerInputProfile.cs | D:\TRbackup\Version4\Terraria.GameInput\PlayerInputProfile.cs | 55 | 2 | RightThumbstickInvertY | bool | `public bool RightThumbstickInvertY;` | `public bool RightThumbstickInvertY;` |
| 2623 | field | Terraria.GameInput.PlayerInputProfile | Terraria.GameInput/PlayerInputProfile.cs | D:\TRbackup\Version4\Terraria.GameInput\PlayerInputProfile.cs | 57 | 2 | InventoryMoveCD | int | `public int InventoryMoveCD = 6;` | `public int InventoryMoveCD = 6;` |

#### 属性（0）

无该类型成员记录。


### 4.19 细分子系统：`InputTriggerState`

- 原报告章节：`4.9.105`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`08` / `玩家输入与玩法`

- 上一级基线细分子系统：`InputTriggerState`
- 细分职责：触发器集合、当前输入模式和触发器打包状态。
- 边界角色：`adapter/state`；最小 seam：input trigger port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：2；声明类型数：2；字段：9；属性：0；合计：9。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（9）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2624 | field | Terraria.GameInput.TriggersPack | Terraria.GameInput/TriggersPack.cs | D:\TRbackup\Version4\Terraria.GameInput\TriggersPack.cs | 7 | 2 | Current | Terraria.GameInput.TriggersSet | `public TriggersSet Current = new TriggersSet();` | `public TriggersSet Current = new TriggersSet();` |
| 2625 | field | Terraria.GameInput.TriggersPack | Terraria.GameInput/TriggersPack.cs | D:\TRbackup\Version4\Terraria.GameInput\TriggersPack.cs | 9 | 2 | Old | Terraria.GameInput.TriggersSet | `public TriggersSet Old = new TriggersSet();` | `public TriggersSet Old = new TriggersSet();` |
| 2626 | field | Terraria.GameInput.TriggersPack | Terraria.GameInput/TriggersPack.cs | D:\TRbackup\Version4\Terraria.GameInput\TriggersPack.cs | 11 | 2 | JustPressed | Terraria.GameInput.TriggersSet | `public TriggersSet JustPressed = new TriggersSet();` | `public TriggersSet JustPressed = new TriggersSet();` |
| 2627 | field | Terraria.GameInput.TriggersPack | Terraria.GameInput/TriggersPack.cs | D:\TRbackup\Version4\Terraria.GameInput\TriggersPack.cs | 13 | 2 | JustReleased | Terraria.GameInput.TriggersSet | `public TriggersSet JustReleased = new TriggersSet();` | `public TriggersSet JustReleased = new TriggersSet();` |
| 2628 | field | Terraria.GameInput.TriggersSet | Terraria.GameInput/TriggersSet.cs | D:\TRbackup\Version4\Terraria.GameInput\TriggersSet.cs | 9 | 2 | KeyStatus | System.Collections.Generic.Dictionary<string, bool> | `public Dictionary<string, bool> KeyStatus = new Dictionary<string, bool>();` | `public Dictionary<string, bool> KeyStatus = new Dictionary<string, bool>();` |
| 2629 | field | Terraria.GameInput.TriggersSet | Terraria.GameInput/TriggersSet.cs | D:\TRbackup\Version4\Terraria.GameInput\TriggersSet.cs | 11 | 2 | LatestInputMode | System.Collections.Generic.Dictionary<string, Terraria.GameInput.InputMode> | `public Dictionary<string, InputMode> LatestInputMode = new Dictionary<string, InputMode>();` | `public Dictionary<string, InputMode> LatestInputMode = new Dictionary<string, InputMode>();` |
| 2630 | field | Terraria.GameInput.TriggersSet | Terraria.GameInput/TriggersSet.cs | D:\TRbackup\Version4\Terraria.GameInput\TriggersSet.cs | 13 | 2 | UsedMovementKey | bool | `public bool UsedMovementKey = true;` | `public bool UsedMovementKey = true;` |
| 2631 | field | Terraria.GameInput.TriggersSet | Terraria.GameInput/TriggersSet.cs | D:\TRbackup\Version4\Terraria.GameInput\TriggersSet.cs | 15 | 2 | HotbarScrollCD | int | `public int HotbarScrollCD;` | `public int HotbarScrollCD;` |
| 2632 | field | Terraria.GameInput.TriggersSet | Terraria.GameInput/TriggersSet.cs | D:\TRbackup\Version4\Terraria.GameInput\TriggersSet.cs | 17 | 2 | HotbarHoldTime | int | `public int HotbarHoldTime;` | `public int HotbarHoldTime;` |

#### 属性（0）

无该类型成员记录。


### 4.20 细分子系统：`PlayerInputRuntimeState`

- 原报告章节：`4.9.106`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`08` / `玩家输入与玩法`

- 上一级基线细分子系统：`PlayerInputRuntimeState`
- 细分职责：玩家输入运行时屏幕、按键和输入接管状态。
- 边界角色：`adapter/state`；最小 seam：player input runtime port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：3；属性：1；合计：4。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（3）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2606 | field | Terraria.GameInput.PlayerInput | Terraria.GameInput/PlayerInput.cs | D:\TRbackup\Version4\Terraria.GameInput\PlayerInput.cs | 22 | 2 | LockGamepadTileUseButton | bool | `public static bool LockGamepadTileUseButton = false;` | `public static bool LockGamepadTileUseButton = false;` |
| 2607 | field | Terraria.GameInput.PlayerInput | Terraria.GameInput/PlayerInput.cs | D:\TRbackup\Version4\Terraria.GameInput\PlayerInput.cs | 24 | 2 | _originalScreenWidth | int | `private static int _originalScreenWidth;` | `private static int _originalScreenWidth;` |
| 2608 | field | Terraria.GameInput.PlayerInput | Terraria.GameInput/PlayerInput.cs | D:\TRbackup\Version4\Terraria.GameInput\PlayerInput.cs | 26 | 2 | _originalScreenHeight | int | `private static int _originalScreenHeight;` | `private static int _originalScreenHeight;` |

#### 属性（1）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3871 | property | Terraria.GameInput.PlayerInput | Terraria.GameInput/PlayerInput.cs | D:\TRbackup\Version4\Terraria.GameInput\PlayerInput.cs | 29 | 2 | OriginalScreenSize | Vector2 | `public static Vector2 OriginalScreenSize => new Vector2(_originalScreenWidth, _originalScreenHeight);` | `public static Vector2 OriginalScreenSize => new Vector2(_originalScreenWidth, _originalScreenHeight);` |


### 4.21 细分子系统：`EquipmentLoadoutState`

- 原报告章节：`4.9.112`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`08` / `玩家输入与玩法`

- 上一级基线细分子系统：`EquipmentLoadoutState`
- 细分职责：装备栏和染料栏位状态。
- 边界角色：`state`；最小 seam：equipment loadout port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：3；属性：0；合计：3。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（3）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3074 | field | Terraria.EquipmentLoadout | Terraria/EquipmentLoadout.cs | D:\TRbackup\Version4\Terraria\EquipmentLoadout.cs | 8 | 2 | Armor | Terraria.Item[] | `public Item[] Armor;` | `public Item[] Armor;` |
| 3075 | field | Terraria.EquipmentLoadout | Terraria/EquipmentLoadout.cs | D:\TRbackup\Version4\Terraria\EquipmentLoadout.cs | 10 | 2 | Dye | Terraria.Item[] | `public Item[] Dye;` | `public Item[] Dye;` |
| 3076 | field | Terraria.EquipmentLoadout | Terraria/EquipmentLoadout.cs | D:\TRbackup\Version4\Terraria\EquipmentLoadout.cs | 12 | 2 | Hide | bool[] | `public bool[] Hide;` | `public bool[] Hide;` |

#### 属性（0）

无该类型成员记录。


### 4.22 细分子系统：`SharedCreativePowerContracts`

- 原报告章节：`4.9.122`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`08` / `玩家输入与玩法`

- 上一级基线细分子系统：`SharedCreativePowerDefinitions`
- 细分职责：创意能力公开接口、权限和服务器配置契约。
- 边界角色：`definition/adapter`；最小 seam：creative power contract port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：0；属性：4；合计：4。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（0）

无该类型成员记录。

#### 属性（4）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3772 | property | Terraria.GameContent.Creative.ICreativePower | Terraria.GameContent.Creative/ICreativePower.cs | D:\TRbackup\Version4\Terraria.GameContent.Creative\ICreativePower.cs | 9 | 2 | PowerId | ushort | `ushort PowerId { get; set; }` | `ushort PowerId { get; set; }` |
| 3773 | property | Terraria.GameContent.Creative.ICreativePower | Terraria.GameContent.Creative/ICreativePower.cs | D:\TRbackup\Version4\Terraria.GameContent.Creative\ICreativePower.cs | 11 | 2 | ServerConfigName | string | `string ServerConfigName { get; set; }` | `string ServerConfigName { get; set; }` |
| 3774 | property | Terraria.GameContent.Creative.ICreativePower | Terraria.GameContent.Creative/ICreativePower.cs | D:\TRbackup\Version4\Terraria.GameContent.Creative\ICreativePower.cs | 13 | 2 | CurrentPermissionLevel | Terraria.GameContent.Creative.PowerPermissionLevel | `PowerPermissionLevel CurrentPermissionLevel { get; set; }` | `PowerPermissionLevel CurrentPermissionLevel { get; set; }` |
| 3775 | property | Terraria.GameContent.Creative.ICreativePower | Terraria.GameContent.Creative/ICreativePower.cs | D:\TRbackup\Version4\Terraria.GameContent.Creative\ICreativePower.cs | 15 | 2 | DefaultPermissionLevel | Terraria.GameContent.Creative.PowerPermissionLevel | `PowerPermissionLevel DefaultPermissionLevel { get; set; }` | `PowerPermissionLevel DefaultPermissionLevel { get; set; }` |


### 4.23 细分子系统：`PlayerMovementCapabilityState`

- 原报告章节：`4.9.142`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`08` / `玩家输入与玩法`

- 上一级基线细分子系统：`PlayerIntentAndMovementState`
- 细分职责：飞行、翅膀、跳跃和便携座椅运动能力状态。
- 边界角色：`state/query`；最小 seam：player movement capability port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：2；声明类型数：2；字段：18；属性：0；合计：18。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（18）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1250 | field | Terraria.DataStructures.PlayerMovementAccsCache | Terraria.DataStructures/PlayerMovementAccsCache.cs | D:\TRbackup\Version4\Terraria.DataStructures\PlayerMovementAccsCache.cs | 5 | 2 | _readyToPaste | bool | `private bool _readyToPaste;` | `private bool _readyToPaste;` |
| 1251 | field | Terraria.DataStructures.PlayerMovementAccsCache | Terraria.DataStructures/PlayerMovementAccsCache.cs | D:\TRbackup\Version4\Terraria.DataStructures\PlayerMovementAccsCache.cs | 7 | 2 | _mountPreventedFlight | bool | `private bool _mountPreventedFlight;` | `private bool _mountPreventedFlight;` |
| 1252 | field | Terraria.DataStructures.PlayerMovementAccsCache | Terraria.DataStructures/PlayerMovementAccsCache.cs | D:\TRbackup\Version4\Terraria.DataStructures\PlayerMovementAccsCache.cs | 9 | 2 | _mountPreventedExtraJumps | bool | `private bool _mountPreventedExtraJumps;` | `private bool _mountPreventedExtraJumps;` |
| 1253 | field | Terraria.DataStructures.PlayerMovementAccsCache | Terraria.DataStructures/PlayerMovementAccsCache.cs | D:\TRbackup\Version4\Terraria.DataStructures\PlayerMovementAccsCache.cs | 11 | 2 | rocketTime | int | `private int rocketTime;` | `private int rocketTime;` |
| 1254 | field | Terraria.DataStructures.PlayerMovementAccsCache | Terraria.DataStructures/PlayerMovementAccsCache.cs | D:\TRbackup\Version4\Terraria.DataStructures\PlayerMovementAccsCache.cs | 13 | 2 | wingTime | float | `private float wingTime;` | `private float wingTime;` |
| 1255 | field | Terraria.DataStructures.PlayerMovementAccsCache | Terraria.DataStructures/PlayerMovementAccsCache.cs | D:\TRbackup\Version4\Terraria.DataStructures\PlayerMovementAccsCache.cs | 15 | 2 | rocketDelay | int | `private int rocketDelay;` | `private int rocketDelay;` |
| 1256 | field | Terraria.DataStructures.PlayerMovementAccsCache | Terraria.DataStructures/PlayerMovementAccsCache.cs | D:\TRbackup\Version4\Terraria.DataStructures\PlayerMovementAccsCache.cs | 17 | 2 | rocketDelay2 | int | `private int rocketDelay2;` | `private int rocketDelay2;` |
| 1257 | field | Terraria.DataStructures.PlayerMovementAccsCache | Terraria.DataStructures/PlayerMovementAccsCache.cs | D:\TRbackup\Version4\Terraria.DataStructures\PlayerMovementAccsCache.cs | 19 | 2 | jumpAgainCloud | bool | `private bool jumpAgainCloud;` | `private bool jumpAgainCloud;` |
| 1258 | field | Terraria.DataStructures.PlayerMovementAccsCache | Terraria.DataStructures/PlayerMovementAccsCache.cs | D:\TRbackup\Version4\Terraria.DataStructures\PlayerMovementAccsCache.cs | 21 | 2 | jumpAgainSandstorm | bool | `private bool jumpAgainSandstorm;` | `private bool jumpAgainSandstorm;` |
| 1259 | field | Terraria.DataStructures.PlayerMovementAccsCache | Terraria.DataStructures/PlayerMovementAccsCache.cs | D:\TRbackup\Version4\Terraria.DataStructures\PlayerMovementAccsCache.cs | 23 | 2 | jumpAgainBlizzard | bool | `private bool jumpAgainBlizzard;` | `private bool jumpAgainBlizzard;` |
| 1260 | field | Terraria.DataStructures.PlayerMovementAccsCache | Terraria.DataStructures/PlayerMovementAccsCache.cs | D:\TRbackup\Version4\Terraria.DataStructures\PlayerMovementAccsCache.cs | 25 | 2 | jumpAgainFart | bool | `private bool jumpAgainFart;` | `private bool jumpAgainFart;` |
| 1261 | field | Terraria.DataStructures.PlayerMovementAccsCache | Terraria.DataStructures/PlayerMovementAccsCache.cs | D:\TRbackup\Version4\Terraria.DataStructures\PlayerMovementAccsCache.cs | 27 | 2 | jumpAgainSail | bool | `private bool jumpAgainSail;` | `private bool jumpAgainSail;` |
| 1262 | field | Terraria.DataStructures.PlayerMovementAccsCache | Terraria.DataStructures/PlayerMovementAccsCache.cs | D:\TRbackup\Version4\Terraria.DataStructures\PlayerMovementAccsCache.cs | 29 | 2 | jumpAgainUnicorn | bool | `private bool jumpAgainUnicorn;` | `private bool jumpAgainUnicorn;` |
| 1267 | field | Terraria.DataStructures.PortableStoolUsage | Terraria.DataStructures/PortableStoolUsage.cs | D:\TRbackup\Version4\Terraria.DataStructures\PortableStoolUsage.cs | 5 | 2 | HasAStool | bool | `public bool HasAStool;` | `public bool HasAStool;` |
| 1268 | field | Terraria.DataStructures.PortableStoolUsage | Terraria.DataStructures/PortableStoolUsage.cs | D:\TRbackup\Version4\Terraria.DataStructures\PortableStoolUsage.cs | 7 | 2 | IsInUse | bool | `public bool IsInUse;` | `public bool IsInUse;` |
| 1269 | field | Terraria.DataStructures.PortableStoolUsage | Terraria.DataStructures/PortableStoolUsage.cs | D:\TRbackup\Version4\Terraria.DataStructures\PortableStoolUsage.cs | 9 | 2 | HeightBoost | int | `public int HeightBoost;` | `public int HeightBoost;` |
| 1270 | field | Terraria.DataStructures.PortableStoolUsage | Terraria.DataStructures/PortableStoolUsage.cs | D:\TRbackup\Version4\Terraria.DataStructures\PortableStoolUsage.cs | 11 | 2 | VisualYOffset | int | `public int VisualYOffset;` | `public int VisualYOffset;` |
| 1271 | field | Terraria.DataStructures.PortableStoolUsage | Terraria.DataStructures/PortableStoolUsage.cs | D:\TRbackup\Version4\Terraria.DataStructures\PortableStoolUsage.cs | 13 | 2 | MapYOffset | int | `public int MapYOffset;` | `public int MapYOffset;` |

#### 属性（0）

无该类型成员记录。


### 4.24 细分子系统：`PlayerIntentAndInteractionState`

- 原报告章节：`4.9.143`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`08` / `玩家输入与玩法`

- 上一级基线细分子系统：`PlayerIntentAndMovementState`
- 细分职责：玩家意图推断和交互锚点状态。
- 边界角色：`state/query`；最小 seam：player intent interaction port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：2；声明类型数：2；字段：14；属性：1；合计：15。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（14）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1236 | field | Terraria.DataStructures.PlayerIntentionGuesser | Terraria.DataStructures/PlayerIntentionGuesser.cs | D:\TRbackup\Version4\Terraria.DataStructures\PlayerIntentionGuesser.cs | 8 | 2 | LastX | int | `public int LastX;` | `public int LastX;` |
| 1237 | field | Terraria.DataStructures.PlayerIntentionGuesser | Terraria.DataStructures/PlayerIntentionGuesser.cs | D:\TRbackup\Version4\Terraria.DataStructures\PlayerIntentionGuesser.cs | 10 | 2 | LastY | int | `public int LastY;` | `public int LastY;` |
| 1238 | field | Terraria.DataStructures.PlayerIntentionGuesser | Terraria.DataStructures/PlayerIntentionGuesser.cs | D:\TRbackup\Version4\Terraria.DataStructures\PlayerIntentionGuesser.cs | 12 | 2 | LastPosition | Vector2 | `public Vector2 LastPosition;` | `public Vector2 LastPosition;` |
| 1239 | field | Terraria.DataStructures.PlayerIntentionGuesser | Terraria.DataStructures/PlayerIntentionGuesser.cs | D:\TRbackup\Version4\Terraria.DataStructures\PlayerIntentionGuesser.cs | 14 | 2 | LastCenter | Vector2 | `public Vector2 LastCenter;` | `public Vector2 LastCenter;` |
| 1240 | field | Terraria.DataStructures.PlayerIntentionGuesser | Terraria.DataStructures/PlayerIntentionGuesser.cs | D:\TRbackup\Version4\Terraria.DataStructures\PlayerIntentionGuesser.cs | 16 | 2 | LastMouse | Vector2 | `public Vector2 LastMouse;` | `public Vector2 LastMouse;` |
| 1241 | field | Terraria.DataStructures.PlayerIntentionGuesser | Terraria.DataStructures/PlayerIntentionGuesser.cs | D:\TRbackup\Version4\Terraria.DataStructures\PlayerIntentionGuesser.cs | 18 | 2 | LastDirection | int | `public int LastDirection;` | `public int LastDirection;` |
| 1242 | field | Terraria.DataStructures.PlayerIntentionGuesser | Terraria.DataStructures/PlayerIntentionGuesser.cs | D:\TRbackup\Version4\Terraria.DataStructures\PlayerIntentionGuesser.cs | 20 | 2 | LastWidth | int | `public int LastWidth;` | `public int LastWidth;` |
| 1243 | field | Terraria.DataStructures.PlayerIntentionGuesser | Terraria.DataStructures/PlayerIntentionGuesser.cs | D:\TRbackup\Version4\Terraria.DataStructures\PlayerIntentionGuesser.cs | 22 | 2 | Intention | Terraria.DataStructures.GuessedPlayerIntention | `public GuessedPlayerIntention Intention;` | `public GuessedPlayerIntention Intention;` |
| 1244 | field | Terraria.DataStructures.PlayerIntentionGuesser | Terraria.DataStructures/PlayerIntentionGuesser.cs | D:\TRbackup\Version4\Terraria.DataStructures\PlayerIntentionGuesser.cs | 24 | 2 | UsageProxy | Terraria.GameContent.SmartCursorHelper.SmartCursorUsageInfo | `public SmartCursorHelper.SmartCursorUsageInfo UsageProxy = new SmartCursorHelper.SmartCursorUsageInfo();` | `public SmartCursorHelper.SmartCursorUsageInfo UsageProxy = new SmartCursorHelper.SmartCursorUsageInfo();` |
| 1245 | field | Terraria.DataStructures.PlayerIntentionGuesser | Terraria.DataStructures/PlayerIntentionGuesser.cs | D:\TRbackup\Version4\Terraria.DataStructures\PlayerIntentionGuesser.cs | 26 | 2 | TimeWithIntention | int | `public int TimeWithIntention;` | `public int TimeWithIntention;` |
| 1246 | field | Terraria.DataStructures.PlayerIntentionGuesser | Terraria.DataStructures/PlayerIntentionGuesser.cs | D:\TRbackup\Version4\Terraria.DataStructures\PlayerIntentionGuesser.cs | 28 | 2 | PlayerActiveActionTimeLeft | int | `public int PlayerActiveActionTimeLeft;` | `public int PlayerActiveActionTimeLeft;` |
| 1247 | field | Terraria.DataStructures.PlayerInteractionAnchor | Terraria.DataStructures/PlayerInteractionAnchor.cs | D:\TRbackup\Version4\Terraria.DataStructures\PlayerInteractionAnchor.cs | 5 | 2 | interactEntityID | int | `public int interactEntityID;` | `public int interactEntityID;` |
| 1248 | field | Terraria.DataStructures.PlayerInteractionAnchor | Terraria.DataStructures/PlayerInteractionAnchor.cs | D:\TRbackup\Version4\Terraria.DataStructures\PlayerInteractionAnchor.cs | 7 | 2 | X | int | `public int X;` | `public int X;` |
| 1249 | field | Terraria.DataStructures.PlayerInteractionAnchor | Terraria.DataStructures/PlayerInteractionAnchor.cs | D:\TRbackup\Version4\Terraria.DataStructures\PlayerInteractionAnchor.cs | 9 | 2 | Y | int | `public int Y;` | `public int Y;` |

#### 属性（1）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3699 | property | Terraria.DataStructures.PlayerInteractionAnchor | Terraria.DataStructures/PlayerInteractionAnchor.cs | D:\TRbackup\Version4\Terraria.DataStructures\PlayerInteractionAnchor.cs | 11 | 2 | InUse | bool | `public bool InUse => interactEntityID != -1;` | `public bool InUse => interactEntityID != -1;` |


### 4.25 细分子系统：`SharedCreativePerPlayerPowerState`

- 原报告章节：`4.9.172`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`08` / `玩家输入与玩法`

- 上一级基线细分子系统：`SharedCreativePowerDefinitions`
- 上一级 peer 细分子系统：`SharedCreativePowerDefinitionState`
- 细分职责：按玩家创意能力的参数、滑杆和目标值状态。
- 边界角色：`definition/state`；最小 seam：creative per-player power port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：3；字段：11；属性：9；合计：20。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（11）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1471 | field | Terraria.GameContent.Creative.CreativePowers.APerPlayerTogglePower | Terraria.GameContent.Creative/CreativePowers.cs | D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs | 22 | 3 | _powerNameKey | string | `internal string _powerNameKey;` | `internal string _powerNameKey;` |
| 1472 | field | Terraria.GameContent.Creative.CreativePowers.APerPlayerTogglePower | Terraria.GameContent.Creative/CreativePowers.cs | D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs | 24 | 3 | _iconLocation | Point | `internal Point _iconLocation;` | `internal Point _iconLocation;` |
| 1473 | field | Terraria.GameContent.Creative.CreativePowers.APerPlayerTogglePower | Terraria.GameContent.Creative/CreativePowers.cs | D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs | 26 | 3 | _defaultToggleState | bool | `internal bool _defaultToggleState;` | `internal bool _defaultToggleState;` |
| 1474 | field | Terraria.GameContent.Creative.CreativePowers.APerPlayerTogglePower | Terraria.GameContent.Creative/CreativePowers.cs | D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs | 28 | 3 | _perPlayerIsEnabled | bool[] | `private bool[] _perPlayerIsEnabled = new bool[255];` | `private bool[] _perPlayerIsEnabled = new bool[255];` |
| 1475 | field | Terraria.GameContent.Creative.CreativePowers.APerPlayerSliderPower | Terraria.GameContent.Creative/CreativePowers.cs | D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs | 90 | 3 | _iconLocation | Point | `internal Point _iconLocation;` | `internal Point _iconLocation;` |
| 1476 | field | Terraria.GameContent.Creative.CreativePowers.APerPlayerSliderPower | Terraria.GameContent.Creative/CreativePowers.cs | D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs | 92 | 3 | _sliderCurrentValueCache | float | `internal float _sliderCurrentValueCache;` | `internal float _sliderCurrentValueCache;` |
| 1477 | field | Terraria.GameContent.Creative.CreativePowers.APerPlayerSliderPower | Terraria.GameContent.Creative/CreativePowers.cs | D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs | 94 | 3 | _powerNameKey | string | `internal string _powerNameKey;` | `internal string _powerNameKey;` |
| 1478 | field | Terraria.GameContent.Creative.CreativePowers.APerPlayerSliderPower | Terraria.GameContent.Creative/CreativePowers.cs | D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs | 96 | 3 | _cachePerPlayer | float[] | `internal float[] _cachePerPlayer = new float[256];` | `internal float[] _cachePerPlayer = new float[256];` |
| 1479 | field | Terraria.GameContent.Creative.CreativePowers.APerPlayerSliderPower | Terraria.GameContent.Creative/CreativePowers.cs | D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs | 98 | 3 | _sliderDefaultValue | float | `internal float _sliderDefaultValue;` | `internal float _sliderDefaultValue;` |
| 1480 | field | Terraria.GameContent.Creative.CreativePowers.APerPlayerSliderPower | Terraria.GameContent.Creative/CreativePowers.cs | D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs | 100 | 3 | _currentTargetValue | float | `private float _currentTargetValue;` | `private float _currentTargetValue;` |
| 1481 | field | Terraria.GameContent.Creative.CreativePowers.APerPlayerSliderPower | Terraria.GameContent.Creative/CreativePowers.cs | D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs | 102 | 3 | _nextTimeWeCanPush | System.DateTime | `private DateTime _nextTimeWeCanPush = DateTime.UtcNow;` | `private DateTime _nextTimeWeCanPush = DateTime.UtcNow;` |

#### 属性（9）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3748 | property | Terraria.GameContent.Creative.CreativePowers.APerPlayerTogglePower | Terraria.GameContent.Creative/CreativePowers.cs | D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs | 30 | 3 | PowerId | ushort | `public ushort PowerId { get; set; }` | `public ushort PowerId { get; set; }` |
| 3749 | property | Terraria.GameContent.Creative.CreativePowers.APerPlayerTogglePower | Terraria.GameContent.Creative/CreativePowers.cs | D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs | 32 | 3 | ServerConfigName | string | `public string ServerConfigName { get; set; }` | `public string ServerConfigName { get; set; }` |
| 3750 | property | Terraria.GameContent.Creative.CreativePowers.APerPlayerTogglePower | Terraria.GameContent.Creative/CreativePowers.cs | D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs | 34 | 3 | CurrentPermissionLevel | Terraria.GameContent.Creative.PowerPermissionLevel | `public PowerPermissionLevel CurrentPermissionLevel { get; set; }` | `public PowerPermissionLevel CurrentPermissionLevel { get; set; }` |
| 3751 | property | Terraria.GameContent.Creative.CreativePowers.APerPlayerTogglePower | Terraria.GameContent.Creative/CreativePowers.cs | D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs | 36 | 3 | DefaultPermissionLevel | Terraria.GameContent.Creative.PowerPermissionLevel | `public PowerPermissionLevel DefaultPermissionLevel { get; set; }` | `public PowerPermissionLevel DefaultPermissionLevel { get; set; }` |
| 3752 | property | Terraria.GameContent.Creative.CreativePowers.APerPlayerSliderPower | Terraria.GameContent.Creative/CreativePowers.cs | D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs | 104 | 3 | PowerId | ushort | `public ushort PowerId { get; set; }` | `public ushort PowerId { get; set; }` |
| 3753 | property | Terraria.GameContent.Creative.CreativePowers.APerPlayerSliderPower | Terraria.GameContent.Creative/CreativePowers.cs | D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs | 106 | 3 | ServerConfigName | string | `public string ServerConfigName { get; set; }` | `public string ServerConfigName { get; set; }` |
| 3754 | property | Terraria.GameContent.Creative.CreativePowers.APerPlayerSliderPower | Terraria.GameContent.Creative/CreativePowers.cs | D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs | 108 | 3 | CurrentPermissionLevel | Terraria.GameContent.Creative.PowerPermissionLevel | `public PowerPermissionLevel CurrentPermissionLevel { get; set; }` | `public PowerPermissionLevel CurrentPermissionLevel { get; set; }` |
| 3755 | property | Terraria.GameContent.Creative.CreativePowers.APerPlayerSliderPower | Terraria.GameContent.Creative/CreativePowers.cs | D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs | 110 | 3 | DefaultPermissionLevel | Terraria.GameContent.Creative.PowerPermissionLevel | `public PowerPermissionLevel DefaultPermissionLevel { get; set; }` | `public PowerPermissionLevel DefaultPermissionLevel { get; set; }` |
| 3771 | property | Terraria.GameContent.Creative.CreativePowers.SpawnRateSliderPerPlayerPower | Terraria.GameContent.Creative/CreativePowers.cs | D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs | 700 | 3 | StrengthMultiplierToGiveNPCs | float | `public float StrengthMultiplierToGiveNPCs { get; private set; }` | `public float StrengthMultiplierToGiveNPCs { get; private set; }` |


### 4.26 细分子系统：`SharedCreativeSharedPowerState`

- 原报告章节：`4.9.173`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`08` / `玩家输入与玩法`

- 上一级基线细分子系统：`SharedCreativePowerDefinitions`
- 上一级 peer 细分子系统：`SharedCreativePowerDefinitionState`
- 细分职责：共享创意能力的权限、开关和全局目标值状态。
- 边界角色：`definition/state`；最小 seam：creative shared power port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：5；字段：9；属性：15；合计：24。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（9）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1482 | field | Terraria.GameContent.Creative.CreativePowers.ASharedButtonPower | Terraria.GameContent.Creative/CreativePowers.cs | D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs | 188 | 3 | _iconLocation | Point | `internal Point _iconLocation;` | `internal Point _iconLocation;` |
| 1483 | field | Terraria.GameContent.Creative.CreativePowers.ASharedButtonPower | Terraria.GameContent.Creative/CreativePowers.cs | D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs | 190 | 3 | _powerNameKey | string | `internal string _powerNameKey;` | `internal string _powerNameKey;` |
| 1484 | field | Terraria.GameContent.Creative.CreativePowers.ASharedButtonPower | Terraria.GameContent.Creative/CreativePowers.cs | D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs | 192 | 3 | _descriptionKey | string | `internal string _descriptionKey;` | `internal string _descriptionKey;` |
| 1485 | field | Terraria.GameContent.Creative.CreativePowers.ASharedSliderPower | Terraria.GameContent.Creative/CreativePowers.cs | D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs | 261 | 3 | _iconLocation | Point | `internal Point _iconLocation;` | `internal Point _iconLocation;` |
| 1486 | field | Terraria.GameContent.Creative.CreativePowers.ASharedSliderPower | Terraria.GameContent.Creative/CreativePowers.cs | D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs | 263 | 3 | _sliderCurrentValueCache | float | `internal float _sliderCurrentValueCache;` | `internal float _sliderCurrentValueCache;` |
| 1487 | field | Terraria.GameContent.Creative.CreativePowers.ASharedSliderPower | Terraria.GameContent.Creative/CreativePowers.cs | D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs | 265 | 3 | _powerNameKey | string | `internal string _powerNameKey;` | `internal string _powerNameKey;` |
| 1488 | field | Terraria.GameContent.Creative.CreativePowers.ASharedSliderPower | Terraria.GameContent.Creative/CreativePowers.cs | D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs | 267 | 3 | _syncToJoiningPlayers | bool | `internal bool _syncToJoiningPlayers = true;` | `internal bool _syncToJoiningPlayers = true;` |
| 1489 | field | Terraria.GameContent.Creative.CreativePowers.ASharedSliderPower | Terraria.GameContent.Creative/CreativePowers.cs | D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs | 269 | 3 | _currentTargetValue | float | `internal float _currentTargetValue;` | `internal float _currentTargetValue;` |
| 1490 | field | Terraria.GameContent.Creative.CreativePowers.ASharedSliderPower | Terraria.GameContent.Creative/CreativePowers.cs | D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs | 271 | 3 | _nextTimeWeCanPush | System.DateTime | `private DateTime _nextTimeWeCanPush = DateTime.UtcNow;` | `private DateTime _nextTimeWeCanPush = DateTime.UtcNow;` |

#### 属性（15）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3756 | property | Terraria.GameContent.Creative.CreativePowers.ASharedButtonPower | Terraria.GameContent.Creative/CreativePowers.cs | D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs | 194 | 3 | PowerId | ushort | `public ushort PowerId { get; set; }` | `public ushort PowerId { get; set; }` |
| 3757 | property | Terraria.GameContent.Creative.CreativePowers.ASharedButtonPower | Terraria.GameContent.Creative/CreativePowers.cs | D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs | 196 | 3 | ServerConfigName | string | `public string ServerConfigName { get; set; }` | `public string ServerConfigName { get; set; }` |
| 3758 | property | Terraria.GameContent.Creative.CreativePowers.ASharedButtonPower | Terraria.GameContent.Creative/CreativePowers.cs | D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs | 198 | 3 | CurrentPermissionLevel | Terraria.GameContent.Creative.PowerPermissionLevel | `public PowerPermissionLevel CurrentPermissionLevel { get; set; }` | `public PowerPermissionLevel CurrentPermissionLevel { get; set; }` |
| 3759 | property | Terraria.GameContent.Creative.CreativePowers.ASharedButtonPower | Terraria.GameContent.Creative/CreativePowers.cs | D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs | 200 | 3 | DefaultPermissionLevel | Terraria.GameContent.Creative.PowerPermissionLevel | `public PowerPermissionLevel DefaultPermissionLevel { get; set; }` | `public PowerPermissionLevel DefaultPermissionLevel { get; set; }` |
| 3760 | property | Terraria.GameContent.Creative.CreativePowers.ASharedTogglePower | Terraria.GameContent.Creative/CreativePowers.cs | D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs | 217 | 3 | PowerId | ushort | `public ushort PowerId { get; set; }` | `public ushort PowerId { get; set; }` |
| 3761 | property | Terraria.GameContent.Creative.CreativePowers.ASharedTogglePower | Terraria.GameContent.Creative/CreativePowers.cs | D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs | 219 | 3 | ServerConfigName | string | `public string ServerConfigName { get; set; }` | `public string ServerConfigName { get; set; }` |
| 3762 | property | Terraria.GameContent.Creative.CreativePowers.ASharedTogglePower | Terraria.GameContent.Creative/CreativePowers.cs | D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs | 221 | 3 | CurrentPermissionLevel | Terraria.GameContent.Creative.PowerPermissionLevel | `public PowerPermissionLevel CurrentPermissionLevel { get; set; }` | `public PowerPermissionLevel CurrentPermissionLevel { get; set; }` |
| 3763 | property | Terraria.GameContent.Creative.CreativePowers.ASharedTogglePower | Terraria.GameContent.Creative/CreativePowers.cs | D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs | 223 | 3 | DefaultPermissionLevel | Terraria.GameContent.Creative.PowerPermissionLevel | `public PowerPermissionLevel DefaultPermissionLevel { get; set; }` | `public PowerPermissionLevel DefaultPermissionLevel { get; set; }` |
| 3764 | property | Terraria.GameContent.Creative.CreativePowers.ASharedTogglePower | Terraria.GameContent.Creative/CreativePowers.cs | D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs | 225 | 3 | Enabled | bool | `public bool Enabled { get; private set; }` | `public bool Enabled { get; private set; }` |
| 3765 | property | Terraria.GameContent.Creative.CreativePowers.ASharedSliderPower | Terraria.GameContent.Creative/CreativePowers.cs | D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs | 273 | 3 | PowerId | ushort | `public ushort PowerId { get; set; }` | `public ushort PowerId { get; set; }` |
| 3766 | property | Terraria.GameContent.Creative.CreativePowers.ASharedSliderPower | Terraria.GameContent.Creative/CreativePowers.cs | D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs | 275 | 3 | ServerConfigName | string | `public string ServerConfigName { get; set; }` | `public string ServerConfigName { get; set; }` |
| 3767 | property | Terraria.GameContent.Creative.CreativePowers.ASharedSliderPower | Terraria.GameContent.Creative/CreativePowers.cs | D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs | 277 | 3 | CurrentPermissionLevel | Terraria.GameContent.Creative.PowerPermissionLevel | `public PowerPermissionLevel CurrentPermissionLevel { get; set; }` | `public PowerPermissionLevel CurrentPermissionLevel { get; set; }` |
| 3768 | property | Terraria.GameContent.Creative.CreativePowers.ASharedSliderPower | Terraria.GameContent.Creative/CreativePowers.cs | D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs | 279 | 3 | DefaultPermissionLevel | Terraria.GameContent.Creative.PowerPermissionLevel | `public PowerPermissionLevel DefaultPermissionLevel { get; set; }` | `public PowerPermissionLevel DefaultPermissionLevel { get; set; }` |
| 3769 | property | Terraria.GameContent.Creative.CreativePowers.ModifyTimeRate | Terraria.GameContent.Creative/CreativePowers.cs | D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs | 408 | 3 | TargetTimeRate | int | `public int TargetTimeRate { get; private set; }` | `public int TargetTimeRate { get; private set; }` |
| 3770 | property | Terraria.GameContent.Creative.CreativePowers.DifficultySliderPower | Terraria.GameContent.Creative/CreativePowers.cs | D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs | 463 | 3 | StrengthMultiplierToGiveNPCs | float | `public float StrengthMultiplierToGiveNPCs { get; private set; }` | `public float StrengthMultiplierToGiveNPCs { get; private set; }` |


## 5. 追溯与验收

> 来源报告 SHA-256：`b944441d92d5103f218b766a3e354200794528a19e65a862e44c2b40a36f1196`
- 本分区细分组数：26；成员数：283；字段：232；属性：51。
- 预期不变量：本文件只出现完整细分子系统；不跨分区复制成员；所有成员的来源序号、原始声明、路径、行列和 C# 类型必须与来源报告一致。
- 验证方式：重新读取 20 个文件，检查文件数、细分组唯一性、来源序号恰好覆盖 `1..4542`、字段/属性合计和逐成员行文本一致；本次分区生成不运行 `dotnet`，因为没有修改 C# 或项目文件。

生成器：`Build/Tools/Generate-Version4NonAuthoritativeComponentPartitionReports.ps1`。
