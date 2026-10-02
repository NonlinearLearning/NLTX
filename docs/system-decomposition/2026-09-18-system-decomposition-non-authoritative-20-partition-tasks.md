# Version4 非权威 16 分区 System 拆分可领取任务

文档 ID：PLAN-2026-09-18-system-decomposition-non-authoritative-16-partitions
逻辑域：system-decomposition
产物类型：claimable-task
状态：active
任务类型：16 个可并行领取的 System 拆分分析任务
输入总量：16 个分区，3,415 条成员记录
输出区域：`docs/system-decomposition/reports/`

## 1. 任务目的

本任务把 Version4 非权威模拟和运行时支撑范围的 P01-P15、P18 分区转换为可独立领取的
System 拆分分析任务。每个分区由一个会话领取，读取本分区成员报告、之前已经完成的
子系统/Component 拆分文档和必要的源码证据，产出一个 System 拆分报告。

本批次只做文档和证据分析，不修改生产源码、项目文件或测试代码，不运行构建和行为测试。
`Complete` 表示本分区 System 报告已经按文档契约交付，不表示非权威运行时已经实现，也不
表示权威模拟行为或迁移行为已经通过。迁移成功仍需后续真实迁移项目的必需行为测试通过。

非权威分区是网络、持久化、UI、外部平台、表现、诊断和共享运行时等边界的输入。它们不能
仅凭成员清单取得权威模拟状态的 owner。涉及权威状态、实体 ID、快照、恢复、事件顺序或
跨分区 API 的结论必须标记为 `crossSubsystemOwner: integration-review`，并交给整合会话裁决。

## 2. 领取前置材料

每个会话在领取后必须读取：

1. [仓库入口](../../../AGENTS.md) 和 [当前进度](../../../Context/progress.md)；
2. [System 拆分规则](../../../.agents/skills/ecs-system/system-splitting/SKILL.md)；
3. [非权威分区会话规则](../../../.agents/skills/version4-partition-session-runner/sessions/version4-non-authoritative-partition-session/SKILL.md)；
4. [ECS 文件组织约束](../../../Context/架构设计/ECS文件组织设计约束.md)；
5. 本文档对应的 P01-P15、P18 专属 prompt；
6. runner 返回的当前分区输入报告；
7. 对应分区已经存在的 Component 设计/执行文档和第一轮 public-decomposition 文档；
8. 为解决具体争议所需的 Version4 源码、调用证据、读写证据和生命周期证据。

已有子系统和 Component 文档只提供候选边界与调查线索，不能直接当作当前 NLTX owner、
调用闭包、调度 DAG 或行为等价证据。旧路径、旧行号和候选 seam 必须回到实际源码重新核对。

## 3. 领取与结算协议

非权威任务只能通过 runner 领取和结算。任务表必须先初始化为独立任务集；任务表副本、状态、
恢复信息和 lock 均由非权威 skill 目录管理。不同任务表必须使用不同的 `TaskSetName`，后续
会话使用同一个名称恢复同一任务集。

```powershell
$runner = '.\Build\Tools\Invoke-Version4NonAuthoritativePartitionSession.ps1'
$taskSetName = 'non-authoritative-system-decomposition-16'
$taskTablePath = '.\docs\system-decomposition\non-authoritative\2026-09-18-system-decomposition-non-authoritative-20-partition-tasks.md'

pwsh -NoProfile -File $runner `
  -Action Initialize `
  -TaskSetName $taskSetName `
  -TaskTablePath $taskTablePath `
  -LockWaitSeconds 60

$claimJson = pwsh -NoProfile -File $runner `
  -Action ClaimNext `
  -TaskSetName $taskSetName `
  -LockWaitSeconds 60
if ($LASTEXITCODE -ne 0) { throw $claimJson }

$claim = $claimJson | ConvertFrom-Json
if ($claim.status -ne 'claimed' -or $claim.lockReleased -ne $true) {
  throw "Partition claim failed: $claimJson"
}

