# CR-2026-09-12 Version4 非权威分区会话清理接口

## 状态

- status: `scheduled`
- level: `moderate`
- decision: `accepted`
- approved_by: `user`
- scheduled_iteration: `current-work-item`
- proposed_at: 2026-09-12（Asia/Shanghai）
- confirmed_at: 2026-09-12（Asia/Shanghai）

## 需求原文

> 为version4-non-authoritative-partition-session也加入此接口并测试

提出渠道：本次对话。

## 变更内容

为 `Build/Tools/Invoke-Version4NonAuthoritativePartitionSession.ps1` 增加显式
`-Action Cleanup -PartitionId Pnn` 接口。调用方确认目标任务可以重新进入后，runner 在
现有 checkout lock 内清空指定分区的全部会话与 worker 动态信息，并将该分区恢复为
`available`。接口不要求旧 `SessionId`，可处理 `available`、`running`、`abandoned`、
`failed` 和 `completed`。

既有批量 `Prune -PreserveRunning` 语义保持不变。`Cleanup` 是单分区强制清理，且即使带
`-Retry` 也不能顺手 reconcile 其他分区。清理正在执行的 managed run 后，旧 worker 的
最终结算必须因 session ownership 失效而被拒绝，不能覆盖新的 `available` 状态。

## 影响评估

| 项目 | 结论 |
| --- | --- |
| 数据库或持久化格式 | 不涉及；复用现有版本 2 JSON ledger 字段 |
| 运行时代码 | 不涉及；仅修改 PowerShell runner、focused contract test 和 skill 文档 |
| 状态生命周期 | 指定分区可从任意已有状态恢复为 `available`，包括 `completed` |
| 并发与锁 | 复用现有独占 `FileStream` checkout lock，清理在锁内读取、重置和保存 |
| 主要风险 | 清理 `running` 记录会允许重复领取，调用方必须先确认旧任务已中断或可丢弃 |
| 验证范围 | 各状态清理、字段清空、重新 Claim、幂等、分区隔离、`-Retry` 隔离、锁竞争和旧 worker 防覆盖 |

## 验收标准

- `Cleanup -PartitionId Pnn` 返回 `status: cleaned`、`previousStatus` 和
  `lockReleased: true`，且不要求旧 session ID。
- 清理后目标分区为 `available`，session、owner、时间戳、命令、输出、错误和退出码均为空。
- 分区固定定义和报告统计字段保留；清理后可重新 `Claim`。
- `running`、`completed`、`failed`、`abandoned` 和已经 `available` 的分区都可清理。
- 清理一个分区不改变其他分区；带 `-Retry` 也不触发其他分区 reconcile。
- 锁竞争返回 `busy`，释放锁后可再次清理；旧 managed worker 不能覆盖清理结果。
- `Test-Version4NonAuthoritativePartitionSession.ps1`、PowerShell parser 和空白检查通过。

## 交付与验证

运行 focused contract suite：

```powershell
pwsh -NoProfile -File .\Build\Tools\Test-Version4NonAuthoritativePartitionSession.ps1
```

本次只修改 PowerShell runner、测试和 skill 文档，不运行 `dotnet` 构建。测试使用临时
state/lock 路径；共享 `.agent-workplace/state` ledger 未被本次测试写入。
