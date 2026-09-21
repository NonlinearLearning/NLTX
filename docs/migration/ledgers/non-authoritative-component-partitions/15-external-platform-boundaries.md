# Version4 非权威组件拆分分区 15/20：外部平台与协议边界

> 来源报告：`docs/migration/ledgers/Version4非权威模拟系统字段属性逐成员源码声明-更细子系统拆分-去除ID类文件.md`
> 来源报告 SHA-256：`b944441d92d5103f218b766a3e354200794528a19e65a862e44c2b40a36f1196`
> 本文件是并行组件拆分工作包，不是新的权威成员清单；成员声明事实直接保留来源报告原文。

## 1. 分区定位与评估

- 分区范围：Social API、IPC、Workshop/join、加密、NAT 和资源包外部适配。
- 本分区组件化重点：外部第三方类型停留在 Adapter 边界，确认输入验证、错误、重试和取消。
- 本分区包含 7 个完整细分子系统、59 条成员记录（字段 51、属性 8）。来源序号覆盖区间 `5..4067`，区间可能与其他分区交错，不以序号定义领域边界。
- 组件边界判定：细分子系统只是并行处理的最小库存边界，不等价于一个组件。必须继续按共同读者/写者、变更原因、生命周期、权威性和副作用分为 Component、System、Query、Command、Adapter 或 Projection。
- 证据状态：成员声明和统计已由来源报告校验；逐成员读者、写者、初始化/清理路径、持久化/网络语义和系统顺序仍需本分区会话补证，不得从名称或数量推断。

## 2. 分区统计

| 父级子系统 | 细分子系统数 | 字段 | 属性 | 合计 |
|---|---:|---:|---:|---:|
| `ExternalBoundaries` | 3 | 29 | 4 | 33 |
| `ExternalDependencyOrGenerated` | 2 | 10 | 4 | 14 |
| `RuntimeComposition` | 1 | 2 | 0 | 2 |
| `SharedRuntimeMechanisms` | 1 | 10 | 0 | 10 |

| 原报告章节 | 父级 | 细分子系统 | 边界角色 | 字段 | 属性 | 合计 | 组件化处理 |
|---|---|---|---|---:|---:|---:|---|
| `4.1.2` | `RuntimeComposition` | `MainPlatformExecutionAdapter` | platform adapter | 2 | 0 | 2 | 待按成员访问模式拆分 |
| `4.8.1` | `ExternalBoundaries` | `SocialApiRegistry` | adapter | 6 | 1 | 7 | 待按成员访问模式拆分 |
| `4.8.2` | `ExternalBoundaries` | `SocialTransportIpc` | adapter | 8 | 1 | 9 | 待按成员访问模式拆分 |
| `4.8.3` | `ExternalBoundaries` | `WorkshopAndJoinBoundaryData` | adapter DTO | 15 | 2 | 17 | 待按成员访问模式拆分 |
| `4.9.58` | `SharedRuntimeMechanisms` | `SharedResourcePackAdapters` | adapter | 10 | 0 | 10 | 待按成员访问模式拆分 |
| `4.10.1` | `ExternalDependencyOrGenerated` | `CryptographicDependency` | external dependency | 10 | 0 | 10 | 待按成员访问模式拆分 |
| `4.10.2` | `ExternalDependencyOrGenerated` | `NatPortMappingInterop` | external adapter | 0 | 4 | 4 | 待按成员访问模式拆分 |

## 3. 并行组件拆分契约

对本文件中的每个细分子系统和每条成员记录执行以下判断，并把结果写入对应的实现阶段设计/审计记录：

- 盘点声明类型、可见性、读者、写者、创建/更新/清理/持久化生命周期、访问频率和副作用；缺证据标记 `missing`，不可用命名猜测补齐。
- 标记权威状态、派生值、缓存、快照、兼容字段和表现数据；Component 只保存一个内聚概念的权威数据。
- 需要行为转换时放入显式 System/Command；纯资格判断和派生计算放入 Query；外部协议、文件、平台和第三方类型放入 Adapter；网络/持久化/日志/UI 输出放入单向 Projection。
- 分别建模实体引用、持久化 ID、网络 ID 和外部 ID；不得把 ID 类文件或外部对象直接塞入共享组件。
- 记录 Interface、Implementation、Seam、Depth、Leverage、Locality，以及必要的系统先后约束、失败归属和重试策略。

