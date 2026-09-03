# CR-2026-09-01: 清理迁移上下文保留约束

## 原始要求

> 清理掉当前代码迁移产生的所有上下文保留约束,并报告
>
> 2,并删除某些无用的记忆
>
> 不清理代码

## 执行边界

- 不修改 src/、Test/、.csproj、Directory.Build.*、运行时契约或验证器。
- 不删除 Build/diagnostics/、历史研究、迁移报告或历史证据。
- 清理模型入口中的当前迁移续接上下文。
- 清理 .agent-workplace/ 中已结束的迁移过程态；保留过程工具脚本。
- 通过记忆更新说明删除明显错误、失败或已被新请求替代的记忆条目。

## 结果口径

这是上下文清理，不是迁移完成、代码删除、行为等价或 WorldGen parity 声明。

## 执行结果

- 已删除 docs/plans/ 下全部 156 个迁移计划 Markdown，约 1,548,040 字节。
- 已删除 Flowstate 迁移 plan/task Markdown 3 个，约 26,930 字节。
- docs/plans/ 当前没有 Markdown 文件。
- Flowstate 剩余的 2 个 plan/task 文件属于文档规范化，不属于迁移计划，予以保留。
- .agent-workplace/ 的迁移过程态已清理；仅保留 9 个过程工具脚本。
- src/、Test/、项目文件、构建策略、docs/migrations/、docs/research/、
  docs/archive/ 和 Build/diagnostics/ 未被修改或删除。
- 文档清单已重新生成：DOCS=332 BUILD_REFS=1113。
- VerifyFlowstateDocs.ps1 通过：docs=332 manifest=332 buildReferences=1113。

历史归档和研究文档中的旧 docs/plans/ 路径没有批量改写，以避免篡改历史证据。
