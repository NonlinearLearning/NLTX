# Arch 迁移执行总账

文档 ID：LEDGER-2026-10-07-ARCH-MIGRATION-EXECUTION  
日期：2026-10-08  
状态：**active / partial；总验收未通过，生产默认路径未切换，旧框架不得删除**  
总验收会话：当前主会话  
执行基线：`e2c686790ab6a4f14505ea914c27478f5959d061`（子会话均从该仓库状态建立隔离证据）

本总账记录基于以下权威输入：

- [自定义 ECS 转向 Arch 的执行计划](../../plans/2026-10-07-custom-ecs-to-arch-execution-plan.md)
- [Arch 2.1.0 官方 API 与迁移事实](../../research/2026-10-07-arch-api-migration-research.md)
- [并行长线执行协调文档](../../plans/2026-10-08-arch-parallel-execution-coordination.md)
- 五份长线执行合同：T1 基线/API、T2 World/身份、T3 生命周期/关系、T4 查询/网络/物品、T5 宿主/性能/退出。

## 1. 总体判定

本轮已经完成“拆分任务、建立详细执行合同、启动 5 个新会话、按 `gpt-6-luna + max`
运行、要求子会话使用 `/goal` 与 `pua`、限制约 10% 核心测试、由主会话验收”的编排目标。

本轮**没有完成 ECS → Arch 生产迁移**。当前可以接受的结果是隔离探针、生产调用闭包盘点、
旧 ECS 生命周期夹具和 custom ECS 宿主基线；这些证据不能替代 Arch 生产路径。T1 最终
API 行为矩阵和 handoff 尚未落盘时，T2–T5 的依赖部分必须继续标为 `blocked-by-prerequisite`。

| 门禁 | 当前状态 | 验收含义 |
| --- | --- | --- |
| A0 基线与输入冻结 | partial | 已有多线源码指纹和主计划；性能输入/预算尚未冻结。 |
| A1 Arch 2.1.0 API 事实 | partial | Arch 包已实际 restore/build；T1 正式矩阵、10% 报告和 scoped commit 已接受，但 malformed-target CommandBuffer 路径仍有进程级崩溃且 `Chunk.cs` 未核验。 |
| A2 World、session token、身份签名 | not-run（生产） | T2 只有隔离 probe；生产仍是 custom `EntityRuntime`。 |
| A3 生命周期与关系 | partial / blocked-by-prerequisite | T3 的约 10% 夹具基于旧 runtime；真实 Arch caller 未接线。 |
| A4–A5 查询、System、网络、物品 | partial / blocked-by-prerequisite | T4 完成审计，未改生产查询/API，未运行 T4 核心测试。 |
| A6 宿主汇合与保存加载 | partial / blocked-by-prerequisite | T5 只有 custom ECS 的一次 tick host smoke。 |
| A7 性能与稳定性 | not-run | A0 性能预算未冻结，未有 Arch 默认宿主可测路径。 |
| A8 删除旧框架 | **禁止执行** | T1–T7 尚未闭环，旧引用仍存在；不得删除旧 runtime/store/compatibility alias。 |

## 2. 并行会话注册表

所有子会话均收到对应的完整执行文档，而不是只有简短提示。下表的 `partial` 是交付状态，
不是迁移完成声明。

| 长线 | 会话 ID | 模型/推理 | worktree | 当前状态 | commit / 交接 |
| --- | --- | --- | --- | --- | --- |
| T1 基线/API | `01a1179b-1b77-7332-aa96-8752173ae654` | `gpt-6-luna / max` | `C:\Users\shan\.codex\worktrees\32f2\NLTX` | partial；总验收已接受 handoff | `0928445dbebdade501bcb39bb944541835538883`；`T1/handoff-report.md` |
| T2 World/身份 | `01a1179b-1bd2-75c2-8805-b6156ba04055` | `gpt-6-luna / max` | `C:\Users\shan\.codex\worktrees\5c94\NLTX` | partial / production blocked | `57f2c75b3f05bdb713bf421ae1ec7c2161f805e2`；`T2/handoff.md` |
| T3 生命周期/关系 | `01a1179b-1a8a-7133-a80c-d7564de08575` | `gpt-6-luna / max` | `C:\Users\shan\.codex\worktrees\be5a\NLTX` | partial / Arch prerequisite blocked | `f0a34c4a5e7707978cafd3ed1184ad053f53598f`；T3 execution report |
| T4 查询/网络/物品 | `01a1179b-1bd6-79b1-b007-36ae25462be5` | `gpt-6-luna / max` | `C:\Users\shan\.codex\worktrees\d0b9\NLTX` | partial / production blocked | `9bdde4e`；`T4/handoff.md` |
| T5 宿主/性能/退出 | `01a1179b-1c9a-7880-b943-b2a9a638b578` | `gpt-6-luna / max` | `C:\Users\shan\.codex\worktrees\b69b\NLTX` | partial / prerequisite blocked | `4a83188`；`T5/handoff-report.md` |

