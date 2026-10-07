# Arch 迁移并行长线执行协调文档

文档 ID：DOC-2026-10-08-ARCH-PARALLEL-COORDINATION  
逻辑域：plans / migration / orchestration  
产物类型：execution-coordination  
状态：active；仅定义并行执行与验收，不代表任何迁移批次已经完成  
日期：2026-10-08  
基线计划：[自定义 ECS 转向 Arch 的执行计划](2026-10-07-custom-ecs-to-arch-execution-plan.md)  
API 事实：[Arch 2.1.0 官方 API 与迁移事实](../research/2026-10-07-arch-api-migration-research.md)

2026-10-08 范围增强：[Arch 原生 API 全面覆盖审计](../reviews/audits/2026-10-08-arch-native-api-coverage-audit.md)。用户要求全部通用 ECS 机制采用 Arch API；以下合同已增加 System、世界单例、关系适用性与全面退出条件。文件更新不证明既有会话已读取增补或其运行目标已改变。

2026-10-08 验收规则变更：[CR-2026-10-08 编译-only 门禁](../../.agent-workplace/changes/CR-2026-10-08-arch-compile-only-acceptance.md)。从本条起，子会话不得运行测试、冒烟、benchmark、simulation、host、WorldFile 或其他运行时验证；只需对受影响项目执行增量编译并提交编译结果。历史运行证据不删除，但不再是本轮门禁。

## 1. 本轮目标

本轮把 A0–A8 迁移计划拆成 5 条长线，由 5 个新会话并行推进。每条长线都拥有独立
worktree、独立证据目录和独立交接报告。新会话必须自行使用 `/goal` 管理长线目标，并
在遇到失败、重复尝试或“环境问题”猜测时使用 `pua` skill 做强制排查；不能把未验证的
推测当成完成结论。

本会话是总验收者，负责：

1. 检查每条长线是否遵守范围、依赖和文件边界。
2. 汇总源码差异、受影响项目编译输出和编译失败范围。
3. 按依赖顺序在主工作树中验收，只把当前源码的编译结果作为本轮门禁结果。
4. 运行时行为、性能、关系清理、世界切换和生产默认路径不在本轮确认；旧框架删除仍需单独获准。

## 2. 并行分区

| 长线 | 会话任务 | 主计划批次 | 允许的主要范围 | 依赖与整合顺序 |
| --- | --- | --- | --- | --- |
| T1 | 基线与 Arch 原生 API 探针 | A0–A1 | 核心、System、关系/Events 兼容性探针与包闭包 | 先产出；所有实现线读取其报告 |
| T2 | World、身份与生产签名 | A2 | 会话、世界单例组件、身份、槽位投影及签名闭包 | T1 后整合；先于 T3/T4 的最终编译 |
| T3 | 实体创建、销毁与关系 | A3 | Player/NPC/Projectile/Item/TileEntity 的创建、生命周期、关系清理 | 依赖 T2 合同；与 T4 可并行开发 |
| T4 | System、查询、网络与物品提交 | A4–A5 | 官方 System 接口、原生查询、包装退出、网络/物品保护 | 依赖 T1/T2，消费 T3 的生命周期合同 |
| T5 | 宿主汇合、测量与退出审计 | A6–A8 | Simulation/NetworkServer/保存加载、性能、旧框架退出审计 | 最后整合；删除动作须等待总验收批准 |

### 2.1 依赖图

```text
T1(A0-A1)
   |
   v
T2(A2) ------+----------------+
   |         |                |
   v         v                |
T3(A3) --> T4(A4-A5) --------+
                                v
                         T5(A6-A8)
```

并行不是跳过依赖。T3/T4/T5 可以先做扫描、接口草案、测试夹具和不依赖新签名的局部
实现；如果依赖报告还没有出现，必须把被依赖部分标为 `blocked-by-prerequisite`，不能
复制一套猜测 API，也不能声称整条长线完成。

## 3. 所有子会话的共同执行合同