$partitionId = $claim.partition
$sessionId = $claim.sessionId
$inputReport = $claim.report
```

正常并行流程使用 `ClaimNext`。只有用户明确分配某个分区时，才使用：

```powershell
pwsh -NoProfile -File $runner `
  -Action Claim `
  -PartitionId Pnn `
  -TaskSetName $taskSetName `
  -LockWaitSeconds 60
```

领取成功后 checkout lock 已释放。分析、读取源码和写报告期间不得持有、删除或重建 lock。
一个会话只能领取一个分区；不得通过手工修改 JSON ledger 绕过重复领取。

完成报告并通过本文档的检查清单后，使用领取返回的同一个 `sessionId`：

```powershell
pwsh -NoProfile -File $runner `
  -Action Complete `
  -PartitionId $partitionId `
  -SessionId $sessionId `
  -TaskSetName $taskSetName `
  -LockWaitSeconds 60
```

无法完成时使用 `Fail` 或 `Abandon`，并写明证据缺口。只有确认旧会话不会继续写入时，才可
使用相同 `-TaskSetName $taskSetName` 执行 `Cleanup -PartitionId Pnn` 重新开放分区。不得使用
`Run` 或 `RunNext` 替代手工文档会话。此任务集不能使用 Component 拆分任务的默认 state/lock。
初始化后恢复任务只需再次传入 `-TaskSetName $taskSetName`；若任务表内容改变，应创建新的
任务集名称。

## 4. 当前纳入的十六个可领取分区

状态不在本文档中手工维护，以 runner 的共享 ledger 为准。表中 `outputReport` 是该分区
唯一允许新建的 System 拆分输出。

