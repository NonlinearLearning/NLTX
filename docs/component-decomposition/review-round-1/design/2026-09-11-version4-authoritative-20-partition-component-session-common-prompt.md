# Version4 权威 20 分区组件设计与执行公共提示词

> 用法：将本文件全文作为公共前置提示词，再附加一个 P01-P20 专属任务提示词。每个并行会话都必须读取本文件和自己的分区提示词后执行。专属任务提示词中的 `partitionId`、输入报告、成员范围和研究报告路径优先于本文件中的示例。

## 0. 会话身份与目标

你是当前分区的唯一执行会话。你的任务是：领取一个尚未领取的 Version4 权威分区，读取该分区的完整成员报告和真实源码证据，产出两份 Markdown：

1. 组件拆分设计文档 `component-design.md`；
2. 组件执行文档 `component-execution.md`。

本轮是 20 个独立分区并行工作。组件设计必须按领域、共同读写者、变更原因、生命周期和语义内聚度分组；组件执行文档必须把设计转成可审查的实施顺序、文件组织、依赖影响和验证计划。当前会话不得处理其他分区成员。

本公共提示词与分区 runner skill 的锁语义是硬约束：锁只保护“领取事务”和“完成/失败结算事务”，领取成功写入 `running` 后立即释放；分析、写 Markdown 和验证计划期间不持有 checkout 锁。不同分区可以并行，同一分区不能重复领取。

## 1. 必读材料

从仓库根目录 `D:\TRbackup\NLTX` 开始，依次实际读取：

1. `AGENTS.md`；
2. `Context/progress.md`；
3. `D:\TRbackup\NLTX\.agents\skills\version4-authoritative-partition-session\SKILL.md`；
4. `D:\TRbackup\NLTX\.agents\skills\public-decomposition\SKILL.md`；
5. `D:\TRbackup\NLTX\.agents\skills\public-decomposition\references\ecs-evidence-protocol.md`；
6. `D:\TRbackup\NLTX\.agents\skills\public-decomposition\references\tmodloader-documentation-retrieval.md`；
7. `D:\TRbackup\NLTX\Context/架构设计\ECS文件组织设计约束.md`；
8. `D:\TRbackup\NLTX\Context/约束\Google-CSharp-Style-Guide-约束.md`；
9. `D:\TRbackup\NLTX\Context/约束\非函数式编码副作用隔离规范.md`；
10. 本公共提示词；
11. 一个且仅一个 P01-P20 专属任务提示词；
12. runner 返回的 `VERSION4_PARTITION_REPORT_PATH` 对应的输入分区报告。

专属任务提示词中列出的 Version4、完整参考源码、tModLoader 文档和 ECS 参考路径是证据检索线索，必须重新定位真实文件、类型、成员、调用者、读者、写者、生命周期和副作用。不得把专属任务提示词中的旧行号当作已验证证据。

## 2. 领取任务：只用 runner

任务领取中心是：

```text
D:\TRbackup\NLTX\Build\Tools\Invoke-Version4AuthoritativePartitionSession.ps1
```

所有会话必须使用已经初始化的任务集。`$taskSetName` 由上层任务表或会话启动上下文提供；
不得省略 `-TaskSetName`，也不得通过固定 manifest、调用方目录或调用方 state/lock 路径运行
runner。

所有并行会话必须按 P01-P20 顺序自动领取下一个可用分区。先执行下面的唯一领取命令；不要先用 `List` 决定分区，也不要用 `Run`/`RunNext` 代替手工文档会话：

```powershell
$claimJson = pwsh -NoProfile -File .\Build\Tools\Invoke-Version4AuthoritativePartitionSession.ps1 `
  -Action ClaimNext `
  -TaskSetName $taskSetName `
  -LockWaitSeconds 60
if ($LASTEXITCODE -ne 0) {
  throw "Partition claim failed with exit code $LASTEXITCODE: $claimJson"
}
$claim = $claimJson | ConvertFrom-Json
if ($claim.status -ne 'claimed') {
  throw "Partition claim did not return claimed: $claimJson"
}
$partitionId = $claim.partition
$sessionId = $claim.sessionId
$inputReport = $claim.report
if ($claim.lockReleased -ne $true) {
  throw "Partition claim did not report immediate lock release: $claimJson"
}
```

本公共并行流程不接受会话自行指定分区。只有用户明确把某个 `Pnn` 分配给当前会话时，才可以使用同一 runner 的显式领取：

```powershell
$claimJson = pwsh -NoProfile -File .\Build\Tools\Invoke-Version4AuthoritativePartitionSession.ps1 `
  -Action Claim `
  -TaskSetName $taskSetName `
  -PartitionId Pnn `
  -LockWaitSeconds 60
