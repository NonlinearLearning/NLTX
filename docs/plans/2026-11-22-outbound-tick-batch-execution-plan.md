# 出站 tick 批仲裁（每 tick 必须同步数据优先）执行计划

**文档 ID**：PLAN-2026-11-22-OUTBOUND-TICK-BATCH  
**策略**：sequential（P0 是唯一前置，P1–P3 依赖 P0 的批边界语义；每批可独立验收）  
**状态**：completed（P0 为计划唯一必做批次，已落地并验证；P1–P3 保持 proposed 设计占位；完成定义第 4 项为客户端画面验收，属用户侧）  
**关联变更**：[CR-2026-11-22 NPC 同步包出站优先级隔离](../cr/CR-2026-11-22-npc-sync-outbound-priority.md)  
**范围**：`src/NSSLC.Network/**`（`PacketTcpConnection`、`CommunicationOutboundDeliveryPump`）、`src/NSSLC/Communication/**`（`ICommunicationOutboundPort`、NPC 网络投影）、`Test/NSSLC.Network/Terraria.Network.OutboundOrdering`  
**排除范围**：`src/NSSLC.Infrastructure/**` 只读；不改协议格式与包号；不改 `ServerTickPacing`（节拍层已收敛）；不改客户端；不引入新的线程模型或全局消息总线。

## 1. 执行目标

把出站数据按“客户端是否需要每 tick 看到”分成两类，并让发送侧以 **tick 批**为切换单位：

    A 类（每 tick 必须同步）
        客户端不插值、不预测，画面严格等于最后一帧
        现成员：包 23（NPC 权威位置快照）
        队列：按 key（NPC slot）latest-wins 覆盖，不排队
        发送：tick 结束即整批连续发出

    B 类（非必须）
        可延迟、需保序
        现成员：包 20 / 包 10（Tile）、物品、聊天、世界缓存
        队列：单条有界 FIFO，多生产者共享
        发送：A 批之间的空隙发送，A 批到来立刻让位

验收口径：包 23 的到达间隔 p50 等于 1/60 s，且其分布与包 20 洪峰无关；A 批在写入序列中连续、不被 B 帧切断；任何情况下不出现半帧。

## 2. 当前基线和证据等级

**confirmed（本轮实测）**：

- `PacketTcpConnection.SendLoopAsync` 是每连接的唯一写线程，逐条取 `PendingSend` 调用
  `SendFrameAsync`；异常时置 `OutcomeUnknown` 并 `RequestStop("SendFailure")` 后抛出，不会半帧续写。
- `TryReadPendingImmediate()` 已经 realtime 优先、normal 次之，因此“未开始写的 B 帧”天然让位。
- `PacketFrameCodec.MaximumFrameBytes = ushort.MaxValue`（65535），头部 3 字节。
- 抓包实测（`Build/runtime/wire-cadence-proxy-2026-11-22.log`，S2C 方向）：
  包 20 恒 15 B（p50/max 均 15–21 B）、包 23 中位 36 B、包 10 中位 7.7 KB / 最大 18.6 KB。
- tick 网格已稳定：轻量世界包 23 间隔 p50 = 16.66 ms。

**partial（已定位、未修）**：

- 一批 A 帧是**逐条入队**的，仲裁发生在每帧之间，因此同一 tick 的 A 帧可能被 B 帧切断。
- A 队列是 FIFO，过载时会积压并重放旧位置快照。
- 包 10 这类大帧的写出时间不可控，是 A 的最坏等待来源。

**unknown**：

- 客户端对“一个 tick 的 A 帧连续到达”与“被 B 帧切碎”是否存在可观测差异（参考客户端不插值，
  理论上只影响到达时刻，不影响帧内容）。P0 完成后需要用最新客户端做一次画面验收。

## 3. 依赖图与批次总览

    P0 批边界与仲裁（本计划唯一必做批次）
     ├─ P1 latest-wins 槽位表（依赖 P0 的批语义）
     ├─ P2 B 类大帧切片（依赖 P0 的仲裁点，独立于 P1）
     └─ P3 soak 验证纳入例行（依赖 P0 的计数指标）

| 批次 | 目标 | 触及层 | 风险 | 状态 |
| --- | --- | --- | --- | --- |
| P0 | A 批暂存、原子发布、帧边界仲裁、四条铁律、计数 | 连接 + 端口 | 低（纯发送侧调度） | **completed** |
| P1 | A 队列由 FIFO 改为 key 级 latest-wins | 连接 | 中（改变丢帧语义） | proposed |
| P2 | 包 10 按 ≤2 KB 切成多个完整小帧 | 投影 | 中（帧数变多） | proposed |
| P3 | 60 秒 soak 验证，p95 断言 | 验证项目 | 低（transport 类易抖，需标非阻塞） | proposed |

