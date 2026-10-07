# NLTX 文档入口

本目录保存 NLTX ECS 拆分、迁移、审查和研究材料。本文档是根级导航入口；当前 Markdown
结构已按领域完成物理整理，具体移动记录见 [document-move-map.tsv](document-move-map.tsv)
和 [CR-2026-09-18](cr/CR-2026-09-18-docs-md-structure-reorganization.md)。

## 先读什么

1. [文档信息架构与管理方案](document-architecture.md)：定义分类、命名、路径归属和后续迁移门禁。
2. [文档库存快照](document-inventory.md)：记录本次盘点时工作树中的目录、数量和已知缺口。
3. [逐文件 Markdown manifest](document-manifest.tsv)：记录当前 canonical 路径、逻辑域和产物类型。
4. Flowstate 生命周期入口 `../dome/dome1/docs/flowstate/README.md` 当前不在本 checkout；现行路径分类和 manifest 规则见 [文档信息架构与管理方案](document-architecture.md)。
5. [ECS 文件组织设计约束](../Context/架构设计/ECS文件组织设计约束.md)：涉及 ECS 领域文件或生产/参考源码目录边界时使用的仓库级约束。
6. [ECS 领域层与基础设施六边形架构边界](../Context/架构设计/ECS领域与基础设施架构边界.md)：说明 ECS 权威状态、应用用例、持久化端口和基础设施适配器的职责与依赖方向。
7. [ECS Entity 组织设计约束](../Context/架构设计/ECSEntity组织设计约束.md)：涉及实体身份、组件关联、创建/销毁、查询和跨实体关系时使用；参考机制见 [SS14 Entity 源码研究](research/2026-10-06-ss14-entity-organization.md)。
8. [Entity 组织迁移代码设计](architecture/2026-10-06-entity-organization-design.md) 与 [执行文档](architecture/execution/2026-10-06-entity-organization-execution.md)：规定统一身份、组件访问、纵向切换及验收门禁。B0–B9 的主验收 receipt 状态为 `ACCEPTED_WITH_NAMED_LIMITS`；receipt 在 15:37 checker 通过后记录了当时文档快照，当前 B9 范围澄清编辑仍待主验收重跑 checker。这不表示全 Terraria 或权威 NetworkServer gameplay parity。B9 旧身份字面审计以生产源码与 `Test` 为范围，并按仓库约束排除只读 `src/NSSLC.Infrastructure/分类参考/`；该目录中的同名参考类型不是生产声明，主 receipt 摘要没有复述此排除项。Simulation DLL `0B052D…` 的 PDB/source map 证明 4 个 assembly 的 PDB/CodeView GUID 与 17 项编译 source Documents 在 artifact-time 对应；主验收 receipt 记录当前树另有 9 项 source hash drift，来自独立 NPC AI 后续写入，超出本迁移运行验收范围。该 drift 不使绑定此 DLL 的运行证据失效，也不能据此声称当前源码等于编译输入。zero-tick 与 NetId 4 spatial/两次切换窄 run 通过；B8 的 38 场景 matrix 与真实 WorldFile late-finalize rollback/retry 证据分别记录在 `621C…`、`5BA221…` 构建/运行证据中。具名限制包括 Mother Slime 237/11 差异且完整因果/parity 未证、NPC 有限内容/AI style、Projectile type 1、NetworkServer packet-27/29 disabled、coin definitions/GetItem Fill stubs，以及 Leashed 无 production caller。细分状态与证据边界见 [执行 ledger](migration/ledgers/2026-10-06-entity-organization-execution-ledger.md)。

## 当前物理结构

目录按领域组织；历史轮次、分区和设计/报告角色只在领域内部继续保留。没有文档承载的
旧目录已移除。

```text
docs/
├── architecture/
│   ├── decisions/                 # ADR
│   └── entity-identity-context.md # 领域术语和身份约束
├── system-decomposition/
│   ├── 2026-09-02-system-split-markdown-writing-guide.md # System 拆分 Markdown 编写指南
│   ├── 2026-09-18-system-decomposition-authoritative-20-partition-tasks.md # 权威任务只读便利副本
│   ├── 2026-09-18-system-decomposition-non-authoritative-20-partition-tasks.md # 非权威任务只读便利副本
│   ├── authoritative/              # 权威 P01-P20 任务与 System 拆分注入提示词
│   ├── non-authoritative/          # 非权威 P01-P20 可领取任务
│   └── reports/                   # System 拆分实验、优化和复盘证据
├── component-decomposition/
│   ├── baseline/                  # 初代组件边界、字段和基线材料
│   ├── review-round-1/            # 第一轮设计、报告和分区提示词
│   └── review-round-2/            # 第二轮 P01-P20 设计、执行和审计
├── migration/
│   ├── assessments/               # 项目级迁移能力评估
│   └── ledgers/                   # 成员/字段 ledger、JSON/TSV 索引和分区表
├── reviews/
│   ├── audits/                    # 项目级和组件级审计
│   └── human/                     # 人工审查与返工约束
├── plans/
│   ├── component-decomposition/   # 组件拆分计划
│   ├── system-decomposition/      # System 实验和实施计划
│   └── tasks/                     # 可执行任务和 PRD
├── research/                      # 外部资料、源码调研和边界研究
├── cr/                            # Flowstate 变更记录
├── document-manifest.tsv          # 当前 Markdown 逐文件索引
└── document-move-map.tsv          # 目录移动和回滚映射
```

