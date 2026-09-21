# P15 外部平台与协议边界：proposed 组件设计

partitionId: P15
sessionId: 24f54074853c473dacbcc4fa1438e4dd
inputReport: D:\TRbackup\NLTX\docs\migration\ledgers\non-authoritative-component-partitions\15-external-platform-boundaries.md
componentDesignPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\non-authoritative\2026-09-11-version4-non-authoritative-P15-external-platform-boundaries-component-design.md
componentExecutionPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\non-authoritative\2026-09-11-version4-non-authoritative-P15-external-platform-boundaries-component-execution.md
sessionRebind: explicit user-requested takeover from document session 149b070c9cf340c4b08c69797c32ce80; prior runner session 36c49a78c06d42b4aa2ccd18414094b0 was failed; current runner session 24f54074853c473dacbcc4fa1438e4dd
designStatus: proposed
executionStatus: completed
implementationStatus: completed
verificationStatus: passed (focused src2 verifier re-run under current session)
completedComponents:
- reverified carried-forward MainPlatformExecutionBoundaryAdapter (source leaf: MainPlatformExecutionAdapter; focused verifier passed)
- reverified carried-forward SocialProviderRegistryBoundary (source leaf: SocialApiRegistry; focused verifier passed)
- reverified carried-forward WeGameIpcTransportBoundary (source leaf: SocialTransportIpc; focused verifier passed)
- reverified carried-forward WorkshopJoinBoundary (source leaf: WorkshopAndJoinBoundaryData; focused verifier passed)
- reverified carried-forward ResourcePackBoundaryAdapter (source leaf: SharedResourcePackAdapters; focused verifier passed)
- reverified carried-forward CryptoBoundaryAdapter (source leaf: CryptographicDependency; focused verifier passed through injected primitive)
- reverified carried-forward NatPortMappingBoundaryAdapter (source leaf: NatPortMappingInterop; focused verifier passed)
currentComponent: completed; evidence and integration review remain
pendingComponents: none (focused source verifier passed; broader host/provider/compatibility evidence remains open)
lastCheckpointUtc: 2026-09-12T07:29:40.951Z
evidence-gap: All proposed P15 boundary source was already present under src2 before this session and the focused build/verifier was re-run successfully. No P15 Component type is currently specified or present; the proposed Component candidates remain integration-review deferred. Native Windows return semantics, real provider registration, host integration, asynchronous pipe behavior, Workshop response ownership, resource-pack disposal under real asset services, NAT COM cleanup, and BCrypt compatibility vectors remain unproven. Version4 and complete-reference bodies are partial/version-drifted evidence and do not establish behavior equivalence.
blocking-decision: crossSubsystemOwner: integration-review is required for runtime/session entity, provider registry lifetime, JoinRequest acceptance owner, Workshop/resource-pack persistence and UI contracts, IPC scheduling, NAT mapping ownership and cleanup, and the exact secret-derivation compatibility policy.

> 本文件仍是第二轮非权威组件设计草案；架构边界、跨分区 owner 和最终接入仍为 `status: proposed`。本实现会话已将候选端口、System、Query、Adapter、Projection 和 verifier 源码保存到 `src2`，但不声明 NLTX 生产能力、迁移完成、行为等价、API 闭合或最终跨分区 owner。

## 1. 当前检查点

本次已完成 `MainPlatformExecutionAdapter` 的成员检查和边界设计。该叶子只有两个常量，它们不是实体状态，也不是 ECS Component 字段：它们属于 proposed Windows 平台适配器的 P/Invoke 配置。Version4 事实是 `Terraria.Main.NativeMethods` 在 `Main.cs:106-113` 声明 `ES_CONTINUOUS`、`ES_SYSTEM_REQUIRED` 和 `SetThreadExecutionState`，`Main.NeverSleep` 在 `Main.cs:2172-2180` 按 Windows 条件调用，`Main.YouCanSleepNow` 在 `Main.cs:2182-2190` 恢复之前返回的执行状态；`Main.DedServ` 在 `Main.cs:2192-2196` 启用保持唤醒，服务结束和世界加载失败路径在 `Main.cs:2710`、`Main.cs:2787` 释放。

提出的最小边界如下：

- `proposed IPlatformExecutionStatePort`：隔离 Windows API；输入为显式的 keep-awake/release 命令，输出为可诊断的旧状态和失败结果。
- `proposed MainPlatformExecutionBoundaryAdapter`：唯一持有 native 调用和 previous execution state；不注册为 ECS Component，不向模拟实体暴露 `uint` 平台标志。
- `proposed PlatformExecutionLifecycleSystem`：读取主机生命周期事件，向 Adapter 提交命令；不直接调用 `kernel32.dll`。
- 不提出 `PlatformExecutionComponent`。该状态是宿主/线程资源，不是稳定实体领域状态；若整合会话要求持久化或调试快照，另行提出 `PlatformExecutionStatusProjection`，不反写模拟。

## 2. 范围与排除

### 2.1 本分区范围

本分区仅覆盖以下 7 个输入叶子和 59 条成员：`MainPlatformExecutionAdapter`、`SocialApiRegistry`、`SocialTransportIpc`、`WorkshopAndJoinBoundaryData`、`SharedResourcePackAdapters`、`CryptographicDependency`、`NatPortMappingInterop`。后续检查点将逐组补齐其成员映射。

### 2.2 排除范围

不在本分区内重新设计世界、玩家、物品、网络协议主体、存档格式、UI 页面、内容定义或通用实体 ID。`SocialAPI.Network` 与网络会话、`Cloud` 与持久化、Workshop 与 UI/内容目录、NAT 与 Netplay 的共享 owner 和调度顺序只提出候选，并统一标记 `crossSubsystemOwner: integration-review`。

## 3. Version4 证据基线

### 3.1 本检查点实际读取的主证据

| source | version | query/evidence | status |
|---|---|---|---|
| `D:\TRbackup\Version4\Terraria\Main.cs:106-113` | Version4 checkout | `Terraria.Main.NativeMethods`: `ES_CONTINUOUS`, `ES_SYSTEM_REQUIRED`, `SetThreadExecutionState` | confirmed |
| `D:\TRbackup\Version4\Terraria\Main.cs:2172-2190` | Version4 checkout | `NeverSleep`/`YouCanSleepNow` 的 Windows 条件、previous execution state 读写 | confirmed |
| `D:\TRbackup\Version4\Terraria\Main.cs:2192-2196`, `2710`, `2782-2788` | Version4 checkout | dedicated server 的启用、错误返回和退出释放路径 | partial: 全局退出调用图仍需整合验证 |
| `D:\TRbackup\Version4\Terraria\Program.cs:204-215` | Version4 checkout | 宿主先调用 `SocialAPI.Initialize`，再进入 `Main` 生命周期 | confirmed |
| `C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Server\DeviceNetwork\Systems\DeviceNetworkSystem.cs` | SS14 reference | `Initialize`、`Update`、`ComponentShutdown` 将资源/队列副作用集中在 System；只作结构参考 | structural-reference |

Version4 是行为事实来源。完整参考、tModLoader 和 SS14 不改变上述平台调用事实。

### 3.2 补证来源和限制

- 同路径完整参考 `D:\TRbackup\无任何删减通过编译\Terraria\Main.cs` 已确认该文件存在，但本检查点未发现比 Version4 更具体的 `Main.NativeMethods` 生命周期证据；不把完整参考存在写成 NLTX 能力。
- `D:\TRbackup\tmodloader-api-docs-stable\index.html` 页眉显示 `tModLoader v2026.07`。本检查点不使用其公开 API 替代 `kernel32.dll` 私有实现；平台执行状态没有与本边界等价的公开 Terraria API 证据。
- SS14 未发现与 Terraria `SetThreadExecutionState` 直接对应的结构；上表的 SS14 证据只说明副作用集中方式，不能推断 Terraria 行为。

## 4. 当前 NLTX 状态（截至本检查点）

`src/` 中未找到 `SocialAPI`、`IPCBase`、`ResourcePack`、`IUPnPNAT`、`BCrypt`、`SetThreadExecutionState` 或对应外部平台适配实现。当前仓库已有组件按领域目录组织，但没有本分区的生产实现。`dome` 中的历史引用仅属于参考/遗留材料，不是当前实现：例如 `dome\dome1\src2\Terraria.WorldFile.V319\LegacyReference\WorldFile.cs:200` 使用 `SocialAPI.Cloud`，`dome\dome1\docs\worldgen\worldgen-parity-report.md:446` 将 BCrypt 变换标为 deferred。当前检查点没有修改生产代码、测试或项目文件。

## 5. 第一组成员归属：MainPlatformExecutionAdapter（2/59）

所有目标类型和路径均为 `status: proposed`。

