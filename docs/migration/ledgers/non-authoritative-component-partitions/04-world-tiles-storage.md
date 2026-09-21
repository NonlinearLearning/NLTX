# Version4 非权威组件拆分分区 04/20：地块、液体与世界存储

> 来源报告：`docs/migration/ledgers/Version4非权威模拟系统字段属性逐成员源码声明-更细子系统拆分-去除ID类文件.md`
> 来源报告 SHA-256：`b944441d92d5103f218b766a3e354200794528a19e65a862e44c2b40a36f1196`
> 本文件是并行组件拆分工作包，不是新的权威成员清单；成员声明事实直接保留来源报告原文。

## 1. 分区定位与评估

- 分区范围：Tile、液体、地块框架、TileEntity、放置、绘制辅助和世界网格存储。
- 本分区组件化重点：确认地块/液体权威状态与缓存、快照、查询和结构变更边界。
- 本分区包含 23 个完整细分子系统、320 条成员记录（字段 310、属性 10）。来源序号覆盖区间 `174..3941`，区间可能与其他分区交错，不以序号定义领域边界。
- 组件边界判定：细分子系统只是并行处理的最小库存边界，不等价于一个组件。必须继续按共同读者/写者、变更原因、生命周期、权威性和副作用分为 Component、System、Query、Command、Adapter 或 Projection。
- 证据状态：成员声明和统计已由来源报告校验；逐成员读者、写者、初始化/清理路径、持久化/网络语义和系统顺序仍需本分区会话补证，不得从名称或数量推断。

## 2. 分区统计

| 父级子系统 | 细分子系统数 | 字段 | 属性 | 合计 |
|---|---:|---:|---:|---:|
| `RuntimeComposition` | 7 | 64 | 0 | 64 |
| `SharedRuntimeMechanisms` | 14 | 216 | 10 | 226 |
| `WorldStorage` | 2 | 30 | 0 | 30 |

| 原报告章节 | 父级 | 细分子系统 | 边界角色 | 字段 | 属性 | 合计 | 组件化处理 |
|---|---|---|---|---:|---:|---:|---|
| `4.1.14` | `RuntimeComposition` | `MainWorldGeometryAndCapacity` | runtime state | 11 | 0 | 11 | 待按成员访问模式拆分 |
| `4.1.16` | `RuntimeComposition` | `MainCameraAndLiquidState` | runtime state | 5 | 0 | 5 | 待按成员访问模式拆分 |
| `4.1.23` | `RuntimeComposition` | `MainTileFrameAndCatchMetadata` | runtime state | 5 | 0 | 5 | 待按成员访问模式拆分 |
| `4.1.24` | `RuntimeComposition` | `MainWorldMapAndTileStore` | runtime state | 2 | 0 | 2 | 待按成员访问模式拆分 |
| `4.1.43` | `RuntimeComposition` | `MainWallAndGlobalTileMetadata` | catalog reference | 6 | 0 | 6 | 待按成员访问模式拆分 |
| `4.1.48` | `RuntimeComposition` | `MainTileBehaviorAndInteractionMetadata` | catalog reference | 20 | 0 | 20 | 待按成员访问模式拆分 |
| `4.1.49` | `RuntimeComposition` | `MainTileLightingAndFrameMetadata` | catalog reference | 15 | 0 | 15 | 待按成员访问模式拆分 |
| `4.5.2` | `WorldStorage` | `TileCellMaterialAndLiquidState` | authoritative snapshot | 9 | 0 | 9 | 待按成员访问模式拆分 |
| `4.5.3` | `WorldStorage` | `TileCellFrameAndBitState` | authoritative snapshot | 21 | 0 | 21 | 待按成员访问模式拆分 |
| `4.9.31` | `SharedRuntimeMechanisms` | `SharedTileAnchorAndReachQueries` | query/state | 18 | 0 | 18 | 待按成员访问模式拆分 |
| `4.9.32` | `SharedRuntimeMechanisms` | `SharedTileFramingAndSignState` | state/query | 16 | 0 | 16 | 待按成员访问模式拆分 |
| `4.9.33` | `SharedRuntimeMechanisms` | `SharedTileSnapshots` | snapshot/query | 18 | 1 | 19 | 待按成员访问模式拆分 |
| `4.9.72` | `SharedRuntimeMechanisms` | `TileEntityRegistryAndBaseState` | state | 14 | 0 | 14 | 待按成员访问模式拆分 |
| `4.9.73` | `SharedRuntimeMechanisms` | `TileEntityDisplayAndInventoryState` | state/presentation | 24 | 0 | 24 | 待按成员访问模式拆分 |
| `4.9.74` | `SharedRuntimeMechanisms` | `TileEntityAnchorAndSensorState` | state/query | 11 | 0 | 11 | 待按成员访问模式拆分 |
| `4.9.75` | `SharedRuntimeMechanisms` | `TileEntityWorldInteractionState` | state/query | 9 | 0 | 9 | 待按成员访问模式拆分 |
| `4.9.121` | `SharedRuntimeMechanisms` | `SharedTilePlacementAnchorAndHookModules` | definition/query | 20 | 0 | 20 | 待按成员访问模式拆分 |
| `4.9.128` | `SharedRuntimeMechanisms` | `SharedTileObjectPreviewState` | query/snapshot | 16 | 9 | 25 | 待按成员访问模式拆分 |
| `4.9.129` | `SharedRuntimeMechanisms` | `SharedTileObjectPlacementValueState` | value object/query | 16 | 0 | 16 | 待按成员访问模式拆分 |
| `4.9.186` | `SharedRuntimeMechanisms` | `SharedTilePlacementCoordinateAndDrawModules` | definition/query | 14 | 0 | 14 | 待按成员访问模式拆分 |
| `4.9.187` | `SharedRuntimeMechanisms` | `SharedTilePlacementBaseAndStyleModules` | definition/query | 14 | 0 | 14 | 待按成员访问模式拆分 |
| `4.9.218` | `SharedRuntimeMechanisms` | `SharedTilePaintRenderTargetState` | presentation state | 12 | 0 | 12 | 待按成员访问模式拆分 |
| `4.9.219` | `SharedRuntimeMechanisms` | `SharedTilePaintVariationAndColorState` | definition/state | 14 | 0 | 14 | 待按成员访问模式拆分 |

## 3. 并行组件拆分契约

对本文件中的每个细分子系统和每条成员记录执行以下判断，并把结果写入对应的实现阶段设计/审计记录：

- 盘点声明类型、可见性、读者、写者、创建/更新/清理/持久化生命周期、访问频率和副作用；缺证据标记 `missing`，不可用命名猜测补齐。
- 标记权威状态、派生值、缓存、快照、兼容字段和表现数据；Component 只保存一个内聚概念的权威数据。
- 需要行为转换时放入显式 System/Command；纯资格判断和派生计算放入 Query；外部协议、文件、平台和第三方类型放入 Adapter；网络/持久化/日志/UI 输出放入单向 Projection。
- 分别建模实体引用、持久化 ID、网络 ID 和外部 ID；不得把 ID 类文件或外部对象直接塞入共享组件。
- 记录 Interface、Implementation、Seam、Depth、Leverage、Locality，以及必要的系统先后约束、失败归属和重试策略。

## 4. 逐成员源码声明

以下细分子系统块保持来源报告的成员表、声明、路径、行列和来源序号；只调整 Markdown 标题层级并增加分区追踪元数据。

### 4.1 细分子系统：`MainWorldGeometryAndCapacity`

- 原报告章节：`4.1.14`
- 父级子系统：`RuntimeComposition`
- 分区工作包：`04` / `地块、液体与世界存储`

- 上一级基线细分子系统：`MainWorldGeometryAndCapacity`
- 细分职责：世界边界、Tile 尺寸、区段和实体容量。
- 边界角色：`runtime state`；最小 seam：world geometry/capacity view；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：11；属性：0；合计：11。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（11）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 174 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 514 | 2 | leftWorld | float | `public static float leftWorld = 0f;` | `public static float leftWorld = 0f;` |
| 175 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 516 | 2 | rightWorld | float | `public static float rightWorld = 134400f;` | `public static float rightWorld = 134400f;` |
| 176 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 518 | 2 | topWorld | float | `public static float topWorld = 0f;` | `public static float topWorld = 0f;` |
| 177 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 520 | 2 | bottomWorld | float | `public static float bottomWorld = 38400f;` | `public static float bottomWorld = 38400f;` |
| 178 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 522 | 2 | maxTilesX | int | `public static int maxTilesX = (int)rightWorld / 16 + 1;` | `public static int maxTilesX = (int)rightWorld / 16 + 1;` |
| 179 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 524 | 2 | maxTilesY | int | `public static int maxTilesY = (int)bottomWorld / 16 + 1;` | `public static int maxTilesY = (int)bottomWorld / 16 + 1;` |
| 180 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 526 | 2 | maxSectionsX | int | `public static int maxSectionsX = maxTilesX / 200;` | `public static int maxSectionsX = maxTilesX / 200;` |
| 181 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 528 | 2 | maxSectionsY | int | `public static int maxSectionsY = maxTilesY / 150;` | `public static int maxSectionsY = maxTilesY / 150;` |
| 182 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 530 | 2 | maxDustToDraw | int | `public static int maxDustToDraw = 6000;` | `public static int maxDustToDraw = 6000;` |
| 183 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 532 | 2 | maxNetPlayers | int | `public static int maxNetPlayers = 255;` | `public static int maxNetPlayers = 255;` |
| 184 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 534 | 2 | maxNPCs | int | `public static readonly int maxNPCs = InitData.MaxNPCs;` | `public static readonly int maxNPCs = InitData.MaxNPCs;` |

#### 属性（0）

无该类型成员记录。


### 4.2 细分子系统：`MainCameraAndLiquidState`

- 原报告章节：`4.1.16`
- 父级子系统：`RuntimeComposition`
- 分区工作包：`04` / `地块、液体与世界存储`

- 上一级基线细分子系统：`MainCameraAndLiquidState`
- 细分职责：相机坐标、液体透明度、样式和缓冲区。
- 边界角色：`runtime state`；最小 seam：camera/liquid view；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：5；属性：0；合计：5。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（5）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 193 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 552 | 2 | invBottom | int | `public int invBottom = 210;` | `public int invBottom = 210;` |
| 194 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 555 | 2 | cameraX | float | `public static float cameraX;` | `public static float cameraX;` |
| 195 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 557 | 2 | cameraY | float | `public static float cameraY;` | `public static float cameraY;` |
| 196 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 559 | 2 | liquidAlpha | float[] | `public static float[] liquidAlpha = new float[15];` | `public static float[] liquidAlpha = new float[15];` |
| 197 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 561 | 2 | waterStyle | int | `public static int waterStyle;` | `public static int waterStyle;` |

#### 属性（0）

无该类型成员记录。


### 4.3 细分子系统：`MainTileFrameAndCatchMetadata`

- 原报告章节：`4.1.23`
- 父级子系统：`RuntimeComposition`
- 分区工作包：`04` / `地块、液体与世界存储`

- 上一级基线细分子系统：`MainTileFrameAndCatchMetadata`
- 细分职责：Tile 砂土、火焰、可捕获标记和帧缓存。
- 边界角色：`runtime state`；最小 seam：tile-frame/catch view；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：5；属性：0；合计：5。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（5）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 369 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 915 | 2 | tileSand | bool[] | `public static bool[] tileSand = new bool[TileID.Count];` | `public static bool[] tileSand = new bool[TileID.Count];` |
| 370 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 917 | 2 | tileFlame | bool[] | `public static bool[] tileFlame = new bool[TileID.Count];` | `public static bool[] tileFlame = new bool[TileID.Count];` |
| 371 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 919 | 2 | npcCatchable | bool[] | `public static bool[] npcCatchable = new bool[NPCID.Count];` | `public static bool[] npcCatchable = new bool[NPCID.Count];` |
| 372 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 921 | 2 | tileFrame | int[] | `public static int[] tileFrame = new int[TileID.Count];` | `public static int[] tileFrame = new int[TileID.Count];` |
| 373 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 923 | 2 | tileFrameCounter | int[] | `public static int[] tileFrameCounter = new int[TileID.Count];` | `public static int[] tileFrameCounter = new int[TileID.Count];` |

#### 属性（0）

无该类型成员记录。


### 4.4 细分子系统：`MainWorldMapAndTileStore`

- 原报告章节：`4.1.24`
- 父级子系统：`RuntimeComposition`
- 分区工作包：`04` / `地块、液体与世界存储`

- 上一级基线细分子系统：`MainWorldMapAndTileStore`
- 细分职责：世界地图和全局 Tile 存储引用。
- 边界角色：`runtime state`；最小 seam：world map/tile store；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：2；属性：0；合计：2。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（2）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 374 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 926 | 2 | Map | Terraria.Map.WorldMap | `public static WorldMap Map;` | `public static WorldMap Map;` |
| 375 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 928 | 2 | tile | Terraria.Tile[,] | `public static Tile[,] tile = new Tile[maxTilesX, maxTilesY];` | `public static Tile[,] tile = new Tile[maxTilesX, maxTilesY];` |

#### 属性（0）

无该类型成员记录。


### 4.5 细分子系统：`MainWallAndGlobalTileMetadata`

- 原报告章节：`4.1.43`
- 父级子系统：`RuntimeComposition`
- 分区工作包：`04` / `地块、液体与世界存储`

- 上一级基线细分子系统：`MainTileAndWallMetadata`
- 细分职责：Main 的墙体、全局合并和音乐淡出元数据。
- 边界角色：`catalog reference`；最小 seam：main wall metadata port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：6；属性：0；合计：6。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（6）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 255 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 687 | 2 | musicFade | float[] | `public static float[] musicFade = new float[maxMusic];` | `public static float[] musicFade = new float[maxMusic];` |
| 262 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 701 | 2 | wallHouse | bool[] | `public static bool[] wallHouse = new bool[WallID.Count];` | `public static bool[] wallHouse = new bool[WallID.Count];` |
| 263 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 703 | 2 | wallDungeon | bool[] | `public static bool[] wallDungeon = new bool[WallID.Count];` | `public static bool[] wallDungeon = new bool[WallID.Count];` |
| 264 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 705 | 2 | wallLight | bool[] | `public static bool[] wallLight = new bool[WallID.Count];` | `public static bool[] wallLight = new bool[WallID.Count];` |
| 265 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 707 | 2 | wallBlend | int[] | `public static int[] wallBlend = new int[WallID.Count];` | `public static int[] wallBlend = new int[WallID.Count];` |
| 281 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 739 | 2 | wallLargeFrames | byte[] | `public static byte[] wallLargeFrames = new byte[WallID.Count];` | `public static byte[] wallLargeFrames = new byte[WallID.Count];` |

#### 属性（0）

无该类型成员记录。


### 4.6 细分子系统：`MainTileBehaviorAndInteractionMetadata`

- 原报告章节：`4.1.48`
- 父级子系统：`RuntimeComposition`
- 分区工作包：`04` / `地块、液体与世界存储`

