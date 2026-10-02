# Roslyn Project Variable Dataflow Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Build and run a `net10.0` Roslyn tool that loads the complete `D:\TRbackup\Version4` project, reports every callable body's variable reads and writes through `AnalyzeDataFlow`, and persists deterministic JSON under the `ecs-system-domain-splitting` skill.

**Architecture:** A standalone console project under the skill's `scripts/ProjectDataflowAnalyzer` directory uses `MSBuildLocator` and `MSBuildWorkspace` to preserve the Version4 `net40` project context. It enables Roslyn concurrent compilation, indexes immutable callable work items, analyzes them with bounded `Parallel.ForEach`, and writes a stable report atomically. A PowerShell fixture verifier runs the built DLL and checks the public JSON contract without compiling the full legacy project.

**Tech Stack:** .NET SDK 10.0.400, C# preview, `Microsoft.CodeAnalysis.CSharp` 4.14.0, `Microsoft.CodeAnalysis.Workspaces.MSBuild` 4.14.0, `Microsoft.Build.Locator` 1.11.2, PowerShell JSON assertions.

---

### Task 1: Write the failing project-level fixture test

**Files:**

- Create: `.agents/skills/ecs-system/scripts/tests/fixtures/ProjectDataflowFixture/ProjectDataflowFixture.csproj`
- Create: `.agents/skills/ecs-system/scripts/tests/fixtures/ProjectDataflowFixture/Sample.cs`
- Create: `.agents/skills/ecs-system/scripts/tests/fixtures/ProjectDataflowFixture/Support.cs`
- Create: `.agents/skills/ecs-system/scripts/tests/Test-ProjectDataflowAnalyzer.ps1`

**Step 1: Create a fixture project with representative callable regions**

Use a small `net10.0` SDK project with no external dependencies. Include a method that reads a parameter and field, declares and writes a local, mutates a field in branches and loops, an expression-bodied method, a constructor initializer, property/indexer/event accessors, a local function, a lambda, an anonymous method, and an abstract/extern signature without a body. Keep the fixture source stable so the output can be compared across runs.

**Step 2: Write the verifier before the analyzer project exists**

The PowerShell verifier must locate:

```text
Build/bin/ProjectDataflowAnalyzer/Debug/net10.0/ProjectDataflowAnalyzer.dll
```

Invoke that DLL with `--project <fixture.csproj>`, `--output <temporary-json>`, and `--max-degree-of-parallelism 2`. Assert the report contains the project name, successful callable records, `readVariables`, `writtenVariables`, `dataFlowsIn`, `dataFlowsOut`, `variablesDeclared`, and `captured`. Assert representative symbols such as `input`, `_field`, `local`, and `Property` occur in the correct sets. Run the same command twice and compare the canonical JSON after removing no fields; the report must be deterministic.

**Step 3: Run the verifier and confirm the intended red state**

Run from `D:\TRbackup\NLTX`:

```powershell
pwsh -NoProfile -File .\.agents\skills\ecs-system\scripts\tests\Test-ProjectDataflowAnalyzer.ps1
```

Expected: failure because `ProjectDataflowAnalyzer.dll` does not exist. Do not create a production project or implementation source before recording this failure.

### Task 2: Create the analyzer project and CLI options

**Files:**

- Create: `.agents/skills/ecs-system/scripts/ProjectDataflowAnalyzer/ProjectDataflowAnalyzer.csproj`
- Create: `.agents/skills/ecs-system/scripts/ProjectDataflowAnalyzer/Program.cs`
- Create: `.agents/skills/ecs-system/scripts/ProjectDataflowAnalyzer/AnalysisOptions.cs`

**Step 1: Add the net10.0 package contract**

Target `net10.0`, enable nullable and implicit usings, and reference exactly the tested Roslyn/MSBuild packages at the versions in this plan. Keep output governed by the repository `Directory.Build.props`, so build artifacts go under `Build/bin/ProjectDataflowAnalyzer`.

**Step 2: Implement explicit argument parsing**

Support `--project`, `--output`, `--configuration` (default `Debug`), `--platform` (default `Any CPU`), `--max-degree-of-parallelism` (1 through 64), and `--help`. Accept a directory only when it contains one top-level `.csproj`; normalize all paths and reject missing inputs before registering MSBuild. Return `64` for usage errors.

**Step 3: Add the top-level execution boundary**

