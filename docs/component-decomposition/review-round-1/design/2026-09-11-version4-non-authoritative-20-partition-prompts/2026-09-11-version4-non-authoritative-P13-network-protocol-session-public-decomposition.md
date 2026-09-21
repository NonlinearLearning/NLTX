# Version4 非权威组件拆分分区 P13：网络协议与会话 - public-decomposition 专属提示词

> 本文件只包含当前分区的专属任务内容，不复制公共提示词正文。
> 使用时：先提供公共提示词，再附加本文件；公共协议中的通用规则、证据门槛、报告结构、禁止项和 Integration Handoff 继续有效。

## 公共前置（仅引用）

- 公共提示词：`D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\design\2026-09-05-version4-public-decomposition-common-protocol.md`
- 必读技能：`D:\TRbackup\NLTX\.agents\skills\public-decomposition\SKILL.md`
- ECS 证据协议：`D:\TRbackup\NLTX\.agents\skills\public-decomposition\references\ecs-evidence-protocol.md`
- tModLoader 检索协议：`D:\TRbackup\NLTX\.agents\skills\public-decomposition\references\tmodloader-documentation-retrieval.md`
- 本文件不重复上述文件内容；执行前必须实际读取并遵守它们。
- 并行会话保障 skill：D:\\TRbackup\\NLTX\\.agents\\skills\\version4-non-authoritative-partition-session\\SKILL.md
- 并行领取/结算 runner：D:\\TRbackup\\NLTX\\Build\\Tools\\Invoke-Version4NonAuthoritativePartitionSession.ps1
- 必须先按 skill 使用 ClaimNext（或用户明确指定时使用 Claim -PartitionId P13），只写本文件的 outputReport，并用领取返回的同一 sessionId 完成 Complete/Fail/Abandon；不得手工编辑 ledger 或 lock。
- 本任务属于 20 个并行会话中的固定分区；currentTaskCount: 20。公共协议中遗留的 19 会话数量说明在本任务包中不适用。

## 任务元数据

- promptId: P13-non-authoritative-public-decomposition-20260911
- partitionId: P13
- inputPartitionReport: D:\TRbackup\NLTX\docs\migration\ledgers\non-authoritative-component-partitions\13-network-protocol-session.md
- outputReport: D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P13-network-protocol-session-public-decomposition.md
- reportType: public-decomposition
- executionMode: independent-read-only-partition
- currentTaskCount: 20
- formalParentSubsystems: NetworkSessionAndSectionStreaming, RuntimeComposition, SharedRuntimeMechanisms
- leafSubsystemCount: 18
- fieldCount: 238
- propertyCount: 15
- memberCount: 253
- sourceReportSha256: b944441d92d5103f218b766a3e354200794528a19e65a862e44c2b40a36f1196
- sourceInventoryStatus: source-inventory-confirmed; member-level owner/lifecycle/side-effect evidence still requires this session to close

## 专属范围锁定

- 本会话只审查输入分区报告中的 18 个叶子子系统和 253 条成员记录。以下清单是本会话唯一的成员范围；不得读取或复制其他 P 分区的成员清单。
- 细分子系统仍只是库存边界，不等价于一个组件；允许在本分区内部按访问模式拆成多个 proposed Component、System、Query、Command、Adapter 或 Projection。
- 来源序号、路径、行号和声明是检索线索；必须回到 Version4 重新定位，不得把输入报告直接当作已确认的读者、写者、生命周期或 owner 证据。

