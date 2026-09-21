# Version4 非权威组件拆分分区 18/20：地图、相机与绘制

> 来源报告：`docs/migration/ledgers/Version4非权威模拟系统字段属性逐成员源码声明-更细子系统拆分-去除ID类文件.md`
> 来源报告 SHA-256：`b944441d92d5103f218b766a3e354200794528a19e65a862e44c2b40a36f1196`
> 本文件是并行组件拆分工作包，不是新的权威成员清单；成员声明事实直接保留来源报告原文。

## 1. 分区定位与评估

- 分区范围：地图编码/存储、地图层、相机矩阵、顶点、绘制批次、光照、shader 和场景聚合。
- 本分区组件化重点：将渲染缓存/投影与世界权威地块状态分离，声明缓存失效条件和帧顺序。
- 本分区包含 29 个完整细分子系统、331 条成员记录（字段 274、属性 57）。来源序号覆盖区间 `10..4535`，区间可能与其他分区交错，不以序号定义领域边界。
- 组件边界判定：细分子系统只是并行处理的最小库存边界，不等价于一个组件。必须继续按共同读者/写者、变更原因、生命周期、权威性和副作用分为 Component、System、Query、Command、Adapter 或 Projection。
- 证据状态：成员声明和统计已由来源报告校验；逐成员读者、写者、初始化/清理路径、持久化/网络语义和系统顺序仍需本分区会话补证，不得从名称或数量推断。

## 2. 分区统计

| 父级子系统 | 细分子系统数 | 字段 | 属性 | 合计 |
|---|---:|---:|---:|---:|
| `ClientPresentationAndTools` | 9 | 78 | 13 | 91 |
| `RuntimeComposition` | 3 | 20 | 0 | 20 |
| `SharedRuntimeMechanisms` | 17 | 176 | 44 | 220 |

| 原报告章节 | 父级 | 细分子系统 | 边界角色 | 字段 | 属性 | 合计 | 组件化处理 |
|---|---|---|---|---:|---:|---:|---|
| `4.1.4` | `RuntimeComposition` | `MainCameraAndUiScaleState` | presentation state | 8 | 0 | 8 | 待按成员访问模式拆分 |
| `4.1.7` | `RuntimeComposition` | `MainCameraAndVisualOffsets` | presentation state | 8 | 0 | 8 | 待按成员访问模式拆分 |
| `4.1.20` | `RuntimeComposition` | `MainSceneMetricsState` | derived/query | 4 | 0 | 4 | 待按成员访问模式拆分 |
| `4.9.7` | `SharedRuntimeMechanisms` | `SharedSceneScanSettings` | query input | 4 | 0 | 4 | 待按成员访问模式拆分 |
| `4.9.8` | `SharedRuntimeMechanisms` | `SharedSceneVisualState` | presentation state | 6 | 0 | 6 | 待按成员访问模式拆分 |
| `4.9.9` | `SharedRuntimeMechanisms` | `SharedCaptureAndCameraSupport` | presentation | 5 | 1 | 6 | 待按成员访问模式拆分 |
| `4.9.76` | `SharedRuntimeMechanisms` | `LightMapCacheState` | cache | 5 | 8 | 13 | 待按成员访问模式拆分 |
| `4.9.77` | `SharedRuntimeMechanisms` | `TileLightScannerState` | query | 1 | 0 | 1 | 待按成员访问模式拆分 |
| `4.9.80` | `SharedRuntimeMechanisms` | `DrawAnimationAndFrameState` | presentation/state | 18 | 2 | 20 | 待按成员访问模式拆分 |
| `4.9.81` | `SharedRuntimeMechanisms` | `DrawCommandAndBatchState` | presentation/command | 20 | 0 | 20 | 待按成员访问模式拆分 |
| `4.9.82` | `SharedRuntimeMechanisms` | `WorldDrawingAuxiliaryState` | presentation | 10 | 0 | 10 | 待按成员访问模式拆分 |
| `4.9.108` | `SharedRuntimeMechanisms` | `EntityShadowPresentationState` | presentation/snapshot | 6 | 1 | 7 | 待按成员访问模式拆分 |
| `4.9.120` | `SharedRuntimeMechanisms` | `SharedShaderBaseParameterState` | presentation | 18 | 3 | 21 | 待按成员访问模式拆分 |
| `4.9.130` | `SharedRuntimeMechanisms` | `SharedLightingCoordinatorState` | state/query | 16 | 3 | 19 | 待按成员访问模式拆分 |
| `4.9.131` | `SharedRuntimeMechanisms` | `SharedLegacyLightingState` | state/query | 19 | 1 | 20 | 待按成员访问模式拆分 |
| `4.9.171` | `SharedRuntimeMechanisms` | `SharedSceneScanAccumulatorState` | query/cache | 8 | 5 | 13 | 待按成员访问模式拆分 |
| `4.9.175` | `SharedRuntimeMechanisms` | `SharedSceneTileAggregateState` | query/cache | 0 | 19 | 19 | 待按成员访问模式拆分 |
| `4.9.182` | `SharedRuntimeMechanisms` | `SharedShaderFamilyDataState` | presentation | 19 | 1 | 20 | 待按成员访问模式拆分 |
| `4.9.183` | `SharedRuntimeMechanisms` | `SharedShaderRegistryAndLookupState` | presentation/query | 9 | 0 | 9 | 待按成员访问模式拆分 |
| `4.9.203` | `SharedRuntimeMechanisms` | `SharedSceneZoneGeometryAndThresholdState` | query/input | 12 | 0 | 12 | 待按成员访问模式拆分 |
| `4.11.12` | `ClientPresentationAndTools` | `MapOverlayAndLayerPresentation` | presentation | 12 | 0 | 12 | 待按成员访问模式拆分 |
| `4.11.13` | `ClientPresentationAndTools` | `WorldMapState` | presentation/state | 4 | 1 | 5 | 待按成员访问模式拆分 |
| `4.11.16` | `ClientPresentationAndTools` | `MapTileUpdateQueueState` | presentation/state | 3 | 0 | 3 | 待按成员访问模式拆分 |
| `4.11.19` | `ClientPresentationAndTools` | `CameraMatrixAndViewportState` | presentation | 8 | 8 | 16 | 待按成员访问模式拆分 |
| `4.11.20` | `ClientPresentationAndTools` | `VertexStripAndColorState` | presentation | 12 | 1 | 13 | 待按成员访问模式拆分 |
| `4.11.24` | `ClientPresentationAndTools` | `MapTileCellState` | snapshot state | 3 | 3 | 6 | 待按成员访问模式拆分 |
| `4.11.28` | `ClientPresentationAndTools` | `MapIoRuntimeState` | adapter state | 4 | 0 | 4 | 待按成员访问模式拆分 |
| `4.11.33` | `ClientPresentationAndTools` | `MapEncodingHeaderBitCatalogState` | adapter state | 24 | 0 | 24 | 待按成员访问模式拆分 |
| `4.11.34` | `ClientPresentationAndTools` | `MapEncodingOptionLimitState` | adapter state | 8 | 0 | 8 | 待按成员访问模式拆分 |

## 3. 并行组件拆分契约

对本文件中的每个细分子系统和每条成员记录执行以下判断，并把结果写入对应的实现阶段设计/审计记录：

- 盘点声明类型、可见性、读者、写者、创建/更新/清理/持久化生命周期、访问频率和副作用；缺证据标记 `missing`，不可用命名猜测补齐。
- 标记权威状态、派生值、缓存、快照、兼容字段和表现数据；Component 只保存一个内聚概念的权威数据。
- 需要行为转换时放入显式 System/Command；纯资格判断和派生计算放入 Query；外部协议、文件、平台和第三方类型放入 Adapter；网络/持久化/日志/UI 输出放入单向 Projection。
- 分别建模实体引用、持久化 ID、网络 ID 和外部 ID；不得把 ID 类文件或外部对象直接塞入共享组件。
- 记录 Interface、Implementation、Seam、Depth、Leverage、Locality，以及必要的系统先后约束、失败归属和重试策略。

## 4. 逐成员源码声明

以下细分子系统块保持来源报告的成员表、声明、路径、行列和来源序号；只调整 Markdown 标题层级并增加分区追踪元数据。

### 4.1 细分子系统：`MainCameraAndUiScaleState`

- 原报告章节：`4.1.4`
- 父级子系统：`RuntimeComposition`
- 分区工作包：`18` / `地图、相机与绘制`

- 上一级基线细分子系统：`MainCameraAndUiScaleState`
- 细分职责：相机对象、视图矩阵和 UI 缩放状态。
- 边界角色：`presentation state`；最小 seam：camera/UI transform view；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：8；属性：0；合计：8。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（8）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 10 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 124 | 2 | GameViewMatrix | Terraria.Graphics.SpriteViewMatrix | `public static SpriteViewMatrix GameViewMatrix;` | `public static SpriteViewMatrix GameViewMatrix;` |
| 11 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 126 | 2 | _uiScaleMatrix | Matrix | `private static Matrix _uiScaleMatrix;` | `private static Matrix _uiScaleMatrix;` |
| 12 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 128 | 2 | _uiScaleWanted | float | `private static float _uiScaleWanted = 1f;` | `private static float _uiScaleWanted = 1f;` |
| 13 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 130 | 2 | _uiScaleUsed | float | `private static float _uiScaleUsed = 1f;` | `private static float _uiScaleUsed = 1f;` |
| 14 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 132 | 2 | SettingPlayWhenUnfocused | bool | `public static bool SettingPlayWhenUnfocused = false;` | `public static bool SettingPlayWhenUnfocused = false;` |
| 15 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 134 | 2 | ReversedUpDownArmorSetBonuses | bool | `public static bool ReversedUpDownArmorSetBonuses;` | `public static bool ReversedUpDownArmorSetBonuses;` |
| 16 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 136 | 2 | instance | Terraria.Main | `public static Main instance;` | `public static Main instance;` |
| 17 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 138 | 5 | Camera | Terraria.Graphics.Camera | `public static Camera Camera = new Camera();` | `public static Camera Camera = new Camera();` |

#### 属性（0）

无该类型成员记录。


### 4.2 细分子系统：`MainCameraAndVisualOffsets`

- 原报告章节：`4.1.7`
- 父级子系统：`RuntimeComposition`
- 分区工作包：`18` / `地图、相机与绘制`

- 上一级基线细分子系统：`MainCameraAndVisualOffsets`
- 细分职责：相机插值、手持/头饰偏移和屏幕平移数据。
- 边界角色：`presentation state`；最小 seam：camera offset view；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：8；属性：0；合计：8。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（8）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 61 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 230 | 2 | cameraLerp | float | `private static float cameraLerp;` | `private static float cameraLerp;` |
| 62 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 232 | 2 | cameraLerpTimer | int | `private static int cameraLerpTimer;` | `private static int cameraLerpTimer;` |
| 63 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 234 | 2 | cameraLerpTimeToggle | int | `private static int cameraLerpTimeToggle;` | `private static int cameraLerpTimeToggle;` |
| 64 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 236 | 2 | OffsetsNPCOffhand | Vector2[] | `public static Vector2[] OffsetsNPCOffhand = new Vector2[5]  	{  		new Vector2(14f, 34f),  		new Vector2(14f, 32f),  		new Vector2(14f, 26f),  		new Vector2(14f, 22f),  		new Vector2(14f, 18f)  	};` | `public static Vector2[] OffsetsNPCOffhand = new Vector2[5] { new Vector2(14f, 34f), new Vector2(14f, 32f), new Vector2(14f, 26f), new Vector2(14f, 22f), new Vector2(14f, 18f) };` |
| 65 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 245 | 2 | OffsetsPlayerOnhand | Vector2[] | `public static Vector2[] OffsetsPlayerOnhand = new Vector2[20]  	{  		new Vector2(6f, 19f),  		new Vector2(5f, 10f),  		new Vector2(12f, 10f),  		new Vector2(13f, 17f),  		new Vector2(12f, 19f),  		new Vector2(5f, 10f),  		new Vector2(7f, 17f),  		new Vector2(6f, 16f),  		new Vector2(6f, 16f),  		new Vector2(6f, 16f),  		new Vector2(6f, 17f),  		new Vector2(7f, 17f),  		new Vector2(7f, 17f),  		new Vector2(7f, 17f),  		new Vector2(8f, 17f),  		new Vector2(9f, 16f),  		new Vector2(9f, 12f),  		new Vector2(8f, 17f),  		new Vector2(7f, 17f),  		new Vector2(7f, 17f)  	};` | `public static Vector2[] OffsetsPlayerOnhand = new Vector2[20] { new Vector2(6f, 19f), new Vector2(5f, 10f), new Vector2(12f, 10f), new Vector2(13f, 17f), new Vector2(12f, 19f), new Vector2(5f, 10f), new Vector2(7f, 17f), new Vector2(6f, 16f), new Vector2(6f, 16f), new Vector2(6f, 16f), new Vector2(6f, 17f), new Vector2(7f, 17f), new Vector2(7f, 17f), new Vector2(7f, 17f), new Vector2(8f, 17f), new Vector2(9f, 16f), new Vector2(9f, 12f), new Vector2(8f, 17f), new Vector2(7f, 17f), new Vector2(7f, 17f) };` |
| 66 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 269 | 2 | OffsetsPlayerHeadgear | Vector2[] | `public static Vector2[] OffsetsPlayerHeadgear = new Vector2[20]  	{  		new Vector2(0f, 2f),  		new Vector2(0f, 2f),  		new Vector2(0f, 2f),  		new Vector2(0f, 2f),  		new Vector2(0f, 2f),  		new Vector2(0f, 2f),  		new Vector2(0f, 2f),  		new Vector2(0f, 0f),  		new Vector2(0f, 0f),  		new Vector2(0f, 0f),  		new Vector2(0f, 2f),  		new Vector2(0f, 2f),  		new Vector2(0f, 2f),  		new Vector2(0f, 2f),  		new Vector2(0f, 0f),  		new Vector2(0f, 0f),  		new Vector2(0f, 0f),  		new Vector2(0f, 2f),  		new Vector2(0f, 2f),  		new Vector2(0f, 2f)  	};` | `public static Vector2[] OffsetsPlayerHeadgear = new Vector2[20] { new Vector2(0f, 2f), new Vector2(0f, 2f), new Vector2(0f, 2f), new Vector2(0f, 2f), new Vector2(0f, 2f), new Vector2(0f, 2f), new Vector2(0f, 2f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 2f), new Vector2(0f, 2f), new Vector2(0f, 2f), new Vector2(0f, 2f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 2f), new Vector2(0f, 2f), new Vector2(0f, 2f) };` |
| 67 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 293 | 2 | CurrentPan | Vector2 | `public static Vector2 CurrentPan = Vector2.Zero;` | `public static Vector2 CurrentPan = Vector2.Zero;` |
| 68 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 295 | 2 | BlackFadeIn | int | `public static int BlackFadeIn;` | `public static int BlackFadeIn;` |

#### 属性（0）

无该类型成员记录。


### 4.3 细分子系统：`MainSceneMetricsState`

- 原报告章节：`4.1.20`
- 父级子系统：`RuntimeComposition`
- 分区工作包：`18` / `地图、相机与绘制`

- 上一级基线细分子系统：`MainSceneMetricsState`
- 细分职责：相机和玩家场景指标缓存。
- 边界角色：`derived/query`；最小 seam：scene metrics cache；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：4；属性：0；合计：4。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（4）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 236 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 640 | 2 | _cameraSceneMetrics | Terraria.SceneMetrics | `private static SceneMetrics _cameraSceneMetrics = new SceneMetrics();` | `private static SceneMetrics _cameraSceneMetrics = new SceneMetrics();` |
| 237 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 642 | 2 | _playerSceneMetrics | Terraria.SceneMetrics | `private static SceneMetrics _playerSceneMetrics = new SceneMetrics();` | `private static SceneMetrics _playerSceneMetrics = new SceneMetrics();` |
| 238 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 644 | 2 | _usingSeparateCameraSceneMetrics | bool | `private static bool _usingSeparateCameraSceneMetrics;` | `private static bool _usingSeparateCameraSceneMetrics;` |
| 239 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 646 | 2 | SceneState | Terraria.SceneState | `public static SceneState SceneState = new SceneState();` | `public static SceneState SceneState = new SceneState();` |

#### 属性（0）

无该类型成员记录。


### 4.4 细分子系统：`SharedSceneScanSettings`

- 原报告章节：`4.9.7`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`18` / `地图、相机与绘制`

- 上一级基线细分子系统：`SharedSceneScanSettings`
- 细分职责：场景扫描中心、可视区域和玩家视角输入。
- 边界角色：`query input`；最小 seam：scene scan request；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：4；属性：0；合计：4。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（4）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3475 | field | Terraria.SceneMetricsScanSettings | Terraria/SceneMetricsScanSettings.cs | D:\TRbackup\Version4\Terraria\SceneMetricsScanSettings.cs | 7 | 2 | VisualScanArea | Rectangle? | `public Rectangle? VisualScanArea;` | `public Rectangle? VisualScanArea;` |
| 3476 | field | Terraria.SceneMetricsScanSettings | Terraria/SceneMetricsScanSettings.cs | D:\TRbackup\Version4\Terraria\SceneMetricsScanSettings.cs | 9 | 2 | BiomeScanCenterPositionInWorld | Vector2 | `public Vector2 BiomeScanCenterPositionInWorld;` | `public Vector2 BiomeScanCenterPositionInWorld;` |
| 3477 | field | Terraria.SceneMetricsScanSettings | Terraria/SceneMetricsScanSettings.cs | D:\TRbackup\Version4\Terraria\SceneMetricsScanSettings.cs | 11 | 2 | ScanNPCPositions | bool | `public bool ScanNPCPositions;` | `public bool ScanNPCPositions;` |
| 3478 | field | Terraria.SceneMetricsScanSettings | Terraria/SceneMetricsScanSettings.cs | D:\TRbackup\Version4\Terraria\SceneMetricsScanSettings.cs | 13 | 2 | PerspectivePlayer | Terraria.Player | `public Player PerspectivePlayer;` | `public Player PerspectivePlayer;` |

