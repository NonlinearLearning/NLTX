# Projectile 设计文档整合、缺陷记录分层与断链修复

文档 ID：CR-2026-11-24-PROJECTILE-DOCS-CONSOLIDATION  
逻辑域：projectile / file-organization  
产物类型：execution  
状态：completed  
日期：2026-11-24  
canonical 路径：`docs/cr/CR-2026-11-24-projectile-docs-consolidation.md`  
上游依据：[项目文件组织设计约束](../../Context/架构约束/项目文件组织设计约束.md) §3.3、§6；
[开发协作与变更约束](../../Context/项目约束/开发协作与变更约束.md) §变更边界、§导航维护；
[CR-2026-11-24 断裂文件夹路径修复](CR-2026-11-24-broken-folder-path-repair.md) §1 判定口径

## 1. 授权与范围

用户要求整合 Projectile 设计文档与缺陷记录。本 CR 覆盖三件事：

1. 删除两份已被实现超越的设计文档，其结论由 README、CR、缺陷记录承接；
2. 建立 `docs/defects/` 记录"已知但未闭合的缺陷"，与 CR（执行记录）、审计（证据范围）分层；
3. 按 [CR-2026-11-24 断裂文件夹路径修复](CR-2026-11-24-broken-folder-path-repair.md) 的判定口径
   清掉本轮删除产生的悬空链接。

范围限定在文档层：`docs/**`、`src/NSSLC/Projectile/README.md`、`Context/progress.md`。
本轮不动 `src/**` 生产代码、`Test/**` 验证项目与 `Context/架构约束/**` 正文。

## 2. 文档删除与承接

| 删除的文档 | 规模 | 删除理由 | 承接位置 |
| --- | ---: | --- | --- |
| `docs/architecture/2026-10-24-projectile-ecs-system-design.md` | 549 行 | 状态停在 `implemented-foundation` / 运行时 partial；正文按"定义系统 + 解析器 + 阶段 4.3"描述，与实际落地（内容族走 LVCS、目标获取独立成系统、子步归共享物理层）不一致 | `src/NSSLC/Projectile/README.md`（组合模型、按阶段系统清单、单例约束）、[实现说明](../reviews/audits/2026-10-25-projectile-implementation.md)、[P0–P4 CR](CR-2026-11-24-projectile-p0-p1-content-and-shoot-seam.md) |
| `docs/architecture/2026-11-23-projectile-penetration-budget-linkage-design.md` | 233 行 | 状态为 `design-proposal（待决策，未实施）`，方案已由 P4 落地并按落地结果改形（三计数 + 回滚） | `Projectile/README.md` 穿透预算段、P0–P4 CR §3 "为什么预算要三计数 / 为什么拒绝也要回滚"、缺陷记录 §4 |

两文档保留 `git` 历史，需要原文时用 `git show 3de0b67^:docs/architecture/<文件名>` 取回。

## 3. 缺陷记录分层

`docs/defects/` 本轮登记为固定落位。两份记录此前已写入该目录，但全仓零入链——按文件名才能发现，
等于不存在；本轮补上入链并在 [项目文件组织设计约束](../../Context/架构约束/项目文件组织设计约束.md) §3.3 登记该目录职责。

| 文件 | 缺陷 | 来源 |
| --- | --- | --- |
| `2026-11-24-projectile-homing-speed-drift.md` | 追踪转向速率单调漂移，断言被放宽成实测区间 | P0–P4 CR §5.1 第 2 条 |
| `2026-11-24-projectile-termination-uniqueness-implicit.md` | 双路径同帧耗尽的终止唯一性靠隐式守卫维持，无测试固定 | P0–P4 CR §5.1 第 3 条 |
| `2026-11-24-projectile-entity-contact-substep-always-zero.md` | 实体接触事实子步号恒 0，与瓦片接触语义不一致 | P0–P4 CR §5.1 第 1 条 |
| `2026-11-24-projectile-homing-profile-values-not-applied.md` | 行为工厂按类型注册，profile 数值对多 profile 内容不生效 | P0–P4 CR §5.1 第 4 条 |

与既有目录的分工：CR 记"这轮做了什么、怎么验的"，审计记"证据范围与覆盖判定"，计划记"批次状态"，
缺陷记录记"哪里不对、判据是什么、怎样算关闭"。缺陷关闭后整份文件删除，不移入归档。

