# 宿主接入网络网关

日期：2026-10-03。组合入口已通过独立 AI 的真实 TCP 验证；最终 57 组验证及接入限制见实施报告。

2026-10-05 接入更新：宿主引用 `Network/NSSLC.Infrastructure.Network.csproj`。
定义、编译器和生成器已复制到本仓库 `Network/Pocket`，项目引用使用本地路径。
生成工具从正式 manifest 生成全目录，C# 产物按用户要求写到
`Network/Pocket/PocketSourceFile`，只按自身 manifest 清理过期文件。
DLL、obj 和 Roslyn 输出仍位于 `Build/`；详情见 [Pocket 入口](../Pocket/README.md)。
生产项目不引用 `分类参考` 或历史原型程序集。

当前阶段 AI 系统暂不实现，不纳入本次网关接入的交付与验收范围。
网络处理器及测试 owner 的接入不代表游戏 AI 行为已实现。

## 协议事实

宿主必须提供真实冻结的 tile frame-important、压缩规则、tile-entity、NPC
life-width、projectile UUID 和模块布局资料；测试中的零表或固定委托只是夹具。
在运行进程中先初始化一次全局模型，然后创建强类型 `ProtocolFacts`：

```csharp
var inputs = new ProtocolInputs(
    frameImportant, allowsSaveCompressionBatching, tileEntityCodecs,
    isServer: true, catchableTypes, lifeWidthResolver, needsUuid, slotCount,
    moduleCodecs, tagEffectNpcSlotCount, tagEffectUsesProcTimes);
var frozenFacts = new ProtocolFacts(inputs, version: protocolFactsVersion);
```

这些表和委托由宿主拥有，初始化后必须保持稳定。生成 reader/writer 直接读取
`ProtocolInputs.Instance`；模型未初始化或重复构造会失败，一个进程只能使用一套
协议事实和端别配置。客户端若需要不同表或 `IsServer` 值，运行在独立进程。
`TerrariaProtocolProfile.Create(facts)` 检查初始化模型，并封存事实注册。
默认 facts 版本为独立实例标识，宿主可显式提供已管理的版本。ProfileKey 包含目录
指纹和该版本，wire Hello 独立检查 `Terraria319`。`Sample` 是正式定义工程保留的
命名空间，不表示引用 Prototypes。包使用消息名称类型和直接成员，不再有包级 Payload/Body。

## 领域端口

`INetworkSessionAuthority` 拥有加入资格、密码/host 验证、玩家槽位分配和绑定释放。
Admit 返回 `SessionAdmission`，拒绝使用空 Binding；密码和 token 不进入诊断。

每个已迁移消息通过 `IPacketHandler<TPacket>` 接入 owner，再用
`gateway.Register(new PacketPolicy(id, stages), handler)` 明确开放。Handler 的
`NetworkSessionContext.Actor` 来自服务端绑定；packet 的 Player/Owner 字段只是线上
声明，哪些字段是发送者、哪些是目标，由既有 owner 判断。网关不覆盖所有 Player 字段。

Handler 等待 owner 提交后才返回 `PacketHandlingResult` 和权威 `OutboundDispatch`。
返回的 packet 与集合必须是稳定、独立拥有的快照，不能引用仍由 owner 原地修改的
状态容器；列表只读并不自动令内部 packet 或数组不可变。
游戏 owner 已提交而发送失败不会回滚领域状态，也不会重执行原命令。Packet13 应接入
既有 `IPlayerPacket13RouteQuery` 与 Player owner；Projectile、Tiles、Chest、Inventory
同样按现有公开能力逐步映射，不另建一套网关游戏状态。

初始化 handler 对 6、8、12 分别返回下一阶段 AwaitSectionRequest、Synchronizing、
Active。只有这些合法组合可推进；发送结束不等于客户端完成同步。初始化输出的
AllowedStages 需明确包含目标当时的阶段，仍按原顺序发送。

Single/ExplicitTargets 携带当前 ConnectionIdentity；AllActiveExceptSender 只选同一
GameSession 的 Active 目标；SectionSubscribers 还要求当前世界代次和区段订阅。
目标在入队和提交前复验。自回显使用 Single/明确目标，不统一排除发送者。

## 生命周期与失败

