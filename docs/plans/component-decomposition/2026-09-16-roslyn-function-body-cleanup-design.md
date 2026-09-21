# Roslyn Function Body Cleanup Design

**Goal:** Add a local Roslyn-based tool to the `ecs-system-domain-splitting` skill that keeps an untouched copy of the full `D:\TRbackup\Version4` project, creates a cleaned copy, and removes C# function bodies in parallel while preserving signatures and property-like members.

**Status:** Approved by continuation of the active goal on 2026-09-16.

## Scope / Evidence

- Source project: `D:\TRbackup\Version4`.
- Current source shape: `TerrariaServer.csproj`, `TerrariaServer.sln`, approximately 993 C# files, and generated `bin`, `obj`, and `.vs` directories.
- The tool processes `.cs` files under the project root. It excludes `.vs`, `bin`, `obj`, and `.git` from both snapshots and the cleanup input because they are IDE or generated output rather than project source.
- The source project is an old `net40` WindowsDesktop project. The cleaned copy is a syntax snapshot and is not required to compile: an empty body is intentionally retained even when the original return type requires a return statement.
- The tool itself targets the repository-selected `net10.0` SDK and Roslyn `Microsoft.CodeAnalysis.CSharp` 4.14.0.

## Ownership / Boundary Decision

The capability is a standalone tool under the skill's `tools/FunctionBodyCleanup` boundary. Its responsibilities are separated as follows:

1. `ProjectSnapshotCopier` owns source discovery, excluded-directory handling, and creation of an untouched `original/Version4` snapshot.
2. `CSharpFunctionBodyRewriter` owns only syntax-level body replacement. It has no file or process I/O.
3. `ProjectCleanupRunner` owns parallel file reads, Roslyn parsing, atomic writes to `cleaned/Version4`, diagnostics, and the run manifest.
4. The CLI and PowerShell launcher own argument parsing and explicit local execution. They do not implement rewrite rules.
5. The verification executable owns fixture assertions and source-preservation checks.

No semantic `MSBuildWorkspace` load is required. The requested transformation is declaration-shape based, and project references from the old `net40` tree would add restore and dependency failure modes without improving the body selection rule.

## Syntax Transformation Contract

The rewriter clears bodies for all body-bearing named function declarations, without filtering on modifiers:

- `MethodDeclarationSyntax`
- `ConstructorDeclarationSyntax`
- `DestructorDeclarationSyntax`
- `OperatorDeclarationSyntax`
- `ConversionOperatorDeclarationSyntax`
- `LocalFunctionStatementSyntax`

It also clears anonymous function bodies in field and other non-property expressions, because the request covers all function bodies. Expression-bodied functions become an empty block and lose only the expression-body semicolon required by the original syntax form.

The rewriter deliberately leaves these subtrees unchanged:

- `PropertyDeclarationSyntax`
- `IndexerDeclarationSyntax`
- `EventDeclarationSyntax`
- Their accessor declarations and accessor blocks
- Expression-bodied properties and indexers

Thus `virtual`, `override`, `abstract`, `async`, `partial`, and similar modifiers do not create protection rules. Declarations that have no body, such as abstract or extern signatures, remain unchanged because there is no body to remove. Constructor initializers and all signature tokens, attributes, type parameters, constraints, modifiers, parameter lists, and return types remain intact.

The rewriter preserves source encoding and surrounding trivia where possible, does not format the entire file, and skips a file with Roslyn parse errors rather than writing a partially understood transformation.

## Schedule / Data Flow

Each invocation creates a unique run directory below:

```text
.agents/skills/ecs-system-domain-splitting/managed/function-body-cleanup/<run-id>/
  original/Version4/     # immutable source snapshot
  cleaned/Version4/      # rewritten project copy
  manifest.json           # counts, diagnostics, options, and paths
```

The sequence is:

1. Validate the source root and management root are distinct writable directories.
2. Copy the source project into `original/Version4`, excluding generated directories.
3. Copy the untouched snapshot into `cleaned/Version4`.
4. Enumerate cleaned `.cs` files in deterministic relative-path order.
5. Run independent Roslyn parse/rewrite/write jobs with `Parallel.ForEachAsync` and a bounded configurable degree of parallelism.
6. Write changed files through a same-directory temporary file and replace operation.
7. Emit a manifest containing source/snapshot paths, exclusions, Roslyn configuration, processed/changed/skipped counts, function/property counts, and file diagnostics.
8. Return a nonzero exit code when copying, parsing, or writing has an error; the original source and untouched snapshot remain available for rollback.

Copying is intentionally completed before any cleanup job starts. Parallelism is limited to independent files; no shared mutable Roslyn rewriter or shared syntax tree is used.

## Migration / Rollback

The existing skill source remains behaviorally compatible except for the new documented execution capability. The source skill files to update are `SKILL.md`, `README.md`, `manifest.json`, and the permission policy metadata. The tool source, launcher, and verification project live under the skill's `tools/` directory. Generated run data stays under `managed/` and is not part of the skill source package.

Rollback is deleting or ignoring the run directory; no source file under `D:\TRbackup\Version4` is written. A failed run is not silently resumed or cleaned up. A later run receives a new run ID. The manifest is the handoff record for the exact original snapshot and cleaned output.

## Verification

The focused verification must prove:

- block-bodied and expression-bodied methods are emptied;
- virtual and override methods are emptied rather than protected;
- constructors, destructors, operators, conversion operators, local functions, and anonymous functions are covered;
- abstract/extern signature-only declarations remain validly untouched;
- properties, indexers, events, accessors, and expression-bodied properties remain unchanged;
- attributes, modifiers, parameters, generic constraints, constructor initializers, and signatures are retained;
- source encoding and non-C# files are preserved;
- a runner creates both an untouched snapshot and a cleaned copy before rewriting;
- the original fixture/project remains byte-for-byte unchanged;
- parse failures are reported and do not overwrite the affected cleaned file;
- parallel execution produces deterministic file-level results and a complete manifest.

After the tool and focused verifier pass, run the tool against the full `D:\TRbackup\Version4` project. Verify the reported file and transformation counts, inspect representative cleaned files and preserved properties, verify the original tree has not changed, and record the exact output paths. Do not compile the cleaned Version4 snapshot; compile and verify only the `net10.0` tool and its focused verifier through `Build/Tools/Invoke-SerialDotnet.ps1` under the repository build-concurrency contract.

## Open Evidence

- Exact Version4 transformation counts are unknown until the first real run and must come from `manifest.json`.
- Whether every source file parses under the selected Roslyn language version is unknown until the real run; skipped files must remain explicit rather than being presented as cleaned.
