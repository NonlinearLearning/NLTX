# architecture 移除非设计信息

文档 ID：CR-2026-11-22-ARCH-DESIGN-ONLY
逻辑域：change-control
产物类型：decision、execution
状态：completed
日期：2026-11-22
canonical 路径：`docs/cr/CR-2026-11-22-arch-design-only.md`
上游依据：[docs 删除与整理审计](../reviews/audits/2026-11-22-docs-deletion-and-organization-audit.md) 第 4 节 P1-1

## 1. 授权与范围

用户要求"去掉 architecture 中非设计信息"，并在方案确认中选定"移出而非删除"。

`architecture/` 的信息域定义是：已确认的架构、约束和决策——ADR、边界原则、命名和组织
规则（[CR-2026-09-18](CR-2026-09-18-docs-md-structure-reorganization.md) 第 3 节、
[项目文件组织设计约束](../../Context/架构设计/项目文件组织设计约束.md)）。
实现记录、盘点矩阵和 System 分区产物不属于这个域。

本次移动 7 个文件、删除 0 个文件，全部限于 `docs/` 内部。源码、测试、项目配置未改动；
未运行构建或测试。

## 2. 移动清单

### 2.1 实现记录与盘点 → `reviews/audits/`

这些是 2026-10-23 至 10-26 期间落地后的实现说明、盘点矩阵和验证记录，性质是"做了什么、
验证了什么"，不是"决定什么"：

| 原路径 | 新路径 |
| --- | --- |
| `architecture/2026-10-24-nsslc-atomic-components-and-client-system-matrix.md` | `reviews/audits/2026-10-24-nsslc-atomic-components-and-client-system-matrix.md` |
| `architecture/2026-10-25-projectile-implementation.md` | `reviews/audits/2026-10-25-projectile-implementation.md` |
| `architecture/2026-10-25-shared-buff-and-npc-ai-implementation.md` | `reviews/audits/2026-10-25-shared-buff-and-npc-ai-implementation.md` |
| `architecture/2026-10-26-physics-combat-projectile-boundary-implementation.md` | `reviews/audits/2026-10-26-physics-combat-projectile-boundary-implementation.md` |
| `architecture/2026-10-26-shared-component-consolidation.md` | `reviews/audits/2026-10-26-shared-component-consolidation.md` |

### 2.2 P04 System 分区产物 → `system-decomposition/`

`architecture/subsystems/` 与 `architecture/execution/` 里的 P04 是权威 System 拆分
分区的子系统设计与执行合同，与 `system-decomposition/reports/`、`authoritative/` 里的
P04 材料同属一条工作线：

| 原路径 | 新路径 |
| --- | --- |
| `architecture/subsystems/authoritative-P04-player-lifecycle-interaction.md` | `system-decomposition/subsystems/authoritative-P04-player-lifecycle-interaction.md` |
| `architecture/execution/authoritative-P04-player-lifecycle-interaction.md` | `system-decomposition/execution/authoritative-P04-player-lifecycle-interaction.md` |

## 3. architecture/ 移动后的状态

保留 9 个文件，全部是设计或决策类：

```text
docs/architecture/
├── decisions/0001-entity-identity-root-and-typed-projections.md   # ADR
├── entity-identity-context.md                                     # 术语与身份约束
├── 2026-10-02-world-load-standard-api-design.md
├── 2026-10-02-world-storage-load-coordinator-design.md
├── 2026-10-06-entity-organization-design.md
├── 2026-10-23-attribute-system-design.md
├── 2026-10-23-gravity-attribute-system-design.md
├── 2026-10-23-nsslc-arch-ecs-organizer.md
└── 2026-10-24-projectile-ecs-system-design.md
```

- `subsystems/` 与 `execution/` 两个子目录移空后删除。
- `execution/` 里剩余 4 份（P01、P11、world-load-standard-api、entity-organization、
  arch-t3）是**架构级**迁移的执行文档，与 entity-organization-design、
  world-load-standard-api-design 成对，不是 System 分区产物，本次保留。
- `2026-10-23-nsslc-arch-ecs-organizer.md` 状态为"实施中"，是分层职责设计，保留。

## 4. 链接处理

移动后逐文件核对了从新位置可达的相对链接，修正了因目录深度变化而失真的路径：

- 5 份移入 `reviews/audits/` 的文件：`../../..` 前缀的仓库根引用改为 `../../../..`
  深度不变（audits 与原 architecture 同为 docs 下两层，实际深度未变，无需改动）。
- 2 份 P04 文件移入 `system-decomposition/` 后，原指向
  `../../system-decomposition/reports/...` 的链接降一级为 `../reports/...`。
- 反向引用方（`reviews/audits/2026-11-02-share-component-consolidation-audit.md`、
  `plans/2026-10-26-hostile-damage-combat-system-plan.md`、
  `src/NSSLC/Components/Projectile/README.md`、
  `system-decomposition/authoritative/...task-table` 等）逐处更新为新路径。

## 5. 恢复

按上表反向移动即可；原文件内容未做任何改写（除第 4 节列出的相对链接行），文件级的
精确恢复基准是执行前的 HEAD（`6518acc`）：

```powershell
git show 6518acc:docs/architecture/2026-10-24-nsslc-atomic-components-and-client-system-matrix.md
```

## 6. 验证

- 移动后 `docs/architecture/` 递归文件数 = 9，全部为设计/决策类。
- 全仓 markdown 对这 7 个文件名的引用无一悬空（链接已随移动更新）。
- `git status` 显示 7 个 rename + 相应链接行的修改，无其他文件被触碰。