## 4. 逐成员源码声明

以下细分子系统块保持来源报告的成员表、声明、路径、行列和来源序号；只调整 Markdown 标题层级并增加分区追踪元数据。

### 4.1 细分子系统：`MainPlatformExecutionAdapter`

- 原报告章节：`4.1.2`
- 父级子系统：`RuntimeComposition`
- 分区工作包：`15` / `外部平台与协议边界`

- 上一级基线细分子系统：`MainPlatformExecutionAdapter`
- 细分职责：平台线程保持常量和原生调用边界。
- 边界角色：`platform adapter`；最小 seam：native execution-state port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：2；属性：0；合计：2。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（2）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 5 | field | Terraria.Main.NativeMethods | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 108 | 3 | ES_CONTINUOUS | uint | `public const uint ES_CONTINUOUS = 2147483648u;` | `public const uint ES_CONTINUOUS = 2147483648u;` |
| 6 | field | Terraria.Main.NativeMethods | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 110 | 3 | ES_SYSTEM_REQUIRED | uint | `public const uint ES_SYSTEM_REQUIRED = 1u;` | `public const uint ES_SYSTEM_REQUIRED = 1u;` |

#### 属性（0）

无该类型成员记录。


### 4.2 细分子系统：`SocialApiRegistry`

- 原报告章节：`4.8.1`
- 父级子系统：`ExternalBoundaries`
- 分区工作包：`15` / `外部平台与协议边界`

- 上一级基线细分子系统：`SocialApiRegistry`
- 细分职责：社交提供程序注册和全局 API 状态。
- 边界角色：`adapter`；最小 seam：social provider port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：6；属性：1；合计：7。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（6）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1000 | field | Terraria.Social.SocialAPI | Terraria.Social/SocialAPI.cs | D:\TRbackup\Version4\Terraria.Social\SocialAPI.cs | 12 | 2 | _mode | Terraria.Social.SocialMode | `private static SocialMode _mode;` | `private static SocialMode _mode;` |
| 1001 | field | Terraria.Social.SocialAPI | Terraria.Social/SocialAPI.cs | D:\TRbackup\Version4\Terraria.Social\SocialAPI.cs | 14 | 2 | Achievements | Terraria.Social.Base.AchievementsSocialModule | `public static Terraria.Social.Base.AchievementsSocialModule Achievements;` | `public static Terraria.Social.Base.AchievementsSocialModule Achievements;` |
| 1002 | field | Terraria.Social.SocialAPI | Terraria.Social/SocialAPI.cs | D:\TRbackup\Version4\Terraria.Social\SocialAPI.cs | 16 | 2 | Cloud | Terraria.Social.Base.CloudSocialModule | `public static Terraria.Social.Base.CloudSocialModule Cloud;` | `public static Terraria.Social.Base.CloudSocialModule Cloud;` |
| 1003 | field | Terraria.Social.SocialAPI | Terraria.Social/SocialAPI.cs | D:\TRbackup\Version4\Terraria.Social\SocialAPI.cs | 18 | 2 | Network | Terraria.Social.Base.NetSocialModule | `public static Terraria.Social.Base.NetSocialModule Network;` | `public static Terraria.Social.Base.NetSocialModule Network;` |
| 1004 | field | Terraria.Social.SocialAPI | Terraria.Social/SocialAPI.cs | D:\TRbackup\Version4\Terraria.Social\SocialAPI.cs | 20 | 2 | JoinRequests | Terraria.Social.Base.ServerJoinRequestsManager | `public static ServerJoinRequestsManager JoinRequests;` | `public static ServerJoinRequestsManager JoinRequests;` |
| 1005 | field | Terraria.Social.SocialAPI | Terraria.Social/SocialAPI.cs | D:\TRbackup\Version4\Terraria.Social\SocialAPI.cs | 22 | 2 | _modules | System.Collections.Generic.List<Terraria.Social.ISocialModule> | `private static List<ISocialModule> _modules;` | `private static List<ISocialModule> _modules;` |

