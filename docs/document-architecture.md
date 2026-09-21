# 文档信息架构与管理方案

状态：N4 批次迁移已完成，进入 N6 文档/链接门禁；盘点日期：2026-09-18；范围：仓库根目录 `docs/`。

本文档解决“文档应该属于哪个信息域、如何从旧目录找到它、如何验证移动没有破坏证据链”的
问题。物理移动已经按照 [document-move-map.tsv](document-move-map.tsv) 和
[CR-2026-09-18](cr/CR-2026-09-18-docs-md-structure-reorganization.md) 执行；文档结论、
运行时代码和测试代码没有改变。

## 1. 设计目标

当前文档集合同时包含组件设计、System 拆分实验、迁移 ledger、研究记录和审计材料。单纯
按产生日期或“第一轮/第二轮”查找，会导致以下问题：

- System 设计和 Component 设计混在同一审查语义下，无法判断文档的边界对象；
- 设计、实施、审计和行为证据没有明确的产物关系；
- `HEAD` 中的旧路径与工作树中的新路径曾经并存，容易误删或重复移动；
- 成员/字段迁移表被误当作 owner 证明，研究结论被误当作源码调用证据；
- 缺少根级入口，维护者只能依赖文件名和目录枚举。

本方案的目标是：

1. 以稳定的逻辑分类驱动物理移动，并保留可回滚映射；
2. 让一个 System 或 Component 的设计、计划、实施和验证可以沿同一主题追踪；
3. 保留历史证据正文，更新其当前路径引用，避免移动后失链；
4. 把路径归属、文档状态和行为验收分开表达；
5. 让后续 20 分区和 System API 拆分可以批量、可回滚地管理。

## 2. 两个正交维度

目录只表达稳定的信息域；文档的生命周期和产物类型由索引或文档头部表达，不重复创建
“设计/实现/报告/最终版”层级。

### 2.1 信息域

| 逻辑域 | 解决的问题 | 允许的主要内容 |
| --- | --- | --- |
| `architecture` | 已确认的架构、约束和决策是什么 | ADR、边界原则、命名和组织规则 |
| `system-decomposition` | 一个 System 的概念行为、API、owner、调度和迁移如何划分 | 边界设计、API 映射、调用/依赖 DAG、实验、行为证据、复盘 |
| `component-decomposition` | 状态、字段、组件和读写归属如何划分 | 组件设计、字段审计、20 分区设计/执行、组件审计 |
| `migration` | 旧成员、旧入口和新实现如何对应 | ledger、源码声明、映射表、迁移项目评估 |
| `research` | 尚未成为结论的外部资料或源码调查是什么 | 资料、调用搜索、边界研究、候选分析 |
| `reviews` | 谁复核了什么，结论的证据范围是什么 | 人工审查、审计、复盘、返工意见 |
| `plans` | 下一批要做什么，完成条件是什么 | 设计计划、实施计划、任务、PRD |
| `change-control` | 为什么改变范围、路径或流程 | CR、例外说明、回滚记录 |
| `archive` | 不再活跃但必须保留的历史材料 | 已完成批次、旧索引、封存快照 |

### 2.2 产物类型

产物类型不是新的顶层目录，而是文档的属性和文件名语义：

| 类型 | 含义 | 不能替代的证据 |
| --- | --- | --- |
| `design` | 提出边界、概念 API、组件或规则 | 不能证明实现已接入 |
| `plan` | 规定批次、顺序、负责人和 DoD | 不能证明计划已完成 |
| `claimable-task` | 规定可由 runner 领取的分区范围、输入和输出 | 不能替代分区报告或迁移验收证据 |
| `claimable-task-copy` | 为导航和人工查阅保留的分区任务只读副本 | 不是 canonical 任务表，不能作为 runner 状态或领取状态来源 |
| `execution` | 记录实际代码或工具实施 | 不能单独证明旧行为一致 |
| `evidence` | 记录调用、读写、调度、输出或行为观察 | 必须标注观察范围和缺口 |
| `audit` | 对一组设计或实现进行复核 | 不能替代原始源码证据 |
| `decision` | 记录已批准的选择、例外或变更 | 必须有影响面和撤销条件 |
| `retrospective` | 记录流程缺陷与优化项 | 不改变前一批次的验收结论 |
| `guide` | 规定文档预处理、编写和输出约定 | 不能替代分区报告或迁移验收证据 |

