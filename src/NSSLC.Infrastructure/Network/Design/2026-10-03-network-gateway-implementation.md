# 网络网关实施与交付报告

日期：2026-10-03（Asia/Shanghai）。用户授权按既有设计实施，测试交给其他 AI。
生产实现由主代理完成；所有构建、运行和测试由独立 `gateway_verification` AI 完成。
当前状态：基础设施已实现；独立 AI 最终构建与 57 组验证全部通过，交付文档已归档。

## 1. 实现范围

新增正式 net10.0 工程，固定 NetCoreServer 8.0.7，提供消息读写、服务端网关、
握手/身份/会话生命周期、权威路由、有界区段缓存、有限客户端重连及宿主组合入口。
应用端口独立于具体网络库，游戏状态继续由既有领域 owner 校验和提交。

| 交付边界 | 主要入口 |
| --- | --- |
| Application 端口与 DTO | [IPacketConnection](../../../NSSLC.Application/Network/IPacketConnection.cs)、[IPacketHandler](../../../NSSLC.Application/Network/IPacketHandler.cs)、[INetworkSessionAuthority](../../../NSSLC.Application/Network/INetworkSessionAuthority.cs) |
| 正式生产项目 | [NSSLC.Infrastructure.Network.csproj](../NSSLC.Infrastructure.Network.csproj) |
| 帧与类型读写 | [PacketConnection](../PacketConnection.cs)、[PacketBinding](../PacketBinding.cs)、[ProtocolProfile](../ProtocolProfile.cs) |
| 网关与会话 | [PacketGateway](../PacketGateway.cs)、[NetworkSession](../NetworkSession.cs) |
| TCP 适配与客户端工厂 | [PacketTcpServer](../PacketTcpServer.cs)、[PacketConnectionFactory](../PacketConnectionFactory.cs) |
| 共享快照与恢复 | [PacketSnapshotCache](../PacketSnapshotCache.cs)、[ReconnectCoordinator](../ReconnectCoordinator.cs) |
| 宿主组合 | [NetworkGatewayHost](../NetworkGatewayHost.cs)、[接入说明](2026-10-03-network-host-integration.md) |
| codec 生成 | [NetworkCodecGenerator](../../../../Build/Tools/NetworkCodecGenerator/Program.cs) |
| 独立验证 | [验证项目](../../../../Test/NSSLC.Infrastructure.Network.Verification/NSSLC.Infrastructure.Network.Verification.csproj)、[完整证据](../../../../Build/diagnostics/NetworkGatewayVerification.md) |

源码和工程没有写入、编译或引用 `NSSLC.Infrastructure/分类参考` 或历史原型程序集。
正式定义工程保留 `Terraria.NetWork.Prototype.PacketDesignCompiler.Sample` 命名空间，
这是其现有公开 packet 类型名称，不表示生产工程引用 Prototypes。

## 2. 协议与生成接入

从 `PacketAllDefinitions.CreateProtocol()` 生成全部 codec 与强类型 binding：165 种
布局、162 个编号、324 个实际方向键。56/69/85 的请求和响应保持独立类型。接收按
方向/ID 查找，发送按方向/精确类型查找；冲突和缺少稳定事实在启动时失败。

目录指纹：`AF3D72A78C28ADF1BA8662C63B60A102792A2B5217E29DE75E01813C7D72C888`。
ProfileKey 另包含事实版本；默认不同事实实例具有独立版本标识。编译器 schema=7、
manifest Version=4、wire Hello=Terraria319 分别维护。事实注册首次读取后封存，宿主
必须提供真实冻结的事实表和纯布局函数，测试夹具不作为生产默认数据。

生成工具位于 Build/Tools，输出位于 Build/generated/Network；通过自身 manifest
管理过期 `.g.cs`，不清理未知文件。NetWorkRoot 可通过 MSBuild 属性重定位，工程没有
硬编码 D 盘路径。编译输出和中间文件按仓库规则放入 Build/bin、Build/obj。

## 3. 收发与会话行为

帧为 UInt16 little-endian 总长、byte ID 和包体，总长 3–65535。接收 callback
立即复制借用数据，支持分片、多帧及半帧期限；邮箱满或额度不足关闭连接。
ReadPacketAsync 单读取者按需解码，保留完整封套。完整帧内部截断、尾随字节或准入
失败保留终止错误并释放后续队列；正常 EOF 和末尾半帧 EOF 排空先前完整消息。

