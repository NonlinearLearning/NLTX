# Arch 迁移并行长线执行协调文档

文档 ID：DOC-2026-10-08-ARCH-PARALLEL-COORDINATION  
逻辑域：plans / migration / orchestration  
产物类型：execution-coordination  
状态：active；仅定义并行执行与验收，不代表任何迁移批次已经完成  
日期：2026-10-08  
基线计划：[自定义 ECS 转向 Arch 的执行计划](2026-10-07-custom-ecs-to-arch-execution-plan.md)  
API 事实：[Arch 2.1.0 官方 API 与迁移事实](../research/2026-10-07-arch-api-migration-research.md)

## 1. 本轮目标

本轮把 A0–A8 迁移计划拆成 5 条长线，由 5 个新会话并行推进。每条长线都拥有独立
worktree、独立证据目录和独立交接报告。新会话必须自行使用 `/goal` 管理长线目标，并
在遇到失败、重复尝试或“环境问题”猜测时使用 `pua` skill 做强制排查；不能把未验证的
推测当成完成结论。

本会话是总验收者，负责：

1. 检查每条长线是否遵守范围、依赖和文件边界。
2. 汇总源码差异、构建输出、10% 核心测试证据和未覆盖范围。
3. 按依赖顺序在主工作树中验收，不把子会话的“本地通过”直接视为迁移完成。
4. 只有在 A0–A8 证据闭环后，才允许进入最终旧框架删除和生产默认路径确认。

## 2. 并行分区

| 长线 | 会话任务 | 主计划批次 | 允许的主要范围 | 依赖与整合顺序 |
| --- | --- | --- | --- | --- |
| T1 | 基线与 Arch 原生 API 探针 | A0–A1 | 基线清单、探针项目、Arch 包与 API 事实 | 先产出；所有实现线读取其报告 |
| T2 | World、身份与生产签名 | A2 | LoadedWorldSession、身份 registry、WorldStorageRoot、生产签名闭包 | T1 后整合；先于 T3/T4 的最终编译 |
| T3 | 实体创建、销毁与关系 | A3 | Player/NPC/Projectile/Item/TileEntity 的创建、生命周期、关系清理 | 依赖 T2 合同；与 T4 可并行开发 |
| T4 | System、查询、网络与物品提交 | A4–A5 | 原生查询、ref 边界、排队请求、物品冲突保护 | 依赖 T2，消费 T3 的生命周期合同 |
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
4. 只做约 10% 的核心测试：选择本线风险最高、能代表主链路的最小测试子集，写明选择
   理由、测试总量估算、实际执行项和未执行项。构建受影响项目不等于执行完整测试套件，
   但构建、编译诊断和一条最小冒烟路径仍必须提供。
5. 所有输出放入 `Build/diagnostics/ArchMigration/<track>/`；不把 DLL、日志、快照或临时
   文件写进 `src/`、项目目录或 `docs/`。新建文档必须遵守 ECS 文件组织及流程边界。
6. 每次声称通过，都记录命令、项目、退出码、warning/error 数、输出路径、source hash、
   DLL/PDB hash、输入 hash；不沿用与当前源码不对应的历史产物。
7. 保留 worktree 中的既有用户改动，不执行 `git reset --hard`、`git clean`、批量删除或
   与本线无关的格式化。不要修改 `Context/` 约束文件。
8. 每条长线结束时提交可审查的 commit（若有代码变更），并生成交接报告，报告必须区分
   `done`、`partial`、`blocked`、`not-run`，列出下一线需要的精确输入。

## 4. 10% 核心测试政策

“只执行 10%”是本轮子会话的测试预算，不是降低证据诚实性的许可：

- 不能运行全量测试后只汇报 10%。
- 不能把编译成功写成行为通过。
- 应优先覆盖会导致数据损坏、跨世界误命中、引用失效、重复提交或资源泄漏的测试。
- 未执行的测试必须进入报告和总验收清单；总验收阶段再决定是否补跑。

## 5. 总验收门槛

本会话收到子会话交接后按以下顺序验收：

1. T1：Arch 2.1.0 在仓库目标 SDK/net10.0 下实际 restore/build/run，且 API 假设有探针证据。
2. T2：一个会话一个 Arch World；身份 token、WorldId、Version 和领域 UUID 没有混淆；生产
   caller 的签名闭包可构建。
3. T3：创建失败、重复释放、实体复用、关系断开及至少一类真实 Spawn/Release 路径有证据。
4. T4：查询/ref 生命周期、网络排队重解析和物品提交冲突保护有证据；没有第二份权威组件状态。
5. T5：真实宿主、切换、保存加载、失败恢复和性能测量有当前源码证据；A8 的删除仅在前四
   线全部可接受后执行。
6. 最终报告不得把未执行的全量矩阵、未接线内容、旧 DLL 结果或历史文档结论冒充通过。

## 6. 交接报告最低格式

```text
Track: Tn
Status: done | partial | blocked | not-run
Goal: ...
Changed files / commits: ...
Dependency evidence consumed: ...
Build commands and results: ...
10% core tests: selected / executed / skipped / rationale
Known failures and exact first error: ...
Uncovered support set: ...
Rollback point: ...
Next owner action: ...
```

