# 架构设计目录更名为架构约束

文档 ID：CR-2026-11-24-ARCH-DESIGN-DIR-RENAME  
逻辑域：file-organization  
产物类型：decision、execution  
状态：completed  
日期：2026-11-24  
canonical 路径：`docs/cr/CR-2026-11-24-arch-constraints-dir-rename.md`  
上游依据：[项目文件组织设计约束](../../Context/架构约束/项目文件组织设计约束.md) §3、§5、§6；  
[开发协作与变更约束](../../Context/项目约束/开发协作与变更约束.md) §变更边界、§导航维护  
前置：[CR-2026-11-24 约束目录更名为项目约束](CR-2026-11-24-constraints-dir-rename.md)

## 1. 授权与范围

用户要求：把 `Context\架构设计` 改为架构约束。本轮只改目录名和指向它的路径引用，不新增、
删除或改写任何文档正文；`src/`、`Test/`、项目文件与构建策略未改动。

| 起点 | 目标 | 文件数 |
| --- | --- | --- |
| `Context/架构设计/` | `Context/架构约束/` | 6 |

6 个文件名保持不变：`ECSEntity组织设计约束.md`、`ECS文件组织设计约束.md`、
`ECS领域与基础设施架构边界.md`、`物品载荷与双宿主设计约束.md`、`组件命名设计约束.md`、
`项目文件组织设计约束.md`。`git status` 全部识别为重命名。

## 2. 命名理由

目录内 5/6 个文件是 `…设计约束.md` / `…架构边界.md`，承载的都是「已确认的架构与设计约束」
（[项目文件组织设计约束](../../Context/架构约束/项目文件组织设计约束.md) §3.3），`架构设计`
读起来像「架构的设计稿」，与目录内**正式约束**的性质不符；`架构约束/` 与同级的
`项目约束/` 一起构成「架构层约束 / 项目层约束」的对称命名。

两轮改名后 `Context/` 的目录语义为：

```text
Context/
  progress.md      # 当前上下文
  架构约束/        # 已确认的架构设计、边界与命名约束
  项目约束/        # 编码、构建与流程约束
  调试工具/        # 调试工具手册
```

## 3. 引用更新

按「移动后修复引用」处理，判定规则与 `.agent-workplace/scripts/fix-stale-doc-paths.ps1` 一致：
旧路径不解析、新路径解析时才改写，并保留原有分隔符风格与相对锚点。

| 范围 | 处理 |
| --- | --- |
| `AGENTS.md` | 6 处入口链接改指 `Context/架构约束/…` |
| `Context/项目约束/` | `../架构设计/…` 相对链接改为 `../架构约束/…` |
| `README.md`、`Test/README.md`、`src/**/README.md` | 仓库与项目入口中的相对链接 |
| `docs/`（architecture、plans、research、reviews、component-decomposition、system-decomposition、cr） | 路径字符串同步；正文论断、证据缺口记录与结论不变 |
| `.agents/skills/`、`.agent-workplace/` | 分区任务表、技能参考资料与派发提示词中的路径；两个路径迁移脚本的映射目标改为 `Context/架构约束` |

`Context/架构设计/` 的旧写法在改动前已存在于 `fix-stale-doc-paths.ps1` 的历史规则表中（根级
`架构设计/` → `Context/架构设计/`），本轮把该规则的**目标值**改为 `Context/架构约束/`，旧
token 保留，以便继续识别遗留文档里的根级写法。

## 4. 未改动对象

- 6 个文档的条款、编号、标题锚点与文件名不变。
- `src/`、`Test/` 源码、`Build/`、项目与构建配置不变；本轮未运行构建或测试——改名只影响文档
  路径，不进入编译面。
- `.agent-workplace/docs/change/` 下两份 2026-09-02 变更记录里的根级 `架构设计/…` 字样
  **保持原样**：它们记录的是当时实际执行的改名操作，指向的文件早已不存在，属于已知的悬空
  历史引用，不因本轮改名而改写。
- 工作区在本轮之前已存在的改动原样保留，未回退也未格式化。

## 5. 验证

- 全仓检索 `Context/架构设计`、`Context\架构设计`：除本 CR 对旧名的记述外 0 处残留。
- 全仓 Markdown 链接检查：指向新目录的 150 条链接中，除 5 条本轮之前即悬空的
  `.agents/skills/version4-partition-session-runner/sessions/**/task-table.md`（相对层级少了两级，
  改名前同样解析不到）外全部解析到存在的文件。
- `git status`：6 个文件为重命名，无删除；`Context/架构约束/` 下文件数与改名前的 6 个一致。

## 6. 回滚

```powershell
cd D:\TRbackup\NLTX
git mv Context\架构约束 Context\架构设计
git checkout -- AGENTS.md README.md Test/README.md src Context docs .agents
```

第二条命令按路径整片还原引用改动；这些路径在本轮之前没有未提交改动。`.agent-workplace/`
被 `.gitignore` 排除，其内的脚本与派发提示词需手工把 `Context/架构约束` 改回
`Context/架构设计`。