接收数组使用 GC 拥有期，packet 不引用归还的池数组。取消在领取帧之前取消等待；
领取后继续交付，编号 10 等待大包资格也保持该边界。Dispose/致命关闭可终止其资格
等待并归还额度。读取错误包含 ID 和包体偏移。

WritePacketAsync 接受精确注册类型，编码后只添加一次帧头。发送采用同一 FIFO、
一次提交一帧；同步 OnSent 先累计，Submit 成功返回后才允许本地完成 receipt。
排队取消可以移除；提交后调用方取消不撤回字节。未提交为 NotSubmitted，提交但
未完整确认为 OutcomeUnknown，禁止静默重发。异常计数或不确定提交立即关闭。

编号 1/38 实现 Hello/密码流程，3 分配应用确认的槽位；161/139 的 host 授权和
82/Ping 由明确能力开关启用。密码与 HostToken 不记录或缓存。错误版本和认证拒绝
尝试限时发送通用 Kick，再关闭。未开放消息在昂贵解码前拒绝。

`IPacketHandler<TPacket>` 显式注册方向对应格式、阶段、模块/action、速率和 host
条件。Opaque、85、94、C2S TileSection，以及缺少语义证据的 Text/空模块不开放。
原矩阵是调查与 owner 映射依据，不是自动开启 162 个 gameplay handler 的名单。

Actor 来自绑定，包体声明原样交给 owner 进行字段语义判断。资料/区段/活动转换仅能
由 owner 对 6/8/12 的接受结果推进；实际发送完成不推断客户端同步完成。
关闭状态不可被迟到绑定、授权或业务结果重新激活；迟到准入 lease 单独撤销。

## 4. 权威输出、容量和缓存

只执行 owner 提交后的 OutboundDispatch。Single/ExplicitTargets 携带当前代次；
广播只选同一 GameSession 的 Active 目标；区段路由还验证世界代次及订阅投影。
发送者和目标在选择、入队及提交前复验，慢目标不会阻止正常目标开始发送。
整个消息处理有独立期限；持续微量发送进展也不能让网关无限等待。

| 项目 | 默认值与行为 |
| --- | --- |
| 网关会话/owner 并发 | 256；每会话一个有序读取/处理循环 |
| 每连接接收 | 128 KiB、64 项，callback 无法等待时关闭 |
| 每连接发送 | 512 KiB、128 个准入/排队项；等待准入调用另限 64 |
| 控制容量 | 发送总额度内 16 KiB/4 项；同 FIFO，小配置时缩小或关闭 |
| 共享网络字节 | 64 MiB，接收/编码帧/发送/缓存共用；多个宿主需共享同一预算对象 |
| 区段缓存 | 16 MiB、256 项、TTL 2 秒，LRU 与世界版本失效 |
| 大包编解码 | 每共享预算内编号 10 单并发 |
| 半帧无进展 | 5 秒；网关另有阶段/总体期限 |
| 发送 | 排队期限 10 秒；提交后无进展 10 秒关闭 |
| 会话期限 | 阶段 10 秒、握手总计 30 秒、同步 60 秒、Active 空闲 60 秒 |
| owner/清理 | 10 秒/5 秒；等待取消不回滚已提交领域效果 |

发送等待者是独立计数，默认最多 128+64=192 个进行中写入调用；不同于设计表中
“等待者计入 128 总项数”的建议，当前实现以两道有限上限分别管理，全部等待受期限约束。
控制预留只保证本连接容量隔离，不保证在全局额度耗尽或当前帧阻塞时 Kick 必定发出。

缓存仅接收明确共享的 S2C 编号 10 快照，键含 profile、世界 ID/代次、区段、revision
和格式变体。owner 的 SectionSubscribers 输出明确提供 SnapshotRevision 才启用。
缓存生成失败回退普通编码，不缓存密码、token、未知或私有消息。返回字节为独立副本；
TTL 惰性清理，可显式 SweepExpired/InvalidateWorld。新版本移除旧版本，拒绝逆向覆盖。
默认不合并 Sync，也不将排队缓存当作跨连接重放日志。

## 5. 连接恢复与副作用

客户端连接工厂每次创建新 adapter 和单调 epoch，等待 TCP OnConnected 才返回。
协调器最多 8 次、失败总窗口 60 秒、单次连接 5 秒；full jitter 从 200 ms 增至
10 秒，时间和随机源可注入。只有 Active 持续 30 秒才清除失败历史，清理不计入活跃时长。

