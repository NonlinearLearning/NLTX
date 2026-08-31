# WorldGen Desert placement selector boundary

The legacy `DesertBiome` pass starts at
`Build/worldgen-oracle/legacy-instrumented-source/Terraria/WorldGen.cs:13020`.
It selects a side-relative initial candidate around the world center, retries
with a wider offset, and consumes an additional `genRand.Next(num5 / 12)` draw
on every retry. `num5` increments for each failed placement; once it exceeds
`Main.maxTilesX / 4`, the dungeon side flips, the counter resets, and the
second flip enables `GenVars.skipDesertTileCheck`.

`LegacyDesertPlacementSelector` models those values as immutable candidate/state
records. The corrected retry implementation now preserves the additional random
draw, including the zero-width behavior for the first eleven failures. A
focused probe verifies the initial side-relative coordinate, candidate Y,
first side flip, second-flip skip-check transition, and the two-sample retry
draw when `failureCount / 12` is non-zero.

This boundary does not claim `DesertBiome.Place` parity. Structure templates,
collision checks, retry success, Remix conversion, progress state, aggregate
ordering, and WLD parity remain deferred; `canRemoveLegacyWorldGen` therefore
remains false.
