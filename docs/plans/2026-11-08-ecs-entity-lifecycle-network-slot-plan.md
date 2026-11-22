# ECS 实体、AI 与网络 Slot 生命周期执行计划

文档 ID：PLAN-2026-11-08-ECS-ENTITY-LIFECYCLE-NETWORK-SLOT
逻辑域：plans
产物类型：plan
状态：completed
范围：`EntityRuntimeRegistry`、`NpcAiRuntime`/`AiEntitySystem`、NPC network slot、Communication session、shutdown 清理
证据入口：`src/NSSLC/Components/Share/Entity/EntityRuntimeRegistry.cs`；`src/NSSLC/Components/Npc/NpcAiRegistration.cs`；`src/NSSLC/Components/Share/AI/AiEntitySystem.cs`；`src/NSSLC.Network/NpcNetworkIdentitySystem.cs`；`src/NSSLC.Server/Program.cs`
canonical 路径：`docs/plans/2026-11-08-ecs-entity-lifecycle-network-slot-plan.md`

## 1. 目标

让一个实体从 allocate/create 到 destroy/retire 的所有外部绑定具有同一生命周期：runtime handle、Arch entity、AI binding、NPC network slot/generation、session/player 关系和挂起事实都能被同步、幂等清理。

## 2. 已确认缺口

- `EntityRuntimeRegistry.Clear()` 存在，但服务器退出路径没有看到共享 registry 的显式清理；其 generation 表是否跨 world 保留需要明确定义。
- `NpcAiRuntime` 只在整体 `Dispose` 时清理 binding；`TryDetach` 没有接入单 NPC 销毁路径。
- `NpcNetworkIdentitySystem` 的 generation 固定为 1，没有显式 allocator/release registry。
- slot 耗尽时 `NpcSpawnFactQueueComponent.Drain()` 已取走全部 facts，系统直接返回，未处理事实没有回放。
- session close、player destroy、session entity destroy 分散在 host callback 和 ingress，不能形成单一生命周期事务。

## 3. 目标契约

- destroy/retire 是一次可追踪的生命周期命令；所有 owner 在同一提交边界处理，重复调用安全。
- registry `Remove`、AI `TryDetach`、slot release 和 session unbind 的顺序有明确规则；任何失败都不会留下“实体已销毁但绑定仍活着”的隐式状态。
- NPC slot allocator 维护 `slot -> generation`，释放后再次分配递增 generation，generation 不为零并按协议宽度溢出规则处理。
- spawn fact 消费采用 cursor、逐条 commit、或显式 remainder requeue；槽位不足只保留事实/发出可观测拒绝，不丢整批数据。
- shutdown 先停止生产者，再处理或明确丢弃 ingress/事实，最后清空所有 registry 和 AI runtime。

## 4. 执行阶段

### Phase 1：生命周期清单和单一入口

- 列出所有 `World.Destroy`、entity definition destroy、projectile termination、player disconnect、NPC retirement 和 session close 路径。
- 为每个路径标注 entity owner、runtime registry、AI、slot、session 和 queue 的读写集。
- 选择统一的 destroy request/commit 入口，禁止新增旁路；与 CommandBuffer 的 playback 顺序对齐。

### Phase 2：registry 与 AI 清理

- 在实体销毁提交前/后定义 `EntityRuntimeRegistry.Remove` 的时机，避免旧 handle 在同 tick 被重新解析。
- 在 NPC destroy/retire 前调用 `AiEntitySystem.TryDetach`，处理 Stop intent 是否需要进入 Movement/Combat queue。
- 让 `NpcAiRuntime.Dispose` 成为 shutdown 的兜底，而不是单实体清理的唯一机制。
- 明确 `EntityRuntimeRegistry.Clear()` 是否保留 `_generations` 以支持同 world identity reuse；world scope 变化时不得误用旧 handle。

### Phase 3：slot allocator 与事实队列

- 引入显式 slot allocator 或等价状态，分配最低可用 slot，记录 generation，释放时递增。
- 把初始 NPC 的手工 slot/generation 也纳入 allocator，避免两套规则。
- 修改 spawn facts 的 drain 语义，使槽位不足时未处理 facts 能重试；记录容量压力和最终拒绝。
- projection 发送 slot/generation 时增加 stale occupant/retire 语义的测试，确认客户端不会把旧 NPC 当成新 NPC。

### Phase 4：shutdown 与 session

- 将 session close、player unbind、session entity destroy 和 outbound pump stop 排序写入 host contract。
- 服务器退出时显式清理 sessions、entities、projectiles、AI bindings 和 slot allocator；清理应可重复调用。

## 5. 验收标准

- 创建→销毁→同一 UUID/slot 重用的测试证明旧 handle/generation 不能解析到新实体。
- NPC 销毁后 `BindingCount`、slot 使用数、registry Count 和 ECS query 结果一致。
- slot 满载时 spawn facts 数量在处理前后可解释，未处理事实不会静默消失。
- 连接正常关闭、异常断开、重复 close、shutdown 取消都不留下 session/player/runtime binding。
- 现有 NPC spawn、AI、network projection、Projectile 和 session tests 回归通过；输出中的 generation/slot 改变必须记录为有意协议行为或保持兼容。
- 静态扫描证明生命周期旁路没有重新出现。
