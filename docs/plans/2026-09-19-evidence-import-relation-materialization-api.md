# Evidence Import And Relation Materialization API Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Add an explicit evidence import pipeline that reads catalog-indexed artifacts through the existing readers, registers immutable stage results, materializes supported relation edges, and attaches the execution DAG so `AnalysisEvidenceApi` can query the imported graph.

**Architecture:** The pipeline is index-driven: each catalog entry is resolved through an explicit reader descriptor and context, never by recursively scanning report directories. A reader result is registered first; a separate relation materializer then converts only facts with sufficient identity into normalized relation candidates, merges duplicate edge identities while accumulating evidence, and submits them to `RelationMultigraph`. Execution ordering remains in `ExecutionDag`; relation edges are semantic evidence and are never used as schedule edges.

**Tech Stack:** C# `net10.0`, nullable reference types, `System.Text.Json`, existing `ArtifactReader` implementations, `AnalysisEvidenceCatalog`, `RelationMultigraph`, `ExecutionDag`, and the serial dotnet wrapper at `Build/Tools/Invoke-SerialDotnet.ps1`.

---

## Scope and invariants

The first implementation supports explicit import of the existing dataflow and called-functions artifacts, plus the existing cleanup transformation result as a stage-only artifact. It materializes `Calls`, `Reads`, `Writes`, `Captures`, and `Transforms` only when the fact payload contains enough canonical endpoint information. Unsupported or incomplete facts remain visible as gaps; they must never be promoted to confirmed relations by guessing a caller, owner, lifecycle edge, event edge, reflection edge, or System boundary.

The importer must preserve these invariants:

- The requested `SourceSnapshotId`, native artifact identity, configuration digest, content hash, and run identity are validated before registration.
- Legacy schema-1 artifacts remain legacy/partial evidence. Caller-supplied context may identify them for reading, but must not upgrade them to confirmed evidence.
- A relation candidate is keyed by source snapshot, source, target, relation kind, structured label, and stable ordinal. Duplicate facts merge evidence references and retain the weakest status and all distinct diagnostics.
- The relation multigraph may contain cycles. Only the explicitly supplied execution dependencies are passed to `ExecutionDag`, which continues to enforce cycle checks.
- The importer is deterministic: descriptors, facts, candidates, evidence references, and diagnostics are normalized before mutation.
- The pipeline is explicit and bounded. It does not scan `reports/`, `managed/`, `dist/`, or any other directory to discover artifacts.

## Planned public API

The names below are the intended first-version API. Keep the types in the existing `EcsSystemDomainSplitting.AnalysisEvidence` namespace and adjust only when an existing type makes a new type unnecessary.

```csharp
public sealed record EvidenceArtifactDescriptor(
  string StageId,
  string ArtifactPath,
  string Producer,
  string ArtifactKind,
  ArtifactReaderContext Context,
  IReadOnlyList<string> DeclaredInputs,
  ExecutionStageKind StageKind = ExecutionStageKind.Analysis,
  bool IsRerunnable = true,
  IReadOnlyList<ArtifactRef>? Inputs = null,
  IReadOnlyList<string>? RequiredCapabilities = null,
  IReadOnlyList<string>? ActualCapabilities = null);

public sealed record EvidenceImportRequest(
  SourceSnapshotId SourceSnapshotId,
  AnalysisRunId RunId,
  CatalogIndex Index,
  IReadOnlyList<EvidenceArtifactDescriptor> Artifacts,
  IReadOnlyList<ExecutionDependency> Dependencies,
  IReadOnlyList<ExecutionBarrier> Barriers);

public sealed record RelationCandidate(
  EntityKey Source,
  EntityKey Target,
  RelationKind Kind,
  string StructuredLabel,
  int StableOrdinal,
  EvidenceStatus Status,
  IReadOnlyList<ArtifactRef> EvidenceRefs,
  IReadOnlyList<Diagnostic> Diagnostics);

public sealed record EvidenceImportResult(
  IReadOnlyList<StageRegistration> Stages,
  IReadOnlyList<RelationEdge> Relations,
  IReadOnlyList<EvidenceGap> Gaps,
  IReadOnlyList<Diagnostic> Diagnostics,
  ExecutionDag ExecutionDag);

public sealed class EvidenceArtifactReaderRegistry
{
  public static EvidenceArtifactReaderRegistry CreateDefault();
  public void Register(
    string producer,
    string artifactKind,
    EvidenceArtifactReader reader,
    EvidenceArtifactResultExpander? expander = null);
  public ArtifactReadResult Read(EvidenceArtifactDescriptor descriptor);
}

public sealed class RelationMaterializer
{
  public IReadOnlyList<RelationCandidate> CreateCandidates(ArtifactReadResult result);
  public RelationMaterializationResult CreateReport(ArtifactReadResult result);
  public IReadOnlyList<RelationEdge> Materialize(
    AnalysisEvidenceCatalog catalog,
    IEnumerable<ArtifactReadResult> results);
  public RelationMaterializationResult MaterializeWithReport(
    AnalysisEvidenceCatalog catalog,
    IEnumerable<ArtifactReadResult> results);
}

public sealed class EvidenceImportPipeline
{
  public EvidenceImportResult Import(
    AnalysisEvidenceCatalog catalog,
    EvidenceImportRequest request,
    EvidenceArtifactReaderRegistry readers);
}
```

