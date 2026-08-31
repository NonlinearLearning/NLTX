# Repository Guidance

## Scope

This file applies to the entire repository. A deeper `AGENTS.md` may add narrower rules for its directory, but it must not weaken the repository build constraints.

## Google C# Style Guide Constraints

- `约束/Google-CSharp-Style-Guide-约束.md` is the mandatory project-local implementation of the Google C# Style Guide. Read it before adding or refactoring C# code.
- Architecture, safety, layering, and testing take priority when they conflict with style. For incremental changes, make the changed block compliant; do not create a broad unrelated formatting diff.
- Use `PascalCase` for types, methods, namespaces, public members, files, and directories; use `camelCase` for locals and parameters; use `_camelCase` for non-public fields and properties; name interfaces with an `I` prefix.
- Use `Npc`, `Ai`, `Id`, and `Uid` rather than all-caps abbreviations. Do not introduce generic `Manager`, `Helper`, `Utility`, `Utils`, `Misc`, or `Data` types that conceal the simulation responsibility.
- Prefer one core type per file and name the file after that type. Keep files and directories in `PascalCase`.
- Put alphabetized `using` directives before the namespace, with `System` namespaces first. Do not use aliases merely to shorten type names.
- Use 2-space indentation, no tabs, a maximum line width of 100 characters, braces for all control-flow blocks, and same-line opening braces. Keep one statement per line and avoid multiple assignments in one statement.
- Prefer `const`, then `readonly`, over magic numbers. Use the narrowest read-only collection interface that expresses an input contract.
- Keep class members grouped by kind and visibility as specified in the constraint document; keep related interface implementations adjacent.

## Build Environment

- Use the .NET SDK selected by `global.json` when a project is present.
- Target `net10.0` unless a project has an explicitly documented compatibility exception.
- Run every `dotnet` command that can compile or write shared build outputs from the repository
  root through `Build/Tools/Invoke-SerialDotnet.ps1` so `NuGet.config`, `Directory.Build.props`,
  and `Directory.Build.targets` are applied and the checkout-wide build lock is honored.
- Use `-p:UseSharedCompilation=false` for serial restore/build/test runs in this workspace to avoid
  compiler output contention.

## Dotnet Build Concurrency Contract

`BUILD-CONCURRENCY-1` is the repository-wide normative contract for every human, Codex,
subagent, and parallel session that shares this checkout. The full command shape lives only in
this section; the other Markdown files link here so the rule has one source of truth.

### Discovery and ownership

- Root `AGENTS.md` is the automatic agent-context entry point. Read this contract before any
  `dotnet` command.
- `progress.md` and `docs/flowstate/README.md` link back to this section. Do not copy a second
  command matrix into either file.
- Source edits may be parallel only when agents have disjoint write sets. Every `dotnet` command
  that can compile or write shared build outputs is one shared critical section for this checkout.
- `Build/Tools/Invoke-SerialDotnet.ps1` is the mandatory launch entry point for every `dotnet`
  command that can compile or write shared build outputs, including `restore`, `build`, `rebuild`,
  `test`, `run`, `publish`, `pack`, `watch`, and `msbuild`. Calling those commands as raw
  `dotnet ...` is a contract violation. The wrapper resolves this checkout's absolute root,
  derives a stable SHA-256 name, and waits on its checkout-specific named Mutex:
  `Global\NLTX-DotnetBuild-<first-16-hex-digits>` on Windows. It passes every argument through
  unchanged and returns the child `dotnet` exit code unchanged.

### Critical-section rule

- At most one compile-capable process group may run in this checkout. A `csc.exe` child or an
  active `dotnet` restore/build/test/run command blocks a new compile-capable command. Compliant
  commands queue at the named Mutex; a process that exits unexpectedly releases its ownership
  through the operating system, and the next waiter continues (including after an abandoned
  Mutex is detected).
- Before starting, inspect the process list. If another process is present or its owner is unclear,
  wait for it; do not assume it is stale and do not terminate an unknown owner.

  ```powershell
  $activeBuilds = Get-CimInstance Win32_Process |
    Where-Object {
      $_.Name -eq 'csc.exe' -or
      ($_.Name -eq 'dotnet.exe' -and $_.CommandLine -match
        '(?i)\b(restore|build|rebuild|test|run|publish|pack|watch|msbuild)\b')
    }
  $activeBuilds | Select-Object ProcessId, ParentProcessId, Name, CommandLine
  ```