| taskId | partitionId | 成员数 | 输入报告 | 专属 prompt | outputReport |
|---|---:|---:|---|---|---|
| NONAUTH-SYS-P01 | P01 | 149 | `docs/migration/ledgers/non-authoritative-component-partitions/01-world-session-runtime.md` | `docs/component-decomposition/review-round-1/design/2026-09-11-version4-non-authoritative-20-partition-prompts/2026-09-11-version4-non-authoritative-P01-world-session-runtime-public-decomposition.md` | `docs/system-decomposition/reports/2026-09-18-system-decomposition-non-authoritative-P01-world-session-runtime.md` |
| NONAUTH-SYS-P02 | P02 | 225 | `docs/migration/ledgers/non-authoritative-component-partitions/02-world-environment-events.md` | `docs/component-decomposition/review-round-1/design/2026-09-11-version4-non-authoritative-20-partition-prompts/2026-09-11-version4-non-authoritative-P02-world-environment-events-public-decomposition.md` | `docs/system-decomposition/reports/2026-09-18-system-decomposition-non-authoritative-P02-world-environment-events.md` |
| NONAUTH-SYS-P03 | P03 | 485 | `docs/migration/ledgers/non-authoritative-component-partitions/03-world-generation-dungeons.md` | `docs/component-decomposition/review-round-1/design/2026-09-11-version4-non-authoritative-20-partition-prompts/2026-09-11-version4-non-authoritative-P03-world-generation-dungeons-public-decomposition.md` | `docs/system-decomposition/reports/2026-09-18-system-decomposition-non-authoritative-P03-world-generation-dungeons.md` |
| NONAUTH-SYS-P04 | P04 | 320 | `docs/migration/ledgers/non-authoritative-component-partitions/04-world-tiles-storage.md` | `docs/component-decomposition/review-round-1/design/2026-09-11-version4-non-authoritative-20-partition-prompts/2026-09-11-version4-non-authoritative-P04-world-tiles-storage-public-decomposition.md` | `docs/system-decomposition/reports/2026-09-18-system-decomposition-non-authoritative-P04-world-tiles-storage.md` |
| NONAUTH-SYS-P05 | P05 | 119 | `docs/migration/ledgers/non-authoritative-component-partitions/05-spatial-motion-physics.md` | `docs/component-decomposition/review-round-1/design/2026-09-11-version4-non-authoritative-20-partition-prompts/2026-09-11-version4-non-authoritative-P05-spatial-motion-physics-public-decomposition.md` | `docs/system-decomposition/reports/2026-09-18-system-decomposition-non-authoritative-P05-spatial-motion-physics.md` |
| NONAUTH-SYS-P06 | P06 | 44 | `docs/migration/ledgers/non-authoritative-component-partitions/06-entity-lifecycle-attribution.md` | `docs/component-decomposition/review-round-1/design/2026-09-11-version4-non-authoritative-20-partition-prompts/2026-09-11-version4-non-authoritative-P06-entity-lifecycle-attribution-public-decomposition.md` | `docs/system-decomposition/reports/2026-09-18-system-decomposition-non-authoritative-P06-entity-lifecycle-attribution.md` |
| NONAUTH-SYS-P07 | P07 | 97 | `docs/migration/ledgers/non-authoritative-component-partitions/07-combat-status-effects.md` | `docs/component-decomposition/review-round-1/design/2026-09-11-version4-non-authoritative-20-partition-prompts/2026-09-11-version4-non-authoritative-P07-combat-status-effects-public-decomposition.md` | `docs/system-decomposition/reports/2026-09-18-system-decomposition-non-authoritative-P07-combat-status-effects.md` |
| NONAUTH-SYS-P08 | P08 | 283 | `docs/migration/ledgers/non-authoritative-component-partitions/08-player-input-gameplay.md` | `docs/component-decomposition/review-round-1/design/2026-09-11-version4-non-authoritative-20-partition-prompts/2026-09-11-version4-non-authoritative-P08-player-input-gameplay-public-decomposition.md` | `docs/system-decomposition/reports/2026-09-18-system-decomposition-non-authoritative-P08-player-input-gameplay.md` |
| NONAUTH-SYS-P09 | P09 | 191 | `docs/migration/ledgers/non-authoritative-component-partitions/09-item-inventory-containers.md` | `docs/component-decomposition/review-round-1/design/2026-09-11-version4-non-authoritative-20-partition-prompts/2026-09-11-version4-non-authoritative-P09-item-inventory-containers-public-decomposition.md` | `docs/system-decomposition/reports/2026-09-18-system-decomposition-non-authoritative-P09-item-inventory-containers.md` |
| NONAUTH-SYS-P10 | P10 | 321 | `docs/migration/ledgers/non-authoritative-component-partitions/10-economy-crafting-fishing-loot.md` | `docs/component-decomposition/review-round-1/design/2026-09-11-version4-non-authoritative-20-partition-prompts/2026-09-11-version4-non-authoritative-P10-economy-crafting-fishing-loot-public-decomposition.md` | `docs/system-decomposition/reports/2026-09-18-system-decomposition-non-authoritative-P10-economy-crafting-fishing-loot.md` |
| NONAUTH-SYS-P11 | P11 | 93 | `docs/migration/ledgers/non-authoritative-component-partitions/11-npc-town-bestiary.md` | `docs/component-decomposition/review-round-1/design/2026-09-11-version4-non-authoritative-20-partition-prompts/2026-09-11-version4-non-authoritative-P11-npc-town-bestiary-public-decomposition.md` | `docs/system-decomposition/reports/2026-09-18-system-decomposition-non-authoritative-P11-npc-town-bestiary.md` |
| NONAUTH-SYS-P12 | P12 | 285 | `docs/migration/ledgers/non-authoritative-component-partitions/12-content-definitions-catalogs.md` | `docs/component-decomposition/review-round-1/design/2026-09-11-version4-non-authoritative-20-partition-prompts/2026-09-11-version4-non-authoritative-P12-content-definitions-catalogs-public-decomposition.md` | `docs/system-decomposition/reports/2026-09-18-system-decomposition-non-authoritative-P12-content-definitions-catalogs.md` |
| NONAUTH-SYS-P13 | P13 | 253 | `docs/migration/ledgers/non-authoritative-component-partitions/13-network-protocol-session.md` | `docs/component-decomposition/review-round-1/design/2026-09-11-version4-non-authoritative-20-partition-prompts/2026-09-11-version4-non-authoritative-P13-network-protocol-session-public-decomposition.md` | `docs/system-decomposition/reports/2026-09-18-system-decomposition-non-authoritative-P13-network-protocol-session.md` |
| NONAUTH-SYS-P14 | P14 | 160 | `docs/migration/ledgers/non-authoritative-component-partitions/14-persistence-recovery-configuration.md` | `docs/component-decomposition/review-round-1/design/2026-09-11-version4-non-authoritative-20-partition-prompts/2026-09-11-version4-non-authoritative-P14-persistence-recovery-configuration-public-decomposition.md` | `docs/system-decomposition/reports/2026-09-18-system-decomposition-non-authoritative-P14-persistence-recovery-configuration.md` |
| NONAUTH-SYS-P15 | P15 | 59 | `docs/migration/ledgers/non-authoritative-component-partitions/15-external-platform-boundaries.md` | `docs/component-decomposition/review-round-1/design/2026-09-11-version4-non-authoritative-20-partition-prompts/2026-09-11-version4-non-authoritative-P15-external-platform-boundaries-public-decomposition.md` | `docs/system-decomposition/reports/2026-09-18-system-decomposition-non-authoritative-P15-external-platform-boundaries.md` |
| NONAUTH-SYS-P18 | P18 | 331 | `docs/migration/ledgers/non-authoritative-component-partitions/18-map-camera-rendering.md` | `docs/component-decomposition/review-round-1/design/2026-09-11-version4-non-authoritative-20-partition-prompts/2026-09-11-version4-non-authoritative-P18-map-camera-rendering-public-decomposition.md` | `docs/system-decomposition/reports/2026-09-18-system-decomposition-non-authoritative-P18-map-camera-rendering.md` |

