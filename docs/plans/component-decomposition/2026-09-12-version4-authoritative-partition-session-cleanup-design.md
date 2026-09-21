# Version4 Authoritative Partition Session Cleanup Design

## Context

`Build/Tools/Invoke-Version4AuthoritativePartitionSession.ps1` 使用私有 JSON ledger 记录
P01-P20 权威分区的 claim、执行和结算状态。会话中断后，手工 claim 可能遗留为 `running`；
即使调用方已经知道该任务确实可以重新进入，旧记录仍会阻止新的 claim。现有
`Abandon` 需要旧 `sessionId`，不适合会话信息已经无法恢复的清理场景。

## Goal

增加一个按分区执行的强制清理接口。调用方明确指定需要清理的分区后，runner 在 checkout
lock 内原子删除该分区的全部会话和执行动态信息，并将它恢复为 `available`，使新会话能够
重新领取任务。

## Decision

新增动作：

```powershell
pwsh -NoProfile -File .\Build\Tools\Invoke-Version4AuthoritativePartitionSession.ps1 `
  -Action Cleanup `
  -PartitionId P04 `
  -LockWaitSeconds 60
```

`Cleanup` 只要求 `-PartitionId`，不要求旧 `-SessionId`。它允许清理目标分区当前的任何
状态，包括 `available`、`running`、`abandoned`、`failed` 和 `completed`。操作在现有
checkout lock 内完成并原子持久化；重复清理同一分区是幂等的。

清理后保留固定清单信息：

- `id`
- `reportFileName`
- `reportPath`
- `expectedMemberCount`
- `observedMemberCount`

清理以下会话与执行动态信息：

- `status`（设置为 `available`）
- `claimMode`
- `sessionId`
- `ownerProcessId`
- `ownerProcessStartTime`
- `claimedAtUtc`
- `startedAtUtc`
- `completedAtUtc`
- `abandonedAtUtc`
- `exitCode`
- `commandPath`
- `commandArguments`
- `stdout`
- `stderr`
- `error`

不会删除分区记录本身，也不会删除整个 ledger；固定 P01-P20 集合仍由 runner 校验。

## Alternatives Considered

1. 只清理 `running`、`abandoned` 和 `failed`。这不能满足调用方明确要求清除全部信息的
   场景，也不能处理已经错误完成但需要重新执行的分区。
2. 删除整个 JSON ledger。范围会扩散到其他并行分区，可能破坏无关会话，不采用。
3. 强制要求旧 `sessionId`。会话中断后旧 ID 可能不可得，无法解决当前重入阻塞问题。

## State and Concurrency

`Cleanup` 与 `Claim`、`Complete`、`Fail`、`Abandon` 使用同一 lock path 和独占
`FileStream`。清理流程为：验证目标 report、获取锁、加载 ledger、读取目标分区旧状态、
重置目标记录、保存 ledger、释放锁、返回结构化结果。其他分区的记录不被修改。

返回结果包含 `status: cleaned`、目标 `partition`、`previousStatus` 和
`lockReleased: true`。如果目标已经是 `available`，仍返回成功；如果报告、分区 ID 或锁
事务无效，则沿用现有 validation/conflict/busy 错误码。

该接口有意允许调用方覆盖仍处于 `running` 的记录。调用方必须只对已确认中断的任务使用
它；runner 不尝试推断调用方的业务判断，也不通过进程存活状态阻止显式清理。

## Verification Plan

扩展 `Build/Tools/Test-Version4AuthoritativePartitionSession.ps1`，覆盖：

- 清理 `running` 后可以使用 `Claim` 重新领取；
- 清理 `completed` 后可以重新领取；
- 清理 `failed` 和 `abandoned` 后会清空旧 session、进程、命令、输出、时间戳和错误字段；
- 对已是 `available` 的分区重复清理成功且不影响其他分区；
- 清理一个分区不会改变另一个分区的状态；
- 清理与并发 claim 共享同一 checkout lock；
- 缺少或非法 `PartitionId` 不写入 ledger。

这是 PowerShell runner 的 focused contract test，不需要运行 `dotnet` 构建。运行时 ledger 和
lock 继续放在 `.agent-workplace/state/`，不进入 Git。
