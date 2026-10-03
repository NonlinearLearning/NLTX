# 宿主接入网络网关

日期：2026-10-03。组合入口已通过独立 AI 的真实 TCP 验证；最终 57 组验证及接入限制见实施报告。

宿主引用 `Network/NSSLC.Infrastructure.Network.csproj`。默认 `NetWorkRoot` 从工程位置
相对查找 `ProjectItem/SourceCode/Net/NetWork`；其他目录结构通过 MSBuild 属性配置。
生成工具从正式 manifest 生成全目录，产物写到 `Build/generated/Network`，只按自身
manifest 清理过期文件。生产项目不引用 `分类参考` 或历史原型程序集。

## 协议事实

创建 `ProtocolFacts`，为 10、20、23、27、72、82、86 的两个实际方向绑定名为 `facts`
的具体事实对象。宿主必须提供真实冻结的 tile frame-important、tile-entity、NPC
life-width、projectile UUID 和模块布局资料；测试中的零表或固定委托只是夹具。

`TerrariaProtocolProfile.Create(facts)` 在启动时解析全部依赖，缺失即失败。第一次 Get
冻结 Bind 注册，稳定事实对象本身也必须保持不变。默认 facts 版本为独立实例标识，
宿主可显式提供已管理的事实版本。ProfileKey 包含目录指纹和该版本，wire Hello 独立
检查 `Terraria319`。`Sample` 是正式定义工程保留的命名空间，不表示引用 Prototypes。

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

生产接入尚需真实客户端序列、实际领域 owner、负载与物理工作集验证。完整格式目录
不表示全部 162 个消息已经实现 gameplay handler；未注册处理器、opaque、85、94 和
尚无语义证据的模块仍禁用。

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