每个新会话收到本表对应的完整长线文档后，必须完成以下事项：

1. 先读取根 `AGENTS.md`、`Context/progress.md`、`Context/约束/开发协作与变更约束.md`
   和 `Context/约束/构建与验证约束.md`；按本线涉及的 ECS、实体、基础设施、副作用和
   C# 主题继续读取对应文档。
2. 以收到的长线文档为 `/goal` 的目标正文；不要另起一个模糊目标。目标必须说明范围、
   交付物、完成条件和当前阻塞项。
3. 明确使用 `C:\Users\shan\.agents\skills\pua\SKILL.md`。连续两次失败、反复微调
   同一方案或准备归因环境时，必须切换本质不同的排查方案，并记录已读错误、搜索、
   原始上下文、前置假设、反向假设、最小隔离和新方向。
4. 不执行任何测试或运行验证。只构建受影响项目，记录项目、命令、退出码、warning/error
   数和 `Build/bin/` 输出路径；编译通过不表示行为通过。
5. 所有输出放入 `Build/diagnostics/ArchMigration/<track>/`；不把 DLL、日志、快照或临时
   文件写进 `src/`、项目目录或 `docs/`。新建文档必须遵守 ECS 文件组织及流程边界。
6. 每次声称通过，都记录命令、项目、退出码、warning/error 数、输出路径、source hash、
   DLL/PDB hash、输入 hash；不沿用与当前源码不对应的历史产物。
7. 保留 worktree 中的既有用户改动，不执行 `git reset --hard`、`git clean`、批量删除或
   与本线无关的格式化。不要修改 `Context/` 约束文件。
8. 每条长线结束时提交可审查的 commit（若有代码变更），并生成交接报告，报告必须区分
   `done`、`partial`、`blocked`、`not-run`，列出下一线需要的精确输入。
9. 读取全面覆盖审计 N1–N5 并记录新增条件的接收状态；不得复制旧框架为新的泛型 facade、
   System 接口/组、关系图或结构缓冲。旧交接必须补充其未覆盖的新增能力，不能仅凭原完成条件结算。

## 4. 编译-only 政策

- 原有“约 10% 核心测试”规则由 CR-2026-10-08 暂停，本轮不得运行测试。
- 不执行 `dotnet test`、`dotnet run`、benchmark、fixture、simulation、host smoke、WorldFile
  或 Arch API 行为探针。
- 只执行受影响项目的增量 `dotnet build`；除非依赖恢复是编译前置，否则不额外运行命令。
- 交接必须明确编译成功/失败、未编译项目和当前阻塞；不得把编译成功写成运行时行为通过。
- 既有 10% 运行证据保留为历史记录，不作为当前子会话交付要求。

## 5. 总验收门槛

本会话收到子会话交接后按以下顺序验收：

1. T1：Arch 2.1.0/Arch.System 及其受影响探针项目编译通过；不运行关系、Events 或行为探针。
2. T2：受影响的 World/身份/生产 caller 项目编译通过；World 单例和身份语义只作为代码设计目标，
   不运行确认。
3. T3：受影响的生命周期/关系 caller 项目编译通过；不运行 Spawn/Release、清理或复用场景。
4. T4：受影响的 System/查询/网络/物品项目编译通过；不运行 ref、队列或冲突保护场景。
5. T5：受影响宿主/保存加载项目编译通过；不运行宿主、切换、保存加载、失败恢复或性能。
6. 最终报告必须把未编译、未接线和未运行内容明确标为 `not-run`/`blocked`；本轮不得声称
   行为、性能或 A8 退出已完成。

## 6. 交接报告最低格式

```text
Track: Tn
Status: done | partial | blocked | not-run
Goal: ...
Changed files / commits: ...
Dependency evidence consumed: ...
Build commands and results: ...
Compile-only gate: affected projects / commands / exit codes / warning-error counts / output paths
Runtime tests and verification: not-run by change control
Known failures and exact first error: ...
Uncovered support set: ...
Rollback point: ...
Next owner action: ...
```
