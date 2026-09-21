# Version4 非权威组件第二轮设计：P13 网络协议与会话

partitionId: P13
sessionId: 02b27d497bf647e8879091412c7fa91d
previousSessionId: 26e2f78c2b1c4eca8488af170b54fe72
staleDocumentSessionId: 6a5c8da4872840ce90e0326dad3647ba
handoffId: manual-handoff-P13-e5ae98903e004eaf8a0f52e4dcd9982d
handoffStatus: handed-off
inputReport: D:\TRbackup\NLTX\docs\migration\ledgers\non-authoritative-component-partitions\13-network-protocol-session.md
componentDesignPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\non-authoritative\2026-09-11-version4-non-authoritative-P13-network-protocol-session-component-design.md
componentExecutionPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\non-authoritative\2026-09-11-version4-non-authoritative-P13-network-protocol-session-component-execution.md
designStatus: proposed
executionStatus: completed (src2-only Component implementation checkpoint)
implementationStatus: completed
verificationStatus: partial
completedComponents:
- Proposed NetworkSessionEndpointState
- Proposed NetworkMapPresentationState
- Proposed NetworkSessionModeState
- Proposed NetworkSessionConfigurationState
- Proposed RemoteEndpointValue
- Proposed NetworkBufferPoolAdapter
- Proposed NetworkMessageBufferAdapterState
- Proposed InboundNetworkCommand
- Proposed NetworkPacketEnvelope
- Proposed RemoteClientRateLimitState
- Proposed RemoteServerConnectionState
- Proposed RemoteClientIdentityAndLifecycleState
- Proposed RemoteClientSectionObservationState
- Proposed WorldSectionStreamingState
- Proposed ActiveSectionObservationState
- Proposed ChatMonitorStateComponent (bounded message-text snapshot state only)
currentComponent: none (remaining proposed boundaries are explicitly deferred)
pendingComponents:
- Proposed RemoteClientStatusProjection (deferred)
- Proposed RemoteClientReceiveState (deferred)
- Proposed NetworkPublicationState (deferred: serialization scratch and connection-buffer ownership require an excluded Adapter/Projection boundary)
- Proposed NetworkSoundProjection (deferred: outbound Projection boundary)
- Proposed NetworkSerializationContext (deferred: per-publication lifecycle requires an excluded Projection/System boundary)
- Proposed ContentNetworkProjection (deferred)
- Proposed ChatCommandRegistryState (deferred: registry behavior is excluded)
- Proposed ChatMessageCommand (deferred: command behavior is excluded)
- Proposed ChatCommandAdapter (deferred: adapter/content behavior is excluded)
- Proposed ChatColorCatalog (deferred: presentation catalog, not an independent Component)
- Proposed ChatPublicationAdapter (deferred: publication effects are excluded)
- Proposed ChatSnippetPresentationState (deferred: external content/UI references require an excluded Adapter/Projection boundary)
- ChatMonitorState container/layout, expiry, queue mutation and monitor writer behavior remain deferred to excluded Presentation/System boundaries
- Integration review and evidence closure
lastCheckpointUtc: 2026-09-12T11:11:05.258Z
src2CodeModified: yes
productionSrcModified: no
verificationCommand: pwsh -NoProfile -Command "& '.\\Build\\Tools\\Invoke-SerialDotnet.ps1' -DotnetArguments @('build', '.\\src2\\Network\\Terraria.NetworkProtocolSession.csproj', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"
verificationResult: exitCode=0; warnings=0; errors=0; artifact=Build/bin/Terraria.NetworkProtocolSession/Debug/net10.0/Terraria.NetworkProtocolSession.dll
focusedVerifierBuildCommand: pwsh -NoProfile -Command "& '.\\Build\\Tools\\Invoke-SerialDotnet.ps1' -DotnetArguments @('build', '.\\src2\\NetworkProtocolSessionVerification\\Terraria.NetworkProtocolSessionVerification.csproj', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"
focusedVerifierBuildResult: exitCode=1; warnings=0; errors=1; CS0246 at src2/NetworkProtocolSessionVerification/Program.cs:336 for intentionally deferred NetworkModuleRegistryState
evidence-gap:
- The first-round P13 research report named by the P13 prompt is absent; the validated input inventory and direct evidence were used instead.
- The prior document recorded session `6a5c8da4872840ce90e0326dad3647ba`, but the runner ledger identifies `26e2f78c2b1c4eca8488af170b54fe72` as the handed-off owner; the former is retained only as `staleDocumentSessionId`.
- Core transport-chain evidence is confirmed, but the complete 253-member reader/writer/lifecycle graph has not been mechanically reconstructed.
- Reduced or stubbed Version4 methods leave behavior, exception, cleanup, and compatibility semantics partial.
- NetworkSessionEndpointState, NetworkMapPresentationState, NetworkSessionModeState, NetworkSessionConfigurationState, RemoteEndpointValue, NetworkBufferPoolAdapter, NetworkMessageBufferAdapterState, InboundNetworkCommand, NetworkPacketEnvelope, RemoteClientRateLimitStateComponent, RemoteServerConnectionStateComponent, RemoteClientIdentityAndLifecycleStateComponent, RemoteClientSectionObservationStateComponent, WorldSectionStreamingStateComponent, ActiveSectionObservationStateComponent, and ChatMonitorStateComponent are implemented under src2; ChatMonitorStateComponent contains only bounded message-text snapshots and the last width-limit value. Section mutation/query behavior, cursor state, rate-limit mutation, socket ownership, connection lifecycle effects, publication scratch ownership, status projection, chat container/layout state, expiry, queue mutation and monitor writer behavior remain outside this Component-only slice.
- NetworkModuleRegistryState remains deferred because its proposed registration and dispatch behavior requires a non-Component adapter/registry boundary, which this session is forbidden to implement.
- The current NetworkProtocolSessionVerification source build exits `1` with `CS0246` at `src2/NetworkProtocolSessionVerification/Program.cs:336` because `NetworkModuleRegistryState` is intentionally deferred; no verifier source was changed. A prior `run --no-build --no-restore` invocation reused an existing artifact and printed `P13 verifier passed.`, but it is not current-source rebuild evidence. No protocol compatibility or behavior-equivalence claim is established.
blocking-decision:
- The final owner of NetworkId, section references, entity/player or compatibility-slot identity, packet schema versions, and cross-domain snapshot ordering remains integration-review.
- The network-thread to simulation-thread commit mechanism and per-message delivery, retry, and cleanup semantics remain integration-review decisions.
- Legacy socket/thread integration and secret/configuration exposure remain outside this implemented slice until focused acceptance evidence exists.
- Remote client rate-limit counters have no Component-owned mutation API yet; the rate-limit system, clock, and rejection command remain deferred.
- Remote server socket ownership, receive-buffer ownership, status projection, and lifecycle transitions remain deferred to excluded adapter/system boundaries.
- Remote client socket ownership, player/entity mapping, status projection, and lifecycle transitions remain deferred to excluded adapter/system boundaries.
- Remote client section coordinates, world-section authority, active-window calculation, and section publication remain `crossSubsystemOwner: integration-review` or excluded Query/System/Adapter boundaries.
- World-section cursor traversal, section flag transitions, frame/refresh counters, and loaded-state queries remain excluded System/Query behavior; only the validated grid state is implemented.
- Network publication scratch, serialization context, chat registry/commands, chat publication, snippet presentation, and monitor container/layout, expiry, queue mutation and writer behavior remain deferred because their ownership requires excluded Adapter/System/Projection behavior or external content/UI types. The implemented ChatMonitorStateComponent is only a bounded text-state slice and does not establish monitor behavior equivalence.

## 1. 设计性质和范围

本文件是 P13 的非权威 proposed 组件设计。它把 Version4 的网络会话、消息协议、区段流、socket、packet、聊天协议和客户端表现库存转成后续可审查的 ECS 边界。所有边界的设计身份仍为 `status: proposed`；实现检查点只记录已在 `src2` 中实际创建并验证的候选类型，不表示迁移完成、协议闭合或行为等价。

本分区覆盖输入报告中的 18 个叶子子系统和 253 条成员记录：

- `RuntimeComposition`：`MainRecentServerAndMapState`、`MainNetworkSessionState`。
- `NetworkSessionAndSectionStreaming`：消息缓冲、网络发布、区段流、远端 server/client、IP 请求、限流、会话配置和传输线程。
- `SharedRuntimeMechanisms`：socket、packet 原语、内容网络模块、区段投影、聊天命令、聊天片段和聊天监视器。

排除范围：权威实体、玩家/NPC/物品/Tile/Liquid 的最终业务 owner，持久化格式的最终 owner，客户端渲染系统的最终 owner，其他 P 分区的成员清单，以及任何 C#、csproj、测试或生成物。跨域状态只提出候选并保留 `crossSubsystemOwner: integration-review`。

## 2. 检查点状态