#### 属性（0）

无该类型成员记录。


### 4.5 细分子系统：`SharedSceneVisualState`

- 原报告章节：`4.9.8`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`18` / `地图、相机与绘制`

- 上一级基线细分子系统：`SharedSceneVisualState`
- 细分职责：场景光照衰减、天气表现强度和视觉过渡状态。
- 边界角色：`presentation state`；最小 seam：scene visual projection；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：6；属性：0；合计：6。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（6）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3479 | field | Terraria.SceneState | Terraria/SceneState.cs | D:\TRbackup\Version4\Terraria\SceneState.cs | 16 | 2 | airLightDecay | float | `public float airLightDecay;` | `public float airLightDecay;` |
| 3480 | field | Terraria.SceneState | Terraria/SceneState.cs | D:\TRbackup\Version4\Terraria\SceneState.cs | 18 | 2 | solidLightDecay | float | `public float solidLightDecay;` | `public float solidLightDecay;` |
| 3481 | field | Terraria.SceneState | Terraria/SceneState.cs | D:\TRbackup\Version4\Terraria\SceneState.cs | 20 | 2 | outsideWeatherEffectIntensity | float | `public float outsideWeatherEffectIntensity;` | `public float outsideWeatherEffectIntensity;` |
| 3482 | field | Terraria.SceneState | Terraria/SceneState.cs | D:\TRbackup\Version4\Terraria\SceneState.cs | 22 | 2 | _strongBlizzardSound | SlotId | `private SlotId _strongBlizzardSound = SlotId.Invalid;` | `private SlotId _strongBlizzardSound = SlotId.Invalid;` |
| 3483 | field | Terraria.SceneState | Terraria/SceneState.cs | D:\TRbackup\Version4\Terraria\SceneState.cs | 24 | 2 | _insideBlizzardSound | SlotId | `private SlotId _insideBlizzardSound = SlotId.Invalid;` | `private SlotId _insideBlizzardSound = SlotId.Invalid;` |
| 3484 | field | Terraria.SceneState | Terraria/SceneState.cs | D:\TRbackup\Version4\Terraria\SceneState.cs | 26 | 2 | skipTransitions | bool | `public bool skipTransitions;` | `public bool skipTransitions;` |

#### 属性（0）

无该类型成员记录。


### 4.6 细分子系统：`SharedCaptureAndCameraSupport`

- 原报告章节：`4.9.9`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`18` / `地图、相机与绘制`

- 上一级基线细分子系统：`SharedCaptureAndCameraSupport`
- 细分职责：截图、无人机相机和钻头调试绘制支持。
- 边界角色：`presentation`；最小 seam：capture/camera adapter；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：3；声明类型数：3；字段：5；属性：1；合计：6。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（5）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1115 | field | Terraria.DataStructures.DrillDebugDraw | Terraria.DataStructures/DrillDebugDraw.cs | D:\TRbackup\Version4\Terraria.DataStructures\DrillDebugDraw.cs | 7 | 2 | point | Vector2 | `public Vector2 point;` | `public Vector2 point;` |
| 1116 | field | Terraria.DataStructures.DrillDebugDraw | Terraria.DataStructures/DrillDebugDraw.cs | D:\TRbackup\Version4\Terraria.DataStructures\DrillDebugDraw.cs | 9 | 2 | color | Color | `public Color color;` | `public Color color;` |
| 1117 | field | Terraria.DataStructures.DroneCameraTracker | Terraria.DataStructures/DroneCameraTracker.cs | D:\TRbackup\Version4\Terraria.DataStructures\DroneCameraTracker.cs | 7 | 2 | _trackedProjectile | Terraria.Projectile | `private Projectile _trackedProjectile;` | `private Projectile _trackedProjectile;` |
| 1118 | field | Terraria.DataStructures.DroneCameraTracker | Terraria.DataStructures/DroneCameraTracker.cs | D:\TRbackup\Version4\Terraria.DataStructures\DroneCameraTracker.cs | 9 | 2 | _lastTrackedType | int | `private int _lastTrackedType;` | `private int _lastTrackedType;` |
| 2633 | field | Terraria.Graphics.Capture.CaptureManager | Terraria.Graphics.Capture/CaptureManager.cs | D:\TRbackup\Version4\Terraria.Graphics.Capture\CaptureManager.cs | 9 | 2 | Instance | Terraria.Graphics.Capture.CaptureManager | `public static CaptureManager Instance = new CaptureManager();` | `public static CaptureManager Instance = new CaptureManager();` |

#### 属性（1）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3872 | property | Terraria.Graphics.Capture.CaptureManager | Terraria.Graphics.Capture/CaptureManager.cs | D:\TRbackup\Version4\Terraria.Graphics.Capture\CaptureManager.cs | 15 | 2 | IsCapturing | bool | `public bool IsCapturing { get { //if (Main.dedServ) return false; } }` | `public bool IsCapturing { get { //if (Main.dedServ) return false; } }` |


### 4.7 细分子系统：`LightMapCacheState`

- 原报告章节：`4.9.76`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`18` / `地图、相机与绘制`

- 上一级基线细分子系统：`LightMapCacheState`
- 细分职责：光照图颜色和掩码缓存。
- 边界角色：`cache`；最小 seam：light map cache port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：5；属性：8；合计：13。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（5）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2673 | field | Terraria.Graphics.Light.LightMap | Terraria.Graphics.Light/LightMap.cs | D:\TRbackup\Version4\Terraria.Graphics.Light\LightMap.cs | 9 | 2 | _colors | Vector3[] | `private Vector3[] _colors;` | `private Vector3[] _colors;` |
| 2674 | field | Terraria.Graphics.Light.LightMap | Terraria.Graphics.Light/LightMap.cs | D:\TRbackup\Version4\Terraria.Graphics.Light\LightMap.cs | 11 | 2 | _mask | Terraria.Graphics.Light.LightMaskMode[] | `private LightMaskMode[] _mask;` | `private LightMaskMode[] _mask;` |
| 2675 | field | Terraria.Graphics.Light.LightMap | Terraria.Graphics.Light/LightMap.cs | D:\TRbackup\Version4\Terraria.Graphics.Light\LightMap.cs | 13 | 2 | _random | Terraria.Utilities.FastRandom | `private FastRandom _random = FastRandom.CreateWithRandomSeed();` | `private FastRandom _random = FastRandom.CreateWithRandomSeed();` |
| 2676 | field | Terraria.Graphics.Light.LightMap | Terraria.Graphics.Light/LightMap.cs | D:\TRbackup\Version4\Terraria.Graphics.Light\LightMap.cs | 15 | 2 | DEFAULT_WIDTH | int | `private const int DEFAULT_WIDTH = 203;` | `private const int DEFAULT_WIDTH = 203;` |
| 2677 | field | Terraria.Graphics.Light.LightMap | Terraria.Graphics.Light/LightMap.cs | D:\TRbackup\Version4\Terraria.Graphics.Light\LightMap.cs | 17 | 2 | DEFAULT_HEIGHT | int | `private const int DEFAULT_HEIGHT = 203;` | `private const int DEFAULT_HEIGHT = 203;` |

#### 属性（8）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3876 | property | Terraria.Graphics.Light.LightMap | Terraria.Graphics.Light/LightMap.cs | D:\TRbackup\Version4\Terraria.Graphics.Light\LightMap.cs | 19 | 2 | Width | int | `public int Width { get; private set; }` | `public int Width { get; private set; }` |
| 3877 | property | Terraria.Graphics.Light.LightMap | Terraria.Graphics.Light/LightMap.cs | D:\TRbackup\Version4\Terraria.Graphics.Light\LightMap.cs | 21 | 2 | Height | int | `public int Height { get; private set; }` | `public int Height { get; private set; }` |
| 3878 | property | Terraria.Graphics.Light.LightMap | Terraria.Graphics.Light/LightMap.cs | D:\TRbackup\Version4\Terraria.Graphics.Light\LightMap.cs | 23 | 2 | LightDecayThroughAir | float | `public float LightDecayThroughAir { get; set; }` | `public float LightDecayThroughAir { get; set; }` |
| 3879 | property | Terraria.Graphics.Light.LightMap | Terraria.Graphics.Light/LightMap.cs | D:\TRbackup\Version4\Terraria.Graphics.Light\LightMap.cs | 25 | 2 | LightDecayThroughSolid | float | `public float LightDecayThroughSolid { get; set; }` | `public float LightDecayThroughSolid { get; set; }` |
| 3880 | property | Terraria.Graphics.Light.LightMap | Terraria.Graphics.Light/LightMap.cs | D:\TRbackup\Version4\Terraria.Graphics.Light\LightMap.cs | 27 | 2 | LightDecayThroughCrackedBrick | float | `public float LightDecayThroughCrackedBrick { get; set; }` | `public float LightDecayThroughCrackedBrick { get; set; }` |
| 3881 | property | Terraria.Graphics.Light.LightMap | Terraria.Graphics.Light/LightMap.cs | D:\TRbackup\Version4\Terraria.Graphics.Light\LightMap.cs | 29 | 2 | LightDecayThroughWater | Vector3 | `public Vector3 LightDecayThroughWater { get; set; }` | `public Vector3 LightDecayThroughWater { get; set; }` |
| 3882 | property | Terraria.Graphics.Light.LightMap | Terraria.Graphics.Light/LightMap.cs | D:\TRbackup\Version4\Terraria.Graphics.Light\LightMap.cs | 31 | 2 | LightDecayThroughHoney | Vector3 | `public Vector3 LightDecayThroughHoney { get; set; }` | `public Vector3 LightDecayThroughHoney { get; set; }` |
| 3883 | property | Terraria.Graphics.Light.LightMap | Terraria.Graphics.Light/LightMap.cs | D:\TRbackup\Version4\Terraria.Graphics.Light\LightMap.cs | 33 | 2 | this[] | Vector3 | `public Vector3 this[int x, int y] { get { return _colors[IndexOf(x, y)]; } set { _colors[IndexOf(x, y)] = value; } }` | `public Vector3 this[int x, int y] { get { return _colors[IndexOf(x, y)]; } set { _colors[IndexOf(x, y)] = value; } }` |


### 4.8 细分子系统：`TileLightScannerState`

- 原报告章节：`4.9.77`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`18` / `地图、相机与绘制`

- 上一级基线细分子系统：`TileLightScannerState`
- 细分职责：Tile 光照扫描和随机扫描辅助状态。
- 边界角色：`query`；最小 seam：tile light scanner port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：1；属性：0；合计：1。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（1）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2678 | field | Terraria.Graphics.Light.TileLightScanner | Terraria.Graphics.Light/TileLightScanner.cs | D:\TRbackup\Version4\Terraria.Graphics.Light\TileLightScanner.cs | 13 | 2 | _random | Terraria.Utilities.FastRandom | `private FastRandom _random = FastRandom.CreateWithRandomSeed();` | `private FastRandom _random = FastRandom.CreateWithRandomSeed();` |

#### 属性（0）

无该类型成员记录。


### 4.9 细分子系统：`DrawAnimationAndFrameState`

- 原报告章节：`4.9.80`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`18` / `地图、相机与绘制`

- 上一级基线细分子系统：`DrawAnimationAndFrameState`
- 细分职责：动画、绘制动画和精灵帧状态。
- 边界角色：`presentation/state`；最小 seam：draw animation frame port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：4；声明类型数：4；字段：18；属性：2；合计：20。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（18）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1096 | field | Terraria.DataStructures.DrawAnimation | Terraria.DataStructures/DrawAnimation.cs | D:\TRbackup\Version4\Terraria.DataStructures\DrawAnimation.cs | 8 | 2 | Frame | int | `public int Frame;` | `public int Frame;` |
| 1097 | field | Terraria.DataStructures.DrawAnimation | Terraria.DataStructures/DrawAnimation.cs | D:\TRbackup\Version4\Terraria.DataStructures\DrawAnimation.cs | 10 | 2 | FrameCount | int | `public int FrameCount;` | `public int FrameCount;` |
| 1098 | field | Terraria.DataStructures.DrawAnimation | Terraria.DataStructures/DrawAnimation.cs | D:\TRbackup\Version4\Terraria.DataStructures\DrawAnimation.cs | 12 | 2 | TicksPerFrame | int | `public int TicksPerFrame;` | `public int TicksPerFrame;` |
| 1099 | field | Terraria.DataStructures.DrawAnimation | Terraria.DataStructures/DrawAnimation.cs | D:\TRbackup\Version4\Terraria.DataStructures\DrawAnimation.cs | 14 | 2 | FrameCounter | int | `public int FrameCounter;` | `public int FrameCounter;` |
| 1100 | field | Terraria.DataStructures.DrawAnimationVertical | Terraria.DataStructures/DrawAnimationVertical.cs | D:\TRbackup\Version4\Terraria.DataStructures\DrawAnimationVertical.cs | 8 | 2 | PingPong | bool | `public bool PingPong;` | `public bool PingPong;` |
| 1101 | field | Terraria.DataStructures.DrawAnimationVertical | Terraria.DataStructures/DrawAnimationVertical.cs | D:\TRbackup\Version4\Terraria.DataStructures\DrawAnimationVertical.cs | 10 | 2 | NotActuallyAnimating | bool | `public bool NotActuallyAnimating;` | `public bool NotActuallyAnimating;` |
| 1293 | field | Terraria.DataStructures.SpriteFrame | Terraria.DataStructures/SpriteFrame.cs | D:\TRbackup\Version4\Terraria.DataStructures\SpriteFrame.cs | 8 | 2 | PaddingX | int | `public int PaddingX;` | `public int PaddingX;` |
| 1294 | field | Terraria.DataStructures.SpriteFrame | Terraria.DataStructures/SpriteFrame.cs | D:\TRbackup\Version4\Terraria.DataStructures\SpriteFrame.cs | 10 | 2 | PaddingY | int | `public int PaddingY;` | `public int PaddingY;` |
| 1295 | field | Terraria.DataStructures.SpriteFrame | Terraria.DataStructures/SpriteFrame.cs | D:\TRbackup\Version4\Terraria.DataStructures\SpriteFrame.cs | 12 | 2 | _currentColumn | byte | `private byte _currentColumn;` | `private byte _currentColumn;` |
| 1296 | field | Terraria.DataStructures.SpriteFrame | Terraria.DataStructures/SpriteFrame.cs | D:\TRbackup\Version4\Terraria.DataStructures\SpriteFrame.cs | 14 | 2 | _currentRow | byte | `private byte _currentRow;` | `private byte _currentRow;` |
| 1297 | field | Terraria.DataStructures.SpriteFrame | Terraria.DataStructures/SpriteFrame.cs | D:\TRbackup\Version4\Terraria.DataStructures\SpriteFrame.cs | 16 | 2 | ColumnCount | byte | `public readonly byte ColumnCount;` | `public readonly byte ColumnCount;` |
| 1298 | field | Terraria.DataStructures.SpriteFrame | Terraria.DataStructures/SpriteFrame.cs | D:\TRbackup\Version4\Terraria.DataStructures\SpriteFrame.cs | 18 | 2 | RowCount | byte | `public readonly byte RowCount;` | `public readonly byte RowCount;` |
| 2991 | field | Terraria.Animation | Terraria/Animation.cs | D:\TRbackup\Version4\Terraria\Animation.cs | 8 | 2 | _animations | System.Collections.Generic.List<Terraria.Animation> | `private static List<Animation> _animations;` | `private static List<Animation> _animations;` |
| 2992 | field | Terraria.Animation | Terraria/Animation.cs | D:\TRbackup\Version4\Terraria\Animation.cs | 10 | 2 | _temporaryAnimations | System.Collections.Generic.Dictionary<Terraria.DataStructures.Point16, Terraria.Animation> | `private static Dictionary<Point16, Animation> _temporaryAnimations;` | `private static Dictionary<Point16, Animation> _temporaryAnimations;` |
| 2993 | field | Terraria.Animation | Terraria/Animation.cs | D:\TRbackup\Version4\Terraria\Animation.cs | 12 | 2 | _awaitingRemoval | System.Collections.Generic.List<Terraria.DataStructures.Point16> | `private static List<Point16> _awaitingRemoval;` | `private static List<Point16> _awaitingRemoval;` |
| 2994 | field | Terraria.Animation | Terraria/Animation.cs | D:\TRbackup\Version4\Terraria\Animation.cs | 14 | 2 | _awaitingAddition | System.Collections.Generic.List<Terraria.Animation> | `private static List<Animation> _awaitingAddition;` | `private static List<Animation> _awaitingAddition;` |
| 2995 | field | Terraria.Animation | Terraria/Animation.cs | D:\TRbackup\Version4\Terraria\Animation.cs | 16 | 2 | _coordinates | Terraria.DataStructures.Point16 | `private Point16 _coordinates;` | `private Point16 _coordinates;` |
| 2996 | field | Terraria.Animation | Terraria/Animation.cs | D:\TRbackup\Version4\Terraria\Animation.cs | 18 | 2 | _tileType | ushort | `private ushort _tileType;` | `private ushort _tileType;` |

