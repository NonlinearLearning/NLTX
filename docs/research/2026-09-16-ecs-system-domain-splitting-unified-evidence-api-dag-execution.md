# ECS System Domain Splitting Unified Evidence API/DAG Implementation Plan

> **For implementer:** 按任务逐项执行本计划；每项先建立失败的 focused verifier，再实现最小行为并记录实际命令、退出码、警告/错误数量和产物路径。任何未执行项保持 `planned`/`not-run`，不得写成已完成。

**Goal:** 在不改变三个既有工具第一阶段分析语义的前提下，增加可版本化、可解释、可分页查询的统一证据 catalog，并按 `ExecutionDag`、`FactClosure`、`RelationMultigraph` 三种语义分别承载阶段依赖、递归事实和关系查询。

**Architecture:** 以 `SourceSnapshotId`、`ArtifactId` 和 canonical key 为边界，使用三个独立 artifact reader 接入既有输出；catalog 负责注册、binding、状态和索引；API 只读 catalog。cleanup 必须由 `DecisionOrAuthorization` 显式授权，写入独立 managed run，旧 JSON 和旧 launcher 在删除门禁通过前保持兼容。

**Tech Stack:** .NET SDK `global.json` 选定版本（当前为 `10.0.400`）、`net10.0`、C#、Roslyn symbol/declaration identity、`System.Text.Json`、现有 PowerShell fixture/verifier、`Build/Tools/Invoke-SerialDotnet.ps1`。

---

## 1. 执行状态与范围

| 项目 | 当前状态 |
| --- | --- |
| 设计依据 | [统一证据 API 与三层图模型设计](2026-09-16-ecs-system-domain-splitting-unified-evidence-api-dag-design.md) |
| 来源研究 | [联合证据 API/DAG 研究报告](2026-09-16-ecs-system-domain-splitting-unified-evidence-api-dag-research.md) |
| 执行状态 | `in-progress` |
| 生产代码状态 | `not-started` |
| 验证状态 | `not-run` |
| 当前文档任务是否运行 build/test | 否；本次没有修改 C# 或工具实现 |
| 目标代码边界 | `.agents/skills/ecs-system/tools/AnalysisEvidenceCatalog` 与配套 fixture/verifier |
| 生成物边界 | `Build/bin/`、`Build/obj/`、`Build/generated/ecs-system-domain-splitting/` 或 `%TEMP%`；不写入 `src/` 或源 artifact |

本计划是后续实现计划，不是当前实现报告。目标路径是拟议路径，执行第一项时必须确认不存在冲突的用户修改、项目文件或已存在的同名类型。

## 2. 执行前硬约束

### 2.1 证据和状态

- 保留 `confirmed`、`partial`、`ambiguous`、`unresolved`、`unknown`、`failed`、`unavailable` 和 `truncated` 的原语义。
- `partial` manifest、缺失 shard、未解析 call 和文件级 cleanup lineage 不能被包装为 `complete` 或 exact binding。
- 当前研究报告的 7855/7157 callable、967 文件、184 diagnostics 以及两次 cleanup run 的 6057/6058 计数只作为输入基线，不是新实现的通过结果。
- 所有真实运行结果写入执行记录；如果命令未运行，文档保留 `not-run`。

### 2.2 构建与并发

每次 compile-capable 命令前，从仓库根目录检查活动的 `dotnet.exe`/`csc.exe`：

```powershell
$activeBuilds = Get-CimInstance Win32_Process |
  Where-Object {
    $_.Name -eq 'csc.exe' -or
    ($_.Name -eq 'dotnet.exe' -and $_.CommandLine -match
      '(?i)\b(restore|build|rebuild|test|run|publish|pack|watch|msbuild)\b')
  }
$activeBuilds | Select-Object ProcessId, ParentProcessId, Name, CommandLine
```

发现其他 owner 或 owner 不清时等待，不终止进程。所有 build/test/restore 命令必须通过：

```powershell
pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 `
  <restore|build|test|run> .\path\AffectedProject.csproj `
  -m:1 -nr:false `
  -p:UseSharedCompilation=false `
  -p:MSBuildNodeReuse=false `
  -p:BuildInParallel=false
```

不得通过后台 job、`ForEach-Object -Parallel`、并行工具调用或 raw `dotnet` 命令绕过共享锁。测试优先使用已 build 的 artifact，并追加 `--no-build --no-restore` 的 verifier 运行方式（若项目 verifier 是可执行程序，则使用仓库脚本约定运行）。

### 2.3 生成输出

- 既有 analyzer artifact 只读；不在其目录覆盖原始 JSON、manifest 或 shard。
- catalog 的可重现输出写入 `Build/generated/ecs-system-domain-splitting/analysis-evidence/<runId>/`。
- 单元/fixture 临时目录使用 `%TEMP%` 下唯一目录，测试结束后只删除本次创建的目录。
- 每个 build 记录项目、精确命令、退出码、warning/error 数、预期 artifact 路径，并确认产物位于 `Build/bin/`。

## 3. 目标文件布局

以下是实现阶段的拟议文件路径；文件创建前先检查用户现有变更和同名内容。

```text
.agents/skills/ecs-system/
  tools/AnalysisEvidenceCatalog/
    AnalysisEvidenceCatalog.csproj
    AnalysisEvidenceCatalog.cs
    Model/SourceSnapshotId.cs
    Model/AnalysisRunId.cs
    Model/ArtifactId.cs
    Model/ArtifactEnvelope.cs
    Model/ArtifactRef.cs
    Model/CallableKey.cs
    Model/EntityKey.cs
    Model/RelationEdgeId.cs
    Model/BindingId.cs
    Model/EvidenceStatus.cs
    Model/Diagnostics.cs
    Artifacts/CalledFunctionsArtifactReader.cs
    Artifacts/ProjectDataflowArtifactReader.cs
    Artifacts/FunctionBodyCleanupArtifactReader.cs
    Artifacts/ArtifactRegistry.cs
    Binding/CanonicalKeyResolver.cs
    Binding/ExplicitBindingStore.cs
    Binding/BindingResult.cs
    Graphs/ExecutionDag.cs
    Graphs/FactClosure.cs
    Graphs/RelationMultigraph.cs
    Graphs/RelationEdge.cs
    Query/AnalysisEvidenceApi.cs
    Query/StageQuery.cs
    Query/EntityQuery.cs
    Query/RelationQuery.cs
    Query/QueryBudget.cs
    Query/ContinuationToken.cs
    Query/QueryResults.cs
    Storage/CatalogIndexStore.cs
    Storage/AtomicCatalogWriter.cs
    Storage/QueryCache.cs
  tools/AnalysisEvidenceCatalog.Verification/
    AnalysisEvidenceCatalog.Verification.csproj
    Program.cs
    Fixtures/FixtureCatalogBuilder.cs
  scripts/tests/fixtures/AnalysisEvidenceFixture/
    Call/...
    Dataflow/...
    Cleanup/...
    Expected/...
  scripts/tests/Test-AnalysisEvidenceCatalog.ps1
  scripts/tests/Test-AnalysisEvidenceIntegration.ps1