| source member | Version4 declaration | state kind | proposed role | readers/writers/lifecycle | evidence |
|---|---|---|---|---|---|
| `5 ES_CONTINUOUS` | `D:\TRbackup\Version4\Terraria\Main.cs:108` `public const uint` | adapter constant / compatibility | `proposed MainPlatformExecutionBoundaryAdapter` 内部常量；不进 Component | 读者：`proposed IPlatformExecutionStatePort` 实现；写者：无；生命周期：Adapter 初始化时可用，进程结束清理 | confirmed declaration, lifecycle partial |
| `6 ES_SYSTEM_REQUIRED` | `D:\TRbackup\Version4\Terraria\Main.cs:110` `public const uint` | adapter constant / compatibility | 同上；与 `ES_CONTINUOUS` 作为一次 keep-awake command 的内部配置 | 读者：proposed Adapter；写者：无；生命周期：宿主进程级 | confirmed declaration, lifecycle partial |

## 6. proposed 设计（本检查点）

### 6.1 依赖方向

```text
proposed PlatformExecutionLifecycleSystem
    -> proposed IPlatformExecutionStatePort
        -> proposed MainPlatformExecutionBoundaryAdapter
            -> kernel32.dll SetThreadExecutionState
```

核心模拟只产生宿主生命周期意图；Adapter 执行 I/O。`Query` 不读取操作系统状态，`Projection` 只能输出最近一次结果和诊断，不成为权威模拟状态。

### 6.2 不变量和失败边界

- `NeverSleep` 成功返回的旧状态只能由同一个 Adapter 实例保存和恢复；不能由 ECS 实体字段拼接。
- 非 Windows 平台必须得到明确的 no-op/unsupported 结果；不能把平台不可用伪装成模拟成功。
- `YouCanSleepNow` 在没有有效 previous state 时不调用恢复；重复 release 必须幂等或返回可识别的 already-released 结果。
- P/Invoke 异常、返回值为零和进程退出均归 Adapter 处理；日志/诊断不能推进游戏状态。

### 6.3 接口契约（全部 proposed）

```text
proposed IPlatformExecutionStatePort
  RequestKeepAwake() -> proposed PlatformExecutionLeaseResult
  ReleaseKeepAwake() -> proposed PlatformExecutionLeaseResult

proposed PlatformExecutionLifecycleSystem
  input: host-start, dedicated-server-start, world-load-failure, server-shutdown
  output: commands to IPlatformExecutionStatePort; diagnostic projection only
  side effects: none directly; Adapter owns OS call and error classification
```

Interface/Implementation/Seam/Depth/Leverage/Locality：Interface 是显式端口；Implementation 是 proposed Windows Adapter；Seam 是可替换的 OS call delegate；Depth 为一层外部边界；Leverage 为把平台 I/O 与宿主生命周期隔离；Locality 为 `RuntimeComposition/Platform`，不进入通用 `Shared/Components`。

## 7. 后续检查点草案

以下名称、路径和顺序均为 proposed，尚未完成成员闭合：

| source leaf | proposed boundary | expected ownership |
|---|---|---|
| `SocialApiRegistry` | `proposed SocialProviderRegistryAdapter` + `proposed ExternalPlatformSessionComponent` | provider refs remain Adapter-owned; session mode/availability may be component state; `crossSubsystemOwner: integration-review` |
| `SocialTransportIpc` | `proposed WeGameIpcTransportAdapter` + `proposed IpcTransportLifecycleSystem` | pipe/buffer/cancellation/callback remain Adapter-owned; decoded payload crosses a command/event port |
| `WorkshopAndJoinBoundaryData` | `proposed WorkshopBoundaryAdapter`, `proposed JoinRequestInboxComponent`, `proposed WorkshopProjection` | external IDs, paths, reports and request DTOs do not enter core as third-party objects |
| `SharedResourcePackAdapters` | `proposed ResourcePackAdapter`, `proposed ResourcePackSelectionComponent`, `proposed ResourcePackProjection` | file/zip/texture/service handles remain Adapter-owned; selection persistence requires integration review |
| `CryptographicDependency` | `proposed SecretDerivationAdapter` | constants and algorithm work buffers stay dependency-internal; no Component |
| `NatPortMappingInterop` | `proposed NatPortMappingAdapter`, `proposed NatPortMappingStatusComponent` | COM handles stay Adapter-owned; status snapshot is non-authoritative and network owner requires integration review |

## 8. SocialApiRegistry 检查点（7/59）

Version4 的 `SocialAPI` 是进程级静态注册表，而不是实体状态容器。`SocialAPI.cs:12-24` 声明 `_mode`、`Achievements`、`Cloud`、`Network`、`JoinRequests`、`_modules` 和只读 `Mode`。`SocialAPI.cs:26-62` 在初始化时选择模式、创建模块列表和 JoinRequests、订阅 `Main.OnTickForInternalCodeOnly`，按 Steam/WeGame 分支加载 provider，并按注册顺序初始化模块；`SocialAPI.cs:64-73` 按反向顺序关闭模块。`Program.cs:204-215` 显示宿主先初始化 SocialAPI，`Main.cs:2886-2890` 显示命令行退出调用 Shutdown，`Netplay.cs:235-244` 在 TCP 监听前尝试 social network 监听，`NetMessage.cs:393-400` 将 social lobby ID 写入网络消息，`LaunchInitializer.cs:193-202` 按模式选择 cloud world 路径。`LoadSteam`/`LoadWeGame` 在 Version4 当前文件中为空，因此具体 provider 注册和其清理不能假定已经被证实。

所有目标均为 `status: proposed`。provider 对象、模块集合、JoinRequests 管理器和第三方 API 引用归 `proposed SocialProviderRegistryAdapter` 所有，不能进入 ECS Component；核心最多接收稳定的模式/能力快照。`proposed ExternalPlatformSessionComponent` 是否存在、挂在哪个 session/world entity 以及是否参与网络或持久化均是 `crossSubsystemOwner: integration-review`。

| source member | Version4 declaration/evidence | state kind | proposed role | ownership and lifecycle | status |
|---|---|---|---|---|---|
| `1000 _mode` | `D:\TRbackup\Version4\Terraria.Social\SocialAPI.cs:12` `private static SocialMode` | provider selection configuration | `proposed SocialProviderRegistryAdapter` 内部模式；可投影为稳定 `SocialMode` 值 | 写者：`Initialize`；读者：`Mode`、provider loader、宿主/Netplay；初始化前无值，Shutdown 后不可继续使用 | confirmed declaration; loader behavior partial |
| `1001 Achievements` | `SocialAPI.cs:14` public static `AchievementsSocialModule` | external provider reference | `proposed IAchievementsPort` 之后的 Adapter-owned provider slot | 只由 provider registration 写入；业务通过 typed port 读取/提交；模块初始化/关闭跟随 registry | confirmed declaration; concrete registration evidence-gap |
| `1002 Cloud` | `SocialAPI.cs:16` public static `CloudSocialModule` | external persistence capability reference | `proposed ICloudStoragePort` backed by `SocialProviderRegistryAdapter` | Adapter owns instance and calls; cloud path/file effects stay outside Component; `Main`/initializer consume explicit results | confirmed declaration; provider and failure semantics partial |
| `1003 Network` | `SocialAPI.cs:18` public static `NetSocialModule` | external social transport/session reference | `proposed ISocialNetworkPort` and provider-owned slot | Netplay asks the port to listen/query lobby; Adapter owns provider callbacks and release; lobby ID is external/network data, not an entity ID | confirmed declaration; cross-subsystem owner integration-review |
| `1004 JoinRequests` | `SocialAPI.cs:20` public static `ServerJoinRequestsManager` | external callback inbox/cache | Adapter-owned join-request bridge feeding later `proposed JoinRequestInboxComponent` or command | constructed during Initialize, subscribed to `Main.OnTickForInternalCodeOnly`; Version4 does not show unsubscribe/detach during Shutdown | confirmed declaration; cleanup evidence-gap |
| `1005 _modules` | `SocialAPI.cs:22` private static `List<ISocialModule>` | ordered provider registry | `proposed SocialProviderRegistryAdapter` private registry | `Initialize` populates and initializes in order; `Shutdown` reverses and shuts down; list must not leak through Query or Component | confirmed declaration; concrete module list partial |
| `1009 Mode` | `SocialAPI.cs:24` `public static SocialMode Mode => _mode` | derived read-only registry state | `proposed SocialModeQuery` over an immutable registry snapshot; optional `proposed SocialSessionCapabilityProjection` | no direct writer; Query is pure, Projection only emits diagnostics/UI/network view after integration approval | confirmed declaration |

### 8.1 proposed boundary and contracts