该目录在本轮期间以增量方式被并行写入（新增与改名都有发生），上表只是开工时的快照；
判断"某缺陷是否已记录"以目录现状为准，不以本表为准。

## 4. 断链修复清单

按上游 CR §1 的判定口径逐条处置：

| 文件 | 原引用 | 处理 |
| --- | --- | --- |
| `src/NSSLC/Projectile/README.md` | 指向已删的 2026-10-24 设计 | 改为指向实现说明与本轮 CR，并说明领域模型以 README 为准 |
| `docs/plans/2026-11-24-projectile-component-systems-execution-plan.md` §上游依据 | 两条已删设计 | 保留 LVCS 设计与实现说明两条，已删两条降级为行内代码并注明现行口径 |
| `docs/plans/2026-10-26-hostile-damage-combat-system-plan.md` §12 | 已删的 2026-10-24 设计 | 删除该行（其余四条仍解析成功） |
| `docs/architecture/2026-11-24-ecs-coordination-rules-design.md` §相关设计 | 已删的穿透预算设计 | 保留条目并加注删除日期与继任位置 |
| `docs/reviews/audits/2026-10-24-nsslc-atomic-components-and-client-system-matrix.md` §头部 | 已删的 2026-10-24 设计 | 降级为行内代码并注明继任者 |
| `src/NSSLC.Infrastructure/Network/Design/README.md` §阅读顺序 5–7 | 指向 `D:/ProjectItem/SourceCode/Net/NetWork/docs/**` 的三个仓库外路径 | 按上游 CR「目标在本仓库从未存在」一档降级为行内代码并注明属外部资料 |
| `docs/README.md` | 23:33 被重新创建并新增 `defects/` 段 | 用户裁决删除（与 [CR-2026-11-22 移除 docs 索引层](CR-2026-11-22-remove-docs-index.md) 及 §3.3 冲突）；其中有价值的缺陷目录说明已并入 §3.3 |

未改动对象（按上游 CR 口径属历史证据，不改写）：

- `docs/cr/CR-2026-11-22-arch-design-only.md` §3：`text` 代码块内的目录快照，不是路径引用。
- `docs/cr/CR-2026-11-24-constraints-drift-sync.md`：记录当时"失效 1 条"的审计事实，改写会伪造史实。
- 其余 2026-09～10 的历史审计与研究：记录当时代码路径，本轮不涉及。

## 5. 验证

| 检查 | 结果 |
| --- | --- |
| 本轮涉及 7 份文档的相对链接逐条解析 | 0 悬空（脚本按文件系统解析，非字符串匹配） |
| `docs/defects/` 下两份缺陷记录被引用 | 2/2（P0–P4 CR §5.1 第 2、3 条 + §5 说明 + 本 CR §3） |
| `Context/progress.md` 行数与体积 | 与上限 70 行 / 6 KB 相比留有余量 |
| 生产代码、验证项目、约束正文 | 未改动 |

未做全仓 959 份 Markdown 的断链清零：历史文档（`docs/component-decomposition/**`、2026-09～10
的审计与研究）中的旧路径属当时代码布局，按上游 CR 口径不改写；其中悬空项与本轮无关。

## 6. 回滚

```powershell
cd D:\TRbackup\NLTX
Remove-Item -Recurse docs/defects
git checkout -- src/NSSLC/Projectile/README.md docs/cr/CR-2026-11-24-projectile-p0-p1-content-and-shoot-seam.md `
  docs/plans/2026-11-24-projectile-component-systems-execution-plan.md `
  docs/plans/2026-10-26-hostile-damage-combat-system-plan.md `
  docs/architecture/2026-11-24-ecs-coordination-rules-design.md `
  docs/reviews/audits/2026-10-24-nsslc-atomic-components-and-client-system-matrix.md
git checkout HEAD -- docs/architecture/2026-10-24-projectile-ecs-system-design.md `
  docs/architecture/2026-11-23-projectile-penetration-budget-linkage-design.md
```

删除本 CR 即回到"设计文档保留、缺陷只在 CR 表格里"的状态；缺陷记录删除不影响代码。
