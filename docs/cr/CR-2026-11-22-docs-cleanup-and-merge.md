# docs 删除、合并与归档记录

文档 ID：CR-2026-11-22-DOCS-CLEANUP-AND-MERGE
逻辑域：change-control
产物类型：decision、execution
状态：completed
日期：2026-11-22
canonical 路径：`docs/cr/CR-2026-11-22-docs-cleanup-and-merge.md`
上游依据：[docs 删除与整理审计](../reviews/audits/2026-11-22-docs-deletion-and-organization-audit.md)

## 1. 授权与范围

用户要求"删除一些文档、整理一些文档，将多个文档合并到一个文档保留精简信息"，并从审计给出的
四个范围选项中选定 **A + C**：

- **A（低风险集）**：删除 2 份根目录任务副本与 3 条失效 manifest 记录；把 22 份 NPC AI
  checkpoint 合并为 1 份；重写 README 目录树、manifest、库存快照。
- **C（归档）**：把 `component-decomposition/` 的初代材料移入 `archive/` 子目录，不删除。

本次全部改动限于 `docs/`。未修改源码、测试、项目配置或运行时行为；未运行构建或测试。

## 2. 删除

| 对象 | 依据 | 恢复方式 |
| --- | --- | --- |
| `docs/system-decomposition/2026-09-18-system-decomposition-non-authoritative-20-partition-tasks.md` | 与 `non-authoritative/` canonical 版本 **MD5 完全相同**；因目录浅一层，其 5 个相对链接（`../../../AGENTS.md` 等）全部指向仓库外 | `git show 6518acc6:docs/system-decomposition/2026-09-18-system-decomposition-non-authoritative-20-partition-tasks.md` |
| `docs/system-decomposition/2026-09-18-system-decomposition-authoritative-20-partition-tasks.md` | 与 canonical 版本仅差相对链接深度，属人工重复维护的第二份拷贝 | 同上，替换文件名 |
| `document-manifest.tsv` 中 3 条记录 | 目标文件在 HEAD 与工作树都不存在：`system-decomposition/reports/2026-09-17-ecs-system-domain-splitting-p06-process-retrospective.md`、`...-p06-system-experiment-optimization-report.md`、`2026-09-17-npc-damage-tracking-system-experiment-optimization-report.md` | 不需要，记录本身是错的 |

两份任务副本的删除使 CR-2026-09-19 描述的"根目录只读便利副本"不再成立；该 CR 作为历史
记录保留原文，其口径由本 CR 覆盖。

## 3. 合并

`docs/migration/ledgers/2026-10-07-npc-ai-*.md` 的 **22 份**检查点合并为
[NPC AI 会话检查点合并账本](../migration/ledgers/2026-10-07-npc-ai-session-checkpoint-ledger.md)
（11.6 KB，原 22 份共 128 KB），源文件删除。

保留的信息：每个切片的源码范围、落地的行为、验证结论、未闭合项、证据目录入口、固定来源
SHA-256（`NPC.cs` 等 6 项）、以及合并后的未闭合总表。

去掉的信息：逐条构建命令、逐场景断言清单、完整控制台日志引用，以及同一来源指纹在不同
切片中的重复。

**证据强度不变**：原文各自标注的 `not-run`、`partial/open`、`failed`、`rejected` 在合并
文档中逐项保留，第 8 节把它们汇总成未闭合总表；合并没有提升任何 profile 或 coverage 状态。

因为 22 份文件从未进入 `document-manifest.tsv`，合并减少了 22 个文件但对 manifest 行数无影响。

引用重定向：原本指向这 22 份文件的 13 处链接已改为指向合并账本，涉及
`migration/ledgers/2026-10-06-npc-ai-profile-coverage-ledger.md`、
`plans/system-decomposition/2026-10-06-npc-ai-system-execution-plan.md`、
`system-decomposition/2026-10-07-npc-ai-g3-reference-differential.md`。

## 4. 归档

| 源 | 目标 | 文件数 |
| --- | --- | ---: |
| `component-decomposition/baseline/` | `component-decomposition/archive/baseline/` | 40 |
| `component-decomposition/review-round-1/` | `component-decomposition/archive/review-round-1/` | 120 |

