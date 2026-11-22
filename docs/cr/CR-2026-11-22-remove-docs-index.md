# 移除 docs 索引层

文档 ID：CR-2026-11-22-REMOVE-DOCS-INDEX
逻辑域：change-control
产物类型：decision、execution
状态：completed
日期：2026-11-22
canonical 路径：`docs/cr/CR-2026-11-22-remove-docs-index.md`
上游依据：[docs 删除与整理审计](../reviews/audits/2026-11-22-docs-deletion-and-organization-audit.md)

## 1. 授权与范围

用户直接要求"去掉整个文档的索引""去掉索引 md 文件"，并在范围确认中选定删除全部 5 个索引/管理
文件，明确接受与并行会话的冲突由对方承担。

本次只删除下列 5 个文件，不移动、不重写任何内容文档：

| 文件 | 原作用 |
| --- | --- |
| `docs/README.md` | docs 根级导航入口、按问题检索表 |
| `docs/document-architecture.md` | 信息架构方案：逻辑域、产物类型、N1–N8 流程 |
| `docs/document-inventory.md` | 2026-09-19 的库存快照 |
| `docs/document-manifest.tsv` | 432 行逐文件索引（逻辑域/产物类型/状态/canonical） |
| `docs/document-move-map.tsv` | 2026-09-18 目录搬迁的 source→target 映射 |

## 2. 理由

索引层与其维护成本不匹配，且从未被真正维护：

- `document-manifest.tsv` 由 2026-09-22 的单个批量提交建立，诞生时 383 行对应 386 个
  Markdown；此后文档增至 570 个 Markdown，它只靠增量补行涨到 432 行，从未整体重生成。
- 仓库内**不存在**任何可重建该索引的脚本；历史脚本 `Build/Tools/GenerateFlowstateManifest.ps1`
  面向更早的 `dome/dome1/docs/flowstate`，已于 2026-10-03 删除。
- 诞生时就带着 3 条指向不存在文件的记录，直到本次审计仍未修正。
- 359/432 行的 `lastReviewed` 都是同一个批量赋值 `2026-09-18`，索引状态因此不具备判读价值。

保留一个持续失真又无人重算的索引，比没有索引更容易误导：读者会把它当成文件是否存在、
文档是否完成的权威。移除后，文档按目录直接定位，证据强弱由各文档正文自述。

## 3. 未改动的对象

- 所有内容文档（592→570 个 Markdown 中的其余部分）正文保持原样。
- 历史执行文档中提到的 `` `docs/document-manifest.tsv` ``、`docs/README.md` 等字样**保持原样**：
  它们记录的是当时实际执行的命令和当时存在的路径，属于历史事实，不因索引被删而改写。
  这些引用不会指向存在的文件，属于已知且接受的状态。
- 源码、测试、项目配置、构建策略未改动；本次未运行构建或测试。

## 4. 恢复

5 个文件在 `HEAD`（`6518acc`）中均被跟踪，逐项恢复即可：

```powershell
git show 6518acc:docs/README.md                 > docs/README.md
git show 6518acc:docs/document-architecture.md  > docs/document-architecture.md
git show 6518acc:docs/document-inventory.md     > docs/document-inventory.md
git show 6518acc:docs/document-manifest.tsv     > docs/document-manifest.tsv
git show 6518acc:docs/document-move-map.tsv     > docs/document-move-map.tsv
```

恢复的是删除前一刻的已提交版本；`document-manifest.tsv` 等文件在删除时存在未提交改动
（并行会话 20:50–20:56 期间的写入），那些改动不包含在上面的恢复命令里。

## 5. 影响与后续

- `docs/` 不再有根级入口文件。AGENTS.md 的导航不依赖 `docs/README.md`，仓库入口链路不受影响。
- 若后续仍需要索引，应先写生成器再引入索引文件，避免再次出现"手写一次、无人重算"的循环。
- 本次与一个并行会话存在文件级冲突：该会话在 20:52–20:56 期间正在重写 `README.md`、
  `document-architecture.md`、`document-inventory.md`、`document-manifest.tsv`、`document-move-map.tsv`。
  若其后续写入重建了上述文件，以本 CR 的授权为准需要再次删除。
