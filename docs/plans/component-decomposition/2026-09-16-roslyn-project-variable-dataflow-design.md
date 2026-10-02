# Roslyn Project Variable Dataflow Design

**Goal:** Add a project-level Roslyn tool for the `ecs-system-domain-splitting` skill that reports the variables read and written by every callable body in `D:\TRbackup\Version4`, with deterministic persisted evidence.

**Status:** Approved by the user on 2026-09-16.

## Scope and evidence

- Source project: `D:\TRbackup\Version4\TerrariaServer.csproj`.
- The source tree is one old Windows Desktop project targeting `net40`, with about 993 C# files and local DLL references.
- The tool runs on the repository-selected `net10.0` SDK and uses Roslyn `4.14.0`.
- The existing `scripts/CalledFunctionsAnalyzer` resolves call sites only. It does not expose Roslyn `DataFlowAnalysis`, so this capability has its own executable and report schema.
- The analyzer reads the project and its source/reference inputs. It writes only the skill-owned report path and never modifies `Version4`.

## Ownership and boundary decision

The new capability lives under:

```text
.agents/skills/ecs-system/scripts/ProjectDataflowAnalyzer/
```

Responsibilities are separated as follows:

1. `ProjectDataflowAnalyzer` owns MSBuild project loading, compilation setup, callable indexing, and Roslyn data-flow extraction.
2. `AnalysisOptions` owns CLI validation and explicit configuration values.
3. `AnalysisModels` owns the serializable report contract and diagnostics.
4. `ReportWriter` owns deterministic JSON serialization and atomic replacement of the output file.
5. The PowerShell launcher owns the skill-relative default output path and invokes the already-built DLL. It does not build, mutate the source project, or implement analysis rules.
6. The fixture verifier owns behavioral assertions and is independent of the full Version4 output.

The capability is read-only with respect to the analyzed project. Its only side effect is an explicit report write under the current skill directory.

## Project loading and schedule

The CLI accepts a project directory or `.csproj`; a directory must contain exactly one top-level project file. The Version4 launcher supplies `D:\TRbackup\Version4\TerrariaServer.csproj`.

The analyzer uses `MSBuildLocator.RegisterDefaults()` followed by `MSBuildWorkspace.OpenProjectAsync`. This preserves the project target framework, local references, SDK defaults, and project-defined parse options. The loaded `CSharpCompilation` is recreated with `CSharpCompilationOptions.WithConcurrentBuild(true)`.

Analysis has four ordered phases:

1. Load the project and collect workspace diagnostics.
2. Obtain the project compilation and index callable regions from all syntax trees.
3. Analyze independent callable regions with bounded `Parallel.ForEach`. Each worker reads immutable Roslyn state and writes to its own result slot; no mutable report collection is shared between workers.
4. Sort results by normalized path, source position, callable kind, and stable ID, then atomically write one JSON report.

The parallel degree is configurable from 1 through 64. The report records the configured value and whether Roslyn concurrent compilation was enabled. A failed callable does not abort unrelated callables; it produces a diagnostic and an explicit `failed` status.

## Data-flow contract

The first release analyzes:

- methods;
- constructors, destructors, operators, and conversion operators;
- local functions;
- property, indexer, and event accessors;
- expression-bodied properties and indexers as implicit getter regions;
- lambda expressions and anonymous methods.

For each region, the tool calls the C# semantic model's `AnalyzeDataFlow` overload appropriate to the syntax node:

- block or statement regions use the statement/body overload;
- expression bodies use the expression overload;
- constructor initializers use the constructor-initializer overload and are merged with the constructor body result.

Each callable record contains:

- stable ID, callable kind, symbol display name, file, line, and column;
- body status: `succeeded`, `no-body`, or `failed`;
- `ReadInside` as `readVariables`;
- `WrittenInside` as `writtenVariables`;
- `DataFlowsIn`, `DataFlowsOut`, `VariablesDeclared`, and `Captured` for ownership and migration review;
- structured symbol details including symbol kind, name, containing symbol, source locations, and fully qualified display name;
- per-region diagnostics.

`writtenVariables` preserves Roslyn's exact `WrittenInside` semantics. Because declarations can also be writes, `variablesDeclared` remains alongside it instead of silently removing declarations from the write set.

## Persistence and report schema

The launcher default is:

```text
D:\TRbackup\NLTX\.agents\skills\ecs-system\reports\data-flow-analysis\Version4.json
```

The report includes schema/tool/Roslyn versions, normalized project path, project name, configuration, platform, target framework, project file hash, source-tree counts, parallel settings, workspace and compilation diagnostics, callable records, and a completeness summary. Arrays and diagnostics are sorted deterministically. The writer creates a same-directory temporary file and atomically replaces the requested report only after serialization succeeds.

The report status is:

- `complete` when the project loaded, compilation exists, and all callable regions were analyzed without errors;
- `partial` when compilation/workspace diagnostics or callable failures remain but usable records were produced;
- `failed` when project loading or compilation creation cannot produce an analyzable project.

The report is generated evidence, not source package content. It is kept below the skill directory for local management and excluded from the distributable skill metadata.

## Error handling and rollback

- Invalid CLI input returns exit code `64`.
- A fatal project load or compilation failure returns exit code `1`.
- A report with workspace, compilation, parse, or callable diagnostics is written and returns exit code `2`.
- Cancellation propagates through the analysis loop and does not get reported as successful completion.
- No cleanup command removes source or prior runs. Re-running replaces only the explicitly named report atomically.
- If the launcher or analyzer fails, `Version4` remains byte-for-byte untouched; the previous report remains intact when a new temporary report cannot be committed.

## Verification

The focused fixture project will prove:

- parameter and local-variable reads;
- local declarations and assignments in `writtenVariables`;
- field and property reads/writes;
- branches, loops, expression-bodied methods, constructors, and constructor initializers;
- accessors, local functions, lambdas, and anonymous methods;
- `DataFlowsIn`, `DataFlowsOut`, `VariablesDeclared`, and `Captured` are serialized;
- overloaded callable IDs are deterministic;
- parse/project diagnostics are explicit and do not fabricate a successful data-flow set;
- two runs produce equivalent sorted JSON apart from timestamps, if timestamps are present.

The real verification then runs the tool against `D:\TRbackup\Version4\TerrariaServer.csproj`, checks the report path and counts, inspects representative records from `Player.cs`, `Projectile.cs`, and `WorldGen.cs`, and confirms the source tree has no hash changes. The tool and verifier are compiled serially through `Build/Tools/Invoke-SerialDotnet.ps1`; no cleaned or altered Version4 source is compiled.

## Open evidence

- Exact callable and diagnostic counts are unknown until the first full project run.
- Whether every legacy source file is accepted by the selected MSBuild/Roslyn installation is unknown until project loading and the full run complete; any gaps remain in the report instead of being hidden.
