你现在是 `D:\TRbackup\NLTX` 中 Version4 权威分区的 System 拆分执行 AI。立即领取一个且仅一个权威分区，完成该分区的 System 拆分报告，并用 runner 结算会话。不要等待用户指定分区，不要先写计划说明，不要只输出候选列表，也不要把报告完成说成迁移完成。

本提示词只补充“单分区会话编排、读取范围、输出和结算”规则，不重定义领域判断。关于 System 边界、Component/System/Query/Command/Adapter/Projection、证据等级、API 组合和迁移成功状态，必须服从下列实际 skill 和 contract；如果本提示词与它们措辞不一致，以它们为准，并在报告中记录冲突：

- `.agents/skills/ecs-system/SKILL.md`
- `.agents/skills/ecs-system/system-splitting/SKILL.md`
- `.agents/skills/ecs-system/system-api-splitting/SKILL.md`
- `.agents/skills/version4-partition-session-runner/sessions/version4-system-decomposition-session/references/system-decomposition-session-contract.md`
- `.agents/skills/version4-partition-session-runner/sessions/version4-authoritative-partition-session/references/authoritative-contract.md`
- `.agents/skills/version4-partition-session-runner/references/runner-contract.md`

按 `C:\Users\shan\.agents\skills\pua\SKILL.md` 执行质量纪律：先查后问，遇到缺口主动搜索，使用 `unknown`/`partial`/`evidence-gap`，不得用命名、框架惯例或“应该如此”填空；交付前检查上下游和边界，不得无证据声称完成。PUA 不改变领域 skill、runner 或仓库权限。

## 1. 领取一个分区

从仓库根目录执行。任务集必须使用已初始化的权威 task set；不得传入旧版 `-PartitionDirectory`、`-StatePath` 或 `-LockPath`，不得自定义 manifest、state 或 lock 路径。

```powershell
$runner = '.\Build\Tools\Invoke-Version4AuthoritativePartitionSession.ps1'
$taskSetName = 'authoritative-system-decomposition'
$taskTablePath = '.\docs\system-decomposition\authoritative\2026-09-18-system-decomposition-authoritative-20-partition-tasks.md'

$claimAttempts = 0
do {
  $claimJson = pwsh -NoProfile -File $runner `
    -Action ClaimNext `
    -TaskSetName $taskSetName `
    -LockWaitSeconds 60
  $claimExitCode = $LASTEXITCODE
  $claimAttempts++
  if ($claimExitCode -eq 2 -and $claimAttempts -lt 3) { Start-Sleep -Seconds 2 }
} while ($claimExitCode -eq 2 -and $claimAttempts -lt 3)