- 上一级基线细分子系统：`MainTileAndWallMetadata`
- 上一级 peer 细分子系统：`MainTileBehaviorMetadata`
- 细分职责：Main 的 Tile 交互、碰撞、放置和行为元数据。
- 边界角色：`catalog reference`；最小 seam：main tile behavior interaction port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：20；属性：0；合计：20。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（20）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 257 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 691 | 2 | tileMergeDirt | bool[] | `public static bool[] tileMergeDirt = new bool[TileID.Count];` | `public static bool[] tileMergeDirt = new bool[TileID.Count];` |
| 258 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 693 | 2 | tileCut | bool[] | `public static bool[] tileCut = new bool[TileID.Count];` | `public static bool[] tileCut = new bool[TileID.Count];` |
| 259 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 695 | 2 | tileAlch | bool[] | `public static bool[] tileAlch = new bool[TileID.Count];` | `public static bool[] tileAlch = new bool[TileID.Count];` |
| 266 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 709 | 2 | tileStone | bool[] | `public static bool[] tileStone = new bool[TileID.Count];` | `public static bool[] tileStone = new bool[TileID.Count];` |
| 267 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 711 | 2 | tileAxe | bool[] | `public static bool[] tileAxe = new bool[TileID.Count];` | `public static bool[] tileAxe = new bool[TileID.Count];` |
| 268 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 713 | 2 | tileHammer | bool[] | `public static bool[] tileHammer = new bool[TileID.Count];` | `public static bool[] tileHammer = new bool[TileID.Count];` |
| 269 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 715 | 2 | tileWaterDeath | bool[] | `public static bool[] tileWaterDeath = new bool[TileID.Count];` | `public static bool[] tileWaterDeath = new bool[TileID.Count];` |
| 270 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 717 | 2 | tileLavaDeath | bool[] | `public static bool[] tileLavaDeath = new bool[TileID.Count];` | `public static bool[] tileLavaDeath = new bool[TileID.Count];` |
| 271 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 719 | 2 | tileTable | bool[] | `public static bool[] tileTable = new bool[TileID.Count];` | `public static bool[] tileTable = new bool[TileID.Count];` |
| 276 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 729 | 2 | tileSolidTop | bool[] | `public static bool[] tileSolidTop = new bool[TileID.Count];` | `public static bool[] tileSolidTop = new bool[TileID.Count];` |
| 277 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 731 | 2 | tileSolid | bool[] | `public static bool[] tileSolid = new bool[TileID.Count];` | `public static bool[] tileSolid = new bool[TileID.Count];` |
| 278 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 733 | 2 | tileBouncy | bool[] | `public static bool[] tileBouncy = new bool[TileID.Count];` | `public static bool[] tileBouncy = new bool[TileID.Count];` |
| 279 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 735 | 2 | tileOreFinderPriority | short[] | `public static short[] tileOreFinderPriority = new short[TileID.Count];` | `public static short[] tileOreFinderPriority = new short[TileID.Count];` |
| 282 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 741 | 2 | tileRope | bool[] | `public static bool[] tileRope = new bool[TileID.Count];` | `public static bool[] tileRope = new bool[TileID.Count];` |
| 285 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 747 | 2 | tileNoAttach | bool[] | `public static bool[] tileNoAttach = new bool[TileID.Count];` | `public static bool[] tileNoAttach = new bool[TileID.Count];` |
| 286 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 749 | 2 | tileNoFail | bool[] | `public static bool[] tileNoFail = new bool[TileID.Count];` | `public static bool[] tileNoFail = new bool[TileID.Count];` |
| 292 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 761 | 2 | tileGlowMask | short[] | `public static short[] tileGlowMask = new short[TileID.Count];` | `public static short[] tileGlowMask = new short[TileID.Count];` |
| 293 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 763 | 2 | tileContainer | bool[] | `public static bool[] tileContainer = new bool[TileID.Count];` | `public static bool[] tileContainer = new bool[TileID.Count];` |
| 294 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 765 | 2 | tileSign | bool[] | `public static bool[] tileSign = new bool[TileID.Count];` | `public static bool[] tileSign = new bool[TileID.Count];` |
| 295 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 767 | 2 | tileMerge | bool[][] | `public static bool[][] tileMerge = new bool[TileID.Count][];` | `public static bool[][] tileMerge = new bool[TileID.Count][];` |

#### 属性（0）

无该类型成员记录。


### 4.7 细分子系统：`MainTileLightingAndFrameMetadata`

- 原报告章节：`4.1.49`
- 父级子系统：`RuntimeComposition`
- 分区工作包：`04` / `地块、液体与世界存储`

- 上一级基线细分子系统：`MainTileAndWallMetadata`
- 上一级 peer 细分子系统：`MainTileBehaviorMetadata`
- 细分职责：Main 的 Tile 光照、框架、砖墙和表现元数据。
- 边界角色：`catalog reference`；最小 seam：main tile lighting frame port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：15；属性：0；合计：15。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（15）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 256 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 689 | 2 | tileLighted | bool[] | `public static bool[] tileLighted = new bool[TileID.Count];` | `public static bool[] tileLighted = new bool[TileID.Count];` |
| 260 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 697 | 2 | tileShine | int[] | `public static int[] tileShine = new int[TileID.Count];` | `public static int[] tileShine = new int[TileID.Count];` |
| 261 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 699 | 2 | tileShine2 | bool[] | `public static bool[] tileShine2 = new bool[TileID.Count];` | `public static bool[] tileShine2 = new bool[TileID.Count];` |
| 272 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 721 | 2 | tileBlockLight | bool[] | `public static bool[] tileBlockLight = new bool[TileID.Count];` | `public static bool[] tileBlockLight = new bool[TileID.Count];` |
| 273 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 723 | 2 | tileNoSunLight | bool[] | `public static bool[] tileNoSunLight = new bool[TileID.Count];` | `public static bool[] tileNoSunLight = new bool[TileID.Count];` |
| 274 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 725 | 2 | tileDungeon | bool[] | `public static bool[] tileDungeon = new bool[TileID.Count];` | `public static bool[] tileDungeon = new bool[TileID.Count];` |
| 275 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 727 | 2 | tileSpelunker | bool[] | `public static bool[] tileSpelunker = new bool[TileID.Count];` | `public static bool[] tileSpelunker = new bool[TileID.Count];` |
| 280 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 737 | 2 | tileLargeFrames | byte[] | `public static byte[] tileLargeFrames = new byte[TileID.Count];` | `public static byte[] tileLargeFrames = new byte[TileID.Count];` |
| 283 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 743 | 2 | tileBrick | bool[] | `public static bool[] tileBrick = new bool[TileID.Count];` | `public static bool[] tileBrick = new bool[TileID.Count];` |
| 284 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 745 | 2 | tileMoss | bool[] | `public static bool[] tileMoss = new bool[TileID.Count];` | `public static bool[] tileMoss = new bool[TileID.Count];` |
| 287 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 751 | 2 | tileCracked | bool[] | `public static bool[] tileCracked = new bool[TileID.Count];` | `public static bool[] tileCracked = new bool[TileID.Count];` |
| 288 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 753 | 2 | tileObsidianKill | bool[] | `public static bool[] tileObsidianKill = new bool[TileID.Count];` | `public static bool[] tileObsidianKill = new bool[TileID.Count];` |
| 289 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 755 | 2 | tileFrameImportant | bool[] | `public static bool[] tileFrameImportant = new bool[TileID.Count];` | `public static bool[] tileFrameImportant = new bool[TileID.Count];` |
| 290 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 757 | 2 | tilePile | bool[] | `public static bool[] tilePile = new bool[TileID.Count];` | `public static bool[] tilePile = new bool[TileID.Count];` |
| 291 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 759 | 2 | tileBlendAll | bool[] | `public static bool[] tileBlendAll = new bool[TileID.Count];` | `public static bool[] tileBlendAll = new bool[TileID.Count];` |

#### 属性（0）

无该类型成员记录。


### 4.8 细分子系统：`TileCellMaterialAndLiquidState`

- 原报告章节：`4.5.2`
- 父级子系统：`WorldStorage`
- 分区工作包：`04` / `地块、液体与世界存储`

- 上一级基线细分子系统：`TileCellStorage`
- 细分职责：单格 Tile 类型、墙体和液体材料状态。
- 边界角色：`authoritative snapshot`；最小 seam：tile material liquid port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：9；属性：0；合计：9。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（9）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 788 | field | Terraria.Tile | Terraria/Tile.cs | D:\TRbackup\Version4\Terraria\Tile.cs | 8 | 2 | type | ushort | `public ushort type;` | `public ushort type;` |
| 789 | field | Terraria.Tile | Terraria/Tile.cs | D:\TRbackup\Version4\Terraria\Tile.cs | 10 | 2 | wall | ushort | `public ushort wall;` | `public ushort wall;` |
| 790 | field | Terraria.Tile | Terraria/Tile.cs | D:\TRbackup\Version4\Terraria\Tile.cs | 12 | 2 | liquid | byte | `public byte liquid;` | `public byte liquid;` |
| 812 | field | Terraria.Tile | Terraria/Tile.cs | D:\TRbackup\Version4\Terraria\Tile.cs | 56 | 2 | Liquid_Water | int | `public const int Liquid_Water = 0;` | `public const int Liquid_Water = 0;` |
| 813 | field | Terraria.Tile | Terraria/Tile.cs | D:\TRbackup\Version4\Terraria\Tile.cs | 58 | 2 | Liquid_Lava | int | `public const int Liquid_Lava = 1;` | `public const int Liquid_Lava = 1;` |
| 814 | field | Terraria.Tile | Terraria/Tile.cs | D:\TRbackup\Version4\Terraria\Tile.cs | 60 | 2 | Liquid_Honey | int | `public const int Liquid_Honey = 2;` | `public const int Liquid_Honey = 2;` |
| 815 | field | Terraria.Tile | Terraria/Tile.cs | D:\TRbackup\Version4\Terraria\Tile.cs | 62 | 2 | Liquid_Shimmer | int | `public const int Liquid_Shimmer = 3;` | `public const int Liquid_Shimmer = 3;` |
| 816 | field | Terraria.Tile | Terraria/Tile.cs | D:\TRbackup\Version4\Terraria\Tile.cs | 64 | 2 | NeitherLavaOrHoney | int | `private const int NeitherLavaOrHoney = 159;` | `private const int NeitherLavaOrHoney = 159;` |
| 817 | field | Terraria.Tile | Terraria/Tile.cs | D:\TRbackup\Version4\Terraria\Tile.cs | 66 | 2 | EitherLavaOrHoney | int | `private const int EitherLavaOrHoney = 96;` | `private const int EitherLavaOrHoney = 96;` |

#### 属性（0）

无该类型成员记录。


### 4.9 细分子系统：`TileCellFrameAndBitState`

- 原报告章节：`4.5.3`
- 父级子系统：`WorldStorage`
- 分区工作包：`04` / `地块、液体与世界存储`

- 上一级基线细分子系统：`TileCellStorage`
- 细分职责：单格 Tile 帧坐标、头位和形状位状态。
- 边界角色：`authoritative snapshot`；最小 seam：tile frame bit port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：21；属性：0；合计：21。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（21）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 791 | field | Terraria.Tile | Terraria/Tile.cs | D:\TRbackup\Version4\Terraria\Tile.cs | 14 | 2 | sTileHeader | ushort | `public ushort sTileHeader;` | `public ushort sTileHeader;` |
| 792 | field | Terraria.Tile | Terraria/Tile.cs | D:\TRbackup\Version4\Terraria\Tile.cs | 16 | 2 | bTileHeader | byte | `public byte bTileHeader;` | `public byte bTileHeader;` |
| 793 | field | Terraria.Tile | Terraria/Tile.cs | D:\TRbackup\Version4\Terraria\Tile.cs | 18 | 2 | bTileHeader2 | byte | `public byte bTileHeader2;` | `public byte bTileHeader2;` |
| 794 | field | Terraria.Tile | Terraria/Tile.cs | D:\TRbackup\Version4\Terraria\Tile.cs | 20 | 2 | bTileHeader3 | byte | `public byte bTileHeader3;` | `public byte bTileHeader3;` |
| 795 | field | Terraria.Tile | Terraria/Tile.cs | D:\TRbackup\Version4\Terraria\Tile.cs | 22 | 2 | frameX | short | `public short frameX;` | `public short frameX;` |
| 796 | field | Terraria.Tile | Terraria/Tile.cs | D:\TRbackup\Version4\Terraria\Tile.cs | 24 | 2 | frameY | short | `public short frameY;` | `public short frameY;` |
| 797 | field | Terraria.Tile | Terraria/Tile.cs | D:\TRbackup\Version4\Terraria\Tile.cs | 26 | 2 | Bit0 | int | `private const int Bit0 = 1;` | `private const int Bit0 = 1;` |
| 798 | field | Terraria.Tile | Terraria/Tile.cs | D:\TRbackup\Version4\Terraria\Tile.cs | 28 | 2 | Bit1 | int | `private const int Bit1 = 2;` | `private const int Bit1 = 2;` |
| 799 | field | Terraria.Tile | Terraria/Tile.cs | D:\TRbackup\Version4\Terraria\Tile.cs | 30 | 2 | Bit2 | int | `private const int Bit2 = 4;` | `private const int Bit2 = 4;` |
| 800 | field | Terraria.Tile | Terraria/Tile.cs | D:\TRbackup\Version4\Terraria\Tile.cs | 32 | 2 | Bit3 | int | `private const int Bit3 = 8;` | `private const int Bit3 = 8;` |
| 801 | field | Terraria.Tile | Terraria/Tile.cs | D:\TRbackup\Version4\Terraria\Tile.cs | 34 | 2 | Bit4 | int | `private const int Bit4 = 16;` | `private const int Bit4 = 16;` |
| 802 | field | Terraria.Tile | Terraria/Tile.cs | D:\TRbackup\Version4\Terraria\Tile.cs | 36 | 2 | Bit5 | int | `private const int Bit5 = 32;` | `private const int Bit5 = 32;` |
| 803 | field | Terraria.Tile | Terraria/Tile.cs | D:\TRbackup\Version4\Terraria\Tile.cs | 38 | 2 | Bit6 | int | `private const int Bit6 = 64;` | `private const int Bit6 = 64;` |
| 804 | field | Terraria.Tile | Terraria/Tile.cs | D:\TRbackup\Version4\Terraria\Tile.cs | 40 | 2 | Bit7 | int | `private const int Bit7 = 128;` | `private const int Bit7 = 128;` |
| 805 | field | Terraria.Tile | Terraria/Tile.cs | D:\TRbackup\Version4\Terraria\Tile.cs | 42 | 2 | Bit15 | ushort | `private const ushort Bit15 = 32768;` | `private const ushort Bit15 = 32768;` |
| 806 | field | Terraria.Tile | Terraria/Tile.cs | D:\TRbackup\Version4\Terraria\Tile.cs | 44 | 2 | Type_Solid | int | `public const int Type_Solid = 0;` | `public const int Type_Solid = 0;` |
| 807 | field | Terraria.Tile | Terraria/Tile.cs | D:\TRbackup\Version4\Terraria\Tile.cs | 46 | 2 | Type_Halfbrick | int | `public const int Type_Halfbrick = 1;` | `public const int Type_Halfbrick = 1;` |
| 808 | field | Terraria.Tile | Terraria/Tile.cs | D:\TRbackup\Version4\Terraria\Tile.cs | 48 | 2 | Type_SlopeDownRight | int | `public const int Type_SlopeDownRight = 2;` | `public const int Type_SlopeDownRight = 2;` |
| 809 | field | Terraria.Tile | Terraria/Tile.cs | D:\TRbackup\Version4\Terraria\Tile.cs | 50 | 2 | Type_SlopeDownLeft | int | `public const int Type_SlopeDownLeft = 3;` | `public const int Type_SlopeDownLeft = 3;` |
| 810 | field | Terraria.Tile | Terraria/Tile.cs | D:\TRbackup\Version4\Terraria\Tile.cs | 52 | 2 | Type_SlopeUpRight | int | `public const int Type_SlopeUpRight = 4;` | `public const int Type_SlopeUpRight = 4;` |
| 811 | field | Terraria.Tile | Terraria/Tile.cs | D:\TRbackup\Version4\Terraria\Tile.cs | 54 | 2 | Type_SlopeUpLeft | int | `public const int Type_SlopeUpLeft = 5;` | `public const int Type_SlopeUpLeft = 5;` |

