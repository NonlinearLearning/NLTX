# 两份领域设计约束移入 docs/architecture

文档 ID：CR-2026-11-24-DOMAIN-CONSTRAINTS-TO-DOCS-ARCHITECTURE  
逻辑域：file-organization  
产物类型：decision、execution  
状态：completed  
日期：2026-11-24  
canonical 路径：`docs/cr/CR-2026-11-24-domain-constraints-to-docs-architecture.md`  
上游依据：[项目文件组织设计约束](../../Context/架构约束/项目文件组织设计约束.md) §3.3、§5、§6；  
[开发协作与变更约束](../../Context/项目约束/开发协作与变更约束.md) §变更边界、§导航维护  
前置：[CR-2026-11-24 架构设计目录更名](CR-2026-11-24-arch-constraints-dir-rename.md)

## 1. 授权与范围

用户要求：把 `物品载荷与双宿主设计约束` 移到 docs 下的文件夹；随后补充 `ECSEntity组织设计约束`
同样处理。目标目录按 `docs/README.md` 对 `architecture/` 的定义「架构设计与已确认约束」选定为
`docs/architecture/`，文件名保持原样。

| 起点 | 目标 | 文件数 |
| --- | --- | --- |
| `Context/架构约束/物品载荷与双宿主设计约束.md` | `docs/architecture/物品载荷与双宿主设计约束.md` | 1 |
| `Context/架构约束/ECSEntity组织设计约束.md` | `docs/architecture/ECSEntity组织设计约束.md` | 1 |

两份文档的条款、编号、标题锚点与文件名不变；`src/`、`Test/`、项目文件与构建策略未改动。

## 2. 理由

两份文档是**领域级**设计约束——实体身份/组件权威状态/生命周期（ECS Entity），以及物品身份、
物品堆、库存槽位与世界掉落物的载荷归属（Item Payload & Dual Host）。`docs/architecture/`
既有的 `2026-10-06-entity-organization-design.md`、`2026-11-23-item-payload-dual-host-*`
一族材料正是这两条约束的推导与执行记录，约束与被约束材料同处便于对照。

`Context/架构约束/` 保留 4 份跨领域通用约束：ECS 文件组织、ECS 领域层与基础设施边界、
组件命名、项目文件组织。划分后两处职责为：

| 目录 | 收什么 |
| --- | --- |
| `Context/架构约束/` | 跨领域、对所有子系统生效的通用组织与边界约束 |
| `docs/architecture/` | 具体领域的设计文档，以及该领域的专用设计约束 |

该划分不改变任何约束的效力等级：两份文档仍是正式约束，仍由根 `AGENTS.md` 提供入口链接
（[项目文件组织设计约束](../../Context/架构约束/项目文件组织设计约束.md) §3.3 要求正式约束必须在入口
可发现）。

## 3. 引用更新

`Context/架构约束/` 与 `docs/architecture/` 同为仓库根下二级目录，相对层级深度一致，因此
`../../Context/架构约束/X.md` → `../../docs/architecture/X.md` 只需替换目录段，`../` 数量不变。

| 范围 | 处理 |
| --- | --- |
| `AGENTS.md` | 2 处入口链接改指 `docs/architecture/…` |
| `docs/`（architecture、architecture/execution、cr、plans、plans/tasks、research、reviews/audits） | 路径字符串同步 |
| 被移动的两份文档 | 自身对兄弟约束的裸文件名链接改为 `../../Context/架构约束/…`；`../项目约束/…` 改为 `../../Context/项目约束/…`；`../../docs/…` 链接保持不变（深度等价） |
| `.agent-workplace/` | 进度记录与 SS14 文档校验脚本中的路径 |

共 12 个文件被改写，另有 2 个文件只有不带目录的文件名引用（审计文档写 `ECSEntity组织设计约束.md:49-50`），
不构成路径，保持原样。

## 4. 未改动对象

- 两份文档的正文条款与编号不变；文件名不变。
- `Context/架构约束/` 余下 4 份文档未被编辑，其互相链接不受影响。
- `项目文件组织设计约束.md` 的正文未改：§3.3 只说 `Context/架构约束/` 收已确认的架构、设计和
  约束文档，并未声明"所有约束都必须在此"；本轮划分与该条相容。
- `docs/README.md` 未改：它对 `architecture/` 的描述「架构设计与已确认约束」已覆盖本轮的落位，
  该 README 按自身声明只描述目录结构与用途，不做逐文件索引。
- `src/`、`Test/` 源码、`Build/`、项目与构建配置不变；未运行构建或测试。

## 5. 验证

- 全仓检索 `Context/架构约束/(物品载荷与双宿主设计约束|ECSEntity组织设计约束).md`：0 处残留。
- 两份文档自身的 17 条 Markdown 链接（10 + 7）全部解析到存在的文件。
- 全仓指向这两份文档的 8 条 Markdown 链接全部解析成功。
- `git status` 显示两者为重命名，无内容丢失。

## 6. 回滚

```powershell
cd D:\TRbackup\NLTX
git mv docs\architecture\物品载荷与双宿主设计约束.md Context\架构约束\
git mv docs\architecture\ECSEntity组织设计约束.md   Context\架构约束\
git checkout -- AGENTS.md docs .agent-workplace
```

第三条命令还原引用改动；`docs/architecture/` 与 `AGENTS.md` 在本轮之前没有未提交改动。
`.agent-workplace/` 被 `.gitignore` 排除，其内两处路径需手工回改。