```text
proposed SocialProviderRegistryAdapter
    -> proposed ISocialProviderRegistryPort
        -> proposed typed provider ports (cloud / achievements / network)
            -> platform provider modules

proposed SocialRegistryLifecycleSystem
    -> initialize mode and provider registry
    -> publish capability snapshot
    -> request reverse-order shutdown

proposed SocialModeQuery
    -> read immutable mode/capability snapshot
    -> return derived eligibility only; no writes or external reads
```

`proposed ISocialProviderRegistryPort` exposes explicit initialize, capability snapshot, typed provider access, and shutdown results. It does not expose `List<ISocialModule>`, provider concrete types, mutable JoinRequests collections, or static globals. Initialization is a host effect; the Adapter owns provider construction and module lifetime. Shutdown must be reverse-order and idempotence/partial-failure behavior must be tested. The current `JoinRequests.Update` subscription is a known lifecycle gap; a future Adapter must detach it before provider shutdown or record why the host event lifetime makes detachment unnecessary.

The candidate `proposed ExternalPlatformSessionComponent` contains only stable data such as `SocialMode`, availability flags, and a generation/epoch if integration requires stale-result rejection. It does not contain Cloud/Network/Achievements objects, `ServerJoinRequestsManager`, lobby handles, provider callbacks, or `_modules`. `SocialModeQuery` is pure over a captured snapshot. `SocialSessionCapabilityProjection` is non-authoritative and may feed UI, diagnostics, or a network/persistence view only after `crossSubsystemOwner: integration-review` decides the contract.

### 8.2 System order and failure boundary

```text
host startup
  -> proposed SocialRegistryLifecycleSystem.Initialize
  -> provider selection and registration
  -> module Initialize in registration order
  -> publish immutable capability snapshot
  -> Netplay/other consumers may issue explicit port commands
host shutdown
  -> stop new social commands
  -> detach JoinRequests tick callback (required proposed behavior)
  -> module Shutdown in reverse order
  -> release provider references and publish terminal diagnostic
```

Provider initialization failure is a typed unavailable/failed result, not a fake `SocialMode.None` success. A callback arriving during shutdown is rejected or drained according to an explicit generation check. Cloud writes and network actions have external side effects; timeout or provider exception must be classified as failure or unknown, with no unbounded retry. Logs and capability projections observe results and do not mutate session state.

### 8.3 Social registry focused verifier plan

All rows below are future `proposed` verifiers; none ran in this session:

| verifier | scope | expected assertion |
|---|---|---|
| `SocialProviderRegistryLifecycleVerifier` (proposed) | fake provider modules and registry port | mode selection, registration order, reverse shutdown, repeated initialize/shutdown, partial initialization cleanup |
| `SocialRegistryOwnershipVerifier` (proposed) | static scan and component schema | no provider object, module list, callback delegate, cloud/network handle, or JoinRequests manager appears in a Component |
| `SocialCallbackDetachmentVerifier` (proposed) | host tick harness | JoinRequests update is attached once, detached before shutdown completion, and cannot mutate state after terminal generation |
| `SocialCapabilityProjectionVerifier` (proposed) | immutable snapshot/query harness | `SocialModeQuery` is pure; unavailable/failed providers are distinguishable from `None`; projection never writes authority |

## 9. SocialTransportIpc 检查点（9/59）

Version4 `Terraria.Social.WeGame.IPCBase.cs:13-29` declares `_producer`, `_consumer`, `_totalData`, `_listLock`, `_pipeBrokenFlag`, `_pipeStream`, `_cancelTokenSrc`, `_onDataArrive`, and `BufferSize`; the event accessors at `:31-41` combine/remove `_onDataArrive`, and the constructor at `:43-46` sets `BufferSize = 256`. `Reset`, `ProcessDataArriveEvent`, `BeginReadData`, `ReadCallback`, `Send`, and `SendCallback` are empty or stubbed in the Version4 checkout (`:47-59`).

The same-path complete reference `D:\TRbackup\无任何删减通过编译\Terraria.Social.WeGame\IPCBase.cs:50-223` supplies supplemental behavior with version drift: lock-protected producer/consumer swapping, cancellation and pipe disposal, queue draining, async `BeginRead`/`EndRead`, message-complete frame assembly, UTF-8 string encoding, async writes, and IOException/InvalidOperationException broken-pipe classification. It also declares `_haveDataToReadFlag`, which is absent from the Version4 member inventory; this is recorded as a drift/evidence gap and is not counted as a tenth P15 member.

All targets remain `status: proposed`. `proposed WeGameIpcTransportAdapter` owns pipe handles, cancellation, locks, mutable byte buffers, callback registration, frame assembly, and broken-pipe state. `proposed IpcTransportLifecycleSystem` drains completed frames on the simulation/host thread and emits explicit `proposed IpcDataReceived` events or commands. No Component owns a `PipeStream`, `CancellationTokenSource`, lock, queue, callback delegate, or third-party protocol type. `BufferSize` is configuration for the Adapter, not entity state; only a validated immutable configuration snapshot may be exposed to diagnostics.

| source member | Version4 declaration/evidence | state kind | proposed role | ownership, lifecycle, and effects | status |
|---|---|---|---|---|---|
| `992 _producer` | `D:\TRbackup\Version4\Terraria.Social.WeGame\IPCBase.cs:13` `List<List<byte>>` | cross-thread ingress buffer | private field of `proposed WeGameIpcTransportAdapter` | async callback appends under `_listLock`; simulation-thread drain swaps/clears; never exposed | confirmed declaration; behavior supplemental |
| `993 _consumer` | `IPCBase.cs:15` `List<List<byte>>` | drain-side buffer | private Adapter field | owns the swapped batch during one drain; clear/reuse and shutdown disposal are Adapter effects | confirmed declaration; behavior supplemental |
| `994 _totalData` | `IPCBase.cs:17` `List<byte>` | partial-frame assembly buffer | private Adapter field | callback appends bytes until message-complete; reset/pipe failure must discard or classify partial frame explicitly | confirmed declaration; framing behavior supplemental |
| `995 _listLock` | `IPCBase.cs:19` `object` | synchronization primitive | Adapter-internal seam | only protects producer/consumer exchange; no ECS visibility; lock ordering and shutdown race require verifier | confirmed declaration; synchronization behavior supplemental |
| `996 _pipeBrokenFlag` | `IPCBase.cs:21` `volatile bool` | transport health/cache | Adapter-internal status | set by read/write technical failures; exposed only as immutable status result; must not silently trigger business state | confirmed declaration; failure behavior supplemental |
| `997 _pipeStream` | `IPCBase.cs:23` `PipeStream` | external resource handle | Adapter-owned pipe resource | created/injected by transport composition, read/written asynchronously, disposed on reset/close; no Component field | confirmed declaration; lifecycle partial |
| `998 _cancelTokenSrc` | `IPCBase.cs:25` `CancellationTokenSource` | asynchronous lifecycle control | Adapter-owned cancellation source | cancellation precedes disposal; cancellation is cooperative and in-flight remote effects are not assumed undone | confirmed declaration; cleanup behavior supplemental |
| `999 _onDataArrive` | `IPCBase.cs:27` `Action<byte[]>` | callback registration | Adapter-owned callback boundary | callback only enqueues complete frames; dispatch is later and explicit; detach before terminal shutdown | confirmed declaration; callback ordering partial |
| `1008 BufferSize` | `IPCBase.cs:29` `int` with constructor default `256` at `:43-46` | transport configuration | proposed `IWeGameIpcTransportPort` configuration/value object | validated positive bound before allocation; stable for a read session; changes require explicit restart or rejection | confirmed declaration |

### 9.1 proposed boundary, framing, and thread contract

```text
async PipeStream read callback
  -> proposed WeGameIpcTransportAdapter.EndRead/frame assembly
  -> lock-protected producer queue
  -> proposed IpcTransportLifecycleSystem drain on designated host/simulation thread
  -> proposed IpcDataReceived event or protocol command

proposed IpcSendCommand
  -> proposed WeGameIpcTransportAdapter UTF-8 encode/write
  -> completion/failure/unknown diagnostic
```

`proposed IWeGameIpcTransportPort` should expose open/read-start, drain-complete-frames, send bytes/string through a stable request type, reset/close, and an immutable status result. The port must not return internal lists or let callers invoke `BeginRead`/`EndRead` directly. The Adapter should serialize `string` input with the observed UTF-8 behavior and preserve the pipe's message-boundary contract; a future implementation must verify whether partial reads use `PipeStream.IsMessageComplete` exactly or another framed protocol.

