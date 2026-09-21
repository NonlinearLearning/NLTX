# Version4 非权威组件拆分分区 13/20：网络协议与会话

> 来源报告：`docs/migration/ledgers/Version4非权威模拟系统字段属性逐成员源码声明-更细子系统拆分-去除ID类文件.md`
> 来源报告 SHA-256：`b944441d92d5103f218b766a3e354200794528a19e65a862e44c2b40a36f1196`
> 本文件是并行组件拆分工作包，不是新的权威成员清单；成员声明事实直接保留来源报告原文。

## 1. 分区定位与评估

- 分区范围：网络会话、消息缓冲、socket、数据包、区段流、远端连接、聊天协议和网络投影。
- 本分区组件化重点：Adapter/Projection 单向消费权威状态，明确线程、限流、发送失败和重试边界。
- 本分区包含 18 个完整细分子系统、253 条成员记录（字段 238、属性 15）。来源序号覆盖区间 `135..3910`，区间可能与其他分区交错，不以序号定义领域边界。
- 组件边界判定：细分子系统只是并行处理的最小库存边界，不等价于一个组件。必须继续按共同读者/写者、变更原因、生命周期、权威性和副作用分为 Component、System、Query、Command、Adapter 或 Projection。
- 证据状态：成员声明和统计已由来源报告校验；逐成员读者、写者、初始化/清理路径、持久化/网络语义和系统顺序仍需本分区会话补证，不得从名称或数量推断。

## 2. 分区统计

| 父级子系统 | 细分子系统数 | 字段 | 属性 | 合计 |
|---|---:|---:|---:|---:|
| `NetworkSessionAndSectionStreaming` | 9 | 113 | 3 | 116 |
| `RuntimeComposition` | 2 | 27 | 0 | 27 |
| `SharedRuntimeMechanisms` | 7 | 98 | 12 | 110 |

| 原报告章节 | 父级 | 细分子系统 | 边界角色 | 字段 | 属性 | 合计 | 组件化处理 |
|---|---|---|---|---:|---:|---:|---|
| `4.1.11` | `RuntimeComposition` | `MainRecentServerAndMapState` | presentation state | 18 | 0 | 18 | 待按成员访问模式拆分 |
| `4.1.33` | `RuntimeComposition` | `MainNetworkSessionState` | runtime state | 9 | 0 | 9 | 待按成员访问模式拆分 |
| `4.3.1` | `NetworkSessionAndSectionStreaming` | `NetworkMessageBufferAndDispatch` | adapter state | 19 | 1 | 20 | 待按成员访问模式拆分 |
| `4.3.2` | `NetworkSessionAndSectionStreaming` | `NetworkPublicationAndSound` | projection state | 12 | 0 | 12 | 待按成员访问模式拆分 |
| `4.3.3` | `NetworkSessionAndSectionStreaming` | `SectionStreamingState` | adapter state | 16 | 0 | 16 | 待按成员访问模式拆分 |
| `4.3.4` | `NetworkSessionAndSectionStreaming` | `NetworkRemoteServerState` | adapter state | 11 | 0 | 11 | 待按成员访问模式拆分 |
| `4.3.5` | `NetworkSessionAndSectionStreaming` | `NetworkRemoteIpRequestAdapter` | adapter DTO | 3 | 0 | 3 | 待按成员访问模式拆分 |
| `4.3.6` | `NetworkSessionAndSectionStreaming` | `NetworkRemoteClientConnectionAndStatusState` | adapter state | 13 | 0 | 13 | 待按成员访问模式拆分 |
| `4.3.7` | `NetworkSessionAndSectionStreaming` | `NetworkRemoteClientSectionAndRateLimitState` | adapter state | 13 | 1 | 14 | 待按成员访问模式拆分 |
| `4.3.8` | `NetworkSessionAndSectionStreaming` | `NetworkSessionConfigurationState` | adapter state | 11 | 1 | 12 | 待按成员访问模式拆分 |
| `4.3.9` | `NetworkSessionAndSectionStreaming` | `NetworkSessionTransportAndThreadState` | adapter state | 15 | 0 | 15 | 待按成员访问模式拆分 |
| `4.9.37` | `SharedRuntimeMechanisms` | `SharedNetworkSocketTransport` | adapter | 18 | 6 | 24 | 待按成员访问模式拆分 |
| `4.9.38` | `SharedRuntimeMechanisms` | `SharedNetworkPacketPrimitives` | adapter | 22 | 3 | 25 | 待按成员访问模式拆分 |
| `4.9.39` | `SharedRuntimeMechanisms` | `SharedContentNetworkModules` | adapter | 6 | 0 | 6 | 待按成员访问模式拆分 |
| `4.9.40` | `SharedRuntimeMechanisms` | `SharedNetworkSectionProjections` | projection | 6 | 0 | 6 | 待按成员访问模式拆分 |
| `4.9.41` | `SharedRuntimeMechanisms` | `SharedChatAndCommandProtocol` | adapter | 15 | 3 | 18 | 待按成员访问模式拆分 |
| `4.9.148` | `SharedRuntimeMechanisms` | `SharedChatSnippetPresentationState` | projection/presentation | 19 | 0 | 19 | 待按成员访问模式拆分 |
| `4.9.149` | `SharedRuntimeMechanisms` | `SharedChatMonitorAndCommandState` | projection/adapter | 12 | 0 | 12 | 待按成员访问模式拆分 |

## 3. 并行组件拆分契约

对本文件中的每个细分子系统和每条成员记录执行以下判断，并把结果写入对应的实现阶段设计/审计记录：

- 盘点声明类型、可见性、读者、写者、创建/更新/清理/持久化生命周期、访问频率和副作用；缺证据标记 `missing`，不可用命名猜测补齐。
- 标记权威状态、派生值、缓存、快照、兼容字段和表现数据；Component 只保存一个内聚概念的权威数据。
- 需要行为转换时放入显式 System/Command；纯资格判断和派生计算放入 Query；外部协议、文件、平台和第三方类型放入 Adapter；网络/持久化/日志/UI 输出放入单向 Projection。
- 分别建模实体引用、持久化 ID、网络 ID 和外部 ID；不得把 ID 类文件或外部对象直接塞入共享组件。
- 记录 Interface、Implementation、Seam、Depth、Leverage、Locality，以及必要的系统先后约束、失败归属和重试策略。

## 4. 逐成员源码声明

以下细分子系统块保持来源报告的成员表、声明、路径、行列和来源序号；只调整 Markdown 标题层级并增加分区追踪元数据。

### 4.1 细分子系统：`MainRecentServerAndMapState`

- 原报告章节：`4.1.11`
- 父级子系统：`RuntimeComposition`
- 分区工作包：`13` / `网络协议与会话`

- 上一级基线细分子系统：`MainRecentServerAndMapState`
- 细分职责：最近服务器、背景层和地图刷新状态。
- 边界角色：`presentation state`；最小 seam：recent-server/map view；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：18；属性：0；合计：18。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（18）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 135 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 430 | 2 | maxMP | int | `public static int maxMP = 10;` | `public static int maxMP = 10;` |
| 136 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 432 | 2 | recentWorld | string[] | `public static string[] recentWorld = new string[maxMP];` | `public static string[] recentWorld = new string[maxMP];` |
| 137 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 434 | 2 | recentIP | string[] | `public static string[] recentIP = new string[maxMP];` | `public static string[] recentIP = new string[maxMP];` |
| 138 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 436 | 2 | recentPort | int[] | `public static int[] recentPort = new int[maxMP];` | `public static int[] recentPort = new int[maxMP];` |
| 139 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 438 | 2 | instantBGTransitionCounter | int | `public static int instantBGTransitionCounter = 2;` | `public static int instantBGTransitionCounter = 2;` |
| 140 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 440 | 2 | bgDelay | int | `public static int bgDelay;` | `public static int bgDelay;` |
| 141 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 442 | 2 | bgStyle | int | `public static int bgStyle;` | `public static int bgStyle;` |
| 142 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 444 | 2 | bgAlphaFrontLayer | float[] | `public static float[] bgAlphaFrontLayer = new float[16];` | `public static float[] bgAlphaFrontLayer = new float[16];` |
| 143 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 446 | 2 | bgAlphaFarBackLayer | float[] | `public static float[] bgAlphaFarBackLayer = new float[16];` | `public static float[] bgAlphaFarBackLayer = new float[16];` |
| 144 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 448 | 2 | wofNPCIndex | int | `public static int wofNPCIndex = -1;` | `public static int wofNPCIndex = -1;` |
| 145 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 450 | 2 | wofDrawAreaTop | int | `public static int wofDrawAreaTop;` | `public static int wofDrawAreaTop;` |
| 146 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 452 | 2 | wofDrawAreaBottom | int | `public static int wofDrawAreaBottom;` | `public static int wofDrawAreaBottom;` |
| 147 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 454 | 2 | refreshMap | bool | `public static bool refreshMap;` | `public static bool refreshMap;` |
| 148 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 456 | 2 | mapReady | bool | `public static bool mapReady;` | `public static bool mapReady;` |
| 149 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 458 | 2 | updateMap | Microsoft.Xna.Framework.Rectangle? | `public static Microsoft.Xna.Framework.Rectangle? updateMap;` | `public static Microsoft.Xna.Framework.Rectangle? updateMap;` |
| 150 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 460 | 2 | mapTimeMax | int | `public static int mapTimeMax = 30;` | `public static int mapTimeMax = 30;` |
| 151 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 462 | 2 | mapTime | int | `public static int mapTime = mapTimeMax;` | `public static int mapTime = mapTimeMax;` |
| 152 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 464 | 2 | clearMap | bool | `public static bool clearMap;` | `public static bool clearMap;` |

#### 属性（0）

无该类型成员记录。


### 4.2 细分子系统：`MainNetworkSessionState`

- 原报告章节：`4.1.33`
- 父级子系统：`RuntimeComposition`
- 分区工作包：`13` / `网络协议与会话`

- 上一级基线细分子系统：`MainNetworkSessionState`
- 细分职责：网络模式切换、玩家更新和连接会话状态。
- 边界角色：`runtime state`；最小 seam：network session adapter；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：9；属性：0；合计：9。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（9）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 447 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1161 | 2 | getIP | string | `public static string getIP = defaultIP;` | `public static string getIP = defaultIP;` |
| 448 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1164 | 2 | menuMultiplayer | bool | `public static bool menuMultiplayer;` | `public static bool menuMultiplayer;` |
| 449 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1166 | 2 | menuServer | bool | `public static bool menuServer;` | `public static bool menuServer;` |
| 450 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1168 | 2 | netMode | int | `public static int netMode;` | `public static int netMode;` |
| 451 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1170 | 2 | _targetNetMode | int | `private static int _targetNetMode;` | `private static int _targetNetMode;` |
| 452 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1172 | 2 | _hasPendingNetmodeChange | bool | `private static bool _hasPendingNetmodeChange;` | `private static bool _hasPendingNetmodeChange;` |
| 453 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1174 | 2 | netPlayCounter | int | `public static int netPlayCounter;` | `public static int netPlayCounter;` |
| 454 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1176 | 2 | lastItemUpdate | int | `public static int lastItemUpdate;` | `public static int lastItemUpdate;` |
| 455 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1178 | 2 | maxItemUpdates | int | `public static int maxItemUpdates = 5;` | `public static int maxItemUpdates = 5;` |

#### 属性（0）

无该类型成员记录。


### 4.3 细分子系统：`NetworkMessageBufferAndDispatch`

- 原报告章节：`4.3.1`
- 父级子系统：`NetworkSessionAndSectionStreaming`
- 分区工作包：`13` / `网络协议与会话`

