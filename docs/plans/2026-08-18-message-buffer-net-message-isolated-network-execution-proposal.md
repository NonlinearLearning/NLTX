# MessageBuffer / NetMessage 隔离网络执行提案


**Goal:** 在不改变外部 V1456 网络帧与命令式调用形状的前提下，将旧版 `MessageBuffer`/
`NetMessage` 纳入可追溯的协议基线，并通过隔离层把 socket、Dome tick、服务器命令和只读网络
对象切片连接起来。

**Architecture:** 旧版源文件只作为排除编译的行为 oracle；协议项目提供帧重组、出站 envelope、
有序 `List` 和不可变切片，服务器在 tick 边界 drain 入站并在 tick 末 flush 出站。Dome 只接收
命令式意图、产生快照/切片；尚未映射的消息显式返回 `Unsupported`。

**Tech Stack:** C# / .NET 10、`Terraria.Dome.Protocol.V1456`、`Terraria.Dome.Server`、
`Terraria.Dome.Simulation`、现有 V1456 Frame/Codec/Session、串行 `dotnet build/run` 验证。

---

> 执行状态：隔离路径首批范围已落地并在 2026-08-19 重新回归；当前提案仍为“部分完成”。
> 本文同时保留原始设计边界、执行记录和未完成的兼容范围；“Complete”只表示本节列出的
> 范围已由命令和产物验证，不表示旧版 162 个消息分支或完整客户端 bootstrap 全部迁移完成。

## 1. 目标与交付边界

将以下两个已被物理删除的旧版网络实现纳入当前 NLTX 项目的网络通信协议迁移基线，并建立
`DomeServer` 与旧版网络协议之间的隔离层：

- `D:\TRbackup\Version4物理删除了某些文件\Terraria\MessageBuffer.cs`
- `D:\TRbackup\Version4物理删除了某些文件\Terraria\NetMessage.cs`

目标不是把旧版 `Terraria` 对象图重新引入服务器，而是保留其消息分支和编码行为作为兼容
基线，使用命令式 API 连接两侧：

```text
客户端字节流
  -> 现有 Frame/Codec/Session
  -> 协议隔离层的入站 List
  -> DomeServer.Update 统一读取并转换为命令
  -> DomeSimulation / DomeServer 执行
  -> 简单对象切片 / 快照
  -> 隔离层的出站 List
  -> 现有 Frame/Codec/Session 写回客户端
```

原始提案阶段只覆盖设计、文件边界、执行顺序和验收标准。当前工作树已经按下方执行记录落地
首批隔离实现；后续扩展仍必须遵守本文的停止条件和回滚边界。

## 2. 已验证的现状

| 证据 | 结果 | 对执行的约束 |
| --- | --- | --- |
| 旧版 `MessageBuffer.cs` | 3,365 行、约 90 KB、208 个 `case` 分支；核心入口为 `GetData`、`Reset`、`ResetReader`、`ResetWriter` | 必须拆出解析职责和 Dome 业务职责，不能在一个方法内恢复旧对象调用 |
| 旧版 `NetMessage.cs` | 2,721 行、约 77 KB、154 个 `case` 分支；包含 `TrySendData`、`SendData`、`ReceiveBytes`、`CheckBytes`、压缩和同步方法 | 发送、接收、压缩、广播和对象同步必须分阶段迁移，并保留未支持分支的显式状态 |
| 旧版依赖 | 直接引用 `Main`、`Netplay`、`Player`、`NPC`、`Projectile`、`Item`、`WorldGen`、`TileEntity`、XNA 类型和旧版本地化/音频类型 | 不复制 `Main.cs`、`Player.cs`、`Netplay` 或 XNA 运行时；只切片网络所需字段 |
| 当前协议项目 | `src/Terraria.Dome.Protocol.V1456` 已有 `TerrariaFrame`、`TerrariaPacketCodec`、`TerrariaSession`、`TerrariaPacketDispatcher`、162 个消息 ID 的目录 | 外部帧格式、消息 ID、Codec 和 Session API 默认不改，隔离层在其后方接入 |
| 当前服务器 | `TerrariaProtocolSessionHost` 负责 socket 读写，`DomeServer` 在每 tick 处理 `_protocolCommands`，随后运行 Simulation 和复制 | 网络线程只入队；Dome tick 统一 drain；不得由 socket 线程直接修改 Simulation |
| 当前工作树 | 存在大量未提交的用户/生成文件 | 本任务只新增本提案及后续明确列出的文件，不回滚或整理无关 diff |

