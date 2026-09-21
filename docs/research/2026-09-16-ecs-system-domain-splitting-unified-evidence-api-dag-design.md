# ECS System Domain Splitting 统一证据 API 与三层图模型设计

| 项目 | 内容 |
| --- | --- |
| 文档状态 | `accepted` |
| 研究基线 | [联合证据 API/DAG 研究报告](2026-09-16-ecs-system-domain-splitting-unified-evidence-api-dag-research.md) |
| 研究日期 | 2026-09-16 |
| 目标范围 | `.agents/skills/ecs-system-domain-splitting` 的三个分析/变换工具及其上层查询能力 |
| 当前实现状态 | 部分实现：`AnalysisEvidenceCatalog`、`AnalysisEvidenceApi`、`ExecutionDag` 与 `RelationMultigraph` 已落地，验收用例 K1–K9、K11、K12 通过；`FactClosure`（设计 Task 7）未实现，因此 K10（递归闭包收敛）与 callable 级精确 cleanup 绑定（K7）为显式 `deferred`，不建空类型、不宣称精确绑定，见[决策记录](../../.agents/skills/ecs-system-domain-splitting/system-splitting/references/cleanup-transformation-map-decision.md) |
| 当前验证状态 | 已执行：catalog 与 verifier 串行构建 `0 警告 0 错误`；fixture 套件使 K1–K9、K11、K12 通过，并打印 `DEFERRED: K10 recursive fact closure (deferred:FactClosureNotImplemented)`，测试脚本断言该行存在，K7/K10 不得计入通过；只读集成对照当前 Version4 产物基线且未修改任何源文件或既有报告。命令、退出码与产物路径见[执行计划 §5 台账](2026-09-16-ecs-system-domain-splitting-unified-evidence-api-dag-execution.md) |

## 1. 决策摘要

本设计采纳研究报告的两个结论：

1. 增加一个统一的 `AnalysisEvidenceCatalog` 与 `AnalysisEvidenceApi`，统一登记 artifact、版本、身份、跨工具 binding、诊断和有边界的查询；三个现有工具仍独立运行、独立产出 artifact，不共享不可审计的执行上下文。
2. 不实现“三份 DAG”。统一模型由一个执行依赖 DAG、一个递归事实闭包和一个关系多重图组成：`ExecutionDag`、`FactClosure`、`RelationMultigraph`。它们共享 snapshot、artifact 和查询协议，但不互相伪装。

统一 API 的目标是让调用方能够回答“某个 `SourceSnapshotId` 下，某个 `AnalysisStageId` 产生了哪些可追溯信息，以及这些信息是否完整”，而不是把三个 JSON 文件拼接成更大的 JSON。任何 `partial`、`unresolved`、`ambiguous`、`unknown`、`failed` 或预算截断都必须保留。

## 2. 范围与证据边界

### 2.1 在范围内

- 为三个现有工具建立统一的 run、source snapshot、artifact 和 schema envelope。
- 为跨工具绑定冻结 canonical `CallableKey`、`EntityKey`、`RelationEdgeId` 和 `BindingId`。
- 通过 adapter/reader 读取既有输出，不改变三个工具第一阶段的分析语义。
- 建立显式的 `ExecutionDag`，表达 artifact 产生、转换、授权和失败传播。
- 将调用、读写、捕获、声明、生命周期和证据关系投影为 `RelationMultigraph`。
- 为未来的递归传播保留独立 `FactClosure` 契约。
- 提供 `GetStage`、`GetEntity`、`GetRelations` 和 `Explain` 四类有边界查询。
- 提供 shard 懒加载、分页、预算、取消、cache invalidation 和诊断透传。
- 保持旧 JSON 字段和旧 launcher 的兼容读取，直到删除门禁全部通过。

### 2.2 不在范围内

- 不把 `CalledFunctionsAnalyzer` 改造成入站 caller、事件、反射、配置或生命周期的完整证明器。
- 不把 `ProjectDataflowAnalyzer` 的 `partial` manifest 升级成 `complete`。
- 不把 `FunctionBodyCleanup` 的文件级 hash 映射冒充函数级精确映射。
- 不凭文件名、行号、display symbol 或单一调用者推断唯一语义 owner。
- 不把所有事实放入一个普通 DAG，也不把 `FactClosure` 的递归收敛改写为拓扑排序。
- 不把 cleanup 自动接入生产源码或自动删除旧 facade。
- 不在本阶段创建新的 ECS Component、领域 System 或运行时调度节点；本设计对象是分析工具及其证据协议。

