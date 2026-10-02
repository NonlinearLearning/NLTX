# 权威 P19 世界生成 System 执行计划

documentKind: system-execution-plan  
partitionId: P19  
derivedFromDesign: docs/system-decomposition/design/2026-09-30-authoritative-P19-world-generation-execution-system-design.md  
derivedFromSystemReport: docs/system-decomposition/reports/2026-09-18-system-decomposition-authoritative-P19-world-generation-execution.md  
sourceReportSessionId: 33c1c3a45612439da49380669be73114  
executionStatus: partial-core-implementation
implementationStatus: partial
verificationStatus: core-slice-only
migrationStatus: deferred
sourceModified: true
buildRun: true
testsRun: true

## 1. 执行边界

本文定义 P19 分阶段执行计划，并记录本轮实现：有序 pass-result commit、一个已激活 pass 的同步 runner/进度/提交组合、按 pass ID 分发到显式 callback 注册的 runner adapter，以及将显式 registration 转换为有序 plan 的 catalog adapter。执行范围、验证证据与未完成集成见第 8 节。本文及代码均不表示 P19 全面接线、行为等价或迁移成功。

来源 `sessionId` `33c1c3a45612439da49380669be73114` 只用于报告 provenance。原 System 报告任务已结算；本文件不是该 runner claim 的 outputReport，也不重新结算或修改原 ledger。本轮代码范围仅限执行 System、runner port/dispatch adapter 和对应核心 verifier，不扩展到 P19 生命周期、持久化、legacy 入口或跨分区 owner。

## 2. 运行路径与证据状态

下表把 Version4 可见的同步流程与镜像补充行为分开。每个阶段只描述 P19 接入所需的边界，不重述 pass 内部地形算法。

| 顺序 | 入口/阶段 | Version4 可见行为 | 证据状态与开放项 |
| --- | --- | --- | --- |
| 1 | `Main` 创建世界 -> `WorldGen.CreateNewWorld` | 设置 active-world/UI/seed 相关状态后调用 `Task.Factory.StartNew` 执行 callback | 源码直接可见；重复启动、调度失败、取消和任务结果观察 unknown。CPG 的该方法 callsite 只在所选 `Main.cs` scope 命中。 |
| 2 | `WorldGen.worldGenCallback` | 播放声音、调用 `GenerateWorld`；仅返回 true 时 `SaveNewWorld`；更新 menu mode、播放声音，再调用完成 callback | Version4 语句顺序可确认；保存异常、callback exception 与 task error propagation 未闭合。 |
| 3 | `WorldGen.GenerateWorld` | 设 run flags；读取 embedded config、调用 hook、创建 generator；clear/reset world、添加 pass、按特殊 seed 禁用 pass；调用 generator 并 Finish；finally 恢复 temporary state 与 run/seed flags | 高层语句顺序和 finally 可见；Hook、pass registration/effect 和外部写入闭包 partial。 |
| 4 | `WorldGenerator.GenerateWorld` setup | `Controller.SetGenerator`、设置 static Controller/Progress、计算 enabled pass 总权重 | 调用与赋值位置可见；Version4 `SetGenerator` 为空，镜像补充的快照载入/禁用旧 skipped pass 行为不能作目标确认。 |
| 5 | pass loop / result commit | Version4 loop 检查 QueuedAbort、Paused；进入 `_controlLock`；以 `PassResults.Count` 选 pass；锁 pass；调用 `RunPass`、追加结果、调用 `OnPassCompleted`；目标新增 `ExecuteActivePass` 单 pass组合，需由调用方先激活当前 plan entry | Version4 loop 结构可见；CPG 对 `_currentPass` 选定范围有 2 项确认写和 2 项 Unknown；PassResults 的访问与可变 alias closure 不完整。目标 runner 仍无生产实现/legacy caller。完整镜像的 `RunPass` 行为只作 reference-only 对照。 |
| 6 | pass completion 与 hash/snapshot | Version4 `OnPassCompleted` stub 阻断完整语义 | 完整镜像展示 hash、manifest mismatch、snapshot cadence、reset/pause/UI 交互，只能作为行为对照清单；必须先确认采用何种源版本，再建立测试预期。 |
| 7 | 生成完成与 save/load | caller 根据 bool 条件保存或继续 WorldFile load 分支 | 保存条件确认；Version4 `WorldFile.LoadWorld` 的 seed/option 路径可见，失败后的所有 reload/error 行为和其他入口 unknown。 |
| 8 | snapshot create/restore/delete | Controller 对 Snapshot 的 Create/Restore/TryReset 调用存在；Restore 可见影响 GenVars、manifest、tile、NPC 和 UI | Version4 converter/stub 与 TileSnapshot 外部实现未闭合；镜像仅补充 bytes/字段处理线索，不能证明 atomicity/rollback。 |