## 5. 之前拆分资料的读取顺序

领取 Pnn 后，除了表格中的 input ledger 和专属 prompt，还必须读取同一 Pnn 的既有
Component 拆分资料：

```text
docs/component-decomposition/review-round-2/non-authoritative/2026-09-11-version4-non-authoritative-Pnn-*-component-design.md
docs/component-decomposition/review-round-2/non-authoritative/2026-09-11-version4-non-authoritative-Pnn-*-component-execution.md
```

如果既有资料缺失、状态不是 `accepted` 或与源码冲突，必须在报告中记录 `evidence-gap`，
以实际源码和调用证据为准，不能静默补写或把非权威 Component owner 直接提升为权威
System owner。网络、存档、UI、表现和外部平台文档尤其要区分输出投影、Adapter 和权威
状态写入。

建议读取顺序：

1. 当前任务表的 input ledger，确认本会话成员闭包；
2. 当前任务表的专属 prompt，确认分区目标和禁止跨分区复制；
3. 对应的 Component design，提取已有候选状态、投影和边界；
4. 对应的 Component execution，提取已有路径、依赖和验证计划；
5. Version4 实际源码及调用/读写/生命周期证据，确认或推翻上述候选；
6. 只在最后形成 System 概念行为、API 组合和调度 DAG。

## 6. System 拆分报告契约

每个会话只能写表格中自己领取的 `outputReport`。报告必须按以下顺序覆盖：

1. **Scope and evidence**：分区范围、来源版本、读取的源码、既有 Component/子系统文档和证据状态；
2. **Conceptual behavior**：把成员按可观察行为、状态不变量和生命周期分组，不按方法数量分组；
3. **Boundary role**：明确这是 Adapter、Projection、Query、Command、后台任务、表现 System、
   网络 System、持久化 System 或其他候选角色，并说明是否允许触碰权威状态；
