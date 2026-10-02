# System 拆分：静态分析与候选边界设计报告

## 结论

System 拆分阶段只完成以下三项工作：

```text
静态代码事实分析
  + AI 语义归纳
  + 候选边界设计
```

本阶段的产物是一个有来源、有证据状态、有缺口和有候选边界的静态设计报告。它不
证明新实现已经接入真实项目，不证明新旧 API 行为等价，也不证明旧实现可以删除。

自动门禁、真实接入、行为验证、`migration-success` 和旧实现删除属于项目级最终验收，
详见 [`项目最终验收与迁移门禁报告`](2026-09-19-project-final-acceptance-and-migration-gates.md)。

## 1. 适用范围与边界

本报告适用于 Version4 或其他 ECS/OOP 到 ECS 的 System 边界拆分。它规定 AI 如何使用
静态工具和有界源码闭包形成候选设计，不规定具体项目的最终迁移门禁。

System 拆分阶段可以：

- 调用静态分析工具和读取已有静态分析产物；
- 发现成员、入口、调用、读写、生命周期、effect 和跨分区关系候选；
- 由 AI 读取证据指向的有界源码闭包并归纳概念行为；
- 设计候选 System、Component、Query、Command、Adapter、Projection 和协调层；
- 记录静态冲突、未知关系、动态风险和跨分区交接事项。

System 拆分阶段不可以：

- 把报告完成写成真实迁移完成；
- 把静态调用图写成完整运行时入口闭包；
- 把候选 Owner 写成唯一 Owner 已被运行时确认；
- 把 API 映射表写成新旧行为等价；
- 把编译、局部 verifier 或文件存在写成迁移成功；
- 在本阶段执行项目最终自动门禁或删除旧实现。

本报告与以下规则配合使用：

- [`ecs-system-domain-splitting/SKILL.md`](../../.agents/skills/ecs-system/SKILL.md)
- [`system-splitting/SKILL.md`](../../.agents/skills/ecs-system/system-splitting/SKILL.md)
- [`system-api-splitting/SKILL.md`](../../.agents/skills/ecs-system/system-api-splitting/SKILL.md)
- [`system-decomposition-session-contract.md`](../../.agents/skills/version4-partition-session-runner/sessions/version4-system-decomposition-session/references/system-decomposition-session-contract.md)
- [`System 拆分 Markdown 编写指南`](2026-09-02-system-split-markdown-writing-guide.md)

## 2. 三项核心工作

### 2.1 静态代码事实分析

静态分析负责发现可定位、可复现的代码事实，并保留无法解析的边。它不负责替项目
作最终验收判断。

| 事实类别 | 静态分析目标 | 不能直接推出的结论 |
| --- | --- | --- |
| 成员与声明 | 类型、成员、可见性、签名、重载、接口、override、事件和委托 | 概念行为一定相同 |
| 入站关系 | 调用点、接口绑定候选、事件订阅、委托保存和回调引用 | 所有运行时入口已经穷尽 |
| 出站关系 | 直接调用、静态传递调用、构造和服务调用 | 调度顺序和运行时可达性 |
| 读写关系 | 字段/property、`ref/out`、集合变更、helper 间接访问 | 哪个写者维护业务不变量 |
| 行为表面 | 条件、前置检查、返回路径、异常路径、事件和 effect 调用 | 调用方观察到的完整语义 |
| 生命周期 | 构造、初始化、注册、更新、结束、销毁、重置和卸载候选 | 运行时生命周期一定闭合 |
| 调度候选 | 显式前后调用、队列入队/刷新、phase 名称和 barrier 声明 | 实际同帧可见性和并行顺序 |
| 动态风险 | 反射、字符串注册、配置、生成代码、native/plugin 边界 | 动态入口的实际运行频率 |
| 跨分区关系 | ID、快照、事件、网络字段、序列化字段和共享服务 | 最终跨分区协调 Owner |

当前已有工具的定位：