本检查点保留 P13 全部 proposed 边界的设计整理，并记录了当前实现切片。16 个可独立表达的 Component/state 边界已在 `src2` 中创建；其中 `ChatMonitorStateComponent` 仅保存有界消息文本快照和最后宽度限制。网络项目串行构建通过，但 focused verifier 的当前源码重建因故意 deferred 的 `NetworkModuleRegistryState` 在 `Program.cs:336` 失败。此前的 `run --no-build --no-restore` 只复用了旧 verifier 产物，不能作为当前源码树的完整验证；其余边界仍明确 deferred，不表示网络协议已闭合。

| proposed 边界 | 角色 | 主要状态 | 单一写入方向 | 当前证据 |
|---|---|---|---|---|
| `Proposed NetworkSessionEndpointState` | Component | 最近服务器 world/IP/port 固定容量列表 | `RecentServerEndpointCommand` proposed 写入 | Version4 `Main.cs:430-436`，confirmed declaration, partial lifecycle |
| `Proposed NetworkMapPresentationState` | Component/Projection | 背景过渡、地图刷新/准备和 map timer | `MapPresentationSystem` proposed 写入；只向 UI projection 输出 | Version4 `Main.cs:438-464`，confirmed declaration, partial readers |
| `Proposed NetworkSessionModeState` | Component | 当前/目标网络模式和 pending transition | `NetworkModeTransitionCommand` proposed 提交 | Version4 `Main.cs:1161-1178`，confirmed declaration, partial ordering |
| `Proposed NetworkSessionConfigurationState` | Component/Adapter state | 监听端口、上限、密码、endpoint、UPNP 和退出策略 | `NetworkConfigurationAdapter` proposed 初始化 | Version4 `Netplay.cs:31-83`，confirmed declaration, lifecycle partial |
| `Proposed NetworkBufferPoolAdapter` | Adapter | 256/1024/16384 标准桶和一次性 custom lease | `NetworkBufferPoolAdapter.Rent/Return` 单一资源 owner | Version4 `LegacyNetBufferPool.cs:8-64`，bucket behavior independently verified, shutdown semantics partial |
| `Proposed NetworkMessageBufferAdapterState` | Adapter state | 有界 read/write byte buffer、帧长度游标、check marker、connection slot 和 write lock | `TryAppendReadBytes`、`TryConsumeReadFrame`、`TryWriteBytes` 与显式 write lease | Version4 `MessageBuffer.cs:29-67`、`NetMessage.cs:2230-2308`；bounds/cursor/lock behavior verified, decode/resource semantics partial |
| `Proposed InboundNetworkCommand` | Command | connection slot、message id、protocol version、receive sequence 和 immutable payload snapshot | `InboundNetworkCommand` constructor owns metadata validation and payload copy | Version4 `MessageBuffer.GetData` dispatch boundary；metadata/payload isolation verified, per-message decode/permission semantics partial |

## 3. Version4 事实和证据分层

### 3.1 已读取的 Version4 证据

| 来源 | 实际位置 | 读取到的事实 | 读者/写者和生命周期 | evidenceStatus |
|---|---|---|---|---|
| Version4 | `D:\TRbackup\Version4\Terraria\Main.cs:430-464`，`Terraria.Main` | `maxMP` 控制最近服务器数组容量；`recentWorld`、`recentIP`、`recentPort` 成组保存最近连接入口；`instantBGTransitionCounter`、`bgDelay`、`bgStyle`、两组背景 alpha 是表现过渡状态；`refreshMap`、`mapReady`、`updateMap`、`mapTimeMax`、`mapTime`、`clearMap` 是地图刷新/计时状态。 | 初始化在静态字段声明；具体 UI、存档和清理写者未完整闭合。最近 server 是客户端入口状态，不是权威世界状态。 | confirmed declaration, partial lifecycle |
| Version4 | `D:\TRbackup\Version4\Terraria\Main.cs:1161-1178`，`Terraria.Main` | `getIP`、`menuMultiplayer`、`menuServer`、`netMode`、`_targetNetMode`、`_hasPendingNetmodeChange`、`netPlayCounter`、`lastItemUpdate`、`maxItemUpdates` 形成运行时网络模式和更新节流状态。 | `Netplay.InitializeServer` 会设置 `Main.netMode=2`；模式切换调用链和 Main-thread 提交点仍需整合确认。 | confirmed declaration, partial writer graph |
| Version4 | `D:\TRbackup\Version4\Terraria\Netplay.cs:31-83,247-355,470-504,566-590` | 会话配置与连接槽、`RemoteServer`、TCP listener、server thread、UPNP 映射、UDP broadcast 和 `MessageBuffer fullBuffer` 集中在静态 Netplay 运行时；server loop 更新客户端，主线程检查完整消息。 | `StartServer` 初始化、监听、启动后台线程；`UpdateConnectedClients` 处理断开和 reset；`UpdateInMainThread` 调用 `NetMessage.CheckBytes`。网络线程和模拟线程边界是关键 integration decision。 | confirmed call chain, partial failure/stop semantics |
| Version4 | `D:\TRbackup\Version4\Terraria\RemoteClient.cs:9-63,67-119,138-205,241-377` | 远端 client 同时拥有 socket、slot/name/state/status、section observation、read buffer 和四类 spam counter；`TryRead` 发起异步接收，回调调用 `NetMessage.ReceiveBytes`，`Reset` 清空 section、buffer、player、socket 和 status。 | `Netplay.UpdateConnectedClients` 驱动 `Update`；`PendingTermination` 经批准后 reset；连接丢失、回调竞态和清理顺序需 focused verifier。 | confirmed declaration and lifecycle path, partial concurrency |
| Version4 | `D:\TRbackup\Version4\Terraria\MessageBuffer.cs:29-67,71-107,125-3365` | 固定大小 read/write byte buffer、reader/writer stream、`messageLength`/`totalData` 游标、`whoAmI`、spam/check flag、临时 AI 数组和 packet history 共存；`GetData` 按 message id 解码并可能直接写入 Player/World 状态或发送回包。 | `NetMessage.ReceiveBytes` 填充 buffer，`CheckBytes` 在主线程调用 `GetData`；此类必须拆成 adapter buffer、decode scratch、inbound command 和 diagnostics projection。 | confirmed declaration and dispatch path, partial per-message semantics |
| Version4 | `D:\TRbackup\Version4\Terraria\NetMessage.cs:24-78,82-2230,2230-2259,2367-2460` | `NetSoundInfo` 是短期声音 payload；`buffer[257]` 是按连接/服务端槽位的 MessageBuffer；compress arrays、death reason、sound info、revenge marker 是发送临时状态；`ReceiveBytes` 与 `CheckBytes` 连接网络字节和主线程解码。 | `SendData` 序列化大量实体/Tile/世界快照；`SendSection` 和 tile methods 输出投影；共享静态压缩 scratch 的线程安全和重入性需 integration-review。 | confirmed declaration, partial thread safety |
| Version4 | `D:\TRbackup\Version4\Terraria.Net\NetManager.cs:7-143`、`NetPacket.cs:7-59` | `NetManager` 按 ushort module id 注册/读取模块并提供 broadcast/send；`NetPacket` 以长度、标记和 module id 组成包头，使用 `CachedBuffer`，发送前 shrink，发送后 recycle。 | `SendData` 异步交给 `ISocket`，异常被吞掉；packet 回收责任由 Send/Broadcast 边界承担。包生命周期不能进入权威 ECS state。 | confirmed declaration and send path, partial failure semantics |
| Version4 | `D:\TRbackup\Version4\Terraria.Net.Sockets\TcpSocket.cs:10-205`、`DebugNetworkStream.cs:10-219` | TCP socket 封装 connect/listen/read/write/remote address；debug stream 用 incoming/outgoing queue、latency、DateTime 和 async result 模拟网络延迟。非 Windows 发送会从 legacy buffer pool 复制并在回调返还。 | listener 和 server loop 使用后台线程；close、disposed、read/write exception 由 adapter 处理，不能由 ECS component 直接持有第三方 stream。 | confirmed transport boundary, partial cancellation/close |
| Version4 | `D:\TRbackup\Version4\Terraria\WorldSections.cs:6-79`，完整参考 `D:\TRbackup\无任何删减通过编译\Terraria\WorldSections.cs:6-380` | Version4 checkout 只保留 section dimensions/data/map cursor 的最小实现；完整参考补足 loaded/framed/map-drawn/refresh bit、frame counters、map iterator 和 bounds checks。 | section 状态由世界/地图处理路径写入，网络 client section observation 是另一份 per-client 状态；不能合并。 | confirmed source split, version-drift/implementation gap |
| Version4 | `D:\TRbackup\Version4\Terraria.DataStructures\ActiveSections.cs:6-58` | `LastActiveTime` 以 section coordinate 和 `Main.GameUpdateCount` 计算 active window；`CheckSection` 更新时间并发出 `SectionActivated`。 | 读取 world time 和 section geometry，写共享 activity grid/event；时间驱动、事件订阅和 reset 顺序需 integration-review。 | confirmed declaration and method path, partial event readers |
| Version4 | `D:\TRbackup\Version4\Terraria.GameContent.NetModules\NetLiquidModule.cs:8-115`、`NetCreativePowerPermissionsModule.cs:7-25` | liquid module 按 section 聚合 dirty packed tile coordinates，再为每个 connected player 序列化；creative permissions module 发送 power id/level payload。 | 读取 Tile/Liquid/Creative 权威状态并通过 NetManager 发包；这些是 projection/adapter，不是网络组件中的权威模拟状态。 | confirmed serialization boundary, deserialize behavior partial |
| Version4 | `D:\TRbackup\Version4\Terraria.Chat\ChatCommandProcessor.cs:9-77`、`ChatMessage.cs:8-28`、`ChatHelper.cs:11-90` | 命令注册、localized command/alias、default command、incoming/outgoing message 和 server-to-client/broadcast chat 是分离 API；`ChatMessage` 具有 command id、text、consumed state。 | `ChatHelper` 负责网络发布和本地显示；命令 processor 读 localization/command handlers；消息消费写者和失败/权限边界仍需闭合。 | confirmed public boundary, partial command semantics |
| Version4 | `D:\TRbackup\Version4\Terraria.UI.Chat\ChatManager.cs:16-157`、`ChatMessageContainer.cs:7-40`、`TextSnippet.cs:6-47`、`PositionedSnippet.cs:6-25` | chat tag registry/regex、parse/layout/size calculation、message container preparation/time-left、text/snippet hover/click/position state 属于 client presentation pipeline。 | UI update loop 更新 container；parse/layout 应保持纯 Query；hover/click 是 presentation effect port。 | confirmed declaration and pipeline, partial rendering effects |

