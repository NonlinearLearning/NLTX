# System 拆分静态分析模型与 AI 工具实现计划

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** 实现一套复用现有 `AnalysisEvidenceCatalog` 的 System 拆分静态分析、证据闭包、AI 上下文和候选边界工具，使入站入口、动态入口、生命周期、写入闭包和跨分区 seam 都能形成可追溯 artifact。

**Architecture:** 共享不可变模型和 provenance 继续位于 `AnalysisEvidenceCatalog`，Roslyn 静态事实分析位于新的 `SystemDecomposition.Analysis`，AI 工具位于 provider-neutral 的 `SystemDecomposition.Ai`，统一 CLI 负责显式范围、阶段 DAG、artifact 导入和报告发布。AI 只消费有界证据并输出带引用的候选设计，不直接修改源码、关系事实或最终验收状态。

**Tech Stack:** C# `net10.0`、nullable reference types、`System.Text.Json`、Roslyn `4.14.0`、现有 `SourceAnalysisInfrastructure`、`AnalysisEvidenceCatalog`、`RelationMultigraph`、`ExecutionDag`、`AnalysisEvidenceApi`，以及仓库的串行构建包装器（实现阶段使用，当前计划阶段不执行）。

**Design:** [`System 拆分静态分析模型与 AI 工具设计`](2026-09-19-system-decomposition-analysis-models-and-ai-tools-design.md)

---

## 执行约束

- 模型、静态 analyzer、AI 工具和统一 CLI 已按本计划落地到
  `.agents/skills/ecs-system-domain-splitting/tools/`；本文件现在同时作为实现状态和后续验证计划。
- 用户已经确定的边界保持不变：System 拆分阶段不编写测试代码、不创建行为 fixture/verifier、
  不运行测试或构建；项目最终验收仍由 [项目最终验收与迁移门禁](../../system-decomposition/2026-09-19-project-final-acceptance-and-migration-gates.md) 负责。
- 本轮只做源码级静态检查、文档链接检查和 diff 检查；没有声明 CLI、Roslyn analyzer、
  artifact reader 或 catalog revision 已编译或运行通过。
- 工具只能写入 `.agents/skills/ecs-system-domain-splitting/tools/` 及 skill-owned artifact output root，
  不修改 `src/`、`Test/`、Version4 源项目、分区任务表或锁文件。
- 不为 `FinalOwnerDecider`、`FinalBoundaryDecider`、`BehaviorEquivalenceDecider`、
  `MigrationSuccessDecider` 或 `LegacyDeletionDecider` 建立自动决策器。
- 每个工具输出都必须区分事实、推导、候选设计和 `unknown`，不可把 AI 输出直接升级为
  `confirmed` 或 `verified`。

## 目标目录

实现和后续验证使用以下目录：

```text
.agents/skills/ecs-system-domain-splitting/tools/
  AnalysisEvidenceCatalog/Model/              # 共享模型扩展
  AnalysisEvidenceCatalog/Artifacts/           # artifact readers / facts
  AnalysisEvidenceCatalog/Graphs/              # relation kinds / graph diagnostics
  AnalysisEvidenceCatalog/Import/              # 已有导入和物化管线扩展
  AnalysisEvidenceCatalog/Query/               # 已有有界 API 扩展
  SourceAnalysisInfrastructure/                # 已有 Roslyn/MSBuild 基础设施
  SystemDecomposition.Analysis/                # 新增静态分析程序集
  SystemDecomposition.Ai/                      # 新增 AI 上下文和候选程序集
  SystemDecomposition.Cli/                     # 新增统一 CLI
```

## 阶段依赖总览

```text
M0 共享模型与身份契约
  -> M1 范围、符号和入口事实
  -> M2 读写、调用、动态、序列化、生命周期和 effect 事实
  -> M3 证据导入、关系物化和 catalog 查询
  -> M4 写入闭包、Owner 候选、调度候选和跨分区 seam
  -> M5 API 概念行为和有界源码闭包
  -> M6 AI 上下文、结构化归纳和边界候选
  -> M7 bundle、交接、报告和 CLI 编排
```

M2 内部可以并行实现，但所有工具必须先依赖 M0 的身份、状态和 artifact envelope；M6 必须等待 M3、M4、M5 的覆盖和缺口结果。

## Task 1: 固化共享 provenance、范围和状态模型