## 3. 架构决策

### 3.1 复制基线与编译边界

第一步将两个源文件原文复制到协议项目的证据目录，保留原始路径、字节长度、SHA-256、复制
时间和版本假设：

```text
src/Terraria.Dome.Protocol.V1456/LegacyReference/
  MessageBuffer.cs
  NetMessage.cs
Build/diagnostics/message-buffer-net-message-source-manifest.json
```

`LegacyReference/*.cs` 通过协议项目的 `<Compile Remove=...>` 排除，不参与编译；它们是行为
oracle 和审计材料，不是生产依赖。这样满足源文件复制和可追溯性，同时不会把完整旧版对象图
带入 `net10.0`。

### 3.2 编译隔离层

在协议项目增加以下隔离类型，使用不同命名空间避免与旧版 `Terraria.MessageBuffer` /
`Terraria.NetMessage` 冲突：

```text
src/Terraria.Dome.Protocol.V1456/Isolation/
  MessageBuffer.cs
  NetMessage.cs
  DomeNetworkIsolation.cs
  NetworkInboundEnvelope.cs
  NetworkOutboundEnvelope.cs
  NetworkCommand.cs
  NetworkObjectSlices.cs
  NetworkWorldSlice.cs
  NetworkTileEntitySlice.cs
```

建议命名空间为 `Terraria.Dome.Protocol.V1456.Isolation`。编译版 `MessageBuffer` 和
`NetMessage` 保留旧版最重要的命令式调用语义，但不再读取全局 `Main` 或写入 socket：

- `MessageBuffer`：管理单会话的帧拼接、长度校验、读取游标和入站 envelope 生成。
- `NetMessage`：把 `TrySendData`、`SendData`、`SendTileSquare`、`SendSection`、
  `BootPlayer` 等调用转换为出站 envelope；不直接编码未通过 `TerrariaPacketCodec` 的帧。
- 旧版分支无法在当前 Dome 中表达时返回 `Unsupported` 结果并记录消息 ID，禁止静默丢弃。

### 3.3 服务器实现边界

协议项目只定义端口和不可变数据；服务器项目实现端口：

```text
src/Terraria.Dome.Server/Protocol/
  DomeNetworkIsolationAdapter.cs
  DomeNetworkUpdateBridge.cs
```

服务器适配器引用 `DomeServer` 已有的命令队列、复制状态和 Simulation 快照创建 API。
协议项目不得反向引用 `Terraria.Dome.Server`，避免项目循环依赖。

## 4. 隔离层数据结构与命令式 API

### 4.1 有序 List 队列

`DomeNetworkIsolation` 使用明确的 `List<T>` 存放跨线程边界的数据；列表只由隔离层拥有，
调用方只能获得 drain 后的只读视图或副本：

```csharp
public sealed class DomeNetworkIsolation
{
  public bool EnqueueInbound(NetworkInboundEnvelope envelope);

  public int Update(IProtocolCommandSink commandSink);

  public void EnqueueOutbound(NetworkOutboundEnvelope envelope);

  public IReadOnlyList<NetworkOutboundEnvelope> ReadOutbound();

  public void ClearOutbound();
}
```

实现要求：

1. 入站、出站和命令列表分别维护，不用一个无类型 `object` 列表承载所有状态。
2. 每个 envelope 带单调递增 `Sequence`、`PlayerSlot`、消息 ID、载荷副本和来源方向。
3. socket 线程只在锁内追加；`Update` 在一个确定性边界内交换列表引用后再处理，避免长时间
   持锁执行 Dome 逻辑。
4. `ReadOutbound` 返回 `IReadOnlyList`；写 socket 成功后由服务器显式 `ClearOutbound`。
5. 队列容量、丢弃策略和关闭状态可观测；容量溢出返回 `false` 并产生可断言的诊断事件。

