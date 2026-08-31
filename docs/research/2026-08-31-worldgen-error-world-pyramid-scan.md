# WorldGen Error World extra-Pyramid first-active scan boundary

## Source authority

The Error World extra-Pyramid branch in
`Build/worldgen-oracle/legacy-instrumented-source/Terraria/WorldGen.cs:16006-16010`
advances the accepted random Y until `Main.tile[num10, num11].active()` is
true, decrements the first active row once, and passes that row to
`Pyramid(num10, num11)`. Unlike the ordinary recorded-candidate branch, this
source loop does not require sand type `53` and does not stop at
`Main.worldSurface`.

The typed boundary uses the immutable snapshot height as a safe upper bound.
It skips inactive rows, returns exactly `activeY - 1` for the first active tile,
and fails closed for out-of-world coordinates, an empty scan, or a first active
tile at row zero whose decremented placement would be invalid. It emits no
structure or tile mutation.

## Typed owner

`src/Terraria.Dome.Simulation/WorldGeneration/LegacyErrorWorldPyramidSurfaceScanPolicy.cs`
owns `TryFindFirstActivePlacementY`. The preceding random count and joint
center/shimmer retry remain in
`LegacyErrorWorldPyramidCandidatePolicy`; this owner is intentionally limited
to the snapshot scan and source-shaped `n--` placement row.

## Verification

The focused probe follows TDD RED/GREEN ordering:

- `Build/diagnostics/server-ecs-convergence/P9-worldgen/surface-desert-pyramid-20260831-01/error-world-pyramid-scan-red.log`
  records the expected missing-owner compile failure.
- `.../error-world-pyramid-scan-green.log` records exit `0` for inactive-row
  skipping, first-active placement, height-limit rejection, invalid-coordinate
  rejection, and row-zero fail-closed behavior.
- `.../error-world-pyramid-scan-final-focused.log` records the clean Release
  rerun with `exitCode=0`.
- `.../simulation-build-error-world-scan-final.log` records the Simulation
  Release build with exit `0`, zero warnings, and zero errors.

## Status and deferred scope

Status: `completed_partial` for the Error World first-active scan boundary
only. Shimmer publication, source-unbounded malformed-world behavior,
`Pyramid(...)` footprint/collision/tile-wall mutation, exact global
RNG/checkpoints, aggregate ordering, full WLD differential, legacy WorldGen
deletion, `canRemoveLegacyWorldGen`, and the 44 deferred `ServerRelevant` rows
remain open.