- 上一级基线细分子系统：`NetworkMessageBufferAndDispatch`
- 细分职责：消息缓冲、读写游标和分发上下文。
- 边界角色：`adapter state`；最小 seam：message decode/dispatch port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：19；属性：1；合计：20。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（19）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 649 | field | Terraria.MessageBuffer | Terraria/MessageBuffer.cs | D:\TRbackup\Version4\Terraria\MessageBuffer.cs | 29 | 2 | readBufferMax | int | `public const int readBufferMax = 131070;` | `public const int readBufferMax = 131070;` |
| 650 | field | Terraria.MessageBuffer | Terraria/MessageBuffer.cs | D:\TRbackup\Version4\Terraria\MessageBuffer.cs | 31 | 2 | writeBufferMax | int | `public const int writeBufferMax = 131070;` | `public const int writeBufferMax = 131070;` |
| 651 | field | Terraria.MessageBuffer | Terraria/MessageBuffer.cs | D:\TRbackup\Version4\Terraria\MessageBuffer.cs | 33 | 2 | broadcast | bool | `public bool broadcast;` | `public bool broadcast;` |
| 652 | field | Terraria.MessageBuffer | Terraria/MessageBuffer.cs | D:\TRbackup\Version4\Terraria\MessageBuffer.cs | 35 | 2 | readBuffer | byte[] | `public byte[] readBuffer = new byte[131070];` | `public byte[] readBuffer = new byte[131070];` |
| 653 | field | Terraria.MessageBuffer | Terraria/MessageBuffer.cs | D:\TRbackup\Version4\Terraria\MessageBuffer.cs | 37 | 2 | writeBuffer | byte[] | `public byte[] writeBuffer = new byte[131070];` | `public byte[] writeBuffer = new byte[131070];` |
| 654 | field | Terraria.MessageBuffer | Terraria/MessageBuffer.cs | D:\TRbackup\Version4\Terraria\MessageBuffer.cs | 39 | 2 | writeLocked | bool | `public bool writeLocked;` | `public bool writeLocked;` |
| 655 | field | Terraria.MessageBuffer | Terraria/MessageBuffer.cs | D:\TRbackup\Version4\Terraria\MessageBuffer.cs | 41 | 2 | messageLength | int | `public int messageLength;` | `public int messageLength;` |
| 656 | field | Terraria.MessageBuffer | Terraria/MessageBuffer.cs | D:\TRbackup\Version4\Terraria\MessageBuffer.cs | 43 | 2 | totalData | int | `public int totalData;` | `public int totalData;` |
| 657 | field | Terraria.MessageBuffer | Terraria/MessageBuffer.cs | D:\TRbackup\Version4\Terraria\MessageBuffer.cs | 45 | 2 | whoAmI | int | `public int whoAmI;` | `public int whoAmI;` |
| 658 | field | Terraria.MessageBuffer | Terraria/MessageBuffer.cs | D:\TRbackup\Version4\Terraria\MessageBuffer.cs | 47 | 2 | spamCount | int | `public int spamCount;` | `public int spamCount;` |
| 659 | field | Terraria.MessageBuffer | Terraria/MessageBuffer.cs | D:\TRbackup\Version4\Terraria\MessageBuffer.cs | 49 | 2 | maxSpam | int | `public int maxSpam;` | `public int maxSpam;` |
| 660 | field | Terraria.MessageBuffer | Terraria/MessageBuffer.cs | D:\TRbackup\Version4\Terraria\MessageBuffer.cs | 51 | 2 | checkBytes | bool | `public bool checkBytes;` | `public bool checkBytes;` |
| 661 | field | Terraria.MessageBuffer | Terraria/MessageBuffer.cs | D:\TRbackup\Version4\Terraria\MessageBuffer.cs | 53 | 2 | readerStream | System.IO.MemoryStream | `public MemoryStream readerStream;` | `public MemoryStream readerStream;` |
| 662 | field | Terraria.MessageBuffer | Terraria/MessageBuffer.cs | D:\TRbackup\Version4\Terraria\MessageBuffer.cs | 55 | 2 | writerStream | System.IO.MemoryStream | `public MemoryStream writerStream;` | `public MemoryStream writerStream;` |
| 663 | field | Terraria.MessageBuffer | Terraria/MessageBuffer.cs | D:\TRbackup\Version4\Terraria\MessageBuffer.cs | 57 | 2 | reader | System.IO.BinaryReader | `public BinaryReader reader;` | `public BinaryReader reader;` |
| 664 | field | Terraria.MessageBuffer | Terraria/MessageBuffer.cs | D:\TRbackup\Version4\Terraria\MessageBuffer.cs | 59 | 2 | writer | System.IO.BinaryWriter | `public BinaryWriter writer;` | `public BinaryWriter writer;` |
| 665 | field | Terraria.MessageBuffer | Terraria/MessageBuffer.cs | D:\TRbackup\Version4\Terraria\MessageBuffer.cs | 61 | 2 | History | Terraria.Testing.PacketHistory | `public PacketHistory History = new PacketHistory();` | `public PacketHistory History = new PacketHistory();` |
| 666 | field | Terraria.MessageBuffer | Terraria/MessageBuffer.cs | D:\TRbackup\Version4\Terraria\MessageBuffer.cs | 63 | 2 | _temporaryProjectileAI | float[] | `private float[] _temporaryProjectileAI = new float[Projectile.maxAI];` | `private float[] _temporaryProjectileAI = new float[Projectile.maxAI];` |
| 667 | field | Terraria.MessageBuffer | Terraria/MessageBuffer.cs | D:\TRbackup\Version4\Terraria\MessageBuffer.cs | 65 | 2 | _temporaryNPCAI | float[] | `private float[] _temporaryNPCAI = new float[NPC.maxAI];` | `private float[] _temporaryNPCAI = new float[NPC.maxAI];` |

#### 属性（1）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 762 | property | Terraria.MessageBuffer | Terraria/MessageBuffer.cs | D:\TRbackup\Version4\Terraria\MessageBuffer.cs | 67 | 2 | RemainingReadBufferLength | int | `public int RemainingReadBufferLength => readBuffer.Length - totalData;` | `public int RemainingReadBufferLength => readBuffer.Length - totalData;` |


### 4.4 细分子系统：`NetworkPublicationAndSound`

- 原报告章节：`4.3.2`
- 父级子系统：`NetworkSessionAndSectionStreaming`
- 分区工作包：`13` / `网络协议与会话`

- 上一级基线细分子系统：`NetworkPublicationAndSound`
- 细分职责：网络消息发布和声音消息载荷状态。
- 边界角色：`projection state`；最小 seam：network publication port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：2；字段：12；属性：0；合计：12。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（12）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 668 | field | Terraria.NetMessage.NetSoundInfo | Terraria/NetMessage.cs | D:\TRbackup\Version4\Terraria\NetMessage.cs | 24 | 3 | position | Vector2 | `public Vector2 position;` | `public Vector2 position;` |
| 669 | field | Terraria.NetMessage.NetSoundInfo | Terraria/NetMessage.cs | D:\TRbackup\Version4\Terraria\NetMessage.cs | 26 | 3 | soundIndex | ushort | `public ushort soundIndex;` | `public ushort soundIndex;` |
| 670 | field | Terraria.NetMessage.NetSoundInfo | Terraria/NetMessage.cs | D:\TRbackup\Version4\Terraria\NetMessage.cs | 28 | 3 | style | int | `public int style;` | `public int style;` |
| 671 | field | Terraria.NetMessage.NetSoundInfo | Terraria/NetMessage.cs | D:\TRbackup\Version4\Terraria\NetMessage.cs | 30 | 3 | volume | float | `public float volume;` | `public float volume;` |
| 672 | field | Terraria.NetMessage.NetSoundInfo | Terraria/NetMessage.cs | D:\TRbackup\Version4\Terraria\NetMessage.cs | 32 | 3 | pitchOffset | float | `public float pitchOffset;` | `public float pitchOffset;` |
| 673 | field | Terraria.NetMessage | Terraria/NetMessage.cs | D:\TRbackup\Version4\Terraria\NetMessage.cs | 66 | 2 | buffer | Terraria.MessageBuffer[] | `public static MessageBuffer[] buffer = new MessageBuffer[257];` | `public static MessageBuffer[] buffer = new MessageBuffer[257];` |
| 674 | field | Terraria.NetMessage | Terraria/NetMessage.cs | D:\TRbackup\Version4\Terraria\NetMessage.cs | 68 | 2 | _compressChestList | short[] | `private static short[] _compressChestList = new short[8000];` | `private static short[] _compressChestList = new short[8000];` |
| 675 | field | Terraria.NetMessage | Terraria/NetMessage.cs | D:\TRbackup\Version4\Terraria\NetMessage.cs | 70 | 2 | _compressSignList | short[] | `private static short[] _compressSignList = new short[32000];` | `private static short[] _compressSignList = new short[32000];` |
| 676 | field | Terraria.NetMessage | Terraria/NetMessage.cs | D:\TRbackup\Version4\Terraria\NetMessage.cs | 72 | 2 | _compressEntities | short[] | `private static short[] _compressEntities = new short[1000];` | `private static short[] _compressEntities = new short[1000];` |
| 677 | field | Terraria.NetMessage | Terraria/NetMessage.cs | D:\TRbackup\Version4\Terraria\NetMessage.cs | 74 | 2 | _currentPlayerDeathReason | Terraria.DataStructures.PlayerDeathReason | `private static PlayerDeathReason _currentPlayerDeathReason;` | `private static PlayerDeathReason _currentPlayerDeathReason;` |
| 678 | field | Terraria.NetMessage | Terraria/NetMessage.cs | D:\TRbackup\Version4\Terraria\NetMessage.cs | 76 | 2 | _currentNetSoundInfo | Terraria.NetMessage.NetSoundInfo | `private static NetSoundInfo _currentNetSoundInfo;` | `private static NetSoundInfo _currentNetSoundInfo;` |
| 679 | field | Terraria.NetMessage | Terraria/NetMessage.cs | D:\TRbackup\Version4\Terraria\NetMessage.cs | 78 | 2 | _currentRevengeMarker | Terraria.GameContent.CoinLossRevengeSystem.RevengeMarker | `private static CoinLossRevengeSystem.RevengeMarker _currentRevengeMarker;` | `private static CoinLossRevengeSystem.RevengeMarker _currentRevengeMarker;` |

#### 属性（0）

无该类型成员记录。


### 4.5 细分子系统：`SectionStreamingState`

- 原报告章节：`4.3.3`
- 父级子系统：`NetworkSessionAndSectionStreaming`
- 分区工作包：`13` / `网络协议与会话`

- 上一级基线细分子系统：`SectionStreamingState`
- 细分职责：世界区段迭代和客户端区段加载状态。
- 边界角色：`adapter state`；最小 seam：section streaming port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：2；字段：16；属性：0；合计：16。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（16）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 746 | field | Terraria.WorldSections.IterationState | Terraria/WorldSections.cs | D:\TRbackup\Version4\Terraria\WorldSections.cs | 10 | 3 | centerPos | Vector2 | `public Vector2 centerPos;` | `public Vector2 centerPos;` |
| 747 | field | Terraria.WorldSections.IterationState | Terraria/WorldSections.cs | D:\TRbackup\Version4\Terraria\WorldSections.cs | 12 | 3 | X | int | `public int X;` | `public int X;` |
| 748 | field | Terraria.WorldSections.IterationState | Terraria/WorldSections.cs | D:\TRbackup\Version4\Terraria\WorldSections.cs | 14 | 3 | Y | int | `public int Y;` | `public int Y;` |
| 749 | field | Terraria.WorldSections.IterationState | Terraria/WorldSections.cs | D:\TRbackup\Version4\Terraria\WorldSections.cs | 16 | 3 | leg | int | `public int leg;` | `public int leg;` |
| 750 | field | Terraria.WorldSections.IterationState | Terraria/WorldSections.cs | D:\TRbackup\Version4\Terraria\WorldSections.cs | 18 | 3 | xDir | int | `public int xDir;` | `public int xDir;` |
| 751 | field | Terraria.WorldSections.IterationState | Terraria/WorldSections.cs | D:\TRbackup\Version4\Terraria\WorldSections.cs | 20 | 3 | yDir | int | `public int yDir;` | `public int yDir;` |
| 752 | field | Terraria.WorldSections | Terraria/WorldSections.cs | D:\TRbackup\Version4\Terraria\WorldSections.cs | 35 | 2 | BitIndex_SectionLoaded | int | `public const int BitIndex_SectionLoaded = 0;` | `public const int BitIndex_SectionLoaded = 0;` |
| 753 | field | Terraria.WorldSections | Terraria/WorldSections.cs | D:\TRbackup\Version4\Terraria\WorldSections.cs | 37 | 2 | BitIndex_SectionFramed | int | `public const int BitIndex_SectionFramed = 1;` | `public const int BitIndex_SectionFramed = 1;` |
| 754 | field | Terraria.WorldSections | Terraria/WorldSections.cs | D:\TRbackup\Version4\Terraria\WorldSections.cs | 39 | 2 | BitIndex_SectionMapDrawn | int | `public const int BitIndex_SectionMapDrawn = 2;` | `public const int BitIndex_SectionMapDrawn = 2;` |
| 755 | field | Terraria.WorldSections | Terraria/WorldSections.cs | D:\TRbackup\Version4\Terraria\WorldSections.cs | 41 | 2 | BitIndex_SectionNeedsRefresh | int | `public const int BitIndex_SectionNeedsRefresh = 3;` | `public const int BitIndex_SectionNeedsRefresh = 3;` |
| 756 | field | Terraria.WorldSections | Terraria/WorldSections.cs | D:\TRbackup\Version4\Terraria\WorldSections.cs | 43 | 2 | width | int | `private int width;` | `private int width;` |
| 757 | field | Terraria.WorldSections | Terraria/WorldSections.cs | D:\TRbackup\Version4\Terraria\WorldSections.cs | 45 | 2 | height | int | `private int height;` | `private int height;` |
| 758 | field | Terraria.WorldSections | Terraria/WorldSections.cs | D:\TRbackup\Version4\Terraria\WorldSections.cs | 47 | 2 | data | Terraria.BitsByte[] | `private BitsByte[] data;` | `private BitsByte[] data;` |
| 759 | field | Terraria.WorldSections | Terraria/WorldSections.cs | D:\TRbackup\Version4\Terraria\WorldSections.cs | 49 | 2 | mapSectionsLeft | int | `private int mapSectionsLeft;` | `private int mapSectionsLeft;` |
| 760 | field | Terraria.WorldSections | Terraria/WorldSections.cs | D:\TRbackup\Version4\Terraria\WorldSections.cs | 55 | 2 | prevFrame | Terraria.WorldSections.IterationState | `private IterationState prevFrame;` | `private IterationState prevFrame;` |
| 761 | field | Terraria.WorldSections | Terraria/WorldSections.cs | D:\TRbackup\Version4\Terraria\WorldSections.cs | 57 | 2 | prevMap | Terraria.WorldSections.IterationState | `private IterationState prevMap;` | `private IterationState prevMap;` |

