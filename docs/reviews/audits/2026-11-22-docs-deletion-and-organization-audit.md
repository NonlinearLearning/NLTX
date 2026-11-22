# docs 删除与整理审计

文档 ID：DOC-2026-11-22-docs-deletion-and-organization-audit
逻辑域：reviews
产物类型：audit
状态：executed（A + C，2026-11-22）；执行记录见 [CR-2026-11-22](../../cr/CR-2026-11-22-docs-cleanup-and-merge.md)
范围：`docs/` 全目录，604 个文件
证据入口：目录枚举、MD5 比对、相对链接可达性检查、`document-manifest.tsv` 覆盖比对
canonical 路径：`docs/reviews/audits/2026-11-22-docs-deletion-and-organization-audit.md`

## 0. 结论

`docs/` 共 **604 个文件、40.3 MB**，其中 Markdown 592 个。逐项核对后：

- **能无损失直接删除的只有约 0.2 MB（约 0.5%）**：一个审计临时文件、两份根目录任务副本的冗余、一条失效索引快照。
- **其余 99.5% 是历史证据正文**，按当时的信息架构方案第 2 节和 CR-2026-09-18 的约束，删除它们等于删证据，必须先有独立 CR，不能作为"整理"顺手做。
- 真正失修的不是文件本身，而是**索引层**：`document-manifest.tsv` 只覆盖 429/592 个 Markdown，`document-inventory.md` 停留在 390 个文件，`README.md` 的目录树已与磁盘不符。

因此建议的处理顺序是：**先修索引 → 再移错位文件 → 最后才谈删除**。

> **执行结果（2026-11-22）**：本报告的 P0 索引修复、第 3 节的删除项、以及
> `component-decomposition/` 的归档已完成，记录见
> [CR-2026-11-22](../../cr/CR-2026-11-22-docs-cleanup-and-merge.md)。执行后的实际数字：
> 583 个文件 / 571 个 Markdown / 40.2 MB；manifest 571 行与磁盘一一对应；文档到文档失效
> 链接 12 处降至 10 处（均为 checkout 外部依赖）。下文保留审计当时的原始数字，不再回改，
> 作为判定依据的存档。
>
> **索引层移除（同日稍后）**：用户随后要求"去掉整个文档的索引"，`docs/README.md`、
> `document-architecture.md`、`document-inventory.md`、`document-manifest.tsv`、
> `document-move-map.tsv` 已全部删除，授权与恢复方式见
> [CR-2026-11-22-remove-docs-index](../../cr/CR-2026-11-22-remove-docs-index.md)。
> 本文提到的上述文件名自此均不再对应存在的文件。

## 1. 当前盘点

| 目录 | 文件数 | 大小 | 主要内容 |
| --- | ---: | ---: | --- |
| `component-decomposition/` | 237 | 10.8 MB | baseline 40、review-round-1 120、review-round-2 77 |
| `plans/` | 129 | 2.0 MB | 根 26、component-decomposition 44、system-decomposition 34、minimal-implementation 11、network-message-registration 12、tasks 2 |
| `migration/` | 84 | 21.2 MB | ledgers 83（其中 39 个 md / 13.3 MB）+ assessments 1 |
| `system-decomposition/` | 57 | 1.9 MB | 根 11、authoritative 8、design 7、execution 6、reports 24、non-authoritative 1 |
| `reviews/` | 29 | 3.2 MB | audits 24、human 2、根 3 |
| `cr/` | 24 | 0.2 MB | 变更记录 |
| `architecture/` | 22 | 0.6 MB | 根 14、execution 6、subsystems 1、decisions 1 |
| `research/` | 17 | 0.4 MB | 外部资料与源码调研 |
| 根级管理文件 | 5 | 0.12 MB | README、架构方案、库存快照、manifest、move-map |