#### 属性（1）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1009 | property | Terraria.Social.SocialAPI | Terraria.Social/SocialAPI.cs | D:\TRbackup\Version4\Terraria.Social\SocialAPI.cs | 24 | 2 | Mode | Terraria.Social.SocialMode | `public static SocialMode Mode => _mode;` | `public static SocialMode Mode => _mode;` |


### 4.3 细分子系统：`SocialTransportIpc`

- 原报告章节：`4.8.2`
- 父级子系统：`ExternalBoundaries`
- 分区工作包：`15` / `外部平台与协议边界`

- 上一级基线细分子系统：`SocialTransportIpc`
- 细分职责：WeGame IPC 传输边界。
- 边界角色：`adapter`；最小 seam：IPC transport port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：8；属性：1；合计：9。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（8）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 992 | field | Terraria.Social.WeGame.IPCBase | Terraria.Social.WeGame/IPCBase.cs | D:\TRbackup\Version4\Terraria.Social.WeGame\IPCBase.cs | 13 | 2 | _producer | System.Collections.Generic.List<System.Collections.Generic.List<byte>> | `private List<List<byte>> _producer = new List<List<byte>>();` | `private List<List<byte>> _producer = new List<List<byte>>();` |
| 993 | field | Terraria.Social.WeGame.IPCBase | Terraria.Social.WeGame/IPCBase.cs | D:\TRbackup\Version4\Terraria.Social.WeGame\IPCBase.cs | 15 | 2 | _consumer | System.Collections.Generic.List<System.Collections.Generic.List<byte>> | `private List<List<byte>> _consumer = new List<List<byte>>();` | `private List<List<byte>> _consumer = new List<List<byte>>();` |
| 994 | field | Terraria.Social.WeGame.IPCBase | Terraria.Social.WeGame/IPCBase.cs | D:\TRbackup\Version4\Terraria.Social.WeGame\IPCBase.cs | 17 | 2 | _totalData | System.Collections.Generic.List<byte> | `private List<byte> _totalData = new List<byte>();` | `private List<byte> _totalData = new List<byte>();` |
| 995 | field | Terraria.Social.WeGame.IPCBase | Terraria.Social.WeGame/IPCBase.cs | D:\TRbackup\Version4\Terraria.Social.WeGame\IPCBase.cs | 19 | 2 | _listLock | object | `private object _listLock = new object();` | `private object _listLock = new object();` |
| 996 | field | Terraria.Social.WeGame.IPCBase | Terraria.Social.WeGame/IPCBase.cs | D:\TRbackup\Version4\Terraria.Social.WeGame\IPCBase.cs | 21 | 2 | _pipeBrokenFlag | bool | `protected volatile bool _pipeBrokenFlag;` | `protected volatile bool _pipeBrokenFlag;` |
| 997 | field | Terraria.Social.WeGame.IPCBase | Terraria.Social.WeGame/IPCBase.cs | D:\TRbackup\Version4\Terraria.Social.WeGame\IPCBase.cs | 23 | 2 | _pipeStream | System.IO.Pipes.PipeStream | `protected PipeStream _pipeStream;` | `protected PipeStream _pipeStream;` |
| 998 | field | Terraria.Social.WeGame.IPCBase | Terraria.Social.WeGame/IPCBase.cs | D:\TRbackup\Version4\Terraria.Social.WeGame\IPCBase.cs | 25 | 2 | _cancelTokenSrc | System.Threading.CancellationTokenSource | `protected CancellationTokenSource _cancelTokenSrc;` | `protected CancellationTokenSource _cancelTokenSrc;` |
| 999 | field | Terraria.Social.WeGame.IPCBase | Terraria.Social.WeGame/IPCBase.cs | D:\TRbackup\Version4\Terraria.Social.WeGame\IPCBase.cs | 27 | 2 | _onDataArrive | System.Action<byte[]> | `protected Action<byte[]> _onDataArrive;` | `protected Action<byte[]> _onDataArrive;` |