#### 属性（0）

无该类型成员记录。


### 4.6 细分子系统：`NetworkRemoteServerState`

- 原报告章节：`4.3.4`
- 父级子系统：`NetworkSessionAndSectionStreaming`
- 分区工作包：`13` / `网络协议与会话`

- 上一级基线细分子系统：`NetworkRemoteServerState`
- 细分职责：远端服务器连接、活动状态和服务器端点。
- 边界角色：`adapter state`；最小 seam：remote server transport port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：11；属性：0；合计：11。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（11）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 735 | field | Terraria.RemoteServer | Terraria/RemoteServer.cs | D:\TRbackup\Version4\Terraria\RemoteServer.cs | 9 | 2 | Socket | Terraria.Net.Sockets.ISocket | `public ISocket Socket = new TcpSocket();` | `public ISocket Socket = new TcpSocket();` |
| 736 | field | Terraria.RemoteServer | Terraria/RemoteServer.cs | D:\TRbackup\Version4\Terraria\RemoteServer.cs | 11 | 2 | IsActive | bool | `public bool IsActive;` | `public bool IsActive;` |
| 737 | field | Terraria.RemoteServer | Terraria/RemoteServer.cs | D:\TRbackup\Version4\Terraria\RemoteServer.cs | 13 | 2 | State | int | `public int State;` | `public int State;` |
| 738 | field | Terraria.RemoteServer | Terraria/RemoteServer.cs | D:\TRbackup\Version4\Terraria\RemoteServer.cs | 15 | 2 | TimeOutTimer | int | `public int TimeOutTimer;` | `public int TimeOutTimer;` |
| 739 | field | Terraria.RemoteServer | Terraria/RemoteServer.cs | D:\TRbackup\Version4\Terraria\RemoteServer.cs | 17 | 2 | PendingTermination | bool | `public bool PendingTermination;` | `public bool PendingTermination;` |
| 740 | field | Terraria.RemoteServer | Terraria/RemoteServer.cs | D:\TRbackup\Version4\Terraria\RemoteServer.cs | 19 | 2 | IsReading | bool | `public bool IsReading;` | `public bool IsReading;` |
| 741 | field | Terraria.RemoteServer | Terraria/RemoteServer.cs | D:\TRbackup\Version4\Terraria\RemoteServer.cs | 21 | 2 | ReadBuffer | byte[] | `public byte[] ReadBuffer;` | `public byte[] ReadBuffer;` |
| 742 | field | Terraria.RemoteServer | Terraria/RemoteServer.cs | D:\TRbackup\Version4\Terraria\RemoteServer.cs | 23 | 2 | StatusText | string | `public string StatusText;` | `public string StatusText;` |
| 743 | field | Terraria.RemoteServer | Terraria/RemoteServer.cs | D:\TRbackup\Version4\Terraria\RemoteServer.cs | 25 | 2 | StatusCount | int | `public int StatusCount;` | `public int StatusCount;` |
| 744 | field | Terraria.RemoteServer | Terraria/RemoteServer.cs | D:\TRbackup\Version4\Terraria\RemoteServer.cs | 27 | 2 | StatusMax | int | `public int StatusMax;` | `public int StatusMax;` |
| 745 | field | Terraria.RemoteServer | Terraria/RemoteServer.cs | D:\TRbackup\Version4\Terraria\RemoteServer.cs | 29 | 2 | ServerSpecialFlags | Terraria.BitsByte | `public BitsByte ServerSpecialFlags;` | `public BitsByte ServerSpecialFlags;` |

#### 属性（0）

无该类型成员记录。


### 4.7 细分子系统：`NetworkRemoteIpRequestAdapter`

- 原报告章节：`4.3.5`
- 父级子系统：`NetworkSessionAndSectionStreaming`
- 分区工作包：`13` / `网络协议与会话`

- 上一级基线细分子系统：`NetworkRemoteIpRequestAdapter`
- 细分职责：远端 IP 请求标识、回调和结果载荷。
- 边界角色：`adapter DTO`；最小 seam：remote IP request callback；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：3；属性：0；合计：3。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（3）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 680 | field | Terraria.Netplay.SetRemoteIPRequestInfo | Terraria/Netplay.cs | D:\TRbackup\Version4\Terraria\Netplay.cs | 24 | 3 | RequestId | int | `public int RequestId;` | `public int RequestId;` |
| 681 | field | Terraria.Netplay.SetRemoteIPRequestInfo | Terraria/Netplay.cs | D:\TRbackup\Version4\Terraria\Netplay.cs | 26 | 3 | SuccessCallback | System.Action | `public Action SuccessCallback;` | `public Action SuccessCallback;` |
| 682 | field | Terraria.Netplay.SetRemoteIPRequestInfo | Terraria/Netplay.cs | D:\TRbackup\Version4\Terraria\Netplay.cs | 28 | 3 | RemoteAddress | string | `public string RemoteAddress;` | `public string RemoteAddress;` |

#### 属性（0）

无该类型成员记录。


### 4.8 细分子系统：`NetworkRemoteClientConnectionAndStatusState`

- 原报告章节：`4.3.6`
- 父级子系统：`NetworkSessionAndSectionStreaming`
- 分区工作包：`13` / `网络协议与会话`

- 上一级基线细分子系统：`NetworkRemoteClientState`
- 上一级 peer 细分子系统：`NetworkRemoteClientState`
- 细分职责：远端客户端 Socket、连接、身份和状态文本。
- 边界角色：`adapter state`；最小 seam：remote client connection status port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：13；属性：0；合计：13。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（13）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 709 | field | Terraria.RemoteClient | Terraria/RemoteClient.cs | D:\TRbackup\Version4\Terraria\RemoteClient.cs | 11 | 2 | Socket | Terraria.Net.Sockets.ISocket | `public ISocket Socket;` | `public ISocket Socket;` |
| 710 | field | Terraria.RemoteClient | Terraria/RemoteClient.cs | D:\TRbackup\Version4\Terraria\RemoteClient.cs | 13 | 2 | Id | int | `public int Id;` | `public int Id;` |
| 711 | field | Terraria.RemoteClient | Terraria/RemoteClient.cs | D:\TRbackup\Version4\Terraria\RemoteClient.cs | 15 | 2 | Name | string | `public string Name = "Anonymous";` | `public string Name = "Anonymous";` |
| 712 | field | Terraria.RemoteClient | Terraria/RemoteClient.cs | D:\TRbackup\Version4\Terraria\RemoteClient.cs | 17 | 2 | IsActive | bool | `public bool IsActive;` | `public bool IsActive;` |
| 713 | field | Terraria.RemoteClient | Terraria/RemoteClient.cs | D:\TRbackup\Version4\Terraria\RemoteClient.cs | 19 | 2 | PendingTermination | bool | `public bool PendingTermination;` | `public bool PendingTermination;` |
| 714 | field | Terraria.RemoteClient | Terraria/RemoteClient.cs | D:\TRbackup\Version4\Terraria\RemoteClient.cs | 21 | 2 | PendingTerminationApproved | bool | `public bool PendingTerminationApproved;` | `public bool PendingTerminationApproved;` |
| 715 | field | Terraria.RemoteClient | Terraria/RemoteClient.cs | D:\TRbackup\Version4\Terraria\RemoteClient.cs | 23 | 2 | IsAnnouncementCompleted | bool | `public bool IsAnnouncementCompleted;` | `public bool IsAnnouncementCompleted;` |
| 716 | field | Terraria.RemoteClient | Terraria/RemoteClient.cs | D:\TRbackup\Version4\Terraria\RemoteClient.cs | 25 | 2 | State | int | `public int State;` | `public int State;` |
| 717 | field | Terraria.RemoteClient | Terraria/RemoteClient.cs | D:\TRbackup\Version4\Terraria\RemoteClient.cs | 27 | 2 | TimeOutTimer | int | `public int TimeOutTimer;` | `public int TimeOutTimer;` |
| 718 | field | Terraria.RemoteClient | Terraria/RemoteClient.cs | D:\TRbackup\Version4\Terraria\RemoteClient.cs | 29 | 2 | StatusText | string | `public string StatusText = "";` | `public string StatusText = "";` |
| 719 | field | Terraria.RemoteClient | Terraria/RemoteClient.cs | D:\TRbackup\Version4\Terraria\RemoteClient.cs | 31 | 2 | StatusText2 | string | `public string StatusText2;` | `public string StatusText2;` |
| 720 | field | Terraria.RemoteClient | Terraria/RemoteClient.cs | D:\TRbackup\Version4\Terraria\RemoteClient.cs | 33 | 2 | StatusCount | int | `public int StatusCount;` | `public int StatusCount;` |
| 721 | field | Terraria.RemoteClient | Terraria/RemoteClient.cs | D:\TRbackup\Version4\Terraria\RemoteClient.cs | 35 | 2 | StatusMax | int | `public int StatusMax;` | `public int StatusMax;` |

#### 属性（0）

无该类型成员记录。


### 4.9 细分子系统：`NetworkRemoteClientSectionAndRateLimitState`

- 原报告章节：`4.3.7`
- 父级子系统：`NetworkSessionAndSectionStreaming`
- 分区工作包：`13` / `网络协议与会话`

