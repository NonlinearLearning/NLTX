# WorldGen RocksInDirt boundary research

**Date:** 2026-08-30  
**Owner:** NLTX WorldGen migration / P9 pass-specific batch  
**Decision:** `RocksInDirt = verified` for the captured default profile; aggregate `WorldGen`
parity remains `partial/blocked`.

## Source boundary

The fresh instrumented Version4 source is
`Build/worldgen-oracle/legacy-instrumented-source/Terraria/WorldGen.cs:12234-12263`.
The pass is guarded by `!Skyblock.denyAllGeneration`; the default profile does not deny
generation. The source keeps one continuous `genRand` stream for the pass and emits three
families of `TileRunner` calls in the order below:

| Family | Source Y range | Density | Strength | Steps | Tile type |
| --- | --- | ---: | --- | --- | ---: |
| `surface-dirt` | `0..(int)worldSurfaceLow` inclusive | `0.00015` | `[4, 15)` | `[5, 40)` | `1` |
| `surface-high-dirt` | `(int)worldSurfaceLow..(int)worldSurfaceHigh` inclusive | `0.0002` | `[4, 10)` | `[5, 30)` | `1` |
| `rock-high-dirt` | `(int)worldSurfaceHigh..(int)rockLayerHigh` inclusive | `0.0045` | `[2, 7)` | `[2, 23)` | `1` |

Each invocation draws X from `[0, maxTilesX)`. In the second family only, the source checks
`Main.tile[x, y - 10].active()` and performs one Y re-roll when that offset tile is inactive;
the re-roll occurs before strength and step draws. The typed owner uses a safe lower bound for
the offset lookup on synthetic profiles, while preserving the source's one-shot branch for
normal `y >= 10` calls.

For the default `4200x1200`, seed-1456 oracle, the three loop counts sum to `24444`. The
instrumented random trace records a reset sample count of `0`, initial peek `810676643`, and
end sample count `3436063` with end peek/RandNext `27042696`.

## Oracle and RED evidence

The fresh oracle capture is under
`Build/diagnostics/server-ecs-convergence/P9-worldgen/rocks-in-dirt-boundary-20260830-12/`:

- `legacy-stage-trace.jsonl` records `Terrain`, `Mount Caves`, `Dirt Wall Backgrounds`, and
  `Rocks In Dirt`.
- `legacy-mount-caves-snapshot-Dirt-Wall-Backgrounds.bin` is the immutable upstream input;
  SHA-256 is `934141E9B5F4FEFD7EC74FCCD98CEAA070071C743C81B59C44B02FD928F96832`.
- `legacy-mount-caves-snapshot-Rocks-In-Dirt.bin` is the downstream oracle;
  SHA-256 is `EB8D62DFF9E8B4F6FE1FB39B57C86FEB6358F6EF98FCC9D78E099EDA68A14198`.
- `legacy-random-trace.jsonl` records the pass random checkpoints above.

The RED comparison in
`Build/diagnostics/server-ecs-convergence/P9-worldgen/rocks-in-dirt-boundary-20260830-01/`
loads the Dirt Wall snapshot and intentionally performs no RocksInDirt production work. It
compares `5,040,000` tiles, reports `285,014` type mismatches, and reports zero active, liquid,
frame, and wall mismatches; the first divergence is `(1,66)`. Its expected exit code is `2`.

## Typed Simulation owner

`LegacyRocksInDirtPassDefinition` owns the immutable three-recipe and three-loop projections,
including density/count calculation and the source ranges. `LegacyRocksInDirtPass` consumes a
`WorldGridSnapshot`, validated `LegacyTerrainRuntimeProfile`, and `LegacyPassRandomState`. It
advances the typed generation state to `WorldGenerationStage.Cave`, emits only
source-attributed `TileChangeCommand` values, and leaves the input snapshot unchanged.

