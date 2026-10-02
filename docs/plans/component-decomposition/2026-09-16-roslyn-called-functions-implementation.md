# Roslyn Called-Functions Analyzer Implementation Plan

> **For Claude:** Execute this plan task-by-task with test-first verification.

**Goal:** Add a Roslyn-based, parallel C# call-site analyzer to the ECS domain
splitting skill for the complete Version4 source tree.

**Architecture:** A .NET 10 console project under the skill's `scripts/`
directory discovers project sources and references, parses files in parallel,
creates one concurrent Roslyn compilation, and resolves calls from an explicitly
selected method. A small PowerShell fixture test exercises the CLI through its
compiled DLL and keeps output assertions independent of the production code.

**Tech Stack:** .NET 10, C# preview, Microsoft.CodeAnalysis.CSharp 4.14.0,
`Parallel.ForEachAsync`, `Parallel.ForEach`, PowerShell JSON assertions.

---

### Task 1: Add the failing fixture test

**Files:**

- Create: `.agents/skills/ecs-system/scripts/tests/fixtures/CallGraphFixture/Target.cs`
- Create: `.agents/skills/ecs-system/scripts/tests/fixtures/CallGraphFixture/Dependencies.cs`
- Create: `.agents/skills/ecs-system/scripts/tests/Test-CalledFunctions.ps1`

Write assertions for cross-file calls, constructors, direct versus transitive
results, parallel statistics, and ambiguous/missing target diagnostics. Run the
test before the analyzer project exists and confirm it fails because the tool
artifact is unavailable.

### Task 2: Implement the analyzer project and CLI

**Files:**

- Create: `.agents/skills/ecs-system/scripts/CalledFunctionsAnalyzer/CalledFunctionsAnalyzer.csproj`
- Create: `.agents/skills/ecs-system/scripts/CalledFunctionsAnalyzer/Program.cs`

Implement project discovery, excluded-directory filtering, source parsing with
`Parallel.ForEachAsync`, concurrent Roslyn compilation, target selection, call
resolution, optional transitive traversal, stable JSON serialization, and CLI
help/error handling.

### Task 3: Run the focused verifier and refactor only after green

Restore/build the affected project serially through the repository wrapper, verify
the DLL under `Build/bin/`, run the fixture test, then run the analyzer against
`D:\TRbackup\Version4` with a known method. Record exit codes and diagnostic
counts without rewriting generated reports or unrelated source files.

### Task 4: Document the skill resource

**Files:**

- Modify: `.agents/skills/ecs-system/SKILL.md`
- Modify: `.agents/skills/ecs-system/README.md`

Add the tool's purpose, invocation shape, project-root example, output contract,
and limitations. Keep detailed flags discoverable through `--help`.