`WorldFile.LoadWorld` 的 autogen 分支在直接源码中先尝试 copied seed；否则 normalize text seed、解析并选择 option、应用 `AutoGenEnabled`，再生成并仅在成功时保存。该顺序是输入兼容合同候选；完整调用者/订阅者闭包仍需补证。

## 3. 目标树与拟定路径

当前代码根为 `src/NSSLC/Component/WorldSession/WorldGeneration`。P19 state components 位于 `Progress`、`Controller`、`Persistence`、`Options` 等目录。`WorldGenerationPassExecutionSystem` 维护 plan/pass state 的开始、完成、失败和控制请求；本轮增加顺序结果提交、一个已激活 pass 的同步执行组合和通用 callback dispatch adapter。该 System 仍没有真实 pass callback、pass catalog 或 legacy caller，也未从 `Main`、`WorldGen` 或 `WorldFile` 调用，不能视为端到端生成 owner。

下列路径只是可能的落点，不应创建空文件或 placeholder；实现前需按当时局部 `AGENTS.md`、命名约束、项目依赖与已有注册方式复核：

| 候选能力 | 暂定目标路径/类型 | 前置约束 |
| --- | --- | --- |
| 生命周期与 worker/scheduler 接入 | `WorldGeneration/Systems/WorldGenerationLifecycleSystem.cs` | Main/WorldFile 入口、执行环境、task 错误和 cleanup contract 明确后实现。 |
| 有序 pass 与 progress/result commit | `WorldGeneration/Systems/WorldGenerationPassExecutionSystem.cs`、`Adapters/IWorldGenerationPassRunner.cs`、`Adapters/RegisteredWorldGenerationPassRunner.cs`、`Adapters/WorldGenerationPassCatalog.cs`、`Passes/WorldGenerationPassExecutionCallback.cs`、`Passes/WorldGenerationPassRegistration.cs`、`Passes/WorldGenerationPassRunOutput.cs` | 有序单 pass API、显式 callback dispatch 和显式 registration catalog 已实现；真实 pass callback 注册、pass-specific config、legacy entry caller、控制锁与失败/取消接入仍待闭合。 |
| pause/hash/abort/reset 协作 | 同 execution owner 的 partial 或 `WorldGenerationControlSystem` | 先证实 lock/commit 协议与可见性；不能引入并行第二 writer。 |
| snapshot/manifest I/O | `WorldGeneration/Persistence/WorldGenerationSnapshotPersistenceAdapter.cs`、`WorldManifestSerializerAdapter.cs` | 先定格式/version gate、恢复 owner、atomicity、错误和文件 scope。 |
| option catalog 与 selection | `WorldGeneration/Options/WorldGenerationOptionCatalog.cs`、selection owner/adapter | seed 归属、event subscribers、registration/init 顺序和不可变 view 明确后实现。 |
| P17/P20 seams | 复用经批准的既有 owner API | 不在 P19 下重复拥有地形、GenVars、tree rules、action chain 或 shape output。 |

## 4. 实施前 Gate

