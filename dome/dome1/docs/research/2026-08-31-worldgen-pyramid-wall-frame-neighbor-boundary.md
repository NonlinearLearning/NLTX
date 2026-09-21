# WorldGen Pyramid wall-frame neighbor boundary

## Source authority

The current instrumented Version4 source is
`Build/worldgen-oracle/legacy-instrumented-source/Terraria/Framing.cs` with SHA-256
`93EDE9CEC249AC2E30FA580BBA35FA9E1311AE872912BB186403E9B0FC85F97A`. The `WallFrame` guard and
neighbor branch are at lines `350-402`. `WallID.Count` is `367` in
`Build/worldgen-oracle/legacy-instrumented-source/Terraria.ID/WallID.cs` (SHA-256
`B03BCF66A8C5D496FC8720FDB2C8AD918CEB4B6B4321AD874A3253A75A86B38C`), and
`TileID.Sets.TruncatesWalls` is the exact set `{54, 328, 459, 748}` in
`Terraria.ID/TileID.cs` (SHA-256
`686F5340E5C71F940BCB3C5E30AB9125D032BAFDE991A34129FAB26662528296`).

The source assigns above/left/right/below to bits `1/2/4/8`. A qualifying neighbor has a
positive wall or is active with a truncating tile type; invisible walls are omitted unless the
caller supplies the source-equivalent visibility value. An invalid or zero center wall takes the
no-frame branch before scanning neighbors, and an outer-border coordinate returns immediately.

## Typed owner

`LegacyTruncatingWallTileRegistry` owns the frozen four-entry set. `LegacyWallFrameNeighborMask`
expresses the four source bits, and `LegacyWallFrameNeighborQuery.TryEvaluate` accepts only an
immutable `WorldGridSnapshot` plus explicit `showInvisibleWalls`. It returns original/effective
center wall types, mask, visibility mode, and normalization state. The query does not read frame
fields, random state, pending mutations, or client `Main` state, and does not mutate the snapshot.

## Verification

The focused `--wall-neighbor-only` verifier records positive-wall, truncating-tile, inactive,
invisible, zero-center, invalid-center, border, and snapshot/state preservation checks. Fresh
focused and dependent WorldGeneration evidence is stored under
`Build/diagnostics/server-ecs-convergence/P9-worldgen/pyramid-wall-frame-neighbor-20260831-01/`
and the current-tree bounded rerun under the adjacent wall-frame evaluation evidence directory.
The stale pre-`WallFrameNumber` bounded artifact is not treated as green; a rebuilt verifier is
required before recording the bounded regression as current.

## Status and deferred scope

Status: `completed_partial` for neighbor classification only. The value lookup tables,
`wallFrameNumber`, wall-21 RNG, frame coordinate mutation, wall paint/coating cleanup, command
publication, Pyramid tunnel/features, client SceneMetrics parity, aggregate ordering, exact RNG/
WLD differential parity, legacy deletion, `canRemoveLegacyWorldGen=false`, and all 44 deferred
`ServerRelevant` rows remain outside this boundary.