#### 属性（1）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1008 | property | Terraria.Social.WeGame.IPCBase | Terraria.Social.WeGame/IPCBase.cs | D:\TRbackup\Version4\Terraria.Social.WeGame\IPCBase.cs | 29 | 2 | BufferSize | int | `public int BufferSize { get; set; }` | `public int BufferSize { get; set; }` |


### 4.4 细分子系统：`WorkshopAndJoinBoundaryData`

- 原报告章节：`4.8.3`
- 父级子系统：`ExternalBoundaries`
- 分区工作包：`15` / `外部平台与协议边界`

- 上一级基线细分子系统：`WorkshopAndJoinBoundaryData`
- 细分职责：创意工坊、联机请求和富状态边界数据。
- 边界角色：`adapter DTO`；最小 seam：workshop/join adapter；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：8；声明类型数：8；字段：15；属性：2；合计：17。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（15）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 977 | field | Terraria.Social.Base.CloudSocialModule | Terraria.Social.Base/CloudSocialModule.cs | D:\TRbackup\Version4\Terraria.Social.Base\CloudSocialModule.cs | 8 | 2 | EnabledByDefault | bool | `public bool EnabledByDefault;` | `public bool EnabledByDefault;` |
| 978 | field | Terraria.Social.Base.FoundWorkshopEntryInfo | Terraria.Social.Base/FoundWorkshopEntryInfo.cs | D:\TRbackup\Version4\Terraria.Social.Base\FoundWorkshopEntryInfo.cs | 5 | 2 | workshopEntryId | ulong | `public ulong workshopEntryId;` | `public ulong workshopEntryId;` |
| 979 | field | Terraria.Social.Base.FoundWorkshopEntryInfo | Terraria.Social.Base/FoundWorkshopEntryInfo.cs | D:\TRbackup\Version4\Terraria.Social.Base\FoundWorkshopEntryInfo.cs | 7 | 2 | publicity | Terraria.Social.Base.WorkshopItemPublicSettingId | `public WorkshopItemPublicSettingId publicity;` | `public WorkshopItemPublicSettingId publicity;` |
| 980 | field | Terraria.Social.Base.FoundWorkshopEntryInfo | Terraria.Social.Base/FoundWorkshopEntryInfo.cs | D:\TRbackup\Version4\Terraria.Social.Base\FoundWorkshopEntryInfo.cs | 9 | 2 | tags | string[] | `public string[] tags;` | `public string[] tags;` |
| 981 | field | Terraria.Social.Base.FoundWorkshopEntryInfo | Terraria.Social.Base/FoundWorkshopEntryInfo.cs | D:\TRbackup\Version4\Terraria.Social.Base\FoundWorkshopEntryInfo.cs | 11 | 2 | previewImagePath | string | `public string previewImagePath;` | `public string previewImagePath;` |
| 982 | field | Terraria.Social.Base.FoundWorkshopEntryInfo | Terraria.Social.Base/FoundWorkshopEntryInfo.cs | D:\TRbackup\Version4\Terraria.Social.Base\FoundWorkshopEntryInfo.cs | 13 | 2 | publishedVersion | int | `public int publishedVersion;` | `public int publishedVersion;` |
| 983 | field | Terraria.Social.Base.RichPresenceState | Terraria.Social.Base/RichPresenceState.cs | D:\TRbackup\Version4\Terraria.Social.Base\RichPresenceState.cs | 17 | 2 | GameMode | Terraria.Social.Base.RichPresenceState.GameModeState | `public GameModeState GameMode;` | `public GameModeState GameMode;` |
| 984 | field | Terraria.Social.Base.ServerJoinRequestsManager | Terraria.Social.Base/ServerJoinRequestsManager.cs | D:\TRbackup\Version4\Terraria.Social.Base\ServerJoinRequestsManager.cs | 8 | 2 | _requests | System.Collections.Generic.List<Terraria.Social.Base.UserJoinToServerRequest> | `private readonly List<UserJoinToServerRequest> _requests;` | `private readonly List<UserJoinToServerRequest> _requests;` |
| 985 | field | Terraria.Social.Base.ServerJoinRequestsManager | Terraria.Social.Base/ServerJoinRequestsManager.cs | D:\TRbackup\Version4\Terraria.Social.Base\ServerJoinRequestsManager.cs | 10 | 2 | CurrentRequests | System.Collections.ObjectModel.ReadOnlyCollection<Terraria.Social.Base.UserJoinToServerRequest> | `public readonly ReadOnlyCollection<UserJoinToServerRequest> CurrentRequests;` | `public readonly ReadOnlyCollection<UserJoinToServerRequest> CurrentRequests;` |
| 986 | field | Terraria.Social.Base.WorkshopIssueReporter | Terraria.Social.Base/WorkshopIssueReporter.cs | D:\TRbackup\Version4\Terraria.Social.Base\WorkshopIssueReporter.cs | 11 | 2 | _reports | System.Collections.Generic.List<Terraria.DataStructures.IssueReport> | `private List<IssueReport> _reports = new List<IssueReport>();` | `private List<IssueReport> _reports = new List<IssueReport>();` |
| 987 | field | Terraria.Social.Base.WorkshopItemPublishSettings | Terraria.Social.Base/WorkshopItemPublishSettings.cs | D:\TRbackup\Version4\Terraria.Social.Base\WorkshopItemPublishSettings.cs | 7 | 2 | UsedTags | Terraria.Social.Base.WorkshopTagOption[] | `public WorkshopTagOption[] UsedTags = new WorkshopTagOption[0];` | `public WorkshopTagOption[] UsedTags = new WorkshopTagOption[0];` |
| 988 | field | Terraria.Social.Base.WorkshopItemPublishSettings | Terraria.Social.Base/WorkshopItemPublishSettings.cs | D:\TRbackup\Version4\Terraria.Social.Base\WorkshopItemPublishSettings.cs | 9 | 2 | Publicity | Terraria.Social.Base.WorkshopItemPublicSettingId | `public WorkshopItemPublicSettingId Publicity;` | `public WorkshopItemPublicSettingId Publicity;` |
| 989 | field | Terraria.Social.Base.WorkshopItemPublishSettings | Terraria.Social.Base/WorkshopItemPublishSettings.cs | D:\TRbackup\Version4\Terraria.Social.Base\WorkshopItemPublishSettings.cs | 11 | 2 | PreviewImagePath | string | `public string PreviewImagePath;` | `public string PreviewImagePath;` |
| 990 | field | Terraria.Social.Base.WorkshopTagOption | Terraria.Social.Base/WorkshopTagOption.cs | D:\TRbackup\Version4\Terraria.Social.Base\WorkshopTagOption.cs | 5 | 2 | NameKey | string | `public readonly string NameKey;` | `public readonly string NameKey;` |
| 991 | field | Terraria.Social.Base.WorkshopTagOption | Terraria.Social.Base/WorkshopTagOption.cs | D:\TRbackup\Version4\Terraria.Social.Base\WorkshopTagOption.cs | 7 | 2 | InternalNameForAPIs | string | `public readonly string InternalNameForAPIs;` | `public readonly string InternalNameForAPIs;` |

