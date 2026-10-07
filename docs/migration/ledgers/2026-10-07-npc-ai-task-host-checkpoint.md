# NPC 任务结束宿主接线与验收

日期：2026-10-07  
状态：有限任务结束 owner 接线已通过独立生产宿主验收；完整 Simulation 当前源码验收 open。  
前置：[领域结束协议检查点](2026-10-07-npc-ai-task-termination-checkpoint.md)  
执行入口：[NPC AI 执行计划](../../plans/system-decomposition/2026-10-06-npc-ai-system-execution-plan.md)

本批由主会话集成领域会话的结束协议，写入范围为 `RuntimeNpcEntity` 的 task snapshot / reference
访问、`RuntimeNpcStore` 的任务结束调用、独立 production-host verifier 和 `Program` 的专属 probe
选项。领域规则继续由 `NpcTaskLifecycleSystem` 持有；身份与组件存储沿用 `EntityRuntime`。

## 接线与顺序

`RuntimeNpcEntity.TryCaptureTaskReference` 通过 `EntityRuntime.TryCapture` 在当前 runtime handle
捕获实际附着 task generation。`TryTerminateTask` 通过同一 handle 的 `TryEdit` 再调用领域协议，
由 runtime 验证组件归属和 lease。snapshot 增加 task generation 与首次结束原因。

普通 `TryRelease` 使用 `Removal`。死亡路径 `ReleaseParentAndChildren` 先捕获关系链释放顺序，
再由 child 到 parent 分别使用 `Death`。`Hydrate` 替换旧 roots、`Reset`、`Dispose` 的批量清理
对仍 Running 的 roots 使用 `WorldUnload`；candidate hydration rollback 使用 `Removal`。
每个 root 先结束 task，再处理现有 immunity / relation 清理，随后进入实体终止、释放 compatibility
slot、移除 runtime root。已释放 session 无法再编辑 task；其组件由 session runtime 销毁。

实际 probe 暴露 `TryRelease` 在借用检查前读取 `InstanceId` 的问题：此时另一组件已被借用，
identity capture 抛异常。已将 runtime readiness 检查移到身份组件读取之前。现在借用期间释放
返回 false，任务与 owner 均保持原态；借用结束后能正常结束/释放。

本批不新增实际类型变换入口。Transform 用例仅提交 task 生命周期边界，并验证随后的
WorldUnload 保留首次原因且继续移除 owner。目标缓存、即时效果、关系与新 profile 的实际
变换规则仍需独立接入；不能把该用例登记为来源 Transform 行为已完成。

## 真实验证

证据目录：`Build/diagnostics/NpcAiRedesign/runs/task-lifecycle-host-20261007/`。

| 检查 | 命令与产物 | 结果 |
| --- | --- | --- |
| 领域验收主会话复核 | `dotnet run --project Test/Terraria.NpcAi.LifecycleVerification/Terraria.NpcAi.LifecycleVerification.csproj --no-build --no-restore`；`domain-review-run.log` | exit 0；PASS |
| 新验收项目 restore | `dotnet restore Test/Terraria.NpcAi.HostLifecycleVerification/Terraria.NpcAi.HostLifecycleVerification.csproj --nologo`；`host-verifier-restore.log` | exit 0 |
| 生产宿主源码独立构建 | `dotnet build Test/Terraria.NpcAi.HostLifecycleVerification/Terraria.NpcAi.HostLifecycleVerification.csproj --no-restore --nologo -v:minimal -p:BuildProjectReferences=false`；`host-verifier-final-build.log` | exit 0；0 warnings / 0 errors；产物 `Build/bin/Terraria.NpcAi.HostLifecycleVerification/Debug/net10.0/Terraria.NpcAi.HostLifecycleVerification.dll` |
| 当前宿主源码快照 | `host-linked-source-before-build.json`、`host-build-manifest.json` | linked source 在构建前后未变化；manifest 明确 `BuildProjectReferences=false` |
| 生产宿主 owner 运行 | `dotnet run --project Test/Terraria.NpcAi.HostLifecycleVerification/Terraria.NpcAi.HostLifecycleVerification.csproj --no-build --no-restore -- Build/diagnostics/WorldGenerationRoundTrip/12345-small-final/generated.wld Build/diagnostics/NpcAiRedesign/runs/task-lifecycle-host-20261007/production-host-report-final.json` | exit 0；PASS；输入世界 SHA-256 不变；运行报告保存三个执行程序集哈希 |
| 整体宿主当前源码 | normal builds 的 `build.log`、`build-2.log` 至 `build-5.log`，隔离尝试 `build-isolated.log` | 未验收；详见下文，失败和中间通过记录均保留 |

独立 verifier 链接生产 owner 和稳定宿主源码，使用正式
`WorldGen.serverLoadWorldCallBack` 加载真实世界，不复制 owner 算法。运行覆盖：同实体任务替换
拒绝旧 task reference、借用期间释放拒绝、Transform 后卸载保留原因、同槽复用拒绝旧 entity
reference、普通释放、真实投射物致死父子清理、Reset、Dispose 和重复 Dispose。
主 session 的初始/最终 NPC 数均为 2，probe 创建的临时 owner 已释放。

结束原因的精确规则由领域 verifier 验证；致死用例证明真实 combat 路径移除 task/relation owner，
不通过诊断回调推进生命周期。新 program orchestration 的 `--npc-task-lifecycle-probe true`
选项已落盘；共享 `Program` 的完整当前源码编排尚未验收。

