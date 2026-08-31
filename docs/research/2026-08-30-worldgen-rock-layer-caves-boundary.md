# WorldGen `RockLayerCaves` boundary research

## Result

The non-Remix base loop of Version4 `GenPassNameID.RockLayerCaves` has a typed,
pass-specific owner in `Terraria.Dome.Simulation`. The accepted scope is the source-backed
invocation contract and snapshot-to-command boundary; it is not a claim of complete
`TileRunner` or aggregate WorldGen parity.

## Source evidence

- Source: `Build/worldgen-oracle/legacy-instrumented-source/Terraria/WorldGen.cs`.
- Current source SHA-256: `C7C2F2196CEA0F6E56C20863A27824917391B74B34232D50590D1DDF4FF321AC`.
- Pass body: lines `12606-12649`; the captured contract envelope is `12605-12660` and records
  excerpt hash `781868ace1ac8cb723f8d804016615602221544f82528e4941bb124005f171ad`, source
  provenance, and the non-Remix scope in
  `Build/diagnostics/server-ecs-convergence/P9-worldgen/rock-layer-caves-boundary-20260830-05/rock-layer-caves-source-contract-20260830.json`.
- Guard: `!Skyblock.denyAllGeneration`.
- Base count: `(int)(Main.maxTilesX * Main.maxTilesY * 0.00013)`, which is `655` for
  `4200 x 1200`. Remix scales this base count by `1.1`; the additional `0.00013 * 0.4`
  paired no-Y-change loop is outside this boundary.
- Per-invocation draw order: `Next(10)` selects `-2` only for result `0`, otherwise `-1`;
  `Next(6, 20)` draws strength; `Next(50, 300)` draws steps; `Next(0, maxTilesX)` draws X;
  and `Next((int)rockLayerHigh, maxTilesY)` draws Y. The ordinary call uses
  `addTile: false`, `speedX: 0`, `speedY: 0`, `noYChange: false`, `overRide: true`, and
  `ignoreTileType: -1`.

The valid oracle capture is
`Build/diagnostics/server-ecs-convergence/P9-worldgen/rock-layer-caves-boundary-20260830-05/`.
It contains seven stage rows through Rock Layer Caves, the Clay input snapshot, the Rock Layer
Caves output snapshot, `655` invocation rows, and the pass-reset random trace. For seed `1456`,
the stream starts at sample count `0` with peek `810676643` and ends at sample count `6945153`
with peek/RandNext `647554835`. The wrapper exit `-1` is expected because the collection script
stops the legacy process immediately after the selected stage snapshot.

## Typed owner and integration

- Definition: `src/Terraria.Dome.Simulation/WorldGeneration/LegacyRockLayerCavesPassDefinition.cs`.
- Owner: `src/Terraria.Dome.Simulation/WorldGeneration/LegacyRockLayerCavesPass.cs`.
- Shared traversal: `src/Terraria.Dome.Simulation/WorldGeneration/LegacyTileRunnerTraversal.cs`.
- Pipeline integration: `src/Terraria.Dome.Simulation/WorldGeneration/WorldGenerationPipeline.cs`.
- Generic fallback exclusion:
  `src/Terraria.Dome.Simulation/WorldGeneration/Systems/LegacyCavePassSystem.cs`.

The owner validates the terrain profile, no-ops before random/state mutation for Skyblock and an
invalid high-rock-layer range, resets a pass-scoped `LegacyPassRandomState` from the world seed,
and consumes an immutable `WorldGridSnapshot`. Projected writes are held separately from the
input snapshot. Commands are source-attributed as
`worldgen.cave.RockLayerCaves.rock-layer` and are committed through the existing deterministic
tile commit system. A profile-enabled request skips the generic RockLayer recipe so the dedicated
owner runs once after the committed Clay snapshot.

For negative TileRunner types, the legacy method clears only the tile's active bit and leaves its
type, frame, wall, and other tile state intact. The command contract now carries
`PreserveTileState`; the projection uses it for this bounded negative-type path. Already-inactive
candidates emit no command, and active type `53` remains excluded by the source branch. The `-2`
liquid/lava writes are intentionally not claimed here and remain deferred.

## Oracle differential

The pass-specific probe is
`Build/diagnostics/server-ecs-convergence/P9-worldgen/rock-layer-caves-boundary-20260830-07/rock-layer-caves-boundary-green.json`.
It loads the immutable Clay snapshot, runs the typed owner with the same seed, commits the typed
commands, and compares active/type/liquid amount/liquid type/frame X/frame Y/wall against the
legacy Rock Layer Caves snapshot.

The probe build exits `0`; the differential run exits `2` by design because the shared traversal
is not full parity. This is a diagnostic partial result, not a failed build or a parity gate.

The run consumed exactly `6945153` random samples and emitted/applied `526468` source-attributed
commands. After preserving negative-type tile state, the field comparison is:

| Field | Mismatches |
| --- | ---: |
| Active | 774708 |
| Type | 0 |
| Liquid amount | 191078 |
| Liquid type | 84463 |
| Frame X | 0 |
| Frame Y | 0 |
| Wall | 0 |
| Compared tiles | 5040000 |

The decision is `partial-rock-layer-caves-traversal-mismatch`. The remaining active/liquid
differences are evidence of the known shared `TileRunner` traversal and liquid side-effect gaps;
they do not authorize an aggregate parity or deletion claim.

## Verification

- Focused RED evidence: `Build/diagnostics/server-ecs-convergence/P9-worldgen/rock-layer-caves-boundary-20260830-01/`.
- Focused GREEN: `dotnet ...Verification.dll --rock-layer-caves-only`, exit `0`.
- Fresh serial Simulation Release build: exit `0`, zero warnings/errors.
- Fresh serial WorldGeneration verifier Release build: exit `0`, zero warnings/errors.
- Fresh focused verifier: exit `0`, one command from the current `800x300` fixture.
- Fresh complete bounded verifier: exit `0`, `280` `PASS`, `40` bounded `CHECK`, and no
  `FAIL`/`ERROR` lines.
- Fresh command and run logs are in
  `Build/diagnostics/server-ecs-convergence/P9-worldgen/rock-layer-caves-boundary-20260830-08/`:
  `simulation-build.log`, `verifier-build.log`, `rock-layer-caves-focused.log`, and
  `worldgeneration-verifier.log`, each with its corresponding exit-code file.

## Deferred boundaries

This is `completed_partial` and remains bounded to the non-Remix base loop. The following remain
open: Remix's additional loop and paired no-Y-change calls; `-2` liquid/lava side effects;
complete `TileRunner` drift, candidate, override, and state semantics; aggregate cave ordering;
full WLD/extended-state differential; legacy `WorldGen.cs` deletion;
`canRemoveLegacyWorldGen`; and the `44` deferred `ServerRelevant` physical-deletion rows.