#### 属性（2）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1006 | property | Terraria.Social.Base.UserJoinToServerRequest | Terraria.Social.Base/UserJoinToServerRequest.cs | D:\TRbackup\Version4\Terraria.Social.Base\UserJoinToServerRequest.cs | 7 | 2 | UserDisplayName | string | `internal string UserDisplayName { get; private set; }` | `internal string UserDisplayName { get; private set; }` |
| 1007 | property | Terraria.Social.Base.UserJoinToServerRequest | Terraria.Social.Base/UserJoinToServerRequest.cs | D:\TRbackup\Version4\Terraria.Social.Base\UserJoinToServerRequest.cs | 9 | 2 | UserFullIdentifier | string | `internal string UserFullIdentifier { get; private set; }` | `internal string UserFullIdentifier { get; private set; }` |


### 4.5 细分子系统：`SharedResourcePackAdapters`

- 原报告章节：`4.9.58`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`15` / `外部平台与协议边界`

- 上一级基线细分子系统：`SharedResourcePackAdapters`
- 细分职责：资源包及资源包列表适配。
- 边界角色：`adapter`；最小 seam：resource pack port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：2；声明类型数：2；字段：10；属性：0；合计：10。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（10）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2753 | field | Terraria.IO.ResourcePack | Terraria.IO/ResourcePack.cs | D:\TRbackup\Version4\Terraria.IO\ResourcePack.cs | 21 | 2 | FullPath | string | `public readonly string FullPath;` | `public readonly string FullPath;` |
| 2754 | field | Terraria.IO.ResourcePack | Terraria.IO/ResourcePack.cs | D:\TRbackup\Version4\Terraria.IO\ResourcePack.cs | 23 | 2 | FileName | string | `public readonly string FileName;` | `public readonly string FileName;` |
| 2755 | field | Terraria.IO.ResourcePack | Terraria.IO/ResourcePack.cs | D:\TRbackup\Version4\Terraria.IO\ResourcePack.cs | 25 | 2 | _services | System.IServiceProvider | `private readonly IServiceProvider _services;` | `private readonly IServiceProvider _services;` |
| 2756 | field | Terraria.IO.ResourcePack | Terraria.IO/ResourcePack.cs | D:\TRbackup\Version4\Terraria.IO\ResourcePack.cs | 27 | 2 | IsCompressed | bool | `public readonly bool IsCompressed;` | `public readonly bool IsCompressed;` |
| 2757 | field | Terraria.IO.ResourcePack | Terraria.IO/ResourcePack.cs | D:\TRbackup\Version4\Terraria.IO\ResourcePack.cs | 29 | 2 | Branding | Terraria.IO.ResourcePack.BrandingType | `public readonly BrandingType Branding;` | `public readonly BrandingType Branding;` |
| 2758 | field | Terraria.IO.ResourcePack | Terraria.IO/ResourcePack.cs | D:\TRbackup\Version4\Terraria.IO\ResourcePack.cs | 31 | 2 | _zipFile | ZipFile | `private readonly ZipFile _zipFile;` | `private readonly ZipFile _zipFile;` |
| 2759 | field | Terraria.IO.ResourcePack | Terraria.IO/ResourcePack.cs | D:\TRbackup\Version4\Terraria.IO\ResourcePack.cs | 33 | 2 | _icon | Texture2D | `private Texture2D _icon;` | `private Texture2D _icon;` |
| 2760 | field | Terraria.IO.ResourcePack | Terraria.IO/ResourcePack.cs | D:\TRbackup\Version4\Terraria.IO\ResourcePack.cs | 35 | 2 | ICON_FILE_NAME | string | `private const string ICON_FILE_NAME = "icon.png";` | `private const string ICON_FILE_NAME = "icon.png";` |
| 2761 | field | Terraria.IO.ResourcePack | Terraria.IO/ResourcePack.cs | D:\TRbackup\Version4\Terraria.IO\ResourcePack.cs | 37 | 2 | PACK_FILE_NAME | string | `private const string PACK_FILE_NAME = "pack.json";` | `private const string PACK_FILE_NAME = "pack.json";` |
| 2762 | field | Terraria.IO.ResourcePackList | Terraria.IO/ResourcePackList.cs | D:\TRbackup\Version4\Terraria.IO\ResourcePackList.cs | 15 | 2 | _resourcePacks | System.Collections.Generic.List<Terraria.IO.ResourcePack> | `private readonly List<ResourcePack> _resourcePacks = new List<ResourcePack>();` | `private readonly List<ResourcePack> _resourcePacks = new List<ResourcePack>();` |