#### 属性（2）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3700 | property | Terraria.DataStructures.SpriteFrame | Terraria.DataStructures/SpriteFrame.cs | D:\TRbackup\Version4\Terraria.DataStructures\SpriteFrame.cs | 20 | 2 | CurrentColumn | byte | `public byte CurrentColumn { get { return _currentColumn; } set { _currentColumn = value; } }` | `public byte CurrentColumn { get { return _currentColumn; } set { _currentColumn = value; } }` |
| 3701 | property | Terraria.DataStructures.SpriteFrame | Terraria.DataStructures/SpriteFrame.cs | D:\TRbackup\Version4\Terraria.DataStructures\SpriteFrame.cs | 32 | 2 | CurrentRow | byte | `public byte CurrentRow { get { return _currentRow; } set { _currentRow = value; } }` | `public byte CurrentRow { get { return _currentRow; } set { _currentRow = value; } }` |


### 4.10 细分子系统：`DrawCommandAndBatchState`

- 原报告章节：`4.9.81`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`18` / `地图、相机与绘制`

- 上一级基线细分子系统：`DrawCommandAndBatchState`
- 细分职责：绘制数据命令和 SpriteBatch 批处理状态。
- 边界角色：`presentation/command`；最小 seam：draw command batch port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：2；声明类型数：2；字段：20；属性：0；合计：20。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（20）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1102 | field | Terraria.DataStructures.DrawData | Terraria.DataStructures/DrawData.cs | D:\TRbackup\Version4\Terraria.DataStructures\DrawData.cs | 8 | 2 | texture | Texture2D | `public Texture2D texture;` | `public Texture2D texture;` |
| 1103 | field | Terraria.DataStructures.DrawData | Terraria.DataStructures/DrawData.cs | D:\TRbackup\Version4\Terraria.DataStructures\DrawData.cs | 10 | 2 | position | Vector2 | `public Vector2 position;` | `public Vector2 position;` |
| 1104 | field | Terraria.DataStructures.DrawData | Terraria.DataStructures/DrawData.cs | D:\TRbackup\Version4\Terraria.DataStructures\DrawData.cs | 12 | 2 | destinationRectangle | Rectangle | `public Rectangle destinationRectangle;` | `public Rectangle destinationRectangle;` |
| 1105 | field | Terraria.DataStructures.DrawData | Terraria.DataStructures/DrawData.cs | D:\TRbackup\Version4\Terraria.DataStructures\DrawData.cs | 14 | 2 | sourceRect | Rectangle? | `public Rectangle? sourceRect;` | `public Rectangle? sourceRect;` |
| 1106 | field | Terraria.DataStructures.DrawData | Terraria.DataStructures/DrawData.cs | D:\TRbackup\Version4\Terraria.DataStructures\DrawData.cs | 16 | 2 | color | Color | `public Color color;` | `public Color color;` |
| 1107 | field | Terraria.DataStructures.DrawData | Terraria.DataStructures/DrawData.cs | D:\TRbackup\Version4\Terraria.DataStructures\DrawData.cs | 18 | 2 | rotation | float | `public float rotation;` | `public float rotation;` |
| 1108 | field | Terraria.DataStructures.DrawData | Terraria.DataStructures/DrawData.cs | D:\TRbackup\Version4\Terraria.DataStructures\DrawData.cs | 20 | 2 | origin | Vector2 | `public Vector2 origin;` | `public Vector2 origin;` |
| 1109 | field | Terraria.DataStructures.DrawData | Terraria.DataStructures/DrawData.cs | D:\TRbackup\Version4\Terraria.DataStructures\DrawData.cs | 22 | 2 | scale | Vector2 | `public Vector2 scale;` | `public Vector2 scale;` |
| 1110 | field | Terraria.DataStructures.DrawData | Terraria.DataStructures/DrawData.cs | D:\TRbackup\Version4\Terraria.DataStructures\DrawData.cs | 24 | 2 | effect | SpriteEffects | `public SpriteEffects effect;` | `public SpriteEffects effect;` |
| 1111 | field | Terraria.DataStructures.DrawData | Terraria.DataStructures/DrawData.cs | D:\TRbackup\Version4\Terraria.DataStructures\DrawData.cs | 26 | 2 | shader | int | `public int shader;` | `public int shader;` |
| 1112 | field | Terraria.DataStructures.DrawData | Terraria.DataStructures/DrawData.cs | D:\TRbackup\Version4\Terraria.DataStructures\DrawData.cs | 28 | 2 | ignorePlayerRotation | bool | `public bool ignorePlayerRotation;` | `public bool ignorePlayerRotation;` |
| 1113 | field | Terraria.DataStructures.DrawData | Terraria.DataStructures/DrawData.cs | D:\TRbackup\Version4\Terraria.DataStructures\DrawData.cs | 30 | 2 | useDestinationRectangle | bool | `public readonly bool useDestinationRectangle;` | `public readonly bool useDestinationRectangle;` |
| 1114 | field | Terraria.DataStructures.DrawData | Terraria.DataStructures/DrawData.cs | D:\TRbackup\Version4\Terraria.DataStructures\DrawData.cs | 32 | 2 | nullRectangle | Rectangle? | `public static Rectangle? nullRectangle;` | `public static Rectangle? nullRectangle;` |
| 1286 | field | Terraria.DataStructures.SpriteBatchBeginner | Terraria.DataStructures/SpriteBatchBeginner.cs | D:\TRbackup\Version4\Terraria.DataStructures\SpriteBatchBeginner.cs | 9 | 2 | sortMode | SpriteSortMode | `private SpriteSortMode sortMode;` | `private SpriteSortMode sortMode;` |
| 1287 | field | Terraria.DataStructures.SpriteBatchBeginner | Terraria.DataStructures/SpriteBatchBeginner.cs | D:\TRbackup\Version4\Terraria.DataStructures\SpriteBatchBeginner.cs | 11 | 2 | blendState | BlendState | `private BlendState blendState;` | `private BlendState blendState;` |
| 1288 | field | Terraria.DataStructures.SpriteBatchBeginner | Terraria.DataStructures/SpriteBatchBeginner.cs | D:\TRbackup\Version4\Terraria.DataStructures\SpriteBatchBeginner.cs | 13 | 2 | samplerState | SamplerState | `private SamplerState samplerState;` | `private SamplerState samplerState;` |
| 1289 | field | Terraria.DataStructures.SpriteBatchBeginner | Terraria.DataStructures/SpriteBatchBeginner.cs | D:\TRbackup\Version4\Terraria.DataStructures\SpriteBatchBeginner.cs | 15 | 2 | depthStencilState | DepthStencilState | `private DepthStencilState depthStencilState;` | `private DepthStencilState depthStencilState;` |
| 1290 | field | Terraria.DataStructures.SpriteBatchBeginner | Terraria.DataStructures/SpriteBatchBeginner.cs | D:\TRbackup\Version4\Terraria.DataStructures\SpriteBatchBeginner.cs | 17 | 2 | rasterizerState | RasterizerState | `private RasterizerState rasterizerState;` | `private RasterizerState rasterizerState;` |
| 1291 | field | Terraria.DataStructures.SpriteBatchBeginner | Terraria.DataStructures/SpriteBatchBeginner.cs | D:\TRbackup\Version4\Terraria.DataStructures\SpriteBatchBeginner.cs | 19 | 2 | effect | Effect | `private Effect effect;` | `private Effect effect;` |
| 1292 | field | Terraria.DataStructures.SpriteBatchBeginner | Terraria.DataStructures/SpriteBatchBeginner.cs | D:\TRbackup\Version4\Terraria.DataStructures\SpriteBatchBeginner.cs | 21 | 2 | transformMatrix | Matrix | `public Matrix transformMatrix;` | `public Matrix transformMatrix;` |

#### 属性（0）

无该类型成员记录。


### 4.11 细分子系统：`WorldDrawingAuxiliaryState`

- 原报告章节：`4.9.82`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`18` / `地图、相机与绘制`

- 上一级基线细分子系统：`WorldDrawingAuxiliaryState`
- 细分职责：粒子编排、Tile 绘制、地平线和辅助渲染状态。
- 边界角色：`presentation`；最小 seam：world drawing auxiliary port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：4；声明类型数：4；字段：10；属性：0；合计：10。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（10）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1526 | field | Terraria.GameContent.Drawing.HorizonHelper | Terraria.GameContent.Drawing/HorizonHelper.cs | D:\TRbackup\Version4\Terraria.GameContent.Drawing\HorizonHelper.cs | 14 | 2 | _horizonBlendState | BlendState | `private BlendState _horizonBlendState = new BlendState  	{  		AlphaSourceBlend = Blend.Zero,  		AlphaDestinationBlend = Blend.InverseSourceAlpha,  		ColorSourceBlend = Blend.Zero,  		ColorDestinationBlend = Blend.InverseSourceAlpha  	};` | `private BlendState _horizonBlendState = new BlendState { AlphaSourceBlend = Blend.Zero, AlphaDestinationBlend = Blend.InverseSourceAlpha, ColorSourceBlend = Blend.Zero, ColorDestinationBlend = Blend.InverseSourceAlpha };` |
| 1527 | field | Terraria.GameContent.Drawing.HorizonHelper | Terraria.GameContent.Drawing/HorizonHelper.cs | D:\TRbackup\Version4\Terraria.GameContent.Drawing\HorizonHelper.cs | 22 | 2 | MoonColors | Color[] | `private static Color[] MoonColors = new Color[9]  	{  		new Color(230, 235, 255),  		new Color(250, 235, 160),  		new Color(230, 255, 230),  		new Color(160, 240, 255),  		new Color(180, 255, 255),  		new Color(230, 255, 230),  		new Color(255, 180, 255),  		new Color(255, 200, 180),  		new Color(225, 180, 255)  	};` | `private static Color[] MoonColors = new Color[9] { new Color(230, 235, 255), new Color(250, 235, 160), new Color(230, 255, 230), new Color(160, 240, 255), new Color(180, 255, 255), new Color(230, 255, 230), new Color(255, 180, 255), new Color(255, 200, 180), new Color(225, 180, 255) };` |
| 1528 | field | Terraria.GameContent.Drawing.NextHorizonRenderer | Terraria.GameContent.Drawing/NextHorizonRenderer.cs | D:\TRbackup\Version4\Terraria.GameContent.Drawing\NextHorizonRenderer.cs | 16 | 2 | _drawData | System.Collections.Generic.List<Terraria.DataStructures.DrawData> | `private List<DrawData> _drawData = new List<DrawData>(200);` | `private List<DrawData> _drawData = new List<DrawData>(200);` |
| 1529 | field | Terraria.GameContent.Drawing.ParticleOrchestraSettings | Terraria.GameContent.Drawing/ParticleOrchestraSettings.cs | D:\TRbackup\Version4\Terraria.GameContent.Drawing\ParticleOrchestraSettings.cs | 8 | 2 | PositionInWorld | Vector2 | `public Vector2 PositionInWorld;` | `public Vector2 PositionInWorld;` |
| 1530 | field | Terraria.GameContent.Drawing.ParticleOrchestraSettings | Terraria.GameContent.Drawing/ParticleOrchestraSettings.cs | D:\TRbackup\Version4\Terraria.GameContent.Drawing\ParticleOrchestraSettings.cs | 10 | 2 | MovementVector | Vector2 | `public Vector2 MovementVector;` | `public Vector2 MovementVector;` |
| 1531 | field | Terraria.GameContent.Drawing.ParticleOrchestraSettings | Terraria.GameContent.Drawing/ParticleOrchestraSettings.cs | D:\TRbackup\Version4\Terraria.GameContent.Drawing\ParticleOrchestraSettings.cs | 12 | 2 | UniqueInfoPiece | int | `public int UniqueInfoPiece;` | `public int UniqueInfoPiece;` |
| 1532 | field | Terraria.GameContent.Drawing.ParticleOrchestraSettings | Terraria.GameContent.Drawing/ParticleOrchestraSettings.cs | D:\TRbackup\Version4\Terraria.GameContent.Drawing\ParticleOrchestraSettings.cs | 14 | 2 | IndexOfPlayerWhoInvokedThis | byte | `public byte IndexOfPlayerWhoInvokedThis;` | `public byte IndexOfPlayerWhoInvokedThis;` |
| 1533 | field | Terraria.GameContent.Drawing.TileDrawingBase | Terraria.GameContent.Drawing/TileDrawingBase.cs | D:\TRbackup\Version4\Terraria.GameContent.Drawing\TileDrawingBase.cs | 10 | 2 | DrawOwnBlacks | bool | `public static bool DrawOwnBlacks = true;` | `public static bool DrawOwnBlacks = true;` |
| 1534 | field | Terraria.GameContent.Drawing.TileDrawingBase | Terraria.GameContent.Drawing/TileDrawingBase.cs | D:\TRbackup\Version4\Terraria.GameContent.Drawing\TileDrawingBase.cs | 12 | 2 | FlushLogData | Terraria.TimeLogger.TimeLogData | `protected TimeLogger.TimeLogData FlushLogData;` | `protected TimeLogger.TimeLogData FlushLogData;` |
| 1535 | field | Terraria.GameContent.Drawing.TileDrawingBase | Terraria.GameContent.Drawing/TileDrawingBase.cs | D:\TRbackup\Version4\Terraria.GameContent.Drawing\TileDrawingBase.cs | 14 | 2 | DrawCallLogData | Terraria.TimeLogger.TimeLogData | `protected TimeLogger.TimeLogData DrawCallLogData;` | `protected TimeLogger.TimeLogData DrawCallLogData;` |

#### 属性（0）

无该类型成员记录。


### 4.12 细分子系统：`EntityShadowPresentationState`

- 原报告章节：`4.9.108`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`18` / `地图、相机与绘制`

- 上一级基线细分子系统：`EntityShadowPresentationState`
- 细分职责：实体阴影的位置、旋转和表现快照。
- 边界角色：`presentation/snapshot`；最小 seam：entity shadow projection；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：6；属性：1；合计：7。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（6）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1119 | field | Terraria.DataStructures.EntityShadowInfo | Terraria.DataStructures/EntityShadowInfo.cs | D:\TRbackup\Version4\Terraria.DataStructures\EntityShadowInfo.cs | 7 | 2 | Position | Vector2 | `public Vector2 Position;` | `public Vector2 Position;` |
| 1120 | field | Terraria.DataStructures.EntityShadowInfo | Terraria.DataStructures/EntityShadowInfo.cs | D:\TRbackup\Version4\Terraria.DataStructures\EntityShadowInfo.cs | 9 | 2 | Rotation | float | `public float Rotation;` | `public float Rotation;` |
| 1121 | field | Terraria.DataStructures.EntityShadowInfo | Terraria.DataStructures/EntityShadowInfo.cs | D:\TRbackup\Version4\Terraria.DataStructures\EntityShadowInfo.cs | 11 | 2 | Origin | Vector2 | `public Vector2 Origin;` | `public Vector2 Origin;` |
| 1122 | field | Terraria.DataStructures.EntityShadowInfo | Terraria.DataStructures/EntityShadowInfo.cs | D:\TRbackup\Version4\Terraria.DataStructures\EntityShadowInfo.cs | 13 | 2 | Direction | int | `public int Direction;` | `public int Direction;` |
| 1123 | field | Terraria.DataStructures.EntityShadowInfo | Terraria.DataStructures/EntityShadowInfo.cs | D:\TRbackup\Version4\Terraria.DataStructures\EntityShadowInfo.cs | 15 | 2 | GravityDirection | int | `public int GravityDirection;` | `public int GravityDirection;` |
| 1124 | field | Terraria.DataStructures.EntityShadowInfo | Terraria.DataStructures/EntityShadowInfo.cs | D:\TRbackup\Version4\Terraria.DataStructures\EntityShadowInfo.cs | 17 | 2 | BodyFrameIndex | int | `public int BodyFrameIndex;` | `public int BodyFrameIndex;` |

#### 属性（1）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3688 | property | Terraria.DataStructures.EntityShadowInfo | Terraria.DataStructures/EntityShadowInfo.cs | D:\TRbackup\Version4\Terraria.DataStructures\EntityShadowInfo.cs | 19 | 2 | HeadgearOffset | Vector2 | `public Vector2 HeadgearOffset => Main.OffsetsPlayerHeadgear[BodyFrameIndex];` | `public Vector2 HeadgearOffset => Main.OffsetsPlayerHeadgear[BodyFrameIndex];` |


### 4.13 细分子系统：`SharedShaderBaseParameterState`

- 原报告章节：`4.9.120`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`18` / `地图、相机与绘制`