每条长线都被要求：仅执行约 10% 高风险核心测试，记录命令/退出码/警告错误数/输出路径和
源码及产物 hash；未运行项必须显式列出。子会话确实使用了 `/goal`，并在失败或反复尝试
时按 `C:\Users\shan\.agents\skills\pua\SKILL.md` 做了记录；这项编排证据不等于行为门禁通过。

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

- T2 没有修改生产签名；当前生产没有 Arch PackageReference、Arch World 或原生 Query。
- 未运行生产 session tick、候选 World 失败清理、旧 token 拒绝、UUID 复活/真实重建、生产 caller smoke。
- T2 只能作为迁移闭包与身份原则输入，不能作为 A2 完成证据。

## 5. T3：生命周期与关系

交接：`C:\Users\shan\.codex\worktrees\be5a\NLTX\docs\architecture\execution\2026-10-08-arch-migration-track-t3-lifecycle-relationships-execution.md`

### 已接受证据

- 约 10% 的旧 runtime 高风险夹具通过：Entity identity/构建/发布/终止/世界卸载，NPC task
  termination/旧 reference/重复释放/关系清理，Projectile create/terminate/generation reuse，
  Player lifecycle/death/respawn，Item reservation/motion/expiry/stale identity，以及 Training Dummy /
  Logic Sensor TileEntity fixture。
- 相关成功项报告为 0 warning、0 error；TileEntity 的双命名空间歧义以显式 alias 修复并记录。
- 明确记录了 Player 普通复活保留 UUID、真实重建产生新 root/UUID，NPC 父子关系按 child→parent
  清理，Projectile 和 Item 的 slot/generation/identity 责任，以及未接线 Leashed TileEntity 不纳入支持集。

### 未接受证据与门禁

- 这些是当前自定义 `EntityRuntime` 的领域/生命周期夹具，不是 Arch 生产路径。
- NPC world-load host 因生产 world-load entry 未发布 session 而阻塞；Arch probe、真实 published-world
  Player、RuntimeItemRegistry host、Leashed TileEntity、性能和全量测试均未执行。
- T3 的 Arch 迁移状态为 `partial / blocked-by-world-load-prerequisite`，不可作为 A3 完成。

## 6. T4：查询、System、网络与物品

交接：`C:\Users\shan\.codex\worktrees\d0b9\NLTX\Build\diagnostics\ArchMigration\T4\handoff.md`

### 已接受证据

- 完成 B0 owner/read-write/structural-change/side-effect/phase-order 矩阵；记录 NPC 同 tick spawn/RNG
  顺序、Player/Projectile/WorldItem slot 顺序、队列闭包捕获风险、item revision 与具名 reservation
  的差异，以及 packet 29 stale/missing/duplicate 请求的 accepted-no-op 风险。
- 审计确认生产仍使用 `EntityRuntime` / `TryEdit` / `Match`；没有 Arch 包、Arch World、QueryDescription
  或原生查询，未猜测新 API，也没有添加 compatibility implementation。

### 未接受证据与门禁

- 没有运行 T4 build/test/simulation smoke；B1 生产访问迁移、B2 队列请求重解析、B3 item 冲突保护、
  B4 约 10% 核心测试均未完成。
- T4 维持 `partial / blocked-by-prerequisite`，需先接受 T1 API、T2 World/identity 和 T3 lifecycle 合同。

## 7. T5：宿主、性能与旧框架退出

交接：`C:\Users\shan\.codex\worktrees\b69b\NLTX\Build\diagnostics\ArchMigration\T5\handoff-report.md`

### 已接受证据

- 当前 custom ECS 兼容输入上的最小 host smoke 通过：WorldFile 319、固定 seed、单玩家、1 tick、
  `Succeeded=true`、最终 tick 为 1、session published、原始 `.wld` hash 未变。
- Simulation/NetworkServer/WorldGeneration 的基线构建结果和失败日志已记录；WorldFile 326 因版本/
  `world.backgrounds.load` 不支持而失败，未将其算作通过。