#### 属性（0）

无该类型成员记录。


### 4.10 细分子系统：`SharedTileAnchorAndReachQueries`

- 原报告章节：`4.9.31`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`04` / `地块、液体与世界存储`

- 上一级基线细分子系统：`SharedTileAnchorAndReachQueries`
- 细分职责：锚点、可达性、坐标和值对象支持。
- 边界角色：`query/state`；最小 seam：anchor/reach query；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：4；声明类型数：5；字段：18；属性：0；合计：18。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（18）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1033 | field | Terraria.DataStructures.AnchorData | Terraria.DataStructures/AnchorData.cs | D:\TRbackup\Version4\Terraria.DataStructures\AnchorData.cs | 7 | 2 | type | Terraria.Enums.AnchorType | `public AnchorType type;` | `public AnchorType type;` |
| 1034 | field | Terraria.DataStructures.AnchorData | Terraria.DataStructures/AnchorData.cs | D:\TRbackup\Version4\Terraria.DataStructures\AnchorData.cs | 9 | 2 | tileCount | int | `public int tileCount;` | `public int tileCount;` |
| 1035 | field | Terraria.DataStructures.AnchorData | Terraria.DataStructures/AnchorData.cs | D:\TRbackup\Version4\Terraria.DataStructures\AnchorData.cs | 11 | 2 | checkStart | int | `public int checkStart;` | `public int checkStart;` |
| 1036 | field | Terraria.DataStructures.AnchorData | Terraria.DataStructures/AnchorData.cs | D:\TRbackup\Version4\Terraria.DataStructures\AnchorData.cs | 13 | 2 | Empty | Terraria.DataStructures.AnchorData | `public static AnchorData Empty;` | `public static AnchorData Empty;` |
| 1037 | field | Terraria.DataStructures.AnchoredEntitiesCollection.IndexPointPair | Terraria.DataStructures/AnchoredEntitiesCollection.cs | D:\TRbackup\Version4\Terraria.DataStructures\AnchoredEntitiesCollection.cs | 10 | 3 | index | int | `public int index;` | `public int index;` |
| 1038 | field | Terraria.DataStructures.AnchoredEntitiesCollection.IndexPointPair | Terraria.DataStructures/AnchoredEntitiesCollection.cs | D:\TRbackup\Version4\Terraria.DataStructures\AnchoredEntitiesCollection.cs | 12 | 3 | coords | Point | `public Point coords;` | `public Point coords;` |
| 1039 | field | Terraria.DataStructures.AnchoredEntitiesCollection | Terraria.DataStructures/AnchoredEntitiesCollection.cs | D:\TRbackup\Version4\Terraria.DataStructures\AnchoredEntitiesCollection.cs | 15 | 2 | _anchoredNPCs | System.Collections.Generic.List<Terraria.DataStructures.AnchoredEntitiesCollection.IndexPointPair> | `private List<IndexPointPair> _anchoredNPCs;` | `private List<IndexPointPair> _anchoredNPCs;` |
| 1040 | field | Terraria.DataStructures.AnchoredEntitiesCollection | Terraria.DataStructures/AnchoredEntitiesCollection.cs | D:\TRbackup\Version4\Terraria.DataStructures\AnchoredEntitiesCollection.cs | 17 | 2 | _anchoredPlayers | System.Collections.Generic.List<Terraria.DataStructures.AnchoredEntitiesCollection.IndexPointPair> | `private List<IndexPointPair> _anchoredPlayers;` | `private List<IndexPointPair> _anchoredPlayers;` |
| 1263 | field | Terraria.DataStructures.Point16 | Terraria.DataStructures/Point16.cs | D:\TRbackup\Version4\Terraria.DataStructures\Point16.cs | 7 | 2 | X | short | `public short X;` | `public short X;` |
| 1264 | field | Terraria.DataStructures.Point16 | Terraria.DataStructures/Point16.cs | D:\TRbackup\Version4\Terraria.DataStructures\Point16.cs | 9 | 2 | Y | short | `public short Y;` | `public short Y;` |
| 1265 | field | Terraria.DataStructures.Point16 | Terraria.DataStructures/Point16.cs | D:\TRbackup\Version4\Terraria.DataStructures\Point16.cs | 11 | 2 | Zero | Terraria.DataStructures.Point16 | `public static Point16 Zero = new Point16(0, 0);` | `public static Point16 Zero = new Point16(0, 0);` |
| 1266 | field | Terraria.DataStructures.Point16 | Terraria.DataStructures/Point16.cs | D:\TRbackup\Version4\Terraria.DataStructures\Point16.cs | 13 | 2 | NegativeOne | Terraria.DataStructures.Point16 | `public static Point16 NegativeOne = new Point16(-1, -1);` | `public static Point16 NegativeOne = new Point16(-1, -1);` |
| 1329 | field | Terraria.DataStructures.TileReachCheckSettings | Terraria.DataStructures/TileReachCheckSettings.cs | D:\TRbackup\Version4\Terraria.DataStructures\TileReachCheckSettings.cs | 8 | 2 | TileRangeMultiplier | int | `public int TileRangeMultiplier;` | `public int TileRangeMultiplier;` |
| 1330 | field | Terraria.DataStructures.TileReachCheckSettings | Terraria.DataStructures/TileReachCheckSettings.cs | D:\TRbackup\Version4\Terraria.DataStructures\TileReachCheckSettings.cs | 10 | 2 | TileReachLimit | int? | `public int? TileReachLimit;` | `public int? TileReachLimit;` |
| 1331 | field | Terraria.DataStructures.TileReachCheckSettings | Terraria.DataStructures/TileReachCheckSettings.cs | D:\TRbackup\Version4\Terraria.DataStructures\TileReachCheckSettings.cs | 12 | 2 | OverrideXReach | int? | `public int? OverrideXReach;` | `public int? OverrideXReach;` |
| 1332 | field | Terraria.DataStructures.TileReachCheckSettings | Terraria.DataStructures/TileReachCheckSettings.cs | D:\TRbackup\Version4\Terraria.DataStructures\TileReachCheckSettings.cs | 14 | 2 | OverrideYReach | int? | `public int? OverrideYReach;` | `public int? OverrideYReach;` |
| 1333 | field | Terraria.DataStructures.TileReachCheckSettings | Terraria.DataStructures/TileReachCheckSettings.cs | D:\TRbackup\Version4\Terraria.DataStructures\TileReachCheckSettings.cs | 16 | 2 | Simple | Terraria.DataStructures.TileReachCheckSettings | `public static readonly TileReachCheckSettings Simple = new TileReachCheckSettings  	{  		TileRangeMultiplier = 1,  		TileReachLimit = 20  	};` | `public static readonly TileReachCheckSettings Simple = new TileReachCheckSettings { TileRangeMultiplier = 1, TileReachLimit = 20 };` |
| 1334 | field | Terraria.DataStructures.TileReachCheckSettings | Terraria.DataStructures/TileReachCheckSettings.cs | D:\TRbackup\Version4\Terraria.DataStructures\TileReachCheckSettings.cs | 22 | 2 | Pylons | Terraria.DataStructures.TileReachCheckSettings | `public static readonly TileReachCheckSettings Pylons = new TileReachCheckSettings  	{  		OverrideXReach = 60,  		OverrideYReach = 60  	};` | `public static readonly TileReachCheckSettings Pylons = new TileReachCheckSettings { OverrideXReach = 60, OverrideYReach = 60 };` |

#### 属性（0）

无该类型成员记录。


### 4.11 细分子系统：`SharedTileFramingAndSignState`

- 原报告章节：`4.9.32`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`04` / `地块、液体与世界存储`

- 上一级基线细分子系统：`SharedTileFramingAndSignState`
- 细分职责：Tile 框架和告示牌状态。
- 边界角色：`state/query`；最小 seam：framing/sign view；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：2；声明类型数：3；字段：16；属性：0；合计：16。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（16）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3078 | field | Terraria.Framing.BlockStyle | Terraria/Framing.cs | D:\TRbackup\Version4\Terraria\Framing.cs | 11 | 3 | top | bool | `public bool top;` | `public bool top;` |
| 3079 | field | Terraria.Framing.BlockStyle | Terraria/Framing.cs | D:\TRbackup\Version4\Terraria\Framing.cs | 13 | 3 | bottom | bool | `public bool bottom;` | `public bool bottom;` |
| 3080 | field | Terraria.Framing.BlockStyle | Terraria/Framing.cs | D:\TRbackup\Version4\Terraria\Framing.cs | 15 | 3 | left | bool | `public bool left;` | `public bool left;` |
| 3081 | field | Terraria.Framing.BlockStyle | Terraria/Framing.cs | D:\TRbackup\Version4\Terraria\Framing.cs | 17 | 3 | right | bool | `public bool right;` | `public bool right;` |
| 3082 | field | Terraria.Framing | Terraria/Framing.cs | D:\TRbackup\Version4\Terraria\Framing.cs | 35 | 2 | selfFrame8WayLookup | Terraria.DataStructures.Point16[][] | `private static Point16[][] selfFrame8WayLookup;` | `private static Point16[][] selfFrame8WayLookup;` |
| 3083 | field | Terraria.Framing | Terraria/Framing.cs | D:\TRbackup\Version4\Terraria\Framing.cs | 37 | 2 | wallFrameLookup | Terraria.DataStructures.Point16[][] | `private static Point16[][] wallFrameLookup;` | `private static Point16[][] wallFrameLookup;` |
| 3084 | field | Terraria.Framing | Terraria/Framing.cs | D:\TRbackup\Version4\Terraria\Framing.cs | 39 | 2 | frameSize8Way | Terraria.DataStructures.Point16 | `private static Point16 frameSize8Way;` | `private static Point16 frameSize8Way;` |
| 3085 | field | Terraria.Framing | Terraria/Framing.cs | D:\TRbackup\Version4\Terraria\Framing.cs | 41 | 2 | wallFrameSize | Terraria.DataStructures.Point16 | `private static Point16 wallFrameSize;` | `private static Point16 wallFrameSize;` |
| 3086 | field | Terraria.Framing | Terraria/Framing.cs | D:\TRbackup\Version4\Terraria\Framing.cs | 43 | 2 | blockStyleLookup | Terraria.Framing.BlockStyle[] | `private static BlockStyle[] blockStyleLookup;` | `private static BlockStyle[] blockStyleLookup;` |
| 3087 | field | Terraria.Framing | Terraria/Framing.cs | D:\TRbackup\Version4\Terraria\Framing.cs | 45 | 2 | phlebasTileFrameNumberLookup | int[][] | `private static int[][] phlebasTileFrameNumberLookup;` | `private static int[][] phlebasTileFrameNumberLookup;` |
| 3088 | field | Terraria.Framing | Terraria/Framing.cs | D:\TRbackup\Version4\Terraria\Framing.cs | 47 | 2 | lazureTileFrameNumberLookup | int[][] | `private static int[][] lazureTileFrameNumberLookup;` | `private static int[][] lazureTileFrameNumberLookup;` |
| 3089 | field | Terraria.Framing | Terraria/Framing.cs | D:\TRbackup\Version4\Terraria\Framing.cs | 49 | 2 | centerWallFrameLookup | int[][] | `private static int[][] centerWallFrameLookup;` | `private static int[][] centerWallFrameLookup;` |
| 3487 | field | Terraria.Sign | Terraria/Sign.cs | D:\TRbackup\Version4\Terraria\Sign.cs | 5 | 2 | maxSigns | int | `public const int maxSigns = 32000;` | `public const int maxSigns = 32000;` |
| 3488 | field | Terraria.Sign | Terraria/Sign.cs | D:\TRbackup\Version4\Terraria\Sign.cs | 7 | 2 | x | int | `public int x;` | `public int x;` |
| 3489 | field | Terraria.Sign | Terraria/Sign.cs | D:\TRbackup\Version4\Terraria\Sign.cs | 9 | 2 | y | int | `public int y;` | `public int y;` |
| 3490 | field | Terraria.Sign | Terraria/Sign.cs | D:\TRbackup\Version4\Terraria\Sign.cs | 11 | 2 | text | string | `public string text;` | `public string text;` |

#### 属性（0）

无该类型成员记录。


### 4.12 细分子系统：`SharedTileSnapshots`

- 原报告章节：`4.9.33`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`04` / `地块、液体与世界存储`

