# CR-2026-11-24 约束文档与代码漂移同步

| 项 | 内容 |
| --- | --- |
| 文档 ID | CR-2026-11-24-constraints-drift-sync |
| 逻辑域 | 文档治理 / 约束正文 |
| 产物类型 | 变更记录 |
| 状态 | 已实施 |
| 日期 | 2026-11-24 |
| canonical 路径 | docs/cr/CR-2026-11-24-constraints-drift-sync.md |
| 上游依据 | [约束与代码漂移审计](../reviews/audits/2026-11-24-constraint-drift-audit.md) |
| 前置 | [CR-2026-11-24-domain-constraints-to-docs-architecture](CR-2026-11-24-domain-constraints-to-docs-architecture.md) |

## 1. 授权与范围

依据 [漂移审计](../reviews/audits/2026-11-24-constraint-drift-audit.md) 的已确认项实施正文修正，
覆盖审计中的 A 类（已失效）与 B 类（代码已深化）共 13 条；D 类（需人工确认）一律不动。

## 2. 理由

约束正文引用了一批已不存在的目录、项目、类型和命名空间形状，按图索骥会得到错误结论；
另一批规则在代码中已被执行得比正文更严格。修正只让正文与现状对齐，不改变任何规则强度。

## 3. 引用更新

| 文档 | 改动 |
| --- | --- |
| Context/架构约束/项目文件组织设计约束.md | 删除 `dome/` 工作区与 `分类参考/` 边界（A1、A2）；§5 命名空间示例改为实测值 |
| Context/架构约束/ECS领域与基础设施架构边界.md | §6 用 `Terraria.NonAuthoritative.*` + WorldStorage 端口替换不存在的 `src/NSSLC.Application`（A3） |
| Context/项目约束/ECS系统输入边界约束.md | 术语域形状改为 `Terraria.Share/Unshared`（A4）；注册表改为实名（A5）；只读端口节按当前 `Seal()` 覆盖重写（B1）；存量节补现状（B4） |
| docs/architecture/物品载荷与双宿主设计约束.md | `ItemDefinition`→`Item`、`ItemDefinitionRegistry`→`ItemRegistry`、`ItemUseDefinition`→`ItemUse`（A6） |
| Context/项目约束/Google-CSharp-Style-Guide-约束.md | §19 命名空间深度改为 3–5 层实测（A7）；§9 增加 ECS 组件 `record struct` 例外（B3） |
| Context/项目约束/构建与验证约束.md | 登记 `PacketDesignCompiler.Generator` 的 `netstandard2.0` 例外（A8） |
| Context/架构约束/组件命名设计约束.md | §7 标注当前 0 个组件继承族，7.1/7.2 无适用对象（B2） |
| Context/项目约束/非函数式编码副作用隔离规范.md | 执行要求节补领域层副作用零残留现状（B5） |

## 4. 未改动对象

- 审计 D1–D6：`ChatNetTextFrame.cs` 的 `System.IO` 归属、`src/NSSLC/Build` 与 `科研.wld` 的去留、
  两处组件命名违规、15 vs 14 验证项目、22 行 tab——均需人工决定方向，本轮不写。
- `docs/architecture/` 与 `docs/research/` 下带日期的设计文档保留旧名，属历史记录。
- 审计文件本身不改；后续若 D 类有结论，增补到同一审计。

## 5. 验证

- 全部 13 份受约束管文档中，A 类旧名残留为 0（`dome/` 仅剩 §13 参考资料的历史路径，已注明不在仓库内）。
- 118 条 Markdown 链接中失效 1 条，为 `2026-11-23-projectile-penetration-budget-linkage-design.md`
  指向外部 GitHub 的既有外链，与本次改动无关。
- 未执行构建：本轮只改 Markdown，不触及源码。

## 6. 回滚

按上表逐文件 `git checkout -- <path>` 即可回滚；本 CR 未删除任何文件、未改名、未改动源码。
