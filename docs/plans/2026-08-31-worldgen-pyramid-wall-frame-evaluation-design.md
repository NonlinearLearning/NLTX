# Pyramid Wall-Frame Evaluation Boundary Design

**Status:** approved for the resumed N6 P2 batch

**Scope:** source-backed `Framing.WallFrame` value evaluation and typed wall-frame command
publication for the already-owned Pyramid request list

## Source authority

The instrumented Version4 source is
`Build/worldgen-oracle/legacy-instrumented-source/Terraria/Framing.cs`. `Framing.WallFrame` is
at lines `350-434`. The Pyramid caller writes wall `34`, then invokes `SquareWallFrame` at
`WorldGen.cs:28425`; the request expansion for that call is already owned by
`LegacyPyramidWallFrameRequestQuery`.

The value path has four independent inputs:

1. the center tile wall value and existing wall-frame number;
2. the four cardinal snapshot neighbors, including the exact four
   `TileID.Sets.TruncatesWalls` tile types and invisible-wall visibility predicate;
3. the immutable `wallLargeFrames`, frame-number, center-offset, and wall-frame lookup tables;
4. the legacy `genRand` stream when a reset frame is requested for an ordinary wall.

The evaluator therefore does not read process-global Terraria state. It accepts an immutable
snapshot, an explicit `showInvisibleWalls` input, and the existing `LegacyPassRandomState`.

## Recommended architecture

- `LegacyTruncatingWallTileRegistry` owns the exact source set `{54, 328, 459, 748}`.
- `LegacyWallFrameLookupRegistry` owns the source lookup tables and frame-size arithmetic;
  callers receive value results, not mutable arrays.
- `WorldTile` gains optional `WallFrameNumber`, `WallFrameX`, and `WallFrameY` value fields so
  reset and non-reset calls have an explicit owner in the snapshot model without conflating wall
  frame coordinates with the tile `FrameX`/`FrameY` used by object and collision rules.
- `LegacyWallFrameEvaluationQuery` evaluates one target without mutating the snapshot. It keeps
  source guard order, wall-zero paint/coating clearing, cardinal mask bits, large-wall and reset
  random branches, full-mask center offsets, and final frame lookup semantics.
- `LegacyPyramidWallFrameCommandProjection` evaluates the ordered Pyramid requests into typed
  `LegacyWallFrameCommand` values. It validates the whole request batch before publication,
  preserves duplicate targets and order, and only commits the working generation state after
  command construction succeeds.
- `LegacyWallFrameCommandCommitSystem` validates command metadata and section-version guards,
  preflights version capacity, then applies all wall-frame results atomically through `WorldGrid`.

`showInvisibleWalls` is explicit because the legacy method delegates to
`Main.ShouldShowInvisibleBlocksAndWalls`, whose client `SceneMetrics` state is not present in the
server simulation. The Pyramid verifier exercises both visibility modes. This batch does not
claim client scene-metric parity.

## Invariants

1. Out-of-world and edge targets are no-ops and consume no random samples.
2. A wall at or above `WallID.Count` is normalized to wall `0`; wall paint and wall coating are
   cleared, and no frame lookup or random draw occurs.
3. A zero wall clears wall paint/coating and leaves frame coordinates and frame number unchanged.
4. Cardinal mask bits are up `1`, left `2`, right `4`, and down `8`; a neighbor qualifies when it
   has a wall or is active with a truncating-wall tile type, subject to invisible visibility.
5. Large-frame wall types use the source coordinate lookup and consume no RNG; ordinary reset
   frames use `Next(0, 3)` and the wall-21 second draw/forced value branch. A non-zero wall write
   updates only `WallFrameNumber`, `WallFrameX`, and `WallFrameY`; tile `FrameX` and `FrameY` are
   preserved.
6. A full cardinal mask adds the source center-wall offset before the four-way frame lookup.
7. Pyramid commands retain request order, duplicate targets, source metadata, and reset intent.
8. Snapshot contents remain unchanged until the explicit commit owner applies commands.
9. Invalid input, sequence exhaustion, section-version exhaustion, and metadata mismatch fail
   closed without partial command publication or world mutation.

## Explicitly deferred

- Pyramid tunnel direction, chest/pile/plant/pot features, and subsequent wall/tile side effects.
- Full `SquareWallFrame` pipeline integration outside Pyramid.
- Client `SceneMetrics`/echo-monolith state, map updates, network publication, render parity, and
  persistence/protocol serialization of the ephemeral wall-frame coordinates.
- Aggregate generation ordering, exact global RNG/checkpoint parity, WLD differential parity,
  legacy `WorldGen.cs` deletion, `canRemoveLegacyWorldGen=false`, and the 44 deferred
  `ServerRelevant` rows.