应用 callback 在新连接重新握手、授权和同步后 MarkActive。临时 I/O/超时可恢复；
协议、配置、无关取消、明确 Stop 终止。停止后不能重启该协调器，晚到成功连接释放。
协调器不持有 packet、密码、HostToken 或旧 completion；游戏命令不自动重放。

领域提交先于输出；发送失败不回滚游戏状态。owner 不响应取消时继续占原并发槽位，
晚到业务输出丢弃。只把取消解释为结束网关等待，不承诺撤回对端处理或领域副作用。
Socket、时钟、随机、后台等待、缓存写入和资源释放集中在基础设施及应用端口边界。

## 6. 独立验收

最终受影响工程构建退出 0、0 警告/0 错误；测试退出 0，**57 组通过、0 组失败**。
执行者为独立 `gateway_verification` AI；主代理只核对证据及源码哈希，没有执行测试。
完整命令、逐类覆盖与源码/程序集快照见
[独立证据报告](../../../../Build/diagnostics/NetworkGatewayVerification.md)。

```powershell
dotnet build Test\NSSLC.Infrastructure.Network.Verification\NSSLC.Infrastructure.Network.Verification.csproj --no-restore --nologo -v:minimal
dotnet run --project Test\NSSLC.Infrastructure.Network.Verification\NSSLC.Infrastructure.Network.Verification.csproj --no-build --no-restore
```

实际输出分别是
`Build/bin/NSSLC.Infrastructure.Network/Debug/net10.0/NSSLC.Infrastructure.Network.dll`
和 `Build/bin/NSSLC.Infrastructure.Network.Verification/Debug/net10.0/NSSLC.Infrastructure.Network.Verification.dll`。
SDK=10.0.400；已查询 MSBuild 的 target/output/intermediate/generated 属性，均符合 Build 路径规则。

最终构建约 5.24 秒，验证进程约 4.40 秒；这些是正确性验证耗时，不是性能指标。
主代理只读核对的三个源码及生产程序集 SHA-256 与独立报告一致：

| 文件 | SHA-256 |
| --- | --- |
| PacketGateway.cs | `72E9F2E60A32D424F644BAAD3E24F27EA474AA79AAB323F4E8AA3CC6954E8D41` |
| NetworkSession.cs | `E998EBFDCC8DEB7E2A11F9875641AA0CC75D5629CF56B10DF13F323638707F0B` |
| PacketConnection.cs | `CF58D82B756145F430C67299225D50E664814B3598CCEA6D3B32E17BC5E9A549` |
| NSSLC.Infrastructure.Network.dll | `59B6646906A5B9C6E7DEAB098B72FEE97373D261F14B7B5E88CC58E6455275AB` |

验收覆盖完整帧和生成目录、同步回调、并发/取消/额度/EOF、身份伪造、默认禁用、
握手/模块/阶段期限、慢目标与旧代次、区段缓存、full jitter 和停止不复活，并通过
真实 NetCoreServer TCP 回环、生成报文网关链路及 NetworkGatewayHost 组合测试。
最后补测确认：已领取编号 10 后的调用方取消仍交付一次；正常 EOF 排空已领取和
排队帧；Dispose 在编解码资格被占用时也能终止等待并回收额度；致命关闭保留原始
协议错误并回收当前/队列帧。启动密码能力固定、模块/action 注册边界和包体偏移诊断
亦已验证。测试 AI 没有修改生产实现；最终验证之后只更新交付文档。

## 7. 实际限制与交付边界

1. 提供可组合的网络库和宿主入口；没有修改既有游戏宿主启动程序，也没有迁移全部
   Player、Projectile、Tiles、Chest、Inventory gameplay handler。宿主需以实际 owner
   和真实协议事实接入，未注册能力继续禁用。
2. 实际 Terraria 客户端、真实世界 owner 并发事务、长时负载、吞吐及物理工作集
   未验证。编译完整目录不证明所有 packet 的真实互通性。
3. 帧字节预算和大包单并发不能严格限制 writer 内部 MemoryStream/压缩工作区，
   64 MiB 不是进程物理内存上限；严格预算仍需生成器入口和工作集验收。
4. 原协议没有跨连接应用 ACK/幂等去重保证，网络库没有新增虚构恢复 token。
5. 保留两个仓库的既有修改，没有修复 NetWork 的损坏 worktree 元信息，也没有提交、
   合并或发布。正式源码与文档的回退边界为本次新增的 Network、Application/Network、
   生成工具和独立验证项目，不能通过广泛清理回退其他人的改动。