**状态：源码已落地，未编译/未运行；本轮仅完成源码级检查。**

**Files:**

- Create: `.agents/skills/ecs-system-domain-splitting/tools/AnalysisEvidenceCatalog/Model/AnalysisContext.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/AnalysisEvidenceCatalog/Model/EvidenceProvenance.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/AnalysisEvidenceCatalog/Model/SystemDecompositionScopeManifest.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/AnalysisEvidenceCatalog/Model/CoverageModels.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/AnalysisEvidenceCatalog/Model/StaticDesignStatus.cs`
- Modify: `.agents/skills/ecs-system-domain-splitting/tools/AnalysisEvidenceCatalog/Model/ImplementationStatus.cs`

**Steps:**

1. 将 `runId`、`sourceSnapshotId`、`configurationDigest`、project/configuration/platform、tool version 和 schema version 组织为不可变 `AnalysisContext`。
2. 实现 `EvidenceProvenance`，使每条事实、关系、AI claim 和报告结论都可以引用 artifact、位置、query scope、状态、诊断和 gap reason。
3. 实现 `SystemDecompositionScopeManifest`，覆盖 profile、partition、session、claimed members、allowed roots、excluded paths、prior artifacts、query scope 和 output root。
4. 实现 `CoverageMatrix`、`CoverageDimension`、`CoverageFinding` 和 `MinimumNextEvidence`；覆盖成员、入口、调用、读写、动态、生命周期、effect、调度、序列化、跨分区和 API。
5. 增加仅用于静态设计的状态模型，例如 `proposed`、`conflicted`、`unknown`、`static-partial`；不得重用 `Verified` 或 `MigrationSuccess` 表示静态完成。
6. 所有集合使用只读接口；所有状态和枚举使用稳定 JSON 名称；序列化契约与现有 `ArtifactEnvelope` 的 `System.Text.Json` 设置一致。

**设计要求：** 不能把 `ObservedItems = 0` 解释为无关系；覆盖记录必须同时携带搜索范围、工具、排除项和 gap。

## Task 2: 扩展事实、关系和 artifact 契约

**状态：源码已落地，未编译/未运行；新增 static-run status constraint 仍需工具验证。**

**Files:**

- Create: `.agents/skills/ecs-system-domain-splitting/tools/AnalysisEvidenceCatalog/Model/SourceLocation.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/AnalysisEvidenceCatalog/Model/QueryScope.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/AnalysisEvidenceCatalog/Model/AnalysisFact.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/AnalysisEvidenceCatalog/Model/ArtifactCapability.cs`
- Modify: `.agents/skills/ecs-system-domain-splitting/tools/AnalysisEvidenceCatalog/Graphs/RelationKind.cs`
- Modify: `.agents/skills/ecs-system-domain-splitting/tools/AnalysisEvidenceCatalog/Artifacts/EvidenceFact.cs`
- Modify: `.agents/skills/ecs-system-domain-splitting/tools/AnalysisEvidenceCatalog/Import/EvidenceArtifactDescriptor.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/AnalysisEvidenceCatalog/Import/EvidenceArtifactStatusConstraint.cs`
- Modify: `.agents/skills/ecs-system-domain-splitting/tools/AnalysisEvidenceCatalog/Import/EvidenceImportPipeline.cs`
- Modify: `.agents/skills/ecs-system-domain-splitting/tools/AnalysisEvidenceCatalog/Import/EvidenceArtifactReaderRegistry.cs`
- Modify: `.agents/skills/ecs-system-domain-splitting/tools/AnalysisEvidenceCatalog/Import/RelationMaterializer.cs`

**Steps:**

1. 保留现有 `EvidenceFact` 的向后兼容读取，同时为新 artifact 提供结构化 `AnalysisFact`，明确 subject、target、fact kind、dispatch、closure eligibility 和 provenance。
2. 在 `RelationKind` 中增加 `Subscribes`、`Unsubscribes`、`Emits`、`Registers`、`Serializes`、`Deserializes`、`ProjectsTo`、`Configures`、`Schedules`、`Queues`、`Publishes`、`Restores`、`Resets` 和 `Rebuilds`。
3. 为每种新 fact kind 增加明确的 endpoint 读取规则；缺少 endpoint 时只产生 gap，不允许使用路径、行号或方法名猜测关系。
4. 为新增 producer/artifact kind 注册显式 reader；禁止 reader registry 通过目录递归搜索自动发现 artifact。
5. 保持 `EvidenceStatusAggregation.Weakest`、artifact hash、snapshot、configuration digest 和 legacy compatibility 规则。
6. 让 `RelationMaterializer` 只物化有明确 endpoint 的边；`OwnerOf`、`MaintainsInvariant` 和 `SystemBoundary` 保持候选设计字段，不作为静态关系自动生成。