## 按问题找文档

| 要找的内容 | 当前 canonical 区域 | 入口或说明 |
| --- | --- | --- |
| System 边界、API、调度和迁移实验 | `system-decomposition/`、`plans/system-decomposition/`、`research/` | 先查边界/API，再查执行和行为证据 |
| System 拆分 Markdown 编写和预处理 | [子系统 Markdown 编写指南](system-decomposition/2026-09-02-system-split-markdown-writing-guide.md) | 用于组织类拆分索引、子系统卡和执行文档 |
| AI 可领取的 20 分区 System 任务 | [权威任务副本](system-decomposition/2026-09-18-system-decomposition-authoritative-20-partition-tasks.md)、[非权威任务副本](system-decomposition/2026-09-18-system-decomposition-non-authoritative-20-partition-tasks.md)；canonical 版本位于 `authoritative/` 和 `non-authoritative/` | 根目录副本只读；领取仍使用 canonical 任务表，输出写入 `system-decomposition/reports/` |
| 权威分区 System 拆分注入提示词 | [权威注入提示词](system-decomposition/authoritative/2026-09-30-authoritative-system-decomposition-injection-prompt.md) | 组合 `ecs-system`、System 会话 profile、权威证据 profile 和 runner 流程 |
| Component 边界和字段归属 | `component-decomposition/` | 设计与执行成对查阅，审计报告作为结论而不是设计替代物 |
| 20 分区成员和迁移映射 | `component-decomposition/review-round-2/`、`migration/ledgers/` | 先查分区执行文档，再查成员/字段 ledger 和源码声明 |
| 外部资料、源码证据和边界调查 | `research/` | 研究材料不直接等同于 owner 或迁移完成证明 |
| 计划、任务和 PRD | `plans/` | 计划不等于实施证据；完成状态必须有执行或验收材料支撑 |
| 架构决策和变更控制 | `architecture/decisions/`、`cr/` | 路径搬迁、范围变更和例外都应有可追溯记录 |
| Entity 运行时组织迁移 | [代码设计](architecture/2026-10-06-entity-organization-design.md)、[执行文档](architecture/execution/2026-10-06-entity-organization-execution.md) | 按源码行为映射、单写切换和真实宿主验收逐批推进 |
| 自定义 ECS 转向 Arch | [执行计划](plans/2026-10-07-custom-ecs-to-arch-execution-plan.md)、[官方 API 研究](research/2026-10-07-arch-api-migration-research.md) | 允许破坏旧 ECS API；采用 Arch 2.1.0 原生 World/Entity/Query，分批验收并退出旧框架；当前仅完成计划 |
| 人工复核与返工要求 | `reviews/human/`、`reviews/audits/` | 人工意见不覆盖源码和调用证据，需要在结论中标注证据范围 |

## 管理规则

- 一份文档默认只有一个 canonical 归属；本次登记的根目录任务文件是只读便利副本
  (`canonical=false`)，不得作为 runner 状态或领取状态的来源。
- 文件级归属和当前路径以 [document-manifest.tsv](document-manifest.tsv) 为准；目录说明不替代逐文件记录。
- 新文档先按“逻辑领域”归类，再按 `design`、`plan`、`execution`、`evidence`、`audit`、
  `retrospective` 区分产物类型，不为每个类型预建空目录。
- 历史证据正文保持稳定；如需移动、删除、合并或批量改名，必须先有独立变更记录，
  同时维护源路径、目标路径、链接影响和回滚方式。本次移动由 CR-2026-09-18 记录。
- 新文档的文件名使用 `YYYY-MM-DD-domain-subject-artifact.md`；已有中文名称和历史命名不
  在本次索引任务中批量重命名。
- JSON、TSV 等结构化文件与其所属 ledger 放在同一迁移证据区域，不放入无语义的 `data/`
  或 `misc/` 目录。
- 研究、设计、执行、审计和行为验收必须区分。仅有设计文档不能宣称迁移完成；行为验证
  通过才是迁移项目的完成依据。

## 本次执行结果

本次已完成 N3 信息架构和 N4 Markdown 物理整理：目录已按领域重设，历史轮次和分区
上下文保留，路径文字与可确定的相对链接已同步，逐文件 manifest 已生成。未修改源码、
测试项目或运行时行为；当前工作树中与本任务无关的既有 Git 删除、暂存和未跟踪变更仍然保留。
