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
- Run `dotnet` commands from the repository root so `NuGet.config`, `Directory.Build.props`, and
  `Directory.Build.targets` are applied.
- Use `-p:UseSharedCompilation=false` for serial restore/build/test runs in this workspace to avoid
  compiler output contention.

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

## Standard Commands

No project file is currently present in this checkout. When a project is restored, use its path from
the repository root and apply `-p:UseSharedCompilation=false` to serial build and test commands.
