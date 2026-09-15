# Roslyn Function Body Cleanup Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Build and run a Roslyn-based tool that snapshots the complete `D:\TRbackup\Version4` project, preserves the snapshot, and produces a parallel-cleaned copy with function bodies removed while property-like members remain unchanged.

**Architecture:** A `net10.0` class library owns deterministic snapshotting, syntax rewriting, parallel file processing, atomic writes, and manifest records. A thin CLI and PowerShell launcher provide safe execution defaults for the skill. A standalone verification executable uses temporary fixtures and the real library so behavior can be tested without compiling the intentionally non-compilable cleaned Version4 snapshot.

**Tech Stack:** .NET SDK 10.0.400, C# preview, Roslyn `Microsoft.CodeAnalysis.CSharp` 4.14.0, PowerShell, executable verification programs.

---

### Task 1: Create the tool and verification project skeleton

**Files:**
- Create: `.agents/skills/ecs-system-domain-splitting/tools/FunctionBodyCleanup/FunctionBodyCleanup.csproj`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/FunctionBodyCleanup.Cli/FunctionBodyCleanup.Cli.csproj`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/FunctionBodyCleanup.Verification/FunctionBodyCleanup.Verification.csproj`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/FunctionBodyCleanup.Verification/Program.cs`

**Step 1: Write the failing verification fixture and API calls**

Create a console verifier that references the empty library project and calls the intended public runner API with a temporary fixture. Include assertions for body removal, property preservation, virtual-method handling, source preservation, and the two output snapshots.

**Step 2: Run the verifier build to verify it fails for the expected reason**

Before any production implementation, inspect active `dotnet.exe`/`csc.exe` processes, then run from `D:\TRbackup\NLTX`:

```powershell
pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 build .\.agents\skills\ecs-system-domain-splitting\tools\FunctionBodyCleanup.Verification\FunctionBodyCleanup.Verification.csproj -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false
```

Expected: compilation fails because the production runner and rewrite contract do not exist yet. This is the TDD red state, not a package or restore failure.

**Step 3: Commit the test-first skeleton**

```powershell
git add -- .agents/skills/ecs-system-domain-splitting/tools docs/plans/2026-09-16-roslyn-function-body-cleanup-implementation.md
git commit -m "test: define function body cleanup contract"
```

### Task 2: Implement the pure Roslyn syntax rewriter

**Files:**
- Create: `.agents/skills/ecs-system-domain-splitting/tools/FunctionBodyCleanup/CSharpFunctionBodyRewriter.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/FunctionBodyCleanup/RewriteStatistics.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/FunctionBodyCleanup/FunctionBodyCleanupOptions.cs`

**Step 1: Implement named declaration rewriting**

Use `CSharpSyntaxRewriter` and rewrite body-bearing `MethodDeclarationSyntax`, `ConstructorDeclarationSyntax`, `DestructorDeclarationSyntax`, `OperatorDeclarationSyntax`, `ConversionOperatorDeclarationSyntax`, and `LocalFunctionStatementSyntax`. Replace block and expression bodies with empty blocks while keeping all declaration tokens, modifiers, attributes, constraints, parameters, return types, and constructor initializers.

**Step 2: Implement anonymous-function rewriting and protection boundaries**

Rewrite anonymous methods and expression-bodied lambdas outside property-like subtrees. Override property, indexer, event, and accessor visits to return the original subtree unchanged. Do not inspect modifiers for `virtual`, `override`, `async`, `partial`, or other special-function protection.

**Step 3: Run the focused verifier to verify the green state**

Build and run the verifier serially through `Invoke-SerialDotnet.ps1`; confirm all fixture assertions pass and record exit code, warning/error counts, and the `Build/bin` artifact path.

### Task 3: Implement encoding-safe file transformation and snapshot ownership

**Files:**
- Create: `.agents/skills/ecs-system-domain-splitting/tools/FunctionBodyCleanup/ProjectSnapshotCopier.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/FunctionBodyCleanup/SourceFileCleanup.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/FunctionBodyCleanup/ProjectCleanupRunner.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/FunctionBodyCleanup/CleanupRunResult.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/FunctionBodyCleanup/FileCleanupResult.cs`
- Modify: `.agents/skills/ecs-system-domain-splitting/tools/FunctionBodyCleanup.Verification/Program.cs`

**Step 1: Add the immutable original snapshot**

Validate that source and management paths are distinct, create a unique run directory, copy the source tree into `original/Version4`, and exclude `.vs`, `bin`, `obj`, and `.git`. Do not follow reparse-point directories. Clone the original snapshot into `cleaned/Version4` before starting any rewrite job.

**Step 2: Add Roslyn parsing and bounded parallel cleanup**

Enumerate cleaned `.cs` files in deterministic relative-path order. Use `Parallel.ForEachAsync` with configurable degree of parallelism; each job independently reads, detects common BOM encodings, parses with `CSharpParseOptions`, rewrites, and writes only changed files. Skip files with parse errors and record diagnostics. Use same-directory temporary files and replace operations for atomic writes.

**Step 3: Add manifest and source-preservation assertions**

Emit `manifest.json` with source/snapshot/cleaned paths, exclusions, Roslyn version and language settings, parallelism, per-file status, counts, and diagnostics. Extend the verifier to assert the source fixture and original snapshot remain byte-for-byte unchanged, while the cleaned fixture contains the expected changes.

**Step 4: Run the focused verifier again**

Run the verifier with `--no-build --no-restore` after its serial build. Expected: all snapshot, encoding, parse-error, parallel-processing, and manifest assertions pass.

### Task 4: Add CLI and skill launcher

**Files:**
- Create: `.agents/skills/ecs-system-domain-splitting/tools/FunctionBodyCleanup.Cli/Program.cs`
- Create: `.agents/skills/ecs-system-domain-splitting/tools/Run-FunctionBodyCleanup.ps1`

**Step 1: Implement explicit CLI options**

Support `--source-root` (default `D:\TRbackup\Version4`), required or explicit `--management-root`, `--max-degree-of-parallelism`, optional deterministic `--run-id`, and `--help`. Return distinct nonzero codes for argument, copy, parse, and write failures.

**Step 2: Implement the PowerShell launcher**

Resolve the current skill root from `$PSScriptRoot`, point management output at `managed/function-body-cleanup`, locate the already-built CLI under `Build/bin`, and execute the DLL. The launcher must not silently build or write to the source tree.

**Step 3: Verify CLI help and fixture execution**

Build the CLI through `Invoke-SerialDotnet.ps1`, run help, and execute the CLI against a temporary fixture with an explicit management root. Verify the manifest and exit codes.

### Task 5: Update skill contract and package metadata

**Files:**
- Modify: `.agents/skills/ecs-system-domain-splitting/SKILL.md`
- Modify: `.agents/skills/ecs-system-domain-splitting/README.md`
- Modify: `.agents/skills/ecs-system-domain-splitting/manifest.json`
- Modify: `.agents/skills/ecs-system-domain-splitting/security/permission_policy.json`
- Modify: `.agents/skills/ecs-system-domain-splitting/security/permission_policy.md`
- Modify: `.agents/skills/ecs-system-domain-splitting/registry/packages/ecs-system-domain-splitting.json`

**Step 1: Document the execution asset**

Document the fixed Version4 target, snapshot layout, excluded generated directories, function/property boundaries, non-compilation caveat, launcher command, and manifest handoff.

**Step 2: Declare capabilities and lifecycle changes**

Declare local file-write and subprocess capabilities with explicit scope, keep network and interactive capabilities disabled, add `tools` to the manifest factory components, and bump package metadata consistently.

**Step 3: Run metadata consistency checks**

Verify JSON parsing, version/name agreement, and that no managed run data is included in source package metadata. Keep generated run directories out of the distributable package.

### Task 6: Build the tool and run the full Version4 cleanup

**Files:**
- Generated but intentionally untracked: `.agents/skills/ecs-system-domain-splitting/managed/function-body-cleanup/<run-id>/`

**Step 1: Inspect the shared build lock and active compiler processes**

Use the exact process discovery from `AGENTS.md`. If any `dotnet.exe` or `csc.exe` build owner is active or unclear, wait and re-check; never terminate it.

**Step 2: Restore and build only the affected CLI project**

Run from the repository root through `Build/Tools/Invoke-SerialDotnet.ps1` with `-m:1`, `-nr:false`, `-p:UseSharedCompilation=false`, `-p:MSBuildNodeReuse=false`, and `-p:BuildInParallel=false`. Record exit code, warning/error counts, and verify the CLI DLL exists under `Build/bin`.

**Step 3: Run the focused verifier without rebuilding**

Use the serial wrapper with `test` or the executable verifier's `run` path and `--no-build --no-restore`, depending on the project shape. Record the exact command and result.

**Step 4: Run the launcher against `D:\TRbackup\Version4`**

Execute the built launcher. It must create the untouched original and cleaned copy under the current skill's `managed` directory before rewriting. Do not compile the cleaned `net40` snapshot.

**Step 5: Inspect real-run evidence**

Read `manifest.json`, verify the expected file count and diagnostics, inspect representative virtual methods, expression-bodied methods, constructors, operators, properties, indexers, and accessors, and compare source hashes or file timestamps to confirm the original project was not modified.

### Task 7: Package and final verification

**Files:**
- Modify generated distribution metadata only if the repository's existing package tooling supports a reproducible rebuild: `.agents/skills/ecs-system-domain-splitting/dist/**`, `reports/package_verification.*`, and registry checksums.

**Step 1: Run the existing package verifier or equivalent local checks**

Confirm safe archive paths, the new tool source is included when packaging is intended, managed output is excluded, and permission metadata matches the executable behavior.

**Step 2: Review the scoped diff**

Use `git diff --check`, `git status --short`, and a path-scoped diff. Confirm unrelated pre-existing changes remain untouched and no generated build output or Version4 snapshot is staged.

**Step 3: Complete the requirement audit**

Match every objective requirement to fresh evidence: Roslyn API usage, parallel cleanup, no special-function protection, property preservation, backup-before-cleanup, Version4 output, build/test results, manifest, and source immutability. Only then report completion.
