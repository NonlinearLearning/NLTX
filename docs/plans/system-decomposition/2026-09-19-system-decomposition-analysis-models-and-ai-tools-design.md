# System 拆分静态分析模型与 AI 工具设计

**状态：** `proposed`。本文仍是 System 拆分预处理工具的设计；对应模型、静态 analyzer、
AI 工具和统一 CLI 已落地到 skill-owned tools 目录，但本轮未运行构建、测试或真实分区扫描。
它不表示任何分区已经完成迁移。

**目标：** 为 System 拆分建立一条可追溯、可恢复、可组合的静态证据和 AI 候选设计管线，覆盖入站入口、动态入口、生命周期闭包、写入闭包、跨分区 seam、概念 API 和候选边界。

**相关文档：**

- [`System 拆分：静态分析与候选边界设计`](../../system-decomposition/2026-09-19-system-decomposition-static-analysis-and-boundary-design.md)
- [`项目最终验收与迁移门禁`](../../system-decomposition/2026-09-19-project-final-acceptance-and-migration-gates.md)
- [`证据导入与关系物化 API 计划`](../2026-09-19-evidence-import-relation-materialization-api.md)
- [`System 拆分静态分析模型与 AI 工具实现计划`](2026-09-19-system-decomposition-analysis-models-and-ai-tools-implementation-plan.md)
- [`ecs-system-domain-splitting/SKILL.md`](../../../.agents/skills/ecs-system/SKILL.md)
- [`system-decomposition-toolchain.md`](../../../.agents/skills/ecs-system/references/system-decomposition-toolchain.md)
- [`ai-synthesis-contract.md`](../../../.agents/skills/ecs-system/references/ai-synthesis-contract.md)

## 1. 设计原则

### 1.1 目标是证据覆盖，不是自动决定边界

工具链负责把“尚未检查”变成以下一种可追踪结果：

```text
confirmed fact
partial fact
ambiguous binding
unresolved relation
dynamic risk
unknown gap
candidate design
```

工具不得把静态发现自动升级为：

```text
final owner
behavior-equivalent
verified
migration-success
deletion-safe
```

唯一 Owner、最终边界、旧新 API 语义等价、迁移成功和旧实现删除仍属于项目级集成审查及最终行为验收。

### 1.2 复用现有证据基础设施

不为每个分析器建立孤立的 JSON、身份系统和关系图。新增工具统一复用：

- `AnalysisEvidenceCatalog`：阶段、artifact、事实、诊断、缺口和版本化只读快照；
- `RelationMultigraph`：有来源的语义关系边；
- `ExecutionDag`：分析阶段依赖、barrier 和失败传播；
- `AnalysisEvidenceApi`：有预算的 stage/entity/relation/binding 查询；
- `SourceAnalysisInfrastructure`：Roslyn/MSBuild 项目加载和 canonical symbol identity；
- `ProjectDataflowAnalyzer`：项目级 callable 读写集；
- `CalledFunctionsAnalyzer`：直接及静态传递出站调用。

新增能力只增加模型、artifact reader、关系类型、分析模块和 AI 上下文/候选模块，不复制这些基础能力。

### 1.3 源码只读，输出显式且可恢复

分析器只能读取明确的 source root、project、configuration 和分区范围；输出只能写到调用方明确指定的 artifact/report 目录。工具不得：

- 修改 Version4 或当前生产源码；
- 递归扫描 `reports/`、`managed/`、`dist/` 来猜测输入；
- 通过文件名、行号、方法名相似度补齐缺失的 canonical endpoint；
- 删除 unresolved、dynamic、ambiguous 或 truncated 关系；
- 把 AI 文本直接写入权威事实表。

## 2. 体系结构

### 2.1 分层

```text
Source root / project / partition manifest
  -> Scope and canonical symbol layer
  -> Roslyn static analyzers
  -> Explicit artifact envelopes
  -> EvidenceImportPipeline
  -> RelationMaterializer / RelationMultigraph
  -> Closure, coverage and cross-partition analysis
  -> Bounded source closure package
  -> AI semantic synthesis
  -> Candidate API / owner / boundary design
  -> StaticEvidenceBundle / IntegrationHandoff / Markdown report
```