- `ProjectDataflowAnalyzer`：callable 级候选读写集、捕获变量和诊断；
- `CalledFunctionsAnalyzer`：选定 callable 的静态出站调用和 unresolved/dispatch 诊断；
- `AnalysisEvidenceCatalog`：artifact、stage、事实、关系、诊断和 evidence gap 的统一登记；
- `RelationMaterializer`：把具有明确 endpoint 的结构化 fact 转成关系候选；
- `ExecutionDag`：组织分析和变换阶段的输入、依赖、barrier 和失败传播。

这些结果只能作为静态证据：

- 出站分析不能代替入站调用、事件注册、反射、配置、Timer、Task 或插件入口搜索；
- callable 读写集不能自动证明唯一权威写者和提交点；
- `ExecutionDag` 是分析/变换阶段依赖，不是目标项目的真实运行时调度图；
- 缺少 caller、target 或 canonical identity 时必须保留 gap，不能根据文件、行号或方法名猜 endpoint。

#### 2.1.1 静态证据导入与关系物化流水线

已有 artifact 进入 System 拆分前，必须经过统一的身份校验和关系物化流程：

```text
已有 artifact 自动注册
  -> schema / sourceSnapshot / configurationDigest 校验
  -> canonical entity binding
  -> fact 导入
  -> relation edge 物化
  -> relation index / DAG 查询
  -> 有界源码闭包读取
  -> AI 语义归纳
  -> 候选边界输出
```

每一步都记录输入、输出、工具版本、查询范围、证据状态和失败原因。`RelationMaterializer`
只能把已有且具有明确 subject/target endpoint 的结构化 fact 物化为关系；它不根据方法名、文件
相邻、行号相近或命名相似猜测 endpoint，也不把未发现的关系物化为空关系。

以下情况必须保留为 `partial`、`ambiguous`、`unresolved`、`unknown` 或 `unavailable`：

- `runId`、`sourceSnapshotId` 或 `configurationDigest` 不一致；
- v1/legacy artifact 缺少 v2 身份；
- virtual、interface、delegate、dynamic、function-pointer 或 reflection 调用无法唯一绑定；
- 分片未加载、hash 漂移、查询预算耗尽或结果被截断；
- 关系 endpoint、调用方向或 canonical entity identity 不完整。

关系查询只消费已经登记的证据，不触发新的分析器或源文件写入。查询预算、cursor、分片加载
和 revision 失效均属于证据状态的一部分，不能通过缩小查询范围来制造完整结果。

证据导入和关系查询使用统一的只读 API：

| 阶段问题 | API | 允许消费的内容 | 不能推出的内容 |
| --- | --- | --- | --- |
| 某个 stage/run 登记了什么 | `AnalysisEvidenceApi.GetStage` | artifacts、facts、relations、diagnostics、gaps | 未登记的事实不存在 |
| 某个 callable、字段或类型有哪些证据 | `AnalysisEvidenceApi.GetEntity` | canonical entity 的跨阶段事实和可选关系 | 模糊名称匹配或隐式绑定 |
| 某实体有哪些关系或路径 | `GetRelations` / `FindPaths` | 按 subject、direction、kind 和 budget 读取已物化边 | 从查询结果推导未登记的新边 |
| 一个 binding 为什么存在或失效 | `AnalysisEvidenceApi.Explain` | artifact、候选、诊断、失效原因和不能证明的内容 | 重新运行分析器或修复 binding |

所有 endpoint 使用同一个 `QueryBudget` 和 v2 cursor。首次查询若没有 canonical key，只能
先通过已有 artifact 建立候选 binding，不能用方法名、文件名或行号替代精确身份。`StageInclude.Derivations`
当前若返回 `Unavailable`，表示递归事实闭包不可用，不表示没有推导关系。

完整工具目录和 artifact 契约见
[`system-decomposition-toolchain.md`](../../.agents/skills/ecs-system/references/system-decomposition-toolchain.md)。

