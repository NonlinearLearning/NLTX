# WorldGen Dungeon Desert cleanup boundary

When the surface-is-desert seed is active, the legacy Dungeon pass invokes
`DungeonDesertCleanup` before `DungeonCrawler.MakeDungeon`. The cleanup starts
at `worldSurface - 10 + buriedEntranceSandDugoutYOffset - Next(25, 46)`, clears
the current tile and the wall on both the current and next row, then widens the
left/right span each row. Width increments are `Next(4, 5)` below 20,
`Next(2, 4)` from 20 through 39, and `Next(1, 3)` at 40 or above. The loop
stops when the row reaches the caller-provided `FindLowestCloud()` boundary and
uses the five-tile `InWorld` inset.

`LegacyDungeonDesertCleanupPass` owns this bounded command projection. It keeps
cloud discovery and tile authority with the caller. Crawler generation, dual
dungeon sequencing, aggregate ordering, complete RNG/WLD parity, and legacy
WorldGen deletion remain deferred.

The post-Dungeon oracle invokes `LavaLayerCaverer()` only when
`dontStarveWorldGen && !tenthAnniversaryWorldGen && !remixWorldGen`.
`LegacyDungeonPostPassPolicy.ShouldRunLavaLayerCaverer` captures this gate;
the lava-caverer traversal and its mutation/RNG parity are intentionally not
implemented by this boundary.