**设计要求：** 这一步只扩展证据模型和导入边界，不实现具体源码扫描器。

## Task 3: 实现范围构建和全项目 canonical symbol index

**状态：源码已落地，未编译/未运行。**

**Files:**

- Create: `.agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Analysis/SystemDecomposition.Analysis.csproj`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Analysis/Scope/ScopeManifestBuilder.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Analysis/Scope/ScopeManifestValidator.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Analysis/Symbols/ProjectSymbolIndex.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Analysis/Symbols/SymbolIndexBuilder.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Analysis/Symbols/SymbolIndexArtifactWriter.cs`
- Modify: `.agents/skills/ecs-system-domain-splitting/tools/SourceAnalysisInfrastructure/SourceAnalysisSymbolKey.cs` only when a narrow shared helper is required

**Steps:**

1. 使 `ScopeManifestBuilder` 从显式分区任务、输入报告、source root、project、configuration、platform 和输出目录生成 manifest。
2. 校验 allowed roots、excluded paths、输入 artifact 的 source identity 和 output path；拒绝超出 root 的路径和跨 snapshot artifact。
3. 复用 `SourceAnalysisInfrastructure` 加载 Roslyn compilation，建立类型、字段、属性、方法、事件、委托、重载、接口和 override 的 canonical index。
4. 为每个 callable 保存 canonical symbol key、声明位置、callable kind、containing type、可见性和 partial/generated 标记。
5. 以稳定路径、声明 span、kind 和 canonical key 排序，原子写入 symbol artifact。
6. 将 workspace/compilation/parse diagnostics 写入 artifact，不把诊断后的不完整 index 标记为 complete。

**设计要求：** 范围外成员只能作为显式 source closure 读取，不得改变当前分区的主范围或自动成为 Owner 事实。

## Task 4: 实现旧入口、入站调用和事件/委托注册分析

**状态：源码已落地，未编译/未运行；动态/框架注册仍属于静态 gap。**

**Files:**

- Create: `.agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Analysis/EntryPoints/LegacyEntryPointInventory.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Analysis/EntryPoints/InboundEntryPointAnalyzer.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Analysis/EntryPoints/EventAndDelegateRegistrationAnalyzer.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Analysis/EntryPoints/EntryPointArtifactWriter.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/AnalysisEvidenceCatalog/Artifacts/EntryPointArtifactReader.cs`
- Modify: `.agents/skills/ecs-system-domain-splitting/tools/AnalysisEvidenceCatalog/Import/EvidenceArtifactReaderRegistry.cs`

**Steps:**

1. 从 symbol index 盘点 public/protected/internal、overload、显式接口、virtual/override/abstract、构造函数、工厂、静态初始化和服务注册入口。
2. 通过 Roslyn `FindReferences`/语义绑定和显式注册模式收集 inbound callers、接口实现、override dispatch 候选和构造/工厂调用者。
3. 分析事件 add/remove、订阅/取消订阅、委托保存、委托传递和委托调用；保存注册点与 callback identity。
4. 对无法唯一解析的 virtual/interface/delegate/dynamic/reflection 入口输出候选和 partial/unresolved gap。
5. 生成 `EntryPointFact`、`InboundRelationArtifact` 和 `EventDelegateRelations`，绑定同一 source snapshot 和 configuration digest。
6. 将 artifact reader 加入 catalog registry，并把 `Calls`、`Subscribes`、`Unsubscribes`、`Registers` 和 `Emits` 交给 relation materializer。

**设计要求：** “没有反向引用命中”只能表达在声明搜索范围内未找到静态 caller，不得表达运行时没有入口。

## Task 5: 实现行为表面和外部边界分析

**状态：源码已落地，未编译/未运行；行为表面只形成候选，不是行为等价证明。**

**Files:**

- Create: `.agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Analysis/Behavior/BehaviorSurfaceAnalyzer.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Analysis/Behavior/ApiPreconditionAnalyzer.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Analysis/Behavior/ApiFailurePathAnalyzer.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Analysis/Behavior/BehaviorSurfaceArtifactWriter.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/AnalysisEvidenceCatalog/Artifacts/BehaviorSurfaceArtifactReader.cs`

**Steps:**

1. 对每个选定入口提取参数、默认值、guard、分支、return、异常、部分写入、queue 操作、事件、网络/存档/日志调用和 clock/random/I/O 读取。
2. 以 callable canonical key 保存语法位置和被调用者 evidence refs，不复制完整源码 body 到事实 artifact。
3. 区分显式业务状态写入、缓存刷新、投影输出、适配转换和外部 effect 候选。
4. 记录失败路径是否已经改变计数、队列、订阅、缓存或事件序列。
5. 生成 `BehaviorSurfaceCandidate`，但将完整语义归纳留给 AI 层。
6. 为旧 API 分支保留默认值、身份、权限、缺失组件、实体死亡和 overload 分流信息。

## Task 6: 实现动态入口、序列化和 effect 边界扫描

**状态：源码已落地，未编译/未运行；动态目标和实际触发频率仍需运行时证据。**

**Files:**

- Create: `.agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Analysis/Dynamic/DynamicBoundaryScanner.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Analysis/Dynamic/RegistrationPatternCatalog.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Analysis/Analysis/SerializationContractAnalyzer.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Analysis/Analysis/EffectBoundaryAnalyzer.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Analysis/Artifacts/DynamicRiskArtifactWriter.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Analysis/Artifacts/SerializationArtifactWriter.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Analysis/Artifacts/EffectArtifactWriter.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/AnalysisEvidenceCatalog/Artifacts/DynamicRiskArtifactReader.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/AnalysisEvidenceCatalog/Artifacts/SerializationArtifactReader.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/AnalysisEvidenceCatalog/Artifacts/EffectArtifactReader.cs`

**Steps:**

1. 扫描 reflection、`Type.GetType`、字符串注册、配置 key、生成代码、脚本、native/plugin、Timer/Task 和动态加载入口。
2. 扫描网络 packet、存档、序列化 attribute/key、版本字段、编码/解码、恢复和迁移入口。
3. 分类 file、network、persistence、logging、metrics、random、clock、queue、cache 和 irreversible effect。
4. 对每个动态表面记录 lookup key、候选 target、解析状态、触发上下文和缺失的 runtime evidence。
5. 物化 `Configures`、`Serializes`、`Deserializes`、`Restores`、`Emits` 和 effect 关系；不将字符串命中当成唯一 target。
6. 将生成代码和外部插件作为风险面输出，即使本地源码没有静态调用者。

## Task 7: 实现生命周期图和状态转移候选

**状态：源码已落地，未编译/未运行；生命周期闭合仍是待补证据项。**

**Files:**

- Create: `.agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Analysis/Lifecycle/LifecycleGraphAnalyzer.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Analysis/Lifecycle/LifecyclePatternCatalog.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Analysis/Lifecycle/LifecycleTransitionExtractor.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Analysis/Artifacts/LifecycleArtifactWriter.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/AnalysisEvidenceCatalog/Artifacts/LifecycleArtifactReader.cs`

**Steps:**

1. 建立 create、activate、update、end、destroy、rebuild、reset、unload、persist、restore、network attach/detach 和 multi-world/session transition pattern。
2. 将构造、初始化、注册、实体槽位变化、事件取消订阅、存档恢复和重建 helper 绑定到 canonical entity/callable。
3. 形成 `LifecycleTransitionCandidate`，包含 current state、trigger、preconditions、state delta、next state、effect、failure 和 rollback candidate。
4. 对只找到 create/update 而缺少 destroy/reset/unload 的情况生成 lifecycle gap，不把生命周期标记为 closed。
5. 识别重入、重复激活、重复销毁和实体已不存在等风险，并把它们交给 API failure/AI 上下文。

## Task 8: 接入现有 dataflow/called-functions 并构建写入闭包

**状态：外部 analyzer 编排和 artifact 导入已落地，未编译/未运行；多成员 CalledFunctions
覆盖和基于外部 shard 的完整写入闭包仍保留 partial gap。**

**Files:**

- Create: `.agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Analysis/Closure/WriteClosureAnalyzer.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Analysis/Closure/CallAndWriteClosureResolver.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Analysis/Closure/StateRoleClassifier.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Analysis/Ownership/InvariantCandidateAnalyzer.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Analysis/Ownership/OwnerCandidateAnalyzer.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Analysis/Ownership/CommitPointCandidateExtractor.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Analysis/Artifacts/WriteClosureArtifactWriter.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/AnalysisEvidenceCatalog/Artifacts/WriteClosureArtifactReader.cs`

**Steps:**

1. 通过明确 stage descriptor 消费现有 `ProjectDataflowAnalyzer` 和 `CalledFunctionsAnalyzer` artifact。
2. 将直接写、静态 helper 写、事件/队列写、恢复写、重建写、reset 写和缓存写按 resource key 聚合。
3. 对出站 closure 只纳入 static 且唯一绑定的调用；virtual/interface/delegate/dynamic/reflection 保留 gap。
4. 将状态分为 authoritative、derived、cache、snapshot、compatibility、presentation 和 unknown 候选。
5. 从 guard、联合读写、异常路径和提交调用中提取 invariant candidate；不能把字段名或写入次数当作不变量证据。
6. 输出 `candidateOwner`、`candidateCommitPoint`、`ownerConflict` 和 `unknownWriter`，不输出最终 Owner。

当前实现已经把两个外部 analyzer 注册为 static-run artifact 并纳入导入、关系和状态传播；
`WriteClosureAnalyzer` 的本地 Roslyn resolver 尚未把所有外部 dataflow shard 逐 callable 合并，
因此多成员/跨 shard 的写入闭包必须保持 `partial`，不能按本地分析结果声明完整。

## Task 9: 实现调度候选、SCC 和跨分区 seam

**状态：源码已落地并接入 bundle/report，未编译/未运行；跨分区协调 owner 仍固定为
`integration-review`，不是最终 owner。**

**Files:**

- Create: `.agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Analysis/Scheduling/ScheduleCandidateExtractor.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Analysis/Graphs/RelationCycleAndSccAnalyzer.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Analysis/CrossPartition/CrossPartitionSeamAnalyzer.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Analysis/CrossPartition/IntegrationHandoffBuilder.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/AnalysisEvidenceCatalog/Model/GraphDiagnostic.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/AnalysisEvidenceCatalog/Artifacts/ScheduleArtifactReader.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/AnalysisEvidenceCatalog/Artifacts/IntegrationHandoffArtifactReader.cs`
- Modify: `.agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Cli/Execution/AnalysisPipelineRunner.cs` for partition manifest identity checks and bundle handoff wiring

**Steps:**

1. 从显式调用顺序、注册顺序、phase 名称、barrier、queue flush、commit/publish 和 visibility hint 提取 `ScheduleCandidate`。
2. 将目标调用图、事件关系图和生命周期图做 SCC 分析；递归、互调和事件回路记录为 `GraphDiagnostic`，不删除边。
3. 读取多个分区 manifest 和 catalog relation snapshot，查找共享 entity ID、Component、snapshot、event、network field、persistence key 和公共 service。
4. 生成 `IntegrationHandoff`，包括 producer、consumer、ordering、lifecycle、version、candidate owners、conflict 和 minimum next evidence。
5. 强制给跨分区 seam 附加 `crossSubsystemOwner: integration-review`。
6. 不把 `ExecutionDag` 的拓扑顺序写成目标项目 phase/barrier 事实。

## Task 10: 实现概念 API、重试/幂等和有限源码闭包

**状态：源码已落地，未编译/未运行；API 一一对应和行为一致性必须由项目最终行为测试确认。**

**Files:**

- Create: `.agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Analysis/Api/LegacyApiInventoryAnalyzer.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Analysis/Api/RetryAndIdempotencyScanner.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Analysis/Api/ConceptBehaviorCandidateExtractor.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Analysis/Api/LegacyNewCompositionMapper.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Analysis/Api/ObservationVectorBuilder.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Analysis/Closure/BoundedSourceClosurePackager.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/AnalysisEvidenceCatalog/Model/ApiBehaviorModels.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/AnalysisEvidenceCatalog/Model/SourceClosureModels.cs`

**Steps:**

1. 汇总旧入口到 `ConceptId` 候选，允许一个旧入口映射多个新 API，也允许多个 overload/事件/回调入口归并到一个 ConceptId。
2. 提取 request/correlation/idempotency key、去重表、重试循环、重复事件和过期范围候选。
3. 生成 `ConceptBehaviorCandidate`，包含输入身份、前置条件、读取、写入、不变量、返回/错误、事件/effect、顺序、可见性、生命周期和 gap。
4. 生成 `LegacyEntryPoint -> ConceptId -> CanonicalNewCompositionCandidate` 映射，不要求方法签名相同。
5. 生成 `ObservationContract`，只列未来最终验收所需的观察轴和场景，不运行行为比较。
6. 根据 manifest、canonical symbol、relation path、entrypoint 和 evidence budget 打包 `SourceClosureManifest`；超出范围必须失败或记录 gap。

## Task 11: 实现证据覆盖审计和静态 bundle 组装

**状态：源码已落地，未编译/未运行；coverage 结果只代表声明范围内的静态覆盖。**

**Files:**

- Create: `.agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Analysis/Coverage/CoverageAndGapAuditor.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Analysis/Coverage/NegativeConclusionAuditor.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Analysis/Bundle/StaticEvidenceBundleAssembler.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/AnalysisEvidenceCatalog/Model/SystemStaticEvidenceBundle.cs`
- Modify: `.agents/skills/ecs-system-domain-splitting/tools/AnalysisEvidenceCatalog/Query/AnalysisEvidenceApi.cs` only for required bounded query fields

**Steps:**

1. 审计每个分区的成员、入口、出站、入站、读写、动态、生命周期、副作用、序列化、调度、跨分区和 API 覆盖。
2. 对“未发现”“未加载”“未解析”“不适用”和“排除”使用不同状态，禁止将其折叠为 empty/complete。
3. 记录 `QueryBudget`、cursor、shard hash、legacy schema、binding ambiguity 和 source closure 缺失。
4. 检查关系 endpoint、source snapshot、run 和 configuration digest 是否一致。
5. 组装完整 `SystemStaticEvidenceBundle`，并将所有 artifact refs、gaps、diagnostics 和候选设计保留下来。
6. 对跨分区字段只生成 handoff，不自动完成 owner reconciliation。

## Task 12: 实现 AI 证据上下文和反证工具

**状态：源码已落地，未编译/未运行；provider 不可用时只交付 deterministic context/prompt。**

**Files:**

- Create: `.agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Ai/SystemDecomposition.Ai.csproj`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Ai/Context/SemanticEvidenceContextBuilder.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Ai/Context/EvidenceTracePackager.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Ai/Context/CounterEvidenceCollector.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Ai/Validation/SemanticSynthesisInputValidator.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/AnalysisEvidenceCatalog/Model/AiSynthesisModels.cs`

