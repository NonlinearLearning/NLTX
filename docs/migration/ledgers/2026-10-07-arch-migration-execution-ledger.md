# Arch 迁移执行总账

文档 ID：LEDGER-2026-10-07-ARCH-MIGRATION-EXECUTION  
日期：2026-10-08  
状态：**active / partial；总验收未通过，生产默认路径未切换，旧框架不得删除**  
总验收会话：当前主会话  
执行基线：`e2c686790ab6a4f14505ea914c27478f5959d061`（子会话均从该仓库状态建立隔离证据）

2026-10-08 验收规则变更：[CR-2026-10-08 编译-only 门禁](../../.agent-workplace/changes/CR-2026-10-08-arch-compile-only-acceptance.md)。后续批次不运行测试、探针、冒烟、benchmark、simulation、host 或其他运行时验证；只记录受影响项目的增量编译结果。历史运行证据保留为历史记录，不能作为本轮新增要求，也不能由编译成功推导行为完成。

本总账记录基于以下权威输入：

- [自定义 ECS 转向 Arch 的执行计划](../../plans/2026-10-07-custom-ecs-to-arch-execution-plan.md)
- [Arch 2.1.0 官方 API 与迁移事实](../../research/2026-10-07-arch-api-migration-research.md)
- [并行长线执行协调文档](../../plans/2026-10-08-arch-parallel-execution-coordination.md)
- 五份长线执行合同：T1 基线/API、T2 World/身份、T3 生命周期/关系、T4 查询/网络/物品、T5 宿主/性能/退出。

## 1. 总体判定

本轮已经完成“拆分任务、建立详细执行合同、启动 5 个新会话、按 `gpt-6-luna + max`
运行、要求子会话使用 `/goal` 与 `pua`、由主会话验收”的编排目标。原先的约 10% 核心测试
要求已由 CR-2026-10-08 暂停。

本轮**没有完成 ECS → Arch 生产迁移**。当前可以接受的结果是隔离探针、生产调用闭包盘点、
T2 的局部 World/身份生产切片、T3/T4 的编译-only 交接和 custom ECS 宿主基线；这些证据不能
替代完整 Arch 生产路径。T1 仍为 partial，T2–T5 的未整合 caller、生命周期、System、宿主和
旧框架退出部分必须继续标为 `partial`/`blocked-by-prerequisite`。

| 门禁 | 当前状态 | 验收含义 |
| --- | --- | --- |
| A0 基线与输入冻结 | partial | 已有多线源码指纹和主计划；性能输入/预算尚未冻结。 |
| A1 Arch 2.1.0 API 事实 | partial | Arch 包已实际 restore/build；T1 正式矩阵、10% 报告和 scoped commit 已接受，但 malformed-target CommandBuffer 路径仍有进程级崩溃且 `Chunk.cs` 未核验。 |
| A2 World、session token、身份签名 | partial / compile observed | T2 的 World bootstrap 与 UUID issuer 切片已提交；最新 issuer follow-up 的 Application 为 6 warning/0 error、Simulation 为 17 warning/0 error，NetworkServer 仅 restore；registry、slot、caller 和旧 runtime 退出仍未完成。 |
| A3 生命周期与关系 | partial / isolated compile only | T3-B3 隔离 Package/Lifecycle/StandardCore probe 的 Debug/Release 编译均为 0 warning/0 error；没有生产 Arch caller 闭包，关系行为保持 not-run。 |
| A4–A5 查询、System、网络、物品 | partial / compile-only handoff accepted | T4-B4 隔离 Arch.System probe 及受影响项目编译均 exit 0；Simulation 17 warning/0 error，其余列出的项目 0/0；生产仍未切入 Arch，行为保持 not-run。 |
| A6 宿主汇合与保存加载 | partial / not-run | T5 compile-only handoff 明确当前 C# build count 为 0；无已接受的 T2–T4 生产闭包，因此 Simulation、NetworkServer、WorldStorage 和宿主均 not-run。 |
| A7 性能与稳定性 | not-run | A0 性能预算未冻结，未有 Arch 默认宿主可测路径。 |
| A8 删除旧框架 | **禁止执行** | T1–T7 尚未闭环，旧引用仍存在；不得删除旧 runtime/store/compatibility alias。 |

## 2. 并行会话注册表

