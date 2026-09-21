# Version4 非权威组件第二轮执行计划：P13 网络协议与会话

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
- The current NetworkProtocolSessionVerification source build exits `1` with `CS0246` at `src2/NetworkProtocolSessionVerification/Program.cs:336` because `NetworkModuleRegistryState` is intentionally deferred; no verifier source was changed. A prior `run --no-build --no-restore` invocation reused an existing artifact and printed `P13 verifier passed.`, but it is not current-source rebuild evidence. The identity/lifecycle, section and ChatMonitorStateComponent boundaries have source and constructor-invariant evidence only. No protocol compatibility or behavior-equivalence claim is established.
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

## 1. 执行性质、输入和禁止项

本文件是 P13 的实施记录，`executionStatus: completed (src2-only Component implementation checkpoint)`。已将可独立表达的 16 个 Component 边界创建于 `src2`；其中 `ChatMonitorStateComponent` 仅保存有界消息文本快照和最后宽度限制，剩余容器布局、过期、队列变更和 monitor writer 行为仍 deferred。剩余发布、注册表、聊天、适配器、投影和表现边界均因任务的 Component-only 限制或跨分区 owner 未裁决而 deferred。网络项目串行构建通过；focused verifier 的当前源码重建因故意缺失的 `NetworkModuleRegistryState` 在 `Program.cs:336` 失败，未修改 verifier，也没有声明行为等价或网络协议闭合。输入包括 P13 的 253 条库存、P13 专属提示、Version4 直接证据、完整参考对 `WorldSections` 的补证、tModLoader `v2026.07` 公开页面和 SS14 最小结构参考。

禁止把 proposed 目录当成已经创建的目录；禁止把 network adapter 中的 socket/stream/reader/writer/Thread/UPNP 类型放入核心 Component；禁止跨 P 分区裁决最终 owner；禁止直接双写 Version4 和新模型而没有明确兼容窗口。

## 1.1 当前实现检查点

- 已完成组件：`Proposed NetworkSessionEndpointState`、`Proposed NetworkMapPresentationState`、`Proposed NetworkSessionModeState`、`Proposed NetworkSessionConfigurationState`、`Proposed RemoteEndpointValue`、`Proposed NetworkBufferPoolAdapter`、`Proposed NetworkMessageBufferAdapterState`、`Proposed InboundNetworkCommand`、`Proposed NetworkPacketEnvelope`、`Proposed RemoteClientRateLimitState`、`Proposed RemoteServerConnectionState`、`Proposed RemoteClientIdentityAndLifecycleState`、`Proposed RemoteClientSectionObservationState`、`Proposed WorldSectionStreamingState`、`Proposed ActiveSectionObservationState`、`Proposed ChatMonitorStateComponent`（仅有界消息文本快照状态）。
- 实际源码：`src2/Network/Session/NetworkSessionEndpointStateComponent.cs`、`src2/Network/Presentation/NetworkMapPresentationStateComponent.cs`、`src2/Network/Session/NetworkSessionModeStateComponent.cs`、`src2/Network/Session/NetworkSessionConfigurationStateComponent.cs`、`src2/Network/Transport/RemoteEndpointValue.cs`、`src2/Network/Transport/NetworkBufferPoolAdapter.cs`、`src2/Network/Protocol/NetworkMessageBufferAdapterState.cs`、`src2/Network/Protocol/InboundNetworkCommand.cs`、`src2/Network/Protocol/NetworkPacketEnvelope.cs`、`src2/Network/Session/RemoteClientRateLimitStateComponent.cs`、`src2/Network/Session/RemoteServerConnectionStateComponent.cs`、`src2/Network/Session/RemoteClientIdentityAndLifecycleStateComponent.cs`、`src2/Network/Section/RemoteClientSectionObservationStateComponent.cs`、`src2/Network/Section/WorldSectionStreamingStateComponent.cs`、`src2/Network/Section/ActiveSectionObservationStateComponent.cs`、`src2/Chat/Presentation/ChatMonitorStateComponent.cs`；验证器入口为 `src2/NetworkProtocolSessionVerification/Program.cs`，项目为 `src2/NetworkProtocolSessionVerification/Terraria.NetworkProtocolSessionVerification.csproj`。
- 验证项目：`src2/NetworkProtocolSessionVerification/Terraria.NetworkProtocolSessionVerification.csproj`。网络项目实际验证：通过 `Build/Tools/Invoke-SerialDotnet.ps1` 串行构建，退出码 `0`，0 warning、0 error，产物为 `Build/bin/Terraria.NetworkProtocolSession/Debug/net10.0/Terraria.NetworkProtocolSession.dll`。验证项目源码重建：同一串行 wrapper 构建退出码 `1`，0 warning、1 error，`src2/NetworkProtocolSessionVerification/Program.cs:336` 缺少故意 deferred 的 `NetworkModuleRegistryState`；没有修改 verifier。此前 `run --no-build --no-restore` 输出 `P13 verifier passed.` 只是复用旧产物，不能替代当前源码构建证据。未声明协议兼容或行为等价。
- 当前新增最小单元已完成为 `Proposed ChatMonitorStateComponent`；它仅保存最多 500 条消息文本快照和最后宽度限制。`NetworkModuleRegistryState`、`RemoteClientStatusProjection`、`RemoteClientReceiveState` 以及其余发布/聊天/表现边界均明确 deferred。

