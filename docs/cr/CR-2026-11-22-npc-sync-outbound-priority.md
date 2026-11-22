# NPC 同步包出站优先级隔离

文档 ID：CR-2026-11-22-NPC-SYNC-OUTBOUND-PRIORITY
逻辑域：change-control
产物类型：decision、execution
状态：completed
日期：2026-11-22
canonical 路径：`docs/cr/CR-2026-11-22-npc-sync-outbound-priority.md`
上游依据：用户报告"史莱姆抖动"并指定根因方向（服务器出站队列中 Tile 更新包挤压 NPC 同步包）

## 1. 症状与结论

史莱姆在服务端权威模式下持续抖动。用户给出的判断是：出站队列里 Tile 更新包挤压 NPC 同步包，
使服务器权威位置更新延迟；最新客户端已按权威标记关闭本地 AI，客户端不是主要根因。

本轮核对了客户端源码与该判断一致，并确认服务端出站路径仍存在"跨优先级共享串行队列"的结构：
NPC 位置快照虽然已被标记为 `Realtime`，但它仍与 Tile/世界包共用同一条投递通道，Tile 队列一
积压，NPC 快照就必须排在其后。修复方向是让两档优先级在**整条出站路径**上互不阻塞，并让
Tick 内的投影工作量有界。

## 2. 客户端证据：权威模式下抖动完全由包 23 到达节拍决定

参考客户端 `D:\TRbackup\最新的客户端\Terraria\MessageBuffer.cs` 包 23 分支：

- `nPC5.serverAuthoritative = bitsByte31[5];`（第 1643 行，NSSLC 自定义位）
- `if (nPC5.serverAuthoritative) { nPC5.netOffset = Vector2.Zero; }`（第 1645-1648 行）

`D:\TRbackup\最新的客户端\Terraria\NPC.cs` `UpdateNPC`（第 91267-91272 行）：

```
if (Main.netMode == 1 && serverAuthoritative)
{
    netOffset = Vector2.Zero;
    FindFrame();
    return;
}
```

即：权威模式下客户端既不跑本地 AI，也不做 `netOffset` 平滑（原版靠 `netOffset` 衰减把两次快照
之间的跳变抹平）。渲染位置严格等于最后一次收到的包 23 位置。因此**任何包 23 的到达间隔抖动都会
直接变成画面抖动**，服务端必须保证包 23 的投递不被其它流量挤压、且节拍稳定。这与用户判断一致，
也说明客户端不是本轮要改的对象。

## 3. 出站路径的排队结构缺陷

修复前 `CommunicationOutboundDeliveryPump` 只有一个 worker：

- 单个 `Task.Run` 顺序 `await` 每一次 `_transport.SendAsync(intent)`；
- 因此一个 Tile 帧的 socket 写完成前，队列里的 NPC 快照无法被取走投递；
- 若 socket 写因客户端读取慢而阻塞，阻塞时间直接叠加到 NPC 快照的延迟上。

`PacketTcpConnection` 侧同样只有一条 `_sendQueue`：即使泵按优先级取件，也只是"取件顺序"不同，
一旦某帧已在写，后续帧仍按 FIFO 排在同一条连接队列里。

同时 `WorldNetworkProjectionSystem` 与 `CommunicationOutboundProjectionSystem` 都在 Tick 内
无界遍历积压：Tile 变更多时，投影阶段耗时会线性增长；Tick 被拉长后下一次 NPC 快照整体后移，
形成"Tile 越多 → Tick 越长 → 位置更新越晚"的正反馈。

### 3.1 修复前测量（模型化传输，每帧固定 2 ms 服务时间，180 tick）

用 `.agent-workplace/scratch/NetworkOutboundLatency` 对同一路径注入固定服务成本，只改变每 tick
的 Tile 帧数量，观察 Realtime 帧的投递延迟：

| Tile 帧/tick | p50 | p95 | 投递总数（应为 180×(n+1)） |
| --- | --- | --- | --- |
| 1  | 2.07 ms | 3.57 ms | 360 = 360 |
| 8  | 4.20 ms | 5.96 ms | 2120 > 1620（队列积压） |
| 20 | 4.38 ms | 5.94 ms | 1678 < 3780（提交被拒） |
| 40 | 4.27 ms | 5.90 ms | 1678 < 7380（提交被拒） |

