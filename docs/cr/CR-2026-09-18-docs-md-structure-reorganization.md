# CR-2026-09-18：Markdown 文档结构重设

状态：accepted
级别：major（文档路径重构）
提出方式：用户直接请求“重新设文件结构来管理 md”
范围：仓库根目录 `docs/` 的 Markdown 文档及其必要的相邻迁移索引文件

## 变更目的

将当前按中文历史批次和临时工作目录混合组织的文档，整理为按领域和交付用途导航的
结构，使 System 拆分、Component 拆分、迁移 ledger、研究、审查和计划可以分别定位。

## 现状基线

- 当前工作树中已经存在旧路径删除和新 `组件文档/` 路径未跟踪并存的重组状态。
- 本 CR 不恢复用户已有删除，不覆盖用户已有未跟踪内容，不修改源码和测试代码。
- 历史文档正文保持不变；只改变物理路径、必要的文件名和相对 Markdown 链接。
- 移动前后通过 `document-move-map.tsv` 保留源路径、目标路径和操作状态。

## 目标结构

```text
docs/
├── README.md
├── architecture/
│   ├── decisions/
│   └── entity-identity-context.md
├── system-decomposition/reports/
├── component-decomposition/
│   ├── baseline/
│   ├── review-round-1/
│   └── review-round-2/
├── migration/
│   ├── assessments/
│   └── ledgers/
├── reviews/
│   ├── audits/
│   └── human/
├── plans/
│   ├── component-decomposition/
│   ├── system-decomposition/
│   └── tasks/
├── research/
└── cr/
```

不存在实际文件的目录不创建；已有分区目录和审查轮次目录在所属领域下原样保留，以免
不同文件同名时发生覆盖。

## 执行规则

1. 先创建目标目录并检查目标路径不存在同名文件。
2. 按 `document-move-map.tsv` 移动现有内容，保留文件内容和修改时间。
3. 对移动后的 Markdown 相对链接做机械重写；外部绝对路径、源码引用和历史文字不改写。
4. 生成当前路径的 Markdown manifest，标注逻辑域、产物类型和 canonical 路径。
5. 检查文件数量、同名冲突、入口链接、移动映射和 `git diff --check`。

## 回滚

使用 `document-move-map.tsv` 中的反向 `target -> source` 映射逐项移回；不使用递归删除、
`git clean`、`git reset --hard` 或 `git checkout --`。若链接重写已发生，使用同一映射
重新计算相对路径，不以批量字符串替换猜测回滚内容。

## 验收口径

- 所有原有 Markdown 文件仍存在，数量不因整理减少。
- 每个 Markdown 文件有且只有一个当前 canonical 路径。
- System、Component、迁移、审查、计划和研究材料能从根入口定位。
- 移动不改变文档结论，不把设计或 ledger 误标为行为迁移完成。