#### 属性（0）

无该类型成员记录。


### 4.6 细分子系统：`CryptographicDependency`

- 原报告章节：`4.10.1`
- 父级子系统：`ExternalDependencyOrGenerated`
- 分区工作包：`15` / `外部平台与协议边界`

- 上一级基线细分子系统：`CryptographicDependency`
- 细分职责：BCrypt 第三方实现字段。
- 边界角色：`external dependency`；最小 seam：crypto adapter；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：10；属性：0；合计：10。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（10）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 4054 | field | BCrypt.Net.BCrypt | BCrypt.Net/BCrypt.cs | D:\TRbackup\Version4\BCrypt.Net\BCrypt.cs | 8 | 2 | DefaultRounds | int | `private const int DefaultRounds = 11;` | `private const int DefaultRounds = 11;` |
| 4055 | field | BCrypt.Net.BCrypt | BCrypt.Net/BCrypt.cs | D:\TRbackup\Version4\BCrypt.Net\BCrypt.cs | 10 | 2 | BCryptSaltLen | int | `private const int BCryptSaltLen = 16;` | `private const int BCryptSaltLen = 16;` |
| 4056 | field | BCrypt.Net.BCrypt | BCrypt.Net/BCrypt.cs | D:\TRbackup\Version4\BCrypt.Net\BCrypt.cs | 12 | 2 | SafeUTF8 | System.Text.Encoding | `private static readonly Encoding SafeUTF8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);` | `private static readonly Encoding SafeUTF8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);` |
| 4057 | field | BCrypt.Net.BCrypt | BCrypt.Net/BCrypt.cs | D:\TRbackup\Version4\BCrypt.Net\BCrypt.cs | 14 | 2 | BlowfishNumRounds | int | `private const int BlowfishNumRounds = 16;` | `private const int BlowfishNumRounds = 16;` |
| 4058 | field | BCrypt.Net.BCrypt | BCrypt.Net/BCrypt.cs | D:\TRbackup\Version4\BCrypt.Net\BCrypt.cs | 16 | 2 | Index64 | int[] | `private static readonly int[] Index64 = new int[128]  	{  		-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,  		-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,  		-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,  		-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,  		-1, -1, -1, -1, -1, -1, 0, 1, 54, 55,  		56, 57, 58, 59, 60, 61, 62, 63, -1, -1,  		-1, -1, -1, -1, -1, 2, 3, 4, 5, 6,  		7, 8, 9, 10, 11, 12, 13, 14, 15, 16,  		17, 18, 19, 20, 21, 22, 23, 24, 25, 26,  		27, -1, -1, -1, -1, -1, -1, 28, 29, 30,  		31, 32, 33, 34, 35, 36, 37, 38, 39, 40,  		41, 42, 43, 44, 45, 46, 47, 48, 49, 50,  		51, 52, 53, -1, -1, -1, -1, -1  	};` | `private static readonly int[] Index64 = new int[128] { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 0, 1, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, -1, -1, -1, -1, -1, -1, -1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, -1, -1, -1, -1, -1, -1, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, -1, -1, -1, -1, -1 };` |
| 4059 | field | BCrypt.Net.BCrypt | BCrypt.Net/BCrypt.cs | D:\TRbackup\Version4\BCrypt.Net\BCrypt.cs | 33 | 2 | EmptyString | string | `private const string EmptyString = "";` | `private const string EmptyString = "";` |
| 4060 | field | BCrypt.Net.BCrypt | BCrypt.Net/BCrypt.cs | D:\TRbackup\Version4\BCrypt.Net\BCrypt.cs | 35 | 2 | DefaultHashVersion | char | `private const char DefaultHashVersion = 'a';` | `private const char DefaultHashVersion = 'a';` |
| 4061 | field | BCrypt.Net.BCrypt | BCrypt.Net/BCrypt.cs | D:\TRbackup\Version4\BCrypt.Net\BCrypt.cs | 37 | 2 | Nul | string | `private const string Nul = "\0";` | `private const string Nul = "\0";` |
| 4062 | field | BCrypt.Net.BCrypt | BCrypt.Net/BCrypt.cs | D:\TRbackup\Version4\BCrypt.Net\BCrypt.cs | 39 | 2 | MinRounds | short | `private const short MinRounds = 4;` | `private const short MinRounds = 4;` |
| 4063 | field | BCrypt.Net.BCrypt | BCrypt.Net/BCrypt.cs | D:\TRbackup\Version4\BCrypt.Net\BCrypt.cs | 41 | 2 | MaxRounds | short | `private const short MaxRounds = 31;` | `private const short MaxRounds = 31;` |