### 3.2 完整参考、tModLoader 和 SS14 交叉证据

| 来源 | 实际位置和查询 | 用途与结论 | evidenceStatus |
|---|---|---|---|
| 完整参考源码 | `D:\TRbackup\无任何删减通过编译\Terraria\WorldSections.cs:49-380` | 只补足 Version4 已存在 `WorldSections` 的状态位和迭代实现，不把完整参考独有文件当作 Version4 事实。 | confirmed supplement |
| tModLoader stable mirror | `D:\TRbackup\tmodloader-api-docs-stable\index.html`，首页标题 `tModLoader Documentation`，版本 `v2026.07` | 版本基线已确认。 | confirmed |
| tModLoader stable mirror | `D:\TRbackup\tmodloader-api-docs-stable\class_main.html#a5b7c21c478b58b72ea5c4e6921e940bd` 与 `#a471ec73217cd5bd5d290d5196fd639d2` | `Main.netMode` 的公开语义为 0 single-player client、1 multiplayer client、2 server；`dedServ` 等价于 server mode。只用于公开 boundary 交叉验证。 | confirmed public API |
| tModLoader stable mirror | `D:\TRbackup\tmodloader-api-docs-stable\class_chat_helper.html#a7bf3083fca81823cad87fbce6f3686d5`、`#a46db2b94f0d1fc88fe05baf931ea17f5` | `ChatHelper` 的 broadcast/send-to-client 是 server-to-client chat publication boundary。 | confirmed public API |
| tModLoader stable mirror | `D:\TRbackup\tmodloader-api-docs-stable\class_chat_command_processor.html#a673ff9303ae9281ea196ca8537fd0e0b`、`#a21494ab137aad11905142d30ac588342`、`#a525c1e21d8420a3e589023d5b22fa5af` | command registration、outgoing creation 和 incoming processing 是可分隔的协议接口。 | confirmed public API |
| tModLoader stable mirror | `D:\TRbackup\tmodloader-api-docs-stable\struct_net_packet.html`、`class_netmode_i_d.html`、`class_net_manager.html` | packet/module/net mode 公开类型存在，但私有线程和写入者仍以 Version4 为准。 | partial cross-check |
| SS14 ECS reference | `C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Shared\Chat\SharedChatSystem.cs:20-52,148-186` | `EntitySystem` 通过显式 `INetManager` 依赖、事件和纯消息处理边界组织聊天；只参考结构粒度，不复制领域语义。 | confirmed structural reference |
| SS14 ECS reference | `C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Shared\Chat\MsgChatMessage.cs:11-90` | 网络 chat message 将 sender 的 `NetEntity`、消息内容和序列化 `NetMessage` 分开；支持本分区把 inbound command 与 outbound projection 分开。 | confirmed structural reference |

### 3.3 当前 NLTX 状态

当前 NLTX 的 `src2` 已实现 P13 会话基础、buffer-pool adapter 和 bounded message-buffer adapter state；网络 socket、packet、消息 decode、远端连接、区段流、projection、chat protocol 和 chat monitor 的其余对应运行时边界仍未实现。已读取的相关基础状态是：

- `src/WorldStorage/WorldSectionState.cs:3-22` 已有 `WorldSectionState`，保存 section flags、尺寸、map remaining 和 revision，但尚未实现 Version4 的完整加载/绘制/refresh 行为。
- `src/WorldStorage/SectionIterationState.cs:3-11` 已有值语义的 section iteration cursor。
- `src/Share/Entity/Components/NetworkEntityId.cs:3-8` 已有 `NetworkEntityId` 值类型，`None=-1`，但它的跨分区最终 owner 未裁决。
- `src/Share/Entity/Components/EntityIdentityState.cs:5-59` 已有标注为 proposed 的 runtime/compatibility/network/persistent identity 分离草案；不能直接视为 P13 网络协议实现。
- `src/WorldStorage/Terraria.WorldStorage.csproj:3,8` 和 `src/Share/Entity/Terraria.EntityEcs.csproj:3,8` 均 target `net10.0`，但当前没有 P13 网络项目或测试项目。

## 4. Proposed 组件设计

### 4.1 会话端点、模式、配置和地图表现

所有路径和类型仅为 proposed，不能理解为当前文件已经存在。

#### `Proposed NetworkSessionEndpointState`

Suggested path: `src2/Network/Session/NetworkSessionEndpointStateComponent.cs` (`status: proposed`)

职责：保存最近连接入口的有限容量客户端表现状态。字段范围是 `recentWorld`、`recentIP`、`recentPort` 和其容量 `maxMP`。它不拥有当前 socket、当前 server connection、world persistence ID 或 network entity ID。不变量是三个数组使用同一 slot、容量一致、空 slot 不被当成 active connection。

读者：连接菜单/客户端 projection。写者：`Proposed RecentServerEndpointCommand` 单一写入 owner。初始化：会话启动时按固定容量初始化；清理：新 profile/session 时清空。副作用：若未来持久化最近 server，必须由独立 `Proposed RecentServerEndpointPersistenceAdapter` 执行，Component 不直接写文件。

#### `Proposed NetworkMapPresentationState`

Suggested path: `src2/Network/Presentation/NetworkMapPresentationStateComponent.cs` (`status: proposed`)

职责：保存 `instantBGTransitionCounter`、`bgDelay`、`bgStyle`、`bgAlphaFrontLayer`、`bgAlphaFarBackLayer`、`wofNPCIndex`、`wofDrawAreaTop`、`wofDrawAreaBottom`、`refreshMap`、`mapReady`、`updateMap`、`mapTimeMax`、`mapTime`、`clearMap`。这些是客户端表现、地图刷新和短期 derived/cache 状态，不是可网络同步的权威 world component。`wofNPCIndex` 与 entity/NPC owner 跨分区，使用 `crossSubsystemOwner: integration-review`。

读者：Map/UI projection 和地图刷新 Query。写者：`Proposed MapPresentationSystem` 和显式 `MapRefreshCommand`。时间输入必须显式传入，不能由 Query 隐式读系统时钟；更新区域的 Rectangle 必须是 value snapshot，不反向修改 Tile authority。

#### `Proposed NetworkSessionModeState`

Suggested path: `src2/Network/Session/NetworkSessionModeStateComponent.cs` (`status: proposed`)

职责：承载 `getIP`、`menuMultiplayer`、`menuServer`、`netMode`、`_targetNetMode`、`_hasPendingNetmodeChange`、`netPlayCounter`、`lastItemUpdate`、`maxItemUpdates`。`netMode` 是 session mode，而非 EntityId；公开模式值应采用稳定的 proposed enum/value object，最终与 `Main.netMode`/`NetmodeID` 的整合由 `integration-review` 决定。

写入规则：UI/启动入口只提交 `Proposed NetworkModeChangeCommand`；`Proposed NetworkSessionTransitionSystem` 验证 mode、初始化/关闭 adapter，并在单一提交点改变 current mode。`lastItemUpdate/maxItemUpdates` 属于节流快照，不能和会话身份混合持久化。

#### `Proposed NetworkSessionConfigurationState`

Suggested path: `src2/Network/Session/NetworkSessionConfigurationStateComponent.cs` (`status: proposed`)

