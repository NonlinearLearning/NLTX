# Pyramid noTunnel opening side-effect boundary design

**Status:** approved for the resumed N6 P2 bounded batch

**Scope:** the source opening loop that runs before Pyramid feature placement and the final
extended tunnel

## Source authority

The instrumented Version4 source is
`Build/worldgen-oracle/legacy-instrumented-source/Terraria/WorldGen.cs`. The opening loop is
`WorldGen.cs:28429-28466`, immediately after the footprint wall scan. It consumes a direction draw
(`Next(2)`), a vertical opening height (`Next(5, 8)`), and a delay counter (`Next(20, 30)`). For
each source column it scans `num10..num10+num11`, clears active type-151 tiles, writes wall `34`
to the two source-adjacent cells, and converts the column to active type `53` after the first
active type-53 tile is observed above it. The loop stops when a column contains no active type-151
tile. The `noTunnel` flag does not change this opening loop; it is consumed at the later feature
boundary (`WorldGen.cs:28490-28595`) before the final extended tunnel.

## Boundary and authority

`LegacyPyramidTunnelOpening` accepts a committed immutable post-footprint snapshot, the existing
`LegacyPyramidStructureRequest`, the footprint's drawn tunnel width, an explicit legacy random
stream, generation state, and a typed tile-command list. It projects commands against a private
working tile map so repeated source writes observe the same state as the legacy direct mutations.
The owner emits only `UpdateTileType`, `UpdateTileShape`, and `SetWall` commands. It does not claim
the chest, pile, plant, pot, final extended tunnel, publication, client SceneMetrics, or global
parity authorities that are still absent.

`NoTunnel` remains present on the request and the result records that the source feature boundary
was reached. This makes the no-tunnel handoff observable without silently treating deferred feature
intents as implemented.

## Invariants

1. The opening consumes exactly three random samples in source order after the footprint width is
   supplied: direction, opening height, and initial delay.
2. Direction is `-1` when `Next(2)` returns zero and `1` otherwise; the start column is
   `originX - tunnelWidth * direction`, and the start row is `originY + tunnelWidth`.
3. Every active type-151 source tile emits wall-34 writes to `(x, y + 1)` and
   `(x + direction, y)`, then an inactive type-151 update. A column that has observed active type-53
   above it emits the source type-53 update and shape reset for every scanned row.
4. Commands are source-attributed, sequential, and unpublished until the complete opening pass has
   validated. Snapshot input, random/state on rejection, and untouched tile fields remain stable.
5. Coordinates outside the immutable world envelope fail closed before random/state/command
   mutation. The owner stops at the safe edge rather than reading outside a malformed snapshot.
6. The final extended tunnel and all feature placement intents remain explicitly deferred, including
   exact aggregate random/checkpoint and WLD parity.
