# 网络领域抽取执行计划

**目标**：把网络相关的数据模型与传输实现从 ECS 组件领域（`src/NSSLC/Components/`）移到独立的网络领域程序集，领域层不再承载协议投影与 TCP 传输。

**决策记录（2026-11-02）**：

| 议题 | 结论 |
| --- | --- |
| 落点 | 新建 `src/NSSLC.Network/`，**不**复活 `src/NSSLC.Infrastructure/Network/` |
| 范围 | 含传输/编解码去重，不只是数据模型 |

## 执行记录

### 第 1 批（2026-11-02 完成）：切断泄漏 + 搬传输簇 + 搬投影子系统

**已完成**：

1. 三处泄漏全部切断：
   - 新增 `Components/Communication/InboundPacketFrame.cs`（领域自有载体：`MessageId` /
     `ModuleId` / `FrameBytes` / `Payload`），`InboundCommunicationPacket.Frame` 由
     `RawPacketFrame` 改为它，领域不再持有传输 DTO
   - `ConnectionClosed` 去掉 `LastReceivedSequence` / `LastSendCertainty`，只留 `(Epoch, Reason)`
   - `OutboundDelivery.cs` 只留 `OutboundIntentDelivery`（随 pump 搬走）；删除
     `OutboundDeliveryToken`、`OutboundDeliverySettlementResult`、`IOutboundDeliverySettlement`
     及 `OutboundPacketIntent.DeliveryToken`（4 个构造点全为 null 的未接线契约）
2. 21 个文件迁入 `src/NSSLC.Network/`，命名空间改为 `Terraria.Network`：
   - 传输/编解码 18 个：`PacketFrameCodec`、`PacketFrameAssembler`、`PacketFrameFormatException`、
     `PacketTcpConnection`、`PacketTcpConnectionOptions`、`TcpListenerAdapter`、
     `TcpListenerAdapterOptions`、`TcpCommunicationHost`、`ConnectionByteBudget`、
     `ICommunicationTransportPort`、`CommunicationOutboundDeliveryPump`、`InboundRawFrame`、
     `RawPacketFrame`、`SendReceipt`、`ProtocolErrorCode`、`TransportSendCertainty`、
     `CommunicationHostRuntime`、`OutboundDelivery`
   - 投影子系统 3 个：`NpcNetworkProjectionSystem`、`WorldNetworkProjectionSystem`、
     `NpcNetworkIdentityComponent`
3. `WorldItemRequestQueue` 由 `internal` 改为 `public`：`WorldNetworkProjectionSystem` 需要读该队列，
   而 `SingletonComponent<T>` 是 internal。依赖方向仍是网络 → 领域。
4. `NSSLC.Server.csproj` 与 `Terraria.NpcAi.MinimalVerification.csproj` 增加对
   `NSSLC.Network` 的引用；两处 `Program.cs` 补 `using Terraria.Network;`。

**为什么投影系统跟着组件一起搬**：`NpcNetworkIdentityComponent` 出现在
`NpcNetworkProjectionSystem` 的 `WithAll<>` 查询里（红线：QueryDescription 中的类型必须留领域）。
只搬组件会让领域反向依赖网络。把组件与其唯一消费者一起搬，方向才成立。

**验证**：`Terraria.Components`、`NSSLC.Network`、`NSSLC.Server`、
`Terraria.NpcAi.MinimalVerification` 四项目 0 错误；`worldprobe` 与 `phase0-before.txt` 逐字节一致；
`tickprobe` 8 行；`queueprobe` 49 PASS / 0 FAIL；`MinimalVerification` 通过（含 NPC 线投影断言）。
`Components/Communication` 由 40 文件降至 19。

### 第 2 批（2026-11-02 完成）：去重评估 —— 决定不做

**测量**：`NSSLC.Network` 与 `MinimalImplementation/Protocol` 的 10 个同名文件，归一化后
8 个完全相同、`InboundRawFrame` 差 1 行、`PacketTcpConnection` 差 9 行（对方多一个
`Version4PacketFormatException` 捕获）。这 8 个文件零领域耦合，是纯帧/字节层。

**结论：不做跨子系统去重。** 理由是两条路都会损害第 1 批的成果：

1. 让 `NSSLC.Network` 引用 `MinimalImplementation.Protocol` —— 刚抽出的网络域反向依赖旧子系统，
   把"抽离"变成"换个地方耦合"。
2. 新建中立帧层项目两边共享 —— 只能省 8 个文件，而 `PacketTcpConnection` 因两套栈各自持有
   独立的 `ConnectionClosed` / `SessionEpoch` / `SendReceipt` 类型，仍必须留两份；同时要把
   `ConnectionByteBudget`（类 + `SharedRaw` + `Lease`）、`PacketFrameAssembler.AppendAvailable` /
   `HasPartialFrameAfterAppend`、`PacketFrameCodec.ParseOwnedFrame`、`RawPacketFrame` 构造函数
   共 5 处 `internal` 提升为公开 API。

