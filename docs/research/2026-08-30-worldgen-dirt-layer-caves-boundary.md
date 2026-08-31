# WorldGen DirtLayerCaves boundary research

**Date:** 2026-08-30
**Owner:** NLTX WorldGen migration / P9 pass-specific batch
**Decision:** `DirtLayerCaves = completed_partial`; aggregate `WorldGen` parity remains blocked.

## Source evidence

The historical instrumented Version4 source capture is
`Build/worldgen-oracle/legacy-instrumented-source/Terraria/WorldGen.cs:12406-12446` with
SHA-256 `2460C020FF86C660C6AE4FC762D7CCACCB1A14C3EEED057560BDFD3607B6ED6A`, captured at
`2026-08-30T14:12:17+08:00`. The source file is mutable and has since been observed stable at
`2,276,185` bytes and `88,582` lines, with latest full-file SHA-256
`37374274D836785E33370F3CD9DD2A733D048930DCE65689A9DC69FD1E052FF3`; the current DirtLayer
block is at lines `12492-12532` (an `86`-line shift). Prior current samples were
`AB912157D8D9C5C738CACE7EF3863F04BBD2427A535B89C2851ECBB10B76F27B` (`2,275,130` bytes,
`88,537` lines) and
`90CDCBFBEF663C43AEAB8DA3C762C9FE8AEBAABC0736FD7B69BEB1D27D78D4E9` (`2,276,107` bytes,
`88,578` lines); they changed by `+977` bytes/`+41` lines and then `+78` bytes/`+4` lines while
this block remained unchanged. No mutable sample replaces the historical capture. The current
block is frozen separately in
`Build/diagnostics/server-ecs-convergence/P9-worldgen/dirt-layer-caves-20260830-01/current-dirt-layer-caves-source-excerpt-20260830.txt`
with SHA-256 `B848DD46AB05C1A131C321F34AA6B651A29FE7A98DF6C04F49A9AD2F05C2791E` (trailing LF).
Its recorded contract fields match the current block, but a byte-identical historical source
snapshot is unavailable; the historical hash remains authoritative for the existing oracle
evidence and is not replaced. The full comparison is recorded in
`current-source-provenance-20260830.json`, `current-source-provenance-20260830-02.json`, and
`current-source-provenance-20260830-03.json`.
The pass is guarded by `!Skyblock.denyAllGeneration`. Its default loop count is
`(int)(Main.maxTilesX * Main.maxTilesY * 3E-05)`, doubled for Remix; this is `151` invocations
for a `4200x1200` world and `302` for Remix.

Each invocation selects `type = -1`, or `-2` when `genRand.Next(6) == 0`, draws X from
`[0, maxTilesX)`, and draws Y from
`[(int)worldSurfaceLow, (int)rockLayerHigh + 1)`. The retry predicate rejects beach-margin
coordinates below `worldSurfaceHigh` and the inclusive `0.45..0.55` spawn-center band below
`Main.worldSurface`, with `smallHolesBeachAvoidance = 340`. Strength is `Next(5, 15)` and steps
are `Next(30, 200)`; Remix scales them by `1.1` and `1.9` and truncates to `int`. TileRunner
defaults are `addTile=false`, `speedX=0`, `speedY=0`, `noYChange=false`, `overwrite=true`, and
`ignoreTileType=-1`.

The source random sequence is now explicit in the typed owner: type selection, candidate X/Y,
each retry X/Y pair, strength, then steps. `LegacyTileRunnerPassInvocation.RandomDrawCount`
records `5 + retryCount * 2`, and the focused verifier checks the actual invocation fields against
an independently advanced `LegacyPassRandomState`.

## Fresh oracle trace

Evidence is under
`Build/diagnostics/server-ecs-convergence/P9-worldgen/dirt-layer-caves-20260830-01/`:

- `legacy-stage-trace.jsonl` records Terrain, Mount Caves, and Dirt Layer Caves. The Dirt Layer
  fingerprint is `7095AC749B33FC7C58976CF169A4FDA7AD08FEF0A30BB92A90000667AEA5893D`.
- `legacy-random-trace.jsonl` records a reset sample count of `0`, shared initial peek
  `810676643`, and Dirt Layer Caves end sample count `576249` with end peek and `RandNext`
  `1359241517`.
- `legacy-mount-caves-snapshot.bin` is the typed Mount-stage snapshot boundary (`55,440,032`
  bytes; SHA-256 `F420D672888D18AECC57E06CC7753E06F7F086D96FC8B6E5400C02CFE195077D`).
- `dirt-layer-caves-source-contract-20260830.json` is the machine-readable source and status
  contract; `audit.log` records the replay, hashes, RED/GREEN evidence, and deferred boundaries.

## Typed implementation

`LegacyDirtLayerCavesPassDefinition` owns the extracted constants, count calculation, `-1/-2`
selection, and Remix scaling. `LegacyDirtLayerCavesPass` consumes an immutable
`WorldGridSnapshot` and validated `LegacyTerrainRuntimeProfile`, applies the exact candidate
retry predicate, and emits `TileChangeCommand` values with source
`worldgen.cave.DirtLayerCaves.dirt-layer`. The existing `TileChangeCommitSystem` remains the
deterministic commit boundary. `LegacyCavePassSystem` uses this owner only when the world width is
larger than both 340-column beach margins; small synthetic fixtures continue using the existing
approximate recipes rather than violating the source guard's geometry assumptions.

## Verification

The focused command:

```text
dotnet run --project Test/Terraria.Dome.WorldGeneration.Verification/Terraria.Dome.WorldGeneration.Verification.csproj --no-build -- --dirt-layer-caves-only
```

passed with `3046` commands. It verifies constants and 151/302 cardinality, candidate boundaries,
type selection, Remix scaling, immutable snapshot preservation, deterministic command replay,
source attribution, command bounds, commit isolation, and the Skyblock no-op guard. The complete
WorldGeneration verifier was rerun from the rebuilt DLL with exit `0`, `280` PASS lines, no
anchored `FAIL`/`ERROR` lines, and an empty stderr log; this is regression evidence only. The
fresh outputs are `dirt-layer-caves-focused-20260830-04.stdout.log` and
`worldgeneration-verifier-full-20260830-05.stdout.log`.

## Explicit limits

This is a source-backed typed slice, not complete parity. `LegacyTileRunnerTraversal` still lacks
full legacy Remix/internal random behavior, complete `type=-2` water/lava/ocean-depth side effects,
frame-important and wall/grass/network side effects, and aggregate pass ordering. Full WLD
differential remains blocked, `canRemoveLegacyWorldGen = false`, and the `44` deferred
`ServerRelevant` rows remain deferred.
