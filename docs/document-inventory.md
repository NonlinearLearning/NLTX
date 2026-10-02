# 文档库存快照

盘点时间：2026-09-19。范围：仓库根目录 `docs/` 在该日的工作树快照；文件计数不代表本次删除后的实时数量。

当前工作树共有 390 个文件：380 个 Markdown、7 个 JSON、3 个 TSV。Markdown 文档已经
按领域重新归档；JSON/TSV 与迁移 ledger 保持相邻，避免结构化事实源和说明文档分离。

## 1. 当前顶层库存

| 当前路径 | 文件数 | Markdown | JSON | TSV | 主要内容 |
| --- | ---: | ---: | ---: | ---: | --- |
| `docs/architecture/` | 2 | 2 | 0 | 0 | ADR 和实体身份上下文 |
| `docs/system-decomposition/` | 5 | 5 | 0 | 0 | 快照时权威/非权威 P01-P20 任务；当前非权威范围为 P01-P15、P18 |
| `docs/component-decomposition/` | 249 | 249 | 0 | 0 | 初代基线、第一/二轮组件设计、执行和审计 |
| `docs/migration/` | 63 | 55 | 7 | 1 | 项目能力评估、成员/字段 ledger、分区表和索引 |
| `docs/reviews/` | 2 | 2 | 0 | 0 | 项目审计和人工审查 |
| `docs/plans/` | 49 | 49 | 0 | 0 | Component/System 实验计划和 PRD；System 分区任务已移出 |
| `docs/research/` | 10 | 10 | 0 | 0 | 外部资料、源码调研、API/DAG 研究 |
| `docs/cr/` | 5 | 5 | 0 | 0 | 变更记录和组件历史 CR |
| 根级管理入口 | 5 | 3 | 0 | 2 | README、架构方案、库存快照、逐文件 manifest、移动映射 |
| **合计** | **390** | **380** | **7** | **3** | 当前物理结构 |

## 2. 领域内部结构

```text
docs/
├── architecture/
│   ├── decisions/
│   └── entity-identity-context.md
├── system-decomposition/
│   ├── 2026-09-02-system-split-markdown-writing-guide.md
│   ├── 2026-09-18-system-decomposition-authoritative-20-partition-tasks.md # 只读便利副本
│   ├── 2026-09-18-system-decomposition-non-authoritative-20-partition-tasks.md # 只读便利副本
│   ├── authoritative/
│   ├── non-authoritative/
│   └── reports/                    # 分区报告输出区域，按需创建
├── component-decomposition/
│   ├── baseline/
│   ├── review-round-1/{design,reports,prompts}
│   └── review-round-2/{design,reports,non-authoritative}
├── migration/
│   ├── assessments/
│   └── ledgers/{authoritative-20-partitions,non-authoritative-component-partitions}
├── reviews/{audits,human}/
├── plans/{component-decomposition,system-decomposition,tasks}/
├── research/
└── cr/
```

`review-round-1` 和 `review-round-2` 的文件名保留原有历史语义；`design`、`reports`、
`non-authoritative` 和分区目录只承担导航职责，不改变文档结论。

System 分区任务的 canonical 入口位于 `system-decomposition/authoritative/` 和
`system-decomposition/non-authoritative/`：权威任务覆盖 P01-P20，非权威当前任务覆盖
P01-P15、P18；根目录另有两份只读便利副本。副本不参与 runner 初始化、领取或状态判断。
`plans/system-decomposition/` 只保留实验设计和实施计划。任务状态以对应 runner 的共享
ledger 为准；任务文档中的表格是范围和输出路径索引，不是人工维护的领取状态表。

## 3. 移动与链接状态

本次移动由 [CR-2026-09-18](cr/CR-2026-09-18-docs-md-structure-reorganization.md) 授权，
逐目录映射记录在 [document-move-map.tsv](document-move-map.tsv)。移动规则如下：

- 文件内容未因目录整理而删除；
- 旧中文外层目录被替换为领域目录，历史轮次和分区仍然保留；
- `CONTEXT.md` 被重命名为 `architecture/entity-identity-context.md`，因为它是架构术语入口；
- 迁移 ledger 的 JSON/TSV 与相关 Markdown 一起移动；
- 可确定的 Markdown 相对链接和现有路径文字已同步，无法证明目标的外部路径未被猜测修改。

Markdown 链接门禁结果：共扫描 676 个链接；153 个仓库内相对链接直接有效，9 个带行号的
报告链接在去除行号后指向现有文件，另有 17 个链接指向当前仓库之外的
`.agents/skills/ecs-system` 参考资料。后 17 个是外部技能依赖，不纳入
仓库内路径迁移，也没有被伪造为本地目标；除此之外没有发现迁移造成的仓库内失链。

## 4. 重复检查

| 检查项 | 结果 | 说明 |
| --- | --- | --- |
| 文件总数 | 390 | 与当前工作树递归文件统计一致 |
| Markdown | 380 | 包含指南、两份 canonical 任务表和两份根目录便利副本 |
| JSON | 7 | 迁移索引、映射和 ledger 结构化材料 |
| TSV | 3 | 源码覆盖表、移动映射和逐文件 manifest |
| 同名文件 | 仍有历史同名文件 | 保留在不同的 `design`/`reports`/轮次上下文中，未覆盖 |
| 文件级 manifest | 已生成 | 见 `document-manifest.tsv` |

## 5. Git 工作树说明

当前工作树原本就包含大量旧路径删除、暂存新增/修改和未跟踪目录。本次只对现有 `docs/`
内容执行了 CR 中列出的结构移动，并保留了这些变更的内容；没有执行 `git reset`、
`git checkout`、`git clean` 或递归删除。

Git 可能把“旧路径删除 + 新路径未跟踪”显示成删除和新增，而不是自动显示为 rename。是否
最终合并、暂存或提交由后续 Git 操作决定，本文件只描述当前工作树事实。

## 6. Manifest 字段

`document-manifest.tsv` 为每个当前 Markdown 文件记录：

```text
path, logicalDomain, artifactType, status, canonical, sourceScope,
relatedPlan, relatedEvidence, owner, lastReviewed, migrationAction
```

其中 `owner` 是文档维护责任，不是源码推断的 System owner；`accepted` 只表示该文档声明
范围的证据通过，不能外推为整个旧系统迁移完成。