| Gate | 工作与交付 | 退出条件 | 当前状态 |
| --- | --- | --- | --- |
| G0 来源冻结 | 对 Version4 文件 hash、CPG manifest、完整参考镜像 hash、NSSLC 目标树分别留 provenance；逐项核对关键 source body 与版本差异 | 每条拟保留行为能指出来自 Version4、reference-only 还是 unknown；无跨快照合并 | 部分完成；CPG `SourceSnapshotId` 为 null，若干 Version4 方法是 stub。 |
| G1 owner/集成合同 | 决定 P17 terrain/GenVars restore、P18 seed selection、P20 action payload、Main/WorldFile save、UI/event subscriber、snapshot storage、scheduler/thread/network 边界 | 每项不变量有唯一 writer、输入输出、失败返回和提交时点；开放项挂 `crossSubsystemOwner: integration-review` | 未完成；依赖决议的代码 slice 不可开始。 |
| G2 API 与行为 oracle | 以概念行为列出 return/error、state delta、events/effects、ordering/visibility、scope/lifecycle、retry/idempotency；区分 Version4 实现与 stub | 每条接入组合有可观察合同；stub 行为不被参考镜像静默补成 Version4 expected behavior | ordered commit、单 active pass runner、callback dispatch 与 registration catalog 已由窄 verifier 覆盖；legacy RunPass、Controller callback、snapshot 和 lifecycle oracle 仍未执行。 |
| G3 调度与 writer 合同 | 定义 lifecycle 与 pass executor 调用关系、sync/async 边界、控制 gate、PassResults/cursor 单 writer、reset/restore visibility | scheduler/phase/barrier 是从实际目标框架注册和调用证据得出；不能靠目录/文件顺序推断 | 未完成。 |
| G4 实现授权 | 依据用户实现要求在目标路径实施一个垂直 slice，并确认 caller 命中新 owner | 不新增重复 authority；旧入口行为与新组合可追踪 | ordered commit、单 active pass runner composition、callback dispatch adapter 与显式 catalog adapter 已实现；没有真实 callback registration 或 production caller，legacy 接线 gate 未完成。 |

G0-G3 中与某 slice 无依赖的工作可独立继续；未知不得扩散成整个 P19 已确认，也不得绕过 owner 决策创建第二套状态。

## 5. Proposed 实施阶段

以下顺序是实现准备建议，不是运行时 scheduler 顺序，也不是已执行的迁移流水线。

### Phase 1: 固定旧入口与 lifecycle transaction

先界定 `Main` / `WorldFile` 的入口与 active-world 输入；提取 run-scoped input/result；显式包住 config load、hook、world prepare/reset、pass list 形成、finish 与 finally cleanup。保留同步 statement order 和 success-only save。先解决任务启动失败、cancel、callback failure、异常和重复 start；其语义 unknown 前不能承诺 task API 兼容。

**退出门**：start、成功、abort、pass failure、hook failure、save/callback failure 各路径的状态清理和 callback/save 次序有明确观察合同；world/session identity 有明确 owner。

### Phase 2: 有序 pass executor 与 progress/result commit

以稳定有序 pass catalog 输入驱动同步 loop。目标已有 `ExecuteActivePass` 执行已激活 descriptor，通过 runner port 同步执行、接收 progress 和 measurement，并在成功时按 plan 序号提交结果；caller 仍负责 descriptor->stage 映射、catalog、具体 pass/config operation 和 loop。progress、skipped pass、random seed 初始化、pass exception 与 result/hash 字段要分别与 Version4 行为对照；镜像中的 `RunPass` 行为保持 reference-only，直到行为 oracle 决定采用。

**退出门**：pass 注册与禁用来源闭合；生产 runner 和 legacy caller 接通；每个 pass 最多一次 result commit；pass 顺序和 cursor/result 一致性验证通过；失败、取消与 pause 路径明确不会伪造成功保存。

### Phase 3: control 与 checkpoint 编排

pause/resume/abort/pause-after-pass/hash mismatch/reset/snapshot 请求经明确的控制边界进入同一 execution/commit gate。保留同步可见点，定义锁竞争、重复请求、current pass 正在执行、reset 与 in-flight run 的行为。先定 world authority 对 restore 的接受/拒绝协议，Control 不直接重写 terrain 或 manifest。

**退出门**：不存在 cursor/PassResults 双写；pause/abort/reset 状态机、返回值、progress/UI effect、快照选择和错误路径都有覆盖；thread safety 由具体 scheduler/lock 证据支撑。

### Phase 4: manifest 与 snapshot persistence

把 manifest 和 tile/GenVars snapshot serialization 从控制流程中隔离。先确认 binary layout、version/GitSHA policy、pass-result order、active-world path、corrupt/truncated data 和 stale history 规则，再实现 adapter。Restore 采用先读/验证、后交 live-world owners 提交的协议；失败的 atomicity/rollback 未定之前不得启用生产恢复。