| 叶子子系统 | 父级 | 原章节 | 边界角色 | 字段 | 属性 | 成员 | 细分职责 |
|---|---|---|---|---:|---:|---:|---|
| `MainRecentServerAndMapState` | `RuntimeComposition` | `4.1.11` | `presentation state` | 18 | 0 | 18 | 最近服务器、背景层和地图刷新状态。 |
| `MainNetworkSessionState` | `RuntimeComposition` | `4.1.33` | `runtime state` | 9 | 0 | 9 | 网络模式切换、玩家更新和连接会话状态。 |
| `NetworkMessageBufferAndDispatch` | `NetworkSessionAndSectionStreaming` | `4.3.1` | `adapter state` | 19 | 1 | 20 | 消息缓冲、读写游标和分发上下文。 |
| `NetworkPublicationAndSound` | `NetworkSessionAndSectionStreaming` | `4.3.2` | `projection state` | 12 | 0 | 12 | 网络消息发布和声音消息载荷状态。 |
| `SectionStreamingState` | `NetworkSessionAndSectionStreaming` | `4.3.3` | `adapter state` | 16 | 0 | 16 | 世界区段迭代和客户端区段加载状态。 |
| `NetworkRemoteServerState` | `NetworkSessionAndSectionStreaming` | `4.3.4` | `adapter state` | 11 | 0 | 11 | 远端服务器连接、活动状态和服务器端点。 |
| `NetworkRemoteIpRequestAdapter` | `NetworkSessionAndSectionStreaming` | `4.3.5` | `adapter DTO` | 3 | 0 | 3 | 远端 IP 请求标识、回调和结果载荷。 |
| `NetworkRemoteClientConnectionAndStatusState` | `NetworkSessionAndSectionStreaming` | `4.3.6` | `adapter state` | 13 | 0 | 13 | 远端客户端 Socket、连接、身份和状态文本。 |
| `NetworkRemoteClientSectionAndRateLimitState` | `NetworkSessionAndSectionStreaming` | `4.3.7` | `adapter state` | 13 | 1 | 14 | 远端客户端区段、读取缓冲和反垃圾限制状态。 |
| `NetworkSessionConfigurationState` | `NetworkSessionAndSectionStreaming` | `4.3.8` | `adapter state` | 11 | 1 | 12 | 网络端口、连接上限、服务器策略和会话配置。 |
| `NetworkSessionTransportAndThreadState` | `NetworkSessionAndSectionStreaming` | `4.3.9` | `adapter state` | 15 | 0 | 15 | 网络客户端、监听器、线程、广播和传输运行状态。 |
| `SharedNetworkSocketTransport` | `SharedRuntimeMechanisms` | `4.9.37` | `adapter` | 18 | 6 | 24 | Socket、调试流和 TCP 传输。 |
| `SharedNetworkPacketPrimitives` | `SharedRuntimeMechanisms` | `4.9.38` | `adapter` | 22 | 3 | 25 | 网络包、地址和缓冲池原语。 |
| `SharedContentNetworkModules` | `SharedRuntimeMechanisms` | `4.9.39` | `adapter` | 6 | 0 | 6 | 内容能力和液体网络模块。 |
| `SharedNetworkSectionProjections` | `SharedRuntimeMechanisms` | `4.9.40` | `projection` | 6 | 0 | 6 | 区段和花朵包的网络投影载荷。 |
| `SharedChatAndCommandProtocol` | `SharedRuntimeMechanisms` | `4.9.41` | `adapter` | 15 | 3 | 18 | 聊天消息、颜色和命令处理协议。 |
| `SharedChatSnippetPresentationState` | `SharedRuntimeMechanisms` | `4.9.148` | `projection/presentation` | 19 | 0 | 19 | 文本、标签、字形和定位片段表现状态。 |
| `SharedChatMonitorAndCommandState` | `SharedRuntimeMechanisms` | `4.9.149` | `projection/adapter` | 12 | 0 | 12 | 聊天监视器、消息缓存和命令格式化处理状态。 |

来源成员的分区内序号线索范围：135..3910；序号不定义领域边界，必须逐条回到输入表和 Version4 覆盖。

## 分区专属目标

围绕“网络协议与会话”完成独立只读的 Version4 成员审查和 ECS 拆分设计。
- 围绕 Main 网络会话、MessageBuffer/dispatch、NetMessage publication、区段 streaming、远端 server/client、socket transport、rate limit、chat/command protocol 和 network projections，核对传输状态、协议载荷与模拟状态的边界。
- 回到 MessageBuffer.cs、NetMessage.cs、Netplay、Network/Remote/Section/Chat 相关源码及调用者，重新确认连接建立、握手、读取、分发、广播、断线、清理和线程生命周期。
- 分别建模 session/connection state、packet primitives、buffer cursor、inbound Command、outbound Projection、section streaming state、chat protocol Adapter 和 rate-limit policy。
- 核对服务端权威、客户端预测/展示、序列化版本、消息重复/乱序、异常关闭、重试和网络线程到模拟线程的提交顺序。

## 专属证据焦点

