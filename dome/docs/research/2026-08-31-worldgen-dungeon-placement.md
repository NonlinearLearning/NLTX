# WorldGen Dungeon placement boundary

The legacy setup selects an initial dungeon side with `genRand.Next(2)`, applies
the source's two drunk-world side flips, and samples the dungeon location from
the side-specific beach interval. Left uses
`[leftBeachEnd + dungeonBeachPadding, maxTilesX * 0.2)`; right uses
`[maxTilesX * 0.8, rightBeachStart - dungeonBeachPadding)`. The oracle's
`dungeonBeachPadding` is 50.

`LegacyDungeonPlacementPolicy` owns this side/location calculation over the
already typed `LegacyBeachBounds`. `SelectPair` preserves the source dual-dungeon
branch by sampling the second location on the opposite side from the same random
stream. It intentionally does not own jungle origin, snow origin, desert cleanup,
or crawler generation. Those authorities and aggregate ordering remain deferred.