- 上一级基线细分子系统：`SharedTileSnapshots`
- 细分职责：Tile 快照值和只读投影。
- 边界角色：`snapshot/query`；最小 seam：tile snapshot view；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：2；字段：18；属性：1；合计：19。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（18）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2967 | field | Terraria.Utilities.TileSnapshot.TileStruct | Terraria.Utilities/TileSnapshot.cs | D:\TRbackup\Version4\Terraria.Utilities\TileSnapshot.cs | 19 | 3 | _type | ushort | `[FieldOffset(0)] private ushort _type;` | `[FieldOffset(0)] private ushort _type;` |
| 2968 | field | Terraria.Utilities.TileSnapshot.TileStruct | Terraria.Utilities/TileSnapshot.cs | D:\TRbackup\Version4\Terraria.Utilities\TileSnapshot.cs | 22 | 3 | _wall_bTileHeader3_packed | ushort | `[FieldOffset(2)] private ushort _wall_bTileHeader3_packed;` | `[FieldOffset(2)] private ushort _wall_bTileHeader3_packed;` |
| 2969 | field | Terraria.Utilities.TileSnapshot.TileStruct | Terraria.Utilities/TileSnapshot.cs | D:\TRbackup\Version4\Terraria.Utilities\TileSnapshot.cs | 25 | 3 | _sTileHeader | ushort | `[FieldOffset(4)] private ushort _sTileHeader;` | `[FieldOffset(4)] private ushort _sTileHeader;` |
| 2970 | field | Terraria.Utilities.TileSnapshot.TileStruct | Terraria.Utilities/TileSnapshot.cs | D:\TRbackup\Version4\Terraria.Utilities\TileSnapshot.cs | 28 | 3 | _frameX | short | `[FieldOffset(6)] private short _frameX;` | `[FieldOffset(6)] private short _frameX;` |
| 2971 | field | Terraria.Utilities.TileSnapshot.TileStruct | Terraria.Utilities/TileSnapshot.cs | D:\TRbackup\Version4\Terraria.Utilities\TileSnapshot.cs | 31 | 3 | _frameY | short | `[FieldOffset(8)] private short _frameY;` | `[FieldOffset(8)] private short _frameY;` |
| 2972 | field | Terraria.Utilities.TileSnapshot.TileStruct | Terraria.Utilities/TileSnapshot.cs | D:\TRbackup\Version4\Terraria.Utilities\TileSnapshot.cs | 34 | 3 | _liquid | byte | `[FieldOffset(10)] private byte _liquid;` | `[FieldOffset(10)] private byte _liquid;` |
| 2973 | field | Terraria.Utilities.TileSnapshot.TileStruct | Terraria.Utilities/TileSnapshot.cs | D:\TRbackup\Version4\Terraria.Utilities\TileSnapshot.cs | 37 | 3 | _bTileHeader | byte | `[FieldOffset(11)] private byte _bTileHeader;` | `[FieldOffset(11)] private byte _bTileHeader;` |
| 2974 | field | Terraria.Utilities.TileSnapshot.TileStruct | Terraria.Utilities/TileSnapshot.cs | D:\TRbackup\Version4\Terraria.Utilities\TileSnapshot.cs | 40 | 3 | _i0 | int | `[FieldOffset(0)] private int _i0;` | `[FieldOffset(0)] private int _i0;` |
| 2975 | field | Terraria.Utilities.TileSnapshot.TileStruct | Terraria.Utilities/TileSnapshot.cs | D:\TRbackup\Version4\Terraria.Utilities\TileSnapshot.cs | 43 | 3 | _i1 | int | `[FieldOffset(4)] private int _i1;` | `[FieldOffset(4)] private int _i1;` |
| 2976 | field | Terraria.Utilities.TileSnapshot.TileStruct | Terraria.Utilities/TileSnapshot.cs | D:\TRbackup\Version4\Terraria.Utilities\TileSnapshot.cs | 46 | 3 | _i2 | int | `[FieldOffset(8)] private int _i2;` | `[FieldOffset(8)] private int _i2;` |
| 2977 | field | Terraria.Utilities.TileSnapshot.TileStruct | Terraria.Utilities/TileSnapshot.cs | D:\TRbackup\Version4\Terraria.Utilities\TileSnapshot.cs | 49 | 3 | _liquidNames | string[] | `private static string[] _liquidNames = new string[4] { "water", "lava", "honey", "shimmer" };` | `private static string[] _liquidNames = new string[4] { "water", "lava", "honey", "shimmer" };` |
| 2978 | field | Terraria.Utilities.TileSnapshot | Terraria.Utilities/TileSnapshot.cs | D:\TRbackup\Version4\Terraria.Utilities\TileSnapshot.cs | 137 | 2 | _worldFile | Terraria.IO.WorldFileData | `private static WorldFileData _worldFile;` | `private static WorldFileData _worldFile;` |
| 2979 | field | Terraria.Utilities.TileSnapshot | Terraria.Utilities/TileSnapshot.cs | D:\TRbackup\Version4\Terraria.Utilities\TileSnapshot.cs | 139 | 2 | _tiles | Terraria.Utilities.TileSnapshot.TileStruct[] | `private static TileStruct[] _tiles;` | `private static TileStruct[] _tiles;` |
| 2980 | field | Terraria.Utilities.TileSnapshot | Terraria.Utilities/TileSnapshot.cs | D:\TRbackup\Version4\Terraria.Utilities\TileSnapshot.cs | 141 | 2 | _tileEntities | System.Collections.Generic.List<Terraria.DataStructures.TileEntity> | `private static List<TileEntity> _tileEntities;` | `private static List<TileEntity> _tileEntities;` |
| 2981 | field | Terraria.Utilities.TileSnapshot | Terraria.Utilities/TileSnapshot.cs | D:\TRbackup\Version4\Terraria.Utilities\TileSnapshot.cs | 143 | 2 | _chests | System.Collections.Generic.List<Terraria.Chest> | `private static List<Chest> _chests;` | `private static List<Chest> _chests;` |
| 2982 | field | Terraria.Utilities.TileSnapshot | Terraria.Utilities/TileSnapshot.cs | D:\TRbackup\Version4\Terraria.Utilities\TileSnapshot.cs | 145 | 2 | _tempStream | System.IO.MemoryStream | `private static MemoryStream _tempStream = new MemoryStream();` | `private static MemoryStream _tempStream = new MemoryStream();` |
| 2983 | field | Terraria.Utilities.TileSnapshot | Terraria.Utilities/TileSnapshot.cs | D:\TRbackup\Version4\Terraria.Utilities\TileSnapshot.cs | 147 | 2 | _tempWriter | System.IO.BinaryWriter | `private static BinaryWriter _tempWriter = new BinaryWriter(_tempStream);` | `private static BinaryWriter _tempWriter = new BinaryWriter(_tempStream);` |
| 2984 | field | Terraria.Utilities.TileSnapshot | Terraria.Utilities/TileSnapshot.cs | D:\TRbackup\Version4\Terraria.Utilities\TileSnapshot.cs | 149 | 2 | _tempReader | System.IO.BinaryReader | `private static BinaryReader _tempReader = new BinaryReader(_tempStream);` | `private static BinaryReader _tempReader = new BinaryReader(_tempStream);` |

#### 属性（1）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3941 | property | Terraria.Utilities.TileSnapshot | Terraria.Utilities/TileSnapshot.cs | D:\TRbackup\Version4\Terraria.Utilities\TileSnapshot.cs | 151 | 2 | Context | object | `public static object Context { get; private set; }` | `public static object Context { get; private set; }` |


### 4.13 细分子系统：`TileEntityRegistryAndBaseState`

- 原报告章节：`4.9.72`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`04` / `地块、液体与世界存储`

- 上一级基线细分子系统：`TileEntityRegistryAndBaseState`
- 细分职责：TileEntity 基类、注册表、类型标识和实体 ID。
- 边界角色：`state`；最小 seam：tile entity registry port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：3；声明类型数：3；字段：14；属性：0；合计：14。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（14）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1299 | field | Terraria.DataStructures.TileEntitiesManager | Terraria.DataStructures/TileEntitiesManager.cs | D:\TRbackup\Version4\Terraria.DataStructures\TileEntitiesManager.cs | 8 | 2 | _nextEntityID | int | `private int _nextEntityID;` | `private int _nextEntityID;` |
| 1300 | field | Terraria.DataStructures.TileEntitiesManager | Terraria.DataStructures/TileEntitiesManager.cs | D:\TRbackup\Version4\Terraria.DataStructures\TileEntitiesManager.cs | 10 | 2 | _types | System.Collections.Generic.Dictionary<int, Terraria.DataStructures.TileEntity> | `private Dictionary<int, TileEntity> _types = new Dictionary<int, TileEntity>();` | `private Dictionary<int, TileEntity> _types = new Dictionary<int, TileEntity>();` |
| 1301 | field | Terraria.DataStructures.TileEntity | Terraria.DataStructures/TileEntity.cs | D:\TRbackup\Version4\Terraria.DataStructures\TileEntity.cs | 13 | 2 | manager | Terraria.DataStructures.TileEntitiesManager | `public static TileEntitiesManager manager;` | `public static TileEntitiesManager manager;` |
| 1302 | field | Terraria.DataStructures.TileEntity | Terraria.DataStructures/TileEntity.cs | D:\TRbackup\Version4\Terraria.DataStructures\TileEntity.cs | 15 | 2 | MaxEntitiesPerChunk | int | `public const int MaxEntitiesPerChunk = 1000;` | `public const int MaxEntitiesPerChunk = 1000;` |
| 1303 | field | Terraria.DataStructures.TileEntity | Terraria.DataStructures/TileEntity.cs | D:\TRbackup\Version4\Terraria.DataStructures\TileEntity.cs | 17 | 2 | EntityCreationLock | object | `public static object EntityCreationLock = new object();` | `public static object EntityCreationLock = new object();` |
| 1304 | field | Terraria.DataStructures.TileEntity | Terraria.DataStructures/TileEntity.cs | D:\TRbackup\Version4\Terraria.DataStructures\TileEntity.cs | 19 | 2 | UpdateEntities | System.Collections.Generic.List<Terraria.DataStructures.TileEntity> | `public static List<TileEntity> UpdateEntities = new List<TileEntity>();` | `public static List<TileEntity> UpdateEntities = new List<TileEntity>();` |
| 1305 | field | Terraria.DataStructures.TileEntity | Terraria.DataStructures/TileEntity.cs | D:\TRbackup\Version4\Terraria.DataStructures\TileEntity.cs | 21 | 2 | ByID | System.Collections.Generic.Dictionary<int, Terraria.DataStructures.TileEntity> | `public static Dictionary<int, TileEntity> ByID = new Dictionary<int, TileEntity>();` | `public static Dictionary<int, TileEntity> ByID = new Dictionary<int, TileEntity>();` |
| 1306 | field | Terraria.DataStructures.TileEntity | Terraria.DataStructures/TileEntity.cs | D:\TRbackup\Version4\Terraria.DataStructures\TileEntity.cs | 23 | 2 | ByPosition | System.Collections.Generic.Dictionary<Terraria.DataStructures.Point16, Terraria.DataStructures.TileEntity> | `public static Dictionary<Point16, TileEntity> ByPosition = new Dictionary<Point16, TileEntity>();` | `public static Dictionary<Point16, TileEntity> ByPosition = new Dictionary<Point16, TileEntity>();` |
| 1307 | field | Terraria.DataStructures.TileEntity | Terraria.DataStructures/TileEntity.cs | D:\TRbackup\Version4\Terraria.DataStructures\TileEntity.cs | 25 | 2 | TileEntitiesNextID | int | `public static int TileEntitiesNextID;` | `public static int TileEntitiesNextID;` |
| 1308 | field | Terraria.DataStructures.TileEntity | Terraria.DataStructures/TileEntity.cs | D:\TRbackup\Version4\Terraria.DataStructures\TileEntity.cs | 27 | 2 | ID | int | `public int ID;` | `public int ID;` |
| 1309 | field | Terraria.DataStructures.TileEntity | Terraria.DataStructures/TileEntity.cs | D:\TRbackup\Version4\Terraria.DataStructures\TileEntity.cs | 29 | 2 | Position | Terraria.DataStructures.Point16 | `public Point16 Position;` | `public Point16 Position;` |
| 1310 | field | Terraria.DataStructures.TileEntity | Terraria.DataStructures/TileEntity.cs | D:\TRbackup\Version4\Terraria.DataStructures\TileEntity.cs | 31 | 2 | type | byte | `public byte type;` | `public byte type;` |
| 1311 | field | Terraria.DataStructures.TileEntity | Terraria.DataStructures/TileEntity.cs | D:\TRbackup\Version4\Terraria.DataStructures\TileEntity.cs | 33 | 2 | RequiresUpdates | bool | `public bool RequiresUpdates;` | `public bool RequiresUpdates;` |
| 1312 | field | Terraria.DataStructures.TileEntityType<T> | Terraria.DataStructures/TileEntityType.cs | D:\TRbackup\Version4\Terraria.DataStructures\TileEntityType.cs | 5 | 2 | EntityTypeID | byte | `protected static byte EntityTypeID;` | `protected static byte EntityTypeID;` |

#### 属性（0）

无该类型成员记录。


### 4.14 细分子系统：`TileEntityDisplayAndInventoryState`

- 原报告章节：`4.9.73`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`04` / `地块、液体与世界存储`

- 上一级基线细分子系统：`TileEntityDisplayAndInventoryState`
- 细分职责：展示架、物品框、食物盘和展示容器状态。
- 边界角色：`state/presentation`；最小 seam：tile entity display inventory port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：6；声明类型数：7；字段：24；属性：0；合计：24。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（24）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2191 | field | Terraria.GameContent.Tile_Entities.TEDeadCellsDisplayJar | Terraria.GameContent.Tile_Entities/TEDeadCellsDisplayJar.cs | D:\TRbackup\Version4\Terraria.GameContent.Tile_Entities\TEDeadCellsDisplayJar.cs | 8 | 2 | item | Terraria.Item | `public Item item;` | `public Item item;` |
| 2192 | field | Terraria.GameContent.Tile_Entities.TEDisplayDoll.DisplayDollPose | Terraria.GameContent.Tile_Entities/TEDisplayDoll.cs | D:\TRbackup\Version4\Terraria.GameContent.Tile_Entities\TEDisplayDoll.cs | 18 | 3 | Pose | Terraria.GameContent.Tile_Entities.DisplayDollPoseID | `public DisplayDollPoseID Pose;` | `public DisplayDollPoseID Pose;` |
| 2193 | field | Terraria.GameContent.Tile_Entities.TEDisplayDoll.DisplayDollPose | Terraria.GameContent.Tile_Entities/TEDisplayDoll.cs | D:\TRbackup\Version4\Terraria.GameContent.Tile_Entities\TEDisplayDoll.cs | 20 | 3 | ItemAnimationPercent | float | `public float ItemAnimationPercent;` | `public float ItemAnimationPercent;` |
| 2194 | field | Terraria.GameContent.Tile_Entities.TEDisplayDoll.DisplayDollPose | Terraria.GameContent.Tile_Entities/TEDisplayDoll.cs | D:\TRbackup\Version4\Terraria.GameContent.Tile_Entities\TEDisplayDoll.cs | 22 | 3 | ItemAimRadians | float? | `public float? ItemAimRadians;` | `public float? ItemAimRadians;` |
| 2195 | field | Terraria.GameContent.Tile_Entities.TEDisplayDoll | Terraria.GameContent.Tile_Entities/TEDisplayDoll.cs | D:\TRbackup\Version4\Terraria.GameContent.Tile_Entities\TEDisplayDoll.cs | 25 | 2 | MyTileID | int | `private const int MyTileID = 470;` | `private const int MyTileID = 470;` |
| 2196 | field | Terraria.GameContent.Tile_Entities.TEDisplayDoll | Terraria.GameContent.Tile_Entities/TEDisplayDoll.cs | D:\TRbackup\Version4\Terraria.GameContent.Tile_Entities\TEDisplayDoll.cs | 27 | 2 | entityTileWidth | int | `public const int entityTileWidth = 2;` | `public const int entityTileWidth = 2;` |
| 2197 | field | Terraria.GameContent.Tile_Entities.TEDisplayDoll | Terraria.GameContent.Tile_Entities/TEDisplayDoll.cs | D:\TRbackup\Version4\Terraria.GameContent.Tile_Entities\TEDisplayDoll.cs | 29 | 2 | entityTileHeight | int | `public const int entityTileHeight = 3;` | `public const int entityTileHeight = 3;` |
| 2198 | field | Terraria.GameContent.Tile_Entities.TEDisplayDoll | Terraria.GameContent.Tile_Entities/TEDisplayDoll.cs | D:\TRbackup\Version4\Terraria.GameContent.Tile_Entities\TEDisplayDoll.cs | 31 | 2 | _dollPlayer | Terraria.Player | `private Player _dollPlayer;` | `private Player _dollPlayer;` |
| 2199 | field | Terraria.GameContent.Tile_Entities.TEDisplayDoll | Terraria.GameContent.Tile_Entities/TEDisplayDoll.cs | D:\TRbackup\Version4\Terraria.GameContent.Tile_Entities\TEDisplayDoll.cs | 33 | 2 | _equip | Terraria.Item[] | `private Item[] _equip;` | `private Item[] _equip;` |
| 2200 | field | Terraria.GameContent.Tile_Entities.TEDisplayDoll | Terraria.GameContent.Tile_Entities/TEDisplayDoll.cs | D:\TRbackup\Version4\Terraria.GameContent.Tile_Entities\TEDisplayDoll.cs | 35 | 2 | _dyes | Terraria.Item[] | `private Item[] _dyes;` | `private Item[] _dyes;` |
| 2201 | field | Terraria.GameContent.Tile_Entities.TEDisplayDoll | Terraria.GameContent.Tile_Entities/TEDisplayDoll.cs | D:\TRbackup\Version4\Terraria.GameContent.Tile_Entities\TEDisplayDoll.cs | 37 | 2 | _misc | Terraria.Item[] | `private Item[] _misc;` | `private Item[] _misc;` |
| 2202 | field | Terraria.GameContent.Tile_Entities.TEDisplayDoll | Terraria.GameContent.Tile_Entities/TEDisplayDoll.cs | D:\TRbackup\Version4\Terraria.GameContent.Tile_Entities\TEDisplayDoll.cs | 39 | 2 | _pose | byte | `private byte _pose;` | `private byte _pose;` |
| 2203 | field | Terraria.GameContent.Tile_Entities.TEDisplayDoll | Terraria.GameContent.Tile_Entities/TEDisplayDoll.cs | D:\TRbackup\Version4\Terraria.GameContent.Tile_Entities\TEDisplayDoll.cs | 41 | 2 | SupportedUseStylePoses | System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<Terraria.GameContent.Tile_Entities.TEDisplayDoll.DisplayDollPose>> | `public static Dictionary<int, List<DisplayDollPose>> SupportedUseStylePoses;` | `public static Dictionary<int, List<DisplayDollPose>> SupportedUseStylePoses;` |
| 2204 | field | Terraria.GameContent.Tile_Entities.TEDisplayDoll | Terraria.GameContent.Tile_Entities/TEDisplayDoll.cs | D:\TRbackup\Version4\Terraria.GameContent.Tile_Entities\TEDisplayDoll.cs | 43 | 2 | _projectileDummy | Terraria.Projectile | `private static Projectile _projectileDummy;` | `private static Projectile _projectileDummy;` |
| 2205 | field | Terraria.GameContent.Tile_Entities.TEFoodPlatter | Terraria.GameContent.Tile_Entities/TEFoodPlatter.cs | D:\TRbackup\Version4\Terraria.GameContent.Tile_Entities\TEFoodPlatter.cs | 9 | 2 | item | Terraria.Item | `public Item item;` | `public Item item;` |
| 2206 | field | Terraria.GameContent.Tile_Entities.TEHatRack | Terraria.GameContent.Tile_Entities/TEHatRack.cs | D:\TRbackup\Version4\Terraria.GameContent.Tile_Entities\TEHatRack.cs | 13 | 2 | MyTileID | int | `private const int MyTileID = 475;` | `private const int MyTileID = 475;` |
| 2207 | field | Terraria.GameContent.Tile_Entities.TEHatRack | Terraria.GameContent.Tile_Entities/TEHatRack.cs | D:\TRbackup\Version4\Terraria.GameContent.Tile_Entities\TEHatRack.cs | 15 | 2 | entityTileWidth | int | `public const int entityTileWidth = 3;` | `public const int entityTileWidth = 3;` |
| 2208 | field | Terraria.GameContent.Tile_Entities.TEHatRack | Terraria.GameContent.Tile_Entities/TEHatRack.cs | D:\TRbackup\Version4\Terraria.GameContent.Tile_Entities\TEHatRack.cs | 17 | 2 | entityTileHeight | int | `public const int entityTileHeight = 4;` | `public const int entityTileHeight = 4;` |
| 2209 | field | Terraria.GameContent.Tile_Entities.TEHatRack | Terraria.GameContent.Tile_Entities/TEHatRack.cs | D:\TRbackup\Version4\Terraria.GameContent.Tile_Entities\TEHatRack.cs | 19 | 2 | _dollPlayer | Terraria.Player | `private Player _dollPlayer;` | `private Player _dollPlayer;` |
| 2210 | field | Terraria.GameContent.Tile_Entities.TEHatRack | Terraria.GameContent.Tile_Entities/TEHatRack.cs | D:\TRbackup\Version4\Terraria.GameContent.Tile_Entities\TEHatRack.cs | 21 | 2 | _items | Terraria.Item[] | `private Item[] _items;` | `private Item[] _items;` |
| 2211 | field | Terraria.GameContent.Tile_Entities.TEHatRack | Terraria.GameContent.Tile_Entities/TEHatRack.cs | D:\TRbackup\Version4\Terraria.GameContent.Tile_Entities\TEHatRack.cs | 23 | 2 | _dyes | Terraria.Item[] | `private Item[] _dyes;` | `private Item[] _dyes;` |
| 2212 | field | Terraria.GameContent.Tile_Entities.TEItemFrame | Terraria.GameContent.Tile_Entities/TEItemFrame.cs | D:\TRbackup\Version4\Terraria.GameContent.Tile_Entities\TEItemFrame.cs | 8 | 2 | item | Terraria.Item | `public Item item;` | `public Item item;` |
| 2231 | field | Terraria.GameContent.Tile_Entities.TEWeaponsRack | Terraria.GameContent.Tile_Entities/TEWeaponsRack.cs | D:\TRbackup\Version4\Terraria.GameContent.Tile_Entities\TEWeaponsRack.cs | 9 | 2 | item | Terraria.Item | `public Item item;` | `public Item item;` |
| 2232 | field | Terraria.GameContent.Tile_Entities.TEWeaponsRack | Terraria.GameContent.Tile_Entities/TEWeaponsRack.cs | D:\TRbackup\Version4\Terraria.GameContent.Tile_Entities\TEWeaponsRack.cs | 11 | 2 | MyTileID | int | `private const int MyTileID = 471;` | `private const int MyTileID = 471;` |