4. **Ownership**：记录读者、写者、间接写入、事件订阅、外部副作用和唯一提交端口；
5. **Boundary decision**：`keep`、`partial` 或 `separate` 的选择，以及拒绝其他方案的理由；
6. **System API**：旧入口到概念行为、新 System API、Adapter、Projection 和组合结果的映射；
7. **Schedule and DAG**：phase、barrier、`must-before`、`must-after`、事件注册、重试和卸载入口；
8. **Lifecycle**：创建、激活、更新、结束、销毁、重建、跨世界、重置、断线、恢复和异常路径；
9. **Integration handoff**：跨分区类型、ID、快照、事件、网络、存档、UI 和共享接口全部标记
   `crossSubsystemOwner: integration-review`，不得在单分区内最终裁决；
10. **Migration behavior contract**：旧行为、新 System 组合、可观察结果、顺序、错误、
    副作用和行为测试映射；本阶段只写验证计划，不执行测试；
11. **Evidence gaps and blocking decisions**：未知、冲突、版本漂移和需要整合会话解决的问题。

报告中的设计状态必须使用 `proposed`，尚未实际验证的字段使用
`verificationStatus: not-run`。非权威 Component 设计报告不能被改写成权威 System owner 结论。

每份输出报告至少使用以下标题，便于后续整合会话批量读取：

```markdown
# System Decomposition Report: non-authoritative Pnn
## Scope and Evidence
## Prior Component Decomposition Reconciliation
## Conceptual Behaviors
## State Ownership and Write Closure
## Boundary Role and Decision
## System API and Legacy Behavior Mapping
## Call and Dependency DAG
## Lifecycle and Side Effects
## Integration Handoff
## Migration Behavior Contract
## Evidence Gaps and Blocking Decisions
## Verification Plan
```

## 7. 完成检查清单

- [ ] 只读取并覆盖领取分区的全部成员，没有复制其他 P 分区清单。
- [ ] 读取并引用对应的既有子系统拆分/Component 设计和执行文档，但重新核对关键源码事实。
- [ ] 区分 Version4 事实、当前 NLTX 状态、`proposed` 设计和验证证据。
- [ ] 有完整读者、写者、间接写入和副作用调用闭包，不能只依据字段声明或投影名称推断 owner。
- [ ] 外部平台、UI、网络、存档、表现和诊断边界已明确 Adapter/Projection 与权威状态的方向。
- [ ] 有生命周期边界和实际调度 DAG，不能用文件顺序或注册顺序替代调度证据。
- [ ] 有旧入口到概念行为、新 API 组合、Adapter/Projection 的一一映射。
- [ ] 共享候选、权威状态和跨分区顺序均标记 `crossSubsystemOwner: integration-review`。
- [ ] 没有修改 `src/`、`Test/`、项目文件、输入 ledger、prompt 或其他分区输出。
- [ ] 未运行构建、测试或行为 verifier 时，报告明确写 `verificationStatus: not-run`。
- [ ] 使用领取返回的原始 `sessionId` 调用 `Complete`、`Fail` 或 `Abandon`。

## 8. 结算状态含义

| runner 状态 | 含义 |
|---|---|
| `available` | 可以被 `ClaimNext` 或显式 `Claim` 领取 |
| `running` | 已被某个 session 领取；不能被其他 session 重复领取 |
| `completed` | 文档任务已结算；不是迁移完成，也不是行为通过 |
| `failed` | 会话遇到明确失败，保留失败原因 |
| `abandoned` | 会话中断或主动放弃，可经确认后用 `-Retry` 重新领取 |

runner 的 JSON 状态和 lock 位于
`.agents/skills/version4-partition-session-runner/sessions/version4-non-authoritative-partition-session/tasks/<TaskSetName>/`，不应加入源码控制，也不应手工编辑。