### 1.2 Packet envelope implementation checkpoint

- Source saved: `src2/Network/Protocol/NetworkPacketEnvelope.cs`.
- Required red phase: the focused verifier exited `1` with four expected missing-type compiler diagnostics for `NetworkPacketEnvelope` before the source file was added.
- Implementation boundary: 5-byte little-endian packet header, bounded payload cursor, `ShrinkToFit`, and exactly-once buffer lease recycle through `NetworkBufferPoolAdapter`.
- Historical green run: the serial focused verifier exited `0`, printed `P13 verifier passed.`, and emitted no warning/error lines while reusing an existing artifact under `Build/bin/Terraria.NetworkProtocolSessionVerification/Debug/net10.0`. The current verifier source rebuild exits `1` with `CS0246` for intentionally deferred `NetworkModuleRegistryState` at `src2/NetworkProtocolSessionVerification/Program.cs:336`; therefore this old artifact run is not current-source verification evidence. No packet-envelope protocol compatibility or behavior-equivalence claim is made.

## 2. Proposed 文件组织和命名

以下每个路径都带 `status: proposed`。目录按能力组织，避免 `Shared/Components/`、`Common/`、`Misc/`；小域直接平铺，只有稳定的 transport/protocol/session/section/chat/presentation 边界才提出子目录。每个文件只承载一个核心 public 类型。

| 能力 | Proposed 文件 | 命名空间候选 | 角色 |
|---|---|---|---|
| session | `src2/Network/Session/NetworkSessionEndpointStateComponent.cs` | `Terraria.Network.Session` | Component |
| session | `src2/Network/Session/NetworkSessionModeStateComponent.cs` | `Terraria.Network.Session` | Component |
| session | `src2/Network/Session/NetworkSessionConfigurationStateComponent.cs` | `Terraria.Network.Session` | Component/adapter state |
| session | `src2/Network/Session/RemoteClientIdentityAndLifecycleStateComponent.cs` | `Terraria.Network.Session` | Component, implemented |
| session | `src2/Network/Session/RemoteClientRateLimitStateComponent.cs` | `Terraria.Network.Session` | Component |
| session | `src2/Network/Session/RemoteServerConnectionStateComponent.cs` | `Terraria.Network.Session` | Component |
| protocol | `src2/Network/Protocol/NetworkMessageBufferAdapterState.cs` | `Terraria.Network.Protocol` | Adapter state |
| protocol | `src2/Network/Protocol/InboundNetworkCommand.cs` | `Terraria.Network.Protocol` | Command |
| protocol | `src2/Network/Protocol/NetworkPacketEnvelope.cs` | `Terraria.Network.Protocol` | Envelope/value boundary |
| protocol | `src2/Network/Protocol/NetworkModuleRegistryState.cs` | `Terraria.Network.Protocol` | Registry state, deferred |
| transport | `src2/Network/Transport/NetworkBufferPoolAdapter.cs` | `Terraria.Network.Transport` | Adapter |
| transport | `src2/Network/Transport/RemoteEndpointValue.cs` | `Terraria.Network.Transport` | Value/adapter boundary |
| transport | `src2/Network/Transport/TcpSocketTransportAdapter.cs` | `Terraria.Network.Transport` | Adapter |
| transport | `src2/Network/Transport/AsyncIoCompletionAdapter.cs` | `Terraria.Network.Transport` | Adapter |
| section | `src2/Network/Section/WorldSectionStreamingStateComponent.cs` | `Terraria.Network.Section` | Component, implemented |
| section | `src2/Network/Section/ActiveSectionObservationStateComponent.cs` | `Terraria.Network.Section` | Component/integration state |
| section | `src2/Network/Section/RemoteClientSectionObservationStateComponent.cs` | `Terraria.Network.Section` | Component, implemented |
| projection | `src2/Network/Projection/NetworkPublicationState.cs` | `Terraria.Network.Projection` | Projection state |
| projection | `src2/Network/Projection/NetworkSoundProjection.cs` | `Terraria.Network.Projection` | Projection |
| projection | `src2/Network/Projection/ContentNetworkProjection.cs` | `Terraria.Network.Projection` | Projection |
| chat | `src2/Network/Chat/ChatCommandRegistryState.cs` | `Terraria.Network.Chat` | Adapter state |
| chat | `src2/Network/Chat/ChatMessageCommand.cs` | `Terraria.Network.Chat` | Command |
| chat | `src2/Network/Chat/ChatCommandAdapter.cs` | `Terraria.Network.Chat` | Adapter |
| chat | `src2/Network/Chat/ChatPublicationAdapter.cs` | `Terraria.Network.Chat` | Adapter/projection |
| chat | `src2/Chat/Presentation/ChatSnippetPresentationState.cs` | `Terraria.Chat.Presentation` | Presentation state |
| chat | `src2/Chat/Presentation/ChatMonitorStateComponent.cs` | `Terraria.Chat.Presentation` | Component |
| presentation | `src2/Network/Presentation/NetworkMapPresentationStateComponent.cs` | `Terraria.Network.Presentation` | Component/projection |
| presentation | `src2/Network/Presentation/RemoteClientStatusProjection.cs` | `Terraria.Network.Presentation` | Projection |