- 上一级基线细分子系统：`NetworkRemoteClientState`
- 上一级 peer 细分子系统：`NetworkRemoteClientState`
- 细分职责：远端客户端区段、读取缓冲和反垃圾限制状态。
- 边界角色：`adapter state`；最小 seam：remote client section rate-limit port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：13；属性：1；合计：14。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（13）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 722 | field | Terraria.RemoteClient | Terraria/RemoteClient.cs | D:\TRbackup\Version4\Terraria\RemoteClient.cs | 37 | 2 | TileSections | bool[,] | `public bool[,] TileSections = new bool[Main.maxTilesX / 200 + 1, Main.maxTilesY / 150 + 1];` | `public bool[,] TileSections = new bool[Main.maxTilesX / 200 + 1, Main.maxTilesY / 150 + 1];` |
| 723 | field | Terraria.RemoteClient | Terraria/RemoteClient.cs | D:\TRbackup\Version4\Terraria\RemoteClient.cs | 39 | 2 | TileSectionsCheckTime | uint[,] | `public uint[,] TileSectionsCheckTime = new uint[Main.maxTilesX / 200 + 1, Main.maxTilesY / 150 + 1];` | `public uint[,] TileSectionsCheckTime = new uint[Main.maxTilesX / 200 + 1, Main.maxTilesY / 150 + 1];` |
| 724 | field | Terraria.RemoteClient | Terraria/RemoteClient.cs | D:\TRbackup\Version4\Terraria\RemoteClient.cs | 41 | 2 | CheckingSections | bool | `public bool CheckingSections;` | `public bool CheckingSections;` |
| 725 | field | Terraria.RemoteClient | Terraria/RemoteClient.cs | D:\TRbackup\Version4\Terraria\RemoteClient.cs | 43 | 2 | ReadBuffer | byte[] | `public byte[] ReadBuffer;` | `public byte[] ReadBuffer;` |
| 726 | field | Terraria.RemoteClient | Terraria/RemoteClient.cs | D:\TRbackup\Version4\Terraria\RemoteClient.cs | 45 | 2 | SpamProjectile | float | `public float SpamProjectile;` | `public float SpamProjectile;` |
| 727 | field | Terraria.RemoteClient | Terraria/RemoteClient.cs | D:\TRbackup\Version4\Terraria\RemoteClient.cs | 47 | 2 | SpamAddBlock | float | `public float SpamAddBlock;` | `public float SpamAddBlock;` |
| 728 | field | Terraria.RemoteClient | Terraria/RemoteClient.cs | D:\TRbackup\Version4\Terraria\RemoteClient.cs | 49 | 2 | SpamDeleteBlock | float | `public float SpamDeleteBlock;` | `public float SpamDeleteBlock;` |
| 729 | field | Terraria.RemoteClient | Terraria/RemoteClient.cs | D:\TRbackup\Version4\Terraria\RemoteClient.cs | 51 | 2 | SpamWater | float | `public float SpamWater;` | `public float SpamWater;` |
| 730 | field | Terraria.RemoteClient | Terraria/RemoteClient.cs | D:\TRbackup\Version4\Terraria\RemoteClient.cs | 53 | 2 | SpamProjectileMax | float | `public float SpamProjectileMax = 100f;` | `public float SpamProjectileMax = 100f;` |
| 731 | field | Terraria.RemoteClient | Terraria/RemoteClient.cs | D:\TRbackup\Version4\Terraria\RemoteClient.cs | 55 | 2 | SpamAddBlockMax | float | `public float SpamAddBlockMax = 100f;` | `public float SpamAddBlockMax = 100f;` |
| 732 | field | Terraria.RemoteClient | Terraria/RemoteClient.cs | D:\TRbackup\Version4\Terraria\RemoteClient.cs | 57 | 2 | SpamDeleteBlockMax | float | `public float SpamDeleteBlockMax = 500f;` | `public float SpamDeleteBlockMax = 500f;` |
| 733 | field | Terraria.RemoteClient | Terraria/RemoteClient.cs | D:\TRbackup\Version4\Terraria\RemoteClient.cs | 59 | 2 | SpamWaterMax | float | `public float SpamWaterMax = 50f;` | `public float SpamWaterMax = 50f;` |
| 734 | field | Terraria.RemoteClient | Terraria/RemoteClient.cs | D:\TRbackup\Version4\Terraria\RemoteClient.cs | 61 | 2 | _isReading | bool | `private volatile bool _isReading;` | `private volatile bool _isReading;` |

#### 属性（1）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 764 | property | Terraria.RemoteClient | Terraria/RemoteClient.cs | D:\TRbackup\Version4\Terraria\RemoteClient.cs | 63 | 2 | ReadBufferFull | bool | `public bool ReadBufferFull => NetMessage.buffer[Id].RemainingReadBufferLength < ReadBuffer.Length;` | `public bool ReadBufferFull => NetMessage.buffer[Id].RemainingReadBufferLength < ReadBuffer.Length;` |


### 4.10 细分子系统：`NetworkSessionConfigurationState`

- 原报告章节：`4.3.8`
- 父级子系统：`NetworkSessionAndSectionStreaming`
- 分区工作包：`13` / `网络协议与会话`

- 上一级基线细分子系统：`NetworkSessionCoordinatorState`
- 上一级 peer 细分子系统：`NetworkSessionCoordinatorState`
- 细分职责：网络端口、连接上限、服务器策略和会话配置。
- 边界角色：`adapter state`；最小 seam：network session configuration port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：11；属性：1；合计：12。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（11）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 683 | field | Terraria.Netplay | Terraria/Netplay.cs | D:\TRbackup\Version4\Terraria\Netplay.cs | 31 | 2 | MaxConnections | int | `public const int MaxConnections = 256;` | `public const int MaxConnections = 256;` |
| 684 | field | Terraria.Netplay | Terraria/Netplay.cs | D:\TRbackup\Version4\Terraria\Netplay.cs | 33 | 2 | NetBufferSize | int | `public const int NetBufferSize = 1024;` | `public const int NetBufferSize = 1024;` |
| 685 | field | Terraria.Netplay | Terraria/Netplay.cs | D:\TRbackup\Version4\Terraria\Netplay.cs | 35 | 2 | DefaultPort | int | `public const int DefaultPort = 7777;` | `public const int DefaultPort = 7777;` |
| 686 | field | Terraria.Netplay | Terraria/Netplay.cs | D:\TRbackup\Version4\Terraria\Netplay.cs | 37 | 2 | BanFilePath | string | `public static string BanFilePath = "banlist.txt";` | `public static string BanFilePath = "banlist.txt";` |
| 687 | field | Terraria.Netplay | Terraria/Netplay.cs | D:\TRbackup\Version4\Terraria\Netplay.cs | 39 | 2 | ServerPassword | string | `public static string ServerPassword = "";` | `public static string ServerPassword = "";` |
| 690 | field | Terraria.Netplay | Terraria/Netplay.cs | D:\TRbackup\Version4\Terraria\Netplay.cs | 45 | 2 | ServerIP | System.Net.IPAddress | `public static IPAddress ServerIP;` | `public static IPAddress ServerIP;` |
| 691 | field | Terraria.Netplay | Terraria/Netplay.cs | D:\TRbackup\Version4\Terraria\Netplay.cs | 47 | 2 | ServerIPText | string | `public static string ServerIPText = "";` | `public static string ServerIPText = "";` |
| 692 | field | Terraria.Netplay | Terraria/Netplay.cs | D:\TRbackup\Version4\Terraria\Netplay.cs | 49 | 2 | IsHostAndPlay | bool | `public static bool IsHostAndPlay;` | `public static bool IsHostAndPlay;` |
| 693 | field | Terraria.Netplay | Terraria/Netplay.cs | D:\TRbackup\Version4\Terraria\Netplay.cs | 51 | 2 | HostToken | string | `public static string HostToken;` | `public static string HostToken;` |
| 697 | field | Terraria.Netplay | Terraria/Netplay.cs | D:\TRbackup\Version4\Terraria\Netplay.cs | 59 | 2 | UseUPNP | bool | `public static bool UseUPNP = true;` | `public static bool UseUPNP = true;` |
| 698 | field | Terraria.Netplay | Terraria/Netplay.cs | D:\TRbackup\Version4\Terraria\Netplay.cs | 61 | 2 | SaveOnServerExit | bool | `public static bool SaveOnServerExit = true;` | `public static bool SaveOnServerExit = true;` |

#### 属性（1）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 763 | property | Terraria.Netplay | Terraria/Netplay.cs | D:\TRbackup\Version4\Terraria\Netplay.cs | 83 | 2 | HandshakeLoggingEnabled | bool | `public static bool HandshakeLoggingEnabled => Program.LaunchParameters.ContainsKey("-handshake-log");` | `public static bool HandshakeLoggingEnabled => Program.LaunchParameters.ContainsKey("-handshake-log");` |


### 4.11 细分子系统：`NetworkSessionTransportAndThreadState`

- 原报告章节：`4.3.9`
- 父级子系统：`NetworkSessionAndSectionStreaming`
- 分区工作包：`13` / `网络协议与会话`

- 上一级基线细分子系统：`NetworkSessionCoordinatorState`
- 上一级 peer 细分子系统：`NetworkSessionCoordinatorState`
- 细分职责：网络客户端、监听器、线程、广播和传输运行状态。
- 边界角色：`adapter state`；最小 seam：network session transport thread port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：15；属性：0；合计：15。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（15）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 688 | field | Terraria.Netplay | Terraria/Netplay.cs | D:\TRbackup\Version4\Terraria\Netplay.cs | 41 | 2 | Clients | Terraria.RemoteClient[] | `public static RemoteClient[] Clients = new RemoteClient[256];` | `public static RemoteClient[] Clients = new RemoteClient[256];` |
| 689 | field | Terraria.Netplay | Terraria/Netplay.cs | D:\TRbackup\Version4\Terraria\Netplay.cs | 43 | 2 | Connection | Terraria.RemoteServer | `public static RemoteServer Connection = new RemoteServer();` | `public static RemoteServer Connection = new RemoteServer();` |
| 694 | field | Terraria.Netplay | Terraria/Netplay.cs | D:\TRbackup\Version4\Terraria\Netplay.cs | 53 | 2 | TcpListener | Terraria.Net.Sockets.ISocket | `public static ISocket TcpListener;` | `public static ISocket TcpListener;` |
| 695 | field | Terraria.Netplay | Terraria/Netplay.cs | D:\TRbackup\Version4\Terraria\Netplay.cs | 55 | 2 | ListenPort | int | `public static int ListenPort = 7777;` | `public static int ListenPort = 7777;` |
| 696 | field | Terraria.Netplay | Terraria/Netplay.cs | D:\TRbackup\Version4\Terraria\Netplay.cs | 57 | 2 | IsListening | bool | `public static bool IsListening = true;` | `public static bool IsListening = true;` |
| 699 | field | Terraria.Netplay | Terraria/Netplay.cs | D:\TRbackup\Version4\Terraria\Netplay.cs | 63 | 2 | Disconnect | bool | `public static bool Disconnect;` | `public static bool Disconnect;` |
| 700 | field | Terraria.Netplay | Terraria/Netplay.cs | D:\TRbackup\Version4\Terraria\Netplay.cs | 65 | 2 | SpamCheck | bool | `public static bool SpamCheck = false;` | `public static bool SpamCheck = false;` |
| 701 | field | Terraria.Netplay | Terraria/Netplay.cs | D:\TRbackup\Version4\Terraria\Netplay.cs | 67 | 2 | HasClients | bool | `public static bool HasClients;` | `public static bool HasClients;` |
| 702 | field | Terraria.Netplay | Terraria/Netplay.cs | D:\TRbackup\Version4\Terraria\Netplay.cs | 69 | 2 | _serverThread | System.Threading.Thread | `private static Thread _serverThread;` | `private static Thread _serverThread;` |
| 703 | field | Terraria.Netplay | Terraria/Netplay.cs | D:\TRbackup\Version4\Terraria\Netplay.cs | 71 | 2 | _upnpnat | NATUPNPLib.UPnPNAT | `private static UPnPNAT _upnpnat;` | `private static UPnPNAT _upnpnat;` |
| 704 | field | Terraria.Netplay | Terraria/Netplay.cs | D:\TRbackup\Version4\Terraria\Netplay.cs | 73 | 2 | _mappings | NATUPNPLib.IStaticPortMappingCollection | `private static IStaticPortMappingCollection _mappings;` | `private static IStaticPortMappingCollection _mappings;` |
| 705 | field | Terraria.Netplay | Terraria/Netplay.cs | D:\TRbackup\Version4\Terraria\Netplay.cs | 75 | 2 | fullBuffer | Terraria.MessageBuffer | `public static MessageBuffer fullBuffer = new MessageBuffer();` | `public static MessageBuffer fullBuffer = new MessageBuffer();` |
| 706 | field | Terraria.Netplay | Terraria/Netplay.cs | D:\TRbackup\Version4\Terraria\Netplay.cs | 77 | 2 | swTicksLast | long | `private static long swTicksLast;` | `private static long swTicksLast;` |
| 707 | field | Terraria.Netplay | Terraria/Netplay.cs | D:\TRbackup\Version4\Terraria\Netplay.cs | 79 | 2 | BroadcastClient | System.Net.Sockets.UdpClient | `private static UdpClient BroadcastClient = null;` | `private static UdpClient BroadcastClient = null;` |
| 708 | field | Terraria.Netplay | Terraria/Netplay.cs | D:\TRbackup\Version4\Terraria\Netplay.cs | 81 | 2 | broadcastThread | System.Threading.Thread | `private static Thread broadcastThread = null;` | `private static Thread broadcastThread = null;` |

#### 属性（0）

无该类型成员记录。


### 4.12 细分子系统：`SharedNetworkSocketTransport`

- 原报告章节：`4.9.37`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`13` / `网络协议与会话`