- Do not launch compile-capable commands with `Start-Process`, background jobs,
  `ForEach-Object -Parallel`, `Promise.all`, or parallel tool calls.
- If independent agents must compile at the same time, use separate worktrees with separate
  `Build/bin` and `Build/obj` roots. Never make parallel sessions share this checkout's outputs.

### Required command shape

Normal work is one affected project at a time. Run restore only when required, then build once and
run verifiers without rebuilding. Every compile-capable invocation uses the wrapper:

```powershell
pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 `
  restore .\path\AffectedProject.csproj -m:1 -nr:false `
  -p:MSBuildNodeReuse=false `
  -p:BuildInParallel=false

pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 `
  build .\path\AffectedProject.csproj -c Release --no-restore `
  -m:1 -nr:false `
  -p:UseSharedCompilation=false `
  -p:MSBuildNodeReuse=false `
  -p:BuildInParallel=false

pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 `
  test .\path\Verifier.csproj -c Release --no-build --no-restore `
  -m:1 -nr:false `
  -p:UseSharedCompilation=false `
  -p:MSBuildNodeReuse=false `
  -p:BuildInParallel=false

pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 `
  run --project .\path\Verifier.csproj -c Release --no-build --no-restore `
  -m:1 -nr:false `
  -p:UseSharedCompilation=false `
  -p:MSBuildNodeReuse=false `
  -p:BuildInParallel=false
```

- Do not use `-t:Rebuild` during normal incremental work. A clean rebuild is an explicit acceptance
  operation and must still be the only compile-capable operation in the checkout.
- Do not build the solution for an incremental slice. A full solution build is allowed only when
  the acceptance gate explicitly requires it and must use the same serial flags.

### Correctness and maintenance gates

- Record the exact command, project, exit code, warning/error counts, and output path for every
  build that supports a claim.
- After a build, verify that its expected artifact is under `Build/bin/` and run the verifier with
  `--no-build`. A focused green verifier is not evidence that an overlapping build was safe.
- If two compile-capable commands overlapped, treat shared `Build/bin` and `Build/obj` outputs as
  untrusted. Wait for all owners to finish, rebuild the affected project serially, and rerun the
  verifier before reporting `verified`.
- The named Mutex in `Build/Tools/Invoke-SerialDotnet.ps1` is the mechanical hard boundary for
  compliant compile-capable commands in this shared checkout. It does not retroactively lock a
  raw `dotnet` or `csc.exe` process that was started before the wrapper, so the process preflight
  remains mandatory when adopting the rule or when an owner is unclear. After a raw overlap,
  follow the output-integrity recovery rule below before trusting artifacts.
- Changes to this contract must update this section first, keep the links in `progress.md` and
  `docs/flowstate/README.md`, and rerun the documented link/diff checks. Do not silently weaken
  the single-project or single-critical-section invariant.

## Output and Generated Files

- Do not write compiled binaries, intermediate files, test results, or source-generator output into `src/` or beside project files.
- Repository-local output locations are enforced by the MSBuild policy:
  - `Build/bin/` for compiled and publish output.
  - `Build/obj/` for intermediate MSBuild/compiler files.
  - `Build/generated/` for compiler/source-generator files.
  - `Build/packages/` for the NuGet global package cache.
- Do not commit regenerable files from these directories. Keep only deliberate documentation or checked-in fixtures under `Build/`.

## Change and Verification Rules

- Keep changes scoped to the requested behavior; preserve unrelated user changes.
- Before claiming a build or test is successful, run the relevant command and report its exit status and warnings/errors.
- For build-policy changes, verify both resolved MSBuild properties and the actual artifact paths.
- Prefer deterministic, serial commands. Use fresh output/evidence paths when a command produces reports.
- Do not use destructive cleanup commands against broad paths. Remove only explicitly identified temporary files created for the current task.

## Model Context Entry Points

- Start with `progress.md` for the compact current-state card.
- Then read `docs/flowstate/README.md` and the active plan/task pair it names.
- For resume work, read `.agent-workplace/state/model-context.json`; consult the full
  `checkpoint.json` artifact list only when a cited evidence path is needed.
- Do not load the entire `docs/plans/`, `docs/research/`, or private task tree unless the active
  batch explicitly requires it.
- Build concurrency contract: `AGENTS.md#dotnet-build-concurrency-contract`; read it before any
  `dotnet` restore/build/test/run command.

## Standard Commands

Use an affected project path from the repository root for incremental work. Apply the serial command
contract above; use `--no-build` for verifier runs after the project has already been built.