## 3. 现有事实与可信边界

| 工具 | 当前可证明 | 当前不能直接证明 | 联合层处理 |
| --- | --- | --- | --- |
| `CalledFunctionsAnalyzer` | 选定 callable 的直接调用；启用 `--transitive` 时的已解析源代码调用闭包 | 入站 caller、事件/反射入口、配置入口、生命周期和运行时可达性 | 保存 `resolved`、候选和 resolution；空 calls 只有在范围完整且扫描成功时才解释为无匹配 |
| `ProjectDataflowAnalyzer` | callable 的读变量、写变量、数据流进出、捕获变量和诊断；按源文件 shard 输出 | 完整调用闭包、运行时入口、未覆盖的 callable 语义 | 透传 manifest/shard 的 `partial`；缺 shard 不能静默减少结果 |
| `FunctionBodyCleanup` | 原始/清理 snapshot、文件级输入输出 hash、清理计数、诊断和 lineage | 清理前后完整行为等价、函数级 residual mapping、运行时语义 | 在没有 transformation map 时，`CleansBodyOf` 最高为 `partial` |

当前研究报告中的关键基线如下：数据流 manifest 为 `partial`，索引 7855 个 callable、分析 7157 个 callable、967 个源文件并保留 184 个诊断；两次 cleanup run 的清理函数计数分别为 6057 和 6058。因此 run identity、shard 状态和诊断不能从查询结果中省略。

## 4. 设计原则

### 4.1 证据状态与实施状态分离

每个结果同时具有两个维度：

- `EvidenceStatus`：`confirmed`、`partial`、`ambiguous`、`unresolved`、`unknown`、`failed`、`unavailable` 或 `truncated`。
- `ImplementationStatus`：`proposed`、`planned`、`implemented`、`verified` 或 `not-run`。

`confirmed` 不等于已接入生产，`implemented` 也不等于已证明完整。API 不得用一个 `complete` 布尔值覆盖两个维度。

### 4.2 身份先于绑定，绑定先于解释

先建立带 snapshot 范围的 canonical key，再做跨 artifact binding，最后附加 evidence/derivation。`Evidence` 和 `Derivation` 是解释集合，不替代事实或关系身份。

### 4.3 阶段查询必须有边界

`GetStage` 返回的是指定 `RunId`/`SourceSnapshotId`/`AnalysisStageId` 的阶段快照，不是无界的全项目 JSON。事实和关系按 selector、shard、分页和预算读取；预算耗尽必须返回 `truncated` 及原因。

### 4.4 调度关系、事实闭包和语义关系不可混用

- `ExecutionDag` 只表达“哪个阶段依赖哪个 artifact/授权”。
- `FactClosure` 只表达递归事实的 seed、admit、derivation 和 convergence。
- `RelationMultigraph` 只表达带类型、label、context 和来源的关系边。

文件顺序、注册顺序、phase 名称或框架示例不能替代目标工程中的实际调度和证据。

### 4.5 变换必须隔离且可回滚

cleanup 只能读取原始 snapshot、写入独立的 managed run 目录，并在显式授权节点之后执行。源树不被修改；catalog 索引采用临时目录加原子替换；发生失败时恢复到上一个 catalog manifest，而不是删除或覆盖旧 artifact。

## 5. 总体架构

```text
Existing artifact producers
  ├─ CalledFunctionsAnalyzer
  ├─ ProjectDataflowAnalyzer
  └─ FunctionBodyCleanup
          │  independent JSON/shards/manifests
          v
  Artifact readers / adapters
          │  envelope + diagnostics + capability
          v
  AnalysisEvidenceCatalog
    ├─ ArtifactRegistry / SnapshotRegistry
    ├─ CanonicalKeyResolver
    ├─ ExplicitBindingStore
    ├─ ExecutionDag
    ├─ FactClosure       (optional propagation consumer)
    └─ RelationMultigraph
          │
          v
  AnalysisEvidenceApi
    ├─ GetStage
    ├─ GetEntity
    ├─ GetRelations
    └─ Explain
```