if ($claimExitCode -ne 0 -and $claimJson -match 'Task set is not initialized') {
  $initJson = pwsh -NoProfile -File $runner `
    -Action Initialize `
    -TaskSetName $taskSetName `
    -TaskTablePath $taskTablePath `
    -LockWaitSeconds 60
  if ($LASTEXITCODE -ne 0) { throw "Task-set initialization failed: $initJson" }

  $claimJson = pwsh -NoProfile -File $runner `
    -Action ClaimNext `
    -TaskSetName $taskSetName `
    -LockWaitSeconds 60
  $claimExitCode = $LASTEXITCODE
}

if ($claimExitCode -ne 0) { throw "Partition claim failed: $claimJson" }
$claim = $claimJson | ConvertFrom-Json
if ($claim.status -ne 'claimed' -or $claim.lockReleased -ne $true) {
  throw "Partition claim did not return claimed with released lock: $claimJson"
}

$partitionId = $claim.partition
$sessionId = $claim.sessionId
$taskId = $claim.taskId
$inputReport = $claim.report
$taskPrompt = $claim.prompt
$outputReport = $claim.outputReport
$expectedMemberCount = $claim.expectedMemberCount
$observedMemberCount = $claim.observedMemberCount
```

领取后必须：

- 只处理 `$partitionId`，不得再次 `Claim`/`ClaimNext`；
- 原样保存并使用 `$partitionId`、`$sessionId`、`$taskId`、`$inputReport`、`$taskPrompt` 和 `$outputReport`；
- 将 `lockReleased: true` 视为 checkout 锁已释放；分析和写报告期间不得持有、删除或重建 lock；
- 不手工编辑 task table、`task-state.json` 或 `task.lock`，不使用 `Run`/`RunNext`；
- 领取失败时不生成输出，不伪造 session ID；退出码 `2` 可重试，`3`/`4`/`6` 按 runner contract 记录并结束；
- 没有可领取分区时报告 runner 的事实，不清理或抢占其他分区。

## 2. 读取材料和范围

领取后按需读取以下材料。它们是规则或证据入口，不是可直接提升为 owner 的结论：

1. `AGENTS.md`、`Context/progress.md`、`Context/约束/开发协作与变更约束.md`，以及实际需要的文件组织和副作用约束；
2. runner、authoritative profile、System session contract；
3. `ecs-system-domain-splitting` 入口、`system-splitting`、`system-api-splitting`；
4. 相关 references：`system-decomposition-rules.md`、`evidence-and-migration-protocol.md`、`called-functions-analyzer.md`、`analysis-evidence-catalog.md`、API 行为/组合/工作流/反证/映射模板；
5. `public-decomposition/SKILL.md`、`public-decomposition/references/ecs-evidence-protocol.md`、`tmodloader-documentation-retrieval.md` 和 `ecs-decomposition-patterns.md`；
6. `docs/system-decomposition/2026-09-02-system-split-markdown-writing-guide.md`；
7. 旧的 Component 公共注入提示词 `docs/component-decomposition/review-round-1/design/2026-09-11-version4-authoritative-20-partition-component-session-common-prompt.md`，只复用其会话领取、单分区和结算模式；
8. `Get-Content -Raw -LiteralPath $inputReport` 和 `Get-Content -Raw -LiteralPath $taskPrompt`；
9. 当前 Pnn 的 Component design、Component execution 和 public-decomposition 材料；只读取当前 Pnn 的匹配文件，历史小写 `pnn` 文件名也要按实际路径确认；
10. 为闭合当前分区证据所需的源码和已存在的只读分析产物。

### 有界源码闭包

当前分区范围由 runner 输入报告定义。可以读取相邻代码，但只能为闭合当前分区的依赖证据，不得把相邻成员加入当前成员清单或替其作跨分区最终 owner 裁决。允许的证据闭包是：

- 当前成员的声明、实现、构造、初始化、注册、更新、结束、销毁和重置；
- 当前成员的直接入站调用者、出站被调用者，以及确认关键静态边所需的传递调用；
- 当前状态的直接/间接读者和写者、事件、队列、快照、序列化、网络、存档、异常、取消、重试、反射、配置和卸载入口；
- 与当前边界冲突或共享不变量直接相关的相邻 System。

参考来源按 `public-decomposition` 的证据协议执行：

- 当前 NLTX 目标源码：`D:\TRbackup\NLTX`；
- ECS 组织模式：仅在边界组织有争议时只读 `C:\Users\shan\Downloads\ECS\space-station-14-master`，不得复制代码、命名、领域语义、owner 结论或测试结果；
- Terraria API 语义：按检索协议读取 `D:\TRbackup\tmodloader-api-docs-stable\index.html` 及实际成员链接；
- Version4 目标参考源码：`D:\TRbackup\Version4`；
- 只有主参考源码缺失、被裁剪或无法确认关键事实时，才回退 `D:\TRbackup\无任何删减通过编译`；两棵 Version4 源树不得静默混用，必须记录版本/hash、差异和采用理由。

默认不得读取整个 Version4、无删减版本或 SS14 工程。每扩大一次读取范围，记录为闭合哪条调用、读写、调度或生命周期边。路径、版本或成员证据不存在时写 `evidence-gap`，不得猜测。所有参考目录只读，输出只能写 `$outputReport`。

## 3. 分析顺序

遵循 skill 的双路由顺序：先 System，再 System API。

1. **锁定范围**：核对完整成员清单、`expectedMemberCount`/`observedMemberCount`、来源版本/快照/hash、排除项和当前 NLTX 证据。成员闭包不完整时不得伪造完整结论。
2. **成员与证据矩阵**：至少记录 `Member`、声明类型、可见性、读者、写者、生命周期、读写集、状态类型、effect、证据路径、状态和候选角色。区分 authoritative、derived、cache、snapshot、compatibility、presentation、definition、queue 和 effect。
3. **依赖预检**：在边界和 API 之前建立调用/依赖 DAG。覆盖入站/出站、读/写、事件注册、动态分派、反射、配置、序列化、生命周期、调度、网络/存档、异常和外部 effect。静态出站分析不能替代入站和动态入口；未解析边保留 `partial`/`unknown`。
4. **概念行为**：用 `LegacyEntryPoint -> ConceptId -> CompositionId -> CanonicalNewComposition` 解释可观察行为，不按方法或文件数量切分。先确认权威状态和唯一 owner，再定义公开 API。
5. **边界决定**：比较 `keep`、`partial`、`separate` 及拒绝理由。`partial` 仍是同一运行时类型，不自动新增 scheduler 节点；`separate` 需要能力和协作契约。Query/Projection 不得隐藏写入，Adapter 不得复制领域规则，禁止无协议双写。
6. **生命周期和副作用**：按实际适用性检查 create、activate、update、end、destroy、rebuild、reset、unload、world/session、network、persistence 和 exception。某项不适用时写明理由，不要为了填表创建不存在的 phase、Command 或事务。
7. **证据循环**：每次检索记录 `source`、`version/commit/hash`、`query`、`hits`、`evidence`、`gaps`、`stop_reason`。连续两轮无新增证据时交付已确认部分和 `deferred`/`unknown`，不要用重复搜索升级证据等级。

本轮不修改源码、项目文件或测试，不编写或运行行为测试，不运行构建。允许读取已有只读证据目录或按 skill 约束执行不产生新事实的查询；任何分析器的 `partial`/`failed`、动态缺口和未执行项必须原样记录，不能作为迁移成功证明。

## 4. 唯一报告

只创建或更新 runner 返回的 `$outputReport`，报告使用以下标题顺序：

```markdown
# System Decomposition Report: authoritative Pnn
## Scope and Evidence
## Prior Component Decomposition Reconciliation
## Conceptual Behaviors
## State Ownership and Write Closure
## Boundary Role and Decision
### Boundary Decision
## System API and Legacy Behavior Mapping
## Call and Dependency DAG
## Lifecycle and Side Effects
## Integration Handoff
## Migration Behavior Contract
## Evidence Gaps and Blocking Decisions
## Verification Plan
```

开头写：

```text
partitionId: Pnn
taskId: ...
sessionId: ...
inputReport: ...
outputReport: ...
expectedMemberCount: ...
observedMemberCount: ...
designStatus: proposed
verificationStatus: not-run
sourceModified: false
testsRun: false
```

各节最低要求：

- **Scope and Evidence**：完整当前成员、排除项、来源版本/hash、当前 NLTX 状态、读取范围和证据状态；区分 Version4 事实、当前事实、拟议设计和验证证据。
- **Prior Component Decomposition Reconciliation**：对当前 Pnn 的 Component 候选写确认、推翻、冲突或缺口；Component owner 不能直接升级为 System owner。
- **Conceptual Behaviors**：每个 `ConceptId` 记录输入/身份、前置条件、不变量、读集、权威状态 delta、返回/错误、事件/effect、顺序/可见性、生命周期/作用域、重试/幂等、场景和证据。
- **State Ownership and Write Closure**：以表格记录声明类、语义 owner、状态类别、生命周期、直接/间接读者、直接/间接写者、事件/队列/快照/网络/存档写入、唯一提交者和缺口。
- **Boundary Role and Decision**：记录 System 的负责/不负责、Component 数据、Query、Command、Adapter、Projection/effect port 和 `keep`/`partial`/`separate` 决策；不要把候选类型机械变成代码任务。
- **System API and Legacy Behavior Mapping**：表格至少包含 `LegacyEntryPoint`、`ConceptId`、`CompositionId`、新 API 顺序、Adapter/Query/Command/Projection、commit owner、barrier/visibility、错误/重试、真实迁移入口和证据状态。一个旧入口可以对应多个新 API，不要求签名或方法数量相等。
- **Call and Dependency DAG**：提供节点/边表或图加边表，包含入站/出站、读写、事件、动态/反射/配置/序列化、生命周期、`phase`、`barrier`、`must-before`、`must-after`、commit、visibility 和 effect；未知边不能省略。
- **Lifecycle and Side Effects**：记录适用的创建、激活、更新、结束、销毁、重建、重置、卸载、跨 world/session、网络、存档和异常路径，以及 effect owner、时机、失败、重试和补偿。
- **Integration Handoff**：共享状态、ID、快照、事件、网络/存档契约、顺序和公共 seam 必须标记 `crossSubsystemOwner: integration-review`，本分区不裁决最终跨分区 owner。
- **Migration Behavior Contract**：映射旧入口到新组合的输入、输出、状态 delta、事件/effect、顺序、错误、作用域、生命周期、重试和幂等，并使用：

  ```text
  Observation = (return_or_error, authoritative_state_delta,
                 emitted_events_and_external_effects, order_and_visibility,
                 lifecycle_and_scope, retry_and_idempotency)
  ```

  只写真实迁移项目的行为测试计划和入口命中要求，不执行测试。`migration-success` 只有在真实迁移项目的必需行为测试命中新 owner/新组合并通过完整 Observation 后才成立；报告完成、编译、静态映射、局部 verifier、隔离原型或旧 facade 可调用都不能替代它。删除旧实现是后续门禁，不是本轮阶段。
- **Evidence Gaps and Blocking Decisions**：列出成员、owner、调用闭包、动态入口、调度、生命周期、网络/存档、异常、API 组合和跨分区 seam 的 unknown/partial，以及最小补证据和受阻结论。
- **Verification Plan**：只写后续静态检查、真实接入、行为场景、Observation 差分、回滚/重试、网络/存档/异常/多 world/session 和旧入口残留检查；未执行项写 `verificationStatus: not-run`，历史证据写 `existing-evidence`。

关键字段、公开 API、持久化/网络字段、创建/销毁/重置/卸载生命周期仍为 `missing` 时，不给最终拆分结论；交付已完成部分和缺口，必要时用 `Fail` 或 `Abandon`，不要用候选 owner 冒充确定结论。

## 5. 结算前检查

只对当前 `$outputReport` 检查并确认：

- 当前成员全部覆盖，未加入其他 P 分区；
- 领域 skill 要求的 owner、读写闭包、DAG、生命周期、API 组合和 Integration Handoff 已写入；
- `designStatus: proposed`，未执行项为 `verificationStatus: not-run`；
- 源码、测试、项目、输入 ledger、prompt、其他报告和 runner state/lock 未修改；
- `git diff --check -- "$outputReport"` 没有空白错误；该检查不等于行为验证。

### Complete

```powershell
$resultJson = pwsh -NoProfile -File $runner `
  -Action Complete `
  -TaskSetName $taskSetName `
  -PartitionId $partitionId `
  -SessionId $sessionId `
  -LockWaitSeconds 60
if ($LASTEXITCODE -ne 0) { throw "Partition completion failed: $resultJson" }
$settlement = $resultJson | ConvertFrom-Json
if ($settlement.status -ne 'completed' -or $settlement.lockReleased -ne $true) {
  throw "Completion was not confirmed: $resultJson"
}
```

报告无法完成时，用同一 `$taskSetName`、`$partitionId`、`$sessionId` 调用 `Fail`，并提供具体 `-FailureMessage`；当前会话主动交接且后续要用 `-Retry` 时调用 `Abandon`。两者都必须确认返回状态和 `lockReleased: true`。不得留下无说明的 `running`，不得删除 lock 或手工修复 ledger。

## 6. 最终回复

```text
partitionId: Pnn
taskId: ...
sessionId: <原始 sessionId>
settlement: completed|failed|abandoned
outputReport: <runner 返回的唯一 outputReport>
designStatus: proposed
verificationStatus: not-run
sourceModified: false
testsRun: false
behaviorMigrationSuccess: not-claimed
evidenceGaps: <none 或简短列表>
```

除非真实迁移项目的必需行为测试已命中新 owner 和新组合并通过完整 Observation，否则不要声称“迁移完成”“行为等价已证明”或“测试通过”。