These are target proposals only. The first implementation slice must reconcile the proposed paths with the repository's actual project boundaries and the formal ECS file organization rule before creating any additional file.

## 3. Source member to target role/file mapping

The following mapping repeats all 253 source members from the input report. The source names remain compatibility evidence; they are not new API names. Each target is proposed and must be implemented only after its owner and dependency direction are approved.

| Source leaf | Source members -> proposed role/file |
|---|---|
| `MainRecentServerAndMapState` | `maxMP`, `recentWorld`, `recentIP`, `recentPort` -> `NetworkSessionEndpointStateComponent`; `instantBGTransitionCounter`, `bgDelay`, `bgStyle`, `bgAlphaFrontLayer`, `bgAlphaFarBackLayer`, `wofNPCIndex`, `wofDrawAreaTop`, `wofDrawAreaBottom`, `refreshMap`, `mapReady`, `updateMap`, `mapTimeMax`, `mapTime`, `clearMap` -> `NetworkMapPresentationStateComponent` |
| `MainNetworkSessionState` | `getIP`, `menuMultiplayer`, `menuServer`, `netMode`, `_targetNetMode`, `_hasPendingNetmodeChange`, `netPlayCounter`, `lastItemUpdate`, `maxItemUpdates` -> `NetworkSessionModeStateComponent` |
| `NetworkMessageBufferAndDispatch` | `readBufferMax`, `writeBufferMax` -> `NetworkBufferLimits` in `NetworkMessageBufferAdapterState`; `broadcast` -> `MessagePublicationRouting`; `readBuffer`, `writeBuffer`, `readerStream`, `writerStream`, `reader`, `writer` -> `NetworkMessageBufferAdapterState`; `writeLocked` -> `OutboundSerializationLease`; `messageLength`, `totalData`, `whoAmI`, `checkBytes` -> `MessageFrameCursorState`; `spamCount`, `maxSpam` -> `CompatibilityRateAccounting`; `History` -> `PacketDiagnosticsProjection`; `_temporaryProjectileAI`, `_temporaryNPCAI` -> `MessageDecodeScratch`; `RemainingReadBufferLength` -> `MessageBufferQuery` |
| `NetworkPublicationAndSound` | `position`, `soundIndex`, `style`, `volume`, `pitchOffset` -> `NetworkSoundProjection`; `buffer`, `_compressChestList`, `_compressSignList`, `_compressEntities` -> `NetworkPublicationState`; `_currentPlayerDeathReason`, `_currentNetSoundInfo`, `_currentRevengeMarker` -> `NetworkSerializationContext` |
| `SectionStreamingState` | `centerPos`, `X`, `Y`, `leg`, `xDir`, `yDir` -> `SectionIterationCursorState`; `BitIndex_SectionLoaded`, `BitIndex_SectionFramed`, `BitIndex_SectionMapDrawn`, `BitIndex_SectionNeedsRefresh`, `width`, `height`, `data`, `mapSectionsLeft`, `prevFrame`, `prevMap` -> `WorldSectionStreamingStateComponent` |
| `NetworkRemoteServerState` | `Socket` -> `RemoteServerTransportHandle`; `IsActive`, `State`, `TimeOutTimer`, `PendingTermination`, `IsReading` -> `RemoteServerConnectionStateComponent`; `ReadBuffer` -> `RemoteServerReceiveAdapter`; `StatusText`, `StatusCount`, `StatusMax` -> `RemoteServerStatusProjection`; `ServerSpecialFlags` -> `RemoteServerCapabilitySnapshot` |
| `NetworkRemoteIpRequestAdapter` | `RequestId`, `SuccessCallback`, `RemoteAddress` -> `RemoteIpLookupRequestAdapter` |
| `NetworkRemoteClientConnectionAndStatusState` | `Socket` -> `RemoteClientTransportHandle`; `Id` -> `NetworkConnectionSlot` with `crossSubsystemOwner: integration-review`; `Name` -> `RemoteClientIdentityState`; `IsActive`, `PendingTermination`, `PendingTerminationApproved`, `IsAnnouncementCompleted`, `State`, `TimeOutTimer` -> `RemoteClientConnectionState`; `StatusText`, `StatusText2`, `StatusCount`, `StatusMax` -> `RemoteClientStatusProjection` |
| `NetworkRemoteClientSectionAndRateLimitState` | `TileSections`, `TileSectionsCheckTime`, `CheckingSections` -> `RemoteClientSectionObservationStateComponent`; `ReadBuffer`, `_isReading`, `ReadBufferFull` -> `RemoteClientReceiveState`; `SpamProjectile`, `SpamAddBlock`, `SpamDeleteBlock`, `SpamWater`, `SpamProjectileMax`, `SpamAddBlockMax`, `SpamDeleteBlockMax`, `SpamWaterMax` -> `RemoteClientRateLimitStateComponent` |
| `NetworkSessionConfigurationState` | `MaxConnections`, `NetBufferSize`, `DefaultPort`, `BanFilePath`, `ServerPassword`, `ServerIP`, `ServerIPText`, `IsHostAndPlay`, `HostToken`, `UseUPNP`, `SaveOnServerExit`, `HandshakeLoggingEnabled` -> `NetworkSessionConfigurationStateComponent` plus `NetworkConfigurationAdapter` |
| `NetworkSessionTransportAndThreadState` | `Clients` -> `RemoteClientRegistry`; `Connection` -> `RemoteServerConnectionStateComponent`; `TcpListener` -> `NetworkListenerTransport`; `ListenPort`, `IsListening`, `Disconnect`, `SpamCheck` -> `NetworkTransportControlState`; `HasClients` -> `NetworkSessionPresenceProjection`; `_serverThread` -> `NetworkServerThreadState`; `_upnpnat`, `_mappings` -> `NetworkPortMappingAdapter`; `fullBuffer` -> `NetworkFullBufferAdapter`; `swTicksLast` -> `NetworkDiagnosticsClockState`; `BroadcastClient`, `broadcastThread` -> `NetworkDiscoveryBroadcastAdapter` |
| `SharedNetworkSocketTransport` | `BaseTimestamp`, `Data` -> `DebugNetworkPacket`; `Latency` -> `NetworkDebugLatencyState`; `_stream` -> `NetworkStreamAdapter`; `_outgoingQueue`, `_incomingQueue` -> `DebugNetworkQueueState`; `_writeException`, `_readException` -> `NetworkIoFailureState`; `_readMode`, `_closed`, `_startTicks`, `_beginReadBuf` -> `NetworkAsyncReadState`; `_connection`, `_listener`, `_listenerCallback`, `_remoteAddress`, `_isListening`, `_debugStream` -> `TcpSocketTransportAdapter`; `AsyncState`, `Read`, `IsCompleted`, `CompletedSynchronously`, `AsyncWaitHandle`, `DataAvailable` -> `AsyncIoCompletionAdapter` |
| `SharedNetworkPacketPrimitives` | `SMALL_BUFFER_SIZE`, `MEDIUM_BUFFER_SIZE`, `LARGE_BUFFER_SIZE`, `bufferLock`, `_smallBufferQueue`, `_mediumBufferQueue`, `_largeBufferQueue`, `_smallBufferCount`, `_mediumBufferCount`, `_largeBufferCount`, `_customBufferCount` -> `NetworkBufferPoolAdapter`; `PacketTypeStorage<T>.Id`, `Module` -> `NetworkModuleRegistration`; `Instance`, `_modules`, `_moduleCount` -> `NetworkModuleRegistryState`; `HEADER_SIZE`, `Id`, `Buffer`, `Length`, `Writer`, `Reader` -> `NetworkPacketEnvelope`; `Type`, `Address`, `Port` -> `RemoteEndpointValue` |
| `SharedContentNetworkModules` | `_setPermissionLevelId` -> `CreativePermissionProjection`; `DirtiedPackedTileCoords`, `ChunkX`, `ChunkY`, `_changesForPlayerCache`, `_changesByChunkCoords` -> `LiquidSectionReplicationProjection` |
| `SharedNetworkSectionProjections` | `SectionInactiveTime`, `LastActiveTime` -> `ActiveSectionObservationState`; `stylesOnPurity`, `stylesOnCorruption`, `stylesOnCrimson`, `stylesOnHallow` -> `FlowerPacketProjection` |
| `SharedChatAndCommandProtocol` | `Name` in `ChatCommandAttribute`, `_name` in `ChatCommandId`, `_localizedCommands`, `_commands`, `_aliases`, `_defaultCommand`, `Commands`, `DebugCommands` -> `ChatCommandRegistryState`; `BossOrEvent`, `World`, `NPCTravel`, `ServerMessage`, `Death` -> `ChatColorCatalog`; `_cachedMessages` -> `ChatMessageCache`; `CommandId`, `Text`, `IsConsumed` -> `ChatMessageCommand`; `PlayerEmojiDuration`, `_byName`, `RESPONSE_COLOR` -> `ChatCommandAdapter` |
| `SharedChatSnippetPresentationState` | `_achievement` -> `AchievementSnippetAdapter`; `ForcedStyle`, `_glyphIndex`, `GlyphsPerLine`, `MaxGlyphs`, `DefaultGlyphStyle`, `GlyphStyle`, `GlyphIndexes` -> `GlyphSnippetPresentationState`; `_item` -> `ItemSnippetAdapter`; `Snippet`, `OrigIndex`, `Line`, `Position`, `Size` -> `PositionedSnippetView`; `Text`, `TextOriginal`, `Color`, `CheckForHover`, `DeleteWhole` -> `TextSnippetPresentationState` |
| `SharedChatMonitorAndCommandState` | `MaxMessages`, `_messages` (text snapshots only), `_lastChatWidthLimit` -> `ChatMonitorStateComponent`; `Format`, `_handlers`, `ShadowDirections` -> `ChatTagRegistryAndLayoutQuery`; `DebugCommands`, `Commands` -> `ChatCommandRegistryState`; `OriginalText`, `_prepared`, `_widthLimitInPixels`, `_timeLeft` -> `ChatMessageContainerState` |