- 上一级基线细分子系统：`SharedNetworkSocketTransport`
- 细分职责：Socket、调试流和 TCP 传输。
- 边界角色：`adapter`；最小 seam：socket transport port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：2；声明类型数：4；字段：18；属性：6；合计：24。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（18）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2840 | field | Terraria.Net.Sockets.DebugNetworkStream.Packet | Terraria.Net.Sockets/DebugNetworkStream.cs | D:\TRbackup\Version4\Terraria.Net.Sockets\DebugNetworkStream.cs | 14 | 3 | BaseTimestamp | System.DateTime | `public DateTime BaseTimestamp;` | `public DateTime BaseTimestamp;` |
| 2841 | field | Terraria.Net.Sockets.DebugNetworkStream.Packet | Terraria.Net.Sockets/DebugNetworkStream.cs | D:\TRbackup\Version4\Terraria.Net.Sockets\DebugNetworkStream.cs | 16 | 3 | Data | System.ArraySegment<byte> | `public ArraySegment<byte> Data;` | `public ArraySegment<byte> Data;` |
| 2842 | field | Terraria.Net.Sockets.DebugNetworkStream | Terraria.Net.Sockets/DebugNetworkStream.cs | D:\TRbackup\Version4\Terraria.Net.Sockets\DebugNetworkStream.cs | 65 | 2 | Latency | uint | `public static uint Latency;` | `public static uint Latency;` |
| 2843 | field | Terraria.Net.Sockets.DebugNetworkStream | Terraria.Net.Sockets/DebugNetworkStream.cs | D:\TRbackup\Version4\Terraria.Net.Sockets\DebugNetworkStream.cs | 67 | 2 | _stream | System.Net.Sockets.NetworkStream | `private readonly NetworkStream _stream;` | `private readonly NetworkStream _stream;` |
| 2844 | field | Terraria.Net.Sockets.DebugNetworkStream | Terraria.Net.Sockets/DebugNetworkStream.cs | D:\TRbackup\Version4\Terraria.Net.Sockets\DebugNetworkStream.cs | 69 | 2 | _outgoingQueue | System.Collections.Generic.Queue<Terraria.Net.Sockets.DebugNetworkStream.Packet> | `private Queue<Packet> _outgoingQueue = new Queue<Packet>();` | `private Queue<Packet> _outgoingQueue = new Queue<Packet>();` |
| 2845 | field | Terraria.Net.Sockets.DebugNetworkStream | Terraria.Net.Sockets/DebugNetworkStream.cs | D:\TRbackup\Version4\Terraria.Net.Sockets\DebugNetworkStream.cs | 71 | 2 | _incomingQueue | System.Collections.Generic.Queue<Terraria.Net.Sockets.DebugNetworkStream.Packet> | `private Queue<Packet> _incomingQueue = new Queue<Packet>();` | `private Queue<Packet> _incomingQueue = new Queue<Packet>();` |
| 2846 | field | Terraria.Net.Sockets.DebugNetworkStream | Terraria.Net.Sockets/DebugNetworkStream.cs | D:\TRbackup\Version4\Terraria.Net.Sockets\DebugNetworkStream.cs | 73 | 2 | _writeException | System.Exception | `private Exception _writeException;` | `private Exception _writeException;` |
| 2847 | field | Terraria.Net.Sockets.DebugNetworkStream | Terraria.Net.Sockets/DebugNetworkStream.cs | D:\TRbackup\Version4\Terraria.Net.Sockets\DebugNetworkStream.cs | 75 | 2 | _readException | System.Exception | `private Exception _readException;` | `private Exception _readException;` |
| 2848 | field | Terraria.Net.Sockets.DebugNetworkStream | Terraria.Net.Sockets/DebugNetworkStream.cs | D:\TRbackup\Version4\Terraria.Net.Sockets\DebugNetworkStream.cs | 77 | 2 | _readMode | Terraria.Net.Sockets.DebugNetworkStream.ReadMode | `private ReadMode _readMode;` | `private ReadMode _readMode;` |
| 2849 | field | Terraria.Net.Sockets.DebugNetworkStream | Terraria.Net.Sockets/DebugNetworkStream.cs | D:\TRbackup\Version4\Terraria.Net.Sockets\DebugNetworkStream.cs | 79 | 2 | _closed | bool | `private bool _closed;` | `private bool _closed;` |
| 2850 | field | Terraria.Net.Sockets.DebugNetworkStream | Terraria.Net.Sockets/DebugNetworkStream.cs | D:\TRbackup\Version4\Terraria.Net.Sockets\DebugNetworkStream.cs | 81 | 2 | _startTicks | long | `private long _startTicks = Stopwatch.GetTimestamp();` | `private long _startTicks = Stopwatch.GetTimestamp();` |
| 2851 | field | Terraria.Net.Sockets.DebugNetworkStream | Terraria.Net.Sockets/DebugNetworkStream.cs | D:\TRbackup\Version4\Terraria.Net.Sockets\DebugNetworkStream.cs | 83 | 2 | _beginReadBuf | System.ArraySegment<byte> | `private ArraySegment<byte> _beginReadBuf;` | `private ArraySegment<byte> _beginReadBuf;` |
| 2852 | field | Terraria.Net.Sockets.TcpSocket | Terraria.Net.Sockets/TcpSocket.cs | D:\TRbackup\Version4\Terraria.Net.Sockets\TcpSocket.cs | 12 | 2 | _connection | System.Net.Sockets.TcpClient | `private TcpClient _connection;` | `private TcpClient _connection;` |
| 2853 | field | Terraria.Net.Sockets.TcpSocket | Terraria.Net.Sockets/TcpSocket.cs | D:\TRbackup\Version4\Terraria.Net.Sockets\TcpSocket.cs | 14 | 2 | _listener | System.Net.Sockets.TcpListener | `private TcpListener _listener;` | `private TcpListener _listener;` |
| 2854 | field | Terraria.Net.Sockets.TcpSocket | Terraria.Net.Sockets/TcpSocket.cs | D:\TRbackup\Version4\Terraria.Net.Sockets\TcpSocket.cs | 16 | 2 | _listenerCallback | Terraria.Net.Sockets.SocketConnectionAccepted | `private SocketConnectionAccepted _listenerCallback;` | `private SocketConnectionAccepted _listenerCallback;` |
| 2855 | field | Terraria.Net.Sockets.TcpSocket | Terraria.Net.Sockets/TcpSocket.cs | D:\TRbackup\Version4\Terraria.Net.Sockets\TcpSocket.cs | 18 | 2 | _remoteAddress | Terraria.Net.RemoteAddress | `private RemoteAddress _remoteAddress;` | `private RemoteAddress _remoteAddress;` |
| 2856 | field | Terraria.Net.Sockets.TcpSocket | Terraria.Net.Sockets/TcpSocket.cs | D:\TRbackup\Version4\Terraria.Net.Sockets\TcpSocket.cs | 20 | 2 | _isListening | bool | `private bool _isListening;` | `private bool _isListening;` |
| 2857 | field | Terraria.Net.Sockets.TcpSocket | Terraria.Net.Sockets/TcpSocket.cs | D:\TRbackup\Version4\Terraria.Net.Sockets\TcpSocket.cs | 22 | 2 | _debugStream | Terraria.Net.Sockets.DebugNetworkStream | `private DebugNetworkStream _debugStream;` | `private DebugNetworkStream _debugStream;` |

#### 属性（6）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3902 | property | Terraria.Net.Sockets.DebugNetworkStream.CompletedAsyncResult | Terraria.Net.Sockets/DebugNetworkStream.cs | D:\TRbackup\Version4\Terraria.Net.Sockets\DebugNetworkStream.cs | 41 | 3 | AsyncState | object | `public object AsyncState { get; set; }` | `public object AsyncState { get; set; }` |
| 3903 | property | Terraria.Net.Sockets.DebugNetworkStream.CompletedAsyncResult | Terraria.Net.Sockets/DebugNetworkStream.cs | D:\TRbackup\Version4\Terraria.Net.Sockets\DebugNetworkStream.cs | 43 | 3 | Read | int | `public int Read { get; set; }` | `public int Read { get; set; }` |
| 3904 | property | Terraria.Net.Sockets.DebugNetworkStream.CompletedAsyncResult | Terraria.Net.Sockets/DebugNetworkStream.cs | D:\TRbackup\Version4\Terraria.Net.Sockets\DebugNetworkStream.cs | 45 | 3 | IsCompleted | bool | `public bool IsCompleted => true;` | `public bool IsCompleted => true;` |
| 3905 | property | Terraria.Net.Sockets.DebugNetworkStream.CompletedAsyncResult | Terraria.Net.Sockets/DebugNetworkStream.cs | D:\TRbackup\Version4\Terraria.Net.Sockets\DebugNetworkStream.cs | 47 | 3 | CompletedSynchronously | bool | `public bool CompletedSynchronously => true;` | `public bool CompletedSynchronously => true;` |
| 3906 | property | Terraria.Net.Sockets.DebugNetworkStream.CompletedAsyncResult | Terraria.Net.Sockets/DebugNetworkStream.cs | D:\TRbackup\Version4\Terraria.Net.Sockets\DebugNetworkStream.cs | 49 | 3 | AsyncWaitHandle | System.Threading.WaitHandle | `public WaitHandle AsyncWaitHandle { get { throw new NotImplementedException(); } }` | `public WaitHandle AsyncWaitHandle { get { throw new NotImplementedException(); } }` |
| 3907 | property | Terraria.Net.Sockets.DebugNetworkStream | Terraria.Net.Sockets/DebugNetworkStream.cs | D:\TRbackup\Version4\Terraria.Net.Sockets\DebugNetworkStream.cs | 85 | 2 | DataAvailable | bool | `public bool DataAvailable { get { lock (_incomingQueue) { if (_incomingQueue.Count > 0) { return _incomingQueue.Peek().IsReady(); } if (_readMode == ReadMode.None) { return _stream.DataAvailable; } return false; } } }` | `public bool DataAvailable { get { lock (_incomingQueue) { if (_incomingQueue.Count > 0) { return _incomingQueue.Peek().IsReady(); } if (_readMode == ReadMode.None) { return _stream.DataAvailable; } return false; } } }` |


### 4.13 细分子系统：`SharedNetworkPacketPrimitives`

- 原报告章节：`4.9.38`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`13` / `网络协议与会话`