### 5.1 组件职责与读写集

| 组件 | Read set | Write set | Emit/副作用 | 所有权边界 |
| --- | --- | --- | --- | --- |
| `CalledFunctionsArtifactReader` | call summary、callable report、resolution diagnostics | 内存中的 call fact、artifact index | `Calls` relation、diagnostic | 不重新运行 analyzer，不补造 caller |
| `ProjectDataflowArtifactReader` | manifest、source-file shard、workspace/analysis diagnostics | 内存中的 dataflow fact、lazy shard index | `Reads`/`Writes`/`Captures` relation、gap | manifest 状态是权威输入；缺 shard 不当作空集合 |
| `FunctionBodyCleanupArtifactReader` | cleanup manifest、original/cleaned snapshot refs、file hash | snapshot lineage 和 file-level transform fact | `Transforms` relation、cleanup diagnostic | 不判断行为等价；无函数 map 时不返回 exact callable binding |
| `ArtifactRegistry` | envelope、content hash、配置 digest | run/artifact/snapshot registry | `Registered` event 或索引记录 | 只登记事实，不修改源文件 |
| `CanonicalKeyResolver` | source snapshot、project path、declaration span、symbol key | canonical key 和 candidate set | `Resolved`/`Ambiguous` binding candidate | 不以 display name 作为主键 |
| `AnalysisEvidenceCatalog` | 所有 reader 的 artifact ref、key 和诊断 | binding、stage index、cache metadata | binding result、evidence trace | 不升级 evidence status，不隐藏未知 |
| `ExecutionDag` | stage descriptors、artifact refs、authorization | stage status、dependency state | failure propagation、barrier | 仅调度 catalog 内的分析/读取/变换操作 |
| `FactClosure` | seed facts、derivation、consumer key policy | admitted facts、SCC/iteration state | convergence result、bounded derivations | 不承担 relation edge identity |
| `RelationMultigraph` | canonical entities、relation records、source refs | edge index、path continuation state | relation/path query result | 同端点不同 relation 不合并 |
| `AnalysisEvidenceApi` | catalog read model、query budget、continuation | query cache metadata | bounded result、query metrics | 只读；不让查询成为第二权威写者 |

## 6. 稳定身份与版本模型

### 6.1 `SourceSnapshotId`

`SourceSnapshotId` 表示一次分析所针对的源代码世界，至少由下列规范化输入的 digest 组成：

```text
SourceSnapshotId = hash(
  ordered source-file relative paths + content digests,
  project-file and reference digests,
  relevant analysis configuration,
  source-root identity/version
)
```

路径排序、路径分隔符、编码和配置序列化必须固定。时间戳、临时目录名和机器绝对路径不能进入语义身份。

### 6.2 其他 key

```text
ArtifactId
  = RunId + ToolKind + ArtifactKind + SchemaVersion + ContentHash

CallableKey
  = SourceSnapshotId
  + ProjectId
  + ProjectRelativePath
  + DeclarationSpan
  + CallableKind
  + CanonicalRoslynSymbolKey

EntityKey
  = SourceSnapshotId + EntityKind + EntityPayload

RelationEdgeId
  = SourceSnapshotId
  + SourceEntityKey
  + TargetEntityKey
  + RelationKind
  + StructuredLabelOrContext
  + StableOrdinal

BindingId
  = RunId + BindingKind + LeftKey + RightKey + BindingRuleVersion
```

`line`/`column`、display symbol、报告文件名和相对路径仍可以作为定位或诊断字段，但不能单独作为跨工具唯一身份。`StableOrdinal` 必须由输入顺序和规范化排序确定，不能使用进程内对象地址或并行完成顺序。

### 6.3 工具输出的兼容演进

第一阶段使用 additive schema：旧字段保留，新 envelope 和 canonical key 作为新字段加入；旧消费者继续读取旧字段，catalog adapter 读取新旧两种版本并明确标记降级状态。

