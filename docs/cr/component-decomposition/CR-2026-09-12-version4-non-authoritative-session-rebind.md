# CR-2026-09-12 Version4 非权威组件 session 重绑定

## 状态

- status: `scheduled`
- level: `major`
- decision: `accepted`
- approved_by: `user`
- scheduled_iteration: `current-work-item`
- proposed_at: 2026-09-12（Asia/Shanghai）
- confirmed_at: 2026-09-12（Asia/Shanghai）
- partition: `P12`
- oldSessionId: `414d8b6cfd644e0f9858c9c38ea464e0`
- newSessionId: `45463d93b7a6426a8a901e48dd9c7b06`

## 需求原文

> 绕过旧session   绑定到新session

提出渠道：本次对话。

## 变更内容

允许 P12 非权威组件实施任务在旧 runner session 已失败、且第二轮文档仍绑定旧 session
时，由明确的人工 handoff 将两份当前 P12 第二轮文档重绑定到新 runner session。此变更
仅适用于当前 P12；不修改历史 session 的实现记录，不把旧 session 冒认成新 session，也不
编辑 runner ledger 或 lock。

重绑定步骤固定为：

1. 通过非权威 runner 清理已失败的 P12 记录。
2. 通过指定分区重新领取，取得新的 manual session ID。
3. 在两份 P12 第二轮文档中同步新的 `sessionId`，保留已有 checkpoint、组件列表和验证证据。
4. 复查两个文档的 `partitionId`、`inputReport`、checkpoint 和新 session 一致后结算 runner。

## 影响评估

| 项目 | 结论 |
| --- | --- |
| 源码行为 | 不改变；重绑定不等于重新实现或行为等价证明 |
| 文档审计 | 记录旧 session、新 session 和 handoff 原因；保留原完成组件与验证状态 |
| runner ledger | 只通过现有 `Cleanup`、`Claim` 和最终结算接口修改，不手工编辑 |
| 并发与锁 | 复用 runner 独占 checkout lock；P12 旧记录已为 failed，ownerProcessId 为空 |
| 回滚 | 可再次通过 runner `Cleanup` 清理新 claim；不得恢复旧 session 为活动 owner |
| 风险 | 旧 session 的源码变更由新 session 采用；不重新验证时不得扩大行为等价声明 |

## 验收标准

- P12 通过 runner 取得新的 `status=claimed`、`claimMode=manual`、`lockReleased=true` 结果。
- 两份 P12 第二轮文档的 `sessionId` 均等于新 runner session，其他 checkpoint 字段同步保留。
- 旧 session `414d8b6cfd644e0f9858c9c38ea464e0` 不再作为活动 owner。
- 不修改 `src`、其他 partition 文档、其他 session 文档、ledger 或 lock 的内容来源。
- 最终 runner 结算使用新 session ID，并记录真实结果。

## 未覆盖范围

- 不新增 handoff runner action。
- 不声称源码由新 session 重新编译、测试或证明行为等价。
- 不改变非权威组件实现模式对 Component-only 修改范围的原有约束。