- A8 只读审计发现 62 个 production-candidate 源文件和 43 个测试文件命中旧框架相关符号；删除未执行。

### 未接受证据与门禁

- fixture host 构建曾因 generator 输出目录与普通 `Build/bin` 硬编码路径不一致失败；普通 Simulation
  build 通过但有 23 warnings，NetworkServer 和 WorldGeneration build 通过。
- 没有 Arch host、两次世界切换、rollback/retry、Arch save/reload、取消/零 tick、benchmark 或性能对照。
- T5 为 `partial / blocked-by-prerequisite`；不能把 custom ECS smoke 作为 A6–A7 或迁移完成证据。

## 8. 测试预算与证据分类

本轮严格区分三类结果：

1. **Arch 隔离探针**：只证明固定版本包/API 的局部行为，不证明生产整合。
2. **custom ECS 生产/领域夹具**：证明旧路径现有不变量和迁移前基线，不证明 Arch parity。
3. **未执行/阻塞**：必须在下一轮以当前源码和新产物重跑，不能复用历史 DLL、旧测试结果或局部 smoke。

各长线按约 10% 高风险预算运行；T4 的生产 B4 明确 `not-run`，T2 的生产矩阵明确 `not-run`，
T5 的 smoke 只计作 custom ECS 复合场景，T1 的最终样本必须在正式 handoff 中给出确切选择/跳过清单。

## 9. 合入与继续执行规则

在主工作树合入任何代码前，按以下顺序：

1. 先核对 T1 handoff、commit、probe source/DLL/PDB/input hash 和 AccessViolation 原始日志；没有
   正式 T1 交接，不推进 A2 生产 API 改造。
2. T2/T3/T4/T5 的代码或测试文件必须逐文件比较；主工作树若已有同路径未跟踪文件，先保留并
   手工合并，禁止直接 cherry-pick 覆盖。尤其是 T3 `Test/Terraria.TileEntityVerification/Program.cs`
   的主工作树版本与 T3 alias 修正版必须先做差异核对。
3. 每次合入只构建受影响项目，输出留在 `Build/bin/`；不执行全 solution Rebuild，不把旧 DLL 当成新证据。
4. A2/A3/A4/A5 必须在同一套 session World/token/identity 合同上完成；不得建立第二套权威组件状态，
   不得把 Arch Entity 写入 UUID、协议、存档或 slot identity。
5. 只有真实 Arch 默认宿主、保存加载、失败恢复、性能门禁和旧引用清零全部有当前源码证据后，才可以
   另开 A8 删除提交；本总账不授予删除权限。

## 10. 下一轮明确动作

1. T1 已提交 `partial` handoff（`0928445dbebdade501bcb39bb944541835538883`）；后续 owner 必须固定说明
   Query/组件/ref 行为、CommandBuffer 崩溃证据、未核验 Chunk 源码和 10% 样本，不得扩大为生产通过。
2. 总验收读取 T1 报告后，检查 T2 隔离 probe 与 T2 production signature 之间的差距；没有猜测 API，
   只在合同明确后切换 `LoadedWorldSession`、identity registry、WorldStorageRoot 和 caller closure。
3. T3 先恢复真实 published-world host，再把生命周期/关系夹具迁到 Arch World；T4 从一个有序的
   非结构访问切片开始，补 ref/copy/structural boundary 后再处理网络队列和 item reservation。
4. T5 在 Arch 默认 Simulation/NetworkServer 装配后重跑宿主、切换、rollback/retry、save/load 和性能。
5. 每次阶段完成都回填本总账，保持 `active`，直到总验收明确达到 A0–A8 门禁；在此之前不调用
   `update_goal(status="complete")` 或 `update_goal(status="blocked")`。

## 11. 当前回滚点与风险

- 子会话均为隔离 worktree；T2/T3/T4/T5 各自的 scoped commit 可单独审查或回退。
- 主工作树当前新增的并行执行文档和本总账均为文档变更；没有生产源代码切换。
- T1 CommandBuffer malformed-target 的进程级 `AccessViolationException` 是最高优先级风险，必须
  在接受任何 CommandBuffer 生产用法前由 owner 明确“禁止条件/隔离策略/升级或修复依据”。
- T5 发现的 326 世界版本和 generator 输出路径问题分别属于输入兼容性与测试宿主配置问题；二者都
  不能被当作 Arch 迁移通过，也不能被静默删除。
