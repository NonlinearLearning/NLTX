# NPC 持续任务结束协议检查点

日期：2026-10-07  
状态：领域协议与独立 verifier 已实现；RuntimeNpcStore 集成由主会话负责，宿主验收仍 open。

## 范围与决定

本切片为 NPC 持续任务在死亡、变换、移除及世界卸载时结束提供领域协议。它只写
`NpcTaskStateComponent`；不写 `NpcLifecycleComponent`、目标、即时效果或关系状态，也不接管
实体身份、关系或存储。实体身份沿用 `RuntimeEntityHandle`，任务上下文使用单调递增的
`TaskGeneration`。不新增注册表、实体 store 或持久化 AI 状态。

生命周期终止是**取消/中断**，不是任务失败。`NpcTaskTerminationResult.PreviousTask` 保留
终止前的 kind、phase、cursor、failure reason 和 generation；`CurrentTask` 清除任务 kind、phase、
cursor 与 failure reason，并记录首次 `NpcTaskEndReason`。因此，原先已 `Completed` 或 `Failed`
的结果能被观察到，但不会作为新实体或变换后 profile 的当前任务继续存在。终止 API 只改领域
状态并返回结果，不运行任务、不重放 tick、不提交效果，也不回滚已经提交的世界效果。

同一 owner/task generation 的同原因重复终止返回 `AlreadyEnded`、无变化。随后收到不同生命周期
原因时返回 `EarlierEndReasonPreserved`、仍为 accepted/no-change；`EndReason` 保留首次原因，
`RequestedEndReason` 报告后续请求。宿主必须继续实体、关系及会话清理，不得因这个 no-op 中断清理。
无效 reason、旧实体 handle 或旧 task generation 返回 `Rejected`，组件状态不变。`Reset` 增加
generation 以拒绝 Reset 前的引用；启动新 task 也增加 generation。

`NpcTaskLifecycleSystem.TryCaptureReference` 是纯查询；但它和 `Terminate` 都不能只凭调用方传入
相同的 handle 证明组件属于该实体。宿主必须先通过 `EntityRuntime.TryCapture`/`TryEdit`，由
运行时验证当前 handle 与实际附着的 `NpcTaskStateComponent`，再捕获/终止 lease。本 verifier
使用真实 `EntityRuntime` 验证附着访问、local slot 复用、generation 失效与 session runtime ID
变化；它没有运行真实 `RuntimeNpcStore` 生命周期。

一般任务入口仍有意保持现状：`Advance`、`Complete`、`Fail`、`Interrupt` 以 task kind 和当前
附着组件为边界，未增加 generation 参数。因此本切片不声称所有异步/延迟任务操作都已受旧 task
lease 防护；未来若引入这些调用，调用方要使用同一 lease 并在宿主 owner 内验证附着组件。

## Owner 与清理边界

| 状态 | 唯一 owner / 现有入口 | 终止处理 |
| --- | --- | --- |
| 任务 kind、phase、cursor、failure、generation 与首次 end reason | `NpcTaskLifecycleSystem` / `NpcTaskStateComponent` | `Terminate` 快照后清除任务实例态；failure 保留在 `PreviousTask`，生命周期结束原因单独记录 |
| NPC 活动/死亡阶段 | `NpcDeathLifecycleSystem` / `NpcLifecycleComponent` | 任务协议不写生命周期字段；宿主在死亡阶段/效果边界正确后调用结束，再释放实体 |
| 目标选择缓存 | `NpcTargetSelectionSystem` / `NpcTargetSelectionStateComponent` | 变换前由 `ResetForTermination` 清缓存；已提交的 `NpcTargetComponent` 由现有 target commit owner 清成无目标 |
| 未提交即时效果 | `NpcImmediateEffectStateComponent` | 变换前调用 `DiscardPendingEffects`；死亡/移除/卸载需在实体终止后不可再消费这些 intent |
| parent/general relation 与 child 反向扫描 | `RuntimeNpcEntity` / `RuntimeNpcStore` | 关系 owner detach；结束结果不得取代 detach；失效 `EntityReference` 必须被当前 `EntityRuntime` 拒绝 |
| Active presence cache | `NpcActivePresenceScanSystem` / `NpcActivePresenceCache` | 若 cache 跨 session 保留，由现有 owner 调用 `Invalidate`；若按 session 新建则随旧 session 丢弃 |
| entity handle 与 session identity | `EntityRuntime` / `RuntimeNpcStore` | 用当前 attached handle 捕获/编辑；runtime ID、local index 与 generation 一起判别旧实体 |

不机械清零实体的所有 AI、生命、世界历史或持久化字段。变换需要按该 profile 的显式规则选择
保留/替换内容；死亡、移除与卸载移除整实体时依靠既有 entity owner 销毁其 transient components。

## 主会话宿主接入清单与调用点

以下位置由主会话负责，不在本切片修改：

1. **`RuntimeNpcEntity.TryCaptureTaskReference` / `TryTerminateTask`**：前者在当前
   `RuntimeHandle` 上经 `EntityRuntime.TryCapture` 取得附着 task generation；后者经同一 handle
   的 `EntityRuntime.TryEdit` 后调用 `NpcTaskLifecycleSystem.Terminate`。不可绕过 runtime 直接把
   旧组件引用交给 System。generation 仅防止同一实体上旧 task lease 结束新 task。