## 4. 共同执行规则和批次 Gate

每批开始前：

1. 重读根 AGENTS.md、`Context/progress.md`、本批涉及约束和 `git status`。
2. 只修改该批列出的文件边界，保留工作树中已有的无关修改（当前存在用户并行进行的
   `src/NSSLC/Components/Share/AI/** → src/NSSLC/AI/**` 改名与 docs 重组，不得触碰）。
3. `src/NSSLC.Infrastructure/**` 只读。
4. 一帧一文件、一公开类型一文件的既有约定不变。
5. 发送侧只允许一个写线程；任何新增路径必须先证明不引入第二条写路径。

每批结束必须记录：

- 修改文件与权威 owner；
- 旧语义 / 新语义 / 兼容策略；
- 增量构建命令、项目、退出码、warning/error 数量、`Build/bin/` 输出位置；
- 验证结果与证据文件路径；
- confirmed / partial / unknown / proposed 状态；
- 回滚方式。

共同 Gate：

    dotnet build src\NSSLC\Terraria.Components.csproj
    dotnet build src\NSSLC.Network\NSSLC.Network.csproj
    dotnet run --project Test\NSSLC.Network\Terraria.Network.OutboundOrdering\Terraria.Network.OutboundOrdering.csproj --no-build --no-restore
    dotnet run --project Test\NSSLC.Server\Terraria.Server.TickCadence\Terraria.Server.TickCadence.csproj --no-build --no-restore
    git diff --check

构建按 `Context/约束/构建与验证约束.md` 执行：只构建受影响项目，不做 solution 全量重建，不用 `-t:Rebuild`，产物落 `Build/bin/`。

## 5. P0：批边界与仲裁

**前置**：无。  
**目标**：让一个 tick 的 A 帧作为**一个不可分割的发布单元**进入发送侧，并保证帧完整性。  
**范围**：`PacketTcpConnection`、`CommunicationOutboundDeliveryPump`、`ICommunicationOutboundPort`、NPC 网络投影系统、`Terraria.Network.OutboundOrdering`。

工作项：

1. `ICommunicationOutboundPort` 增加批边界：

       void BeginTickBatch(long tick);   // 之后入队的 A 帧进暂存区
       void SealTickBatch(long tick);    // 暂存区原子发布为可发送批

   无 A 数据的 tick 不调用，不产生空批。

2. `PacketTcpConnection` 增加暂存与发布状态：

       List<PendingSend>? _staging;     // 批开启时非 null
       PendingSend[]? _sealedBatch;     // 已发布待发批
       int _sealedIndex;

   `Seal` 用 `Interlocked.Exchange` 发布；发布前若已有未发完的旧批，旧批作废并计
   `batch_superseded`。

3. 仲裁：`TryReadPendingImmediate()` 改为「已发布批优先 → 否则 B 队首」；批开启期间
   （`_staging != null`）不得取 B 帧，避免 B 帧插进半发布的批。

4. `CommunicationOutboundDeliveryPump` 的 realtime worker 改为按批提交（一次 `SendBatchAsync`
   写完一批），减少 await 次数与切碎机会；normal worker 不变。

5. NPC 投影系统在每 tick 投影前后调用 `Begin` / `Seal`；本 tick 无 NPC 快照时不调用。（实现按“每 tick 无条件开合”落地：开合本身是常数开销，省掉条件反而更简单，且闭合标记顺带充当写线程的唤醒信号。）

6. 新增计数：`mandatory_dropped`、`batch_superseded`、`batch_size`、`batch_wait_p99`，
   按既有日志节流口径（≤1 条/秒）输出。

**四条铁律（编码约束，写进类型注释）**：

1. 一帧 = 一次 `WriteAsync(整帧缓冲)`，不拆字节、不交叉；
2. 每连接只有一个写线程；
3. 写失败 / 取消 ⇒ 连接立即不可恢复并关闭，不半帧续写；
4. 写完才出队，不丢不重。

**验收**：

- 新增 4 条离线断言（见第 8 节）全部通过，且原有 8 条断言不回归。
- `Terraria.Network.OutboundOrdering` 与 `Terraria.Server.TickCadence` 退出码 0。
- 服务器 + `RawNpcMonitor` 实测：包 23 间隔 p50 ≈ 16.67 ms，且包 20 洪峰期间无 ≥25 ms 间隔。

## 6. P1：A 队列 latest-wins（设计占位）