Realtime 延迟随 Tile 流量单调上升（2.07 → 4.38 ms），且 Tile 队列一旦积压就不再排空。

## 4. 改动

| 文件 | 改动 | 目的 |
| --- | --- | --- |
| `src/NSSLC.Network/CommunicationOutboundDeliveryPump.cs` | 拆成 realtime / normal 两条独立投递 worker，各自 `await` 自己的传输写入；`LastDelivery` 改为 `Interlocked.Exchange` | 一个优先级等 socket 写时，另一个优先级仍能继续投递 |
| `src/NSSLC.Network/PacketTcpConnection.cs` | 新增 `_realtimeSendQueue`、`_realtimeSendFrameSlots`、`_realtimeSendBudget`、`_pendingRealtimeSendAdmissions`；发送循环 realtime 先取；`PendingSend` 记录自己的信号量 | 同一连接内 authoritative 位置快照可越过已排队的 Tile 帧 |
| `src/NSSLC.Network/PacketTcpConnectionOptions.cs` | 新增 `MaximumPendingRealtimeSendBytes/Frames/Requests`（128 KB / 32 / 16）及校验 | 为 realtime 预留独立配额，正常流量既不能占用也不能借用 |
| `src/NSSLC/Components/Communication/CommunicationOutboundMessageBufferComponent.cs` | 新增 `TryPeek`（realtime 优先） | 让投影"先确认能提交再出队"，被拒时保持原位而不是排到队尾 |
| `src/NSSLC/Components/Communication/CommunicationSessionSystems.cs` | 投影改为 peek→submit→dequeue，每会话每 tick 最多 `DefaultMaximumIntentsPerTick = 256` 条，遇到拒绝立即停止本轮 | Tick 内的工作量有界、顺序不乱、不丢包 |
| `src/NSSLC/Components/World/Contract/IWorldProjectionSource.cs`、`WorldRuntime.cs` | `DrainPendingMutations(int maximum)`：只取至多 maximum 条，其余留在世界侧继续与后续变更按坐标合并 | Tile 洪水不会把 Tick 拖长；未发送的变更不会丢失 |
| `src/NSSLC.Network/WorldNetworkProjectionSystem.cs` | 每 tick 至多取 `DefaultMaximumTileMutationsPerTick = 128` 条 Tile 变更 | 同上 |
| `src/NSSLC.Server/ServerTickPacing.cs`、`Program.RunTicksAsync` | 用 `Stopwatch` 单调网格取代 `PeriodicTimer`；超时丢弃错过格点而不再补发；暴露 `tick_skipped` / `tick_slow` | 包 23 的到达节拍不再在 10/25 ms 之间摆动 |

设计取舍：

- 两条 worker 只保序各自档位内的顺序。两档承载的是互相独立的事实（NPC 位置 vs Tile/世界状态），
  跨档乱序是本次修复想要的效果，不引入语义风险。
- Realtime 配额是预留而不是抢占：正常流量的上限没有被放宽，`connection-normal-allowance` 一类的
  边界仍然成立（见第 5 节）。
- Tile 变更用"延后"而不是"丢弃"来限流：超出本 tick 配额的变更留在 `WorldRuntime` 的
  `_pendingMutations` 中，同一坐标的后续变更继续按既有 `WorldMutationRules` 合并。

## 5. 验证

新增 `Test/NSSLC.Network/Terraria.Network.OutboundOrdering`（控制台验证程序，退出码即结论）：

命令：
`dotnet build Test\NSSLC.Network\Terraria.Network.OutboundOrdering\Terraria.Network.OutboundOrdering.csproj`
→ 0 警告 0 错误；
`dotnet run --project Test\NSSLC.Network\Terraria.Network.OutboundOrdering\Terraria.Network.OutboundOrdering.csproj --no-build --no-restore`
→ 退出码 0，日志 `Build/runtime/network-outbound-ordering-2026-11-22.log`。