#### 属性（0）

无该类型成员记录。


### 4.15 细分子系统：`TileEntityAnchorAndSensorState`

- 原报告章节：`4.9.74`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`04` / `地块、液体与世界存储`

- 上一级基线细分子系统：`TileEntityAnchorAndSensorState`
- 细分职责：逻辑传感器和生物/风筝锚点状态。
- 边界角色：`state/query`；最小 seam：tile entity anchor sensor port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：3；声明类型数：3；字段：11；属性：0；合计：11。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（11）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2189 | field | Terraria.GameContent.Tile_Entities.TECritterAnchor | Terraria.GameContent.Tile_Entities/TECritterAnchor.cs | D:\TRbackup\Version4\Terraria.GameContent.Tile_Entities\TECritterAnchor.cs | 9 | 2 | _myEntityID | byte | `private static byte _myEntityID;` | `private static byte _myEntityID;` |
| 2190 | field | Terraria.GameContent.Tile_Entities.TECritterAnchor | Terraria.GameContent.Tile_Entities/TECritterAnchor.cs | D:\TRbackup\Version4\Terraria.GameContent.Tile_Entities\TECritterAnchor.cs | 11 | 2 | CritterPrototypes | Terraria.GameContent.LeashedEntities.LeashedCritter[] | `public static LeashedCritter[] CritterPrototypes;` | `public static LeashedCritter[] CritterPrototypes;` |
| 2213 | field | Terraria.GameContent.Tile_Entities.TEKiteAnchor | Terraria.GameContent.Tile_Entities/TEKiteAnchor.cs | D:\TRbackup\Version4\Terraria.GameContent.Tile_Entities\TEKiteAnchor.cs | 9 | 2 | _myEntityID | byte | `private static byte _myEntityID;` | `private static byte _myEntityID;` |
| 2215 | field | Terraria.GameContent.Tile_Entities.TELogicSensor | Terraria.GameContent.Tile_Entities/TELogicSensor.cs | D:\TRbackup\Version4\Terraria.GameContent.Tile_Entities\TELogicSensor.cs | 23 | 2 | playerBox | System.Collections.Generic.Dictionary<int, Rectangle> | `private static Dictionary<int, Rectangle> playerBox = new Dictionary<int, Rectangle>();` | `private static Dictionary<int, Rectangle> playerBox = new Dictionary<int, Rectangle>();` |
| 2216 | field | Terraria.GameContent.Tile_Entities.TELogicSensor | Terraria.GameContent.Tile_Entities/TELogicSensor.cs | D:\TRbackup\Version4\Terraria.GameContent.Tile_Entities\TELogicSensor.cs | 25 | 2 | tripPoints | System.Collections.Generic.List<System.Tuple<Terraria.DataStructures.Point16, bool>> | `private static List<Tuple<Point16, bool>> tripPoints = new List<Tuple<Point16, bool>>();` | `private static List<Tuple<Point16, bool>> tripPoints = new List<Tuple<Point16, bool>>();` |
| 2217 | field | Terraria.GameContent.Tile_Entities.TELogicSensor | Terraria.GameContent.Tile_Entities/TELogicSensor.cs | D:\TRbackup\Version4\Terraria.GameContent.Tile_Entities\TELogicSensor.cs | 27 | 2 | markedIDsForRemoval | System.Collections.Generic.List<int> | `private static List<int> markedIDsForRemoval = new List<int>();` | `private static List<int> markedIDsForRemoval = new List<int>();` |
| 2218 | field | Terraria.GameContent.Tile_Entities.TELogicSensor | Terraria.GameContent.Tile_Entities/TELogicSensor.cs | D:\TRbackup\Version4\Terraria.GameContent.Tile_Entities\TELogicSensor.cs | 29 | 2 | inUpdateLoop | bool | `private static bool inUpdateLoop;` | `private static bool inUpdateLoop;` |
| 2219 | field | Terraria.GameContent.Tile_Entities.TELogicSensor | Terraria.GameContent.Tile_Entities/TELogicSensor.cs | D:\TRbackup\Version4\Terraria.GameContent.Tile_Entities\TELogicSensor.cs | 31 | 2 | playerBoxFilled | bool | `private static bool playerBoxFilled;` | `private static bool playerBoxFilled;` |
| 2220 | field | Terraria.GameContent.Tile_Entities.TELogicSensor | Terraria.GameContent.Tile_Entities/TELogicSensor.cs | D:\TRbackup\Version4\Terraria.GameContent.Tile_Entities\TELogicSensor.cs | 33 | 2 | logicCheck | Terraria.GameContent.Tile_Entities.TELogicSensor.LogicCheckType | `public LogicCheckType logicCheck;` | `public LogicCheckType logicCheck;` |
| 2221 | field | Terraria.GameContent.Tile_Entities.TELogicSensor | Terraria.GameContent.Tile_Entities/TELogicSensor.cs | D:\TRbackup\Version4\Terraria.GameContent.Tile_Entities\TELogicSensor.cs | 35 | 2 | On | bool | `public bool On;` | `public bool On;` |
| 2222 | field | Terraria.GameContent.Tile_Entities.TELogicSensor | Terraria.GameContent.Tile_Entities/TELogicSensor.cs | D:\TRbackup\Version4\Terraria.GameContent.Tile_Entities\TELogicSensor.cs | 37 | 2 | CountedData | int | `public int CountedData;` | `public int CountedData;` |

#### 属性（0）

无该类型成员记录。


### 4.16 细分子系统：`TileEntityWorldInteractionState`

- 原报告章节：`4.9.75`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`04` / `地块、液体与世界存储`

- 上一级基线细分子系统：`TileEntityWorldInteractionState`
- 细分职责：训练假人、传送晶塔和带物品的实体锚点交互状态。
- 边界角色：`state/query`；最小 seam：tile entity interaction port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：3；声明类型数：3；字段：9；属性：0；合计：9。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（9）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2214 | field | Terraria.GameContent.Tile_Entities.TELeashedEntityAnchorWithItem | Terraria.GameContent.Tile_Entities/TELeashedEntityAnchorWithItem.cs | D:\TRbackup\Version4\Terraria.GameContent.Tile_Entities\TELeashedEntityAnchorWithItem.cs | 8 | 2 | itemType | int | `protected int itemType;` | `protected int itemType;` |
| 2223 | field | Terraria.GameContent.Tile_Entities.TETeleportationPylon | Terraria.GameContent.Tile_Entities/TETeleportationPylon.cs | D:\TRbackup\Version4\Terraria.GameContent.Tile_Entities\TETeleportationPylon.cs | 7 | 2 | MyTileID | int | `private const int MyTileID = 597;` | `private const int MyTileID = 597;` |
| 2224 | field | Terraria.GameContent.Tile_Entities.TETeleportationPylon | Terraria.GameContent.Tile_Entities/TETeleportationPylon.cs | D:\TRbackup\Version4\Terraria.GameContent.Tile_Entities\TETeleportationPylon.cs | 9 | 2 | entityTileWidth | int | `public const int entityTileWidth = 3;` | `public const int entityTileWidth = 3;` |
| 2225 | field | Terraria.GameContent.Tile_Entities.TETeleportationPylon | Terraria.GameContent.Tile_Entities/TETeleportationPylon.cs | D:\TRbackup\Version4\Terraria.GameContent.Tile_Entities\TETeleportationPylon.cs | 11 | 2 | entityTileHeight | int | `public const int entityTileHeight = 4;` | `public const int entityTileHeight = 4;` |
| 2226 | field | Terraria.GameContent.Tile_Entities.TETrainingDummy | Terraria.GameContent.Tile_Entities/TETrainingDummy.cs | D:\TRbackup\Version4\Terraria.GameContent.Tile_Entities\TETrainingDummy.cs | 10 | 2 | playerBoxes | System.Collections.Generic.List<Rectangle> | `private static List<Rectangle> playerBoxes = new List<Rectangle>();` | `private static List<Rectangle> playerBoxes = new List<Rectangle>();` |
| 2227 | field | Terraria.GameContent.Tile_Entities.TETrainingDummy | Terraria.GameContent.Tile_Entities/TETrainingDummy.cs | D:\TRbackup\Version4\Terraria.GameContent.Tile_Entities\TETrainingDummy.cs | 12 | 2 | playerBoxFilled | bool | `private static bool playerBoxFilled;` | `private static bool playerBoxFilled;` |
| 2228 | field | Terraria.GameContent.Tile_Entities.TETrainingDummy | Terraria.GameContent.Tile_Entities/TETrainingDummy.cs | D:\TRbackup\Version4\Terraria.GameContent.Tile_Entities\TETrainingDummy.cs | 14 | 2 | npcSlotsFull | bool | `private static bool npcSlotsFull;` | `private static bool npcSlotsFull;` |
| 2229 | field | Terraria.GameContent.Tile_Entities.TETrainingDummy | Terraria.GameContent.Tile_Entities/TETrainingDummy.cs | D:\TRbackup\Version4\Terraria.GameContent.Tile_Entities\TETrainingDummy.cs | 16 | 2 | npc | int | `public int npc;` | `public int npc;` |
| 2230 | field | Terraria.GameContent.Tile_Entities.TETrainingDummy | Terraria.GameContent.Tile_Entities/TETrainingDummy.cs | D:\TRbackup\Version4\Terraria.GameContent.Tile_Entities\TETrainingDummy.cs | 18 | 2 | activationRetryCooldown | int | `public int activationRetryCooldown;` | `public int activationRetryCooldown;` |

#### 属性（0）

无该类型成员记录。


### 4.17 细分子系统：`SharedTilePlacementAnchorAndHookModules`

- 原报告章节：`4.9.121`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`04` / `地块、液体与世界存储`