#### 2.1.1a 实际工具实现和统一入口

当前实现位于
`.agents/skills/ecs-system/tools/`，按职责分为：

| 层 | 实际路径 | 责任 |
| --- | --- | --- |
| 源码装载 | `SourceAnalysisInfrastructure/SourceAnalysisProjectLoader.cs` | 以显式 configuration/platform 加载 MSBuild/Roslyn project，生成 source/configuration provenance |
| 共享证据模型 | `AnalysisEvidenceCatalog/Model/` | `AnalysisContext`、scope、coverage、fact、API contract、source closure、bundle |
| 关系与执行图 | `AnalysisEvidenceCatalog/Graphs/`、`Import/`、`Query/` | `RelationMultigraph`、`ExecutionDag`、artifact reader、relation materializer、bounded query |
| 静态事实分析 | `SystemDecomposition.Analysis/` | 入口、入站、行为表面、动态风险、序列化、effect、生命周期、写入闭包、调度、API 和跨分区 seam |
| AI 边界分析 | `SystemDecomposition.Ai/` | 有界上下文、反证、provider-neutral request、claim 校验、候选边界和报告 |
| 统一 CLI | `SystemDecomposition.Cli/` | scope、静态分析、证据导入、AI context、AI result 校验和报告发布 |

CLI 的阶段顺序是：

```text
build-scope
  -> run-static
  -> import-evidence
  -> build-ai-context
  -> validate-ai-result
  -> emit-report
```

各阶段的边界：

- `build-scope` 只读取显式 project、成员清单、source root 和配置，输出
  `scope-manifest.json` 与 `analysis-context.json`；它不递归猜测分区，也不将任务表归属写成 owner。
- `run-static` 读取 scope，构建 canonical symbol index，运行静态 analyzer，并将 artifact 写入
  `staging/` 后再移动到发布目录；异常时保留 staging，不发布半套 artifact。
- `run-static` 可以通过重复的 `--partition-manifest` 注册其他分区；当前 scope 会自动加入，所有
  manifest 必须通过相同 `sourceSnapshotId`、`configurationDigest`、`projectId` 校验。该输入只为
  `IntegrationHandoff` 提供跨分区比较范围，不自动决定共享状态 owner。
- `import-evidence` 只读取 static-run manifest 中列出的 artifact，通过
  `EvidenceArtifactReaderRegistry`、`EvidenceImportPipeline` 和 `RelationMaterializer` 完成身份校验、
  relation edge 物化和 `ExecutionDag` 构造；它不搜索未知目录。
- `build-ai-context` 读取静态 bundle、coverage、relation 和 bounded source closure，生成
  `ai-context.json` 与 provider-neutral `ai-request.json`。
- `validate-ai-result` 只校验 request/context identity、claim evidence refs、反证和静态状态词汇；
  它不运行项目，也不决定迁移成功。
- `emit-report` 生成 static Markdown、integration handoff 和 provenance index，并强制报告保持
  `proposed`/`partial`/`unknown`/`not-run` 语义。

外部 `ProjectDataflowAnalyzer` 和 `CalledFunctionsAnalyzer` 的执行参数、工作目录、stdout/stderr、
退出码、超时和统一分析身份写入 static-run manifest。CalledFunctions 当前按单 callable、单 root
接口运行；多成员或多 root scope 会保留明确 gap。外部 analyzer 未配置或无法启动时生成
`unavailable` artifact，不将空集当作 confirmed。导入时 static-run descriptor 的状态和 gap 会约束
reader envelope、fact 和 artifact ref，防止失败/partial 产物被 reader 升级。

PowerShell 启动器
[`Run-SystemDecomposition.ps1`](../../.agents/skills/ecs-system/tools/Run-SystemDecomposition.ps1)
只调用已经生成的 CLI 程序集；它不会自动 restore、build、test、run 或修改 source tree。
当前实现阶段只做源码级检查，未运行 CLI、测试或构建。