## 4. Staged implementation sequence

### Step 0: Freeze evidence and contracts

Record the input report hash, exact Version4 source commit/snapshot, protocol message/module schema inventory, and the integration decisions for slot/EntityId/NetworkId/section identity. No implementation starts while any member is missing a role. Add a machine-readable member coverage check outside this session only if the integration owner authorizes it.

### Step 1: Session configuration and endpoint state

Create proposed tests and then implementation for configuration value validation, endpoint text/address separation, mode transition intent, and recent server capacity. Keep `NetworkConfigurationAdapter` as the only boundary for DNS, UPNP, launch parameters, ban file and secret material. Add compatibility read adapters before changing callers. Single write owners: `NetworkSessionTransitionSystem` for mode; `RecentServerEndpointCommandHandler` for recent endpoints; `NetworkConfigurationAdapter` for external configuration.

Rollback: remove new registration and route reads back through the legacy facade while preserving the legacy static state. Condition: any mismatch in mode transition, slot allocation or secret exposure blocks rollout.

### Step 2: Packet and buffer primitives

Introduce bounded buffer lease, packet envelope, module registry and endpoint value seams. Port `NetPacket` header/shrink/recycle behavior with tests for oversize, partial write, exception and double recycle. Keep the old `NetManager` facade as a compatibility adapter with one packet recycle owner. Do not expose `BinaryReader`, `BinaryWriter`, `CachedBuffer` or sockets to domain components.