```

目录可以在 Task 1 中按真实项目规模微调，但必须保持领域优先、一个核心公开类型一个同名文件、测试与被测工具边界清晰；不得先创建空的 `Components/Queries/Systems` 模板目录。

## 4. 任务清单

### Task 0: 固定基线与兼容矩阵

**Files:**

 Read: `.agents/skills/ecs-system/tools/scripts/CalledFunctionsAnalyzer/Program.cs`
 Read: `.agents/skills/ecs-system/tools/scripts/ProjectDataflowAnalyzer/AnalysisModels.cs`
 Read: `.agents/skills/ecs-system/tools/scripts/ProjectDataflowAnalyzer/ReportWriter.cs`
- Read: `.agents/skills/ecs-system/tools/FunctionBodyCleanup/ProjectCleanupRunner.cs`
- Read: `.agents/skills/ecs-system/tools/FunctionBodyCleanup/SourceFileCleanup.cs`
- Read: `.agents/skills/ecs-system/reports/data-flow-analysis/Version4/manifest.json`
- Read: `.agents/skills/ecs-system/managed/function-body-cleanup/version4-20260916-001/manifest.json`
- Read: `.agents/skills/ecs-system/managed/function-body-cleanup/version4-20260916-002/manifest.json`
- Modify: this execution document, only to record the completed inventory and unresolved items

**Step 1: Confirm workspace state**

Run `git status --short` and record overlapping changes in the task notes. Do not revert or clean user files.

**Step 2: Build the compatibility matrix**

Record old top-level fields, manifest status values, shard path rules, cleanup file lineage fields, schema versions, and all current launcher consumers. Mark each field `confirmed`, `partial`, or `unknown`.

**Step 3: Define fixture contracts**

Select minimal fixtures for overloads, partial declarations, lambda/local functions, unresolved calls, partial dataflow manifests, missing shards, two cleanup runs, same-endpoint multi-edge relations and recursive facts.

**Exit gate:** no implementation task starts until every adapter input has a named fixture or an explicit `not-covered` gap.

**Task 0 record (2026-09-16, read-only inventory):**

- Scope is limited to the three existing artifact producers under this skill and the new
  `tools/AnalysisEvidenceCatalog` boundary. The catalog will not change analyzer semantics or
  write to `src/`, the Version4 source tree, or existing report directories.
- The checkout already contains broad unrelated user changes. The target skill directory has no
  overlapping Git change; the two design/ execution documents are pre-existing untracked inputs.
  No user files were reverted or cleaned.
- The linked `Context/约束/公共拆分约束.md` is absent from the current checkout. The available
  ECS rules, evidence protocol, C# style, side-effect isolation, and build/output constraints
  were read; the missing linked document remains an execution gap rather than an invented rule.

| Adapter input | Current fields and status | Compatibility decision | Fixture / gap |
| --- | --- | --- | --- |
| `CalledFunctionsAnalyzer` summary and structured reports | schema `1.0`; root, project path/references, target name/symbol/type/path/line/column, calls with `resolved`, candidates, resolution, source declaration, diagnostics, statistics, configuration; confirmed as legacy output | Keep old JSON and launcher readable. A display symbol, report filename, or line/column alone yields `legacy` plus `MissingIdentityField`, never confirmed cross-tool binding | Named fixture: `Call/Overloads.cs`, `PartialTarget.Part1.cs`, `PartialTarget.Part2.cs`, `LambdaAndLocal.cs`; unresolved-call fixture is planned. Current output does not expose canonical Roslyn symbol key: `not-covered` until additive fields or an adapter fixture supplies them |
| `ProjectDataflowAnalyzer` manifest | top-level schema/tool/Roslyn/status/project/configuration/summary/diagnostic arrays/files; project has 967 source files and shards; current status `partial`, summary 7855/7157/184 | Manifest status is authoritative. Missing or unloaded shard is not an empty result; retain shard ref and return unavailable/unknown | Named fixtures: `Dataflow/partial-manifest.json`, `Dataflow/missing-shard-manifest.json`; real Version4 manifest is read-only integration input |
| Dataflow source-file shard | `sourceFile`, `projectRelativePath`, `callables`, `diagnostics`; callable has id/kind/symbol/containingSymbol/file/line/column/status/read/write/data-flow/declared/captured/diagnostics | Load shard lazily and preserve callable status/diagnostics. Existing display symbol and location remain navigation fields only | Named source-file shard fixture is planned under `Dataflow/`; current 967-shard set is integration input |
| `FunctionBodyCleanup` manifest | schema `1.0`; source/management/run/original/cleaned roots; excluded dirs; Roslyn settings; file/count status; per-file input/output SHA-256 and cleanup counts | Register each run separately. File-level input/output lineage is `partial` for callable binding; no guessed line map | Named fixtures: `Cleanup/run-001/manifest.json`, `Cleanup/run-002/manifest.json`; current runs are read-only integration inputs (6057 vs 6058) |
| Existing launchers/consumers | `Run-CalledFunctionsAnalyzer.ps1`, `Run-ProjectDataflowAnalyzer.ps1`, `Run-FunctionBodyCleanup.ps1`, their focused PowerShell verifiers, README/reference docs, registry and permission policy | Do not replace or delete launchers/fields. Catalog readers are opt-in and additive | Existing verifier suite remains the regression baseline; no current catalog consumer exists |
| Three-layer graph | No current graph implementation | `ExecutionDag` and `RelationMultigraph` are required; `FactClosure` is conditional | Same-endpoint `Calls`/`Reads`, converging sources and cycle fixtures are named for Task 6; recursive consumer decision is `deferred` pending a real consumer |

**Task 0 fixture contract:**

- Identity: overloads, partial declarations, lambda/local functions, stable source snapshot and
  changed source/configuration.
- Artifact status: unresolved call with candidates/diagnostic, partial dataflow manifest, missing
  shard, two cleanup runs, and file-level cleanup lineage without callable exactness.
- Graph/query: two same-endpoint relation kinds, two source paths converging on one node, a cycle,
  bounded traversal, selector/pagination/continuation, cache invalidation, and cleanup authorization.
- Explicit not-covered gaps: current legacy called-functions output has no canonical symbol key;
  current cleanup output has no function-level transformation map; no confirmed recursive
  propagation consumer or payload identity matrix exists yet. These gaps remain `partial`,
  `unknown`, or `deferred` in the catalog and are not filled by heuristics.

### Task 1: Add artifact envelope and registry

**Files:**

- Create: `.agents/skills/ecs-system/tools/AnalysisEvidenceCatalog/AnalysisEvidenceCatalog.csproj`
- Create: `.agents/skills/ecs-system/tools/AnalysisEvidenceCatalog/Model/AnalysisRunId.cs`
- Create: `.agents/skills/ecs-system/tools/AnalysisEvidenceCatalog/Model/SourceSnapshotId.cs`
- Create: `.agents/skills/ecs-system/tools/AnalysisEvidenceCatalog/Model/ArtifactId.cs`
- Create: `.agents/skills/ecs-system/tools/AnalysisEvidenceCatalog/Model/ArtifactRef.cs`
- Create: `.agents/skills/ecs-system/tools/AnalysisEvidenceCatalog/Model/ArtifactEnvelope.cs`
- Create: `.agents/skills/ecs-system/tools/AnalysisEvidenceCatalog/Model/EvidenceStatus.cs`
- Create: `.agents/skills/ecs-system/tools/AnalysisEvidenceCatalog/Model/Diagnostics.cs`
- Create: `.agents/skills/ecs-system/tools/AnalysisEvidenceCatalog/Artifacts/ArtifactRegistry.cs`
- Create: `.agents/skills/ecs-system/tools/AnalysisEvidenceCatalog.Verification/AnalysisEvidenceCatalog.Verification.csproj`
- Create: `.agents/skills/ecs-system/tools/AnalysisEvidenceCatalog.Verification/Program.cs`

**Step 1: Write failing verification**

Add assertions that an envelope round-trips all required fields, content hash participates in `ArtifactId`, legacy artifacts retain a diagnostic, and a duplicate `(RunId, ArtifactId)` registration is rejected without mutating the existing record.

**Step 2: Run the verifier**

Run the new verifier before implementation. Expected: fail because the project/types do not exist.

**Step 3: Implement the minimal model**

Use immutable value records for IDs and explicit status/capability collections. Keep `CreatedAt` and absolute paths diagnostic-only; do not include them in semantic identity.

**Step 4: Build and rerun**

From repository root run the serial wrapper for `build` on `AnalysisEvidenceCatalog.Verification.csproj`, then run the verifier with `--no-build --no-restore` if supported. Expected: round-trip and duplicate-registration assertions pass.

**Exit gate:** old analyzer JSON remains readable by its existing tests; new envelope is additive and deterministic.

### Task 2: Freeze canonical source and callable identity

**Files:**

- Create: `tools/AnalysisEvidenceCatalog/Model/CallableKey.cs`
- Create: `tools/AnalysisEvidenceCatalog/Model/EntityKey.cs`
- Create: `tools/AnalysisEvidenceCatalog/Binding/CanonicalKeyResolver.cs`
- Modify: `tools/AnalysisEvidenceCatalog.Verification/Program.cs`
- Create: `scripts/tests/fixtures/AnalysisEvidenceFixture/Call/Overloads.cs`
- Create: `scripts/tests/fixtures/AnalysisEvidenceFixture/Call/PartialTarget.Part1.cs`
- Create: `scripts/tests/fixtures/AnalysisEvidenceFixture/Call/PartialTarget.Part2.cs`
- Create: `scripts/tests/fixtures/AnalysisEvidenceFixture/Call/LambdaAndLocal.cs`

**Step 1: Write failing identity cases**

Assert that overloads, partial declarations, lambdas and local functions produce distinct keys; equivalent input ordering produces the same `SourceSnapshotId`; changed source/configuration produces a different snapshot.

**Step 2: Run the focused verifier**

Expected: fail on missing canonical resolver and fixture loader.

**Step 3: Implement identity and candidate handling**

Use project-relative path, declaration span, callable kind and canonical Roslyn symbol identity. Keep display symbol, line and column as display fields only. If exact identity is unavailable, return candidates with `ambiguous`/`unknown`, never heuristic `confirmed`.

**Step 4: Verify determinism**

Run the fixture twice with the same normalized input and compare all IDs and serialized output hashes. Expected: byte-stable results.

**Exit gate:** no cross-tool binding code may use display name, line/column or report filename as the primary key.

### Task 3: Implement the three artifact readers

**Files:**

- Create: `tools/AnalysisEvidenceCatalog/Artifacts/CalledFunctionsArtifactReader.cs`
- Create: `tools/AnalysisEvidenceCatalog/Artifacts/ProjectDataflowArtifactReader.cs`
- Create: `tools/AnalysisEvidenceCatalog/Artifacts/FunctionBodyCleanupArtifactReader.cs`
- Modify: `tools/AnalysisEvidenceCatalog/Artifacts/ArtifactRegistry.cs`
- Create: `scripts/tests/fixtures/AnalysisEvidenceFixture/Dataflow/partial-manifest.json`
- Create: `scripts/tests/fixtures/AnalysisEvidenceFixture/Dataflow/missing-shard-manifest.json`
- Create: `scripts/tests/fixtures/AnalysisEvidenceFixture/Cleanup/run-001/manifest.json`
- Create: `scripts/tests/fixtures/AnalysisEvidenceFixture/Cleanup/run-002/manifest.json`
- Modify: `tools/AnalysisEvidenceCatalog.Verification/Program.cs`

**Step 1: Write failing reader tests**

Assert the following exact behavior:

- unresolved call records remain `unresolved` with candidates and diagnostics;
- dataflow `partial` manifest produces `partial` coverage;
- missing shard produces `unavailable`/`unknown` and a shard reference;
- cleanup reader registers two run IDs separately and does not merge their counts;
- file-level cleanup mapping does not produce callable-level exact binding.

**Step 2: Run the verifier**

Expected: fail because adapters and status propagation are absent.

**Step 3: Implement lazy readers**

Validate schema, snapshot, hash and capability before registration. Index shard metadata first; load shard content only on query. Preserve unknown fields for forward compatibility where practical, but do not infer missing semantics.

**Step 4: Verify against current artifacts**

Run the reader verifier against fixture artifacts first, then read the current Version4 manifest and both cleanup manifests in read-only mode. Expected: current partial/lineage facts match the report baseline; no source or existing report file changes.

**Exit gate:** an empty list is returned as “no match” only when the reader proves complete coverage for the requested scope.

### Task 4: Add explicit binding store and evidence explanation

**Files:**

- Create: `tools/AnalysisEvidenceCatalog/Binding/BindingResult.cs`
- Create: `tools/AnalysisEvidenceCatalog/Binding/ExplicitBindingStore.cs`
- Create: `tools/AnalysisEvidenceCatalog/Binding/BindingInvalidation.cs`
- Create: `tools/AnalysisEvidenceCatalog/Query/EvidenceTrace.cs`
- Modify: `tools/AnalysisEvidenceCatalog.Verification/Program.cs`

**Step 1: Write failing binding tests**

Cover exact symbol binding, candidate overloads, legacy artifact without snapshot, unresolved call, partial source and source/configuration invalidation. Assert `MatchBasis`, candidates, artifact refs and diagnostics are present.

**Step 2: Run to confirm failure**

Expected: missing store and trace implementation.

**Step 3: Implement explicit binding**

Bind only compatible `SourceSnapshotId` values and exact canonical identities. Store candidate sets and invalidation conditions as first-class data. A heuristic may produce a candidate but cannot produce `confirmed`.

**Step 4: Verify explanation completeness**

For every status except a fully confirmed exact binding, `Explain` must return at least one reason, evidence ref or gap. Expected: no status is represented by a bare Boolean.

**Exit gate:** catalog preserves evidence level, derivation level and implementation status separately.

### Task 5: Implement `ExecutionDag` and authorization barrier

**Files:**

- Create: `tools/AnalysisEvidenceCatalog/Graphs/ExecutionDag.cs`
- Create: `tools/AnalysisEvidenceCatalog/Graphs/ExecutionStage.cs`
- Create: `tools/AnalysisEvidenceCatalog/Graphs/ExecutionDependency.cs`
- Create: `tools/AnalysisEvidenceCatalog/Graphs/StageStatus.cs`
- Create: `tools/AnalysisEvidenceCatalog/Graphs/ExecutionBarrier.cs`
- Modify: `tools/AnalysisEvidenceCatalog/AnalysisEvidenceCatalog.cs`
- Modify: `tools/AnalysisEvidenceCatalog.Verification/Program.cs`

**Step 1: Write failing stage tests**

Assert that call/dataflow stages can be independent, binding waits for both declared inputs, cleanup cannot run without authorization, a failed input propagates to dependent stages, and a completed stage can be rerun with a new run ID without overwriting old artifacts.

**Step 2: Run to confirm failure**

Expected: no dependency or barrier behavior exists.

**Step 3: Implement the minimal DAG**

Validate duplicate nodes, missing inputs and cycles. Store artifact refs, capabilities, failure reasons, barrier and rerun policy. Stage nodes must not be individual callable nodes.

**Step 4: Verify failure and rerun semantics**

Use an in-memory fixture and inspect serialized stage status and artifact lineage. Expected: cleanup remains blocked until the explicit authorization node completes.

**Exit gate:** DAG order is never used to infer owner, fact convergence or relation semantics.

### Task 6: Implement `RelationMultigraph`

**Files:**

- Create: `tools/AnalysisEvidenceCatalog/Graphs/RelationMultigraph.cs`
- Create: `tools/AnalysisEvidenceCatalog/Graphs/RelationEdge.cs`
- Create: `tools/AnalysisEvidenceCatalog/Model/RelationEdgeId.cs`
- Create: `tools/AnalysisEvidenceCatalog/Query/RelationQuery.cs`
- Create: `tools/AnalysisEvidenceCatalog/Query/RelationQueryResult.cs`
- Modify: `tools/AnalysisEvidenceCatalog.Verification/Program.cs`

**Step 1: Write failing relation tests**

Add two edges with the same source/target but kinds `Calls` and `Reads`; add two sources converging to one node; add a cycle. Assert both edges, source identity and edge-distinct paths survive serialization and bounded traversal.

**Step 2: Run to confirm failure**

Expected: missing edge identity and traversal implementation.

**Step 3: Implement edge-first storage**

Use `RelationEdgeId` containing kind, context and stable ordinal. Cache/index by snapshot and relation kind, not only endpoint pair. Track visited state by source plus edge/path context where the query profile requires it.

**Step 4: Verify budget behavior**

Set a small traversal budget and assert `truncated` with a reason and metrics. Expected: a bounded result is never labeled `complete` merely because traversal stopped.

**Exit gate:** relation queries do not synthesize `FactClosure` convergence or execution ordering.

### Task 7: Add conditional `FactClosure`

**Files:**

- Create only after the consumer decision: `tools/AnalysisEvidenceCatalog/Graphs/FactClosure.cs`
- Create only after the consumer decision: `tools/AnalysisEvidenceCatalog/Graphs/FactKey.cs`
- Create only after the consumer decision: `tools/AnalysisEvidenceCatalog/Graphs/Derivation.cs`
- Create only after the consumer decision: `tools/AnalysisEvidenceCatalog.Verification/FactClosureVerification.cs`
- Modify: `docs/research/2026-09-16-ecs-system-domain-splitting-unified-evidence-api-dag-design.md`

**Decision gate before code:** identify a real recursive propagation consumer, list payload fields that affect downstream decisions, define canonical fact identity, and state the required provenance depth/budget. If no consumer is confirmed, record this task as `deferred` and do not create empty graph types.

**Step 1: Write the consumer-specific failing fixture**

Include seed facts, a recursive derivation, an SCC/cycle, duplicate derivations for one fact, a non-identity payload and a source-resolution fallback.

**Step 2: Implement worklist/fixed-point behavior**

Keep canonical facts separate from derivations. Return convergence status, iterations, SCC information and bounded explanations. Do not expose a fake topological order.

**Step 3: Verify identity policy**

Assert that payload fields classified as decision-relevant produce distinct fact keys and explanation-only payloads aggregate as derivation metadata.

**Exit gate:** `FactClosure` is only implemented with a named consumer and an approved payload matrix.

### Task 8: Implement bounded `AnalysisEvidenceApi`

**Files:**

- Create: `tools/AnalysisEvidenceCatalog/Query/AnalysisEvidenceApi.cs`
- Create: `tools/AnalysisEvidenceCatalog/Query/StageQuery.cs`
- Create: `tools/AnalysisEvidenceCatalog/Query/EntityQuery.cs`
- Create: `tools/AnalysisEvidenceCatalog/Query/QueryBudget.cs`
- Create: `tools/AnalysisEvidenceCatalog/Query/ContinuationToken.cs`
- Create: `tools/AnalysisEvidenceCatalog/Query/QueryResults.cs`
- Create: `tools/AnalysisEvidenceCatalog/Storage/QueryCache.cs`
- Modify: `tools/AnalysisEvidenceCatalog/AnalysisEvidenceCatalog.cs`
- Modify: `tools/AnalysisEvidenceCatalog.Verification/Program.cs`

**Step 1: Write failing API contract tests**

Cover `GetStage`, `GetEntity`, `GetRelations` and `Explain`. Assert required snapshot/stage selection, ambiguous missing run, selector filtering, diagnostics/unknowns, max items/bytes/shard loads, continuation token and cache invalidation.

**Step 2: Run to confirm failure**

Expected: API methods, budget enforcement and continuation token validation are absent.

**Step 3: Implement the read facade**

Return metadata/counts/artifact refs before lazy facts. Include stage status and query metrics. Validate continuation tokens against snapshot, query profile, selector and API schema; reject tokens from a different snapshot.

**Step 4: Verify K1-K12 mapping**

Map each design acceptance case to one focused assertion. For K4/K5/K11, explicitly assert partial/unavailable/truncated status instead of only checking returned item counts.

**Exit gate:** no `GetAll` or unbounded stage endpoint exists.

### Task 9: Add cleanup transformation map and isolated commit path

**Files:**

- Modify: `.agents/skills/ecs-system/tools/FunctionBodyCleanup/ProjectCleanupRunner.cs` only if the map can be added without changing current cleanup semantics
- Modify: `.agents/skills/ecs-system/tools/FunctionBodyCleanup/SourceFileCleanup.cs` only if function-level anchors are available without unsafe rewriting
- Create: `tools/AnalysisEvidenceCatalog/Artifacts/CleanupTransformationMap.cs`
- Create: `tools/AnalysisEvidenceCatalog/Storage/AtomicCatalogWriter.cs`
- Modify: `tools/AnalysisEvidenceCatalog.Verification/Program.cs`

**Step 1: Write failing lineage tests**

Assert original source hashes are unchanged, original and cleaned roots are distinct, file-level-only mapping returns `partial`, function-level map returns exact only for mapped functions, failed replacement leaves the previous index readable, and a cleanup run never merges with another run.

**Step 2: Decide whether source cleanup changes are necessary**

If the current cleanup implementation cannot provide safe function anchors, keep the reader file-level and mark callable exact binding deferred. Do not add guessed line mappings.

**Step 3: Implement atomic catalog write**

Write a new run/index under a unique temporary directory, flush/close it, validate manifest and content hashes, then atomically replace the current catalog pointer. Preserve the previous pointer on failure.

**Step 4: Verify rollback**

Inject a failed write or invalid hash and assert the previous catalog remains queryable and no source file changes. Expected: recovery does not require deleting generated data.

**Exit gate:** cleanup is never automatically invoked by a read query; only an authorized execution stage may submit it.

### Task 10: Integration, migration switch and deletion review

**Files:**

 Create: `.agents/skills/ecs-system/tools/scripts/tests/Test-AnalysisEvidenceCatalog.ps1`
 Create: `.agents/skills/ecs-system/tools/scripts/tests/Test-AnalysisEvidenceIntegration.ps1`
- Modify: `.agents/skills/ecs-system/README.md` if user-facing usage is added
- Modify: this execution document with actual commands and results
- Do not delete existing analyzer fields, launchers, manifests or cleanup outputs in this task

**Step 1: Run existing regression fixtures serially**

Run the existing CalledFunctions, ProjectDataflow and cleanup verification suites one at a time. Record exact command, exit code, warning/error count and artifact path.

**Step 2: Run catalog fixture suite**

Use the new PowerShell verifier and the verification project against deterministic fixture inputs. Expected: K1-K12 pass except any explicitly deferred K10/K7 case, which must be reported as deferred rather than skipped silently.

**Step 3: Run read-only integration against current artifacts**

Read the current dataflow manifest and both cleanup manifests. Compare snapshot/run identity, status, diagnostics and counts with the research baseline. No source or pre-existing report file may be modified.

**Step 4: Exercise opt-in migration switch**

Run old reader and catalog reader against the same artifact set. Compare identity, status and relation results. The catalog reader may add fields, but it may not erase or reinterpret unresolved/partial facts.

**Step 5: Review deletion gate**

Do not remove compatibility fields or old facades unless all of the following have evidence: static/config/reflection/generated/runtime caller closure; new/old differential results; lifecycle/exception/entity-destruction/multi-world semantics; network/save/recovery if applicable; retry/rollback/effect owner; affected project build and no-build verifier; performance/batching/structural-change review; traceable rollback point.

**Exit gate:** the migration switch remains reversible, and the catalog is not called “complete” solely because it compiles or fixture tests pass.

**Deletion gate review result (executed 2026-09-16).** No compatibility field, legacy reader, launcher, manifest or cleanup output was deleted. Every gate is recorded as passed, partial, not applicable, or blocking.

| Gate | Verdict | Evidence, or the evidence that is missing |
| --- | --- | --- |
| static/config/reflection/generated/runtime caller closure | partial | Static part measured: no C# project outside `.agents` references `FunctionBodyCleanup`, `CalledFunctionsAnalyzer` or `ProjectDataflowAnalyzer`, and the only references to `AnalysisEvidenceCatalog`/`AnalysisEvidenceApi` outside the tool projects are documentation, `README.md` and the two test scripts. Config, reflection, generated and runtime closure are **not** measured. |
| new/old differential results | partial | Counts, run identity, status and evidence references agree between the raw manifests and the catalog readers (dataflow 967 shards / 7855 indexed / 7157 analyzed / 184 diagnostics; cleanup 6057 and 6058 cleaned functions over 967 files in two distinct runs). No field-by-field differential of reads, writes or relations exists. |
| lifecycle/exception/entity-destruction/multi-world semantics | not applicable | The catalog is a read model over analyzer artifacts; it owns no entity, world or lifecycle state. |
| network/save/recovery | not applicable | No networked or persisted game state is read or written by the catalog. |
| retry/rollback/effect owner | passed | An atomic index failure keeps the previous pointer and preserves the staging directory for diagnosis (fixture-asserted); cleanup failure stages are explicit; the catalog assembly carries no reference to cleanup tooling, so a read query cannot become a second committer. |
| affected project build and no-build verifier | passed | Four projects build with 0 warnings / 0 errors through the serial wrapper, and the catalog verifier also runs with `--no-build --no-restore`. |
| performance/batching/structural-change review | **blocking** | Budget and truncation behaviour is asserted, but no timing or throughput measurement over the real Version4 set was taken and no structural-change review of the 967-shard read path exists. |
| traceable rollback point | **blocking** | `.gitignore:27` ignores `.agents`, so the catalog, its fixtures and the two test scripts are untracked: there is **no VCS rollback point**. Rollback currently relies on the additive migration switch (disable the catalog and keep reading legacy manifests) plus the preserved atomic-writer pointer. |

**Deletion decision: blocked.** The catalog is additive, so retaining the old readers and manifests costs nothing, and two gates have no evidence. The migration switch stays reversible and the compatibility path stays reachable.

## 5. Verification command ledger

The implementer must replace the placeholders below with actual output after each task. This section is intentionally empty of success claims until execution occurs.

All `dotnet` invocations below were issued from the repository root through `Build/Tools/Invoke-SerialDotnet.ps1`; each compile was preceded by a check for an active `dotnet.exe`/`csc.exe` build process and reported `no active builds`. Verifier runs used an already built artifact (`--no-build --no-restore`) and changed only temporary output.

| Date | Task | Command | Project/input | Exit code | Warnings/errors | Output path | Status |
| --- | --- | --- | --- | ---: | --- | --- | --- |
| 2026-09-16 | Task 8/9 build | `Invoke-SerialDotnet.ps1 -- build <verification csproj> '-maxcpucount:1' '-nodeReuse:false' '-p:UseSharedCompilation=false' '-p:MSBuildNodeReuse=false' '-p:BuildInParallel=false'` | `tools/AnalysisEvidenceCatalog.Verification` (pulls in `tools/AnalysisEvidenceCatalog`) | 0 | 0 warnings / 0 errors | `Build/bin/AnalysisEvidenceCatalog.Verification/Debug/net10.0/AnalysisEvidenceCatalog.Verification.dll` | `pass` |
| 2026-09-16 | Task 8/9 catalog fixture suite | `Invoke-SerialDotnet.ps1 -- run --project <verification csproj> '--no-build' '--no-restore'` | `scripts/tests/fixtures/AnalysisEvidenceFixture` | 0 | n/a (no compile) | stdout `DEFERRED: K10 recursive fact closure (deferred:FactClosureNotImplemented) is not implemented; no convergence or derivation query is claimed.` then `PASS: analysis evidence envelope, registry, readers, binding, execution DAG, relation multigraph, bounded API and isolated cleanup commit path` | `pass` (K1–K9, K11, K12; K7 and K10 deferred and reported) |
| 2026-09-16 | Task 10 Step 2 | `pwsh -NoProfile -File .agents/skills/ecs-system/tools/scripts/tests/Test-AnalysisEvidenceCatalog.ps1` | catalog fixture suite; temp root `Build/tmp/analysis-evidence-catalog` | 0 | n/a | stdout `PASS: analysis evidence catalog fixture suite (K1-K9, K11, K12; K7 and K10 deferred and reported)`; the script asserts the K10 `DEFERRED` line exists; 0 temporary items left behind | `pass` |
| 2026-09-16 | Task 10 Step 3/4 | `Invoke-SerialDotnet.ps1 -- run --project <verification csproj> '--no-build' '--no-restore' '--' '--integration'` | real Version4 dataflow manifest + both cleanup manifests | 0 | n/a (no compile) | stdout `PASS: read-only integration over current Version4 artifacts matches the research baseline without modifying any source or report file` | `pass` |
| 2026-09-16 | Task 10 Step 3/4 | `pwsh -NoProfile -File .agents/skills/ecs-system/tools/scripts/tests/Test-AnalysisEvidenceIntegration.ps1` | same three manifests; SHA-256 compared before and after | 0 | n/a | stdout `PASS: read-only integration over current Version4 artifacts (no file modified)` | `pass` |
| 2026-09-16 | Task 10 Step 1 build | serial wrapper `build` | `scripts/CalledFunctionsAnalyzer` | 0 | 0 warnings / 0 errors | `Build/bin/CalledFunctionsAnalyzer/Debug/net10.0/CalledFunctionsAnalyzer.dll` | `pass` |
| 2026-09-16 | Task 10 Step 1 build | serial wrapper `build` | `scripts/ProjectDataflowAnalyzer` | 0 | 0 warnings / 0 errors | `Build/bin/ProjectDataflowAnalyzer/Debug/net10.0/ProjectDataflowAnalyzer.dll` | `pass` |
| 2026-09-16 | Task 10 Step 1 build | serial wrapper `build` | `tools/FunctionBodyCleanup.Verification` | 0 | 0 warnings / 0 errors | `Build/bin/FunctionBodyCleanup.Verification/Debug/net10.0/FunctionBodyCleanup.Verification.dll` | `pass` |
| 2026-09-16 | Task 10 Step 1 | `pwsh -NoProfile -File .../Test-CalledFunctions.ps1` with `TEMP`/`TMP` = `Build/tmp/evidence-tests` | call graph fixture (`CallGraphFixture`) | 0 | n/a | stdout `PASS: called-functions analyzer fixture` | `pass` |
| 2026-09-16 | Task 10 Step 1 | `pwsh -NoProfile -File .../Test-Run-CalledFunctionsAnalyzer.ps1` with `TEMP`/`TMP` = `Build/tmp/evidence-tests` | analyzer PowerShell launcher | 0 | n/a | stdout `PASS: called-functions PowerShell launcher` | `pass` |
| 2026-09-16 | Task 10 Step 1 | `pwsh -NoProfile -File .../Test-ProjectDataflowAnalyzer.ps1` with `TEMP`/`TMP` = `Build/tmp/evidence-tests` | project dataflow fixture | 0 | n/a | stdout `PASS: project variable dataflow analyzer fixture` | `pass` |
| 2026-09-16 | Task 10 Step 1 | `dotnet Build/bin/FunctionBodyCleanup.Verification/Debug/net10.0/FunctionBodyCleanup.Verification.dll` | cleanup fixture set | 0 | n/a | stdout `Function body cleanup verification passed.` | `pass` |
| 2026-09-16 | Task 10 Step 1 (environment) | the same three PowerShell suites **without** redirecting `TEMP` | ambient `TEMP` = `C:\WINDOWS\TEMP` | 1 | n/a | `Remove-Item -LiteralPath <dir> -Recurse -Force` → `拒绝访问。` at `Test-CalledFunctions.ps1:163`, `Test-Run-CalledFunctionsAnalyzer.ps1:62`, `Test-ProjectDataflowAnalyzer.ps1:195` | `environment-blocked` |

Environment note for the last row: `[System.IO.Path]::GetTempPath()` resolves to `C:\WINDOWS\TEMP\` for this account (`16ach-7900x\shan`). That directory denies enumeration and `Get-Acl`, and `Remove-Item -Recurse -Force` on a self-created child directory is refused, while `[System.IO.Directory]::Delete($path, $true)`, `cmd /c rd /s /q` and `Remove-Item -Recurse -Force` under `Build\tmp\` all succeed. The three suites therefore failed only in their `finally` cleanup step, which also risked masking a real assertion failure. Redirecting `TEMP`/`TMP` to a repository-local temporary root makes all three pass unchanged. **Minimum next evidence:** make the temporary root explicit in the affected suites (or state the redirect in the runbook) so a future run cannot report an access-denied cleanup as a verification result.

For a compile-capable run, record the wrapper command in full. For a verifier run, record whether it used an already built artifact and whether it changed only a temporary output directory.

### 5.1 Unresolved, partial and deferred conditions

Every non-`confirmed` condition carried by this work, with the minimum next evidence that would clear it. None of these is silently skipped.

| Condition | Status | Why it stands | Minimum next evidence |
| --- | --- | --- | --- |
| Recursive fact closure (acceptance case K10) | `deferred` | `FactClosure` is not implemented, so closure convergence and derivation queries have no coverage. The verifier prints `DEFERRED: K10 recursive fact closure (deferred:FactClosureNotImplemented)` and `Test-AnalysisEvidenceCatalog.ps1` asserts that line exists, so the case cannot be silently counted as passing. | A concrete consumer needing recursive propagation, plus the payload identity matrix and provenance requirements named in the design's conditional phase. |
| Callable-level exact cleanup binding (acceptance case K7) | `deferred` | The Roslyn rewriter is purely syntactic, `RewriteStatistics` exposes only aggregate counters, and cleanup manifest schema 1.0 records no per-function entry, so no safe anchor exists. Design §8 forbids file-level lineage from claiming callable precision. See `references/cleanup-transformation-map-decision.md`. | A named consumer, per-declaration syntax anchors emitted by the rewriter, a schema version that keeps 1.0 fields, and a fixture proving mapped callables exact while unmapped stay partial. |
| `FactClosure` conditional/lazy closure (design Task 7) | `deferred` | No consumer needs it yet, and an empty closure type would create a second vocabulary with no evidence behind it. | A concrete query that must answer “what is only conditionally reachable”, plus the conditional/delayed keys it would key on. |
| Dataflow stage completeness | `partial` | The current manifest is itself `partial`; the reader now records `ManifestCoverageIncomplete` with a reference instead of returning a smaller complete set. | A manifest whose `status` is `confirmed`, or a per-shard analysis count so coverage can be compared rather than inferred. |
| Legacy dataflow identity | `partial` | Schema 1.0 carries no native run, snapshot or configuration identity, so the reader registers `LegacyCompatibility` with `MissingIdentityField`. | Emit `runId`, `sourceSnapshotId` and `configurationDigest` in the analyzer manifest. |
| Performance of the 967-shard read path | `unmeasured` | Budget and truncation are asserted, but no timing or throughput measurement was taken. | A recorded timing run over the real Version4 manifest, including shard-load cost. |
| VCS rollback point for the skill | `absent` | `.gitignore:27` ignores `.agents`, so the catalog, its fixtures and the test scripts are untracked. | Either track the skill tree, or record the rollback as “delete the additive tool projects and disable the switch”, accepted explicitly by the owner. |

## 6. Rollback and failure handling

| Failure | Detection | Recovery | Do not do |
| --- | --- | --- | --- |
| schema mismatch | envelope validation diagnostic | register as unavailable/legacy; keep old artifact | silently drop fields |
| snapshot mismatch | key/artifact validation | isolate run and mark binding invalidated | bind by path alone |
| missing dataflow shard | shard load diagnostic | return unavailable/unknown with shard ref | return a smaller complete set |
| ambiguous callable | multiple canonical candidates | return candidates and defer binding | choose latest/shortest name |
| cleanup map unavailable | no function-level mapping | keep file-level partial lineage | infer from line numbers |
| atomic index failure | write/hash/replace exception | keep previous pointer and temp run for diagnosis | delete previous index |
| verifier regression | non-zero exit or changed baseline | stop migration switch, retain compatibility path | remove old analyzer output |
| overlapping build | active process owner unclear | wait and rerun serially | terminate process or build in parallel |

## 7. Completion definition

The execution work is complete only when:

- the required catalog/API code and focused verifiers exist at the approved paths;
- all required stage, identity, binding, graph, query, rollback and compatibility gates are evidenced;
- actual build/test commands comply with the serial .NET contract and their outputs are recorded;
- generated artifacts are under approved `Build/` or temporary locations;
- all unresolved/partial/unknown conditions are listed with minimum next evidence;
- the reversible migration switch is exercised;
- deletion review explicitly records which gates are not applicable, which are passed, and which block deletion;
- the design and execution documents agree on stage names, key semantics, status vocabulary and current implementation status.

“编译成功”“fixture 通过”“catalog audit ok”任何单项都不能单独表示旧实现可删除或全项目语义已证明。

## 8. References

- [统一证据 API 与三层图模型设计](2026-09-16-ecs-system-domain-splitting-unified-evidence-api-dag-design.md)
- [联合证据 API/DAG 研究报告](2026-09-16-ecs-system-domain-splitting-unified-evidence-api-dag-research.md)
- [ECS System 领域拆分技能](../../.agents/skills/ecs-system/SKILL.md)
 [系统拆分规则](../../.agents/skills/ecs-system/system-splitting/references/system-decomposition-rules.md)
 [证据循环与迁移协议](../../.agents/skills/ecs-system/system-splitting/references/evidence-and-migration-protocol.md)
- [构建与验证约束](../../Context/约束/构建与验证约束.md)
- [输出风险约束](../../.agents/skills/ecs-system/reports/output-risk-profile.md)