时间分布：2026-09 命名 325 个、2026-10 命名 93 个、2026-11 命名 21 个，其余 153 个无日期前缀（集中在 migration 51、component-decomposition 42、plans 24、cr 24）。

Git 状态：HEAD 跟踪 596 个文件，工作树 604 个；7 个 2026-11 计划/评审为未跟踪新文件，`document-manifest.tsv` 与一份 CR 已修改未提交。

## 2. 判定口径

本审计只把满足以下之一的对象列为"可删除"：

1. 本次审计产生的临时文件；
2. 内容完全一致、且删除后不损失信息的副本；
3. 可从唯一事实源重新生成的派生视图；
4. 指向已不存在文件的索引条目（删记录，不删文件）。

不满足以上条件的，一律只列为"整理/归档"，不列为删除。

## 3. 可以删除

| # | 对象 | 大小 | 依据 | 风险 |
| --- | --- | ---: | --- | --- |
| 1 | `docs/.tmp-doc-index.csv` | 111 KB | 本次审计生成的临时索引 | 无，已删除 |
| 2 | `system-decomposition/2026-09-18-system-decomposition-non-authoritative-20-partition-tasks.md` | 17.6 KB | 与 `non-authoritative/` 下 canonical 版本 **MD5 完全相同**；因目录深度不同，根副本的 5 个相对链接全部失效（指向 `../../../AGENTS.md` 等） | 低。README 声明它是"只读便利副本"，删除前需在 CR 中改口径，或改为指向 canonical 的一行链接 |
| 3 | `system-decomposition/2026-09-18-system-decomposition-authoritative-20-partition-tasks.md` | 17.7 KB | 与 canonical 版本仅差相对链接深度，属人工维护的第二份拷贝（本次已发现两处链接层级被手工重写） | 低。同上，建议保留其一 |
| 4 | `document-manifest.tsv` 中 3 条幽灵记录 | — | 文件在 HEAD 和工作树都不存在：`system-decomposition/reports/2026-09-17-ecs-system-domain-splitting-p06-process-retrospective.md`、`...-p06-system-experiment-optimization-report.md`、`2026-09-17-npc-damage-tracking-system-experiment-optimization-report.md` | 无，仅删索引行 |
| 5 | `docs/document-inventory.md` | 5.7 KB | 自述盘点 390 个文件，实际 604；其"当前顶层库存"表已全面失真 | 中。它是 2026-09-19 的快照，建议改写为"历史快照"而非直接删 |

合计可删约 **0.2 MB**。除此之外没有发现可以无损删除的文件：全目录只有 1 组字节级重复（上表第 2 项），没有空文件，最小文件 654 B（ADR，有独立内容）。

不建议删除、但常被误判为"可删"的三类：

- `component-decomposition/baseline/`（40 个，1.5 MB）：被 round-1/round-2 覆盖结论，但它是第一轮结论的输入证据，删了就无法复现分级依据。
- `plans/2026-10-08-arch-*`（11 个）：Arch 迁移线已被 2026-10-09 的 components-only 要求终止，但这些计划是 CR-2026-10-09 的授权来源。
- `reviews/audits/2026-10-08-simple-game-component-grading.{json,csv,md}`（2.8 MB 合计）：同一数据集的三种形态，但 JSON 是逐组件明细的唯一载体。

## 4. 需要整理

### P0 索引与导航（不改内容，只改索引）

1. **`document-manifest.tsv` 严重滞后**：432 行，覆盖 429 个现存 Markdown（592 个中的 72.5%），另有 3 条幽灵记录。未登记 163 个，按目录分布：

   | 目录 | 未登记 |
   | --- | ---: |
   | `plans/system-decomposition/` | 24 |
   | `system-decomposition/reports/` | 24 |
   | `migration/ledgers/` | 23 |
   | `plans/`（根） | 15 |
   | `cr/` | 13 |
   | `plans/network-message-registration/` | 12 |
   | `plans/minimal-implementation/` | 11 |
   | `architecture/`（根） | 10 |
   | 其余 | 31 |

   注意：manifest 里 410 条状态是 `active`，其中很大一部分其实是 2026-09 的已完成批次，状态字段没有随执行更新。