所有子会话均收到对应的完整执行文档，而不是只有简短提示。下表的 `partial` 是交付状态，
不是迁移完成声明。

| 长线 | 会话 ID | 模型/推理 | worktree | 当前状态 | commit / 交接 |
| --- | --- | --- | --- | --- | --- |
| T1 基线/API | `01a1179b-1b77-7332-aa96-8752173ae654` | `gpt-6-luna / max` | `C:\Users\shan\.codex\worktrees\32f2\NLTX` | partial；总验收已接受 handoff | `937bfba2a739f47d9d2aa34e7e690050f9a68450`；`T1/handoff-report.md` |
| T2 World/身份 | `01a1179b-1bd2-75c2-8805-b6156ba04055` | `gpt-6-luna / max` | `C:\Users\shan\.codex\worktrees\5c94\NLTX` | partial / production still incomplete | `5742970437f1cde67fa1f601c04120b135c4d39f` + `a1efd50`; `T2-B2/handoff.md` |
| T3 生命周期/关系 | `01a1179b-1a8a-7133-a80c-d7564de08575` | `gpt-6-luna / max` | `C:\Users\shan\.codex\worktrees\be5a\NLTX` | partial / isolated compile only | `7e472d27681829da008f0f3293d8ddd9fc6c8f91`; `T3-B3/handoff.md` |
| T4 查询/网络/物品 | `01a1179b-1bd6-79b1-b007-36ae25462be5` | `gpt-6-luna / max` | `C:\Users\shan\.codex\worktrees\d0b9\NLTX` | partial / production blocked | `9550753cb35344e44d3132bce8cfb6a37e943d10` + `51bd23d` + `f691393`; `T4-B4/handoff.md` |
| T5 宿主/性能/退出 | `01a1179b-1c9a-7880-b943-b2a9a638b578` | `gpt-6-luna / max` | `C:\Users\shan\.codex\worktrees\b69b\NLTX` | partial / prerequisite blocked | `0c2d186` + `f64c25b` + `e7b725f`; `T5/compile-only-handoff.md` |

后续每条长线只需记录受影响项目的编译命令/退出码/警告错误数/`Build/bin/` 输出路径和
未编译范围；不得运行测试或运行时探针。子会话继续使用 `/goal`，并在编译失败或反复尝试
时按 `C:\Users\shan\.agents\skills\pua\SKILL.md` 做排查记录；编排证据不等于行为门禁通过。

## 3. T1：基线与 Arch 2.1.0 原生 API