当旧 artifact 没有 `SourceSnapshotId`、symbol key 或 configuration digest 时：

- 可以注册为 legacy artifact；
- 可以用于文件级或报告级导航；
- 不得在没有额外证据时产生 `confirmed` 的跨工具 callable binding；
- 返回 `partial` 或 `unknown`，并附 `MissingIdentityField` gap。

## 7. Artifact envelope

三个工具的顶层输出由 adapter 规范化为以下逻辑 envelope。具体 JSON 字段名可以兼容现有 camelCase schema，但语义必须保持一致。

```text
AnalysisArtifactEnvelope {
  SchemaVersion,
  RunId,
  SourceSnapshotId,
  ToolKind,
  ArtifactKind,
  ProducerVersion,
  ConfigurationDigest,
  InputRoot,
  OutputRoot,
  ContentHash,
  Status,
  Capabilities,
  Inputs: ArtifactRef[],
  Outputs: ArtifactRef[],
  Diagnostics: Diagnostic[],
  Coverage: CoverageDescriptor,
  CreatedAt,
  Compatibility: LegacyCompatibility
}
```

`CreatedAt` 仅用于运维和排序，不进入 `ArtifactId`。`InputRoot`/`OutputRoot` 的绝对路径用于诊断，不作为跨机器 identity。大型 dataflow 内容通过 `ArtifactRef` 指向 shard，不能在 envelope 中内嵌全部 callable。

## 8. Binding 与状态传播

### 8.1 Binding 结果

```text
BindingResult {
  BindingId,
  Left: EntityKey,
  Right: EntityKey,
  Kind: CallableCorrespondence | Reads | Writes | Calls | CleansBodyOf,
  Status: EvidenceStatus = Confirmed | Partial | Ambiguous | Unresolved | Unknown | Failed | Unavailable | Truncated,
  MatchBasis: SymbolKey | DeclarationSpan | SourceHash | CandidateHeuristic,
  Candidates: EntityKey[],
  EvidenceRefs: ArtifactRef[],
  Diagnostics: Diagnostic[],
  InvalidatedBy: SnapshotIdOrConfigurationChange
}
```

### 8.2 状态规则

- `confirmed`：同一 `SourceSnapshotId` 下，canonical symbol 与 declaration anchor 唯一匹配，且必要 artifact 具备足够覆盖。
- `partial`：匹配关系成立，但源 artifact 是 partial、shard 未全部加载或只能证明文件级/阶段级关系。
- `ambiguous`：多个 overload、partial declaration、生成代码或多个候选未消歧。
- `unresolved`：调用工具明确记录 symbol/call 无法解析；必须保留原始候选和诊断。
- `unknown`：当前工具范围没有覆盖该问题，不能把“没有记录”解释成“没有事实”。
- `failed`：产生该 artifact 的操作失败；依赖此 artifact 的 stage 不能假装完成。
- `unavailable`：所需 capability、shard 或输入不可用。
- `truncated`：查询因 budget、分页或取消提前结束；不代表事实不存在。

结果聚合遵循最弱状态传播：任何未解决且影响查询结论的输入都必须在结果中留下 gap。只有查询声明的范围完整、所需 shard 全部读取、stage 状态完成且没有匹配时，空集合才可解释为“范围内无匹配”。

## 9. 三层图模型

### 9.1 `ExecutionDag`

节点是 stage 或 artifact producer，不是每一个 C# callable。建议的初始阶段依赖如下：

```text
SourceSnapshot
  ├─> CallAnalysis -----------┐
  └─> DataflowAnalysis -------┼─> RelationAndEvidenceBinding
                              │          │
                              │          v
                              └────> DecisionOrAuthorization
                                             │
                                             v
                                      CleanupTransformation
                                             │
                                             v
                                       CleanedSnapshot
```

`CallAnalysis` 和 `DataflowAnalysis` 可以并行，但 binding 必须等待它们声明的输入。cleanup 必须等待显式 `DecisionOrAuthorization`，不能因为分析结果存在就自动执行。

每个 stage 至少记录：

- `StageId`、`RunId`、`SourceSnapshotId`；
- 输入/输出 `ArtifactRef`；
- required capability 与实际 capability；
- `Status`、失败原因、诊断和覆盖范围；
- must-before/must-after、barrier、是否可重跑；
- 失败后的 downstream 状态和回滚点。

