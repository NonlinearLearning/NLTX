# 消息读写 API 与生产文件布局

日期：2026-10-03。状态：提案；代码块为契约示例，尚未编译或实施。关联：[网关模型](2026-10-03-network-domain-and-gateway.md)、[缓冲与重试](2026-10-03-buffer-connection-retry.md)。

## 1. 选择的接口形状

当前生产 packet 是普通 C# 值类型或类，不依赖历史原型 `INetPacket`。建议以普通 packet 值写入，以包含协议身份的 `PacketMessage` 读取；通过启动时的强类型 binding 统一不同生成 reader/writer。

以下类型均为拟新增。`IPacketConnection`/`PacketMessage`/`PacketWriteReceipt` 是应用需要的稳定端口与 DTO，放在 Application 的 Network 能力目录；具体 `PacketConnection`、工厂、网关及 NetCoreServer adapter 按用户指定放在 `NSSLC.Infrastructure/Network/`。这是内层拥有端口的现有 NLTX 规则；不要求 Application 引用具体网络库。

```csharp
public interface IPacketConnection : IAsyncDisposable
{
    ValueTask<PacketMessage?> ReadPacketAsync(
        CancellationToken cancellationToken = default);

    ValueTask<PacketWriteReceipt> WritePacketAsync<TPacket>(
        TPacket packet,
        CancellationToken cancellationToken = default);
}
```

`PacketMessage` 对外只读，包含 ProfileKey、ActualDirection、MessageId、SessionKey、Epoch、InboundSequence，以及解码出的 `object Payload`；提供 `Get<TPacket>()` 校验并取值。稳定端口的方向值只表达 ClientToServer/ServerToClient，由 Infrastructure 显式映射当前编译器的 WireDirection；Application 不为该枚举引用带 Roslyn 的编译器工程。构造由已注册 binding 完成，业务调用者不能任意组合 ID、方向和 payload。`PacketWriteReceipt` 包含 Epoch、OutboundSequence、MessageId、FrameBytes，表示整帧在本地 Socket 发送完成；没有对端接收、业务确认或幂等重试含义。

`WritePacketAsync<TPacket>` 依据固定的发送方向和 `typeof(TPacket)` 查找 binding；由 binding 提供消息 ID。实际对象类型必须与注册类型一致，禁止以 `object`/基类静默绕过类型绑定。对于运行时封套的权威转发，内部 binding 直接分发已知具体类型，不增加一个开放的“任意 ID＋object”发送入口。

调用形状如下；`responsePacket` 是当前包模型的普通值，并非要求改造为公共基类：

```csharp
PacketMessage? message = await connection.ReadPacketAsync(cancellationToken);
if (message is not null)
{
    Packet20Packet packet = message.Get<Packet20Packet>();
    // 向应用提交已准入的消息；此处不直接写世界状态。
}

PacketWriteReceipt receipt =
    await connection.WritePacketAsync(responsePacket, cancellationToken);
```

服务端会话只由网关拥有一个读取循环。业务 owner 消费应用命令或权威 packet，不与网关竞争 `ReadPacketAsync`。客户端或工具可以直接拥有一个 `IPacketConnection` 并使用该接口。

## 2. 明确读写契约

