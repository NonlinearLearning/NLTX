# T5 B0/B3 source and project audit

- Worktree baseline: detached `HEAD e2c686790ab6a4f14505ea914c27478f5959d061`; SDK `10.0.400`; `global.json` selects `10.0.400`; source-tree fingerprint `D22E54FEBE9AD29ECF8DB40E872A77D0A7195ECA3174C1625265B23527FC0CFE`.
- Existing user changes: six untracked `docs/plans/2026-10-08-arch-migration-track-t1...t5` and coordination contracts. They remain untouched and unstaged.
- T1–T4/A0 evidence directories were absent before this T5 run; their contract documents still say active. No accepted handoff or total-acceptance approval exists in the current worktree evidence.
- Search of production and tests found no `PackageReference Include="Arch"`, `using Arch.Core`, `Arch.Core.World`, or `Arch.Buffer` usage (the no-match search exit was 1; interpreted as zero hits).
- The host and session still compile against the custom runtime: `LoadedWorldSession` constructs one `EntityRuntime`, exposes its runtime id, passes it to `WorldStorageRoot`, and disposes the storage then runtime. `WorldStorageRoot` owns player/NPC/projectile/world-item slot stores, projectile identities, tile entities, tile map, and related registries on that runtime.
- Simulation composition constructs `LoadedWorldSession` and runtime owners around `session.EntityRuntime`; the success smoke report records UUID/runtime-id references and `ContentSource=simulation-core-v3`, confirming this was a custom-ECS baseline. NetworkServer loads a `LoadedWorldSession`, reads `session.EntityRuntime.RuntimeId`, and passes the value to packet/world owners.
- Save/load remain in Application/Infrastructure: `WorldLoadCoordinator`, `WorldSaveCoordinator`, and `WorldStorageCoordinatorFactory`; no Arch.Persistence is referenced. The 319 fixture roundtrip verified 27 APIs and legacy DTO/Codec persistence only. It is fixture setup and is not Arch save/load evidence.
- Normal load/tick/exit has one passing baseline smoke (1 tick, one player). Two switches, cancel, zero tick, late-finalize failure/rollback/retry remain unexecuted. CLI hooks exist in current Simulation source, but their Arch ownership semantics are absent.
- The tracked `src/World/科研.wld` file is 11,713,581 bytes, SHA256 `7B6D6F46D6C805402AE22D381A16CDFE0CD60B676549EF0AFF5DB0A8F8B53B9E`, WorldFile version 326; `world.backgrounds.load` rejects 326. It was read-only throughout. The generated 319 fixture lives only under T5 diagnostics.
- Exact candidate production source references and test/reference counts are in `a8-delete-manifest.csv` and `a8-symbol-counts.csv`. `src/NSSLC.Infrastructure/分类参考/` was counted separately as reference-only, excluded from production-candidate counts per repository constraints.

## Current composition and ownership entry points

| Path | Observed role | Current custom-runtime dependency |
| --- | --- | --- |
| `src/NSSLC.Application/WorldStorage/Loading/LoadedWorldSession.cs` | session creation, token/runtime identity, publication state and disposal | `new EntityRuntime`, `WorldRuntimeId`, `EntityRuntime` property |
| `src/NSSLC/Component/WorldStorage/System/WorldStorageRoot.cs` | world slots, item storage, projectile identity and TileEntity stores | constructor and aliases take the session `EntityRuntime` |
| `src/NSSLC.Tools.Simulation/Program.cs` | default headless host, load, runtime owner creation, switch and rollback probes, tick and optional save | uses session runtime to create all runtime owners |
| `src/NSSLC.Tools.NetworkServer/Program.cs` | network host composition and packet projections | reads runtime id from loaded session |
| `src/NSSLC.Application/Simulation/WorldSimulationKernel.cs` | owner-thread deterministic phase loop and request queue | coordinates through `LoadedWorldSession`; no Arch World |
| `src/NSSLC.Infrastructure/WorldStorage/WorldStorageCoordinatorFactory.cs` | prepare/commit/publish/recovery wiring | owns session publication/retirement with `EntityRuntime.EnsureCanDispose()` |

## Source evidence classification

A8 deletion is **not executed**. The CSV is an audit candidate list, not authorization to delete. Each row was found in `src/` outside `分类参考`; project compile inclusion and dependency closure must be reviewed with T1–T4 integrated. Test files and reference-only sources are reported separately. No source files were edited in T5.