### 9.2 `FactClosure`

`FactClosure` 只用于确实存在递归传播的 consumer。内部采用 worklist/fixed-point/SCC，外部返回：

- seed facts；
- canonical/admitted facts；
- `Derivation`/provenance；
- iteration、depth 和 SCC 信息；
- convergence status；
- source resolution fallback 和诊断。

payload 是否进入 fact identity 必须由 consumer 矩阵决定：如果 payload 会改变后续 decision、relation 或安全证明，必须进入 `FactKey` 或成为明确 discriminator；如果只影响解释，可以放入 derivation。不能因为“字段看起来相同”自动合并。

### 9.3 `RelationMultigraph`

以下关系是独立 edge，即使端点相同也不能按 node pair 合并：

```text
Calls
Reads
Writes
Captures
Declares
References
LifecycleCreates
LifecycleDestroys
DerivedFrom
EvidenceFor
Transforms
Supersedes
```

`RelationEdgeId` 必须包含 relation kind、structured label/context 和稳定 ordinal。path identity 至少包含 source identity 与 edge sequence；不能只按 node sequence 去重，否则会压掉同端点不同语义边或不同 source 汇聚路径。

关系查询需要明确区分：node reachability、edge materialization、edge-distinct path、source-preserving continuation 和 evidence explanation。

## 10. API 契约

### 10.1 入口

```text
GetStage(StageQuery) -> StageSnapshotResult
GetEntity(EntityQuery) -> EntityResult
GetRelations(RelationQuery) -> RelationResult
Explain(BindingId or RelationEdgeId) -> EvidenceTrace
```

### 10.2 `StageQuery`

```text
StageQuery {
  RunId: optional,
  SourceSnapshotId: required,
  StageId: required,
  Subject: optional EntitySelector,
  RelationKinds: optional set,
  EvidenceStatuses: optional set,
  Include: Metadata | Artifacts | Facts | Relations | Diagnostics | Derivations,
  Budget: MaxItems + MaxBytes + MaxShardLoads + Cancellation,
  ContinuationToken: optional
}
```

`RunId` 缺省时只能在 catalog 中存在唯一候选时解析；有多个 run 必须返回 `ambiguous`，不能选“最新的一次”作为隐式语义。

`StageId` 是字符串键，不是编译期枚举。当前实现登记的 stage id 为 `DataflowAnalysis`、`CallAnalysis`、`CallAnalysisDetailed`、`FunctionBodyCleanup`；同一 stage 的第二次运行以独立 `RunId` 登记，不合并。`PerformanceStageId` 是否映射到 `AnalysisStageId` 仍是 §15 的开放问题。

所有结果的 `Status` 统一使用 §4.1 的同一个 `EvidenceStatus` 词汇表：`BindingResult`、`StageSnapshotResult` 与逐条 fact 共用它，不另设 `Complete` 值。需要表达“完整”时用 `confirmed`——最弱状态传播要求单一有序词汇表。

### 10.3 结果

```text
StageSnapshotResult {
  Stage: StageDescriptor,
  Status: EvidenceStatus = Confirmed | Partial | Ambiguous | Unresolved | Unknown | Failed | Unavailable | Truncated,
  Metadata: StageMetadata,
  Inputs: ArtifactRef[],
  Outputs: ArtifactRef[],
  Facts: FactRef[],
  Relations: RelationRef[],
  Diagnostics: Diagnostic[],
  Unknowns: EvidenceGap[],
  Counts: StageCounts,
  ContinuationToken: optional,
  QueryMetrics: QueryMetrics
}
```

首屏查询应优先返回 metadata、counts、diagnostics 和 artifact refs；事实/关系按 continuation 或 selector 加载。缓存 key 必须至少包含 `SourceSnapshotId`、stage、query profile、selector、capability、budget 和 API schema version。

### 10.4 `Explain`

`Explain` 返回一条绑定或关系的完整证据链：

- 左右实体 key 和 relation/binding kind；
- 参与的 artifact、shard 和 content hash；
- match basis、候选、诊断和 status 变化；
- derivation/source mark（若有）；
- invalidation 条件；
- 当前结论不能证明的内容和最小补证据。