- 上一级基线细分子系统：`SharedTilePlacementModules`
- 细分职责：锚点、液体规则、交替 Tile 和放置 hook 模块。
- 边界角色：`definition/query`；最小 seam：tile placement anchor hook port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：7；声明类型数：7；字段：20；属性：0；合计：20。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（20）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2792 | field | Terraria.Modules.AnchorDataModule | Terraria.Modules/AnchorDataModule.cs | D:\TRbackup\Version4\Terraria.Modules\AnchorDataModule.cs | 7 | 2 | top | Terraria.DataStructures.AnchorData | `public AnchorData top;` | `public AnchorData top;` |
| 2793 | field | Terraria.Modules.AnchorDataModule | Terraria.Modules/AnchorDataModule.cs | D:\TRbackup\Version4\Terraria.Modules\AnchorDataModule.cs | 9 | 2 | bottom | Terraria.DataStructures.AnchorData | `public AnchorData bottom;` | `public AnchorData bottom;` |
| 2794 | field | Terraria.Modules.AnchorDataModule | Terraria.Modules/AnchorDataModule.cs | D:\TRbackup\Version4\Terraria.Modules\AnchorDataModule.cs | 11 | 2 | left | Terraria.DataStructures.AnchorData | `public AnchorData left;` | `public AnchorData left;` |
| 2795 | field | Terraria.Modules.AnchorDataModule | Terraria.Modules/AnchorDataModule.cs | D:\TRbackup\Version4\Terraria.Modules\AnchorDataModule.cs | 13 | 2 | right | Terraria.DataStructures.AnchorData | `public AnchorData right;` | `public AnchorData right;` |
| 2796 | field | Terraria.Modules.AnchorDataModule | Terraria.Modules/AnchorDataModule.cs | D:\TRbackup\Version4\Terraria.Modules\AnchorDataModule.cs | 15 | 2 | wall | bool | `public bool wall;` | `public bool wall;` |
| 2797 | field | Terraria.Modules.AnchorTypesModule | Terraria.Modules/AnchorTypesModule.cs | D:\TRbackup\Version4\Terraria.Modules\AnchorTypesModule.cs | 7 | 2 | tileValid | int[] | `public int[] tileValid;` | `public int[] tileValid;` |
| 2798 | field | Terraria.Modules.AnchorTypesModule | Terraria.Modules/AnchorTypesModule.cs | D:\TRbackup\Version4\Terraria.Modules\AnchorTypesModule.cs | 9 | 2 | tileInvalid | int[] | `public int[] tileInvalid;` | `public int[] tileInvalid;` |
| 2799 | field | Terraria.Modules.AnchorTypesModule | Terraria.Modules/AnchorTypesModule.cs | D:\TRbackup\Version4\Terraria.Modules\AnchorTypesModule.cs | 11 | 2 | tileAlternates | int[] | `public int[] tileAlternates;` | `public int[] tileAlternates;` |
| 2800 | field | Terraria.Modules.AnchorTypesModule | Terraria.Modules/AnchorTypesModule.cs | D:\TRbackup\Version4\Terraria.Modules\AnchorTypesModule.cs | 13 | 2 | wallValid | int[] | `public int[] wallValid;` | `public int[] wallValid;` |
| 2801 | field | Terraria.Modules.LiquidDeathModule | Terraria.Modules/LiquidDeathModule.cs | D:\TRbackup\Version4\Terraria.Modules\LiquidDeathModule.cs | 5 | 2 | water | bool | `public bool water;` | `public bool water;` |
| 2802 | field | Terraria.Modules.LiquidDeathModule | Terraria.Modules/LiquidDeathModule.cs | D:\TRbackup\Version4\Terraria.Modules\LiquidDeathModule.cs | 7 | 2 | lava | bool | `public bool lava;` | `public bool lava;` |
| 2803 | field | Terraria.Modules.LiquidPlacementModule | Terraria.Modules/LiquidPlacementModule.cs | D:\TRbackup\Version4\Terraria.Modules\LiquidPlacementModule.cs | 7 | 2 | water | Terraria.Enums.LiquidPlacement | `public LiquidPlacement water;` | `public LiquidPlacement water;` |
| 2804 | field | Terraria.Modules.LiquidPlacementModule | Terraria.Modules/LiquidPlacementModule.cs | D:\TRbackup\Version4\Terraria.Modules\LiquidPlacementModule.cs | 9 | 2 | lava | Terraria.Enums.LiquidPlacement | `public LiquidPlacement lava;` | `public LiquidPlacement lava;` |
| 2805 | field | Terraria.Modules.TileObjectAlternatesModule | Terraria.Modules/TileObjectAlternatesModule.cs | D:\TRbackup\Version4\Terraria.Modules\TileObjectAlternatesModule.cs | 8 | 2 | data | System.Collections.Generic.List<Terraria.ObjectData.TileObjectData> | `public List<TileObjectData> data;` | `public List<TileObjectData> data;` |
| 2834 | field | Terraria.Modules.TileObjectSubTilesModule | Terraria.Modules/TileObjectSubTilesModule.cs | D:\TRbackup\Version4\Terraria.Modules\TileObjectSubTilesModule.cs | 8 | 2 | data | System.Collections.Generic.List<Terraria.ObjectData.TileObjectData> | `public List<TileObjectData> data;` | `public List<TileObjectData> data;` |
| 2835 | field | Terraria.Modules.TilePlacementHooksModule | Terraria.Modules/TilePlacementHooksModule.cs | D:\TRbackup\Version4\Terraria.Modules\TilePlacementHooksModule.cs | 7 | 2 | check | Terraria.DataStructures.PlacementHook | `public PlacementHook check;` | `public PlacementHook check;` |
| 2836 | field | Terraria.Modules.TilePlacementHooksModule | Terraria.Modules/TilePlacementHooksModule.cs | D:\TRbackup\Version4\Terraria.Modules\TilePlacementHooksModule.cs | 9 | 2 | postPlaceEveryone | Terraria.DataStructures.PlacementHook | `public PlacementHook postPlaceEveryone;` | `public PlacementHook postPlaceEveryone;` |
| 2837 | field | Terraria.Modules.TilePlacementHooksModule | Terraria.Modules/TilePlacementHooksModule.cs | D:\TRbackup\Version4\Terraria.Modules\TilePlacementHooksModule.cs | 11 | 2 | postPlaceMyPlayer | Terraria.DataStructures.PlacementHook | `public PlacementHook postPlaceMyPlayer;` | `public PlacementHook postPlaceMyPlayer;` |
| 2838 | field | Terraria.Modules.TilePlacementHooksModule | Terraria.Modules/TilePlacementHooksModule.cs | D:\TRbackup\Version4\Terraria.Modules\TilePlacementHooksModule.cs | 13 | 2 | placeOverride | Terraria.DataStructures.PlacementHook | `public PlacementHook placeOverride;` | `public PlacementHook placeOverride;` |
| 2839 | field | Terraria.Modules.TilePlacementHooksModule | Terraria.Modules/TilePlacementHooksModule.cs | D:\TRbackup\Version4\Terraria.Modules\TilePlacementHooksModule.cs | 15 | 2 | getStyleMethod | Terraria.DataStructures.GetStyleMethod | `public GetStyleMethod getStyleMethod;` | `public GetStyleMethod getStyleMethod;` |

#### 属性（0）

无该类型成员记录。


### 4.18 细分子系统：`SharedTileObjectPreviewState`

- 原报告章节：`4.9.128`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`04` / `地块、液体与世界存储`

- 上一级基线细分子系统：`SharedTileObjectPlacementDefinitions`
- 细分职责：TileObject 放置预览、缓存和有效性百分比。
- 边界角色：`query/snapshot`；最小 seam：tile object preview port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：16；属性：9；合计：25。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（16）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1313 | field | Terraria.DataStructures.TileObjectPreviewData | Terraria.DataStructures/TileObjectPreviewData.cs | D:\TRbackup\Version4\Terraria.DataStructures\TileObjectPreviewData.cs | 7 | 2 | _type | ushort | `private ushort _type;` | `private ushort _type;` |
| 1314 | field | Terraria.DataStructures.TileObjectPreviewData | Terraria.DataStructures/TileObjectPreviewData.cs | D:\TRbackup\Version4\Terraria.DataStructures\TileObjectPreviewData.cs | 9 | 2 | _style | short | `private short _style;` | `private short _style;` |
| 1315 | field | Terraria.DataStructures.TileObjectPreviewData | Terraria.DataStructures/TileObjectPreviewData.cs | D:\TRbackup\Version4\Terraria.DataStructures\TileObjectPreviewData.cs | 11 | 2 | _alternate | int | `private int _alternate;` | `private int _alternate;` |
| 1316 | field | Terraria.DataStructures.TileObjectPreviewData | Terraria.DataStructures/TileObjectPreviewData.cs | D:\TRbackup\Version4\Terraria.DataStructures\TileObjectPreviewData.cs | 13 | 2 | _random | int | `private int _random;` | `private int _random;` |
| 1317 | field | Terraria.DataStructures.TileObjectPreviewData | Terraria.DataStructures/TileObjectPreviewData.cs | D:\TRbackup\Version4\Terraria.DataStructures\TileObjectPreviewData.cs | 15 | 2 | _active | bool | `private bool _active;` | `private bool _active;` |
| 1318 | field | Terraria.DataStructures.TileObjectPreviewData | Terraria.DataStructures/TileObjectPreviewData.cs | D:\TRbackup\Version4\Terraria.DataStructures\TileObjectPreviewData.cs | 17 | 2 | _size | Terraria.DataStructures.Point16 | `private Point16 _size;` | `private Point16 _size;` |
| 1319 | field | Terraria.DataStructures.TileObjectPreviewData | Terraria.DataStructures/TileObjectPreviewData.cs | D:\TRbackup\Version4\Terraria.DataStructures\TileObjectPreviewData.cs | 19 | 2 | _coordinates | Terraria.DataStructures.Point16 | `private Point16 _coordinates;` | `private Point16 _coordinates;` |
| 1320 | field | Terraria.DataStructures.TileObjectPreviewData | Terraria.DataStructures/TileObjectPreviewData.cs | D:\TRbackup\Version4\Terraria.DataStructures\TileObjectPreviewData.cs | 21 | 2 | _objectStart | Terraria.DataStructures.Point16 | `private Point16 _objectStart;` | `private Point16 _objectStart;` |
| 1321 | field | Terraria.DataStructures.TileObjectPreviewData | Terraria.DataStructures/TileObjectPreviewData.cs | D:\TRbackup\Version4\Terraria.DataStructures\TileObjectPreviewData.cs | 23 | 2 | _data | int[,] | `private int[,] _data;` | `private int[,] _data;` |
| 1322 | field | Terraria.DataStructures.TileObjectPreviewData | Terraria.DataStructures/TileObjectPreviewData.cs | D:\TRbackup\Version4\Terraria.DataStructures\TileObjectPreviewData.cs | 25 | 2 | _dataSize | Terraria.DataStructures.Point16 | `private Point16 _dataSize;` | `private Point16 _dataSize;` |
| 1323 | field | Terraria.DataStructures.TileObjectPreviewData | Terraria.DataStructures/TileObjectPreviewData.cs | D:\TRbackup\Version4\Terraria.DataStructures\TileObjectPreviewData.cs | 27 | 2 | _percentValid | float | `private float _percentValid;` | `private float _percentValid;` |
| 1324 | field | Terraria.DataStructures.TileObjectPreviewData | Terraria.DataStructures/TileObjectPreviewData.cs | D:\TRbackup\Version4\Terraria.DataStructures\TileObjectPreviewData.cs | 29 | 2 | placementCache | Terraria.DataStructures.TileObjectPreviewData | `public static TileObjectPreviewData placementCache;` | `public static TileObjectPreviewData placementCache;` |
| 1325 | field | Terraria.DataStructures.TileObjectPreviewData | Terraria.DataStructures/TileObjectPreviewData.cs | D:\TRbackup\Version4\Terraria.DataStructures\TileObjectPreviewData.cs | 31 | 2 | randomCache | Terraria.DataStructures.TileObjectPreviewData | `public static TileObjectPreviewData randomCache;` | `public static TileObjectPreviewData randomCache;` |
| 1326 | field | Terraria.DataStructures.TileObjectPreviewData | Terraria.DataStructures/TileObjectPreviewData.cs | D:\TRbackup\Version4\Terraria.DataStructures\TileObjectPreviewData.cs | 33 | 2 | None | int | `public const int None = 0;` | `public const int None = 0;` |
| 1327 | field | Terraria.DataStructures.TileObjectPreviewData | Terraria.DataStructures/TileObjectPreviewData.cs | D:\TRbackup\Version4\Terraria.DataStructures\TileObjectPreviewData.cs | 35 | 2 | ValidSpot | int | `public const int ValidSpot = 1;` | `public const int ValidSpot = 1;` |
| 1328 | field | Terraria.DataStructures.TileObjectPreviewData | Terraria.DataStructures/TileObjectPreviewData.cs | D:\TRbackup\Version4\Terraria.DataStructures\TileObjectPreviewData.cs | 37 | 2 | InvalidSpot | int | `public const int InvalidSpot = 2;` | `public const int InvalidSpot = 2;` |

#### 属性（9）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3702 | property | Terraria.DataStructures.TileObjectPreviewData | Terraria.DataStructures/TileObjectPreviewData.cs | D:\TRbackup\Version4\Terraria.DataStructures\TileObjectPreviewData.cs | 39 | 2 | Active | bool | `public bool Active { get { return _active; } set { _active = value; } }` | `public bool Active { get { return _active; } set { _active = value; } }` |
| 3703 | property | Terraria.DataStructures.TileObjectPreviewData | Terraria.DataStructures/TileObjectPreviewData.cs | D:\TRbackup\Version4\Terraria.DataStructures\TileObjectPreviewData.cs | 51 | 2 | Type | ushort | `public ushort Type { get { return _type; } set { _type = value; } }` | `public ushort Type { get { return _type; } set { _type = value; } }` |
| 3704 | property | Terraria.DataStructures.TileObjectPreviewData | Terraria.DataStructures/TileObjectPreviewData.cs | D:\TRbackup\Version4\Terraria.DataStructures\TileObjectPreviewData.cs | 63 | 2 | Style | short | `public short Style { get { return _style; } set { _style = value; } }` | `public short Style { get { return _style; } set { _style = value; } }` |
| 3705 | property | Terraria.DataStructures.TileObjectPreviewData | Terraria.DataStructures/TileObjectPreviewData.cs | D:\TRbackup\Version4\Terraria.DataStructures\TileObjectPreviewData.cs | 75 | 2 | Alternate | int | `public int Alternate { get { return _alternate; } set { _alternate = value; } }` | `public int Alternate { get { return _alternate; } set { _alternate = value; } }` |
| 3706 | property | Terraria.DataStructures.TileObjectPreviewData | Terraria.DataStructures/TileObjectPreviewData.cs | D:\TRbackup\Version4\Terraria.DataStructures\TileObjectPreviewData.cs | 87 | 2 | Random | int | `public int Random { get { return _random; } set { _random = value; } }` | `public int Random { get { return _random; } set { _random = value; } }` |
| 3707 | property | Terraria.DataStructures.TileObjectPreviewData | Terraria.DataStructures/TileObjectPreviewData.cs | D:\TRbackup\Version4\Terraria.DataStructures\TileObjectPreviewData.cs | 99 | 2 | Size | Terraria.DataStructures.Point16 | `public Point16 Size { get { return _size; } set { if (value.X <= 0 \|\| value.Y <= 0) { throw new FormatException("PlacementData.Size was set to a negative value."); } if (value.X > _dataSize.X \|\| value.Y > _dataSize.Y) { int num = ((value.X > _dataSize.X) ? value.X : _dataSize.X); int num2 = ((value.Y > _dataSize.Y) ? value.Y : _dataSize.Y); int[,] array = new int[num, num2]; if (_data != null) { for (int i = 0; i < _dataSize.X; i++) { for (int j = 0; j < _dataSize.Y; j++) { array[i, j] = _data[i, j]; } } } _data = array; _dataSize = new Point16(num, num2); } _size = value; } }` | `public Point16 Size { get { return _size; } set { if (value.X <= 0 \|\| value.Y <= 0) { throw new FormatException("PlacementData.Size was set to a negative value."); } if (value.X > _dataSize.X \|\| value.Y > _dataSize.Y) { int num = ((value.X > _dataSize.X) ? value.X : _dataSize.X); int num2 = ((value.Y > _dataSize.Y) ? value.Y : _dataSize.Y); int[,] array = new int[num, num2]; if (_data != null) { for (int i = 0; i < _dataSize.X; i++) { for (int j = 0; j < _dataSize.Y; j++) { array[i, j] = _data[i, j]; } } } _data = array; _dataSize = new Point16(num, num2); } _size = value; } }` |
| 3708 | property | Terraria.DataStructures.TileObjectPreviewData | Terraria.DataStructures/TileObjectPreviewData.cs | D:\TRbackup\Version4\Terraria.DataStructures\TileObjectPreviewData.cs | 133 | 2 | Coordinates | Terraria.DataStructures.Point16 | `public Point16 Coordinates { get { return _coordinates; } set { _coordinates = value; } }` | `public Point16 Coordinates { get { return _coordinates; } set { _coordinates = value; } }` |
| 3709 | property | Terraria.DataStructures.TileObjectPreviewData | Terraria.DataStructures/TileObjectPreviewData.cs | D:\TRbackup\Version4\Terraria.DataStructures\TileObjectPreviewData.cs | 145 | 2 | ObjectStart | Terraria.DataStructures.Point16 | `public Point16 ObjectStart { get { return _objectStart; } set { _objectStart = value; } }` | `public Point16 ObjectStart { get { return _objectStart; } set { _objectStart = value; } }` |
| 3710 | property | Terraria.DataStructures.TileObjectPreviewData | Terraria.DataStructures/TileObjectPreviewData.cs | D:\TRbackup\Version4\Terraria.DataStructures\TileObjectPreviewData.cs | 157 | 2 | this[] | int | `public int this[int x, int y] { get { if (x < 0 \|\| y < 0 \|\| x >= _size.X \|\| y >= _size.Y) { throw new IndexOutOfRangeException(); } return _data[x, y]; } set { if (x < 0 \|\| y < 0 \|\| x >= _size.X \|\| y >= _size.Y) { throw new IndexOutOfRangeException(); } _data[x, y] = value; } }` | `public int this[int x, int y] { get { if (x < 0 \|\| y < 0 \|\| x >= _size.X \|\| y >= _size.Y) { throw new IndexOutOfRangeException(); } return _data[x, y]; } set { if (x < 0 \|\| y < 0 \|\| x >= _size.X \|\| y >= _size.Y) { throw new IndexOutOfRangeException(); } _data[x, y] = value; } }` |


