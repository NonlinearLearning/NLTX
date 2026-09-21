# WorldGen WavyCaves Boundary

**Date:** 2026-08-31  
**Flowstate:** `N6` / `in_progress_with_deferred_findings`  
**Status:** `completed_partial`

## Legacy source contract

The source-backed boundary is `Build/worldgen-oracle/legacy-instrumented-source/Terraria/WorldGen.cs:12786-12831`.
The pass runs only when `!Skyblock.denyAllGeneration && dontStarveWorldGen`. Its invocation count
is `floor(35 * (width / 4200)^2)` and Remix divides that truncated count by `3`.

Each invocation interpolates X from `80` to `width - 80`, samples Y from
`worldSurface + 100` through `UnderworldLayer - 100` (exclusive), and retries up to 100 times
when the previous Y is within 80 tiles. `Main.UnderworldLayer` is `maxTilesY - 200`, so the
effective upper bound is `height - 300`. The remaining draws are strength `Next(3, 6)`, percent
`0.25 + NextDouble()`, steps `Next(300, 500)`, and tile type `-1`, followed by `WavyCaverer`.

## Typed owner

`src/Terraria.Dome.Simulation/WorldGeneration/LegacyWavyCavesPass.cs` owns the pass-level guard,
density, Y spacing, X interpolation, and random draw order. The pipeline invokes it through
`WorldGenerationRequest.IsDontStarveWorld` and `IsSkyblockWorld`. Terminal carving remains owned
by the existing `LegacyWavyCaverer` helper and commits through the typed tile-command boundary.
The same request-level Skyblock value is propagated to the existing RocksInDirt, DirtInRocks, Clay,
RockLayerCaves, SurfaceCaves, and MountainCaves owners, keeping profile-enabled cave passes under
one guard authority.
`LegacyCavePassSystem` also forwards it to the profile-backed DirtLayerCaves owner. SmallHoles has
an explicit guarded execution overload, so its Skyblock path exits before validating liquid input,
sampling random state, advancing generation state, or emitting tile/liquid commands.

## Verification evidence

Evidence is under
`Build/diagnostics/server-ecs-convergence/P9-worldgen/wavy-caves-boundary-20260831-02/`:

- `wavy-caves-focused-green.log`: both focused checks passed, including the exact
  `UnderworldLayer - 100` oracle; the enabled fixture emitted `20,180` commands and consumed
  `1,593` random samples.
- `verifier-build-green-retry.log`: Release verifier build completed after concurrent compiler
  processes drained; no C# warnings or errors were reported by the completed build.
- `worldgeneration-verifier-full.log`: full WorldGeneration verifier exited `0`.
- `Build/diagnostics/server-ecs-convergence/P9-worldgen/skyblock-cave-guard-propagation-20260831-01/`
  records the regression gate after replacing the remaining hardcoded cave-owner guards.
- `Build/diagnostics/server-ecs-convergence/P9-worldgen/smallholes-skyblock-guard-20260831-01/`
  records a clean verifier rebuild, full verifier, Simulation build, Server build, and diff check.
  All exit `0`; the full verifier reports 280 PASS and 40 CHECK lines.

The focused checks also prove that Skyblock and non-dont-starve guards consume no commands,
random samples, state, or sequence, and that enabled commands carry `worldgen.cave.WavyCaverer`
source attribution.

## Deferred boundaries

This is not full WavyCaverer or WorldGen parity. Full helper shape/random parity, `dontStarve`
runtime projection, complete TileRunner traversal and liquid side effects, aggregate cave
ordering, full WLD/extended-state differential, `canRemoveLegacyWorldGen`, legacy deletion, and
the 44 deferred `ServerRelevant` rows remain deferred. Repository release state remains
`in_progress_with_deferred_findings`, `canRemoveLegacyWorldGen=false`, and aggregate
WorldGen/WLD `partial_blocked`.