2. **`README.md` 的"当前物理结构"与磁盘不符**：
   - 列出了不存在的 `review-round-1/{prompts}`、`review-round-2/{design,reports}`；
   - 漏掉了实际存在的 `architecture/execution/`、`architecture/subsystems/`、`plans/minimal-implementation/`、`plans/network-message-registration/`、`reviews/` 根级 3 个文件；
   - 第 18 行仍把 `../dome/dome1/docs/flowstate/README.md` 写成生命周期入口，该路径不存在。

3. **导航覆盖率低**：592 个 Markdown 中 **405 个没有任何入站 Markdown 链接**。除 manifest 外，文档之间基本靠文件名猜。这解释了为什么"哪些能删"难以判断。

### P1 目录归属错误（移动，不删除）

1. **`architecture/` 混入 system-decomposition 产物**，共 4 个：
   - `architecture/execution/2026-09-30-authoritative-P01-liquid-wiring-spatial-death-teleport-system-execution.md`
   - `architecture/execution/2026-09-30-authoritative-P11-player-presentation-derived-execution.md`
   - `architecture/execution/authoritative-P04-player-lifecycle-interaction.md`
   - `architecture/subsystems/authoritative-P04-player-lifecycle-interaction.md`

   `architecture` 的定义是 ADR、边界原则和命名规则；这 4 个是 P01/P04/P11 的执行合同与子系统设计，应并入 `system-decomposition/`。

2. **`reviews/` 根级 3 个文件未归 `audits/`**：`2026-10-22-minimal-server-fix-record.md`、`2026-10-26-projectile-attributes-boundary-review.md`、`audit-report-NLTX-2026-10-04.md`。

3. **`plans/` 根目录混放两种生命周期**：26 个文件里，11 个是 2026-10-08 的 Arch 迁移线（已被 CR-2026-10-09 终止，状态仍写 `active`），15 个是 2026-10-26 至 11-20 的现行计划。建议按 `plans/archive/` 与 `plans/active/` 分层，或至少在 README 标注状态。

4. **P01–P20 的 design/execution 散落在 5 处**：`system-decomposition/authoritative/`（P10、P11、P14）、`system-decomposition/design/`（P01、P03、P08、P09、P17、P18、P19）、`system-decomposition/execution/`（P03、P08、P09、P17、P18、P19）、`plans/system-decomposition/`（P02、P06、P07、P12、P13、P16、P20）、`architecture/execution/`（P01、P04、P11）。同一个分区的设计/执行/报告无法沿一条路径找齐，这是当前最难用的部分。

### P2 大文件与派生视图（合并/补文档）

1. **`migration/ledgers/` 的 README 只登记了 7 个文件，实际有 39 个 md**。未登记的大头是 12 份生成视图，合计约 14 MB：

   | 文件 | 大小 |
   | --- | ---: |
   | `Version4字段属性按系统子系统拆分报告-去除ID类文件.md` | 2.36 MB |
   | `Version4字段属性逐成员源码声明-去除ID类文件.md` | 2.16 MB |
   | `Version4非权威模拟系统字段属性逐成员源码声明-更细子系统拆分-去除ID类文件.md` | 2.00 MB |
   | `Version4字段逐成员源码声明-去除ID类文件.md` | 1.84 MB |
   | `Version4非权威模拟系统字段属性逐成员源码声明-去除ID类文件.md` | 1.53 MB |
   | `Version4全部字段属性明细-去除ID类文件.md` | 1.10 MB |
   | `Version4权威模拟系统字段属性逐成员源码声明-更细子系统拆分-去除ID类文件.md` | 0.93 MB |
   | `Version4权威模拟系统字段属性逐成员源码声明-去除ID类文件.md` | 0.66 MB |
   | `Version4属性逐成员源码声明-去除ID类文件.md` | 0.32 MB |
   | 其余 3 份 | 约 0.4 MB |

   这些是"去除 ID 类文件"的多轮切片，相互覆盖关系没有声明。建议在 README 标明哪一份是当前视图、其余为历史视图，或迁移到 `migration/ledgers/archive/`。