### 4.2 入站流程

`TerrariaProtocolSessionHost` 保持现有外部协议和帧读取方式，只将已读帧交给隔离层：

1. `MessageBuffer.ReceiveBytes` 校验长度前缀和单帧边界。
2. `TerrariaPacketDispatcher` / `TerrariaSession` 继续执行消息 ID、方向、会话状态和字段校验。
3. 通过 `EnqueueInbound` 写入隔离层列表；不得在 socket 线程调用 Simulation。
4. `DomeNetworkUpdateBridge` 在 `DomeServer.Update` 的协议命令阶段调用 `Update`，按序生成
   现有 `ApplyPlayerControlCommand`、`ApplyTileManipulationCommand`、`OpenChestCommand`、
   `ToggleDoorCommand`、`UpdateSignCommand` 等命令。
5. 需要同步应答的 bootstrap、sign-open、world-data、section 请求使用带完成句柄的 envelope；
   完成句柄只能携带简单对象或编码后的帧，不能泄露 `DomeSimulation` 可变引用。

### 4.3 出站流程

1. `DomeServer` 和复制系统从 Simulation 创建不可变切片。
2. `NetMessage` 将切片交给现有 `TerrariaPacketCodec` 编码，并通过 `EnqueueOutbound` 写入列表。
3. `DomeNetworkUpdateBridge` 在 tick 结束的统一发送阶段按 `Sequence`、会话和目标客户端分组。
4. `SessionReplicationState` 继续负责实际 `NetworkStream.WriteAsync`；隔离层不持有 socket。
5. 编码失败、目标会话不存在或客户端被忽略时产生明确的结果枚举和计数。

## 5. 简单对象切片

切片只保留网络协议确实读取或发送的字段；源对象用于一次性映射，不成为隔离层引用。优先
复用当前已有快照，新增类型放在 `NetworkObjectSlices.cs` 或按核心类型拆成独立文件。当前
World 和 TileEntity 已拆为独立文件：

| 切片 | 来源对象/现有快照 | 首批字段 |
| --- | --- | --- |
| `NetworkPlayerSlice` | `PlayerSnapshot`、`PlayerStateSnapshot`、`LegacyPlayerControlsState` | slot、active、name、position、velocity、life/mana、selected item、controls、zone、loadout、buffs |
| `NetworkNpcSlice` | `NpcReplicationSnapshot`、`NpcHomeSnapshot` | identity、active、type、position、velocity、life、buffs、home coordinates |
| `NetworkProjectileSlice` | `ProjectileReplicationSnapshot` | identity、owner、type、position、velocity、damage、knockback、ai 数组切片 |
| `NetworkItemSlice` | `ItemReplicationSnapshot`、`ItemStack` | identity、owner、type、stack、prefix、position、active |
| `NetworkTileSectionSlice` | `WorldSectionSnapshot`、`WorldTile`、`LegacyTileSectionTile` | section 坐标、尺寸、tile/wall/liquid/wire/frame 字段、RLE 许可 |
| `NetworkChestSlice` | `ChestSnapshot`、`ChestItemReplicationSnapshot` | chest id、坐标、size、slot、item type/prefix/stack |
| `NetworkSignSlice` | `SignSnapshot`、`SignReplicationSnapshot` | sign id、坐标、text、revision、owner slot |
| `NetworkTileEntitySlice` | `LegacyTileEntity` 及其派生类型 | entity kind、id、坐标、有限 item/equipment 数组 |
| `NetworkWorldSlice` | `LegacyWorldDataContext` 和 `WorldJoinStateSnapshot` | world dimensions、spawn、time、progression、background、ore tiers、quest/biome 状态 |

切片规则：

- 数组和字符串必须复制或转为只读容器，不能把 `Main.player[i]`、`Item[]`、`Tile[]` 暴露出去。
- `float[] ai`、buff、背包和 tile section 采用明确上限；超过上限返回 `InvalidData`。
- 缺少 Dome 权威数据的旧版字段使用显式 `Unknown`/`Unavailable` 状态，不能填入客户端看似
  合法的猜测值。