`EvidenceArtifactResultExpander` is used by the default dataflow registration to load every
explicitly indexed shard during an import. This makes relation materialization complete for the
artifact set selected by the request while leaving the catalog query API's existing lazy-shard
behavior unchanged. Call-summary facts without an explicit caller endpoint remain gaps; the
pipeline does not infer a caller from a flattened or transitive summary.

`CatalogIndex` remains the persisted publication record and is not silently expanded into a directory manifest. The pipeline validates that every requested descriptor matches an index entry by run, stage, path, and content hash. Missing, duplicated, or unindexed descriptors fail before catalog mutation. `EvidenceArtifactReaderRegistry` uses `(Producer, ArtifactKind)` as the dispatch key; the descriptor supplies the reader context because schema-1 artifacts require caller-supplied identity while schema-2 artifacts must match native identity.

## Fact-to-relation rules

The materializer must use structured payload parsing and produce a gap instead of a guessed edge when a rule cannot be satisfied.

| Fact kind | Relation | Required endpoint information | Status rule |
| --- | --- | --- | --- |
| `Calls` | `Calls` | Canonical caller and callee keys in the payload; a selected-call summary without caller identity is not enough | Weakest fact/endpoint status; unresolved or non-static dispatch stays partial/ambiguous/unresolved |
| `Reads` / dataflow read entry | `Reads` | Canonical callable and field/resource key | Weakest status of fact and parsed endpoint |
| `Writes` / dataflow write entry | `Writes` | Canonical callable and field/resource key | Weakest status of fact and parsed endpoint |
| `Captures` / dataflow capture entry | `Captures` | Canonical callable and captured entity key | Weakest status of fact and parsed endpoint |
| cleanup transformation fact | `Transforms` | Explicit original and cleaned entity/file keys | File-level cleanup remains partial and cannot claim callable-level exactness |

Stable ordinals are assigned from normalized fact order within one artifact, not completion order. Evidence references are deduplicated by artifact identity and path. Diagnostics are deduplicated by code, message, path, line, and column. The materializer must not create `LifecycleCreates`, `LifecycleDestroys`, `EvidenceFor`, owner, or System-boundary edges in this first version.

## Execution steps

### Task 1: Add the failing core verification

**Files:**
- Modify: `.agents/skills/ecs-system-domain-splitting/tools/AnalysisEvidenceCatalog.Verification/Program.cs`

Add one focused verifier routine for the import pipeline. It must construct an explicit index and descriptors over existing fixtures, register a structured fixture reader, invoke the pipeline, and assert all of the following:

1. A descriptor imports and registers its stage without a manual `RegisterStage` call.
2. Supported structured dataflow/call facts produce queryable `Calls`, `Reads`, and `Writes` edges.
3. Duplicate relation facts produce one edge with merged evidence rather than an `AddEdge` collision.
4. A legacy/partial artifact remains partial after import and contributes a partial edge or gap.
5. The supplied execution dependencies and barriers are attached to the catalog DAG, while a relation cycle is not treated as a DAG failure.
6. A path/hash/index mismatch is rejected before the catalog receives a partial registration.

The same routine must also exercise `EvidenceArtifactReaderRegistry.CreateDefault()` against the
existing partial dataflow fixture and assert that its indexed shard is expanded and yields
`Reads`/`Writes` candidates. The verifier's fixture resolver must accept the current repository
location under `tools/scripts/tests/fixtures` and retain compatibility with the former location.

