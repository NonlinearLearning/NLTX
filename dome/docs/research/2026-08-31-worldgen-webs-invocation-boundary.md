# WorldGen Webs invocation boundary

The legacy Webs pass at
`Build/worldgen-oracle/legacy-instrumented-source/Terraria/WorldGen.cs:14142-14180`
iterates `floor(width * height * 0.0006)` times, selecting X in the 20-column
interior and Y from `worldSurfaceHigh` through `height - 20`. Candidates may be
replaced by indexed `GenVars.mCaveX/mCaveY` history. The recovery scan descends
through empty tiles to the first solid support, chooses a random horizontal
direction, scans to the edge of the open run, and emits TileRunner type 51 with
add-tile, strength `[4,11)`, steps `[2,4)`, and vertical speed `-1`.

`LegacyWebsCandidateSelector`, `LegacyWebsPassDefinition`, and
`LegacyWebsInvocationFactory` encode this invocation-level contract. Focused
evidence reports 11 passing sections, including Webs invocation behavior and
the preceding WorldGen slices. This is not full Webs parity: cave history,
traversal, RNG synchronization, aggregate ordering, and WLD parity remain open.