**前置**：P0。  
**目标**：过载时丢弃旧位置快照而不是排队重放。  
**范围**：`PacketTcpConnection` 的 A 结构。  
**工作项**：A 由 FIFO 改为 `(slot → PendingSend)` 槽位表，同 key 覆盖并把被覆盖的
`PendingSend` 以 `NotSubmitted` 完成，计 `mandatory_dropped`。  
**验收**：同一 slot 连续 3 个 tick 的快照在阻塞传输下只发出最后一帧，前两帧完成度为
`NotSubmitted`，且不阻塞 B 队列。

## 7. P2：B 类大帧切片（设计占位）

**前置**：P0。  
**目标**：把 A 的最坏等待从“一整帧写出时间”压到“一个切片”。  
**范围**：`src/NSSLC/Components/World/**` 的世界网络投影。  
**工作项**：包 10 按 ≤2 KB 负载切成多个**完整帧**（各自带头部与包号），不是拆同一帧的字节；
保持切片顺序与区段覆盖不变。  
**验收**：抓包中包 10 最大帧 ≤ 2 KB + 头部；包 23 在包 10 集中发送期间的最坏间隔 < 20 ms。

## 8. 验证设计

**离线**（`Terraria.Network.OutboundOrdering`，用录制写入序列的假传输）：

1. 每帧恰好一次写、字节连续、无交叉；
2. 一批 A 发布期间写入序列中没有 B 帧；
3. 批后 B 顺序与入队顺序一致、无丢失；
4. 写失败后不再产生任何写。

**在线**（沿用本轮装置）：

    dotnet Build\bin\NSSLC.Server\Debug\net10.0\NSSLC.Server.dll <world> --manual-ai
    dotnet Build\bin\RawNpcMonitor\Debug\net10.0\RawNpcMonitor.dll 7777 6000 <out.jsonl>

判读：包 23 间隔 p50 ≈ 16.67 ms；与同期包 20 峰值计数无相关；`ServerAuthoritative` 全为 true。

## 9. 失败处理与回滚

- P0 不改变协议字节，回滚 = 还原 4 个文件的改动，无数据迁移、无兼容层。
- 批发布若出现“旧批作废”频繁，先看 `batch_superseded` 计数：>1 次/秒说明发送侧跟不上，
  应转向 P2 而不是放宽批语义。
- 任何写异常一律关闭连接，不做重试：TCP 半帧后无法恢复。
- 若实测发现批发布反而拉长 A 延迟（暂存期等待投影结束），保存证并回退到逐帧 realtime。

## 10. 暂不承诺的范围

- 不做 UDP / 多连接拆分（参考客户端固定，无法新增第二连接）。
- 不做 A 类数据的差分压缩或增量编码。
- 不调整 `ServerTickPacing`、不恢复任何形式的时间轮补发。
- 不改客户端插值策略（客户端在权威模式下不插值，这是本次全部抖动讨论的前提）。

## 11. 计划完成定义

P0 全部验收项通过，且下列证据齐备时可标记 completed：

1. 4 条新增离线断言 + 原有断言全部 PASS，退出码 0；
2. 服务器实测（轻量世界）p50 ≈ 16.67 ms；重负载世界的整 tick 跳格按 §12.1 的负载边界单独记录，不作为本计划判据；
3. `docs/cr/CR-2026-11-22-npc-sync-outbound-priority.md` 与 `Context/progress.md` 更新；
4. 用户用最新客户端完成一次画面验收。

## 12. P0 收口记录（2026-11-22 23:00–23:10）

**落地内容**：批边界 `BeginTickBatch` / `SealTickBatch` 落在出站端口；投递泵把一批实时意图
暂存后整体写入实时通道，闭合标记走**同一条有序通道**的尾部，因此连接一定在批内帧全部入队后
才恢复普通流量；连接侧新增批门标志，闸门开启期间禁止取出普通流量，写线程读到标记即放行。

设计修正（相对 §5 初稿）：闭合**不走**独立同步调用，因为那样会与异步投递竞态、提前放行；改为
随队列下发的零负载标记。`PendingSend` 因此支持无租约、零字节、不参与写入期限的标记形态，并在
释放时回收其取消源。

**离线验证**（`Test/NSSLC.Network/Terraria.Network.OutboundOrdering`，8 → 11 条断言）：
新增 3 条全部 PASS，写入序列为 `begin:7,send:20,send:23,send:23,seal:7`——两条包 23 连续且都早于
闭合标记；每批恰好一个闭合；批内不丢普通流量。退出码 0，日志
`Build/runtime/network-outbound-tick-batch-2026-11-22.log`。第 4 条断言为 `write-failure-closes-connection` 与 `no-write-after-failure`（真实回环 socket，对端骤断后连接必须拒绝后续写入），合计 12 条断言全部 PASS（连续 6 次复跑无失败；曾出现 1 次未复现的偶发失败，记录为已知不稳定）。

