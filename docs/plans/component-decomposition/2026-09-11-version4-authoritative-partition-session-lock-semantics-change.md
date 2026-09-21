# Version4 权威分区领取锁语义变更记录

## 变更原文

> 注意这个锁是为了保证任务不被重复领取,领取任务并已领取就必须释放锁

## 变更分级

- `changeId`: `CR-2026-09-11-version4-partition-claim-lock`
- `level`: `moderate`
- `decision`: `accepted`
- `approvedBy`: `user`
- `channel`: 当前会话消息
- `date`: `2026-09-11`

## 原协议

runner 原先从领取开始一直持有 checkout 独占锁，直到外部 worker 执行和最终状态写入结束。这个语义会阻止不同分区并行执行，锁的保护范围大于“防止重复领取”的需求。

## 新协议

锁只覆盖两个短临界区：

1. `Claim`/`ClaimNext` 或 `Run`/`RunNext` 的报告验证、状态读取、分区选择、状态写入；
2. `Complete`/`Fail`/`Abandon` 或托管 `Run` 的最终状态写入。

领取成功后立即释放锁。不同分区可以并行分析和执行；同一分区因为 ledger 已是 `running`，重复领取返回 `conflict`。手工 `Claim` 记录使用 `claimMode: manual`，不会因为领取命令进程结束而被自动判定为 abandoned；托管 `Run` 仍记录 owner process，并支持死亡 owner 恢复。

## 影响范围

- runner 新增 `Claim`、`ClaimNext`、`Complete`、`Fail`、`Abandon` 动作；
- `Run`/`RunNext` 改为领取和执行分离锁，但保留自动化 worker 兼容；
- 状态账本 schema 升级为 `2`，新增 `claimMode` 和更完整的领取/结算字段输出；
- 所有并行会话公共提示词改为先领取、再写两份 Markdown、最后结算；
- 现有 P01-P20 清单、成员数、报告只读约束和退出码保持不变；
- 不修改 Version4、NLTX `src/`、测试项目或生产 C# 文件。

## 验证要求

- 同一分区重复 `Claim` 必须返回 `conflict`；
- 领取 P01 后，另一会话领取 P02 必须成功；
- `Complete`/`Fail` 必须验证 `sessionId` 所有权；
- 托管 `Run` 的 child 失败后仍写入 `failed` 并释放锁；
- 死亡托管 owner 可以变为 `abandoned` 并通过 `-Retry` 恢复；
- 契约测试不得再把“执行期间第二分区 busy”作为正确行为。