- 上一级基线细分子系统：`SharedNetworkPacketPrimitives`
- 细分职责：网络包、地址和缓冲池原语。
- 边界角色：`adapter`；最小 seam：packet buffer port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：5；声明类型数：6；字段：22；属性：3；合计：25。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（22）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2858 | field | Terraria.Net.LegacyNetBufferPool | Terraria.Net/LegacyNetBufferPool.cs | D:\TRbackup\Version4\Terraria.Net\LegacyNetBufferPool.cs | 8 | 2 | SMALL_BUFFER_SIZE | int | `private const int SMALL_BUFFER_SIZE = 256;` | `private const int SMALL_BUFFER_SIZE = 256;` |
| 2859 | field | Terraria.Net.LegacyNetBufferPool | Terraria.Net/LegacyNetBufferPool.cs | D:\TRbackup\Version4\Terraria.Net\LegacyNetBufferPool.cs | 10 | 2 | MEDIUM_BUFFER_SIZE | int | `private const int MEDIUM_BUFFER_SIZE = 1024;` | `private const int MEDIUM_BUFFER_SIZE = 1024;` |
| 2860 | field | Terraria.Net.LegacyNetBufferPool | Terraria.Net/LegacyNetBufferPool.cs | D:\TRbackup\Version4\Terraria.Net\LegacyNetBufferPool.cs | 12 | 2 | LARGE_BUFFER_SIZE | int | `private const int LARGE_BUFFER_SIZE = 16384;` | `private const int LARGE_BUFFER_SIZE = 16384;` |
| 2861 | field | Terraria.Net.LegacyNetBufferPool | Terraria.Net/LegacyNetBufferPool.cs | D:\TRbackup\Version4\Terraria.Net\LegacyNetBufferPool.cs | 14 | 2 | bufferLock | object | `private static object bufferLock = new object();` | `private static object bufferLock = new object();` |
| 2862 | field | Terraria.Net.LegacyNetBufferPool | Terraria.Net/LegacyNetBufferPool.cs | D:\TRbackup\Version4\Terraria.Net\LegacyNetBufferPool.cs | 16 | 2 | _smallBufferQueue | System.Collections.Generic.Queue<byte[]> | `private static Queue<byte[]> _smallBufferQueue = new Queue<byte[]>();` | `private static Queue<byte[]> _smallBufferQueue = new Queue<byte[]>();` |
| 2863 | field | Terraria.Net.LegacyNetBufferPool | Terraria.Net/LegacyNetBufferPool.cs | D:\TRbackup\Version4\Terraria.Net\LegacyNetBufferPool.cs | 18 | 2 | _mediumBufferQueue | System.Collections.Generic.Queue<byte[]> | `private static Queue<byte[]> _mediumBufferQueue = new Queue<byte[]>();` | `private static Queue<byte[]> _mediumBufferQueue = new Queue<byte[]>();` |
| 2864 | field | Terraria.Net.LegacyNetBufferPool | Terraria.Net/LegacyNetBufferPool.cs | D:\TRbackup\Version4\Terraria.Net\LegacyNetBufferPool.cs | 20 | 2 | _largeBufferQueue | System.Collections.Generic.Queue<byte[]> | `private static Queue<byte[]> _largeBufferQueue = new Queue<byte[]>();` | `private static Queue<byte[]> _largeBufferQueue = new Queue<byte[]>();` |
| 2865 | field | Terraria.Net.LegacyNetBufferPool | Terraria.Net/LegacyNetBufferPool.cs | D:\TRbackup\Version4\Terraria.Net\LegacyNetBufferPool.cs | 22 | 2 | _smallBufferCount | int | `private static int _smallBufferCount;` | `private static int _smallBufferCount;` |
| 2866 | field | Terraria.Net.LegacyNetBufferPool | Terraria.Net/LegacyNetBufferPool.cs | D:\TRbackup\Version4\Terraria.Net\LegacyNetBufferPool.cs | 24 | 2 | _mediumBufferCount | int | `private static int _mediumBufferCount;` | `private static int _mediumBufferCount;` |
| 2867 | field | Terraria.Net.LegacyNetBufferPool | Terraria.Net/LegacyNetBufferPool.cs | D:\TRbackup\Version4\Terraria.Net\LegacyNetBufferPool.cs | 26 | 2 | _largeBufferCount | int | `private static int _largeBufferCount;` | `private static int _largeBufferCount;` |
| 2868 | field | Terraria.Net.LegacyNetBufferPool | Terraria.Net/LegacyNetBufferPool.cs | D:\TRbackup\Version4\Terraria.Net\LegacyNetBufferPool.cs | 28 | 2 | _customBufferCount | int | `private static int _customBufferCount;` | `private static int _customBufferCount;` |
| 2869 | field | Terraria.Net.NetManager.PacketTypeStorage<T> | Terraria.Net/NetManager.cs | D:\TRbackup\Version4\Terraria.Net\NetManager.cs | 11 | 3 | Id | ushort | `public static ushort Id;` | `public static ushort Id;` |
| 2870 | field | Terraria.Net.NetManager.PacketTypeStorage<T> | Terraria.Net/NetManager.cs | D:\TRbackup\Version4\Terraria.Net\NetManager.cs | 13 | 3 | Module | T | `public static T Module;` | `public static T Module;` |
| 2871 | field | Terraria.Net.NetManager | Terraria.Net/NetManager.cs | D:\TRbackup\Version4\Terraria.Net\NetManager.cs | 18 | 2 | Instance | Terraria.Net.NetManager | `public static readonly NetManager Instance = new NetManager();` | `public static readonly NetManager Instance = new NetManager();` |
| 2872 | field | Terraria.Net.NetManager | Terraria.Net/NetManager.cs | D:\TRbackup\Version4\Terraria.Net\NetManager.cs | 20 | 2 | _modules | System.Collections.Generic.Dictionary<ushort, Terraria.Net.NetModule> | `private Dictionary<ushort, NetModule> _modules = new Dictionary<ushort, NetModule>();` | `private Dictionary<ushort, NetModule> _modules = new Dictionary<ushort, NetModule>();` |
| 2873 | field | Terraria.Net.NetManager | Terraria.Net/NetManager.cs | D:\TRbackup\Version4\Terraria.Net\NetManager.cs | 22 | 2 | _moduleCount | ushort | `private ushort _moduleCount;` | `private ushort _moduleCount;` |
| 2874 | field | Terraria.Net.NetPacket | Terraria.Net/NetPacket.cs | D:\TRbackup\Version4\Terraria.Net\NetPacket.cs | 9 | 2 | HEADER_SIZE | int | `public const int HEADER_SIZE = 5;` | `public const int HEADER_SIZE = 5;` |
| 2875 | field | Terraria.Net.NetPacket | Terraria.Net/NetPacket.cs | D:\TRbackup\Version4\Terraria.Net\NetPacket.cs | 11 | 2 | Id | ushort | `public readonly ushort Id;` | `public readonly ushort Id;` |
| 2876 | field | Terraria.Net.NetPacket | Terraria.Net/NetPacket.cs | D:\TRbackup\Version4\Terraria.Net\NetPacket.cs | 13 | 2 | Buffer | Terraria.DataStructures.CachedBuffer | `public readonly CachedBuffer Buffer;` | `public readonly CachedBuffer Buffer;` |
| 2877 | field | Terraria.Net.RemoteAddress | Terraria.Net/RemoteAddress.cs | D:\TRbackup\Version4\Terraria.Net\RemoteAddress.cs | 5 | 2 | Type | Terraria.Net.AddressType | `public AddressType Type;` | `public AddressType Type;` |
| 2878 | field | Terraria.Net.TcpAddress | Terraria.Net/TcpAddress.cs | D:\TRbackup\Version4\Terraria.Net\TcpAddress.cs | 7 | 2 | Address | System.Net.IPAddress | `public IPAddress Address;` | `public IPAddress Address;` |
| 2879 | field | Terraria.Net.TcpAddress | Terraria.Net/TcpAddress.cs | D:\TRbackup\Version4\Terraria.Net\TcpAddress.cs | 9 | 2 | Port | int | `public int Port;` | `public int Port;` |

#### 属性（3）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3908 | property | Terraria.Net.NetPacket | Terraria.Net/NetPacket.cs | D:\TRbackup\Version4\Terraria.Net\NetPacket.cs | 15 | 2 | Length | int | `public int Length { get; private set; }` | `public int Length { get; private set; }` |
| 3909 | property | Terraria.Net.NetPacket | Terraria.Net/NetPacket.cs | D:\TRbackup\Version4\Terraria.Net\NetPacket.cs | 17 | 2 | Writer | System.IO.BinaryWriter | `public BinaryWriter Writer => Buffer.Writer;` | `public BinaryWriter Writer => Buffer.Writer;` |
| 3910 | property | Terraria.Net.NetPacket | Terraria.Net/NetPacket.cs | D:\TRbackup\Version4\Terraria.Net\NetPacket.cs | 19 | 2 | Reader | System.IO.BinaryReader | `public BinaryReader Reader => Buffer.Reader;` | `public BinaryReader Reader => Buffer.Reader;` |


### 4.14 细分子系统：`SharedContentNetworkModules`

- 原报告章节：`4.9.39`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`13` / `网络协议与会话`

- 上一级基线细分子系统：`SharedContentNetworkModules`
- 细分职责：内容能力和液体网络模块。
- 边界角色：`adapter`；最小 seam：content network module；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：2；声明类型数：3；字段：6；属性：0；合计：6。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（6）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2139 | field | Terraria.GameContent.NetModules.NetCreativePowerPermissionsModule | Terraria.GameContent.NetModules/NetCreativePowerPermissionsModule.cs | D:\TRbackup\Version4\Terraria.GameContent.NetModules\NetCreativePowerPermissionsModule.cs | 9 | 2 | _setPermissionLevelId | byte | `private const byte _setPermissionLevelId = 0;` | `private const byte _setPermissionLevelId = 0;` |
| 2140 | field | Terraria.GameContent.NetModules.NetLiquidModule.ChunkChanges | Terraria.GameContent.NetModules/NetLiquidModule.cs | D:\TRbackup\Version4\Terraria.GameContent.NetModules\NetLiquidModule.cs | 12 | 3 | DirtiedPackedTileCoords | System.Collections.Generic.HashSet<int> | `public HashSet<int> DirtiedPackedTileCoords;` | `public HashSet<int> DirtiedPackedTileCoords;` |
| 2141 | field | Terraria.GameContent.NetModules.NetLiquidModule.ChunkChanges | Terraria.GameContent.NetModules/NetLiquidModule.cs | D:\TRbackup\Version4\Terraria.GameContent.NetModules\NetLiquidModule.cs | 14 | 3 | ChunkX | int | `public int ChunkX;` | `public int ChunkX;` |
| 2142 | field | Terraria.GameContent.NetModules.NetLiquidModule.ChunkChanges | Terraria.GameContent.NetModules/NetLiquidModule.cs | D:\TRbackup\Version4\Terraria.GameContent.NetModules\NetLiquidModule.cs | 16 | 3 | ChunkY | int | `public int ChunkY;` | `public int ChunkY;` |
| 2143 | field | Terraria.GameContent.NetModules.NetLiquidModule | Terraria.GameContent.NetModules/NetLiquidModule.cs | D:\TRbackup\Version4\Terraria.GameContent.NetModules\NetLiquidModule.cs | 33 | 2 | _changesForPlayerCache | System.Collections.Generic.List<int> | `private static List<int> _changesForPlayerCache = new List<int>();` | `private static List<int> _changesForPlayerCache = new List<int>();` |
| 2144 | field | Terraria.GameContent.NetModules.NetLiquidModule | Terraria.GameContent.NetModules/NetLiquidModule.cs | D:\TRbackup\Version4\Terraria.GameContent.NetModules\NetLiquidModule.cs | 35 | 2 | _changesByChunkCoords | System.Collections.Generic.Dictionary<Point, Terraria.GameContent.NetModules.NetLiquidModule.ChunkChanges> | `private static Dictionary<Point, ChunkChanges> _changesByChunkCoords = new Dictionary<Point, ChunkChanges>();` | `private static Dictionary<Point, ChunkChanges> _changesByChunkCoords = new Dictionary<Point, ChunkChanges>();` |

#### 属性（0）

无该类型成员记录。


### 4.15 细分子系统：`SharedNetworkSectionProjections`

- 原报告章节：`4.9.40`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`13` / `网络协议与会话`

- 上一级基线细分子系统：`SharedNetworkSectionProjections`
- 细分职责：区段和花朵包的网络投影载荷。
- 边界角色：`projection`；最小 seam：section projection；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：2；声明类型数：2；字段：6；属性：0；合计：6。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（6）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1028 | field | Terraria.DataStructures.ActiveSections | Terraria.DataStructures/ActiveSections.cs | D:\TRbackup\Version4\Terraria.DataStructures\ActiveSections.cs | 8 | 2 | SectionInactiveTime | uint | `public static readonly uint SectionInactiveTime = 60u;` | `public static readonly uint SectionInactiveTime = 60u;` |
| 1029 | field | Terraria.DataStructures.ActiveSections | Terraria.DataStructures/ActiveSections.cs | D:\TRbackup\Version4\Terraria.DataStructures\ActiveSections.cs | 10 | 2 | LastActiveTime | uint[,] | `private static uint[,] LastActiveTime = new uint[Main.maxTilesX / 200 + 1, Main.maxTilesY / 150 + 1];` | `private static uint[,] LastActiveTime = new uint[Main.maxTilesX / 200 + 1, Main.maxTilesY / 150 + 1];` |
| 1165 | field | Terraria.DataStructures.FlowerPacketInfo | Terraria.DataStructures/FlowerPacketInfo.cs | D:\TRbackup\Version4\Terraria.DataStructures\FlowerPacketInfo.cs | 7 | 2 | stylesOnPurity | System.Collections.Generic.List<int> | `public List<int> stylesOnPurity = new List<int>();` | `public List<int> stylesOnPurity = new List<int>();` |
| 1166 | field | Terraria.DataStructures.FlowerPacketInfo | Terraria.DataStructures/FlowerPacketInfo.cs | D:\TRbackup\Version4\Terraria.DataStructures\FlowerPacketInfo.cs | 9 | 2 | stylesOnCorruption | System.Collections.Generic.List<int> | `public List<int> stylesOnCorruption = new List<int>();` | `public List<int> stylesOnCorruption = new List<int>();` |
| 1167 | field | Terraria.DataStructures.FlowerPacketInfo | Terraria.DataStructures/FlowerPacketInfo.cs | D:\TRbackup\Version4\Terraria.DataStructures\FlowerPacketInfo.cs | 11 | 2 | stylesOnCrimson | System.Collections.Generic.List<int> | `public List<int> stylesOnCrimson = new List<int>();` | `public List<int> stylesOnCrimson = new List<int>();` |
| 1168 | field | Terraria.DataStructures.FlowerPacketInfo | Terraria.DataStructures/FlowerPacketInfo.cs | D:\TRbackup\Version4\Terraria.DataStructures\FlowerPacketInfo.cs | 13 | 2 | stylesOnHallow | System.Collections.Generic.List<int> | `public List<int> stylesOnHallow = new List<int>();` | `public List<int> stylesOnHallow = new List<int>();` |