证据目录：`C:\Users\shan\.codex\worktrees\32f2\NLTX\Build\diagnostics\ArchMigration\T1\`

### 已观察事实

- 独立 `Test/Terraria.Arch.Verification/` 已实际 restore/build `Arch 2.1.0`。
- `net10.0` 实际选择 Arch `lib/net8.0` 资产；已解析的关键依赖包括 `Arch.LowLevel 1.1.5`、
  `Collections.Pooled 2.0.0-preview.27`、`CommunityToolkit.HighPerformance 8.2.2`、
  `Microsoft.Extensions.ObjectPool 7.0.0` 和 `ZeroAllocJobScheduler 1.1.2`。
- NuGet.org 签名验证已通过；包、nuspec、XML API 文档、固定源码和多个 hash 已保存。
- 已获得 `World.Destroy(World)` 调用 `Dispose()`、`Entity.Id/WorldId/Version`、
  `CommandBuffer.Create(ComponentType[])` 和 `Playback(World, bool dispose = true)` 的固定源码/API 证据。
- 已有五项代表性抽样的前四项通过：World ID/复用相关边界、跨 World 观察边界、struct/class 组件
  关键访问、Query 组合关键路径。

### 未接受的关键项

- T1 正式 `API behavior matrix`、10% 测试报告、partial handoff 和 scoped commit 尚未生成。
- CommandBuffer 抽样在修正断言后，故意触碰 malformed/missing component target 的路径触发
  进程级 `AccessViolationException`，退出码 `-1073741819`，堆栈涉及
  `Arch.Core.Chunk.GetArray` → `Arch.Buffer.CommandBuffer.Playback`。
- 该异常必须保留为 Arch 2.1.0 探针边界风险；不能被解释成生产迁移失败，也不能被忽略或改写成
  “全部通过”。T1 已停止新增测试/网络下载，转为固定源码和本地输出核验；若 `Chunk.cs` 源码未取到，
  handoff 必须将其标为未核验并保持 `partial`。

### T1 验收决定

`partial`。T1 handoff 与 scoped commit 已接受，但不能把未验证的组件、Query、ref 生命周期或
CommandBuffer malformed-target 行为扩写成生产 API 合同；T2–T5 只能消费报告中明确标注为已观察的
边界，并继续等待生产签名/生命周期整合。

## 3.1 范围增强：不得以新名字保留第二套通用 ECS

新增静态审计 [Arch 原生 API 覆盖与自定义 ECS 退出审计](../../reviews/audits/2026-10-08-arch-native-api-coverage-audit.md)
已作为“增强项待实施和行为验证”的范围输入，而不是迁移通过证据。审计确认当前主工作树生产
路径仍没有 `Arch.Core`/`Arch.System` 引用，`EntityRuntime`、`ComponentStore`、`ComponentAccess`、
通用 `RuntimeEntityHandle`、自制 `IWorldSimulationTickPhase` 和 Arch World 外的可写
`WorldSessionRestoreState` 仍然存在。

因此，后续计划仍保留以下设计边界；按编译-only 变更，本轮只把受影响项目编译作为硬门槛，
其余内容不运行验证：

- 通用 System 接口/组、世界级权威状态、组件存储、查询/编辑/快照、结构缓冲和通用关系图不能
  通过改名或转发层继续存在；领域规则、协议槽位、UUID 和文件 DTO 仍由各自 owner 负责。
- T2-B2 必须先交付唯一 Arch World owner、完整 token/WorldId/Version registry 和世界单例实体；
  T3/T4/T5 在此之前只能做不依赖生产签名的审计或夹具。
- `Arch.System`、`Arch.Relationships`、Arch-Events 的版本/配置/清理行为本轮不运行独立探针；
  它们必须标为未验证风险，不能从编译结果推导兼容性通过。

T2-B2 的执行合同见 [生产 World、身份与世界权威状态切换](../../plans/2026-10-08-arch-migration-track-t2-production-world-identity-execution.md)。
该合同不改变当前 A2 `not-run（生产）` 的行为结论；当前编译-only 门禁只要求当前源码和
scoped handoff 的受影响项目编译通过，运行时生产验证保持 `not-run`。

## 4. T2：World、身份与生产签名

交接：`C:\Users\shan\.codex\worktrees\5c94\NLTX\Build\diagnostics\ArchMigration\T2\handoff.md`

### 已接受证据

- 隔离 probe 实际 restore/build Arch 2.1.0，并通过 World/Entity 生命周期 smoke：
  `WorldId`、`Version`、`IsAlive`、不同 World 拒绝和重复 Dispose；0 warning、0 error。
- 已盘点 111 个生产 `.cs` 文件、1,033 条精确旧 runtime/API 引用、25 个项目和 59 条
  ProjectReference 边；`LoadedWorldSession`、`EntityRuntime`、`EntityIdentityRegistry`、
  `WorldStorageRoot` 的 owner/调用闭包已记录。
- 已明确迁移合同：Arch Entity 不能替代领域 UUID、协议 identity、slot ID 或 TileEntity ID；
  外部入口必须按 session token → `WorldId` → `World.IsAlive` → 生命周期/能力 → 组件访问顺序解析。

### 未接受证据与门禁

- T2-B2 已有局部生产切片：Application 直接引用 Arch 2.1.0，LoadedWorldSession 创建 Arch World/世界单例，
  并提交 `EntityUuidIssuer` 与 registry 的局部职责拆分；最新 issuer follow-up 的 Application 编译为 6/0，
  Simulation 编译为 17/0；警告均来自未修改依赖侧代码。
- EntityIdentityRegistry 仍未映射到 `(session token, Arch.Entity)`；`EntityRuntime`、`ComponentStore`、
  `ComponentAccess`、`RuntimeEntityHandle` 和 `WorldStorageRoot` 的旧生产路径仍在，slot/Network/lifecycle
  caller 尚未完成同批切换。默认 session issuer wiring 的唯一所有权也仍需后续整合。
- 未运行生产 session tick、候选 World 失败清理、旧 token 拒绝、UUID 复活/真实重建、生产 caller smoke；
  T2 只能作为 A2 的 partial compile evidence，不能作为 A2 完成证据。

## 5. T3：生命周期与关系

交接：`C:\Users\shan\.codex\worktrees\be5a\NLTX\docs\architecture\execution\2026-10-08-arch-migration-track-t3-lifecycle-relationships-execution.md`

### 已接受证据

- T3-B3 提交了隔离关系兼容性与生命周期编译 probe；PackageCompatibility、LifecycleProbe、StandardCoreProbe
  的 Debug/Release 编译记录均为 0 warning、0 error。
- 这些项目只证明隔离源码闭包可编译；没有生产 Player/NPC/Projectile/Item/TileEntity 的 Arch caller
  切换，也没有在当前规则下运行创建、销毁、关系清理、复用或世界退出场景。

### 未接受证据与门禁

- T3 仍未消费 T2 的完整 Arch World/token/identity caller 合同；现有关系清理和世界卸载证据属于历史或
  隔离编译上下文，不是 Arch 生产路径。
- 真实 published-world Player、RuntimeItemRegistry host、Leashed TileEntity、性能和所有运行时行为均
  保持 `not-run`。T3 状态为 `partial / blocked-by-prerequisite`，不可作为 A3 完成。

## 6. T4：查询、System、网络与物品

交接：`C:\Users\shan\.codex\worktrees\d0b9\NLTX\Build\diagnostics\ArchMigration\T4\handoff.md`

### 已接受证据

- 完成 B0 owner/read-write/structural-change/side-effect/phase-order 矩阵；记录 NPC 同 tick spawn/RNG
  顺序、Player/Projectile/WorldItem slot 顺序、队列闭包捕获风险、item revision 与具名 reservation
  的差异，以及 packet 29 stale/missing/duplicate 请求的 accepted-no-op 风险。
- 审计确认生产仍使用 `EntityRuntime` / `TryEdit` / `Match`；没有 Arch 包、Arch World、QueryDescription
  或原生查询，未猜测新 API，也没有添加 compatibility implementation。
- T4-B4 已提交隔离 `Arch.System 1.1.0` / `Arch 2.1.0` compile-only probe 及逐项目构建记录；受影响
  项目均 exit 0，Simulation 为 17 warning/0 error，其余列出的项目为 0/0。

### 未接受证据与门禁

- 没有把隔离 probe 当成生产迁移；B1 生产访问迁移、B2 队列请求重解析、B3 item 冲突保护以及
  所有运行时行为均未执行。
- T4 维持 `partial / blocked-by-prerequisite`，需先接受 T1 API、T2 World/identity 和 T3 lifecycle 合同。

## 7. T5：宿主、性能与旧框架退出

交接：`C:\Users\shan\.codex\worktrees\b69b\NLTX\Build\diagnostics\ArchMigration\T5\handoff-report.md`

### 已接受证据

- 当前 compile-only handoff 已提交，明确 T5 checkout 没有生产源或项目文件差异，因此 C# build count 为 0；
  Simulation、NetworkServer、Application、WorldStorage 和宿主均为 `not-run`。
- 更早的 custom ECS host smoke、Simulation/NetworkServer baseline build 和 A8 静态清单仅作历史背景，
  不作为当前门禁证据；没有 Arch host、世界切换、save/load、rollback/retry、benchmark 或 A8 删除。

### 未接受证据与门禁

- 当前没有可接受的 Arch host 编译闭包，因为 T2–T4 生产合同未整合到 T5 checkout；没有执行 C# build。
- 没有 Arch host、两次世界切换、rollback/retry、Arch save/reload、取消/零 tick、benchmark 或性能对照。
- T5 为 `partial / blocked-by-prerequisite`；不能把 custom ECS smoke 作为 A6–A7 或迁移完成证据。

## 7.1 编译-only 当前观察快照（未结算）

以下是主会话读取隔离 worktree 后的当前观察，不等于已合入或已接受：

- T2 `a1efd50` 工作树：World bootstrap 与 UUID issuer 局部生产切片已提交；最新 Application 编译为 6/0，
  Simulation 为 17/0，NetworkServer 仅 restore；registry/caller/旧 runtime 退出仍 partial。
- T3 `7e472d2` 工作树：隔离关系与生命周期 probe 的 Debug/Release 编译记录为 0/0；没有生产 Arch caller handoff。
- T4 `f691393` 工作树：已刷新 T2/T3 prerequisite 与 compile-only 口径；B4 隔离 probe 和列出的受影响项目编译 exit 0，生产仍未接入 Arch。
- T5 `e7b725f` 工作树：compile-only handoff 已补充当前 T2–T4 prerequisite snapshot，当前 C# build count 为 0；宿主和保存加载均 not-run。

因此当前只能接受这些作为“编译结果观察”和局部生产切片，不能更新为 A2–A6 完成，也不能授权 A8 删除。

## 8. 编译-only 门禁与历史证据分类

本轮严格区分三类结果：

1. **Arch 隔离探针**：只证明固定版本包/API 的局部行为，不证明生产整合。
2. **custom ECS 生产/领域夹具**：证明旧路径现有不变量和迁移前基线，不证明 Arch parity。
3. **未执行/阻塞**：在当前编译-only 规则下保持 `not-run`/`blocked`，不运行测试；若未来恢复
   运行时验收，再以当前源码和新产物重跑，不能复用历史 DLL、旧测试结果或局部 smoke。

当前不设测试预算。T1–T5 后续只执行受影响项目的增量编译；运行时、性能、关系清理、宿主
切换、保存加载和 A8 行为审计均标记为 `not-run`。历史 10% 样本和 custom ECS smoke 仅保留
作历史背景，不得作为本轮新增交付。

## 9. 合入与继续执行规则

在主工作树合入任何代码前，按以下顺序：

1. 先核对 T1 handoff、commit、编译输入和当前编译错误；没有正式 T1 交接，不推进 A2 生产 API
   改造。
2. T2/T3/T4/T5 的代码或测试文件必须逐文件比较；主工作树若已有同路径未跟踪文件，先保留并
   手工合并，禁止直接 cherry-pick 覆盖。尤其是 T3 `Test/Terraria.TileEntityVerification/Program.cs`
   的主工作树版本与 T3 alias 修正版必须先做差异核对。
3. 每次合入只构建受影响项目，输出留在 `Build/bin/`；不执行全 solution Rebuild，不执行测试或运行，
   不把旧 DLL 当成当前编译结果。
4. A2/A3/A4/A5 必须在同一套 session World/token/identity 合同上完成；不得建立第二套权威组件状态，
   不得把 Arch Entity 写入 UUID、协议、存档或 slot identity。
5. 编译-only 规则不授予 A8 删除权限；真实 Arch 默认宿主、保存加载、失败恢复、性能和旧引用清零
   仍是未验证风险，删除动作必须另行批准。

## 10. 下一轮明确动作

1. T1 保持 `partial` handoff（`937bfba2a739f47d9d2aa34e7e690050f9a68450`）；后续 owner 只需
   固定说明受影响项目的编译结果和未编译范围，不运行 Query/组件/ref/CommandBuffer 行为。
2. 总验收继续检查 T2 `a1efd50` 的 identity issuer/registry 切片及其 Application/Simulation 编译闭包；
   下一步仍是把 registry、WorldStorageRoot、slot 和 caller closure 一并迁到 Arch Entity/session token。
3. T3/T4/T5 的当前 handoff 已接受为 partial compile-only 记录；后续只在整合后的受影响项目上增量编译，
   生命周期、关系、网络、物品、宿主和性能运行项全部保留为 `not-run`，不以编译结果冒充行为通过。
4. 每次阶段完成都回填本总账，保持 `active`；编译通过不触发 `update_goal(status="complete")`，
   因为生产行为和 A8 仍未验证。

## 11. 当前回滚点与风险

- 子会话均为隔离 worktree；T2/T3/T4/T5 各自的 scoped commit 可单独审查或回退。
- 主工作树当前新增的并行执行文档和本总账均为文档变更；没有生产源代码切换。
- T1 CommandBuffer malformed-target 的进程级 `AccessViolationException` 是最高优先级风险，必须
  在接受任何 CommandBuffer 生产用法前由 owner 明确“禁止条件/隔离策略/升级或修复依据”。
- T5 发现的 326 世界版本和 generator 输出路径问题分别属于输入兼容性与测试宿主配置问题；二者都
  不能被当作 Arch 迁移通过，也不能被静默删除。