- 切片转换集中在 `DomeNetworkSliceFactory`（若复杂度超过一个文件则拆成按对象类型的 mapper），
  不在 `MessageBuffer`/`NetMessage` 中遍历 ECS。

## 6. 执行阶段

### 阶段 0：基线冻结

**文件：** 两个 `LegacyReference` 源文件、`Build/diagnostics/message-buffer-net-message-source-manifest.json`、
`docs/protocol/message-buffer-net-message-status.md`。

**动作：** 复制原文；记录 SHA-256、长度、时间、消息 ID 计数和每个公开入口；将 162 个协议 ID
与 `TerrariaMessageCatalog` 对照，标出 `Handled`、`Framed`、`Unsupported` 和版本差异。

**验收：** 源文件可独立恢复，manifest 可重算一致；没有任何旧版 `Terraria.*` 类型被加入新项目
引用。

### 阶段 1：建立隔离契约和切片

**文件：** `Isolation/Network*.cs`、切片类型、协议项目 `.csproj` 的 Compile 排除规则。

**动作：** 先实现不可变 envelope、命令结果、容量和序列号；再从现有 Simulation 快照创建切片；
为每个切片写字段上限和缺省语义。

**验收：** 协议项目可单独编译；切片测试证明修改原始 Simulation 对象不会修改已入队的 envelope。

### 阶段 2：接入编译版 `MessageBuffer` / `NetMessage`

**文件：** `Isolation/MessageBuffer.cs`、`Isolation/NetMessage.cs`、必要的 Codec 适配方法。

**动作：** 以旧版 `GetData`、`ReceiveBytes`、`SendData`、`SendSection`、`SendTileSquare` 的
   分支清单为迁移索引；每完成一个消息族就将状态从 `Unsupported` 改为 `Handled`，并保留 oracle
   输入输出样本。先覆盖 hello/bootstrap、player controls、world/section、tile、item/NPC/
   projectile、chest/sign/door，再处理 net module 和低频事件。

**验收：** 编译版不出现 `Main`、`Netplay`、XNA、旧版 `Player` 等引用；未实现消息返回可观测的
   `Unsupported`，不静默成功。

### 阶段 3：接入 `DomeServer.Update`

**文件：** `DomeNetworkIsolationAdapter.cs`、`DomeNetworkUpdateBridge.cs`、
`TerrariaProtocolSessionHost.cs`、`DomeServer.cs`。

**动作：** 将网络线程入站帧接入列表；在现有协议命令处理阶段统一 drain；保留现有命令类型和
   `SessionReplicationState`，逐步把出站写入切换为列表 drain。为 bootstrap 的同步完成句柄增加
   超时和会话关闭处理。

**验收：** socket 线程不直接修改 Simulation；一个 tick 内同一玩家命令按 sequence 顺序执行；
   tick 结束后出站帧按目标会话稳定排序且无重复发送。

### 阶段 4：兼容回归与切换

**文件：** 新增协议隔离验证项目和 loopback 验证；必要时仅修改现有测试入口。

**动作：** 使用特性开关保留旧 `TerrariaProtocolSessionHost` 路径，先运行双路径对比，再默认启用
   隔离路径。对每个已处理消息记录输入帧、命令、切片和输出帧的 manifest。

**验收：** 外部客户端观察到的帧字节、顺序、连接状态和错误行为与现有兼容基线一致；失败时可
   关闭特性开关回到旧路径。

## 7. 验证矩阵

所有 serial 命令从仓库根目录运行，并使用 `-p:UseSharedCompilation=false`：

```powershell
Get-FileHash 'D:\TRbackup\Version4物理删除了某些文件\Terraria\MessageBuffer.cs' -Algorithm SHA256
Get-FileHash 'D:\TRbackup\Version4物理删除了某些文件\Terraria\NetMessage.cs' -Algorithm SHA256

dotnet build .\src\Terraria.Dome.Protocol.V1456\Terraria.Dome.Protocol.V1456.csproj `
  -p:UseSharedCompilation=false
