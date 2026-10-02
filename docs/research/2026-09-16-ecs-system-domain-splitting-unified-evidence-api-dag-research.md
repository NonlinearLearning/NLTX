# ECS System Domain Splitting：三工具联合索引与三层图模型研究报告

研究日期：2026-09-16

研究对象：`D:\TRbackup\NLTX\.agents\skills\ecs-system`

对照对象：`D:\ProjectItem\SourceCode\Net\NL`

结论状态：已完成代码事实、当前生成物、官方资料和回读校正；本报告提出 API 和图模型设计，不声明该 API 已经实现。

## 1. 最终结论

### 1.1 是否需要三个工具的联合 API

需要，但目标应是**统一证据目录和查询 API**，不是把三个工具改造成一个共享执行上下文的“大分析器”。三个工具的职责和可信边界不同：

| 工具 | 当前职责 | 可证明的事实 | 不能直接证明的事实 |
| --- | --- | --- | --- |
| `CalledFunctionsAnalyzer` | Roslyn 出站调用分析 | 选定 callable 的直接调用；开启 `--transitive` 后的已解析源代码调用闭包 | 入站 caller、事件/反射入口、生命周期、配置入口、运行时可达性 |
| `ProjectDataflowAnalyzer` | 项目级 callable 读写与数据流分析 | callable 的读变量、写变量、数据流入/出、捕获变量及诊断 | 调用关系的完整闭包、运行时入口、`partial` 之外的完整语义 |
| `FunctionBodyCleanup` | 创建原始/清理后源码快照并移除函数体 | 文件级输入/输出 hash、清理计数、快照 lineage、清理诊断 | 清理前后的完整行为等价、函数级 residual mapping、运行时语义 |

因此应增加一个位于三个工具之上的 `EvidenceCatalog` 或 `AnalysisRunIndex`：各工具仍独立运行并产生自己的 artifact，catalog 负责注册、版本化、绑定、诊断汇总和查询。它不应替工具补造缺失的 callers、生命周期或动态入口事实。

### 1.2 是否需要三份 DAG

不建议实现成“三份 DAG”，也不建议一工具一 DAG。正确的复用方式是采用 NL 中已经验证的**三层图模型**：

1. `ExecutionDag`：外层分析/变换阶段的输入输出依赖。它可以是真正的 DAG，并负责调度顺序、barrier、artifact lineage 和失败传播。
2. `FactClosure`：Propagation 类递归事实闭包。它使用 worklist、SCC/fixed-point 和 derivation，不应强行表示成普通 DAG。
3. `RelationMultigraph`：调用、读写、生命周期、证据和 artifact 关系的查询图。它是带类型、label、context 和 source 的有向多重图，可以有环，也允许相同端点之间存在多条语义不同的边。

三个工具到三层图不是一一对应关系：调用分析和数据流主要贡献 `RelationMultigraph` 的边与事实，清理工具贡献 `ExecutionDag` 中的变换阶段和 snapshot lineage；它们都可能被 catalog 的统一查询 API 读取。

### 1.3 是否应该提供按阶段快速查询的统一 API

应该提供，但“某阶段的所有信息”必须是**有边界的阶段快照查询**，而不是无界地把所有 JSON 拼接返回。API 至少应支持：

- 按 `RunId`、`SourceSnapshotId`、`AnalysisStageId` 精确定位版本；
- 返回阶段元数据、输入/输出 artifact 引用、事实、关系、诊断、unknown/partial 状态和统计摘要；
- 按 `EntityKey`、关系类型、文件、callable、状态和证据等级筛选；
- 按 shard 懒加载、分页和预算限制返回大结果；
- 对 `confirmed`、`partial`、`ambiguous`、`unresolved`、`unknown`、`failed` 保留原语义；
- 对每条跨工具绑定返回依据、候选和失效原因，而不是只返回一个布尔值。

建议统一 API 叫 `AnalysisEvidenceApi`，其核心入口可以是：

```text
GetStage(StageQuery) -> StageSnapshotResult
GetEntity(EntityQuery) -> EntityResult
GetRelations(RelationQuery) -> RelationResult
Explain(BindingId or RelationEdgeId) -> EvidenceTrace
```

`GetStage` 是按阶段查询的 facade；内部仍由三个 artifact reader 和三个图层分别执行，避免把不同语义压成一个列表。

## 2. 研究方法与证据状态

本轮按“先本地代码，后外部资料，再回读本地”的循环进行：

1. 先读取仓库入口、进度、变更和输出约束，再读取 skill、三个工具的实现/参考文档和当前 manifest。
2. 再读取 NL 的 RuleGraph、Propagation fixed-point、CPG query 和三层图比较研究。
3. 针对阶段、binding key、fixed-point/provenance 和多关系 edge 查阅官方资料。
4. 对照外部资料回读本地实现，检查哪些结论可以迁入，哪些只是类比，补上 status、snapshot 和 identity 限制。

`brave-search` 的 Node 入口在当前环境因 `BRAVE_API_KEY` 缺失而不能完成搜索/抽取；随后使用 PowerShell 直接访问官方 URL。第二轮直连核验的八个官方页面均返回 HTTP 200。网络资料只用于校正设计原则，不替代本地代码事实。