`LegacyTileRunnerTraversal` owns the bounded envelope walk and candidate/override queries. A
shared projected-tile dictionary lets later invocations in this pass observe earlier uncommitted
tile projections without mutating the immutable snapshot. `WorldGenerationPipeline` commits the
Dirt Wall Backgrounds batch first, then appends and commits RocksInDirt commands through
`TileChangeCommitSystem`; `LegacyTileRunnerCommandCommitBoundary` is covered as the batch-level
atomic boundary.

## Verification

The focused verifier entry point is:

```powershell
dotnet build Test/Terraria.Dome.WorldGeneration.Verification/Terraria.Dome.WorldGeneration.Verification.csproj `
  -c Release -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -m:1 --no-restore

dotnet run --project Test/Terraria.Dome.WorldGeneration.Verification/Terraria.Dome.WorldGeneration.Verification.csproj `
  -c Release --no-build -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -m:1 -- `
  --rocks-in-dirt-only
```

Fresh pass-specific evidence is in
`Build/diagnostics/server-ecs-convergence/P9-worldgen/rocks-in-dirt-boundary-20260830-16/` and
`Build/diagnostics/server-ecs-convergence/P9-worldgen/rocks-in-dirt-boundary-20260830-17/`;
the fresh rerun is under
`Build/diagnostics/server-ecs-convergence/P9-worldgen/rocks-in-dirt-boundary-20260830-18/`:

- The focused `--rocks-in-dirt-only` verifier exits `0` on the current bounded fixture and emits
  `2916` commands. It verifies recipes, cardinalities, reroll ordering, source attribution,
  immutable input, deterministic replay, commit isolation, and the Skyblock no-op guard. The
  earlier `2245`-command `200x150` run is retained as a historical artifact only.
- The focused GREEN artifact compares the `4200x1200`, seed-1456 oracle and reports `285014`
  commands, `285014` applied commands, `Attributed = true`, and zero active/type/liquid/frame/
  wall mismatches.
- The Simulation and WorldGeneration verification Release builds exit `0` with `0` warnings and
  `0` errors; the full WorldGeneration verifier exits `0`.
- The earlier `231275` command / `53739` type-mismatch result came from the pre-projected
  traversal. The corrected traversal uses the invocation's original `strength` in the legacy
  distance predicate; the final root probe now records `24444` matching invocations and a
  zero-mismatch tile snapshot. Generic TileRunner modes outside this source boundary remain
  unverified.

An isolated root probe was also rebuilt with current sources and compared against the same oracle
snapshots in
`Build/diagnostics/server-ecs-convergence/P9-worldgen/rocks-in-dirt-boundary-20260830-12/`
(invocation and candidate traces) and
`Build/diagnostics/server-ecs-convergence/P9-worldgen/rocks-in-dirt-boundary-20260830-16/`.
It records `24444` invocation records with zero draw/sample mismatches, the oracle random sample
count `3436063`, and a zero-mismatch tile snapshot after commit (`285014` commands). This
positive result is still pass-specific: it does not exercise every TileRunner mode or every
WorldGen pass, and it does not cover extended tile side effects or command-sequence equivalence.

## Explicit limits and deletion gate

This batch does not claim `Mount Caves`, `Tunnels`, `DirtInRocks`, `Clay`, complete cave ordering,
non-cave generation, full WLD differential parity, or complete legacy `WorldGen.cs` replacement.
Generic TileRunner semantics, extended tile state, side effects, multi-profile behavior, and
global random scheduling remain deferred. `canRemoveLegacyWorldGen` remains `false`, and all
44 deferred `ServerRelevant` physical-deletion rows remain unchanged.

The repository-wide status is therefore still:

```text
WorldGen RocksInDirt boundary: verified (captured default profile)
NPC field/property migration: partial
WorldGen oracle parity: blocked/deferred
canRemoveLegacyWorldGen: false
```

The unresolved aggregate baseline still compares `5,040,000` tiles with `3,190,404` tile
mismatches and `1,046,843` extended-state mismatches; the 44 `ServerRelevant` physical-deletion
rows remain deferred.