字段映射：`MaxConnections`、`NetBufferSize`、`DefaultPort`、`BanFilePath`、`ServerPassword`、`ServerIP`、`ServerIPText`、`IsHostAndPlay`、`HostToken`、`UseUPNP`、`SaveOnServerExit`、`HandshakeLoggingEnabled`。配置与外部地址/secret 不应成为 Entity component；建议它作为 session aggregate 的 adapter-owned state，只有经过 `NetworkConfigurationAdapter` 才接触 DNS、UPNP、环境启动参数或文件。

不变量：连接上限、缓冲上限和监听端口在 listener 启动前冻结；`ServerIP` 是解析后的 endpoint，`ServerIPText` 是用户输入/显示文本，两者不可互相覆盖；`ServerPassword`/`HostToken` 不进入客户端 projection 或日志；handshake logging 只影响诊断，不推进业务状态。

### 4.2 MessageBuffer、解码和 inbound command

#### `Proposed NetworkMessageBufferAdapterState`

Suggested path: `src2/Network/Protocol/NetworkMessageBufferAdapterState.cs` (`status: proposed`)

成员映射：`readBufferMax`、`writeBufferMax` -> `Proposed NetworkBufferLimits`; `readBuffer`、`writeBuffer`、`readerStream`、`writerStream`、`reader`、`writer` -> adapter-owned transient resources; `messageLength`、`totalData`、`RemainingReadBufferLength` -> `Proposed MessageFrameCursorState`/`Proposed MessageBufferQuery`; `writeLocked` -> outbound serialization lease; `broadcast` -> publication routing intent; `whoAmI` -> `Proposed NetworkConnectionSlot` candidate, not EntityId；`checkBytes` -> main-thread drain marker；`History` -> diagnostics projection/cache；`_temporaryProjectileAI`、`_temporaryNPCAI` -> decode scratch only; `spamCount`、`maxSpam` -> compatibility rate accounting, final owner integration-review。

Adapter 负责收集 byte stream、检查 frame length、创建 bounded reader/writer、释放 MemoryStream/BinaryReader/BinaryWriter，并把 decode result 交给显式 `Proposed InboundNetworkCommandQueue`。它不能直接写 `Player`、NPC、Tile、Liquid 或 world state。`GetData` 中当前 Version4 的 per-message switch 需要分批转换为 command handlers，每个 handler 标出 server authority、client prediction、权限和 idempotency。

#### `Proposed InboundNetworkCommand`

Suggested path: `src2/Network/Protocol/InboundNetworkCommand.cs` (`status: proposed`)

Command payload 至少包含 connection slot、protocol message id、schema/version、payload snapshot 和 receive sequence；不能把 `BinaryReader`、socket 或第三方对象带入模拟层。`Proposed NetworkDecodeSystem` 只解码和校验，`Proposed NetworkCommandCommitSystem` 在模拟线程按明确顺序提交。未知 message、短 frame、非法 slot、重复 sequence 和权限拒绝都应生成可观察结果，不得静默改变权威状态。

### 4.3 Packet、module、buffer pool 和 endpoint 原语

#### `Proposed NetworkPacketEnvelope`

Suggested path: `src2/Network/Protocol/NetworkPacketEnvelope.cs` (`status: implemented under src2; boundary remains proposed`)

成员映射：`HEADER_SIZE`、`Id`、`Buffer`、`Length`、`Writer`、`Reader`。Envelope 是短期 packet resource/value boundary，不是长期 ECS component；`Writer`/`Reader` 只在 adapter 内部可见。包头长度、marker、module id 和 payload length 必须在 construction/`ShrinkToFit` 时校验。所有发送/广播路径只能有一个 recycle owner；发送失败时必须记录 buffer 是否已转移给 async adapter，避免 double recycle。

当前实现文件为 `src2/Network/Protocol/NetworkPacketEnvelope.cs`。它通过 `NetworkBufferPoolAdapter` 租借一个有界 lease，写入 5 字节 little-endian header，提供 payload 写入游标、`ShrinkToFit` 和 exactly-once `Recycle`；没有暴露 `BinaryReader`、`BinaryWriter`、socket 或流。历史 packet-envelope slice 先观察到缺失类型的红测，随后由旧 verifier 产物运行得到绿测；当前 verifier 源码重建仍受故意 deferred 的 `NetworkModuleRegistryState` 阻塞，不能据此宣称当前源码树的 verifier 通过。

#### `Proposed NetworkModuleRegistryState` (`deferred`)

Suggested path: `src2/Network/Protocol/NetworkModuleRegistryState.cs` (`status: proposed`)

成员映射：`PacketTypeStorage<T>.Id`、`PacketTypeStorage<T>.Module`、`NetManager.Instance`、`_modules`、`_moduleCount`。该边界需要 module type/id registration、dispatch lookup 和 adapter handoff；这些是本轮明确排除的 Registry/Adapter 行为，因此不创建 `NetworkModuleRegistryState.cs`。module id/version 是协议兼容字段，不能当作 EntityId 或 world revision。

实现决策：`NetworkModuleRegistryState` 记录为 `deferred`，原因是它不能由只含内聚状态、字段、构造函数和不变量的 Component 独立表达；现有 verifier 对 `Register<T>()`、`GetId<T>()`、`TryDispatch(...)` 和 `TryRegister<T>(...)` 的依赖也属于禁止修改的测试/Registry 依赖。继续实现独立的连接和区段状态 Component。

#### `Proposed NetworkBufferPoolAdapter`

Suggested path: `src2/Network/Transport/NetworkBufferPoolAdapter.cs` (`status: proposed`)

成员映射：`SMALL_BUFFER_SIZE`、`MEDIUM_BUFFER_SIZE`、`LARGE_BUFFER_SIZE`、`bufferLock`、三个 queue、三个 bucket count、`_customBufferCount`。pool 的 queue/count 是 adapter cache，所有权必须有租借/返还契约；大小分类和自定义 buffer 不能泄漏到 simulation component。重点 verifier 是 double return、错误 bucket、异常 send 和 process shutdown 排空。

#### `Proposed RemoteEndpointValue`

Suggested path: `src2/Network/Transport/RemoteEndpointValue.cs` (`status: proposed`)

成员映射：`RemoteAddress.Type`、`TcpAddress.Address`、`TcpAddress.Port`，以及 `GetIdentifier`、`GetFriendlyName`、`IsLocalHost` 的行为 seam。IP address 是外部 endpoint value，不是 persistence ID、network entity ID 或 player slot。DNS/loopback 判断必须由 adapter 提供；日志输出需要脱敏策略。

### 4.4 远端 server/client、区段观察和限流

#### `Proposed RemoteServerConnectionState`

Suggested path: `src2/Network/Session/RemoteServerConnectionStateComponent.cs` (`status: proposed`)

成员映射：`Socket` -> transport handle；`IsActive`、`State`、`TimeOutTimer`、`PendingTermination`、`IsReading` -> connection lifecycle；`ReadBuffer` -> receive adapter lease；`StatusText`、`StatusCount`、`StatusMax` -> presentation projection；`ServerSpecialFlags` -> handshake/session capability snapshot。它不直接拥有 `Main.player` 或 world state。

`Proposed RemoteServerLifecycleSystem` 负责 connect/read/timeout/termination state transition；socket close、buffer release 和 client/server projection 在终止路径必须 exactly-once 或明确允许重复。当前 Version4 `RemoteServer` 的行为实现较少，标记 `partial`。

当前 Component 实现：`src2/Network/Session/RemoteServerConnectionStateComponent.cs`。它保存 `IsActive`、`State`、`TimeOutTimer`、`PendingTermination`、`IsReading` 和以 `byte` 表示的 `ServerSpecialFlags` 快照；构造函数拒绝负状态/计时器，并拒绝 inactive/read 状态组合。它不保存 `ISocket`、接收缓冲、状态文本，也不实现生命周期迁移或 special-flag reset。

#### `Proposed RemoteClientIdentityAndLifecycleState`

Suggested path: `src2/Network/Session/RemoteClientIdentityAndLifecycleStateComponent.cs` (`status: proposed`)

成员映射：`Socket`、`Id`、`Name`、`IsActive`、`PendingTermination`、`PendingTerminationApproved`、`IsAnnouncementCompleted`、`State`、`TimeOutTimer`。`Id` 是 connection/player compatibility slot candidate，不是 `NetworkEntityId`；最终 slot-to-player/entity relation `crossSubsystemOwner: integration-review`。

当前 Component 实现：`src2/Network/Session/RemoteClientIdentityAndLifecycleStateComponent.cs`。它保存非负 `CompatibilitySlot`、非空 `Name` 和连接生命周期快照；空白名字会被拒绝，负 slot/state/timeout 会被拒绝。它不保存 socket、player/entity 引用、状态文本，也不实现 reset、read、timeout 或 termination transition。

#### `Proposed RemoteClientStatusProjection`

Suggested path: `src2/Network/Presentation/RemoteClientStatusProjection.cs` (`status: proposed`)