#### 2.1.2 范围清单、快照和配置身份

每个 System 分区在分析开始前形成 `SystemDecompositionScopeManifest`：

```text
profile
partitionId
sessionId
inputReport
claimedMembers
allowedSourceRoots
excludedPaths
sourceSnapshotId
configurationDigest
configuration
platform
analysisToolVersions
priorArtifacts
queryScope
```

范围清单约束如下：

- 只能把当前领取分区的成员作为本次报告的主范围；
- 未领取分区的报告、历史 Component 拆分和 public-decomposition 文档只能作为待复核候选；
- 分区任务表的成员归属不能自动证明 System 所有权；
- 范围外成员可以作为必要的证据闭包被读取，但必须记录读取理由、关联关系和证据状态；
- 没有稳定快照、配置摘要或工具版本时，结论至少降级为 `partial` 或 `unknown`；
- 不得因为某成员不在当前分区，就宣称它没有调用者、写者、生命周期入口或副作用。

源快照、配置、平台、条件编译符号和工具版本必须与每条事实和关系绑定。不同身份上下文的
artifact 不得直接合并；身份不一致时先登记 evidence gap，再决定是否重新分析。

#### 2.1.3 覆盖率、负向结论和证据缺口

静态分析必须提供覆盖矩阵，而不是只提供“找到的成员和调用”：

| 覆盖面 | 必须说明 |
| --- | --- |
| 成员覆盖 | claimed member 是否全部有 canonical identity 和状态 |
| 入口覆盖 | public、protected、internal、interface、override、event、delegate、配置、反射入口 |
| 出站覆盖 | 直接调用、静态传递调用、未解析调用和 dispatch 类型 |
| 入站覆盖 | caller、注册点、回调保存点、Timer/Task 和插件入口 |
| 读写覆盖 | 直接写、间接写、队列写、恢复写、重建写和缓存写 |
| 生命周期覆盖 | create、activate、update、end、destroy、rebuild、reset、unload |
| 副作用覆盖 | network、persistence、logging、random、clock、I/O 和不可逆 effect |
| 跨分区覆盖 | ID、snapshot、event、serialization、network 和 persistence contract |

以下负向结论禁止裸写：

```text
没有搜索命中 != 没有关系
没有静态边 != 没有运行时入口
没有发现写者 != 没有写者
没有加载 shard != 该 shard 没有事实
没有找到 caller != 没有入站调用
```

每个“未发现”都必须附带搜索范围、使用的工具、身份上下文、未覆盖入口类型和最小补证据
建议。动态、反射、配置、序列化、异常、卸载和生成代码风险不能用静态搜索为空消除。

### 2.2 AI 语义归纳

AI 不应从整个代码库无边界地手工搜索，也不应把工具输出直接当成语义结论。AI 应沿
证据索引读取当前分区的有界源码闭包：

```text
静态索引和关系候选
  -> 当前 System 的成员、入口和缺口
  -> 直接入站/出站源码
  -> 关键读者、写者、生命周期和 effect 源码
  -> 相邻 System 的冲突或共享不变量
  -> AI 语义归纳和反证记录
```

AI 需要归纳：

- 不同 overload、接口入口、事件入口和回调是否表达同一个概念行为；
- 前置条件、返回值、错误边界、状态 delta、事件和副作用是否属于同一契约；
- 哪些字段是 authoritative、derived、cache、snapshot、compatibility 或 presentation；
- 哪个候选写者维护不变量，哪些写者只是恢复、投影、适配或缓存更新；
- 哪些 API 步骤必须保持顺序，哪些只是当前实现的偶然顺序；
- Query、Projection、Adapter 是否隐藏了写入、缓存刷新或领域规则；
- 哪些跨分区关系必须交给 `integration-review`。

静态阶段允许使用以下状态：

```text
static-confirmed
static-partial
static-candidate
proposed
conflicted
unknown
deferred-to-final-verification
```

