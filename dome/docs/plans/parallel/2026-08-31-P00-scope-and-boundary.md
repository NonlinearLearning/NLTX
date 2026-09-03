# P00 Scope and Boundary Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** 冻结所有并行字段审计共用的服务器范围、字段卡格式、状态词和 Protocol 排除边界。

**Architecture:** 以 source declaration 为事实入口，以唯一 owner、生命周期和 persistence
边界为字段卡核心。P00 只定义合同和负向边界，不为任何领域创建运行时实现或组件。

**Tech Stack:** Documentation-only Markdown; no C# changes, no new functions, no tests/builds.

---

## 1. Task identity

| 项目 | 内容 |
| --- | --- |
| Task | `P00` |
| Parent plan | `docs/plans/2026-08-31-server-required-field-property-migration-plan.md` |
| Wave | `0`，所有领域任务的前置合同 |
| Write set | 仅本文件 |
| Read set | Root AGENTS、progress、Flowstate README、active plan/task、parent plan 第 0-5 节、相关 ledger |
| Status | `[x] complete` |
| Verification | 本轮不运行 test/build/verifier/regression/docs gate |

## 2. Scope contract

- 只核对 server-authoritative state、服务器持久化值、服务器 host business state 和服务端
  规则 Definition。
- 只保留真正影响世界规则、实体生命周期、移动/碰撞/战斗、库存/装备/Buff、WorldObjects、
  生成、权限、会话或恢复的值。
- `host-only` 可以被服务器使用，但不进入 Simulation component。
- derived property 必须优先由唯一 query 计算，不为同一语义制造第二个可写缓存。
- `legacy-reference` 只表示旧来源/Oracle/导入参考，不是新运行时 owner。

## 3. Mandatory field-card columns

每个 P01-P08 工作稿的每个 declaration-level 行必须填写：

| 列 | 要求 |
| --- | --- |
| ID | 稳定且在本子任务内唯一 |
| Legacy name | 保留旧字段/属性的精确拼写和大小写 |
| Source anchor/hash | 文件、行号/声明范围和 source snapshot hash |
| Exact type | 完整 C# 类型，包括数组长度、nullable、static/readonly 形状 |
| Static/instance | `static`、instance、property 或 derived property |
| Default | 声明默认值、隐式零值和语义 reset sentinel 分开记录 |
| Mutability | readonly、mutable、derived、per-tick 或 host-owned |
| Lifetime | bootstrap、tick、entity、world、restart、session 或 host |
| Server meaning | 只写权威业务语义，不写显示/封包语义 |
| Owner | 唯一 Definition、Component、Query、System、Command、Snapshot 或 host owner |
| Persistence | business persistence、derived、transient、host-only 或不保存 |
| Exclusion | Protocol、transport、replication、client/UI/audio/device 等排除原因 |
| Status | `accepted-narrow`、`partial`、`deferred`、`host-only`、`excluded`、`legacy-reference` 或 `not-run` |
| Next action | 下一项具体核对，而不是泛化的“继续迁移” |

## 4. Status rules

- `[ ]` 是当前工作项未完成标记；它不能被已有 owner 名称替换。
- `accepted-narrow` 只代表列明的值域、分支或 registry 证据，不代表旧类全量 parity。
- `partial` 表示已有形状或窄证据，但仍有 consumer、顺序、恢复或边界未闭合。
- `deferred` 表示已确认属于服务器责任，但证据链不足，禁止以同名字段填空。
- `excluded` 表示明确不属于本次服务器字段迁移；排除行仍要保留，防止后续回流。
- `not-run` 表示本轮没有执行验证；不得改写成失败或通过。

## 5. Protocol hard boundary

以下内容永远只能作为 `excluded` 负向核对项出现：

- packet/message DTO、message ID、packet kind、send/receive mode；
- payload length、bit position/mask、byte cursor、reader/writer state、compression、framing；
- replication snapshot、replication cursor、sync cadence、network revision、net spam；
- `SyncNPC`、`SyncProjectile`、`SyncItem`、`SyncPlayer`、`SyncChestItem` 的 wire field；
- `NetworkPlayerSlice`、`PlayerReplicationState`、`ProjectileNetworkUpdateComponent` 等
  只用于复制的 owner；
- message 87 或任何 open/update/delete/request/response 的字段布局。

业务身份不自动排除：`WorldId`、Player handle、NPC entity handle、Projectile entity handle、
Item instance handle、Chest identity、`Projectile.identity`、`NPC.netID` 和业务 revision 仍
可作为运行时/存档语义。`MessageBuffer.whoAmI` 的 wire 表达则单独排除。

## 6. Parallel write-set contract

| 子任务 | 允许写入 | 禁止写入 |
| --- | --- | --- |
| P01-P09 | 各自 `docs/plans/parallel/` 工作稿 | parent plan、其他工作稿、源码、CSV、JSON、manifest |
| P10-P11 | 各自汇合工作稿 | P01-P09 工作稿、parent plan、源码、构建输出 |
| P99 | parent plan 的新增/修订内容 | 任意实现文件和其他迁移文档 |

如果执行者不能保证独立写集，必须串行执行，不得通过共享文件临时合并。

## 7. Ordered actions

- [x] 固化本文件中的字段卡列和状态词。
- [x] 将 Protocol 负向边界提供给 P09，并把身份误报规则提供给 P01-P08。
- [x] 将当前 source/hash 记录规则提供给所有领域任务。
- [x] 完成本文件后，允许 P01-P09 并行启动；不启动任何代码迁移。

### Completion evidence

- Completed: `2026-09-01`。
- Contract source: 本文件第 2-6 节；下游任务通过 `Dependency | P00` 和本文件路径引用该合同。
- Handoff payload: 字段卡列、状态词、Protocol 排除项、业务身份例外和 source/hash 记录规则已
  在本文件中固定，供 P01-P09 使用。
- Boundary check: 本次变更仅更新本文件的任务状态和有序动作勾选；未创建 server field owner，
  未修改源码、parent plan、其他工作稿、manifest 或构建输出。
- Verification status: `not-run`（按提案约束，不运行 test/build/verifier/regression/docs gate）。

## 8. Handoff

交付给 P01-P09 的内容必须包括本文件路径、字段卡列、状态词和 Protocol 排除规则。P00
本身不产生任何 server field owner，不改变总体 `partial`，也不关闭 WorldGen 删除门。