2. **`RuntimeNpcStore.TryRelease`（死亡、普通移除、父子链释放）**：在当前槽位 generation、
   `RuntimeHandle`、实例索引及未借用检查通过后，捕获/终止 task；然后由现有 relation owner
   `DetachChildren`，再 `TryBeginTermination`、释放 compatibility slot、`TryRemoveEntity`。
   死亡调用应位于死亡阶段和已提交 death effect 的边界之后。`EarlierEndReasonPreserved` 是
   accepted/no-change，不能阻止上述后续清理。
3. **`RuntimeNpcStore.ReleaseParentAndChildren`**：先用仍有效的 parent relations 形成释放顺序，
   再对 children 和 parent 分别执行死亡终止协议；保持当前逆序 child release 与 parent 最后释放。
   relation detach/失效引用拒绝必须独立完成，不能只依赖 task 结束。
4. **`RuntimeNpcStore.RemoveOwnedRuntimeEntities`**：`Hydrate` 替换旧 NPC roots、`Reset`、
   `Dispose` 的默认原因是 `WorldUnload`；candidate hydration rollback 可用 `Removal`。每个仍为
   `Running` 的 runtime root 在 `TryBeginTermination`/`TryRemoveEntity` 前经附着 task component
   完成一次性终止。已处于 `Terminating` 的 root 必须能证明先前终止边界已完成；被释放的 session
   不可再写入其 runtime。
5. **变换提交点（仍 open）**：在 transformation owner 已接受变换后、旧定义/profile state
   被替换前，用 `Transform` 结束旧 task；由 target owner 清除 selection cache 与旧 target，丢弃
   未提交 immediate intents，并由关系 owner detach 不随变换保留的关系。只有明确的 profile 规则
   可以将某些状态转移到新形态。当前 `RuntimeNpcStore` 未见通用变换提交入口；
   `NpcBloodMoonTransformationSystem` 当前是 intent 计算，不等于宿主变换接线。不得把 `Transform`
   偷换成 `Removal`，也不能声称此路径已经验收。
6. **世界级 cache 与关系闭包（待宿主核实）**：确认卸载/替换 `EntityRuntime` 时，所有指向旧 NPC
   的 relation 都由 relation owner detach 或随 session 一同销毁；若 active-presence cache 跨 session
   复用则通过其 owner `Invalidate`。新的 session/slot 不得解析旧 `RuntimeEntityHandle` 或
   `EntityReference`。

## 独立验证范围与限制

`Test/Terraria.NpcAi.LifecycleVerification` 引用 NPC domain 与现有 `EntityRuntime`，覆盖四种结束原因、
running/completed/failed task 的前态快照与清除、重复终止、不同后续结束原因保留首次 reason、
Reset 后旧 task lease 拒绝、同实体 task generation 拒绝、槽位复用的 runtime generation 拒绝、
新 session runtime ID 拒绝、目标纯查询不写入、目标 owner cache reset、即时 effects 丢弃，以及
删除后旧关系引用不解析。Verifier 是独立 runtime fixture，不是 `RuntimeNpcStore` unload、Transform
或完整关系接线证据；Simulation acceptance 由主会话继续执行并独立记录。

## 领域与独立 verifier 最终验证证据

日期：2026-10-07

| 命令 | 项目/结果 | 退出码 | warning/error | Build 输出 |
| --- | --- | ---: | --- | --- |
| `dotnet build src/NSSLC/Component/Npc/Terraria.Npc.csproj --no-restore --nologo -v:minimal` | `Terraria.Npc` 成功 | 0 | 0 / 0 | `Build/bin/Terraria.Npc/Debug/net10.0/Terraria.Npc.dll` |
| `dotnet build Test/Terraria.NpcAi.LifecycleVerification/Terraria.NpcAi.LifecycleVerification.csproj --no-restore --nologo -v:minimal` | `Terraria.NpcAi.LifecycleVerification` 成功 | 0 | 0 / 0 | `Build/bin/Terraria.NpcAi.LifecycleVerification/Debug/net10.0/Terraria.NpcAi.LifecycleVerification.dll` |
| `dotnet run --project Test/Terraria.NpcAi.LifecycleVerification/Terraria.NpcAi.LifecycleVerification.csproj --no-build --no-restore` | `PASS: NPC task termination lifecycle verification` | 0 | 不适用 | 使用上方 verifier Build 输出 |

完整控制台记录位于 `Build/diagnostics/NpcAiRedesign/task-termination-20261007/`：
`npc-project-build.log`、`lifecycle-verifier-build.log` 和 `lifecycle-verifier-run.log`。

以上只验证 NPC 领域项目和独立 fixture。它不验证 `RuntimeNpcEntity`、`RuntimeNpcStore` 或
Simulation 宿主接线。主会话报告宿主源码已完成，但完整依赖构建遇到并行
`NetworkPlayerOwner.State.cs` 中 `LocationComponent`/`ColliderComponent` 缺少 using 的编译错误；
宿主源码隔离验证与完整依赖构建复核仍由主会话负责，不能把此前宿主产物视为当前源码的通过证据。