解释输出不能把推导结论写成原始代码事实；`inferred`、`proposed` 与 `confirmed` 必须分栏或分字段表达。

## 11. 生命周期、缓存与副作用协议

### 11.1 注册与读取生命周期

1. 创建 `SourceSnapshotId`，登记输入 digest 和分析配置。
2. 每个工具独立创建 `RunId`，完成后写 envelope/manifest。
3. reader 先校验 schema、content hash、snapshot 一致性和 capability，再注册 artifact。
4. catalog 生成 key、候选和显式 binding；任何缺口继续向上暴露。
5. API 仅从 catalog read model 查询；lazy shard 读取必须受 budget 和 cancellation 控制。
6. source/configuration/schema 改变时使相关 binding 和 cache 标记 `invalidated`，不能继续复用旧结果。

### 11.2 变换和唯一提交者

- analyzer 输出是不可变输入；catalog 不回写 analyzer 原始输出。
- cleanup 只对显式授权的 snapshot 执行，写到独立 managed run；源树 hash 在前后必须一致。
- catalog index 使用临时目录生成，通过同卷原子替换成为当前版本；失败时保留旧 index。
- query cache 只能缓存结果，不拥有权威事实；缓存失效不能改变 artifact。
- 每一个对外可见 cleanup 输出只有一个提交者；shadow 或比较路径只能写差异记录，不能同时提交第二份状态或副作用。

### 11.3 回滚

回滚记录必须包含：

- 当前与上一版本的 catalog manifest、schema 和 content hash；
- 被替换的 index/metadata 路径；
- 未完成的 shard load、队列和订阅；
- cleanup managed run 的 `OriginalSnapshotRoot`、`CleanedSnapshotRoot` 和 run ID；
- 已提交不可逆效果（如果未来存在）的补偿边界。

回滚通过重新指向上一份完整 manifest 恢复查询视图；不删除原始 artifact，不把 feature flag 切换当作状态回滚的充分证明。

## 12. 实现阶段与门禁

| 阶段 | 交付 | 进入条件 | 退出门禁 |
| --- | --- | --- | --- |
| P0 | schema/consumer inventory | 已锁定当前工具输出和 fixture | 旧字段、manifest 状态、未知项均有清单 |
| P1 | artifact envelope/registry | P0 完成 | 三工具可注册；旧 JSON 仍可读；hash 与 snapshot 可重现 |
| P2 | canonical key | P1 完成 | overload、partial、lambda、local function fixture 不碰撞 |
| P3 | explicit binding/catalog | P2 完成 | exact/candidate/unknown/unresolved 均可解释，partial 不升级 |
| P4 | `ExecutionDag` | P3 完成 | stage 依赖、barrier、失败传播和 cleanup authorization 可验证 |
| P5 | `RelationMultigraph` | P3 完成 | 同端点不同 relation 保留；edge/path identity 稳定 |
| P6 | `GetStage`/bounded query | P4/P5 完成 | shard、分页、budget、cache invalidation 和 continuation 可验证 |
| P7 | cleanup transformation map | P4 完成且有真实 consumer | 无 map 时明确 partial；有 map 时 exact fixture 通过 |
| P8 | 集成与灰度 | P1-P7 的必需项完成 | 真实 artifact 读取、回滚和删除门禁审查通过 |

`FactClosure` 是条件阶段：只有在真实 consumer 确认存在递归传播、payload identity 矩阵和 provenance 需求后才实现；在此之前只冻结接口和 unknown/deferred 记录，不为“完整性”预建空实现。

## 13. 验收矩阵