**Steps:**

1. 按 manifest、partition、ConceptId 和 `QueryBudget` 读取 catalog stage/entity/relation/path/binding evidence。
2. 只装配 `BoundedSourceClosurePackager` 允许的源码文件和范围，记录 sourceContextDigest。
3. 将 evidence refs、diagnostics、coverage、gaps、counter evidence 和 forbidden conclusions 写入 `AiContextPackage`。
4. 对每个 candidate writer、caller、lifecycle、projection 和 separate boundary 执行反向查询，收集冲突和缺口。
5. `SemanticSynthesisInputValidator` 在缺少关键 reader/writer/caller/lifecycle/effect 时返回 blocked/unknown，不生成伪完整上下文。
6. AI 模块不得引用生产 System、Component 或迁移代码；它只读取 catalog 和结构化分析结果。

## Task 13: 实现 provider-neutral AI 归纳和候选边界生成

**状态：源码已落地，未编译/未运行；禁止升级为 final owner、behavior-equivalent 或
`migration-success`。**

**Files:**

- Create: `.agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Ai/Synthesis/AiSemanticSynthesisGateway.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Ai/Synthesis/SemanticClaimValidator.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Ai/Synthesis/BoundaryCandidateGenerator.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Ai/Synthesis/IntegrationHandoffCandidateGenerator.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/AnalysisEvidenceCatalog/Model/BoundaryDecisionModels.cs`

