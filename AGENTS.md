# Repository Guidance

## Scope

This file applies to the repository. A deeper `AGENTS.md` may add rules for its
directory, but cannot weaken these build and safety requirements.

## ECS File Organization

- For any ECS component, query, system, event, command, snapshot, definition, or related domain
  file added, moved, split, merged, or renamed anywhere in this repository, read and follow
  [`架构设计/ECS文件组织设计约束.md`](架构设计/ECS文件组织设计约束.md).
- That document is the formal repository-wide entry for ECS file organization. It takes
  precedence over older ECS proposals or sample-specific notes unless a more specific
  directory-level `AGENTS.md` explicitly defines an approved exception.
- Keep the change scoped: do not delete or rewrite unrelated architecture documents merely
  because they are older analyses or historical records.

## C# Code

- Before adding or refactoring C#, read `约束/Google-CSharp-Style-Guide-约束.md`.
- For changed code, follow its naming, layout, and member-ordering rules without
  creating unrelated formatting churn.
- Architecture, safety, layering, and testing take precedence when they conflict
  with style.
- Use `PascalCase` for types and public members, `camelCase` for locals and
  parameters, `_camelCase` for non-public fields and properties, and an `I`
  prefix for interfaces. Use `Npc`, `Ai`, `Id`, and `Uid` rather than all-caps
  abbreviations.
- Prefer one core type per PascalCase file. Do not introduce generic
  `Manager`, `Helper`, `Utility`, `Utils`, `Misc`, or `Data` types.

## Dotnet Build Concurrency Contract

- Use the SDK selected by `global.json`; target `net10.0` unless a project
  explicitly documents a compatibility exception.
- Every compile-capable `dotnet` command (`restore`, `build`, `rebuild`, `test`, `run`,
  `publish`, `pack`, `watch`, or `msbuild`) must run from the repository root
  through `Build/Tools/Invoke-SerialDotnet.ps1`. Raw `dotnet` invocations are
  not allowed.
- Only one compile-capable process may use this checkout at a time. Before
  starting one, inspect active `dotnet.exe` and `csc.exe` processes; wait for an
  existing or unclear owner and do not terminate it. Do not background or
  parallelize build commands.
- Build only the affected project for an incremental change. Restore only when
  necessary, then run verifiers with `--no-build --no-restore`. Do not use
  `-t:Rebuild` unless a clean rebuild is an explicit acceptance requirement.

```powershell
pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 `
  <restore|build|test|run> .\path\AffectedProject.csproj `
  -m:1 -nr:false `
  -p:UseSharedCompilation=false `
  -p:MSBuildNodeReuse=false `
  -p:BuildInParallel=false
```

- If compile-capable commands overlap, treat `Build/bin/` and `Build/obj/` as
  untrusted. Wait for all owners, rebuild the affected project serially, and
  rerun its verifier before reporting a result.

## Outputs And Verification

- Keep generated output out of source directories. The repository uses
  `Build/bin/`, `Build/obj/`, `Build/generated/`, and `Build/packages/`.
  Do not commit regenerable output from these directories.
- Keep changes scoped and preserve unrelated user changes. Do not run broad
  destructive cleanup commands.
- Before reporting a build or test as successful, record the command, project,
  exit code, warning/error counts, and output path. Verify build artifacts are
  under `Build/bin/`.