- 上一级基线细分子系统：`SharedShaderData`
- 细分职责：基础 shader、屏幕参数和效果参数缓存。
- 边界角色：`presentation`；最小 seam：shader parameter port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：2；声明类型数：3；字段：18；属性：3；合计：21。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（18）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2713 | field | Terraria.Graphics.Shaders.ScreenShaderData | Terraria.Graphics.Shaders/ScreenShaderData.cs | D:\TRbackup\Version4\Terraria.Graphics.Shaders\ScreenShaderData.cs | 9 | 2 | _uColor | Vector3 | `private Vector3 _uColor = Vector3.One;` | `private Vector3 _uColor = Vector3.One;` |
| 2714 | field | Terraria.Graphics.Shaders.ScreenShaderData | Terraria.Graphics.Shaders/ScreenShaderData.cs | D:\TRbackup\Version4\Terraria.Graphics.Shaders\ScreenShaderData.cs | 11 | 2 | _uSecondaryColor | Vector3 | `private Vector3 _uSecondaryColor = Vector3.One;` | `private Vector3 _uSecondaryColor = Vector3.One;` |
| 2715 | field | Terraria.Graphics.Shaders.ScreenShaderData | Terraria.Graphics.Shaders/ScreenShaderData.cs | D:\TRbackup\Version4\Terraria.Graphics.Shaders\ScreenShaderData.cs | 13 | 2 | _uOpacity | float | `private float _uOpacity = 1f;` | `private float _uOpacity = 1f;` |
| 2716 | field | Terraria.Graphics.Shaders.ScreenShaderData | Terraria.Graphics.Shaders/ScreenShaderData.cs | D:\TRbackup\Version4\Terraria.Graphics.Shaders\ScreenShaderData.cs | 15 | 2 | _globalOpacity | float | `private float _globalOpacity = 1f;` | `private float _globalOpacity = 1f;` |
| 2717 | field | Terraria.Graphics.Shaders.ScreenShaderData | Terraria.Graphics.Shaders/ScreenShaderData.cs | D:\TRbackup\Version4\Terraria.Graphics.Shaders\ScreenShaderData.cs | 17 | 2 | _uIntensity | float | `private float _uIntensity = 1f;` | `private float _uIntensity = 1f;` |
| 2718 | field | Terraria.Graphics.Shaders.ScreenShaderData | Terraria.Graphics.Shaders/ScreenShaderData.cs | D:\TRbackup\Version4\Terraria.Graphics.Shaders\ScreenShaderData.cs | 19 | 2 | _uTargetPosition | Vector2 | `private Vector2 _uTargetPosition = Vector2.One;` | `private Vector2 _uTargetPosition = Vector2.One;` |
| 2719 | field | Terraria.Graphics.Shaders.ScreenShaderData | Terraria.Graphics.Shaders/ScreenShaderData.cs | D:\TRbackup\Version4\Terraria.Graphics.Shaders\ScreenShaderData.cs | 21 | 2 | _uDirection | Vector2 | `private Vector2 _uDirection = new Vector2(0f, 1f);` | `private Vector2 _uDirection = new Vector2(0f, 1f);` |
| 2720 | field | Terraria.Graphics.Shaders.ScreenShaderData | Terraria.Graphics.Shaders/ScreenShaderData.cs | D:\TRbackup\Version4\Terraria.Graphics.Shaders\ScreenShaderData.cs | 23 | 2 | _uProgress | float | `private float _uProgress;` | `private float _uProgress;` |
| 2721 | field | Terraria.Graphics.Shaders.ScreenShaderData | Terraria.Graphics.Shaders/ScreenShaderData.cs | D:\TRbackup\Version4\Terraria.Graphics.Shaders\ScreenShaderData.cs | 25 | 2 | _uImageOffset | Vector2 | `private Vector2 _uImageOffset = Vector2.Zero;` | `private Vector2 _uImageOffset = Vector2.Zero;` |
| 2722 | field | Terraria.Graphics.Shaders.ScreenShaderData | Terraria.Graphics.Shaders/ScreenShaderData.cs | D:\TRbackup\Version4\Terraria.Graphics.Shaders\ScreenShaderData.cs | 27 | 2 | _uAssetImages | Asset<Texture2D>[] | `private Asset<Texture2D>[] _uAssetImages = new Asset<Texture2D>[3];` | `private Asset<Texture2D>[] _uAssetImages = new Asset<Texture2D>[3];` |
| 2723 | field | Terraria.Graphics.Shaders.ScreenShaderData | Terraria.Graphics.Shaders/ScreenShaderData.cs | D:\TRbackup\Version4\Terraria.Graphics.Shaders\ScreenShaderData.cs | 29 | 2 | _uCustomImages | Texture2D[] | `private Texture2D[] _uCustomImages = new Texture2D[3];` | `private Texture2D[] _uCustomImages = new Texture2D[3];` |
| 2724 | field | Terraria.Graphics.Shaders.ScreenShaderData | Terraria.Graphics.Shaders/ScreenShaderData.cs | D:\TRbackup\Version4\Terraria.Graphics.Shaders\ScreenShaderData.cs | 31 | 2 | _samplerStates | SamplerState[] | `private SamplerState[] _samplerStates = new SamplerState[3];` | `private SamplerState[] _samplerStates = new SamplerState[3];` |
| 2725 | field | Terraria.Graphics.Shaders.ScreenShaderData | Terraria.Graphics.Shaders/ScreenShaderData.cs | D:\TRbackup\Version4\Terraria.Graphics.Shaders\ScreenShaderData.cs | 33 | 2 | _imageScales | Vector2[] | `private Vector2[] _imageScales = new Vector2[3]  	{  		Vector2.One,  		Vector2.One,  		Vector2.One  	};` | `private Vector2[] _imageScales = new Vector2[3] { Vector2.One, Vector2.One, Vector2.One };` |
| 2726 | field | Terraria.Graphics.Shaders.ScreenShaderData | Terraria.Graphics.Shaders/ScreenShaderData.cs | D:\TRbackup\Version4\Terraria.Graphics.Shaders\ScreenShaderData.cs | 40 | 2 | uImageSize | Terraria.Graphics.Shaders.ShaderData.EffectParameter<Vector2>[] | `private EffectParameter<Vector2>[] uImageSize = new EffectParameter<Vector2>[4];` | `private EffectParameter<Vector2>[] uImageSize = new EffectParameter<Vector2>[4];` |
| 2727 | field | Terraria.Graphics.Shaders.ShaderData.EffectParameter<T> | Terraria.Graphics.Shaders/ShaderData.cs | D:\TRbackup\Version4\Terraria.Graphics.Shaders\ShaderData.cs | 14 | 3 | _setValue | System.Action<T> | `private readonly Action<T> _setValue;` | `private readonly Action<T> _setValue;` |
| 2728 | field | Terraria.Graphics.Shaders.ShaderData.EffectParameter<T> | Terraria.Graphics.Shaders/ShaderData.cs | D:\TRbackup\Version4\Terraria.Graphics.Shaders\ShaderData.cs | 16 | 3 | _cachedParameters | System.Runtime.CompilerServices.ConditionalWeakTable<Terraria.Graphics.Shaders.ShaderData.EffectParameter, object> | `private static ConditionalWeakTable<EffectParameter, object> _cachedParameters = new ConditionalWeakTable<EffectParameter, object>();` | `private static ConditionalWeakTable<EffectParameter, object> _cachedParameters = new ConditionalWeakTable<EffectParameter, object>();` |
| 2729 | field | Terraria.Graphics.Shaders.ShaderData | Terraria.Graphics.Shaders/ShaderData.cs | D:\TRbackup\Version4\Terraria.Graphics.Shaders\ShaderData.cs | 24 | 2 | _shader | Asset<Effect> | `private readonly Asset<Effect> _shader;` | `private readonly Asset<Effect> _shader;` |
| 2730 | field | Terraria.Graphics.Shaders.ShaderData | Terraria.Graphics.Shaders/ShaderData.cs | D:\TRbackup\Version4\Terraria.Graphics.Shaders\ShaderData.cs | 26 | 2 | _passName | string | `private readonly string _passName;` | `private readonly string _passName;` |

#### 属性（3）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3887 | property | Terraria.Graphics.Shaders.ScreenShaderData | Terraria.Graphics.Shaders/ScreenShaderData.cs | D:\TRbackup\Version4\Terraria.Graphics.Shaders\ScreenShaderData.cs | 42 | 2 | Intensity | float | `public float Intensity => _uIntensity;` | `public float Intensity => _uIntensity;` |
| 3888 | property | Terraria.Graphics.Shaders.ScreenShaderData | Terraria.Graphics.Shaders/ScreenShaderData.cs | D:\TRbackup\Version4\Terraria.Graphics.Shaders\ScreenShaderData.cs | 44 | 2 | CombinedOpacity | float | `public float CombinedOpacity => _uOpacity * _globalOpacity;` | `public float CombinedOpacity => _uOpacity * _globalOpacity;` |
| 3889 | property | Terraria.Graphics.Shaders.ShaderData | Terraria.Graphics.Shaders/ShaderData.cs | D:\TRbackup\Version4\Terraria.Graphics.Shaders\ShaderData.cs | 28 | 2 | Shader | Effect | `public Effect Shader { get { if (_shader != null) { return _shader.Value; } return null; } }` | `public Effect Shader { get { if (_shader != null) { return _shader.Value; } return null; } }` |


### 4.14 细分子系统：`SharedLightingCoordinatorState`

- 原报告章节：`4.9.130`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`18` / `地图、相机与绘制`

- 上一级基线细分子系统：`LightingEngineState`
- 细分职责：新旧光照引擎切换、活动引擎和逐帧光照状态。
- 边界角色：`state/query`；最小 seam：lighting coordinator port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：2；声明类型数：3；字段：16；属性：3；合计：19。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（16）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2664 | field | Terraria.Graphics.Light.LightingEngine.PerFrameLight | Terraria.Graphics.Light/LightingEngine.cs | D:\TRbackup\Version4\Terraria.Graphics.Light\LightingEngine.cs | 15 | 3 | Position | Point | `public readonly Point Position;` | `public readonly Point Position;` |
| 2665 | field | Terraria.Graphics.Light.LightingEngine.PerFrameLight | Terraria.Graphics.Light/LightingEngine.cs | D:\TRbackup\Version4\Terraria.Graphics.Light\LightingEngine.cs | 17 | 3 | Color | Vector3 | `public readonly Vector3 Color;` | `public readonly Vector3 Color;` |
| 2666 | field | Terraria.Graphics.Light.LightingEngine | Terraria.Graphics.Light/LightingEngine.cs | D:\TRbackup\Version4\Terraria.Graphics.Light\LightingEngine.cs | 26 | 2 | AREA_PADDING | int | `public const int AREA_PADDING = 28;` | `public const int AREA_PADDING = 28;` |
| 2667 | field | Terraria.Graphics.Light.LightingEngine | Terraria.Graphics.Light/LightingEngine.cs | D:\TRbackup\Version4\Terraria.Graphics.Light\LightingEngine.cs | 28 | 2 | NON_VISIBLE_PADDING | int | `private const int NON_VISIBLE_PADDING = 18;` | `private const int NON_VISIBLE_PADDING = 18;` |
| 2668 | field | Terraria.Graphics.Light.LightingEngine | Terraria.Graphics.Light/LightingEngine.cs | D:\TRbackup\Version4\Terraria.Graphics.Light\LightingEngine.cs | 30 | 2 | _perFrameLights | System.Collections.Generic.List<Terraria.Graphics.Light.LightingEngine.PerFrameLight> | `private List<PerFrameLight> _perFrameLights = new List<PerFrameLight>();` | `private List<PerFrameLight> _perFrameLights = new List<PerFrameLight>();` |
| 2669 | field | Terraria.Graphics.Light.LightingEngine | Terraria.Graphics.Light/LightingEngine.cs | D:\TRbackup\Version4\Terraria.Graphics.Light\LightingEngine.cs | 32 | 2 | _oldPerFrameLights | System.Collections.Generic.List<Terraria.Graphics.Light.LightingEngine.PerFrameLight> | `private List<PerFrameLight> _oldPerFrameLights = new List<PerFrameLight>();` | `private List<PerFrameLight> _oldPerFrameLights = new List<PerFrameLight>();` |
| 2670 | field | Terraria.Graphics.Light.LightingEngine | Terraria.Graphics.Light/LightingEngine.cs | D:\TRbackup\Version4\Terraria.Graphics.Light\LightingEngine.cs | 34 | 2 | _tileScanner | Terraria.Graphics.Light.TileLightScanner | `private TileLightScanner _tileScanner = new TileLightScanner();` | `private TileLightScanner _tileScanner = new TileLightScanner();` |
| 2671 | field | Terraria.Graphics.Light.LightingEngine | Terraria.Graphics.Light/LightingEngine.cs | D:\TRbackup\Version4\Terraria.Graphics.Light\LightingEngine.cs | 36 | 2 | _activeLightMap | Terraria.Graphics.Light.LightMap | `private LightMap _activeLightMap = new LightMap();` | `private LightMap _activeLightMap = new LightMap();` |
| 2672 | field | Terraria.Graphics.Light.LightingEngine | Terraria.Graphics.Light/LightingEngine.cs | D:\TRbackup\Version4\Terraria.Graphics.Light\LightingEngine.cs | 38 | 2 | _workingLightMap | Terraria.Graphics.Light.LightMap | `private LightMap _workingLightMap = new LightMap();` | `private LightMap _workingLightMap = new LightMap();` |
| 3306 | field | Terraria.Lighting | Terraria/Lighting.cs | D:\TRbackup\Version4\Terraria\Lighting.cs | 12 | 2 | DEFAULT_GLOBAL_BRIGHTNESS | float | `private const float DEFAULT_GLOBAL_BRIGHTNESS = 1.2f;` | `private const float DEFAULT_GLOBAL_BRIGHTNESS = 1.2f;` |
| 3307 | field | Terraria.Lighting | Terraria/Lighting.cs | D:\TRbackup\Version4\Terraria\Lighting.cs | 14 | 2 | BLIND_GLOBAL_BRIGHTNESS | float | `private const float BLIND_GLOBAL_BRIGHTNESS = 1f;` | `private const float BLIND_GLOBAL_BRIGHTNESS = 1f;` |
| 3308 | field | Terraria.Lighting | Terraria/Lighting.cs | D:\TRbackup\Version4\Terraria\Lighting.cs | 16 | 2 | OffScreenTiles | int | `[Old] public static int OffScreenTiles = 45;` | `[Old] public static int OffScreenTiles = 45;` |
| 3309 | field | Terraria.Lighting | Terraria/Lighting.cs | D:\TRbackup\Version4\Terraria\Lighting.cs | 19 | 2 | _mode | Terraria.Graphics.Light.LightMode | `private static LightMode _mode = LightMode.Color;` | `private static LightMode _mode = LightMode.Color;` |
| 3310 | field | Terraria.Lighting | Terraria/Lighting.cs | D:\TRbackup\Version4\Terraria\Lighting.cs | 21 | 2 | NewEngine | Terraria.Graphics.Light.LightingEngine | `private static readonly LightingEngine NewEngine = new LightingEngine();` | `private static readonly LightingEngine NewEngine = new LightingEngine();` |
| 3311 | field | Terraria.Lighting | Terraria/Lighting.cs | D:\TRbackup\Version4\Terraria\Lighting.cs | 23 | 2 | LegacyEngine | Terraria.Graphics.Light.LegacyLighting | `private static readonly LegacyLighting LegacyEngine = new LegacyLighting();` | `private static readonly LegacyLighting LegacyEngine = new LegacyLighting();` |
| 3312 | field | Terraria.Lighting | Terraria/Lighting.cs | D:\TRbackup\Version4\Terraria\Lighting.cs | 25 | 2 | _activeEngine | Terraria.Graphics.Light.ILightingEngine | `private static ILightingEngine _activeEngine;` | `private static ILightingEngine _activeEngine;` |

#### 属性（3）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3972 | property | Terraria.Lighting | Terraria/Lighting.cs | D:\TRbackup\Version4\Terraria\Lighting.cs | 27 | 2 | GlobalBrightness | float | `public static float GlobalBrightness { get; set; }` | `public static float GlobalBrightness { get; set; }` |
| 3973 | property | Terraria.Lighting | Terraria/Lighting.cs | D:\TRbackup\Version4\Terraria\Lighting.cs | 29 | 2 | Mode | Terraria.Graphics.Light.LightMode | `public static LightMode Mode { get { return _mode; } set { _mode = value; switch (_mode) { case LightMode.Color: _activeEngine = NewEngine; LegacyEngine.Mode = 0; OffScreenTiles = 35; break; case LightMode.White: _activeEngine = LegacyEngine; LegacyEngine.Mode = 1; break; case LightMode.Retro: _activeEngine = LegacyEngine; LegacyEngine.Mode = 2; break; case LightMode.Trippy: _activeEngine = LegacyEngine; LegacyEngine.Mode = 3; break; } Main.renderCount = 0; Main.renderNow = false; } }` | `public static LightMode Mode { get { return _mode; } set { _mode = value; switch (_mode) { case LightMode.Color: _activeEngine = NewEngine; LegacyEngine.Mode = 0; OffScreenTiles = 35; break; case LightMode.White: _activeEngine = LegacyEngine; LegacyEngine.Mode = 1; break; case LightMode.Retro: _activeEngine = LegacyEngine; LegacyEngine.Mode = 2; break; case LightMode.Trippy: _activeEngine = LegacyEngine; LegacyEngine.Mode = 3; break; } Main.renderCount = 0; Main.renderNow = false; } }` |
| 3974 | property | Terraria.Lighting | Terraria/Lighting.cs | D:\TRbackup\Version4\Terraria\Lighting.cs | 63 | 2 | UsingNewLighting | bool | `public static bool UsingNewLighting => Mode == LightMode.Color;` | `public static bool UsingNewLighting => Mode == LightMode.Color;` |