成员映射：`StatusText`、`StatusText2`、`StatusCount`、`StatusMax`。状态文本、百分比和 localized endpoint/name 只能是 projection；不能让 UI 写回 connection state。`UpdateStatusText` 的 socket/address 读取是 adapter boundary，异常应转换为 status error/termination command。

#### `Proposed RemoteClientSectionObservationState`

Suggested path: `src2/Network/Section/RemoteClientSectionObservationStateComponent.cs` (`status: proposed`)

成员映射：`TileSections`、`TileSectionsCheckTime`、`CheckingSections`、`SectionRange`、`IsSectionActive`、`ReadBufferFull` 相关 section/read capacity decision。它表示每个 remote client 对 world section 的观察/传输资格，不表示 world section loaded authority。section coordinate 和 player slot 使用 `crossSubsystemOwner: integration-review`。

当前 Component 实现：`src2/Network/Section/RemoteClientSectionObservationStateComponent.cs`。它保存固定尺寸的 `TileSections`、`TileSectionsCheckTime` 和 `CheckingSections` 状态，并在构造时验证两张二维表的尺寸一致；数组属性返回防御性复制。`SectionRange`、`IsSectionActive`、`ReadBufferFull`、坐标裁剪、网络发送和写入时钟不属于本 Component。

#### `Proposed RemoteClientRateLimitState`

Suggested path: `src2/Network/Session/RemoteClientRateLimitStateComponent.cs` (`status: proposed`)

成员映射：`SpamProjectile`、`SpamAddBlock`、`SpamDeleteBlock`、`SpamWater`、四个 `*Max` 上限，以及 `SpamUpdate`/`SpamClear` 语义。限流是按连接的 security policy state，不是战斗/Tile/Liquid authority。`Proposed RateLimitQuery` 只计算 allow/reject；`Proposed RateLimitSystem` 写计数、衰减并发出 boot/reject command。上限来源、时钟和浮点衰减必须可注入且可测试。

#### `Proposed RemoteClientReceiveState`

Suggested path: `src2/Network/Transport/RemoteClientReceiveStateComponent.cs` (`status: proposed`)

成员映射：`ReadBuffer`、`_isReading`、`TryRead`、`ServerReadCallBack`、`IsConnected`。read buffer 属于 socket adapter；`_isReading` 是并发保护/短期 I/O state，不应作为可持久化组件。zero-byte read、ObjectDisposed、socket error 和 cancellation 转成 `ConnectionTerminationCommand`，未知结果不得自动重试业务 command。

### 4.5 Section streaming 和 activity

#### `Proposed WorldSectionStreamingState`

Suggested path: `src2/Network/Section/WorldSectionStreamingStateComponent.cs` (`status: proposed`)

字段映射：`width`、`height`、`data`、四个 bit index、`mapSectionsLeft`、`prevFrame`、`prevMap`、`IterationState.centerPos/X/Y/leg/xDir/yDir`。一个 section 的 loaded/framed/map-drawn/needs-refresh flags 作为同一 section lifecycle concept 合并；map cursor 作为 transient traversal state，不与 flags 混成一个 public component。`SetAllSectionsLoaded`、`SetTilesLoaded`、`SetSectionFramed`、`SetSectionAsRefreshed` 等行为属于 `SectionLifecycleSystem`，`TileLoaded`/`SectionLoaded`/`MapSectionDrawn` 属于纯 `SectionStateQuery`。

完整参考表明 `frameSectionsLeft` 和 `_sectionsNeedingRefresh` 也是派生计数，建议只缓存并由 state transition owner 维护，使用 invariant verifier 复算。Version4 当前文件缺少这些字段，记录 `version-drift/implementation-gap`。

当前 Component 实现：`src2/Network/Section/WorldSectionStreamingStateComponent.cs`。它保存 section 网格尺寸、每个 section 的 `byte` flag 快照和 `MapSectionsLeft`；构造函数验证网格乘积、flag 数量和剩余数量范围，并对输入 flag 做防御性复制。`IterationState`、cursor、section flag 迁移、`frameSectionsLeft`、`_sectionsNeedingRefresh` 以及 `TileLoaded`/`SectionLoaded` 查询均不属于该 Component。

#### `Proposed ActiveSectionObservationState`

Suggested path: `src2/Network/Section/ActiveSectionObservationStateComponent.cs` (`status: proposed`)

成员映射：`SectionInactiveTime`、`LastActiveTime`、`CheckSection`、`IsSectionActive`、`SectionActivated` 和 `ClampSectionCoords`。活动时间依赖 `WorldTick` 和 section coordinate，建议由 world/network integration owner 保存，而不是挂到每个 client。`ActiveSections` 事件是 outbound observation signal；不能从 event listener 隐式修改 Tile/Entity authority。

### 4.6 Network publication、sound 和 content modules

#### `Proposed NetworkPublicationState`

Suggested path: `src2/Network/Projection/NetworkPublicationState.cs` (`status: proposed`)

成员映射：`buffer`、`_compressChestList`、`_compressSignList`、`_compressEntities`。这些是批量序列化 scratch/connection buffer，不是世界组件。`Proposed SnapshotPublicationSystem` 读取权威 snapshot，生成 immutable outbound envelopes，并在 network adapter 接收前完成 schema/version 标记。

#### `Proposed NetworkSoundProjection`

Suggested path: `src2/Network/Projection/NetworkSoundProjection.cs` (`status: proposed`)

成员映射：`NetSoundInfo.position`、`soundIndex`、`style`、`volume`、`pitchOffset`。声音 payload 是客户端 projection，position 与 entity/spatial owner 的关系 `crossSubsystemOwner: integration-review`。可选字段必须显式表示 absent/default，不能将 `-1` 作为未声明的隐式协议约定而散落在调用者。

#### `Proposed NetworkSerializationContext`

Suggested path: `src2/Network/Projection/NetworkSerializationContext.cs` (`status: proposed`)

成员映射：`_currentPlayerDeathReason`、`_currentNetSoundInfo`、`_currentRevengeMarker`。它们只在单次 serialization command 期间存在，禁止跨 tick 持有或被多个连接并行覆写。每次 publication 必须使用局部 context 或显式租约，并在成功/异常路径清理。

#### `Proposed ContentNetworkProjection`

Suggested path: `src2/Network/Projection/ContentNetworkProjection.cs` (`status: proposed`)

成员映射：`NetCreativePowerPermissionsModule._setPermissionLevelId`、`NetLiquidModule.ChunkChanges.DirtiedPackedTileCoords`、`ChunkX`、`ChunkY`、`NetLiquidModule._changesForPlayerCache`、`_changesByChunkCoords`；同时覆盖 `FlowerPacketInfo.stylesOnPurity`、`stylesOnCorruption`、`stylesOnCrimson`、`stylesOnHallow`。

Liquid dirty set 的 owner 必须是 Liquid authority；本分区只提出按 section/client capability 过滤的 projection adapter。flower lists 是 content payload/serialization input，不是 persistent component。`NetCreativePowerPermissionsModule` 的 `powerId/level` 是 protocol payload，最终 Creative authority 属于 integration-review。

### 4.7 Chat command protocol

#### `Proposed ChatCommandRegistryState`

Suggested path: `src2/Network/Chat/ChatCommandRegistryState.cs` (`status: proposed`)

成员映射：`ChatCommandAttribute.Name`、`ChatCommandId._name`、`ChatCommandProcessor._localizedCommands`、`_commands`、`_aliases`、`_defaultCommand`、`ChatManager.Commands`、`DebugCommands`。注册/alias preparation 是 session/content initialization effect；command lookup Query 必须只读；command implementation 不能被网络 adapter 直接创建任意 world mutation。

#### `Proposed ChatMessageCommand`

Suggested path: `src2/Network/Chat/ChatMessageCommand.cs` (`status: proposed`)

成员映射：`ChatMessage.CommandId`、`Text`、`IsConsumed`。`CreateOutgoingMessage` 产生 command intent；`ProcessIncomingMessage` 先做 protocol/schema/permission validation，再提交显式 `ChatCommandExecution`。`IsConsumed` 只能由 processor 的单一 owner 改变，重复 incoming message 应有 sequence/idempotency policy。

#### `Proposed ChatCommandAdapter`

Suggested path: `src2/Network/Chat/ChatCommandAdapter.cs` (`status: proposed`)

成员映射：`EmojiCommand.PlayerEmojiDuration`、`_byName`、`EmoteCommand.RESPONSE_COLOR` 以及 `Initialize`/`PrepareAliases`/incoming/outgoing hooks。localization、emote registry 和 response color 都是 adapter/content dependencies；emoji/emote command 触发的 player effect 交给对应 gameplay owner，P13 不宣布 owner。

#### `Proposed ChatColorCatalog`

Suggested path: `src2/Network/Chat/ChatColorCatalog.cs` (`status: proposed`)

成员映射：`ChatColors.BossOrEvent`、`World`、`NPCTravel`、`ServerMessage`、`Death`。它是 presentation value catalog，不是 network authority；颜色覆盖与 death/world event payload 的跨分区读取标记 `crossSubsystemOwner: integration-review`。