| 情况 | 选定的对外语义 |
| --- | --- |
| 正常读取 | 等待一个完整帧，完成格式/长度校验，返回独立拥有内容的 PacketMessage |
| 正常 EOF | 已完成帧先排空，然后返回 null；Null 只表示干净 EOF |
| EOF 时有半包 | `PacketProtocolException(IncompleteFrameAtEof)`；记录长度和偏移，不拼入下次连接 |
| 无效长度、方向或未知格式 | PacketProtocolException；未知格式不能默认解释为空体 |
| 已完整帧但 reader 返回 Truncated/InvalidData | 协议错误；复用现有 PacketReadError，不从下一帧补字节 |
| 取消读取 | OperationCanceledException；取消等待不移除未交付帧、不关闭正常连接；交付与取消的竞争以一次原子领取为界 |
| 并发读取 | 第二个活跃读取立即 InvalidOperationException；无需依赖 Channel 的优化提示证明单消费者 |
| 编码约束失败 | `PacketEncodingException`；TryWrite=null 映射为 EncodingRejected，不伪造字段级写入诊断 |
| 写入成功 | 等待当前 Epoch 的完整帧累计 OnSent；返回本地发送 receipt |
| 写入开始前取消 | 从未进入库发送缓冲的项可移除，抛取消；无字节属于该项被发送 |
| 已提交给库后取消 | 调用方取消不再撤回该帧；继续完成，成功返回 receipt；内部发送期限届满则关闭连接并报 OutcomeUnknown |
| 断线或发送失败 | 已提交但未完整确认的项报 OutcomeUnknown；仍在网关队列未提交的项报 NotSubmitted |
| DisposeAsync | 禁止新准入，终止生命周期，等待有界清理；每个 waiter 和每份内存恰好完成/释放一次 |

并发写入允许，但顺序定义为获得发送准入序号的顺序；跨线程方法调用开始时间不是排序保证。发送队列的每个完整帧字节连续，不能交叉。调用方在 WritePacketAsync 完成前不得修改传入 packet 及其可变数组/集合；网关获得额度后编码为独立字节，入队后不再保留可变 packet 引用。

业务准入失败可以由网关丢弃、返回受控拒绝或关闭；不把它伪装成 codec 错误。底层连接读取可报告方向/格式错误，握手权限判断则归会话协调器；服务端路径使用内部完整帧元数据先做准入，避免为 C2S 禁用的 Packet10 触发解压。

## 3. 完整帧及目录

帧结构是 `[UInt16 LE totalLength][byte MessageId][body]`，totalLength 包含 3 字节头。合法总长 3–65535，包体最多 65532；见 [PacketFrameLimits](D:/ProjectItem/SourceCode/Net/NetWork/src/Packets/PacketModels.cs:5)。使用 BinaryPrimitives 显式读写 little-endian，不依赖 BitConverter 的机器端序。

拆帧步骤：

1. 不足两字节长度头时保留已接收字节；累计及时间额度仍生效。
2. 读 totalLength，若小于 3 立即拒绝；在协议配置允许的最大帧长内等待。
3. 达到完整 totalLength 后切出一个窗口，byte[2] 为 ID；一次 callback 可以产生多帧。
4. 按 ProfileKey＋实际方向＋MessageId 找唯一格式，再传 body=frame[3..]。
5. 新建绑定该 body 的 reader，只调用一次 `ReadFrameDetailed()`；错误消费量为 0，不提交无效消息。
6. 无论成功与否，完整帧窗口的资源在规定位置释放；无效帧之后不继续尝试字节搜索“重同步”。

稳定事实绑定在 profile/binding 创建时，不能由每个包临时查询可变世界或权限。协议事实对象虽 getter-only，现有生成 reader/writer 保存的只是引用；宿主应先防御性复制需要的数组/表，封存后不再变更。

调查期间共享工作区增加了 schema=7 的 WireFormat/Sequence/Layout/Constructor 组合能力；见[当前基线](D:/ProjectItem/SourceCode/Net/NetWork/CONTEXT.md)和[组合格式设计](D:/ProjectItem/SourceCode/Net/NetWork/docs/plans/2026-10-03-composable-packet-formats-design.md)。它们仍只处理包体，不改变此处完整帧和网关职责。binding生成须纳入Dependencies.Formats的嵌套事实映射，并使用实际生成factory；当前结构化嵌套writer会先缓冲元素再复制，也要计入编码工作区预算。移交报告中的schema=6是此前快照，不是对端版本。

格式注册有两套索引：接收 `(actualDirection, id)`；发送 `(actualDirection, packetType)`，统一属于同一选定 profile。Bidirectional 扩展为两个实际方向键；和单向声明碰撞必须在启动时报错。不同发送类型占相同方向＋ID、不同 profile 的 fact 混用均拒绝。格式清单存在不表示启用许可，仍须经过 PacketPolicy。