这些状态表示静态设计和证据充分度，不表示运行时行为已经验证。

### 2.3 候选边界设计

候选边界依据能力、权威状态、不变量、读写集、生命周期和协作协议，而不是方法数、
文件数、Component 数量、类名或静态调用边数量。

候选设计至少说明：

| 设计项 | System 阶段输出 |
| --- | --- |
| System 责任 | 负责和明确不负责的能力 |
| 状态 Owner | 候选权威状态、候选写者和不变量依据 |
| Component | 数据形状、作用域、生命周期和访问方向 |
| Query | 只读数据来源以及隐式 effect 风险 |
| Command | 外部意图、结构变化、延迟可见性或重试依据 |
| Adapter | 参数、身份、协议或版本转换范围 |
| Projection | 单向派生输出和禁止回写规则 |
| 协调层 | 跨 Owner 顺序、事务、补偿或交接候选 |
| API 组合 | `LegacyEntryPoint -> ConceptId -> CompositionId` 候选映射 |
| 调度关系 | 静态可见的前后关系和未确认的 runtime gap |

边界设计是 `proposed` 或静态证据支持的候选，不是实现授权或行为验收。

#### 2.3.1 keep / partial / separate 决策记录

每个候选边界至少记录一个结构化 `BoundaryDecision`：

```text
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
```

决策规则如下：

- `partial` 只是同一运行时类型的源码组织方式，不增加调度节点；
- `separate` 必须有能力边界、输入/输出和协作契约，不能只因为文件或方法不同；
- 共享 Component 不自动意味着必须合并；多个消费者也不自动意味着必须拆分；
- 方法数量、文件数量、Component 数量和调用边数量不能单独作为拆分依据；
- 一个旧 API 调用多个内部方法时，不机械创建多个 System；先按概念行为和不变量判断；
- 每个被拒绝的方案都写出拒绝理由，触发条件变化时记录重新审查条件。

## 3. 六类关键问题的静态处理规则

### 3.1 旧入口库存

使用符号索引和反向引用分析查找：

- public/protected/internal 方法及 overload；
- 接口实现和显式接口实现；
- virtual/override/abstract 实现；
- 构造函数、工厂、服务注册和静态初始化；
- 事件处理器、订阅和取消订阅；
- 委托保存、传递和调用；
- command、网络、序列化、配置和反射入口；
- 生成代码、脚本和 native/plugin 边界。

AI 将这些入口归并为稳定 `ConceptId`，不按方法名寻找一一对应的新方法。静态分析
找不到的入口必须列为 `dynamic-risk` 或 `unknown`，不能当成“没有入口”。

### 3.2 旧入口行为表面

工具提取以下候选事实：

```text
parameters
return paths
guard conditions
thrown exceptions
field/component writes
events emitted
queue operations
network/persistence/logging calls
clock/random/I/O reads
```

AI 根据入口本体、直接调用者、关键被调用者和相关状态形成
`StaticBehaviorContractCandidate`：

```text
preconditions
return_or_error
authoritative_state_delta_candidate
emitted_events_and_effects_candidate
order_and_visibility_candidate
lifecycle_and_scope_candidate
retry_and_idempotency_unknowns
```

为了让旧入口到新组合的映射可被后续验收消费，候选契约还应尽量形成稳定的
`ConceptId` 记录：

```text
ConceptId
Scope
Stability
LegacyEntryPoints
InputsAndIdentity
Preconditions
Reads
InvariantCandidate
AuthoritativeWritesCandidate
ReturnOrError
EmittedEventsAndEffects
OrderAndCommitBoundary
VisibilityCandidate
LifecycleAndScope
FailurePolicyCandidate
RetryAndIdempotencyUnknowns
CanonicalNewCompositionCandidate
RequiredBehaviorScenarios
EvidenceStatus
OpenGaps
```