**退出门**：save/load 与旧记录 compatibility、有害半恢复的处理、snapshot cache/delete/world switching、P17 tile/GenVars/NPC handoff 有实际测试和 owner 决议。

### Phase 5: option catalog 与 selection

拆清不可变 option definition/catalog 与 run/world selection state。catalog query 返回只读数据，不暴露旧静态 mutable option alias；selection command 保留 Reset-before-Select、同步 Enabled hook/event 和 `AutoGenEnabled` 配置效果。先枚举 event subscribers 与 P18 seed resolver，再改变入口或 event registration。

**退出门**：注册重复/顺序、文本和值的归一化、copied seed 分支、server flags、同步回调可见性和下一 run 的效果均有明确合同。

### Phase 6: legacy adapter 与跨域集成

只在 lifecycle/pass/control/persistence/options 单独完成各自门禁后，逐一接通 `Main`、`WorldFile`、legacy callback 与 UI/extension seam。保留旧 facade 仅作为受控适配层；不要让旧入口与新 owner 同时提交同一不变量。P17/P18/P20 handoff 和 scheduler integration 一起做端到端确认。

**退出门**：所有进入 P19 的真实入口均命中新 owner/API composition；失败、卸载、换 world、save 和重复进入路径都有接入证据。

### Phase 7: 行为验收与旧实现清理决策

在另一个明确授权的实现/验证任务中，运行真实迁移项目要求的行为测试并记录 source revision、测试入口与 observation vector。只有新 owner 与组合被测试命中且必需测试通过，才能报告迁移成功。删除旧实现或兼容层需单独的 caller、序列化、协议和 rollback 证据；本计划不包含删除授权。

## 6. 后续行为验证矩阵

本表仍定义尚未执行的端到端覆盖范围。ordered commit、单 active pass runner 与 callback dispatch verifier 已运行，详见第 8 节；其结果不能替代本表场景验收。

| 场景 | 必须观察 | 主要未决项 |
| --- | --- | --- |
| 新世界成功/失败 | callback/task 结果、pass order、manifest/hash、sound/menu、success-only save、finally flags | task/callback/save exception 和重试路径。 |
| missing-file autogen / copied seed | seed 分支、option selection/event、生成结果、保存条件、后续 load/error | seed owner、event subscribers 与 load continuation。 |
| enabled/disabled pass sequence | pass identity/order、skipped flag、weight/progress、result order、cursor visibility | Version4 RunPass/Controller stubs；注册及配置规则。 |
| pause/resume/abort/run-to-pass | 状态跃迁、control lock、当前/下一 pass、返回值、UI visibility、响应性、幂等性 | OnPaused/OnPassCompleted 方法体与 lock/thread 语义。 |
| hash mismatch 与 snapshot cadence | hash input/output、mismatch pause、snapshot timing、history invalidate/replace | Version4 completion callback stub；reference-only 细节需选定 oracle。 |
| snapshot create/restore/delete | manifest/GenVars/tile 数据、NPC cleanup、temp state、cache/files、corruption/rollback/world identity | converter、TileSnapshot 与 P17 authority/transaction。 |
| manifest round-trip/save failure | record version gate、字段顺序、nullable values、fallback、user-facing error/rethrow | format compatibility 和异常恢复。 |
| option catalog/selection | unique registration、seed matching、Reset/Select 顺序、autogen flags、callbacks/next-run effects | subscriber closure、P18 seed handoff、extension registration。 |
| session/multi-world/thread | duplicate start、cancel、active-world replacement、unload、thread affinity、run isolation | 当前 shared static/thread-static 混用无法证明支持语义。 |

测试必须调用真实 legacy entry 与 proposed System composition；孤立 helper 测试、编译成功、文件存在、静态 CPG 或完整参考镜像均不能单独证明行为等价。

## 7. Stop conditions 与当前状态

以下任一条件影响当前 slice 时，暂停该 slice 的实现并交 `integration-review`：