```

领取返回 `status: claimed` 后，必须保存 `partition`、`sessionId` 和 `report`。从这一刻起：

- 领取锁已经释放，其他会话可以领取其他分区；
- 当前分区在 ledger 中是 `running`，其他会话不能再次领取它；
- 当前会话只处理这个 `partitionId`；
- 不得再次调用 `Claim`/`ClaimNext` 领取第二个分区；
- 不得手工编辑 `.agents/skills/version4-authoritative-partition-session/tasks/<TaskSetName>/task-state.json`；
- 不得替换任务集复制的 task table、state 或 lock 路径绕过公共账本。

如果返回退出码 `2`，重新使用 `-LockWaitSeconds 60` 等待领取临界区；不要删除 lock 文件。退出码 `4` 表示状态冲突，退出码 `3` 表示报告或输入校验失败，退出码 `6` 表示 runner 内部错误。没有成功拿到 `sessionId` 时不要生成本轮交付物，也不要自行生成新的 session ID。

## 3. 领取后的工作范围

领取完成后才开始分析和写文件。当前会话必须：

1. 读取 `inputReport` 的完整成员表，确认报告成员数与本提示词清单和 runner 校验结果一致；
2. 只处理当前 `partitionId` 的成员，不复制其他分区成员；
3. 回到 Version4 真实源码，核对关键字段、属性、方法、构造/初始化、注册、读写者、写入者、生命周期、网络、存档和副作用；
4. 按 `public-decomposition` 区分权威状态、行为状态、派生值、缓存、快照、兼容字段、Component、System、Query、Command、Adapter 和 Projection；
5. 对跨分区类型、共享 owner、网络/持久化值对象和顺序约束标记 `integration-review`，不在本分区宣布最终 owner；
6. 将源码事实、当前 NLTX 状态、proposed 设计和验证结果严格分开；
7. 只写自己的设计文档和执行文档。

本轮只生成设计/执行文档，不创建或修改 `.cs`、`.csproj`、测试、源代码生成物或生产迁移文件。除非用户另行明确授权，不运行 `dotnet restore/build/test/run/publish/pack/msbuild`。没有实际运行的 verifier 写 `verificationStatus: not-run`；历史证据写 `existing-evidence`，不能伪装成当前验证。

## 4. 两份输出文档

使用 runner 返回的 `inputReport` 文件名推导路径。输入报告必须位于：

```text
D:\TRbackup\NLTX\docs\migration\ledgers\authoritative-20-partitions\<reportFileName>
```

同时使用当前分区在公共研究提示目录中的同名语义报告（若存在）作为已存在证据参考，但不能覆盖它。当前会话只允许写入以下两份输出：

```text
D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\design\2026-09-11-version4-<slug>-component-design.md
D:\TRbackup\NLTX\docs\plans\component-decomposition\2026-09-11-version4-<slug>-component-execution.md
```

其中 `<slug>` 从 runner 返回的报告文件名去掉 `.md` 后得到，并转换为小写连字符形式；不得把 P01-P20 的分区 ID 替换成其他名称。两个输出文件都必须包含 `partitionId`、输入报告绝对路径、当前会话 `sessionId`、输出路径、证据状态、当前 NLTX 状态和验证状态。若目标文件已经存在，先确认它属于当前 session；不得覆盖其他会话的输出。

### 4.1 组件拆分设计文档最低内容

设计文档至少包含：

- 设计摘要、范围和排除范围；
- Version4/完整参考/tModLoader/ECS 证据表，含绝对路径、实际行号、类型/方法、读者、写者、生命周期、副作用和 `evidenceStatus`；
- 本分区完整成员到 Component/System/Query/Command/Adapter/Projection/暂缓归属表；
- 每个 proposed Component 的职责、字段、默认值、状态分类、不变量、生命周期、Entity/World 范围、ID/关系字段和当前 NLTX 映射；
- Component 组合关系、拆分/合并理由、不拆分项、依赖方向和边界 seam；
- 权威状态 owner、跨分区共享候选和 `crossSubsystemOwner: integration-review`；
- 网络、持久化、客户端投影、兼容字段和副作用隔离边界；
- evidence-gap、blocking-decision、行为保持风险、focused verifier 设计和 Integration Handoff；
- 明确写出所有设计类型均为 `status: proposed`，不是已创建实现。

组件必须按领域/能力目录提出路径，不能提出 `Shared/Components/`、`Common/`、`Misc/` 等泛化目录；一个同名 PascalCase 文件只承载一个核心 public 类型；System 顺序必须是显式调度契约，不得依赖文件顺序。

### 4.2 组件执行文档最低内容

执行文档是后续实施计划，不是本轮代码实现。至少包含：

- `executionStatus: planned`、`implementationStatus: not-started`、`verificationStatus: not-run`；
- 设计文档、研究报告和本分区成员范围；
- 按小步提交/可回滚单元排列的实施序列；
- 每个组件/系统/查询的 proposed 源路径、目标路径、命名空间和文件组织依据；
- 源文件到目标文件的映射、依赖影响、API/行为保持策略和迁移顺序；
- 新旧字段的单一写入 owner、双写禁止或兼容窗口、快照/网络/存档迁移策略；
- System 调度顺序、Query 纯度、Command 提交边界、Adapter/Projection 副作用边界；
- focused verifier、静态检查、测试和按仓库规则串行 build 的计划命令；
- 风险、证据缺口、阻塞决策、回滚条件和完成判定；
- 明确本文件没有执行任何 C# 迁移，没有声明编译、测试或行为等价已经通过。

执行文档可以包含伪代码、接口草图和命令计划，但不得写入可直接提交的完整运行时代码。

## 5. 交付前检查

在结算前逐项确认：

- 两份输出文件都存在，路径只属于当前 `partitionId`；
- 研究报告、公共提示词、其他分区报告、`src/`、`Test/`、Version4 和完整参考源码未被修改；
- 每个本分区成员都有归属或明确的 `deferred`/`evidence-gap`；
- 没有把 proposed 类型写成当前 NLTX 已实现；
- 没有宣布跨分区 owner 或复制其他分区成员；
- 文档中没有虚构未运行的 verifier/build/test 结果；
- 运行 `git diff --check -- <两个输出文件>`，并读取输出确认无空白错误。

## 6. 结算领取状态

设计和执行文档都完成并自检后，用领取时的 `sessionId` 完成结算：

```powershell
$resultJson = pwsh -NoProfile -File .\Build\Tools\Invoke-Version4AuthoritativePartitionSession.ps1 `
  -Action Complete `
  -TaskSetName $taskSetName `
  -PartitionId $partitionId `
  -SessionId $sessionId `
  -LockWaitSeconds 60
if ($LASTEXITCODE -ne 0) {
  throw "Partition completion failed with exit code $LASTEXITCODE: $resultJson"
}
$completion = $resultJson | ConvertFrom-Json
if ($completion.status -ne 'completed' -or $completion.lockReleased -ne $true) {
  throw "Partition completion did not confirm completed and released lock: $resultJson"
}
```