所有 owner 端口应响应 CancellationToken。网关限制并发调用并对等待设期限，但取消
不会撤销已经提交的领域效果。超时后的业务输出丢弃；晚到的准入绑定会调用 Release。
不合作的 owner 保留其原调用槽位，宿主必须修复该 owner，不能无限增开任务。

独立诊断通过 `TryReadDiagnostic` 读取有限的元数据通道。其 DropOldest 只影响诊断，
业务帧邮箱满会关闭连接。成功 Write receipt 表示整帧在本地发送完成，不表示对端业务 ACK。

生产接入尚需完整生产领域 owner、负载与物理工作集验证。完整格式目录不表示全部
162 个消息已经实现 gameplay handler；未注册处理器、opaque、85、94 和尚无语义证据的
模块仍禁用。

2026-10-05 真实客户端验收补充：无界面 `world-player` 场景通过原版客户端发送 207 帧，
跨越 600 个 tile 并请求 3 个新区段；随后使用包 17 破坏一个附近的普通活动 tile，测试宿主
只在进程内存 overlay 中提交变更，并通过包 20 回传；客户端原版解析器确认 tile inactive，
再请求同一区段，确认新快照仍保留破坏结果。
该 host 行为仅用于探测，不包含玩家物理/碰撞、掉落、保护规则或存档，也不代表生产 Tiles
owner 已迁移；原世界文件不会被写入。当前阶段 AI 系统仍暂不实现。

## 组合入口

正式组合类型是 `NetworkGatewayHost`。以下 `frozenFacts`、`sessionAuthority` 和三个
handler 由宿主的实际领域实现提供；网关没有模拟世界或默认放行 handler。

```csharp
await using var host = new NetworkGatewayHost(
    IPAddress.Loopback, 7777, frozenFacts, sessionAuthority);
host.Gateway.Register(
    new PacketPolicy(6, NetworkSessionStage.AwaitPlayerData), worldRequestHandler);
host.Gateway.Register(
    new PacketPolicy(8, NetworkSessionStage.AwaitSectionRequest), sectionRequestHandler);
host.Gateway.Register(
    new PacketPolicy(12, NetworkSessionStage.Synchronizing), playerJoinHandler);
host.Start();
```

`Gateway` 在 Start 前允许注册；Start 封存策略，再启动监听。
`Profile`、`Snapshots`、服务端和 `Connections` 共享一个 `Budget`。
宿主自己选择监听地址；以上回环地址适用于本地联调。Dispose 依次停止监听/会话、
关闭网关、释放快照缓存。独立 AI 的真实 TCP 验收使用临时回环端口。

区段广播的 `OutboundDispatch` 明确设置 SectionSubscribers、WorldKey、
WorldGeneration、Section 和 SnapshotRevision 后才可复用缓存。缓存只接收 S2C
编号 10 的共享编码结果；普通广播不缓存。键还包含 profile、revision 和格式变体。
TryGet 返回防御性副本，TTL 在访问或 SweepExpired 时惰性清理；owner 世界切换时
调用 InvalidateWorld。预算满时缓存可以失效，网关回到普通编码发送。

客户端恢复使用 `ReconnectCoordinator`，connect 委托绑定
`host.Connections.ConnectAsync(address, port, timeout, token)` 或独立连接工厂。
每次 callback 得到新的 `ReconnectSession.Connection`，应用重新完成 Hello、授权、
世界同步后调用 MarkActive，并运行该连接的读取流程。被踢或主动退出返回 Stop；
允许恢复的断开返回 RetryAfterDisconnect。协议错误及其他永久错误终止。

默认最多 8 次尝试、单次 5 秒、失败窗口 60 秒，full jitter 从 200 ms 增至 10 秒。
仅 Active 持续 30 秒才清除失败历史；清理耗时不计入 Active。Stop 取消退避、连接和
使用等待，晚到的成功连接释放。协调器只运行一次，不保存 packet、密码或 HostToken。

编号 10 编码、解码和缓存生成共用单并发资格，避免多连接同时达到已有 codec 的
大工作区上限。该资格和帧字节预算不限制 writer 内部 MemoryStream 的精确物理容量；
严格工作区限额仍需生成器支持和工作集测量。