Rollback: disable the new send adapter and use legacy `NetManager` while retaining diagnostics. Condition: any packet length, module id, buffer ownership or send ordering discrepancy blocks rollout.

### Step 3: Receive buffer and command submission

Split `MessageBuffer` into receive adapter, frame cursor, decode scratch, diagnostics projection and inbound command queue. Port only one protocol message family per change. The decode system validates framing, protocol version, connection state, rate limit and sequence. The simulation commit system is the sole submission boundary for domain writes.

Compatibility: legacy `GetData` remains behind an adapter during the window. Do not dual-write authoritative domain fields. If a message is decoded by both paths for comparison, only one path may commit and the other must be observation-only.

Rollback: stop command queue consumption and re-enable legacy dispatch at a clean session boundary. Condition: duplicate command, partial frame, permission or exception mismatch blocks rollout.

### Step 4: Remote connection lifecycle and rate policy

Separate server connection, client identity/lifecycle, receive state, status projection, section observation and rate-limit state. Port accept/handshake/password/timeout/zero-read/error/reset as a state machine. Use injected monotonic clock and explicit cancellation. Make `PendingTermination` and `PendingTerminationApproved` a single transition owner; socket close, buffer lease release and player disconnect hook must be idempotent.

Compatibility: keep `RemoteClient.Id` as a compatibility slot adapter, never as a proposed ECS EntityId. Preserve status text as a projection. No retry after unknown remote result without idempotency evidence.

### Step 5: Section streaming and activity

Implement section flags, map/frame/refresh counters and client section observation separately. Reconcile reduced Version4 `WorldSections` with complete reference before porting. Use explicit section coordinate value and world tick input. Verify out-of-range coordinates, active-window expiry, section reload, refresh count conservation and map cursor termination.

Compatibility: translate old `TileSections` and world section arrays at the adapter boundary. Never use client observation to mark world section authority loaded.

### Step 6: Projection families

