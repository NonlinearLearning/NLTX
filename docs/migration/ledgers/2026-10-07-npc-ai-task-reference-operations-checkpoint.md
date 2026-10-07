# NPC AI 持续任务引用绑定推进协议检查点

日期：2026-10-07  
状态：领域引用绑定入口与独立 verifier 已实现；RuntimeNpcEntity/RuntimeNpcStore 宿主接入由主会话负责。

## 范围

本切片把 `Advance`、`Complete`、`Fail` 和 `Interrupt` 增加为带
`RuntimeEntityHandle + NpcTaskReference` 的领域入口。`NpcTaskReference` 继续复用实体运行时
身份和 `TaskGeneration`，没有新增 store、registry 或持久化 AI 状态。领域入口只读写附着的
`NpcTaskStateComponent`；它不执行即时效果、不发布消息，也不改变目标、关系、生命周期或实体
存储状态。

宿主必须先通过当前 `EntityRuntime` 对当前 `RuntimeEntityHandle` 捕获/编辑实际附着的
`NpcTaskStateComponent`，再调用引用绑定入口。领域方法自身再次核对当前句柄和 task generation；
这样旧组件引用不能绕过运行时附着验证影响新实体。

## 新入口与拒绝协议

四个新入口保留原方法名并以引用参数重载：

```text
Advance(state, currentEntityHandle, reference, expectedKind)
Complete(state, currentEntityHandle, reference, expectedKind)
Fail(state, currentEntityHandle, reference, expectedKind, failureReason)
Interrupt(state, currentEntityHandle, reference, expectedKind, failureReason)
```

返回 `NpcTaskReferenceOperationResult`，包含接受/变化标志、操作前后 task 快照以及
`NpcTaskReferenceOperationRejectionReason`。拒绝原因按以下边界区分：

| 原因 | 含义 | 状态变化 |
| --- | --- | --- |
| `InvalidReference` | 句柄未分配、引用未分配或 task generation 为零 | 无 |
| `StaleEntityReference` | 引用来自另一实体、槽位代际或 session | 无 |
| `StaleTaskReference` | 同一实体上的旧 task generation | 无 |
| `NotRunning` | 引用仍对应当前代际，但任务已完成、失败、中断或已被生命周期结束清空 | 无 |
| `TaskKindMismatch` | 当前运行任务与调用方期望 kind 不同 | 无 |
| `InvalidTaskKind` | 请求 kind 为 `None` 或未定义枚举值 | 无 |
| `InvalidFailureReason` | `Fail`/`Interrupt` 使用 `None` 或未定义枚举值 | 无 |

身份、generation、kind、运行态和 failure reason 的检查顺序固定为：引用有效性、实体句柄、
task generation、kind 有效性、运行态、kind 匹配、失败原因。失败路径返回前后相同快照，不写入
task state。`Enter` 和 kind-only `Fail`/`Interrupt` 也拒绝未定义枚举值。

原有 `Advance(state, kind)`、`Complete(state, kind)`、`Fail(state, kind, reason)` 和
`Interrupt(state, kind, reason)` 保留，兼容只在同一同步调用边界内已经持有当前附着 state 的
调用方；延迟、异步、排队或可能跨生命周期的工作必须使用引用绑定重载。宿主接入不应把 kind-only
入口当作旧 lease 的安全替代品。

## 验证范围

独立 `Terraria.NpcAi.LifecycleVerification` 使用真实 `EntityRuntime` fixture，覆盖：

- 同 kind 重启后旧 reference 对四个操作全部返回 `StaleTaskReference`；新 reference 能推进；
- 旧实体槽位复用返回 `StaleEntityReference`，新 session 的身份也不能解析旧引用；
- kind mismatch、invalid failure reason 和所有拒绝路径保持 task state 不变；
- 完成后重复 complete/fail、失败后重复 fail/interrupt、以及中断后重复 interrupt 返回
  `NotRunning` 且 `Changed == false`；
- `Reset` 后旧 reference 返回 `StaleTaskReference`；`Death`、`Transform`、`Removal`、
  `WorldUnload` 终止后四个操作均返回 `NotRunning`；
- 既有终止协议、目标缓存/即时效果 owner 清理和关系失效回归继续通过。

验证 fixture 不是 `RuntimeNpcStore` 的真实卸载或变换接线证据；主会话负责宿主调用点和完整
Simulation acceptance。

## 构建与运行证据

日期：2026-10-07

| 命令 | 结果 | 退出码 | warning/error | Build 输出 |
| --- | --- | ---: | --- | --- |
| `dotnet build src/NSSLC/Component/Npc/Terraria.Npc.csproj --no-restore --nologo -v:minimal` | `Terraria.Npc` 成功 | 0 | 0 / 0 | `Build/bin/Terraria.Npc/Debug/net10.0/Terraria.Npc.dll` |
| `dotnet build Test/Terraria.NpcAi.LifecycleVerification/Terraria.NpcAi.LifecycleVerification.csproj --no-restore --nologo -v:minimal` | `Terraria.NpcAi.LifecycleVerification` 成功 | 0 | 0 / 0 | `Build/bin/Terraria.NpcAi.LifecycleVerification/Debug/net10.0/Terraria.NpcAi.LifecycleVerification.dll` |
| `dotnet run --project Test/Terraria.NpcAi.LifecycleVerification/Terraria.NpcAi.LifecycleVerification.csproj --no-build --no-restore` | `PASS: NPC task termination lifecycle verification` | 0 | 不适用 | 使用上方 verifier Build 输出 |

完整控制台记录位于 `Build/diagnostics/NpcAiRedesign/task-reference-operations-20261007/`：
`npc-project-build.log`、`lifecycle-verifier-build.log` 和 `lifecycle-verifier-run.log`。

这些结果只证明 NPC 领域项目和独立 verifier。`RuntimeNpcEntity`、`RuntimeNpcStore` 的真实
引用绑定调用点以及完整 Simulation acceptance 仍由主会话负责；本切片没有修改宿主文件。