#### `Proposed ChatPublicationAdapter`

Suggested path: `src2/Network/Chat/ChatPublicationAdapter.cs` (`status: proposed`)

接口方向：`ChatPublicationCommand -> NetworkText/Color/author -> NetTextModule/NetManager -> client projection`。成员对应 `ChatHelper._cachedMessages`、`SendChatMessageToClient*`、`BroadcastChatMessage*`、`OnlySendToPlayersWhoAreLoggedIn`、`DisplayMessage`。server broadcast/send 与 local display 分为两个 effect port；cache capacity/eviction 和 message author identity 不能隐式写入 simulation。

### 4.8 Chat snippets、layout 和 monitor

#### `Proposed ChatSnippetPresentationState`

Suggested path: `src2/Chat/Presentation/ChatSnippetPresentationState.cs` (`status: proposed`)

成员映射：`AchievementSnippet._achievement`、`GlyphSnippet.ForcedStyle`、`_glyphIndex`、`GlyphsPerLine`、`MaxGlyphs`、`DefaultGlyphStyle`、`GlyphStyle`、`GlyphIndexes`、`ItemSnippet._item`、`PositionedSnippet.Snippet`、`OrigIndex`、`Line`、`Position`、`Size`、`TextSnippet.Text`、`TextOriginal`、`Color`、`CheckForHover`、`DeleteWhole`。所有外部 `Achievement`/`Item`/input button 类型只在 adapter/handler 边界使用；snippet component 只保存 presentation snapshot/reference token。

`ChatManager.ParseMessage` 和 `LayoutSnippets` 是 proposed pure queries，输出 immutable `ChatSnippetView`/`PositionedSnippetView`；`OnHover`/`OnClick` 生成 UI command，不反向改变 world/network authority。`GlyphIndexes` 和 style 是 input-device/content cache，声明失效和重新注册策略。

#### `Proposed ChatMonitorState`

Suggested path: `src2/Chat/Presentation/ChatMonitorStateComponent.cs` (`status: proposed; bounded Component slice implemented under src2`)

成员映射：`RemadeChatMonitor.MaxMessages`、`_messages`（只取消息文本快照）、`_lastChatWidthLimit`；`ChatManager.Regexes.Format`、`_handlers`、`ShadowDirections`；`ChatMessageContainer.OriginalText`、`_prepared`、`_widthLimitInPixels`、`_timeLeft`。当前 `ChatMonitorStateComponent` 只保存默认容量为 500 的有界消息文本快照、快照数量和最后宽度限制，并对输入集合做防御性复制；不持有 `ChatMessageContainer`，不实现队列变更、过期、布局、regex parse、handler lookup、绘制或 UI 交互。上述容器和 monitor writer 行为由排除的 Presentation/System 边界负责，仍保持 deferred。

## 5. 253 条成员覆盖表

以下 18 行按输入报告叶子分组，但每一行列出的成员都来自本分区 253 条库存，并且已给出 proposed 归属。叶子分组不是组件边界；角色列对应第 4 节的 proposed 类型。

| 叶子子系统 | 全部成员 -> proposed 归属 |
|---|---|
| `MainRecentServerAndMapState` | `maxMP`, `recentWorld`, `recentIP`, `recentPort` -> `NetworkSessionEndpointState`; `instantBGTransitionCounter`, `bgDelay`, `bgStyle`, `bgAlphaFrontLayer`, `bgAlphaFarBackLayer`, `wofNPCIndex`, `wofDrawAreaTop`, `wofDrawAreaBottom`, `refreshMap`, `mapReady`, `updateMap`, `mapTimeMax`, `mapTime`, `clearMap` -> `NetworkMapPresentationState` |
| `MainNetworkSessionState` | `getIP`, `menuMultiplayer`, `menuServer`, `netMode`, `_targetNetMode`, `_hasPendingNetmodeChange`, `netPlayCounter`, `lastItemUpdate`, `maxItemUpdates` -> `NetworkSessionModeState` |
| `NetworkMessageBufferAndDispatch` | `readBufferMax`, `writeBufferMax` -> `NetworkBufferLimits`; `broadcast` -> `MessagePublicationRouting`; `readBuffer`, `writeBuffer`, `readerStream`, `writerStream`, `reader`, `writer` -> `NetworkMessageBufferAdapterState`; `writeLocked` -> `OutboundSerializationLease`; `messageLength`, `totalData`, `whoAmI`, `checkBytes` -> `MessageFrameCursorState`; `spamCount`, `maxSpam` -> `CompatibilityRateAccounting`; `History` -> `PacketDiagnosticsProjection`; `_temporaryProjectileAI`, `_temporaryNPCAI` -> `MessageDecodeScratch`; `RemainingReadBufferLength` -> `MessageBufferQuery` |
| `NetworkPublicationAndSound` | `NetSoundInfo.position`, `soundIndex`, `style`, `volume`, `pitchOffset` -> `NetworkSoundProjection`; `buffer`, `_compressChestList`, `_compressSignList`, `_compressEntities` -> `NetworkPublicationState`; `_currentPlayerDeathReason`, `_currentNetSoundInfo`, `_currentRevengeMarker` -> `NetworkSerializationContext` |
| `SectionStreamingState` | `IterationState.centerPos`, `X`, `Y`, `leg`, `xDir`, `yDir` -> `SectionIterationCursorState`; `BitIndex_SectionLoaded`, `BitIndex_SectionFramed`, `BitIndex_SectionMapDrawn`, `BitIndex_SectionNeedsRefresh`, `width`, `height`, `data`, `mapSectionsLeft`, `prevFrame`, `prevMap` -> `WorldSectionStreamingState` |
| `NetworkRemoteServerState` | `Socket` -> `RemoteServerTransportHandle`; `IsActive`, `State`, `TimeOutTimer`, `PendingTermination`, `IsReading` -> `RemoteServerConnectionState`; `ReadBuffer` -> `RemoteServerReceiveAdapter`; `StatusText`, `StatusCount`, `StatusMax` -> `RemoteServerStatusProjection`; `ServerSpecialFlags` -> `RemoteServerCapabilitySnapshot` |
| `NetworkRemoteIpRequestAdapter` | `RequestId`, `SuccessCallback`, `RemoteAddress` -> `RemoteIpLookupRequestAdapter` |
| `NetworkRemoteClientConnectionAndStatusState` | `Socket` -> `RemoteClientTransportHandle`; `Id` -> `NetworkConnectionSlot` (`crossSubsystemOwner: integration-review`); `Name` -> `RemoteClientIdentityState`; `IsActive`, `PendingTermination`, `PendingTerminationApproved`, `IsAnnouncementCompleted`, `State`, `TimeOutTimer` -> `RemoteClientConnectionState`; `StatusText`, `StatusText2`, `StatusCount`, `StatusMax` -> `RemoteClientStatusProjection` |
| `NetworkRemoteClientSectionAndRateLimitState` | `TileSections`, `TileSectionsCheckTime`, `CheckingSections` -> `RemoteClientSectionObservationState`; `ReadBuffer`, `_isReading`, `ReadBufferFull` -> `RemoteClientReceiveState`; `SpamProjectile`, `SpamAddBlock`, `SpamDeleteBlock`, `SpamWater`, `SpamProjectileMax`, `SpamAddBlockMax`, `SpamDeleteBlockMax`, `SpamWaterMax` -> `RemoteClientRateLimitState` |
| `NetworkSessionConfigurationState` | `MaxConnections`, `NetBufferSize`, `DefaultPort`, `BanFilePath`, `ServerPassword`, `ServerIP`, `ServerIPText`, `IsHostAndPlay`, `HostToken`, `UseUPNP`, `SaveOnServerExit`, `HandshakeLoggingEnabled` -> `NetworkSessionConfigurationState` |
| `NetworkSessionTransportAndThreadState` | `Clients` -> `RemoteClientRegistry`; `Connection` -> `RemoteServerConnectionState`; `TcpListener` -> `NetworkListenerTransport`; `ListenPort`, `IsListening`, `Disconnect`, `SpamCheck` -> `NetworkTransportControlState`; `HasClients` -> `NetworkSessionPresenceProjection`; `_serverThread` -> `NetworkServerThreadState`; `_upnpnat`, `_mappings` -> `NetworkPortMappingAdapter`; `fullBuffer` -> `NetworkFullBufferAdapter`; `swTicksLast` -> `NetworkDiagnosticsClockState`; `BroadcastClient`, `broadcastThread` -> `NetworkDiscoveryBroadcastAdapter` |
| `SharedNetworkSocketTransport` | `Packet.BaseTimestamp`, `Data` -> `DebugNetworkPacket`; `Latency` -> `DebugNetworkLatencyState`; `_stream` -> `NetworkStreamAdapter`; `_outgoingQueue`, `_incomingQueue` -> `DebugNetworkQueueState`; `_writeException`, `_readException` -> `NetworkIoFailureState`; `_readMode`, `_closed`, `_startTicks`, `_beginReadBuf` -> `NetworkAsyncReadState`; `_connection`, `_listener`, `_listenerCallback`, `_remoteAddress`, `_isListening`, `_debugStream` -> `TcpSocketTransportAdapter`; `CompletedAsyncResult.AsyncState`, `Read`, `IsCompleted`, `CompletedSynchronously`, `AsyncWaitHandle`, `DataAvailable` -> `AsyncIoCompletionAdapter` |
| `SharedNetworkPacketPrimitives` | `LegacyNetBufferPool.SMALL_BUFFER_SIZE`, `MEDIUM_BUFFER_SIZE`, `LARGE_BUFFER_SIZE`, `bufferLock`, `_smallBufferQueue`, `_mediumBufferQueue`, `_largeBufferQueue`, `_smallBufferCount`, `_mediumBufferCount`, `_largeBufferCount`, `_customBufferCount` -> `NetworkBufferPoolAdapter`; `PacketTypeStorage<T>.Id`, `Module` -> `NetworkModuleRegistration`; `NetManager.Instance`, `_modules`, `_moduleCount` -> `NetworkModuleRegistryState`; `NetPacket.HEADER_SIZE`, `Id`, `Buffer`, `Length`, `Writer`, `Reader` -> `NetworkPacketEnvelope`; `RemoteAddress.Type`, `TcpAddress.Address`, `Port` -> `RemoteEndpointValue` |
| `SharedContentNetworkModules` | `NetCreativePowerPermissionsModule._setPermissionLevelId` -> `CreativePermissionProjection`; `ChunkChanges.DirtiedPackedTileCoords`, `ChunkX`, `ChunkY`, `_changesForPlayerCache`, `_changesByChunkCoords` -> `LiquidSectionReplicationProjection` |
| `SharedNetworkSectionProjections` | `ActiveSections.SectionInactiveTime`, `LastActiveTime` -> `ActiveSectionObservationState`; `FlowerPacketInfo.stylesOnPurity`, `stylesOnCorruption`, `stylesOnCrimson`, `stylesOnHallow` -> `FlowerPacketProjection` |
| `SharedChatAndCommandProtocol` | `ChatCommandAttribute.Name`, `ChatCommandId._name`, `ChatCommandProcessor._localizedCommands`, `_commands`, `_aliases`, `_defaultCommand`, `ChatManager.Commands`, `DebugCommands` -> `ChatCommandRegistryState`; `ChatColors.BossOrEvent`, `World`, `NPCTravel`, `ServerMessage`, `Death` -> `ChatColorCatalog`; `ChatHelper._cachedMessages` -> `ChatMessageCache`; `ChatMessage.CommandId`, `Text`, `IsConsumed` -> `ChatMessageCommand`; `EmojiCommand.PlayerEmojiDuration`, `_byName`, `EmoteCommand.RESPONSE_COLOR` -> `ChatCommandAdapter` |
| `SharedChatSnippetPresentationState` | `AchievementSnippet._achievement` -> `AchievementSnippetAdapter`; `GlyphSnippet.ForcedStyle`, `_glyphIndex`, `GlyphsPerLine`, `MaxGlyphs`, `DefaultGlyphStyle`, `GlyphStyle`, `GlyphIndexes` -> `GlyphSnippetPresentationState`; `ItemSnippet._item` -> `ItemSnippetAdapter`; `PositionedSnippet.Snippet`, `OrigIndex`, `Line`, `Position`, `Size` -> `PositionedSnippetView`; `TextSnippet.Text`, `TextOriginal`, `Color`, `CheckForHover`, `DeleteWhole` -> `TextSnippetPresentationState` |
| `SharedChatMonitorAndCommandState` | `RemadeChatMonitor.MaxMessages`, `_messages` (text snapshots only), `_lastChatWidthLimit` -> `ChatMonitorStateComponent`; `ChatManager.Regexes.Format`, `_handlers`, `ShadowDirections` -> `ChatTagRegistryAndLayoutQuery`; `ChatManager.DebugCommands`, `Commands` -> `ChatCommandRegistryState`; `ChatMessageContainer.OriginalText`, `_prepared`, `_widthLimitInPixels`, `_timeLeft` -> `ChatMessageContainerState` |