#### 属性（0）

无该类型成员记录。


### 4.16 细分子系统：`SharedChatAndCommandProtocol`

- 原报告章节：`4.9.41`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`13` / `网络协议与会话`

- 上一级基线细分子系统：`SharedChatAndCommandProtocol`
- 细分职责：聊天消息、颜色和命令处理协议。
- 边界角色：`adapter`；最小 seam：chat command port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：8；声明类型数：8；字段：15；属性：3；合计：18。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（15）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1013 | field | Terraria.Chat.Commands.ChatCommandAttribute | Terraria.Chat.Commands/ChatCommandAttribute.cs | D:\TRbackup\Version4\Terraria.Chat.Commands\ChatCommandAttribute.cs | 8 | 2 | Name | string | `public readonly string Name;` | `public readonly string Name;` |
| 1014 | field | Terraria.Chat.Commands.EmojiCommand | Terraria.Chat.Commands/EmojiCommand.cs | D:\TRbackup\Version4\Terraria.Chat.Commands\EmojiCommand.cs | 11 | 2 | PlayerEmojiDuration | int | `public const int PlayerEmojiDuration = 360;` | `public const int PlayerEmojiDuration = 360;` |
| 1015 | field | Terraria.Chat.Commands.EmojiCommand | Terraria.Chat.Commands/EmojiCommand.cs | D:\TRbackup\Version4\Terraria.Chat.Commands\EmojiCommand.cs | 13 | 2 | _byName | System.Collections.Generic.Dictionary<Terraria.Localization.LocalizedText, int> | `private readonly Dictionary<LocalizedText, int> _byName = new Dictionary<LocalizedText, int>();` | `private readonly Dictionary<LocalizedText, int> _byName = new Dictionary<LocalizedText, int>();` |
| 1016 | field | Terraria.Chat.Commands.EmoteCommand | Terraria.Chat.Commands/EmoteCommand.cs | D:\TRbackup\Version4\Terraria.Chat.Commands\EmoteCommand.cs | 9 | 2 | RESPONSE_COLOR | Color | `private static readonly Color RESPONSE_COLOR = new Color(200, 100, 0);` | `private static readonly Color RESPONSE_COLOR = new Color(200, 100, 0);` |
| 1017 | field | Terraria.Chat.ChatColors | Terraria.Chat/ChatColors.cs | D:\TRbackup\Version4\Terraria.Chat\ChatColors.cs | 7 | 2 | BossOrEvent | Color | `public static readonly Color BossOrEvent = new Color(175, 75, 255);` | `public static readonly Color BossOrEvent = new Color(175, 75, 255);` |
| 1018 | field | Terraria.Chat.ChatColors | Terraria.Chat/ChatColors.cs | D:\TRbackup\Version4\Terraria.Chat\ChatColors.cs | 9 | 2 | World | Color | `public static readonly Color World = new Color(50, 255, 130);` | `public static readonly Color World = new Color(50, 255, 130);` |
| 1019 | field | Terraria.Chat.ChatColors | Terraria.Chat/ChatColors.cs | D:\TRbackup\Version4\Terraria.Chat\ChatColors.cs | 11 | 2 | NPCTravel | Color | `public static readonly Color NPCTravel = new Color(50, 125, 255);` | `public static readonly Color NPCTravel = new Color(50, 125, 255);` |
| 1020 | field | Terraria.Chat.ChatColors | Terraria.Chat/ChatColors.cs | D:\TRbackup\Version4\Terraria.Chat\ChatColors.cs | 13 | 2 | ServerMessage | Color | `public static readonly Color ServerMessage = new Color(255, 240, 20);` | `public static readonly Color ServerMessage = new Color(255, 240, 20);` |
| 1021 | field | Terraria.Chat.ChatColors | Terraria.Chat/ChatColors.cs | D:\TRbackup\Version4\Terraria.Chat\ChatColors.cs | 15 | 2 | Death | Color | `public static readonly Color Death = new Color(255, 25, 25);` | `public static readonly Color Death = new Color(255, 25, 25);` |
| 1022 | field | Terraria.Chat.ChatCommandId | Terraria.Chat/ChatCommandId.cs | D:\TRbackup\Version4\Terraria.Chat\ChatCommandId.cs | 10 | 2 | _name | string | `private readonly string _name;` | `private readonly string _name;` |
| 1023 | field | Terraria.Chat.ChatCommandProcessor | Terraria.Chat/ChatCommandProcessor.cs | D:\TRbackup\Version4\Terraria.Chat\ChatCommandProcessor.cs | 11 | 2 | _localizedCommands | System.Collections.Generic.Dictionary<Terraria.Localization.LocalizedText, Terraria.Chat.ChatCommandId> | `private readonly Dictionary<LocalizedText, ChatCommandId> _localizedCommands = new Dictionary<LocalizedText, ChatCommandId>();` | `private readonly Dictionary<LocalizedText, ChatCommandId> _localizedCommands = new Dictionary<LocalizedText, ChatCommandId>();` |
| 1024 | field | Terraria.Chat.ChatCommandProcessor | Terraria.Chat/ChatCommandProcessor.cs | D:\TRbackup\Version4\Terraria.Chat\ChatCommandProcessor.cs | 13 | 2 | _commands | System.Collections.Generic.Dictionary<Terraria.Chat.ChatCommandId, Terraria.Chat.Commands.IChatCommand> | `private readonly Dictionary<ChatCommandId, IChatCommand> _commands = new Dictionary<ChatCommandId, IChatCommand>();` | `private readonly Dictionary<ChatCommandId, IChatCommand> _commands = new Dictionary<ChatCommandId, IChatCommand>();` |
| 1025 | field | Terraria.Chat.ChatCommandProcessor | Terraria.Chat/ChatCommandProcessor.cs | D:\TRbackup\Version4\Terraria.Chat\ChatCommandProcessor.cs | 15 | 2 | _aliases | System.Collections.Generic.Dictionary<Terraria.Localization.LocalizedText, System.Func<string>> | `private Dictionary<LocalizedText, Func<string>> _aliases = new Dictionary<LocalizedText, Func<string>>();` | `private Dictionary<LocalizedText, Func<string>> _aliases = new Dictionary<LocalizedText, Func<string>>();` |
| 1026 | field | Terraria.Chat.ChatCommandProcessor | Terraria.Chat/ChatCommandProcessor.cs | D:\TRbackup\Version4\Terraria.Chat\ChatCommandProcessor.cs | 17 | 2 | _defaultCommand | Terraria.Chat.Commands.IChatCommand | `private IChatCommand _defaultCommand;` | `private IChatCommand _defaultCommand;` |
| 1027 | field | Terraria.Chat.ChatHelper | Terraria.Chat/ChatHelper.cs | D:\TRbackup\Version4\Terraria.Chat\ChatHelper.cs | 13 | 2 | _cachedMessages | System.Collections.Generic.List<System.Tuple<string, Color>> | `private static List<Tuple<string, Color>> _cachedMessages = new List<Tuple<string, Color>>();` | `private static List<Tuple<string, Color>> _cachedMessages = new List<Tuple<string, Color>>();` |

#### 属性（3）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3682 | property | Terraria.Chat.ChatMessage | Terraria.Chat/ChatMessage.cs | D:\TRbackup\Version4\Terraria.Chat\ChatMessage.cs | 10 | 2 | CommandId | Terraria.Chat.ChatCommandId | `public ChatCommandId CommandId { get; private set; }` | `public ChatCommandId CommandId { get; private set; }` |
| 3683 | property | Terraria.Chat.ChatMessage | Terraria.Chat/ChatMessage.cs | D:\TRbackup\Version4\Terraria.Chat\ChatMessage.cs | 12 | 2 | Text | string | `public string Text { get; set; }` | `public string Text { get; set; }` |
| 3684 | property | Terraria.Chat.ChatMessage | Terraria.Chat/ChatMessage.cs | D:\TRbackup\Version4\Terraria.Chat\ChatMessage.cs | 14 | 2 | IsConsumed | bool | `public bool IsConsumed { get; private set; }` | `public bool IsConsumed { get; private set; }` |


### 4.17 细分子系统：`SharedChatSnippetPresentationState`

- 原报告章节：`4.9.148`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`13` / `网络协议与会话`