**保留的重复**：`NSSLC.Network` 与 `MinimalImplementation.Protocol` 是两套有意的独立实现，
`MinimalImplementation` 侧另有 8 个下游项目。重复已知、已记录，不再视为缺陷。

## 1. 为什么不用 `NSSLC.Infrastructure.Network`

它是 `8e11204 最小服务器完成` 这次提交刻意裁剪掉的架构残骸：

```
8e11204  5034 files changed, 174639 insertions(+), 344105 deletions(-)
```

同一次提交删掉了 `src/NSSLC.Application` 的 **170 个文件**，而 `NSSLC.Infrastructure.Network`
缺失的依赖正出自那里：

| 缺失类型 | 全仓声明数 | 在 Network 项目中的引用行数 |
| --- | --- | --- |
| `PacketHandlingResult` | 0 | 198 |
| `NetworkSessionContext` | 0 | 117 |
| `PacketDirection` | 0 | 67 |
| `ConnectionIdentity` | 0 | 22 |
| `SenderBinding` | 0 | 10 |
| `SessionAdmission` | 0 | 9 |
| `INetworkSessionAuthority` | 0 | 5 |

另有 3 个 `ProjectReference` 指向不存在的路径（`NSSLC/Component/Projectile`、
`NSSLC/Component/WorldSession`、`NSSLC.Application.csproj`），代码生成器 target 退出码 1，
且**没有任何项目引用它**、当前生产代码完全不使用它。

"修好它"等于从历史恢复 170 个文件复活旧架构，风险远高于新建。

## 2. 实测依赖关系（2026-11-02）

`Terraria.Components.csproj` 零 `ProjectReference`，是依赖图叶子。因此**领域不能引用网络**，
只有网络引用领域。

`Components/Communication/` 下 17 个候选文件的内部引用计数：

| 类型 | Components 内其它引用文件数 | 引用者 |
| --- | --- | --- |
| `PacketFrameCodec` | 4 | PacketFrameAssembler, PacketTcpConnection, PacketTcpConnectionOptions, RawPacketFrame |
| `PacketFrameAssembler` | 1 | PacketTcpConnection |
| `PacketFrameFormatException` | 3 | PacketFrameAssembler, PacketFrameCodec, PacketTcpConnection |
| `PacketTcpConnection` | 1 | TcpCommunicationHost |
| `PacketTcpConnectionOptions` | 5 | CommunicationHostRuntime, ConnectionByteBudget, PacketFrameAssembler, PacketTcpConnection |
| `TcpListenerAdapter` | 1 | TcpCommunicationHost |
| `TcpListenerAdapterOptions` | 3 | CommunicationHostRuntime, TcpCommunicationHost, TcpListenerAdapter |
| `TcpCommunicationHost` | 1 | CommunicationHostRuntime |
| `ConnectionByteBudget` | 1 | PacketTcpConnection |
| `ICommunicationTransportPort` | 2 | CommunicationOutboundDeliveryPump, TcpCommunicationHost |
| `CommunicationOutboundDeliveryPump` | 1 | CommunicationHostRuntime |
| `InboundRawFrame` | 3 | CommunicationHostRuntime, PacketTcpConnection, TcpCommunicationHost |
| `RawPacketFrame` | 5 | InboundCommunicationPacket, InboundRawFrame, PacketFrameAssembler, PacketFrameCodec |
| `SendReceipt` | 5 | CommunicationOutboundDeliveryPump, ICommunicationTransportPort, OutboundDelivery, PacketTcpConnection |
| `ProtocolErrorCode` | 3 | CommunicationOutboundDeliveryPump, PacketTcpConnection, SendReceipt |
| `TransportSendCertainty` | 4 | CommunicationOutboundDeliveryPump, ConnectionClosed, PacketTcpConnection, SendReceipt |
| `CommunicationHostRuntime` | 0（仅 `NSSLC.Server/Program.cs:133,270` 使用） | — |

**结论：不存在可单独安全搬走的文件。** 帧层（`PacketFrameAssembler`）反向引用传输层
（`PacketTcpConnectionOptions`），而三个 DTO 被 ECS 侧持有：

| 泄漏点 | ECS 侧文件 | 处理方式 |
| --- | --- | --- |
| `RawPacketFrame` | `InboundCommunicationPacket.cs:6` | 改为持有原始字节，帧解码留在网络侧 |
| `SendReceipt` | `OutboundDelivery.cs:53,58,63` | 见下方 `OutboundDelivery.cs` 拆分说明 |
| `TransportSendCertainty` | `ConnectionClosed.cs:8,14` | 拆成领域事件 `(Epoch, Reason)`，传输细节留在网络侧 |

**`OutboundDelivery.cs` 不能整体删除**（2026-11-02 复核修正）：该文件含 4 个类型，
存活状况不同：