| 检查 | 结果 |
| --- | --- |
| `pump-realtime-latency-does-not-scale-with-tile-volume` | PASS：轻载 p50 2.93 ms / p95 5.07 ms，重载（24 Tile 帧/tick）p50 2.42 ms / p95 2.99 ms |
| `connection-realtime-not-blocked-by-normal-backlog` | PASS：normal 写被 socket 阻塞时（8×65 KB 未读），Realtime 帧仍被接纳并最终 `LocallySent` |
| `projection-bounded-per-tick` | PASS：预算 4 时提交 4 条、剩 6 条 |
| `projection-preserves-order` | PASS：先入队的先提交 |
| `projection-retries-rejected-intent-in-place` | PASS：被拒后仍在队首，重试时先提交它 |
| `projection-takes-realtime-first` | PASS：Realtime 优先于先入队的 Normal |
| `world-mutation-intake-bounded-and-preserved` | PASS：上限 3 时三次 drain 得 3/3/6，之后为空 |

同一模型化传输路径在修复后的对照（180 tick，2 ms/帧）：

| Tile 帧/tick | 修复前 p50 / p95 | 修复后 p50 / p95 |
| --- | --- | --- |
| 1  | 2.07 / 3.57 ms | 2.16 / 3.32 ms |
| 8  | 4.20 / 5.96 ms | 2.19 / 3.20 ms |
| 20 | 4.38 / 5.94 ms | 2.13 / 3.10 ms |
| 40 | 4.27 / 5.90 ms | 2.89 / 3.13 ms |

### 5.1 全量回归

用户同期把 `Test/` 重组为 `Test/<生产边界>/<验证项目>/`，旧路径全部作废。按新布局对全部 14 个
验证项目逐个 `dotnet build` + `dotnet run --no-build --no-restore`：

| 项目 | build | run | FAIL 行数 |
| --- | --- | --- | --- |
| Terraria.Network.OutboundOrdering | 0 | 0 | 0 |
| Terraria.Attributes | 0 | 0 | 0 |
| Terraria.Chat | 0 | 0 | 0 |
| Terraria.Combat.ProtectionWindow | 0 | 0 | 0 |
| Terraria.Entity.Definition | 0 | 0 | 0 |
| Terraria.Entity.Scheduling | 0 | 0 | 0 |
| Terraria.Inventory | 0 | 0 | 0 |
| Terraria.ItemUse | 0 | 0 | 0 |
| Terraria.Npc.Ai | 0 | 0 | 0 |
| Terraria.NpcSpawn | 0 | 0 | 0 |
| Terraria.Projectile | 0 | 0 | 0 |
| Terraria.TileEntity | 0 | 0 | 0 |
| Terraria.WorldItem | 0 | 0 | 0 |
| Terraria.Server.TickCadence | 0 | 0 | 0 |

14/14 退出码 0，共 263 条 PASS、0 条 FAIL（2026-11-22 22:02 复跑，含本轮新增的 Terraria.Server.TickCadence）。
构建（均输出到 `Build/bin/`）：


- `dotnet build src\NSSLC.Network\NSSLC.Network.csproj`：退出码 0，0 警告 0 错误。
- `dotnet build src\NSSLC.Server\NSSLC.Server.csproj`：退出码 0，0 错误；19 条既存警告全部来自
  未改动的 `src/NSSLC.Infrastructure/WorldGeneration`。
- 监听冒烟：`dotnet Build\bin\NSSLC.Server\Debug\net10.0\NSSLC.Server.dll Build\runtime\CommunicationGameTest-20261027\world.wld --manual-ai`
  12 秒后主动停止，日志 `Build/runtime/server-smoke-outbound-2026-11-22.log` 出现
  `ecs.server.ready ai=manual listen=127.0.0.1:7777`，stderr 为空。

## 6. 线路级复核（2026-11-22 21:45–22:02）

用 `Build/runtime/RawNpcMonitor`，配合 `HeadlessPacketProxy`（`7778→7777`）与直连两种方式，
在 5–6 秒观察窗口内统计包 23 的实际到达间隔。

### 6.1 采集条件