### 4.15 细分子系统：`SharedLegacyLightingState`

- 原报告章节：`4.9.131`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`18` / `地图、相机与绘制`

- 上一级基线细分子系统：`LightingEngineState`
- 细分职责：旧光照扫描、临时光源和旧光照图状态。
- 边界角色：`state/query`；最小 seam：legacy lighting port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：3；字段：19；属性：1；合计：20。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（19）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2645 | field | Terraria.Graphics.Light.LegacyLighting.LightingSwipeData | Terraria.Graphics.Light/LegacyLighting.cs | D:\TRbackup\Version4\Terraria.Graphics.Light\LegacyLighting.cs | 16 | 3 | InnerLoop1Start | int | `public int InnerLoop1Start;` | `public int InnerLoop1Start;` |
| 2646 | field | Terraria.Graphics.Light.LegacyLighting.LightingSwipeData | Terraria.Graphics.Light/LegacyLighting.cs | D:\TRbackup\Version4\Terraria.Graphics.Light\LegacyLighting.cs | 18 | 3 | InnerLoop1End | int | `public int InnerLoop1End;` | `public int InnerLoop1End;` |
| 2647 | field | Terraria.Graphics.Light.LegacyLighting.LightingSwipeData | Terraria.Graphics.Light/LegacyLighting.cs | D:\TRbackup\Version4\Terraria.Graphics.Light\LegacyLighting.cs | 20 | 3 | InnerLoop2Start | int | `public int InnerLoop2Start;` | `public int InnerLoop2Start;` |
| 2648 | field | Terraria.Graphics.Light.LegacyLighting.LightingSwipeData | Terraria.Graphics.Light/LegacyLighting.cs | D:\TRbackup\Version4\Terraria.Graphics.Light\LegacyLighting.cs | 22 | 3 | InnerLoop2End | int | `public int InnerLoop2End;` | `public int InnerLoop2End;` |
| 2649 | field | Terraria.Graphics.Light.LegacyLighting.LightingSwipeData | Terraria.Graphics.Light/LegacyLighting.cs | D:\TRbackup\Version4\Terraria.Graphics.Light\LegacyLighting.cs | 24 | 3 | JaggedArray | Terraria.Graphics.Light.LegacyLighting.LightingState[][] | `public LightingState[][] JaggedArray;` | `public LightingState[][] JaggedArray;` |
| 2650 | field | Terraria.Graphics.Light.LegacyLighting.LightingState | Terraria.Graphics.Light/LegacyLighting.cs | D:\TRbackup\Version4\Terraria.Graphics.Light\LegacyLighting.cs | 37 | 3 | R | float | `public float R;` | `public float R;` |
| 2651 | field | Terraria.Graphics.Light.LegacyLighting.LightingState | Terraria.Graphics.Light/LegacyLighting.cs | D:\TRbackup\Version4\Terraria.Graphics.Light\LegacyLighting.cs | 39 | 3 | R2 | float | `public float R2;` | `public float R2;` |
| 2652 | field | Terraria.Graphics.Light.LegacyLighting.LightingState | Terraria.Graphics.Light/LegacyLighting.cs | D:\TRbackup\Version4\Terraria.Graphics.Light\LegacyLighting.cs | 41 | 3 | G | float | `public float G;` | `public float G;` |
| 2653 | field | Terraria.Graphics.Light.LegacyLighting.LightingState | Terraria.Graphics.Light/LegacyLighting.cs | D:\TRbackup\Version4\Terraria.Graphics.Light\LegacyLighting.cs | 43 | 3 | G2 | float | `public float G2;` | `public float G2;` |
| 2654 | field | Terraria.Graphics.Light.LegacyLighting.LightingState | Terraria.Graphics.Light/LegacyLighting.cs | D:\TRbackup\Version4\Terraria.Graphics.Light\LegacyLighting.cs | 45 | 3 | B | float | `public float B;` | `public float B;` |
| 2655 | field | Terraria.Graphics.Light.LegacyLighting.LightingState | Terraria.Graphics.Light/LegacyLighting.cs | D:\TRbackup\Version4\Terraria.Graphics.Light\LegacyLighting.cs | 47 | 3 | B2 | float | `public float B2;` | `public float B2;` |
| 2656 | field | Terraria.Graphics.Light.LegacyLighting.LightingState | Terraria.Graphics.Light/LegacyLighting.cs | D:\TRbackup\Version4\Terraria.Graphics.Light\LegacyLighting.cs | 49 | 3 | CrackedLight | bool | `public bool CrackedLight;` | `public bool CrackedLight;` |
| 2657 | field | Terraria.Graphics.Light.LegacyLighting.LightingState | Terraria.Graphics.Light/LegacyLighting.cs | D:\TRbackup\Version4\Terraria.Graphics.Light\LegacyLighting.cs | 51 | 3 | StopLight | bool | `public bool StopLight;` | `public bool StopLight;` |
| 2658 | field | Terraria.Graphics.Light.LegacyLighting.LightingState | Terraria.Graphics.Light/LegacyLighting.cs | D:\TRbackup\Version4\Terraria.Graphics.Light\LegacyLighting.cs | 53 | 3 | WetLight | bool | `public bool WetLight;` | `public bool WetLight;` |
| 2659 | field | Terraria.Graphics.Light.LegacyLighting.LightingState | Terraria.Graphics.Light/LegacyLighting.cs | D:\TRbackup\Version4\Terraria.Graphics.Light\LegacyLighting.cs | 55 | 3 | HoneyLight | bool | `public bool HoneyLight;` | `public bool HoneyLight;` |
| 2660 | field | Terraria.Graphics.Light.LegacyLighting | Terraria.Graphics.Light/LegacyLighting.cs | D:\TRbackup\Version4\Terraria.Graphics.Light\LegacyLighting.cs | 58 | 2 | MAX_TEMP_LIGHTS | int | `private const int MAX_TEMP_LIGHTS = 2000;` | `private const int MAX_TEMP_LIGHTS = 2000;` |
| 2661 | field | Terraria.Graphics.Light.LegacyLighting | Terraria.Graphics.Light/LegacyLighting.cs | D:\TRbackup\Version4\Terraria.Graphics.Light\LegacyLighting.cs | 60 | 2 | _tileScanner | Terraria.Graphics.Light.TileLightScanner | `private TileLightScanner _tileScanner = new TileLightScanner();` | `private TileLightScanner _tileScanner = new TileLightScanner();` |
| 2662 | field | Terraria.Graphics.Light.LegacyLighting | Terraria.Graphics.Light/LegacyLighting.cs | D:\TRbackup\Version4\Terraria.Graphics.Light\LegacyLighting.cs | 64 | 2 | _swipeRandom | Terraria.Utilities.FastRandom | `private static FastRandom _swipeRandom = FastRandom.CreateWithRandomSeed();` | `private static FastRandom _swipeRandom = FastRandom.CreateWithRandomSeed();` |
| 2663 | field | Terraria.Graphics.Light.LegacyLighting | Terraria.Graphics.Light/LegacyLighting.cs | D:\TRbackup\Version4\Terraria.Graphics.Light\LegacyLighting.cs | 66 | 2 | _lightMap | Terraria.Graphics.Light.LightMap | `private LightMap _lightMap = new LightMap();` | `private LightMap _lightMap = new LightMap();` |

#### 属性（1）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3875 | property | Terraria.Graphics.Light.LegacyLighting | Terraria.Graphics.Light/LegacyLighting.cs | D:\TRbackup\Version4\Terraria.Graphics.Light\LegacyLighting.cs | 68 | 2 | Mode | int | `public int Mode { get; set; }` | `public int Mode { get; set; }` |


### 4.16 细分子系统：`SharedSceneScanAccumulatorState`

- 原报告章节：`4.9.171`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`18` / `地图、相机与绘制`

- 上一级基线细分子系统：`SharedSceneMetricsSnapshot`
- 上一级 peer 细分子系统：`SharedSceneScanAndZoneState`
- 细分职责：场景扫描计数、位置快照、最近实体和扫描缓存。
- 边界角色：`query/cache`；最小 seam：scene scan accumulator port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：8；属性：5；合计：13。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（8）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3433 | field | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 20 | 2 | BestOreType | int | `public int BestOreType;` | `public int BestOreType;` |
| 3468 | field | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 90 | 2 | CanPlayCreditsRoll | bool | `public bool CanPlayCreditsRoll;` | `public bool CanPlayCreditsRoll;` |
| 3469 | field | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 92 | 2 | NPCBannerBuff | bool[] | `public bool[] NPCBannerBuff = new bool[BannerSystem.MaxBannerTypes];` | `public bool[] NPCBannerBuff = new bool[BannerSystem.MaxBannerTypes];` |
| 3470 | field | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 94 | 2 | hasBanner | bool | `public bool hasBanner;` | `public bool hasBanner;` |
| 3471 | field | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 96 | 2 | ClosestNPCPosition | Vector2[] | `public Vector2[] ClosestNPCPosition = new Vector2[NPCID.Count];` | `public Vector2[] ClosestNPCPosition = new Vector2[NPCID.Count];` |
| 3472 | field | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 98 | 2 | _dummyPlayer | Terraria.Player | `private static Player _dummyPlayer = new Player();` | `private static Player _dummyPlayer = new Player();` |
| 3473 | field | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 100 | 2 | _tileCounts | int[] | `private readonly int[] _tileCounts = new int[TileID.Count];` | `private readonly int[] _tileCounts = new int[TileID.Count];` |
| 3474 | field | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 102 | 2 | _liquidCounts | int[] | `private readonly int[] _liquidCounts = new int[LiquidID.Count];` | `private readonly int[] _liquidCounts = new int[LiquidID.Count];` |

#### 属性（5）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3979 | property | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 104 | 2 | LastScanTime | uint | `public uint LastScanTime { get; private set; }` | `public uint LastScanTime { get; private set; }` |
| 3980 | property | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 106 | 2 | Center | Vector2 | `public Vector2 Center { get; private set; }` | `public Vector2 Center { get; private set; }` |
| 3981 | property | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 108 | 2 | TileCenter | Point | `public Point TileCenter { get; private set; }` | `public Point TileCenter { get; private set; }` |
| 3982 | property | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 110 | 2 | BestOrePosition | Point | `public Point BestOrePosition { get; private set; }` | `public Point BestOrePosition { get; private set; }` |
| 4022 | property | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 190 | 2 | PerspectivePlayer | Terraria.Player | `public Player PerspectivePlayer { get; private set; }` | `public Player PerspectivePlayer { get; private set; }` |


### 4.17 细分子系统：`SharedSceneTileAggregateState`

- 原报告章节：`4.9.175`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`18` / `地图、相机与绘制`

- 上一级基线细分子系统：`SharedSceneMetricsSnapshot`
- 上一级 peer 细分子系统：`SharedSceneAggregateAndDecorationState`
- 细分职责：场景 Tile、液体、实体和资源聚合计数。
- 边界角色：`query/cache`；最小 seam：scene tile aggregate port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：0；属性：19；合计：19。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（0）

无该类型成员记录。

#### 属性（19）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3983 | property | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 112 | 2 | ShimmerTileCount | int | `public int ShimmerTileCount { get; set; }` | `public int ShimmerTileCount { get; set; }` |
| 3984 | property | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 114 | 2 | EvilTileCount | int | `public int EvilTileCount { get; set; }` | `public int EvilTileCount { get; set; }` |
| 3985 | property | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 116 | 2 | HolyTileCount | int | `public int HolyTileCount { get; set; }` | `public int HolyTileCount { get; set; }` |
| 3986 | property | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 118 | 2 | HoneyBlockCount | int | `public int HoneyBlockCount { get; set; }` | `public int HoneyBlockCount { get; set; }` |
| 3989 | property | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 124 | 2 | SandTileCount | int | `public int SandTileCount { get; private set; }` | `public int SandTileCount { get; private set; }` |
| 3990 | property | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 126 | 2 | MushroomTileCount | int | `public int MushroomTileCount { get; private set; }` | `public int MushroomTileCount { get; private set; }` |
| 3991 | property | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 128 | 2 | SnowTileCount | int | `public int SnowTileCount { get; private set; }` | `public int SnowTileCount { get; private set; }` |
| 3992 | property | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 130 | 2 | WaterCandleCount | int | `public int WaterCandleCount { get; private set; }` | `public int WaterCandleCount { get; private set; }` |
| 3993 | property | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 132 | 2 | PeaceCandleCount | int | `public int PeaceCandleCount { get; private set; }` | `public int PeaceCandleCount { get; private set; }` |
| 3994 | property | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 134 | 2 | ShadowCandleCount | int | `public int ShadowCandleCount { get; private set; }` | `public int ShadowCandleCount { get; private set; }` |
| 3995 | property | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 136 | 2 | PartyMonolithCount | int | `public int PartyMonolithCount { get; private set; }` | `public int PartyMonolithCount { get; private set; }` |
| 3996 | property | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 138 | 2 | MeteorTileCount | int | `public int MeteorTileCount { get; private set; }` | `public int MeteorTileCount { get; private set; }` |
| 3997 | property | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 140 | 2 | BloodTileCount | int | `public int BloodTileCount { get; private set; }` | `public int BloodTileCount { get; private set; }` |
| 3998 | property | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 142 | 2 | JungleTileCount | int | `public int JungleTileCount { get; private set; }` | `public int JungleTileCount { get; private set; }` |
| 3999 | property | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 144 | 2 | DungeonTileCount | int | `public int DungeonTileCount { get; private set; }` | `public int DungeonTileCount { get; private set; }` |
| 4017 | property | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 180 | 2 | GraveyardTileCount | int | `public int GraveyardTileCount { get; private set; }` | `public int GraveyardTileCount { get; private set; }` |
| 4018 | property | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 182 | 2 | DesertSandTileCount | int | `public int DesertSandTileCount { get; private set; }` | `public int DesertSandTileCount { get; private set; }` |
| 4019 | property | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 184 | 2 | OceanSandTileCount | int | `public int OceanSandTileCount { get; private set; }` | `public int OceanSandTileCount { get; private set; }` |
| 4021 | property | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 188 | 2 | TownNPCCount | int | `public int TownNPCCount { get; private set; }` | `public int TownNPCCount { get; private set; }` |


### 4.18 细分子系统：`SharedShaderFamilyDataState`

- 原报告章节：`4.9.182`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`18` / `地图、相机与绘制`