- 上一级基线细分子系统：`SharedChatPresentation`
- 细分职责：文本、标签、字形和定位片段表现状态。
- 边界角色：`projection/presentation`；最小 seam：chat snippet presentation port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：5；声明类型数：6；字段：19；属性：0；合计：19。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（19）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2233 | field | Terraria.GameContent.UI.Chat.AchievementTagHandler.AchievementSnippet | Terraria.GameContent.UI.Chat/AchievementTagHandler.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Chat\AchievementTagHandler.cs | 12 | 3 | _achievement | Terraria.Achievements.Achievement | `private Achievement _achievement;` | `private Achievement _achievement;` |
| 2234 | field | Terraria.GameContent.UI.Chat.GlyphTagHandler.GlyphSnippet | Terraria.GameContent.UI.Chat/GlyphTagHandler.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Chat\GlyphTagHandler.cs | 32 | 3 | ForcedStyle | int | `public int ForcedStyle = -1;` | `public int ForcedStyle = -1;` |
| 2235 | field | Terraria.GameContent.UI.Chat.GlyphTagHandler.GlyphSnippet | Terraria.GameContent.UI.Chat/GlyphTagHandler.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Chat\GlyphTagHandler.cs | 34 | 3 | _glyphIndex | int | `private int _glyphIndex;` | `private int _glyphIndex;` |
| 2236 | field | Terraria.GameContent.UI.Chat.GlyphTagHandler | Terraria.GameContent.UI.Chat/GlyphTagHandler.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Chat\GlyphTagHandler.cs | 54 | 2 | GlyphsPerLine | int | `private const int GlyphsPerLine = 25;` | `private const int GlyphsPerLine = 25;` |
| 2237 | field | Terraria.GameContent.UI.Chat.GlyphTagHandler | Terraria.GameContent.UI.Chat/GlyphTagHandler.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Chat\GlyphTagHandler.cs | 56 | 2 | MaxGlyphs | int | `private const int MaxGlyphs = 26;` | `private const int MaxGlyphs = 26;` |
| 2238 | field | Terraria.GameContent.UI.Chat.GlyphTagHandler | Terraria.GameContent.UI.Chat/GlyphTagHandler.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Chat\GlyphTagHandler.cs | 58 | 2 | DefaultGlyphStyle | int | `public const int DefaultGlyphStyle = -1;` | `public const int DefaultGlyphStyle = -1;` |
| 2239 | field | Terraria.GameContent.UI.Chat.GlyphTagHandler | Terraria.GameContent.UI.Chat/GlyphTagHandler.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Chat\GlyphTagHandler.cs | 60 | 2 | GlyphStyle | int | `public static int GlyphStyle = -1;` | `public static int GlyphStyle = -1;` |
| 2240 | field | Terraria.GameContent.UI.Chat.GlyphTagHandler | Terraria.GameContent.UI.Chat/GlyphTagHandler.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Chat\GlyphTagHandler.cs | 62 | 2 | GlyphIndexes | System.Collections.Generic.Dictionary<string, int> | `private static Dictionary<string, int> GlyphIndexes = new Dictionary<string, int>  	{  		{  			Buttons.A.ToString(),  			0  		},  		{  			Buttons.B.ToString(),  			1  		},  		{  			Buttons.Back.ToString(),  			4  		},  		{  			Buttons.DPadDown.ToString(),  			15  		},  		{  			Buttons.DPadLeft.ToString(),  			14  		},  		{  			Buttons.DPadRight.ToString(),  			13  		},  		{  			Buttons.DPadUp.ToString(),  			16  		},  		{  			Buttons.LeftShoulder.ToString(),  			6  		},  		{  			Buttons.LeftStick.ToString(),  			10  		},  		{  			Buttons.LeftThumbstickDown.ToString(),  			20  		},  		{  			Buttons.LeftThumbstickLeft.ToString(),  			17  		},  		{  			Buttons.LeftThumbstickRight.ToString(),  			18  		},  		{  			Buttons.LeftThumbstickUp.ToString(),  			19  		},  		{  			Buttons.LeftTrigger.ToString(),  			8  		},  		{  			Buttons.RightShoulder.ToString(),  			7  		},  		{  			Buttons.RightStick.ToString(),  			11  		},  		{  			Buttons.RightThumbstickDown.ToString(),  			24  		},  		{  			Buttons.RightThumbstickLeft.ToString(),  			21  		},  		{  			Buttons.RightThumbstickRight.ToString(),  			22  		},  		{  			Buttons.RightThumbstickUp.ToString(),  			23  		},  		{  			Buttons.RightTrigger.ToString(),  			9  		},  		{  			Buttons.Start.ToString(),  			5  		},  		{  			Buttons.X.ToString(),  			2  		},  		{  			Buttons.Y.ToString(),  			3  		},  		{ "RightStickAxis", 12 },  		{ "LR", 25 }  	};` | `private static Dictionary<string, int> GlyphIndexes = new Dictionary<string, int> { { Buttons.A.ToString(), 0 }, { Buttons.B.ToString(), 1 }, { Buttons.Back.ToString(), 4 }, { Buttons.DPadDown.ToString(), 15 }, { Buttons.DPadLeft.ToString(), 14 }, { Buttons.DPadRight.ToString(), 13 }, { Buttons.DPadUp.ToString(), 16 }, { Buttons.LeftShoulder.ToString(), 6 }, { Buttons.LeftStick.ToString(), 10 }, { Buttons.LeftThumbstickDown.ToString(), 20 }, { Buttons.LeftThumbstickLeft.ToString(), 17 }, { Buttons.LeftThumbstickRight.ToString(), 18 }, { Buttons.LeftThumbstickUp.ToString(), 19 }, { Buttons.LeftTrigger.ToString(), 8 }, { Buttons.RightShoulder.ToString(), 7 }, { Buttons.RightStick.ToString(), 11 }, { Buttons.RightThumbstickDown.ToString(), 24 }, { Buttons.RightThumbstickLeft.ToString(), 21 }, { Buttons.RightThumbstickRight.ToString(), 22 }, { Buttons.RightThumbstickUp.ToString(), 23 }, { Buttons.RightTrigger.ToString(), 9 }, { Buttons.Start.ToString(), 5 }, { Buttons.X.ToString(), 2 }, { Buttons.Y.ToString(), 3 }, { "RightStickAxis", 12 }, { "LR", 25 } };` |
| 2241 | field | Terraria.GameContent.UI.Chat.ItemTagHandler.ItemSnippet | Terraria.GameContent.UI.Chat/ItemTagHandler.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Chat\ItemTagHandler.cs | 13 | 3 | _item | Terraria.Item | `private Item _item;` | `private Item _item;` |
| 2939 | field | Terraria.UI.Chat.PositionedSnippet | Terraria.UI.Chat/PositionedSnippet.cs | D:\TRbackup\Version4\Terraria.UI.Chat\PositionedSnippet.cs | 7 | 2 | Snippet | Terraria.UI.Chat.TextSnippet | `public readonly TextSnippet Snippet;` | `public readonly TextSnippet Snippet;` |
| 2940 | field | Terraria.UI.Chat.PositionedSnippet | Terraria.UI.Chat/PositionedSnippet.cs | D:\TRbackup\Version4\Terraria.UI.Chat\PositionedSnippet.cs | 9 | 2 | OrigIndex | int | `public readonly int OrigIndex;` | `public readonly int OrigIndex;` |
| 2941 | field | Terraria.UI.Chat.PositionedSnippet | Terraria.UI.Chat/PositionedSnippet.cs | D:\TRbackup\Version4\Terraria.UI.Chat\PositionedSnippet.cs | 11 | 2 | Line | int | `public readonly int Line;` | `public readonly int Line;` |
| 2942 | field | Terraria.UI.Chat.PositionedSnippet | Terraria.UI.Chat/PositionedSnippet.cs | D:\TRbackup\Version4\Terraria.UI.Chat\PositionedSnippet.cs | 13 | 2 | Position | Vector2 | `public Vector2 Position;` | `public Vector2 Position;` |
| 2943 | field | Terraria.UI.Chat.PositionedSnippet | Terraria.UI.Chat/PositionedSnippet.cs | D:\TRbackup\Version4\Terraria.UI.Chat\PositionedSnippet.cs | 15 | 2 | Size | Vector2 | `public Vector2 Size;` | `public Vector2 Size;` |
| 2944 | field | Terraria.UI.Chat.TextSnippet | Terraria.UI.Chat/TextSnippet.cs | D:\TRbackup\Version4\Terraria.UI.Chat\TextSnippet.cs | 8 | 2 | Text | string | `public string Text;` | `public string Text;` |
| 2945 | field | Terraria.UI.Chat.TextSnippet | Terraria.UI.Chat/TextSnippet.cs | D:\TRbackup\Version4\Terraria.UI.Chat\TextSnippet.cs | 10 | 2 | TextOriginal | string | `public string TextOriginal;` | `public string TextOriginal;` |
| 2946 | field | Terraria.UI.Chat.TextSnippet | Terraria.UI.Chat/TextSnippet.cs | D:\TRbackup\Version4\Terraria.UI.Chat\TextSnippet.cs | 12 | 2 | Color | Color | `public Color Color = Color.White;` | `public Color Color = Color.White;` |
| 2947 | field | Terraria.UI.Chat.TextSnippet | Terraria.UI.Chat/TextSnippet.cs | D:\TRbackup\Version4\Terraria.UI.Chat\TextSnippet.cs | 14 | 2 | CheckForHover | bool | `public bool CheckForHover;` | `public bool CheckForHover;` |
| 2948 | field | Terraria.UI.Chat.TextSnippet | Terraria.UI.Chat/TextSnippet.cs | D:\TRbackup\Version4\Terraria.UI.Chat\TextSnippet.cs | 16 | 2 | DeleteWhole | bool | `public bool DeleteWhole;` | `public bool DeleteWhole;` |

#### 属性（0）

无该类型成员记录。


### 4.18 细分子系统：`SharedChatMonitorAndCommandState`

- 原报告章节：`4.9.149`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`13` / `网络协议与会话`

- 上一级基线细分子系统：`SharedChatPresentation`
- 细分职责：聊天监视器、消息缓存和命令格式化处理状态。
- 边界角色：`projection/adapter`；最小 seam：chat monitor command port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：3；声明类型数：4；字段：12；属性：0；合计：12。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（12）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2242 | field | Terraria.GameContent.UI.Chat.RemadeChatMonitor | Terraria.GameContent.UI.Chat/RemadeChatMonitor.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Chat\RemadeChatMonitor.cs | 13 | 2 | MaxMessages | int | `private const int MaxMessages = 500;` | `private const int MaxMessages = 500;` |
| 2243 | field | Terraria.GameContent.UI.Chat.RemadeChatMonitor | Terraria.GameContent.UI.Chat/RemadeChatMonitor.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Chat\RemadeChatMonitor.cs | 15 | 2 | _messages | System.Collections.Generic.List<Terraria.UI.Chat.ChatMessageContainer> | `private List<ChatMessageContainer> _messages;` | `private List<ChatMessageContainer> _messages;` |
| 2244 | field | Terraria.GameContent.UI.Chat.RemadeChatMonitor | Terraria.GameContent.UI.Chat/RemadeChatMonitor.cs | D:\TRbackup\Version4\Terraria.GameContent.UI.Chat\RemadeChatMonitor.cs | 17 | 2 | _lastChatWidthLimit | int | `private int _lastChatWidthLimit;` | `private int _lastChatWidthLimit;` |
| 2930 | field | Terraria.UI.Chat.ChatManager.Regexes | Terraria.UI.Chat/ChatManager.cs | D:\TRbackup\Version4\Terraria.UI.Chat\ChatManager.cs | 20 | 3 | Format | System.Text.RegularExpressions.Regex | `public static readonly Regex Format = new Regex("(?<!\\\\)\\[(?<tag>[a-zA-Z]{1,10})(\\/(?<options>[^:]+))?:(?<text>.+?)(?<!\\\\)\\]", RegexOptions.Compiled \| RegexOptions.Singleline);` | `public static readonly Regex Format = new Regex("(?<!\\\\)\\[(?<tag>[a-zA-Z]{1,10})(\\/(?<options>[^:]+))?:(?<text>.+?)(?<!\\\\)\\]", RegexOptions.Compiled \| RegexOptions.Singleline);` |
| 2931 | field | Terraria.UI.Chat.ChatManager | Terraria.UI.Chat/ChatManager.cs | D:\TRbackup\Version4\Terraria.UI.Chat\ChatManager.cs | 23 | 2 | DebugCommands | Terraria.Testing.ChatCommands.DebugCommandProcessor | `public static readonly DebugCommandProcessor DebugCommands = new DebugCommandProcessor();` | `public static readonly DebugCommandProcessor DebugCommands = new DebugCommandProcessor();` |
| 2932 | field | Terraria.UI.Chat.ChatManager | Terraria.UI.Chat/ChatManager.cs | D:\TRbackup\Version4\Terraria.UI.Chat\ChatManager.cs | 25 | 2 | Commands | Terraria.Chat.ChatCommandProcessor | `public static readonly ChatCommandProcessor Commands = new ChatCommandProcessor();` | `public static readonly ChatCommandProcessor Commands = new ChatCommandProcessor();` |
| 2933 | field | Terraria.UI.Chat.ChatManager | Terraria.UI.Chat/ChatManager.cs | D:\TRbackup\Version4\Terraria.UI.Chat\ChatManager.cs | 27 | 2 | _handlers | System.Collections.Concurrent.ConcurrentDictionary<string, Terraria.UI.Chat.ITagHandler> | `private static ConcurrentDictionary<string, ITagHandler> _handlers = new ConcurrentDictionary<string, ITagHandler>();` | `private static ConcurrentDictionary<string, ITagHandler> _handlers = new ConcurrentDictionary<string, ITagHandler>();` |
| 2934 | field | Terraria.UI.Chat.ChatManager | Terraria.UI.Chat/ChatManager.cs | D:\TRbackup\Version4\Terraria.UI.Chat\ChatManager.cs | 29 | 2 | ShadowDirections | Vector2[] | `public static readonly Vector2[] ShadowDirections = new Vector2[4]  	{  		-Vector2.UnitX,  		Vector2.UnitX,  		-Vector2.UnitY,  		Vector2.UnitY  	};` | `public static readonly Vector2[] ShadowDirections = new Vector2[4] { -Vector2.UnitX, Vector2.UnitX, -Vector2.UnitY, Vector2.UnitY };` |
| 2935 | field | Terraria.UI.Chat.ChatMessageContainer | Terraria.UI.Chat/ChatMessageContainer.cs | D:\TRbackup\Version4\Terraria.UI.Chat\ChatMessageContainer.cs | 9 | 2 | OriginalText | string | `public string OriginalText;` | `public string OriginalText;` |
| 2936 | field | Terraria.UI.Chat.ChatMessageContainer | Terraria.UI.Chat/ChatMessageContainer.cs | D:\TRbackup\Version4\Terraria.UI.Chat\ChatMessageContainer.cs | 11 | 2 | _prepared | bool | `private bool _prepared;` | `private bool _prepared;` |
| 2937 | field | Terraria.UI.Chat.ChatMessageContainer | Terraria.UI.Chat/ChatMessageContainer.cs | D:\TRbackup\Version4\Terraria.UI.Chat\ChatMessageContainer.cs | 15 | 2 | _widthLimitInPixels | int | `private int _widthLimitInPixels;` | `private int _widthLimitInPixels;` |
| 2938 | field | Terraria.UI.Chat.ChatMessageContainer | Terraria.UI.Chat/ChatMessageContainer.cs | D:\TRbackup\Version4\Terraria.UI.Chat\ChatMessageContainer.cs | 17 | 2 | _timeLeft | int | `private int _timeLeft;` | `private int _timeLeft;` |

#### 属性（0）

无该类型成员记录。


## 5. 追溯与验收

> 来源报告 SHA-256：`b944441d92d5103f218b766a3e354200794528a19e65a862e44c2b40a36f1196`
- 本分区细分组数：18；成员数：253；字段：238；属性：15。
- 预期不变量：本文件只出现完整细分子系统；不跨分区复制成员；所有成员的来源序号、原始声明、路径、行列和 C# 类型必须与来源报告一致。
- 验证方式：重新读取 20 个文件，检查文件数、细分组唯一性、来源序号恰好覆盖 `1..4542`、字段/属性合计和逐成员行文本一致；本次分区生成不运行 `dotnet`，因为没有修改 C# 或项目文件。

生成器：`Build/Tools/Generate-Version4NonAuthoritativeComponentPartitionReports.ps1`。
