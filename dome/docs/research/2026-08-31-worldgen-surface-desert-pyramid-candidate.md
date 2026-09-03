# WorldGen surface-desert extra-Pyramid candidate boundary

## Source authority

The source-backed block is the `surfaceIsDesert` branch of the legacy
`Pyramids` pass at
`Build/worldgen-oracle/legacy-instrumented-source/Terraria/WorldGen.cs:15970-15990`.
The branch is reached only after the pass-level `!Skyblock` and
`!noSurfaceNoPyramids` guard. It performs a random extra-Pyramid count, scales
that count by the integer `worldWidth / 4200`, and then selects one X per
iteration from `[300, worldWidth - 300)`. The X-only retry loop redraws while
the strict center interval `worldWidth * 0.47 < x < worldWidth * 0.53` is hit.

The selected column is scanned from `FindLowestCloud()` toward
`Main.worldSurface`; the first active tile must be sand type `53`, after which
the source decrements the scan row and calls `Pyramid(num7, n)`. The scan is
owned separately by `LegacyPyramidSurfaceScanPolicy`; this batch does not
reimplement `FindLowestCloud`, sand scanning, or `Pyramid(...)` structure
mutation.

The source file hash for this capture is
`C7C2F2196CEA0F6E56C20863A27824917391B74B34232D50590D1DDF4FF321AC`.

## Typed owner

`src/Terraria.Dome.Simulation/WorldGeneration/LegacySurfaceDesertPyramidCandidatePolicy.cs`
contains:

- `LegacySurfaceDesertPyramidCandidate`, an immutable X plus center-retry
  count;
- `CalculateIterationCount`, which consumes the source `Next(5, 8)` draw and
  applies integer `4200` width scaling;
- `SelectCandidates`, which preserves one X draw per candidate plus X-only
  center retries;
- `IsCenterRejected`, exposing the strict center predicate for direct
  endpoint verification.

The owner validates that `[300, width - 300)` contains at least one
non-center coordinate before entering the retry loop. This is a fail-closed
simulation boundary for malformed narrow dimensions; it does not alter normal
Terraria-sized worlds.

## Verification

The focused probe follows TDD RED/GREEN ordering:

- `Build/diagnostics/server-ecs-convergence/P9-worldgen/surface-desert-pyramid-20260831-01/surface-desert-pyramid-red.log`
  records the expected compile failure while the new owner is absent.
- `.../surface-desert-pyramid-green.log` records exit `0` for count draw and
  width scaling, strict center endpoints, candidate bounds, deterministic
  replay, retry sample accounting, and the narrow-world guard. Existing
  Underworld, Dungeon, Pyramid, and Tunnels assertions in the same probe also
  remain green.
- `.../simulation-build.log` records the Simulation Release build with exit
  `0`, zero warnings, and zero errors.

## Status and deferred scope

Status: `completed_partial` for surface-desert extra-Pyramid candidate count
and X retry selection only.

Deferred are `FindLowestCloud` ownership, surface sand scan integration,
`Pyramid(...)` footprint/collision/tile-wall mutation, per-candidate
structure publication, complete secret-seed variants, exact global RNG
checkpoints, aggregate ordering, full WLD differential, legacy WorldGen
deletion, `canRemoveLegacyWorldGen`, and the 44 deferred `ServerRelevant`
rows.
