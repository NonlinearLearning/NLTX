# Version4 权威 20 分区 System 拆分可领取任务

文档 ID：PLAN-2026-09-18-system-decomposition-authoritative-20-partitions
逻辑域：system-decomposition
产物类型：claimable-task
状态：active
任务类型：20 个可并行领取的 System 拆分分析任务
输入总量：20 个分区，2,659 条成员记录
输出区域：`docs/system-decomposition/reports/`

## 1. 任务目的

本任务把 Version4 权威模拟系统的 P01-P20 分区转换为可独立领取的 System 拆分分析任务。
每个分区由一个会话领取，读取本分区成员报告和必要的源码证据，产出一个 System 拆分报告。

本批次只做文档和证据分析，不修改生产源码、项目文件或测试代码，不运行构建和行为测试。
`Complete` 表示本分区报告已经按文档契约交付，不表示 System 已经实现，也不表示迁移行为
已经通过。迁移成功仍需后续真实迁移项目的必需行为测试通过。

权威分区是 Version4 的模拟状态和行为输入，不等于已经确认的 NLTX owner。任何 owner、
提交点、跨分区顺序或公共 API 结论都必须保留证据状态，并在证据不足时标记为
`unknown`、`partial` 或 `crossSubsystemOwner: integration-review`。

## 2. 领取前置材料

每个会话在领取后必须读取：

1. [仓库入口](../../AGENTS.md) 和 [当前进度](../../Context/progress.md)；
2. [System 拆分规则](../../.agents/skills/ecs-system-domain-splitting/system-splitting/SKILL.md)；
3. [权威分区会话规则](../../.agents/skills/version4-authoritative-partition-session/SKILL.md)；
4. [ECS 文件组织约束](../../Context/架构设计/ECS文件组织设计约束.md)；
5. 本文档对应的 P01-P20 专属 prompt；
6. runner 返回的当前分区输入报告；
7. 为解决具体争议所需的 Version4 源码、调用证据、读写证据和生命周期证据。

专属 prompt 和输入 ledger 是检索入口，不是已经确认的 owner、调用闭包或调度证据。旧路径、
旧行号和候选 seam 必须回到实际源码重新核对。

## 3. 领取与结算协议

权威任务只能通过 runner 领取和结算。任务表必须先初始化为独立任务集；任务表副本、状态、
恢复信息和 lock 均由权威 skill 目录管理。不同任务表必须使用不同的 `TaskSetName`，后续
会话使用同一个名称恢复同一任务集。

