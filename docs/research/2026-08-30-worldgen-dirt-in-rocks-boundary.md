# WorldGen DirtInRocks boundary research

**Date:** 2026-08-30

**Owner:** NLTX WorldGen migration / P9 pass-specific batch
**Decision:** `DirtInRocks = verified` for the captured seed-1456 default profile; aggregate
`WorldGen` parity remains `partial/blocked`.

## Source boundary

The selected Version4 source block is
`Build/worldgen-oracle/legacy-instrumented-source/Terraria/WorldGen.cs:12321-12353`.
The pass is guarded by `!Skyblock.denyAllGeneration`. Its base loop uses density `0.005`,
which produces `25,200` invocations for the `4200x1200` default profile. Each invocation
draws, in order, X from `[0, maxTilesX)`, Y from `[(int)rockLayerLow, maxTilesY)`, strength
from `[2, 6)`, and steps from `[2, 40)`, then calls `TileRunner` with target type `0`.

The typed recipe preserves the source flags and ranges:

| Field | Source contract |
| --- | --- |
| Pass | `DirtInRocks` |
| Tile type | `0` |
| Add tile | `false` |
| Strength | `[2, 6)` |
| Steps | `[2, 40)` |
| Y range | `rockLayerLow..maxTilesY` |
| Density | `0.005` |
| Random reset | world seed before the pass |

The post-loop Remix branch is separate: every column draws `genRand.Next(-1, 3)`, starts at
`(int)Main.worldSurface + draw`, and toggles active type `0` to `1` or active type `1` to `0`
through the remaining rows. The typed owner keeps this as source-attributed commands and the
focused verifier checks its draw count and toggles; the captured oracle configuration is
non-Remix, so full Remix oracle parity is not claimed.

## Oracle and RED evidence

The instrumented legacy run is under
`Build/diagnostics/server-ecs-convergence/P9-worldgen/dirt-in-rocks-boundary-20260830-02/`.
It observed the Dirt In Rocks stage, recorded `25,200` invocation lines, and consumed
`3,724,072` random samples from a reset seed-1456 stream. The random trace starts at peek
`810676643` and ends at peek/RandNext `1756760189`. The Dirt In Rocks stage fingerprint is
`EA5E3F9B96938A65838A22097564594CB3CF64F7ADDF02564DC3DB881A0A9B2D`.

The immutable Rocks In Dirt input snapshot is
`legacy-mount-caves-snapshot-Rocks-In-Dirt.bin` (55,440,034 bytes,
SHA-256 `EB8D62DFF9E8B4F6FE1FB39B57C86FEB6358F6EF98FCC9D78E099EDA68A14198`). The downstream
oracle snapshot is `legacy-mount-caves-snapshot-Dirt-In-Rocks.bin` (55,440,034 bytes,
SHA-256 `5DD4904442EA066C7F40103E4614F376CA7B0AF287373B0D3A9D43591866E701`). The wrapper
intentionally stops after the stage, so `ExitCode=-1`, `StageObserved=true`, and
`StoppedAfterStage=true` are expected rather than oracle failures.

The initial focused RED artifact is
`Build/diagnostics/server-ecs-convergence/P9-worldgen/dirt-in-rocks-boundary-20260830-01/focused-red-run.log`;
before the owner existed it failed at the intended assertion because no source-backed
invocations were emitted. The first owner comparison in
`.../dirt-in-rocks-boundary-20260830-04/` exposed `610` type mismatches (`0->53`) and no
other tile-field mismatches. Diagnostic candidate traces in
`.../dirt-in-rocks-boundary-20260830-07/` showed the cause: the legacy `TileRunner` predicate
preserves type `53` while `l < Main.worldSurface`, and the runtime values are
`GenVars.worldSurface=229` versus `Main.worldSurface=325`.

`TerrainPass.cs:212` computes `Main.worldSurface` as `(int)(worldSurfaceHigh + 25.0)`. Passing
the profile's `229` value into the preservation policy therefore overwrote 610 type-53 tiles
in rows `y=311..324`. `LegacyMainWorldSurfacePolicy.Resolve` now supplies the computed `325`
value to both the base TileRunner policy and the Remix start-row calculation.

## Typed Simulation owner

`LegacyDirtInRocksPassDefinition` owns the constants, recipe, density/cardinality calculation,
and Remix offset range. `LegacyDirtInRocksPass` consumes an immutable `WorldGridSnapshot`, a
validated `LegacyTerrainRuntimeProfile`, and a pass-reset `LegacyPassRandomState`. It advances
the generation state to `Cave`, emits only source-attributed `TileChangeCommand` values, and
keeps a projected-tile dictionary so later invocations and the Remix scan observe earlier
uncommitted writes without mutating the input snapshot. `WorldGenerationPipeline` commits the
pass after the RocksInDirt snapshot through the existing deterministic tile commit boundary.

The owner does not add legacy `Main`, `GenVars`, networking, host, or oracle references to
Simulation. The generic `LegacyTileRunnerTraversal` remains shared infrastructure; this batch
does not claim that every TileRunner mode is complete.

## Verification

The focused verifier command is:

```powershell
dotnet Build/bin/Terraria.Dome.WorldGeneration.Verification/Release/net10.0/Terraria.Dome.WorldGeneration.Verification.dll `
  --dirt-in-rocks-only
```

The final focused run exits `0` and reports `839` bounded base commands. It verifies the source
recipe, `25,200`/`150` invocation cardinalities, draw ranges/order, immutable input,
deterministic replay, source attribution, atomic commit, Skyblock no-op behavior, Remix's
one-sample-per-column toggle contract, and the type-53 preservation regression fixture. That
fixture resolves `Main.worldSurface=65`, seeds type-53 tiles in `y=50..64`, and rejects any
type-0 overwrite in that band.

The fresh default-profile probe is
`Build/diagnostics/server-ecs-convergence/P9-worldgen/dirt-in-rocks-boundary-20260830-08/dirt-in-rocks-boundary-green.json`.
It records `25,200` expected invocations, `3,724,072` simulation random samples,
`515,925` emitted/applied commands, `Stage=Cave`, and zero mismatches for active, type,
liquid amount/type, frame X/Y, and wall fields. Its decision is
`pass-specific-dirt-in-rocks-matched`.

Fresh reproducibility logs in the same directory are:

- `simulation-build.log` — Release Simulation build, exit `0`, 0 warnings, 0 errors.
- `worldgeneration-verifier-build.log` — Release verifier build, exit `0`, 0 warnings,
  0 errors.
- `focused-verifier-run.log` and `focused-verifier-run.stderr.log` — focused run exit `0`;
  stderr is empty.
- `full-verifier-run.log` and `full-verifier-run.stderr.log` — complete bounded verifier exit
  `0`, `280` `PASS`, `40` bounded `CHECK`, `0` `FAIL`/`ERROR`, stderr empty.

## Explicit limits and deletion gate

This is a bounded pass-specific result, not aggregate cave or world parity. Complete TileRunner
semantics, aggregate cave ordering, upstream Mount/Tunnels behavior, full Clay Remix/aggregate
parity and remaining WorldGen passes, full Remix oracle parity, extended state/side effects, command-sequence parity,
and full WLD differential remain deferred. The repository baseline remains `5,040,000` compared
tiles, `3,190,404` tile mismatches, and `1,046,843` extended-state mismatches.

`canRemoveLegacyWorldGen` remains `false`; legacy `WorldGen.cs` is not deleted, and all `44`
deferred `ServerRelevant` physical-deletion rows remain unchanged.