Run the verifier through the serial wrapper and confirm that it fails because the new import API does not yet exist. Do not weaken the assertions to make the pre-implementation run pass.

```powershell
pwsh -File Build/Tools/Invoke-SerialDotnet.ps1 -- run --project .agents/skills/ecs-system-domain-splitting/tools/AnalysisEvidenceCatalog.Verification/AnalysisEvidenceCatalog.Verification.csproj
```

Expected result: compilation failure naming the missing import API, with no production implementation added yet.

### Task 2: Add descriptor and reader-dispatch models

**Files:**
- Create: `.agents/skills/ecs-system-domain-splitting/tools/AnalysisEvidenceCatalog/Import/EvidenceArtifactDescriptor.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/AnalysisEvidenceCatalog/Import/EvidenceImportRequest.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/AnalysisEvidenceCatalog/Import/EvidenceArtifactReaderRegistry.cs`
- Modify: `.agents/skills/ecs-system-domain-splitting/tools/AnalysisEvidenceCatalog/Storage/AtomicCatalogWriter.cs` only if a small index lookup helper is needed

Implement immutable descriptor/request records and a registry that maps producer plus artifact kind to a reader delegate. Validate non-empty identifiers, reject duplicate registration keys, normalize path comparison with full paths, and expose no fallback directory search. Keep reader construction outside the catalog so the read model does not reference analyzer-specific assemblies.

Add index validation that matches `(RunId, StageId, ArtifactPath, ContentHash)` and rejects an entry whose persisted status or path does not match the requested descriptor. The check must run before calling any reader or mutating `AnalysisEvidenceCatalog`.

### Task 3: Implement fact parsing and candidate normalization

**Files:**
- Create: `.agents/skills/ecs-system-domain-splitting/tools/AnalysisEvidenceCatalog/Import/RelationCandidate.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/AnalysisEvidenceCatalog/Import/RelationMaterializer.cs`
- Modify: `.agents/skills/ecs-system-domain-splitting/tools/AnalysisEvidenceCatalog/Artifacts/CalledFunctionsArtifactReader.cs` only if the existing payload needs a backward-compatible structured caller/callee representation
- Modify: `.agents/skills/ecs-system-domain-splitting/tools/AnalysisEvidenceCatalog/Artifacts/ProjectDataflowArtifactReader.cs` only if the existing fact payload needs a backward-compatible structured read/write/capture representation

Parse JSON payloads using `JsonDocument` or the existing support helpers. Do not infer a caller from a selected-call summary, do not treat a file path as a callable key when a canonical key is required, and preserve the existing legacy and non-static dispatch gaps. Convert accepted facts to candidates, sort them deterministically, then merge identical candidate identities while unioning evidence references and diagnostics and applying `EvidenceStatusAggregation.Weakest`.

### Task 4: Make relation insertion idempotent for materialization

**Files:**
- Modify: `.agents/skills/ecs-system-domain-splitting/tools/AnalysisEvidenceCatalog/Graphs/RelationMultigraph.cs`

Add the public `AddOrMergeEdge` operation suitable for the materializer. It must preserve the current edge identity contract, update an existing edge with the weakest status plus the union of evidence references and diagnostics, and update all adjacency indexes exactly once. Existing direct `AddEdge` behavior for callers that accidentally register an identical edge remains strict.

The implementation must invalidate the immutable graph snapshot and invoke the catalog revision callback once per observable merge, not once per duplicate evidence reference.

### Task 5: Implement the import pipeline and DAG attachment

**Files:**
- Create: `.agents/skills/ecs-system-domain-splitting/tools/AnalysisEvidenceCatalog/Import/EvidenceImportResult.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/AnalysisEvidenceCatalog/Import/EvidenceImportPipeline.cs`
- Modify: `.agents/skills/ecs-system-domain-splitting/tools/AnalysisEvidenceCatalog/AnalysisEvidenceCatalog.cs` only for a narrow atomic registration/materialization helper if required

Implement the ordered transaction:

1. Validate request snapshot, index, descriptors, paths, hashes, and duplicate keys.
2. Resolve and read all artifact results without mutating the catalog.
3. Build the execution DAG from declared stage descriptors and explicit dependencies/barriers; let existing cycle and missing-stage checks fail the request.
4. Preflight all relation candidates in a temporary graph so malformed edge identity cannot leave a partially registered catalog.
5. Register all stages.
6. Materialize and merge relation candidates.
7. Attach the validated DAG to the catalog.
8. Return imported stages, materialized edges, diagnostics, gaps, and the DAG.

If a later step fails, the pipeline must not leave a half-imported catalog. Use preflight validation and/or a temporary catalog plan so the current catalog mutation is committed only after all input checks, reader calls, and DAG validation succeed. Do not roll back unrelated pre-existing stages or relations.

### Task 6: Run the core 10% verification

**Files:**
- Modify: `.agents/skills/ecs-system-domain-splitting/tools/AnalysisEvidenceCatalog.Verification/Program.cs` only as needed to integrate the focused routine into the existing verifier entry point

Before any build or run, verify `dotnet.exe` and `csc.exe` are discoverable. Then run only the affected verification project through the serial wrapper; do not build the full solution and do not run unrelated migration test suites.

```powershell
Get-Command dotnet -ErrorAction Stop
& 'C:\Program Files\dotnet\sdk\10.0.400\Roslyn\bincore\csc.exe' -help *> $null
$project = '.agents/skills/ecs-system-domain-splitting/tools/AnalysisEvidenceCatalog.Verification/AnalysisEvidenceCatalog.Verification.csproj'
pwsh -NoProfile -Command "& '.\Build\Tools\Invoke-SerialDotnet.ps1' -DotnetArguments @('build', '$project', '--no-restore', '/m:1', '/nr:false', '/p:UseSharedCompilation=false', '/p:MSBuildNodeReuse=false', '/p:BuildInParallel=false')"
pwsh -NoProfile -Command "& '.\Build\Tools\Invoke-SerialDotnet.ps1' -DotnetArguments @('run', '--project', '$project', '--no-build', '--no-restore')"
git diff --check
```

On this Windows checkout, invoking the wrapper with `-DotnetArguments` avoids PowerShell binding
the dotnet `--project` or `/p:` switches as wrapper parameters. The repository-wide mutex and
serial execution are still provided by `Invoke-SerialDotnet.ps1`.

Record the exit code, compiler warnings/errors, verifier summary, and the affected `Build/bin` output. The core 10% gate is passed only when the focused import assertions pass and the pre-existing verifier assertions still pass. Full coverage of dynamic dispatch, reflection, event registration, lifecycle closure, owner/System inference, migration behavior equivalence, and all report partitions remains explicitly out of this first gate.

Actual verification for this implementation:

- The first TDD run failed at compilation because the new import API types were absent.
- After implementation, the affected verification project built with exit code `0`, `0` warnings,
  and `0` errors.
- The no-build verifier run completed with exit code `0` and printed `PASS: analysis evidence
  envelope, registry, readers, binding, execution DAG, relation multigraph, bounded API and
  isolated cleanup commit path`.
- Output was written under `Build/bin/AnalysisEvidenceCatalog/Debug/net10.0/` and
  `Build/bin/AnalysisEvidenceCatalog.Verification/Debug/net10.0/`.

### Task 7: Review and handoff

Review the diff for accidental changes outside the evidence catalog, verification project, and plan. Confirm that the public API does not advertise unsupported closure or migration-success semantics. Update the plan with the actual verification command and residual gaps, then commit only the scoped changes if the user requests a commit.

## Rollback

The change is isolated to the evidence catalog import namespace and its focused verifier. To roll it back, remove the import API files and the focused verifier call while retaining the existing explicit reader, graph, and catalog APIs. Do not remove or rewrite published catalog artifacts, source reports, or unrelated working-tree changes.

## Acceptance criteria

- The execution plan exists at the prescribed path and describes exact files, APIs, failure behavior, and verification commands.
- An explicit catalog index can be imported without manual stage registration.
- Reader output is registered before relation materialization, and relation queries observe the resulting edges.
- Duplicate edges are merged with accumulated evidence and weakest-status propagation.
- Partial/legacy evidence remains partial and gaps remain queryable.
- Execution dependencies/barriers are attached to `ExecutionDag`; semantic relation cycles do not become scheduling cycles.
- Invalid index/path/hash/context input is rejected before catalog mutation.
- The affected project builds and the focused core verifier passes through the serialized wrapper.
