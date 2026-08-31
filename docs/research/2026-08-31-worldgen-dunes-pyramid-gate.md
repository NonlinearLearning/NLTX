# WorldGen Dunes and Pyramid generation gates

The legacy source exposes two adjacent but independent generation passes. The
`DunesAndPyramidLocations` pass (oracle `WorldGen.cs:11850-11911`) performs dune
placement and records pyramid candidates only when both Skyblock and No Surface
are disabled. The later `Pyramids` pass (oracle `WorldGen.cs:15907-16014`) runs
when Skyblock is disabled and the `noSurfaceNoPyramids` variation is disabled.
Those guards are not interchangeable: for example, a No Surface + Error World
configuration can skip dune placement while still allowing the Pyramids pass.

`DunesAndPyramidGenerationGate.ShouldRunDunes` and
`DunesAndPyramidGenerationGate.ShouldRunPyramids` preserve the two source-level
authorities. `ShouldRun` remains a compatibility aggregate that is true only
when both pass-specific gates are true; a pipeline must select the method for
the pass it is executing rather than using the aggregate as a shared gate.

The focused boundary does not claim dune structure placement or Pyramid
mutation. `DunesBiome.Place`, pyramid chance resolution, candidate retry
selection (jungle/center/snow vetoes), structure collision handling, and
aggregate ordering remain deferred. The candidate eligibility and active-sand
surface scan are recorded separately in
`2026-08-31-worldgen-pyramid-candidate-boundary.md`.

The Dunes coordinate retry loop now has a separate narrow owner,
`LegacyDunesCandidatePolicy`. It preserves the source `RandomWorldPoint(0, 500,
0, 500)` draw order (X then Y), the scaled jungle-origin exclusion, fixed
300-tile center exclusion, snow-origin range with 300-tile padding, and the
source retry thresholds that disable the jungle veto at `attempt >= width` and
the snow veto at `attempt >= width * 2`. The selector returns only the random
origin and attempt count; `DunesBiome.Place` and all structure writes remain
outside this boundary. See
`2026-08-31-worldgen-dunes-candidate-retry-boundary.md` for the source contract.

## Verification

- `Build/diagnostics/server-ecs-convergence/P9-worldgen/dunes-pyramid-candidate-20260831-01/simulation-build-final.log`
  — Simulation Release build, exit 0, no warnings or errors.
- `Build/diagnostics/server-ecs-convergence/P9-worldgen/dunes-pyramid-candidate-20260831-01/pyramid-scan-independent-gates-green-rerun.log`
  — focused underworld regression plus independent-gate and Pyramid scan
  assertions, exit 0.
- `Build/diagnostics/server-ecs-convergence/P9-worldgen/dunes-pyramid-candidate-20260831-01/tunnels-regression.log`
  — existing Tunnels/WorldGen boundary regression, exit 0.
- `Build/diagnostics/server-ecs-convergence/P9-worldgen/dunes-pyramid-candidate-20260831-01/dunes-candidate-retry-determinism-green.log`
  — focused Dunes retry thresholds, coordinate bounds, random draw count, and
  deterministic replay assertions, exit 0.
