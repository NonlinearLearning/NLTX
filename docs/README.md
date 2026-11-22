# docs/

本目录保存 NLTX 项目的过程文档：架构设计、System/Component 拆分材料、迁移 ledger、
外部研究、审查审计、计划任务和变更记录。

本 README 只描述目录结构和用途，不是逐文件索引。全仓库 `docs/` 不维护全局索引文件，
文档按领域目录直接定位，依据见 [CR-2026-11-22-remove-docs-index](cr/CR-2026-11-22-remove-docs-index.md)。

## 目录结构

```text
docs/
├── architecture/                  # 架构设计与已确认约束
│   ├── decisions/                 #   ADR（实体身份根与 typed projection）
│   ├── execution/                 #   架构级执行文档（实体组织、世界加载 API）
│   ├── subsystems/                #   子系统设计（P04）
│   └── *.md                       #   属性系统、投射物、物理战斗边界等设计/实现
├── system-decomposition/          # System 拆分：设计、执行、报告、任务
│   ├── authoritative/             #   权威分区（P10/P11/P14）设计+执行
│   ├── non-authoritative/         #   非权威分区任务
│   ├── design/                    #   P01/P03/P08/P09/P17-P19 设计
│   ├── execution/                 #   对应执行文档
│   ├── reports/                   #   P01-P20 分区报告、复核与 crosswalk
│   └── *.md                       #   编写指南、NPC AI 重设计与验证、验收门禁
├── component-decomposition/       # Component 拆分材料
│   ├── review-round-2/            #   第二轮 P01-P20 组件设计+执行（45 份）
│   └── archive/                   #   初代 baseline 与第一轮审查（160 份，已归档）
├── migration/                     # 旧实现 → 新实现的过程证据
│   ├── ledgers/                   #   成员迁移账本（JSON 事实源 + 生成的速查视图）、
│   │                              #   实体组织/NPC AI 执行 ledger、合并后的检查点账本
│   └── assessments/               #   项目级迁移能力评估
├── reviews/                       # 复核与审计
│   ├── audits/                    #   项目级与组件级审计（分级、物理、接线、世界组件化）
│   ├── human/                     #   人工审查与返工约束
│   └── *.md                       #   未归类的历史审查记录
├── plans/                         # 计划与任务
│   ├── minimal-implementation/    #   最小实现主计划与 P01-P07 模块计划
│   ├── network-message-registration/ # 网络消息注册 10 类 + 验收
│   ├── system-decomposition/      #   System 拆分实验与执行计划
│   ├── component-decomposition/   #   组件批次计划
│   └── tasks/                     #   PRD 与可执行任务
├── research/                      # 外部资料与源码调研（ECS 选型、SS14、Arch API）
└── cr/                            # 变更记录（授权、范围、回滚方式）
    └── component-decomposition/   #   组件拆分历史 CR
```

## 怎么找

| 要找什么 | 去哪里 |
| --- | --- |
| 已确认的架构约束、身份模型 | `architecture/`，从 [entity-identity-context.md](architecture/entity-identity-context.md) 和 [decisions/0001](architecture/decisions/0001-entity-identity-root-and-typed-projections.md) 开始 |
| 某个 System 的边界、API、行为证据 | `system-decomposition/`：报告在 `reports/`，设计在 `design/` 或 `authoritative/`，执行在 `execution/` 或 `authoritative/` |
| 组件如何拆分、字段归属 | `component-decomposition/review-round-2/`；第一轮与初代基线在 `archive/` |
| 旧成员的迁移映射 | `migration/ledgers/README.md`，唯一事实源是 `Version4-member-migration-map.json` |
| 审计结论与证据范围 | `reviews/audits/` |
| 下一批做什么、完成条件 | `plans/` |
| 某次变更为什么这么做 | `cr/`，文件名 `CR-YYYY-MM-DD-主题.md` |

## 约定

- 文件名默认 `YYYY-MM-DD-主题-产物类型.md`；早期中文命名和 `CR-` 前缀文件保留原名。
- 一份文档只有一个 canonical 路径；历史证据正文不改写，移动/删除/合并需要 `cr/` 里有
  对应记录。最近的整理记录：
  [CR-2026-11-22-docs-cleanup-and-merge](cr/CR-2026-11-22-docs-cleanup-and-merge.md)、
  [CR-2026-11-22-remove-docs-index](cr/CR-2026-11-22-remove-docs-index.md)、
  [CR-2026-11-22-nsslc-domain-directory-flatten](cr/CR-2026-11-22-nsslc-domain-directory-flatten.md)。
- `component-decomposition/archive/` 是归档区，内容不再更新，结论以 `review-round-2/` 为准。
- 状态与证据范围以各文档正文头部为准；目录位置不代表完成状态，"设计"不等于"已实现"。