| 编号 | 场景 | 必须证明 |
| --- | --- | --- |
| K1 | 同一 callable 出现在 call/dataflow artifact | canonical `CallableKey` exact bind；display name 不参与唯一性 |
| K2 | overload、partial declaration、lambda、local function | key 不碰撞；候选和 ambiguous 可解释 |
| K3 | unresolved call | 保留 `unresolved`、候选和诊断；不能变成空调用集 |
| K4 | dataflow manifest 为 partial | stage 返回 partial 和 gap；不能返回 complete |
| K5 | shard 缺失/不可用 | 返回 unavailable/unknown 及 shard ref；不静默减少结果 |
| K6 | 同一 source root 的两个 cleanup run | run/snapshot 不同；不混合不同清理计数或 artifact |
| K7 | cleanup 前后声明位置变化 | 没有 map 只能 partial；有 map 才 exact |
| K8 | 同端点 `Calls` 与 `Reads` | 返回两条 edge；不按 node pair 去重 |
| K9 | 两个 source 汇聚到同一节点 | path 保留 source identity；不错误剪枝 |
| K10 | 递归传播 | closure convergence 和 derivation 可查询；不要求 DAG 拓扑序 |
| K11 | query budget 达到上限 | 返回 truncated/reason；不能误报 complete |
| K12 | source/configuration 改变 | 旧 binding/cache 标记 invalidated，不继续复用 |

除 K10 外，P1-P6 的实现不应依赖传播算法；除 K7 外，文件级 cleanup lineage 不能宣称 callable 级 exact binding。

## 14. 反对的方案

### 14.1 三个工具合并成一个共享执行上下文

拒绝原因：工具的可信边界、失败模式和输出粒度不同；共享隐式状态会让 artifact lineage、重跑和审计不可见。catalog 通过显式 artifact ref 连接三个工具，保留各自可独立重跑的能力。

### 14.2 把所有信息压成 `GetAll(stage)`

拒绝原因：无法控制 7855 callable/967 shard 的内存和响应大小，也无法区分当前没有加载 shard 与确实没有事实。采用 typed selector、lazy shard、分页和预算。

### 14.3 仅使用一个 DAG

拒绝原因：递归 fact closure 可能有 SCC，relation multigraph 允许环和同端点多边；用 DAG 拓扑序表示它们会丢失 provenance、edge identity 或收敛状态。

### 14.4 以文件路径、行号或 display symbol 直接绑定

拒绝原因：重排、overload、partial、生成代码和 cleanup 变换都会使这些定位不稳定。它们只能作为辅助定位，canonical key 必须包含 snapshot、declaration anchor 和 symbol identity。

### 14.5 以“编译通过”作为删除门禁

拒绝原因：编译不能证明入站 caller、动态入口、生命周期、网络/存档、失败/重试和唯一副作用提交者。删除旧实现必须满足研究报告和证据协议规定的完整删除门禁。

## 15. 未决问题与最小补证据

1. `CalledFunctionsAnalyzer` 是直接升级 JSON schema，还是由 adapter 生成 canonical key；需要一次 schema compatibility 评估和旧消费者扫描。
2. `ProjectDataflowAnalyzer` 在 partial/生成代码/多项目引用场景下如何合并 symbol、span 和 source location；需要专门 fixture。
3. `FunctionBodyCleanup` 是否需要函数级 transformation map；如果要提供 callable 级 `GetStage`，这是 cleanup exact binding 的最小补件。
4. `PerformanceStageId` 是否只保留为 telemetry namespace，还是显式映射到新的 `AnalysisStageId`；需要确认现有消费者是否依赖枚举值。
5. propagation payload 是否改变 downstream decision；需要 consumer 矩阵后再冻结 `FactKey`。
6. 入站 caller、事件订阅、反射、配置和生命周期入口仍不在现有 call analyzer 的证明范围；若删除门禁依赖这些事实，必须另建 analyzer 或补人工/运行时证据。

## 16. 参考

- [联合证据 API/DAG 研究报告](2026-09-16-ecs-system-domain-splitting-unified-evidence-api-dag-research.md)
- [ECS System 领域拆分技能](../../.agents/skills/ecs-system-domain-splitting/SKILL.md)
- [系统拆分规则](../../.agents/skills/ecs-system-domain-splitting/system-splitting/references/system-decomposition-rules.md)
- [证据循环与迁移协议](../../.agents/skills/ecs-system-domain-splitting/system-splitting/references/evidence-and-migration-protocol.md)
- [输出风险约束](../../.agents/skills/ecs-system-domain-splitting/reports/output-risk-profile.md)
- [构建与验证约束](../../Context/约束/构建与验证约束.md)