The callback thread may perform only buffer mutation and technical error classification. It must not mutate ECS state, publish UI effects, or call arbitrary game logic. The designated lifecycle System materializes queued frames once per tick, publishes or commands them in FIFO batch order, and owns any protocol-level validation. If a frame is malformed, oversized, cancelled, or arrives after generation shutdown, it is rejected with a typed diagnostic. A write that returns before remote processing is confirmed is success of local submission only; timeout/disconnect after submission is `unknown`, not a safe retry. No unbounded retry is proposed.

### 9.2 System order and focused verifier plan

```text
transport open/configure
  -> create cancellation/resource ownership
  -> start at most one read chain
  -> async callback assembles complete frames
  -> tick System drains queue and emits stable payloads
shutdown
  -> stop accepting sends
  -> cancel reads
  -> detach callback/event
  -> dispose pipe
  -> publish terminal status after resources are released
```

The complete verifier matrix remains proposed; this session's focused verifier covers the core registry, IPC, Workshop/Join, ResourcePack, crypto-ordering, NAT, and platform paths. The broader host/provider/ownership checks below remain unrun:

| verifier | scope | expected assertion |
|---|---|---|
| `WeGameIpcFramingVerifier` (proposed) | fake pipe with split and multi-frame reads | partial bytes are retained, complete messages are emitted once, FIFO order is preserved, and malformed/oversized frames are classified |
| `WeGameIpcConcurrencyVerifier` (proposed) | callback/drain/shutdown harness | producer/consumer swap is race-free; callback cannot enqueue after close; lock is not held while invoking external handlers |
| `WeGameIpcFailureVerifier` (proposed) | fake pipe failures | IOException, invalid operation, cancellation, and local-write success/remote-unknown have distinct outcomes |
| `WeGameIpcOwnershipVerifier` (proposed) | static scan/schema | no PipeStream, cancellation source, lock, mutable queue, or callback delegate appears in a Component |

## 10. WorkshopAndJoinBoundaryData 检查点（17/59）

Version4 declares the 17 inventoried members in the eight `Terraria.Social.Base` files listed by the input report. `CloudSocialModule.EnabledByDefault` is read by `Main.cs:2493` when creating world metadata and the cloud provider is queried by `Main.cs:1786-1793`; `Program.cs:204-215` initializes the registry before the host. `WorkshopSocialModule.cs:7-30` shows the external operations that consume publish settings and return `FoundWorkshopEntryInfo`, subscribed paths, or imported worlds, but concrete provider implementations are not present in the Version4 evidence. `RichPresenceState.GameMode` is a single derived game-mode value; the complete reference's `GetCurrentState` maps menu/player/world/single/multi states, while Version4 retains only the field and equality stub. Version4 `ServerJoinRequestsManager` owns `_requests`, a read-only view, and per-tick validity removal (`ServerJoinRequestsManager.cs:8-28`); `UserJoinToServerRequest.cs:7-18` owns display name and full external identifier and exposes abstract validity/presentation methods. `WorkshopIssueReporter` stores `_reports` and returns a new list in the Version4 stub; `IssueReport` timestamps reports with `DateTime.Now` in `Terraria.DataStructures.IssueReport.cs:5-15`.

The same-path complete reference is supplemental and version-drifted: `ServerJoinRequestsManager` adds `Add`, accept/reject removal, and added/removed events; `UserJoinToServerRequest` adds accept/reject events; `WorkshopIssueReporter` adds a 1000-report cap and UI notification callbacks; `WorkshopItemPublishSettings` adds a pure tag-name projection. These members are not in the Version4 P15 inventory and are not counted as additional members. tModLoader v2026.07 pages for `FoundWorkshopEntryInfo`, `RichPresenceState`, `WorkshopIssueReporter`, `WorkshopItemPublishSettings`, and `WorkshopTagOption` corroborate public shapes only and do not establish NLTX behavior.

All target types are `status: proposed`. `proposed WorkshopBoundaryAdapter` owns provider DTO conversion, publish/lookup/import commands, external workshop IDs, file paths, tag arrays, and provider callbacks. `proposed JoinRequestInboxComponent` is only a candidate for a core/session entity: if integration approves it, it stores a third-party-free immutable request snapshot, validity deadline/generation, and a separate external user identifier, while the Adapter retains the provider request object. `proposed RichPresenceProjection` and `proposed WorkshopIssueProjection` are non-authoritative output views. No Component stores `FoundWorkshopEntryInfo`, `UserJoinToServerRequest`, `IssueReport`, provider module, cloud object, or arbitrary mutable arrays.

| source member | Version4 declaration/evidence | state kind | proposed role | ownership, lifecycle, and effects | status |
|---|---|---|---|---|---|
| `977 CloudSocialModule.EnabledByDefault` | `D:\TRbackup\Version4\Terraria.Social.Base\CloudSocialModule.cs:8`; read at `Main.cs:2493` | cloud capability policy | Adapter-owned cloud capability; optional stable `CloudSaveAvailability` projection | provider writes during registration/configuration; world metadata consumes a snapshot; persistence owner is cross-subsystem | confirmed declaration; provider policy partial |
| `978 FoundWorkshopEntryInfo.workshopEntryId` | `FoundWorkshopEntryInfo.cs:5` `ulong` | external Workshop identifier | Adapter DTO / `proposed WorkshopEntrySnapshot.ExternalWorkshopId` | provider response only; never entity/network ID; retained only while lookup/publish result is needed | confirmed declaration |
| `979 FoundWorkshopEntryInfo.publicity` | `FoundWorkshopEntryInfo.cs:7` `WorkshopItemPublicSettingId` | external metadata | Workshop response DTO/projection | read by publish UI/compatibility layer; no core authority without integration decision | confirmed declaration |
| `980 FoundWorkshopEntryInfo.tags` | `FoundWorkshopEntryInfo.cs:9` `string[]` | external metadata collection | immutable Workshop response snapshot | Adapter copies/validates values; no mutable array leak; tag semantics/version are provider-owned | confirmed declaration |
| `981 FoundWorkshopEntryInfo.previewImagePath` | `FoundWorkshopEntryInfo.cs:11` `string` | external file path | Adapter DTO and UI projection field | path is external/presentation data; no persistence/network reuse as generic ID | confirmed declaration |
| `982 FoundWorkshopEntryInfo.publishedVersion` | `FoundWorkshopEntryInfo.cs:13` `int` | provider metadata/version | Workshop response snapshot | compare only in provider/workshop policy; not game protocol version | confirmed declaration |
| `983 RichPresenceState.GameMode` | `RichPresenceState.cs:17` `GameModeState` | derived presentation state | `proposed RichPresenceProjection` | computed from explicit host/game snapshot; provider update is an external side effect; no ECS authority | confirmed field; derivation partial |
| `984 ServerJoinRequestsManager._requests` | `ServerJoinRequestsManager.cs:8` `List<UserJoinToServerRequest>` | transient external request inbox | Adapter-owned queue; candidate source for `proposed JoinRequestInboxComponent` | Update removes invalid requests; add/accept/reject behavior is incomplete/version-drift; no provider object crosses | confirmed declaration; lifecycle partial |
| `985 ServerJoinRequestsManager.CurrentRequests` | `ServerJoinRequestsManager.cs:10` read-only collection | read view/cache | Adapter query -> immutable request snapshots | read-only wrapper still exposes mutable element identity; copy and validate before projection | confirmed declaration |
| `986 WorkshopIssueReporter._reports` | `WorkshopIssueReporter.cs:11` `List<IssueReport>` | diagnostic accumulator | Adapter-owned bounded report store; `proposed WorkshopIssueProjection` | Version4 capacity/notification behavior is missing; DateTime-based report creation is external clock input; no business state mutation | confirmed declaration; retention partial |
| `987 WorkshopItemPublishSettings.UsedTags` | `WorkshopItemPublishSettings.cs:7` `WorkshopTagOption[]` | publish command input | stable `proposed WorkshopPublishRequest` tag values | Command owner validates/copies; provider Adapter maps to API names; do not persist third-party array | confirmed declaration |
| `988 WorkshopItemPublishSettings.Publicity` | `WorkshopItemPublishSettings.cs:9` enum | publish command input | `proposed WorkshopPublishRequest.Visibility` | validated enum maps to provider visibility; no implicit default across providers | confirmed declaration |
| `989 WorkshopItemPublishSettings.PreviewImagePath` | `WorkshopItemPublishSettings.cs:11` string | publish command input/path | `proposed WorkshopPublishRequest.PreviewPath` | Adapter validates path and performs I/O; path is not network/entity identity | confirmed declaration |
| `990 WorkshopTagOption.NameKey` | `WorkshopTagOption.cs:5` readonly string | localization/presentation metadata | immutable tag option DTO | UI/localization reader only; no provider write by itself | confirmed declaration |
| `991 WorkshopTagOption.InternalNameForAPIs` | `WorkshopTagOption.cs:7` readonly string | provider serialization metadata | Adapter-only tag mapping value | provider boundary converts it to request payload; never use it as a general content ID | confirmed declaration |
| `1006 UserJoinToServerRequest.UserDisplayName` | `UserJoinToServerRequest.cs:7` internal string | presentation snapshot | copied field in proposed join request snapshot | Adapter sanitizes/copies for UI; not authentication identity or persistence key | confirmed declaration |
| `1007 UserJoinToServerRequest.UserFullIdentifier` | `UserJoinToServerRequest.cs:9` internal string | external user identifier | separate `ExternalUserIdentifier` in proposed snapshot | provider-owned identity; never conflate with Entity ID, persistence ID, or network ID | confirmed declaration |

