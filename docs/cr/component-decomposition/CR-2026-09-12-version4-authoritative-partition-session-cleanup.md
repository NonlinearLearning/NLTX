# CR-2026-09-12 Version4 权威分区会话清理接口

## 状态

- status: `scheduled`
- level: `moderate`
- decision: `accepted`
- approved_by: `user`
- scheduled_iteration: `current-work-item`
- proposed_at: 2026-09-12（Asia/Shanghai）
- confirmed_at: 2026-09-12（Asia/Shanghai）

## 需求原文

> 为version4-authoritative-partition-session编写清理信息接口,现在会话中断了重入任务时ai清理不掉相关信息会残留会话信息导致新会话重入不了任务

> 我知道需要清理什么任务,所以此接口是将信息全部删除

提出渠道：本次对话。

## 变更内容

为 `Build/Tools/Invoke-Version4AuthoritativePartitionSession.ps1` 新增显式
`-Action Cleanup -PartitionId Pnn` 接口。调用方已经确认需要清理的任务后，runner 在
checkout lock 内清空该分区全部会话与执行动态信息，并将它恢复为 `available`，使新会话
可以重新领取。

清理适用于 `available`、`running`、`abandoned`、`failed` 和 `completed`，不要求旧
`SessionId`。清理后保留固定 P01-P20 清单信息和报告验证数据，不删除分区记录本身或整个
JSON ledger。重复清理同一分区必须幂等成功。

## 影响评估

| 项目 | 结论 |
| --- | --- |
| 数据库或持久化格式 | 不涉及；只扩展私有 JSON ledger 的 runner 行为 |
| 运行时代码 | 不涉及；修改 PowerShell runner 和 focused contract test |
| 状态生命周期 | 显式清理可以把 `completed` 恢复为 `available`，允许确认后的任务重入 |
| 并发与锁 | 复用现有 checkout lock，在单次锁事务内完成读取、重置和持久化 |
| 主要风险 | 清理仍在执行的 `running` 记录会允许重复领取；调用方必须先确认会话已中断 |
| 验证范围 | 各状态清理、清理后重新 Claim、动态字段清空、隔离其他分区、锁与输入错误 |

## 验收标准

- `Cleanup -PartitionId Pnn` 对合法分区返回 `status: cleaned`、`previousStatus` 和
  `lockReleased: true`。
- 清理后目标分区为 `available`，旧 `sessionId`、进程、时间戳、命令、输出、错误和退出码
  均为空；固定 report/manifest 字段保留。
- `running`、`failed`、`abandoned`、`completed` 清理后均可以重新 `Claim`。
- 已经是 `available` 的分区重复清理成功，且其他分区状态不变。
- 无效分区、无效报告和锁竞争沿用现有错误码与保护行为。
- focused contract test 通过，且 `git diff --check` 无错误。

## 交付与验证

实现由当前工作项执行。由于只修改 PowerShell 工具，不运行 `dotnet`；运行
`pwsh -NoProfile -File .\Build\Tools\Test-Version4AuthoritativePartitionSession.ps1`
作为 focused contract suite。运行时 ledger 和 lock 继续位于 `.agent-workplace/state/`，不
提交到 Git。