## 3. 当前路径到逻辑域的映射

以下是已经落地的当前路径映射。源路径和目标路径的逐目录记录见
[document-move-map.tsv](document-move-map.tsv)。

| 当前路径 | 逻辑域 | 主要产物 | 本阶段处理 |
| --- | --- | --- | --- |
| `docs/system-decomposition/` | `system-decomposition` | guide、claimable-task、claimable-task-copy、evidence、audit、retrospective | 已完成；指南、权威/非权威 P01-P20 任务入口、根目录只读副本和分区报告输出区域 |
| `docs/plans/system-decomposition/` | `plans` + `system-decomposition` | plan | 已完成；保留 System 实验设计和实施计划，P01-P20 任务入口已移至 System 领域目录 |
| `docs/component-decomposition/baseline/` | `component-decomposition` | design、baseline | 已完成；早期基线不覆盖第二轮结论 |
| `docs/component-decomposition/review-round-1/` | `component-decomposition` | design、evidence | 已完成；保留第一轮设计、报告和提示词子目录 |
| `docs/component-decomposition/review-round-2/` | `component-decomposition` | design、execution、audit | 已完成；P01-P20 成对查阅设计与执行 |
| `docs/migration/ledgers/` | `migration` | ledger、mapping、source declaration | 已完成；成员级证据不自动构成 owner 证明 |
| `docs/migration/assessments/` | `migration` | assessment | 已完成；项目级迁移能力评估 |
| `docs/architecture/decisions/`、`entity-identity-context.md` | `architecture` | decision、context | 已完成；架构决策与术语约束集中管理 |
| `docs/cr/` | `change-control` | decision、scope change | 已完成；Flowstate 兼容入口 |
| `docs/plans/component-decomposition/`、`tasks/` | `plans` | plan、task、PRD | 已完成；组件批次计划与任务分开 |
| `docs/research/` | `research` | research、evidence | 保留；研究结论必须标明源码/调用证据范围 |
| `docs/reviews/human/`、`audits/` | `reviews` | audit、review | 已完成；人工约束和机器证据分开 |

## 4. 实际物理结构

下面是本次已经落地的结构。目录名表达信息域，轮次和分区只在领域内部表达历史上下文。

```text
docs/
├── README.md
├── architecture/{decisions,entity-identity-context.md}
├── system-decomposition/
│   ├── 2026-09-02-system-split-markdown-writing-guide.md
│   ├── 2026-09-18-system-decomposition-authoritative-20-partition-tasks.md # 只读便利副本
│   ├── 2026-09-18-system-decomposition-non-authoritative-20-partition-tasks.md # 只读便利副本
│   ├── authoritative/              # 权威 P01-P20 任务入口
│   ├── non-authoritative/          # 非权威 P01-P20 任务入口
│   └── reports/                    # 分区报告输出区域，按需创建
├── component-decomposition/{baseline,review-round-1,review-round-2}/
├── migration/{assessments,ledgers}/
├── reviews/{audits,human}/
├── plans/{component-decomposition,system-decomposition,tasks}/
├── research/
└── cr/
```

目录按需创建：没有真实文档时不创建空目录。旧中文外层目录已经移除，文件正文中的历史
主题名称保留；当前文件级 canonical 路径以 `document-manifest.tsv` 为准。

### 4.1 System 文档内部规则

System 文档按“领域对象 + 产物类型”组合命名，不按“第几次聊天”命名：

```text
system-decomposition/
  2026-09-18-system-decomposition-npc-damage-tracking-boundary-design.md
  2026-09-18-system-decomposition-npc-damage-tracking-api-map.md
  2026-09-18-system-decomposition-npc-damage-tracking-behavior-evidence.md
  2026-09-18-system-decomposition-npc-damage-tracking-retrospective.md
```

权威和非权威分区任务属于 System 领域的可领取入口，与实验计划分开管理：

```text
system-decomposition/
  2026-09-18-system-decomposition-authoritative-20-partition-tasks.md # 只读便利副本
  2026-09-18-system-decomposition-non-authoritative-20-partition-tasks.md # 只读便利副本
  authoritative/2026-09-18-system-decomposition-authoritative-20-partition-tasks.md
  non-authoritative/2026-09-18-system-decomposition-non-authoritative-20-partition-tasks.md
```

