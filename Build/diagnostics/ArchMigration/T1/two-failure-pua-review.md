# PUA failure review: selected probe invocation

Failure count in the current repair sequence: 2.

## Failure signals read verbatim

1. `dotnet run Test/Terraria.Arch.Verification/Terraria.Arch.Verification.csproj ...` exited 1 with: `找不到要运行的项目。请确保 C:\Users\shan\.codex\worktrees\32f2\NLTX 中存在项目，或使用 --project 传递项目路径。`
2. `apply_patch` exited without editing the file: `Failed to find expected lines in .../invoke-dotnet.ps1`.

Both outputs are preserved in `core-risk-sample.log` and the tool transcript. The failed patch changed no source.

## Search, source context, and assumptions

- The project exists at `Test/Terraria.Arch.Verification/Terraria.Arch.Verification.csproj`.
- Its built DLL exists at `Build/bin/Terraria.Arch.Verification/Debug/net10.0/Terraria.Arch.Verification.dll`.
- `dotnet run --help` says `--project <PROJECT_PATH>` selects the project; without it, the current directory is used.
- The wrapper's source line built arguments as `@('run', $projectPath) + $ExtraArgs`, which omitted `--project`.
- The attempted patch context did not exactly match the wrapper's current text. It was based on a stale/incorrect context assumption.

## Reversed hypothesis and minimal isolation

The failure is in the diagnostic wrapper's CLI argument construction, not restore, build, the project path, or the Arch API. The command never launched the compiled program. The inverse hypothesis is supported by the existing DLL and the .NET CLI help output.

## Direction change

Use a dedicated test runner that invokes `dotnet run --project <path>` explicitly and writes its own output, exit code, warning/error counts, source/DLL/PDB/input hashes, and ledger row. Keep the generic helper for restore/build evidence only. Run the already selected five risk cases once; do not expand the test selection because of the wrapper issue.