**Steps:**

1. 定义 `AiSemanticSynthesisGateway` 的 provider-neutral 输入输出：文件上下文模式、标准输入模式和可选受控 provider adapter。
2. 不把网络模型调用作为默认依赖；默认模式生成可复现的 context/prompt artifact，并消费外部提供的结构化 AI result。
3. 校验每个 `AiSemanticClaim` 的 kind、evidence refs、counter refs、status 和 forbidden upgrade reason。
4. 禁止 fact-summary 使用不存在的 source location；禁止没有证据引用的陈述成为 confirmed。
5. 根据静态证据和 AI claims 生成 `BoundaryDecisionCandidate`：`keep`、`partial` 或 `separate`，并写出 rejected alternatives 与 revisit conditions。
6. 生成 owner、commit point、Concept API 和 IntegrationHandoff 候选，但不得生成 final owner、behavior-equivalent、verified 或 migration-success。

## Task 14: 实现 bundle、报告、链接和 provenance 输出

**状态：源码已落地，未编译/未运行；报告输出仍是静态候选报告。**

**Files:**

- Create: `.agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Ai/Output/StaticReportEmitter.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Ai/Output/IntegrationHandoffEmitter.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Ai/Output/ReportStatusGuard.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Ai/Output/ReportLinkAndEvidenceAuditor.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Ai/Output/EvidenceProvenanceExporter.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Ai/Output/PartitionReportConsistencyChecker.cs`

