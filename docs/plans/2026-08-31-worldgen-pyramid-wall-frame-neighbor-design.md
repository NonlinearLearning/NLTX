# Pyramid Wall-Frame Neighbor Classification Design

**Status:** approved for the resumed N6 batch

**Scope:** source-backed `Framing.WallFrame` center validation and cardinal-neighbor
classification only

## Source evidence

The legacy implementation is in
`Build/worldgen-oracle/legacy-instrumented-source/Terraria/Framing.cs`.
`WallFrame` rejects coordinates on the outer world border at lines `354-357`, normalizes a
center wall whose type is outside `WallID.Count` at lines `358-361`, and returns without frame
calculation when the effective center wall is zero at lines `362-367`. For a non-empty center it
builds a four-bit mask at lines `369-402` in this order:

```text
above (x, y - 1) -> bit 1
left  (x - 1, y) -> bit 2
right (x + 1, y) -> bit 4
down  (x, y + 1) -> bit 8
```

A neighbor contributes its bit when its wall type is positive, or when it is active and its tile
type is in `TileID.Sets.TruncatesWalls`. The current oracle defines that set as tile types
`54`, `328`, `459`, and `748` at `Terraria.ID/TileID.cs:363`. The invisible-wall predicate is
applied after this candidate check; an invisible neighbor is ignored unless the caller supplies
the source-equivalent `showInvisibleWalls` value. `WallID.Count` is `367` at
`Terraria.ID/WallID.cs:803`.

## Boundary

Add a pure Simulation query that accepts an immutable `WorldGridSnapshot`, an interior coordinate,
and an explicit `showInvisibleWalls` value. It returns a typed result containing the original and
effective center wall types, the four-bit cardinal mask, and whether the center wall was
normalized. Invalid or border coordinates fail closed before reading tile contents. A zero or
invalid center wall produces a successful no-frame result with an empty mask.

The truncating tile set is exposed by a frozen `LegacyTruncatingWallTileRegistry`. The query does
not depend on client `SceneMetrics`, global `Main`, pending mutations, `LegacyPassRandomState`,
`WorldGenerationStateComponent`, lookup tables, or frame fields. It does not produce
`TileFrameCommand` values or mutate the snapshot.

## Invariants

1. Only strict interior coordinates are accepted; failure returns a default result and a reason.
2. Invalid center wall types are treated as wall type zero for this value calculation, with an
   explicit normalization marker.
3. A valid non-zero center scans exactly the four cardinal neighbors using source bit assignments.
4. Positive neighbor walls qualify regardless of neighbor tile activity or wall validity.
5. Active truncating tiles qualify even with wall type zero.
6. Invisible qualifying neighbors are omitted when `showInvisibleWalls` is false and retained when
   it is true.
7. Center wall zero/normalization never scans neighbors and never emits a frame value.
8. The query consumes no random samples, does not advance generation state, and leaves the
   snapshot and its section versions unchanged.

## Explicitly deferred

`wallFrameLookup`, `centerWallFrameLookup`, large-wall frame-number tables, `wallFrameNumber`,
wall-21 random behavior, `FrameX`/`FrameY` mutation, wall-color/paint cleanup, command publication,
Pyramid tunnel/features, aggregate ordering, exact RNG/checkpoint parity, full WLD differential,
legacy `WorldGen.cs` deletion, `canRemoveLegacyWorldGen`, and all 44 deferred `ServerRelevant`
rows remain outside this slice.