### 10.1 proposed boundaries and explicit effects

```text
proposed WorkshopPublishCommand / proposed WorkshopLookupQuery
  -> proposed WorkshopBoundaryAdapter
      -> provider WorkshopSocialModule
      -> immutable result / failure / unknown

provider join callback
  -> Adapter-owned request queue
  -> validity/expiry calculation
  -> proposed JoinRequestInboxComponent snapshot (integration-review)
  -> explicit AcceptJoinCommand or RejectJoinCommand

game snapshot
  -> pure proposed RichPresenceQuery
  -> proposed RichPresenceProjection
  -> provider update command (external side effect)
```

Publish, download, import, cloud file, and provider UI operations are effects. A completed local upload request is not proof of remote publication; delayed callbacks and unknown results require idempotency key/provider lookup or manual recovery. Request acceptance/rejection must be a single explicit command owner; removal from the Adapter queue occurs after the command result, while expiry removes only the snapshot or provider request according to the provider contract. Per-tick validity checks are deterministic over an injected time/deadline snapshot; they do not read clocks directly inside a pure Query.

Rich Presence is a projection, not gameplay state. `GameMode` must be derived from an explicit game/session snapshot, and provider notification is ordered after a changed projection is observed. `IssueReport.timeReported` is diagnostic time, not simulation time; report retention and UI notification are Adapter/Projection concerns. `GetReports` must return a defensive immutable snapshot in any future implementation, because a read-only interface does not make the list immutable.

### 10.2 Workshop/Join focused verifier plan

The complete verifier matrix remains proposed; the focused verifier covers the core IPC paths, while the broader host/provider and ownership checks below remain unrun:

| verifier | scope | expected assertion |
|---|---|---|
| `WorkshopDtoOwnershipVerifier` (proposed) | DTO/schema/static scan | external Workshop IDs, paths, tags, provider DTOs, cloud modules, and issue-report lists do not enter core Components |
| `JoinRequestLifecycleVerifier` (proposed) | fake provider clock and request harness | duplicate replacement, expiry, accept/reject removal, callback detachment, stale generation rejection, and defensive snapshots are deterministic |
| `WorkshopEffectResultVerifier` (proposed) | fake Workshop port | publish/download/import success, explicit failure, timeout/unknown, cancellation, and bounded retry policy remain distinguishable |
| `RichPresenceProjectionVerifier` (proposed) | immutable game snapshot | all five GameMode states map deterministically; unchanged state does not emit duplicate provider updates |
| `WorkshopReportProjectionVerifier` (proposed) | fake report store | report timestamp/retention and UI notification are observational and cannot mutate gameplay authority |

## 11. SharedResourcePackAdapters 检查点（10/59）

Version4 `Terraria.IO.ResourcePack.cs:21-37` declares path/name metadata, a service provider, compression and branding flags, a `ZipFile` handle, an icon cache, and the icon/manifest format constants. Its constructor at `:39-57` performs file/directory existence checks, derives `FileName`, stores the service/path/branding values, opens a zip for compressed input, and calls `LoadManifest`; `CreateIcon` and `LoadManifest` are empty at `:59-62`, so manifest/icon behavior is not closed by the Version4 body. `ResourcePackList.cs:15-23` owns a private list and copies an input enumeration into it. Current Version4 callers show only the Workshop provider signatures and commented resource-pack preference hooks in `Main.cs:3314-3323`; no complete selection/persistence path is confirmed in this checkout.

The same-path complete reference is supplemental and version-drifted. It adds `_contentSource`, `Icon`, manifest properties (`Name`, `Description`, `Author`, `Version`), mutable `IsEnabled`/`SortingOrder`, content-source creation, zip/file stream access, manifest parsing, and ResourcePackList enumeration/sort/JSON/workshop discovery. These added fields/properties are not in the Version4 P15 inventory and are not counted as extra members. tModLoader v2026.07 `class_resource_pack.html` and `class_resource_pack_list.html` corroborate the public shape only; they do not establish current NLTX behavior.

All target types are `status: proposed`. `proposed ResourcePackBoundaryAdapter` owns path validation, directory/zip selection, manifest/icon reads, service/content-source handles, and disposal. `proposed ResourcePackMetadataProjection` exposes immutable name/path/branding/compression metadata only after successful parsing. `proposed ResourcePackCollectionAdapter` owns the private list and deterministic discovery/deduplication; a `proposed ResourcePackSelectionComponent` is optional and blocked on the missing Version4 selection/persistence contract. No Component may own `ZipFile`, `Texture2D`, `IServiceProvider`, content source, open stream, or raw path as an implicit authoritative identity.

| source member | Version4 declaration/evidence | state kind | proposed role | ownership, lifecycle, and effects | status |
|---|---|---|---|---|---|
| `2753 ResourcePack.FullPath` | `D:\TRbackup\Version4\Terraria.IO\ResourcePack.cs:21` readonly string | external resource locator | Adapter-owned path; optional validated metadata projection | constructor validates file/directory and all reads stay Adapter-owned; not an entity/persistence ID | confirmed declaration; path policy partial |
| `2754 ResourcePack.FileName` | `ResourcePack.cs:23` readonly string | derived display/discovery key | immutable metadata projection and collection dedup key | derived with `Path.GetFileName`; collision and case policy require verifier; not a global ID | confirmed declaration |
| `2755 ResourcePack._services` | `ResourcePack.cs:25` readonly `IServiceProvider` | dependency locator | Adapter composition dependency | only Adapter resolves asset/content services; no service locator in Component | confirmed declaration; use partial |
| `2756 ResourcePack.IsCompressed` | `ResourcePack.cs:27` readonly bool | derived storage format | immutable `ResourcePackFormat` metadata | derived from file existence in constructor; inconsistent file/directory races are Adapter failures | confirmed declaration |
| `2757 ResourcePack.Branding` | `ResourcePack.cs:29` readonly `BrandingType` | external origin metadata | immutable branding projection | provider/workshop origin only; cannot select persistence or network owner by itself | confirmed declaration |
| `2758 ResourcePack._zipFile` | `ResourcePack.cs:31` readonly `ZipFile` | external resource handle | private Adapter resource | created only for compressed input; close/dispose on failed parse, replacement, and shutdown; never crosses boundary | confirmed declaration; disposal partial |
| `2759 ResourcePack._icon` | `ResourcePack.cs:33` `Texture2D` cache | presentation/cache | Adapter-owned lazy icon cache and projection source | asset load may fail; cache invalidation/disposal follows graphics owner; no ECS authority | confirmed declaration; body stub |
| `2760 ResourcePack.ICON_FILE_NAME` | `ResourcePack.cs:35` private const string | format compatibility constant | Adapter-internal constant | read path only; no registration key or Component field | confirmed declaration |
| `2761 ResourcePack.PACK_FILE_NAME` | `ResourcePack.cs:37` private const string | format compatibility constant | Adapter-internal constant | manifest lookup only; parse failure is explicit invalid-pack result | confirmed declaration |
| `2762 ResourcePackList._resourcePacks` | `ResourcePackList.cs:15` private readonly list | adapter collection/cache | private state of `proposed ResourcePackCollectionAdapter` | constructor copies inputs; discovery/dedup/sort/persistence snapshots are Adapter/Projection effects; no list leak | confirmed declaration; collection lifecycle partial |

### 11.1 proposed resource-pack flow and ownership

```text
proposed ResourcePackDiscoveryCommand(search root, workshop paths)
  -> proposed ResourcePackCollectionAdapter
      -> proposed ResourcePackBoundaryAdapter
          -> file system / zip / manifest / asset services
      -> immutable ResourcePackMetadataProjection

proposed ResourcePackSelectionCommand
  -> optional proposed ResourcePackSelectionComponent (integration-review)
  -> adapter-owned load/apply command
```