**Steps:**

1. 按统一章节顺序输出 Scope/Evidence、Conceptual Behaviors、Ownership、Boundary、API mapping、DAG、Lifecycle、Handoff、Behavior Contract、Gaps 和 Verification Plan。
2. 报告中区分源码事实、历史参考、AI 推导、候选设计和最终验收输入。
3. 自动补充 `designStatus: proposed`、`verificationStatus: not-run` 和跨分区 `integration-review` 状态。
4. `ReportStatusGuard` 扫描结构化报告和 Markdown，阻止 `verified`、`migration-success`、`behavior-equivalent`、`deletion-safe` 等静态阶段禁用状态。
5. `ReportLinkAndEvidenceAuditor` 检查本地文件链接、artifact ref、source location、relation edge 和 evidence gap 是否存在且身份一致。
6. `EvidenceProvenanceExporter` 输出报告结论到 artifact、关系和源码位置的可追溯索引。
7. `PartitionReportConsistencyChecker` 只检查格式、字段和状态词汇一致性，不把 20 分区的一致性当作 Owner 已确认。

## Task 15: 实现统一 CLI 和阶段编排

**状态：源码已落地，未运行构建/测试；本轮补充外部 analyzer provenance、状态约束、执行 DAG
启动门禁和重复 `--partition-manifest` 输入。**