#### 属性（0）

无该类型成员记录。


### 4.7 细分子系统：`NatPortMappingInterop`

- 原报告章节：`4.10.2`
- 父级子系统：`ExternalDependencyOrGenerated`
- 分区工作包：`15` / `外部平台与协议边界`

- 上一级基线细分子系统：`NatPortMappingInterop`
- 细分职责：NAT/UPnP 端口映射互操作属性。
- 边界角色：`external adapter`；最小 seam：UPnP COM adapter；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：2；声明类型数：2；字段：0；属性：4；合计：4。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（0）

无该类型成员记录。

#### 属性（4）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 4064 | property | NATUPNPLib.IStaticPortMapping | NATUPNPLib/IStaticPortMapping.cs | D:\TRbackup\Version4\NATUPNPLib\IStaticPortMapping.cs | 14 | 2 | InternalPort | int | `[DispId(3)] int InternalPort { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)] [DispId(3)] get; }` | `[DispId(3)] int InternalPort { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)] [DispId(3)] get; }` |
| 4065 | property | NATUPNPLib.IStaticPortMapping | NATUPNPLib/IStaticPortMapping.cs | D:\TRbackup\Version4\NATUPNPLib\IStaticPortMapping.cs | 22 | 2 | Protocol | string | `[DispId(4)] string Protocol { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)] [DispId(4)] [return: MarshalAs(UnmanagedType.BStr)] get; }` | `[DispId(4)] string Protocol { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)] [DispId(4)] [return: MarshalAs(UnmanagedType.BStr)] get; }` |
| 4066 | property | NATUPNPLib.IStaticPortMapping | NATUPNPLib/IStaticPortMapping.cs | D:\TRbackup\Version4\NATUPNPLib\IStaticPortMapping.cs | 31 | 2 | InternalClient | string | `[DispId(5)] string InternalClient { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)] [DispId(5)] [return: MarshalAs(UnmanagedType.BStr)] get; }` | `[DispId(5)] string InternalClient { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)] [DispId(5)] [return: MarshalAs(UnmanagedType.BStr)] get; }` |
| 4067 | property | NATUPNPLib.IUPnPNAT | NATUPNPLib/IUPnPNAT.cs | D:\TRbackup\Version4\NATUPNPLib\IUPnPNAT.cs | 12 | 2 | StaticPortMappingCollection | NATUPNPLib.IStaticPortMappingCollection | `[DispId(1)] IStaticPortMappingCollection StaticPortMappingCollection { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)] [DispId(1)] [return: MarshalAs(UnmanagedType.Interface)] get; }` | `[DispId(1)] IStaticPortMappingCollection StaticPortMappingCollection { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)] [DispId(1)] [return: MarshalAs(UnmanagedType.Interface)] get; }` |


## 5. 追溯与验收

> 来源报告 SHA-256：`b944441d92d5103f218b766a3e354200794528a19e65a862e44c2b40a36f1196`
- 本分区细分组数：7；成员数：59；字段：51；属性：8。
- 预期不变量：本文件只出现完整细分子系统；不跨分区复制成员；所有成员的来源序号、原始声明、路径、行列和 C# 类型必须与来源报告一致。
- 验证方式：重新读取 20 个文件，检查文件数、细分组唯一性、来源序号恰好覆盖 `1..4542`、字段/属性合计和逐成员行文本一致；本次分区生成不运行 `dotnet`，因为没有修改 C# 或项目文件。

生成器：`Build/Tools/Generate-Version4NonAuthoritativeComponentPartitionReports.ps1`。