The Adapter must distinguish directory packs from compressed packs, validate the manifest before exposing metadata, and close all file/zip/stream resources on success, parse failure, cancellation, and replacement. A cached icon is presentation data and may be dropped/reloaded; it must not be serialized as entity state. Resource-pack path, FileName, Workshop ID, persistence entry key, and content asset path remain distinct identifiers. Discovery failures can skip one invalid pack only if the collection contract says so; otherwise the error must stop the operation explicitly rather than silently produce a partial list.

The complete reference's `IsEnabled` and `SortingOrder` are not Version4 members. They may inform a future selection component only after the integration owner confirms persistence format and application order. Until then, `ResourcePackList` is an Adapter-owned collection and any UI list is a projection. No file, zip, asset, or service side effect is performed by a Query.

### 11.2 Resource-pack focused verifier plan

The complete verifier matrix remains proposed; the focused verifier covers defensive Workshop snapshots, Join expiry/generation, and rich-presence calculation, while the broader provider-effect checks below remain unrun:

| verifier | scope | expected assertion |
|---|---|---|
| `ResourcePackDiscoveryVerifier` (proposed) | temporary directory/zip fixtures | missing paths, directory packs, zip packs, duplicate filenames, malformed manifests, and deterministic ordering are classified |
| `ResourcePackOwnershipVerifier` (proposed) | static scan/schema | no ZipFile, Texture2D, IServiceProvider, stream, or raw resource handle enters a Component |
| `ResourcePackLifecycleVerifier` (proposed) | fake zip/content/asset ports | all success, parse failure, cancellation, replacement, and shutdown paths release resources exactly within Adapter ownership |
| `ResourcePackMetadataProjectionVerifier` (proposed) | immutable DTO harness | path, filename, compression, branding, and manifest values are copied without mutable collection leaks |
| `ResourcePackSelectionCompatibilityVerifier` (proposed) | future persistence fixture | only runs after integration defines IsEnabled/SortingOrder ownership; until then remains deferred |

## 12. CryptographicDependency 检查点（10/59）

Version4 `D:\TRbackup\Version4\BCrypt.Net\BCrypt.cs:8-41` declares the ten inventoried members: `DefaultRounds`, `BCryptSaltLen`, strict `SafeUTF8`, `BlowfishNumRounds`, the 128-entry `Index64` table, `EmptyString`, `DefaultHashVersion`, `Nul`, `MinRounds`, and `MaxRounds`. `CryptRaw` is a stub at `BCrypt.cs:42-44`, so the Version4 checkout does not independently prove the algorithm body. The same-path complete reference is supplemental and version-drifted: it adds Blowfish `POrig`/`SOrig` tables, mutable `_p`/`_s` work buffers, key expansion/encipher helpers, base64 support, and a concrete `CryptRaw` implementation. Its method validates work factor 4..31 and a 16-byte salt, performs the expanded key schedule and repeated keying, and emits the raw ciphertext; these extra implementation members are not in the Version4 P15 inventory.

Version4 `Terraria.Utilities\Secrets.cs:9-29` fixes the salt to base64 `fT2JQQzNMJl2NRoMbo9RjA==`, encodes input with `Encoding.UTF8`, calls `CryptRaw` with work factor 4, performs 1000 byte swaps, calls `CryptRaw` again with the same salt/work factor, and returns standard base64. `WorldGen.cs:476` and `:517` consume `Secrets.ToSecret` for secret-seed comparisons. This is a compatibility-sensitive deterministic calculation, not ECS entity state and not an external I/O boundary in itself; its implementation must remain behind a proposed Adapter until the exact reference algorithm, encoding/error behavior, and output vectors are locked.

All target types are `status: proposed`. `proposed SecretDerivationAdapter` owns the BCrypt constants/tables, mutable cryptographic work buffers, salt validation, encoding, and deterministic transform. `proposed ISecretDerivationPort` exposes only a value-in/value-out operation with typed invalid-input/compatibility errors. No Component, snapshot, network field, persistence record, log, or query stores secret input, salt, intermediate bytes, P/S tables, or raw cryptographic output unless a separately approved caller contract requires a derived comparison value.

| source member | Version4 declaration/evidence | state kind | proposed role | ownership and compatibility boundary | status |
|---|---|---|---|---|---|
| `4054 DefaultRounds` | `D:\TRbackup\Version4\BCrypt.Net\BCrypt.cs:8` const int `11` | algorithm compatibility constant | Adapter-internal default parameter | not used by the observed `Secrets` call, so do not silently substitute it for work factor 4 | confirmed declaration; usage partial |
| `4055 BCryptSaltLen` | `BCrypt.cs:10` const int `16` | algorithm validation constant | Adapter-internal salt-length check | complete reference validates exactly 16 bytes; mismatch is explicit input error | confirmed declaration; body supplemental |
| `4056 SafeUTF8` | `BCrypt.cs:12` strict UTF-8 Encoding | encoding policy/dependency | Adapter-internal encoding seam | invalid UTF-8 behavior must be locked; outer `Secrets` uses `Encoding.UTF8.GetBytes` and needs compatibility decision | confirmed declaration; call path partial |
| `4057 BlowfishNumRounds` | `BCrypt.cs:14` const int `16` | algorithm round constant | Adapter-internal Blowfish configuration | not a gameplay or persisted field; exact use comes from complete-reference implementation | confirmed declaration; body supplemental |
| `4058 Index64` | `BCrypt.cs:16-31` static int[128] alphabet index table | immutable algorithm table | Adapter-internal decode table | copy only within implementation; no public/shared mutable array | confirmed declaration; Version4 behavior stub |
| `4059 EmptyString` | `BCrypt.cs:33` const string | compatibility/parser constant | Adapter-internal constant | no Component or protocol representation | confirmed declaration |
| `4060 DefaultHashVersion` | `BCrypt.cs:35` const char `a` | hash-format compatibility constant | Adapter-internal format setting | cannot be changed without output-vector review | confirmed declaration; body supplemental |
| `4061 Nul` | `BCrypt.cs:37` const string | byte/string compatibility constant | Adapter-internal terminator setting | preserve only if complete algorithm requires it; not exposed to callers | confirmed declaration; body supplemental |
| `4062 MinRounds` | `BCrypt.cs:39` const short `4` | input validation bound | Adapter-internal bound | observed `Secrets` work factor equals the minimum; invalid values return typed rejection | confirmed declaration |
| `4063 MaxRounds` | `BCrypt.cs:41` const short `31` | input validation bound | Adapter-internal bound | cap prevents unbounded cost; caller cannot override without explicit compatibility decision | confirmed declaration |

### 12.1 proposed crypto boundary and ordering

```text
proposed SecretDerivationCommand(plain input, compatibility profile)
  -> proposed ISecretDerivationPort
      -> proposed SecretDerivationAdapter / BCrypt implementation
  -> deterministic result or typed rejection
  -> WorldGen comparison/projection caller
```

The Adapter must be deterministic for identical input, salt, work factor, and compatibility profile. It must not read clocks, random sources, files, network state, or mutable global ECS state. The caller owns normalization such as WorldGen's lowercase/alphanumeric preprocessing; the Adapter owns encoding and algorithm validation only if that matches the locked reference contract. Intermediate arrays and tables are zeroed/discarded according to the eventual security review; logging must never include plaintext, salt, or derived secret.

The two `CryptRaw` calls and 1000-swap loop are an explicit ordered transform. They must not be parallelized, cached across different inputs without an owner, or replaced by a platform BCrypt API unless output vectors prove compatibility. Work factor 4 is not interchangeable with `DefaultRounds = 11`. Because Version4's `CryptRaw` body is stubbed and the complete reference is a supplemental path, cross-version output vectors, empty input, long input, non-ASCII input, invalid salt, invalid work factor, and byte-length edge cases are `evidence-gap` until independently verified.

### 12.2 Crypto focused verifier plan

The complete verifier matrix remains proposed; the focused verifier covers directory/zip discovery, metadata parsing, ordering, and disposal, while the broader asset-service/selection checks below remain unrun:

| verifier | scope | expected assertion |
|---|---|---|
| `SecretDerivationVectorVerifier` (proposed) | locked reference vectors and WorldGen callers | two-pass transform, salt, work factor, swap loop, base64 output, and known secret-seed values match the selected compatibility profile |
| `BcryptValidationVerifier` (proposed) | fake/real Adapter boundary | 16-byte salt, rounds 4..31, empty/long/non-ASCII input, and invalid arguments have stable outcomes |
| `SecretOwnershipVerifier` (proposed) | static scan/API shape | no plaintext, salt, intermediate buffer, table, or derived secret is stored in a Component or emitted to logs/network snapshots |
| `SecretDeterminismVerifier` (proposed) | repeated pure calls | same explicit inputs produce the same bytes and no hidden clock/random/global-state read occurs |

