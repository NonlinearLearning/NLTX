# 约束目录更名为项目约束

文档 ID：CR-2026-11-24-CONSTRAINTS-DIR-RENAME  
逻辑域：file-organization  
产物类型：decision、execution  
状态：completed  
日期：2026-11-24  
canonical 路径：`docs/cr/CR-2026-11-24-constraints-dir-rename.md`  
上游依据：[项目文件组织设计约束](../../Context/架构约束/项目文件组织设计约束.md) §3、§5、§6；
[开发协作与变更约束](../../Context/项目约束/开发协作与变更约束.md) §变更边界、§导航维护

## 1. 授权与范围

用户要求：把 `D:\TRbackup\NLTX\Context\约束` 改为项目约束。本轮只改目录名和指向它的路径引用，
不新增、删除或改写任何约束正文；`src/`、`Test/`、项目文件与构建策略未改动。

| 起点 | 目标 | 文件数 |
| --- | --- | --- |
| `Context/约束/` | `Context/项目约束/` | 6 |

6 个文件名保持不变：`ECS系统输入边界约束.md`、`Google-CSharp-Style-Guide-约束.md`、
`progress信息保存约束.md`、`开发协作与变更约束.md`、`构建与验证约束.md`、
`非函数式编码副作用隔离规范.md`。`git status` 全部识别为重命名（5 个 R100，1 个另带本轮之前
已存在的工作区改动）。

## 2. 命名理由

`Context/架构约束/` 下已有 `…设计约束.md` 一族文档，同级的 `约束/` 只说明「是约束」，不说明
约束的适用范围；`项目约束/` 明确该目录承载仓库级编码、构建与流程约束，与 `架构约束/` 的
设计约束在路径上即可区分。同级目录 `架构设计/` 已于同日改名为 `架构约束/`，见
[CR-2026-11-24 架构设计目录更名](CR-2026-11-24-arch-constraints-dir-rename.md)。

## 3. 引用更新

按「移动后修复引用」处理，判定规则与 `.agent-workplace/scripts/fix-stale-doc-paths.ps1` 一致：
旧路径不解析、新路径解析时才改写，并保留原有分隔符风格与相对锚点。

| 范围 | 处理 |
| --- | --- |
| `AGENTS.md` | 6 处入口链接改指 `Context/项目约束/…` |
| `Context/架构约束/`、`Context/调试工具/` | `../约束/…` 相对链接改为 `../项目约束/…` |
| `Test/README.md` | 1 处相对链接 |
| `docs/`（architecture、plans、plans/component-decomposition、research、reviews、component-decomposition、system-decomposition、cr） | 路径字符串同步；正文论断、证据缺口记录与结论不变 |
| `.agents/skills/public-decomposition/`、`.agent-workplace/` | 技能参考资料与派发提示词中的路径；两个路径迁移脚本的映射目标改为 `Context/项目约束` |

合计 78 个 Markdown/脚本文件、167 处 `Context/项目约束` 路径引用（含 `.agent-workplace/` 下不纳入版本库的派发文档与脚本）。历史文档里的路径字符串随
本轮改名同步（旧目录已不存在，改写后指向真实目录）；其中「某文件在当前 checkout 缺失」的判定
保持原样，`公共拆分约束.md` 等仍是已知缺失，不因改名变成已存在。

## 4. 未改动对象

- 6 个约束文档的条款、编号、标题锚点与文件名不变。
- `src/`、`Test/` 源码、`Build/`、项目与构建配置不变；本轮未运行构建或测试——改名只影响文档
  路径，不进入编译面。
- 工作区在本轮之前已存在的改动（`src/`、`Test/` 下的文件，`Context/progress.md`，
  `progress信息保存约束.md`）原样保留，未回退也未格式化。

## 5. 验证

- 全仓检索 `Context/约束`、`Context\约束`：除本 CR 对旧名的记述外 0 处残留。
- 全仓 Markdown 链接检查：指向新目录的 56 条链接全部解析到存在的文件。
- `git status`：6 个文件为重命名，无删除；`Context/项目约束/` 下文件数与改名前的 6 个一致。

## 6. 回滚

```powershell
cd D:\TRbackup\NLTX
git mv Context\项目约束 Context\约束
git checkout -- AGENTS.md Test/README.md Context/架构约束 Context/调试工具 docs .agents
```

第二条命令按路径整片还原引用改动；这些路径在本轮之前没有未提交改动，因此不会连带丢弃他人
改动。若只想撤回部分文件，改用 `git diff -- <path>` 逐条还原。`.agent-workplace/` 被
`.gitignore` 排除，其内的两个迁移脚本与派发提示词需手工把 `Context/项目约束` 改回
`Context/约束`。
