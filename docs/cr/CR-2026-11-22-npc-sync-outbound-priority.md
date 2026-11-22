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

用户同期把 `Test/` 重组为 `Test/<生产边界>/<验证项目>/`，旧路径全部作废。按新布局对全部 13 个
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

13/13 退出码 0，无 FAIL 行。构建（均输出到 `Build/bin/`）：


- `dotnet build src\NSSLC.Network\NSSLC.Network.csproj`：退出码 0，0 警告 0 错误。
- `dotnet build src\NSSLC.Server\NSSLC.Server.csproj`：退出码 0，0 错误；19 条既存警告全部来自
  未修改的 `NSSLC.WorldGeneration`。
- 监听冒烟：`dotnet Build\bin\NSSLC.Server\Debug\net10.0\NSSLC.Server.dll Build\runtime\CommunicationGameTest-20261027\world.wld --manual-ai`
  12 秒后主动停止，日志 `Build/runtime/server-smoke-outbound-2026-11-22.log` 出现
  `ecs.server.ready ai=manual listen=127.0.0.1:7777`，stderr 为空。

## 6. 未完成与风险

- 未做原版客户端的画面验收：需要用户用最新客户端实际观察史莱姆抖动是否消失。本轮只证明了
  出站排队结构缺陷已被消除、Realtime 投递延迟不再随 Tile 流量增长。
- Realtime 配额与 Tile 限流取的是保守默认值（32 帧 / 128 KB / 16 次准入；每 tick 256 条 ECS
  意图、128 条 Tile 变更）。大规模液体或爆炸场景若仍观察到 Tile 延迟，可再调这两个常量。
- 本轮期间 `Test/` 被用户重组为 `Test/<生产边界>/<验证项目>/` 的两级布局，旧路径全部作废。
  本次新增的验证项目最终落在 `Test/NSSLC.Network/Terraria.Network.OutboundOrdering`，
  并在 `Test/manifest.tsv` 登记为 `transport`。历史 CR 里出现的旧 `Test/*.Verification`
  路径按 `Test/README.md` 的迁移对照表换算。