本报告的证据标记含义如下：

- `confirmed`：代码、manifest 或官方语义直接支持；
- `partial`：只覆盖部分路径、存在诊断或分析范围限制；
- `proposed`：设计建议，尚未实现；
- `unknown`：当前材料不能证明，不能当作空集合或完成条件。

## 3. 当前 skill 和三个工具的代码事实

### 3.1 skill 的交付约束

当前 skill 要求以可验证的领域行为切片为迁移单位，按 owner、不变量、读写集、生命周期、调用闭包、调度、验证和回滚取证；它明确要求分析器输出不能代替入站调用和动态入口搜索，并要求 `partial`/`unknown` 保持为证据缺口。[`SKILL.md:23-31`](../../.agents/skills/ecs-system/SKILL.md#L23-L31)

这直接决定联合 API 不能只做“把三个工具的结果合并为一份 JSON”：它必须能表达缺口和反证，且不能因为某个 analyzer 没返回记录就把事实升级为“没有调用者”或“没有写入者”。

### 3.2 CalledFunctionsAnalyzer 的绑定限制

`CalledFunctionsAnalyzer` 选择目标后建立调用闭包；只有在 `--transitive` 开启且调用已解析、目标 symbol 存在并且能在 source callable index 中找到时，才会继续入队下一个 callable。[`Program.cs:338-378`](../../.agents/skills/ecs-system/tools/scripts/CalledFunctionsAnalyzer/Program.cs#L338-L378)

每条 call record 当前保存 kind、syntax、symbol、resolved、containing type、调用位置、depth、source declaration、候选和 resolution；target 主要以 path、line、column 和 Roslyn display symbol 表示。[`Program.cs:454-486`](../../.agents/skills/ecs-system/tools/scripts/CalledFunctionsAnalyzer/Program.cs#L454-L486) `CallableRecord` 内部虽然持有 Roslyn symbol、declaration、path、line 和 column，但当前结构化输出没有与 dataflow 相同的正式 callable key。[`Program.cs:1023-1056`](../../.agents/skills/ecs-system/tools/scripts/CalledFunctionsAnalyzer/Program.cs#L1023-L1056)

工具可以写 summary JSON，也可以写每个 callable 的结构化报告和根级 `call-graph-index.json`。[`Program.cs:489-577`](../../.agents/skills/ecs-system/tools/scripts/CalledFunctionsAnalyzer/Program.cs#L489-L577) 当前 `reports` 目录没有发现现成的 `call-graph-index.json`，所以本报告不把调用图 artifact 当作当前已存在的完整报告。

这个边界意味着：联合 API 必须保留 `resolved=false`、候选和 resolution；不能用 `calls=[]` 表示“没有调用”，除非该查询明确表明完整扫描成功且没有匹配。

### 3.3 ProjectDataflowAnalyzer 的身份和 partial 状态

数据流分析通过 `MSBuildWorkspace` 打开项目并建立 CSharp compilation，再索引 syntax tree 中的 callable。[`ProjectDataflowAnalyzer.cs:16-98`](../../.agents/skills/ecs-system/tools/scripts/ProjectDataflowAnalyzer/ProjectDataflowAnalyzer.cs#L16-L98) callable ID 当前由 `file|declaration.SpanStart|kind|symbolText` 组成。[`ProjectDataflowAnalyzer.cs:357-393`](../../.agents/skills/ecs-system/tools/scripts/ProjectDataflowAnalyzer/ProjectDataflowAnalyzer.cs#L357-L393)

输出模型的 callable 包括 `ReadVariables`、`WrittenVariables`、`DataFlowsIn`、`DataFlowsOut`、`VariablesDeclared`、`Captured` 和 diagnostics；manifest 还记录 project、configuration、workspace/compilation/analysis diagnostics 和按源文件的 shard index。[`AnalysisModels.cs:144-159`](../../.agents/skills/ecs-system/tools/scripts/ProjectDataflowAnalyzer/AnalysisModels.cs#L144-L159)、[`AnalysisModels.cs:204-228`](../../.agents/skills/ecs-system/tools/scripts/ProjectDataflowAnalyzer/AnalysisModels.cs#L204-L228)

分析状态在存在诊断或 failed callable 时为 `partial`，不是把失败吞掉后返回 `complete`。[`ProjectDataflowAnalyzer.cs:100-162`](../../.agents/skills/ecs-system/tools/scripts/ProjectDataflowAnalyzer/ProjectDataflowAnalyzer.cs#L100-L162)

当前 manifest：

| 项目 | 当前事实 |
| --- | --- |
| 输出 | `.agents/skills/ecs-system/reports/data-flow-analysis/Version4/manifest.json` |
| 状态 | `partial` |
| 源文件 / syntax tree | 967 / 967 |
| callable indexed / analyzed | 7855 / 7157 |
| no-body / failed | 698 / 0 |
| diagnostics | 184 |

ReportWriter 按项目相对源路径写 `.dataflow.json` shard，并先写临时目录再替换输出目录，避免旧 shard 混入新报告。[`ReportWriter.cs:6-71`](../../.agents/skills/ecs-system/tools/scripts/ProjectDataflowAnalyzer/ReportWriter.cs#L6-L71)、[`ReportWriter.cs:74-123`](../../.agents/skills/ecs-system/tools/scripts/ProjectDataflowAnalyzer/ReportWriter.cs#L74-L123)

因此联合 API 必须支持按 callable lazy load dataflow shard，并把 manifest 的 `partial` 传播到 stage/result；不能将 `7157 analyzed` 包装成“项目全部 callable 都有完整读写集”。

### 3.4 FunctionBodyCleanup 的 snapshot lineage

Cleanup runner 为每次运行创建唯一 run directory，先复制 `original`，再复制出 `cleaned` 工作副本；清理在副本中并行执行，最后把输入/输出 hash、计数、诊断和每个文件状态写入 manifest。[`ProjectCleanupRunner.cs:15-72`](../../.agents/skills/ecs-system/tools/FunctionBodyCleanup/ProjectCleanupRunner.cs#L15-L72)、[`ProjectCleanupRunner.cs:75-115`](../../.agents/skills/ecs-system/tools/FunctionBodyCleanup/ProjectCleanupRunner.cs#L75-L115)

函数体清理会保留声明外壳但可能使清理副本无法编译；它不是语义 analyzer，也不应被当作调用图或 dataflow 图节点。[`function-body-cleanup.md:16-21`](../../.agents/skills/ecs-system/system-splitting/references/function-body-cleanup.md#L16-L21) `SourceFileCleanup` 对 parse error 标记 `skipped`，成功改写则记录输入和输出 hash。[`SourceFileCleanup.cs:16-74`](../../.agents/skills/ecs-system/tools/FunctionBodyCleanup/SourceFileCleanup.cs#L16-L74)

当前两次 manifest 的摘要如下：

| run | snapshot files | processed | changed | cleaned functions | succeeded |
| --- | ---: | ---: | ---: | ---: | --- |
| `version4-20260916-001` | 1087 | 967 | 583 | 6057 | true |
| `version4-20260916-002` | 1087 | 967 | 583 | 6058 | true |

同一 source root、同样文件计数和不同清理函数计数足以说明 run identity 不能省略。即使差异来自源树或工具状态变化，查询方也必须能够回答“绑定的是哪一次 run”。

另外，当前 cleanup manifest 是文件级输入/输出映射，没有现成的函数级 residual mapping。因此 `CleansBodyOf(originalCallable, cleanedCallable)` 在没有额外映射 artifact 时最多是 `partial`，不能只凭相对路径和行号宣布 exact。

## 4. NL 的三层图事实

### 4.1 外层 RuleGraph 是 DAG

NL 的 `RuleNodeId` 由 `RuleKind + RuleId` 组成，`CompiledRuleGraph` 保存有序节点和索引。[`RuleGraph.cs:3-54`](D:/ProjectItem/SourceCode/Net/NL/src/NLISSN.Rule/RuleGraph.cs#L3-L54)

`RuleGraphCompiler` 在建边前检查 duplicate node、unknown producer、syntax contract compatibility 和重复 dependency；随后进行确定性的拓扑排序，发现环就失败。[`RuleGraphCompiler.cs:3-109`](D:/ProjectItem/SourceCode/Net/NL/src/NLISSN.Rule/RuleGraphCompiler.cs#L3-L109) `RuleGraphExecutor` 以依赖图执行，并保留 node result、status、telemetry 和并发统计。[`RuleGraphExecutor.cs:9-18`](D:/ProjectItem/SourceCode/Net/NL/src/NLISSN.Rule/RuleGraphExecutor.cs#L9-L18)、[`RuleGraphExecutor.cs:200-271`](D:/ProjectItem/SourceCode/Net/NL/src/NLISSN.Rule/RuleGraphExecutor.cs#L200-L271)

这个层适合承载 skill 的阶段依赖，例如：

```text
SourceSnapshot
  -> CallAnalysis       \
  -> DataflowAnalysis    -> EvidenceBinding -> Decision/Authorization
  -> CpgOrSourceFacts  /
Decision/Authorization -> CleanupTransformation -> CleanedSnapshot
```

上图表达的是 artifact/阶段依赖，不代表调用、读写或传播事实本身是 DAG。

### 4.2 Propagation 是 fixed-point，不是普通 DAG

`PropagationFixedPointExecutor` 使用 `sourceFacts`、`admittedFacts` 和优先队列 worklist；新 admitted fact 会再次进入 worklist，直到没有新事实。[`PropagationFixedPointExecutor.cs:14-88`](D:/ProjectItem/SourceCode/Net/NL/src/NLISSN.Core/Propagation/PropagationFixedPointExecutor.cs#L14-L88)

它还存在 source mark projection 的 fallback：下游不一定能重建带 payload/provenance 的完整 key，因此会按可用的 rule、source tree、anchor 和 fact kind 选择 source fact。[`PropagationFixedPointExecutor.cs:91-121`](D:/ProjectItem/SourceCode/Net/NL/src/NLISSN.Core/Propagation/PropagationFixedPointExecutor.cs#L91-L121) 这已经说明 fact identity、source resolution 和 provenance 不是同一个字段。

`PropagationFactKey` 当前显式包含 rule、file/span/raw kind、fact kind/semantic tag、source tree version、anchor、payload identity 和 provenance identity。[`PropagationFactKey.cs:10-68`](D:/ProjectItem/SourceCode/Net/NL/src/NLISSN.Core/Propagation/PropagationFactKey.cs#L10-L68) `PropagatedMarkRecord` 也把 Payload、SourceMark、Depth 和 Provenance 分开保存。[`PropagatedMarkRecord.cs:7-27`](D:/ProjectItem/SourceCode/Net/NL/src/NLISSN.Core/Propagation/PropagatedMarkRecord.cs#L7-L27)

因此统一 API 可以把 propagation 暴露为 `FactClosure`，但必须返回 `fact key`、`derivation/provenance`、收敛状态和 source resolution 状态；不能用一个拓扑序列伪装成传播图。

### 4.3 CPG 是有预算的关系多图

NL 的 CPG query 已经区分 profile、direction、source/target selector、traversal budget、required capabilities 和 purpose；结果还携带 status、truncation reason、unavailable shards、capability、cache hit、访问计数和 metrics。[`CpgRelationQuery.cs:7-29`](D:/ProjectItem/SourceCode/Net/NL/src/NLCPG/Analysis/CpgRelationQuery.cs#L7-L29)、[`CpgRelationQuery.cs:98-141`](D:/ProjectItem/SourceCode/Net/NL/src/NLCPG/Analysis/CpgRelationQuery.cs#L98-L141)

图冻结后会按确定性顺序建立 CSR adjacency、按 kind 和 file 的索引，并生成 graph snapshot version。[`NLCPGGraphIndex.cs:68-175`](D:/ProjectItem/SourceCode/Net/NL/src/NLCPG/Model/NLCPGGraphIndex.cs#L68-L175) query service 的 cache key 包含 graph snapshot version、query profile、direction、selector、budget、capability 和 purpose；缺少 capability 会返回 `Unavailable`。[`CpgRelationQueryService.cs:23-60`](D:/ProjectItem/SourceCode/Net/NL/src/NLCPG/Analysis/CpgRelationQueryService.cs#L23-L60)

但当前实现的 continuation `visitedStates` 主要按 node、hops、call-depth 和 call-stack 去重，path 最后只按 node sequence 去重。[`CpgRelationQueryService.cs:93-106`](D:/ProjectItem/SourceCode/Net/NL/src/NLCPG/Analysis/CpgRelationQueryService.cs#L93-L106)、[`CpgRelationQueryService.cs:190-227`](D:/ProjectItem/SourceCode/Net/NL/src/NLCPG/Analysis/CpgRelationQueryService.cs#L190-L227) 这会压缩同端点不同语义边或不同 source 汇聚的 path；NL 现有比较研究已经把它列为 edge/path identity 风险。[`2026-09-08-dag-semantic-edge-comparative-research.md:109-122`](D:/ProjectItem/SourceCode/Net/NL/docs/plans/2026-09-08-dag-semantic-edge-comparative-research.md#L109-L122)

所以关系层必须是 first-class edge 的 multigraph。`RelationEdgeId` 至少需要 source、target、kind、structured label/context 和稳定 ordinal；path identity 需要包括 source 和 edge sequence，不能只用 node pair。

### 4.4 NL 自己的研究结论

NL 的比较研究明确写出：没有一个可以整体移植的相同 DAG 框架，因为同时存在外层规则调度 DAG、递归事实闭包和可查询多关系 CPG。[`2026-09-08-dag-semantic-edge-comparative-research.md:6-17`](D:/ProjectItem/SourceCode/Net/NL/docs/plans/2026-09-08-dag-semantic-edge-comparative-research.md#L6-L17) 它还把 `PortKey`、`RuleNodeId`、`FactKey` 和 `RelationEdgeId` 分开，并明确说 Evidence/Derivation 不是这些 identity 的替代品。[`2026-09-08-dag-semantic-edge-comparative-research.md:19-30`](D:/ProjectItem/SourceCode/Net/NL/docs/plans/2026-09-08-dag-semantic-edge-comparative-research.md#L19-L30)

这与本轮三个工具的输出差异相互印证：应该统一 identity、artifact 和查询协议，而不是统一成一个 DAG 数据结构。

## 5. 网络资料对本地结论的补正

### 5.1 ECS scheduler 资料只支持调度层

- Space Station 14 的 ECS 文档说明 Entity System 通过事件订阅接收回调，系统拥有行为和公开方法，系统之间可以用 API 和事件协作。它支持“调用图不等于所有运行时入口”的本地判断。
- Bevy ECS 文档说明 system 处理特定组件集合，系统默认尽可能并行，只有需要严格顺序时才显式使用 chain。它支持将读写/顺序约束放在 `ExecutionDag`，但不支持把所有语义关系都当作顺序边。
- Flecs 文档将 system 定义为 query + function，既可以手动运行，也可以按 phase 放入 pipeline。phase/pipeline 是调度概念，不是对事实、provenance 或关系 edge 的唯一身份定义。

官方页面：

- [Space Station 14 ECS](https://docs.spacestation14.com/en/robust-toolbox/ecs.html)
- [Bevy ECS](https://bevy.org/learn/quick-start/getting-started/ecs/)
- [Flecs Systems](https://www.flecs.dev/flecs/Systems.html)

因此可以复用 NL 的 stage/phase 思路，但不能用外部 ECS 的 phase 名称替代 skill 的 evidence identity，也不能因为文件或 system 的阶段顺序就推断 owner 或调用闭包。

### 5.2 Dagger 只提供 typed key 和 binding 校验顺序

Dagger Core Semantics 将 key 定义为 Java type 加可选 qualifier，并在生成前检查 binding graph 的 missing/duplicate binding 和普通 cycle。[Dagger Core Semantics](https://dagger.dev/semantics/)

可迁入的顺序是：

```text
结构化 key
  -> 根据 cardinality 检查 binding 集合
  -> 生成关系边
```

不可照搬的是 exactly-one producer。skill 和 NL 都允许 fan-in/fan-out，例如多个 producer 汇聚或一个 mark 向多个 consumer 传播。因此本地 `BindingResult` 需要保存 cardinality policy 和候选集，而不是强行返回唯一 binding。

### 5.3 Soufflé 支持 fixed-point/provenance 分层

Soufflé Tutorial 将 Datalog 描述为支持 recursive queries；其 provenance 文档把 tuple 的 proof/explanation 作为独立能力。它支持以下设计：`FactClosure` 维护 canonical facts 和收敛，`Derivation`/`Evidence` 另行记录来源解释。

官方资料：

- [Soufflé Tutorial](https://souffle-lang.github.io/tutorial)
- [Soufflé Provenance](https://souffle-lang.github.io/provenance)

它不能替 NLTX 决定任意 C# payload 是否构成不同事实；payload identity 仍必须通过本地 consumer 的领域契约确认。若 payload 会改变后续 decision、relation 或安全证明，就必须进入 fact identity 或有明确 discriminator；如果只影响解释，应作为 derivation 聚合。

### 5.4 TinkerPop 支持 edge 作为一等关系

TinkerPop 的 `Edge` 是带 `outVertex`、`inVertex`、direction 和 label 的 `Element`，`Element` 有自身 identifier。[Edge Javadoc](https://tinkerpop.apache.org/javadocs/current/core/org/apache/tinkerpop/gremlin/structure/Edge.html)、[Element Javadoc](https://tinkerpop.apache.org/javadocs/current/core/org/apache/tinkerpop/gremlin/structure/Element.html)

这支持 `RelationEdgeId` 和 edge-distinct path 的设计，但不等于要引入 TinkerPop，也不替代 NL 的冻结图、CSR、预算和 capability 协议。外部资料只补强“边不能降格为 node pair”的判断。

## 6. 建议的统一 API 与数据模型

### 6.1 统一 API 的职责边界

建议新增一个逻辑组件，名称可为 `AnalysisEvidenceCatalog`，由三个 reader/adaptor 接入：

```text
CalledFunctionsArtifactReader
ProjectDataflowArtifactReader
FunctionBodyCleanupArtifactReader
                  |
                  v
        AnalysisEvidenceCatalog
          |       |       |
   ExecutionDag FactClosure RelationMultigraph
                  |
                  v
         AnalysisEvidenceApi
```

catalog 的职责是：

- 注册每次分析/清理 run 的输入、配置、工具 schema 和 artifact hash；
- 为不同工具输出生成统一 typed key；
- 只创建有依据的 cross-tool binding，并保留 candidate、diagnostic 和 status；
- 建立 stage/artifact/entity/relation 的索引，按 shard 懒加载；
- 对同一 snapshot 的查询缓存结果，对 snapshot 变化使缓存失效；
- 提供可解释的 binding/evidence trace。

catalog 不负责：

- 把 unresolved call 推断成 resolved；
- 把 dataflow `partial` 改成 `complete`；
- 把 cleanup 后的源码当作原始源码继续复用语义事实；
- 凭文件名、行号或 method display name 猜测唯一绑定；
- 以 PerformanceStage 的耗时遥测代替语义阶段的事实索引。

### 6.2 建议的稳定 identity

所有 key 都应带 snapshot/run 范围。建议至少包含：

```text
SourceSnapshotId
  = source file content digests
  + project file/reference digests
  + relevant analysis configuration

ArtifactId
  = RunId + ToolKind + ArtifactKind + SchemaVersion + ContentHash

CallableKey
  = SourceSnapshotId
  + ProjectId
  + ProjectRelativePath
  + DeclarationSpan
  + CallableKind
  + canonical Roslyn SymbolKey

EntityKey
  = SourceSnapshotId + EntityKind + EntityPayload

RelationEdgeId
  = SourceSnapshotId
  + SourceEntityKey
  + TargetEntityKey
  + RelationKind
  + StructuredLabel/Context
  + StableOrdinal

BindingId
  = RunId + BindingKind + LeftKey + RightKey + BindingRuleVersion
```

具体适配：

- CalledFunctionsAnalyzer 应从内部 `declaration.Span` 和 Roslyn symbol 生成 `CallableKey`，并在 JSON 输出中增加该 key；当前的 line/column 只能作为定位信息。
- ProjectDataflowAnalyzer 应同时输出 canonical `CallableKey`，而不是只输出 `file|span|kind|symbolText` 字符串；现有 ID 可作为向后兼容字段。
- Cleanup 应保留 `OriginalSnapshotId`、`CleanedSnapshotId`、run ID、文件 hash 和 transformation map。没有函数级 map 时，cleanup 到 callable 的关系应标 `partial`。
- Evidence/Derivation 是解释集合，不参与替代 fact identity；同一 fact 可以有多份 derivation。

### 6.3 阶段命名和 NL PerformanceStage 的关系

建议使用独立的 `AnalysisStageId`，例如：

```text
SourceSnapshot
CallAnalysis
DataflowAnalysis
FactAndRelationBinding
DecisionOrAuthorization
CleanupTransformation
CleanedSnapshot
```

可以把 NL 的 `PerformanceStageId` 作为 telemetry 的 stage namespace 或附加 metadata，但不能直接把 `Run`、`Rule.Mark`、`Rule.Propagate` 等耗时阶段当作语义 evidence stage。性能阶段回答“何时执行/花了多久”，evidence stage 回答“产生了哪些可追溯事实/是否完整”。

### 6.4 按阶段查询的 API 形状

建议使用 typed request，而不是 `GetAll(stageName)`：

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

StageSnapshotResult {
  Stage: StageDescriptor,
  Status: Complete | Partial | Failed | Unavailable | Unknown,
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

`GetStage` 可以快速返回阶段的 metadata、counts、artifact refs 和诊断，再按 `ContinuationToken` 获取事实/关系；这样对 7855 callable、967 shard 的报告不会因为“查询某一阶段全部信息”一次性加载全部 JSON。

### 6.5 Binding 结果必须是可解释对象

建议的 binding 结果：

```text
BindingResult {
  BindingId,
  Left: EntityKey,
  Right: EntityKey,
  Kind: CallableCorrespondence | Reads | Writes | Calls | CleansBodyOf,
  Status: Confirmed | Partial | Ambiguous | Unresolved | Unknown,
  MatchBasis: SymbolKey | DeclarationSpan | SourceHash | CandidateHeuristic,
  Candidates: EntityKey[],
  EvidenceRefs: ArtifactRef[],
  Diagnostics: Diagnostic[],
  InvalidatedBy: SnapshotId or ConfigurationChange
}
```

状态规则：

- `Confirmed`：同一 `SourceSnapshotId` 下，canonical symbol 和 declaration anchor 唯一匹配；
- `Partial`：匹配成立但来源 artifact 是 partial、shard 未加载或只能证明文件级/阶段级关系；
- `Ambiguous`：多个 overload、候选 symbol 或多个 route 仍未消歧；
- `Unresolved`：工具明确记录调用/符号无法解析；
- `Unknown`：工具范围未覆盖、缺失入口或缺少必要能力；
- `Complete` 只表示该查询范围和所需能力已完成，不表示整个项目语义已完全证明。

空结果只有在对应阶段状态为 complete、完整范围已声明、所有必要 shard 已读取且没有匹配时才可以解释为“无匹配”。

## 7. 三层图的实现建议

### 7.1 `ExecutionDag`

节点是阶段或 artifact producer，不是每个 C# callable。每个节点记录：输入 artifact、输出 artifact、required capability、status、失败原因、barrier 和可重跑条件。边是 `Requires`、`Produces`、`Transforms`、`Authorizes` 等有限关系。

推荐初始阶段：

```text
SourceSnapshot
  -> CallAnalysis
  -> DataflowAnalysis
  -> RelationOrEvidenceBinding
  -> Decision/Authorization
  -> CleanupTransformation
  -> CleanedSnapshot
```

`CallAnalysis` 和 `DataflowAnalysis` 可以并行；`RelationOrEvidenceBinding` 等待两者所需 artifact，但只绑定已确认 key。`CleanupTransformation` 必须等待显式 authorization，不能仅因分析结果存在就自动执行。

### 7.2 `FactClosure`

内部使用 worklist/fixed-point，而对外暴露：

- seed facts；
- admitted/canonical facts；
- derivation/provenance；
- iteration/depth；
- convergence status；
- source resolution fallback 和诊断。

其 key 应类似 `FactKey`，但 payload 是否参与 identity 不能由工具自动决定。该决定必须由下游 consumer 矩阵、测试和安全证明确认。`FactClosure` 可以有 SCC/cycle；统一 API 查询时应返回 `ClosureStatus` 和 bounded derivations，而不是伪造拓扑排序。

### 7.3 `RelationMultigraph`

将以下关系作为一等 edge：

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

不同语义 relation 即使 source/target 相同也不应自动合并。查询需要区分：

- node reachability；
- relation edge materialization；
- edge-distinct path；
- source-preserving continuation；
- evidence/derivation explanation。

可复用 NL 的 profile、selector、capability、budget、snapshot cache 和 unavailable/truncated status 设计，但应把 `RelationEdgeId` 和 source 纳入 continuation/path identity。

## 8. 迁移和实现顺序

### Phase 1：只增加 artifact envelope，不改变 analyzer 语义

为三个工具输出增加统一 envelope：`RunId`、`SourceSnapshotId`、tool/schema/version、configuration digest、input root、output root、content hash、status、diagnostics 和 capability。保留原 JSON 字段，避免一次性破坏既有消费者。

### Phase 2：冻结 `CallableKey` 和 `EntityKey`

让 CalledFunctionsAnalyzer 和 ProjectDataflowAnalyzer 输出同一 canonical callable key；优先使用 Roslyn symbol key + declaration span + project-relative path。保留旧 display symbol/path/line/column 作为 display/diagnostic 字段，不再作为主绑定键。

### Phase 3：实现 catalog 的显式 binding

先做 exact binding，再返回 candidate/ambiguous/unknown。跨工具关系必须记录 match basis、artifact refs 和 invalidation condition。任何 unresolved/partial 都沿结果传递。

### Phase 4：建立一层 `ExecutionDag`

只把 artifact-producing stage 放进 DAG。将 cleanup 作为 `Authorization -> Transform -> CleanedSnapshot`，并为函数级清理映射补充 transformation map；在此之前只提供 file-level lineage。

### Phase 5：将 facts 和 relations 分别投影

数据流、调用、读写和证据关系进入 `RelationMultigraph`；若未来加入 propagation，则单独使用 `FactClosure`。不要从 relation edge 反推 fact convergence，也不要从 execution order 反推 ownership。

### Phase 6：实现 `GetStage` 和 bounded query

先返回 metadata/counts/diagnostics/artifact refs，再按选择器和预算加载 shards。缓存键必须包含 `SourceSnapshotId`、query profile、selector、capability、budget 和 API schema；源快照或工具 schema 改变就失效。

## 9. 验收矩阵

本研究没有运行 dotnet build/test，因为没有修改 C# 或工具实现；下列是实现该设计时必须增加的 focused verification：

| Case | 输入 | 必须证明 |
| --- | --- | --- |
| K1 | 同一方法在 call/dataflow 两个 artifact 中出现 | 通过 canonical `CallableKey` exact bind；display name 不参与唯一性 |
| K2 | overload、partial declaration、lambda 和 local function | key 不碰撞；ambiguous/candidate 可解释 |
| K3 | CalledFunctions unresolved call | 结果保留 `Unresolved`、候选和诊断；不能变成空调用集 |
| K4 | dataflow manifest 为 partial | `GetStage(DataflowAnalysis)` 返回 partial 和 unknown gaps；不能返回 complete |
| K5 | dataflow shard 缺失或不可用 | 返回 `Unavailable`/`Unknown` 及 shard ref；不静默减少结果 |
| K6 | 两个 cleanup run 的同一 source root | `RunId`/`CleanedSnapshotId` 不同；查询不会混合 6057 与 6058 的结果 |
| K7 | cleanup 前后函数声明位置变化 | 没有 transformation map 时只能 partial；有 map 时按 map exact bind |
| K8 | 同端点 `Calls` 和 `Reads` 两条边 | relation query 返回两条 edge；不能按 node pair 去重 |
| K9 | 两个 source 汇聚到同一中间节点 | path 保留 source identity；relation profile 不能错误剪枝第二个 source |
| K10 | propagation 递归 | `FactClosure` 收敛状态和 derivation 可查询；不要求 DAG 拓扑序 |
| K11 | query budget 达到上限 | 返回 truncated 和 reason；不能误报 complete |
| K12 | source hash/configuration 改变 | 旧 binding/cache 被标 invalidated，不继续复用 |

## 10. 未决问题和最小补证据

以下问题当前仍是 `unknown` 或 `proposed`，不应在 skill 中写成已实现能力：

1. CalledFunctionsAnalyzer 是否要直接升级 JSON schema，还是由 catalog adapter 外部生成 canonical callable key；需要一次 schema compatibility 评估。
2. ProjectDataflowAnalyzer 的 symbol display、source location 和 declaration span 在所有 partial/生成代码/多项目引用情形下如何合并；需要 fixture 覆盖。
3. `FunctionBodyCleanup` 是否需要输出函数级 transformation map；若后续要查询“某阶段某 callable 的完整信息”，这是 cleanup exact binding 的最小补件。
4. `PerformanceStageId` 是否仅保留为 telemetry namespace，还是由新的 `AnalysisStageId` 显式映射；需要确认现有消费者是否依赖其枚举值。
5. propagation payload 是否改变 downstream decision；需要按 consumer 列出 payload 矩阵后才能最终冻结 `FactKey`。
6. 入站 caller、事件订阅、反射、配置和生命周期入口目前不是 CalledFunctionsAnalyzer 的证明范围；若拆分删除门禁依赖这些事实，仍需独立 analyzer 或人工/运行时证据。

## 11. 最后回答用户的两个问题

**问题一：是否需要三个工具的联合 API？**

需要一个统一 `AnalysisEvidenceApi`/`EvidenceCatalog`，用于版本化 artifact、结构化 key、显式 binding、状态传播、shard 查询和证据解释；不需要把三个工具合并成一个 analyzer，也不应共享不可审计的隐式内存状态。

**问题二：是否需要做成三份 DAG，并使用 NL DAG 不同层概念？**

不应做成三份 DAG。应借用 NL 的三层语义，但命名为：一个真正的 `ExecutionDag`、一个 `FactClosure`、一个 `RelationMultigraph`。阶段查询 API 对外统一，内部按 graph kind/profile 分发。这样既能快速查询某阶段的完整可见信息，又不会把递归闭包、关系多图和执行顺序互相误当成同一种事实。

**推荐落地顺序：**先做 artifact envelope 和 canonical `CallableKey`，再做显式 binding/catalog，再做 `ExecutionDag` 和 `GetStage`，最后根据真实 consumer 需求补齐 `FactClosure` 与 edge/path identity。Cleanup 的函数级 mapping 和 dataflow partial 传播是删除门禁前的必要条件。

## 12. 来源

### 本地一手代码和生成物

- `D:\TRbackup\NLTX\.agents\skills\ecs-system\SKILL.md`
+ `D:\TRbackup\NLTX\.agents\skills\ecs-system\tools\scripts\CalledFunctionsAnalyzer\Program.cs`
+ `D:\TRbackup\NLTX\.agents\skills\ecs-system\tools\scripts\ProjectDataflowAnalyzer\AnalysisModels.cs`
+ `D:\TRbackup\NLTX\.agents\skills\ecs-system\tools\scripts\ProjectDataflowAnalyzer\ProjectDataflowAnalyzer.cs`
+ `D:\TRbackup\NLTX\.agents\skills\ecs-system\tools\scripts\ProjectDataflowAnalyzer\ReportWriter.cs`
- `D:\TRbackup\NLTX\.agents\skills\ecs-system\tools\FunctionBodyCleanup\ProjectCleanupRunner.cs`
- `D:\TRbackup\NLTX\.agents\skills\ecs-system\tools\FunctionBodyCleanup\SourceFileCleanup.cs`
- `D:\TRbackup\NLTX\.agents\skills\ecs-system\reports\data-flow-analysis\Version4\manifest.json`
- `D:\TRbackup\NLTX\.agents\skills\ecs-system\managed\function-body-cleanup\version4-20260916-001\manifest.json`
- `D:\TRbackup\NLTX\.agents\skills\ecs-system\managed\function-body-cleanup\version4-20260916-002\manifest.json`

### NL 一手代码和设计记录

- `D:\ProjectItem\SourceCode\Net\NL\docs\plans\2026-09-08-dag-semantic-edge-comparative-research.md`
- `D:\ProjectItem\SourceCode\Net\NL\src\NLISSN.Rule\RuleGraph.cs`
- `D:\ProjectItem\SourceCode\Net\NL\src\NLISSN.Rule\RuleGraphCompiler.cs`
- `D:\ProjectItem\SourceCode\Net\NL\src\NLISSN.Rule\RuleGraphExecutor.cs`
- `D:\ProjectItem\SourceCode\Net\NL\src\NLISSN.Core\Propagation\PropagationFixedPointExecutor.cs`
- `D:\ProjectItem\SourceCode\Net\NL\src\NLISSN.Core\Propagation\PropagationFactKey.cs`
- `D:\ProjectItem\SourceCode\Net\NL\src\NLCPG\Analysis\CpgRelationQuery.cs`
- `D:\ProjectItem\SourceCode\Net\NL\src\NLCPG\Analysis\CpgRelationQueryService.cs`
- `D:\ProjectItem\SourceCode\Net\NL\src\NLCPG\Model\NLCPGGraphIndex.cs`

### 官方外部资料

- [Space Station 14 ECS](https://docs.spacestation14.com/en/robust-toolbox/ecs.html)
- [Bevy ECS](https://bevy.org/learn/quick-start/getting-started/ecs/)
- [Flecs Systems](https://www.flecs.dev/flecs/Systems.html)
- [Dagger Core Semantics](https://dagger.dev/semantics/)
- [Soufflé Tutorial](https://souffle-lang.github.io/tutorial)
- [Soufflé Provenance](https://souffle-lang.github.io/provenance)
- [Apache TinkerPop Edge Javadoc](https://tinkerpop.apache.org/javadocs/current/core/org/apache/tinkerpop/gremlin/structure/Edge.html)
- [Apache TinkerPop Element Javadoc](https://tinkerpop.apache.org/javadocs/current/core/org/apache/tinkerpop/gremlin/structure/Element.html)