根目录两份任务文件只用于快速查阅，canonical 任务表仍是 `authoritative/` 和
`non-authoritative/` 下的文件；runner 初始化、领取和恢复必须使用 canonical 路径。

`plans/system-decomposition/` 只保留 System 实验设计和实施计划；领取后的报告统一写入
`system-decomposition/reports/`，任务状态仍以对应 runner 的共享 ledger 为准。

API 拆分、System 拆分和行为验证属于同一迁移单元，但不能合并成一篇无法独立更新的
“全能报告”。建议每个迁移单元至少有：

- 一个概念边界和状态所有权说明；
- 一个旧入口到概念行为再到新 API 的映射；
- 一个调用/依赖 DAG 和生命周期边界；
- 一个行为验收记录，明确通过、未覆盖和阻塞项；
- 需要返工时的一份复盘或 CR。

### 4.2 Component 文档内部规则

Component 文档保持“组件状态”和“System 行为”分离。P01-P20 的设计和执行文档可以
按分区成对链接，但不能因为执行文档已经存在就把未实现组件标为迁移完成。字段/成员
ledger 需要保留来源声明、读者、写者、间接写入和未覆盖项。

## 5. 新文档的最小元数据

已有历史文件不做批量加头部。本方案生效后新增的文档至少在开头写明：

```text
文档 ID：DOC-YYYY-MM-DD-...
逻辑域：system-decomposition | component-decomposition | migration | ...
产物类型：design | plan | execution | evidence | audit | decision | retrospective
状态：draft | active | accepted | deferred | archived
范围：涉及的 System、Component、分区或迁移批次
证据入口：源码、调用搜索、行为验证或关联文档
canonical 路径：本文档唯一正式路径
```

其中 `accepted` 只表示该文档或该证据切片通过了其声明范围的验收，不表示整个旧系统
已经迁移完成。迁移项目的完成条件仍然是目标行为验证通过。

## 6. 文档创建与迁移流程

### N1：库存

记录路径、扩展名、所属目录、工作树状态、文件类型和主要主题。当前快照见
[document-inventory.md](document-inventory.md)。

### N2：范围

明确这次整理是否只做索引、是否允许物理移动、是否包含历史证据、是否包含链接修复。
本轮范围包含目录移动、必要的职责子目录重命名、路径文字同步和可确定相对链接修复。

### N3：信息架构

为每个文件指定一个逻辑域和一个 canonical 归属，区分设计、计划、实施、证据和审计。
不能以“内容看起来相似”替代来源和调用证据。

### N4：批次迁移

本批次已在 CR 记录后执行。一个批次只处理明确的逻辑域，保留源路径到目标路径的映射；
发生路径冲突时停止，不覆盖现有文件。

### N6：文档和链接门禁

检查：

- 每个当前文件都有唯一逻辑归属；
- 入口链接和仓库内相对链接仍指向存在文件；仓库外技能参考必须显式标记为 external dependency；
- 迁移 ledger 的源路径、目标路径和状态一致；
- 没有因为移动历史文档而改写历史证据的原始语义；
- 新的 System 文档均标注行为验收范围和未覆盖项。

当前阶段只需要 Markdown/路径级检查，不触发构建，也不新增测试代码。当前 17 个
`.agents/skills` 参考链接属于仓库外技能依赖，不在物理搬迁范围内。

### N7/N8：发布和复盘

发布时记录实际移动、保留的兼容入口和未处理项；复盘时单独记录分类错误、重复文档、
失效链接和错误 owner 推断，不能直接修改旧审计结论来掩盖过程问题。

## 7. 本次明确不做的事

- 不恢复或删除当前工作区已有的用户变更；
- 不把 `dome/dome1/docs/flowstate/` 复制成第二个根级 Flowstate；
- 不重写中文历史文件名；仅重命名职责目录和明确的 `CONTEXT.md` 架构入口；
- 不把研究、设计或字段映射单独当成行为迁移完成证明；
- 不创建没有实际文档承载的空目录；
- 不运行构建或测试，也不修改源码和测试项目。

后续若继续搬迁，应先更新文件级 manifest 和 path map，再以新的 change record 分批执行。