| 项 | 值 |
| --- | --- |
| 服务器 | `--manual-ai`，`ecs.server.ready` 后接入 |
| 世界 A | `src/World/科研.wld`（8400×2400，液体活跃） |
| 世界 B | `Build/runtime/CommunicationGameTest-20261027/world.wld`（1×753，**零包 20**） |
| 客户端 | `RawNpcMonitor`（裸 socket，逐帧时间戳，`ServerAuthoritative` 位独立解码） |
| 证据 | `Build/runtime/raw-npc-monitor{,-light,-direct,-fixed-light,-instr-light}-2026-11-22.jsonl`、`wire-cadence-proxy-2026-11-22.log` |

两种采集方式（经代理 / 直连）得到同一分布，说明差异不是代理引入的。

### 6.2 世界 A：Tile 洪水下的隔离效果

5 秒窗口内收到 **39,864 条包 20**（≈8018/s），每 tick 的包 20 簇大小中位数正好 **128**，
与 `DefaultMaximumTileMutationsPerTick` 一致——限流生效且未丢变更。

| 指标 | 值 |
| --- | --- |
| 包 23 条数 | 624（312 × 2 NPC，全部到达，无丢弃） |
| 间隔 p50 / p90 / p95 / p99 | 15.58 / 17.3 / 18.6 / 31.2 ms |
| 间隔 ≥25 ms 的次数 | 9（每次窗口内夹 **127** 条包 20；正常窗口平均 63 条） |
| `ServerAuthoritative` | 全部为 `true` |

即：极端 Tile 洪水下 Realtime 帧仍在 ~16 ms 节拍内，只有 9 次被挤后一个周期。

### 6.3 世界 B：零 Tile 流量暴露的第二个成因

**关键发现**：在完全无 Tile 更新的世界里，节拍依旧不规则（p50 15.8 / p90 23.7 / p95 26.0 ms，
标准差 7.1 ms），说明剩余抖动与 Tile 队列无关，而在 tick 调度本身。

原 `PeriodicTimer(1000/60 ms)` 的行为：某次 tick 超时后，到期信号已经置位，
`WaitForNextTickAsync` 立刻返回，于是错过的格点被**合并成补发**，包 23 的间隔在 10 ms 与 25 ms
之间摆动；客户端在权威模式下不插值，这种摆动直接等于画面抖动。

修复（`src/NSSLC.Server/ServerTickPacing.cs` + `Program.RunTicksAsync`）：

- 用 `Stopwatch` 单调网格取代 `PeriodicTimer`；尾部 ≤2 ms 忙等，避免 15.6 ms 系统定时器粒度主导节拍。
- 超时后**丢弃**错过的格点并推进到严格未来的格点，绝不零间隔补发；丢弃次数以
  `ecs.server.tick_skipped` 告警计数暴露（按秒节流，带累计值）。
- 单次 `Update` 超过 12 ms 时输出 `ecs.server.tick_slow`（同样按秒节流，带窗口内最大值），
  把"服务器卡顿"与"投递延迟"分开定位。
- 网格语义由 `Test/NSSLC.Server/Terraria.Server.TickCadence` 的 6 项确定性检查锁定
  （未超时推进一格、零余量按错过处理、超时 1.5/2.5 周期分别丢 1/2 格、返回值严格未来、永不零间隔补发）。

| 世界 B 指标（5–6 s 窗口） | 改造前 PeriodicTimer | 改造后网格调度（终版） |
| --- | --- | --- |
| p50 | 15.8 ms | **16.66 ms**（正好等于 1/60 s） |
| p90 | 23.7 ms | 22.1 ms |
| p95 | 26.0 ms | 24.8 ms |
| p99 | 31.8 ms | 27.5–30.2 ms |
| 标准差 | 7.1 ms | 4.8–6.1 ms |

终版复测（`Build/runtime/raw-npc-monitor-final-light-2026-11-22.jsonl`）里 p50 已经精确等于
60 Hz 周期，说明稳态 tick 网格本身没有偏差；剩下的长尾来自两处，本轮只定位不改动：

- 该世界液面活跃时 `Update` 偶尔超过一个周期（`tick_slow` 12–24 ms），这些时刻按设计丢弃格点，
  于是出现 33 ms 的整周期缺口；