## 6. 组合关系、依赖方向和顺序

Proposed data flow:

```text
SocketTransportAdapter
  -> NetworkReceiveAdapter
  -> MessageFrameCursor / NetworkDecodeSystem
  -> InboundNetworkCommandQueue
  -> NetworkCommandCommitSystem
  -> authoritative domain state (integration-review)

authoritative snapshots
  -> Section/Liquid/Creative/Chat/Sound Projection systems
  -> NetworkPacketEnvelope
  -> NetworkSendAdapter
  -> SocketTransportAdapter

Chat command text
  -> ChatCommandAdapter
  -> ChatMessageCommand
  -> command processor / domain command owner
  -> ChatPublicationAdapter or ChatMonitor projection
```

Proposed scheduling contract:

1. `NetworkTransportPollSystem` receives bytes and records bounded frames; it does not mutate simulation state.
2. `NetworkDecodeSystem` validates frame length, schema, connection state, rate policy and sequence, then emits immutable inbound commands.
3. `NetworkCommandCommitSystem` runs on the simulation thread and is the only proposed boundary that can submit domain mutations; authoritative owners remain integration-review.
4. `SectionObservationSystem` updates client section eligibility from explicit position/world-section snapshots; it must run before section projection selection.
5. `SnapshotProjectionSystem` reads committed state and creates outbound projections. Liquid/content/chat/sound projections may be independent only after their input snapshot is frozen.
6. `NetworkSendSystem` owns packet shrink, async send handoff, completion accounting and recycle. File order, thread start order and packet id do not define simulation order.
7. `ChatLayoutQuery` and `SectionStateQuery` are pure calculations. UI systems may consume their result but cannot write authoritative network/session state.

Required ordering remains `crossSubsystemOwner: integration-review` when it crosses entity, Tile/Liquid, player, UI, audio or persistence boundaries.

## 7. ID、快照和副作用边界

| Identifier/value | Proposed meaning | Forbidden conflation | Owner status |
|---|---|---|---|
| runtime EntityId | ECS runtime row identity | network slot, persistence ID, protocol module id | integration-review |
| compatibility/player slot | Version4 `RemoteClient.Id`/`whoAmI` compatibility index | EntityId or remote endpoint | integration-review |
| NetworkId | wire identity for a networked entity or snapshot relation | connection slot or packet module id | integration-review |
| PersistentId/world ID | save/load identity | network address or transient sequence | integration-review |
| SectionCoordinate | world section address | section loaded flag or client observation timestamp | integration-review |
| protocol message/module id | schema dispatch discriminator | entity identity or ordering guarantee | P13 adapter candidate |
| receive sequence | transport observation for duplicate/order detection | simulation tick or persistence revision | integration-review |

Network I/O, time, random latency, DNS, UPNP, UDP discovery, threads, logging, diagnostics, UI, audio and persistence all stay in explicit adapters/projections. Query code receives time and snapshot data as inputs. Timeout is not automatically failure; an async send with unknown completion needs an auditable retry/idempotency policy.

## 8. Evidence gaps and blocking decisions

### Evidence gaps

- The full member-level caller graph for all 253 rows is not mechanically generated in this session; the coverage table proves source member inclusion, while reader/writer status is group-level and marked partial where not directly closed.
- `NetMessage` contains a large message switch. The exact authority, schema version, duplicate behavior and failure policy for every message id is deferred to focused per-message verifiers.
- Version4 `WorldSections.cs` is reduced relative to the complete reference; frame/refresh counters and map iteration semantics require a version-specific reconciliation before implementation.
- `NetLiquidModule.Deserialize` and `NetCreativePowerPermissionsModule.Deserialize` are stubbed in the checked Version4 file; inbound authority and validation are missing evidence.
- `ChatCommandProcessor.CreateOutgoingMessage` and `ProcessIncomingMessage`, and several snippet/monitor methods are stubbed; only public boundary and field lifecycle are confirmed.
- Static fields, async callbacks, legacy buffer recycling, swallowed exceptions and `Thread.Abort` create concurrency/cleanup gaps.
- P13 first-round research output is absent; no first-round design conclusion is treated as evidence.

### Blocking decisions

1. `NetworkId`, player slot, runtime `EntityId`, persistent identity and external endpoint must be assigned by the integration session. Candidate: keep them separate as in `EntityIdentityState`; alternative: retain a compatibility slot adapter temporarily. The choice changes every inbound/outbound mapping.
2. The network thread to simulation thread handoff must be selected. Candidate A is immutable command queue with main-thread commit; candidate B is a lock-protected shared buffer. A is easier to verify but may require batching/backpressure; B preserves legacy shape but keeps hidden races.
3. Packet delivery semantics must be selected per projection: at-most-once send, retryable idempotent snapshot, or explicit ack/replay. A single global policy is unsafe for chat, section data, liquid deltas and sound.
4. Final owner and order for Tile/Liquid section activation, player handshake, chat command execution and UI/audio projections require integration-review.