分层职责：

| 层 | 负责 | 不负责 |
| --- | --- | --- |
| Scope | 范围、快照、配置、排除项和输入身份 | 推断 System Owner |
| Symbol | 类型、成员、重载、接口、override 和 canonical key | 推断概念行为 |
| Static analysis | 代码事实和风险面 | 证明运行时闭包 |
| Evidence | artifact 注册、状态传播、关系物化和追溯 | 生成缺失事实 |
| Closure | 入站/出站/读写/生命周期/副作用的候选闭包 | 证明运行时实际发生 |
| AI context | 将有界证据和反证打包给模型 | 自行扩展源码范围 |
| AI synthesis | 语义归纳和候选设计 | 将推断写成 confirmed |
| Report | 统一交付和阻断状态 | 声明迁移成功 |

### 2.2 项目和程序集边界

建议新增以下工具程序集，保持现有程序集职责清晰：

```text
.agents/skills/ecs-system/tools/
  AnalysisEvidenceCatalog/              # 现有：共享模型、artifact、关系和查询
  SourceAnalysisInfrastructure/          # 现有：Roslyn/MSBuild 基础设施
  SystemDecomposition.Analysis/          # 新增：静态分析、闭包、覆盖和候选事实
  SystemDecomposition.Ai/                # 新增：AI 上下文、结构化归纳和候选设计
  SystemDecomposition.Cli/               # 新增：范围驱动的统一命令入口
  scripts/ProjectDataflowAnalyzer/       # 现有：独立 dataflow CLI
  scripts/CalledFunctionsAnalyzer/       # 现有：独立 called-functions CLI
```

`AnalysisEvidenceCatalog` 是底层契约，不引用 `SystemDecomposition.Analysis` 或 AI provider。分析器引用 catalog 和 source infrastructure；AI 模块只依赖 catalog 模型、查询 API 和分析输出契约，避免 AI 层反向控制静态事实。

## 3. 共享模型设计

下列模型是跨工具的稳定契约。它们优先作为不可变 `record` 或只读集合发布；实现时不得把可变 Roslyn symbol、`SyntaxNode` 或外部服务句柄放进持久化模型。

### 3.1 身份和来源模型

```text
AnalysisContext
  runId: AnalysisRunId
  sourceSnapshotId: SourceSnapshotId
  configurationDigest: string
  projectId: string
  configuration: string
  platform: string
  toolchainVersion: string
  schemaVersion: string

EvidenceProvenance
  evidenceId: string
  artifactRefs: ArtifactRef[]
  sourceLocations: SourceLocation[]
  queryScope: QueryScope
  status: EvidenceStatus
  diagnostics: Diagnostic[]
  gapReason: string?
```

规则：

- `sourceSnapshotId` 表示源文件内容身份；配置、引用、语言版本和条件符号进入 `configurationDigest`；
- `runId`、快照和配置不匹配的事实不能合并；
- canonical symbol key 是 callable identity，文件和行号只用于定位；
- legacy artifact 保持 `legacy`/`partial`，不因查询方提供上下文而升级；
- 所有输出都带 producer/tool/schema/version，便于失效和重跑。

### 3.2 范围和覆盖模型

```text
SystemDecompositionScopeManifest
  profile
  partitionId
  sessionId
  inputReport
  claimedMembers
  allowedSourceRoots
  excludedPaths
  sourceSnapshotId
  configurationDigest
  project
  configuration
  platform
  analysisToolVersions
  priorArtifacts
  queryScope
  outputRoot
  declaredDynamicSurfaces

CoverageMatrix
  scopeId
  dimension
  expectedItems
  observedItems
  unresolvedItems
  missingItems
  excludedItems
  evidenceRefs
  status
  minimumNextEvidence
```

`CoverageMatrix` 的维度至少包括：成员、入站入口、出站调用、读写、事件/委托、动态入口、生命周期、effect、调度、序列化、跨分区 seam 和概念 API。`observedItems = 0` 不表示没有关系，必须同时记录搜索范围和 gap。

### 3.3 入口和行为模型