其中 `InputsAndIdentity` 至少考虑 entity/world/session、调用方或权限、默认值、overload
分流、时钟、随机源、request/correlation/idempotency key 以及缺失组件或已销毁实体。一个
旧入口可以对应多个新 API；多个 overload、接口入口、事件入口或适配入口也可以归于同一
`ConceptId`。`CanonicalNewCompositionCandidate` 只表达候选组合和顺序，不表达已经真实
接入或行为等价。

“真正承诺”的最终行为不在本阶段确认。

### 3.3 唯一权威状态和提交点

静态阶段汇总：

```text
Member
  -> known readers
  -> known direct writers
  -> indirect writer chains
  -> event/queue writers
  -> persistence/network restore writers
  -> reset/rebuild writers
```

AI 根据不变量和状态组合提出：

```text
candidateOwner
candidateCommitPoint
ownerConflict
unknownWriter
```

如果存在多个写者，报告描述各写者的角色，不能把数量最多的写者认定为 Owner。适合
使用 `static-owner-candidate`、`static-owner-conflicted`、`static-owner-partial` 和
`static-owner-unknown` 等诊断标签。

### 3.4 API 顺序、barrier 和可见性

本阶段只确认源码中有明确依据的约束：

```text
validate -> commit
enqueue -> flush
register -> dispatch
commit -> publish
A explicitly calls B
```

以下内容没有运行时证据时保留为候选或未知：scheduler 实际注册顺序、同帧可见性、
事件队列刷新点、多线程顺序、运行时 barrier 是否生效、外部效果的实际重试和补偿。

新 API 组合的状态只能是 `proposed-composition` 或 `static-composition-candidate`。

### 3.5 Query、Projection、Adapter 的隐式写入

| 角色 | 静态检查重点 |
| --- | --- |
| Query | 是否写权威状态、发领域事件、修改队列、刷新有副作用的缓存、读取时钟/随机或触发 I/O |
| Projection | 是否只从权威状态向外输出，是否反向修改权威状态 |
| Adapter | 是否只做参数/身份/协议转换，是否复制领域规则或直接写多个 Owner |
| Command | 是否有外部意图、结构变化、延迟可见性、排队、重试或幂等依据 |

发现违反约束时记录路径和 `role-conflict`，由 AI 判断是设计错误、合法 effect 还是需要
重新划分 Owner。本阶段不将该诊断升级为最终门禁结果。

### 3.6 跨分区共享契约

静态工具生成以下关系候选：

```text
Partition A
  -> shared entity id
  -> shared component or snapshot
  -> event/message
  -> network field
  -> persistence key
  -> Partition B
```

关系类型至少覆盖：

```text
Calls, References, Reads, Writes, Subscribes, Emits, Registers,
Serializes, Deserializes, Creates, Destroys, ProjectsTo, Configures, Schedules
```

当前分区不得单方面决定跨分区最终 Owner。共享状态、ID、快照、事件、网络/存档契约、
顺序和公共 seam 统一标记：

```text
crossSubsystemOwner: integration-review
```

### 3.7 调用图、依赖图与调度 DAG 的区分

本阶段同时处理三种图，但它们的节点、边和结论边界不同，不能混用：

| 图 | 表达内容 | 允许推出的结论 | 不允许推出的结论 |
| --- | --- | --- | --- |
| 分析流水线 DAG | artifact 注册、分析器、物化、查询、AI 归纳和报告输出 | 分析步骤依赖、输入、barrier 和失败传播 | 目标项目运行时顺序 |
| 目标调用/依赖图 | call、read、write、emit、subscribe、register、serialize、lifecycle 和 effect | 已观察的静态依赖、候选闭包和边界冲突 | 所有运行时入口已闭合、实际并发顺序 |
| 候选调度 DAG | phase、barrier、must-before、must-after、可并行和 commit point | 候选 System 协作顺序和待确认调度约束 | 目标调度器已经按候选关系执行 |