## 9. Focused verifier plan

设计阶段没有运行 verifier；实现阶段记录了 focused verifier 的历史旧产物运行结果，但当前源码重建未通过。当前缓冲边界已覆盖：有界追加、容量失败原子性、2 字节 little-endian 帧长度观察、完整帧隔离快照、消费后游标复位、write lock 排他性、write capacity 和 reset；inbound command 已覆盖元数据保留、负 slot 拒绝和 payload snapshot 隔离。剩余 proposed checks:

- `NetworkMessageBufferVerifier`: partial frame accumulation, zero-length/oversize frame, bounded reader, reader/writer disposal, repeated frame and buffer ownership remain to be added; the current focused verifier covers the bounded cursor and write-lock slice.
- `NetworkConnectionLifecycleVerifier`: accept, handshake states, password rejection, timeout, zero-byte read, socket exception, pending termination approval, reset and player disconnect exactly-once.
- `NetworkRateLimitVerifier`: each counter increment/decay/threshold, disabled policy, fractional underflow, boot command, repeated packet and monotonic test clock.
- `NetworkSectionVerifier`: coordinate clamping, active window, client observation versus world loaded state, section range edges, map cursor progress, refresh counter conservation.
- `NetworkPacketVerifier`: header length, module id lookup, shrink-to-fit, buffer pool bucket, send failure and recycle exactly-once. The packet-envelope test now passes in the focused verifier; send-failure integration remains pending.
- `NetworkProjectionVerifier`: liquid filtering per section/client, creative permission payload, sound optional fields, chat author/recipient filter, sequence/version metadata.
- `ChatProtocolVerifier`: command registration/alias collision, outgoing/incoming parse, consumed state, duplicate command, localization failure and permission boundary.
- `ChatPresentationVerifier`: tag parsing, escaped delimiters, layout wrapping, hover/click command generation, width invalidation and max-message eviction.
- Static review: no external socket/stream type in proposed core components; no Query writes; no projection writes back to authority; no broad mutable collection leakage; no unbounded retry.
- The affected verifier was run serially through `Build/Tools/Invoke-SerialDotnet.ps1` with `run --project .\src2\NetworkProtocolSessionVerification\Terraria.NetworkProtocolSessionVerification.csproj --no-build --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false`; this historical no-build run exited `0`, printed `P13 verifier passed.`, and reused the artifact under `Build/bin/Terraria.NetworkProtocolSessionVerification/Debug/net10.0`, including `Terraria.NetworkProtocolSessionVerification.dll` and the executable output. The current verifier source build exits `1` with `CS0246` at `src2/NetworkProtocolSessionVerification/Program.cs:336` for intentionally deferred `NetworkModuleRegistryState`; the old artifact run is not current-source verification evidence. The earlier red run exited `1` with four expected missing-type compiler diagnostics before the source was added.

| checkpoint-11 | sixteen implemented boundaries, including `ChatMonitorStateComponent` as bounded message-text snapshot state only | none (remaining proposed boundaries are explicitly deferred) | publication scratch, serialization context, content projection, chat registry/commands/publication, snippet presentation, monitor container/layout/expiry/queue writer behavior, integration review | first-round report absent; complete 253-member reader/writer/lifecycle graph remains partial; monitor text snapshots have constructor-invariant evidence only; queue mutation, container layout/cache, expiry and UI lifecycle remain partial; no protocol compatibility or behavior-equivalence evidence | final ID owner, section/world authority, active-window clock owner, network-thread commit boundary, connection/resource owner, delivery/retry semantics, publication scratch lifetime, chat/UI owner and monitor writer require integration review | 2026-09-12T11:11:05.258Z |

## 10. Integration Handoff

partitionId: P13
sessionId: 02b27d497bf647e8879091412c7fa91d
previousSessionId: 26e2f78c2b1c4eca8488af170b54fe72
staleDocumentSessionId: 6a5c8da4872840ce90e0326dad3647ba
handoffId: manual-handoff-P13-e5ae98903e004eaf8a0f52e4dcd9982d
handoffStatus: handed-off
inputReport: D:\TRbackup\NLTX\docs\migration\ledgers\non-authoritative-component-partitions\13-network-protocol-session.md
componentDesignPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\non-authoritative\2026-09-11-version4-non-authoritative-P13-network-protocol-session-component-design.md
componentExecutionPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\non-authoritative\2026-09-11-version4-non-authoritative-P13-network-protocol-session-component-execution.md
evidenceStatus: source inventory confirmed; core boundary evidence confirmed; member-level lifecycle partial
nltxStatus: src2 contains 16 implemented Component/state boundaries: NetworkSessionEndpointState, NetworkMapPresentationState, NetworkSessionModeState, NetworkSessionConfigurationState, RemoteEndpointValue, NetworkBufferPoolAdapter, NetworkMessageBufferAdapterState, InboundNetworkCommand, NetworkPacketEnvelope, RemoteClientRateLimitStateComponent, RemoteServerConnectionStateComponent, RemoteClientIdentityAndLifecycleStateComponent, RemoteClientSectionObservationStateComponent, WorldSectionStreamingStateComponent, ActiveSectionObservationStateComponent, and ChatMonitorStateComponent (bounded message-text snapshot state only); remaining Registry/Adapter/Projection/System/Query/Command boundaries and chat monitor container/layout/expiry/queue writer behavior are explicitly deferred
verificationStatus: partial
confirmedOwners:
- Version4 socket/stream adapter boundary
- Version4 message frame and packet serialization boundary
- Version4 remote connection and client section observation concepts
- Version4 chat send/broadcast and chat presentation public boundaries
implementedComponents:
- NetworkSessionEndpointState
- NetworkMapPresentationState
- NetworkSessionModeState
- NetworkSessionConfigurationState
- RemoteEndpointValue
- NetworkBufferPoolAdapter
- NetworkMessageBufferAdapterState
- InboundNetworkCommand
- NetworkPacketEnvelope
- RemoteClientRateLimitStateComponent
- RemoteServerConnectionStateComponent
- RemoteClientIdentityAndLifecycleStateComponent
- RemoteClientSectionObservationStateComponent
- WorldSectionStreamingStateComponent
- ActiveSectionObservationStateComponent
- ChatMonitorStateComponent (bounded message-text snapshot state only)
deferredBoundaries:
- NetworkModuleRegistryState (registration/dispatch behavior excluded)
- RemoteClientStatusProjection
- RemoteClientReceiveState
- NetworkPublicationState
- NetworkSoundProjection
- NetworkSerializationContext
- ContentNetworkProjection
- ChatCommandRegistryState
- ChatMessageCommand
- ChatCommandAdapter
- ChatColorCatalog
- ChatPublicationAdapter
- ChatSnippetPresentationState
- ChatMonitorState container/layout, expiry, queue mutation and monitor writer behavior
- integration review and evidence closure
sharedTypesForIntegrationReview:
- runtime EntityId, compatibility/player slot, NetworkId, PersistentId, NetworkText and snapshot sequence
- section coordinate/reference and Tile/Liquid authority relation
- chat command owner and player/entity author relation
- final network-to-simulation commit queue and system order
crossSubsystemReaders:
- player, entity, Tile, Liquid, world session, persistence, UI, audio and diagnostics candidates
crossSubsystemWriters:
- handshake/session transition, player input, entity/world mutation and content projection candidates
orderingConstraints:
- receive/decode before simulation commit
- section observation before section projection selection
- committed snapshot before packet publication
- packet send completion before buffer recycle
- chat command validation before chat publication
boundaryChallenges:
- do not aggregate socket, buffer, packet, connection, section, chat and simulation snapshot into one component
- preserve separate client section observation and world section authority
- make retries, duplicate messages, partial frames, shutdown and async resource release explicit
evidenceGaps:
- first-round P13 report absent
- full 253-member reader/writer/lifecycle graph not mechanically closed
- reduced/stubbed Version4 methods and static/thread cleanup semantics
- ChatMonitorStateComponent has constructor-invariant evidence only; container/layout, expiry, queue mutation and monitor writer behavior remain unverified
blockingDecisions:
- ID ownership, thread handoff, delivery semantics, section/content/chat final owners and order
- ChatMonitorState container/layout/expiry/queue writer ownership
notImplemented:
- the sixteen listed Component/state boundaries are implemented under `src2`; ChatMonitorStateComponent is limited to bounded message-text snapshots; all remaining proposed P13 boundaries are deferred, including Registry/Adapter/System/Query/Command/Projection and chat/presentation behavior; protocol compatibility and behavior-equivalence verification remain unclaimed
verifierPlan:
- focused buffer, lifecycle, rate, section, packet, projection, chat protocol and chat presentation verifiers

本文件不是迁移完成报告、行为等价证明、API/网络/持久化闭合声明，也不是当前 NLTX 已实现能力的完整声明。当前实现仅位于 `src2`；生产 `src` 未修改。