**Files:**

- Create: `.agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Cli/SystemDecomposition.Cli.csproj`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Cli/Program.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Cli/Commands/BuildScopeCommand.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Cli/Commands/RunStaticAnalysisCommand.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Cli/Commands/ImportEvidenceCommand.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Cli/Commands/BuildAiContextCommand.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Cli/Commands/ValidateAiResultCommand.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Cli/Commands/EmitReportCommand.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Cli/Execution/AnalysisPipelineRunner.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Cli/Execution/StageRunnerRegistry.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Cli/Execution/ExternalAnalyzerProcessRunner.cs`
- Modify: `.agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Cli/Execution/AnalysisPipelineRunner.cs` for external analyzer identity, status, gap and DAG propagation
- Modify: `.agents/skills/ecs-system-domain-splitting/tools/AnalysisEvidenceCatalog/Graphs/ExecutionDag.cs` for dependency/barrier start guards
- Create: `.agents/skills/ecs-system-domain-splitting/tools/SystemDecomposition.Cli/Commands/CommandSupport.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/Run-SystemDecomposition.ps1`

**Steps:**

1. 实现 `build-scope`：读取显式任务表/分区和 source identity，生成 manifest，不扫描未知目录。
2. 实现 `run-static`：按 `ExecutionDag` 注册 stage，调用新分析器和现有 ProjectDataflow/CalledFunctions CLI，写 artifact 到 staging root。
3. 实现 `import-evidence`：使用现有 `EvidenceImportPipeline`、reader registry 和 relation materializer，成功后原子发布 catalog revision。
4. 实现 `build-ai-context`：执行 relation query、coverage、counter evidence 和 source closure package。
5. 实现 `validate-ai-result`：只校验结构化模型输出、evidence refs、状态和禁止升级，不调用最终验收门禁。
6. 实现 `emit-report`：组装 static bundle、IntegrationHandoff、Markdown 报告和 provenance index。
7. `ExternalAnalyzerProcessRunner` 使用参数数组、显式 cwd、超时、stdout/stderr artifact 和退出状态；不得拼接未经验证的 shell 命令。
8. 阶段失败保留已发布前的 staging artifact；catalog 不得留下半导入 revision；重跑使用新的 run/revision。
9. PowerShell launcher 只传递参数和调用已经构建的 CLI，不自动 restore/build，不修改 source tree。

## Task 16: 扩展文档和技能路由

**状态：reference、路由和 CLI 文档已更新，未运行 skill 压力评测、构建或测试。**

**Files:**

- Modify: `docs/system-decomposition/2026-09-19-system-decomposition-static-analysis-and-boundary-design.md`
- Modify: `.agents/skills/ecs-system-domain-splitting/system-splitting/SKILL.md`
- Modify: `.agents/skills/ecs-system-domain-splitting/SKILL.md`
- Create: `.agents/skills/ecs-system-domain-splitting/references/system-decomposition-toolchain.md`
- Create: `.agents/skills/ecs-system-domain-splitting/references/ai-synthesis-contract.md`

**Steps:**

1. 将实现后的工具名称、输入输出、artifact kind、relation kind、状态边界和 DAG stage 加入静态拆分预处理文档。
2. 在 system-splitting skill 的工具速查中加入新 CLI/API，并明确每个工具不能证明的内容。
3. 在入口 skill 中保持路由边界：System 先确定 owner/边界，API skill 再处理旧新概念组合；AI 工具不能反向创建运行时 System。
4. 将 provider-neutral AI 契约、禁止状态和 evidence reference 规则放入单独 reference，避免入口 skill 膨胀。
5. 更新工具输出文档、路径和 artifact schema 的本地链接，不修改历史报告的结论。

## 已落地交付顺序

实际代码仍可按以下逻辑批次审查，便于恢复和定位：

```text
Commit A  shared models + relation kinds + schema contracts
Commit B  scope manifest + symbol index + entrypoint/inbound artifacts
Commit C  behavior + dynamic + serialization + effect artifacts
Commit D  lifecycle + write closure + ownership candidates
Commit E  schedule/SCC + cross-partition seam + API candidates
Commit F  coverage audit + source closure + evidence import readers
Commit G  AI context + counter evidence + synthesis validator
Commit H  boundary candidate + bundle + report/provenance emitters
Commit I  CLI orchestration + skill/reference documentation
```

每个提交只包含对应工具和文档，不包含生产 `src/`、测试代码、最终验收结果或旧实现清理。

## 尚未执行的验证边界

需要由后续明确授权任务单独处理：

- schema/identity、artifact reader、relation materializer 和 catalog revision 的工具验证；
- 各 Roslyn analyzer 的 fixture/contract 验证；
- 统一 CLI 的串行构建和 smoke；
- 真实 Version4 source snapshot 的只读分析；
- 20 分区报告的一致性和 provenance 检查；
- 最终项目级真实接入、行为测试、`migration-success` 和旧实现删除门禁。

这些事项当前只作为交付边界记录；本轮不写测试代码、不运行测试、不运行构建，也不声明通过。

## 风险和回滚

- **Roslyn 诊断过多：** 保留 partial artifact，缩小分析 stage 的声明范围或重新选择 configuration；不删除已生成 gap。
- **动态入口无法解析：** 输出 dynamic-risk/unknown，交给集成审查；不通过字符串猜测 target。
- **关系类型扩展破坏旧 reader：** 保持旧 fact kind 兼容，使用 schema version 和显式 reader dispatch 回滚。
- **AI 输出格式错误：** 保留 `AiContextPackage`，拒绝候选写入，不生成空报告结论。
- **跨分区身份不一致：** 生成 IntegrationHandoff 并阻断 owner 汇总，等待 integration-review。
- **阶段中断：** staging artifact 可保留，catalog 只发布完整 revision；使用新 runId 重跑。
- **范围误配：** manifest validator 在分析前阻断，不能通过删除超范围成员或输入来“修复”覆盖率。

## 当前完成定义

本轮完成表示共享模型、静态 analyzer、证据导入/关系物化连接、AI context/claim/boundary
工具、统一 CLI 和文档路由已经写入目标目录，并通过源码级检查。它不表示工具已编译或运行，
也不表示任何 System 边界正确、API 行为等价、真实项目已接入、迁移成功或旧实现可删除。
