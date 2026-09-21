# WorldGen Dungeon entrance-anchor boundary

The legacy `Dungeon` generation pass computes an entrance anchor before calling
`DungeonCrawler.MakeDungeon`. The source initializes the anchor at
`(worldSurface + rockLayer) / 2 + genRand.Next(-200, 200)`, probes ten positions
for solid terrain, scans downward until `SolidTile(y + 10)` or midpoint `+ 200`,
and, when the initial probe finds solid terrain, applies a maximum 60-tile
clearance adjustment. The drunk-world branch overrides the result to
`worldSurface + 70` unless the no-surface seed is enabled.

`LegacyDungeonEntranceAnchorPolicy` owns this bounded calculation. It accepts
the solid-terrain predicate from the caller so tile solidity remains an explicit
world-generation authority. The focused scratch probe verifies the solid-scan
path and drunk-world override.

The Dungeon crawler, room graph, dual-dungeon sequencing, desert cleanup,
post-Dungeon lava pass, aggregate ordering, complete RNG/WLD parity, and legacy
WorldGen deletion remain deferred.