Port network snapshot, sound, liquid, creative permission and flower payloads as one-way projections. Freeze an input snapshot before serialization. Add schema/version and recipient/section filtering. Keep entity/player/Tile/Liquid owners outside P13 until integration review. Define whether each payload is at-most-once, retryable idempotent or acked.

Rollback: disable each projection family independently and retain legacy send path. Condition: schema, recipient, optional-field, section-filter or recycle mismatch blocks that family.

### Step 7: Chat protocol and presentation

Split command registry, inbound/outbound chat commands, publication adapter, color catalog, snippet adapters, pure parse/layout queries and monitor queue. Register handlers during content/session initialization; command handlers emit explicit domain commands. Chat publication may broadcast or target a logged-in client, while local display is a separate projection. Enforce max-message eviction and width invalidation through one monitor writer.

Compatibility: retain legacy `ChatHelper` facade as adapter; compare rendered projection and recipient selection before switching. Do not put `TextSnippet` or UI objects on network wire.

### Step 8: Remove compatibility only after acceptance

Remove legacy fields only after focused verifiers, static checks, per-family protocol compatibility, shutdown tests and serial project verification pass. This is outside the current session and requires integration owner authorization.

## 5. Single write owners and side-effect ledger

| Effect | Single proposed owner | Reads | Writes/effects | Failure/retry |
|---|---|---|---|---|
| socket open/listen/close | `TcpSocketTransportAdapter` | configuration, endpoint, cancellation | OS socket and callbacks | explicit close; no business retry without policy |
| receive bytes | `NetworkReceiveAdapter` | socket, buffer lease | frame buffer/queue | zero-read/error -> termination command |
| frame decode | `NetworkDecodeSystem` | immutable bytes, protocol registry | inbound command/diagnostic | reject malformed/unknown with bounded result |
| simulation mutation | domain owner after `NetworkCommandCommitSystem` | command and authority state | ECS/world state | no duplicate commit; version check |
| packet serialization | projection system + `NetworkPacketEnvelope` | frozen snapshot | buffer bytes | schema failure rejects before send |
| send/recycle | `NetworkSendSystem` | envelope, connection | socket async send, diagnostics, recycle | unknown completion is not safe retry |
| rate policy | `RateLimitSystem` | explicit clock, policy, connection counters | counters/reject/boot command | bounded decay; no hidden clock |
| section observation | `SectionObservationSystem` | player position, section snapshot, clock | per-client observation | stale observation expires; does not mutate world authority |
| chat registry | `ChatCommandRegistryAdapter` | localization/content | handler registry | collision is initialization error |
| chat publication | `ChatPublicationAdapter` | chat command/projection, recipient state | packet/local UI message | recipient filter and delivery policy explicit |
| chat monitor | `ChatMonitorSystem` | message projection, width, delta | bounded display queue/cache | max count and expiry deterministic |

## 6. Network, snapshot and persistence migration

- No P13 component is persisted by default. Socket handles, buffers, readers/writers, thread state, rate counters, message cursors, chat monitor cache and debug latency are transient.
- Persisted recent server entries, if retained, use a dedicated versioned adapter and never store password, host token or live socket state.
- Network snapshots carry separate runtime entity reference, compatibility slot, network identity, section reference, protocol schema/version and receive sequence. They must not serialize persistence IDs unless the owning subsystem explicitly provides them.
- During a compatibility window, the legacy `NetMessage`/`NetManager` facade may read or publish, but a single authority must commit domain state. Observation-only comparison is allowed; dual commit is forbidden.
- Section and liquid projections need per-recipient filtering and explicit duplicate policy. Chat and sound should not inherit section/liquid retry semantics.
- Shutdown must stop accepting new work, cancel/drain receive/send adapters, settle or discard pending commands according to policy, release buffer leases, close sockets and then clear session state. `Thread.Abort` cannot be the new lifecycle contract.

## 7. Focused verifier, static check and build plan

The following commands/checks remain planned unless explicitly recorded as executed below; the current message-buffer slice has already been verified:

1. Run focused unit/integration verifiers for buffer framing, packet ownership, module dispatch, lifecycle, rate limit, section state, projections and chat. Cover failure, timeout unknown, cancellation, duplicate, out-of-order and resource release cases.
2. Run static checks for external type leakage, Query writes, projection writes, mutable collection exposure, hidden global state, unbounded retry and missing cancellation/close paths.
3. Verify all 253 input members have a role and no member is silently dropped. Reconcile Version4 and complete reference line/symbol drift before implementing each family.
4. For affected project verification, run from repository root only through:

   `pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 build .\path\AffectedProject.csproj -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false`

    Then run the focused verifier with `--no-build --no-restore` only after its source build succeeds. This session ran that command through the serial wrapper against an existing verifier artifact; it exited `0` and printed `P13 verifier passed.`, but the current-source rebuild exits `1` with `CS0246` for intentionally deferred `NetworkModuleRegistryState`, so the old artifact run is not current-source verification evidence. The network project build exited `0` with 0 warnings and 0 errors, producing `Build/bin/Terraria.NetworkProtocolSession/Debug/net10.0/Terraria.NetworkProtocolSession.dll`.