dotnet build .\src\Terraria.Dome.Server\Terraria.Dome.Server.csproj `
  -p:UseSharedCompilation=false

dotnet run --project .\Test\Terraria.Dome.Protocol.Compatibility.Verification\Terraria.Dome.Protocol.Compatibility.Verification.csproj `
  -p:UseSharedCompilation=false
dotnet run --project .\Test\Terraria.Dome.World.Protocol.Verification\Terraria.Dome.World.Protocol.Verification.csproj `
  -p:UseSharedCompilation=false
dotnet run --project .\Test\Terraria.Dome.FullClientBootstrap.Verification\Terraria.Dome.FullClientBootstrap.Verification.csproj `
  -p:UseSharedCompilation=false
dotnet run --project .\Test\Terraria.Dome.NetworkIsolation.Verification\Terraria.Dome.NetworkIsolation.Verification.csproj `
  -p:UseSharedCompilation=false
```

新增隔离验证至少覆盖：

1. 分片帧重组、零长度/超长帧、错误长度前缀和 `CheckBytes` 边界。
2. 入站 List 的 sequence 稳定性、容量满、关闭和跨线程追加。
3. `Update` 一次性 drain；同一 tick 不重复消费；命令失败不会吞掉后续序号。
4. `Player/Npc/Projectile/Item/TileSection/Chest/Sign/World/TileEntity` 切片的字段和所有权隔离。
5. `TrySendData` 的目标客户端、ignore 客户端、广播和 `Unsupported` 结果；广播必须跳过
   `IgnoreClient`，不能把它当作普通目标。
6. loopback bootstrap、section、player controls、tile/chest/sign/door、ping 和断开清理。
7. 对源 oracle 的关键消息族进行字节级或字段级差分；版本不一致的分支单独列入报告。

## 8. 风险、停止条件与回滚

### 高风险

- 源文件版本与 V1456 协议目录不一致，导致同一消息 ID 的字段布局不同。
- 旧版静态全局状态（`NetMessage.buffer`、压缩数组、超时/广播状态）被错误地恢复为共享状态，
  造成多会话串包。
- 同步请求需要立即应答，而 Dome 只在 tick 更新，造成死锁或 bootstrap 超时。
- tile/entity/item 的切片不完整，客户端能连接但在 section 或交互阶段断开。

### 停止条件

- 任何新代码直接引用旧版 `Main`、`Netplay`、XNA 或完整旧对象类型。
- 任何消息在没有 `Handled`/`Unsupported` 结果的情况下被静默丢弃。
- 出站帧无法关联来源命令和 sequence，或同一 envelope 被发送两次。
- 兼容回归只通过“连接成功”而没有验证字节/字段、顺序和 tick 消费证据。

### 回滚

1. 保留 `LegacyReference` 和 manifest，不删除审计基线。
2. 通过特性开关停用 `DomeNetworkUpdateBridge`，恢复现有 `TerrariaProtocolSessionHost` 路径。
3. 只回滚本阶段新增的隔离文件、项目 Compile 排除规则和明确的接入点；不得回滚用户已有修改。

## 9. 完成定义

本提案只有在以下条件全部满足时才算执行完成：

- 两个源文件已复制并有可复算的 manifest，且不作为生产代码编译。
- 编译版 `MessageBuffer`/`NetMessage` 只依赖协议隔离契约和 Codec，不依赖旧版对象图。
- 入站/出站网络数据确实经过隔离层的有序 `List`，由 `DomeServer.Update` 统一读取/写出。
- Dome 只通过命令 API 接收网络意图，只通过简单对象切片提供网络状态。
- 协议项目、服务器项目、隔离验证和已有 loopback 验证均给出命令及退出码证据。
- 当前支持范围、未支持消息、源版本差异和残余风险写入最终验证报告。

## 10. 待批准事项

执行前需要确认以下默认选择；若不改，本提案按默认值实施：

1. **复制位置：** `src/Terraria.Dome.Protocol.V1456/LegacyReference`，并从编译中排除。
2. **隔离实现位置：** 协议契约在 `Terraria.Dome.Protocol.V1456/Isolation`，Dome 适配器在
   `Terraria.Dome.Server/Protocol`。