目标调用/依赖图可以存在递归、互调或事件回路。不能因为无法拓扑排序就删除边；应标记
`cycle`、`boundary-risk` 或 `unknown`，记录涉及的节点、关系类型和最小补证据。只有在
候选调度 DAG 中明确声明的调度关系才要求作为候选顺序检查，`ExecutionDag` 本身的拓扑
排序不能证明目标项目的 runtime phase、barrier 或同帧可见性。

### 3.8 生命周期状态转移候选

生命周期不能只记录“有构造函数”或“有 Update”。对每个候选状态建立以下状态转移记录：

```text
CurrentState
  -> Trigger
  -> Preconditions
  -> StateDelta
  -> Event / Effect
  -> NextState
  -> Failure / Rollback
```

至少检查以下转移及其作用域：

```text
create
activate
update
end
destroy
rebuild
reset
unload
persist
restore
network attach/detach
multi-world/session boundary
```

每个转移都记录入口、读写集、事件/队列、外部 effect、异常出口、重入和重复调用风险。
静态阶段只能输出 `lifecycle-candidate`、`lifecycle-partial` 或 `lifecycle-unknown`；
不能因找到创建和更新路径就推断销毁、重建、卸载或多 world 语义已闭合。

### 3.9 跨分区 Integration Handoff 契约

凡是涉及跨分区共享状态或公共 seam，都输出结构化 `IntegrationHandoff`：

```text
SeamId
CurrentPartition
RelatedPartitions
SharedStateOrContract
ObservedReaders
ObservedWriters
Producer
Consumer
OrderingRequirement
LifecycleRequirement
VersionRequirement
CandidateOwners
Conflict
MinimumNextEvidence
crossSubsystemOwner: integration-review
```

交接不能只写“共享 Component”或“存在依赖”，还要说明：

- 谁创建共享 ID，以及 ID 的 world/session/entity 作用域和生命周期；
- snapshot 是权威事实、缓存还是 projection，谁负责恢复；
- event 的生产者、消费者、注册入口、重复语义和失败处理；
- network/persistence 字段由谁定义、谁编码、谁恢复和谁承担版本兼容；
- 跨分区调用的顺序、可见性、失败传播和补偿候选；
- 当前分区无法确认的事项和下一步最小静态补证据。

当前分区不得将 `CandidateOwners` 直接升级为最终 Owner；最终结论留给项目级
`integration-review`。

## 4. 标准静态证据包

建议每个 System 分区形成 `SystemStaticEvidenceBundle`，作为静态分析和 AI 语义归纳的
交接对象，而不是最终验收凭证：

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

每条事实至少绑定 source snapshot/commit/hash、工具版本、configuration digest、稳定
实体身份、来源位置、证据状态、unresolved/ambiguous/dynamic 原因和最终验收待确认项。

### 4.1 单条事实、关系和缺口的最小记录

顶层证据包之外，单条事实和关系至少具备以下字段：

```text
EvidenceId
ArtifactId
RunId
SchemaVersion
SourceSnapshotId
ConfigurationDigest
SubjectCanonicalKey
TargetCanonicalKey
RelationKind
Direction
SourceLocation
QueryScope
EvidenceStatus
DispatchKind
ClosureEligible
Diagnostic
GapReason
```

不适用字段写 `not-applicable`，不能因为缺字段而省略事实的身份和状态。
`SubjectCanonicalKey` 和 `TargetCanonicalKey` 优先使用 v2 canonical symbol/entity key；
行号和文件路径仅用于定位，不能单独作为身份。每条记录必须能追溯到原始 artifact 和
查询上下文。

### 4.2 证据状态传播、截断和失效

证据目录、关系物化和 AI 归纳遵循最弱状态传播：

```text
Confirmed
  < Partial
  < Ambiguous
  < Unresolved
  < Unknown
  < Failed
  < Unavailable
  < Truncated
```

实际使用的原始状态名称以工具契约为准；报告中的 `static-confirmed`、`static-partial`、
`static-candidate`、`proposed` 等是对事实状态和设计状态的分层标记，不能覆盖原始 gap。