独立构建复用已存在的依赖产物，不能证明所有依赖源码同时可构建。normal build 曾 exit 0、
2 warnings / 0 errors，但回退会话随后继续写入。后续 normal build 分别遇到并行 NetworkPlayer
owner 缺少 using、NetworkWorldItem owner 泛型参数数量和 rollback probe 尚未落盘的类型。
最后 normal 尝试 `build-5.log` 是 exit 1、0 warnings / 3 errors。main 为最初两处 rollback
类型歧义补了明确的 `Terraria.Npc.NpcSlot` / `Terraria.WorldStorage.TileCoordinate`；未删除或
绕开整体宿主中的回退功能。`acceptance-result.json` 明确整体当前源码状态为 false。

## 未闭合与后续

同 kind 重启后的 `Advance`、`Complete`、`Fail`、`Interrupt` 引用保护已交原生命周期会话继续
长线实现；主会话后续接入真实宿主。该续批完成前，上述同步 kind-only 入口不能用于迟到的
异步请求。实际 Transform、跨世界生命周期闭包、目标/效果的变换清理、source golden、
authority/network、保存恢复与删除门禁仍 open。本批不提升任何覆盖 profile 为 verified。

## 回退

只回退本批新增的 task reference adapter、task snapshot 两字段、`TryRelease` / 批量清理 /
死亡链中的 task 结束调用、专属 probe 及新 verifier。保留并行 owner 对身份、borrow readiness、
关系、immunity 和候选回退的改动；借用检查前移应独立评估，不通过整体文件替换回退。
领域协议及其独立 verifier 属于前置会话，按其 checkpoint 单独回退。回退代码不重放已经
提交的致死、生成或世界效果，不恢复已经移除的 entity handle。

## 引用推进与完整宿主定向验收续批

在上述隔离证据之后，领域会话交付了引用绑定的四个推进入口，主会话完成生产 adapter
接线。`RuntimeNpcEntity.TryAdvanceTask/TryCompleteTask/TryFailTask/TryInterruptTask` 均接受
调用方捕获的 `NpcTaskReference`，通过当前 attached component 的 `TryEdit` 调用领域验证。
Guide 的同步调用也改用这些入口；回家流程从运行中 task snapshot 捕获 generation 并传递，
没有在迟到操作执行时重新捕获新 reference 来绕过旧 lease 验证。

新证据目录为 `Build/diagnostics/NpcAiRedesign/runs/task-reference-host-20261007/`。

| 检查 | 命令 / 日志 | 结果 |
| --- | --- | --- |
| NPC 领域当前源码 | `dotnet build src/NSSLC/Component/Npc/Terraria.Npc.csproj --no-restore --nologo -v:minimal`；`npc-build.log` | exit 0；0 warnings / 0 errors；`Build/bin/Terraria.Npc/Debug/net10.0/` |
| 链接生产 owner 的独立 verifier | README 命令，build 加 `-p:BuildProjectReferences=false`；`host-build.log` / `host-run.log` | build/run exit 0；build 0 / 0；`production-host-report.json` 的 ReferenceOperations 全部 true |
| Simulation 完整依赖构建 | `dotnet build src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj --no-restore --nologo -v:minimal`；`simulation-build.log` | exit 0；17 warnings / 0 errors；`Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll` |
| 领域新协议复核 | `dotnet run --project Test/Terraria.NpcAi.LifecycleVerification/Terraria.NpcAi.LifecycleVerification.csproj --no-build --no-restore`；`domain-reference-review-run.log` | exit 0；PASS |
| 七条真实 Program 场景 | 下方命令与同名 `.log/.json`；`acceptance-result.json` 保存退出码、构建统计与产物哈希 | 全部 exit 0、Succeeded=true、SourceFileUnchanged=true |

七条场景以同一只读世界和 fresh Simulation 产物运行，命令前缀为：

```powershell
dotnet Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll Build/diagnostics/WorldGenerationRoundTrip/12345-small-final/generated.wld <ticks> <options> --report <report.json>
```

| JSON / log 名称 | ticks / options | 观察与断言 |
| --- | --- | --- |
| task-probe | 1 / `--npc-task-lifecycle-probe true` | 同 kind 重启后四个旧操作拒绝；有效推进/完成/失败/中断通过；旧实体、借用、终止与释放回归通过 |
| guide-success | 120 / `--npc-home-return-probe success` | Completed、cursor=60、保留住房；末 tick 不再次传送 |
| guide-blocked | 120 / `--npc-home-return-probe blocked` | Failed、cursor=60、NoPath、无家；失败保持 |
| guide-invalid | 120 / `--npc-home-return-probe invalid` | None/Idle、cursor=0、不请求传送 |
| guide-success-boundary | 60 / success | Completed；HomeTeleportRequested=true、HomeTeleportSucceeded=true |
| guide-success-after | 61 / success | Completed/cursor=60；传送意图已清除；位置保持回家结果 |
| guide-blocked-boundary | 60 / blocked | Failed/NoPath；HomeTeleportRequested=true、HomeTeleportFailed=true |

本次 fresh Simulation 构建通过，替代前文“最新 normal build 仍失败”的当前判断；旧失败证据
保持历史事实。该结果没有重跑完整大回归，没有关闭 Source behavior、authority/network、
保存恢复、真实类型变换或剩余 profile。`acceptance-result.json` 明确
`FullSimulationRegressionRerun=false`、`CoverageProfilesPromoted=0`。

本续批回退限于四个 adapter 方法、Guide 的 reference 参数与调用、probe 的 ReferenceOperations
及新 verifier 对应验收。前置终止协议与引用领域协议分别按它们的 checkpoint 回退，保留
其他 owner 的空间、身份、immunity 与回退修复。