Register `MSBuildLocator` once, create `MSBuildWorkspace` with the requested configuration/platform, subscribe to `WorkspaceFailed`, run the analyzer, write the report, and map fatal versus partial outcomes to exit codes `1` and `2`. Propagate cancellation and do not catch it as a successful result.

### Task 3: Implement project loading and callable indexing

**Files:**

- Create: `.agents/skills/ecs-system/scripts/ProjectDataflowAnalyzer/ProjectDataflowAnalyzer.cs`
- Create: `.agents/skills/ecs-system/scripts/ProjectDataflowAnalyzer/AnalysisModels.cs`

**Step 1: Load the real project compilation**

Open the supplied `.csproj` with `MSBuildWorkspace.OpenProjectAsync`, call `GetCompilationAsync`, require a `CSharpCompilation`, and recreate it with `WithConcurrentBuild(true)`. Preserve the project parse options and references. Record workspace diagnostics and compilation diagnostics without discarding usable syntax trees.

**Step 2: Define callable region discovery**

Index method, constructor, destructor, operator, conversion operator, local-function, accessor, expression-bodied property/indexer, lambda, and anonymous-method regions. Keep signature-only declarations as `no-body` records. For each target retain its syntax node, body/region node, constructor initializer if present, semantic model, normalized path, source position, kind, and stable ID.

**Step 3: Make indexing deterministic and thread-safe**

Use one result slot per syntax tree or target index during parallel indexing. Do not append to shared mutable lists from workers. Build the final target list only after all workers finish, then sort by normalized path, line, column, kind, and ID. Keep nested callable records distinct and use synthetic location-based IDs when Roslyn has no declared symbol.

### Task 4: Implement Roslyn data-flow extraction

**Files:**

- Modify: `.agents/skills/ecs-system/scripts/ProjectDataflowAnalyzer/ProjectDataflowAnalyzer.cs`
- Modify: `.agents/skills/ecs-system/scripts/ProjectDataflowAnalyzer/AnalysisModels.cs`

**Step 1: Analyze each callable region in bounded parallel**

Use `Parallel.ForEach` with `ParallelOptions.MaxDegreeOfParallelism`. For block/statement bodies call the corresponding C# `SemanticModel.AnalyzeDataFlow` overload; for expression bodies call the expression overload; for constructor initializers call the constructor-initializer overload and merge the region sets. Use immutable Roslyn state and per-target result slots.

**Step 2: Serialize exact read/write sets**

Map `ReadInside` to `readVariables` and `WrittenInside` to `writtenVariables`. Also serialize `DataFlowsIn`, `DataFlowsOut`, `VariablesDeclared`, and `Captured`. Preserve declared variables in `writtenVariables` because Roslyn treats declarations as writes; do not silently reinterpret the API result.

**Step 3: Add structured symbol and failure records**

For each symbol record its kind, name, containing symbol, fully qualified display name, and source locations. A failed analysis must contain `status: failed`, an error diagnostic, and empty/explicit sets; a signature-only declaration must contain `status: no-body`. Compilation warnings and errors remain visible in the report and affect completeness status.

### Task 5: Add deterministic atomic JSON output

**Files:**

- Create: `.agents/skills/ecs-system/scripts/ProjectDataflowAnalyzer/ReportWriter.cs`
- Modify: `.agents/skills/ecs-system/scripts/ProjectDataflowAnalyzer/Program.cs`

**Step 1: Implement the versioned report schema**

Emit schema version `1.0` with tool/Roslyn versions, project path/name, configuration, platform, target framework, project hash, source and callable counts, parallel settings, workspace/compilation diagnostics, callable records, and `complete`/`partial`/`failed` status. Sort every list before serialization. Avoid wall-clock fields so equivalent runs compare byte-for-byte.

**Step 2: Write reports only at the explicit output boundary**

Create the output directory, serialize UTF-8 JSON to a same-directory temporary file, flush and atomically replace the target. On serialization or replacement failure, remove only the uniquely named temporary file and leave an existing report untouched. Never write to the analyzed project.

**Step 3: Run the fixture verifier for the first green state**

After creating the production implementation, restore/build the analyzer serially, verify the expected DLL under `Build/bin`, and rerun:

```powershell
pwsh -NoProfile -File .\.agents\skills\ecs-system\scripts\tests\Test-ProjectDataflowAnalyzer.ps1
```