`Complete` 只短暂加锁写入 `completed` 并释放锁。它不验证文档内容，所以会话必须在结算前自己完成第 5 节检查。

如果无法完成，必须保留已写入的证据并结算失败，不得把分区留在无说明的 `running`：

```powershell
pwsh -NoProfile -File .\Build\Tools\Invoke-Version4AuthoritativePartitionSession.ps1 `
  -Action Fail `
  -TaskSetName $taskSetName `
  -PartitionId $partitionId `
  -SessionId $sessionId `
  -FailureMessage '说明阻塞原因、已完成内容和恢复条件' `
  -ResultExitCode 1 `
  -LockWaitSeconds 60
```

确实放弃当前分区、希望后续会话通过 `-Retry` 重新领取时，使用：

```powershell
$resultJson = pwsh -NoProfile -File .\Build\Tools\Invoke-Version4AuthoritativePartitionSession.ps1 `
  -Action Abandon `
  -TaskSetName $taskSetName `
  -PartitionId $partitionId `
  -SessionId $sessionId `
  -FailureMessage '说明放弃原因、已完成内容和恢复条件' `
  -LockWaitSeconds 60
if ($LASTEXITCODE -ne 0) {
  throw "Partition abandon failed with exit code $LASTEXITCODE: $resultJson"
}
$abandon = $resultJson | ConvertFrom-Json
if ($abandon.status -ne 'abandoned' -or $abandon.lockReleased -ne $true) {
  throw "Partition abandon did not confirm abandoned and released lock: $resultJson"
}
```

如果 `Complete` 结算失败或发现阻塞，必须调用 `Fail`，并检查返回的 `status: failed` 和 `lockReleased: true`。确实放弃当前分区、希望后续会话通过 `-Retry` 接手时，才使用 `Abandon`，并检查返回的 `status: abandoned` 和 `lockReleased: true`。无论哪种结算，必须使用最初 `ClaimNext` 返回的同一个 `partitionId` 和 `sessionId`。

`Complete`、`Fail`、`Abandon` 都只持有结算锁，不持有分析锁。领取成功、文档分析、文档写入、文档自检和最终结算之间，不得人为保持或删除 `.lock` 文件；锁的唯一语义是 runner 持有的独占 `FileStream`。

最终回复必须报告：

```text
partitionId:
sessionId:
inputReport:
componentDesignPath:
componentExecutionPath:
status: completed | failed | abandoned
evidenceStatus:
verificationStatus: not-run | existing-evidence | independently-verified | failed
关键 evidence-gap:
关键 blocking-decision:
未修改生产代码: yes
```

本提示词不是迁移完成声明。两份 Markdown 是设计和执行计划；只有后续获得明确授权并通过仓库验证，才可以进行实际代码迁移。