```text
EntryPointFact
  entryPointId
  canonicalKey
  entryKind
  visibility
  declaringType
  overloadIdentity
  inboundCallers
  registrationSites
  callbackStorageSites
  dispatchKind
  evidence

BehaviorSurfaceCandidate
  conceptCandidateId
  entryPointIds
  inputsAndIdentity
  preconditions
  reads
  writes
  returnOrError
  exceptions
  emittedEvents
  queueOperations
  externalEffects
  orderCandidates
  lifecycleCandidates
  retryAndIdempotencyUnknowns
  evidenceStatus
  openGaps
```

`EntryPointFact` 记录“入口存在”；`BehaviorSurfaceCandidate` 记录“从静态代码可观察到的行为候选”。二者不能合并成一个“已确认契约”。

### 3.4 动态边界和 effect 模型

```text
DynamicRiskFact
  riskId
  surfaceKind: reflection | string-registration | configuration | generated-code |
    native | plugin | script | timer | task | serializer | unknown
  sourceLocation
  lookupKey
  candidateTargets
  resolutionStatus
  triggerContext
  missingRuntimeEvidence
  evidence

EffectCandidate
  effectId
  effectKind: network | persistence | file | logging | metrics | random | clock |
    external-process | irreversible | cache | queue | unknown
  ownerCandidate
  sourceLocation
  inputReads
  outputWrites
  commitCandidate
  retryCandidate
  compensationUnknown
  evidence
```

动态扫描只生产风险和候选 target。未能解析的反射字符串、配置值和生成类型必须保留为 `unknown` 或 `partial`。

### 3.5 生命周期、写入和状态角色模型

```text
LifecycleTransitionCandidate
  transitionId
  scope: process | world | session | entity | frame | unknown
  currentState
  trigger
  preconditions
  stateDelta
  nextState
  emittedEvents
  effects
  failurePath
  rollbackCandidate
  reentrancyRisk
  evidenceStatus

WriteClosure
  resourceKey
  directWriters
  indirectWriterChains
  eventQueueWriters
  restoreWriters
  rebuildWriters
  resetWriters
  cacheWriters
  unresolvedWriters
  candidateCommitPoints
  ownerConflict
  evidenceStatus

StateRoleCandidate
  resourceKey
  role: authoritative | derived | cache | snapshot | compatibility | presentation | unknown
  lifecycle
  observedReaders
  observedWriters
  reason
  evidenceStatus
```

`WriteClosure` 不选择最终 Owner。它只保证所有已发现写入角色和未知写入被同一结构保存，供 AI 和集成审查使用。

### 3.6 调度、图和跨分区模型

```text
ScheduleCandidate
  scheduleId
  subject
  relation: must-before | must-after | barrier | publishes | visible-after | parallel
  target
  phaseCandidate
  commitPointCandidate
  sourceBasis
  runtimeGap

GraphDiagnostic
  graphKind: call | relation | lifecycle | schedule | analysis-pipeline
  nodes
  edges
  stronglyConnectedComponents
  cycleKind
  boundaryRisk
  evidence

IntegrationHandoff
  seamId
  currentPartition
  relatedPartitions
  sharedStateOrContract
  observedReaders
  observedWriters
  producer
  consumer
  orderingRequirement
  lifecycleRequirement
  versionRequirement
  candidateOwners
  conflict
  minimumNextEvidence
  crossSubsystemOwner: integration-review
```

目标调用图允许循环；候选调度 DAG 才检查拓扑顺序。分析流水线 DAG 只描述工具阶段，不能与目标运行时 DAG 混用。

### 3.7 概念 API、边界和 AI 模型