- 上一级基线细分子系统：`SharedShaderData`
- 上一级 peer 细分子系统：`SharedShaderFamilyCatalogState`
- 细分职责：Hair、Misc、Armor 等 shader 家族数据和参数。
- 边界角色：`presentation`；最小 seam：shader family data port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：3；声明类型数：3；字段：19；属性：1；合计：20。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（19）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2685 | field | Terraria.Graphics.Shaders.ArmorShaderData | Terraria.Graphics.Shaders/ArmorShaderData.cs | D:\TRbackup\Version4\Terraria.Graphics.Shaders\ArmorShaderData.cs | 10 | 2 | _uColor | Vector3 | `private Vector3 _uColor = Vector3.One;` | `private Vector3 _uColor = Vector3.One;` |
| 2686 | field | Terraria.Graphics.Shaders.ArmorShaderData | Terraria.Graphics.Shaders/ArmorShaderData.cs | D:\TRbackup\Version4\Terraria.Graphics.Shaders\ArmorShaderData.cs | 12 | 2 | _uSecondaryColor | Vector3 | `private Vector3 _uSecondaryColor = Vector3.One;` | `private Vector3 _uSecondaryColor = Vector3.One;` |
| 2687 | field | Terraria.Graphics.Shaders.ArmorShaderData | Terraria.Graphics.Shaders/ArmorShaderData.cs | D:\TRbackup\Version4\Terraria.Graphics.Shaders\ArmorShaderData.cs | 14 | 2 | _uSaturation | float | `private float _uSaturation = 1f;` | `private float _uSaturation = 1f;` |
| 2688 | field | Terraria.Graphics.Shaders.ArmorShaderData | Terraria.Graphics.Shaders/ArmorShaderData.cs | D:\TRbackup\Version4\Terraria.Graphics.Shaders\ArmorShaderData.cs | 16 | 2 | _uTargetPosition | Vector2 | `private Vector2 _uTargetPosition = Vector2.One;` | `private Vector2 _uTargetPosition = Vector2.One;` |
| 2695 | field | Terraria.Graphics.Shaders.HairShaderData | Terraria.Graphics.Shaders/HairShaderData.cs | D:\TRbackup\Version4\Terraria.Graphics.Shaders\HairShaderData.cs | 10 | 2 | _uColor | Vector3 | `protected Vector3 _uColor = Vector3.One;` | `protected Vector3 _uColor = Vector3.One;` |
| 2696 | field | Terraria.Graphics.Shaders.HairShaderData | Terraria.Graphics.Shaders/HairShaderData.cs | D:\TRbackup\Version4\Terraria.Graphics.Shaders\HairShaderData.cs | 12 | 2 | _uSecondaryColor | Vector3 | `protected Vector3 _uSecondaryColor = Vector3.One;` | `protected Vector3 _uSecondaryColor = Vector3.One;` |
| 2697 | field | Terraria.Graphics.Shaders.HairShaderData | Terraria.Graphics.Shaders/HairShaderData.cs | D:\TRbackup\Version4\Terraria.Graphics.Shaders\HairShaderData.cs | 14 | 2 | _uSaturation | float | `protected float _uSaturation = 1f;` | `protected float _uSaturation = 1f;` |
| 2698 | field | Terraria.Graphics.Shaders.HairShaderData | Terraria.Graphics.Shaders/HairShaderData.cs | D:\TRbackup\Version4\Terraria.Graphics.Shaders\HairShaderData.cs | 16 | 2 | _uOpacity | float | `protected float _uOpacity = 1f;` | `protected float _uOpacity = 1f;` |
| 2699 | field | Terraria.Graphics.Shaders.HairShaderData | Terraria.Graphics.Shaders/HairShaderData.cs | D:\TRbackup\Version4\Terraria.Graphics.Shaders\HairShaderData.cs | 18 | 2 | _uImage | Asset<Texture2D> | `protected Asset<Texture2D> _uImage;` | `protected Asset<Texture2D> _uImage;` |
| 2700 | field | Terraria.Graphics.Shaders.HairShaderData | Terraria.Graphics.Shaders/HairShaderData.cs | D:\TRbackup\Version4\Terraria.Graphics.Shaders\HairShaderData.cs | 20 | 2 | _shaderDisabled | bool | `protected bool _shaderDisabled;` | `protected bool _shaderDisabled;` |
| 2701 | field | Terraria.Graphics.Shaders.HairShaderData | Terraria.Graphics.Shaders/HairShaderData.cs | D:\TRbackup\Version4\Terraria.Graphics.Shaders\HairShaderData.cs | 22 | 2 | _uTargetPosition | Vector2 | `private Vector2 _uTargetPosition = Vector2.One;` | `private Vector2 _uTargetPosition = Vector2.One;` |
| 2705 | field | Terraria.Graphics.Shaders.MiscShaderData | Terraria.Graphics.Shaders/MiscShaderData.cs | D:\TRbackup\Version4\Terraria.Graphics.Shaders\MiscShaderData.cs | 11 | 2 | _uColor | Vector3 | `private Vector3 _uColor = Vector3.One;` | `private Vector3 _uColor = Vector3.One;` |
| 2706 | field | Terraria.Graphics.Shaders.MiscShaderData | Terraria.Graphics.Shaders/MiscShaderData.cs | D:\TRbackup\Version4\Terraria.Graphics.Shaders\MiscShaderData.cs | 13 | 2 | _uSecondaryColor | Vector3 | `private Vector3 _uSecondaryColor = Vector3.One;` | `private Vector3 _uSecondaryColor = Vector3.One;` |
| 2707 | field | Terraria.Graphics.Shaders.MiscShaderData | Terraria.Graphics.Shaders/MiscShaderData.cs | D:\TRbackup\Version4\Terraria.Graphics.Shaders\MiscShaderData.cs | 15 | 2 | _uImage0 | Asset<Texture2D> | `private Asset<Texture2D> _uImage0;` | `private Asset<Texture2D> _uImage0;` |
| 2708 | field | Terraria.Graphics.Shaders.MiscShaderData | Terraria.Graphics.Shaders/MiscShaderData.cs | D:\TRbackup\Version4\Terraria.Graphics.Shaders\MiscShaderData.cs | 17 | 2 | _uImage1 | Asset<Texture2D> | `private Asset<Texture2D> _uImage1;` | `private Asset<Texture2D> _uImage1;` |
| 2709 | field | Terraria.Graphics.Shaders.MiscShaderData | Terraria.Graphics.Shaders/MiscShaderData.cs | D:\TRbackup\Version4\Terraria.Graphics.Shaders\MiscShaderData.cs | 19 | 2 | _uImage2 | Asset<Texture2D> | `private Asset<Texture2D> _uImage2;` | `private Asset<Texture2D> _uImage2;` |
| 2710 | field | Terraria.Graphics.Shaders.MiscShaderData | Terraria.Graphics.Shaders/MiscShaderData.cs | D:\TRbackup\Version4\Terraria.Graphics.Shaders\MiscShaderData.cs | 21 | 2 | _useProjectionMatrix | bool | `private bool _useProjectionMatrix;` | `private bool _useProjectionMatrix;` |
| 2711 | field | Terraria.Graphics.Shaders.MiscShaderData | Terraria.Graphics.Shaders/MiscShaderData.cs | D:\TRbackup\Version4\Terraria.Graphics.Shaders\MiscShaderData.cs | 23 | 2 | _shaderSpecificData | Vector4 | `private Vector4 _shaderSpecificData = Vector4.Zero;` | `private Vector4 _shaderSpecificData = Vector4.Zero;` |
| 2712 | field | Terraria.Graphics.Shaders.MiscShaderData | Terraria.Graphics.Shaders/MiscShaderData.cs | D:\TRbackup\Version4\Terraria.Graphics.Shaders\MiscShaderData.cs | 25 | 2 | _customSamplerState | SamplerState | `private SamplerState _customSamplerState;` | `private SamplerState _customSamplerState;` |

#### 属性（1）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3886 | property | Terraria.Graphics.Shaders.HairShaderData | Terraria.Graphics.Shaders/HairShaderData.cs | D:\TRbackup\Version4\Terraria.Graphics.Shaders\HairShaderData.cs | 24 | 2 | ShaderDisabled | bool | `public bool ShaderDisabled => _shaderDisabled;` | `public bool ShaderDisabled => _shaderDisabled;` |


### 4.19 细分子系统：`SharedShaderRegistryAndLookupState`

- 原报告章节：`4.9.183`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`18` / `地图、相机与绘制`

- 上一级基线细分子系统：`SharedShaderData`
- 上一级 peer 细分子系统：`SharedShaderFamilyCatalogState`
- 细分职责：GameShaders、ShaderDataSet 注册、索引和查找状态。
- 边界角色：`presentation/query`；最小 seam：shader registry lookup port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：3；声明类型数：3；字段：9；属性：0；合计：9。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（9）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2689 | field | Terraria.Graphics.Shaders.ArmorShaderDataSet | Terraria.Graphics.Shaders/ArmorShaderDataSet.cs | D:\TRbackup\Version4\Terraria.Graphics.Shaders\ArmorShaderDataSet.cs | 8 | 2 | _shaderData | System.Collections.Generic.List<Terraria.Graphics.Shaders.ArmorShaderData> | `protected List<ArmorShaderData> _shaderData = new List<ArmorShaderData>();` | `protected List<ArmorShaderData> _shaderData = new List<ArmorShaderData>();` |
| 2690 | field | Terraria.Graphics.Shaders.ArmorShaderDataSet | Terraria.Graphics.Shaders/ArmorShaderDataSet.cs | D:\TRbackup\Version4\Terraria.Graphics.Shaders\ArmorShaderDataSet.cs | 10 | 2 | _shaderLookupDictionary | System.Collections.Generic.Dictionary<int, int> | `protected Dictionary<int, int> _shaderLookupDictionary = new Dictionary<int, int>();` | `protected Dictionary<int, int> _shaderLookupDictionary = new Dictionary<int, int>();` |
| 2691 | field | Terraria.Graphics.Shaders.ArmorShaderDataSet | Terraria.Graphics.Shaders/ArmorShaderDataSet.cs | D:\TRbackup\Version4\Terraria.Graphics.Shaders\ArmorShaderDataSet.cs | 12 | 2 | _shaderDataCount | int | `protected int _shaderDataCount;` | `protected int _shaderDataCount;` |
| 2692 | field | Terraria.Graphics.Shaders.GameShaders | Terraria.Graphics.Shaders/GameShaders.cs | D:\TRbackup\Version4\Terraria.Graphics.Shaders\GameShaders.cs | 7 | 2 | Armor | Terraria.Graphics.Shaders.ArmorShaderDataSet | `public static ArmorShaderDataSet Armor = new ArmorShaderDataSet();` | `public static ArmorShaderDataSet Armor = new ArmorShaderDataSet();` |
| 2693 | field | Terraria.Graphics.Shaders.GameShaders | Terraria.Graphics.Shaders/GameShaders.cs | D:\TRbackup\Version4\Terraria.Graphics.Shaders\GameShaders.cs | 9 | 2 | Hair | Terraria.Graphics.Shaders.HairShaderDataSet | `public static HairShaderDataSet Hair = new HairShaderDataSet();` | `public static HairShaderDataSet Hair = new HairShaderDataSet();` |
| 2694 | field | Terraria.Graphics.Shaders.GameShaders | Terraria.Graphics.Shaders/GameShaders.cs | D:\TRbackup\Version4\Terraria.Graphics.Shaders\GameShaders.cs | 11 | 2 | Misc | System.Collections.Generic.Dictionary<string, Terraria.Graphics.Shaders.MiscShaderData> | `public static Dictionary<string, MiscShaderData> Misc = new Dictionary<string, MiscShaderData>();` | `public static Dictionary<string, MiscShaderData> Misc = new Dictionary<string, MiscShaderData>();` |
| 2702 | field | Terraria.Graphics.Shaders.HairShaderDataSet | Terraria.Graphics.Shaders/HairShaderDataSet.cs | D:\TRbackup\Version4\Terraria.Graphics.Shaders\HairShaderDataSet.cs | 10 | 2 | _shaderData | System.Collections.Generic.List<Terraria.Graphics.Shaders.HairShaderData> | `protected List<HairShaderData> _shaderData = new List<HairShaderData>();` | `protected List<HairShaderData> _shaderData = new List<HairShaderData>();` |
| 2703 | field | Terraria.Graphics.Shaders.HairShaderDataSet | Terraria.Graphics.Shaders/HairShaderDataSet.cs | D:\TRbackup\Version4\Terraria.Graphics.Shaders\HairShaderDataSet.cs | 12 | 2 | _shaderLookupDictionary | System.Collections.Generic.Dictionary<int, short> | `protected Dictionary<int, short> _shaderLookupDictionary = new Dictionary<int, short>();` | `protected Dictionary<int, short> _shaderLookupDictionary = new Dictionary<int, short>();` |
| 2704 | field | Terraria.Graphics.Shaders.HairShaderDataSet | Terraria.Graphics.Shaders/HairShaderDataSet.cs | D:\TRbackup\Version4\Terraria.Graphics.Shaders\HairShaderDataSet.cs | 14 | 2 | _shaderDataCount | byte | `protected byte _shaderDataCount;` | `protected byte _shaderDataCount;` |

#### 属性（0）

无该类型成员记录。


### 4.20 细分子系统：`SharedSceneZoneGeometryAndThresholdState`

- 原报告章节：`4.9.203`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`18` / `地图、相机与绘制`

- 上一级基线细分子系统：`SharedSceneMetricsSnapshot`
- 上一级 peer 细分子系统：`SharedSceneZoneDefinitionState`
- 细分职责：场景扫描窗口、层高、阈值和区域几何输入。
- 边界角色：`query/input`；最小 seam：scene zone geometry threshold port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：12；属性：0；合计：12。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（12）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3429 | field | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 12 | 2 | AssumedConstantScreenSize | Point | `private static readonly Point AssumedConstantScreenSize = new Point(1920, 1200);` | `private static readonly Point AssumedConstantScreenSize = new Point(1920, 1200);` |
| 3430 | field | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 14 | 2 | ZoneScanPadding | int | `private static readonly int ZoneScanPadding = 25;` | `private static readonly int ZoneScanPadding = 25;` |
| 3431 | field | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 16 | 2 | ZoneScanSize | Point | `public static readonly Point ZoneScanSize = new Point(AssumedConstantScreenSize.X / 16 + ZoneScanPadding * 2 - 1, AssumedConstantScreenSize.Y / 16 + ZoneScanPadding * 2 - 1);` | `public static readonly Point ZoneScanSize = new Point(AssumedConstantScreenSize.X / 16 + ZoneScanPadding * 2 - 1, AssumedConstantScreenSize.Y / 16 + ZoneScanPadding * 2 - 1);` |
| 3432 | field | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 18 | 2 | TownNPCRectSize | Vector2 | `public static readonly Vector2 TownNPCRectSize = AssumedConstantScreenSize.ToVector2() * 2f;` | `public static readonly Vector2 TownNPCRectSize = AssumedConstantScreenSize.ToVector2() * 2f;` |
| 3434 | field | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 22 | 2 | SnowTileMax | int | `public static int SnowTileMax = 6000;` | `public static int SnowTileMax = 6000;` |
| 3435 | field | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 24 | 2 | MushroomTileThreshold | int | `public static int MushroomTileThreshold = 100;` | `public static int MushroomTileThreshold = 100;` |
| 3436 | field | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 26 | 2 | BelowSurface | bool | `public bool BelowSurface;` | `public bool BelowSurface;` |
| 3437 | field | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 28 | 2 | ZoneSkyHeight | bool | `public bool ZoneSkyHeight;` | `public bool ZoneSkyHeight;` |
| 3438 | field | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 30 | 2 | ZoneOverworldHeight | bool | `public bool ZoneOverworldHeight;` | `public bool ZoneOverworldHeight;` |
| 3439 | field | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 32 | 2 | ZoneDirtLayerHeight | bool | `public bool ZoneDirtLayerHeight;` | `public bool ZoneDirtLayerHeight;` |
| 3440 | field | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 34 | 2 | ZoneRockLayerHeight | bool | `public bool ZoneRockLayerHeight;` | `public bool ZoneRockLayerHeight;` |
| 3441 | field | Terraria.SceneMetrics | Terraria/SceneMetrics.cs | D:\TRbackup\Version4\Terraria\SceneMetrics.cs | 36 | 2 | ZoneUnderworldHeight | bool | `public bool ZoneUnderworldHeight;` | `public bool ZoneUnderworldHeight;` |

#### 属性（0）

无该类型成员记录。


### 4.21 细分子系统：`MapOverlayAndLayerPresentation`

- 原报告章节：`4.11.12`
- 父级子系统：`ClientPresentationAndTools`
- 分区工作包：`18` / `地图、相机与绘制`

- 上一级基线细分子系统：`MapOverlayAndLayerPresentation`
- 细分职责：地图覆盖层、Ping 和传送晶塔图层表现。
- 边界角色：`presentation`；最小 seam：map overlay layer view；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：3；声明类型数：4；字段：12；属性：0；合计：12。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（12）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 4250 | field | Terraria.Map.MapOverlayDrawContext | Terraria.Map/MapOverlayDrawContext.cs | D:\TRbackup\Version4\Terraria.Map\MapOverlayDrawContext.cs | 11 | 2 | _mapPosition | Vector2 | `private readonly Vector2 _mapPosition;` | `private readonly Vector2 _mapPosition;` |
| 4251 | field | Terraria.Map.MapOverlayDrawContext | Terraria.Map/MapOverlayDrawContext.cs | D:\TRbackup\Version4\Terraria.Map\MapOverlayDrawContext.cs | 13 | 2 | _mapOffset | Vector2 | `private readonly Vector2 _mapOffset;` | `private readonly Vector2 _mapOffset;` |
| 4252 | field | Terraria.Map.MapOverlayDrawContext | Terraria.Map/MapOverlayDrawContext.cs | D:\TRbackup\Version4\Terraria.Map\MapOverlayDrawContext.cs | 15 | 2 | _clippingRect | Rectangle? | `private readonly Rectangle? _clippingRect;` | `private readonly Rectangle? _clippingRect;` |
| 4253 | field | Terraria.Map.MapOverlayDrawContext | Terraria.Map/MapOverlayDrawContext.cs | D:\TRbackup\Version4\Terraria.Map\MapOverlayDrawContext.cs | 17 | 2 | _mapScale | float | `private readonly float _mapScale;` | `private readonly float _mapScale;` |
| 4254 | field | Terraria.Map.MapOverlayDrawContext | Terraria.Map/MapOverlayDrawContext.cs | D:\TRbackup\Version4\Terraria.Map\MapOverlayDrawContext.cs | 19 | 2 | _drawScale | float | `private readonly float _drawScale;` | `private readonly float _drawScale;` |
| 4255 | field | Terraria.Map.MapOverlayDrawContext | Terraria.Map/MapOverlayDrawContext.cs | D:\TRbackup\Version4\Terraria.Map\MapOverlayDrawContext.cs | 21 | 2 | _opacity | float | `private readonly float _opacity;` | `private readonly float _opacity;` |
| 4262 | field | Terraria.Map.PingMapLayer.Ping | Terraria.Map/PingMapLayer.cs | D:\TRbackup\Version4\Terraria.Map\PingMapLayer.cs | 15 | 3 | Position | Vector2 | `public readonly Vector2 Position;` | `public readonly Vector2 Position;` |
| 4263 | field | Terraria.Map.PingMapLayer.Ping | Terraria.Map/PingMapLayer.cs | D:\TRbackup\Version4\Terraria.Map\PingMapLayer.cs | 17 | 3 | Time | System.DateTime | `public readonly DateTime Time;` | `public readonly DateTime Time;` |
| 4264 | field | Terraria.Map.PingMapLayer | Terraria.Map/PingMapLayer.cs | D:\TRbackup\Version4\Terraria.Map\PingMapLayer.cs | 26 | 2 | PING_DURATION_IN_SECONDS | double | `private const double PING_DURATION_IN_SECONDS = 15.0;` | `private const double PING_DURATION_IN_SECONDS = 15.0;` |
| 4265 | field | Terraria.Map.PingMapLayer | Terraria.Map/PingMapLayer.cs | D:\TRbackup\Version4\Terraria.Map\PingMapLayer.cs | 28 | 2 | PING_FRAME_RATE | double | `private const double PING_FRAME_RATE = 10.0;` | `private const double PING_FRAME_RATE = 10.0;` |
| 4266 | field | Terraria.Map.PingMapLayer | Terraria.Map/PingMapLayer.cs | D:\TRbackup\Version4\Terraria.Map\PingMapLayer.cs | 30 | 2 | _pings | SlotVector<Terraria.Map.PingMapLayer.Ping> | `private readonly SlotVector<Ping> _pings = new SlotVector<Ping>(100);` | `private readonly SlotVector<Ping> _pings = new SlotVector<Ping>(100);` |
| 4267 | field | Terraria.Map.TeleportPylonsMapLayer | Terraria.Map/TeleportPylonsMapLayer.cs | D:\TRbackup\Version4\Terraria.Map\TeleportPylonsMapLayer.cs | 16 | 2 | BorderSize | int | `public const int BorderSize = 10;` | `public const int BorderSize = 10;` |

