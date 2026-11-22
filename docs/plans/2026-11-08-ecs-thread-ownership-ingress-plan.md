# ECS 线程所有权与原始 Ingress 执行计划

文档 ID：PLAN-2026-11-08-ECS-THREAD-OWNERSHIP-INGRESS
逻辑域：plans
产物类型：plan
状态：completed
范围：TCP worker、通信 host、session ingress、Preparation 阶段和连接生命周期
证据入口：`src/NSSLC.Network/CommunicationHostRuntime.cs`；`src/NSSLC/Components/Communication/CommunicationSessionIngress.cs`；`src/NSSLC.Server/Program.cs`；`src/NSSLC/Components/Share/Entity/EcsServer.cs`
canonical 路径：`docs/plans/2026-11-08-ecs-thread-ownership-ingress-plan.md`

## 1. 目标与非目标

目标是让 Arch `World` 只有 ECS Tick 线程访问。网络线程只能把不可变原始事件写入线程安全 ingress；连接打开、完整帧、连接关闭都由 `Preparation` 阶段消费。

非目标：本批不改协议字节布局、不改变 packet handler 的业务规则、不把所有外部传输代码搬入 ECS、不在网络线程执行玩家创建/销毁或 session 状态迁移。

## 2. 当前问题证据

- `CommunicationHostRuntime.EnqueueInboundAsync` 直接调用 `Ingress.TryEnqueue`。
- `CommunicationSessionIngress.TryEnqueue` 访问 `_sessions`、`World.IsAlive`、`World.TryGet`，并修改 `CommunicationSessionComponent` 和 inbound buffer。
- `TryOpen`/`TryClose` 直接调用 `_world.Create`/`_world.Destroy`。
- `Program` 的 `onSessionOpened`/`onSessionClosed` 回调直接创建和销毁玩家实体。
- `RunTicksAsync` 另一条 `PeriodicTimer` 循环同时调用 `EcsServer.Update`。

这些调用路径静态确认了线程边界穿透；本批需用测试证明修复后的访问规则，而不是只删除几个方法调用。

## 3. 目标形态

```text
TCP worker
  -> ThreadSafeRawIngress (immutable Open/Packet/Close events)
  -> CommunicationIngressPreparationSystem
       -> session lifecycle request / packet queue / player lifecycle request
       -> EcsServer command buffer
  -> Commit barrier
       -> entity create/destroy and component changes
```

推荐的原始事件契约至少包含：事件种类、`SessionEpoch`、单调 ingress sequence、接收时间或 transport timestamp、packet 的不可变 bytes，或 close reason。事件必须在入队后不再被修改；不得把 `ArchEntity`、ECS component 引用、`World`、普通 `Dictionary` 或可变 handler 状态放进事件。

`CreateEnvelope` 不得为了填充 session stage 查询 ECS；stage 在 `Preparation` 消费时从 session component 读取。worker 只负责帧边界、限流、取消和传输错误。

## 4. 执行阶段

### Phase 1：建立 raw ingress

- 在 `NSSLC.Network` 或通信边界定义线程安全的 raw ingress port 和不可变事件类型。
- 选定有界 `Channel<T>`、`ConcurrentQueue<T>` 加 shutdown 标志，或等价实现；记录容量、满载、关闭、取消和丢弃语义。
- `CommunicationHostRuntime` 只持有该 ingress port；`OpenSessionAsync`、`EnqueueInboundAsync`、`CloseSessionAsync` 只创建事件并入队。
- 删除 worker 对 `CommunicationSessionIngress` 的直接调用和对 ECS World 的引用。
- 保持 epoch 分配使用原子操作；不要把 session dictionary 搬到网络 worker。

### Phase 2：Preparation 消费

- 新增 `CommunicationIngressPreparationSystem` 或等价 Preparation system，唯一消费 raw ingress。
- 该 system 维护 session epoch 到实体的解析状态，读取/修改 ECS session component，并产生玩家创建、玩家销毁和 packet handler 输入。
- open/packet/close 的顺序必须由 ingress sequence 或明确的 per-epoch order 保证；关闭后的 packet 必须被拒绝或丢弃并可观测。
- 在同一 tick 中无法通过 deferred entity creation 看到新 session 时，保留 packet 直到实体可见；不得为了“同帧”重新开放 worker 访问 World。
- 将 Program 的 `onSessionOpened`/`onSessionClosed` 业务逻辑迁入 Preparation/Commit 可追踪路径。玩家实体创建、销毁遵循 CommandBuffer 子计划，不保留隐藏 callback 直写。

### Phase 3：关闭与 shutdown

- 定义 host 停止时的 raw ingress drain、取消和丢弃语义。
- 确保 listener 停止后不会再产生新事件；已入队事件在 ECS 主循环中按约定处理或记录为 shutdown discard。
- 在 server shutdown 前先停止生产者，再完成最后一个 ECS 处理边界，最后清理 session/runtime registry。

## 5. 验收标准

- 静态检查：`NSSLC.Network` 的 worker/host 路径不出现 `ArchWorld` 查询、`World.Create/Destroy/Add/Remove`、ECS component 读取或 session dictionary 访问；这些只出现在 Preparation/Commit 所属代码。
- 并发测试：至少两个 producer 同时提交 open/packet/close，ECS 单线程 drain 后 epoch 和 sequence 顺序稳定，不抛 `Dictionary` 并发异常。
- 生命周期测试：open → packet → close → late packet；断线和取消各走一次，session/entity/player 清理幂等。
- 背压测试：raw ingress 满载、关闭和取消结果有明确状态；不得静默把成功入队伪装成已处理。
- 协议回归：现有聊天、玩家输入、通信启动和 outbound projection 验证保持行为等价。
- 构建：按根级构建约束记录 `Terraria.Components`、`NSSLC.Network`、`NSSLC.Server` 和新增验证项目的命令、退出码、warning/error 计数及输出路径。

## 6. 依赖与风险

本计划先于 CommandBuffer 实现，但必须为其提供结构变更请求边界；如果 session open/close 需要两个可见性 barrier，记录该选择并在 CommandBuffer 计划中统一。不要用 `lock` 包住整个 Arch World 作为最终方案；锁只能保护 ingress 元数据，不能证明 Arch World 支持跨线程调用。
