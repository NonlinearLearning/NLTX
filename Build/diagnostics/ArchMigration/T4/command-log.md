# T4-B0/B1 command and evidence log

Worktree: C:\Users\shan\.codex\worktrees\d0b9\NLTX
Baseline: e2c686790ab6a4f14505ea914c27478f5959d061
Timestamp: 2026-10-08 Asia/Shanghai

The commands below are read-only inventory/evidence commands. No build, restore, test or runtime verification command was run for T4 in this batch.

| Command / operation | Project or scope | Exit | Warnings / errors | Saved output | Source/DLL/PDB/input hash |
| --- | --- | ---: | ---: | --- | --- |
| dotnet --version; inspect project.assets.json for Simulation, NetworkServer and selected verification projects | SDK/environment | 0 | 0 / 0 | environment.txt | SDK 10.0.400; no project assets found; no DLL/PDB output |
| rg --files docs Context filtered for Arch plans, research, Context and navigation | T4 plan and inputs | 0 | 0 / 0 | handoff.md; source-hashes.md | combined input manifest in source-hashes.md |
| rg -n for Match, TryEdit, QueryDescription, Query, EntityReference, reservation and item revision symbols | Simulation/Application/Network/Items source | 0 | 0 / 0 | b0-b1-access-ownership-matrix.md | current source hashes in source-hashes.md |
| rg -n 'using Arch|Arch.Core|QueryDescription|\\.Query\\(' over audited production directories | NSSLC, Application, Simulation, NetworkServer | 1 (zero matches) | 0 / 0 | b0-b1-access-ownership-matrix.md | no T4 DLL/PDB; current source manifest hash in source-hashes.md |
| inspect WorldSimulationKernel, WorldSimulationPhase and simulation host phase registration | Application / Simulation host | 0 | 0 / 0 | b0-b1-access-ownership-matrix.md | source hashes in source-hashes.md |
| inspect T2 handoff.md, T3 execution handoff and T3-command-index.json in their isolated worktrees | T2/T3 dependency evidence | 0 | 0 / 0 | handoff.md | exact external evidence hashes in source-hashes.md |
| inspect T1/T2/T3 thread snapshots and list T1/T2/T3 diagnostic handoff files | coordination state | 0 | 0 / 0 | handoff.md | no T4 DLL/PDB/input produced |
| git diff --cached --check | T4 diagnostics commit | 0 after whitespace correction | 0 / 0 | staged diff only | diagnostics/source fingerprints listed in source-hashes.md |
| dotnet build / dotnet test / simulation smoke | T4 | not-run | n/a | no output | no source change; no DLL/PDB generated |

The manifest covers 38 current-worktree source, project, plan, research and world-input files. Combined manifest SHA-256: EBBF255273BCB228FAB7BCE341815DB3E5EA380A07018C4BB43D81DAA1FD6C2C. See source-hashes.md for per-file values and external T2/T3 hashes.
