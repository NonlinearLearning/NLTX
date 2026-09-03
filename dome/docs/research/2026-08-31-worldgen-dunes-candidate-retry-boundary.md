# WorldGen Dunes candidate retry boundary

## Source authority

The source block is the Dunes portion of
`Build/worldgen-oracle/legacy-instrumented-source/Terraria/WorldGen.cs:11850-11911`.
For each Dunes iteration it repeatedly calls
`RandomWorldPoint(0, 500, 0, 500)`, which draws X from
`[500, worldWidth - 500)` and Y from `[0, worldHeight)`. The candidate is
rejected when any of these source predicates is true:

- `abs(x - jungleOriginX) < (int)(600 * worldWidth / 4200)`;
- `abs(x - worldWidth / 2) < 300`;
- `x > snowOriginLeft - 300 && x < snowOriginRight + 300`.

The retry counter is incremented after each random point. At
`attemptCount >= worldWidth` the jungle veto is disabled; at
`attemptCount >= worldWidth * 2` the snow veto is disabled. The center veto is
not disabled by the source loop. Once all active vetoes are false, the origin
is passed to `DunesBiome.Place`.

## Typed owner

`src/Terraria.Dome.Simulation/WorldGeneration/LegacyDunesCandidatePolicy.cs`
contains:

- `LegacyDunesCandidate`, an immutable `(X, Y, AttemptCount)` result;
- `IsLocationRejected`, which exposes the source predicates and threshold
  transitions for focused verification;
- `Select`, which performs the source-shaped X-then-Y random loop against an
  immutable `WorldMetadata` dimension contract.

Distances are evaluated as `long` values to avoid integer overflow on malformed
origin inputs. The selector rejects metadata too narrow to provide a non-empty
500-tile-padded X range. It does not place a Dunes structure, publish a
protected-structure footprint, or consume the later Pyramid chance draw.

## Verification

The scratch probe recorded the TDD RED compile failure in
`Build/diagnostics/server-ecs-convergence/P9-worldgen/dunes-pyramid-candidate-20260831-01/dunes-candidate-retry-red.log`.
After the owner was added,
`.../dunes-candidate-retry-green-rerun.log` records the threshold boundary
(`attempt=width` retains snow rejection, `attempt=2*width` releases it), valid
coordinate bounds, deterministic replay, and exactly two random samples per
attempt. The same probe also reruns the existing underworld, Pyramid, and
Tunnels assertions.

## Status and deferred scope

Status: `completed_partial` for Dunes candidate coordinate selection only.

Deferred work includes `DunesBiome.Place`, biome structure footprint and
collision authority, candidate placement progress/aggregate ordering, the
per-iteration pyramid chance and `GenVars.PyrX/PyrY` publication, exact global
RNG checkpoints, complete tile/structure side effects, full WLD differential,
legacy WorldGen deletion, `canRemoveLegacyWorldGen`, and the 44 deferred
`ServerRelevant` rows.