### 4.19 细分子系统：`SharedTileObjectPlacementValueState`

- 原报告章节：`4.9.129`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`04` / `地块、液体与世界存储`

- 上一级基线细分子系统：`SharedTileObjectPlacementDefinitions`
- 细分职责：TileObject 坐标、样式、placement hook 和放置结果值。
- 边界角色：`value object/query`；最小 seam：tile object placement value port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：3；声明类型数：3；字段：16；属性：0；合计：16。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（16）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1208 | field | Terraria.DataStructures.PlacementDetails | Terraria.DataStructures/PlacementDetails.cs | D:\TRbackup\Version4\Terraria.DataStructures\PlacementDetails.cs | 5 | 2 | tileType | int | `public int tileType;` | `public int tileType;` |
| 1209 | field | Terraria.DataStructures.PlacementDetails | Terraria.DataStructures/PlacementDetails.cs | D:\TRbackup\Version4\Terraria.DataStructures\PlacementDetails.cs | 7 | 2 | tileStyle | short | `public short tileStyle;` | `public short tileStyle;` |
| 1210 | field | Terraria.DataStructures.PlacementHook | Terraria.DataStructures/PlacementHook.cs | D:\TRbackup\Version4\Terraria.DataStructures\PlacementHook.cs | 7 | 2 | hook | Terraria.DataStructures.PlacementHook.HookFormat | `public HookFormat hook;` | `public HookFormat hook;` |
| 1211 | field | Terraria.DataStructures.PlacementHook | Terraria.DataStructures/PlacementHook.cs | D:\TRbackup\Version4\Terraria.DataStructures\PlacementHook.cs | 9 | 2 | badReturn | int | `public int badReturn;` | `public int badReturn;` |
| 1212 | field | Terraria.DataStructures.PlacementHook | Terraria.DataStructures/PlacementHook.cs | D:\TRbackup\Version4\Terraria.DataStructures\PlacementHook.cs | 11 | 2 | badResponse | int | `public int badResponse;` | `public int badResponse;` |
| 1213 | field | Terraria.DataStructures.PlacementHook | Terraria.DataStructures/PlacementHook.cs | D:\TRbackup\Version4\Terraria.DataStructures\PlacementHook.cs | 13 | 2 | processedCoordinates | bool | `public bool processedCoordinates;` | `public bool processedCoordinates;` |
| 1214 | field | Terraria.DataStructures.PlacementHook | Terraria.DataStructures/PlacementHook.cs | D:\TRbackup\Version4\Terraria.DataStructures\PlacementHook.cs | 15 | 2 | Empty | Terraria.DataStructures.PlacementHook | `public static PlacementHook Empty = new PlacementHook(null, 0, 0, processedCoordinates: false);` | `public static PlacementHook Empty = new PlacementHook(null, 0, 0, processedCoordinates: false);` |
| 1215 | field | Terraria.DataStructures.PlacementHook | Terraria.DataStructures/PlacementHook.cs | D:\TRbackup\Version4\Terraria.DataStructures\PlacementHook.cs | 17 | 2 | Response_AllInvalid | int | `public const int Response_AllInvalid = 0;` | `public const int Response_AllInvalid = 0;` |
| 3509 | field | Terraria.TileObject | Terraria/TileObject.cs | D:\TRbackup\Version4\Terraria\TileObject.cs | 13 | 2 | xCoord | int | `public int xCoord;` | `public int xCoord;` |
| 3510 | field | Terraria.TileObject | Terraria/TileObject.cs | D:\TRbackup\Version4\Terraria\TileObject.cs | 15 | 2 | yCoord | int | `public int yCoord;` | `public int yCoord;` |
| 3511 | field | Terraria.TileObject | Terraria/TileObject.cs | D:\TRbackup\Version4\Terraria\TileObject.cs | 17 | 2 | type | int | `public int type;` | `public int type;` |
| 3512 | field | Terraria.TileObject | Terraria/TileObject.cs | D:\TRbackup\Version4\Terraria\TileObject.cs | 19 | 2 | style | int | `public int style;` | `public int style;` |
| 3513 | field | Terraria.TileObject | Terraria/TileObject.cs | D:\TRbackup\Version4\Terraria\TileObject.cs | 21 | 2 | alternate | int | `public int alternate;` | `public int alternate;` |
| 3514 | field | Terraria.TileObject | Terraria/TileObject.cs | D:\TRbackup\Version4\Terraria\TileObject.cs | 23 | 2 | random | int | `public int random;` | `public int random;` |
| 3515 | field | Terraria.TileObject | Terraria/TileObject.cs | D:\TRbackup\Version4\Terraria\TileObject.cs | 25 | 2 | Empty | Terraria.TileObject | `public static TileObject Empty = default(TileObject);` | `public static TileObject Empty = default(TileObject);` |
| 3516 | field | Terraria.TileObject | Terraria/TileObject.cs | D:\TRbackup\Version4\Terraria\TileObject.cs | 27 | 2 | objectPreview | Terraria.DataStructures.TileObjectPreviewData | `public static TileObjectPreviewData objectPreview = new TileObjectPreviewData();` | `public static TileObjectPreviewData objectPreview = new TileObjectPreviewData();` |

#### 属性（0）

无该类型成员记录。


### 4.20 细分子系统：`SharedTilePlacementCoordinateAndDrawModules`

- 原报告章节：`4.9.186`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`04` / `地块、液体与世界存储`

- 上一级基线细分子系统：`SharedTilePlacementModules`
- 上一级 peer 细分子系统：`SharedTilePlacementGeometryModules`
- 细分职责：Tile 放置坐标、绘制模块和几何输出。
- 边界角色：`definition/query`；最小 seam：tile placement coordinate draw port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：2；声明类型数：2；字段：14；属性：0；合计：14。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（14）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2813 | field | Terraria.Modules.TileObjectCoordinatesModule | Terraria.Modules/TileObjectCoordinatesModule.cs | D:\TRbackup\Version4\Terraria.Modules\TileObjectCoordinatesModule.cs | 9 | 2 | width | int | `public int width;` | `public int width;` |
| 2814 | field | Terraria.Modules.TileObjectCoordinatesModule | Terraria.Modules/TileObjectCoordinatesModule.cs | D:\TRbackup\Version4\Terraria.Modules\TileObjectCoordinatesModule.cs | 11 | 2 | heights | int[] | `public int[] heights;` | `public int[] heights;` |
| 2815 | field | Terraria.Modules.TileObjectCoordinatesModule | Terraria.Modules/TileObjectCoordinatesModule.cs | D:\TRbackup\Version4\Terraria.Modules\TileObjectCoordinatesModule.cs | 13 | 2 | padding | int | `public int padding;` | `public int padding;` |
| 2816 | field | Terraria.Modules.TileObjectCoordinatesModule | Terraria.Modules/TileObjectCoordinatesModule.cs | D:\TRbackup\Version4\Terraria.Modules\TileObjectCoordinatesModule.cs | 15 | 2 | paddingFix | Terraria.DataStructures.Point16 | `public Point16 paddingFix;` | `public Point16 paddingFix;` |
| 2817 | field | Terraria.Modules.TileObjectCoordinatesModule | Terraria.Modules/TileObjectCoordinatesModule.cs | D:\TRbackup\Version4\Terraria.Modules\TileObjectCoordinatesModule.cs | 17 | 2 | styleWidth | int | `public int styleWidth;` | `public int styleWidth;` |
| 2818 | field | Terraria.Modules.TileObjectCoordinatesModule | Terraria.Modules/TileObjectCoordinatesModule.cs | D:\TRbackup\Version4\Terraria.Modules\TileObjectCoordinatesModule.cs | 19 | 2 | styleHeight | int | `public int styleHeight;` | `public int styleHeight;` |
| 2819 | field | Terraria.Modules.TileObjectCoordinatesModule | Terraria.Modules/TileObjectCoordinatesModule.cs | D:\TRbackup\Version4\Terraria.Modules\TileObjectCoordinatesModule.cs | 21 | 2 | calculated | bool | `public bool calculated;` | `public bool calculated;` |
| 2820 | field | Terraria.Modules.TileObjectCoordinatesModule | Terraria.Modules/TileObjectCoordinatesModule.cs | D:\TRbackup\Version4\Terraria.Modules\TileObjectCoordinatesModule.cs | 23 | 2 | drawStyleOffset | int | `public int drawStyleOffset;` | `public int drawStyleOffset;` |
| 2821 | field | Terraria.Modules.TileObjectCoordinatesModule | Terraria.Modules/TileObjectCoordinatesModule.cs | D:\TRbackup\Version4\Terraria.Modules\TileObjectCoordinatesModule.cs | 25 | 2 | drawFrameOffsets | Rectangle[,] | `public Rectangle[,] drawFrameOffsets;` | `public Rectangle[,] drawFrameOffsets;` |
| 2822 | field | Terraria.Modules.TileObjectDrawModule | Terraria.Modules/TileObjectDrawModule.cs | D:\TRbackup\Version4\Terraria.Modules\TileObjectDrawModule.cs | 5 | 2 | xOffset | int | `public int xOffset;` | `public int xOffset;` |
| 2823 | field | Terraria.Modules.TileObjectDrawModule | Terraria.Modules/TileObjectDrawModule.cs | D:\TRbackup\Version4\Terraria.Modules\TileObjectDrawModule.cs | 7 | 2 | yOffset | int | `public int yOffset;` | `public int yOffset;` |
| 2824 | field | Terraria.Modules.TileObjectDrawModule | Terraria.Modules/TileObjectDrawModule.cs | D:\TRbackup\Version4\Terraria.Modules\TileObjectDrawModule.cs | 9 | 2 | flipHorizontal | bool | `public bool flipHorizontal;` | `public bool flipHorizontal;` |
| 2825 | field | Terraria.Modules.TileObjectDrawModule | Terraria.Modules/TileObjectDrawModule.cs | D:\TRbackup\Version4\Terraria.Modules\TileObjectDrawModule.cs | 11 | 2 | flipVertical | bool | `public bool flipVertical;` | `public bool flipVertical;` |
| 2826 | field | Terraria.Modules.TileObjectDrawModule | Terraria.Modules/TileObjectDrawModule.cs | D:\TRbackup\Version4\Terraria.Modules\TileObjectDrawModule.cs | 13 | 2 | stepDown | int | `public int stepDown;` | `public int stepDown;` |

#### 属性（0）

无该类型成员记录。


### 4.21 细分子系统：`SharedTilePlacementBaseAndStyleModules`

- 原报告章节：`4.9.187`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`04` / `地块、液体与世界存储`

- 上一级基线细分子系统：`SharedTilePlacementModules`
- 上一级 peer 细分子系统：`SharedTilePlacementGeometryModules`
- 细分职责：Tile 放置基础、样式和共享几何输入模块。
- 边界角色：`definition/query`；最小 seam：tile placement base style port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：2；声明类型数：2；字段：14；属性：0；合计：14。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（14）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2806 | field | Terraria.Modules.TileObjectBaseModule | Terraria.Modules/TileObjectBaseModule.cs | D:\TRbackup\Version4\Terraria.Modules\TileObjectBaseModule.cs | 8 | 2 | width | int | `public int width;` | `public int width;` |
| 2807 | field | Terraria.Modules.TileObjectBaseModule | Terraria.Modules/TileObjectBaseModule.cs | D:\TRbackup\Version4\Terraria.Modules\TileObjectBaseModule.cs | 10 | 2 | height | int | `public int height;` | `public int height;` |
| 2808 | field | Terraria.Modules.TileObjectBaseModule | Terraria.Modules/TileObjectBaseModule.cs | D:\TRbackup\Version4\Terraria.Modules\TileObjectBaseModule.cs | 12 | 2 | origin | Terraria.DataStructures.Point16 | `public Point16 origin;` | `public Point16 origin;` |
| 2809 | field | Terraria.Modules.TileObjectBaseModule | Terraria.Modules/TileObjectBaseModule.cs | D:\TRbackup\Version4\Terraria.Modules\TileObjectBaseModule.cs | 14 | 2 | direction | Terraria.Enums.TileObjectDirection | `public TileObjectDirection direction;` | `public TileObjectDirection direction;` |
| 2810 | field | Terraria.Modules.TileObjectBaseModule | Terraria.Modules/TileObjectBaseModule.cs | D:\TRbackup\Version4\Terraria.Modules\TileObjectBaseModule.cs | 16 | 2 | randomRange | int | `public int randomRange;` | `public int randomRange;` |
| 2811 | field | Terraria.Modules.TileObjectBaseModule | Terraria.Modules/TileObjectBaseModule.cs | D:\TRbackup\Version4\Terraria.Modules\TileObjectBaseModule.cs | 18 | 2 | flattenAnchors | bool | `public bool flattenAnchors;` | `public bool flattenAnchors;` |
| 2812 | field | Terraria.Modules.TileObjectBaseModule | Terraria.Modules/TileObjectBaseModule.cs | D:\TRbackup\Version4\Terraria.Modules\TileObjectBaseModule.cs | 20 | 2 | specificRandomStyles | int[] | `public int[] specificRandomStyles;` | `public int[] specificRandomStyles;` |
| 2827 | field | Terraria.Modules.TileObjectStyleModule | Terraria.Modules/TileObjectStyleModule.cs | D:\TRbackup\Version4\Terraria.Modules\TileObjectStyleModule.cs | 5 | 2 | style | int | `public int style;` | `public int style;` |
| 2828 | field | Terraria.Modules.TileObjectStyleModule | Terraria.Modules/TileObjectStyleModule.cs | D:\TRbackup\Version4\Terraria.Modules\TileObjectStyleModule.cs | 7 | 2 | horizontal | bool | `public bool horizontal;` | `public bool horizontal;` |
| 2829 | field | Terraria.Modules.TileObjectStyleModule | Terraria.Modules/TileObjectStyleModule.cs | D:\TRbackup\Version4\Terraria.Modules\TileObjectStyleModule.cs | 9 | 2 | styleWrapLimit | int | `public int styleWrapLimit;` | `public int styleWrapLimit;` |
| 2830 | field | Terraria.Modules.TileObjectStyleModule | Terraria.Modules/TileObjectStyleModule.cs | D:\TRbackup\Version4\Terraria.Modules\TileObjectStyleModule.cs | 11 | 2 | styleMultiplier | int | `public int styleMultiplier;` | `public int styleMultiplier;` |
| 2831 | field | Terraria.Modules.TileObjectStyleModule | Terraria.Modules/TileObjectStyleModule.cs | D:\TRbackup\Version4\Terraria.Modules\TileObjectStyleModule.cs | 13 | 2 | styleLineSkip | int | `public int styleLineSkip;` | `public int styleLineSkip;` |
| 2832 | field | Terraria.Modules.TileObjectStyleModule | Terraria.Modules/TileObjectStyleModule.cs | D:\TRbackup\Version4\Terraria.Modules\TileObjectStyleModule.cs | 15 | 2 | styleWrapLimitVisualOverride | int? | `public int? styleWrapLimitVisualOverride;` | `public int? styleWrapLimitVisualOverride;` |
| 2833 | field | Terraria.Modules.TileObjectStyleModule | Terraria.Modules/TileObjectStyleModule.cs | D:\TRbackup\Version4\Terraria.Modules\TileObjectStyleModule.cs | 17 | 2 | styleLineSkipVisualoverride | int? | `public int? styleLineSkipVisualoverride;` | `public int? styleLineSkipVisualoverride;` |