| 类型 | NSSLC 侧外部引用 | 处理 |
| --- | --- | --- |
| `OutboundIntentDelivery` | 5 处，全部在 `CommunicationOutboundDeliveryPump.cs` | 随 pump 一起搬入网络领域 |
| `OutboundDeliveryToken` | 2 处，仅 `OutboundPacketIntent.DeliveryToken` 属性声明 | 该属性在全部 4 个构造点均为 null，属未接线契约，与属性一并删除 |
| `OutboundDeliverySettlementResult` | 0 | 删除 |
| `IOutboundDeliverySettlement` | 0（`MinimalImplementation.Contracts` 里的同名类型是另一套，不受影响） | 删除 |

## 3. 执行顺序

1. **拆泄漏**（前置，必须先做完才能搬任何文件）
   - 按上表拆分 `OutboundDelivery.cs`：搬 `OutboundIntentDelivery`，删其余 3 个类型与
     `OutboundPacketIntent.DeliveryToken`
   - `ConnectionClosed` 去掉 `LastReceivedSequence` / `LastSendCertainty`
   - `InboundCommunicationPacket.Frame` 改为 `ReadOnlyMemory<byte>`，同步改
     `CommunicationSessionIngress` 的解码调用点
2. **整体搬传输簇**到 `src/NSSLC.Network/`，命名空间 `Terraria.Communication` → `Terraria.Network`
3. `NSSLC.Server.csproj` 增加对 `NSSLC.Network` 的引用
4. **搬数据模型**：`NpcNetworkIdentityComponent`（4 处引用，见下）等纯协议投影
5. **去重**：`Components/Communication/` 的 10 个契约文件与 `MinimalImplementation/Contracts/`
   逐字节相同、只差 namespace；`PacketTcpConnection` 等与 `MinimalImplementation/Protocol/`
   相同。需先决定两套栈哪套活，再删重复。

## 4. 数据模型搬迁清单

| 文件 | 判定 | 依据 |
| --- | --- | --- |
| `Npc/NpcNetworkIdentityComponent.cs` | **移走** | 字段 `Slot`/`Generation`/`NetId` 全是客户端索引与线格式，零领域读取者；注释自证 "protocol projection only" |
| `Share/Item/ItemIdRules.cs` | 移走（`*NetworkType` 两个方法） | 纯协议宽度适配，全仓零调用点（死代码） |
| `Player/PlayerInventoryComponent.cs` | 留领域 | 承载权威库存，但注释与字段需清理（`Favorited` 是客户端 UI 标志） |
| `Player/PlayerInputRequest*.cs`（5 个） | 留领域 | 域外写领域状态的命令端口，无线格式字段 |

`NpcNetworkIdentityComponent` 全部引用点：`NpcNetworkProjectionSystem.cs:34,93,238`、
`NSSLC.Server/Program.cs:465`、`Test/Terraria.NpcAi.MinimalVerification/Program.cs:171`。

## 5. 红线：搬走即造成反向依赖

以下类型出现在 ECS 系统构造参数或 `QueryDescription` 中，**必须留在领域程序集**：

`CommunicationSessionComponent`、`CommunicationInboundMessageBufferComponent`、
`CommunicationOutboundMessageBufferComponent`、`CommunicationSessionStage`、`SessionEpoch`、
`PacketSequence`、`OutboundPacketIntent`、`InboundCommunicationPacket`、
`InboundPacketEnvelope`、`CommitVersion`、`ICommunicationOutboundPort`。

两个最易踩的点：

- `SessionEpoch` 已被 World 域 4 个文件使用（`WorldItemState.ReservedFor`、
  `WorldItemRequest.Actor`、`WorldItemStore`、`WorldItemRules`）。把它当网络模型搬走会
  连带把 World 域拖进基础设施依赖。
- `ICommunicationOutboundPort`（领域定义、必须留）与 `ICommunicationTransportPort`
  （两端都在宿主侧、可移）名字对称、生命周期相邻，但依赖方向相反。

## 6. 验证

每个批次后跑：`Terraria.Components`、`NSSLC.Server`、`NSSLC.Network` 三项目构建 0 错误；
`Build/worldprobe` 195 行、`Build/tickprobe` 8 行与基线逐字节一致；
`Build/queueprobe` 49 项断言全通过；`Test/Terraria.NpcAi.MinimalVerification` 通过。

**构建告警口径（2026-11-02 实测修正）**：增量构建下 `NSSLC.Server` 报 0 警告，
但 `-t:Rebuild` 重建依赖闭包后会出现 **22 个警告**，全部来自依赖项目
（`NSSLC.WorldGeneration`、`MinimalImplementation.World` 的 CS8602/CS8632/CS0108/CS0219 等），
与本次改动无关且为既有问题。`Terraria.Components` 与 `NSSLC.Network` 本身 0 警告。
后续验收应使用 `-t:Rebuild` 口径，避免把"依赖未被重编"误读成"零警告"。

**已知缺口**：TCP 与协议栈在 NSSLC 侧零测试覆盖（`MinimalImplementation.Verification.Communication`
测的是另一套实现）。搬迁前没有行为等价基线，第 2 步之后只能靠构建通过判断，风险需记明。
