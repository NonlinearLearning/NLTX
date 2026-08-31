# WorldGen Pyramid wall-frame evaluation boundary

## Source authority

The current instrumented legacy source is
`Build/worldgen-oracle/legacy-instrumented-source/Terraria/Framing.cs` with SHA-256
`93EDE9CEC249AC2E30FA580BBA35FA9E1311AE872912BB186403E9B0FC85F97A`. The normalized inclusive
`Framing.cs:350-434` excerpt has SHA-256
`62EDDB7858469EBF14FB7A7CED64AF8D9E014401C0A30E9436997F049F59864F`. The Pyramid caller is
`WorldGen.cs:28425`; its already-owned request list invokes this method with `resetFrame=true`.

`Framing.Initialize` at lines `111-150` supplies the phlebas, lazure, center-offset, and
20-by-4 wall-frame lookup tables. `Main.wallLargeFrames` is initialized and populated at
`Main.cs:1469`, `9861-10048`; the typed large-wall owner contains the exact Version4 assignments.
The source uses `WallID.Count=367` and `TileID.Sets.TruncatesWalls={54,328,459,748}`. The
client-side `Main.ShouldShowInvisibleBlocksAndWalls` delegates to SceneMetrics, so server
evaluation receives `showInvisibleWalls` explicitly rather than pretending to own client state.

## Typed owners

- `LegacyWallFrameLookupRegistry` owns immutable value copies of all source frame-number,
  center-offset, and wall-frame tables, with 36-pixel frame arithmetic.
- `LegacyWallFrameNeighborQuery` and `LegacyTruncatingWallTileRegistry` own cardinal mask and
  visibility semantics.
- `WorldTile.WallFrameNumber`, `WorldTile.WallFrameX`, and `WorldTile.WallFrameY` make the source's
  two-bit wall frame number and its two coordinate fields explicit in immutable snapshots. The
  existing `WorldTile.FrameX`/`FrameY` remain tile-object coordinates and are not written by
  `Framing.WallFrame`. Non-reset evaluation masks the number with `3`, matching the legacy tile
  accessor's two-bit storage semantics; reset and large-wall branches write a value in the same
  domain.
- `LegacyWallFrameEvaluationQuery` evaluates one interior target without mutating the snapshot.
  It preserves invalid/zero wall paint-coating cleanup, large-wall coordinate tables, ordinary
  reset draws, wall-21's second draw/forced value, full-mask center offsets, and final lookup;
  non-zero results write only the wall-frame fields and preserve tile-object frame coordinates.
- `LegacyPyramidWallFrameCommandProjection` evaluates ordered Pyramid requests into typed
  `LegacyWallFrameCommand` values with source metadata, duplicate order, expected section version,
  and a working framing-stage state. The request envelope is fail-closed: every center must leave a
  complete 3x3 neighborhood inside the world, every target must be a direct neighbor, and a
  post-framing generation state or empty batch cannot consume state or random samples.
- `LegacyWallFrameCommandCommitSystem` validates sequences, lookup-domain values, source metadata,
  mask/lookup consistency, section guards, and per-section version capacity before applying the
  resulting immutable tiles.

No global `Main.tile`, client SceneMetrics, pending mutation collection, or mutable lookup array is
read by the evaluator. The command projection is Pyramid-specific and does not change the existing
generic y-major `SquareWallFrameRequestQuery`.

## Verification

The focused `--wall-frame-evaluation-only` verifier covers ordinary wall `34` reset accounting,
independent tile-vs-wall frame preservation, all-four-neighbor center offsets,
truncating/invisible visibility, large walls `179` and `185`,
wall `21` random forcing, zero/invalid cleanup, two-bit non-reset normalization, edge no-ops,
ordered duplicate requests, empty and malformed request rejection, post-framing state rejection,
typed command publication, successful commit, mask/lookup mismatch rejection, stale section
rejection, and terminal sequence rejection. The prior `...-03` rerun is retained as a failed
diagnostic: its stale verifier fixture was rejected by a hard-coded tile-frame assertion even
though the commit result had no failure reason. The verifier now compares the committed center
tile with the final command result and separately asserts that the pre-existing tile `FrameX/Y`
are preserved. Fresh serial rebuild and focused evidence is recorded under
`Build/diagnostics/server-ecs-convergence/P9-worldgen/pyramid-wall-frame-value-mutation-20260831-01/`.
The focused Pyramid evaluation, neighbor, framing, and full verifiers exit `0`; the dependent
Simulation, Pyramid verifier, Server, Main inventory, and WorldGeneration verifier Release builds
exit `0` with zero warnings and errors, and the WorldGeneration bounded run reports `280 PASS`,
`40 CHECK`, and no anchored `FAIL`/`ERROR` lines. The post-build bounded WorldGeneration verifier
also exits `0` with `280` PASS, `40` CHECK, and no anchored `FAIL`/`ERROR` or
`MissingMethodException` lines.

## Status and deferred scope

Status: `completed_partial` for source-backed value evaluation and typed Pyramid wall-frame
mutation only. Pyramid tunnel direction and chest/pile/plant/pot features, full SquareWallFrame
pipeline integration, client SceneMetrics/map/network behavior, persistence/protocol serialization
of the ephemeral wall-frame coordinates, aggregate generation order, exact global RNG/checkpoint
parity, full WLD/extended-state differential parity, legacy `WorldGen.cs` deletion,
`canRemoveLegacyWorldGen=false`, and all 44 deferred `ServerRelevant` rows remain open.
