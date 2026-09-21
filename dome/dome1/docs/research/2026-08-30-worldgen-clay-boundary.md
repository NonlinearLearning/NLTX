# WorldGen Clay boundary research

**Date:** 2026-08-30  
**Owner:** NLTX WorldGen migration / P9 pass-specific batch  
**Decision:** `Clay = verified` for the captured seed-1456 default profile; aggregate `WorldGen`
parity remains `partial/blocked`.

## Source boundary

The selected Version4 source block is
`Build/worldgen-oracle/legacy-instrumented-source/Terraria/WorldGen.cs:12383-12503`.
The pass is guarded by `!Skyblock.denyAllGeneration` and uses target tile type `40`. The source
executes four continuous `TileRunner` families in this order:

| Family | Source Y range | Density | Strength | Steps |
| --- | --- | ---: | --- | --- |
| `surface-low-clay` | `0..(int)worldSurfaceLow` | `2E-05` | `[4, 14)` | `[10, 50)` |
| `remix-clay` | `(int)rockLayer - 25..maxTilesY - 350` | `7E-05` | `[8, 15)` | `[5, 50)` |
| `surface-high-clay` | `(int)worldSurfaceLow..(int)worldSurfaceHigh + 1` | `5E-05` | `[8, 14)` | `[15, 45)` |
| `rock-high-clay` | `(int)worldSurfaceHigh..(int)rockLayerHigh + 1` | `2E-05` | `[8, 15)` | `[5, 50)` |

The non-Remix path runs the first, third, and fourth families. For a `4200x1200` world their
floor-truncated invocation counts are `100`, `252`, and `100`, for `452` total invocations.
The Remix path replaces the two normal high families with `352` `remix-clay` invocations. Each
invocation draws X, Y, strength, and steps in that order from a pass-reset random stream.

After the runners, the source scans columns `5 .. maxTilesX - 5` and rows
`1 .. Main.worldSurface - 2`. It stops at the first active tile in each column, then converts up
to five type-40 tiles beginning at that row to type `0`. The typed owner resolves the legacy
`Main.worldSurface` projection with `worldSurfaceHigh + 25`, rather than using the
`GenVars.worldSurface` value used by the Y ranges.

## Oracle and RED evidence

The fresh instrumented legacy run is under
`Build/diagnostics/server-ecs-convergence/P9-worldgen/clay-boundary-20260830-01/`:

- `legacy-stage-trace.jsonl` observes `Terrain`, `Mount Caves`, `Dirt Wall Backgrounds`,
  `Rocks In Dirt`, `Dirt In Rocks`, and `Clay`.
- `legacy-mount-caves-snapshot-Dirt-In-Rocks.bin` is the immutable upstream input. It is
  55,440,034 bytes with SHA-256
  `5DD4904442EA066C7F40103E4614F376CA7B0AF287373B0D3A9D43591866E701`.
- `legacy-mount-caves-snapshot-Clay.bin` is the downstream Clay snapshot. It is 55,440,025
  bytes with SHA-256 `C39DC98ECB82C22248FE0B245755C7CCF29E299C2D8CAAE1032F93099465AD2C`.
- `legacy-clay-invocations.jsonl` records `452` non-Remix invocation rows, split as `100`,
  `252`, and `100` by family. `legacy-random-trace.jsonl` records `522,600` samples between
  the Clay start and end checkpoints. `legacy-clay-cleanup.jsonl` records `4,190` column rows,
  including `1,044` changed tiles.
- The wrapper intentionally stops after the selected stage. Its `ExitCode=-1`,
  `StageObserved=true`, and `StoppedAfterStage=true` are expected capture results.

The pre-owner focused verifier run is retained in
`Build/diagnostics/server-ecs-convergence/P9-worldgen/clay-boundary-20260830-01/focused-red-run.log`.
It failed at the intended boundary assertion because the current pipeline had no Clay owner or
Clay output.

## Typed Simulation owner

`LegacyClayPassDefinition` owns the source recipes, densities, target type, and floor-based
cardinality calculation. `LegacyClayPass` consumes an immutable `WorldGridSnapshot`, a validated
`LegacyTerrainRuntimeProfile`, and a pass-reset `LegacyPassRandomState`. It emits only
source-attributed `TileChangeCommand` values, keeps projected writes in a private dictionary so
later runners and cleanup observe uncommitted mutations, and gives cleanup commands priority `1`
so they commit after runner commands. The Skyblock deny-generation guard is a no-op that does not
consume random samples or mutate generation state.

`WorldGenerationPipeline` commits Clay after the typed DirtInRocks boundary and before the Cave
stage capture when a runtime terrain profile is supplied. The owner has no legacy `Main`,
`GenVars`, networking, host, save, or oracle dependency.

## Verification and differential result

The focused verifier command is:

```powershell
dotnet run --project Test/Terraria.Dome.WorldGeneration.Verification/Terraria.Dome.WorldGeneration.Verification.csproj `
  -c Release --no-build -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -- --clay-only
```

It exits `0` and verifies the recipe/range contract, floor counts, random draw shape, immutable
input, deterministic replay, source attribution, first-active/five-row cleanup, cleanup priority,
atomic commit, Skyblock no-op, and the separate Remix branch.

The production-vs-oracle snapshot probe is
`Build/diagnostics/server-ecs-convergence/P9-worldgen/clay-boundary-20260830-03/
clay-boundary-green.json`. It loads the DirtInRocks input snapshot, executes the production owner,
commits the typed batch, and compares all `4200x1200` tiles against the Clay snapshot. The result
records:

- `63,216` emitted commands and `63,216` applied commands;
- `522,600` simulation random samples, matching the oracle checkpoint;
- `1,044` cleanup commands and source/sequence ranges for every command family;
- zero active, type, liquid amount/type, frame X/Y, and wall mismatches; and
- no first-mismatch coordinates (`FirstMismatches=[]`).

The fresh serial Release Simulation build and WorldGeneration verifier build both exit `0` with
zero warnings and errors; their logs are `clay-boundary-20260830-03/simulation-build-final.log`
and `clay-boundary-20260830-03/worldgeneration-verifier-build-final.log`. The fresh Clay probe
build and run also exit `0`, with an empty `clay-probe-run.stderr.log`; the complete bounded
WorldGeneration verifier exits `0` with an empty stderr log. These are bounded and pass-specific
results; they do not establish aggregate cave ordering or full WorldGen parity.

## Explicit limits and deletion gate

This result does not claim full Remix oracle parity, complete TileRunner semantics, upstream
Mount/Tunnels parity, aggregate cave ordering, extended tile side effects, command-sequence
equivalence, full WLD differential parity, or compatibility with all world profiles. The repository
baseline remains `5,040,000` compared tiles, `3,190,404` tile mismatches, and `1,046,843`
extended-state mismatches. `canRemoveLegacyWorldGen` remains `false`; legacy `WorldGen.cs` is not
deleted, and all `44` deferred `ServerRelevant` physical-deletion rows remain unchanged.