- 尚无唯一 world/terrain/GenVars restore owner；
- pass 顺序、pass list writer、结果提交者或控制锁协议仍有争议；
- 源码 stub 与 reference-only 行为之间尚未选择行为 oracle；
- save/manifest/snapshot 格式、atomicity、rollback 或 world identity 未定；
- P18 seed、P20 action/shape、UI/event subscriber、scheduler/thread/network entry 未闭合；
- 当前 CPG/source snapshot binding 不足以支持拟做的 source-level claim。

当前状态：`executionStatus: partial-core-implementation`，`implementationStatus: partial`，`verificationStatus: core-slice-only`，`migrationStatus: deferred`。未运行完整 P19 行为 verifier；不作迁移成功结论。

## 8. 已实施核心 slice

### 8.1 代码边界

- `WorldGenerationManifestComponent` 持有只读顺序 `PassResults` 视图；追加只接受下一个 `PassIndex`，并拒绝混合 generation ID。
- `WorldGenerationPassExecutionSystem.TryGetNextPass` 按 `PassResults.Count` 选择显式 plan 中的下一未提交 descriptor；disabled pass 仍占据原顺序位置，stage 和 effect 留给调用边界。
- `IWorldGenerationPassRunner.Execute` 通过显式 runner port 同步执行一个已激活的 descriptor，输入 active pass state 和当前可表达的 `WorldGenerationConfigurationSnapshot`；执行时可通过同步 progress callback 报告 message/value。接口要求 runner 在返回前结束调用且不保留 callback。
- `WorldGenerationPassExecutionSystem.ExecuteActivePass` 只接受 manifest prefix 对应的当前 active enabled pass；开始时把 local progress 置零，runner 成功返回后由系统填入 generation ID、pass index、pass ID，再提交 result/checkpoint 并刷新 weighted progress。完成后 local progress 归零，message 与 current pass weight 保留。
- 普通 runner exception 返回 `WorldGenerationPassExecutionAttempt`，以 `FailPass` 生成带失败原因的 state，不追加成功结果；异常对象一并交给 caller。`OperationCanceledException` 原样向上传播，不伪装成普通 pass failure。异常前已报告的 progress 不回滚；caller 仍须把返回的 state 写回其 authority。
- runner 输出 `DurationMs`、`Hash` 与可选 `RandomNextValue`。这些数据均由 runner 显式提供；本 System 不读取时钟、不调用随机源，也不映射 hash 或 legacy random stream。
- `RegisteredWorldGenerationPassRunner` 在构造时复制 callback collection，以 `Ordinal` pass ID 查找并拒绝空白/重复 ID 或空 callback。执行时要求 active state 的 pass ID/version 与 descriptor 一致；缺少 callback 或 callback 返回 null 会抛出异常，由 `ExecuteActivePass` 转成 failed state 且不提交 pass result。Callback 的实际世界效果和配置解释仍由注册方提供。
- `WorldGenerationPassCatalog` 按调用方给出的 registration 顺序构造 `WorldGenerationPlanComponent` 和 `RegisteredWorldGenerationPassRunner`；enabled registration 必须带 callback，disabled registration 仍占据 plan 顺序但不进入可执行 callback 集合，重复 pass ID 被拒绝。它不从 Version4/WorldGen 发现 pass，也不解释 pass-specific configuration。
- `WorldGenerationPassExecutionSystem.CompletePassWithResult` 验证活动 pass、generation、plan 顺序和结果身份，再提交 manifest result 并返回完成后的 pass state/checkpoint。
- `CommitSkippedPassResult` 只接受 plan 中禁用的 pass，以及由调用方提供的 skipped result；不在该 API 中推导或执行 Version4 `RunPass` 行为。

该 slice 现在提供单 active pass runner boundary、通用 pass ID callback dispatch adapter 和显式 registration catalog；没有真实 pass callback 注册、pass-specific configuration 映射、Version4 `WorldGen` pass catalog、legacy entry caller、整体 loop/control lock、pause 中断、hash/snapshot/UI、serializer 或 lifecycle cleanup。完整参考树的 `GenPass.ApplyPass(progress, configuration)` 与具名 pass delegate 只用于支持“注册并分发 pass 操作”的组合形状；Version4 `RunPass` 仍是 stub，不能据此宣称行为等价或迁移成功。