2. **22 个 `2026-10-07-npc-ai-*-checkpoint.md`**（合计 128 KB）是同一轮 NPC AI 会话的逐步检查点，粒度细到 `sitting-tick`、`walk-conversation`。建议合并为 1 份会话 ledger，原始 checkpoint 移入 `migration/ledgers/checkpoints/`。

3. **组件分级的三形态**：`2026-10-08-simple-game-component-grading.json`（2.35 MB，487 组件逐项明细）、`.csv`（411 KB）、`.md`（18 KB 摘要）没有声明主从关系。应明确 JSON 为事实源、其余为派生视图。

4. **`review-round-1/` 存在同名不同物**：顶层 19 个 `2026-09-05-version4-*-public-decomposition.md` 是"审查与拆分设计"结果，`design/` 下 19 个同名文件是"只读审查提示词"。文件名几乎一致，极易误删或误引。

### P3 链接失效

31 个文件共 **309 个相对链接不可达**。绝大多数指向重构中已移动的 `src/` 路径（如 `src/NSSLC/Component/...`），属源码 churn，不是文档问题；但以下集中在文档内部，需要修：

| 文件 | 失效链接数 |
| --- | ---: |
| `reviews/audits/2026-10-09-components-only-inventory.md` | 106 |
| `reviews/2026-10-22-minimal-server-fix-record.md` | 32 |
| `reviews/audits/2026-11-20-attribute-system-architecture-review.md` | 26 |
| `architecture/2026-10-06-entity-organization-design.md` | 22 |
| `research/2026-09-16-ecs-system-domain-splitting-unified-evidence-api-dag-research.md` | 15 |
| `plans/system-decomposition/2026-10-06-npc-ai-system-execution-plan.md` | 11 |

另有两处指向仓库外且已不存在：`../dome/dome1/docs/flowstate/README.md`（README 与 document-architecture 各一处）。

## 5. 建议执行顺序

1. **立 CR**：记录本次删除与移动范围、回滚方式（`document-move-map.tsv` 同步更新）。
2. **重生成 manifest**：补齐 163 个未登记文件，删除 3 条幽灵行，把已完成批次的状态从 `active` 改为实际状态。这一步不移动任何文件，风险最低、收益最大。
3. **修 README 与 inventory**：按磁盘实际结构重写目录树；把 inventory 标为 2026-09-19 历史快照或重算。
4. **移动 P1 的 7 个错位文件**（4 个 architecture + 3 个 reviews 根文件）。
5. **补 `migration/ledgers/README.md` 的文件角色表**，明确 12 份生成视图的主从关系。
6. **合并 22 个 checkpoint**。
7. **删除第 3 节的 1、2、4 项**（约 0.13 MB）。

## 6. 不建议做的事

- 不要按"旧"或"看起来重复"批量删 `component-decomposition/` 与 `system-decomposition/`：它们是分级与拆分的原始证据，删掉后 `document-manifest.tsv` 里的引用和 CR 的授权依据会一起断。
- 不要按日期清理 `plans/`：2026-09 的计划虽然不再执行，但仍是 2026-10 之后每次范围变更的上游依据。
- 不要把 `docs/` 整体移出仓库"瘦身"：40 MB 对一个含 2,232 个受保护基础设施文件的仓库不构成负担，而历史证据一旦离开工作树就失去可检索性。

## 7. 本次未做的事

只做只读检查与一份审计记录。除新建本文件外，未删除、移动、重命名任何文档；未修改源码、测试或项目配置；未运行构建。