```text
ConceptBehaviorCandidate
  conceptId
  scope
  legacyEntryPoints
  inputsAndIdentity
  preconditions
  reads
  invariantCandidate
  authoritativeWritesCandidate
  returnOrError
  emittedEventsAndEffects
  orderAndCommitBoundary
  visibilityCandidate
  lifecycleAndScope
  failurePolicyCandidate
  retryAndIdempotencyUnknowns
  canonicalNewCompositionCandidate
  requiredBehaviorScenarios
  evidenceStatus
  openGaps

BoundaryDecisionCandidate
  decisionId
  chosen: keep | partial | separate
  capability
  invariant
  stateOwnerCandidate
  readWriteImpact
  lifecycleImpact
  scheduleImpact
  sideEffectImpact
  collaborationContract
  rejectedAlternatives
  revisitConditions
  evidenceRefs
  designStatus: proposed | conflicted | unknown

AiSemanticClaim
  claimId
  claimKind: fact-summary | inference | counter-evidence | design-candidate | gap
  statement
  evidenceRefs
  counterEvidenceRefs
  confidenceLabel
  allowedStatus: static-confirmed | static-partial | static-candidate |
    proposed | conflicted | unknown
  forbiddenUpgradeReason

AiSynthesisResult
  requestId
  promptVersion
  schemaVersion
  claims
  conceptBehaviors
  boundaryCandidates
  integrationHandoffs
  unresolvedQuestions
  sourceContextDigest
  status
```

AI 结果只能使用静态阶段状态。`AiSemanticClaim.claimKind = fact-summary` 也必须引用 artifact；没有引用的陈述只能是 `unknown` 或被拒绝。

## 4. 工具设计

### 4.1 范围和符号工具

| 工具 | 输入 | 输出 | 依赖 |
| --- | --- | --- | --- |
| `ScopeManifestBuilder` | 分区任务、输入报告、source root、project、配置和排除项 | `SystemDecompositionScopeManifest` | `SourceSnapshotId`、文件 hash、任务表 |
| `ProjectSymbolIndex` | MSBuild/Roslyn project 和 manifest | canonical type/callable/member index | `SourceAnalysisInfrastructure` |
| `LegacyEntryPointInventory` | symbol index、反向引用候选和分区成员 | `EntryPointFact` | symbol、catalog |

范围工具不读取其他分区来替代当前分区成员；必要的证据闭包通过显式 `allowedSourceRoots` 和 `queryScope` 进入。

### 4.2 静态事实工具

| 工具 | 责任 | 主要 artifact |
| --- | --- | --- |
| `InboundEntryPointAnalyzer` | caller、接口实现、override、构造/工厂、注册和回调反向入口 | `InboundRelations` |
| `EventAndDelegateRegistrationAnalyzer` | subscribe/unsubscribe、委托保存/传递/调用和注册生命周期 | `EventDelegateRelations` |
| `BehaviorSurfaceAnalyzer` | guard、return、exception、队列、事件、调用和外部 effect | `BehaviorSurface` |
| `DynamicBoundaryScanner` | 反射、字符串注册、配置、生成代码、native/plugin、script、Timer/Task | `DynamicRisk` |
| `SerializationContractAnalyzer` | 网络、存档、序列化键、版本字段、编码和恢复入口 | `SerializationContract` |
| `LifecycleGraphAnalyzer` | create/activate/update/end/destroy/rebuild/reset/unload/persist/restore | `LifecycleGraph` |
| `EffectBoundaryAnalyzer` | network、persistence、I/O、random、clock、logging、metrics、不可逆 effect | `EffectCandidates` |
| `ProjectDataflowAnalyzer` | callable 读写、capture、dataflow in/out | 现有 dataflow artifact |
| `CalledFunctionsAnalyzer` | 直接和静态传递出站调用 | 现有 called-functions artifact |

`ProjectDataflowAnalyzer` 和 `CalledFunctionsAnalyzer` 保持现有独立 CLI；新工具通过明确 stage descriptor 调用或消费其 artifact，不复制 Roslyn project loader。

### 4.3 闭包、所有权和图工具