本轮重新使用只读 CPG Query API 查询 Version4 `Terraria.WorldBuilding/WorldGenerator.cs`：`RunPass` 符号和所选文件范围内 1 个直接调用点返回 `complete`；`GenerateWorld` callable facts 返回 `partial` 并含 `CalleeEffectsNotExpanded` gap，数据库 `SourceSnapshotId` 为 null。完整参考项目 `D:\TRbackup\无任何删减通过编译` 的 `WorldGenerator.RunPass` 显示 disabled skip、pass-name configuration lookup、progress start/end、random reset、异常报告与 result measurements；这些仅为 reference-only，callback adapter 不实现或承诺这些行为。

同一只读 CPG 会话还查询了 `Terraria/WorldGen.cs` 的 `AddPasses`：符号及其在选定 `WorldGen.GenerateWorld` 范围内的一个直接调用点返回 `complete`；查询 `GetPassConfiguration` 在选定 Version4 范围没有返回符号项，故目标 pass 配置映射保持 `unknown`。直接源码显示 Version4 的 `AddGenerationPass` overload 是空方法，而完整参考项目的 overload 会 append 到 generator，因此不能把参考注册行为当作目标 catalog 合同。上述 CPG 数据库 manifest SHA-256 为 `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`，`SourceSnapshotId` 为 null。

### 8.2 验证

已按仓库串行 wrapper、SDK `10.0.400` 执行单项目构建：

```powershell
$dotnetArguments = @(
  'build',
  '.\Test\Terraria.WorldGeneration.PassExecution.Core.Verification\Terraria.WorldGeneration.PassExecution.Core.Verification.csproj',
  '--configuration', 'Release',
  '--no-restore',
  '-m:1', '-nr:false',
  '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false',
  '-p:BuildInParallel=false'
)
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArguments
```

结果：exit code `0`，`0` warnings，`0` errors；输出位于 `Build/bin/Terraria.WorldGeneration.PassExecution.Core.Verification/Release/net10.0/`。

随后使用同一 wrapper 运行该 verifier，带 `--no-build --no-restore`，exit code `0`，输出：`PASS: registered world-generation pass dispatch and commit boundary`。该单项目核心切片覆盖按 plan 顺序选择并激活 pass、通过 registration catalog 保留 enabled/disabled 顺序并调用其生成的 runner、通过注册 adapter 精确 dispatch、拒绝重复 catalog/callback 注册、显式 configuration/state 传入 callback、progress callback、duration/hash/random checkpoint 值的结果提交、completion stage/checkpoint、disabled pass 的 skipped commit、重复 commit 拒绝，以及 callback 普通异常返回 failed state 且不追加成功结果。未覆盖真实 pass callback、pass-specific config、随机源重置、legacy entry、完整 loop、pause/cancel、snapshot、存档或运行时行为；未运行全量 solution 或其他 verifier。

verifier 的可复现运行命令（在仓库根目录执行；`Invoke-SerialDotnet.ps1` 会取得 checkout 串行锁）：

```powershell
$dotnetSdk400 = 'C:\Users\shan\AppData\Local\Codex\dotnet-sdk-10.0.400'
$previousPath = $env:PATH
$env:PATH = "$dotnetSdk400;$previousPath"
try {
  $dotnetArguments = @(
    'run',
    '--project',
    '.\Test\Terraria.WorldGeneration.PassExecution.Core.Verification\Terraria.WorldGeneration.PassExecution.Core.Verification.csproj',
    '--configuration', 'Release',
    '--no-build', '--no-restore',
    '-m:1', '-nr:false',
    '-p:UseSharedCompilation=false',
    '-p:MSBuildNodeReuse=false',
    '-p:BuildInParallel=false'
  )
  & .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArguments
}
finally {
  $env:PATH = $previousPath
}
```

此会话开始时系统默认解析到 SDK `10.0.100`，仓库 `global.json` 要求 `10.0.400`；上述临时 PATH 前置用于选中本机 SDK `10.0.400`。该命令不重建项目，只运行已有 Release 输出。

完整 solution、其他 verification 项目、旧入口行为和 runtime 未运行。`migrationStatus` 仍为 `deferred`。
