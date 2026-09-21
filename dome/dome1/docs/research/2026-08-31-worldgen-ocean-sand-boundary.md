# WorldGen OceanSand boundary research

**Date:** 2026-08-31  
**Flowstate:** `N6` / `in_progress_with_deferred_findings`  
**Status:** `completed_partial`  
**Owner:** NLTX WorldGen migration / P9 pass-specific batch

## Source evidence

The instrumented Version4 source capture is
`Build/worldgen-oracle/legacy-instrumented-source/Terraria/WorldGen.cs:11913-12021`.
At inspection time the complete source was `2,278,888` bytes and `88,669` lines, with
SHA-256 `C7C2F2196CEA0F6E56C20863A27824917391B74B34232D50590D1DDF4FF321AC`.

The source guard is `!Skyblock.denyAllGeneration && !SecretSeed.noSurface.Enabled`. The pass
executes exactly three source iterations. Each iteration draws an initial X from
`Next(Main.maxTilesX)` and retries while the candidate is strictly inside the central
`40%..60%` band. The left width is `Next(35, 90)`, with the center iteration adding
`(int)(Next(20, 40) * (Main.maxTilesX / 4200.0))`; each side can then be doubled by
`Next(3) == 0`, and the center iteration is doubled again. The right width is drawn separately
with the same doubling rules. Iteration zero is forced to `[0, GenVars.leftBeachEnd)`, iteration
one performs its setup draws and then skips, and iteration two is forced to
`[GenVars.rightBeachStart, Main.maxTilesX)`.

For each active band, depth starts at `Next(50, 100)`. Every column consumes `Next(2)` and,
when enabled, `Next(-1, 2)` followed by the source `50..200` clamp. The first active tile is
scanned from Y zero while `Y < (Main.worldSurface + Main.rockLayer) / 2.0`. At the band midpoint,
`Next(6) == 0` records a pyramid candidate. The carve depth is the depth limited by both band
edges, then increased by `Next(5)`. The legacy inner loop writes tile type `53` only when each
row passes independent random edge checks; that mutation is intentionally outside this boundary.

## Typed owner

The scheduling and projection owner is
`src/Terraria.Dome.Simulation/WorldGeneration/LegacyOceanSandPass.cs`, with immutable value
records in `LegacyOceanSandBandPlan`, `LegacyOceanSandColumnPlan`,
`LegacyOceanSandTileWrite`, and `LegacyOceanSandPassResult`. `CreatePlan` validates the runtime
profile, preserves the source guard and draw order, records per-band/per-column random sample
counts, and returns frozen copies of all result lists. Its `TileWrites` list records every row that
passes the source's independent left/right random edge checks, while retaining the source behavior
that a write may target an inactive tile. `AppendCommands` projects those writes as
`TileChangeKind.UpdateTileType` commands for type `53`, explicitly preserving the source tile's
active bit and committing through the existing typed `TileChangeCommitSystem` boundary in the
focused verifier. It consumes an immutable `WorldGridSnapshot` and a pass-scoped
`LegacyPassRandomState`; no legacy `Main`, mutable tile array, network, or persistence dependency
is introduced.

The accepted scheduling boundary covers:

- Skyblock and No Surface no-op guards that consume no random state;
- all three source iterations, center retry exclusion, width draws, scaling, doubling, and
  beach forcing;
- center-iteration setup draws followed by the source-equivalent skip;
- initial depth, per-column drift, clamp, first-active scan, midpoint pyramid signal, and depth
  variation; and
- deterministic replay, bounded column metadata, immutable result-list publication, and the
  source-attributed type-53 tile-write projection.

It does not claim aggregate pipeline wiring, full `TileRunner` traversal, pyramid structure
placement/collision semantics, dune placement, aggregate pass order, global RNG parity, WLD or
extended-state parity, or legacy `WorldGen` deletion.

## Underworld compatibility correction

The same verifier rerun exposed an existing edge-origin incompatibility in
`LegacyUnderworldEvilRunnerFactory`. The source at
`WorldGen.cs:14278-14304` calls `TileRunner` with `origin.X + Next(-10, 10)` and can therefore
produce a negative start at the world edge; legacy `TileRunner` accepts that start and clamps its
traversal envelope later. The public `LegacyTileRunnerRequest` constructor previously rejected
all negative coordinates before traversal.

`LegacyTileRunnerRequest.CreateLegacyUnboundedStart(...)` is now an internal, explicit
compatibility path used only by `LegacyUnderworldEvilRunnerFactory`; the public constructor keeps
its negative-coordinate rejection. The focused regression proves both sides: ordinary callers
still fail closed, while the source-shaped Underworld request retains the negative X without
throwing. This change does not widen ordinary request validation or implement Underworld liquid
mutation.

## Verification evidence

Scheduling-only evidence remains under
`Build/diagnostics/server-ecs-convergence/P9-worldgen/ocean-sand-boundary-20260831-02/`.
Fresh projection evidence is under
`Build/diagnostics/server-ecs-convergence/P9-worldgen/ocean-sand-boundary-20260831-03/`:

- `ocean-sand-focused.log` and `ocean-sand-focused-exit-code.txt` — focused OceanSand replay,
  guards, three-band scheduling, beach forcing, depth state, pyramid signals, deterministic
  in-bounds type-53 writes, source-attributed command emission, and actual world-grid commit;
  exit code `0`, `200` columns, `11,308` random samples.
- `underworld-evil-bounds-focused.log` and `underworld-evil-bounds-exit-code.txt` — public request
  rejection plus source-compatible edge start; exit code `0`.
- `worldgen-focused.log` and `worldgen-focused-exit-code.txt` — complete bounded WorldGeneration
  verifier from the rebuilt Release DLL after the projection change; exit code `0`, `280` anchored
  `PASS` lines, `40` `CHECK` lines, no anchored `FAIL`/`ERROR` lines, and no unhandled exception.

The serial Release builds also passed with zero warnings and zero errors:

- `simulation-build.log` / `simulation-build-exit-code.txt` for
  `src/Terraria.Dome.Simulation/Terraria.Dome.Simulation.csproj`;
- `verifier-build.log` / `verifier-build-exit-code.txt` for
  `Test/Terraria.Dome.WorldGeneration.Verification/Terraria.Dome.WorldGeneration.Verification.csproj`;
- `server-build.log` / `server-build-exit-code.txt` for
  `src/Terraria.Dome.Server/Terraria.Dome.Server.csproj`.

The `...-20260831-03/` build logs are the post-projection rebuilds. The earlier `...-20260831-02/`
build logs establish the pre-projection scheduling boundary and remain retained for replay history.

## Current limits and gate state

This is a source-backed scheduling and type-53 projection slice, not complete OceanSand or WorldGen
parity. Aggregate pipeline wiring, tile/structure authority beyond the direct projection, full
TileRunner and pyramid behavior, shared aggregate random ordering, WLD differential, extended
state, and all other deferred WorldGen passes remain open. The overall WorldGen differential
remains negative, `canRemoveLegacyWorldGen = false`, and all `44` `ServerRelevant` ledger rows
remain `deferred`. This batch therefore remains `completed_partial`; the green focused and bounded
verifier runs do not authorize legacy deletion or a full-parity claim.
