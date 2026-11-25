# 修复断裂的文件夹路径

文档 ID：CR-2026-11-24-BROKEN-FOLDER-PATH-REPAIR  
逻辑域：file-organization  
产物类型：execution  
状态：completed  
日期：2026-11-24  
canonical 路径：`docs/cr/CR-2026-11-24-broken-folder-path-repair.md`  
上游依据：[项目文件组织设计约束](../../Context/架构约束/项目文件组织设计约束.md) §3.3；
[开发协作与变更约束](../../Context/项目约束/开发协作与变更约束.md) §变更边界、§导航维护

## 1. 授权与范围

用户要求：「修复断裂的文件夹路径」。修复对象是**指向已不存在目录或文件的路径引用**，即按链接
目标解析后不存在者。判定口径：

| 类别 | 处理 |
| --- | --- |
| 活跃文档的链接，目标已迁移到可唯一定位的新路径 | 改指真实路径 |
| 活跃文档的链接，目标在本仓库从未存在或已删除且无继任者 | 降级为行内代码并加注说明，不留悬空链接 |
| 历史文档中记录当时事实的路径 | 不改动，保留为历史证据 |
| 代码块内的目录树片段（`Component/`、`Query/` 等） | 不是路径引用，不在范围 |

「活跃文档」= `Context/**`、根 `AGENTS.md`、`Test/README.md`、`src/**/README.md`、
`docs/README.md`、`CR-2026-11-*` 与 `docs/{architecture,reviews,plans}/*2026-11*`，共 90 份。

## 2. 理由

目录重命名与扁平化（`Components/` → `Share/`、`ItemUse/` → `Share/ItemUse/`、
`EntitySpawn|Session|TileEntity` → `World/ComponentSystem/`）后，部分文档仍指向旧目录；
另有些条目指向从未建立的路径。悬空链接会让读者与 AI 会话按错目录检索，属于导航层失效。

## 3. 修复清单

| 文件 | 原引用 | 处理 |
| --- | --- | --- |
| `src/NSSLC/README.md` | `ItemUse/` → 该域已不存在 | 改为 `Share/ItemUse/`（该域在 `Share/` 下） |
| `src/NSSLC.Infrastructure/Network/Design/README.md` | `../../../NSSLC.Tools.NetworkServer/README.md` | 该项目在仓库与 git 历史中都不存在；改为指 `src/NSSLC.Server` 的 `-networktest` 宿主与 `Build/runtime/` 探针 |
| `Context/项目约束/非函数式编码副作用隔离规范.md` | 两条 `../../dome/dome1/docs/research/*.md` | 仓库根无 `dome/`；降级为行内代码并注「原 dome 工作区，已不在本仓库内」 |
| `Context/项目约束/Google-CSharp-Style-Guide-约束.md` §29 | 指向 `公共拆分约束.md` 的相对链接 | 该文件全盘不存在；改为文字说明并指向两份实际存在的拆分约束 |
| `docs/reviews/audits/2026-11-20-attribute-system-architecture-review.md` | 约 30 处 `src/NSSLC/Components/Share/…:行号` | 前缀改为 `src/NSSLC/Share/…`，按四类职责目录补 `Component/`、`System/`、`Buff/System/`；去掉 `:行号` 后缀（带行号的链接无法解析） |
| 同上 | `…/Buff/BuffDefinition.cs` | 该文件现名 `Buff.cs`（`Share/Attributes/Buff/System/Buff.cs`），链接已改指新文件 |
| `src/NSSLC.Infrastructure/Network/Pocket/README.md`、`src/NSSLC.Infrastructure/WorldGeneration/README.md` | `Build/diagnostics/PocketIntegration/`、`Build/diagnostics/WorldGenerationCompile/` | 子目录已在本地证据区清理（`/Build/diagnostics/` 在 `.gitignore` 内）；措辞改为过去时并注明属本地证据区 |
| `.agents/skills/version4-partition-session-runner/sessions/**/task-table.md`（5 份） | 25 条 `../../../AGENTS.md` 等 | 层级算错：文件在 `sessions/<会话>/tasks/<任务>/` 下 7 层，`../../../` 只到 `sessions/`；统一改为 `../../../../../../../` |

`Context/架构约束/ECS领域与基础设施架构边界.md` 曾指向 `src/NSSLC.Application/…`，该条目已在并行会话中
改写为「本仓库没有独立的 Application 项目」并改指 `WorldStorageCoordinatorFactory.cs`，本轮无需再改。

## 4. 未改动对象

- 历史文档中的旧路径（`docs/component-decomposition/**`、`docs/migration/**`、
  `docs/plans/**`、2026-11-01～11-06 的审计）：记录当时事实，改写会伪造史实。
- 各 CR 中对旧目录名的记述（如 `Context/约束` → `Context/项目约束` 的迁移记录）：即重命名文档本身。
- `Test/README.md` §4 的 `Test/Terraria.Components/Npc/`：原文已注明「现在不建」，是有意示例。
- `src/NSSLC.Infrastructure/Diagnostics/README.md` 中「原位置 `src/NSSLC/Diagnostics/`」：描述迁移来源。
- `Context/架构约束/*` 的正文规则与 `src/`、`Test/`、构建策略：本轮只修路径引用。

## 5. 验证

- 90 份活跃文档的 markdown 相对链接全部解析成功：断链 **0**。
- 5 份 `task-table.md` 的 25 条链接全部解析成功：断链 **0**。
- 指向 `AGENTS.md` 的全仓相对链接 9 条、`AGENTS.md` 自身 15 条链接全部解析成功。
- 解析到根 `README.md` 的引用：**0**（该文件已被用户删除，无残留引用）。
- 本工作区的 `git` 子进程不可用，以上结论由逐文件存在性解析得出。

## 6. 回滚

```powershell
cd D:\TRbackup\NLTX
git checkout -- src/NSSLC/README.md src/NSSLC.Infrastructure Context docs/reviews/audits/2026-11-20-attribute-system-architecture-review.md
# .agents/ 被 .gitignore 排除，需手工把 ../../../../../../../ 改回 ../../../
```