| 工具 | 责任 | 输出边界 |
| --- | --- | --- |
| `WriteClosureAnalyzer` | 将直接、间接、队列、恢复、重建、reset 和缓存写统一到资源维度 | `WriteClosure`，不决定 Owner |
| `StateRoleClassifier` | 根据生命周期、读写和来源提出状态角色 | `StateRoleCandidate` |
| `InvariantCandidateAnalyzer` | 从 guard、联合读写、失败路径和提交点提取不变量候选 | `InvariantCandidate` |
| `OwnerCandidateAnalyzer` | 聚合写入角色、恢复者、提交点和冲突 | `OwnerCandidate` |
| `CommitPointCandidateExtractor` | 识别 validate/commit/publish/flush/apply 候选 | `CommitPointCandidate` |
| `ScheduleCandidateExtractor` | 提取显式顺序、phase、barrier、queue flush 和 visibility 候选 | `ScheduleCandidate` |
| `RelationCycleAndSccAnalyzer` | 识别递归、互调、事件回路和 SCC | `GraphDiagnostic` |
| `CrossPartitionSeamAnalyzer` | 关联共享 ID、Component、snapshot、event、网络和持久化关系 | `IntegrationHandoff` |
| `CoverageAndGapAuditor` | 计算覆盖矩阵、未检查范围、状态传播和最小补证据 | `CoverageMatrix`、`EvidenceGap` |

这些工具只能对 catalog 中已登记的关系和事实工作；未加载或未登记的数据不会被当作空集。

### 4.4 API 和源码上下文工具

| 工具 | 责任 | 输出 |
| --- | --- | --- |
| `LegacyApiInventoryAnalyzer` | 汇总旧公开入口、overload、接口/事件/回调入口和调用方 | API inventory |
| `ApiPreconditionAnalyzer` | 参数、权限、身份、缺失组件、默认值和 overload 分流 | 前置条件候选 |
| `ApiFailurePathAnalyzer` | 异常、拒绝、部分提交、队列残留和回滚候选 | 失败路径候选 |
| `RetryAndIdempotencyScanner` | request key、去重、重试循环、重复事件和幂等判断 | 重试/幂等候选 |
| `ConceptBehaviorCandidateExtractor` | 将多个入口归并到稳定 `ConceptId` | `ConceptBehaviorCandidate` |
| `LegacyNewCompositionMapper` | 生成旧入口到 ConceptId 到新组合的候选映射 | `CompositionCandidate` |
| `ObservationVectorBuilder` | 生成未来验收的 result/state/event/effect/order/lifecycle/retry 观察轴 | `ObservationContract` |
| `BoundedSourceClosurePackager` | 按 manifest、canonical key 和关系边打包有限源码闭包 | `SourceClosureManifest` |

### 4.5 AI 工具

AI 工具不负责发现未经 catalog 登记的代码，也不默认调用外部模型服务。

| 工具 | 责任 | 输入 | 输出 |
| --- | --- | --- | --- |
| `SemanticEvidenceContextBuilder` | 按分区和 ConceptId 组装有界证据上下文 | catalog 查询、source closure、gaps | `AiContextPackage` |
| `EvidenceTracePackager` | 将结论绑定到 artifact、位置、关系和缺口 | facts/relations/diagnostics | trace bundle |
| `CounterEvidenceCollector` | 查询相反写者、替代 Owner、未解析入口和冲突边 | relation graph、coverage | counter-evidence bundle |
| `SemanticSynthesisInputValidator` | 检查 reader/writer/caller/lifecycle/effect 是否缺失 | `AiContextPackage` | 可消费/阻断结果 |
| `AiSemanticSynthesisGateway` | 输出 provider-neutral prompt，或校验结构化模型结果 | context package、schema | `AiSynthesisResult` |
| `BoundaryCandidateGenerator` | 将 AI 结论和静态证据合成为 keep/partial/separate 候选 | synthesis、ownership、schedule | `BoundaryDecisionCandidate` |
| `StaticEvidenceBundleAssembler` | 汇总所有 artifact 和候选设计 | 分区级证据 | `SystemStaticEvidenceBundle` |
| `IntegrationHandoffEmitter` | 输出跨分区 seam 交接 | seam candidates | `IntegrationHandoff` |
| `StaticReportEmitter` | 生成统一 Markdown 报告 | bundle、AI 结果、gaps | System 报告 |
| `ReportStatusGuard` | 阻止静态报告出现最终验收状态 | report AST/metadata | allow/block diagnostics |
| `EvidenceProvenanceExporter` | 生成报告结论到源码/关系/artifact 的反向追溯 | report、catalog | provenance index |

