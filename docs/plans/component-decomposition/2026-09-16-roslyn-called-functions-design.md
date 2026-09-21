# Roslyn Called-Functions Analyzer Design

## Goal

Add a reusable tool to `ecs-system-domain-splitting` that analyzes the whole
`D:\TRbackup\Version4` C# project and reports the functions called from one
selected function.

## Scope

- Discover the project file from a supplied source root, or accept an explicit
  `.csproj` path.
- Enumerate C# source files while excluding generated/build directories.
- Parse source files concurrently with `Parallel.ForEachAsync`.
- Build one Roslyn compilation with concurrent semantic analysis enabled.
- Bind invocations and object creations to `IMethodSymbol` targets.
- Support direct calls by default and an optional transitive call closure.
- Emit deterministic JSON containing the target, call sites, resolved symbols,
  source declarations, diagnostics, and parallel-scan statistics.

## Boundary Decisions

- The tool uses `Microsoft.CodeAnalysis.CSharp` directly instead of
  `MSBuildWorkspace`; Version4 targets .NET Framework 4.0 and contains legacy
  references that are not safe to evaluate through the NLTX .NET 10 build.
- Project metadata is read with `XDocument`. Source discovery remains explicit
  and deterministic, while `<Reference><HintPath>` and user-supplied references
  provide external assembly symbols.
- A target is selected by method name, with optional type and file filters. An
  ambiguous match is an error rather than an arbitrary choice.
- Unresolved calls remain in the JSON with `resolved: false`; partial source
  compilation must not silently discard evidence.

## Verification

- A fixture spanning multiple files proves cross-file binding, constructor
  detection, deterministic output, and transitive closure.
- The full-project command is run against `D:\TRbackup\Version4` and records
  file counts, diagnostics, and the selected target.
- The project is built serially through `Build/Tools/Invoke-SerialDotnet.ps1`;
  the expected artifact must be under `Build/bin/`.