- 先完整读取本分区输入报告的成员表，再回到 D:\TRbackup\Version4 重新核对字段、属性、声明类型、初始化/注册、直接读者、直接写者、清理路径和副作用。
- 下列路径只作为本分区的检索种子；路径存在不等于成员语义已确认，必须按公共证据协议记录实际行号、符号、调用者、读者、写者、生命周期和 evidenceStatus。
  - `D:\TRbackup\Version4\Terraria.Chat.Commands/ChatCommandAttribute.cs`
  - `D:\TRbackup\Version4\Terraria.Chat.Commands/EmojiCommand.cs`
  - `D:\TRbackup\Version4\Terraria.Chat.Commands/EmoteCommand.cs`
  - `D:\TRbackup\Version4\Terraria.Chat/ChatColors.cs`
  - `D:\TRbackup\Version4\Terraria.Chat/ChatCommandId.cs`
  - `D:\TRbackup\Version4\Terraria.Chat/ChatCommandProcessor.cs`
  - `D:\TRbackup\Version4\Terraria.Chat/ChatHelper.cs`
  - `D:\TRbackup\Version4\Terraria.Chat/ChatMessage.cs`
  - `D:\TRbackup\Version4\Terraria.DataStructures/ActiveSections.cs`
  - `D:\TRbackup\Version4\Terraria.DataStructures/FlowerPacketInfo.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.NetModules/NetCreativePowerPermissionsModule.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.NetModules/NetLiquidModule.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.UI.Chat/AchievementTagHandler.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.UI.Chat/GlyphTagHandler.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.UI.Chat/ItemTagHandler.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.UI.Chat/RemadeChatMonitor.cs`
  - `D:\TRbackup\Version4\Terraria.Net.Sockets/DebugNetworkStream.cs`
  - `D:\TRbackup\Version4\Terraria.Net.Sockets/TcpSocket.cs`
  - `D:\TRbackup\Version4\Terraria.Net/LegacyNetBufferPool.cs`
  - `D:\TRbackup\Version4\Terraria.Net/NetManager.cs`
  - `D:\TRbackup\Version4\Terraria.Net/NetPacket.cs`
  - `D:\TRbackup\Version4\Terraria.Net/RemoteAddress.cs`
  - `D:\TRbackup\Version4\Terraria.Net/TcpAddress.cs`
  - `D:\TRbackup\Version4\Terraria.UI.Chat/ChatManager.cs`
  - `D:\TRbackup\Version4\Terraria.UI.Chat/ChatMessageContainer.cs`
  - `D:\TRbackup\Version4\Terraria.UI.Chat/PositionedSnippet.cs`
  - `D:\TRbackup\Version4\Terraria.UI.Chat/TextSnippet.cs`
  - `D:\TRbackup\Version4\Terraria/Main.cs`
  - `D:\TRbackup\Version4\Terraria/MessageBuffer.cs`
  - `D:\TRbackup\Version4\Terraria/NetMessage.cs`
  - `D:\TRbackup\Version4\Terraria/Netplay.cs`
  - `D:\TRbackup\Version4\Terraria/RemoteClient.cs`
  - `D:\TRbackup\Version4\Terraria/RemoteServer.cs`
  - `D:\TRbackup\Version4\Terraria/WorldSections.cs`

- 输入分区抽取出的声明类型焦点：
  - `Terraria.Chat.ChatColors`
  - `Terraria.Chat.ChatCommandId`
  - `Terraria.Chat.ChatCommandProcessor`
  - `Terraria.Chat.ChatHelper`
  - `Terraria.Chat.ChatMessage`
  - `Terraria.Chat.Commands.ChatCommandAttribute`
  - `Terraria.Chat.Commands.EmojiCommand`
  - `Terraria.Chat.Commands.EmoteCommand`
  - `Terraria.DataStructures.ActiveSections`
  - `Terraria.DataStructures.FlowerPacketInfo`
  - `Terraria.GameContent.NetModules.NetCreativePowerPermissionsModule`
  - `Terraria.GameContent.NetModules.NetLiquidModule`
  - `Terraria.GameContent.NetModules.NetLiquidModule.ChunkChanges`
  - `Terraria.GameContent.UI.Chat.AchievementTagHandler.AchievementSnippet`
  - `Terraria.GameContent.UI.Chat.GlyphTagHandler`
  - `Terraria.GameContent.UI.Chat.GlyphTagHandler.GlyphSnippet`
  - `Terraria.GameContent.UI.Chat.ItemTagHandler.ItemSnippet`
  - `Terraria.GameContent.UI.Chat.RemadeChatMonitor`
  - `Terraria.Main`
  - `Terraria.MessageBuffer`
  - `Terraria.Net.LegacyNetBufferPool`
  - `Terraria.Net.NetManager`
  - `Terraria.Net.NetManager.PacketTypeStorage<T>`
  - `Terraria.Net.NetPacket`
  - `Terraria.Net.RemoteAddress`
  - `Terraria.Net.Sockets.DebugNetworkStream`
  - `Terraria.Net.Sockets.DebugNetworkStream.CompletedAsyncResult`
  - `Terraria.Net.Sockets.DebugNetworkStream.Packet`
  - `Terraria.Net.Sockets.TcpSocket`
  - `Terraria.Net.TcpAddress`
  - `Terraria.NetMessage`
  - `Terraria.NetMessage.NetSoundInfo`
  - `Terraria.Netplay`
  - `Terraria.Netplay.SetRemoteIPRequestInfo`
  - `Terraria.RemoteClient`
  - `Terraria.RemoteServer`
  - `Terraria.UI.Chat.ChatManager`
  - `Terraria.UI.Chat.ChatManager.Regexes`
  - `Terraria.UI.Chat.ChatMessageContainer`
  - `Terraria.UI.Chat.PositionedSnippet`
  - `Terraria.UI.Chat.TextSnippet`
  - `Terraria.WorldSections`
  - `Terraria.WorldSections.IterationState`