5. Confirm artifacts are under `Build/bin/`, record command, project, exit code, warning/error counts and output path, and do not claim behavior equivalence from a compile alone.

## 8. Rollback and completion criteria

Rollback triggers: protocol schema mismatch, duplicate authoritative commit, wrong recipient/section filtering, unbounded retry, packet double recycle, resource leak, shutdown race, connection slot/entity ID conflation, chat command permission bypass, or any verifier that cannot distinguish timeout unknown from failure.

Rollback procedure: stop new adapter registration at a session boundary; drain or discard pending commands per explicit policy; disable the affected projection/transport family; close/release resources through its owner; restore legacy facade reads/writes without dual commit; retain diagnostics and evidence for the failed slice.

Completion requires all 253 source members mapped, every cross-domain owner reviewed, all focused verifiers passing, protocol/packet compatibility evidence recorded, static checks clean, affected project build/test executed serially under repository policy, and no unclassified proposed path. None of these implementation completion conditions has been performed in this session.

## 9. Checkpoint log

| checkpoint | completedComponents | currentComponent | pendingComponents | evidence-gap | blocking-decision | lastCheckpointUtc |
|---|---|---|---|---|---|---|
| 1 | `NetworkSessionEndpointState`, `NetworkMapPresentationState` | `NetworkSessionConfigurationState` | buffer, packet, connection, section, projection, chat families | first-round report absent; lifecycle graph partial | IDs and thread handoff unresolved | 2026-09-11T17:18:22.098Z |
| checkpoint-1 | six implemented boundaries | `NetworkMessageBufferAdapterState` | buffer, packet, connection, section, projection, chat families | first-round report absent; lifecycle graph partial; reduced or stubbed methods leave semantics partial | final ID/slot owner, network-thread commit boundary, delivery/retry/cleanup semantics, and cross-domain snapshot order require integration review | 2026-09-11T17:44:30.120Z |
| checkpoint-2 | seven implemented boundaries, including `NetworkMessageBufferAdapterState` | `InboundNetworkCommand` | packet, connection, section, projection, chat families | first-round report absent; full 253-member reader/writer/lifecycle graph not mechanically reconstructed; reader/writer resource and per-message decode semantics remain partial | final ID/slot owner, network-thread commit boundary, delivery/retry/cleanup semantics, and cross-domain snapshot order require integration review | 2026-09-11T21:32:20.1305451Z |
| checkpoint-3 | eight implemented boundaries, including `InboundNetworkCommand` | `NetworkPacketEnvelope` | connection, section, projection, chat families | first-round report absent; full 253-member reader/writer/lifecycle graph not mechanically reconstructed; per-message decode, packet ownership and integration semantics remain partial | final ID/slot owner, network-thread commit boundary, delivery/retry/cleanup semantics, and cross-domain snapshot order require integration review | 2026-09-11T21:44:26.1520418Z |
| checkpoint-4 | nine implemented boundaries, including `NetworkPacketEnvelope` | `NetworkModuleRegistryState` | connection, section, projection, chat families | first-round report absent; full 253-member reader/writer/lifecycle graph not mechanically reconstructed; per-message decode, send ownership and integration semantics remain partial | final ID/slot owner, network-thread commit boundary, delivery/retry/cleanup semantics, and cross-domain snapshot order require integration review | 2026-09-12T02:02:05.3282706Z |
| checkpoint-5 | ten implemented boundaries, including `RemoteClientRateLimitStateComponent` as counter/limit state only | `NetworkModuleRegistryState` (deferred) | module registry, connection, section, projection, chat families | first-round report absent; rate-limit writer/clock/rejection behavior and full 253-member reader/writer/lifecycle graph remain partial; NetworkModuleRegistryState requires excluded adapter behavior | final ID/slot owner, network-thread commit boundary, rate-limit clock/decay owner, delivery/retry/cleanup semantics, and cross-domain snapshot order require integration review | 2026-09-12T08:28:00.0000000Z |
| checkpoint-6 | eleven implemented boundaries, including `RemoteServerConnectionStateComponent` as lifecycle state only | `RemoteClientIdentityAndLifecycleState` | client section, projection, chat families | first-round report absent; socket/read-buffer ownership, lifecycle transition effects, rate-limit writer/clock/rejection behavior and full 253-member reader/writer/lifecycle graph remain partial | final ID/slot owner, network-thread commit boundary, connection resource owner, rate-limit clock/decay owner, delivery/retry/cleanup semantics, and cross-domain snapshot order require integration review | 2026-09-12T08:48:00.0000000Z |
| checkpoint-7 | twelve implemented boundaries, including `RemoteClientIdentityAndLifecycleStateComponent` as compatibility-slot/lifecycle state only | `RemoteClientSectionObservationState` | section, projection, chat families | first-round report absent; player/entity mapping, socket/read-buffer ownership, lifecycle transition effects, rate-limit writer/clock/rejection behavior and full 253-member reader/writer/lifecycle graph remain partial | final ID/slot owner, section coordinate owner, network-thread commit boundary, connection resource owner, rate-limit clock/decay owner, delivery/retry/cleanup semantics, and cross-domain snapshot order require integration review | 2026-09-12T08:59:27.4931163Z |
| checkpoint-8 | thirteen implemented boundaries, including `RemoteClientSectionObservationStateComponent` as defensive-copied observation state | `WorldSectionStreamingState` | active-section, projection, chat families | first-round report absent; section writer/query lifecycle, coordinate owner, read-buffer ownership, rate-limit writer/clock/rejection behavior and full 253-member reader/writer/lifecycle graph remain partial; receive and status boundaries are excluded | final ID/slot owner, section coordinate/world authority, active-window owner, network-thread commit boundary, connection resource owner, rate-limit clock/decay owner, delivery/retry/cleanup semantics, and cross-domain snapshot order require integration review | 2026-09-12T09:06:14.8785517Z |
| checkpoint-9 | fourteen implemented boundaries, including `WorldSectionStreamingStateComponent` as validated flag/grid state | `ActiveSectionObservationState` | projection, chat families | first-round report absent; cursor/flag writer lifecycle, active time source, coordinate owner, socket/read-buffer ownership, rate-limit writer/clock/rejection behavior and full 253-member reader/writer/lifecycle graph remain partial; receive and status boundaries are excluded | final ID owner, section/world authority, active-window clock owner, network-thread commit boundary, connection resource owner, rate-limit clock/decay owner, delivery/retry/cleanup semantics, and cross-domain snapshot order require integration review | 2026-09-12T09:16:55.0920008Z |
| checkpoint-10 | fifteen implemented boundaries, including `ActiveSectionObservationStateComponent` as validated active-time state | `NetworkPublicationState` (deferred) | publication scratch, serialization context, content projection, chat registry/commands/publication, snippet and monitor presentation, integration review | first-round report absent; complete 253-member reader/writer/lifecycle graph remains partial; section mutation/query behavior, cursor traversal, rate-limit mutation, socket/read-buffer ownership, publication ownership and chat/UI lifecycle remain outside the Component-only slice | final ID owner, section/world authority, active-window clock owner, network-thread commit boundary, connection/resource owner, delivery/retry semantics, publication scratch lifetime and chat/UI owner require integration review | 2026-09-12T10:00:00.0000000Z |
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
- transport adapter owns socket/stream/buffer resource effects
- decode adapter owns frame validation and command creation
- commit boundary is proposed but final domain owner is integration-review
- projections own outbound encoding, not source authority
implementedComponents:
- the sixteen Component/state boundaries listed in the design document are implemented under `src2`; ChatMonitorStateComponent is limited to bounded message-text snapshots
deferredBoundaries:
- remaining Registry/Adapter/System/Query/Command/Projection boundaries, chat presentation behavior, and integration review/evidence closure remain deferred
sharedTypesForIntegrationReview:
- EntityId, player/compatibility slot, NetworkId, PersistentId, section references, snapshot sequence and chat author
crossSubsystemReaders:
- entity/player, Tile/Liquid, world session, persistence, UI, audio and diagnostics candidates
crossSubsystemWriters:
- handshake, player input, world/content authority and UI command candidates
orderingConstraints:
- receive -> decode -> commit -> snapshot -> project -> send -> recycle
boundaryChallenges:
- preserve separate transport, protocol, connection, section, chat and presentation lifecycles
evidenceGaps:
- missing first-round P13 report; incomplete per-member caller/lifecycle graph; reduced/stubbed source methods
- ChatMonitorStateComponent has constructor-invariant evidence only; container/layout, expiry, queue mutation and monitor writer behavior remain unverified
blockingDecisions:
- final ID owner, thread handoff, delivery/retry semantics and cross-domain order
- ChatMonitorState container/layout/expiry/queue writer ownership
notImplemented:
- the sixteen listed Component/state boundaries are implemented under `src2`; ChatMonitorStateComponent is limited to bounded text snapshots; all remaining proposed P13 boundaries are deferred, including Registry/Adapter/System/Query/Command/Projection and chat/presentation behavior; protocol compatibility and behavior-equivalence verification remain unclaimed
verifierPlan:
- focused buffer, lifecycle, rate, section, packet, projection and chat suites plus serial affected-project verification

本文件是后续实施计划与增量实现记录，不是迁移完成报告、行为等价证明或当前 NLTX 已实现能力的完整声明。当前实现仅位于 `src2`；生产 `src` 未修改。
