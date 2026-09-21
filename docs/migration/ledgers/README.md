# Version4 → NLTX 成员迁移参考表

本目录是当前 checkout 的 Version4 成员迁移账本入口。

## 唯一事实源

`Version4-member-migration-map.json` 是唯一人工维护的迁移事实源。它以当前绑定的 Roslyn
快照和显式 target manifest 为范围，保存：

- `inventory.sourceMembers`：Version4 字段/属性候选及稳定 `sourceMemberId`；
- `decisions`：每个源成员恰好一条扁平处置记录；
- `evidenceCatalog`：第一轮审查、声明、读者、写者、权威和生命周期证据；
- `workItems`：可脱离聊天历史恢复的结构化后续工作；
- `generatedViewMetadata`：派生视图的事实哈希和文件哈希。

## 文件角色

| 文件 | 角色 |
| --- | --- |
| `Version4-member-migration-map.json` | 唯一事实源 |
| `Version4-member-migration-quick-reference.json` | AI 优先读取的机器速查视图 |
| `Version4-member-migration-quick-reference.md` | 人工审查用速查视图 |
| `Version4-member-migration-context-packet.json` | 当前唯一队列项的有界恢复包 |
| `Version4-component-target-index.json` | 显式目标 manifest 的反向索引 |
| `version4-member-migration-audit.json` | 最近一次确定性审计结果 |
| `version4-member-migration-ledger.schema.json` | 本账本使用的 JSON Schema |

源/目标快照和 target manifest 保留在 `Build/generated/`，并由账本中的路径与 SHA-256 绑定；它们
不是本目录的第二事实源。

## AI 恢复顺序

1. 先读取 `Version4-member-migration-quick-reference.json` 的 `recoveryHeader` 和队列；
2. 若存在 active claim，只恢复该 `workItemId`；否则按稳定排序选择唯一下一项；
3. 只读取该 context packet 指出的源成员、第一轮审查证据、目标声明和验证引用；
4. 需要改变事实时只修改 `Version4-member-migration-map.json`，然后重新生成本目录全部派生视图；
5. 运行 `audit_ledger.py`，审计失败时保留 blocker，不提升为 `verified`；
6. 未完成事项必须留在 `nextWorkItemRef` 和结构化 `workItems` 中。

禁止根据组件数量、目录位置、类名相似度、历史聊天记录或一份声明性设计文档猜测迁移关系。
`D:/TRbackup/Version4` 是唯一完整覆盖基线；`dome/dome1/docs/migrations/` 仍是历史证据区。账本工作不
修改 `src/`、`dome/src/`、测试代码或 Version4 源码，且物理删除旧成员始终禁止。

## 当前基线状态

本次建立基于 2026-09-07 当前 checkout 中的绑定快照和第一轮审查结构化分析：506 个源成员、506
条 decision、3,210 条证据、64 个 work item、81 个显式目标成员。76 条 decision 已记录显式
`move`/`merge`/`split` 目标但仍处于 `migrated`，430 条 decision 处于 `deferred`（其中 5
条是兼容投影边界）；目标反向索引实际引用 80 个成员，另有 1 个明确标记为 `origin: new` 的
权威设计根。验证目录为空，因而 `verified` 数为 0；最近一次审计为 `errors: 0`。
这表示覆盖账本和恢复入口已经建立，不表示 Version4 行为、网络、持久化或运行时等价已经完成。

最近一次审计结果见 `version4-member-migration-audit.json`。审计通过只证明账本结构、快照绑定、
显式目标反向覆盖和证据引用没有违反门禁；它不替代成员级读写闭合或运行时验证。