56/69/85 的请求与响应类型不合并。85 的响应空体与请求 bool 仅是本地裁剪快照事实，能力开放保持禁用，直到真实双方协议样例证明。

## 4. 生成 codec 的桥接

`PacketBinding<TPacket>` 拟为 Infrastructure 内部小接口后的实现；保存 descriptor、稳定依赖、具体读写委托和必要的内存复制规则。只在启动时强类型注册，热路径不反射查找方法名、不使用 dynamic，也不从历史原型注册表恢复未知类型。

以下是手工强类型注册的接入示意，不是已有 Register API：

```csharp
var dependencies = new Packet20PacketDependencies(facts: factsValue);

bindings.Register<Packet20Packet>(
    messageId: 20,
    actualDirection: WireDirection.ClientToServer,
    decode: body => new Packet20PacketPacketCodecReader(body, dependencies)
        .ReadFrameDetailed(),
    encode: packet => new Packet20PacketPacketCodecWriter(dependencies)
        .TryWrite(packet));
```

生成名称在传入 `ProtocolManifest` 时会带上协议注册前缀；上述 Packet20 名称只对应移交报告的独立编译路径。不能手抄示例名称用于另一生成模式。`PacketAllDefinitions.CreateProtocol()` 目前构造 Name=`TerrariaV4`、Version=`4`；[ProtocolBackend](D:/ProjectItem/SourceCode/Net/NetWork/src/ProtocolBackend.cs:24) 会生成每个注册项的 Descriptor、CreateReader/CreateWriter。未来优先由 manifest 生成 binding 表，沿用真实 Artifact 名称，并验证与 descriptor 一致。

写入桥接顺序为：获得最大帧容量预留 → writer.TryWrite(packet) → 若 null 则释放预留并拒绝 → 验证 body.Length ≤65532、Position=0 → 分配/租用精确帧并写一次头和 body → 释放 writer 返回流 → 调整额度为实际帧长 → 有界入队 → 等待发送 receipt。任何编码失败都发生在该帧提交给 NetCoreServer 之前。不能把包体 MemoryStream.Position 当作长度，也不能再让 MessageFrame.Writer 写一次重复头。

最大帧预留限制等待编码与排队数量，不会自动限制现有 writer 的内部 MemoryStream。当前 [生成器](D:/ProjectItem/SourceCode/Net/NetWork/src/CSharpBackend.cs:652) 创建自己的流，只有部分字段/codec窗口长度约束；全包65532限制仍需桥接检查。严格编码内存限额的后续改动应为生成 writer 增加可选的构造期包体预算，并在分配、普通字段和codec写入前执行全包额度，继续保留 TryWrite(packet) 方法形状；这是一项拟实施的生成器接入任务，当前 API 尚无该入口。压缩前工作区另行限额，不能由压缩后帧长约束替代。

读对象不应引用归还到 pool 的帧内存。常规字段生成对象自然拥有其数组；含 ReadOnlyMemory/Opaque 的 binding 必须复制保留片段，或明确深复制为消息拥有的数据，再释放帧租约。ReadOnlyMemory 不证明不可变或独立拥有。若未来增加 lease API，应另外设计显式释放契约，本次不让业务调用者释放网络帧。

## 5. 生成源码和项目接入

现有 `PacketDesignCompiler.Compile` 返回 `CompilationResult.Artifacts`；引用 `src/Packets` 只获得定义和值类型，不自动获得编译后的生成 reader/writer。本轮没有生成或编译新的产物。

建议将生成接入独立成一个受控步骤：

1. 在 NetWork 构建流程编译已冻结 manifest，带目录指纹输出 packet codec 和 binding 源文件。
2. 用一个真实生成项目/程序集显式编译产物，引用现有 packet 值与 compiler runtime，不引用 Prototypes。
3. NLTX 通过正式 ProjectReference 或版本化包引用生成程序集；首次联调可以用显式可配置外部 ProjectReference，不在 csproj 写死开发机 D 盘绝对路径。
4. 所有 NLTX 生成源码、中间文件和输出遵循 `Build/generated/`、`Build/obj/`、`Build/bin/`；生产项目不得通配包含 `分类参考/`。
5. 启动验证 profile 指纹、方向键、packetType、事实完整性、reader/writer 能力和启用 policy 的 handler 是否齐备；缺失即启动失败。