- ECS 投影 → 投递泵 → 连接队列 → socket 的异步链路，以及测量端逐帧落盘，都会引入毫秒级抖动。

两级告警按秒节流（5 秒窗口内各 5 条），避免重负载世界把日志淹没
（`Build/runtime/server-smoke-tickpacing-throttled-2026-11-22.log`）。


### 6.4 工具缺陷修复（随本轮一并处理）

- `HeadlessPacketProxy` 两个中继方向并发写同一个 `StreamWriter`，触发
  `InvalidOperationException: The stream is currently in use by a previous operation`，导致抓包
  恒定在包 7 之后断开。已用 `SemaphoreSlim` 串行化日志写入（`Build/runtime/HeadlessPacketProxy/Program.cs`）。
- 该工程目录下残留 2026-11-20 的旧 `obj/`（含生成的 `AssemblyInfo.cs`），与重定向后的中间目录
  重复，构建报 `CS0579`；删除残留后 0 警告 0 错误。
- `RawNpcMonitor` 增加端口/窗口/输出路径参数与窗口到期取消的正常收尾，支持直连测量。

## 6.5 tick 批仲裁（2026-11-22 23:00 P0 落地）

在出站侧把数据按“客户端是否需要每 tick 看到”分两类：实时快照类（包 23）为 A，其余共用一条 B
队列。投影每 tick 开批、闭合；连接在同一批内不接受普通流量，闭合标记随实时通道下发，因此批内帧
一定先于闭合。带外结论：

| 同 tick 两条包 23 的相邻关系 | 改造前 | 改造后 |
| --- | --- | --- |
| 完全连续 | 0.0% | 52.4% |
| 中间夹 1 条其它帧 | 49.9% | 0.0% |
| 中间夹 ≥2 条 | 50.1% | 47.6% |

即“单个 Tile 帧把快照切断”已消除；剩余 48.4% 源于该世界整 tick 超时跳格（`tick_slow` 13–19 ms），
属负载层而非排队层。重负载世界包 23 间隔 p50 17.7 / p90 31.8 ms，轻量世界仍为 16.66 ms。
批计数已上线：`ecs.outbound.batch` 每秒最多一条，输出 `batchesPerSec` / `framesPerSec` / `batchSize` / `batchWaitP99Ms` / `mandatoryDropped` / `batchSuperseded`。实测 `batchSize≈1.4`、`batchWaitP99Ms≈1.0`，即批闭合到放行普通流量的等待在 1 ms 量级，不构成新的阻塞点。

详见 [执行计划](../plans/2026-11-22-outbound-tick-batch-execution-plan.md) 第 12 节。

## 7. 未完成与风险

- 未做原版客户端的画面验收：需要用户用最新客户端实际观察史莱姆抖动是否消失。本轮已把证据推进到
  线路级（包 23 到达间隔分布），但"画面是否还抖"仍要人眼确认。联机验收步骤见
  `Context/调试工具/调试工具客户端操作手册.md`；抓包判读用其 §7.2（筛 `S2C id=23` 的到达间隔，
  对照同期 `S2C id=20` 计数），修复后包 23 间隔应与包 20 洪峰解耦。
- 残余 ±4 ms 量级方差位于 ECS 投影 → 投递泵 → 连接队列 → socket 的异步链路。要再压缩需要让
  realtime 帧在 tick 线程上同步写出（跨 `NSSLC.Network` 的架构变更），本轮未做。
- Realtime 配额与 Tile 限流取的是保守默认值（32 帧 / 128 KB / 16 次准入；每 tick 256 条 ECS
  意图、128 条 Tile 变更）。大规模液体或爆炸场景若仍观察到 Tile 延迟，可再调这两个常量。
- 本轮期间 `Test/` 被用户重组为 `Test/<生产边界>/<验证项目>/` 的两级布局，旧路径全部作废。
  本次新增的验证项目最终落在 `Test/NSSLC.Network/Terraria.Network.OutboundOrdering`。
  历史 CR 里出现的旧 `Test/*.Verification`
  路径按 `Test/README.md` 的迁移对照表换算。