```powershell
$runner = '.\Build\Tools\Invoke-Version4AuthoritativePartitionSession.ps1'
$taskSetName = 'authoritative-system-decomposition'
$taskTablePath = '.\docs\system-decomposition\authoritative\2026-09-18-system-decomposition-authoritative-20-partition-tasks.md'

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

## 4. 二十个可领取分区

状态不在本文档中手工维护，以 runner 的共享 ledger 为准。表中 `outputReport` 是该分区
唯一允许新建的 System 拆分输出。

| taskId | partitionId | 成员数 | 输入报告 | 专属 prompt | outputReport |
|---|---:|---:|---|---|---|
| AUTH-SYS-P01 | P01 | 113 | `docs/migration/ledgers/authoritative-20-partitions/P01-Liquid-Wiring-Spatial-Death-Teleport.md` | `docs/component-decomposition/review-round-1/design/2026-09-11-version4-authoritative-20-partition-prompts/2026-09-11-version4-P01-liquid-wiring-spatial-death-teleport-public-decomposition.md` | `docs/system-decomposition/reports/2026-09-18-system-decomposition-authoritative-P01-liquid-wiring-spatial-death-teleport.md` |
| AUTH-SYS-P02 | P02 | 98 | `docs/migration/ledgers/authoritative-20-partitions/P02-Leashed-Entity.md` | `docs/component-decomposition/review-round-1/design/2026-09-11-version4-authoritative-20-partition-prompts/2026-09-11-version4-P02-leashed-entity-public-decomposition.md` | `docs/system-decomposition/reports/2026-09-18-system-decomposition-authoritative-P02-leashed-entity.md` |
| AUTH-SYS-P03 | P03 | 163 | `docs/migration/ledgers/authoritative-20-partitions/P03-Mount-Vehicle.md` | `docs/component-decomposition/review-round-1/design/2026-09-11-version4-authoritative-20-partition-prompts/2026-09-11-version4-P03-mount-vehicle-public-decomposition.md` | `docs/system-decomposition/reports/2026-09-18-system-decomposition-authoritative-P03-mount-vehicle.md` |
| AUTH-SYS-P04 | P04 | 112 | `docs/migration/ledgers/authoritative-20-partitions/P04-Player-Lifecycle-Interaction.md` | `docs/component-decomposition/review-round-1/design/2026-09-11-version4-authoritative-20-partition-prompts/2026-09-11-version4-P04-player-lifecycle-interaction-public-decomposition.md` | `docs/system-decomposition/reports/2026-09-18-system-decomposition-authoritative-P04-player-lifecycle-interaction.md` |
| AUTH-SYS-P05 | P05 | 164 | `docs/migration/ledgers/authoritative-20-partitions/P05-Player-Progression-Pets-Minions.md` | `docs/component-decomposition/review-round-1/design/2026-09-11-version4-authoritative-20-partition-prompts/2026-09-11-version4-P05-player-progression-pets-minions-public-decomposition.md` | `docs/system-decomposition/reports/2026-09-18-system-decomposition-authoritative-P05-player-progression-pets-minions.md` |
| AUTH-SYS-P06 | P06 | 254 | `docs/migration/ledgers/authoritative-20-partitions/P06-Player-Combat-Status.md` | `docs/component-decomposition/review-round-1/design/2026-09-11-version4-authoritative-20-partition-prompts/2026-09-11-version4-P06-player-combat-status-public-decomposition.md` | `docs/system-decomposition/reports/2026-09-18-system-decomposition-authoritative-P06-player-combat-status.md` |
| AUTH-SYS-P07 | P07 | 112 | `docs/migration/ledgers/authoritative-20-partitions/P07-Player-Mobility.md` | `docs/component-decomposition/review-round-1/design/2026-09-11-version4-authoritative-20-partition-prompts/2026-09-11-version4-P07-player-mobility-public-decomposition.md` | `docs/system-decomposition/reports/2026-09-18-system-decomposition-authoritative-P07-player-mobility.md` |
| AUTH-SYS-P08 | P08 | 100 | `docs/migration/ledgers/authoritative-20-partitions/P08-Player-Environment-Armor.md` | `docs/component-decomposition/review-round-1/design/2026-09-11-version4-authoritative-20-partition-prompts/2026-09-11-version4-P08-player-environment-armor-public-decomposition.md` | `docs/system-decomposition/reports/2026-09-18-system-decomposition-authoritative-P08-player-environment-armor.md` |
| AUTH-SYS-P09 | P09 | 105 | `docs/migration/ledgers/authoritative-20-partitions/P09-Player-Inventory-Equipment.md` | `docs/component-decomposition/review-round-1/design/2026-09-11-version4-authoritative-20-partition-prompts/2026-09-11-version4-P09-player-inventory-equipment-public-decomposition.md` | `docs/system-decomposition/reports/2026-09-18-system-decomposition-authoritative-P09-player-inventory-equipment.md` |
| AUTH-SYS-P10 | P10 | 118 | `docs/migration/ledgers/authoritative-20-partitions/P10-Player-Input-Control.md` | `docs/component-decomposition/review-round-1/design/2026-09-11-version4-authoritative-20-partition-prompts/2026-09-11-version4-P10-player-input-control-public-decomposition.md` | `docs/system-decomposition/reports/2026-09-18-system-decomposition-authoritative-P10-player-input-control.md` |
| AUTH-SYS-P11 | P11 | 164 | `docs/migration/ledgers/authoritative-20-partitions/P11-Player-Presentation-Derived.md` | `docs/component-decomposition/review-round-1/design/2026-09-11-version4-authoritative-20-partition-prompts/2026-09-11-version4-P11-player-presentation-derived-public-decomposition.md` | `docs/system-decomposition/reports/2026-09-18-system-decomposition-authoritative-P11-player-presentation-derived.md` |
| AUTH-SYS-P12 | P12 | 189 | `docs/migration/ledgers/authoritative-20-partitions/P12-NPC-Combat-Network-Damage.md` | `docs/component-decomposition/review-round-1/design/2026-09-11-version4-authoritative-20-partition-prompts/2026-09-11-version4-P12-npc-combat-network-damage-public-decomposition.md` | `docs/system-decomposition/reports/2026-09-18-system-decomposition-authoritative-P12-npc-combat-network-damage.md` |
| AUTH-SYS-P13 | P13 | 106 | `docs/migration/ledgers/authoritative-20-partitions/P13-NPC-Town-Progression.md` | `docs/component-decomposition/review-round-1/design/2026-09-11-version4-authoritative-20-partition-prompts/2026-09-11-version4-P13-npc-town-progression-public-decomposition.md` | `docs/system-decomposition/reports/2026-09-18-system-decomposition-authoritative-P13-npc-town-progression.md` |
| AUTH-SYS-P14 | P14 | 117 | `docs/migration/ledgers/authoritative-20-partitions/P14-NPC-Spawn-Eligibility.md` | `docs/component-decomposition/review-round-1/design/2026-09-11-version4-authoritative-20-partition-prompts/2026-09-11-version4-P14-npc-spawn-eligibility-public-decomposition.md` | `docs/system-decomposition/reports/2026-09-18-system-decomposition-authoritative-P14-npc-spawn-eligibility.md` |
| AUTH-SYS-P15 | P15 | 126 | `docs/migration/ledgers/authoritative-20-partitions/P15-Projectile.md` | `docs/component-decomposition/review-round-1/design/2026-09-11-version4-authoritative-20-partition-prompts/2026-09-11-version4-P15-projectile-public-decomposition.md` | `docs/system-decomposition/reports/2026-09-18-system-decomposition-authoritative-P15-projectile.md` |
| AUTH-SYS-P16 | P16 | 133 | `docs/migration/ledgers/authoritative-20-partitions/P16-World-Lifecycle-Housing-Metrics.md` | `docs/component-decomposition/review-round-1/design/2026-09-11-version4-authoritative-20-partition-prompts/2026-09-11-version4-P16-world-lifecycle-housing-metrics-public-decomposition.md` | `docs/system-decomposition/reports/2026-09-18-system-decomposition-authoritative-P16-world-lifecycle-housing-metrics.md` |
| AUTH-SYS-P17 | P17 | 146 | `docs/migration/ledgers/authoritative-20-partitions/P17-World-Terrain-Biomes-GenVars.md` | `docs/component-decomposition/review-round-1/design/2026-09-11-version4-authoritative-20-partition-prompts/2026-09-11-version4-P17-world-terrain-biomes-genvars-public-decomposition.md` | `docs/system-decomposition/reports/2026-09-18-system-decomposition-authoritative-P17-world-terrain-biomes-genvars.md` |
| AUTH-SYS-P18 | P18 | 136 | `docs/migration/ledgers/authoritative-20-partitions/P18-World-Seeds-Skyblock-Definitions.md` | `docs/component-decomposition/review-round-1/design/2026-09-11-version4-authoritative-20-partition-prompts/2026-09-11-version4-P18-world-seeds-skyblock-definitions-public-decomposition.md` | `docs/system-decomposition/reports/2026-09-18-system-decomposition-authoritative-P18-world-seeds-skyblock-definitions.md` |
| AUTH-SYS-P19 | P19 | 84 | `docs/migration/ledgers/authoritative-20-partitions/P19-World-Generation-Execution.md` | `docs/component-decomposition/review-round-1/design/2026-09-11-version4-authoritative-20-partition-prompts/2026-09-11-version4-P19-world-generation-execution-public-decomposition.md` | `docs/system-decomposition/reports/2026-09-18-system-decomposition-authoritative-P19-world-generation-execution.md` |
| AUTH-SYS-P20 | P20 | 119 | `docs/migration/ledgers/authoritative-20-partitions/P20-World-Generation-Actions-Shapes.md` | `docs/component-decomposition/review-round-1/design/2026-09-11-version4-authoritative-20-partition-prompts/2026-09-11-version4-P20-world-generation-actions-shapes-public-decomposition.md` | `docs/system-decomposition/reports/2026-09-18-system-decomposition-authoritative-P20-world-generation-actions-shapes.md` |

## 5. 之前拆分资料的读取顺序

领取 Pnn 后，除了表格中的 input ledger 和专属 prompt，还必须读取同一 Pnn 的既有
Component 拆分资料：

```text
docs/component-decomposition/review-round-2/2026-09-11-version4-Pnn-*-component-design.md
docs/component-decomposition/review-round-2/2026-09-11-version4-Pnn-*-component-execution.md
```

P11、P13、P14、P17、P18、P20 的历史文件名中分区编号使用小写 `pnn`；在 Windows
工作树中按分区编号和 `component-design`/`component-execution` 搜索即可。若既有资料缺失、
状态不是 `accepted` 或与源码冲突，必须在报告中记录 `evidence-gap`，以实际源码和调用证据
为准，不能静默补写或把 Component owner 直接提升为 System owner。

建议读取顺序：

1. 当前任务表的 input ledger，确认本会话成员闭包；
2. 当前任务表的专属 prompt，确认分区目标和禁止跨分区复制；
3. 对应的 Component design，提取已有候选状态和边界；
4. 对应的 Component execution，提取已有路径、依赖和验证计划；
5. Version4 实际源码及调用/读写/生命周期证据，确认或推翻上述候选；
6. 只在最后形成 System 概念行为、API 组合和调度 DAG。

## 6. System 拆分报告契约

每个会话只能写表格中自己领取的 `outputReport`。报告必须按以下顺序覆盖：

1. **Scope and evidence**：分区范围、来源版本、读取的源码和证据状态；
2. **Conceptual behavior**：把成员按概念行为、状态不变量和生命周期分组，不按方法数量分组；
3. **Ownership**：候选权威状态、唯一提交者、读者、写者、间接写入和副作用调用闭包；
4. **Boundary decision**：`keep`、`partial` 或 `separate` 的选择，以及拒绝其他方案的理由；
5. **System API**：旧入口到概念行为、新 System API、Adapter、Projection 和组合结果的映射；
6. **Schedule and DAG**：phase、barrier、`must-before`、`must-after`、事件注册和调度入口；
7. **Lifecycle**：创建、激活、更新、结束、销毁、重建、跨世界、重置和异常路径；
8. **Integration handoff**：跨分区类型、ID、快照、事件、网络、存档和共享接口全部标记
   `crossSubsystemOwner: integration-review`，不得在单分区内最终裁决；
9. **Migration behavior contract**：旧行为、新 System 组合、可观察结果、顺序、错误、
   副作用和行为测试映射；本阶段只写验证计划，不执行测试；
10. **Evidence gaps and blocking decisions**：未知、冲突、版本漂移和需要整合会话解决的问题。

报告中的设计状态必须使用 `proposed`，尚未实际验证的字段使用
`verificationStatus: not-run`。不能把已有 Component 设计报告直接当作 System owner 证据。

每份输出报告至少使用以下标题，便于后续整合会话批量读取：

```markdown
# System Decomposition Report: authoritative Pnn
## Scope and Evidence
## Prior Component Decomposition Reconciliation
## Conceptual Behaviors
## State Ownership and Write Closure
## Boundary Decision
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
- [ ] 区分 Version4 事实、当前 NLTX 状态、`proposed` 设计和验证证据。
- [ ] 有源码和调用证据支持关键 owner；无法闭合的入站、动态分派、反射、配置、序列化、
      异常和卸载入口已列为 gap。
- [ ] 有完整读者、写者、间接写入和副作用调用闭包，不能只依据字段声明推断 owner。
- [ ] 有生命周期边界和实际调度 DAG，不能用文件顺序或注册顺序替代调度证据。
- [ ] 有旧入口到概念行为、新 API 组合、Adapter/Projection 的一一映射。
- [ ] 共享候选均标记 `crossSubsystemOwner: integration-review`。
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
`.agents/skills/version4-authoritative-partition-session/tasks/<TaskSetName>/`，不应加入源码控制，也不应手工编辑。