3. **队列模型：** 入站、出站、命令三份有序 `List<T>`，锁内追加、tick 边界交换和 drain。
4. **兼容策略：** 保持现有外部协议不变，按消息族增量启用；旧版无法表达的分支显式返回
   `Unsupported`，不做静默降级。

## 11. 执行记录（2026-08-19 复核）

### 已落地文件和边界

| 范围 | 产物 | 执行结果 |
| --- | --- | --- |
| 源文件证据 | `src/Terraria.Dome.Protocol.V1456/LegacyReference/MessageBuffer.cs`、`NetMessage.cs` | 与指定 Version4 文件逐字节复制；SDK 编译排除 |
| 来源清单 | `Build/diagnostics/message-buffer-net-message-source-manifest.json` | 记录字节数、行数、SHA-256、来源和目标路径 |
| 入站隔离 | `Isolation/DomeNetworkIsolation.cs`、`NetworkInboundEnvelope.cs`、`MessageBuffer.cs` | 锁内追加、单调序列、容量拒绝、分片帧重组、tick 一次性 drain |
| 出站隔离 | `Isolation/NetMessage.cs`、`NetworkOutboundEnvelope.cs` | 命令式 `TrySendData`/`SendData`，载荷复制，帧进入出站 List |
| Dome 桥接 | `src/Terraria.Dome.Server/Protocol/DomeNetworkUpdateBridge.cs`、`DomeNetworkIsolationAdapter.cs` | PlayerControls、TileManipulation、Chest、Door、Sign 命令在 tick 入口统一转换 |
| 复制发送 | `DomeServer.QueueOutboundFrames`、`FlushNetworkIsolationOutboundAsync` | 复制帧先入隔离出站 List，tick 末按目标会话统一 flush；会话写线程仍负责 socket |
| 对象切片 | `Isolation/Network*Slice.cs`、`NetworkWorldSlice.cs`、`NetworkTileEntitySlice.cs` | 从快照复制 Player/Npc/Projectile/Item/Chest/Sign/TileSection/World/TileEntity 字段，不暴露 Dome 可变存储 |
| 诊断语义 | `DomeNetworkIsolation.UnsupportedMessageIds` | bridge 对未映射消息返回 `Unsupported` 并记录消息 ID，禁止静默 `Accepted` |
| 广播语义 | `NetworkOutboundEnvelope.IgnoreClient`、`FlushNetworkIsolationOutboundAsync` | 广播按目标会话发送并跳过明确的 ignore 客户端 |
| 初始 NPC 边界 | `EnsureInitialNpcsCommand`、`EnsureInitialNpcsAsync` | SpawnTileData 请求入队；Dome tick 创建默认 NPC 后才构造初始 world stream |
| 验证项目 | `Test/Terraria.Dome.NetworkIsolation.Verification` | 10 项隔离检查，含分片帧、容量、序列、载荷所有权、网络对象切片、Unsupported 诊断和 Dome tick drain |

### 已验证命令

以下命令从仓库根目录串行运行，均使用 `-p:UseSharedCompilation=false`。2026-08-19 的
当前工作树结果见执行记录；FullClientBootstrap 已在修复后通过，旧的失败输出不再作为当前证据：

```powershell
dotnet build .\src\Terraria.Dome.Simulation\Terraria.Dome.Simulation.csproj `
  -p:UseSharedCompilation=false
dotnet build .\src\Terraria.Dome.Protocol.V1456\Terraria.Dome.Protocol.V1456.csproj `
  -p:UseSharedCompilation=false
dotnet build .\src\Terraria.Dome.Server\Terraria.Dome.Server.csproj `
  -p:UseSharedCompilation=false
dotnet run --project .\Test\Terraria.Dome.NetworkIsolation.Verification\Terraria.Dome.NetworkIsolation.Verification.csproj `
  -p:UseSharedCompilation=false
dotnet run --project .\Test\Terraria.Dome.World.Loopback.Verification\Terraria.Dome.World.Loopback.Verification.csproj `
  -p:UseSharedCompilation=false
