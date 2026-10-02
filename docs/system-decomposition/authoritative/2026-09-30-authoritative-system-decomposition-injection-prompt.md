# Version4 权威分区 System 拆分注入提示词

- 文档 ID：DOC-2026-09-30-authoritative-system-decomposition-injection-prompt
- 逻辑域：system-decomposition
- 产物类型：guide
- 状态：active
- 范围：Version4 权威 P01-P20 System 拆分单分区会话
- 证据入口：`ecs-system`、`version4-partition-session-runner` 及其权威 System 会话契约
- canonical 路径：`docs/system-decomposition/authoritative/2026-09-30-authoritative-system-decomposition-injection-prompt.md`

## 注入提示词（199 字）

用 `ecs-system` 拆分，API 不足回源码查关系；依 `version4-partition-session-runner` 的 System/权威 profile 只领一分区。按 claim 输入写 `outputReport`，用原 `sessionId` 结算。交 proposed 静态报告，不改代码、不跑构建/测试；缺口标 unknown，验证 not-run，不称迁移成功。