`AiSemanticSynthesisGateway` 可以实现文件模式、标准输入模式或受控 provider adapter；默认实现只产生上下文和验证结构，不把网络调用作为 System 拆分的必需依赖。

## 5. Artifact 和关系契约

### 5.1 Artifact envelope

所有新增 artifact 复用现有 `ArtifactEnvelope`，并要求：

```text
toolKind
artifactKind
schemaVersion
runId
sourceSnapshotId
configurationDigest
inputRoot / outputRoot
contentHash
status
capabilities
inputs / outputs
coverage
diagnostics
legacyCompatibility
```

artifact payload 的顶层状态和内部事实状态分别保存。一个 artifact 可以整体 `partial`，其中一条事实可以是 `confirmed`；整体状态不能被单条事实覆盖。

### 5.2 关系类型扩展

现有 `RelationKind` 继续保留 `Calls`、`Reads`、`Writes`、`Captures`、`Declares`、`References`、`LifecycleCreates`、`LifecycleDestroys`、`DerivedFrom`、`EvidenceFor`、`Transforms` 和 `Supersedes`。为满足 System 拆分模型，补充：

```text
Subscribes
Unsubscribes
Emits
Registers
Serializes
Deserializes
ProjectsTo
Configures
Schedules
Queues
Publishes
Restores
Resets
Rebuilds
```

关系 materializer 只有在 fact payload 提供明确 endpoint 时创建这些边；`OwnerOf`、`MaintainsInvariant`、`SystemBoundary` 不作为静态事实关系自动生成，只作为 AI/设计候选字段保存。

### 5.3 状态传播

沿用 `EvidenceStatus` 的最弱状态传播。任何 canonical binding、dispatch、shard、hash、configuration、coverage 或查询预算 gap 都必须进入结果。`unknown` 不可通过“没有结果”压缩为 confirmed。

## 6. 分析执行 DAG

### 6.1 阶段顺序

```text
scope.manifest
  -> symbol.index
  -> entrypoint.inventory
  -> inbound.relations
  -> dataflow.read-write
  -> called-functions.outbound
  -> dynamic.boundaries
  -> lifecycle.graph
  -> serialization.contract
  -> effect.boundaries
  -> evidence.import
  -> relations.materialize
  -> relation.query.snapshot
  -> write.closure
  -> ownership.candidates
  -> schedule.candidates
  -> cross-partition.seams
  -> coverage.gap-audit
  -> source.closure
  -> ai.context
  -> ai.synthesis
  -> boundary.candidates
  -> bundle.assemble
  -> report.emit
```

### 6.2 Barrier 和失败策略

| Barrier | 依赖 | 失败处理 |
| --- | --- | --- |
| `identity-ready` | scope、symbol | canonical identity 不可用时阻断依赖入口的结论 |
| `static-facts-ready` | inbound、dataflow、calls、dynamic、lifecycle、effect | 保留可用 artifact，缺口传播，不缩小范围 |
| `evidence-ready` | import、materialize、relation snapshot | 身份/hash/index 失败时不得修改 catalog |
| `closure-ready` | write、ownership、schedule、seam、coverage | 关键闭包缺失时阻断 AI 语义确认 |
| `ai-input-ready` | source closure、trace、counter evidence、validator | AI 输入不完整时只输出 blocked/unknown |
| `report-ready` | synthesis、boundary candidates、bundle | 禁止状态升级，报告可交付但保留 gap |

阶段 DAG 是分析流水线 DAG。目标 System 的调用循环和事件回路存入关系图/SCC 诊断，不接入此 DAG。

## 7. AI 输入输出与防幻觉规则

### 7.1 `AiContextPackage`

```text
AiContextPackage
  contextId
  scopeManifest
  sourceClosure
  selectedFacts
  selectedRelations
  coverage
  diagnostics
  evidenceGaps
  counterEvidence
  requiredQuestions
  forbiddenConclusions
  sourceContextDigest
  schemaVersion
```

`selectedFacts` 和 `selectedRelations` 必须受 `QueryBudget`、scope 和 source closure 限制。`forbiddenConclusions` 至少包含：最终 Owner、运行时调度确认、行为等价、迁移成功和删除安全。