以下事件必须降低或阻断依赖该证据的结论：

- schema、run、source snapshot 或 configuration digest 不匹配；
- artifact 为 legacy，或 canonical binding 不唯一；
- shard 缺失、未加载、hash 漂移、解析失败或查询预算耗尽；
- relation 被截断、目标不可解析或 dispatch 不是静态唯一绑定；
- AI 读取的源码闭包缺少关键 reader、writer、caller、lifecycle 或 effect。

任何降级都要保留原始 artifact、定位信息、gap reason 和最小补证据。不能通过缩小结果集、
忽略 unresolved edge、把 `StageInclude.Derivations` 的不可用当作空集，或把一次旧查询的
cache 当作当前完整证据来提升状态。

### 4.3 System 报告最小输出模板

每个分区的最终静态报告至少按下列顺序交付，以便后续集成审查统一消费：

```text
# System Decomposition Report: <profile> <partition>
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

报告必须区分 Version4/current-NLTX 源码事实、历史或外部参考、AI 推导和候选设计。跨分区
共享状态、ID、snapshot、event、network/persistence contract、顺序和公共 seam 必须带
`crossSubsystemOwner: integration-review`。报告结算时默认使用 `designStatus: proposed`
和 `verificationStatus: not-run`，除非有本阶段明确允许的更强静态证据；静态证据不能写成
`verified`、`migration-success`、`behavior-equivalent` 或 `deletion-safe`。

静态证据包禁止包含以下最终状态：

```text
verified
migration-success
behavior-equivalent
deletion-safe
```

## 5. 推荐流程与 System 报告交付

```text
锁定源快照和分区范围
  -> 静态符号、调用、读写和风险面索引
  -> 物化有明确 endpoint 的静态关系
  -> AI 读取有界源码闭包
  -> 归纳概念行为、Owner 候选和 API 组合
  -> 识别角色冲突和跨分区 seam
  -> 形成候选边界与证据缺口
  -> 输出 proposed/partial/unknown System 报告
```

停止条件是静态证据已经足以支撑候选边界，或连续搜索没有新增静态事实。停止不代表
迁移成功；关键缺口必须进入 `Evidence Gaps and Blocking Decisions`。

System 报告至少包含：

- Scope and Evidence；
- Conceptual Behaviors；
- State Ownership and Write Closure；
- Boundary Role and Decision；
- System API and Legacy Behavior Mapping；
- Call and Dependency DAG；
- Lifecycle and Side Effects；
- Integration Handoff；
- Migration Behavior Contract；
- Evidence Gaps and Blocking Decisions；
- Verification Plan。

其中 `Migration Behavior Contract` 只记录未来验收所需的观察向量、入口命中要求和行为
场景清单，不执行行为验证，也不写通过状态。System 拆分阶段：

- 不编写测试代码；
- 不创建行为 fixture 或 verifier；
- 不运行测试、构建或最终自动门禁；
- 不记录行为测试通过/失败；
- 只把静态报告中的候选契约、缺口和未来验收输入要求交给项目最终验收阶段。

## 6. System 阶段禁止的状态升级

| 静态事实 | 不能推出 |
| --- | --- |
| 成员清单完整 | System Owner 已确认 |
| 静态调用图存在 | 入站和动态入口已闭合 |
| 读写分析无记录 | 没有写者或没有副作用 |
| API 映射表已填写 | 新旧 API 行为等价 |
| `ExecutionDag` 可拓扑排序 | 运行时 phase/barrier 已确认 |
| 编译通过 | 迁移成功 |
| 局部 verifier 通过 | 真实项目行为通过 |
| 旧 facade 仍可调用 | 新路径已成为真实路径 |
| 新 Component 已创建 | 旧 Component 可以删除 |

System 拆分完成表示静态事实、语义归纳、候选边界和缺口已经交付；它不表示项目
迁移完成。