## 13. NatPortMappingInterop 检查点（4/59）

Version4 declares the four inventoried COM properties: `IStaticPortMapping.InternalPort` at `D:\TRbackup\Version4\NATUPNPLib\IStaticPortMapping.cs:14-20`, `IStaticPortMapping.Protocol` at `:22-29`, `IStaticPortMapping.InternalClient` at `:31-38`, and `IUPnPNAT.StaticPortMappingCollection` at `D:\TRbackup\Version4\NATUPNPLib\IUPnPNAT.cs:12-18`. The related collection interface has enumeration, `Remove`, and `Add`, but those methods are not members in this P15 property inventory. `Netplay.OpenPort` at `D:\TRbackup\Version4\Terraria\Netplay.cs:181-206` creates the UPnP COM object through the observed CLSID, gets the collection, enumerates mappings, and treats a mapping with matching internal port, local internal client, and TCP protocol as already present; otherwise it adds the same port as external and internal with description `Terraria Server`. `UseUPNP` gates the call in `Netplay.InitializeServer` at `:294-305`, after listener startup. The surrounding `_upnpnat` and `_mappings` fields are Netplay state, not P15 inventory members.

Version4 wraps `OpenPort` in a broad catch at server initialization and provides no confirmed mapping removal or COM-release path when the server stops. tModLoader v2026.07 interface pages corroborate the COM property/method shape only and cannot establish Terraria cleanup behavior. This leaves NAT ownership, cleanup, duplicate mappings, adapter failure classification, and post-exception result state as explicit evidence gaps.

All targets are `status: proposed`. `proposed NatPortMappingAdapter` owns COM activation, `IUPnPNAT` and collection handles, enumeration, Add/Remove calls, local-address lookup, and exception translation. `proposed NatPortMappingLifecycleSystem` converts host server start/stop into explicit ensure/release commands. `proposed NatPortMappingKey` is a provider-neutral value object that separates external port, internal port, protocol, internal client, and description; it is not an Entity ID, persistence ID, network session ID, or Workshop external ID. `proposed NatPortMappingStatusProjection` is non-authoritative. A `proposed NatPortMappingStatusComponent` is optional only if integration review requires a session diagnostic snapshot; it must not own COM handles or become the source of truth for router state.

| source member | Version4 declaration/evidence | state kind | proposed role | ownership, lifecycle, and effects | status |
|---|---|---|---|---|---|
| `4064 IStaticPortMapping.InternalPort` | `D:\TRbackup\Version4\NATUPNPLib\IStaticPortMapping.cs:14-20` readonly COM property | external mapping observation | Adapter-side mapping candidate field in `proposed NatPortMappingKey` | read during enumeration; copied to a stable value before comparison; never a Component handle | confirmed declaration |
| `4065 IStaticPortMapping.Protocol` | `IStaticPortMapping.cs:22-29` readonly BSTR property | external mapping observation | normalized protocol in Adapter-side key | current caller compares TCP; normalization/case policy must be fixed before release; no generic string ID | confirmed declaration |
| `4066 IStaticPortMapping.InternalClient` | `IStaticPortMapping.cs:31-38` readonly BSTR property | external mapping observation | validated internal client address in Adapter-side key | compared with the resolved local address; address resolution is an external effect and may change between ensure/release | confirmed declaration |
| `4067 IUPnPNAT.StaticPortMappingCollection` | `D:\TRbackup\Version4\NATUPNPLib\IUPnPNAT.cs:12-18` readonly COM interface property | external resource/collection handle | private handle inside `proposed NatPortMappingAdapter` | acquired after COM activation; enumeration/Add/Remove occur only through Adapter; release and stale-handle behavior require lifecycle verifier | confirmed declaration; cleanup partial |

### 13.1 proposed NAT boundary and ordering

```text
proposed ServerStartCommand
  -> proposed NatPortMappingLifecycleSystem
  -> proposed INatPortMappingPort.Ensure(mapping key)
      -> proposed NatPortMappingAdapter
          -> COM UPnP activation / collection enumeration / Add
  -> success / already-present / unavailable / failure / unknown

proposed ServerStopCommand
  -> proposed NatPortMappingLifecycleSystem
  -> proposed INatPortMappingPort.Release(mapping ownership)
  -> confirmed removed / already-absent / not-owned / failure / unknown
```

The observed Version4 order is server initialization, social/TCP listener startup, then optional UPnP ensure. A proposed implementation must make that order explicit and must not make listener readiness depend on a successful router mapping unless integration assigns that policy. Enumeration is a read of external mutable state; the Adapter must materialize it before comparing and must not hold a COM object or lazy enumerator in a Component.

Ensure should be idempotent for the Adapter's ownership key. Existing mappings must not be removed or overwritten merely because the description differs, and release must not remove a mapping that the process did not create. Since Version4's match omits an explicit external-port property and no ownership token is confirmed, the exact ownership/release rule is `crossSubsystemOwner: integration-review`. If Add or Remove throws after the router may have applied the operation, the result is unknown; confirm through a fresh query before retrying. No unbounded retry is proposed.

The status projection reports only the last observed result, key, and diagnostic category. It must not claim router truth after a failed or interrupted query, and it must not drive gameplay/network state by itself. COM references, collection enumerators, local address lookup, and exception objects remain Adapter-owned. Cleanup must cover server stop, failed listener startup, process exit, cancellation, and partial initialization, but the actual Version4 paths are not confirmed.

### 13.2 NAT focused verifier plan

The complete verifier matrix remains proposed; the focused verifier covers ordered secret derivation through the injected primitive, while compatibility vectors and the concrete BCrypt primitive remain unrun:

| verifier | scope | expected assertion |
|---|---|---|
| `NatPortMappingEnsureVerifier` (proposed) | fake COM collection | matching internal port/client/TCP is recognized, absent mapping is added with the selected key, and repeated ensure does not duplicate it |
| `NatPortMappingOwnershipVerifier` (proposed) | fake existing mappings and release policy | only mappings owned by the current server session are released; unrelated mappings remain untouched |
| `NatPortMappingFailureVerifier` (proposed) | activation/enumeration/Add/Remove failures | unavailable, explicit failure, cancellation, and post-operation unknown outcomes are distinct and retry is bounded |
| `NatPortMappingLifecycleVerifier` (proposed) | server start/stop harness | ensure follows listener startup, cleanup covers stop and failed startup, and COM references are released by the Adapter |
| `NatPortMappingComponentVerifier` (proposed) | schema/static scan | no COM interface, collection, enumerator, exception, or lazy external query appears in a Component; optional status snapshot is non-authoritative |

## 14. Evidence gap and blocking decision

- `evidence-gap`: P15 first-round report is absent; Version4 generated/trimmed bodies leave concrete provider registration, IPC asynchronous behavior, Workshop response ownership, resource-pack disposal, and shutdown cleanup partly unconfirmed.
- `evidence-gap`: the Version4 Main path proves keep-awake call ordering for the observed paths, but not every client exit/error path or whether a failed native call is logged/handled elsewhere.
- `blocking-decision`: runtime/session entity owner, provider registry lifetime, cross-subsystem network/persistence/UI contracts, and final System order must be decided by integration review. This document deliberately does not choose among host service, world singleton, or session entity ownership.

## 15. Checkpoint handoff

checkpointStatus: explicitly-rebound-and-reverified
completedComponents: reverified carried-forward MainPlatformExecutionBoundaryAdapter; reverified carried-forward SocialProviderRegistryBoundary; reverified carried-forward WeGameIpcTransportBoundary; reverified carried-forward WorkshopJoinBoundary; reverified carried-forward ResourcePackBoundaryAdapter; reverified carried-forward CryptoBoundaryAdapter; reverified carried-forward NatPortMappingBoundaryAdapter (focused verifier passed; no new Component file was authorized or added)
currentComponent: completed; evidence and integration review remain
pendingComponents: none (focused source verifier passed; broader host/provider/compatibility evidence remains open)
lastCheckpointUtc: 2026-09-12T07:29:40.951Z
executionStatus: completed
implementationStatus: completed
verificationStatus: passed (focused src2 verifier re-run under current session)

