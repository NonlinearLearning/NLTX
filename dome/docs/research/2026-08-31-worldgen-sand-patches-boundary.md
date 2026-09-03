# WorldGen SandPatches boundary research

**Date:** 2026-08-31
**Flowstate:** `N6` / `in_progress_with_deferred_findings`
**Status:** `completed_partial`
**Owner:** NLTX WorldGen migration / P9 pass-specific batch

## Source evidence

The instrumented Version4 source capture is
`Build/worldgen-oracle/legacy-instrumented-source/Terraria/WorldGen.cs:12022-12052`.
The complete capture is `2,278,888` bytes and `88,669` lines, with SHA-256
`C7C2F2196CEA0F6E56C20863A27824917391B74B34232D50590D1DDF4FF321AC`. The UTF-8 excerpt
hash for the inclusive 31-line pass block is
`208e30ea4af71b3dbdd36489232423a5b01fa02f9b7dd1c37be7191ffcf81274`.

The source registers `GenPassNameID.SandPatches` and runs only when
`!Skyblock.denyAllGeneration`. Its default invocation count is
`(int)(Main.maxTilesX * 0.013)`; Remix divides that integer count by `4`. Every invocation
draws X from `Next(0, Main.maxTilesX)` and default Y from
`Next((int)Main.worldSurface, (int)Main.rockLayer)`. Remix replaces the initial Y range with
`Next((int)Main.rockLayer - 100, Main.maxTilesY - 350)`.

The source retries while X is strictly inside `Main.maxTilesX * 0.46..0.54` and Y is below
`Main.worldSurface + 150.0`. Every retry draws a new X from the full width and deliberately
falls back to the default surface-to-rock Y range, even for Remix. The default Y is drawn before
the Remix override, so a non-retried invocation consumes five scheduling draws in Remix (X,
default Y, Remix Y, strength, steps); each retry adds another X/Y pair. The final request draws
strength from `Next(15, 70)`, steps from `Next(20, 130)`, and calls
`TileRunner(x, y, strength, steps, 53)` with the overload defaults (`addTile=false`, zero
direction, `noYChange=false`, overwrite enabled, and `ignoreTileType=-1`).

## Typed owner

`src/Terraria.Dome.Simulation/WorldGeneration/LegacySandPatchesPassDefinition.cs` owns the
source constants, integer count/divisor behavior, and validated default/Remix/retry Y ranges.
`LegacySandPatchesPass` owns pass-scoped random scheduling and creates immutable
`LegacyTileRunnerPassInvocation` records. It preserves the final draw coordinates after central
retries and records `RandomDrawCount = (isRemixWorld ? 5 : 4) + (retryCount * 2)` for the
initial/final request draws plus every retry X/Y pair.

`AppendCommands` reuses the existing `LegacyTileRunnerRequest`, invocation,
`LegacyTileRunnerTraversal`, `WorldGenerationStateComponent`, and `TileChangeCommand` boundaries.
It reads only a `WorldGridSnapshot`, advances the state to `Cave` when work exists, and attributes
commands through the existing `worldgen.cave.SandPatches.sand-patch` recipe source. No mutable
legacy `Main` state, direct world-array mutation, network, or persistence dependency was added.

The accepted narrow boundary covers:

- the Skyblock no-op guard without random consumption;
- default and Remix invocation cardinality, including integer Remix division;
- default, Remix, and retry Y ranges;
- strict central retry and source-compatible default-Y fallback;
- strength, steps, TileRunner request fields, deterministic draw accounting, and in-bounds
  coordinates; and
- deterministic source-attributed traversal commands and typed tile-command commit from an
  immutable snapshot.

## Verification evidence

Fresh evidence is under
`Build/diagnostics/server-ecs-convergence/P9-worldgen/sand-patches-boundary-20260831-01/`:

- `sand-patches-red-build.log` / `sand-patches-red-build-exit-code.txt` records the initial
  TDD RED compile with the expected missing `LegacySandPatchesPassDefinition` and
  `LegacySandPatchesPass` symbols (exit `1`). The first isolated attempts also record unrelated
  output-path and concurrent dependency-build failures; they are retained as diagnostics.
- `simulation-build-corrected-20260831-01.log` /
  `simulation-build-corrected-20260831-01-exit-code.txt` records a fresh post-correction
  Simulation Release build (exit `0`, zero warnings/errors). The earlier concurrent
  `DomeSimulation.cs` (`CS1503`) failure remains retained in `simulation-build-2.log` /
  `simulation-build-2-exit-code.txt` as a historical diagnostic; this batch did not alter that
  ECS work.
- `verifier-build-corrected-20260831-01.log` /
  `verifier-build-corrected-20260831-01-exit-code.txt` records the verifier Release build
  against the corrected Simulation DLL (exit `0`, zero warnings/errors).
- `server-build-corrected-20260831-01.log` /
  `server-build-corrected-20260831-01-exit-code.txt` records the dependent Server Release build
  (exit `0`, zero warnings/errors).
- `style-check-corrected-20260831-01.log` /
  `style-check-corrected-20260831-01-exit-code.txt` records the scoped Google-style hygiene check
  for both new owner files (exit `0`, no tabs, no lines over 100 characters).
- `sand-patches-source-probe.log` / `sand-patches-source-probe-exit-code.txt` compiles and runs
  the current two owner files against the previously built Simulation reference: exit `0`, Remix
  source-order replay (`17` scheduling draws), bounded default traversal commands, and actual
  typed tile commit. Its compiler warnings are limited to intentional scratch-probe duplicate
  type shadowing and a private output-path override.
- `sand-patches-focused-final.log` / `sand-patches-focused-final-exit-code.txt` records the last
  verifier run before the Remix draw-order correction and is retained as historical evidence.
- `sand-patches-focused-corrected-20260831-01.log` /
  `sand-patches-focused-corrected-20260831-01-exit-code.txt` records the current focused GREEN
  run: exit `0`, `13` default invocations, `54` default scheduling samples, `14,594` bounded
  default commands, deterministic replay, and actual typed tile commit. The independent source
  probe remains retained as additional Remix source-order evidence (`17` scheduling draws).
- `worldgen-focused-final.log` / `worldgen-focused-final-exit-code.txt` retains the prior
  complete verifier result (`281` `PASS` lines, including the stage-trace line) as historical
  evidence. The current corrected bounded WorldGeneration verifier is recorded in
  `worldgen-focused-corrected-20260831-01.log` /
  `worldgen-focused-corrected-20260831-01-exit-code.txt`: exit `0`, `281` `PASS` lines
  (including the stage-trace line), `40` `CHECK` lines, and no anchored `FAIL:`/`ERROR:` lines.

The current-tree rerun is under
`Build/diagnostics/server-ecs-convergence/P9-worldgen/sand-patches-boundary-20260831-02/`.
Its focused run exits `0` with `13` default invocations, `54` scheduling samples, and
`14,594` bounded commands. A fresh bounded WorldGeneration verifier rebuild and run also
exit `0`, with `280 PASS`, `40 CHECK`, and no anchored `FAIL:`/`ERROR:` lines. These
current-tree results confirm regression availability after the Pyramid footprint owner
landed; they do not change the partial boundary or complete-parity gate.

## Current limits and gate state

This is a source-backed SandPatches scheduling and TileRunner invocation/traversal boundary, not
complete SandPatches or WorldGen parity. Full TileRunner semantic parity, aggregate pass wiring
and ordering, global RNG/checkpoint parity, WLD and extended-state parity, and legacy WorldGen
deletion remain deferred. The overall WorldGen differential remains negative,
`canRemoveLegacyWorldGen = false`, and all `44` `ServerRelevant` ledger rows remain
`deferred`. Green focused or bounded verifiers do not authorize legacy deletion or a full-parity
claim.