ProfileKey 应体现快照/协议事实版本与目录指纹；目前 `TerrariaV4`/`4` 是目录标识，真实握手 `Terraria319` 应独立检查，不能由 Version4 文件夹名推导互通性。

## 6. 未来文件清单

本轮只建立 Design 下 Markdown。下列文件按职责分批建立，不预建空目录；一个核心公开类型一个同名文件。

| 拟路径 | 职责 |
| --- | --- |
| `NSSLC.Infrastructure/Network/NSSLC.Infrastructure.Network.csproj` | net10.0 正式工程，固定库版本与显式编译边界 |
| `Network/PacketConnection.cs` | 对外两个读写方法和 DisposeAsync；封装帧、binding 与发送 waiter |
| `Network/PacketConnectionFactory.cs` | 创建客户端连接、绑定 profile、超时与重试选项 |
| `Network/PacketGateway.cs` | 接入、会话注册、目标分发和停止；不直写 ECS |
| `Network/NetworkSession.cs` | 当前 Epoch、准入阶段、绑定投影、截止时间及有序事件 |
| `Network/PacketFrameAssembler.cs` | 完整帧判断、半包累积及窗口拥有期 |
| `Network/PacketBinding.cs` | 生成 codec 的内部类型擦除和准确错误映射 |
| `Network/ProtocolProfile.cs` | 稳定事实、目录和运行时索引 |
| `Network/PacketSendQueue.cs` | 字节＋项数额度、串行提交、取消及发送 completion |
| `Network/PacketSnapshotCache.cs` | 如启用区段帧缓存，集中处理世界修订键、失效与 LRU |
| `Network/NetCoreServerSessionAdapter.cs` | TcpSession 继承、回调转 owned event、累计 OnSent |
| `Network/NetCoreServerClientAdapter.cs` | TcpClient 继承、异步生命周期、新代次隔离 |
| `Network/ReconnectCoordinator.cs` | 仅发起连接的有限重试与停止 |
| `NSSLC.Application/Network/IPacketConnection.cs` 等 DTO 文件 | 内层需要的稳定读写端口；不引用 NetCoreServer |
| `NSSLC.Application/Network/NetworkSessionCoordinator.cs` | 握手用例、消息许可、领域调用与权威输出意图 |
| `NSSLC.Application/Network/PacketPolicy.cs` | 显式方向/阶段/模块许可；复用协议编号，不复制 enum |
| 宿主的 `NetworkComposition.cs`（按实际宿主位置确定） | Profile、owner mapper、NetCoreServer adapter 和应用绑定 |
| `Test/NSSLC.Infrastructure.Network.Verification/` | 未来独立验证入口；本轮不创建或运行 |

端口位置并不要求把所有网络内部 seam 公开。内部队列、计数器、frame 租约和 retry timer 保持 Infrastructure 私有；不为单一实现额外建立五套可替换抽象。内层处理收到的 payload 时，由组合入口注册具体 packet→现有 owner command 的强类型映射；Application 契约不为引用编译器的 Sample 命名空间而反向依赖生成器。

## 7. 接入兼容及验证目标

保留现有包体字节和生成 codec 契约；不修改普通 packet 为 INetPacket，不把 world 状态塞入 dependencies。既有 Player/Projectile 部分拥有自己的 packet codec 端口，接入时建立明确 adapter 和 mapper，逐步让生成 codec 作为其实现；不能一轮删除旧入口或重复解析同一 body。

未来验证覆盖：长度=3/65535、分片及多帧、完整帧内截断、尾随字节、56/69 方向类型、85 禁用、opaque 拒绝、值类型读取失败、事实对象隔离、写 null、重复 reader、并发写帧不交错、发送完成计数、取消边界、EOF 半包、旧 Epoch 回调、慢目标隔离。准确构建/测试命令及输出须在实施时记录；这些是验收目标，当前没有运行结果。