理由：这两批是初代基线和第一轮设计，其边界结论已被 `review-round-2/` 覆盖；但它们仍是
字段归属、审计结论和分级依据的原始输入，删除会使上游证据链断裂，因此降级为归档区。

`archive/` 只表示"不再是当前结论的输入"，不表示内容失效。目录内的相对链接（如
`archive/review-round-1/reports/` 到 `archive/baseline/`）在移动后仍然有效。

## 5. 链接影响

| 项 | 数量 | 处理 |
| --- | ---: | --- |
| 指向 `component-decomposition/baseline`、`review-round-1` 的链接 | 17 | 插入 `archive/` 段，保持原相对前缀 |
| 归档区内部链接的深度修正 | 4 | `../../../Context/...` → `../../../../Context/...` |
| 指向被合并 checkpoint 的链接 | 13 | 改指合并账本 |
| README、document-architecture 中的副本引用 | 4 处 | 改为 canonical 路径并注明删除 |

检查结果：仓库内**文档到文档**的失效链接从 31 个文件 / 309 处降到 **12 处**，其中 2 处是
README 已修正外的历史引用、10 处指向本 checkout 之外的依赖（`.agents/skills/`、`dome/`、
`Test/` 下已退出的验证项目）。指向 `src/` 的历史源码路径未逐条修复，按"当时的路径"保留。

## 6. 索引重算

- `README.md`：目录树按当前磁盘重写，补上 `architecture/{execution,subsystems}`、
  `system-decomposition/{design,execution}`、`plans/{minimal-implementation,network-message-registration}`、
  `component-decomposition/archive`；删除不存在的 `review-round-1/prompts` 与
  `review-round-2/{design,reports}` 描述；"先读什么"中指向不存在 Flowstate 入口的说明改为
  指向信息架构方案 `document-architecture.md`（该文件随后按
  [CR-2026-11-22-remove-docs-index](CR-2026-11-22-remove-docs-index.md) 整体删除）。
- `document-architecture.md`：§4 物理结构与 §4.1 任务入口规则同步，新增"不得再建立深度不同的
  副本"约束。
- `document-inventory.md`：按 2026-11-22 实况重写，582 文件 / 40.2 MB。
- `document-manifest.tsv`：432 行 → **570 行**，与磁盘 570 个 Markdown 一一对应，无缺失、
  无孤儿、无重复路径。新增行的 `migrationAction` 为 `added-2026-11-22`，归档行为
  `archived-2026-11-22`。
- `document-move-map.tsv`：追加本轮归档、删除与合并共 5 条记录。

## 7. 验证

| 检查 | 结果 |
| --- | --- |
| manifest 行数 vs 磁盘 Markdown | 570 = 570，diff 为空 |
| manifest 路径可解析 | 570/570 |
| 文档到文档失效链接 | 12（其中 10 个为 checkout 外部依赖） |
| 目录内空目录 | 0 |
| 字节级重复文件 | 0 组 |
| 源码/测试改动 | 无 |

未执行：构建、测试、运行时验证。本次改动不涉及可执行代码。

## 8. 回滚

全部改动可用一条命令回到本轮之前：

```powershell
git restore --source=6518acc6 --worktree -- docs
```

该提交是本次执行前的 HEAD。注意它会同时撤销工作树中本轮之前就存在的未提交改动
（`docs/document-manifest.tsv` 的先前修改、7 个未跟踪的 2026-11 计划文件不受影响）。
逐文件的源/目标映射见 `document-move-map.tsv`（该文件随后按
[CR-2026-11-22-remove-docs-index](CR-2026-11-22-remove-docs-index.md) 整体删除，内容可从
`6518acc6` 取回）；合并前的 22 份
checkpoint 原文可从 `6518acc6` 逐份取出。

## 9. 未做的事

- 未删除任何设计、审计、报告或 ledger 正文；只删了 2 份重复副本和 3 条错误索引记录。
- 未合并 `migration/ledgers/` 的 12 份「去除ID类文件」生成视图（约 14 MB）。这些是数据集
  切片，主从关系尚未声明，合并会丢失明细表；需先确定当前视图再进行。
- 未修复指向 `src/` 的历史源码链接（270 处），也未重跑分级或迁移审计。