- 若路径、类型或行号漂移，记录 version-drift 或 evidence-mismatch；不得静默沿用库存报告行号。Version4 是 Terraria 私有实现的事实来源，完整源码只补证 Version4 已存在文件，tModLoader 只交叉验证公开边界，SS14 只参考结构粒度。

## 专属审查问题

- Socket、远端连接、NetworkSession、MessageBuffer、Packet primitive 和 Section streaming 的权威写者与生命周期是否独立？
- 网络消息是外部 Adapter、输入 Command、状态快照 Projection 还是音频/UI payload？如何防止协议类型进入核心组件？
- 区段加载、内容/液体投影、聊天/命令和网络声音消息是否有独立可靠性、顺序和幂等性约束？
- 连接断开、反垃圾限制、线程停止和半完成事务如何清理；哪些 NetworkId/SectionId/EntityId 必须 integration-review？

## 专属不拆分边界

- 不要把 socket、buffer、packet、connection、section、chat 和模拟快照合成一个 NetworkComponent。
- 不要把网络读写游标、消息载荷、重试队列、音频包或区段更新项当作长期权威世界状态。
- 不要把传输线程顺序、文件顺序或消息号本身当作模拟 System 顺序；必须由调度/提交契约表达。

专属跨域提醒：重点记录与实体、Tile/液体、玩家输入、持久化、音频和 UI 的 integration-risk；NetworkId、snapshot、section reference、chat command owner 统一 crossSubsystemOwner: integration-review。

## 输出要求

- 将完整研究报告只写入：D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P13-network-protocol-session-public-decomposition.md
- 不要写入输入分区报告、公共提示词、其他分区报告、源代码、测试、项目文件或迁移文件。
- 报告必须逐条覆盖本分区成员，给出成员归属表、状态所有权、Component/System/Query/Command/Adapter/Projection 边界、接口契约、依赖方向、顺序约束、不拆分项、兼容策略、evidence-gap、blocking-decision、focused verifier 计划和 Integration Handoff。
- 本轮只读审查不运行编译型命令；没有实际执行的 verifier 必须写 verificationStatus: not-run。
- proposed 类型、路径和接口必须明确标记 proposed；不得写成当前 NLTX 已存在或已经迁移。
- 跨两个或以上分区使用的类型、接口、ID、快照、值对象和 System 顺序只能提出候选，必须写 crossSubsystemOwner: integration-review，不得在本报告单方面裁决。
- 不得修改任何生产代码、测试、项目文件、生成器、公共协议、分区报告或其他并行会话输出。

## 分区完成检查

- [ ] 逐条覆盖本分区全部 253 条成员，成员事实来自输入表但关键语义已回到 Version4 复核。
- [ ] 只使用本分区叶子清单；没有复制、重新设计或宣布其他 P 分区成员。
- [ ] Component 只承载内聚权威状态；派生值、缓存、查询、短期 payload、日志、网络/存档/UI 输出按正确角色隔离。
- [ ] 所有共享候选写 crossSubsystemOwner: integration-review；没有把 proposed 设计写成已实现能力。
- [ ] 未执行的构建、测试和 verifier 均明确 verificationStatus: not-run；证据缺口、冲突和 blocking-decision 已单列。
- [ ] 输出文件路径唯一为：D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P13-network-protocol-session-public-decomposition.md

## 会话结束回报

最终消息只回报以下字段：
- partitionId: P13
- outputReport: D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P13-network-protocol-session-public-decomposition.md
- evidenceSourcesRead: 实际读取的 Version4 文件/类型、必要的完整源码补证、tModLoader 页面和 SS14 参考
- evidenceGaps: 仍缺失或冲突的关键证据
- blockingDecisions: 需要最终整合会话裁决的 owner/顺序/事务边界
- verificationStatus: not-run
- productionCodeModified: no

本提示词只定义本分区的专属范围和审查抓手；公共协议负责共同证据规则、报告结构、禁止项和 Integration Handoff。