implementationFiles:
- src2/ExternalPlatformBoundaries/RuntimeComposition/Platform/IPlatformExecutionStatePort.cs
- src2/ExternalPlatformBoundaries/RuntimeComposition/Platform/MainPlatformExecutionBoundaryAdapter.cs
- src2/ExternalPlatformBoundaries/RuntimeComposition/Platform/PlatformExecutionLifecycleSystem.cs
- src2/ExternalPlatformBoundaries/RuntimeComposition/Platform/PlatformExecutionResult.cs
- src2/ExternalPlatformBoundaries/RuntimeComposition/Platform/PlatformExecutionSnapshot.cs
- src2/ExternalPlatformBoundaries/RuntimeComposition/Platform/WindowsPlatformExecutionStatePort.cs
- src2/ExternalPlatformBoundaries/Terraria.ExternalPlatformBoundaries.csproj
- src2/ExternalPlatformBoundariesVerification/Program.cs
- src2/ExternalPlatformBoundariesVerification/Terraria.ExternalPlatformBoundariesVerification.csproj
- src2/ExternalPlatformBoundaries/Social/Provider/SocialProviderMode.cs
- src2/ExternalPlatformBoundaries/Social/Provider/SocialProviderCapabilities.cs
- src2/ExternalPlatformBoundaries/Social/Provider/SocialProviderLifecycleResult.cs
- src2/ExternalPlatformBoundaries/Social/Provider/ISocialProviderModule.cs
- src2/ExternalPlatformBoundaries/Social/Provider/ISocialJoinRequestTickSource.cs
- src2/ExternalPlatformBoundaries/Social/Provider/SocialProviderRegistrySnapshot.cs
- src2/ExternalPlatformBoundaries/Social/Provider/ISocialProviderRegistryPort.cs
- src2/ExternalPlatformBoundaries/Social/Provider/SocialProviderRegistryAdapter.cs
- src2/ExternalPlatformBoundaries/Social/Provider/SocialRegistryLifecycleSystem.cs
- src2/ExternalPlatformBoundaries/Social/Provider/SocialModeQuery.cs
- src2/ExternalPlatformBoundaries/Social/WeGame/WeGameIpcTransportOptions.cs
- src2/ExternalPlatformBoundaries/Social/WeGame/IWeGameIpcPipe.cs
- src2/ExternalPlatformBoundaries/Social/WeGame/WeGameIpcFrame.cs
- src2/ExternalPlatformBoundaries/Social/WeGame/IpcTransportResult.cs
- src2/ExternalPlatformBoundaries/Social/WeGame/WeGameIpcTransportStatus.cs
- src2/ExternalPlatformBoundaries/Social/WeGame/IWeGameIpcTransportPort.cs
- src2/ExternalPlatformBoundaries/Social/WeGame/WeGameIpcTransportAdapter.cs
- src2/ExternalPlatformBoundaries/Social/WeGame/IpcTransportLifecycleSystem.cs
- src2/ExternalPlatformBoundaries/Workshop/WorkshopPublicity.cs
- src2/ExternalPlatformBoundaries/Workshop/WorkshopOperationStatus.cs
- src2/ExternalPlatformBoundaries/Workshop/WorkshopOperationResult.cs
- src2/ExternalPlatformBoundaries/Workshop/WorkshopTagValue.cs
- src2/ExternalPlatformBoundaries/Workshop/WorkshopEntrySnapshot.cs
- src2/ExternalPlatformBoundaries/Workshop/WorkshopPublishRequest.cs
- src2/ExternalPlatformBoundaries/Workshop/WorkshopLookupRequest.cs
- src2/ExternalPlatformBoundaries/Workshop/IWorkshopProviderPort.cs
- src2/ExternalPlatformBoundaries/Workshop/WorkshopBoundaryAdapter.cs
- src2/ExternalPlatformBoundaries/Workshop/CloudCapabilitySnapshot.cs
- src2/ExternalPlatformBoundaries/Workshop/CloudCapabilityProjection.cs
- src2/ExternalPlatformBoundaries/Workshop/JoinRequestSnapshot.cs
- src2/ExternalPlatformBoundaries/Workshop/JoinRequestInboxAdapter.cs
- src2/ExternalPlatformBoundaries/Workshop/RichPresenceGameMode.cs
- src2/ExternalPlatformBoundaries/Workshop/RichPresenceGameSnapshot.cs
- src2/ExternalPlatformBoundaries/Workshop/RichPresenceQuery.cs
- src2/ExternalPlatformBoundaries/Workshop/WorkshopIssueReportSnapshot.cs
- src2/ExternalPlatformBoundaries/Workshop/WorkshopIssueReportStore.cs
- src2/ExternalPlatformBoundaries/ResourcePacks/ResourcePackBranding.cs
- src2/ExternalPlatformBoundaries/ResourcePacks/ResourcePackLoadStatus.cs
- src2/ExternalPlatformBoundaries/ResourcePacks/ResourcePackMetadataSnapshot.cs
- src2/ExternalPlatformBoundaries/ResourcePacks/ResourcePackLoadResult.cs
- src2/ExternalPlatformBoundaries/ResourcePacks/ResourcePackBoundaryAdapter.cs
- src2/ExternalPlatformBoundaries/ResourcePacks/ResourcePackCandidate.cs
- src2/ExternalPlatformBoundaries/ResourcePacks/ResourcePackDiscoveryResult.cs
- src2/ExternalPlatformBoundaries/ResourcePacks/ResourcePackCollectionAdapter.cs
- src2/ExternalPlatformBoundaries/Cryptography/ISecretDerivationPrimitive.cs
- src2/ExternalPlatformBoundaries/Cryptography/ISecretDerivationPort.cs
- src2/ExternalPlatformBoundaries/Cryptography/SecretDerivationAdapter.cs
- src2/ExternalPlatformBoundaries/Nat/NatPortMappingStatus.cs
- src2/ExternalPlatformBoundaries/Nat/NatPortMappingKey.cs
- src2/ExternalPlatformBoundaries/Nat/IStaticPortMappingSnapshot.cs
- src2/ExternalPlatformBoundaries/Nat/INatPortMappingCollectionPort.cs
- src2/ExternalPlatformBoundaries/Nat/NatPortMappingResult.cs
- src2/ExternalPlatformBoundaries/Nat/INatPortMappingPort.cs
- src2/ExternalPlatformBoundaries/Nat/NatPortMappingAdapter.cs
- src2/ExternalPlatformBoundaries/Nat/NatPortMappingLifecycleSystem.cs

verificationEvidence:
- command: pwsh -NoProfile -Command "$dotnetArgs = @('build', '.\src2\ExternalPlatformBoundariesVerification\Terraria.ExternalPlatformBoundariesVerification.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false'); & .\Build\Tools\Invoke-SerialDotnet.ps1 @dotnetArgs"
  project: src2/ExternalPlatformBoundariesVerification/Terraria.ExternalPlatformBoundariesVerification.csproj
  exitCode: 0
  warnings: 0
  errors: 0
  output: Build/bin/Terraria.ExternalPlatformBoundaries/Debug/net10.0/Terraria.ExternalPlatformBoundaries.dll; Build/bin/Terraria.ExternalPlatformBoundariesVerification/Debug/net10.0/Terraria.ExternalPlatformBoundariesVerification.dll
- command: pwsh -NoProfile -Command "$dotnetArgs = @('run', '--project', '.\src2\ExternalPlatformBoundariesVerification\Terraria.ExternalPlatformBoundariesVerification.csproj', '--no-build', '--no-restore'); & .\Build\Tools\Invoke-SerialDotnet.ps1 @dotnetArgs"
  project: src2/ExternalPlatformBoundariesVerification/Terraria.ExternalPlatformBoundariesVerification.csproj
  exitCode: 0
  output: P15 external-platform boundary verifier passed.
- command: pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments "build .\src2\ExternalPlatformBoundariesVerification\Terraria.ExternalPlatformBoundariesVerification.csproj -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false"
  project: src2/ExternalPlatformBoundariesVerification/Terraria.ExternalPlatformBoundariesVerification.csproj
  exitCode: 0
  warnings: 0
  errors: 0
  output: Build/bin/Terraria.ExternalPlatformBoundaries/Debug/net10.0/Terraria.ExternalPlatformBoundaries.dll; Build/bin/Terraria.ExternalPlatformBoundariesVerification/Debug/net10.0/Terraria.ExternalPlatformBoundariesVerification.dll
- command: pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments "run --project .\src2\ExternalPlatformBoundariesVerification\Terraria.ExternalPlatformBoundariesVerification.csproj --no-build --no-restore"
  project: src2/ExternalPlatformBoundariesVerification/Terraria.ExternalPlatformBoundariesVerification.csproj
  exitCode: 0
  output: P15 external-platform boundary verifier passed.
- scope: focused verifier passed for platform lease results, social lifecycle order/cleanup, IPC framing/UTF-8/close, Workshop defensive snapshots, Join generation/expiry, Rich Presence derivation, directory/zip ResourcePack metadata, ordered two-pass secret transform, and NAT matching/ownership.
- not proven: real provider registration, host lifecycle wiring, real PipeStream/COM/asset services, complete BCrypt raw primitive compatibility, production registration, behavior equivalence, network/persistence closure, and cross-partition ownership.
