# WorldGen Error World extra-Pyramid candidate boundary

## Source authority

The source-backed block is the `errorWorld` branch of the legacy `Pyramids`
pass at
`Build/worldgen-oracle/legacy-instrumented-source/Terraria/WorldGen.cs:15992-16010`.
The branch runs inside the pass-level `!Skyblock` and
`!noSurfaceNoPyramids` guard. It draws `genRand.Next(5, 8)`, multiplies by the
integer `worldWidth / 4200`, then divides by
`SecretSeed.Variations.errorWorldAdjustment(1.0)`. For each resulting
iteration it draws X from `[300, worldWidth - 300)` and Y from
`[FindLowestCloud(), (int)Main.rockLayer)`.

The source retries both coordinates together while either strict center
predicate (`width * 0.47 < x < width * 0.53`) or Euclidean shimmer distance
(`distance < 300`) rejects the point. After a candidate is accepted, the
legacy code scans to the first active tile, decrements Y, and calls
`Pyramid(...)`; this batch intentionally stops before that scan and structure
mutation.

The source file hash for this capture is
`C7C2F2196CEA0F6E56C20863A27824917391B74B34232D50590D1DDF4FF321AC`.

## Typed owner

`src/Terraria.Dome.Simulation/WorldGeneration/LegacyErrorWorldPyramidCandidatePolicy.cs`
contains:

- `LegacyErrorWorldPyramidCandidate`, an immutable X/Y coordinate and joint
  retry count;
- `CalculateIterationCount`, which preserves the count draw, integer width
  scaling, and positive error-world adjustment divisor;
- `SelectCandidates`, which preserves X-then-Y random draw order and redraws
  both coordinates on every rejection;
- `IsCenterRejected` and `IsLocationRejected`, exposing the strict center and
  `< 300` shimmer predicates for direct boundary verification.

The owner validates positive dimensions and a non-empty Y range and rejects a
world width with no usable `[300,width-300)` range before entering the retry
loop. It returns candidate decisions only; it does not publish shimmer state,
read tiles, perform the first-active scan, decrement placement Y, or call
`Pyramid(...)`.

## Verification

The focused probe follows TDD RED/GREEN ordering:

- `Build/diagnostics/server-ecs-convergence/P9-worldgen/surface-desert-pyramid-20260831-01/error-world-pyramid-red.log`
  records the expected compile failure while the new owner is absent.
- `.../error-world-pyramid-green.log` records exit `0` for count scaling,
  strict center and shimmer endpoints, source coordinate ranges, joint retry
  accounting, deterministic replay, and the narrow-world guard. The existing
  Underworld, Dungeon, Pyramid, and Tunnels assertions in the same probe also
  remain green.
- `.../final-focused-verifier.log` records the clean Release rerun with
  `exitCode=0` after the production build.
- `.../simulation-build-error-world.log` records the Simulation Release build
  with exit `0`, zero warnings, and zero errors.

## Status and deferred scope

Status: `completed_partial` for Error World extra-Pyramid count and joint
center/shimmer candidate selection only.

Deferred are shimmer publication provenance, first-active tile scanning and
`n--` placement, `Pyramid(...)` footprint/collision/tile-wall mutation,
complete error-world adjustment variants, exact global RNG/checkpoints,
aggregate ordering, full WLD differential, legacy WorldGen deletion,
`canRemoveLegacyWorldGen`, and the 44 deferred `ServerRelevant` rows.