#### 属性（0）

无该类型成员记录。


### 4.22 细分子系统：`SharedTilePaintRenderTargetState`

- 原报告章节：`4.9.218`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`04` / `地块、液体与世界存储`

- 上一级基线细分子系统：`SharedTilePaintState`
- 上一级 peer 细分子系统：`SharedTilePaintState`
- 细分职责：TilePaintSystemV2 的渲染目标、缓存集合和绘制请求状态。
- 边界角色：`presentation state`；最小 seam：tile paint render target port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：6；字段：12；属性：0；合计：12。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（12）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2544 | field | Terraria.GameContent.TilePaintSystemV2.ARenderTargetHolder | Terraria.GameContent/TilePaintSystemV2.cs | D:\TRbackup\Version4\Terraria.GameContent\TilePaintSystemV2.cs | 13 | 3 | Target | RenderTarget2D | `public RenderTarget2D Target;` | `public RenderTarget2D Target;` |
| 2545 | field | Terraria.GameContent.TilePaintSystemV2.ARenderTargetHolder | Terraria.GameContent/TilePaintSystemV2.cs | D:\TRbackup\Version4\Terraria.GameContent\TilePaintSystemV2.cs | 15 | 3 | _wasPrepared | bool | `protected bool _wasPrepared;` | `protected bool _wasPrepared;` |
| 2546 | field | Terraria.GameContent.TilePaintSystemV2.TreeTopRenderTargetHolder | Terraria.GameContent/TilePaintSystemV2.cs | D:\TRbackup\Version4\Terraria.GameContent\TilePaintSystemV2.cs | 24 | 3 | Key | Terraria.GameContent.TilePaintSystemV2.TreeFoliageVariantKey | `public TreeFoliageVariantKey Key;` | `public TreeFoliageVariantKey Key;` |
| 2548 | field | Terraria.GameContent.TilePaintSystemV2.TileRenderTargetHolder | Terraria.GameContent/TilePaintSystemV2.cs | D:\TRbackup\Version4\Terraria.GameContent\TilePaintSystemV2.cs | 38 | 3 | Key | Terraria.GameContent.TilePaintSystemV2.TileVariationkey | `public TileVariationkey Key;` | `public TileVariationkey Key;` |
| 2549 | field | Terraria.GameContent.TilePaintSystemV2.CageTopRenderTargetHolder | Terraria.GameContent/TilePaintSystemV2.cs | D:\TRbackup\Version4\Terraria.GameContent\TilePaintSystemV2.cs | 45 | 3 | Key | Terraria.GameContent.TilePaintSystemV2.CageTopVariationkey | `public CageTopVariationkey Key;` | `public CageTopVariationkey Key;` |
| 2550 | field | Terraria.GameContent.TilePaintSystemV2.WallRenderTargetHolder | Terraria.GameContent/TilePaintSystemV2.cs | D:\TRbackup\Version4\Terraria.GameContent\TilePaintSystemV2.cs | 52 | 3 | Key | Terraria.GameContent.TilePaintSystemV2.WallVariationKey | `public WallVariationKey Key;` | `public WallVariationKey Key;` |
| 2561 | field | Terraria.GameContent.TilePaintSystemV2 | Terraria.GameContent/TilePaintSystemV2.cs | D:\TRbackup\Version4\Terraria.GameContent\TilePaintSystemV2.cs | 173 | 2 | _cageTopRenders | System.Collections.Generic.Dictionary<Terraria.GameContent.TilePaintSystemV2.CageTopVariationkey, Terraria.GameContent.TilePaintSystemV2.CageTopRenderTargetHolder> | `private Dictionary<CageTopVariationkey, CageTopRenderTargetHolder> _cageTopRenders = new Dictionary<CageTopVariationkey, CageTopRenderTargetHolder>();` | `private Dictionary<CageTopVariationkey, CageTopRenderTargetHolder> _cageTopRenders = new Dictionary<CageTopVariationkey, CageTopRenderTargetHolder>();` |
| 2562 | field | Terraria.GameContent.TilePaintSystemV2 | Terraria.GameContent/TilePaintSystemV2.cs | D:\TRbackup\Version4\Terraria.GameContent\TilePaintSystemV2.cs | 175 | 2 | _tilesRenders | System.Collections.Generic.Dictionary<Terraria.GameContent.TilePaintSystemV2.TileVariationkey, Terraria.GameContent.TilePaintSystemV2.TileRenderTargetHolder> | `private Dictionary<TileVariationkey, TileRenderTargetHolder> _tilesRenders = new Dictionary<TileVariationkey, TileRenderTargetHolder>();` | `private Dictionary<TileVariationkey, TileRenderTargetHolder> _tilesRenders = new Dictionary<TileVariationkey, TileRenderTargetHolder>();` |
| 2563 | field | Terraria.GameContent.TilePaintSystemV2 | Terraria.GameContent/TilePaintSystemV2.cs | D:\TRbackup\Version4\Terraria.GameContent\TilePaintSystemV2.cs | 177 | 2 | _wallsRenders | System.Collections.Generic.Dictionary<Terraria.GameContent.TilePaintSystemV2.WallVariationKey, Terraria.GameContent.TilePaintSystemV2.WallRenderTargetHolder> | `private Dictionary<WallVariationKey, WallRenderTargetHolder> _wallsRenders = new Dictionary<WallVariationKey, WallRenderTargetHolder>();` | `private Dictionary<WallVariationKey, WallRenderTargetHolder> _wallsRenders = new Dictionary<WallVariationKey, WallRenderTargetHolder>();` |
| 2564 | field | Terraria.GameContent.TilePaintSystemV2 | Terraria.GameContent/TilePaintSystemV2.cs | D:\TRbackup\Version4\Terraria.GameContent\TilePaintSystemV2.cs | 179 | 2 | _treeTopRenders | System.Collections.Generic.Dictionary<Terraria.GameContent.TilePaintSystemV2.TreeFoliageVariantKey, Terraria.GameContent.TilePaintSystemV2.TreeTopRenderTargetHolder> | `private Dictionary<TreeFoliageVariantKey, TreeTopRenderTargetHolder> _treeTopRenders = new Dictionary<TreeFoliageVariantKey, TreeTopRenderTargetHolder>();` | `private Dictionary<TreeFoliageVariantKey, TreeTopRenderTargetHolder> _treeTopRenders = new Dictionary<TreeFoliageVariantKey, TreeTopRenderTargetHolder>();` |
| 2565 | field | Terraria.GameContent.TilePaintSystemV2 | Terraria.GameContent/TilePaintSystemV2.cs | D:\TRbackup\Version4\Terraria.GameContent\TilePaintSystemV2.cs | 181 | 2 | _treeBranchRenders | System.Collections.Generic.Dictionary<Terraria.GameContent.TilePaintSystemV2.TreeFoliageVariantKey, Terraria.GameContent.TilePaintSystemV2.TreeBranchTargetHolder> | `private Dictionary<TreeFoliageVariantKey, TreeBranchTargetHolder> _treeBranchRenders = new Dictionary<TreeFoliageVariantKey, TreeBranchTargetHolder>();` | `private Dictionary<TreeFoliageVariantKey, TreeBranchTargetHolder> _treeBranchRenders = new Dictionary<TreeFoliageVariantKey, TreeBranchTargetHolder>();` |
| 2566 | field | Terraria.GameContent.TilePaintSystemV2 | Terraria.GameContent/TilePaintSystemV2.cs | D:\TRbackup\Version4\Terraria.GameContent\TilePaintSystemV2.cs | 183 | 2 | _requests | System.Collections.Generic.List<Terraria.GameContent.TilePaintSystemV2.ARenderTargetHolder> | `private List<ARenderTargetHolder> _requests = new List<ARenderTargetHolder>();` | `private List<ARenderTargetHolder> _requests = new List<ARenderTargetHolder>();` |

#### 属性（0）

无该类型成员记录。


### 4.23 细分子系统：`SharedTilePaintVariationAndColorState`

- 原报告章节：`4.9.219`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`04` / `地块、液体与世界存储`

- 上一级基线细分子系统：`SharedTilePaintState`
- 上一级 peer 细分子系统：`SharedTilePaintState`
- 细分职责：Tile/墙/树/笼样式变体键与颜色缓存状态。
- 边界角色：`definition/state`；最小 seam：tile paint variation color port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：2；声明类型数：6；字段：14；属性：0；合计：14。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（14）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2547 | field | Terraria.GameContent.TilePaintSystemV2.TreeBranchTargetHolder | Terraria.GameContent/TilePaintSystemV2.cs | D:\TRbackup\Version4\Terraria.GameContent\TilePaintSystemV2.cs | 31 | 3 | Key | Terraria.GameContent.TilePaintSystemV2.TreeFoliageVariantKey | `public TreeFoliageVariantKey Key;` | `public TreeFoliageVariantKey Key;` |
| 2551 | field | Terraria.GameContent.TilePaintSystemV2.TileVariationkey | Terraria.GameContent/TilePaintSystemV2.cs | D:\TRbackup\Version4\Terraria.GameContent\TilePaintSystemV2.cs | 59 | 3 | TileType | int | `public int TileType;` | `public int TileType;` |
| 2552 | field | Terraria.GameContent.TilePaintSystemV2.TileVariationkey | Terraria.GameContent/TilePaintSystemV2.cs | D:\TRbackup\Version4\Terraria.GameContent\TilePaintSystemV2.cs | 61 | 3 | TileStyle | int | `public int TileStyle;` | `public int TileStyle;` |
| 2553 | field | Terraria.GameContent.TilePaintSystemV2.TileVariationkey | Terraria.GameContent/TilePaintSystemV2.cs | D:\TRbackup\Version4\Terraria.GameContent\TilePaintSystemV2.cs | 63 | 3 | PaintColor | int | `public int PaintColor;` | `public int PaintColor;` |
| 2554 | field | Terraria.GameContent.TilePaintSystemV2.WallVariationKey | Terraria.GameContent/TilePaintSystemV2.cs | D:\TRbackup\Version4\Terraria.GameContent\TilePaintSystemV2.cs | 87 | 3 | WallType | int | `public int WallType;` | `public int WallType;` |
| 2555 | field | Terraria.GameContent.TilePaintSystemV2.WallVariationKey | Terraria.GameContent/TilePaintSystemV2.cs | D:\TRbackup\Version4\Terraria.GameContent\TilePaintSystemV2.cs | 89 | 3 | PaintColor | int | `public int PaintColor;` | `public int PaintColor;` |
| 2556 | field | Terraria.GameContent.TilePaintSystemV2.TreeFoliageVariantKey | Terraria.GameContent/TilePaintSystemV2.cs | D:\TRbackup\Version4\Terraria.GameContent\TilePaintSystemV2.cs | 113 | 3 | TextureIndex | int | `public int TextureIndex;` | `public int TextureIndex;` |
| 2557 | field | Terraria.GameContent.TilePaintSystemV2.TreeFoliageVariantKey | Terraria.GameContent/TilePaintSystemV2.cs | D:\TRbackup\Version4\Terraria.GameContent\TilePaintSystemV2.cs | 115 | 3 | TextureStyle | int | `public int TextureStyle;` | `public int TextureStyle;` |
| 2558 | field | Terraria.GameContent.TilePaintSystemV2.TreeFoliageVariantKey | Terraria.GameContent/TilePaintSystemV2.cs | D:\TRbackup\Version4\Terraria.GameContent\TilePaintSystemV2.cs | 117 | 3 | PaintColor | int | `public int PaintColor;` | `public int PaintColor;` |
| 2559 | field | Terraria.GameContent.TilePaintSystemV2.CageTopVariationkey | Terraria.GameContent/TilePaintSystemV2.cs | D:\TRbackup\Version4\Terraria.GameContent\TilePaintSystemV2.cs | 141 | 3 | CageStyle | int | `public int CageStyle;` | `public int CageStyle;` |
| 2560 | field | Terraria.GameContent.TilePaintSystemV2.CageTopVariationkey | Terraria.GameContent/TilePaintSystemV2.cs | D:\TRbackup\Version4\Terraria.GameContent\TilePaintSystemV2.cs | 143 | 3 | PaintColor | int | `public int PaintColor;` | `public int PaintColor;` |
| 3506 | field | Terraria.TileColorCache | Terraria/TileColorCache.cs | D:\TRbackup\Version4\Terraria\TileColorCache.cs | 5 | 2 | Color | byte | `public byte Color;` | `public byte Color;` |
| 3507 | field | Terraria.TileColorCache | Terraria/TileColorCache.cs | D:\TRbackup\Version4\Terraria\TileColorCache.cs | 7 | 2 | FullBright | bool | `public bool FullBright;` | `public bool FullBright;` |
| 3508 | field | Terraria.TileColorCache | Terraria/TileColorCache.cs | D:\TRbackup\Version4\Terraria\TileColorCache.cs | 9 | 2 | Invisible | bool | `public bool Invisible;` | `public bool Invisible;` |

#### 属性（0）

无该类型成员记录。


## 5. 追溯与验收

> 来源报告 SHA-256：`b944441d92d5103f218b766a3e354200794528a19e65a862e44c2b40a36f1196`
- 本分区细分组数：23；成员数：320；字段：310；属性：10。
- 预期不变量：本文件只出现完整细分子系统；不跨分区复制成员；所有成员的来源序号、原始声明、路径、行列和 C# 类型必须与来源报告一致。
- 验证方式：重新读取 20 个文件，检查文件数、细分组唯一性、来源序号恰好覆盖 `1..4542`、字段/属性合计和逐成员行文本一致；本次分区生成不运行 `dotnet`，因为没有修改 C# 或项目文件。

生成器：`Build/Tools/Generate-Version4NonAuthoritativeComponentPartitionReports.ps1`。
