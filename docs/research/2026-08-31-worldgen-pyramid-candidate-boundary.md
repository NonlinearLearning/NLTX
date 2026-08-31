# WorldGen Pyramid candidate boundary

## Source authority

The source-backed boundary is the `Pyramids` pass in
`Build/worldgen-oracle/legacy-instrumented-source/Terraria/WorldGen.cs`:

- `15918-15929`: iterate recorded `GenVars.PyrX/PyrY` candidates and apply
  strict world-edge and dungeon-side filters, with the anniversary
  Underground Desert veto.
- `15931-15935`: scan downward in the column (increasing tile Y) until the
  first active tile or the world-surface limit; continue only for sand tile
  type `53`.
- `15936-15952`: reject candidates closer than the prior pyramid threshold
  (`220`, or `110` for drunk worlds), then decrement the active Y once before
  structure placement.
- `15953-15965`: dual-dungeon potential-bounds checks and the `Pyramid(...)`
  structure mutation are downstream of this boundary and are not implemented
  here.

The adjacent `DunesAndPyramidLocations` source pass is at
`WorldGen.cs:11850-11911`; its own gate and candidate-recording retry loop are
documented in `2026-08-31-worldgen-dunes-pyramid-gate.md`.

## Typed owners

- `src/Terraria.Dome.Simulation/WorldGeneration/LegacyPyramidCandidatePolicy.cs`
  owns the coordinate, dungeon-side, anniversary-desert, and prior-pyramid
  spacing eligibility checks.
- `src/Terraria.Dome.Simulation/WorldGeneration/LegacyPyramidSurfaceScanPolicy.cs`
  owns the snapshot-based first-active scan, exact sand-type check, and the
  source-shaped `activeY - 1` placement result.
- `src/Terraria.Dome.Simulation/WorldGeneration/LegacyPyramidDualDungeonPolicy.cs`
  owns the dual-dungeon potential-bounds decision that follows the `k--`
  placement adjustment.
- `src/Terraria.Dome.Simulation/WorldGeneration/DunesAndPyramidGenerationGate.cs`
  exposes separate Dunes and Pyramids execution gates; its aggregate helper is
  compatibility-only.

## Narrow contract

`LegacyPyramidCandidatePolicy.IsEligible` preserves the following source
predicates:

1. `x > 300 && x < worldWidth - 300` (strict boundaries).
2. Left Dungeon rejects `x < generatingDungeonPositionX + worldWidth * 0.15`.
3. Right Dungeon rejects `x > generatingDungeonPositionX - worldWidth * 0.15`.
4. In a tenth-anniversary, non-dual world, a candidate inside the supplied
   `DungeonBoundsSnapshot` is rejected. A missing snapshot fails closed because
   this branch has no safe way to prove that the Desert bounds were published.
5. Existing pyramid X coordinates reject distance `< 220`, or `< 110` for drunk
   generation; equality is accepted. Distance is calculated in `long` so an
   invalid extreme input cannot overflow `Math.Abs(int)`.

The policy validates the candidate X and non-negative Y, and reports invalid
enum values through their own parameter name. It intentionally does not
validate an upper Y bound because the candidate policy has no world-height
authority; the snapshot-based scan performs full tile-bound checks.

`LegacyPyramidSurfaceScanPolicy.TryFindSandPlacementY` starts at the recorded
candidate Y and inspects rows while `y < worldSurfaceY`. Inactive rows are
skipped. The first active row must be sand type `53`; otherwise the candidate
is rejected. On success the returned placement row is exactly one row above
that active sand tile (`activeY - 1`), matching the legacy `k--` boundary.
Malformed/out-of-world scan coordinates fail closed without reading the
snapshot. No `Pyramid(...)` footprint or tile mutation is emitted.

`LegacyPyramidDualDungeonPolicy.Evaluate` preserves the source's two-step
potential-bounds check with fluff `5`. A hit at `placementY + 125` shifts the
placement up by `50` and changes the maximum depth to `100`; a second hit at
the adjusted `placementY + 100` rejects the candidate. Non-dual worlds bypass
the check. A dual-world decision without a published outer-bounds list fails
closed, because allowing structure placement without the dungeon authority
would be unsafe. This owner returns a decision only; it does not call
`Pyramid(...)`.

## Verification

The focused probe follows RED/GREEN ordering:

- `Build/diagnostics/server-ecs-convergence/P9-worldgen/dunes-pyramid-candidate-20260831-01/pyramid-scan-independent-gates-red.log`
  records the expected missing-owner compile failures for the new gate methods
  and scan policy.
- `.../pyramid-scan-independent-gates-green-rerun.log` records exit 0 for the
  underworld regression, independent Dunes/Pyramids gates, first-active sand
  scan, `k--` placement, non-sand rejection, and surface-limit rejection.
- `.../pyramid-coordinate-validation-red.log` records the expected failing
  coordinate assertion before validation was added.
- `.../pyramid-coordinate-validation-green.log` records exit 0 after the
  validation and overflow-safe distance change.
- `.../pyramid-dual-bounds-red.log` records the expected missing-owner compile
  failures for the dual-dungeon decision owner.
- `.../pyramid-dual-bounds-green.log` records exit 0 for the two-step dual
  potential-bounds veto, depth adjustment, non-dual bypass, and missing-bounds
  fail-closed assertions.
- `.../simulation-build-final.log` records the final Simulation Release build,
  exit 0 with no warnings or errors.
- `.../tunnels-regression.log` records the existing Tunnels/WorldGen regression,
  exit 0.

## Status and deferred scope

Status: `completed_partial` for the candidate/gate/scan boundary only.

Still deferred are DunesBiome structure placement, the Dunes candidate
jungle/center/snow rejection loop and RNG/checkpoints, Pyramid structure
footprints and mutation, dual-dungeon potential-bounds veto, Pyramid min-depth
and `num2` structure arguments, surface-desert and Error World extra pyramids,
complete TileRunner/structure side effects, aggregate ordering, full WLD and
random differential parity, legacy WorldGen deletion, `canRemoveLegacyWorldGen`,
and the 44 deferred `ServerRelevant` rows.