#### 属性（0）

无该类型成员记录。


### 4.22 细分子系统：`WorldMapState`

- 原报告章节：`4.11.13`
- 父级子系统：`ClientPresentationAndTools`
- 分区工作包：`18` / `地图、相机与绘制`

- 上一级基线细分子系统：`WorldMapState`
- 细分职责：世界地图尺寸和地图持久状态。
- 边界角色：`presentation/state`；最小 seam：world map storage port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：4；属性：1；合计：5。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（4）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 4268 | field | Terraria.Map.WorldMap | Terraria.Map/WorldMap.cs | D:\TRbackup\Version4\Terraria.Map\WorldMap.cs | 12 | 2 | MaxWidth | int | `public readonly int MaxWidth;` | `public readonly int MaxWidth;` |
| 4269 | field | Terraria.Map.WorldMap | Terraria.Map/WorldMap.cs | D:\TRbackup\Version4\Terraria.Map\WorldMap.cs | 14 | 2 | MaxHeight | int | `public readonly int MaxHeight;` | `public readonly int MaxHeight;` |
| 4270 | field | Terraria.Map.WorldMap | Terraria.Map/WorldMap.cs | D:\TRbackup\Version4\Terraria.Map\WorldMap.cs | 16 | 2 | BlackEdgeWidth | int | `public const int BlackEdgeWidth = 40;` | `public const int BlackEdgeWidth = 40;` |
| 4271 | field | Terraria.Map.WorldMap | Terraria.Map/WorldMap.cs | D:\TRbackup\Version4\Terraria.Map\WorldMap.cs | 18 | 2 | _tiles | Terraria.Map.MapTile[,] | `private MapTile[,] _tiles;` | `private MapTile[,] _tiles;` |

#### 属性（1）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 4535 | property | Terraria.Map.WorldMap | Terraria.Map/WorldMap.cs | D:\TRbackup\Version4\Terraria.Map\WorldMap.cs | 20 | 2 | this[] | Terraria.Map.MapTile | `public MapTile this[int x, int y] => _tiles[x, y];` | `public MapTile this[int x, int y] => _tiles[x, y];` |


### 4.23 细分子系统：`MapTileUpdateQueueState`

- 原报告章节：`4.11.16`
- 父级子系统：`ClientPresentationAndTools`
- 分区工作包：`18` / `地图、相机与绘制`

- 上一级基线细分子系统：`MapTileStorageAndUpdateState`
- 细分职责：地图区域更新队列、锁和待处理更新状态。
- 边界角色：`presentation/state`；最小 seam：map tile update queue port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：3；属性：0；合计：3。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（3）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 4259 | field | Terraria.Map.MapUpdateQueue | Terraria.Map/MapUpdateQueue.cs | D:\TRbackup\Version4\Terraria.Map\MapUpdateQueue.cs | 11 | 2 | MAX_QUEUED_UPDATES | int | `private const int MAX_QUEUED_UPDATES = 262144;` | `private const int MAX_QUEUED_UPDATES = 262144;` |
| 4260 | field | Terraria.Map.MapUpdateQueue | Terraria.Map/MapUpdateQueue.cs | D:\TRbackup\Version4\Terraria.Map\MapUpdateQueue.cs | 13 | 2 | _areaUpdateQueue | System.Collections.Generic.List<Rectangle> | `private static List<Rectangle> _areaUpdateQueue = new List<Rectangle>();` | `private static List<Rectangle> _areaUpdateQueue = new List<Rectangle>();` |
| 4261 | field | Terraria.Map.MapUpdateQueue | Terraria.Map/MapUpdateQueue.cs | D:\TRbackup\Version4\Terraria.Map\MapUpdateQueue.cs | 15 | 2 | _lock | object | `private static readonly object _lock = new object();` | `private static readonly object _lock = new object();` |

#### 属性（0）

无该类型成员记录。


### 4.24 细分子系统：`CameraMatrixAndViewportState`

- 原报告章节：`4.11.19`
- 父级子系统：`ClientPresentationAndTools`
- 分区工作包：`18` / `地图、相机与绘制`

- 上一级基线细分子系统：`CameraAndVertexPresentation`
- 细分职责：相机视口、缩放、平移和图形变换矩阵状态。
- 边界角色：`presentation`；最小 seam：camera matrix viewport port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：2；声明类型数：2；字段：8；属性：8；合计：16。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（8）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 4194 | field | Terraria.Graphics.SpriteViewMatrix | Terraria.Graphics/SpriteViewMatrix.cs | D:\TRbackup\Version4\Terraria.Graphics\SpriteViewMatrix.cs | 8 | 2 | _zoom | Vector2 | `private Vector2 _zoom = Vector2.One;` | `private Vector2 _zoom = Vector2.One;` |
| 4195 | field | Terraria.Graphics.SpriteViewMatrix | Terraria.Graphics/SpriteViewMatrix.cs | D:\TRbackup\Version4\Terraria.Graphics\SpriteViewMatrix.cs | 10 | 2 | _translation | Vector2 | `private Vector2 _translation = Vector2.Zero;` | `private Vector2 _translation = Vector2.Zero;` |
| 4196 | field | Terraria.Graphics.SpriteViewMatrix | Terraria.Graphics/SpriteViewMatrix.cs | D:\TRbackup\Version4\Terraria.Graphics\SpriteViewMatrix.cs | 12 | 2 | _zoomMatrix | Matrix | `private Matrix _zoomMatrix = Matrix.Identity;` | `private Matrix _zoomMatrix = Matrix.Identity;` |
| 4197 | field | Terraria.Graphics.SpriteViewMatrix | Terraria.Graphics/SpriteViewMatrix.cs | D:\TRbackup\Version4\Terraria.Graphics\SpriteViewMatrix.cs | 14 | 2 | _transformationMatrix | Matrix | `private Matrix _transformationMatrix = Matrix.Identity;` | `private Matrix _transformationMatrix = Matrix.Identity;` |
| 4198 | field | Terraria.Graphics.SpriteViewMatrix | Terraria.Graphics/SpriteViewMatrix.cs | D:\TRbackup\Version4\Terraria.Graphics\SpriteViewMatrix.cs | 16 | 2 | _normalizedTransformationMatrix | Matrix | `private Matrix _normalizedTransformationMatrix = Matrix.Identity;` | `private Matrix _normalizedTransformationMatrix = Matrix.Identity;` |
| 4199 | field | Terraria.Graphics.SpriteViewMatrix | Terraria.Graphics/SpriteViewMatrix.cs | D:\TRbackup\Version4\Terraria.Graphics\SpriteViewMatrix.cs | 22 | 2 | _graphicsDevice | GraphicsDevice | `private GraphicsDevice _graphicsDevice;` | `private GraphicsDevice _graphicsDevice;` |
| 4200 | field | Terraria.Graphics.SpriteViewMatrix | Terraria.Graphics/SpriteViewMatrix.cs | D:\TRbackup\Version4\Terraria.Graphics\SpriteViewMatrix.cs | 24 | 2 | PixelPerfectOffset | float | `private const float PixelPerfectOffset = 0.00390625f;` | `private const float PixelPerfectOffset = 0.00390625f;` |
| 4201 | field | Terraria.Graphics.SpriteViewMatrix | Terraria.Graphics/SpriteViewMatrix.cs | D:\TRbackup\Version4\Terraria.Graphics\SpriteViewMatrix.cs | 26 | 2 | PixelPerfectSafeZoomLevelStep | float | `private const float PixelPerfectSafeZoomLevelStep = 1f / 128f;` | `private const float PixelPerfectSafeZoomLevelStep = 1f / 128f;` |

#### 属性（8）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 4523 | property | Terraria.Graphics.Camera | Terraria.Graphics/Camera.cs | D:\TRbackup\Version4\Terraria.Graphics\Camera.cs | 8 | 2 | UnscaledPosition | Vector2 | `public Vector2 UnscaledPosition => Main.screenPosition;` | `public Vector2 UnscaledPosition => Main.screenPosition;` |
| 4524 | property | Terraria.Graphics.Camera | Terraria.Graphics/Camera.cs | D:\TRbackup\Version4\Terraria.Graphics\Camera.cs | 10 | 2 | UnscaledSize | Vector2 | `public Vector2 UnscaledSize => new Vector2(Main.screenWidth, Main.screenHeight);` | `public Vector2 UnscaledSize => new Vector2(Main.screenWidth, Main.screenHeight);` |
| 4525 | property | Terraria.Graphics.Camera | Terraria.Graphics/Camera.cs | D:\TRbackup\Version4\Terraria.Graphics\Camera.cs | 12 | 2 | ScaledPosition | Vector2 | `public Vector2 ScaledPosition => UnscaledPosition + GameViewMatrix.Translation;` | `public Vector2 ScaledPosition => UnscaledPosition + GameViewMatrix.Translation;` |
| 4526 | property | Terraria.Graphics.Camera | Terraria.Graphics/Camera.cs | D:\TRbackup\Version4\Terraria.Graphics\Camera.cs | 14 | 2 | ScaledSize | Vector2 | `public Vector2 ScaledSize => UnscaledSize - GameViewMatrix.Translation * 2f;` | `public Vector2 ScaledSize => UnscaledSize - GameViewMatrix.Translation * 2f;` |
| 4527 | property | Terraria.Graphics.Camera | Terraria.Graphics/Camera.cs | D:\TRbackup\Version4\Terraria.Graphics\Camera.cs | 46 | 2 | GameViewMatrix | Terraria.Graphics.SpriteViewMatrix | `public SpriteViewMatrix GameViewMatrix => Main.GameViewMatrix;` | `public SpriteViewMatrix GameViewMatrix => Main.GameViewMatrix;` |
| 4528 | property | Terraria.Graphics.Camera | Terraria.Graphics/Camera.cs | D:\TRbackup\Version4\Terraria.Graphics\Camera.cs | 50 | 2 | Center | Vector2 | `public Vector2 Center => UnscaledPosition + UnscaledSize * 0.5f;` | `public Vector2 Center => UnscaledPosition + UnscaledSize * 0.5f;` |
| 4529 | property | Terraria.Graphics.SpriteViewMatrix | Terraria.Graphics/SpriteViewMatrix.cs | D:\TRbackup\Version4\Terraria.Graphics\SpriteViewMatrix.cs | 28 | 2 | Translation | Vector2 | `public Vector2 Translation { get { if (ShouldRebuild()) { Rebuild(); } return _translation; } }` | `public Vector2 Translation { get { if (ShouldRebuild()) { Rebuild(); } return _translation; } }` |
| 4530 | property | Terraria.Graphics.SpriteViewMatrix | Terraria.Graphics/SpriteViewMatrix.cs | D:\TRbackup\Version4\Terraria.Graphics\SpriteViewMatrix.cs | 40 | 2 | TransformationMatrix | Matrix | `public Matrix TransformationMatrix { get { if (ShouldRebuild()) { Rebuild(); } return _transformationMatrix; } }` | `public Matrix TransformationMatrix { get { if (ShouldRebuild()) { Rebuild(); } return _transformationMatrix; } }` |


### 4.25 细分子系统：`VertexStripAndColorState`

- 原报告章节：`4.11.20`
- 父级子系统：`ClientPresentationAndTools`
- 分区工作包：`18` / `地图、相机与绘制`

- 上一级基线细分子系统：`CameraAndVertexPresentation`
- 细分职责：顶点条、顶点声明、索引和顶点颜色状态。
- 边界角色：`presentation`；最小 seam：vertex strip color port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：2；声明类型数：3；字段：12；属性：1；合计：13。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（12）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 4202 | field | Terraria.Graphics.VertexColors | Terraria.Graphics/VertexColors.cs | D:\TRbackup\Version4\Terraria.Graphics\VertexColors.cs | 7 | 2 | TopLeftColor | Color | `public Color TopLeftColor;` | `public Color TopLeftColor;` |
| 4203 | field | Terraria.Graphics.VertexColors | Terraria.Graphics/VertexColors.cs | D:\TRbackup\Version4\Terraria.Graphics\VertexColors.cs | 9 | 2 | TopRightColor | Color | `public Color TopRightColor;` | `public Color TopRightColor;` |
| 4204 | field | Terraria.Graphics.VertexColors | Terraria.Graphics/VertexColors.cs | D:\TRbackup\Version4\Terraria.Graphics\VertexColors.cs | 11 | 2 | BottomLeftColor | Color | `public Color BottomLeftColor;` | `public Color BottomLeftColor;` |
| 4205 | field | Terraria.Graphics.VertexColors | Terraria.Graphics/VertexColors.cs | D:\TRbackup\Version4\Terraria.Graphics\VertexColors.cs | 13 | 2 | BottomRightColor | Color | `public Color BottomRightColor;` | `public Color BottomRightColor;` |
| 4206 | field | Terraria.Graphics.VertexStrip.CustomVertexInfo | Terraria.Graphics/VertexStrip.cs | D:\TRbackup\Version4\Terraria.Graphics\VertexStrip.cs | 17 | 3 | Position | Vector2 | `public Vector2 Position;` | `public Vector2 Position;` |
| 4207 | field | Terraria.Graphics.VertexStrip.CustomVertexInfo | Terraria.Graphics/VertexStrip.cs | D:\TRbackup\Version4\Terraria.Graphics\VertexStrip.cs | 19 | 3 | Color | Color | `public Color Color;` | `public Color Color;` |
| 4208 | field | Terraria.Graphics.VertexStrip.CustomVertexInfo | Terraria.Graphics/VertexStrip.cs | D:\TRbackup\Version4\Terraria.Graphics\VertexStrip.cs | 21 | 3 | TexCoord | Vector3 | `public Vector3 TexCoord;` | `public Vector3 TexCoord;` |
| 4209 | field | Terraria.Graphics.VertexStrip.CustomVertexInfo | Terraria.Graphics/VertexStrip.cs | D:\TRbackup\Version4\Terraria.Graphics\VertexStrip.cs | 23 | 3 | _vertexDeclaration | VertexDeclaration | `private static VertexDeclaration _vertexDeclaration = new VertexDeclaration(new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0), new VertexElement(8, VertexElementFormat.Color, VertexElementUsage.Color, 0), new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 0));` | `private static VertexDeclaration _vertexDeclaration = new VertexDeclaration(new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0), new VertexElement(8, VertexElementFormat.Color, VertexElementUsage.Color, 0), new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 0));` |
| 4210 | field | Terraria.Graphics.VertexStrip | Terraria.Graphics/VertexStrip.cs | D:\TRbackup\Version4\Terraria.Graphics\VertexStrip.cs | 35 | 2 | _vertices | Terraria.Graphics.VertexStrip.CustomVertexInfo[] | `private CustomVertexInfo[] _vertices = new CustomVertexInfo[1];` | `private CustomVertexInfo[] _vertices = new CustomVertexInfo[1];` |
| 4211 | field | Terraria.Graphics.VertexStrip | Terraria.Graphics/VertexStrip.cs | D:\TRbackup\Version4\Terraria.Graphics\VertexStrip.cs | 37 | 2 | _indices | short[] | `private short[] _indices = new short[1];` | `private short[] _indices = new short[1];` |
| 4212 | field | Terraria.Graphics.VertexStrip | Terraria.Graphics/VertexStrip.cs | D:\TRbackup\Version4\Terraria.Graphics\VertexStrip.cs | 39 | 2 | _temporaryPositionsCache | System.Collections.Generic.List<Vector2> | `private List<Vector2> _temporaryPositionsCache = new List<Vector2>();` | `private List<Vector2> _temporaryPositionsCache = new List<Vector2>();` |
| 4213 | field | Terraria.Graphics.VertexStrip | Terraria.Graphics/VertexStrip.cs | D:\TRbackup\Version4\Terraria.Graphics\VertexStrip.cs | 41 | 2 | _temporaryRotationsCache | System.Collections.Generic.List<float> | `private List<float> _temporaryRotationsCache = new List<float>();` | `private List<float> _temporaryRotationsCache = new List<float>();` |