dotnet run --project .\Test\Terraria.Dome.TileInteraction.Verification\Terraria.Dome.TileInteraction.Verification.csproj `
  -p:UseSharedCompilation=false
dotnet run --project .\Test\Terraria.Dome.WiringLiquidChest.Loopback.Verification\Terraria.Dome.WiringLiquidChest.Loopback.Verification.csproj `
  -p:UseSharedCompilation=false
dotnet run --project .\Test\Terraria.Dome.Protocol.Compatibility.Verification\Terraria.Dome.Protocol.Compatibility.Verification.csproj `
  -p:UseSharedCompilation=false
dotnet run --project .\Test\Terraria.Dome.World.Protocol.Verification\Terraria.Dome.World.Protocol.Verification.csproj `
  -p:UseSharedCompilation=false
```

结果：Simulation、Protocol、Server 三个项目构建均为 exit 0、0 warning、0 error；隔离验证
输出 `PASS: 10 network isolation checks`；协议长度契约、三个 World Protocol 检查、World
loopback、TileInteraction 和 Wiring/Liquid/Chest loopback 均通过。FullClientBootstrap 在修复后
连续 3 次通过。

FullClientBootstrap 已重新通过，输出为
`PASS: full-client bootstrap completes without source-player authority replay`。其初始 NPC
settlement 现在通过 `EnsureInitialNpcsCommand` 在 Dome tick 创建，并由
`CreateDefaultWorldNpcs` 使用 `ResolveDefaultNpcSpawn` 返回的完整坐标投影到玩家可见 section，
不再把 NPC 的 Y 坐标强制写成 `0.0f`；`SessionReplicationState` 同时以
`DefaultJoinNpcFrameBudget` 限制常规 settlement 帧，避免无限增量复制。

TileInteraction 在早期串行回归中曾出现等待首个可见 delta 的 3 秒间歇超时；加入失败状态诊断
并重新编译依赖后连续 3 次通过全部 7 项。验证器只对 `SyncNPC` settlement 使用 quiet window，
继续消费并忽略计划中的非 NPC 世界变化帧。

### 当前不宣称完成的范围

1. `DomeNetworkUpdateBridge` 对尚未建立 Dome 命令映射的消息返回 `Unsupported`；部分低频消息
   仍未迁移，禁止把“连接成功”当作完整 V1456 parity。
2. bootstrap、初始 world/section 和同步 sign/world 响应仍由 session host 直写，以避免把
   tick-only 队列引入握手死锁；这是明确的同步 bootstrap 边界。动态响应（包括 Ping）已经
   通过 `EnqueueOutboundFrame -> NetMessage.TrySendData -> isolation outbound List`，在 tick
   末统一 flush，不再直接写 socket。
3. `EnsureInitialNpcsCommand` 已恢复为真正的 tick 边界命令：session host 等待完成句柄，Dome
   tick 创建 NPC 后再生成初始 stream；这条路径由 World loopback 验证。
4. FullClientBootstrap 当前已通过；完整旧版 162 个消息分支 parity 仍为 Partial，Unsupported
   消息和同步 bootstrap 直写边界仍是后续迁移范围。
5. 回滚章节所述的独立 runtime feature switch 尚未落地；当前保留的同步 session-host 边界不能
   被误称为可热切换的旧路径，真正回滚仍需按章节 8 的窄范围操作。
6. 自定义 `BaseIntermediateOutputPath` 的并行构建曾触发 MSBuild 循环依赖或 Arch 包解析错误；
   验收命令因此固定为仓库默认输出、串行执行，不把该环境失败误记为源码通过。

### 执行后的验收结论

首批隔离网络路径已经具备可运行、可测试的闭环：socket 输入进入隔离 List，Dome tick 统一
转换命令，动态复制和 Ping 帧进入隔离出站 List 后再交给现有 session writer；网络对象切片已
覆盖 World 和 TileEntity，FullClientBootstrap 也已验证通过。该结论仍不覆盖完整旧版 162 个
消息分支 parity，扩展消息族时必须先增加 `Handled`/`Unsupported` 证据和对应 loopback 差分。
同步 bootstrap 直写是有意保留的残余架构边界；runtime rollback feature switch 仍是后续工作。