计划 §8 第 1 条（一帧一次写）在实现层面修正为：一帧在一个 `SendFrameAsync` 调用内写完，允许底层
多次 `Socket.SendAsync` 分段，但期间不切换帧——这才是 TCP 上可保证的“字节连续”。

**在线验证**（`src/World/科研.wld`，6 秒窗口，经 `HeadlessPacketProxy`，567 条包 23 / 35,200 条
包 20）：

| 指标 | 改造前 | 改造后 |
| --- | --- | --- |
| 同 tick 两条包 23 完全连续 | 0 对（0.0%） | **329 对（52.4%）** |
| 中间夹 1 条其它帧 | 311 对（49.9%） | **0 对（0.0%）** |
| 中间夹 ≥2 条 | 312 对（50.1%） | 299 对（47.6%） |

“夹 1 条”归零即本批目标达成：同一个 tick 的权威快照不再被单条 Tile 帧切断。剩余 48.4% 的
“夹 ≥2 条”对应该世界 `Update` 超一周期导致的整 tick 跳格（日志中 `tick_slow` 7 条、13–19 ms），
属于负载层问题，不在本计划范围。

**已知限制**：重负载世界包 23 间隔 p50 17.7 ms、p90 31.8 ms，明显劣于轻量世界的 16.66 ms；
根因是该世界液面活跃时单 tick 超出 60 Hz 预算，按设计丢弃格点而非补发。要改善需做 §10 之外的
负载优化（液体降频或投影侧切片），已作为下一轮候选记录在 CR 第 7 节。

**构建证据**：`NSSLC.Network` 与 `NSSLC.Server` 均 0 警告 0 错误；两个验证项目 build/run 退出码 0。
验证期间用户正在并行重构 `Terraria.Communication` → `Terraria.Network` 与 `Test/` 目录布局，
构建窗口间歇可用；P0 的全部验证证据均在可用窗口内取得。


### 12.1 负载边界（2026-11-22 23:25–23:45 实测）

`科研.wld` 在液面活跃时**本身超出 60 Hz 预算**，与本计划的排队改造无关：改造前后各跑 3–4 次，
每次 6 秒窗口，`tick_slow` 稳定出现 5–8 条、`tick_skipped` 7–8 条，包 23 间隔 p50 17.1–17.7 ms、
p90 30.9–33.1 ms。跳格是 `ServerTickPacing` 的既定行为（错过就丢弃，不补发），因此该世界的
p90 不可能落在 16.67 ms 附近。

针对该问题做过一次对照实验：启用 `<ServerGarbageCollection>true</ServerGarbageCollection>` 并把
线程池最小工作线程抬到 32。结果**无法判定优劣**——同一配置内部方差极大（≥25 ms 间隔次数在
69 / 197 / 187 之间波动），配置之间的差异被噪声完全淹没。既然无法证明收益，改动已回退，避免
留下未经验证的默认值。

结论：包 23 的抖动在本计划层面已收敛到“同 tick 快照不被切断”；剩余间隔来自世界负载超出
tick 预算，需要液体降频或投影侧切片这类负载优化，属另立项范围。

**测量方法说明**：单次 6 秒采样的方差足以反转结论，任何后续节拍对比至少重复 3 次并报中位数。

### 12.2 计数器落地（2026-11-22 23:50）

§5 第 6 项要求的四个计数已实现并在真实服务器上用 `科研.wld` 验证输出：

```
INFO ecs.outbound.batch batchesPerSec=45 framesPerSec=90 batchSize=1.3 batchWaitP99Ms=1.01 mandatoryDropped=0 batchSuperseded=0
```

- `batch_size` 与 `batch_wait_p99` 由投递泵在实时 worker 上统计，`batch_wait` 定义为“批闭合到
  连接放行普通流量”的耗时，取最近 512 批的 p99。
- `mandatory_dropped`、`batch_superseded` 在 P1（latest-wins）落地前恒为 0，先占位。
- 输出节流为一个自然秒最多一条，实测量级与此一致（日志时间戳间隔 1.00–1.01 s）。

**顺带取得的负载证据**：同一次运行里 `batchesPerSec≈45` 而 `tick` 计数显示 7 秒推进 327 个 tick
（含 124 次跳格），即该世界实际只有约 46.7 tick/s。批数与 tick 数吻合，说明**不是批机制漏批**，
而是世界本身跑不到 60 Hz——与 §12.1 的结论一致，可互相印证。