#### 属性（1）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 4531 | property | Terraria.Graphics.VertexStrip.CustomVertexInfo | Terraria.Graphics/VertexStrip.cs | D:\TRbackup\Version4\Terraria.Graphics\VertexStrip.cs | 25 | 3 | VertexDeclaration | VertexDeclaration | `public VertexDeclaration VertexDeclaration => _vertexDeclaration;` | `public VertexDeclaration VertexDeclaration => _vertexDeclaration;` |


### 4.26 细分子系统：`MapTileCellState`

- 原报告章节：`4.11.24`
- 父级子系统：`ClientPresentationAndTools`
- 分区工作包：`18` / `地图、相机与绘制`

- 上一级基线细分子系统：`MapTileStorageAndUpdateState`
- 上一级 peer 细分子系统：`MapTileEncodingAndStorageState`
- 细分职责：单个地图 Tile 的类型、光照、颜色和更新标志。
- 边界角色：`snapshot state`；最小 seam：map tile cell snapshot port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：3；属性：3；合计：6。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（3）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 4256 | field | Terraria.Map.MapTile | Terraria.Map/MapTile.cs | D:\TRbackup\Version4\Terraria.Map\MapTile.cs | 5 | 2 | Type | ushort | `public ushort Type;` | `public ushort Type;` |
| 4257 | field | Terraria.Map.MapTile | Terraria.Map/MapTile.cs | D:\TRbackup\Version4\Terraria.Map\MapTile.cs | 7 | 2 | Light | byte | `public byte Light;` | `public byte Light;` |
| 4258 | field | Terraria.Map.MapTile | Terraria.Map/MapTile.cs | D:\TRbackup\Version4\Terraria.Map\MapTile.cs | 9 | 2 | _extraData | byte | `private byte _extraData;` | `private byte _extraData;` |

#### 属性（3）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 4532 | property | Terraria.Map.MapTile | Terraria.Map/MapTile.cs | D:\TRbackup\Version4\Terraria.Map\MapTile.cs | 11 | 2 | IsChanged | bool | `public bool IsChanged { get { return (_extraData & 0x80) != 0; } set { if (value) { _extraData \|= 128; } else { _extraData &= 127; } } }` | `public bool IsChanged { get { return (_extraData & 0x80) != 0; } set { if (value) { _extraData \|= 128; } else { _extraData &= 127; } } }` |
| 4533 | property | Terraria.Map.MapTile | Terraria.Map/MapTile.cs | D:\TRbackup\Version4\Terraria.Map\MapTile.cs | 30 | 2 | UpdateQueued | bool | `public bool UpdateQueued { get { return (_extraData & 0x40) != 0; } set { if (value) { _extraData \|= 64; } else { _extraData &= 191; } } }` | `public bool UpdateQueued { get { return (_extraData & 0x40) != 0; } set { if (value) { _extraData \|= 64; } else { _extraData &= 191; } } }` |
| 4534 | property | Terraria.Map.MapTile | Terraria.Map/MapTile.cs | D:\TRbackup\Version4\Terraria.Map\MapTile.cs | 49 | 2 | Color | byte | `public byte Color { get { return (byte)(_extraData & 0x1F); } set { _extraData = (byte)((_extraData & -32) \| (value & 0x1F)); } }` | `public byte Color { get { return (byte)(_extraData & 0x1F); } set { _extraData = (byte)((_extraData & -32) \| (value & 0x1F)); } }` |


### 4.27 细分子系统：`MapIoRuntimeState`

- 原报告章节：`4.11.28`
- 父级子系统：`ClientPresentationAndTools`
- 分区工作包：`18` / `地图、相机与绘制`

- 上一级基线细分子系统：`MapTileStorageAndUpdateState`
- 上一级 peer 细分子系统：`MapEncodingCatalogAndIoState`
- 细分职责：地图文件锁、场景缓存、雪量和压缩运行状态。
- 边界角色：`adapter state`；最小 seam：map I/O runtime port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：4；属性：0；合计：4。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（4）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 4245 | field | Terraria.Map.MapHelper | Terraria.Map/MapHelper.cs | D:\TRbackup\Version4\Terraria.Map\MapHelper.cs | 80 | 2 | IOLock | object | `private static object IOLock = new object();` | `private static object IOLock = new object();` |
| 4246 | field | Terraria.Map.MapHelper | Terraria.Map/MapHelper.cs | D:\TRbackup\Version4\Terraria.Map\MapHelper.cs | 82 | 2 | sceneArea | Rectangle | `public static Rectangle sceneArea;` | `public static Rectangle sceneArea;` |
| 4247 | field | Terraria.Map.MapHelper | Terraria.Map/MapHelper.cs | D:\TRbackup\Version4\Terraria.Map\MapHelper.cs | 84 | 2 | sceneSnowiness | float | `public static float sceneSnowiness;` | `public static float sceneSnowiness;` |
| 4249 | field | Terraria.Map.MapHelper | Terraria.Map/MapHelper.cs | D:\TRbackup\Version4\Terraria.Map\MapHelper.cs | 88 | 2 | zlibDecompress | ZlibCodec | `private static ZlibCodec zlibDecompress = new ZlibCodec(CompressionMode.Decompress);` | `private static ZlibCodec zlibDecompress = new ZlibCodec(CompressionMode.Decompress);` |

#### 属性（0）

无该类型成员记录。


### 4.28 细分子系统：`MapEncodingHeaderBitCatalogState`

- 原报告章节：`4.11.33`
- 父级子系统：`ClientPresentationAndTools`
- 分区工作包：`18` / `地图、相机与绘制`

- 上一级基线细分子系统：`MapTileStorageAndUpdateState`
- 上一级 peer 细分子系统：`MapEncodingHeaderCatalogState`
- 细分职责：地图编码 Header 位布局、保留位和颜色位定义。
- 边界角色：`adapter state`；最小 seam：map header bit layout port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：24；属性：0；合计：24。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（24）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 4215 | field | Terraria.Map.MapHelper | Terraria.Map/MapHelper.cs | D:\TRbackup\Version4\Terraria.Map\MapHelper.cs | 20 | 2 | HeaderEmpty | int | `private const int HeaderEmpty = 0;` | `private const int HeaderEmpty = 0;` |
| 4216 | field | Terraria.Map.MapHelper | Terraria.Map/MapHelper.cs | D:\TRbackup\Version4\Terraria.Map\MapHelper.cs | 22 | 2 | HeaderTile | int | `private const int HeaderTile = 1;` | `private const int HeaderTile = 1;` |
| 4217 | field | Terraria.Map.MapHelper | Terraria.Map/MapHelper.cs | D:\TRbackup\Version4\Terraria.Map\MapHelper.cs | 24 | 2 | HeaderWall | int | `private const int HeaderWall = 2;` | `private const int HeaderWall = 2;` |
| 4218 | field | Terraria.Map.MapHelper | Terraria.Map/MapHelper.cs | D:\TRbackup\Version4\Terraria.Map\MapHelper.cs | 26 | 2 | HeaderWater | int | `private const int HeaderWater = 3;` | `private const int HeaderWater = 3;` |
| 4219 | field | Terraria.Map.MapHelper | Terraria.Map/MapHelper.cs | D:\TRbackup\Version4\Terraria.Map\MapHelper.cs | 28 | 2 | HeaderLava | int | `private const int HeaderLava = 4;` | `private const int HeaderLava = 4;` |
| 4220 | field | Terraria.Map.MapHelper | Terraria.Map/MapHelper.cs | D:\TRbackup\Version4\Terraria.Map\MapHelper.cs | 30 | 2 | HeaderHoney | int | `private const int HeaderHoney = 5;` | `private const int HeaderHoney = 5;` |
| 4221 | field | Terraria.Map.MapHelper | Terraria.Map/MapHelper.cs | D:\TRbackup\Version4\Terraria.Map\MapHelper.cs | 32 | 2 | HeaderHeavenAndHell | int | `private const int HeaderHeavenAndHell = 6;` | `private const int HeaderHeavenAndHell = 6;` |
| 4222 | field | Terraria.Map.MapHelper | Terraria.Map/MapHelper.cs | D:\TRbackup\Version4\Terraria.Map\MapHelper.cs | 34 | 2 | HeaderBackground | int | `private const int HeaderBackground = 7;` | `private const int HeaderBackground = 7;` |
| 4223 | field | Terraria.Map.MapHelper | Terraria.Map/MapHelper.cs | D:\TRbackup\Version4\Terraria.Map\MapHelper.cs | 36 | 2 | Header2_ReadHeader3Bit | int | `private const int Header2_ReadHeader3Bit = 1;` | `private const int Header2_ReadHeader3Bit = 1;` |
| 4224 | field | Terraria.Map.MapHelper | Terraria.Map/MapHelper.cs | D:\TRbackup\Version4\Terraria.Map\MapHelper.cs | 38 | 2 | Header2Color1 | int | `private const int Header2Color1 = 2;` | `private const int Header2Color1 = 2;` |
| 4225 | field | Terraria.Map.MapHelper | Terraria.Map/MapHelper.cs | D:\TRbackup\Version4\Terraria.Map\MapHelper.cs | 40 | 2 | Header2Color2 | int | `private const int Header2Color2 = 4;` | `private const int Header2Color2 = 4;` |
| 4226 | field | Terraria.Map.MapHelper | Terraria.Map/MapHelper.cs | D:\TRbackup\Version4\Terraria.Map\MapHelper.cs | 42 | 2 | Header2Color3 | int | `private const int Header2Color3 = 8;` | `private const int Header2Color3 = 8;` |
| 4227 | field | Terraria.Map.MapHelper | Terraria.Map/MapHelper.cs | D:\TRbackup\Version4\Terraria.Map\MapHelper.cs | 44 | 2 | Header2Color4 | int | `private const int Header2Color4 = 16;` | `private const int Header2Color4 = 16;` |
| 4228 | field | Terraria.Map.MapHelper | Terraria.Map/MapHelper.cs | D:\TRbackup\Version4\Terraria.Map\MapHelper.cs | 46 | 2 | Header2Color5 | int | `private const int Header2Color5 = 32;` | `private const int Header2Color5 = 32;` |
| 4229 | field | Terraria.Map.MapHelper | Terraria.Map/MapHelper.cs | D:\TRbackup\Version4\Terraria.Map\MapHelper.cs | 48 | 2 | Header2ShimmerBit | int | `private const int Header2ShimmerBit = 64;` | `private const int Header2ShimmerBit = 64;` |
| 4230 | field | Terraria.Map.MapHelper | Terraria.Map/MapHelper.cs | D:\TRbackup\Version4\Terraria.Map\MapHelper.cs | 50 | 2 | Header2_UnusedBit8 | int | `private const int Header2_UnusedBit8 = 128;` | `private const int Header2_UnusedBit8 = 128;` |
| 4231 | field | Terraria.Map.MapHelper | Terraria.Map/MapHelper.cs | D:\TRbackup\Version4\Terraria.Map\MapHelper.cs | 52 | 2 | Header3_ReservedForHeader4Bit | int | `private const int Header3_ReservedForHeader4Bit = 1;` | `private const int Header3_ReservedForHeader4Bit = 1;` |
| 4232 | field | Terraria.Map.MapHelper | Terraria.Map/MapHelper.cs | D:\TRbackup\Version4\Terraria.Map\MapHelper.cs | 54 | 2 | Header3_UnusudBit2 | int | `private const int Header3_UnusudBit2 = 2;` | `private const int Header3_UnusudBit2 = 2;` |
| 4233 | field | Terraria.Map.MapHelper | Terraria.Map/MapHelper.cs | D:\TRbackup\Version4\Terraria.Map\MapHelper.cs | 56 | 2 | Header3_UnusudBit3 | int | `private const int Header3_UnusudBit3 = 4;` | `private const int Header3_UnusudBit3 = 4;` |
| 4234 | field | Terraria.Map.MapHelper | Terraria.Map/MapHelper.cs | D:\TRbackup\Version4\Terraria.Map\MapHelper.cs | 58 | 2 | Header3_UnusudBit4 | int | `private const int Header3_UnusudBit4 = 8;` | `private const int Header3_UnusudBit4 = 8;` |
| 4235 | field | Terraria.Map.MapHelper | Terraria.Map/MapHelper.cs | D:\TRbackup\Version4\Terraria.Map\MapHelper.cs | 60 | 2 | Header3_UnusudBit5 | int | `private const int Header3_UnusudBit5 = 16;` | `private const int Header3_UnusudBit5 = 16;` |
| 4236 | field | Terraria.Map.MapHelper | Terraria.Map/MapHelper.cs | D:\TRbackup\Version4\Terraria.Map\MapHelper.cs | 62 | 2 | Header3_UnusudBit6 | int | `private const int Header3_UnusudBit6 = 32;` | `private const int Header3_UnusudBit6 = 32;` |
| 4237 | field | Terraria.Map.MapHelper | Terraria.Map/MapHelper.cs | D:\TRbackup\Version4\Terraria.Map\MapHelper.cs | 64 | 2 | Header3_UnusudBit7 | int | `private const int Header3_UnusudBit7 = 64;` | `private const int Header3_UnusudBit7 = 64;` |
| 4238 | field | Terraria.Map.MapHelper | Terraria.Map/MapHelper.cs | D:\TRbackup\Version4\Terraria.Map\MapHelper.cs | 66 | 2 | Header3_UnusudBit8 | int | `private const int Header3_UnusudBit8 = 128;` | `private const int Header3_UnusudBit8 = 128;` |

#### 属性（0）

无该类型成员记录。


### 4.29 细分子系统：`MapEncodingOptionLimitState`

- 原报告章节：`4.11.34`
- 父级子系统：`ClientPresentationAndTools`
- 分区工作包：`18` / `地图、相机与绘制`

- 上一级基线细分子系统：`MapTileStorageAndUpdateState`
- 上一级 peer 细分子系统：`MapEncodingHeaderCatalogState`
- 细分职责：地图编码绘制循环、选项上限、渐变上限和区块尺寸。
- 边界角色：`adapter state`；最小 seam：map encoding option limit port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：8；属性：0；合计：8。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（8）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 4214 | field | Terraria.Map.MapHelper | Terraria.Map/MapHelper.cs | D:\TRbackup\Version4\Terraria.Map\MapHelper.cs | 18 | 2 | drawLoopMilliseconds | int | `public const int drawLoopMilliseconds = 5;` | `public const int drawLoopMilliseconds = 5;` |
| 4239 | field | Terraria.Map.MapHelper | Terraria.Map/MapHelper.cs | D:\TRbackup\Version4\Terraria.Map\MapHelper.cs | 68 | 2 | maxTileOptions | int | `public const int maxTileOptions = 13;` | `public const int maxTileOptions = 13;` |
| 4240 | field | Terraria.Map.MapHelper | Terraria.Map/MapHelper.cs | D:\TRbackup\Version4\Terraria.Map\MapHelper.cs | 70 | 2 | maxWallOptions | int | `public const int maxWallOptions = 2;` | `public const int maxWallOptions = 2;` |
| 4241 | field | Terraria.Map.MapHelper | Terraria.Map/MapHelper.cs | D:\TRbackup\Version4\Terraria.Map\MapHelper.cs | 72 | 2 | maxLiquidTypes | int | `private const int maxLiquidTypes = 4;` | `private const int maxLiquidTypes = 4;` |
| 4242 | field | Terraria.Map.MapHelper | Terraria.Map/MapHelper.cs | D:\TRbackup\Version4\Terraria.Map\MapHelper.cs | 74 | 2 | maxSkyGradients | int | `private const int maxSkyGradients = 256;` | `private const int maxSkyGradients = 256;` |
| 4243 | field | Terraria.Map.MapHelper | Terraria.Map/MapHelper.cs | D:\TRbackup\Version4\Terraria.Map\MapHelper.cs | 76 | 2 | maxDirtGradients | int | `private const int maxDirtGradients = 256;` | `private const int maxDirtGradients = 256;` |
| 4244 | field | Terraria.Map.MapHelper | Terraria.Map/MapHelper.cs | D:\TRbackup\Version4\Terraria.Map\MapHelper.cs | 78 | 2 | maxRockGradients | int | `private const int maxRockGradients = 256;` | `private const int maxRockGradients = 256;` |
| 4248 | field | Terraria.Map.MapHelper | Terraria.Map/MapHelper.cs | D:\TRbackup\Version4\Terraria.Map\MapHelper.cs | 86 | 2 | MapChunkSize | int | `public const int MapChunkSize = 64;` | `public const int MapChunkSize = 64;` |

#### 属性（0）

无该类型成员记录。


## 5. 追溯与验收

> 来源报告 SHA-256：`b944441d92d5103f218b766a3e354200794528a19e65a862e44c2b40a36f1196`
- 本分区细分组数：29；成员数：331；字段：274；属性：57。
- 预期不变量：本文件只出现完整细分子系统；不跨分区复制成员；所有成员的来源序号、原始声明、路径、行列和 C# 类型必须与来源报告一致。
- 验证方式：重新读取 20 个文件，检查文件数、细分组唯一性、来源序号恰好覆盖 `1..4542`、字段/属性合计和逐成员行文本一致；本次分区生成不运行 `dotnet`，因为没有修改 C# 或项目文件。

生成器：`Build/Tools/Generate-Version4NonAuthoritativeComponentPartitionReports.ps1`。