Expected: all fixture assertions pass, including exact deterministic output and diagnostics for no-body/invalid cases.

### Task 6: Add the skill launcher and documentation

**Files:**

- Create: `.agents/skills/ecs-system/scripts/Run-ProjectDataflowAnalyzer.ps1`
- Modify: `.agents/skills/ecs-system/SKILL.md`
- Modify: `.agents/skills/ecs-system/README.md`
- Modify: `.agents/skills/ecs-system/manifest.json`
- Modify: `.agents/skills/ecs-system/security/permission_policy.json`
- Modify: `.agents/skills/ecs-system/security/permission_policy.md`

**Step 1: Implement a no-build launcher**

Resolve the skill root from `$PSScriptRoot`, default the project to `D:\TRbackup\Version4\TerrariaServer.csproj`, default output to `reports/data-flow-analysis/Version4.json`, locate the already-built DLL under `Build/bin`, and invoke it. The launcher must fail clearly when the artifact is missing and must not invoke `dotnet build` or modify Version4.

**Step 2: Document the reusable execution asset**

Add the launcher command, report location, `AnalyzeDataFlow` semantics, parallelism option, completeness states, and the distinction between source-project reads and skill-report writes. Keep detailed flags discoverable through `--help`.

**Step 3: Align capability metadata**

Remove the stale “no executable scripts” claim. Declare only local subprocess execution and file-write scope; keep network and interactive capabilities disabled. Keep generated reports outside the package source manifest.

### Task 7: Verify the full Version4 project

**Files:**

- Generated, local-only: `.agents/skills/ecs-system/reports/data-flow-analysis/Version4.json`

**Step 1: Check the repository build lock before compilation**

Run the exact `dotnet.exe`/`csc.exe` process discovery from `AGENTS.md`. If an active or unclear owner exists, wait and re-check; never terminate it. Do not run compile-capable commands in parallel.

**Step 2: Restore and build the affected analyzer project**

From `D:\TRbackup\NLTX`, use the wrapper for the analyzer project only:

```powershell
pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 restore .\.agents\skills\ecs-system\scripts\ProjectDataflowAnalyzer\ProjectDataflowAnalyzer.csproj -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false
pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 build .\.agents\skills\ecs-system\scripts\ProjectDataflowAnalyzer\ProjectDataflowAnalyzer.csproj --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false
```

Record each command, exit code, warning/error counts, and verify `Build/bin/ProjectDataflowAnalyzer/Debug/net10.0/ProjectDataflowAnalyzer.dll` exists.

**Step 3: Run focused verification without rebuilding**

Run the fixture verifier after the build using its already-built DLL. Record its exit code and assert that no source or unrelated worktree path changed.

**Step 4: Run the launcher against Version4**

Execute:

```powershell
pwsh -NoProfile -File .\.agents\skills\ecs-system\scripts\Run-ProjectDataflowAnalyzer.ps1
```

Read the generated JSON, record document/callable/success/failure/diagnostic counts, and inspect representative records from `Terraria/Player.cs`, `Terraria/Projectile.cs`, and `Terraria/WorldGen.cs`. Do not compile the Version4 project as part of analyzer verification.

**Step 5: Prove source immutability and stable output**

Hash the Version4 source files before and after the run, compare hashes, and rerun with the same options to confirm stable sorted function and symbol records. Treat any parse or workspace gap as explicit `partial` evidence, not as a successful complete analysis.

### Task 8: Review the scoped result and complete the audit

**Files:**

- Review only: `.agents/skills/ecs-system/scripts/ProjectDataflowAnalyzer/**`, launcher, docs, metadata, and generated report.

**Step 1: Run static and metadata checks**

Use `git diff --check`, JSON parsing, and path-scoped status/diff checks. Confirm pre-existing changes and the other ignored tools remain untouched. Verify no `Build/bin`, `Build/obj`, or generated report is staged.

**Step 2: Check the explicit requirements**

Match fresh evidence to every requirement: whole Version4 project input, Roslyn concurrent compilation, bounded parallel analysis, `AnalyzeDataFlow`, read/write sets, deterministic persistence under the skill, diagnostics, source immutability, focused tests, full-run report, and documented usage.

**Step 3: Report only verified claims**

Include exact build/test/run commands, exit codes, warning/error counts, artifact/report paths, and any unresolved `partial` evidence. Do not claim completion from compilation alone.