### 7.2 模型结果验证

AI 输出必须是结构化 JSON 或等价可解析对象；Markdown 只作为最终呈现格式。校验器检查：

1. 每个 claim 有 evidence ref，或明确为 inference/design/unknown；
2. evidence ref 属于相同 source snapshot/run/configuration 范围；
3. claim 使用的字段在 context package 中存在；
4. `fact-summary` 不得写出 context 中不存在的 source location；
5. 发现 counter evidence 时不得输出无条件 confirmed；
6. `candidateOwner`、`BoundaryDecisionCandidate` 和 `CompositionCandidate` 不得升级为 final owner 或 verified；
7. 缺少关键 reader/writer/caller/lifecycle/effect 时必须输出 gap；
8. 结果中不允许出现 `migration-success`、`deletion-safe` 或未经授权的行为变化隐藏项。

### 7.3 反证优先

`CounterEvidenceCollector` 必须对每个 Owner/边界候选执行反向查询：

```text
candidate writer -> other writers
candidate caller -> unresolved callers
candidate lifecycle -> missing transitions
candidate projection -> writes back to authority
candidate separate boundary -> shared invariant / shared commit
```

如果反证查询无法完成，候选状态为 `conflicted` 或 `unknown`，不能由 AI 自行忽略。

## 8. 报告和交接输出

`StaticEvidenceBundleAssembler` 必须产出：

```text
SystemStaticEvidenceBundle
  scope
  sourceSnapshot
  legacyEntryPoints
  inboundRelations
  outboundRelations
  readWriteSets
  eventAndDelegateRelations
  dynamicRiskSurfaces
  lifecycleCandidates
  effectCandidates
  staticScheduleCandidates
  crossPartitionRelations
  candidateOwners
  candidateApiCompositions
  semanticFindings
  evidenceGaps
```

`StaticReportEmitter` 按以下顺序生成分区报告：

```text
Scope and Evidence
Prior Component Decomposition Reconciliation
Conceptual Behaviors
State Ownership and Write Closure
Boundary Role and Decision
System API and Legacy Behavior Mapping
Call and Dependency DAG
Lifecycle and Side Effects
Integration Handoff
Migration Behavior Contract
Evidence Gaps and Blocking Decisions
Verification Plan
```

报告默认：

```text
designStatus: proposed
verificationStatus: not-run
crossSubsystemOwner: integration-review
```

## 9. 不在本设计内自动化的最终决策

禁止实现以下自动决策器：

```text
FinalOwnerDecider
FinalBoundaryDecider
BehaviorEquivalenceDecider
MigrationSuccessDecider
LegacyDeletionDecider
```

可以实现对应的候选生成器和阻断审计器，但最终决策必须进入项目级验收，且迁移成功必须由真实迁移项目的必需行为测试确认。

## 10. 安全、确定性和恢复

- 所有路径先规范化并验证位于允许 root 内；输出使用 staging 加原子发布；
- stage、事实、关系、诊断和 evidence ref 按稳定 key 排序；
- artifact 内容 hash 漂移时拒绝导入并保留旧 revision；
- 一个阶段失败不把空输出当成 complete；独立阶段可继续，但最终状态是 partial/blocked；
- AI provider 超时、格式错误或不可用时保留 `AiContextPackage`，不生成空的候选设计；
- 重跑使用新的 `runId` 或明确 revision，不能覆盖不同 source snapshot 的证据；
- 工具只写 skill-owned reports/artifacts，不修改源项目、任务表、锁文件或生产代码。

## 11. 完成定义

工具链实现完成表示：

- 所有模型可以在同一 provenance/schema 下序列化和查询；
- 五类主要缺口都有对应 artifact 或明确 `unknown` 输出；
- 所有 artifact 都能通过 `EvidenceImportPipeline` 进入 catalog；
- 关系和分析 DAG 可查询且不混用；
- AI 输入范围可复现，输出有证据引用、反证和禁止状态校验；
- System 报告可以由 bundle 重建，并保留 gap 和 `integration-review` seam。

这不表示：System 边界已经正确、API 已经行为等价、真实项目已接入、迁移成功或旧实现可删除。
